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
    ///     Army name editor form
    /// </summary>
    public partial class CorpsNameEditorForm : Form
    {
        #region Internal field

        /// <summary>
        ///     Prefix history
        /// </summary>
        private readonly History _prefixHistory = new History(HistorySize);

        /// <summary>
        ///     History at the time of equipment
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
        public CorpsNameEditorForm()
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
            // Update the display of the corps name list
            UpdateNameList();

            // Update the display as the edited flag is cleared
            branchListBox.Refresh();
            countryListBox.Refresh();
        }

        /// <summary>
        ///     Processing after data storage
        /// </summary>
        public void OnFileSaved()
        {
            // Update the display as the edited flag is cleared
            branchListBox.Refresh();
            countryListBox.Refresh();
        }

        /// <summary>
        ///     Processing after changing edit items
        /// </summary>
        /// <param name="id">Edit items ID</param>
        public void OnItemChanged(EditorItemId id)
        {
            // do nothing
        }

        #endregion

        #region Form

        /// <summary>
        ///     Form initialization
        /// </summary>
        private void InitForm()
        {
            // Army list box
            branchListBox.ItemHeight = DeviceCaps.GetScaledHeight(branchListBox.ItemHeight);
            // National list box
            countryListBox.ItemHeight = DeviceCaps.GetScaledHeight(countryListBox.ItemHeight);

            // Window position
            Location = HoI2EditorController.Settings.CorpsNameEditor.Location;
            Size = HoI2EditorController.Settings.CorpsNameEditor.Size;
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

            // Read the character string definition file
            Config.Load();

            // Initialize the military list box
            InitBranchListBox();

            // Initialize the national list box
            InitCountryListBox();

            // Initialize history
            InitHistory();

            // Initialize option settings
            InitOption();

            // Read the corps name definition file
            CorpsNames.Load();

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
            HoI2EditorController.OnCorpsNameEditorFormClosed();
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
                HoI2EditorController.Settings.CorpsNameEditor.Location = Location;
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
                HoI2EditorController.Settings.CorpsNameEditor.Size = Size;
            }
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

        #endregion

        #region Army list box

        /// <summary>
        ///     Initialize the military list box
        /// </summary>
        private void InitBranchListBox()
        {
            branchListBox.BeginUpdate();
            branchListBox.Items.Clear();
            branchListBox.Items.Add(Config.GetText("EYR_ARMY"));
            branchListBox.Items.Add(Config.GetText("EYR_NAVY"));
            branchListBox.Items.Add(Config.GetText("EYR_AIRFORCE"));

            // Reflects the selected line
            int index = HoI2EditorController.Settings.CorpsNameEditor.Branch;
            if ((index < 0) || (index >= branchListBox.Items.Count))
            {
                index = 0;
            }
            branchListBox.SelectedIndex = index;

            branchListBox.EndUpdate();
        }

        /// <summary>
        ///     Item drawing process of the military list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBranchListBoxDrawItem(object sender, DrawItemEventArgs e)
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
                Branch branch = (Branch) (e.Index + 1);
                brush = CorpsNames.IsDirty(branch)
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(SystemColors.WindowText);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = branchListBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the military list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBranchListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the display of the corps name list
            UpdateNameList();

            // Update the display of the national list box as the edited flag changes
            countryListBox.Refresh();

            // Save the selected military department
            HoI2EditorController.Settings.CorpsNameEditor.Branch = branchListBox.SelectedIndex;
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
            int index = HoI2EditorController.Settings.CorpsNameEditor.Country;
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
                Branch branch = (Branch) (branchListBox.SelectedIndex + 1);
                Country country = Countries.Tags[e.Index];
                brush = CorpsNames.IsDirty(branch, country)
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(SystemColors.WindowText);
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
            // Update the display of the corps name list
            UpdateNameList();

            // Save the selected nation
            HoI2EditorController.Settings.CorpsNameEditor.Country = countryListBox.SelectedIndex;
        }

        #endregion

        #region Army name list

        /// <summary>
        ///     Update the corps name list
        /// </summary>
        private void UpdateNameList()
        {
            nameTextBox.Clear();

            // Return if there is no selected military department
            if (branchListBox.SelectedIndex < 0)
            {
                return;
            }
            Branch branch = (Branch) (branchListBox.SelectedIndex + 1);

            // Return if there is no selected nation
            if (countryListBox.SelectedIndex < 0)
            {
                return;
            }
            Country country = Countries.Tags[countryListBox.SelectedIndex];

            // Add corps names in order
            StringBuilder sb = new StringBuilder();
            foreach (string name in CorpsNames.GetNames(branch, country))
            {
                sb.AppendLine(name);
            }

            nameTextBox.Text = sb.ToString();
        }

        /// <summary>
        ///     Processing when changing the corps name list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNameTextBoxValidated(object sender, EventArgs e)
        {
            // Return if there is no selected military department
            if (branchListBox.SelectedIndex < 0)
            {
                return;
            }
            Branch branch = (Branch) (branchListBox.SelectedIndex + 1);

            // Return if there is no selected nation
            if (countryListBox.SelectedIndex < 0)
            {
                return;
            }
            Country country = Countries.Tags[countryListBox.SelectedIndex];

            // Update corps name list
            CorpsNames.SetNames(nameTextBox.Lines.Where(line => !string.IsNullOrEmpty(line)).ToList(), branch,
                country);

            // Update the display as the edited flag is updated
            branchListBox.Refresh();
            countryListBox.Refresh();
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

            Log.Info("[CorpsName] Replace: {0} -> {1}", to, with);

            if (allBranchCheckBox.Checked)
            {
                if (allCountryCheckBox.Checked)
                {
                    // Replace all corps names
                    CorpsNames.ReplaceAll(to, with, regexCheckBox.Checked);
                }
                else
                {
                    // Return if there is no selection in the national list box
                    if (countryListBox.SelectedIndex < 0)
                    {
                        return;
                    }
                    Country country = Countries.Tags[countryListBox.SelectedIndex];

                    // Replace all military corps names
                    CorpsNames.ReplaceAllBranches(to, with, country, regexCheckBox.Checked);
                }
            }
            else
            {
                // Return if there is no selection in the military list box
                if (branchListBox.SelectedIndex < 0)
                {
                    return;
                }
                Branch branch = (Branch) (branchListBox.SelectedIndex + 1);

                if (allCountryCheckBox.Checked)
                {
                    // Replace army names in all countries
                    CorpsNames.ReplaceAllCountries(to, with, branch, regexCheckBox.Checked);
                }
                else
                {
                    // Return if there is no selection in the national list box
                    if (countryListBox.SelectedIndex < 0)
                    {
                        return;
                    }
                    Country country = Countries.Tags[countryListBox.SelectedIndex];

                    // Replace the corps name
                    CorpsNames.Replace(to, with, branch, country, regexCheckBox.Checked);
                }
            }

            // Update the display of the corps name list
            UpdateNameList();

            // Update the display as the edited flag is updated
            branchListBox.Refresh();
            countryListBox.Refresh();

            // Update history
            _toHistory.Add(to);
            _withHistory.Add(with);

            HoI2EditorController.Settings.CorpsNameEditor.ToHistory = _toHistory.Get().ToList();
            HoI2EditorController.Settings.CorpsNameEditor.WithHistory = _withHistory.Get().ToList();

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
            // Return if there is no selection in the military list box
            if (branchListBox.SelectedIndex < 0)
            {
                return;
            }
            Branch branch = (Branch) (branchListBox.SelectedIndex + 1);

            // Return if there is no selection in the national list box
            if (countryListBox.SelectedIndex < 0)
            {
                return;
            }
            Country country = Countries.Tags[countryListBox.SelectedIndex];

            string prefix = prefixComboBox.Text;
            string suffix = suffixComboBox.Text;
            int start = (int) startNumericUpDown.Value;
            int end = (int) endNumericUpDown.Value;

            Log.Info("[CorpsName] Add: {0}-{1} {2} {3} [{4}] <{5}>", start, end, prefix, suffix,
                Branches.GetName(branch), Countries.Strings[(int) country]);

            // Add corps names at once
            CorpsNames.AddSequential(prefix, suffix, start, end, branch, country);

            // Update the display of the corps name list
            UpdateNameList();

            // Update the display as the edited flag is updated
            branchListBox.Refresh();
            countryListBox.Refresh();

            // Update history
            _prefixHistory.Add(prefix);
            _suffixHistory.Add(suffix);

            HoI2EditorController.Settings.CorpsNameEditor.PrefixHistory = _prefixHistory.Get().ToList();
            HoI2EditorController.Settings.CorpsNameEditor.SuffixHistory = _suffixHistory.Get().ToList();

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
            Log.Info("[CorpsName] Interpolate");

            if (allBranchCheckBox.Checked)
            {
                if (allCountryCheckBox.Checked)
                {
                    // Interpolate all corps names
                    CorpsNames.InterpolateAll();
                }
                else
                {
                    // Return if there is no selection in the national list box
                    if (countryListBox.SelectedIndex < 0)
                    {
                        return;
                    }
                    Country country = Countries.Tags[countryListBox.SelectedIndex];

                    // Interpolate the names of all military corps
                    CorpsNames.InterpolateAllBranches(country);
                }
            }
            else
            {
                // Return if there is no selection in the military list box
                if (branchListBox.SelectedIndex < 0)
                {
                    return;
                }
                Branch branch = (Branch) (branchListBox.SelectedIndex + 1);

                if (allCountryCheckBox.Checked)
                {
                    // Interpolate the army names of all countries
                    CorpsNames.InterpolateAllCountries(branch);
                }
                else
                {
                    // Return if there is no selection in the national list box
                    if (countryListBox.SelectedIndex < 0)
                    {
                        return;
                    }
                    Country country = Countries.Tags[countryListBox.SelectedIndex];

                    // Interpolate the corps name
                    CorpsNames.Interpolate(branch, country);
                }
            }

            // Update the display of the corps name list
            UpdateNameList();

            // Update the display as the edited flag is updated
            branchListBox.Refresh();
            countryListBox.Refresh();
        }

        /// <summary>
        ///     History initialization
        /// </summary>
        private void InitHistory()
        {
            _toHistory.Set(HoI2EditorController.Settings.CorpsNameEditor.ToHistory.ToArray());
            _withHistory.Set(HoI2EditorController.Settings.CorpsNameEditor.WithHistory.ToArray());
            _prefixHistory.Set(HoI2EditorController.Settings.CorpsNameEditor.PrefixHistory.ToArray());
            _suffixHistory.Set(HoI2EditorController.Settings.CorpsNameEditor.SuffixHistory.ToArray());

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
            allBranchCheckBox.Checked = HoI2EditorController.Settings.CorpsNameEditor.ApplyAllBranches;
            allCountryCheckBox.Checked = HoI2EditorController.Settings.CorpsNameEditor.ApplyAllCountires;
            regexCheckBox.Checked = HoI2EditorController.Settings.CorpsNameEditor.RegularExpression;
        }

        /// <summary>
        ///     Applies to all military departments Processing when the check status of the check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllBranchCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            HoI2EditorController.Settings.CorpsNameEditor.ApplyAllBranches = allBranchCheckBox.Checked;
        }

        /// <summary>
        ///     Applies to all nations Processing when the check status of the check box changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllCountryCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            HoI2EditorController.Settings.CorpsNameEditor.ApplyAllCountires = allCountryCheckBox.Checked;
        }

        /// <summary>
        ///     Processing when the check status of the regular expression check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRegexCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            HoI2EditorController.Settings.CorpsNameEditor.RegularExpression = regexCheckBox.Checked;
        }

        #endregion
    }
}
