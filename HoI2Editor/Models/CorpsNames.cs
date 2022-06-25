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
    ///     Class holding new corps name
    /// </summary>
    public static class CorpsNames
    {
        #region Internal field

        /// <summary>
        ///     Army name list
        /// </summary>
        private static readonly List<string>[,] Items =
            new List<string>[Enum.GetValues(typeof (Branch)).Length, Enum.GetValues(typeof (Country)).Length];

        /// <summary>
        ///     Loaded flag
        /// </summary>
        private static bool _loaded;

        /// <summary>
        ///     Edited flag
        /// </summary>
        private static bool _dirtyFlag;

        /// <summary>
        ///     Edited flags for each military department
        /// </summary>
        private static readonly bool[] BranchDirtyFlags = new bool[Enum.GetValues(typeof (Branch)).Length];

        /// <summary>
        ///     Edited flags by nation
        /// </summary>
        private static readonly bool[,] CountryDirtyFlags =
            new bool[Enum.GetValues(typeof (Branch)).Length, Enum.GetValues(typeof (Country)).Length];

        #endregion

        #region File reading

        /// <summary>
        ///     Request reloading of army name definition file
        /// </summary>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     Reload the army name definition files
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
        ///     Read the corps name definition file
        /// </summary>
        public static void Load()
        {
            // Back if loaded
            if (_loaded)
            {
                return;
            }

            // Clear the corps name list
            foreach (
                Branch branch in Enum.GetValues(typeof (Branch)).Cast<Branch>().Where(branch => branch != Branch.None))
            {
                foreach (Country country in Countries.Tags)
                {
                    Items[(int) branch, (int) country] = null;
                }
            }

            bool error = false;

            // Read the army corps name definition file
            string fileName = Game.GetReadFileName(Game.ArmyNamesPathName);
            if (File.Exists(fileName))
            {
                try
                {
                    LoadFile(Branch.Army, fileName);
                }
                catch (Exception)
                {
                    error = true;
                    Log.Error("[CorpsName] Read error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                        Resources.EditorCorpsName, MessageBoxButtons.OK, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return;
                    }
                }
            }
            else
            {
                error = true;
            }

            // Read the Navy Corps name definition file
            fileName = Game.GetReadFileName(Game.NavyNamesPathName);
            if (File.Exists(fileName))
            {
                try
                {
                    LoadFile(Branch.Navy, fileName);
                }
                catch (Exception)
                {
                    error = true;
                    Log.Error("[CorpsName] Read error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                        Resources.EditorCorpsName, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return;
                    }
                }
            }
            else
            {
                error = true;
            }

            // Read the Air Force Corps name definition file
            fileName = Game.GetReadFileName(Game.AirNamesPathName);
            if (File.Exists(fileName))
            {
                try
                {
                    LoadFile(Branch.Airforce, fileName);
                }
                catch (Exception)
                {
                    error = true;
                    Log.Error("[CorpsName] Read error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                        Resources.EditorCorpsName, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return;
                    }
                }
            }
            else
            {
                error = true;
            }

            // Return if reading fails
            if (error)
            {
                return;
            }

            // Clear all edited flags
            ResetDirtyAll();

            // Set the read flag
            _loaded = true;
        }

        /// <summary>
        ///     Read the corps name definition file
        /// </summary>
        /// <param name="branch">Military department</param>
        /// <param name="fileName">file name</param>
        private static void LoadFile(Branch branch, string fileName)
        {
            Log.Verbose("[CorpsName] Load: {0}", Path.GetFileName(fileName));

            using (CsvLexer lexer = new CsvLexer(fileName))
            {
                while (!lexer.EndOfStream)
                {
                    ParseLine(lexer, branch);
                }
            }
        }

        /// <summary>
        ///     Interpret the corps name definition line
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <param name="branch">Military department</param>
        private static void ParseLine(CsvLexer lexer, Branch branch)
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
                Log.Warning("[CorpsName] Invalid token count: {0} ({1} L{2})\n", tokens.Length, lexer.FileName,
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
                Log.Warning("[CorpsName] Invalid country: {0} ({1} L{2})\n", tokens.Length, lexer.FileName, lexer.LineNo);
                return;
            }
            Country country = Countries.StringMap[countryName];

            // Army name
            string name = tokens[1];
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            // Add corps name
            AddName(name, branch, country);
        }

        #endregion

        #region File writing

        /// <summary>
        ///     Save the corps name definition file
        /// </summary>
        /// <returns>If saving fails false false return it</returns>
        public static bool Save()
        {
            bool error = false;

            // Save the Army Corps name definition file
            if (IsDirty(Branch.Army))
            {
                string fileName = Game.GetWriteFileName(Game.ArmyNamesPathName);
                try
                {
                    SaveFile(Branch.Army, fileName);
                }
                catch (Exception)
                {
                    error = true;
                    Log.Error("[CorpsName] Write error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                        Resources.EditorCorpsName, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return false;
                    }
                }
            }


            // Save the Navy Corps name definition file
            if (IsDirty(Branch.Navy))
            {
                string fileName = Game.GetWriteFileName(Game.NavyNamesPathName);
                try
                {
                    SaveFile(Branch.Navy, fileName);
                }
                catch (Exception)
                {
                    error = true;
                    Log.Error("[CorpsName] Write error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                        Resources.EditorCorpsName, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return false;
                    }
                }
            }

            // Save the Air Force Corps name definition file
            if (IsDirty(Branch.Airforce))
            {
                string fileName = Game.GetWriteFileName(Game.AirNamesPathName);
                try
                {
                    SaveFile(Branch.Airforce, fileName);
                }
                catch (Exception)
                {
                    error = true;
                    Log.Error("[CorpsName] Write error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                        Resources.EditorCorpsName, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
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

            // Clear all edited flags
            ResetDirtyAll();

            return true;
        }

        /// <summary>
        ///     Save the corps name definition file
        /// </summary>
        /// <param name="branch">Military department</param>
        /// <param name="fileName">file name</param>
        private static void SaveFile(Branch branch, string fileName)
        {
            // db db. If there is no folder, create it
            string folderName = Game.GetWriteFileName(Game.DatabasePathName);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            Log.Info("[CorpsName] Save: {0}", Path.GetFileName(fileName));

            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                foreach (Country country in Countries.Tags.Where(country => Items[(int) branch, (int) country] != null))
                {
                    foreach (string name in Items[(int) branch, (int) country])
                    {
                        writer.WriteLine("{0};{1}", Countries.Strings[(int) country], name);
                    }
                }
            }
        }

        #endregion

        #region Army name operation

        /// <summary>
        ///     Get a corps name list
        /// </summary>
        /// <param name="branch">Military department</param>
        /// <param name="country">Country tag</param>
        /// <returns>Corps name list</returns>
        public static IEnumerable<string> GetNames(Branch branch, Country country)
        {
            return Items[(int) branch, (int) country] ?? new List<string>();
        }

        /// <summary>
        ///     Add corps name
        /// </summary>
        /// <param name="name">Corps name</param>
        /// <param name="branch">Military department</param>
        /// <param name="country">Country tag</param>
        private static void AddName(string name, Branch branch, Country country)
        {
            // Create a list if not registered
            if (Items[(int) branch, (int) country] == null)
            {
                Items[(int) branch, (int) country] = new List<string>();
            }

            // Add corps name
            Items[(int) branch, (int) country].Add(name);
        }

        /// <summary>
        ///     Set the corps name list
        /// </summary>
        /// <param name="names">Corps name list</param>
        /// <param name="branch">Military department</param>
        /// <param name="country">Country tag</param>
        public static void SetNames(List<string> names, Branch branch, Country country)
        {
            // Return if there is no change in the corps name list
            if (Items[(int) branch, (int) country] != null && names.SequenceEqual(Items[(int) branch, (int) country]))
            {
                return;
            }

            Log.Info("[CorpsName] Set: [{0}] <{1}>", Branches.GetName(branch), Countries.Strings[(int) country]);

            // Set up a corps name list
            Items[(int) branch, (int) country] = names;

            // Set the edited flag
            SetDirty(branch, country);
        }

        /// <summary>
        ///     Replace the corps name
        /// </summary>
        /// <param name="s">Substitution source string</param>
        /// <param name="t">Replacement destination character string</param>
        /// <param name="branch">Military department</param>
        /// <param name="country">Country tag</param>
        /// <param name="regex">Whether to use regular expressions</param>
        public static void Replace(string s, string t, Branch branch, Country country, bool regex)
        {
            // Do nothing if not registered
            if (Items[(int) branch, (int) country] == null)
            {
                return;
            }

            List<string> names = Items[(int) branch, (int) country]
                .Select(name => regex ? Regex.Replace(name, s, t) : name.Replace(s, t)).ToList();
            SetNames(names, branch, country);
        }

        /// <summary>
        ///     Replace all corps names
        /// </summary>
        /// <param name="s">Substitution source string</param>
        /// <param name="t">Replacement destination character string</param>
        /// <param name="regex">Whether to use regular expressions</param>
        public static void ReplaceAll(string s, string t, bool regex)
        {
            foreach (Branch branch in Enum.GetValues(typeof (Branch)))
            {
                foreach (Country country in Countries.Tags)
                {
                    Replace(s, t, branch, country, regex);
                }
            }
        }

        /// <summary>
        ///     Replace all military corps names
        /// </summary>
        /// <param name="s">Substitution source string</param>
        /// <param name="t">Replacement destination character string</param>
        /// <param name="country">Country tag</param>
        /// <param name="regex">Whether to use regular expressions</param>
        public static void ReplaceAllBranches(string s, string t, Country country, bool regex)
        {
            foreach (Branch branch in Enum.GetValues(typeof (Branch)))
            {
                Replace(s, t, branch, country, regex);
            }
        }

        /// <summary>
        ///     Replace army names in all countries
        /// </summary>
        /// <param name="s">Substitution source string</param>
        /// <param name="t">Replacement destination character string</param>
        /// <param name="branch">Military department</param>
        /// <param name="regex">Whether to use regular expressions</param>
        public static void ReplaceAllCountries(string s, string t, Branch branch, bool regex)
        {
            foreach (Country country in Countries.Tags)
            {
                Replace(s, t, branch, country, regex);
            }
        }

        /// <summary>
        ///     Add a serial number to the corps name
        /// </summary>
        /// <param name="prefix">prefix</param>
        /// <param name="suffix">Suffix</param>
        /// <param name="start">Starting number</param>
        /// <param name="end">End number</param>
        /// <param name="branch">Military department</param>
        /// <param name="country">Country tag</param>
        public static void AddSequential(string prefix, string suffix, int start, int end, Branch branch,
            Country country)
        {
            // Create a list if not registered
            if (Items[(int) branch, (int) country] == null)
            {
                Items[(int) branch, (int) country] = new List<string>();
            }

            for (int i = start; i <= end; i++)
            {
                string name = $"{prefix}{i}{suffix}";
                if (!Items[(int) branch, (int) country].Contains(name))
                {
                    AddName(name, branch, country);
                    SetDirty(branch, country);
                }
            }
        }

        /// <summary>
        ///     Serial number interpolation of army names
        /// </summary>
        /// <param name="branch">Military department</param>
        /// <param name="country">Country tag</param>
        public static void Interpolate(Branch branch, Country country)
        {
            // Do nothing if not registered
            if (Items[(int) branch, (int) country] == null)
            {
                return;
            }

            List<string> names = new List<string>();
            Regex r = new Regex("([^\\d]*)(\\d+)(.*)");
            string pattern = "";
            int prev = 0;
            bool found = false;
            foreach (string name in Items[(int) branch, (int) country])
            {
                if (r.IsMatch(name))
                {
                    int n;
                    if (int.TryParse(r.Replace(name, "$2"), out n))
                    {
                        if (!found)
                        {
                            // Set the output pattern
                            pattern = r.Replace(name, "$1{0}$3");
                            found = true;
                        }
                        else
                        {
                            // Interpolate between the previous number and the current number
                            if (prev + 1 < n)
                            {
                                for (int i = prev + 1; i < n; i++)
                                {
                                    string s = string.Format(pattern, i);
                                    if (!names.Contains(s))
                                    {
                                        names.Add(s);
                                    }
                                }
                            }
                        }
                        prev = n;
                    }
                }
                names.Add(name);
            }

            SetNames(names, branch, country);
        }

        /// <summary>
        ///     Serial number interpolation of all army names
        /// </summary>
        public static void InterpolateAll()
        {
            foreach (Branch branch in Enum.GetValues(typeof (Branch)))
            {
                foreach (Country country in Countries.Tags)
                {
                    Interpolate(branch, country);
                }
            }
        }

        /// <summary>
        ///     Serial number interpolation of all military corps names
        /// </summary>
        /// <param name="country">Country tag</param>
        public static void InterpolateAllBranches(Country country)
        {
            foreach (Branch branch in Enum.GetValues(typeof (Branch)))
            {
                Interpolate(branch, country);
            }
        }

        /// <summary>
        ///     Interpolate the names of all nations in sequence
        /// </summary>
        /// <param name="branch">Military department</param>
        public static void InterpolateAllCountries(Branch branch)
        {
            foreach (Country country in Countries.Tags)
            {
                Interpolate(branch, country);
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
        /// <param name="branch">Military department</param>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirty(Branch branch)
        {
            return BranchDirtyFlags[(int) branch];
        }

        /// <summary>
        ///     Get if it has been edited
        /// </summary>
        /// <param name="branch">Military department</param>
        /// <param name="country">Country tag</param>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirty(Branch branch, Country country)
        {
            return CountryDirtyFlags[(int) branch, (int) country];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="branch">Military department</param>
        /// <param name="country">Country tag</param>
        private static void SetDirty(Branch branch, Country country)
        {
            CountryDirtyFlags[(int) branch, (int) country] = true;
            BranchDirtyFlags[(int) branch] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        private static void ResetDirtyAll()
        {
            foreach (Branch branch in Enum.GetValues(typeof (Branch)))
            {
                foreach (Country country in Enum.GetValues(typeof (Country)))
                {
                    CountryDirtyFlags[(int) branch, (int) country] = false;
                }
                BranchDirtyFlags[(int) branch] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }
}
