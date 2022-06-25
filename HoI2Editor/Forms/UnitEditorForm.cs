using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Controls;
using HoI2Editor.Models;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Unit model editor form
    /// </summary>
    public partial class UnitEditorForm : Form
    {
        #region Public constant

        /// <summary>
        ///     Number of columns in the unit model list view
        /// </summary>
        public const int ModelListColumnCount = 10;

        /// <summary>
        ///     Number of columns in improved list view
        /// </summary>
        public const int UpgradeListColumnCount = 3;

        /// <summary>
        ///     Number of columns in equipment list view
        /// </summary>
        public const int EquipmentListColumnCount = 2;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public UnitEditorForm()
        {
            InitializeComponent();

            // Form initialization
            InitForm();
        }

        /// <summary>
        ///     Initialize edit items
        /// </summary>
        private void InitEditableItems()
        {
            Graphics g = Graphics.FromHwnd(Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;

            // National list view
            countryListView.BeginUpdate();
            countryListView.Items.Clear();
            foreach (Country country in Countries.Tags)
            {
                countryListView.Items.Add(Countries.Strings[(int) country]);
            }
            countryListView.EndUpdate();

            // Military combo box
            branchComboBox.BeginUpdate();
            branchComboBox.Items.Clear();
            foreach (string s in Branches.GetNames())
            {
                branchComboBox.Items.Add(s);
            }
            branchComboBox.EndUpdate();

            // Attachable brigade list view
            allowedBrigadesListView.BeginUpdate();
            allowedBrigadesListView.Items.Clear();
            int width = 60;
            foreach (UnitType type in Units.BrigadeTypes)
            {
                string s = Units.Items[(int) type].ToString();
                allowedBrigadesListView.Items.Add(s);
                // +16 Is a check box minute
                width = Math.Max(width,
                    (int) g.MeasureString(s, allowedBrigadesListView.Font).Width + DeviceCaps.GetScaledWidth(16));
            }
            allowedBrigadesDummyColumnHeader.Width = width;
            allowedBrigadesListView.EndUpdate();

            if (Game.Type == GameType.DarkestHour && Game.Version >= 103)
            {
                // Real unit type combo box
                realUnitTypeComboBox.BeginUpdate();
                realUnitTypeComboBox.Items.Clear();
                width = realUnitTypeComboBox.Width;
                foreach (RealUnitType type in Enum.GetValues(typeof (RealUnitType)))
                {
                    string s = Units.Items[(int) Units.RealTypeTable[(int) type]].ToString();
                    realUnitTypeComboBox.Items.Add(s);
                    width = Math.Max(width,
                        (int) g.MeasureString(s, realUnitTypeComboBox.Font).Width +
                        SystemInformation.VerticalScrollBarWidth + margin);
                }
                realUnitTypeComboBox.DropDownWidth = width;
                realUnitTypeComboBox.EndUpdate();

                // Sprite type combo box
                spriteTypeComboBox.BeginUpdate();
                spriteTypeComboBox.Items.Clear();
                width = spriteTypeComboBox.Width;
                foreach (SpriteType type in Enum.GetValues(typeof (SpriteType)))
                {
                    string s = Units.Items[(int) Units.SpriteTypeTable[(int) type]].ToString();
                    spriteTypeComboBox.Items.Add(s);
                    width = Math.Max(width,
                        (int) g.MeasureString(s, spriteTypeComboBox.Font).Width +
                        SystemInformation.VerticalScrollBarWidth + margin);
                }
                spriteTypeComboBox.DropDownWidth = width;
                spriteTypeComboBox.EndUpdate();

                // Alternative unit type combo box
                transmuteComboBox.BeginUpdate();
                transmuteComboBox.Items.Clear();
                width = transmuteComboBox.Width;
                foreach (UnitType type in Units.DivisionTypes)
                {
                    string s = Units.Items[(int) type].ToString();
                    transmuteComboBox.Items.Add(s);
                    width = Math.Max(width,
                        (int) g.MeasureString(s, transmuteComboBox.Font).Width +
                        SystemInformation.VerticalScrollBarWidth + margin);
                }
                transmuteComboBox.DropDownWidth = width;
                transmuteComboBox.EndUpdate();

                // Resource combo box
                resourceComboBox.BeginUpdate();
                resourceComboBox.Items.Clear();
                width = resourceComboBox.Width;
                foreach (EquipmentType type in Enum.GetValues(typeof (EquipmentType)))
                {
                    string s = Config.GetText(Units.EquipmentNames[(int) type]);
                    resourceComboBox.Items.Add(s);
                    width = Math.Max(width,
                        (int) g.MeasureString(s, resourceComboBox.Font).Width +
                        SystemInformation.VerticalScrollBarWidth + margin);
                }
                resourceComboBox.DropDownWidth = width;
                resourceComboBox.EndUpdate();
            }

            // Checkbox string
            cagCheckBox.Text = Config.GetText("NAME_CAG");
            escortCheckBox.Text = Config.GetText("NAME_ESCORT");
            engineerCheckBox.Text = Config.GetText("NAME_ENGINEER");
        }

        /// <summary>
        ///     Restrict editing items depending on the type of game
        /// </summary>
        private void RestrictEditableItems()
        {
            // AoD
            if (Game.Type == GameType.ArsenalOfDemocracy)
            {
                maxSpeedStepLabel.Enabled = true;
                maxSpeedStepComboBox.Enabled = true;
                maxSupplyStockLabel.Enabled = true;
                maxSupplyStockTextBox.Enabled = true;
                maxOilStockLabel.Enabled = true;
                maxOilStockTextBox.Enabled = true;
                artilleryBombardmentLabel.Enabled = true;
                artilleryBombardmentTextBox.Enabled = true;
            }
            else
            {
                maxSpeedStepLabel.Enabled = false;
                maxSpeedStepComboBox.Enabled = false;
                maxSupplyStockLabel.Enabled = false;
                maxSupplyStockTextBox.Enabled = false;
                maxOilStockLabel.Enabled = false;
                maxOilStockTextBox.Enabled = false;
                artilleryBombardmentLabel.Enabled = false;
                artilleryBombardmentTextBox.Enabled = false;

                maxSpeedStepComboBox.ResetText();
                maxSupplyStockTextBox.ResetText();
                maxOilStockTextBox.ResetText();
                artilleryBombardmentTextBox.ResetText();
            }

            // DH
            if (Game.Type == GameType.DarkestHour)
            {
                reinforceCostLabel.Enabled = true;
                reinforceCostTextBox.Enabled = true;
                reinforceTimeLabel.Enabled = true;
                reinforceTimeTextBox.Enabled = true;
                upgradeTimeBoostCheckBox.Enabled = true;
                autoUpgradeCheckBox.Enabled = true;
                noFuelCombatModLabel.Enabled = true;
                noFuelCombatModTextBox.Enabled = true;
                upgradeGroupBox.Enabled = true;
            }
            else
            {
                reinforceCostLabel.Enabled = false;
                reinforceCostTextBox.Enabled = false;
                reinforceTimeLabel.Enabled = false;
                reinforceTimeTextBox.Enabled = false;
                upgradeTimeBoostCheckBox.Enabled = false;
                autoUpgradeCheckBox.Enabled = false;
                autoUpgradeClassComboBox.Enabled = false;
                autoUpgradeModelComboBox.Enabled = false;
                noFuelCombatModLabel.Enabled = false;
                noFuelCombatModTextBox.Enabled = false;
                upgradeGroupBox.Enabled = false;

                maxAllowedBrigadesNumericUpDown.ResetText();
                reinforceCostTextBox.ResetText();
                reinforceTimeTextBox.ResetText();
                autoUpgradeClassComboBox.SelectedIndex = -1;
                autoUpgradeClassComboBox.ResetText();
                autoUpgradeModelComboBox.SelectedIndex = -1;
                autoUpgradeModelComboBox.ResetText();
                noFuelCombatModTextBox.ResetText();
                upgradeListView.Items.Clear();
                upgradeTypeComboBox.SelectedIndex = -1;
                upgradeTypeComboBox.ResetText();
                upgradeCostTextBox.ResetText();
                upgradeTimeTextBox.ResetText();
            }

            // DH1.03 from
            if (Game.Type == GameType.DarkestHour && Game.Version >= 103)
            {
                productableCheckBox.Enabled = true;
                cagCheckBox.Enabled = true;
                escortCheckBox.Enabled = true;
                engineerCheckBox.Enabled = true;
                eyrLabel.Enabled = true;
                eyrNumericUpDown.Enabled = true;
                gfxPrioLabel.Enabled = true;
                gfxPrioNumericUpDown.Enabled = true;
                listPrioLabel.Enabled = true;
                listPrioNumericUpDown.Enabled = true;
                uiPrioLabel.Enabled = true;
                uiPrioNumericUpDown.Enabled = true;
                realUnitTypeLabel.Enabled = true;
                realUnitTypeComboBox.Enabled = true;
                defaultTypeCheckBox.Enabled = true;
                spriteTypeLabel.Enabled = true;
                spriteTypeComboBox.Enabled = true;
                transmuteLabel.Enabled = true;
                transmuteComboBox.Enabled = true;
                militaryValueLabel.Enabled = true;
                militaryValueTextBox.Enabled = true;
                speedCapAllLabel.Enabled = true;
                speedCapAllTextBox.Enabled = true;
                equipmentGroupBox.Enabled = true;
            }
            else
            {
                productableCheckBox.Enabled = false;
                cagCheckBox.Enabled = false;
                escortCheckBox.Enabled = false;
                engineerCheckBox.Enabled = false;
                eyrLabel.Enabled = false;
                eyrNumericUpDown.Enabled = false;
                gfxPrioLabel.Enabled = false;
                gfxPrioNumericUpDown.Enabled = false;
                listPrioLabel.Enabled = false;
                listPrioNumericUpDown.Enabled = false;
                uiPrioLabel.Enabled = false;
                uiPrioNumericUpDown.Enabled = false;
                realUnitTypeLabel.Enabled = false;
                realUnitTypeComboBox.Enabled = false;
                defaultTypeCheckBox.Enabled = false;
                spriteTypeLabel.Enabled = false;
                spriteTypeComboBox.Enabled = false;
                transmuteLabel.Enabled = false;
                transmuteComboBox.Enabled = false;
                militaryValueLabel.Enabled = false;
                militaryValueTextBox.Enabled = false;
                speedCapAllLabel.Enabled = false;
                speedCapAllTextBox.Enabled = false;
                equipmentGroupBox.Enabled = false;

                productableCheckBox.Checked = false;
                detachableCheckBox.Checked = false;
                cagCheckBox.Checked = false;
                escortCheckBox.Checked = false;
                engineerCheckBox.Checked = false;
                eyrNumericUpDown.ResetText();
                gfxPrioNumericUpDown.ResetText();
                listPrioNumericUpDown.ResetText();
                uiPrioNumericUpDown.ResetText();
                realUnitTypeComboBox.SelectedIndex = -1;
                realUnitTypeComboBox.ResetText();
                spriteTypeComboBox.SelectedIndex = -1;
                spriteTypeComboBox.ResetText();
                transmuteComboBox.SelectedIndex = -1;
                transmuteComboBox.ResetText();
                militaryValueTextBox.ResetText();
                speedCapAllTextBox.ResetText();
                equipmentListView.Items.Clear();
                resourceComboBox.SelectedIndex = -1;
                resourceComboBox.ResetText();
                quantityTextBox.ResetText();
            }

            // AoD1.07 After or DH
            if (((Game.Type == GameType.ArsenalOfDemocracy) && (Game.Version >= 107)) ||
                (Game.Type == GameType.DarkestHour))
            {
                maxAllowedBrigadesLabel.Enabled = true;
                maxAllowedBrigadesNumericUpDown.Enabled = true;
            }
            else
            {
                maxAllowedBrigadesLabel.Enabled = false;
                maxAllowedBrigadesNumericUpDown.Enabled = false;
            }

            // AoD or DH1.03 from
            if (Game.Type == GameType.ArsenalOfDemocracy || (Game.Type == GameType.DarkestHour && Game.Version >= 103))
            {
                branchComboBox.Enabled = true;
                detachableCheckBox.Enabled = true;
            }
            else
            {
                branchComboBox.Enabled = false;
                detachableCheckBox.Enabled = false;
            }
        }

        #endregion

        #region Data processing

        /// <summary>
        ///     Processing after reading data
        /// </summary>
        public void OnFileLoaded()
        {
            // Since the value of the brigade that can be attached will change, cancel the selection once
            classListBox.SelectedIndex = -1;

            // Initialize the character string of the edit item of the unit model
            InitModelItemText();

            // Initialize edit items
            InitEditableItems();

            // Restrict editing items depending on the type of game
            RestrictEditableItems();

            // Update the unit list
            UpdateUnitList();
        }

        /// <summary>
        ///     Processing after data storage
        /// </summary>
        public void OnFileSaved()
        {
            // Update the display as the edited flag is cleared
            classListBox.Refresh();
            modelListView.Refresh();
            UpdateClassEditableItems();
            UpdateModelEditableItems();
        }

        /// <summary>
        ///     Processing after changing edit items
        /// </summary>
        /// <param name="id">Edit items ID</param>
        public void OnItemChanged(EditorItemId id)
        {
            switch (id)
            {
                case EditorItemId.MaxAllowedBrigades:
                    Log.Verbose("[Unit] Notified max allowed brigades");
                    // Update the display of the maximum number of attached brigades
                    UpdateMaxAllowedBrigades();
                    break;

                case EditorItemId.CommonModelName:
                    Log.Verbose("[Unit] Notified common model name");
                    // Update the model name in the unit model list
                    UpdateModelListName();
                    // Update the display of the unit model name
                    UpdateModelNameTextBox();
                    break;

                case EditorItemId.CountryModelName:
                    Log.Verbose("[Unit] Notified country model name");
                    // Update the model name in the unit model list
                    UpdateModelListName();
                    // Update the display of the unit model name
                    UpdateModelNameTextBox();
                    break;
            }
        }

        #endregion

        #region Form

        /// <summary>
        ///     Form initialization
        /// </summary>
        private void InitForm()
        {
            // Unit model list view
            noColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.ModelListColumnWidth[0];
            nameColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.ModelListColumnWidth[1];
            buildCostColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.ModelListColumnWidth[2];
            buildTimeColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.ModelListColumnWidth[3];
            manpowerColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.ModelListColumnWidth[4];
            supplyColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.ModelListColumnWidth[5];
            fuelColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.ModelListColumnWidth[6];
            organisationColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.ModelListColumnWidth[7];
            moraleColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.ModelListColumnWidth[8];
            maxSpeedColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.ModelListColumnWidth[9];

            // Unit class list box
            classListBox.ItemHeight = DeviceCaps.GetScaledHeight(classListBox.ItemHeight);

            // National list view
            countryDummyColumnHeader.Width = DeviceCaps.GetScaledWidth(countryDummyColumnHeader.Width);

            // Improved list view
            upgradeTypeColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.UpgradeListColumnWidth[0];
            upgradeCostColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.UpgradeListColumnWidth[1];
            upgradeTimeColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.UpgradeListColumnWidth[2];

            // Equipment list view
            resourceColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.EquipmentListColumnWidth[0];
            quantityColumnHeader.Width = HoI2EditorController.Settings.UnitEditor.EquipmentListColumnWidth[1];


            // Window position
            Location = HoI2EditorController.Settings.UnitEditor.Location;
            Size = HoI2EditorController.Settings.UnitEditor.Size;
        }

        /// <summary>
        ///     Processing when loading a form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormLoad(object sender, EventArgs e)
        {
            // Load the game settings file
            Misc.Load();

            // Initialize national data
            Countries.Init();

            // Initialize unit data
            Units.Init();

            // Misc Read the file
            Misc.Load();

            // Read the character string definition file
            Config.Load();

            // Read unit data
            Units.Load();

            // Processing after reading data
            OnFileLoaded();
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
            HoI2EditorController.OnUnitEditorFormClosed();
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
                HoI2EditorController.Settings.UnitEditor.Location = Location;
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
                HoI2EditorController.Settings.UnitEditor.Size = Size;
            }
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

        #endregion

        #region Unit class list

        /// <summary>
        ///     Update the unit class list
        /// </summary>
        private void UpdateUnitList()
        {
            // Register an item in the list box
            classListBox.BeginUpdate();
            classListBox.Items.Clear();
            foreach (UnitType type in Units.UnitTypes)
            {
                UnitClass unit = Units.Items[(int) type];
                classListBox.Items.Add(unit);
            }
            classListBox.EndUpdate();

            // Select the first item
            if (classListBox.Items.Count > 0)
            {
                classListBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        ///     Item drawing process of unit class list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClassListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            UnitClass unit = Units.Items[(int) Units.UnitTypes[e.Index]];

            // Draw the background
            e.DrawBackground();
            if (((e.State & DrawItemState.Selected) == 0) && (unit.Models.Count > 0))
            {
                e.Graphics.FillRectangle(
                    unit.Organization == UnitOrganization.Division ? Brushes.AliceBlue : Brushes.Honeydew,
                    new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
            }

            // Draw an item
            Brush brush;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // Change the text color for items that have changed
                brush = unit.IsDirty() ? new SolidBrush(Color.Red) : new SolidBrush(classListBox.ForeColor);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = classListBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the unit class list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClassListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the unit model list
            UpdateModelList();

            // Update unit class edits
            UpdateClassEditableItems();

            // When editing a unit model
            if (editTabControl.SelectedIndex == (int) UnitEditorTab.Model)
            {
                if (modelListView.Items.Count > 0)
                {
                    // Select the first item
                    modelListView.Items[0].Focused = true;
                    modelListView.Items[0].Selected = true;
                }
                else
                {
                    // Select the Unit Class tab
                    editTabControl.SelectedIndex = (int) UnitEditorTab.Class;

                    // Disable edit items in the unit model
                    DisableModelEditableItems();
                }
            }
            else
            {
                // Disable edit items
                DisableModelEditableItems();
            }
        }

        #endregion

        #region Unit model list

        /// <summary>
        ///     Update the display of the unit model list
        /// </summary>
        private void UpdateModelList()
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Register an item in the list view
            modelListView.BeginUpdate();
            modelListView.Items.Clear();
            for (int i = 0; i < unit.Models.Count; i++)
            {
                ListViewItem item = CreateModelListItem(unit, i);
                modelListView.Items.Add(item);
            }
            modelListView.EndUpdate();
        }

        /// <summary>
        ///     Update the model name in the unit model list
        /// </summary>
        private void UpdateModelListName()
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Update items in list view
            Country country = GetSelectedCountry();
            modelListView.BeginUpdate();
            for (int i = 0; i < unit.Models.Count; i++)
            {
                string name = unit.GetCountryModelName(i, country);
                if (string.IsNullOrEmpty(name))
                {
                    name = unit.GetModelName(i);
                }
                modelListView.Items[i].SubItems[1].Text = name;
            }
            modelListView.EndUpdate();
        }

        /// <summary>
        ///     Processing when changing the selection item in the unit model list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModelListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // If there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                // Disable edit items
                DisableModelEditableItems();
                return;
            }

            // Enable edit items for the unit model
            EnableModelEditableItems();

            // Update the value of the edit item of the unit model
            UpdateModelEditableItems();

            // Select the Unit Model tab
            editTabControl.SelectedIndex = (int) UnitEditorTab.Model;

            // Item move button status update
            int index = modelListView.SelectedIndices[0];
            topButton.Enabled = index != 0;
            upButton.Enabled = index != 0;
            downButton.Enabled = index != modelListView.Items.Count - 1;
            bottomButton.Enabled = index != modelListView.Items.Count - 1;
        }

        /// <summary>
        ///     Processing when changing the width of columns in the unit model list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModelListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < ModelListColumnCount))
            {
                HoI2EditorController.Settings.UnitEditor.ModelListColumnWidth[e.ColumnIndex] =
                    modelListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Processing before editing items in the unit model list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModelListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 1: // name
                    e.Type = ItemEditType.Text;
                    e.Text = modelNameTextBox.Text;
                    break;

                case 2: // I C
                    e.Type = ItemEditType.Text;
                    e.Text = costTextBox.Text;
                    break;

                case 3: // time
                    e.Type = ItemEditType.Text;
                    e.Text = buildTimeTextBox.Text;
                    break;

                case 4: // Labor force
                    e.Type = ItemEditType.Text;
                    e.Text = manPowerTextBox.Text;
                    break;

                case 5: // Supplies
                    e.Type = ItemEditType.Text;
                    e.Text = supplyConsumptionTextBox.Text;
                    break;

                case 6: // fuel
                    e.Type = ItemEditType.Text;
                    e.Text = fuelConsumptionTextBox.Text;
                    break;

                case 7: // Organization rate
                    e.Type = ItemEditType.Text;
                    e.Text = defaultOrganisationTextBox.Text;
                    break;

                case 8: // morale
                    e.Type = ItemEditType.Text;
                    e.Text = moraleTextBox.Text;
                    break;

                case 9: // speed
                    e.Type = ItemEditType.Text;
                    e.Text = maxSpeedTextBox.Text;
                    break;
            }
        }

        /// <summary>
        ///     Processing after editing items in the unit model list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModelListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 1: // name
                    modelNameTextBox.Text = e.Text;
                    break;

                case 2: // I C
                    costTextBox.Text = e.Text;
                    OnCostTextBoxValidated(costTextBox, new EventArgs());
                    break;

                case 3: // time
                    buildTimeTextBox.Text = e.Text;
                    OnBuildTimeTextBoxValidated(buildTimeTextBox, new EventArgs());
                    break;

                case 4: // Labor force
                    manPowerTextBox.Text = e.Text;
                    OnManPowerTextBoxValidated(manPowerTextBox, new EventArgs());
                    break;

                case 5: // Supplies
                    supplyConsumptionTextBox.Text = e.Text;
                    OnSupplyConsumptionTextBoxValidated(supplyConsumptionTextBox, new EventArgs());
                    break;

                case 6: // fuel
                    fuelConsumptionTextBox.Text = e.Text;
                    OnFuelConsumptionTextBoxValidated(fuelConsumptionTextBox, new EventArgs());
                    break;

                case 7: // Organization rate
                    defaultOrganisationTextBox.Text = e.Text;
                    OnDefaultOrganizationTextBoxValidated(defaultOrganisationTextBox, new EventArgs());
                    break;

                case 8: // morale
                    moraleTextBox.Text = e.Text;
                    OnMoraleTextBoxValidated(moraleTextBox, new EventArgs());
                    break;

                case 9: // speed
                    maxSpeedTextBox.Text = e.Text;
                    OnMaxSpeedTextBoxValidated(maxSpeedTextBox, new EventArgs());
                    break;
            }

            // Since the items in the list view will be updated by yourself, it will be treated as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Processing when replacing items in the unit model list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModelListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // I will replace the items on my own, so I will treat it as canceled
            e.Cancel = true;

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;
            if (srcIndex < destIndex)
            {
                destIndex--;
            }

            // Move the unit model
            MoveModel(unit, srcIndex, destIndex);

            // Notify the update of the unit model list
            HoI2EditorController.OnItemChanged(EditorItemId.ModelList, this);
        }

        /// <summary>
        ///     Create an item in the unit model list view
        /// </summary>
        /// <param name="unit">Unit class</param>
        /// <param name="index">Unit model index</param>
        /// <returns>Items in the unit model list view</returns>
        private ListViewItem CreateModelListItem(UnitClass unit, int index)
        {
            UnitModel model = unit.Models[index];

            ListViewItem item = new ListViewItem { Text = IntHelper.ToString(index) };
            string name = unit.GetCountryModelName(index, GetSelectedCountry());
            if (string.IsNullOrEmpty(name))
            {
                name = unit.GetModelName(index);
            }
            item.SubItems.Add(name);
            item.SubItems.Add(DoubleHelper.ToString(model.Cost));
            item.SubItems.Add(DoubleHelper.ToString(model.BuildTime));
            item.SubItems.Add(DoubleHelper.ToString(model.ManPower));
            item.SubItems.Add(DoubleHelper.ToString(model.SupplyConsumption));
            item.SubItems.Add(DoubleHelper.ToString(model.FuelConsumption));
            item.SubItems.Add(DoubleHelper.ToString(model.DefaultOrganization));
            item.SubItems.Add(DoubleHelper.ToString(model.Morale));
            item.SubItems.Add(DoubleHelper.ToString(model.MaxSpeed));

            return item;
        }

        /// <summary>
        ///     Processing when a new button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Insert the unit model
            UnitModel model = new UnitModel();
            int index = modelListView.SelectedIndices.Count > 0 ? modelListView.SelectedIndices[0] + 1 : 0;
            InsertModel(unit, model, index, "");

            // Notify the update of the unit model list
            HoI2EditorController.OnItemChanged(EditorItemId.ModelList, this);
        }

        /// <summary>
        ///     Processing when the duplicate button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloneButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];

            // Insert the unit model
            UnitModel model = new UnitModel(unit.Models[index]);
            InsertModel(unit, model, index + 1, unit.GetModelName(index));

            // Notify the update of the unit model list
            HoI2EditorController.OnItemChanged(EditorItemId.ModelList, this);
        }

        /// <summary>
        ///     Processing when the delete button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];

            // Delete the unit model
            RemoveModel(unit, index);

            // Notify the update of the unit model list
            HoI2EditorController.OnItemChanged(EditorItemId.ModelList, this);
        }

        /// <summary>
        ///     Processing when the button is pressed to the beginning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTopButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];

            // Do nothing at the top of the list
            if (index == 0)
            {
                return;
            }

            // Move the unit model
            MoveModel(unit, index, 0);

            // Notify the update of the unit model list
            HoI2EditorController.OnItemChanged(EditorItemId.ModelList, this);
        }

        /// <summary>
        ///     Processing when pressing the up button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];

            // Do nothing at the top of the list
            if (index == 0)
            {
                return;
            }

            // Move the unit model
            MoveModel(unit, index, index - 1);

            // Notify the update of the unit model list
            HoI2EditorController.OnItemChanged(EditorItemId.ModelList, this);
        }

        /// <summary>
        ///     Processing when the down button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];

            // Do nothing at the end of the list
            if (index == unit.Models.Count - 1)
            {
                return;
            }

            // Move the unit model
            MoveModel(unit, index, index + 1);

            // Notify the update of the unit model list
            HoI2EditorController.OnItemChanged(EditorItemId.ModelList, this);
        }

        /// <summary>
        ///     Processing when the button is pressed to the end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBottonButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];

            // Do nothing at the end of the list
            if (index == unit.Models.Count - 1)
            {
                return;
            }

            // Move the unit model
            MoveModel(unit, index, unit.Models.Count - 1);

            // Notify the update of the unit model list
            HoI2EditorController.OnItemChanged(EditorItemId.ModelList, this);
        }

        /// <summary>
        ///     Insert the unit model
        /// </summary>
        /// <param name="unit">Unit class</param>
        /// <param name="model">Unit model to be inserted</param>
        /// <param name="index">Position to insert</param>
        /// <param name="name">Unit model name</param>
        private void InsertModel(UnitClass unit, UnitModel model, int index, string name)
        {
            // Insert a unit model into a unit class
            unit.InsertModel(model, index, name);

            // Update the display of the unit model list
            UpdateModelList();

            // Select the inserted item
            modelListView.Items[index].Focused = true;
            modelListView.Items[index].Selected = true;

            // Make the inserted item visible
            modelListView.EnsureVisible(index);
        }

        /// <summary>
        ///     Delete the unit model
        /// </summary>
        /// <param name="unit">Unit class</param>
        /// <param name="index">Position to delete</param>
        private void RemoveModel(UnitClass unit, int index)
        {
            // Remove the unit model from the unit class
            unit.RemoveModel(index);

            // Update the display of the unit model list
            UpdateModelList();

            // Select the next item after the deleted item
            if (index < modelListView.Items.Count - 1)
            {
                modelListView.Items[index].Focused = true;
                modelListView.Items[index].Selected = true;
            }
            else if (modelListView.Items.Count > 0)
            {
                modelListView.Items[index - 1].Focused = true;
                modelListView.Items[index - 1].Selected = true;
            }
        }

        /// <summary>
        ///     Move the unit model
        /// </summary>
        /// <param name="unit">Unit class</param>
        /// <param name="src">Source position</param>
        /// <param name="dest">Destination position</param>
        private void MoveModel(UnitClass unit, int src, int dest)
        {
            // Move the unit model of the unit class
            unit.MoveModel(src, dest);

            // Update the display of the unit model list
            UpdateModelList();

            // Select the item to move to
            modelListView.Items[dest].Focused = true;
            modelListView.Items[dest].Selected = true;

            // Display the item to move to
            modelListView.EnsureVisible(dest);
        }

        #endregion

        #region National list view

        /// <summary>
        ///     Get the selected country tag
        /// </summary>
        /// <returns></returns>
        private Country GetSelectedCountry()
        {
            return countryListView.SelectedIndices.Count > 0
                ? Countries.Tags[countryListView.SelectedIndices[0]]
                : Country.None;
        }

        /// <summary>
        ///     Processing when changing the selection item in the national list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Update the model name in the unit model list
            UpdateModelListName();

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];

            // Update the unit model image name
            Image prev = modelImagePictureBox.Image;
            string fileName = GetModelImageFileName(unit, index, GetSelectedCountry());
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                Bitmap bitmap = new Bitmap(fileName);
                bitmap.MakeTransparent(Color.Lime);
                modelImagePictureBox.Image = bitmap;
            }
            else
            {
                modelImagePictureBox.Image = null;
            }
            prev?.Dispose();

            // Update the unit model name
            UpdateModelNameTextBox();
        }

        #endregion

        #region Unit class tab

        /// <summary>
        ///     Update the edit items on the unit class tab
        /// </summary>
        private void UpdateClassEditableItems()
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            classNameTextBox.Text = Config.ExistsKey(unit.Name) ? Config.GetText(unit.Name) : "";
            classShortNameTextBox.Text = unit.GetShortName();
            classDescTextBox.Text = unit.GetDesc();
            classShortDescTextBox.Text = unit.GetShortDesc();

            // Army
            branchComboBox.SelectedIndex = (int) unit.Branch - 1;

            // Attached brigade
            if (unit.Organization == UnitOrganization.Division)
            {
                if (unit.CanModifyMaxAllowedBrigades())
                {
                    maxAllowedBrigadesLabel.Enabled = true;
                    maxAllowedBrigadesNumericUpDown.Enabled = true;
                }
                else
                {
                    maxAllowedBrigadesLabel.Enabled = false;
                    maxAllowedBrigadesNumericUpDown.Enabled = false;
                }
                maxAllowedBrigadesNumericUpDown.Value = unit.GetMaxAllowedBrigades();
                maxAllowedBrigadesNumericUpDown.Text = IntHelper.ToString((int) maxAllowedBrigadesNumericUpDown.Value);

                Graphics g = Graphics.FromHwnd(allowedBrigadesListView.Handle);
                int width = DeviceCaps.GetScaledWidth(60);
                allowedBrigadesListView.ItemChecked -= OnAllowedBrigadesListViewItemChecked;
                allowedBrigadesListView.Enabled = true;
                allowedBrigadesListView.BeginUpdate();
                allowedBrigadesListView.Items.Clear();
                foreach (UnitClass brigade in Units.BrigadeTypes
                    .Select(type => Units.Items[(int) type])
                    .Where(brigade => (brigade.Branch == unit.Branch) && (brigade.Models.Count > 0)))
                {
                    string s = brigade.ToString();
                    // +16 Is a check box minute
                    width = Math.Max(width,
                        (int) g.MeasureString(s, allowedBrigadesListView.Font).Width + DeviceCaps.GetScaledWidth(16));
                    ListViewItem item = new ListViewItem
                    {
                        Text = s,
                        Checked = unit.AllowedBrigades.Contains(brigade.Type),
                        ForeColor = unit.IsDirtyAllowedBrigades(brigade.Type) ? Color.Red : SystemColors.WindowText,
                        Tag = brigade
                    };
                    allowedBrigadesListView.Items.Add(item);
                }
                allowedBrigadesDummyColumnHeader.Width = width;
                allowedBrigadesListView.EndUpdate();
                allowedBrigadesListView.ItemChecked += OnAllowedBrigadesListViewItemChecked;
            }
            else
            {
                maxAllowedBrigadesLabel.Enabled = false;
                maxAllowedBrigadesNumericUpDown.Enabled = false;
                maxAllowedBrigadesNumericUpDown.ResetText();

                allowedBrigadesListView.Enabled = false;
                allowedBrigadesListView.BeginUpdate();
                allowedBrigadesListView.Items.Clear();
                allowedBrigadesListView.EndUpdate();
            }

            // DH1.03 Subsequent unit settings
            if ((Game.Type == GameType.DarkestHour) && (Game.Version >= 103))
            {
                listPrioLabel.Enabled = true;
                listPrioNumericUpDown.Enabled = true;
                listPrioNumericUpDown.Value = unit.ListPrio;
                listPrioNumericUpDown.Text = IntHelper.ToString(unit.ListPrio);

                // Division
                if (unit.Organization == UnitOrganization.Division)
                {
                    eyrLabel.Enabled = true;
                    eyrNumericUpDown.Enabled = true;
                    gfxPrioLabel.Enabled = true;
                    gfxPrioNumericUpDown.Enabled = true;
                    uiPrioLabel.Enabled = true;
                    uiPrioNumericUpDown.Enabled = true;
                    realUnitTypeLabel.Enabled = true;
                    realUnitTypeComboBox.Enabled = true;
                    spriteTypeLabel.Enabled = true;
                    spriteTypeComboBox.Enabled = true;
                    transmuteLabel.Enabled = true;
                    transmuteComboBox.Enabled = true;
                    militaryValueLabel.Enabled = true;
                    militaryValueTextBox.Enabled = true;
                    defaultTypeCheckBox.Enabled = true;
                    productableCheckBox.Enabled = true;

                    eyrNumericUpDown.Value = unit.Eyr;
                    eyrNumericUpDown.Text = IntHelper.ToString((int) eyrNumericUpDown.Value);
                    gfxPrioNumericUpDown.Value = unit.GfxPrio;
                    gfxPrioNumericUpDown.Text = IntHelper.ToString((int) gfxPrioNumericUpDown.Value);
                    uiPrioNumericUpDown.Value = unit.UiPrio;
                    uiPrioNumericUpDown.Text = IntHelper.ToString((int) uiPrioNumericUpDown.Value);
                    realUnitTypeComboBox.SelectedIndex = (int) unit.RealType;
                    spriteTypeComboBox.SelectedIndex = (int) unit.Sprite;
                    transmuteComboBox.SelectedIndex = Array.IndexOf(Units.DivisionTypes, unit.Transmute);
                    militaryValueTextBox.Text = DoubleHelper.ToString(unit.Value);
                }
                else
                {
                    eyrLabel.Enabled = false;
                    eyrNumericUpDown.Enabled = false;
                    gfxPrioLabel.Enabled = false;
                    gfxPrioNumericUpDown.Enabled = false;
                    uiPrioLabel.Enabled = false;
                    uiPrioNumericUpDown.Enabled = false;
                    realUnitTypeLabel.Enabled = false;
                    realUnitTypeComboBox.Enabled = false;
                    spriteTypeLabel.Enabled = false;
                    spriteTypeComboBox.Enabled = false;
                    transmuteLabel.Enabled = false;
                    transmuteComboBox.Enabled = false;
                    militaryValueLabel.Enabled = false;
                    militaryValueTextBox.Enabled = false;
                    defaultTypeCheckBox.Enabled = false;
                    productableCheckBox.Enabled = false;

                    eyrNumericUpDown.ResetText();
                    gfxPrioNumericUpDown.ResetText();
                    uiPrioNumericUpDown.ResetText();
                    realUnitTypeComboBox.SelectedIndex = -1;
                    realUnitTypeComboBox.ResetText();
                    spriteTypeComboBox.SelectedIndex = -1;
                    spriteTypeComboBox.ResetText();
                    transmuteComboBox.SelectedIndex = -1;
                    transmuteComboBox.ResetText();
                    militaryValueTextBox.ResetText();
                }

                // Army Brigade
                if ((unit.Branch == Branch.Army) && (unit.Organization == UnitOrganization.Brigade))
                {
                    engineerCheckBox.Enabled = true;
                }
                else
                {
                    engineerCheckBox.Enabled = false;
                }

                // Navy brigade
                if ((unit.Branch == Branch.Navy) && (unit.Organization == UnitOrganization.Brigade))
                {
                    cagCheckBox.Enabled = true;
                }
                else
                {
                    cagCheckBox.Enabled = false;
                }

                // Air Force Brigade
                if ((unit.Branch == Branch.Airforce) && (unit.Organization == UnitOrganization.Brigade))
                {
                    escortCheckBox.Enabled = true;
                }
                else
                {
                    escortCheckBox.Enabled = false;
                }
            }
            else
            {
                eyrLabel.Enabled = false;
                eyrNumericUpDown.Enabled = false;
                gfxPrioLabel.Enabled = false;
                gfxPrioNumericUpDown.Enabled = false;
                listPrioLabel.Enabled = false;
                listPrioNumericUpDown.Enabled = false;
                uiPrioLabel.Enabled = false;
                uiPrioNumericUpDown.Enabled = false;
                realUnitTypeLabel.Enabled = false;
                realUnitTypeComboBox.Enabled = false;
                spriteTypeLabel.Enabled = false;
                spriteTypeComboBox.Enabled = false;
                transmuteLabel.Enabled = false;
                transmuteComboBox.Enabled = false;
                militaryValueLabel.Enabled = false;
                militaryValueTextBox.Enabled = false;
                cagCheckBox.Enabled = false;
                escortCheckBox.Enabled = false;
                engineerCheckBox.Enabled = false;
                defaultTypeCheckBox.Enabled = false;
                productableCheckBox.Enabled = false;

                eyrNumericUpDown.ResetText();
                gfxPrioNumericUpDown.ResetText();
                listPrioNumericUpDown.ResetText();
                uiPrioNumericUpDown.ResetText();
                realUnitTypeComboBox.SelectedIndex = -1;
                realUnitTypeComboBox.ResetText();
                spriteTypeComboBox.SelectedIndex = -1;
                spriteTypeComboBox.ResetText();
                transmuteComboBox.SelectedIndex = -1;
                transmuteComboBox.ResetText();
                militaryValueTextBox.ResetText();
            }

            // Maximum production speed
            if ((Game.Type == GameType.ArsenalOfDemocracy) && (unit.Organization == UnitOrganization.Division))
            {
                maxSpeedStepLabel.Enabled = true;
                maxSpeedStepComboBox.Enabled = true;
                maxSpeedStepComboBox.SelectedIndex = unit.MaxSpeedStep;
            }
            else
            {
                maxSpeedStepLabel.Enabled = false;
                maxSpeedStepComboBox.Enabled = false;
                maxSpeedStepComboBox.SelectedIndex = -1;
                maxSpeedStepComboBox.ResetText();
            }

            // Detachable
            if (Game.Type == GameType.ArsenalOfDemocracy)
            {
                detachableCheckBox.Enabled = unit.Organization == UnitOrganization.Brigade;
            }
            else if ((Game.Type == GameType.DarkestHour) && (Game.Version >= 103))
            {
                detachableCheckBox.Enabled = (unit.Branch == Branch.Navy) &&
                                             (unit.Organization == UnitOrganization.Brigade);
            }
            else
            {
                detachableCheckBox.Enabled = false;
            }

            detachableCheckBox.Checked = detachableCheckBox.Enabled && unit.Detachable;
            cagCheckBox.Checked = cagCheckBox.Enabled && unit.Cag;
            escortCheckBox.Checked = escortCheckBox.Enabled && unit.Escort;
            engineerCheckBox.Checked = engineerCheckBox.Enabled && unit.Engineer;
            defaultTypeCheckBox.Checked = unit.DefaultType;
            productableCheckBox.Checked = unit.Productable;

            // Improvement
            if ((Game.Type == GameType.DarkestHour) && (Game.Version >= 103) &&
                (unit.Organization == UnitOrganization.Division))
            {
                upgradeGroupBox.Enabled = true;
                UpdateUpgradeList(unit);
                UpdateUpgradeTypeComboBox();
                const string def = "0";
                upgradeCostTextBox.Text = def;
                upgradeTimeTextBox.Text = def;
            }
            else
            {
                upgradeGroupBox.Enabled = false;
                // Clear the value of the edit item
                upgradeListView.BeginUpdate();
                upgradeListView.Items.Clear();
                upgradeListView.EndUpdate();
                upgradeTypeComboBox.BeginUpdate();
                upgradeTypeComboBox.Items.Clear();
                upgradeTypeComboBox.EndUpdate();
                upgradeCostTextBox.ResetText();
                upgradeTimeTextBox.ResetText();
            }

            // Set the color of the edit item
            classNameTextBox.ForeColor = unit.IsDirty(UnitClassItemId.Name) ? Color.Red : SystemColors.WindowText;
            classShortNameTextBox.ForeColor = unit.IsDirty(UnitClassItemId.ShortName)
                ? Color.Red
                : SystemColors.WindowText;
            classDescTextBox.ForeColor = unit.IsDirty(UnitClassItemId.Desc) ? Color.Red : SystemColors.WindowText;
            classShortDescTextBox.ForeColor = unit.IsDirty(UnitClassItemId.ShortDesc)
                ? Color.Red
                : SystemColors.WindowText;
            eyrNumericUpDown.ForeColor = unit.IsDirty(UnitClassItemId.Eyr) ? Color.Red : SystemColors.WindowText;
            gfxPrioNumericUpDown.ForeColor = unit.IsDirty(UnitClassItemId.GfxPrio) ? Color.Red : SystemColors.WindowText;
            listPrioNumericUpDown.ForeColor = unit.IsDirty(UnitClassItemId.ListPrio)
                ? Color.Red
                : SystemColors.WindowText;
            uiPrioNumericUpDown.ForeColor = unit.IsDirty(UnitClassItemId.UiPrio) ? Color.Red : SystemColors.WindowText;
            militaryValueTextBox.ForeColor = unit.IsDirty(UnitClassItemId.Vaule) ? Color.Red : SystemColors.WindowText;
            detachableCheckBox.ForeColor = unit.IsDirty(UnitClassItemId.Detachable)
                ? Color.Red
                : SystemColors.WindowText;
            cagCheckBox.ForeColor = unit.IsDirty(UnitClassItemId.Cag) ? Color.Red : SystemColors.WindowText;
            escortCheckBox.ForeColor = unit.IsDirty(UnitClassItemId.Escort) ? Color.Red : SystemColors.WindowText;
            engineerCheckBox.ForeColor = unit.IsDirty(UnitClassItemId.Engineer) ? Color.Red : SystemColors.WindowText;
            defaultTypeCheckBox.ForeColor = unit.IsDirty(UnitClassItemId.DefaultType)
                ? Color.Red
                : SystemColors.WindowText;
            productableCheckBox.ForeColor = unit.IsDirty(UnitClassItemId.Productable)
                ? Color.Red
                : SystemColors.WindowText;
            maxAllowedBrigadesNumericUpDown.ForeColor =
                unit.IsDirty(UnitClassItemId.MaxAllowedBrigades) ? Color.Red : SystemColors.WindowText;

            if ((Game.Type == GameType.DarkestHour) && (Game.Version >= 103))
            {
                upgradeCostTextBox.ForeColor = SystemColors.WindowText;
                upgradeTimeTextBox.ForeColor = SystemColors.WindowText;
            }
        }

        /// <summary>
        ///     Update the maximum number of attached brigades
        /// </summary>
        private void UpdateMaxAllowedBrigades()
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // If the selected unit class is a brigade, do nothing
            if (unit.Organization == UnitOrganization.Brigade)
            {
                return;
            }

            maxAllowedBrigadesNumericUpDown.Value = unit.GetMaxAllowedBrigades();
            maxAllowedBrigadesNumericUpDown.Text = IntHelper.ToString((int) maxAllowedBrigadesNumericUpDown.Value);

            maxAllowedBrigadesNumericUpDown.ForeColor =
                unit.IsDirty(UnitClassItemId.MaxAllowedBrigades) ? Color.Red : SystemColors.WindowText;
        }

        /// <summary>
        ///     Item drawing process of the military combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBranchComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Brush brush;
            if ((e.Index == (int) unit.Branch - 1) && unit.IsDirty(UnitClassItemId.Branch))
            {
                brush = new SolidBrush(Color.Red);
            }
            else
            {
                brush = new SolidBrush(SystemColors.WindowText);
            }
            string s = branchComboBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of real unit type combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRealUnitTypeComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Brush brush;
            if ((e.Index == (int) unit.RealType) && unit.IsDirty(UnitClassItemId.RealType))
            {
                brush = new SolidBrush(Color.Red);
            }
            else
            {
                brush = new SolidBrush(SystemColors.WindowText);
            }
            string s = realUnitTypeComboBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Sprite type Combo box item drawing process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpriteTypeComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Brush brush;
            if ((e.Index == (int) unit.Sprite) && unit.IsDirty(UnitClassItemId.Sprite))
            {
                brush = new SolidBrush(Color.Red);
            }
            else
            {
                brush = new SolidBrush(SystemColors.WindowText);
            }
            string s = spriteTypeComboBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of alternative unit combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTransmuteComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            UnitType type = Units.DivisionTypes[e.Index];
            Brush brush;
            if ((type == unit.Transmute) && unit.IsDirty(UnitClassItemId.Transmute))
            {
                brush = new SolidBrush(Color.Red);
            }
            else
            {
                brush = new SolidBrush(SystemColors.WindowText);
            }
            string s = transmuteComboBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of maximum production speed combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxSpeedStepComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Brush brush;
            if ((e.Index == unit.MaxSpeedStep) && unit.IsDirty(UnitClassItemId.MaxSpeedStep))
            {
                brush = new SolidBrush(Color.Red);
            }
            else
            {
                brush = new SolidBrush(SystemColors.WindowText);
            }
            string s = maxSpeedStepComboBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Unit class name Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClassNameTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (Config.ExistsKey(unit.Name))
            {
                if (classNameTextBox.Text.Equals(Config.GetText(unit.Name)))
                {
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(classNameTextBox.Text))
                {
                    return;
                }
            }

            Log.Info("[Unit] unit name: {0} -> {1}", Config.GetText(unit.Name), classNameTextBox.Text);

            // Update value
            Config.SetText(unit.Name, classNameTextBox.Text, Game.UnitTextFileName);

            // Update the display of the unit class list box
            classListBox.Refresh();

            if (unit.Organization == UnitOrganization.Division)
            {
                Graphics g = Graphics.FromHwnd(Handle);
                int margin = DeviceCaps.GetScaledWidth(2) + 1;

                if ((Game.Type == GameType.DarkestHour) && (Game.Version >= 103))
                {
                    // Update the item of the real unit combo box
                    int index = Array.IndexOf(Units.RealTypeTable, unit.Type);
                    if (index >= 0)
                    {
                        realUnitTypeComboBox.Items[index] = classNameTextBox.Text;
                        // Update dropdown width
                        realUnitTypeComboBox.DropDownWidth =
                            Math.Max(realUnitTypeComboBox.DropDownWidth,
                                (int) g.MeasureString(classNameTextBox.Text, realUnitTypeComboBox.Font).Width +
                                SystemInformation.VerticalScrollBarWidth + margin);
                    }

                    // Update items in the sprite combo box
                    index = Array.IndexOf(Units.SpriteTypeTable, unit.Type);
                    if (index >= 0)
                    {
                        spriteTypeComboBox.Items[index] = classNameTextBox.Text;
                        // Update dropdown width
                        spriteTypeComboBox.DropDownWidth =
                            Math.Max(spriteTypeComboBox.DropDownWidth,
                                (int) g.MeasureString(classNameTextBox.Text, spriteTypeComboBox.Font).Width +
                                SystemInformation.VerticalScrollBarWidth + margin);
                    }

                    // Update the entry in the alternate unit combo box
                    transmuteComboBox.Items[classListBox.SelectedIndex] = classNameTextBox.Text;
                    // Update dropdown width
                    transmuteComboBox.DropDownWidth =
                        Math.Max(transmuteComboBox.DropDownWidth,
                            (int) g.MeasureString(classNameTextBox.Text, transmuteComboBox.Font).Width +
                            SystemInformation.VerticalScrollBarWidth + margin);
                }
            }

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Name);

            // Change the font color
            classNameTextBox.ForeColor = Color.Red;

            // Notify the update of the unit class name
            HoI2EditorController.OnItemChanged(EditorItemId.UnitName, this);
        }

        /// <summary>
        ///     Unit class Short name Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClassShortNameTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (classShortNameTextBox.Text.Equals(unit.GetShortName()))
            {
                return;
            }

            Log.Info("[Unit] unit short name: {0} -> {1} ({2})", unit.GetShortName(), classShortNameTextBox.Text, unit);

            // Update value
            Config.SetText(unit.ShortName, classShortNameTextBox.Text, Game.UnitTextFileName);

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.ShortName);

            // Change the font color
            classShortNameTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Unit class Description Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClassDescTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (classDescTextBox.Text.Equals(Config.GetText(unit.Desc)))
            {
                return;
            }

            Log.Info("[Unit] unit desc: {0} -> {1} ({2})", unit.GetDesc(), classDescTextBox.Text, unit);

            // Update value
            Config.SetText(unit.Desc, classDescTextBox.Text, Game.UnitTextFileName);

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Desc);

            // Change the font color
            classDescTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Unit class abbreviated description Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClassShortDescTextBox(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (classShortDescTextBox.Text.Equals(Config.GetText(unit.ShortDesc)))
            {
                return;
            }

            Log.Info("[Unit] unit short desc: {0} -> {1} ({2})", unit.GetShortDesc(), classShortDescTextBox.Text, unit);

            // Update value
            Config.SetText(unit.ShortDesc, classShortDescTextBox.Text, Game.UnitTextFileName);

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.ShortDesc);

            // Change the font color
            classShortDescTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the selection item of the military combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBranchComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            Branch branch = (Branch) (branchComboBox.SelectedIndex + 1);
            if (branch == unit.Branch)
            {
                return;
            }

            Log.Info("[Unit] branch: {0} -> {1} ({2})", Branches.GetName(unit.Branch), Branches.GetName(branch), unit);

            // Update value
            unit.Branch = branch;

            // DH1.03 If you change the division's military department after that, the actual unit type will also be linked.
            if ((Game.Type == GameType.DarkestHour) && (Game.Version >= 103) &&
                (unit.Organization == UnitOrganization.Division))
            {
                RealUnitType type;
                switch (branch)
                {
                    case Branch.Army:
                        type = RealUnitType.Infantry;
                        break;

                    case Branch.Navy:
                        type = RealUnitType.Destroyer;
                        break;

                    case Branch.Airforce:
                        type = RealUnitType.Interceptor;
                        break;

                    default:
                        type = RealUnitType.Infantry;
                        break;
                }

                Log.Info("[Unit] Switched real unit type: {0} -> {1} ({2})",
                    Units.Items[(int) Units.RealTypeTable[(int) unit.RealType]],
                    Units.Items[(int) Units.RealTypeTable[(int) type]], unit);

                unit.RealType = type;

                // Set the edited flag
                unit.SetDirty(UnitClassItemId.RealType);
            }

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Branch);

            // Unit class tab / / Update the display of the unit model tab
            UpdateClassEditableItems();
            UpdateModelEditableItems();
        }

        /// <summary>
        ///     Processing when changing statistical groups
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEyrNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int eyr = (int) eyrNumericUpDown.Value;
            if (eyr == unit.Eyr)
            {
                return;
            }

            Log.Info("[Unit] eyr: {0} -> {1} ({2})", unit.Eyr, eyr, unit);

            // Update value
            unit.Eyr = eyr;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Eyr);

            // Change the font color
            eyrNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing image priority
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGraphicsPriorityNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int prio = (int) gfxPrioNumericUpDown.Value;
            if (prio == unit.GfxPrio)
            {
                return;
            }

            Log.Info("[Unit] gfx prio: {0} -> {1} ({2})", unit.GfxPrio, prio, unit);

            // Update value
            unit.GfxPrio = prio;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.GfxPrio);

            // Change the font color
            gfxPrioNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing list priority
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListPrioNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int prio = (int) listPrioNumericUpDown.Value;
            if (prio == unit.ListPrio)
            {
                return;
            }

            Log.Info("[Unit] list prio: {0} -> {1} ({2})", unit.ListPrio, prio, unit);

            // Update value
            unit.ListPrio = prio;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.ListPrio);

            // Change the font color
            listPrioNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     UI UI Processing when changing priority
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUiPrioNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int prio = (int) uiPrioNumericUpDown.Value;
            if (prio == unit.UiPrio)
            {
                return;
            }

            Log.Info("[Unit] ui prio: {0} -> {1} ({2})", unit.UiPrio, prio, unit);

            // Update value
            unit.UiPrio = prio;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.UiPrio);

            // Change the font color
            uiPrioNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the selection item of the actual unit type combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRealUnitTypeComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing when deselected
            if (realUnitTypeComboBox.SelectedIndex == -1)
            {
                return;
            }

            // Do nothing if the value does not change
            RealUnitType type = (RealUnitType) realUnitTypeComboBox.SelectedIndex;
            if (type == unit.RealType)
            {
                return;
            }

            Log.Info("[Unit] real unit type: {0} -> {1} ({2})",
                Units.Items[(int) Units.RealTypeTable[(int) unit.RealType]],
                Units.Items[(int) Units.RealTypeTable[(int) type]], unit);

            // Update value
            unit.RealType = type;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.RealType);

            // Update drawing to change the item color of the actual unit type combo box
            realUnitTypeComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when changing the state of the standard production type check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDefaultTypeCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (defaultTypeCheckBox.Checked == unit.DefaultType)
            {
                return;
            }

            Log.Info("[Unit] default production type: {0} -> {1} ({2})", BoolHelper.ToString(unit.DefaultType),
                BoolHelper.ToString(defaultTypeCheckBox.Checked), unit);

            // Update value
            unit.DefaultType = defaultTypeCheckBox.Checked;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.DefaultType);

            // Change the font color
            defaultTypeCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the selection item of the sprite type combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpriteTypeComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing when deselected
            if (spriteTypeComboBox.SelectedIndex == -1)
            {
                return;
            }

            // Do nothing if the value does not change
            SpriteType type = (SpriteType) spriteTypeComboBox.SelectedIndex;
            if (type == unit.Sprite)
            {
                return;
            }

            Log.Info("[Unit] sprite type: {0} -> {1} ({2})",
                Units.Items[(int) Units.SpriteTypeTable[(int) unit.Sprite]],
                Units.Items[(int) Units.SpriteTypeTable[(int) type]], unit);

            // Update value
            unit.Sprite = type;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Sprite);

            // Sprite type Update drawing to change the item color of the combo box
            spriteTypeComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when changing the selection item of the alternative unit combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTransmuteComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing when deselected
            if (transmuteComboBox.SelectedIndex == -1)
            {
                return;
            }

            // Do nothing if the value does not change
            UnitType type = Units.DivisionTypes[transmuteComboBox.SelectedIndex];
            if (type == unit.Transmute)
            {
                return;
            }

            Log.Info("[Unit] transmute type: {0} -> {1} ({2})", Units.Items[(int) unit.Transmute],
                Units.Items[(int) type], unit);

            // Update value
            unit.Transmute = type;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Transmute);

            // Update drawing to change the item color of the alternative unit combo box
            transmuteComboBox.Refresh();
        }

        /// <summary>
        ///     Military power text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMilitaryValueTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(militaryValueTextBox.Text, out val))
            {
                militaryValueTextBox.Text = DoubleHelper.ToString(unit.Value);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, unit.Value))
            {
                return;
            }

            Log.Info("[Unit] military value: {0} -> {1} ({2})", DoubleHelper.ToString(unit.Value),
                DoubleHelper.ToString(val), unit);

            // Update value
            unit.Value = val;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Vaule);

            // Change the font color
            militaryValueTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the selection item of the maximum production speed combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxSpeedStepComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing when deselected
            if (maxSpeedStepComboBox.SelectedIndex == -1)
            {
                return;
            }

            // Do nothing if the value does not change
            int val = maxSpeedStepComboBox.SelectedIndex;
            if (val == unit.MaxSpeedStep)
            {
                return;
            }

            Log.Info("[Unit] max speed step: {0} -> {1} ({2})", unit.MaxSpeedStep, val, unit);

            // Update value
            unit.MaxSpeedStep = val;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.MaxSpeedStep);

            // Update drawing to change the item color of the maximum production speed combo box
            maxSpeedStepComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when the status of the productable check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProductableCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }
            // Do nothing if the value does not change
            if (productableCheckBox.Checked == unit.Productable)
            {
                return;
            }

            Log.Info("[Unit] productable: {0} -> {1} ({2})", BoolHelper.ToString(unit.Productable),
                BoolHelper.ToString(productableCheckBox.Checked), unit);

            // Update value
            unit.Productable = productableCheckBox.Checked;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Productable);

            // Change the font color
            productableCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the status of removable check boxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDetachableCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (detachableCheckBox.Checked == unit.Detachable)
            {
                return;
            }

            Log.Info("[Unit] detachable: {0} -> {1} ({2})", BoolHelper.ToString(unit.Detachable),
                BoolHelper.ToString(detachableCheckBox.Checked), unit);

            // Update value
            unit.Detachable = detachableCheckBox.Checked;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Detachable);

            // Change the font color
            detachableCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the status of the carrier air wing check box changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCagCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (cagCheckBox.Checked == unit.Cag)
            {
                return;
            }

            Log.Info("[Unit] cag: {0} -> {1} ({2})", BoolHelper.ToString(unit.Cag),
                BoolHelper.ToString(cagCheckBox.Checked), unit);

            // Update value
            unit.Cag = cagCheckBox.Checked;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Cag);

            // Change the font color
            cagCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the escort fighter check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEscortCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (escortCheckBox.Checked == unit.Escort)
            {
                return;
            }

            Log.Info("[Unit] escort: {0} -> {1} ({2})", BoolHelper.ToString(unit.Escort),
                BoolHelper.ToString(escortCheckBox.Checked), unit);

            // Update value
            unit.Escort = escortCheckBox.Checked;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Escort);

            // Change the font color
            escortCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the engineer check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEngineerCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (engineerCheckBox.Checked == unit.Engineer)
            {
                return;
            }

            Log.Info("[Unit] engineer: {0} -> {1} ({2})", BoolHelper.ToString(unit.Engineer),
                BoolHelper.ToString(engineerCheckBox.Checked), unit);

            // Update value
            unit.Engineer = engineerCheckBox.Checked;

            // Set the edited flag
            unit.SetDirty(UnitClassItemId.Engineer);

            // Change the font color
            engineerCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the maximum number of attached brigades
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxAllowedBrigadesNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // HoI2 or AoD1.07 Do nothing before
            if ((Game.Type == GameType.HeartsOfIron2) ||
                ((Game.Type == GameType.ArsenalOfDemocracy) && (Game.Version < 107)))
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (maxAllowedBrigadesNumericUpDown.Value == unit.GetMaxAllowedBrigades())
            {
                return;
            }

            Log.Info("[Unit] Max allowed brigades: {0} -> {1} ({2})", unit.GetMaxAllowedBrigades(),
                maxAllowedBrigadesNumericUpDown.Value, unit);

            // Update value
            unit.SetMaxAllowedBrigades((int) maxAllowedBrigadesNumericUpDown.Value);

            // Change the font color
            maxAllowedBrigadesNumericUpDown.ForeColor = Color.Red;

            // Notify the update of the maximum number of attached brigades
            if (Game.Type == GameType.ArsenalOfDemocracy)
            {
                HoI2EditorController.OnItemChanged(EditorItemId.MaxAllowedBrigades, this);
            }
        }

        /// <summary>
        ///     Processing when checking the check status of the attached brigade list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllowedBrigadesListViewItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }
            UnitClass brigade = e.Item.Tag as UnitClass;
            if (brigade == null)
            {
                return;
            }

            if (e.Item.Checked)
            {
                // Do nothing if the value does not change
                if (unit.AllowedBrigades.Contains(brigade.Type))
                {
                    return;
                }

                Log.Info("[Unit] Added allowed brigades: {0} ({1})", brigade, unit);

                // Update value
                unit.AllowedBrigades.Add(brigade.Type);
            }
            else
            {
                // Do nothing if the value does not change
                if (!unit.AllowedBrigades.Contains(brigade.Type))
                {
                    return;
                }

                Log.Info("[Unit] Removed allowed brigades: {0} ({1})", brigade, unit);

                // Update value
                unit.AllowedBrigades.Remove(brigade.Type);
            }

            // Set the edited flag
            unit.SetDirtyAllowedBrigades(brigade.Type);

            // Change the font color
            e.Item.ForeColor = Color.Red;
        }

        #endregion

        #region Unit class tab ―――― Improvement

        /// <summary>
        ///     Update items in the improved list view
        /// </summary>
        /// <param name="unit"></param>
        private void UpdateUpgradeList(UnitClass unit)
        {
            // Register items in order
            upgradeListView.BeginUpdate();
            upgradeListView.Items.Clear();
            foreach (UnitUpgrade upgrade in unit.Upgrades)
            {
                upgradeListView.Items.Add(CreateUpgradeListItem(upgrade));
            }
            upgradeListView.EndUpdate();

            // Disable edit items if improvement information is not registered
            if (unit.Upgrades.Count == 0)
            {
                DisableUpgradeItems();
                return;
            }

            // Enable edit items
            EnableUpgradeItems();
        }

        /// <summary>
        ///     Enable edit items for improvement information
        /// </summary>
        private void EnableUpgradeItems()
        {
            upgradeRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for improvement information
        /// </summary>
        private void DisableUpgradeItems()
        {
            upgradeRemoveButton.Enabled = false;
        }

        /// <summary>
        ///     Update the item of the improved unit type combo box
        /// </summary>
        private void UpdateUpgradeTypeComboBox()
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            Graphics g = Graphics.FromHwnd(autoUpgradeClassComboBox.Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;
            upgradeTypeComboBox.BeginUpdate();
            upgradeTypeComboBox.Items.Clear();
            int width = upgradeTypeComboBox.Width;
            // If the current improvement class does not match the military department, register as a candidate in one shot
            UnitClass current = null;
            if (upgradeListView.SelectedIndices.Count > 0)
            {
                current = Units.Items[(int) unit.Upgrades[upgradeListView.SelectedIndices[0]].Type];
                if ((current.Branch != unit.Branch) || (current.Models.Count == 0))
                {
                    width = Math.Max(width,
                        (int) g.MeasureString(current.ToString(), upgradeTypeComboBox.Font).Width +
                        SystemInformation.VerticalScrollBarWidth + margin);
                    upgradeTypeComboBox.Items.Add(current);
                }
            }
            foreach (UnitClass u in Units.DivisionTypes
                .Select(type => Units.Items[(int) type])
                .Where(u => (u.Branch == unit.Branch) &&
                            (u.Models.Count > 0)))
            {
                width = Math.Max(width,
                    (int) g.MeasureString(u.ToString(), upgradeTypeComboBox.Font).Width +
                    SystemInformation.VerticalScrollBarWidth + margin);
                upgradeTypeComboBox.Items.Add(u);
            }
            upgradeTypeComboBox.DropDownWidth = width;
            if (current != null)
            {
                upgradeTypeComboBox.SelectedItem = current;
            }
            else
            {
                if (upgradeTypeComboBox.Items.Count > 0)
                {
                    upgradeTypeComboBox.SelectedIndex = 0;
                }
            }
            upgradeTypeComboBox.EndUpdate();
        }

        /// <summary>
        ///     Item drawing process of improved unit type combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeTypeComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            UnitClass u = upgradeTypeComboBox.Items[e.Index] as UnitClass;
            if (u != null)
            {
                Brush brush;
                if (upgradeListView.SelectedIndices.Count > 0)
                {
                    UnitUpgrade upgrade = unit.Upgrades[upgradeListView.SelectedIndices[0]];
                    if ((u.Type == upgrade.Type) && upgrade.IsDirty(UnitUpgradeItemId.Type))
                    {
                        brush = new SolidBrush(Color.Red);
                    }
                    else
                    {
                        brush = new SolidBrush(SystemColors.WindowText);
                    }
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                e.Graphics.DrawString(u.ToString(), e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Process when changing the selected item in the improved list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Prohibit editing if there is no selection
            if (upgradeListView.SelectedIndices.Count == 0)
            {
                DisableUpgradeItems();
                return;
            }
            UnitUpgrade upgrade = unit.Upgrades[upgradeListView.SelectedIndices[0]];

            // Update the value of the edit item
            UpdateUpgradeTypeComboBox();
            upgradeCostTextBox.Text = DoubleHelper.ToString(upgrade.UpgradeCostFactor);
            upgradeTimeTextBox.Text = DoubleHelper.ToString(upgrade.UpgradeTimeFactor);

            // Update the color of the edit item
            upgradeCostTextBox.ForeColor = upgrade.IsDirty(UnitUpgradeItemId.UpgradeCostFactor)
                ? Color.Red
                : SystemColors.WindowText;
            upgradeTimeTextBox.ForeColor = upgrade.IsDirty(UnitUpgradeItemId.UpgradeTimeFactor)
                ? Color.Red
                : SystemColors.WindowText;

            // Enable edit items
            EnableUpgradeItems();
        }

        /// <summary>
        ///     Processing when changing the width of columns in the improved list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < UpgradeListColumnCount))
            {
                HoI2EditorController.Settings.UnitEditor.UpgradeListColumnWidth[e.ColumnIndex] =
                    upgradeListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Processing before editing items in the improved list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // Unit type
                    e.Type = ItemEditType.List;
                    e.Items = (from UnitClass unit in upgradeTypeComboBox.Items select unit.ToString()).ToArray();
                    e.Index = upgradeTypeComboBox.SelectedIndex;
                    e.DropDownWidth = upgradeTypeComboBox.DropDownWidth;
                    break;

                case 1: // I C
                    e.Type = ItemEditType.Text;
                    e.Text = upgradeCostTextBox.Text;
                    break;

                case 2: // time
                    e.Type = ItemEditType.Text;
                    e.Text = upgradeTimeTextBox.Text;
                    break;
            }
        }

        /// <summary>
        ///     Processing after editing items in the improved list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // Unit type
                    upgradeTypeComboBox.SelectedIndex = e.Index;
                    break;

                case 1: // I C
                    upgradeCostTextBox.Text = e.Text;
                    OnUpgradeCostTextBoxValidated(upgradeCostTextBox, new EventArgs());
                    break;

                case 2: // time
                    upgradeTimeTextBox.Text = e.Text;
                    OnUpgradeTimeTextBoxValidated(upgradeTimeTextBox, new EventArgs());
                    break;
            }

            // Since the items in the list view will be updated by yourself, it will be treated as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Processing when replacing items in the improved list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;

            // Move improvement information
            UnitUpgrade upgrade = unit.Upgrades[srcIndex];
            unit.Upgrades.Insert(destIndex, upgrade);
            if (srcIndex < destIndex)
            {
                unit.Upgrades.RemoveAt(srcIndex);
            }
            else
            {
                unit.Upgrades.RemoveAt(srcIndex + 1);
            }

            Log.Info("[Unit] Moved upgrade info: {0} -> {1} {2} [{3}]", srcIndex, destIndex,
                Units.Items[(int) upgrade.Type], unit);

            // Set the edited flag
            upgrade.SetDirty();
            unit.SetDirtyFile();
        }

        /// <summary>
        ///     Process when changing the selection item of the improved unit type combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeTypeComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no selection
            if (upgradeListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = upgradeListView.SelectedIndices[0];
            UnitUpgrade upgrade = unit.Upgrades[index];

            // Do nothing if the value does not change
            UnitClass selected = upgradeTypeComboBox.SelectedItem as UnitClass;
            if (selected == null)
            {
                return;
            }
            if (selected.Type == upgrade.Type)
            {
                return;
            }

            UnitClass old = Units.Items[(int) upgrade.Type];

            Log.Info("[Unit] upgrade type: {0} -> {1} ({2})", old, selected, unit);

            // Update value
            upgrade.Type = selected.Type;

            // Update items in the improved list view
            upgradeListView.Items[index].Text = selected.ToString();

            // Set the edited flag
            upgrade.SetDirty(UnitUpgradeItemId.Type);
            unit.SetDirtyFile();

            if ((old.Branch != unit.Branch) || (old.Models.Count == 0))
            {
                // If the improved class and the military department do not match, update the item
                UpdateUpgradeTypeComboBox();
            }
            else
            {
                // Improved drawing update to change the item color of the unit type combo box
                upgradeTypeComboBox.Refresh();
            }
        }

        /// <summary>
        ///     Improved cost Textbox Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeCostTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no selection
            if (upgradeListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = upgradeListView.SelectedIndices[0];
            UnitUpgrade upgrade = unit.Upgrades[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(upgradeCostTextBox.Text, out val))
            {
                upgradeCostTextBox.Text = DoubleHelper.ToString(upgrade.UpgradeCostFactor);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, upgrade.UpgradeCostFactor))
            {
                return;
            }

            Log.Info("[Unit] upgrade cost: {0} -> {1} ({2})", DoubleHelper.ToString(upgrade.UpgradeCostFactor),
                DoubleHelper.ToString(val), unit);

            // Update value
            upgrade.UpgradeCostFactor = val;

            // Update items in the improved list view
            upgradeListView.Items[index].SubItems[1].Text = DoubleHelper.ToString(val);

            // Set the edited flag
            upgrade.SetDirty(UnitUpgradeItemId.UpgradeCostFactor);
            unit.SetDirtyFile();

            // Change the font color
            upgradeCostTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Improvement time Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeTimeTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no selection
            if (upgradeListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = upgradeListView.SelectedIndices[0];
            UnitUpgrade upgrade = unit.Upgrades[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(upgradeTimeTextBox.Text, out val))
            {
                upgradeTimeTextBox.Text = DoubleHelper.ToString(upgrade.UpgradeTimeFactor);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, upgrade.UpgradeTimeFactor))
            {
                return;
            }

            Log.Info("[Unit] upgrade time: {0} -> {1} ({2})", DoubleHelper.ToString(upgrade.UpgradeTimeFactor),
                DoubleHelper.ToString(val), unit);

            // Update value
            upgrade.UpgradeTimeFactor = val;

            // Update items in the improved list view
            upgradeListView.Items[index].SubItems[2].Text = DoubleHelper.ToString(val);

            // Set the edited flag
            upgrade.SetDirty(UnitUpgradeItemId.UpgradeTimeFactor);
            unit.SetDirtyFile();

            // Change the font color
            upgradeTimeTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the add button for improvement information is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeAddButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            UnitClass selected = upgradeTypeComboBox.SelectedItem as UnitClass;
            UnitUpgrade upgrade = new UnitUpgrade { Type = selected?.Type ?? unit.Type };
            double val;
            if (DoubleHelper.TryParse(upgradeCostTextBox.Text, out val))
            {
                upgrade.UpgradeCostFactor = val;
            }
            if (DoubleHelper.TryParse(upgradeTimeTextBox.Text, out val))
            {
                upgrade.UpgradeTimeFactor = val;
            }

            Log.Info("[Unit] Added upgrade info: {0} {1} {2} ({3})", Units.Items[(int) upgrade.Type],
                DoubleHelper.ToString(upgrade.UpgradeCostFactor), DoubleHelper.ToString(upgrade.UpgradeTimeFactor), unit);

            // Add improvement information
            unit.Upgrades.Add(upgrade);

            // Set the edited flag
            upgrade.SetDirtyAll();
            unit.SetDirtyFile();

            // Add an item to the improved list view
            AddUpgradeListItem(upgrade);
        }

        /// <summary>
        ///     Processing when the delete button of improvement information is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no selection
            if (upgradeListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = upgradeListView.SelectedIndices[0];

            Log.Info("[Unit] Removed upgrade info: {0} ({1})", Units.Items[(int) unit.Upgrades[index].Type], unit);

            // Delete improvement information
            unit.Upgrades.RemoveAt(index);

            // Set the edited flag
            unit.SetDirtyFile();

            // Remove an item from the improved list view
            RemoveUpgradeListItem(index);
        }

        /// <summary>
        ///     Create an item in the improvement list
        /// </summary>
        /// <param name="upgrade">Improved settings</param>
        /// <returns>Items on the improvement list</returns>
        private static ListViewItem CreateUpgradeListItem(UnitUpgrade upgrade)
        {
            ListViewItem item = new ListViewItem { Text = Units.Items[(int) upgrade.Type].ToString() };
            item.SubItems.Add(DoubleHelper.ToString(upgrade.UpgradeCostFactor));
            item.SubItems.Add(DoubleHelper.ToString(upgrade.UpgradeTimeFactor));

            return item;
        }

        /// <summary>
        ///     Add an item in the improvement list
        /// </summary>
        /// <param name="upgrade">Improved settings for addition</param>
        private void AddUpgradeListItem(UnitUpgrade upgrade)
        {
            // Add an item in the improved list view
            upgradeListView.Items.Add(CreateUpgradeListItem(upgrade));

            // Select the added item
            int index = upgradeListView.Items.Count - 1;
            upgradeListView.Items[index].Focused = true;
            upgradeListView.Items[index].Selected = true;
            upgradeListView.EnsureVisible(index);

            // Enable edit items for improvement
            EnableUpgradeItems();
        }

        /// <summary>
        ///     Remove an item from the improvement list
        /// </summary>
        /// <param name="index">Position of the item to be deleted</param>
        private void RemoveUpgradeListItem(int index)
        {
            // Delete items in the improved list view
            upgradeListView.Items.RemoveAt(index);

            if (index < upgradeListView.Items.Count)
            {
                // Select next to the added item
                upgradeListView.Items[index].Focused = true;
                upgradeListView.Items[index].Selected = true;
            }
            else if (index > 0)
            {
                // Select the last item
                upgradeListView.Items[upgradeListView.Items.Count - 1].Focused = true;
                upgradeListView.Items[upgradeListView.Items.Count - 1].Selected = true;
            }
            else
            {
                // Disable improvement edit items
                DisableUpgradeItems();
            }
        }

        #endregion

        #region Unit model tab

        /// <summary>
        ///     Update the value of the edit item on the unit model tab
        /// </summary>
        private void UpdateModelEditableItems()
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // Model image
            Image prev = modelImagePictureBox.Image;
            string fileName = GetModelImageFileName(unit, index, GetSelectedCountry());
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                Bitmap bitmap = new Bitmap(fileName);
                bitmap.MakeTransparent(Color.Lime);
                modelImagePictureBox.Image = bitmap;
            }
            else
            {
                modelImagePictureBox.Image = null;
            }
            prev?.Dispose();
            // Model icon
            prev = modelIconPictureBox.Image;
            fileName = GetModelIconFileName(unit, index);
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                Bitmap bitmap = new Bitmap(GetModelIconFileName(unit, index));
                bitmap.MakeTransparent(Color.Lime);
                modelIconPictureBox.Image = bitmap;
            }
            else
            {
                modelIconPictureBox.Image = null;
            }
            prev?.Dispose();
            // Model name
            UpdateModelNameTextBox();

            // Organization rate
            defaultOrganisationTextBox.Text = DoubleHelper.ToString(model.DefaultOrganization);
            defaultOrganisationTextBox.ForeColor = model.IsDirty(UnitModelItemId.DefaultOrganization)
                ? Color.Red
                : SystemColors.WindowText;
            // morale
            moraleTextBox.Text = DoubleHelper.ToString(model.Morale);
            moraleTextBox.ForeColor = model.IsDirty(UnitModelItemId.Morale) ? Color.Red : SystemColors.WindowText;
            // Consumables
            supplyConsumptionTextBox.Text = DoubleHelper.ToString(model.SupplyConsumption);
            supplyConsumptionTextBox.ForeColor = model.IsDirty(UnitModelItemId.SupplyConsumption)
                ? Color.Red
                : SystemColors.WindowText;
            // Fuel consumption
            fuelConsumptionTextBox.Text = DoubleHelper.ToString(model.FuelConsumption);
            fuelConsumptionTextBox.ForeColor = model.IsDirty(UnitModelItemId.FuelConsumption)
                ? Color.Red
                : SystemColors.WindowText;
            // requirement I C
            costTextBox.Text = DoubleHelper.ToString(model.Cost);
            costTextBox.ForeColor = model.IsDirty(UnitModelItemId.Cost) ? Color.Red : SystemColors.WindowText;
            // Necessary Time
            buildTimeTextBox.Text = DoubleHelper.ToString(model.BuildTime);
            buildTimeTextBox.ForeColor = model.IsDirty(UnitModelItemId.BuildTime) ? Color.Red : SystemColors.WindowText;
            // Labor force
            manPowerTextBox.Text = DoubleHelper.ToString(model.ManPower);
            manPowerTextBox.ForeColor = model.IsDirty(UnitModelItemId.ManPower) ? Color.Red : SystemColors.WindowText;
            // Maximum speed
            maxSpeedTextBox.Text = DoubleHelper.ToString(model.MaxSpeed);
            maxSpeedTextBox.ForeColor = model.IsDirty(UnitModelItemId.MaxSpeed) ? Color.Red : SystemColors.WindowText;
            // Anti-aircraft defense
            airDefenceTextBox.Text = DoubleHelper.ToString(model.AirDefence);
            airDefenceTextBox.ForeColor = model.IsDirty(UnitModelItemId.AirDefense)
                ? Color.Red
                : SystemColors.WindowText;
            // Anti-aircraft attack power
            airAttackTextBox.Text = DoubleHelper.ToString(model.AirAttack);
            airAttackTextBox.ForeColor = model.IsDirty(UnitModelItemId.AirAttack) ? Color.Red : SystemColors.WindowText;

            // Army
            if (unit.Branch == Branch.Army)
            {
                // Cruising distance
                rangeLabel.Enabled = false;
                rangeTextBox.Enabled = false;
                rangeTextBox.ResetText();
                // Transport load
                transportWeightLabel.Enabled = true;
                transportWeightTextBox.Enabled = true;
                transportWeightTextBox.Text = DoubleHelper.ToString(model.TransportWeight);
                transportWeightTextBox.ForeColor = model.IsDirty(UnitModelItemId.TransportWeight)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Transport capacity
                transportCapabilityLabel.Enabled = false;
                transportCapabilityTextBox.Enabled = false;
                transportCapabilityTextBox.ResetText();
                // Control
                suppressionLabel.Enabled = true;
                suppressionTextBox.Enabled = true;
                suppressionTextBox.Text = DoubleHelper.ToString(model.Suppression);
                suppressionTextBox.ForeColor = model.IsDirty(UnitModelItemId.Suppression)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Defense power
                defensivenessLabel.Enabled = true;
                defensivenessTextBox.Enabled = true;
                defensivenessTextBox.Text = DoubleHelper.ToString(model.Defensiveness);
                defensivenessTextBox.ForeColor = model.IsDirty(UnitModelItemId.Defensiveness)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Endurance
                toughnessLabel.Enabled = true;
                toughnessTextBox.Enabled = true;
                toughnessTextBox.Text = DoubleHelper.ToString(model.Toughness);
                toughnessTextBox.ForeColor = model.IsDirty(UnitModelItemId.Toughness)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Vulnerability
                softnessLabel.Enabled = true;
                softnessTextBox.Enabled = true;
                softnessTextBox.Text = DoubleHelper.ToString(model.Softness);
                softnessTextBox.ForeColor = model.IsDirty(UnitModelItemId.Softness)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Ground search enemy power
                surfaceDetectionCapabilityLabel.Enabled = false;
                surfaceDetectionCapabilityTextBox.Enabled = false;
                surfaceDetectionCapabilityTextBox.ResetText();
                // Anti-aircraft enemy power
                airDetectionCapabilityLabel.Enabled = false;
                airDetectionCapabilityTextBox.Enabled = false;
                airDetectionCapabilityTextBox.ResetText();
            }
            else
            {
                // Cruising distance
                rangeLabel.Enabled = true;
                rangeTextBox.Enabled = true;
                rangeTextBox.Text = DoubleHelper.ToString(model.Range);
                rangeTextBox.ForeColor = model.IsDirty(UnitModelItemId.Range) ? Color.Red : SystemColors.WindowText;
                // Transport load
                transportWeightLabel.Enabled = false;
                transportWeightTextBox.Enabled = false;
                transportWeightTextBox.ResetText();
                // Transport capacity
                transportCapabilityLabel.Enabled = true;
                transportCapabilityTextBox.Enabled = true;
                transportCapabilityTextBox.Text = DoubleHelper.ToString(model.TransportCapability);
                transportCapabilityTextBox.ForeColor = model.IsDirty(UnitModelItemId.TransportCapability)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Control
                suppressionLabel.Enabled = false;
                suppressionTextBox.Enabled = false;
                suppressionTextBox.ResetText();
                // Defense power
                defensivenessLabel.Enabled = false;
                defensivenessTextBox.Enabled = false;
                defensivenessTextBox.ResetText();
                // Endurance
                toughnessLabel.Enabled = false;
                toughnessTextBox.Enabled = false;
                toughnessTextBox.ResetText();
                // Vulnerability
                softnessLabel.Enabled = false;
                softnessTextBox.Enabled = false;
                softnessTextBox.ResetText();
                // Ground search enemy power
                surfaceDetectionCapabilityLabel.Enabled = true;
                surfaceDetectionCapabilityTextBox.Enabled = true;
                surfaceDetectionCapabilityTextBox.Text = DoubleHelper.ToString(model.SurfaceDetectionCapability);
                surfaceDetectionCapabilityTextBox.ForeColor = model.IsDirty(UnitModelItemId.SurfaceDetectionCapability)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Anti-aircraft enemy power
                airDetectionCapabilityLabel.Enabled = true;
                airDetectionCapabilityTextBox.Enabled = true;
                airDetectionCapabilityTextBox.Text = DoubleHelper.ToString(model.AirDetectionCapability);
                airDetectionCapabilityTextBox.ForeColor = model.IsDirty(UnitModelItemId.AirDetectionCapability)
                    ? Color.Red
                    : SystemColors.WindowText;
            }

            // Army division
            if ((unit.Branch == Branch.Army) && (unit.Organization == UnitOrganization.Division))
            {
                // Speed cap (( artillery )
                speedCapArtLabel.Enabled = true;
                speedCapArtTextBox.Enabled = true;
                speedCapArtTextBox.Text = DoubleHelper.ToString(model.SpeedCapArt);
                speedCapArtTextBox.ForeColor = model.IsDirty(UnitModelItemId.SpeedCapArt)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Speed cap (( Engineer )
                speedCapEngLabel.Enabled = true;
                speedCapEngTextBox.Enabled = true;
                speedCapEngTextBox.Text = DoubleHelper.ToString(model.SpeedCapEng);
                speedCapEngTextBox.ForeColor = model.IsDirty(UnitModelItemId.SpeedCapEng)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Speed cap (( Anti-tank )
                speedCapAtLabel.Enabled = true;
                speedCapAtTextBox.Enabled = true;
                speedCapAtTextBox.Text = DoubleHelper.ToString(model.SpeedCapAt);
                speedCapAtTextBox.ForeColor = model.IsDirty(UnitModelItemId.SpeedCapAt)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Speed cap (( Anti-aircraft )
                speedCapAaLabel.Enabled = true;
                speedCapAaTextBox.Enabled = true;
                speedCapAaTextBox.Text = DoubleHelper.ToString(model.SpeedCapAa);
                speedCapAaTextBox.ForeColor = model.IsDirty(UnitModelItemId.SpeedCapAa)
                    ? Color.Red
                    : SystemColors.WindowText;
            }
            else
            {
                // Speed cap (( artillery )
                speedCapArtLabel.Enabled = false;
                speedCapArtTextBox.Enabled = false;
                speedCapArtTextBox.ResetText();
                // Speed cap (( Engineer )
                speedCapEngLabel.Enabled = false;
                speedCapEngTextBox.Enabled = false;
                speedCapEngTextBox.ResetText();
                // Speed cap (( Anti-tank )
                speedCapAtTextBox.Enabled = false;
                speedCapAaLabel.Enabled = false;
                speedCapAtTextBox.ResetText();
                // Speed cap (( Anti-aircraft )
                speedCapAtLabel.Enabled = false;
                speedCapAaTextBox.Enabled = false;
                speedCapAaTextBox.ResetText();
            }

            // Navy
            if (unit.Branch == Branch.Navy)
            {
                // Anti-ship defense
                seaDefenceLabel.Enabled = true;
                seaDefenceTextBox.Enabled = true;
                seaDefenceTextBox.Text = DoubleHelper.ToString(model.SeaDefense);
                seaDefenceTextBox.ForeColor = model.IsDirty(UnitModelItemId.SeaDefense)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Interpersonal attack power
                softAttackLabel.Enabled = false;
                softAttackTextBox.Enabled = false;
                softAttackTextBox.ResetText();
                // Anti-instep attack power
                hardAttackLabel.Enabled = false;
                hardAttackTextBox.Enabled = false;
                hardAttackTextBox.ResetText();
                // Ship-to-ship attack power
                seaAttackLabel.Enabled = true;
                seaAttackTextBox.Enabled = true;
                seaAttackTextBox.Text = DoubleHelper.ToString(model.SeaAttack);
                seaAttackTextBox.ForeColor = model.IsDirty(UnitModelItemId.SeaAttack)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Anti-submarine attack power
                subAttackLabel.Enabled = true;
                subAttackTextBox.Enabled = true;
                subAttackTextBox.Text = DoubleHelper.ToString(model.SubAttack);
                subAttackTextBox.ForeColor = model.IsDirty(UnitModelItemId.SubAttack)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Fleet attack power
                convoyAttackLabel.Enabled = true;
                convoyAttackTextBox.Enabled = true;
                convoyAttackTextBox.Text = DoubleHelper.ToString(model.ConvoyAttack);
                convoyAttackTextBox.ForeColor = model.IsDirty(UnitModelItemId.ConvoyAttack)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Coastal artillery ability
                shoreBombardmentLabel.Enabled = true;
                shoreBombardmentTextBox.Enabled = true;
                shoreBombardmentTextBox.Text = DoubleHelper.ToString(model.ShoreBombardment);
                shoreBombardmentTextBox.ForeColor = model.IsDirty(UnitModelItemId.ShoreBombardment)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Range
                distanceLabel.Enabled = true;
                distanceTextBox.Enabled = true;
                distanceTextBox.Text = DoubleHelper.ToString(model.Distance);
                distanceTextBox.ForeColor = model.IsDirty(UnitModelItemId.Distance)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Visibility
                visibilityLabel.Enabled = true;
                visibilityTextBox.Enabled = true;
                visibilityTextBox.Text = DoubleHelper.ToString(model.Visibility);
                visibilityTextBox.ForeColor = model.IsDirty(UnitModelItemId.Visibility)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Anti-ship search enemy power
                subDetectionCapabilityLabel.Enabled = true;
                subDetectionCapabilityTextBox.Enabled = true;
                subDetectionCapabilityTextBox.Text = DoubleHelper.ToString(model.SubDetectionCapability);
                subDetectionCapabilityTextBox.ForeColor = model.IsDirty(UnitModelItemId.SubDetectionCapability)
                    ? Color.Red
                    : SystemColors.WindowText;
            }
            else
            {
                // Anti-ship defense
                seaDefenceLabel.Enabled = false;
                seaDefenceTextBox.Enabled = false;
                seaDefenceTextBox.ResetText();
                // Interpersonal attack power
                softAttackLabel.Enabled = true;
                softAttackTextBox.Enabled = true;
                softAttackTextBox.Text = DoubleHelper.ToString(model.SoftAttack);
                softAttackTextBox.ForeColor = model.IsDirty(UnitModelItemId.SoftAttack)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Anti-instep attack power
                hardAttackLabel.Enabled = true;
                hardAttackTextBox.Enabled = true;
                hardAttackTextBox.Text = DoubleHelper.ToString(model.HardAttack);
                hardAttackTextBox.ForeColor = model.IsDirty(UnitModelItemId.HardAttack)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Ship-to-ship attack power
                seaAttackLabel.Enabled = false;
                seaAttackTextBox.Enabled = false;
                seaAttackTextBox.ResetText();
                // Anti-submarine attack power
                subAttackLabel.Enabled = false;
                subAttackTextBox.Enabled = false;
                subAttackTextBox.ResetText();
                // Fleet attack power
                convoyAttackLabel.Enabled = false;
                convoyAttackTextBox.Enabled = false;
                convoyAttackTextBox.ResetText();
                // Coastal artillery ability
                shoreBombardmentLabel.Enabled = false;
                shoreBombardmentTextBox.Enabled = false;
                shoreBombardmentTextBox.ResetText();
                // Range
                distanceLabel.Enabled = false;
                distanceTextBox.Enabled = false;
                distanceTextBox.ResetText();
                // Visibility
                visibilityLabel.Enabled = false;
                visibilityTextBox.Enabled = false;
                visibilityTextBox.ResetText();
                // Anti-ship search enemy power
                subDetectionCapabilityLabel.Enabled = false;
                subDetectionCapabilityTextBox.Enabled = false;
                subDetectionCapabilityTextBox.ResetText();
            }

            // Air Force
            if (unit.Branch == Branch.Airforce)
            {
                // Ground defense
                surfaceDefenceLabel.Enabled = true;
                surfaceDefenceTextBox.Enabled = true;
                surfaceDefenceTextBox.Text = DoubleHelper.ToString(model.SurfaceDefence);
                surfaceDefenceTextBox.ForeColor = model.IsDirty(UnitModelItemId.SurfaceDefense)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Air-to-ship attack power
                navalAttackLabel.Enabled = true;
                navalAttackTextBox.Enabled = true;
                navalAttackTextBox.Text = DoubleHelper.ToString(model.NavalAttack);
                navalAttackTextBox.ForeColor = model.IsDirty(UnitModelItemId.NavalAttack)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Strategic bombing attack power
                strategicAttackLabel.Enabled = true;
                strategicAttackTextBox.Enabled = true;
                strategicAttackTextBox.Text = DoubleHelper.ToString(model.StrategicAttack);
                strategicAttackTextBox.ForeColor = model.IsDirty(UnitModelItemId.StrategicAttack)
                    ? Color.Red
                    : SystemColors.WindowText;
            }
            else
            {
                // Ground defense
                surfaceDefenceLabel.Enabled = false;
                surfaceDefenceTextBox.Enabled = false;
                surfaceDefenceTextBox.ResetText();
                // Air-to-ship attack power
                navalAttackLabel.Enabled = false;
                navalAttackTextBox.Enabled = false;
                navalAttackTextBox.ResetText();
                // Strategic bombing attack power
                strategicAttackLabel.Enabled = false;
                strategicAttackTextBox.Enabled = false;
                strategicAttackTextBox.ResetText();
            }

            // AoD / Army
            if ((Game.Type == GameType.ArsenalOfDemocracy) && (unit.Branch == Branch.Army))
            {
                // Largest supplies
                maxSupplyStockLabel.Enabled = true;
                maxSupplyStockTextBox.Enabled = true;
                maxSupplyStockTextBox.Text = DoubleHelper.ToString(model.MaxSupplyStock);
                maxSupplyStockTextBox.ForeColor = model.IsDirty(UnitModelItemId.MaxSupplyStock)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Maximum fuel
                maxOilStockLabel.Enabled = true;
                maxOilStockTextBox.Enabled = true;
                maxOilStockTextBox.Text = DoubleHelper.ToString(model.MaxOilStock);
                maxOilStockTextBox.ForeColor = model.IsDirty(UnitModelItemId.MaxOilStock)
                    ? Color.Red
                    : SystemColors.WindowText;
            }
            else
            {
                // Largest supplies
                maxSupplyStockLabel.Enabled = false;
                maxSupplyStockTextBox.Enabled = false;
                maxSupplyStockTextBox.ResetText();
                // Maximum fuel
                maxOilStockLabel.Enabled = false;
                maxOilStockTextBox.Enabled = false;
                maxOilStockTextBox.ResetText();
            }

            // AoD / Army Brigade
            if ((Game.Type == GameType.ArsenalOfDemocracy) &&
                (unit.Branch == Branch.Army) &&
                (unit.Organization == UnitOrganization.Brigade))
            {
                // Shooting ability
                artilleryBombardmentLabel.Enabled = true;
                artilleryBombardmentTextBox.Enabled = true;
                artilleryBombardmentTextBox.Text = DoubleHelper.ToString(model.ArtilleryBombardment);
                artilleryBombardmentTextBox.ForeColor = model.IsDirty(UnitModelItemId.ArtilleryBombardment)
                    ? Color.Red
                    : SystemColors.WindowText;
            }
            else
            {
                // Shooting ability
                artilleryBombardmentLabel.Enabled = false;
                artilleryBombardmentTextBox.Enabled = false;
                artilleryBombardmentTextBox.ResetText();
            }

            // DH / Division
            if ((Game.Type == GameType.DarkestHour) && (unit.Organization == UnitOrganization.Division))
            {
                // Replenishment cost
                reinforceCostLabel.Enabled = true;
                reinforceCostTextBox.Enabled = true;
                reinforceCostTextBox.Text = DoubleHelper.ToString(model.ReinforceCostFactor);
                reinforceCostTextBox.ForeColor = model.IsDirty(UnitModelItemId.ReinforceCostFactor)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Replenishment time
                reinforceTimeLabel.Enabled = true;
                reinforceTimeTextBox.Enabled = true;
                reinforceTimeTextBox.Text = DoubleHelper.ToString(model.ReinforceTimeFactor);
                reinforceTimeTextBox.ForeColor = model.IsDirty(UnitModelItemId.ReinforceTimeFactor)
                    ? Color.Red
                    : SystemColors.WindowText;
            }
            else
            {
                // Replenishment cost
                reinforceCostLabel.Enabled = false;
                reinforceCostTextBox.Enabled = false;
                reinforceCostTextBox.ResetText();
                // Replenishment time
                reinforceTimeLabel.Enabled = false;
                reinforceTimeTextBox.Enabled = false;
                reinforceTimeTextBox.ResetText();
            }

            // DH / Army Division
            if ((Game.Type == GameType.DarkestHour) &&
                (unit.Branch == Branch.Army) &&
                (unit.Organization == UnitOrganization.Division))
            {
                // Fuel shortage correction
                noFuelCombatModLabel.Enabled = true;
                noFuelCombatModTextBox.Enabled = true;
                noFuelCombatModTextBox.Text = DoubleHelper.ToString(model.NoFuelCombatMod);
                noFuelCombatModTextBox.ForeColor = model.IsDirty(UnitModelItemId.NoFuelCombatMod)
                    ? Color.Red
                    : SystemColors.WindowText;
            }
            else
            {
                // Fuel shortage correction
                noFuelCombatModLabel.Enabled = false;
                noFuelCombatModTextBox.Enabled = false;
                noFuelCombatModTextBox.ResetText();
            }

            // DH apart from / / Navy Division
            if ((Game.Type != GameType.DarkestHour) &&
                (unit.Branch == Branch.Navy) &&
                (unit.Organization == UnitOrganization.Division))
            {
                // Improvement cost
                upgradeCostFactorLabel.Enabled = false;
                upgradeCostFactorTextBox.Enabled = false;
                upgradeCostFactorTextBox.ResetText();
                // Improvement time
                upgradeTimeFactorLabel.Enabled = false;
                upgradeTimeFactorTextBox.Enabled = false;
                upgradeTimeFactorTextBox.ResetText();
                // 2 Stage improvement
                upgradeTimeBoostCheckBox.Enabled = false;
                upgradeTimeBoostCheckBox.Checked = false;
                autoUpgradeCheckBox.Enabled = false;
                autoUpgradeCheckBox.Checked = false;
                // Automatic improvement destination
                autoUpgradeClassComboBox.BeginUpdate();
                autoUpgradeClassComboBox.Items.Clear();
                autoUpgradeClassComboBox.EndUpdate();
                autoUpgradeModelComboBox.BeginUpdate();
                autoUpgradeModelComboBox.Items.Clear();
                autoUpgradeModelComboBox.EndUpdate();
            }
            else
            {
                // Improvement cost
                upgradeCostFactorLabel.Enabled = true;
                upgradeCostFactorTextBox.Enabled = true;
                upgradeCostFactorTextBox.Text = DoubleHelper.ToString(model.UpgradeCostFactor);
                upgradeCostFactorTextBox.ForeColor = model.IsDirty(UnitModelItemId.UpgradeCostFactor)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Improvement time
                upgradeTimeFactorLabel.Enabled = true;
                upgradeTimeFactorTextBox.Enabled = true;
                upgradeTimeFactorTextBox.Text = DoubleHelper.ToString(model.UpgradeTimeFactor);
                upgradeTimeFactorTextBox.ForeColor = model.IsDirty(UnitModelItemId.UpgradeTimeFactor)
                    ? Color.Red
                    : SystemColors.WindowText;
                // 2 Stage improvement
                upgradeTimeBoostCheckBox.Enabled = Game.Type == GameType.DarkestHour;
                upgradeTimeBoostCheckBox.Checked = model.UpgradeTimeBoost;
                upgradeTimeBoostCheckBox.ForeColor = model.IsDirty(UnitModelItemId.UpgradeTimeBoost)
                    ? Color.Red
                    : SystemColors.WindowText;
                // Automatic improvement destination
                autoUpgradeCheckBox.Enabled = Game.Type == GameType.DarkestHour;
                autoUpgradeCheckBox.Checked = model.AutoUpgrade;
                autoUpgradeCheckBox.ForeColor = model.IsDirty(UnitModelItemId.AutoUpgrade)
                    ? Color.Red
                    : SystemColors.WindowText;
                UpdateAutoUpgradeClassList();
                UpdateAutoUpgradeModelList();
            }

            // DH1.03 from
            if ((Game.Type == GameType.DarkestHour) && (Game.Version >= 103))
            {
                // Update equipment list
                UpdateEquipmentList(model);
            }

            // DH1.03 from / / Army Brigade
            if ((Game.Type == GameType.DarkestHour) &&
                (Game.Version >= 103) &&
                (unit.Branch == Branch.Army) &&
                (unit.Organization == UnitOrganization.Brigade))
            {
                // Speed cap
                speedCapAllLabel.Enabled = true;
                speedCapAllTextBox.Enabled = true;
                speedCapAllTextBox.Text = DoubleHelper.ToString(model.SpeedCap);
                speedCapAllTextBox.ForeColor = model.IsDirty(UnitModelItemId.SpeedCap)
                    ? Color.Red
                    : SystemColors.WindowText;
            }
            else
            {
                // Speed cap
                speedCapAllLabel.Enabled = false;
                speedCapAllTextBox.Enabled = false;
                speedCapAllTextBox.ResetText();
            }
        }

        /// <summary>
        ///     Enable edit items on the Unit Model tab
        /// </summary>
        private void EnableModelEditableItems()
        {
            modelNameTextBox.Enabled = true;
            basicGroupBox.Enabled = true;
            productionGroupBox.Enabled = true;
            speedGroupBox.Enabled = true;
            battleGroupBox.Enabled = true;

            cloneButton.Enabled = true;
            removeButton.Enabled = true;

            // DH1.03 from
            if ((Game.Type == GameType.DarkestHour) && (Game.Version >= 103))
            {
                equipmentGroupBox.Enabled = true;
            }
            else
            {
                equipmentGroupBox.Enabled = false;
            }
        }

        /// <summary>
        ///     Disable edit items on the unit model tab
        /// </summary>
        private void DisableModelEditableItems()
        {
            modelNameTextBox.Enabled = false;
            basicGroupBox.Enabled = false;
            productionGroupBox.Enabled = false;
            speedGroupBox.Enabled = false;
            battleGroupBox.Enabled = false;
            equipmentGroupBox.Enabled = false;

            Image prev = modelImagePictureBox.Image;
            modelImagePictureBox.Image = null;
            prev?.Dispose();
            prev = modelIconPictureBox.Image;
            modelIconPictureBox.Image = null;
            prev?.Dispose();
            modelNameTextBox.ResetText();

            defaultOrganisationTextBox.ResetText();
            moraleTextBox.ResetText();
            rangeTextBox.ResetText();
            transportWeightTextBox.ResetText();
            transportCapabilityTextBox.ResetText();
            suppressionTextBox.ResetText();
            supplyConsumptionTextBox.ResetText();
            fuelConsumptionTextBox.ResetText();
            maxSupplyStockTextBox.ResetText();
            maxOilStockTextBox.ResetText();

            costTextBox.ResetText();
            buildTimeTextBox.ResetText();
            manPowerTextBox.ResetText();
            upgradeCostFactorTextBox.ResetText();
            upgradeTimeFactorTextBox.ResetText();
            reinforceCostTextBox.ResetText();
            reinforceTimeTextBox.ResetText();

            maxSpeedTextBox.ResetText();
            speedCapAllTextBox.ResetText();
            speedCapArtTextBox.ResetText();
            speedCapEngTextBox.ResetText();
            speedCapAtTextBox.ResetText();
            speedCapAaTextBox.ResetText();

            defensivenessTextBox.ResetText();
            seaDefenceTextBox.ResetText();
            airDefenceTextBox.ResetText();
            surfaceDefenceTextBox.ResetText();
            toughnessTextBox.ResetText();
            softnessTextBox.ResetText();
            softAttackTextBox.ResetText();
            hardAttackTextBox.ResetText();
            seaAttackTextBox.ResetText();
            subAttackTextBox.ResetText();
            convoyAttackTextBox.ResetText();
            shoreBombardmentTextBox.ResetText();
            airAttackTextBox.ResetText();
            navalAttackTextBox.ResetText();
            strategicAttackTextBox.ResetText();
            artilleryBombardmentTextBox.ResetText();
            distanceTextBox.ResetText();
            visibilityTextBox.ResetText();
            surfaceDetectionCapabilityTextBox.ResetText();
            subDetectionCapabilityTextBox.ResetText();
            airDetectionCapabilityTextBox.ResetText();
            noFuelCombatModTextBox.ResetText();

            equipmentListView.Items.Clear();
            resourceComboBox.ResetText();
            quantityTextBox.ResetText();

            cloneButton.Enabled = false;
            removeButton.Enabled = false;
            topButton.Enabled = false;
            upButton.Enabled = false;
            downButton.Enabled = false;
            bottomButton.Enabled = false;
        }

        /// <summary>
        ///     Initialize the character string of the edit item of the unit model
        /// </summary>
        private void InitModelItemText()
        {
            if (Misc.CombatMode)
            {
                defensivenessLabel.Text = Resources.UnitModelDefensiveVulnerablity;
                toughnessLabel.Text = Resources.UnitModelOffensiveVulnerability;
                seaDefenceLabel.Text = Resources.UnitModelNavalVulnerability;
                airDefenceLabel.Text = Resources.UnitModelAirVulnerability;
                surfaceDefenceLabel.Text = Resources.UnitModelGroundVulnerability;
            }
            else
            {
                defensivenessLabel.Text = Resources.UnitModelDefensiveness;
                toughnessLabel.Text = Resources.UnitModelToughness;
                seaDefenceLabel.Text = Resources.UnitModelSeaDefence;
                airDefenceLabel.Text = Resources.UnitModelAirDefence;
                surfaceDefenceLabel.Text = Resources.UnitModelSurfaceDefence;
            }
        }

        /// <summary>
        ///     Update the display of the unit model name
        /// </summary>
        private void UpdateModelNameTextBox()
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];

            // Update the unit model name
            UnitModel model = unit.Models[index];
            Country country = GetSelectedCountry();
            if (country == Country.None)
            {
                modelNameTextBox.Text = unit.GetModelName(index);
                modelNameTextBox.ForeColor = model.IsDirty(UnitModelItemId.Name) ? Color.Red : SystemColors.WindowText;
            }
            else
            {
                string name = unit.GetCountryModelName(index, country);
                if (string.IsNullOrEmpty(name))
                {
                    modelNameTextBox.Text = unit.GetModelName(index);
                    modelNameTextBox.ForeColor = model.IsDirty(UnitModelItemId.Name) ? Color.Salmon : Color.Gray;
                }
                else
                {
                    modelNameTextBox.Text = name;
                    modelNameTextBox.ForeColor = model.IsDirtyName(country) ? Color.Red : SystemColors.WindowText;
                }
            }
        }

        /// <summary>
        ///     Processing when changing the unit model name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModelNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // Do nothing if the value does not change
            Country country = GetSelectedCountry();
            string name = unit.GetCountryModelName(index, country);
            if (string.IsNullOrEmpty(name))
            {
                name = unit.GetModelName(index);
            }
            if (modelNameTextBox.Text.Equals(name))
            {
                return;
            }

            if ((country != Country.None) && string.IsNullOrEmpty(modelNameTextBox.Text))
            {
                // Delete the model name by country
                unit.RemoveModelName(index, country);
                // Set a common model name
                modelNameTextBox.Text = unit.GetModelName(index);
                // Change the font color
                modelNameTextBox.ForeColor = model.IsDirty(UnitModelItemId.Name) ? Color.Salmon : Color.Gray;
            }
            else
            {
                // Update value
                unit.SetModelName(index, country, modelNameTextBox.Text);
                // Change the font color
                modelNameTextBox.ForeColor = Color.Red;
            }

            // Update the items in the unit model list
            modelListView.Items[index].SubItems[1].Text = modelNameTextBox.Text;

            // Set the edited flag
            model.SetDirtyName(country);
            unit.SetDirty();

            // Notify the update of the unit model name
            HoI2EditorController.OnItemChanged(
                country == Country.None ? EditorItemId.CommonModelName : EditorItemId.CountryModelName, this);
        }

        /// <summary>
        ///     Get the file name of the unit model image
        /// </summary>
        /// <param name="unit">Unit class</param>
        /// <param name="index">Unit model index</param>
        /// <param name="country">Country tag</param>
        /// <returns>File name of unit model image</returns>
        private static string GetModelImageFileName(UnitClass unit, int index, Country country)
        {
            string name;
            string fileName;
            if (country != Country.None)
            {
                // Country tag designation / / Model number specification
                name = string.Format(
                    unit.Organization == UnitOrganization.Division
                        ? "ill_div_{0}_{1}_{2}.bmp"
                        : "ill_bri_{0}_{1}_{2}.bmp",
                    Countries.Strings[(int) country],
                    Units.UnitNumbers[(int) unit.Type],
                    index);
                fileName = Game.GetReadFileName(Game.ModelPicturePathName, name);
                if (File.Exists(fileName))
                {
                    return fileName;
                }

                // Country tag designation / / The model number is 0 specify
                name = string.Format(
                    unit.Organization == UnitOrganization.Division
                        ? "ill_div_{0}_{1}_0.bmp"
                        : "ill_bri_{0}_{1}_0.bmp",
                    Countries.Strings[(int) country],
                    Units.UnitNumbers[(int) unit.Type]);
                fileName = Game.GetReadFileName(Game.ModelPicturePathName, name);
                if (File.Exists(fileName))
                {
                    return fileName;
                }
            }

            // Model number specification
            name = string.Format(
                unit.Organization == UnitOrganization.Division
                    ? "ill_div_{0}_{1}.bmp"
                    : "ill_bri_{0}_{1}.bmp",
                Units.UnitNumbers[(int) unit.Type],
                index);
            fileName = Game.GetReadFileName(Game.ModelPicturePathName, name);
            if (File.Exists(fileName))
            {
                return fileName;
            }

            // The model number is 0 specify
            name = string.Format(
                unit.Organization == UnitOrganization.Division
                    ? "ill_div_{0}_0.bmp"
                    : "ill_bri_{0}_0.bmp",
                Units.UnitNumbers[(int) unit.Type]);
            fileName = Game.GetReadFileName(Game.ModelPicturePathName, name);
            return File.Exists(fileName) ? fileName : string.Empty;
        }

        /// <summary>
        ///     Get the file name of the unit model icon
        /// </summary>
        /// <param name="unit">Unit class</param>
        /// <param name="index">Unit model index</param>
        /// <returns>File name of the unit model icon</returns>
        private static string GetModelIconFileName(UnitClass unit, int index)
        {
            // The brigade does not have an icon, so it returns an empty string
            if (unit.Organization == UnitOrganization.Brigade)
            {
                return string.Empty;
            }

            string name = $"model_{Units.UnitNumbers[(int) unit.Type]}_{index}.bmp";
            string fileName = Game.GetReadFileName(Game.ModelPicturePathName, name);
            return File.Exists(fileName) ? fileName : string.Empty;
        }

        #endregion

        #region Unit model tab ―――― Basic status

        /// <summary>
        ///     Organization rate text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDefaultOrganizationTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(defaultOrganisationTextBox.Text, out val))
            {
                defaultOrganisationTextBox.Text = DoubleHelper.ToString(model.DefaultOrganization);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.DefaultOrganization))
            {
                return;
            }

            Log.Info("[Unit] default organization: {0} -> {1} ({2})", DoubleHelper.ToString(model.DefaultOrganization),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.DefaultOrganization = val;

            // Update the items in the unit model list
            modelListView.Items[index].SubItems[7].Text = DoubleHelper.ToString(val);

            // Set the edited flag
            model.SetDirty(UnitModelItemId.DefaultOrganization);
            unit.SetDirtyFile();

            // Change the font color
            defaultOrganisationTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Morale text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMoraleTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(moraleTextBox.Text, out val))
            {
                moraleTextBox.Text = DoubleHelper.ToString(model.Morale);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.Morale))
            {
                return;
            }

            Log.Info("[Unit] morale: {0} -> {1} ({2})", DoubleHelper.ToString(model.Morale), DoubleHelper.ToString(val),
                unit.GetModelName(index));

            // Update value
            model.Morale = val;

            // Update the items in the unit model list
            modelListView.Items[index].SubItems[8].Text = DoubleHelper.ToString(val);

            // Set the edited flag
            model.SetDirty(UnitModelItemId.Morale);
            unit.SetDirtyFile();

            // Change the font color
            moraleTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Cruising distance text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRangeTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(rangeTextBox.Text, out val))
            {
                rangeTextBox.Text = DoubleHelper.ToString(model.Range);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.Range))
            {
                return;
            }

            Log.Info("[Unit] range: {0} -> {1} ({2})", DoubleHelper.ToString(model.Range), DoubleHelper.ToString(val),
                unit.GetModelName(index));

            // Update value
            model.Range = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.Range);
            unit.SetDirtyFile();

            // Change the font color
            rangeTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Transport load text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTransportWeightTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(transportWeightTextBox.Text, out val))
            {
                transportWeightTextBox.Text = DoubleHelper.ToString(model.TransportWeight);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.TransportWeight))
            {
                return;
            }

            Log.Info("[Unit] transport weight: {0} -> {1} ({2})", DoubleHelper.ToString(model.TransportWeight),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.TransportWeight = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.TransportWeight);
            unit.SetDirtyFile();

            // Change the font color
            transportWeightTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Transport capacity text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTransportCapabilityTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(transportCapabilityTextBox.Text, out val))
            {
                transportCapabilityTextBox.Text = DoubleHelper.ToString(model.TransportCapability);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.TransportCapability))
            {
                return;
            }

            Log.Info("[Unit] transport capacity: {0} -> {1} ({2})", DoubleHelper.ToString(model.TransportCapability),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.TransportCapability = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.DefaultOrganization);
            unit.SetDirtyFile();

            // Change the font color
            transportCapabilityTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Control text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSuppressionTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(suppressionTextBox.Text, out val))
            {
                suppressionTextBox.Text = DoubleHelper.ToString(model.Suppression);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.Suppression))
            {
                return;
            }

            Log.Info("[Unit] suppression: {0} -> {1} ({2})", DoubleHelper.ToString(model.Suppression),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.Suppression = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.Suppression);
            unit.SetDirtyFile();

            // Change the font color
            suppressionTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Consumables Text Box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSupplyConsumptionTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(supplyConsumptionTextBox.Text, out val))
            {
                supplyConsumptionTextBox.Text = DoubleHelper.ToString(model.SupplyConsumption);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SupplyConsumption))
            {
                return;
            }

            Log.Info("[Unit] supply consumption: {0} -> {1} ({2})", DoubleHelper.ToString(model.SupplyConsumption),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.SupplyConsumption = val;

            // Update the items in the unit model list
            modelListView.Items[index].SubItems[5].Text = DoubleHelper.ToString(val);

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SupplyConsumption);
            unit.SetDirtyFile();

            // Change the font color
            supplyConsumptionTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Fuel consumption text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFuelConsumptionTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(fuelConsumptionTextBox.Text, out val))
            {
                fuelConsumptionTextBox.Text = DoubleHelper.ToString(model.FuelConsumption);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.FuelConsumption))
            {
                return;
            }

            Log.Info("[Unit] fuel consumption: {0} -> {1} ({2})", DoubleHelper.ToString(model.FuelConsumption),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.FuelConsumption = val;

            // Update the items in the unit model list
            modelListView.Items[index].SubItems[6].Text = DoubleHelper.ToString(val);

            // Set the edited flag
            model.SetDirty(UnitModelItemId.FuelConsumption);
            unit.SetDirtyFile();

            // Change the font color
            fuelConsumptionTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Maximum supplies Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxSupplyStockTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(maxSupplyStockTextBox.Text, out val))
            {
                maxSupplyStockTextBox.Text = DoubleHelper.ToString(model.MaxSupplyStock);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.MaxSupplyStock))
            {
                return;
            }

            Log.Info("[Unit] max supply stock: {0} -> {1} ({2})", DoubleHelper.ToString(model.MaxSupplyStock),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.MaxSupplyStock = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.MaxSupplyStock);
            unit.SetDirtyFile();

            // Change the font color
            maxSupplyStockTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Maximum fuel text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxOilStockTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(maxOilStockTextBox.Text, out val))
            {
                maxOilStockTextBox.Text = DoubleHelper.ToString(model.MaxOilStock);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.MaxOilStock))
            {
                return;
            }

            Log.Info("[Unit] max oil stock: {0} -> {1} ({2})", DoubleHelper.ToString(model.MaxOilStock),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.MaxOilStock = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.MaxOilStock);
            unit.SetDirtyFile();

            // Change the font color
            maxOilStockTextBox.ForeColor = Color.Red;
        }

        #endregion

        #region Unit model tab ―――― Production status

        /// <summary>
        ///     Update the automatic improvement destination list
        /// </summary>
        private void UpdateAutoUpgradeClassList()
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            Graphics g = Graphics.FromHwnd(autoUpgradeClassComboBox.Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;
            autoUpgradeClassComboBox.BeginUpdate();
            autoUpgradeClassComboBox.Items.Clear();
            if (model.AutoUpgrade)
            {
                int width = autoUpgradeClassComboBox.Width;
                // If the current automatic improvement destination class does not match the military department, register as a candidate with one shot
                UnitClass current = Units.Items[(int) model.UpgradeClass];
                if (current.Branch != unit.Branch)
                {
                    width = Math.Max(width,
                        (int) g.MeasureString(current.ToString(), autoUpgradeClassComboBox.Font).Width +
                        SystemInformation.VerticalScrollBarWidth + margin);
                    autoUpgradeClassComboBox.Items.Add(current);
                }
                foreach (UnitClass u in Units.UnitTypes
                    .Select(type => Units.Items[(int) type])
                    .Where(u => (u.Branch == unit.Branch) &&
                                (u.Organization == unit.Organization) &&
                                (u.Models.Count > 0)))
                {
                    width = Math.Max(width,
                        (int) g.MeasureString(u.ToString(), autoUpgradeClassComboBox.Font).Width +
                        SystemInformation.VerticalScrollBarWidth + margin);
                    autoUpgradeClassComboBox.Items.Add(u);
                }
                autoUpgradeClassComboBox.DropDownWidth = width;
                autoUpgradeClassComboBox.SelectedItem = Units.Items[(int) model.UpgradeClass];
                autoUpgradeClassComboBox.Enabled = true;
            }
            else
            {
                autoUpgradeClassComboBox.Enabled = false;
                autoUpgradeClassComboBox.SelectedIndex = -1;
                autoUpgradeClassComboBox.ResetText();
            }
            autoUpgradeClassComboBox.EndUpdate();
        }

        /// <summary>
        ///     Update the display of the model to be automatically improved
        /// </summary>
        private void UpdateAutoUpgradeModelList()
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            Graphics g = Graphics.FromHwnd(autoUpgradeModelComboBox.Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;
            autoUpgradeModelComboBox.BeginUpdate();
            autoUpgradeModelComboBox.Items.Clear();
            if (model.AutoUpgrade)
            {
                UnitClass upgrade = Units.Items[(int) model.UpgradeClass];
                int width = autoUpgradeModelComboBox.Width;
                for (int i = 0; i < upgrade.Models.Count; i++)
                {
                    string s = upgrade.GetModelName(i);
                    width = Math.Max(width,
                        (int) g.MeasureString(s, autoUpgradeModelComboBox.Font).Width +
                        SystemInformation.VerticalScrollBarWidth + margin);
                    autoUpgradeModelComboBox.Items.Add(s);
                }
                autoUpgradeModelComboBox.DropDownWidth = width;
                if ((model.UpgradeModel >= 0) && (model.UpgradeModel < upgrade.Models.Count))
                {
                    autoUpgradeModelComboBox.SelectedIndex = model.UpgradeModel;
                }
                else
                {
                    autoUpgradeModelComboBox.SelectedIndex = -1;
                    autoUpgradeModelComboBox.Text = DoubleHelper.ToString(model.UpgradeModel);
                }
                autoUpgradeModelComboBox.Enabled = true;
            }
            else
            {
                autoUpgradeModelComboBox.Enabled = false;
                autoUpgradeModelComboBox.SelectedIndex = -1;
                autoUpgradeModelComboBox.ResetText();
            }
            autoUpgradeModelComboBox.ForeColor = model.IsDirty(UnitModelItemId.UpgradeModel)
                ? Color.Red
                : SystemColors.WindowText;
            autoUpgradeModelComboBox.EndUpdate();
        }

        /// <summary>
        ///     requirement I C Processing after moving the text box focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCostTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(costTextBox.Text, out val))
            {
                costTextBox.Text = DoubleHelper.ToString(model.Cost);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.Cost))
            {
                return;
            }

            Log.Info("[Unit] cost: {0} -> {1} ({2})", DoubleHelper.ToString(model.Cost), DoubleHelper.ToString(val),
                unit.GetModelName(index));

            // Update value
            model.Cost = val;

            // Update the items in the unit model list
            modelListView.Items[index].SubItems[2].Text = DoubleHelper.ToString(val);

            // Set the edited flag
            model.SetDirty(UnitModelItemId.Cost);
            unit.SetDirtyFile();

            // Change the font color
            costTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Required time Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBuildTimeTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(buildTimeTextBox.Text, out val))
            {
                buildTimeTextBox.Text = DoubleHelper.ToString(model.BuildTime);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.BuildTime))
            {
                return;
            }

            Log.Info("[Unit] build time: {0} -> {1} ({2})", DoubleHelper.ToString(model.BuildTime),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.BuildTime = val;

            // Update the items in the unit model list
            modelListView.Items[index].SubItems[3].Text = DoubleHelper.ToString(val);

            // Set the edited flag
            model.SetDirty(UnitModelItemId.BuildTime);
            unit.SetDirtyFile();

            // Change the font color
            buildTimeTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Labor Text Box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnManPowerTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(manPowerTextBox.Text, out val))
            {
                manPowerTextBox.Text = DoubleHelper.ToString(model.ManPower);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.ManPower))
            {
                return;
            }

            Log.Info("[Unit] manpower: {0} -> {1} ({2})", DoubleHelper.ToString(model.ManPower),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.ManPower = val;

            // Update the items in the unit model list
            modelListView.Items[index].SubItems[4].Text = DoubleHelper.ToString(val);

            // Set the edited flag
            model.SetDirty(UnitModelItemId.ManPower);
            unit.SetDirtyFile();

            // Change the font color
            manPowerTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Improved cost Textbox Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeCostFactorTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(upgradeCostFactorTextBox.Text, out val))
            {
                upgradeCostFactorTextBox.Text = DoubleHelper.ToString(model.UpgradeCostFactor);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.UpgradeCostFactor))
            {
                return;
            }

            Log.Info("[Unit] upgrade cost factor: {0} -> {1} ({2})", DoubleHelper.ToString(model.UpgradeCostFactor),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.UpgradeCostFactor = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.UpgradeCostFactor);
            unit.SetDirtyFile();

            // Change the font color
            upgradeCostFactorTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Improvement time Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeTimeFactorTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(upgradeTimeFactorTextBox.Text, out val))
            {
                upgradeTimeFactorTextBox.Text = DoubleHelper.ToString(model.UpgradeTimeFactor);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.UpgradeTimeFactor))
            {
                return;
            }

            Log.Info("[Unit] upgrade time factor: {0} -> {1} ({2})", DoubleHelper.ToString(model.UpgradeTimeFactor),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.UpgradeTimeFactor = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.UpgradeTimeFactor);
            unit.SetDirtyFile();

            // Change the font color
            upgradeTimeFactorTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Replenishment cost Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReinforceCostTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(reinforceCostTextBox.Text, out val))
            {
                reinforceCostTextBox.Text = DoubleHelper.ToString(model.ReinforceCostFactor);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.ReinforceCostFactor))
            {
                return;
            }

            Log.Info("[Unit] reinforce cost: {0} -> {1} ({2})", DoubleHelper.ToString(model.ReinforceCostFactor),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.ReinforceCostFactor = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.ReinforceCostFactor);
            unit.SetDirtyFile();

            // Change the font color
            reinforceCostTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Replenishment time Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReinforceTimeTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(reinforceTimeTextBox.Text, out val))
            {
                reinforceTimeTextBox.Text = DoubleHelper.ToString(model.ReinforceTimeFactor);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.ReinforceTimeFactor))
            {
                return;
            }

            Log.Info("[Unit] reinforce time: {0} -> {1} ({2})", DoubleHelper.ToString(model.ReinforceTimeFactor),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.ReinforceTimeFactor = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.ReinforceTimeFactor);
            unit.SetDirtyFile();

            // Change the font color
            reinforceTimeTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     2 Processing when the check status of the stage improvement check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgradeTimeBoostCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // DH If the unit class selected other than is the Navy Division, do nothing
            if ((Game.Type != GameType.DarkestHour) && (unit.Branch == Branch.Navy) &&
                (unit.Organization == UnitOrganization.Division))
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // Do nothing if the value does not change
            if (upgradeTimeBoostCheckBox.Checked == model.UpgradeTimeBoost)
            {
                return;
            }

            Log.Info("[Unit] upgrade time boost: {0} -> {1} ({2})", BoolHelper.ToString(model.UpgradeTimeBoost),
                BoolHelper.ToString(upgradeTimeBoostCheckBox.Checked), unit.GetModelName(index));

            // Update value
            model.UpgradeTimeBoost = upgradeTimeBoostCheckBox.Checked;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.UpgradeTimeBoost);
            unit.SetDirtyFile();

            // Change the font color
            upgradeTimeBoostCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the check status of the automatic improvement check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAutoUpgradeCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // DH If the unit class selected other than is the Navy Division, do nothing
            if ((Game.Type != GameType.DarkestHour) && (unit.Branch == Branch.Navy) &&
                (unit.Organization == UnitOrganization.Division))
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // Do nothing if the value does not change
            if (autoUpgradeCheckBox.Checked == model.AutoUpgrade)
            {
                return;
            }

            Log.Info("[Unit] auto upgrade: {0} -> {1} ({2})", BoolHelper.ToString(model.AutoUpgrade),
                BoolHelper.ToString(autoUpgradeCheckBox.Checked), unit.GetModelName(index));

            // Update value
            model.AutoUpgrade = autoUpgradeCheckBox.Checked;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.AutoUpgrade);
            unit.SetDirtyFile();

            // Change the font color
            autoUpgradeCheckBox.ForeColor = Color.Red;

            // Update the automatic improvement destination list
            UpdateAutoUpgradeClassList();
            UpdateAutoUpgradeModelList();
        }

        /// <summary>
        ///     Item drawing process of automatic improvement destination class combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAutoUpgradeClassComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int i = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[i];

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            UnitClass u = autoUpgradeClassComboBox.Items[e.Index] as UnitClass;
            if (u != null)
            {
                Brush brush;
                if ((u.Type == model.UpgradeClass) && model.IsDirty(UnitModelItemId.UpgradeClass))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = autoUpgradeClassComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of the automatic improvement destination model combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAutoUpgradeModelComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int i = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[i];

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Brush brush;
            if ((e.Index == model.UpgradeModel) && model.IsDirty(UnitModelItemId.UpgradeModel))
            {
                brush = new SolidBrush(Color.Red);
            }
            else
            {
                brush = new SolidBrush(SystemColors.WindowText);
            }
            string s = autoUpgradeModelComboBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the automatic improvement destination class combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAutoUpgradeClassComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selected item
            if (autoUpgradeClassComboBox.SelectedIndex < 0)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // Do nothing if the value does not change
            UnitClass upgrade = autoUpgradeClassComboBox.SelectedItem as UnitClass;
            if (upgrade == null)
            {
                return;
            }
            if (upgrade.Type == model.UpgradeClass)
            {
                return;
            }

            Log.Info("[Unit] auto upgrade class: {0} -> {1} ({2})", Units.Items[(int) model.UpgradeClass],
                Units.Items[(int) upgrade.Type], unit.GetModelName(index));

            // Update value
            UnitClass old = Units.Items[(int) model.UpgradeClass];
            model.UpgradeClass = upgrade.Type;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.UpgradeClass);
            unit.SetDirtyFile();

            if (old.Branch != unit.Branch)
            {
                // If the auto-improvement destination class and the military department do not match, update the item
                UpdateAutoUpgradeClassList();
            }
            else
            {
                // Update drawing to change the item color of the automatic improvement destination class combo box
                autoUpgradeClassComboBox.Refresh();
            }

            // Update the display of the automatic improvement destination model combo box
            UpdateAutoUpgradeModelList();
        }

        /// <summary>
        ///     Processing when changing the selection item of the automatic improvement destination model combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAutoUpgradeModelComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selected item
            if (autoUpgradeModelComboBox.SelectedIndex < 0)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // Do nothing if the value does not change
            if (autoUpgradeModelComboBox.SelectedIndex == model.UpgradeModel)
            {
                return;
            }

            Log.Info("[Unit] auto upgrade model: {0} -> {1} ({2})",
                Units.Items[(int) model.UpgradeClass].GetModelName(model.UpgradeModel),
                Units.Items[(int) model.UpgradeClass].GetModelName(autoUpgradeModelComboBox.SelectedIndex),
                unit.GetModelName(index));

            // Update value
            model.UpgradeModel = autoUpgradeModelComboBox.SelectedIndex;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.UpgradeModel);
            unit.SetDirtyFile();

            // Change the font color
            autoUpgradeModelComboBox.ForeColor = Color.Red;

            // Update drawing to change the item color of the automatic improvement destination model combo box
            autoUpgradeModelComboBox.Refresh();
        }

        /// <summary>
        ///     Processing after moving the focus of the automatic improvement destination model combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAutoUpgradeModelComboBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is a selected item
            if (autoUpgradeModelComboBox.SelectedIndex >= 0)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            UnitClass upgrade = Units.Items[(int) model.UpgradeClass];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            int val;
            if (!IntHelper.TryParse(autoUpgradeModelComboBox.Text, out val))
            {
                if ((model.UpgradeModel >= 0) && (model.UpgradeModel < upgrade.Models.Count))
                {
                    autoUpgradeModelComboBox.SelectedIndex = model.UpgradeModel;
                }
                else
                {
                    autoUpgradeModelComboBox.SelectedIndex = -1;
                    autoUpgradeModelComboBox.Text = DoubleHelper.ToString(model.UpgradeModel);
                }
                return;
            }

            // Do not update if the value does not change
            if (val == model.UpgradeModel)
            {
                // If there is a selection item, return the number to the selection item
                if ((val >= 0) && (val < upgrade.Models.Count))
                {
                    autoUpgradeModelComboBox.SelectedIndex = model.UpgradeModel;
                }
                return;
            }

            Log.Info("[Unit] auto upgrade model: {0} -> {1} ({2})",
                Units.Items[(int) model.UpgradeClass].GetModelName(model.UpgradeModel),
                Units.Items[(int) model.UpgradeClass].GetModelName(val), unit.GetModelName(index));

            // Update value
            model.UpgradeModel = val;

            // If there is a selection item, return the number to the selection item
            {
                if ((val >= 0) && (val < upgrade.Models.Count))
                {
                    autoUpgradeModelComboBox.SelectedIndex = model.UpgradeModel;
                }
            }

            // Set the edited flag
            model.SetDirty(UnitModelItemId.UpgradeModel);
            unit.SetDirtyFile();

            // Change the font color
            autoUpgradeModelComboBox.ForeColor = Color.Red;

            // Update drawing to change the item color of the automatic improvement destination model combo box
            autoUpgradeModelComboBox.Refresh();
        }

        #endregion

        #region Unit model tab ―――― Speed status

        /// <summary>
        ///     Maximum speed text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxSpeedTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(maxSpeedTextBox.Text, out val))
            {
                maxSpeedTextBox.Text = DoubleHelper.ToString(model.MaxSpeed);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.MaxSpeed))
            {
                return;
            }

            Log.Info("[Unit] max speed: {0} -> {1} ({2})", DoubleHelper.ToString(model.MaxSpeed),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.MaxSpeed = val;

            // Update the items in the unit model list
            modelListView.Items[index].SubItems[9].Text = DoubleHelper.ToString(val);

            // Set the edited flag
            model.SetDirty(UnitModelItemId.MaxSpeed);
            unit.SetDirtyFile();

            // Change the font color
            maxSpeedTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Speed cap Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeedCapTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(speedCapAllTextBox.Text, out val))
            {
                speedCapAllTextBox.Text = DoubleHelper.ToString(model.SpeedCap);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SpeedCap))
            {
                return;
            }

            Log.Info("[Unit] speed cap: {0} -> {1} ({2})", DoubleHelper.ToString(model.SpeedCap),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.SpeedCap = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SpeedCap);
            unit.SetDirtyFile();

            // Change the font color
            speedCapAllTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Artillery Brigade Speed Cap Text Box Processing After Focus Move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeedCapArtTextBox(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(speedCapArtTextBox.Text, out val))
            {
                speedCapArtTextBox.Text = DoubleHelper.ToString(model.SpeedCapArt);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SpeedCapArt))
            {
                return;
            }

            Log.Info("[Unit] speed cap art: {0} -> {1} ({2})", DoubleHelper.ToString(model.SpeedCapArt),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.SpeedCapArt = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SpeedCapArt);
            unit.SetDirtyFile();

            // Change the font color
            speedCapArtTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Engineer Brigade Speed Cap Text Box Processing After Focus Move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeedCapEngTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(speedCapEngTextBox.Text, out val))
            {
                speedCapEngTextBox.Text = DoubleHelper.ToString(model.SpeedCapEng);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SpeedCapEng))
            {
                return;
            }

            Log.Info("[Unit] speed cap eng: {0} -> {1} ({2})", DoubleHelper.ToString(model.SpeedCapEng),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.SpeedCapEng = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SpeedCapEng);
            unit.SetDirtyFile();

            // Change the font color
            speedCapEngTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-tank brigade speed cap text box processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeedCapAtTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(speedCapAtTextBox.Text, out val))
            {
                speedCapAtTextBox.Text = DoubleHelper.ToString(model.SpeedCapAt);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SpeedCapAt))
            {
                return;
            }

            Log.Info("[Unit] speed cap at: {0} -> {1} ({2})", DoubleHelper.ToString(model.SpeedCapAt),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.SpeedCapAt = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SpeedCapAt);
            unit.SetDirtyFile();

            // Change the font color
            speedCapAtTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-aircraft brigade speed cap text box processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeedCapAaTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(speedCapAaTextBox.Text, out val))
            {
                speedCapAaTextBox.Text = DoubleHelper.ToString(model.SpeedCapAa);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SpeedCapAa))
            {
                return;
            }

            Log.Info("[Unit] speed cap aa: {0} -> {1} ({2})", DoubleHelper.ToString(model.SpeedCapAa),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.SpeedCapAa = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SpeedCapAa);
            unit.SetDirtyFile();

            // Change the font color
            speedCapAaTextBox.ForeColor = Color.Red;
        }

        #endregion

        #region Unit model tab ―――― Combat status

        /// <summary>
        ///     Defense text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDefensivenessTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(defensivenessTextBox.Text, out val))
            {
                defensivenessTextBox.Text = DoubleHelper.ToString(model.Defensiveness);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.Defensiveness))
            {
                return;
            }

            Log.Info("[Unit] defensiveness: {0} -> {1} ({2})", DoubleHelper.ToString(model.Defensiveness),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.Defensiveness = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.Defensiveness);
            unit.SetDirtyFile();

            // Change the font color
            defensivenessTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-ship defense text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSeaDefenceTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(seaDefenceTextBox.Text, out val))
            {
                seaDefenceTextBox.Text = DoubleHelper.ToString(model.SeaDefense);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SeaDefense))
            {
                return;
            }

            Log.Info("[Unit] sea defence: {0} -> {1} ({2})", DoubleHelper.ToString(model.SeaDefense),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.SeaDefense = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SeaDefense);
            unit.SetDirtyFile();

            // Change the font color
            seaDefenceTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-aircraft defense text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAirDefenceTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(airDefenceTextBox.Text, out val))
            {
                airDefenceTextBox.Text = DoubleHelper.ToString(model.AirDefence);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.AirDefence))
            {
                return;
            }

            Log.Info("[Unit] air defence: {0} -> {1} ({2})", DoubleHelper.ToString(model.AirDefence),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.AirDefence = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.AirDefense);
            unit.SetDirtyFile();

            // Change the font color
            airDefenceTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Ground defense text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSurfaceDefenceTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(surfaceDefenceTextBox.Text, out val))
            {
                surfaceDefenceTextBox.Text = DoubleHelper.ToString(model.SurfaceDefence);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SurfaceDefence))
            {
                return;
            }

            Log.Info("[Unit] surface defence: {0} -> {1} ({2})", DoubleHelper.ToString(model.SurfaceDefence),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.SurfaceDefence = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SurfaceDefense);
            unit.SetDirtyFile();

            // Change the font color
            surfaceDefenceTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Durability Text Box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnToughnessTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(toughnessTextBox.Text, out val))
            {
                toughnessTextBox.Text = DoubleHelper.ToString(model.Toughness);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.Toughness))
            {
                return;
            }

            Log.Info("[Unit] toughness: {0} -> {1} ({2})", DoubleHelper.ToString(model.Toughness),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.Toughness = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.Toughness);
            unit.SetDirtyFile();

            // Change the font color
            toughnessTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Vulnerability Textbox Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSoftnessTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(softnessTextBox.Text, out val))
            {
                softnessTextBox.Text = DoubleHelper.ToString(model.Softness);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.Softness))
            {
                return;
            }

            Log.Info("[Unit] softness: {0} -> {1} ({2})", DoubleHelper.ToString(model.Softness),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.Softness = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.Softness);
            unit.SetDirtyFile();

            // Change the font color
            softnessTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Interpersonal attack power Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSoftAttackTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(softAttackTextBox.Text, out val))
            {
                softAttackTextBox.Text = DoubleHelper.ToString(model.SoftAttack);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SoftAttack))
            {
                return;
            }

            Log.Info("[Unit] soft attack: {0} -> {1} ({2})", DoubleHelper.ToString(model.SoftAttack),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.SoftAttack = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SoftAttack);
            unit.SetDirtyFile();

            // Change the font color
            softAttackTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-instep attack power Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHardAttackTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(hardAttackTextBox.Text, out val))
            {
                hardAttackTextBox.Text = DoubleHelper.ToString(model.HardAttack);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.HardAttack))
            {
                return;
            }

            Log.Info("[Unit] hard attack: {0} -> {1} ({2})", DoubleHelper.ToString(model.HardAttack),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.HardAttack = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.HardAttack);
            unit.SetDirtyFile();

            // Change the font color
            hardAttackTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-ship attack power Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSeaAttackTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(seaAttackTextBox.Text, out val))
            {
                seaAttackTextBox.Text = DoubleHelper.ToString(model.SeaAttack);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SeaAttack))
            {
                return;
            }

            Log.Info("[Unit] sea attack: {0} -> {1} ({2})", DoubleHelper.ToString(model.SeaAttack),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.SeaAttack = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SeaAttack);
            unit.SetDirtyFile();

            // Change the font color
            seaAttackTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-submarine attack power Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSubAttackTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(subAttackTextBox.Text, out val))
            {
                subAttackTextBox.Text = DoubleHelper.ToString(model.SubAttack);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SubAttack))
            {
                return;
            }

            Log.Info("[Unit] sub attack: {0} -> {1} ({2})", DoubleHelper.ToString(model.SubAttack),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.SubAttack = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SubAttack);
            unit.SetDirtyFile();

            // Change the font color
            subAttackTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Fleet attack power Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConvoyAttackTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(convoyAttackTextBox.Text, out val))
            {
                convoyAttackTextBox.Text = DoubleHelper.ToString(model.ConvoyAttack);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.ConvoyAttack))
            {
                return;
            }

            Log.Info("[Unit] convoy attack: {0} -> {1} ({2})", DoubleHelper.ToString(model.ConvoyAttack),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.ConvoyAttack = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.ConvoyAttack);
            unit.SetDirtyFile();

            // Change the font color
            convoyAttackTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Coastal artillery ability Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShoreBombardmentTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (
                !DoubleHelper.TryParse(shoreBombardmentTextBox.Text, out val))
            {
                shoreBombardmentTextBox.Text = DoubleHelper.ToString(model.ShoreBombardment);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.ShoreBombardment))
            {
                return;
            }

            Log.Info("[Unit] shore bombardment: {0} -> {1} ({2})", DoubleHelper.ToString(model.ShoreBombardment),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.ShoreBombardment = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.ShoreBombardment);
            unit.SetDirtyFile();

            // Change the font color
            shoreBombardmentTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-aircraft attack power Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAirAttackTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(airAttackTextBox.Text, out val))
            {
                airAttackTextBox.Text = DoubleHelper.ToString(model.AirAttack);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.AirAttack))
            {
                return;
            }

            Log.Info("[Unit] air attack: {0} -> {1} ({2})", DoubleHelper.ToString(model.AirAttack),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.AirAttack = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.AirAttack);
            unit.SetDirtyFile();

            // Change the font color
            airAttackTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Air-to-ship attack power Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavalAttackTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(navalAttackTextBox.Text, out val))
            {
                navalAttackTextBox.Text = DoubleHelper.ToString(model.NavalAttack);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.NavalAttack))
            {
                return;
            }

            Log.Info("[Unit] naval attack: {0} -> {1} ({2})", DoubleHelper.ToString(model.NavalAttack),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.NavalAttack = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.NavalAttack);
            unit.SetDirtyFile();

            // Change the font color
            navalAttackTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Strategic bombing attack power Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStrategicAttackTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(strategicAttackTextBox.Text, out val))
            {
                strategicAttackTextBox.Text = DoubleHelper.ToString(model.StrategicAttack);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.StrategicAttack))
            {
                return;
            }

            Log.Info("[Unit] strategic attack: {0} -> {1} ({2})", DoubleHelper.ToString(model.StrategicAttack),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.StrategicAttack = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.StrategicAttack);
            unit.SetDirtyFile();

            // Change the font color
            strategicAttackTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Shooting ability Text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnArtilleryBombardmentTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(artilleryBombardmentTextBox.Text, out val))
            {
                artilleryBombardmentTextBox.Text = DoubleHelper.ToString(model.ArtilleryBombardment);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.ArtilleryBombardment))
            {
                return;
            }

            Log.Info("[Unit] artillery bombardment: {0} -> {1} ({2})", DoubleHelper.ToString(model.ArtilleryBombardment),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.ArtilleryBombardment = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.ArtilleryBombardment);
            unit.SetDirtyFile();

            // Change the font color
            artilleryBombardmentTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Range text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDistanceTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(distanceTextBox.Text, out val))
            {
                distanceTextBox.Text = DoubleHelper.ToString(model.Distance);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.Distance))
            {
                return;
            }

            Log.Info("[Unit] distance: {0} -> {1} ({2})", DoubleHelper.ToString(model.Distance),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.Distance = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.Distance);
            unit.SetDirtyFile();

            // Change the font color
            distanceTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Visibility Text Box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVisibilityTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(visibilityTextBox.Text, out val))
            {
                visibilityTextBox.Text = DoubleHelper.ToString(model.Visibility);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.Visibility))
            {
                return;
            }

            Log.Info("[Unit] visibility: {0} -> {1} ({2})", DoubleHelper.ToString(model.Visibility),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.Visibility = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.Visibility);
            unit.SetDirtyFile();

            // Change the font color
            visibilityTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Ground search Enemy text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSurfaceDetectionCapabilityTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(surfaceDetectionCapabilityTextBox.Text, out val))
            {
                surfaceDetectionCapabilityTextBox.Text = DoubleHelper.ToString(model.SurfaceDetectionCapability);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SurfaceDetectionCapability))
            {
                return;
            }

            Log.Info("[Unit] surface detection capability: {0} -> {1} ({2})",
                DoubleHelper.ToString(model.SurfaceDetectionCapability), DoubleHelper.ToString(val),
                unit.GetModelName(index));

            // Update value
            model.SurfaceDetectionCapability = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SurfaceDetectionCapability);
            unit.SetDirtyFile();

            // Change the font color
            surfaceDetectionCapabilityTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-submarine enemy power text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSubDetectionCapabilityTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(subDetectionCapabilityTextBox.Text, out val))
            {
                subDetectionCapabilityTextBox.Text = DoubleHelper.ToString(model.SubDetectionCapability);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.SubDetectionCapability))
            {
                return;
            }

            Log.Info("[Unit] sub detection capability: {0} -> {1} ({2})",
                DoubleHelper.ToString(model.SubDetectionCapability), DoubleHelper.ToString(val),
                unit.GetModelName(index));

            // Update value
            model.SubDetectionCapability = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.SubDetectionCapability);
            unit.SetDirtyFile();

            // Change the font color
            subDetectionCapabilityTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-aircraft enemy power text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAirDetectionCapabilityTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(airDetectionCapabilityTextBox.Text, out val))
            {
                airDetectionCapabilityTextBox.Text = DoubleHelper.ToString(model.AirDetectionCapability);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.AirDetectionCapability))
            {
                return;
            }

            Log.Info("[Unit] air detection capablity: {0} -> {1} ({2})",
                DoubleHelper.ToString(model.AirDetectionCapability), DoubleHelper.ToString(val),
                unit.GetModelName(index));

            // Update value
            model.AirDetectionCapability = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.AirDetectionCapability);
            unit.SetDirtyFile();

            // Change the font color
            airDetectionCapabilityTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Fuel shortage correction text box Processing after focus movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNoFuelCombatModTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(noFuelCombatModTextBox.Text, out val))
            {
                noFuelCombatModTextBox.Text = DoubleHelper.ToString(model.NoFuelCombatMod);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, model.NoFuelCombatMod))
            {
                return;
            }

            Log.Info("[Unit] no fuel combat mod: {0} -> {1} ({2})", DoubleHelper.ToString(model.NoFuelCombatMod),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            model.NoFuelCombatMod = val;

            // Set the edited flag
            model.SetDirty(UnitModelItemId.NoFuelCombatMod);
            unit.SetDirtyFile();

            // Change the font color
            noFuelCombatModTextBox.ForeColor = Color.Red;
        }

        #endregion

        #region Unit model tab ―――― Equipment

        /// <summary>
        ///     Update items in the equipment list view
        /// </summary>
        /// <param name="model"></param>
        private void UpdateEquipmentList(UnitModel model)
        {
            // Register items in order
            equipmentListView.BeginUpdate();
            equipmentListView.Items.Clear();
            foreach (UnitEquipment equipment in model.Equipments)
            {
                equipmentListView.Items.Add(CreateEquipmentListItem(equipment));
            }
            equipmentListView.EndUpdate();

            // Disable edit items if there are no items
            if (model.Equipments.Count == 0)
            {
                DisableEquipmentItems();
                return;
            }

            // Select the first item
            equipmentListView.Items[0].Focused = true;
            equipmentListView.Items[0].Selected = true;

            // Enable edit items
            EnableEquipmentItems();
        }

        /// <summary>
        ///     Enable edit items for equipment
        /// </summary>
        private void EnableEquipmentItems()
        {
            resourceComboBox.Enabled = true;
            quantityTextBox.Enabled = true;

            equipmentRemoveButton.Enabled = true;
            equipmentUpButton.Enabled = true;
            equipmentDownButton.Enabled = true;
        }

        /// <summary>
        ///     Disable equipment edit items
        /// </summary>
        private void DisableEquipmentItems()
        {
            resourceComboBox.Enabled = false;
            quantityTextBox.Enabled = false;

            resourceComboBox.SelectedIndex = -1;
            resourceComboBox.ResetText();
            quantityTextBox.ResetText();

            equipmentRemoveButton.Enabled = false;
            equipmentUpButton.Enabled = false;
            equipmentDownButton.Enabled = false;
        }

        /// <summary>
        ///     Item drawing process of resource combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResourceComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int i = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[i];

            // Do nothing if there is no selection
            if (equipmentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = equipmentListView.SelectedIndices[0];
            UnitEquipment equipment = model.Equipments[index];

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Brush brush;
            if ((e.Index == (int) equipment.Resource) && equipment.IsDirty(UnitEquipmentItemId.Resource))
            {
                brush = new SolidBrush(Color.Red);
            }
            else
            {
                brush = new SolidBrush(SystemColors.WindowText);
            }
            string s = resourceComboBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item in the equipment list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEquipmentListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int i = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[i];

            // Do nothing if there is no selection
            if (equipmentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = equipmentListView.SelectedIndices[0];
            UnitEquipment equipment = model.Equipments[index];

            // Update the value of the edit item
            resourceComboBox.SelectedIndex = (int) equipment.Resource;
            quantityTextBox.Text = DoubleHelper.ToString(equipment.Quantity);

            // Update the color of the edit item
            quantityTextBox.ForeColor = equipment.IsDirty(UnitEquipmentItemId.Quantity)
                ? Color.Red
                : SystemColors.WindowText;

            // Enable edit items
            EnableEquipmentItems();
        }

        /// <summary>
        ///     Processing when changing the width of columns in the equipment list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEquipmentListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < EquipmentListColumnCount))
            {
                HoI2EditorController.Settings.UnitEditor.EquipmentListColumnWidth[e.ColumnIndex] =
                    equipmentListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Processing before editing items in the equipment list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEquipmentListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // resource
                    e.Type = ItemEditType.List;
                    e.Items = resourceComboBox.Items.Cast<string>();
                    e.Index = resourceComboBox.SelectedIndex;
                    e.DropDownWidth = resourceComboBox.DropDownWidth;
                    break;

                case 1: // amount
                    e.Type = ItemEditType.Text;
                    e.Text = quantityTextBox.Text;
                    break;
            }
        }

        /// <summary>
        ///     Processing after editing items in the equipment list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEquipmentListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // resource
                    resourceComboBox.SelectedIndex = e.Index;
                    break;

                case 1: // amount
                    quantityTextBox.Text = e.Text;
                    OnQuantityTextBoxValidated(quantityTextBox, new EventArgs());
                    break;
            }

            // Since the items in the list view will be updated by yourself, it will be treated as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Processing when replacing items in the equipment list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEquipmentListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[index];

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;

            // Move equipment information
            UnitEquipment equipment = model.Equipments[srcIndex];
            model.Equipments.Insert(destIndex, equipment);
            if (srcIndex < destIndex)
            {
                model.Equipments.RemoveAt(srcIndex);
            }
            else
            {
                model.Equipments.RemoveAt(srcIndex + 1);
            }

            Log.Info("[Unit] Moved equipment: {0} -> {1} {2} [{3}]", srcIndex, destIndex,
                Config.GetText(Units.EquipmentNames[(int) equipment.Resource]), unit.GetModelName(index));

            // Set the edited flag
            equipment.SetDirty();
            model.SetDirty();
            unit.SetDirtyFile();
        }

        /// <summary>
        ///     Processing when changing the selection item of the resource combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResourceComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int i = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[i];

            // Do nothing if there is no selection
            if (equipmentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = equipmentListView.SelectedIndices[0];
            UnitEquipment equipment = model.Equipments[index];

            // Do nothing if the value does not change
            EquipmentType type = (EquipmentType) resourceComboBox.SelectedIndex;
            if (type == equipment.Resource)
            {
                return;
            }

            Log.Info("[Unit] equipment resource: {0} -> {1} ({2})",
                Config.GetText(Units.EquipmentNames[(int) equipment.Resource]),
                Config.GetText(Units.EquipmentNames[(int) type]), unit.GetModelName(index));

            // Update value
            equipment.Resource = type;

            // Update items in the equipment list view
            equipmentListView.Items[index].Text = Config.GetText(Units.EquipmentNames[(int) type]);

            // Set the edited flag
            equipment.SetDirty(UnitEquipmentItemId.Resource);
            model.SetDirty();
            unit.SetDirtyFile();

            // Update drawing to change the item color of the resource combo box
            resourceComboBox.Refresh();
        }

        /// <summary>
        ///     Processing after moving the focus of the amount text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnQuantityTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int i = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[i];

            // Do nothing if there is no selection
            if (equipmentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = equipmentListView.SelectedIndices[0];
            UnitEquipment equipment = model.Equipments[index];

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(quantityTextBox.Text, out val))
            {
                quantityTextBox.Text = DoubleHelper.ToString(equipment.Quantity);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, equipment.Quantity))
            {
                return;
            }

            Log.Info("[Unit] equipment quantity: {0} -> {1} ({2})", DoubleHelper.ToString(equipment.Quantity),
                DoubleHelper.ToString(val), unit.GetModelName(index));

            // Update value
            equipment.Quantity = val;

            // Update items in the equipment list view
            equipmentListView.Items[index].SubItems[1].Text = quantityTextBox.Text;

            // Set the edited flag
            equipment.SetDirty(UnitEquipmentItemId.Quantity);
            model.SetDirty();
            unit.SetDirtyFile();

            // Change the font color
            quantityTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the add button of equipment is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEquipmentAddButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int i = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[i];

            Log.Info("[Unit] Added new equipment: ({0})", unit.GetModelName(i));

            // Add an item to the equipment list
            UnitEquipment equipment = new UnitEquipment();
            model.Equipments.Add(equipment);

            // Set the edited flag
            equipment.SetDirtyAll();
            model.SetDirty();
            unit.SetDirtyFile();

            // Add an item to the equipment list view
            AddEquipmentListItem(equipment);
        }

        /// <summary>
        ///     Processing when the equipment delete button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEquipmentRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int i = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[i];

            // Do nothing if there is no selection
            if (equipmentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = equipmentListView.SelectedIndices[0];

            Log.Info("[Unit] Removed equipment: {0} ({1})",
                Config.GetText(Units.EquipmentNames[(int) model.Equipments[index].Resource]), unit.GetModelName(i));

            // Remove an item from the equipment list
            model.Equipments.RemoveAt(index);

            // Set the edited flag
            model.SetDirty();
            unit.SetDirtyFile();

            // Remove an item from the equipment list view
            RemoveEquipmentListItem(index);
        }

        /// <summary>
        ///     Processing when the button is pressed on the equipment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEquipmentUpButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int i = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[i];

            // Do nothing if there is no selection
            if (equipmentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = equipmentListView.SelectedIndices[0];

            // Do nothing at the top of the list
            if (index == 0)
            {
                return;
            }

            // Move items in the equipment list
            model.MoveEquipment(index, index - 1);

            // Set the edited flag
            model.SetDirty();
            unit.SetDirtyFile();

            // Move items in the equipment list view
            MoveEquipmentListItem(index, index - 1);
        }

        /// <summary>
        ///     Processing when the button is pressed under the equipment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEquipmentDownButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no unit class selected
            UnitClass unit = classListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            // Do nothing if there is no unit model selected
            if (modelListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int i = modelListView.SelectedIndices[0];
            UnitModel model = unit.Models[i];

            // Do nothing if there is no selection
            if (equipmentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = equipmentListView.SelectedIndices[0];

            // Do nothing at the end of the list
            if (index == equipmentListView.Items.Count - 1)
            {
                return;
            }

            // Move items in the equipment list
            model.MoveEquipment(index, index + 1);

            // Set the edited flag
            model.SetDirty();
            unit.SetDirtyFile();

            // Move items in the equipment list view
            MoveEquipmentListItem(index, index + 1);
        }

        /// <summary>
        ///     Create an item in the equipment list
        /// </summary>
        /// <param name="equipment">Equipment</param>
        /// <returns>Equipment list items</returns>
        private static ListViewItem CreateEquipmentListItem(UnitEquipment equipment)
        {
            ListViewItem item = new ListViewItem
            {
                Text = Config.GetText(Units.EquipmentNames[(int) equipment.Resource])
            };
            item.SubItems.Add(DoubleHelper.ToString(equipment.Quantity));

            return item;
        }

        /// <summary>
        ///     Add an item in the equipment list view
        /// </summary>
        /// <param name="equipment">Equipment to be added</param>
        private void AddEquipmentListItem(UnitEquipment equipment)
        {
            // Add an item to the equipment list view
            ListViewItem item = CreateEquipmentListItem(equipment);
            equipmentListView.Items.Add(item);

            // Select the added item
            int index = equipmentListView.Items.Count - 1;
            equipmentListView.Items[index].Focused = true;
            equipmentListView.Items[index].Selected = true;
            equipmentListView.EnsureVisible(index);

            // Enable edit items
            EnableEquipmentItems();
        }

        /// <summary>
        ///     Remove an item from the equipment list view
        /// </summary>
        /// <param name="index">Position of the item to be deleted</param>
        private void RemoveEquipmentListItem(int index)
        {
            // Remove an item from the equipment list view
            equipmentListView.Items.RemoveAt(index);

            if (index < equipmentListView.Items.Count)
            {
                // Select next to the deleted item
                equipmentListView.Items[index].Focused = true;
                equipmentListView.Items[index].Selected = true;
            }
            else if (index > 0)
            {
                // Select the last item
                equipmentListView.Items[equipmentListView.Items.Count - 1].Focused = true;
                equipmentListView.Items[equipmentListView.Items.Count - 1].Selected = true;
            }
            else
            {
                // Disable edit items when there are no more items
                DisableEquipmentItems();
            }
        }

        /// <summary>
        ///     Move items in the equipment list
        /// </summary>
        /// <param name="src">Source position</param>
        /// <param name="dest">Destination position</param>
        private void MoveEquipmentListItem(int src, int dest)
        {
            ListViewItem item = equipmentListView.Items[src].Clone() as ListViewItem;
            if (item == null)
            {
                return;
            }

            if (src > dest)
            {
                // When moving up
                equipmentListView.Items.Insert(dest, item);
                equipmentListView.Items.RemoveAt(src + 1);
            }
            else
            {
                // When moving down
                equipmentListView.Items.Insert(dest + 1, item);
                equipmentListView.Items.RemoveAt(src);
            }

            // Select the item to move to
            equipmentListView.Items[dest].Focused = true;
            equipmentListView.Items[dest].Selected = true;
            equipmentListView.EnsureVisible(dest);
        }

        #endregion
    }

    /// <summary>
    ///     Unit editor tab number
    /// </summary>
    public enum UnitEditorTab
    {
        Class, // Unit class
        Model // Unit model
    }
}
