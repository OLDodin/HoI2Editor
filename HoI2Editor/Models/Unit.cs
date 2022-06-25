using System;
using System.Collections.Generic;
using System.Linq;
using HoI2Editor.Utilities;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Unit class
    /// </summary>
    public class UnitClass
    {
        #region Public properties

        /// <summary>
        ///     Unit type
        /// </summary>
        public UnitType Type { get; }

        /// <summary>
        ///     Unit military department
        /// </summary>
        public Branch Branch { get; set; }

        /// <summary>
        ///     Unit organization
        /// </summary>
        public UnitOrganization Organization { get; }

        /// <summary>
        ///     name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Abbreviated name
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        ///     explanation
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        ///     Brief explanation
        /// </summary>
        public string ShortDesc { get; set; }

        /// <summary>
        ///     Statistics group
        /// </summary>
        public int Eyr { get; set; }

        /// <summary>
        ///     Sprite type
        /// </summary>
        public SpriteType Sprite { get; set; }

        /// <summary>
        ///     Class to use when production is not possible
        /// </summary>
        public UnitType Transmute { get; set; }

        /// <summary>
        ///     Image priority
        /// </summary>
        public int GfxPrio { get; set; }

        /// <summary>
        ///     Military power
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        ///     List priority
        /// </summary>
        public int ListPrio { get; set; }

        /// <summary>
        ///     UI UI priority
        /// </summary>
        public int UiPrio { get; set; }

        /// <summary>
        ///     Actual unit type
        /// </summary>
        public RealUnitType RealType { get; set; }

        /// <summary>
        ///     Maximum production speed
        /// </summary>
        public int MaxSpeedStep { get; set; }

        /// <summary>
        ///     Whether it can be produced in the initial state
        /// </summary>
        public bool Productable { get; set; }

        /// <summary>
        ///     Whether it is a carrier air wing
        /// </summary>
        public bool Cag { get; set; }

        /// <summary>
        ///     Whether it is an escort fighter
        /// </summary>
        public bool Escort { get; set; }

        /// <summary>
        ///     Whether it is an engineer
        /// </summary>
        public bool Engineer { get; set; }

        /// <summary>
        ///     Whether it is a standard production type
        /// </summary>
        public bool DefaultType { get; set; }

        /// <summary>
        ///     Is the brigade removable?
        /// </summary>
        public bool Detachable { get; set; }

        /// <summary>
        ///     Maximum number of brigades (-1 Undefined )
        /// </summary>
        public int MaxAllowedBrigades { get; set; }

        /// <summary>
        ///     Attachable brigade
        /// </summary>
        public List<UnitType> AllowedBrigades { get; private set; }

        /// <summary>
        ///     Model list
        /// </summary>
        public List<UnitModel> Models { get; }

        /// <summary>
        ///     Unit update information
        /// </summary>
        public List<UnitUpgrade> Upgrades { get; private set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag of attachable brigade
        /// </summary>
        private readonly List<UnitType> _dirtyBrigades = new List<UnitType>();

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (UnitClassItemId)).Length];

        /// <summary>
        ///     Edited flag in unit definition file
        /// </summary>
        private bool _dirtyFileFlag;

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        /// <summary>
        ///     Entity existence flag
        /// </summary>
        /// <remarks>
        ///     d_rsv_33 ~ d_rsv_40 , b_rsv_36 ~ b_rsv_40 This value is false false Then list_prio Only saved
        ///     d_01 ~ d_99 , b_01 ~ b_99 This value is false false If not saved
        /// </ remarks>
        private bool _entityFlag;

        #endregion

        #region Internal constant

        /// <summary>
        ///     Initial setting value of military department
        /// </summary>
        private static readonly Branch[] DefaultBranches =
        {
            Branch.None,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Airforce,
            Branch.Airforce,
            Branch.Airforce,
            Branch.Airforce,
            Branch.Airforce,
            Branch.Airforce,
            Branch.Airforce,
            Branch.Airforce,
            Branch.Airforce,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Airforce,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Navy,
            Branch.Airforce,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Navy,
            Branch.Navy,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army,
            Branch.Army
        };

        /// <summary>
        ///     Initial setting value of organization
        /// </summary>
        private static readonly UnitOrganization[] DefaultOrganizations =
        {
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Division,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade,
            UnitOrganization.Brigade
        };

        /// <summary>
        ///     Initial setting value of unit name
        /// </summary>
        private static readonly string[] DefaultNames =
        {
            "",
            "INFANTRY",
            "CAVALRY",
            "MOTORIZED",
            "MECHANIZED",
            "LIGHT_ARMOR",
            "ARMOR",
            "PARATROOPER",
            "MARINE",
            "BERGSJAEGER",
            "GARRISON",
            "HQ",
            "MILITIA",
            "MULTI_ROLE",
            "INTERCEPTOR",
            "STRATEGIC_BOMBER",
            "TACTICAL_BOMBER",
            "NAVAL_BOMBER",
            "CAS",
            "TRANSPORT_PLANE",
            "FLYING_BOMB",
            "FLYING_ROCKET",
            "BATTLESHIP",
            "LIGHT_CRUISER",
            "HEAVY_CRUISER",
            "BATTLECRUISER",
            "DESTROYER",
            "CARRIER",
            "ESCORT_CARRIER",
            "SUBMARINE",
            "NUCLEAR_SUBMARINE",
            "TRANSPORT",
            "LIGHT_CARRIER",
            "ROCKET_INTERCEPTOR",
            "D_RSV_33",
            "D_RSV_34",
            "D_RSV_35",
            "D_RSV_36",
            "D_RSV_37",
            "D_RSV_38",
            "D_RSV_39",
            "D_RSV_40",
            "D_01",
            "D_02",
            "D_03",
            "D_04",
            "D_05",
            "D_06",
            "D_07",
            "D_08",
            "D_09",
            "D_10",
            "D_11",
            "D_12",
            "D_13",
            "D_14",
            "D_15",
            "D_16",
            "D_17",
            "D_18",
            "D_19",
            "D_20",
            "D_21",
            "D_22",
            "D_23",
            "D_24",
            "D_25",
            "D_26",
            "D_27",
            "D_28",
            "D_29",
            "D_30",
            "D_31",
            "D_32",
            "D_33",
            "D_34",
            "D_35",
            "D_36",
            "D_37",
            "D_38",
            "D_39",
            "D_40",
            "D_41",
            "D_42",
            "D_43",
            "D_44",
            "D_45",
            "D_46",
            "D_47",
            "D_48",
            "D_49",
            "D_50",
            "D_51",
            "D_52",
            "D_53",
            "D_54",
            "D_55",
            "D_56",
            "D_57",
            "D_58",
            "D_59",
            "D_60",
            "D_61",
            "D_62",
            "D_63",
            "D_64",
            "D_65",
            "D_66",
            "D_67",
            "D_68",
            "D_69",
            "D_70",
            "D_71",
            "D_72",
            "D_73",
            "D_74",
            "D_75",
            "D_76",
            "D_77",
            "D_78",
            "D_79",
            "D_80",
            "D_81",
            "D_82",
            "D_83",
            "D_84",
            "D_85",
            "D_86",
            "D_87",
            "D_88",
            "D_89",
            "D_90",
            "D_91",
            "D_92",
            "D_93",
            "D_94",
            "D_95",
            "D_96",
            "D_97",
            "D_98",
            "D_99",
            "NONE",
            "ARTILLERY",
            "SP_ARTILLERY",
            "ROCKET_ARTILLERY",
            "SP_ROCKET_ARTILLERY",
            "ANTITANK",
            "TANK_DESTROYER",
            "LIGHT_ARMOR",
            "HEAVY_ARMOR",
            "SUPER_HEAVY_ARMOR",
            "ARMORED_CAR",
            "ANTIAIR",
            "POLICE",
            "ENGINEER",
            "CAG",
            "ESCORT",
            "NAVAL_ASW",
            "NAVAL_ANTI_AIR_S",
            "NAVAL_RADAR_S",
            "NAVAL_FIRE_CONTROLL_S",
            "NAVAL_IMPROVED_HULL_S",
            "NAVAL_TORPEDOES_S",
            "NAVAL_ANTI_AIR_L",
            "NAVAL_RADAR_L",
            "NAVAL_FIRE_CONTROLL_L",
            "NAVAL_IMPROVED_HULL_L",
            "NAVAL_TORPEDOES_L",
            "NAVAL_MINES",
            "NAVAL_SA_L",
            "NAVAL_SPOTTER_L",
            "NAVAL_SPOTTER_S",
            "B_U1",
            "B_U2",
            "B_U3",
            "B_U4",
            "B_U5",
            "B_U6",
            "B_U7",
            "B_U8",
            "B_U9",
            "B_U10",
            "B_U11",
            "B_U12",
            "B_U13",
            "B_U14",
            "B_U15",
            "B_U16",
            "B_U17",
            "B_U18",
            "B_U19",
            "B_U20",
            "CAVALRY_BRIGADE",
            "SP_ANTI_AIR",
            "MEDIUM_ARMOR",
            "FLOATPLANE",
            "LCAG",
            "AMPH_LIGHT_ARMOR_BRIGADE",
            "GLI_LIGHT_ARMOR_BRIGADE",
            "GLI_LIGHT_ARTILLERY",
            "SH_ARTILLERY",
            "B_RSV_36",
            "B_RSV_37",
            "B_RSV_38",
            "B_RSV_39",
            "B_RSV_40",
            "B_01",
            "B_02",
            "B_03",
            "B_04",
            "B_05",
            "B_06",
            "B_07",
            "B_08",
            "B_09",
            "B_10",
            "B_11",
            "B_12",
            "B_13",
            "B_14",
            "B_15",
            "B_16",
            "B_17",
            "B_18",
            "B_19",
            "B_20",
            "B_21",
            "B_22",
            "B_23",
            "B_24",
            "B_25",
            "B_26",
            "B_27",
            "B_28",
            "B_29",
            "B_30",
            "B_31",
            "B_32",
            "B_33",
            "B_34",
            "B_35",
            "B_36",
            "B_37",
            "B_38",
            "B_39",
            "B_40",
            "B_41",
            "B_42",
            "B_43",
            "B_44",
            "B_45",
            "B_46",
            "B_47",
            "B_48",
            "B_49",
            "B_50",
            "B_51",
            "B_52",
            "B_53",
            "B_54",
            "B_55",
            "B_56",
            "B_57",
            "B_58",
            "B_59",
            "B_60",
            "B_61",
            "B_62",
            "B_63",
            "B_64",
            "B_65",
            "B_66",
            "B_67",
            "B_68",
            "B_69",
            "B_70",
            "B_71",
            "B_72",
            "B_73",
            "B_74",
            "B_75",
            "B_76",
            "B_77",
            "B_78",
            "B_79",
            "B_80",
            "B_81",
            "B_82",
            "B_83",
            "B_84",
            "B_85",
            "B_86",
            "B_87",
            "B_88",
            "B_89",
            "B_90",
            "B_91",
            "B_92",
            "B_93",
            "B_94",
            "B_95",
            "B_96",
            "B_97",
            "B_98",
            "B_99"
        };

        /// <summary>
        ///     Initial value of the maximum number of attached brigades
        /// </summary>
        private static readonly int[] DefaultMaxBrigades =
        {
            0,
            1,
            1,
            1,
            1,
            1,
            1,
            1,
            1,
            1,
            1,
            1,
            1,
            0,
            0,
            1,
            1,
            1,
            0,
            0,
            0,
            0,
            5,
            2,
            3,
            4,
            1,
            1,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public UnitClass(UnitType type)
        {
            Type = type;
            Branch = DefaultBranches[(int) type];
            Organization = DefaultOrganizations[(int) type];
            ListPrio = -1;
            MaxAllowedBrigades = -1;
            AllowedBrigades = new List<UnitType>();
            Models = new List<UnitModel>();
            Upgrades = new List<UnitUpgrade>();

            if (Game.Type == GameType.ArsenalOfDemocracy)
            {
                // Initial setting of maximum production speed
                if (Organization == UnitOrganization.Division)
                {
                    MaxSpeedStep = 2;
                }

                // Initial setting for detachable brigade
                if (Organization == UnitOrganization.Brigade)
                {
                    Detachable = true;
                }
            }

            string s = DefaultNames[(int) Type];
            Name = "NAME_" + s;
            ShortName = "SNAME_" + s;
            Desc = "LDESC_" + s;
            ShortDesc = "SDESC_" + s;
        }

        #endregion

        #region String operation

        /// <summary>
        ///     Get the unit class name
        /// </summary>
        /// <returns>Unit class name</returns>
        public override string ToString()
        {
            return Config.ExistsKey(Name) ? Config.GetText(Name) : Units.Strings[(int) Type];
        }

        /// <summary>
        ///     Get the unit abbreviation name
        /// </summary>
        /// <returns>Abbreviated name</returns>
        public string GetShortName()
        {
            return Config.ExistsKey(ShortName) ? Config.GetText(ShortName) : "";
        }

        /// <summary>
        ///     Get unit description
        /// </summary>
        /// <returns>Unit description</returns>
        public string GetDesc()
        {
            return Config.ExistsKey(Desc) ? Config.GetText(Desc) : "";
        }

        /// <summary>
        ///     Get unit abbreviation description
        /// </summary>
        /// <returns>Unit shortening explanation</returns>
        public string GetShortDesc()
        {
            return Config.ExistsKey(ShortDesc) ? Config.GetText(ShortDesc) : "";
        }

        #endregion

        #region Data access

        /// <summary>
        ///     Get if the maximum number of attached brigades is editable
        /// </summary>
        /// <returns>If editable true true return it</returns>
        public bool CanModifyMaxAllowedBrigades()
        {
            // Not editable for brigades
            if (Organization == UnitOrganization.Brigade)
            {
                return false;
            }

            // DH Then editable
            if (Game.Type == GameType.DarkestHour)
            {
                return true;
            }

            // AoD1.07 After that, only the ship unit can be edited
            if ((Game.Type == GameType.ArsenalOfDemocracy) && (Game.Version >= 107))
            {
                switch (Type)
                {
                    case UnitType.Transport:
                    case UnitType.Submarine:
                    case UnitType.NuclearSubmarine:
                    case UnitType.Destroyer:
                    case UnitType.LightCruiser:
                    case UnitType.HeavyCruiser:
                    case UnitType.BattleCruiser:
                    case UnitType.BattleShip:
                    case UnitType.EscortCarrier:
                    case UnitType.Carrier:
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Get the maximum number of attached brigades
        /// </summary>
        /// <returns>Maximum number of attached brigades</returns>
        public int GetMaxAllowedBrigades()
        {
            // If the value has already been set, the set value is returned.
            if ((Game.Type == GameType.DarkestHour) && (MaxAllowedBrigades >= 0))
            {
                return MaxAllowedBrigades;
            }

            if (Game.Type == GameType.ArsenalOfDemocracy)
            {
                // DDA When AoD Reset the location where the maximum number of attached brigades is different
                if (Type == UnitType.EscortCarrier || Type == UnitType.Cas)
                {
                    return 1;
                }

                // AoD1.07 In the following cases misc Defines the maximum number of brigades attached to a ship
                if (Game.Version >= 107)
                {
                    switch (Type)
                    {
                        case UnitType.Transport:
                            return Misc.TpMaxAttach;

                        case UnitType.Submarine:
                            return Misc.SsMaxAttach;

                        case UnitType.NuclearSubmarine:
                            return Misc.SsnMaxAttach;

                        case UnitType.Destroyer:
                            return Misc.DdMaxAttach;

                        case UnitType.LightCruiser:
                            return Misc.ClMaxAttach;

                        case UnitType.HeavyCruiser:
                            return Misc.CaMaxAttach;

                        case UnitType.BattleCruiser:
                            return Misc.BcMaxAttach;

                        case UnitType.BattleShip:
                            return Misc.BbMaxAttach;

                        case UnitType.LightCarrier:
                            return Misc.CvlMaxAttach;

                        case UnitType.Carrier:
                            return Misc.CvMaxAttach;
                    }
                }
            }

            // Returns the default setting
            return DefaultMaxBrigades[(int) Type];
        }

        /// <summary>
        ///     Set the maximum number of attached brigades
        /// </summary>
        /// <param name="brigades"></param>
        public void SetMaxAllowedBrigades(int brigades)
        {
            // Do nothing for the brigade
            if (Organization == UnitOrganization.Brigade)
            {
                return;
            }

            // HoI2 or AoD1.07 Do nothing before
            if ((Game.Type == GameType.HeartsOfIron2) ||
                ((Game.Type == GameType.ArsenalOfDemocracy) && (Game.Version < 107)))
            {
                return;
            }

            if (Game.Type == GameType.DarkestHour)
            {
                // DH If, update the unit settings
                MaxAllowedBrigades = brigades;
            }
            else
            {
                // AoD1.07In the following cases, only the ship unit will update the value.
                switch (Type)
                {
                    case UnitType.Transport:
                        Misc.TpMaxAttach = brigades;
                        break;

                    case UnitType.Submarine:
                        Misc.SsMaxAttach = brigades;
                        break;

                    case UnitType.NuclearSubmarine:
                        Misc.SsnMaxAttach = brigades;
                        break;

                    case UnitType.Destroyer:
                        Misc.DdMaxAttach = brigades;
                        break;

                    case UnitType.LightCruiser:
                        Misc.ClMaxAttach = brigades;
                        break;

                    case UnitType.HeavyCruiser:
                        Misc.CaMaxAttach = brigades;
                        break;

                    case UnitType.BattleCruiser:
                        Misc.BcMaxAttach = brigades;
                        break;

                    case UnitType.BattleShip:
                        Misc.BbMaxAttach = brigades;
                        break;

                    case UnitType.LightCarrier:
                        Misc.CvlMaxAttach = brigades;
                        break;

                    case UnitType.Carrier:
                        Misc.CvMaxAttach = brigades;
                        break;

                    default:
                        return;
                }
            }

            // Set the edited flag
            SetDirty(UnitClassItemId.MaxAllowedBrigades);
        }

        #endregion

        #region Unit model list

        /// <summary>
        ///     Insert the unit model
        /// </summary>
        /// <param name="model">Unit model to be inserted</param>
        /// <param name="index">Position to insert</param>
        /// <param name="name">Unit model name</param>
        public void InsertModel(UnitModel model, int index, string name)
        {
            Log.Info("[Unit] Insert model: {0} ({1})", index, this);

            // Change the unit model name after the insertion position
            SlideModelNamesDown(index, Models.Count - 1);

            // Delete the country unit model name of the insertion position
            foreach (Country country in Countries.Tags)
            {
                RemoveModelName(index, country);
            }

            // Change the unit model name of the insertion position
            SetModelName(index, name);

            // Insert an item in the unit model list
            Models.Insert(index, model);

            // Set the edited flag
            model.SetDirtyAll();
            SetDirtyFile();
        }

        /// <summary>
        ///     Delete the unit model
        /// </summary>
        /// <param name="index">Position to delete</param>
        public void RemoveModel(int index)
        {
            Log.Info("[Unit] Remove model: {0} ({1})", index, this);

            // Change the unit model name after the deleted position
            SlideModelNamesUp(index + 1, Models.Count - 1);

            // Delete the last unit model name
            RemoveModelName(Models.Count - 1);
            foreach (Country country in Countries.Tags)
            {
                RemoveModelName(Models.Count - 1, country);
            }

            // Remove an item from the unit model list
            Models.RemoveAt(index);

            // Set the edited flag
            SetDirtyFile();
        }

        /// <summary>
        ///     Move the unit model
        /// </summary>
        /// <param name="src">Source position</param>
        /// <param name="dest">Destination position</param>
        public void MoveModel(int src, int dest)
        {
            Log.Info("[Unit] Move model: {0} -> {1} ({2})", src, dest, this);

            UnitModel model = Models[src];

            // Move items in the unit model list
            if (src > dest)
            {
                // When moving up
                Models.Insert(dest, model);
                Models.RemoveAt(src + 1);
            }
            else
            {
                // When moving down
                Models.Insert(dest + 1, model);
                Models.RemoveAt(src);
            }

            // Save the unit model name of the move source
            string name = GetModelName(src);
            Dictionary<Country, string> names = Countries.Tags.Where(country => ExistsModelName(src, country))
                .ToDictionary(country => country, country => GetCountryModelName(src, country));

            // Change the unit model name between the source and destination
            if (src > dest)
            {
                // When moving up
                SlideModelNamesDown(dest, src - 1);
            }
            else
            {
                // When moving down
                SlideModelNamesUp(src + 1, dest);
            }

            // Change the destination unit model name
            SetModelName(dest, name);
            foreach (KeyValuePair<Country, string> pair in names)
            {
                SetModelName(dest, pair.Key, pair.Value);
            }

            // Set the edited flag
            SetDirtyFile();
        }

        #endregion

        #region Unit model name

        /// <summary>
        ///     Get a common unit model name
        /// </summary>
        /// <param name="index">Unit model index</param>
        /// <returns>Unit model name</returns>
        public string GetModelName(int index)
        {
            string key = GetModelNameKey(index);
            return Config.ExistsKey(key) ? Config.GetText(key) : "";
        }

        /// <summary>
        ///     Get the unit model name by country
        /// </summary>
        /// <param name="index">Unit model index</param>
        /// <param name="country">Country tag</param>
        /// <returns>Unit model name</returns>
        public string GetCountryModelName(int index, Country country)
        {
            string key = GetModelNameKey(index, country);
            return Config.ExistsKey(key) ? Config.GetText(key) : "";
        }

        /// <summary>
        ///     Set a common unit model name
        /// </summary>
        /// <param name="index">Unit model index</param>
        /// <param name="s">Unit model name</param>
        private void SetModelName(int index, string s)
        {
            Log.Info("[Unit] Set model name: {0} - {1} ({2})", index, s, this);

            Config.SetText(GetModelNameKey(index), s, Game.UnitTextFileName);
        }

        /// <summary>
        ///     Set the unit model name for each country
        /// </summary>
        /// <param name="index">Unit model index</param>
        /// <param name="country">Country tag</param>
        /// <param name="s">Unit model name</param>
        public void SetModelName(int index, Country country, string s)
        {
            if (country == Country.None)
            {
                SetModelName(index, s);
                return;
            }

            Log.Info("[Unit] Set country model name: {0} - {1} <{2}> ({3})", index, s, Countries.Strings[(int) country],
                this);

            Config.SetText(GetModelNameKey(index, country), s, Game.ModelTextFileName);
        }

        /// <summary>
        ///     Copy the common unit model name
        /// </summary>
        /// <param name="src">Index of copy source unit model</param>
        /// <param name="dest">Index of copy source unit model</param>
        private void CopyModelName(int src, int dest)
        {
            Log.Info("[Unit] Copy model name: {0} -> {1} ({2})", src, dest, this);

            SetModelName(dest, GetModelName(src));
        }

        /// <summary>
        ///     Copy the unit model name by country
        /// </summary>
        /// <param name="src">Index of copy source unit model</param>
        /// <param name="dest">Index of copy source unit model</param>
        /// <param name="country">Country tag</param>
        private void CopyModelName(int src, int dest, Country country)
        {
            if (country == Country.None)
            {
                SetModelName(dest, GetModelName(src));
                return;
            }

            Log.Info("[Unit] Copy country model name: {0} -> {1} <{2}> ({3})", src, dest,
                Countries.Strings[(int) country], this);

            SetModelName(dest, country, GetCountryModelName(src, country));
        }

        /// <summary>
        ///     Delete the common unit model name
        /// </summary>
        /// <param name="index">Unit model index</param>
        private void RemoveModelName(int index)
        {
            // Do nothing if there is no common unit model name
            string key = GetModelNameKey(index);
            if (!Config.ExistsKey(key))
            {
                return;
            }

            Log.Info("[Unit] Remove model name: {0} ({1})", index, this);

            Config.RemoveText(key, Game.UnitTextFileName);
        }

        /// <summary>
        ///     Delete the unit model name by country
        /// </summary>
        /// <param name="index">Unit model index</param>
        /// <param name="country">Country tag</param>
        public void RemoveModelName(int index, Country country)
        {
            if (country == Country.None)
            {
                RemoveModelName(index);
                return;
            }

            // Do nothing if there is no country-specific unit model name
            string key = GetModelNameKey(index, country);
            if (!Config.ExistsKey(key))
            {
                return;
            }

            Log.Info("[Unit] Remove country model name: {0} <{1}> ({2})", index, Countries.Strings[(int) country], this);

            Config.RemoveText(key, Game.ModelTextFileName);
        }

        /// <summary>
        ///     Unit model name 1 Move up
        /// </summary>
        /// <param name="start">Starting position</param>
        /// <param name="end">End position</param>
        private void SlideModelNamesUp(int start, int end)
        {
            // If the start position is behind the end position, replace it.
            if (start > end)
            {
                int tmp = start;
                start = end;
                end = tmp;
            }

            Log.Info("[Unit] Slide model names up: {0} - {1} ({2})", start, end, this);

            // Move the common model name up in order
            for (int i = start; i <= end; i++)
            {
                if (ExistsModelName(i))
                {
                    CopyModelName(i, i - 1);
                }
                else
                {
                    if (ExistsModelName(i - 1))
                    {
                        RemoveModelName(i - 1);
                    }
                }
            }

            // Move up the model names by country in order
            foreach (Country country in Countries.Tags)
            {
                for (int i = start; i <= end; i++)
                {
                    if (ExistsModelName(i, country))
                    {
                        CopyModelName(i, i - 1, country);
                    }
                    else
                    {
                        if (ExistsModelName(i - 1, country))
                        {
                            RemoveModelName(i - 1, country);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Unit model name 1 Move down
        /// </summary>
        /// <param name="start">Starting position</param>
        /// <param name="end">End position</param>
        private void SlideModelNamesDown(int start, int end)
        {
            // If the start position is behind the end position, replace it.
            if (start > end)
            {
                int tmp = start;
                start = end;
                end = tmp;
            }

            Log.Info("[Unit] Slide model names down: {0} - {1} ({2})", start, end, this);

            // Move the common model name down in order
            for (int i = end; i >= start; i--)
            {
                if (ExistsModelName(i))
                {
                    CopyModelName(i, i + 1);
                }
                else
                {
                    if (ExistsModelName(i + 1))
                    {
                        RemoveModelName(i + 1);
                    }
                }
            }

            // Move down the model names by country in order
            foreach (Country country in Countries.Tags)
            {
                for (int i = end; i >= start; i--)
                {
                    if (ExistsModelName(i, country))
                    {
                        CopyModelName(i, i + 1, country);
                    }
                    else
                    {
                        if (ExistsModelName(i + 1, country))
                        {
                            RemoveModelName(i + 1, country);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Get if there is a common model name
        /// </summary>
        /// <param name="index">Unit model index</param>
        /// <returns>If the model name exists true true return it</returns>
        private bool ExistsModelName(int index)
        {
            string key = GetModelNameKey(index);
            return !string.IsNullOrEmpty(key) && Config.ExistsKey(key);
        }

        /// <summary>
        ///     Get if a country-specific model name exists
        /// </summary>
        /// <param name="index">Unit model index</param>
        /// <param name="country">Country tag</param>
        /// <returns>If the model name exists true true return it</returns>
        public bool ExistsModelName(int index, Country country)
        {
            if (country == Country.None)
            {
                return ExistsModelName(index);
            }
            string key = GetModelNameKey(index, country);
            return !string.IsNullOrEmpty(key) && Config.ExistsKey(key);
        }

        /// <summary>
        ///     Get the key for a common unit model name
        /// </summary>
        /// <param name="index">Unit model index</param>
        /// <returns>Unit model name key</returns>
        private string GetModelNameKey(int index)
        {
            string format = Organization == UnitOrganization.Division ? "MODEL_{0}_{1}" : "BRIG_MODEL_{0}_{1}";
            return string.Format(format, Units.UnitNumbers[(int) Type], index);
        }

        /// <summary>
        ///     Get the key for the unit model name by country
        /// </summary>
        /// <param name="index">Unit model index</param>
        /// <param name="country">Country tag</param>
        /// <returns>Unit model name key</returns>
        private string GetModelNameKey(int index, Country country)
        {
            if (country == Country.None)
            {
                return GetModelNameKey(index);
            }
            string format = Organization == UnitOrganization.Division ? "MODEL_{0}_{1}_{2}" : "BRIG_MODEL_{0}_{1}_{2}";
            return string.Format(format, Countries.Strings[(int) country], Units.UnitNumbers[(int) Type], index);
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the unit class has been edited
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
        public bool IsDirty(UnitClassItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(UnitClassItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
            SetEntity();

            switch (id)
            {
                case UnitClassItemId.MaxSpeedStep: // Maximum production speed
                case UnitClassItemId.Detachable: // Is the brigade removable?
                    _dirtyFileFlag = true;
                    Units.SetDirty();
                    break;

                case UnitClassItemId.Name:
                case UnitClassItemId.ShortName:
                case UnitClassItemId.Desc:
                case UnitClassItemId.ShortDesc:
                    // It is not necessary to update the unit definition file because only the character string is updated.
                    break;

                case UnitClassItemId.Eyr: // Statistics group
                case UnitClassItemId.Sprite: // Sprite type
                case UnitClassItemId.Transmute: // Class to use when production is not possible
                case UnitClassItemId.GfxPrio: // Image priority
                case UnitClassItemId.Vaule: // Military power
                case UnitClassItemId.ListPrio: // List priority
                case UnitClassItemId.UiPrio: // UI priority
                case UnitClassItemId.RealType: // Actual unit type
                case UnitClassItemId.Productable: // Whether it can be produced in the initial state
                case UnitClassItemId.Cag: // Whether it is a carrier air wing
                case UnitClassItemId.Escort: // Whether it is an escort fighter
                case UnitClassItemId.Engineer: // Whether it is an engineer
                case UnitClassItemId.DefaultType: // Whether it is a standard production type
                    if (Organization == UnitOrganization.Division)
                    {
                        Units.SetDirtyDivisionTypes();
                    }
                    else
                    {
                        Units.SetDirtyBrigadeTypes();
                    }
                    break;

                case UnitClassItemId.Branch: // Unit military department
                    switch (Game.Type)
                    {
                        case GameType.ArsenalOfDemocracy:
                            // AoD In the case of the unit definition file, the division / / Editable with the brigade
                            _dirtyFileFlag = true;
                            Units.SetDirty();
                            break;

                        case GameType.DarkestHour:
                            // DH In the case of division / / Editable in the brigade definition file
                            if (Organization == UnitOrganization.Division)
                            {
                                Units.SetDirtyDivisionTypes();
                            }
                            else
                            {
                                Units.SetDirtyBrigadeTypes();
                            }
                            break;
                    }
                    break;

                case UnitClassItemId.MaxAllowedBrigades: // Maximum number of brigades
                    switch (Game.Type)
                    {
                        case GameType.ArsenalOfDemocracy:
                            // AoD In the case of ship unit only Misc Do nothing here because it is set on the side
                            break;

                        case GameType.DarkestHour:
                            // DH In the case of, it can be edited in the unit definition file
                            _dirtyFileFlag = true;
                            Units.SetDirty();
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
            SetEntity();
        }

        /// <summary>
        ///     Gets whether the unit definition file has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyFile()
        {
            return _dirtyFileFlag;
        }

        /// <summary>
        ///     Set the edited flag in the unit definition file
        /// </summary>
        /// <returns></returns>
        public void SetDirtyFile()
        {
            _dirtyFileFlag = true;
            _dirtyFlag = true;
            SetEntity();
            Units.SetDirty();
        }

        /// <summary>
        ///     Get if the attachable brigade has been edited
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsDirtyAllowedBrigades(UnitType type)
        {
            return _dirtyBrigades.Contains(type);
        }

        /// <summary>
        ///     Set the edited flag of the attachable brigade
        /// </summary>
        /// <param name="type">Brigade type</param>
        public void SetDirtyAllowedBrigades(UnitType type)
        {
            if (!_dirtyBrigades.Contains(type))
            {
                _dirtyBrigades.Add(type);
            }
            _dirtyFileFlag = true;
            _dirtyFlag = true;
            SetEntity();
            Units.SetDirty();
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (UnitClassItemId id in Enum.GetValues(typeof (UnitClassItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyBrigades.Clear();
            foreach (UnitModel model in Models)
            {
                model.ResetDirtyAll();
            }
            _dirtyFileFlag = false;
            _dirtyFlag = false;
        }

        /// <summary>
        ///     Get if the entity of the item exists
        /// </summary>
        /// <returns>If the entity exists true true return it</returns>
        public bool ExistsEntity()
        {
            return _entityFlag;
        }

        /// <summary>
        ///     Set the entity existence flag
        /// </summary>
        public void SetEntity()
        {
            // DH1.03 Do nothing except after
            if ((Game.Type != GameType.DarkestHour) || (Game.Version < 103))
            {
                return;
            }

            // Do nothing if set
            if (_entityFlag)
            {
                return;
            }

            _entityFlag = true;
        }

        #endregion
    }

    /// <summary>
    ///     Unit model
    /// </summary>
    public class UnitModel
    {
        #region Public properties

        /// <summary>
        ///     requirement I C
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        ///     Time required for production
        /// </summary>
        public double BuildTime { get; set; }

        /// <summary>
        ///     Necessary human resources
        /// </summary>
        public double ManPower { get; set; }

        /// <summary>
        ///     Moving Speed
        /// </summary>
        public double MaxSpeed { get; set; }

        /// <summary>
        ///     Speed cap with artillery brigade
        /// </summary>
        public double SpeedCapArt { get; set; }

        /// <summary>
        ///     Speed cap when accompanied by an engineer brigade
        /// </summary>
        public double SpeedCapEng { get; set; }

        /// <summary>
        ///     Speed cap when accompanied by anti-tank brigade
        /// </summary>
        public double SpeedCapAt { get; set; }

        /// <summary>
        ///     Speed cap when accompanied by anti-aircraft brigade
        /// </summary>
        public double SpeedCapAa { get; set; }

        /// <summary>
        ///     Cruising distance
        /// </summary>
        public double Range { get; set; }

        /// <summary>
        ///     Organization rate
        /// </summary>
        public double DefaultOrganization { get; set; }

        /// <summary>
        ///     morale
        /// </summary>
        public double Morale { get; set; }

        /// <summary>
        ///     Defense power
        /// </summary>
        public double Defensiveness { get; set; }

        /// <summary>
        ///     Anti-ship / / Anti-submarine defense
        /// </summary>
        public double SeaDefense { get; set; }

        /// <summary>
        ///     Anti-aircraft defense
        /// </summary>
        public double AirDefence { get; set; }

        /// <summary>
        ///     Ground / / Anti-ship defense
        /// </summary>
        public double SurfaceDefence { get; set; }

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
        ///     Anti-ship attack power (( Air Force )
        /// </summary>
        public double NavalAttack { get; set; }

        /// <summary>
        ///     Strategic bombing power
        /// </summary>
        public double StrategicAttack { get; set; }

        /// <summary>
        ///     Range distance
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        ///     Anti-ship search ability
        /// </summary>
        public double SurfaceDetectionCapability { get; set; }

        /// <summary>
        ///     Anti-submarine enemy ability
        /// </summary>
        public double SubDetectionCapability { get; set; }

        /// <summary>
        ///     Anti-aircraft search ability
        /// </summary>
        public double AirDetectionCapability { get; set; }

        /// <summary>
        ///     Visibility
        /// </summary>
        public double Visibility { get; set; }

        /// <summary>
        ///     Required TC
        /// </summary>
        public double TransportWeight { get; set; }

        /// <summary>
        ///     Transport capacity
        /// </summary>
        public double TransportCapability { get; set; }

        /// <summary>
        ///     Consumables
        /// </summary>
        public double SupplyConsumption { get; set; }

        /// <summary>
        ///     Fuel consumption
        /// </summary>
        public double FuelConsumption { get; set; }

        /// <summary>
        ///     Improved time correction
        /// </summary>
        public double UpgradeTimeFactor { get; set; }

        /// <summary>
        ///     Improvement I C correction
        /// </summary>
        public double UpgradeCostFactor { get; set; }

        /// <summary>
        ///     Artillery attack power (AoD)
        /// </summary>
        public double ArtilleryBombardment { get; set; }

        /// <summary>
        ///     Maximum carry-on supplies (AoD)
        /// </summary>
        public double MaxSupplyStock { get; set; }

        /// <summary>
        ///     Maximum portable fuel (AoD)
        /// </summary>
        public double MaxOilStock { get; set; }

        /// <summary>
        ///     Combat correction when running out of fuel (DH)
        /// </summary>
        public double NoFuelCombatMod { get; set; }

        /// <summary>
        ///     Replenishment time correction (DH)
        /// </summary>
        public double ReinforceTimeFactor { get; set; }

        /// <summary>
        ///     Replenishment I C correction (DH)
        /// </summary>
        public double ReinforceCostFactor { get; set; }

        /// <summary>
        ///     Whether to correct the improvement time (DH)
        /// </summary>
        public bool UpgradeTimeBoost { get; set; } = true;

        /// <summary>
        ///     Do you allow automatic improvements to other divisions?(DH)
        /// </summary>
        public bool AutoUpgrade { get; set; }

        /// <summary>
        ///     Unit class to be automatically improved (DH)
        /// </summary>
        public UnitType UpgradeClass { get; set; }

        /// <summary>
        ///     Automatic improvement destination model number (DH)
        /// </summary>
        public int UpgradeModel { get; set; }

        /// <summary>
        ///     Speed cap (DH1.03 from )
        /// </summary>
        public double SpeedCap { get; set; }

        /// <summary>
        ///     Equipment (DH1.03 from )
        /// </summary>
        public List<UnitEquipment> Equipments { get; } = new List<UnitEquipment>();

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (UnitModelItemId)).Length];

        /// <summary>
        ///     Edited flag for country model name
        /// </summary>
        private readonly bool[] _nameDirtyFlags = new bool[Enum.GetValues(typeof (Country)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public UnitModel()
        {
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">The unit model of the duplication source</param>
        public UnitModel(UnitModel original)
        {
            Cost = original.Cost;
            BuildTime = original.BuildTime;
            ManPower = original.ManPower;
            MaxSpeed = original.MaxSpeed;
            SpeedCapArt = original.SpeedCapArt;
            SpeedCapEng = original.SpeedCapEng;
            SpeedCapAt = original.SpeedCapAt;
            SpeedCapAa = original.SpeedCapAa;
            Range = original.Range;
            DefaultOrganization = original.DefaultOrganization;
            Morale = original.Morale;
            Defensiveness = original.Defensiveness;
            SeaDefense = original.SeaDefense;
            AirDefence = original.AirDefence;
            SurfaceDefence = original.SurfaceDefence;
            Toughness = original.Toughness;
            Softness = original.Softness;
            Suppression = original.Suppression;
            SoftAttack = original.SoftAttack;
            HardAttack = original.HardAttack;
            SeaAttack = original.SeaAttack;
            SubAttack = original.SubAttack;
            ConvoyAttack = original.ConvoyAttack;
            ShoreBombardment = original.ShoreBombardment;
            AirAttack = original.AirAttack;
            NavalAttack = original.NavalAttack;
            StrategicAttack = original.StrategicAttack;
            Distance = original.Distance;
            SurfaceDetectionCapability = original.SurfaceDetectionCapability;
            SubDetectionCapability = original.SubDetectionCapability;
            AirDetectionCapability = original.AirDetectionCapability;
            Visibility = original.Visibility;
            TransportWeight = original.TransportWeight;
            TransportCapability = original.TransportCapability;
            SupplyConsumption = original.SupplyConsumption;
            FuelConsumption = original.FuelConsumption;
            UpgradeTimeFactor = original.UpgradeTimeFactor;
            UpgradeCostFactor = original.UpgradeCostFactor;
            ArtilleryBombardment = original.ArtilleryBombardment;
            MaxSupplyStock = original.MaxSupplyStock;
            MaxOilStock = original.MaxOilStock;
            NoFuelCombatMod = original.NoFuelCombatMod;
            ReinforceTimeFactor = original.ReinforceTimeFactor;
            ReinforceCostFactor = original.ReinforceCostFactor;
            UpgradeTimeBoost = original.UpgradeTimeBoost;
            AutoUpgrade = original.AutoUpgrade;
            UpgradeClass = original.UpgradeClass;
            UpgradeModel = original.UpgradeModel;
            SpeedCap = original.SpeedCap;
            foreach (UnitEquipment equipment in original.Equipments)
            {
                Equipments.Add(new UnitEquipment(equipment));
            }
        }

        #endregion

        #region Equipment list

        /// <summary>
        ///     Move equipment items
        /// </summary>
        /// <param name="src">Source position</param>
        /// <param name="dest">Destination position</param>
        public void MoveEquipment(int src, int dest)
        {
            UnitEquipment equipment = Equipments[src];

            if (src > dest)
            {
                // When moving up
                Equipments.Insert(dest, equipment);
                Equipments.RemoveAt(src + 1);
            }
            else
            {
                // When moving down
                Equipments.Insert(dest + 1, equipment);
                Equipments.RemoveAt(src);
            }
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the unit model has been edited
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
        public bool IsDirty(UnitModelItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Get if the country model name has been edited
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirtyName(Country country)
        {
            return country == Country.None ? _dirtyFlags[(int) UnitModelItemId.Name] : _nameDirtyFlags[(int) country];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(UnitModelItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag for the country model name
        /// </summary>
        /// <param name="country">item ID</param>
        public void SetDirtyName(Country country)
        {
            if (country == Country.None)
            {
                _dirtyFlags[(int) UnitModelItemId.Name] = true;
            }
            else
            {
                _nameDirtyFlags[(int) country] = true;
            }
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (UnitModelItemId id in Enum.GetValues(typeof (UnitModelItemId)))
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
            foreach (UnitModelItemId id in Enum.GetValues(typeof (UnitModelItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            foreach (Country country in Countries.Tags)
            {
                _nameDirtyFlags[(int) country] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Unit equipment information
    /// </summary>
    public class UnitEquipment
    {
        #region Public properties

        /// <summary>
        ///     resource
        /// </summary>
        public EquipmentType Resource { get; set; }

        /// <summary>
        ///     amount
        /// </summary>
        public double Quantity { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (UnitEquipmentItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public UnitEquipment()
        {
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">Unit equipment information of the duplication source</param>
        public UnitEquipment(UnitEquipment original)
        {
            Resource = original.Resource;
            Quantity = original.Quantity;
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the unit equipment information has been edited
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
        public bool IsDirty(UnitEquipmentItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(UnitEquipmentItemId id)
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
            foreach (UnitEquipmentItemId id in Enum.GetValues(typeof (UnitEquipmentItemId)))
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
            foreach (UnitEquipmentItemId id in Enum.GetValues(typeof (UnitEquipmentItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Unit update information
    /// </summary>
    public class UnitUpgrade
    {
        #region Public properties

        /// <summary>
        ///     Unit type
        /// </summary>
        public UnitType Type { get; set; }

        /// <summary>
        ///     Improved time correction
        /// </summary>
        public double UpgradeTimeFactor { get; set; }

        /// <summary>
        ///     Improvement I C correction
        /// </summary>
        public double UpgradeCostFactor { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (UnitUpgradeItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the unit update information has been edited
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
        public bool IsDirty(UnitUpgradeItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(UnitUpgradeItemId id)
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
            foreach (UnitUpgradeItemId id in Enum.GetValues(typeof (UnitUpgradeItemId)))
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
            foreach (UnitUpgradeItemId id in Enum.GetValues(typeof (UnitUpgradeItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Unit organization
    /// </summary>
    public enum UnitOrganization
    {
        Division, // Division
        Brigade // brigade
    }

    /// <summary>
    ///     Unit type
    /// </summary>
    public enum UnitType
    {
        Undefined, // undefined

        // Division
        Infantry,
        Cavalry,
        Motorized,
        Mechanized,
        LightArmor,
        Armor,
        Paratrooper,
        Marine,
        Bergsjaeger,
        Garrison,
        Hq,
        Militia,
        MultiRole,
        Interceptor,
        StrategicBomber,
        TacticalBomber,
        NavalBomber,
        Cas,
        TransportPlane,
        FlyingBomb,
        FlyingRocket,
        BattleShip,
        LightCruiser,
        HeavyCruiser,
        BattleCruiser,
        Destroyer,
        Carrier,
        EscortCarrier,
        Submarine,
        NuclearSubmarine,
        Transport,
        // DH1.03 only
        LightCarrier,
        RocketInterceptor,
        ReserveDivision33,
        ReserveDivision34,
        ReserveDivision35,
        ReserveDivision36,
        ReserveDivision37,
        ReserveDivision38,
        ReserveDivision39,
        ReserveDivision40,
        Division01,
        Division02,
        Division03,
        Division04,
        Division05,
        Division06,
        Division07,
        Division08,
        Division09,
        Division10,
        Division11,
        Division12,
        Division13,
        Division14,
        Division15,
        Division16,
        Division17,
        Division18,
        Division19,
        Division20,
        Division21,
        Division22,
        Division23,
        Division24,
        Division25,
        Division26,
        Division27,
        Division28,
        Division29,
        Division30,
        Division31,
        Division32,
        Division33,
        Division34,
        Division35,
        Division36,
        Division37,
        Division38,
        Division39,
        Division40,
        Division41,
        Division42,
        Division43,
        Division44,
        Division45,
        Division46,
        Division47,
        Division48,
        Division49,
        Division50,
        Division51,
        Division52,
        Division53,
        Division54,
        Division55,
        Division56,
        Division57,
        Division58,
        Division59,
        Division60,
        Division61,
        Division62,
        Division63,
        Division64,
        Division65,
        Division66,
        Division67,
        Division68,
        Division69,
        Division70,
        Division71,
        Division72,
        Division73,
        Division74,
        Division75,
        Division76,
        Division77,
        Division78,
        Division79,
        Division80,
        Division81,
        Division82,
        Division83,
        Division84,
        Division85,
        Division86,
        Division87,
        Division88,
        Division89,
        Division90,
        Division91,
        Division92,
        Division93,
        Division94,
        Division95,
        Division96,
        Division97,
        Division98,
        Division99,

        // brigade
        None,
        Artillery,
        SpArtillery,
        RocketArtillery,
        SpRctArtillery,
        AntiTank,
        TankDestroyer,
        LightArmorBrigade,
        HeavyArmor,
        SuperHeavyArmor,
        ArmoredCar,
        AntiAir,
        Police,
        Engineer,
        Cag,
        Escort,
        NavalAsw,
        NavalAntiAirS,
        NavalRadarS,
        NavalFireControllS,
        NavalImprovedHullS,
        NavalTorpedoesS,
        NavalAntiAirL,
        NavalRadarL,
        NavalFireControllL,
        NavalImprovedHullL,
        NavalTorpedoesL,
        // AoD only
        NavalMines,
        NavalSaL,
        NavalSpotterL,
        NavalSpotterS,
        ExtraBrigade1,
        ExtraBrigade2,
        ExtraBrigade3,
        ExtraBrigade4,
        ExtraBrigade5,
        ExtraBrigade6,
        ExtraBrigade7,
        ExtraBrigade8,
        ExtraBrigade9,
        ExtraBrigade10,
        ExtraBrigade11,
        ExtraBrigade12,
        ExtraBrigade13,
        ExtraBrigade14,
        ExtraBrigade15,
        ExtraBrigade16,
        ExtraBrigade17,
        ExtraBrigade18,
        ExtraBrigade19,
        ExtraBrigade20,
        // DH1.03 only
        CavalryBrigade,
        SpAntiAir,
        MediumArmor,
        FloatPlane,
        LightCag,
        AmphArmor,
        GliderArmor,
        GliderArtillery,
        SuperHeavyArtillery,
        ReserveBrigade36,
        ReserveBrigade37,
        ReserveBrigade38,
        ReserveBrigade39,
        ReserveBrigade40,
        Brigade01,
        Brigade02,
        Brigade03,
        Brigade04,
        Brigade05,
        Brigade06,
        Brigade07,
        Brigade08,
        Brigade09,
        Brigade10,
        Brigade11,
        Brigade12,
        Brigade13,
        Brigade14,
        Brigade15,
        Brigade16,
        Brigade17,
        Brigade18,
        Brigade19,
        Brigade20,
        Brigade21,
        Brigade22,
        Brigade23,
        Brigade24,
        Brigade25,
        Brigade26,
        Brigade27,
        Brigade28,
        Brigade29,
        Brigade30,
        Brigade31,
        Brigade32,
        Brigade33,
        Brigade34,
        Brigade35,
        Brigade36,
        Brigade37,
        Brigade38,
        Brigade39,
        Brigade40,
        Brigade41,
        Brigade42,
        Brigade43,
        Brigade44,
        Brigade45,
        Brigade46,
        Brigade47,
        Brigade48,
        Brigade49,
        Brigade50,
        Brigade51,
        Brigade52,
        Brigade53,
        Brigade54,
        Brigade55,
        Brigade56,
        Brigade57,
        Brigade58,
        Brigade59,
        Brigade60,
        Brigade61,
        Brigade62,
        Brigade63,
        Brigade64,
        Brigade65,
        Brigade66,
        Brigade67,
        Brigade68,
        Brigade69,
        Brigade70,
        Brigade71,
        Brigade72,
        Brigade73,
        Brigade74,
        Brigade75,
        Brigade76,
        Brigade77,
        Brigade78,
        Brigade79,
        Brigade80,
        Brigade81,
        Brigade82,
        Brigade83,
        Brigade84,
        Brigade85,
        Brigade86,
        Brigade87,
        Brigade88,
        Brigade89,
        Brigade90,
        Brigade91,
        Brigade92,
        Brigade93,
        Brigade94,
        Brigade95,
        Brigade96,
        Brigade97,
        Brigade98,
        Brigade99
    }

    /// <summary>
    ///     Actual unit type (DH1.03 For later )
    /// </summary>
    /// <remarks>
    ///     AI Used to limit the production of
    ///     production AI: Militia / Infantry
    ///     Partisan : Militia / Infantry
    ///     alien : Infantry / Armor / StrategicBomber / Interceptor / Destroyer / Carrier
    /// </ remarks>
    public enum RealUnitType
    {
        Infantry,
        Cavalry,
        Motorized,
        Mechanized,
        LightArmor,
        Armor,
        Garrison,
        Hq,
        Paratrooper,
        Marine,
        Bergsjaeger,
        Cas,
        MultiRole,
        Interceptor,
        StrategicBomber,
        TacticalBomber,
        NavalBomber,
        TransportPlane,
        BattleShip,
        LightCruiser,
        HeavyCruiser,
        BattleCruiser,
        Destroyer,
        Carrier,
        Submarine,
        Transport,
        FlyingBomb,
        FlyingRocket,
        Militia,
        EscortCarrier,
        NuclearSubmarine
    }

    /// <summary>
    ///     Sprite type (DH1.03 For later )
    /// </summary>
    public enum SpriteType
    {
        Infantry,
        Cavalry,
        Motorized,
        Mechanized,
        LPanzer,
        Panzer,
        Paratrooper,
        Marine,
        Bergsjaeger,
        Fighter,
        Escort,
        Interceptor,
        Bomber,
        Tactical,
        Cas,
        Naval,
        TransportPlane,
        BattleShip,
        BattleCruiser,
        HeavyCruiser,
        LightCruiser,
        Destroyer,
        Carrier,
        Submarine,
        Transport,
        Militia,
        Garrison,
        Hq,
        FlyingBomb,
        Rocket,
        NuclearSubmarine,
        EscortCarrier,
        LightCarrier,
        RocketInterceptor,
        ReserveDivision33,
        ReserveDivision34,
        ReserveDivision35,
        ReserveDivision36,
        ReserveDivision37,
        ReserveDivision38,
        ReserveDivision39,
        ReserveDivision40,
        Division01,
        Division02,
        Division03,
        Division04,
        Division05,
        Division06,
        Division07,
        Division08,
        Division09,
        Division10,
        Division11,
        Division12,
        Division13,
        Division14,
        Division15,
        Division16,
        Division17,
        Division18,
        Division19,
        Division20,
        Division21,
        Division22,
        Division23,
        Division24,
        Division25,
        Division26,
        Division27,
        Division28,
        Division29,
        Division30,
        Division31,
        Division32,
        Division33,
        Division34,
        Division35,
        Division36,
        Division37,
        Division38,
        Division39,
        Division40,
        Division41,
        Division42,
        Division43,
        Division44,
        Division45,
        Division46,
        Division47,
        Division48,
        Division49,
        Division50,
        Division51,
        Division52,
        Division53,
        Division54,
        Division55,
        Division56,
        Division57,
        Division58,
        Division59,
        Division60,
        Division61,
        Division62,
        Division63,
        Division64,
        Division65,
        Division66,
        Division67,
        Division68,
        Division69,
        Division70,
        Division71,
        Division72,
        Division73,
        Division74,
        Division75,
        Division76,
        Division77,
        Division78,
        Division79,
        Division80,
        Division81,
        Division82,
        Division83,
        Division84,
        Division85,
        Division86,
        Division87,
        Division88,
        Division89,
        Division90,
        Division91,
        Division92,
        Division93,
        Division94,
        Division95,
        Division96,
        Division97,
        Division98,
        Division99
    }

    /// <summary>
    ///     Equipment type (DH1.03 For later )
    /// </summary>
    public enum EquipmentType
    {
        Manpower,
        Equipment,
        Artillery,
        HeavyArtillery,
        AntiAir,
        AntiTank,
        Horses,
        Trucks,
        Halftracks,
        ArmoredCar,
        LightArmor,
        MediumArmor,
        HeavyArmor,
        TankDestroyer,
        SpArtillery,
        Fighter,
        HeavyFighter,
        RocketInterceptor,
        Bomber,
        HeavyBomber,
        TransportPlane,
        Floatplane,
        Helicopter,
        Rocket,
        Balloon,
        Transports,
        Escorts,
        Transport,
        Battleship,
        BattleCruiser,
        HeavyCruiser,
        Carrier,
        EscortCarrier,
        LightCruiser,
        Destroyer,
        Submarine,
        NuclearSubmarine
    }

    /// <summary>
    ///     Unit class item ID
    /// </summary>
    public enum UnitClassItemId
    {
        Type, // Unit type
        Branch, // Unit military department
        Organization, // Unit organization
        Name, // name
        ShortName, // Abbreviated name
        Desc, // explanation
        ShortDesc, // Brief explanation
        Eyr, // Statistics group
        Sprite, // Sprite type
        Transmute, // Class to use when production is not possible
        GfxPrio, // Image priority
        Vaule, // Military power
        ListPrio, // List priority
        UiPrio, // UI priority
        RealType, // Actual unit type
        MaxSpeedStep, // Maximum production speed
        Productable, // Whether it can be produced in the initial state
        Cag, // Whether it is a carrier air wing
        Escort, // Whether it is an escort fighter
        Engineer, // Whether it is an engineer
        DefaultType, // Whether it is a standard production type
        Detachable, // Is the brigade removable?
        MaxAllowedBrigades // Maximum number of brigades
    }

    /// <summary>
    ///     Unit model item ID
    /// </summary>
    public enum UnitModelItemId
    {
        Name, // name
        Cost, // requirement I C
        BuildTime, // Time required for production
        ManPower, // Necessary human resources
        MaxSpeed, // Moving Speed
        SpeedCapArt, // Speed cap with artillery brigade
        SpeedCapEng, // Speed cap when accompanied by an engineer brigade
        SpeedCapAt, // Speed cap when accompanied by anti-tank brigade
        SpeedCapAa, // Speed cap when accompanied by anti-aircraft brigade
        Range, // Cruising distance
        DefaultOrganization, // Organization rate
        Morale, // morale
        Defensiveness, // Defense power
        SeaDefense, // Anti-ship / / Anti-submarine defense
        AirDefense, // Anti-aircraft defense
        SurfaceDefense, // Ground / / Anti-ship defense
        Toughness, // Endurance
        Softness, // Vulnerability
        Suppression, // Control
        SoftAttack, // Interpersonal attack power
        HardAttack, // Anti-instep attack power
        SeaAttack, // Anti-ship attack power (( Navy)
        SubAttack, // Anti-submarine attack power
        ConvoyAttack, // Trade destructive power
        ShoreBombardment, // Gulf attack power
        AirAttack, // Anti-aircraft attack power
        NavalAttack, // Anti-ship attack power (( Air Force )
        StrategicAttack, // Strategic bombing power
        Distance, // Range distance
        SurfaceDetectionCapability, // Anti-ship search ability
        SubDetectionCapability, // Anti-submarine enemy ability
        AirDetectionCapability, // Anti-aircraft search ability
        Visibility, // Visibility
        TransportWeight, // Required TC
        TransportCapability, // Transport capacity
        SupplyConsumption, // Consumables
        FuelConsumption, // Fuel consumption
        UpgradeTimeFactor, // Improved time correction
        UpgradeCostFactor, // Improvement I C correction
        ArtilleryBombardment, // Bombardment attack power (AoD)
        MaxSupplyStock, // Maximum carry-on supplies (AoD)
        MaxOilStock, // Maximum portable fuel (AoD)
        NoFuelCombatMod, // Combat correction when fuel runs out (DH)
        ReinforceTimeFactor, // Replenishment time correction (DH)
        ReinforceCostFactor, // Replenishment I C correction (DH)
        UpgradeTimeBoost, // Whether to correct the improvement time (DH)
        AutoUpgrade, // Do you allow automatic improvement to other divisions? (DH)
        UpgradeClass, // Unit class to be automatically improved (DH)
        UpgradeModel, // Automatic improvement destination model number (DH)
        SpeedCap // Speed cap (DH1.03 from )
    }

    /// <summary>
    ///     Unit equipment items ID
    /// </summary>
    public enum UnitEquipmentItemId
    {
        Resource, // resource
        Quantity // amount
    }

    /// <summary>
    ///     Unit update item ID
    /// </summary>
    public enum UnitUpgradeItemId
    {
        Type, // Unit type
        UpgradeTimeFactor, // Improved time correction
        UpgradeCostFactor // Improvement I C correction
    }
}
