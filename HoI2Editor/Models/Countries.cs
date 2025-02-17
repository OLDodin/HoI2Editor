﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     National data
    /// </summary>
    public static class Countries
    {
        #region Public properties

        /// <summary>
        ///     Country tag list
        /// </summary>
        public static Country[] Tags { get; private set; }

        /// <summary>
        ///     With country tag string ID Correspondence of
        /// </summary>
        public static Dictionary<string, Country> StringMap { get; }

        #endregion

        #region Public constant

        /// <summary>
        ///     Country tag string
        /// </summary>
        public static readonly string[] Strings =
        {
            "",
            "AFG",
            "ALB",
            "ALG",
            "ALI",
            "ALS",
            "ANG",
            "ARA",
            "ARG",
            "ARM",
            "AST",
            "AUS",
            "AXI",
            "AZB",
            "BEL",
            "BEN",
            "BHU",
            "BLR",
            "BOL",
            "BOS",
            "BRA",
            "BRU",
            "BUL",
            "BUR",
            "CAL",
            "CAM",
            "CAN",
            "CGX",
            "CHC",
            "CHI",
            "CHL",
            "CMB",
            "COL",
            "CON",
            "COS",
            "CRO",
            "CSA",
            "CSX",
            "CUB",
            "CXB",
            "CYN",
            "CYP",
            "CZE",
            "DDR",
            "DEN",
            "DFR",
            "DOM",
            "EAF",
            "ECU",
            "EGY",
            "ENG",
            "EQA",
            "EST",
            "ETH",
            "EUS",
            "FIN",
            "FLA",
            "FRA",
            "GAB",
            "GEO",
            "GER",
            "GLD",
            "GRE",
            "GUA",
            "GUI",
            "GUY",
            "HAI",
            "HOL",
            "HON",
            "HUN",
            "ICL",
            "IDC",
            "IND",
            "INO",
            "IRE",
            "IRQ",
            "ISR",
            "ITA",
            "JAP",
            "JOR",
            "KAZ",
            "KOR",
            "KUR",
            "KYG",
            "LAO",
            "LAT",
            "LBY",
            "LEB",
            "LIB",
            "LIT",
            "LUX",
            "MAD",
            "MAL",
            "MAN",
            "MEN",
            "MEX",
            "MLY",
            "MON",
            "MOR",
            "MOZ",
            "MTN",
            "NAM",
            "NEP",
            "NIC",
            "NIG",
            "NOR",
            "NZL",
            "OMN",
            "OTT",
            "PAK",
            "PAL",
            "PAN",
            "PAR",
            "PER",
            "PHI",
            "POL",
            "POR",
            "PRI",
            "PRK",
            "PRU",
            "QUE",
            "REB",
            "RHO",
            "ROM",
            "RSI",
            "RUS",
            "SAF",
            "SAL",
            "SAR",
            "SAU",
            "SCA",
            "SCH",
            "SCO",
            "SER",
            "SIA",
            "SIB",
            "SIE",
            "SIK",
            "SLO",
            "SLV",
            "SOM",
            "SOV",
            "SPA",
            "SPR",
            "SUD",
            "SWE",
            "SYR",
            "TAJ",
            "TAN",
            "TEX",
            "TIB",
            "TRA",
            "TRK",
            "TUN",
            "TUR",
            "UAP",
            "UAU",
            "UBO",
            "UCH",
            "UCS",
            "UER",
            "UES",
            "UGS",
            "UIC",
            "UIR",
            "UKR",
            "UPE",
            "UPR",
            "UPS",
            "URO",
            "URU",
            "USA",
            "USN",
            "UTC",
            "UTL",
            "UTO",
            "UZB",
            "VEN",
            "VIC",
            "VIE",
            "WLL",
            "YEM",
            "YUG",
            "U00",
            "U01",
            "U02",
            "U03",
            "U04",
            "U05",
            "U06",
            "U07",
            "U08",
            "U09",
            "U10",
            "U11",
            "U12",
            "U13",
            "U14",
            "U15",
            "U16",
            "U17",
            "U18",
            "U19",
            "U20",
            "U21",
            "U22",
            "U23",
            "U24",
            "U25",
            "U26",
            "U27",
            "U28",
            "U29",
            "U30",
            "U31",
            "U32",
            "U33",
            "U34",
            "U35",
            "U36",
            "U37",
            "U38",
            "U39",
            "U40",
            "U41",
            "U42",
            "U43",
            "U44",
            "U45",
            "U46",
            "U47",
            "U48",
            "U49",
            "U50",
            "U51",
            "U52",
            "U53",
            "U54",
            "U55",
            "U56",
            "U57",
            "U58",
            "U59",
            "U60",
            "U61",
            "U62",
            "U63",
            "U64",
            "U65",
            "U66",
            "U67",
            "U68",
            "U69",
            "U70",
            "U71",
            "U72",
            "U73",
            "U74",
            "U75",
            "U76",
            "U77",
            "U78",
            "U79",
            "U80",
            "U81",
            "U82",
            "U83",
            "U84",
            "U85",
            "U86",
            "U87",
            "U88",
            "U89",
            "U90",
            "U91",
            "U92",
            "U93",
            "U94",
            "U95",
            "U96",
            "U97",
            "U98",
            "U99",
            "UA0",
            "UA1",
            "UA2",
            "UA3",
            "UA4",
            "UA5",
            "UA6",
            "UA7",
            "UA8",
            "UA9",
            "UB0",
            "UB1",
            "UB2",
            "UB3",
            "UB4",
            "UB5",
            "UB6",
            "UB7",
            "UB8",
            "UB9",
            "UC0",
            "UC1",
            "UC2",
            "UC3",
            "UC4",
            "UC5",
            "UC6",
            "UC7",
            "UC8",
            "UC9",
            "UD0",
            "UD1",
            "UD2",
            "UD3",
            "UD4",
            "UD5",
            "UD6",
            "UD7",
            "UD8",
            "UD9",
            "UE0",
            "UE1",
            "UE2",
            "UE3",
            "UE4",
            "UE5",
            "UE6",
            "UE7",
            "UE8",
            "UE9",
            "UF0",
            "UF1",
            "UF2",
            "UF3",
            "UF4",
            "UF5",
            "UF6",
            "UF7",
            "UF8",
            "UF9"
        };

        #endregion

        #region Internal constant

        /// <summary>
        ///     Country tag list (HoI2)
        /// </summary>
        private static readonly Country[] TagsHoI2 =
        {
            Country.AFG,
            Country.ALB,
            Country.ALG,
            Country.ALI,
            Country.ALS,
            Country.ANG,
            Country.ARA,
            Country.ARG,
            Country.ARM,
            Country.AST,
            Country.AUS,
            Country.AXI,
            Country.AZB,
            Country.BEL,
            Country.BEN,
            Country.BHU,
            Country.BLR,
            Country.BOL,
            Country.BOS,
            Country.BRA,
            Country.BRU,
            Country.BUL,
            Country.BUR,
            Country.CAL,
            Country.CAM,
            Country.CAN,
            Country.CGX,
            Country.CHC,
            Country.CHI,
            Country.CHL,
            Country.CMB,
            Country.COL,
            Country.CON,
            Country.COS,
            Country.CRO,
            Country.CSA,
            Country.CSX,
            Country.CUB,
            Country.CXB,
            Country.CYN,
            Country.CYP,
            Country.CZE,
            Country.DDR,
            Country.DEN,
            Country.DFR,
            Country.DOM,
            Country.EAF,
            Country.ECU,
            Country.EGY,
            Country.ENG,
            Country.EQA,
            Country.EST,
            Country.ETH,
            Country.EUS,
            Country.FIN,
            Country.FLA,
            Country.FRA,
            Country.GAB,
            Country.GEO,
            Country.GER,
            Country.GLD,
            Country.GRE,
            Country.GUA,
            Country.GUI,
            Country.GUY,
            Country.HAI,
            Country.HOL,
            Country.HON,
            Country.HUN,
            Country.ICL,
            Country.IDC,
            Country.IND,
            Country.INO,
            Country.IRE,
            Country.IRQ,
            Country.ISR,
            Country.ITA,
            Country.JAP,
            Country.JOR,
            Country.KAZ,
            Country.KOR,
            Country.KUR,
            Country.KYG,
            Country.LAO,
            Country.LAT,
            Country.LBY,
            Country.LEB,
            Country.LIB,
            Country.LIT,
            Country.LUX,
            Country.MAD,
            Country.MAL,
            Country.MAN,
            Country.MEN,
            Country.MEX,
            Country.MLY,
            Country.MON,
            Country.MOR,
            Country.MOZ,
            Country.MTN,
            Country.NAM,
            Country.NEP,
            Country.NIC,
            Country.NIG,
            Country.NOR,
            Country.NZL,
            Country.OMN,
            Country.OTT,
            Country.PAK,
            Country.PAL,
            Country.PAN,
            Country.PAR,
            Country.PER,
            Country.PHI,
            Country.POL,
            Country.POR,
            Country.PRI,
            Country.PRK,
            Country.PRU,
            Country.QUE,
            Country.REB,
            Country.RHO,
            Country.ROM,
            Country.RSI,
            Country.RUS,
            Country.SAF,
            Country.SAL,
            Country.SAR,
            Country.SAU,
            Country.SCA,
            Country.SCH,
            Country.SCO,
            Country.SER,
            Country.SIA,
            Country.SIB,
            Country.SIE,
            Country.SIK,
            Country.SLO,
            Country.SLV,
            Country.SOM,
            Country.SOV,
            Country.SPA,
            Country.SPR,
            Country.SUD,
            Country.SWE,
            Country.SYR,
            Country.TAJ,
            Country.TAN,
            Country.TEX,
            Country.TIB,
            Country.TRA,
            Country.TRK,
            Country.TUN,
            Country.TUR,
            Country.UAP,
            Country.UAU,
            Country.UBO,
            Country.UCH,
            Country.UCS,
            Country.UER,
            Country.UES,
            Country.UGS,
            Country.UIC,
            Country.UIR,
            Country.UKR,
            Country.UPE,
            Country.UPR,
            Country.UPS,
            Country.URO,
            Country.URU,
            Country.USA,
            Country.USN,
            Country.UTC,
            Country.UTL,
            Country.UTO,
            Country.UZB,
            Country.VEN,
            Country.VIC,
            Country.VIE,
            Country.WLL,
            Country.YEM,
            Country.YUG,
            Country.U00,
            Country.U01,
            Country.U02,
            Country.U03,
            Country.U04,
            Country.U05,
            Country.U06,
            Country.U07,
            Country.U08,
            Country.U09,
            Country.U10,
            Country.U11,
            Country.U12,
            Country.U13,
            Country.U14,
            Country.U15,
            Country.U16,
            Country.U17,
            Country.U18,
            Country.U19,
            Country.U20,
            Country.U21,
            Country.U22,
            Country.U23,
            Country.U24,
            Country.U25,
            Country.U26,
            Country.U27,
            Country.U28,
            Country.U29,
            Country.U30,
            Country.U31,
            Country.U32,
            Country.U33,
            Country.U34,
            Country.U35,
            Country.U36,
            Country.U37,
            Country.U38,
            Country.U39,
            Country.U40,
            Country.U41,
            Country.U42,
            Country.U43,
            Country.U44,
            Country.U45,
            Country.U46,
            Country.U47,
            Country.U48,
            Country.U49,
            Country.U50,
            Country.U51,
            Country.U52,
            Country.U53,
            Country.U54,
            Country.U55,
            Country.U56,
            Country.U57,
            Country.U58,
            Country.U59,
            Country.U60,
            Country.U61,
            Country.U62,
            Country.U63,
            Country.U64,
            Country.U65,
            Country.U66,
            Country.U67,
            Country.U68,
            Country.U69,
            Country.U70,
            Country.U71,
            Country.U72,
            Country.U73,
            Country.U74,
            Country.U75,
            Country.U76,
            Country.U77,
            Country.U78,
            Country.U79,
            Country.U80,
            Country.U81,
            Country.U82,
            Country.U83,
            Country.U84,
            Country.U85,
            Country.U86,
            Country.U87,
            Country.U88,
            Country.U89,
            Country.U90,
            Country.U91,
            Country.U92,
            Country.U93,
            Country.U94,
            Country.U95,
            Country.U96,
            Country.U97,
            Country.U98,
            Country.U99,
        };

        /// <summary>
        ///     Country tag list (AoD)
        /// </summary>
        private static readonly Country[] TagsAoD =
        {
            Country.AFG,
            Country.ALB,
            Country.ALG,
            Country.ALI,
            Country.ALS,
            Country.ANG,
            Country.ARA,
            Country.ARG,
            Country.ARM,
            Country.AST,
            Country.AUS,
            Country.AXI,
            Country.AZB,
            Country.BEL,
            Country.BEN,
            Country.BHU,
            Country.BLR,
            Country.BOL,
            Country.BOS,
            Country.BRA,
            Country.BRU,
            Country.BUL,
            Country.BUR,
            Country.CAL,
            Country.CAM,
            Country.CAN,
            Country.CGX,
            Country.CHC,
            Country.CHI,
            Country.CHL,
            Country.CMB,
            Country.COL,
            Country.CON,
            Country.COS,
            Country.CRO,
            Country.CSA,
            Country.CSX,
            Country.CUB,
            Country.CXB,
            Country.CYN,
            Country.CYP,
            Country.CZE,
            Country.DDR,
            Country.DEN,
            Country.DFR,
            Country.DOM,
            Country.EAF,
            Country.ECU,
            Country.EGY,
            Country.ENG,
            Country.EQA,
            Country.EST,
            Country.ETH,
            Country.EUS,
            Country.FIN,
            Country.FLA,
            Country.FRA,
            Country.GAB,
            Country.GEO,
            Country.GER,
            Country.GLD,
            Country.GRE,
            Country.GUA,
            Country.GUI,
            Country.GUY,
            Country.HAI,
            Country.HOL,
            Country.HON,
            Country.HUN,
            Country.ICL,
            Country.IDC,
            Country.IND,
            Country.INO,
            Country.IRE,
            Country.IRQ,
            Country.ISR,
            Country.ITA,
            Country.JAP,
            Country.JOR,
            Country.KAZ,
            Country.KOR,
            Country.KUR,
            Country.KYG,
            Country.LAO,
            Country.LAT,
            Country.LBY,
            Country.LEB,
            Country.LIB,
            Country.LIT,
            Country.LUX,
            Country.MAD,
            Country.MAL,
            Country.MAN,
            Country.MEN,
            Country.MEX,
            Country.MLY,
            Country.MON,
            Country.MOR,
            Country.MOZ,
            Country.MTN,
            Country.NAM,
            Country.NEP,
            Country.NIC,
            Country.NIG,
            Country.NOR,
            Country.NZL,
            Country.OMN,
            Country.OTT,
            Country.PAK,
            Country.PAL,
            Country.PAN,
            Country.PAR,
            Country.PER,
            Country.PHI,
            Country.POL,
            Country.POR,
            Country.PRI,
            Country.PRK,
            Country.PRU,
            Country.QUE,
            Country.REB,
            Country.RHO,
            Country.ROM,
            Country.RSI,
            Country.RUS,
            Country.SAF,
            Country.SAL,
            Country.SAR,
            Country.SAU,
            Country.SCA,
            Country.SCH,
            Country.SCO,
            Country.SER,
            Country.SIA,
            Country.SIB,
            Country.SIE,
            Country.SIK,
            Country.SLO,
            Country.SLV,
            Country.SOM,
            Country.SOV,
            Country.SPA,
            Country.SPR,
            Country.SUD,
            Country.SWE,
            Country.SYR,
            Country.TAJ,
            Country.TAN,
            Country.TEX,
            Country.TIB,
            Country.TRA,
            Country.TRK,
            Country.TUN,
            Country.TUR,
            Country.UAP,
            Country.UAU,
            Country.UBO,
            Country.UCH,
            Country.UCS,
            Country.UER,
            Country.UES,
            Country.UGS,
            Country.UIC,
            Country.UIR,
            Country.UKR,
            Country.UPE,
            Country.UPR,
            Country.UPS,
            Country.URO,
            Country.URU,
            Country.USA,
            Country.USN,
            Country.UTC,
            Country.UTL,
            Country.UTO,
            Country.UZB,
            Country.VEN,
            Country.VIC,
            Country.VIE,
            Country.WLL,
            Country.YEM,
            Country.YUG,
            Country.U00,
            Country.U01,
            Country.U02,
            Country.U03,
            Country.U04,
            Country.U05,
            Country.U06,
            Country.U07,
            Country.U08,
            Country.U09,
            Country.U10,
            Country.U11,
            Country.U12,
            Country.U13,
            Country.U14,
            Country.U15,
            Country.U16,
            Country.U17,
            Country.U18,
            Country.U19,
            Country.U20,
            Country.U21,
            Country.U22,
            Country.U23,
            Country.U24,
            Country.U25,
            Country.U26,
            Country.U27,
            Country.U28,
            Country.U29,
            Country.U30,
            Country.U31,
            Country.U32,
            Country.U33,
            Country.U34,
            Country.U35,
            Country.U36,
            Country.U37,
            Country.U38,
            Country.U39,
            Country.U40,
            Country.U41,
            Country.U42,
            Country.U43,
            Country.U44,
            Country.U45,
            Country.U46,
            Country.U47,
            Country.U48,
            Country.U49,
            Country.U50,
            Country.U51,
            Country.U52,
            Country.U53,
            Country.U54,
            Country.U55,
            Country.U56,
            Country.U57,
            Country.U58,
            Country.U59,
            Country.U60,
            Country.U61,
            Country.U62,
            Country.U63,
            Country.U64,
            Country.U65,
            Country.U66,
            Country.U67,
            Country.U68,
            Country.U69,
            Country.U70,
            Country.U71,
            Country.U72,
            Country.U73,
            Country.U74,
            Country.U75,
            Country.U76,
            Country.U77,
            Country.U78,
            Country.U79,
            Country.U80,
            Country.U81,
            Country.U82,
            Country.U83,
            Country.U84,
            Country.U85,
            Country.U86,
            Country.U87,
            Country.U88,
            Country.U89,
            Country.U90,
            Country.U91,
            Country.U92,
            Country.U93,
            Country.U94,
            Country.U95,
            Country.U96,
            Country.U97,
            Country.U98,
            Country.U99,
            Country.UA0,
            Country.UA1,
            Country.UA2,
            Country.UA3,
            Country.UA4,
            Country.UA5,
            Country.UA6,
            Country.UA7,
            Country.UA8,
            Country.UA9,
            Country.UB0,
            Country.UB1,
            Country.UB2,
            Country.UB3,
            Country.UB4,
            Country.UB5,
            Country.UB6,
            Country.UB7,
            Country.UB8,
            Country.UB9,
            Country.UC0,
            Country.UC1,
            Country.UC2,
            Country.UC3,
            Country.UC4,
            Country.UC5,
            Country.UC6,
            Country.UC7,
            Country.UC8,
            Country.UC9,
            Country.UD0,
            Country.UD1,
            Country.UD2,
            Country.UD3,
            Country.UD4,
            Country.UD5,
            Country.UD6,
            Country.UD7,
            Country.UD8,
            Country.UD9,
            Country.UE0,
            Country.UE1,
            Country.UE2,
            Country.UE3,
            Country.UE4,
            Country.UE5,
            Country.UE6,
            Country.UE7,
            Country.UE8,
            Country.UE9,
            Country.UF0,
            Country.UF1,
            Country.UF2,
            Country.UF3,
            Country.UF4,
            Country.UF5,
            Country.UF6,
            Country.UF7,
            Country.UF8,
            Country.UF9
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Countries()
        {
            StringMap = new Dictionary<string, Country>();
            foreach (Country country in Enum.GetValues(typeof (Country)))
            {
                StringMap.Add(Strings[(int) country], country);
            }
        }

        /// <summary>
        ///     Initialize the country tag list
        /// </summary>
        public static void Init()
        {
            switch (Game.Type)
            {
                case GameType.HeartsOfIron2:
                case GameType.DarkestHour:
                    if( Game.Version >= 105 )
                    {
                        Tags = TagsAoD;
                    } else
                    {
                        Tags = TagsHoI2;
                    }
                    break;

                case GameType.ArsenalOfDemocracy:
                    Tags = TagsAoD;
                    break;
            }
        }

        #endregion

        #region String operation

        /// <summary>
        ///     Get the country name
        /// </summary>
        /// <param name="country">Nation</param>
        /// <returns>Country name</returns>
        public static string GetName(Country country)
        {
            if (country == Country.None)
            {
                return "";
            }
            string tag = Strings[(int) country];
            return Config.ExistsKey(tag) ? Config.GetText(tag) : tag;
        }

        /// <summary>
        ///     Get country tag name and country name
        /// </summary>
        /// <param name="country">Nation</param>
        /// <returns>Country tag name and country name</returns>
        public static string GetTagName(Country country)
        {
            if (country == Country.None)
            {
                return "";
            }
            string tag = Strings[(int) country];
            return Config.ExistsKey(tag)
                ? $"{tag} {Config.GetText(tag)}"
                : tag;
        }

        /// <summary>
        ///     Get the string of the country tag name list
        /// </summary>
        /// <param name="countries">National list</param>
        /// <returns>Country tag name list string</returns>
        public static string GetTagList(IEnumerable<Country> countries)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Country country in countries)
            {
                sb.AppendFormat("{0}, ", Strings[(int) country]);
            }
            int len = sb.Length;
            return len > 0 ? sb.ToString(0, len - 2) : "";
        }

        /// <summary>
        ///     Get the string of country name list
        /// </summary>
        /// <param name="countries">National list</param>
        /// <returns>Country name list string</returns>
        public static string GetNameList(IEnumerable<Country> countries)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Country country in countries)
            {
                sb.AppendFormat("{0}, ", GetName(country));
            }
            int len = sb.Length;
            return len > 0 ? sb.ToString(0, len - 2) : "";
        }

        #endregion
    }

    /// <summary>
    ///     Country tag
    /// </summary>
    public enum Country
    {
        None, // No definition

        // ReSharper disable Inconsistent Naming
        AFG,
        ALB,
        ALG,
        ALI,
        ALS,
        ANG,
        ARA,
        ARG,
        ARM,
        AST,
        AUS,
        AXI,
        AZB,
        BEL,
        BEN,
        BHU,
        BLR,
        BOL,
        BOS,
        BRA,
        BRU,
        BUL,
        BUR,
        CAL,
        CAM,
        CAN,
        CGX,
        CHC,
        CHI,
        CHL,
        CMB,
        COL,
        CON,
        COS,
        CRO,
        CSA,
        CSX,
        CUB,
        CXB,
        CYN,
        CYP,
        CZE,
        DDR,
        DEN,
        DFR,
        DOM,
        EAF,
        ECU,
        EGY,
        ENG,
        EQA,
        EST,
        ETH,
        EUS,
        FIN,
        FLA,
        FRA,
        GAB,
        GEO,
        GER,
        GLD,
        GRE,
        GUA,
        GUI,
        GUY,
        HAI,
        HOL,
        HON,
        HUN,
        ICL,
        IDC,
        IND,
        INO,
        IRE,
        IRQ,
        ISR,
        ITA,
        JAP,
        JOR,
        KAZ,
        KOR,
        KUR,
        KYG,
        LAO,
        LAT,
        LBY,
        LEB,
        LIB,
        LIT,
        LUX,
        MAD,
        MAL,
        MAN,
        MEN,
        MEX,
        MLY,
        MON,
        MOR,
        MOZ,
        MTN,
        NAM,
        NEP,
        NIC,
        NIG,
        NOR,
        NZL,
        OMN,
        OTT,
        PAK,
        PAL,
        PAN,
        PAR,
        PER,
        PHI,
        POL,
        POR,
        PRI,
        PRK,
        PRU,
        QUE,
        REB,
        RHO,
        ROM,
        RSI,
        RUS,
        SAF,
        SAL,
        SAR,
        SAU,
        SCA,
        SCH,
        SCO,
        SER,
        SIA,
        SIB,
        SIE,
        SIK,
        SLO,
        SLV,
        SOM,
        SOV,
        SPA,
        SPR,
        SUD,
        SWE,
        SYR,
        TAJ,
        TAN,
        TEX,
        TIB,
        TRA,
        TRK,
        TUN,
        TUR,
        UAP,
        UAU,
        UBO,
        UCH,
        UCS,
        UER,
        UES,
        UGS,
        UIC,
        UIR,
        UKR,
        UPE,
        UPR,
        UPS,
        URO,
        URU,
        USA,
        USN,
        UTC,
        UTL,
        UTO,
        UZB,
        VEN,
        VIC,
        VIE,
        WLL,
        YEM,
        YUG,
        U00,
        U01,
        U02,
        U03,
        U04,
        U05,
        U06,
        U07,
        U08,
        U09,
        U10,
        U11,
        U12,
        U13,
        U14,
        U15,
        U16,
        U17,
        U18,
        U19,
        U20,
        U21,
        U22,
        U23,
        U24,
        U25,
        U26,
        U27,
        U28,
        U29,
        U30,
        U31,
        U32,
        U33,
        U34,
        U35,
        U36,
        U37,
        U38,
        U39,
        U40,
        U41,
        U42,
        U43,
        U44,
        U45,
        U46,
        U47,
        U48,
        U49,
        U50,
        U51,
        U52,
        U53,
        U54,
        U55,
        U56,
        U57,
        U58,
        U59,
        U60,
        U61,
        U62,
        U63,
        U64,
        U65,
        U66,
        U67,
        U68,
        U69,
        U70,
        U71,
        U72,
        U73,
        U74,
        U75,
        U76,
        U77,
        U78,
        U79,
        U80,
        U81,
        U82,
        U83,
        U84,
        U85,
        U86,
        U87,
        U88,
        U89,
        U90,
        U91,
        U92,
        U93,
        U94,
        U95,
        U96,
        U97,
        U98,
        U99,

        // AoD only
        UA0,
        UA1,
        UA2,
        UA3,
        UA4,
        UA5,
        UA6,
        UA7,
        UA8,
        UA9,
        UB0,
        UB1,
        UB2,
        UB3,
        UB4,
        UB5,
        UB6,
        UB7,
        UB8,
        UB9,
        UC0,
        UC1,
        UC2,
        UC3,
        UC4,
        UC5,
        UC6,
        UC7,
        UC8,
        UC9,
        UD0,
        UD1,
        UD2,
        UD3,
        UD4,
        UD5,
        UD6,
        UD7,
        UD8,
        UD9,
        UE0,
        UE1,
        UE2,
        UE3,
        UE4,
        UE5,
        UE6,
        UE7,
        UE8,
        UE9,
        UF0,
        UF1,
        UF2,
        UF3,
        UF4,
        UF5,
        UF6,
        UF7,
        UF8,
        UF9
        // ReSharper restore Inconsistent Naming
    }
}
