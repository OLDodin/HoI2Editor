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
    ///     Minister Editor Form
    /// </summary>
    public partial class MinisterEditorForm : Form
    {
        #region Internal field

        /// <summary>
        ///     List of ministers after narrowing down
        /// </summary>
        private readonly List<Minister> _list = new List<Minister>();

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
            StartYear,
            EndYear,
            Position,
            Personality,
            Ideology
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
        ///     Number of columns in the ministerial list view
        /// </summary>
        public const int MinisterListColumnCount = 8;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public MinisterEditorForm()
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
            // Narrow down the ministerial list
            NarrowMinisterList();

            // Sort the ministerial list
            SortMinisterList();

            // Update the display of the cabinet list
            UpdateMinisterList();

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
            // Minister list view
            countryColumnHeader.Width = HoI2EditorController.Settings.MinisterEditor.ListColumnWidth[0];
            idColumnHeader.Width = HoI2EditorController.Settings.MinisterEditor.ListColumnWidth[1];
            nameColumnHeader.Width = HoI2EditorController.Settings.MinisterEditor.ListColumnWidth[2];
            startYearColumnHeader.Width = HoI2EditorController.Settings.MinisterEditor.ListColumnWidth[3];
            endYearColumnHeader.Width = HoI2EditorController.Settings.MinisterEditor.ListColumnWidth[4];
            positionColumnHeader.Width = HoI2EditorController.Settings.MinisterEditor.ListColumnWidth[5];
            personalityColumnHeader.Width = HoI2EditorController.Settings.MinisterEditor.ListColumnWidth[6];
            ideologyColumnHeader.Width = HoI2EditorController.Settings.MinisterEditor.ListColumnWidth[7];

            // National list box
            countryListBox.ColumnWidth = DeviceCaps.GetScaledWidth(countryListBox.ColumnWidth);
            countryListBox.ItemHeight = DeviceCaps.GetScaledHeight(countryListBox.ItemHeight);

            // Window position
            Location = HoI2EditorController.Settings.MinisterEditor.Location;
            Size = HoI2EditorController.Settings.MinisterEditor.Size;
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

            // Initialize ministerial characteristics
            Ministers.InitPersonality();

            // Load the game settings file
            Misc.Load();

            // Read the character string definition file
            Config.Load();

            // Initialize edit items
            InitEditableItems();

            // Initialize the national list box
            InitCountryListBox();

            // Read ministerial files
            Ministers.Load();

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
            HoI2EditorController.OnMinisterEditorFormClosed();
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
                HoI2EditorController.Settings.MinisterEditor.Location = Location;
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
                HoI2EditorController.Settings.MinisterEditor.Size = Size;
            }
        }

        /// <summary>
        ///     Processing when the batch edit button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBatchButtonClick(object sender, EventArgs e)
        {
            MinisterBatchEditArgs args = new MinisterBatchEditArgs();
            args.TargetCountries.AddRange(from string name in countryListBox.SelectedItems
                select Countries.StringMap[name]);

            // Display the batch edit dialog
            MinisterBatchDialog dialog = new MinisterBatchDialog(args);
            if (dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            // If the end year is not set Misc Change the value of
            if (args.Items[(int) MinisterBatchItemId.EndYear] && !Misc.UseNewMinisterFilesFormat)
            {
                Misc.UseNewMinisterFilesFormat = true;
                HoI2EditorController.OnItemChanged(EditorItemId.MinisterEndYear, this);
            }

            // If the retirement year is not set Misc Change the value of
            if (args.Items[(int) MinisterBatchItemId.RetirementYear] && !Misc.EnableRetirementYearMinisters)
            {
                Misc.EnableRetirementYearMinisters = true;
                HoI2EditorController.OnItemChanged(EditorItemId.MinisterRetirementYear, this);
            }

            // Bulk editing process
            Ministers.BatchEdit(args);

            // Update ministerial list
            NarrowMinisterList();
            UpdateMinisterList();

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

        #region Minister list view

        /// <summary>
        ///     Update the display of the ministerial list
        /// </summary>
        private void UpdateMinisterList()
        {
            ministerListView.BeginUpdate();
            ministerListView.Items.Clear();

            // Register items in order
            foreach (Minister minister in _list)
            {
                ministerListView.Items.Add(CreateMinisterListViewItem(minister));
            }

            if (ministerListView.Items.Count > 0)
            {
                // Select the first item
                ministerListView.Items[0].Focused = true;
                ministerListView.Items[0].Selected = true;

                // Enable edit items
                EnableEditableItems();
            }
            else
            {
                // Disable edit items
                DisableEditableItems();
            }

            ministerListView.EndUpdate();
        }

        /// <summary>
        ///     Narrow down the ministerial list by country tag
        /// </summary>
        private void NarrowMinisterList()
        {
            _list.Clear();

            // Create a list of selected nations
            List<Country> tags = (from string s in countryListBox.SelectedItems select Countries.StringMap[s]).ToList();

            // Narrow down the commanders belonging to the selected nation in order
            _list.AddRange(Ministers.Items.Where(minister => tags.Contains(minister.Country)));
        }

        /// <summary>
        ///     Sort the ministerial list
        /// </summary>
        private void SortMinisterList()
        {
            switch (_key)
            {
                case SortKey.None: // No sort
                    break;

                case SortKey.Tag: // Country tag
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((minister1, minister2) => minister1.Country - minister2.Country);
                    }
                    else
                    {
                        _list.Sort((minister1, minister2) => minister2.Country - minister1.Country);
                    }
                    break;

                case SortKey.Id: // ID
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((minister1, minister2) => minister1.Id - minister2.Id);
                    }
                    else
                    {
                        _list.Sort((minister1, minister2) => minister2.Id - minister1.Id);
                    }
                    break;

                case SortKey.Name: // name
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((minister1, minister2) => string.CompareOrdinal(minister1.Name, minister2.Name));
                    }
                    else
                    {
                        _list.Sort((minister1, minister2) => string.CompareOrdinal(minister2.Name, minister1.Name));
                    }
                    break;

                case SortKey.StartYear: // Start year
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((minister1, minister2) => minister1.StartYear - minister2.StartYear);
                    }
                    else
                    {
                        _list.Sort((minister1, minister2) => minister2.StartYear - minister1.StartYear);
                    }
                    break;

                case SortKey.EndYear: // End year
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((minister1, minister2) => minister1.EndYear - minister2.EndYear);
                    }
                    else
                    {
                        _list.Sort((minister1, minister2) => minister2.EndYear - minister1.EndYear);
                    }
                    break;

                case SortKey.Position: // Status
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((minister1, minister2) => minister1.Position - minister2.Position);
                    }
                    else
                    {
                        _list.Sort((minister1, minister2) => minister2.Position - minister1.Position);
                    }
                    break;

                case SortKey.Personality: // Characteristic
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((minister1, minister2) => minister1.Personality - minister2.Personality);
                    }
                    else
                    {
                        _list.Sort((minister1, minister2) => minister2.Personality - minister1.Personality);
                    }
                    break;

                case SortKey.Ideology: // ideology
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((minister1, minister2) => minister1.Ideology - minister2.Ideology);
                    }
                    else
                    {
                        _list.Sort((minister1, minister2) => minister2.Ideology - minister1.Ideology);
                    }
                    break;
            }
        }

        /// <summary>
        ///     Processing when changing the selection item in the ministerial list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinisterListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update edit items
            UpdateEditableItems();
        }

        /// <summary>
        ///     Processing before editing items in the ministerial list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinisterListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
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

                case 3: // Start year
                    e.Type = ItemEditType.Text;
                    e.Text = startYearNumericUpDown.Text;
                    break;

                case 4: // End year
                    e.Type = ItemEditType.Text;
                    e.Text = endYearNumericUpDown.Text;
                    break;

                case 5: // Status
                    e.Type = ItemEditType.List;
                    e.Items = positionComboBox.Items.Cast<string>();
                    e.Index = positionComboBox.SelectedIndex;
                    e.DropDownWidth = positionComboBox.DropDownWidth;
                    break;

                case 6: // Characteristic
                    e.Type = ItemEditType.List;
                    e.Items = personalityComboBox.Items.Cast<string>();
                    e.Index = personalityComboBox.SelectedIndex;
                    e.DropDownWidth = personalityComboBox.DropDownWidth;
                    break;

                case 7: // ideology
                    e.Type = ItemEditType.List;
                    e.Items = ideologyComboBox.Items.Cast<string>();
                    e.Index = ideologyComboBox.SelectedIndex;
                    e.DropDownWidth = ideologyComboBox.DropDownWidth;
                    break;
            }
        }

        /// <summary>
        ///     Processing after editing items in the ministerial list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinisterListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
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

                case 3: // Start year
                    startYearNumericUpDown.Text = e.Text;
                    break;

                case 4: // End year
                    endYearNumericUpDown.Text = e.Text;
                    break;

                case 5: // Status
                    positionComboBox.SelectedIndex = e.Index;
                    break;

                case 6: // Characteristic
                    personalityComboBox.SelectedIndex = e.Index;
                    break;

                case 7: // ideology
                    ideologyComboBox.SelectedIndex = e.Index;
                    break;
            }

            // Since the items in the list view will be updated by yourself, it will be treated as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Processing when replacing items in the ministerial list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinisterListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // I will replace the items on my own, so I will treat it as canceled
            e.Cancel = true;

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;
            if (srcIndex < destIndex)
            {
                destIndex--;
            }

            Minister src = ministerListView.Items[srcIndex].Tag as Minister;
            if (src == null)
            {
                return;
            }
            Minister dest = ministerListView.Items[destIndex].Tag as Minister;
            if (dest == null)
            {
                return;
            }

            // Move items in the ministerial list
            Ministers.MoveItem(src, dest);
            MoveListItem(srcIndex, destIndex);

            // Set the edited flag
            Ministers.SetDirty(src.Country);
        }

        /// <summary>
        ///     Processing when a column is clicked in the ministerial list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinisterListViewColumnClick(object sender, ColumnClickEventArgs e)
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

                case 3: // Start year
                    if (_key == SortKey.StartYear)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.StartYear;
                    }
                    break;

                case 4: // End year
                    if (_key == SortKey.EndYear)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.EndYear;
                    }
                    break;

                case 5: // Status
                    if (_key == SortKey.Position)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Position;
                    }
                    break;

                case 6: // Characteristic
                    if (_key == SortKey.Personality)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Personality;
                    }
                    break;

                case 7: // ideology
                    if (_key == SortKey.Ideology)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Ideology;
                    }
                    break;

                default:
                    // Do nothing when clicking on a column with no items
                    return;
            }

            // Sort the ministerial list
            SortMinisterList();

            // Update ministerial list
            UpdateMinisterList();
        }

        /// <summary>
        ///     Processing when changing the width of columns in the cabinet list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinisterListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < MinisterListColumnCount))
            {
                HoI2EditorController.Settings.MinisterEditor.ListColumnWidth[e.ColumnIndex] =
                    ministerListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Processing when a new button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewButtonClick(object sender, EventArgs e)
        {
            Minister minister;
            Minister selected = GetSelectedMinister();
            if (selected != null)
            {
                // If there is a choice, the country tag or ID To take over and create an item
                minister = new Minister(selected)
                {
                    Id = Ministers.GetNewId(selected.Country),
                    Name = "",
                    PictureName = ""
                };

                // Set edited flags for each minister
                minister.SetDirtyAll();

                // Insert an item into the cabinet list
                Ministers.InsertItem(minister, selected);
                InsertListItem(minister, ministerListView.SelectedIndices[0] + 1);
            }
            else
            {
                Country country = Countries.Tags[countryListBox.SelectedIndex];
                // Create a new item
                minister = new Minister
                {
                    Country = country,
                    Id = Ministers.GetNewId(country),
                    StartYear = 1930,
                    EndYear = 1970,
                    RetirementYear = 1999,
                    Position = MinisterPosition.None,
                    Personality = 0,
                    Ideology = MinisterIdeology.None,
                    Loyalty = MinisterLoyalty.None
                };

                // Set edited flags for each minister
                minister.SetDirtyAll();

                // Add an item to the cabinet list
                Ministers.AddItem(minister);
                AddListItem(minister);

                // Enable edit items
                EnableEditableItems();
            }

            // Set edited flags for each country
            Ministers.SetDirty(minister.Country);

            // If it does not exist in the file list, add it
            if (!Ministers.FileNameMap.ContainsKey(minister.Country))
            {
                Ministers.FileNameMap.Add(minister.Country, Game.GetMinisterFileName(minister.Country));
                Ministers.SetDirtyList();
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
            Minister selected = GetSelectedMinister();
            if (selected == null)
            {
                return;
            }

            // Create an item by taking over the selected item
            Minister minister = new Minister(selected)
            {
                Id = Ministers.GetNewId(selected.Country)
            };

            // Set edited flags for each minister
            minister.SetDirtyAll();

            // Insert an item into the cabinet list
            Ministers.InsertItem(minister, selected);
            InsertListItem(minister, ministerListView.SelectedIndices[0] + 1);

            // Set edited flags for each country
            Ministers.SetDirty(minister.Country);
        }

        /// <summary>
        ///     Processing when the delete button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Minister selected = GetSelectedMinister();
            if (selected == null)
            {
                return;
            }

            // Remove an item from the cabinet list
            Ministers.RemoveItem(selected);
            RemoveItem(ministerListView.SelectedIndices[0]);

            // Disable edit items when there are no items in the list
            if (ministerListView.Items.Count == 0)
            {
                DisableEditableItems();
            }

            // Set the edited flag
            Ministers.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Processing when the button is pressed to the beginning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTopButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Minister selected = GetSelectedMinister();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the top of the list
            int index = ministerListView.SelectedIndices[0];
            if (ministerListView.SelectedIndices[0] == 0)
            {
                return;
            }

            Minister top = ministerListView.Items[0].Tag as Minister;
            if (top == null)
            {
                return;
            }

            // Move items in the ministerial list
            Ministers.MoveItem(selected, top);
            MoveListItem(index, 0);

            // Set the edited flag
            Ministers.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Processing when pressing the up button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Minister selected = GetSelectedMinister();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the top of the list
            int index = ministerListView.SelectedIndices[0];
            if (index == 0)
            {
                return;
            }

            Minister upper = ministerListView.Items[index - 1].Tag as Minister;
            if (upper == null)
            {
                return;
            }

            // Move items in the ministerial list
            Ministers.MoveItem(selected, upper);
            MoveListItem(index, index - 1);

            // Set the edited flag
            Ministers.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Processing when the down button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Minister selected = GetSelectedMinister();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the end of the list
            int index = ministerListView.SelectedIndices[0];
            if (index == ministerListView.Items.Count - 1)
            {
                return;
            }

            Minister lower = ministerListView.Items[index + 1].Tag as Minister;
            if (lower == null)
            {
                return;
            }

            // Move items in the ministerial list
            Ministers.MoveItem(selected, lower);
            MoveListItem(index, index + 1);

            // Set the edited flag
            Ministers.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Processing when the button is pressed to the end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBottomButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Minister selected = GetSelectedMinister();
            if (selected == null)
            {
                return;
            }

            // Do nothing if the selection is at the end of the list
            int index = ministerListView.SelectedIndices[0];
            int bottomIndex = ministerListView.Items.Count - 1;
            if (ministerListView.SelectedIndices[0] == bottomIndex)
            {
                return;
            }

            Minister bottom = ministerListView.Items[ministerListView.Items.Count - 1].Tag as Minister;
            if (bottom == null)
            {
                return;
            }

            // Move items in the ministerial list
            Ministers.MoveItem(selected, bottom);
            MoveListItem(index, ministerListView.Items.Count - 1);

            // Set the edited flag
            Ministers.SetDirty(selected.Country);
        }

        /// <summary>
        ///     Add an item to the cabinet list
        /// </summary>
        /// <param name="minister">Items to be inserted</param>
        private void AddListItem(Minister minister)
        {
            // Add an item to the refined list
            _list.Add(minister);

            // Add an item to the ministerial list view
            ministerListView.Items.Add(CreateMinisterListViewItem(minister));

            // Select the added item
            ministerListView.Items[ministerListView.Items.Count - 1].Focused = true;
            ministerListView.Items[ministerListView.Items.Count - 1].Selected = true;
            ministerListView.EnsureVisible(ministerListView.Items.Count - 1);
        }

        /// <summary>
        ///     Insert an item into the cabinet list
        /// </summary>
        /// <param name="minister">Items to be inserted</param>
        /// <param name="index">Insertion destination position</param>
        private void InsertListItem(Minister minister, int index)
        {
            // Insert an item in the refined list
            _list.Insert(index, minister);

            // Insert an item in the ministerial list view
            ministerListView.Items.Insert(index, CreateMinisterListViewItem(minister));

            // Select the inserted item
            ministerListView.Items[index].Focused = true;
            ministerListView.Items[index].Selected = true;
            ministerListView.EnsureVisible(index);
        }

        /// <summary>
        ///     Remove an item from the cabinet list
        /// </summary>
        /// <param name="index">Position to be deleted</param>
        private void RemoveItem(int index)
        {
            // Remove an item from the refined list
            _list.RemoveAt(index);

            // Remove an item from the ministerial list view
            ministerListView.Items.RemoveAt(index);

            // Select the next item after the deleted item
            if (index < ministerListView.Items.Count)
            {
                ministerListView.Items[index].Focused = true;
                ministerListView.Items[index].Selected = true;
            }
            else if (index - 1 >= 0)
            {
                // At the end of the list, select the item before the deleted item
                ministerListView.Items[index - 1].Focused = true;
                ministerListView.Items[index - 1].Selected = true;
            }
        }

        /// <summary>
        ///     Move items in the cabinet list
        /// </summary>
        /// <param name="src">Source position</param>
        /// <param name="dest">Destination position</param>
        private void MoveListItem(int src, int dest)
        {
            Minister minister = _list[src];

            if (src > dest)
            {
                // When moving up
                // Move items in the refined list
                _list.Insert(dest, minister);
                _list.RemoveAt(src + 1);

                // Move items in the ministerial list view
                ministerListView.Items.Insert(dest, CreateMinisterListViewItem(minister));
                ministerListView.Items.RemoveAt(src + 1);
            }
            else
            {
                // When moving down
                // Move items in the refined list
                _list.Insert(dest + 1, minister);
                _list.RemoveAt(src);

                // Move items in the ministerial list view
                ministerListView.Items.Insert(dest + 1, CreateMinisterListViewItem(minister));
                ministerListView.Items.RemoveAt(src);
            }

            // Select the item to move to
            ministerListView.Items[dest].Focused = true;
            ministerListView.Items[dest].Selected = true;
            ministerListView.EnsureVisible(dest);
        }

        /// <summary>
        ///     Create an item in the ministerial list view
        /// </summary>
        /// <param name="minister">Ministerial data</param>
        /// <returns>Items in the ministerial list view</returns>
        private static ListViewItem CreateMinisterListViewItem(Minister minister)
        {
            if (minister == null)
            {
                return null;
            }

            ListViewItem item = new ListViewItem
            {
                Text = Countries.Strings[(int) minister.Country],
                Tag = minister
            };
            item.SubItems.Add(IntHelper.ToString(minister.Id));
            item.SubItems.Add(minister.Name);
            item.SubItems.Add(IntHelper.ToString(minister.StartYear));
            item.SubItems.Add(Misc.UseNewMinisterFilesFormat ? IntHelper.ToString(minister.EndYear) : "");
            item.SubItems.Add(Config.GetText(Ministers.PositionNames[(int) minister.Position]));
            item.SubItems.Add(Ministers.Personalities[minister.Personality].NameText);
            item.SubItems.Add(Config.GetText(Ministers.IdeologyNames[(int) minister.Ideology]));

            return item;
        }

        /// <summary>
        ///     Get selected ministerial data
        /// </summary>
        /// <returns>Selected ministerial data</returns>
        private Minister GetSelectedMinister()
        {
            // If there is no selection
            if (ministerListView.SelectedItems.Count == 0)
            {
                return null;
            }

            return ministerListView.SelectedItems[0].Tag as Minister;
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
            foreach (Country country in HoI2EditorController.Settings.MinisterEditor.Countries)
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
                brush = Ministers.IsDirty(country) ? new SolidBrush(Color.Red) : new SolidBrush(SystemColors.WindowText);
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
            HoI2EditorController.Settings.MinisterEditor.Countries =
                countryListBox.SelectedIndices.Cast<int>().Select(index => Countries.Tags[index]).ToList();

            // Update ministerial list
            NarrowMinisterList();
            UpdateMinisterList();
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

            // Issue a dummy event to narrow down the ministerial list
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

            // Status
            positionComboBox.BeginUpdate();
            positionComboBox.Items.Clear();
            width = positionComboBox.Width;
            foreach (string s in Ministers.PositionNames.Where(id => id != TextId.Empty).Select(Config.GetText))
            {
                positionComboBox.Items.Add(s);
                width = Math.Max(width, (int) g.MeasureString(s, positionComboBox.Font).Width + margin);
            }
            positionComboBox.DropDownWidth = width;
            positionComboBox.EndUpdate();

            // Characteristic
            personalityComboBox.DropDownWidth =
                Ministers.Personalities
                    .Select(info => (int) g.MeasureString(info.NameText, personalityComboBox.Font).Width +
                                    SystemInformation.VerticalScrollBarWidth + margin)
                    .Concat(new[] { personalityComboBox.Width })
                    .Max();

            // ideology
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

            // Loyalty
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
        }

        /// <summary>
        ///     Update edit items
        /// </summary>
        private void UpdateEditableItems()
        {
            // Do nothing if there is no selection
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Update edit items
            UpdateEditableItemsValue(minister);

            // Update the color of the edit item
            UpdateEditableItemsColor(minister);

            // Item move button status update
            topButton.Enabled = ministerListView.SelectedIndices[0] != 0;
            upButton.Enabled = ministerListView.SelectedIndices[0] != 0;
            downButton.Enabled = ministerListView.SelectedIndices[0] != ministerListView.Items.Count - 1;
            bottomButton.Enabled = ministerListView.SelectedIndices[0] != ministerListView.Items.Count - 1;
        }

        /// <summary>
        ///     Update the value of the edit item
        /// </summary>
        /// <param name="minister">Ministerial data</param>
        private void UpdateEditableItemsValue(Minister minister)
        {
            countryComboBox.SelectedIndex = minister.Country != Country.None ? (int) minister.Country - 1 : -1;
            idNumericUpDown.Value = minister.Id;
            nameTextBox.Text = minister.Name;
            startYearNumericUpDown.Value = minister.StartYear;
            if (Misc.UseNewMinisterFilesFormat)
            {
                endYearLabel.Enabled = true;
                endYearNumericUpDown.Enabled = true;
                endYearNumericUpDown.Value = minister.EndYear;
                endYearNumericUpDown.Text = IntHelper.ToString((int) endYearNumericUpDown.Value);
            }
            else
            {
                endYearLabel.Enabled = false;
                endYearNumericUpDown.Enabled = false;
                endYearNumericUpDown.ResetText();
            }
            if (Misc.EnableRetirementYearMinisters)
            {
                retirementYearLabel.Enabled = true;
                retirementYearNumericUpDown.Enabled = true;
                retirementYearNumericUpDown.Value = minister.RetirementYear;
                retirementYearNumericUpDown.Text = IntHelper.ToString((int) retirementYearNumericUpDown.Value);
            }
            else
            {
                retirementYearLabel.Enabled = false;
                retirementYearNumericUpDown.Enabled = false;
                retirementYearNumericUpDown.ResetText();
            }
            positionComboBox.SelectedIndex = minister.Position != MinisterPosition.None
                ? (int) minister.Position - 1
                : -1;
            UpdatePersonalityComboBox(minister);
            ideologyComboBox.SelectedIndex = minister.Ideology != MinisterIdeology.None
                ? (int) minister.Ideology - 1
                : -1;
            loyaltyComboBox.SelectedIndex = minister.Loyalty != MinisterLoyalty.None ? (int) minister.Loyalty - 1 : -1;
            pictureNameTextBox.Text = minister.PictureName;
            UpdateMinisterPicture(minister);
        }

        /// <summary>
        ///     Update the color of the edit item
        /// </summary>
        /// <param name="minister">Ministerial data</param>
        private void UpdateEditableItemsColor(Minister minister)
        {
            // Update the color of the combo box
            countryComboBox.Refresh();
            positionComboBox.Refresh();
            personalityComboBox.Refresh();
            ideologyComboBox.Refresh();
            loyaltyComboBox.Refresh();

            // Update the color of the edit item
            idNumericUpDown.ForeColor = minister.IsDirty(MinisterItemId.Id) ? Color.Red : SystemColors.WindowText;
            nameTextBox.ForeColor = minister.IsDirty(MinisterItemId.Name) ? Color.Red : SystemColors.WindowText;
            startYearNumericUpDown.ForeColor = minister.IsDirty(MinisterItemId.StartYear)
                ? Color.Red
                : SystemColors.WindowText;
            endYearNumericUpDown.ForeColor = minister.IsDirty(MinisterItemId.EndYear)
                ? Color.Red
                : SystemColors.WindowText;
            retirementYearNumericUpDown.ForeColor = minister.IsDirty(MinisterItemId.RetirementYear)
                ? Color.Red
                : SystemColors.WindowText;
            pictureNameTextBox.ForeColor = minister.IsDirty(MinisterItemId.PictureName)
                ? Color.Red
                : SystemColors.WindowText;
        }

        /// <summary>
        ///     Enable edit items
        /// </summary>
        private void EnableEditableItems()
        {
            countryComboBox.Enabled = true;
            idNumericUpDown.Enabled = true;
            nameTextBox.Enabled = true;
            startYearNumericUpDown.Enabled = true;
            positionComboBox.Enabled = true;
            personalityComboBox.Enabled = true;
            ideologyComboBox.Enabled = true;
            loyaltyComboBox.Enabled = true;
            pictureNameTextBox.Enabled = true;
            pictureNameBrowseButton.Enabled = true;

            // Reset the character string cleared at the time of invalidation
            idNumericUpDown.Text = IntHelper.ToString((int) idNumericUpDown.Value);
            startYearNumericUpDown.Text = IntHelper.ToString((int) startYearNumericUpDown.Value);

            if (Misc.UseNewMinisterFilesFormat)
            {
                endYearNumericUpDown.Enabled = true;
                endYearNumericUpDown.Text = IntHelper.ToString((int) endYearNumericUpDown.Value);
            }
            if (Misc.EnableRetirementYearMinisters)
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
            startYearNumericUpDown.ResetText();
            endYearNumericUpDown.ResetText();
            retirementYearNumericUpDown.ResetText();
            pictureNameTextBox.ResetText();
            ministerPictureBox.ImageLocation = "";

            countryComboBox.Enabled = false;
            idNumericUpDown.Enabled = false;
            nameTextBox.Enabled = false;
            startYearNumericUpDown.Enabled = false;
            endYearNumericUpDown.Enabled = false;
            retirementYearNumericUpDown.Enabled = false;
            positionComboBox.Enabled = false;
            personalityComboBox.Enabled = false;
            ideologyComboBox.Enabled = false;
            loyaltyComboBox.Enabled = false;
            pictureNameTextBox.Enabled = false;
            pictureNameBrowseButton.Enabled = false;

            cloneButton.Enabled = false;
            removeButton.Enabled = false;
            topButton.Enabled = false;
            upButton.Enabled = false;
            downButton.Enabled = false;
            bottomButton.Enabled = false;
        }

        /// <summary>
        ///     Update the item in the ministerial trait combo box
        /// </summary>
        /// <param name="minister">Ministerial data</param>
        private void UpdatePersonalityComboBox(Minister minister)
        {
            personalityComboBox.BeginUpdate();
            personalityComboBox.Items.Clear();
            if (minister.Position == MinisterPosition.None)
            {
                // If the value of ministerial status is incorrect, register only the current ministerial characteristics
                personalityComboBox.Items.Add(Ministers.Personalities[minister.Personality].NameText);
                personalityComboBox.SelectedIndex = 0;
            }
            else if (!Ministers.PositionPersonalityTable[(int) minister.Position].Contains(minister.Personality))
            {
                // If the ministerial characteristics do not match the ministerial status, register as a candidate in one shot
                personalityComboBox.Items.Add(Ministers.Personalities[minister.Personality].NameText);
                personalityComboBox.SelectedIndex = 0;

                // Register ministerial status and corresponding ministerial characteristics in order
                foreach (int personality in Ministers.PositionPersonalityTable[(int) minister.Position])
                {
                    personalityComboBox.Items.Add(Ministers.Personalities[personality].NameText);
                }
            }
            else
            {
                // Register ministerial status and corresponding ministerial characteristics in order
                foreach (int personality in Ministers.PositionPersonalityTable[(int) minister.Position])
                {
                    personalityComboBox.Items.Add(Ministers.Personalities[personality].NameText);
                    if (personality == minister.Personality)
                    {
                        personalityComboBox.SelectedIndex = personalityComboBox.Items.Count - 1;
                    }
                }
            }
            personalityComboBox.EndUpdate();
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
            Minister minister = GetSelectedMinister();
            if (minister != null)
            {
                Brush brush;
                if ((Countries.Tags[e.Index] == minister.Country) && minister.IsDirty(MinisterItemId.Country))
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
        ///     Item drawing process of ministerial status combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPositionComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Minister minister = GetSelectedMinister();
            if (minister != null)
            {
                Brush brush;
                if ((e.Index == (int) minister.Position - 1) && minister.IsDirty(MinisterItemId.Position))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = positionComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of ministerial characteristic combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPersonalityComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Minister minister = GetSelectedMinister();
            if (minister != null)
            {
                Brush brush;
                if ((minister.Position == MinisterPosition.None) ||
                    !Ministers.PositionPersonalityTable[(int) minister.Position].Contains(minister.Personality))
                {
                    // If the value of ministerial status is incorrect, only the current ministerial characteristics are registered.
                    // If the ministerial trait does not match the ministerial status, the current ministerial trait is registered at the top.
                    if ((e.Index == 0) && minister.IsDirty(MinisterItemId.Personality))
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
                    if ((Ministers.PositionPersonalityTable[(int) minister.Position][e.Index] ==
                         minister.Personality) &&
                        minister.IsDirty(MinisterItemId.Personality))
                    {
                        brush = new SolidBrush(Color.Red);
                    }
                    else
                    {
                        brush = new SolidBrush(SystemColors.WindowText);
                    }
                }
                string s = personalityComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of ideology combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIdeologyComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Minister minister = GetSelectedMinister();
            if (minister != null)
            {
                Brush brush;
                if ((e.Index == (int) minister.Ideology - 1) && minister.IsDirty(MinisterItemId.Ideology))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = ideologyComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Loyalty combo box item drawing process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoyaltyComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Minister minister = GetSelectedMinister();
            if (minister != null)
            {
                Brush brush;
                if ((e.Index == (int) minister.Loyalty - 1) && minister.IsDirty(MinisterItemId.Loyalty))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = loyaltyComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Update the item in the ministerial image picture box
        /// </summary>
        /// <param name="minister">Ministerial data</param>
        private void UpdateMinisterPicture(Minister minister)
        {
            if (!string.IsNullOrEmpty(minister.PictureName) &&
                (minister.PictureName.IndexOfAny(Path.GetInvalidPathChars()) < 0))
            {
                string fileName = Game.GetReadFileName(Game.PersonPicturePathName,
                    Path.ChangeExtension(minister.PictureName, ".bmp"));
                ministerPictureBox.ImageLocation = File.Exists(fileName) ? fileName : "";
            }
            else
            {
                ministerPictureBox.ImageLocation = "";
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
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Do nothing if the value does not change
            Country country = Countries.Tags[countryComboBox.SelectedIndex];
            if (country == minister.Country)
            {
                return;
            }

            // Set the edited flag for the country tag before the change
            Ministers.SetDirty(minister.Country);

            Log.Info("[Minister] country: {0} -> {1} ({2}: {3})", Countries.Strings[(int) minister.Country],
                Countries.Strings[(int) country], minister.Id, minister.Name);

            // Update value
            minister.Country = country;

            // Update items in the ministerial list view
            ministerListView.SelectedItems[0].Text = Countries.Strings[(int) minister.Country];

            // Set edited flags for each minister
            minister.SetDirty(MinisterItemId.Country);

            // Set the edited flag of the changed country tag
            Ministers.SetDirty(minister.Country);

            // If it does not exist in the file list, add it
            if (!Ministers.FileNameMap.ContainsKey(minister.Country))
            {
                Ministers.FileNameMap.Add(minister.Country, Game.GetMinisterFileName(minister.Country));
                Ministers.SetDirtyList();
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
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int id = (int) idNumericUpDown.Value;
            if (id == minister.Id)
            {
                return;
            }

            Log.Info("[Minister] id: {0} -> {1} ({2})", minister.Id, id, minister.Name);

            // Update value
            minister.Id = id;

            // Update items in the ministerial list view
            ministerListView.SelectedItems[0].SubItems[1].Text = IntHelper.ToString(minister.Id);

            // Set the edited flag
            minister.SetDirty(MinisterItemId.Id);
            Ministers.SetDirty(minister.Country);

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
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Do nothing if the value does not change
            string name = nameTextBox.Text;
            if (string.IsNullOrEmpty(name))
            {
                if (string.IsNullOrEmpty(minister.Name))
                {
                    return;
                }
            }
            else
            {
                if (name.Equals(minister.Name))
                {
                    return;
                }
            }

            Log.Info("[Minister] name: {0} -> {1} ({2})", minister.Name, name, minister.Id);

            // Update value
            minister.Name = name;

            // Update items in the ministerial list view
            ministerListView.SelectedItems[0].SubItems[2].Text = minister.Name;

            // Set the edited flag
            minister.SetDirty(MinisterItemId.Name);
            Ministers.SetDirty(minister.Country);

            // Change the font color
            nameTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the start year
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartYearNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int startYear = (int) startYearNumericUpDown.Value;
            if (startYear == minister.StartYear)
            {
                return;
            }

            Log.Info("[Minister] start year: {0} -> {1} ({2}: {3})", minister.StartYear, startYear, minister.Id,
                minister.Name);

            // Update value
            minister.StartYear = startYear;

            // Update items in the ministerial list view
            ministerListView.SelectedItems[0].SubItems[3].Text = IntHelper.ToString(minister.StartYear);

            // Set the edited flag
            minister.SetDirty(MinisterItemId.StartYear);
            Ministers.SetDirty(minister.Country);

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
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int endYear = (int) endYearNumericUpDown.Value;
            if (endYear == minister.EndYear)
            {
                return;
            }

            Log.Info("[Minister] end year: {0} -> {1} ({2}: {3})", minister.EndYear, endYear, minister.Id, minister.Name);

            // Update value
            minister.EndYear = endYear;

            // Update items in the ministerial list view
            ministerListView.SelectedItems[0].SubItems[4].Text = IntHelper.ToString(minister.EndYear);

            // Set the edited flag
            minister.SetDirty(MinisterItemId.EndYear);
            Ministers.SetDirty(minister.Country);

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
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int retirementYear = (int) retirementYearNumericUpDown.Value;
            if (retirementYear == minister.RetirementYear)
            {
                return;
            }

            Log.Info("[Minister] retirement year: {0} -> {1} ({2}: {3})", minister.RetirementYear, retirementYear,
                minister.Id, minister.Name);

            // Update value
            minister.RetirementYear = retirementYear;

            // Set the edited flag
            minister.SetDirty(MinisterItemId.RetirementYear);
            Ministers.SetDirty(minister.Country);

            // Change the font color
            retirementYearNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing ministerial status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPositionComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Do nothing if the value does not change
            MinisterPosition position = (MinisterPosition) (positionComboBox.SelectedIndex + 1);
            if (position == minister.Position)
            {
                return;
            }

            Log.Info("[Minister] position: {0} -> {1} ({2}: {3})",
                Config.GetText(Ministers.PositionNames[(int) minister.Position]),
                Config.GetText(Ministers.PositionNames[(int) position]), minister.Id, minister.Name);

            // Update value
            minister.Position = position;

            // Update items in the ministerial list view
            ministerListView.SelectedItems[0].SubItems[5].Text =
                Config.GetText(Ministers.PositionNames[(int) minister.Position]);

            // Change trait options according to your position
            UpdatePersonalityComboBox(minister);

            // Set the edited flag
            minister.SetDirty(MinisterItemId.Position);
            Ministers.SetDirty(minister.Country);

            // Update drawing to change the item color of the ministerial status combo box
            positionComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when changing ministerial characteristics
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPersonalityComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Cannot be changed when the ministerial status is indefinite
            if (minister.Position == MinisterPosition.None)
            {
                return;
            }

            // Do nothing if the value does not change
            int personality;
            if (Ministers.PositionPersonalityTable[(int) minister.Position].Contains(minister.Personality))
            {
                personality =
                    Ministers.PositionPersonalityTable[(int) minister.Position][personalityComboBox.SelectedIndex];
            }
            else
            {
                if (personalityComboBox.SelectedIndex == 0)
                {
                    return;
                }
                personality =
                    Ministers.PositionPersonalityTable[(int) minister.Position][personalityComboBox.SelectedIndex - 1];
            }
            if (personality == minister.Personality)
            {
                return;
            }

            Log.Info("[Minister] personality: {0} -> {1} ({2}: {3})",
                Ministers.Personalities[minister.Personality].NameText, Ministers.Personalities[personality].NameText,
                minister.Id, minister.Name);

            // Update value
            minister.Personality = personality;

            // Update items in the ministerial list view
            ministerListView.SelectedItems[0].SubItems[6].Text = Ministers.Personalities[minister.Personality].NameText;

            // Set the edited flag
            minister.SetDirty(MinisterItemId.Personality);
            Ministers.SetDirty(minister.Country);

            // Update items in the ministerial combo box
            UpdatePersonalityComboBox(minister);
        }

        /// <summary>
        ///     Processing when changing ideology
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIdeologyComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Do nothing if the value does not change
            MinisterIdeology ideology = !string.IsNullOrEmpty(ideologyComboBox.Items[0].ToString())
                ? (MinisterIdeology) (ideologyComboBox.SelectedIndex + 1)
                : (MinisterIdeology) ideologyComboBox.SelectedIndex;
            if (ideology == minister.Ideology)
            {
                return;
            }

            Log.Info("[Minister] ideology: {0} -> {1} ({2}: {3})",
                Config.GetText(Ministers.IdeologyNames[(int) minister.Ideology]),
                Config.GetText(Ministers.IdeologyNames[(int) ideology]), minister.Id, minister.Name);

            // Update value
            minister.Ideology = ideology;

            // Update items in the ministerial list view
            ministerListView.SelectedItems[0].SubItems[7].Text =
                Config.GetText(Ministers.IdeologyNames[(int) minister.Ideology]);

            // Set the edited flag
            minister.SetDirty(MinisterItemId.Ideology);
            Ministers.SetDirty(minister.Country);

            // Update drawing to change the item color of the ideology combo box
            ideologyComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when changing loyalty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoyaltyComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Do nothing if the value does not change
            MinisterLoyalty loyalty = !string.IsNullOrEmpty(loyaltyComboBox.Items[0].ToString())
                ? (MinisterLoyalty) (loyaltyComboBox.SelectedIndex + 1)
                : (MinisterLoyalty) loyaltyComboBox.SelectedIndex;
            if (loyalty == minister.Loyalty)
            {
                return;
            }

            Log.Info("[Minister] loyalty: {0} -> {1} ({2}: {3})", Ministers.LoyaltyNames[(int) minister.Loyalty],
                Ministers.LoyaltyNames[(int) loyalty], minister.Id, minister.Name);

            // Update value
            minister.Loyalty = loyalty;

            // Set the edited flag
            minister.SetDirty(MinisterItemId.Loyalty);
            Ministers.SetDirty(minister.Country);

            // Update drawing to change the item color of the loyalty combo box
            loyaltyComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when changing the image file name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPictureNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Do nothing if the value does not change
            string pictureName = pictureNameTextBox.Text;
            if (string.IsNullOrEmpty(pictureName))
            {
                if (string.IsNullOrEmpty(minister.PictureName))
                {
                    return;
                }
            }
            else
            {
                if (pictureName.Equals(minister.PictureName))
                {
                    return;
                }
            }

            Log.Info("[Minister] picture name: {0} -> {1} ({2}: {3})", minister.PictureName, pictureName, minister.Id,
                minister.Name);

            // Update value
            minister.PictureName = pictureName;

            // Update ministerial image
            UpdateMinisterPicture(minister);

            // Set the edited flag
            minister.SetDirty(MinisterItemId.PictureName);
            Ministers.SetDirty(minister.Country);

            // Set the font color
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
            Minister minister = GetSelectedMinister();
            if (minister == null)
            {
                return;
            }

            // Open the file selection dialog
            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = Path.Combine(Game.FolderName, Game.PersonPicturePathName),
                FileName = minister.PictureName,
                Filter = Resources.OpenBitmapFileDialogFilter
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pictureNameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }

        #endregion
    }
}
