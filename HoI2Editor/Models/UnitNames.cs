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
    ///     新規ユニット名を保持するクラス
    /// </summary>
    public static class UnitNames
    {
        #region 公開プロパティ

        /// <summary>
        ///     利用可能なユニット名種類
        /// </summary>
        public static UnitNameType[] Types { get; private set; }

        #endregion

        #region 内部フィールド

        /// <summary>
        ///     ユニット名
        /// </summary>
        private static readonly Dictionary<Country, Dictionary<UnitNameType, List<string>>> Items =
            new Dictionary<Country, Dictionary<UnitNameType, List<string>>>();

        /// <summary>
        ///     ユニット名種類文字列とIDの対応付け
        /// </summary>
        private static readonly Dictionary<string, UnitNameType> TypeStringMap = new Dictionary<string, UnitNameType>();

        /// <summary>
        ///     読み込み済みフラグ
        /// </summary>
        private static bool _loaded;

        /// <summary>
        ///     編集済みフラグ
        /// </summary>
        private static bool _dirtyFlag;

        /// <summary>
        ///     国家ごとの編集済みフラグ
        /// </summary>
        private static readonly bool[] CountryDirtyFlags = new bool[Enum.GetValues(typeof (Country)).Length];

        /// <summary>
        ///     ユニット名種類ごとの編集済みフラグ
        /// </summary>
        private static readonly bool[,] TypeDirtyFlags =
            new bool[Enum.GetValues(typeof (Country)).Length, Enum.GetValues(typeof (UnitNameType)).Length];

        #endregion

        #region 公開定数

        /// <summary>
        ///     ユニット種類名
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

        #region 内部定数

        /// <summary>
        ///     ユニット種類文字列
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
        ///     利用可能なユニット名種類 (DDA/AoD/DH1.02)
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
        ///     利用可能なユニット名種類 (DH1.03)
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

        #region 初期化

        /// <summary>
        ///     静的コンストラクタ
        /// </summary>
        static UnitNames()
        {
            // ユニット名種類
            foreach (UnitNameType type in Enum.GetValues(typeof (UnitNameType)))
            {
                TypeStringMap.Add(TypeStrings[(int) type].ToUpper(), type);
            }
        }

        /// <summary>
        ///     ユニット名データを初期化する
        /// </summary>
        public static void Init()
        {
            // 利用可能なユニット名種類
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

        #region ファイル読み込み

        /// <summary>
        ///     ユニット名定義ファイルの再読み込みを要求する
        /// </summary>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     ユニット名定義ファイル群を再読み込みする
        /// </summary>
        public static void Reload()
        {
            // 読み込み前なら何もしない
            if (!_loaded)
            {
                return;
            }

            _loaded = false;

            Load();
        }

        /// <summary>
        ///     ユニット名定義ファイルを読み込む
        /// </summary>
        public static void Load()
        {
            // 読み込み済みならば戻る
            if (_loaded)
            {
                return;
            }

            Items.Clear();

            // ユニット名定義ファイルが存在しなければ戻る
            string fileName = Game.GetReadFileName(Game.UnitNamesPathName);
            if (!File.Exists(fileName))
            {
                return;
            }

            // ユニット名定義ファイルを読み込む
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

            // 編集済みフラグを全て解除する
            ResetDirtyAll();

            // 読み込み済みフラグを設定する
            _loaded = true;
        }

        /// <summary>
        ///     ユニット名定義ファイルを読み込む
        /// </summary>
        /// <param name="fileName">ファイル名</param>
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
        ///     ユニット名定義行を解釈する
        /// </summary>
        /// <param name="lexer">字句解析器</param>
        private static void ParseLine(CsvLexer lexer)
        {
            string[] tokens = lexer.GetTokens();

            // 空行を読み飛ばす
            if (tokens == null)
            {
                return;
            }

            // トークン数が足りない行は読み飛ばす
            if (tokens.Length != 3)
            {
                Log.Warning("[UnitName] Invalid token count: {0} ({1} L{2})", tokens.Length, lexer.FileName,
                    lexer.LineNo);
                // 余分な項目がある場合は解析を続ける
                if (tokens.Length < 3)
                {
                    return;
                }
            }

            // 国タグ
            string countryName = tokens[0].ToUpper();
            if (!Countries.StringMap.ContainsKey(countryName))
            {
                Log.Warning("[UnitName] Invalid country: {0} ({1} L{2})", tokens[0], lexer.FileName, lexer.LineNo);
                return;
            }
            Country country = Countries.StringMap[countryName];

            // ユニット種類
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

            // ユニット名
            string name = tokens[2];
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            // ユニット名を追加する
            AddName(name, country, type);
        }

        #endregion

        #region ファイル書き込み

        /// <summary>
        ///     ユニット名定義ファイルを保存する
        /// </summary>
        /// <returns>保存に失敗すればfalseを返す</returns>
        public static bool Save()
        {
            // 編集済みでなければ何もしない
            if (!IsDirty())
            {
                return true;
            }

            string fileName = Game.GetWriteFileName(Game.UnitNamesPathName);
            try
            {
                // dbフォルダがなければ作成する
                string folderName = Game.GetWriteFileName(Game.DatabasePathName);
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                // ユニット名定義ファイルを保存する
                SaveFile(fileName);
            }
            catch (Exception)
            {
                Log.Error("[UnitName] Write error: {0}", fileName);
                MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                    Resources.EditorUnitName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // 編集済みフラグを全て解除する
            ResetDirtyAll();

            return true;
        }

        /// <summary>
        ///     ユニット名定義ファイルを保存する
        /// </summary>
        /// <param name="fileName">対象ファイル名</param>
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

        #region ユニット名操作

        /// <summary>
        ///     ユニット名リストを取得する
        /// </summary>
        /// <param name="country">国タグ</param>
        /// <param name="type">ユニット名の種類</param>
        /// <returns>ユニット名リスト</returns>
        public static IEnumerable<string> GetNames(Country country, UnitNameType type)
        {
            // 未登録の場合は空のリストを返す
            if (!ExistsType(country, type))
            {
                return new List<string>();
            }

            return Items[country][type];
        }

        /// <summary>
        ///     ユニット名を追加する
        /// </summary>
        /// <param name="name">ユニット名</param>
        /// <param name="country">国タグ</param>
        /// <param name="type">ユニット名の種類</param>
        private static void AddName(string name, Country country, UnitNameType type)
        {
            // 未登録の場合は項目を作成する
            if (!ExistsCountry(country))
            {
                Items.Add(country, new Dictionary<UnitNameType, List<string>>());
            }
            if (!ExistsType(country, type))
            {
                Items[country].Add(type, new List<string>());
            }

            // ユニット名を追加する
            Items[country][type].Add(name);
        }

        /// <summary>
        ///     ユニット名リストを設定する
        /// </summary>
        /// <param name="names">ユニット名リスト</param>
        /// <param name="country">国タグ</param>
        /// <param name="type">ユニット名の種類</param>
        public static void SetNames(List<string> names, Country country, UnitNameType type)
        {
            // 未登録の場合は項目を作成する
            if (!ExistsCountry(country))
            {
                Items.Add(country, new Dictionary<UnitNameType, List<string>>());
            }
            if (!ExistsType(country, type))
            {
                Items[country].Add(type, new List<string>());
            }

            // ユニット名リストに変更がなければ戻る
            if (names.SequenceEqual(Items[country][type]))
            {
                return;
            }

            Log.Info("[UnitName] Set: [{0}] <{1}>", Config.GetText(TypeNames[(int) type]),
                Countries.Strings[(int) country]);

            // ユニット名リストを設定する
            Items[country][type] = names;

            // 編集済みフラグを設定する
            SetDirty(country, type);
        }

        /// <summary>
        ///     ユニット名を置換する
        /// </summary>
        /// <param name="s">置換元文字列</param>
        /// <param name="t">置換先文字列</param>
        /// <param name="country">国タグ</param>
        /// <param name="type">ユニット名種類</param>
        /// <param name="regex">正規表現を使用するか</param>
        public static void Replace(string s, string t, Country country, UnitNameType type, bool regex)
        {
            List<string> names =
                Items[country][type].Select(name => regex ? Regex.Replace(name, s, t) : name.Replace(s, t)).ToList();
            SetNames(names, country, type);
        }

        /// <summary>
        ///     全てのユニット名を置換する
        /// </summary>
        /// <param name="s">置換元文字列</param>
        /// <param name="t">置換先文字列</param>
        /// <param name="regex">正規表現を使用するか</param>
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
        ///     全ての国のユニット名を置換する
        /// </summary>
        /// <param name="s">置換元文字列</param>
        /// <param name="t">置換先文字列</param>
        /// <param name="type">ユニット名種類</param>
        /// <param name="regex">正規表現を使用するか</param>
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
        ///     全てのユニット名種類のユニット名を置換する
        /// </summary>
        /// <param name="s">置換元文字列</param>
        /// <param name="t">置換先文字列</param>
        /// <param name="country">国タグ</param>
        /// <param name="regex">正規表現を使用するか</param>
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
        ///     ユニット名を連番追加する
        /// </summary>
        /// <param name="prefix">接頭辞</param>
        /// <param name="suffix">接尾辞</param>
        /// <param name="start">開始番号</param>
        /// <param name="end">終了番号</param>
        /// <param name="country">国タグ</param>
        /// <param name="type">ユニット名種類</param>
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
        ///     ユニット名を連番補間する
        /// </summary>
        /// <param name="country">国タグ</param>
        /// <param name="type">ユニット名種類</param>
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
                            // 出力パターンを設定する
                            pattern = r.Replace(name, "$1{0}$3");
                            found = true;
                        }
                        else
                        {
                            // 前の番号と現在の番号の間を補間する
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
        ///     全てのユニット名を連番補間する
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
        ///     全ての国のユニット名を連番補間する
        /// </summary>
        /// <param name="type">ユニット名種類</param>
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
        ///     全てのユニット名種類のユニット名を連番補間する
        /// </summary>
        /// <param name="country">国タグ</param>
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
        ///     項目が存在するかを返す
        /// </summary>
        /// <param name="name">項目名</param>
        /// <param name="country">国タグ</param>
        /// <param name="type">ユニット名の種類</param>
        /// <returns>項目が存在すればtrueを返す</returns>
        private static bool Exists(string name, Country country, UnitNameType type)
        {
            if (!ExistsType(country, type))
            {
                return false;
            }

            return Items[country][type].Contains(name);
        }

        /// <summary>
        ///     指定した国の項目が存在するかを返す
        /// </summary>
        /// <param name="country">国タグ</param>
        /// <returns>項目が存在すればtrueを返す</returns>
        private static bool ExistsCountry(Country country)
        {
            return Items.ContainsKey(country);
        }

        /// <summary>
        ///     指定したユニット種類の項目が存在するかを返す
        /// </summary>
        /// <param name="country">国タグ</param>
        /// <param name="type">ユニット名の種類</param>
        /// <returns>項目が存在すればtrueを返す</returns>
        private static bool ExistsType(Country country, UnitNameType type)
        {
            if (!ExistsCountry(country))
            {
                return false;
            }

            return Items[country].ContainsKey(type);
        }

        #endregion

        #region 編集済みフラグ操作

        /// <summary>
        ///     編集済みかどうかを取得する
        /// </summary>
        /// <returns>編集済みならばtrueを返す</returns>
        public static bool IsDirty()
        {
            return _dirtyFlag;
        }

        /// <summary>
        ///     編集済みかどうかを取得する
        /// </summary>
        /// <param name="country">国タグ</param>
        /// <returns>編集済みならばtrueを返す</returns>
        public static bool IsDirty(Country country)
        {
            return CountryDirtyFlags[(int) country];
        }

        /// <summary>
        ///     編集済みかどうかを取得する
        /// </summary>
        /// <param name="country">国タグ</param>
        /// <param name="type">ユニット名種類</param>
        /// <returns>編集済みならばtrueを返す</returns>
        public static bool IsDirty(Country country, UnitNameType type)
        {
            return TypeDirtyFlags[(int) country, (int) type];
        }

        /// <summary>
        ///     編集済みフラグを設定する
        /// </summary>
        /// <param name="country">国タグ</param>
        /// <param name="type">ユニット名種類</param>
        private static void SetDirty(Country country, UnitNameType type)
        {
            TypeDirtyFlags[(int) country, (int) type] = true;
            CountryDirtyFlags[(int) country] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     編集済みフラグを全て解除する
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
    ///     ユニット名種類
    /// </summary>
    public enum UnitNameType
    {
        Hq, // 司令部
        Infantry, // 歩兵
        Garrison, // 守備師団
        Cavalry, // 騎兵
        Motorized, // 自動車化歩兵
        Mechanized, // 機械化歩兵
        LightArmor, // 軽戦車
        Armor, // 戦車
        Paratrooper, // 空挺兵
        Marine, // 海兵
        Bergsjaeger, // 山岳兵
        Militia, // 民兵
        Fighter, // 戦闘機
        Interceptor, // 迎撃機
        RocketInterceptor, // ロケット迎撃機
        EscortFighter, // 護衛戦闘機
        StrategicBomber, // 戦略爆撃機
        TacticalBomber, // 戦術爆撃機
        Cas, // 近接航空支援機
        NavalBomber, // 海軍爆撃機
        TransportPlane, // 輸送機
        FlyingBomb, // 飛行爆弾
        FlyingRocket, // 戦略ロケット
        Battleship, // 戦艦
        BattleCruiser, // 巡洋戦艦
        Carrier, // 空母
        EscortCarrier, // 護衛空母
        LightCarrier, // 軽空母
        HeavyCruiser, // 重巡洋艦
        LightCruiser, // 軽巡洋艦
        Destroyer, // 駆逐艦
        Submarine, // 潜水艦
        NuclearSubmarine, // 原子力潜水艦
        Transport, // 輸送艦
        ReserveDivision33, // 予備師団33
        ReserveDivision34, // 予備師団34
        ReserveDivision35, // 予備師団35
        ReserveDivision36, // 予備師団36
        ReserveDivision37, // 予備師団37
        ReserveDivision38, // 予備師団38
        ReserveDivision39, // 予備師団39
        ReserveDivision40, // 予備師団40
        Division01, // ユーザー定義師団01
        Division02, // ユーザー定義師団02
        Division03, // ユーザー定義師団03
        Division04, // ユーザー定義師団04
        Division05, // ユーザー定義師団05
        Division06, // ユーザー定義師団06
        Division07, // ユーザー定義師団07
        Division08, // ユーザー定義師団08
        Division09, // ユーザー定義師団09
        Division10, // ユーザー定義師団10
        Division11, // ユーザー定義師団11
        Division12, // ユーザー定義師団12
        Division13, // ユーザー定義師団13
        Division14, // ユーザー定義師団14
        Division15, // ユーザー定義師団15
        Division16, // ユーザー定義師団16
        Division17, // ユーザー定義師団17
        Division18, // ユーザー定義師団18
        Division19, // ユーザー定義師団19
        Division20, // ユーザー定義師団20
        Division21, // ユーザー定義師団21
        Division22, // ユーザー定義師団22
        Division23, // ユーザー定義師団23
        Division24, // ユーザー定義師団24
        Division25, // ユーザー定義師団25
        Division26, // ユーザー定義師団26
        Division27, // ユーザー定義師団27
        Division28, // ユーザー定義師団28
        Division29, // ユーザー定義師団29
        Division30, // ユーザー定義師団30
        Division31, // ユーザー定義師団31
        Division32, // ユーザー定義師団32
        Division33, // ユーザー定義師団33
        Division34, // ユーザー定義師団34
        Division35, // ユーザー定義師団35
        Division36, // ユーザー定義師団36
        Division37, // ユーザー定義師団37
        Division38, // ユーザー定義師団38
        Division39, // ユーザー定義師団39
        Division40, // ユーザー定義師団40
        Division41, // ユーザー定義師団41
        Division42, // ユーザー定義師団42
        Division43, // ユーザー定義師団43
        Division44, // ユーザー定義師団44
        Division45, // ユーザー定義師団45
        Division46, // ユーザー定義師団46
        Division47, // ユーザー定義師団47
        Division48, // ユーザー定義師団48
        Division49, // ユーザー定義師団49
        Division50, // ユーザー定義師団50
        Division51, // ユーザー定義師団51
        Division52, // ユーザー定義師団52
        Division53, // ユーザー定義師団53
        Division54, // ユーザー定義師団54
        Division55, // ユーザー定義師団55
        Division56, // ユーザー定義師団56
        Division57, // ユーザー定義師団57
        Division58, // ユーザー定義師団58
        Division59, // ユーザー定義師団59
        Division60, // ユーザー定義師団60
        Division61, // ユーザー定義師団61
        Division62, // ユーザー定義師団62
        Division63, // ユーザー定義師団63
        Division64, // ユーザー定義師団64
        Division65, // ユーザー定義師団65
        Division66, // ユーザー定義師団66
        Division67, // ユーザー定義師団67
        Division68, // ユーザー定義師団68
        Division69, // ユーザー定義師団69
        Division70, // ユーザー定義師団70
        Division71, // ユーザー定義師団71
        Division72, // ユーザー定義師団72
        Division73, // ユーザー定義師団73
        Division74, // ユーザー定義師団74
        Division75, // ユーザー定義師団75
        Division76, // ユーザー定義師団76
        Division77, // ユーザー定義師団77
        Division78, // ユーザー定義師団78
        Division79, // ユーザー定義師団79
        Division80, // ユーザー定義師団80
        Division81, // ユーザー定義師団81
        Division82, // ユーザー定義師団82
        Division83, // ユーザー定義師団83
        Division84, // ユーザー定義師団84
        Division85, // ユーザー定義師団85
        Division86, // ユーザー定義師団86
        Division87, // ユーザー定義師団87
        Division88, // ユーザー定義師団88
        Division89, // ユーザー定義師団89
        Division90, // ユーザー定義師団90
        Division91, // ユーザー定義師団91
        Division92, // ユーザー定義師団92
        Division93, // ユーザー定義師団93
        Division94, // ユーザー定義師団94
        Division95, // ユーザー定義師団95
        Division96, // ユーザー定義師団96
        Division97, // ユーザー定義師団97
        Division98, // ユーザー定義師団98
        Division99 // ユーザー定義師団99
    }
}