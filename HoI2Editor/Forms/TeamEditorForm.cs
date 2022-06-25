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
    ///     Research institution editor form
    /// </summary>
    public partial class TeamEditorForm : Form
    {
        #region Internal field

        /// <summary>
        ///     List of research institutes after narrowing down
        /// </summary>
        private readonly List<Team> _list = new List<Team>();

        /// <summary>
        ///     Arrangement of research characteristics combo boxes
        /// </summary>
        private readonly ComboBox[] _specialityComboBoxes;

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
            Skill,
            StartYear,
            EndYear,
            Speciality
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
        ///     Number of columns in the research institution list view
        /// </summary>
        public const int TeamListColumnCount = 7;

        #endregion

        #region Internal constant

        /// <summary>
        ///     Number of editable characteristics
        /// </summary>
        private const int MaxEditableSpecialities = 7;

        /// <summary>
        ///     Items of research characteristics ID
        /// </summary>
        private static readonly TeamItemId[] SpecialityItemIds =
        {
            TeamItemId.Speciality1,
            TeamItemId.Speciality2,
            TeamItemId.Speciality3,
            TeamItemId.Speciality4,
            TeamItemId.Speciality5,
            TeamItemId.Speciality6,
            TeamItemId.Speciality7
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public TeamEditorForm()
        {
            InitializeComponent();

            // Initialize the array of research property combo boxes
            _specialityComboBoxes = new[]
            {
                specialityComboBox1,
                specialityComboBox2,
                specialityComboBox3,
                specialityComboBox4,
                specialityComboBox5,
                specialityComboBox6,
                specialityComboBox7
            };

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
            // Narrow down the list of research institutes
            NarrowTeamList();

            // Sort the list of research institutes
            SortTeamList();

            // Update the display of the research institute list
            UpdateTeamList();

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
            // Research institution list view
            countryColumnHeader.Width = HoI2EditorController.Settings.TeamEditor.ListColumnWidth[0];
            idColumnHeader.Width = HoI2EditorController.Settings.TeamEditor.ListColumnWidth[1];
            nameColumnHeader.Width = HoI2EditorController.Settings.TeamEditor.ListColumnWidth[2];
            skillColumnHeader.Width = HoI2EditorController.Settings.TeamEditor.ListColumnWidth[3];
            startYearColumnHeader.Width = HoI2EditorController.Settings.TeamEditor.ListColumnWidth[4];
            endYearColumnHeader.Width = HoI2EditorController.Settings.TeamEditor.ListColumnWidth[5];
            specialityColumnHeader.Width = HoI2EditorController.Settings.TeamEditor.ListColumnWidth[6];

            // National list box
            countryListBox.ColumnWidth = DeviceCaps.GetScaledWidth(countryListBox.ColumnWidth);
            countryListBox.ItemHeight = DeviceCaps.GetScaledHeight(countryListBox.ItemHeight);

            // Characteristic combo box
            specialityComboBox1.ItemHeight = DeviceCaps.GetScaledHeight(specialityComboBox1.ItemHeight);
            specialityComboBox2.ItemHeight = DeviceCaps.GetScaledHeight(specialityComboBox2.ItemHeight);
            specialityComboBox3.ItemHeight = DeviceCaps.GetScaledHeight(specialityComboBox3.ItemHeight);
            specialityComboBox4.ItemHeight = DeviceCaps.GetScaledHeight(specialityComboBox4.ItemHeight);
            specialityComboBox5.ItemHeight = DeviceCaps.GetScaledHeight(specialityComboBox5.ItemHeight);
            specialityComboBox6.ItemHeight = DeviceCaps.GetScaledHeight(specialityComboBox6.ItemHeight);
            specialityComboBox7.ItemHeight = DeviceCaps.GetScaledHeight(specialityComboBox7.ItemHeight);

            // Window position
            Location = HoI2EditorController.Settings.TeamEditor.Location;
            Size = HoI2EditorController.Settings.TeamEditor.Size;
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

            // Initialize research characteristics
            Techs.InitSpecialities();

            // Load the game settings file
            Misc.Load();

            // Read the character string definition file
            Config.Load();

            // Create a dummy image list to set the height of the research institution list view
            teamListView.SmallImageList = new ImageList { ImageSize = new Size(1, DeviceCaps.GetScaledHeight(18)) };

            // Initialize edit items
            InitEditableItems();

            // Initialize the national list box
            InitCountryListBox();

            // Read research institution files
            Teams.Load();

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
            HoI2EditorController.OnTeamEditorFormClosed();
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
                HoI2EditorController.Settings.TeamEditor.Location = Location;
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
                HoI2EditorController.Settings.TeamEditor.Size = Size;
            }
        }

        /// <summary>
        ///     Processing when the batch edit button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBatchButtonClick(object sender, EventArgs e)
        {
            TeamBatchEditArgs args = new TeamBatchEditArgs();
            args.TargetCountries.AddRange(from string name in countryListBox.SelectedItems
                select Countries.StringMap[name]);

            // Display the batch edit dialog
            TeamBatchDialog dialog = new TeamBatchDialog(args);
            if (dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            // Bulk editing process
            Teams.BatchEdit(args);

            // Update the research institution list
            NarrowTeamList();
            UpdateTeamList();

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

        #region Research institution list view

        /// <summary>
        ///     Update the display of the research institute list
        /// </summary>
        private void UpdateTeamList()
        {
            teamListView.BeginUpdate();
            teamListView.Items.Clear();

            // Register items in order
            foreach (Team team in _list)
            {
                teamListView.Items.Add(CreateTeamListViewItem(team));
            }

            if (teamListView.Items.Count > 0)
            {
                // Select the first item
                teamListView.Items[0].Focused = true;
                teamListView.Items[0].Selected = true;

                // Enable edit items
                EnableEditableItems();
            }
            else
            {
                // Disable edit items
                DisableEditableItems();
            }

            teamListView.EndUpdate();
        }

        /// <summary>
        ///     Narrow down the list of research institutions by country tag
        /// </summary>
        private void NarrowTeamList()
        {
            _list.Clear();

            // Create a list of selected nations
            List<Country> tags = (from string s in countryListBox.SelectedItems select Countries.StringMap[s]).ToList();

            // Narrow down the research institutions belonging to the selected country in order
            _list.AddRange(Teams.Items.Where(team => tags.Contains(team.Country)));
        }

        /// <summary>
        ///     Sort the list of research institutes
        /// </summary>
        private void SortTeamList()
        {
            switch (_key)
            {
                case SortKey.None: // No sort
                    break;

                case SortKey.Tag: // Country tag
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((team1, team2) => team1.Country - team2.Country);
                    }
                    else
                    {
                        _list.Sort((team1, team2) => team2.Country - team1.Country);
                    }
                    break;

                case SortKey.Id: // ID
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((team1, team2) => team1.Id - team2.Id);
                    }
                    else
                    {
                        _list.Sort((team1, team2) => team2.Id - team1.Id);
                    }
                    break;

                case SortKey.Name: // name
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((team1, team2) => string.CompareOrdinal(team1.Name, team2.Name));
                    }
                    else
                    {
                        _list.Sort((team1, team2) => string.CompareOrdinal(team2.Name, team1.Name));
                    }
                    break;

                case SortKey.Skill: // skill
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((team1, team2) => team1.Skill - team2.Skill);
                    }
                    else
                    {
                        _list.Sort((team1, team2) => team2.Skill - team1.Skill);
                    }
                    break;

                case SortKey.StartYear: // Start year
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((team1, team2) => team1.StartYear - team2.StartYear);
                    }
                    else
                    {
                        _list.Sort((team1, team2) => team2.StartYear - team1.StartYear);
                    }
                    break;

                case SortKey.EndYear: // End year
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((team1, team2) => team1.EndYear - team2.EndYear);
                    }
                    else
                    {
                        _list.Sort((team1, team2) => team2.EndYear - team1.EndYear);
                    }
                    break;

                case SortKey.Speciality: // Characteristic
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((team1, team2) =>
                        {
                            if (team1.Specialities[0] > team2.Specialities[0])
                            {
                                return 1;
                            }
                            if (team1.Specialities[0] < team2.Specialities[0])
                            {
                                return -1;
                            }
                            if (team1.Specialities[1] > team2.Specialities[1])
                            {
                                return 1;
                            }
                            if (team1.Specialities[1] < team2.Specialities[1])
                            {
                                return -1;
                            }
                            if (team1.Specialities[2] > team2.Specialities[2])
                            {
                                return 1;
                            }
                            if (team1.Specialities[2] < team2.Specialities[2])
                            {
                                return -1;
                            }
                            if (team1.Specialities[3] > team2.Specialities[3])
                            {
                                return 1;
                            }
                            if (team1.Specialities[3] < team2.Specialities[3])
                            {
                                return -1;
                            }
                            if (team1.Specialities[4] > team2.Specialities[4])
                            {
                                return 1;
                            }
                            if (team1.Specialities[4] < team2.Specialities[4])
                            {
                                return -1;
                            }
                            if (team1.Specialities[5] > team2.Specialities[5])
                            {
                                return 1;
                            }
                            if (team1.Specialities[5] < team2.Specialities[5])
                            {
                                return -1;
                            }
                            if (team1.Specialities[6] > team2.Specialities[6])
                            {
                                return 1;
                            }
                            if (team1.Specialities[6] < team2.Specialities[6])
                            {
                                return -1;
                            }
                            return 0;
                        });
                    }
                    else
                    {
                        _list.Sort((team1, team2) =>
                        {
                            if (team1.Specialities[0] < team2.Specialities[0])
                            {
                                return 1;
                            }
                            if (team1.Specialities[0] > team2.Specialities[0])
                            {
                                return -1;
                            }
                            if (team1.Specialities[1] < team2.Specialities[1])
                            {
                                return 1;
                            }
                            if (team1.Specialities[1] > team2.Specialities[1])
                            {
                                return -1;
                            }
                            if (team1.Specialities[2] < team2.Specialities[2])
                            {
                                return 1;
                            }
                            if (team1.Specialities[2] > team2.Specialities[2])
                            {
                                return -1;
                            }
                            if (team1.Specialities[3] < team2.Specialities[3])
                            {
                                return 1;
                            }
                            if (team1.Specialities[3] > team2.Specialities[3])
                            {
                                return -1;
                            }
                            if (team1.Specialities[4] < team2.Specialities[4])
                            {
                                return 1;
                            }
                            if (team1.Specialities[4] > team2.Specialities[4])
                            {
                                return -1;
                            }
                            if (team1.Specialities[5] < team2.Specialities[5])
                            {
                                return 1;
                            }
                            if (team1.Specialities[5] > team2.Specialities[5])
                            {
                                return -1;
                            }
                            if (team1.Specialities[6] < team2.Specialities[6])
                            {
                                return 1;
                            }
                            if (team1.Specialities[6] > team2.Specialities[6])
                            {
                                return -1;
                            }
                            return 0;
                        });
                    }
                    break;
            }
        }

        /// <summary>
        ///     Research institution list view sub-item drawing process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTeamListViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 6: // Research characteristics
                    e.Graphics.FillRectangle(
                        teamListView.SelectedIndices.Count > 0 && e.ItemIndex == teamListView.SelectedIndices[0]
                            ? (teamListView.Focused ? SystemBrushes.Highlight : SystemBrushes.Control)
                            : SystemBrushes.Window, e.Bounds);
                    DrawTechSpecialityIcon(e, teamListView.Items[e.ItemIndex].Tag as Team);
                    break;

                default:
                    e.DrawDefault = true;
                    break;
            }
        }

        /// <summary>
        ///     Research characteristic icon drawing process of research institution list view
        /// </summary>
        /// <param name="e"></param>
        /// <param name="team">Research institution data</param>
        private static void DrawTechSpecialityIcon(DrawListViewSubItemEventArgs e, Team team)
        {
            if (team == null)
            {
                return;
            }

            Rectangle rect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 1, DeviceCaps.GetScaledWidth(16),
                DeviceCaps.GetScaledHeight(16));
            for (int i = 0; i < Team.SpecialityLength; i++)
            {
                // Do nothing without research characteristics
                if (team.Specialities[i] == TechSpeciality.None)
                {
                    continue;
                }

                // Draw a research characteristic icon
                if ((int) team.Specialities[i] - 1 < Techs.SpecialityImages.Images.Count)
                {
                    e.Graphics.DrawImage(
                        Techs.SpecialityImages.Images[Array.IndexOf(Techs.Specialities, team.Specialities[i]) - 1], rect);
                }
                rect.X += DeviceCaps.GetScaledWidth(16) + 3;
            }
        }

        /// <summary>
        ///     Column header drawing process of research institution list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTeamListViewDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        /// <summary>
        ///     Processing when changing the selection item in the research institution list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTeamListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update edit items
            UpdateEditableItems();
        }

        /// <summary>
        ///     Processing before editing items in the research institution list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTeamListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
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

                case 3: // skill
                    e.Type = ItemEditType.Text;
                    e.Text = skillNumericUpDown.Text;
                    break;

                case 4: // Start year
                    e.Type = ItemEditType.Text;
                    e.Text = startYearNumericUpDown.Text;
                    break;

                case 5: // End year
                    e.Type = ItemEditType.Text;
                    e.Text = endYearNumericUpDown.Text;
                    break;
            }
        }

        /// <summary>
        ///     Processing after editing items in the research institute list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTeamListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
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

                case 3: // skill
                    skillNumericUpDown.Text = e.Text;
                    break;

                case 4: // Start year
                    startYearNumericUpDown.Text = e.Text;
                    break;

                case 5: // End year
                    endYearNumericUpDown.Text = e.Text;
                    break;
            }

            // Since the items in the list view will be updated by yourself, it will be treated as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Processing when replacing items in the research institution list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTeamListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // I will replace the items on my own, so I will treat it as canceled
            e.Cancel = true;

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;
            if (srcIndex < destIndex)
            {
                destIndex--;
            }

            Team src = teamListView.Items[srcIndex].Tag as Team;
            if (src == null)
            {
                return;
            }
            Team dest = teamListView.Items[destIndex].Tag as Team;
            if (dest == null)
            {
                return;
            }

            // Move items in the research institute list
            Teams.MoveItem(src, dest);
            MoveListItem(srcIndex, destIndex);

            // Set the edited flag
            Teams.SetDirty(src.Country);
        }

        /// <summary>
        ///     Processing when a column is clicked in the research institution list view
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

                case 3: // skill
                    if (_key == SortKey.Skill)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Skill;
                    }
                    break;

                case 4: // Start year
                    if (_key == SortKey.StartYear)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.StartYear;
                    }
                    break;

                case 5: // End year
                    if (_key == SortKey.EndYear)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.EndYear;
                    }
                    break;

                case 6: // Characteristic
                    if (_key == SortKey.Speciality)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Speciality;
                    }
                    break;

                default:
                    // Do nothing when clicking on a column with no items
                    return;
            }

            // Sort the list of research institutes
            SortTeamList();

            // Update the research institution list
            UpdateTeamList();
        }

        /// <summary>
        ///     Processing when changing the width of columns in the research institution list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTeamListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < TeamListColumnCount))
            {
                HoI2EditorController.Settings.TeamEditor.ListColumnWidth[e.ColumnIndex] =
                    teamListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Processing when a new button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewButtonClick(object sender, EventArgs e)
        {
            Team team;
            Team selected = GetSelectedTeam();
            if (selected != null)
            {
                // If there is a choice, the country tag or ID To take over and create an item
                team = new Team(selected)
                {
                    Id = Teams.GetNewId(selected.Country),
                    Name = "",
                    PictureName = ""
                };

                // Set edited flags for each research institution
                team.SetDirtyAll();

                // Insert an item in the research institution list
                Teams.InsertItem(team, selected);
                InsertListItem(team, teamListView.SelectedIndices[0] + 1);
            }
            else
            {
                Country country = Countries.Tags[countryListBox.SelectedIndex];
                // Create a new item
                team = new Team
                {
                    Country = country,
                    Id = Teams.GetNewId(country),
                    Skill = 1,
                    StartYear = 1930,
                    EndYear = 1970
                };

                // Set edited flags for each research institution
                team.SetDirtyAll();

                // Add an item to the research institution list
                Teams.AddItem(team);
                AddListItem(team);

                // Enable edit items
                EnableEditableItems();
            }

            // Set edited flags for each country
            Teams.SetDirty(team.Country);

            // Notify the update of the research institute list
            HoI2EditorController.OnItemChanged(EditorItemId.TeamList, this);

            // If it does not exist in the file list, add it
            if (!Teams.FileNameMap.ContainsKey(team.Country))
            {
                Teams.FileNameMap.Add(team.Country, Game.GetTeamFileName(team.Country));
                Teams.SetDirtyList();
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
            Team selected = GetSelectedTeam();
            if (selected == null)
            {
                return;
            }

            // Create an item by taking over the selected item
            Team team = new Team(selected)
            {
                Id = Teams.GetNewId(selected.Country)
            };

            // Set edited flags for each research institution
            team.SetDirtyAll();

            // Insert an item in the research institution list
            Teams.InsertItem(team, selected);
            InsertListItem(team, teamListView.SelectedIndices[0] + 1);

            // Set edited flags for each country
            Teams.SetDirty(team.Country);

            // Notify the update of the research institute list
            HoI2EditorController.OnItemChanged(EditorItemId.TeamList, this);
        }

        /// <summary>
        ///     Processing when the delete button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team selected = GetSelectedTeam();
            if (selected == null)
            {
                return;
            }

            // Remove an item from the research institute list
            Teams.RemoveItem(selected);
            RemoveItem(teamListView.SelectedIndices[0]);

            // Disable edit items when there are no items in the list
            if (teamListView.Items.Count == 0)
            {
                DisableEditableItems();
            }

            // Set the edited flag
            Teams.SetDirty(selected.Country);

            // Notify the update of the research institute list
            HoI2EditorController.OnItemChanged(EditorItemId.TeamList, this);
        }

        /// <summary>
        ///     Processing when the button is pressed to the beginning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTopButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team selected = GetSelectedTeam();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the top of the list
            int index = teamListView.SelectedIndices[0];
            if (index == 0)
            {
                return;
            }

            Team top = teamListView.Items[0].Tag as Team;
            if (top == null)
            {
                return;
            }

            // Move items in the research institute list
            Teams.MoveItem(selected, top);
            MoveListItem(index, 0);

            // Set the edited flag
            Teams.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Processing when pressing the up button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team selected = GetSelectedTeam();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the top of the list
            int index = teamListView.SelectedIndices[0];
            if (index == 0)
            {
                return;
            }

            Team upper = teamListView.Items[index - 1].Tag as Team;
            if (upper == null)
            {
                return;
            }

            // Move items in the research institute list
            Teams.MoveItem(selected, upper);
            MoveListItem(index, index - 1);

            // Set the edited flag
            Teams.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Processing when the down button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team selected = GetSelectedTeam();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the end of the list
            int index = teamListView.SelectedIndices[0];
            if (index == teamListView.Items.Count - 1)
            {
                return;
            }

            Team lower = teamListView.Items[index + 1].Tag as Team;
            if (lower == null)
            {
                return;
            }

            // Move items in the research institute list
            Teams.MoveItem(selected, lower);
            MoveListItem(index, index + 1);

            // Set the edited flag
            Teams.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Processing when the button is pressed to the end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBottomButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team selected = GetSelectedTeam();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the end of the list
            int index = teamListView.SelectedIndices[0];
            if (teamListView.SelectedIndices[0] == teamListView.Items.Count - 1)
            {
                return;
            }

            Team bottom = teamListView.Items[teamListView.Items.Count - 1].Tag as Team;
            if (bottom == null)
            {
                return;
            }

            // Move items in the research institute list
            Teams.MoveItem(selected, bottom);
            MoveListItem(index, teamListView.Items.Count - 1);

            // Set the edited flag
            Teams.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Add an item to the research institution list
        /// </summary>
        /// <param name="team">Items to be added</param>
        private void AddListItem(Team team)
        {
            // Add an item to the refined list
            _list.Add(team);

            // Add an item to the research institute list view
            teamListView.Items.Add(CreateTeamListViewItem(team));

            // Select the added item
            teamListView.Items[teamListView.Items.Count - 1].Focused = true;
            teamListView.Items[teamListView.Items.Count - 1].Selected = true;
            teamListView.EnsureVisible(teamListView.Items.Count - 1);
        }

        /// <summary>
        ///     Insert an item in the research institution list
        /// </summary>
        /// <param name="team">Items to be inserted</param>
        /// <param name="index">Insertion destination position</param>
        private void InsertListItem(Team team, int index)
        {
            // Insert an item in the refined list
            _list.Insert(index, team);

            // Insert an item in the laboratory list view
            teamListView.Items.Insert(index, CreateTeamListViewItem(team));

            // Select the inserted item
            teamListView.Items[index].Focused = true;
            teamListView.Items[index].Selected = true;
            teamListView.EnsureVisible(index);
        }

        /// <summary>
        ///     Remove an item from the research institute list
        /// </summary>
        /// <param name="index">Position to be deleted</param>
        private void RemoveItem(int index)
        {
            // Remove an item from the refined list
            _list.RemoveAt(index);

            // Remove an item from the laboratory list view
            teamListView.Items.RemoveAt(index);

            // Select the next item after the deleted item
            if (index < teamListView.Items.Count)
            {
                teamListView.Items[index].Focused = true;
                teamListView.Items[index].Selected = true;
            }
            else if (index - 1 >= 0)
            {
                // At the end of the list, select the item before the deleted item
                teamListView.Items[index - 1].Focused = true;
                teamListView.Items[index - 1].Selected = true;
            }
        }

        /// <summary>
        ///     Move items in the research institute list
        /// </summary>
        /// <param name="src">Source position</param>
        /// <param name="dest">Destination position</param>
        private void MoveListItem(int src, int dest)
        {
            Team team = _list[src];

            if (src > dest)
            {
                // When moving up
                // Move items in the refined list
                _list.Insert(dest, team);
                _list.RemoveAt(src + 1);

                // Move items in the research institute list view
                teamListView.Items.Insert(dest, CreateTeamListViewItem(team));
                teamListView.Items.RemoveAt(src + 1);
            }
            else
            {
                // When moving down
                // Move items in the refined list
                _list.Insert(dest + 1, team);
                _list.RemoveAt(src);

                // Move items in the research institute list view
                teamListView.Items.Insert(dest + 1, CreateTeamListViewItem(team));
                teamListView.Items.RemoveAt(src);
            }

            // Select the item to move to
            teamListView.Items[dest].Focused = true;
            teamListView.Items[dest].Selected = true;
            teamListView.EnsureVisible(dest);
        }

        /// <summary>
        ///     Create an item in the research institution list view
        /// </summary>
        /// <param name="team">Research institution data</param>
        /// <returns>Items in the research institution list view</returns>
        private static ListViewItem CreateTeamListViewItem(Team team)
        {
            if (team == null)
            {
                return null;
            }

            ListViewItem item = new ListViewItem
            {
                Text = Countries.Strings[(int) team.Country],
                Tag = team
            };
            item.SubItems.Add(IntHelper.ToString(team.Id));
            item.SubItems.Add(team.Name);
            item.SubItems.Add(IntHelper.ToString(team.Skill));
            item.SubItems.Add(IntHelper.ToString(team.StartYear));
            item.SubItems.Add(IntHelper.ToString(team.EndYear));
            item.SubItems.Add("");

            return item;
        }

        /// <summary>
        ///     Get the data of the selected research institution
        /// </summary>
        /// <returns>Selected research institute data</returns>
        private Team GetSelectedTeam()
        {
            // If there is no selection
            if (teamListView.SelectedItems.Count == 0)
            {
                return null;
            }

            return teamListView.SelectedItems[0].Tag as Team;
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
            foreach (Country country in HoI2EditorController.Settings.TeamEditor.Countries)
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
                brush = Teams.IsDirty(country)
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(countryListBox.ForeColor);
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
            HoI2EditorController.Settings.TeamEditor.Countries =
                countryListBox.SelectedIndices.Cast<int>().Select(index => Countries.Tags[index]).ToList();

            // Update the research institution list
            NarrowTeamList();
            UpdateTeamList();
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

            // Issue a dummy event to narrow down the list of research institutes
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

            // Research characteristics
            for (int i = 0; i < MaxEditableSpecialities; i++)
            {
                _specialityComboBoxes[i].Tag = i;
                _specialityComboBoxes[i].Items.Clear();
            }
            width = specialityComboBox1.Width;
            int additional = DeviceCaps.GetScaledWidth(16) + 3 + SystemInformation.VerticalScrollBarWidth;
            foreach (string s in Techs.Specialities.Select(Techs.GetSpecialityName))
            {
                for (int i = 0; i < MaxEditableSpecialities; i++)
                {
                    _specialityComboBoxes[i].Items.Add(s);
                }
                // Adding the width of the research characteristic icon
                width = Math.Max(width,
                    (int) g.MeasureString(s, specialityComboBox1.Font).Width + additional);
            }
            for (int i = 0; i < MaxEditableSpecialities; i++)
            {
                _specialityComboBoxes[i].DropDownWidth = width;
            }
        }

        /// <summary>
        ///     Update edit items
        /// </summary>
        private void UpdateEditableItems()
        {
            // Do nothing if there is no selection
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            // Update edit items
            UpdateEditableItemsValue(team);

            // Update the color of the edit item
            UpdateEditableItemsColor(team);

            // Item move button status update
            topButton.Enabled = teamListView.SelectedIndices[0] != 0;
            upButton.Enabled = teamListView.SelectedIndices[0] != 0;
            downButton.Enabled = teamListView.SelectedIndices[0] != teamListView.Items.Count - 1;
            bottomButton.Enabled = teamListView.SelectedIndices[0] != teamListView.Items.Count - 1;
        }

        /// <summary>
        ///     Update the value of the edit item
        /// </summary>
        /// <param name="team">Research institution data</param>
        private void UpdateEditableItemsValue(Team team)
        {
            countryComboBox.SelectedIndex = team.Country != Country.None ? (int) team.Country - 1 : -1;
            idNumericUpDown.Value = team.Id;
            nameTextBox.Text = team.Name;
            skillNumericUpDown.Value = team.Skill;
            startYearNumericUpDown.Value = team.StartYear;
            endYearNumericUpDown.Value = team.EndYear;
            for (int i = 0; i < MaxEditableSpecialities; i++)
            {
                _specialityComboBoxes[i].SelectedIndex = Array.IndexOf(Techs.Specialities, team.Specialities[i]);
            }
            pictureNameTextBox.Text = team.PictureName;
            UpdateTeamPicture(team);
        }

        /// <summary>
        ///     Update the color of the edit item
        /// </summary>
        /// <param name="team">Research institution data</param>
        private void UpdateEditableItemsColor(Team team)
        {
            // Update the color of the combo box
            countryComboBox.Refresh();
            for (int i = 0; i < MaxEditableSpecialities; i++)
            {
                _specialityComboBoxes[i].Refresh();
            }

            // Update the color of the edit item
            idNumericUpDown.ForeColor = team.IsDirty(TeamItemId.Id) ? Color.Red : SystemColors.WindowText;
            nameTextBox.ForeColor = team.IsDirty(TeamItemId.Name) ? Color.Red : SystemColors.WindowText;
            skillNumericUpDown.ForeColor = team.IsDirty(TeamItemId.Skill) ? Color.Red : SystemColors.WindowText;
            startYearNumericUpDown.ForeColor = team.IsDirty(TeamItemId.StartYear) ? Color.Red : SystemColors.WindowText;
            endYearNumericUpDown.ForeColor = team.IsDirty(TeamItemId.EndYear) ? Color.Red : SystemColors.WindowText;
            pictureNameTextBox.ForeColor = team.IsDirty(TeamItemId.PictureName) ? Color.Red : SystemColors.WindowText;
        }

        /// <summary>
        ///     Enable edit items
        /// </summary>
        private void EnableEditableItems()
        {
            countryComboBox.Enabled = true;
            idNumericUpDown.Enabled = true;
            nameTextBox.Enabled = true;
            skillNumericUpDown.Enabled = true;
            startYearNumericUpDown.Enabled = true;
            endYearNumericUpDown.Enabled = true;
            pictureNameTextBox.Enabled = true;
            pictureNameBrowseButton.Enabled = true;
            for (int i = 0; i < MaxEditableSpecialities; i++)
            {
                _specialityComboBoxes[i].Enabled = true;
            }

            // Reset the character string cleared at the time of invalidation
            idNumericUpDown.Text = IntHelper.ToString((int) idNumericUpDown.Value);
            skillNumericUpDown.Text = IntHelper.ToString((int) skillNumericUpDown.Value);
            startYearNumericUpDown.Text = IntHelper.ToString((int) startYearNumericUpDown.Value);
            endYearNumericUpDown.Text = IntHelper.ToString((int) endYearNumericUpDown.Value);

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
            skillNumericUpDown.ResetText();
            startYearNumericUpDown.ResetText();
            endYearNumericUpDown.ResetText();
            pictureNameTextBox.ResetText();
            teamPictureBox.ImageLocation = "";

            countryComboBox.Enabled = false;
            idNumericUpDown.Enabled = false;
            nameTextBox.Enabled = false;
            skillNumericUpDown.Enabled = false;
            startYearNumericUpDown.Enabled = false;
            endYearNumericUpDown.Enabled = false;
            pictureNameTextBox.Enabled = false;
            pictureNameBrowseButton.Enabled = false;
            for (int i = 0; i < MaxEditableSpecialities; i++)
            {
                _specialityComboBoxes[i].Enabled = false;
            }

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
            Team team = GetSelectedTeam();
            if (team != null)
            {
                Brush brush;
                if ((Countries.Tags[e.Index] == team.Country) && team.IsDirty(TeamItemId.Country))
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
        ///     Item drawing process of research characteristic combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpecialityComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null)
            {
                return;
            }
            int no = (int) comboBox.Tag;

            // Draw the background
            e.DrawBackground();

            Team team = GetSelectedTeam();
            if (team != null)
            {
                // Draw an icon
                if (e.Index > 0 && e.Index - 1 < Techs.SpecialityImages.Images.Count)
                {
                    Rectangle gr = new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, DeviceCaps.GetScaledWidth(16),
                        DeviceCaps.GetScaledHeight(16));
                    e.Graphics.DrawImage(Techs.SpecialityImages.Images[e.Index - 1], gr);
                }

                // Draw a string of items
                Brush brush;
                if ((Techs.Specialities[e.Index] == team.Specialities[no]) && team.IsDirty(SpecialityItemIds[no]))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = comboBox.Items[e.Index].ToString();
                Rectangle tr = new Rectangle(e.Bounds.X + DeviceCaps.GetScaledWidth(16) + 3, e.Bounds.Y + 3,
                    e.Bounds.Width - DeviceCaps.GetScaledWidth(16) - 3, e.Bounds.Height);
                e.Graphics.DrawString(s, e.Font, brush, tr);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Update the items in the research institution image picture box
        /// </summary>
        /// <param name="team">Research institution data</param>
        private void UpdateTeamPicture(Team team)
        {
            if (!string.IsNullOrEmpty(team.PictureName) &&
                (team.PictureName.IndexOfAny(Path.GetInvalidPathChars()) < 0))
            {
                string fileName = Game.GetReadFileName(Game.PersonPicturePathName,
                    Path.ChangeExtension(team.PictureName, ".bmp"));
                teamPictureBox.ImageLocation = File.Exists(fileName) ? fileName : "";
            }
            else
            {
                teamPictureBox.ImageLocation = "";
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
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            // Do nothing if the value does not change
            Country country = Countries.Tags[countryComboBox.SelectedIndex];
            if (country == team.Country)
            {
                return;
            }

            // Set the edited flag for the country tag before the change
            Teams.SetDirty(team.Country);

            Log.Info("[Team] country: {0} -> {1} ({2}: {3})", Countries.Strings[(int) team.Country],
                Countries.Strings[(int) country], team.Id, team.Name);

            // Update value
            team.Country = country;

            // Update the items in the research institution list view
            teamListView.SelectedItems[0].Text = Countries.Strings[(int) team.Country];

            // Set edited flags for each research institution
            team.SetDirty(TeamItemId.Country);

            // Set the edited flag of the changed country tag
            Teams.SetDirty(team.Country);

            // If it does not exist in the file list, add it
            if (!Teams.FileNameMap.ContainsKey(team.Country))
            {
                Teams.FileNameMap.Add(team.Country, Game.GetTeamFileName(team.Country));
                Teams.SetDirtyList();
            }

            // Update drawing to change the item color of the national combo box
            countryComboBox.Refresh();

            // Update drawing to change the item color of the national list box
            countryListBox.Refresh();

            // Notify the renewal of the nation to which the research institution belongs
            HoI2EditorController.OnItemChanged(EditorItemId.TeamCountry, this);
        }

        /// <summary>
        ///     ID Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIdNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int id = (int) idNumericUpDown.Value;
            if (id == team.Id)
            {
                return;
            }

            Log.Info("[Team] id: {0} -> {1} ({2})", team.Id, id, team.Name);

            // Update value
            team.Id = id;

            // Update the items in the research institution list view
            teamListView.SelectedItems[0].SubItems[1].Text = IntHelper.ToString(team.Id);

            // Set the edited flag
            team.SetDirty(TeamItemId.Id);
            Teams.SetDirty(team.Country);

            // Change the font color
            idNumericUpDown.ForeColor = Color.Red;

            // research Institute ID Notify for updates
            HoI2EditorController.OnItemChanged(EditorItemId.TeamId, this);
        }

        /// <summary>
        ///     Processing when changing the name string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            // Do nothing if the value does not change
            string name = nameTextBox.Text;
            if (string.IsNullOrEmpty(name))
            {
                if (string.IsNullOrEmpty(team.Name))
                {
                    return;
                }
            }
            else
            {
                if (name.Equals(team.Name))
                {
                    return;
                }
            }

            Log.Info("[Team] name: {0} -> {1} ({2})", team.Name, name, team.Id);

            // Update value
            team.Name = name;

            // Update the items in the research institution list view
            teamListView.SelectedItems[0].SubItems[2].Text = team.Name;

            // Set the edited flag
            team.SetDirty(TeamItemId.Name);
            Teams.SetDirty(team.Country);

            // Change the font color
            nameTextBox.ForeColor = Color.Red;

            // Notify the update of the research institute name
            HoI2EditorController.OnItemChanged(EditorItemId.TeamName, this);
        }

        /// <summary>
        ///     Processing when changing skills
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSkillNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int skill = (int) skillNumericUpDown.Value;
            if (skill == team.Skill)
            {
                return;
            }

            Log.Info("[Team] skill: {0} -> {1} ({2}: {3})", team.Skill, skill, team.Id, team.Name);

            // Update value
            team.Skill = skill;

            // Update the items in the research institution list view
            teamListView.SelectedItems[0].SubItems[3].Text = IntHelper.ToString(team.Skill);

            // Set the edited flag
            team.SetDirty(TeamItemId.Skill);
            Teams.SetDirty(team.Country);

            // Change the font color
            skillNumericUpDown.ForeColor = Color.Red;

            // Notify research institution skill updates
            HoI2EditorController.OnItemChanged(EditorItemId.TeamSkill, this);
        }

        /// <summary>
        ///     Processing when changing the start year
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartYearNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int startYear = (int) startYearNumericUpDown.Value;
            if (startYear == team.StartYear)
            {
                return;
            }

            Log.Info("[Team] start year: {0} -> {1} ({2}: {3})", team.StartYear, startYear, team.Id, team.Name);

            // Update value
            team.StartYear = startYear;

            // Update the items in the research institution list view
            teamListView.SelectedItems[0].SubItems[4].Text = IntHelper.ToString(team.StartYear);

            // Set the edited flag
            team.SetDirty(TeamItemId.StartYear);
            Teams.SetDirty(team.Country);

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
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int endYear = (int) endYearNumericUpDown.Value;
            if (endYear == team.EndYear)
            {
                return;
            }

            Log.Info("[Team] end year: {0} -> {1} ({2}: {3})", team.EndYear, endYear, team.Id, team.Name);

            // Update value
            team.EndYear = endYear;

            // Update the items in the research institution list view
            teamListView.SelectedItems[0].SubItems[5].Text = IntHelper.ToString(team.EndYear);

            // Set the edited flag
            team.SetDirty(TeamItemId.EndYear);
            Teams.SetDirty(team.Country);

            // Change the font color
            endYearNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing research characteristics
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpecialityComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null)
            {
                return;
            }
            int no = (int) comboBox.Tag;

            // Do nothing if the value does not change
            TechSpeciality speciality = Techs.Specialities[comboBox.SelectedIndex];
            if (speciality == team.Specialities[no])
            {
                return;
            }

            Log.Info("[Team] speciality{0}: {1} -> {2} ({3}: {4})", no, Techs.GetSpecialityName(team.Specialities[no]),
                Techs.GetSpecialityName(speciality), team.Id, team.Name);

            // Change research characteristics
            ChangeTechSpeciality(team, no, speciality);

            // Update the items in the research institution list view
            teamListView.Refresh();

            // Update edit items
            UpdateEditableItemsValue(team);

            // Update the color of the edit item
            UpdateEditableItemsColor(team);

            // Notify the update of the characteristics of the research institution
            HoI2EditorController.OnItemChanged(EditorItemId.TeamSpeciality, this);
        }

        /// <summary>
        ///     Change research characteristics
        /// </summary>
        /// <param name="team">Target research institution</param>
        /// <param name="no">Research characteristic number</param>
        /// <param name="speciality">Research characteristics</param>
        private static void ChangeTechSpeciality(Team team, int no, TechSpeciality speciality)
        {
            if (speciality == TechSpeciality.None)
            {
                // If changed without characteristics, fill in the following items
                for (int i = no; i < MaxEditableSpecialities; i++)
                {
                    if (team.Specialities[i] != TechSpeciality.None || team.Specialities[i + 1] != TechSpeciality.None)
                    {
                        // 1 Pack in front
                        team.Specialities[i] = team.Specialities[i + 1];
                        // Set the edited flag
                        team.SetDirty(SpecialityItemIds[i]);
                    }
                }
            }
            else
            {
                // If there is a vacancy before the changed part, pack it
                for (int i = 0; i < no; i++)
                {
                    if (team.Specialities[i] == TechSpeciality.None)
                    {
                        no = i;
                        break;
                    }
                }
                // Search for duplicate items
                for (int i = 0; i < MaxEditableSpecialities; i++)
                {
                    if (i == no)
                    {
                        continue;
                    }

                    if (speciality == team.Specialities[i])
                    {
                        // Do nothing if it overlaps with other items and the original has no characteristics
                        if (team.Specialities[no] == TechSpeciality.None)
                        {
                            return;
                        }
                        // Swap characteristics with duplicate items
                        team.Specialities[i] = team.Specialities[no];
                        // Set the edited flag to be exchanged
                        team.SetDirty(SpecialityItemIds[i]);
                        break;
                    }
                }
                // Update the value of the target item
                team.Specialities[no] = speciality;
                // Set the edited flag of the target item
                team.SetDirty(SpecialityItemIds[no]);
            }

            // Set edited flags by country
            Teams.SetDirty(team.Country);
        }

        /// <summary>
        ///     Processing when changing the image file name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPictureNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            // Do nothing if the value does not change
            string pictureName = pictureNameTextBox.Text;
            if (string.IsNullOrEmpty(pictureName))
            {
                if (string.IsNullOrEmpty(team.PictureName))
                {
                    return;
                }
            }
            else
            {
                if (pictureName.Equals(team.PictureName))
                {
                    return;
                }
            }

            Log.Info("[Team] picture name: {0} -> {1} ({2}: {3})", team.PictureName, pictureName, team.Id, team.Name);

            // Update value
            team.PictureName = pictureName;

            // Update image file
            UpdateTeamPicture(team);

            // Set the edited flag
            team.SetDirty(TeamItemId.PictureName);
            Teams.SetDirty(team.Country);

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
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = Path.Combine(Game.FolderName, Game.PersonPicturePathName),
                FileName = team.PictureName,
                Filter = Resources.OpenBitmapFileDialogFilter
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pictureNameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }

        /// <summary>
        ///     ID Processing when the forward button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSortIdButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            // Research characteristics ID Sort in order
            SortSpeciality(team, new IdComparer());
        }

        /// <summary>
        ///     ABC Processing when the forward button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSortAbcButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Team team = GetSelectedTeam();
            if (team == null)
            {
                return;
            }

            // Research characteristics ABC Sort in order
            SortSpeciality(team, new AbcComparer());
        }

        /// <summary>
        ///     Sort research characteristics
        /// </summary>
        /// <param name="team">Research institutes to be sorted</param>
        /// <param name="comparer">For sorting</param>
        private void SortSpeciality(Team team, IComparer<TechSpeciality> comparer)
        {
            // Save items before sorting
            const int max = 7;
            TechSpeciality[] old = new TechSpeciality[max];
            for (int i = 0; i < max; i++)
            {
                old[i] = team.Specialities[i];
            }

            // Sort
            Array.Sort(team.Specialities, 0, max, comparer);

            // Research characteristics 1 Update check
            if (team.Specialities[0] != old[0])
            {
                // Set the edited flag
                team.SetDirty(TeamItemId.Speciality1);
                Teams.SetDirty(team.Country);
            }
            // Research characteristics 2 Update check
            if (team.Specialities[1] != old[1])
            {
                // Set the edited flag
                team.SetDirty(TeamItemId.Speciality2);
                Teams.SetDirty(team.Country);
            }
            // Research characteristics 3Update check
            if (team.Specialities[2] != old[2])
            {
                // Set the edited flag
                team.SetDirty(TeamItemId.Speciality3);
                Teams.SetDirty(team.Country);
            }
            // Research characteristics Four Update check
            if (team.Specialities[3] != old[3])
            {
                // Set the edited flag
                team.SetDirty(TeamItemId.Speciality4);
                Teams.SetDirty(team.Country);
            }
            // Research characteristics Five Update check
            if (team.Specialities[4] != old[4])
            {
                // Set the edited flag
                team.SetDirty(TeamItemId.Speciality5);
                Teams.SetDirty(team.Country);
            }
            // Research characteristics 6 Update check
            if (team.Specialities[5] != old[5])
            {
                // Set the edited flag
                team.SetDirty(TeamItemId.Speciality6);
                Teams.SetDirty(team.Country);
            }
            // Research characteristics 7 Update check
            if (team.Specialities[6] != old[6])
            {
                // Set the edited flag
                team.SetDirty(TeamItemId.Speciality7);
                Teams.SetDirty(team.Country);
            }

            // Update the items in the research institution list view
            teamListView.Refresh();

            // Update edit items
            UpdateEditableItemsValue(team);

            // Update the color of the edit item
            UpdateEditableItemsColor(team);
        }

        /// <summary>
        ///     Of research characteristics ABC For forward sorting
        /// </summary>
        private class AbcComparer : IComparer<TechSpeciality>
        {
            /// <summary>
            ///     ABC Priority
            /// </summary>
            private static readonly int[] Priorities =
            {
                109,
                3,
                29,
                15,
                12,
                47,
                18,
                39,
                34,
                0,
                37,
                36,
                25,
                20,
                28,
                42,
                24,
                11,
                14,
                46,
                19,
                21,
                13,
                23,
                33,
                35,
                2,
                17,
                7,
                9,
                45,
                22,
                41,
                40,
                38,
                4,
                32,
                48,
                8,
                44,
                16,
                6,
                31,
                1,
                27,
                26,
                5,
                43,
                30,
                10,
                49,
                50,
                51,
                52,
                53,
                54,
                55,
                56,
                57,
                58,
                59,
                60,
                61,
                62,
                63,
                64,
                65,
                66,
                67,
                68,
                69,
                70,
                71,
                72,
                73,
                74,
                75,
                76,
                77,
                78,
                79,
                80,
                81,
                82,
                83,
                84,
                85,
                86,
                87,
                88,
                89,
                90,
                91,
                92,
                93,
                94,
                95,
                96,
                97,
                98,
                99,
                100,
                101,
                102,
                103,
                104,
                105,
                106,
                107,
                108
            };

            /// <summary>
            ///     Compare research characteristics
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(TechSpeciality x, TechSpeciality y)
            {
                return Priorities[(int) x] - Priorities[(int) y];
            }
        }

        /// <summary>
        ///     Of research characteristics ID For forward sorting
        /// </summary>
        private class IdComparer : IComparer<TechSpeciality>
        {
            /// <summary>
            ///     Compare research characteristics
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(TechSpeciality x, TechSpeciality y)
            {
                // Move backward if not specified
                if (x == TechSpeciality.None)
                {
                    return 1;
                }
                if (y == TechSpeciality.None)
                {
                    return -1;
                }
                return (int) x - (int) y;
            }
        }

        #endregion
    }
}
