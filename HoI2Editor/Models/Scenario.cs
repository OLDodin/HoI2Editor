using System;
using System.Collections.Generic;
using System.Text;
using HoI2Editor.Utilities;

namespace HoI2Editor.Models
{

    #region Scenario data

    /// <summary>
    ///     Scenario data
    /// </summary>
    public class Scenario
    {
        #region Public properties

        /// <summary>
        ///     Whether it is a save game
        /// </summary>
        public bool IsSaveGame { get; set; }

        /// <summary>
        ///     Scenario name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Panel image name
        /// </summary>
        public string PanelName { get; set; }

        /// <summary>
        ///     Scenario header
        /// </summary>
        public ScenarioHeader Header { get; set; }

        /// <summary>
        ///     Scenario global data
        /// </summary>
        public ScenarioGlobalData GlobalData { get; set; }

        /// <summary>
        ///     Events that have occurred
        /// </summary>
        public List<int> HistoryEvents { get; } = new List<int>();

        /// <summary>
        ///     Pause event
        /// </summary>
        public List<int> SleepEvents { get; } = new List<int>();

        /// <summary>
        ///     Event occurrence date and time
        /// </summary>
        public Dictionary<int, GameDate> SaveDates { get; set; }

        /// <summary>
        ///     Map settings
        /// </summary>
        public MapSettings Map { get; set; }

        /// <summary>
        ///     Event file
        /// </summary>
        public List<string> EventFiles { get; } = new List<string>();

        /// <summary>
        ///     All event files
        /// </summary>
        public List<string> AllEventFiles { get; } = new List<string>();

        /// <summary>
        ///     Include file
        /// </summary>
        public List<string> IncludeFiles { get; } = new List<string>();

        /// <summary>
        ///     Include folder
        /// </summary>
        public string IncludeFolder { get; set; }

        /// <summary>
        ///     Province settings
        /// </summary>
        public List<ProvinceSettings> Provinces { get; } = new List<ProvinceSettings>();

        /// <summary>
        ///     By country inc Whether to define province settings for
        /// </summary>
        public bool IsCountryProvinceSettings { get; set; }

        /// <summary>
        ///     bases.inc Whether to define provision settings for
        /// </summary>
        public bool IsBaseProvinceSettings { get; set; }

        /// <summary>
        ///     bases_DOD.inc Whether to define province settings for
        /// </summary>
        public bool IsBaseDodProvinceSettings { get; set; }

        /// <summary>
        ///     depots.inc Whether to define province settings for
        /// </summary>
        public bool IsDepotsProvinceSettings { get; set; }

        /// <summary>
        ///     vp.inc Whether to define provision settings for
        /// </summary>
        public bool IsVpProvinceSettings { get; set; }

        /// <summary>
        ///     National information
        /// </summary>
        public List<CountrySettings> Countries { get; } = new List<CountrySettings>();

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        /// <summary>
        ///     Edited flags for selectable countries
        /// </summary>
        private readonly HashSet<Country> _dirtySelectableCountries = new HashSet<Country>();

        /// <summary>
        ///     Edited flags in province settings
        /// </summary>
        private bool _dirtyProvinces;

        /// <summary>
        ///     vp.inc Edited flag
        /// </summary>
        private bool _dirtyVpInc;

        #endregion

        #region Public constant

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            Name, // Scenario name
            PanelName, // Panel image name
            IncludeFolder, // Include folder
            FreeSelection, // Free choice of the nation
            BattleScenario, // Short scenario
            AiAggressive, // AI Aggression
            Difficulty, // difficulty
            GameSpeed, // Game speed
            AllowDiplomacy, // Allow diplomacy
            AllowProduction, // Allow production
            AllowTechnology, // Allow technology development
            StartYear, // Start year
            StartMonth, // Start month
            StartDay, // start date
            EndYear, // End year
            EndMonth, // End month
            EndDay // End date
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if it has been edited
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
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Get if the target selectable country has been edited
        /// </summary>
        /// <param name="country">Target country</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtySelectableCountry(Country country)
        {
            return _dirtySelectableCountries.Contains(country);
        }

        /// <summary>
        ///     Set edited flags for selectable countries
        /// </summary>
        /// <param name="country">Target country</param>
        public void SetDirtySelectableCountry(Country country)
        {
            _dirtySelectableCountries.Add(country);
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Get if the provision data has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyProvinces()
        {
            return _dirtyProvinces;
        }

        /// <summary>
        ///     Set the edited flag for province data
        /// </summary>
        public void SetDirtyProvinces()
        {
            _dirtyProvinces = true;
        }

        /// <summary>
        ///     vp.inc Gets whether is edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyVpInc()
        {
            return _dirtyVpInc;
        }

        /// <summary>
        ///     vp.inc Set the edited flag of
        /// </summary>
        public void SetDirtyVpInc()
        {
            _dirtyVpInc = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }

            _dirtySelectableCountries.Clear();

            if (Header?.MajorCountries != null)
            {
                foreach (MajorCountrySettings major in Header.MajorCountries)
                {
                    major.ResetDirtyAll();
                }
            }

            if (GlobalData != null)
            {
                GlobalData.Axis?.ResetDirtyAll();
                GlobalData.Allies?.ResetDirtyAll();
                GlobalData.Comintern?.ResetDirtyAll();
                foreach (Alliance alliance in GlobalData.Alliances)
                {
                    alliance.ResetDirtyAll();
                }
                foreach (War war in GlobalData.Wars)
                {
                    war.ResetDirtyAll();
                }
                foreach (Treaty nonAggression in GlobalData.NonAggressions)
                {
                    nonAggression.ResetDirtyAll();
                }
                foreach (Treaty peace in GlobalData.Peaces)
                {
                    peace.ResetDirtyAll();
                }
                foreach (Treaty trade in GlobalData.Trades)
                {
                    trade.ResetDirtyAll();
                }
            }

            foreach (CountrySettings settings in Countries)
            {
                settings.ResetDirtyAll();
            }

            foreach (ProvinceSettings settings in Provinces)
            {
                settings.ResetDirtyAll();
            }

            _dirtyProvinces = false;
            _dirtyVpInc = false;

            _dirtyFlag = false;
        }

        #endregion
    }

    #endregion

    #region Scenario header

    /// <summary>
    ///     Scenario header
    /// </summary>
    public class ScenarioHeader
    {
        #region Public properties

        /// <summary>
        ///     Scenario header name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Start date and time
        /// </summary>
        public GameDate StartDate { get; set; }

        /// <summary>
        ///     Start year
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        ///     End year
        /// </summary>
        public int EndYear { get; set; }

        /// <summary>
        ///     Free choice of the nation
        /// </summary>
        public bool IsFreeSelection { get; set; } = true;

        /// <summary>
        ///     Short scenario
        /// </summary>
        public bool IsBattleScenario { get; set; }

        /// <summary>
        ///     Selectable nations
        /// </summary>
        public List<Country> SelectableCountries { get; } = new List<Country>();

        /// <summary>
        ///     Major country setting
        /// </summary>
        public List<MajorCountrySettings> MajorCountries { get; } = new List<MajorCountrySettings>();

        /// <summary>
        ///     AI Aggression
        /// </summary>
        public int AiAggressive { get; set; } = AiAggressiveDefault;

        /// <summary>
        ///     difficulty
        /// </summary>
        public int Difficulty { get; set; } = DifficultyDefault;

        /// <summary>
        ///     Game speed
        /// </summary>
        public int GameSpeed { get; set; } = GameSpeedDefault;

        #endregion

        #region Public constant

        /// <summary>
        ///     AI Initial value of aggression
        /// </summary>
        public const int AiAggressiveDefault = 2;

        /// <summary>
        ///     Initial value of difficulty
        /// </summary>
        public const int DifficultyDefault = 2;

        /// <summary>
        ///     Initial value of game speed
        /// </summary>
        public const int GameSpeedDefault = 3;

        /// <summary>
        ///     AI Number of aggression options
        /// </summary>
        public const int AiAggressiveCount = 5;

        /// <summary>
        ///     Number of difficulty options
        /// </summary>
        public const int DifficultyCount = 5;

        /// <summary>
        ///     Number of game speed options
        /// </summary>
        public const int GameSpeedCount = 8;

        #endregion
    }

    /// <summary>
    ///     Major country setting
    /// </summary>
    public class MajorCountrySettings
    {
        #region Public properties

        /// <summary>
        ///     Country tag
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        ///     Country name definition
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Flag extension
        /// </summary>
        public string FlagExt { get; set; }

        /// <summary>
        ///     Explanatory text
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        ///     National strategy
        /// </summary>
        public string CountryTactics { get; set; }

        /// <summary>
        ///     Propaganda image name
        /// </summary>
        public string PictureName { get; set; }

        /// <summary>
        ///     Music file name
        /// </summary>
        public string Songs { get; set; }

        /// <summary>
        ///     Placed at the right end
        /// </summary>
        public bool Bottom { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            NameKey, // Country name definition
            NameString, // Country name string
            FlagExt, // Flag suffix
            DescKey, // Description definition
            DescString, // Descriptive text string
            CountryTactics, // National strategy
            PictureName, // Propaganda image name
            Songs, // Music file name
            Bottom // Placed at the right end
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the item has been edited
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
        }

        #endregion
    }

    #endregion

    #region Scenario global data

    /// <summary>
    ///     Scenario global data
    /// </summary>
    public class ScenarioGlobalData
    {
        #region Public properties

        /// <summary>
        ///     Rule setting
        /// </summary>
        public ScenarioRules Rules { get; set; }

        /// <summary>
        ///     Start date and time
        /// </summary>
        public GameDate StartDate { get; set; }

        /// <summary>
        ///     End date and time
        /// </summary>
        public GameDate EndDate { get; set; }

        /// <summary>
        ///     Axis country
        /// </summary>
        public Alliance Axis { get; set; }

        /// <summary>
        ///     Allied
        /// </summary>
        public Alliance Allies { get; set; }

        /// <summary>
        ///     Communist country
        /// </summary>
        public Alliance Comintern { get; set; }

        /// <summary>
        ///     Alliance list
        /// </summary>
        public List<Alliance> Alliances { get; } = new List<Alliance>();

        /// <summary>
        ///     War list
        /// </summary>
        public List<War> Wars { get; } = new List<War>();

        /// <summary>
        ///     Non-invasion treaty list
        /// </summary>
        public List<Treaty> NonAggressions { get; } = new List<Treaty>();

