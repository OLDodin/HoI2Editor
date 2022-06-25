﻿using System;
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
    ///     Providence data group
    /// </summary>
    public static class Provinces
    {
        #region Public properties

        /// <summary>
        ///     Master Providence List
        /// </summary>
        public static List<Province> Items { get; }

        /// <summary>
        ///     Sea area list
        /// </summary>
        public static List<int> SeaZones { get; }

        /// <summary>
        ///     With the sea area ID Correspondence of
        /// </summary>
        public static Dictionary<int, Province> SeaZoneMap { get; }

        /// <summary>
        ///     Available continents ID
        /// </summary>
        public static List<ContinentId> Continents { get; private set; }

        /// <summary>
        ///     Available regions ID
        /// </summary>
        public static List<RegionId> Regions { get; private set; }

        /// <summary>
        ///     Available area ID
        /// </summary>
        public static List<AreaId> Areas { get; private set; }

        /// <summary>
        ///     Available climate ID
        /// </summary>
        public static List<ClimateId> Climates { get; private set; }

        /// <summary>
        ///     Available terrain ID
        /// </summary>
        public static List<TerrainId> Terrains { get; private set; }

        /// <summary>
        ///     Correspondence between continents and regions
        /// </summary>
        public static Dictionary<ContinentId, List<RegionId>> ContinentRegionMap { get; private set; }

        /// <summary>
        ///     Correspondence between regions
        /// </summary>
        public static Dictionary<RegionId, List<AreaId>> RegionAreaMap { get; private set; }

        /// <summary>
        ///     Correspondence between regions and provinces
        /// </summary>
        public static Dictionary<AreaId, List<Province>> AreaProvinceMap { get; private set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     With regional strings ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, AreaId> AreaStringMap = new Dictionary<string, AreaId>();

        /// <summary>
        ///     With local strings ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, RegionId> RegionStringMap = new Dictionary<string, RegionId>();

        /// <summary>
        ///     With continental strings ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, ContinentId> ContinentStringMap =
            new Dictionary<string, ContinentId>();

        /// <summary>
        ///     With climate string ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, ClimateId> ClimateStringMap = new Dictionary<string, ClimateId>();

        /// <summary>
        ///     With terrain character string ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, TerrainId> TerrainStringMap = new Dictionary<string, TerrainId>();

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

        #endregion

        #region Internal constant

        /// <summary>
        ///     Regional string
        /// </summary>
        private static readonly string[] AreaStrings =
        {
            "-",
            "Adelaide",
            "Afghanistan",
            "Agats",
            "Alabama",
            "Alaska",
            "Alaskan Archipelago",
            "Albania",
            "Alberta",
            "Alice Springs",
            "Alpes_Provence",
            "Amur",
            "Anatolian North Coast",
            "Anatolian South Coast",
            "Angolan Coast",
            "Angolan Plains",
            "Anhui",
            "Antilles",
            "Appennine Ridge",
            "Aquitaine",
            "Arabian Desert",
            "Arizona",
            "Arkansas",
            "Arkhangelsk",
            "Armenia_Azerbaijan",
            "Astrakhan",
            "Asuncion",
            "Attica",
            "Austria",
            "Babo",
            "Baden_Wurttemberg",
            "Bahamas Islands",
            "Baleares",
            "Belgorod",
            "Bavaria",
            "Bechuanaland",
            "Bengal",
            "Bengazi",
            "Bermuda",
            "Bessarabia",
            "Bohemia",
            "Bombay",
            "Bonin",
            "Bosnia",
            "Bosporus",
            "Bougainville",
            "Bourgogne_Champagne",
            "Brandenburg",
            "Brasilia",
            "Brest Litovsk",
            "Brisbane",
            "British Columbia",
            "Brittany",
            "Bulgaria",
            "Burma",
            "Cairns",
            "California",
            "Cameroonian Jungle",
            "Canarias",
            "Cantabric Chain",
            "Cap Verde Islands",
            "Cape",
            "Caracas",
            "Carolinese",
            "Catalonia",
            "Celebes",
            "Central Africa Desert",
            "Central Africa Plains",
            "Central Finland",
            "Central Italy",
            "Central Norway",
            "Central Rainforests",
            "Central Serbia",
            "Central Spain",
            "Central Turkey",
            "Chahar",
            "Chihuahua",
            "Chile",
            "Chilean Archipelago",
            "Colorado",
            "Connecticut_Rhode Island",
            "Continental Spain",
            "Croatia",
            "Cuba",
            "Danakil Plateau",
            "Darwin",
            "Deccar",
            "Delhi",
            "Delta",
            "Denmark",
            "Diego Garcia Island",
            "Dnepropretovsk",
            "East Andalucia",
            "East Atlas",
            "East England",
            "East Java",
            "East Persia",
            "East Prussia",
            "East Serbia",
            "Eastern Anatolia",
            "Eastern Ghat",
            "Eastern Hungary",
            "Eire",
            "Engels",
            "El Alamein",
            "El Rif",
            "Espirtu Santo",
            "Estonia",
            "Ethiopian Highland",
            "Fiji",
            "Flanders",
            "Flores_Timor",
            "Florida",
            "Fujian",
            "Gabes",
            "Gansu",
            "Georgia",
            "Georgien",
            "Goetaland",
            "Gold Coast",
            "Greek Island",
            "Grodno",
            "Groznyi",
            "Guadalcanal",
            "Guangdong",
            "Guangxi",
            "Guayana",
            "Guinean Coast",
            "Guizhou",
            "Hannover_Magdeburg",
            "Hawaii",
            "Hebei",
            "Hedjaz",
            "Heilongjiang",
            "Henan",
            "Hessen",
            "Himalayas",
            "Hispaniola",
            "Holstein_Mecklemburg",
            "Honshu",
            "Hubei",
            "Hunan",
            "Iceland",
            "Idaho",
            "Illinois",
            "Indiana",
            "Indochine",
            "Interior Algeria",
            "Interior Libya",
            "Iowa",
            "Iquitos",
            "Iraq",
            "Irkutsk",
            "Ivory Coast",
            "Jiangsu",
            "Jiangxi",
            "Jilin",
            "Johnson Island",
            "Kamchatka",
            "Kansas",
            "Karelia",
            "Kassarine",
            "Kazakstan",
            "Kazan",
            "Kentucky",
            "Kenya",
            "Khabarovsk",
            "Kharkov",
            "Kiev",
            "Kirgiz Steppe",
            "Kirgizistan",
            "Kirov",
            "Kola",
            "Kongolese Jungle",
            "Kostroma",
            "Krakow",
            "Krasnodar",
            "Krim",
            "Kurdufan",
            "Kursk",
            "Kuybyshev",
            "Kyushu_Shikoku",
            "La Paz",
            "La Plata",
            "Lae",
            "Lakes",
            "Lapland",
            "Latvia",
            "Lebanon_Syria",
            "Leningrad",
            "Levant",
            "Leyte Island Group",
            "Liaoning",
            "Ligurian Islands",
            "Lima",
            "Limousin_Auvergne",
            "Lithuania",
            "Loire",
            "Lorraine_Alsace",
            "Louisiana",
            "Lower Argentine",
            "Lower California",
            "Luzon",
            "Lvov",
            "Madagascar",
            "Magadan",
            "Magdalena",
            "Maghreb Coast",
            "Maine",
            "Malacka",
            "Malian Desert",
            "Malian Valleys",
            "Manaus",
            "Manitoba",
            "Maracaibo",
            "Marcus_Wake",
            "Marshalls",
            "Maryland_Delaware",
            "Massachussets",
            "Mato Grosso",
            "Mauretania",
            "Melbourne",
            "Melkosopotjnik",
            "Memel",
            "Mexico City",
            "Michigan",
            "Midway",
            "Mindanao",
            "Minnesota",
            "Minsk",
            "Mississippi",
            "Missouri",
            "Mocambique",
            "Molucks",
            "Mongolia",
            "Montana",
            "Morocco",
            "Moscow",
            "Mozyr",
            "Nagpur",
            "Nalchik",
            "Nambia",
            "Nauru",
            "Nebraska",
            "Nedre Norrland",
            "Netherlands",
            "New Brunswick",
            "New Foundland",
            "New Hampshire",
            "New Jersey",
            "New Mexico",
            "New York",
            "New Zealand",
            "Nevada",
            "Niassa",
            "Nicaragua",
            "Niger",
            "Nile Valley",
            "Nizhnij Novgorod",
            "Normandy",
            "North Borneo",
            "North Carolina",
            "North Dakota",
            "North England",
            "North Ethiopia",
            "North Gilberts",
            "North Ireland",
            "North Italy",
            "North Krasnoyarsk",
            "North Marianes",
            "North Nigeria",
            "North Persia",
            "North Rhodesia",
            "North Romania",
            "North Scotland",
            "Northern Norway",
            "Northwest Territories",
            "Nova Scotia",
            "Novgorod",
            "Novosibirsk",
            "Nubian Desert",
            "Odessa",
            "Oesterbotten",
            "Oestra Svealand",
            "Oevre Norrland",
            "Ohio",
            "Oklahoma",
            "Omsk",
            "Ontario",
            "Oran",
            "Oregon",
            "Orenburg",
            "Orissa",
            "Pakistan",
            "Palawan_Mindoro",
            "Palestine",
            "Panaman Ridge",
            "Paris",
            "Pas de Calais",
            "Pennsylvania",
            "Penza",
            "Perm",
            "Persian Gulf Coast",
            "Perth",
            "Petrozavodsk",
            "Phoenix",
            "Polotsk",
            "Poltava",
            "Polynesia",
            "Port Moresby",
            "Portugal",
            "Poznan",
            "Primorski",
            "Pskov",
            "Pyongyang",
            "Pyrenees_Languedoc",
            "Qattara",
            "Qinghai",
            "Quebec",
            "Quito",
            "Rabaul",
            "Rajastan",
            "Red Sea Coast",
            "Rehe",
            "Reservoire Rybinsk",
            "Rhineland",
            "Rio de Janeiro",
            "Rio de Oro",
            "Rogachev",
            "Rostov",
            "Ryazan",
            "Sachsen",
            "Sakhalin_Kuriles",
            "Samoa",
            "Sao Paulo",
            "Saransk",
            "Sarmi",
            "Saskatchewan",
            "Senegambia",
            "Senjavin",
            "Seoul",
            "Shaanxi",
            "Shaba",
            "Shandong",
            "Shanxi",
            "Siam",
            "Sichuan",
            "Sicilian Gap",
            "Sidi Barrani",
            "Silesia",
            "Sirte",
            "Slave Coast",
            "Slovakia",
            "Smolensk",
            "Somalia",
            "Somaliland",
            "South Borneo",
            "South Carolina",
            "South Dakota",
            "South Ethiopia",
            "South Finland",
            "South Gilberts",
            "South Italy",
            "South Krasnoyarsk",
            "South Nigeria",
            "South Persia",
            "South Rhodesia",
            "South Romania",
            "South Scotland",
            "Southcentral Norway",
            "Southern Norway",
            "St.Helena",
            "Stalingrad",
            "Suez Channel Area",
            "Suiyuan_Ningxia",
            "Sumatra",
            "Sunda Islands",
            "Sverdlovsk",
            "Switzerland",
            "Sydney",
            "Syzran",
            "Tadzjikistan",
            "Taiwan",
            "Tajmyr_Evenk",
            "Tambov",
            "Tanganyika",
            "Tennessee",
            "Texas",
            "The Azores",
            "The Falklands",
            "Thrace",
            "Tibet",
            "Tierra del Fuego",
            "Tobruk",
            "Tohoku",
            "Tomsk",
            "Transjordan",
            "Transnistria",
            "Transvaal_Natal",
            "Transylvania",
            "Tunis",
            "Turkestan",
            "Turkmenistan",
            "Tyumen",
            "Ufa",
            "Uganda",
            "Upper Argentine",
            "Upper Nile Valley",
            "Utah",
            "Uzbekistan",
            "Vaestra Svealand",
            "Wallonia",
            "Warsaw",
            "Washington",
            "Velikiye Luki",
            "Vera Cruz",
            "Vermont",
            "Vorkuta",
            "West Andalucia",
            "West Atlas",
            "West England",
            "West Java",
            "West Virginia",
            "Western Desert",
            "Western Ghat",
            "Western Hungary",
            "Westphalen",
            "Virginia",
            "Wisconsin",
            "Vladimir",
            "Volta",
            "Wyoming",
            "Xikang",
            "Xinjiang",
            "Yakutsk",
            "Yemenite Mountains",
            "Yukon",
            "Yunnan",
            "Zabaikalye",
            "Zhejiang",
            "Ryukyusland",
            "North Bismarck Archipelago",
            "North New Hebrides",
            "South New Hebrides",
            "Central Solomons",
            "West Aleutians",
            "Ceylon",
            "Hokkaido",
            "North Burma",
            "East Bengal",
            "Kra Peninsula",
            "Tripoli",
            "Greenland",
            "Lake",
            "Adriatic Sea",
            "Aegean Sea",
            "Black Sea",
            "Red Sea",
            "Tyrrhenian Sea",
            "Bothnian Bay",
            "Northern Baltic",
            "Southern Baltic",
            "Kattegat",
            "Barents Sea",
            "Luzon Strait",
            "Philipine Trench",
            "Sulu Sea",
            "Celebes Sea",
            "Coast of Brunei",
            "Flores Sea",
            "Molucca Sea",
            "Banda Sea",
            "West Coral Sea",
            "Arafura Sea",
            "Java Ridge",
            "Malacca Strait",
            "Gulf of Siam",
            "Spratly Sea",
            "Coast of Indochina",
            "Taiwan Strait",
            "Irish Sea",
            "English Channel",
            "Denmark Strait",
            "Southeast Northsea",
            "West Northsea",
            "Central Northsea",
            "Norwegian Sea",
            "Costa del Sol",
            "Algerian Coast",
            "Egyptian Coast",
            "Gulf of Lyon",
            "Sea of Japan",
            "North Bismarck Sea",
            "South Bismarck Sea",
            "Marianas",
            "Marshalls Sea",
            "Western Solomons",
            "Eastern Solomons",
            "East Coral Sea",
            "Coast of Japan",
            "Western Approaches",
            "Greenland Coast",
            "Faroes Gap",
            "North Atlantic",
            "Bay of Biscay",
            "Azores",
            "Portugese Coast",
            "Cap St Vincent",
            "Coast of Brazil",
            "Cap Verde",
            "Gulf of Guinea",
            "Central Atlantic",
            "Coast of Africa",
            "Coast of Bissao",
            "Solomon Sea",
            "North Tasman Sea",
            "South Tasman Sea",
            "East Great Australian Bight",
            "West Great Australian Bight",
            "Hudson Bay",
            "Persian Gulf",
            "Yucatan Strait",
            "Central Carribean",
            "Windward Islands",
            "West Gulf of Mexico",
            "East Gulf of Mexico",
            "Florida Strait",
            "Bermuda Triangle",
            "Northern Sea of Labrador",
            "Southern Sea of Labrador",
            "Grand Banks",
            "The Seamounts",
            "Canadian Maritimes",
            "US Maritimes",
            "Guiana Basin",
            "South-Central Mid-Atlantic Ridge",
            "Central Mid-Atlantic Ridge",
            "Aleutians",
            "Carolines",
            "Central Philippine Sea",
            "Coast of Ceylon",
            "Coast of Kamchatka",
            "East Bay of Bengal",
            "East Bering Sea",
            "East Mariana Basin",
            "East Sea of Okhotsk",
            "Gulf of Alaska",
            "Hawaiian Rise",
            "Java Trench",
            "Mariana Trench",
            "Mid-Pacific Mountains",
            "Ninetyeast Ridge",
            "North Northeast Pacific Basin",
            "Northwest Pacific Basin",
            "Ryukyus",
            "South Sea of Okhotsk",
            "Timor Sea",
            "West Bay of Bengal",
            "West Bering Sea",
            "West Coast of Australia",
            "West Coast of Mexico",
            "West Coast of the United States",
            "West Sea of Okhotsk",
            "Yellow Sea",
            "Atlantic-Indian Ridge",
            "Coast of South Africa",
            "Fiji Basin",
            "Gilberts",
            "Line Islands",
            "Mascarene Plateau",
            "Mid-Indian Ridge",
            "North Arabian Sea",
            "North East Pacific Ocean",
            "North Mozambique Channel",
            "Northeast Coast of Madagascar",
            "South Arabian Sea",
            "South East Pacific Ocean",
            "South Mozambique Channel",
            "Southeast Coast of Madagascar",
            "Southeast Indian Ocean",
            "Southwest Indian Ocean",
            "East Cost of New Zealand",
            "North Southeast Pacific Ocean",
            "South Southeast Pacific Ocean",
            "Southeast Pacific Basin",
            "Southwest Pacific Basin",
            "West Coast of Central America",
            "West Coast of Chile",
            "West Coast of Peru",
            "Horn of Africa",
            "Coast of Angola-Namibia",
            "Angola Plain",
            "Argentine Plain",
            "Coast of Argentina",
            "Coast of Uruguay",
            "Coast of New Guinea",
            "Coast of Guyana",
            "Coast of Recife",
            "Cape Finisterre",
            "Arctic Ocean",
            "The Hebreides",
            "Irish West Coast",
            "Pernambuco Plain",
            "Ascension Fracture Zone",
            "East North Sea",
            "Caspian Sea"
        };

        /// <summary>
        ///     Area name
        /// </summary>
        private static readonly string[] AreaNames =
        {
            "AREA_-",
            "AREA_ADELAIDE",
            "AREA_AFGHANISTAN",
            "AREA_AGATS",
            "AREA_ALABAMA",
            "AREA_ALASKA",
            "AREA_ALASKAN_ARCHIPELAGO",
            "AREA_ALBANIA",
            "AREA_ALBERTA",
            "AREA_ALICE_SPRINGS",
            "AREA_ALPES_PROVENCE",
            "AREA_AMUR",
            "AREA_ANATOLIAN_NORTH_COAST",
            "AREA_ANATOLIAN_SOUTH_COAST",
            "AREA_ANGOLAN_COAST",
            "AREA_ANGOLAN_PLAINS",
            "AREA_ANHUI",
            "AREA_ANTILLES",
            "AREA_APPENNINE_RIDGE",
            "AREA_AQUITAINE",
            "AREA_ARABIAN_DESERT",
            "AREA_ARIZONA",
            "AREA_ARKANSAS",
            "AREA_ARKHANGELSK",
            "AREA_ARMENIA_AZERBAIJAN",
            "AREA_ASTRAKHAN",
            "AREA_ASUNCION",
            "AREA_ATTICA",
            "AREA_AUSTRIA",
            "AREA_BABO",
            "AREA_BADEN_WURTTEMBERG",
            "AREA_BAHAMAS_ISLANDS",
            "AREA_BALEARES",
            "AREA_BELGOROD",
            "AREA_BAVARIA",
            "AREA_BECHUANALAND",
            "AREA_BENGAL",
            "AREA_BENGAZI",
            "AREA_BERMUDA",
            "AREA_BESSARABIA",
            "AREA_BOHEMIA",
            "AREA_BOMBAY",
            "AREA_BONIN",
            "AREA_BOSNIA",
            "AREA_BOSPORUS",
            "AREA_BOUGAINVILLE",
            "AREA_BOURGOGNE_CHAMPAGNE",
            "AREA_BRANDENBURG",
            "AREA_BRASILIA",
            "AREA_BREST_LITOVSK",
            "AREA_BRISBANE",
            "AREA_BRITISH_COLUMBIA",
            "AREA_BRITTANY",
            "AREA_BULGARIA",
            "AREA_BURMA",
            "AREA_CAIRNS",
            "AREA_CALIFORNIA",
            "AREA_CAMEROONIAN_JUNGLE",
            "AREA_CANARIAS",
            "AREA_CANTABRIC_CHAIN",
            "AREA_CAP_VERDE_ISLANDS",
            "AREA_CAPE",
            "AREA_CARACAS",
            "AREA_CAROLINESE",
            "AREA_CATALONIA",
            "AREA_CELEBES",
            "AREA_CENTRAL_AFRICA_DESERT",
            "AREA_CENTRAL_AFRICA_PLAINS",
            "AREA_CENTRAL_FINLAND",
            "AREA_CENTRAL_ITALY",
            "AREA_CENTRAL_NORWAY",
            "AREA_CENTRAL_RAINFORESTS",
            "AREA_CENTRAL_SERBIA",
            "AREA_CENTRAL_SPAIN",
            "AREA_CENTRAL_TURKEY",
            "AREA_CHAHAR",
            "AREA_CHIHUAHUA",
            "AREA_CHILE",
            "AREA_CHILEAN_ARCHIPELAGO",
            "AREA_COLORADO",
            "AREA_CONNECTICUT_RHODE_ISLAND",
            "AREA_CONTINENTAL_SPAIN",
            "AREA_CROATIA",
            "AREA_CUBA",
            "AREA_DANAKIL_PLATEAU",
            "AREA_DARWIN",
            "AREA_DECCAR",
            "AREA_DELHI",
            "AREA_DELTA",
            "AREA_DENMARK",
            "AREA_DIEGO_GARCIA_ISLAND",
            "AREA_DNEPROPRETOVSK",
            "AREA_EAST_ANDALUCIA",
            "AREA_EAST_ATLAS",
            "AREA_EAST_ENGLAND",
            "AREA_EAST_JAVA",
            "AREA_EAST_PERSIA",
            "AREA_EAST_PRUSSIA",
            "AREA_EAST_SERBIA",
            "AREA_EASTERN_ANATOLIA",
            "AREA_EASTERN_GHAT",
            "AREA_EASTERN_HUNGARY",
            "AREA_EIRE",
            "AREA_ENGELS",
            "AREA_EL_ALAMEIN",
            "AREA_EL_RIF",
            "AREA_ESPIRTU_SANTO",
            "AREA_ESTONIA",
            "AREA_ETHIOPIAN_HIGHLAND",
            "AREA_FIJI",
            "AREA_FLANDERS",
            "AREA_FLORES_TIMOR",
            "AREA_FLORIDA",
            "AREA_FUJIAN",
            "AREA_GABES",
            "AREA_GANSU",
            "AREA_GEORGIA",
            "AREA_GEORGIEN",
            "AREA_GOETALAND",
            "AREA_GOLD_COAST",
            "AREA_GREEK_ISLAND",
            "AREA_GRODNO",
            "AREA_GROZNYI",
            "AREA_GUADALCANAL",
            "AREA_GUANGDONG",
            "AREA_GUANGXI",
            "AREA_GUAYANA",
            "AREA_GUINEAN_COAST",
            "AREA_GUIZHOU",
            "AREA_HANNOVER_MAGDEBURG",
            "AREA_HAWAII",
            "AREA_HEBEI",
            "AREA_HEDJAZ",
            "AREA_HEILONGJIANG",
            "AREA_HENAN",
            "AREA_HESSEN",
            "AREA_HIMALAYAS",
            "AREA_HISPANIOLA",
            "AREA_HOLSTEIN_MECKLEMBURG",
            "AREA_HONSHU",
            "AREA_HUBEI",
            "AREA_HUNAN",
            "AREA_ICELAND",
            "AREA_IDAHO",
            "AREA_ILLINOIS",
            "AREA_INDIANA",
            "AREA_INDOCHINE",
            "AREA_INTERIOR_ALGERIA",
            "AREA_INTERIOR_LIBYA",
            "AREA_IOWA",
            "AREA_IQUITOS",
            "AREA_IRAQ",
            "AREA_IRKUTSK",
            "AREA_IVORY_COAST",
            "AREA_JIANGSU",
            "AREA_JIANGXI",
            "AREA_JILIN",
            "AREA_JOHNSON_ISLAND",
            "AREA_KAMCHATKA",
            "AREA_KANSAS",
            "AREA_KARELIA",
            "AREA_KASSARINE",
            "AREA_KAZAKSTAN",
            "AREA_KAZAN",
            "AREA_KENTUCKY",
            "AREA_KENYA",
            "AREA_KHABAROVSK",
            "AREA_KHARKOV",
            "AREA_KIEV",
            "AREA_KIRGIZ_STEPPE",
            "AREA_KIRGIZISTAN",
            "AREA_KIROV",
            "AREA_KOLA",
            "AREA_KONGOLESE_JUNGLE",
            "AREA_KOSTROMA",
            "AREA_KRAKOW",
            "AREA_KRASNODAR",
            "AREA_KRIM",
            "AREA_KURDUFAN",
            "AREA_KURSK",
            "AREA_KUYBYSHEV",
            "AREA_KYUSHU_SHIKOKU",
            "AREA_LA_PAZ",
            "AREA_LA_PLATA",
            "AREA_LAE",
            "AREA_LAKES",
            "AREA_LAPLAND",
            "AREA_LATVIA",
            "AREA_LEBANON_SYRIA",
            "AREA_LENINGRAD",
            "AREA_LEVANT",
            "AREA_LEYTE_ISLAND_GROUP",
            "AREA_LIAONING",
            "AREA_LIGURIAN_ISLANDS",
            "AREA_LIMA",
            "AREA_LIMOUSIN_AUVERGNE",
            "AREA_LITHUANIA",
            "AREA_LOIRE",
            "AREA_LORRAINE_ALSACE",
            "AREA_LOUISIANA",
            "AREA_LOWER_ARGENTINE",
            "AREA_LOWER_CALIFORNIA",
            "AREA_LUZON",
            "AREA_LVOV",
            "AREA_MADAGASCAR",
            "AREA_MAGADAN",
            "AREA_MAGDALENA",
            "AREA_MAGHREB_COAST",
            "AREA_MAINE",
            "AREA_MALACKA",
            "AREA_MALIAN_DESERT",
            "AREA_MALIAN_VALLEYS",
            "AREA_MANAUS",
            "AREA_MANITOBA",
            "AREA_MARACAIBO",
            "AREA_MARCUS_WAKE",
            "AREA_MARSHALLS",
            "AREA_MARYLAND_DELAWARE",
            "AREA_MASSACHUSSETS",
            "AREA_MATO_GROSSO",
            "AREA_MAURETANIA",
            "AREA_MELBOURNE",
            "AREA_MELKOSOPOTJNIK",
            "AREA_MEMEL",
            "AREA_MEXICO_CITY",
            "AREA_MICHIGAN",
            "AREA_MIDWAY",
            "AREA_MINDANAO",
            "AREA_MINNESOTA",
            "AREA_MINSK",
            "AREA_MISSISSIPPI",
            "AREA_MISSOURI",
            "AREA_MOCAMBIQUE",
            "AREA_MOLUCKS",
            "AREA_MONGOLIA",
            "AREA_MONTANA",
            "AREA_MOROCCO",
            "AREA_MOSCOW",
            "AREA_MOZIR",
            "AREA_NAGPUR",
            "AREA_NALCHIK",
            "AREA_NAMBIA",
            "AREA_NAURU",
            "AREA_NEBRASKA",
            "AREA_NEDRE_NORRLAND",
            "AREA_NETHERLANDS",
            "AREA_NEW_BRUNSWICK",
            "AREA_NEW_FOUNDLAND",
            "AREA_NEW_HAMPSHIRE",
            "AREA_NEW_JERSEY",
            "AREA_NEW_MEXICO",
            "AREA_NEW_YORK",
            "AREA_NEW_ZEALAND",
            "AREA_NEVADA",
            "AREA_NIASSA",
            "AREA_NICARAGUA",
            "AREA_NIGER",
            "AREA_NILE_VALLEY",
            "AREA_NIZHNIJ_NOVGOROD",
            "AREA_NORMANDY",
            "AREA_NORTH_BORNEO",
            "AREA_NORTH_CAROLINA",
            "AREA_NORTH_DAKOTA",
            "AREA_NORTH_ENGLAND",
            "AREA_NORTH_ETHIOPIA",
            "AREA_NORTH_GILBERTS",
            "AREA_NORTH_IRELAND",
            "AREA_NORTH_ITALY",
            "AREA_NORTH_KRASNOYARSK",
            "AREA_NORTH_MARIANES",
            "AREA_NORTH_NIGERIA",
            "AREA_NORTH_PERSIA",
            "AREA_NORTH_RHODESIA",
            "AREA_NORTH_ROMANIA",
            "AREA_NORTH_SCOTLAND",
            "AREA_NORTHERN_NORWAY",
            "AREA_NORTHWEST_TERRITORIES",
            "AREA_NOVA_SCOTIA",
            "AREA_NOVGOROD",
            "AREA_NOVOSIBIRSK",
            "AREA_NUBIAN_DESERT",
            "AREA_ODESSA",
            "AREA_OESTERBOTTEN",
            "AREA_OESTRA_SVEALAND",
            "AREA_OEVRE_NORRLAND",
            "AREA_OHIO",
            "AREA_OKLAHOMA",
            "AREA_OMSK",
            "AREA_ONTARIO",
            "AREA_ORAN",
            "AREA_OREGON",
            "AREA_ORENBURG",
            "AREA_ORISSA",
            "AREA_PAKISTAN",
            "AREA_PALAWAN_MINDORO",
            "AREA_PALESTINE",
            "AREA_PANAMAN_RIDGE",
            "AREA_PARIS",
            "AREA_PAS_DE_CALAIS",
            "AREA_PENNSYLVANIA",
            "AREA_PENZA",
            "AREA_PERM",
            "AREA_PERSIAN_GULF_COAST",
            "AREA_PERTH",
            "AREA_PETROZAVODSK",
            "AREA_PHOENIX",
            "AREA_POLOTSK",
            "AREA_POLTAVA",
            "AREA_POLYNESIA",
            "AREA_PORT_MORESBY",
            "AREA_PORTUGAL",
            "AREA_POZNAN",
            "AREA_PRIMORSKI",
            "AREA_PSKOV",
            "AREA_PYONGYANG",
            "AREA_PYRENEES_LANGUEDOC",
            "AREA_QATTARA",
            "AREA_QINGHAI",
            "AREA_QUEBEC",
            "AREA_QUITO",
            "AREA_RABAUL",
            "AREA_RAJASTAN",
            "AREA_RED_SEA_COAST",
            "AREA_REHE",
            "AREA_RESERVOIRE_RYBINSK",
            "AREA_RHINELAND",
            "AREA_RIO_DE_JANEIRO",
            "AREA_RIO_DE_ORO",
            "AREA_ROGACHEV",
            "AREA_ROSTOV",
            "AREA_RYAZAN",
            "AREA_SACHSEN",
            "AREA_SAKHALIN_KURILES",
            "AREA_SAMOA",
            "AREA_SAO_PAULO",
            "AREA_SARANSK",
            "AREA_SARMI",
            "AREA_SASKATCHEWAN",
            "AREA_SENEGAMBIA",
            "AREA_SENJAVIN",
            "AREA_SEOUL",
            "AREA_SHAANXI",
            "AREA_SHABA",
            "AREA_SHANDONG",
            "AREA_SHANXI",
            "AREA_SIAM",
            "AREA_SICHUAN",
            "AREA_SICILIAN_GAP",
            "AREA_SIDI_BARRANI",
            "AREA_SILESIA",
            "AREA_SIRTE",
            "AREA_SLAVE_COAST",
            "AREA_SLOVAKIA",
            "AREA_SMOLENSK",
            "AREA_SOMALIA",
            "AREA_SOMALILAND",
            "AREA_SOUTH_BORNEO",
            "AREA_SOUTH_CAROLINA",
            "AREA_SOUTH_DAKOTA",
            "AREA_SOUTH_ETHIOPIA",
            "AREA_SOUTH_FINLAND",
            "AREA_SOUTH_GILBERTS",
            "AREA_SOUTH_ITALY",
            "AREA_SOUTH_KRASNOYARSK",
            "AREA_SOUTH_NIGERIA",
            "AREA_SOUTH_PERSIA",
            "AREA_SOUTH_RHODESIA",
            "AREA_SOUTH_ROMANIA",
            "AREA_SOUTH_SCOTLAND",
            "AREA_SOUTHCENTRAL_NORWAY",
            "AREA_SOUTHERN_NORWAY",
            "AREA_ST.HELENA",
            "AREA_STALINGRAD",
            "AREA_SUEZ_CHANNEL_AREA",
            "AREA_SUIYUAN_NINGXIA",
            "AREA_SUMATRA",
            "AREA_SUNDA_ISLANDS",
            "AREA_SVERDLOVSK",
            "AREA_SWITZERLAND",
            "AREA_SYDNEY",
            "AREA_SYZRAN",
            "AREA_TADZJIKISTAN",
            "AREA_TAIWAN",
            "AREA_TAJMYR_EVENK",
            "AREA_TAMBOV",
            "AREA_TANGANYIKA",
            "AREA_TENNESSEE",
            "AREA_TEXAS",
            "AREA_THE_AZORES",
            "AREA_THE_FALKLANDS",
            "AREA_THRACE",
            "AREA_TIBET",
            "AREA_TIERRA_DEL_FUEGO",
            "AREA_TOBRUK",
            "AREA_TOHOKU",
            "AREA_TOMSK",
            "AREA_TRANSJORDAN",
            "AREA_TRANSNISTRIA",
            "AREA_TRANSVAAL_NATAL",
            "AREA_TRANSYLVANIA",
            "AREA_TUNIS",
            "AREA_TURKESTAN",
            "AREA_TURKMENISTAN",
            "AREA_TYUMEN",
            "AREA_UFA",
            "AREA_UGANDA",
            "AREA_UPPER_ARGENTINE",
            "AREA_UPPER_NILE_VALLEY",
            "AREA_UTAH",
            "AREA_UZBEKISTAN",
            "AREA_VAESTRA_SVEALAND",
            "AREA_WALLONIA",
            "AREA_WARSAW",
            "AREA_WASHINGTON",
            "AREA_VELIKIYE_LUKI",
            "AREA_VERA_CRUZ",
            "AREA_VERMONT",
            "AREA_VORKUTA",
            "AREA_WEST_ANDALUCIA",
            "AREA_WEST_ATLAS",
            "AREA_WEST_ENGLAND",
            "AREA_WEST_JAVA",
            "AREA_WEST_VIRGINIA",
            "AREA_WESTERN_DESERT",
            "AREA_WESTERN_GHAT",
            "AREA_WESTERN_HUNGARY",
            "AREA_WESTPHALEN",
            "AREA_VIRGINIA",
            "AREA_WISCONSIN",
            "AREA_VLADIMIR",
            "AREA_VOLTA",
            "AREA_WYOMING",
            "AREA_XIKANG",
            "AREA_XINJIANG",
            "AREA_YAKUTSK",
            "AREA_YEMENITE_MOUNTAINS",
            "AREA_YUKON",
            "AREA_YUNNAN",
            "AREA_ZABAIKALYE",
            "AREA_ZHEJIANG",
            "AREA_RYUKYULAND",
            "AREA_NORTH_BISMARCK_ARCHIPELAGO",
            "AREA_NORTH_NEW_HEBRIDES",
            "AREA_SOUTH_NEW_HEBRIDES",
            "AREA_CENTRAL_SOLOMONS",
            "AREA_WEST_ALEUTIANS",
            "AREA_CEYLON",
            "AREA_HOKKAIDO",
            "AREA_NORTH_BURMA",
            "AREA_EAST_BENGAL",
            "AREA_KRA_PENINSULA",
            "AREA_TRIPOLI",
            "AREA_GREENLAND",
            "AREA_LAKE",
            "AREA_ADRIATIC_SEA",
            "AREA_AEGEAN_SEA",
            "AREA_BLACK_SEA",
            "AREA_RED_SEA",
            "AREA_TYRRHENIAN_SEA",
            "AREA_BOTHNIAN_BAY",
            "AREA_NORTHERN_BALTIC",
            "AREA_SOUTHERN_BALTIC",
            "AREA_KATTEGAT",
            "AREA_BARENTS_SEA",
            "AREA_LUZON_STRAIT",
            "AREA_PHILIPPINE_TRENCH",
            "AREA_SULU_SEA",
            "AREA_CELEBES_SEA",
            "AREA_COAST_OF_BRUNEI",
            "AREA_FLORES_SEA",
            "AREA_MOLUCCA_SEA",
            "AREA_BANDA_SEA",
            "AREA_WEST_CORAL_SEA",
            "AREA_ARAFURA_SEA",
            "AREA_JAVA_RIDGE",
            "AREA_MALACCA_STRAIT",
            "AREA_GULF_OF_SIAM",
            "AREA_SPRATLY_SEA",
            "AREA_COAST_OF_INDOCHINA",
            "AREA_TAIWAN_STRAIT",
            "AREA_IRISH_SEA",
            "AREA_ENGLISH_CHANNEL",
            "AREA_DENMARK_STRAIT",
            "AREA_SOUTHEAST_NORTHSEA",
            "AREA_WEST_NORTHSEA",
            "AREA_CENTRAL_NORTHSEA",
            "AREA_NORWEGIAN_SEA",
            "AREA_COSTA_DEL_SOL",
            "AREA_ALGERIAN_COAST",
            "AREA_EGYPTIAN_COAST",
            "AREA_GULF_OF_LYON",
            "AREA_SEA_OF_JAPAN",
            "AREA_NORTH_BISMARCK_SEA",
            "AREA_SOUTH_BISMARCK_SEA",
            "AREA_MARIANAS",
            "AREA_MARSHALLS_SEA",
            "AREA_WESTERN_SOLOMONS",
            "AREA_EASTERN_SOLOMONS",
            "AREA_EAST_CORAL_SEA",
            "AREA_COAST_OF_JAPAN",
            "AREA_WESTERN_APPROACHES",
            "AREA_GREENLAND_COAST",
            "AREA_FAROES_GAP",
            "AREA_NORTH_ATLANTIC",
            "AREA_BAY_OF_BISCAY",
            "AREA_AZORES",
            "AREA_PORTUGESE_COAST",
            "AREA_CAP_ST_VINCENT",
            "AREA_COAST_OF_BRAZIL",
            "AREA_CAP_VERDE",
            "AREA_GULF_OF_GUINEA",
            "AREA_CENTRAL_ATLANTIC",
            "AREA_COAST_OF_AFRICA",
            "AREA_COAST_OF_BISSAO",
            "AREA_SOLOMON_SEA",
            "AREA_NORTH_TASMAN_SEA",
            "AREA_SOUTH_TASMAN_SEA",
            "AREA_EAST_GREAT_AUSTRALIAN_BIGHT",
            "AREA_WEST_GREAT_AUSTRALIAN_BIGHT",
            "AREA_HUDSON_BAY",
            "AREA_PERSIAN_GULF",
            "AREA_YUCATAN_STRAIT",
            "AREA_CENTRAL_CARRIBEAN",
            "AREA_WINDWARD_ISLANDS",
            "AREA_WEST_GULF_OF_MEXICO",
            "AREA_EAST_GULF_OF_MEXICO",
            "AREA_FLORIDA_STRAIT",
            "AREA_BERMUDA_TRIANGLE",
            "NORTHERN_SEA_OF_LABRADOR",
            "SOUTHERN_SEA_OF_LABRADOR",
            "GRAND_BANKS",
            "THE_SEAMOUNTS",
            "CANADA_MARITIMES",
            "US_MARITIMES",
            "GUIANA_BASIN",
            "SOUTH_CENTRAL_MID_ATLANTIC_RIDGE",
            "CENTRAL_MID_ATLANTIC_RIDGE",
            "AREA_ALEUTIANS",
            "AREA_CAROLINES",
            "AREA_CENTRAL_PHILIPPINE_SEA",
            "AREA_COAST_OF_CEYLON",
            "AREA_COAST_OF_KAMCHATKA",
            "AREA_EAST_BAY_OF_BENGAL",
            "AREA_EAST_BERING_SEA",
            "AREA_EAST_MARIANA_BASIN",
            "AREA_EAST_SEA_OF_OKHOTSK",
            "AREA_GULF_OF_ALASKA",
            "AREA_HAWAIIAN_RISE",
            "AREA_JAVA_TRENCH",
            "AREA_MARIANA_TRENCH",
            "AREA_MID_PACIFIC_MOUNTAINS",
            "AREA_NINETYEAST_RIDGE",
            "AREA_NORTH_NORTHEAST_PACIFIC_BASIN",
            "AREA_NORTHWEST_PACIFIC_BASIN",
            "AREA_RYUKYUS",
            "AREA_SOUTH_SEA_OF_OKHOTSK",
            "AREA_TIMOR_SEA",
            "AREA_WEST_BAY_OF_BENGAL",
            "AREA_WEST_BERING_SEA",
            "AREA_WEST_COAST_OF_AUSTRALIA",
            "AREA_WEST_COAST_OF_MEXICO",
            "AREA_WEST_COAST_OF_THE_UNITED_STATES",
            "AREA_WEST_SEA_OF_OKHOTSK",
            "AREA_YELLOW_SEA",
            "AREA_ATLANTIC_INDIAN_RIDGE",
            "AREA_COAST_OF_SOUTH_AFRICA",
            "AREA_FIJI_BASIN",
            "AREA_GILBERTS",
            "AREA_LINE_ISLANDS",
            "AREA_MASCARENE_PLATEAU",
            "AREA_MID_INDIAN_RIDGE",
            "AREA_NORTH_ARABIAN_SEA",
            "AREA_NORTH_EAST_PACIFIC_OCEAN",
            "AREA_NORTH_MOZAMBIQUE_CHANNEL",
            "AREA_NORTHEAST_COAST_OF_MADAGASCAR",
            "AREA_SOUTH_ARABIAN_SEA",
            "AREA_SOUTH_EAST_PACIFIC_OCEAN",
            "AREA_SOUTH_MOZAMBIQUE_CHANNEL",
            "AREA_SOUTHEAST_COAST_OF_MADAGASCAR",
            "AREA_SOUTHEAST_INDIAN_OCEAN",
            "AREA_SOUTHWEST_INDIAN_OCEAN",
            "AREA_EAST_COST_OF_NEW_ZEALAND",
            "AREA_NORTH_SOUTHEAST_PACIFIC_OCEAN",
            "AREA_SOUTH_SOUTHEAST_PACIFIC_OCEAN",
            "AREA_SOUTHEAST_PACIFIC_BASIN",
            "AREA_SOUTHWEST_PACIFIC_BASIN",
            "AREA_WEST_COAST_OF_CENTRAL_AMERICA",
            "AREA_WEST_COAST_OF_CHILE",
            "AREA_WEST_COAST_OF_PERU",
            "AREA_HORN_OF_AFRICA",
            "AREA_COAST_OF_ANGOLA_NAMIBIA",
            "AREA_ANGOLA_PLAIN",
            "AREA_ARGENTINE_PLAIN",
            "AREA_COAST_OF_ARGENTINA",
            "AREA_COAST_OF_URUGUAY",
            "AREA_COAST_OF_NEW_GUINEA",
            "AREA_COAST_OF_GUYANA",
            "AREA_COAST_OF_RECIFE",
            "AREA_CAPE_FINISTERRE",
            "AREA_ARCTIC_OCEAN",
            "AREA_HEBREIDES",
            "AREA_IRISH_WEST_COAST",
            "AREA_PERNANBUCO_PLAIN",
            "AREA_ASCENSION_FRACTURE_ZONE",
            "AREA_EAST_NORTH_SEA",
            "AREA_CASPIAN_SEA"
        };

        /// <summary>
        ///     Local character string
        /// </summary>
        private static readonly string[] RegionStrings =
        {
            "-",
            "Afghanistan",
            "Alaska",
            "Algeria",
            "Amazonas",
            "American Pacific",
            "Anatolia",
            "Andes",
            "Asian Pacific",
            "Australia",
            "Austria",
            "Balkan",
            "Baltic States",
            "Belarus",
            "Benelux",
            "Bismarck Archipelago",
            "Brazil",
            "Brazilian Highlands",
            "Canada",
            "Caribbean",
            "Central Africa",
            "Central America",
            "Central Asia",
            "China",
            "Czechoslovakia",
            "Denmark",
            "Diego Garcia Island",
            "East Africa",
            "Eastern Russia",
            "Egypt",
            "England",
            "Ethiopia",
            "Far East Siberia",
            "Finland",
            "France",
            "Germany",
            "Gran Chaco",
            "Grand Colombia",
            "Greenland",
            "Horn of Africa",
            "Hungary",
            "Iceland",
            "India",
            "Indochine",
            "Indonesia",
            "Ireland",
            "Irkutsk",
            "Italy",
            "Japan",
            "Kaukasus",
            "Kazakstan",
            "Korea",
            "Krasnoyarsk",
            "Libya",
            "Madagascar",
            "Mexico",
            "Middle East",
            "Midwest US",
            "Morocco",
            "New Guinea",
            "North Solomon Islands",
            "Northeast US",
            "Northern Permafrost",
            "Northern Russia",
            "Northwest US",
            "Norway",
            "Novosibirsk",
            "Patagonia",
            "Persia",
            "Philippines",
            "Poland",
            "Portugal",
            "Rio de la Plata",
            "Romania",
            "Russia",
            "Sahara",
            "Scotland",
            "South Africa",
            "South Solomon Islands",
            "Southcentral US",
            "Southeast US",
            "Southern Russia",
            "Southwest Africa",
            "Southwest US",
            "Spain",
            "Sudan",
            "Sweden",
            "Switzerland",
            "Tomsk",
            "Tunisia",
            "Ukraine",
            "Urals",
            "West Africa",
            "White Sea Tundra",
            "Eastern Canada",
            "Western Canada",
            "Western Russia",
            "Northern Canada",
            "Lake",
            "Black Sea",
            "Baltic Sea",
            "Western Mediterranean",
            "Central Mediterranean",
            "Eastern Mediterranean",
            "South China Sea",
            "Philippine Sea",
            "Moluccas",
            "Java Sea",
            "Coral Sea",
            "East China Sea",
            "North Sea",
            "Northeast Atlantic",
            "North Atlatic",
            "Northwest Atlantic",
            "Ice Sea",
            "Tasman Sea",
            "Carribean",
            "Great Australian Bight",
            "Sargassos",
            "Mexican Gulf",
            "Canadian Arctic",
            "Grand Banks Shelf",
            "Eastern Seaboard",
            "Central Mid-Atlantic Ridge",
            "Bismarck Sea",
            "Bay of Bengal",
            "Bering Sea",
            "Central Pacific Ocean",
            "East Indian Ocean",
            "Home Islands",
            "North Pacific Ocean",
            "Sea of Okhotsk",
            "West Coast of North America",
            "Arabian Sea",
            "Cape of Good Hope",
            "Coast of Madagascar",
            "East Pacific Ocean",
            "Mozambique Channel",
            "South Pacific Ocean",
            "South Indian Ocean",
            "West Indian Ocean",
            "Pacific-Antarctic Ridge",
            "Southeast Pacific Ocean",
            "West Coast of South America",
            "Southeast Atlantic Ocean",
            "Southwest Atlantic Ocean",
            "Western Ice Sea",
            "Brazilian Coast",
            "Southcentral Atlantic",
            "Eastern Atlantic",
            "Celtic Sea",
            "Iberian West Coast"
        };

        /// <summary>
        ///     Local name
        /// </summary>
        private static readonly string[] RegionNames =
        {
            "REG_-",
            "REGION_AFGHANISTAN",
            "REGION_ALASKA",
            "REGION_ALGERIA",
            "REGION_AMAZONAS",
            "REGION_AMERICAN_PACIFIC",
            "REGION_ANATOLIA",
            "REGION_ANDES",
            "REGION_ASIAN_PACIFIC",
            "REGION_AUSTRALIA",
            "REGION_AUSTRIA",
            "REGION_BALKAN",
            "REGION_BALTIC_STATES",
            "REGION_BELARUS",
            "REGION_BENELUX",
            "REGION_BISMARCK_ARCHIPELAGO",
            "REGION_BRAZIL",
            "REGION_BRAZILIAN_HIGHLANDS",
            "REGION_CANADA",
            "REGION_CARIBBEAN",
            "REGION_CENTRAL_AFRICA",
            "REGION_CENTRAL_AMERICA",
            "REGION_CENTRAL_ASIA",
            "REGION_CHINA",
            "REGION_CZECHOSLOVAKIA",
            "REGION_DENMARK",
            "REGION_DIEGO_GARCIA_ISLAND",
            "REGION_EAST_AFRICA",
            "REGION_EASTERN_RUSSIA",
            "REGION_EGYPT",
            "REGION_ENGLAND",
            "REGION_ETHIOPIA",
            "REGION_FAR_EAST_SIBERIA",
            "REGION_FINLAND",
            "REGION_FRANCE",
            "REGION_GERMANY",
            "REGION_GRAN_CHACO",
            "REGION_GRAND_COLOMBIA",
            "REGION_GREENLAND",
            "REGION_HORN_OF_AFRICA",
            "REGION_HUNGARY",
            "REGION_ICELAND",
            "REGION_INDIA",
            "REGION_INDOCHINE",
            "REGION_INDONESIA",
            "REGION_IRELAND",
            "REGION_IRKUTSK",
            "REGION_ITALY",
            "REGION_JAPAN",
            "REGION_KAUKASUS",
            "REGION_KAZAKSTAN",
            "REGION_KOREA",
            "REGION_KRASNOYARSK_",
            "REGION_LIBYA",
            "REGION_MADAGASCAR",
            "REGION_MEXICO",
            "REGION_MIDDLE_EAST",
            "REGION_MIDWEST_US",
            "REGION_MOROCCO",
            "REGION_NEW_GUINEA",
            "REGION_NORTH_SOLOMON_ISLANDS",
            "REGION_NORTHEAST_US_",
            "REGION_NORTHERN_PERMAFROST",
            "REGION_NORTHERN_RUSSIA",
            "REGION_NORTHWEST_US",
            "REGION_NORWAY",
            "REGION_NOVOSIBIRSK",
            "REGION_PATAGONIA",
            "REGION_PERSIA",
            "REGION_PHILIPPINES",
            "REGION_POLAND",
            "REGION_PORTUGAL",
            "REGION_RIO_DE_LA_PLATA",
            "REGION_ROMANIA",
            "REGION_RUSSIA",
            "REGION_SAHARA",
            "REGION_SCOTLAND",
            "REGION_SOUTH_AFRICA",
            "REGION_SOUTH_SOLOMON_ISLANDS",
            "REGION_SOUTHCENTRAL_US",
            "REGION_SOUTHEAST_US",
            "REGION_SOUTHERN_RUSSIA",
            "REGION_SOUTHWEST_AFRICA",
            "REGION_SOUTHWEST_US",
            "REGION_SPAIN",
            "REGION_SUDAN",
            "REGION_SWEDEN",
            "REGION_SWITZERLAND",
            "REGION_TOMSK",
            "REGION_TUNISIA",
            "REGION_UKRAINE",
            "REGION_URALS",
            "REGION_WEST_AFRICA",
            "REGION_WHITE_SEA_TUNDRA",
            "REGION_EASTERN_CANADA",
            "REGION_WESTERN_CANADA",
            "REGION_WESTERN_RUSSIA",
            "REGION_NORTHERN_CANADA",
            "REG_LAKE",
            "SEAREGION_BLACK_SEA",
            "SEAREGION_BALTIC_SEA",
            "SEAREGION_WESTERN_MEDITERRANEAN",
            "SEAREGION_CENTRAL_MEDITERRANEAN",
            "SEAREGION_EASTERN_MEDITERRANEAN",
            "SEAREGION_SOUTH_CHINA_SEA",
            "SEAREGION_PHILIPPINE_SEA",
            "SEAREGION_MOLUCCAS",
            "SEAREGION_JAVA_SEA",
            "SEAREGION_CORAL_SEA",
            "SEAREGION_EAST_CHINA_SEA",
            "SEAREGION_NORTH_SEA",
            "SEAREGION_NORTHEAST_ATLANTIC",
            "SEAREGION_NORTH_ATLANTIC",
            "SEAREGION_NORTHWEST_ATLANTIC",
            "SEAREGION_ICE_SEA",
            "SEAREGION_TASMAN_SEA",
            "SEAREGION_CARRIBEAN",
            "SEAREGION_GREAT_AUSTRALIAN_BIGHT",
            "SEAREGION_SARGASSOS",
            "SEAREGION_MEXICAN_GULF",
            "SEAREGION_CANADIAN_ARCTIC",
            "SEAREGION_GRAND_BANKS_SHELF",
            "SEAREGION_EASTERN_SEABOARD",
            "SEAREGION_CENTRAL_MID_ATLANTIC_RIDGE",
            "SEAREGION_BISMARCK_SEA",
            "SEAREGION_BAY_OF_BENGAL",
            "SEAREGION_BERING_SEA",
            "SEAREGION_CENTRAL_PACIFIC_OCEAN",
            "SEAREGION_EAST_INDIAN_OCEAN",
            "SEAREGION_HOME_ISLANDS",
            "SEAREGION_NORTH_PACIFIC_OCEAN",
            "SEAREGION_SEA_OF_OKHOTSK",
            "SEAREGION_WEST_COAST_OF_NORTH_AMERICA",
            "SEAREGION_ARABIAN_SEA",
            "SEAREGION_CAPE_OF_GOOD_HOPE",
            "SEAREGION_COAST_OF_MADAGASCAR",
            "SEAREGION_EAST_PACIFIC_OCEAN",
            "SEAREGION_MOZAMBIQUE_CHANNEL",
            "SEAREGION_SOUTH_PACIFIC_OCEAN",
            "SEAREGION_SOUTH_INDIAN_OCEAN",
            "SEAREGION_WEST_INDIAN_OCEAN",
            "SEAREGION_PACIFIC_ANTARCTIC_RIDGE",
            "SEAREGION_SOUTHEAST_PACIFIC_OCEAN",
            "SEAREGION_WEST_COAST_OF_SOUTH_AMERICA",
            "SEAREGION_SOUTHEAST_ATLANTIC_OCEAN",
            "SEAREGION_SOUTHWEST_ATLANTIC_OCEAN",
            "SEAREGION_WESTERN_ICE_SEA",
            "SEAREGION_BRAZILIAN_COAST",
            "SEAREGION_SOUTHCENTRAL_ATLANTIC",
            "SEAREGION_EASTERN_ATLANTIC",
            "SEAREGION_CELTIC_SEA",
            "SEAREGION_IBERIAN_WEST_COAST"
        };

        /// <summary>
        ///     Continent string
        /// </summary>
        private static readonly string[] ContinentStrings =
        {
            "-",
            "North America",
            "South America",
            "Europe",
            "Asia",
            "Oceania",
            "Africa",
            "Lake",
            "Atlantic Ocean",
            "Pacific Ocean",
            "Indian Ocean"
        };

        /// <summary>
        ///     Continent name
        /// </summary>
        private static readonly string[] ContinentNames =
        {
            "CON_-",
            "CON_NAmerica",
            "CON_SAmerica",
            "CON_Europe",
            "CON_Asia",
            "CON_Oceania",
            "CON_Africa",
            "CON_Lake",
            "CON_AtlanticOcean",
            "CON_PacificOcean",
            "CON_IndianOcean"
        };

        /// <summary>
        ///     Climate string
        /// </summary>
        private static readonly string[] ClimateStrings =
        {
            "-",
            "Arctic",
            "Subarctic",
            "Temperate",
            "Muddy",
            "Mediterranean",
            "Subtropical",
            "Tropical",
            "Arid"
        };

        /// <summary>
        ///     Climate name
        /// </summary>
        private static readonly string[] ClimateNames =
        {
            "CLIMATE_-",
            "CLIMATE_ARCTIC",
            "CLIMATE_SUBARCTIC",
            "CLIMATE_TEMPERATE",
            "CLIMATE_MUDDY",
            "CLIMATE_MEDITERRANEAN",
            "CLIMATE_SUBTROPICAL",
            "CLIMATE_TROPICAL",
            "CLIMATE_ARID"
        };

        /// <summary>
        ///     Terrain string
        /// </summary>
        private static readonly string[] TerrainStrings =
        {
            "Plains",
            "Forest",
            "Mountain",
            "Desert",
            "Marsh",
            "Hills",
            "Jungle",
            "Urban",
            "Ocean",
            "River",
            "Terra incognito",
            "Unknown",
            "Clear"
        };

        /// <summary>
        ///     Topography name
        /// </summary>
        private static readonly string[] TerrainNames =
        {
            "TERRAIN_PLAINS",
            "TERRAIN_FOREST",
            "TERRAIN_MOUNTAIN",
            "TERRAIN_DESERT",
            "TERRAIN_MARSH",
            "TERRAIN_HILLS",
            "TERRAIN_JUNGLE",
            "TERRAIN_URBAN",
            "TERRAIN_OCEAN",
            "TERRAIN_RIVER",
            "TERRAIN_TERRA_INCOGNITO",
            "TERRAIN_NONE",
            "TERRAIN_PLAINS"
        };

        /// <summary>
        ///     Available continents ID (HoI2)
        /// </summary>
        private static readonly ContinentId[] ContinentsHoI2 =
        {
            ContinentId.None,
            ContinentId.NorthAmerica,
            ContinentId.SouthAmerica,
            ContinentId.Europe,
            ContinentId.Asia,
            ContinentId.Oceania,
            ContinentId.Africa
        };

        /// <summary>
        ///     Available regions ID (HoI2)
        /// </summary>
        private static readonly RegionId[] RegionsHoI2 =
        {
            RegionId.None,
            RegionId.Afghanistan,
            RegionId.Alaska,
            RegionId.Algeria,
            RegionId.Amazonas,
            RegionId.AmericanPacific,
            RegionId.Anatolia,
            RegionId.Andes,
            RegionId.AsianPacific,
            RegionId.Australia,
            RegionId.Austria,
            RegionId.Balkan,
            RegionId.BalticStates,
            RegionId.Belarus,
            RegionId.Benelux,
            RegionId.BismarckArchipelago,
            RegionId.Brazil,
            RegionId.BrazilianHighlands,
            RegionId.Canada,
            RegionId.Caribbean,
            RegionId.CentralAfrica,
            RegionId.CentralAmerica,
            RegionId.CentralAsia,
            RegionId.China,
            RegionId.Czechoslovakia,
            RegionId.Denmark,
            RegionId.DiegoGarciaIsland,
            RegionId.EastAfrica,
            RegionId.Egypt,
            RegionId.England,
            RegionId.Ethiopia,
            RegionId.FarEastSiberia,
            RegionId.Finland,
            RegionId.France,
            RegionId.Germany,
            RegionId.GranChaco,
            RegionId.GrandColombia,
            RegionId.Greenland,
            RegionId.HornOfAfrica,
            RegionId.Hungary,
            RegionId.Iceland,
            RegionId.India,
            RegionId.Indochine,
            RegionId.Indonesia,
            RegionId.Ireland,
            RegionId.Irkutsk,
            RegionId.Italy,
            RegionId.Japan,
            RegionId.Kaukasus,
            RegionId.Kazakstan,
            RegionId.Korea,
            RegionId.Krasnoyarsk,
            RegionId.Libya,
            RegionId.Madagascar,
            RegionId.Mexico,
            RegionId.MiddleEast,
            RegionId.MidwestUs,
            RegionId.Morocco,
            RegionId.NewGuinea,
            RegionId.NorthSolomonIslands,
            RegionId.NortheastUs,
            RegionId.NorthernPermafrost,
            RegionId.NorthwestUs,
            RegionId.Norway,
            RegionId.Novosibirsk,
            RegionId.Patagonia,
            RegionId.Persia,
            RegionId.Philippines,
            RegionId.Poland,
            RegionId.Portugal,
            RegionId.RioDeLaPlata,
            RegionId.Romania,
            RegionId.Russia,
            RegionId.Sahara,
            RegionId.Scotland,
            RegionId.SouthAfrica,
            RegionId.SouthSolomonIslands,
            RegionId.SouthcentralUs,
            RegionId.SoutheastUs,
            RegionId.SouthwestAfrica,
            RegionId.SouthwestUs,
            RegionId.Spain,
            RegionId.Sudan,
            RegionId.Sweden,
            RegionId.Switzerland,
            RegionId.Tomsk,
            RegionId.Tunisia,
            RegionId.Ukraine,
            RegionId.Urals,
            RegionId.WestAfrica,
            RegionId.WhiteSeaTundra,
            RegionId.EasternCanada,
            RegionId.WesternCanada,
            RegionId.NorthernCanada,
            RegionId.Lake,
            RegionId.BlackSea,
            RegionId.BalticSea,
            RegionId.WesternMediterranean,
            RegionId.CentralMediterranean,
            RegionId.EasternMediterranean,
            RegionId.SouthChinaSea,
            RegionId.PhilippineSea,
            RegionId.Moluccas,
            RegionId.JavaSea,
            RegionId.CoralSea,
            RegionId.EastChinaSea,
            RegionId.NorthSea,
            RegionId.NortheastAtlantic,
            RegionId.NorthAtlatic,
            RegionId.NorthwestAtlantic,
            RegionId.IceSea,
            RegionId.TasmanSea,
            RegionId.Carribean,
            RegionId.GreatAustralianBight,
            RegionId.Sargassos,
            RegionId.MexicanGulf,
            RegionId.CanadianArctic,
            RegionId.GrandBanksShelf,
            RegionId.EasternSeaboard,
            RegionId.CentralMidAtlanticRidge,
            RegionId.BismarckSea,
            RegionId.BayOfBengal,
            RegionId.BeringSea,
            RegionId.CentralPacificOcean,
            RegionId.EastIndianOcean,
            RegionId.HomeIslands,
            RegionId.NorthPacificOcean,
            RegionId.SeaOfOkhotsk,
            RegionId.WestCoastOfNorthAmerica,
            RegionId.ArabianSea,
            RegionId.CapeOfGoodHope,
            RegionId.CoastOfMadagascar,
            RegionId.EastPacificOcean,
            RegionId.MozambiqueChannel,
            RegionId.SouthPacificOcean,
            RegionId.SouthIndianOcean,
            RegionId.WestIndianOcean,
            RegionId.PacificAntarcticRidge,
            RegionId.SoutheastPacificOcean,
            RegionId.WestCoastOfSouthAmerica,
            RegionId.SoutheastAtlanticOcean,
            RegionId.SouthwestAtlanticOcean,
            RegionId.WesternIceSea,
            RegionId.BrazilianCoast,
            RegionId.SouthcentralAtlantic,
            RegionId.EasternAtlantic,
            RegionId.CelticSea,
            RegionId.IberianWestCoast
        };

        /// <summary>
        ///     Available regions ID (DH)
        /// </summary>
        private static readonly RegionId[] RegionsDh =
        {
            RegionId.None,
            RegionId.Afghanistan,
            RegionId.Alaska,
            RegionId.Algeria,
            RegionId.Amazonas,
            RegionId.AmericanPacific,
            RegionId.Anatolia,
            RegionId.Andes,
            RegionId.AsianPacific,
            RegionId.Australia,
            RegionId.Austria,
            RegionId.Balkan,
            RegionId.BalticStates,
            RegionId.Belarus,
            RegionId.Benelux,
            RegionId.BismarckArchipelago,
            RegionId.Brazil,
            RegionId.BrazilianHighlands,
            RegionId.Canada,
            RegionId.Caribbean,
            RegionId.CentralAfrica,
            RegionId.CentralAmerica,
            RegionId.CentralAsia,
            RegionId.China,
            RegionId.Czechoslovakia,
            RegionId.Denmark,
            RegionId.DiegoGarciaIsland,
            RegionId.EastAfrica,
            RegionId.EasternRussia,
            RegionId.Egypt,
            RegionId.England,
            RegionId.Ethiopia,
            RegionId.FarEastSiberia,
            RegionId.Finland,
            RegionId.France,
            RegionId.Germany,
            RegionId.GranChaco,
            RegionId.GrandColombia,
            RegionId.Greenland,
            RegionId.HornOfAfrica,
            RegionId.Hungary,
            RegionId.Iceland,
            RegionId.India,
            RegionId.Indochine,
            RegionId.Indonesia,
            RegionId.Ireland,
            RegionId.Irkutsk,
            RegionId.Italy,
            RegionId.Japan,
            RegionId.Kaukasus,
            RegionId.Kazakstan,
            RegionId.Korea,
            RegionId.Krasnoyarsk,
            RegionId.Libya,
            RegionId.Madagascar,
            RegionId.Mexico,
            RegionId.MiddleEast,
            RegionId.MidwestUs,
            RegionId.Morocco,
            RegionId.NewGuinea,
            RegionId.NorthSolomonIslands,
            RegionId.NortheastUs,
            RegionId.NorthernPermafrost,
            RegionId.NorthernRussia,
            RegionId.NorthwestUs,
            RegionId.Norway,
            RegionId.Novosibirsk,
            RegionId.Patagonia,
            RegionId.Persia,
            RegionId.Philippines,
            RegionId.Poland,
            RegionId.Portugal,
            RegionId.RioDeLaPlata,
            RegionId.Romania,
            RegionId.Russia,
            RegionId.Sahara,
            RegionId.Scotland,
            RegionId.SouthAfrica,
            RegionId.SouthSolomonIslands,
            RegionId.SouthcentralUs,
            RegionId.SoutheastUs,
            RegionId.SouthernRussia,
            RegionId.SouthwestAfrica,
            RegionId.SouthwestUs,
            RegionId.Spain,
            RegionId.Sudan,
            RegionId.Sweden,
            RegionId.Switzerland,
            RegionId.Tomsk,
            RegionId.Tunisia,
            RegionId.Ukraine,
            RegionId.Urals,
            RegionId.WestAfrica,
            RegionId.WhiteSeaTundra,
            RegionId.EasternCanada,
            RegionId.WesternCanada,
            RegionId.WesternRussia,
            RegionId.NorthernCanada,
            RegionId.Lake,
            RegionId.BlackSea,
            RegionId.BalticSea,
            RegionId.WesternMediterranean,
            RegionId.CentralMediterranean,
            RegionId.EasternMediterranean,
            RegionId.SouthChinaSea,
            RegionId.PhilippineSea,
            RegionId.Moluccas,
            RegionId.JavaSea,
            RegionId.CoralSea,
            RegionId.EastChinaSea,
            RegionId.NorthSea,
            RegionId.NortheastAtlantic,
            RegionId.NorthAtlatic,
            RegionId.NorthwestAtlantic,
            RegionId.IceSea,
            RegionId.TasmanSea,
            RegionId.Carribean,
            RegionId.GreatAustralianBight,
            RegionId.Sargassos,
            RegionId.MexicanGulf,
            RegionId.CanadianArctic,
            RegionId.GrandBanksShelf,
            RegionId.EasternSeaboard,
            RegionId.CentralMidAtlanticRidge,
            RegionId.BismarckSea,
            RegionId.BayOfBengal,
            RegionId.BeringSea,
            RegionId.CentralPacificOcean,
            RegionId.EastIndianOcean,
            RegionId.HomeIslands,
            RegionId.NorthPacificOcean,
            RegionId.SeaOfOkhotsk,
            RegionId.WestCoastOfNorthAmerica,
            RegionId.ArabianSea,
            RegionId.CapeOfGoodHope,
            RegionId.CoastOfMadagascar,
            RegionId.EastPacificOcean,
            RegionId.MozambiqueChannel,
            RegionId.SouthPacificOcean,
            RegionId.SouthIndianOcean,
            RegionId.WestIndianOcean,
            RegionId.PacificAntarcticRidge,
            RegionId.SoutheastPacificOcean,
            RegionId.WestCoastOfSouthAmerica,
            RegionId.SoutheastAtlanticOcean,
            RegionId.SouthwestAtlanticOcean,
            RegionId.WesternIceSea,
            RegionId.BrazilianCoast,
            RegionId.SouthcentralAtlantic,
            RegionId.EasternAtlantic,
            RegionId.CelticSea,
            RegionId.IberianWestCoast
        };

        /// <summary>
        ///     Available area ID (HoI2)
        /// </summary>
        private static readonly AreaId[] AreasHoI2 =
        {
            AreaId.None,
            AreaId.Adelaide,
            AreaId.Afghanistan,
            AreaId.Agats,
            AreaId.Alabama,
            AreaId.Alaska,
            AreaId.AlaskanArchipelago,
            AreaId.Albania,
            AreaId.Alberta,
            AreaId.AliceSprings,
            AreaId.AlpesProvence,
            AreaId.Amur,
            AreaId.AnatolianNorthCoast,
            AreaId.AnatolianSouthCoast,
            AreaId.AngolanCoast,
            AreaId.AngolanPlains,
            AreaId.Anhui,
            AreaId.Antilles,
            AreaId.AppennineRidge,
            AreaId.Aquitaine,
            AreaId.ArabianDesert,
            AreaId.Arizona,
            AreaId.Arkansas,
            AreaId.Arkhangelsk,
            AreaId.ArmeniaAzerbaijan,
            AreaId.Astrakhan,
            AreaId.Asuncion,
            AreaId.Attica,
            AreaId.Austria,
            AreaId.Babo,
            AreaId.BadenWurttemberg,
            AreaId.BahamasIslands,
            AreaId.Baleares,
            AreaId.Bavaria,
            AreaId.Bechuanaland,
            AreaId.Bengal,
            AreaId.Bengazi,
            AreaId.Bermuda,
            AreaId.Bessarabia,
            AreaId.Bohemia,
            AreaId.Bombay,
            AreaId.Bonin,
            AreaId.Bosnia,
            AreaId.Bosporus,
            AreaId.Bougainville,
            AreaId.BourgogneChampagne,
            AreaId.Brandenburg,
            AreaId.Brasilia,
            AreaId.BrestLitovsk,
            AreaId.Brisbane,
            AreaId.BritishColumbia,
            AreaId.Brittany,
            AreaId.Bulgaria,
            AreaId.Burma,
            AreaId.Cairns,
            AreaId.California,
            AreaId.CameroonianJungle,
            AreaId.Canarias,
            AreaId.CantabricChain,
            AreaId.CapVerdeIslands,
            AreaId.Cape,
            AreaId.Caracas,
            AreaId.Carolinese,
            AreaId.Catalonia,
            AreaId.Celebes,
            AreaId.CentralAfricaDesert,
            AreaId.CentralAfricaPlains,
            AreaId.CentralFinland,
            AreaId.CentralItaly,
            AreaId.CentralNorway,
            AreaId.CentralRainforests,
            AreaId.CentralSerbia,
            AreaId.CentralSpain,
            AreaId.CentralTurkey,
            AreaId.Chahar,
            AreaId.Chihuahua,
            AreaId.Chile,
            AreaId.ChileanArchipelago,
            AreaId.Colorado,
            AreaId.ConnecticutRhodeIsland,
            AreaId.ContinentalSpain,
            AreaId.Croatia,
            AreaId.Cuba,
            AreaId.DanakilPlateau,
            AreaId.Darwin,
            AreaId.Deccar,
            AreaId.Delhi,
            AreaId.Delta,
            AreaId.Denmark,
            AreaId.DiegoGarciaIsland,
            AreaId.Dnepropretovsk,
            AreaId.EastAndalucia,
            AreaId.EastAtlas,
            AreaId.EastEngland,
            AreaId.EastJava,
            AreaId.EastPersia,
            AreaId.EastPrussia,
            AreaId.EastSerbia,
            AreaId.EasternAnatolia,
            AreaId.EasternGhat,
            AreaId.EasternHungary,
            AreaId.Eire,
            AreaId.ElAlamein,
            AreaId.ElRif,
            AreaId.EspirtuSanto,
            AreaId.Estonia,
            AreaId.EthiopianHighland,
            AreaId.Fiji,
            AreaId.Flanders,
            AreaId.FloresTimor,
            AreaId.Florida,
            AreaId.Fujian,
            AreaId.Gabes,
            AreaId.Gansu,
            AreaId.Georgia,
            AreaId.Georgien,
            AreaId.Goetaland,
            AreaId.GoldCoast,
            AreaId.GreekIsland,
            AreaId.Grodno,
            AreaId.Guadalcanal,
            AreaId.Guangdong,
            AreaId.Guangxi,
            AreaId.Guayana,
            AreaId.GuineanCoast,
            AreaId.Guizhou,
            AreaId.HannoverMagdeburg,
            AreaId.Hawaii,
            AreaId.Hebei,
            AreaId.Hedjaz,
            AreaId.Heilongjiang,
            AreaId.Henan,
            AreaId.Hessen,
            AreaId.Himalayas,
            AreaId.Hispaniola,
            AreaId.HolsteinMecklemburg,
            AreaId.Honshu,
            AreaId.Hubei,
            AreaId.Hunan,
            AreaId.Iceland,
            AreaId.Idaho,
            AreaId.Illinois,
            AreaId.Indiana,
            AreaId.Indochine,
            AreaId.InteriorAlgeria,
            AreaId.InteriorLibya,
            AreaId.Iowa,
            AreaId.Iquitos,
            AreaId.Iraq,
            AreaId.Irkutsk,
            AreaId.IvoryCoast,
            AreaId.Jiangsu,
            AreaId.Jiangxi,
            AreaId.Jilin,
            AreaId.JohnsonIsland,
            AreaId.Kamchatka,
            AreaId.Kansas,
            AreaId.Karelia,
            AreaId.Kassarine,
            AreaId.Kazakstan,
            AreaId.Kazan,
            AreaId.Kentucky,
            AreaId.Kenya,
            AreaId.Khabarovsk,
            AreaId.Kharkov,
            AreaId.Kiev,
            AreaId.KirgizSteppe,
            AreaId.Kirgizistan,
            AreaId.Kirov,
            AreaId.KongoleseJungle,
            AreaId.Kostroma,
            AreaId.Krakow,
            AreaId.Krasnodar,
            AreaId.Krim,
            AreaId.Kurdufan,
            AreaId.Kursk,
            AreaId.Kuybyshev,
            AreaId.KyushuShikoku,
            AreaId.LaPaz,
            AreaId.LaPlata,
            AreaId.Lae,
            AreaId.Lakes,
            AreaId.Lapland,
            AreaId.Latvia,
            AreaId.LebanonSyria,
            AreaId.Leningrad,
            AreaId.Levant,
            AreaId.LeyteIslandGroup,
            AreaId.Liaoning,
            AreaId.LigurianIslands,
            AreaId.Lima,
            AreaId.LimousinAuvergne,
            AreaId.Lithuania,
            AreaId.Loire,
            AreaId.LorraineAlsace,
            AreaId.Louisiana,
            AreaId.LowerArgentine,
            AreaId.LowerCalifornia,
            AreaId.Luzon,
            AreaId.Lvov,
            AreaId.Madagascar,
            AreaId.Magadan,
            AreaId.Magdalena,
            AreaId.MaghrebCoast,
            AreaId.Maine,
            AreaId.Malacka,
            AreaId.MalianDesert,
            AreaId.MalianValleys,
            AreaId.Manaus,
            AreaId.Manitoba,
            AreaId.Maracaibo,
            AreaId.MarcusWake,
            AreaId.Marshalls,
            AreaId.MarylandDelaware,
            AreaId.Massachussets,
            AreaId.MatoGrosso,
            AreaId.Mauretania,
            AreaId.Melbourne,
            AreaId.Melkosopotjnik,
            AreaId.Memel,
            AreaId.MexicoCity,
            AreaId.Michigan,
            AreaId.Midway,
            AreaId.Mindanao,
            AreaId.Minnesota,
            AreaId.Minsk,
            AreaId.Mississippi,
            AreaId.Missouri,
            AreaId.Mocambique,
            AreaId.Molucks,
            AreaId.Mongolia,
            AreaId.Montana,
            AreaId.Morocco,
            AreaId.Moscow,
            AreaId.Nagpur,
            AreaId.Nambia,
            AreaId.Nauru,
            AreaId.Nebraska,
            AreaId.NedreNorrland,
            AreaId.Netherlands,
            AreaId.NewBrunswick,
            AreaId.NewFoundland,
            AreaId.NewHampshire,
            AreaId.NewJersey,
            AreaId.NewMexico,
            AreaId.NewYork,
            AreaId.NewZealand,
            AreaId.Nevada,
            AreaId.Niassa,
            AreaId.Nicaragua,
            AreaId.Niger,
            AreaId.NileValley,
            AreaId.NizhnijNovgorod,
            AreaId.Normandy,
            AreaId.NorthBorneo,
            AreaId.NorthCarolina,
            AreaId.NorthDakota,
            AreaId.NorthEngland,
            AreaId.NorthEthiopia,
            AreaId.NorthGilberts,
            AreaId.NorthIreland,
            AreaId.NorthItaly,
            AreaId.NorthKrasnoyarsk,
            AreaId.NorthMarianes,
            AreaId.NorthNigeria,
            AreaId.NorthPersia,
            AreaId.NorthRhodesia,
            AreaId.NorthRomania,
            AreaId.NorthScotland,
            AreaId.NorthernNorway,
            AreaId.NorthwestTerritories,
            AreaId.NovaScotia,
            AreaId.Novgorod,
            AreaId.Novosibirsk,
            AreaId.NubianDesert,
            AreaId.Odessa,
            AreaId.Oesterbotten,
            AreaId.OestraSvealand,
            AreaId.OevreNorrland,
            AreaId.Ohio,
            AreaId.Oklahoma,
            AreaId.Omsk,
            AreaId.Ontario,
            AreaId.Oran,
            AreaId.Oregon,
            AreaId.Orissa,
            AreaId.Pakistan,
            AreaId.PalawanMindoro,
            AreaId.Palestine,
            AreaId.PanamanRidge,
            AreaId.Paris,
            AreaId.PasDeCalais,
            AreaId.Pennsylvania,
            AreaId.Penza,
            AreaId.Perm,
            AreaId.PersianGulfCoast,
            AreaId.Perth,
            AreaId.Phoenix,
            AreaId.Polotsk,
            AreaId.Polynesia,
            AreaId.PortMoresby,
            AreaId.Portugal,
            AreaId.Poznan,
            AreaId.Primorski,
            AreaId.Pskov,
            AreaId.Pyongyang,
            AreaId.PyreneesLanguedoc,
            AreaId.Qattara,
            AreaId.Qinghai,
            AreaId.Quebec,
            AreaId.Quito,
            AreaId.Rabaul,
            AreaId.Rajastan,
            AreaId.RedSeaCoast,
            AreaId.Rehe,
            AreaId.ReservoireRybinsk,
            AreaId.RioDeJaneiro,
            AreaId.RioDeOro,
            AreaId.Rogachev,
            AreaId.Rostov,
            AreaId.Ryazan,
            AreaId.Sachsen,
            AreaId.SakhalinKuriles,
            AreaId.Samoa,
            AreaId.SaoPaulo,
            AreaId.Saransk,
            AreaId.Sarmi,
            AreaId.Saskatchewan,
            AreaId.Senegambia,
            AreaId.Senjavin,
            AreaId.Seoul,
            AreaId.Shaanxi,
            AreaId.Shaba,
            AreaId.Shandong,
            AreaId.Shanxi,
            AreaId.Siam,
            AreaId.Sichuan,
            AreaId.SicilianGap,
            AreaId.SidiBarrani,
            AreaId.Sirte,
            AreaId.SlaveCoast,
            AreaId.Slovakia,
            AreaId.Smolensk,
            AreaId.Somalia,
            AreaId.Somaliland,
            AreaId.SouthBorneo,
            AreaId.SouthCarolina,
            AreaId.SouthDakota,
            AreaId.SouthEthiopia,
            AreaId.SouthFinland,
            AreaId.SouthGilberts,
            AreaId.SouthItaly,
            AreaId.SouthKrasnoyarsk,
            AreaId.SouthNigeria,
            AreaId.SouthPersia,
            AreaId.SouthRhodesia,
            AreaId.SouthRomania,
            AreaId.SouthScotland,
            AreaId.SouthcentralNorway,
            AreaId.SouthernNorway,
            AreaId.StHelena,
            AreaId.Stalingrad,
            AreaId.SuezChannelArea,
            AreaId.SuiyuanNingxia,
            AreaId.Sumatra,
            AreaId.SundaIslands,
            AreaId.Sverdlovsk,
            AreaId.Switzerland,
            AreaId.Sydney,
            AreaId.Syzran,
            AreaId.Tadzjikistan,
            AreaId.Taiwan,
            AreaId.TajmyrEvenk,
            AreaId.Tambov,
            AreaId.Tanganyika,
            AreaId.Tennessee,
            AreaId.Texas,
            AreaId.TheAzores,
            AreaId.TheFalklands,
            AreaId.Thrace,
            AreaId.Tibet,
            AreaId.TierraDelFuego,
            AreaId.Tobruk,
            AreaId.Tohoku,
            AreaId.Tomsk,
            AreaId.Transjordan,
            AreaId.Transnistria,
            AreaId.TransvaalNatal,
            AreaId.Transylvania,
            AreaId.Tunis,
            AreaId.Turkestan,
            AreaId.Turkmenistan,
            AreaId.Tyumen,
            AreaId.Ufa,
            AreaId.Uganda,
            AreaId.UpperArgentine,
            AreaId.UpperNileValley,
            AreaId.Utah,
            AreaId.Uzbekistan,
            AreaId.VaestraSvealand,
            AreaId.Wallonia,
            AreaId.Warsaw,
            AreaId.Washington,
            AreaId.VelikiyeLuki,
            AreaId.VeraCruz,
            AreaId.Vermont,
            AreaId.WestAndalucia,
            AreaId.WestAtlas,
            AreaId.WestEngland,
            AreaId.WestJava,
            AreaId.WestVirginia,
            AreaId.WesternDesert,
            AreaId.WesternGhat,
            AreaId.WesternHungary,
            AreaId.Westphalen,
            AreaId.Virginia,
            AreaId.Wisconsin,
            AreaId.Vladimir,
            AreaId.Volta,
            AreaId.Wyoming,
            AreaId.Xikang,
            AreaId.Xinjiang,
            AreaId.Yakutsk,
            AreaId.YemeniteMountains,
            AreaId.Yukon,
            AreaId.Yunnan,
            AreaId.Zabaikalye,
            AreaId.Zhejiang,
            AreaId.Ryukyusland,
            AreaId.NorthBismarckArchipelago,
            AreaId.NorthNewHebrides,
            AreaId.SouthNewHebrides,
            AreaId.CentralSolomons,
            AreaId.WestAleutians,
            AreaId.Ceylon,
            AreaId.Hokkaido,
            AreaId.NorthBurma,
            AreaId.EastBengal,
            AreaId.KraPeninsula,
            AreaId.Tripoli,
            AreaId.Greenland,
            AreaId.Lake,
            AreaId.AdriaticSea,
            AreaId.AegeanSea,
            AreaId.BlackSea,
            AreaId.RedSea,
            AreaId.TyrrhenianSea,
            AreaId.BothnianBay,
            AreaId.NorthernBaltic,
            AreaId.SouthernBaltic,
            AreaId.Kattegat,
            AreaId.BarentsSea,
            AreaId.LuzonStrait,
            AreaId.PhilipineTrench,
            AreaId.SuluSea,
            AreaId.CelebesSea,
            AreaId.CoastOfBrunei,
            AreaId.FloresSea,
            AreaId.MoluccaSea,
            AreaId.BandaSea,
            AreaId.WestCoralSea,
            AreaId.ArafuraSea,
            AreaId.JavaRidge,
            AreaId.MalaccaStrait,
            AreaId.GulfOfSiam,
            AreaId.SpratlySea,
            AreaId.CoastOfIndochina,
            AreaId.TaiwanStrait,
            AreaId.IrishSea,
            AreaId.EnglishChannel,
            AreaId.DenmarkStrait,
            AreaId.SoutheastNorthsea,
            AreaId.WestNorthsea,
            AreaId.CentralNorthsea,
            AreaId.NorwegianSea,
            AreaId.CostaDelSol,
            AreaId.AlgerianCoast,
            AreaId.EgyptianCoast,
            AreaId.GulfOfLyon,
            AreaId.SeaOfJapan,
            AreaId.NorthBismarckSea,
            AreaId.SouthBismarckSea,
            AreaId.Marianas,
            AreaId.MarshallsSea,
            AreaId.WesternSolomons,
            AreaId.EasternSolomons,
            AreaId.EastCoralSea,
            AreaId.CoastOfJapan,
            AreaId.WesternApproaches,
            AreaId.GreenlandCoast,
            AreaId.FaroesGap,
            AreaId.NorthAtlantic,
            AreaId.BayOfBiscay,
            AreaId.Azores,
            AreaId.PortugeseCoast,
            AreaId.CapStVincent,
            AreaId.CoastOfBrazil,
            AreaId.CapVerde,
            AreaId.GulfOfGuinea,
            AreaId.CentralAtlantic,
            AreaId.CoastOfAfrica,
            AreaId.CoastOfBissao,
            AreaId.SolomonSea,
            AreaId.NorthTasmanSea,
            AreaId.SouthTasmanSea,
            AreaId.EastGreatAustralianBight,
            AreaId.WestGreatAustralianBight,
            AreaId.HudsonBay,
            AreaId.PersianGulf,
            AreaId.YucatanStrait,
            AreaId.CentralCarribean,
            AreaId.WindwardIslands,
            AreaId.WestGulfOfMexico,
            AreaId.EastGulfOfMexico,
            AreaId.FloridaStrait,
            AreaId.BermudaTriangle,
            AreaId.NorthernSeaOfLabrador,
            AreaId.SouthernSeaOfLabrador,
            AreaId.GrandBanks,
            AreaId.TheSeamounts,
            AreaId.CanadianMaritimes,
            AreaId.UsMaritimes,
            AreaId.GuianaBasin,
            AreaId.SouthCentralMidAtlanticRidge,
            AreaId.CentralMidAtlanticRidge,
            AreaId.Aleutians,
            AreaId.Carolines,
            AreaId.CentralPhilippineSea,
            AreaId.CoastOfCeylon,
            AreaId.CoastOfKamchatka,
            AreaId.EastBayOfBengal,
            AreaId.EastBeringSea,
            AreaId.EastMarianaBasin,
            AreaId.EastSeaOfOkhotsk,
            AreaId.GulfOfAlaska,
            AreaId.HawaiianRise,
            AreaId.JavaTrench,
            AreaId.MarianaTrench,
            AreaId.MidPacificMountains,
            AreaId.NinetyeastRidge,
            AreaId.NorthNortheastPacificBasin,
            AreaId.NorthwestPacificBasin,
            AreaId.Ryukyus,
            AreaId.SouthSeaOfOkhotsk,
            AreaId.TimorSea,
            AreaId.WestBayOfBengal,
            AreaId.WestBeringSea,
            AreaId.WestCoastOfAustralia,
            AreaId.WestCoastOfMexico,
            AreaId.WestCoastOfTheUnitedStates,
            AreaId.WestSeaOfOkhotsk,
            AreaId.YellowSea,
            AreaId.AtlanticIndianRidge,
            AreaId.CoastOfSouthAfrica,
            AreaId.FijiBasin,
            AreaId.Gilberts,
            AreaId.LineIslands,
            AreaId.MascarenePlateau,
            AreaId.MidIndianRidge,
            AreaId.NorthArabianSea,
            AreaId.NorthEastPacificOcean,
            AreaId.NorthMozambiqueChannel,
            AreaId.NortheastCoastOfMadagascar,
            AreaId.SouthArabianSea,
            AreaId.SouthEastPacificOcean,
            AreaId.SouthMozambiqueChannel,
            AreaId.SoutheastCoastOfMadagascar,
            AreaId.SoutheastIndianOcean,
            AreaId.SouthwestIndianOcean,
            AreaId.EastCostOfNewZealand,
            AreaId.NorthSoutheastPacificOcean,
            AreaId.SouthSoutheastPacificOcean,
            AreaId.SoutheastPacificBasin,
            AreaId.SouthwestPacificBasin,
            AreaId.WestCoastOfCentralAmerica,
            AreaId.WestCoastOfChile,
            AreaId.WestCoastOfPeru,
            AreaId.HornOfAfrica,
            AreaId.CoastOfAngolaNamibia,
            AreaId.AngolaPlain,
            AreaId.ArgentinePlain,
            AreaId.CoastOfArgentina,
            AreaId.CoastOfUruguay,
            AreaId.CoastOfNewGuinea,
            AreaId.CoastOfGuyana,
            AreaId.CoastOfRecife,
            AreaId.CapeFinisterre,
            AreaId.ArcticOcean,
            AreaId.TheHebreides,
            AreaId.IrishWestCoast,
            AreaId.PernambucoPlain,
            AreaId.AscensionFractureZone,
            AreaId.EastNorthSea
        };

        /// <summary>
        ///     Available area ID (DH)
        /// </summary>
        private static readonly AreaId[] AreasDh =
        {
            AreaId.None,
            AreaId.Adelaide,
            AreaId.Afghanistan,
            AreaId.Agats,
            AreaId.Alabama,
            AreaId.Alaska,
            AreaId.AlaskanArchipelago,
            AreaId.Albania,
            AreaId.Alberta,
            AreaId.AliceSprings,
            AreaId.AlpesProvence,
            AreaId.Amur,
            AreaId.AnatolianNorthCoast,
            AreaId.AnatolianSouthCoast,
            AreaId.AngolanCoast,
            AreaId.AngolanPlains,
            AreaId.Anhui,
            AreaId.Antilles,
            AreaId.AppennineRidge,
            AreaId.Aquitaine,
            AreaId.ArabianDesert,
            AreaId.Arizona,
            AreaId.Arkansas,
            AreaId.Arkhangelsk,
            AreaId.ArmeniaAzerbaijan,
            AreaId.Astrakhan,
            AreaId.Asuncion,
            AreaId.Attica,
            AreaId.Austria,
            AreaId.Babo,
            AreaId.BadenWurttemberg,
            AreaId.BahamasIslands,
            AreaId.Baleares,
            AreaId.Belgorod,
            AreaId.Bavaria,
            AreaId.Bechuanaland,
            AreaId.Bengal,
            AreaId.Bengazi,
            AreaId.Bermuda,
            AreaId.Bessarabia,
            AreaId.Bohemia,
            AreaId.Bombay,
            AreaId.Bonin,
            AreaId.Bosnia,
            AreaId.Bosporus,
            AreaId.Bougainville,
            AreaId.BourgogneChampagne,
            AreaId.Brandenburg,
            AreaId.Brasilia,
            AreaId.BrestLitovsk,
            AreaId.Brisbane,
            AreaId.BritishColumbia,
            AreaId.Brittany,
            AreaId.Bulgaria,
            AreaId.Burma,
            AreaId.Cairns,
            AreaId.California,
            AreaId.CameroonianJungle,
            AreaId.Canarias,
            AreaId.CantabricChain,
            AreaId.CapVerdeIslands,
            AreaId.Cape,
            AreaId.Caracas,
            AreaId.Carolinese,
            AreaId.Catalonia,
            AreaId.Celebes,
            AreaId.CentralAfricaDesert,
            AreaId.CentralAfricaPlains,
            AreaId.CentralFinland,
            AreaId.CentralItaly,
            AreaId.CentralNorway,
            AreaId.CentralRainforests,
            AreaId.CentralSerbia,
            AreaId.CentralSpain,
            AreaId.CentralTurkey,
            AreaId.Chahar,
            AreaId.Chihuahua,
            AreaId.Chile,
            AreaId.ChileanArchipelago,
            AreaId.Colorado,
            AreaId.ConnecticutRhodeIsland,
            AreaId.ContinentalSpain,
            AreaId.Croatia,
            AreaId.Cuba,
            AreaId.DanakilPlateau,
            AreaId.Darwin,
            AreaId.Deccar,
            AreaId.Delhi,
            AreaId.Delta,
            AreaId.Denmark,
            AreaId.DiegoGarciaIsland,
            AreaId.Dnepropretovsk,
            AreaId.EastAndalucia,
            AreaId.EastAtlas,
            AreaId.EastEngland,
            AreaId.EastJava,
            AreaId.EastPersia,
            AreaId.EastPrussia,
            AreaId.EastSerbia,
            AreaId.EasternAnatolia,
            AreaId.EasternGhat,
            AreaId.EasternHungary,
            AreaId.Eire,
            AreaId.Engels,
            AreaId.ElAlamein,
            AreaId.ElRif,
            AreaId.EspirtuSanto,
            AreaId.Estonia,
            AreaId.EthiopianHighland,
            AreaId.Fiji,
            AreaId.Flanders,
            AreaId.FloresTimor,
            AreaId.Florida,
            AreaId.Fujian,
            AreaId.Gabes,
            AreaId.Gansu,
            AreaId.Georgia,
            AreaId.Georgien,
            AreaId.Goetaland,
            AreaId.GoldCoast,
            AreaId.GreekIsland,
            AreaId.Grodno,
            AreaId.Groznyi,
            AreaId.Guadalcanal,
            AreaId.Guangdong,
            AreaId.Guangxi,
            AreaId.Guayana,
            AreaId.GuineanCoast,
            AreaId.Guizhou,
            AreaId.HannoverMagdeburg,
            AreaId.Hawaii,
            AreaId.Hebei,
            AreaId.Hedjaz,
            AreaId.Heilongjiang,
            AreaId.Henan,
            AreaId.Hessen,
            AreaId.Himalayas,
            AreaId.Hispaniola,
            AreaId.HolsteinMecklemburg,
            AreaId.Honshu,
            AreaId.Hubei,
            AreaId.Hunan,
            AreaId.Iceland,
            AreaId.Idaho,
            AreaId.Illinois,
            AreaId.Indiana,
            AreaId.Indochine,
            AreaId.InteriorAlgeria,
            AreaId.InteriorLibya,
            AreaId.Iowa,
            AreaId.Iquitos,
            AreaId.Iraq,
            AreaId.Irkutsk,
            AreaId.IvoryCoast,
            AreaId.Jiangsu,
            AreaId.Jiangxi,
            AreaId.Jilin,
            AreaId.JohnsonIsland,
            AreaId.Kamchatka,
            AreaId.Kansas,
            AreaId.Karelia,
            AreaId.Kassarine,
            AreaId.Kazakstan,
            AreaId.Kazan,
            AreaId.Kentucky,
            AreaId.Kenya,
            AreaId.Khabarovsk,
            AreaId.Kharkov,
            AreaId.Kiev,
            AreaId.KirgizSteppe,
            AreaId.Kirgizistan,
            AreaId.Kirov,
            AreaId.Kola,
            AreaId.KongoleseJungle,
            AreaId.Kostroma,
            AreaId.Krakow,
            AreaId.Krasnodar,
            AreaId.Krim,
            AreaId.Kurdufan,
            AreaId.Kursk,
            AreaId.Kuybyshev,
            AreaId.KyushuShikoku,
            AreaId.LaPaz,
            AreaId.LaPlata,
            AreaId.Lae,
            AreaId.Lakes,
            AreaId.Lapland,
            AreaId.Latvia,
            AreaId.LebanonSyria,
            AreaId.Leningrad,
            AreaId.Levant,
            AreaId.LeyteIslandGroup,
            AreaId.Liaoning,
            AreaId.LigurianIslands,
            AreaId.Lima,
            AreaId.LimousinAuvergne,
            AreaId.Lithuania,
            AreaId.Loire,
            AreaId.LorraineAlsace,
            AreaId.Louisiana,
            AreaId.LowerArgentine,
            AreaId.LowerCalifornia,
            AreaId.Luzon,
            AreaId.Lvov,
            AreaId.Madagascar,
            AreaId.Magadan,
            AreaId.Magdalena,
            AreaId.MaghrebCoast,
            AreaId.Maine,
            AreaId.Malacka,
            AreaId.MalianDesert,
            AreaId.MalianValleys,
            AreaId.Manaus,
            AreaId.Manitoba,
            AreaId.Maracaibo,
            AreaId.MarcusWake,
            AreaId.Marshalls,
            AreaId.MarylandDelaware,
            AreaId.Massachussets,
            AreaId.MatoGrosso,
            AreaId.Mauretania,
            AreaId.Melbourne,
            AreaId.Melkosopotjnik,
            AreaId.Memel,
            AreaId.MexicoCity,
            AreaId.Michigan,
            AreaId.Midway,
            AreaId.Mindanao,
            AreaId.Minnesota,
            AreaId.Minsk,
            AreaId.Mississippi,
            AreaId.Missouri,
            AreaId.Mocambique,
            AreaId.Molucks,
            AreaId.Mongolia,
            AreaId.Montana,
            AreaId.Morocco,
            AreaId.Moscow,
            AreaId.Mozyr,
            AreaId.Nagpur,
            AreaId.Nalchik,
            AreaId.Nambia,
            AreaId.Nauru,
            AreaId.Nebraska,
            AreaId.NedreNorrland,
            AreaId.Netherlands,
            AreaId.NewBrunswick,
            AreaId.NewFoundland,
            AreaId.NewHampshire,
            AreaId.NewJersey,
            AreaId.NewMexico,
            AreaId.NewYork,
            AreaId.NewZealand,
            AreaId.Nevada,
            AreaId.Niassa,
            AreaId.Nicaragua,
            AreaId.Niger,
            AreaId.NileValley,
            AreaId.NizhnijNovgorod,
            AreaId.Normandy,
            AreaId.NorthBorneo,
            AreaId.NorthCarolina,
            AreaId.NorthDakota,
            AreaId.NorthEngland,
            AreaId.NorthEthiopia,
            AreaId.NorthGilberts,
            AreaId.NorthIreland,
            AreaId.NorthItaly,
            AreaId.NorthKrasnoyarsk,
            AreaId.NorthMarianes,
            AreaId.NorthNigeria,
            AreaId.NorthPersia,
            AreaId.NorthRhodesia,
            AreaId.NorthRomania,
            AreaId.NorthScotland,
            AreaId.NorthernNorway,
            AreaId.NorthwestTerritories,
            AreaId.NovaScotia,
            AreaId.Novgorod,
            AreaId.Novosibirsk,
            AreaId.NubianDesert,
            AreaId.Odessa,
            AreaId.Oesterbotten,
            AreaId.OestraSvealand,
            AreaId.OevreNorrland,
            AreaId.Ohio,
            AreaId.Oklahoma,
            AreaId.Omsk,
            AreaId.Ontario,
            AreaId.Oran,
            AreaId.Oregon,
            AreaId.Orenburg,
            AreaId.Orissa,
            AreaId.Pakistan,
            AreaId.PalawanMindoro,
            AreaId.Palestine,
            AreaId.PanamanRidge,
            AreaId.Paris,
            AreaId.PasDeCalais,
            AreaId.Pennsylvania,
            AreaId.Penza,
            AreaId.Perm,
            AreaId.PersianGulfCoast,
            AreaId.Perth,
            AreaId.Petrozavodsk,
            AreaId.Phoenix,
            AreaId.Polotsk,
            AreaId.Poltava,
            AreaId.Polynesia,
            AreaId.PortMoresby,
            AreaId.Portugal,
            AreaId.Poznan,
            AreaId.Primorski,
            AreaId.Pskov,
            AreaId.Pyongyang,
            AreaId.PyreneesLanguedoc,
            AreaId.Qattara,
            AreaId.Qinghai,
            AreaId.Quebec,
            AreaId.Quito,
            AreaId.Rabaul,
            AreaId.Rajastan,
            AreaId.RedSeaCoast,
            AreaId.Rehe,
            AreaId.ReservoireRybinsk,
            AreaId.Rhineland,
            AreaId.RioDeJaneiro,
            AreaId.RioDeOro,
            AreaId.Rogachev,
            AreaId.Rostov,
            AreaId.Ryazan,
            AreaId.Sachsen,
            AreaId.SakhalinKuriles,
            AreaId.Samoa,
            AreaId.SaoPaulo,
            AreaId.Saransk,
            AreaId.Sarmi,
            AreaId.Saskatchewan,
            AreaId.Senegambia,
            AreaId.Senjavin,
            AreaId.Seoul,
            AreaId.Shaanxi,
            AreaId.Shaba,
            AreaId.Shandong,
            AreaId.Shanxi,
            AreaId.Siam,
            AreaId.Sichuan,
            AreaId.SicilianGap,
            AreaId.SidiBarrani,
            AreaId.Silesia,
            AreaId.Sirte,
            AreaId.SlaveCoast,
            AreaId.Slovakia,
            AreaId.Smolensk,
            AreaId.Somalia,
            AreaId.Somaliland,
            AreaId.SouthBorneo,
            AreaId.SouthCarolina,
            AreaId.SouthDakota,
            AreaId.SouthEthiopia,
            AreaId.SouthFinland,
            AreaId.SouthGilberts,
            AreaId.SouthItaly,
            AreaId.SouthKrasnoyarsk,
            AreaId.SouthNigeria,
            AreaId.SouthPersia,
            AreaId.SouthRhodesia,
            AreaId.SouthRomania,
            AreaId.SouthScotland,
            AreaId.SouthcentralNorway,
            AreaId.SouthernNorway,
            AreaId.StHelena,
            AreaId.Stalingrad,
            AreaId.SuezChannelArea,
            AreaId.SuiyuanNingxia,
            AreaId.Sumatra,
            AreaId.SundaIslands,
            AreaId.Sverdlovsk,
            AreaId.Switzerland,
            AreaId.Sydney,
            AreaId.Syzran,
            AreaId.Tadzjikistan,
            AreaId.Taiwan,
            AreaId.TajmyrEvenk,
            AreaId.Tambov,
            AreaId.Tanganyika,
            AreaId.Tennessee,
            AreaId.Texas,
            AreaId.TheAzores,
            AreaId.TheFalklands,
            AreaId.Thrace,
            AreaId.Tibet,
            AreaId.TierraDelFuego,
            AreaId.Tobruk,
            AreaId.Tohoku,
            AreaId.Tomsk,
            AreaId.Transjordan,
            AreaId.Transnistria,
            AreaId.TransvaalNatal,
            AreaId.Transylvania,
            AreaId.Tunis,
            AreaId.Turkestan,
            AreaId.Turkmenistan,
            AreaId.Tyumen,
            AreaId.Ufa,
            AreaId.Uganda,
            AreaId.UpperArgentine,
            AreaId.UpperNileValley,
            AreaId.Utah,
            AreaId.Uzbekistan,
            AreaId.VaestraSvealand,
            AreaId.Wallonia,
            AreaId.Warsaw,
            AreaId.Washington,
            AreaId.VelikiyeLuki,
            AreaId.VeraCruz,
            AreaId.Vermont,
            AreaId.Vorkuta,
            AreaId.WestAndalucia,
            AreaId.WestAtlas,
            AreaId.WestEngland,
            AreaId.WestJava,
            AreaId.WestVirginia,
            AreaId.WesternDesert,
            AreaId.WesternGhat,
            AreaId.WesternHungary,
            AreaId.Westphalen,
            AreaId.Virginia,
            AreaId.Wisconsin,
            AreaId.Vladimir,
            AreaId.Volta,
            AreaId.Wyoming,
            AreaId.Xikang,
            AreaId.Xinjiang,
            AreaId.Yakutsk,
            AreaId.YemeniteMountains,
            AreaId.Yukon,
            AreaId.Yunnan,
            AreaId.Zabaikalye,
            AreaId.Zhejiang,
            AreaId.Ryukyusland,
            AreaId.NorthBismarckArchipelago,
            AreaId.NorthNewHebrides,
            AreaId.SouthNewHebrides,
            AreaId.CentralSolomons,
            AreaId.WestAleutians,
            AreaId.Ceylon,
            AreaId.Hokkaido,
            AreaId.NorthBurma,
            AreaId.EastBengal,
            AreaId.KraPeninsula,
            AreaId.Tripoli,
            AreaId.Greenland,
            AreaId.Lake,
            AreaId.AdriaticSea,
            AreaId.AegeanSea,
            AreaId.BlackSea,
            AreaId.RedSea,
            AreaId.TyrrhenianSea,
            AreaId.BothnianBay,
            AreaId.NorthernBaltic,
            AreaId.SouthernBaltic,
            AreaId.Kattegat,
            AreaId.BarentsSea,
            AreaId.LuzonStrait,
            AreaId.PhilipineTrench,
            AreaId.SuluSea,
            AreaId.CelebesSea,
            AreaId.CoastOfBrunei,
            AreaId.FloresSea,
            AreaId.MoluccaSea,
            AreaId.BandaSea,
            AreaId.WestCoralSea,
            AreaId.ArafuraSea,
            AreaId.JavaRidge,
            AreaId.MalaccaStrait,
            AreaId.GulfOfSiam,
            AreaId.SpratlySea,
            AreaId.CoastOfIndochina,
            AreaId.TaiwanStrait,
            AreaId.IrishSea,
            AreaId.EnglishChannel,
            AreaId.DenmarkStrait,
            AreaId.SoutheastNorthsea,
            AreaId.WestNorthsea,
            AreaId.CentralNorthsea,
            AreaId.NorwegianSea,
            AreaId.CostaDelSol,
            AreaId.AlgerianCoast,
            AreaId.EgyptianCoast,
            AreaId.GulfOfLyon,
            AreaId.SeaOfJapan,
            AreaId.NorthBismarckSea,
            AreaId.SouthBismarckSea,
            AreaId.Marianas,
            AreaId.MarshallsSea,
            AreaId.WesternSolomons,
            AreaId.EasternSolomons,
            AreaId.EastCoralSea,
            AreaId.CoastOfJapan,
            AreaId.WesternApproaches,
            AreaId.GreenlandCoast,
            AreaId.FaroesGap,
            AreaId.NorthAtlantic,
            AreaId.BayOfBiscay,
            AreaId.Azores,
            AreaId.PortugeseCoast,
            AreaId.CapStVincent,
            AreaId.CoastOfBrazil,
            AreaId.CapVerde,
            AreaId.GulfOfGuinea,
            AreaId.CentralAtlantic,
            AreaId.CoastOfAfrica,
            AreaId.CoastOfBissao,
            AreaId.SolomonSea,
            AreaId.NorthTasmanSea,
            AreaId.SouthTasmanSea,
            AreaId.EastGreatAustralianBight,
            AreaId.WestGreatAustralianBight,
            AreaId.HudsonBay,
            AreaId.PersianGulf,
            AreaId.YucatanStrait,
            AreaId.CentralCarribean,
            AreaId.WindwardIslands,
            AreaId.WestGulfOfMexico,
            AreaId.EastGulfOfMexico,
            AreaId.FloridaStrait,
            AreaId.BermudaTriangle,
            AreaId.NorthernSeaOfLabrador,
            AreaId.SouthernSeaOfLabrador,
            AreaId.GrandBanks,
            AreaId.TheSeamounts,
            AreaId.CanadianMaritimes,
            AreaId.UsMaritimes,
            AreaId.GuianaBasin,
            AreaId.SouthCentralMidAtlanticRidge,
            AreaId.CentralMidAtlanticRidge,
            AreaId.Aleutians,
            AreaId.Carolines,
            AreaId.CentralPhilippineSea,
            AreaId.CoastOfCeylon,
            AreaId.CoastOfKamchatka,
            AreaId.EastBayOfBengal,
            AreaId.EastBeringSea,
            AreaId.EastMarianaBasin,
            AreaId.EastSeaOfOkhotsk,
            AreaId.GulfOfAlaska,
            AreaId.HawaiianRise,
            AreaId.JavaTrench,
            AreaId.MarianaTrench,
            AreaId.MidPacificMountains,
            AreaId.NinetyeastRidge,
            AreaId.NorthNortheastPacificBasin,
            AreaId.NorthwestPacificBasin,
            AreaId.Ryukyus,
            AreaId.SouthSeaOfOkhotsk,
            AreaId.TimorSea,
            AreaId.WestBayOfBengal,
            AreaId.WestBeringSea,
            AreaId.WestCoastOfAustralia,
            AreaId.WestCoastOfMexico,
            AreaId.WestCoastOfTheUnitedStates,
            AreaId.WestSeaOfOkhotsk,
            AreaId.YellowSea,
            AreaId.AtlanticIndianRidge,
            AreaId.CoastOfSouthAfrica,
            AreaId.FijiBasin,
            AreaId.Gilberts,
            AreaId.LineIslands,
            AreaId.MascarenePlateau,
            AreaId.MidIndianRidge,
            AreaId.NorthArabianSea,
            AreaId.NorthEastPacificOcean,
            AreaId.NorthMozambiqueChannel,
            AreaId.NortheastCoastOfMadagascar,
            AreaId.SouthArabianSea,
            AreaId.SouthEastPacificOcean,
            AreaId.SouthMozambiqueChannel,
            AreaId.SoutheastCoastOfMadagascar,
            AreaId.SoutheastIndianOcean,
            AreaId.SouthwestIndianOcean,
            AreaId.EastCostOfNewZealand,
            AreaId.NorthSoutheastPacificOcean,
            AreaId.SouthSoutheastPacificOcean,
            AreaId.SoutheastPacificBasin,
            AreaId.SouthwestPacificBasin,
            AreaId.WestCoastOfCentralAmerica,
            AreaId.WestCoastOfChile,
            AreaId.WestCoastOfPeru,
            AreaId.HornOfAfrica,
            AreaId.CoastOfAngolaNamibia,
            AreaId.AngolaPlain,
            AreaId.ArgentinePlain,
            AreaId.CoastOfArgentina,
            AreaId.CoastOfUruguay,
            AreaId.CoastOfNewGuinea,
            AreaId.CoastOfGuyana,
            AreaId.CoastOfRecife,
            AreaId.CapeFinisterre,
            AreaId.ArcticOcean,
            AreaId.TheHebreides,
            AreaId.IrishWestCoast,
            AreaId.PernambucoPlain,
            AreaId.AscensionFractureZone,
            AreaId.EastNorthSea,
            AreaId.CaspianSea
        };

        /// <summary>
        ///     Available climate ID (HoI2)
        /// </summary>
        private static readonly ClimateId[] ClimatesHoI2 =
        {
            ClimateId.None,
            ClimateId.Arctic,
            ClimateId.Subarctic,
            ClimateId.Temperate,
            ClimateId.Muddy,
            ClimateId.Mediterranean,
            ClimateId.Subtropical,
            ClimateId.Tropical,
            ClimateId.Arid
        };

        /// <summary>
        ///     Available terrain ID (HoI2)
        /// </summary>
        private static readonly TerrainId[] TerrainsHoI2 =
        {
            TerrainId.Unknown,
            TerrainId.Plains,
            TerrainId.Forest,
            TerrainId.Mountain,
            TerrainId.Desert,
            TerrainId.Marsh,
            TerrainId.Hills,
            TerrainId.Jungle,
            TerrainId.Urban,
            TerrainId.Ocean,
            TerrainId.River
        };

        /// <summary>
        ///     Region name replacement table (AoD1.10 from )
        /// </summary>
        private static readonly Dictionary<AreaId, string> ReplacingAreaNamesAod = new Dictionary<AreaId, string>
        {
            { AreaId.AtlanticIndianRidge, "AREA_ATLANTIC-INDIAN_RIDGE" },
            { AreaId.CoastOfAngolaNamibia, "AREA_COAST_OF_ANGOLA-NAMIBIA" },
            { AreaId.MidIndianRidge, "AREA_MID-INDIAN_RIDGE" },
            { AreaId.MidPacificMountains, "AREA_MID-PACIFIC_MOUNTAINS" },
            { AreaId.PernambucoPlain, "AREA_PERNAMBUCO_PLAIN" },
            { AreaId.CanadianMaritimes, "AREA_CANADIAN_MARITIMES" },
            { AreaId.CentralMidAtlanticRidge, "AREA_CENTRAL_MID-ATLANTIC_RIDGE" },
            { AreaId.GrandBanks, "AREA_GRAND_BANKS" },
            { AreaId.GuianaBasin, "AREA_GUIANA_BASIN" },
            { AreaId.NorthernSeaOfLabrador, "AREA_NORTHERN_SEA_OF_LABRADOR" },
            { AreaId.SouthCentralMidAtlanticRidge, "AREA_SOUTH-CENTRAL_MID-ATLANTIC_RIDGE" },
            { AreaId.SouthernSeaOfLabrador, "AREA_SOUTHERN_SEA_OF_LABRADOR" },
            { AreaId.TheSeamounts, "AREA_THE_SEAMOUNTS" },
            { AreaId.UsMaritimes, "AREA_US_MARITIMES" }
        };

        /// <summary>
        ///     Local name replacement table (AoD1.10 from )
        /// </summary>
        private static readonly Dictionary<RegionId, string> ReplacingRegionNamesAod = new Dictionary<RegionId, string>
        {
            { RegionId.CentralMidAtlanticRidge, "SEAREGION_CENTRAL_MID-ATLANTIC_RIDGE" }
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Provinces()
        {
            // Master Providence List
            Items = new List<Province>();

            // Sea area list
            SeaZones = new List<int>();

            // With the sea area ID Correspondence of
            SeaZoneMap = new Dictionary<int, Province>();

            // With a regional string ID Correspondence of
            foreach (AreaId area in Enum.GetValues(typeof (AreaId)))
            {
                AreaStringMap.Add(AreaStrings[(int) area].ToLower(), area);
            }

            // With local strings ID Correspondence of
            foreach (RegionId region in Enum.GetValues(typeof (RegionId)))
            {
                RegionStringMap.Add(RegionStrings[(int) region].ToLower(), region);
            }

            // With continental strings ID Correspondence of
            foreach (ContinentId continent in Enum.GetValues(typeof (ContinentId)))
            {
                ContinentStringMap.Add(ContinentStrings[(int) continent].ToLower(), continent);
            }

            // With climate string ID Correspondence of
            foreach (ClimateId climate in Enum.GetValues(typeof (ClimateId)))
            {
                ClimateStringMap.Add(ClimateStrings[(int) climate].ToLower(), climate);
            }

            // With terrain strings ID Correspondence of
            foreach (TerrainId terrain in Enum.GetValues(typeof (TerrainId)))
            {
                TerrainStringMap.Add(TerrainStrings[(int) terrain].ToLower(), terrain);
            }
        }

        /// <summary>
        ///     Initialization process
        /// </summary>
        public static void Init()
        {
            // Available continents IDInitialization of
            Continents = new List<ContinentId>(ContinentsHoI2);

            // Available regions ID Initialization of
            Regions = new List<RegionId>(Game.Type == GameType.DarkestHour ? RegionsDh : RegionsHoI2);

            // Available area ID Initialization of
            Areas = new List<AreaId>(Game.Type == GameType.DarkestHour ? AreasDh : AreasHoI2);

            // Available climate ID Initialization of
            Climates = new List<ClimateId>(ClimatesHoI2);

            // Available terrain ID Initialization of
            Terrains = new List<TerrainId>(TerrainsHoI2);
        }

        #endregion

        #region File reading

        /// <summary>
        ///     Request a reload of the provision file
        /// </summary>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     Reload the province files
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
        ///     Read the Providence files
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
        ///     Delay reading of provision files
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
        ///     Read the Providence files
        /// </summary>
        private static void LoadFiles()
        {
            Items.Clear();

            // Read the province data in order
            bool error = false;
            string fileName = Game.GetReadFileName(Game.GetProvinceFolderName(), Game.ProvinceFileName);
            try
            {
                LoadFile(fileName);
            }
            catch (Exception)
            {
                error = true;
                Log.Error("[Province] Read error: {0}", fileName);
                MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                    Resources.EditorProvince, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Continent / / Local / / Regional mapping
            AttachProvinces();

            // Update the sea area list
            UpdateSeaZones();

            // Return if reading fails
            if (error)
            {
                return;
            }

            // Set the read flag
            _loaded = true;
        }

        /// <summary>
        ///     Read the provision file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void LoadFile(string fileName)
        {
            Log.Verbose("[Province] Load: {0}", Path.GetFileName(fileName));

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

                while (!lexer.EndOfStream)
                {
                    Province province = ParseLine(lexer);

                    // Skip blank lines
                    if (province == null)
                    {
                        continue;
                    }

                    Items.Add(province);
                }

                ResetDirty();
            }
        }

        /// <summary>
        ///     Interpret the Providence definition line
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Providence data</returns>
        private static Province ParseLine(CsvLexer lexer)
        {
            string[] tokens = lexer.GetTokens();

            // ID Skip lines that are not specified
            if (string.IsNullOrEmpty(tokens?[0]))
            {
                return null;
            }

            // Skip lines with insufficient tokens
            if (tokens.Length < 49)
            {
                Log.Warning("[Province] Invalid token count: {0} ({1} L{2})", tokens.Length, lexer.FileName,
                    lexer.LineNo);
                return null;
            }

            Province province = new Province();
            int index = 0;

            // Providence ID
            int n;
            if (!int.TryParse(tokens[index], out n))
            {
                Log.Warning("[Province] Invalid id: {0} ({1} L{2})", tokens[index], lexer.FileName, lexer.LineNo);
                return null;
            }
            province.Id = n;
            index++;

            // Province name
            province.Name = tokens[index];
            index++;

            // area ID
            string s = tokens[index].ToLower();
            if (string.IsNullOrEmpty(s))
            {
                province.Area = AreaId.None;
            }
            else if (AreaStringMap.ContainsKey(s))
            {
                province.Area = AreaStringMap[s];
            }
            else
            {
                Log.Warning("[Province] Invalid area: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
                province.Area = AreaId.None;
            }
            index++;

            // Local ID
            s = tokens[index].ToLower();
            if (string.IsNullOrEmpty(s))
            {
                province.Region = RegionId.None;
            }
            else if (RegionStringMap.ContainsKey(s))
            {
                province.Region = RegionStringMap[s];
            }
            else
            {
                Log.Warning("[Province] Invalid region: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
                province.Region = RegionId.None;
            }
            index++;

            // Continent ID
            s = tokens[index].ToLower();
            if (string.IsNullOrEmpty(s))
            {
                province.Continent = ContinentId.None;
            }
            else if (ContinentStringMap.ContainsKey(s))
            {
                province.Continent = ContinentStringMap[s];
            }
            else
            {
                Log.Warning("[Province] Invalid continent: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
                province.Continent = ContinentId.None;
            }
            index++;

            // climate ID
            s = tokens[index].ToLower();
            if (string.IsNullOrEmpty(s))
            {
                province.Climate = ClimateId.None;
            }
            else if (ClimateStringMap.ContainsKey(s))
            {
                province.Climate = ClimateStringMap[s];
            }
            else
            {
                Log.Warning("[Province] Invalid climate: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
                province.Climate = ClimateId.None;
            }
            index++;

            // terrain ID
            s = tokens[index].ToLower();
            if (string.IsNullOrEmpty(s))
            {
                province.Terrain = TerrainId.Unknown;
            }
            else if (TerrainStringMap.ContainsKey(s))
            {
                province.Terrain = TerrainStringMap[s];
            }
            else
            {
                Log.Warning("[Province] Invalid terrain: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
                province.Terrain = TerrainId.Unknown;
            }
            index += 3;

            // infrastructure
            double d;
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.Infrastructure = 0;
            }
            else if (DoubleHelper.TryParse(tokens[index], out d))
            {
                province.Infrastructure = d;
            }
            else
            {
                Log.Warning("[Province] Invalid infrastructure: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index += 2;

            // Presence or absence of sandy beach
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.Beaches = false;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.Beaches = n > 0;
            }
            else
            {
                Log.Warning("[Province] Invalid beach: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Presence or absence of a port
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.PortAllowed = false;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.PortAllowed = n > 0;
            }
            else
            {
                Log.Warning("[Province] Invalid port allowed: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Sea area of the harbor
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.PortSeaZone = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.PortSeaZone = n;
            }
            else
            {
                Log.Warning("[Province] Invalid port sea zone: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // I C
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.Ic = 0;
            }
            else if (DoubleHelper.TryParse(tokens[index], out d))
            {
                province.Ic = d;
            }
            else
            {
                Log.Warning("[Province] Invalid ic: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Labor force
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.Manpower = 0;
            }
            else if (DoubleHelper.TryParse(tokens[index], out d))
            {
                province.Manpower = d;
            }
            else
            {
                Log.Warning("[Province] Invalid manpower: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // oil
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.Oil = 0;
            }
            else if (DoubleHelper.TryParse(tokens[index], out d))
            {
                province.Oil = d;
            }
            else
            {
                Log.Warning("[Province] Invalid oil: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // metal
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.Metal = 0;
            }
            else if (DoubleHelper.TryParse(tokens[index], out d))
            {
                province.Metal = d;
            }
            else
            {
                Log.Warning("[Province] Invalid metal: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // energy
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.Energy = 0;
            }
            else if (DoubleHelper.TryParse(tokens[index], out d))
            {
                province.Energy = d;
            }
            else
            {
                Log.Warning("[Province] Invalid energy: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Rare resources
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.RareMaterials = 0;
            }
            else if (DoubleHelper.TryParse(tokens[index], out d))
            {
                province.RareMaterials = d;
            }
            else
            {
                Log.Warning("[Province] Invalid rare materials: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Of city X Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.CityXPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.CityXPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid city posision x: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Of city Y Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.CityYPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.CityYPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid city position y: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Of the armyX Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.ArmyXPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.ArmyXPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid army position x: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Of the army Y Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.ArmyYPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.ArmyYPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid army position y: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Of the harbor X Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.PortXPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.PortXPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid port position x: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Of the harbor Y Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.PortYPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.PortYPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid port position y: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // On the sandy beach X Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.BeachXPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.BeachXPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid beach position x: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // On the sandy beach Y Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.BeachYPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.BeachYPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid beach position y: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Sandy beach icon
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.BeachIcon = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.BeachIcon = n;
            }
            else
            {
                Log.Warning("[Province] Invalid beach icon: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Fortress X Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.FortXPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.FortXPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid fort position x: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Fortress Y Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.FortYPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.FortYPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid fort position y: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Anti-aircraft gun X Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.AaXPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.AaXPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid aa position x: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Anti-aircraft gun Y Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.AaYPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.AaYPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid aa position y: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Of the counter X Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.CounterXPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.CounterXPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid counter position x: {0} [{1}: {2}] ({3} L{4})", tokens[index],
                    province.Id, province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Of the counter Y Coordinate
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.CounterYPos = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.CounterYPos = n;
            }
            else
            {
                Log.Warning("[Province] Invalid counter position y: {0} [{1}: {2}] ({3} L{4})", tokens[index],
                    province.Id, province.Name, lexer.FileName, lexer.LineNo);
            }
            index += 11;

            // fill X Coordinate 1
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.FillCoordX1 = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.FillCoordX1 = n;
            }
            else
            {
                Log.Warning("[Province] Invalid fill position x1: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // fill Y Coordinate 1
            if (string.IsNullOrEmpty(tokens[index]))
            {
                province.FillCoordY1 = 0;
            }
            else if (int.TryParse(tokens[index], out n))
            {
                province.FillCoordY1 = n;
            }
            else
            {
                Log.Warning("[Province] Invalid fill position y1: {0} [{1}: {2}] ({3} L{4})", tokens[index], province.Id,
                    province.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // fill X Coordinate 2
            if (index < tokens.Length)
            {
                if (string.IsNullOrEmpty(tokens[index]))
                {
                    province.FillCoordX2 = 0;
                }
                else if (int.TryParse(tokens[index], out n))
                {
                    province.FillCoordX2 = n;
                }
            }
            index++;

            // fill Y Coordinate 2
            if (index < tokens.Length)
            {
                if (string.IsNullOrEmpty(tokens[index]))
                {
                    province.FillCoordY2 = 0;
                }
                else if (int.TryParse(tokens[index], out n))
                {
                    province.FillCoordY2 = n;
                }
            }
            index++;

            // fill X Coordinate 3
            if (index < tokens.Length)
            {
                if (string.IsNullOrEmpty(tokens[index]))
                {
                    province.FillCoordX3 = 0;
                }
                else if (int.TryParse(tokens[index], out n))
                {
                    province.FillCoordX3 = n;
                }
            }
            index++;

            // fill Y Coordinate 3
            if (index < tokens.Length)
            {
                if (string.IsNullOrEmpty(tokens[index]))
                {
                    province.FillCoordY3 = 0;
                }
                else if (int.TryParse(tokens[index], out n))
                {
                    province.FillCoordY3 = n;
                }
            }
            index++;

            // fill X Coordinate Four
            if (index < tokens.Length)
            {
                if (string.IsNullOrEmpty(tokens[index]))
                {
                    province.FillCoordX4 = 0;
                }
                else if (int.TryParse(tokens[index], out n))
                {
                    province.FillCoordX4 = n;
                }
            }
            index++;

            // fill Y Coordinate Four
            if (index < tokens.Length)
            {
                if (string.IsNullOrEmpty(tokens[index]))
                {
                    province.FillCoordY4 = 0;
                }
                else if (int.TryParse(tokens[index], out n))
                {
                    province.FillCoordY4 = n;
                }
            }
            index++;

            // fill X Coordinate Five
            if (index < tokens.Length)
            {
                if (string.IsNullOrEmpty(tokens[index]))
                {
                    province.FillCoordX5 = 0;
                }
                else if (int.TryParse(tokens[index], out n))
                {
                    province.FillCoordX5 = n;
                }
            }
            index++;

            // fill Y Coordinate Five
            if (index < tokens.Length)
            {
                if (string.IsNullOrEmpty(tokens[index]))
                {
                    province.FillCoordY5 = 0;
                }
                else if (int.TryParse(tokens[index], out n))
                {
                    province.FillCoordY5 = n;
                }
            }
            index++;

            // fillX Coordinate 6
            if (index < tokens.Length)
            {
                if (string.IsNullOrEmpty(tokens[index]))
                {
                    province.FillCoordX6 = 0;
                }
                else if (int.TryParse(tokens[index], out n))
                {
                    province.FillCoordX6 = n;
                }
            }

            return province;
        }

        #endregion

        #region File writing

        /// <summary>
        ///     Save the province definition file
        /// </summary>
        /// <returns>If saving fails false false return it</returns>
        public static bool Save()
        {
            // Wait for completion if loading is in progress
            if (Worker.IsBusy)
            {
                WaitLoading();
            }

            if (IsDirty())
            {
                string folderName = Game.GetWriteFileName(Game.GetProvinceFolderName());
                if (string.IsNullOrEmpty(folderName))
                {
                    return false;
                }

                string fileName = Path.Combine(folderName, Game.ProvinceFileName);
                try
                {
                    // Create a folder if it doesn't exist
                    if (!Directory.Exists(folderName))
                    {
                        Directory.CreateDirectory(folderName);
                    }

                    // Save the province definition file
                    Log.Info("[Province] Save: {0}", Path.GetFileName(fileName));
                    SaveFile(fileName);
                }
                catch (Exception)
                {
                    Log.Error("[Province] Write error: {0}", fileName);
                    MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                        Resources.EditorProvince, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Clear the edited flag
                ResetDirty();
            }

            if (_loaded)
            {
                // When saving only the character string definition, the edited flag of the province name is not cleared, so clear all here.
                foreach (Province province in Items)
                {
                    province.ResetDirtyAll();
                }
            }

            return true;
        }

        /// <summary>
        ///     Save the province definition file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void SaveFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                // Write header line
                writer.WriteLine(
                    "Id;Name;Area;Region;Continent;Climate;Terrain;SizeModifier;AirCapacity;Infrastructure;City;Beaches;Port Allowed;Port Seazone;IC;Manpower;Oil;Metal;Energy;Rare Materials;City XPos;City YPos;Army XPos;Army YPos;Port XPos;Port YPos;Beach XPos;Beach YPos;Beach Icon;Fort XPos;Fort YPos;AA XPos;AA YPos;Counter x;Counter Y;Terrain variant;Terrain x;Terrain Y;Terrain variant;Terrain x;Terrain Y;Terrain variant;Terrain x;Terrain Y;Terrain variant;Fill coord X;Fill coord Y;;;;;;;;;"
                    );

                // Write the province definition lines in order
                foreach (Province province in Items)
                {
                    if (Game.Type == GameType.DarkestHour)
                    {
                        writer.WriteLine(
                            "{0};{1};{2};{3};{4};{5};{6};;;{7};;{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21};{22};{23};{24};{25};{26};{27};{28};{29};{30};{31};;;;;;;;;;;{32};{33};{34};{35};{36};{37};{38};{39};{40};{41};{42}",
                            province.Id,
                            province.Name,
                            AreaStrings[(int) province.Area],
                            RegionStrings[(int) province.Region],
                            province.Continent != ContinentId.None ? ContinentStrings[(int) province.Continent] : "",
                            ClimateStrings[(int) province.Climate],
                            TerrainStrings[(int) province.Terrain],
                            DoubleHelper.ToString(province.Infrastructure),
                            province.Beaches ? 1 : 0,
                            province.PortAllowed ? 1 : 0,
                            province.PortSeaZone,
                            DoubleHelper.ToString(province.Ic),
                            DoubleHelper.ToString(province.Manpower),
                            DoubleHelper.ToString(province.Oil),
                            DoubleHelper.ToString(province.Metal),
                            DoubleHelper.ToString(province.Energy),
                            DoubleHelper.ToString(province.RareMaterials),
                            province.CityXPos,
                            province.CityYPos,
                            province.ArmyXPos,
                            province.ArmyYPos,
                            province.PortXPos,
                            province.PortYPos,
                            province.BeachXPos,
                            province.BeachYPos,
                            province.BeachIcon,
                            province.FortXPos,
                            province.FortYPos,
                            province.AaXPos,
                            province.AaYPos,
                            province.CounterXPos,
                            province.CounterYPos,
                            province.FillCoordX1,
                            province.FillCoordY1,
                            province.FillCoordX2,
                            province.FillCoordY2,
                            province.FillCoordX3 != 0 ? IntHelper.ToString(province.FillCoordX3) : "",
                            province.FillCoordY3 != 0 ? IntHelper.ToString(province.FillCoordY3) : "",
                            province.FillCoordX4 != 0 ? IntHelper.ToString(province.FillCoordX4) : "",
                            province.FillCoordY4 != 0 ? IntHelper.ToString(province.FillCoordY4) : "",
                            province.FillCoordX5 != 0 ? IntHelper.ToString(province.FillCoordX5) : "",
                            province.FillCoordY5 != 0 ? IntHelper.ToString(province.FillCoordY5) : "",
                            province.FillCoordX6 != 0 ? IntHelper.ToString(province.FillCoordX6) : "");
                    }
                    else
                    {
                        writer.Write(
                            "{0};{1};{2};{3};{4};{5};{6};;;{7};;{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21};{22};{23};{24};{25};{26};{27};{28};{29};{30};{31};;;;;;;;;;;{32};{33};{34};{35}",
                            province.Id,
                            province.Name,
                            AreaStrings[(int) province.Area],
                            RegionStrings[(int) province.Region],
                            ContinentStrings[(int) province.Continent],
                            ClimateStrings[(int) province.Climate],
                            TerrainStrings[(int) province.Terrain],
                            DoubleHelper.ToString(province.Infrastructure),
                            province.Beaches ? 1 : 0,
                            province.PortAllowed ? 1 : 0,
                            province.PortSeaZone,
                            province.Terrain != TerrainId.Ocean ? DoubleHelper.ToString(province.Ic) : "",
                            province.Terrain != TerrainId.Ocean ? DoubleHelper.ToString(province.Manpower) : "",
                            province.Terrain != TerrainId.Ocean ? DoubleHelper.ToString(province.Oil) : "",
                            province.Terrain != TerrainId.Ocean ? DoubleHelper.ToString(province.Metal) : "",
                            province.Terrain != TerrainId.Ocean ? DoubleHelper.ToString(province.Energy) : "",
                            province.Terrain != TerrainId.Ocean ? DoubleHelper.ToString(province.RareMaterials) : "",
                            province.CityXPos != 0 ? IntHelper.ToString(province.CityXPos) : "",
                            province.CityYPos != 0 ? IntHelper.ToString(province.CityYPos) : "",
                            province.ArmyXPos != 0 ? IntHelper.ToString(province.ArmyXPos) : "",
                            province.ArmyYPos != 0 ? IntHelper.ToString(province.ArmyYPos) : "",
                            province.PortXPos != 0 ? IntHelper.ToString(province.PortXPos) : "",
                            province.PortYPos != 0 ? IntHelper.ToString(province.PortYPos) : "",
                            province.BeachXPos != 0 ? IntHelper.ToString(province.BeachXPos) : "",
                            province.BeachYPos != 0 ? IntHelper.ToString(province.BeachYPos) : "",
                            province.BeachXPos != 0 ? IntHelper.ToString(province.BeachIcon) : "",
                            province.FortXPos != 0 ? IntHelper.ToString(province.FortXPos) : "",
                            province.FortYPos != 0 ? IntHelper.ToString(province.FortYPos) : "",
                            province.AaXPos != 0 ? IntHelper.ToString(province.AaXPos) : "",
                            province.AaYPos != 0 ? IntHelper.ToString(province.AaYPos) : "",
                            province.CounterXPos != 0 ? IntHelper.ToString(province.CounterXPos) : "",
                            province.CounterYPos != 0 ? IntHelper.ToString(province.CounterYPos) : "",
                            province.FillCoordX1,
                            province.FillCoordY1,
                            province.FillCoordX2,
                            province.FillCoordY2);
                        if (province.FillCoordX3 != 0 || province.FillCoordY3 != 0 ||
                            province.FillCoordX2 != -1 || province.FillCoordY2 != -1)
                        {
                            writer.Write(";{0};{1}", province.FillCoordX3, province.FillCoordY3);
                            if (province.FillCoordX4 != 0 || province.FillCoordY4 != 0 ||
                                province.FillCoordX3 != -1 || province.FillCoordY3 != -1)
                            {
                                writer.Write(";{0};{1}", province.FillCoordX4, province.FillCoordY4);
                            }
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        #endregion

        #region Province list operation

        /// <summary>
        ///     Add an item to the province list
        /// </summary>
        /// <param name="province">Items to be added</param>
        public static void AddItem(Province province)
        {
            Items.Add(province);

            if (province.Terrain == TerrainId.Ocean)
            {
                AddSeaZone(province);
            }
        }

        /// <summary>
        ///     Insert an item in the province list
        /// </summary>
        /// <param name="province">Items to be inserted</param>
        /// <param name="position">Item immediately before the insertion position</param>
        public static void InsertItem(Province province, Province position)
        {
            Items.Insert(Items.IndexOf(position) + 1, province);

            if (province.Terrain == TerrainId.Ocean)
            {
                AddSeaZone(province);
            }
        }

        /// <summary>
        ///     Remove an item from the province list
        /// </summary>
        /// <param name="province">Items to be deleted</param>
        public static void RemoveItem(Province province)
        {
            Items.Remove(province);

            if (province.Terrain == TerrainId.Ocean)
            {
                RemoveSeaZone(province);
            }
        }

        /// <summary>
        ///     Move items in the province list
        /// </summary>
        /// <param name="src">Item of move source</param>
        /// <param name="dest">Item to move to</param>
        public static void MoveItem(Province src, Province dest)
        {
            int srcIndex = Items.IndexOf(src);
            int destIndex = Items.IndexOf(dest);

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

        #region Province data manipulation

        /// <summary>
        ///     Change the region of the province
        /// </summary>
        /// <param name="province">Providence</param>
        /// <param name="area">area</param>
        public static void ModifyArea(Province province, AreaId area)
        {
            // Change the correspondence between regions and provinces
            DetachAreaProvince(province.Area, province);
            AttachAreaProvince(area, province);

            // Change the correspondence between regions
            DetachRegionArea(province.Region, province.Area);
            AttachRegionArea(province.Region, area);

            // Update value
            province.Area = area;
        }

        /// <summary>
        ///     Change the province of Providence
        /// </summary>
        /// <param name="province">Providence</param>
        /// <param name="region">Local</param>
        public static void ModifyRegion(Province province, RegionId region)
        {
            // Change the correspondence between regions
            DetachRegionArea(province.Region, province.Area);
            AttachRegionArea(region, province.Area);

            // Change the correspondence between continents and regions
            DetachContinentRegion(province.Continent, province.Region);
            AttachContinentRegion(province.Continent, region);

            // Update value
            province.Region = region;
        }

        /// <summary>
        ///     Change the continent of province
        /// </summary>
        /// <param name="province">Providence</param>
        /// <param name="continent">Continent</param>
        public static void ModifyContinent(Province province, ContinentId continent)
        {
            // Change the correspondence between continents and regions
            DetachContinentRegion(province.Continent, province.Region);
            AttachContinentRegion(continent, province.Region);

            // Update value
            province.Continent = continent;
        }

        #endregion

        #region Sea area list operation

        /// <summary>
        ///     Update the sea area list
        /// </summary>
        private static void UpdateSeaZones()
        {
            SeaZones.Clear();
            SeaZoneMap.Clear();
            foreach (Province province in Items.Where(province =>
                (province.Terrain == TerrainId.Ocean) &&
                Config.ExistsKey(province.Name) &&
                !string.IsNullOrEmpty(province.GetName())))
            {
                SeaZones.Add(province.Id);
                SeaZoneMap[province.Id] = province;
            }
        }

        /// <summary>
        ///     Add an item to the sea area list
        /// </summary>
        /// <param name="province"></param>
        private static void AddSeaZone(Province province)
        {
            // Do nothing if the name is an empty string
            if (!Config.ExistsKey(province.Name) || string.IsNullOrEmpty(province.GetName()))
            {
                return;
            }

            SeaZones.Add(province.Id);
            SeaZoneMap[province.Id] = province;
        }

        /// <summary>
        ///     Remove an item from the sea area list
        /// </summary>
        /// <param name="province"></param>
        private static void RemoveSeaZone(Province province)
        {
            // Do nothing if the name is an empty string
            if (!Config.ExistsKey(province.Name) || string.IsNullOrEmpty(province.GetName()))
            {
                return;
            }

            SeaZones.Remove(province.Id);
            SeaZoneMap.Remove(province.Id);
        }

        #endregion

        #region Continent / / Local / / Regional mapping

        /// <summary>
        ///     Continent / / Local / / Associate regions
        /// </summary>
        private static void AttachProvinces()
        {
            ContinentRegionMap = new Dictionary<ContinentId, List<RegionId>>();
            RegionAreaMap = new Dictionary<RegionId, List<AreaId>>();
            AreaProvinceMap = new Dictionary<AreaId, List<Province>>();

            foreach (Province province in Items)
            {
                // Correspondence between continents and regions
                AttachContinentRegion(province.Continent, province.Region);

                // Correspondence between regions
                AttachRegionArea(province.Region, province.Area);

                // Correspondence between regions and provinces
                AttachAreaProvince(province.Area, province);
            }
        }

        /// <summary>
        ///     Set the correspondence between continents and regions
        /// </summary>
        /// <param name="continentId">Continent</param>
        /// <param name="regionId">Local</param>
        private static void AttachContinentRegion(ContinentId continentId, RegionId regionId)
        {
            // Create if there is no continent item
            if (!ContinentRegionMap.ContainsKey(continentId))
            {
                ContinentRegionMap.Add(continentId, new List<RegionId>());
            }

            // Add a region to the region list
            if (!ContinentRegionMap[continentId].Contains(regionId))
            {
                ContinentRegionMap[continentId].Add(regionId);
            }
        }

        /// <summary>
        ///     Break the correspondence between continents and regions
        /// </summary>
        /// <param name="continentId">Continent</param>
        /// <param name="regionId">Local</param>
        private static void DetachContinentRegion(ContinentId continentId, RegionId regionId)
        {
            // Do nothing if there is no continental item
            if (!ContinentRegionMap.ContainsKey(continentId))
            {
                return;
            }

            // Remove a region from the region list
            if (ContinentRegionMap[continentId].Contains(regionId))
            {
                ContinentRegionMap[continentId].Remove(regionId);
                // Delete the continental item when there are no more items in the local list
                if (ContinentRegionMap[continentId].Count == 0)
                {
                    ContinentRegionMap.Remove(continentId);
                }
            }
        }

        /// <summary>
        ///     Set the correspondence between regions
        /// </summary>
        /// <param name="regionId">Local</param>
        /// <param name="areaId">area</param>
        private static void AttachRegionArea(RegionId regionId, AreaId areaId)
        {
            // Create if there is no local item
            if (!RegionAreaMap.ContainsKey(regionId))
            {
                RegionAreaMap.Add(regionId, new List<AreaId>());
            }

            // Add a region to the region list
            if (!RegionAreaMap[regionId].Contains(areaId))
            {
                RegionAreaMap[regionId].Add(areaId);
            }
        }

        /// <summary>
        ///     Break the association between regions
        /// </summary>
        /// <param name="regionId">Local</param>
        /// <param name="areaId">area</param>
        private static void DetachRegionArea(RegionId regionId, AreaId areaId)
        {
            // Do nothing if there are no local items
            if (!RegionAreaMap.ContainsKey(regionId))
            {
                return;
            }

            // Remove a region from the region list
            if (RegionAreaMap[regionId].Contains(areaId))
            {
                RegionAreaMap[regionId].Remove(areaId);
                // Delete local items when there are no more items in the area list
                if (RegionAreaMap[regionId].Count == 0)
                {
                    RegionAreaMap.Remove(regionId);
                }
            }
        }

        /// <summary>
        ///     Set the correspondence between regions and provinces
        /// </summary>
        /// <param name="areaId">area</param>
        /// <param name="province">Providence</param>
        private static void AttachAreaProvince(AreaId areaId, Province province)
        {
            // Create if there is no regional item
            if (!AreaProvinceMap.ContainsKey(areaId))
            {
                AreaProvinceMap.Add(areaId, new List<Province>());
            }

            // Add a Providence to the Providence List
            if (!AreaProvinceMap[areaId].Contains(province))
            {
                AreaProvinceMap[areaId].Add(province);
            }
        }

        /// <summary>
        ///     Disassociate a region with a province
        /// </summary>
        /// <param name="areaId">area</param>
        /// <param name="province">Providence</param>
        private static void DetachAreaProvince(AreaId areaId, Province province)
        {
            // Do nothing if there are no regional items
            if (!AreaProvinceMap.ContainsKey(areaId))
            {
                return;
            }

            // Remove Provinces from the Provinces list
            if (AreaProvinceMap[areaId].Contains(province))
            {
                AreaProvinceMap[areaId].Remove(province);
                // Delete regional items when there are no more items on the provision list
                if (AreaProvinceMap[areaId].Count == 0)
                {
                    AreaProvinceMap.Remove(areaId);
                }
            }
        }

        #endregion

        #region String operation

        /// <summary>
        ///     Get the continent name
        /// </summary>
        /// <param name="continent">Continent</param>
        /// <returns>Continent name</returns>
        public static string GetContinentName(ContinentId continent)
        {
            return Config.GetText(ContinentNames[(int) continent]);
        }

        /// <summary>
        ///     Get the local name
        /// </summary>
        /// <param name="region">Local</param>
        /// <returns>Local name</returns>
        public static string GetRegionName(RegionId region)
        {
            // AoD1.10 In the following cases, check if the character string definition has changed.
            if ((Game.Type == GameType.ArsenalOfDemocracy) && (Game.Version >= 110))
            {
                if (ReplacingRegionNamesAod.ContainsKey(region))
                {
                    return Config.GetText(ReplacingRegionNamesAod[region]);
                }
            }

            return Config.GetText(RegionNames[(int) region]);
        }

        /// <summary>
        ///     Get the area name
        /// </summary>
        /// <param name="area">area</param>
        /// <returns>Area name</returns>
        public static string GetAreaName(AreaId area)
        {
            // AoD1.10 In the following cases, check if the character string definition has changed.
            if ((Game.Type == GameType.ArsenalOfDemocracy) && (Game.Version >= 110))
            {
                if (ReplacingAreaNamesAod.ContainsKey(area))
                {
                    return Config.GetText(ReplacingAreaNamesAod[area]);
                }
            }

            return Config.GetText(AreaNames[(int) area]);
        }

        /// <summary>
        ///     Get the climate name
        /// </summary>
        /// <param name="climate">climate</param>
        /// <returns>Climate name</returns>
        public static string GetClimateName(ClimateId climate)
        {
            return Config.GetText(ClimateNames[(int) climate]);
        }

        /// <summary>
        ///     Get the terrain name
        /// </summary>
        /// <param name="terrain">terrain</param>
        /// <returns>Topography name</returns>
        public static string GetTerrainName(TerrainId terrain)
        {
            return Config.GetText(TerrainNames[(int) terrain]);
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
        ///     Clear the edited flag
        /// </summary>
        private static void ResetDirty()
        {
            _dirtyFlag = false;
        }

        #endregion
    }
}
