using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HoI2Editor.Parsers;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Commander data group
    /// </summary>
    public static class Leaders
    {
        #region Public properties

        /// <summary>
        ///     Master commander list
        /// </summary>
        public static List<Leader> Items { get; }

        /// <summary>
        ///     Correspondence between country tag and commander file name
        /// </summary>
        public static Dictionary<Country, string> FileNameMap { get; }

        /// <summary>
        ///     Already used ID list
        /// </summary>
        public static HashSet<int> IdSet { get; }

        /// <summary>
        ///     Class name
        /// </summary>
        public static string[] RankNames { get; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Loaded flag
        /// </summary>
        private static bool _loaded;

        /// <summary>
        ///     For lazy loading
        /// </summary>
        private static readonly BackgroundWorker Worker = new BackgroundWorker();

        /// <summary>
        ///     Edited flag
        /// </summary>
        private static bool _dirtyFlag;

        /// <summary>
        ///     Edited flags by nation
        /// </summary>
        private static readonly bool[] DirtyFlags = new bool[Enum.GetValues(typeof (Country)).Length];

        /// <summary>
        ///     Edited flag in commander list file
        /// </summary>
        private static bool _dirtyListFlag;

        #endregion

        #region Public constant

        /// <summary>
        ///     Commander characteristic value
        /// </summary>
        public static readonly uint[] TraitsValues =
        {
            LeaderTraits.LogisticsWizard,
            LeaderTraits.DefensiveDoctrine,
            LeaderTraits.OffensiveDoctrine,
            LeaderTraits.WinterSpecialist,
            LeaderTraits.Trickster,
            LeaderTraits.Engineer,
            LeaderTraits.FortressBuster,
            LeaderTraits.PanzerLeader,
            LeaderTraits.Commando,
            LeaderTraits.OldGuard,
            LeaderTraits.SeaWolf,
            LeaderTraits.BlockadeRunner,
            LeaderTraits.SuperiorTactician,
            LeaderTraits.Spotter,
            LeaderTraits.TankBuster,
            LeaderTraits.CarpetBomber,
            LeaderTraits.NightFlyer,
            LeaderTraits.FleetDestroyer,
            LeaderTraits.DesertFox,
            LeaderTraits.JungleRat,
            LeaderTraits.UrbanWarfareSpecialist,
            LeaderTraits.Ranger,
            LeaderTraits.Mountaineer,
            LeaderTraits.HillsFighter,
            LeaderTraits.CounterAttacker,
            LeaderTraits.Assaulter,
            LeaderTraits.Encircler,
            LeaderTraits.Ambusher,
            LeaderTraits.Disciplined,
            LeaderTraits.ElasticDefenceSpecialist,
            LeaderTraits.Blitzer
        };

        /// <summary>
        ///     Commander characteristic name
        /// </summary>
        public static readonly string[] TraitsNames =
        {
            "TRAIT_LOGWIZ",
            "TRAIT_DEFDOC",
            "TRAIT_OFFDOC",
            "TRAIT_WINSPE",
            "TRAIT_TRICKS",
            "TRAIT_ENGINE",
            "TRAIT_FORBUS",
            "TRAIT_PNZLED",
            "TRAIT_COMMAN",
            "TRAIT_OLDGRD",
            "TRAIT_SEAWOL",
            "TRAIT_BLKRUN",
            "TRAIT_SUPTAC",
            "TRAIT_SPOTTE",
            "TRAIT_TNKBUS",
            "TRAIT_CRPBOM",
            "TRAIT_NGHTFL",
            "TRAIT_FLTDES",
            "TRAIT_DSRFOX",
            "TRAIT_JUNGLE",
            "TRAIT_URBAN",
            "TRAIT_FOREST",
            "TRAIT_MOUNTAIN",
            "TRAIT_HILLS",
            "TRAIT_COUNTER",
            "TRAIT_ASSAULT",
            "TRAIT_ENCIRCL",
            "TRAIT_AMBUSH",
            "TRAIT_DELAY",
            "TRAIT_TATICAL",
            "TRAIT_BREAK"
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Leaders()
        {
            // Master Commander List
            Items = new List<Leader>();

            // Correspondence between country tag and commander file name
            FileNameMap = new Dictionary<Country, string>();

            // Already used ID list
            IdSet = new HashSet<int>();

            // class
            RankNames = new[] { "", Resources.Rank3, Resources.Rank2, Resources.Rank1, Resources.Rank0 };
        }

        #endregion

        #region File reading

        /// <summary>
        ///     Request a reload of the commander file
        /// </summary>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     Reload commander files
        /// </summary>
        public static void Reload()
        {
            // Do nothing before loading
            if (!_loaded)
            {
                return;
            }

            _loaded = false;

            Load();
        }

        /// <summary>
        ///     Read commander files
        /// </summary>
        public static void Load()
        {
            // Do nothing if already loaded
            if (_loaded)
            {
                return;
            }

            // Wait for completion if loading is in progress
            if (Worker.IsBusy)
            {
                WaitLoading();
                return;
            }

            LoadFiles();
        }

        /// <summary>
        ///     Delayed loading of commander files
        /// </summary>
        /// <param name="handler">Read complete event handler</param>
        public static void LoadAsync(RunWorkerCompletedEventHandler handler)
        {
            // Call the completion event handler if it has already been read
            if (_loaded)
            {
                handler?.Invoke(null, new RunWorkerCompletedEventArgs(null, null, false));
                return;
            }

            // Register the read completion event handler
            if (handler != null)
            {
                Worker.RunWorkerCompleted += handler;
                Worker.RunWorkerCompleted += OnWorkerRunWorkerCompleted;
            }

            // Return if loading is in progress
            if (Worker.IsBusy)
            {
                return;
            }

            // If it has already been read here, the completion event handler has already been called, so return without doing anything.
            if (_loaded)
            {
                return;
            }

            // Start lazy loading
            Worker.DoWork += OnWorkerDoWork;
            Worker.RunWorkerAsync();
        }

        /// <summary>
        ///     Wait until loading is complete
        /// </summary>
        public static void WaitLoading()
        {
            while (Worker.IsBusy)
            {
                Application.DoEvents();
            }
        }

        /// <summary>
        ///     Determine if lazy loading is in progress
        /// </summary>
        /// <returns>If delayed reading is in progress true true return it</returns>
        public static bool IsLoading()
        {
            return Worker.IsBusy;
        }

        /// <summary>
        ///     Delayed read processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            LoadFiles();
        }

        /// <summary>
        ///     Processing when lazy loading is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Processing when lazy loading is completed
            HoI2EditorController.OnLoadingCompleted();
        }

        /// <summary>
        ///     Read commander files
        /// </summary>
        private static void LoadFiles()
        {
            Items.Clear();
            IdSet.Clear();
            FileNameMap.Clear();

            switch (Game.Type)
            {
                case GameType.HeartsOfIron2:
                case GameType.ArsenalOfDemocracy:
                    if (!LoadHoI2())
                    {
                        return;
                    }
                    break;

                case GameType.DarkestHour:
                    if (!LoadDh())
                    {
                        return;
                    }
                    break;
            }

            // Clear the edited flag
            _dirtyFlag = false;

            // Set the read flag
            _loaded = true;
        }

        /// <summary>
        ///     Read commander files (HoI2 / AoD / DH-MOD When not in use )
        /// </summary>
        /// <returns>If reading fails false false return it</returns>
        private static bool LoadHoI2()
        {
            List<string> filelist = new List<string>();
            string folderName;
            bool error = false;

            // Load the commander file in the save folder
            if (Game.IsExportFolderActive)
            {
                folderName = Game.GetExportFileName(Game.LeaderPathName);
                if (Directory.Exists(folderName))
                {
                    foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                    {
                        try
                        {
                            // Read commander file
                            LoadFile(fileName);

                            // Register the read file name in the commander file list
                            string name = Path.GetFileName(fileName);
                            if (!string.IsNullOrEmpty(name))
                            {
                                filelist.Add(name.ToLower());
                            }
                        }
                        catch (Exception)
                        {
                            error = true;
                            Log.Error("[Leader] Read error: {0}", fileName);
                            if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                Resources.EditorLeader, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                == DialogResult.Cancel)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            // MOD Read the commander file in the folder
            if (Game.IsModActive)
            {
                folderName = Game.GetModFileName(Game.LeaderPathName);
                if (Directory.Exists(folderName))
                {
                    foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                    {
                        try
                        {
                            // Read commander file
                            LoadFile(fileName);

                            // Register the read file name in the commander file list
                            string name = Path.GetFileName(fileName);
                            if (!string.IsNullOrEmpty(name))
                            {
                                filelist.Add(name.ToLower());
                            }
                        }
                        catch (Exception)
                        {
                            error = true;
                            Log.Error("[Leader] Read error: {0}", fileName);
                            if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                Resources.EditorLeader, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                == DialogResult.Cancel)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            // Read the commander file in the vanilla folder
            folderName = Path.Combine(Game.FolderName, Game.LeaderPathName);
            if (Directory.Exists(folderName))
            {
                foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                {
                    // MOD Ignore files read in folders
                    string name = Path.GetFileName(fileName);
                    if (string.IsNullOrEmpty(name) || filelist.Contains(name.ToLower()))
                    {
                        continue;
                    }

                    try
                    {
                        // Read commander file
                        LoadFile(fileName);
                    }
                    catch (Exception)
                    {
                        error = true;
                        Log.Error("[Leader] Read error: {0}", fileName);
                        if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                            Resources.EditorLeader, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                            == DialogResult.Cancel)
                        {
                            return false;
                        }
                    }
                }
            }

            return !error;
        }

        /// <summary>
        ///     Read commander files (DH-MOD while using it )
        /// </summary>
        /// <returns>If reading fails false false return it</returns>
        private static bool LoadDh()
        {
            // If the commander list file does not exist, use the conventional loading method.
            string listFileName = Game.GetReadFileName(Game.DhLeaderListPathName);
            if (!File.Exists(listFileName))
            {
                return LoadHoI2();
            }

            // Read the commander list file
            IEnumerable<string> fileList;
            try
            {
                fileList = LoadList(listFileName);
            }
            catch (Exception)
            {
                Log.Error("[Leader] Read error: {0}", listFileName);
                MessageBox.Show($"{Resources.FileReadError}: {listFileName}",
                    Resources.EditorLeader, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            bool error = false;
            foreach (string fileName in fileList.Select(name => Game.GetReadFileName(Game.LeaderPathName, name)))
            {
                try
                {
                    // Read commander file
                    LoadFile(fileName);
                }
                catch (Exception)
                {
                    error = true;
                    Log.Error("[Leader] Read error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                        Resources.EditorLeader, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return false;
                    }
                }
            }

            return !error;
        }

        /// <summary>
        ///     Read the commander list file (DH)
        /// </summary>
        private static IEnumerable<string> LoadList(string fileName)
        {
            Log.Verbose("[Leader] Load: {0}", Path.GetFileName(fileName));

            List<string> list = new List<string>();
            using (StreamReader reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    // Blank line
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    // Comment line
                    if (line[0] == '#')
                    {
                        continue;
                    }

                    list.Add(line);
                }
            }
            return list;
        }

        /// <summary>
        ///     Read commander file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void LoadFile(string fileName)
        {
            Log.Verbose("[Leader] Load: {0}", Path.GetFileName(fileName));

            using (CsvLexer lexer = new CsvLexer(fileName))
            {
                // Skip empty files
                if (lexer.EndOfStream)
                {
                    return;
                }

                // Read header line
                lexer.SkipLine();

                // Skip files with only header lines
                if (lexer.EndOfStream)
                {
                    return;
                }

                // 1 Read line by line
                Country country = Country.None;
                while (!lexer.EndOfStream)
                {
                    Leader leader = ParseLine(lexer);

                    // Skip blank lines
                    if (leader == null)
                    {
                        continue;
                    }

                    Items.Add(leader);

                    if (country == Country.None)
                    {
                        country = leader.Country;
                        if (country != Country.None && !FileNameMap.ContainsKey(country))
                        {
                            FileNameMap.Add(country, lexer.FileName);
                        }
                    }
                }

                ResetDirty(country);
            }
        }

        /// <summary>
        ///     Interpret the commander-defined line
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Commander data</returns>
        private static Leader ParseLine(CsvLexer lexer)
        {
            string[] tokens = lexer.GetTokens();

            // Skip blank lines
            if (tokens == null)
            {
                return null;
            }

            // Skip lines with insufficient tokens
            if (tokens.Length != (Misc.EnableRetirementYearLeaders ? 19 : 18))
            {
                Log.Warning("[Leader] Invalid token count: {0} ({1} L{2})", tokens.Length, lexer.FileName, lexer.LineNo);
                // At the end x x There is no / / Continue analysis if there are extra items
                if (tokens.Length < (Misc.EnableRetirementYearLeaders ? 18 : 17))
                {
                    return null;
                }
            }

            // Skip lines without a name
            if (string.IsNullOrEmpty(tokens[0]))
            {
                return null;
            }

            Leader leader = new Leader();
            int index = 0;

            // name
            leader.Name = tokens[index];
            index++;

            // ID
            int id;
            if (!int.TryParse(tokens[index], out id))
            {
                Log.Warning("[Leader] Invalid id: {0} [{1}] ({2} L{3})", tokens[index], leader.Name, lexer.FileName,
                    lexer.LineNo);
                return null;
            }
            leader.Id = id;
            index++;

            // Nation
            if (string.IsNullOrEmpty(tokens[index]) || !Countries.StringMap.ContainsKey(tokens[index].ToUpper()))
            {
                Log.Warning("[Leader] Invalid country: {0} [{1}: {2}] ({3} L{4})", tokens[index], leader.Id, leader.Name,
                    lexer.FileName, lexer.LineNo);
                return null;
            }
            leader.Country = Countries.StringMap[tokens[index].ToUpper()];
            index++;

            // Year of appointment
            for (int i = 0; i < 4; i++)
            {
                int rankYear;
                if (int.TryParse(tokens[index], out rankYear))
                {
                    leader.RankYear[i] = rankYear;
                }
                else
                {
                    leader.RankYear[i] = 1990;
                    Log.Warning("[Leader] Invalid rank{0} year: {1} [{2}: {3}] ({4} L{5})", i, tokens[index], leader.Id,
                        leader.Name, lexer.FileName, lexer.LineNo);
                }
                index++;
            }

            // Ideal class
            int idealRank;
            if (int.TryParse(tokens[index], out idealRank) && 0 <= idealRank && idealRank <= 3)
            {
                leader.IdealRank = (LeaderRank) (4 - idealRank);
            }
            else
            {
                leader.IdealRank = LeaderRank.None;
                Log.Warning("[Leader] Invalid ideal rank: {0} [{1}: {2}] ({3} L{4})", tokens[index], leader.Id,
                    leader.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Maximum skill
            int maxSkill;
            if (int.TryParse(tokens[index], out maxSkill))
            {
                leader.MaxSkill = maxSkill;
            }
            else
            {
                leader.MaxSkill = 0;
                Log.Warning("[Leader] Invalid max skill: {0} [{1}: {2}] ({3} L{4})", tokens[index], leader.Id,
                    leader.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Commander characteristics
            uint traits;
            if (uint.TryParse(tokens[index], out traits))
            {
                leader.Traits = traits;
            }
            else
            {
                leader.Traits = LeaderTraits.None;
                Log.Warning("[Leader] Invalid trait: {0} [{1}: {2}] ({3} L{4})", tokens[index], leader.Id,
                    leader.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // skill
            int skill;
            if (int.TryParse(tokens[index], out skill))
            {
                leader.Skill = skill;
            }
            else
            {
                leader.Skill = 0;
                Log.Warning("[Leader] Invalid skill: {0} [{1}: {2}] ({3} L{4})", tokens[index], leader.Id, leader.Name,
                    lexer.FileName, lexer.LineNo);
            }
            index++;

            // Experience point
            int experience;
            if (int.TryParse(tokens[index], out experience))
            {
                leader.Experience = experience;
            }
            else
            {
                leader.Experience = 0;
                Log.Warning("[Leader] Invalid experience: {0} [{1}: {2}] ({3} L{4})", tokens[index], leader.Id,
                    leader.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Loyalty
            int loyalty;
            if (int.TryParse(tokens[index], out loyalty))
            {
                leader.Loyalty = loyalty;
            }
            else
            {
                leader.Loyalty = 0;
                Log.Warning("[Leader] Invalid loyalty: {0} [{1}: {2}] ({3} L{4})", tokens[index], leader.Id, leader.Name,
                    lexer.FileName, lexer.LineNo);
            }
            index++;

            // Army
            int branch;
            if (int.TryParse(tokens[index], out branch))
            {
                leader.Branch = (Branch) (branch + 1);
            }
            else
            {
                leader.Branch = Branch.None;
                Log.Warning("[Leader] Invalid branch: {0} [{1}: {2}] ({3} L{4})", tokens[index], leader.Id, leader.Name,
                    lexer.FileName, lexer.LineNo);
            }
            index++;

            // Image file name
            leader.PictureName = tokens[index];
            index++;

            if (leader.PictureName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 || System.Text.RegularExpressions.Regex.IsMatch(leader.PictureName, @"\p{IsCyrillic}"))
            {
                Log.Warning("[Leader] Picture name contains invalid char {0}:  L{1}", lexer.PathName, lexer.LineNo);
            }
            else
            {
                string imgFileName = Path.Combine(Game.PersonPicturePathName, leader.PictureName);
                imgFileName += ".bmp";
                string pathName = Game.GetReadFileName(imgFileName);
                if (!File.Exists(pathName))
                {
                    Log.Warning("[Leader] Picture not exist {0}:  L{1}", lexer.PathName, lexer.LineNo);
                }
            }

            // Start year
            int startYear;
            if (int.TryParse(tokens[index], out startYear))
            {
                leader.StartYear = startYear;
            }
            else
            {
                leader.StartYear = 1930;
                Log.Warning("[Leader] Invalid start year: {0} [{1}: {2}] ({3} L{4})", tokens[index], leader.Id,
                    leader.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // End year
            int endYear;
            if (int.TryParse(tokens[index], out endYear))
            {
                leader.EndYear = endYear;
            }
            else
            {
                leader.EndYear = 1970;
                Log.Warning("[Leader] Invalid end year: {0} [{1}: {2}] ({3} L{4})", tokens[index], leader.Id,
                    leader.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Retirement year
            if (Misc.EnableRetirementYearLeaders)
            {
                int retirementYear;
                if (int.TryParse(tokens[index], out retirementYear))
                {
                    leader.RetirementYear = retirementYear;
                }
                else
                {
                    leader.RetirementYear = 1999;
                    Log.Warning("[Leader] Invalid retirement year: {0} [{1}: {2}] ({3} L{4})", tokens[index], leader.Id,
                        leader.Name, lexer.FileName, lexer.LineNo);
                }
            }
            else
            {
                leader.RetirementYear = 1999;
            }

            return leader;
        }

        #endregion

        #region File writing

        /// <summary>
        ///     Save commander files
        /// </summary>
        /// <returns>If saving fails false false return it</returns>
        public static bool Save()
        {
            // Do nothing if not edited
            if (!IsDirty())
            {
                return true;
            }

            // Wait for completion if loading is in progress
            if (Worker.IsBusy)
            {
                WaitLoading();
            }

            // Save the commander list file
            if ((Game.Type == GameType.DarkestHour) && IsDirtyList())
            {
                try
                {
                    SaveList();
                }
                catch (Exception)
                {
                    string fileName = Game.GetWriteFileName(Game.DhLeaderListPathName);
                    Log.Error("[Leader] Write error: {0}", fileName);
                    MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                        Resources.EditorLeader, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            bool error = false;
            foreach (Country country in Countries.Tags
                .Where(country => DirtyFlags[(int) country] && country != Country.None))
            {
                try
                {
                    // Save the commander file
                    SaveFile(country);
                }
                catch (Exception)
                {
                    error = true;
                    string fileName = Game.GetWriteFileName(Game.LeaderPathName, Game.GetLeaderFileName(country));
                    Log.Error("[Leader] Write error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                        Resources.EditorLeader, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return false;
                    }
                }
            }

            // Return if saving fails
            if (error)
            {
                return false;
            }

            // Clear the edited flag
            _dirtyFlag = false;

            return true;
        }

        /// <summary>
        ///     Save the commander list file (DH)
        /// </summary>
        private static void SaveList()
        {
            // Create a database folder if it does not exist
            string folderName = Game.GetWriteFileName(Game.DatabasePathName);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string fileName = Game.GetWriteFileName(Game.DhLeaderListPathName);
            Log.Info("[Leader] Save: {0}", Path.GetFileName(fileName));

            // Write the registered commander file names in order
            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                foreach (string name in FileNameMap.Select(pair => pair.Value))
                {
                    writer.WriteLine(name);
                }
            }

            // Clear the edited flag
            ResetDirtyList();
        }

        /// <summary>
        ///     Save the commander file
        /// </summary>
        /// <param name="country">Country tag</param>
        private static void SaveFile(Country country)
        {
            // Create a commander folder if it does not exist
            string folderName = Game.GetWriteFileName(Game.LeaderPathName);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string name = Game.GetLeaderFileName(country);
            string fileName = Path.Combine(folderName, name);
            Log.Info("[Leader] Save: {0}", name);

            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                int lineNo = 2;

                // Write header line
                writer.WriteLine(
                    Misc.EnableRetirementYearLeaders
                        ? "Name;ID;Country;Rank 3 Year;Rank 2 Year;Rank 1 Year;Rank 0 Year;Ideal Rank;Max Skill;Traits;Skill;Experience;Loyalty;Type;Picture;Start Year;End Year;Retirement Year;x"
                        : "Name;ID;Country;Rank 3 Year;Rank 2 Year;Rank 1 Year;Rank 0 Year;Ideal Rank;Max Skill;Traits;Skill;Experience;Loyalty;Type;Picture;Start Year;End Year;x");

                // Write commander definition lines in order
                foreach (Leader leader in Items.Where(leader => leader.Country == country))
                {
                    // If an invalid value is set, a warning will be output to the log.
                    if (leader.Branch == Branch.None)
                    {
                        Log.Warning("[Leader] Invalid branch: {0} {1} ({2} L{3})", leader.Id, leader.Name, name, lineNo);
                    }
                    if (leader.IdealRank == LeaderRank.None)
                    {
                        Log.Warning("[Leader] Invalid ideal rank: {0} {1} ({2} L{3})", leader.Id, leader.Name, name,
                            lineNo);
                    }

                    // Write commander definition line
                    if (Misc.EnableRetirementYearLeaders)
                    {
                        writer.WriteLine(
                            "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};x",
                            leader.Name,
                            leader.Id,
                            Countries.Strings[(int) leader.Country],
                            leader.RankYear[0],
                            leader.RankYear[1],
                            leader.RankYear[2],
                            leader.RankYear[3],
                            leader.IdealRank != LeaderRank.None ? IntHelper.ToString(4 - (int) leader.IdealRank) : "",
                            leader.MaxSkill,
                            leader.Traits,
                            leader.Skill,
                            leader.Experience,
                            leader.Loyalty,
                            leader.Branch != Branch.None ? IntHelper.ToString((int) (leader.Branch - 1)) : "",
                            leader.PictureName,
                            leader.StartYear,
                            leader.EndYear,
                            leader.RetirementYear);
                    }
                    else
                    {
                        writer.WriteLine(
                            "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};x",
                            leader.Name,
                            leader.Id,
                            Countries.Strings[(int) leader.Country],
                            leader.RankYear[0],
                            leader.RankYear[1],
                            leader.RankYear[2],
                            leader.RankYear[3],
                            leader.IdealRank != LeaderRank.None ? IntHelper.ToString(4 - (int) leader.IdealRank) : "",
                            leader.MaxSkill,
                            leader.Traits,
                            leader.Skill,
                            leader.Experience,
                            leader.Loyalty,
                            leader.Branch != Branch.None ? IntHelper.ToString((int) (leader.Branch - 1)) : "",
                            leader.PictureName,
                            leader.StartYear,
                            leader.EndYear);
                    }

                    // Clear the edited flag
                    leader.ResetDirtyAll();

                    lineNo++;
                }
            }

            ResetDirty(country);
        }

        #endregion

        #region Commander list operation

        /// <summary>
        ///     Add an item to the commander list
        /// </summary>
        /// <param name="leader">Items to be inserted</param>
        public static void AddItem(Leader leader)
        {
            Log.Info("[Leader] Add leader: ({0}: {1}) <{2}>", leader.Id, leader.Name,
                Countries.Strings[(int) leader.Country]);

            Items.Add(leader);
        }

        /// <summary>
        ///     Insert an item in the commander list
        /// </summary>
        /// <param name="leader">Items to be inserted</param>
        /// <param name="position">Insert destination item</param>
        public static void InsertItem(Leader leader, Leader position)
        {
            int index = Items.IndexOf(position) + 1;

            Log.Info("[Leader] Insert leader: {0} ({1}: {2}) <{3}>", index, leader.Id, leader.Name,
                Countries.Strings[(int) leader.Country]);

            Items.Insert(index, leader);
        }

        /// <summary>
        ///     Remove an item from the commander list
        /// </summary>
        /// <param name="leader">Items to be deleted</param>
        public static void RemoveItem(Leader leader)
        {
            Log.Info("[Leader] Remove leader: ({0}: {1}) <{2}>", leader.Id, leader.Name,
                Countries.Strings[(int) leader.Country]);

            Items.Remove(leader);

            // Already used ID Remove from list
            IdSet.Remove(leader.Id);
        }

        /// <summary>
        ///     Move items in the commander list
        /// </summary>
        /// <param name="src">Item of move source</param>
        /// <param name="dest">Item to move to</param>
        public static void MoveItem(Leader src, Leader dest)
        {
            int srcIndex = Items.IndexOf(src);
            int destIndex = Items.IndexOf(dest);

            Log.Info("[Leader] Move leader: {0} -> {1} ({2}: {3}) <{4}>", srcIndex, destIndex, src.Id, src.Name,
                Countries.Strings[(int) src.Country]);

            if (srcIndex > destIndex)
            {
                // When moving up
                Items.Insert(destIndex, src);
                Items.RemoveAt(srcIndex + 1);
            }
            else
            {
                // When moving down
                Items.Insert(destIndex + 1, src);
                Items.RemoveAt(srcIndex);
            }
        }

        #endregion

        #region Bulk editing

        /// <summary>
        ///     Bulk editing
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        public static void BatchEdit(LeaderBatchEditArgs args)
        {
            LogBatchEdit(args);

            IEnumerable<Leader> leaders = GetBatchEditLeaders(args);
            Country newCountry;
            switch (args.ActionMode)
            {
                case BatchActionMode.Modify:
                    // Bulk edit commanders
                    foreach (Leader leader in leaders)
                    {
                        BatchEditLeader(leader, args);
                    }
                    break;

                case BatchActionMode.Copy:
                    // Copy the commander
                    newCountry = args.Destination;
                    int id = args.Id;
                    foreach (Leader leader in leaders)
                    {
                        id = GetNewId(id);
                        Leader newLeader = new Leader(leader)
                        {
                            Country = newCountry,
                            Id = id
                        };
                        newLeader.SetDirtyAll();
                        Items.Add(newLeader);
                    }

                    // Set the edited flag for the destination country
                    SetDirty(newCountry);

                    // If the copy destination country does not exist in the file list, add it
                    if (!FileNameMap.ContainsKey(newCountry))
                    {
                        FileNameMap.Add(newCountry, Game.GetLeaderFileName(newCountry));
                        SetDirtyList();
                    }
                    break;

                case BatchActionMode.Move:
                    // Move commander
                    newCountry = args.Destination;
                    foreach (Leader leader in leaders)
                    {
                        // Set the edited flag for the country before the move
                        SetDirty(leader.Country);

                        leader.Country = newCountry;
                        leader.SetDirty(LeaderItemId.Country);
                    }

                    // Set the edited flag for the destination country
                    SetDirty(newCountry);

                    // If the destination country does not exist in the file list, add it.
                    if (!FileNameMap.ContainsKey(newCountry))
                    {
                        FileNameMap.Add(newCountry, Game.GetLeaderFileName(newCountry));
                        SetDirtyList();
                    }
                    break;
            }
        }

        /// <summary>
        ///     Individual processing of batch editing
        /// </summary>
        /// <param name="leader">Target commander</param>
        /// <param name="args">Batch editing parameters</param>
        private static void BatchEditLeader(Leader leader, LeaderBatchEditArgs args)
        {
            // Ideal class
            if (args.Items[(int) LeaderBatchItemId.IdealRank])
            {
                if (leader.IdealRank != args.IdealRank)
                {
                    leader.IdealRank = args.IdealRank;
                    leader.SetDirty(LeaderItemId.IdealRank);
                    SetDirty(leader.Country);
                }
            }

            // skill
            if (args.Items[(int) LeaderBatchItemId.Skill])
            {
                if (leader.Skill != args.Skill)
                {
                    leader.Skill = args.Skill;
                    leader.SetDirty(LeaderItemId.Skill);
                    SetDirty(leader.Country);
                }
            }

            // Maximum skill
            if (args.Items[(int) LeaderBatchItemId.MaxSkill])
            {
                if (leader.MaxSkill != args.MaxSkill)
                {
                    leader.MaxSkill = args.MaxSkill;
                    leader.SetDirty(LeaderItemId.MaxSkill);
                    SetDirty(leader.Country);
                }
            }

            // Experience point
            if (args.Items[(int) LeaderBatchItemId.Experience])
            {
                if (leader.Experience != args.Experience)
                {
                    leader.Experience = args.Experience;
                    leader.SetDirty(LeaderItemId.Experience);
                    SetDirty(leader.Country);
                }
            }

            // Loyalty
            if (args.Items[(int) LeaderBatchItemId.Loyalty])
            {
                if (leader.Loyalty != args.Loyalty)
                {
                    leader.Loyalty = args.Loyalty;
                    leader.SetDirty(LeaderItemId.Loyalty);
                    SetDirty(leader.Country);
                }
            }

            // Start year
            if (args.Items[(int) LeaderBatchItemId.StartYear])
            {
                if (leader.StartYear != args.StartYear)
                {
                    leader.StartYear = args.StartYear;
                    leader.SetDirty(LeaderItemId.StartYear);
                    SetDirty(leader.Country);
                }
            }

            // End year
            if (args.Items[(int) LeaderBatchItemId.EndYear])
            {
                if (leader.EndYear != args.EndYear)
                {
                    leader.EndYear = args.EndYear;
                    leader.SetDirty(LeaderItemId.EndYear);
                    SetDirty(leader.Country);
                }
            }

            // Retirement year
            if (args.Items[(int) LeaderBatchItemId.RetirementYear])
            {
                if (leader.RetirementYear != args.RetirementYear)
                {
                    leader.RetirementYear = args.RetirementYear;
                    leader.SetDirty(LeaderItemId.RetirementYear);
                    SetDirty(leader.Country);
                }
            }

            // Major General Year
            if (args.Items[(int) LeaderBatchItemId.Rank3Year])
            {
                if (leader.RankYear[0] != args.RankYear[0])
                {
                    leader.RankYear[0] = args.RankYear[0];
                    leader.SetDirty(LeaderItemId.Rank3Year);
                    SetDirty(leader.Country);
                }
            }

            // Year of middle general
            if (args.Items[(int) LeaderBatchItemId.Rank2Year])
            {
                if (leader.RankYear[1] != args.RankYear[1])
                {
                    leader.RankYear[1] = args.RankYear[1];
                    leader.SetDirty(LeaderItemId.Rank2Year);
                    SetDirty(leader.Country);
                }
            }

            // General Year
            if (args.Items[(int) LeaderBatchItemId.Rank1Year])
            {
                if (leader.RankYear[2] != args.RankYear[2])
                {
                    leader.RankYear[2] = args.RankYear[2];
                    leader.SetDirty(LeaderItemId.Rank1Year);
                    SetDirty(leader.Country);
                }
            }

            // Marshal Year
            if (args.Items[(int) LeaderBatchItemId.Rank0Year])
            {
                if (leader.RankYear[3] != args.RankYear[3])
                {
                    leader.RankYear[3] = args.RankYear[3];
                    leader.SetDirty(LeaderItemId.Rank0Year);
                    SetDirty(leader.Country);
                }
            }
        }

        /// <summary>
        ///     Get a list of commanders for bulk editing
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        /// <returns>List of commanders to be edited in bulk</returns>
        private static IEnumerable<Leader> GetBatchEditLeaders(LeaderBatchEditArgs args)
        {
            return args.CountryMode == BatchCountryMode.All
                ? Items
                    .Where(leader =>
                        (leader.Branch == Branch.Army && args.Army) ||
                        (leader.Branch == Branch.Navy && args.Navy) ||
                        (leader.Branch == Branch.Airforce && args.Airforce))
                    .ToList()
                : Items
                    .Where(leader => args.TargetCountries.Contains(leader.Country))
                    .Where(leader =>
                        (leader.Branch == Branch.Army && args.Army) ||
                        (leader.Branch == Branch.Navy && args.Navy) ||
                        (leader.Branch == Branch.Airforce && args.Airforce))
                    .ToList();
        }

        /// <summary>
        ///     Batch edit processing log output
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        private static void LogBatchEdit(LeaderBatchEditArgs args)
        {
            Log.Verbose($"[Leader] Batch {GetBatchEditItemLog(args)} ({GetBatchEditModeLog(args)})");
        }

        /// <summary>
        ///     Get the log string of batch edit items
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        /// <returns>Log string</returns>
        private static string GetBatchEditItemLog(LeaderBatchEditArgs args)
        {
            StringBuilder sb = new StringBuilder();
            if (args.Items[(int) LeaderBatchItemId.IdealRank])
            {
                sb.AppendFormat($" ideal rank: {Config.GetText(RankNames[(int) args.IdealRank])}");
            }
            if (args.Items[(int) LeaderBatchItemId.Skill])
            {
                sb.AppendFormat($" skill: {args.Skill}");
            }
            if (args.Items[(int) LeaderBatchItemId.MaxSkill])
            {
                sb.AppendFormat($" max skill: {args.MaxSkill}");
            }
            if (args.Items[(int) LeaderBatchItemId.Experience])
            {
                sb.AppendFormat($" experience: {args.Experience}");
            }
            if (args.Items[(int) LeaderBatchItemId.Loyalty])
            {
                sb.AppendFormat($" loyalty: {args.Loyalty}");
            }
            if (args.Items[(int) LeaderBatchItemId.StartYear])
            {
                sb.AppendFormat($" start year: {args.StartYear}");
            }
            if (args.Items[(int) LeaderBatchItemId.EndYear])
            {
                sb.AppendFormat($" end year: {args.EndYear}");
            }
            if (args.Items[(int) LeaderBatchItemId.RetirementYear])
            {
                sb.AppendFormat($" retirement year: {args.RetirementYear}");
            }
            if (args.Items[(int) LeaderBatchItemId.Rank3Year])
            {
                sb.AppendFormat($" rank3 year: {args.RankYear[0]}");
            }
            if (args.Items[(int) LeaderBatchItemId.Rank2Year])
            {
                sb.AppendFormat($" rank2 year: {args.RankYear[1]}");
            }
            if (args.Items[(int) LeaderBatchItemId.Rank1Year])
            {
                sb.AppendFormat($" rank1 year: {args.RankYear[2]}");
            }
            if (args.ActionMode == BatchActionMode.Copy)
            {
                sb.Append($" Copy: {Countries.Strings[(int) args.Destination]} id: {args.Id}");
            }
            else if (args.ActionMode == BatchActionMode.Move)
            {
                sb.Append($" Move: {Countries.Strings[(int) args.Destination]} id: {args.Id}");
            }
            if (sb.Length > 0)
            {
                sb.Remove(0, 1);
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Get the log character string of the batch edit target mode
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        /// <returns>Log string</returns>
        private static string GetBatchEditModeLog(LeaderBatchEditArgs args)
        {
            StringBuilder sb = new StringBuilder();

            // Countries subject to batch editing
            if (args.CountryMode == BatchCountryMode.All)
            {
                sb.Append("ALL");
            }
            else
            {
                foreach (Country country in args.TargetCountries)
                {
                    sb.AppendFormat(" {0}", Countries.Strings[(int) country]);
                }
                if (sb.Length > 0)
                {
                    sb.Remove(0, 1);
                }
            }

            // Collective editing target military department
            if (!args.Army || !args.Navy || !args.Airforce)
            {
                sb.Append($"|{(args.Army ? 'o' : 'x')}{(args.Navy ? 'o' : 'x')}{(args.Airforce ? 'o' : 'x')}");
            }
            return sb.ToString();
        }

        #endregion

        #region ID operation

        /// <summary>
        ///     Unused commander ID To get
        /// </summary>
        /// <param name="country">Target country tag</param>
        /// <returns>Commander ID</returns>
        public static int GetNewId(Country country)
        {
            // Commander of the target country ID Maximum value of +1 Start searching from
            int id = GetMaxId(country);
            // unused ID Until you find ID of 1 Increase by little
            return GetNewId(id);
        }

        /// <summary>
        ///     Unused commander ID To get
        /// </summary>
        /// <param name="id">start ID</param>
        /// <returns>Commander ID</returns>
        public static int GetNewId(int id)
        {
            while (IdSet.Contains(id))
            {
                id++;
            }
            return id;
        }

        /// <summary>
        ///     Commander of the target country ID Get the maximum value of
        /// </summary>
        /// <param name="country">Target country</param>
        /// <returns>Commander ID</returns>
        private static int GetMaxId(Country country)
        {
            if (country == Country.None)
            {
                return 1;
            }
            List<int> ids = Items.Where(leader => leader.Country == country).Select(leader => leader.Id).ToList();
            if (!ids.Any())
            {
                return 1;
            }
            return ids.Max() + 1;
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if it has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirty()
        {
            return _dirtyFlag || _dirtyListFlag;
        }

        /// <summary>
        ///     Get if the commander list file has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        private static bool IsDirtyList()
        {
            return _dirtyListFlag;
        }

        /// <summary>
        ///     Get if it has been edited
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirty(Country country)
        {
            return DirtyFlags[(int) country];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="country">Country tag</param>
        public static void SetDirty(Country country)
        {
            DirtyFlags[(int) country] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag for the commander list file
        /// </summary>
        public static void SetDirtyList()
        {
            _dirtyListFlag = true;
        }

        /// <summary>
        ///     Clear the edited flag
        /// </summary>
        /// <param name="country">Country tag</param>
        private static void ResetDirty(Country country)
        {
            DirtyFlags[(int) country] = false;
        }

        /// <summary>
        ///     Clear the edited flag of the commander list file
        /// </summary>
        private static void ResetDirtyList()
        {
            _dirtyListFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Parameters for commander batch editing
    /// </summary>
    public class LeaderBatchEditArgs
    {
        #region Public properties

        /// <summary>
        ///     Batch edit target country mode
        /// </summary>
        public BatchCountryMode CountryMode { get; set; }

        /// <summary>
        ///     Target country list
        /// </summary>
        public List<Country> TargetCountries { get; } = new List<Country>();

        /// <summary>
        ///     Whether to target army commanders
        /// </summary>
        public bool Army { get; set; }

        /// <summary>
        ///     Whether to target Navy commanders
        /// </summary>
        public bool Navy { get; set; }

        /// <summary>
        ///     Whether to target air force commanders
        /// </summary>
        public bool Airforce { get; set; }

        /// <summary>
        ///     Batch edit operation mode
        /// </summary>
        public BatchActionMode ActionMode { get; set; }

        /// <summary>
        ///     copy / / Designated country of destination
        /// </summary>
        public Country Destination { get; set; }

        /// <summary>
        ///     start ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Bulk edit items
        /// </summary>
        public bool[] Items { get; } = new bool[Enum.GetValues(typeof (LeaderBatchItemId)).Length];

        /// <summary>
        ///     Ideal class
        /// </summary>
        public LeaderRank IdealRank { get; set; }

        /// <summary>
        ///     skill
        /// </summary>
        public int Skill { get; set; }

        /// <summary>
        ///     Maximum skill
        /// </summary>
        public int MaxSkill { get; set; }

        /// <summary>
        ///     Experience point
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        ///     Loyalty
        /// </summary>
        public int Loyalty { get; set; }

        /// <summary>
        ///     Start year
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        ///     End year
        /// </summary>
        public int EndYear { get; set; }

        /// <summary>
        ///     Retirement year
        /// </summary>
        public int RetirementYear { get; set; }

        /// <summary>
        ///     Year of appointment
        /// </summary>
        public int[] RankYear { get; } = new int[Leader.RankLength];

        #endregion
    }

    /// <summary>
    ///     Batch edit target country mode
    /// </summary>
    public enum BatchCountryMode
    {
        All, // all
        Selected, // Selected country
        Specified // Designated country
    }

    /// <summary>
    ///     Batch edit operation mode
    /// </summary>
    public enum BatchActionMode
    {
        Modify, // edit
        Copy, // copy
        Move // Move
    }

    /// <summary>
    ///     Bulk edit items ID
    /// </summary>
    public enum LeaderBatchItemId
    {
        IdealRank, // Ideal class
        Skill, // skill
        MaxSkill, // Maximum skill
        Experience, // Experience point
        Loyalty, // Loyalty
        StartYear, // Start year
        EndYear, // End year
        RetirementYear, // Retirement year
        Rank3Year, // Major General Year
        Rank2Year, // Year of middle general
        Rank1Year, // General Year
        Rank0Year // Marshal Year
    }
}