        /// <summary>
        ///     Peace Treaty List
        /// </summary>
        public List<Treaty> Peaces { get; } = new List<Treaty>();

        /// <summary>
        ///     Trade list
        /// </summary>
        public List<Treaty> Trades { get; } = new List<Treaty>();

        /// <summary>
        ///     Global flag list
        /// </summary>
        public Dictionary<string, string> Flags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        ///     Waiting event list
        /// </summary>
        public List<QueuedEvent> QueuedEvents { get; } = new List<QueuedEvent>();

        /// <summary>
        ///     Pause commander
        /// </summary>
        public List<int> DormantLeaders { get; } = new List<int>();

        /// <summary>
        ///     Pause minister
        /// </summary>
        public List<int> DormantMinisters { get; } = new List<int>();

        /// <summary>
        ///     Rest research institution
        /// </summary>
        public List<int> DormantTeams { get; } = new List<int>();

        /// <summary>
        ///     Pause all commanders
        /// </summary>
        public bool DormantLeadersAll { get; set; }

        /// <summary>
        ///     Weather settings
        /// </summary>
        public Weather Weather { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public ScenarioGlobalData()
        {
            // null null Delete if operation can be guaranteed
            Rules = new ScenarioRules();
            Axis = new Alliance();
            Allies = new Alliance();
            Comintern = new Alliance();
        }

        #endregion
    }

    /// <summary>
    ///     Waiting event
    /// </summary>
    public class QueuedEvent
    {
        #region Public properties

        /// <summary>
        ///     Country of origin of the event
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        ///     event ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Event occurrence wait time
        /// </summary>
        public int Hour { get; set; }

        #endregion
    }

    /// <summary>
    ///     Rule setting
    /// </summary>
    public class ScenarioRules
    {
        #region Public properties

        /// <summary>
        ///     Allow diplomacy
        /// </summary>
        public bool AllowDiplomacy { get; set; }

        /// <summary>
        ///     Allow production
        /// </summary>
        public bool AllowProduction { get; set; }

        /// <summary>
        ///     Allow technology development
        /// </summary>
        public bool AllowTechnology { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public ScenarioRules()
        {
            AllowDiplomacy = true;
            AllowProduction = true;
            AllowTechnology = true;
        }

        #endregion
    }

    #endregion

    #region weather

    /// <summary>
    ///     Weather settings
    /// </summary>
    public class Weather
    {
        #region Public properties

        /// <summary>
        ///     Fixed setting
        /// </summary>
        public bool Static { get; set; }

        /// <summary>
        ///     Weather pattern
        /// </summary>
        public List<WeatherPattern> Patterns { get; } = new List<WeatherPattern>();

        #endregion
    }

    /// <summary>
    ///     Weather pattern
    /// </summary>
    public class WeatherPattern
    {
        #region Public properties

        /// <summary>
        ///     type When id id Pair of
        /// </summary>
        public TypeId Id { get; set; }

        /// <summary>
        ///     Provincial list
        /// </summary>
        public List<int> Provinces { get; } = new List<int>();

        /// <summary>
        ///     Central Providence
        /// </summary>
        public int Centre { get; set; }

        /// <summary>
        ///     speed
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        ///     direction
        /// </summary>
        public string Heading { get; set; }

        #endregion
    }

    /// <summary>
    ///     Type of weather
    /// </summary>
    public enum WeatherType
    {
        None,
        Clear, // Sunny
        Frozen, // Below freezing
        Raining, // rainfall
        Snowing, // snowfall
        Storm, // storm
        Blizzard, // Snowstorm
        Muddy // Muddy land
    }

    #endregion

    #region map

    /// <summary>
    ///     Map settings
    /// </summary>
    public class MapSettings
    {
        #region Public properties

        /// <summary>
        ///     Whether all provisions are valid
        /// </summary>
        public bool All { get; set; } = true;

        /// <summary>
        ///     Effective provision
        /// </summary>
        public List<int> Yes { get; } = new List<int>();

        /// <summary>
        ///     Invalid Providence
        /// </summary>
        public List<int> No { get; } = new List<int>();

        /// <summary>
        ///     Map range(( upper left )
        /// </summary>
        public MapPoint Top { get; set; }

        /// <summary>
        ///     Map range (( Bottom right )
        /// </summary>
        public MapPoint Bottom { get; set; }

        #endregion
    }

    /// <summary>
    ///     Map coordinates
    /// </summary>
    public class MapPoint
    {
        #region Public properties

        /// <summary>
        ///     X Coordinate
        /// </summary>
        public int X { get; set; }

        /// <summary>
        ///     Y Coordinate
        /// </summary>
        public int Y { get; set; }

        #endregion
    }

    #endregion

    #region Providence

    /// <summary>
    ///     Province settings
    /// </summary>
    public class ProvinceSettings
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
        ///     Factory size
        /// </summary>
        public BuildingSize Ic { get; set; }

        /// <summary>
        ///     Infrastructure size
        /// </summary>
        public BuildingSize Infrastructure { get; set; }

        /// <summary>
        ///     Land fortress size
        /// </summary>
        public BuildingSize LandFort { get; set; }

        /// <summary>
        ///     Coastal fortress size
        /// </summary>
        public BuildingSize CoastalFort { get; set; }

        /// <summary>
        ///     Anti-aircraft gun size
        /// </summary>
        public BuildingSize AntiAir { get; set; }

        /// <summary>
        ///     Air force base size
        /// </summary>
        public BuildingSize AirBase { get; set; }

        /// <summary>
        ///     Naval base size
        /// </summary>
        public BuildingSize NavalBase { get; set; }

        /// <summary>
        ///     Radar base size
        /// </summary>
        public BuildingSize RadarStation { get; set; }

        /// <summary>
        ///     Reactor size
        /// </summary>
        public BuildingSize NuclearReactor { get; set; }

        /// <summary>
        ///     Rocket test site size
        /// </summary>
        public BuildingSize RocketTest { get; set; }

        /// <summary>
        ///     Synthetic oil factory size
        /// </summary>
        public BuildingSize SyntheticOil { get; set; }

        /// <summary>
        ///     Synthetic material factory size
        /// </summary>
        public BuildingSize SyntheticRares { get; set; }

        /// <summary>
        ///     Nuclear power plant size
        /// </summary>
        public BuildingSize NuclearPower { get; set; }

        /// <summary>
        ///     Stockpile of supplies
        /// </summary>
        public double SupplyPool { get; set; }

        /// <summary>
        ///     Oil reserves
        /// </summary>
        public double OilPool { get; set; }

        /// <summary>
        ///     Energy stockpile
        /// </summary>
        public double EnergyPool { get; set; }

        /// <summary>
        ///     Metal stockpile
        /// </summary>
        public double MetalPool { get; set; }

        /// <summary>
        ///     Stock of rare resources
        /// </summary>
        public double RareMaterialsPool { get; set; }

        /// <summary>
        ///     Energy output
        /// </summary>
        public double Energy { get; set; }

        /// <summary>
        ///     Maximum energy output
        /// </summary>
        public double MaxEnergy { get; set; }

        /// <summary>
        ///     Metal output
        /// </summary>
        public double Metal { get; set; }

        /// <summary>
        ///     Maximum metal output
        /// </summary>
        public double MaxMetal { get; set; }

        /// <summary>
        ///     Rare resource output
        /// </summary>
        public double RareMaterials { get; set; }

        /// <summary>
        ///     Maximum scarce resource output
        /// </summary>
        public double MaxRareMaterials { get; set; }

        /// <summary>
        ///     Oil output
        /// </summary>
        public double Oil { get; set; }

        /// <summary>
        ///     Maximum oil output
        /// </summary>
        public double MaxOil { get; set; }

        /// <summary>
        ///     Human resources
        /// </summary>
        public double Manpower { get; set; }

        /// <summary>
        ///     Maximum human resources
        /// </summary>
        public double MaxManpower { get; set; }

        /// <summary>
        ///     Victory points
        /// </summary>
        public int Vp { get; set; }

        /// <summary>
        ///     Rebellion rate
        /// </summary>
        public double RevoltRisk { get; set; }

