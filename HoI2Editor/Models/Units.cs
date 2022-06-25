﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    ///     Unit data group
    /// </summary>
    public static class Units
    {
        #region Public properties

        /// <summary>
        ///     Unit list
        /// </summary>
        public static List<UnitClass> Items { get; }

        /// <summary>
        ///     Unit type string and ID Correspondence of
        /// </summary>
        public static Dictionary<string, UnitType> StringMap { get; }

        /// <summary>
        ///     With the actual unit type character string ID Correspondence of
        /// </summary>
        public static Dictionary<string, RealUnitType> RealStringMap { get; }

        /// <summary>
        ///     Sprite type with character string ID Correspondence of
        /// </summary>
        public static Dictionary<string, SpriteType> SpriteStringMap { get; }

        /// <summary>
        ///     With equipment string ID Correspondence of
        /// </summary>
        public static Dictionary<string, EquipmentType> EquipmentStringMap { get; }

        /// <summary>
        ///     Available unit types
        /// </summary>
        public static List<UnitType> UnitTypes { get; private set; }

        /// <summary>
        ///     Available division unit types
        /// </summary>
        public static UnitType[] DivisionTypes { get; private set; }

        /// <summary>
        ///     Available brigade unit types
        /// </summary>
        public static UnitType[] BrigadeTypes { get; private set; }

        #endregion

        #region Internal field

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

        /// <summary>
        ///     Edited flag in division unit class definition file
        /// </summary>
        private static bool _divisionTypesDirty;

        /// <summary>
        ///     Edited flag in brigade unit class definition file
        /// </summary>
        private static bool _brigadeTypesDirty;

        /// <summary>
        ///     Model name edited flag for each country
        /// </summary>
        private static readonly bool[] CountryNameDirtyFlags = new bool[Enum.GetValues(typeof (Country)).Length];

        /// <summary>
        ///     Unit name Model name for each type Edited flag
        /// </summary>
        private static readonly bool[,] TypeNameDirtyFlags =
            new bool[Enum.GetValues(typeof (Country)).Length, Enum.GetValues(typeof (UnitType)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     Unit type string table
        /// </summary>
        public static readonly string[] Strings =
        {
            "",
            "infantry",
            "cavalry",
            "motorized",
            "mechanized",
            "light_armor",
            "armor",
            "paratrooper",
            "marine",
            "bergsjaeger",
            "garrison",
            "hq",
            "militia",
            "multi_role",
            "interceptor",
            "strategic_bomber",
            "tactical_bomber",
            "naval_bomber",
            "cas",
            "transport_plane",
            "flying_bomb",
            "flying_rocket",
            "battleship",
            "light_cruiser",
            "heavy_cruiser",
            "battlecruiser",
            "destroyer",
            "carrier",
            "escort_carrier",
            "submarine",
            "nuclear_submarine",
            "transport",
            "light_carrier",
            "rocket_interceptor",
            "d_rsv_33",
            "d_rsv_34",
            "d_rsv_35",
            "d_rsv_36",
            "d_rsv_37",
            "d_rsv_38",
            "d_rsv_39",
            "d_rsv_40",
            "d_01",
            "d_02",
            "d_03",
            "d_04",
            "d_05",
            "d_06",
            "d_07",
            "d_08",
            "d_09",
            "d_10",
            "d_11",
            "d_12",
            "d_13",
            "d_14",
            "d_15",
            "d_16",
            "d_17",
            "d_18",
            "d_19",
            "d_20",
            "d_21",
            "d_22",
            "d_23",
            "d_24",
            "d_25",
            "d_26",
            "d_27",
            "d_28",
            "d_29",
            "d_30",
            "d_31",
            "d_32",
            "d_33",
            "d_34",
            "d_35",
            "d_36",
            "d_37",
            "d_38",
            "d_39",
            "d_40",
            "d_41",
            "d_42",
            "d_43",
            "d_44",
            "d_45",
            "d_46",
            "d_47",
            "d_48",
            "d_49",
            "d_50",
            "d_51",
            "d_52",
            "d_53",
            "d_54",
            "d_55",
            "d_56",
            "d_57",
            "d_58",
            "d_59",
            "d_60",
            "d_61",
            "d_62",
            "d_63",
            "d_64",
            "d_65",
            "d_66",
            "d_67",
            "d_68",
            "d_69",
            "d_70",
            "d_71",
            "d_72",
            "d_73",
            "d_74",
            "d_75",
            "d_76",
            "d_77",
            "d_78",
            "d_79",
            "d_80",
            "d_81",
            "d_82",
            "d_83",
            "d_84",
            "d_85",
            "d_86",
            "d_87",
            "d_88",
            "d_89",
            "d_90",
            "d_91",
            "d_92",
            "d_93",
            "d_94",
            "d_95",
            "d_96",
            "d_97",
            "d_98",
            "d_99",
            "none",
            "artillery",
            "sp_artillery",
            "rocket_artillery",
            "sp_rct_artillery",
            "anti_tank",
            "tank_destroyer",
            "light_armor_brigade",
            "heavy_armor",
            "super_heavy_armor",
            "armored_car",
            "anti_air",
            "police",
            "engineer",
            "cag",
            "escort",
            "naval_asw",
            "naval_anti_air_s",
            "naval_radar_s",
            "naval_fire_controll_s",
            "naval_improved_hull_s",
            "naval_torpedoes_s",
            "naval_anti_air_l",
            "naval_radar_l",
            "naval_fire_controll_l",
            "naval_improved_hull_l",
            "naval_torpedoes_l",
            "naval_mines",
            "naval_sa_l",
            "naval_spotter_l",
            "naval_spotter_s",
            "b_u1",
            "b_u2",
            "b_u3",
            "b_u4",
            "b_u5",
            "b_u6",
            "b_u7",
            "b_u8",
            "b_u9",
            "b_u10",
            "b_u11",
            "b_u12",
            "b_u13",
            "b_u14",
            "b_u15",
            "b_u16",
            "b_u17",
            "b_u18",
            "b_u19",
            "b_u20",
            "cavalry_brigade",
            "sp_anti_air",
            "medium_armor",
            "floatplane",
            "light_cag",
            "amph_armor",
            "glider_armor",
            "glider_artillery",
            "super_heavy_artillery",
            "b_rsv_36",
            "b_rsv_37",
            "b_rsv_38",
            "b_rsv_39",
            "b_rsv_40",
            "b_01",
            "b_02",
            "b_03",
            "b_04",
            "b_05",
            "b_06",
            "b_07",
            "b_08",
            "b_09",
            "b_10",
            "b_11",
            "b_12",
            "b_13",
            "b_14",
            "b_15",
            "b_16",
            "b_17",
            "b_18",
            "b_19",
            "b_20",
            "b_21",
            "b_22",
            "b_23",
            "b_24",
            "b_25",
            "b_26",
            "b_27",
            "b_28",
            "b_29",
            "b_30",
            "b_31",
            "b_32",
            "b_33",
            "b_34",
            "b_35",
            "b_36",
            "b_37",
            "b_38",
            "b_39",
            "b_40",
            "b_41",
            "b_42",
            "b_43",
            "b_44",
            "b_45",
            "b_46",
            "b_47",
            "b_48",
            "b_49",
            "b_50",
            "b_51",
            "b_52",
            "b_53",
            "b_54",
            "b_55",
            "b_56",
            "b_57",
            "b_58",
            "b_59",
            "b_60",
            "b_61",
            "b_62",
            "b_63",
            "b_64",
            "b_65",
            "b_66",
            "b_67",
            "b_68",
            "b_69",
            "b_70",
            "b_71",
            "b_72",
            "b_73",
            "b_74",
            "b_75",
            "b_76",
            "b_77",
            "b_78",
            "b_79",
            "b_80",
            "b_81",
            "b_82",
            "b_83",
            "b_84",
            "b_85",
            "b_86",
            "b_87",
            "b_88",
            "b_89",
            "b_90",
            "b_91",
            "b_92",
            "b_93",
            "b_94",
            "b_95",
            "b_96",
            "b_97",
            "b_98",
            "b_99"
        };

        /// <summary>
        ///     Real unit string
        /// </summary>
        public static readonly string[] RealStrings =
        {
            "infantry",
            "cavalry",
            "motorized",
            "mechanized",
            "light_armor",
            "armor",
            "garrison",
            "hq",
            "paratrooper",
            "marine",
            "bergsjaeger",
            "cas",
            "multi_role",
            "interceptor",
            "strategic_bomber",
            "tactical_bomber",
            "naval_bomber",
            "transport_plane",
            "battleship",
            "light_cruiser",
            "heavy_cruiser",
            "battlecruiser",
            "destroyer",
            "carrier",
            "submarine",
            "transport",
            "flying_bomb",
            "flying_rocket",
            "militia",
            "escort_carrier",
            "nuclear_submarine"
        };

        /// <summary>
        ///     Sprite type string
        /// </summary>
        public static readonly string[] SpriteStrings =
        {
            "infantry",
            "cavalry",
            "motorized",
            "mechanized",
            "l_panzer",
            "panzer",
            "paratrooper",
            "marine",
            "bergsjaeger",
            "fighter",
            "escort",
            "interceptor",
            "bomber",
            "tactical",
            "cas",
            "naval",
            "transportplane",
            "battleship",
            "battlecruiser",
            "heavy_cruiser",
            "light_cruiser",
            "destroyer",
            "carrier",
            "submarine",
            "transport",
            "militia",
            "garrison",
            "hq",
            "flying_bomb",
            "rocket",
            "nuclear_submarine",
            "escort_carrier",
            "light_carrier",
            "rocket_interceptor",
            "d_rsv_33",
            "d_rsv_34",
            "d_rsv_35",
            "d_rsv_36",
            "d_rsv_37",
            "d_rsv_38",
            "d_rsv_39",
            "d_rsv_40",
            "d_01",
            "d_02",
            "d_03",
            "d_04",
            "d_05",
            "d_06",
            "d_07",
            "d_08",
            "d_09",
            "d_10",
            "d_11",
            "d_12",
            "d_13",
            "d_14",
            "d_15",
            "d_16",
            "d_17",
            "d_18",
            "d_19",
            "d_20",
            "d_21",
            "d_22",
            "d_23",
            "d_24",
            "d_25",
            "d_26",
            "d_27",
            "d_28",
            "d_29",
            "d_30",
            "d_31",
            "d_32",
            "d_33",
            "d_34",
            "d_35",
            "d_36",
            "d_37",
            "d_38",
            "d_39",
            "d_40",
            "d_41",
            "d_42",
            "d_43",
            "d_44",
            "d_45",
            "d_46",
            "d_47",
            "d_48",
            "d_49",
            "d_50",
            "d_51",
            "d_52",
            "d_53",
            "d_54",
            "d_55",
            "d_56",
            "d_57",
            "d_58",
            "d_59",
            "d_60",
            "d_61",
            "d_62",
            "d_63",
            "d_64",
            "d_65",
            "d_66",
            "d_67",
            "d_68",
            "d_69",
            "d_70",
            "d_71",
            "d_72",
            "d_73",
            "d_74",
            "d_75",
            "d_76",
            "d_77",
            "d_78",
            "d_79",
            "d_80",
            "d_81",
            "d_82",
            "d_83",
            "d_84",
            "d_85",
            "d_86",
            "d_87",
            "d_88",
            "d_89",
            "d_90",
            "d_91",
            "d_92",
            "d_93",
            "d_94",
            "d_95",
            "d_96",
            "d_97",
            "d_98",
            "d_99"
        };

        /// <summary>
        ///     Equipment string
        /// </summary>
        public static readonly string[] EquipmentStrings =
        {
            "manpower",
            "equipment",
            "artillery",
            "heavy_artillery",
            "anti_air",
            "anti_tank",
            "horses",
            "trucks",
            "halftracks",
            "armored_car",
            "light_armor",
            "medium_armor",
            "heavy_armor",
            "tank_destroyer",
            "sp_artillery",
            "fighter",
            "heavy_fighter",
            "rocket_interceptor",
            "bomber",
            "heavy_bomber",
            "transport_plane",
            "floatplane",
            "helicopter",
            "rocket",
            "balloon",
            "transports",
            "escorts",
            "transport",
            "battleship",
            "battlecruiser",
            "heavy_cruiser",
            "carrier",
            "escort_carrier",
            "light_cruiser",
            "destroyer",
            "submarine",
            "nuclear_submarine"
        };

        /// <summary>
        ///     Correspondence between actual unit type and unit type
        /// </summary>
        public static readonly UnitType[] RealTypeTable =
        {
            UnitType.Infantry,
            UnitType.Cavalry,
            UnitType.Motorized,
            UnitType.Mechanized,
            UnitType.LightArmor,
            UnitType.Armor,
            UnitType.Garrison,
            UnitType.Hq,
            UnitType.Paratrooper,
            UnitType.Marine,
            UnitType.Bergsjaeger,
            UnitType.Cas,
            UnitType.MultiRole,
            UnitType.Interceptor,
            UnitType.StrategicBomber,
            UnitType.TacticalBomber,
            UnitType.NavalBomber,
            UnitType.TransportPlane,
            UnitType.BattleShip,
            UnitType.LightCruiser,
            UnitType.HeavyCruiser,
            UnitType.BattleCruiser,
            UnitType.Destroyer,
            UnitType.Carrier,
            UnitType.Submarine,
            UnitType.Transport,
            UnitType.FlyingBomb,
            UnitType.FlyingRocket,
            UnitType.Militia,
            UnitType.EscortCarrier,
            UnitType.NuclearSubmarine
        };

        /// <summary>
        ///     Correspondence between sprite type and unit type
        /// </summary>
        public static readonly UnitType[] SpriteTypeTable =
        {
            UnitType.Infantry,
            UnitType.Cavalry,
            UnitType.Motorized,
            UnitType.Mechanized,
            UnitType.LightArmor,
            UnitType.Armor,
            UnitType.Paratrooper,
            UnitType.Marine,
            UnitType.Bergsjaeger,
            UnitType.MultiRole,
            UnitType.Escort,
            UnitType.Interceptor,
            UnitType.StrategicBomber,
            UnitType.TacticalBomber,
            UnitType.Cas,
            UnitType.NavalBomber,
            UnitType.TransportPlane,
            UnitType.BattleShip,
            UnitType.BattleCruiser,
            UnitType.HeavyCruiser,
            UnitType.LightCruiser,
            UnitType.Destroyer,
            UnitType.Carrier,
            UnitType.Submarine,
            UnitType.Transport,
            UnitType.Militia,
            UnitType.Garrison,
            UnitType.Hq,
            UnitType.FlyingBomb,
            UnitType.FlyingRocket,
            UnitType.NuclearSubmarine,
            UnitType.EscortCarrier,
            UnitType.LightCarrier,
            UnitType.RocketInterceptor,
            UnitType.ReserveDivision33,
            UnitType.ReserveDivision34,
            UnitType.ReserveDivision35,
            UnitType.ReserveDivision36,
            UnitType.ReserveDivision37,
            UnitType.ReserveDivision38,
            UnitType.ReserveDivision39,
            UnitType.ReserveDivision40,
            UnitType.Division01,
            UnitType.Division02,
            UnitType.Division03,
            UnitType.Division04,
            UnitType.Division05,
            UnitType.Division06,
            UnitType.Division07,
            UnitType.Division08,
            UnitType.Division09,
            UnitType.Division10,
            UnitType.Division11,
            UnitType.Division12,
            UnitType.Division13,
            UnitType.Division14,
            UnitType.Division15,
            UnitType.Division16,
            UnitType.Division17,
            UnitType.Division18,
            UnitType.Division19,
            UnitType.Division20,
            UnitType.Division21,
            UnitType.Division22,
            UnitType.Division23,
            UnitType.Division24,
            UnitType.Division25,
            UnitType.Division26,
            UnitType.Division27,
            UnitType.Division28,
            UnitType.Division29,
            UnitType.Division30,
            UnitType.Division31,
            UnitType.Division32,
            UnitType.Division33,
            UnitType.Division34,
            UnitType.Division35,
            UnitType.Division36,
            UnitType.Division37,
            UnitType.Division38,
            UnitType.Division39,
            UnitType.Division40,
            UnitType.Division41,
            UnitType.Division42,
            UnitType.Division43,
            UnitType.Division44,
            UnitType.Division45,
            UnitType.Division46,
            UnitType.Division47,
            UnitType.Division48,
            UnitType.Division49,
            UnitType.Division50,
            UnitType.Division51,
            UnitType.Division52,
            UnitType.Division53,
            UnitType.Division54,
            UnitType.Division55,
            UnitType.Division56,
            UnitType.Division57,
            UnitType.Division58,
            UnitType.Division59,
            UnitType.Division60,
            UnitType.Division61,
            UnitType.Division62,
            UnitType.Division63,
            UnitType.Division64,
            UnitType.Division65,
            UnitType.Division66,
            UnitType.Division67,
            UnitType.Division68,
            UnitType.Division69,
            UnitType.Division70,
            UnitType.Division71,
            UnitType.Division72,
            UnitType.Division73,
            UnitType.Division74,
            UnitType.Division75,
            UnitType.Division76,
            UnitType.Division77,
            UnitType.Division78,
            UnitType.Division79,
            UnitType.Division80,
            UnitType.Division81,
            UnitType.Division82,
            UnitType.Division83,
            UnitType.Division84,
            UnitType.Division85,
            UnitType.Division86,
            UnitType.Division87,
            UnitType.Division88,
            UnitType.Division89,
            UnitType.Division90,
            UnitType.Division91,
            UnitType.Division92,
            UnitType.Division93,
            UnitType.Division94,
            UnitType.Division95,
            UnitType.Division96,
            UnitType.Division97,
            UnitType.Division98,
            UnitType.Division99
        };

        /// <summary>
        ///     Equipment name
        /// </summary>
        public static readonly string[] EquipmentNames =
        {
            "EQ_MANPOWER",
            "EQ_EQUIPMENT",
            "EQ_ARTILLERY",
            "EQ_HEAVY_ARTILLERY",
            "EQ_ANTI_AIR",
            "EQ_ANTI_TANK",
            "EQ_HORSES",
            "EQ_TRUCKS",
            "EQ_HALFTRACKS",
            "EQ_ARMORED_CARS",
            "EQ_LIGHT_ARMOR",
            "EQ_MEDIUM_ARMOR",
            "EQ_HEAVY_ARMOR",
            "EQ_TANK_DESTROYER",
            "EQ_SP_ARTILLERY",
            "EQ_FIGHTERS",
            "EQ_HEAVY_FIGHTERS",
            "EQ_ROCKET_INTERCEPTORS",
            "EQ_BOMBERS",
            "EQ_HEAVY_BOMBERS",
            "EQ_TRANSPORT_PLANES",
            "EQ_FLOATPLANE",
            "EQ_HELICOPTERS",
            "EQ_ROCKETS",
            "EQ_BALLOONS",
            "EQ_CONV_TRANS",
            "EQ_CONV_ESC",
            "EQ_TRANSPORT",
            "EQ_BATTLESHIP",
            "EQ_BATTLECRUISER",
            "EQ_HEAVY_CRUISER",
            "EQ_CARRIER",
            "EQ_ESCORT_CARRIER",
            "EQ_LIGHT_CRUISER",
            "EQ_DESTROYER",
            "EQ_SUBMARINE",
            "EQ_NUCLEAR_SUBMARINE"
        };

        /// <summary>
        ///     Initial setting value of unit number
        /// </summary>
        public static readonly int[] UnitNumbers =
        {
            0,
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
            19,
            20,
            21,
            22,
            23,
            24,
            25,
            26,
            27,
            28,
            29,
            30,
            31,
            32,
            33,
            34,
            35,
            36,
            37,
            38,
            39,
            40,
            41,
            42,
            43,
            44,
            45,
            46,
            47,
            48,
            49,
            50,
            51,
            52,
            53,
            54,
            55,
            56,
            57,
            58,
            59,
            60,
            61,
            62,
            63,
            64,
            65,
            66,
            67,
            68,
            69,
            70,
            71,
            72,
            73,
            74,
            75,
            76,
            77,
            78,
            79,
            80,
            81,
            82,
            83,
            84,
            85,
            86,
            87,
            88,
            89,
            90,
            91,
            92,
            93,
            94,
            95,
            96,
            97,
            98,
            99,
            100,
            101,
            102,
            103,
            104,
            105,
            106,
            107,
            108,
            109,
            110,
            111,
            112,
            113,
            114,
            115,
            116,
            117,
            118,
            119,
            120,
            121,
            122,
            123,
            124,
            125,
            126,
            127,
            128,
            129,
            130,
            131,
            132,
            133,
            134,
            135,
            136,
            137,
            138,
            139,
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
            19,
            20,
            21,
            22,
            23,
            24,
            25,
            26,
            27,
            28,
            29,
            30,
            31,
            32,
            33,
            34,
            35,
            36,
            37,
            38,
            39,
            40,
            41,
            42,
            43,
            44,
            45,
            46,
            47,
            48,
            49,
            50,
            27,
            28,
            29,
            30,
            31,
            32,
            33,
            34,
            35,
            36,
            37,
            38,
            39,
            40,
            41,
            42,
            43,
            44,
            45,
            46,
            47,
            48,
            49,
            50,
            51,
            52,
            53,
            54,
            55,
            56,
            57,
            58,
            59,
            60,
            61,
            62,
            63,
            64,
            65,
            66,
            67,
            68,
            69,
            70,
            71,
            72,
            73,
            74,
            75,
            76,
            77,
            78,
            79,
            80,
            81,
            82,
            83,
            84,
            85,
            86,
            87,
            88,
            89,
            90,
            91,
            92,
            93,
            94,
            95,
            96,
            97,
            98,
            99,
            100,
            101,
            102,
            103,
            104,
            105,
            106,
            107,
            108,
            109,
            110,
            111,
            112,
            113,
            114,
            115,
            116,
            117,
            118,
            119,
            120,
            121,
            122,
            123,
            124,
            125,
            126,
            127,
            128,
            129,
            130,
            131,
            132,
            133,
            134,
            135,
            136,
            137,
            138,
            139
        };

        /// <summary>
        ///     Army corresponding to the actual unit type
        /// </summary>
        public static readonly Branch[] RealBranchTable =
        {
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
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Navy,
            Branch.Airforce,
            Branch.Airforce,
            Branch.Army,
            Branch.Navy,
            Branch.Navy
        };

        #endregion

        #region Internal constant

        /// <summary>
        ///     Available division unit types (HoI2)
        /// </summary>
        private static readonly UnitType[] DivisionTypesHoI2 =
        {
            UnitType.Infantry,
            UnitType.Cavalry,
            UnitType.Motorized,
            UnitType.Mechanized,
            UnitType.LightArmor,
            UnitType.Armor,
            UnitType.Paratrooper,
            UnitType.Marine,
            UnitType.Bergsjaeger,
            UnitType.Garrison,
            UnitType.Hq,
            UnitType.Militia,
            UnitType.MultiRole,
            UnitType.Interceptor,
            UnitType.StrategicBomber,
            UnitType.TacticalBomber,
            UnitType.NavalBomber,
            UnitType.Cas,
            UnitType.TransportPlane,
            UnitType.FlyingBomb,
            UnitType.FlyingRocket,
            UnitType.BattleShip,
            UnitType.LightCruiser,
            UnitType.HeavyCruiser,
            UnitType.BattleCruiser,
            UnitType.Destroyer,
            UnitType.Carrier,
            UnitType.EscortCarrier,
            UnitType.Submarine,
            UnitType.NuclearSubmarine,
            UnitType.Transport
        };

        /// <summary>
        ///     Available division unit types (AoD)
        /// </summary>
        private static readonly UnitType[] DivisionTypesAoD =
        {
            UnitType.Infantry,
            UnitType.Cavalry,
            UnitType.Motorized,
            UnitType.Mechanized,
            UnitType.LightArmor,
            UnitType.Armor,
            UnitType.Paratrooper,
            UnitType.Marine,
            UnitType.Bergsjaeger,
            UnitType.Garrison,
            UnitType.Hq,
            UnitType.Militia,
            UnitType.MultiRole,
            UnitType.Interceptor,
            UnitType.StrategicBomber,
            UnitType.TacticalBomber,
            UnitType.NavalBomber,
            UnitType.Cas,
            UnitType.TransportPlane,
            UnitType.FlyingBomb,
            UnitType.FlyingRocket,
            UnitType.BattleShip,
            UnitType.LightCruiser,
            UnitType.HeavyCruiser,
            UnitType.BattleCruiser,
            UnitType.Destroyer,
            UnitType.Carrier,
            UnitType.EscortCarrier,
            UnitType.Submarine,
            UnitType.NuclearSubmarine,
            UnitType.Transport
        };

        /// <summary>
        ///     Available division unit types (DH1.03 from )
        /// </summary>
        private static readonly UnitType[] DivisionTypesDh =
        {
            UnitType.Infantry,
            UnitType.Cavalry,
            UnitType.Motorized,
            UnitType.Mechanized,
            UnitType.LightArmor,
            UnitType.Armor,
            UnitType.Paratrooper,
            UnitType.Marine,
            UnitType.Bergsjaeger,
            UnitType.Garrison,
            UnitType.Hq,
            UnitType.Militia,
            UnitType.MultiRole,
            UnitType.Interceptor,
            UnitType.StrategicBomber,
            UnitType.TacticalBomber,
            UnitType.NavalBomber,
            UnitType.Cas,
            UnitType.TransportPlane,
            UnitType.FlyingBomb,
            UnitType.FlyingRocket,
            UnitType.BattleShip,
            UnitType.LightCruiser,
            UnitType.HeavyCruiser,
            UnitType.BattleCruiser,
            UnitType.Destroyer,
            UnitType.Carrier,
            UnitType.EscortCarrier,
            UnitType.Submarine,
            UnitType.NuclearSubmarine,
            UnitType.Transport,
            UnitType.LightCarrier,
            UnitType.RocketInterceptor,
            UnitType.ReserveDivision33,
            UnitType.ReserveDivision34,
            UnitType.ReserveDivision35,
            UnitType.ReserveDivision36,
            UnitType.ReserveDivision37,
            UnitType.ReserveDivision38,
            UnitType.ReserveDivision39,
            UnitType.ReserveDivision40,
            UnitType.Division01,
            UnitType.Division02,
            UnitType.Division03,
            UnitType.Division04,
            UnitType.Division05,
            UnitType.Division06,
            UnitType.Division07,
            UnitType.Division08,
            UnitType.Division09,
            UnitType.Division10,
            UnitType.Division11,
            UnitType.Division12,
            UnitType.Division13,
            UnitType.Division14,
            UnitType.Division15,
            UnitType.Division16,
            UnitType.Division17,
            UnitType.Division18,
            UnitType.Division19,
            UnitType.Division20,
            UnitType.Division21,
            UnitType.Division22,
            UnitType.Division23,
            UnitType.Division24,
            UnitType.Division25,
            UnitType.Division26,
            UnitType.Division27,
            UnitType.Division28,
            UnitType.Division29,
            UnitType.Division30,
            UnitType.Division31,
            UnitType.Division32,
            UnitType.Division33,
            UnitType.Division34,
            UnitType.Division35,
            UnitType.Division36,
            UnitType.Division37,
            UnitType.Division38,
            UnitType.Division39,
            UnitType.Division40,
            UnitType.Division41,
            UnitType.Division42,
            UnitType.Division43,
            UnitType.Division44,
            UnitType.Division45,
            UnitType.Division46,
            UnitType.Division47,
            UnitType.Division48,
            UnitType.Division49,
            UnitType.Division50,
            UnitType.Division51,
            UnitType.Division52,
            UnitType.Division53,
            UnitType.Division54,
            UnitType.Division55,
            UnitType.Division56,
            UnitType.Division57,
            UnitType.Division58,
            UnitType.Division59,
            UnitType.Division60,
            UnitType.Division61,
            UnitType.Division62,
            UnitType.Division63,
            UnitType.Division64,
            UnitType.Division65,
            UnitType.Division66,
            UnitType.Division67,
            UnitType.Division68,
            UnitType.Division69,
            UnitType.Division70,
            UnitType.Division71,
            UnitType.Division72,
            UnitType.Division73,
            UnitType.Division74,
            UnitType.Division75,
            UnitType.Division76,
            UnitType.Division77,
            UnitType.Division78,
            UnitType.Division79,
            UnitType.Division80,
            UnitType.Division81,
            UnitType.Division82,
            UnitType.Division83,
            UnitType.Division84,
            UnitType.Division85,
            UnitType.Division86,
            UnitType.Division87,
            UnitType.Division88,
            UnitType.Division89,
            UnitType.Division90,
            UnitType.Division91,
            UnitType.Division92,
            UnitType.Division93,
            UnitType.Division94,
            UnitType.Division95,
            UnitType.Division96,
            UnitType.Division97,
            UnitType.Division98,
            UnitType.Division99
        };

        /// <summary>
        ///     Available brigade unit types (HoI2)
        /// </summary>
        private static readonly UnitType[] BrigadeTypesHoI2 =
        {
            UnitType.None,
            UnitType.Artillery,
            UnitType.SpArtillery,
            UnitType.RocketArtillery,
            UnitType.SpRctArtillery,
            UnitType.AntiTank,
            UnitType.TankDestroyer,
            UnitType.LightArmorBrigade,
            UnitType.HeavyArmor,
            UnitType.SuperHeavyArmor,
            UnitType.ArmoredCar,
            UnitType.AntiAir,
            UnitType.Police,
            UnitType.Engineer,
            UnitType.Cag,
            UnitType.Escort,
            UnitType.NavalAsw,
            UnitType.NavalAntiAirS,
            UnitType.NavalRadarS,
            UnitType.NavalFireControllS,
            UnitType.NavalImprovedHullS,
            UnitType.NavalTorpedoesS,
            UnitType.NavalAntiAirL,
            UnitType.NavalRadarL,
            UnitType.NavalFireControllL,
            UnitType.NavalImprovedHullL,
            UnitType.NavalTorpedoesL
        };

        /// <summary>
        ///     Available unit types (AoD)
        /// </summary>
        private static readonly UnitType[] BrigadeTypesAoD =
        {
            UnitType.None,
            UnitType.Artillery,
            UnitType.SpArtillery,
            UnitType.RocketArtillery,
            UnitType.SpRctArtillery,
            UnitType.AntiTank,
            UnitType.TankDestroyer,
            UnitType.LightArmorBrigade,
            UnitType.HeavyArmor,
            UnitType.SuperHeavyArmor,
            UnitType.ArmoredCar,
            UnitType.AntiAir,
            UnitType.Police,
            UnitType.Engineer,
            UnitType.Cag,
            UnitType.Escort,
            UnitType.NavalAsw,
            UnitType.NavalAntiAirS,
            UnitType.NavalRadarS,
            UnitType.NavalFireControllS,
            UnitType.NavalImprovedHullS,
            UnitType.NavalTorpedoesS,
            UnitType.NavalAntiAirL,
            UnitType.NavalRadarL,
            UnitType.NavalFireControllL,
            UnitType.NavalImprovedHullL,
            UnitType.NavalTorpedoesL,
            UnitType.NavalMines,
            UnitType.NavalSaL,
            UnitType.NavalSpotterL,
            UnitType.NavalSpotterS,
            UnitType.ExtraBrigade1,
            UnitType.ExtraBrigade2,
            UnitType.ExtraBrigade3,
            UnitType.ExtraBrigade4,
            UnitType.ExtraBrigade5,
            UnitType.ExtraBrigade6,
            UnitType.ExtraBrigade7,
            UnitType.ExtraBrigade8,
            UnitType.ExtraBrigade9,
            UnitType.ExtraBrigade10,
            UnitType.ExtraBrigade11,
            UnitType.ExtraBrigade12,
            UnitType.ExtraBrigade13,
            UnitType.ExtraBrigade14,
            UnitType.ExtraBrigade15,
            UnitType.ExtraBrigade16,
            UnitType.ExtraBrigade17,
            UnitType.ExtraBrigade18,
            UnitType.ExtraBrigade19,
            UnitType.ExtraBrigade20
        };

        /// <summary>
        ///     Available unit types (DH1.03 from )
        /// </summary>
        private static readonly UnitType[] BrigadeTypesDh =
        {
            UnitType.None,
            UnitType.Artillery,
            UnitType.SpArtillery,
            UnitType.RocketArtillery,
            UnitType.SpRctArtillery,
            UnitType.AntiTank,
            UnitType.TankDestroyer,
            UnitType.LightArmorBrigade,
            UnitType.HeavyArmor,
            UnitType.SuperHeavyArmor,
            UnitType.ArmoredCar,
            UnitType.AntiAir,
            UnitType.Police,
            UnitType.Engineer,
            UnitType.Cag,
            UnitType.Escort,
            UnitType.NavalAsw,
            UnitType.NavalAntiAirS,
            UnitType.NavalRadarS,
            UnitType.NavalFireControllS,
            UnitType.NavalImprovedHullS,
            UnitType.NavalTorpedoesS,
            UnitType.NavalAntiAirL,
            UnitType.NavalRadarL,
            UnitType.NavalFireControllL,
            UnitType.NavalImprovedHullL,
            UnitType.NavalTorpedoesL,
            UnitType.CavalryBrigade,
            UnitType.SpAntiAir,
            UnitType.MediumArmor,
            UnitType.FloatPlane,
            UnitType.LightCag,
            UnitType.AmphArmor,
            UnitType.GliderArmor,
            UnitType.GliderArtillery,
            UnitType.SuperHeavyArtillery,
            UnitType.ReserveBrigade36,
            UnitType.ReserveBrigade37,
            UnitType.ReserveBrigade38,
            UnitType.ReserveBrigade39,
            UnitType.ReserveBrigade40,
            UnitType.Brigade01,
            UnitType.Brigade02,
            UnitType.Brigade03,
            UnitType.Brigade04,
            UnitType.Brigade05,
            UnitType.Brigade06,
            UnitType.Brigade07,
            UnitType.Brigade08,
            UnitType.Brigade09,
            UnitType.Brigade10,
            UnitType.Brigade11,
            UnitType.Brigade12,
            UnitType.Brigade13,
            UnitType.Brigade14,
            UnitType.Brigade15,
            UnitType.Brigade16,
            UnitType.Brigade17,
            UnitType.Brigade18,
            UnitType.Brigade19,
            UnitType.Brigade20,
            UnitType.Brigade21,
            UnitType.Brigade22,
            UnitType.Brigade23,
            UnitType.Brigade24,
            UnitType.Brigade25,
            UnitType.Brigade26,
            UnitType.Brigade27,
            UnitType.Brigade28,
            UnitType.Brigade29,
            UnitType.Brigade30,
            UnitType.Brigade31,
            UnitType.Brigade32,
            UnitType.Brigade33,
            UnitType.Brigade34,
            UnitType.Brigade35,
            UnitType.Brigade36,
            UnitType.Brigade37,
            UnitType.Brigade38,
            UnitType.Brigade39,
            UnitType.Brigade40,
            UnitType.Brigade41,
            UnitType.Brigade42,
            UnitType.Brigade43,
            UnitType.Brigade44,
            UnitType.Brigade45,
            UnitType.Brigade46,
            UnitType.Brigade47,
            UnitType.Brigade48,
            UnitType.Brigade49,
            UnitType.Brigade50,
            UnitType.Brigade51,
            UnitType.Brigade52,
            UnitType.Brigade53,
            UnitType.Brigade54,
            UnitType.Brigade55,
            UnitType.Brigade56,
            UnitType.Brigade57,
            UnitType.Brigade58,
            UnitType.Brigade59,
            UnitType.Brigade60,
            UnitType.Brigade61,
            UnitType.Brigade62,
            UnitType.Brigade63,
            UnitType.Brigade64,
            UnitType.Brigade65,
            UnitType.Brigade66,
            UnitType.Brigade67,
            UnitType.Brigade68,
            UnitType.Brigade69,
            UnitType.Brigade70,
            UnitType.Brigade71,
            UnitType.Brigade72,
            UnitType.Brigade73,
            UnitType.Brigade74,
            UnitType.Brigade75,
            UnitType.Brigade76,
            UnitType.Brigade77,
            UnitType.Brigade78,
            UnitType.Brigade79,
            UnitType.Brigade80,
            UnitType.Brigade81,
            UnitType.Brigade82,
            UnitType.Brigade83,
            UnitType.Brigade84,
            UnitType.Brigade85,
            UnitType.Brigade86,
            UnitType.Brigade87,
            UnitType.Brigade88,
            UnitType.Brigade89,
            UnitType.Brigade90,
            UnitType.Brigade91,
            UnitType.Brigade92,
            UnitType.Brigade93,
            UnitType.Brigade94,
            UnitType.Brigade95,
            UnitType.Brigade96,
            UnitType.Brigade97,
            UnitType.Brigade98,
            UnitType.Brigade99
        };

        /// <summary>
        ///     Initial setting value of unit definition file name
        /// </summary>
        private static readonly string[] DefaultFileNames =
        {
            "",
            "infantry.txt",
            "cavalry.txt",
            "motorized.txt",
            "mechanized.txt",
            "light_armor.txt",
            "armor.txt",
            "paratrooper.txt",
            "marine.txt",
            "bergsjaeger.txt",
            "garrison.txt",
            "hq.txt",
            "militia.txt",
            "multi_role.txt",
            "interceptor.txt",
            "strategic_bomber.txt",
            "tactical_bomber.txt",
            "naval_bomber.txt",
            "cas.txt",
            "transport_plane.txt",
            "flying_bomb.txt",
            "flying_rocket.txt",
            "battleship.txt",
            "light_cruiser.txt",
            "heavy_cruiser.txt",
            "battlecruiser.txt",
            "destroyer.txt",
            "carrier.txt",
            "escort_carrier.txt",
            "submarine.txt",
            "nuclear_submarine.txt",
            "transport.txt",
            "light_carrier.txt",
            "rocket_interceptor.txt",
            "d_rsv_33.txt",
            "d_rsv_34.txt",
            "d_rsv_35.txt",
            "d_rsv_36.txt",
            "d_rsv_37.txt",
            "d_rsv_38.txt",
            "d_rsv_39.txt",
            "d_rsv_40.txt",
            "d_01.txt",
            "d_02.txt",
            "d_03.txt",
            "d_04.txt",
            "d_05.txt",
            "d_06.txt",
            "d_07.txt",
            "d_08.txt",
            "d_09.txt",
            "d_10.txt",
            "d_11.txt",
            "d_12.txt",
            "d_13.txt",
            "d_14.txt",
            "d_15.txt",
            "d_16.txt",
            "d_17.txt",
            "d_18.txt",
            "d_19.txt",
            "d_20.txt",
            "d_21.txt",
            "d_22.txt",
            "d_23.txt",
            "d_24.txt",
            "d_25.txt",
            "d_26.txt",
            "d_27.txt",
            "d_28.txt",
            "d_29.txt",
            "d_30.txt",
            "d_31.txt",
            "d_32.txt",
            "d_33.txt",
            "d_34.txt",
            "d_35.txt",
            "d_36.txt",
            "d_37.txt",
            "d_38.txt",
            "d_39.txt",
            "d_40.txt",
            "d_41.txt",
            "d_42.txt",
            "d_43.txt",
            "d_44.txt",
            "d_45.txt",
            "d_46.txt",
            "d_47.txt",
            "d_48.txt",
            "d_49.txt",
            "d_50.txt",
            "d_51.txt",
            "d_52.txt",
            "d_53.txt",
            "d_54.txt",
            "d_55.txt",
            "d_56.txt",
            "d_57.txt",
            "d_58.txt",
            "d_59.txt",
            "d_60.txt",
            "d_61.txt",
            "d_62.txt",
            "d_63.txt",
            "d_64.txt",
            "d_65.txt",
            "d_66.txt",
            "d_67.txt",
            "d_68.txt",
            "d_69.txt",
            "d_70.txt",
            "d_71.txt",
            "d_72.txt",
            "d_73.txt",
            "d_74.txt",
            "d_75.txt",
            "d_76.txt",
            "d_77.txt",
            "d_78.txt",
            "d_79.txt",
            "d_80.txt",
            "d_81.txt",
            "d_82.txt",
            "d_83.txt",
            "d_84.txt",
            "d_85.txt",
            "d_86.txt",
            "d_87.txt",
            "d_88.txt",
            "d_89.txt",
            "d_90.txt",
            "d_91.txt",
            "d_92.txt",
            "d_93.txt",
            "d_94.txt",
            "d_95.txt",
            "d_96.txt",
            "d_97.txt",
            "d_98.txt",
            "d_99.txt",
            "none.txt",
            "artillery.txt",
            "sp_artillery.txt",
            "rocket_artillery.txt",
            "sp_rct_artillery.txt",
            "anti_tank.txt",
            "tank_destroyer.txt",
            "light_armor_brigade.txt",
            "heavy_armor.txt",
            "super_heavy_armor.txt",
            "armored_car.txt",
            "anti_air.txt",
            "police.txt",
            "engineer.txt",
            "cag.txt",
            "escort.txt",
            "naval_asw.txt",
            "naval_anti_air_s.txt",
            "naval_radar_s.txt",
            "naval_fire_controll_s.txt",
            "naval_improved_hull_s.txt",
            "naval_torpedoes_s.txt",
            "naval_anti_air_l.txt",
            "naval_radar_l.txt",
            "naval_fire_controll_l.txt",
            "naval_improved_hull_l.txt",
            "naval_torpedoes_l.txt",
            "naval_mines.txt",
            "naval_sa_l.txt",
            "naval_spotter_l.txt",
            "naval_spotter_s.txt",
            "b_u1.txt",
            "b_u2.txt",
            "b_u3.txt",
            "b_u4.txt",
            "b_u5.txt",
            "b_u6.txt",
            "b_u7.txt",
            "b_u8.txt",
            "b_u9.txt",
            "b_u10.txt",
            "b_u11.txt",
            "b_u12.txt",
            "b_u13.txt",
            "b_u14.txt",
            "b_u15.txt",
            "b_u16.txt",
            "b_u17.txt",
            "b_u18.txt",
            "b_u19.txt",
            "b_u20.txt",
            "cavalry_brigade.txt",
            "sp_anti_air.txt",
            "medium_armor.txt",
            "floatplane.txt",
            "light_cag.txt",
            "amph_armor.txt",
            "glider_armor.txt",
            "glider_artillery.txt",
            "super_heavy_artillery.txt",
            "b_rsv_36.txt",
            "b_rsv_37.txt",
            "b_rsv_38.txt",
            "b_rsv_39.txt",
            "b_rsv_40.txt",
            "b_01.txt",
            "b_02.txt",
            "b_03.txt",
            "b_04.txt",
            "b_05.txt",
            "b_06.txt",
            "b_07.txt",
            "b_08.txt",
            "b_09.txt",
            "b_10.txt",
            "b_11.txt",
            "b_12.txt",
            "b_13.txt",
            "b_14.txt",
            "b_15.txt",
            "b_16.txt",
            "b_17.txt",
            "b_18.txt",
            "b_19.txt",
            "b_20.txt",
            "b_21.txt",
            "b_22.txt",
            "b_23.txt",
            "b_24.txt",
            "b_25.txt",
            "b_26.txt",
            "b_27.txt",
            "b_28.txt",
            "b_29.txt",
            "b_30.txt",
            "b_31.txt",
            "b_32.txt",
            "b_33.txt",
            "b_34.txt",
            "b_35.txt",
            "b_36.txt",
            "b_37.txt",
            "b_38.txt",
            "b_39.txt",
            "b_40.txt",
            "b_41.txt",
            "b_42.txt",
            "b_43.txt",
            "b_44.txt",
            "b_45.txt",
            "b_46.txt",
            "b_47.txt",
            "b_48.txt",
            "b_49.txt",
            "b_50.txt",
            "b_51.txt",
            "b_52.txt",
            "b_53.txt",
            "b_54.txt",
            "b_55.txt",
            "b_56.txt",
            "b_57.txt",
            "b_58.txt",
            "b_59.txt",
            "b_60.txt",
            "b_61.txt",
            "b_62.txt",
            "b_63.txt",
            "b_64.txt",
            "b_65.txt",
            "b_66.txt",
            "b_67.txt",
            "b_68.txt",
            "b_69.txt",
            "b_70.txt",
            "b_71.txt",
            "b_72.txt",
            "b_73.txt",
            "b_74.txt",
            "b_75.txt",
            "b_76.txt",
            "b_77.txt",
            "b_78.txt",
            "b_79.txt",
            "b_80.txt",
            "b_81.txt",
            "b_82.txt",
            "b_83.txt",
            "b_84.txt",
            "b_85.txt",
            "b_86.txt",
            "b_87.txt",
            "b_88.txt",
            "b_89.txt",
            "b_90.txt",
            "b_91.txt",
            "b_92.txt",
            "b_93.txt",
            "b_94.txt",
            "b_95.txt",
            "b_96.txt",
            "b_97.txt",
            "b_98.txt",
            "b_99.txt"
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Units()
        {
            // Unit list
            Items = new List<UnitClass>();

            // Unit type string and ID Initialize the mapping of
            StringMap = new Dictionary<string, UnitType>();
            foreach (UnitType type in Enum.GetValues(typeof (UnitType)))
            {
                StringMap.Add(Strings[(int) type], type);
            }

            // With the actual unit type string ID Initialize the mapping of
            RealStringMap = new Dictionary<string, RealUnitType>();
            foreach (RealUnitType type in Enum.GetValues(typeof (RealUnitType)))
            {
                RealStringMap.Add(RealStrings[(int) type], type);
            }

            // Sprite type with character string ID Initialize the mapping of
            SpriteStringMap = new Dictionary<string, SpriteType>();
            foreach (SpriteType type in Enum.GetValues(typeof (SpriteType)))
            {
                SpriteStringMap.Add(SpriteStrings[(int) type], type);
            }

            // With equipment string ID Initialize the mapping of
            EquipmentStringMap = new Dictionary<string, EquipmentType>();
            foreach (EquipmentType type in Enum.GetValues(typeof (EquipmentType)))
            {
                EquipmentStringMap.Add(EquipmentStrings[(int) type], type);
            }
        }

        /// <summary>
        ///     Initialize unit data
        /// </summary>
        public static void Init()
        {
            InitTypes();
        }

        /// <summary>
        ///     Initialize available unit types
        /// </summary>
        private static void InitTypes()
        {
            if (Game.Type == GameType.ArsenalOfDemocracy)
            {
                DivisionTypes = DivisionTypesAoD;
                BrigadeTypes = BrigadeTypesAoD;
            }
            else if (Game.Type == GameType.DarkestHour && Game.Version >= 103)
            {
                DivisionTypes = DivisionTypesDh;
                BrigadeTypes = BrigadeTypesDh;
            }
            else
            {
                DivisionTypes = DivisionTypesHoI2;
                BrigadeTypes = BrigadeTypesHoI2;
            }

            UnitTypes = new List<UnitType>();
            UnitTypes.AddRange(DivisionTypes);
            UnitTypes.AddRange(BrigadeTypes);
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
        ///     Reload the unit definition files
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
        ///     Read the unit definition files
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
        ///     Lazy loading of unit definition files
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
        ///     Read the unit definition files
        /// </summary>
        private static void LoadFiles()
        {
            Items.Clear();

            // Set the initial value of unit class data
            foreach (UnitType type in Enum.GetValues(typeof (UnitType)))
            {
                Items.Add(new UnitClass(type));
            }

            // Read the unit class definition file (DH1.03 from )
            if (Game.Type == GameType.DarkestHour && Game.Version >= 103)
            {
                string fileName = "";
                try
                {
                    fileName = Game.GetReadFileName(Game.DhDivisionTypePathName);
                    LoadDivisionTypes(fileName);

                    fileName = Game.GetReadFileName(Game.DhBrigadeTypePathName);
                    LoadBrigadeTypes(fileName);
                }
                catch (Exception)
                {
                    Log.Error("[Unit] Read error: {0}", fileName);
                    MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                        Resources.EditorUnit, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Read the unit definition files in order
            bool error = false;
            foreach (UnitType type in UnitTypes)
            {
                try
                {
                    LoadFile(type);
                }
                catch (Exception)
                {
                    error = true;
                    UnitClass unit = Items[(int) type];
                    string fileName =
                        Game.GetReadFileName(
                            unit.Organization == UnitOrganization.Division
                                ? Game.DivisionPathName
                                : Game.BrigadePathName, DefaultFileNames[(int) type]);
                    Log.Error("[Unit] Read error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                        Resources.EditorUnit, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return;
                    }
                }
            }

            // Return if reading fails
            if (error)
            {
                return;
            }

            // Clear the edited flag
            _dirtyFlag = false;

            // Clear the edited flag for the model name
            ResetDirtyAllModelName();

            // Reflect the edited flag of the maximum number of attached brigades
            if ((Game.Type == GameType.ArsenalOfDemocracy) && (Game.Version >= 107))
            {
                UpdateDirtyMaxAllowedBrigades();
            }

            // Set the read flag
            _loaded = true;
        }

        /// <summary>
        ///     Read the unit definition file
        /// </summary>
        /// <param name="type">Unit type</param>
        private static void LoadFile(UnitType type)
        {
            // Parse the unit definition file
            UnitClass unit = Items[(int) type];
            string fileName =
                Game.GetReadFileName(
                    unit.Organization == UnitOrganization.Division ? Game.DivisionPathName : Game.BrigadePathName,
                    DefaultFileNames[(int) type]);
            if (!File.Exists(fileName))
            {
                return;
            }

            Log.Verbose("[Unit] Load: {0}", Path.GetFileName(fileName));

            UnitParser.Parse(fileName, unit);
        }

        /// <summary>
        ///     Read the division unit class definition file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void LoadDivisionTypes(string fileName)
        {
            // Back if the file does not exist
            if (!File.Exists(fileName))
            {
                return;
            }

            Log.Verbose("[Unit] Load: {0}", Path.GetFileName(fileName));

            // Parse the file
            UnitParser.ParseDivisionTypes(fileName, Items);

            // Clear the edited flag
            ResetDirtyDivisionTypes();
        }

        /// <summary>
        ///     Read the brigade unit class definition file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void LoadBrigadeTypes(string fileName)
        {
            // Back if the file does not exist
            if (!File.Exists(fileName))
            {
                return;
            }

            Log.Verbose("[Unit] Load: {0}", Path.GetFileName(fileName));

            // Parse the file
            UnitParser.ParseBrigadeTypes(fileName, Items);

            // Clear the edited flag
            ResetDirtyBrigadeTypes();
        }

        #endregion

        #region File writing

        /// <summary>
        ///     Save the unit definition files
        /// </summary>
        /// <returns>If saving fails false false return it</returns>
        public static bool Save()
        {
            // Wait for completion if loading is in progress
            if (Worker.IsBusy)
            {
                WaitLoading();
            }

            if ((Game.Type == GameType.DarkestHour) && (Game.Version >= 103))
            {
                if (IsDirtyDivisionTypes())
                {
                    // Save the division definition file
                    string fileName = Game.GetWriteFileName(Game.DhDivisionTypePathName);
                    try
                    {
                        SaveDivisionTypes(fileName);
                    }
                    catch (Exception)
                    {
                        Log.Error("[Unit] Write error: {0}", fileName);
                        MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                            Resources.EditorUnit, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                if (IsDirtyBrigadeTypes())
                {
                    // Save the brigade definition file
                    string fileName = Game.GetWriteFileName(Game.DhBrigadeTypePathName);
                    try
                    {
                        SaveBrigadeTypes(fileName);
                    }
                    catch (Exception)
                    {
                        Log.Error("[Unit] Write error: {0}", fileName);
                        MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                            Resources.EditorUnit, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }

            // Save in order to the unit definition file
            bool error = false;
            foreach (UnitClass unit in Items.Where(unit => unit.IsDirtyFile()))
            {
                string folderName = Game.GetWriteFileName(unit.Organization == UnitOrganization.Division
                    ? Game.DivisionPathName
                    : Game.BrigadePathName);
                string fileName = Path.Combine(folderName, DefaultFileNames[(int) unit.Type]);

                try
                {
                    // Division / / Create a brigade definition folder if it does not exist
                    if (!Directory.Exists(folderName))
                    {
                        Directory.CreateDirectory(folderName);
                    }

                    Log.Info("[Unit] Save: {0}", Path.GetFileName(fileName));

                    // Save the unit definition file
                    UnitWriter.Write(unit, fileName);
                }
                catch (Exception)
                {
                    error = true;
                    Log.Error("[Unit] Write error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                        Resources.EditorUnit, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
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

            // Clear the edited flag
            _dirtyFlag = false;

            if (_loaded)
            {
                // When saving only the character string definition, the edited flag such as the unit class name is not cleared, so clear all here.
                foreach (UnitClass unit in Items)
                {
                    unit.ResetDirtyAll();
                }

                // Clear the edited flag for the model name
                ResetDirtyAllModelName();
            }

            return true;
        }

        /// <summary>
        ///     Save the division unit class definition file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void SaveDivisionTypes(string fileName)
        {
            // Do nothing if there is no change
            if (!IsDirtyDivisionTypes())
            {
                return;
            }

            // If there is no unit definition folder, create it
            string folderName = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(folderName) && !Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            Log.Info("[Unit] Save: {0}", Path.GetFileName(fileName));

            // Save the division unit class definition file
            UnitWriter.WriteDivisionTypes(Items, fileName);

            // Clear the edited flag
            ResetDirtyDivisionTypes();
        }

        /// <summary>
        ///     Save the brigade unit class definition file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void SaveBrigadeTypes(string fileName)
        {
            // Do nothing if there is no change
            if (!IsDirtyBrigadeTypes())
            {
                return;
            }

            // If there is no unit definition folder, create it
            string folderName = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(folderName) && !Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            Log.Info("[Unit] Save: {0}", Path.GetFileName(fileName));

            // Save the brigade unit class definition file
            UnitWriter.WriteBrigadeTypes(Items, fileName);

            // Clear the edited flag
            ResetDirtyBrigadeTypes();
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
        ///     Get if the division unit class definition has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        private static bool IsDirtyDivisionTypes()
        {
            return _divisionTypesDirty;
        }

        /// <summary>
        ///     Get if the brigade unit class definition has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        private static bool IsDirtyBrigadeTypes()
        {
            return _brigadeTypesDirty;
        }

        /// <summary>
        ///     Set the edited flag for the division unit class definition
        /// </summary>
        public static void SetDirtyDivisionTypes()
        {
            _divisionTypesDirty = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag for the brigade unit class definition
        /// </summary>
        public static void SetDirtyBrigadeTypes()
        {
            _brigadeTypesDirty = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear the edited flag in the division unit class definition
        /// </summary>
        private static void ResetDirtyDivisionTypes()
        {
            _divisionTypesDirty = false;
        }

        /// <summary>
        ///     Clear the edited flag in the brigade unit class definition
        /// </summary>
        private static void ResetDirtyBrigadeTypes()
        {
            _brigadeTypesDirty = false;
        }

        /// <summary>
        ///     Get if the model name has been edited
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirtyModelName(Country country)
        {
            return CountryNameDirtyFlags[(int) country];
        }

        /// <summary>
        ///     Get if the model name has been edited
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirtyModelName(Country country, UnitType type)
        {
            return TypeNameDirtyFlags[(int) country, (int) type];
        }

        /// <summary>
        ///     Set the edited flag for the model name
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <param name="type">Unit name type</param>
        public static void SetDirtyModelName(Country country, UnitType type)
        {
            TypeNameDirtyFlags[(int) country, (int) type] = true;
            CountryNameDirtyFlags[(int) country] = true;
        }

        /// <summary>
        ///     Clear all edited flags for the model name
        /// </summary>
        private static void ResetDirtyAllModelName()
        {
            foreach (Country country in Enum.GetValues(typeof (Country)))
            {
                foreach (UnitType type in UnitTypes)
                {
                    TypeNameDirtyFlags[(int) country, (int) type] = false;
                }
                CountryNameDirtyFlags[(int) country] = false;
            }
        }

        /// <summary>
        ///     Update the edited flag for the maximum number of attached brigades
        /// </summary>
        private static void UpdateDirtyMaxAllowedBrigades()
        {
            // Maximum number of equipment attached to the transport ship
            if (Misc.IsDirty(MiscItemId.TpMaxAttach))
            {
                Items[(int) UnitType.Transport].SetDirty(UnitClassItemId.MaxAllowedBrigades);
            }
            // Maximum number of submersible equipment
            if (Misc.IsDirty(MiscItemId.SsMaxAttach))
            {
                Items[(int) UnitType.Submarine].SetDirty(UnitClassItemId.MaxAllowedBrigades);
            }
            // Maximum number of equipment attached to nuclear submarines
            if (Misc.IsDirty(MiscItemId.SsnMaxAttach))
            {
                Items[(int) UnitType.NuclearSubmarine].SetDirty(UnitClassItemId.MaxAllowedBrigades);
            }
            // Maximum number of equipment attached to the destroyer
            if (Misc.IsDirty(MiscItemId.DdMaxAttach))
            {
                Items[(int) UnitType.Destroyer].SetDirty(UnitClassItemId.MaxAllowedBrigades);
            }
            // Maximum number of equipment attached to light cruisers
            if (Misc.IsDirty(MiscItemId.ClMaxAttach))
            {
                Items[(int) UnitType.LightCruiser].SetDirty(UnitClassItemId.MaxAllowedBrigades);
            }
            // Maximum number of heavy cruisers attached
            if (Misc.IsDirty(MiscItemId.CaMaxAttach))
            {
                Items[(int) UnitType.HeavyCruiser].SetDirty(UnitClassItemId.MaxAllowedBrigades);
            }
            // Maximum number of equipment attached to cruise battleships
            if (Misc.IsDirty(MiscItemId.BcMaxAttach))
            {
                Items[(int) UnitType.BattleCruiser].SetDirty(UnitClassItemId.MaxAllowedBrigades);
            }
            // Maximum number of attached equipment for battleships
            if (Misc.IsDirty(MiscItemId.BbMaxAttach))
            {
                Items[(int) UnitType.BattleShip].SetDirty(UnitClassItemId.MaxAllowedBrigades);
            }
            // Maximum number of equipment attached to the light carrier
            if (Misc.IsDirty(MiscItemId.CvlMaxAttach))
            {
                Items[(int) UnitType.EscortCarrier].SetDirty(UnitClassItemId.MaxAllowedBrigades);
            }
            // Maximum number of equipment attached to the aircraft carrier
            if (Misc.IsDirty(MiscItemId.CvMaxAttach))
            {
                Items[(int) UnitType.Carrier].SetDirty(UnitClassItemId.MaxAllowedBrigades);
            }
        }

        #endregion
    }
}
