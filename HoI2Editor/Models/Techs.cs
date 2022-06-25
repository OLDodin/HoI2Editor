﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
    ///     Technical data group
    /// </summary>
    public static class Techs
    {
        #region Public property

        /// <summary>
        ///     Technical group list
        /// </summary>
        public static List<TechGroup> Groups { get; }

        /// <summary>
        ///     Technical ID list
        /// </summary>
        public static List<int> TechIds { get; }

        /// <summary>
        ///     Technical ID correspondence table
        /// </summary>
        public static Dictionary<int, TechItem> TechIdMap { get; }

        /// <summary>
        ///     Research characteristics list
        /// </summary>
        public static TechSpeciality[] Specialities { get; private set; }

        /// <summary>
        ///     Research characteristic character string and ID correspondence
        /// </summary>
        public static Dictionary<string, TechSpeciality> SpecialityStringMap { get; }

        /// <summary>
        ///     Research characteristic image list
        /// </summary>
        public static ImageList SpecialityImages { get; private set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Duplicate character string list
        /// </summary>
        private static readonly Dictionary<string, int> DuplicatedList = new Dictionary<string, int>();

        /// <summary>
        ///     Readed flag
        /// </summary>
        private static bool _loaded;

        /// <summary>
        ///     For delay reading
        /// </summary>
        private static readonly BackgroundWorker Worker = new BackgroundWorker();

        /// <summary>
        ///     Edited flag
        /// </summary>
        private static bool _dirtyFlag;

        #endregion

        #region Public constant

        /// <summary>
        ///     Technical category string
        /// </summary>
        public static readonly string[] CategoryStrings =
        {
            "infantry",
            "armor",
            "naval",
            "aircraft",
            "industry",
            "land_doctrines",
            "secret_weapons",
            "naval_doctrines",
            "air_doctrines"
        };

        /// <summary>
        ///     Technical category name
        /// </summary>
        private static readonly string[] CategoryNames =
        {
            "INFANTRY",
            "ARMOR",
            "NAVAL",
            "AIRCRAFT",
            "INDUSTRY",
            "LD",
            "SW",
            "ND",
            "AD"
        };

        /// <summary>
        ///     Research characteristic text column
        /// </summary>
        public static readonly string[] SpecialityStrings =
        {
            "",
            "artillery",
            "mechanics",
            "electronics",
            "chemistry",
            "training",
            "general_equipment",
            "rocketry",
            "naval_engineering",
            "aeronautics",
            "nuclear_physics",
            "nuclear_engineering",
            "management",
            "industrial_engineering",
            "mathematics",
            "small_unit_tactics",
            "large_unit_tactics",
            "centralized_execution",
            "decentralized_execution",
            "technical_efficiency",
            "individual_courage",
            "infantry_focus",
            "combined_arms_focus",
            "large_unit_focus",
            "naval_artillery",
            "naval_training",
            "aircraft_testing",
            "fighter_tactics",
            "bomber_tactics",
            "carrier_tactics",
            "submarine_tactics",
            "large_taskforce_tactics",
            "small_taskforce_tactics",
            "seamanship",
            "piloting",
            "avionics",
            "munitions",
            "vehicle_engineering",
            "carrier_design",
            "submarine_design",
            "fighter_design",
            "bomber_design",
            "mountain_training",
            "airborne_training",
            "marine_training",
            "maneuver_tactics",
            "blitzkrieg_tactics",
            "static_defense_tactics",
            "medicine",
            "cavalry_tactics",
            "rt_user_1",
            "rt_user_2",
            "rt_user_3",
            "rt_user_4",
            "rt_user_5",
            "rt_user_6",
            "rt_user_7",
            "rt_user_8",
            "rt_user_9",
            "rt_user_10",
            "rt_user_11",
            "rt_user_12",
            "rt_user_13",
            "rt_user_14",
            "rt_user_15",
            "rt_user_16",
            "rt_user_17",
            "rt_user_18",
            "rt_user_19",
            "rt_user_20",
            "rt_user_21",
            "rt_user_22",
            "rt_user_23",
            "rt_user_24",
            "rt_user_25",
            "rt_user_26",
            "rt_user_27",
            "rt_user_28",
            "rt_user_29",
            "rt_user_30",
            "rt_user_31",
            "rt_user_32",
            "rt_user_33",
            "rt_user_34",
            "rt_user_35",
            "rt_user_36",
            "rt_user_37",
            "rt_user_38",
            "rt_user_39",
            "rt_user_40",
            "rt_user_41",
            "rt_user_42",
            "rt_user_43",
            "rt_user_44",
            "rt_user_45",
            "rt_user_46",
            "rt_user_47",
            "rt_user_48",
            "rt_user_49",
            "rt_user_50",
            "rt_user_51",
            "rt_user_52",
            "rt_user_53",
            "rt_user_54",
            "rt_user_55",
            "rt_user_56",
            "rt_user_57",
            "rt_user_58",
            "rt_user_59",
            "rt_user_60"
        };

        /// <summary>
        ///     Category string and ID correspondence
        /// </summary>
        public static readonly Dictionary<string, TechCategory> CategoryMap
            = new Dictionary<string, TechCategory>
            {
                { "infantry", TechCategory.Infantry },
                { "armor", TechCategory.Armor },
                { "naval", TechCategory.Naval },
                { "aircraft", TechCategory.Aircraft },
                { "industry", TechCategory.Industry },
                { "land_doctrines", TechCategory.LandDoctrines },
                { "secret_weapons", TechCategory.SecretWeapons },
                { "naval_doctrines", TechCategory.NavalDoctrines },
                { "air_doctrines", TechCategory.AirDoctrines }
            };

        #endregion

        #region Internal fixed number

        /// <summary>
        ///     Technical definition file name
        /// </summary>
        private static readonly string[] FileNames =
        {
            "infantry_tech.txt",
            "armor_tech.txt",
            "naval_tech.txt",
            "aircraft_tech.txt",
            "industry_tech.txt",
            "land_doctrines_tech.txt",
            "secret_weapons_tech.txt",
            "naval_doctrines_tech.txt",
            "air_doctrines_tech.txt"
        };

        /// <summary>
        ///     Research characteristics list (HoI2)
        /// </summary>
        private static readonly TechSpeciality[] SpecialitiesHoI2 =
        {
            TechSpeciality.None,
            TechSpeciality.Artillery,
            TechSpeciality.Mechanics,
            TechSpeciality.Electronics,
            TechSpeciality.Chemistry,
            TechSpeciality.Training,
            TechSpeciality.GeneralEquipment,
            TechSpeciality.Rocketry,
            TechSpeciality.NavalEngineering,
            TechSpeciality.Aeronautics,
            TechSpeciality.NuclearPhysics,
            TechSpeciality.NuclearEngineering,
            TechSpeciality.Management,
            TechSpeciality.IndustrialEngineering,
            TechSpeciality.Mathematics,
            TechSpeciality.SmallUnitTactics,
            TechSpeciality.LargeUnitTactics,
            TechSpeciality.CentralizedExecution,
            TechSpeciality.DecentralizedExecution,
            TechSpeciality.TechnicalEfficiency,
            TechSpeciality.IndividualCourage,
            TechSpeciality.InfantryFocus,
            TechSpeciality.CombinedArmsFocus,
            TechSpeciality.LargeUnitFocus,
            TechSpeciality.NavalArtillery,
            TechSpeciality.NavalTraining,
            TechSpeciality.AircraftTesting,
            TechSpeciality.FighterTactics,
            TechSpeciality.BomberTactics,
            TechSpeciality.CarrierTactics,
            TechSpeciality.SubmarineTactics,
            TechSpeciality.LargeTaskforceTactics,
            TechSpeciality.SmallTaskforceTactics,
            TechSpeciality.Seamanship,
            TechSpeciality.Piloting
        };

        /// <summary>
        ///     Research characteristics list (DH1.02)
        /// </summary>
        private static readonly TechSpeciality[] SpecialitiesDh102 =
        {
            TechSpeciality.None,
            TechSpeciality.Artillery,
            TechSpeciality.Mechanics,
            TechSpeciality.Electronics,
            TechSpeciality.Chemistry,
            TechSpeciality.Training,
            TechSpeciality.GeneralEquipment,
            TechSpeciality.Rocketry,
            TechSpeciality.NavalEngineering,
            TechSpeciality.Aeronautics,
            TechSpeciality.NuclearPhysics,
            TechSpeciality.NuclearEngineering,
            TechSpeciality.Management,
            TechSpeciality.IndustrialEngineering,
            TechSpeciality.Mathematics,
            TechSpeciality.SmallUnitTactics,
            TechSpeciality.LargeUnitTactics,
            TechSpeciality.CentralizedExecution,
            TechSpeciality.DecentralizedExecution,
            TechSpeciality.TechnicalEfficiency,
            TechSpeciality.IndividualCourage,
            TechSpeciality.InfantryFocus,
            TechSpeciality.CombinedArmsFocus,
            TechSpeciality.LargeUnitFocus,
            TechSpeciality.NavalArtillery,
            TechSpeciality.NavalTraining,
            TechSpeciality.AircraftTesting,
            TechSpeciality.FighterTactics,
            TechSpeciality.BomberTactics,
            TechSpeciality.CarrierTactics,
            TechSpeciality.SubmarineTactics,
            TechSpeciality.LargeTaskforceTactics,
            TechSpeciality.SmallTaskforceTactics,
            TechSpeciality.Seamanship,
            TechSpeciality.Piloting,
            TechSpeciality.Avionics,
            TechSpeciality.Munitions,
            TechSpeciality.VehicleEngineering,
            TechSpeciality.CarrierDesign,
            TechSpeciality.SubmarineDesign,
            TechSpeciality.FighterDesign,
            TechSpeciality.BomberDesign,
            TechSpeciality.MountainTraining,
            TechSpeciality.AirborneTraining,
            TechSpeciality.MarineTraining,
            TechSpeciality.ManeuverTactics,
            TechSpeciality.BlitzkriegTactics,
            TechSpeciality.StaticDefenseTactics,
            TechSpeciality.Medicine,
            TechSpeciality.RtUser1,
            TechSpeciality.RtUser2,
            TechSpeciality.RtUser3,
            TechSpeciality.RtUser4,
            TechSpeciality.RtUser5,
            TechSpeciality.RtUser6,
            TechSpeciality.RtUser7,
            TechSpeciality.RtUser8,
            TechSpeciality.RtUser9,
            TechSpeciality.RtUser10,
            TechSpeciality.RtUser11,
            TechSpeciality.RtUser12,
            TechSpeciality.RtUser13,
            TechSpeciality.RtUser14,
            TechSpeciality.RtUser15,
            TechSpeciality.RtUser16
        };

        /// <summary>
        ///     Research characteristics list (DH1.03)
        /// </summary>
        private static readonly TechSpeciality[] SpecialitiesDh =
        {
            TechSpeciality.None,
            TechSpeciality.Artillery,
            TechSpeciality.Mechanics,
            TechSpeciality.Electronics,
            TechSpeciality.Chemistry,
            TechSpeciality.Training,
            TechSpeciality.GeneralEquipment,
            TechSpeciality.Rocketry,
            TechSpeciality.NavalEngineering,
            TechSpeciality.Aeronautics,
            TechSpeciality.NuclearPhysics,
            TechSpeciality.NuclearEngineering,
            TechSpeciality.Management,
            TechSpeciality.IndustrialEngineering,
            TechSpeciality.Mathematics,
            TechSpeciality.SmallUnitTactics,
            TechSpeciality.LargeUnitTactics,
            TechSpeciality.CentralizedExecution,
            TechSpeciality.DecentralizedExecution,
            TechSpeciality.TechnicalEfficiency,
            TechSpeciality.IndividualCourage,
            TechSpeciality.InfantryFocus,
            TechSpeciality.CombinedArmsFocus,
            TechSpeciality.LargeUnitFocus,
            TechSpeciality.NavalArtillery,
            TechSpeciality.NavalTraining,
            TechSpeciality.AircraftTesting,
            TechSpeciality.FighterTactics,
            TechSpeciality.BomberTactics,
            TechSpeciality.CarrierTactics,
            TechSpeciality.SubmarineTactics,
            TechSpeciality.LargeTaskforceTactics,
            TechSpeciality.SmallTaskforceTactics,
            TechSpeciality.Seamanship,
            TechSpeciality.Piloting,
            TechSpeciality.Avionics,
            TechSpeciality.Munitions,
            TechSpeciality.VehicleEngineering,
            TechSpeciality.CarrierDesign,
            TechSpeciality.SubmarineDesign,
            TechSpeciality.FighterDesign,
            TechSpeciality.BomberDesign,
            TechSpeciality.MountainTraining,
            TechSpeciality.AirborneTraining,
            TechSpeciality.MarineTraining,
            TechSpeciality.ManeuverTactics,
            TechSpeciality.BlitzkriegTactics,
            TechSpeciality.StaticDefenseTactics,
            TechSpeciality.Medicine,
            TechSpeciality.CavalryTactics,
            TechSpeciality.RtUser1,
            TechSpeciality.RtUser2,
            TechSpeciality.RtUser3,
            TechSpeciality.RtUser4,
            TechSpeciality.RtUser5,
            TechSpeciality.RtUser6,
            TechSpeciality.RtUser7,
            TechSpeciality.RtUser8,
            TechSpeciality.RtUser9,
            TechSpeciality.RtUser10,
            TechSpeciality.RtUser11,
            TechSpeciality.RtUser12,
            TechSpeciality.RtUser13,
            TechSpeciality.RtUser14,
            TechSpeciality.RtUser15,
            TechSpeciality.RtUser16,
            TechSpeciality.RtUser17,
            TechSpeciality.RtUser18,
            TechSpeciality.RtUser19,
            TechSpeciality.RtUser20,
            TechSpeciality.RtUser21,
            TechSpeciality.RtUser22,
            TechSpeciality.RtUser23,
            TechSpeciality.RtUser24,
            TechSpeciality.RtUser25,
            TechSpeciality.RtUser26,
            TechSpeciality.RtUser27,
            TechSpeciality.RtUser28,
            TechSpeciality.RtUser29,
            TechSpeciality.RtUser30,
            TechSpeciality.RtUser31,
            TechSpeciality.RtUser32,
            TechSpeciality.RtUser33,
            TechSpeciality.RtUser34,
            TechSpeciality.RtUser35,
            TechSpeciality.RtUser36,
            TechSpeciality.RtUser37,
            TechSpeciality.RtUser38,
            TechSpeciality.RtUser39,
            TechSpeciality.RtUser40,
            TechSpeciality.RtUser41,
            TechSpeciality.RtUser42,
            TechSpeciality.RtUser43,
            TechSpeciality.RtUser44,
            TechSpeciality.RtUser45,
            TechSpeciality.RtUser46,
            TechSpeciality.RtUser47,
            TechSpeciality.RtUser48,
            TechSpeciality.RtUser49,
            TechSpeciality.RtUser50,
            TechSpeciality.RtUser51,
            TechSpeciality.RtUser52,
            TechSpeciality.RtUser53,
            TechSpeciality.RtUser54,
            TechSpeciality.RtUser55,
            TechSpeciality.RtUser56,
            TechSpeciality.RtUser57,
            TechSpeciality.RtUser58,
            TechSpeciality.RtUser59,
            TechSpeciality.RtUser60
        };

        /// <summary>
        ///     Research characteristic name
        /// </summary>
        private static readonly string[] SpecialityNames =
        {
            "",
            "RT_ARTILLERY",
            "RT_MECHANICS",
            "RT_ELECTRONICS",
            "RT_CHEMISTRY",
            "RT_TRAINING",
            "RT_GENERAL_EQUIPMENT",
            "RT_ROCKETRY",
            "RT_NAVAL_ENGINEERING",
            "RT_AERONAUTICS",
            "RT_NUCLEAR_PHYSICS",
            "RT_NUCLEAR_ENGINEERING",
            "RT_MANAGEMENT",
            "RT_INDUSTRIAL_ENGINEERING",
            "RT_MATHEMATICS",
            "RT_SMALL_UNIT_TACTICS",
            "RT_LARGE_UNIT_TACTICS",
            "RT_CENTRALIZED_EXECUTION",
            "RT_DECENTRALIZED_EXECUTION",
            "RT_TECHNICAL_EFFICIENCY",
            "RT_INDIVIDUAL_COURAGE",
            "RT_INFANTRY_FOCUS",
            "RT_COMBINED_ARMS_FOCUS",
            "RT_LARGE_UNIT_FOCUS",
            "RT_NAVAL_ARTILLERY",
            "RT_NAVAL_TRAINING",
            "RT_AIRCRAFT_TESTING",
            "RT_FIGHTER_TACTICS",
            "RT_BOMBER_TACTICS",
            "RT_CARRIER_TACTICS",
            "RT_SUBMARINE_TACTICS",
            "RT_LARGE_TASKFORCE_TACTICS",
            "RT_SMALL_TASKFORCE_TACTICS",
            "RT_SEAMANSHIP",
            "RT_PILOTING",
            "RT_AVIONICS",
            "RT_MUNITIONS",
            "RT_VEHICLE_ENGINEERING",
            "RT_CARRIER_DESIGN",
            "RT_SUBMARINE_DESIGN",
            "RT_FIGHTER_DESIGN",
            "RT_BOMBER_DESIGN",
            "RT_MOUNTAIN_TRAINING",
            "RT_AIRBORNE_TRAINING",
            "RT_MARINE_TRAINING",
            "RT_MANEUVER_TACTICS",
            "RT_BLITZKRIEG_TACTICS",
            "RT_STATIC_DEFENSE_TACTICS",
            "RT_MEDICINE",
            "RT_CAVALRY_TACTICS",
            "RT_USER_1",
            "RT_USER_2",
            "RT_USER_3",
            "RT_USER_4",
            "RT_USER_5",
            "RT_USER_6",
            "RT_USER_7",
            "RT_USER_8",
            "RT_USER_9",
            "RT_USER_10",
            "RT_USER_11",
            "RT_USER_12",
            "RT_USER_13",
            "RT_USER_14",
            "RT_USER_15",
            "RT_USER_16",
            "RT_USER_17",
            "RT_USER_18",
            "RT_USER_19",
            "RT_USER_20",
            "RT_USER_21",
            "RT_USER_22",
            "RT_USER_23",
            "RT_USER_24",
            "RT_USER_25",
            "RT_USER_26",
            "RT_USER_27",
            "RT_USER_28",
            "RT_USER_29",
            "RT_USER_30",
            "RT_USER_31",
            "RT_USER_32",
            "RT_USER_33",
            "RT_USER_34",
            "RT_USER_35",
            "RT_USER_36",
            "RT_USER_37",
            "RT_USER_38",
            "RT_USER_39",
            "RT_USER_40",
            "RT_USER_41",
            "RT_USER_42",
            "RT_USER_43",
            "RT_USER_44",
            "RT_USER_45",
            "RT_USER_46",
            "RT_USER_47",
            "RT_USER_48",
            "RT_USER_49",
            "RT_USER_50",
            "RT_USER_51",
            "RT_USER_52",
            "RT_USER_53",
            "RT_USER_54",
            "RT_USER_55",
            "RT_USER_56",
            "RT_USER_57",
            "RT_USER_58",
            "RT_USER_59",
            "RT_USER_60"
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Techs()
        {
            // Technical group list
            Groups = new List<TechGroup>();

            // Technical ID list
            TechIds = new List<int>();

            // Technical ID correspondence
            TechIdMap = new Dictionary<int, TechItem>();

            // Research characteristic character string and ID correspondence
            SpecialityStringMap = new Dictionary<string, TechSpeciality>();
            foreach (TechSpeciality speciality in Enum.GetValues(typeof (TechSpeciality)))
            {
                SpecialityStringMap.Add(SpecialityStrings[(int) speciality], speciality);
            }
        }

        /// <summary>
        ///     Initialize research characteristics
        /// </summary>
        public static void InitSpecialities()
        {
            // Set a research characteristic list
            switch (Game.Type)
            {
                case GameType.HeartsOfIron2:
                case GameType.ArsenalOfDemocracy:
                    Specialities = SpecialitiesHoI2;
                    break;

                case GameType.DarkestHour:
                    Specialities = Game.Version >= 103 ? SpecialitiesDh : SpecialitiesDh102;
                    break;
            }

            // Create a research characteristic image list
            Bitmap bitmap = new Bitmap(Game.GetReadFileName(Game.TechIconPathName));
            SpecialityImages = new ImageList
            {
                ImageSize = new Size(24, 24),
                TransparentColor = bitmap.GetPixel(0, 0)
            };
            SpecialityImages.Images.AddStrip(bitmap);
        }

        #endregion

        #region File reading

        /// <summary>
        ///     Request a relay of technical files
        /// </summary>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     Reload the technical file group
        /// </summary>
        public static void Reload()
        {
            // Do nothing before reading
            if (!_loaded)
            {
                return;
            }

            _loaded = false;

            Load();
        }

        /// <summary>
        ///     Read the technical definition file group
        /// </summary>
        public static void Load()
        {
            // If you have read it, go back
            if (_loaded)
            {
                return;
            }

            // Wait for completion if you are in the middle
            if (Worker.IsBusy)
            {
                WaitLoading();
                return;
            }

            LoadFiles();
        }

        /// <summary>
        ///     Delayed the technical definition file group
        /// </summary>
        /// <param name="handler">Reading completion event handler</param>
        public static void LoadAsync(RunWorkerCompletedEventHandler handler)
        {
            // If you have already loaded, call the completed event handler
            if (_loaded)
            {
                handler?.Invoke(null, new RunWorkerCompletedEventArgs(null, null, false));
                return;
            }

            // Register the reading event handler
            if (handler != null)
            {
                Worker.RunWorkerCompleted += handler;
                Worker.RunWorkerCompleted += OnWorkerRunWorkerCompleted;
            }

            // Return if you are in the middle
            if (Worker.IsBusy)
            {
                return;
            }

            // If you have read here, you will have already called the completed event handler, so return without doing anything.
            if (_loaded)
            {
                return;
            }

            // Start late reading
            Worker.DoWork += OnWorkerDoWork;
            Worker.RunWorkerAsync();
        }

        /// <summary>
        ///     Wait until the loading is completed
        /// </summary>
        public static void WaitLoading()
        {
            while (Worker.IsBusy)
            {
                Application.DoEvents();
            }
        }

        /// <summary>
        ///     Judge whether or not to read delayed
        /// </summary>
        /// <returns>Return True if you are ready to read</returns>
        public static bool IsLoading()
        {
            return Worker.IsBusy;
        }

        /// <summary>
        ///     Late reading processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            LoadFiles();
        }

        /// <summary>
        ///     Processing at the time of delayed reading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Processing at the time of delayed reading
            HoI2EditorController.OnLoadingCompleted();
        }

        /// <summary>
        ///     Read the technical definition file group
        /// </summary>
        private static void LoadFiles()
        {
            // Initialization of commands
            Commands.Init();

            Groups.Clear();

            bool error = false;
            foreach (TechCategory category in Enum.GetValues(typeof (TechCategory)))
            {
                string fileName = FileNames[(int) category];
                string pathName = Game.GetReadFileName(Game.TechPathName, fileName);
                try
                {
                    // Read the technical definition file
                    LoadFile(pathName);
                }
                catch (Exception)
                {
                    error = true;
                    Log.Error("[Tech] Read error: {0}", pathName);
                    if (MessageBox.Show($"{Resources.FileReadError}: {pathName}",
                        Resources.EditorTech, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        break;
                    }
                }
            }

            // Update technical ID correspondence
            UpdateTechIdMap();

            // Update the duplicate character string list
            UpdateDuplicatedList();

            // Register a temporary key with a broken link on the list
            AddUnlinkedTempKey();

            // Return if you fail to read
            if (error)
            {
                return;
            }

            // Unlock the edited flag
            _dirtyFlag = false;

            // Set the read flag
            _loaded = true;
        }

        /// <summary>
        ///     Read the technical definition file
        /// </summary>
        /// <param name="fileName">Technical definition file name</param>
        private static void LoadFile(string fileName)
        {
            Log.Verbose("[Tech] Load: {0}", Path.GetFileName(fileName));

            TechGroup grp = TechParser.Parse(fileName);
            if (grp == null)
            {
                Log.Error("[Tech] Read error: {0}", Path.GetFileName(fileName));
                return;
            }
            Groups.Add(grp);
        }

        #endregion

        #region File writing

        /// <summary>
        ///     Save the technical definition file group
        /// </summary>
        /// <returns>If you fail to save, return False</returns>
        public static bool Save()
        {
            // Wait for completion if you are in the middle
            if (Worker.IsBusy)
            {
                WaitLoading();
            }

            if (IsDirty())
            {
                string folderName = Game.GetWriteFileName(Game.TechPathName);
                try
                {
                    // Create if there is no technical definition folder
                    if (!Directory.Exists(folderName))
                    {
                        Directory.CreateDirectory(folderName);
                    }
                }
                catch (Exception)
                {
                    Log.Error("[Tech] Write error: {0}", folderName);
                    MessageBox.Show($"{Resources.FileWriteError}: {folderName}",
                        Resources.EditorTech, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    return false;
                }

                bool error = false;
                foreach (TechGroup grp in Groups.Where(grp => grp.IsDirty()))
                {
                    string fileName = Path.Combine(folderName, FileNames[(int) grp.Category]);
                    try
                    {
                        // Save the technical definition file
                        Log.Info("[Tech] Save: {0}", Path.GetFileName(fileName));
                        TechWriter.Write(grp, fileName);
                    }
                    catch (Exception)
                    {
                        error = true;
                        Log.Error("[Tech] Write error: {0}", fileName);
                        if (MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                            Resources.EditorTech, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                            == DialogResult.Cancel)
                        {
                            return false;
                        }
                    }
                }

                // Return if you fail to save
                if (error)
                {
                    return false;
                }

                // Unlock the edited flag
                _dirtyFlag = false;
            }

            if (_loaded)
            {
                // When saving only character string definitions, clear all here because the edited flags such as the technical name are not cleared.
                foreach (TechGroup grp in Groups)
                {
                    grp.ResetDirtyAll();
                }
            }

            return true;
        }

        /// <summary>
        ///     Change the character string key to a storage format
        /// </summary>
        public static void RenameKeys()
        {
            foreach (TechGroup grp in Groups)
            {
                string categoryName = CategoryNames[(int) grp.Category];
                bool dirty = false;

                // technology
                List<int> list = new List<int>();
                foreach (TechItem item in grp.Items.OfType<TechItem>())
                {
                    item.AddKeyNumbers(list);
                }
                foreach (TechItem item in grp.Items.OfType<TechItem>())
                {
                    if (item.RenameKeys(categoryName, list))
                    {
                        dirty = true;
                    }
                }

                // label label
                list = new List<int>();
                foreach (TechLabel item in grp.Items.OfType<TechLabel>())
                {
                    item.AddKeyNumbers(list);
                }
                foreach (TechLabel item in grp.Items.OfType<TechLabel>())
                {
                    if (item.RenameKeys(categoryName, list))
                    {
                        dirty = true;
                    }
                }

                // Update the edited flag
                if (dirty)
                {
                    grp.SetDirty();
                }
            }
        }

        #endregion

        #region Technical items and ID correspondence

        /// <summary>
        ///     Change the technical ID
        /// </summary>
        /// <param name="item">Technical project</param>
        /// <param name="id">Technical ID</param>
        public static void ModifyTechId(TechItem item, int id)
        {
            // Delete technical items and ID correspondence before changing values
            TechIds.Remove(id);
            TechIdMap.Remove(id);

            // Update the value
            item.Id = id;

            // Update technical items and ID correspondence
            UpdateTechIdMap();
        }

        /// <summary>
        ///     Update technical items and ID correspondence
        /// </summary>
        public static void UpdateTechIdMap()
        {
            TechIds.Clear();
            TechIdMap.Clear();
            foreach (TechItem item in Groups.SelectMany(grp => grp.Items.OfType<TechItem>()))
            {
                if (!TechIds.Contains(item.Id))
                {
                    TechIds.Add(item.Id);
                    TechIdMap.Add(item.Id, item);
                }
            }
        }

        /// <summary>
        ///     Get unused technical ID
        /// </summary>
        /// <param name="startId">ID to start search</param>
        /// <returns>Unused technical ID</returns>
        public static int GetNewId(int startId)
        {
            int id = startId;
            while (TechIds.Contains(id))
            {
                id += 10;
            }
            return id;
        }

        #endregion

        #region Text column operation

        /// <summary>
        ///     Get whether the definition name of the string is duplicated
        /// </summary>
        /// <param name="name">Target character string definition name</param>
        /// <returns>If the definition name is duplicated, return True</returns>
        public static bool IsDuplicatedName(string name)
        {
            return DuplicatedList.ContainsKey(name) && (DuplicatedList[name] > 1);
        }

        /// <summary>
        ///     Update the duplicate character string list
        /// </summary>
        private static void UpdateDuplicatedList()
        {
            DuplicatedList.Clear();
            foreach (ITechItem item in Groups.SelectMany(grp => grp.Items))
            {
                AddDuplicatedListItem(item);
            }
        }

        /// <summary>
        ///     Add an item to the duplicate character string list
        /// </summary>
        /// <param name="item">Technical project</param>
        public static void AddDuplicatedListItem(ITechItem item)
        {
            TechItem techItem = item as TechItem;
            if (techItem != null)
            {
                IncrementDuplicatedListCount(techItem.Name);
                IncrementDuplicatedListCount(techItem.ShortName);
                IncrementDuplicatedListCount(techItem.Desc);
                foreach (TechComponent component in techItem.Components)
                {
                    IncrementDuplicatedListCount(component.Name);
                }
                return;
            }

            TechLabel labelItem = item as TechLabel;
            if (labelItem != null)
            {
                IncrementDuplicatedListCount(labelItem.Name);
            }
        }

        /// <summary>
        ///     Delete the item of the duplicate character string list
        /// </summary>
        /// <param name="item">Technical project</param>
        public static void RemoveDuplicatedListItem(ITechItem item)
        {
            TechItem techItem = item as TechItem;
            if (techItem != null)
            {
                DecrementDuplicatedListCount(techItem.Name);
                DecrementDuplicatedListCount(techItem.ShortName);
                DecrementDuplicatedListCount(techItem.Desc);
                foreach (TechComponent component in techItem.Components)
                {
                    DecrementDuplicatedListCount(component.Name);
                }
                return;
            }

            TechLabel labelItem = item as TechLabel;
            if (labelItem != null)
            {
                DecrementDuplicatedListCount(labelItem.Name);
            }
        }

        /// <summary>
        ///     Increment of duplicate character string list
        /// </summary>
        /// <param name="name">Target character string definition name</param>
        public static void IncrementDuplicatedListCount(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            if (!DuplicatedList.ContainsKey(name))
            {
                DuplicatedList.Add(name, 1);
            }
            else
            {
                DuplicatedList[name]++;
                Log.Info("[Tech] Incremented duplicated list: {0} {1}", name, DuplicatedList[name]);
            }
        }

        /// <summary>
        ///     Deliver the count of the duplicate character string list
        /// </summary>
        /// <param name="name">Target character string definition name</param>
        public static void DecrementDuplicatedListCount(string name)
        {
            if (!string.IsNullOrEmpty(name) && DuplicatedList.ContainsKey(name))
            {
                DuplicatedList[name]--;
                if (DuplicatedList[name] == 0)
                {
                    DuplicatedList.Remove(name);
                }
                else
                {
                    Log.Info("[Tech] Decremented duplicated list: {0} {1}", name, DuplicatedList[name]);
                }
            }
        }

        /// <summary>
        ///     Register a temporary key with a broken link on the list
        /// </summary>
        private static void AddUnlinkedTempKey()
        {
            foreach (ITechItem item in Groups.SelectMany(grp => grp.Items))
            {
                if (item is TechItem)
                {
                    TechItem techItem = item as TechItem;
                    if (Config.IsTempKey(techItem.Name))
                    {
                        Config.AddTempKey(techItem.Name);
                    }
                    if (Config.IsTempKey(techItem.ShortName))
                    {
                        Config.AddTempKey(techItem.ShortName);
                    }
                    if (Config.IsTempKey(techItem.Desc))
                    {
                        Config.AddTempKey(techItem.Desc);
                    }
                    foreach (TechComponent component in techItem.Components)
                    {
                        if (Config.IsTempKey(component.Name))
                        {
                            Config.AddTempKey(component.Name);
                        }
                    }
                }
                else if (item is TechLabel)
                {
                    TechLabel labelItem = item as TechLabel;
                    if (Config.IsTempKey(labelItem.Name))
                    {
                        Config.AddTempKey(labelItem.Name);
                    }
                }
            }
        }

        /// <summary>
        ///     Get research characteristic name
        /// </summary>
        /// <param name="speciality">Research characteristics</param>
        /// <returns>Research characteristic name</returns>
        public static string GetSpecialityName(TechSpeciality speciality)
        {
            string name = SpecialityNames[(int) speciality];
            return Config.ExistsKey(name) ? Config.GetText(name) : name;
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get whether or not it has been edited
        /// </summary>
        /// <returns>If you have edited, return True</returns>
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

        #endregion
    }
}
