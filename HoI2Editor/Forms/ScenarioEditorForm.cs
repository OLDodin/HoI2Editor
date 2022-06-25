using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Controllers;
using HoI2Editor.Models;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Scenario editor form
    /// </summary>
    public partial class ScenarioEditorForm : Form
    {
        #region Internal field

        /// <summary>
        ///     Scenario editor controller
        /// </summary>
        private ScenarioEditorController _controller;

        /// <summary>
        ///     Tab page number
        /// </summary>
        private TabPageNo _tabPageNo;

        /// <summary>
        ///     Tab page initialization flag
        /// </summary>
        private readonly bool[] _tabPageInitialized = new bool[Enum.GetValues(typeof (TabPageNo)).Length];

        /// <summary>
        ///     Edit items ID And control association
        /// </summary>
        private readonly Dictionary<ScenarioEditorItemId, Control> _itemControls =
            new Dictionary<ScenarioEditorItemId, Control>();

        /// <summary>
        ///     List of selectable countries other than major countries
        /// </summary>
        private List<Country> _majorFreeCountries;

        /// <summary>
        ///     List of countries other than selectable countries
        /// </summary>
        private List<Country> _selectableFreeCountries;

        /// <summary>
        ///     List of non-allied nations
        /// </summary>
        private List<Country> _allianceFreeCountries;

        /// <summary>
        ///     List of non-war nations
        /// </summary>
        private List<Country> _warFreeCountries;

        /// <summary>
        ///     Technical item list
        /// </summary>
        private List<TechItem> _techs;

        /// <summary>
        ///     Invention event list
        /// </summary>
        private List<TechEvent> _inventions;

        /// <summary>
        ///     Technology tree panel controller
        /// </summary>
        private TechTreePanelController _techTreePanelController;

        /// <summary>
        ///     Map panel controller
        /// </summary>
        private MapPanelController _mapPanelController;

        /// <summary>
        ///     Map panel initialization flag
        /// </summary>
        private bool _mapPanelInitialized;

        /// <summary>
        ///     Unit tree controller
        /// </summary>
        private UnitTreeController _unitTreeController;

        /// <summary>
        ///     Selected nation
        /// </summary>
        private Country _selectedCountry;

        /// <summary>
        ///     The last unit's military department
        /// </summary>
        private Branch _lastUnitBranch;

        /// <summary>
        ///     The final division's military department
        /// </summary>
        private Branch _lastDivisionBranch;

        #endregion

        #region Internal constant

        /// <summary>
        ///     Tab page number
        /// </summary>
        private enum TabPageNo
        {
            Main, // Main
            Alliance, // alliance
            Relation, // relationship
            Trade, // Trade
            Country, // Nation
            Government, // government
            Technology, // Technology
            Province, // Providence
            Oob // Early troops
        }

        /// <summary>
        ///     AI Aggressive string name
        /// </summary>
        private readonly TextId[] _aiAggressiveNames =
        {
            TextId.OptionAiAggressiveness1,
            TextId.OptionAiAggressiveness2,
            TextId.OptionAiAggressiveness3,
            TextId.OptionAiAggressiveness4,
            TextId.OptionAiAggressiveness5
        };

        /// <summary>
        ///     Difficulty string name
        /// </summary>
        private readonly TextId[] _difficultyNames =
        {
            TextId.OptionDifficulty1,
            TextId.OptionDifficulty2,
            TextId.OptionDifficulty3,
            TextId.OptionDifficulty4,
            TextId.OptionDifficulty5
        };

        /// <summary>
        ///     Game speed string name
        /// </summary>
        private readonly TextId[] _gameSpeedNames =
        {
            TextId.OptionGameSpeed0,
            TextId.OptionGameSpeed1,
            TextId.OptionGameSpeed2,
            TextId.OptionGameSpeed3,
            TextId.OptionGameSpeed4,
            TextId.OptionGameSpeed5,
            TextId.OptionGameSpeed6,
            TextId.OptionGameSpeed7
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public ScenarioEditorForm()
        {
            InitializeComponent();

            // Form initialization
            InitForm();
        }

        #endregion

        #region Data processing

        /// <summary>
        ///     Processing after reading data
        /// </summary>
        public void OnFileLoaded()
        {
            // Do nothing before loading
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // Initialize scenario-related information
            Scenarios.Init();

            // Clear the initialized state of each tab page
            foreach (TabPageNo page in Enum.GetValues(typeof (TabPageNo)))
            {
                _tabPageInitialized[(int) page] = false;
            }

            // Update edit items
            OnMainTabPageFileLoad();
            OnAllianceTabPageFileLoad();
            OnRelationTabPageFileLoad();
            OnTradeTabPageFileLoad();
            OnCountryTabPageFileLoad();
            OnGovernmentTabPageFileLoad();
            OnTechTabPageFileLoad();
            OnProvinceTabPageFileLoad();
            OnOobTabPageFileLoad();
        }

        /// <summary>
        ///     Processing after data storage
        /// </summary>
        public void OnFileSaved()
        {
            // Clear the initialized state of each tab page
            foreach (TabPageNo page in Enum.GetValues(typeof (TabPageNo)))
            {
                _tabPageInitialized[(int) page] = false;
            }

            // Forcibly refresh the display of the selection tab
            OnScenarioTabControlSelectedIndexChanged(null, null);
        }

        /// <summary>
        ///     Processing after changing edit items
        /// </summary>
        /// <param name="id">Edit items ID</param>
        public void OnItemChanged(EditorItemId id)
        {
            // do nothing
        }

        /// <summary>
        ///     Processing when map loading is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapFileLoad(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }

            if (e.Cancelled)
            {
                return;
            }

            // Do nothing unless Providence stub is selected
            if (_tabPageNo != TabPageNo.Province)
            {
                return;
            }

            // Update the map panel
            UpdateMapPanel();
        }

        #endregion

        #region Form

        /// <summary>
        ///     Form initialization
        /// </summary>
        private void InitForm()
        {
            // Window position
            Location = HoI2EditorController.Settings.ScenarioEditor.Location;
            Size = HoI2EditorController.Settings.ScenarioEditor.Size;

            // Technology tree panel
            _techTreePanelController = new TechTreePanelController(techTreePictureBox) { ApplyItemStatus = true };
            _techTreePanelController.ItemMouseClick += OnTechTreeItemMouseClick;
            _techTreePanelController.QueryItemStatus += OnQueryTechTreeItemStatus;

            // Map panel
            _mapPanelController = new MapPanelController(provinceMapPanel, provinceMapPictureBox);

            // Unit tree
            _unitTreeController = new UnitTreeController(unitTreeView);

            // controller
            _controller = new ScenarioEditorController(this, _mapPanelController, _unitTreeController);
        }

        /// <summary>
        ///     Processing when loading a form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormLoad(object sender, EventArgs e)
        {
            // Initialize national data
            Countries.Init();

            // Initialize ministerial characteristics
            Ministers.InitPersonality();

            // Initialize unit data
            Units.Init();

            // Initialize province data
            Provinces.Init();

            // Load the game settings file
            Misc.Load();

            // Read the character string definition file
            Config.Load();

            // Delay loading the map
            Maps.LoadAsync(MapLevel.Level2, OnMapFileLoad);

            // Delay loading commander data
            Leaders.LoadAsync(null);

            // Delay reading of ministerial data
            Ministers.LoadAsync(null);

            // Delay reading technical data
            Techs.LoadAsync(null);

            // Delay reading of provision data
            Provinces.LoadAsync(null);

            // Delay reading unit data
            Units.LoadAsync(null);

            // Initialize display items
            OnMainTabPageFormLoad();
            OnAllianceTabPageFormLoad();
            OnRelationTabPageFormLoad();
            OnTradeTabPageFormLoad();
            OnCountryTabPageFormLoad();
            OnGovernmentTabPageFormLoad();
            OnTechTabPageFormLoad();
            OnProvinceTabPageFormLoad();
            OnOobTabPageFormLoad();

            // Update the edit items if the scenario file has already been read
            if (Scenarios.IsLoaded())
            {
                OnFileLoaded();
            }
        }

        /// <summary>
        ///     Processing when closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            // Close form if not edited
            if (!HoI2EditorController.IsDirty())
            {
                return;
            }

            // Ask if you want to save
            DialogResult result = MessageBox.Show(Resources.ConfirmSaveMessage, Text, MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
            switch (result)
            {
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
                case DialogResult.Yes:
                    HoI2EditorController.Save();
                    break;
                case DialogResult.No:
                    HoI2EditorController.SaveCanceled = true;
                    break;
            }
        }

        /// <summary>
        ///     Processing after closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            HoI2EditorController.OnScenarioEditorFormClosed();
        }

        /// <summary>
        ///     Processing when moving the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormMove(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                HoI2EditorController.Settings.ScenarioEditor.Location = Location;
            }
        }

        /// <summary>
        ///     Processing at the time of form resizing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                HoI2EditorController.Settings.ScenarioEditor.Size = Size;
            }
        }

        /// <summary>
        ///     Processing when the check button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCheckButtonClick(object sender, EventArgs e)
        {
            // Wait until the provision data reading is completed
            Provinces.WaitLoading();

            DataChecker.CheckScenario();
        }

        /// <summary>
        ///     Processing when the reload button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReloadButtonClick(object sender, EventArgs e)
        {
            // Ask if you want to save it if edited
            if (HoI2EditorController.IsDirty())
            {
                DialogResult result = MessageBox.Show(Resources.ConfirmSaveMessage, Text, MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.Yes:
                        HoI2EditorController.Save();
                        break;
                }
            }

            HoI2EditorController.Reload();
        }

        /// <summary>
        ///     Processing when the save button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.Save();
        }

        /// <summary>
        ///     Processing when the close button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Processing when changing the selection tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScenarioTabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            _tabPageNo = (TabPageNo) scenarioTabControl.SelectedIndex;

            switch (_tabPageNo)
            {
                case TabPageNo.Main:
                    OnMainTabPageSelected();
                    break;

                case TabPageNo.Alliance:
                    OnAllianceTabPageSelected();
                    break;

                case TabPageNo.Relation:
                    OnRelationTabPageSelected();
                    break;

                case TabPageNo.Trade:
                    OnTradeTabPageSelected();
                    break;

                case TabPageNo.Country:
                    OnCountryTabPageSelected();
                    break;

                case TabPageNo.Government:
                    OnGovernmentTabPageSelected();
                    break;

                case TabPageNo.Technology:
                    OnTechTabPageSelected();
                    break;

                case TabPageNo.Province:
                    OnProvinceTabPageSelected();
                    break;

                case TabPageNo.Oob:
                    OnOobTabPageSelected();
                    break;
            }
        }

        #endregion

        #region Main tab

        #region Main tab ―――― common

        /// <summary>
        ///     Initialize the edit items on the main tab
        /// </summary>
        private void InitMainTab()
        {
            InitScenarioListBox();
            InitScenarioInfoItems();
            InitScenarioOptionItems();
            InitSelectableItems();
        }

        /// <summary>
        ///     Update the edit items on the main tab
        /// </summary>
        private void UpdateMainTab()
        {
            // Do nothing if initialized
            if (_tabPageInitialized[(int) TabPageNo.Main])
            {
                return;
            }

            // Update edit items
            UpdateScenarioInfoItems();
            UpdateScenarioOptionItems();

            // Update the list of selectable countries
            UpdateSelectableList();

            // Enable edit items on the main tab
            EnableMainItems();

            // Set the initialized flag
            _tabPageInitialized[(int) TabPageNo.Main] = true;
        }

        /// <summary>
        ///     Enable edit items on the main tab
        /// </summary>
        private void EnableMainItems()
        {
            scenarioInfoGroupBox.Enabled = true;
            scenarioOptionGroupBox.Enabled = true;
            countrySelectionGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Processing when loading a form on the main tab
        /// </summary>
        private void OnMainTabPageFormLoad()
        {
            // Initialize the main tab
            InitMainTab();
        }

        /// <summary>
        ///     Processing when reading a file on the main tab
        /// </summary>
        private void OnMainTabPageFileLoad()
        {
            // Do nothing unless the main tab is selected
            if (_tabPageNo != TabPageNo.Main)
            {
                return;
            }

            // Update the display at the first transition
            UpdateMainTab();
        }

        /// <summary>
        ///     Processing when selecting the main tab
        /// </summary>
        private void OnMainTabPageSelected()
        {
            // Do nothing if scenario not loaded
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // Update the display at the first transition
            UpdateMainTab();
        }

        #endregion

        #region Main tab ―――― Scenario loading

        /// <summary>
        ///     Initialize the scenario list box
        /// </summary>
        private void InitScenarioListBox()
        {
            // Enable any radio button in the folder group box
            vanillaRadioButton.Checked = true;
            if (Game.IsModActive && Directory.Exists(Game.GetModFileName(Game.ScenarioPathName)))
            {
                modRadioButton.Checked = true;
            }
            else
            {
                modRadioButton.Enabled = false;
            }
            if (Game.IsExportFolderActive && Directory.Exists(Game.GetExportFileName(Game.ScenarioPathName)))
            {
                exportRadioButton.Checked = true;
            }
            else
            {
                exportRadioButton.Enabled = false;
            }
        }

        /// <summary>
        ///     Update the display of the scenario list box
        /// </summary>
        private void UpdateScenarioListBox()
        {
            scenarioListBox.Items.Clear();

            string folderName;
            if (exportRadioButton.Checked)
            {
                folderName = Game.GetExportFileName(Game.ScenarioPathName);
            }
            else if (modRadioButton.Checked)
            {
                folderName = Game.GetModFileName(Game.ScenarioPathName);
            }
            else
            {
                folderName = Game.GetVanillaFileName(Game.ScenarioPathName);
            }

            // Back if there is no scenario folder
            if (!Directory.Exists(folderName))
            {
                return;
            }

            // eug Add files in order
            string[] fileNames = Directory.GetFiles(folderName, "*.eug");
            foreach (string fileName in fileNames)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    scenarioListBox.Items.Add(Path.GetFileName(fileName));
                }
            }

            // Select the first item
            if (scenarioListBox.Items.Count > 0)
            {
                scenarioListBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        ///     Processing when the load button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoadButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection in the scenario list box
            if (scenarioListBox.SelectedIndex < 0)
            {
                return;
            }

            // Ask if you want to save it if edited
            if (Scenarios.IsLoaded() && HoI2EditorController.IsDirty())
            {
                DialogResult result = MessageBox.Show(Resources.ConfirmSaveMessage, Text, MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.Yes:
                        HoI2EditorController.Save();
                        break;
                }
            }

            string fileName = scenarioListBox.Items[scenarioListBox.SelectedIndex].ToString();
            string pathName;
            if (exportRadioButton.Checked)
            {
                pathName = Game.GetExportFileName(Game.ScenarioPathName, fileName);
            }
            else if (modRadioButton.Checked)
            {
                pathName = Game.GetModFileName(Game.ScenarioPathName, fileName);
            }
            else
            {
                pathName = Game.GetVanillaFileName(Game.ScenarioPathName, fileName);
            }

            // Read the scenario file
            if (File.Exists(pathName))
            {
                Scenarios.Load(pathName);
            }

            // Processing after reading data
            OnFileLoaded();
        }

        /// <summary>
        ///     Checking the folder radio button Processing when the status changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFolderRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            RadioButton button = sender as RadioButton;
            if (button != null && button.Checked)
            {
                UpdateScenarioListBox();
            }
        }

        #endregion

        #region Main tab ―――― Scenario information

        /// <summary>
        ///     Initialize the edit items of the scenario information
        /// </summary>
        private void InitScenarioInfoItems()
        {
            _itemControls.Add(ScenarioEditorItemId.ScenarioName, scenarioNameTextBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioPanelName, panelImageTextBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioStartYear, startYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioStartMonth, startMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioStartDay, startDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioEndYear, endYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioEndMonth, endMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioEndDay, endDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioIncludeFolder, includeFolderTextBox);

            scenarioNameTextBox.Tag = ScenarioEditorItemId.ScenarioName;
            panelImageTextBox.Tag = ScenarioEditorItemId.ScenarioPanelName;
            startYearTextBox.Tag = ScenarioEditorItemId.ScenarioStartYear;
            startMonthTextBox.Tag = ScenarioEditorItemId.ScenarioStartMonth;
            startDayTextBox.Tag = ScenarioEditorItemId.ScenarioStartDay;
            endYearTextBox.Tag = ScenarioEditorItemId.ScenarioEndYear;
            endMonthTextBox.Tag = ScenarioEditorItemId.ScenarioEndMonth;
            endDayTextBox.Tag = ScenarioEditorItemId.ScenarioEndDay;
            includeFolderTextBox.Tag = ScenarioEditorItemId.ScenarioIncludeFolder;
        }

        /// <summary>
        ///     Update the edit items of the scenario information
        /// </summary>
        private void UpdateScenarioInfoItems()
        {
            _controller.UpdateItemValue(scenarioNameTextBox);
            _controller.UpdateItemValue(panelImageTextBox);
            _controller.UpdateItemValue(startYearTextBox);
            _controller.UpdateItemValue(startMonthTextBox);
            _controller.UpdateItemValue(startDayTextBox);
            _controller.UpdateItemValue(endYearTextBox);
            _controller.UpdateItemValue(endMonthTextBox);
            _controller.UpdateItemValue(endDayTextBox);
            _controller.UpdateItemValue(includeFolderTextBox);

            _controller.UpdateItemColor(scenarioNameTextBox);
            _controller.UpdateItemColor(panelImageTextBox);
            _controller.UpdateItemColor(startYearTextBox);
            _controller.UpdateItemColor(startMonthTextBox);
            _controller.UpdateItemColor(startDayTextBox);
            _controller.UpdateItemColor(endYearTextBox);
            _controller.UpdateItemColor(endMonthTextBox);
            _controller.UpdateItemColor(endDayTextBox);
            _controller.UpdateItemColor(includeFolderTextBox);

            UpdatePanelImage(Scenarios.Data.PanelName);
        }

        /// <summary>
        ///     Processing when the panel image name reference button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPanelImageBrowseButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;

            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = Game.GetReadFileName(Game.ScenarioDataPathName),
                FileName = scenario.PanelName,
                Filter = Resources.OpenBitmapFileDialogFilter
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                panelImageTextBox.Text = Game.GetRelativePathName(dialog.FileName);
            }
        }

        /// <summary>
        ///     Processing when the include folder browse button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIncludeFolderBrowseButtonClick(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                SelectedPath = Game.GetReadFileName(Game.ScenarioDataPathName),
                ShowNewFolderButton = true
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                includeFolderTextBox.Text = Game.GetRelativePathName(dialog.SelectedPath, Game.ScenarioPathName);
            }
        }

        /// <summary>
        ///     Update panel image
        /// </summary>
        /// <param name="fileName">file name</param>
        public void UpdatePanelImage(string fileName)
        {
            Image prev = panelPictureBox.Image;
            if (!string.IsNullOrEmpty(fileName) &&
                (fileName.IndexOfAny(Path.GetInvalidPathChars()) < 0))
            {
                string pathName = Game.GetReadFileName(fileName);
                if (File.Exists(pathName))
                {
                    Bitmap bitmap = new Bitmap(pathName);
                    bitmap.MakeTransparent(Color.Lime);
                    panelPictureBox.Image = bitmap;
                }
                else
                {
                    panelPictureBox.Image = null;
                }
            }
            else
            {
                panelPictureBox.Image = null;
            }
            prev?.Dispose();
        }

        #endregion

        #region Main tab ――――option

        /// <summary>
        ///     Initialize the edit items of the scenario option
        /// </summary>
        private void InitScenarioOptionItems()
        {
            _itemControls.Add(ScenarioEditorItemId.ScenarioBattleScenario, battleScenarioCheckBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioFreeSelection, freeCountryCheckBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioAllowDiplomacy, allowDiplomacyCheckBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioAllowProduction, allowProductionCheckBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioAllowTechnology, allowTechnologyCheckBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioAiAggressive, aiAggressiveComboBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioDifficulty, difficultyComboBox);
            _itemControls.Add(ScenarioEditorItemId.ScenarioGameSpeed, gameSpeedComboBox);

            battleScenarioCheckBox.Tag = ScenarioEditorItemId.ScenarioBattleScenario;
            freeCountryCheckBox.Tag = ScenarioEditorItemId.ScenarioFreeSelection;
            allowDiplomacyCheckBox.Tag = ScenarioEditorItemId.ScenarioAllowDiplomacy;
            allowProductionCheckBox.Tag = ScenarioEditorItemId.ScenarioAllowProduction;
            allowTechnologyCheckBox.Tag = ScenarioEditorItemId.ScenarioAllowTechnology;
            aiAggressiveComboBox.Tag = ScenarioEditorItemId.ScenarioAiAggressive;
            difficultyComboBox.Tag = ScenarioEditorItemId.ScenarioDifficulty;
            gameSpeedComboBox.Tag = ScenarioEditorItemId.ScenarioGameSpeed;

            // AI Aggression combo box
            aiAggressiveComboBox.BeginUpdate();
            aiAggressiveComboBox.Items.Clear();
            for (int i = 0; i < ScenarioHeader.AiAggressiveCount; i++)
            {
                aiAggressiveComboBox.Items.Add(Config.GetText(_aiAggressiveNames[i]));
            }
            aiAggressiveComboBox.EndUpdate();

            // Difficulty combo box
            difficultyComboBox.BeginUpdate();
            difficultyComboBox.Items.Clear();
            for (int i = 0; i < ScenarioHeader.DifficultyCount; i++)
            {
                difficultyComboBox.Items.Add(Config.GetText(_difficultyNames[i]));
            }
            difficultyComboBox.EndUpdate();

            // Game speed combo box
            gameSpeedComboBox.BeginUpdate();
            gameSpeedComboBox.Items.Clear();
            for (int i = 0; i < ScenarioHeader.GameSpeedCount; i++)
            {
                gameSpeedComboBox.Items.Add(Config.GetText(_gameSpeedNames[i]));
            }
            gameSpeedComboBox.EndUpdate();
        }

        /// <summary>
        ///     Update scenario option edits
        /// </summary>
        private void UpdateScenarioOptionItems()
        {
            _controller.UpdateItemValue(battleScenarioCheckBox);
            _controller.UpdateItemValue(freeCountryCheckBox);
            _controller.UpdateItemValue(allowDiplomacyCheckBox);
            _controller.UpdateItemValue(allowProductionCheckBox);
            _controller.UpdateItemValue(allowTechnologyCheckBox);
            _controller.UpdateItemValue(aiAggressiveComboBox);
            _controller.UpdateItemValue(difficultyComboBox);
            _controller.UpdateItemValue(gameSpeedComboBox);

            _controller.UpdateItemColor(battleScenarioCheckBox);
            _controller.UpdateItemColor(freeCountryCheckBox);
            _controller.UpdateItemColor(allowDiplomacyCheckBox);
            _controller.UpdateItemColor(allowProductionCheckBox);
            _controller.UpdateItemColor(allowTechnologyCheckBox);
            _controller.UpdateItemColor(aiAggressiveComboBox);
            _controller.UpdateItemColor(difficultyComboBox);
            _controller.UpdateItemColor(gameSpeedComboBox);

            aiAggressiveComboBox.Refresh();
            difficultyComboBox.Refresh();
            gameSpeedComboBox.Refresh();
        }

        #endregion

        #region Main tab ―――― National choice

        /// <summary>
        ///     Update the list of selectable countries
        /// </summary>
        private void UpdateSelectableList()
        {
            List<Country> majors = Scenarios.Data.Header.MajorCountries.Select(major => major.Country).ToList();
            majorListBox.BeginUpdate();
            majorListBox.Items.Clear();
            foreach (Country country in majors)
            {
                majorListBox.Items.Add(Countries.GetTagName(country));
            }
            majorListBox.EndUpdate();

            _majorFreeCountries =
                Scenarios.Data.Header.SelectableCountries.Where(country => !majors.Contains(country)).ToList();
            selectableListBox.BeginUpdate();
            selectableListBox.Items.Clear();
            foreach (Country country in _majorFreeCountries)
            {
                selectableListBox.Items.Add(Countries.GetTagName(country));
            }
            selectableListBox.EndUpdate();

            _selectableFreeCountries =
                Countries.Tags.Where(country => !Scenarios.Data.Header.SelectableCountries.Contains(country)).ToList();
            unselectableListBox.BeginUpdate();
            unselectableListBox.Items.Clear();
            foreach (Country country in _selectableFreeCountries)
            {
                unselectableListBox.Items.Add(Countries.GetTagName(country));
            }
            unselectableListBox.EndUpdate();

            // Disable operation buttons in major countries
            DisableMajorButtons();

            // Disable edit items
            DisableSelectableItems();

            // Clear edit items
            ClearSelectableItems();
        }

        /// <summary>
        ///     Initialize edit items for selectable countries
        /// </summary>
        private void InitSelectableItems()
        {
            _itemControls.Add(ScenarioEditorItemId.MajorCountryNameKey, majorCountryNameKeyTextBox);
            _itemControls.Add(ScenarioEditorItemId.MajorCountryNameString, majorCountryNameStringTextBox);
            _itemControls.Add(ScenarioEditorItemId.MajorFlagExt, majorFlagExtTextBox);
            _itemControls.Add(ScenarioEditorItemId.MajorCountryDescKey, countryDescKeyTextBox);
            _itemControls.Add(ScenarioEditorItemId.MajorCountryDescString, countryDescStringTextBox);
            _itemControls.Add(ScenarioEditorItemId.MajorPropaganada, propagandaTextBox);

            majorCountryNameKeyTextBox.Tag = ScenarioEditorItemId.MajorCountryNameKey;
            majorCountryNameStringTextBox.Tag = ScenarioEditorItemId.MajorCountryNameString;
            majorFlagExtTextBox.Tag = ScenarioEditorItemId.MajorFlagExt;
            countryDescKeyTextBox.Tag = ScenarioEditorItemId.MajorCountryDescKey;
            countryDescStringTextBox.Tag = ScenarioEditorItemId.MajorCountryDescString;
            propagandaTextBox.Tag = ScenarioEditorItemId.MajorPropaganada;
        }

        /// <summary>
        ///     Update selectable country edits
        /// </summary>
        /// <param name="major">Major country setting</param>
        private void UpdateSelectableItems(MajorCountrySettings major)
        {
            _controller.UpdateItemValue(majorCountryNameKeyTextBox, major);
            _controller.UpdateItemValue(majorCountryNameStringTextBox, major);
            _controller.UpdateItemValue(majorFlagExtTextBox, major);
            _controller.UpdateItemValue(countryDescKeyTextBox, major);
            _controller.UpdateItemValue(countryDescStringTextBox, major);
            _controller.UpdateItemValue(propagandaTextBox, major);

            _controller.UpdateItemColor(majorCountryNameKeyTextBox, major);
            _controller.UpdateItemColor(majorCountryNameStringTextBox, major);
            _controller.UpdateItemColor(majorFlagExtTextBox, major);
            _controller.UpdateItemColor(countryDescKeyTextBox, major);
            _controller.UpdateItemColor(countryDescStringTextBox, major);
            _controller.UpdateItemColor(propagandaTextBox, major);

            UpdatePropagandaImage(major.Country, major.PictureName);
        }

        /// <summary>
        ///     Clear edit items for selectable countries
        /// </summary>
        private void ClearSelectableItems()
        {
            majorCountryNameKeyTextBox.Text = "";
            majorCountryNameStringTextBox.Text = "";
            majorFlagExtTextBox.Text = "";
            countryDescKeyTextBox.Text = "";
            countryDescStringTextBox.Text = "";
            propagandaTextBox.Text = "";
            Image prev = propagandaPictureBox.Image;
            propagandaPictureBox.Image = null;
            prev?.Dispose();
        }

        /// <summary>
        ///     Enable edit items for selectable countries
        /// </summary>
        private void EnableSelectableItems()
        {
            majorCountryNameLabel.Enabled = true;
            majorCountryNameKeyTextBox.Enabled = (Game.Type == GameType.DarkestHour) && (Game.Version >= 104);
            majorCountryNameStringTextBox.Enabled = true;
            majorFlagExtLabel.Enabled = (Game.Type == GameType.DarkestHour) && (Game.Version >= 104);
            majorFlagExtTextBox.Enabled = (Game.Type == GameType.DarkestHour) && (Game.Version >= 104);
            countryDescLabel.Enabled = true;
            countryDescKeyTextBox.Enabled = true;
            countryDescStringTextBox.Enabled = true;
            propagandaLabel.Enabled = true;
            propagandaTextBox.Enabled = true;
            propagandaBrowseButton.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for selectable countries
        /// </summary>
        private void DisableSelectableItems()
        {
            majorCountryNameLabel.Enabled = false;
            majorCountryNameKeyTextBox.Enabled = false;
            majorCountryNameStringTextBox.Enabled = false;
            majorFlagExtLabel.Enabled = false;
            majorFlagExtTextBox.Enabled = false;
            countryDescLabel.Enabled = false;
            countryDescKeyTextBox.Enabled = false;
            countryDescStringTextBox.Enabled = false;
            propagandaLabel.Enabled = false;
            propagandaTextBox.Enabled = false;
            propagandaBrowseButton.Enabled = false;
        }

        /// <summary>
        ///     Enable operation buttons in major countries
        /// </summary>
        private void EnableMajorButtons()
        {
            int index = majorListBox.SelectedIndex;
            int count = majorListBox.Items.Count;

            majorRemoveButton.Enabled = true;
            majorUpButton.Enabled = index > 0;
            majorDownButton.Enabled = index < count - 1;
        }

        /// <summary>
        ///     Disable operation buttons in major countries
        /// </summary>
        private void DisableMajorButtons()
        {
            majorRemoveButton.Enabled = false;
            majorUpButton.Enabled = false;
            majorDownButton.Enabled = false;
        }

        /// <summary>
        ///     Enable the operation buttons of the selected country
        /// </summary>
        private void EnableSelectableButtons()
        {
            majorAddButton.Enabled = true;
            selectableRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     Disable the operation buttons of the selected country
        /// </summary>
        private void DisableSelectableButtons()
        {
            majorAddButton.Enabled = false;
            selectableRemoveButton.Enabled = false;
        }

        /// <summary>
        ///     Enable operation buttons for non-selected countries
        /// </summary>
        private void EnableUnselectableButtons()
        {
            selectableAddButton.Enabled = true;
        }

        /// <summary>
        ///     Disable operation buttons for non-selected countries
        /// </summary>
        private void DisableUnselectableButtons()
        {
            selectableAddButton.Enabled = false;
        }

        /// <summary>
        ///     Item drawing process of major country list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMajorListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index < 0)
            {
                return;
            }

            ListBox control = sender as ListBox;
            if (control == null)
            {
                return;
            }

            Scenario scenario = Scenarios.Data;
            List<MajorCountrySettings> majors = scenario.Header.MajorCountries;

            // Draw the background
            e.DrawBackground();

            // Draw an item
            Brush brush;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // Change the text color for items that have changed
                bool dirty = scenario.IsDirtySelectableCountry(majors[e.Index].Country);
                brush = new SolidBrush(dirty ? Color.Red : control.ForeColor);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of selectable country list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectableListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index < 0)
            {
                return;
            }

            ListBox control = sender as ListBox;
            if (control == null)
            {
                return;
            }

            Scenario scenario = Scenarios.Data;

            // Draw the background
            e.DrawBackground();

            // Draw an item
            Brush brush;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // Change the text color for items that have changed
                bool dirty = scenario.IsDirtySelectableCountry(_majorFreeCountries[e.Index]);
                brush = new SolidBrush(dirty ? Color.Red : control.ForeColor);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of non-selected country list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnselectableListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index < 0)
            {
                return;
            }

            ListBox control = sender as ListBox;
            if (control == null)
            {
                return;
            }

            Scenario scenario = Scenarios.Data;

            // Draw the background
            e.DrawBackground();

            // Draw an item
            Brush brush;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // Change the text color for items that have changed
                bool dirty = scenario.IsDirtySelectableCountry(_selectableFreeCountries[e.Index]);
                brush = new SolidBrush(dirty ? Color.Red : control.ForeColor);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item in the major country list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMajorListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable edit items if there are no selections
            if (majorListBox.SelectedIndex < 0)
            {
                // Disable operation buttons in major countries
                DisableMajorButtons();

                // Disable edit items
                DisableSelectableItems();

                // Clear edit items
                ClearSelectableItems();
                return;
            }

            MajorCountrySettings major = GetSelectedMajorCountry();

            // Update edit items
            UpdateSelectableItems(major);

            // Enable edit items
            EnableSelectableItems();

            // Enable operation buttons in major countries
            EnableMajorButtons();
        }

        /// <summary>
        ///     Processing when changing the selection item in the selectable country list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectableListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectableListBox.SelectedItems.Count == 0)
            {
                // Disable the operation buttons of the selected country
                DisableSelectableButtons();
                return;
            }

            // Enable the operation buttons of the selected country
            EnableSelectableButtons();
        }

        /// <summary>
        ///     Processing when changing the selected item in the non-selected country list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnselectableListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (unselectableListBox.SelectedItems.Count == 0)
            {
                // Disable operation buttons for non-selected countries
                DisableUnselectableButtons();
                return;
            }

            // Enable operation buttons for non-selected countries
            EnableUnselectableButtons();
        }

        /// <summary>
        ///     Processing when the button is pressed on the major countries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMajorUpButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<MajorCountrySettings> majors = scenario.Header.MajorCountries;

            // Move items in the list of major countries
            int index = majorListBox.SelectedIndex;
            MajorCountrySettings major = majors[index];
            majors.RemoveAt(index);
            majors.Insert(index - 1, major);

            // Move items in the major country list box
            majorListBox.SelectedIndexChanged -= OnMajorListBoxSelectedIndexChanged;
            majorListBox.Items.RemoveAt(index);
            majorListBox.Items.Insert(index - 1, Countries.GetTagName(major.Country));
            majorListBox.SelectedIndexChanged += OnMajorListBoxSelectedIndexChanged;
            majorListBox.SelectedIndex = index - 1;

            // Set the edited flag
            scenario.SetDirtySelectableCountry(major.Country);
            Scenarios.SetDirty();
        }

        /// <summary>
        ///     Processing when the button is pressed under the major countries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMajorDownButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<MajorCountrySettings> majors = scenario.Header.MajorCountries;

            // Move items in the list of major countries
            int index = majorListBox.SelectedIndex;
            MajorCountrySettings major = majors[index];
            majors.RemoveAt(index);
            majors.Insert(index + 1, major);

            // Move items in the major country list box
            majorListBox.SelectedIndexChanged -= OnMajorListBoxSelectedIndexChanged;
            majorListBox.Items.RemoveAt(index);
            majorListBox.Items.Insert(index + 1, Countries.GetTagName(major.Country));
            majorListBox.SelectedIndexChanged += OnMajorListBoxSelectedIndexChanged;
            majorListBox.SelectedIndex = index + 1;

            // Set the edited flag
            scenario.SetDirtySelectableCountry(major.Country);
            Scenarios.SetDirty();
        }

        /// <summary>
        ///     Processing when the major country addition button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMajorAddButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            ScenarioHeader header = scenario.Header;

            List<Country> countries =
                (from int index in selectableListBox.SelectedIndices select _majorFreeCountries[index]).ToList();
            majorListBox.BeginUpdate();
            selectableListBox.BeginUpdate();
            foreach (Country country in countries)
            {
                // Add to major country list box
                majorListBox.Items.Add(Countries.GetTagName(country));

                // Add to list of major countries
                MajorCountrySettings major = new MajorCountrySettings { Country = country };
                header.MajorCountries.Add(major);

                // Remove from selectable country list box
                int index = _majorFreeCountries.IndexOf(country);
                selectableListBox.Items.RemoveAt(index);
                _majorFreeCountries.RemoveAt(index);

                // Set the edited flag
                scenario.SetDirtySelectableCountry(country);
                Scenarios.SetDirty();

                Log.Info("[Scenario] major country: +{0}", Countries.Strings[(int) country]);
            }
            majorListBox.EndUpdate();
            selectableListBox.EndUpdate();

            // Select the item you added to the major country list box
            majorListBox.SelectedIndex = majorListBox.Items.Count - 1;
        }

        /// <summary>
        ///     Processing when the major country delete button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMajorRemoveButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            ScenarioHeader header = scenario.Header;
            int index = majorListBox.SelectedIndex;
            Country country = header.MajorCountries[index].Country;

            // Set the edited flag
            scenario.SetDirtySelectableCountry(country);
            Scenarios.SetDirty();

            // Remove from major country list box
            majorListBox.SelectedIndexChanged -= OnMajorListBoxSelectedIndexChanged;
            majorListBox.Items.RemoveAt(index);

            // Select the following items in the major country list box
            if (majorListBox.Items.Count > 0)
            {
                majorListBox.SelectedIndex = index < majorListBox.Items.Count ? index : index - 1;
            }

            majorListBox.SelectedIndexChanged += OnMajorListBoxSelectedIndexChanged;

            // Remove from major country list
            header.MajorCountries.RemoveAt(index);

            // Call the event handler to update the selection
            OnMajorListBoxSelectedIndexChanged(sender, e);

            // Add to selectable country list box
            index = _majorFreeCountries.FindIndex(c => c > country);
            if (index < 0)
            {
                index = _majorFreeCountries.Count;
            }
            selectableListBox.Items.Insert(index, Countries.GetTagName(country));
            _majorFreeCountries.Insert(index, country);

            Log.Info("[Scenario] major country: -{0}", Countries.Strings[(int) country]);

            // Update button status
            if (majorListBox.Items.Count == 0)
            {
                majorRemoveButton.Enabled = false;
            }
        }

        /// <summary>
        ///     Processing when the selectable country addition button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectableAddButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            ScenarioHeader header = scenario.Header;

            List<Country> countries =
                (from int index in unselectableListBox.SelectedIndices select _selectableFreeCountries[index]).ToList();
            selectableListBox.BeginUpdate();
            unselectableListBox.BeginUpdate();
            foreach (Country country in countries)
            {
                // Add to selectable country list box
                int index = _majorFreeCountries.FindIndex(c => c > country);
                if (index < 0)
                {
                    index = _majorFreeCountries.Count;
                }
                selectableListBox.Items.Insert(index, Countries.GetTagName(country));
                _majorFreeCountries.Insert(index, country);

                // Add to selectable country list
                index = header.SelectableCountries.FindIndex(c => c > country);
                if (index < 0)
                {
                    index = header.SelectableCountries.Count;
                }
                header.SelectableCountries.Insert(index, country);

                // Remove from non-selected country list box
                index = _selectableFreeCountries.IndexOf(country);
                unselectableListBox.Items.RemoveAt(index);
                _selectableFreeCountries.RemoveAt(index);

                // Set the edited flag
                scenario.SetDirtySelectableCountry(country);
                Scenarios.SetDirty();

                Log.Info("[Scenario] selectable country: +{0}", Countries.Strings[(int) country]);
            }
            selectableListBox.EndUpdate();
            unselectableListBox.EndUpdate();
        }

        /// <summary>
        ///     Processing when the selectable country delete button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectableRemoveButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            ScenarioHeader header = scenario.Header;

            List<Country> countries =
                (from int index in selectableListBox.SelectedIndices select _majorFreeCountries[index]).ToList();
            selectableListBox.BeginUpdate();
            unselectableListBox.BeginUpdate();
            foreach (Country country in countries)
            {
                // Add to non-selected country list box
                int index = _selectableFreeCountries.FindIndex(c => c > country);
                if (index < 0)
                {
                    index = _selectableFreeCountries.Count;
                }
                unselectableListBox.Items.Insert(index, Countries.GetTagName(country));
                _selectableFreeCountries.Insert(index, country);

                // Remove from selectable country list box
                index = _majorFreeCountries.IndexOf(country);
                selectableListBox.Items.RemoveAt(index);
                _majorFreeCountries.RemoveAt(index);

                // Remove from selectable country list
                header.SelectableCountries.Remove(country);

                // Set the edited flag
                scenario.SetDirtySelectableCountry(country);
                Scenarios.SetDirty();

                Log.Info("[Scenario] selectable country: -{0}", Countries.Strings[(int) country]);
            }
            selectableListBox.EndUpdate();
            unselectableListBox.EndUpdate();
        }

        /// <summary>
        ///     Processing when the propaganda image name reference button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropagandaBrowseButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<MajorCountrySettings> majors = scenario.Header.MajorCountries;
            MajorCountrySettings major = majors[majorListBox.SelectedIndex];

            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = Game.GetReadFileName(Game.ScenarioDataPathName),
                FileName = major.PictureName,
                Filter = Resources.OpenBitmapFileDialogFilter
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                propagandaTextBox.Text = Game.GetRelativePathName(dialog.FileName);
            }
        }

        /// <summary>
        ///     Get the selected major country settings
        /// </summary>
        /// <returns>Selected major country settings</returns>
        private MajorCountrySettings GetSelectedMajorCountry()
        {
            if (majorListBox.SelectedIndex < 0)
            {
                return null;
            }
            return Scenarios.Data.Header.MajorCountries[majorListBox.SelectedIndex];
        }

        /// <summary>
        ///     Update propaganda images
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <param name="fileName">Propaganda image name</param>
        public void UpdatePropagandaImage(Country country, string fileName)
        {
            Image prev = propagandaPictureBox.Image;
            propagandaPictureBox.Image = GetPropagandaImage(country, fileName);
            prev?.Dispose();
        }

        /// <summary>
        ///     Get national propaganda images
        /// </summary>
        /// <param name="country">Country tag</param>
        /// <param name="fileName">Propaganda image name</param>
        /// <returns>Propaganda image</returns>
        private static Image GetPropagandaImage(Country country, string fileName)
        {
            Bitmap bitmap;
            string pathName;
            if (!string.IsNullOrEmpty(fileName) &&
                (fileName.IndexOfAny(Path.GetInvalidPathChars()) < 0))
            {
                pathName = Game.GetReadFileName(fileName);
                if (File.Exists(pathName))
                {
                    bitmap = new Bitmap(pathName);
                    bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
                    return bitmap;
                }
            }

            pathName = Game.GetReadFileName(Game.ScenarioDataPathName,
                $"propaganda_{Countries.Strings[(int) country]}.bmp");
            if (!File.Exists(pathName))
            {
                return null;
            }

            bitmap = new Bitmap(pathName);
            bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
            return bitmap;
        }

        #endregion

        #region Main tab ―――― Edit items

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScenarioIntItemTextBoxValidated(object sender, EventArgs e)
        {
            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control);
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val);

            // Update value
            _controller.SetItemValue(itemId, val);

            // Set the edited flag
            _controller.SetItemDirty(itemId);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val);
        }

        /// <summary>
        ///     Processing when changing the value of a text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScenarioStringItemTextBoxTextChanged(object sender, EventArgs e)
        {
            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Do nothing if the value does not change
            string val = control.Text;
            if (val.Equals((string) _controller.GetItemValue(itemId)))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val);

            // Update value
            _controller.SetItemValue(itemId, val);

            // Set the edited flag
            _controller.SetItemDirty(itemId);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val);
        }

        /// <summary>
        ///     Item drawing process of combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScenarioItemComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;
            int val = (int) _controller.GetItemValue(itemId);
            bool dirty = (e.Index == val) && _controller.IsItemDirty(itemId);
            Brush brush = new SolidBrush(dirty ? Color.Red : control.ForeColor);
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScenarioItemComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }

            // Do nothing if there is no selection
            if (control.SelectedIndex < 0)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Do nothing if the value does not change
            int val = control.SelectedIndex;
            if (val == (int) _controller.GetItemValue(itemId))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val);

            // Update value
            _controller.SetItemValue(itemId, val);

            // Set the edited flag
            _controller.SetItemDirty(itemId);

            // Update drawing to change item color
            control.Refresh();

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val);
        }

        /// <summary>
        ///     Processing when changing the check status of a check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScenarioItemCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Do nothing if the value does not change
            bool val = control.Checked;
            object prev = _controller.GetItemValue(itemId);
            if ((prev != null) && (val == (bool) prev))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val);

            // Update value
            _controller.SetItemValue(itemId, val);

            // Set the edited flag
            _controller.SetItemDirty(itemId);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val);
        }

        /// <summary>
        ///     Processing when changing the value of a text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectableStringItemTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            MajorCountrySettings major = GetSelectedMajorCountry();
            if (major == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Do nothing if the value does not change
            string val = control.Text;
            if (val.Equals((string) _controller.GetItemValue(itemId, major)))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, major);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, major);

            // Update value
            _controller.SetItemValue(itemId, val, major);

            // Set the edited flag
            _controller.SetItemDirty(itemId, major);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, major);
        }

        #endregion

        #endregion

        #region Alliance tab

        #region Alliance tab ―――― common

        /// <summary>
        ///     Initialize the edit items on the Alliance tab
        /// </summary>
        private void InitAllianceTab()
        {
            InitAllianceItems();
            InitWarItems();
        }

        /// <summary>
        ///     Update the edit items on the Alliance tab
        /// </summary>
        private void UpdateAllianceTab()
        {
            // Do nothing if initialized
            if (_tabPageInitialized[(int) TabPageNo.Alliance])
            {
                return;
            }

            // Update the alliance list
            UpdateAllianceList();

            // Update the war list
            UpdateWarList();

            // Activate the alliance list
            EnableAllianceList();

            // Activate the war list
            EnableWarList();

            // Set the initialized flag
            _tabPageInitialized[(int) TabPageNo.Alliance] = true;
        }

        /// <summary>
        ///     Processing when loading a form on the Alliance tab
        /// </summary>
        private void OnAllianceTabPageFormLoad()
        {
            // Initialize the Alliance tab
            InitAllianceTab();
        }

        /// <summary>
        ///     Processing when reading a file on the Alliance tab
        /// </summary>
        private void OnAllianceTabPageFileLoad()
        {
            // Do nothing unless Alliance tab is selected
            if (_tabPageNo != TabPageNo.Alliance)
            {
                return;
            }

            // Update the display at the first transition
            UpdateAllianceTab();
        }

        /// <summary>
        ///     Processing when selecting the Alliance tab
        /// </summary>
        private void OnAllianceTabPageSelected()
        {
            // Do nothing if scenario not loaded
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // Update the display at the first transition
            UpdateAllianceTab();
        }

        #endregion

        #region Alliance tab ―――― alliance

        /// <summary>
        ///     Initialize alliance edits
        /// </summary>
        private void InitAllianceItems()
        {
            _itemControls.Add(ScenarioEditorItemId.AllianceName, allianceNameTextBox);
            _itemControls.Add(ScenarioEditorItemId.AllianceType, allianceTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.AllianceId, allianceIdTextBox);

            allianceNameTextBox.Tag = ScenarioEditorItemId.AllianceName;
            allianceTypeTextBox.Tag = ScenarioEditorItemId.AllianceType;
            allianceIdTextBox.Tag = ScenarioEditorItemId.AllianceId;
        }

        /// <summary>
        ///     Update alliance edits
        /// </summary>
        /// <param name="alliance">alliance</param>
        private void UpdateAllianceItems(Alliance alliance)
        {
            // Update the value of the edit item
            _controller.UpdateItemValue(allianceNameTextBox, alliance);
            _controller.UpdateItemValue(allianceTypeTextBox, alliance);
            _controller.UpdateItemValue(allianceIdTextBox, alliance);

            // Update the color of the edit item
            _controller.UpdateItemColor(allianceNameTextBox, alliance);
            _controller.UpdateItemColor(allianceTypeTextBox, alliance);
            _controller.UpdateItemColor(allianceIdTextBox, alliance);

            // Update the list of alliance member countries
            UpdateAllianceParticipant(alliance);
        }

        /// <summary>
        ///     Clear alliance edits
        /// </summary>
        private void ClearAllianceItems()
        {
            // Clear edit items
            allianceNameTextBox.Text = "";
            allianceTypeTextBox.Text = "";
            allianceIdTextBox.Text = "";

            // Clear the list of alliance member countries
            ClearAllianceParticipant();
        }

        /// <summary>
        ///     Activate alliance edits
        /// </summary>
        private void EnableAllianceItems()
        {
            int index = allianceListView.SelectedIndices[0];

            // Axis powers / / Allied / / Can only be renamed in communal countries
            allianceNameLabel.Enabled = index < 3;
            allianceNameTextBox.Enabled = index < 3;
            allianceIdLabel.Enabled = true;
            allianceTypeTextBox.Enabled = true;
            allianceIdTextBox.Enabled = true;

            // Activate the list of alliance members
            EnableAllianceParticipant();
        }

        /// <summary>
        ///     Disable alliance edits
        /// </summary>
        private void DisableAllianceItems()
        {
            allianceNameLabel.Enabled = false;
            allianceNameTextBox.Enabled = false;
            allianceIdLabel.Enabled = false;
            allianceTypeTextBox.Enabled = false;
            allianceIdTextBox.Enabled = false;

            // Disable the list of alliance members
            DisableAllianceParticipant();
        }

        #endregion

        #region Alliance tab ―――― Alliance list

        /// <summary>
        ///     Update the alliance list
        /// </summary>
        private void UpdateAllianceList()
        {
            ScenarioGlobalData data = Scenarios.Data.GlobalData;

            allianceListView.BeginUpdate();
            allianceListView.Items.Clear();

            // Axis country
            ListViewItem item = new ListViewItem
            {
                Text = (string) _controller.GetItemValue(ScenarioEditorItemId.AllianceName, data.Axis),
                Tag = data.Axis
            };
            item.SubItems.Add(data.Axis != null ? Countries.GetNameList(data.Axis.Participant) : "");
            allianceListView.Items.Add(item);

            // Allied
            item = new ListViewItem
            {
                Text = (string) _controller.GetItemValue(ScenarioEditorItemId.AllianceName, data.Allies),
                Tag = data.Allies
            };
            item.SubItems.Add(data.Allies != null ? Countries.GetNameList(data.Allies.Participant) : "");
            allianceListView.Items.Add(item);

            // Communist country
            item = new ListViewItem
            {
                Text = (string) _controller.GetItemValue(ScenarioEditorItemId.AllianceName, data.Comintern),
                Tag = data.Comintern
            };
            item.SubItems.Add(data.Comintern != null ? Countries.GetNameList(data.Comintern.Participant) : "");
            allianceListView.Items.Add(item);

            // Other alliances
            foreach (Alliance alliance in data.Alliances)
            {
                item = new ListViewItem
                {
                    Text = Resources.Alliance,
                    Tag = alliance
                };
                item.SubItems.Add(Countries.GetNameList(alliance.Participant));
                allianceListView.Items.Add(item);
            }

            allianceListView.EndUpdate();

            // Disable the alliance operation button
            DisableAllianceItemButtons();

            // Disable alliance member country operation buttons
            DisableAllianceParticipantButtons();

            // Disable edit items
            DisableAllianceItems();

            // Clear edit items
            ClearAllianceItems();
        }

        /// <summary>
        ///     Activate the alliance list
        /// </summary>
        private void EnableAllianceList()
        {
            allianceGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Enable alliance operation buttons
        /// </summary>
        private void EnableAllianceItemButtons()
        {
            int count = allianceListView.Items.Count;
            int index = allianceListView.SelectedIndices[0];

            // Axis powers / / Allied / / Communist countries change order / / Cannot be deleted
            allianceUpButton.Enabled = index > 3;
            allianceDownButton.Enabled = (index < count - 1) && (index >= 3);
            allianceRemoveButton.Enabled = index >= 3;
        }

        /// <summary>
        ///     Disable the alliance operation button
        /// </summary>
        private void DisableAllianceItemButtons()
        {
            allianceUpButton.Enabled = false;
            allianceDownButton.Enabled = false;
            allianceRemoveButton.Enabled = false;
        }

        /// <summary>
        ///     Processing when changing the selection item in the alliance list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable alliance member country operation buttons
            DisableAllianceParticipantButtons();

            // Disable edit items if there are no selections
            if (allianceListView.SelectedItems.Count == 0)
            {
                // Disable the alliance operation button
                DisableAllianceItemButtons();

                // Disable edit items
                DisableAllianceItems();

                // Clear edit items
                ClearAllianceItems();
                return;
            }

            Alliance alliance = GetSelectedAlliance();

            // Update edit items
            UpdateAllianceItems(alliance);

            // Enable edit items
            EnableAllianceItems();

            // Enable alliance operation buttons
            EnableAllianceItemButtons();
        }

        /// <summary>
        ///     Processing when the button is pressed on the alliance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceUpButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Alliance> alliances = scenario.GlobalData.Alliances;

            // Move items in the Alliance List view
            int index = allianceListView.SelectedIndices[0];
            ListViewItem item = allianceListView.Items[index];
            allianceListView.Items.RemoveAt(index);
            allianceListView.Items.Insert(index - 1, item);
            allianceListView.Items[index - 1].Focused = true;
            allianceListView.Items[index - 1].Selected = true;
            allianceListView.EnsureVisible(index - 1);

            // Move items in the alliance list
            index -= 3; // -3 Is a pivotal country / / Allied / / Communist countries
            Alliance alliance = alliances[index];
            alliances.RemoveAt(index);
            alliances.Insert(index - 1, alliance);

            // Set the edited flag
            Scenarios.SetDirty();
        }

        /// <summary>
        ///     Processing when the button is pressed under the alliance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceDownButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Alliance> alliances = scenario.GlobalData.Alliances;

            // Move items in the Alliance List view
            int index = allianceListView.SelectedIndices[0];
            ListViewItem item = allianceListView.Items[index];
            allianceListView.Items.RemoveAt(index);
            allianceListView.Items.Insert(index + 1, item);
            allianceListView.Items[index + 1].Focused = true;
            allianceListView.Items[index + 1].Selected = true;
            allianceListView.EnsureVisible(index + 1);

            // Move items in the alliance list
            index -= 3; // -3 Is a pivotal country / / Allied / / Communist countries
            Alliance alliance = alliances[index];
            alliances.RemoveAt(index);
            alliances.Insert(index + 1, alliance);

            // Set the edited flag
            Scenarios.SetDirty();
        }

        /// <summary>
        ///     Processing when a new button of the alliance is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceNewButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Alliance> alliances = scenario.GlobalData.Alliances;

            // Add an item to the alliance list
            Alliance alliance = new Alliance { Id = Scenarios.GetNewTypeId(Scenarios.DefaultAllianceType, 1) };
            alliances.Add(alliance);

            // Add an item to the Alliance List view
            ListViewItem item = new ListViewItem { Text = Resources.Alliance, Tag = alliance };
            item.SubItems.Add("");
            allianceListView.Items.Add(item);

            Log.Info("[Scenario] alliance added ({0})", allianceListView.Items.Count - 4);

            // Set the edited flag
            alliance.SetDirty(Alliance.ItemId.Type);
            alliance.SetDirty(Alliance.ItemId.Id);
            Scenarios.SetDirty();

            // Select the added item
            if (allianceListView.SelectedIndices.Count > 0)
            {
                ListViewItem prev = allianceListView.SelectedItems[0];
                prev.Focused = false;
                prev.Selected = false;
            }
            item.Focused = true;
            item.Selected = true;
        }

        /// <summary>
        ///     Processing when the delete button of the alliance is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceRemoveButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Alliance> alliances = scenario.GlobalData.Alliances;

            // Axis country / / Allied / / Communist countries cannot be deleted
            int index = allianceListView.SelectedIndices[0] - 3;
            if (index < 0)
            {
                return;
            }

            Alliance alliance = alliances[index];

            // type When id idDelete the pair of
            Scenarios.RemoveTypeId(alliance.Id);

            // Remove an item from the alliance list
            alliances.RemoveAt(index);

            // Remove an item from the Alliance List view
            allianceListView.Items.RemoveAt(index + 3);

            Log.Info("[Scenario] alliance removed ({0})", index);

            // Set the edited flag
            Scenarios.SetDirty();

            // Select next to the deleted item
            index += index < alliances.Count ? 3 : 3 - 1;
            allianceListView.Items[index].Focused = true;
            allianceListView.Items[index].Selected = true;
        }


        /// <summary>
        ///     Set the item string in the alliance list view
        /// </summary>
        /// <param name="no">Item Number</param>
        /// <param name="s">Character string</param>
        public void SetAllianceListItemText(int no, string s)
        {
            allianceListView.SelectedItems[0].SubItems[no].Text = s;
        }

        /// <summary>
        ///     Get information about the selected alliance
        /// </summary>
        /// <returns>Selected alliance information</returns>
        private Alliance GetSelectedAlliance()
        {
            return allianceListView.SelectedItems.Count > 0 ? allianceListView.SelectedItems[0].Tag as Alliance : null;
        }

        #endregion

        #region Alliance tab ――――Alliance member countries

        /// <summary>
        ///     Update the list of alliance member countries
        /// </summary>
        /// <param name="alliance">alliance</param>
        private void UpdateAllianceParticipant(Alliance alliance)
        {
            // Alliance member countries
            allianceParticipantListBox.BeginUpdate();
            allianceParticipantListBox.Items.Clear();
            foreach (Country country in alliance.Participant)
            {
                allianceParticipantListBox.Items.Add(Countries.GetTagName(country));
            }
            allianceParticipantListBox.EndUpdate();

            // Non-participating countries
            _allianceFreeCountries = Countries.Tags.Where(country => !alliance.Participant.Contains(country)).ToList();
            allianceFreeCountryListBox.BeginUpdate();
            allianceFreeCountryListBox.Items.Clear();
            foreach (Country country in _allianceFreeCountries)
            {
                allianceFreeCountryListBox.Items.Add(Countries.GetTagName(country));
            }
            allianceFreeCountryListBox.EndUpdate();
        }

        /// <summary>
        ///     Clear the list of alliance member countries
        /// </summary>
        private void ClearAllianceParticipant()
        {
            allianceParticipantListBox.Items.Clear();
            allianceFreeCountryListBox.Items.Clear();
        }

        /// <summary>
        ///     Activate the list of alliance members
        /// </summary>
        private void EnableAllianceParticipant()
        {
            allianceParticipantLabel.Enabled = true;
            allianceParticipantListBox.Enabled = true;
            allianceFreeCountryListBox.Enabled = true;
        }

        /// <summary>
        ///     Disable the list of alliance members
        /// </summary>
        private void DisableAllianceParticipant()
        {
            allianceParticipantLabel.Enabled = false;
            allianceParticipantListBox.Enabled = false;
            allianceFreeCountryListBox.Enabled = false;
        }

        /// <summary>
        ///     Disable alliance member country operation buttons
        /// </summary>
        private void DisableAllianceParticipantButtons()
        {
            allianceParticipantAddButton.Enabled = false;
            allianceParticipantRemoveButton.Enabled = false;
            allianceLeaderButton.Enabled = false;
        }

        /// <summary>
        ///     Item drawing process of alliance member country list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceParticipantListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index < 0)
            {
                return;
            }

            ListBox control = sender as ListBox;
            if (control == null)
            {
                return;
            }

            // Do nothing if there is no selection
            Alliance alliance = GetSelectedAlliance();
            if (alliance == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw an item
            Brush brush;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // Change the text color for items that have changed
                bool dirty = alliance.IsDirtyCountry(alliance.Participant[e.Index]);
                brush = new SolidBrush(dirty ? Color.Red : control.ForeColor);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of list box of non-participating countries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceCountryListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index < 0)
            {
                return;
            }

            ListBox control = sender as ListBox;
            if (control == null)
            {
                return;
            }

            // Do nothing if there is no selection
            Alliance alliance = GetSelectedAlliance();
            if (alliance == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw an item
            Brush brush;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // Change the text color for items that have changed
                bool dirty = alliance.IsDirtyCountry(_allianceFreeCountries[e.Index]);
                brush = new SolidBrush(dirty ? Color.Red : control.ForeColor);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item in the alliance member country list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceParticipantListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            int count = allianceParticipantListBox.SelectedIndices.Count;
            int index = allianceParticipantListBox.SelectedIndex;

            allianceParticipantRemoveButton.Enabled = count > 0;
            allianceLeaderButton.Enabled = (count == 1) && (index > 0);
        }

        /// <summary>
        ///     Processing when changing the selection item in the list box of non-participating countries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            int count = allianceFreeCountryListBox.SelectedIndices.Count;

            allianceParticipantAddButton.Enabled = count > 0;
        }

        /// <summary>
        ///     Processing when the add button of the alliance member country is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceParticipantAddButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Alliance alliance = GetSelectedAlliance();
            if (alliance == null)
            {
                return;
            }

            List<Country> countries =
                (from int index in allianceFreeCountryListBox.SelectedIndices select _allianceFreeCountries[index])
                    .ToList();
            allianceParticipantListBox.BeginUpdate();
            allianceFreeCountryListBox.BeginUpdate();
            foreach (Country country in countries)
            {
                // Add to Alliance Participation List Box
                allianceParticipantListBox.Items.Add(Countries.GetTagName(country));

                // Add to Alliance Participation List
                alliance.Participant.Add(country);

                // Remove from alliance non-participating country list box
                int index = _allianceFreeCountries.IndexOf(country);
                allianceFreeCountryListBox.Items.RemoveAt(index);
                _allianceFreeCountries.RemoveAt(index);

                // Set the edited flag
                alliance.SetDirtyCountry(country);
                Scenarios.SetDirty();

                // Update items in Alliance List View
                allianceListView.SelectedItems[0].SubItems[1].Text = Countries.GetNameList(alliance.Participant);

                Log.Info("[Scenario] alliance participant: +{0} ({1})", Countries.Strings[(int) country],
                    allianceListView.SelectedIndices[0]);
            }
            allianceParticipantListBox.EndUpdate();
            allianceFreeCountryListBox.EndUpdate();
        }

        /// <summary>
        ///     Processing when the delete button of the alliance member country is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceParticipantRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Alliance alliance = GetSelectedAlliance();
            if (alliance == null)
            {
                return;
            }

            List<Country> countries =
                (from int index in allianceParticipantListBox.SelectedIndices select alliance.Participant[index])
                    .ToList();
            allianceParticipantListBox.BeginUpdate();
            allianceFreeCountryListBox.BeginUpdate();
            foreach (Country country in countries)
            {
                // Add to non-alliance country list box
                int index = _allianceFreeCountries.FindIndex(c => c > country);
                if (index < 0)
                {
                    index = _allianceFreeCountries.Count;
                }
                allianceFreeCountryListBox.Items.Insert(index, Countries.GetTagName(country));
                _allianceFreeCountries.Insert(index, country);

                // Remove from Alliance Member List Box
                index = alliance.Participant.IndexOf(country);
                allianceParticipantListBox.Items.RemoveAt(index);

                // Remove from alliance member list
                alliance.Participant.Remove(country);

                // Set the edited flag
                alliance.SetDirtyCountry(country);
                Scenarios.SetDirty();

                // Update items in Alliance List View
                allianceListView.SelectedItems[0].SubItems[1].Text = Countries.GetNameList(alliance.Participant);

                Log.Info("[Scenario] alliance participant: -{0} ({1})", Countries.Strings[(int) country],
                    allianceListView.SelectedIndices[0]);
            }
            allianceParticipantListBox.EndUpdate();
            allianceFreeCountryListBox.EndUpdate();
        }

        /// <summary>
        ///     Processing when the setting button is pressed for the leader of the alliance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceLeaderButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Alliance alliance = GetSelectedAlliance();
            if (alliance == null)
            {
                return;
            }

            int index = allianceParticipantListBox.SelectedIndex;
            Country country = alliance.Participant[index];

            // Move to the top of the Alliance Participation List Box
            allianceParticipantListBox.BeginUpdate();
            allianceParticipantListBox.Items.RemoveAt(index);
            allianceParticipantListBox.Items.Insert(0, Countries.GetTagName(country));
            allianceParticipantListBox.EndUpdate();

            // Move to the top of the list of alliance member countries
            alliance.Participant.RemoveAt(index);
            alliance.Participant.Insert(0, country);

            // Set the edited flag
            alliance.SetDirtyCountry(country);
            Scenarios.SetDirty();

            // Update items in Alliance List View
            allianceListView.SelectedItems[0].SubItems[1].Text = Countries.GetNameList(alliance.Participant);

            Log.Info("[Scenario] alliance leader: {0} ({1})", Countries.Strings[(int) country],
                allianceListView.SelectedIndices[0]);
        }

        #endregion

        #region Alliance tab ―――― war

        /// <summary>
        ///     Initialize the edit items of the war
        /// </summary>
        private void InitWarItems()
        {
            _itemControls.Add(ScenarioEditorItemId.WarStartYear, warStartYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.WarStartMonth, warStartMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.WarStartDay, warStartDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.WarEndYear, warEndYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.WarEndMonth, warEndMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.WarEndDay, warEndDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.WarType, warTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.WarId, warIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.WarAttackerType, warAttackerTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.WarAttackerId, warAttackerIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.WarDefenderType, warDefenderTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.WarDefenderId, warDefenderIdTextBox);

            warStartYearTextBox.Tag = ScenarioEditorItemId.WarStartYear;
            warStartMonthTextBox.Tag = ScenarioEditorItemId.WarStartMonth;
            warStartDayTextBox.Tag = ScenarioEditorItemId.WarStartDay;
            warEndYearTextBox.Tag = ScenarioEditorItemId.WarEndYear;
            warEndMonthTextBox.Tag = ScenarioEditorItemId.WarEndMonth;
            warEndDayTextBox.Tag = ScenarioEditorItemId.WarEndDay;
            warTypeTextBox.Tag = ScenarioEditorItemId.WarType;
            warIdTextBox.Tag = ScenarioEditorItemId.WarId;
            warAttackerTypeTextBox.Tag = ScenarioEditorItemId.WarAttackerType;
            warAttackerIdTextBox.Tag = ScenarioEditorItemId.WarAttackerId;
            warDefenderTypeTextBox.Tag = ScenarioEditorItemId.WarDefenderType;
            warDefenderIdTextBox.Tag = ScenarioEditorItemId.WarDefenderId;
        }

        /// <summary>
        ///     Update war edits
        /// </summary>
        /// <param name="war">war</param>
        private void UpdateWarItems(War war)
        {
            // Update the value of the edit item
            _controller.UpdateItemValue(warStartYearTextBox, war);
            _controller.UpdateItemValue(warStartMonthTextBox, war);
            _controller.UpdateItemValue(warStartDayTextBox, war);
            _controller.UpdateItemValue(warEndYearTextBox, war);
            _controller.UpdateItemValue(warEndMonthTextBox, war);
            _controller.UpdateItemValue(warEndDayTextBox, war);
            _controller.UpdateItemValue(warTypeTextBox, war);
            _controller.UpdateItemValue(warIdTextBox, war);
            _controller.UpdateItemValue(warAttackerTypeTextBox, war);
            _controller.UpdateItemValue(warAttackerIdTextBox, war);
            _controller.UpdateItemValue(warDefenderTypeTextBox, war);
            _controller.UpdateItemValue(warDefenderIdTextBox, war);

            // Update the color of the edit item
            _controller.UpdateItemColor(warStartYearTextBox, war);
            _controller.UpdateItemColor(warStartMonthTextBox, war);
            _controller.UpdateItemColor(warStartDayTextBox, war);
            _controller.UpdateItemColor(warEndYearTextBox, war);
            _controller.UpdateItemColor(warEndMonthTextBox, war);
            _controller.UpdateItemColor(warEndDayTextBox, war);
            _controller.UpdateItemColor(warTypeTextBox, war);
            _controller.UpdateItemColor(warIdTextBox, war);
            _controller.UpdateItemColor(warAttackerTypeTextBox, war);
            _controller.UpdateItemColor(warAttackerIdTextBox, war);
            _controller.UpdateItemColor(warDefenderTypeTextBox, war);
            _controller.UpdateItemColor(warDefenderIdTextBox, war);

            // Update the list of participating countries
            UpdateWarParticipant(war);
        }

        /// <summary>
        ///     Clear war edits
        /// </summary>
        private void ClearWarItems()
        {
            // Clear edit items
            warStartYearTextBox.Text = "";
            warStartMonthTextBox.Text = "";
            warStartDayTextBox.Text = "";
            warEndYearTextBox.Text = "";
            warEndMonthTextBox.Text = "";
            warEndDayTextBox.Text = "";
            warTypeTextBox.Text = "";
            warIdTextBox.Text = "";
            warAttackerTypeTextBox.Text = "";
            warAttackerIdTextBox.Text = "";
            warDefenderTypeTextBox.Text = "";
            warDefenderIdTextBox.Text = "";

            // Clear the list of countries participating in the war
            ClearWarParticipant();
        }

        /// <summary>
        ///     Enable war edits
        /// </summary>
        private void EnableWarItems()
        {
            warStartDateLabel.Enabled = true;
            warStartYearTextBox.Enabled = true;
            warStartMonthTextBox.Enabled = true;
            warStartDayTextBox.Enabled = true;
            warEndDateLabel.Enabled = true;
            warEndYearTextBox.Enabled = true;
            warEndMonthTextBox.Enabled = true;
            warEndDayTextBox.Enabled = true;
            warIdLabel.Enabled = true;
            warTypeTextBox.Enabled = true;
            warIdTextBox.Enabled = true;
            warAttackerIdLabel.Enabled = true;
            warAttackerTypeTextBox.Enabled = true;
            warAttackerIdTextBox.Enabled = true;
            warDefenderIdLabel.Enabled = true;
            warDefenderTypeTextBox.Enabled = true;
            warDefenderIdTextBox.Enabled = true;

            // Activate the list of participating countries
            EnableWarParticipant();
        }

        /// <summary>
        ///     Disable war edits
        /// </summary>
        private void DisableWarItems()
        {
            warStartDateLabel.Enabled = false;
            warStartYearTextBox.Enabled = false;
            warStartMonthTextBox.Enabled = false;
            warStartDayTextBox.Enabled = false;
            warEndDateLabel.Enabled = false;
            warEndYearTextBox.Enabled = false;
            warEndMonthTextBox.Enabled = false;
            warEndDayTextBox.Enabled = false;
            warIdLabel.Enabled = false;
            warTypeTextBox.Enabled = false;
            warIdTextBox.Enabled = false;
            warAttackerIdLabel.Enabled = false;
            warAttackerTypeTextBox.Enabled = false;
            warAttackerIdTextBox.Enabled = false;
            warDefenderIdLabel.Enabled = false;
            warDefenderTypeTextBox.Enabled = false;
            warDefenderIdTextBox.Enabled = false;

            // Disable the list of participating countries
            DisableWarParticipant();
        }

        #endregion

        #region Alliance tab ―――― War list

        /// <summary>
        ///     Update the war list view
        /// </summary>
        private void UpdateWarList()
        {
            ScenarioGlobalData data = Scenarios.Data.GlobalData;

            warListView.BeginUpdate();
            warListView.Items.Clear();
            foreach (War war in data.Wars)
            {
                ListViewItem item = new ListViewItem
                {
                    Text = Countries.GetNameList(war.Attackers.Participant),
                    Tag = war
                };
                item.SubItems.Add(Countries.GetNameList(war.Defenders.Participant));
                warListView.Items.Add(item);
            }
            warListView.EndUpdate();

            // Disable the war operation button
            DisableWarItemButtons();

            // Disable the operation buttons of the countries participating in the war
            DisableWarParticipantButtons();

            // Disable edit items
            DisableWarItems();

            // Clear edit items
            ClearWarItems();
        }

        /// <summary>
        ///     Activate the war list
        /// </summary>
        private void EnableWarList()
        {
            warGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Enable the war operation button
        /// </summary>
        private void EnableWarItemButtons()
        {
            int count = warListView.Items.Count;
            int index = warListView.SelectedIndices[0];

            warUpButton.Enabled = index > 0;
            warDownButton.Enabled = index < count - 1;
            warRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     Enable the war operation button
        /// </summary>
        private void DisableWarItemButtons()
        {
            warUpButton.Enabled = false;
            warDownButton.Enabled = false;
            warRemoveButton.Enabled = false;
        }

        /// <summary>
        ///     Processing when changing the selection item in the war list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable the operation buttons of the countries participating in the war
            DisableWarParticipantButtons();

            // Disable edit items if there are no selections
            if (warListView.SelectedItems.Count == 0)
            {
                // Disable the war operation button
                DisableWarItemButtons();

                // Disable edit items
                DisableWarItems();

                // Clear edit items
                ClearWarItems();
                return;
            }

            War war = GetSelectedWar();

            // Update edit items
            UpdateWarItems(war);

            // Enable edit items
            EnableWarItems();

            // Enable the war operation button
            EnableWarItemButtons();
        }

        /// <summary>
        ///     Processing when the button is pressed on the top of the war
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarUpButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<War> wars = scenario.GlobalData.Wars;

            // Move items in the war list view
            int index = warListView.SelectedIndices[0];
            ListViewItem item = warListView.Items[index];
            warListView.Items.RemoveAt(index);
            warListView.Items.Insert(index - 1, item);
            warListView.Items[index - 1].Focused = true;
            warListView.Items[index - 1].Selected = true;
            warListView.EnsureVisible(index - 1);

            // Move items in the war list
            War war = wars[index];
            wars.RemoveAt(index);
            wars.Insert(index - 1, war);

            // Set the edited flag
            Scenarios.SetDirty();
        }

        /// <summary>
        ///     Processing when the button is pressed under the war
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarDownButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<War> wars = scenario.GlobalData.Wars;

            // Move items in the war list view
            int index = warListView.SelectedIndices[0];
            ListViewItem item = warListView.Items[index];
            warListView.Items.RemoveAt(index);
            warListView.Items.Insert(index + 1, item);
            warListView.Items[index + 1].Focused = true;
            warListView.Items[index + 1].Selected = true;
            warListView.EnsureVisible(index + 1);

            // Move items in the war list
            War war = wars[index];
            wars.RemoveAt(index);
            wars.Insert(index + 1, war);

            // Set the edited flag
            Scenarios.SetDirty();
        }

        /// <summary>
        ///     Processing when a new button of war is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarNewButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<War> wars = scenario.GlobalData.Wars;

            // Add an item to the war list
            War war = new War
            {
                StartDate = new GameDate(),
                EndDate = new GameDate(),
                Id = Scenarios.GetNewTypeId(Scenarios.DefaultWarType, 1),
                Attackers = new Alliance { Id = Scenarios.GetNewTypeId(Scenarios.DefaultWarType, 1) },
                Defenders = new Alliance { Id = Scenarios.GetNewTypeId(Scenarios.DefaultWarType, 1) }
            };
            wars.Add(war);

            // Add an item to the war list view
            ListViewItem item = new ListViewItem { Tag = war };
            item.SubItems.Add("");
            warListView.Items.Add(item);

            Log.Info("[Scenario] war added ({0})", warListView.Items.Count - 1);

            // Set the edited flag
            war.SetDirty(War.ItemId.StartYear);
            war.SetDirty(War.ItemId.StartMonth);
            war.SetDirty(War.ItemId.StartDay);
            war.SetDirty(War.ItemId.EndYear);
            war.SetDirty(War.ItemId.EndMonth);
            war.SetDirty(War.ItemId.EndDay);
            war.SetDirty(War.ItemId.Type);
            war.SetDirty(War.ItemId.Id);
            war.SetDirty(War.ItemId.AttackerType);
            war.SetDirty(War.ItemId.AttackerId);
            war.SetDirty(War.ItemId.DefenderType);
            war.SetDirty(War.ItemId.DefenderId);
            Scenarios.SetDirty();

            // Select the added item
            if (warListView.SelectedIndices.Count > 0)
            {
                ListViewItem prev = warListView.SelectedItems[0];
                prev.Focused = false;
                prev.Selected = false;
            }
            item.Focused = true;
            item.Selected = true;
        }

        /// <summary>
        ///     Processing when the delete button of the war is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarRemoveButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<War> wars = scenario.GlobalData.Wars;

            // Axis country / / Allied / / Communist countries cannot be deleted
            int index = warListView.SelectedIndices[0];
            if (index < 0)
            {
                return;
            }

            War war = wars[index];

            Log.Info("[Scenario] war removed ({0})", index);

            // type When id idDelete the pair of
            Scenarios.RemoveTypeId(war.Id);

            // Remove an item from the war list
            wars.RemoveAt(index);

            // Remove an item from the war list view
            warListView.Items.RemoveAt(index);

            // Set the edited flag
            Scenarios.SetDirty();

            // Select next to the deleted item
            if (index >= wars.Count)
            {
                index --;
            }
            if (index >= 0)
            {
                warListView.Items[index].Focused = true;
                warListView.Items[index].Selected = true;
            }
        }

        /// <summary>
        ///     Get the selected war information
        /// </summary>
        /// <returns>Selected war information</returns>
        private War GetSelectedWar()
        {
            return warListView.SelectedItems.Count > 0 ? warListView.SelectedItems[0].Tag as War : null;
        }

        #endregion

        #region Alliance tab ―――― War participating countries

        /// <summary>
        ///     Update the list of participating countries
        /// </summary>
        /// <param name="war"></param>
        private void UpdateWarParticipant(War war)
        {
            IEnumerable<Country> countries = Countries.Tags;

            // Attacking Participating Countries
            warAttackerListBox.BeginUpdate();
            warAttackerListBox.Items.Clear();
            if (war.Attackers?.Participant != null)
            {
                foreach (Country country in war.Attackers.Participant)
                {
                    warAttackerListBox.Items.Add(Countries.GetTagName(country));
                }
                countries = countries.Where(country => !war.Attackers.Participant.Contains(country));
            }
            warAttackerListBox.EndUpdate();

            // Defender Participating Countries
            warDefenderListBox.BeginUpdate();
            warDefenderListBox.Items.Clear();
            if (war.Defenders?.Participant != null)
            {
                foreach (Country country in war.Defenders.Participant)
                {
                    warDefenderListBox.Items.Add(Countries.GetTagName(country));
                }
                countries = countries.Where(country => !war.Defenders.Participant.Contains(country));
            }
            warDefenderListBox.EndUpdate();

            // Non-participating countries
            _warFreeCountries = countries.ToList();
            warFreeCountryListBox.BeginUpdate();
            warFreeCountryListBox.Items.Clear();
            foreach (Country country in _warFreeCountries)
            {
                warFreeCountryListBox.Items.Add(Countries.GetTagName(country));
            }
            warFreeCountryListBox.EndUpdate();
        }

        /// <summary>
        ///     Clear the list of participating countries
        /// </summary>
        private void ClearWarParticipant()
        {
            warAttackerListBox.Items.Clear();
            warDefenderListBox.Items.Clear();
            warFreeCountryListBox.Items.Clear();
        }

        /// <summary>
        ///     Activate the list of participating countries
        /// </summary>
        private void EnableWarParticipant()
        {
            warAttackerLabel.Enabled = true;
            warAttackerListBox.Enabled = true;
            warDefenderLabel.Enabled = true;
            warDefenderListBox.Enabled = true;
            warFreeCountryListBox.Enabled = true;
        }

        /// <summary>
        ///     Disable the list of participating countries
        /// </summary>
        private void DisableWarParticipant()
        {
            warAttackerLabel.Enabled = false;
            warAttackerListBox.Enabled = false;
            warDefenderLabel.Enabled = false;
            warDefenderListBox.Enabled = false;
            warFreeCountryListBox.Enabled = false;
        }

        /// <summary>
        ///     Disable the operation buttons of the countries participating in the war
        /// </summary>
        private void DisableWarParticipantButtons()
        {
            warAttackerAddButton.Enabled = false;
            warAttackerRemoveButton.Enabled = false;
            warAttackerLeaderButton.Enabled = false;
            warDefenderAddButton.Enabled = false;
            warDefenderRemoveButton.Enabled = false;
            warDefenderLeaderButton.Enabled = false;
        }

        /// <summary>
        ///     Item drawing process of the list box of participating countries on the war attack side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarAttackerListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index < 0)
            {
                return;
            }

            ListBox control = sender as ListBox;
            if (control == null)
            {
                return;
            }

            // Do nothing if there is no selection
            War war = GetSelectedWar();
            if (war == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw an item
            Brush brush;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // Change the text color for items that have changed
                bool dirty = war.IsDirtyCountry(war.Attackers.Participant[e.Index]);
                brush = new SolidBrush(dirty ? Color.Red : control.ForeColor);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of the war defender participating country list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarDefenderListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index < 0)
            {
                return;
            }
            ListBox control = sender as ListBox;
            if (control == null)
            {
                return;
            }

            // Do nothing if there is no selection
            War war = GetSelectedWar();
            if (war == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw an item
            Brush brush;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // Change the text color for items that have changed
                bool dirty = war.IsDirtyCountry(war.Defenders.Participant[e.Index]);
                brush = new SolidBrush(dirty ? Color.Red : control.ForeColor);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of non-war country list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarCountryListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index < 0)
            {
                return;
            }

            ListBox control = sender as ListBox;
            if (control == null)
            {
                return;
            }

            // Do nothing if there is no selection
            War war = GetSelectedWar();
            if (war == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw an item
            Brush brush;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // Change the text color for items that have changed
                bool dirty = war.IsDirtyCountry(_warFreeCountries[e.Index]);
                brush = new SolidBrush(dirty ? Color.Red : control.ForeColor);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item in the list box of participating countries on the war attack side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarAttackerListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            int count = warAttackerListBox.SelectedIndices.Count;
            int index = warAttackerListBox.SelectedIndex;

            warAttackerRemoveButton.Enabled = count > 0;
            warAttackerLeaderButton.Enabled = (count == 1) && (index > 0);
        }

        /// <summary>
        ///     Processing when changing the selection item in the list box of participating countries on the war defender side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarDefenderListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            int count = warDefenderListBox.SelectedIndices.Count;
            int index = warDefenderListBox.SelectedIndex;

            warDefenderRemoveButton.Enabled = count > 0;
            warDefenderLeaderButton.Enabled = (count == 1) && (index > 0);
        }

        /// <summary>
        ///     Processing when changing the selection item in the list box of non-war countries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            int count = warFreeCountryListBox.SelectedIndices.Count;

            warAttackerAddButton.Enabled = count > 0;
            warDefenderAddButton.Enabled = count > 0;
        }

        /// <summary>
        ///     Processing when the add button of the participating countries on the war attack side is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarAttackerAddButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            War war = GetSelectedWar();
            if (war == null)
            {
                return;
            }

            List<Country> countries =
                (from int index in warFreeCountryListBox.SelectedIndices select _warFreeCountries[index]).ToList();
            warAttackerListBox.BeginUpdate();
            warFreeCountryListBox.BeginUpdate();
            foreach (Country country in countries)
            {
                // Add to the list box of participating countries on the war attack side
                warAttackerListBox.Items.Add(Countries.GetTagName(country));

                // Add to the list of participating countries on the war attack side
                war.Attackers.Participant.Add(country);

                // Remove from the list of non-war countries
                int index = _warFreeCountries.IndexOf(country);
                warFreeCountryListBox.Items.RemoveAt(index);
                _warFreeCountries.RemoveAt(index);

                // Set the edited flag
                war.SetDirtyCountry(country);
                Scenarios.SetDirty();

                // Update items in the war list view
                warListView.SelectedItems[0].SubItems[0].Text = Countries.GetNameList(war.Attackers.Participant);

                Log.Info("[Scenario] war attacker: +{0} ({1})", Countries.Strings[(int) country],
                    warListView.SelectedIndices[0]);
            }
            warAttackerListBox.EndUpdate();
            warFreeCountryListBox.EndUpdate();
        }

        /// <summary>
        ///     Processing when the delete button of the participating countries on the war attack side is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarAttackerRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            War war = GetSelectedWar();
            if (war == null)
            {
                return;
            }

            List<Country> countries =
                (from int index in warAttackerListBox.SelectedIndices select war.Attackers.Participant[index]).ToList();
            warAttackerListBox.BeginUpdate();
            warFreeCountryListBox.BeginUpdate();
            foreach (Country country in countries)
            {
                // Add to the list box of non-war countries
                int index = _warFreeCountries.FindIndex(c => c > country);
                if (index < 0)
                {
                    index = _warFreeCountries.Count;
                }
                warFreeCountryListBox.Items.Insert(index, Countries.GetTagName(country));
                _warFreeCountries.Insert(index, country);

                // Remove from the list box of participating countries on the war attack side
                index = war.Attackers.Participant.IndexOf(country);
                warAttackerListBox.Items.RemoveAt(index);

                // Remove from the list of participating countries on the war attack side
                war.Attackers.Participant.Remove(country);

                // Set the edited flag
                war.SetDirtyCountry(country);
                Scenarios.SetDirty();

                // Update items in the war list view
                warListView.SelectedItems[0].SubItems[0].Text = Countries.GetNameList(war.Attackers.Participant);

                Log.Info("[Scenario] war attacker: -{0} ({1})", Countries.Strings[(int) country],
                    warListView.SelectedIndices[0]);
            }
            warAttackerListBox.EndUpdate();
            warFreeCountryListBox.EndUpdate();
        }

        /// <summary>
        ///     Processing when the add button of the war defender participating country is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarDefenderAddButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            War war = GetSelectedWar();
            if (war == null)
            {
                return;
            }

            List<Country> countries =
                (from int index in warFreeCountryListBox.SelectedIndices select _warFreeCountries[index]).ToList();
            warDefenderListBox.BeginUpdate();
            warFreeCountryListBox.BeginUpdate();
            foreach (Country country in countries)
            {
                // Add to War Defender Participating Country List Box
                warDefenderListBox.Items.Add(Countries.GetTagName(country));

                // Add to the list of participating countries of the war defender
                war.Defenders.Participant.Add(country);

                // Remove from the list of non-war countries
                int index = _warFreeCountries.IndexOf(country);
                warFreeCountryListBox.Items.RemoveAt(index);
                _warFreeCountries.RemoveAt(index);

                // Set the edited flag
                war.SetDirtyCountry(country);
                Scenarios.SetDirty();

                // Update items in the war list view
                warListView.SelectedItems[0].SubItems[1].Text = Countries.GetNameList(war.Defenders.Participant);

                Log.Info("[Scenario] war defender: +{0} ({1})", Countries.Strings[(int) country],
                    warListView.SelectedIndices[0]);
            }
            warDefenderListBox.EndUpdate();
            warFreeCountryListBox.EndUpdate();
        }

        /// <summary>
        ///     Processing when the delete button of the participating country on the war defender is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarDefenderRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            War war = GetSelectedWar();
            if (war == null)
            {
                return;
            }

            List<Country> countries =
                (from int index in warDefenderListBox.SelectedIndices select war.Defenders.Participant[index]).ToList();
            warDefenderListBox.BeginUpdate();
            warFreeCountryListBox.BeginUpdate();
            foreach (Country country in countries)
            {
                // Add to the list box of non-war countries
                int index = _warFreeCountries.FindIndex(c => c > country);
                if (index < 0)
                {
                    index = _warFreeCountries.Count;
                }
                warFreeCountryListBox.Items.Insert(index, Countries.GetTagName(country));
                _warFreeCountries.Insert(index, country);

                // Remove from War Defender Participating Country List Box
                index = war.Defenders.Participant.IndexOf(country);
                warDefenderListBox.Items.RemoveAt(index);

                // Remove from the list of participating countries on the war defender
                war.Defenders.Participant.Remove(country);

                // Set the edited flag
                war.SetDirtyCountry(country);
                Scenarios.SetDirty();

                // Update items in the war list view
                warListView.SelectedItems[0].SubItems[1].Text = Countries.GetNameList(war.Defenders.Participant);

                Log.Info("[Scenario] war defender: -{0} ({1})", Countries.Strings[(int) country],
                    warListView.SelectedIndices[0]);
            }
            warDefenderListBox.EndUpdate();
            warFreeCountryListBox.EndUpdate();
        }

        /// <summary>
        ///     Processing when the setting button is pressed for the leader on the war attack side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarAttackerLeaderButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            War war = GetSelectedWar();
            if (war == null)
            {
                return;
            }

            int index = warAttackerListBox.SelectedIndex;
            Country country = war.Attackers.Participant[index];

            // Move to the top of the list box of participating countries on the war attack side
            warAttackerListBox.BeginUpdate();
            warAttackerListBox.Items.RemoveAt(index);
            warAttackerListBox.Items.Insert(0, Countries.GetTagName(country));
            warAttackerListBox.EndUpdate();

            // Move to the top of the list of participating countries on the war attack side
            war.Attackers.Participant.RemoveAt(index);
            war.Attackers.Participant.Insert(0, country);

            // Set the edited flag
            war.SetDirtyCountry(country);
            Scenarios.SetDirty();

            // Update items in the war list view
            warListView.SelectedItems[0].SubItems[0].Text = Countries.GetNameList(war.Attackers.Participant);

            Log.Info("[Scenario] war attacker leader: {0} ({1})", Countries.Strings[(int) country],
                warListView.SelectedIndices[0]);
        }

        /// <summary>
        ///     Processing when the setting button is pressed for the leader on the war defense side
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarDefenderLeaderButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            War war = GetSelectedWar();
            if (war == null)
            {
                return;
            }

            int index = warDefenderListBox.SelectedIndex;
            Country country = war.Defenders.Participant[index];

            // Move to the top of the War Defender Participating Country List Box
            warDefenderListBox.BeginUpdate();
            warDefenderListBox.Items.RemoveAt(index);
            warDefenderListBox.Items.Insert(0, Countries.GetTagName(country));
            warDefenderListBox.EndUpdate();

            // Move to the top of the list of participating countries on the war defender side
            war.Defenders.Participant.RemoveAt(index);
            war.Defenders.Participant.Insert(0, country);

            // Set the edited flag
            war.SetDirtyCountry(country);
            Scenarios.SetDirty();

            // Update items in the war list view
            warListView.SelectedItems[0].SubItems[1].Text = Countries.GetNameList(war.Defenders.Participant);

            Log.Info("[Scenario] war defender leader: {0} ({1})", Countries.Strings[(int) country],
                warListView.SelectedIndices[0]);
        }

        #endregion

        #region Alliance tab ―――― Edit items

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Alliance alliance = GetSelectedAlliance();
            if (alliance == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, alliance);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, alliance);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, alliance);
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, alliance, allianceListView.SelectedIndices[0]);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, alliance);

            // Update value
            _controller.SetItemValue(itemId, val, alliance);

            // Set the edited flag
            _controller.SetItemDirty(itemId, alliance);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, alliance);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWarIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            War war = GetSelectedWar();
            if (war == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, war);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, war);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, war);
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, war, warListView.SelectedIndices[0]);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, war);

            // Update value
            _controller.SetItemValue(itemId, val, war);

            // Set the edited flag
            _controller.SetItemDirty(itemId, war);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, war);
        }

        /// <summary>
        ///     Processing when changing the value of a text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllianceStringItemTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Alliance alliance = GetSelectedAlliance();
            if (alliance == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Do nothing if the value does not change
            string val = control.Text;
            if (val.Equals((string) _controller.GetItemValue(itemId, alliance)))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, alliance, allianceListView.SelectedIndices[0]);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, alliance);

            // Update value
            _controller.SetItemValue(itemId, val, alliance);

            // Set the edited flag
            _controller.SetItemDirty(itemId, alliance);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, alliance);
        }

        #endregion

        #endregion

        #region Relationship tab

        #region Relationship tab ―――― common

        /// <summary>
        ///     Initialize the relationship tab
        /// </summary>
        private void InitRelationTab()
        {
            InitRelationItems();
            InitGuaranteedItems();
            InitNonAggressionItems();
            InitPeaceItems();
            InitIntelligenceItems();
        }

        /// <summary>
        ///     Update the relationship tab
        /// </summary>
        private void UpdateRelationTab()
        {
            // Do nothing if initialized
            if (_tabPageInitialized[(int) TabPageNo.Relation])
            {
                return;
            }

            // Update the list of selected countries
            UpdateCountryListBox(relationCountryListBox);

            // Activate the list of selected countries
            EnableRelationCountryList();

            // Clear the national relations list
            ClearRelationList();

            // Disable edit items
            DisableRelationItems();
            DisableGuaranteedItems();
            DisableNonAggressionItems();
            DisablePeaceItems();
            DisableIntelligenceItems();

            // Clear edit items
            ClearRelationItems();
            ClearGuaranteedItems();
            ClearNonAggressionItems();
            ClearPeaceItems();
            ClearIntelligenceItems();

            // Activate the national relations list
            EnableRelationList();

            // Set the initialized flag
            _tabPageInitialized[(int) TabPageNo.Relation] = true;
        }

        /// <summary>
        ///     Processing when loading the form of the relation tab
        /// </summary>
        private void OnRelationTabPageFormLoad()
        {
            // Initialize the relationship tab
            InitRelationTab();
        }

        /// <summary>
        ///     Processing when reading a file on the relation tab
        /// </summary>
        private void OnRelationTabPageFileLoad()
        {
            // Do nothing unless the relationship tab is selected
            if (_tabPageNo != TabPageNo.Relation)
            {
                return;
            }

            // Update the display at the first transition
            UpdateRelationTab();
        }

        /// <summary>
        ///     Processing when the relationship tab is selected
        /// </summary>
        private void OnRelationTabPageSelected()
        {
            // Do nothing if scenario not loaded
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // Update the display at the first transition
            UpdateRelationTab();
        }

        #endregion

        #region National list

        /// <summary>
        ///     Activate the national list box
        /// </summary>
        private void EnableRelationCountryList()
        {
            relationCountryListBox.Enabled = true;
        }

        /// <summary>
        ///     Processing when changing the selection item of the national list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear the national relations list if there are no choices
            if (relationCountryListBox.SelectedIndex < 0)
            {
                // Clear the national relations list
                ClearRelationList();
                return;
            }

            // Update national relations list
            UpdateRelationList();
        }

        /// <summary>
        ///     Acquire a country of choice for national relations
        /// </summary>
        /// <returns>Country of choice for national relations</returns>
        private Country GetSelectedRelationCountry()
        {
            return relationCountryListBox.SelectedIndex >= 0
                ? Countries.Tags[relationCountryListBox.SelectedIndex]
                : Country.None;
        }

        #endregion

        #region Relationship tab ―――― National relations list

        /// <summary>
        ///     Update national relations list
        /// </summary>
        private void UpdateRelationList()
        {
            Country selected = GetSelectedRelationCountry();
            CountrySettings settings = Scenarios.GetCountrySettings(selected);

            relationListView.BeginUpdate();
            relationListView.Items.Clear();
            foreach (Country target in Countries.Tags)
            {
                relationListView.Items.Add(CreateRelationListItem(selected, target, settings));
            }
            relationListView.EndUpdate();
        }

        /// <summary>
        ///     Clear the national relations list
        /// </summary>
        private void ClearRelationList()
        {
            relationListView.BeginUpdate();
            relationListView.Items.Clear();
            relationListView.EndUpdate();
        }

        /// <summary>
        ///     Activate the national relations list
        /// </summary>
        private void EnableRelationList()
        {
            relationListView.Enabled = true;
        }

        /// <summary>
        ///     Processing when changing the selection item in the national relations list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable edit items if there are no selections
            if (relationListView.SelectedIndices.Count == 0)
            {
                // Disable edit items
                DisableRelationItems();
                DisableGuaranteedItems();
                DisableNonAggressionItems();
                DisablePeaceItems();
                DisableIntelligenceItems();

                // Clear edit items
                ClearRelationItems();
                ClearGuaranteedItems();
                ClearNonAggressionItems();
                ClearPeaceItems();
                ClearIntelligenceItems();
                return;
            }

            Country selected = GetSelectedRelationCountry();
            Country target = GetTargetRelationCountry();
            CountrySettings settings = Scenarios.GetCountrySettings(selected);
            Relation relation = Scenarios.GetCountryRelation(selected, target);
            Treaty nonAggression = Scenarios.GetNonAggression(selected, target);
            Treaty peace = Scenarios.GetPeace(selected, target);
            SpySettings spy = Scenarios.GetCountryIntelligence(selected, target);

            // Update edit items
            UpdateRelationItems(relation, target, settings);
            UpdateGuaranteedItems(relation);
            UpdateNonAggressionItems(nonAggression);
            UpdatePeaceItems(peace);
            UpdateIntelligenceItems(spy);

            // Enable edit items
            EnableRelationItems();
            EnableGuaranteedItems();
            EnableNonAggressionItems();
            EnablePeaceItems();
            EnableIntelligenceItems();
        }

        /// <summary>
        ///     Set the item string in the trade list view
        /// </summary>
        /// <param name="index">Item index</param>
        /// <param name="no">Item Number</param>
        /// <param name="s">Character string</param>
        public void SetRelationListItemText(int index, int no, string s)
        {
            relationListView.Items[index].SubItems[no].Text = s;
        }

        /// <summary>
        ///     Set the character string of the selection item in the trade list view
        /// </summary>
        /// <param name="no">Item Number</param>
        /// <param name="s">Character string</param>
        public void SetRelationListItemText(int no, string s)
        {
            relationListView.SelectedItems[0].SubItems[no].Text = s;
        }

        /// <summary>
        ///     Create an item in the national relations list view
        /// </summary>
        /// <param name="selected">Selected country</param>
        /// <param name="target">Target country</param>
        /// <param name="settings">National setting</param>
        /// <returns>Items in the national relations list view</returns>
        private ListViewItem CreateRelationListItem(Country selected, Country target, CountrySettings settings)
        {
            ListViewItem item = new ListViewItem(Countries.GetTagName(target));
            Relation relation = Scenarios.GetCountryRelation(selected, target);
            Treaty nonAggression = Scenarios.GetNonAggression(selected, target);
            Treaty peace = Scenarios.GetPeace(selected, target);
            SpySettings spy = Scenarios.GetCountryIntelligence(selected, target);

            item.SubItems.Add(
                ObjectHelper.ToString(_controller.GetItemValue(ScenarioEditorItemId.DiplomacyRelationValue, relation)));
            item.SubItems.Add(
                (Country) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyMaster, settings) == target
                    ? Resources.Yes
                    : "");
            item.SubItems.Add(
                (Country) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyMilitaryControl, settings) == target
                    ? Resources.Yes
                    : "");
            item.SubItems.Add(
                (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyMilitaryAccess, relation)
                    ? Resources.Yes
                    : "");
            item.SubItems.Add(
                (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyGuaranteed, relation) ? Resources.Yes : "");
            item.SubItems.Add(
                (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyNonAggression, nonAggression)
                    ? Resources.Yes
                    : "");
            item.SubItems.Add(
                (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyPeace, peace) ? Resources.Yes : "");
            item.SubItems.Add(
                ObjectHelper.ToString(_controller.GetItemValue(ScenarioEditorItemId.IntelligenceSpies, spy)));

            return item;
        }

        /// <summary>
        ///     Acquire the target country of national relations
        /// </summary>
        /// <returns>Target country of national relations</returns>
        private Country GetTargetRelationCountry()
        {
            return relationListView.SelectedItems.Count > 0
                ? Countries.Tags[relationListView.SelectedIndices[0]]
                : Country.None;
        }

        #endregion

        #region Relationship tab ―――― National relations

        /// <summary>
        ///     Initialize national relations edit items
        /// </summary>
        private void InitRelationItems()
        {
            _itemControls.Add(ScenarioEditorItemId.DiplomacyRelationValue, relationValueTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyMaster, masterCheckBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyMilitaryControl, controlCheckBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyMilitaryAccess, accessCheckBox);

            relationValueTextBox.Tag = ScenarioEditorItemId.DiplomacyRelationValue;
            masterCheckBox.Tag = ScenarioEditorItemId.DiplomacyMaster;
            controlCheckBox.Tag = ScenarioEditorItemId.DiplomacyMilitaryControl;
            accessCheckBox.Tag = ScenarioEditorItemId.DiplomacyMilitaryAccess;
        }

        /// <summary>
        ///     Update national relations edits
        /// </summary>
        /// <param name="relation">National relations</param>
        /// <param name="target">Partner country</param>
        /// <param name="settings">National setting</param>
        private void UpdateRelationItems(Relation relation, Country target, CountrySettings settings)
        {
            _controller.UpdateItemValue(relationValueTextBox, relation);
            _controller.UpdateItemValue(masterCheckBox, target, settings);
            _controller.UpdateItemValue(controlCheckBox, target, settings);
            _controller.UpdateItemValue(accessCheckBox, relation);

            _controller.UpdateItemColor(relationValueTextBox, relation);
            _controller.UpdateItemColor(masterCheckBox, settings);
            _controller.UpdateItemColor(controlCheckBox, settings);
            _controller.UpdateItemColor(accessCheckBox, relation);
        }

        /// <summary>
        ///     Clear national relations edit items
        /// </summary>
        private void ClearRelationItems()
        {
            relationValueTextBox.Text = "";
            masterCheckBox.Checked = false;
            controlCheckBox.Checked = false;
            accessCheckBox.Checked = false;
        }

        /// <summary>
        ///     Enable national relations edits
        /// </summary>
        private void EnableRelationItems()
        {
            relationGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable national relations edits
        /// </summary>
        private void DisableRelationItems()
        {
            relationGroupBox.Enabled = false;
        }

        #endregion

        #region Relationship tab ―――― Independence guarantee

        /// <summary>
        ///     Initialize the edit items of the independence guarantee
        /// </summary>
        private void InitGuaranteedItems()
        {
            _itemControls.Add(ScenarioEditorItemId.DiplomacyGuaranteed, guaranteedCheckBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyGuaranteedEndYear, guaranteedYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyGuaranteedEndMonth, guaranteedMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyGuaranteedEndDay, guaranteedDayTextBox);

            guaranteedCheckBox.Tag = ScenarioEditorItemId.DiplomacyGuaranteed;
            guaranteedYearTextBox.Tag = ScenarioEditorItemId.DiplomacyGuaranteedEndYear;
            guaranteedMonthTextBox.Tag = ScenarioEditorItemId.DiplomacyGuaranteedEndMonth;
            guaranteedDayTextBox.Tag = ScenarioEditorItemId.DiplomacyGuaranteedEndDay;
        }

        /// <summary>
        ///     Update independence guarantee edit items
        /// </summary>
        /// <param name="relation">National relations</param>
        private void UpdateGuaranteedItems(Relation relation)
        {
            _controller.UpdateItemValue(guaranteedCheckBox, relation);
            _controller.UpdateItemValue(guaranteedYearTextBox, relation);
            _controller.UpdateItemValue(guaranteedMonthTextBox, relation);
            _controller.UpdateItemValue(guaranteedDayTextBox, relation);

            _controller.UpdateItemColor(guaranteedCheckBox, relation);
            _controller.UpdateItemColor(guaranteedYearTextBox, relation);
            _controller.UpdateItemColor(guaranteedMonthTextBox, relation);
            _controller.UpdateItemColor(guaranteedDayTextBox, relation);

            bool flag = (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyGuaranteed, relation);
            guaranteedYearTextBox.Enabled = flag;
            guaranteedMonthTextBox.Enabled = flag;
            guaranteedDayTextBox.Enabled = flag;
        }

        /// <summary>
        ///     Clear the edit items of the independence guarantee
        /// </summary>
        private void ClearGuaranteedItems()
        {
            guaranteedCheckBox.Checked = false;
            guaranteedYearTextBox.Text = "";
            guaranteedMonthTextBox.Text = "";
            guaranteedDayTextBox.Text = "";

            guaranteedYearTextBox.Enabled = false;
            guaranteedMonthTextBox.Enabled = false;
            guaranteedDayTextBox.Enabled = false;
        }

        /// <summary>
        ///     Enable independence guarantee edit items
        /// </summary>
        private void EnableGuaranteedItems()
        {
            guaranteedGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for independence guarantee
        /// </summary>
        private void DisableGuaranteedItems()
        {
            guaranteedGroupBox.Enabled = false;
        }

        #endregion

        #region Relationship tab ―――― Non-aggression treaty

        /// <summary>
        ///     Initialize the edit items of the non-invasion treaty
        /// </summary>
        private void InitNonAggressionItems()
        {
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggression, nonAggressionCheckBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionStartYear, nonAggressionStartYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionStartMonth, nonAggressionStartMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionStartDay, nonAggressionStartDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionEndYear, nonAggressionEndYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionEndMonth, nonAggressionEndMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionEndDay, nonAggressionEndDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionType, nonAggressionTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyNonAggressionId, nonAggressionIdTextBox);

            nonAggressionCheckBox.Tag = ScenarioEditorItemId.DiplomacyNonAggression;
            nonAggressionStartYearTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionStartYear;
            nonAggressionStartMonthTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionStartMonth;
            nonAggressionStartDayTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionStartDay;
            nonAggressionEndYearTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionEndYear;
            nonAggressionEndMonthTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionEndMonth;
            nonAggressionEndDayTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionEndDay;
            nonAggressionTypeTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionType;
            nonAggressionIdTextBox.Tag = ScenarioEditorItemId.DiplomacyNonAggressionId;
        }

        /// <summary>
        ///     Update the edit items of the non-invasion treaty
        /// </summary>
        /// <param name="treaty">Agreement</param>
        private void UpdateNonAggressionItems(Treaty treaty)
        {
            _controller.UpdateItemValue(nonAggressionCheckBox, treaty);
            _controller.UpdateItemValue(nonAggressionStartYearTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionStartMonthTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionStartDayTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionEndYearTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionEndMonthTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionEndDayTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionTypeTextBox, treaty);
            _controller.UpdateItemValue(nonAggressionIdTextBox, treaty);

            _controller.UpdateItemColor(nonAggressionCheckBox, treaty);
            _controller.UpdateItemColor(nonAggressionStartYearTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionStartMonthTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionStartDayTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionEndYearTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionEndMonthTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionEndDayTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionTypeTextBox, treaty);
            _controller.UpdateItemColor(nonAggressionIdTextBox, treaty);

            bool flag = (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyNonAggression, treaty);
            nonAggressionStartLabel.Enabled = flag;
            nonAggressionStartYearTextBox.Enabled = flag;
            nonAggressionStartMonthTextBox.Enabled = flag;
            nonAggressionStartDayTextBox.Enabled = flag;
            nonAggressionEndLabel.Enabled = flag;
            nonAggressionEndYearTextBox.Enabled = flag;
            nonAggressionEndMonthTextBox.Enabled = flag;
            nonAggressionEndDayTextBox.Enabled = flag;
            nonAggressionIdLabel.Enabled = flag;
            nonAggressionTypeTextBox.Enabled = flag;
            nonAggressionIdTextBox.Enabled = flag;
        }

        /// <summary>
        ///     Clear the edit items of the non-invasion treaty
        /// </summary>
        private void ClearNonAggressionItems()
        {
            nonAggressionCheckBox.Checked = false;
            nonAggressionStartYearTextBox.Text = "";
            nonAggressionStartMonthTextBox.Text = "";
            nonAggressionStartDayTextBox.Text = "";
            nonAggressionEndYearTextBox.Text = "";
            nonAggressionEndMonthTextBox.Text = "";
            nonAggressionEndDayTextBox.Text = "";
            nonAggressionTypeTextBox.Text = "";
            nonAggressionIdTextBox.Text = "";

            nonAggressionStartLabel.Enabled = false;
            nonAggressionStartYearTextBox.Enabled = false;
            nonAggressionStartMonthTextBox.Enabled = false;
            nonAggressionStartDayTextBox.Enabled = false;
            nonAggressionEndLabel.Enabled = false;
            nonAggressionEndYearTextBox.Enabled = false;
            nonAggressionEndMonthTextBox.Enabled = false;
            nonAggressionEndDayTextBox.Enabled = false;
            nonAggressionIdLabel.Enabled = false;
            nonAggressionTypeTextBox.Enabled = false;
            nonAggressionIdTextBox.Enabled = false;
        }

        /// <summary>
        ///     Activate the edit items of the non-invasion treaty
        /// </summary>
        private void EnableNonAggressionItems()
        {
            nonAggressionGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Invalidate the edit items of the non-aggression treaty
        /// </summary>
        private void DisableNonAggressionItems()
        {
            nonAggressionGroupBox.Enabled = false;
        }

        #endregion

        #region Relationship tab ―――― Peace treaty

        /// <summary>
        ///     Initialize the edit items of the peace treaty
        /// </summary>
        private void InitPeaceItems()
        {
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeace, peaceCheckBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceStartYear, peaceStartYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceStartMonth, peaceStartMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceStartDay, peaceStartDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceEndYear, peaceEndYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceEndMonth, peaceEndMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceEndDay, peaceEndDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceType, peaceTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.DiplomacyPeaceId, peaceIdTextBox);

            peaceCheckBox.Tag = ScenarioEditorItemId.DiplomacyPeace;
            peaceStartYearTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceStartYear;
            peaceStartMonthTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceStartMonth;
            peaceStartDayTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceStartDay;
            peaceEndYearTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceEndYear;
            peaceEndMonthTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceEndMonth;
            peaceEndDayTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceEndDay;
            peaceTypeTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceType;
            peaceIdTextBox.Tag = ScenarioEditorItemId.DiplomacyPeaceId;
        }

        /// <summary>
        ///     Update peace treaty edits
        /// </summary>
        /// <param name="treaty">Agreement</param>
        private void UpdatePeaceItems(Treaty treaty)
        {
            _controller.UpdateItemValue(peaceCheckBox, treaty);
            _controller.UpdateItemValue(peaceStartYearTextBox, treaty);
            _controller.UpdateItemValue(peaceStartMonthTextBox, treaty);
            _controller.UpdateItemValue(peaceStartDayTextBox, treaty);
            _controller.UpdateItemValue(peaceEndYearTextBox, treaty);
            _controller.UpdateItemValue(peaceEndMonthTextBox, treaty);
            _controller.UpdateItemValue(peaceEndDayTextBox, treaty);
            _controller.UpdateItemValue(peaceTypeTextBox, treaty);
            _controller.UpdateItemValue(peaceIdTextBox, treaty);

            _controller.UpdateItemColor(peaceCheckBox, treaty);
            _controller.UpdateItemColor(peaceStartYearTextBox, treaty);
            _controller.UpdateItemColor(peaceStartMonthTextBox, treaty);
            _controller.UpdateItemColor(peaceStartDayTextBox, treaty);
            _controller.UpdateItemColor(peaceEndYearTextBox, treaty);
            _controller.UpdateItemColor(peaceEndMonthTextBox, treaty);
            _controller.UpdateItemColor(peaceEndDayTextBox, treaty);
            _controller.UpdateItemColor(peaceTypeTextBox, treaty);
            _controller.UpdateItemColor(peaceIdTextBox, treaty);

            bool flag = (bool) _controller.GetItemValue(ScenarioEditorItemId.DiplomacyPeace, treaty);
            peaceStartLabel.Enabled = flag;
            peaceStartYearTextBox.Enabled = flag;
            peaceStartMonthTextBox.Enabled = flag;
            peaceStartDayTextBox.Enabled = flag;
            peaceEndLabel.Enabled = flag;
            peaceEndYearTextBox.Enabled = flag;
            peaceEndMonthTextBox.Enabled = flag;
            peaceEndDayTextBox.Enabled = flag;
            peaceIdLabel.Enabled = flag;
            peaceTypeTextBox.Enabled = flag;
            peaceIdTextBox.Enabled = flag;
        }

        /// <summary>
        ///     Clear the editing items of the Peace Treaty
        /// </summary>
        private void ClearPeaceItems()
        {
            peaceCheckBox.Checked = false;
            peaceStartYearTextBox.Text = "";
            peaceStartMonthTextBox.Text = "";
            peaceStartDayTextBox.Text = "";
            peaceEndYearTextBox.Text = "";
            peaceEndMonthTextBox.Text = "";
            peaceEndDayTextBox.Text = "";
            peaceTypeTextBox.Text = "";
            peaceIdTextBox.Text = "";

            peaceStartLabel.Enabled = false;
            peaceStartYearTextBox.Enabled = false;
            peaceStartMonthTextBox.Enabled = false;
            peaceStartDayTextBox.Enabled = false;
            peaceEndLabel.Enabled = false;
            peaceEndYearTextBox.Enabled = false;
            peaceEndMonthTextBox.Enabled = false;
            peaceEndDayTextBox.Enabled = false;
            peaceIdLabel.Enabled = false;
            peaceTypeTextBox.Enabled = false;
            peaceIdTextBox.Enabled = false;
        }

        /// <summary>
        ///     Activate the editing items of the Peace Treaty
        /// </summary>
        private void EnablePeaceItems()
        {
            peaceGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Invalidate the edit items of the peace treaty
        /// </summary>
        private void DisablePeaceItems()
        {
            peaceGroupBox.Enabled = false;
        }

        #endregion

        #region Relationship tab ―――― Intelligence

        /// <summary>
        ///     Update intelligence edits
        /// </summary>
        private void InitIntelligenceItems()
        {
            _itemControls.Add(ScenarioEditorItemId.IntelligenceSpies, spyNumNumericUpDown);

            spyNumNumericUpDown.Tag = ScenarioEditorItemId.IntelligenceSpies;
        }

        /// <summary>
        ///     Update intelligence edits
        /// </summary>
        /// <param name="spy">Intelligence settings</param>
        private void UpdateIntelligenceItems(SpySettings spy)
        {
            _controller.UpdateItemValue(spyNumNumericUpDown, spy);

            _controller.UpdateItemColor(spyNumNumericUpDown, spy);
        }

        /// <summary>
        ///     Clear intelligence edits
        /// </summary>
        private void ClearIntelligenceItems()
        {
            spyNumNumericUpDown.Value = 0;
        }

        /// <summary>
        ///     Enable intelligence editing items
        /// </summary>
        private void EnableIntelligenceItems()
        {
            intelligenceGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable intelligence edit items
        /// </summary>
        private void DisableIntelligenceItems()
        {
            intelligenceGroupBox.Enabled = false;
        }

        #endregion

        #region Relationship tab ―――― Edit items

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(selected);
            Relation relation = Scenarios.GetCountryRelation(selected, target);

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, relation);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, relation);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, relation);
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(selected);
            }

            if (relation == null)
            {
                relation = new Relation { Country = target };
                settings.Relations.Add(relation);
                Scenarios.SetCountryRelation(selected, relation);
            }

            _controller.OutputItemValueChangedLog(itemId, val, selected, relation);

            // Update value
            _controller.SetItemValue(itemId, val, relation);

            // Set the edited flag
            _controller.SetItemDirty(itemId, relation, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, relation);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationDoubleItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(selected);
            Relation relation = Scenarios.GetCountryRelation(selected, target);

            // Returns the value if the string cannot be converted to a number
            double val;
            if (!DoubleHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, relation);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, relation);
            if ((prev == null) && DoubleHelper.IsZero(val))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && DoubleHelper.IsEqual(val, (double) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, relation);
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(selected);
            }

            if (relation == null)
            {
                relation = new Relation { Country = target };
                settings.Relations.Add(relation);
                Scenarios.SetCountryRelation(selected, relation);
            }

            _controller.OutputItemValueChangedLog(itemId, val, selected, relation);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, relation);

            // Update value
            _controller.SetItemValue(itemId, val, relation);

            // Set the edited flag
            _controller.SetItemDirty(itemId, relation, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, relation);
        }

        /// <summary>
        ///     Processing when changing the check status of a check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationItemCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(selected);
            Relation relation = Scenarios.GetCountryRelation(selected, target);

            // Do nothing if it has not changed from the initial value
            bool val = control.Checked;
            object prev = _controller.GetItemValue(itemId, relation);
            if ((prev == null) && !val)
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (bool) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(selected);
            }

            if (relation == null)
            {
                relation = new Relation { Country = target };
                settings.Relations.Add(relation);
                Scenarios.SetCountryRelation(selected, relation);
            }

            _controller.OutputItemValueChangedLog(itemId, val, selected, relation);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, relation);

            // Update value
            _controller.SetItemValue(itemId, val, relation);

            // Set the edited flag
            _controller.SetItemDirty(itemId, relation, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, relation);
        }

        /// <summary>
        ///     Processing when changing the check status of a check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationCountryItemCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(selected);

            // Do nothing if it has not changed from the initial value
            Country val = control.Checked ? target : Country.None;
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev == null) && (val == Country.None))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (Country) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(selected);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, settings);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationNonAggressionItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            Treaty treaty = Scenarios.GetNonAggression(selected, target);

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, treaty);

            // Update value
            _controller.SetItemValue(itemId, val, treaty);

            // Set the edited flag
            _controller.SetItemDirty(itemId, treaty);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, treaty);
        }

        /// <summary>
        ///     Processing when changing the check status of a check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationNonAggressionItemCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            Treaty treaty = Scenarios.GetNonAggression(selected, target);

            // Do nothing if it has not changed from the initial value
            bool val = control.Checked;
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && !val)
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (bool) prev))
            {
                return;
            }

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, treaty);

            // Update value
            if (val)
            {
                treaty = new Treaty
                {
                    Type = TreatyType.NonAggression,
                    Country1 = selected,
                    Country2 = target,
                    StartDate = new GameDate(),
                    EndDate = new GameDate(),
                    Id = Scenarios.GetNewTypeId(Scenarios.DefaultTreatyType, 1)
                };
                Scenarios.Data.GlobalData.NonAggressions.Add(treaty);
                Scenarios.SetNonAggression(treaty);
            }
            else
            {
                Scenarios.Data.GlobalData.NonAggressions.Remove(treaty);
                Scenarios.RemoveNonAggression(treaty);
            }

            _controller.OutputItemValueChangedLog(itemId, val, !val, treaty);

            // Set the edited flag
            _controller.SetItemDirty(itemId, treaty);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, val ? treaty : null);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationPeaceItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            Treaty treaty = Scenarios.GetPeace(selected, target);

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, treaty);

            // Update value
            _controller.SetItemValue(itemId, val, treaty);

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // Set the edited flag
            _controller.SetItemDirty(itemId, treaty);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, treaty);
        }

        /// <summary>
        ///     Processing when changing the check status of a check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationPeaceItemCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            Treaty treaty = Scenarios.GetPeace(selected, target);

            // Do nothing if it has not changed from the initial value
            bool val = control.Checked;
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && !val)
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (bool) prev))
            {
                return;
            }

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, treaty);

            // Update value
            if (val)
            {
                treaty = new Treaty
                {
                    Type = TreatyType.Peace,
                    Country1 = selected,
                    Country2 = target,
                    StartDate = new GameDate(),
                    EndDate = new GameDate(),
                    Id = Scenarios.GetNewTypeId(Scenarios.DefaultTreatyType, 1)
                };
                Scenarios.Data.GlobalData.Peaces.Add(treaty);
                Scenarios.SetPeace(treaty);
            }
            else
            {
                Scenarios.Data.GlobalData.Peaces.Remove(treaty);
                Scenarios.RemovePeace(treaty);
            }

            _controller.OutputItemValueChangedLog(itemId, val, !val, treaty);

            // Set the edited flag
            _controller.SetItemDirty(itemId, treaty);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, val ? treaty : null);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRelationIntelligenceItemNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Country selected = GetSelectedRelationCountry();
            if (selected == Country.None)
            {
                return;
            }
            Country target = GetTargetRelationCountry();
            if (target == Country.None)
            {
                return;
            }

            NumericUpDown control = sender as NumericUpDown;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(selected);
            SpySettings spy = Scenarios.GetCountryIntelligence(selected, target);

            // Do nothing if it has not changed from the initial value
            int val = (int) control.Value;
            object prev = _controller.GetItemValue(itemId, spy);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val))
            {
                _controller.UpdateItemValue(control, spy);
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(selected);
            }

            if (spy == null)
            {
                spy = new SpySettings { Country = target };
                Scenarios.SetCountryIntelligence(selected, spy);
            }

            _controller.OutputItemValueChangedLog(itemId, val, selected, spy);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, spy);

            // Update value
            _controller.SetItemValue(itemId, val, spy);

            // Set the edited flag
            _controller.SetItemDirty(itemId, spy, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, spy);
        }

        #endregion

        #endregion

        #region Trade tab

        #region Trade tab ―――― common

        /// <summary>
        ///     Initialize the edit items on the Trade tab
        /// </summary>
        private void InitTradeTab()
        {
            InitTradeInfoItems();
            InitTradeDealsItems();
        }

        /// <summary>
        ///     Update edits on the Trade tab
        /// </summary>
        private void UpdateTradeTab()
        {
            // Do nothing if initialized
            if (_tabPageInitialized[(int) TabPageNo.Trade])
            {
                return;
            }

            // Update Trading Country Combo Box
            UpdateCountryComboBox(tradeCountryComboBox1, false);
            UpdateCountryComboBox(tradeCountryComboBox2, false);

            // Update trade list
            UpdateTradeList();

            // Activate the trade list
            EnableTradeList();

            // Activate the new button
            EnableTradeNewButton();

            // Set the initialized flag
            _tabPageInitialized[(int) TabPageNo.Trade] = true;
        }

        /// <summary>
        ///     Processing when loading a form on the trade tab
        /// </summary>
        private void OnTradeTabPageFormLoad()
        {
            // Initialize the trade tab
            InitTradeTab();
        }

        /// <summary>
        ///     Processing when reading a file on the trade tab
        /// </summary>
        private void OnTradeTabPageFileLoad()
        {
            // Do nothing unless the trade tab is selected
            if (_tabPageNo != TabPageNo.Trade)
            {
                return;
            }

            // Update the display at the first transition
            UpdateTradeTab();
        }

        /// <summary>
        ///     Processing when trade tab is selected
        /// </summary>
        private void OnTradeTabPageSelected()
        {
            // Do nothing if scenario not loaded
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // Update the display at the first transition
            UpdateTradeTab();
        }

        #endregion

        #region Trade tab ――――Trade list

        /// <summary>
        ///     Update the trade list display
        /// </summary>
        private void UpdateTradeList()
        {
            List<Treaty> trades = Scenarios.Data.GlobalData.Trades;
            tradeListView.BeginUpdate();
            tradeListView.Items.Clear();
            foreach (Treaty treaty in trades)
            {
                tradeListView.Items.Add(CreateTradeListViewItem(treaty));
            }
            tradeListView.EndUpdate();

            // Disable edit items
            DisableTradeInfoItems();
            DisableTradeDealsItems();
            DisableTradeButtons();

            // Clear edit items
            ClearTradeInfoItems();
            ClearTradeDealsItems();
        }

        /// <summary>
        ///     Activate the trade list
        /// </summary>
        private void EnableTradeList()
        {
            tradeListView.Enabled = true;
        }

        /// <summary>
        ///     Activate the new button
        /// </summary>
        private void EnableTradeNewButton()
        {
            tradeNewButton.Enabled = true;
        }

        /// <summary>
        ///     delete / / Up / / Enable the down button
        /// </summary>
        private void EnableTradeButtons()
        {
            int index = tradeListView.SelectedIndices[0];
            int count = tradeListView.Items.Count;
            tradeUpButton.Enabled = index > 0;
            tradeDownButton.Enabled = index < count - 1;
            tradeRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     delete / / Up / / Disable the down button
        /// </summary>
        private void DisableTradeButtons()
        {
            tradeUpButton.Enabled = false;
            tradeDownButton.Enabled = false;
            tradeRemoveButton.Enabled = false;
        }

        /// <summary>
        ///     Processing when changing the selection item in the trade list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable edit items if there are no selections
            if (tradeListView.SelectedIndices.Count == 0)
            {
                // Disable edit items
                DisableTradeInfoItems();
                DisableTradeDealsItems();
                DisableTradeButtons();

                // Clear edit items
                ClearTradeInfoItems();
                ClearTradeDealsItems();
                return;
            }

            Treaty treaty = GetSelectedTrade();

            // Update edit items
            UpdateTradeInfoItems(treaty);
            UpdateTradeDealsItems(treaty);

            // Enable edit items
            EnableTradeInfoItems();
            EnableTradeDealsItems();
            EnableTradeButtons();
        }

        /// <summary>
        ///     Processing when the button is pressed on the trade
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeUpButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Treaty> trades = scenario.GlobalData.Trades;

            // Move items in the trade list view
            int index = tradeListView.SelectedIndices[0];
            ListViewItem item = tradeListView.Items[index];
            tradeListView.Items.RemoveAt(index);
            tradeListView.Items.Insert(index - 1, item);
            tradeListView.Items[index - 1].Focused = true;
            tradeListView.Items[index - 1].Selected = true;
            tradeListView.EnsureVisible(index - 1);

            // Move items in the trade list
            Treaty trade = trades[index];
            trades.RemoveAt(index);
            trades.Insert(index - 1, trade);

            // Set the edited flag
            Scenarios.Data.SetDirty();
            Scenarios.SetDirty();
        }

        /// <summary>
        ///     Processing when the button is pressed under trade
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeDownButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Treaty> trades = scenario.GlobalData.Trades;

            // Move items in the trade list view
            int index = tradeListView.SelectedIndices[0];
            ListViewItem item = tradeListView.Items[index];
            tradeListView.Items.RemoveAt(index);
            tradeListView.Items.Insert(index + 1, item);
            tradeListView.Items[index + 1].Focused = true;
            tradeListView.Items[index + 1].Selected = true;
            tradeListView.EnsureVisible(index + 1);

            // Move items in the trade list
            Treaty trade = trades[index];
            trades.RemoveAt(index);
            trades.Insert(index + 1, trade);

            // Set the edited flag
            Scenarios.Data.SetDirty();
            Scenarios.SetDirty();
        }

        /// <summary>
        ///     Processing when a new trade button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeNewButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Treaty> trades = scenario.GlobalData.Trades;

            // Add an item to the trade list
            Treaty trade = new Treaty
            {
                StartDate = new GameDate(),
                EndDate = new GameDate(),
                Id = Scenarios.GetNewTypeId(Scenarios.DefaultTreatyType, 1)
            };
            trades.Add(trade);

            // Add an item to the trade list view
            ListViewItem item = new ListViewItem { Tag = trade };
            item.SubItems.Add("");
            item.SubItems.Add("");
            tradeListView.Items.Add(item);

            // Set the edited flag
            trade.SetDirty(Treaty.ItemId.StartYear);
            trade.SetDirty(Treaty.ItemId.StartMonth);
            trade.SetDirty(Treaty.ItemId.StartDay);
            trade.SetDirty(Treaty.ItemId.EndYear);
            trade.SetDirty(Treaty.ItemId.EndMonth);
            trade.SetDirty(Treaty.ItemId.EndDay);
            trade.SetDirty(Treaty.ItemId.Type);
            trade.SetDirty(Treaty.ItemId.Id);
            trade.SetDirty(Treaty.ItemId.Cancel);
            Scenarios.Data.SetDirty();
            Scenarios.SetDirty();

            // Select the added item
            if (tradeListView.SelectedIndices.Count > 0)
            {
                ListViewItem prev = tradeListView.SelectedItems[0];
                prev.Focused = false;
                prev.Selected = false;
            }
            item.Focused = true;
            item.Selected = true;
        }

        /// <summary>
        ///     Processing when the delete button of trade is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeRemoveButtonClick(object sender, EventArgs e)
        {
            Scenario scenario = Scenarios.Data;
            List<Treaty> trades = scenario.GlobalData.Trades;

            int index = tradeListView.SelectedIndices[0];
            Treaty trade = trades[index];

            // type When id idDelete the pair of
            Scenarios.RemoveTypeId(trade.Id);

            // Remove an item from the trade list
            trades.RemoveAt(index);

            // Remove an item from the trade list view
            tradeListView.Items.RemoveAt(index);

            // Set the edited flag
            Scenarios.Data.SetDirty();
            Scenarios.SetDirty();

            // Select next to the deleted item
            if (index == trades.Count)
            {
                index--;
            }
            if (index >= 0)
            {
                tradeListView.Items[index].Focused = true;
                tradeListView.Items[index].Selected = true;
            }
        }

        /// <summary>
        ///     Set the character string of the selection item in the trade list view
        /// </summary>
        /// <param name="no">Item Number</param>
        /// <param name="s">Character string</param>
        public void SetTradeListItemText(int no, string s)
        {
            tradeListView.SelectedItems[0].SubItems[no].Text = s;
        }

        /// <summary>
        ///     Create a trade list item
        /// </summary>
        /// <param name="treaty">Trade information</param>
        /// <returns>Trade list items</returns>
        private static ListViewItem CreateTradeListViewItem(Treaty treaty)
        {
            ListViewItem item = new ListViewItem
            {
                Text = Countries.GetName(treaty.Country1),
                Tag = treaty
            };
            item.SubItems.Add(Countries.GetName(treaty.Country2));
            item.SubItems.Add(treaty.GetTradeString());

            return item;
        }

        /// <summary>
        ///     Get selected trade information
        /// </summary>
        /// <returns>Selected trade information</returns>
        private Treaty GetSelectedTrade()
        {
            return tradeListView.SelectedItems.Count > 0 ? tradeListView.SelectedItems[0].Tag as Treaty : null;
        }

        #endregion

        #region Trade tab ―――― Trade information

        /// <summary>
        ///     Initialize trade information edit items
        /// </summary>
        private void InitTradeInfoItems()
        {
            _itemControls.Add(ScenarioEditorItemId.TradeStartYear, tradeStartYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeStartMonth, tradeStartMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeStartDay, tradeStartDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeEndYear, tradeEndYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeEndMonth, tradeEndMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeEndDay, tradeEndDayTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeType, tradeTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeId, tradeIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.TradeCancel, tradeCancelCheckBox);

            tradeStartYearTextBox.Tag = ScenarioEditorItemId.TradeStartYear;
            tradeStartMonthTextBox.Tag = ScenarioEditorItemId.TradeStartMonth;
            tradeStartDayTextBox.Tag = ScenarioEditorItemId.TradeStartDay;
            tradeEndYearTextBox.Tag = ScenarioEditorItemId.TradeEndYear;
            tradeEndMonthTextBox.Tag = ScenarioEditorItemId.TradeEndMonth;
            tradeEndDayTextBox.Tag = ScenarioEditorItemId.TradeEndDay;
            tradeTypeTextBox.Tag = ScenarioEditorItemId.TradeType;
            tradeIdTextBox.Tag = ScenarioEditorItemId.TradeId;
            tradeCancelCheckBox.Tag = ScenarioEditorItemId.TradeCancel;
        }

        /// <summary>
        ///     Update trade information edits
        /// </summary>
        /// <param name="treaty">Agreement</param>
        private void UpdateTradeInfoItems(Treaty treaty)
        {
            // Update the display of edit items
            _controller.UpdateItemValue(tradeStartYearTextBox, treaty);
            _controller.UpdateItemValue(tradeStartMonthTextBox, treaty);
            _controller.UpdateItemValue(tradeStartDayTextBox, treaty);
            _controller.UpdateItemValue(tradeEndYearTextBox, treaty);
            _controller.UpdateItemValue(tradeEndMonthTextBox, treaty);
            _controller.UpdateItemValue(tradeEndDayTextBox, treaty);
            _controller.UpdateItemValue(tradeTypeTextBox, treaty);
            _controller.UpdateItemValue(tradeIdTextBox, treaty);
            _controller.UpdateItemValue(tradeCancelCheckBox, treaty);

            // Update the color of the edit item
            _controller.UpdateItemColor(tradeStartYearTextBox, treaty);
            _controller.UpdateItemColor(tradeStartMonthTextBox, treaty);
            _controller.UpdateItemColor(tradeStartDayTextBox, treaty);
            _controller.UpdateItemColor(tradeEndYearTextBox, treaty);
            _controller.UpdateItemColor(tradeEndMonthTextBox, treaty);
            _controller.UpdateItemColor(tradeEndDayTextBox, treaty);
            _controller.UpdateItemColor(tradeTypeTextBox, treaty);
            _controller.UpdateItemColor(tradeIdTextBox, treaty);
            _controller.UpdateItemColor(tradeCancelCheckBox, treaty);
        }

        /// <summary>
        ///     Clear the display of trade information edit items
        /// </summary>
        private void ClearTradeInfoItems()
        {
            tradeStartYearTextBox.Text = "";
            tradeStartMonthTextBox.Text = "";
            tradeStartDayTextBox.Text = "";
            tradeEndYearTextBox.Text = "";
            tradeEndMonthTextBox.Text = "";
            tradeEndDayTextBox.Text = "";
            tradeTypeTextBox.Text = "";
            tradeIdTextBox.Text = "";
            tradeCancelCheckBox.Checked = false;
        }

        /// <summary>
        ///     Enable trade information edits
        /// </summary>
        private void EnableTradeInfoItems()
        {
            tradeInfoGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for trade information
        /// </summary>
        private void DisableTradeInfoItems()
        {
            tradeInfoGroupBox.Enabled = false;
        }

        #endregion

        #region Trade tab ―――― Trade details

        /// <summary>
        ///     Initialize trade content edit items
        /// </summary>
        private void InitTradeDealsItems()
        {
            _itemControls.Add(ScenarioEditorItemId.TradeCountry1, tradeCountryComboBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeCountry2, tradeCountryComboBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeEnergy1, tradeEnergyTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeEnergy2, tradeEnergyTextBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeMetal1, tradeMetalTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeMetal2, tradeMetalTextBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeRareMaterials1, tradeRareMaterialsTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeRareMaterials2, tradeRareMaterialsTextBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeOil1, tradeOilTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeOil2, tradeOilTextBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeSupplies1, tradeSuppliesTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeSupplies2, tradeSuppliesTextBox2);
            _itemControls.Add(ScenarioEditorItemId.TradeMoney1, tradeMoneyTextBox1);
            _itemControls.Add(ScenarioEditorItemId.TradeMoney2, tradeMoneyTextBox2);

            tradeCountryComboBox1.Tag = ScenarioEditorItemId.TradeCountry1;
            tradeCountryComboBox2.Tag = ScenarioEditorItemId.TradeCountry2;
            tradeEnergyTextBox1.Tag = ScenarioEditorItemId.TradeEnergy1;
            tradeEnergyTextBox2.Tag = ScenarioEditorItemId.TradeEnergy2;
            tradeMetalTextBox1.Tag = ScenarioEditorItemId.TradeMetal1;
            tradeMetalTextBox2.Tag = ScenarioEditorItemId.TradeMetal2;
            tradeRareMaterialsTextBox1.Tag = ScenarioEditorItemId.TradeRareMaterials1;
            tradeRareMaterialsTextBox2.Tag = ScenarioEditorItemId.TradeRareMaterials2;
            tradeOilTextBox1.Tag = ScenarioEditorItemId.TradeOil1;
            tradeOilTextBox2.Tag = ScenarioEditorItemId.TradeOil2;
            tradeSuppliesTextBox1.Tag = ScenarioEditorItemId.TradeSupplies1;
            tradeSuppliesTextBox2.Tag = ScenarioEditorItemId.TradeSupplies2;
            tradeMoneyTextBox1.Tag = ScenarioEditorItemId.TradeMoney1;
            tradeMoneyTextBox2.Tag = ScenarioEditorItemId.TradeMoney2;

            // Trade resource label
            tradeEnergyLabel.Text = Config.GetText(TextId.ResourceEnergy);
            tradeMetalLabel.Text = Config.GetText(TextId.ResourceMetal);
            tradeRareMaterialsLabel.Text = Config.GetText(TextId.ResourceRareMaterials);
            tradeOilLabel.Text = Config.GetText(TextId.ResourceOil);
            tradeSuppliesLabel.Text = Config.GetText(TextId.ResourceSupplies);
            tradeMoneyLabel.Text = Config.GetText(TextId.ResourceMoney);
        }

        /// <summary>
        ///     Update trade content edits
        /// </summary>
        /// <param name="treaty">Agreement</param>
        private void UpdateTradeDealsItems(Treaty treaty)
        {
            // Update the display of edit items
            _controller.UpdateItemValue(tradeCountryComboBox1, treaty);
            _controller.UpdateItemValue(tradeCountryComboBox2, treaty);
            _controller.UpdateItemValue(tradeEnergyTextBox1, treaty);
            _controller.UpdateItemValue(tradeEnergyTextBox2, treaty);
            _controller.UpdateItemValue(tradeMetalTextBox1, treaty);
            _controller.UpdateItemValue(tradeMetalTextBox2, treaty);
            _controller.UpdateItemValue(tradeRareMaterialsTextBox1, treaty);
            _controller.UpdateItemValue(tradeRareMaterialsTextBox2, treaty);
            _controller.UpdateItemValue(tradeOilTextBox1, treaty);
            _controller.UpdateItemValue(tradeOilTextBox2, treaty);
            _controller.UpdateItemValue(tradeSuppliesTextBox1, treaty);
            _controller.UpdateItemValue(tradeSuppliesTextBox2, treaty);
            _controller.UpdateItemValue(tradeMoneyTextBox1, treaty);
            _controller.UpdateItemValue(tradeMoneyTextBox2, treaty);

            // Update the color of the edit item
            _controller.UpdateItemColor(tradeEnergyTextBox1, treaty);
            _controller.UpdateItemColor(tradeEnergyTextBox2, treaty);
            _controller.UpdateItemColor(tradeMetalTextBox1, treaty);
            _controller.UpdateItemColor(tradeMetalTextBox2, treaty);
            _controller.UpdateItemColor(tradeRareMaterialsTextBox1, treaty);
            _controller.UpdateItemColor(tradeRareMaterialsTextBox2, treaty);
            _controller.UpdateItemColor(tradeOilTextBox1, treaty);
            _controller.UpdateItemColor(tradeOilTextBox2, treaty);
            _controller.UpdateItemColor(tradeSuppliesTextBox1, treaty);
            _controller.UpdateItemColor(tradeSuppliesTextBox2, treaty);
            _controller.UpdateItemColor(tradeMoneyTextBox1, treaty);
            _controller.UpdateItemColor(tradeMoneyTextBox2, treaty);
        }

        /// <summary>
        ///     Clear the display of edit items of trade details
        /// </summary>
        private void ClearTradeDealsItems()
        {
            tradeCountryComboBox1.SelectedIndex = -1;
            tradeCountryComboBox2.SelectedIndex = -1;
            tradeEnergyTextBox1.Text = "";
            tradeEnergyTextBox2.Text = "";
            tradeMetalTextBox1.Text = "";
            tradeMetalTextBox2.Text = "";
            tradeRareMaterialsTextBox1.Text = "";
            tradeRareMaterialsTextBox2.Text = "";
            tradeOilTextBox1.Text = "";
            tradeOilTextBox2.Text = "";
            tradeSuppliesTextBox1.Text = "";
            tradeSuppliesTextBox2.Text = "";
            tradeMoneyTextBox1.Text = "";
            tradeMoneyTextBox2.Text = "";
        }

        /// <summary>
        ///     Enable trade content edits
        /// </summary>
        private void EnableTradeDealsItems()
        {
            tradeDealsGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable trade content edits
        /// </summary>
        private void DisableTradeDealsItems()
        {
            tradeDealsGroupBox.Enabled = false;
        }

        /// <summary>
        ///     Processing when the trading country replacement button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeSwapButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            // Swap values
            Country country = treaty.Country1;
            treaty.Country1 = treaty.Country2;
            treaty.Country2 = country;

            treaty.Energy = -treaty.Energy;
            treaty.Metal = -treaty.Metal;
            treaty.RareMaterials = -treaty.RareMaterials;
            treaty.Oil = -treaty.Oil;
            treaty.Supplies = -treaty.Supplies;
            treaty.Money = -treaty.Money;

            // Set the edited flag
            treaty.SetDirty(Treaty.ItemId.Country1);
            treaty.SetDirty(Treaty.ItemId.Country2);
            treaty.SetDirty(Treaty.ItemId.Energy);
            treaty.SetDirty(Treaty.ItemId.Metal);
            treaty.SetDirty(Treaty.ItemId.RareMaterials);
            treaty.SetDirty(Treaty.ItemId.Oil);
            treaty.SetDirty(Treaty.ItemId.Supplies);
            treaty.SetDirty(Treaty.ItemId.Money);
            Scenarios.SetDirty();

            // Update items in the trade list view
            ListViewItem item = tradeListView.SelectedItems[0];
            item.Text = Countries.GetName(treaty.Country1);
            item.SubItems[1].Text = Countries.GetName(treaty.Country2);
            item.SubItems[2].Text = treaty.GetTradeString();

            // Update edit items
            UpdateTradeDealsItems(treaty);
        }

        #endregion

        #region Trade tab ―――― Edit items

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val, treaty))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, treaty);

            // Update value
            _controller.SetItemValue(itemId, val, treaty);

            // Set the edited flag
            _controller.SetItemDirty(itemId, treaty);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, treaty);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeDoubleItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Returns the value if the string cannot be converted to a number
            double val;
            if (!DoubleHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, treaty);
            if ((prev == null) && DoubleHelper.IsZero(val))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && DoubleHelper.IsEqual(val, (double) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val, treaty))
            {
                _controller.UpdateItemValue(control, treaty);
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, treaty);

            // Update value
            _controller.SetItemValue(itemId, val, treaty);

            // Set the edited flag
            _controller.SetItemDirty(itemId, treaty);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, treaty);
        }

        /// <summary>
        ///     Processing when changing the check status of a check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeItemCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Do nothing if the value does not change
            bool val = control.Checked;
            if (val == (bool) _controller.GetItemValue(itemId, treaty))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, treaty);

            // Update value
            _controller.SetItemValue(itemId, val, treaty);

            // Set the edited flag
            _controller.SetItemDirty(itemId, treaty);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, treaty);
        }

        /// <summary>
        ///     Item drawing process of combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeCountryItemComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index < 0)
            {
                return;
            }

            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }

            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;
            Country val = (Country) _controller.GetItemValue(itemId, treaty);
            Country sel = (Country) _controller.GetListItemValue(itemId, e.Index);
            Brush brush = (val == sel) && _controller.IsItemDirty(itemId, treaty)
                ? new SolidBrush(Color.Red)
                : new SolidBrush(SystemColors.WindowText);
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeCountryItemComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }

            // Do nothing if there is no selection
            if (control.SelectedIndex < 0)
            {
                return;
            }

            Treaty treaty = GetSelectedTrade();
            if (treaty == null)
            {
                return;
            }

            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Do nothing if the value does not change
            Country val = Countries.Tags[control.SelectedIndex];
            if (val == (Country) _controller.GetItemValue(itemId, treaty))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, treaty);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, treaty);

            // Update value
            _controller.SetItemValue(itemId, val, treaty);

            // Set the edited flag
            _controller.SetItemDirty(itemId, treaty);

            // Update drawing to change item color
            control.Refresh();

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, treaty);
        }

        #endregion

        #endregion

        #region National tab

        #region National tab ―――― common

        /// <summary>
        ///     Initialize the national tab
        /// </summary>
        private void InitCountryTab()
        {
            InitCountryInfoItems();
            InitCountryModifierItems();
            InitCountryResourceItems();
            InitCountryAiItems();
        }

        /// <summary>
        ///     Update the edit items on the national tab
        /// </summary>
        private void UpdateCountryTab()
        {
            // Do nothing if initialized
            if (_tabPageInitialized[(int) TabPageNo.Country])
            {
                return;
            }

            // Update Brotherhood Combo Box
            UpdateCountryComboBox(regularIdComboBox, true);

            // Update national list box
            UpdateCountryListBox(countryListBox);

            // Enable national list box
            EnableCountryListBox();

            // Disable edit items
            DisableCountryInfoItems();
            DisableCountryModifierItems();
            DisableCountryResourceItems();
            DisableCountryAiItems();

            // Clear edit items
            ClearCountryInfoItems();
            ClearCountryModifierItems();
            ClearCountryResourceItems();
            ClearCountryAiItems();

            // Set the initialized flag
            _tabPageInitialized[(int) TabPageNo.Country] = true;
        }

        /// <summary>
        ///     Processing when loading the form of the national tab
        /// </summary>
        private void OnCountryTabPageFormLoad()
        {
            // Initialize the national tab
            InitCountryTab();
        }

        /// <summary>
        ///     Processing when reading a file on the national tab
        /// </summary>
        private void OnCountryTabPageFileLoad()
        {
            // Do nothing unless the national tab is selected
            if (_tabPageNo != TabPageNo.Country)
            {
                return;
            }

            // Update the display at the first transition
            UpdateCountryTab();
        }

        /// <summary>
        ///     Processing when selecting a national tab
        /// </summary>
        private void OnCountryTabPageSelected()
        {
            // Do nothing if scenario not loaded
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // Update the display at the first transition
            UpdateCountryTab();
        }

        #endregion

        #region National tab ―――― Nation

        /// <summary>
        ///     Activate the national list box
        /// </summary>
        private void EnableCountryListBox()
        {
            countryListBox.Enabled = true;
        }

        /// <summary>
        ///     Processing when changing the selection item of the national list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable edit items if there are no selections
            if (countryListBox.SelectedIndex < 0)
            {
                // Disable edit items
                DisableCountryInfoItems();
                DisableCountryModifierItems();
                DisableCountryResourceItems();
                DisableCountryAiItems();

                // Clear edit items
                ClearCountryInfoItems();
                ClearCountryModifierItems();
                ClearCountryResourceItems();
                ClearCountryAiItems();
                return;
            }

            Country country = GetSelectedCountry();
            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // Update edit items
            UpdateCountryInfoItems(country, settings);
            UpdateCountryModifierItems(settings);
            UpdateCountryResourceItems(settings);
            UpdateCountryAiItems(settings);

            // Enable edit items
            EnableCountryInfoItems();
            EnableCountryModifierItems();
            EnableCountryResourceItems();
            EnableCountryAiItems();
        }

        /// <summary>
        ///     Get the selected nation
        /// </summary>
        /// <returns>Selected nation</returns>
        private Country GetSelectedCountry()
        {
            return countryListBox.SelectedIndex >= 0 ? Countries.Tags[countryListBox.SelectedIndex] : Country.None;
        }

        #endregion

        #region National tab ―――― National information

        /// <summary>
        ///     Initialize the edit items of national information
        /// </summary>
        private void InitCountryInfoItems()
        {
            _itemControls.Add(ScenarioEditorItemId.CountryNameKey, countryNameKeyTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryNameString, countryNameStringTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryFlagExt, flagExtTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryRegularId, regularIdComboBox);
            _itemControls.Add(ScenarioEditorItemId.CountryBelligerence, belligerenceTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryDissent, dissentTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryExtraTc, extraTcTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryNuke, nukeTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryNukeYear, nukeYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryNukeMonth, nukeMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryNukeDay, nukeDayTextBox);

            countryNameKeyTextBox.Tag = ScenarioEditorItemId.CountryNameKey;
            countryNameStringTextBox.Tag = ScenarioEditorItemId.CountryNameString;
            flagExtTextBox.Tag = ScenarioEditorItemId.CountryFlagExt;
            regularIdComboBox.Tag = ScenarioEditorItemId.CountryRegularId;
            belligerenceTextBox.Tag = ScenarioEditorItemId.CountryBelligerence;
            dissentTextBox.Tag = ScenarioEditorItemId.CountryDissent;
            extraTcTextBox.Tag = ScenarioEditorItemId.CountryExtraTc;
            nukeTextBox.Tag = ScenarioEditorItemId.CountryNuke;
            nukeYearTextBox.Tag = ScenarioEditorItemId.CountryNukeYear;
            nukeMonthTextBox.Tag = ScenarioEditorItemId.CountryNukeMonth;
            nukeDayTextBox.Tag = ScenarioEditorItemId.CountryNukeDay;
        }

        /// <summary>
        ///     Update national information edit items
        /// </summary>
        /// <param name="country">Selected country</param>
        /// <param name="settings">National setting</param>
        private void UpdateCountryInfoItems(Country country, CountrySettings settings)
        {
            // Update the display of edit items
            _controller.UpdateItemValue(countryNameKeyTextBox, settings);
            _controller.UpdateItemValue(countryNameStringTextBox, country, settings);
            _controller.UpdateItemValue(flagExtTextBox, settings);
            _controller.UpdateItemValue(regularIdComboBox, settings);
            _controller.UpdateItemValue(belligerenceTextBox, settings);
            _controller.UpdateItemValue(dissentTextBox, settings);
            _controller.UpdateItemValue(extraTcTextBox, settings);
            _controller.UpdateItemValue(nukeTextBox, settings);
            _controller.UpdateItemValue(nukeYearTextBox, settings);
            _controller.UpdateItemValue(nukeMonthTextBox, settings);
            _controller.UpdateItemValue(nukeDayTextBox, settings);

            // Update the color of the edit item
            _controller.UpdateItemColor(countryNameKeyTextBox, settings);
            _controller.UpdateItemColor(countryNameStringTextBox, country, settings);
            _controller.UpdateItemColor(flagExtTextBox, settings);
            _controller.UpdateItemColor(belligerenceTextBox, settings);
            _controller.UpdateItemColor(dissentTextBox, settings);
            _controller.UpdateItemColor(extraTcTextBox, settings);
            _controller.UpdateItemColor(nukeTextBox, settings);
            _controller.UpdateItemColor(nukeYearTextBox, settings);
            _controller.UpdateItemColor(nukeMonthTextBox, settings);
            _controller.UpdateItemColor(nukeDayTextBox, settings);
        }

        /// <summary>
        ///     Clear the display of edit items of national information
        /// </summary>
        private void ClearCountryInfoItems()
        {
            countryNameKeyTextBox.Text = "";
            countryNameStringTextBox.Text = "";
            flagExtTextBox.Text = "";
            regularIdComboBox.SelectedIndex = -1;
            belligerenceTextBox.Text = "";
            dissentTextBox.Text = "";
            extraTcTextBox.Text = "";
            nukeTextBox.Text = "";
            nukeYearTextBox.Text = "";
            nukeMonthTextBox.Text = "";
            nukeDayTextBox.Text = "";
        }

        /// <summary>
        ///     Enable editing items for national information
        /// </summary>
        private void EnableCountryInfoItems()
        {
            countryInfoGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for national information
        /// </summary>
        private void DisableCountryInfoItems()
        {
            countryInfoGroupBox.Enabled = false;
        }

        #endregion

        #region National tab ―――― Correction value

        /// <summary>
        ///     Initialize the edit items of the national correction value
        /// </summary>
        private void InitCountryModifierItems()
        {
            _itemControls.Add(ScenarioEditorItemId.CountryGroundDefEff, groundDefEffTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryPeacetimeIcModifier, peacetimeIcModifierTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryWartimeIcModifier, wartimeIcModifierTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryIndustrialModifier, industrialModifierTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryRelativeManpower, relativeManpowerTextBox);

            groundDefEffTextBox.Tag = ScenarioEditorItemId.CountryGroundDefEff;
            peacetimeIcModifierTextBox.Tag = ScenarioEditorItemId.CountryPeacetimeIcModifier;
            wartimeIcModifierTextBox.Tag = ScenarioEditorItemId.CountryWartimeIcModifier;
            industrialModifierTextBox.Tag = ScenarioEditorItemId.CountryIndustrialModifier;
            relativeManpowerTextBox.Tag = ScenarioEditorItemId.CountryRelativeManpower;
        }

        /// <summary>
        ///     Update national correction value edit items
        /// </summary>
        /// <param name="settings">National setting</param>
        private void UpdateCountryModifierItems(CountrySettings settings)
        {
            // Update the display of edit items
            _controller.UpdateItemValue(groundDefEffTextBox, settings);
            _controller.UpdateItemValue(peacetimeIcModifierTextBox, settings);
            _controller.UpdateItemValue(wartimeIcModifierTextBox, settings);
            _controller.UpdateItemValue(industrialModifierTextBox, settings);
            _controller.UpdateItemValue(relativeManpowerTextBox, settings);

            // Update the color of the edit item
            _controller.UpdateItemColor(groundDefEffTextBox, settings);
            _controller.UpdateItemColor(peacetimeIcModifierTextBox, settings);
            _controller.UpdateItemColor(wartimeIcModifierTextBox, settings);
            _controller.UpdateItemColor(industrialModifierTextBox, settings);
            _controller.UpdateItemColor(relativeManpowerTextBox, settings);
        }

        /// <summary>
        ///     Clear the display of the edit item of the national correction value
        /// </summary>
        private void ClearCountryModifierItems()
        {
            groundDefEffTextBox.Text = "";
            peacetimeIcModifierTextBox.Text = "";
            wartimeIcModifierTextBox.Text = "";
            industrialModifierTextBox.Text = "";
            relativeManpowerTextBox.Text = "";
        }

        /// <summary>
        ///     Enable edit items for national correction values
        /// </summary>
        private void EnableCountryModifierItems()
        {
            countryModifierGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for national correction values
        /// </summary>
        private void DisableCountryModifierItems()
        {
            countryModifierGroupBox.Enabled = false;
        }

        #endregion

        #region National tab ―――― Resource information

        /// <summary>
        ///     Initialize the edit items of national resource information
        /// </summary>
        private void InitCountryResourceItems()
        {
            _itemControls.Add(ScenarioEditorItemId.CountryEnergy, countryEnergyTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryMetal, countryMetalTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryRareMaterials, countryRareMaterialsTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOil, countryOilTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountrySupplies, countrySuppliesTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryMoney, countryMoneyTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryTransports, countryTransportsTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryEscorts, countryEscortsTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryManpower, countryManpowerTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOffmapEnergy, offmapEnergyTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOffmapMetal, offmapMetalTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOffmapRareMaterials, offmapRareMaterialsTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOffmapOil, offmapOilTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOffmapSupplies, offmapSuppliesTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOffmapMoney, offmapMoneyTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOffmapTransports, offmapTransportsTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOffmapEscorts, offmapEscortsTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOffmapManpower, offmapManpowerTextBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOffmapIc, offmapIcTextBox);

            countryEnergyTextBox.Tag = ScenarioEditorItemId.CountryEnergy;
            countryMetalTextBox.Tag = ScenarioEditorItemId.CountryMetal;
            countryRareMaterialsTextBox.Tag = ScenarioEditorItemId.CountryRareMaterials;
            countryOilTextBox.Tag = ScenarioEditorItemId.CountryOil;
            countrySuppliesTextBox.Tag = ScenarioEditorItemId.CountrySupplies;
            countryMoneyTextBox.Tag = ScenarioEditorItemId.CountryMoney;
            countryTransportsTextBox.Tag = ScenarioEditorItemId.CountryTransports;
            countryEscortsTextBox.Tag = ScenarioEditorItemId.CountryEscorts;
            countryManpowerTextBox.Tag = ScenarioEditorItemId.CountryManpower;
            offmapEnergyTextBox.Tag = ScenarioEditorItemId.CountryOffmapEnergy;
            offmapMetalTextBox.Tag = ScenarioEditorItemId.CountryOffmapMetal;
            offmapRareMaterialsTextBox.Tag = ScenarioEditorItemId.CountryOffmapRareMaterials;
            offmapOilTextBox.Tag = ScenarioEditorItemId.CountryOffmapOil;
            offmapSuppliesTextBox.Tag = ScenarioEditorItemId.CountryOffmapSupplies;
            offmapMoneyTextBox.Tag = ScenarioEditorItemId.CountryOffmapMoney;
            offmapTransportsTextBox.Tag = ScenarioEditorItemId.CountryOffmapTransports;
            offmapEscortsTextBox.Tag = ScenarioEditorItemId.CountryOffmapEscorts;
            offmapManpowerTextBox.Tag = ScenarioEditorItemId.CountryOffmapManpower;
            offmapIcTextBox.Tag = ScenarioEditorItemId.CountryOffmapIc;

            // National resource label
            countryEnergyLabel.Text = Config.GetText(TextId.ResourceEnergy);
            countryMetalLabel.Text = Config.GetText(TextId.ResourceMetal);
            countryRareMaterialsLabel.Text = Config.GetText(TextId.ResourceRareMaterials);
            countryOilLabel.Text = Config.GetText(TextId.ResourceOil);
            countrySuppliesLabel.Text = Config.GetText(TextId.ResourceSupplies);
            countryMoneyLabel.Text = Config.GetText(TextId.ResourceMoney);
            countryTransportsLabel.Text = Config.GetText(TextId.ResourceTransports);
            countryEscortsLabel.Text = Config.GetText(TextId.ResourceEscorts);
            countryManpowerLabel.Text = Config.GetText(TextId.ResourceManpower);
            countryIcLabel.Text = Config.GetText(TextId.ResourceIc);
        }

        /// <summary>
        ///     Update the edit items of national resource information
        /// </summary>
        /// <param name="settings">National setting</param>
        private void UpdateCountryResourceItems(CountrySettings settings)
        {
            // Update the display of edit items
            _controller.UpdateItemValue(countryEnergyTextBox, settings);
            _controller.UpdateItemValue(countryMetalTextBox, settings);
            _controller.UpdateItemValue(countryRareMaterialsTextBox, settings);
            _controller.UpdateItemValue(countryOilTextBox, settings);
            _controller.UpdateItemValue(countrySuppliesTextBox, settings);
            _controller.UpdateItemValue(countryMoneyTextBox, settings);
            _controller.UpdateItemValue(countryTransportsTextBox, settings);
            _controller.UpdateItemValue(countryEscortsTextBox, settings);
            _controller.UpdateItemValue(countryManpowerTextBox, settings);
            _controller.UpdateItemValue(offmapEnergyTextBox, settings);
            _controller.UpdateItemValue(offmapMetalTextBox, settings);
            _controller.UpdateItemValue(offmapRareMaterialsTextBox, settings);
            _controller.UpdateItemValue(offmapOilTextBox, settings);
            _controller.UpdateItemValue(offmapSuppliesTextBox, settings);
            _controller.UpdateItemValue(offmapMoneyTextBox, settings);
            _controller.UpdateItemValue(offmapTransportsTextBox, settings);
            _controller.UpdateItemValue(offmapEscortsTextBox, settings);
            _controller.UpdateItemValue(offmapManpowerTextBox, settings);
            _controller.UpdateItemValue(offmapIcTextBox, settings);

            // Update the color of the edit item
            _controller.UpdateItemColor(countryEnergyTextBox, settings);
            _controller.UpdateItemColor(countryMetalTextBox, settings);
            _controller.UpdateItemColor(countryRareMaterialsTextBox, settings);
            _controller.UpdateItemColor(countryOilTextBox, settings);
            _controller.UpdateItemColor(countrySuppliesTextBox, settings);
            _controller.UpdateItemColor(countryMoneyTextBox, settings);
            _controller.UpdateItemColor(countryTransportsTextBox, settings);
            _controller.UpdateItemColor(countryEscortsTextBox, settings);
            _controller.UpdateItemColor(countryManpowerTextBox, settings);
            _controller.UpdateItemColor(offmapEnergyTextBox, settings);
            _controller.UpdateItemColor(offmapMetalTextBox, settings);
            _controller.UpdateItemColor(offmapRareMaterialsTextBox, settings);
            _controller.UpdateItemColor(offmapOilTextBox, settings);
            _controller.UpdateItemColor(offmapSuppliesTextBox, settings);
            _controller.UpdateItemColor(offmapMoneyTextBox, settings);
            _controller.UpdateItemColor(offmapTransportsTextBox, settings);
            _controller.UpdateItemColor(offmapEscortsTextBox, settings);
            _controller.UpdateItemColor(offmapManpowerTextBox, settings);
            _controller.UpdateItemColor(offmapIcTextBox, settings);
        }

        /// <summary>
        ///     Clear the display of edit items of national resource information
        /// </summary>
        private void ClearCountryResourceItems()
        {
            countryEnergyTextBox.Text = "";
            countryMetalTextBox.Text = "";
            countryRareMaterialsTextBox.Text = "";
            countryOilTextBox.Text = "";
            countrySuppliesTextBox.Text = "";
            countryMoneyTextBox.Text = "";
            countryTransportsTextBox.Text = "";
            countryEscortsTextBox.Text = "";
            countryManpowerTextBox.Text = "";
            offmapEnergyTextBox.Text = "";
            offmapMetalTextBox.Text = "";
            offmapRareMaterialsTextBox.Text = "";
            offmapOilTextBox.Text = "";
            offmapSuppliesTextBox.Text = "";
            offmapMoneyTextBox.Text = "";
            offmapTransportsTextBox.Text = "";
            offmapEscortsTextBox.Text = "";
            offmapManpowerTextBox.Text = "";
            offmapIcTextBox.Text = "";
        }

        /// <summary>
        ///     Enable editing items for national resource information
        /// </summary>
        private void EnableCountryResourceItems()
        {
            countryResourceGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for national resource information
        /// </summary>
        private void DisableCountryResourceItems()
        {
            countryResourceGroupBox.Enabled = false;
        }

        #endregion

        #region National tab --AI information

        /// <summary>
        ///     Nation AI Initialize information edit items
        /// </summary>
        private void InitCountryAiItems()
        {
            _itemControls.Add(ScenarioEditorItemId.CountryAiFileName, aiFileNameTextBox);

            aiFileNameTextBox.Tag = ScenarioEditorItemId.CountryAiFileName;
        }

        /// <summary>
        ///     Nation AI Update information edit items
        /// </summary>
        /// <param name="settings">National setting</param>
        private void UpdateCountryAiItems(CountrySettings settings)
        {
            // Update the display of edit items
            _controller.UpdateItemValue(aiFileNameTextBox, settings);

            // Update the color of the edit item
            _controller.UpdateItemColor(aiFileNameTextBox, settings);
        }

        /// <summary>
        ///     Nation AI Clear the display of information edit items
        /// </summary>
        private void ClearCountryAiItems()
        {
            aiFileNameTextBox.Text = "";
        }

        /// <summary>
        ///     AIProcessing when the file name reference button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAiFileNameBrowseButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Country country = GetSelectedCountry();
            if (country == Country.None)
            {
                return;
            }

            CountrySettings settings = Scenarios.GetCountrySettings(country);

            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = Game.GetReadFileName(Game.AiPathName),
                FileName = settings.AiFileName,
                Filter = Resources.OpenAiFileDialogFilter
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string fileName;
                if (Game.IsExportFolderActive)
                {
                    fileName = PathHelper.GetRelativePathName(dialog.FileName, Game.ExportFolderName);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        aiFileNameTextBox.Text = fileName;
                        return;
                    }
                }
                if (Game.IsModActive)
                {
                    fileName = PathHelper.GetRelativePathName(dialog.FileName, Game.ModFolderName);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        aiFileNameTextBox.Text = fileName;
                        return;
                    }
                }
                fileName = PathHelper.GetRelativePathName(dialog.FileName, Game.FolderName);
                if (!string.IsNullOrEmpty(fileName))
                {
                    aiFileNameTextBox.Text = fileName;
                    return;
                }
                aiFileNameTextBox.Text = dialog.FileName;
            }
        }

        /// <summary>
        ///     Nation AI Enable information editing items
        /// </summary>
        private void EnableCountryAiItems()
        {
            aiGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Nation AI Disable information edit items
        /// </summary>
        private void DisableCountryAiItems()
        {
            aiGroupBox.Enabled = false;
        }

        #endregion

        #region National tab ―――― Edit items

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selected nation
            Country country = GetSelectedCountry();
            if (country == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, settings);
                return;
            }

            // Do nothing if it has not changed from the initial value
            if ((settings == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val, settings))
            {
                _controller.UpdateItemValue(control, settings);
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, settings);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryDoubleItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selected nation
            Country country = GetSelectedCountry();
            if (country == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // Returns the value if the string cannot be converted to a number
            double val;
            if (!DoubleHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, settings);
                return;
            }

            // Do nothing if it has not changed from the initial value
            if ((settings == null) && DoubleHelper.IsZero(val))
            {
                return;
            }

            // Do nothing if the value does not change
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev != null) && DoubleHelper.IsEqual(val, (double) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val, settings))
            {
                _controller.UpdateItemValue(control, settings);
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, settings);
        }

        /// <summary>
        ///     Processing when changing the value of a text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryStringItemTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selected nation
            Country country = GetSelectedCountry();
            if (country == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, country, settings);
            string val = control.Text;
            if ((prev == null) && string.IsNullOrEmpty(val))
            {
                return;
            }

            // Do nothing if the value does not change
            if (val.Equals(_controller.GetItemValue(itemId, settings)))
            {
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, settings);
        }

        /// <summary>
        ///     Processing when changing the value of a text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryNameItemTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selected nation
            Country country = GetSelectedCountry();
            if (country == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, country, settings);
            string val = control.Text;
            if ((prev == null) && string.IsNullOrEmpty(val))
            {
                return;
            }

            // Do nothing if the value does not change
            if (val.Equals(_controller.GetItemValue(itemId, country, settings)))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, settings);
        }

        /// <summary>
        ///     Item drawing process of combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryItemComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index < 0)
            {
                return;
            }

            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            if (e.Index > 0)
            {
                Country country = GetSelectedCountry();
                CountrySettings settings = Scenarios.GetCountrySettings(country);
                ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;
                object val = _controller.GetItemValue(itemId, settings);
                object sel = _controller.GetListItemValue(itemId, e.Index);
                Brush brush = (val != null) && ((Country) val == (Country) sel) &&
                              _controller.IsItemDirty(itemId, settings)
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(SystemColors.WindowText);
                string s = control.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryItemComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Country country = GetSelectedCountry();
            if (country == Country.None)
            {
                return;
            }

            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // Do nothing if it has not changed from the initial value
            if ((settings == null) && (control.SelectedIndex == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            Country val = control.SelectedIndex > 0 ? Countries.Tags[control.SelectedIndex - 1] : Country.None;
            if ((settings != null) && (val == (Country) _controller.GetItemValue(itemId, settings)))
            {
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);

            // Update drawing to change the item color of the combo box
            control.Refresh();
        }

        #endregion

        #endregion

        #region Government tab

        #region Government tab ―――― common

        /// <summary>
        ///     Initialize the government tab
        /// </summary>
        private void InitGovernmentTab()
        {
            InitPoliticalSliderItems();
            InitCabinetItems();
        }

        /// <summary>
        ///     Update the display of the government tab
        /// </summary>
        private void UpdateGovernmentTab()
        {
            // Do nothing if initialized
            if (_tabPageInitialized[(int) TabPageNo.Government])
            {
                return;
            }

            // Update national list box
            UpdateCountryListBox(governmentCountryListBox);

            // Enable national list box
            EnableGovernmentCountryListBox();

            // Disable edit items
            DisablePoliticalSliderItems();
            DisableCabinetItems();

            // Clear edit items
            ClearPoliticalSliderItems();
            ClearCabinetItems();

            // Set the initialized flag
            _tabPageInitialized[(int) TabPageNo.Government] = true;
        }

        /// <summary>
        ///     Processing when loading a form on the government tab
        /// </summary>
        private void OnGovernmentTabPageFormLoad()
        {
            // Initialize the government tab
            InitGovernmentTab();
        }

        /// <summary>
        ///     Processing when reading a file on the Government tab
        /// </summary>
        private void OnGovernmentTabPageFileLoad()
        {
            // Do nothing unless the government tab is selected
            if (_tabPageNo != TabPageNo.Government)
            {
                return;
            }

            // Wait until the reading of ministerial data is completed
            Ministers.WaitLoading();

            // Update the display at the first transition
            UpdateGovernmentTab();
        }

        /// <summary>
        ///     Processing when selecting the government tab
        /// </summary>
        private void OnGovernmentTabPageSelected()
        {
            // Do nothing if scenario not loaded
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // Wait until the reading of ministerial data is completed
            Ministers.WaitLoading();

            // Update the display at the first transition
            UpdateGovernmentTab();
        }

        #endregion

        #region Government tab ―――― Nation

        /// <summary>
        ///     Activate the national list box
        /// </summary>
        private void EnableGovernmentCountryListBox()
        {
            governmentCountryListBox.Enabled = true;
        }

        /// <summary>
        ///     Processing when changing the selection item of the national list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGovernmentCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable edit items if there are no selections
            if (governmentCountryListBox.SelectedIndex < 0)
            {
                // Disable edit items
                DisablePoliticalSliderItems();
                DisableCabinetItems();

                // Clear edit items
                ClearPoliticalSliderItems();
                ClearCabinetItems();
                return;
            }

            Country country = GetSelectedGovernmentCountry();
            CountrySettings settings = Scenarios.GetCountrySettings(country);
            ScenarioHeader header = Scenarios.Data.Header;
            int year = header.StartDate?.Year ?? header.StartYear;

            // Update edit items
            UpdatePoliticalSliderItems(settings);
            UpdateCabinetItems(country, settings, year);

            // Enable edit items
            EnablePoliticalSliderItems();
            EnableCabinetItems();
        }

        /// <summary>
        ///     Get the selected nation
        /// </summary>
        /// <returns>Selected nation</returns>
        private Country GetSelectedGovernmentCountry()
        {
            if (governmentCountryListBox.SelectedIndex < 0)
            {
                return Country.None;
            }
            return Countries.Tags[governmentCountryListBox.SelectedIndex];
        }

        #endregion

        #region Government tab ―――― Policy slider

        /// <summary>
        ///     Initialize the edit items of the policy slider
        /// </summary>
        private void InitPoliticalSliderItems()
        {
            _itemControls.Add(ScenarioEditorItemId.SliderYear, sliderYearTextBox);
            _itemControls.Add(ScenarioEditorItemId.SliderMonth, sliderMonthTextBox);
            _itemControls.Add(ScenarioEditorItemId.SliderDay, sliderDayTextBox);

            _itemControls.Add(ScenarioEditorItemId.SliderDemocratic, democraticTrackBar);
            _itemControls.Add(ScenarioEditorItemId.SliderPoliticalLeft, politicalLeftTrackBar);
            _itemControls.Add(ScenarioEditorItemId.SliderFreedom, freedomTrackBar);
            _itemControls.Add(ScenarioEditorItemId.SliderFreeMarket, freeMarketTrackBar);
            _itemControls.Add(ScenarioEditorItemId.SliderProfessionalArmy, professionalArmyTrackBar);
            _itemControls.Add(ScenarioEditorItemId.SliderDefenseLobby, defenseLobbyTrackBar);
            _itemControls.Add(ScenarioEditorItemId.SliderInterventionism, interventionismTrackBar);

            sliderYearTextBox.Tag = ScenarioEditorItemId.SliderYear;
            sliderMonthTextBox.Tag = ScenarioEditorItemId.SliderMonth;
            sliderDayTextBox.Tag = ScenarioEditorItemId.SliderDay;

            democraticTrackBar.Tag = ScenarioEditorItemId.SliderDemocratic;
            politicalLeftTrackBar.Tag = ScenarioEditorItemId.SliderPoliticalLeft;
            freedomTrackBar.Tag = ScenarioEditorItemId.SliderFreedom;
            freeMarketTrackBar.Tag = ScenarioEditorItemId.SliderFreeMarket;
            professionalArmyTrackBar.Tag = ScenarioEditorItemId.SliderProfessionalArmy;
            defenseLobbyTrackBar.Tag = ScenarioEditorItemId.SliderDefenseLobby;
            interventionismTrackBar.Tag = ScenarioEditorItemId.SliderInterventionism;

            democraticLabel.Text = Config.GetText(TextId.SliderDemocratic);
            authoritarianLabel.Text = Config.GetText(TextId.SliderAuthoritarian);
            politicalLeftLabel.Text = Config.GetText(TextId.SliderPoliticalLeft);
            politicalRightLabel.Text = Config.GetText(TextId.SliderPoliticalRight);
            openSocietyLabel.Text = Config.GetText(TextId.SliderOpenSociety);
            closedSocietyLabel.Text = Config.GetText(TextId.SliderClosedSociety);
            freeMarketLabel.Text = Config.GetText(TextId.SliderFreeMarket);
            centralPlanningLabel.Text = Config.GetText(TextId.SliderCentralPlanning);
            standingArmyLabel.Text = Config.GetText(TextId.SliderStandingArmy);
            draftedArmyLabel.Text = Config.GetText(TextId.SliderDraftedArmy);
            hawkLobbyLabel.Text = Config.GetText(TextId.SliderHawkLobby);
            doveLobbyLabel.Text = Config.GetText(TextId.SliderDoveLobby);
            interventionismLabel.Text = Config.GetText(TextId.SliderInterventionism);
            isolationismLabel.Text = Config.GetText(TextId.SlidlaIsolationism);

            authoritarianLabel.Left = democraticTrackBar.Left + democraticTrackBar.Width - authoritarianLabel.Width;
            politicalRightLabel.Left = politicalLeftTrackBar.Left + politicalLeftTrackBar.Width -
                                       politicalRightLabel.Width;
            closedSocietyLabel.Left = freedomTrackBar.Left + freedomTrackBar.Width - closedSocietyLabel.Width;
            centralPlanningLabel.Left = freeMarketTrackBar.Left + freeMarketTrackBar.Width - centralPlanningLabel.Width;
            draftedArmyLabel.Left = professionalArmyTrackBar.Left + professionalArmyTrackBar.Width -
                                    draftedArmyLabel.Width;
            doveLobbyLabel.Left = defenseLobbyTrackBar.Left + defenseLobbyTrackBar.Width - doveLobbyLabel.Width;
            isolationismLabel.Left = interventionismTrackBar.Left + interventionismTrackBar.Width -
                                     isolationismLabel.Width;
        }

        /// <summary>
        ///     Update policy slider edits
        /// </summary>
        /// <param name="settings">National setting</param>
        private void UpdatePoliticalSliderItems(CountrySettings settings)
        {
            _controller.UpdateItemValue(sliderYearTextBox, settings);
            _controller.UpdateItemValue(sliderMonthTextBox, settings);
            _controller.UpdateItemValue(sliderDayTextBox, settings);

            _controller.UpdateItemColor(sliderYearTextBox, settings);
            _controller.UpdateItemColor(sliderMonthTextBox, settings);
            _controller.UpdateItemColor(sliderDayTextBox, settings);

            _controller.UpdateItemValue(democraticTrackBar, settings);
            _controller.UpdateItemValue(politicalLeftTrackBar, settings);
            _controller.UpdateItemValue(freedomTrackBar, settings);
            _controller.UpdateItemValue(freeMarketTrackBar, settings);
            _controller.UpdateItemValue(professionalArmyTrackBar, settings);
            _controller.UpdateItemValue(defenseLobbyTrackBar, settings);
            _controller.UpdateItemValue(interventionismTrackBar, settings);
        }

        /// <summary>
        ///     Clear the display of the edit item of the policy slider
        /// </summary>
        private void ClearPoliticalSliderItems()
        {
            sliderYearTextBox.Text = "";
            sliderMonthTextBox.Text = "";
            sliderDayTextBox.Text = "";

            democraticTrackBar.Value = 6;
            politicalLeftTrackBar.Value = 6;
            freedomTrackBar.Value = 6;
            freeMarketTrackBar.Value = 6;
            professionalArmyTrackBar.Value = 6;
            defenseLobbyTrackBar.Value = 6;
            interventionismTrackBar.Value = 6;
        }

        /// <summary>
        ///     Enable edits in the policy slider
        /// </summary>
        private void EnablePoliticalSliderItems()
        {
            politicalSliderGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items on the policy slider
        /// </summary>
        private void DisablePoliticalSliderItems()
        {
            politicalSliderGroupBox.Enabled = false;
        }

        #endregion

        #region Government tab ―――― Minister

        /// <summary>
        ///     Initialize ministerial edits
        /// </summary>
        private void InitCabinetItems()
        {
            _itemControls.Add(ScenarioEditorItemId.CabinetHeadOfState, headOfStateComboBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetHeadOfStateType, headOfStateTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetHeadOfStateId, headOfStateIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetHeadOfGovernment, headOfGovernmentComboBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetHeadOfGovernmentType, headOfGovernmentTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetHeadOfGovernmentId, headOfGovernmentIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetForeignMinister, foreignMinisterComboBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetForeignMinisterType, foreignMinisterTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetForeignMinisterId, foreignMinisterIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetArmamentMinister, armamentMinisterComboBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetArmamentMinisterType, armamentMinisterTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetArmamentMinisterId, armamentMinisterIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetMinisterOfSecurity, ministerOfSecurityComboBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetMinisterOfSecurityType, ministerOfSecurityTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetMinisterOfSecurityId, ministerOfSecurityIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetMinisterOfIntelligence, ministerOfIntelligenceComboBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetMinisterOfIntelligenceType, ministerOfIntelligenceTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetMinisterOfIntelligenceId, ministerOfIntelligenceIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfStaff, chiefOfStaffComboBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfStaffType, chiefOfStaffTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfStaffId, chiefOfStaffIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfArmy, chiefOfArmyComboBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfArmyType, chiefOfArmyTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfArmyId, chiefOfArmyIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfNavy, chiefOfNavyComboBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfNavyType, chiefOfNavyTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfNavyId, chiefOfNavyIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfAir, chiefOfAirComboBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfAirType, chiefOfAirTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.CabinetChiefOfAirId, chiefOfAirIdTextBox);

            headOfStateComboBox.Tag = ScenarioEditorItemId.CabinetHeadOfState;
            headOfStateTypeTextBox.Tag = ScenarioEditorItemId.CabinetHeadOfStateType;
            headOfStateIdTextBox.Tag = ScenarioEditorItemId.CabinetHeadOfStateId;
            headOfGovernmentComboBox.Tag = ScenarioEditorItemId.CabinetHeadOfGovernment;
            headOfGovernmentTypeTextBox.Tag = ScenarioEditorItemId.CabinetHeadOfGovernmentType;
            headOfGovernmentIdTextBox.Tag = ScenarioEditorItemId.CabinetHeadOfGovernmentId;
            foreignMinisterComboBox.Tag = ScenarioEditorItemId.CabinetForeignMinister;
            foreignMinisterTypeTextBox.Tag = ScenarioEditorItemId.CabinetForeignMinisterType;
            foreignMinisterIdTextBox.Tag = ScenarioEditorItemId.CabinetForeignMinisterId;
            armamentMinisterComboBox.Tag = ScenarioEditorItemId.CabinetArmamentMinister;
            armamentMinisterTypeTextBox.Tag = ScenarioEditorItemId.CabinetArmamentMinisterType;
            armamentMinisterIdTextBox.Tag = ScenarioEditorItemId.CabinetArmamentMinisterId;
            ministerOfSecurityComboBox.Tag = ScenarioEditorItemId.CabinetMinisterOfSecurity;
            ministerOfSecurityTypeTextBox.Tag = ScenarioEditorItemId.CabinetMinisterOfSecurityType;
            ministerOfSecurityIdTextBox.Tag = ScenarioEditorItemId.CabinetMinisterOfSecurityId;
            ministerOfIntelligenceComboBox.Tag = ScenarioEditorItemId.CabinetMinisterOfIntelligence;
            ministerOfIntelligenceTypeTextBox.Tag = ScenarioEditorItemId.CabinetMinisterOfIntelligenceType;
            ministerOfIntelligenceIdTextBox.Tag = ScenarioEditorItemId.CabinetMinisterOfIntelligenceId;
            chiefOfStaffComboBox.Tag = ScenarioEditorItemId.CabinetChiefOfStaff;
            chiefOfStaffTypeTextBox.Tag = ScenarioEditorItemId.CabinetChiefOfStaffType;
            chiefOfStaffIdTextBox.Tag = ScenarioEditorItemId.CabinetChiefOfStaffId;
            chiefOfArmyComboBox.Tag = ScenarioEditorItemId.CabinetChiefOfArmy;
            chiefOfArmyTypeTextBox.Tag = ScenarioEditorItemId.CabinetChiefOfArmyType;
            chiefOfArmyIdTextBox.Tag = ScenarioEditorItemId.CabinetChiefOfArmyId;
            chiefOfNavyComboBox.Tag = ScenarioEditorItemId.CabinetChiefOfNavy;
            chiefOfNavyTypeTextBox.Tag = ScenarioEditorItemId.CabinetChiefOfNavyType;
            chiefOfNavyIdTextBox.Tag = ScenarioEditorItemId.CabinetChiefOfNavyId;
            chiefOfAirComboBox.Tag = ScenarioEditorItemId.CabinetChiefOfAir;
            chiefOfAirTypeTextBox.Tag = ScenarioEditorItemId.CabinetChiefOfAirType;
            chiefOfAirIdTextBox.Tag = ScenarioEditorItemId.CabinetChiefOfAirId;

            headOfStateLabel.Text = Config.GetText(TextId.MinisterHeadOfState);
            headOfGovernmentLabel.Text = Config.GetText(TextId.MinisterHeadOfGovernment);
            foreignMinisterlabel.Text = Config.GetText(TextId.MinisterForeignMinister);
            armamentMinisterLabel.Text = Config.GetText(TextId.MinisterArmamentMinister);
            ministerOfSecurityLabel.Text = Config.GetText(TextId.MinisterMinisterOfSecurity);
            ministerOfIntelligenceLabel.Text = Config.GetText(TextId.MinisterMinisterOfIntelligence);
            chiefOfStaffLabel.Text = Config.GetText(TextId.MinisterChiefOfStaff);
            chiefOfArmyLabel.Text = Config.GetText(TextId.MinisterChiefOfArmy);
            chiefOfNavyLabel.Text = Config.GetText(TextId.MinisterChiefOfNavy);
            chiefOfAirLabel.Text = Config.GetText(TextId.MinisterChiefOfAir);
        }

        /// <summary>
        ///     Update ministerial edits
        /// </summary>
        /// <param name="country">Selected country</param>
        /// <param name="settings">National setting</param>
        /// <param name="year">Target year</param>
        private void UpdateCabinetItems(Country country, CountrySettings settings, int year)
        {
            // Update the list of ministerial candidates
            _controller.UpdateMinisterList(country, year);

            // Update the display of the ministerial combo box
            _controller.UpdateItemValue(headOfStateComboBox, settings);
            _controller.UpdateItemValue(headOfGovernmentComboBox, settings);
            _controller.UpdateItemValue(foreignMinisterComboBox, settings);
            _controller.UpdateItemValue(armamentMinisterComboBox, settings);
            _controller.UpdateItemValue(ministerOfSecurityComboBox, settings);
            _controller.UpdateItemValue(ministerOfIntelligenceComboBox, settings);
            _controller.UpdateItemValue(chiefOfStaffComboBox, settings);
            _controller.UpdateItemValue(chiefOfArmyComboBox, settings);
            _controller.UpdateItemValue(chiefOfNavyComboBox, settings);
            _controller.UpdateItemValue(chiefOfAirComboBox, settings);

            // Minister type / id Update the display of the text box
            _controller.UpdateItemValue(headOfStateTypeTextBox, settings);
            _controller.UpdateItemValue(headOfStateIdTextBox, settings);
            _controller.UpdateItemValue(headOfGovernmentTypeTextBox, settings);
            _controller.UpdateItemValue(headOfGovernmentIdTextBox, settings);
            _controller.UpdateItemValue(foreignMinisterTypeTextBox, settings);
            _controller.UpdateItemValue(foreignMinisterIdTextBox, settings);
            _controller.UpdateItemValue(armamentMinisterTypeTextBox, settings);
            _controller.UpdateItemValue(armamentMinisterIdTextBox, settings);
            _controller.UpdateItemValue(ministerOfSecurityTypeTextBox, settings);
            _controller.UpdateItemValue(ministerOfSecurityIdTextBox, settings);
            _controller.UpdateItemValue(ministerOfIntelligenceTypeTextBox, settings);
            _controller.UpdateItemValue(ministerOfIntelligenceIdTextBox, settings);
            _controller.UpdateItemValue(chiefOfStaffTypeTextBox, settings);
            _controller.UpdateItemValue(chiefOfStaffIdTextBox, settings);
            _controller.UpdateItemValue(chiefOfArmyTypeTextBox, settings);
            _controller.UpdateItemValue(chiefOfArmyIdTextBox, settings);
            _controller.UpdateItemValue(chiefOfNavyTypeTextBox, settings);
            _controller.UpdateItemValue(chiefOfNavyIdTextBox, settings);
            _controller.UpdateItemValue(chiefOfAirTypeTextBox, settings);
            _controller.UpdateItemValue(chiefOfAirIdTextBox, settings);

            // Minister type / id Update text box color
            _controller.UpdateItemColor(headOfStateTypeTextBox, settings);
            _controller.UpdateItemColor(headOfStateIdTextBox, settings);
            _controller.UpdateItemColor(headOfGovernmentTypeTextBox, settings);
            _controller.UpdateItemColor(headOfGovernmentIdTextBox, settings);
            _controller.UpdateItemColor(foreignMinisterTypeTextBox, settings);
            _controller.UpdateItemColor(foreignMinisterIdTextBox, settings);
            _controller.UpdateItemColor(armamentMinisterTypeTextBox, settings);
            _controller.UpdateItemColor(armamentMinisterIdTextBox, settings);
            _controller.UpdateItemColor(ministerOfSecurityTypeTextBox, settings);
            _controller.UpdateItemColor(ministerOfSecurityIdTextBox, settings);
            _controller.UpdateItemColor(ministerOfIntelligenceTypeTextBox, settings);
            _controller.UpdateItemColor(ministerOfIntelligenceIdTextBox, settings);
            _controller.UpdateItemColor(chiefOfStaffTypeTextBox, settings);
            _controller.UpdateItemColor(chiefOfStaffIdTextBox, settings);
            _controller.UpdateItemColor(chiefOfArmyTypeTextBox, settings);
            _controller.UpdateItemColor(chiefOfArmyIdTextBox, settings);
            _controller.UpdateItemColor(chiefOfNavyTypeTextBox, settings);
            _controller.UpdateItemColor(chiefOfNavyIdTextBox, settings);
            _controller.UpdateItemColor(chiefOfAirTypeTextBox, settings);
            _controller.UpdateItemColor(chiefOfAirIdTextBox, settings);
        }

        /// <summary>
        ///     Clear the display of ministerial edit items
        /// </summary>
        private void ClearCabinetItems()
        {
            headOfStateComboBox.SelectedIndex = -1;
            headOfGovernmentComboBox.SelectedIndex = -1;
            foreignMinisterComboBox.SelectedIndex = -1;
            armamentMinisterComboBox.SelectedIndex = -1;
            ministerOfSecurityComboBox.SelectedIndex = -1;
            ministerOfIntelligenceComboBox.SelectedIndex = -1;
            chiefOfStaffComboBox.SelectedIndex = -1;
            chiefOfArmyComboBox.SelectedIndex = -1;
            chiefOfNavyComboBox.SelectedIndex = -1;
            chiefOfAirComboBox.SelectedIndex = -1;

            headOfStateTypeTextBox.Text = "";
            headOfStateIdTextBox.Text = "";
            headOfGovernmentTypeTextBox.Text = "";
            headOfGovernmentIdTextBox.Text = "";
            foreignMinisterTypeTextBox.Text = "";
            foreignMinisterIdTextBox.Text = "";
            armamentMinisterTypeTextBox.Text = "";
            armamentMinisterIdTextBox.Text = "";
            ministerOfSecurityTypeTextBox.Text = "";
            ministerOfSecurityIdTextBox.Text = "";
            ministerOfIntelligenceTypeTextBox.Text = "";
            ministerOfIntelligenceIdTextBox.Text = "";
            chiefOfStaffTypeTextBox.Text = "";
            chiefOfStaffIdTextBox.Text = "";
            chiefOfArmyTypeTextBox.Text = "";
            chiefOfArmyIdTextBox.Text = "";
            chiefOfNavyTypeTextBox.Text = "";
            chiefOfNavyIdTextBox.Text = "";
            chiefOfAirTypeTextBox.Text = "";
            chiefOfAirIdTextBox.Text = "";
        }

        /// <summary>
        ///     Enable ministerial edits
        /// </summary>
        private void EnableCabinetItems()
        {
            cabinetGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable ministerial edits
        /// </summary>
        private void DisableCabinetItems()
        {
            cabinetGroupBox.Enabled = false;
        }

        #endregion

        #region Government tab ―――― Edit items

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGovernmentIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selected nation
            Country country = GetSelectedGovernmentCountry();
            if (country == Country.None)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, settings);
                return;
            }

            // Do nothing if it has not changed from the initial value
            if ((settings == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            // Returns a value if it is invalid
            if (!_controller.IsItemValueValid(itemId, val, settings))
            {
                _controller.UpdateItemValue(control, settings);
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, settings);
        }

        /// <summary>
        ///     Policy slider Process when changing the value of the slide bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPoliticalSliderTrackBarScroll(object sender, EventArgs e)
        {
            // Do nothing if there is no selected nation
            Country country = GetSelectedGovernmentCountry();
            if (country == Country.None)
            {
                return;
            }

            CountrySettings settings = Scenarios.GetCountrySettings(country);

            TrackBar control = (TrackBar) sender;
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Do nothing if it has not changed from the initial value
            int val = 11 - control.Value;
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev == null) && (val == 5))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);
        }

        /// <summary>
        ///     Item drawing process of minister combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCabinetComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Country country = GetSelectedGovernmentCountry();
            CountrySettings settings = Scenarios.GetCountrySettings(country);
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;
            object val = _controller.GetItemValue(itemId, settings);
            object sel = _controller.GetListItemValue(itemId, e.Index);
            Brush brush = (val != null) && (sel != null) && ((int) val == (int) sel) &&
                          _controller.IsItemDirty(itemId, settings)
                ? new SolidBrush(Color.Red)
                : new SolidBrush(SystemColors.WindowText);
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the ministerial combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCabinetComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            ComboBox control = (ComboBox) sender;
            int index = control.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            // Do nothing if there is no selected nation
            Country country = GetSelectedGovernmentCountry();
            if (country == Country.None)
            {
                return;
            }
            CountrySettings settings = Scenarios.GetCountrySettings(country);

            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            // Do nothing if it has not changed from the initial value
            object val = _controller.GetListItemValue(itemId, index);
            if (val == null)
            {
                return;
            }

            // Do nothing if the value does not change
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev != null) && ((int) val == (int) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, settings);
        }

        #endregion

        #endregion

        #region Tech tab

        #region Technology tab ―――― common

        /// <summary>
        ///     Initialize the technology tab
        /// </summary>
        private static void InitTechTab()
        {
            // do nothing
        }

        /// <summary>
        ///     Update the technology tab
        /// </summary>
        private void UpdateTechTab()
        {
            // Do nothing if initialized
            if (_tabPageInitialized[(int) TabPageNo.Technology])
            {
                return;
            }

            // Initialize the technical category list box
            InitTechCategoryListBox();

            // Enable the technology category list box
            EnableTechCategoryListBox();

            // Update national list box
            UpdateCountryListBox(techCountryListBox);

            // Enable national list box
            EnableTechCountryListBox();

            // Disable edit items
            DisableTechItems();

            // Clear edit items
            ClearTechItems();

            // Set the initialized flag
            _tabPageInitialized[(int) TabPageNo.Technology] = true;
        }

        /// <summary>
        ///     Processing when loading a form on the Technology tab
        /// </summary>
        private static void OnTechTabPageFormLoad()
        {
            // Initialize the technology tab
            InitTechTab();
        }

        /// <summary>
        ///     Processing when reading a file on the Technology tab
        /// </summary>
        private void OnTechTabPageFileLoad()
        {
            // Do nothing unless the government tab is selected
            if (_tabPageNo != TabPageNo.Technology)
            {
                return;
            }

            // Wait until the technical data has been read
            Techs.WaitLoading();

            // Update the display at the first transition
            UpdateTechTab();
        }

        /// <summary>
        ///     Processing when technology tab is selected
        /// </summary>
        private void OnTechTabPageSelected()
        {
            // Do nothing if scenario not loaded
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // Wait until the technical data has been read
            Techs.WaitLoading();

            // Update the display at the first transition
            UpdateTechTab();
        }

        #endregion

        #region Technology tab ―――― Technology category

        /// <summary>
        ///     Initialize the technical category list box
        /// </summary>
        private void InitTechCategoryListBox()
        {
            techCategoryListBox.BeginUpdate();
            techCategoryListBox.Items.Clear();
            foreach (TechGroup grp in Techs.Groups)
            {
                techCategoryListBox.Items.Add(grp);
            }
            techCategoryListBox.SelectedIndex = 0;
            techCategoryListBox.EndUpdate();
        }

        /// <summary>
        ///     Enable the technology category list box
        /// </summary>
        private void EnableTechCategoryListBox()
        {
            techCategoryListBox.Enabled = true;
        }

        /// <summary>
        ///     Processing when changing the selection item in the technical category list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechCategoryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable edits if no nation is selected
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                // Disable edit items
                DisableTechItems();

                // Clear edit items
                ClearTechItems();
                return;
            }

            // Update edit items
            UpdateTechItems();

            // Enable edit items
            EnableTechItems();
        }

        /// <summary>
        ///     Get the selected technology group
        /// </summary>
        /// <returns>Selected technology group</returns>
        private TechGroup GetSelectedTechGroup()
        {
            if (techCategoryListBox.SelectedIndex < 0)
            {
                return null;
            }
            return Techs.Groups[techCategoryListBox.SelectedIndex];
        }

        #endregion

        #region Technology tab ―――― Nation

        /// <summary>
        ///     Activate the national list box
        /// </summary>
        private void EnableTechCountryListBox()
        {
            techCountryListBox.Enabled = true;
        }

        /// <summary>
        ///     Processing when changing the selection item of the national list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable edit items if there are no selections
            if (techCountryListBox.SelectedIndex < 0)
            {
                // Disable edit items
                DisableTechItems();

                // Clear edit items
                ClearTechItems();
                return;
            }

            // Update edit items
            UpdateTechItems();

            // Enable edit items
            EnableTechItems();
        }

        /// <summary>
        ///     Get the selected nation
        /// </summary>
        /// <returns>Selected nation</returns>
        private Country GetSelectedTechCountry()
        {
            if (techCountryListBox.SelectedIndex < 0)
            {
                return Country.None;
            }
            return Countries.Tags[techCountryListBox.SelectedIndex];
        }

        #endregion

        #region Technology tab ―――― Edit items

        /// <summary>
        ///     Update technical edits
        /// </summary>
        private void UpdateTechItems()
        {
            Country country = GetSelectedTechCountry();
            CountrySettings settings = Scenarios.GetCountrySettings(country);
            TechGroup grp = GetSelectedTechGroup();

            // List of owned technologies
            _techs = grp.Items.OfType<TechItem>().ToList();
            UpdateOwnedTechList(settings);

            // Blueprint list
            UpdateBlueprintList(settings);

            // Invention event list
            _inventions = Techs.Groups.SelectMany(g => g.Items.OfType<TechEvent>()).ToList();
            UpdateInventionList(settings);

            // Update the tech tree
            _techTreePanelController.Category = grp.Category;
            _techTreePanelController.Update();
        }

        /// <summary>
        ///     Clear the display of technical edit items
        /// </summary>
        private void ClearTechItems()
        {
            // Clear the technology tree
            _techTreePanelController.Clear();

            // Clear edit items
            ownedTechsListView.Items.Clear();
            blueprintsListView.Items.Clear();
            inventionsListView.Items.Clear();
        }

        /// <summary>
        ///     Enable technical edits
        /// </summary>
        private void EnableTechItems()
        {
            ownedTechsLabel.Enabled = true;
            ownedTechsListView.Enabled = true;
            blueprintsLabel.Enabled = true;
            blueprintsListView.Enabled = true;
            inventionsLabel.Enabled = true;
            inventionsListView.Enabled = true;
        }

        /// <summary>
        ///     Disable technical edits
        /// </summary>
        private void DisableTechItems()
        {
            ownedTechsLabel.Enabled = false;
            ownedTechsListView.Enabled = false;
            blueprintsLabel.Enabled = false;
            blueprintsListView.Enabled = false;
            inventionsLabel.Enabled = false;
            inventionsListView.Enabled = false;
        }

        /// <summary>
        ///     Update the display of the possessed technology list
        /// </summary>
        /// <param name="settings">National setting</param>
        private void UpdateOwnedTechList(CountrySettings settings)
        {
            ownedTechsListView.ItemChecked -= OnOwnedTechsListViewItemChecked;
            ownedTechsListView.BeginUpdate();
            ownedTechsListView.Items.Clear();
            if (settings != null)
            {
                foreach (TechItem item in _techs)
                {
                    string name = item.ToString();
                    ownedTechsListView.Items.Add(new ListViewItem
                    {
                        Text = name,
                        Checked = settings.TechApps.Contains(item.Id),
                        ForeColor = settings.IsDirtyOwnedTech(item.Id) ? Color.Red : ownedTechsListView.ForeColor,
                        Tag = item
                    });
                }
            }
            else
            {
                foreach (TechItem item in _techs)
                {
                    string name = item.ToString();
                    ownedTechsListView.Items.Add(new ListViewItem { Text = name, Tag = item });
                }
            }
            ownedTechsListView.EndUpdate();
            ownedTechsListView.ItemChecked += OnOwnedTechsListViewItemChecked;
        }

        /// <summary>
        ///     Update the display of the blue photo list
        /// </summary>
        /// <param name="settings">National setting</param>
        private void UpdateBlueprintList(CountrySettings settings)
        {
            blueprintsListView.ItemChecked -= OnBlueprintsListViewItemChecked;
            blueprintsListView.BeginUpdate();
            blueprintsListView.Items.Clear();
            if (settings != null)
            {
                foreach (TechItem item in _techs)
                {
                    string name = item.ToString();
                    blueprintsListView.Items.Add(new ListViewItem
                    {
                        Text = name,
                        Checked = settings.BluePrints.Contains(item.Id),
                        ForeColor = settings.IsDirtyBlueprint(item.Id) ? Color.Red : ownedTechsListView.ForeColor,
                        Tag = item
                    });
                }
            }
            else
            {
                foreach (TechItem item in _techs)
                {
                    string name = item.ToString();
                    blueprintsListView.Items.Add(new ListViewItem { Text = name, Tag = item });
                }
            }
            blueprintsListView.EndUpdate();
            blueprintsListView.ItemChecked += OnBlueprintsListViewItemChecked;
        }

        /// <summary>
        ///     Update the display of the invention event list
        /// </summary>
        /// <param name="settings">National setting</param>
        private void UpdateInventionList(CountrySettings settings)
        {
            inventionsListView.ItemChecked -= OnInveitionsListViewItemChecked;
            inventionsListView.BeginUpdate();
            inventionsListView.Items.Clear();
            if (settings != null)
            {
                foreach (TechEvent ev in _inventions)
                {
                    inventionsListView.Items.Add(new ListViewItem
                    {
                        Text = ev.ToString(),
                        Checked = settings.Inventions.Contains(ev.Id),
                        ForeColor = settings.IsDirtyInvention(ev.Id) ? Color.Red : inventionsListView.ForeColor,
                        Tag = ev
                    });
                }
            }
            else
            {
                foreach (TechEvent ev in _inventions)
                {
                    inventionsListView.Items.Add(new ListViewItem { Text = ev.ToString(), Tag = ev });
                }
            }
            inventionsListView.EndUpdate();
            inventionsListView.ItemChecked += OnInveitionsListViewItemChecked;
        }

        /// <summary>
        ///     Processing when the check status of the possessed technology list view is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOwnedTechsListViewItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // Do nothing if there is no selected nation
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                return;
            }

            TechItem item = e.Item.Tag as TechItem;
            if (item == null)
            {
                return;
            }
            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // Do nothing if the value does not change
            bool val = e.Item.Checked;
            if ((settings != null) && (val == settings.TechApps.Contains(item.Id)))
            {
                return;
            }

            Log.Info("[Scenario] owned techs: {0}{1} ({2})", val ? '+' : '-', item.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            // Update value
            if (val)
            {
                settings.TechApps.Add(item.Id);
            }
            else
            {
                settings.TechApps.Remove(item.Id);
            }

            // Set the edited flag
            settings.SetDirtyOwnedTech(item.Id);
            Scenarios.SetDirty();

            // Change the font color
            e.Item.ForeColor = Color.Red;

            // Update item labels in the tech tree
            _techTreePanelController.UpdateItem(item);
        }

        /// <summary>
        ///     Process when changing the check status of the blue photo list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBlueprintsListViewItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // Do nothing if there is no selected nation
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                return;
            }

            TechItem item = e.Item.Tag as TechItem;
            if (item == null)
            {
                return;
            }
            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // Do nothing if the value does not change
            bool val = e.Item.Checked;
            if ((settings != null) && (val == settings.BluePrints.Contains(item.Id)))
            {
                return;
            }

            Log.Info("[Scenario] blurprints: {0}{1} ({2})", val ? '+' : '-', item.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            // Update value
            if (val)
            {
                settings.BluePrints.Add(item.Id);
            }
            else
            {
                settings.BluePrints.Remove(item.Id);
            }

            // Set the edited flag
            settings.SetDirtyBlueprint(item.Id);
            Scenarios.SetDirty();

            // Change the font color
            e.Item.ForeColor = Color.Red;

            // Update item labels in the tech tree
            _techTreePanelController.UpdateItem(item);
        }

        /// <summary>
        ///     Processing when the check status of the invention event list view is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInveitionsListViewItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // Do nothing if there is no selected nation
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                return;
            }

            TechEvent ev = e.Item.Tag as TechEvent;
            if (ev == null)
            {
                return;
            }
            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // Do nothing if the value does not change
            bool val = e.Item.Checked;
            if ((settings != null) && (val == settings.Inventions.Contains(ev.Id)))
            {
                return;
            }

            Log.Info("[Scenario] inventions: {0}{1} ({2})", val ? '+' : '-', ev.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            // Update value
            if (val)
            {
                settings.Inventions.Add(ev.Id);
            }
            else
            {
                settings.Inventions.Remove(ev.Id);
            }

            // Set the edited flag
            settings.SetDirtyInvention(ev.Id);
            Scenarios.SetDirty();

            // Change the font color
            e.Item.ForeColor = Color.Red;

            // Update item labels in the tech tree
            _techTreePanelController.UpdateItem(ev);
        }

        #endregion

        #region Tech tab ―――― Technology tree

        /// <summary>
        ///     Processing when clicking the item label mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechTreeItemMouseClick(object sender, TechTreePanelController.ItemMouseEventArgs e)
        {
            // Do nothing if there is no selected nation
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                return;
            }

            TechItem tech = e.Item as TechItem;
            if (tech != null)
            {
                // Left-click to switch the presence or absence of possessed technology
                if (e.Button == MouseButtons.Left)
                {
                    ToggleOwnedTech(tech, country);
                }
                // Right-click to switch between blueprints
                else if (e.Button == MouseButtons.Right)
                {
                    ToggleBlueprint(tech, country);
                }
                return;
            }

            TechEvent ev = e.Item as TechEvent;
            if (ev != null)
            {
                // Left-click to switch the presence or absence of possessed technology
                if (e.Button == MouseButtons.Left)
                {
                    ToggleInvention(ev, country);
                }
            }
        }

        /// <summary>
        ///     Switch the presence or absence of possessed technology
        /// </summary>
        /// <param name="item">Target technology</param>
        /// <param name="country">Target country</param>
        private void ToggleOwnedTech(TechItem item, Country country)
        {
            CountrySettings settings = Scenarios.GetCountrySettings(country);
            bool val = (settings == null) || !settings.TechApps.Contains(item.Id);

            Log.Info("[Scenario] owned techs: {0}{1} ({2})", val ? '+' : '-', item.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            // Update value
            if (val)
            {
                settings.TechApps.Add(item.Id);
            }
            else
            {
                settings.TechApps.Remove(item.Id);
            }

            // Set the edited flag
            settings.SetDirtyOwnedTech(item.Id);
            Scenarios.SetDirty();

            // Update item labels in the tech tree
            _techTreePanelController.UpdateItem(item);

            // Update the display of the owned technology list view
            int index = _techs.IndexOf(item);
            if (index >= 0)
            {
                ListViewItem li = ownedTechsListView.Items[index];
                li.Checked = val;
                li.ForeColor = Color.Red;
                li.EnsureVisible();
            }
        }

        /// <summary>
        ///     Switch the presence or absence of possessed technology
        /// </summary>
        /// <param name="item">Target technology</param>
        /// <param name="country">Target country</param>
        private void ToggleBlueprint(TechItem item, Country country)
        {
            CountrySettings settings = Scenarios.GetCountrySettings(country);
            bool val = (settings == null) || !settings.BluePrints.Contains(item.Id);

            Log.Info("[Scenario] blueprints: {0}{1} ({2})", val ? '+' : '-', item.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            if (val)
            {
                settings.BluePrints.Add(item.Id);
            }
            else
            {
                settings.BluePrints.Remove(item.Id);
            }

            // Set the edited flag
            settings.SetDirtyBlueprint(item.Id);
            Scenarios.SetDirty();

            // Update item labels in the tech tree
            _techTreePanelController.UpdateItem(item);

            // Update the display of the owned technology list view
            int index = _techs.IndexOf(item);
            if (index >= 0)
            {
                ListViewItem li = blueprintsListView.Items[index];
                li.Checked = val;
                li.ForeColor = Color.Red;
                li.EnsureVisible();
            }
        }

        /// <summary>
        ///     Switch the presence or absence of an invention event
        /// </summary>
        /// <param name="item">Target invention event</param>
        /// <param name="country">Target country</param>
        private void ToggleInvention(TechEvent item, Country country)
        {
            CountrySettings settings = Scenarios.GetCountrySettings(country);
            bool val = (settings == null) || !settings.Inventions.Contains(item.Id);

            Log.Info("[Scenario] inventions: {0}{1} ({2})", val ? '+' : '-', item.Id, Countries.Strings[(int) country]);

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            if (val)
            {
                settings.Inventions.Add(item.Id);
            }
            else
            {
                settings.Inventions.Remove(item.Id);
            }

            // Set the edited flag
            settings.SetDirtyInvention(item.Id);
            Scenarios.SetDirty();

            // Update item labels in the tech tree
            _techTreePanelController.UpdateItem(item);

            // Update the display of the owned technology list view
            int index = _inventions.IndexOf(item);
            if (index >= 0)
            {
                ListViewItem li = inventionsListView.Items[index];
                li.Checked = val;
                li.ForeColor = Color.Red;
                li.EnsureVisible();
            }
        }

        /// <summary>
        ///     Returns the status of a technical item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnQueryTechTreeItemStatus(object sender, TechTreePanelController.QueryItemStatusEventArgs e)
        {
            // Do nothing if there is no selected nation
            Country country = GetSelectedTechCountry();
            if (country == Country.None)
            {
                return;
            }

            CountrySettings settings = Scenarios.GetCountrySettings(country);
            if (settings == null)
            {
                return;
            }

            TechItem tech = e.Item as TechItem;
            if (tech != null)
            {
                e.Done = settings.TechApps.Contains(tech.Id);
                e.Blueprint = settings.BluePrints.Contains(tech.Id);
                return;
            }

            TechEvent ev = e.Item as TechEvent;
            if (ev != null)
            {
                e.Done = settings.Inventions.Contains(ev.Id);
            }
        }

        #endregion

        #endregion

        #region Providence stub

        #region Providence stub ―――― common

        /// <summary>
        ///     Initialize the province stub
        /// </summary>
        private void InitProvinceTab()
        {
            InitMapFilter();
            InitProvinceIdTextBox();
            InitProvinceCountryItems();
            InitProvinceInfoItems();
            InitProvinceResourceItems();
            InitProvinceBuildingItems();
        }

        /// <summary>
        ///     Update the display of the Providence stub
        /// </summary>
        private void UpdateProvinceTab()
        {
            // Do nothing if initialized
            if (_tabPageInitialized[(int) TabPageNo.Province])
            {
                return;
            }

            // Initialize the land province list
            _controller.InitProvinceList();

            // Initialize the provision list
            InitProvinceList();

            // Update national filter
            UpdateProvinceCountryFilter();

            // Activate the provision list
            EnableProvinceList();

            // Enable national filter
            EnableProvinceCountryFilter();

            // ID Enable text box
            EnableProvinceIdTextBox();

            // Disable edit items
            DisableProvinceCountryItems();
            DisableProvinceInfoItems();
            DisableProvinceResourceItems();
            DisableProvinceBuildingItems();

            // Clear the display of edit items
            ClearProvinceCountryItems();
            ClearProvinceInfoItems();
            ClearProvinceResourceItems();
            ClearProvinceBuildingItems();

            // Set the initialized flag
            _tabPageInitialized[(int) TabPageNo.Province] = true;
        }

        /// <summary>
        ///     Processing when loading a form of Providence stub
        /// </summary>
        private void OnProvinceTabPageFormLoad()
        {
            // Initialize the province stub
            InitProvinceTab();
        }

        /// <summary>
        ///     Processing when reading a file of Providence stub
        /// </summary>
        private void OnProvinceTabPageFileLoad()
        {
            // Do nothing unless Providence stub is selected
            if (_tabPageNo != TabPageNo.Province)
            {
                return;
            }

            // Wait until the provision data has been read
            Provinces.WaitLoading();

            // Update the display at the first transition
            UpdateProvinceTab();
        }

        /// <summary>
        ///     Processing when Provins stub is selected
        /// </summary>
        private void OnProvinceTabPageSelected()
        {
            // Do nothing if scenario not loaded
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // Wait until the provision data has been read
            Provinces.WaitLoading();

            // Update the display at the first transition
            UpdateProvinceTab();

            // Update the map panel if loaded and uninitialized
            UpdateMapPanel();
        }

        #endregion

        #region Providence stub ―――― map

        /// <summary>
        ///     Update the map panel
        /// </summary>
        private void UpdateMapPanel()
        {
            // Do nothing before loading the map
            if (!Maps.IsLoaded[(int) MapLevel.Level2])
            {
                return;
            }

            // Do nothing if initialized
            if (_mapPanelInitialized)
            {
                return;
            }

            // Set the initialized flag
            _mapPanelInitialized = true;

            // Enable the map panel
            _mapPanelController.ProvinceMouseClick += OnMapPanelMouseClick;
            _mapPanelController.Show();

            // Enable map filter
            EnableMapFilter();

            // Scroll to see the selection provisions
            Province province = GetSelectedProvince();
            if (province != null)
            {
                _mapPanelController.ScrollToProvince(province.Id);
            }
        }

        /// <summary>
        ///     What to do when you click the mouse on the map panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapPanelMouseClick(object sender, MapPanelController.ProvinceEventArgs e)
        {
            // Do nothing except left click
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            // Do nothing unless it is a land provision
            if (_controller.GetLandProvinceIndex(e.Id) < 0)
            {
                return;
            }

            // Selected province ID To update
            provinceIdTextBox.Text = IntHelper.ToString(e.Id);

            // Select Providence
            SelectProvince(e.Id);

            Country country = GetSelectedProvinceCountry();
            switch (_mapPanelController.FilterMode)
            {
                case MapPanelController.MapFilterMode.None:
                    Country target = (from settings in Scenarios.Data.Countries
                        where settings.ControlledProvinces.Contains(e.Id)
                        select settings.Country).FirstOrDefault();
                    provinceCountryFilterComboBox.SelectedIndex = Array.IndexOf(Countries.Tags, target) + 1;
                    break;

                case MapPanelController.MapFilterMode.Core:
                    if (country != Country.None)
                    {
                        coreProvinceCheckBox.Checked = !coreProvinceCheckBox.Checked;
                    }
                    break;

                case MapPanelController.MapFilterMode.Owned:
                    if (country != Country.None && ownedProvinceCheckBox.Enabled)
                    {
                        ownedProvinceCheckBox.Checked = !ownedProvinceCheckBox.Checked;
                    }
                    break;

                case MapPanelController.MapFilterMode.Controlled:
                    if (country != Country.None && controlledProvinceCheckBox.Enabled)
                    {
                        controlledProvinceCheckBox.Checked = !controlledProvinceCheckBox.Checked;
                    }
                    break;

                case MapPanelController.MapFilterMode.Claimed:
                    if (country != Country.None)
                    {
                        claimedProvinceCheckBox.Checked = !claimedProvinceCheckBox.Checked;
                    }
                    break;
            }
        }

        #endregion

        #region Province tab ―――― Map filter

        /// <summary>
        ///     Initialize the map filter
        /// </summary>
        private void InitMapFilter()
        {
            mapFilterNoneRadioButton.Tag = MapPanelController.MapFilterMode.None;
            mapFilterCoreRadioButton.Tag = MapPanelController.MapFilterMode.Core;
            mapFilterOwnedRadioButton.Tag = MapPanelController.MapFilterMode.Owned;
            mapFilterControlledRadioButton.Tag = MapPanelController.MapFilterMode.Controlled;
            mapFilterClaimedRadioButton.Tag = MapPanelController.MapFilterMode.Claimed;
        }

        /// <summary>
        ///     Enable map filter
        /// </summary>
        private void EnableMapFilter()
        {
            mapFilterGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Process when changing the check status of the map filter radio button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapFilterRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null)
            {
                return;
            }

            // When there is no check, other items are checked and will not be processed.
            if (!radioButton.Checked)
            {
                return;
            }

            // Update filter mode
            _mapPanelController.FilterMode = (MapPanelController.MapFilterMode) radioButton.Tag;
        }

        #endregion

        #region Province tab ―――― National filter

        /// <summary>
        ///     Update national filter
        /// </summary>
        private void UpdateProvinceCountryFilter()
        {
            provinceCountryFilterComboBox.BeginUpdate();
            provinceCountryFilterComboBox.Items.Clear();
            provinceCountryFilterComboBox.Items.Add("");
            foreach (Country country in Countries.Tags)
            {
                provinceCountryFilterComboBox.Items.Add(Scenarios.GetCountryTagName(country));
            }
            provinceCountryFilterComboBox.EndUpdate();
        }

        /// <summary>
        ///     Enable national filter
        /// </summary>
        private void EnableProvinceCountryFilter()
        {
            provinceCountryFilterLabel.Enabled = true;
            provinceCountryFilterComboBox.Enabled = true;
        }

        /// <summary>
        ///     Processing when changing the selection item of the national filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceCountryFilterComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            Country country = GetSelectedProvinceCountry();

            // Update the province list
            UpdateProvinceList(country);

            // Update map filter
            _mapPanelController.SelectedCountry = country;

            // Update Provins National Group Box edits
            Province province = GetSelectedProvince();
            if ((country != Country.None) && (province != null))
            {
                CountrySettings settings = Scenarios.GetCountrySettings(country);
                UpdateProvinceCountryItems(province, settings);
                EnableProvinceCountryItems();
            }
            else
            {
                DisableProvinceCountryItems();
                ClearProvinceCountryItems();
            }
        }

        /// <summary>
        ///     Get the country of choice
        /// </summary>
        /// <returns>Selected country</returns>
        private Country GetSelectedProvinceCountry()
        {
            if (provinceCountryFilterComboBox.SelectedIndex <= 0)
            {
                return Country.None;
            }
            return Countries.Tags[provinceCountryFilterComboBox.SelectedIndex - 1];
        }

        #endregion

        #region Providence stub ―――― Province ID

        /// <summary>
        ///     Province ID Initialize the text box
        /// </summary>
        private void InitProvinceIdTextBox()
        {
            _itemControls.Add(ScenarioEditorItemId.ProvinceId, provinceIdTextBox);

            provinceIdTextBox.Tag = ScenarioEditorItemId.ProvinceId;
        }

        /// <summary>
        ///     Providence ID Enable text box
        /// </summary>
        private void EnableProvinceIdTextBox()
        {
            provinceIdLabel.Enabled = true;
            provinceIdTextBox.Enabled = true;
        }

        /// <summary>
        ///     Providence ID Of the text box ID Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceIdTextBoxValidated(object sender, EventArgs e)
        {
            Province province = GetSelectedProvince();

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(provinceIdTextBox.Text, out val))
            {
                if (province != null)
                {
                    provinceIdTextBox.Text = IntHelper.ToString(province.Id);
                }
                return;
            }

            // Do nothing if it has not changed from the initial value
            if ((province == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((province != null) && (val == province.Id))
            {
                return;
            }

            // Select Providence
            SelectProvince(val);
        }

        /// <summary>
        ///     Select Providence
        /// </summary>
        /// <param name="id">Providence ID</param>
        private void SelectProvince(int id)
        {
            // Change the selections in the province list view
            int index = _controller.GetLandProvinceIndex(id);
            if (index >= 0)
            {
                ListViewItem item = provinceListView.Items[index];
                item.Focused = true;
                item.Selected = true;
                item.EnsureVisible();
            }
        }

        #endregion

        #region Providence stub ―――― Provincial list

        /// <summary>
        ///     Initialize the land provision list
        /// </summary>
        private void InitProvinceList()
        {
            provinceListView.BeginUpdate();
            provinceListView.Items.Clear();
            foreach (Province province in _controller.GetLandProvinces())
            {
                ListViewItem item = CreateProvinceListItem(province);
                provinceListView.Items.Add(item);
            }
            provinceListView.EndUpdate();
        }

        /// <summary>
        ///     Update the province list
        /// </summary>
        /// <param name="country">Selected country</param>
        private void UpdateProvinceList(Country country)
        {
            CountrySettings settings = Scenarios.GetCountrySettings(country);

            provinceListView.BeginUpdate();
            if (settings != null)
            {
                foreach (ListViewItem item in provinceListView.Items)
                {
                    Province province = (Province) item.Tag;
                    item.SubItems[2].Text = province.Id == settings.Capital ? Resources.Yes : "";
                    item.SubItems[3].Text = settings.NationalProvinces.Contains(province.Id) ? Resources.Yes : "";
                    item.SubItems[4].Text = settings.OwnedProvinces.Contains(province.Id) ? Resources.Yes : "";
                    item.SubItems[5].Text = settings.ControlledProvinces.Contains(province.Id) ? Resources.Yes : "";
                    item.SubItems[6].Text = settings.ClaimedProvinces.Contains(province.Id) ? Resources.Yes : "";
                }
            }
            else
            {
                foreach (ListViewItem item in provinceListView.Items)
                {
                    item.SubItems[2].Text = "";
                    item.SubItems[3].Text = "";
                    item.SubItems[4].Text = "";
                    item.SubItems[5].Text = "";
                    item.SubItems[6].Text = "";
                }
            }
            provinceListView.EndUpdate();
        }

        /// <summary>
        ///     Activate the province list
        /// </summary>
        private void EnableProvinceList()
        {
            provinceListView.Enabled = true;
        }

        /// <summary>
        ///     Set the item string in the province list view
        /// </summary>
        /// <param name="index">Provincial list view index</param>
        /// <param name="no">Item Number</param>
        /// <param name="s">Character string</param>
        public void SetProvinceListItemText(int index, int no, string s)
        {
            provinceListView.Items[index].SubItems[no].Text = s;
        }

        /// <summary>
        ///     Create an item in the Providence list view
        /// </summary>
        /// <param name="province">Providence data</param>
        /// <returns>Province list view items</returns>
        private static ListViewItem CreateProvinceListItem(Province province)
        {
            ProvinceSettings settings = Scenarios.GetProvinceSettings(province.Id);

            ListViewItem item = new ListViewItem { Text = IntHelper.ToString(province.Id), Tag = province };
            item.SubItems.Add(Scenarios.GetProvinceName(province, settings));
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");

            return item;
        }

        /// <summary>
        ///     Get the selected province
        /// </summary>
        /// <returns></returns>
        private Province GetSelectedProvince()
        {
            if (provinceListView.SelectedIndices.Count == 0)
            {
                return null;
            }
            return provinceListView.SelectedItems[0].Tag as Province;
        }

        /// <summary>
        ///     Processing when changing the selection item in the province list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable edit items if there are no selections
            Province province = GetSelectedProvince();
            if (province == null)
            {
                // Disable edit items
                DisableProvinceCountryItems();
                DisableProvinceInfoItems();
                DisableProvinceResourceItems();
                DisableProvinceBuildingItems();

                // Clear the display of edit items
                ClearProvinceCountryItems();
                ClearProvinceInfoItems();
                ClearProvinceResourceItems();
                ClearProvinceBuildingItems();
                return;
            }

            ProvinceSettings settings = Scenarios.GetProvinceSettings(province.Id);
            Country country = GetSelectedProvinceCountry();
            CountrySettings countrySettings = Scenarios.GetCountrySettings(country);

            // Update the display of edit items
            UpdateProvinceCountryItems(province, countrySettings);
            UpdateProvinceInfoItems(province, settings);
            UpdateProvinceResourceItems(settings);
            UpdateProvinceBuildingItems(settings);

            // Enable edit items
            if (country != Country.None)
            {
                EnableProvinceCountryItems();
            }
            EnableProvinceInfoItems();
            EnableProvinceResourceItems();
            EnableProvinceBuildingItems();

            // Scroll the map
            _mapPanelController.ScrollToProvince(province.Id);
        }

        #endregion

        #region Providence stub ―――― National information

        /// <summary>
        ///     Initialize the edit items of the province national information
        /// </summary>
        private void InitProvinceCountryItems()
        {
            _itemControls.Add(ScenarioEditorItemId.CountryCapital, capitalCheckBox);
            _itemControls.Add(ScenarioEditorItemId.CountryCoreProvinces, coreProvinceCheckBox);
            _itemControls.Add(ScenarioEditorItemId.CountryOwnedProvinces, ownedProvinceCheckBox);
            _itemControls.Add(ScenarioEditorItemId.CountryControlledProvinces, controlledProvinceCheckBox);
            _itemControls.Add(ScenarioEditorItemId.CountryClaimedProvinces, claimedProvinceCheckBox);

            capitalCheckBox.Tag = ScenarioEditorItemId.CountryCapital;
            coreProvinceCheckBox.Tag = ScenarioEditorItemId.CountryCoreProvinces;
            ownedProvinceCheckBox.Tag = ScenarioEditorItemId.CountryOwnedProvinces;
            controlledProvinceCheckBox.Tag = ScenarioEditorItemId.CountryControlledProvinces;
            claimedProvinceCheckBox.Tag = ScenarioEditorItemId.CountryClaimedProvinces;
        }

        /// <summary>
        ///     Update the edit items of Providence National Information
        /// </summary>
        /// <param name="province">Providence</param>
        /// <param name="settings">National setting</param>
        private void UpdateProvinceCountryItems(Province province, CountrySettings settings)
        {
            _controller.UpdateItemValue(capitalCheckBox, province, settings);
            _controller.UpdateItemValue(coreProvinceCheckBox, province, settings);
            _controller.UpdateItemValue(ownedProvinceCheckBox, province, settings);
            _controller.UpdateItemValue(controlledProvinceCheckBox, province, settings);
            _controller.UpdateItemValue(claimedProvinceCheckBox, province, settings);

            _controller.UpdateItemColor(capitalCheckBox, province, settings);
            _controller.UpdateItemColor(coreProvinceCheckBox, province, settings);
            _controller.UpdateItemColor(ownedProvinceCheckBox, province, settings);
            _controller.UpdateItemColor(controlledProvinceCheckBox, province, settings);
            _controller.UpdateItemColor(claimedProvinceCheckBox, province, settings);
        }

        /// <summary>
        ///     Clear the display of edit items of Providence national information
        /// </summary>
        private void ClearProvinceCountryItems()
        {
            capitalCheckBox.Checked = false;
            coreProvinceCheckBox.Checked = false;
            ownedProvinceCheckBox.Checked = false;
            controlledProvinceCheckBox.Checked = false;
            claimedProvinceCheckBox.Checked = false;
        }

        /// <summary>
        ///     Enable edit items for Providence national information
        /// </summary>
        private void EnableProvinceCountryItems()
        {
            provinceCountryGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for province national information
        /// </summary>
        private void DisableProvinceCountryItems()
        {
            provinceCountryGroupBox.Enabled = false;
        }

        #endregion

        #region Province tab ―――― Providence information

        /// <summary>
        ///     Initialize the edit items of the provision information
        /// </summary>
        private void InitProvinceInfoItems()
        {
            _itemControls.Add(ScenarioEditorItemId.ProvinceNameKey, provinceNameKeyTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNameString, provinceNameStringTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceVp, vpTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRevoltRisk, revoltRiskTextBox);

            provinceNameKeyTextBox.Tag = ScenarioEditorItemId.ProvinceNameKey;
            provinceNameStringTextBox.Tag = ScenarioEditorItemId.ProvinceNameString;
            vpTextBox.Tag = ScenarioEditorItemId.ProvinceVp;
            revoltRiskTextBox.Tag = ScenarioEditorItemId.ProvinceRevoltRisk;
        }

        /// <summary>
        ///     Update the edit items of the province information
        /// </summary>
        /// <param name="province">Providence</param>
        /// <param name="settings">Providence settings</param>
        private void UpdateProvinceInfoItems(Province province, ProvinceSettings settings)
        {
            _controller.UpdateItemValue(provinceIdTextBox, province);
            _controller.UpdateItemValue(provinceNameKeyTextBox, province, settings);
            _controller.UpdateItemValue(provinceNameStringTextBox, province, settings);
            _controller.UpdateItemValue(vpTextBox, settings);
            _controller.UpdateItemValue(revoltRiskTextBox, settings);

            _controller.UpdateItemColor(provinceNameKeyTextBox, settings);
            _controller.UpdateItemColor(provinceNameStringTextBox, settings);
            _controller.UpdateItemColor(vpTextBox, settings);
            _controller.UpdateItemColor(revoltRiskTextBox, settings);
        }

        /// <summary>
        ///     Clear the display of edit items in province information
        /// </summary>
        private void ClearProvinceInfoItems()
        {
            provinceIdTextBox.Text = "";
            provinceNameKeyTextBox.Text = "";
            provinceNameStringTextBox.Text = "";
            vpTextBox.Text = "";
            revoltRiskTextBox.Text = "";
        }

        /// <summary>
        ///     Enable edit items for provision information
        /// </summary>
        private void EnableProvinceInfoItems()
        {
            provinceInfoGroupBox.Enabled = true;
            provinceNameKeyTextBox.Enabled = Game.Type == GameType.DarkestHour;
        }

        /// <summary>
        ///     Disable edit items in province information
        /// </summary>
        private void DisableProvinceInfoItems()
        {
            provinceInfoGroupBox.Enabled = false;
        }

        #endregion

        #region Province tab ―――― Resource information

        /// <summary>
        ///     Initialize edit items of Providence resource information
        /// </summary>
        private void InitProvinceResourceItems()
        {
            // Resource name label
            provinceManpowerLabel.Text = Config.GetText(TextId.ResourceManpower);
            provinceEnergyLabel.Text = Config.GetText(TextId.ResourceEnergy);
            provinceMetalLabel.Text = Config.GetText(TextId.ResourceMetal);
            provinceRareMaterialsLabel.Text = Config.GetText(TextId.ResourceRareMaterials);
            provinceOilLabel.Text = Config.GetText(TextId.ResourceOil);
            provinceSuppliesLabel.Text = Config.GetText(TextId.ResourceSupplies);

            // Edit items
            _itemControls.Add(ScenarioEditorItemId.ProvinceManpowerCurrent, manpowerCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceManpowerMax, manpowerMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceEnergyPool, energyPoolTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceEnergyCurrent, energyCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceEnergyMax, energyMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceMetalPool, metalPoolTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceMetalCurrent, metalCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceMetalMax, metalMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRareMaterialsPool, rareMaterialsPoolTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRareMaterialsCurrent, rareMaterialsCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRareMaterialsMax, rareMaterialsMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceOilPool, oilPoolTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceOilCurrent, oilCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceOilMax, oilMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSupplyPool, suppliesPoolTextBox);

            manpowerCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceManpowerCurrent;
            manpowerMaxTextBox.Tag = ScenarioEditorItemId.ProvinceManpowerMax;
            energyPoolTextBox.Tag = ScenarioEditorItemId.ProvinceEnergyPool;
            energyCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceEnergyCurrent;
            energyMaxTextBox.Tag = ScenarioEditorItemId.ProvinceEnergyMax;
            metalPoolTextBox.Tag = ScenarioEditorItemId.ProvinceMetalPool;
            metalCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceMetalCurrent;
            metalMaxTextBox.Tag = ScenarioEditorItemId.ProvinceMetalMax;
            rareMaterialsPoolTextBox.Tag = ScenarioEditorItemId.ProvinceRareMaterialsPool;
            rareMaterialsCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceRareMaterialsCurrent;
            rareMaterialsMaxTextBox.Tag = ScenarioEditorItemId.ProvinceRareMaterialsMax;
            oilPoolTextBox.Tag = ScenarioEditorItemId.ProvinceOilPool;
            oilCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceOilCurrent;
            oilMaxTextBox.Tag = ScenarioEditorItemId.ProvinceOilMax;
            suppliesPoolTextBox.Tag = ScenarioEditorItemId.ProvinceSupplyPool;
        }

        /// <summary>
        ///     Update the edit items of Providence resource information
        /// </summary>
        /// <param name="settings">Providence settings</param>
        private void UpdateProvinceResourceItems(ProvinceSettings settings)
        {
            _controller.UpdateItemValue(manpowerCurrentTextBox, settings);
            _controller.UpdateItemValue(manpowerMaxTextBox, settings);
            _controller.UpdateItemValue(energyPoolTextBox, settings);
            _controller.UpdateItemValue(energyCurrentTextBox, settings);
            _controller.UpdateItemValue(energyMaxTextBox, settings);
            _controller.UpdateItemValue(metalPoolTextBox, settings);
            _controller.UpdateItemValue(metalCurrentTextBox, settings);
            _controller.UpdateItemValue(metalMaxTextBox, settings);
            _controller.UpdateItemValue(rareMaterialsPoolTextBox, settings);
            _controller.UpdateItemValue(rareMaterialsCurrentTextBox, settings);
            _controller.UpdateItemValue(rareMaterialsMaxTextBox, settings);
            _controller.UpdateItemValue(oilPoolTextBox, settings);
            _controller.UpdateItemValue(oilCurrentTextBox, settings);
            _controller.UpdateItemValue(oilMaxTextBox, settings);
            _controller.UpdateItemValue(suppliesPoolTextBox, settings);

            _controller.UpdateItemColor(manpowerCurrentTextBox, settings);
            _controller.UpdateItemColor(manpowerMaxTextBox, settings);
            _controller.UpdateItemColor(energyPoolTextBox, settings);
            _controller.UpdateItemColor(energyCurrentTextBox, settings);
            _controller.UpdateItemColor(energyMaxTextBox, settings);
            _controller.UpdateItemColor(metalPoolTextBox, settings);
            _controller.UpdateItemColor(metalCurrentTextBox, settings);
            _controller.UpdateItemColor(metalMaxTextBox, settings);
            _controller.UpdateItemColor(rareMaterialsPoolTextBox, settings);
            _controller.UpdateItemColor(rareMaterialsCurrentTextBox, settings);
            _controller.UpdateItemColor(rareMaterialsMaxTextBox, settings);
            _controller.UpdateItemColor(oilPoolTextBox, settings);
            _controller.UpdateItemColor(oilCurrentTextBox, settings);
            _controller.UpdateItemColor(oilMaxTextBox, settings);
            _controller.UpdateItemColor(suppliesPoolTextBox, settings);
        }

        /// <summary>
        ///     Clear the display of edit items of province resource information
        /// </summary>
        private void ClearProvinceResourceItems()
        {
            manpowerCurrentTextBox.Text = "";
            manpowerMaxTextBox.Text = "";
            energyPoolTextBox.Text = "";
            energyCurrentTextBox.Text = "";
            energyMaxTextBox.Text = "";
            metalPoolTextBox.Text = "";
            metalCurrentTextBox.Text = "";
            metalMaxTextBox.Text = "";
            rareMaterialsPoolTextBox.Text = "";
            rareMaterialsCurrentTextBox.Text = "";
            rareMaterialsMaxTextBox.Text = "";
            oilPoolTextBox.Text = "";
            oilCurrentTextBox.Text = "";
            oilMaxTextBox.Text = "";
            suppliesPoolTextBox.Text = "";
        }

        /// <summary>
        ///     Enable edit items for province resource information
        /// </summary>
        private void EnableProvinceResourceItems()
        {
            provinceResourceGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for Providence resource information
        /// </summary>
        private void DisableProvinceResourceItems()
        {
            provinceResourceGroupBox.Enabled = false;
        }

        #endregion

        #region Providence stub ―――― Building information

        /// <summary>
        ///     Initialize the edit items of the province building information
        /// </summary>
        private void InitProvinceBuildingItems()
        {
            _itemControls.Add(ScenarioEditorItemId.ProvinceIcCurrent, icCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceIcMax, icMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceIcRelative, icRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceInfrastructureCurrent, infrastructureCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceInfrastructureMax, infrastructureMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceInfrastructureRelative, infrastructureRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceLandFortCurrent, landFortCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceLandFortMax, landFortMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceLandFortRelative, landFortRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceCoastalFortCurrent, coastalFortCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceCoastalFortMax, coastalFortMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceCoastalFortRelative, coastalFortRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAntiAirCurrent, antiAirCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAntiAirMax, antiAirMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAntiAirRelative, antiAirRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAirBaseCurrent, airBaseCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAirBaseMax, airBaseMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceAirBaseRelative, airBaseRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNavalBaseCurrent, navalBaseCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNavalBaseMax, navalBaseMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNavalBaseRelative, navalBaseRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRadarStationCurrent, radarStationCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRadarStationMax, radarStationMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRadarStationRelative, radarStationRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearReactorCurrent, nuclearReactorCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearReactorMax, nuclearReactorMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearReactorRelative, nuclearReactorRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRocketTestCurrent, rocketTestCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRocketTestMax, rocketTestMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceRocketTestRelative, rocketTestRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticOilCurrent, syntheticOilCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticOilMax, syntheticOilMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticOilRelative, syntheticOilRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticRaresCurrent, syntheticRaresCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticRaresMax, syntheticRaresMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceSyntheticRaresRelative, syntheticRaresRelativeTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearPowerCurrent, nuclearPowerCurrentTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearPowerMax, nuclearPowerMaxTextBox);
            _itemControls.Add(ScenarioEditorItemId.ProvinceNuclearPowerRelative, nuclearPowerRelativeTextBox);

            icCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceIcCurrent;
            icMaxTextBox.Tag = ScenarioEditorItemId.ProvinceIcMax;
            icRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceIcRelative;
            infrastructureCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceInfrastructureCurrent;
            infrastructureMaxTextBox.Tag = ScenarioEditorItemId.ProvinceInfrastructureMax;
            infrastructureRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceInfrastructureRelative;
            landFortCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceLandFortCurrent;
            landFortMaxTextBox.Tag = ScenarioEditorItemId.ProvinceLandFortMax;
            landFortRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceLandFortRelative;
            coastalFortCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceCoastalFortCurrent;
            coastalFortMaxTextBox.Tag = ScenarioEditorItemId.ProvinceCoastalFortMax;
            coastalFortRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceCoastalFortRelative;
            antiAirCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceAntiAirCurrent;
            antiAirMaxTextBox.Tag = ScenarioEditorItemId.ProvinceAntiAirMax;
            antiAirRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceAntiAirRelative;
            airBaseCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceAirBaseCurrent;
            airBaseMaxTextBox.Tag = ScenarioEditorItemId.ProvinceAirBaseMax;
            airBaseRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceAirBaseRelative;
            navalBaseCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceNavalBaseCurrent;
            navalBaseMaxTextBox.Tag = ScenarioEditorItemId.ProvinceNavalBaseMax;
            navalBaseRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceNavalBaseRelative;
            radarStationCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceRadarStationCurrent;
            radarStationMaxTextBox.Tag = ScenarioEditorItemId.ProvinceRadarStationMax;
            radarStationRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceRadarStationRelative;
            nuclearReactorCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearReactorCurrent;
            nuclearReactorMaxTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearReactorMax;
            nuclearReactorRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearReactorRelative;
            rocketTestCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceRocketTestCurrent;
            rocketTestMaxTextBox.Tag = ScenarioEditorItemId.ProvinceRocketTestMax;
            rocketTestRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceRocketTestRelative;
            syntheticOilCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticOilCurrent;
            syntheticOilMaxTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticOilMax;
            syntheticOilRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticOilRelative;
            syntheticRaresCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticRaresCurrent;
            syntheticRaresMaxTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticRaresMax;
            syntheticRaresRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceSyntheticRaresRelative;
            nuclearPowerCurrentTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearPowerCurrent;
            nuclearPowerMaxTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearPowerMax;
            nuclearPowerRelativeTextBox.Tag = ScenarioEditorItemId.ProvinceNuclearPowerRelative;
        }

        /// <summary>
        ///     Update the edit items of the Providence building information
        /// </summary>
        /// <param name="settings">Providence settings</param>
        private void UpdateProvinceBuildingItems(ProvinceSettings settings)
        {
            _controller.UpdateItemValue(icCurrentTextBox, settings);
            _controller.UpdateItemValue(icMaxTextBox, settings);
            _controller.UpdateItemValue(icRelativeTextBox, settings);
            _controller.UpdateItemValue(infrastructureCurrentTextBox, settings);
            _controller.UpdateItemValue(infrastructureMaxTextBox, settings);
            _controller.UpdateItemValue(infrastructureRelativeTextBox, settings);
            _controller.UpdateItemValue(landFortCurrentTextBox, settings);
            _controller.UpdateItemValue(landFortMaxTextBox, settings);
            _controller.UpdateItemValue(landFortRelativeTextBox, settings);
            _controller.UpdateItemValue(coastalFortCurrentTextBox, settings);
            _controller.UpdateItemValue(coastalFortMaxTextBox, settings);
            _controller.UpdateItemValue(coastalFortRelativeTextBox, settings);
            _controller.UpdateItemValue(antiAirCurrentTextBox, settings);
            _controller.UpdateItemValue(antiAirMaxTextBox, settings);
            _controller.UpdateItemValue(antiAirRelativeTextBox, settings);
            _controller.UpdateItemValue(airBaseCurrentTextBox, settings);
            _controller.UpdateItemValue(airBaseMaxTextBox, settings);
            _controller.UpdateItemValue(airBaseRelativeTextBox, settings);
            _controller.UpdateItemValue(navalBaseCurrentTextBox, settings);
            _controller.UpdateItemValue(navalBaseMaxTextBox, settings);
            _controller.UpdateItemValue(navalBaseRelativeTextBox, settings);
            _controller.UpdateItemValue(radarStationCurrentTextBox, settings);
            _controller.UpdateItemValue(radarStationMaxTextBox, settings);
            _controller.UpdateItemValue(radarStationRelativeTextBox, settings);
            _controller.UpdateItemValue(nuclearReactorCurrentTextBox, settings);
            _controller.UpdateItemValue(nuclearReactorMaxTextBox, settings);
            _controller.UpdateItemValue(nuclearReactorRelativeTextBox, settings);
            _controller.UpdateItemValue(rocketTestCurrentTextBox, settings);
            _controller.UpdateItemValue(rocketTestMaxTextBox, settings);
            _controller.UpdateItemValue(rocketTestRelativeTextBox, settings);
            _controller.UpdateItemValue(syntheticOilCurrentTextBox, settings);
            _controller.UpdateItemValue(syntheticOilMaxTextBox, settings);
            _controller.UpdateItemValue(syntheticOilRelativeTextBox, settings);
            _controller.UpdateItemValue(syntheticRaresCurrentTextBox, settings);
            _controller.UpdateItemValue(syntheticRaresMaxTextBox, settings);
            _controller.UpdateItemValue(syntheticRaresRelativeTextBox, settings);
            _controller.UpdateItemValue(nuclearPowerCurrentTextBox, settings);
            _controller.UpdateItemValue(nuclearPowerMaxTextBox, settings);
            _controller.UpdateItemValue(nuclearPowerRelativeTextBox, settings);

            _controller.UpdateItemColor(icCurrentTextBox, settings);
            _controller.UpdateItemColor(icMaxTextBox, settings);
            _controller.UpdateItemColor(icRelativeTextBox, settings);
            _controller.UpdateItemColor(infrastructureCurrentTextBox, settings);
            _controller.UpdateItemColor(infrastructureMaxTextBox, settings);
            _controller.UpdateItemColor(infrastructureRelativeTextBox, settings);
            _controller.UpdateItemColor(landFortCurrentTextBox, settings);
            _controller.UpdateItemColor(landFortMaxTextBox, settings);
            _controller.UpdateItemColor(landFortRelativeTextBox, settings);
            _controller.UpdateItemColor(coastalFortCurrentTextBox, settings);
            _controller.UpdateItemColor(coastalFortMaxTextBox, settings);
            _controller.UpdateItemColor(coastalFortRelativeTextBox, settings);
            _controller.UpdateItemColor(antiAirCurrentTextBox, settings);
            _controller.UpdateItemColor(antiAirMaxTextBox, settings);
            _controller.UpdateItemColor(antiAirRelativeTextBox, settings);
            _controller.UpdateItemColor(airBaseCurrentTextBox, settings);
            _controller.UpdateItemColor(airBaseMaxTextBox, settings);
            _controller.UpdateItemColor(airBaseRelativeTextBox, settings);
            _controller.UpdateItemColor(navalBaseCurrentTextBox, settings);
            _controller.UpdateItemColor(navalBaseMaxTextBox, settings);
            _controller.UpdateItemColor(navalBaseRelativeTextBox, settings);
            _controller.UpdateItemColor(radarStationCurrentTextBox, settings);
            _controller.UpdateItemColor(radarStationMaxTextBox, settings);
            _controller.UpdateItemColor(radarStationRelativeTextBox, settings);
            _controller.UpdateItemColor(nuclearReactorCurrentTextBox, settings);
            _controller.UpdateItemColor(nuclearReactorMaxTextBox, settings);
            _controller.UpdateItemColor(nuclearReactorRelativeTextBox, settings);
            _controller.UpdateItemColor(rocketTestCurrentTextBox, settings);
            _controller.UpdateItemColor(rocketTestMaxTextBox, settings);
            _controller.UpdateItemColor(rocketTestRelativeTextBox, settings);
            _controller.UpdateItemColor(syntheticOilCurrentTextBox, settings);
            _controller.UpdateItemColor(syntheticOilMaxTextBox, settings);
            _controller.UpdateItemColor(syntheticOilRelativeTextBox, settings);
            _controller.UpdateItemColor(syntheticRaresCurrentTextBox, settings);
            _controller.UpdateItemColor(syntheticRaresMaxTextBox, settings);
            _controller.UpdateItemColor(syntheticRaresRelativeTextBox, settings);
            _controller.UpdateItemColor(nuclearPowerCurrentTextBox, settings);
            _controller.UpdateItemColor(nuclearPowerMaxTextBox, settings);
            _controller.UpdateItemColor(nuclearPowerRelativeTextBox, settings);
        }

        /// <summary>
        ///     Clear the display of edit items of Providence building information
        /// </summary>
        private void ClearProvinceBuildingItems()
        {
            icCurrentTextBox.Text = "";
            icMaxTextBox.Text = "";
            icRelativeTextBox.Text = "";
            infrastructureCurrentTextBox.Text = "";
            infrastructureMaxTextBox.Text = "";
            infrastructureRelativeTextBox.Text = "";
            landFortCurrentTextBox.Text = "";
            landFortMaxTextBox.Text = "";
            landFortRelativeTextBox.Text = "";
            coastalFortCurrentTextBox.Text = "";
            coastalFortMaxTextBox.Text = "";
            coastalFortRelativeTextBox.Text = "";
            antiAirCurrentTextBox.Text = "";
            antiAirMaxTextBox.Text = "";
            antiAirRelativeTextBox.Text = "";
            airBaseCurrentTextBox.Text = "";
            airBaseMaxTextBox.Text = "";
            airBaseRelativeTextBox.Text = "";
            navalBaseCurrentTextBox.Text = "";
            navalBaseMaxTextBox.Text = "";
            navalBaseRelativeTextBox.Text = "";
            radarStationCurrentTextBox.Text = "";
            radarStationMaxTextBox.Text = "";
            radarStationRelativeTextBox.Text = "";
            nuclearReactorCurrentTextBox.Text = "";
            nuclearReactorMaxTextBox.Text = "";
            nuclearReactorRelativeTextBox.Text = "";
            rocketTestCurrentTextBox.Text = "";
            rocketTestMaxTextBox.Text = "";
            rocketTestRelativeTextBox.Text = "";
            syntheticOilCurrentTextBox.Text = "";
            syntheticOilMaxTextBox.Text = "";
            syntheticOilRelativeTextBox.Text = "";
            syntheticRaresCurrentTextBox.Text = "";
            syntheticRaresMaxTextBox.Text = "";
            syntheticRaresRelativeTextBox.Text = "";
            nuclearPowerCurrentTextBox.Text = "";
            nuclearPowerMaxTextBox.Text = "";
            nuclearPowerRelativeTextBox.Text = "";
        }

        /// <summary>
        ///     Enable edit items for Providence building information
        /// </summary>
        private void EnableProvinceBuildingItems()
        {
            provinceBuildingGroupBox.Enabled = true;

            bool flag = Game.Type == GameType.ArsenalOfDemocracy;
            provinceSyntheticOilLabel.Enabled = flag;
            syntheticOilCurrentTextBox.Enabled = flag;
            syntheticOilMaxTextBox.Enabled = flag;
            syntheticOilRelativeTextBox.Enabled = flag;
            provinceSyntheticRaresLabel.Enabled = flag;
            syntheticRaresCurrentTextBox.Enabled = flag;
            syntheticRaresMaxTextBox.Enabled = flag;
            syntheticRaresRelativeTextBox.Enabled = flag;
            provinceNuclearPowerLabel.Enabled = flag;
            nuclearPowerCurrentTextBox.Enabled = flag;
            nuclearPowerMaxTextBox.Enabled = flag;
            nuclearPowerRelativeTextBox.Enabled = flag;
        }

        /// <summary>
        ///     Disable edit items for Providence building information
        /// </summary>
        private void DisableProvinceBuildingItems()
        {
            provinceBuildingGroupBox.Enabled = false;
        }

        #endregion

        #region Providence stub ―――― Edit items

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            ProvinceSettings settings = Scenarios.GetProvinceSettings(province.Id);

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, settings);
                return;
            }

            // Do nothing if it has not changed from the initial value
            if ((settings == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = new ProvinceSettings { Id = province.Id };
                Scenarios.AddProvinceSettings(settings);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, settings);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceDoubleItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            ProvinceSettings settings = Scenarios.GetProvinceSettings(province.Id);

            // Returns the value if the string cannot be converted to a number
            double val;
            if (!DoubleHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, settings);
                return;
            }

            // Do nothing if it has not changed from the initial value
            if ((settings == null) && DoubleHelper.IsZero(val))
            {
                return;
            }

            // Do nothing if the value does not change
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev != null) && DoubleHelper.IsEqual(val, (double) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = new ProvinceSettings { Id = province.Id };
                Scenarios.AddProvinceSettings(settings);
            }

            _controller.OutputItemValueChangedLog(itemId, val, settings);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, settings);
        }

        /// <summary>
        ///     Processing when changing the value of a text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceStringItemTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            ProvinceSettings settings = Scenarios.GetProvinceSettings(province.Id);

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, province, settings);
            string val = control.Text;
            if ((prev == null) && string.IsNullOrEmpty(val))
            {
                return;
            }

            // Do nothing if the value does not change
            if (val.Equals(prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = new ProvinceSettings { Id = province.Id };
                Scenarios.AddProvinceSettings(settings);
            }

            _controller.OutputItemValueChangedLog(itemId, val, province, settings);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, settings);

            // Update value
            _controller.SetItemValue(itemId, val, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, province, settings);
        }

        /// <summary>
        ///     Processing when changing the check status of a check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }
            Country country = GetSelectedProvinceCountry();
            if (country == Country.None)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(country);

            // Do nothing if it has not changed from the initial value
            bool val = control.Checked;
            if ((settings == null) && !val)
            {
                return;
            }

            // Do nothing if the value does not change
            object prev = _controller.GetItemValue(itemId, province, settings);
            if ((prev != null) && (val == (bool) prev))
            {
                return;
            }

            if (settings == null)
            {
                settings = Scenarios.CreateCountrySettings(country);
            }

            _controller.OutputItemValueChangedLog(itemId, val, province, settings);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, province, settings);

            // Update value
            _controller.SetItemValue(itemId, val, province, settings);

            // Set the edited flag
            _controller.SetItemDirty(itemId, province, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, val, province, settings);
        }

        #endregion

        #endregion

        #region Initial unit tab

        #region Initial unit tab ―――― common

        /// <summary>
        ///     Initialize the initial unit tab
        /// </summary>
        private void InitOobTab()
        {
            InitUnitTree();
            InitOobUnitItems();
            InitOobDivisionItems();
        }

        /// <summary>
        ///     Update the display of the initial unit tab
        /// </summary>
        private void UpdateOobTab()
        {
            // Do nothing if initialized
            if (_tabPageInitialized[(int) TabPageNo.Oob])
            {
                return;
            }

            // Initialize the provision list
            _controller.InitProvinceList();

            // Initialize the unit type list
            _controller.InitUnitTypeList();

            // Deselect a country from the unit tree controller
            _unitTreeController.Country = Country.None;

            // Update national list box
            UpdateCountryListBox(oobCountryListBox);

            // Enable national list box
            EnableOobCountryListBox();

            // Enable the unit tree
            EnableUnitTree();

            // Disable edit items
            DisableOobUnitItems();
            DisableOobDivisionItems();

            // Clear edit items
            ClearOobUnitItems();
            ClearOobDivisionItems();

            // Set the initialized flag
            _tabPageInitialized[(int) TabPageNo.Oob] = true;
        }

        /// <summary>
        ///     Processing when loading the form of the initial unit tab
        /// </summary>
        private void OnOobTabPageFormLoad()
        {
            // Initialize the initial unit tab
            InitOobTab();
        }

        /// <summary>
        ///     Processing when reading a file on the initial unit tab
        /// </summary>
        private void OnOobTabPageFileLoad()
        {
            // Do nothing unless the initial unit tab is selected
            if (_tabPageNo != TabPageNo.Oob)
            {
                return;
            }

            // Wait until the commander data has been read
            Leaders.WaitLoading();

            // Wait until the provision data has been read
            Provinces.WaitLoading();

            // Wait until the unit data reading is completed
            Units.WaitLoading();

            // Update the display at the first transition
            UpdateOobTab();
        }

        /// <summary>
        ///     Processing when initial unit tab is selected
        /// </summary>
        private void OnOobTabPageSelected()
        {
            // Do nothing if scenario not loaded
            if (!Scenarios.IsLoaded())
            {
                return;
            }

            // Wait until the commander data has been read
            Leaders.WaitLoading();

            // Wait until the provision data has been read
            Provinces.WaitLoading();

            // Wait until the unit data reading is completed
            Units.WaitLoading();

            // Update the display at the first transition
            UpdateOobTab();
        }

        #endregion

        #region Initial unit tab ―――― Nation

        /// <summary>
        ///     Activate the national list box
        /// </summary>
        private void EnableOobCountryListBox()
        {
            oobCountryListBox.Enabled = true;
        }

        /// <summary>
        ///     Processing when changing the selection item of the national list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOobCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable edit items if there are no selections
            if (oobCountryListBox.SelectedIndex < 0)
            {
                // Disable edit items
                DisableOobUnitItems();
                DisableOobDivisionItems();

                // Clear edit items
                ClearOobUnitItems();
                ClearOobDivisionItems();
                return;
            }

            _selectedCountry = Countries.Tags[oobCountryListBox.SelectedIndex];

            // Initialize the commander list
            ScenarioHeader header = Scenarios.Data.Header;
            int year = header.StartDate?.Year ?? header.StartYear;
            _controller.UpdateLeaderList(_selectedCountry, year);

            // Update the unit tree
            _unitTreeController.Country = _selectedCountry;
        }

        #endregion

        #region Initial unit tab ―――― Unit tree

        /// <summary>
        ///     Initialize the unit tree
        /// </summary>
        private void InitUnitTree()
        {
            _unitTreeController.AfterSelect += OnUnitTreeAfterSelect;
        }

        /// <summary>
        ///     Enable the unit tree
        /// </summary>
        private void EnableUnitTree()
        {
            unitTreeView.Enabled = true;

            // Disable tree operation buttons
            oobAddUnitButton.Enabled = false;
            oobAddDivisionButton.Enabled = false;
            oobCloneButton.Enabled = false;
            oobRemoveButton.Enabled = false;
            oobTopButton.Enabled = false;
            oobUpButton.Enabled = false;
            oobDownButton.Enabled = false;
            oobBottomButton.Enabled = false;
        }

        /// <summary>
        ///     Processing when selecting a node in the unit tree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnitTreeAfterSelect(object sender, UnitTreeController.UnitTreeViewEventArgs e)
        {
            // Update button status
            oobAddUnitButton.Enabled = e.CanAddUnit;
            oobAddDivisionButton.Enabled = e.CanAddDivision;
            bool selected = (e.Unit != null) || (e.Division != null);
            oobCloneButton.Enabled = selected;
            oobRemoveButton.Enabled = selected;
            TreeNode parent = e.Node.Parent;
            if (selected && (parent != null))
            {
                int index = parent.Nodes.IndexOf(e.Node);
                int bottom = parent.Nodes.Count - 1;
                oobTopButton.Enabled = index > 0;
                oobUpButton.Enabled = index > 0;
                oobDownButton.Enabled = index < bottom;
                oobBottomButton.Enabled = index < bottom;
            }
            else
            {
                oobTopButton.Enabled = false;
                oobUpButton.Enabled = false;
                oobDownButton.Enabled = false;
                oobBottomButton.Enabled = false;
            }

            if (e.Unit != null)
            {
                UpdateOobUnitItems(e.Unit);
                EnableOobUnitItems();
            }
            else
            {
                DisableOobUnitItems();
                ClearOobUnitItems();
            }

            if (e.Division != null)
            {
                UpdateOobDivisionItems(e.Division);
                EnableOobDivisionItems();
            }
            else
            {
                DisableOobDivisionItems();
                ClearOobDivisionItems();
            }
        }

        /// <summary>
        ///     Processing when the button is pressed to the beginning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOobTopButtonClick(object sender, EventArgs e)
        {
            _unitTreeController.MoveTop();
        }

        /// <summary>
        ///     Processing when pressing the up button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOobUpButtonClick(object sender, EventArgs e)
        {
            _unitTreeController.MoveUp();
        }

        /// <summary>
        ///     Processing when the down button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOobDownButtonClick(object sender, EventArgs e)
        {
            _unitTreeController.MoveDown();
        }

        /// <summary>
        ///     Processing when the button is pressed to the end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOobBottomButtonClick(object sender, EventArgs e)
        {
            _unitTreeController.MoveBottom();
        }

        /// <summary>
        ///     Processing when a new unit button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOobAddUnitButtonClick(object sender, EventArgs e)
        {
            _unitTreeController.AddUnit();
        }

        /// <summary>
        ///     Processing when the new division button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOobAddDivisionButtonClick(object sender, EventArgs e)
        {
            _unitTreeController.AddDivision();
        }

        /// <summary>
        ///     Processing when the duplicate button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOobCloneButtonClick(object sender, EventArgs e)
        {
            _unitTreeController.Clone();
        }

        /// <summary>
        ///     Processing when the delete button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOobRemoveButtonClick(object sender, EventArgs e)
        {
            _unitTreeController.Remove();
        }

        #endregion

        #region Initial unit tab ―――― Unit information

        /// <summary>
        ///     Initialize the edit item of unit information
        /// </summary>
        private void InitOobUnitItems()
        {
            _itemControls.Add(ScenarioEditorItemId.UnitType, unitTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.UnitId, unitIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.UnitName, unitNameTextBox);
            _itemControls.Add(ScenarioEditorItemId.UnitLocationId, locationTextBox);
            _itemControls.Add(ScenarioEditorItemId.UnitLocation, locationComboBox);
            _itemControls.Add(ScenarioEditorItemId.UnitBaseId, baseTextBox);
            _itemControls.Add(ScenarioEditorItemId.UnitBase, baseComboBox);
            _itemControls.Add(ScenarioEditorItemId.UnitLeaderId, leaderTextBox);
            _itemControls.Add(ScenarioEditorItemId.UnitLeader, leaderComboBox);
            _itemControls.Add(ScenarioEditorItemId.UnitMorale, unitMoraleTextBox);
            _itemControls.Add(ScenarioEditorItemId.UnitDigIn, digInTextBox);

            unitTypeTextBox.Tag = ScenarioEditorItemId.UnitType;
            unitIdTextBox.Tag = ScenarioEditorItemId.UnitId;
            unitNameTextBox.Tag = ScenarioEditorItemId.UnitName;
            locationTextBox.Tag = ScenarioEditorItemId.UnitLocationId;
            locationComboBox.Tag = ScenarioEditorItemId.UnitLocation;
            baseTextBox.Tag = ScenarioEditorItemId.UnitBaseId;
            baseComboBox.Tag = ScenarioEditorItemId.UnitBase;
            leaderTextBox.Tag = ScenarioEditorItemId.UnitLeaderId;
            leaderComboBox.Tag = ScenarioEditorItemId.UnitLeader;
            unitMoraleTextBox.Tag = ScenarioEditorItemId.UnitMorale;
            digInTextBox.Tag = ScenarioEditorItemId.UnitDigIn;
        }

        /// <summary>
        ///     Update the edit item of unit information
        /// </summary>
        /// <param name="unit">unit</param>
        private void UpdateOobUnitItems(Unit unit)
        {
            _controller.UpdateItemValue(unitTypeTextBox, unit);
            _controller.UpdateItemValue(unitIdTextBox, unit);
            _controller.UpdateItemValue(unitNameTextBox, unit);
            _controller.UpdateItemValue(locationTextBox, unit);
            _controller.UpdateItemValue(baseTextBox, unit);
            _controller.UpdateItemValue(leaderTextBox, unit);
            _controller.UpdateItemValue(unitMoraleTextBox, unit);
            _controller.UpdateItemValue(digInTextBox, unit);

            _controller.UpdateItemColor(unitTypeTextBox, unit);
            _controller.UpdateItemColor(unitIdTextBox, unit);
            _controller.UpdateItemColor(unitNameTextBox, unit);
            _controller.UpdateItemColor(locationTextBox, unit);
            _controller.UpdateItemColor(baseTextBox, unit);
            _controller.UpdateItemColor(leaderTextBox, unit);
            _controller.UpdateItemColor(unitMoraleTextBox, unit);
            _controller.UpdateItemColor(digInTextBox, unit);

            // If the unit's line is changed
            if (unit.Branch != _lastUnitBranch)
            {
                _lastUnitBranch = unit.Branch;

                // Change list choices
                _controller.UpdateListItems(locationComboBox, unit);
                _controller.UpdateListItems(baseComboBox, unit);
                _controller.UpdateListItems(leaderComboBox, unit);

                // Editing restrictions by military department
                switch (unit.Branch)
                {
                    case Branch.Army:
                        baseLabel.Enabled = false;
                        baseTextBox.Enabled = false;
                        baseComboBox.Enabled = false;
                        digInLabel.Enabled = true;
                        digInTextBox.Enabled = true;
                        break;

                    case Branch.Navy:
                    case Branch.Airforce:
                        baseLabel.Enabled = true;
                        baseTextBox.Enabled = true;
                        baseComboBox.Enabled = true;
                        digInLabel.Enabled = false;
                        digInTextBox.Enabled = false;
                        break;
                }
            }

            _controller.UpdateItemValue(locationComboBox, unit);
            _controller.UpdateItemValue(baseComboBox, unit);
            _controller.UpdateItemValue(leaderComboBox, unit);

            _controller.UpdateItemColor(locationComboBox, unit);
            _controller.UpdateItemColor(baseComboBox, unit);
            _controller.UpdateItemColor(leaderComboBox, unit);
        }

        /// <summary>
        ///     Clear the edit item of unit information
        /// </summary>
        private void ClearOobUnitItems()
        {
            unitTypeTextBox.Text = "";
            unitIdTextBox.Text = "";
            unitNameTextBox.Text = "";
            locationTextBox.Text = "";
            locationComboBox.SelectedIndex = -1;
            baseTextBox.Text = "";
            baseComboBox.SelectedIndex = -1;
            leaderTextBox.Text = "";
            leaderComboBox.SelectedIndex = -1;
            unitMoraleTextBox.Text = "";
            digInTextBox.Text = "";
        }

        /// <summary>
        ///     Enable edit items for unit information
        /// </summary>
        private void EnableOobUnitItems()
        {
            unitGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for unit information
        /// </summary>
        private void DisableOobUnitItems()
        {
            unitGroupBox.Enabled = false;
        }

        #endregion

        #region Initial unit tab ―――― Division information

        /// <summary>
        ///     Initialize the edit items of the division information
        /// </summary>
        private void InitOobDivisionItems()
        {
            _itemControls.Add(ScenarioEditorItemId.DivisionType, divisionTypeTextBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionId, divisionIdTextBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionName, divisionNameTextBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionUnitType, unitTypeComboBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionModel, unitModelComboBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionBrigadeType1, brigadeTypeComboBox1);
            _itemControls.Add(ScenarioEditorItemId.DivisionBrigadeType2, brigadeTypeComboBox2);
            _itemControls.Add(ScenarioEditorItemId.DivisionBrigadeType3, brigadeTypeComboBox3);
            _itemControls.Add(ScenarioEditorItemId.DivisionBrigadeType4, brigadeTypeComboBox4);
            _itemControls.Add(ScenarioEditorItemId.DivisionBrigadeType5, brigadeTypeComboBox5);
            _itemControls.Add(ScenarioEditorItemId.DivisionBrigadeModel1, brigadeModelComboBox1);
            _itemControls.Add(ScenarioEditorItemId.DivisionBrigadeModel2, brigadeModelComboBox2);
            _itemControls.Add(ScenarioEditorItemId.DivisionBrigadeModel3, brigadeModelComboBox3);
            _itemControls.Add(ScenarioEditorItemId.DivisionBrigadeModel4, brigadeModelComboBox4);
            _itemControls.Add(ScenarioEditorItemId.DivisionBrigadeModel5, brigadeModelComboBox5);
            _itemControls.Add(ScenarioEditorItemId.DivisionStrength, strengthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionMaxStrength, maxStrengthTextBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionOrganisation, organisationTextBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionMaxOrganisation, maxOrganisationTextBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionMorale, divisionMoraleTextBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionExperience, experienceTextBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionLocked, lockedCheckBox);
            _itemControls.Add(ScenarioEditorItemId.DivisionDormant, dormantCheckBox);

            divisionTypeTextBox.Tag = ScenarioEditorItemId.DivisionType;
            divisionIdTextBox.Tag = ScenarioEditorItemId.DivisionId;
            divisionNameTextBox.Tag = ScenarioEditorItemId.DivisionName;
            unitTypeComboBox.Tag = ScenarioEditorItemId.DivisionUnitType;
            unitModelComboBox.Tag = ScenarioEditorItemId.DivisionModel;
            brigadeTypeComboBox1.Tag = ScenarioEditorItemId.DivisionBrigadeType1;
            brigadeTypeComboBox2.Tag = ScenarioEditorItemId.DivisionBrigadeType2;
            brigadeTypeComboBox3.Tag = ScenarioEditorItemId.DivisionBrigadeType3;
            brigadeTypeComboBox4.Tag = ScenarioEditorItemId.DivisionBrigadeType4;
            brigadeTypeComboBox5.Tag = ScenarioEditorItemId.DivisionBrigadeType5;
            brigadeModelComboBox1.Tag = ScenarioEditorItemId.DivisionBrigadeModel1;
            brigadeModelComboBox2.Tag = ScenarioEditorItemId.DivisionBrigadeModel2;
            brigadeModelComboBox3.Tag = ScenarioEditorItemId.DivisionBrigadeModel3;
            brigadeModelComboBox4.Tag = ScenarioEditorItemId.DivisionBrigadeModel4;
            brigadeModelComboBox5.Tag = ScenarioEditorItemId.DivisionBrigadeModel5;
            strengthTextBox.Tag = ScenarioEditorItemId.DivisionStrength;
            maxStrengthTextBox.Tag = ScenarioEditorItemId.DivisionMaxStrength;
            organisationTextBox.Tag = ScenarioEditorItemId.DivisionOrganisation;
            maxOrganisationTextBox.Tag = ScenarioEditorItemId.DivisionMaxOrganisation;
            divisionMoraleTextBox.Tag = ScenarioEditorItemId.DivisionMorale;
            experienceTextBox.Tag = ScenarioEditorItemId.DivisionExperience;
            lockedCheckBox.Tag = ScenarioEditorItemId.DivisionLocked;
            dormantCheckBox.Tag = ScenarioEditorItemId.DivisionDormant;
        }

        /// <summary>
        ///     Update the edit items of the division information
        /// </summary>
        /// <param name="division">Division</param>
        private void UpdateOobDivisionItems(Division division)
        {
            _controller.UpdateItemValue(divisionTypeTextBox, division);
            _controller.UpdateItemValue(divisionIdTextBox, division);
            _controller.UpdateItemValue(divisionNameTextBox, division);
            _controller.UpdateItemValue(strengthTextBox, division);
            _controller.UpdateItemValue(maxStrengthTextBox, division);
            _controller.UpdateItemValue(organisationTextBox, division);
            _controller.UpdateItemValue(maxOrganisationTextBox, division);
            _controller.UpdateItemValue(divisionMoraleTextBox, division);
            _controller.UpdateItemValue(experienceTextBox, division);
            _controller.UpdateItemValue(lockedCheckBox, division);
            _controller.UpdateItemValue(dormantCheckBox, division);

            _controller.UpdateItemColor(divisionTypeTextBox, division);
            _controller.UpdateItemColor(divisionIdTextBox, division);
            _controller.UpdateItemColor(divisionNameTextBox, division);
            _controller.UpdateItemColor(strengthTextBox, division);
            _controller.UpdateItemColor(maxStrengthTextBox, division);
            _controller.UpdateItemColor(organisationTextBox, division);
            _controller.UpdateItemColor(maxOrganisationTextBox, division);
            _controller.UpdateItemColor(divisionMoraleTextBox, division);
            _controller.UpdateItemColor(experienceTextBox, division);
            _controller.UpdateItemColor(lockedCheckBox, division);
            _controller.UpdateItemColor(dormantCheckBox, division);

            CountrySettings settings = Scenarios.GetCountrySettings(_selectedCountry);

            // If the division's military department changes, so does the list choices
            if (division.Branch != _lastDivisionBranch)
            {
                _lastDivisionBranch = division.Branch;
                _controller.UpdateListItems(unitTypeComboBox, division, settings);
                _controller.UpdateListItems(brigadeTypeComboBox1, division, settings);
                _controller.UpdateListItems(brigadeTypeComboBox2, division, settings);
                _controller.UpdateListItems(brigadeTypeComboBox3, division, settings);
                _controller.UpdateListItems(brigadeTypeComboBox4, division, settings);
                _controller.UpdateListItems(brigadeTypeComboBox5, division, settings);
            }

            _controller.UpdateListItems(unitModelComboBox, division, settings);
            _controller.UpdateListItems(brigadeModelComboBox1, division, settings);
            _controller.UpdateListItems(brigadeModelComboBox2, division, settings);
            _controller.UpdateListItems(brigadeModelComboBox3, division, settings);
            _controller.UpdateListItems(brigadeModelComboBox4, division, settings);
            _controller.UpdateListItems(brigadeModelComboBox5, division, settings);

            _controller.UpdateItemValue(unitTypeComboBox, division);
            _controller.UpdateItemValue(brigadeTypeComboBox1, division);
            _controller.UpdateItemValue(brigadeTypeComboBox2, division);
            _controller.UpdateItemValue(brigadeTypeComboBox3, division);
            _controller.UpdateItemValue(brigadeTypeComboBox4, division);
            _controller.UpdateItemValue(brigadeTypeComboBox5, division);
            _controller.UpdateItemValue(unitModelComboBox, division);
            _controller.UpdateItemValue(brigadeModelComboBox1, division);
            _controller.UpdateItemValue(brigadeModelComboBox2, division);
            _controller.UpdateItemValue(brigadeModelComboBox3, division);
            _controller.UpdateItemValue(brigadeModelComboBox4, division);
            _controller.UpdateItemValue(brigadeModelComboBox5, division);

            _controller.UpdateItemColor(unitTypeComboBox, division);
            _controller.UpdateItemColor(brigadeTypeComboBox1, division);
            _controller.UpdateItemColor(brigadeTypeComboBox2, division);
            _controller.UpdateItemColor(brigadeTypeComboBox3, division);
            _controller.UpdateItemColor(brigadeTypeComboBox4, division);
            _controller.UpdateItemColor(brigadeTypeComboBox5, division);
            _controller.UpdateItemColor(unitModelComboBox, division);
            _controller.UpdateItemColor(brigadeModelComboBox1, division);
            _controller.UpdateItemColor(brigadeModelComboBox2, division);
            _controller.UpdateItemColor(brigadeModelComboBox3, division);
            _controller.UpdateItemColor(brigadeModelComboBox4, division);
            _controller.UpdateItemColor(brigadeModelComboBox5, division);
        }

        /// <summary>
        ///     Clear the edit items of the division information
        /// </summary>
        private void ClearOobDivisionItems()
        {
            divisionTypeTextBox.Text = "";
            divisionIdTextBox.Text = "";
            divisionNameTextBox.Text = "";
            unitTypeComboBox.SelectedIndex = -1;
            unitModelComboBox.SelectedIndex = -1;
            brigadeTypeComboBox1.SelectedIndex = -1;
            brigadeTypeComboBox2.SelectedIndex = -1;
            brigadeTypeComboBox3.SelectedIndex = -1;
            brigadeTypeComboBox4.SelectedIndex = -1;
            brigadeTypeComboBox5.SelectedIndex = -1;
            brigadeModelComboBox1.SelectedIndex = -1;
            brigadeModelComboBox2.SelectedIndex = -1;
            brigadeModelComboBox3.SelectedIndex = -1;
            brigadeModelComboBox4.SelectedIndex = -1;
            brigadeModelComboBox5.SelectedIndex = -1;
            strengthTextBox.Text = "";
            maxStrengthTextBox.Text = "";
            organisationTextBox.Text = "";
            maxOrganisationTextBox.Text = "";
            divisionMoraleTextBox.Text = "";
            experienceTextBox.Text = "";
            lockedCheckBox.Checked = false;
            dormantCheckBox.Checked = false;
        }

        /// <summary>
        ///     Enable edit items for division information
        /// </summary>
        private void EnableOobDivisionItems()
        {
            divisionGroupBox.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for division information
        /// </summary>
        private void DisableOobDivisionItems()
        {
            divisionGroupBox.Enabled = false;
        }

        #endregion

        #region Initial unit tab ―――― Edit items

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnitIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Unit unit = _unitTreeController.GetSelectedUnit();
            if (unit == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(_selectedCountry);

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, unit);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, unit);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, unit);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, unit);

            // Update value
            _controller.SetItemValue(itemId, val, unit);

            // Set the edited flag
            _controller.SetItemDirty(itemId, unit, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, unit);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnitDoubleItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Unit unit = _unitTreeController.GetSelectedUnit();
            if (unit == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(_selectedCountry);

            // Returns the value if the string cannot be converted to a number
            double val;
            if (!DoubleHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, unit);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, settings);
            if ((prev == null) && DoubleHelper.IsZero(val))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && DoubleHelper.IsEqual(val, (double) prev))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, unit);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, unit);

            // Update value
            _controller.SetItemValue(itemId, val, unit);

            // Set the edited flag
            _controller.SetItemDirty(itemId, unit, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, unit);
        }

        /// <summary>
        ///     Processing when changing the value of a text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnitStringItemTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Unit unit = _unitTreeController.GetSelectedUnit();
            if (unit == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(_selectedCountry);

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, unit);
            string val = control.Text;
            if ((prev == null) && string.IsNullOrEmpty(val))
            {
                return;
            }

            // Do nothing if the value does not change
            if (val.Equals(prev))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, unit);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, unit);

            // Update value
            _controller.SetItemValue(itemId, val, unit);

            // Set the edited flag
            _controller.SetItemDirty(itemId, unit, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, unit);
        }

        /// <summary>
        ///     Item drawing process of combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnitComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            Unit unit = _unitTreeController.GetSelectedUnit();
            if (unit == null)
            {
                return;
            }

            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;
            object val = _controller.GetItemValue(itemId, unit);
            object sel = _controller.GetListItemValue(itemId, e.Index, unit);
            Brush brush = ((int) val == (int) sel) && _controller.IsItemDirty(itemId, unit)
                ? new SolidBrush(Color.Red)
                : new SolidBrush(SystemColors.WindowText);
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnitComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            ComboBox control = (ComboBox) sender;
            int index = control.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            Unit unit = _unitTreeController.GetSelectedUnit();
            if (unit == null)
            {
                return;
            }

            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;
            CountrySettings settings = Scenarios.GetCountrySettings(_selectedCountry);

            // Do nothing if it has not changed from the initial value
            object val = _controller.GetListItemValue(itemId, index, unit);
            if (val == null)
            {
                return;
            }

            // Do nothing if the value does not change
            object prev = _controller.GetItemValue(itemId, unit);
            if ((prev != null) && ((int) val == (int) prev))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, unit);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, unit, settings);

            // Update value
            _controller.SetItemValue(itemId, val, unit);

            // Set the edited flag
            _controller.SetItemDirty(itemId, unit, settings);

            // Update drawing to change text color
            control.Refresh();

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, unit);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDivisionIntItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Division division = _unitTreeController.GetSelectedDivision();
            if (division == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(_selectedCountry);

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, division);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, division);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, division);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, division);

            // Update value
            _controller.SetItemValue(itemId, val, division);

            // Set the edited flag
            _controller.SetItemDirty(itemId, division, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, division, settings);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDivisionDoubleItemTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Division division = _unitTreeController.GetSelectedDivision();
            if (division == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(_selectedCountry);

            // Returns the value if the string cannot be converted to a number
            double val;
            if (!DoubleHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, division);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, division);
            if ((prev == null) && DoubleHelper.IsZero(val))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && DoubleHelper.IsEqual(val, (double) prev))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, division);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, division);

            // Update value
            _controller.SetItemValue(itemId, val, division);

            // Set the edited flag
            _controller.SetItemDirty(itemId, division, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, division, settings);
        }

        /// <summary>
        ///     Processing when changing the value of a text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDivisionStringItemTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Division division = _unitTreeController.GetSelectedDivision();
            if (division == null)
            {
                return;
            }

            TextBox control = sender as TextBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(_selectedCountry);

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, division);
            string val = control.Text;
            if ((prev == null) && string.IsNullOrEmpty(val))
            {
                return;
            }

            // Do nothing if the value does not change
            if (val.Equals(prev))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, division);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, division);

            // Update value
            _controller.SetItemValue(itemId, val, division);

            // Set the edited flag
            _controller.SetItemDirty(itemId, division, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, division, settings);
        }

        /// <summary>
        ///     Item drawing process of combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDivisionComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            Division division = _unitTreeController.GetSelectedDivision();
            if (division == null)
            {
                return;
            }

            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;
            object val = _controller.GetItemValue(itemId, division);
            object sel = _controller.GetListItemValue(itemId, e.Index, division);
            Brush brush = ((int) val == (int) sel) && _controller.IsItemDirty(itemId, division)
                ? new SolidBrush(Color.Red)
                : new SolidBrush(SystemColors.WindowText);
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDivisionComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            ComboBox control = (ComboBox) sender;
            int index = control.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            Division division = _unitTreeController.GetSelectedDivision();
            if (division == null)
            {
                return;
            }

            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;
            CountrySettings settings = Scenarios.GetCountrySettings(_selectedCountry);

            // Do nothing if it has not changed from the initial value
            object val = _controller.GetListItemValue(itemId, index, division);
            if (val == null)
            {
                return;
            }

            // Do nothing if the value does not change
            object prev = _controller.GetItemValue(itemId, division);
            if ((prev != null) && ((int) val == (int) prev))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, division);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, division, settings);

            // Update value
            _controller.SetItemValue(itemId, val, division);

            // Set the edited flag
            _controller.SetItemDirty(itemId, division, settings);

            // Update drawing to change text color
            control.Refresh();

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, division, settings);
        }

        /// <summary>
        ///     Processing after moving the focus of the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDivisionComboBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Division division = _unitTreeController.GetSelectedDivision();
            if (division == null)
            {
                return;
            }

            ComboBox control = sender as ComboBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(_selectedCountry);

            // Returns the value if the string cannot be converted to a number
            int val;
            if (!IntHelper.TryParse(control.Text, out val))
            {
                _controller.UpdateItemValue(control, division);
                return;
            }

            // Do nothing if it has not changed from the initial value
            object prev = _controller.GetItemValue(itemId, division);
            if ((prev == null) && (val == 0))
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (int) prev))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, division);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, division);

            // Update value
            _controller.SetItemValue(itemId, val, division);

            // Set the edited flag
            _controller.SetItemDirty(itemId, division, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, division, settings);
        }

        /// <summary>
        ///     Processing when changing the check status of a check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDivisionCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Division division = _unitTreeController.GetSelectedDivision();
            if (division == null)
            {
                return;
            }

            CheckBox control = sender as CheckBox;
            if (control == null)
            {
                return;
            }
            ScenarioEditorItemId itemId = (ScenarioEditorItemId) control.Tag;

            CountrySettings settings = Scenarios.GetCountrySettings(_selectedCountry);

            // Do nothing if it has not changed from the initial value
            bool val = control.Checked;
            object prev = _controller.GetItemValue(itemId, division);
            if ((prev == null) && !val)
            {
                return;
            }

            // Do nothing if the value does not change
            if ((prev != null) && (val == (bool) prev))
            {
                return;
            }

            _controller.OutputItemValueChangedLog(itemId, val, division);

            // Processing before changing item values
            _controller.PreItemChanged(itemId, val, division, settings);

            // Update value
            _controller.SetItemValue(itemId, val, division);

            // Set the edited flag
            _controller.SetItemDirty(itemId, division, settings);

            // Change the font color
            control.ForeColor = Color.Red;

            // Processing after changing the item value
            _controller.PostItemChanged(itemId, division, settings);
        }

        #endregion

        #endregion

        #region common

        #region common ―――― Nation

        /// <summary>
        ///     Update national list box
        /// </summary>
        /// <param name="control">Control</param>
        private static void UpdateCountryListBox(ListBox control)
        {
            control.BeginUpdate();
            control.Items.Clear();
            foreach (Country country in Countries.Tags)
            {
                control.Items.Add(Scenarios.GetCountryTagName(country));
            }
            control.EndUpdate();
        }

        /// <summary>
        ///     Update national combo box
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="allowEmpty">Whether to allow empty items</param>
        private void UpdateCountryComboBox(ComboBox control, bool allowEmpty)
        {
            Graphics g = Graphics.FromHwnd(Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;

            int width = control.Width;
            control.BeginUpdate();
            control.Items.Clear();
            if (allowEmpty)
            {
                control.Items.Add("");
            }
            foreach (Country country in Countries.Tags)
            {
                string s = Countries.GetTagName(country);
                control.Items.Add(s);
                width = Math.Max(width,
                    (int) g.MeasureString(s, control.Font).Width + SystemInformation.VerticalScrollBarWidth + margin);
            }
            control.DropDownWidth = width;
            control.EndUpdate();
        }

        /// <summary>
        ///     Item drawing process of national list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index < 0)
            {
                return;
            }

            ListBox control = sender as ListBox;
            if (control == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw an item
            Brush brush;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // Change the text color for items that have changed
                CountrySettings settings = Scenarios.GetCountrySettings(Countries.Tags[e.Index]);
                brush = new SolidBrush(settings != null
                    ? (settings.IsDirty() ? Color.Red : control.ForeColor)
                    : Color.LightGray);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = control.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        #endregion

        #region common ―――― Edit items

        /// <summary>
        ///     Edit items ID Get the control associated with
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public Control GetItemControl(ScenarioEditorItemId itemId)
        {
            return _itemControls.ContainsKey(itemId) ? _itemControls[itemId] : null;
        }

        #endregion

        #endregion
    }
}
