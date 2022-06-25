using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HoI2Editor.Models;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Unit name editor form
    /// </summary>
    public partial class UnitNameEditorForm : Form
    {
        #region Internal field

        /// <summary>
        ///     Prefix history
        /// </summary>
        private readonly History _prefixHistory = new History(HistorySize);

        /// <summary>
        ///     History of suffixes
        /// </summary>
        private readonly History _suffixHistory = new History(HistorySize);

        /// <summary>
        ///     Replacement source history
        /// </summary>
        private readonly History _toHistory = new History(HistorySize);

        /// <summary>
        ///     Replacement history
        /// </summary>
        private readonly History _withHistory = new History(HistorySize);

        #endregion

        #region Internal constant

        /// <summary>
        ///     Maximum number of histories
        /// </summary>
        private const int HistorySize = 10;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public UnitNameEditorForm()
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
            // Update the display of the unit name list
            UpdateNameList();

            // Update the display as the edited flag is cleared
            countryListBox.Refresh();
            typeListBox.Refresh();
        }

        /// <summary>
        ///     Processing after data storage
        /// </summary>
        public void OnFileSaved()
        {
            // Update the display as the edited flag is cleared
            countryListBox.Refresh();
            typeListBox.Refresh();
        }

        /// <summary>
        ///     Processing after changing edit items
        /// </summary>
        /// <param name="id">Edit items ID</param>
        public void OnItemChanged(EditorItemId id)
        {
            switch (id)
            {
                case EditorItemId.UnitName:
                    Log.Verbose("[UnitName] Changed unit name");
                    // Update the display items in the unit type list box
                    UpdateTypeListBox();
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
            // National list box
            countryListBox.ItemHeight = DeviceCaps.GetScaledHeight(countryListBox.ItemHeight);
            // Unit type list box
            typeListBox.ItemHeight = DeviceCaps.GetScaledHeight(typeListBox.ItemHeight);

            // Window position
            Location = HoI2EditorController.Settings.UnitNameEditor.Location;
            Size = HoI2EditorController.Settings.UnitNameEditor.Size;
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

            // Initialize unit name data
            UnitNames.Init();

            // Read the character string definition file
            Config.Load();

            // Initialize the national list box
            InitCountryListBox();

            // Initialize the unit type list box
            InitTypeListBox();

            // Initialize history
            InitHistory();

            // Initialize option settings
            InitOption();

            // Read the unit name definition file
            UnitNames.Load();

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
            HoI2EditorController.OnUnitNameEditorFormClosed();
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
                HoI2EditorController.Settings.UnitNameEditor.Location = Location;
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
                HoI2EditorController.Settings.UnitNameEditor.Size = Size;
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

        #region National list box

        /// <summary>
        ///     Initialize the national list box
        /// </summary>
        private void InitCountryListBox()
        {
            countryListBox.BeginUpdate();
            countryListBox.Items.Clear();
            foreach (string s in Countries.Tags
                .Select(country => Countries.Strings[(int) country])
                .Select(name => Config.ExistsKey(name)
                    ? $"{name} {Config.GetText(name)}"
                    : name))
            {
                countryListBox.Items.Add(s);
            }

            // Reflect the selected nation
            int index = HoI2EditorController.Settings.UnitNameEditor.Country;
            if ((index < 0) || (index >= countryListBox.Items.Count))
            {
                index = 0;
            }
            countryListBox.SelectedIndex = index;

            countryListBox.EndUpdate();
        }

        /// <summary>
        ///     Item drawing process of national list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Brush brush;
            if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
            {
                // Change the text color for items that have changed
                Country country = Countries.Tags[e.Index];
                brush = UnitNames.IsDirty(country) ? new SolidBrush(Color.Red) : new SolidBrush(SystemColors.WindowText);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = countryListBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the national list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the display of the unit name list
            UpdateNameList();

            // Update the display of the unit type list box as the edited flag changes.
            typeListBox.Refresh();

            // Save the selected nation
            HoI2EditorController.Settings.UnitNameEditor.Country = countryListBox.SelectedIndex;
        }

        #endregion

        #region Unit type list box

        /// <summary>
        ///     Initialize the unit type list box
        /// </summary>
        private void InitTypeListBox()
        {
            bool isComplemented = false ;
            typeListBox.BeginUpdate();
            typeListBox.Items.Clear();

            foreach (UnitNameType type in UnitNames.Types)
            {
                string UnitType = Config.GetText(UnitNames.TypeNames[(int)type],ref isComplemented);   /* テキストとテキストの辞書内の状況を取得 */
                if(!isComplemented)
                {
                    typeListBox.Items.Add(UnitType);
                }
                else
                {
                    //UnitType = string.Empty;
                    UnitType = UnitNames.TypeNames[(int)type].Remove(0, 5).ToLower();
                    typeListBox.Items.Add(UnitType);
                }
            }

            // Reflects the selected unit type
            int index = HoI2EditorController.Settings.UnitNameEditor.UnitType;
            if ((index < 0) || (index >= typeListBox.Items.Count))
            {
                index = 0;
            }
            typeListBox.SelectedIndex = index;

            typeListBox.EndUpdate();
        }

        /// <summary>
        ///     Update the display items in the unit type list box
        /// </summary>
        private void UpdateTypeListBox()
        {
            typeListBox.BeginUpdate();
            int top = typeListBox.TopIndex;
            int i = 0;
            foreach (UnitNameType type in UnitNames.Types)
            {
                typeListBox.Items[i] = Config.GetText(UnitNames.TypeNames[(int) type]);
                i++;
            }
            typeListBox.TopIndex = top;
            typeListBox.EndUpdate();
        }

        /// <summary>
        ///     Item drawing process of unit type list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTypeListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Brush brush;
            if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
            {
                // Change the text color for items that have changed
                Country country = Countries.Tags[countryListBox.SelectedIndex];
                UnitNameType type = UnitNames.Types[e.Index];
                brush = UnitNames.IsDirty(country, type)
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(SystemColors.WindowText);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = typeListBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item in the unit type list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTypeListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the display of the unit name list
            UpdateNameList();

            // Save the selected unit type
            HoI2EditorController.Settings.UnitNameEditor.UnitType = typeListBox.SelectedIndex;
        }

        #endregion

        #region Unit name list

        /// <summary>
        ///     Update the unit name list
        /// </summary>
        private void UpdateNameList()
        {
            nameTextBox.Clear();

            // Return if there is no selected nation
            if (countryListBox.SelectedIndex < 0)
            {
                return;
            }
            Country country = Countries.Tags[countryListBox.SelectedIndex];

            // Return if there is no selected unit name type
            if (typeListBox.SelectedIndex < 0)
            {
                return;
            }
            UnitNameType type = UnitNames.Types[typeListBox.SelectedIndex];

            // Add unit names in order
            StringBuilder sb = new StringBuilder();
            foreach (string name in UnitNames.GetNames(country, type))
            {
                sb.AppendLine(name);
            }

            nameTextBox.Text = sb.ToString();
        }

        /// <summary>
        ///     Processing when changing the unit name list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNameTextBoxValidated(object sender, EventArgs e)
        {
            // Return if there is no selected nation
            if (countryListBox.SelectedIndex < 0)
            {
                return;
            }
            Country country = Countries.Tags[countryListBox.SelectedIndex];

            // Return if there is no selected unit name type
            if (typeListBox.SelectedIndex < 0)
            {
                return;
            }
            UnitNameType type = UnitNames.Types[typeListBox.SelectedIndex];

            // Update the unit name list
            UnitNames.SetNames(nameTextBox.Lines.Where(line => !string.IsNullOrEmpty(line)).ToList(), country, type);

            // Update the display of the national list box because the edited flag is updated
            countryListBox.Refresh();
            typeListBox.Refresh();
        }

        #endregion

        #region Editing function

        /// <summary>
        ///     Processing when the undo button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUndoButtonClick(object sender, EventArgs e)
        {
            nameTextBox.Undo();
        }

        /// <summary>
        ///     Processing when the cut button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCutButtonClick(object sender, EventArgs e)
        {
            nameTextBox.Cut();
        }

        /// <summary>
        ///     Processing when the copy button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCopyButtonClick(object sender, EventArgs e)
        {
            nameTextBox.Copy();
        }

        /// <summary>
        ///     Processing when the paste button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPasteButtonClick(object sender, EventArgs e)
        {
            nameTextBox.Paste();
        }

        /// <summary>
        ///     Processing when the replace button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReplaceButtonClick(object sender, EventArgs e)
        {
            string to = toComboBox.Text;
            string with = withComboBox.Text;

            Log.Info("[UnitName] Replace: {0} -> {1}", to, with);

            if (allCountryCheckBox.Checked)
            {
                if (allUnitTypeCheckBox.Checked)
                {
                    // Replace all unit names
                    UnitNames.ReplaceAll(to, with, regexCheckBox.Checked);
                }
                else
                {
                    // Return if there is no selection item in the unit name type list box
                    if (typeListBox.SelectedIndex < 0)
                    {
                        return;
                    }
                    // Replace unit names in all countries
                    UnitNames.ReplaceAllCountries(to, with,
                        UnitNames.Types[typeListBox.SelectedIndex], regexCheckBox.Checked);
                }
            }
            else
            {
                // Return if there is no selection in the national list box
                if (countryListBox.SelectedIndex < 0)
                {
                    return;
                }
                if (allUnitTypeCheckBox.Checked)
                {
                    // Replace unit names for all unit name types
                    UnitNames.ReplaceAllTypes(to, with,
                        Countries.Tags[countryListBox.SelectedIndex], regexCheckBox.Checked);
                }
                else
                {
                    // Return if there is no selection item in the unit name type list box
                    if (typeListBox.SelectedIndex < 0)
                    {
                        return;
                    }
                    // Replace unit name
                    UnitNames.Replace(to, with, Countries.Tags[countryListBox.SelectedIndex],
                        UnitNames.Types[typeListBox.SelectedIndex], regexCheckBox.Checked);
                }
            }

            // Update the display of the unit name list
            UpdateNameList();

            // Update the display of the national list box because the edited flag is updated
            countryListBox.Refresh();
            typeListBox.Refresh();

            // Update history
            _toHistory.Add(to);
            _withHistory.Add(with);

            HoI2EditorController.Settings.UnitNameEditor.ToHistory = _toHistory.Get().ToList();
            HoI2EditorController.Settings.UnitNameEditor.WithHistory = _withHistory.Get().ToList();

            // Update history combo box
            UpdateReplaceHistory();
        }

        /// <summary>
        ///     Processing when the add button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddButtonClick(object sender, EventArgs e)
        {
            // Return if there is no selected nation
            if (countryListBox.SelectedIndex < 0)
            {
                return;
            }
            Country country = Countries.Tags[countryListBox.SelectedIndex];

            // Return if there is no selected unit name type
            if (typeListBox.SelectedIndex < 0)
            {
                return;
            }
            UnitNameType type = UnitNames.Types[typeListBox.SelectedIndex];

            string prefix = prefixComboBox.Text;
            string suffix = suffixComboBox.Text;
            int start = (int) startNumericUpDown.Value;
            int end = (int) endNumericUpDown.Value;

            Log.Info("[UnitName] Add: {0}-{1} {2} {3} [{4}] <{5}>", start, end, prefix, suffix,
                Config.GetText(UnitNames.TypeNames[(int) type]), Countries.Strings[(int) country]);

            // Add unit names at once
            UnitNames.AddSequential(prefix, suffix, start, end, country, type);

            // Update the display of the unit name list
            UpdateNameList();

            // Update the display of the national list box because the edited flag is updated
            countryListBox.Refresh();
            typeListBox.Refresh();

            // Update history
            _prefixHistory.Add(prefix);
            _suffixHistory.Add(suffix);

            HoI2EditorController.Settings.UnitNameEditor.PrefixHistory = _prefixHistory.Get().ToList();
            HoI2EditorController.Settings.UnitNameEditor.SuffixHistory = _suffixHistory.Get().ToList();

            // Update history combo box
            UpdateAddHistory();
        }

        /// <summary>
        ///     Processing when the interpolation button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInterpolateButtonClick(object sender, EventArgs e)
        {
            Log.Info("[UnitName] Interpolate");

            if (allCountryCheckBox.Checked)
            {
                if (allUnitTypeCheckBox.Checked)
                {
                    // Interpolate all unit names
                    UnitNames.InterpolateAll();
                }
                else
                {
                    // Return if there is no selection item in the unit name type list box
                    if (typeListBox.SelectedIndex < 0)
                    {
                        return;
                    }
                    // Interpolate unit names in all countries
                    UnitNames.InterpolateAllCountries(UnitNames.Types[typeListBox.SelectedIndex]);
                }
            }
            else
            {
                // Return if there is no selection in the national list box
                if (countryListBox.SelectedIndex < 0)
                {
                    return;
                }
                if (allUnitTypeCheckBox.Checked)
                {
                    // Interpolate unit names for all unit name types
                    UnitNames.InterpolateAllTypes(Countries.Tags[countryListBox.SelectedIndex]);
                }
                else
                {
                    // Return if there is no selection item in the unit name type list box
                    if (typeListBox.SelectedIndex < 0)
                    {
                        return;
                    }
                    // Interpolate the unit name
                    UnitNames.Interpolate(Countries.Tags[countryListBox.SelectedIndex],
                        UnitNames.Types[typeListBox.SelectedIndex]);
                }
            }

            // Update the display of the unit name list
            UpdateNameList();

            // Update the display as the edited flag is updated
            countryListBox.Refresh();
            typeListBox.Refresh();
        }

        /// <summary>
        ///     History initialization
        /// </summary>
        private void InitHistory()
        {
            _toHistory.Set(HoI2EditorController.Settings.UnitNameEditor.ToHistory.ToArray());
            _withHistory.Set(HoI2EditorController.Settings.UnitNameEditor.WithHistory.ToArray());
            _prefixHistory.Set(HoI2EditorController.Settings.UnitNameEditor.PrefixHistory.ToArray());
            _suffixHistory.Set(HoI2EditorController.Settings.UnitNameEditor.SuffixHistory.ToArray());

            UpdateReplaceHistory();
            UpdateAddHistory();

            if (toComboBox.Items.Count > 0)
            {
                toComboBox.SelectedIndex = 0;
            }

            if (withComboBox.Items.Count > 0)
            {
                withComboBox.SelectedIndex = 0;
            }

            if (prefixComboBox.Items.Count > 0)
            {
                prefixComboBox.SelectedIndex = 0;
            }

            if (suffixComboBox.Items.Count > 0)
            {
                suffixComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        ///     Update the replacement history combo box
        /// </summary>
        private void UpdateReplaceHistory()
        {
            toComboBox.Items.Clear();
            foreach (string s in _toHistory.Get())
            {
                toComboBox.Items.Add(s);
            }

            withComboBox.Items.Clear();
            foreach (string s in _withHistory.Get())
            {
                withComboBox.Items.Add(s);
            }
        }

        /// <summary>
        ///     Update additional history combo box
        /// </summary>
        private void UpdateAddHistory()
        {
            prefixComboBox.Items.Clear();
            foreach (string s in _prefixHistory.Get())
            {
                prefixComboBox.Items.Add(s);
            }

            suffixComboBox.Items.Clear();
            foreach (string s in _suffixHistory.Get())
            {
                suffixComboBox.Items.Add(s);
            }
        }

        /// <summary>
        ///     Initialize option settings
        /// </summary>
        private void InitOption()
        {
            allCountryCheckBox.Checked = HoI2EditorController.Settings.UnitNameEditor.ApplyAllCountires;
            allUnitTypeCheckBox.Checked = HoI2EditorController.Settings.UnitNameEditor.ApplyAllUnitTypes;
            regexCheckBox.Checked = HoI2EditorController.Settings.UnitNameEditor.RegularExpression;
        }

        /// <summary>
        ///     Applies to all nations Processing when the check status of the check box changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllCountryCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            HoI2EditorController.Settings.UnitNameEditor.ApplyAllCountires = allCountryCheckBox.Checked;
        }

        /// <summary>
        ///     Applies to all unit types Processing when the check status of the check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllUnitTypeCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            HoI2EditorController.Settings.UnitNameEditor.ApplyAllUnitTypes = allUnitTypeCheckBox.Checked;
        }

        /// <summary>
        ///     Processing when the check status of the regular expression check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRegexCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            HoI2EditorController.Settings.UnitNameEditor.RegularExpression = regexCheckBox.Checked;
        }

        #endregion
    }
}
