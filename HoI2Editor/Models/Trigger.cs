using System.Collections.Generic;
using System.Text;
using HoI2Editor.Utilities;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     trigger
    /// </summary>
    public class Trigger
    {
        #region Public properties

        /// <summary>
        ///     Trigger type
        /// </summary>
        public TriggerType Type { get; set; }

        /// <summary>
        ///     Trigger value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        ///     Trigger text line
        /// </summary>
        public int LineNum { get; set; }

        #endregion

        #region Public constant

        /// <summary>
        ///     Trigger string table
        /// </summary>
        public static readonly string[] TypeStringTable =
        {
            "",
            "and",
            "or",
            "not",
            "year",
            "month",
            "day",
            "event",
            "random",
            "ai",
            "flag",
            "local_flag",
            "intel_diff",
            "dissent",
            "leader",
            "incabinet",
            "domestic",
            "government",
            "ideology",
            "atwar",
            "minister",
            "major",
            "ispuppet",
            "puppet",
            "headofgovernment",
            "headofstate",
            "technology",
            "is_tech_active",
            "can_change_policy",
            "province_revoltrisk",
            "nuke",
            "energy",
            "oil",
            "rare_materials",
            "metal",
            "supplies",
            "manpower",
            "owned",
            "control",
            "division_exists",
            "division_in_province",
            "armor",
            "light_armor",
            "bergsjaeger",
            "cavalry",
            "garrison",
            "hq",
            "infantry",
            "marine",
            "mechanized",
            "militia",
            "motorized",
            "paratrooper",
            "cas",
            "escort",
            "flying_bomb",
            "flying_rocket",
            "interceptor",
            "multi_role",
            "naval_bomber",
            "strategic_bomber",
            "tactical_bomber",
            "transport_plane",
            "battlecruiser",
            "battleship",
            "carrier",
            "escort_carrier",
            "destroyer",
            "heavy_cruiser",
            "light_cruiser",
            "submarine",
            "nuclear_submarine",
            "transport",
            "army",
            "exists",
            "alliance",
            "access",
            "non_aggression",
            "trade",
            "guarantee",
            "war",
            "lost_vp",
            "lost_national",
            "lost_ic",
            "axis",
            "allies",
            "comintern",
            "vp",
            "range",
            "belligerence",
            "under_attack",
            "attack",
            "difficulty",
            "land_percentage",
            "naval_percentage",
            "air_percentage",
            "country",
            "relation",
            "province",
            "data",
            "value",
            "type",
            "id",
            "min",
            "max",
            "size",
            "which",
            "team",
            "areaowned",
            "areacontrol",
            "ic",
            "capital_province",
            "big_alliance",
            "national_idea",
            "land_combat",
            "tech_team",
            "money",
            "military_control",
            "losses",
            "province_building",
            "participant",
            "embargo",
            "claims",
            "escortpool",
            "convoypool",
            "stockpile",
            "import",
            "export",
            "resource_shortage",
            "capital",
            "continent",
            "core",
            "policy",
            "building",
            "nuclear_reactor",
            "rocket_test",
            "nuked",
            "intelligence",
            "area",
            "region",
            "research_mod",
            "alliance_leader",
            "days",
            "when",
            "where"
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public Trigger()
        {
            LineNum = -1;
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">Copy source trigger</param>
        public Trigger(Trigger original)
        {
            Type = original.Type;
            if (original.Value.GetType() == typeof(List<Trigger>))
            {
                Value = new List<Trigger>();
                foreach (Trigger trigger in (List<Trigger>) original.Value)
                {
                    ((List<Trigger>) Value).Add(new Trigger(trigger));
                }
            }
            else
            {
                Value = original.Value;
                LineNum = original.LineNum;
            }
        }

        #endregion

        #region String operation

        /// <summary>
        ///     Convert to a string
        /// </summary>
        /// <returns>Character string</returns>
        public override string ToString()
        {
            // For single trigger
            if (Value.GetType() != typeof(List<Trigger>))
            {
                return $"{TypeStringTable[(int) Type]} = {ObjectHelper.ToString(Value)}";
            }

            // For container triggers
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} = {{", TypeStringTable[(int) Type]);
            List<Trigger> triggers = Value as List<Trigger>;
            if (triggers != null)
            {
                foreach (Trigger trigger in triggers)
                {
                    sb.AppendFormat(" {0}", trigger);
                }
            }
            sb.Append(" }");
            return sb.ToString();
        }

        #endregion
    }

    /// <summary>
    ///     Trigger type
    /// </summary>
    public enum TriggerType
    {
        None,
        And,
        Or,
        Not,
        Year,
        Month,
        Day,
        Event,
        Random,
        Ai,
        Flag,
        LocalFlag,
        IntelDiff,
        Dissent,
        Leader,
        InCabinet,
        Domestic,
        Government,
        Ideology,
        AtWar,
        Minister,
        Major,
        IsPuppet,
        Puppet,
        HeadOfGovernment,
        HeadOfState,
        Technology,
        IsTechActive,
        CanChangePolicy,
        ProvinceRevoltRisk,
        Nuke,
        Energy,
        Oil,
        RareMaterials,
        Metal,
        Supplies,
        ManPower,
        Owned,
        Control,
        DivisionExists,
        DivisionInProvince,
        Armor,
        LightArmor,
        Bergsjaeger,
        Cavalry,
        Garrison,
        Hq,
        Infantry,
        Marine,
        Mechanized,
        Militia,
        Motorized,
        Paratrooper,
        Cas,
        Escort,
        FlyingBomb,
        FlyingRocket,
        Interceptor,
        MultiRole,
        NavalBomber,
        StrategicBomber,
        TacticalBomber,
        TransportPlane,
        BattleCruiser,
        BattleShip,
        Carrier,
        EscortCarrier,
        Destroyer,
        HeavyCruiser,
        LightCruiser,
        Submarine,
        NuclearSubmarine,
        Transport,
        Army,
        Exists,
        Alliance,
        Access,
        NonAggression,
        Trade,
        Guarantee,
        War,
        LostVp,
        LostNational,
        LostIc,
        Axis,
        Allies,
        Comintern,
        Vp,
        Range,
        Belligerence,
        UnderAttack,
        Attack,
        Difficulty,
        LandPercentage,
        NavalPercentage,
        AirPercentage,
        Country,
        Relation,
        Province,
        // values
        Data,
        Value,
        Type,
        Id,
        Min, 
        Max, 
        Size,
        Which, 
        // AoD Add with
        Team,
        AreaOwned,
        AreaControl,
        Ic,
        CapitalProvince,
        BigAlliance,
        NationalIdea,
        LandCombat,
        TechTeam,
        Money,
        MilitaryControl,
        Losses,
        ProvinceBuilding,
        // DH Add with
        Participant,
        Embargo,
        Claims,
        EscortPool,
        ConvoyPool,
        Stockpile,
        Import,
        Export,
        ResourceShortage,
        Capital,
        Continent,
        Core,
        Policy,
        Building,
        NuclearReactor,
        RocketTest,
        Nuked,
        Intelligence,
        Area,
        Region,
        ResearchMod,
        AllianceLeader,
        Days,
        When,
        Where
    }

    /// <summary>
    ///     Types of trigger parameters
    /// </summary>
    public enum TriggerParamType
    {
        None,
        Container,
        Int,
        String,
        YesNo,
        Country,
        CountryPair,
        CountryInt,
        CountryDouble,
        CountryYesNo,
        CountryAlliance,
        CountryIdea,
        ProvinceInt,
        ProvinceCountry,
        ProvinceCountry2,
        Size,
        DomesticInt,
        Government,
        Ideology,
        Random,
        IntelDiff,
        IsPuppet,
        Technology,
        DivisionExists,
        DivisionInProvince,
        Garrison,
        Range,
        Belligerence,
        UnderAttack,
        Relation,
        CapitalProvince,
        TechTeam,
        Losses,
        ProvinceBuilding,
        Embargo,
        Resource,
        ResourceAll,
        Policy,
        Building,
        Nuked,
        Intelligence,
        Area,
        Region,
        ResearchMod
    }
}
