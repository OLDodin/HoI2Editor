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
    ///     Class to hold new unit name
    /// </summary>
    public static class UnitNames
    {
        #region Public properties

        /// <summary>
        ///     Available unit name types
        /// </summary>
        public static UnitNameType[] Types { get; private set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Unit name
        /// </summary>
        private static readonly Dictionary<Country, Dictionary<UnitNameType, List<string>>> Items =
            new Dictionary<Country, Dictionary<UnitNameType, List<string>>>();

        /// <summary>
        ///     Unit name type string and ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, UnitNameType> TypeStringMap = new Dictionary<string, UnitNameType>();

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
        private static readonly bool[] CountryDirtyFlags = new bool[Enum.GetValues(typeof (Country)).Length];

        /// <summary>
        ///     Edited flag for each unit name type
        /// </summary>
        private static readonly bool[,] TypeDirtyFlags =
            new bool[Enum.GetValues(typeof (Country)).Length, Enum.GetValues(typeof (UnitNameType)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     Unit type name
        /// </summary>
        public static readonly string[] TypeNames =
        {
            "NAME_HQ",
            "NAME_INFANTRY",
            "NAME_GARRISON",
            "NAME_CAVALRY",
            "NAME_MOTORIZED",
            "NAME_MECHANIZED",
            "NAME_LIGHT_ARMOR",
            "NAME_ARMOR",
            "NAME_PARATROOPER",
            "NAME_MARINE",
            "NAME_BERGSJAEGER",
            "NAME_MILITIA",
            "NAME_MULTI_ROLE",
            "NAME_INTERCEPTOR",
            "NAME_ROCKET_INTERCEPTOR",
            "NAME_ESCORT",
            "NAME_STRATEGIC_BOMBER",
            "NAME_TACTICAL_BOMBER",
            "NAME_CAS",
            "NAME_NAVAL_BOMBER",
            "NAME_TRANSPORT_PLANE",
            "NAME_FLYING_BOMB",
            "NAME_FLYING_ROCKET",
            "NAME_BATTLESHIP",
            "NAME_BATTLECRUISER",
            "NAME_CARRIER",
            "NAME_ESCORT_CARRIER",
            "NAME_LIGHT_CARRIER",
            "NAME_HEAVY_CRUISER",
            "NAME_LIGHT_CRUISER",
            "NAME_DESTROYER",
            "NAME_SUBMARINE",
            "NAME_NUCLEAR_SUBMARINE",
            "NAME_TRANSPORT",
            "NAME_D_RSV_33",
            "NAME_D_RSV_34",
            "NAME_D_RSV_35",
            "NAME_D_RSV_36",
            "NAME_D_RSV_37",
            "NAME_D_RSV_38",
            "NAME_D_RSV_39",
            "NAME_D_RSV_40",
            "NAME_D_01",
            "NAME_D_02",
            "NAME_D_03",
            "NAME_D_04",
            "NAME_D_05",
            "NAME_D_06",
            "NAME_D_07",
            "NAME_D_08",
            "NAME_D_09",
            "NAME_D_10",
            "NAME_D_11",
            "NAME_D_12",
            "NAME_D_13",
            "NAME_D_14",
            "NAME_D_15",
            "NAME_D_16",
            "NAME_D_17",
            "NAME_D_18",
            "NAME_D_19",
            "NAME_D_20",
            "NAME_D_21",
            "NAME_D_22",
            "NAME_D_23",
            "NAME_D_24",
            "NAME_D_25",
            "NAME_D_26",
            "NAME_D_27",
            "NAME_D_28",
            "NAME_D_29",
            "NAME_D_30",
            "NAME_D_31",
            "NAME_D_32",
            "NAME_D_33",
            "NAME_D_34",
            "NAME_D_35",
            "NAME_D_36",
            "NAME_D_37",
            "NAME_D_38",
            "NAME_D_39",
            "NAME_D_40",
            "NAME_D_41",
            "NAME_D_42",
            "NAME_D_43",
            "NAME_D_44",
            "NAME_D_45",
            "NAME_D_46",
            "NAME_D_47",
            "NAME_D_48",
            "NAME_D_49",
            "NAME_D_50",
            "NAME_D_51",
            "NAME_D_52",
            "NAME_D_53",
            "NAME_D_54",
            "NAME_D_55",
            "NAME_D_56",
            "NAME_D_57",
            "NAME_D_58",
            "NAME_D_59",
            "NAME_D_60",
            "NAME_D_61",
            "NAME_D_62",
            "NAME_D_63",
            "NAME_D_64",
            "NAME_D_65",
            "NAME_D_66",
            "NAME_D_67",
            "NAME_D_68",
            "NAME_D_69",
            "NAME_D_70",
            "NAME_D_71",
            "NAME_D_72",
            "NAME_D_73",
            "NAME_D_74",
            "NAME_D_75",
            "NAME_D_76",
            "NAME_D_77",
            "NAME_D_78",
            "NAME_D_79",
            "NAME_D_80",
            "NAME_D_81",
            "NAME_D_82",
            "NAME_D_83",
            "NAME_D_84",
            "NAME_D_85",
            "NAME_D_86",
            "NAME_D_87",
            "NAME_D_88",
            "NAME_D_89",
            "NAME_D_90",
            "NAME_D_91",
            "NAME_D_92",
            "NAME_D_93",
            "NAME_D_94",
            "NAME_D_95",
            "NAME_D_96",
            "NAME_D_97",
            "NAME_D_98",
            "NAME_D_99"
        };

        #endregion

        #region Internal constant

        /// <summary>
        ///     Unit type string
        /// </summary>
        private static readonly string[] TypeStrings =
        {
            "HQ",
            "Inf",
            "Gar",
            "Cav",
            "Mot",
            "Mec",
            "L ARM",
            "Arm",
            "Par",
            "Mar",
            "Mtn",
            "Mil",
            "Fig",
            "Int F",
            "32",
            "Esc F",
            "Str",
            "Tac",
            "CAS",
            "Nav",
            "Trp",
            "V1",
            "V2",
            "BB",
            "BC",
            "CV",
            "27",
            "31",
            "CA",
            "CL",
            "DD",
            "SS",
            "NS",
            "TP",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "41",
            "42",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49",
            "50",
            "51",
            "52",
            "53",
            "54",
            "55",
            "56",
            "57",
            "58",
            "59",
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "100",
            "101",
            "102",
            "103",
            "104",
            "105",
            "106",
            "107",
            "108",
            "109",
            "110",
            "111",
            "112",
            "113",
            "114",
            "115",
            "116",
            "117",
            "118",
            "119",
            "120",
            "121",
            "122",
            "123",
            "124",
            "125",
            "126",
            "127",
            "128",
            "129",
            "130",
            "131",
            "132",
            "133",
            "134",
            "135",
            "136",
            "137",
            "138",
            "139"
        };

        /// <summary>
        ///     Available unit name types (DDA / AoD / DH1.02)
        /// </summary>
        private static readonly UnitNameType[] TypesHoI2 =
        {
            UnitNameType.Hq,
            UnitNameType.Infantry,
            UnitNameType.Garrison,
            UnitNameType.Cavalry,
            UnitNameType.Motorized,
            UnitNameType.Mechanized,
            UnitNameType.LightArmor,
            UnitNameType.Armor,
            UnitNameType.Paratrooper,
            UnitNameType.Marine,
            UnitNameType.Bergsjaeger,
            UnitNameType.Militia,
            UnitNameType.Fighter,
            UnitNameType.Interceptor,
            UnitNameType.EscortFighter,
            UnitNameType.StrategicBomber,
            UnitNameType.TacticalBomber,
            UnitNameType.Cas,
            UnitNameType.NavalBomber,
            UnitNameType.TransportPlane,
            UnitNameType.FlyingBomb,
            UnitNameType.FlyingRocket,
            UnitNameType.Battleship,
            UnitNameType.BattleCruiser,
            UnitNameType.Carrier,
            UnitNameType.EscortCarrier,
            UnitNameType.HeavyCruiser,
            UnitNameType.LightCruiser,
            UnitNameType.Destroyer,
            UnitNameType.Submarine,
            UnitNameType.NuclearSubmarine,
            UnitNameType.Transport
        };

        /// <summary>
        ///     Available unit name types (DH1.03)
        /// </summary>
        private static readonly UnitNameType[] TypesDh103 =
        {
            UnitNameType.Hq,
            UnitNameType.Infantry,
            UnitNameType.Garrison,
            UnitNameType.Cavalry,
            UnitNameType.Motorized,
            UnitNameType.Mechanized,
            UnitNameType.LightArmor,
            UnitNameType.Armor,
            UnitNameType.Paratrooper,
            UnitNameType.Marine,
            UnitNameType.Bergsjaeger,
            UnitNameType.Militia,
            UnitNameType.Fighter,
            UnitNameType.Interceptor,
            UnitNameType.RocketInterceptor,
            UnitNameType.EscortFighter,
            UnitNameType.StrategicBomber,
            UnitNameType.TacticalBomber,
            UnitNameType.Cas,
            UnitNameType.NavalBomber,
            UnitNameType.TransportPlane,
            UnitNameType.FlyingBomb,
            UnitNameType.FlyingRocket,
            UnitNameType.Battleship,
            UnitNameType.BattleCruiser,
            UnitNameType.Carrier,
            UnitNameType.EscortCarrier,
            UnitNameType.LightCarrier,
            UnitNameType.HeavyCruiser,
            UnitNameType.LightCruiser,
            UnitNameType.Destroyer,
            UnitNameType.Submarine,
            UnitNameType.NuclearSubmarine,
            UnitNameType.Transport,
            UnitNameType.ReserveDivision33,
            UnitNameType.ReserveDivision34,
            UnitNameType.ReserveDivision35,
            UnitNameType.ReserveDivision36,
            UnitNameType.ReserveDivision37,
            UnitNameType.ReserveDivision38,
            UnitNameType.ReserveDivision39,
            UnitNameType.ReserveDivision40,
            UnitNameType.Division01,
            UnitNameType.Division02,
            UnitNameType.Division03,
            UnitNameType.Division04,
            UnitNameType.Division05,
            UnitNameType.Division06,
            UnitNameType.Division07,
            UnitNameType.Division08,
            UnitNameType.Division09,
            UnitNameType.Division10,
            UnitNameType.Division11,
            UnitNameType.Division12,
            UnitNameType.Division13,
            UnitNameType.Division14,
            UnitNameType.Division15,
            UnitNameType.Division16,
            UnitNameType.Division17,
            UnitNameType.Division18,
            UnitNameType.Division19,
            UnitNameType.Division20,
            UnitNameType.Division21,
            UnitNameType.Division22,
            UnitNameType.Division23,
            UnitNameType.Division24,
            UnitNameType.Division25,
            UnitNameType.Division26,
            UnitNameType.Division27,
            UnitNameType.Division28,
            UnitNameType.Division29,
            UnitNameType.Division30,
            UnitNameType.Division31,
            UnitNameType.Division32,
            UnitNameType.Division33,
            UnitNameType.Division34,
            UnitNameType.Division35,
            UnitNameType.Division36,
            UnitNameType.Division37,
            UnitNameType.Division38,
            UnitNameType.Division39,
            UnitNameType.Division40,
            UnitNameType.Division41,
            UnitNameType.Division42,
            UnitNameType.Division43,
            UnitNameType.Division44,
            UnitNameType.Division45,
            UnitNameType.Division46,
            UnitNameType.Division47,
            UnitNameType.Division48,
            UnitNameType.Division49,
            UnitNameType.Division50,
            UnitNameType.Division51,
            UnitNameType.Division52,
            UnitNameType.Division53,
            UnitNameType.Division54,
            UnitNameType.Division55,
            UnitNameType.Division56,
            UnitNameType.Division57,
            UnitNameType.Division58,
            UnitNameType.Division59,
            UnitNameType.Division60,
            UnitNameType.Division61,
            UnitNameType.Division62,
            UnitNameType.Division63,
            UnitNameType.Division64,
            UnitNameType.Division65,
            UnitNameType.Division66,
            UnitNameType.Division67,
            UnitNameType.Division68,
            UnitNameType.Division69,
            UnitNameType.Division70,
            UnitNameType.Division71,
            UnitNameType.Division72,
            UnitNameType.Division73,
            UnitNameType.Division74,
            UnitNameType.Division75,
            UnitNameType.Division76,
            UnitNameType.Division77,
            UnitNameType.Division78,
            UnitNameType.Division79,
            UnitNameType.Division80,
            UnitNameType.Division81,
            UnitNameType.Division82,
            UnitNameType.Division83,
            UnitNameType.Division84,
            UnitNameType.Division85,
            UnitNameType.Division86,
            UnitNameType.Division87,
            UnitNameType.Division88,
            UnitNameType.Division89,
            UnitNameType.Division90,
            UnitNameType.Division91,
            UnitNameType.Division92,
            UnitNameType.Division93,
            UnitNameType.Division94,
            UnitNameType.Division95,
            UnitNameType.Division96,
            UnitNameType.Division97,
            UnitNameType.Division98,
            UnitNameType.Division99
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     Static constructor
        /// </summary>
        static UnitNames()
        {
            // Unit name type
            foreach (UnitNameType type in Enum.GetValues(typeof (UnitNameType)))
            {
                TypeStringMap.Add(TypeStrings[(int) type].ToUpper(), type);
            }
        }

        /// <summary>
        ///     Initialize unit name data
        /// </summary>
        public static void Init()
        {
            // Available unit name types
            if (Game.Type == GameType.DarkestHour && Game.Version >= 103)
            {
                Types = TypesDh103;
            }
            else
            {
                Types = TypesHoI2;
            }
        }

        #endregion

        #region File reading

        /// <summary>
        ///     Request to reload the unit name definition file
        /// </summary>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     Reload the unit name definition files
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
        ///     Read the unit name definition file
        /// </summary>
        public static void Load()
        {
            // Back if loaded
            if (_loaded)
            {
                return;
            }

            Items.Clear();

            // Return if the unit name definition file does not exist
            string fileName = Game.GetReadFileName(Game.UnitNamesPathName);
            if (!File.Exists(fileName))
            {
                return;
            }

            // Read the unit name definition file
            try
            {
                LoadFile(fileName);
            }
            catch (Exception)
            {
                Log.Error("[UnitName] Read error: {0}", fileName);
                MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                    Resources.EditorUnitName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Clear all edited flags
            ResetDirtyAll();

            // Set the read flag
            _loaded = true;
        }

        /// <summary>
        ///     Read the unit name definition file
        /// </summary>
        /// <param name="fileName">file name</param>
        private static void LoadFile(string fileName)
        {
            Log.Verbose("[UnitName] Load: {0}", Path.GetFileName(fileName));

            using (CsvLexer lexer = new CsvLexer(fileName))
            {
                while (!lexer.EndOfStream)
                {
                    ParseLine(lexer);
                }
            }
        }

        /// <summary>
        ///     Interpret the unit name definition line
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        private static void ParseLine(CsvLexer lexer)
        {
            string[] tokens = lexer.GetTokens();

            // Skip blank lines
            if (tokens == null)
            {
                return;
            }

            // Skip lines with insufficient tokens
            if (tokens.Length != 3)
            {
                Log.Warning("[UnitName] Invalid token count: {0} ({1} L{2})", tokens.Length, lexer.FileName,
                    lexer.LineNo);
                // Continue analysis if there are extra items
                if (tokens.Length < 3)
                {
                    return;
                }
            }

            // Country tag
            string countryName = tokens[0].ToUpper();
            if (!Countries.StringMap.ContainsKey(countryName))
            {
                Log.Warning("[UnitName] Invalid country: {0} ({1} L{2})", tokens[0], lexer.FileName, lexer.LineNo);
                return;
            }
            Country country = Countries.StringMap[countryName];

            // Unit type
            string typeName = tokens[1].ToUpper();
            if (!TypeStringMap.ContainsKey(typeName))
            {
                Log.Warning("[UnitName] Invalid unit type: {0} ({1} L{2})", tokens[1], lexer.FileName, lexer.LineNo);
                return;
            }
            UnitNameType type = TypeStringMap[typeName];
            if (!Types.Contains(type))
            {
                Log.Warning("[UnitName] Invalid unit type: {0} ({1} L{2})", tokens[1], lexer.FileName, lexer.LineNo);
                return;
            }

            // Unit name
            string name = tokens[2];
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            // Add a unit name
            AddName(name, country, type);
        }

        #endregion

        #region File writing

        /// <summary>
        ///     Save the unit name definition file
        /// </summary>
        /// <returns>If saving fails false false return it</returns>
        public static bool Save()
        {
            // Do nothing if not edited
            if (!IsDirty())
            {
                return true;
            }

            string fileName = Game.GetWriteFileName(Game.UnitNamesPathName);
            try
            {
                // db db. If there is no folder, create it
                string folderName = Game.GetWriteFileName(Game.DatabasePathName);
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                // Save the unit name definition file
                SaveFile(fileName);
            }
            catch (Exception)
            {
                Log.Error("[UnitName] Write error: {0}", fileName);
                MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                    Resources.EditorUnitName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Clear all edited flags
            ResetDirtyAll();

            return true;
        }

        /// <summary>
        ///     Save the unit name definition file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void SaveFile(string fileName)
        {
            Log.Info("[UnitName] Save: {0}", Path.GetFileName(fileName));

            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                foreach (Country country in Items.Select(pair => pair.Key)
                    .Where(country => ExistsCountry(country) && Items[country].Count > 0))
                {
                    Country c = country;
                    foreach (UnitNameType type in Items[country]
                        .Select(pair => pair.Key).Where(type => ExistsType(c, type) && Items[c][type].Count > 0))
                    {
                        foreach (string name in Items[country][type])
                        {
                            writer.WriteLine("{0};{1};{2}", Countries.Strings[(int) country], TypeStrings[(int) type],
                                name);
                        }
                    }
                }
            }
        }

        #endregion

        #region Unit name operation

        /// <summary>
        ///     Get the unit name list
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        /// <returns>Unit name list</returns>
        public static IEnumerable<string> GetNames(Country country, UnitNameType type)
        {
            // Returns an empty list if unregistered
            if (!ExistsType(country, type))
            {
                return new List<string>();
            }

            return Items[country][type];
        }

        /// <summary>
        ///     Add a unit name
        /// </summary>
        /// <param name="name">Unit name</param>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        private static void AddName(string name, Country country, UnitNameType type)
        {
            // Create an item if not registered
            if (!ExistsCountry(country))
            {
                Items.Add(country, new Dictionary<UnitNameType, List<string>>());
            }
            if (!ExistsType(country, type))
            {
                Items[country].Add(type, new List<string>());
            }

            // Add a unit name
            Items[country][type].Add(name);
        }

        /// <summary>
        ///     Set the unit name list
        /// </summary>
        /// <param name="names">Unit name list</param>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        public static void SetNames(List<string> names, Country country, UnitNameType type)
        {
            // Create an item if not registered
            if (!ExistsCountry(country))
            {
                Items.Add(country, new Dictionary<UnitNameType, List<string>>());
            }
            if (!ExistsType(country, type))
            {
                Items[country].Add(type, new List<string>());
            }

            // Return if there is no change in the unit name list
            if (names.SequenceEqual(Items[country][type]))
            {
                return;
            }

            Log.Info("[UnitName] Set: [{0}] <{1}>", Config.GetText(TypeNames[(int) type]),
                Countries.Strings[(int) country]);

            // Set the unit name list
            Items[country][type] = names;

            // Set the edited flag
            SetDirty(country, type);
        }

        /// <summary>
        ///     Replace unit name
        /// </summary>
        /// <param name="s">Substitution source string</param>
        /// <param name="t">Replacement destination character string</param>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        /// <param name="regex">Whether to use regular expressions</param>
        public static void Replace(string s, string t, Country country, UnitNameType type, bool regex)
        {
            List<string> names =
                Items[country][type].Select(name => regex ? Regex.Replace(name, s, t) : name.Replace(s, t)).ToList();
            SetNames(names, country, type);
        }

        /// <summary>
        ///     Replace all unit names
        /// </summary>
        /// <param name="s">Substitution source string</param>
        /// <param name="t">Replacement destination character string</param>
        /// <param name="regex">Whether to use regular expressions</param>
        public static void ReplaceAll(string s, string t, bool regex)
        {
            List<KeyValuePair<Country, UnitNameType>> pairs =
                (from country in Items.Select(pair => pair.Key)
                    from type in Items[country].Select(pair => pair.Key)
                    select new KeyValuePair<Country, UnitNameType>(country, type)).ToList();
            foreach (KeyValuePair<Country, UnitNameType> pair in pairs)
            {
                Replace(s, t, pair.Key, pair.Value, regex);
            }
        }

        /// <summary>
        ///     Replace unit names in all countries
        /// </summary>
        /// <param name="s">Substitution source string</param>
        /// <param name="t">Replacement destination character string</param>
        /// <param name="type">Unit name type</param>
        /// <param name="regex">Whether to use regular expressions</param>
        public static void ReplaceAllCountries(string s, string t, UnitNameType type, bool regex)
        {
            List<Country> countries =
                Items.Select(pair => pair.Key).Where(country => Items[country].ContainsKey(type)).ToList();
            foreach (Country country in countries)
            {
                Replace(s, t, country, type, regex);
            }
        }

        /// <summary>
        ///     Replace unit names for all unit name types
        /// </summary>
        /// <param name="s">Substitution source string</param>
        /// <param name="t">Replacement destination character string</param>
        /// <param name="country">Country tag</param>
        /// <param name="regex">Whether to use regular expressions</param>
        public static void ReplaceAllTypes(string s, string t, Country country, bool regex)
        {
            List<UnitNameType> types = new List<UnitNameType>();
            if (Items.ContainsKey(country))
            {
                types.AddRange(Items[country].Select(pair => pair.Key));
            }
            foreach (UnitNameType type in types)
            {
                Replace(s, t, country, type, regex);
            }
        }

        /// <summary>
        ///     Add unit names serially
        /// </summary>
        /// <param name="prefix">prefix</param>
        /// <param name="suffix">Suffix</param>
        /// <param name="start">Starting number</param>
        /// <param name="end">End number</param>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        public static void AddSequential(string prefix, string suffix, int start, int end, Country country,
            UnitNameType type)
        {
            for (int i = start; i <= end; i++)
            {
                string name = $"{prefix}{i}{suffix}";
                if (!Exists(name, country, type))
                {
                    AddName(name, country, type);
                    SetDirty(country, type);
                }
            }
        }

        /// <summary>
        ///     Sequential number interpolation of unit names
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        public static void Interpolate(Country country, UnitNameType type)
        {
            List<string> names = new List<string>();
            Regex r = new Regex("([^\\d]*)(\\d+)(.*)");
            string pattern = "";
            int prev = 0;
            bool found = false;
            foreach (string name in Items[country][type])
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

            SetNames(names, country, type);
        }

        /// <summary>
        ///     Interpolate all unit names sequentially
        /// </summary>
        public static void InterpolateAll()
        {
            List<KeyValuePair<Country, UnitNameType>> pairs =
                (from country in Items.Select(pair => pair.Key)
                    from type in Items[country].Select(pair => pair.Key)
                    select new KeyValuePair<Country, UnitNameType>(country, type)).ToList();
            foreach (KeyValuePair<Country, UnitNameType> pair in pairs)
            {
                Interpolate(pair.Key, pair.Value);
            }
        }

        /// <summary>
        ///     Serial number interpolation for unit names in all countries
        /// </summary>
        /// <param name="type">Unit name type</param>
        public static void InterpolateAllCountries(UnitNameType type)
        {
            List<Country> countries =
                Items.Select(pair => pair.Key).Where(country => Items[country].ContainsKey(type)).ToList();
            foreach (Country country in countries)
            {
                Interpolate(country, type);
            }
        }

        /// <summary>
        ///     Sequential number interpolation of unit names of all unit name types
        /// </summary>
        /// <param name="country">Country tag</param>
        public static void InterpolateAllTypes(Country country)
        {
            List<UnitNameType> types = new List<UnitNameType>();
            if (Items.ContainsKey(country))
            {
                types.AddRange(Items[country].Select(pair => pair.Key));
            }
            foreach (UnitNameType type in types)
            {
                Interpolate(country, type);
            }
        }

        /// <summary>
        ///     Returns whether the item exists
        /// </summary>
        /// <param name="name">item name</param>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        /// <returns>If the item exists true true return it</returns>
        private static bool Exists(string name, Country country, UnitNameType type)
        {
            if (!ExistsType(country, type))
            {
                return false;
            }

            return Items[country][type].Contains(name);
        }

        /// <summary>
        ///     Returns whether the item in the specified country exists
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <returns>If the item exists true true return it</returns>
        private static bool ExistsCountry(Country country)
        {
            return Items.ContainsKey(country);
        }

        /// <summary>
        ///     Returns whether an item of the specified unit type exists
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        /// <returns>If the item exists true true return it</returns>
        private static bool ExistsType(Country country, UnitNameType type)
        {
            if (!ExistsCountry(country))
            {
                return false;
            }

            return Items[country].ContainsKey(type);
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
            return CountryDirtyFlags[(int) country];
        }

        /// <summary>
        ///     Get if it has been edited
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirty(Country country, UnitNameType type)
        {
            return TypeDirtyFlags[(int) country, (int) type];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        private static void SetDirty(Country country, UnitNameType type)
        {
            TypeDirtyFlags[(int) country, (int) type] = true;
            CountryDirtyFlags[(int) country] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        private static void ResetDirtyAll()
        {
            foreach (Country country in Enum.GetValues(typeof (Country)))
            {
                foreach (UnitNameType type in Enum.GetValues(typeof (UnitNameType)))
                {
                    TypeDirtyFlags[(int) country, (int) type] = false;
                }
                CountryDirtyFlags[(int) country] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Unit name type
    /// </summary>
    public enum UnitNameType
    {
        Hq, // Headquarters
        Infantry, // infantry
        Garrison, // Defensive division
        Cavalry, // cavalry
        Motorized, // Motorized infantry
        Mechanized, // Mechanized infantry
        LightArmor, // Light tank
        Armor, // tank
        Paratrooper, // Airborne soldiers
        Marine, // Marines
        Bergsjaeger, // Mountain soldier
        Militia, // militia
        Fighter, // Fighter
        Interceptor, // Interceptor
        RocketInterceptor, // Rocket interceptor
        EscortFighter, // Escort fighter
        StrategicBomber, // Strategic bomber
        TacticalBomber, // Tactical bomber
        Cas, // Close air support
        NavalBomber, // Navy bomber
        TransportPlane, // Transport machine
        FlyingBomb, // Flying bomb
        FlyingRocket, // Strategic rocket
        Battleship, // Battleship
        BattleCruiser, // Cruise battleship
        Carrier, // aircraft carrier
        EscortCarrier, // Escort carrier
        LightCarrier, // Light aircraft carrier
        HeavyCruiser, // Heavy cruiser
        LightCruiser, // Light cruiser
        Destroyer, // Destroyer
        Submarine, // submarine
        NuclearSubmarine, // Nuclear submarine
        Transport, // Transport ship
        ReserveDivision33, // Reserve Division 33 33
        ReserveDivision34, // Reserve Division 34
        ReserveDivision35, // Reserve Division 35
        ReserveDivision36, // Reserve division 36
        ReserveDivision37, // Reserve division 37 37
        ReserveDivision38, // Reserve division 38
        ReserveDivision39, // Reserve division 39 39
        ReserveDivision40, // Reserve Division 40
        Division01, // User-defined division 01 01
        Division02, // User-defined division 02 02
        Division03, // User-defined division 03 03
        Division04, // User-defined division 04 04
        Division05, // User-defined division 05 05
        Division06, // User-defined division 06 06
        Division07, // User-defined division 07 07
        Division08, // User-defined division 08 08
        Division09, // User-defined division 09 09
        Division10, // User-defined division Ten
        Division11, // User-defined division 11 11
        Division12, // User-defined division 12
        Division13, // User-defined division 13
        Division14, // User-defined division 14
        Division15, // User-defined division 15
        Division16, // User-defined division 16 16
        Division17, // User-defined division 17 17
        Division18, // User-defined division 18 18
        Division19, // User-defined division 19 19
        Division20, // User-defined division 20
        Division21, // User-defined division twenty one
        Division22, // User-defined division twenty two
        Division23, // User-defined division twenty three
        Division24, // User-defined division twenty four
        Division25, // User-defined division twenty five
        Division26, // User-defined division 26
        Division27, // User-defined division 27
        Division28, // User-defined division 28 28
        Division29, // User-defined division 29
        Division30, // User-defined division 30
        Division31, // User-defined division 31
        Division32, // User-defined division 32
        Division33, // User-defined division 33 33
        Division34, // User-defined division 34
        Division35, // User-defined division 35
        Division36, // User-defined division 36
        Division37, // User-defined division 37 37
        Division38, // User-defined division 38
        Division39, // User-defined division 39 39
        Division40, // User-defined division 40
        Division41, // User-defined division 41 41
        Division42, // User-defined division 42
        Division43, // User-defined division 43
        Division44, // User-defined division 44
        Division45, // User-defined division 45 45
        Division46, // User-defined division 46
        Division47, // User-defined division 47 47
        Division48, // User-defined division 48
        Division49, // User-defined division 49
        Division50, // User-defined division 50
        Division51, // User-defined division 51
        Division52, // User-defined division 52 52
        Division53, // User-defined division 53
        Division54, // User-defined division 54
        Division55, // User-defined division 55 55
        Division56, // User-defined division 56
        Division57, // User-defined division 57 57
        Division58, // User-defined division 58
        Division59, // User-defined division 59
        Division60, // User-defined division 60
        Division61, // User-defined division 61
        Division62, // User-defined division 62
        Division63, // User-defined division 63 63
        Division64, // User-defined division 64
        Division65, // User-defined division 65 65
        Division66, // User-defined division 66 66
        Division67, // User-defined division 67 67
        Division68, // User-defined division 68 68
        Division69, // User-defined division 69
        Division70, // User-defined division 70
        Division71, // User-defined division 71 71
        Division72, // User-defined division 72
        Division73, // User-defined division 73
        Division74, // User-defined division 74 74
        Division75, // User-defined division 75
        Division76, // User-defined division 76 76
        Division77, // User-defined division 77 77
        Division78, // User-defined division 78 78
        Division79, // User-defined division 79 79
        Division80, // User-defined division 80
        Division81, // User-defined division 81
        Division82, // User-defined division 82
        Division83, // User-defined division 83
        Division84, // User-defined division 84 84
        Division85, // User-defined division 85
        Division86, // User-defined division 86
        Division87, // User-defined division 87
        Division88, // User-defined division 88
        Division89, // User-defined division 89
        Division90, // User-defined division 90
        Division91, // User-defined division 91
        Division92, // User-defined division 92
        Division93, // User-defined division 93
        Division94, // User-defined division 94
        Division95, // User-defined division 95
        Division96, // User-defined division 96
        Division97, // User-defined division 97
        Division98, // User-defined division 98
        Division99 // User-defined division 99
    }
}