        /// <summary>
        ///     weather
        /// </summary>
        public WeatherType Weather { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Public constant

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            NameKey, // Province name key
            NameString, // Providence name string
            Ic, // I C
            MaxIc, // maximum I C
            RelativeIc, // relative I C
            Infrastructure, // infrastructure
            MaxInfrastructure, // Maximum infrastructure
            RelativeInfrastructure, // Relative infrastructure
            LandFort, // Land fortress
            MaxLandFort, // Largest land fortress
            RelativeLandFort, // Relative land fortress
            CoastalFort, // Coastal fortress
            MaxCoastalFort, // Largest coastal fortress
            RelativeCoastalFort, // Relative coastal fortress
            AntiAir, // Anti-aircraft gun
            MaxAntiAir, // Maximum anti-aircraft gun
            RelativeAntiAir, // Relative anti-aircraft gun
            AirBase, // Air Force Base
            MaxAirBase, // Largest air force base
            RelativeAirBase, // Relative air force base
            NavalBase, // Navy base
            MaxNavalBase, // Largest naval base
            RelativeNavalBase, // Relative naval base
            RadarStation, // Radar base
            MaxRadarStation, // Largest radar base
            RelativeRadarStation, // Relative radar base
            NuclearReactor, // Reactor
            MaxNuclearReactor, // Largest reactor
            RelativeNuclearReactor, // Relative reactor
            RocketTest, // Rocket test site
            MaxRocketTest, // Largest rocket test site
            RelativeRocketTest, // Relative rocket test site
            SyntheticOil, // Synthetic oil factory
            MaxSyntheticOil, // Largest synthetic oil factory
            RelativeSyntheticOil, // Relative synthetic oil factory
            SyntheticRares, // Synthetic material factory
            MaxSyntheticRares, // Largest synthetic material factory
            RelativeSyntheticRares, // Relative synthetic material factory
            NuclearPower, // Nuclear power plant
            MaxNuclearPower, // Largest nuclear power plant
            RelativeNuclearPower, // Relative nuclear power plant
            SupplyPool, // Stock of supplies
            OilPool, // Oil reserves
            EnergyPool, // Energy reserve
            MetalPool, // Metal reserve
            RareMaterialsPool, // Rare resource stockpile
            Energy, // Energy output
            MaxEnergy, // Maximum energy output
            Metal, // Metal output
            MaxMetal, // Maximum metal output
            RareMaterials, // Rare resource output
            MaxRareMaterials, // Maximum scarce resource output
            Oil, // Oil output
            MaxOil, // Maximum oil output
            Manpower, // Human resources
            MaxManpower, // Maximum human resources
            Vp, // Victory points
            RevoltRisk // Rebellion rate
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the provision settings have been edited
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
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    #endregion

    #region building

    /// <summary>
    ///     Building size
    /// </summary>
    public class BuildingSize
    {
        #region Public properties

        /// <summary>
        ///     Relative size
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        ///     Maximum size
        /// </summary>
        public double MaxSize { get; set; }

        /// <summary>
        ///     Current size
        /// </summary>
        public double CurrentSize { get; set; }

        #endregion
    }

    /// <summary>
    ///     Building information in production
    /// </summary>
    public class BuildingDevelopment
    {
        #region Public properties

        /// <summary>
        ///     type When id id Pair of
        /// </summary>
        public TypeId Id { get; set; }

        /// <summary>
        ///     name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Building type
        /// </summary>
        public BuildingType Type { get; set; }

        /// <summary>
        ///     position
        /// </summary>
        public int Location { get; set; }

        /// <summary>
        ///     requirement I C
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        ///     Necessary human resources
        /// </summary>
        public double Manpower { get; set; }

        /// <summary>
        ///     Completion date
        /// </summary>
        public GameDate Date { get; set; }

        /// <summary>
        ///     Progress rate increment
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        ///     Total progress rate
        /// </summary>
        public double TotalProgress { get; set; }

        /// <summary>
        ///     Continuous production bonus
        /// </summary>
        public double GearingBonus { get; set; }

        /// <summary>
        ///     Continuous production
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        ///     Number of completed production
        /// </summary>
        public int Done { get; set; }

        /// <summary>
        ///     Days to complete
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        ///     the first 1 Number of days to complete the unit
        /// </summary>
        public int DaysForFirst { get; set; }

        /// <summary>
        ///     Stopping
        /// </summary>
        public bool Halted { get; set; }

        /// <summary>
        ///     Whether to delete the queue on completion
        /// </summary>
        public bool CloseWhenFinished { get; set; }

        /// <summary>
        ///     details unknown
        /// </summary>
        public bool WaitingForClosure { get; set; }

        /// <summary>
        ///     Speed Step of production
        /// </summary>
        public int SpeedStep { get; set; }

        #endregion
    }

    /// <summary>
    ///     Building type
    /// </summary>
    public enum BuildingType
    {
        None,
        Ic, // plant
        Infrastructure, // infrastructure
        CoastalFort, // Coastal fortress
        LandFort, // Land fortress
        AntiAir, // Anti-aircraft gun
        AirBase, // Air base
        NavalBase, // Navy base
        RadarStation, // Radar base
        NuclearReactor, // Reactor
        RocketTest, // Rocket test site
        SyntheticOil, // Synthetic oil factory
        SyntheticRares, // Synthetic material factory
        NuclearPower // Nuclear power plant
    }

    #endregion

    #region Diplomatic

    /// <summary>
    ///     Alliance setting
    /// </summary>
    public class Alliance
    {
        #region Public properties

        /// <summary>
        ///     type When id id Pair of
        /// </summary>
        public TypeId Id { get; set; }

        /// <summary>
        ///     Participating countries
        /// </summary>
        public List<Country> Participant { get; } = new List<Country>();

        /// <summary>
        ///     Alliance name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Whether it is a defense alliance
        /// </summary>
        public bool Defensive { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flags of participating countries
        /// </summary>
        private readonly HashSet<Country> _dirtyCountries = new HashSet<Country>();

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            Type, // type
            Id, // id id
            Name // Alliance name
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the item has been edited
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
        }

        /// <summary>
        ///     Get if the target country has been edited
        /// </summary>
        /// <param name="country">Target country</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyCountry(Country country)
        {
            return _dirtyCountries.Contains(country);
        }

        /// <summary>
        ///     Set edited flags for participating countries
        /// </summary>
        /// <param name="country">Target country</param>
        public void SetDirtyCountry(Country country)
        {
            _dirtyCountries.Add(country);
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }

            _dirtyCountries.Clear();
        }

        #endregion
    }

    /// <summary>
    ///     War setting
    /// </summary>
    public class War
    {
        #region Public properties

        /// <summary>
        ///     type When id id Pair of
        /// </summary>
        public TypeId Id { get; set; }

        /// <summary>
        ///     Start date and time
        /// </summary>
        public GameDate StartDate { get; set; }

        /// <summary>
        ///     End date and time
        /// </summary>
        public GameDate EndDate { get; set; }

        /// <summary>
        ///     Attacking Participating Countries
        /// </summary>
        public Alliance Attackers { get; set; }

        /// <summary>
        ///     Defender Participating Countries
        /// </summary>
        public Alliance Defenders { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flags of participating countries
        /// </summary>
        private readonly HashSet<Country> _dirtyCountries = new HashSet<Country>();

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            Type, // type
            Id, // id id
            StartYear, // Start year
            StartMonth, // Start month
            StartDay, // start date
            EndYear, // End year
            EndMonth, // End month
            EndDay, // End date
            AttackerType, // Attacker type
            AttackerId, // Attacker id
            DefenderType, // Defender type
            DefenderId // Defender id
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the item has been edited
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
        }

        /// <summary>
        ///     Get if the target country has been edited
        /// </summary>
        /// <param name="country">Target country</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyCountry(Country country)
        {
            return _dirtyCountries.Contains(country);
        }

        /// <summary>
        ///     Set edited flags for participating countries
        /// </summary>
        /// <param name="country">Target country</param>
        public void SetDirtyCountry(Country country)
        {
            _dirtyCountries.Add(country);
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }

            _dirtyCountries.Clear();
        }

        #endregion
    }

    /// <summary>
    ///     Diplomatic agreement setting
    /// </summary>
    public class Treaty
    {
        #region Public properties

        /// <summary>
        ///     type When id id Pair of
        /// </summary>
        public TypeId Id { get; set; }

        /// <summary>
        ///     Pair of id and type
        /// </summary>
        public TypeId TradeConvoyId { get; set; }

        /// <summary>
        ///     Types of diplomatic agreements
        /// </summary>
        public TreatyType Type { get; set; }

        /// <summary>
        ///     Target country 1
        /// </summary>
        public Country Country1 { get; set; }

        /// <summary>
        ///     Target country 2
        /// </summary>
        public Country Country2 { get; set; }

        /// <summary>
        ///     Start date and time
        /// </summary>
        public GameDate StartDate { get; set; }

        /// <summary>
        ///     End date and time
        /// </summary>
        public GameDate EndDate { get; set; }

        /// <summary>
        ///     Funding
        /// </summary>
        public double Money { get; set; }

        /// <summary>
        ///     Supplies
        /// </summary>
        public double Supplies { get; set; }

        /// <summary>
        ///     energy
        /// </summary>
        public double Energy { get; set; }

        /// <summary>
        ///     metal
        /// </summary>
        public double Metal { get; set; }

        /// <summary>
        ///     Rare resources
        /// </summary>
        public double RareMaterials { get; set; }

        /// <summary>
        ///     oil
        /// </summary>
        public double Oil { get; set; }

        /// <summary>
        ///     Whether it can be canceled
        /// </summary>
        public bool Cancel { get; set; } = true;

        /// <summary>
        ///     Whether it is foreign trade
        /// </summary>
        public bool IsOverSea { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Public constant

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            Type, // type
            Id, // id id
            Country1, // Target country 1
            Country2, // Target country 2
            StartYear, // Start year
            StartMonth, // Start month
            StartDay, // start date
            EndYear, // End year
            EndMonth, // End month
            EndDay, // End date
            Money, // Funding
            Supplies, // Supplies
            Energy, // energy
            Metal, // metal
            RareMaterials, // Rare resources
            Oil, // oil
            Cancel, // Whether it can be canceled
            TradeConvoyId
        }

        #endregion

        #region String operation

