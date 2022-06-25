using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Parsers;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;
using HoI2Editor.Writers;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Scenario data group
    /// </summary>
    public static class Scenarios
    {
        #region Public properties

        /// <summary>
        ///     Scenario data
        /// </summary>
        public static Scenario Data { get; private set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Scenario file name
        /// </summary>
        private static string _fileName;

        /// <summary>
        ///     Loaded flag
        /// </summary>
        private static bool _loaded;

        /// <summary>
        ///     Edited flag
        /// </summary>
        private static bool _dirtyFlag;

        /// <summary>
        ///     Correspondence between country tags and major country settings
        /// </summary>
        private static readonly Dictionary<Country, MajorCountrySettings> MajorTable =
            new Dictionary<Country, MajorCountrySettings>();

        /// <summary>
        ///     Correspondence between country tag and national setting
        /// </summary>
        private static readonly Dictionary<Country, CountrySettings> CountryTable =
            new Dictionary<Country, CountrySettings>();

        /// <summary>
        ///     Correspondence between country tags and national relations
        /// </summary>
        private static readonly Dictionary<Country, Dictionary<Country, Relation>> RelationTable =
            new Dictionary<Country, Dictionary<Country, Relation>>();

        /// <summary>
        ///     Correspondence of country tag and non-invasion treaty
        /// </summary>
        private static readonly Dictionary<Country, Dictionary<Country, Treaty>> NonAggressionTable =
            new Dictionary<Country, Dictionary<Country, Treaty>>();

        /// <summary>
        ///     Correspondence between country tag and peace treaty
        /// </summary>
        private static readonly Dictionary<Country, Dictionary<Country, Treaty>> PeaceTable =
            new Dictionary<Country, Dictionary<Country, Treaty>>();

        /// <summary>
        ///     Correspondence between country tags and intelligence settings
        /// </summary>
        private static readonly Dictionary<Country, Dictionary<Country, SpySettings>> SpyTable =
            new Dictionary<Country, Dictionary<Country, SpySettings>>();

        /// <summary>
        ///     Providence ID And provision settings mapping
        /// </summary>
        private static readonly Dictionary<int, ProvinceSettings> ProvinceTable =
            new Dictionary<int, ProvinceSettings>();

        /// <summary>
        ///     Providence ID Correspondence between and Provins holding countries
        /// </summary>
        private static readonly Dictionary<int, Country> OwnedCountries = new Dictionary<int, Country>();

        /// <summary>
        ///     Providence IDCorrespondence between and province-dominated countries
        /// </summary>
        private static readonly Dictionary<int, Country> ControlledCountries = new Dictionary<int, Country>();

        /// <summary>
        ///     Used type When id Pair of
        /// </summary>
        private static Dictionary<int, HashSet<int>> _usedTypeIds;

        #endregion

        #region Public constant

        /// <summary>
        ///     Alliance standard type
        /// </summary>
        public const int DefaultAllianceType = 15000;

        /// <summary>
        ///     War standard type
        /// </summary>
        public const int DefaultWarType = 9430;

        /// <summary>
        ///     Diplomatic agreement standard type
        /// </summary>
        public const int DefaultTreatyType = 16384;

        /// <summary>
        ///     Commander's standard type
        /// </summary>
        public const int DefaultLeaderType = 6;

        /// <summary>
        ///     Ministerial standard type
        /// </summary>
        public const int DefaultMinisterType = 9;

        /// <summary>
        ///     Research institute standards type
        /// </summary>
        public const int DefaultTeamType = 10;

        /// <summary>
        ///     Month name string
        /// </summary>
        public static readonly string[] MonthStrings =
        {
            "january",
            "february",
            "march",
            "april",
            "may",
            "june",
            "july",
            "august",
            "september",
            "october",
            "november",
            "december"
        };

        /// <summary>
        ///     Diplomatic agreement character string
        /// </summary>
        public static readonly string[] TreatyStrings =
        {
            "non_aggression",
            "peace",
            "trade"
        };

        /// <summary>
        ///     Weather string
        /// </summary>
        public static readonly string[] WeatherStrings =
        {
            "",
            "clear",
            "frozen",
            "raining",
            "snowing",
            "storm",
            "blizzard",
            "muddy"
        };

        /// <summary>
        ///     Polity string
        /// </summary>
        public static readonly string[] GovernmentStrings =
        {
            "",
            "nazi",
            "fascist",
            "paternal_autocrat",
            "social_conservative",
            "market_liberal",
            "social_liberal",
            "social_democrat",
            "left_wing_radical",
            "leninist",
            "stalinist"
        };

        /// <summary>
        ///     Building string
        /// </summary>
        public static readonly string[] BuildingStrings =
        {
            "",
            "ic",
            "infrastructure",
            "coastal_fort",
            "land_fort",
            "anti_air",
            "air_base",
            "naval_base",
            "radar_station",
            "nuclear_reactor",
            "rocket_test",
            "synthetic_oil",
            "synthetic_rares",
            "nuclear_power"
        };

        /// <summary>
        ///     Mission string
        /// </summary>
        public static readonly string[] MissionStrings =
        {
            "",
            "attack",
            "rebase",
            "strat_redeploy",
            "support_attack",
            "support_defense",
            "reserves",
            "anti_partisan_duty",
            "artillery_bombardment",
            "planned_defense",
            "air_superiority",
            "ground_attack",
            "runway_cratering",
            "installation_strike",
            "interdiction",
            "naval_strike",
            "port_strike",
            "logistical_strike",
            "strategic_bombardment",
            "air_supply",
            "airborne_assault",
            "air_scramble",
            "convoy_raiding",
            "asw",
            "naval_interdiction",
            "shore_bombardment",
            "amphibious_assault",
            "sea_transport",
            "naval_combat_patrol",
            "sneak_move",
            "naval_scramble",
            "convoy_air_raiding",
            "naval_port_strike",
            "naval_airbase_strike",
            "nuke",
            "retreat"
        };

        /// <summary>
        ///     Convoy string
        /// </summary>
        public static readonly string[] ConvoyStrings =
        {
            "",
            "transports",
            "escorts"
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     Initialization process
        /// </summary>
        public static void Init()
        {
            // Initialize major country settings
            InitMajorCountries();

            // Initialize national information
            InitCountries();

            // Initialize the non-invasion treaty
            InitNonAggressions();

            // Initialize the peace treaty
            InitPeaces();

            // Used type When id id Initialize the set of
            InitTypeIds();

            // Initialize province information
            InitProvinces();
        }

        #endregion

        #region File reading

        /// <summary>
        ///     Get if the file has been read
        /// </summary>
        /// <returns>If you read the file true true return it</returns>
        public static bool IsLoaded()
        {
            return _loaded;
        }

        /// <summary>
        ///     Request a file reload
        /// </summary>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     Reload the scenario files
        /// </summary>
        public static void Reload()
        {
            // Do nothing before loading
            if (!_loaded)
            {
                return;
            }

            _loaded = false;

            LoadFiles();
        }

        /// <summary>
        ///     Read scenario files
        /// </summary>
        public static void Load(string fileName)
        {
            // Do nothing if it matches the loaded file name
            if (_loaded && fileName.Equals(_fileName))
            {
                return;
            }

            _fileName = fileName;

            LoadFiles();
        }

        /// <summary>
        ///     Read the scenario file
        /// </summary>
        private static void LoadFiles()
        {
            // Interpret the scenario file
            Log.Verbose("[Scenario] Load: {0}", Path.GetFileName(_fileName));
            try
            {
                Data = new Scenario();
                ScenarioParser.Parse(_fileName, Data);
            }
            catch (Exception)
            {
                Log.Error("[Scenario] Read error: {0}", _fileName);
                MessageBox.Show($"{Resources.FileReadError}: {_fileName}",
                    Resources.EditorScenario, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Clear all edited flags
            ResetDirtyAll();

            // Set the read flag
            _loaded = true;
        }

        #endregion

        #region File writing

        /// <summary>
        ///     Save the scenario files
        /// </summary>
        /// <returns>If saving fails false false return it</returns>
        public static bool Save()
        {
            // Do nothing before loading
            if (!_loaded)
            {
                return true;
            }

            // Province settings ID Sort in order
            SortProvinceSettings();

            Scenario scenario = Data;
            if (scenario.IsDirtyProvinces())
            {
                // bases.inc
                if (scenario.IsBaseProvinceSettings && !SaveBasesIncFile())
                {
                    return false;
                }

                // bases_DOD.inc
                if (scenario.IsBaseDodProvinceSettings && !SaveBasesDodIncFile())
                {
                    return false;
                }

                // depots.inc
                if (scenario.IsDepotsProvinceSettings && !SaveDepotsIncFile())
                {
                    return false;
                }
            }

            // vp.inc
            if (scenario.IsVpProvinceSettings && scenario.IsDirtyVpInc() && !SaveVpIncFile())
            {
                return false;
            }

            // By country inc
            if (scenario.IsDirtyProvinces())
            {
                if (scenario.Countries.Any(settings => !SaveCountryFiles(settings)))
                {
                    return false;
                }
            }
            else
            {
                if (scenario.Countries.Where(settings => settings.IsDirty())
                    .Any(settings => !SaveCountryFiles(settings)))
                {
                    return false;
                }
            }

            // Scenario file
            if (scenario.IsDirty() && !SaveScenarioFile())
            {
                return false;
            }

            // Clear the edited flag
            ResetDirtyAll();

            return true;
        }

        /// <summary>
        ///     Save the scenario file
        /// </summary>
        /// <returns>If the save is successful true true return it</returns>
        private static bool SaveScenarioFile()
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                return false;
            }

            // If the scenario folder does not exist, create it
            string folderName = Game.GetWriteFileName(Game.ScenarioPathName);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string fileName = Path.Combine(folderName, Path.GetFileName(_fileName));
            try
            {
                // Save the scenario file
                Log.Info("[Scenario] Save: {0}", Path.GetFileName(fileName));
                ScenarioWriter.Write(Data, fileName);
            }
            catch (Exception)
            {
                Log.Error("[Scenario] Write error: {0}", fileName);
                MessageBox.Show($"{Resources.FileWriteError}: {fileName}", Resources.EditorScenario,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Save the base definition file
        /// </summary>
        /// <returns>If the save is successful true true return it</returns>
        private static bool SaveBasesIncFile()
        {
            // Create a scenario include folder if it does not exist
            string folderName = Game.GetWriteFileName(Path.Combine(Game.ScenarioPathName, Data.IncludeFolder));
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string fileName = Path.Combine(folderName, Game.BasesIncFileName);
            try
            {
                Log.Info("[Scenario] Save: {0}", Path.GetFileName(fileName));
                ScenarioWriter.WriteBasesInc(Data, fileName);
            }
            catch (Exception)
            {
                Log.Error("[Scenario] Write error: {0}", fileName);
                MessageBox.Show($"{Resources.FileWriteError}: {fileName}", Resources.EditorScenario,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Save the base definition file(DH Full 33 Year scenario )
        /// </summary>
        /// <returns>If the save is successful true true return it</returns>
        private static bool SaveBasesDodIncFile()
        {
            // Create a scenario include folder if it does not exist
            string folderName = Game.GetWriteFileName(Path.Combine(Game.ScenarioPathName, Data.IncludeFolder));
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string fileName = Path.Combine(folderName, Game.BasesIncDodFileName);
            try
            {
                Log.Info("[Scenario] Save: {0}", Path.GetFileName(fileName));
                ScenarioWriter.WriteBasesDodInc(Data, fileName);
            }
            catch (Exception)
            {
                Log.Error("[Scenario] Write error: {0}", fileName);
                MessageBox.Show($"{Resources.FileWriteError}: {fileName}", Resources.EditorScenario,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Save the resource stockpile definition file
        /// </summary>
        /// <returns>If the save is successful true true return it</returns>
        private static bool SaveDepotsIncFile()
        {
            // Create a scenario include folder if it does not exist
            string folderName = Game.GetWriteFileName(Path.Combine(Game.ScenarioPathName, Data.IncludeFolder));
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string fileName = Path.Combine(folderName, Game.DepotsIncFileName);
            try
            {
                Log.Info("[Scenario] Save: {0}", Path.GetFileName(fileName));
                ScenarioWriter.WriteDepotsInc(Data, fileName);
            }
            catch (Exception)
            {
                Log.Error("[Scenario] Write error: {0}", fileName);
                MessageBox.Show($"{Resources.FileWriteError}: {fileName}", Resources.EditorScenario,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     VP Save the definition file
        /// </summary>
        /// <returns>If the save is successful true true return it</returns>
        private static bool SaveVpIncFile()
        {
            // Create a scenario include folder if it does not exist
            string folderName = Game.GetWriteFileName(Path.Combine(Game.ScenarioPathName, Data.IncludeFolder));
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string fileName = Path.Combine(folderName, Game.VpIncFileName);
            try
            {
                Log.Info("[Scenario] Save: {0}", Path.GetFileName(fileName));
                ScenarioWriter.WriteVpInc(Data, fileName);
            }
            catch (Exception)
            {
                Log.Error("[Scenario] Write error: {0}", fileName);
                MessageBox.Show($"{Resources.FileWriteError}: {fileName}", Resources.EditorScenario,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     By country inc Save the file
        /// </summary>
        /// <param name="settings">National setting</param>
        private static bool SaveCountryFiles(CountrySettings settings)
        {
            // Create a scenario include folder if it does not exist
            string folderName = Game.GetWriteFileName(Path.Combine(Game.ScenarioPathName, Data.IncludeFolder));
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string fileName = Path.Combine(folderName, string.IsNullOrEmpty(settings.FileName)
                ? $"{Countries.Strings[(int) settings.Country].ToLower()}.inc"
                : settings.FileName);
            try
            {
                Log.Info("[Scenario] Save: {0}", Path.GetFileName(fileName));
                ScenarioWriter.WriteCountrySettings(settings, Data, fileName);
            }
            catch (Exception)
            {
                Log.Error("[Scenario] Write error: {0}", fileName);
                MessageBox.Show($"{Resources.FileWriteError}: {fileName}", Resources.EditorScenario,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Nation

        /// <summary>
        ///     Initialize major country settings
        /// </summary>
        private static void InitMajorCountries()
        {
            MajorTable.Clear();
            foreach (MajorCountrySettings major in Data.Header.MajorCountries)
            {
                MajorTable[major.Country] = major;
            }
        }

        /// <summary>
        ///     Initialize national information
        /// </summary>
        private static void InitCountries()
        {
            CountryTable.Clear();
            RelationTable.Clear();
            SpyTable.Clear();
            foreach (CountrySettings settings in Data.Countries)
            {
                Country country = settings.Country;

                // Correspondence between country tags and national settings
                CountryTable[country] = settings;

                // Correspondence between country tags and national relations
                if (!RelationTable.ContainsKey(country))
                {
                    RelationTable.Add(country, new Dictionary<Country, Relation>());
                }
                foreach (Relation relation in settings.Relations)
                {
                    RelationTable[country][relation.Country] = relation;
                }

                // Correspondence between country tags and intelligence settings
                if (!SpyTable.ContainsKey(country))
                {
                    SpyTable.Add(country, new Dictionary<Country, SpySettings>());
                }
                foreach (SpySettings spy in settings.Intelligence)
                {
                    SpyTable[country][spy.Country] = spy;
                }
            }
        }

        /// <summary>
        ///     Initialize the non-aggression treaty
        /// </summary>
        private static void InitNonAggressions()
        {
            NonAggressionTable.Clear();
            foreach (Treaty nonAggression in Data.GlobalData.NonAggressions)
            {
                if (!NonAggressionTable.ContainsKey(nonAggression.Country1))
                {
                    NonAggressionTable.Add(nonAggression.Country1, new Dictionary<Country, Treaty>());
                }
                NonAggressionTable[nonAggression.Country1][nonAggression.Country2] = nonAggression;
                if (!NonAggressionTable.ContainsKey(nonAggression.Country2))
                {
                    NonAggressionTable.Add(nonAggression.Country2, new Dictionary<Country, Treaty>());
                }
                NonAggressionTable[nonAggression.Country2][nonAggression.Country1] = nonAggression;
            }
        }

        /// <summary>
        ///     Initialize the peace treaty
        /// </summary>
        private static void InitPeaces()
        {
            PeaceTable.Clear();
            foreach (Treaty peace in Data.GlobalData.Peaces)
            {
                if (!PeaceTable.ContainsKey(peace.Country1))
                {
                    PeaceTable.Add(peace.Country1, new Dictionary<Country, Treaty>());
                }
                PeaceTable[peace.Country1][peace.Country2] = peace;
                if (!PeaceTable.ContainsKey(peace.Country2))
                {
                    PeaceTable.Add(peace.Country2, new Dictionary<Country, Treaty>());
                }
                PeaceTable[peace.Country2][peace.Country1] = peace;
            }
        }

        /// <summary>
        ///     Get major country settings
        /// </summary>
        /// <param name="country">Target country</param>
        /// <returns>Major country setting</returns>
        public static MajorCountrySettings GetMajorCountrySettings(Country country)
        {
            return MajorTable.ContainsKey(country) ? MajorTable[country] : null;
        }

        /// <summary>
        ///     Set major country settings
        /// </summary>
        /// <param name="major">Major country setting</param>
        public static void SetMajorCountrySettings(MajorCountrySettings major)
        {
            MajorTable[major.Country] = major;
        }

        /// <summary>
        ///     Delete major country settings
        /// </summary>
        /// <param name="major">Major country setting</param>
        public static void RemoveMajorCountrySettings(MajorCountrySettings major)
        {
            if (MajorTable.ContainsKey(major.Country))
            {
                MajorTable.Remove(major.Country);
            }
        }

        /// <summary>
        ///     Get the national setting
        /// </summary>
        /// <param name="country">Target country</param>
        /// <returns>National setting</returns>
        public static CountrySettings GetCountrySettings(Country country)
        {
            return CountryTable.ContainsKey(country) ? CountryTable[country] : null;
        }

        /// <summary>
        ///     Create a national setting
        /// </summary>
        /// <param name="country">Target country</param>
        /// <returns>National setting</returns>
        public static CountrySettings CreateCountrySettings(Country country)
        {
            CountrySettings settings = new CountrySettings
            {
                Country = country,
                FileName = country != Country.CON ? $"{Countries.Strings[(int) country].ToLower()}.inc" : "congo.inc"
            };

            // Register in the national setting table
            CountryTable[country] = settings;

            // Add to national setting list
            Data.Countries.Add(settings);

            // Add include files
            Data.IncludeFiles.Add($"scenarios\\{Data.IncludeFolder}\\{settings.FileName}");

            // Set the edited flag of the scenario file
            Data.SetDirty();

            return settings;
        }

        /// <summary>
        ///     Acquire national relations
        /// </summary>
        /// <param name="country1">Target country 1</param>
        /// <param name="country2">Target country 2</param>
        /// <returns>National relations</returns>
        public static Relation GetCountryRelation(Country country1, Country country2)
        {
            return RelationTable.ContainsKey(country1) && RelationTable[country1].ContainsKey(country2)
                ? RelationTable[country1][country2]
                : null;
        }

        /// <summary>
        ///     Set up national relations
        /// </summary>
        /// <param name="country">Target country</param>
        /// <param name="relation">National relations</param>
        public static void SetCountryRelation(Country country, Relation relation)
        {
            if (!RelationTable.ContainsKey(country))
            {
                RelationTable.Add(country, new Dictionary<Country, Relation>());
            }
            RelationTable[country][relation.Country] = relation;
        }

        /// <summary>
        ///     Obtain a non-invasion treaty
        /// </summary>
        /// <param name="country1">Target country 1</param>
        /// <param name="country2">Target country 2</param>
        /// <returns>Non-invasion treaty</returns>
        public static Treaty GetNonAggression(Country country1, Country country2)
        {
            return NonAggressionTable.ContainsKey(country1) && NonAggressionTable[country1].ContainsKey(country2)
                ? NonAggressionTable[country1][country2]
                : null;
        }

        /// <summary>
        ///     Set up a non-aggression treaty
        /// </summary>
        /// <param name="treaty">Non-invasion treaty</param>
        public static void SetNonAggression(Treaty treaty)
        {
            if (!NonAggressionTable.ContainsKey(treaty.Country1))
            {
                NonAggressionTable.Add(treaty.Country1, new Dictionary<Country, Treaty>());
            }
            NonAggressionTable[treaty.Country1][treaty.Country2] = treaty;

            if (!NonAggressionTable.ContainsKey(treaty.Country2))
            {
                NonAggressionTable.Add(treaty.Country2, new Dictionary<Country, Treaty>());
            }
            NonAggressionTable[treaty.Country2][treaty.Country1] = treaty;
        }

        /// <summary>
        ///     Delete the non-aggression treaty
        /// </summary>
        /// <param name="treaty">Non-invasion treaty</param>
        public static void RemoveNonAggression(Treaty treaty)
        {
            if (NonAggressionTable.ContainsKey(treaty.Country1) &&
                NonAggressionTable[treaty.Country1].ContainsKey(treaty.Country2))
            {
                NonAggressionTable[treaty.Country1].Remove(treaty.Country2);
            }

            if (NonAggressionTable.ContainsKey(treaty.Country2) &&
                NonAggressionTable[treaty.Country2].ContainsKey(treaty.Country1))
            {
                NonAggressionTable[treaty.Country2].Remove(treaty.Country1);
            }
        }

        /// <summary>
        ///     Obtain a peace treaty
        /// </summary>
        /// <param name="country1">Target country 1</param>
        /// <param name="country2">Target country 2</param>
        /// <returns>Peace treaty</returns>
        public static Treaty GetPeace(Country country1, Country country2)
        {
            return PeaceTable.ContainsKey(country1) && PeaceTable[country1].ContainsKey(country2)
                ? PeaceTable[country1][country2]
                : null;
        }

        /// <summary>
        ///     Set up a peace treaty
        /// </summary>
        /// <param name="peace">Peace treaty</param>
        public static void SetPeace(Treaty peace)
        {
            if (!PeaceTable.ContainsKey(peace.Country1))
            {
                PeaceTable.Add(peace.Country1, new Dictionary<Country, Treaty>());
            }
            PeaceTable[peace.Country1][peace.Country2] = peace;

            if (!PeaceTable.ContainsKey(peace.Country2))
            {
                PeaceTable.Add(peace.Country2, new Dictionary<Country, Treaty>());
            }
            PeaceTable[peace.Country2][peace.Country1] = peace;
        }

        /// <summary>
        ///     Delete the peace treaty
        /// </summary>
        /// <param name="peace">Peace treaty</param>
        public static void RemovePeace(Treaty peace)
        {
            if (PeaceTable.ContainsKey(peace.Country1) &&
                PeaceTable[peace.Country1].ContainsKey(peace.Country2))
            {
                PeaceTable[peace.Country1].Remove(peace.Country2);
            }

            if (PeaceTable.ContainsKey(peace.Country2) &&
                PeaceTable[peace.Country2].ContainsKey(peace.Country1))
            {
                PeaceTable[peace.Country2].Remove(peace.Country1);
            }
        }

        /// <summary>
        ///     Get intelligence settings
        /// </summary>
        /// <param name="country1">Target country 1</param>
        /// <param name="country2">Target country 2</param>
        /// <returns>Intelligence settings</returns>
        public static SpySettings GetCountryIntelligence(Country country1, Country country2)
        {
            return SpyTable.ContainsKey(country1) && SpyTable[country1].ContainsKey(country2)
                ? SpyTable[country1][country2]
                : null;
        }

        /// <summary>
        ///     Set intelligence settings
        /// </summary>
        /// <param name="country">Target country</param>
        /// <param name="spy">Intelligence settings</param>
        public static void SetCountryIntelligence(Country country, SpySettings spy)
        {
            if (!SpyTable.ContainsKey(country))
            {
                SpyTable.Add(country, new Dictionary<Country, SpySettings>());
            }
            SpyTable[country][spy.Country] = spy;
        }

        /// <summary>
        ///     Get the country tag and country name string
        /// </summary>
        /// <param name="country">Nation</param>
        /// <returns>Country tag and country name string</returns>
        public static string GetCountryTagName(Country country)
        {
            return $"{Countries.Strings[(int) country]} {GetCountryName(country)}";
        }

        /// <summary>
        ///     Get the country name string
        /// </summary>
        /// <param name="country">Nation</param>
        /// <returns>Country name string</returns>
        public static string GetCountryName(Country country)
        {
            // Country name set in major countries
            MajorCountrySettings major = GetMajorCountrySettings(country);
            if (!string.IsNullOrEmpty(major?.Name))
            {
                return Config.ExistsKey(major.Name) ? Config.GetText(major.Name) : "";
            }

            // Country name of national setting
            CountrySettings settings = GetCountrySettings(country);
            if (!string.IsNullOrEmpty(settings?.Name))
            {
                return Config.ExistsKey(settings.Name) ? Config.GetText(settings.Name) : settings.Name;
            }

            // Standard country name
            return Countries.GetName(country);
        }

        #endregion

        #region Providence

        /// <summary>
        ///     Get the Providence name
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <returns>Providence name</returns>
        public static string GetProvinceName(int id)
        {
            Province province = Provinces.Items[id];
            ProvinceSettings settings = GetProvinceSettings(id);
            if (!string.IsNullOrEmpty(settings?.Name))
            {
                return Config.ExistsKey(settings.Name) ? Config.GetText(settings.Name) : "";
            }
            return province.GetName();
        }

        /// <summary>
        ///     Get the Providence name
        /// </summary>
        /// <param name="province">Providence</param>
        /// <param name="settings">Providence settings</param>
        /// <returns>Providence name</returns>
        public static string GetProvinceName(Province province, ProvinceSettings settings)
        {
            if (!string.IsNullOrEmpty(settings?.Name))
            {
                return Config.ExistsKey(settings.Name) ? Config.GetText(settings.Name) : "";
            }
            return province.GetName();
        }

        /// <summary>
        ///     Set the province name
        /// </summary>
        /// <param name="province">Providence</param>
        /// <param name="settings">Providence settings</param>
        /// <param name="s">Providence name</param>
        public static void SetProvinceName(Province province, ProvinceSettings settings, string s)
        {
            if (!string.IsNullOrEmpty(settings?.Name))
            {
                Config.SetText(settings.Name, s, Game.ScenarioTextFileName);
            }
            else
            {
                province.SetName(s);
            }
        }

        /// <summary>
        ///     Initialize provision information
        /// </summary>
        private static void InitProvinces()
        {
            // Province ID And the mapping of province settings
            ProvinceTable.Clear();
            foreach (ProvinceSettings settings in Data.Provinces)
            {
                ProvinceTable[settings.Id] = settings;
            }

            // Province ID Correspondence between and Provins holding countries
            OwnedCountries.Clear();
            foreach (CountrySettings settings in Data.Countries)
            {
                foreach (int id in settings.OwnedProvinces)
                {
                    if (OwnedCountries.ContainsKey(id))
                    {
                        Log.Warning("[Scenario] duplicated owned province: {0} <{1}> <{2}>", id,
                            Countries.Strings[(int) settings.Country], Countries.Strings[(int) OwnedCountries[id]]);
                    }
                    OwnedCountries[id] = settings.Country;
                }
            }

            // Providence ID Correspondence between and province-dominated countries
            ControlledCountries.Clear();
            foreach (CountrySettings settings in Data.Countries)
            {
                foreach (int id in settings.ControlledProvinces)
                {
                    if (ControlledCountries.ContainsKey(id))
                    {
                        Log.Warning("[Scenario] duplicated controlled province: {0} <{1}> <{2}>", id,
                            Countries.Strings[(int) settings.Country], Countries.Strings[(int) ControlledCountries[id]]);
                    }
                    ControlledCountries[id] = settings.Country;
                }
            }
        }

        /// <summary>
        ///     Get province settings
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <returns>Providence settings</returns>
        public static ProvinceSettings GetProvinceSettings(int id)
        {
            if (!ProvinceTable.ContainsKey(id))
            {
                return null;
            }
            return ProvinceTable[id];
        }

        /// <summary>
        ///     Add provision settings
        /// </summary>
        /// <param name="settings">Providence settings</param>
        public static void AddProvinceSettings(ProvinceSettings settings)
        {
            ProvinceSettings prev = Data.Provinces.Find(ps => ps.Id == settings.Id);
            if (prev == null)
            {
                Data.Provinces.Add(settings);
                ProvinceTable[settings.Id] = settings;
            }
            else
            {
                MergeProvinceSettings(prev, settings);
            }
        }

        /// <summary>
        ///     Merge province settings
        /// </summary>
        /// <param name="prev">Province settings 1</param>
        /// <param name="settings">Province settings 2</param>
        private static void MergeProvinceSettings(ProvinceSettings prev, ProvinceSettings settings)
        {
            if (!string.IsNullOrEmpty(settings.Name))
            {
                prev.Name = settings.Name;
            }
            if (settings.Ic != null)
            {
                prev.Ic = settings.Ic;
            }
            if (settings.Infrastructure != null)
            {
                prev.Infrastructure = settings.Infrastructure;
            }
            if (settings.LandFort != null)
            {
                prev.LandFort = settings.LandFort;
            }
            if (settings.CoastalFort != null)
            {
                prev.CoastalFort = settings.CoastalFort;
            }
            if (settings.AntiAir != null)
            {
                prev.AntiAir = settings.AntiAir;
            }
            if (settings.AirBase != null)
            {
                prev.AirBase = settings.AirBase;
            }
            if (settings.NavalBase != null)
            {
                prev.NavalBase = settings.NavalBase;
            }
            if (settings.RadarStation != null)
            {
                prev.RadarStation = settings.RadarStation;
            }
            if (settings.NuclearReactor != null)
            {
                prev.NuclearReactor = settings.NuclearReactor;
            }
            if (settings.RocketTest != null)
            {
                prev.RocketTest = settings.RocketTest;
            }
            if (settings.SyntheticOil != null)
            {
                prev.SyntheticOil = settings.SyntheticOil;
            }
            if (settings.SyntheticRares != null)
            {
                prev.SyntheticRares = settings.SyntheticRares;
            }
            if (settings.NuclearPower != null)
            {
                prev.NuclearPower = settings.NuclearPower;
            }
            if (!DoubleHelper.IsZero(settings.Manpower))
            {
                prev.Manpower = settings.Manpower;
            }
            if (!DoubleHelper.IsZero(settings.MaxManpower))
            {
                prev.MaxManpower = settings.MaxManpower;
            }
            if (!DoubleHelper.IsZero(settings.EnergyPool))
            {
                prev.EnergyPool = settings.EnergyPool;
            }
            if (!DoubleHelper.IsZero(settings.Energy))
            {
                prev.Energy = settings.Energy;
            }
            if (!DoubleHelper.IsZero(settings.MaxEnergy))
            {
                prev.MaxEnergy = settings.MaxEnergy;
            }
            if (!DoubleHelper.IsZero(settings.MetalPool))
            {
                prev.MetalPool = settings.MetalPool;
            }
            if (!DoubleHelper.IsZero(settings.Metal))
            {
                prev.Metal = settings.Metal;
            }
            if (!DoubleHelper.IsZero(settings.MaxMetal))
            {
                prev.MaxMetal = settings.MaxMetal;
            }
            if (!DoubleHelper.IsZero(settings.RareMaterialsPool))
            {
                prev.RareMaterialsPool = settings.RareMaterialsPool;
            }
            if (!DoubleHelper.IsZero(settings.RareMaterials))
            {
                prev.RareMaterials = settings.RareMaterials;
            }
            if (!DoubleHelper.IsZero(settings.MaxRareMaterials))
            {
                prev.MaxRareMaterials = settings.MaxRareMaterials;
            }
            if (!DoubleHelper.IsZero(settings.OilPool))
            {
                prev.OilPool = settings.OilPool;
            }
            if (!DoubleHelper.IsZero(settings.Oil))
            {
                prev.Oil = settings.Oil;
            }
            if (!DoubleHelper.IsZero(settings.MaxOil))
            {
                prev.MaxOil = settings.MaxOil;
            }
            if (!DoubleHelper.IsZero(settings.SupplyPool))
            {
                prev.SupplyPool = settings.SupplyPool;
            }
            if (settings.Vp != 0)
            {
                prev.Vp = settings.Vp;
            }
            if (!DoubleHelper.IsZero(settings.RevoltRisk))
            {
                prev.RevoltRisk = settings.RevoltRisk;
            }
        }

        /// <summary>
        ///     Providence settings ID Sort in order
        /// </summary>
        private static void SortProvinceSettings()
        {
            Data.Provinces.Sort((x, y) => x.Id - y.Id);
        }

        /// <summary>
        ///     Add core provisions
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <param name="settings">National setting</param>
        public static void AddCoreProvince(int id, CountrySettings settings)
        {
            // Add core provisions
            if (!settings.NationalProvinces.Contains(id))
            {
                settings.NationalProvinces.Add(id);
            }
        }

        /// <summary>
        ///     Remove core provisions
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <param name="settings">National setting</param>
        public static void RemoveCoreProvince(int id, CountrySettings settings)
        {
            // Remove core provinces
            if (settings.NationalProvinces.Contains(id))
            {
                settings.NationalProvinces.Remove(id);
            }
        }

        /// <summary>
        ///     Add owned provinces
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <param name="settings">National setting</param>
        public static void AddOwnedProvince(int id, CountrySettings settings)
        {
            // Add owned provinces
            if (!settings.OwnedProvinces.Contains(id))
            {
                settings.OwnedProvinces.Add(id);
            }

            // Delete the province owned by the original possession country
            if (OwnedCountries.ContainsKey(id))
            {
                GetCountrySettings(OwnedCountries[id]).OwnedProvinces.Remove(id);
            }

            // Province ID And update the correspondence between provinces and provinces
            OwnedCountries[id] = settings.Country;
        }

        /// <summary>
        ///     Delete owned provinces
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <param name="settings">National setting</param>
        public static void RemoveOwnedProvince(int id, CountrySettings settings)
        {
            // Delete owned provisions
            if (settings.OwnedProvinces.Contains(id))
            {
                settings.OwnedProvinces.Remove(id);
            }

            // Providence ID And remove the association between Provins countries
            OwnedCountries.Remove(id);
        }

        /// <summary>
        ///     Add dominance province
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <param name="settings">National setting</param>
        public static void AddControlledProvince(int id, CountrySettings settings)
        {
            // Add Dominance Providence
            if (!settings.ControlledProvinces.Contains(id))
            {
                settings.ControlledProvinces.Add(id);
            }

            // Remove the ruled province of the original ruler
            if (ControlledCountries.ContainsKey(id))
            {
                GetCountrySettings(ControlledCountries[id]).ControlledProvinces.Remove(id);
            }

            // Providence ID And update the correspondence between provinces and provinces
            ControlledCountries[id] = settings.Country;
        }

        /// <summary>
        ///     Remove Domination Province
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <param name="settings">National setting</param>
        public static void RemoveControlledProvince(int id, CountrySettings settings)
        {
            // Remove Dominance Providence
            if (settings.ControlledProvinces.Contains(id))
            {
                settings.ControlledProvinces.Remove(id);
            }

            // Province ID And remove the correspondence between the province and the province
            ControlledCountries.Remove(id);
        }

        /// <summary>
        ///     Add territorial claim province
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <param name="settings">National setting</param>
        public static void AddClaimedProvince(int id, CountrySettings settings)
        {
            // Add territorial claim provision
            if (!settings.ClaimedProvinces.Contains(id))
            {
                settings.ClaimedProvinces.Add(id);
            }
        }

        /// <summary>
        ///     Remove territorial claim provisions
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <param name="settings">National setting</param>
        public static void RemoveClaimedProvince(int id, CountrySettings settings)
        {
            // Remove territorial claim province
            if (settings.ClaimedProvinces.Contains(id))
            {
                settings.ClaimedProvinces.Remove(id);
            }
        }

        #endregion

        #region type When id id Pair of

        /// <summary>
        ///     type When id id Initialize the set of
        /// </summary>
        private static void InitTypeIds()
        {
            _usedTypeIds = new Dictionary<int, HashSet<int>>
            {
                { DefaultAllianceType, new HashSet<int>() },
                { DefaultWarType, new HashSet<int>() },
                { DefaultTreatyType, new HashSet<int>() },
                { DefaultLeaderType, new HashSet<int>() },
                { DefaultMinisterType, new HashSet<int>() },
                { DefaultTeamType, new HashSet<int>() }
            };

            Scenario scenario = Data;
            ScenarioGlobalData data = scenario.GlobalData;

            if (data.Axis != null)
            {
                AddTypeId(data.Axis.Id);
            }
            if (data.Allies != null)
            {
                AddTypeId(data.Allies.Id);
            }
            if (data.Comintern != null)
            {
                AddTypeId(data.Comintern.Id);
            }
            foreach (Alliance alliance in data.Alliances)
            {
                AddTypeId(alliance.Id);
            }
            foreach (War war in data.Wars)
            {
                if (war.Attackers != null)
                {
                    AddTypeId(war.Attackers.Id);
                }
                if (war.Defenders != null)
                {
                    AddTypeId(war.Defenders.Id);
                }
            }

            foreach (Treaty nonAggression in data.NonAggressions)
            {
                AddTypeId(nonAggression.Id);
            }
            foreach (Treaty peace in data.Peaces)
            {
                AddTypeId(peace.Id);
            }
            foreach (Treaty trade in data.Trades)
            {
                AddTypeId(trade.Id);
            }

            if (data.Weather != null)
            {
                foreach (WeatherPattern pattern in data.Weather.Patterns)
                {
                    AddTypeId(pattern.Id);
                }
            }

            foreach (CountrySettings settings in scenario.Countries)
            {
                AddTypeId(settings.HeadOfState);
                AddTypeId(settings.HeadOfGovernment);
                AddTypeId(settings.ForeignMinister);
                AddTypeId(settings.ArmamentMinister);
                AddTypeId(settings.MinisterOfSecurity);
                AddTypeId(settings.MinisterOfIntelligence);
                AddTypeId(settings.ChiefOfStaff);
                AddTypeId(settings.ChiefOfArmy);
                AddTypeId(settings.ChiefOfNavy);
                AddTypeId(settings.ChiefOfAir);

                foreach (Unit unit in settings.LandUnits)
                {
                    AddTypeId(unit.Id);
                    foreach (Division division in unit.Divisions)
                    {
                        AddTypeId(division.Id);
                    }
                }
                foreach (Unit unit in settings.NavalUnits)
                {
                    AddTypeId(unit.Id);
                    foreach (Division division in unit.Divisions)
                    {
                        AddTypeId(division.Id);
                    }
                    foreach (Unit landUnit in unit.LandUnits)
                    {
                        AddTypeId(landUnit.Id);
                        foreach (Division landDivision in landUnit.Divisions)
                        {
                            AddTypeId(landDivision.Id);
                        }
                    }
                }
                foreach (Unit unit in settings.AirUnits)
                {
                    AddTypeId(unit.Id);
                    foreach (Division division in unit.Divisions)
                    {
                        AddTypeId(division.Id);
                    }
                    foreach (Unit landUnit in unit.LandUnits)
                    {
                        AddTypeId(landUnit.Id);
                        foreach (Division landDivision in landUnit.Divisions)
                        {
                            AddTypeId(landDivision.Id);
                        }
                    }
                }
                foreach (DivisionDevelopment division in settings.DivisionDevelopments)
                {
                    AddTypeId(division.Id);
                }

                foreach (BuildingDevelopment building in settings.BuildingDevelopments)
                {
                    AddTypeId(building.Id);
                }

                foreach (Convoy convoy in settings.Convoys)
                {
                    AddTypeId(convoy.Id);
                    AddTypeId(convoy.TradeId);
                }
                foreach (ConvoyDevelopment convoy in settings.ConvoyDevelopments)
                {
                    AddTypeId(convoy.Id);
                }
            }
        }

        /// <summary>
        ///     type When id id Returns whether a pair of
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="id id">id id</param>
        /// <returns>type When id If there is a pair of true true return it</returns>
        public static bool ExistsTypeId(int type, int id)
        {
            return _usedTypeIds.ContainsKey(type) && _usedTypeIds[type].Contains(id);
        }

        /// <summary>
        ///     type When id id Register a pair of
        /// </summary>
        /// <param name="id">type When id id Pair of</param>
        /// <returns>If registration is successful true true return it</returns>
        public static bool AddTypeId(TypeId id)
        {
            if (id == null)
            {
                return false;
            }

            if (!_usedTypeIds.ContainsKey(id.Type))
            {
                _usedTypeIds.Add(id.Type, new HashSet<int>());
            }

            if (_usedTypeIds[id.Type].Contains(id.Id))
            {
                return false;
            }
            _usedTypeIds[id.Type].Add(id.Id);

            return true;
        }

        /// <summary>
        ///     type When id Delete the pair of
        /// </summary>
        /// <param name="id">type When id id Pair of</param>
        /// <returns>If the deletion is successful true true return it</returns>
        public static bool RemoveTypeId(TypeId id)
        {
            if (id == null)
            {
                return false;
            }

            if (!_usedTypeIds.ContainsKey(id.Type))
            {
                return false;
            }

            if (!_usedTypeIds[id.Type].Contains(id.Id))
            {
                return false;
            }
            _usedTypeIds[id.Type].Remove(id.Id);

            return true;
        }

        /// <summary>
        ///     New type To get
        /// </summary>
        /// <param name="startType">Start exploration type</param>
        /// <returns>New type</returns>
        public static int GetNewType(int startType)
        {
            int type = startType;
            while (!_usedTypeIds.ContainsKey(type))
            {
                type++;
            }
            return type;
        }

        /// <summary>
        ///     New type To get
        /// </summary>
        /// <param name="startType">Start exploration type</param>
        /// <param name="id id">id id</param>
        /// <returns>New type</returns>
        public static int GetNewType(int startType, int id)
        {
            int type = startType;
            while (ExistsTypeId(type, id))
            {
                type++;
            }
            return type;
        }

        /// <summary>
        ///     New id id To get
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="startId">Start exploration id</param>
        /// <returns>New id</returns>
        public static int GetNewId(int type, int startId)
        {
            int id = startId;
            if (!_usedTypeIds.ContainsKey(type))
            {
                return id;
            }
            HashSet<int> ids = _usedTypeIds[type];
            while (ids.Contains(id))
            {
                id++;
            }
            return id;
        }

        /// <summary>
        ///     type To set
        /// </summary>
        /// <param name="typeId">type When id id Pair of</param>
        /// <param name="type">type The value of the</param>
        public static void SetType(TypeId typeId, int type)
        {
            RemoveTypeId(typeId);
            typeId.Type = type;
            AddTypeId(typeId);
        }

        /// <summary>
        ///     id id To set
        /// </summary>
        /// <param name="typeId">type When id id Pair of</param>
        /// <param name="id">id The value of the</param>
        public static void SetId(TypeId typeId, int id)
        {
            RemoveTypeId(typeId);
            typeId.Id = id;
            AddTypeId(typeId);
        }

        /// <summary>
        ///     New id id To get
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="startId">Start exploration id</param>
        /// <returns>New id</returns>
        public static TypeId GetNewTypeId(int type, int startId)
        {
            if (!_usedTypeIds.ContainsKey(type))
            {
                _usedTypeIds.Add(type, new HashSet<int>());
            }
            HashSet<int> ids = _usedTypeIds[type];
            int id = startId;
            while (ids.Contains(id))
            {
                id++;
            }
            _usedTypeIds[type].Add(id);
            return new TypeId { Type = type, Id = id };
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
        ///     Set the edited flag
        /// </summary>
        public static void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        private static void ResetDirtyAll()
        {
            Data.ResetDirtyAll();

            _dirtyFlag = false;
        }

        #endregion
    }
}
