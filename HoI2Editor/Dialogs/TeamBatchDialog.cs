using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Dialogs
{
    /// <summary>
    ///     Research institution batch edit dialog
    /// </summary>
    public partial class TeamBatchDialog : Form
    {
        #region Internal field

        /// <summary>
        ///     Batch editing parameters
        /// </summary>
        private readonly TeamBatchEditArgs _args;

        /// <summary>
        ///     start ID Has changed
        /// </summary>
        private bool _idChanged;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="args">Batch editing parameters</param>
        public TeamBatchDialog(TeamBatchEditArgs args)
        {
            InitializeComponent();

            _args = args;
        }

        #endregion

        #region Form

        /// <summary>
        ///     Processing when loading a form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormLoad(object sender, EventArgs e)
        {
            Graphics g = Graphics.FromHwnd(Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;

            // Selected country combo box
            srcComboBox.BeginUpdate();
            srcComboBox.Items.Clear();
            int width = srcComboBox.Width;
            foreach (string s in Countries.Tags
                .Select(country => Countries.Strings[(int) country])
                .Select(name => Config.ExistsKey(name)
                    ? $"{name} {Config.GetText(name)}"
                    : name))
            {
                srcComboBox.Items.Add(s);
                width = Math.Max(width,
                    (int) g.MeasureString(s, srcComboBox.Font).Width + SystemInformation.VerticalScrollBarWidth +
                    margin);
            }
            srcComboBox.DropDownWidth = width;
            srcComboBox.EndUpdate();
            if (srcComboBox.Items.Count > 0)
            {
                srcComboBox.SelectedIndex = Countries.Tags.ToList().IndexOf(_args.TargetCountries[0]);
            }
            srcComboBox.SelectedIndexChanged += OnSrcComboBoxSelectedIndexChanged;

            // copy / /Destination combo box
            destComboBox.BeginUpdate();
            destComboBox.Items.Clear();
            width = destComboBox.Width;
            foreach (string s in Countries.Tags
                .Select(country => Countries.Strings[(int) country])
                .Select(name => Config.ExistsKey(name)
                    ? $"{name} {Config.GetText(name)}"
                    : name))
            {
                destComboBox.Items.Add(s);
                width = Math.Max(width,
                    (int) g.MeasureString(s, srcComboBox.Font).Width +
                    SystemInformation.VerticalScrollBarWidth +
                    margin);
            }
            destComboBox.DropDownWidth = width;
            destComboBox.EndUpdate();
            if (_args.TargetCountries.Count > 0)
            {
                destComboBox.SelectedIndex = Countries.Tags.ToList().IndexOf(_args.TargetCountries[0]);
            }
            destComboBox.SelectedIndexChanged += OnDestComboBoxSelectedIndexChanged;

            // start ID
            if (_args.TargetCountries.Count > 0)
            {
                idNumericUpDown.Value = Teams.GetNewId(_args.TargetCountries[0]);
            }
            idNumericUpDown.ValueChanged += OnIdNumericUpDownValueChanged;
        }

        /// <summary>
        ///     OK Processing when a key is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOkButtonClick(object sender, EventArgs e)
        {
            // Target country mode
            if (allRadioButton.Checked)
            {
                _args.CountryMode = BatchCountryMode.All;
            }
            else if (selectedRadioButton.Checked)
            {
                _args.CountryMode = BatchCountryMode.Selected;
            }
            else
            {
                _args.CountryMode = BatchCountryMode.Specified;
                _args.TargetCountries.Clear();
                _args.TargetCountries.Add(Countries.Tags[srcComboBox.SelectedIndex]);
            }

            // action mode
            if (copyRadioButton.Checked)
            {
                _args.ActionMode = BatchActionMode.Copy;
            }
            else if (moveRadioButton.Checked)
            {
                _args.ActionMode = BatchActionMode.Move;
            }
            else
            {
                _args.ActionMode = BatchActionMode.Modify;
            }

            _args.Destination = Countries.Tags[destComboBox.SelectedIndex];
            _args.Id = (int) idNumericUpDown.Value;

            // Edit items
            _args.Items[(int) TeamBatchItemId.Skill] = skillCheckBox.Checked;
            _args.Items[(int) TeamBatchItemId.StartYear] = startYearCheckBox.Checked;
            _args.Items[(int) TeamBatchItemId.EndYear] = endYearCheckBox.Checked;

            _args.Skill = (int) skillNumericUpDown.Value;
            _args.StartYear = (int) startYearNumericUpDown.Value;
            _args.EndYear = (int) endYearNumericUpDown.Value;
        }

        #endregion

        #region Edit items

        /// <summary>
        ///     Processing when changing the selection item of the target country combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSrcComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (srcComboBox.SelectedIndex < 0)
            {
                return;
            }

            specifiedRadioButton.Checked = true;
        }

        /// <summary>
        ///     Process when changing the check status of the copy radio button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCopyRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (!copyRadioButton.Checked)
            {
                return;
            }

            skillCheckBox.Checked = false;
            startYearCheckBox.Checked = false;
            endYearCheckBox.Checked = false;
        }

        /// <summary>
        ///     Processing when changing the check status of the move radio button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMoveRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (!moveRadioButton.Checked)
            {
                return;
            }

            skillCheckBox.Checked = false;
            startYearCheckBox.Checked = false;
            endYearCheckBox.Checked = false;
        }

        /// <summary>
        ///     copy / / Processing when changing the selection item of the destination combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDestComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (destComboBox.SelectedIndex < 0)
            {
                return;
            }

            if (!copyRadioButton.Checked && !moveRadioButton.Checked)
            {
                copyRadioButton.Checked = true;
            }

            // start ID If the numerical value of numerical up / down has not been changed, change it.
            if (!_idChanged)
            {
                idNumericUpDown.ValueChanged -= OnIdNumericUpDownValueChanged;
                idNumericUpDown.Value = Teams.GetNewId(Countries.Tags[destComboBox.SelectedIndex]);
                idNumericUpDown.ValueChanged += OnIdNumericUpDownValueChanged;
            }
        }

        /// <summary>
        ///     start ID Processing when changing the value of numerical up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIdNumericUpDownValueChanged(object sender, EventArgs e)
        {
            _idChanged = true;

            if (!copyRadioButton.Checked && !moveRadioButton.Checked)
            {
                copyRadioButton.Checked = true;
            }
        }

        /// <summary>
        ///     Processing when changing the check status of the skill check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSkillCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (skillCheckBox.Checked)
            {
                modifyRadioButton.Checked = true;
            }
        }

        /// <summary>
        ///     Processing when the check status of the start year check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartYearCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (startYearCheckBox.Checked)
            {
                modifyRadioButton.Checked = true;
            }
        }

        /// <summary>
        ///     Processing when the check status of the end year check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEndYearCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (endYearCheckBox.Checked)
            {
                modifyRadioButton.Checked = true;
            }
        }

        /// <summary>
        ///     Processing when changing the value of skill value up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSkillNumericUpDownValueChanged(object sender, EventArgs e)
        {
            skillCheckBox.Checked = true;
        }

        /// <summary>
        ///     Processing when changing the value of the start year value up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartYearNumericUpDownValueChanged(object sender, EventArgs e)
        {
            startYearCheckBox.Checked = true;
        }

        /// <summary>
        ///     Processing when changing the value of the end year numerical value up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEndYearNumericUpDownValueChanged(object sender, EventArgs e)
        {
            endYearCheckBox.Checked = true;
        }

        #endregion
    }
}
