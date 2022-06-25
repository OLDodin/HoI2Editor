using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using HoI2Editor.Parsers;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Character string definition class
    /// </summary>
    internal static class Config
    {
        #region Public property

        /// <summary>
        ///     Language mode
        /// </summary>
        public static LanguageMode LangMode
        {
            get { return _langMode; }
            set
            {
                _langMode = value;
                Log.Info("[Config] Language Mode: {0}", LanguageModeStrings[(int) _langMode]);
            }
        }

        /// <summary>
        ///     Language index
        /// </summary>
        /// <remarks>
        ///     In a Japanese environment, the first language is Japanese, the next is English (in English version Japanese) and the rest is empty
        ///     Unless it is in a Japanese environment, the order of EXTRA1 / 2 in the British, France and France Itsumin Junami
        /// </ remarks>
        public static int LangIndex
        {
            private get { return _langIndex; }
            set
            {
                _langIndex = value;
                Log.Info("[Config] Language Index: {0} ({1})", _langIndex, LanguageStrings[(int) _langMode][_langIndex]);
            }
        }

        #endregion

        #region Internal field

        /// <summary>
        ///     Language mode
        /// </summary>
        private static LanguageMode _langMode;

        /// <summary>
        ///     Language index
        /// </summary>
        private static int _langIndex;

        /// <summary>
        ///     Character string conversion table
        /// </summary>
        private static readonly Dictionary<string, string[]> Text = new Dictionary<string, string[]>();

        /// <summary>
        ///     Replacement character string conversion table
        /// </summary>
        /// <remarks>
        ///     Registered strings are referenced in priority over Text ..
        ///     It is ignored when it is written in the file.
        ///     Use when you want to correct the duplicated character string inside the editor.
        /// </ remarks>
        private static readonly Dictionary<string, string[]> ReplacedText = new Dictionary<string, string[]>();

        /// <summary>
        ///     Supplementary string conversion table
        /// </summary>
        /// <remarks>
        ///     The registered character string is referenced when there is no definition in Text.
        ///     It is ignored when it is written in the file.
        ///     Use when you want to complement the string inside the editor.
        /// </ remarks>
        private static readonly Dictionary<string, string[]> ComplementedText = new Dictionary<string, string[]>();

        /// <summary>
        ///     Character string definition order list table
        /// </summary>
        /// <remarks>
        ///     Maintain the order of each string definition file.
        ///     Use to maintain the original order when the change is saved.
        /// </ remarks>
        private static readonly Dictionary<string, List<string>> OrderListTable = new Dictionary<string, List<string>>();

        /// <summary>
        ///     Character string reservation list table
        /// </summary>
        /// <remarks>
        ///     Maintain the character string definition that requires additional in the middle of editing.
        ///     When saving, it is added to the end of each file.
        /// </ remarks>
        private static readonly Dictionary<string, List<string>> ReservedListTable =
            new Dictionary<string, List<string>>();

        /// <summary>
        ///     Character string definition file table
        /// </summary>
        private static readonly Dictionary<string, string> TextFileTable = new Dictionary<string, string>();

        /// <summary>
        ///     Temporary key list
        /// </summary>
        /// <remarks>
        ///     It is registered when a temporary key is issued, or when the definition saved as the key is temporarily.
        ///     Deleted from the list when the temporary key is renamed.
        ///     The definition of temporary key is skipped when saving character strings.
        /// </ remarks>
        private static readonly List<string> TempKeyList = new List<string>();

        /// <summary>
        ///     List of edited files
        /// </summary>
        /// <remarks>
        ///     The file name is saved by the relative path from the config file
        /// </ remarks>
        private static readonly List<string> DirtyFiles = new List<string>();

        /// <summary>
        ///     Readed flag
        /// </summary>
        private static bool _loaded;

        /// <summary>
        ///     Number for temporary key creation
        /// </summary>
        private static int _tempNo = 1;

        /// <summary>
        ///     Regular expression to determine whether it is a temporary key
        /// </summary>
        private static readonly Regex RegexTempKey = new Regex("_EDITOR_TEMP_\\d+");

        #endregion

        #region Public number

        /// <summary>
        ///     Word name column
        /// </summary>
        public static readonly string[][] LanguageStrings =
        {
            new[] { Resources.LanguageJapanese },
            new[]
            {
                Resources.LanguageEnglish, Resources.LanguageFrench, Resources.LanguageItalian,
                Resources.LanguageSpanish, Resources.LanguageGerman, Resources.LanguagePolish,
                Resources.LanguagePortuguese, Resources.LanguageRussian, Resources.LanguageExtra1,
                Resources.LanguageExtra2
            },
            new[] { Resources.LanguageJapanese, Resources.LanguageEnglish },
            new[] { Resources.LanguageKorean },
            new[] { Resources.LanguageChinese },
            new[] { Resources.LanguageChinese }
        };

        #endregion

        #region Internal fixed number

        /// <summary>
        ///     Maximum number of languages
        /// </summary>
        private const int MaxLanguages = 10;

        /// <summary>
        ///     Language mode string
        /// </summary>
        private static readonly string[] LanguageModeStrings =
        {
            "Japanese",
            "English",
            "Patched Japanese",
            "Patched Korean",
            "Patched Traditional Chinese",
            "Patched Simplified Chinese"
        };

        /// <summary>
        ///     Key string
        /// </summary>
        private static readonly string[] KeyStrings =
        {
            "",
            "EYR_ARMY",
            "EYR_NAVY",
            "EYR_AIRFORCE",
            "EYR_AXIS",
            "EYR_ALLIES",
            "EYR_COM",
            "CATEGORY_NATIONAL_SOCIALIST",
            "CATEGORY_FASCIST",
            "CATEGORY_PATERNAL_AUTOCRAT",
            "CATEGORY_SOCIAL_CONSERVATIVE",
            "CATEGORY_MARKET_LIBERAL",
            "CATEGORY_SOCIAL_LIBERAL",
            "CATEGORY_SOCIAL_DEMOCRAT",
            "CATEGORY_LEFT_WING_RADICAL",
            "CATEGORY_LENINIST",
            "CATEGORY_STALINIST",
            "HOIG_HEAD_OF_STATE",
            "HOIG_HEAD_OF_GOVERNMENT",
            "HOIG_FOREIGN_MINISTER",
            "HOIG_ARMAMENT_MINISTER",
            "HOIG_MINISTER_OF_SECURITY",
            "HOIG_MINISTER_OF_INTELLIGENCE",
            "HOIG_CHIEF_OF_STAFF",
            "HOIG_CHIEF_OF_ARMY",
            "HOIG_CHIEF_OF_NAVY",
            "HOIG_CHIEF_OF_AIR",
            "FEOPT_AI_LEVEL1",
            "FEOPT_AI_LEVEL2",
            "FEOPT_AI_LEVEL3",
            "FEOPT_AI_LEVEL4",
            "FEOPT_AI_LEVEL5",
            "FE_DIFFI1",
            "FE_DIFFI2",
            "FE_DIFFI3",
            "FE_DIFFI4",
            "FE_DIFFI5",
            "FEOPT_GAMESPEED0",
            "FEOPT_GAMESPEED1",
            "FEOPT_GAMESPEED2",
            "FEOPT_GAMESPEED3",
            "FEOPT_GAMESPEED4",
            "FEOPT_GAMESPEED5",
            "FEOPT_GAMESPEED6",
            "FEOPT_GAMESPEED7",
            "RESOURCE_ENERGY",
            "RESOURCE_METAL",
            "RESOURCE_RARE_MATERIALS",
            "RESOURCE_OIL",
            "RESOURCE_SUPPLY",
            "RESOURCE_MONEY",
            "CIW_TRANSPORTS",
            "CIW_ESCORTS",
            "RESOURCE_IC",
            "RESOURCE_MANPOWER",
            "DOMNAME_DEM_L",
            "DOMNAME_DEM_R",
            "DOMNAME_POL_L",
            "DOMNAME_POL_R",
            "DOMNAME_FRE_L",
            "DOMNAME_FRE_R",
            "DOMNAME_FRM_L",
            "DOMNAME_FRM_R",
            "DOMNAME_PRO_L",
            "DOMNAME_PRO_R",
            "DOMNAME_DEF_L",
            "DOMNAME_DEF_R",
            "DOMNAME_INT_L",
            "DOMNAME_INT_R"
        };

        #endregion

        #region File reading

        /// <summary>
        ///     Request a re -loading of a character string definition file
        /// </summary>
        /// <remarks>
        ///     Call if there is a game folder, MOD name, game type, or language change
        /// </ remarks>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     Reload the string definition file group
        /// </summary>
        public static void Reload()
        {
            // Do nothing before reading
            if (!_loaded)
            {
                return;
            }

            _loaded = false;

            Load();
        }

        /// <summary>
        ///     Read the character string files
        /// </summary>
        public static void Load()
        {
            // If you have read it, go back
            if (_loaded)
            {
                return;
            }

            Text.Clear();
            ReplacedText.Clear();
            ComplementedText.Clear();
            OrderListTable.Clear();
            ReservedListTable.Clear();
            TextFileTable.Clear();
            TempKeyList.Clear();
            DirtyFiles.Clear();

            List<string> fileList = new List<string>();
            string folderName;
            bool error = false;

            // If you want to use a non-default map in DH, load province names.csv from the map folder
            if ((Game.Type == GameType.DarkestHour) && (Misc.MapNumber != 0))
            {
                folderName = Path.Combine(Game.MapPathName, $"Map_{Misc.MapNumber}");
                string fileName = Game.GetReadFileName(folderName, Game.ProvinceTextFileName);
                if (File.Exists(fileName))
                {
                    string name = Path.GetFileName(fileName);
                    try
                    {
                        LoadFile(fileName);
                        if (!string.IsNullOrEmpty(name))
                        {
                            fileList.Add(name.ToLower());
                        }
                    }
                    catch (Exception)
                    {
                        error = true;
                        Log.Error("[Config] Read error: {0}", fileName);
                        if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                            Resources.EditorConfig, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                            == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                }
            }

            // Read the character string files in the saved folder
            if (Game.IsExportFolderActive)
            {
                folderName = Game.GetExportFileName(Game.ConfigPathName);
                if (Directory.Exists(folderName))
                {
                    foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                    {
                        string name = Path.GetFileName(fileName);
                        if (!string.IsNullOrEmpty(name) && !fileList.Contains(name.ToLower()))
                        {
                            try
                            {
                                LoadFile(fileName);
                                fileList.Add(name.ToLower());
                            }
                            catch (Exception)
                            {
                                error = true;
                                Log.Error("[Config] Read error: {0}", fileName);
                                if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                    Resources.EditorConfig, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                    == DialogResult.Cancel)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            // Read the character string files in the mod folder
            if (Game.IsModActive)
            {
                folderName = Game.GetModFileName(Game.ConfigPathName);
                if (Directory.Exists(folderName))
                {
                    foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                    {
                        string name = Path.GetFileName(fileName);
                        if (!string.IsNullOrEmpty(name) && !fileList.Contains(name.ToLower()))
                        {
                            try
                            {
                                LoadFile(fileName);
                                fileList.Add(name.ToLower());
                            }
                            catch (Exception)
                            {
                                error = true;
                                Log.Error("[Config] Read error: {0}", fileName);
                                if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                    Resources.EditorConfig, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                    == DialogResult.Cancel)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            // Read the character string files in the vanilla folder
            folderName = Path.Combine(Game.FolderName, Game.ConfigPathName);
            if (Directory.Exists(folderName))
            {
                foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                {
                    string name = Path.GetFileName(fileName);
                    if (!string.IsNullOrEmpty(name) && !fileList.Contains(name.ToLower()))
                    {
                        try
                        {
                            LoadFile(fileName);
                        }
                        catch (Exception)
                        {
                            error = true;
                            Log.Error("[Config] Read error: {0}", fileName);
                            if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                Resources.EditorConfig, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                == DialogResult.Cancel)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            // Ao D reads the files under config \ Additional
            if (Game.Type == GameType.ArsenalOfDemocracy)
            {
                fileList.Clear();

                if (Game.IsExportFolderActive)
                {
                    folderName = Game.GetExportFileName(Game.ConfigAdditionalPathName);
                    if (Directory.Exists(folderName))
                    {
                        foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                        {
                            try
                            {
                                LoadFile(fileName);
                                string name = Path.GetFileName(fileName);
                                if (!string.IsNullOrEmpty(name))
                                {
                                    fileList.Add(Path.Combine("Additional", name.ToLower()));
                                }
                            }
                            catch (Exception)
                            {
                                error = true;
                                Log.Error("[Config] Read error: {0}", fileName);
                                if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                    Resources.EditorConfig, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                    == DialogResult.Cancel)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }

                if (Game.IsModActive)
                {
                    folderName = Game.GetModFileName(Game.ConfigAdditionalPathName);
                    if (Directory.Exists(folderName))
                    {
                        foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                        {
                            try
                            {
                                LoadFile(fileName);
                                string name = Path.GetFileName(fileName);
                                if (!string.IsNullOrEmpty(name))
                                {
                                    fileList.Add(Path.Combine("Additional", name.ToLower()));
                                }
                            }
                            catch (Exception)
                            {
                                error = true;
                                Log.Error("[Config] Read error: {0}", fileName);
                                if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                    Resources.EditorConfig, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                    == DialogResult.Cancel)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }

                folderName = Path.Combine(Game.FolderName, Game.ConfigAdditionalPathName);
                if (Directory.Exists(folderName))
                {
                    foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                    {
                        string name = Path.GetFileName(fileName);
                        if (!string.IsNullOrEmpty(name) &&
                            !fileList.Contains(Path.Combine("Additional", name.ToLower())))
                        {
                            try
                            {
                                LoadFile(fileName);
                            }
                            catch (Exception)
                            {
                                error = true;
                                Log.Error("[Config] Read error: {0}", fileName);
                                if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                    Resources.EditorConfig, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                    == DialogResult.Cancel)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            // Replace the duplicate character string
            ModifyDuplicatedStrings();

            // Supplement the lack of strings
            AddInsufficientStrings();

            // Return if you fail to read
            if (error)
            {
                return;
            }

            // Set the read flag
            _loaded = true;
        }

        /// <summary>
        ///     Read the character string file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void LoadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            string name = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            // Ignore files that are not used during the game
            if (name.Equals("editor.csv") || name.Equals("launcher.csv"))
            {
                return;
            }

            string dirName = Path.GetFileName(Path.GetDirectoryName(fileName));
            if (!string.IsNullOrEmpty(dirName) && dirName.ToLower().Equals("additional"))
            {
                name = Path.Combine("Addtional", name);
            }

            Log.Verbose("[Config] Load: {0}", name);

            // Set of tokens
            int expectedCount;
            int effectiveCount;
            if (name.Equals("editor.csv"))
            {
                expectedCount = 11;
                effectiveCount = 10;
            }
            else if (name.Equals("famous_quotes.csv"))
            {
                expectedCount = 16;
                effectiveCount = 16;
            }
            else if (name.Equals("launcher.csv"))
            {
                expectedCount = 10;
                effectiveCount = 10;
            }
            else
            {
                expectedCount = 12;
                effectiveCount = 11;
            }

            List<string> orderList = new List<string>();

            using (CsvLexer lexer = new CsvLexer(fileName))
            {
                while (!lexer.EndOfStream)
                {
                    string[] tokens = lexer.GetTokens();

                    // Read and skip the blank line
                    if (tokens == null)
                    {
                        orderList.Add("");
                        continue;
                    }

                    // Register the first token in the defined order list
                    orderList.Add(tokens[0]);

                    // Read a line where the number of tokens is not enough
                    if (tokens.Length != expectedCount)
                    {
                        Log.Warning("[Config] Invalid token count: {0} ({1} L{2})", tokens.Length, name, lexer.LineNo);

                        // If there is no X at the end / extra items, continue the analysis
                        if (tokens.Length < effectiveCount)
                        {
                            continue;
                        }
                    }

                    // Sprinkle, skip the comment line
                    if (tokens.Length <= 1 || string.IsNullOrEmpty(tokens[0]) || tokens[0][0] == '#')
                    {
                        continue;
                    }

                    string key = tokens[0].ToUpper();
                    // If the temporary key remains in the file for any reason, register as a temporary key list
                    if (RegexTempKey.IsMatch(key))
                    {
                        TempKeyList.Add(key);
                        Log.Warning("[Config] Unexpected temp key: {0} ({1} L{2})", key, name, lexer.LineNo);
                    }

                    // Register in the conversion table
                    string[] t = new string[MaxLanguages];
                    for (int i = 0; i < MaxLanguages; i++)
                    {
                        t[i] = tokens[i + 1];
                    }
                    Text[key] = t;

                    // Register in the character string definition file table
                    TextFileTable[key] = name;
                }
            }

            // Register in the definition order list table
            OrderListTable.Add(name, orderList);
        }

        #endregion

        #region File writing

        /// <summary>
        ///     Save character string files
        /// </summary>
        /// <returns>If you fail to save, return False</returns>
        public static bool Save()
        {
            // Do nothing unless you have already edited
            if (!IsDirty())
            {
                return true;
            }

            bool error = false;
            foreach (string fileName in DirtyFiles)
            {
                try
                {
                    SaveFile(fileName);
                }
                catch (Exception)
                {
                    error = true;
                    string pathName = Game.GetWriteFileName(Game.ConfigPathName, fileName);
                    Log.Error("[Config] Write error: {0}", pathName);
                    if (MessageBox.Show($"{Resources.FileWriteError}: {pathName}",
                        Resources.EditorUnit, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return false;
                    }
                }
            }

            // Return if you fail to save
            if (error)
            {
                return false;
            }

            // Unlock all edited flags
            ResetDirtyAll();

            return true;
        }

        /// <summary>
        ///     Save the character string file
        /// </summary>
        /// <param name="fileName">file name</param>
        private static void SaveFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            string name = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            string dirName = Path.GetFileName(Path.GetDirectoryName(fileName));
            if (!string.IsNullOrEmpty(dirName) && dirName.ToLower().Equals("additional"))
            {
                name = Path.Combine("Addtional", name);
            }

            Log.Info("[Config] Save: {0}", name);

            // Get the saved folder name
            string folderName = Game.GetWriteFileName(fileName.Equals(Game.ProvinceTextFileName)
                ? Game.GetProvinceNameFolderName()
                : Game.ConfigPathName);

            // Create if there is no character string folder
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            string pathName = Path.Combine(folderName, fileName);

            using (StreamWriter writer = new StreamWriter(pathName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                // Flag to write additional character strings in the first EOF definition
                bool firsteof = true;

                // Existing character string definition
                foreach (string key in OrderListTable[fileName])
                {
                    // Empty line
                    if (string.IsNullOrEmpty(key))
                    {
                        writer.WriteLine(";;;;;;;;;;;X");
                        continue;
                    }

                    // Comment line
                    if (key[0] == '#')
                    {
                        // Pioneer
                        if (key.Equals("#  STRING NAME (do not change!)"))
                        {
                            writer.WriteLine(
                                "#  STRING NAME (do not change!);English;French;Italian;Spanish;German;Polish;Portuguese;Russian;;Extra2;X");
                            continue;
                        }
                        // Output additional character strings just before the EOF at the end of the file
                        if (key.Equals("#EOF") && firsteof)
                        {
                            // Additional text columns
                            WriteAdditionalStrings(fileName, writer);
                            firsteof = false;
                        }
                        writer.WriteLine("{0};;;;;;;;;;;X", key);
                        continue;
                    }

                    // Text series definition
                    string k = key.ToUpper();
                    // Do not save the temporary key
                    if (TempKeyList.Contains(k))
                    {
                        TempKeyList.Remove(k);
                        Log.Warning("[Config] Removed unused temp key: {0}", key);
                        continue;
                    }
                    // Do not save un registered keys
                    if (!Text.ContainsKey(k))
                    {
                        Log.Warning("[Config] Skipped unexisting key: {0} ({1})", key, name);
                        continue;
                    }
                    string[] t = Text[k];
                    writer.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};X",
                        key, t[0], t[1], t[2], t[3], t[4], t[5], t[6], t[7], t[8], t[9]);
                }

                // Insurance when there is no EOF at the end of the file
                if (firsteof)
                {
                    // Additional text columns
                    WriteAdditionalStrings(fileName, writer);
                    // At the end
                    writer.WriteLine("#EOF;;;;;;;;;;;X");
                }
            }
        }

        /// <summary>
        ///     Output additional character string definitions
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="writer">For file writing</param>
        private static void WriteAdditionalStrings(string fileName, StreamWriter writer)
        {
            // Return if there is no additional string definition
            if (!ReservedListTable.ContainsKey(fileName))
            {
                return;
            }

            // Output additional character string definitions in order
            foreach (string key in ReservedListTable[fileName])
            {
                string k = key.ToUpper();
                // Do not save the temporary key
                if (TempKeyList.Contains(k))
                {
                    Log.Warning("[Config] Skipped temp key: {0} ({1})", key, fileName);
                    TempKeyList.Remove(k);
                    continue;
                }
                if (Text.ContainsKey(k))
                {
                    string[] t = Text[k];
                    writer.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};X",
                        key, t[0], t[1], t[2], t[3], t[4], t[5], t[6], t[7], t[8], t[9]);
                }
                else
                {
                    writer.WriteLine("{0};;;;;;;;;;;X", key);
                }
            }
        }

        #endregion

        #region Text column operation

        /// <summary>
        ///     Get a string
        /// </summary>
        /// <param name="key">Definition name of string</param>
        /// <returns>Acquired character string</returns>
        public static string GetText(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "";
            }
            key = key.ToUpper();

            // If it is registered in the replacement character string conversion table, refer to it with priority.
            if (ReplacedText.ContainsKey(key))
            {
                return ReplacedText[key][LangIndex];
            }

            // See if it is registered in the string conversion table
            if (Text.ContainsKey(key))
            {
                return Text[key][LangIndex];
            }

            // See if it is registered in the complementary string conversion table
            if (ComplementedText.ContainsKey(key))
            {
                return ComplementedText[key][LangIndex];
            }

            // If it is not registered in the table, return the definition name
            Log.Warning("[Config] GetText failed: {0}", key);
            return key;
        }

        /// <summary>
        ///     Get a string (reference mode acquisition version)
        /// </summary>
        /// <param name="key">Definition name of string</param>
        /// <returns>Acquired character string</returns>
        public static string GetText(string key, ref bool isComplemented)
        {
            isComplemented = false;
            if (string.IsNullOrEmpty(key))
            {
                return "";
            }
            key = key.ToUpper();

            // If it is registered in the replacement character string conversion table, refer to it with priority.
            if (ReplacedText.ContainsKey(key))
            {
                isComplemented = false;
                return ReplacedText[key][LangIndex];
            }

            // See if it is registered in the string conversion table
            if (Text.ContainsKey(key))
            {
                isComplemented = false;
                return Text[key][LangIndex];
            }

            // See if it is registered in the complementary string conversion table
            if (ComplementedText.ContainsKey(key))
            {
                isComplemented = true;
                return ComplementedText[key][LangIndex];
            }

            // If it is not registered in the table, return the definition name
            Log.Warning("[Config] GetText failed: {0}", key);
            return key;
        }

        /// <summary>
        ///     Get a string
        /// </summary>
        /// <param name="id">Text column ID</param>
        /// <returns>Acquired character string</returns>
        public static string GetText(TextId id)
        {
            string key = KeyStrings[(int) id];
            return GetText(key);
        }

        /// <summary>
        ///     Set the string
        /// </summary>
        /// <param name="key">Definition name of string</param>
        /// <param name="text">Registered character string</param>
        /// <param name="fileName">Character string definition file name</param>
        /// <remarks>
        ///     If the string is not registered, add a new one, change the value if it is registered.
        ///     Specifying the file name is valid only if there is no existing definition
        /// </ remarks>
        public static void SetText(string key, string text, string fileName)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            key = key.ToUpper();

            // If it is not registered in the string conversion table, register
            if (!Text.ContainsKey(key))
            {
                // Create if there is no reservation list
                if (!ReservedListTable.ContainsKey(fileName))
                {
                    ReservedListTable.Add(fileName, new List<string>());
                }

                // Register in the reservation list
                ReservedListTable[fileName].Add(key);

                // Register with a string conversion table
                Text[key] = new string[MaxLanguages];

                // Register in the character string definition file table
                TextFileTable[key] = fileName;

                Log.Info("[Config] Added {0} ({1})", key, fileName);
            }
            else if (TextFileTable.ContainsKey(key))
            {
                // If it is an existing definition, search and replace the file name
                fileName = TextFileTable[key];
            }

            // Change the character string of the string conversion table
            Text[key][LangIndex] = text;
            Log.Info("[Config] Set {0}: {1}", key, text);

            // Set the edited flag
            SetDirty(fileName);
        }

        /// <summary>
        ///     Set the string
        /// </summary>
        /// <param name="id">Text column ID</param>
        /// <param name="text">Registered character string</param>
        /// <param name="fileName">Character string definition file name</param>
        public static void SetText(TextId id, string text, string fileName)
        {
            string key = KeyStrings[(int) id];
            SetText(key, text, fileName);
        }

        /// <summary>
        ///     Change the character string.
        /// </summary>
        /// <param name="oldKey">Character string definition name to be changed</param>
        /// <param name="newKey">Character string definition name after change</param>
        /// <param name="fileName">Character string definition file name</param>
        public static void RenameText(string oldKey, string newKey, string fileName)
        {
            if (string.IsNullOrEmpty(oldKey) || string.IsNullOrEmpty(newKey))
            {
                return;
            }
            oldKey = oldKey.ToUpper();
            newKey = newKey.ToUpper();

            // Register with a string conversion table
            if (Text.ContainsKey(oldKey))
            {
                if (!Text.ContainsKey(newKey))
                {
                    Text.Add(newKey, Text[oldKey]);
                    Log.Info("[Config] Rename: {0} - {1}", oldKey, newKey);
                }
                else
                {
                    // With converted key: If the temporary key was saved without being renamed
                    Text[newKey] = Text[oldKey];
                    Log.Warning("[Config] Rename target already exists in text table: {0} - {1}", oldKey, newKey);
                }
                Text.Remove(oldKey);
            }
            else
            {
                if (!Text.ContainsKey(newKey))
                {
                    // Register with a string conversion table
                    Text[newKey] = new string[MaxLanguages];
                    Text[newKey][LangIndex] = "";
                }
                // No key before conversion: If the temporary key is duplicated and has already been renamed
                Log.Warning("[Config] Rename source does not exist in text table: {0} - {1}", oldKey, newKey);
            }

            // Register with the reservation list
            if (ReservedListTable.ContainsKey(fileName))
            {
                if (ReservedListTable[fileName].Contains(oldKey))
                {
                    if (!ReservedListTable[fileName].Contains(newKey))
                    {
                        ReservedListTable[fileName].Add(newKey);
                        Log.Info("[Config] Replaced reserved list: {0} - {1} ({2})", oldKey, newKey, fileName);
                    }
                    else
                    {
                        Log.Warning("[Config] Already exists in reserved list: {0} - {1} ({2})", oldKey, newKey,
                            fileName);
                    }
                    ReservedListTable[fileName].Remove(oldKey);
                }
            }

            // Rewrite the character string definition order list
            if (OrderListTable.ContainsKey(fileName) && OrderListTable[fileName].Contains(oldKey))
            {
                int index = OrderListTable[fileName].LastIndexOf(oldKey);
                OrderListTable[fileName][index] = newKey;
            }

            // Register with the character string definition file table
            if (TextFileTable.ContainsKey(oldKey))
            {
                TextFileTable.Remove(fileName);
            }
            if (!TextFileTable.ContainsKey(newKey))
            {
                TextFileTable.Add(newKey, fileName);
            }

            // Delete from a temporary key list
            if (TempKeyList.Contains(oldKey))
            {
                TempKeyList.Remove(oldKey);
                Log.Info("[Config] Removed temp list: {0}", oldKey);
            }

            // Set the edited flag
            SetDirty(fileName);
        }

        /// <summary>
        ///     Delete the string
        /// </summary>
        /// <param name="key">Definition name of string</param>
        /// <param name="fileName">Character string definition file name</param>
        public static void RemoveText(string key, string fileName)
        {
            // Delete from the string conversion table
            if (Text.ContainsKey(key))
            {
                Text.Remove(key);
                Log.Info("[Config] Removed text: {0} ({1})", key, Path.GetFileName(fileName));
            }

            // Delete from the reservation list
            if (ReservedListTable.ContainsKey(fileName) && ReservedListTable[fileName].Contains(key))
            {
                ReservedListTable[fileName].Remove(key);
                Log.Info("[Config] Removed reserved list: {0} ({1})", key, Path.GetFileName(fileName));
            }

            // Delete from the list of character string definition
            if (OrderListTable.ContainsKey(fileName) && OrderListTable[fileName].Contains(key))
            {
                OrderListTable[fileName].Remove(key);
            }

            // Delete from the character string definition file table
            if (TextFileTable.ContainsKey(key))
            {
                TextFileTable.Remove(key);
            }

            // Delete from a temporary key list
            if (TempKeyList.Contains(key))
            {
                TempKeyList.Remove(key);
                Log.Info("[Config] Removed temp list: {0}", key);
            }

            // Set the edited flag
            SetDirty(fileName);
        }

        /// <summary>
        ///     Return whether the string is registered
        /// </summary>
        /// <param name="key">Definition name of string</param>
        /// <returns>Return true if the string is registered</returns>
        public static bool ExistsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            key = key.ToUpper();

            return Text.ContainsKey(key);
        }

        /// <summary>
        ///     Judge whether it is a temporary key
        /// </summary>
        /// <param name="key">Definition name of string</param>
        /// <returns>Whether it is a temporary key</returns>
        public static bool IsTempKey(string key)
        {
            return !string.IsNullOrEmpty(key) && RegexTempKey.IsMatch(key);
        }

        /// <summary>
        ///     Get a temporary key
        /// </summary>
        /// <returns>Temporary key name</returns>
        public static string GetTempKey()
        {
            string key;
            do
            {
                key = $"_EDITOR_TEMP_{_tempNo}";
                _tempNo++;
            } while (TempKeyList.Contains(key) || ExistsKey(key));

            // Register as a temporary key list
            TempKeyList.Add(key);
            Log.Info("[Config] New temp key: {0}", key);

            return key;
        }

        /// <summary>
        ///     Register as a temporary key list
        /// </summary>
        /// <param name="key">Definition name of string</param>
        public static void AddTempKey(string key)
        {
            if (!TempKeyList.Contains(key))
            {
                TempKeyList.Add(key);
                Log.Info("[Config] Added temp key: {0}", key);
            }
        }

        /// <summary>
        ///     Register in the replacement string conversion table
        /// </summary>
        /// <param name="key">Definition name of string</param>
        /// <param name="text">Registered character string</param>
        private static void AddReplacedText(string key, string text)
        {
            // Register in the replacement string conversion table
            ReplacedText[key] = new string[MaxLanguages];
            ReplacedText[key][LangIndex] = text;
        }

        /// <summary>
        ///     Register with a complementary character string conversion table
        /// </summary>
        /// <param name="key">Definition name of string</param>
        /// <param name="text">Registered character string</param>
        private static void AddComplementedText(string key, string text)
        {
            // If there is a registered character string, do nothing
            if (Text.ContainsKey(key))
            {
                return;
            }

            // Register with a complementary character string conversion table
            ComplementedText[key] = new string[MaxLanguages];
            ComplementedText[key][LangIndex] = text;
        }

        /// <summary>
        ///     Fix duplicate character strings
        /// </summary>
        private static void ModifyDuplicatedStrings()
        {
            // Decisive Battle Doctrine: Army Commander / Navy Commander
            if (ExistsKey("NPERSONALITY_DECISIVE_BATTLE_DOCTRINE") &&
                ExistsKey("NPERSONALITY_DECISIVE_BATTLE_DOCTRINE2") &&
                GetText("NPERSONALITY_DECISIVE_BATTLE_DOCTRINE")
                    .Equals(GetText("NPERSONALITY_DECISIVE_BATTLE_DOCTRINE2")))
            {
                AddReplacedText("NPERSONALITY_DECISIVE_BATTLE_DOCTRINE",
                    $"{GetText("NPERSONALITY_DECISIVE_BATTLE_DOCTRINE")}({Resources.BranchArmy})");
                AddReplacedText("NPERSONALITY_DECISIVE_BATTLE_DOCTRINE2",
                    $"{GetText("NPERSONALITY_DECISIVE_BATTLE_DOCTRINE2")}({Resources.BranchNavy})");
            }

            // Paranoid Grandiose Delusionist: Hitler / Stalin
            if (ExistsKey("NPERSONALITY_HITLER") &&
                ExistsKey("NPERSONALITY_STALIN") &&
                GetText("NPERSONALITY_HITLER").Equals(GetText("NPERSONALITY_STALIN")))
            {
                AddReplacedText("NPERSONALITY_HITLER",
                    $"{GetText("NPERSONALITY_HITLER")}({Resources.MinisterHitler})");
                AddReplacedText("NPERSONALITY_STALIN",
                    $"{GetText("NPERSONALITY_STALIN")}({Resources.MinisterStalin})");
            }

            // German Military Adviser: Piper / Meissner / Bronzart / Seeckt / Elephant / Pasiwitz / Zerno / Golz / Siefeld / Tovne / Usedom
            if (ExistsKey("NPERSONALITY_GER_MIL_M1") &&
                ExistsKey("NPERSONALITY_GER_MIL_M2") &&
                ExistsKey("NPERSONALITY_GER_MIL_M3") &&
                ExistsKey("NPERSONALITY_GER_MIL_M4") &&
                ExistsKey("NPERSONALITY_GER_MIL_M5") &&
                ExistsKey("NPERSONALITY_GER_MIL_M6") &&
                ExistsKey("NPERSONALITY_GER_MIL_M7") &&
                ExistsKey("NPERSONALITY_GER_MIL_M8") &&
                ExistsKey("NPERSONALITY_GER_MIL_M9") &&
                ExistsKey("NPERSONALITY_GER_MIL_M10") &&
                ExistsKey("NPERSONALITY_GER_MIL_M11") &&
                GetText("NPERSONALITY_GER_MIL_M1").Equals(GetText("NPERSONALITY_GER_MIL_M2")) &&
                GetText("NPERSONALITY_GER_MIL_M1").Equals(GetText("NPERSONALITY_GER_MIL_M3")) &&
                GetText("NPERSONALITY_GER_MIL_M1").Equals(GetText("NPERSONALITY_GER_MIL_M4")) &&
                GetText("NPERSONALITY_GER_MIL_M1").Equals(GetText("NPERSONALITY_GER_MIL_M5")) &&
                GetText("NPERSONALITY_GER_MIL_M1").Equals(GetText("NPERSONALITY_GER_MIL_M6")) &&
                GetText("NPERSONALITY_GER_MIL_M1").Equals(GetText("NPERSONALITY_GER_MIL_M7")) &&
                GetText("NPERSONALITY_GER_MIL_M1").Equals(GetText("NPERSONALITY_GER_MIL_M8")) &&
                GetText("NPERSONALITY_GER_MIL_M1").Equals(GetText("NPERSONALITY_GER_MIL_M9")) &&
                GetText("NPERSONALITY_GER_MIL_M1").Equals(GetText("NPERSONALITY_GER_MIL_M10")) &&
                GetText("NPERSONALITY_GER_MIL_M1").Equals(GetText("NPERSONALITY_GER_MIL_M11")))
            {
                AddReplacedText("NPERSONALITY_GER_MIL_M1",
                    $"{GetText("NPERSONALITY_GER_MIL_M1")}({Resources.MinisterPeiper})");
                AddReplacedText("NPERSONALITY_GER_MIL_M2",
                    $"{GetText("NPERSONALITY_GER_MIL_M2")}({Resources.MinisterMeissner})");
                AddReplacedText("NPERSONALITY_GER_MIL_M3",
                    $"{GetText("NPERSONALITY_GER_MIL_M3")}({Resources.MinisterBronsart})");
                AddReplacedText("NPERSONALITY_GER_MIL_M4",
                    $"{GetText("NPERSONALITY_GER_MIL_M4")}({Resources.MinisterSeeckt})");
                AddReplacedText("NPERSONALITY_GER_MIL_M5",
                    $"{GetText("NPERSONALITY_GER_MIL_M5")}({Resources.MinisterSouchon})");
                AddReplacedText("NPERSONALITY_GER_MIL_M6",
                    $"{GetText("NPERSONALITY_GER_MIL_M6")}({Resources.MinisterPaschwitz})");
                AddReplacedText("NPERSONALITY_GER_MIL_M7",
                    $"{GetText("NPERSONALITY_GER_MIL_M7")}({Resources.MinisterSerno})");
                AddReplacedText("NPERSONALITY_GER_MIL_M8",
                    $"{GetText("NPERSONALITY_GER_MIL_M8")}({Resources.MinisterGoltz})");
                AddReplacedText("NPERSONALITY_GER_MIL_M9",
                    $"{GetText("NPERSONALITY_GER_MIL_M9")}({Resources.MinisterSievert})");
                AddReplacedText("NPERSONALITY_GER_MIL_M10",
                    $"{GetText("NPERSONALITY_GER_MIL_M10")}({Resources.MinisterThauvenay})");
                AddReplacedText("NPERSONALITY_GER_MIL_M11",
                    $"{GetText("NPERSONALITY_GER_MIL_M11")}({Resources.MinisterUsedom})");
            }

            // Cryptanalysis expert: Sinclair / Friedman
            if (ExistsKey("NPERSONALITY_SINCLAIR") &&
                ExistsKey("NPERSONALITY_FRIEDMAN") &&
                GetText("NPERSONALITY_SINCLAIR").Equals(GetText("NPERSONALITY_FRIEDMAN")))
            {
                AddReplacedText("NPERSONALITY_SINCLAIR",
                    $"{GetText("NPERSONALITY_SINCLAIR")}({Resources.MinisterSinclair})");
                AddReplacedText("NPERSONALITY_FRIEDMAN",
                    $"{GetText("NPERSONALITY_FRIEDMAN")}({Resources.MinisterFriedman})");
            }
        }

        /// <summary>
        ///     Add a shortage string
        /// </summary>
        private static void AddInsufficientStrings()
        {
            // Without a brigade
            AddComplementedText("NAME_NONE", Resources.BrigadeNone);

            // Mainland name
            AddComplementedText("CON_LAKE", Resources.ContinentLake);
            AddComplementedText("CON_ATLANTICOCEAN", Resources.ContinentAtlanticOcean);
            AddComplementedText("CON_PACIFICOCEAN", Resources.ContinentPacificOcean);
            AddComplementedText("CON_INDIANOCEAN", Resources.ContinentIndianOcean);

            // Local name
            AddComplementedText("REG_-", "-");

            if (Game.Type == GameType.ArsenalOfDemocracy)
            {
                // User -defined unit class name
                for (int i = 1; i <= 20; i++)
                {
                    AddComplementedText($"NAME_B_U{i}",
                        $"{Resources.BrigadeUser}{i}");
                }
            }

            if (Game.Type == GameType.DarkestHour)
            {
                // DH -specific research characteristics
                AddComplementedText("RT_AVIONICS", Resources.SpecialityAvionics);
                AddComplementedText("RT_MUNITIONS", Resources.SpecialityMunitions);
                AddComplementedText("RT_VEHICLE_ENGINEERING", Resources.SpecialityVehicleEngineering);
                AddComplementedText("RT_CARRIER_DESIGN", Resources.SpecialityCarrierDesign);
                AddComplementedText("RT_SUBMARINE_DESIGN", Resources.SpecialitySubmarineDesign);
                AddComplementedText("RT_FIGHTER_DESIGN", Resources.SpecialityFighterDesign);
                AddComplementedText("RT_BOMBER_DESIGN", Resources.SpecialityBomberDesign);
                AddComplementedText("RT_MOUNTAIN_TRAINING", Resources.SpecialityMountainTraining);
                AddComplementedText("RT_AIRBORNE_TRAINING", Resources.SpecialityAirborneTraining);
                AddComplementedText("RT_MARINE_TRAINING", Resources.SpecialityMarineTraining);
                AddComplementedText("RT_MANEUVER_TACTICS", Resources.SpecialityManeuverTactics);
                AddComplementedText("RT_BLITZKRIEG_TACTICS", Resources.SpecialityBlitzkriegTactics);
                AddComplementedText("RT_STATIC_DEFENSE_TACTICS", Resources.SpecialityStaticDefenseTactics);
                AddComplementedText("RT_MEDICINE", Resources.SpecialityMedicine);
                AddComplementedText("RT_CAVALRY_TACTICS", Resources.SpecialityCavalryTactics);

                // Research characteristics of user -defined
                for (int i = 1; i <= 60; i++)
                {
                    AddComplementedText($"RT_USER{i}",
                        $"{Resources.SpecialityUser}{i}");
                }

                // DH -specific unit class name
                AddComplementedText("NAME_LIGHT_CARRIER", Resources.DivisionLightCarrier);
                AddComplementedText("NAME_ROCKET_INTERCEPTOR", Resources.DivisionRocketInterceptor);
                AddComplementedText("NAME_CAVALRY_BRIGADE", Resources.BrigadeCavalry);
                AddComplementedText("NAME_SP_ANTI_AIR", Resources.BrigadeSpAntiAir);
                AddComplementedText("NAME_MEDIUM_ARMOR", Resources.BrigadeMediumTank);
                AddComplementedText("NAME_FLOATPLANE", Resources.BrigadeFloatPlane);
                AddComplementedText("NAME_LCAG", Resources.BrigadeLightCarrierAirGroup);
                AddComplementedText("NAME_AMPH_LIGHT_ARMOR_BRIGADE", Resources.BrigadeAmphibiousLightArmor);
                AddComplementedText("NAME_GLI_LIGHT_ARMOR_BRIGADE", Resources.BrigadeGliderLightArmor);
                AddComplementedText("NAME_GLI_LIGHT_ARTILLERY", Resources.BrigadeGliderLightArtillery);
                AddComplementedText("NAME_SH_ARTILLERY", Resources.BrigadeSuperHeavyArtillery);

                // User -defined unit class name
                for (int i = 33; i <= 40; i++)
                {
                    AddComplementedText($"NAME_D_RSV_{i}",
                        $"{Resources.DivisionReserved}{i}");
                }
                for (int i = 36; i <= 40; i++)
                {
                    AddComplementedText($"NAME_B_RSV_{i}",
                        $"{Resources.BrigadeReserved}{i}");
                }
                for (int i = 1; i <= 99; i++)
                {
                    AddComplementedText($"NAME_D_{i:D2}",
                        $"{Resources.DivisionUser}{i}");
                    AddComplementedText($"NAME_B_{i:D2}",
                        $"{Resources.BrigadeUser}{i}");
                }

                // Brigade unit class name not defined in DH Full
                AddComplementedText("NAME_ROCKET_ARTILLERY", Resources.BrigadeRocketArtillery);
                AddComplementedText("NAME_SP_ROCKET_ARTILLERY", Resources.BrigadeSpRocketArtillery);
                AddComplementedText("NAME_ANTITANK", Resources.BrigadeAntiTank);
                AddComplementedText("NAME_NAVAL_TORPEDOES_L", Resources.BrigadeNavalTorpedoesL);

                // Ministerial traits not defined in DH None / Light
                AddComplementedText("GENERIC MINISTER", Resources.MinisterPersonalityGenericMinister);
            }
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get whether or not it has been edited
        /// </summary>
        /// <returns>If you have edited, return True</returns>
        public static bool IsDirty()
        {
            return DirtyFiles.Count > 0;
        }

        /// <summary>
        ///     Update the edited flag
        /// </summary>
        /// <param name="fileName">Character string definition file name</param>
        public static void SetDirty(string fileName)
        {
            if (!DirtyFiles.Contains(fileName))
            {
                DirtyFiles.Add(fileName);
            }
        }

        /// <summary>
        ///     Unlock all edited flags
        /// </summary>
        private static void ResetDirtyAll()
        {
            DirtyFiles.Clear();
        }

        #endregion
    }

    /// <summary>
    ///     Language mode
    /// </summary>
    public enum LanguageMode
    {
        Japanese, // Japanese version
        English, // English version
        PatchedJapanese, // English version of Japanese language
        PatchedKorean, // English version Korean
        PatchedTraditionalChinese, // English version of traditional Chinese characters in Chinese
        PatchedSimplifiedChinese // English version Simplified Chinese translation
    }

    /// <summary>
    ///     Text column ID
    /// </summary>
    public enum TextId
    {
        Empty, // Empty text
        BranchArmy, // army
        BranchNavy, // navy
        BranchAirForce, // air force
        AllianceAxis, // Axis
        AllianceAllies, // Allied country
        AllianceComintern, // Community
        IdeologyNationalSocialist, // Socialist
        IdeologyFascist, // fascist
        IdeologyPaternalAutocrat, // Prestige
        IdeologySocialConservative, // Conservative
        IdeologyMarketLiberal, // Freedom
        IdeologySocialLiberal, // Social liberal
        IdeologySocialDemocrat, // Social democracy
        IdeologyLeftWingRadical, // Givey left -wing
        IdeologyLeninist, // Leninist
        IdeologyStalinist, // Stalinist
        MinisterHeadOfState, // leader of a nation
        MinisterHeadOfGovernment, // Government first class
        MinisterForeignMinister, // Foreign minister
        MinisterArmamentMinister, // Minister
        MinisterMinisterOfSecurity, // Minister of Home Affairs
        MinisterMinisterOfIntelligence, // Intelligence
        MinisterChiefOfStaff, // Joint Chief of Staff
        MinisterChiefOfArmy, // Commander of the Army
        MinisterChiefOfNavy, // Commander of the Navy
        MinisterChiefOfAir, // Air Force Commander
        OptionAiAggressiveness1, // Timid
        OptionAiAggressiveness2, // Boldness
        OptionAiAggressiveness3, // standard
        OptionAiAggressiveness4, // Aggressive
        OptionAiAggressiveness5, // Excessive
        OptionDifficulty1, // Extremely difficult
        OptionDifficulty2, // difficult
        OptionDifficulty3, // standard
        OptionDifficulty4, // Easy
        OptionDifficulty5, // Very easy
        OptionGameSpeed0, // Very slow
        OptionGameSpeed1, // slow slow
        OptionGameSpeed2, // Somewhat slow
        OptionGameSpeed3, // standard
        OptionGameSpeed4, // Slightly faster
        OptionGameSpeed5, // fast
        OptionGameSpeed6, // Very fast
        OptionGameSpeed7, // Extremely fast
        ResourceEnergy, // energy
        ResourceMetal, // Metal
        ResourceRareMaterials, // Hoped resource
        ResourceOil, // oil
        ResourceSupplies, // Material
        ResourceMoney, // funds
        ResourceTransports, // Transportation fleet
        ResourceEscorts, // Frigate
        ResourceIc, // Industrial power
        ResourceManpower, // Labor force
        SliderDemocratic, // Democratic
        SliderAuthoritarian, // Dictatorship
        SliderPoliticalLeft, // Political leftist
        SliderPoliticalRight, // Political right
        SliderOpenSociety, // Open society
        SliderClosedSociety, // Lock society
        SliderFreeMarket, // Free economy
        SliderCentralPlanning, // Central planning economy
        SliderStandingArmy, // standing army
        SliderDraftedArmy, // Conscription army
        SliderHawkLobby, // Hawk school
        SliderDoveLobby, // Pigment
        SliderInterventionism, // Interventionism
        SlidlaIsolationism // Isolation
    }
}