        /// <summary>
        ///     Get the trade content string
        /// </summary>
        /// <returns>Trade content string</returns>
        public string GetTradeString()
        {
            StringBuilder sb = new StringBuilder();
            if (!DoubleHelper.IsZero(Energy))
            {
                sb.AppendFormat("{0}:{1}, ", Config.GetText(TextId.ResourceEnergy), DoubleHelper.ToString1(Energy));
            }
            if (!DoubleHelper.IsZero(Metal))
            {
                sb.AppendFormat("{0}:{1}, ", Config.GetText(TextId.ResourceMetal), DoubleHelper.ToString1(Metal));
            }
            if (!DoubleHelper.IsZero(RareMaterials))
            {
                sb.AppendFormat("{0}:{1}, ", Config.GetText(TextId.ResourceRareMaterials),
                    DoubleHelper.ToString1(RareMaterials));
            }
            if (!DoubleHelper.IsZero(Oil))
            {
                sb.AppendFormat("{0}:{1}, ", Config.GetText(TextId.ResourceOil), DoubleHelper.ToString1(Oil));
            }
            if (!DoubleHelper.IsZero(Supplies))
            {
                sb.AppendFormat("{0}:{1}, ", Config.GetText(TextId.ResourceSupplies), DoubleHelper.ToString1(Supplies));
            }
            if (!DoubleHelper.IsZero(Money))
            {
                sb.AppendFormat("{0}:{1}, ", Config.GetText(TextId.ResourceMoney), DoubleHelper.ToString1(Money));
            }
            int len = sb.Length;
            return len > 0 ? sb.ToString(0, len - 2) : "";
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the diplomatic agreement settings have been edited
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
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     National relations setting
    /// </summary>
    public class Relation
    {
        #region Public properties

        /// <summary>
        ///     Partner country
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        ///     Relationship value
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        ///     Passage permission
        /// </summary>
        public bool Access { get; set; }

        /// <summary>
        ///     Independence guarantee deadline
        /// </summary>
        public GameDate Guaranteed { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            Value, // Relationship value
            Access, // Passage permission
            Guaranteed, // Independent warranty
            GuaranteedYear, // Independence guarantee expiration year
            GuaranteedMonth, // Independence guarantee deadline month
            GuaranteedDay // Independence guarantee deadline
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the item has been edited
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
        }

        #endregion
    }

    /// <summary>
    ///     Types of diplomatic agreements
    /// </summary>
    public enum TreatyType
    {
        NonAggression, // Non-invasion treaty
        Peace, // Armistice agreement
        Trade // Trade
    }

    #endregion

    #region Nation

    /// <summary>
    ///     National setting
    /// </summary>
    public class CountrySettings
    {
        #region Public properties

        /// <summary>
        ///     file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     Country tag
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        ///     Country name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Flag suffix
        /// </summary>
        public string FlagExt { get; set; }

        /// <summary>
        ///     Brotherhood
        /// </summary>
        public Country RegularId { get; set; }

        /// <summary>
        ///     Independent regime
        /// </summary>
        public GovernmentType IntrinsicGovType { get; set; }

        /// <summary>
        ///     Suzerainty
        /// </summary>
        public Country Master { get; set; }

        /// <summary>
        ///     Country of acquisition of commandership
        /// </summary>
        public Country Control { get; set; }

        /// <summary>
        ///     Warlikeness
        /// </summary>
        public int Belligerence { get; set; }

        /// <summary>
        ///     Additional transport capacity
        /// </summary>
        public double ExtraTc { get; set; }

        /// <summary>
        ///     National dissatisfaction
        /// </summary>
        public double Dissent { get; set; }

        /// <summary>
        ///     capital
        /// </summary>
        public int Capital { get; set; }

        /// <summary>
        ///     TC correction
        /// </summary>
        public double TcModifier { get; set; }

        /// <summary>
        ///     Occupied territory TCcorrection
        /// </summary>
        public double TcOccupiedModifier { get; set; }

        /// <summary>
        ///     Consumption compensation
        /// </summary>
        public double AttritionModifier { get; set; }

        /// <summary>
        ///     Gradual withdrawal correction
        /// </summary>
        public double TricklebackModifier { get; set; }

        /// <summary>
        ///     Maximum assault landing correction
        /// </summary>
        public int MaxAmphibModifier { get; set; }

        /// <summary>
        ///     Replenishment correction
        /// </summary>
        public double SupplyDistModifier { get; set; }

        /// <summary>
        ///     Repair correction
        /// </summary>
        public double RepairModifier { get; set; }

        /// <summary>
        ///     Research correction
        /// </summary>
        public double ResearchModifier { get; set; }

        /// <summary>
        ///     Peacetime I C correction
        /// </summary>
        public double PeacetimeIcModifier { get; set; }

        /// <summary>
        ///     War time I C correction
        /// </summary>
        public double WartimeIcModifier { get; set; }

        /// <summary>
        ///     Industrial power correction
        /// </summary>
        public double IndustrialModifier { get; set; }

        /// <summary>
        ///     Ground defense correction
        /// </summary>
        public double GroundDefEff { get; set; }

        /// <summary>
        ///     AI file name
        /// </summary>
        public string AiFileName { get; set; }

        /// <summary>
        ///     AI setting
        /// </summary>
        public AiSettings AiSettings { get; set; }

        /// <summary>
        ///     Human resources
        /// </summary>
        public double Manpower { get; set; }

        /// <summary>
        ///     Human resource correction value
        /// </summary>
        public double RelativeManpower { get; set; }

        /// <summary>
        ///     energy
        /// </summary>
        public double Energy { get; set; }

        /// <summary>
        ///     metal
        /// </summary>
        public double Metal { get; set; }

        /// <summary>
        ///     Rare resources
        /// </summary>
        public double RareMaterials { get; set; }

        /// <summary>
        ///     oil
        /// </summary>
        public double Oil { get; set; }

        /// <summary>
        ///     Supplies
        /// </summary>
        public double Supplies { get; set; }

        /// <summary>
        ///     Funding
        /// </summary>
        public double Money { get; set; }

        /// <summary>
        ///     Transport fleet
        /// </summary>
        public int Transports { get; set; }

        /// <summary>
        ///     Escort ship
        /// </summary>
        public int Escorts { get; set; }

        /// <summary>
        ///     nuclear weapons
        /// </summary>
        public int Nuke { get; set; }

        /// <summary>
        ///     Off-map resources
        /// </summary>
        public ResourceSettings Offmap { get; set; }

        /// <summary>
        ///     Consumer goods I C ratio
        /// </summary>
        public double ConsumerSlider { get; set; }

        /// <summary>
        ///     Supplies I C ratio
        /// </summary>
        public double SupplySlider { get; set; }

        /// <summary>
        ///     production I C ratio
        /// </summary>
        public double ProductionSlider { get; set; }

        /// <summary>
        ///     Replenishment I C ratio
        /// </summary>
        public double ReinforcementSlider { get; set; }

        /// <summary>
        ///     upgrading I C ratio
        /// </summary>
        public double UpgradingSlider { get; set; }

        /// <summary>
        ///     Expense Synthetic
        /// </summary>
        public double ExpenseSyntheticSlider { get; set; }

        /// <summary>
        ///     Expense Synthetic Rares
        /// </summary>
        public double ExpenseSyntheticRaresSlider { get; set; }


        /// <summary>
        ///     Diplomatic relations
        /// </summary>
        public List<Relation> Relations { get; } = new List<Relation>();

        /// <summary>
        ///     Intelligence information
        /// </summary>
        public List<SpySettings> Intelligence { get; } = new List<SpySettings>();

        /// <summary>
        ///     Core Providence
        /// </summary>
        public List<int> NationalProvinces { get; } = new List<int>();

        /// <summary>
        ///     Owned Providence
        /// </summary>
        public List<int> OwnedProvinces { get; } = new List<int>();

        /// <summary>
        ///     Domination province
        /// </summary>
        public List<int> ControlledProvinces { get; } = new List<int>();

        /// <summary>
        ///     Province claim
        /// </summary>
        public List<int> ClaimedProvinces { get; } = new List<int>();

        /// <summary>
        ///     Owned technology
        /// </summary>
        public List<int> TechApps { get; } = new List<int>();

        /// <summary>
        ///     Blueprint
        /// </summary>
        public List<int> BluePrints { get; } = new List<int>();

        /// <summary>
        ///     Invention event
        /// </summary>
        public List<int> Inventions { get; } = new List<int>();

        /// <summary>
        ///     Invalid technology
        /// </summary>
        public List<int> Deactivate { get; } = new List<int>();

        /// <summary>
        ///     Policy slider
        /// </summary>
        public CountryPolicy Policy { get; set; }

        /// <summary>
        ///     Date and time of completion of nuclear weapons
        /// </summary>
        public GameDate NukeDate { get; set; }

        /// <summary>
        ///     Head of State
        /// </summary>
        public TypeId HeadOfState { get; set; }

        /// <summary>
        ///     Government leaders
        /// </summary>
        public TypeId HeadOfGovernment { get; set; }

        /// <summary>
        ///     Minister of Foreign Affairs
        /// </summary>
        public TypeId ForeignMinister { get; set; }

        /// <summary>
        ///     Minister of Munitions
        /// </summary>
        public TypeId ArmamentMinister { get; set; }

        /// <summary>
        ///     Minister of Interior
        /// </summary>
        public TypeId MinisterOfSecurity { get; set; }

        /// <summary>
        ///     Minister of Information
        /// </summary>
        public TypeId MinisterOfIntelligence { get; set; }

        /// <summary>
        ///     Chief of the Defense Staff
        /// </summary>
        public TypeId ChiefOfStaff { get; set; }

        /// <summary>
        ///     Commander General of the Army
        /// </summary>
        public TypeId ChiefOfArmy { get; set; }

        /// <summary>
        ///     Navy Commander
        /// </summary>
        public TypeId ChiefOfNavy { get; set; }

        /// <summary>
        ///     Air Force Commander
        /// </summary>
        public TypeId ChiefOfAir { get; set; }

        /// <summary>
        ///     Public awareness
        /// </summary>
        public string NationalIdentity { get; set; }

        /// <summary>
        ///     Social policy
        /// </summary>
        public string SocialPolicy { get; set; }

        /// <summary>
        ///     National culture
        /// </summary>
        public string NationalCulture { get; set; }

        /// <summary>
        ///     Pause commander
        /// </summary>
        public List<int> DormantLeaders { get; } = new List<int>();

        /// <summary>
        ///     Pause minister
        /// </summary>
        public List<int> DormantMinisters { get; } = new List<int>();

        /// <summary>
        ///     Rest research institution
        /// </summary>
        public List<int> DormantTeams { get; } = new List<int>();

        /// <summary>
        ///     Extraction commander
        /// </summary>
        public List<int> StealLeaders { get; } = new List<int>();

        /// <summary>
        ///     Producible division
        /// </summary>
        public Dictionary<UnitType, bool> AllowedDivisions { get; } = new Dictionary<UnitType, bool>();

        /// <summary>
        ///     Producible brigade
        /// </summary>
        public Dictionary<UnitType, bool> AllowedBrigades { get; } = new Dictionary<UnitType, bool>();

        /// <summary>
        ///     Transport fleet
        /// </summary>
        public List<Convoy> Convoys { get; } = new List<Convoy>();

        /// <summary>
        ///     Army unit
        /// </summary>
        public List<Unit> LandUnits { get; } = new List<Unit>();

        /// <summary>
        ///     Navy unit
        /// </summary>
        public List<Unit> NavalUnits { get; } = new List<Unit>();

        /// <summary>
        ///     Air Force Unit
        /// </summary>
        public List<Unit> AirUnits { get; } = new List<Unit>();

        /// <summary>
        ///     During production division
        /// </summary>
        public List<DivisionDevelopment> DivisionDevelopments { get; } = new List<DivisionDevelopment>();

        /// <summary>
        ///     During production brigade
        /// </summary>
        public List<DivisionDevelopment> BrigadeDevelopments { get; } = new List<DivisionDevelopment>();

        /// <summary>
        ///     Convoy in production
        /// </summary>
        public List<ConvoyDevelopment> ConvoyDevelopments { get; } = new List<ConvoyDevelopment>();

        /// <summary>
        ///     Building in production
        /// </summary>
        public List<BuildingDevelopment> BuildingDevelopments { get; } = new List<BuildingDevelopment>();

        /// <summary>
        ///     Army Division
        /// </summary>
        public List<Division> LandDivisions { get; } = new List<Division>();

        /// <summary>
        ///     Navy Division
        /// </summary>
        public List<Division> NavalDivisions { get; } = new List<Division>();

        /// <summary>
        ///     Luftwaffe Division
        /// </summary>
        public List<Division> AirDivisions { get; } = new List<Division>();

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        /// <summary>
        ///     Edited flag of owned technology
        /// </summary>
        private readonly HashSet<int> _dirtyOwnedTechs = new HashSet<int>();

        /// <summary>
        ///     Edited flag for blueprint
        /// </summary>
        private readonly HashSet<int> _dirtyBlueprints = new HashSet<int>();

        /// <summary>
        ///     Edited flag for invention event
        /// </summary>
        private readonly HashSet<int> _dirtyInventions = new HashSet<int>();

        /// <summary>
        ///     Edited flags for core provinces
        /// </summary>
        private readonly HashSet<int> _dirtyCoreProvinces = new HashSet<int>();

        /// <summary>
        ///     Edited flag of possessed Providence
        /// </summary>
        private readonly HashSet<int> _dirtyOwnedProvinces = new HashSet<int>();

        /// <summary>
        ///     Edited flag of dominance province
        /// </summary>
        private readonly HashSet<int> _dirtyControlledProvinces = new HashSet<int>();

        /// <summary>
        ///     Edited flag of territorial claim province
        /// </summary>
        private readonly HashSet<int> _dirtyClaimedProvinces = new HashSet<int>();

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Public constant

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            NameKey, // Country name definition
            NameString, // Country name string
            FlagExt, // Flag suffix
            RegularId, // Brotherhood
            IntrinsicGovType, // Independent regime
            Master, // Suzerainty
            Control, // Country of acquisition of commandership
            Belligerence, // Warlikeness
            ExtraTc, // Additional transport capacity
            Dissent, // National dissatisfaction
            Capital, // capital
            PeacetimeIcModifier, // Normal time I C correction
            WartimeIcModifier, // War time I C correction
            IndustrialModifier, // Industrial power correction
            GroundDefEff, // Ground defense correction
            AiFileName, // AI file name
            Manpower, // Human resources
            RelativeManpower, // Human resource correction value
            Energy, // energy
            Metal, // metal
            RareMaterials, // Rare resources
            Oil, // oil
            Supplies, // Supplies
            Money, // Funding
            Transports, // Transport fleet
            Escorts, // Escort ship
            OffmapIc, // Off-map industrial strength
            OffmapManpower, // Foreign resources on the map
            OffmapEnergy, // Off-map energy
            OffmapMetal, // Off-map metal
            OffmapRareMaterials, // Rare resources off the map
            OffmapOil, // Off-map oil
            OffmapSupplies, // Map foreign goods
            OffmapMoney, // Off-map funds
            OffmapTransports, // Off-map convoy
            OffmapEscorts, // Off-map escort ship
            ConsumerSlider, // Consumer goods I C ratio
            SupplySlider, // Supplies I C ratio
            ProductionSlider, // production I C ratio
            ReinforcementSlider, // Replenishment I C ratio
            SliderYear, // Slider movable year
            SliderMonth, // Slider movable month
            SliderDay, // Slider movable date
            Democratic, // Democratic ――――Dictatorship
            PoliticalLeft, // Political left ―――― Political right
            Freedom, // Open society ―――― Closed society
            FreeMarket, // Free economy ―――― Central planned economy
            ProfessionalArmy, // Standing army ―――― Recruitment army
            DefenseLobby, // Taka faction ―――― Pigeon faction
            Interventionism, // Interventionism ―――― Isolation
            Nuke, // nuclear weapons
            NukeYear, // Year of nuclear weapon production
            NukeMonth, // Nuclear weapon production month
            NukeDay, // Nuclear weapon production date
            HeadOfStateType, // Of the head of state type
            HeadOfGovernmentType, // Of the government leaders type
            ForeignMinisterType, // Foreign Minister type
            ArmamentMinisterType, // Of the Minister of Military Demand type
            MinisterOfSecurityType, // Of the Minister of Interior type
            MinisterOfIntelligenceType, // Information Minister type
            ChiefOfStaffType, // Of the Integrated Chief of Staff type
            ChiefOfArmyType, // Commander General of the Army type
            ChiefOfNavyType, // Of the Navy Commander type
            ChiefOfAirType, // Air Force General Commander type
            HeadOfStateId, // Of the head of state id id
            HeadOfGovernmentId, // Of the government leaders id id
            ForeignMinisterId, // Foreign Minister id id
            ArmamentMinisterId, // Of the Minister of Military Demand id id
            MinisterOfSecurityId, // Of the Minister of Interior id id
            MinisterOfIntelligenceId, // Information Minister id id
            ChiefOfStaffId, // Of the Integrated Chief of Staff id id
            ChiefOfArmyId, // Of the Army General Commanderid id
            ChiefOfNavyId, // Of the Navy Commander id
            ChiefOfAirId // Air Force General Commander id
        }

        #endregion

        #region type When id Group operation

        /// <summary>
        ///     New id id To get
        /// </summary>
        /// <returns>New id</returns>
        public TypeId GetNewUnitTypeId()
        {
            return Scenarios.GetNewTypeId(
                LandUnits.Count > 0 && LandUnits[0].Id != null ? LandUnits[0].Id.Type : Scenarios.GetNewType(1), 1);
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the national settings have been edited
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
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Get if the target technology has been edited
        /// </summary>
        /// <param name="id">Technology ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyOwnedTech(int id)
        {
            return _dirtyOwnedTechs.Contains(id);
        }

        /// <summary>
        ///     Set the edited flag of the possessed technology
        /// </summary>
        /// <param name="id">Technology ID</param>
        public void SetDirtyOwnedTech(int id)
        {
            _dirtyOwnedTechs.Add(id);
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Get if the target blueprint has been edited
        /// </summary>
        /// <param name="id">Technology ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyBlueprint(int id)
        {
            return _dirtyBlueprints.Contains(id);
        }

        /// <summary>
        ///     Set the edited flag for blueprints
        /// </summary>
        /// <param name="id">Technology ID</param>
        public void SetDirtyBlueprint(int id)
        {
            _dirtyBlueprints.Add(id);
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Get if the subject invention event has been edited
        /// </summary>
        /// <param name="id">event ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyInvention(int id)
        {
            return _dirtyInventions.Contains(id);
        }

        /// <summary>
        ///     Set the edited flag for an invention event
        /// </summary>
        /// <param name="id">event ID</param>
        public void SetDirtyInvention(int id)
        {
            _dirtyInventions.Add(id);
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Get if the target core province has been edited
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyCoreProvinces(int id)
        {
            return _dirtyCoreProvinces.Contains(id);
        }

        /// <summary>
        ///     Set edited flags for core provinces
        /// </summary>
        /// <param name="id">Providence ID</param>
        public void SetDirtyCoreProvinces(int id)
        {
            _dirtyCoreProvinces.Add(id);
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Get if the target's possessed province has been edited
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyOwnedProvinces(int id)
        {
            return _dirtyOwnedProvinces.Contains(id);
        }

        /// <summary>
        ///     Set the edited flag for your province
        /// </summary>
        /// <param name="id">Providence ID</param>
        public void SetDirtyOwnedProvinces(int id)
        {
            _dirtyOwnedProvinces.Add(id);
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Gets whether the target's dominance province has been edited
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyControlledProvinces(int id)
        {
            return _dirtyControlledProvinces.Contains(id);
        }

        /// <summary>
        ///     Set the edited flag for the dominant province
        /// </summary>
        /// <param name="id">Providence ID</param>
        public void SetDirtyControlledProvinces(int id)
        {
            _dirtyControlledProvinces.Add(id);
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Gets whether the subject's claim of sovereignty province has been edited
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyClaimedProvinces(int id)
        {
            return _dirtyClaimedProvinces.Contains(id);
        }

        /// <summary>
        ///     Set Edited Flags for Province Claims
        /// </summary>
        /// <param name="id">Providence ID</param>
        public void SetDirtyClaimedProvinces(int id)
        {
            _dirtyClaimedProvinces.Add(id);
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;

            _dirtyOwnedTechs.Clear();
            _dirtyBlueprints.Clear();
            _dirtyInventions.Clear();
            _dirtyCoreProvinces.Clear();
            _dirtyOwnedProvinces.Clear();
            _dirtyControlledProvinces.Clear();
            _dirtyClaimedProvinces.Clear();

            if (Relations != null)
            {
                foreach (Relation relation in Relations)
                {
                    relation.ResetDirtyAll();
                }
            }

            if (Intelligence != null)
            {
                foreach (SpySettings spy in Intelligence)
                {
                    spy.ResetDirtyAll();
                }
            }

            foreach (Unit unit in LandUnits)
            {
                unit.ResetDirtyAll();
            }
            foreach (Unit unit in NavalUnits)
            {
                unit.ResetDirtyAll();
            }
            foreach (Unit unit in AirUnits)
            {
                unit.ResetDirtyAll();
            }

            foreach (Division division in LandDivisions)
            {
                division.ResetDirtyAll();
            }
            foreach (Division division in NavalDivisions)
            {
                division.ResetDirtyAll();
            }
            foreach (Division division in AirDivisions)
            {
                division.ResetDirtyAll();
            }

            foreach (DivisionDevelopment division in DivisionDevelopments)
            {
                division.ResetDirtyAll();
            }

            foreach (DivisionDevelopment division in BrigadeDevelopments)
            {
                division.ResetDirtyAll();
            }
            
        }

        #endregion
    }

    /// <summary>
    ///     AI setting
    /// </summary>
    public class AiSettings
    {
        #region Public properties

        /// <summary>
        ///     Local flag list
        /// </summary>
        public Dictionary<string, string> Flags { get; set; }

        #endregion
    }

    /// <summary>
    ///     Resource setting
    /// </summary>
    public class ResourceSettings
    {
        #region Public properties

        /// <summary>
        ///     Industrial power
        /// </summary>
        public double Ic { get; set; }

        /// <summary>
        ///     Human resources
        /// </summary>
        public double Manpower { get; set; }

        /// <summary>
        ///     energy
        /// </summary>
        public double Energy { get; set; }

        /// <summary>
        ///     metal
        /// </summary>
        public double Metal { get; set; }

        /// <summary>
        ///     Rare resources
        /// </summary>
        public double RareMaterials { get; set; }

        /// <summary>
        ///     oil
        /// </summary>
        public double Oil { get; set; }

        /// <summary>
        ///     Supplies
        /// </summary>
        public double Supplies { get; set; }

        /// <summary>
        ///     Funding
        /// </summary>
        public double Money { get; set; }

        /// <summary>
        ///     Transport fleet
        /// </summary>
        public int Transports { get; set; }

        /// <summary>
        ///     Escort ship
        /// </summary>
        public int Escorts { get; set; }

        #endregion
    }

    /// <summary>
    ///     Intelligence settings
    /// </summary>
    public class SpySettings
    {
        #region Public properties

        /// <summary>
        ///     Partner country
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        ///     Number of spies
        /// </summary>
        public int Spies { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            Spies // Number of spies
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the item has been edited
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
        }

        #endregion
    }

    /// <summary>
    ///     Policy slider
    /// </summary>
    public class CountryPolicy
    {
        #region Public properties

        /// <summary>
        ///     Slider movable date and time
        /// </summary>
        public GameDate Date { get; set; }

        /// <summary>
        ///     Democratic ―――― Dictatorship
        /// </summary>
        public int Democratic { get; set; } = 5;

        /// <summary>
        ///     Political left ―――― Political right
        /// </summary>
        public int PoliticalLeft { get; set; } = 5;

        /// <summary>
        ///     Open society ―――― Closed society
        /// </summary>
        public int Freedom { get; set; } = 5;

        /// <summary>
        ///     Free economy ―――― Central planned economy
        /// </summary>
        public int FreeMarket { get; set; } = 5;

        /// <summary>
        ///     Standing army ―――― Recruitment army (DH Full Then mobilize ―――― Reinstatement )
        /// </summary>
        public int ProfessionalArmy { get; set; } = 5;

        /// <summary>
        ///     Taka faction ―――― Dove
        /// </summary>
        public int DefenseLobby { get; set; } = 5;

        /// <summary>
        ///     Interventionism―――― Isolationism
        /// </summary>
        public int Interventionism { get; set; } = 5;

        #endregion
    }

    /// <summary>
    ///     Polity
    /// </summary>
    public enum GovernmentType
    {
        None,
        Nazi, // State socialism
        Fascist, // fascism
        PaternalAutocrat, // Tyranny
        SocialConservative, // Social conservatives
        MarketLiberal, // Free economics
        SocialLiberal, // Social liberty
        SocialDemocrat, // Social democracy
        LeftWingRadical, // Radical left wing
        Leninist, // Leninism
        Stalinist // Stalinism
    }

    #endregion

    #region unit

    /// <summary>
    ///     unit
    /// </summary>
    public class Unit
    {
        #region Public properties

        /// <summary>
        ///     type When id id Pair of
        /// </summary>
        public TypeId Id { get; set; }

        /// <summary>
        ///     Unit name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Army
        /// </summary>
        public Branch Branch { get; set; }

        /// <summary>
        ///     Commander's country
        /// </summary>
        public Country Control { get; set; }

        /// <summary>
        ///     Commander
        /// </summary>
        public int Leader { get; set; }

        /// <summary>
        ///     present location
        /// </summary>
        public int Location { get; set; }

        /// <summary>
        ///     Immediately before position
        /// </summary>
        public int PrevProv { get; set; }

        /// <summary>
        ///     Reference position
        /// </summary>
        public int Home { get; set; }

        /// <summary>
        ///     Affiliation base
        /// </summary>
        public int Base { get; set; }

        /// <summary>
        ///     塹 壕 level
        /// </summary>
        public double DigIn { get; set; }

        /// <summary>
        ///     morale
        /// </summary>
        public double Morale { get; set; }

        /// <summary>
        ///     mission
        /// </summary>
        public Mission Mission { get; set; }

        /// <summary>
        ///     Specified date and time
        /// </summary>
        public GameDate Date { get; set; }

        /// <summary>
        ///     development ( details unknown )
        /// </summary>
        public bool Development { get; set; } = true;

        /// <summary>
        ///     Move completion date and time
        /// </summary>
        public GameDate MoveTime { get; set; }

        /// <summary>
        ///     Travel route
        /// </summary>
        public List<int> Movement { get; } = new List<int>();

        /// <summary>
        ///     Attack date and time
        /// </summary>
        public GameDate AttackDate { get; set; }

        /// <summary>
        ///     During landing
        /// </summary>
        public bool Invasion { get; set; }

        /// <summary>
        ///     Landing destination
        /// </summary>
        public int Target { get; set; }

        /// <summary>
        ///     Death guard order
        /// </summary>
        public bool StandGround { get; set; }

        /// <summary>
        ///     Burnt soil operation
        /// </summary>
        public bool ScorchGround { get; set; }

        /// <summary>
        ///     priority
        /// </summary>
        public bool Prioritized { get; set; }

        /// <summary>
        ///     Can be improved
        /// </summary>
        public bool CanUpgrade { get; set; }

        /// <summary>
        ///     Can be replenished
        /// </summary>
        public bool CanReinforcement { get; set; }

        /// <summary>
        ///     Composition division
        /// </summary>
        public List<Division> Divisions { get; } = new List<Division>();

        /// <summary>
        ///     On-board unit
        /// </summary>
        public List<Unit> LandUnits { get; } = new List<Unit>();

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            Type,
            Id,
            Name,
            Control,
            Leader,
            Location,
            PrevProv,
            Home,
            Base,
            DigIn,
            Morale,
            Year,
            Month,
            Day,
            Hour,
            Development,
            MoveYear,
            MoveMonth,
            MoveDay,
            MoveHour,
            AttackYear,
            AttackMonth,
            AttackDay,
            AttackHour,
            Invasion,
            Target,
            StandGround,
            ScorchGround,
            Prioritized,
            CanUpgrade,
            CanReinforcement
        }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public Unit()
        {
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">The unit from which it was duplicated</param>
        public Unit(Unit original)
        {
            Id = Scenarios.GetNewTypeId(original.Id.Type, original.Id.Id);
            Name = original.Name;
            Branch = original.Branch;
            Control = original.Control;
            Leader = original.Leader;
            Location = original.Location;
            PrevProv = original.PrevProv;
            Home = original.Home;
            Base = original.Base;
            DigIn = original.DigIn;
            Morale = original.Morale;
            if (original.Mission != null)
            {
                Mission = new Mission(original.Mission);
            }
            if (original.Date != null)
            {
                Date = new GameDate(original.Date);
            }
            Development = original.Development;
            if (original.MoveTime != null)
            {
                MoveTime = new GameDate(original.MoveTime);
            }
            Movement.AddRange(original.Movement);
            if (original.AttackDate != null)
            {
                AttackDate = new GameDate(original.AttackDate);
            }
            Invasion = original.Invasion;
            Target = original.Target;
            StandGround = original.StandGround;
            ScorchGround = original.ScorchGround;
            Prioritized = original.Prioritized;
            CanUpgrade = original.CanUpgrade;
            CanReinforcement = original.CanReinforcement;
            foreach (Division division in original.Divisions)
            {
                Divisions.Add(new Division(division));
            }
            foreach (Unit landUnit in original.LandUnits)
            {
                LandUnits.Add(new Unit(landUnit));
            }
        }

        #endregion

        #region type When id Group operation

        /// <summary>
        ///     type When id Delete the pair of
        /// </summary>
        public void RemoveTypeId()
        {
            Scenarios.RemoveTypeId(Id);
            foreach (Division division in Divisions)
            {
                division.RemoveTypeId();
            }
            foreach (Unit landUnit in LandUnits)
            {
                landUnit.RemoveTypeId();
            }
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the provision settings have been edited
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
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = true;
            }
            _dirtyFlag = true;

            Mission?.SetDirtyAll();

            foreach (Division division in Divisions)
            {
                division.SetDirtyAll();
            }

            foreach (Unit landUnit in LandUnits)
            {
                landUnit.SetDirtyAll();
            }
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;

            Mission?.ResetDirtyAll();

            foreach (Division division in Divisions)
            {
                division.ResetDirtyAll();
            }

            foreach (Unit landUnit in LandUnits)
            {
                landUnit.ResetDirtyAll();
            }
        }

        #endregion
    }

    #endregion

    #region Division

    /// <summary>
    ///     Division
    /// </summary>
    public class Division
    {
        #region Public properties

        /// <summary>
        ///     type When id id Pair of
        /// </summary>
        public TypeId Id { get; set; }

        /// <summary>
        ///     Division name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Army
        /// </summary>
        public Branch Branch { get; set; }

        /// <summary>
        ///     Unit type
        /// </summary>
        public UnitType Type { get; set; }

        /// <summary>
        ///     Model number
        /// </summary>
        public int Model { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     Equipped with nuclear weapons
        /// </summary>
        public bool Nuke { get; set; }

        /// <summary>
        ///     Unit type of attached brigade
        /// </summary>
        public UnitType Extra1 { get; set; }

        /// <summary>
        ///     Unit type of attached brigade
        /// </summary>
        public UnitType Extra2 { get; set; }

        /// <summary>
        ///     Unit type of attached brigade
        /// </summary>
        public UnitType Extra3 { get; set; }

        /// <summary>
        ///     Unit type of attached brigade
        /// </summary>
        public UnitType Extra4 { get; set; }

        /// <summary>
        ///     Unit type of attached brigade
        /// </summary>
        public UnitType Extra5 { get; set; }

        /// <summary>
        ///     Model number of the attached brigade
        /// </summary>
        public int BrigadeModel1 { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     Model number of the attached brigade
        /// </summary>
        public int BrigadeModel2 { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     Model number of the attached brigade
        /// </summary>
        public int BrigadeModel3 { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     Model number of the attached brigade
        /// </summary>
        public int BrigadeModel4 { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     Model number of the attached brigade
        /// </summary>
        public int BrigadeModel5 { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     Maximum strength
        /// </summary>
        public double MaxStrength { get; set; }

        /// <summary>
        ///     Strength
        /// </summary>
        public double Strength { get; set; }

        /// <summary>
        ///     Maximum organization rate
        /// </summary>
        public double MaxOrganisation { get; set; }

        /// <summary>
        ///     Organization rate
        /// </summary>
        public double Organisation { get; set; }

        /// <summary>
        ///     morale
        /// </summary>
        public double Morale { get; set; }

        /// <summary>
        ///     Experience point
        /// </summary>
        public double Experience { get; set; }

        /// <summary>
        ///     Improvement progress rate
        /// </summary>
        public double UpgradeProgress { get; set; }

        /// <summary>
        ///     Relocation destination Providence
        /// </summary>
        public int RedeployTarget { get; set; }

        /// <summary>
        ///     Relocation destination unit name
        /// </summary>
        public string RedeployUnitName { get; set; }

        /// <summary>
        ///     Relocation destination unit ID
        /// </summary>
        public TypeId RedeployUnitId { get; set; }

        /// <summary>
        ///     Offensive start date and time
        /// </summary>
        public GameDate Offensive { get; set; }

        /// <summary>
        ///     Supplies
        /// </summary>
        public double Supplies { get; set; }

        /// <summary>
        ///     fuel
        /// </summary>
        public double Fuel { get; set; }

        /// <summary>
        ///     Largest supplies
        /// </summary>
        public double MaxSupplies { get; set; }

        /// <summary>
        ///     Maximum fuel
        /// </summary>
        public double MaxFuel { get; set; }

        /// <summary>
        ///     Material consumption
        /// </summary>
        public double SupplyConsumption { get; set; }

        /// <summary>
        ///     Fuel consumption
        /// </summary>
        public double FuelConsumption { get; set; }

        /// <summary>
        ///     Maximum speed
        /// </summary>
        public double MaxSpeed { get; set; }

        /// <summary>
        ///     Artillery speed cap
        /// </summary>
        public double SpeedCapArt { get; set; }

        /// <summary>
        ///     Engineer speed cap
        /// </summary>
        public double SpeedCapEng { get; set; }

        /// <summary>
        ///     Anti-aircraft speed cap
        /// </summary>
        public double SpeedCapAa { get; set; }

        /// <summary>
        ///     Anti-tank speed cap
        /// </summary>
        public double SpeedCapAt { get; set; }

        /// <summary>
        ///     Transport load
        /// </summary>
        public double TransportWeight { get; set; }

        /// <summary>
        ///     Transport capacity
        /// </summary>
        public double TransportCapability { get; set; }

        /// <summary>
        ///     Defense power
        /// </summary>
        public double Defensiveness { get; set; }

        /// <summary>
        ///     Endurance
        /// </summary>
        public double Toughness { get; set; }

        /// <summary>
        ///     Vulnerability
        /// </summary>
        public double Softness { get; set; }

        /// <summary>
        ///     Control
        /// </summary>
        public double Suppression { get; set; }

        /// <summary>
        ///     Anti-ship / / Anti-submarine defense
        /// </summary>
        public double SeaDefense { get; set; }

        /// <summary>
        ///     Ground defense
        /// </summary>
        public double SurfaceDefence { get; set; }

        /// <summary>
        ///     Anti-aircraft defense
        /// </summary>
        public double AirDefence { get; set; }

        /// <summary>
        ///     Interpersonal attack power
        /// </summary>
        public double SoftAttack { get; set; }

        /// <summary>
        ///     Anti-instep attack power
        /// </summary>
        public double HardAttack { get; set; }

        /// <summary>
        ///     Anti-ship attack power (( Navy )
        /// </summary>
        public double SeaAttack { get; set; }

        /// <summary>
        ///     Anti-submarine attack power
        /// </summary>
        public double SubAttack { get; set; }

        /// <summary>
        ///     Commerce raiding power
        /// </summary>
        public double ConvoyAttack { get; set; }

        /// <summary>
        ///     Gulf attack power
        /// </summary>
        public double ShoreBombardment { get; set; }

        /// <summary>
        ///     Anti-aircraft attack power
        /// </summary>
        public double AirAttack { get; set; }

        /// <summary>
        ///     Strategic bombing attack power
        /// </summary>
        public double StrategicAttack { get; set; }

        /// <summary>
        ///     Anti-ship attack power
        /// </summary>
        public double NavalAttack { get; set; }

        /// <summary>
        ///     Shooting ability
        /// </summary>
        public double ArtilleryBombardment { get; set; }

        /// <summary>
        ///     Anti-ship search ability
        /// </summary>
        public double SurfaceDetection { get; set; }

        /// <summary>
        ///     Anti-aircraft search ability
        /// </summary>
        public double AirDetection { get; set; }

        /// <summary>
        ///     Anti-submarine enemy ability
        /// </summary>
        public double SubDetection { get; set; }

        /// <summary>
        ///     Visibility
        /// </summary>
        public double Visibility { get; set; }

        /// <summary>
        ///     Cruising distance
        /// </summary>
        public double Range { get; set; }

        /// <summary>
        ///     Range distance
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        ///     Moving distance
        /// </summary>
        public double Travelled { get; set; }

        /// <summary>
        ///     Cannot move
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        ///     Hibernate
        /// </summary>
        public bool Dormant { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     Undefined model number
        /// </summary>
        public const int UndefinedModelNo = -1;

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            Type,
            Id,
            Name,
            UnitType,
            Model,
            Nuke,
            BrigadeType1,
            BrigadeType2,
            BrigadeType3,
            BrigadeType4,
            BrigadeType5,
            BirgadeModel1,
            BirgadeModel2,
            BirgadeModel3,
            BirgadeModel4,
            BirgadeModel5,
            MaxStrength,
            Strength,
            MaxOrganisation,
            Organisation,
            Morale,
            Experience,
            UpgradeProgress,
            RedeployTarget,
            RedeployUnitName,
            RedeployUnitType,
            RedeployUnitId,
            Supplies,
            Fuel,
            MaxSupplies,
            MaxFuel,
            SupplyConsumption,
            FuelConsumption,
            MaxSpeed,
            SpeedCapArt,
            SpeedCapEng,
            SpeedCapAa,
            SpeedCapAt,
            TransportWeight,
            TransportCapability,
            Defensiveness,
            Toughness,
            Softness,
            Suppression,
            SeaDefence,
            SurfaceDefence,
            AirDefence,
            SoftAttack,
            HardAttack,
            SeaAttack,
            SubAttack,
            ConvoyAttack,
            ShoreBombardment,
            AirAttack,
            StrategicAttack,
            NavalAttack,
            ArtilleryBombardment,
            SurfaceDetection,
            AirDetection,
            SubDetection,
            Visibility,
            Range,
            Distance,
            Travelled,
            Locked,
            Dormant
        }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public Division()
        {
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">Original division</param>
        public Division(Division original)
        {
            Id = Scenarios.GetNewTypeId(original.Id.Type, original.Id.Id);
            Name = original.Name;
            Branch = original.Branch;
            Type = original.Type;
            Model = original.Model;
            Nuke = original.Nuke;
            Extra1 = original.Extra1;
            Extra2 = original.Extra2;
            Extra3 = original.Extra3;
            Extra4 = original.Extra4;
            Extra5 = original.Extra5;
            BrigadeModel1 = original.BrigadeModel1;
            BrigadeModel2 = original.BrigadeModel2;
            BrigadeModel3 = original.BrigadeModel3;
            BrigadeModel4 = original.BrigadeModel4;
            BrigadeModel5 = original.BrigadeModel5;
            MaxStrength = original.MaxStrength;
            Strength = original.Strength;
            MaxOrganisation = original.MaxOrganisation;
            Organisation = original.Organisation;
            Morale = original.Morale;
            Experience = original.Experience;
            UpgradeProgress = original.UpgradeProgress;
            RedeployTarget = original.RedeployTarget;
            RedeployUnitName = original.RedeployUnitName;
            if (original.RedeployUnitId != null)
            {
                RedeployUnitId = Scenarios.GetNewTypeId(original.RedeployUnitId.Type, original.RedeployUnitId.Id);
            }
            if (original.Offensive != null)
            {
                Offensive = new GameDate(original.Offensive);
            }
            Supplies = original.Supplies;
            Fuel = original.Fuel;
            MaxSupplies = original.MaxSupplies;
            MaxFuel = original.MaxFuel;
            SupplyConsumption = original.SupplyConsumption;
            FuelConsumption = original.FuelConsumption;
            MaxSpeed = original.MaxSpeed;
            SpeedCapArt = original.SpeedCapArt;
            SpeedCapEng = original.SpeedCapEng;
            SpeedCapAa = original.SpeedCapAa;
            SpeedCapAt = original.SpeedCapAt;
            TransportWeight = original.TransportWeight;
            TransportCapability = original.TransportCapability;
            Defensiveness = original.Defensiveness;
            Toughness = original.Toughness;
            Softness = original.Softness;
            Suppression = original.Suppression;
            SeaDefense = original.SeaDefense;
            SurfaceDefence = original.SurfaceDefence;
            AirDefence = original.AirDefence;
            SoftAttack = original.SoftAttack;
            HardAttack = original.HardAttack;
            SeaAttack = original.SeaAttack;
            SubAttack = original.SubAttack;
            ConvoyAttack = original.ConvoyAttack;
            ShoreBombardment = original.ShoreBombardment;
            AirAttack = original.AirAttack;
            StrategicAttack = original.StrategicAttack;
            NavalAttack = original.NavalAttack;
            ArtilleryBombardment = original.ArtilleryBombardment;
            SurfaceDetection = original.SurfaceDetection;
            AirDetection = original.AirDetection;
            SubDetection = original.SubDetection;
            Visibility = original.Visibility;
            Range = original.Range;
            Distance = original.Distance;
            Travelled = original.Travelled;
            Locked = original.Locked;
            Dormant = original.Dormant;
        }

        #endregion

        #region type When id Group operation

        /// <summary>
        ///     type When id Delete the pair of
        /// </summary>
        public void RemoveTypeId()
        {
            Scenarios.RemoveTypeId(Id);
            Scenarios.RemoveTypeId(RedeployUnitId);
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the provision settings have been edited
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
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = true;
            }
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     During production division
    /// </summary>
    public class DivisionDevelopment
    {
        #region Public properties

        /// <summary>
        ///     type When id id Pair of
        /// </summary>
        public TypeId Id { get; set; }

        /// <summary>
        ///     Division name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     requirement I C
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        ///     Necessary human resources
        /// </summary>
        public double Manpower { get; set; }

        /// <summary>
        ///     unitcost ( details unknown )
        /// </summary>
        public bool UnitCost { get; set; } = true;

        /// <summary>
        ///     new_model ( details unknown )
        /// </summary>
        public bool NewModel { get; set; } = true;

        /// <summary>
        ///     Completion date
        /// </summary>
        public GameDate Date { get; set; }

        /// <summary>
        ///     Progress rate increment
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        ///     Total progress rate
        /// </summary>
        public double TotalProgress { get; set; }

        /// <summary>
        ///     Continuous production bonus
        /// </summary>
        public double GearingBonus { get; set; }

        /// <summary>
        ///     Total production number
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        ///     Number of completed production
        /// </summary>
        public int Done { get; set; }

        /// <summary>
        ///     Days to complete
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        ///     1 Number of days to complete the unit
        /// </summary>
        public int DaysForFirst { get; set; }

        /// <summary>
        ///     Stopping
        /// </summary>
        public bool Halted { get; set; }

        /// <summary>
        ///     Whether to delete the queue on completion
        /// </summary>
        public bool CloseWhenFinished { get; set; }

        /// <summary>
        ///     waitingforclosure (waitforclosure ( details unknown )
        /// </summary>
        public bool WaitingForClosure { get; set; }

        /// <summary>
        ///     Production line preparation time
        /// </summary>
        public double RetoolingTime { get; set; }

        /// <summary>
        ///     Unit type
        /// </summary>
        public UnitType Type { get; set; }

        /// <summary>
        ///     Model number
        /// </summary>
        public int Model { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     Unit type of attached brigade
        /// </summary>
        public UnitType Extra1 { get; set; }

        /// <summary>
        ///     Unit type of attached brigade
        /// </summary>
        public UnitType Extra2 { get; set; }

        /// <summary>
        ///     Unit type of attached brigade
        /// </summary>
        public UnitType Extra3 { get; set; }

        /// <summary>
        ///     Unit type of attached brigade
        /// </summary>
        public UnitType Extra4 { get; set; }

        /// <summary>
        ///     Unit type of attached brigade
        /// </summary>
        public UnitType Extra5 { get; set; }

        /// <summary>
        ///     Model number of the attached brigade
        /// </summary>
        public int BrigadeModel1 { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     Model number of the attached brigade
        /// </summary>
        public int BrigadeModel2 { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     Model number of the attached brigade
        /// </summary>
        public int BrigadeModel3 { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     Model number of the attached brigade
        /// </summary>
        public int BrigadeModel4 { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     Model number of the attached brigade
        /// </summary>
        public int BrigadeModel5 { get; set; } = UndefinedModelNo;

        /// <summary>
        ///     position
        /// </summary>
        public int Location { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     Undefined model number
        /// </summary>
        public const int UndefinedModelNo = -1;

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            Type,
            Id,
            Name,
            Cost,
            Manpower,
            UnitCost,
            NewModel,
            Year,
            Month,
            Day,
            Progress,
            TotalProgress,
            GearingBonus,
            Size,
            Done,
            Days,
            DaysForFirst,
            Halted,
            CloseWhenFinished,
            WaitingForClosure,
            UnitType,
            Model,
            BrigadeType1,
            BrigadeType2,
            BrigadeType3,
            BrigadeType4,
            BrigadeType5,
            BrigadeModel1,
            BrigadeModel2,
            BrigadeModel3,
            BrigadeModel4,
            BrigadeModel5
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the provision settings have been edited
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
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = true;
            }
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    #endregion

    #region mission

    /// <summary>
    ///     mission
    /// </summary>
    public class Mission
    {
        #region Public properties

        /// <summary>
        ///     Type of mission
        /// </summary>
        public MissionType Type { get; set; }

        /// <summary>
        ///     Target Providence
        /// </summary>
        public int Target { get; set; }

        /// <summary>
        ///     Coverage (AoD only )
        /// </summary>
        public int MissionScope { get; set; }

        /// <summary>
        ///     Strength / / Lower limit of organization rate
        /// </summary>
        public double Percentage { get; set; }

        /// <summary>
        ///     Perform at night
        /// </summary>
        public bool Night { get; set; }

        /// <summary>
        ///     Daytime execution
        /// </summary>
        public bool Day { get; set; }

        /// <summary>
        ///     Coverage (DH only )
        /// </summary>
        public int TargetZone { get; set; }

        /// <summary>
        ///     Convoy attack (DH only )
        /// </summary>
        public bool AttackConvoy { get; set; }

        /// <summary>
        ///     Lower limit of organization rate (DH only )
        /// </summary>
        public double OrgLimit { get; set; }

        /// <summary>
        ///     Start date and time
        /// </summary>
        public GameDate StartDate { get; set; }

        /// <summary>
        ///     End date and time
        /// </summary>
        public GameDate EndDate { get; set; }

        /// <summary>
        ///     mission
        /// </summary>
        public int Task { get; set; }

        /// <summary>
        ///     position
        /// </summary>
        public int Location { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (ItemId)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     item ID
        /// </summary>
        public enum ItemId
        {
            Type,
            Target,
            MissionScope,
            Percentage,
            Night,
            Day,
            TargetZone,
            AttackConvoy,
            OrgLimit,
            StartYear,
            StartMonth,
            StartDay,
            StartHour,
            EndYear,
            EndMonth,
            EndDay,
            EndHour,
            Task,
            Location
        }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public Mission()
        {
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">Duplication source mission</param>
        public Mission(Mission original)
        {
            Type = original.Type;
            Target = original.Target;
            MissionScope = original.MissionScope;
            Percentage = original.Percentage;
            Night = original.Night;
            Day = original.Day;
            TargetZone = original.TargetZone;
            AttackConvoy = original.AttackConvoy;
            OrgLimit = original.OrgLimit;
            if (original.StartDate != null)
            {
                StartDate = new GameDate(original.StartDate);
            }
            if (original.EndDate != null)
            {
                EndDate = new GameDate(original.EndDate);
            }
            Task = original.Task;
            Location = original.Location;
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the item has been edited
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirty(ItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(ItemId id)
        {
            _dirtyFlags[(int) id] = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = true;
            }
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (ItemId id in Enum.GetValues(typeof (ItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
        }

        #endregion
    }

    /// <summary>
    ///     Type of mission
    /// </summary>
    public enum MissionType
    {
        None,
        Attack, // attack
        Rebase, // Base move
        StratRedeploy, // Strategic redeployment
        SupportAttack, // Support attack
        SupportDefense, // Defense support
        Reserves, // stand-by
        AntiPartisanDuty, // Partisan eradication
        ArtilleryBombardment, // Bombardment mission
        PlannedDefense, // Defense plan
        AirSuperiority, // Air superiority
        GroundAttack, // Ground attack
        RunwayCratering, // Airport airstrikes
        InstallationStrike, // Attack on military facilities
        Interdiction, // Ground support
        NavalStrike, // Ship attack
        PortStrike, // Port attack
        LogisticalStrike, // Soldier attack
        StrategicBombardment, // Strategic bombing
        AirSupply, // Air supply
        AirborneAssault, // Airborne assault
        AirScramble, // Air emergency sortie
        ConvoyRaiding, // Fleet raid
        Asw, // Anti-submarine strategy
        NavalInterdiction, // Maritime block
        ShoreBombardment, // Coastal bombardment
        AmphibiousAssault, // Assault landing
        SeaTransport, // Marine transport
        NavalCombatPatrol, // Maritime combat patrol
        SneakMove, // Covert movement
        NavalScramble, // Maritime emergency sortie
        ConvoyAirRaiding, // Convoy bombing
        NavalPortStrike, // Port attack by aircraft carrier
        NavalAirbaseStrike, // Airbase attack by aircraft carrier
        Nuke, // Nuclear attack
        Retreat // retreat
    }

    #endregion

    #region Transport fleet

    /// <summary>
    ///     Transport fleet
    /// </summary>
    public class Convoy
    {
        #region Public properties

        /// <summary>
        ///     type When id id Pair of
        /// </summary>
        public TypeId Id { get; set; }

        /// <summary>
        ///     Trade ID
        /// </summary>
        public TypeId TradeId { get; set; }

        /// <summary>
        ///     Whether it is a convoy for trade
        /// </summary>
        public bool IsTrade { get; set; }

        /// <summary>
        ///     Number of transport ships
        /// </summary>
        public int Transports { get; set; }

        /// <summary>
        ///     Number of escort vessels
        /// </summary>
        public int Escorts { get; set; }

        /// <summary>
        ///     Whether or not energy is transported
        /// </summary>
        public bool Energy { get; set; }

        /// <summary>
        ///     Whether metal is transported
        /// </summary>
        public bool Metal { get; set; }

        /// <summary>
        ///     Whether or not rare resources are transported
        /// </summary>
        public bool RareMaterials { get; set; }

        /// <summary>
        ///     With or without oil transportation
        /// </summary>
        public bool Oil { get; set; }

        /// <summary>
        ///     Whether or not goods are transported
        /// </summary>
        public bool Supplies { get; set; }

        /// <summary>
        ///     sea route
        /// </summary>
        public List<int> Path { get; set; } = new List<int>();

        #endregion
    }

    /// <summary>
    ///     Convoy in production
    /// </summary>
    public class ConvoyDevelopment
    {
        #region Public properties

        /// <summary>
        ///     type When id id Pair of
        /// </summary>
        public TypeId Id { get; set; }

        /// <summary>
        ///     name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Types of transport fleet
        /// </summary>
        public ConvoyType Type { get; set; }

        /// <summary>
        ///     position
        /// </summary>
        public int Location { get; set; }

        /// <summary>
        ///     requirement I C
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        ///     Necessary human resources
        /// </summary>
        public double Manpower { get; set; }

        /// <summary>
        ///     Completion date
        /// </summary>
        public GameDate Date { get; set; }

        /// <summary>
        ///     Progress rate increment
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        ///     Total progress rate
        /// </summary>
        public double TotalProgress { get; set; }

        /// <summary>
        ///     Continuous production bonus
        /// </summary>
        public double GearingBonus { get; set; }

        /// <summary>
        ///     Continuous production
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        ///     Number of completed production
        /// </summary>
        public int Done { get; set; }

        /// <summary>
        ///     Days to complete
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        ///     the first 1 Number of days to complete the unit
        /// </summary>
        public int DaysForFirst { get; set; }

        /// <summary>
        ///     Stopping
        /// </summary>
        public bool Halted { get; set; }

        /// <summary>
        ///     Whether to delete the queue on completion
        /// </summary>
        public bool CloseWhenFinished { get; set; }

        /// <summary>
        ///     details unknown
        /// </summary>
        public bool WaitingForClosure { get; set; }

        /// <summary>
        ///     Production line preparation time
        /// </summary>
        public double RetoolingTime { get; set; }

        #endregion
    }

    /// <summary>
    ///     Types of transport fleet
    /// </summary>
    public enum ConvoyType
    {
        None,
        Transports, // Transport ship
        Escorts // Escort ship
    }

    #endregion

    #region General purpose

    /// <summary>
    ///     type When id id Pair of
    /// </summary>
    public class TypeId
    {
        #region Public properties

        /// <summary>
        ///     id id
        /// </summary>
        public int Id;

        /// <summary>
        ///     type
        /// </summary>
        public int Type;

        #endregion
    }

    #endregion
}
