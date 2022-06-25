using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using HoI2Editor.Models;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Main form
    /// </summary>
    public partial class MainForm : Form
    {
        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // Initialization of window position
            InitPosition();
        }

        /// <summary>
        ///   Processing when loading a form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMainFormLoad(object sender, EventArgs e)
        {
            // Update the version character string
            Text = HoI2EditorController.Version;

            // Initialize the log level
            logLevelComboBox.SelectedIndex = Log.Level;

            // When the type of game is confirmed before the log initialization, the log outputs here
            if (Game.Type != GameType.None)
            {
                Game.OutputGameType();
                Game.OutputGameVersion();
            }

            // Initialize the prohibition check box for loading the map
            mapLoadCheckBox.Checked = Maps.ForbidLoad;

            // Set the game folder name in the initial state
            if (!string.IsNullOrEmpty(Game.FolderName))
            {
                gameFolderTextBox.Text = HoI2EditorController.Settings.Main.GameFolder;
                Log.Error("Game Folder: {0}", HoI2EditorController.Settings.Main.GameFolder);

                if (!string.IsNullOrEmpty(HoI2EditorController.Settings.Main.ModFolder))
                {
                    modTextBox.Text = HoI2EditorController.Settings.Main.ModFolder;
                    Log.Error("MOD Name: {0}", HoI2EditorController.Settings.Main.ModFolder);
                }
                if (!string.IsNullOrEmpty(HoI2EditorController.Settings.Main.ExportFolder))
                {
                    exportFolderTextBox.Text = HoI2EditorController.Settings.Main.ExportFolder;
                    Log.Error("Export Name: {0}", HoI2EditorController.Settings.Main.ExportFolder);
                }

                // Update the language list
                UpdateLanguage();

                // If the game folder name is not valid, disable data editing
                if (!Game.IsGameFolderActive)
                {
                    editGroupBox.Enabled = false;
                    return;
                }

                // If it is not used in other editor processes, enable data editing
                editGroupBox.Enabled = HoI2EditorController.LockMutex(Game.FolderName);
            }
            else
            {
                SetFolderName(Environment.CurrentDirectory);
            }
        }

        #endregion

        #region End processing

        /// <summary>
        ///     Processing when pressing the end button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExitButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Processing when closing a form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            // Close the form if it is not edited
            if (!HoI2EditorController.IsDirty())
            {
                return;
            }

            // Close the form if you have already canceled the save
            if (HoI2EditorController.SaveCanceled)
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
            }
        }

        #endregion

        #region Window position

        /// <summary>
        ///     Initialization of window position
        /// </summary>
        private void InitPosition()
        {
            // Window position
            Location = HoI2EditorController.Settings.Main.Location;
            Size = HoI2EditorController.Settings.Main.Size;
        }

        /// <summary>
        ///     Processing when moving the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMainFormMove(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                HoI2EditorController.Settings.Main.Location = Location;
            }
        }

        /// <summary>
        ///     Processing for form resization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMainFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                HoI2EditorController.Settings.Main.Size = Size;
            }
        }

        #endregion

        #region Individual editor call

        /// <summary>
        ///     Commander button pressing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeaderButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchLeaderEditorForm();
        }

        /// <summary>
        ///     Processing when pressing the cabinet button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinisterButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchMinisterEditorForm();
        }

        /// <summary>
        ///     Processing when pressing the research institution button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTeamButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchTeamEditorForm();
        }

        /// <summary>
        ///     Processing when pressing Provin button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchProvinceEditorForm();
        }

        /// <summary>
        ///     Technical tree button processing when pressing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchTechEditorForm();
        }

        /// <summary>
        ///     Processing when pressing the unit model button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnitButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchUnitEditorForm();
        }

        /// <summary>
        ///     Processing when pressing the game setting button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMiscButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchMiscEditorForm();
        }

        /// <summary>
        ///     Processing when pressing the corps name button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCorpsNameButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchCorpsNameEditorForm();
        }

        /// <summary>
        ///     Unit name button pressing when pressing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnitNameButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchUnitNameEditorForm();
        }

        /// <summary>
        ///     Treatment when pressing the model name button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModelNameButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchModelNameEditorForm();
        }

        /// <summary>
        ///     Random commander's name button pressing processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRandomLeaderButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchRandomLeaderEditorForm();
        }

        /// <summary>
        ///     Research speed button processing when pressing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResearchButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchResearchViewerForm();
        }

        /// <summary>
        ///     Processing when pressing the scenario button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScenarioButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchScenarioEditorForm();
        }

        /// <summary>
        ///     Processing when pressing the events button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventsButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.LaunchLocHelperForm();
        }

        #endregion

        #region Folder name

        /// <summary>
        ///     Processing when the game folder reference button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameFolderBrowseButtonClick(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                SelectedPath = Game.FolderName,
                ShowNewFolderButton = false,
                Description = Resources.OpenGameFolderDialogDescription
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                gameFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        ///     Game folder Name character string processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameFolderTextBoxTextChanged(object sender, EventArgs e)
        {
            // Return if the game folder name does not change
            if (gameFolderTextBox.Text.Equals(Game.FolderName))
            {
                return;
            }

            // Remember language mode
            LanguageMode prev = Config.LangMode;

            // Change the game folder name
            Game.FolderName = gameFolderTextBox.Text;

            // Update the language list when the language mode is changed
            if (Config.LangMode != prev)
            {
                UpdateLanguage();
            }

            // If the game folder name is not valid, disable data editing
            if (!Game.IsGameFolderActive)
            {
                editGroupBox.Enabled = false;
                return;
            }

            // If it is not used in other editor processes, enable data editing
            editGroupBox.Enabled = HoI2EditorController.LockMutex(Game.FolderName);
        }

        /// <summary>
        ///     MOD folder reference button processing when pressing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModFolderBrowseButtonClick(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                SelectedPath = Game.IsModActive ? Game.ModFolderName : Game.FolderName,
                ShowNewFolderButton = false,
                Description = Resources.OpenModFolderDialogDescription
            };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string modName = Path.GetFileName(dialog.SelectedPath);
            string folderName = Path.GetDirectoryName(dialog.SelectedPath);
            if (string.Equals(Path.GetFileName(folderName), Game.ModPathNameDh))
            {
                folderName = Path.GetDirectoryName(folderName);
            }

            // If the game folder of the MOD and the game folder of the saved folder do not match, clear the saved folder
            if (Game.IsExportFolderActive && !string.Equals(folderName, Game.FolderName))
            {
                exportFolderTextBox.Text = "";
            }

            gameFolderTextBox.Text = folderName;
            modTextBox.Text = modName;
        }

        /// <summary>
        ///     MOD Name character string processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModTextBoxTextChanged(object sender, EventArgs e)
        {
            Game.ModName = modTextBox.Text;
        }

        /// <summary>
        ///     Save folder Name Referral processing when pressing button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExportFolderBrowseButtonClick(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                SelectedPath = Game.IsExportFolderActive ? Game.ExportFolderName : Game.FolderName,
                ShowNewFolderButton = false,
                Description = Resources.OpenExportFolderDialogDescription
            };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string exportName = Path.GetFileName(dialog.SelectedPath);
            string folderName = Path.GetDirectoryName(dialog.SelectedPath);
            if (string.Equals(Path.GetFileName(folderName), Game.ModPathNameDh))
            {
                folderName = Path.GetDirectoryName(folderName);
            }

            // If the game folder of the saved folder and the MOD game folder do not match, clear the mod folder
            if (Game.IsModActive && !string.Equals(folderName, Game.FolderName))
            {
                modTextBox.Text = "";
            }

            gameFolderTextBox.Text = folderName;
            exportFolderTextBox.Text = exportName;
        }

        /// <summary>
        ///     Processing when saving folder character string changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExportFolderTextBoxTextChanged(object sender, EventArgs e)
        {
            Game.ExportName = exportFolderTextBox.Text;
        }

        /// <summary>
        ///     Processing when dragging into the game folder name text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameFolderTextBoxDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        /// <summary>
        ///     Processing when dropped into a game folder name text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameFolderTextBoxDragDrop(object sender, DragEventArgs e)
        {
            string[] fileNames = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
            gameFolderTextBox.Text = fileNames[0];
            modTextBox.Text = "";
            exportFolderTextBox.Text = "";
        }

        /// <summary>
        ///     Processing when dragging into a mod name text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModTextBoxDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            string[] fileNames = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
            string folderName = Path.GetDirectoryName(fileNames[0]);
            if (string.Equals(Path.GetFileName(folderName), Game.ModPathNameDh))
            {
                folderName = Path.GetDirectoryName(folderName);
            }

            // If the game folder of the MOD and the game folder of the saved folder do not match, do not allow drops
            if (Game.IsExportFolderActive && !string.Equals(folderName, Game.FolderName))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Copy;
        }

        /// <summary>
        ///     MOD name When dropped into a text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModTextBoxDragDrop(object sender, DragEventArgs e)
        {
            string[] fileNames = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
            string modName = Path.GetFileName(fileNames[0]);
            string folderName = Path.GetDirectoryName(fileNames[0]);
            if (string.Equals(Path.GetFileName(folderName), Game.ModPathNameDh))
            {
                folderName = Path.GetDirectoryName(folderName);
            }

            // If the game folder is not valid, clear if the saved folder name is set
            if (!Game.IsGameFolderActive && !string.IsNullOrEmpty(Game.ExportName))
            {
                exportFolderTextBox.Text = "";
            }

            gameFolderTextBox.Text = folderName;
            modTextBox.Text = modName;
        }

        /// <summary>
        ///     Processing when dragged into a save folder name text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExportFolderTextBoxDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            string[] fileNames = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
            string folderName = Path.GetDirectoryName(fileNames[0]);
            if (string.Equals(Path.GetFileName(folderName), Game.ModPathNameDh))
            {
                folderName = Path.GetDirectoryName(folderName);
            }

            // If the game folder of the saved folder and the MOD game folder do not match, do not allow drops
            if (Game.IsModActive && !string.Equals(folderName, Game.FolderName))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Copy;
        }

        /// <summary>
        ///     Processing when dropped into a saved folder name text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExportFolderTextBoxDragDrop(object sender, DragEventArgs e)
        {
            string[] fileNames = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
            string exportName = Path.GetFileName(fileNames[0]);
            string folderName = Path.GetDirectoryName(fileNames[0]);
            if (string.Equals(Path.GetFileName(folderName), Game.ModPathNameDh))
            {
                folderName = Path.GetDirectoryName(folderName);
            }

            // Clear if the MOD name is set when the game folder is not valid
            if (!Game.IsGameFolderActive && !string.IsNullOrEmpty(Game.ModName))
            {
                modTextBox.Text = "";
            }

            gameFolderTextBox.Text = folderName;
            exportFolderTextBox.Text = exportName;
        }

        /// <summary>
        ///     Processing when dragging to the main form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMainFormDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        /// <summary>
        ///     Processing when dropped to the main form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMainFormDragDrop(object sender, DragEventArgs e)
        {
            string[] fileNames = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
            SetFolderName(fileNames[0]);
        }

        /// <summary>
        ///     Set the game folder / MOD name / save folder name
        /// </summary>
        /// <param name="folderName">Target folder name</param>
        private void SetFolderName(string folderName)
        {
            if (!IsGameFolder(folderName))
            {
                string modName = Path.GetFileName(folderName);
                string subFolderName = Path.GetDirectoryName(folderName);
                if (string.Equals(Path.GetFileName(subFolderName), Game.ModPathNameDh))
                {
                    subFolderName = Path.GetDirectoryName(subFolderName);
                }
                if (IsGameFolder(subFolderName))
                {
                    gameFolderTextBox.Text = subFolderName;
                    modTextBox.Text = modName;
                    exportFolderTextBox.Text = "";
                    return;
                }
            }
            gameFolderTextBox.Text = folderName;
            modTextBox.Text = "";
            exportFolderTextBox.Text = "";
        }

        /// <summary>
        ///     Judge whether the specified folder is a game folder
        /// </summary>
        /// <param name="folderName">Target folder name</param>
        /// <returns>If it is a game folder, return True</returns>
        private static bool IsGameFolder(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                return false;
            }
            // Hearts of Iron 2 Japanese version
            if (File.Exists(Path.Combine(folderName, "DoomsdayJP.exe")))
            {
                return true;
            }
            // Hearts of Iron 2 English version
            if (File.Exists(Path.Combine(folderName, "Hoi2.exe")))
            {
                return true;
            }
            // Arsenal of Democracy Japanese version / English version
            if (File.Exists(Path.Combine(folderName, "AODGame.exe")))
            {
                return true;
            }
            // Darkest Hour English version
            if (File.Exists(Path.Combine(folderName, "Darkest Hour.exe")))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Game folder name / MOD name / Save folder Available
        /// </summary>
        public void EnableFolderChange()
        {
            gameFolderTextBox.Enabled = true;
            gameFolderBrowseButton.Enabled = true;
            modTextBox.Enabled = true;
            modFolderBrowseButton.Enabled = true;
            exportFolderTextBox.Enabled = true;
            exportFolderBrowseButton.Enabled = true;
        }

        /// <summary>
        ///     Game folder name / MOD name / Save folder No change
        /// </summary>
        public void DisableFolderChange()
        {
            gameFolderTextBox.Enabled = false;
            gameFolderBrowseButton.Enabled = false;
            modTextBox.Enabled = false;
            modFolderBrowseButton.Enabled = false;
            exportFolderTextBox.Enabled = false;
            exportFolderBrowseButton.Enabled = false;
        }

        #endregion

        #region Language

        /// <summary>
        ///     Update the language list
        /// </summary>
        private void UpdateLanguage()
        {
            // Register the language string
            languageComboBox.BeginUpdate();
            languageComboBox.Items.Clear();
            foreach (string s in Config.LanguageStrings[(int) Config.LangMode])
            {
                languageComboBox.Items.Add(s);
            }
            languageComboBox.EndUpdate();
            languageComboBox.SelectedIndex = 0;
        }

        /// <summary>
        ///     Processing at the time of language change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLanguageComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the language index
            Config.LangIndex = languageComboBox.SelectedIndex;

            // Set the code page
            switch (Config.LangMode)
            {
                case LanguageMode.Japanese:
                    // Japanese is 932
                    Game.CodePage = 932;
                    break;

                case LanguageMode.English:
                    // Russian is 1251 / other 1252
                    Game.CodePage = languageComboBox.SelectedIndex == 7 ? 1251 : 1252;
                    break;

                case LanguageMode.PatchedJapanese:
                    // Japanese is 932 / other 1252
                    Game.CodePage = languageComboBox.SelectedIndex == 0 ? 932 : 1252;
                    break;

                case LanguageMode.PatchedKorean:
                    // Korean is 949
                    Game.CodePage = 949;
                    break;

                case LanguageMode.PatchedTraditionalChinese:
                    // Traditional Chinese is 950
                    Game.CodePage = 950;
                    break;

                case LanguageMode.PatchedSimplifiedChinese:
                    // Simplified Chinese is 936
                    Game.CodePage = 936;
                    break;
            }
        }

        #endregion

        #region Option setting

        /// <summary>
        ///     Log -level combo box selection item processing when changing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogLevelComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            Log.Level = logLevelComboBox.SelectedIndex;
            Log.Error("[Log] Level: {0}", (TraceLevel) Log.Level);
        }

        /// <summary>
        ///     Map loading prohibited check box treatment when changing status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapLoadCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            Maps.ForbidLoad = mapLoadCheckBox.Checked;
        }

        #endregion
    }
}
