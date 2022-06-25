using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using HoI2Editor.Parsers;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Class holding a random commander name
    /// </summary>
    public static class RandomLeaders
    {
        #region Internal field

        /// <summary>
        ///     Random commander name list
        /// </summary>
        private static readonly List<string>[] Items = new List<string>[Enum.GetValues(typeof (Country)).Length];

        /// <summary>
        ///     Loaded flag
        /// </summary>
        private static bool _loaded;

        /// <summary>
        ///     Edited flag
        /// </summary>
        private static bool _dirtyFlag;

        /// <summary>
        ///     Edited flags by nation
        /// </summary>
        private static readonly bool[] DirtyFlags = new bool[Enum.GetValues(typeof (Country)).Length];

        #endregion

        #region File reading

        /// <summary>
        ///     Request a reload of the random commander name definition file
        /// </summary>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     Reload the random commander name definition files
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
        ///     Read the random commander name definition file
        /// </summary>
        public static void Load()
        {
            // Back if loaded
            if (_loaded)
            {
                return;
            }

            // Clear the random commander name list
            foreach (Country country in Countries.Tags)
            {
                Items[(int) country] = null;
            }

            // Return if the random commander name definition file does not exist
            string fileName = Game.GetReadFileName(Game.RandomLeadersPathName);
            if (!File.Exists(fileName))
            {
                return;
            }

            // Read the random commander name definition file
            LoadFile(fileName);
            try
            {
                LoadFile(fileName);
            }
            catch (Exception)
            {
                Log.Error("[RandomLeader] Read error: {0}", fileName);
                MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                    Resources.EditorCorpsName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Clear all edited flags
            ResetDirtyAll();

            // Set the read flag
            _loaded = true;
        }

        /// <summary>
        ///     Read the random commander name definition file
        /// </summary>
        /// <param name="fileName">file name</param>
        private static void LoadFile(string fileName)
        {
            Log.Verbose("[RandomLeader] Load: {0}", Path.GetFileName(fileName));

            using (CsvLexer lexer = new CsvLexer(fileName))
            {
                while (!lexer.EndOfStream)
                {
                    ParseLine(lexer);
                }
            }
        }

        /// <summary>
        ///     Interpret the random commander name definition line
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Ministerial data</returns>
        private static void ParseLine(CsvLexer lexer)
        {
            string[] tokens = lexer.GetTokens();

            // Skip blank lines
            if (tokens == null)
            {
                return;
            }

            // Skip lines with insufficient tokens
            if (tokens.Length != 2)
            {
                Log.Warning("[RandomLeader] Invalid token count: {0} ({1} L{2})", tokens.Length, lexer.FileName,
                    lexer.LineNo);
                // Continue analysis if there are extra items
                if (tokens.Length < 2)
                {
                    return;
                }
            }

            // Country tag
            string countryName = tokens[0].ToUpper();
            if (!Countries.StringMap.ContainsKey(countryName))
            {
                Log.Warning("[RandomLeader] Invalid country: {0} ({1} L{2})", countryName, lexer.FileName, lexer.LineNo);
                return;
            }
            Country country = Countries.StringMap[countryName];

            // Random commander name
            string name = tokens[1];
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            // Add a random commander name
            AddName(name, country);
        }

        #endregion

        #region File writing

        /// <summary>
        ///     Save the random commander name definition file
        /// </summary>
        /// <returns>If saving fails false false return it</returns>
        public static bool Save()
        {
            // Do nothing if not edited
            if (!IsDirty())
            {
                return true;
            }

            string folderName = Game.GetWriteFileName(Game.DatabasePathName);
            string fileName = Game.GetWriteFileName(Game.RandomLeadersPathName);
            try
            {
                // db db. If there is no folder, create it
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                // Save the random commander name definition file
                SaveFile(fileName);
            }
            catch (Exception)
            {
                Log.Error("[RandomLeader] Write error: {0}", fileName);
                MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                    Resources.EditorCorpsName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Clear all edited flags
            ResetDirtyAll();

            return true;
        }

        /// <summary>
        ///     Save the random commander name definition file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void SaveFile(string fileName)
        {
            Log.Info("[RandomLeader] Save: {0}", Path.GetFileName(fileName));

            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                foreach (Country country in Countries.Tags.Where(country => Items[(int) country] != null))
                {
                    foreach (string name in Items[(int) country])
                    {
                        writer.WriteLine("{0};{1}", Countries.Strings[(int) country], name);
                    }
                }
            }
        }

        #endregion

        #region Random commander name operation

        /// <summary>
        ///     Get a list of random commander names
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <returns>Random commander name list</returns>
        public static IEnumerable<string> GetNames(Country country)
        {
            return Items[(int) country] ?? new List<string>();
        }

        /// <summary>
        ///     Add a random commander name
        /// </summary>
        /// <param name="name">Random commander name</param>
        /// <param name="country">Country tag</param>
        private static void AddName(string name, Country country)
        {
            // Create a list if not registered
            if (Items[(int) country] == null)
            {
                Items[(int) country] = new List<string>();
            }

            // Add a random commander name
            Items[(int) country].Add(name);
        }

        /// <summary>
        ///     Set up a random commander name list
        /// </summary>
        /// <param name="names">Random commander name list</param>
        /// <param name="country">Country tag</param>
        public static void SetNames(List<string> names, Country country)
        {
            // Return if there is no change in the random commander name list
            if (Items[(int) country] != null && names.SequenceEqual(Items[(int) country]))
            {
                return;
            }

            Log.Info("[RandomLeader] Set: <{0}>", Countries.Strings[(int) country]);

            // Set up a random commander name list
            Items[(int) country] = names;

            // Set the edited flag
            SetDirty(country);
        }

        /// <summary>
        ///     Replace random commander name
        /// </summary>
        /// <param name="s">Substitution source string</param>
        /// <param name="t">Replacement destination character string</param>
        /// <param name="country">Country tag</param>
        /// <param name="regex">Whether to use regular expressions</param>
        public static void Replace(string s, string t, Country country, bool regex)
        {
            // Do nothing if not registered
            if (Items[(int) country] == null)
            {
                return;
            }

            List<string> names =
                Items[(int) country].Select(name => regex ? Regex.Replace(name, s, t) : name.Replace(s, t)).ToList();
            SetNames(names, country);
        }

        /// <summary>
        ///     Replace all random commander names
        /// </summary>
        /// <param name="s">Substitution source string</param>
        /// <param name="t">Replacement destination character string</param>
        /// <param name="regex">Whether to use regular expressions</param>
        public static void ReplaceAll(string s, string t, bool regex)
        {
            foreach (Country country in Countries.Tags)
            {
                Replace(s, t, country, regex);
            }
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if it has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirty()
        {
            return _dirtyFlag;
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
        private static void SetDirty(Country country)
        {
            DirtyFlags[(int) country] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        private static void ResetDirtyAll()
        {
            foreach (Country country in Enum.GetValues(typeof (Country)))
            {
                DirtyFlags[(int) country] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }
}
