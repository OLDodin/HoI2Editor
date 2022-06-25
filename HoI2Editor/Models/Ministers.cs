using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
    ///     Ministerial data group
    /// </summary>
    public static class Ministers
    {
        #region Public properties

        /// <summary>
        ///     Master Minister List
        /// </summary>
        public static List<Minister> Items { get; }

        /// <summary>
        ///     Correspondence between country tag and ministerial file name
        /// </summary>
        public static Dictionary<Country, string> FileNameMap { get; }

        /// <summary>
        ///     Already used ID list
        /// </summary>
        public static HashSet<int> IdSet { get; }

        /// <summary>
        ///     List of ministerial characteristics
        /// </summary>
        public static MinisterPersonalityInfo[] Personalities { get; private set; }

        /// <summary>
        ///     Correspondence between ministerial status and characteristics
        /// </summary>
        public static List<int>[] PositionPersonalityTable { get; }

        /// <summary>
        ///     Loyalty name
        /// </summary>
        public static string[] LoyaltyNames { get; }

        #endregion

        #region Internal field

        /// <summary>
        ///     With the ministerial status string ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, MinisterPosition> PositionStringMap =
            new Dictionary<string, MinisterPosition>();

        /// <summary>
        ///     With the ministerial characteristic character string ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, int> PersonalityStringMap = new Dictionary<string, int>();

        /// <summary>
        ///     With ideological strings ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, MinisterIdeology> IdeologyStringMap =
            new Dictionary<string, MinisterIdeology>();

        /// <summary>
        ///     Loyalty string and ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, MinisterLoyalty> LoyaltyStringMap =
            new Dictionary<string, MinisterLoyalty>();

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
        ///     Edited flag
        /// </summary>
        private static readonly bool[] DirtyFlags = new bool[Enum.GetValues(typeof (Country)).Length];

        /// <summary>
        ///     Edited flag in ministerial list file
        /// </summary>
        private static bool _dirtyListFlag;

        #endregion

        #region Public constant

        /// <summary>
        ///     Ministerial status name
        /// </summary>
        public static readonly TextId[] PositionNames =
        {
            TextId.Empty,
            TextId.MinisterHeadOfState,
            TextId.MinisterHeadOfGovernment,
            TextId.MinisterForeignMinister,
            TextId.MinisterArmamentMinister,
            TextId.MinisterMinisterOfSecurity,
            TextId.MinisterMinisterOfIntelligence,
            TextId.MinisterChiefOfStaff,
            TextId.MinisterChiefOfArmy,
            TextId.MinisterChiefOfNavy,
            TextId.MinisterChiefOfAir
        };

        /// <summary>
        ///     Ideology name
        /// </summary>
        public static readonly TextId[] IdeologyNames =
        {
            TextId.Empty,
            TextId.IdeologyNationalSocialist,
            TextId.IdeologyFascist,
            TextId.IdeologyPaternalAutocrat,
            TextId.IdeologySocialConservative,
            TextId.IdeologyMarketLiberal,
            TextId.IdeologySocialLiberal,
            TextId.IdeologySocialDemocrat,
            TextId.IdeologyLeftWingRadical,
            TextId.IdeologyLeninist,
            TextId.IdeologyStalinist
        };

        #endregion

        #region Internal constant

        /// <summary>
        ///     Ministerial status string
        /// </summary>
        private static readonly string[] PositionStrings =
        {
            "",
            "Head of State",
            "Head of Government",
            "Foreign Minister",
            "Minister of Armament",
            "Minister of Security",
            "Head of Military Intelligence",
            "Chief of Staff",
            "Chief of Army",
            "Chief of Navy",
            "Chief of Air Force"
        };

        /// <summary>
        ///     Ministerial characteristic string (HoI2)
        /// </summary>
        private static readonly string[] PersonalityStringsHoI2 =
        {
            "Undistinguished Suit",
            "Autocratic Charmer",
            "Barking Buffoon",
            "Benevolent Gentleman",
            "Die-hard Reformer",
            "Insignificant Layman",
            "Pig-headed Isolationist",
            "Popular Figurehead",
            "Powerhungry Demagogue",
            "Resigned Generalissimo",
            "Ruthless Powermonger",
            "Stern Imperialist",
            "Weary Stiffneck",
            "Ambitious Union Boss",
            "Backroom Backstabber",
            "Corporate Suit",
            "Flamboyant Tough Guy",
            "Happy Amateur",
            "Naive Optimist",
            "Old Admiral",
            "Old Air Marshal",
            "Old General",
            "Political Protege",
            "Silent Workhorse",
            "Smiling Oilman",
            "Apologetic Clerk",
            "Biased Intellectual",
            "Ideological Crusader",
            "Iron Fisted Brute",
            "General Staffer",
            "Great Compromiser",
            "The Cloak N Dagger Schemer",
            "Administrative Genius",
            "Air Superiority Proponent",
            "Battle Fleet Proponent",
            "Resource Industrialist",
            "Laissez-Faire Capitalist",
            "Theoretical Scientist",
            "Military Entrepreneur",
            "Submarine Proponent",
            "Tank Proponent",
            "Infantry Proponent",
            "Corrupt Kleptocrat",
            "Air to Ground Proponent",
            "Air to Sea Proponent",
            "Strategic Air Proponent",
            "Back Stabber",
            "Compassionate Gentleman",
            "Crime Fighter",
            "Crooked Kleptocrat",
            "Efficient Sociopath",
            "Man of the People",
            "Prince of Terror",
            "Silent Lawyer",
            "Dismal Enigma",
            "Industrial Specialist",
            "Logistics Specialist",
            "Naval Intelligence Specialist",
            "Political Specialist",
            "Technical Specialist",
            "School of Defence",
            "School of Fire Support",
            "School of Mass Combat",
            "School of Manoeuvre",
            "School of Psychology",
            "Armoured Spearhead Doctrine",
            "Decisive Battle Doctrine",
            "Elastic Defence Doctrine",
            "Guns and Butter Doctrine",
            "Static Defence Doctrine",
            "Base Control Doctrine",
            "Decisive Naval Battle Doctrine",
            "Indirect Approach Doctrine",
            "Open Seas Doctrine",
            "Power Projection Doctrine",
            "Air Superiority Doctrine",
            "Army Aviation Doctrine",
            "Carpet Bombing Doctrine",
            "Naval Aviation Doctrine",
            "Vertical Envelopment Doctrine"
        };

        /// <summary>
        ///     Ministerial characteristic name (HoI2)
        /// </summary>
        private static readonly string[] PersonalityNamesHoI2 =
        {
            "NPERSONALITY_UNDISTINGUISHED_SUIT",
            "NPERSONALITY_AUTOCRATIC_CHARMER",
            "NPERSONALITY_BARKING_BUFFOON",
            "NPERSONALITY_BENEVOLENT_GENTLEMAN",
            "NPERSONALITY_DIE_HARD_REFORMER",
            "NPERSONALITY_INSIGNIFICANT_LAYMAN",
            "NPERSONALITY_PIG_HEADED_ISOLATIONIST",
            "NPERSONALITY_POPULAR_FIGUREHEAD",
            "NPERSONALITY_POWER_HUNGRY_DEMAGOGUE",
            "NPERSONALITY_RESIGNED_GENERALISSIMO",
            "NPERSONALITY_RUTHLESS_POWERMONGER",
            "NPERSONALITY_STERN_IMPERIALIST",
            "NPERSONALITY_WEARY_STIFF_NECK",
            "NPERSONALITY_AMBITIOUS_UNION_BOSS",
            "NPERSONALITY_BACKROOM_BACKSTABBER",
            "NPERSONALITY_CORPORATE_SUIT",
            "NPERSONALITY_FLAMBOYANT_TOUGH_GUY",
            "NPERSONALITY_HAPPY_AMATEUR",
            "NPERSONALITY_NAIVE_OPTIMIST",
            "NPERSONALITY_OLD_ADMIRAL",
            "NPERSONALITY_OLD_AIR_MARSHAL",
            "NPERSONALITY_OLD_GENERAL",
            "NPERSONALITY_POLITICAL_PROTEGE",
            "NPERSONALITY_SILENT_WORKHORSE",
            "NPERSONALITY_SMILING_OILMAN",
            "NPERSONALITY_APOLOGETIC_CLERK",
            "NPERSONALITY_BIASED_INTELLECTUAL",
            "NPERSONALITY_IDEOLOGICAL_CRUSADER",
            "NPERSONALITY_IRON_FISTED_BRUTE",
            "NPERSONALITY_GENERAL_STAFFER",
            "NPERSONALITY_GREAT_COMPROMISER",
            "NPERSONALITY_THE_CLOAK_N_DAGGER_SCHEMER",
            "NPERSONALITY_ADMINISTRATIVE_GENIUS",
            "NPERSONALITY_AIR_SUPERIORITY_PROPONENT",
            "NPERSONALITY_BATTLE_FLEET_PROPONENT",
            "NPERSONALITY_RESOURCE_INDUSTRIALIST",
            "NPERSONALITY_LAISSEZ_FAIRES_CAPITALIST",
            "NPERSONALITY_THEORETICAL_SCIENTIST",
            "NPERSONALITY_MILITARY_ENTREPRENEUR",
            "NPERSONALITY_SUBMARINE_PROPONENT",
            "NPERSONALITY_TANK_PROPONENT",
            "NPERSONALITY_INFANTRY_PROPONENT",
            "NPERSONALITY_CORRUPT_KLEPTOCRAT",
            "NPERSONALITY_AIR_TO_GROUND_PROPONENT",
            "NPERSONALITY_AIR_TO_SEA_PROPONENT",
            "NPERSONALITY_STRATEGIC_AIR_PROPONENT",
            "NPERSONALITY_BACK_STABBER",
            "NPERSONALITY_COMPASSIONATE_GENTLEMAN",
            "NPERSONALITY_CRIME_FIGHTER",
            "NPERSONALITY_CROOKED_KLEPTOCRAT",
            "NPERSONALITY_EFFICIENT_SOCIOPATH",
            "NPERSONALITY_MAN_OF_THE_PEOPLE",
            "NPERSONALITY_PRINCE_OF_TERROR",
            "NPERSONALITY_SILENT_LAWYER",
            "NPERSONALITY_DISMAL_ENIGMA",
            "NPERSONALITY_INDUSTRIAL_SPECIALIST",
            "NPERSONALITY_LOGISTICS_SPECIALIST",
            "NPERSONALITY_NAVAL_INTELLIGENCE_SPECIALIST",
            "NPERSONALITY_POLITICAL_SPECIALIST",
            "NPERSONALITY_TECHNICAL_SPECIALIST",
            "NPERSONALITY_SCHOOL_OF_DEFENCE",
            "NPERSONALITY_SCHOOL_OF_FIRE_SUPPORT",
            "NPERSONALITY_SCHOOL_OF_MASS_COMBAT",
            "NPERSONALITY_SCHOOL_OF_MANOEUVRE",
            "NPERSONALITY_SCHOOL_OF_PSYCHOLOGY",
            "NPERSONALITY_ARMOURED_SPEARHEAD_DOCTRINE",
            "NPERSONALITY_DECISIVE_BATTLE_DOCTRINE",
            "NPERSONALITY_ELASTIC_DEFENCE_DOCTRINE",
            "NPERSONALITY_GUNS_AND_BUTTER_DOCTRINE",
            "NPERSONALITY_STATIC_DEFENCE_DOCTRINE",
            "NPERSONALITY_BASE_CONTROL_DOCTRINE",
            "NPERSONALITY_DECISIVE_BATTLE_DOCTRINE2",
            "NPERSONALITY_INDIRECT_APPROACH_DOCTRINE",
            "NPERSONALITY_OPEN_SEAS_DOCTRINE",
            "NPERSONALITY_POWER_PROJECTION_DOCTRINE",
            "NPERSONALITY_AIR_SUPERIORITY_DOCTRINE",
            "NPERSONALITY_ARMY_AVIATION_DOCTRINE",
            "NPERSONALITY_CARPET_BOMBING_DOCTRINE",
            "NPERSONALITY_NAVAL_AVIATION_DOCTRINE",
            "NPERSONALITY_VERTICAL_ENVELOPMENT_DOCTRINE"
        };

        /// <summary>
        ///     Characteristics of the head of state (HoI2)
        /// </summary>
        private static readonly int[] HeadOfStatePersonalitiesHoI2 =
        {
            (int) MinisterPersonalityHoI2.AutocraticCharmer,
            (int) MinisterPersonalityHoI2.BarkingBuffoon,
            (int) MinisterPersonalityHoI2.BenevolentGentleman,
            (int) MinisterPersonalityHoI2.DieHardReformer,
            (int) MinisterPersonalityHoI2.InsignificantLayman,
            (int) MinisterPersonalityHoI2.PigHeadedIsolationist,
            (int) MinisterPersonalityHoI2.PopularFigurehead,
            (int) MinisterPersonalityHoI2.PowerHungryDemagogue,
            (int) MinisterPersonalityHoI2.ResignedGeneralissimo,
            (int) MinisterPersonalityHoI2.RuthlessPowermonger,
            (int) MinisterPersonalityHoI2.SternImperalist,
            (int) MinisterPersonalityHoI2.WearyStiffNeck,
            (int) MinisterPersonalityHoI2.UndistinguishedSuit
        };

        /// <summary>
        ///     Characteristics of government leaders (HoI2)
        /// </summary>
        private static readonly int[] HeadOfGovernmentPersonalitiesHoI2 =
        {
            (int) MinisterPersonalityHoI2.AmbitiousUnionBoss,
            (int) MinisterPersonalityHoI2.BackroomBackstabber,
            (int) MinisterPersonalityHoI2.CorporateSuit,
            (int) MinisterPersonalityHoI2.FlamboyantToughGuy,
            (int) MinisterPersonalityHoI2.HappyAmateur,
            (int) MinisterPersonalityHoI2.NaiveOptimist,
            (int) MinisterPersonalityHoI2.OldAdmiral,
            (int) MinisterPersonalityHoI2.OldAirMarshal,
            (int) MinisterPersonalityHoI2.OldGeneral,
            (int) MinisterPersonalityHoI2.PoliticalProtege,
            (int) MinisterPersonalityHoI2.SilentWorkhorse,
            (int) MinisterPersonalityHoI2.SmilingOilman,
            (int) MinisterPersonalityHoI2.UndistinguishedSuit
        };

        /// <summary>
        ///     Characteristics of the Minister of Foreign Affairs (HoI2)
        /// </summary>
        private static readonly int[] ForeignMinisterPersonalitiesHoI2 =
        {
            (int) MinisterPersonalityHoI2.ApologeticClerk,
            (int) MinisterPersonalityHoI2.BiasedIntellectual,
            (int) MinisterPersonalityHoI2.IdeologyCrusader,
            (int) MinisterPersonalityHoI2.IronFistedBrute,
            (int) MinisterPersonalityHoI2.GeneralStaffer,
            (int) MinisterPersonalityHoI2.GreatCompromiser,
            (int) MinisterPersonalityHoI2.TheCloakNDaggerSchemer,
            (int) MinisterPersonalityHoI2.UndistinguishedSuit
        };

        /// <summary>
        ///     Characteristics of the Minister of Military Demand (HoI2)
        /// </summary>
        private static readonly int[] MinisterOfArmamentPersonalitiesHoI2 =
        {
            (int) MinisterPersonalityHoI2.AdministrativeGenius,
            (int) MinisterPersonalityHoI2.AirSuperiorityProponent,
            (int) MinisterPersonalityHoI2.BattleFleetProponent,
            (int) MinisterPersonalityHoI2.ResourceIndustrialist,
            (int) MinisterPersonalityHoI2.LaissezFaireCapitalist,
            (int) MinisterPersonalityHoI2.TheoreticalScientist,
            (int) MinisterPersonalityHoI2.MilitaryEnterpreneur,
            (int) MinisterPersonalityHoI2.SubmarineProponent,
            (int) MinisterPersonalityHoI2.TankProponent,
            (int) MinisterPersonalityHoI2.InfantryProponent,
            (int) MinisterPersonalityHoI2.CorruptKleptocrat,
            (int) MinisterPersonalityHoI2.AirToGroundProponent,
            (int) MinisterPersonalityHoI2.AirToSeaProponent,
            (int) MinisterPersonalityHoI2.StrategicAirProponent,
            (int) MinisterPersonalityHoI2.UndistinguishedSuit
        };

        /// <summary>
        ///     Characteristics of the Minister of Interior (HoI2)
        /// </summary>
        private static readonly int[] MinisterOfSecurityPersonalitiesHoI2 =
        {
            (int) MinisterPersonalityHoI2.BackStabber,
            (int) MinisterPersonalityHoI2.CompassionateGentleman,
            (int) MinisterPersonalityHoI2.CrimeFighter,
            (int) MinisterPersonalityHoI2.CrookedKleptocrat,
            (int) MinisterPersonalityHoI2.EfficientSociopath,
            (int) MinisterPersonalityHoI2.ManOfThePeople,
            (int) MinisterPersonalityHoI2.PrinceOfTerror,
            (int) MinisterPersonalityHoI2.SilentLawyer,
            (int) MinisterPersonalityHoI2.UndistinguishedSuit
        };

        /// <summary>
        ///     Characteristics of the Minister of Information (HoI2)
        /// </summary>
        private static readonly int[] HeadOfMilitaryIntelligencePersonalitiesHoI2 =
        {
            (int) MinisterPersonalityHoI2.DismalEnigma,
            (int) MinisterPersonalityHoI2.IndustrialSpecialist,
            (int) MinisterPersonalityHoI2.LogisticsSpecialist,
            (int) MinisterPersonalityHoI2.NavalIntelligenceSpecialist,
            (int) MinisterPersonalityHoI2.PoliticalSpecialist,
            (int) MinisterPersonalityHoI2.TechnicalSpecialist,
            (int) MinisterPersonalityHoI2.UndistinguishedSuit
        };

        /// <summary>
        ///     Characteristics of the Integrated Chief of Staff (HoI2)
        /// </summary>
        private static readonly int[] ChiefOfStaffPersonalitiesHoI2 =
        {
            (int) MinisterPersonalityHoI2.SchoolOfDefence,
            (int) MinisterPersonalityHoI2.SchoolOfFireSupport,
            (int) MinisterPersonalityHoI2.SchoolOfMassCombat,
            (int) MinisterPersonalityHoI2.SchoolOfManeuvre,
            (int) MinisterPersonalityHoI2.SchoolOfPsychology,
            (int) MinisterPersonalityHoI2.UndistinguishedSuit
        };

        /// <summary>
        ///     Characteristics of Army General Commander (HoI2)
        /// </summary>
        private static readonly int[] ChiefOfArmyPersonalitiesHoI2 =
        {
            (int) MinisterPersonalityHoI2.ArmouredSpearheadDoctrine,
            (int) MinisterPersonalityHoI2.DecisiveBattleDoctrine,
            (int) MinisterPersonalityHoI2.ElasticDefenceDoctrine,
            (int) MinisterPersonalityHoI2.GunsAndButterDoctrine,
            (int) MinisterPersonalityHoI2.StaticDefenceDoctrine,
            (int) MinisterPersonalityHoI2.UndistinguishedSuit
        };

        /// <summary>
        ///     Characteristics of Navy Commander (HoI2)
        /// </summary>
        private static readonly int[] ChiefOfNavyPersonalitiesHoI2 =
        {
            (int) MinisterPersonalityHoI2.BaseControlDoctrine,
            (int) MinisterPersonalityHoI2.DecisiveNavalBattleDoctrine,
            (int) MinisterPersonalityHoI2.IndirectApproachDoctrine,
            (int) MinisterPersonalityHoI2.OpenSeasDoctrine,
            (int) MinisterPersonalityHoI2.PowerProjectionDoctrine,
            (int) MinisterPersonalityHoI2.UndistinguishedSuit
        };

        /// <summary>
        ///     Characteristics of Air Force Commander (HoI2)
        /// </summary>
        private static readonly int[] ChiefOfAirForcePersonalitiesHoI2 =
        {
            (int) MinisterPersonalityHoI2.AirSuperiorityDoctrine,
            (int) MinisterPersonalityHoI2.ArmyAviationDoctrine,
            (int) MinisterPersonalityHoI2.CarpetBombingDoctrine,
            (int) MinisterPersonalityHoI2.NavalAviationDoctrine,
            (int) MinisterPersonalityHoI2.VerticalEnvelopmentDoctrine,
            (int) MinisterPersonalityHoI2.UndistinguishedSuit
        };

        /// <summary>
        ///     Correspondence between ministerial status and characteristics (HoI2)
        /// </summary>
        private static readonly int[][] PositionPersonalityTableHoI2 =
        {
            null,
            HeadOfStatePersonalitiesHoI2,
            HeadOfGovernmentPersonalitiesHoI2,
            ForeignMinisterPersonalitiesHoI2,
            MinisterOfArmamentPersonalitiesHoI2,
            MinisterOfSecurityPersonalitiesHoI2,
            HeadOfMilitaryIntelligencePersonalitiesHoI2,
            ChiefOfStaffPersonalitiesHoI2,
            ChiefOfArmyPersonalitiesHoI2,
            ChiefOfNavyPersonalitiesHoI2,
            ChiefOfAirForcePersonalitiesHoI2
        };

        /// <summary>
        ///     Ideology string
        /// </summary>
        private static readonly string[] IdeologyStrings =
        {
            "",
            "NS",
            "FA",
            "PA",
            "SC",
            "ML",
            "SL",
            "SD",
            "LWR",
            "LE",
            "ST"
        };

        /// <summary>
        ///     Loyalty string
        /// </summary>
        private static readonly string[] LoyaltyStrings =
        {
            "",
            "Very High",
            "High",
            "Medium",
            "Low",
            "Very Low",
            "Undying",
            "NA"
        };

        /// <summary>
        ///     Special case of conversion to the first capital of the cabinet characteristic character string
        /// </summary>
        private static readonly Dictionary<string, string> PersonalityStringCaseMap
            = new Dictionary<string, string>
            {
                { "die-hard reformer", "Die-hard Reformer" },
                { "pig-headed isolationist", "Pig-headed Isolationist" },
                { "air to ground proponent", "Air to Ground Proponent" },
                { "air to sea proponent", "Air to Sea Proponent" },
                { "man of the people", "Man of the People" },
                { "prince of terror", "Prince of Terror" },
                { "school of defence", "School of Defence" },
                { "school of fire support", "School of Fire Support" },
                { "school of mass combat", "School of Mass Combat" },
                { "school of manoeuvre", "School of Manoeuvre" },
                { "school of psychology", "School of Psychology" },
                { "guns and butter doctrine", "Guns and Butter Doctrine" },
                { "health and safety", "Health and Safety" },
                { "doctrine of autonomy", "Doctrine of Autonomy" },
                { "ger_mil_m1", "ger_mil_m1" },
                { "ger_mil_m2", "ger_mil_m2" },
                { "ger_mil_m3", "ger_mil_m3" },
                { "ger_mil_m4", "ger_mil_m4" },
                { "ger_mil_m5", "ger_mil_m5" },
                { "ger_mil_m6", "ger_mil_m6" },
                { "ger_mil_m7", "ger_mil_m7" },
                { "ger_mil_m8", "ger_mil_m8" },
                { "ger_mil_m9", "ger_mil_m9" },
                { "ger_mil_m10", "ger_mil_m10" },
                { "ger_mil_m11", "ger_mil_m11" },
                { "brit_nav_mis", "brit_nav_mis" },
                { "ss reichsfuhrer", "SS Reichsfuhrer" },
                { "salesman of deception", "Salesman of Deception" },
                { "master of propaganda", "Master of Propaganda" },
                { "undersecretary of war", "Undersecretary of War" },
                { "persuader of democracies", "Persuader of Democracies" },
                { "father of united nations", "Father of United Nations" },
                { "director of fbi", "Director of FBI" },
                { "secretary of war", "Secretary of War" },
                { "ambassador to un", "Ambassador to UN" },
                { "secretary of the interior", "Secretary of the Interior" },
                { "supporter of devaluation", "Supporter of Devaluation" },
                { "opposer of the far right", "Opposer of the Far Right" },
                { "supporter of friendly relations", "Supporter of Friendly Relations" },
                { "opposer to military spending", "Opposer to Military Spending" }
            };

        /// <summary>
        ///     Correlation of common spelling mistakes in ministerial characteristic strings with correct values
        /// </summary>
        private static readonly Dictionary<string, string> PersonalityStringTypoMap
            = new Dictionary<string, string>
            {
                { "barking buffon", "barking buffoon" },
                { "iron-fisted brute", "iron fisted brute" },
                { "the cloak-n-dagger schemer", "the cloak n dagger schemer" },
                { "cloak-n-dagger schemer", "the cloak n dagger schemer" },
                { "cloak n dagger schemer", "the cloak n dagger schemer" },
                { "laissez-faires capitalist", "laissez-faire capitalist" },
                { "laissez faires capitalist", "laissez-faire capitalist" },
                { "laissez faire capitalist", "laissez-faire capitalist" },
                { "military entrepeneur", "military entrepreneur" },
                { "crooked plutocrat", "crooked kleptocrat" },
                { "school of defense", "school of defence" },
                { "school of maneouvre", "school of manoeuvre" },
                { "elastic defense doctrine", "elastic defence doctrine" },
                { "static defense doctrine", "static defence doctrine" },
                { "vertical envelopement doctrine", "vertical envelopment doctrine" }
            };

        /// <summary>
        ///     Ministerial characteristics (HoI2)
        /// </summary>
        private enum MinisterPersonalityHoI2
        {
            // General purpose
            UndistinguishedSuit, // Mediocre politician

            // National leader
            AutocraticCharmer, // One-man politician
            BarkingBuffoon, // A clown with only a mouth
            BenevolentGentleman, // Noble gentleman
            DieHardReformer, // Indomitable reformer
            InsignificantLayman, // Incompetent ordinary person
            PigHeadedIsolationist, // Hard-lined isolationist
            PopularFigurehead, // Form-only leader
            PowerHungryDemagogue, // Power-hungry instigator
            ResignedGeneralissimo, // Retired Marshal
            RuthlessPowermonger, // Dead in power
            SternImperalist, // Strict imperialist
            WearyStiffNeck, // Cowardly stubborn

            // Government leaders
            AmbitiousUnionBoss, // Ambitious former trade union representative
            BackroomBackstabber, // Master of behind-the-scenes work
            CorporateSuit, // Former businessman
            FlamboyantToughGuy, // Flashy tough guy
            HappyAmateur, // Lucky amateur
            NaiveOptimist, // Naive optimist
            OldAdmiral, // Former Navy General
            OldAirMarshal, // Former Air Force General
            OldGeneral, // Former Army General
            PoliticalProtege, // The waist of a powerful person
            SilentWorkhorse, // Silent diligent
            SmilingOilman, // Smiley oil king

            // Minister of Foreign Affairs
            ApologeticClerk, // Mind official
            BiasedIntellectual, // A knowledgeable person with prejudice
            IdeologyCrusader, // Ideological warrior
            IronFistedBrute, // Ruthless
            GeneralStaffer, // Staff type
            GreatCompromiser, // Master of mediation
            TheCloakNDaggerSchemer, // Plotter

            // Minister of Military Demand
            AdministrativeGenius, // Genius practitioner
            AirSuperiorityProponent, // Emphasis on air control rights
            BattleFleetProponent, // Fleet emphasis
            ResourceIndustrialist, // Mining entrepreneur type
            LaissezFaireCapitalist, // Laissez-faire
            TheoreticalScientist, // Theoretical scientist
            MilitaryEnterpreneur, // Experienced hero
            SubmarineProponent, // Focus on submarines
            TankProponent, // Focus on tanks
            InfantryProponent, // Focus on infantry
            CorruptKleptocrat, // Thief politician
            AirToGroundProponent, // Emphasis on air-to-ground combat
            AirToSeaProponent, // Emphasis on air-to-sea combat
            StrategicAirProponent, // Focus on strategic bombing

            // Minister of Interior
            BackStabber, // Master of behind-the-scenes work
            CompassionateGentleman, // Compassionate gentleman
            CrimeFighter, // Focus on security
            CrookedKleptocrat, // Rogue politician
            EfficientSociopath, // Antisocial efficiencyist
            ManOfThePeople, // Allies of the common people
            PrinceOfTerror, // Promoter of fear politics
            SilentLawyer, // Quiet lawyer

            // Minister of Information
            DismalEnigma, // Creepy mysterious person
            IndustrialSpecialist, // Industrial analysis expert
            LogisticsSpecialist, // Specialist in military analysis
            NavalIntelligenceSpecialist, // Navy information expert
            PoliticalSpecialist, // Political analysis expert
            TechnicalSpecialist, // Technical analysis expert

            // Chief of the Defense Staff
            SchoolOfDefence, // Defenseist
            SchoolOfFireSupport, // Thermal support theorist
            SchoolOfMassCombat, // Human wave tacticalist
            SchoolOfManeuvre, // Mobileist
            SchoolOfPsychology, // Psychologist

            // Army General Commander
            ArmouredSpearheadDoctrine, // Armored assault doctrine
            DecisiveBattleDoctrine, // Decisive battle doctrine (( Army )
            ElasticDefenceDoctrine, // Elastic defense doctrine
            GunsAndButterDoctrine, // Armed priority doctrine
            StaticDefenceDoctrine, // Static defense doctrine

            // Navy Commander
            BaseControlDoctrine, // Base domination doctrine
            DecisiveNavalBattleDoctrine, // Decisive battle doctrine (( Navy )
            IndirectApproachDoctrine, // Indirect approach doctrine
            OpenSeasDoctrine, // Sotokai Doctorin
            PowerProjectionDoctrine, // Force input doctrine

            // Air Force Commander
            AirSuperiorityDoctrine, // Air Control Doctorin
            ArmyAviationDoctrine, // Army support doctrine
            CarpetBombingDoctrine, // Carpet bombing doctrine
            NavalAviationDoctrine, // Navy support doctrine
            VerticalEnvelopmentDoctrine // Three-dimensional concentrated attack doctrine
        }

        #endregion

        #region Initialization

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Ministers()
        {
            // Master Minister List
            Items = new List<Minister>();

            // Correspondence between country tags and ministerial file names
            FileNameMap = new Dictionary<Country, string>();

            // Already used ID list
            IdSet = new HashSet<int>();

            // Correspondence between ministerial status and characteristics
            PositionPersonalityTable = new List<int>[Enum.GetValues(typeof (MinisterPosition)).Length];

            // Ministerial status
            foreach (MinisterPosition position in Enum.GetValues(typeof (MinisterPosition)))
            {
                PositionStringMap.Add(PositionStrings[(int) position].ToLower(), position);
            }

            // Loyalty
            LoyaltyNames = new[]
            {
                "",
                Resources.LoyaltyVeryHigh,
                Resources.LoyaltyHigh,
                Resources.LoyaltyMedium,
                Resources.LoyaltyLow,
                Resources.LoyaltyVeryLow,
                Resources.LoyaltyUndying,
                Resources.LoyaltyNA
            };
            foreach (MinisterLoyalty loyalty in Enum.GetValues(typeof (MinisterLoyalty)))
            {
                LoyaltyStringMap.Add(LoyaltyStrings[(int) loyalty].ToLower(), loyalty);
            }

            // ideology
            foreach (MinisterIdeology ideology in Enum.GetValues(typeof (MinisterIdeology)))
            {
                IdeologyStringMap.Add(IdeologyStrings[(int) ideology].ToLower(), ideology);
            }
        }

        #endregion

        #region Ministerial characteristics

        /// <summary>
        ///     Initialize ministerial characteristics
        /// </summary>
        public static void InitPersonality()
        {
            // Back if loaded
            if (_loaded)
            {
                return;
            }

            // Initialize ministerial characteristics
            switch (Game.Type)
            {
                case GameType.HeartsOfIron2:
                    // Initialize ministerial characteristics
                    InitPeronalityHoI2();
                    break;

                case GameType.ArsenalOfDemocracy:
                    // Read the ministerial characteristic definition file
                    LoadPersonalityAoD();
                    break;

                case GameType.DarkestHour:
                    // Read the ministerial characteristic definition file
                    LoadPersonalityDh();
                    break;
            }
        }

        /// <summary>
        ///     Initialize ministerial characteristics (HoI2)
        /// </summary>
        private static void InitPeronalityHoI2()
        {
            int positionCount = Enum.GetValues(typeof (MinisterPosition)).Length;
            int personalityCount = Enum.GetValues(typeof (MinisterPersonalityHoI2)).Length;

            // Initialize ministerial characteristic information
            Personalities = new MinisterPersonalityInfo[personalityCount];
            PersonalityStringMap.Clear();
            for (int i = 0; i < personalityCount; i++)
            {
                MinisterPersonalityInfo info = new MinisterPersonalityInfo
                {
                    String = PersonalityStringsHoI2[i],
                    Name = PersonalityNamesHoI2[i]
                };
                Personalities[i] = info;
                PersonalityStringMap.Add(info.String.ToLower(), i);
            }

            // Initialize the correspondence between ministerial status and ministerial characteristics
            for (int i = 0; i < positionCount; i++)
            {
                // MinisterPosition.None Do nothing to
                if (PositionPersonalityTableHoI2[i] == null)
                {
                    continue;
                }

                PositionPersonalityTable[i] = PositionPersonalityTableHoI2[i].ToList();
                foreach (int j in PositionPersonalityTable[i])
                {
                    Personalities[j].Position[i] = true;
                }
            }
        }

        /// <summary>
        ///     Read ministerial characteristics (AoD)
        /// </summary>
        private static void LoadPersonalityAoD()
        {
            PersonalityStringMap.Clear();
            for (int i = 0; i < Enum.GetValues(typeof (MinisterPosition)).Length; i++)
            {
                PositionPersonalityTable[i] = new List<int>();
            }

            // Read ministerial characteristics file
            List<MinisterPersonalityInfo> list =
                MinisterModifierParser.Parse(Game.GetReadFileName(Game.MinisterPersonalityPathNameAoD));
            Personalities = list.ToArray();

            // Initialize the related table
            for (int i = 0; i < Personalities.Length; i++)
            {
                string s = Personalities[i].String.ToLower();
                if (PersonalityStringMap.ContainsKey(s))
                {
                    Log.Warning($"[Minister] Duplicated personality strings: {Personalities[i].String}");
                }
                PersonalityStringMap[s] = i;
                Personalities[i].String = GetCasePersonalityString(Personalities[i].String.ToLower());
                for (int j = 0; j < Enum.GetValues(typeof (MinisterPosition)).Length; j++)
                {
                    if (Personalities[i].Position[j])
                    {
                        PositionPersonalityTable[j].Add(i);
                    }
                }
            }
        }

        /// <summary>
        ///     Read ministerial characteristics (DH)
        /// </summary>
        private static void LoadPersonalityDh()
        {
            PersonalityStringMap.Clear();
            for (int i = 0; i < Enum.GetValues(typeof (MinisterPosition)).Length; i++)
            {
                PositionPersonalityTable[i] = new List<int>();
            }

            // Read ministerial characteristics file
            List<MinisterPersonalityInfo> list =
                MinisterPersonalityParser.Parse(Game.GetReadFileName(Game.MinisterPersonalityPathNameDh));
            Personalities = list.ToArray();

            // Initialize the related table
            for (int i = 0; i < Personalities.Length; i++)
            {
                string s = Personalities[i].String.ToLower();
                if (PersonalityStringMap.ContainsKey(s))
                {
                    Log.Warning($"[Minister] Duplicated personality strings: {Personalities[i].String}");
                }
                PersonalityStringMap[s] = i;
                Personalities[i].String = GetCasePersonalityString(Personalities[i].String.ToLower());
                for (int j = 0; j < Enum.GetValues(typeof (MinisterPosition)).Length; j++)
                {
                    if (Personalities[i].Position[j])
                    {
                        PositionPersonalityTable[j].Add(i);
                    }
                }
            }
        }

        /// <summary>
        ///     Converts a cabinet characteristic string to uppercase only at the beginning of a word
        /// </summary>
        /// <param name="s">Ministerial characteristic character string</param>
        /// <returns>Converted string</returns>
        private static string GetCasePersonalityString(string s)
        {
            // If there are special conversion rules
            if (PersonalityStringCaseMap.ContainsKey(s))
            {
                return PersonalityStringCaseMap[s];
            }

            // Convert only word strings to uppercase
            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
            return textInfo.ToTitleCase(s);
        }

        #endregion

        #region File reading

        /// <summary>
        ///     Request a reload of the ministerial file
        /// </summary>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     Reload ministerial files
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
        ///     Read ministerial files
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
        ///     Delayed loading of ministerial files
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
        ///     Read ministerial files
        /// </summary>
        private static void LoadFiles()
        {
            Items.Clear();
            IdSet.Clear();
            FileNameMap.Clear();

            switch (Game.Type)
            {
                case GameType.HeartsOfIron2:
                case GameType.ArsenalOfDemocracy:
                    if (!LoadHoI2())
                    {
                        return;
                    }
                    break;

                case GameType.DarkestHour:
                    if (!LoadDh())
                    {
                        return;
                    }
                    break;
            }

            // Clear the edited flag
            _dirtyFlag = false;

            // Set the read flag
            _loaded = true;
        }

        /// <summary>
        ///     Read ministerial files (HoI2 / AoD / DH-MOD When not in use )
        /// </summary>
        /// <returns>If reading fails false false return it</returns>
        private static bool LoadHoI2()
        {
            List<string> fileList = new List<string>();
            string folderName;
            bool error = false;

            // Load ministerial files in the save folder
            if (Game.IsExportFolderActive)
            {
                folderName = Game.GetExportFileName(Game.MinisterPathName);
                if (Directory.Exists(folderName))
                {
                    foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                    {
                        try
                        {
                            // Read ministerial files
                            LoadFile(fileName);

                            // Register the read file name in the ministerial file list
                            string name = Path.GetFileName(fileName);
                            if (!string.IsNullOrEmpty(name))
                            {
                                fileList.Add(name.ToLower());
                            }
                        }
                        catch (Exception)
                        {
                            error = true;
                            Log.Error("[Minister] Read error: {0}", fileName);
                            if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                Resources.EditorMinister, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                == DialogResult.Cancel)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            // MOD Read ministerial files in a folder
            if (Game.IsModActive)
            {
                folderName = Game.GetModFileName(Game.MinisterPathName);
                if (Directory.Exists(folderName))
                {
                    foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                    {
                        try
                        {
                            // Read ministerial files
                            LoadFile(fileName);

                            // Register the read file name in the ministerial file list
                            string name = Path.GetFileName(fileName);
                            if (!string.IsNullOrEmpty(name))
                            {
                                fileList.Add(name.ToLower());
                            }
                        }
                        catch (Exception)
                        {
                            error = true;
                            Log.Error("[Minister] Read error: {0}", fileName);
                            if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                                Resources.EditorMinister, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                == DialogResult.Cancel)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            // Read ministerial files in the vanilla folder
            folderName = Path.Combine(Game.FolderName, Game.MinisterPathName);
            if (Directory.Exists(folderName))
            {
                foreach (string fileName in Directory.GetFiles(folderName, "*.csv"))
                {
                    // MOD Ignore files read in folders
                    string name = Path.GetFileName(fileName);
                    if (string.IsNullOrEmpty(name) || fileList.Contains(name.ToLower()))
                    {
                        continue;
                    }

                    try
                    {
                        // Read ministerial files
                        LoadFile(fileName);
                    }
                    catch (Exception)
                    {
                        error = true;
                        Log.Error("[Minister] Read error: {0}", fileName);
                        if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                            Resources.EditorMinister, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                            == DialogResult.Cancel)
                        {
                            return false;
                        }
                    }
                }
            }

            return !error;
        }

        /// <summary>
        ///     Read ministerial files (DH-MOD while using it )
        /// </summary>
        /// <returns>If reading fails false false return it</returns>
        private static bool LoadDh()
        {
            // If the ministerial list file does not exist, use the conventional reading method.
            string listFileName = Game.GetReadFileName(Game.DhMinisterListPathName);
            if (!File.Exists(listFileName))
            {
                return LoadHoI2();
            }

            // Read ministerial list file
            IEnumerable<string> fileList;
            try
            {
                fileList = LoadList(listFileName);
            }
            catch (Exception)
            {
                Log.Error("[Minister] Read error: {0}", listFileName);
                MessageBox.Show($"{Resources.FileReadError}: {listFileName}",
                    Resources.EditorMinister, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            bool error = false;
            foreach (string fileName in fileList.Select(name => Game.GetReadFileName(Game.MinisterPathName, name)))
            {
                try
                {
                    // Read ministerial files
                    LoadFile(fileName);
                }
                catch (Exception)
                {
                    error = true;
                    Log.Error("[Minister] Read error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                        Resources.EditorMinister, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return false;
                    }
                }
            }

            return !error;
        }

        /// <summary>
        ///     Read ministerial list file (DH)
        /// </summary>
        private static IEnumerable<string> LoadList(string fileName)
        {
            Log.Verbose("[Minister] Load: {0}", Path.GetFileName(fileName));

            List<string> list = new List<string>();
            using (StreamReader reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    // Blank line
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    // Comment line
                    if (line[0] == '#')
                    {
                        continue;
                    }

                    list.Add(line);
                }
            }
            return list;
        }

        /// <summary>
        ///     Read ministerial files
        /// </summary>
        /// <param name="fileName">Target file name</param>
        private static void LoadFile(string fileName)
        {
            Log.Verbose("[Minister] Load: {0}", Path.GetFileName(fileName));

            using (CsvLexer lexer = new CsvLexer(fileName))
            {
                // Skip empty files
                if (lexer.EndOfStream)
                {
                    return;
                }

                // Country tag reading
                string[] tokens = lexer.GetTokens();
                if (tokens == null || tokens.Length == 0 || string.IsNullOrEmpty(tokens[0]))
                {
                    return;
                }
                // Do nothing for unsupported country tags
                if (!Countries.StringMap.ContainsKey(tokens[0].ToUpper()))
                {
                    return;
                }
                Country country = Countries.StringMap[tokens[0].ToUpper()];

                // Skip files with only header lines
                if (lexer.EndOfStream)
                {
                    return;
                }

                while (!lexer.EndOfStream)
                {
                    Minister minister = ParseLine(lexer, country);

                    // Skip blank lines
                    if (minister == null)
                    {
                        continue;
                    }

                    Items.Add(minister);
                }

                ResetDirty(country);

                if (country != Country.None && !FileNameMap.ContainsKey(country))
                {
                    FileNameMap.Add(country, lexer.FileName);
                }
            }
        }

        /// <summary>
        ///     Interpret the ministerial definition line
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <param name="country">National tag</param>
        /// <returns>Ministerial data</returns>
        private static Minister ParseLine(CsvLexer lexer, Country country)
        {
            string[] tokens = lexer.GetTokens();

            // ID Skip lines that are not specified
            if (string.IsNullOrEmpty(tokens?[0]))
            {
                return null;
            }

            // Skip lines with insufficient tokens
            if (tokens.Length != (Misc.EnableRetirementYearMinisters ? 11 : (Misc.UseNewMinisterFilesFormat ? 10 : 9)))
            {
                Log.Warning("[Minister] Invalid token count: {0} ({1} L{2})", tokens.Length, lexer.FileName,
                    lexer.LineNo);
                // At the end x x There is no / / Continue analysis if there are extra items
                if (tokens.Length < (Misc.EnableRetirementYearMinisters ? 10 : (Misc.UseNewMinisterFilesFormat ? 9 : 8)))
                {
                    return null;
                }
            }

            Minister minister = new Minister { Country = country };
            int index = 0;

            // ID
            int id;
            if (!int.TryParse(tokens[index], out id))
            {
                Log.Warning("[Minister] Invalid id: {0} ({1} L{2})", tokens[index], lexer.FileName, lexer.LineNo);
                return null;
            }
            minister.Id = id;
            index++;

            // Ministerial status
            string positionName = tokens[index].ToLower();
            if (PositionStringMap.ContainsKey(positionName))
            {
                minister.Position = PositionStringMap[positionName];
            }
            else
            {
                minister.Position = MinisterPosition.None;
                Log.Warning("[Minister] Invalid position: {0} [{1}] ({2} L{3})", tokens[index], minister.Id,
                    lexer.FileName, lexer.LineNo);
            }
            index++;

            // name
            minister.Name = tokens[index];
            index++;

            // Start year
            int startYear;
            if (int.TryParse(tokens[index], out startYear))
            {
                minister.StartYear = startYear + (Misc.UseNewMinisterFilesFormat ? 0 : 1900);
            }
            else
            {
                minister.StartYear = 1936;
                Log.Warning("[Minister] Invalid start year: {0} [{1}: {2}] ({3} L{4})", tokens[index], minister.Id,
                    minister.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // End year
            if (Misc.UseNewMinisterFilesFormat)
            {
                int endYear;
                if (int.TryParse(tokens[index], out endYear))
                {
                    minister.EndYear = endYear;
                }
                else
                {
                    minister.EndYear = 1970;
                    Log.Warning("[Minister] Invalid end year: {0} [{1}: {2}] ({3} L{4})", tokens[index], minister.Id,
                        minister.Name, lexer.FileName, lexer.LineNo);
                }
                index++;
            }
            else
            {
                minister.EndYear = 1970;
            }

            // Retirement year
            if (Misc.EnableRetirementYearMinisters)
            {
                int retirementYear;
                if (int.TryParse(tokens[index], out retirementYear))
                {
                    minister.RetirementYear = retirementYear;
                }
                else
                {
                    minister.RetirementYear = 1999;
                    Log.Warning("[Minister] Invalid retirement year: {0} [{1}: {2}] ({3} L{4})", tokens[index],
                        minister.Id, minister.Name, lexer.FileName, lexer.LineNo);
                }
                index++;
            }
            else
            {
                minister.RetirementYear = 1999;
            }

            // ideology
            string ideologyName = tokens[index].ToLower();
            if (IdeologyStringMap.ContainsKey(ideologyName))
            {
                minister.Ideology = IdeologyStringMap[ideologyName];
            }
            else
            {
                minister.Ideology = MinisterIdeology.None;
                Log.Warning("[Minister] Invalid ideology: {0} [{1}: {2}] ({3} L{4})", tokens[index], minister.Id,
                    minister.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Ministerial characteristics
            string personalityName = tokens[index].ToLower();
            if (PersonalityStringMap.ContainsKey(personalityName))
            {
                minister.Personality = PersonalityStringMap[personalityName];
            }
            else
            {
                if (PersonalityStringTypoMap.ContainsKey(personalityName) &&
                    PersonalityStringMap.ContainsKey(PersonalityStringTypoMap[personalityName]))
                {
                    minister.Personality = PersonalityStringMap[PersonalityStringTypoMap[personalityName]];
                    Log.Warning("[Minister] Modified personality: {0} -> {1} [{2}: {3}] ({4} L{5})", tokens[index],
                        Personalities[minister.Personality].String, minister.Id, minister.Name, lexer.FileName,
                        lexer.LineNo);
                }
                else
                {
                    minister.Personality = 0;
                    Log.Warning("[Minister] Invalid personality: {0} [{1}: {2}] ({3} L{4})", tokens[index], minister.Id,
                        minister.Name, lexer.FileName, lexer.LineNo);
                }
            }
            index++;

            // Loyalty
            string loyaltyName = tokens[index].ToLower();
            if (LoyaltyStringMap.ContainsKey(loyaltyName))
            {
                minister.Loyalty = LoyaltyStringMap[loyaltyName];
            }
            else
            {
                minister.Loyalty = MinisterLoyalty.None;
                Log.Warning("[Minister] Invalid loyalty: {0} [{1}: {2}] ({3} L{4})", tokens[index], minister.Id,
                    minister.Name, lexer.FileName, lexer.LineNo);
            }
            index++;

            // Image file name
            minister.PictureName = tokens[index];

            return minister;
        }

        #endregion

        #region File writing

        /// <summary>
        ///     Save ministerial files
        /// </summary>
        /// <returns>If saving fails false false return it</returns>
        public static bool Save()
        {
            // Do nothing if not edited
            if (!IsDirty())
            {
                return true;
            }

            // Wait for completion if loading is in progress
            if (Worker.IsBusy)
            {
                WaitLoading();
            }

            // Save the ministerial list file
            if ((Game.Type == GameType.DarkestHour) && IsDirtyList())
            {
                try
                {
                    SaveList();
                }
                catch (Exception)
                {
                    string fileName = Game.GetWriteFileName(Game.DhMinisterListPathName);
                    Log.Error("[Minister] Write error: {0}", fileName);
                    MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                        Resources.EditorMinister, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            bool error = false;
            foreach (Country country in Countries.Tags
                .Where(country => DirtyFlags[(int) country] && country != Country.None))
            {
                try
                {
                    // Save ministerial files
                    SaveFile(country);
                }
                catch (Exception)
                {
                    error = true;
                    string fileName = Game.GetWriteFileName(Game.MinisterPathName, Game.GetMinisterFileName(country));
                    Log.Error("[Minister] Write error: {0}", fileName);
                    if (MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                        Resources.EditorMinister, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                        return false;
                }
            }

            // Return if saving fails
            if (error)
            {
                return false;
            }

            // Clear the edited flag
            _dirtyFlag = false;

            return true;
        }

        /// <summary>
        ///     Save the ministerial list file (DH)
        /// </summary>
        private static void SaveList()
        {
            // Create a database folder if it does not exist
            string folderName = Game.GetWriteFileName(Game.DatabasePathName);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string fileName = Game.GetWriteFileName(Game.DhMinisterListPathName);
            Log.Info("[Minister] Save: {0}", Path.GetFileName(fileName));

            // Write the registered ministerial file names in order
            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                foreach (string name in FileNameMap.Select(pair => pair.Value))
                {
                    writer.WriteLine(name);
                }
            }

            // Clear the edited flag
            ResetDirtyList();
        }

        /// <summary>
        ///     Save ministerial files
        /// </summary>
        /// <param name="country">Country tag</param>
        private static void SaveFile(Country country)
        {
            // Create a ministerial folder if it does not exist
            string folderName = Game.GetWriteFileName(Game.MinisterPathName);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            string name = Game.GetMinisterFileName(country);
            string fileName = Path.Combine(folderName, name);
            Log.Info("[Minister] Save: {0}", name);

            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                int lineNo = 3;

                // Write header line
                if (Misc.EnableRetirementYearMinisters)
                {
                    writer.WriteLine(
                        "{0};Ruling Cabinet - Start;Name;Start Year;End Year;Retirement Year;Ideology;Personality;Loyalty;Picturename;X",
                        Countries.Strings[(int) country]);
                    writer.WriteLine(";Replacements;;;;;;;;;X");
                }
                else if (Misc.UseNewMinisterFilesFormat)
                {
                    writer.WriteLine(
                        "{0};Ruling Cabinet - Start;Name;Start Year;End Year;Ideology;Personality;Loyalty;Picturename;X",
                        Countries.Strings[(int) country]);
                    writer.WriteLine(";Replacements;;;;;;;;X");
                }
                else
                {
                    writer.WriteLine("{0};Ruling Cabinet - Start;Name;Pool;Ideology;Personality;Loyalty;Picturename;x",
                        Countries.Strings[(int) country]);
                    writer.WriteLine(";Replacements;;;;;;;x");
                }

                // Write the ministerial definition lines in order
                foreach (Minister minister in Items.Where(minister => minister.Country == country))
                {
                    // If an invalid value is set, a warning will be output to the log.
                    if (minister.Position == MinisterPosition.None)
                    {
                        Log.Warning("[Minister] Invalid position: {0} {1} ({2} L{3})", minister.Id, minister.Name, name,
                            lineNo);
                    }
                    if (minister.Ideology == MinisterIdeology.None)
                    {
                        Log.Warning("[Minister] Invalid ideology: {0} {1} ({2} L{3})", minister.Id, minister.Name, name,
                            lineNo);
                    }
                    if (minister.Loyalty == MinisterLoyalty.None)
                    {
                        Log.Warning("[Minister] Invalid loyalty: {0} {1} ({2} L{3})", minister.Id, minister.Name, name,
                            lineNo);
                    }

                    // Write a ministerial definition line
                    if (Misc.EnableRetirementYearMinisters)
                    {
                        writer.WriteLine(
                            "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};X",
                            minister.Id,
                            PositionStrings[(int) minister.Position],
                            minister.Name,
                            minister.StartYear,
                            minister.EndYear,
                            minister.RetirementYear,
                            IdeologyStrings[(int) minister.Ideology],
                            Personalities[minister.Personality].String,
                            LoyaltyStrings[(int) minister.Loyalty],
                            minister.PictureName);
                    }
                    else if (Misc.UseNewMinisterFilesFormat)
                    {
                        writer.WriteLine(
                            "{0};{1};{2};{3};{4};{5};{6};{7};{8};X",
                            minister.Id,
                            PositionStrings[(int) minister.Position],
                            minister.Name,
                            minister.StartYear,
                            minister.EndYear,
                            IdeologyStrings[(int) minister.Ideology],
                            Personalities[minister.Personality].String,
                            LoyaltyStrings[(int) minister.Loyalty],
                            minister.PictureName);
                    }
                    else
                    {
                        writer.WriteLine(
                            "{0};{1};{2};{3};{4};{5};{6};{7};x",
                            minister.Id,
                            PositionStrings[(int) minister.Position],
                            minister.Name,
                            minister.StartYear - 1900,
                            IdeologyStrings[(int) minister.Ideology],
                            Personalities[minister.Personality].String,
                            LoyaltyStrings[(int) minister.Loyalty],
                            minister.PictureName);
                    }

                    // Clear the edited flag
                    minister.ResetDirtyAll();

                    lineNo++;
                }
            }

            ResetDirty(country);
        }

        #endregion

        #region Minister list operation

        /// <summary>
        ///     Add an item to the cabinet list
        /// </summary>
        /// <param name="minister">Items to be inserted</param>
        public static void AddItem(Minister minister)
        {
            Log.Info("[Minister] Add minister: ({0}: {1}) <{2}>", minister.Id, minister.Name,
                Countries.Strings[(int) minister.Country]);

            Items.Add(minister);
        }

        /// <summary>
        ///     Insert an item into the cabinet list
        /// </summary>
        /// <param name="minister">Items to be inserted</param>
        /// <param name="position">Item immediately before the insertion position</param>
        public static void InsertItem(Minister minister, Minister position)
        {
            int index = Items.IndexOf(position) + 1;

            Log.Info("[Minister] Insert minister: {0} ({1}: {2}) <{3}>", index, minister.Id, minister.Name,
                Countries.Strings[(int) minister.Country]);

            Items.Insert(index, minister);
        }

        /// <summary>
        ///     Remove an item from the cabinet list
        /// </summary>
        /// <param name="minister"></param>
        public static void RemoveItem(Minister minister)
        {
            Log.Info("[Minister] Move minister: ({0}: {1}) <{2}>", minister.Id, minister.Name,
                Countries.Strings[(int) minister.Country]);

            Items.Remove(minister);

            // Already used ID Remove from list
            IdSet.Remove(minister.Id);
        }

        /// <summary>
        ///     Move items in the cabinet list
        /// </summary>
        /// <param name="src">Item of move source</param>
        /// <param name="dest">Item to move to</param>
        public static void MoveItem(Minister src, Minister dest)
        {
            int srcIndex = Items.IndexOf(src);
            int destIndex = Items.IndexOf(dest);

            Log.Info("[Minister] Move minister: {0} -> {1} ({2}: {3}) <{4}>", srcIndex, destIndex, src.Id, src.Name,
                Countries.Strings[(int) src.Country]);

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

        #region Bulk editing

        /// <summary>
        ///     Bulk editing
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        public static void BatchEdit(MinisterBatchEditArgs args)
        {
            LogBatchEdit(args);

            IEnumerable<Minister> ministers = GetBatchEditMinisters(args);
            Country newCountry;
            switch (args.ActionMode)
            {
                case BatchActionMode.Modify:
                    // Bulk edit ministers
                    foreach (Minister minister in ministers)
                    {
                        BatchEditMinister(minister, args);
                    }
                    break;

                case BatchActionMode.Copy:
                    // Copy ministers
                    newCountry = args.Destination;
                    int id = args.Id;
                    foreach (Minister minister in ministers)
                    {
                        id = GetNewId(id);
                        Minister newMinister = new Minister(minister)
                        {
                            Country = newCountry,
                            Id = id
                        };
                        newMinister.SetDirtyAll();
                        Items.Add(newMinister);
                    }

                    // Set the edited flag for the destination country
                    SetDirty(newCountry);

                    // If the copy destination country does not exist in the file list, add it
                    if (!FileNameMap.ContainsKey(newCountry))
                    {
                        FileNameMap.Add(newCountry, Game.GetMinisterFileName(newCountry));
                        SetDirtyList();
                    }
                    break;

                case BatchActionMode.Move:
                    // Move ministers
                    newCountry = args.Destination;
                    foreach (Minister minister in ministers)
                    {
                        // Set the edited flag for the country before the move
                        SetDirty(minister.Country);

                        minister.Country = newCountry;
                        minister.SetDirty(MinisterItemId.Country);
                    }

                    // Set the edited flag for the destination country
                    SetDirty(newCountry);

                    // If the destination country does not exist in the file list, add it.
                    if (!FileNameMap.ContainsKey(newCountry))
                    {
                        FileNameMap.Add(newCountry, Game.GetMinisterFileName(newCountry));
                        SetDirtyList();
                    }
                    break;
            }
        }

        /// <summary>
        ///     Individual processing of batch editing
        /// </summary>
        /// <param name="minister">Target ministers</param>
        /// <param name="args">Batch editing parameters</param>
        private static void BatchEditMinister(Minister minister, MinisterBatchEditArgs args)
        {
            // Start year
            if (args.Items[(int) MinisterBatchItemId.StartYear])
            {
                if (minister.StartYear != args.StartYear)
                {
                    minister.StartYear = args.StartYear;
                    minister.SetDirty(MinisterItemId.StartYear);
                    SetDirty(minister.Country);
                }
            }

            // End year
            if (args.Items[(int) MinisterBatchItemId.EndYear])
            {
                if (minister.EndYear != args.EndYear)
                {
                    minister.EndYear = args.EndYear;
                    minister.SetDirty(MinisterItemId.EndYear);
                    SetDirty(minister.Country);
                }
            }

            // Retirement year
            if (args.Items[(int) MinisterBatchItemId.RetirementYear])
            {
                if (minister.RetirementYear != args.RetirementYear)
                {
                    minister.RetirementYear = args.RetirementYear;
                    minister.SetDirty(MinisterItemId.RetirementYear);
                    SetDirty(minister.Country);
                }
            }

            // ideology
            if (args.Items[(int) MinisterBatchItemId.Ideology])
            {
                if (minister.Ideology != args.Ideology)
                {
                    minister.Ideology = args.Ideology;
                    minister.SetDirty(MinisterItemId.Ideology);
                    SetDirty(minister.Country);
                }
            }

            // Loyalty
            if (args.Items[(int) MinisterBatchItemId.Loyalty])
            {
                if (minister.Loyalty != args.Loyalty)
                {
                    minister.Loyalty = args.Loyalty;
                    minister.SetDirty(MinisterItemId.Loyalty);
                    SetDirty(minister.Country);
                }
            }
        }

        /// <summary>
        ///     Get a list of ministers for bulk editing
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        /// <returns>List of ministers subject to batch editing</returns>
        private static IEnumerable<Minister> GetBatchEditMinisters(MinisterBatchEditArgs args)
        {
            return args.CountryMode == BatchCountryMode.All
                ? Items.Where(minister => args.PositionMode[(int) minister.Position]).ToList()
                : Items.Where(minister => args.TargetCountries.Contains(minister.Country))
                    .Where(minister => args.PositionMode[(int) minister.Position]).ToList();
        }

        /// <summary>
        ///     Batch edit processing log output
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        private static void LogBatchEdit(MinisterBatchEditArgs args)
        {
            Log.Verbose($"[Minister] Batch {GetBatchEditItemLog(args)} ({GetBatchEditModeLog(args)})");
        }

        /// <summary>
        ///     Get the log string of batch edit items
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        /// <returns>Log string</returns>
        private static string GetBatchEditItemLog(MinisterBatchEditArgs args)
        {
            StringBuilder sb = new StringBuilder();
            if (args.Items[(int) MinisterBatchItemId.StartYear])
            {
                sb.Append($" start year: {args.StartYear}");
            }
            if (args.Items[(int) MinisterBatchItemId.EndYear])
            {
                sb.Append($" end year: {args.EndYear}");
            }
            if (args.Items[(int) MinisterBatchItemId.RetirementYear])
            {
                sb.Append($" retirement year: {args.RetirementYear}");
            }
            if (args.Items[(int) MinisterBatchItemId.Ideology])
            {
                sb.Append($" ideology: {IdeologyNames[(int) args.Ideology]}");
            }
            if (args.Items[(int) MinisterBatchItemId.Loyalty])
            {
                sb.Append($" loyalty: {LoyaltyNames[(int) args.Loyalty]}");
            }
            if (args.ActionMode == BatchActionMode.Copy)
            {
                sb.Append($" Copy: {Countries.Strings[(int) args.Destination]} id: {args.Id}");
            }
            else if (args.ActionMode == BatchActionMode.Move)
            {
                sb.Append($" Move: {Countries.Strings[(int) args.Destination]} id: {args.Id}");
            }
            if (sb.Length > 0)
            {
                sb.Remove(0, 1);
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Get the log character string of the batch edit target mode
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        /// <returns>Log string</returns>
        private static string GetBatchEditModeLog(MinisterBatchEditArgs args)
        {
            StringBuilder sb = new StringBuilder();

            // Countries subject to batch editing
            if (args.CountryMode == BatchCountryMode.All)
            {
                sb.Append("ALL");
            }
            else
            {
                foreach (Country country in args.TargetCountries)
                {
                    sb.Append($" {Countries.Strings[(int) country]}");
                }
                if (sb.Length > 0)
                {
                    sb.Remove(0, 1);
                }
            }

            // Batch edit target position
            if (!args.PositionMode[(int) MinisterPosition.HeadOfState] ||
                !args.PositionMode[(int) MinisterPosition.HeadOfGovernment] ||
                !args.PositionMode[(int) MinisterPosition.ForeignMinister] ||
                !args.PositionMode[(int) MinisterPosition.MinisterOfArmament] ||
                !args.PositionMode[(int) MinisterPosition.MinisterOfSecurity] ||
                !args.PositionMode[(int) MinisterPosition.HeadOfMilitaryIntelligence] ||
                !args.PositionMode[(int) MinisterPosition.ChiefOfStaff] ||
                !args.PositionMode[(int) MinisterPosition.ChiefOfArmy] ||
                !args.PositionMode[(int) MinisterPosition.ChiefOfNavy] ||
                !args.PositionMode[(int) MinisterPosition.ChiefOfAirForce])
            {
                sb.Append(
                    $"|{(args.PositionMode[(int) MinisterPosition.HeadOfState] ? 'o' : 'x')}" +
                    $"{(args.PositionMode[(int) MinisterPosition.HeadOfGovernment] ? 'o' : 'x')}" +
                    $"{(args.PositionMode[(int) MinisterPosition.ForeignMinister] ? 'o' : 'x')}" +
                    $"{(args.PositionMode[(int) MinisterPosition.MinisterOfArmament] ? 'o' : 'x')}" +
                    $"{(args.PositionMode[(int) MinisterPosition.MinisterOfSecurity] ? 'o' : 'x')}" +
                    $"{(args.PositionMode[(int) MinisterPosition.HeadOfMilitaryIntelligence] ? 'o' : 'x')}" +
                    $"{(args.PositionMode[(int) MinisterPosition.ChiefOfStaff] ? 'o' : 'x')}" +
                    $"{(args.PositionMode[(int) MinisterPosition.ChiefOfArmy] ? 'o' : 'x')}" +
                    $"{(args.PositionMode[(int) MinisterPosition.ChiefOfNavy] ? 'o' : 'x')}" +
                    $"{(args.PositionMode[(int) MinisterPosition.ChiefOfAirForce] ? 'o' : 'x')}");
            }
            return sb.ToString();
        }

        #endregion

        #region ID operation

        /// <summary>
        ///     Unused ministers ID To get
        /// </summary>
        /// <param name="country">Target country tag</param>
        /// <returns>Minister ID</returns>
        public static int GetNewId(Country country)
        {
            // Ministers of the target country ID Maximum value of +1 Start searching from
            int id = GetMaxId(country);
            // unused ID Until you find ID of 1 Increase by little
            return GetNewId(id);
        }

        /// <summary>
        ///     Unused ministers ID To get
        /// </summary>
        /// <param name="id">start ID</param>
        /// <returns>Minister ID</returns>
        public static int GetNewId(int id)
        {
            while (IdSet.Contains(id))
            {
                id++;
            }
            return id;
        }

        /// <summary>
        ///     Ministers of the target country ID Get the maximum value of
        /// </summary>
        /// <param name="country">Target country</param>
        /// <returns>Minister ID</returns>
        private static int GetMaxId(Country country)
        {
            if (country == Country.None)
            {
                return 1;
            }
            List<int> ids =
                Items.Where(minister => minister.Country == country).Select(minister => minister.Id).ToList();
            if (!ids.Any())
            {
                return 1;
            }
            return ids.Max() + 1;
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if it has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirty()
        {
            return _dirtyFlag || _dirtyListFlag;
        }

        /// <summary>
        ///     Get if the ministerial list file has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        private static bool IsDirtyList()
        {
            return _dirtyListFlag;
        }

        /// <summary>
        ///     Get if it has been edited
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirty(Country country)
        {
            return DirtyFlags[(int) country];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="country">Country tag</param>
        public static void SetDirty(Country country)
        {
            DirtyFlags[(int) country] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag for the ministerial list file
        /// </summary>
        public static void SetDirtyList()
        {
            _dirtyListFlag = true;
        }

        /// <summary>
        ///     Clear the edited flag
        /// </summary>
        /// <param name="country">Country tag</param>
        private static void ResetDirty(Country country)
        {
            DirtyFlags[(int) country] = false;
        }

        /// <summary>
        ///     Clear the edited flag of the ministerial list file
        /// </summary>
        private static void ResetDirtyList()
        {
            _dirtyListFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Ministerial characteristic information
    /// </summary>
    public class MinisterPersonalityInfo
    {
        #region Public properties

        /// <summary>
        ///     Correspondence between ministerial status and ministerial characteristics
        /// </summary>
        public bool[] Position { get; } = new bool[Enum.GetValues(typeof (MinisterPosition)).Length];

        /// <summary>
        ///     Ministerial characteristic name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Character string of ministerial characteristic name
        /// </summary>
        public string NameText => Config.ExistsKey(Name) ? Config.GetText(Name) : Name;

        /// <summary>
        ///     Ministerial characteristic string
        /// </summary>
        public string String { get; set; }

        #endregion
    }

    /// <summary>
    ///     Parameters for ministerial batch editing
    /// </summary>
    public class MinisterBatchEditArgs
    {
        #region Public properties

        /// <summary>
        ///     Batch edit target country mode
        /// </summary>
        public BatchCountryMode CountryMode { get; set; }

        /// <summary>
        ///     Target country list
        /// </summary>
        public List<Country> TargetCountries { get; } = new List<Country>();

        /// <summary>
        ///     Batch edit target position mode
        /// </summary>
        public bool[] PositionMode { get; } = new bool[Enum.GetValues(typeof (MinisterPosition)).Length];

        /// <summary>
        ///     Batch edit operation mode
        /// </summary>
        public BatchActionMode ActionMode { get; set; }

        /// <summary>
        ///     copy / / Designated country of destination
        /// </summary>
        public Country Destination { get; set; }

        /// <summary>
        ///     start ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Bulk edit items
        /// </summary>
        public bool[] Items { get; } = new bool[Enum.GetValues(typeof (MinisterBatchItemId)).Length];

        /// <summary>
        ///     Start year
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        ///     End year
        /// </summary>
        public int EndYear { get; set; }

        /// <summary>
        ///     Retirement year
        /// </summary>
        public int RetirementYear { get; set; }

        /// <summary>
        ///     ideology
        /// </summary>
        public MinisterIdeology Ideology { get; set; }

        /// <summary>
        ///     Loyalty
        /// </summary>
        public MinisterLoyalty Loyalty { get; set; }

        #endregion
    }

    /// <summary>
    ///     Ministerial batch edit items ID
    /// </summary>
    public enum MinisterBatchItemId
    {
        StartYear, // Start year
        EndYear, // End year
        RetirementYear, // Retirement year
        Ideology, // ideology
        Loyalty // Loyalty
    }
}
