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
            "battle_cruiser",
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
            "team",
            "areaowned",
            "areacontrol",
            "ic",
            "capital_province",
            "big_alliance",
            "national_idea",
            "land_combat",
            "tech_ream",
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
            "alliance_leader"
        };

        /// <summary>
        ///     Trigger parameter type table
        /// </summary>
        public static readonly TriggerParamType[] ParamTypeTable =
        {
            TriggerParamType.None,
            TriggerParamType.Container, // and
            TriggerParamType.Container, // or
            TriggerParamType.Container, // not
            TriggerParamType.Int, // year year
            TriggerParamType.Int, // month
            TriggerParamType.Int, // day
            TriggerParamType.Int, // event event
            TriggerParamType.Random, // random
            TriggerParamType.CountryYesNo, // ai
            TriggerParamType.String, // flag flag
            TriggerParamType.String, // local_flag
            TriggerParamType.IntelDiff, // intel_diff
            TriggerParamType.Int, // dissent
            TriggerParamType.Int, // leader
            TriggerParamType.Int, // incabinet
            TriggerParamType.DomesticInt, // domestic
            TriggerParamType.Government, // government
            TriggerParamType.Ideology, // ideology
            TriggerParamType.CountryYesNo, // atwar
            TriggerParamType.Int, // minister
            TriggerParamType.YesNo, // major
            TriggerParamType.IsPuppet, // ispuppet
            TriggerParamType.CountryPair, // puppet
            TriggerParamType.Int, // headofgovernment
            TriggerParamType.Int, // headofstate
            TriggerParamType.Technology, // technology
            TriggerParamType.Int, // is_tech_active
            TriggerParamType.DomesticInt, // can_change_policy
            TriggerParamType.ProvinceInt, // province_revoltrisk
            TriggerParamType.Int, // nuke
            TriggerParamType.Int, // energy
            TriggerParamType.Int, // oil
            TriggerParamType.Int, // rare_materials
            TriggerParamType.Int, // metal
            TriggerParamType.Int, // supplies supplies
            TriggerParamType.Int, // manpower
            TriggerParamType.ProvinceCountry2, // owned
            TriggerParamType.ProvinceCountry2, // control
            TriggerParamType.DivisionExists, // division_exists
            TriggerParamType.DivisionInProvince, // division_in_province
            TriggerParamType.Size, // armor
            TriggerParamType.Size, // light_armor
            TriggerParamType.Size, // bergsjaeger
            TriggerParamType.Size, // cavalry
            TriggerParamType.Garrison, // garrison
            TriggerParamType.Size, // hq
            TriggerParamType.Size, // infantry
            TriggerParamType.Size, // marine
            TriggerParamType.Size, // mechanized
            TriggerParamType.Size, // militia
            TriggerParamType.Size, // motorized
            TriggerParamType.Size, // paratrooper
            TriggerParamType.Size, // cas
            TriggerParamType.Size, // escort
            TriggerParamType.Size, // flying_bomb
            TriggerParamType.Size, // flying_rocket
            TriggerParamType.Size, // interceptor
            TriggerParamType.Size, // multi_role
            TriggerParamType.Size, // naval_bomber
            TriggerParamType.Size, // strategic_bomber
            TriggerParamType.Size, // tactical_bomber
            TriggerParamType.Size, // transport_plane
            TriggerParamType.Size, // battle_cruiser
            TriggerParamType.Size, // battleship
            TriggerParamType.Size, // carrier
            TriggerParamType.Size, // escort_carrier
            TriggerParamType.Size, // destroyer
            TriggerParamType.Size, // heavy_cruiser
            TriggerParamType.Size, // light_cruiser
            TriggerParamType.Size, // submarine
            TriggerParamType.Size, // nuclear_submarine
            TriggerParamType.Size, // transport
            TriggerParamType.Int, // army
            TriggerParamType.Country, // exists exists
            TriggerParamType.CountryPair, // alliance
            TriggerParamType.CountryPair, // access
            TriggerParamType.CountryPair, // non_aggression
            TriggerParamType.CountryPair, // trade
            TriggerParamType.CountryPair, // guarantee guarantee
            TriggerParamType.CountryPair, // war
            TriggerParamType.CountryInt, // lost_vp
            TriggerParamType.CountryInt, // lost_national
            TriggerParamType.CountryInt, // lost_ic
            TriggerParamType.Int, // axis axis
            TriggerParamType.Int, // allies
            TriggerParamType.Int, // comintern
            TriggerParamType.Int, // vp
            TriggerParamType.Range, // range
            TriggerParamType.Belligerence, // belligerence
            TriggerParamType.UnderAttack, // under_attack
            TriggerParamType.Country, // attack
            TriggerParamType.Int, // difficulty difficulty
            TriggerParamType.CountryDouble, // land_percentage
            TriggerParamType.CountryDouble, // naval_percentage
            TriggerParamType.CountryDouble, // air_percentage
            TriggerParamType.Country, // country country
            TriggerParamType.Relation, // relation
            TriggerParamType.Int, // team
            TriggerParamType.ProvinceCountry2, // areaowned
            TriggerParamType.ProvinceCountry2, // areacontrol
            TriggerParamType.Size, // I C
            TriggerParamType.CapitalProvince, // capital_province
            TriggerParamType.CountryAlliance, // big_alliance
            TriggerParamType.CountryIdea, // national_idea
            TriggerParamType.Int, // land_combat
            TriggerParamType.TechTeam, // tech_ream
            TriggerParamType.Int, // money
            TriggerParamType.CountryPair, // military_control
            TriggerParamType.Losses, // losses losses
            TriggerParamType.ProvinceBuilding, // province_building
            TriggerParamType.CountryInt, // participant
            TriggerParamType.Embargo, // embargo
            TriggerParamType.ProvinceCountry, // claims
            TriggerParamType.Int, // escortpool
            TriggerParamType.Int, // convoypool
            TriggerParamType.Resource, // stockpile
            TriggerParamType.Resource, // import
            TriggerParamType.Resource, // export export
            TriggerParamType.ResourceAll, // resource_shortage
            TriggerParamType.ProvinceCountry, // capital
            TriggerParamType.String, // continent
            TriggerParamType.ProvinceCountry, // core
            TriggerParamType.Policy, // policy
            TriggerParamType.Building, // building
            TriggerParamType.Size, // nuclear_reactor
            TriggerParamType.Size, // rocket_test
            TriggerParamType.Nuked, // nuked
            TriggerParamType.Intelligence, // intelligence
            TriggerParamType.Area, // area
            TriggerParamType.Region, // region
            TriggerParamType.ResearchMod, // research_mod
            TriggerParamType.Int // alliance_leader
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public Trigger()
        {
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">Copy source trigger</param>
        public Trigger(Trigger original)
        {
            Type = original.Type;
            if (ParamTypeTable[(int) Type] == TriggerParamType.Container)
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
            if (ParamTypeTable[(int) Type] != TriggerParamType.Container)
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
        AllianceLeader
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
