using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Controls;
using HoI2Editor.Dialogs;
using HoI2Editor.Models;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Commander Editor Form
    /// </summary>
    public partial class LeaderEditorForm : Form
    {
        #region Internal field

        /// <summary>
        ///     List of commanders after narrowing down
        /// </summary>
        private readonly List<Leader> _list = new List<Leader>();

        /// <summary>
        ///     Sort target
        /// </summary>
        private SortKey _key = SortKey.None;

        /// <summary>
        ///     Sort order
        /// </summary>
        private SortOrder _order = SortOrder.Ascendant;

        /// <summary>
        ///     Sort target
        /// </summary>
        private enum SortKey
        {
            None,
            Tag,
            Id,
            Name,
            Branch,
            Skill,
            MaxSkill,
            StartYear,
            EndYear,
            Traits
        }

        /// <summary>
        ///     Sort order
        /// </summary>
        private enum SortOrder
        {
            Ascendant,
            Decendant
        }

        #endregion

        #region Public constant

        /// <summary>
        ///     Number of columns in the commander list view
        /// </summary>
        public const int LeaderListColumnCount = 9;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public LeaderEditorForm()
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
            // Narrow down the list of commanders
            NarrowLeaderList();

            // Sort the commander list
            SortLeaderList();

            // Update the display of the commander list
            UpdateLeaderList();

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
            UpdateEditableItems();
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
            // Commander list view
            countryColumnHeader.Width = HoI2EditorController.Settings.LeaderEditor.ListColumnWidth[0];
            idColumnHeader.Width = HoI2EditorController.Settings.LeaderEditor.ListColumnWidth[1];
            nameColumnHeader.Width = HoI2EditorController.Settings.LeaderEditor.ListColumnWidth[2];
            branchColumnHeader.Width = HoI2EditorController.Settings.LeaderEditor.ListColumnWidth[3];
            skillColumnHeader.Width = HoI2EditorController.Settings.LeaderEditor.ListColumnWidth[4];
            maxSkillColumnHeader.Width = HoI2EditorController.Settings.LeaderEditor.ListColumnWidth[5];
            startYearColumnHeader.Width = HoI2EditorController.Settings.LeaderEditor.ListColumnWidth[6];
            endYearColumnHeader.Width = HoI2EditorController.Settings.LeaderEditor.ListColumnWidth[7];
            traitsColumnHeader.Width = HoI2EditorController.Settings.LeaderEditor.ListColumnWidth[8];

            // National list box
            countryListBox.ColumnWidth = DeviceCaps.GetScaledWidth(countryListBox.ColumnWidth);
            countryListBox.ItemHeight = DeviceCaps.GetScaledHeight(countryListBox.ItemHeight);

            // Window position
            Location = HoI2EditorController.Settings.LeaderEditor.Location;
            Size = HoI2EditorController.Settings.LeaderEditor.Size;
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

            // Load the game settings file
            Misc.Load();

            // Read the character string definition file
            Config.Load();

            // Initialize the characteristic string
            InitTraitsText();

            // Initialize edit items
            InitEditableItems();

            // Initialize the national list box
            InitCountryListBox();

            // Read commander file
            Leaders.Load();

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
            HoI2EditorController.OnLeaderEditorFormClosed();
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
                HoI2EditorController.Settings.LeaderEditor.Location = Location;
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
                HoI2EditorController.Settings.LeaderEditor.Size = Size;
            }
        }

        /// <summary>
        ///     Processing when the batch edit button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBatchButtonClick(object sender, EventArgs e)
        {
            LeaderBatchEditArgs args = new LeaderBatchEditArgs();
            args.TargetCountries.AddRange(from string name in countryListBox.SelectedItems
                select Countries.StringMap[name]);

            // Display the batch edit dialog
            LeaderBatchDialog dialog = new LeaderBatchDialog(args);
            if (dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            // If the retirement year is not set Misc Change the value of
            if (args.Items[(int) LeaderBatchItemId.RetirementYear] && !Misc.EnableRetirementYearLeaders)
            {
                Misc.EnableRetirementYearLeaders = true;
                HoI2EditorController.OnItemChanged(EditorItemId.LeaderRetirementYear, this);
            }

            // Bulk editing process
            Leaders.BatchEdit(args);

            // Update the commander list
            NarrowLeaderList();
            UpdateLeaderList();

            // Update drawing to change the item color of the national list box
            countryListBox.Refresh();
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

        #region Commander list view

        /// <summary>
        ///     Update the display of the commander list
        /// </summary>
        private void UpdateLeaderList()
        {
            leaderListView.BeginUpdate();
            leaderListView.Items.Clear();

            // Register items in order
            foreach (Leader leader in _list)
            {
                leaderListView.Items.Add(CreateLeaderListViewItem(leader));
            }

            if (leaderListView.Items.Count > 0)
            {
                // Select the first item
                leaderListView.Items[0].Focused = true;
                leaderListView.Items[0].Selected = true;

                // Enable edit items
                EnableEditableItems();
            }
            else
            {
                // Disable edit items
                DisableEditableItems();
            }

            leaderListView.EndUpdate();
        }

        /// <summary>
        ///     Narrow down the commander list
        /// </summary>
        private void NarrowLeaderList()
        {
            _list.Clear();

            // Return if there is no selected nation
            if (countryListBox.SelectedIndices.Count == 0)
            {
                return;
            }

            // Get the characteristic narrowing mask
            uint traitsMask = GetNarrowedTraits();

            // Create a list of selected nations
            List<Country> tags = (from string s in countryListBox.SelectedItems select Countries.StringMap[s]).ToList();

            // Narrow down the commanders belonging to the selected nation in order
            foreach (Leader leader in Leaders.Items.Where(leader => tags.Contains(leader.Country)))
            {
                // Narrowing down by military department
                switch (leader.Branch)
                {
                    case Branch.Army:
                        if (!armyNarrowCheckBox.Checked)
                        {
                            continue;
                        }
                        break;

                    case Branch.Navy:
                        if (!navyNarrowCheckBox.Checked)
                        {
                            continue;
                        }
                        break;

                    case Branch.Airforce:
                        if (!airforceNarrowCheckBox.Checked)
                        {
                            continue;
                        }
                        break;

                    default:
                        if (!armyNarrowCheckBox.Checked ||
                            !navyNarrowCheckBox.Checked ||
                            !airforceNarrowCheckBox.Checked)
                        {
                            continue;
                        }
                        break;
                }

                // Narrowing down by commander characteristics
                if (traitsOrNarrowRadioButton.Checked)
                {
                    // OR conditions
                    if ((leader.Traits & traitsMask) == 0)
                    {
                        continue;
                    }
                }
                else if (traitsAndNarrowRadioButton.Checked)
                {
                    // AND AND conditions
                    if ((leader.Traits & traitsMask) != traitsMask)
                    {
                        continue;
                    }
                }

                _list.Add(leader);
            }
        }

        /// <summary>
        ///     Sort the commander list
        /// </summary>
        private void SortLeaderList()
        {
            switch (_key)
            {
                case SortKey.None: // No sort
                    break;

                case SortKey.Tag: // Country tag
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((leader1, leader2) => leader1.Country - leader2.Country);
                    }
                    else
                    {
                        _list.Sort((leader1, leader2) => leader2.Country - leader1.Country);
                    }
                    break;

                case SortKey.Id: // ID
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((leader1, leader2) => leader1.Id - leader2.Id);
                    }
                    else
                    {
                        _list.Sort((leader1, leader2) => leader2.Id - leader1.Id);
                    }
                    break;

                case SortKey.Name: // name
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((leader1, leader2) => string.CompareOrdinal(leader1.Name, leader2.Name));
                    }
                    else
                    {
                        _list.Sort((leader1, leader2) => string.CompareOrdinal(leader2.Name, leader1.Name));
                    }
                    break;

                case SortKey.Branch: // Army
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((leader1, leader2) => leader1.Branch - leader2.Branch);
                    }
                    else
                    {
                        _list.Sort((leader1, leader2) => leader2.Branch - leader1.Branch);
                    }
                    break;

                case SortKey.Skill: // skill
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((leader1, leader2) => leader1.Skill - leader2.Skill);
                    }
                    else
                    {
                        _list.Sort((leader1, leader2) => leader2.Skill - leader1.Skill);
                    }
                    break;

                case SortKey.MaxSkill: // Maximum skill
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((leader1, leader2) => leader1.MaxSkill - leader2.MaxSkill);
                    }
                    else
                    {
                        _list.Sort((leader1, leader2) => leader2.MaxSkill - leader1.MaxSkill);
                    }
                    break;

                case SortKey.StartYear: // Start year
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((leader1, leader2) => leader1.StartYear - leader2.StartYear);
                    }
                    else
                    {
                        _list.Sort((leader1, leader2) => leader2.StartYear - leader1.StartYear);
                    }
                    break;

                case SortKey.EndYear: // End year
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((leader1, leader2) => leader1.EndYear - leader2.EndYear);
                    }
                    else
                    {
                        _list.Sort((leader1, leader2) => leader2.EndYear - leader1.EndYear);
                    }
                    break;

                case SortKey.Traits: // Characteristic
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((leader1, leader2) => (int) (leader1.Traits - (long) leader2.Traits));
                    }
                    else
                    {
                        _list.Sort((leader1, leader2) => (int) (leader2.Traits - (long) leader1.Traits));
                    }
                    break;
            }
        }

        /// <summary>
        ///     Processing when changing the selection item in the commander list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeaderListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update edit items
            UpdateEditableItems();
        }

        /// <summary>
        ///     Processing before editing items in the commander list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeaderListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // Country tag
                    e.Type = ItemEditType.List;
                    e.Items = countryComboBox.Items.Cast<string>();
                    e.Index = countryComboBox.SelectedIndex;
                    e.DropDownWidth = countryComboBox.DropDownWidth;
                    break;

                case 1: // ID
                    e.Type = ItemEditType.Text;
                    e.Text = idNumericUpDown.Text;
                    break;

                case 2: // name
                    e.Type = ItemEditType.Text;
                    e.Text = nameTextBox.Text;
                    break;

                case 3: // Army
                    e.Type = ItemEditType.List;
                    e.Items = branchComboBox.Items.Cast<string>();
                    e.Index = branchComboBox.SelectedIndex;
                    e.DropDownWidth = branchComboBox.DropDownWidth;
                    break;

                case 4: // skill
                    e.Type = ItemEditType.Text;
                    e.Text = skillNumericUpDown.Text;
                    break;

                case 5: // Maximum skill
                    e.Type = ItemEditType.Text;
                    e.Text = maxSkillNumericUpDown.Text;
                    break;

                case 6: // Start year
                    e.Type = ItemEditType.Text;
                    e.Text = startYearNumericUpDown.Text;
                    break;

                case 7: // End year
                    e.Type = ItemEditType.Text;
                    e.Text = endYearNumericUpDown.Text;
                    break;

                case 8: // Characteristic
                    e.Type = ItemEditType.None;
                    break;
            }
        }

        /// <summary>
        ///     Processing after editing items in the commander list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeaderListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // Country tag
                    countryComboBox.SelectedIndex = e.Index;
                    break;

                case 1: // ID
                    idNumericUpDown.Text = e.Text;
                    break;

                case 2: // name
                    nameTextBox.Text = e.Text;
                    break;

                case 3: // Army
                    branchComboBox.SelectedIndex = e.Index;
                    break;

                case 4: // skill
                    skillNumericUpDown.Text = e.Text;
                    break;

                case 5: // Maximum skill
                    maxSkillNumericUpDown.Text = e.Text;
                    break;

                case 6: // Start year
                    startYearNumericUpDown.Text = e.Text;
                    break;

                case 7: // End year
                    endYearNumericUpDown.Text = e.Text;
                    break;
            }

            // Since the items in the list view will be updated by yourself, it will be treated as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Processing when replacing items in the commander list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeaderListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // I will replace the items on my own, so I will treat it as canceled
            e.Cancel = true;

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;
            if (srcIndex < destIndex)
            {
                destIndex--;
            }

            Leader src = leaderListView.Items[srcIndex].Tag as Leader;
            if (src == null)
            {
                return;
            }
            Leader dest = leaderListView.Items[destIndex].Tag as Leader;
            if (dest == null)
            {
                return;
            }

            // Move items in the commander list
            Leaders.MoveItem(src, dest);
            MoveListItem(srcIndex, destIndex);

            // Set the edited flag
            Leaders.SetDirty(src.Country);
        }

        /// <summary>
        ///     Processing when a column is clicked in the commander list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeaderListViewColumnClick(object sender, ColumnClickEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // Country tag
                    if (_key == SortKey.Tag)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Tag;
                    }
                    break;

                case 1: // ID
                    if (_key == SortKey.Id)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Id;
                    }
                    break;

                case 2: // name
                    if (_key == SortKey.Name)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Name;
                    }
                    break;

                case 3: // Army
                    if (_key == SortKey.Branch)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Branch;
                    }
                    break;

                case 4: // skill
                    if (_key == SortKey.Skill)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Skill;
                    }
                    break;

                case 5: // Maximum skill
                    if (_key == SortKey.MaxSkill)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.MaxSkill;
                    }
                    break;

                case 6: // Start year
                    if (_key == SortKey.StartYear)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.StartYear;
                    }
                    break;

                case 7: // End year
                    if (_key == SortKey.EndYear)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.EndYear;
                    }
                    break;

                case 8: // Characteristic
                    if (_key == SortKey.Traits)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Traits;
                    }
                    break;

                default:
                    // Do nothing when clicking on a column with no items
                    return;
            }

            // Sort the commander list
            SortLeaderList();

            // Update the commander list
            UpdateLeaderList();
        }

        /// <summary>
        ///     Processing when changing the width of columns in the commander list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeaderListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < LeaderListColumnCount))
            {
                HoI2EditorController.Settings.LeaderEditor.ListColumnWidth[e.ColumnIndex] =
                    leaderListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Processing when a new button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewButtonClick(object sender, EventArgs e)
        {
            Leader leader;
            Leader selected = GetSelectedLeader();
            if (selected != null)
            {
                // If there is a choice, the country tag or ID To take over and create an item
                leader = new Leader(selected)
                {
                    Id = Leaders.GetNewId(selected.Country),
                    Name = "",
                    PictureName = ""
                };

                // Set edited flags for each commander
                leader.SetDirtyAll();

                // Insert an item in the commander list
                Leaders.InsertItem(leader, selected);
                InsertListItem(leader, leaderListView.SelectedIndices[0] + 1);
            }
            else
            {
                Country country = Countries.Tags[countryListBox.SelectedIndex];
                // Create a new item
                leader = new Leader
                {
                    Country = country,
                    Id = Leaders.GetNewId(country),
                    Branch = Branch.None,
                    IdealRank = LeaderRank.None,
                    StartYear = 1930,
                    EndYear = 1990,
                    RetirementYear = 1999
                };
                leader.RankYear[0] = 1930;
                leader.RankYear[1] = 1990;
                leader.RankYear[2] = 1990;
                leader.RankYear[3] = 1990;

                // Set edited flags for each commander
                leader.SetDirtyAll();

                // Add an item to the commander list
                Leaders.AddItem(leader);
                AddListItem(leader);

                // Enable edit items
                EnableEditableItems();
            }

            // Set edited flags for each country
            Leaders.SetDirty(leader.Country);

            // If it does not exist in the file list, add it
            if (!Leaders.FileNameMap.ContainsKey(leader.Country))
            {
                Leaders.FileNameMap.Add(leader.Country, Game.GetLeaderFileName(leader.Country));
                Leaders.SetDirtyList();
            }
        }

        /// <summary>
        ///     Processing when the duplicate button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloneButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader selected = GetSelectedLeader();
            if (selected == null)
            {
                return;
            }

            // Create an item by taking over the selected item
            Leader leader = new Leader(selected)
            {
                Id = Leaders.GetNewId(selected.Country)
            };

            // Set edited flags for each commander
            leader.SetDirtyAll();

            // Insert an item in the commander list
            Leaders.InsertItem(leader, selected);
            InsertListItem(leader, leaderListView.SelectedIndices[0] + 1);

            // Set edited flags for each country
            Leaders.SetDirty(leader.Country);
        }

        /// <summary>
        ///     Processing when the delete button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader selected = GetSelectedLeader();
            if (selected == null)
            {
                return;
            }

            // Remove an item from the commander list
            Leaders.RemoveItem(selected);
            RemoveItem(leaderListView.SelectedIndices[0]);

            // Disable edit items when there are no items in the list
            if (leaderListView.Items.Count == 0)
            {
                DisableEditableItems();
            }

            // Set the edited flag
            Leaders.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Processing when the button is pressed to the beginning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTopButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader selected = GetSelectedLeader();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the top of the list
            int index = leaderListView.SelectedIndices[0];
            if (index == 0)
            {
                return;
            }

            Leader top = leaderListView.Items[0].Tag as Leader;
            if (top == null)
            {
                return;
            }

            // Move items in the commander list
            Leaders.MoveItem(selected, top);
            MoveListItem(index, 0);

            // Set the edited flag
            Leaders.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Processing when pressing the up button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader selected = GetSelectedLeader();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the top of the list
            int index = leaderListView.SelectedIndices[0];
            if (index == 0)
            {
                return;
            }

            Leader upper = leaderListView.Items[index - 1].Tag as Leader;
            if (upper == null)
            {
                return;
            }

            // Move items in the commander list
            Leaders.MoveItem(selected, upper);
            MoveListItem(index, index - 1);

            // Set the edited flag
            Leaders.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Processing when the down button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader selected = GetSelectedLeader();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the end of the list
            int index = leaderListView.SelectedIndices[0];
            if (index == leaderListView.Items.Count - 1)
            {
                return;
            }

            Leader lower = leaderListView.Items[index + 1].Tag as Leader;
            if (lower == null)
            {
                return;
            }

            // Move items in the commander list
            Leaders.MoveItem(selected, lower);
            MoveListItem(index, index + 1);

            // Set the edited flag
            Leaders.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Processing when the button is pressed to the end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBottomButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader selected = GetSelectedLeader();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the end of the list
            int index = leaderListView.SelectedIndices[0];
            if (index == leaderListView.Items.Count - 1)
            {
                return;
            }

            Leader bottom = leaderListView.Items[leaderListView.Items.Count - 1].Tag as Leader;
            if (bottom == null)
            {
                return;
            }

            // Move items in the commander list
            Leaders.MoveItem(selected, bottom);
            MoveListItem(index, leaderListView.Items.Count - 1);

            // Set the edited flag
            Leaders.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Add an item to the commander list
        /// </summary>
        /// <param name="leader">Items to be inserted</param>
        private void AddListItem(Leader leader)
        {
            // Add an item to the refined list
            _list.Add(leader);

            // Add an item to the commander list view
            leaderListView.Items.Add(CreateLeaderListViewItem(leader));

            // Select the added item
            leaderListView.Items[leaderListView.Items.Count - 1].Focused = true;
            leaderListView.Items[leaderListView.Items.Count - 1].Selected = true;
            leaderListView.EnsureVisible(leaderListView.Items.Count - 1);
        }

        /// <summary>
        ///     Insert an item in the commander list
        /// </summary>
        /// <param name="leader">Items to be inserted</param>
        /// <param name="index">Insertion destination position</param>
        private void InsertListItem(Leader leader, int index)
        {
            // Insert an item in the refined list
            _list.Insert(index, leader);

            // Insert an item in the commander list view
            ListViewItem item = CreateLeaderListViewItem(leader);
            leaderListView.Items.Insert(index, item);

            // Select the inserted item
            leaderListView.Items[index].Focused = true;
            leaderListView.Items[index].Selected = true;
            leaderListView.EnsureVisible(index);
        }

        /// <summary>
        ///     Remove an item from the commander list
        /// </summary>
        /// <param name="index">Position to be deleted</param>
        private void RemoveItem(int index)
        {
            // Remove an item from the refined list
            _list.RemoveAt(index);

            // Remove an item from the commander list view
            leaderListView.Items.RemoveAt(index);

            // Select the next item after the deleted item
            if (index < leaderListView.Items.Count)
            {
                leaderListView.Items[index].Focused = true;
                leaderListView.Items[index].Selected = true;
            }
            else if (index - 1 >= 0)
            {
                // At the end of the list, select the item before the deleted item
                leaderListView.Items[index - 1].Focused = true;
                leaderListView.Items[index - 1].Selected = true;
            }
        }

        /// <summary>
        ///     Move items in the commander list
        /// </summary>
        /// <param name="src">Source position</param>
        /// <param name="dest">Destination position</param>
        private void MoveListItem(int src, int dest)
        {
            Leader leader = _list[src];
            ListViewItem item = CreateLeaderListViewItem(leader);

            if (src > dest)
            {
                // When moving up
                // Move items in the refined list
                _list.Insert(dest, leader);
                _list.RemoveAt(src + 1);

                // Move items in the commander list view
                leaderListView.Items.Insert(dest, item);
                leaderListView.Items.RemoveAt(src + 1);
            }
            else
            {
                // When moving down
                // Move items in the refined list
                _list.Insert(dest + 1, leader);
                _list.RemoveAt(src);

                // Move items in the commander list view
                leaderListView.Items.Insert(dest + 1, item);
                leaderListView.Items.RemoveAt(src);
            }

            // Select the item to move to
            leaderListView.Items[dest].Focused = true;
            leaderListView.Items[dest].Selected = true;
            leaderListView.EnsureVisible(dest);
        }

        /// <summary>
        ///     Create an item in the commander list view
        /// </summary>
        /// <param name="leader">Commander data</param>
        /// <returns>Items in the commander list view</returns>
        private static ListViewItem CreateLeaderListViewItem(Leader leader)
        {
            ListViewItem item = new ListViewItem
            {
                Text = Countries.Strings[(int) leader.Country],
                Tag = leader
            };
            item.SubItems.Add(IntHelper.ToString(leader.Id));
            item.SubItems.Add(leader.Name);
            item.SubItems.Add(Branches.GetName(leader.Branch));
            item.SubItems.Add(IntHelper.ToString(leader.Skill));
            item.SubItems.Add(IntHelper.ToString(leader.MaxSkill));
            item.SubItems.Add(IntHelper.ToString(leader.StartYear));
            item.SubItems.Add(IntHelper.ToString(leader.EndYear));
            item.SubItems.Add(GetLeaderTraitsText(leader.Traits));

            return item;
        }

        /// <summary>
        ///     Get selected commander data
        /// </summary>
        /// <returns>Selected commander data</returns>
        private Leader GetSelectedLeader()
        {
            // If there is no selection
            if (leaderListView.SelectedItems.Count == 0)
            {
                return null;
            }

            return leaderListView.SelectedItems[0].Tag as Leader;
        }

        /// <summary>
        ///     Get the commander characteristic string
        /// </summary>
        /// <param name="traits">Commander characteristics</param>
        /// <returns>Commander characteristic string</returns>
        private static string GetLeaderTraitsText(uint traits)
        {
            string s = Enum.GetValues(typeof (LeaderTraitsId))
                .Cast<LeaderTraitsId>()
                .Where(id => (traits & Leaders.TraitsValues[(int) id]) != 0)
                .Aggregate("",
                    (current, id) =>
                        $"{current}, {Config.GetText(Leaders.TraitsNames[(int) id])}");
            // " Of the first item "","" To remove"
            if (!string.IsNullOrEmpty(s))
            {
                s = s.Substring(2);
            }

            return s;
        }

        #endregion

        #region search

        /// <summary>
        ///     Processing when the check status of the military check box changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBranchNarrowCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Update the commander list
            NarrowLeaderList();
            UpdateLeaderList();
        }

        /// <summary>
        ///     Processing when the check status of the characteristic narrowing check box changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTraitsNarrowCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (GetNarrowedTraits() == 0)
            {
                // To the characteristics 1 If none are checked, change without filter
                if (!traitsNoneNarrowRadioButton.Checked)
                {
                    traitsNoneNarrowRadioButton.Checked = true;
                }
            }
            else
            {
                // If the characteristics are checked without a filter OR Change to conditions
                if (traitsNoneNarrowRadioButton.Checked)
                {
                    traitsOrNarrowRadioButton.Checked = true;
                }
            }

            // Update the commander list
            NarrowLeaderList();
            UpdateLeaderList();
        }

        /// <summary>
        ///     Characteristics Narrowing down condition Radio button check Processing when the state changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTraitsNarrowRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            // Update the commander list
            NarrowLeaderList();
            UpdateLeaderList();
        }

        /// <summary>
        ///     Processing when the selection inversion button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTraitsNarrowInvertButtonClick(object sender, EventArgs e)
        {
            logisticsWizardNarrowCheckBox.Checked = !logisticsWizardNarrowCheckBox.Checked;
            defensiveDoctrineNarrowCheckBox.Checked = !defensiveDoctrineNarrowCheckBox.Checked;
            offensiveDoctrineNarrowCheckBox.Checked = !offensiveDoctrineNarrowCheckBox.Checked;
            winterSpecialistNarrowCheckBox.Checked = !winterSpecialistNarrowCheckBox.Checked;
            tricksterNarrowCheckBox.Checked = !tricksterNarrowCheckBox.Checked;
            engineerNarrowCheckBox.Checked = !engineerNarrowCheckBox.Checked;
            fortressBusterNarrowCheckBox.Checked = !fortressBusterNarrowCheckBox.Checked;
            panzerLeaderNarrowCheckBox.Checked = !panzerLeaderNarrowCheckBox.Checked;
            commandoNarrowCheckBox.Checked = !commandoNarrowCheckBox.Checked;
            oldGuardNarrowCheckBox.Checked = !oldGuardNarrowCheckBox.Checked;
            seaWolfNarrowCheckBox.Checked = !seaWolfNarrowCheckBox.Checked;
            blockadeRunnerNarrowCheckBox.Checked = !blockadeRunnerNarrowCheckBox.Checked;
            superiorTacticianNarrowCheckBox.Checked = !superiorTacticianNarrowCheckBox.Checked;
            spotterNarrowCheckBox.Checked = !spotterNarrowCheckBox.Checked;
            tankBusterNarrowCheckBox.Checked = !tankBusterNarrowCheckBox.Checked;
            carpetBomberNarrowCheckBox.Checked = !carpetBomberNarrowCheckBox.Checked;
            nightFlyerNarrowCheckBox.Checked = !nightFlyerNarrowCheckBox.Checked;
            fleetDestroyerNarrowCheckBox.Checked = !fleetDestroyerNarrowCheckBox.Checked;
            desertFoxNarrowCheckBox.Checked = !desertFoxNarrowCheckBox.Checked;
            jungleRatNarrowCheckBox.Checked = !jungleRatNarrowCheckBox.Checked;
            urbanWarfareSpecialistNarrowCheckBox.Checked = !urbanWarfareSpecialistNarrowCheckBox.Checked;
            rangerNarrowCheckBox.Checked = !rangerNarrowCheckBox.Checked;
            mountaineerNarrowCheckBox.Checked = !mountaineerNarrowCheckBox.Checked;
            hillsFighterNarrowCheckBox.Checked = !hillsFighterNarrowCheckBox.Checked;
            counterAttackerNarrowCheckBox.Checked = !counterAttackerNarrowCheckBox.Checked;
            assaulterNarrowCheckBox.Checked = !assaulterNarrowCheckBox.Checked;
            encirclerNarrowCheckBox.Checked = !encirclerNarrowCheckBox.Checked;
            ambusherNarrowCheckBox.Checked = !ambusherNarrowCheckBox.Checked;
            disciplinedNarrowCheckBox.Checked = !disciplinedNarrowCheckBox.Checked;
            elasticDefenceSpecialistNarrowCheckBox.Checked = !elasticDefenceSpecialistNarrowCheckBox.Checked;
            blitzerNarrowCheckBox.Checked = !blitzerNarrowCheckBox.Checked;
        }

        /// <summary>
        ///     Initialize the commander characteristic string
        /// </summary>
        private void InitTraitsText()
        {
            logisticsWizardCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.LogisticsWizard]);
            defensiveDoctrineCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.DefensiveDoctrine]);
            offensiveDoctrineCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.OffensiveDoctrine]);
            winterSpecialistCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.WinterSpecialist]);
            tricksterCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Trickster]);
            engineerCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Engineer]);
            fortressBusterCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.FortressBuster]);
            panzerLeaderCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.PanzerLeader]);
            commandoCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Commando]);
            oldGuardCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.OldGuard]);
            seaWolfCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.SeaWolf]);
            blockadeRunnerCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.BlockadeRunner]);
            superiorTacticianCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.SuperiorTactician]);
            spotterCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Spotter]);
            tankBusterCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.TankBuster]);
            carpetBomberCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.CarpetBomber]);
            nightFlyerCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.NightFlyer]);
            fleetDestroyerCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.FleetDestroyer]);
            desertFoxCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.DesertFox]);
            jungleRatCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.JungleRat]);
            urbanWarfareSpecialistCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.UrbanWarfareSpecialist]);
            rangerCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Ranger]);
            mountaineerCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Mountaineer]);
            hillsFighterCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.HillsFighter]);
            counterAttackerCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.CounterAttacker]);
            assaulterCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Assaulter]);
            encirclerCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Encircler]);
            ambusherCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Ambusher]);
            disciplinedCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Disciplined]);
            elasticDefenceSpecialistCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.ElasticDefenceSpecialist]);
            blitzerCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Blitzer]);

            logisticsWizardNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.LogisticsWizard]);
            defensiveDoctrineNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.DefensiveDoctrine]);
            offensiveDoctrineNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.OffensiveDoctrine]);
            winterSpecialistNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.WinterSpecialist]);
            tricksterNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Trickster]);
            engineerNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Engineer]);
            fortressBusterNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.FortressBuster]);
            panzerLeaderNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.PanzerLeader]);
            commandoNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Commando]);
            oldGuardNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.OldGuard]);
            seaWolfNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.SeaWolf]);
            blockadeRunnerNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.BlockadeRunner]);
            superiorTacticianNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.SuperiorTactician]);
            spotterNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Spotter]);
            tankBusterNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.TankBuster]);
            carpetBomberNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.CarpetBomber]);
            nightFlyerNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.NightFlyer]);
            fleetDestroyerNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.FleetDestroyer]);
            desertFoxNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.DesertFox]);
            jungleRatNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.JungleRat]);
            urbanWarfareSpecialistNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.UrbanWarfareSpecialist]);
            rangerNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Ranger]);
            mountaineerNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Mountaineer]);
            hillsFighterNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.HillsFighter]);
            counterAttackerNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.CounterAttacker]);
            assaulterNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Assaulter]);
            encirclerNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Encircler]);
            ambusherNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Ambusher]);
            disciplinedNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Disciplined]);
            elasticDefenceSpecialistNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.ElasticDefenceSpecialist]);
            blitzerNarrowCheckBox.Text =
                Config.GetText(Leaders.TraitsNames[(int) LeaderTraitsId.Blitzer]);
        }

        /// <summary>
        ///     Acquire narrowed commander characteristics
        /// </summary>
        /// <returns>Commander characteristics</returns>
        private uint GetNarrowedTraits()
        {
            uint traits = 0;

            // Station management
            if (logisticsWizardNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.LogisticsWizard;
            }
            // Defensive doctrine
            if (defensiveDoctrineNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.DefensiveDoctrine;
            }
            // Offensive doctrine
            if (offensiveDoctrineNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.OffensiveDoctrine;
            }
            // Winter battle
            if (winterSpecialistNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.WinterSpecialist;
            }
            // Assault
            if (tricksterNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.Trickster;
            }
            // Engineer
            if (engineerNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.Engineer;
            }
            // Fortress attack
            if (fortressBusterNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.FortressBuster;
            }
            // Armored battle
            if (panzerLeaderNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.PanzerLeader;
            }
            // Special battle
            if (commandoNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.Commando;
            }
            // Classic school
            if (oldGuardNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.OldGuard;
            }
            // Sea wolf
            if (seaWolfNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.SeaWolf;
            }
            // Master of blocking line breakthrough
            if (blockadeRunnerNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.BlockadeRunner;
            }
            // Outstanding tactician
            if (superiorTacticianNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.SuperiorTactician;
            }
            // Searching enemy
            if (spotterNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.Spotter;
            }
            // Anti-tank attack
            if (tankBusterNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.TankBuster;
            }
            // Rug bombing
            if (carpetBomberNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.CarpetBomber;
            }
            // Night air operations
            if (nightFlyerNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.NightFlyer;
            }
            // Anti-ship attack
            if (fleetDestroyerNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.FleetDestroyer;
            }
            // Desert fox
            if (desertFoxNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.DesertFox;
            }
            // Dense forest rat
            if (jungleRatNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.JungleRat;
            }
            // Urban warfare
            if (urbanWarfareSpecialistNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.UrbanWarfareSpecialist;
            }
            // Ranger
            if (rangerNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.Ranger;
            }
            // Mountain warfare
            if (mountaineerNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.Mountaineer;
            }
            // The Front Line
            if (hillsFighterNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.HillsFighter;
            }
            // Counterattack
            if (counterAttackerNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.CounterAttacker;
            }
            // Assault battle
            if (assaulterNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.Assaulter;
            }
            // Siege
            if (encirclerNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.Encircler;
            }
            // Surprise battle
            if (ambusherNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.Ambusher;
            }
            // Discipline
            if (disciplinedNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.Disciplined;
            }
            // Tactical retreat
            if (elasticDefenceSpecialistNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.ElasticDefenceSpecialist;
            }
            // Blitzkrieg
            if (blitzerNarrowCheckBox.Checked)
            {
                traits |= LeaderTraits.Blitzer;
            }

            return traits;
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
            foreach (Country country in Countries.Tags)
            {
                countryListBox.Items.Add(Countries.Strings[(int) country]);
            }

            // Processing selection events takes time, so temporarily disable it
            countryListBox.SelectedIndexChanged -= OnCountryListBoxSelectedIndexChanged;
            // Reflect the selected nation
            foreach (Country country in HoI2EditorController.Settings.LeaderEditor.Countries)
            {
                int index = Array.IndexOf(Countries.Tags, country);
                if (index >= 0)
                {
                    countryListBox.SetSelected(Array.IndexOf(Countries.Tags, country), true);
                }
            }
            // Undo selection event
            countryListBox.SelectedIndexChanged += OnCountryListBoxSelectedIndexChanged;

            int count = countryListBox.SelectedItems.Count;
            // Select all according to the number of selections / / Switch all cancellations
            countryAllButton.Text = count <= 1 ? Resources.KeySelectAll : Resources.KeyUnselectAll;
            // Disable the add new button if the number of selections is zero
            newButton.Enabled = count > 0;

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
                brush = Leaders.IsDirty(country) ? new SolidBrush(Color.Red) : new SolidBrush(SystemColors.WindowText);
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
            int count = countryListBox.SelectedItems.Count;

            // Select all according to the number of selections / / Switch all cancellations
            countryAllButton.Text = count <= 1 ? Resources.KeySelectAll : Resources.KeyUnselectAll;

            // Disable the add new button if the number of selections is zero
            newButton.Enabled = count > 0;

            // Save the selected nation
            HoI2EditorController.Settings.LeaderEditor.Countries =
                countryListBox.SelectedIndices.Cast<int>().Select(index => Countries.Tags[index]).ToList();

            // Update the commander list
            NarrowLeaderList();
            UpdateLeaderList();
        }

        /// <summary>
        ///     Select all national list boxes / / Processing when all release buttons are pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryAllButtonClick(object sender, EventArgs e)
        {
            countryListBox.BeginUpdate();

            // Processing selection events takes time, so temporarily disable it
            countryListBox.SelectedIndexChanged -= OnCountryListBoxSelectedIndexChanged;

            if (countryListBox.SelectedItems.Count <= 1)
            {
                // Select in reverse order to set the scroll position to the beginning
                for (int i = countryListBox.Items.Count - 1; i >= 0; i--)
                {
                    countryListBox.SetSelected(i, true);
                }
            }
            else
            {
                for (int i = 0; i < countryListBox.Items.Count; i++)
                {
                    countryListBox.SetSelected(i, false);
                }
            }

            // Undo selection event
            countryListBox.SelectedIndexChanged += OnCountryListBoxSelectedIndexChanged;

            // Issue a dummy event to narrow down the commander list
            OnCountryListBoxSelectedIndexChanged(sender, e);

            countryListBox.EndUpdate();
        }

        #endregion

        #region Edit items

        /// <summary>
        ///     Initialize edit items
        /// </summary>
        private void InitEditableItems()
        {
            Graphics g = Graphics.FromHwnd(Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;

            // Country tag
            countryComboBox.BeginUpdate();
            countryComboBox.Items.Clear();
            int width = countryComboBox.Width;
            foreach (string s in Countries.Tags
                .Select(country => Countries.Strings[(int) country])
                .Select(name => Config.ExistsKey(name)
                    ? $"{name} {Config.GetText(name)}"
                    : name))
            {
                countryComboBox.Items.Add(s);
                width = Math.Max(width,
                    (int) g.MeasureString(s, countryComboBox.Font).Width + SystemInformation.VerticalScrollBarWidth +
                    margin);
            }
            countryComboBox.DropDownWidth = width;
            countryComboBox.EndUpdate();

            // Army
            branchComboBox.BeginUpdate();
            branchComboBox.Items.Clear();
            width = branchComboBox.Width;
            foreach (string s in Branches.GetNames())
            {
                branchComboBox.Items.Add(s);
                width = Math.Max(width, (int) g.MeasureString(s, branchComboBox.Font).Width + margin);
            }
            branchComboBox.DropDownWidth = width;
            branchComboBox.EndUpdate();

            armyNarrowCheckBox.Text = Branches.GetName(Branch.Army);
            navyNarrowCheckBox.Text = Branches.GetName(Branch.Navy);
            airforceNarrowCheckBox.Text = Branches.GetName(Branch.Airforce);

            // class
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
        }

        /// <summary>
        ///     Update edit items
        /// </summary>
        private void UpdateEditableItems()
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Update edit items
            UpdateEditableItemsValue(leader);

            // Update the color of the edit item
            UpdateEditableItemsColor(leader);

            // Item move button status update
            int index = leaderListView.SelectedIndices[0];
            int bottom = leaderListView.Items.Count - 1;
            topButton.Enabled = index != 0;
            upButton.Enabled = index != 0;
            downButton.Enabled = index != bottom;
            bottomButton.Enabled = index != bottom;
        }

        /// <summary>
        ///     Update the value of the edit item
        /// </summary>
        /// <param name="leader">Commander data</param>
        private void UpdateEditableItemsValue(Leader leader)
        {
            // Update the value of the edit item
            countryComboBox.SelectedIndex = leader.Country != Country.None ? (int) leader.Country - 1 : -1;
            idNumericUpDown.Value = leader.Id;
            nameTextBox.Text = leader.Name;
            branchComboBox.SelectedIndex = leader.Branch != Branch.None ? (int) leader.Branch - 1 : -1;
            idealRankComboBox.SelectedIndex = leader.IdealRank != LeaderRank.None ? (int) leader.IdealRank - 1 : -1;
            skillNumericUpDown.Value = leader.Skill;
            maxSkillNumericUpDown.Value = leader.MaxSkill;
            experienceNumericUpDown.Value = leader.Experience;
            loyaltyNumericUpDown.Value = leader.Loyalty;
            startYearNumericUpDown.Value = leader.StartYear;
            endYearNumericUpDown.Value = leader.EndYear;
            if (Misc.EnableRetirementYearLeaders)
            {
                retirementYearLabel.Enabled = true;
                retirementYearNumericUpDown.Enabled = true;
                retirementYearNumericUpDown.Value = leader.RetirementYear;
                retirementYearNumericUpDown.Text = IntHelper.ToString((int) retirementYearNumericUpDown.Value);
            }
            else
            {
                retirementYearLabel.Enabled = false;
                retirementYearNumericUpDown.Enabled = false;
                retirementYearNumericUpDown.ResetText();
            }
            rankYearNumericUpDown1.Value = leader.RankYear[0];
            rankYearNumericUpDown2.Value = leader.RankYear[1];
            rankYearNumericUpDown3.Value = leader.RankYear[2];
            rankYearNumericUpDown4.Value = leader.RankYear[3];
            pictureNameTextBox.Text = leader.PictureName;
            UpdateLeaderPicture(leader);

            // Update the state of the characteristic check box
            logisticsWizardCheckBox.Checked = (leader.Traits & LeaderTraits.LogisticsWizard) != 0;
            defensiveDoctrineCheckBox.Checked = (leader.Traits & LeaderTraits.DefensiveDoctrine) != 0;
            offensiveDoctrineCheckBox.Checked = (leader.Traits & LeaderTraits.OffensiveDoctrine) != 0;
            winterSpecialistCheckBox.Checked = (leader.Traits & LeaderTraits.WinterSpecialist) != 0;
            tricksterCheckBox.Checked = (leader.Traits & LeaderTraits.Trickster) != 0;
            engineerCheckBox.Checked = (leader.Traits & LeaderTraits.Engineer) != 0;
            fortressBusterCheckBox.Checked = (leader.Traits & LeaderTraits.FortressBuster) != 0;
            panzerLeaderCheckBox.Checked = (leader.Traits & LeaderTraits.PanzerLeader) != 0;
            commandoCheckBox.Checked = (leader.Traits & LeaderTraits.Commando) != 0;
            oldGuardCheckBox.Checked = (leader.Traits & LeaderTraits.OldGuard) != 0;
            seaWolfCheckBox.Checked = (leader.Traits & LeaderTraits.SeaWolf) != 0;
            blockadeRunnerCheckBox.Checked = (leader.Traits & LeaderTraits.BlockadeRunner) != 0;
            superiorTacticianCheckBox.Checked = (leader.Traits & LeaderTraits.SuperiorTactician) != 0;
            spotterCheckBox.Checked = (leader.Traits & LeaderTraits.Spotter) != 0;
            tankBusterCheckBox.Checked = (leader.Traits & LeaderTraits.TankBuster) != 0;
            carpetBomberCheckBox.Checked = (leader.Traits & LeaderTraits.CarpetBomber) != 0;
            nightFlyerCheckBox.Checked = (leader.Traits & LeaderTraits.NightFlyer) != 0;
            fleetDestroyerCheckBox.Checked = (leader.Traits & LeaderTraits.FleetDestroyer) != 0;
            desertFoxCheckBox.Checked = (leader.Traits & LeaderTraits.DesertFox) != 0;
            jungleRatCheckBox.Checked = (leader.Traits & LeaderTraits.JungleRat) != 0;
            urbanWarfareSpecialistCheckBox.Checked = (leader.Traits & LeaderTraits.UrbanWarfareSpecialist) != 0;
            rangerCheckBox.Checked = (leader.Traits & LeaderTraits.Ranger) != 0;
            mountaineerCheckBox.Checked = (leader.Traits & LeaderTraits.Mountaineer) != 0;
            hillsFighterCheckBox.Checked = (leader.Traits & LeaderTraits.HillsFighter) != 0;
            counterAttackerCheckBox.Checked = (leader.Traits & LeaderTraits.CounterAttacker) != 0;
            assaulterCheckBox.Checked = (leader.Traits & LeaderTraits.Assaulter) != 0;
            encirclerCheckBox.Checked = (leader.Traits & LeaderTraits.Encircler) != 0;
            ambusherCheckBox.Checked = (leader.Traits & LeaderTraits.Ambusher) != 0;
            disciplinedCheckBox.Checked = (leader.Traits & LeaderTraits.Disciplined) != 0;
            elasticDefenceSpecialistCheckBox.Checked = (leader.Traits & LeaderTraits.ElasticDefenceSpecialist) != 0;
            blitzerCheckBox.Checked = (leader.Traits & LeaderTraits.Blitzer) != 0;
        }

        /// <summary>
        ///     Update the color of the edit item
        /// </summary>
        /// <param name="leader"></param>
        private void UpdateEditableItemsColor(Leader leader)
        {
            // Update the color of the combo box
            countryComboBox.Refresh();
            branchComboBox.Refresh();
            idealRankComboBox.Refresh();

            // Update the color of the edit item
            idNumericUpDown.ForeColor = leader.IsDirty(LeaderItemId.Id) ? Color.Red : SystemColors.WindowText;
            nameTextBox.ForeColor = leader.IsDirty(LeaderItemId.Name) ? Color.Red : SystemColors.WindowText;
            skillNumericUpDown.ForeColor = leader.IsDirty(LeaderItemId.Skill) ? Color.Red : SystemColors.WindowText;
            maxSkillNumericUpDown.ForeColor = leader.IsDirty(LeaderItemId.MaxSkill)
                ? Color.Red
                : SystemColors.WindowText;
            experienceNumericUpDown.ForeColor = leader.IsDirty(LeaderItemId.Experience)
                ? Color.Red
                : SystemColors.WindowText;
            loyaltyNumericUpDown.ForeColor = leader.IsDirty(LeaderItemId.Loyalty) ? Color.Red : SystemColors.WindowText;
            startYearNumericUpDown.ForeColor = leader.IsDirty(LeaderItemId.StartYear)
                ? Color.Red
                : SystemColors.WindowText;
            endYearNumericUpDown.ForeColor = leader.IsDirty(LeaderItemId.EndYear) ? Color.Red : SystemColors.WindowText;
            retirementYearNumericUpDown.ForeColor = leader.IsDirty(LeaderItemId.RetirementYear)
                ? Color.Red
                : SystemColors.WindowText;
            rankYearNumericUpDown1.ForeColor = leader.IsDirty(LeaderItemId.Rank3Year)
                ? Color.Red
                : SystemColors.WindowText;
            rankYearNumericUpDown2.ForeColor = leader.IsDirty(LeaderItemId.Rank2Year)
                ? Color.Red
                : SystemColors.WindowText;
            rankYearNumericUpDown3.ForeColor = leader.IsDirty(LeaderItemId.Rank1Year)
                ? Color.Red
                : SystemColors.WindowText;
            rankYearNumericUpDown4.ForeColor = leader.IsDirty(LeaderItemId.Rank0Year)
                ? Color.Red
                : SystemColors.WindowText;
            pictureNameTextBox.ForeColor = leader.IsDirty(LeaderItemId.PictureName)
                ? Color.Red
                : SystemColors.WindowText;

            // Update the item color of the characteristic check box
            logisticsWizardCheckBox.ForeColor = leader.IsDirty(LeaderItemId.LogisticsWizard)
                ? Color.Red
                : SystemColors.WindowText;
            defensiveDoctrineCheckBox.ForeColor = leader.IsDirty(LeaderItemId.DefensiveDoctrine)
                ? Color.Red
                : SystemColors.WindowText;
            offensiveDoctrineCheckBox.ForeColor = leader.IsDirty(LeaderItemId.OffensiveDoctrine)
                ? Color.Red
                : SystemColors.WindowText;
            winterSpecialistCheckBox.ForeColor = leader.IsDirty(LeaderItemId.WinterSpecialist)
                ? Color.Red
                : SystemColors.WindowText;
            tricksterCheckBox.ForeColor = leader.IsDirty(LeaderItemId.Trickster) ? Color.Red : SystemColors.WindowText;
            engineerCheckBox.ForeColor = leader.IsDirty(LeaderItemId.Engineer) ? Color.Red : SystemColors.WindowText;
            fortressBusterCheckBox.ForeColor = leader.IsDirty(LeaderItemId.FortressBuster)
                ? Color.Red
                : SystemColors.WindowText;
            panzerLeaderCheckBox.ForeColor = leader.IsDirty(LeaderItemId.PanzerLeader)
                ? Color.Red
                : SystemColors.WindowText;
            commandoCheckBox.ForeColor = leader.IsDirty(LeaderItemId.Commando) ? Color.Red : SystemColors.WindowText;
            oldGuardCheckBox.ForeColor = leader.IsDirty(LeaderItemId.OldGuard) ? Color.Red : SystemColors.WindowText;
            seaWolfCheckBox.ForeColor = leader.IsDirty(LeaderItemId.SeaWolf) ? Color.Red : SystemColors.WindowText;
            blockadeRunnerCheckBox.ForeColor = leader.IsDirty(LeaderItemId.BlockadeRunner)
                ? Color.Red
                : SystemColors.WindowText;
            superiorTacticianCheckBox.ForeColor = leader.IsDirty(LeaderItemId.SuperiorTactician)
                ? Color.Red
                : SystemColors.WindowText;
            spotterCheckBox.ForeColor = leader.IsDirty(LeaderItemId.Spotter) ? Color.Red : SystemColors.WindowText;
            tankBusterCheckBox.ForeColor = leader.IsDirty(LeaderItemId.TankBuster) ? Color.Red : SystemColors.WindowText;
            carpetBomberCheckBox.ForeColor = leader.IsDirty(LeaderItemId.CarpetBomber)
                ? Color.Red
                : SystemColors.WindowText;
            nightFlyerCheckBox.ForeColor = leader.IsDirty(LeaderItemId.NightFlyer) ? Color.Red : SystemColors.WindowText;
            fleetDestroyerCheckBox.ForeColor = leader.IsDirty(LeaderItemId.FleetDestroyer)
                ? Color.Red
                : SystemColors.WindowText;
            desertFoxCheckBox.ForeColor = leader.IsDirty(LeaderItemId.DesertFox) ? Color.Red : SystemColors.WindowText;
            jungleRatCheckBox.ForeColor = leader.IsDirty(LeaderItemId.JungleRat) ? Color.Red : SystemColors.WindowText;
            urbanWarfareSpecialistCheckBox.ForeColor = leader.IsDirty(LeaderItemId.UrbanWarfareSpecialist)
                ? Color.Red
                : SystemColors.WindowText;
            rangerCheckBox.ForeColor = leader.IsDirty(LeaderItemId.Ranger) ? Color.Red : SystemColors.WindowText;
            mountaineerCheckBox.ForeColor = leader.IsDirty(LeaderItemId.Mountaineer)
                ? Color.Red
                : SystemColors.WindowText;
            hillsFighterCheckBox.ForeColor = leader.IsDirty(LeaderItemId.HillsFighter)
                ? Color.Red
                : SystemColors.WindowText;
            counterAttackerCheckBox.ForeColor = leader.IsDirty(LeaderItemId.CounterAttacker)
                ? Color.Red
                : SystemColors.WindowText;
            assaulterCheckBox.ForeColor = leader.IsDirty(LeaderItemId.Assaulter) ? Color.Red : SystemColors.WindowText;
            encirclerCheckBox.ForeColor = leader.IsDirty(LeaderItemId.Encircler) ? Color.Red : SystemColors.WindowText;
            ambusherCheckBox.ForeColor = leader.IsDirty(LeaderItemId.Ambusher) ? Color.Red : SystemColors.WindowText;
            disciplinedCheckBox.ForeColor = leader.IsDirty(LeaderItemId.Disciplined)
                ? Color.Red
                : SystemColors.WindowText;
            elasticDefenceSpecialistCheckBox.ForeColor = leader.IsDirty(LeaderItemId.ElasticDefenceSpecialist)
                ? Color.Red
                : SystemColors.WindowText;
            blitzerCheckBox.ForeColor = leader.IsDirty(LeaderItemId.Blitzer) ? Color.Red : SystemColors.WindowText;
        }

        /// <summary>
        ///     Enable edit items
        /// </summary>
        private void EnableEditableItems()
        {
            countryComboBox.Enabled = true;
            idNumericUpDown.Enabled = true;
            nameTextBox.Enabled = true;
            branchComboBox.Enabled = true;
            idealRankComboBox.Enabled = true;
            skillNumericUpDown.Enabled = true;
            maxSkillNumericUpDown.Enabled = true;
            experienceNumericUpDown.Enabled = true;
            loyaltyNumericUpDown.Enabled = true;
            startYearNumericUpDown.Enabled = true;
            endYearNumericUpDown.Enabled = true;
            rankYearNumericUpDown1.Enabled = true;
            rankYearNumericUpDown2.Enabled = true;
            rankYearNumericUpDown3.Enabled = true;
            rankYearNumericUpDown4.Enabled = true;
            pictureNameTextBox.Enabled = true;
            pictureNameBrowseButton.Enabled = true;
            traitsGroupBox.Enabled = true;

            // Reset the character string cleared at the time of invalidation
            idNumericUpDown.Text = IntHelper.ToString((int) idNumericUpDown.Value);
            skillNumericUpDown.Text = IntHelper.ToString((int) skillNumericUpDown.Value);
            maxSkillNumericUpDown.Text = IntHelper.ToString((int) maxSkillNumericUpDown.Value);
            experienceNumericUpDown.Text = IntHelper.ToString((int) experienceNumericUpDown.Value);
            loyaltyNumericUpDown.Text = IntHelper.ToString((int) loyaltyNumericUpDown.Value);
            startYearNumericUpDown.Text = IntHelper.ToString((int) startYearNumericUpDown.Value);
            endYearNumericUpDown.Text = IntHelper.ToString((int) endYearNumericUpDown.Value);
            rankYearNumericUpDown1.Text = IntHelper.ToString((int) rankYearNumericUpDown1.Value);
            rankYearNumericUpDown2.Text = IntHelper.ToString((int) rankYearNumericUpDown2.Value);
            rankYearNumericUpDown3.Text = IntHelper.ToString((int) rankYearNumericUpDown3.Value);
            rankYearNumericUpDown4.Text = IntHelper.ToString((int) rankYearNumericUpDown4.Value);

            if (Misc.EnableRetirementYearLeaders)
            {
                retirementYearNumericUpDown.Enabled = true;
                retirementYearNumericUpDown.Text = IntHelper.ToString((int) retirementYearNumericUpDown.Value);
            }

            cloneButton.Enabled = true;
            removeButton.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items
        /// </summary>
        private void DisableEditableItems()
        {
            countryComboBox.SelectedIndex = -1;
            countryComboBox.ResetText();
            idNumericUpDown.ResetText();
            nameTextBox.ResetText();
            branchComboBox.SelectedIndex = -1;
            branchComboBox.ResetText();
            idealRankComboBox.SelectedIndex = -1;
            idealRankComboBox.ResetText();
            skillNumericUpDown.ResetText();
            maxSkillNumericUpDown.ResetText();
            experienceNumericUpDown.ResetText();
            loyaltyNumericUpDown.ResetText();
            startYearNumericUpDown.ResetText();
            endYearNumericUpDown.ResetText();
            retirementYearNumericUpDown.ResetText();
            rankYearNumericUpDown1.ResetText();
            rankYearNumericUpDown2.ResetText();
            rankYearNumericUpDown3.ResetText();
            rankYearNumericUpDown4.ResetText();
            pictureNameTextBox.ResetText();
            leaderPictureBox.ImageLocation = "";

            ResetTraitsCheckBoxValue();

            countryComboBox.Enabled = false;
            idNumericUpDown.Enabled = false;
            nameTextBox.Enabled = false;
            branchComboBox.Enabled = false;
            idealRankComboBox.Enabled = false;
            skillNumericUpDown.Enabled = false;
            maxSkillNumericUpDown.Enabled = false;
            experienceNumericUpDown.Enabled = false;
            loyaltyNumericUpDown.Enabled = false;
            startYearNumericUpDown.Enabled = false;
            endYearNumericUpDown.Enabled = false;
            retirementYearNumericUpDown.Enabled = false;
            rankYearNumericUpDown1.Enabled = false;
            rankYearNumericUpDown2.Enabled = false;
            rankYearNumericUpDown3.Enabled = false;
            rankYearNumericUpDown4.Enabled = false;
            pictureNameTextBox.Enabled = false;
            pictureNameBrowseButton.Enabled = false;
            traitsGroupBox.Enabled = false;

            cloneButton.Enabled = false;
            removeButton.Enabled = false;
            topButton.Enabled = false;
            upButton.Enabled = false;
            downButton.Enabled = false;
            bottomButton.Enabled = false;
        }

        /// <summary>
        ///     Item drawing process of national combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Leader leader = GetSelectedLeader();
            if (leader != null)
            {
                Brush brush;
                if ((Countries.Tags[e.Index] == leader.Country) && leader.IsDirty(LeaderItemId.Country))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = countryComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
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

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Leader leader = GetSelectedLeader();
            if (leader != null)
            {
                Brush brush;
                if ((e.Index == (int) leader.Branch - 1) && leader.IsDirty(LeaderItemId.Branch))
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
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of ideal class combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIdealRankComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Leader leader = GetSelectedLeader();
            if (leader != null)
            {
                Brush brush;
                if ((e.Index == (int) leader.IdealRank - 1) && leader.IsDirty(LeaderItemId.IdealRank))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = idealRankComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Update commander image picture box items
        /// </summary>
        /// <param name="leader">Commander data</param>
        private void UpdateLeaderPicture(Leader leader)
        {
            if (!string.IsNullOrEmpty(leader.PictureName) &&
                (leader.PictureName.IndexOfAny(Path.GetInvalidPathChars()) < 0))
            {
                string fileName = Game.GetReadFileName(Game.PersonPicturePathName,
                    Path.ChangeExtension(leader.PictureName, ".bmp"));
                leaderPictureBox.ImageLocation = File.Exists(fileName) ? fileName : "";
            }
            else
            {
                leaderPictureBox.ImageLocation = "";
            }
        }

        /// <summary>
        ///     Processing when changing country tag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            Country country = Countries.Tags[countryComboBox.SelectedIndex];
            if (country == leader.Country)
            {
                return;
            }

            // Set the edited flag for the country tag before the change
            Leaders.SetDirty(leader.Country);

            Log.Info("[Leader] country: {0} -> {1} ({2}: {3})", Countries.Strings[(int) leader.Country],
                Countries.Strings[(int) country], leader.Id, leader.Name);

            // Update value
            leader.Country = country;

            // Update items in the commander list view
            leaderListView.SelectedItems[0].Text = Countries.Strings[(int) leader.Country];

            // Set edited flags for each commander
            leader.SetDirty(LeaderItemId.Country);

            // Set the edited flag of the changed country tag
            Leaders.SetDirty(leader.Country);

            // If it does not exist in the file list, add it
            if (!Leaders.FileNameMap.ContainsKey(leader.Country))
            {
                Leaders.FileNameMap.Add(leader.Country, Game.GetLeaderFileName(leader.Country));
                Leaders.SetDirtyList();
            }

            // Update drawing to change the item color of the national combo box
            countryComboBox.Refresh();

            // Update drawing to change the item color of the national list box
            countryListBox.Refresh();
        }

        /// <summary>
        ///     ID Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIdNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int id = (int) idNumericUpDown.Value;
            if (id == leader.Id)
            {
                return;
            }

            Log.Info("[Leader] id: {0} -> {1} ({2})", leader.Id, id, leader.Name);

            // Update value
            leader.Id = id;

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[1].Text = IntHelper.ToString(leader.Id);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Id);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            idNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the name string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            string name = nameTextBox.Text;
            if (string.IsNullOrEmpty(name))
            {
                if (string.IsNullOrEmpty(leader.Name))
                {
                    return;
                }
            }
            else
            {
                if (name.Equals(leader.Name))
                {
                    return;
                }
            }

            Log.Info("[Leader] name: {0} -> {1} ({2})", leader.Name, name, leader.Id);

            // Update value
            leader.Name = name;

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[2].Text = leader.Name;

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Name);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            nameTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing military department
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBranchComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            Branch branch = (Branch) (branchComboBox.SelectedIndex + 1);
            if (branch == leader.Branch)
            {
                return;
            }

            Log.Info("[Leader] branch: {0} -> {1} ({2}: {3})", Branches.GetName(leader.Branch), Branches.GetName(branch),
                leader.Id, leader.Name);

            // Update value
            leader.Branch = branch;

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[3].Text = Branches.GetName(leader.Branch);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Branch);
            Leaders.SetDirty(leader.Country);

            // Update drawing to change the item color of the military combo box
            branchComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when changing the ideal class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIdealRankComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            LeaderRank idealRank = (LeaderRank) (idealRankComboBox.SelectedIndex + 1);
            if (idealRank == leader.IdealRank)
            {
                return;
            }

            Log.Info("[Leader] ideak rank: {0} -> {1} ({2}: {3})", Leaders.RankNames[(int) leader.IdealRank],
                Leaders.RankNames[(int) idealRank], leader.Id, leader.Name);

            // Update value
            leader.IdealRank = idealRank;

            // Set the edited flag
            leader.SetDirty(LeaderItemId.IdealRank);
            Leaders.SetDirty(leader.Country);

            // Update drawing to change the item color of the ideal class combo box
            idealRankComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when changing skills
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSkillNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int skill = (int) skillNumericUpDown.Value;
            if (skill == leader.Skill)
            {
                return;
            }

            Log.Info("[Leader] skill: {0} -> {1} ({2}: {3})", leader.Skill, skill, leader.Id, leader.Name);

            // Update value
            leader.Skill = skill;

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[4].Text = IntHelper.ToString(leader.Skill);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Skill);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            skillNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the maximum skill
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMaxSkillNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int maxSkill = (int) maxSkillNumericUpDown.Value;
            if (maxSkill == leader.MaxSkill)
            {
                return;
            }

            Log.Info("[Leader] max skill: {0} -> {1} ({2}: {3})", leader.MaxSkill, maxSkill, leader.Id, leader.Name);

            // Update value
            leader.MaxSkill = maxSkill;

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[5].Text = IntHelper.ToString(leader.MaxSkill);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.MaxSkill);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            maxSkillNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing experience value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExperienceNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int experience = (int) experienceNumericUpDown.Value;
            if (experience == leader.Experience)
            {
                return;
            }

            Log.Info("[Leader] experience: {0} -> {1} ({2}: {3})", leader.Experience, experience, leader.Id, leader.Name);

            // Update value
            leader.Experience = experience;

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Experience);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            experienceNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing loyalty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoyaltyNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }
            // Do nothing if the value does not change
            int loyalty = (int) loyaltyNumericUpDown.Value;
            if (loyalty == leader.Loyalty)
            {
                return;
            }

            Log.Info("[Leader] loyalty: {0} -> {1} ({2}: {3})", leader.Loyalty, loyalty, leader.Id, leader.Name);

            // Update value
            leader.Loyalty = loyalty;

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Loyalty);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            loyaltyNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the start year
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartYearNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int startYear = (int) startYearNumericUpDown.Value;
            if (startYear == leader.StartYear)
            {
                return;
            }

            Log.Info("[Leader] start year: {0} -> {1} ({2}: {3})", leader.StartYear, startYear, leader.Id, leader.Name);

            // Update value
            leader.StartYear = startYear;

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[6].Text = IntHelper.ToString(leader.StartYear);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.StartYear);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            startYearNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the end year
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEndYearNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int endYear = (int) endYearNumericUpDown.Value;
            if (endYear == leader.EndYear)
            {
                return;
            }

            Log.Info("[Leader] end year: {0} -> {1} ({2}: {3})", leader.EndYear, endYear, leader.Id, leader.Name);

            // Update value
            leader.EndYear = endYear;

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[7].Text = IntHelper.ToString(leader.EndYear);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.EndYear);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            endYearNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing retirement year
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRetirementYearNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int retirementYear = (int) retirementYearNumericUpDown.Value;
            if (retirementYear == leader.RetirementYear)
            {
                return;
            }

            Log.Info("[Leader] retirement year: {0} -> {1} ({2}: {3})", leader.RetirementYear, retirementYear, leader.Id,
                leader.Name);

            // Update value
            leader.RetirementYear = retirementYear;

            // Set the edited flag
            leader.SetDirty(LeaderItemId.RetirementYear);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            retirementYearNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the major general year is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearNumericUpDown1ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int year = (int) rankYearNumericUpDown1.Value;
            if (year == leader.RankYear[0])
            {
                return;
            }

            Log.Info("[Leader] rank3 year: {0} -> {1} ({2}: {3})", leader.RankYear[0], year, leader.Id, leader.Name);

            // Update value
            leader.RankYear[0] = year;

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Rank3Year);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            rankYearNumericUpDown1.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the year of the middle general
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearNumericUpDown2ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int year = (int) rankYearNumericUpDown2.Value;
            if (year == leader.RankYear[1])
            {
                return;
            }

            Log.Info("[Leader] rank2 year: {0} -> {1} ({2}: {3})", leader.RankYear[1], year, leader.Id, leader.Name);

            // Update value
            leader.RankYear[1] = year;

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Rank2Year);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            rankYearNumericUpDown2.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the general officer's year is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearNumericUpDown3ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int year = (int) rankYearNumericUpDown3.Value;
            if (year == leader.RankYear[2])
            {
                return;
            }

            Log.Info("[Leader] rank1 year: {0} -> {1} ({2}: {3})", leader.RankYear[2], year, leader.Id, leader.Name);

            // Update value
            leader.RankYear[2] = year;

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Rank1Year);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            rankYearNumericUpDown3.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the year of Marshal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankYearNumericUpDown4ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int year = (int) rankYearNumericUpDown4.Value;
            if (year == leader.RankYear[3])
            {
                return;
            }

            Log.Info("[Leader] rank0 year: {0} -> {1} ({2}: {3})", leader.RankYear[3], year, leader.Id, leader.Name);

            // Update value
            leader.RankYear[3] = year;

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Rank0Year);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            rankYearNumericUpDown4.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the image file name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPictureNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            string pictureName = pictureNameTextBox.Text;
            if (string.IsNullOrEmpty(pictureName))
            {
                if (string.IsNullOrEmpty(leader.PictureName))
                {
                    return;
                }
            }
            else
            {
                if (pictureName.Equals(leader.PictureName))
                {
                    return;
                }
            }

            Log.Info("[Leader] picture name: {0} -> {1} ({2}: {3})", leader.PictureName, pictureName, leader.Id,
                leader.Name);

            // Update value
            leader.PictureName = pictureName;

            // Update commander image
            UpdateLeaderPicture(leader);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.PictureName);
            Leaders.SetDirty(leader.Country);

            // Change the font color
            pictureNameTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the image file name reference button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPictureNameReferButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Open the file selection dialog
            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = Path.Combine(Game.FolderName, Game.PersonPicturePathName),
                FileName = leader.PictureName,
                Filter = Resources.OpenBitmapFileDialogFilter
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pictureNameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }

        #endregion

        #region Commander characteristics

        /// <summary>
        ///     Set the values of the commander characteristic check box at once
        /// </summary>
        private void ResetTraitsCheckBoxValue()
        {
            logisticsWizardCheckBox.Checked = false;
            defensiveDoctrineCheckBox.Checked = false;
            offensiveDoctrineCheckBox.Checked = false;
            winterSpecialistCheckBox.Checked = false;
            tricksterCheckBox.Checked = false;
            engineerCheckBox.Checked = false;
            fortressBusterCheckBox.Checked = false;
            panzerLeaderCheckBox.Checked = false;
            commandoCheckBox.Checked = false;
            oldGuardCheckBox.Checked = false;
            seaWolfCheckBox.Checked = false;
            blockadeRunnerCheckBox.Checked = false;
            superiorTacticianCheckBox.Checked = false;
            spotterCheckBox.Checked = false;
            tankBusterCheckBox.Checked = false;
            carpetBomberCheckBox.Checked = false;
            nightFlyerCheckBox.Checked = false;
            fleetDestroyerCheckBox.Checked = false;
            desertFoxCheckBox.Checked = false;
            jungleRatCheckBox.Checked = false;
            urbanWarfareSpecialistCheckBox.Checked = false;
            rangerCheckBox.Checked = false;
            mountaineerCheckBox.Checked = false;
            hillsFighterCheckBox.Checked = false;
            counterAttackerCheckBox.Checked = false;
            assaulterCheckBox.Checked = false;
            encirclerCheckBox.Checked = false;
            ambusherCheckBox.Checked = false;
            disciplinedCheckBox.Checked = false;
            elasticDefenceSpecialistCheckBox.Checked = false;
            blitzerCheckBox.Checked = false;
        }

        /// <summary>
        ///     Processing when the status of the military station management check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLogisticsWizardCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = logisticsWizardCheckBox.Checked ? LeaderTraits.LogisticsWizard : 0;
            if (((leader.Traits & LeaderTraits.LogisticsWizard) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.LogisticsWizard;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.LogisticsWizard);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            logisticsWizardCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the state of the defensive doctor check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDefensiveDoctrineCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = defensiveDoctrineCheckBox.Checked ? LeaderTraits.DefensiveDoctrine : 0;
            if (((leader.Traits & LeaderTraits.DefensiveDoctrine) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.DefensiveDoctrine;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.DefensiveDoctrine);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            defensiveDoctrineCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the state of the offensive doctor check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOffensiveDoctrineCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = offensiveDoctrineCheckBox.Checked ? LeaderTraits.OffensiveDoctrine : 0;
            if (((leader.Traits & LeaderTraits.OffensiveDoctrine) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.OffensiveDoctrine;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.OffensiveDoctrine);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            offensiveDoctrineCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the status of the winter battle check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWinterSpecialistCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = winterSpecialistCheckBox.Checked ? LeaderTraits.WinterSpecialist : 0;
            if (((leader.Traits & LeaderTraits.WinterSpecialist) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.WinterSpecialist;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.WinterSpecialist);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            winterSpecialistCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the ambush check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTricksterCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = tricksterCheckBox.Checked ? LeaderTraits.Trickster : 0;
            if (((leader.Traits & LeaderTraits.Trickster) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.Trickster;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Trickster);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            tricksterCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the engineer check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEngineerCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = engineerCheckBox.Checked ? LeaderTraits.Engineer : 0;
            if (((leader.Traits & LeaderTraits.Engineer) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.Engineer;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Engineer);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            engineerCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the fortress attack check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFortressBusterCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = fortressBusterCheckBox.Checked ? LeaderTraits.FortressBuster : 0;
            if (((leader.Traits & LeaderTraits.FortressBuster) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.FortressBuster;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.FortressBuster);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            fortressBusterCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the armored battle check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPanzerLeaderCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = panzerLeaderCheckBox.Checked ? LeaderTraits.PanzerLeader : 0;
            if (((leader.Traits & LeaderTraits.PanzerLeader) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.PanzerLeader;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.PanzerLeader);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            panzerLeaderCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the special battle check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandoCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = commandoCheckBox.Checked ? LeaderTraits.Commando : 0;
            if (((leader.Traits & LeaderTraits.Commando) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.Commando;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Commando);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            commandoCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the classical check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOldGuardCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = oldGuardCheckBox.Checked ? LeaderTraits.OldGuard : 0;
            if (((leader.Traits & LeaderTraits.OldGuard) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.OldGuard;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.OldGuard);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            oldGuardCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the sea wolf check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSeaWolfCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = seaWolfCheckBox.Checked ? LeaderTraits.SeaWolf : 0;
            if (((leader.Traits & LeaderTraits.SeaWolf) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.SeaWolf;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.SeaWolf);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            seaWolfCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the master check box for breaking through the blockade
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBlockadeRunnerCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = blockadeRunnerCheckBox.Checked ? LeaderTraits.BlockadeRunner : 0;
            if (((leader.Traits & LeaderTraits.BlockadeRunner) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.BlockadeRunner;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.BlockadeRunner);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            blockadeRunnerCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Outstanding tactician What to do when the check box changes state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSuperiorTacticianCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = superiorTacticianCheckBox.Checked ? LeaderTraits.SuperiorTactician : 0;
            if (((leader.Traits & LeaderTraits.SuperiorTactician) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.SuperiorTactician;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.SuperiorTactician);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            superiorTacticianCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the status of the search enemy check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpotterCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = spotterCheckBox.Checked ? LeaderTraits.Spotter : 0;
            if (((leader.Traits & LeaderTraits.Spotter) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.Spotter;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Spotter);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            spotterCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the anti-tank attack check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTankBusterCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = tankBusterCheckBox.Checked ? LeaderTraits.TankBuster : 0;
            if (((leader.Traits & LeaderTraits.TankBuster) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.TankBuster;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.TankBuster);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            tankBusterCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the carpet bombing check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCarpetBomberCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = carpetBomberCheckBox.Checked ? LeaderTraits.CarpetBomber : 0;
            if (((leader.Traits & LeaderTraits.CarpetBomber) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.CarpetBomber;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.CarpetBomber);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            carpetBomberCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the night aviation operation check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNightFlyerCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = nightFlyerCheckBox.Checked ? LeaderTraits.NightFlyer : 0;
            if (((leader.Traits & LeaderTraits.NightFlyer) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.NightFlyer;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.NightFlyer);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            nightFlyerCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the anti-ship attack check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFleetDestroyerCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = fleetDestroyerCheckBox.Checked ? LeaderTraits.FleetDestroyer : 0;
            if (((leader.Traits & LeaderTraits.FleetDestroyer) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.FleetDestroyer;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.FleetDestroyer);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            fleetDestroyerCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the fox checkbox in the desert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDesertFoxCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = desertFoxCheckBox.Checked ? LeaderTraits.DesertFox : 0;
            if (((leader.Traits & LeaderTraits.DesertFox) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.DesertFox;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.DesertFox);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            desertFoxCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the mouse check box in the jungle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJungleRatCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = jungleRatCheckBox.Checked ? LeaderTraits.JungleRat : 0;
            if (((leader.Traits & LeaderTraits.JungleRat) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.JungleRat;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.JungleRat);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            jungleRatCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the city battle check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUrbanWarfareSpecialistCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = urbanWarfareSpecialistCheckBox.Checked ? LeaderTraits.UrbanWarfareSpecialist : 0;
            if (((leader.Traits & LeaderTraits.UrbanWarfareSpecialist) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.UrbanWarfareSpecialist;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.UrbanWarfareSpecialist);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            urbanWarfareSpecialistCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the state of the ranger check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRangerCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = rangerCheckBox.Checked ? LeaderTraits.Ranger : 0;
            if (((leader.Traits & LeaderTraits.Ranger) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.Ranger;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Ranger);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            rangerCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the mountain battle check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMountaineerCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = mountaineerCheckBox.Checked ? LeaderTraits.Mountaineer : 0;
            if (((leader.Traits & LeaderTraits.Mountaineer) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.Mountaineer;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Mountaineer);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            mountaineerCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the highland battle check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHillsFighterCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = hillsFighterCheckBox.Checked ? LeaderTraits.HillsFighter : 0;
            if (((leader.Traits & LeaderTraits.HillsFighter) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.HillsFighter;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.HillsFighter);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            hillsFighterCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the state of the counterattack check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCounterAttackerCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = counterAttackerCheckBox.Checked ? LeaderTraits.CounterAttacker : 0;
            if (((leader.Traits & LeaderTraits.CounterAttacker) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.CounterAttacker;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.CounterAttacker);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            counterAttackerCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the assault battle check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAssaulterCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = assaulterCheckBox.Checked ? LeaderTraits.Assaulter : 0;
            if (((leader.Traits & LeaderTraits.Assaulter) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.Assaulter;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Assaulter);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            assaulterCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the siege battle check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEncirclerCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = encirclerCheckBox.Checked ? LeaderTraits.Encircler : 0;
            if (((leader.Traits & LeaderTraits.Encircler) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.Encircler;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Encircler);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            encirclerCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the surprise battle check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAmbusherCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = ambusherCheckBox.Checked ? LeaderTraits.Ambusher : 0;
            if (((leader.Traits & LeaderTraits.Ambusher) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.Ambusher;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Ambusher);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            ambusherCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the state of the discipline check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisiplinedCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = disciplinedCheckBox.Checked ? LeaderTraits.Disciplined : 0;
            if (((leader.Traits & LeaderTraits.Disciplined) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.Disciplined;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Disciplined);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            disciplinedCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the tactical retreat check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnElasticDefenceSpecialistCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = elasticDefenceSpecialistCheckBox.Checked ? LeaderTraits.ElasticDefenceSpecialist : 0;
            if (((leader.Traits & LeaderTraits.ElasticDefenceSpecialist) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.ElasticDefenceSpecialist;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.ElasticDefenceSpecialist);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            elasticDefenceSpecialistCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the state of the electric shock battle check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBlitzerCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Leader leader = GetSelectedLeader();
            if (leader == null)
            {
                return;
            }

            // Do nothing if the value does not change
            uint trait = blitzerCheckBox.Checked ? LeaderTraits.Blitzer : 0;
            if (((leader.Traits & LeaderTraits.Blitzer) ^ trait) == 0)
            {
                return;
            }

            uint old = leader.Traits;

            // Update value
            leader.Traits &= ~LeaderTraits.Blitzer;
            leader.Traits |= trait;

            Log.Info("[Leader] traits: {0} -> {1} ({2}: {3})", old, leader.Traits, leader.Id, leader.Name);

            // Update items in the commander list view
            leaderListView.SelectedItems[0].SubItems[8].Text = GetLeaderTraitsText(leader.Traits);

            // Set the edited flag
            leader.SetDirty(LeaderItemId.Blitzer);
            Leaders.SetDirty(leader.Country);

            // Change the item color
            blitzerCheckBox.ForeColor = Color.Red;
        }

        #endregion
    }
}
