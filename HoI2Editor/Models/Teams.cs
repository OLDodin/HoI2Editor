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
    ///     Research institute data group
    /// </summary>
    public static class Teams
    {
        #region Public properties

        /// <summary>
        ///     List of master research institutes
        /// </summary>
        public static List<Team> Items { get; }

        /// <summary>
        ///     Correspondence between country tag and research institution file name
        /// </summary>
        public static Dictionary<Country, string> FileNameMap { get; }

        /// <summary>
        ///     Already used ID list
        /// </summary>
        public static HashSet<int> IdSet { get; }

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
        ///     Edited flag
        /// </summary>
        private static readonly bool[] DirtyFlags = new bool[Enum.GetValues(typeof (Country)).Length];

        /// <summary>
        ///     Edited flag for research institution list file
        /// </summary>
        private static bool _dirtyListFlag;

        #endregion

        #region Initialization

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Teams()
        {
            // List of master research institutes
            Items = new List<Team>();

            // Correspondence between country tag and research institution file name
            FileNameMap = new Dictionary<Country, string>();

            // Already used ID list
            IdSet = new HashSet<int>();
        }

        #endregion

        #region File reading

        /// <summary>
        ///     Request a reload of the laboratory file
        /// </summary>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     Reload the research institute files
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
        ///     Read research institution files
        /// </summary>
        public static void Load()
        {
            // Back if loaded
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
        ///     Lazy loading of research institute files
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
        ///     Read research institution files
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
        ///     Read research institute files (HoI2 / AoD / DH-MOD When not in use )
        /// </summary>
        /// <returns>If reading fails false false return it</returns>
        private static bool LoadHoI2()
        {
            List<string> list = new List<string>();
            string folderName;
            bool error = false;

            // Read the research institute file in the save folder
            if (Game.IsExportFolderActive)
            {
                folderName = Game.GetExportFileName(Game.TeamPathName);
                if (Directory.Exists(folderName))
                {
                    foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                    {
                        try
                        {
                            // Read research institution files
                            LoadFile(fileName);

                            // Register the read file name in the research institute file list
                            string name = Path.GetFileName(fileName);
                            if (!string.IsNullOrEmpty(name))
                            {
                                list.Add(name.ToLower());
                            }
                        }
                        catch (Exception)
                        {
                            error = true;
                            Log.Error("[Team] Read error: {0}", fileName);
                            if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                Resources.EditorTeam, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                == DialogResult.Cancel)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            // MOD Read the research institution files in the folder
            if (Game.IsModActive)
            {
                folderName = Game.GetModFileName(Game.TeamPathName);
                if (Directory.Exists(folderName))
                {
                    foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                    {
                        try
                        {
                            // Read research institution files
                            LoadFile(fileName);

                            // Register the read file name in the research institute file list
                            string name = Path.GetFileName(fileName);
                            if (!string.IsNullOrEmpty(name))
                            {
                                list.Add(name.ToLower());
                            }
                        }
                        catch (Exception)
                        {
                            error = true;
                            Log.Error("[Team] Read error: {0}", fileName);
                            if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                Resources.EditorTeam, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                == DialogResult.Cancel)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            // Read the research institution file in the vanilla folder
            folderName = Path.Combine(Game.FolderName, Game.TeamPathName);
            if (Directory.Exists(folderName))
            {
                foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                {
                    // MOD Ignore files read in folders
                    string name = Path.GetFileName(fileName);
                    if (string.IsNullOrEmpty(name) || list.Contains(name.ToLower()))
                    {
                        continue;
                    }

                    try
                    {
                        // Read research institution files
                        LoadFile(fileName);
                    }
                    catch (Exception)
                    {
                        error = true;
                        Log.Error("[Team] Read error: {0}", fileName);
                        if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                            Resources.EditorTeam, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
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
        ///     Read research institute files (DH-MOD while using it )
        /// </summary>
        /// <returns>If reading fails false false return it</returns>
        private static bool LoadDh()
        {
            // If the research institute list file does not exist, use the conventional reading method.
            string listFileName = Game.GetReadFileName(Game.DhTeamListPathName);
            if (!File.Exists(listFileName))
            {
                return LoadHoI2();
            }

            // Read the research institution list file
            IEnumerable<string> fileList;
            try
            {
                fileList = LoadList(listFileName);
            }
            catch (Exception)
            {
                Log.Error("[Team] Read error: {0}", listFileName);
                MessageBox.Show($"{Resources.FileReadError}: {listFileName}",
                    Resources.EditorTeam, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            bool error = false;
            foreach (string fileName in fileList.Select(name => Game.GetReadFileName(Game.TeamPathName, name)))
            {
                try
                {
                    // Read research institution files
                    LoadFile(fileName);
                }
                catch (Exception)
                {
                    error = true;
                    Log.Error("[Team] Read error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                        Resources.EditorTeam, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return false;
                    }
                }
            }

            return !error;
        }

        /// <summary>
        ///     Read the research institution list file (DH)
        /// </summary>
        private static IEnumerable<string> LoadList(string fileName)
        {
            Log.Verbose("[Team] Load: {0}", Path.GetFileName(fileName));

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
        ///     Read the research institute file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void LoadFile(string fileName)
        {
            Log.Verbose("[Team] Load: {0}", Path.GetFileName(fileName));

            using (CsvLexer lexer = new CsvLexer(fileName))
            {
                // Skip empty files
                if (lexer.EndOfStream)
                {
                    return;
                }

                // Country tag reading
                string[] tokens = lexer.GetTokens();
                if (tokens == null || tokens.Length == 0 || string.IsNullOrEmpty(tokens[0]))
                {
                    return;
                }
                // Do nothing for unsupported country tags
                if (!Countries.StringMap.ContainsKey(tokens[0].ToUpper()))
                {
                    return;
                }
                Country country = Countries.StringMap[tokens[0].ToUpper()];

                // Skip files with only header lines
                if (lexer.EndOfStream)
                {
                    return;
                }

                while (!lexer.EndOfStream)
                {
                    Team team = ParseLine(lexer, country);

                    // Skip blank lines
                    if (team == null)
                    {
                        continue;
                    }

                    Items.Add(team);
                }

                ResetDirty(country);

                if (country != Country.None && !FileNameMap.ContainsKey(country))
                {
                    FileNameMap.Add(country, lexer.FileName);
                }
            }
        }

        /// <summary>
        ///     Interpret the research institute definition line
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <param name="country">National tag</param>
        /// <returns>Research institution data</returns>
        private static Team ParseLine(CsvLexer lexer, Country country)
        {
            string[] tokens = lexer.GetTokens();

            // ID Skip lines that are not specified
            if (string.IsNullOrEmpty(tokens?[0]))
            {
                return null;
            }

            // Skip lines with insufficient tokens
            if (tokens.Length != 39)
            {
                Log.Warning("[Team] Invalid token count: {0} ({1} L{2})", tokens.Length, lexer.FileName, lexer.LineNo);
                // At the end x x There is no / / Continue analysis if there are extra items
                if (tokens.Length < 38)
                {
                    return null;
                }
            }

            Team team = new Team { Country = country };
            int index = 0;

            // ID
            int id;
            if (!int.TryParse(tokens[index], out id))
            {
                Log.Warning("[Team] Invalid id: {0} ({1} L{2})", tokens[index], lexer.FileName, lexer.LineNo);
                return null;
            }
            team.Id = id;
            index++;

            // name
            team.Name = tokens[index];
            index++;

            // Image file name
            team.PictureName = tokens[index];
            index++;

            if (team.PictureName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 || System.Text.RegularExpressions.Regex.IsMatch(team.PictureName, @"\p{IsCyrillic}"))
            {
                Log.Warning("[Team] Picture name contains invalid char {0}:  L{1}", lexer.PathName, lexer.LineNo);
            }
            else
            {
                string imgFileName = Path.Combine(Game.PersonPicturePathName, team.PictureName);
                imgFileName += ".bmp";
                string pathName = Game.GetReadFileName(imgFileName);
                if (!File.Exists(pathName))
                {
                    Log.Info("[Team] Picture not exist {0}:  L{1}", lexer.PathName, lexer.LineNo);
                }
            }

            // skill
            int skill;
            if (int.TryParse(tokens[index], out skill))
            {
                team.Skill = skill;
            }
            else
            {
                team.Skill = 1;
                Log.Warning("[Team] Invalid skill: {0} [{1}: {2}] ({3} L{4})", tokens[index], team.Id, team.Name,
                    lexer.FileName, lexer.LineNo);
            }
            index++;

            // Start year
            int startYear;
            if (int.TryParse(tokens[index], out startYear))
            {
                team.StartYear = startYear;
            }
            else
            {
                team.StartYear = 1930;
                Log.Warning("[Team] Invalid start year: {0} [{1}: {2}] ({3} L{4})", tokens[index], team.Id, team.Name,
                    lexer.FileName, lexer.LineNo);
            }
            index++;

            // End year
            int endYear;
            if (int.TryParse(tokens[index], out endYear))
            {
                team.EndYear = endYear;
            }
            else
            {
                team.EndYear = 1970;
                Log.Warning("[Team] Invalid end year: {0} [{1}: {2}] ({3} L{4})", tokens[index], team.Id, team.Name,
                    lexer.FileName, lexer.LineNo);
            }
            index++;

            // Research characteristics
            for (int i = 0; i < Team.SpecialityLength; i++, index++)
            {
                string s = tokens[index].ToLower();

                // Empty string
                if (string.IsNullOrEmpty(s))
                {
                    team.Specialities[i] = TechSpeciality.None;
                    continue;
                }

                // Invalid research characteristic string
                if (!Techs.SpecialityStringMap.ContainsKey(s))
                {
                    team.Specialities[i] = TechSpeciality.None;
                    Log.Warning("[Team] Invalid speciality: {0} [{1}: {2}] ({3} L{4})", tokens[index], team.Id,
                        team.Name, lexer.FileName, lexer.LineNo);
                    continue;
                }

                // Unsupported research characteristics
                TechSpeciality speciality = Techs.SpecialityStringMap[s];
                if (!Techs.Specialities.Contains(speciality))
                {
                    Log.Warning("[Team] Invalid speciality: {0} [{1}: {2}] ({3} L{4})", tokens[index], team.Id,
                        team.Name, lexer.FileName, lexer.LineNo);
                    continue;
                }

                team.Specialities[i] = speciality;
            }

            return team;
        }

        #endregion

        #region File writing

        /// <summary>
        ///     Save research institution files
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

            // Save the research institute list file
            if ((Game.Type == GameType.DarkestHour) && IsDirtyList())
            {
                try
                {
                    SaveList();
                }
                catch (Exception)
                {
                    string fileName = Game.GetWriteFileName(Game.DhTeamListPathName);
                    Log.Error("[Team] Write error: {0}", fileName);
                    MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                        Resources.EditorTeam, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            bool error = false;
            foreach (Country country in Countries.Tags
                .Where(country => DirtyFlags[(int) country] && country != Country.None))
            {
                try
                {
                    // Save the laboratory file
                    SaveFile(country);
                }
                catch (Exception)
                {
                    error = true;
                    string fileName = Game.GetWriteFileName(Game.MinisterPathName, Game.GetMinisterFileName(country));
                    Log.Error("[Team] Write error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                        Resources.EditorMinister, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                        return false;
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
        ///     Save the research institution list file (DH)
        /// </summary>
        private static void SaveList()
        {
            // Create a database folder if it does not exist
            string folderName = Game.GetWriteFileName(Game.DatabasePathName);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string fileName = Game.GetWriteFileName(Game.DhTeamListPathName);
            Log.Info("[Team] Save: {0}", Path.GetFileName(fileName));

            // Write the registered research institution file names in order
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
        ///     Save the laboratory file
        /// </summary>
        /// <param name="country">Country tag</param>
        private static void SaveFile(Country country)
        {
            // Create a research institution folder if it does not exist
            string folderName = Game.GetWriteFileName(Game.TeamPathName);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string name = Game.GetTeamFileName(country);
            string fileName = Path.Combine(folderName, name);
            Log.Info("[Team] Save: {0}", name);

            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                // Write header line
                writer.WriteLine(
                    "{0};Name;Pic Name;Skill;Start Year;End Year;Speciality1;Speciality2;Speciality3;Speciality4;Speciality5;Speciality6;Speciality7;Speciality8;Speciality9;Speciality10;Speciality11;Speciality12;Speciality13;Speciality14;Speciality15;Speciality16;Speciality17;Speciality18;Speciality19;Speciality20;Speciality21;Speciality22;Speciality23;Speciality24;Speciality25;Speciality26;Speciality27;Speciality28;Speciality29;Speciality30;Speciality31;Speciality32;x",
                    Countries.Strings[(int) country]);

                // Write the research institution definition lines in order
                foreach (Team team in Items.Where(team => team.Country == country))
                {
                    writer.Write(
                        "{0};{1};{2};{3};{4};{5}",
                        team.Id,
                        team.Name,
                        team.PictureName,
                        team.Skill,
                        team.StartYear,
                        team.EndYear);
                    for (int i = 0; i < Team.SpecialityLength; i++)
                    {
                        writer.Write(";{0}",
                            team.Specialities[i] != TechSpeciality.None
                                ? Techs.SpecialityStrings[(int) team.Specialities[i]]
                                : "");
                    }
                    writer.WriteLine(";x");

                    // Clear the edited flag
                    team.ResetDirtyAll();
                }
            }

            ResetDirty(country);
        }

        #endregion

        #region Research institution list operation

        /// <summary>
        ///     Add an item to the research institution list
        /// </summary>
        /// <param name="team">Items to be added</param>
        public static void AddItem(Team team)
        {
            Log.Info("[Team] Add team: ({0}: {1}) <{2}>", team.Id, team.Name, Countries.Strings[(int) team.Country]);

            Items.Add(team);
        }

        /// <summary>
        ///     Insert an item in the research institution list
        /// </summary>
        /// <param name="team">Items to be inserted</param>
        /// <param name="position">Item immediately before the insertion position</param>
        public static void InsertItem(Team team, Team position)
        {
            int index = Items.IndexOf(position) + 1;

            Log.Info("[Team] Insert team: {0} ({1}: {2}) <{3}>", index, team.Id, team.Name,
                Countries.Strings[(int) team.Country]);

            Items.Insert(index, team);
        }

        /// <summary>
        ///     Remove an item from the research institute list
        /// </summary>
        /// <param name="team">Items to be deleted</param>
        public static void RemoveItem(Team team)
        {
            Log.Info("[Team] Remove team: ({0}: {1}) <{2}>", team.Id, team.Name,
                Countries.Strings[(int) team.Country]);

            Items.Remove(team);

            // Already used ID Remove from list
            IdSet.Remove(team.Id);
        }

        /// <summary>
        ///     Move items in the research institute list
        /// </summary>
        /// <param name="src">Item of move source</param>
        /// <param name="dest">Item to move to</param>
        public static void MoveItem(Team src, Team dest)
        {
            int srcIndex = Items.IndexOf(src);
            int destIndex = Items.IndexOf(dest);

            Log.Info("[Team] Move team: {0} -> {1} ({2}: {3}) <{4}>", srcIndex, destIndex, src.Id, src.Name,
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
        public static void BatchEdit(TeamBatchEditArgs args)
        {
            LogBatchEdit(args);

            IEnumerable<Team> teams = GetBatchEditTeams(args);
            Country newCountry;
            switch (args.ActionMode)
            {
                case BatchActionMode.Modify:
                    // Batch edit research institutes
                    foreach (Team team in teams)
                    {
                        BatchEditTeam(team, args);
                    }
                    break;

                case BatchActionMode.Copy:
                    // Copy the research institute
                    newCountry = args.Destination;
                    int id = args.Id;
                    foreach (Team team in teams)
                    {
                        id = GetNewId(id);
                        Team newTeam = new Team(team)
                        {
                            Country = newCountry,
                            Id = id
                        };
                        newTeam.SetDirtyAll();
                        Items.Add(newTeam);
                    }

                    // Set the edited flag for the destination country
                    SetDirty(newCountry);

                    // If the copy destination country does not exist in the file list, add it
                    if (!FileNameMap.ContainsKey(newCountry))
                    {
                        FileNameMap.Add(newCountry, Game.GetTeamFileName(newCountry));
                        SetDirtyList();
                    }
                    break;

                case BatchActionMode.Move:
                    // Move research institute
                    newCountry = args.Destination;
                    foreach (Team team in teams)
                    {
                        // Set the edited flag for the country before the move
                        SetDirty(team.Country);

                        team.Country = newCountry;
                        team.SetDirty(TeamItemId.Country);
                    }

                    // Set the edited flag for the destination country
                    SetDirty(newCountry);

                    // If the destination country does not exist in the file list, add it.
                    if (!FileNameMap.ContainsKey(newCountry))
                    {
                        FileNameMap.Add(newCountry, Game.GetTeamFileName(newCountry));
                        SetDirtyList();
                    }
                    break;
            }
        }

        /// <summary>
        ///     Individual processing of batch editing
        /// </summary>
        /// <param name="team">Target research institute</param>
        /// <param name="args">Batch editing parameters</param>
        private static void BatchEditTeam(Team team, TeamBatchEditArgs args)
        {
            // skill
            if (args.Items[(int) TeamBatchItemId.Skill])
            {
                if (team.Skill != args.Skill)
                {
                    team.Skill = args.Skill;
                    team.SetDirty(TeamItemId.Skill);
                    SetDirty(team.Country);
                }
            }

            // Start year
            if (args.Items[(int) TeamBatchItemId.StartYear])
            {
                if (team.StartYear != args.StartYear)
                {
                    team.StartYear = args.StartYear;
                    team.SetDirty(TeamItemId.StartYear);
                    SetDirty(team.Country);
                }
            }

            // End year
            if (args.Items[(int) TeamBatchItemId.EndYear])
            {
                if (team.EndYear != args.EndYear)
                {
                    team.EndYear = args.EndYear;
                    team.SetDirty(TeamItemId.EndYear);
                    SetDirty(team.Country);
                }
            }
        }

        /// <summary>
        ///     Get a list of research institutes to be edited in bulk
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        /// <returns>List of research institutes subject to batch editing</returns>
        private static IEnumerable<Team> GetBatchEditTeams(TeamBatchEditArgs args)
        {
            return args.CountryMode == BatchCountryMode.All
                ? Items.ToList()
                : Items.Where(team => args.TargetCountries.Contains(team.Country)).ToList();
        }

        /// <summary>
        ///     Batch edit processing log output
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        private static void LogBatchEdit(TeamBatchEditArgs args)
        {
            Log.Verbose($"[Team] Batch {GetBatchEditItemLog(args)} ({GetBatchEditModeLog(args)})");
        }

        /// <summary>
        ///     Get the log string of batch edit items
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        /// <returns>Log string</returns>
        private static string GetBatchEditItemLog(TeamBatchEditArgs args)
        {
            StringBuilder sb = new StringBuilder();
            if (args.Items[(int) TeamBatchItemId.Skill])
            {
                sb.AppendFormat($" skill: {args.Skill}");
            }
            if (args.Items[(int) TeamBatchItemId.StartYear])
            {
                sb.AppendFormat($" start year: {args.StartYear}");
            }
            if (args.Items[(int) TeamBatchItemId.EndYear])
            {
                sb.AppendFormat($" end year: {args.EndYear}");
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
        private static string GetBatchEditModeLog(TeamBatchEditArgs args)
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

            return sb.ToString();
        }

        #endregion

        #region ID operation

        /// <summary>
        ///     Unused research institute ID To get
        /// </summary>
        /// <param name="country">Target country tag</param>
        /// <returns>research Institute ID</returns>
        public static int GetNewId(Country country)
        {
            // Research institutes in the target country ID Maximum value of +1 Start searching from
            int id = GetMaxId(country);
            // unused ID Until you find ID of 1 Increase by little
            return GetNewId(id);
        }

        /// <summary>
        ///     Unused research institute ID To get
        /// </summary>
        /// <param name="id">start ID</param>
        /// <returns>research Institute ID</returns>
        public static int GetNewId(int id)
        {
            while (IdSet.Contains(id))
            {
                id++;
            }
            return id;
        }

        /// <summary>
        ///     Research institutes in the target country IDGet the maximum value of
        /// </summary>
        /// <param name="country">Target country</param>
        /// <returns>research Institute ID</returns>
        private static int GetMaxId(Country country)
        {
            if (country == Country.None)
            {
                return 1;
            }
            List<int> ids = Items.Where(team => team.Country == country).Select(team => team.Id).ToList();
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
        ///     Get whether the research institute list file has been edited
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
        ///     Set the edited flag of the laboratory list file
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
        ///     Clear the edited flag of the research institute list file
        /// </summary>
        private static void ResetDirtyList()
        {
            _dirtyListFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Parameters for batch editing of research institutes
    /// </summary>
    public class TeamBatchEditArgs
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
        public bool[] Items { get; } = new bool[Enum.GetValues(typeof (TeamBatchItemId)).Length];

        /// <summary>
        ///     skill
        /// </summary>
        public int Skill { get; set; }

        /// <summary>
        ///     Start year
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        ///     End year
        /// </summary>
        public int EndYear { get; set; }

        #endregion
    }

    /// <summary>
    ///     Bulk edit items ID
    /// </summary>
    public enum TeamBatchItemId
    {
        Skill, // skill
        StartYear, // Start year
        EndYear // End year
    }
}
