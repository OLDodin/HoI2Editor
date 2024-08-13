using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using HoI2Editor.Forms;
using HoI2Editor.Models;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor
{
    /// <summary>
    ///     Application controller class
    /// </summary>
    public static class HoI2EditorController
    {
        #region Editor version

        /// <summary>
        ///     Application name
        /// </summary>
        public const string Name = "Alternative HoI2 Editor";

        /// <summary>
        ///     Editor version
        /// </summary>
        public static string Version { get; private set; }

        /// <summary>
        ///     Initialize the editor version
        /// </summary>
        public static void InitVersion()
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            if (info.FilePrivatePart > 0 && info.FilePrivatePart <= 26)
            {
                Version =
                    $"{Name} Ver {info.FileMajorPart}.{info.FileMinorPart}{info.FileBuildPart}{'`' + info.FilePrivatePart}";
            }
            else
            {
                Version = $"{Name} Ver {info.FileMajorPart}.{info.FileMinorPart}{info.FileBuildPart}";
            }
        }

        #endregion

        #region Resource management

        /// <summary>
        ///     Resource manager
        /// </summary>
        private static readonly ResourceManager ResourceManager
            = new ResourceManager("HoI2Editor.Properties.Resources", typeof (Resources).Assembly);

        /// <summary>
        ///     Get resource string
        /// </summary>
        /// <param name="name">Resource name</param>
        public static string GetResourceString(string name)
        {
            return ResourceManager.GetString(name);
        }

        #endregion

        #region Data processing

        /// <summary>
        ///     Whether the preservation was canceled
        /// </summary>
        public static bool SaveCanceled { get; set; }

        /// <summary>
        ///     Get whether or not it has been edited
        /// </summary>
        /// <returns>If you have edited, return True</returns>
        public static bool IsDirty()
        {
            return Misc.IsDirty() ||
                   Config.IsDirty() ||
                   Leaders.IsDirty() ||
                   Ministers.IsDirty() ||
                   Teams.IsDirty() ||
                   Provinces.IsDirty() ||
                   Techs.IsDirty() ||
                   Units.IsDirty() ||
                   CorpsNames.IsDirty() ||
                   UnitNames.IsDirty() ||
                   RandomLeaders.IsDirty() ||
                   Scenarios.IsDirty();
        }

        /// <summary>
        ///     Request a file reload
        /// </summary>
        public static void RequestReload()
        {
            Misc.RequestReload();
            Config.RequestReload();
            Leaders.RequestReload();
            Ministers.RequestReload();
            Teams.RequestReload();
            Techs.RequestReload();
            Units.RequestReload();
            Provinces.RequestReload();
            CorpsNames.RequestReload();
            UnitNames.RequestReload();
            RandomLeaders.RequestReload();
            Scenarios.RequestReload();
            Maps.RequestReload();
            HoI2Editor.Models.Events.RequestReload();

            SaveCanceled = false;

            Log.Verbose("Request to reload");
        }

        /// <summary>
        ///     Reload data
        /// </summary>
        public static void Reload()
        {
            Log.Info("Reload");

            // Reload data
            Misc.Reload();
            Config.Reload();
            Leaders.Reload();
            Ministers.Reload();
            Teams.Reload();
            Provinces.Reload();
            Techs.Reload();
            Units.Reload();
            CorpsNames.Reload();
            UnitNames.Reload();
            RandomLeaders.Reload();
            Scenarios.Reload();
            HoI2Editor.Models.Events.Reload();

            // Update processing call after data reading
            OnFileLoaded();

            SaveCanceled = false;
        }

        /// <summary>
        ///     Save data
        /// </summary>
        public static void Save()
        {
            Log.Info("Save");

            // Change the temporary key of the character string to a preservation format
            Techs.RenameKeys();

            // Save edited data
            SaveFiles();

            // Update processing call after data saving
            OnFileSaved();

            SaveCanceled = false;
        }

        /// <summary>
        ///     Save data
        /// </summary>
        private static void SaveFiles()
        {
            if (!Misc.Save())
            {
                return;
            }
            if (!Config.Save())
            {
                return;
            }
            if (!Leaders.Save())
            {
                return;
            }
            if (!Ministers.Save())
            {
                return;
            }
            if (!Teams.Save())
            {
                return;
            }
            if (!Provinces.Save())
            {
                return;
            }
            if (!Techs.Save())
            {
                return;
            }
            if (!Units.Save())
            {
                return;
            }
            if (!CorpsNames.Save())
            {
                return;
            }
            if (!UnitNames.Save())
            {
                return;
            }
            if (!RandomLeaders.Save())
            {
                return;
            }
            Scenarios.Save();
        }

        /// <summary>
        ///     Update processing call after data reading
        /// </summary>
        private static void OnFileLoaded()
        {
            _leaderEditorForm?.OnFileLoaded();
            _ministerEditorForm?.OnFileLoaded();
            _teamEditorForm?.OnFileLoaded();
            _provinceEditorForm?.OnFileLoaded();
            _techEditorForm?.OnFileLoaded();
            _unitEditorForm?.OnFileLoaded();
            _miscEditorForm?.OnFileLoaded();
            _corpsNameEditorForm?.OnFileLoaded();
            _unitNameEditorForm?.OnFileLoaded();
            _modelNameEditorForm?.OnFileLoaded();
            _randomLeaderEditorForm?.OnFileLoaded();
            _researchViewerForm?.OnFileLoaded();
            _scenarioEditorForm?.OnFileLoaded();
        }

        /// <summary>
        ///     Update processing call after data saving
        /// </summary>
        private static void OnFileSaved()
        {
            _leaderEditorForm?.OnFileSaved();
            _ministerEditorForm?.OnFileSaved();
            _teamEditorForm?.OnFileSaved();
            _provinceEditorForm?.OnFileSaved();
            _techEditorForm?.OnFileSaved();
            _unitEditorForm?.OnFileSaved();
            _miscEditorForm?.OnFileSaved();
            _corpsNameEditorForm?.OnFileSaved();
            _unitNameEditorForm?.OnFileSaved();
            _modelNameEditorForm?.OnFileSaved();
            _randomLeaderEditorForm?.OnFileSaved();
            _scenarioEditorForm?.OnFileSaved();
        }

        /// <summary>
        ///     Update processing call after changing edit items
        /// </summary>
        /// <param name="id">Editing project ID</param>
        /// <param name="form">Call source form</param>
        public static void OnItemChanged(EditorItemId id, Form form)
        {
            if (form != _leaderEditorForm)
            {
                _leaderEditorForm?.OnItemChanged(id);
            }
            if (form != _ministerEditorForm)
            {
                _ministerEditorForm?.OnItemChanged(id);
            }
            if (form != _teamEditorForm)
            {
                _teamEditorForm?.OnItemChanged(id);
            }
            if (form != _provinceEditorForm)
            {
                _provinceEditorForm?.OnItemChanged(id);
            }
            if (form != _techEditorForm)
            {
                _techEditorForm?.OnItemChanged(id);
            }
            if (form != _unitEditorForm)
            {
                _unitEditorForm?.OnItemChanged(id);
            }
            if (form != _miscEditorForm)
            {
                _miscEditorForm?.OnItemChanged(id);
            }
            if (form != _corpsNameEditorForm)
            {
                _corpsNameEditorForm?.OnItemChanged(id);
            }
            if (form != _unitNameEditorForm)
            {
                _unitNameEditorForm?.OnItemChanged(id);
            }
            if (form != _modelNameEditorForm)
            {
                _modelNameEditorForm?.OnItemChanged(id);
            }
            if (form != _randomLeaderEditorForm)
            {
                _randomLeaderEditorForm?.OnItemChanged(id);
            }
            _researchViewerForm?.OnItemChanged(id);
            if (form != _scenarioEditorForm)
            {
                _scenarioEditorForm?.OnItemChanged(id);
            }
        }

        /// <summary>
        ///     Update processing call after delayed reading
        /// </summary>
        public static void OnLoadingCompleted()
        {
            if (!ExistsEditorForms() && !IsLoadingData())
            {
                _mainForm.EnableFolderChange();
            }
            else
            {
                _mainForm.DisableFolderChange();
            }
        }

        /// <summary>
        ///     Determine whether the data is delayed
        /// </summary>
        /// <returns>Return True if you are reading the data delayed</returns>
        private static bool IsLoadingData()
        {
            return Leaders.IsLoading() ||
                   Ministers.IsLoading() ||
                   Teams.IsLoading() ||
                   Provinces.IsLoading() ||
                   Techs.IsLoading() ||
                   Units.IsLoading() ||
                   HoI2Editor.Models.Events.IsLoading() ||
                   Maps.IsLoading();
        }

        #endregion

        #region Editorform management

        /// <summary>
        ///     Main form
        /// </summary>
        private static MainForm _mainForm;

        /// <summary>
        ///     Commander editor form
        /// </summary>
        private static LeaderEditorForm _leaderEditorForm;

        /// <summary>
        ///     Ministerial editor form
        /// </summary>
        private static MinisterEditorForm _ministerEditorForm;

        /// <summary>
        ///     Research institution editor form
        /// </summary>
        private static TeamEditorForm _teamEditorForm;

        /// <summary>
        ///     Provin Editor form
        /// </summary>
        private static ProvinceEditorForm _provinceEditorForm;

        /// <summary>
        ///     Technical tree editor form
        /// </summary>
        private static TechEditorForm _techEditorForm;

        /// <summary>
        ///     Unit model editor form
        /// </summary>
        private static UnitEditorForm _unitEditorForm;

        /// <summary>
        ///     Game setting editor form
        /// </summary>
        private static MiscEditorForm _miscEditorForm;

        /// <summary>
        ///     Corps name editor form
        /// </summary>
        private static CorpsNameEditorForm _corpsNameEditorForm;

        /// <summary>
        ///     Unit name editor form
        /// </summary>
        private static UnitNameEditorForm _unitNameEditorForm;

        /// <summary>
        ///     Model name editor form
        /// </summary>
        private static ModelNameEditorForm _modelNameEditorForm;

        /// <summary>
        ///     Random commander editor form
        /// </summary>
        private static RandomLeaderEditorForm _randomLeaderEditorForm;

        /// <summary>
        ///     Research speed viewer form
        /// </summary>
        private static ResearchViewerForm _researchViewerForm;

        /// <summary>
        ///     Scenario editor form
        /// </summary>
        private static ScenarioEditorForm _scenarioEditorForm;

        /// <summary>
        ///     Event editor form
        /// </summary>
        private static LocHelperForm _locHelperForm;

        /// <summary>
        ///     Start the main form
        /// </summary>
        public static void LaunchMainForm()
        {
            _mainForm = new MainForm();
            Application.Run(_mainForm);
        }

        /// <summary>
        ///     Start the commander's editor form
        /// </summary>
        public static void LaunchLeaderEditorForm()
        {
            if (_leaderEditorForm == null)
            {
                _leaderEditorForm = new LeaderEditorForm();
                _leaderEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _leaderEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start the ministerial editor form
        /// </summary>
        public static void LaunchMinisterEditorForm()
        {
            if (_ministerEditorForm == null)
            {
                _ministerEditorForm = new MinisterEditorForm();
                _ministerEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _ministerEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start the research institution editor form
        /// </summary>
        public static void LaunchTeamEditorForm()
        {
            if (_teamEditorForm == null)
            {
                _teamEditorForm = new TeamEditorForm();
                _teamEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _teamEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start Province Editor Form
        /// </summary>
        public static void LaunchProvinceEditorForm()
        {
            if (_provinceEditorForm == null)
            {
                _provinceEditorForm = new ProvinceEditorForm();
                _provinceEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _provinceEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start the Technical Tree Editor Form
        /// </summary>
        public static void LaunchTechEditorForm()
        {
            if (_techEditorForm == null)
            {
                _techEditorForm = new TechEditorForm();
                _techEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _techEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start the unit model editorform
        /// </summary>
        public static void LaunchUnitEditorForm()
        {
            if (_unitEditorForm == null)
            {
                _unitEditorForm = new UnitEditorForm();
                _unitEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _unitEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start the game setting editor form
        /// </summary>
        public static void LaunchMiscEditorForm()
        {
            if (_miscEditorForm == null)
            {
                _miscEditorForm = new MiscEditorForm();
                _miscEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _miscEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start the corps name editorform
        /// </summary>
        public static void LaunchCorpsNameEditorForm()
        {
            if (_corpsNameEditorForm == null)
            {
                _corpsNameEditorForm = new CorpsNameEditorForm();
                _corpsNameEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _corpsNameEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start the unit name editorform
        /// </summary>
        public static void LaunchUnitNameEditorForm()
        {
            if (_unitNameEditorForm == null)
            {
                _unitNameEditorForm = new UnitNameEditorForm();
                _unitNameEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _unitNameEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start the model name editorform
        /// </summary>
        public static void LaunchModelNameEditorForm()
        {
            if (_modelNameEditorForm == null)
            {
                _modelNameEditorForm = new ModelNameEditorForm();
                _modelNameEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _modelNameEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start the random commander's editor form
        /// </summary>
        public static void LaunchRandomLeaderEditorForm()
        {
            if (_randomLeaderEditorForm == null)
            {
                _randomLeaderEditorForm = new RandomLeaderEditorForm();
                _randomLeaderEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _randomLeaderEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start research speed view aphom
        /// </summary>
        public static void LaunchResearchViewerForm()
        {
            if (_researchViewerForm == null)
            {
                _researchViewerForm = new ResearchViewerForm();
                _researchViewerForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _researchViewerForm.Activate();
            }
        }

        /// <summary>
        ///     Start the scenario editorform
        /// </summary>
        public static void LaunchScenarioEditorForm()
        {
            if (_scenarioEditorForm == null)
            {
                _scenarioEditorForm = new ScenarioEditorForm();
                _scenarioEditorForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _scenarioEditorForm.Activate();
            }
        }

        /// <summary>
        ///     Start the event editor form
        /// </summary>
        public static void LaunchLocHelperForm()
        {
            if (_locHelperForm == null)
            {
                _locHelperForm = new LocHelperForm();
                _locHelperForm.Show();

                OnEditorStatusUpdate();
            }
            else
            {
                _locHelperForm.Activate();
            }
        }

        /// <summary>
        ///     Commander Editor Form Close processing
        /// </summary>
        public static void OnLeaderEditorFormClosed()
        {
            _leaderEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Processing for the ministerial editor form close
        /// </summary>
        public static void OnMinisterEditorFormClosed()
        {
            _ministerEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Research institution editorform processing for closing lose
        /// </summary>
        public static void OnTeamEditorFormClosed()
        {
            _teamEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Province Editor Form Close Processing
        /// </summary>
        public static void OnProvinceEditorFormClosed()
        {
            _provinceEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Technical Tree Editor Form Close Processing
        /// </summary>
        public static void OnTechEditorFormClosed()
        {
            _techEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Unit model editorform processing for closing lose
        /// </summary>
        public static void OnUnitEditorFormClosed()
        {
            _unitEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Game setting editor form processing when closed
        /// </summary>
        public static void OnMiscEditorFormClosed()
        {
            _miscEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Corps name editorform processing for closing lose
        /// </summary>
        public static void OnCorpsNameEditorFormClosed()
        {
            _corpsNameEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Unit name editorform processing when closed
        /// </summary>
        public static void OnUnitNameEditorFormClosed()
        {
            _unitNameEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Model name editorform processing for closing lose
        /// </summary>
        public static void OnModelNameEditorFormClosed()
        {
            _modelNameEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Random commander Editorform Close processing
        /// </summary>
        public static void OnRandomLeaderEditorFormClosed()
        {
            _randomLeaderEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Research speed View Affomed Close processing
        /// </summary>
        public static void OnResearchViewerFormClosed()
        {
            _researchViewerForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Scenario editorform processing for closing lose
        /// </summary>
        public static void OnScenarioEditorFormClosed()
        {
            _scenarioEditorForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Event editorform processing for closing lose
        /// </summary>
        public static void OnLocHelperFormClosed()
        {
            _locHelperForm = null;

            OnEditorStatusUpdate();
        }

        /// <summary>
        ///     Processing when updating the state of the editor
        /// </summary>
        private static void OnEditorStatusUpdate()
        {
            if (!ExistsEditorForms() && !IsLoadingData())
            {
                _mainForm.EnableFolderChange();
            }
            else
            {
                _mainForm.DisableFolderChange();
            }
        }

        /// <summary>
        ///     Judge whether an editor form exists
        /// </summary>
        /// <returns>If there is an editor form, return True</returns>
        private static bool ExistsEditorForms()
        {
            return _leaderEditorForm != null ||
                   _ministerEditorForm != null ||
                   _teamEditorForm != null ||
                   _provinceEditorForm != null ||
                   _techEditorForm != null ||
                   _unitEditorForm != null ||
                   _miscEditorForm != null ||
                   _corpsNameEditorForm != null ||
                   _unitNameEditorForm != null ||
                   _modelNameEditorForm != null ||
                   _randomLeaderEditorForm != null ||
                   _researchViewerForm != null ||
                   _scenarioEditorForm != null ||
                   _locHelperForm != null;
        }

        #endregion

        #region Multi-compilation prohibition

        /// <summary>
        ///     Mutex for multiple editing ban
        /// </summary>
        private const string MutextName = "Alternative HoI2 Editor";

        private static Mutex _mutex;

        /// <summary>
        ///     Lock mutex
        /// </summary>
        /// <param name="key">Key string</param>
        /// <returns>If you succeed in locking, return True</returns>
        public static bool LockMutex(string key)
        {
            // If you have already locked mutex, release it
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex = null;
            }

            _mutex = new Mutex(false, $"{MutextName}: {key.GetHashCode()}");

            if (!_mutex.WaitOne(0, false))
            {
                _mutex = null;
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Unlock Mutex
        /// </summary>
        public static void UnlockMutex()
        {
            if (_mutex == null)
            {
                return;
            }

            _mutex.ReleaseMutex();
            _mutex = null;
        }

        #endregion

        #region Set value management

        /// <summary>
        ///     Setting file name
        /// </summary>
        private const string SettingsFileName = "HoI2Editor.settings";

        /// <summary>
        ///     Set value
        /// </summary>
        public static HoI2EditorSettings Settings { get; private set; }

        /// <summary>
        ///     Read the configuration value
        /// </summary>
        public static void LoadSettings()
        {
            if (!File.Exists(SettingsFileName))
            {
                Settings = new HoI2EditorSettings();
                //Set the log output level to Information
                Log.Level = 3;
            }
            else
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof (HoI2EditorSettings));
                    using (FileStream fs = new FileStream(SettingsFileName, FileMode.Open, FileAccess.Read))
                    {
                        Settings = serializer.Deserialize(fs) as HoI2EditorSettings;
                        if (Settings == null)
                        {
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    Log.Error("[Settings] Read error");
                    Settings = new HoI2EditorSettings();
                }
            }
            Settings.Round();
        }

        /// <summary>
        ///     Save the set value
        /// </summary>
        public static void SaveSettings()
        {
            if (Settings == null)
            {
                return;
            }
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof (HoI2EditorSettings));
                using (FileStream fs = new FileStream(SettingsFileName, FileMode.Create, FileAccess.Write))
                {
                    serializer.Serialize(fs, Settings);
                }
            }
            catch (Exception)
            {
                Log.Error("[Settings] Write error");
            }
        }

        #endregion
    }

    /// <summary>
    ///     Edit item ID of the editor
    /// </summary>
    public enum EditorItemId
    {
        LeaderRetirementYear, // Retirement year setting of commander
        MinisterEndYear, // Ministerial End Year settings
        MinisterRetirementYear, // Retirement year setting of ministers
        TeamList, // Research institution list
        TeamCountry, // Research institution affiliated country
        TeamName, // Research institution name
        TeamId, // Research institution ID
        TeamSkill, // Research institution skills
        TeamSpeciality, // Research institution characteristics
        TechItemList, // Technical item list
        TechItemName, // Technical project name
        TechItemId, // Technical project ID
        TechItemYear, // Fiscal year of technical items
        TechComponentList, // Small research list
        TechComponentSpeciality, // Characteristics of small research
        TechComponentDifficulty, // Difficulty of small research
        TechComponentDoubleTime, // Double time setting of small research
        UnitName, // Unit class name
        MaxAllowedBrigades, // Maximum attached number of brigades
        ModelList, // Unit model list
        CommonModelName, // Common unit model name
        CountryModelName // Country unit model name
    }
}
