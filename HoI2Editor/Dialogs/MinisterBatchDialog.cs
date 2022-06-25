using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Dialogs
{
    /// <summary>
    ///     Cabinet batch edit dialog
    /// </summary>
    public partial class MinisterBatchDialog : Form
    {
        #region Internal field

        /// <summary>
        ///     Batch editing parameters
        /// </summary>
        private readonly MinisterBatchEditArgs _args;

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
        public MinisterBatchDialog(MinisterBatchEditArgs args)
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

            // Target country combo box
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

            // Ministerial status
            hosCheckBox.Text = Config.GetText(TextId.MinisterHeadOfState);
            hogCheckBox.Text = Config.GetText(TextId.MinisterHeadOfGovernment);
            mofCheckBox.Text = Config.GetText(TextId.MinisterForeignMinister);
            moaCheckBox.Text = Config.GetText(TextId.MinisterArmamentMinister);
            mosCheckBox.Text = Config.GetText(TextId.MinisterMinisterOfSecurity);
            moiCheckBox.Text = Config.GetText(TextId.MinisterMinisterOfIntelligence);
            cosCheckBox.Text = Config.GetText(TextId.MinisterChiefOfStaff);
            coaCheckBox.Text = Config.GetText(TextId.MinisterChiefOfArmy);
            conCheckBox.Text = Config.GetText(TextId.MinisterChiefOfNavy);
            coafCheckBox.Text = Config.GetText(TextId.MinisterChiefOfAir);

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
                    (int) g.MeasureString(s, destComboBox.Font).Width + SystemInformation.VerticalScrollBarWidth +
                    margin);
            }
            destComboBox.DropDownWidth = width;
            destComboBox.EndUpdate();
            if (destComboBox.Items.Count > 0)
            {
                destComboBox.SelectedIndex = Countries.Tags.ToList().IndexOf(_args.TargetCountries[0]);
            }
            destComboBox.SelectedIndexChanged += OnDestComboBoxSelectedIndexChanged;

            // start ID
            if (_args.TargetCountries.Count > 0)
            {
                idNumericUpDown.Value = Ministers.GetNewId(_args.TargetCountries[0]);
            }
            idNumericUpDown.ValueChanged += OnIdNumericUpDownValueChanged;

            // End year
            if (Game.Type != GameType.DarkestHour)
            {
                endYearCheckBox.Enabled = false;
                endYearNumericUpDown.Enabled = false;
                endYearNumericUpDown.ResetText();
            }

            // Retirement year
            if ((Game.Type != GameType.DarkestHour) || (Game.Version < 103))
            {
                retirementYearCheckBox.Enabled = false;
                retirementYearNumericUpDown.Enabled = false;
                retirementYearNumericUpDown.ResetText();
            }

            // Ideology combo box
            ideologyComboBox.BeginUpdate();
            ideologyComboBox.Items.Clear();
            width = ideologyComboBox.Width;
            foreach (string s in Ministers.IdeologyNames.Where(id => id != TextId.Empty).Select(Config.GetText))
            {
                ideologyComboBox.Items.Add(s);
                width = Math.Max(width, (int) g.MeasureString(s, ideologyComboBox.Font).Width + margin);
            }
            ideologyComboBox.DropDownWidth = width;
            ideologyComboBox.EndUpdate();
            if (ideologyComboBox.Items.Count > 0)
            {
                ideologyComboBox.SelectedIndex = 0;
            }
            ideologyComboBox.SelectedIndexChanged += OnIdeologyComboBoxSelectedIndexChanged;

            // Loyalty combo box
            loyaltyComboBox.BeginUpdate();
            loyaltyComboBox.Items.Clear();
            width = loyaltyComboBox.Width;
            foreach (string s in Ministers.LoyaltyNames.Where(name => !string.IsNullOrEmpty(name)))
            {
                loyaltyComboBox.Items.Add(s);
                width = Math.Max(width, (int) g.MeasureString(s, loyaltyComboBox.Font).Width + margin);
            }
            loyaltyComboBox.DropDownWidth = width;
            loyaltyComboBox.EndUpdate();
            if (loyaltyComboBox.Items.Count > 0)
            {
                loyaltyComboBox.SelectedIndex = 0;
            }
            loyaltyComboBox.SelectedIndexChanged += OnLoyaltyComboBoxSelectedIndexChanged;
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

            // Target position mode
            _args.PositionMode[(int) MinisterPosition.HeadOfState] = hosCheckBox.Checked;
            _args.PositionMode[(int) MinisterPosition.HeadOfGovernment] = hogCheckBox.Checked;
            _args.PositionMode[(int) MinisterPosition.ForeignMinister] = mofCheckBox.Checked;
            _args.PositionMode[(int) MinisterPosition.MinisterOfArmament] = moaCheckBox.Checked;
            _args.PositionMode[(int) MinisterPosition.MinisterOfSecurity] = mosCheckBox.Checked;
            _args.PositionMode[(int) MinisterPosition.HeadOfMilitaryIntelligence] = moiCheckBox.Checked;
            _args.PositionMode[(int) MinisterPosition.ChiefOfStaff] = cosCheckBox.Checked;
            _args.PositionMode[(int) MinisterPosition.ChiefOfArmy] = coaCheckBox.Checked;
            _args.PositionMode[(int) MinisterPosition.ChiefOfNavy] = conCheckBox.Checked;
            _args.PositionMode[(int) MinisterPosition.ChiefOfAirForce] = coafCheckBox.Checked;

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
            _args.Items[(int) MinisterBatchItemId.StartYear] = startYearCheckBox.Checked;
            _args.Items[(int) MinisterBatchItemId.EndYear] = endYearCheckBox.Checked;
            _args.Items[(int) MinisterBatchItemId.RetirementYear] = retirementYearCheckBox.Checked;
            _args.Items[(int) MinisterBatchItemId.Ideology] = ideologyCheckBox.Checked;
            _args.Items[(int) MinisterBatchItemId.Loyalty] = loyaltyCheckBox.Checked;

            _args.StartYear = (int) startYearNumericUpDown.Value;
            _args.EndYear = (int) endYearNumericUpDown.Value;
            _args.RetirementYear = (int) retirementYearNumericUpDown.Value;
            _args.Ideology = (MinisterIdeology) (ideologyComboBox.SelectedIndex + 1);
            _args.Loyalty = (MinisterLoyalty) (loyaltyComboBox.SelectedIndex + 1);
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

            startYearCheckBox.Checked = false;
            endYearCheckBox.Checked = false;
            retirementYearCheckBox.Checked = false;
            ideologyCheckBox.Checked = false;
            loyaltyCheckBox.Checked = false;
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

            startYearCheckBox.Checked = false;
            endYearCheckBox.Checked = false;
            retirementYearCheckBox.Checked = false;
            ideologyCheckBox.Checked = false;
            loyaltyCheckBox.Checked = false;
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
                idNumericUpDown.Value = Ministers.GetNewId(Countries.Tags[destComboBox.SelectedIndex]);
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
        ///     Processing when the check status of the retirement year check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRetirementYearCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (retirementYearCheckBox.Checked)
            {
                modifyRadioButton.Checked = true;
            }
        }

        /// <summary>
        ///     Processing when changing the check status of the ideology check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIdeologyCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (ideologyCheckBox.Checked)
            {
                modifyRadioButton.Checked = true;
            }
        }

        /// <summary>
        ///     Processing when the check status of the loyalty check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoyaltyCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (loyaltyCheckBox.Checked)
            {
                modifyRadioButton.Checked = true;
            }
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

        /// <summary>
        ///     Processing when changing the value of retirement year numerical value up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRetirementYearNumericUpDownValueChanged(object sender, EventArgs e)
        {
            retirementYearCheckBox.Checked = true;
        }

        /// <summary>
        ///     Processing when changing the selection item of the ideology combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIdeologyComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (ideologyComboBox.SelectedIndex < 0)
            {
                return;
            }

            ideologyCheckBox.Checked = true;
        }

        /// <summary>
        ///     Processing when changing the selection item of the loyalty combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoyaltyComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (loyaltyComboBox.SelectedIndex < 0)
            {
                return;
            }

            loyaltyCheckBox.Checked = true;
        }

        #endregion
    }
}
