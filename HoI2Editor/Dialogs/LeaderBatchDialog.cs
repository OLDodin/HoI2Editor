using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Dialogs
{
    /// <summary>
    ///     Commander batch edit dialog
    /// </summary>
    public partial class LeaderBatchDialog : Form
    {
        #region Internal field

        /// <summary>
        ///     Batch editing parameters
        /// </summary>
        private readonly LeaderBatchEditArgs _args;

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
        public LeaderBatchDialog(LeaderBatchEditArgs args)
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
                    (int) g.MeasureString(s, srcComboBox.Font).Width +
                    SystemInformation.VerticalScrollBarWidth +
                    margin);
            }
            srcComboBox.DropDownWidth = width;
            srcComboBox.EndUpdate();
            if (_args.TargetCountries.Count > 0)
            {
                srcComboBox.SelectedIndex = Countries.Tags.ToList().IndexOf(_args.TargetCountries[0]);
            }
            srcComboBox.SelectedIndexChanged += OnSrcComboBoxSelectedIndexChanged;

            // Army
            armyCheckBox.Text = Config.GetText(TextId.BranchArmy);
            navyCheckBox.Text = Config.GetText(TextId.BranchNavy);
            airforceCheckBox.Text = Config.GetText(TextId.BranchAirForce);

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
                idNumericUpDown.Value = Leaders.GetNewId(_args.TargetCountries[0]);
            }
            idNumericUpDown.ValueChanged += OnIdNumericUpDownValueChanged;

            // Ideal class combo box
            idealRankComboBox.BeginUpdate();
            idealRankComboBox.Items.Clear();
            width = idealRankComboBox.Width;
            foreach (string s in Leaders.RankNames.Where(name => !string.IsNullOrEmpty(name)))
            {
                idealRankComboBox.Items.Add(s);
                width = Math.Max(width, (int) g.MeasureString(s, idealRankComboBox.Font).Width + margin);
            }
            idealRankComboBox.DropDownWidth = width;
            idealRankComboBox.EndUpdate();
            if (idealRankComboBox.Items.Count > 0)
            {
                idealRankComboBox.SelectedIndex = 0;
            }
            idealRankComboBox.SelectedIndexChanged += OnIdealRankComboBoxSelectedIndexChanged;

            // Retirement year
            if ((Game.Type != GameType.DarkestHour) || (Game.Version < 103))
            {
                retirementYearCheckBox.Enabled = false;
                retirementYearNumericUpDown.Enabled = false;
                retirementYearNumericUpDown.ResetText();
            }
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

            // Army
            _args.Army = armyCheckBox.Checked;
            _args.Navy = navyCheckBox.Checked;
            _args.Airforce = airforceCheckBox.Checked;

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
            _args.Items[(int) LeaderBatchItemId.IdealRank] = idealRankCheckBox.Checked;
            _args.Items[(int) LeaderBatchItemId.Skill] = skillCheckBox.Checked;
            _args.Items[(int) LeaderBatchItemId.MaxSkill] = maxSkillCheckBox.Checked;
            _args.Items[(int) LeaderBatchItemId.Experience] = experienceCheckBox.Checked;
            _args.Items[(int) LeaderBatchItemId.Loyalty] = loyaltyCheckBox.Checked;
            _args.Items[(int) LeaderBatchItemId.StartYear] = startYearCheckBox.Checked;
            _args.Items[(int) LeaderBatchItemId.EndYear] = endYearCheckBox.Checked;
            _args.Items[(int) LeaderBatchItemId.RetirementYear] = retirementYearCheckBox.Checked;
            _args.Items[(int) LeaderBatchItemId.Rank3Year] = rankYearCheckBox1.Checked;
            _args.Items[(int) LeaderBatchItemId.Rank2Year] = rankYearCheckBox2.Checked;
            _args.Items[(int) LeaderBatchItemId.Rank1Year] = rankYearCheckBox3.Checked;
            _args.Items[(int) LeaderBatchItemId.Rank0Year] = rankYearCheckBox4.Checked;

            _args.IdealRank = (LeaderRank) (idealRankComboBox.SelectedIndex + 1);
            _args.Skill = (int) skillNumericUpDown.Value;
            _args.MaxSkill = (int) maxSkillNumericUpDown.Value;
            _args.Experience = (int) experienceNumericUpDown.Value;
            _args.Loyalty = (int) loyaltyNumericUpDown.Value;
            _args.StartYear = (int) startYearNumericUpDown.Value;
            _args.EndYear = (int) endYearNumericUpDown.Value;
            _args.RetirementYear = (int) retirementYearNumericUpDown.Value;
            _args.RankYear[0] = (int) rankYearNumericUpDown1.Value;
            _args.RankYear[1] = (int) rankYearNumericUpDown2.Value;
            _args.RankYear[2] = (int) rankYearNumericUpDown3.Value;
            _args.RankYear[3] = (int) rankYearNumericUpDown4.Value;
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

            idealRankCheckBox.Checked = false;
            skillCheckBox.Checked = false;
            maxSkillCheckBox.Checked = false;
            experienceCheckBox.Checked = false;
            loyaltyCheckBox.Checked = false;
            startYearCheckBox.Checked = false;
            endYearCheckBox.Checked = false;
            retirementYearCheckBox.Checked = false;
            rankYearCheckBox1.Checked = false;
            rankYearCheckBox2.Checked = false;
            rankYearCheckBox3.Checked = false;
            rankYearCheckBox4.Checked = false;
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

            idealRankCheckBox.Checked = false;
            skillCheckBox.Checked = false;
            maxSkillCheckBox.Checked = false;
            experienceCheckBox.Checked = false;
            loyaltyCheckBox.Checked = false;
            startYearCheckBox.Checked = false;
            endYearCheckBox.Checked = false;
            retirementYearCheckBox.Checked = false;
            rankYearCheckBox1.Checked = false;
            rankYearCheckBox2.Checked = false;
            rankYearCheckBox3.Checked = false;
            rankYearCheckBox4.Checked = false;
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
                idNumericUpDown.Value = Leaders.GetNewId(Countries.Tags[destComboBox.SelectedIndex]);
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
        ///     Processing when the check status of the ideal class check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIdealRankCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (idealRankCheckBox.Checked)
            {
                modifyRadioButton.Checked = true;
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
        ///     Processing when the check status of the maximum skill check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxSkillCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (maxSkillCheckBox.Checked)
            {
                modifyRadioButton.Checked = true;
            }
        }

        /// <summary>
        ///     Processing when the check status of the experience value check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExperienceCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (experienceCheckBox.Checked)
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
        ///     Processing when the check status of the Major General Year check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearCheckBox1CheckedChanged(object sender, EventArgs e)
        {
            if (rankYearCheckBox1.Checked)
            {
                modifyRadioButton.Checked = true;
            }
        }

        /// <summary>
        ///     Processing when the check status of the middle general officer year check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearCheckBox2CheckedChanged(object sender, EventArgs e)
        {
            if (rankYearCheckBox2.Checked)
            {
                modifyRadioButton.Checked = true;
            }
        }

        /// <summary>
        ///     Processing when the check status of the general officer year check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearCheckBox3CheckedChanged(object sender, EventArgs e)
        {
            if (rankYearCheckBox3.Checked)
            {
                modifyRadioButton.Checked = true;
            }
        }

        /// <summary>
        ///     Processing when the check status of the Marshal Officer Year check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearCheckBox4CheckedChanged(object sender, EventArgs e)
        {
            if (rankYearCheckBox4.Checked)
            {
                modifyRadioButton.Checked = true;
            }
        }

        /// <summary>
        ///     Processing when changing the selection item of the ideal class combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIdealRankComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (idealRankComboBox.SelectedIndex < 0)
            {
                return;
            }

            idealRankCheckBox.Checked = true;
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
        ///     Processing when changing the value of maximum skill value up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxSkillNumericUpDownValueChanged(object sender, EventArgs e)
        {
            maxSkillCheckBox.Checked = true;
        }

        /// <summary>
        ///     Processing when changing the value of the experience value numerical value up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExperienceNumericUpDownValueChanged(object sender, EventArgs e)
        {
            experienceCheckBox.Checked = true;
        }

        /// <summary>
        ///     Processing when changing the value of loyalty value up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoyaltyNumericUpDownValueChanged(object sender, EventArgs e)
        {
            loyaltyCheckBox.Checked = true;
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
        ///     Maj. Gen. Year Processing when changing the value up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearNumericUpDown1ValueChanged(object sender, EventArgs e)
        {
            rankYearCheckBox1.Checked = true;
        }

        /// <summary>
        ///     Processing when changing the value of the year value up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearNumericUpDown2ValueChanged(object sender, EventArgs e)
        {
            rankYearCheckBox2.Checked = true;
        }

        /// <summary>
        ///     Processing when changing the value of the general officer year value up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearNumericUpDown3ValueChanged(object sender, EventArgs e)
        {
            rankYearCheckBox3.Checked = true;
        }

        /// <summary>
        ///     Marshal Officer Year processing when changing the value up / down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearNumericUpDown4ValueChanged(object sender, EventArgs e)
        {
            rankYearCheckBox4.Checked = true;
        }

        #endregion
    }
}
