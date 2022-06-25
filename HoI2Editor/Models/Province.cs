using System;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Providence data
    /// </summary>
    public class Province
    {
        #region Public properties

        /// <summary>
        ///     Providence ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Providence name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     area ID
        /// </summary>
        public AreaId Area { get; set; }

        /// <summary>
        ///     Local ID
        /// </summary>
        public RegionId Region { get; set; }

        /// <summary>
        ///     Continent ID
        /// </summary>
        public ContinentId Continent { get; set; }

        /// <summary>
        ///     climate ID
        /// </summary>
        public ClimateId Climate { get; set; }

        /// <summary>
        ///     terrain ID
        /// </summary>
        public TerrainId Terrain { get; set; }

        /// <summary>
        ///     Size correction (( not clear )
        /// </summary>
        public int SizeModifier { get; set; }

        /// <summary>
        ///     Aircraft capacity (( not clear )
        /// </summary>
        public int AirCapacity { get; set; }

        /// <summary>
        ///     infrastructure
        /// </summary>
        public double Infrastructure { get; set; }

        /// <summary>
        ///     City (( not clear )
        /// </summary>
        public int City { get; set; }

        /// <summary>
        ///     Presence or absence of sandy beach
        /// </summary>
        public bool Beaches { get; set; }

        /// <summary>
        ///     Presence or absence of a port
        /// </summary>
        public bool PortAllowed { get; set; }

        /// <summary>
        ///     Sea area of the harbor
        /// </summary>
        public int PortSeaZone { get; set; }

        /// <summary>
        ///     I C
        /// </summary>
        public double Ic { get; set; }

        /// <summary>
        ///     Labor force
        /// </summary>
        public double Manpower { get; set; }

        /// <summary>
        ///     oil
        /// </summary>
        public double Oil { get; set; }

        /// <summary>
        ///     metal
        /// </summary>
        public double Metal { get; set; }

        /// <summary>
        ///     energy
        /// </summary>
        public double Energy { get; set; }

        /// <summary>
        ///     Rare resources
        /// </summary>
        public double RareMaterials { get; set; }

        /// <summary>
        ///     Of city X Coordinate
        /// </summary>
        public int CityXPos { get; set; }

        /// <summary>
        ///     Of city Y Coordinate
        /// </summary>
        public int CityYPos { get; set; }

        /// <summary>
        ///     Of the army X Coordinate
        /// </summary>
        public int ArmyXPos { get; set; }

        /// <summary>
        ///     Of the army Y Coordinate
        /// </summary>
        public int ArmyYPos { get; set; }

        /// <summary>
        ///     Of the harbor X Coordinate
        /// </summary>
        public int PortXPos { get; set; }

        /// <summary>
        ///     Of the harbor Y Coordinate
        /// </summary>
        public int PortYPos { get; set; }

        /// <summary>
        ///     On the sandy beach X Coordinate
        /// </summary>
        public int BeachXPos { get; set; }

        /// <summary>
        ///     On the sandy beach Y Coordinate
        /// </summary>
        public int BeachYPos { get; set; }

        /// <summary>
        ///     Sandy beach icon
        /// </summary>
        public int BeachIcon { get; set; }

        /// <summary>
        ///     Of the fortress X Coordinate
        /// </summary>
        public int FortXPos { get; set; }

        /// <summary>
        ///     Fortress Y Coordinate
        /// </summary>
        public int FortYPos { get; set; }

        /// <summary>
        ///     Anti-aircraft gun X Coordinate
        /// </summary>
        public int AaXPos { get; set; }

        /// <summary>
        ///     Anti-aircraft gun Y Coordinate
        /// </summary>
        public int AaYPos { get; set; }

        /// <summary>
        ///     Of the counter X Coordinate
        /// </summary>
        public int CounterXPos { get; set; }

        /// <summary>
        ///     Of the counter Y Coordinate
        /// </summary>
        public int CounterYPos { get; set; }

        /// <summary>
        ///     not clear
        /// </summary>
        public int TerrainVariant1 { get; set; }

        /// <summary>
        ///     not clear
        /// </summary>
        public int TerrainXPos1 { get; set; }

        /// <summary>
        ///     not clear
        /// </summary>
        public int TerrainYPos1 { get; set; }

        /// <summary>
        ///     not clear
        /// </summary>
        public int TerrainVariant2 { get; set; }

        /// <summary>
        ///     not clear
        /// </summary>
        public int TerrainXPos2 { get; set; }

        /// <summary>
        ///     not clear
        /// </summary>
        public int TerrainYPos2 { get; set; }

        /// <summary>
        ///     not clear
        /// </summary>
        public int TerrainVariant3 { get; set; }

        /// <summary>
        ///     not clear
        /// </summary>
        public int TerrainXPos3 { get; set; }

        /// <summary>
        ///     not clear
        /// </summary>
        public int TerrainYPos3 { get; set; }

        /// <summary>
        ///     not clear
        /// </summary>
        public int TerrainVariant4 { get; set; }

        /// <summary>
        ///     fill X Coordinate 1
        /// </summary>
        public int FillCoordX1 { get; set; }

        /// <summary>
        ///     fill Y Coordinate 1
        /// </summary>
        public int FillCoordY1 { get; set; }

        /// <summary>
        ///     fill X Coordinate 2
        /// </summary>
        public int FillCoordX2 { get; set; }

        /// <summary>
        ///     fill Y Coordinate 2
        /// </summary>
        public int FillCoordY2 { get; set; }

        /// <summary>
        ///     fill X Coordinate 3
        /// </summary>
        public int FillCoordX3 { get; set; }

        /// <summary>
        ///     fill Y Coordinate 3
        /// </summary>
        public int FillCoordY3 { get; set; }

        /// <summary>
        ///     fill X Coordinate Four
        /// </summary>
        public int FillCoordX4 { get; set; }

        /// <summary>
        ///     fill Y Coordinate Four
        /// </summary>
        public int FillCoordY4 { get; set; }

        /// <summary>
        ///     fill X Coordinate Five
        /// </summary>
        public int FillCoordX5 { get; set; }

        /// <summary>
        ///     fill Y Coordinate Five
        /// </summary>
        public int FillCoordY5 { get; set; }

        /// <summary>
        ///     fill X Coordinate 6
        /// </summary>
        public int FillCoordX6 { get; set; }

        /// <summary>
        ///     Whether it is land provision
        /// </summary>
        public bool IsLand
        {
            get
            {
                switch (Terrain)
                {
                    case TerrainId.Plains:
                    case TerrainId.Forest:
                    case TerrainId.Mountain:
                    case TerrainId.Desert:
                    case TerrainId.Marsh:
                    case TerrainId.Hills:
                    case TerrainId.Jungle:
                    case TerrainId.Urban:
                    case TerrainId.Clear:
                        return true;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        ///     Whether it is a marine provision
        /// </summary>
        public bool IsSea
        {
            get
            {
                switch (Terrain)
                {
                    case TerrainId.Ocean:
                    case TerrainId.River:
                        return true;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        ///     Whether it is invalid provision
        /// </summary>
        public bool IsInvalid
        {
            get
            {
                switch (Terrain)
                {
                    case TerrainId.TerraIncognito:
                    case TerrainId.Unknown:
                        return true;

                    default:
                        return false;
                }
            }
        }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ProvinceItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region String operation

        /// <summary>
        ///     Get the Providence name
        /// </summary>
        /// <returns>Providence name</returns>
        public string GetName()
        {
            return Config.GetText(Name);
        }

        /// <summary>
        ///     Set the province name
        /// </summary>
        /// <param name="s">Providence name</param>
        public void SetName(string s)
        {
            Config.SetText(Name, s, Game.ProvinceTextFileName);
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the provision data has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirty()
        {
            return _dirtyFlag;
        }

        /// <summary>
        ///     Get if the item has been edited
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirty(ProvinceItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ProvinceItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (TeamItemId id in Enum.GetValues(typeof (ProvinceItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     area ID
    /// </summary>
    public enum AreaId
    {
        None,

        // For land
        Adelaide,
        Afghanistan,
        Agats,
        Alabama,
        Alaska,
        AlaskanArchipelago,
        Albania,
        Alberta,
        AliceSprings,
        AlpesProvence,
        Amur,
        AnatolianNorthCoast,
        AnatolianSouthCoast,
        AngolanCoast,
        AngolanPlains,
        Anhui,
        Antilles,
        AppennineRidge,
        Aquitaine,
        ArabianDesert,
        Arizona,
        Arkansas,
        Arkhangelsk,
        ArmeniaAzerbaijan,
        Astrakhan,
        Asuncion,
        Attica,
        Austria,
        Babo,
        BadenWurttemberg,
        BahamasIslands,
        Baleares,
        Belgorod, // DH only
        Bavaria,
        Bechuanaland,
        Bengal,
        Bengazi,
        Bermuda,
        Bessarabia,
        Bohemia,
        Bombay,
        Bonin,
        Bosnia,
        Bosporus,
        Bougainville,
        BourgogneChampagne,
        Brandenburg,
        Brasilia,
        BrestLitovsk,
        Brisbane,
        BritishColumbia,
        Brittany,
        Bulgaria,
        Burma,
        Cairns,
        California,
        CameroonianJungle,
        Canarias,
        CantabricChain,
        CapVerdeIslands,
        Cape,
        Caracas,
        Carolinese,
        Catalonia,
        Celebes,
        CentralAfricaDesert,
        CentralAfricaPlains,
        CentralFinland,
        CentralItaly,
        CentralNorway,
        CentralRainforests,
        CentralSerbia,
        CentralSpain,
        CentralTurkey,
        Chahar,
        Chihuahua,
        Chile,
        ChileanArchipelago,
        Colorado,
        ConnecticutRhodeIsland,
        ContinentalSpain,
        Croatia,
        Cuba,
        DanakilPlateau,
        Darwin,
        Deccar,
        Delhi,
        Delta,
        Denmark,
        DiegoGarciaIsland,
        Dnepropretovsk,
        EastAndalucia,
        EastAtlas,
        EastEngland,
        EastJava,
        EastPersia,
        EastPrussia,
        EastSerbia,
        EasternAnatolia,
        EasternGhat,
        EasternHungary,
        Eire,
        Engels, // DH only
        ElAlamein,
        ElRif,
        EspirtuSanto,
        Estonia,
        EthiopianHighland,
        Fiji,
        Flanders,
        FloresTimor,
        Florida,
        Fujian,
        Gabes,
        Gansu,
        Georgia,
        Georgien,
        Goetaland,
        GoldCoast,
        GreekIsland,
        Grodno,
        Groznyi, // DH only
        Guadalcanal,
        Guangdong,
        Guangxi,
        Guayana,
        GuineanCoast,
        Guizhou,
        HannoverMagdeburg,
        Hawaii,
        Hebei,
        Hedjaz,
        Heilongjiang,
        Henan,
        Hessen,
        Himalayas,
        Hispaniola,
        HolsteinMecklemburg,
        Honshu,
        Hubei,
        Hunan,
        Iceland,
        Idaho,
        Illinois,
        Indiana,
        Indochine,
        InteriorAlgeria,
        InteriorLibya,
        Iowa,
        Iquitos,
        Iraq,
        Irkutsk,
        IvoryCoast,
        Jiangsu,
        Jiangxi,
        Jilin,
        JohnsonIsland,
        Kamchatka,
        Kansas,
        Karelia,
        Kassarine,
        Kazakstan,
        Kazan,
        Kentucky,
        Kenya,
        Khabarovsk,
        Kharkov,
        Kiev,
        KirgizSteppe,
        Kirgizistan,
        Kirov,
        Kola, // DH only
        KongoleseJungle,
        Kostroma,
        Krakow,
        Krasnodar,
        Krim,
        Kurdufan,
        Kursk,
        Kuybyshev,
        KyushuShikoku,
        LaPaz,
        LaPlata,
        Lae,
        Lakes,
        Lapland,
        Latvia,
        LebanonSyria,
        Leningrad,
        Levant,
        LeyteIslandGroup,
        Liaoning,
        LigurianIslands,
        Lima,
        LimousinAuvergne,
        Lithuania,
        Loire,
        LorraineAlsace,
        Louisiana,
        LowerArgentine,
        LowerCalifornia,
        Luzon,
        Lvov,
        Madagascar,
        Magadan,
        Magdalena,
        MaghrebCoast,
        Maine,
        Malacka,
        MalianDesert,
        MalianValleys,
        Manaus,
        Manitoba,
        Maracaibo,
        MarcusWake,
        Marshalls,
        MarylandDelaware,
        Massachussets,
        MatoGrosso,
        Mauretania,
        Melbourne,
        Melkosopotjnik,
        Memel,
        MexicoCity,
        Michigan,
        Midway,
        Mindanao,
        Minnesota,
        Minsk,
        Mississippi,
        Missouri,
        Mocambique,
        Molucks,
        Mongolia,
        Montana,
        Morocco,
        Moscow,
        Mozyr, // DH only
        Nagpur,
        Nalchik, // DH only
        Nambia,
        Nauru,
        Nebraska,
        NedreNorrland,
        Netherlands,
        NewBrunswick,
        NewFoundland,
        NewHampshire,
        NewJersey,
        NewMexico,
        NewYork,
        NewZealand,
        Nevada,
        Niassa,
        Nicaragua,
        Niger,
        NileValley,
        NizhnijNovgorod,
        Normandy,
        NorthBorneo,
        NorthCarolina,
        NorthDakota,
        NorthEngland,
        NorthEthiopia,
        NorthGilberts,
        NorthIreland,
        NorthItaly,
        NorthKrasnoyarsk,
        NorthMarianes,
        NorthNigeria,
        NorthPersia,
        NorthRhodesia,
        NorthRomania,
        NorthScotland,
        NorthernNorway,
        NorthwestTerritories,
        NovaScotia,
        Novgorod,
        Novosibirsk,
        NubianDesert,
        Odessa,
        Oesterbotten,
        OestraSvealand,
        OevreNorrland,
        Ohio,
        Oklahoma,
        Omsk,
        Ontario,
        Oran,
        Oregon,
        Orenburg, // DH only
        Orissa,
        Pakistan,
        PalawanMindoro,
        Palestine,
        PanamanRidge,
        Paris,
        PasDeCalais,
        Pennsylvania,
        Penza,
        Perm,
        PersianGulfCoast,
        Perth,
        Petrozavodsk, // DH only
        Phoenix,
        Polotsk,
        Poltava, // DH only
        Polynesia,
        PortMoresby,
        Portugal,
        Poznan,
        Primorski,
        Pskov,
        Pyongyang,
        PyreneesLanguedoc,
        Qattara,
        Qinghai,
        Quebec,
        Quito,
        Rabaul,
        Rajastan,
        RedSeaCoast,
        Rehe,
        ReservoireRybinsk,
        Rhineland, // DH only
        RioDeJaneiro,
        RioDeOro,
        Rogachev,
        Rostov,
        Ryazan,
        Sachsen,
        SakhalinKuriles,
        Samoa,
        SaoPaulo,
        Saransk,
        Sarmi,
        Saskatchewan,
        Senegambia,
        Senjavin,
        Seoul,
        Shaanxi,
        Shaba,
        Shandong,
        Shanxi,
        Siam,
        Sichuan,
        SicilianGap,
        SidiBarrani,
        Silesia, // DH only
        Sirte,
        SlaveCoast,
        Slovakia,
        Smolensk,
        Somalia,
        Somaliland,
        SouthBorneo,
        SouthCarolina,
        SouthDakota,
        SouthEthiopia,
        SouthFinland,
        SouthGilberts,
        SouthItaly,
        SouthKrasnoyarsk,
        SouthNigeria,
        SouthPersia,
        SouthRhodesia,
        SouthRomania,
        SouthScotland,
        SouthcentralNorway,
        SouthernNorway,
        StHelena,
        Stalingrad,
        SuezChannelArea,
        SuiyuanNingxia,
        Sumatra,
        SundaIslands,
        Sverdlovsk,
        Switzerland,
        Sydney,
        Syzran,
        Tadzjikistan,
        Taiwan,
        TajmyrEvenk,
        Tambov,
        Tanganyika,
        Tennessee,
        Texas,
        TheAzores,
        TheFalklands,
        Thrace,
        Tibet,
        TierraDelFuego,
        Tobruk,
        Tohoku,
        Tomsk,
        Transjordan,
        Transnistria,
        TransvaalNatal,
        Transylvania,
        Tunis,
        Turkestan,
        Turkmenistan,
        Tyumen,
        Ufa,
        Uganda,
        UpperArgentine,
        UpperNileValley,
        Utah,
        Uzbekistan,
        VaestraSvealand,
        Wallonia,
        Warsaw,
        Washington,
        VelikiyeLuki,
        VeraCruz,
        Vermont,
        Vorkuta, // DH only
        WestAndalucia,
        WestAtlas,
        WestEngland,
        WestJava,
        WestVirginia,
        WesternDesert,
        WesternGhat,
        WesternHungary,
        Westphalen,
        Virginia,
        Wisconsin,
        Vladimir,
        Volta,
        Wyoming,
        Xikang,
        Xinjiang,
        Yakutsk,
        YemeniteMountains,
        Yukon,
        Yunnan,
        Zabaikalye,
        Zhejiang,
        Ryukyusland,
        NorthBismarckArchipelago,
        NorthNewHebrides,
        SouthNewHebrides,
        CentralSolomons,
        WestAleutians,
        Ceylon,
        Hokkaido,
        NorthBurma,
        EastBengal,
        KraPeninsula,
        Tripoli,
        Greenland,

        // For the ocean
        Lake,
        AdriaticSea,
        AegeanSea,
        BlackSea,
        RedSea,
        TyrrhenianSea,
        BothnianBay,
        NorthernBaltic,
        SouthernBaltic,
        Kattegat,
        BarentsSea,
        LuzonStrait,
        PhilipineTrench,
        SuluSea,
        CelebesSea,
        CoastOfBrunei,
        FloresSea,
        MoluccaSea,
        BandaSea,
        WestCoralSea,
        ArafuraSea,
        JavaRidge,
        MalaccaStrait,
        GulfOfSiam,
        SpratlySea,
        CoastOfIndochina,
        TaiwanStrait,
        IrishSea,
        EnglishChannel,
        DenmarkStrait,
        SoutheastNorthsea,
        WestNorthsea,
        CentralNorthsea,
        NorwegianSea,
        CostaDelSol,
        AlgerianCoast,
        EgyptianCoast,
        GulfOfLyon,
        SeaOfJapan,
        NorthBismarckSea,
        SouthBismarckSea,
        Marianas,
        MarshallsSea,
        WesternSolomons,
        EasternSolomons,
        EastCoralSea,
        CoastOfJapan,
        WesternApproaches,
        GreenlandCoast,
        FaroesGap,
        NorthAtlantic,
        BayOfBiscay,
        Azores,
        PortugeseCoast,
        CapStVincent,
        CoastOfBrazil,
        CapVerde,
        GulfOfGuinea,
        CentralAtlantic,
        CoastOfAfrica,
        CoastOfBissao,
        SolomonSea,
        NorthTasmanSea,
        SouthTasmanSea,
        EastGreatAustralianBight,
        WestGreatAustralianBight,
        HudsonBay,
        PersianGulf,
        YucatanStrait,
        CentralCarribean,
        WindwardIslands,
        WestGulfOfMexico,
        EastGulfOfMexico,
        FloridaStrait,
        BermudaTriangle,
        NorthernSeaOfLabrador,
        SouthernSeaOfLabrador,
        GrandBanks,
        TheSeamounts,
        CanadianMaritimes,
        UsMaritimes,
        GuianaBasin,
        SouthCentralMidAtlanticRidge,
        CentralMidAtlanticRidge,
        Aleutians,
        Carolines,
        CentralPhilippineSea,
        CoastOfCeylon,
        CoastOfKamchatka,
        EastBayOfBengal,
        EastBeringSea,
        EastMarianaBasin,
        EastSeaOfOkhotsk,
        GulfOfAlaska,
        HawaiianRise,
        JavaTrench,
        MarianaTrench,
        MidPacificMountains,
        NinetyeastRidge,
        NorthNortheastPacificBasin,
        NorthwestPacificBasin,
        Ryukyus,
        SouthSeaOfOkhotsk,
        TimorSea,
        WestBayOfBengal,
        WestBeringSea,
        WestCoastOfAustralia,
        WestCoastOfMexico,
        WestCoastOfTheUnitedStates,
        WestSeaOfOkhotsk,
        YellowSea,
        AtlanticIndianRidge,
        CoastOfSouthAfrica,
        FijiBasin,
        Gilberts,
        LineIslands,
        MascarenePlateau,
        MidIndianRidge,
        NorthArabianSea,
        NorthEastPacificOcean,
        NorthMozambiqueChannel,
        NortheastCoastOfMadagascar,
        SouthArabianSea,
        SouthEastPacificOcean,
        SouthMozambiqueChannel,
        SoutheastCoastOfMadagascar,
        SoutheastIndianOcean,
        SouthwestIndianOcean,
        EastCostOfNewZealand,
        NorthSoutheastPacificOcean,
        SouthSoutheastPacificOcean,
        SoutheastPacificBasin,
        SouthwestPacificBasin,
        WestCoastOfCentralAmerica,
        WestCoastOfChile,
        WestCoastOfPeru,
        HornOfAfrica,
        CoastOfAngolaNamibia,
        AngolaPlain,
        ArgentinePlain,
        CoastOfArgentina,
        CoastOfUruguay,
        CoastOfNewGuinea,
        CoastOfGuyana,
        CoastOfRecife,
        CapeFinisterre,
        ArcticOcean,
        TheHebreides,
        IrishWestCoast,
        PernambucoPlain,
        AscensionFractureZone,
        EastNorthSea,
        CaspianSea // DH only
    }

    /// <summary>
    ///     Local ID
    /// </summary>
    public enum RegionId
    {
        None,

        // For land
        Afghanistan,
        Alaska,
        Algeria,
        Amazonas,
        AmericanPacific,
        Anatolia,
        Andes,
        AsianPacific,
        Australia,
        Austria,
        Balkan,
        BalticStates,
        Belarus,
        Benelux,
        BismarckArchipelago,
        Brazil,
        BrazilianHighlands,
        Canada,
        Caribbean,
        CentralAfrica,
        CentralAmerica,
        CentralAsia,
        China,
        Czechoslovakia,
        Denmark,
        DiegoGarciaIsland,
        EastAfrica,
        EasternRussia,
        Egypt,
        England,
        Ethiopia,
        FarEastSiberia,
        Finland,
        France,
        Germany,
        GranChaco,
        GrandColombia,
        Greenland,
        HornOfAfrica,
        Hungary,
        Iceland,
        India,
        Indochine,
        Indonesia,
        Ireland,
        Irkutsk,
        Italy,
        Japan,
        Kaukasus,
        Kazakstan,
        Korea,
        Krasnoyarsk,
        Libya,
        Madagascar,
        Mexico,
        MiddleEast,
        MidwestUs,
        Morocco,
        NewGuinea,
        NorthSolomonIslands,
        NortheastUs,
        NorthernPermafrost,
        NorthernRussia,
        NorthwestUs,
        Norway,
        Novosibirsk,
        Patagonia,
        Persia,
        Philippines,
        Poland,
        Portugal,
        RioDeLaPlata,
        Romania,
        Russia,
        Sahara,
        Scotland,
        SouthAfrica,
        SouthSolomonIslands,
        SouthcentralUs,
        SoutheastUs,
        SouthernRussia,
        SouthwestAfrica,
        SouthwestUs,
        Spain,
        Sudan,
        Sweden,
        Switzerland,
        Tomsk,
        Tunisia,
        Ukraine,
        Urals,
        WestAfrica,
        WhiteSeaTundra,
        EasternCanada,
        WesternCanada,
        WesternRussia,
        NorthernCanada,

        // For the ocean
        Lake,
        BlackSea,
        BalticSea,
        WesternMediterranean,
        CentralMediterranean,
        EasternMediterranean,
        SouthChinaSea,
        PhilippineSea,
        Moluccas,
        JavaSea,
        CoralSea,
        EastChinaSea,
        NorthSea,
        NortheastAtlantic,
        NorthAtlatic,
        NorthwestAtlantic,
        IceSea,
        TasmanSea,
        Carribean,
        GreatAustralianBight,
        Sargassos,
        MexicanGulf,
        CanadianArctic,
        GrandBanksShelf,
        EasternSeaboard,
        CentralMidAtlanticRidge,
        BismarckSea,
        BayOfBengal,
        BeringSea,
        CentralPacificOcean,
        EastIndianOcean,
        HomeIslands,
        NorthPacificOcean,
        SeaOfOkhotsk,
        WestCoastOfNorthAmerica,
        ArabianSea,
        CapeOfGoodHope,
        CoastOfMadagascar,
        EastPacificOcean,
        MozambiqueChannel,
        SouthPacificOcean,
        SouthIndianOcean,
        WestIndianOcean,
        PacificAntarcticRidge,
        SoutheastPacificOcean,
        WestCoastOfSouthAmerica,
        SoutheastAtlanticOcean,
        SouthwestAtlanticOcean,
        WesternIceSea,
        BrazilianCoast,
        SouthcentralAtlantic,
        EasternAtlantic,
        CelticSea,
        IberianWestCoast
    }

    /// <summary>
    ///     Continent ID
    /// </summary>
    public enum ContinentId
    {
        None,
        NorthAmerica,
        SouthAmerica,
        Europe,
        Asia,
        Oceania,
        Africa,
        Lake,
        AtlanticOcean,
        PacificOcean,
        IndianOcean
    }

    /// <summary>
    ///     climate ID
    /// </summary>
    public enum ClimateId
    {
        None,
        Arctic,
        Subarctic,
        Temperate,
        Muddy,
        Mediterranean,
        Subtropical,
        Tropical,
        Arid
    }

    /// <summary>
    ///     terrain ID
    /// </summary>
    public enum TerrainId
    {
        Plains,
        Forest,
        Mountain,
        Desert,
        Marsh,
        Hills,
        Jungle,
        Urban,
        Ocean,
        River,
        TerraIncognito,
        Unknown,
        Clear
    }

    /// <summary>
    ///     Province item ID
    /// </summary>
    public enum ProvinceItemId
    {
        Id, // Providence ID
        Name, // Province name
        Area, // area
        Region, // Local
        Continent, // Continent
        Climate, // climate
        Terrain, // terrain
        SizeModifier, // Size correction
        AirCapacity, // Aircraft capacity
        Infrastructure, // infrastructure
        City, // City
        Beaches, // Presence or absence of sandy beach
        PortAllowed, // Presence or absence of a port
        PortSeaZone, // Sea area of the harbor
        Ic, // I C
        Manpower, // Labor force
        Oil, // oil
        Metal, // metal
        Energy, // energy
        RareMaterials, // Rare resources
        CityXPos, // Of city X Coordinate
        CityYPos, // Of city Y Coordinate
        ArmyXPos, // Of the armyX Coordinate
        ArmyYPos, // Of the army Y Coordinate
        PortXPos, // Of the harbor X Coordinate
        PortYPos, // Of the harbor Y Coordinate
        BeachXPos, // On the sandy beach X Coordinate
        BeachYPos, // On the sandy beach Y Coordinate
        BeachIcon, // Sandy beach icon
        FortXPos, // Fortress X Coordinate
        FortYPos, // Fortress Y Coordinate
        AaXPos, // Anti-aircraft gun X Coordinate
        AaYPos, // Anti-aircraft gun Y Coordinate
        CounterXPos, // Of the counter X Coordinate
        CounterYPos, // Of the counter Y Coordinate
        TerrainVariant1,
        TerrainXPos1,
        TerrainYPos1,
        TerrainVariant2,
        TerrainXPos2,
        TerrainYPos2,
        TerrainVariant3,
        TerrainXPos3,
        TerrainYPos3,
        TerrainVariant4,
        FillCoordX1, // fill X Coordinate 1
        FillCoordY1, // fill Y Coordinate 1
        FillCoordX2, // fill X Coordinate 2
        FillCoordY2, // fill Y Coordinate 2
        FillCoordX3, // fill X Coordinate 3
        FillCoordY3, // fill Y Coordinate 3
        FillCoordX4, // fill X Coordinate Four
        FillCoordY4, // fill Y Coordinate Four
        FillCoordX5, // fill X Coordinate Five
        FillCoordY5, // fill Y Coordinate Five
        FillCoordX6 // fillX Coordinate 6
    }
}
