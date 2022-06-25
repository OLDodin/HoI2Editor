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
    ///     Random Commander Name Editor Form
    /// </summary>
    public partial class RandomLeaderEditorForm : Form
    {
        #region Internal field

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
        public RandomLeaderEditorForm()
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
            // Update the display of the random commander name list
            UpdateNameList();

            // Update the display as the edited flag is cleared
            countryListBox.Refresh();
        }

        /// <summary>
        ///     Processing after data storage
        /// </summary>
        public void OnFileSaved()
        {
            // Update the display as the edited flag is cleared
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
            // National list box
            countryListBox.ItemHeight = DeviceCaps.GetScaledHeight(countryListBox.ItemHeight);

            // Window position
            Location = HoI2EditorController.Settings.RandomLeaderEditor.Location;
            Size = HoI2EditorController.Settings.RandomLeaderEditor.Size;
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

            // Initialize the national list box
            InitCountryListBox();

            // Initialize history
            InitHistory();

            // Initialize option settings
            InitOption();

            // Read the random commander name definition file
            RandomLeaders.Load();

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
            HoI2EditorController.OnRandomLeaderEditorFormClosed();
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
                HoI2EditorController.Settings.RandomLeaderEditor.Location = Location;
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
                HoI2EditorController.Settings.RandomLeaderEditor.Size = Size;
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
            int index = HoI2EditorController.Settings.RandomLeaderEditor.Country;
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
                brush = RandomLeaders.IsDirty(country)
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
            // Update the display of the random commander name list
            UpdateNameList();

            // Save the selected nation
            HoI2EditorController.Settings.RandomLeaderEditor.Country = countryListBox.SelectedIndex;
        }

        #endregion

        #region Random commander name list

        /// <summary>
        ///     Update random commander name list
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

            // Add random commander names in order
            StringBuilder sb = new StringBuilder();
            foreach (string name in RandomLeaders.GetNames(country))
            {
                sb.AppendLine(name);
            }

            nameTextBox.Text = sb.ToString();
        }

        /// <summary>
        ///     Processing when changing the random commander name list
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

            // Update Random Commander Name List
            RandomLeaders.SetNames(nameTextBox.Lines.Where(line => !string.IsNullOrEmpty(line)).ToList(), country);

            // Update the display as the edited flag is updated
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

            Log.Info("[RandomLeader] Replace: {0} -> {1}", to, with);

            if (allCountryCheckBox.Checked)
            {
                // Replace random commander names in all countries
                RandomLeaders.ReplaceAll(to, with, regexCheckBox.Checked);
            }
            else
            {
                // Return if there is no selection in the national list box
                if (countryListBox.SelectedIndex < 0)
                {
                    return;
                }
                Country country = Countries.Tags[countryListBox.SelectedIndex];

                // Replace random commander name
                RandomLeaders.Replace(to, with, country, regexCheckBox.Checked);
            }

            // Update the display of the random commander name list
            UpdateNameList();

            // Update the display as the edited flag is updated
            countryListBox.Refresh();

            // Update history
            _toHistory.Add(to);
            _withHistory.Add(with);

            HoI2EditorController.Settings.RandomLeaderEditor.ToHistory = _toHistory.Get().ToList();
            HoI2EditorController.Settings.RandomLeaderEditor.WithHistory = _withHistory.Get().ToList();

            // Update history combo box
            UpdateHistory();
        }

        /// <summary>
        ///     History initialization
        /// </summary>
        private void InitHistory()
        {
            _toHistory.Set(HoI2EditorController.Settings.RandomLeaderEditor.ToHistory.ToArray());
            _withHistory.Set(HoI2EditorController.Settings.RandomLeaderEditor.WithHistory.ToArray());

            UpdateHistory();

            if (toComboBox.Items.Count > 0)
            {
                toComboBox.SelectedIndex = 0;
            }

            if (withComboBox.Items.Count > 0)
            {
                withComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        ///     Update history combo box
        /// </summary>
        private void UpdateHistory()
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
        ///     Initialize option settings
        /// </summary>
        private void InitOption()
        {
            allCountryCheckBox.Checked = HoI2EditorController.Settings.RandomLeaderEditor.ApplyAllCountires;
            regexCheckBox.Checked = HoI2EditorController.Settings.RandomLeaderEditor.RegularExpression;
        }

        /// <summary>
        ///     Applies to all nations Processing when the check status of the check box changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllCountryCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            HoI2EditorController.Settings.RandomLeaderEditor.ApplyAllCountires = allCountryCheckBox.Checked;
        }

        /// <summary>
        ///     Processing when the check status of the regular expression check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRegexCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            HoI2EditorController.Settings.RandomLeaderEditor.RegularExpression = regexCheckBox.Checked;
        }

        #endregion
    }
}
