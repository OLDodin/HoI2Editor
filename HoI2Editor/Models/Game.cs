using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using HoI2Editor.Utilities;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Game -related data
    /// </summary>
    public static class Game
    {
        #region Public property

        /// <summary>
        ///     Kind of game
        /// </summary>
        public static GameType Type
        {
            get { return _type; }
            private set
            {
                _type = value;
                OutputGameType();
            }
        }

        /// <summary>
        ///     Game version
        /// </summary>
        public static int Version
        {
            get { return _version; }
            private set
            {
                _version = value;
                OutputGameVersion();
            }
        }

        /// <summary>
        ///     Code page when writing files
        /// </summary>
        public static int CodePage
        {
            get { return _codePage; }
            set
            {
                _codePage = value;
                Log.Verbose("CodePage: {0}", _codePage);

                // Request a file reload
                HoI2EditorController.RequestReload();
            }
        }

        /// <summary>
        ///     Game folder name
        /// </summary>
        public static string FolderName
        {
            get { return _folderName; }
            set
            {
                if (value.Equals(FolderName))
                {
                    return;
                }

                _folderName = value;
                Log.Error("Game Folder: {0}", _folderName);

                // Determine the type of game
                DistinguishGameType();

                // Determine the game version
                DistinguishGameVersion();

                // Determination of language mode
                DistinguishLanguageMode();

                // Update the mod folder name
                UpdateModFolderName();

                // Update the saved folder name
                UpdateExportFolderName();

                // Request a file reload
                HoI2EditorController.RequestReload();
            }
        }

        /// <summary>
        ///     Whether the game folder is effective
        /// </summary>
        public static bool IsGameFolderActive => Type != GameType.None;

        /// <summary>
        ///     MOD name
        /// </summary>
        public static string ModName
        {
            get { return _modName; }
            set
            {
                if (value.Equals(_modName))
                {
                    return;
                }

                _modName = value;
                Log.Error("MOD Name: {0}", _modName);

                // Update the mod folder name
                UpdateModFolderName();

                // Request a file reload
                HoI2EditorController.RequestReload();
            }
        }

        /// <summary>
        ///     Whether mod is effective
        /// </summary>
        public static bool IsModActive { get; private set; }

        /// <summary>
        ///     MOD folder name
        /// </summary>
        public static string ModFolderName { get; private set; }

        /// <summary>
        ///     Whether the storage folder name is valid
        /// </summary>
        public static bool IsExportFolderActive { get; private set; }

        /// <summary>
        ///     Save folder name
        /// </summary>
        public static string ExportName
        {
            get { return _exportName; }
            set
            {
                if (value.Equals(_exportName))
                {
                    return;
                }

                _exportName = value;
                Log.Error("Export Name: {0}", _exportName);

                // Update the saved folder name
                UpdateExportFolderName();

                // Request a file reload
                HoI2EditorController.RequestReload();
            }
        }

        /// <summary>
        ///     Save folder name
        /// </summary>
        public static string ExportFolderName => _exportFolderName;

        #endregion

        #region Internal field

        /// <summary>
        ///     Kind of game
        /// </summary>
        private static GameType _type;

        /// <summary>
        ///     Game version
        /// </summary>
        private static int _version;

        /// <summary>
        ///     Game folder name
        /// </summary>
        private static string _folderName;

        /// <summary>
        ///     MOD name
        /// </summary>
        private static string _modName;

        /// <summary>
        ///     Save folder name (MOD name)
        /// </summary>
        private static string _exportName;

        /// <summary>
        ///     Save folder name (full path)
        /// </summary>
        private static string _exportFolderName;

        /// <summary>
        ///     Executive file name
        /// </summary>
        private static string _exeFileName;

        /// <summary>
        ///     Code page when writing files
        /// </summary>
        private static int _codePage;

        #endregion

        #region Public constant

        /// <summary>
        ///     AI folder
        /// </summary>
        public const string AiPathName = "ai";

        /// <summary>
        ///     String definition folder
        /// </summary>
        public const string ConfigPathName = "config";

        /// <summary>
        ///     Additional string definition folder (Ao D)
        /// </summary>
        public const string ConfigAdditionalPathName = "config\\Additional";

        /// <summary>
        ///     Database folder
        /// </summary>
        public const string DatabasePathName = "db";

        /// <summary>
        ///     Commander folder
        /// </summary>
        public const string LeaderPathName = "db\\leaders";

        /// <summary>
        ///     Ministerial folder
        /// </summary>
        public const string MinisterPathName = "db\\ministers";

        /// <summary>
        ///     Research institution folder
        /// </summary>
        public const string TeamPathName = "db\\tech\\teams";

        /// <summary>
        ///     Technical folder
        /// </summary>
        public const string TechPathName = "db\\tech";

        /// <summary>
        ///     Unit folder
        /// </summary>
        public const string UnitPathName = "db\\units";

        /// <summary>
        ///     Division unit folder
        /// </summary>
        public const string DivisionPathName = "db\\units\\divisions";

        /// <summary>
        ///     Brigade unit folder
        /// </summary>
        public const string BrigadePathName = "db\\units\\brigades";

        /// <summary>
        ///     General image folder
        /// </summary>
        public const string PicturePathName = "gfx\\interface";

        /// <summary>
        ///     Advocacy / Ministerial / Research Institute Image Folder
        /// </summary>
        public const string PersonPicturePathName = "gfx\\interface\\pics";

        /// <summary>
        ///     Technical image folder
        /// </summary>
        public const string TechPicturePathName = "gfx\\interface\\tech";

        /// <summary>
        ///     Unit model image folder
        /// </summary>
        public const string ModelPicturePathName = "gfx\\interface\\models";

        /// <summary>
        ///     Map folder name
        /// </summary>
        public const string MapPathName = "map";

        /// <summary>
        ///     Image folder name in the map folder
        /// </summary>
        public const string MapImagePathName = "gfx";

        /// <summary>
        ///     MOD folder name (DH)
        /// </summary>
        public const string ModPathNameDh = "Mods";

        /// <summary>
        ///     Scenario -offer
        /// </summary>
        public const string ScenarioPathName = "scenarios";

        /// <summary>
        ///     Scenario data folder
        /// </summary>
        public const string ScenarioDataPathName = "scenarios\\data";

        /// <summary>
        ///     Misc file name
        /// </summary>
        public const string MiscPathName = "db\\misc.txt";

        /// <summary>
        ///     Ministerial characteristics file name (Ao D)
        /// </summary>
        public const string MinisterPersonalityPathNameAoD = "db\\ministers\\minister_modifiers.txt";

        /// <summary>
        ///     Ministerial characteristic file name (DH)
        /// </summary>
        public const string MinisterPersonalityPathNameDh = "db\\ministers\\minister_personalities.txt";

        /// <summary>
        ///     Advocacy list file name (DH)
        /// </summary>
        public const string DhLeaderListPathName = "db\\leaders.txt";

        /// <summary>
        ///     Ministerial list file name (DH)
        /// </summary>
        public const string DhMinisterListPathName = "db\\ministers.txt";

        /// <summary>
        ///     Research institution list file name (DH)
        /// </summary>
        public const string DhTeamListPathName = "db\\teams.txt";

        /// <summary>
        ///     Events data folder
        /// </summary>
        public const string EventsPathName = "db\\events";

        /// <summary>
        ///     Province definition file name
        /// </summary>
        public const string ProvinceFileName = "province.csv";

        /// <summary>
        ///     Division unit class definition file name (DH)
        /// </summary>
        public const string DhDivisionTypePathName = "db\\units\\division_types.txt";

        /// <summary>
        ///     Brigade unit class definition file name (DH)
        /// </summary>
        public const string DhBrigadeTypePathName = "db\\units\\brigade_types.txt";

        /// <summary>
        ///     File name of research characteristic icon
        /// </summary>
        public const string TechIconPathName = "gfx\\interface\\tc_icons.bmp";

        /// <summary>
        ///     Research characteristic overlay icon file name
        /// </summary>
        public const string TechIconOverlayPathName = "gfx\\interface\\tc_icon_overlay.bmp";

        /// <summary>
        ///     Technical label file name
        /// </summary>
        public const string TechLabelPathName = "gfx\\interface\\button_tech_normal.bmp";

        /// <summary>
        ///     Completed technology label file name
        /// </summary>
        public const string DoneTechLabelPathName = "gfx\\interface\\button_tech_done.bmp";

        /// <summary>
        ///     Event label file name
        /// </summary>
        public const string SecretLabelPathName = "gfx\\interface\\button_tech_secret.bmp";

        /// <summary>
        ///     Blue photo icon file name
        /// </summary>
        public const string BlueprintIconPathName = "gfx\\interface\\icon_blueprints.bmp";

        /// <summary>
        ///     File name of technical character string definition
        /// </summary>
        public const string TechTextFileName = "tech_names.csv";

        /// <summary>
        ///     Unit character string definition file name
        /// </summary>
        public const string UnitTextFileName = "unit_names.csv";

        /// <summary>
        ///     File name of the model string definition by country
        /// </summary>
        public const string ModelTextFileName = "models.csv";

        /// <summary>
        ///     Province Definition file name
        /// </summary>
        public const string ProvinceTextFileName = "province_names.csv";

        /// <summary>
        ///     Place definition file name
        /// </summary>
        public const string WorldTextFileName = "world_names.csv";

        /// <summary>
        ///     Scenario character string definition file name
        /// </summary>
        public const string ScenarioTextFileName = "scenario_text.csv";

        /// <summary>
        ///     Map data file name (936x360)
        /// </summary>
        public const string LightMap1FileName = "lightmap1.tbl";

        /// <summary>
        ///     Map data file name (468x180)
        /// </summary>
        public const string LightMap2FileName = "lightmap2.tbl";

        /// <summary>
        ///     Map data file name (234x90)
        /// </summary>
        public const string LightMap3FileName = "lightmap3.tbl";

        /// <summary>
        ///     Map data file name (117x45)
        /// </summary>
        public const string LightMap4FileName = "lightmap4.tbl";

        /// <summary>
        ///     Province boundary definition file name
        /// </summary>
        public const string BoundBoxFileName = "boundbox.tbl";

        /// <summary>
        ///     Color scale table file name
        /// </summary>
        public const string ColorScalesFileName = "colorscales.csv";

        /// <summary>
        ///     Unit name definition file name
        /// </summary>
        public const string UnitNamesPathName = "db\\unitnames.csv";

        /// <summary>
        ///     Army Corps name definition file name
        /// </summary>
        public const string ArmyNamesPathName = "db\\armynames.csv";

        /// <summary>
        ///     Navy Corps name definition file name
        /// </summary>
        public const string NavyNamesPathName = "db\\navynames.csv";

        /// <summary>
        ///     Air Force Corps name definition file name
        /// </summary>
        public const string AirNamesPathName = "db\\airnames.csv";

        /// <summary>
        ///     Random commander's name definition file
        /// </summary>
        public const string RandomLeadersPathName = "db\\randomleaders.csv";

        /// <summary>
        ///     Standard definition file name
        /// </summary>
        public const string BasesIncFileName = "bases.inc";

        /// <summary>
        ///     Base definition file name (DH Full 33 year scenario)
        /// </summary>
        public const string BasesIncDodFileName = "bases_DOD.inc";

        /// <summary>
        ///     Resources storage definition file name
        /// </summary>
        public const string DepotsIncFileName = "depots.inc";

        /// <summary>
        ///     VP definition file name
        /// </summary>
        public const string VpIncFileName = "vp.inc";

        #endregion

        #region Internal fixed number

        /// <summary>
        ///     Game type string
        /// </summary>
        private static readonly string[] GameTypeStrings =
        {
            "Unknown",
            "Hearts of Iron 2",
            "Arsenal of Democracy",
            "Darkest Hour"
        };

        #endregion

        #region Path operation

        /// <summary>
        ///     Get the file name of the vanilla folder
        /// </summary>
        /// <param name="pathName">Pass name</param>
        /// <returns>file name</returns>
        public static string GetVanillaFileName(string pathName)
        {
            return Path.Combine(FolderName, pathName);
        }

        /// <summary>
        ///     Get the file name of the vanilla folder
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <param name="fileName">file name</param>
        /// <returns>file name</returns>
        public static string GetVanillaFileName(string folderName, string fileName)
        {
            string pathName = Path.Combine(folderName, fileName);
            return GetVanillaFileName(pathName);
        }

        /// <summary>
        ///     Get the file name of the mod folder
        /// </summary>
        /// <param name="pathName">Pass name</param>
        /// <returns>file name</returns>
        public static string GetModFileName(string pathName)
        {
            return Path.Combine(ModFolderName, pathName);
        }

        /// <summary>
        ///     Get the file name of the mod folder
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <param name="fileName">file name</param>
        /// <returns>file name</returns>
        public static string GetModFileName(string folderName, string fileName)
        {
            string pathName = Path.Combine(folderName, fileName);
            return GetModFileName(pathName);
        }

        /// <summary>
        ///     Get the file name of the saved folder
        /// </summary>
        /// <param name="pathName">Pass name</param>
        /// <returns>file name</returns>
        public static string GetExportFileName(string pathName)
        {
            return Path.Combine(ExportFolderName, pathName);
        }

        /// <summary>
        ///     Get the file name of the saved folder
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <param name="fileName">file name</param>
        /// <returns>file name</returns>
        public static string GetExportFileName(string folderName, string fileName)
        {
            string pathName = Path.Combine(folderName, fileName);
            return GetExportFileName(pathName);
        }

        /// <summary>
        ///     Get a file name for reading in consideration of MOD folder / saved folder
        /// </summary>
        /// <param name="pathName">Pass name</param>
        /// <returns>file name</returns>
        public static string GetReadFileName(string pathName)
        {
            if (IsExportFolderActive)
            {
                string fileName = GetExportFileName(pathName);
                if (File.Exists(fileName) || Directory.Exists(fileName))
                {
                    return fileName;
                }
            }
            if (IsModActive)
            {
                string fileName = GetModFileName(pathName);
                if (File.Exists(fileName) || Directory.Exists(fileName))
                {
                    return fileName;
                }
            }
            return GetVanillaFileName(pathName);
        }

        /// <summary>
        ///     Get a file name for reading in consideration of MOD folder / saved folder
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <param name="fileName">file name</param>
        /// <returns>file name</returns>
        public static string GetReadFileName(string folderName, string fileName)
        {
            string pathName = Path.Combine(folderName, fileName);
            return GetReadFileName(pathName);
        }

        /// <summary>
        ///     Get a file name for writing in consideration of MOD folder / saved folder
        /// </summary>
        /// <param name="pathName">Pass name</param>
        /// <returns>file name</returns>
        public static string GetWriteFileName(string pathName)
        {
            if (IsExportFolderActive)
            {
                return GetExportFileName(pathName);
            }
            if (IsModActive)
            {
                return GetModFileName(pathName);
            }
            return GetVanillaFileName(pathName);
        }

        /// <summary>
        ///     Get a file name for writing in consideration of MOD folder / saved folder
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <param name="fileName">file name</param>
        /// <returns>file name</returns>
        public static string GetWriteFileName(string folderName, string fileName)
        {
            string pathName = Path.Combine(folderName, fileName);
            return GetWriteFileName(pathName);
        }

        /// <summary>
        ///     Get the relative path name
        /// </summary>
        /// <param name="pathName">Pass name</param>
        /// <returns>Relative path name</returns>
        public static string GetRelativePathName(string pathName)
        {
            string name;
            if (IsExportFolderActive)
            {
                name = PathHelper.GetRelativePathName(pathName, ExportFolderName);
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
            if (IsModActive)
            {
                name = PathHelper.GetRelativePathName(pathName, ModFolderName);
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
            name = PathHelper.GetRelativePathName(pathName, FolderName);
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            return pathName;
        }

        /// <summary>
        ///     Get the relative path name
        /// </summary>
        /// <param name="pathName">Pass name</param>
        /// <param name="folderName">Standard folder name</param>
        /// <returns>Relative path name</returns>
        public static string GetRelativePathName(string pathName, string folderName)
        {
            string name;
            if (IsExportFolderActive)
            {
                name = PathHelper.GetRelativePathName(pathName, Path.Combine(ExportFolderName, folderName));
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
            if (IsModActive)
            {
                name = PathHelper.GetRelativePathName(pathName, Path.Combine(ModFolderName, folderName));
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
            name = PathHelper.GetRelativePathName(pathName, Path.Combine(FolderName, folderName));
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            return pathName;
        }

        /// <summary>
        ///     Get the commander's file name
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <returns>Commander file name</returns>
        public static string GetLeaderFileName(Country country)
        {
            return Leaders.FileNameMap.ContainsKey(country)
                ? Leaders.FileNameMap[country]
                : $"leaders{Countries.Strings[(int) country].ToUpper()}.csv";
        }

        /// <summary>
        ///     Get the ministerial file name
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <returns>Ministerial file name</returns>
        public static string GetMinisterFileName(Country country)
        {
            return Ministers.FileNameMap.ContainsKey(country)
                ? Ministers.FileNameMap[country]
                : $"ministers_{Countries.Strings[(int) country].ToLower()}.csv";
        }

        /// <summary>
        ///     Get a research institution file name
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <returns>Research institution file name</returns>
        public static string GetTeamFileName(Country country)
        {
            return Teams.FileNameMap.ContainsKey(country)
                ? Teams.FileNameMap[country]
                : $"teams_{Countries.Strings[(int) country].ToLower()}.csv";
        }

        /// <summary>
        ///     Get the Province definition folder name
        /// </summary>
        /// <returns>Province definition folder name</returns>
        public static string GetProvinceFolderName()
        {
            // Vanilla map
            if (Type != GameType.DarkestHour || Misc.MapNumber == 0)
            {
                return DatabasePathName;
            }

            // DH map extension
            return Path.Combine(MapPathName, $"Map_{Misc.MapNumber}");
        }

        /// <summary>
        ///     Get the Province name folder name
        /// </summary>
        /// <returns>Province name folder name</returns>
        public static string GetProvinceNameFolderName()
        {
            // Vanilla map
            if (Type != GameType.DarkestHour || Misc.MapNumber == 0)
            {
                return ConfigPathName;
            }

            // DH map extension
            return Path.Combine(MapPathName, $"Map_{Misc.MapNumber}");
        }

        /// <summary>
        ///     Get the Province image folder name
        /// </summary>
        /// <param name="id">Provin ID</param>
        /// <returns>Province image folder name</returns>
        public static string GetProvinceImageFileName(int id)
        {
            string folderName;
            if (Type != GameType.DarkestHour || Misc.MapNumber == 0)
            {
                // Vanilla Province Image folder
                folderName = PicturePathName;
            }
            else
            {
                // DH map extension
                folderName = Path.Combine(MapPathName, $"Map_{Misc.MapNumber}");
                folderName = Path.Combine(folderName, MapImagePathName);
            }

            return Path.Combine(folderName, $"ill_prov_{id}.bmp");
        }

        /// <summary>
        ///     Get the map folder name
        /// </summary>
        /// <returns>Map folder name</returns>
        public static string GetMapFolderName()
        {
            string folderName;
            if (Type != GameType.DarkestHour || Misc.MapNumber == 0)
            {
                folderName = MapPathName;
            }
            else
            {
                folderName = Path.Combine(MapPathName, $"Map_{Misc.MapNumber}");
            }
            return folderName;
        }

        /// <summary>
        ///     Update the mod folder name
        /// </summary>
        private static void UpdateModFolderName()
        {
            if (!IsGameFolderActive)
            {
                IsModActive = false;
                ModFolderName = "";
                return;
            }
            if (string.IsNullOrEmpty(_modName))
            {
                IsModActive = false;
                ModFolderName = FolderName;
                return;
            }
            IsModActive = true;
            switch (Type)
            {
                case GameType.DarkestHour:
                    ModFolderName = Path.Combine(Path.Combine(FolderName, ModPathNameDh), ModName);
                    break;

                default:
                    ModFolderName = Path.Combine(FolderName, ModName);
                    break;
            }
        }

        /// <summary>
        ///     Update the saved folder name
        /// </summary>
        private static void UpdateExportFolderName()
        {
            if (!IsGameFolderActive)
            {
                IsExportFolderActive = false;
                _exportFolderName = "";
                return;
            }
            if (string.IsNullOrEmpty(_exportName))
            {
                IsExportFolderActive = false;
                _exportFolderName = FolderName;
                return;
            }
            IsExportFolderActive = true;
            switch (Type)
            {
                case GameType.DarkestHour:
                    _exportFolderName = Path.Combine(Path.Combine(FolderName, ModPathNameDh), ExportName);
                    break;

                default:
                    _exportFolderName = Path.Combine(FolderName, ExportName);
                    break;
            }
        }

        #endregion

        #region Game type / version

        /// <summary>
        ///     Get if DH Full
        /// </summary>
        /// <returns>Returns true if DH Full</returns>
        public static bool IsDhFull()
        {
            return (Type == GameType.DarkestHour) && (Misc.MapNumber > 0);
        }

        /// <summary>
        ///     Automatically determine the type of game
        /// </summary>
        private static void DistinguishGameType()
        {
            if (string.IsNullOrEmpty(FolderName))
            {
                Type = GameType.None;
                return;
            }

            // DH
            string fileName = Path.Combine(FolderName, "Darkest Hour.exe");
            if (File.Exists(fileName))
            {
                Type = GameType.DarkestHour;
                _exeFileName = fileName;
                return;
            }

            // HoI2
            fileName = Path.Combine(FolderName, "Hoi2.exe");
            if (File.Exists(fileName))
            {
                Type = GameType.HeartsOfIron2;
                _exeFileName = fileName;
                return;
            }
            fileName = Path.Combine(FolderName, "DoomsdayJP.exe");
            if (File.Exists(fileName))
            {
                Type = GameType.HeartsOfIron2;
                _exeFileName = fileName;
                return;
            }

            // AoD
            fileName = Path.Combine(FolderName, "AODGame.exe");
            if (File.Exists(fileName))
            {
                Type = GameType.ArsenalOfDemocracy;
                _exeFileName = fileName;
                return;
            }

            Type = GameType.None;
        }

        /// <summary>
        ///     Automatically determine the game version
        /// </summary>
        private static void DistinguishGameVersion()
        {
            if (Type == GameType.None)
            {
                Version = 100;
                return;
            }

            // Read the binary column of the executable
            FileInfo info = new FileInfo(_exeFileName);
            long size = info.Length;
            byte[] data = new byte[size];

            FileStream s = info.OpenRead();
            s.Read(data, 0, (int) size);
            s.Close();

            // Search for version string
            byte[] pattern;
            List<uint> l;
            uint offset;
            switch (Type)
            {
                case GameType.HeartsOfIron2:
                    // Doomsday Armageddon v XX
                    pattern = new byte[]
                    {
                        0x44, 0x6F, 0x6F, 0x6D, 0x73, 0x64, 0x61, 0x79,
                        0x20, 0x41, 0x72, 0x6D, 0x61, 0x67, 0x65, 0x64,
                        0x64, 0x6F, 0x6E, 0x20, 0x76, 0x20
                    };
                    l = BinaryScan(data, pattern, 0, (uint) size);
                    if (l.Count == 0)
                    {
                        // Iron Cross Armageddon X.XX
                        pattern = new byte[]
                        {
                            0x49, 0x72, 0x6F, 0x6E, 0x20, 0x43, 0x72, 0x6F,
                            0x73, 0x73, 0x20, 0x41, 0x72, 0x6D, 0x61, 0x67,
                            0x65, 0x64, 0x64, 0x6F, 0x6E, 0x20
                        };
                        l = BinaryScan(data, pattern, 0, (uint) size);
                        if (l.Count == 0)
                        {
                            // In the case of a Japanese version, it is fixed to 1.2 because it is not possible to acquire the version.
                            Version = 120;
                            return;
                        }
                        offset = l[0] + (uint) pattern.Length;
                        Version = (data[offset] - '0') * 100 + (data[offset + 2] - '0') * 10 +
                                  (data[offset + 3] - '0');
                    }
                    else
                    {
                        offset = l[0] + (uint) pattern.Length;
                        Version = (data[offset] - '0') * 100 + (data[offset + 2] - '0') * 10;
                    }
                    break;

                case GameType.ArsenalOfDemocracy:
                    // Arsenal of Democracy X.XX
                    pattern = new byte[]
                    {
                        0x41, 0x72, 0x73, 0x65, 0x6E, 0x61, 0x6C, 0x20,
                        0x6F, 0x66, 0x20, 0x44, 0x65, 0x6D, 0x6F, 0x63,
                        0x72, 0x61, 0x63, 0x79, 0x20
                    };
                    l = BinaryScan(data, pattern, 0, (uint) size);
                    if (l.Count == 0)
                    {
                        // Arsenal Of Democracy v X.XX
                        pattern = new byte[]
                        {
                            0x41, 0x72, 0x73, 0x65, 0x6E, 0x61, 0x6C, 0x20,
                            0x4F, 0x66, 0x20, 0x44, 0x65, 0x6D, 0x6F, 0x63,
                            0x72, 0x61, 0x63, 0x79, 0x20, 0x76, 0x20
                        };
                        l = BinaryScan(data, pattern, 0, (uint) size);
                        if (l.Count == 0)
                        {
                            // If the version is not possible, fix it to 1.04
                            Version = 104;
                            return;
                        }
                    }
                    offset = l[0] + (uint) pattern.Length;
                    Version = (data[offset] - '0') * 100 + (data[offset + 2] - '0') * 10 + (data[offset + 3] - '0');
                    break;

                case GameType.DarkestHour:
                    // Darkest Hour v X.XX
                    pattern = new byte[]
                    {
                        0x44, 0x61, 0x72, 0x6B, 0x65, 0x73, 0x74, 0x20,
                        0x48, 0x6F, 0x75, 0x72, 0x20, 0x76, 0x20
                    };
                    l = BinaryScan(data, pattern, 0, (uint) size);
                    if (l.Count == 0)
                    {
                        // If the version is not possible, fix it to 1.02
                        Version = 102;
                        return;
                    }
                    offset = l[0] + (uint) pattern.Length;
                    Version = (data[offset] - '0') * 100 + (data[offset + 2] - '0') * 10 + (data[offset + 3] - '0');
                    break;

                default:
                    // Doomsday Armageddon v XX
                    pattern = new byte[]
                    {
                        0x44, 0x6F, 0x6F, 0x6D, 0x73, 0x64, 0x61, 0x79,
                        0x20, 0x41, 0x72, 0x6D, 0x61, 0x67, 0x65, 0x64,
                        0x64, 0x6F, 0x6E, 0x20, 0x76, 0x20
                    };
                    l = BinaryScan(data, pattern, 0, (uint) size);
                    if (l.Count == 0)
                    {
                        // In the case of a Japanese version, it is fixed to 1.2 because it is not possible to acquire the version.
                        Version = 120;
                        return;
                    }
                    offset = l[0] + (uint) pattern.Length;
                    Version = (data[offset] - '0') * 100 + (data[offset + 2] - '0') * 10;
                    break;
            }
        }

        /// <summary>
        ///     Explore binary columns
        /// </summary>
        /// <param name="target">Data to be searched</param>
        /// <param name="pattern">Byte pattern to explore</param>
        /// <param name="start">Start position</param>
        /// <param name="size">Byte size to explore</param>
        /// <returns>If you succeed in searching, return True</returns>
        private static List<uint> BinaryScan(byte[] target, byte[] pattern, uint start, uint size)
        {
            List<uint> result = new List<uint>();
            for (uint offset = start; offset <= start + size - pattern.Length; offset++)
            {
                if (IsBinaryMatch(target, pattern, offset))
                {
                    result.Add(offset);
                }
            }
            return result;
        }

        /// <summary>
        ///     Judge whether the binary column matches
        /// </summary>
        /// <param name="target">Data to be searched</param>
        /// <param name="pattern">Byte pattern to explore</param>
        /// <param name="offset">Position to judge</param>
        /// <returns>If the binary column matches, return True</returns>
        private static bool IsBinaryMatch(byte[] target, byte[] pattern, uint offset)
        {
            int i;
            for (i = 0; i < pattern.Length; i++)
            {
                if (target[offset + i] != pattern[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Output the type of game
        /// </summary>
        public static void OutputGameType()
        {
            // If the type is unknown, it will not output anything
            if (_type == GameType.None)
            {
                return;
            }

            Log.Error("Game Type: {0}", GameTypeStrings[(int) _type]);
        }

        /// <summary>
        ///     Output the game version
        /// </summary>
        public static void OutputGameVersion()
        {
            string s;
            switch (_type)
            {
                case GameType.HeartsOfIron2:
                    s = $"{_version / 100}.{_version % 100 / 10}";
                    break;

                case GameType.ArsenalOfDemocracy:
                case GameType.DarkestHour:
                    s = $"{_version / 100}.{_version % 100 / 10}{_version % 10}";
                    break;

                default:
                    // If the type is unknown, it will not output anything
                    return;
            }
            Log.Error("Game Version: {0}", s);
        }

        #endregion

        #region language

        /// <summary>
        ///     Automatically determine the language mode
        /// </summary>
        private static void DistinguishLanguageMode()
        {
            // If the game folder name is not set, it will not be determined
            if (string.IsNullOrEmpty(FolderName))
            {
                return;
            }

            // If there is no game folder, it will not be determined
            if (!Directory.Exists(FolderName))
            {
                return;
            }

            // If the type of game is unknown, it will not be determined
            if (Type == GameType.None)
            {
                return;
            }

            // If inmm.dll exists, the environment where the English version is patched
            if (File.Exists(Path.Combine(FolderName, "_inmm.dll")))
            {
                CultureInfo culture = Thread.CurrentThread.CurrentUICulture;
                // English version of Japanese language
                if (culture.Equals(CultureInfo.GetCultureInfo("ja-JP")))
                {
                    Config.LangMode = LanguageMode.PatchedJapanese;
                    return;
                }
                // English version Korean
                if (culture.Equals(CultureInfo.GetCultureInfo("ko-KR")))
                {
                    Config.LangMode = LanguageMode.PatchedKorean;
                    return;
                }
                // English version of traditional Chinese characters in Chinese
                if (culture.Equals(CultureInfo.GetCultureInfo("zh-TW")) ||
                    culture.Equals(CultureInfo.GetCultureInfo("zh-Hant")) ||
                    culture.Equals(CultureInfo.GetCultureInfo("zh-HK")) ||
                    culture.Equals(CultureInfo.GetCultureInfo("zh-MO")))
                {
                    Config.LangMode = LanguageMode.PatchedTraditionalChinese;
                    return;
                }
                // English version Simplified Chinese translation
                if (culture.Equals(CultureInfo.GetCultureInfo("zh-CN")) ||
                    culture.Equals(CultureInfo.GetCultureInfo("zh-Hans")) ||
                    culture.Equals(CultureInfo.GetCultureInfo("zh-SG")))
                {
                    Config.LangMode = LanguageMode.PatchedSimplifiedChinese;
                    return;
                }
            }

            // Japanese version if Doomsday JP.exe (Ho I2) /cyberfront.url (Ao D) exists
            if (File.Exists(Path.Combine(FolderName, "DoomsdayJP.exe")) ||
                File.Exists(Path.Combine(FolderName, "cyberfront.url")))
            {
                Config.LangMode = LanguageMode.Japanese;
                return;
            }

            // Other than that, English version
            Config.LangMode = LanguageMode.English;
        }

        #endregion
    }

    /// <summary>
    ///     Kind of game
    /// </summary>
    public enum GameType
    {
        None,
        HeartsOfIron2, // Hearts of Iron 2 (Doomsday Armageddon)
        ArsenalOfDemocracy, // Arsenal of Democracy
        DarkestHour // Darkest Hour
    }
}
