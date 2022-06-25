using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Controllers;
using HoI2Editor.Controls;
using HoI2Editor.Models;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Technical tree editor form
    /// </summary>
    public partial class TechEditorForm : Form
    {
        #region Internal field

        /// <summary>
        ///     Technical tree panel controller
        /// </summary>
        private TechTreePanelController _techTreePanelController;

        #endregion

        #region Public number

        /// <summary>
        ///     Number of columns in required technology list views
        /// </summary>
        public const int RequiredListColumnCount = 2;

        /// <summary>
        ///     Number of columns in small research list views
        /// </summary>
        public const int ComponentListColumnCount = 5;

        /// <summary>
        ///     Number of columns of technical effect list views
        /// </summary>
        public const int EffectListColumnCount = 5;

        /// <summary>
        ///     Number of columns of coordinates list views
        /// </summary>
        public const int PositionListColumnCount = 2;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public TechEditorForm()
        {
            InitializeComponent();

            // Initialization of the form
            InitForm();
        }

        #endregion

        #region Data processing

        /// <summary>
        ///     Processing after reading data
        /// </summary>
        public void OnFileLoaded()
        {
            // Initialize the editing items of the technical tab
            InitTechItems();

            // Initialize editing items for small research tabs
            InitComponentItems();

            // Initialize editing items on the technical effect tab
            InitEffectItems();

            // Update the technical list of the required technology tab
            UpdateRequiredTechListItems();

            // Update the technical list of technical event tabs
            UpdateEventTechListItems();

            // Initialize the category list box
            InitCategoryList();
        }

        /// <summary>
        ///     Processing after data saving
        /// </summary>
        public void OnFileSaved()
        {
            // Update the display because the edited flag is cleared
            categoryListBox.Refresh();
            techListBox.Refresh();
            UpdateCategoryItems();
            UpdateEditableItems();
        }

        /// <summary>
        ///     Processing after changing edit items
        /// </summary>
        /// <param name="id">Editing project ID</param>
        public void OnItemChanged(EditorItemId id)
        {
            // do nothing
        }

        #endregion

        #region Form

        /// <summary>
        ///     Initialization of the form
        /// </summary>
        private void InitForm()
        {
            // Technical category list box
            categoryListBox.ItemHeight = DeviceCaps.GetScaledHeight(categoryListBox.ItemHeight);

            // Technical item list box
            techListBox.ItemHeight = DeviceCaps.GetScaledHeight(techListBox.ItemHeight);

            // Technical coordinates list view
            techXColumnHeader.Width = HoI2EditorController.Settings.TechEditor.TechPositionListColumnWidth[0];
            techYColumnHeader.Width = HoI2EditorController.Settings.TechEditor.TechPositionListColumnWidth[1];

            // AND Conditions required Technology List View
            andIdColumnHeader.Width = HoI2EditorController.Settings.TechEditor.AndRequiredListColumnWidth[0];
            andNameColumnHeader.Width = HoI2EditorController.Settings.TechEditor.AndRequiredListColumnWidth[1];

            // OR Conditions required Technical Wrist View
            orIdColumnHeader.Width = HoI2EditorController.Settings.TechEditor.OrRequiredListColumnWidth[0];
            orNameColumnHeader.Width = HoI2EditorController.Settings.TechEditor.OrRequiredListColumnWidth[1];

            // List of small research
            componentIdColumnHeader.Width = HoI2EditorController.Settings.TechEditor.ComponentListColumnWidth[0];
            componentNameColumnHeader.Width = HoI2EditorController.Settings.TechEditor.ComponentListColumnWidth[1];
            componentSpecialityColumnHeader.Width = HoI2EditorController.Settings.TechEditor.ComponentListColumnWidth[2];
            componentDifficultyColumnHeader.Width = HoI2EditorController.Settings.TechEditor.ComponentListColumnWidth[3];
            componentDoubleTimeColumnHeader.Width = HoI2EditorController.Settings.TechEditor.ComponentListColumnWidth[4];

            // Small research characteristic combo box
            componentSpecialityComboBox.ItemHeight = DeviceCaps.GetScaledHeight(componentSpecialityComboBox.ItemHeight);

            // Technical effect wrist view
            commandTypeColumnHeader.Width = HoI2EditorController.Settings.TechEditor.EffectListColumnWidth[0];
            commandWhichColumnHeader.Width = HoI2EditorController.Settings.TechEditor.EffectListColumnWidth[1];
            commandValueColumnHeader.Width = HoI2EditorController.Settings.TechEditor.EffectListColumnWidth[2];
            commandWhenColumnHeader.Width = HoI2EditorController.Settings.TechEditor.EffectListColumnWidth[3];
            commandWhereColumnHeader.Width = HoI2EditorController.Settings.TechEditor.EffectListColumnWidth[4];

            // Label coordinates list view
            labelXColumnHeader.Width = HoI2EditorController.Settings.TechEditor.LabelPositionListColumnWidth[0];
            labelYColumnHeader.Width = HoI2EditorController.Settings.TechEditor.LabelPositionListColumnWidth[1];

            // Invention event coordinates list view
            eventXColumnHeader.Width = HoI2EditorController.Settings.TechEditor.EventPositionListColumnWidth[0];
            eventYColumnHeader.Width = HoI2EditorController.Settings.TechEditor.EventPositionListColumnWidth[1];

            // Technical tree panel
            _techTreePanelController = new TechTreePanelController(treePictureBox) { AllowDragDrop = true };
            _techTreePanelController.ItemMouseDown += OnTechTreeLabelMouseDown;
            _techTreePanelController.ItemDragDrop += OnTreePictureBoxDragDrop;

            // Window position
            Location = HoI2EditorController.Settings.TechEditor.Location;
            Size = HoI2EditorController.Settings.TechEditor.Size;
        }

        /// <summary>
        ///     Change the size of the form according to the technology tree image size
        /// </summary>
        private void UpdateFormSize()
        {
            // Get the size of the desktop
            Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

            int height = Height + (treePictureBox.Image.Height - treePanel.Height);

            Height = Math.Min(height, screenRect.Height);
        }

        /// <summary>
        ///     Processing when reading form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormLoad(object sender, EventArgs e)
        {
            // Initialize research characteristics
            Techs.InitSpecialities();

            // Read the game configuration file
            Misc.Load();

            // Read the character string definition file
            Config.Load();

            // Read the technical definition file
            Techs.Load();

            // Processing after reading data
            OnFileLoaded();

            // Change the size of the form according to the technology tree image size
            UpdateFormSize();
        }

        /// <summary>
        ///     Processing for form closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            // Close the form if it is not edited
            if (!HoI2EditorController.IsDirty())
            {
                return;
            }

            // Inquiries whether to save
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
        ///     Processing after form closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            HoI2EditorController.OnTechEditorFormClosed();
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
                HoI2EditorController.Settings.TechEditor.Location = Location;
            }
        }

        /// <summary>
        ///     Processing for form resization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                HoI2EditorController.Settings.TechEditor.Size = Size;
            }
        }

        /// <summary>
        ///     Treatment when pressing the re -loading button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReloadButtonClick(object sender, EventArgs e)
        {
            // If you have edited, ask whether to save
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
        ///     Processing when pressing the storage button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.Save();
        }

        /// <summary>
        ///     Processing when pressing the closing button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region Category Category

        /// <summary>
        ///     Initialize the category list box
        /// </summary>
        private void InitCategoryList()
        {
            categoryListBox.Items.Clear();
            foreach (TechGroup grp in Techs.Groups)
            {
                categoryListBox.Items.Add(grp);
            }

            // Reflect the selected category
            int index = HoI2EditorController.Settings.TechEditor.Category;
            if ((index < 0) || (index >= categoryListBox.Items.Count))
            {
                index = 0;
            }
            categoryListBox.SelectedIndex = index;
        }

        /// <summary>
        ///     Processing when changing the selection item of the category list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCategoryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the item list
            UpdateItemList();

            // Update the technical tree panel
            _techTreePanelController.Category = (TechCategory) categoryListBox.SelectedIndex;
            _techTreePanelController.Update();

            // Update the category tab item
            UpdateCategoryItems();

            // Disable technical items editing tabs
            DisableTechTab();
            DisableRequiredTab();
            DisableComponentTab();
            DisableEffectTab();
            DisableLabelTab();
            DisableEventTab();

            // Select a category tab
            editTabControl.SelectedIndex = (int) TechEditorTab.Category;

            cloneButton.Enabled = false;
            removeButton.Enabled = false;
            topButton.Enabled = false;
            upButton.Enabled = false;
            downButton.Enabled = false;
            bottomButton.Enabled = false;

            // Save the selected category
            HoI2EditorController.Settings.TechEditor.Category = categoryListBox.SelectedIndex;
        }

        /// <summary>
        ///     Category list box item drawing process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCategoryListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // If there are no items, do nothing
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw the character string of the item
            Brush brush;
            if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
            {
                // For items with changes, change the character color
                TechGroup grp = Techs.Groups[e.Index];
                brush = grp.IsDirty() ? new SolidBrush(Color.Red) : new SolidBrush(categoryListBox.ForeColor);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            e.Graphics.DrawString(categoryListBox.Items[e.Index].ToString(), e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Get the selected technical group
        /// </summary>
        /// <returns>Selected technology group</returns>
        private TechGroup GetSelectedGroup()
        {
            return Techs.Groups[categoryListBox.SelectedIndex];
        }

        #endregion

        #region Technical item list

        /// <summary>
        ///     Update the display of the technical item list
        /// </summary>
        private void UpdateItemList()
        {
            techListBox.BeginUpdate();

            techListBox.Items.Clear();
            treePictureBox.Controls.Clear();

            foreach (ITechItem item in Techs.Groups[categoryListBox.SelectedIndex].Items)
            {
                techListBox.Items.Add(item);
                _techTreePanelController.AddItem(item);
            }

            techListBox.EndUpdate();
        }

        /// <summary>
        ///     Technical item List box selection item processing when changing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update edit items
            UpdateEditableItems();
        }

        /// <summary>
        ///     Technical item list viewing items when replacing items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechListBoxItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // Swap the items on your own, so treat it as a cancellation
            e.Cancel = true;

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;

            ITechItem src = techListBox.Items[srcIndex] as ITechItem;
            if (src == null)
            {
                return;
            }
            ITechItem dest = techListBox.Items[destIndex] as ITechItem;
            if (dest == null)
            {
                return;
            }


            // Move the item in the technical item list
            TechGroup grp = GetSelectedGroup();
            grp.MoveItem(src, dest);

            // Move the item list view item
            MoveTechListItem(srcIndex, destIndex);

            // Set the edited flag
            grp.SetDirty();
        }

        /// <summary>
        ///     Update edit items
        /// </summary>
        private void UpdateEditableItems()
        {
            // If there is no selection item
            ITechItem item = GetSelectedItem();
            if (item == null)
            {
                // Disable technology / necessary technology / small research / technical effect / technical label / technical event tab
                DisableTechTab();
                DisableRequiredTab();
                DisableComponentTab();
                DisableEffectTab();
                DisableLabelTab();
                DisableEventTab();

                // Select a category tab
                editTabControl.SelectedIndex = (int) TechEditorTab.Category;

                return;
            }

            if (item is TechItem)
            {
                // Update edit items
                TechItem applicationItem = item as TechItem;
                UpdateTechItems(applicationItem);
                UpdateRequiredItems(applicationItem);
                UpdateComponentItems(applicationItem);
                UpdateEffectItems(applicationItem);

                // Enable technology / necessary technology / small research / technical effect tab
                EnableTechTab();
                EnableRequiredTab();
                EnableComponentTab();
                EnableEffectTab();

                // Disable technical label / technical event tab
                DisableLabelTab();
                DisableEventTab();

                // Select a technical tab if you select anything other than technology / necessary technology / small research / technical effect tab
                if (editTabControl.SelectedIndex != (int) TechEditorTab.Tech &&
                    editTabControl.SelectedIndex != (int) TechEditorTab.Required &&
                    editTabControl.SelectedIndex != (int) TechEditorTab.Component &&
                    editTabControl.SelectedIndex != (int) TechEditorTab.Effect)
                {
                    editTabControl.SelectedIndex = (int) TechEditorTab.Tech;
                }
            }
            else if (item is TechLabel)
            {
                // Update edit items
                UpdateLabelItems(item as TechLabel);

                // Enable the technical label tab
                EnableLabelTab();

                // Disable technology / necessary technology / small research / technical effect / technical event tab
                DisableTechTab();
                DisableRequiredTab();
                DisableComponentTab();
                DisableEffectTab();
                DisableEventTab();

                // Select a technical label tab
                editTabControl.SelectedIndex = (int) TechEditorTab.Label;
            }
            else if (item is TechEvent)
            {
                // Update edit items
                UpdateEventItems(item as TechEvent);

                // Enable the technical event tab
                EnableEventTab();

                // Disable technology / necessary technology / small research / technical effect / technical label tab
                DisableTechTab();
                DisableRequiredTab();
                DisableComponentTab();
                DisableEffectTab();
                DisableLabelTab();

                // Select a technical event tab
                editTabControl.SelectedIndex = (int) TechEditorTab.Event;
            }

            cloneButton.Enabled = true;
            removeButton.Enabled = true;
            topButton.Enabled = techListBox.SelectedIndex != 0;
            upButton.Enabled = techListBox.SelectedIndex != 0;
            downButton.Enabled = techListBox.SelectedIndex != techListBox.Items.Count - 1;
            bottomButton.Enabled = techListBox.SelectedIndex != techListBox.Items.Count - 1;
        }

        /// <summary>
        ///     Technical item list box item drawing process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // If there are no items, do nothing
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Change the background color
            if ((e.State & DrawItemState.Selected) == 0)
            {
                if (techListBox.Items[e.Index] is TechLabel)
                {
                    e.Graphics.FillRectangle(Brushes.AliceBlue,
                        new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
                }
                else if (techListBox.Items[e.Index] is TechEvent)
                {
                    e.Graphics.FillRectangle(Brushes.Honeydew,
                        new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
                }
            }

            // Draw the character string of the item
            Brush brush;
            if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
            {
                // For items with changes, change the character color
                ITechItem item = techListBox.Items[e.Index] as ITechItem;
                brush = item != null && item.IsDirty()
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(categoryListBox.ForeColor);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            e.Graphics.DrawString(techListBox.Items[e.Index].ToString(), e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when pressing the new technology button in the technical item list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewTechButtonClick(object sender, EventArgs e)
        {
            TechGroup grp = GetSelectedGroup();

            // Create an item
            TechItem item = new TechItem
            {
                Name = Config.GetTempKey(),
                ShortName = Config.GetTempKey(),
                Desc = Config.GetTempKey(),
                Year = 1936
            };
            Config.SetText(item.Name, "", Game.TechTextFileName);
            Config.SetText(item.ShortName, "", Game.TechTextFileName);
            Config.SetText(item.Desc, "", Game.TechTextFileName);

            // Register in the duplicate character string list
            Techs.AddDuplicatedListItem(item);

            // Set the edited flag
            grp.SetDirty();
            item.SetDirtyAll();

            ITechItem selected = techListBox.SelectedItem as ITechItem;
            if (selected != null)
            {
                // Take over the top coordinates of the selection item
                item.Positions.Add(new TechPosition { X = selected.Positions[0].X, Y = selected.Positions[0].Y });

                if (selected is TechItem)
                {
                    // If the selection item is a technical application, increase the ID by 10
                    TechItem selectedApplication = selected as TechItem;
                    item.Id = Techs.GetNewId(selectedApplication.Id + 10);
                }
                else
                {
                    // Search for unused technical IDs with 1010 or later
                    item.Id = Techs.GetNewId(1010);
                }

                // Add an empty research
                item.CreateNewComponents();

                // Insert the item in the technical item list
                grp.InsertItem(item, selected);

                // Insert the item in the list view
                InsertTechListItem(item, techListBox.SelectedIndex + 1);
            }
            else
            {
                // Register the provisional coordinates
                item.Positions.Add(new TechPosition());

                // Search for unused technical IDs with 1010 or later
                item.Id = Techs.GetNewId(1010);

                // Add an empty research
                item.CreateNewComponents();

                // Add items to the technical item list
                grp.AddItem(item);

                // Add the item to the item list view
                AddTechListItem(item);
            }

            // Add a label to the technical tree
            _techTreePanelController.AddItem(item);

            // Update technical items and ID correspondence
            Techs.UpdateTechIdMap();
            // Update the required technology combo box
            UpdateRequiredTechListItems();
            // Update the technical event technology ID combo box item
            UpdateEventTechListItems();

            // Notify the update of the technical item list
            HoI2EditorController.OnItemChanged(EditorItemId.TechItemList, this);

            Log.Info("[Tech] Added new tech: {0}", item.Id);
        }

        /// <summary>
        ///     Processing when pressing a new label button in the technical item list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewLabelButtonClick(object sender, EventArgs e)
        {
            TechGroup grp = GetSelectedGroup();

            // Create an item
            TechLabel item = new TechLabel { Name = Config.GetTempKey() };
            Config.SetText(item.Name, "", Game.TechTextFileName);

            // Register in the duplicate character string list
            Techs.AddDuplicatedListItem(item);

            // Set the edited flag
            grp.SetDirty();
            item.SetDirtyAll();

            ITechItem selected = techListBox.SelectedItem as ITechItem;
            if (selected != null)
            {
                // Take over the top coordinates of the selection item
                item.Positions.Add(new TechPosition { X = selected.Positions[0].X, Y = selected.Positions[0].Y });

                // Insert the item in the technical item list
                grp.InsertItem(item, selected);

                // Insert the item in the list view
                InsertTechListItem(item, techListBox.SelectedIndex + 1);
            }
            else
            {
                // Register the provisional coordinates
                item.Positions.Add(new TechPosition());

                // Add items to the technical item list
                grp.AddItem(item);

                // Add the item to the item list view
                AddTechListItem(item);
            }

            // Add a label to the technical tree
            _techTreePanelController.AddItem(item);

            Log.Info("[Tech] Added new label");
        }

        /// <summary>
        ///     Processing when pressing a new event button in the technical item list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewEventButtonClick(object sender, EventArgs e)
        {
            TechGroup grp = GetSelectedGroup();

            // Create an item
            TechEvent item = new TechEvent();

            // Set the edited flag
            grp.SetDirty();
            item.SetDirtyAll();

            ITechItem selected = techListBox.SelectedItem as ITechItem;
            if (selected != null)
            {
                // Take over the top coordinates of the selection item
                item.Positions.Add(new TechPosition { X = selected.Positions[0].X, Y = selected.Positions[0].Y });

                // If the selection item is a technical event, increase the ID by 10
                if (selected is TechEvent)
                {
                    TechEvent selectedEvent = selected as TechEvent;
                    item.Id = selectedEvent.Id + 10;
                }

                // Insert the item in the technical item list
                grp.InsertItem(item, selected);

                // Insert the item in the list view
                InsertTechListItem(item, techListBox.SelectedIndex + 1);
            }
            else
            {
                // Register the provisional coordinates
                item.Positions.Add(new TechPosition());

                // Add items to the technical item list
                grp.AddItem(item);

                // Add the item to the item list view
                AddTechListItem(item);
            }

            // Add a label to the technical tree
            _techTreePanelController.AddItem(item);

            Log.Info("[Tech] Added new event: {0}", item.Id);
        }

        /// <summary>
        ///     Processing when pressing the duplicate button on the technical item list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloneButtonClick(object sender, EventArgs e)
        {
            TechGroup grp = GetSelectedGroup();

            // If you don't have a selection item, you won't do anything
            ITechItem selected = GetSelectedItem();
            if (selected == null)
            {
                return;
            }

            // Duplicate the item
            ITechItem item = selected.Clone();

            // Register in the duplicate character string list
            Techs.AddDuplicatedListItem(item);

            // Insert the item in the technical item list
            grp.InsertItem(item, selected);

            if (item is TechItem)
            {
                // Update technical items and ID correspondence
                Techs.UpdateTechIdMap();
                // Update the required technology combo box
                UpdateRequiredTechListItems();
                // Update the technical event technology ID combo box item
                UpdateEventTechListItems();
            }

            // Set the edited flag
            grp.SetDirty();
            item.SetDirtyAll();

            // Insert the item in the list view
            InsertTechListItem(item, techListBox.SelectedIndex + 1);

            // Add a label to the technical tree
            _techTreePanelController.AddItem(item);

            if (item is TechItem)
            {
                // Notify the update of the technical item list
                HoI2EditorController.OnItemChanged(EditorItemId.TechItemList, this);

                TechItem techItem = item as TechItem;
                Log.Info("[Tech] Added new tech: {0}", techItem.Id);
            }
            else if (item is TechLabel)
            {
                Log.Info("[Tech] Added new label");
            }
            else if (item is TechEvent)
            {
                TechEvent eventItem = item as TechEvent;
                Log.Info("[Tech] Added new event: {0}", eventItem.Id);
            }
        }

        /// <summary>
        ///     Technical item list deletion button processing when pressing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRemoveButtonClick(object sender, EventArgs e)
        {
            TechGroup grp = GetSelectedGroup();

            // If you don't have a selection item, you won't do anything
            ITechItem selected = GetSelectedItem();
            if (selected == null)
            {
                return;
            }

            // Delete items from the technical item list
            grp.RemoveItem(selected);

            // Delete the item from the item list view
            RemoveTechListItem(techListBox.SelectedIndex);

            // Delete the label from the technical tree
            _techTreePanelController.RemoveItem(selected);

            if (selected is TechItem)
            {
                // Update the required technology combo box
                UpdateRequiredTechListItems();
                // Update the technical event technology ID combo box item
                UpdateEventTechListItems();
            }

            // If the items are gone, disable edited items
            if (techListBox.Items.Count == 0)
            {
                // Disable technology / necessary technology / small research / technical effect / technical label / technical event tab
                DisableTechTab();
                DisableRequiredTab();
                DisableComponentTab();
                DisableEffectTab();
                DisableLabelTab();
                DisableEventTab();

                // Select a category tab
                editTabControl.SelectedIndex = (int) TechEditorTab.Category;
            }

            // Set the edited flag
            grp.SetDirty();

            if (selected is TechItem)
            {
                // Notify the update of the technical item list
                HoI2EditorController.OnItemChanged(EditorItemId.TechItemList, this);

                TechItem techItem = selected as TechItem;
                Log.Info("[Tech] Removed tech: {0} [{1}]", techItem.Id, techItem);
            }
            else if (selected is TechLabel)
            {
                TechLabel labelItem = selected as TechLabel;
                Log.Info("[Tech] Removed label: {0}", labelItem);
            }
            else if (selected is TechEvent)
            {
                TechEvent eventItem = selected as TechEvent;
                Log.Info("[Tech] Removed event: {0}", eventItem.Id);
            }
        }

        /// <summary>
        ///     Processing when pressing button to the beginning of the technical item list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTopButtonClick(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            ITechItem selected = GetSelectedItem();
            if (selected == null)
            {
                return;
            }

            // If the selection item is the top of the list, do nothing
            int index = techListBox.SelectedIndex;
            if (index == 0)
            {
                return;
            }

            TechGroup grp = GetSelectedGroup();
            ITechItem top = grp.Items[0];
            if (top == null)
            {
                return;
            }

            // Move the item in the technical item list
            grp.MoveItem(selected, top);

            // Move the item list view item
            MoveTechListItem(index, 0);

            if (selected is TechItem)
            {
                // Update the required technology combo box
                UpdateRequiredTechListItems();
                // Update the technical event technology ID combo box item
                UpdateEventTechListItems();
            }

            // Set the edited flag
            grp.SetDirty();
        }

        /// <summary>
        ///     Processing when pressing button on top of the technical item list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpButtonClick(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            ITechItem selected = GetSelectedItem();
            if (selected == null)
            {
                return;
            }

            // If the selection item is the top of the list, do nothing
            int index = techListBox.SelectedIndex;
            if (index == 0)
            {
                return;
            }

            TechGroup grp = GetSelectedGroup();
            ITechItem upper = grp.Items[index - 1];

            // Move the item in the technical item list
            grp.MoveItem(selected, upper);

            // Move the item list view item
            MoveTechListItem(index, index - 1);

            if (selected is TechItem)
            {
                // Update the required technology combo box
                UpdateRequiredTechListItems();
                // Update the technical event technology ID combo box item
                UpdateEventTechListItems();
            }

            // Set the edited flag
            grp.SetDirty();
        }

        /// <summary>
        ///     Processing when pressing button under the technical item list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownButtonClick(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            ITechItem selected = GetSelectedItem();
            if (selected == null)
            {
                return;
            }

            // If the selection item is the end of the list, do nothing
            int index = techListBox.SelectedIndex;
            if (index == techListBox.Items.Count - 1)
            {
                return;
            }

            TechGroup grp = GetSelectedGroup();
            ITechItem lower = grp.Items[index + 1];

            // Move the item in the technical item list
            grp.MoveItem(selected, lower);

            // Move the item list view item
            MoveTechListItem(index, index + 1);

            if (selected is TechItem)
            {
                // Update the required technology combo box
                UpdateRequiredTechListItems();
                // Update the technical event technology ID combo box item
                UpdateEventTechListItems();
            }

            // Set the edited flag
            grp.SetDirty();
        }

        /// <summary>
        ///     Processing when pressing button to the end of the technical item list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBottomButtonClick(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            ITechItem selected = GetSelectedItem();
            if (selected == null)
            {
                return;
            }

            // If the selection item is the end of the list, do nothing
            int index = techListBox.SelectedIndex;
            if (index == techListBox.Items.Count - 1)
            {
                return;
            }

            TechGroup grp = GetSelectedGroup();
            ITechItem bottom = grp.Items[techListBox.Items.Count - 1];

            // Move the item in the technical item list
            grp.MoveItem(selected, bottom);

            // Move the item list view item
            MoveTechListItem(index, techListBox.Items.Count - 1);

            if (selected is TechItem)
            {
                // Update the required technology combo box
                UpdateRequiredTechListItems();
                // Update the technical event technology ID combo box item
                UpdateEventTechListItems();
            }

            // Set the edited flag
            grp.SetDirty();
        }

        /// <summary>
        ///     Add the item to the item list view
        /// </summary>
        /// <param name="item">Additional items</param>
        private void AddTechListItem(ITechItem item)
        {
            // Add the item to the item list view
            techListBox.Items.Add(item);

            // Select the added item
            techListBox.SelectedIndex = techListBox.Items.Count - 1;
        }

        /// <summary>
        ///     Insert the item in the list view
        /// </summary>
        /// <param name="item">Items to be inserted</param>
        /// <param name="index">Position of insertion destination</param>
        private void InsertTechListItem(object item, int index)
        {
            // Insert the item in the list view
            techListBox.Items.Insert(index, item);

            // Select the inserted item
            techListBox.SelectedIndex = index;
        }

        /// <summary>
        ///     Delete item list view items
        /// </summary>
        /// <param name="index">Position to be deleted</param>
        private void RemoveTechListItem(int index)
        {
            // Delete the item from the item list view
            techListBox.Items.RemoveAt(index);

            if (index < techListBox.Items.Count)
            {
                // Select the following items in the deleted item
                techListBox.SelectedIndex = index;
            }
            else if (index > 0)
            {
                // At the end of the list, select the item in front of the deleted item.
                techListBox.SelectedIndex = index - 1;
            }
        }

        /// <summary>
        ///     Move the item list view item
        /// </summary>
        /// <param name="src">Location of the source</param>
        /// <param name="dest">Position of destination</param>
        private void MoveTechListItem(int src, int dest)
        {
            ITechItem item = techListBox.Items[src] as ITechItem;
            if (item == null)
            {
                return;
            }

            if (src > dest)
            {
                // When moving up
                techListBox.Items.Insert(dest, item);
                techListBox.Items.RemoveAt(src + 1);
            }
            else
            {
                // When moving down
                techListBox.Items.Insert(dest + 1, item);
                techListBox.Items.RemoveAt(src);
            }

            // Select the destination item
            techListBox.SelectedIndex = dest;
        }

        /// <summary>
        ///     Get the selected technical items
        /// </summary>
        /// <returns>Selected technical items</returns>
        private ITechItem GetSelectedItem()
        {
            return techListBox.SelectedItem as ITechItem;
        }

        #endregion

        #region Technical tree

        /// <summary>
        ///     Technical tree label mouse processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechTreeLabelMouseDown(object sender, TechTreePanelController.ItemMouseEventArgs e)
        {
            // Select the technical item list item
            techListBox.SelectedItem = e.Item;
        }

        /// <summary>
        ///     Processing when dropped into a technical tree picture box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTreePictureBoxDragDrop(object sender, TechTreePanelController.ItemDragEventArgs e)
        {
            // Update the items in the coordinates list view
            for (int i = 0; i < e.Item.Positions.Count; i++)
            {
                if (e.Item.Positions[i] == e.Position)
                {
                    if (e.Item is TechItem)
                    {
                        techPositionListView.Items[i].Text = IntHelper.ToString(e.Position.X);
                        techPositionListView.Items[i].SubItems[1].Text = IntHelper.ToString(e.Position.Y);
                        techXNumericUpDown.Value = e.Position.X;
                        techYNumericUpDown.Value = e.Position.Y;
                        techXNumericUpDown.ForeColor = Color.Red;
                        techYNumericUpDown.ForeColor = Color.Red;
                    }
                    else if (e.Item is TechLabel)
                    {
                        labelPositionListView.Items[i].Text = IntHelper.ToString(e.Position.X);
                        labelPositionListView.Items[i].SubItems[1].Text = IntHelper.ToString(e.Position.Y);
                        labelXNumericUpDown.Value = e.Position.X;
                        labelYNumericUpDown.Value = e.Position.Y;
                        labelXNumericUpDown.ForeColor = Color.Red;
                        labelYNumericUpDown.ForeColor = Color.Red;
                    }
                    else
                    {
                        eventPositionListView.Items[i].Text = IntHelper.ToString(e.Position.X);
                        eventPositionListView.Items[i].SubItems[1].Text = IntHelper.ToString(e.Position.Y);
                        eventXNumericUpDown.Value = e.Position.X;
                        eventYNumericUpDown.Value = e.Position.Y;
                        eventXNumericUpDown.ForeColor = Color.Red;
                        eventYNumericUpDown.ForeColor = Color.Red;
                    }
                    break;
                }
            }
            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            e.Item.SetDirty();
            e.Position.SetDirtyAll();
        }

        #endregion

        #region Category tab

        /// <summary>
        ///     Update the category tab item
        /// </summary>
        private void UpdateCategoryItems()
        {
            // Update the value of edit items
            TechGroup grp = GetSelectedGroup();
            categoryNameTextBox.Text = grp.ToString();
            categoryDescTextBox.Text = grp.GetDesc();

            // Update the color of the edit item
            categoryNameTextBox.ForeColor = grp.IsDirty(TechGroupItemId.Name) ? Color.Red : SystemColors.WindowText;
            categoryDescTextBox.ForeColor = grp.IsDirty(TechGroupItemId.Desc) ? Color.Red : SystemColors.WindowText;
        }

        /// <summary>
        ///     Processing when the technology group name is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCategoryNameTextBoxTextChanged(object sender, EventArgs e)
        {
            TechGroup grp = GetSelectedGroup();

            // If there is no change in the value, do nothing
            string name = categoryNameTextBox.Text;
            if (name.Equals(grp.ToString()))
            {
                return;
            }

            Log.Info("[Tech] Changed category name: {0} -> {1} <{2}>", grp, name, grp.Name);

            // Update the value
            Config.SetText(grp.Name, name, Game.TechTextFileName);

            // The display is updated by resetting the category list box item
            // At this time, the focus is removed by the re -selection, so the event handler is temporarily disabled.
            categoryListBox.SelectedIndexChanged -= OnCategoryListBoxSelectedIndexChanged;
            categoryListBox.Items[(int) grp.Category] = name;
            categoryListBox.SelectedIndexChanged += OnCategoryListBoxSelectedIndexChanged;

            // Set the edited flag
            grp.SetDirty(TechGroupItemId.Name);

            // Change the character color
            categoryNameTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Technical group explanation processing when changing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCategoryDescTextBoxTextChanged(object sender, EventArgs e)
        {
            TechGroup grp = GetSelectedGroup();

            // If there is no change in the value, do nothing
            string desc = categoryDescTextBox.Text;
            if (desc.Equals(grp.GetDesc()))
            {
                return;
            }

            Log.Info("[Tech] Changed category description: {0} -> {1} <{2}>", grp.GetDesc(), desc, grp.Desc);

            // Update the value
            Config.SetText(grp.Desc, desc, Game.TechTextFileName);

            // Set the edited flag
            grp.SetDirty(TechGroupItemId.Desc);

            // Change the character color
            categoryDescTextBox.ForeColor = Color.Red;
        }

        #endregion

        #region Technical tab

        /// <summary>
        ///     Initialize the technical tab item
        /// </summary>
        private void InitTechItems()
        {
            // Image file name
            bool flag = Game.Type == GameType.DarkestHour;
            techPictureNameLabel.Enabled = flag;
            techPictureNameTextBox.Enabled = flag;
            techPictureNameBrowseButton.Enabled = flag;
        }

        /// <summary>
        ///     Update the technical tab item
        /// </summary>
        /// <param name="item">Technical application</param>
        private void UpdateTechItems(TechItem item)
        {
            // Update the value of edit items
            techNameTextBox.Text = item.ToString();
            techShortNameTextBox.Text = item.GetShortName();
            techIdNumericUpDown.Value = item.Id;
            techYearNumericUpDown.Value = item.Year;
            UpdateTechPositionList(item);
            UpdateTechPicture(item);

            // Update the color of the edit item
            techNameTextBox.ForeColor = item.IsDirty(TechItemId.Name) ? Color.Red : SystemColors.WindowText;
            techShortNameTextBox.ForeColor = item.IsDirty(TechItemId.ShortName) ? Color.Red : SystemColors.WindowText;
            techIdNumericUpDown.ForeColor = item.IsDirty(TechItemId.Id) ? Color.Red : SystemColors.WindowText;
            techYearNumericUpDown.ForeColor = item.IsDirty(TechItemId.Year) ? Color.Red : SystemColors.WindowText;
            techPictureNameTextBox.ForeColor = item.IsDirty(TechItemId.PictureName)
                ? Color.Red
                : SystemColors.WindowText;
        }

        /// <summary>
        ///     Enable the technical tab
        /// </summary>
        private void EnableTechTab()
        {
            // Enable the tab
            editTabControl.TabPages[(int) TechEditorTab.Tech].Enabled = true;

            // Reset the clear value when disabling
            techIdNumericUpDown.Text = IntHelper.ToString((int) techIdNumericUpDown.Value);
            techYearNumericUpDown.Text = IntHelper.ToString((int) techYearNumericUpDown.Value);
        }

        /// <summary>
        ///     Disable the technical tab
        /// </summary>
        private void DisableTechTab()
        {
            // Disable the tab
            editTabControl.TabPages[(int) TechEditorTab.Tech].Enabled = false;

            // Clear the value of edit items
            techNameTextBox.ResetText();
            techShortNameTextBox.ResetText();
            techIdNumericUpDown.ResetText();
            techYearNumericUpDown.ResetText();
            techPositionListView.Items.Clear();
            techXNumericUpDown.ResetText();
            techYNumericUpDown.ResetText();
            Image prev = techPictureBox.Image;
            techPictureBox.Image = null;
            prev?.Dispose();
        }

        /// <summary>
        ///     Update the technical coordinates
        /// </summary>
        /// <param name="item">technology</param>
        private void UpdateTechPositionList(TechItem item)
        {
            techPositionListView.BeginUpdate();
            techPositionListView.Items.Clear();

            foreach (TechPosition position in item.Positions)
            {
                ListViewItem li = new ListViewItem(IntHelper.ToString(position.X));
                li.SubItems.Add(IntHelper.ToString(position.Y));
                techPositionListView.Items.Add(li);
            }

            if (techPositionListView.Items.Count > 0)
            {
                // Select the first item
                techPositionListView.Items[0].Focused = true;
                techPositionListView.Items[0].Selected = true;

                // Enable editing items
                EnableTechPositionItems();
            }
            else
            {
                // Disable edit items
                DisableTechPositionItems();
            }

            techPositionListView.EndUpdate();
        }

        /// <summary>
        ///     Enable the editing item of technical coordinates
        /// </summary>
        private void EnableTechPositionItems()
        {
            // Reset the clear value when disabling
            techXNumericUpDown.Text = IntHelper.ToString((int) techXNumericUpDown.Value);
            techYNumericUpDown.Text = IntHelper.ToString((int) techYNumericUpDown.Value);

            // Enable editing items
            techXNumericUpDown.Enabled = true;
            techYNumericUpDown.Enabled = true;

            techPositionRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     Disable editing items of technical coordinates
        /// </summary>
        private void DisableTechPositionItems()
        {
            // Clear the value of edit items
            techXNumericUpDown.ResetText();
            techYNumericUpDown.ResetText();

            // Disable edit items
            techXNumericUpDown.Enabled = false;
            techYNumericUpDown.Enabled = false;

            techPositionRemoveButton.Enabled = false;
        }

        /// <summary>
        ///     Processing at the time of technical name change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // If there is no change in the value, do nothing
            string name = techNameTextBox.Text;
            if (name.Equals(item.ToString()))
            {
                return;
            }

            // If it is a duplicated character string, reset the definition name
            if (Techs.IsDuplicatedName(item.Name))
            {
                Techs.DecrementDuplicatedListCount(item.Name);
                item.Name = Config.GetTempKey();
                Techs.IncrementDuplicatedListCount(item.Name);
            }

            Log.Info("[Tech] Changed tech name: {0} -> {1} <{2}>", item, name, item.Name);

            // Update the value
            Config.SetText(item.Name, name, Game.TechTextFileName);

            // The display is updated by resetting the item list box item
            // At this time, the focus is removed by the re -selection, so the event handler is temporarily disabled.
            techListBox.SelectedIndexChanged -= OnTechListBoxSelectedIndexChanged;
            techListBox.Items[techListBox.SelectedIndex] = item;
            techListBox.SelectedIndexChanged += OnTechListBoxSelectedIndexChanged;

            // The display is updated by resetting the technical combo box item
            // At this time, the focus is removed by the re -selection, so the event handler is temporarily disabled.
            andTechComboBox.SelectedIndexChanged -= OnAndTechComboBoxSelectedIndexChanged;
            orTechComboBox.SelectedIndexChanged -= OnOrTechComboBoxSelectedIndexChanged;
            eventTechComboBox.SelectedIndexChanged -= OnEventTechComboBoxSelectedIndexChanged;
            for (int i = 0; i < andTechComboBox.Items.Count; i++)
            {
                if (andTechComboBox.Items[i] == item)
                {
                    andTechComboBox.Items[i] = item;
                    orTechComboBox.Items[i] = item;
                    eventTechComboBox.Items[i] = item;
                }
            }
            andTechComboBox.SelectedIndexChanged += OnAndTechComboBoxSelectedIndexChanged;
            orTechComboBox.SelectedIndexChanged += OnOrTechComboBoxSelectedIndexChanged;
            eventTechComboBox.SelectedIndexChanged += OnEventTechComboBoxSelectedIndexChanged;

            // Set the edited flag
            item.SetDirty(TechItemId.Name);

            // Change the character color
            techNameTextBox.ForeColor = Color.Red;

            // Notify the update of the technical item name
            HoI2EditorController.OnItemChanged(EditorItemId.TechItemName, this);
        }

        /// <summary>
        ///     Processing at the time of technology shortening name change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechShortNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // If there is no change in the value, do nothing
            string shortName = techShortNameTextBox.Text;
            if (shortName.Equals(item.GetShortName()))
            {
                return;
            }

            // If it is a duplicated character string, reset the definition name
            if (Techs.IsDuplicatedName(item.ShortName))
            {
                Techs.DecrementDuplicatedListCount(item.ShortName);
                item.ShortName = Config.GetTempKey();
                Techs.IncrementDuplicatedListCount(item.ShortName);
            }

            Log.Info("[Tech] Changed tech short name: {0} -> {1} <{2}>", item.GetShortName(), shortName, item.ShortName);

            // Update the value
            Config.SetText(item.ShortName, shortName, Game.TechTextFileName);

            // Update the label name on the technical tree
            _techTreePanelController.UpdateItem(item);

            // Set the edited flag
            item.SetDirty(TechItemId.ShortName);

            // Change the character color
            techShortNameTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing technical ID
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechIdNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // If there is no change in the value, do nothing
            int id = (int) techIdNumericUpDown.Value;
            if (id == item.Id)
            {
                return;
            }

            Log.Info("[Tech] Changed tech id: {0} -> {1} [{2}]", item.Id, id, item.Name);

            // Update the value
            Techs.ModifyTechId(item, id);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty(TechItemId.Id);

            // Change the character color
            techIdNumericUpDown.ForeColor = Color.Red;

            // Notify the update of the technical item ID
            HoI2EditorController.OnItemChanged(EditorItemId.TechItemId, this);
        }

        /// <summary>
        ///     Processing at the time of historical year change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechYearNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // If there is no change in the value, do nothing
            int year = (int) techYearNumericUpDown.Value;
            if (year == item.Year)
            {
                return;
            }

            Log.Info("[Tech] Changed tech year: {0} -> {1} [{2}]", item.Year, year, item);

            // Update the value
            item.Year = year;

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty(TechItemId.Year);

            // Change the character color
            techYearNumericUpDown.ForeColor = Color.Red;

            // Notify the renewal of the historical year of technical items
            HoI2EditorController.OnItemChanged(EditorItemId.TechItemYear, this);
        }

        /// <summary>
        ///     Technical coordinates list view selection items processing when changing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechPositionListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // If there is no selection item in the technical coordinates list, disable edited items
            if (techPositionListView.SelectedIndices.Count == 0)
            {
                DisableTechPositionItems();
                return;
            }

            // Update edit items
            TechPosition position = item.Positions[techPositionListView.SelectedIndices[0]];
            techXNumericUpDown.Value = position.X;
            techYNumericUpDown.Value = position.Y;

            // Update the color of the edit item
            techXNumericUpDown.ForeColor = position.IsDirty(TechPositionItemId.X) ? Color.Red : SystemColors.WindowText;
            techYNumericUpDown.ForeColor = position.IsDirty(TechPositionItemId.Y) ? Color.Red : SystemColors.WindowText;

            // Enable editing items
            EnableTechPositionItems();
        }

        /// <summary>
        ///     Technical coordinates list viewing of columns when changing width
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechPositionListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < PositionListColumnCount))
            {
                HoI2EditorController.Settings.TechEditor.TechPositionListColumnWidth[e.ColumnIndex] =
                    techPositionListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Technical coordinates list views Pre -editing processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechPositionListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // X
                    e.Type = ItemEditType.Text;
                    e.Text = techXNumericUpDown.Text;
                    break;

                case 1: // Y
                    e.Type = ItemEditType.Text;
                    e.Text = techYNumericUpDown.Text;
                    break;
            }
        }

        /// <summary>
        ///     Technical coordinates list viewing post -editing processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechPositionListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // X
                    techXNumericUpDown.Text = e.Text;
                    break;

                case 1: // Y
                    techYNumericUpDown.Text = e.Text;
                    break;
            }

            // We will update the list view items on our own, so we will treat them as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Technical coordinates list viewing items when replacing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechPositionListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;

            // Move technical coordinates
            TechPosition position = item.Positions[srcIndex];
            item.Positions.Insert(destIndex, position);
            if (srcIndex < destIndex)
            {
                item.Positions.RemoveAt(srcIndex);
            }
            else
            {
                item.Positions.RemoveAt(srcIndex + 1);
            }

            Log.Info("[Tech] Move tech position: {0} -> {1} ({2}, {3}) [{4}]", srcIndex, destIndex, position.X,
                position.Y, item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
        }

        /// <summary>
        ///     Technical X processing when coordinates change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechXNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            if (techPositionListView.SelectedIndices.Count == 0)
            {
                return;
            }

            int index = techPositionListView.SelectedIndices[0];
            TechPosition position = item.Positions[index];

            // If there is no change in the value, do nothing
            int x = (int) techXNumericUpDown.Value;
            if (x == position.X)
            {
                return;
            }

            Log.Info("[Tech] Changed tech position: ({0},{1}) -> ({2},{1}) [{3}]", position.X, position.Y, x, item);

            // Update the value
            position.X = x;

            // Update the display of coordinates list views
            techPositionListView.Items[index].Text = IntHelper.ToString(x);

            // Update the position of the label
            _techTreePanelController.UpdateItem(item, position);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            position.SetDirty(TechPositionItemId.X);

            // Change the character color
            techXNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Technical Y coordinates processing when changing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechYNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            if (techPositionListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = techPositionListView.SelectedIndices[0];
            TechPosition position = item.Positions[techPositionListView.SelectedIndices[0]];

            // If there is no change in the value, do nothing
            int y = (int) techYNumericUpDown.Value;
            if (y == position.Y)
            {
                return;
            }

            Log.Info("[Tech] Changed tech position: ({0},{1}) -> ({0},{2}) [{3}]", position.X, position.Y, y, item);

            // Update the value
            position.Y = y;

            // Update the display of coordinates list views
            techPositionListView.Items[index].SubItems[1].Text = IntHelper.ToString(y);

            // Update the position of the label
            _techTreePanelController.UpdateItem(item, position);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            position.SetDirty(TechPositionItemId.Y);

            // Change the character color
            techYNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Technical coordinates additional button processing processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechPositionAddButtonClick(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Add coordinates to the list
            TechPosition position = new TechPosition { X = 0, Y = 0 };
            item.Positions.Add(position);

            Log.Info("[Tech] Added tech position: ({0}, {1}) [{2}]", position.X, position.Y, item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            position.SetDirtyAll();

            // Add the coordinates list view items
            ListViewItem li = new ListViewItem { Text = IntHelper.ToString(position.X) };
            li.SubItems.Add(IntHelper.ToString(position.Y));
            techPositionListView.Items.Add(li);

            // Select the added item
            techPositionListView.Items[techPositionListView.Items.Count - 1].Focused = true;
            techPositionListView.Items[techPositionListView.Items.Count - 1].Selected = true;
            techPositionListView.EnsureVisible(techPositionListView.Items.Count - 1);

            // Enable editing items
            EnableTechPositionItems();

            // Add a label to the technical tree
            _techTreePanelController.AddItem(item, position);
        }

        /// <summary>
        ///     Technical coordinates removal button processing processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechPositionRemoveButtonClick(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            if (techPositionListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = techPositionListView.SelectedIndices[0];
            TechPosition position = item.Positions[index];

            Log.Info("[Tech] Removed tech position: ({0}, {1}) [{2}]", position.X, position.Y, item);

            // Delete the coordinates from the list
            item.Positions.RemoveAt(index);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();

            // Delete the items in the coordinates list view
            techPositionListView.Items.RemoveAt(index);

            if (index < techPositionListView.Items.Count)
            {
                // Select the following items in the deleted item
                techPositionListView.Items[index].Focused = true;
                techPositionListView.Items[index].Selected = true;
            }
            else if (index > 0)
            {
                // At the end of the list, select the item in front of the deleted item.
                techPositionListView.Items[techPositionListView.Items.Count - 1].Focused = true;
                techPositionListView.Items[techPositionListView.Items.Count - 1].Selected = true;
            }
            else
            {
                // If the items are gone, disable edited items
                DisableTechPositionItems();
            }

            // Delete the label from the technical tree
            _techTreePanelController.RemoveItem(item, position);
        }

        /// <summary>
        ///     Update technical images
        /// </summary>
        /// <param name="item">Technical application</param>
        private void UpdateTechPicture(TechItem item)
        {
            // Image File Name Update the value of the text box
            if (Game.Type == GameType.DarkestHour)
            {
                techPictureNameTextBox.Text = item.PictureName ?? "";
            }

            // Update the color of the edit item
            techPictureNameTextBox.ForeColor = item.IsDirty(TechItemId.PictureName)
                ? Color.Red
                : SystemColors.WindowText;

            Image prev = techPictureBox.Image;
            string name = !string.IsNullOrEmpty(item.PictureName) &&
                          (item.PictureName.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                ? item.PictureName
                : IntHelper.ToString(item.Id);
            string fileName = Game.GetReadFileName(Game.TechPicturePathName, $"{name}.bmp");
            if (File.Exists(fileName))
            {
                // Update technical images
                Bitmap bitmap = new Bitmap(fileName);
                bitmap.MakeTransparent();
                techPictureBox.Image = bitmap;
            }
            else
            {
                techPictureBox.Image = null;
            }
            prev?.Dispose();
        }

        /// <summary>
        ///     Processing when changing the image file name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechPictureNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // If there is no change in the value, do nothing
            string pictureName = techPictureNameTextBox.Text;
            if (pictureName.Equals(item.PictureName))
            {
                return;
            }

            Log.Info("[Tech] Changed tech picture: {0} -> {1} [{2}]", item.PictureName, pictureName, item);

            // Update the value
            item.PictureName = pictureName;

            // Update technical images
            UpdateTechPicture(item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty(TechItemId.PictureName);

            // Change the character color
            techPictureNameTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Image file name Referral processing when pressing button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechPictureNameBrowseButtonClick(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = Path.Combine(Game.FolderName, Game.TechPicturePathName),
                FileName = item.PictureName,
                Filter = Resources.OpenBitmapFileDialogFilter
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Image File Name Update the value of the text box
                techPictureNameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }

        #endregion

        #region Required technology tab

        /// <summary>
        ///     Update the required technical tab items
        /// </summary>
        /// <param name="item">Technical application</param>
        private void UpdateRequiredItems(TechItem item)
        {
            UpdateAndRequiredList(item);
            UpdateOrRequiredList(item);
        }

        /// <summary>
        ///     Enable the required technology tab
        /// </summary>
        private void EnableRequiredTab()
        {
            // Enable the tab
            editTabControl.TabPages[(int) TechEditorTab.Required].Enabled = true;
        }

        /// <summary>
        ///     Disable the required technology tab
        /// </summary>
        private void DisableRequiredTab()
        {
            // Disable the tab
            editTabControl.TabPages[(int) TechEditorTab.Required].Enabled = false;

            // Clear the required technical list
            andRequiredListView.Items.Clear();
            orRequiredListView.Items.Clear();

            // Disable edit items
            DisableAndRequiredItems();
            DisableOrRequiredItems();
        }

        /// <summary>
        ///     Update the technical list of the required technology tab
        /// </summary>
        private void UpdateRequiredTechListItems()
        {
            Graphics g = Graphics.FromHwnd(Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;

            andTechComboBox.BeginUpdate();
            orTechComboBox.BeginUpdate();

            andTechComboBox.Items.Clear();
            orTechComboBox.Items.Clear();

            int width = andTechComboBox.Width;
            foreach (TechItem item in Techs.TechIdMap.Select(pair => pair.Value))
            {
                andTechComboBox.Items.Add(item);
                orTechComboBox.Items.Add(item);
                width = Math.Max(width,
                    (int) g.MeasureString(item.ToString(), andTechComboBox.Font).Width +
                    SystemInformation.VerticalScrollBarWidth + margin);
            }
            andTechComboBox.DropDownWidth = width;
            orTechComboBox.DropDownWidth = width;

            andTechComboBox.EndUpdate();
            orTechComboBox.EndUpdate();
        }

        /// <summary>
        ///     Update the required technical list
        /// </summary>
        /// <param name="item">Technical application</param>
        private void UpdateAndRequiredList(TechItem item)
        {
            andRequiredListView.BeginUpdate();
            andRequiredListView.Items.Clear();

            foreach (int id in item.AndRequiredTechs.Select(tech => tech.Id))
            {
                ListViewItem li = new ListViewItem { Text = IntHelper.ToString(id) };
                if (Techs.TechIdMap.ContainsKey(id))
                {
                    li.SubItems.Add(Techs.TechIdMap[id].ToString());
                }
                andRequiredListView.Items.Add(li);
            }

            if (andRequiredListView.Items.Count > 0)
            {
                // Select the first item
                andRequiredListView.Items[0].Focused = true;
                andRequiredListView.Items[0].Selected = true;

                // Enable editing items
                EnableAndRequiredItems();
            }
            else
            {
                // Disable edit items
                DisableAndRequiredItems();
            }

            andRequiredListView.EndUpdate();
        }


        /// <summary>
        ///     OR Condition Update the Technical List
        /// </summary>
        /// <param name="item">Technical application</param>
        private void UpdateOrRequiredList(TechItem item)
        {
            orRequiredListView.BeginUpdate();
            orRequiredListView.Items.Clear();

            foreach (int id in item.OrRequiredTechs.Select(tech => tech.Id))
            {
                ListViewItem li = new ListViewItem { Text = IntHelper.ToString(id) };
                if (Techs.TechIdMap.ContainsKey(id))
                {
                    li.SubItems.Add(Techs.TechIdMap[id].ToString());
                }
                orRequiredListView.Items.Add(li);
            }

            if (orRequiredListView.Items.Count > 0)
            {
                // Select the first item
                orRequiredListView.Items[0].Focused = true;
                orRequiredListView.Items[0].Selected = true;

                // Enable editing items
                EnableOrRequiredItems();
            }
            else
            {
                // Disable edit items
                DisableOrRequiredItems();
            }

            orRequiredListView.EndUpdate();
        }

        /// <summary>
        ///     Enable editing items for and requirements for and conditions
        /// </summary>
        private void EnableAndRequiredItems()
        {
            // Reset the clear value when disabling
            andIdNumericUpDown.Text = IntHelper.ToString((int) andIdNumericUpDown.Value);

            // Enable editing items
            andIdNumericUpDown.Enabled = true;
            andTechComboBox.Enabled = true;

            andRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     Disable editing items for and requirements for and conditions
        /// </summary>
        private void DisableAndRequiredItems()
        {
            // Clear the value of edit items
            andIdNumericUpDown.ResetText();
            andTechComboBox.SelectedIndex = -1;
            andTechComboBox.ResetText();

            // Disable edit items
            andIdNumericUpDown.Enabled = false;
            andTechComboBox.Enabled = false;

            andRemoveButton.Enabled = false;
        }

        /// <summary>
        ///     OR Conditions Enable editing items for technology
        /// </summary>
        private void EnableOrRequiredItems()
        {
            // Reset the clear value when disabling
            orIdNumericUpDown.Text = IntHelper.ToString((int) orIdNumericUpDown.Value);

            // Enable editing items
            orIdNumericUpDown.Enabled = true;
            orTechComboBox.Enabled = true;

            orRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     Orcare the editing items of requirements for conditions are disabled
        /// </summary>
        private void DisableOrRequiredItems()
        {
            // Clear the value of edit items
            orIdNumericUpDown.ResetText();
            orTechComboBox.SelectedIndex = -1;
            orTechComboBox.ResetText();

            // Disable edit items
            orIdNumericUpDown.Enabled = false;
            orTechComboBox.Enabled = false;

            orRemoveButton.Enabled = false;
        }

        /// <summary>
        ///     AND Conditions required Technology Combo Box item drawing process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAndTechComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // If there are no items, do nothing
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw the character string of the item
            TechItem item = GetSelectedItem() as TechItem;
            if (item != null && andRequiredListView.SelectedIndices.Count > 0)
            {
                RequiredTech tech = item.AndRequiredTechs[andRequiredListView.SelectedIndices[0]];
                Brush brush;
                if ((Techs.TechIds[e.Index] == tech.Id) && tech.IsDirty())
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = andTechComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     OR Conditions required Technical Combo Box item drawing process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrTechComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // If there are no items, do nothing
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw the character string of the item
            TechItem item = GetSelectedItem() as TechItem;
            if (item != null && orRequiredListView.SelectedIndices.Count > 0)
            {
                RequiredTech tech = item.OrRequiredTechs[orRequiredListView.SelectedIndices[0]];
                Brush brush;
                if ((Techs.TechIds[e.Index] == tech.Id) && tech.IsDirty())
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = orTechComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     AND Conditions required Technical List Selection item processing when changing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAndRequiredListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // If there is no selection item in the technical item list, do nothing
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // If there is no selection item in the required technical list, disable edited items
            if (andRequiredListView.SelectedIndices.Count == 0)
            {
                DisableAndRequiredItems();
                return;
            }

            // Update edit items
            RequiredTech tech = item.AndRequiredTechs[andRequiredListView.SelectedIndices[0]];
            int id = tech.Id;
            andIdNumericUpDown.Value = id;

            // Update the color of the combo box
            andTechComboBox.Refresh();

            // Update the color of the edit item
            andIdNumericUpDown.ForeColor = tech.IsDirty() ? Color.Red : SystemColors.WindowText;

            // Update the selection item of the AND condition required technology combo box
            if (Techs.TechIds.Contains(id))
            {
                andTechComboBox.SelectedIndex = Techs.TechIds.IndexOf(id);
            }
            else
            {
                andTechComboBox.SelectedIndex = -1;
                andTechComboBox.ResetText();
            }

            // Enable editing items
            EnableAndRequiredItems();
        }

        /// <summary>
        ///     OR Conditional requirement Technical list processing when changing items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrRequiredListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // If there is no selection item in the technical item list, do nothing
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // If there is no selection item in the required technical list, disable edited items
            if (orRequiredListView.SelectedIndices.Count == 0)
            {
                DisableOrRequiredItems();
                return;
            }

            // Update edit items
            RequiredTech tech = item.OrRequiredTechs[orRequiredListView.SelectedIndices[0]];
            int id = tech.Id;
            orIdNumericUpDown.Value = id;

            // Update the color of the combo box
            orTechComboBox.Refresh();

            // Update the color of the edit item
            orIdNumericUpDown.ForeColor = tech.IsDirty() ? Color.Red : SystemColors.WindowText;

            // OR Conditions The selection item of the technical combo box is updated
            if (Techs.TechIds.Contains(id))
            {
                orTechComboBox.SelectedIndex = Techs.TechIds.IndexOf(id);
            }
            else
            {
                orTechComboBox.SelectedIndex = -1;
                orTechComboBox.ResetText();
            }

            // Enable editing items
            EnableOrRequiredItems();
        }

        /// <summary>
        ///     AND Conditions required Technical list processing when changing width
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAndRequiredListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < RequiredListColumnCount))
            {
                HoI2EditorController.Settings.TechEditor.AndRequiredListColumnWidth[e.ColumnIndex] =
                    andRequiredListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     OR Conditional requirements Technical list processing when changing width
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrRequiredListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < RequiredListColumnCount))
            {
                HoI2EditorController.Settings.TechEditor.OrRequiredListColumnWidth[e.ColumnIndex] =
                    orRequiredListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     AND Conditions required Technical List View Item Pre-editing processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAndRequiredListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // ID
                    e.Type = ItemEditType.Text;
                    e.Text = andIdNumericUpDown.Text;
                    break;

                case 1: // name
                    e.Type = ItemEditType.List;
                    e.Items = andTechComboBox.Items.Cast<string>();
                    e.Index = andTechComboBox.SelectedIndex;
                    e.DropDownWidth = andTechComboBox.DropDownWidth;
                    break;
            }
        }

        /// <summary>
        ///     AND Conditions required Technical List View Items after editing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAndRequiredListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // ID
                    andIdNumericUpDown.Text = e.Text;
                    break;

                case 1: // name
                    andTechComboBox.SelectedIndex = e.Index;
                    break;
            }

            // We will update the list view items on our own, so we will treat them as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     OR Conditions Need Technical List View Item Pre-editing processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrRequiredListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // ID
                    e.Type = ItemEditType.Text;
                    e.Text = orIdNumericUpDown.Text;
                    break;

                case 1: // name
                    e.Type = ItemEditType.List;
                    e.Items = orTechComboBox.Items.Cast<string>();
                    e.Index = orTechComboBox.SelectedIndex;
                    e.DropDownWidth = orTechComboBox.DropDownWidth;
                    break;
            }
        }

        /// <summary>
        ///     OR Conditions required Technical List View Items after editing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrRequiredListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // ID
                    orIdNumericUpDown.Text = e.Text;
                    break;

                case 1: // name
                    orTechComboBox.SelectedIndex = e.Index;
                    break;
            }

            // We will update the list view items on our own, so we will treat them as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     AND Conditions required Technical List of Technical Listing Items when replacing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAndRequiredListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;

            // Move necessary technology
            RequiredTech tech = item.AndRequiredTechs[srcIndex];
            item.AndRequiredTechs.Insert(destIndex, tech);
            if (srcIndex < destIndex)
            {
                item.AndRequiredTechs.RemoveAt(srcIndex);
            }
            else
            {
                item.AndRequiredTechs.RemoveAt(srcIndex + 1);
            }

            Log.Info("[Tech] Move and required tech: {0} -> {1} {2} [{3}]", srcIndex, destIndex, tech.Id, item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            tech.SetDirty();
        }

        /// <summary>
        ///     OR Conditional requirement Technical List of Technical List View Processing when replacing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrRequiredListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;

            // Move necessary technology
            RequiredTech tech = item.OrRequiredTechs[srcIndex];
            item.OrRequiredTechs.Insert(destIndex, tech);
            if (srcIndex < destIndex)
            {
                item.OrRequiredTechs.RemoveAt(srcIndex);
            }
            else
            {
                item.OrRequiredTechs.RemoveAt(srcIndex + 1);
            }

            Log.Info("[Tech] Move or required tech: {0} -> {1} {2} [{3}]", srcIndex, destIndex, tech.Id, item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            tech.SetDirty();
        }

        /// <summary>
        ///     AND condition requirement Technology Additional button processing processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAndAddButtonClick(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Add item to AND conditions required technical list
            RequiredTech tech = new RequiredTech();
            item.AndRequiredTechs.Add(tech);

            Log.Info("[Tech] Added and required tech: {0} [{1}]", tech.Id, item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            tech.SetDirty();

            // Added items to AND conditions required technology list views
            AddAndRequiredListItem(0);
        }

        /// <summary>
        ///     OR Conditional requirement Technology Additional button processing when pressing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrAddButtonClick(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Or Conditional requirements Add item to the technical list
            RequiredTech tech = new RequiredTech();
            item.OrRequiredTechs.Add(tech);

            Log.Info("[Tech] Added or required tech: {0} [{1}]", tech.Id, item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            tech.SetDirty();

            // OR Conditions Necessary Technical Technology Add Items to Wrist View
            AddOrRequiredListItem(0);
        }

        /// <summary>
        ///     AND Conditions required Technology Deletion Treatment when pressing button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAndRemoveButtonClick(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            if (andRequiredListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = andRequiredListView.SelectedIndices[0];

            Log.Info("[Tech] Removed and required tech: {0} [{1}]", item.AndRequiredTechs[index].Id, item);

            // Delete items from the required technical list
            RemoveAndRequiredListItem(index);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();

            // AND Conditions Delete items from the Technical List View
            item.AndRequiredTechs.RemoveAt(index);
        }

        /// <summary>
        ///     OR Conditional requirement Technical deletion processing when pressing button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrRemoveButtonClick(object sender, EventArgs e)
        {
            // If you don't have a selection item, you won't do anything
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            if (orRequiredListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = orRequiredListView.SelectedIndices[0];

            Log.Info("[Tech] Removed or required tech: {0} [{1}]", item.OrRequiredTechs[index].Id, item);

            // OR Conditions Delete items from the Technical List
            RemoveOrRequiredListItem(index);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();

            // OR Conditions Delete items from the Technical List View
            item.OrRequiredTechs.RemoveAt(index);
        }

        /// <summary>
        ///     AND Conditions required Technical Technology ID
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAndIdNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            if (andRequiredListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = andRequiredListView.SelectedIndices[0];

            // Do nothing if the value does not change
            RequiredTech tech = item.AndRequiredTechs[index];
            int id = (int) andIdNumericUpDown.Value;
            if (id == tech.Id)
            {
                return;
            }

            Log.Info("[Tech] Changed and required tech: {0} -> {1} [{2}]", tech.Id, id, item);

            // Update value
            tech.Id = id;

            // AND AND Condition Required technology Update the selection items in the combo box
            if (Techs.TechIds.Contains(id))
            {
                andTechComboBox.SelectedIndex = Techs.TechIds.IndexOf(id);
            }
            else
            {
                andTechComboBox.SelectedIndex = -1;
                andTechComboBox.ResetText();
            }

            // AND AND Update the items in the condition required technology list
            ModifyAndRequiredListItem(id, index);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            tech.SetDirty();

            // Change the font color
            andIdNumericUpDown.ForeColor = Color.Red;

            // AND ANDCondition Required technology Update drawing to change the item color of the combo box
            andTechComboBox.Refresh();
        }

        /// <summary>
        ///     OR Condition Required technology ID Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrIdNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            if (orRequiredListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = orRequiredListView.SelectedIndices[0];

            // Do nothing if the value does not change
            RequiredTech tech = item.OrRequiredTechs[index];
            int id = (int) orIdNumericUpDown.Value;
            if (id == tech.Id)
            {
                return;
            }

            Log.Info("[Tech] Changed or required tech: {0} -> {1} [{2}]", tech.Id, id, item);

            // Update value
            tech.Id = id;

            // OR Condition Required technology Update the selection items in the combo box
            orTechComboBox.SelectedIndex = -1;
            if (Techs.TechIds.Contains(id))
            {
                orTechComboBox.SelectedIndex = Techs.TechIds.IndexOf(id);
            }
            else
            {
                orTechComboBox.SelectedIndex = -1;
                orTechComboBox.ResetText();
            }

            // OR Update the items in the condition required technology list
            ModifyOrRequiredListItem(id, index);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            tech.SetDirty();

            // Change the font color
            orIdNumericUpDown.ForeColor = Color.Red;

            // OR Condition Required Technique Update drawing to change the item color of the combo box
            orTechComboBox.Refresh();
        }

        /// <summary>
        ///     AND AND Processing when condition required technology change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAndTechComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            if (andRequiredListView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Do nothing if the value does not change
            int index = andRequiredListView.SelectedIndices[0];
            if (index == -1)
            {
                return;
            }
            RequiredTech tech = item.AndRequiredTechs[index];
            if (andTechComboBox.SelectedIndex == -1)
            {
                return;
            }
            int id = Techs.TechIds[andTechComboBox.SelectedIndex];
            if (id == tech.Id)
            {
                return;
            }

            // AND AND Condition required technology ID Update the value of
            andIdNumericUpDown.Value = id;
        }

        /// <summary>
        ///     OR Processing when condition required technology change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrTechComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            if (orRequiredListView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Do nothing if the value does not change
            int index = orRequiredListView.SelectedIndices[0];
            if (index == -1)
            {
                return;
            }
            RequiredTech tech = item.OrRequiredTechs[index];
            if (orTechComboBox.SelectedIndex == -1)
            {
                return;
            }
            int id = Techs.TechIds[orTechComboBox.SelectedIndex];
            if (id == tech.Id)
            {
                return;
            }

            // OR Condition required technology ID Update the value of
            orIdNumericUpDown.Value = id;
        }

        /// <summary>
        ///     AND AND Condition Required technology Add an item to the list view
        /// </summary>
        /// <param name="id">Required technology ID</param>
        private void AddAndRequiredListItem(int id)
        {
            // Add an item to the list
            ListViewItem li = new ListViewItem { Text = IntHelper.ToString(id) };
            if (Techs.TechIdMap.ContainsKey(id))
            {
                li.SubItems.Add(Techs.TechIdMap[id].ToString());
            }
            andRequiredListView.Items.Add(li);

            // Select the added item
            int index = andRequiredListView.Items.Count - 1;
            andRequiredListView.Items[index].Focused = true;
            andRequiredListView.Items[index].Selected = true;
            andRequiredListView.EnsureVisible(index);

            // Enable edit items
            EnableAndRequiredItems();
        }

        /// <summary>
        ///     OR Condition Required technology Add an item to the list view
        /// </summary>
        /// <param name="id">Required technology ID</param>
        private void AddOrRequiredListItem(int id)
        {
            // Add an item to the list
            ListViewItem li = new ListViewItem { Text = IntHelper.ToString(id) };
            if (Techs.TechIdMap.ContainsKey(id))
            {
                li.SubItems.Add(Techs.TechIdMap[id].ToString());
            }
            orRequiredListView.Items.Add(li);

            // Select the added item
            int index = orRequiredListView.Items.Count - 1;
            orRequiredListView.Items[index].Focused = true;
            orRequiredListView.Items[index].Selected = true;
            orRequiredListView.EnsureVisible(index);

            // Enable edit items
            EnableOrRequiredItems();
        }

        /// <summary>
        ///     AND AND Change the items in the condition required technology list
        /// </summary>
        /// <param name="id">Required technology ID</param>
        /// <param name="index">Item index to be changed</param>
        private void ModifyAndRequiredListItem(int id, int index)
        {
            andRequiredListView.Items[index].SubItems.Clear();
            andRequiredListView.Items[index].Text = IntHelper.ToString(id);
            if (Techs.TechIdMap.ContainsKey(id))
            {
                andRequiredListView.Items[index].SubItems.Add(Techs.TechIdMap[id].ToString());
            }
        }

        /// <summary>
        ///     OR Change the items in the condition required technology list
        /// </summary>
        /// <param name="id">Required technology ID</param>
        /// <param name="index">Item index to be changed</param>
        private void ModifyOrRequiredListItem(int id, int index)
        {
            orRequiredListView.Items[index].SubItems.Clear();
            orRequiredListView.Items[index].Text = IntHelper.ToString(id);
            if (Techs.TechIdMap.ContainsKey(id))
            {
                orRequiredListView.Items[index].SubItems.Add(Techs.TechIdMap[id].ToString());
            }
        }

        /// <summary>
        ///     AND AND Delete the item of the condition required technology list
        /// </summary>
        /// <param name="index">Item index to be deleted</param>
        private void RemoveAndRequiredListItem(int index)
        {
            // Remove an item from the list
            andRequiredListView.Items[index].Remove();

            if (index < andRequiredListView.Items.Count)
            {
                // Select the next item after the deleted item
                andRequiredListView.Items[index].Focused = true;
                andRequiredListView.Items[index].Selected = true;
            }
            else if (index > 0)
            {
                // At the end of the list, select the item before the deleted item
                andRequiredListView.Items[andRequiredListView.Items.Count - 1].Focused = true;
                andRequiredListView.Items[andRequiredListView.Items.Count - 1].Selected = true;
            }
            else
            {
                // Disable edit items when there are no more items
                DisableAndRequiredItems();
            }
        }

        /// <summary>
        ///     OR Delete the item in the condition required technology list
        /// </summary>
        /// <param name="index">Item index to be deleted</param>
        private void RemoveOrRequiredListItem(int index)
        {
            // Remove an item from the list
            orRequiredListView.Items[index].Remove();

            if (index < orRequiredListView.Items.Count)
            {
                // Select the next item after the deleted item
                orRequiredListView.Items[index].Focused = true;
                orRequiredListView.Items[index].Selected = true;
            }
            else if (index > 0)
            {
                // At the end of the list, select the item before the deleted item
                orRequiredListView.Items[orRequiredListView.Items.Count - 1].Focused = true;
                orRequiredListView.Items[orRequiredListView.Items.Count - 1].Selected = true;
            }
            else
            {
                // Disable edit items when there are no more items
                DisableOrRequiredItems();
            }
        }

        #endregion

        #region Small study tab

        /// <summary>
        ///     Initialize the items on the scholarship tab
        /// </summary>
        private void InitComponentItems()
        {
            // Small research characteristics
            componentSpecialityComboBox.BeginUpdate();
            componentSpecialityComboBox.Items.Clear();
            Graphics g = Graphics.FromHwnd(componentSpecialityComboBox.Handle);
            int width = componentSpecialityComboBox.Width;
            int additional = SystemInformation.VerticalScrollBarWidth + DeviceCaps.GetScaledWidth(16) + 3;
            foreach (string name in Techs.Specialities
                .Where(speciality => speciality != TechSpeciality.None)
                .Select(Techs.GetSpecialityName))
            {
                componentSpecialityComboBox.Items.Add(name);
                width = Math.Max(width,
                    (int) g.MeasureString(name, componentSpecialityComboBox.Font).Width + additional);
            }
            componentSpecialityComboBox.DropDownWidth = width;
            componentSpecialityComboBox.EndUpdate();
        }

        /// <summary>
        ///     Update the items on the scholarship tab
        /// </summary>
        /// <param name="item">Technology</param>
        private void UpdateComponentItems(TechItem item)
        {
            componentListView.BeginUpdate();
            componentListView.Items.Clear();

            foreach (TechComponent component in item.Components)
            {
                ListViewItem listItem = CreateComponentListItem(component);

                componentListView.Items.Add(listItem);
            }

            if (componentListView.Items.Count > 0)
            {
                // Select the first item
                componentListView.Items[0].Focused = true;
                componentListView.Items[0].Selected = true;

                // Enable edit items
                EnableComponentItems();
            }
            else
            {
                // Disable edit items
                DisableComponentItems();
            }

            componentListView.EndUpdate();
        }

        /// <summary>
        ///     Enable the scholarship tab
        /// </summary>
        private void EnableComponentTab()
        {
            // Enable tabs
            editTabControl.TabPages[(int) TechEditorTab.Component].Enabled = true;
        }

        /// <summary>
        ///     Disable the scholarship tab
        /// </summary>
        private void DisableComponentTab()
        {
            // Disable tabs
            editTabControl.TabPages[(int) TechEditorTab.Component].Enabled = false;

            // Clear the items in the scholarship list
            componentListView.Items.Clear();

            // Disable edit items
            DisableComponentItems();
        }

        /// <summary>
        ///     Enable edit items on the scholarship tab
        /// </summary>
        private void EnableComponentItems()
        {
            // Reset the value cleared at the time of invalidation
            componentIdNumericUpDown.Text = IntHelper.ToString((int) componentIdNumericUpDown.Value);
            componentDifficultyNumericUpDown.Text = IntHelper.ToString((int) componentDifficultyNumericUpDown.Value);

            // Enable edit items
            componentIdNumericUpDown.Enabled = true;
            componentNameTextBox.Enabled = true;
            componentSpecialityComboBox.Enabled = true;
            componentDifficultyNumericUpDown.Enabled = true;
            componentDoubleTimeCheckBox.Enabled = true;

            componentCloneButton.Enabled = true;
            componentRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items on the scholarship tab
        /// </summary>
        private void DisableComponentItems()
        {
            // Clear the value of the edit item
            componentIdNumericUpDown.ResetText();
            componentNameTextBox.ResetText();
            componentSpecialityComboBox.SelectedIndex = -1;
            componentSpecialityComboBox.ResetText();
            componentDifficultyNumericUpDown.ResetText();
            componentDoubleTimeCheckBox.Checked = false;

            // Disable edit items
            componentIdNumericUpDown.Enabled = false;
            componentNameTextBox.Enabled = false;
            componentSpecialityComboBox.Enabled = false;
            componentDifficultyNumericUpDown.Enabled = false;
            componentDoubleTimeCheckBox.Enabled = false;

            componentCloneButton.Enabled = false;
            componentRemoveButton.Enabled = false;
            componentUpButton.Enabled = false;
            componentDownButton.Enabled = false;
        }

        /// <summary>
        ///     Processing when changing the selection item in the essay list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Disable edit items if there are no selection items in the scholarship list
            if (componentListView.SelectedIndices.Count == 0)
            {
                DisableComponentItems();
                return;
            }
            int index = componentListView.SelectedIndices[0];

            TechComponent component = item.Components[index];
            if (component == null)
            {
                return;
            }

            // Update the value of the edit item
            componentIdNumericUpDown.Value = component.Id;
            componentNameTextBox.Text = component.ToString();
            componentSpecialityComboBox.SelectedIndex = Array.IndexOf(Techs.Specialities, component.Speciality) - 1;
            componentDifficultyNumericUpDown.Value = component.Difficulty;
            componentDoubleTimeCheckBox.Checked = component.DoubleTime;

            // Update the color of the combo box
            componentSpecialityComboBox.Refresh();

            // Update the color of the edit item
            componentIdNumericUpDown.ForeColor = component.IsDirty(TechComponentItemId.Id)
                ? Color.Red
                : SystemColors.WindowText;
            componentNameTextBox.ForeColor = component.IsDirty(TechComponentItemId.Name)
                ? Color.Red
                : SystemColors.WindowText;
            componentDifficultyNumericUpDown.ForeColor = component.IsDirty(TechComponentItemId.Difficulty)
                ? Color.Red
                : SystemColors.WindowText;
            componentDoubleTimeCheckBox.ForeColor = component.IsDirty(TechComponentItemId.DoubleTime)
                ? Color.Red
                : SystemColors.WindowText;

            // Enable edit items
            EnableComponentItems();

            componentUpButton.Enabled = index != 0;
            componentDownButton.Enabled = index != item.Components.Count - 1;
        }

        /// <summary>
        ///     Processing when changing the width of columns in the scholarship list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < ComponentListColumnCount))
            {
                HoI2EditorController.Settings.TechEditor.ComponentListColumnWidth[e.ColumnIndex] =
                    componentListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Processing before editing items in the scholarship list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // ID
                    e.Type = ItemEditType.Text;
                    e.Text = componentIdNumericUpDown.Text;
                    break;

                case 1: // Small research name
                    e.Type = ItemEditType.Text;
                    e.Text = componentNameTextBox.Text;
                    break;

                case 2: // Research characteristics
                    e.Type = ItemEditType.List;
                    e.Items = componentSpecialityComboBox.Items.Cast<string>();
                    e.Index = componentSpecialityComboBox.SelectedIndex;
                    e.DropDownWidth = componentSpecialityComboBox.DropDownWidth;
                    break;

                case 3: // difficulty
                    e.Type = ItemEditType.Text;
                    e.Text = componentDifficultyNumericUpDown.Text;
                    break;

                case 4: // 2 Double
                    e.Type = ItemEditType.Bool;
                    e.Flag = componentDoubleTimeCheckBox.Checked;
                    break;
            }
        }

        /// <summary>
        ///     Processing after editing items in the scholarship list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // ID
                    componentIdNumericUpDown.Text = e.Text;
                    break;

                case 1: // Small research name
                    componentNameTextBox.Text = e.Text;
                    break;

                case 2: // Research characteristics
                    componentSpecialityComboBox.SelectedIndex = e.Index;
                    break;

                case 3: // difficulty
                    componentDifficultyNumericUpDown.Text = e.Text;
                    break;

                case 4: // 2 Double
                    componentDoubleTimeCheckBox.Checked = e.Flag;
                    break;
            }

            // Since the items in the list view will be updated by yourself, it will be treated as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Processing when replacing items in the scholarship list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // Do nothing if there is no selection
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;

            // Move essays
            TechComponent component = item.Components[srcIndex];
            item.Components.Insert(destIndex, component);
            if (srcIndex < destIndex)
            {
                item.Components.RemoveAt(srcIndex);
            }
            else
            {
                item.Components.RemoveAt(srcIndex + 1);
            }

            Log.Info("[Tech] Move component: {0} -> {1} {2} [{3}]", srcIndex, destIndex, component.Id, item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            component.SetDirty();

            // Notify the update of the essay list
            HoI2EditorController.OnItemChanged(EditorItemId.TechComponentList, this);
        }

        /// <summary>
        ///     Item drawing process of the small research characteristic combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentSpecialityComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a research characteristic icon
            int iconWidth = DeviceCaps.GetScaledWidth(16);
            int iconHeight = DeviceCaps.GetScaledHeight(16);
            if (e.Index < Techs.SpecialityImages.Images.Count &&
                !string.IsNullOrEmpty(componentSpecialityComboBox.Items[e.Index].ToString()))
            {
                e.Graphics.DrawImage(Techs.SpecialityImages.Images[e.Index],
                    new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, iconWidth, iconHeight));
            }

            // Draw a string of items
            TechItem item = GetSelectedItem() as TechItem;
            if (item != null && componentListView.SelectedIndices.Count > 0)
            {
                TechComponent component = item.Components[componentListView.SelectedIndices[0]];
                Brush brush;
                if ((Techs.Specialities[e.Index + 1] == component.Speciality) &&
                    component.IsDirty(TechComponentItemId.Specilaity))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                e.Graphics.DrawString(
                    componentSpecialityComboBox.Items[e.Index].ToString(), e.Font, brush,
                    new Rectangle(e.Bounds.X + iconWidth + 3, e.Bounds.Y + 3, e.Bounds.Width - iconHeight - 3,
                        e.Bounds.Height));
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when a new button is pressed in a small study
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentNewButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            TechComponent component = TechComponent.Create();

            // Register in the duplicate character string list
            Techs.IncrementDuplicatedListCount(component.Name);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            component.SetDirtyAll();

            if (componentListView.SelectedIndices.Count > 0)
            {
                int index = componentListView.SelectedIndices[0];
                TechComponent selected = item.Components[index];
                component.Id = item.GetNewComponentId(selected.Id + 1);

                // Insert an item into a list
                item.InsertComponent(component, index + 1);

                // Insert an item in the scholarship list view
                InsertComponentListItem(component, index + 1);
            }
            else
            {
                component.Id = item.GetNewComponentId(item.Id + 1);

                // Add an item to the list
                item.AddComponent(component);

                // Add an item to the scholarship list view
                AddComponentListItem(component);
            }

            // Notify the update of the essay list
            HoI2EditorController.OnItemChanged(EditorItemId.TechComponentList, this);

            Log.Info("[Tech] Added new tech component: {0} [{1}]", component.Id, item);
        }

        /// <summary>
        ///     Processing when the duplicate button of the essay is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentCloneButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the scholarship list
            if (componentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = componentListView.SelectedIndices[0];

            TechComponent selected = item.Components[index];
            TechComponent component = selected.Clone();
            component.Id = item.GetNewComponentId(selected.Id);

            Log.Info("[Tech] Added new tech component: {0} [{1}]", component.Id, item);

            // Register in the duplicate character string list
            Techs.IncrementDuplicatedListCount(component.Name);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            component.SetDirtyAll();

            // Insert an item into a list
            item.InsertComponent(component, index + 1);

            // Insert an item in the scholarship list view
            InsertComponentListItem(component, index + 1);

            // Notify the update of the essay list
            HoI2EditorController.OnItemChanged(EditorItemId.TechComponentList, this);
        }

        /// <summary>
        ///     Processing when the delete button of the essay is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the scholarship list
            if (componentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = componentListView.SelectedIndices[0];

            Log.Info("[Tech] Removed new tech component: {0} [{1}]", item.Components[index], item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();

            // Remove an item from the list
            item.RemoveComponent(index);

            // Remove an item from the scholarship list view
            RemoveComponentListItem(index);

            // Notify the update of the essay list
            HoI2EditorController.OnItemChanged(EditorItemId.TechComponentList, this);
        }

        /// <summary>
        ///     Processing when the button is pressed on the top of the essay
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentUpButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the scholarship list
            if (componentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = componentListView.SelectedIndices[0];

            // Do nothing at the top of the list
            if (index == 0)
            {
                return;
            }

            // Move items
            item.MoveComponent(index, index - 1);
            MoveComponentListItem(index, index - 1);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
        }

        /// <summary>
        ///     Processing when the button is pressed under the essay
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentDownButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the scholarship list
            if (componentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = componentListView.SelectedIndices[0];

            // Do nothing at the end of the list
            if (index == componentListView.Items.Count - 1)
            {
                return;
            }

            // Move items
            item.MoveComponent(index, index + 1);
            MoveComponentListItem(index, index + 1);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
        }

        /// <summary>
        ///     Small study ID Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentIdNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the scholarship list
            if (componentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = componentListView.SelectedIndices[0];

            TechComponent component = item.Components[index];
            if (component == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int id = (int) componentIdNumericUpDown.Value;
            if (id == component.Id)
            {
                return;
            }

            Log.Info("[Tech] Changed tech component id: {0} -> {1} [{2}]", component.Id, id, component);

            // Update value
            component.Id = id;

            // Update items in the scholarship list view
            componentListView.Items[index].Text = IntHelper.ToString(id);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            component.SetDirty(TechComponentItemId.Id);

            // Change the font color
            componentIdNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the name of a small research
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the scholarship list
            if (componentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = componentListView.SelectedIndices[0];

            TechComponent component = item.Components[index];
            if (component == null)
            {
                return;
            }

            // Do nothing if the value does not change
            string name = componentNameTextBox.Text;
            if (name.Equals(component.ToString()))
            {
                return;
            }

            // If it is a duplicate character string, reset the definition name
            if (Techs.IsDuplicatedName(component.Name))
            {
                Techs.DecrementDuplicatedListCount(component.Name);
                component.Name = Config.GetTempKey();
                Techs.IncrementDuplicatedListCount(component.Name);
            }

            Log.Info("[Tech] Changed tech component name: {0} -> {1} <{2}>", component, name, component.Name);

            // Update value
            Config.SetText(component.Name, name, Game.TechTextFileName);

            // Update items in the scholarship list view
            componentListView.Items[index].SubItems[1].Text = name;

            // Set the edited flag
            item.SetDirty();
            component.SetDirty(TechComponentItemId.Name);

            // Change the font color
            componentNameTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the characteristics of a small study
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentSpecialityComboBoxSelectionChangeCommitted(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the scholarship list
            if (componentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = componentListView.SelectedIndices[0];

            TechComponent component = item.Components[index];
            if (component == null)
            {
                return;
            }

            // Do nothing if the value does not change
            TechSpeciality speciality = Techs.Specialities[componentSpecialityComboBox.SelectedIndex + 1];
            if (speciality == component.Speciality)
            {
                return;
            }

            Log.Info("[Tech] Changed tech component speciality: {0} -> {1} [{2}]",
                Techs.GetSpecialityName(component.Speciality), Techs.GetSpecialityName(speciality), component);

            // Update value
            component.Speciality = speciality;

            // Update items in the scholarship list view
            componentListView.Items[index].SubItems[2].Text = Techs.GetSpecialityName(speciality);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            component.SetDirty(TechComponentItemId.Specilaity);

            // Update drawing to change the item color of the essay characteristic combo box
            componentSpecialityComboBox.Refresh();

            // Notify updates of scholarship characteristics
            HoI2EditorController.OnItemChanged(EditorItemId.TechComponentSpeciality, this);
        }

        /// <summary>
        ///     Processing when changing the difficulty level of a small study
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentDifficultyNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the scholarship list
            if (componentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = componentListView.SelectedIndices[0];

            TechComponent component = item.Components[index];
            if (component == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int difficulty = (int) componentDifficultyNumericUpDown.Value;
            if (difficulty == component.Difficulty)
            {
                return;
            }

            Log.Info("[Tech] Changed tech component difficulty: {0} -> {1} [{2}]", component.Difficulty, difficulty,
                component);

            // Update value
            component.Difficulty = difficulty;

            // Update items in the scholarship list view
            componentListView.Items[index].SubItems[3].Text = IntHelper.ToString(difficulty);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            component.SetDirty(TechComponentItemId.Difficulty);

            // Change the font color
            componentDifficultyNumericUpDown.ForeColor = Color.Red;

            // Notify the update of the difficulty level of the essay
            HoI2EditorController.OnItemChanged(EditorItemId.TechComponentDifficulty, this);
        }

        /// <summary>
        ///     Small study 2 Processing when changing the double time setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComponentDoubleTimeCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the scholarship list
            if (componentListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = componentListView.SelectedIndices[0];

            TechComponent component = item.Components[index];
            if (component == null)
            {
                return;
            }

            // Do nothing if the value does not change
            bool doubleTime = componentDoubleTimeCheckBox.Checked;
            if (doubleTime == component.DoubleTime)
            {
                return;
            }

            Log.Info("[Tech] Changed tech component double time: {0} -> {1} [{2}]", component.DoubleTime, doubleTime,
                component);

            // Update value
            component.DoubleTime = doubleTime;

            // Update items in the scholarship list view
            componentListView.Items[index].SubItems[4].Text = doubleTime ? Resources.Yes : Resources.No;

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            component.SetDirty(TechComponentItemId.DoubleTime);

            // Change the font color
            componentDoubleTimeCheckBox.ForeColor = Color.Red;

            // Notify the update of the difficulty level of the essay
            HoI2EditorController.OnItemChanged(EditorItemId.TechComponentDoubleTime, this);
        }

        /// <summary>
        ///     Create an item in the scholarship list
        /// </summary>
        /// <param name="component">Small study</param>
        /// <returns>Items on the essay list</returns>
        private static ListViewItem CreateComponentListItem(TechComponent component)
        {
            ListViewItem li = new ListViewItem { Text = IntHelper.ToString(component.Id) };
            li.SubItems.Add(component.ToString());
            li.SubItems.Add(Techs.GetSpecialityName(component.Speciality));
            li.SubItems.Add(IntHelper.ToString(component.Difficulty));
            li.SubItems.Add(component.DoubleTime ? Resources.Yes : Resources.No);

            return li;
        }

        /// <summary>
        ///     Add an item in the scholarship list
        /// </summary>
        /// <param name="component">Small study to be added</param>
        private void AddComponentListItem(TechComponent component)
        {
            // Add an item to the list
            ListViewItem li = CreateComponentListItem(component);
            componentListView.Items.Add(li);

            // Select the added item
            int index = componentListView.Items.Count - 1;
            componentListView.Items[index].Focused = true;
            componentListView.Items[index].Selected = true;
            componentListView.EnsureVisible(index);

            // Enable edit items
            EnableComponentItems();
        }

        /// <summary>
        ///     Insert an item in the scholarship list
        /// </summary>
        /// <param name="component">Small study to be inserted</param>
        /// <param name="index">Position to insert</param>
        private void InsertComponentListItem(TechComponent component, int index)
        {
            // Add an item to the list
            ListViewItem li = CreateComponentListItem(component);
            componentListView.Items.Insert(index, li);

            // Select the inserted item
            componentListView.Items[index].Focused = true;
            componentListView.Items[index].Selected = true;
            componentListView.EnsureVisible(index);
        }

        /// <summary>
        ///     Move items in the scholarship list
        /// </summary>
        /// <param name="src">Source position</param>
        /// <param name="dest">Destination position</param>
        private void MoveComponentListItem(int src, int dest)
        {
            ListViewItem li = componentListView.Items[src].Clone() as ListViewItem;
            if (li == null)
            {
                return;
            }

            if (src > dest)
            {
                // When moving up
                componentListView.Items.Insert(dest, li);
                componentListView.Items.RemoveAt(src + 1);
            }
            else
            {
                // When moving down
                componentListView.Items.Insert(dest + 1, li);
                componentListView.Items.RemoveAt(src);
            }

            // Select the item to move to
            componentListView.Items[dest].Focused = true;
            componentListView.Items[dest].Selected = true;
            componentListView.EnsureVisible(dest);
        }

        /// <summary>
        ///     Remove an item from the scholarship list
        /// </summary>
        /// <param name="index">Position of the item to be deleted</param>
        private void RemoveComponentListItem(int index)
        {
            componentListView.Items.RemoveAt(index);

            if (index < componentListView.Items.Count)
            {
                // Select the next item after the deleted item
                componentListView.Items[index].Focused = true;
                componentListView.Items[index].Selected = true;
            }
            else if (index > 0)
            {
                // At the end of the list, select the item before the deleted item
                componentListView.Items[componentListView.Items.Count - 1].Focused = true;
                componentListView.Items[componentListView.Items.Count - 1].Selected = true;
            }
            else
            {
                // Disable edit items when there are no more items
                DisableComponentItems();
            }
        }

        #endregion

        #region Technique effect tab

        /// <summary>
        ///     Initialize the edit items on the technical effect tab
        /// </summary>
        private void InitEffectItems()
        {
            Graphics g = Graphics.FromHwnd(Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;

            // Types of technical effects
            commandTypeComboBox.BeginUpdate();
            commandTypeComboBox.Items.Clear();
            int width = commandTypeComboBox.Width;
            foreach (string name in Commands.Types.Select(type => Commands.Strings[(int) type]))
            {
                commandTypeComboBox.Items.Add(name);
                width = Math.Max(width,
                    (int) g.MeasureString(name, commandTypeComboBox.Font).Width +
                    SystemInformation.VerticalScrollBarWidth + margin);
            }
            commandTypeComboBox.DropDownWidth = width;
            commandTypeComboBox.EndUpdate();
        }

        /// <summary>
        ///     Update the items on the Technical Effects tab
        /// </summary>
        /// <param name="item">Technology</param>
        private void UpdateEffectItems(TechItem item)
        {
            effectListView.BeginUpdate();
            effectListView.Items.Clear();

            // Add items in order
            foreach (Command command in item.Effects)
            {
                ListViewItem listItem = CreateEffectListItem(command);

                effectListView.Items.Add(listItem);
            }

            if (effectListView.Items.Count > 0)
            {
                // Select the first item
                effectListView.Items[0].Focused = true;
                effectListView.Items[0].Selected = true;

                // Enable edit items
                EnableEffectItems();
            }
            else
            {
                // Disable edit items
                DisableEffectItems();
            }

            effectListView.EndUpdate();
        }

        /// <summary>
        ///     Activate the Tech Effects tab
        /// </summary>
        private void EnableEffectTab()
        {
            // Enable tabs
            editTabControl.TabPages[(int) TechEditorTab.Effect].Enabled = true;
        }

        /// <summary>
        ///     Disable the technical effects tab
        /// </summary>
        private void DisableEffectTab()
        {
            // Disable tabs
            editTabControl.TabPages[(int) TechEditorTab.Effect].Enabled = false;

            // Clear the items in the technical effect list
            effectListView.Items.Clear();

            // Disable edit items
            DisableEffectItems();
        }

        /// <summary>
        ///     Enable edit items on the Technical Effects tab
        /// </summary>
        private void EnableEffectItems()
        {
            // Enable edit items
            commandTypeComboBox.Enabled = true;
            commandWhichComboBox.Enabled = true;
            commandValueComboBox.Enabled = true;
            commandWhenComboBox.Enabled = true;
            commandWhereComboBox.Enabled = true;

            effectCloneButton.Enabled = true;
            effectRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items on the technical effects tab
        /// </summary>
        private void DisableEffectItems()
        {
            // Clear the value of the edit item
            commandTypeComboBox.SelectedIndex = -1;
            commandTypeComboBox.ResetText();
            commandWhichComboBox.SelectedIndex = -1;
            commandWhichComboBox.ResetText();
            commandValueComboBox.SelectedIndex = -1;
            commandValueComboBox.ResetText();
            commandWhenComboBox.SelectedIndex = -1;
            commandWhenComboBox.ResetText();
            commandWhereComboBox.SelectedIndex = -1;
            commandWhereComboBox.ResetText();

            // Disable edit items
            commandTypeComboBox.Enabled = false;
            commandWhichComboBox.Enabled = false;
            commandValueComboBox.Enabled = false;
            commandWhenComboBox.Enabled = false;
            commandWhereComboBox.Enabled = false;

            effectCloneButton.Enabled = false;
            effectRemoveButton.Enabled = false;
            effectUpButton.Enabled = false;
            effectDownButton.Enabled = false;
        }

        /// <summary>
        ///     Item drawing process of technical effect type combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandTypeComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            TechItem item = GetSelectedItem() as TechItem;
            if (item != null && effectListView.SelectedIndices.Count > 0)
            {
                Command command = item.Effects[effectListView.SelectedIndices[0]];
                Brush brush;
                if ((Commands.Types[e.Index] == command.Type) && command.IsDirty(CommandItemId.Type))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = commandTypeComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Technical effect which which Item drawing process of parameter combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandWhichComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            TechItem item = GetSelectedItem() as TechItem;
            if (item != null && effectListView.SelectedIndices.Count > 0)
            {
                Command command = item.Effects[effectListView.SelectedIndices[0]];
                Brush brush = command.IsDirty(CommandItemId.Which)
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(SystemColors.WindowText);
                string s = ObjectHelper.ToString(commandWhichComboBox.Items[e.Index]);
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Technical effect value value Item drawing process of parameter combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandValueComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            TechItem item = GetSelectedItem() as TechItem;
            if (item != null && effectListView.SelectedIndices.Count > 0)
            {
                Command command = item.Effects[effectListView.SelectedIndices[0]];
                Brush brush = command.IsDirty(CommandItemId.Value)
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(SystemColors.WindowText);
                string s = ObjectHelper.ToString(commandValueComboBox.Items[e.Index]);
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Technical effect when Item drawing process of parameter combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandWhenComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            TechItem item = GetSelectedItem() as TechItem;
            if (item != null && effectListView.SelectedIndices.Count > 0)
            {
                Command command = item.Effects[effectListView.SelectedIndices[0]];
                Brush brush = command.IsDirty(CommandItemId.When)
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(SystemColors.WindowText);
                string s = ObjectHelper.ToString(commandWhenComboBox.Items[e.Index]);
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Technical effect where Item drawing process of parameter combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandWhereComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            TechItem item = GetSelectedItem() as TechItem;
            if (item != null && effectListView.SelectedIndices.Count > 0)
            {
                Command command = item.Effects[effectListView.SelectedIndices[0]];
                Brush brush = command.IsDirty(CommandItemId.Where)
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(SystemColors.WindowText);
                string s = ObjectHelper.ToString(commandWhereComboBox.Items[e.Index]);
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item in the technical effect list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEffectListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Disable edit items if there are no selection items in the technical effect list
            if (effectListView.SelectedIndices.Count == 0)
            {
                DisableEffectItems();
                return;
            }
            int index = effectListView.SelectedIndices[0];

            Command command = item.Effects[index];
            if (command == null)
            {
                return;
            }

            // Update the value of the edit item
            if (command.Type != CommandType.None)
            {
                commandTypeComboBox.SelectedIndex = Commands.Types.IndexOf(command.Type);
            }
            else
            {
                commandTypeComboBox.SelectedIndex = -1;
                commandTypeComboBox.Text = "";
            }
            commandWhichComboBox.Text = ObjectHelper.ToString(command.Which);
            commandValueComboBox.Text = ObjectHelper.ToString(command.Value);
            commandWhenComboBox.Text = ObjectHelper.ToString(command.When);
            commandWhereComboBox.Text = ObjectHelper.ToString(command.Where);

            // Update the color of the combo box
            commandTypeComboBox.Refresh();
            commandWhichComboBox.Refresh();
            commandValueComboBox.Refresh();
            commandWhenComboBox.Refresh();
            commandWhereComboBox.Refresh();
            commandWhichComboBox.ForeColor = command.IsDirty(CommandItemId.Which) ? Color.Red : SystemColors.WindowText;
            commandValueComboBox.ForeColor = command.IsDirty(CommandItemId.Value) ? Color.Red : SystemColors.WindowText;
            commandWhenComboBox.ForeColor = command.IsDirty(CommandItemId.When) ? Color.Red : SystemColors.WindowText;
            commandWhereComboBox.ForeColor = command.IsDirty(CommandItemId.Where) ? Color.Red : SystemColors.WindowText;

            // Enable edit items
            EnableEffectItems();

            effectUpButton.Enabled = index != 0;
            effectDownButton.Enabled = index != item.Effects.Count - 1;
        }

        /// <summary>
        ///     What to do when changing the width of a column in the technical effect list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEffectListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < EffectListColumnCount))
            {
                HoI2EditorController.Settings.TechEditor.EffectListColumnWidth[e.ColumnIndex] =
                    effectListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Processing before editing items in the technical effect list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEffectListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // type
                    e.Type = ItemEditType.List;
                    e.Items = commandTypeComboBox.Items.Cast<string>();
                    e.Index = commandTypeComboBox.SelectedIndex;
                    e.DropDownWidth = commandTypeComboBox.DropDownWidth;
                    break;

                case 1: // Which
                    e.Type = ItemEditType.Text;
                    e.Text = commandWhichComboBox.Text;
                    break;

                case 2: // Value Value.
                    e.Type = ItemEditType.Text;
                    e.Text = commandValueComboBox.Text;
                    break;

                case 3: // When
                    e.Type = ItemEditType.Text;
                    e.Text = commandWhenComboBox.Text;
                    break;

                case 4: // Where
                    e.Type = ItemEditType.Text;
                    e.Text = commandWhereComboBox.Text;
                    break;
            }
        }

        /// <summary>
        ///     Processing after editing items in the technical effect list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEffectListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // type
                    commandTypeComboBox.SelectedIndex = e.Index;
                    break;

                case 1: // Which
                    commandWhichComboBox.Text = e.Text;
                    break;

                case 2: // Value Value.
                    commandValueComboBox.Text = e.Text;
                    break;

                case 3: // When
                    commandWhenComboBox.Text = e.Text;
                    break;

                case 4: // Where
                    commandWhereComboBox.Text = e.Text;
                    break;
            }

            // Since the items in the list view will be updated by yourself, it will be treated as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Processing when replacing items in the technical effect list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEffectListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // Do nothing if there is no selection
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;

            // Move technology effects
            Command command = item.Effects[srcIndex];
            item.Effects.Insert(destIndex, command);
            if (srcIndex < destIndex)
            {
                item.Effects.RemoveAt(srcIndex);
            }
            else
            {
                item.Effects.RemoveAt(srcIndex + 1);
            }

            Log.Info("[Tech] Move effect: {0} -> {1} {2} [{3}]", srcIndex, destIndex,
                Commands.Strings[(int) command.Type], item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            command.SetDirty();
        }

        /// <summary>
        ///     Processing when a new button is pressed for a technical effect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEffectNewButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            Command command = new Command();

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            command.SetDirtyAll();

            if (effectListView.SelectedIndices.Count > 0)
            {
                // Insert an item in the list
                int index = effectListView.SelectedIndices[0];
                item.InsertCommand(command, index + 1);

                // Insert an item in the technical effect list view
                InsertEffectListItem(command, index + 1);
            }
            else
            {
                // Add an item to the list
                item.AddCommand(command);

                // Add an item to the technical effect list view
                AddEffectListItem(command);
            }

            Log.Info("[Tech] Added new effect: [{0}]", item);
        }

        /// <summary>
        ///     Processing when the duplicate button of the technical effect is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEffectCloneButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the technical effect list
            if (effectListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = effectListView.SelectedIndices[0];

            Command command = new Command(item.Effects[index]);

            Log.Info("[Tech] Added new effect: {0} [{1}]", Commands.Strings[(int) command.Type], item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            command.SetDirtyAll();

            // Insert an item in the list
            item.InsertCommand(command, index + 1);

            // Insert an item in the technical effect list view
            InsertEffectListItem(command, index + 1);
        }

        /// <summary>
        ///     Processing when the delete button of the technical effect is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEffectRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the technical effect list
            if (effectListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = effectListView.SelectedIndices[0];

            Log.Info("[Tech] Removed effect: {0} [{1}]", Commands.Strings[(int) item.Effects[index].Type], item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();

            // Remove an item from the list
            item.RemoveCommand(index);

            // Remove an item from the technical effect list view
            RemoveEffectListItem(index);
        }

        /// <summary>
        ///     Processing when the button is pressed on the technical effect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEffectUpButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the technical effect list
            if (effectListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = effectListView.SelectedIndices[0];

            // Do nothing at the top of the list
            if (index == 0)
            {
                return;
            }

            // Move items
            item.MoveCommand(index, index - 1);
            MoveEffectListItem(index, index - 1);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
        }

        /// <summary>
        ///     Processing when the button is pressed under the technical effect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEffectDownButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the technical effect list
            if (effectListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = effectListView.SelectedIndices[0];

            // Do nothing at the end of the list
            if (index == effectListView.Items.Count - 1)
            {
                return;
            }

            // Move items
            item.MoveCommand(index, index + 1);
            MoveEffectListItem(index, index + 1);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
        }

        /// <summary>
        ///     Processing when changing the type of technical effect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandTypeComboBoxSelectionChangeCommitted(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the technical effect list
            if (effectListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = effectListView.SelectedIndices[0];

            // Do nothing if there is no command list selection
            if (commandTypeComboBox.SelectedIndex == -1)
            {
                return;
            }

            Command command = item.Effects[index];
            if (command == null)
            {
                return;
            }

            // Do nothing if the value does not change
            CommandType type = Commands.Types[commandTypeComboBox.SelectedIndex];
            if (type == command.Type)
            {
                return;
            }

            Log.Info("[Tech] Changed tech effect type: {0} -> {1} [{2}]", Commands.Strings[(int) command.Type],
                Commands.Strings[(int) type], item);

            // Update value
            command.Type = type;

            // Update the display of the technical effect list view
            effectListView.Items[index].Text = Commands.Strings[(int) type];

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            command.SetDirty(CommandItemId.Type);

            // Technology effect type Update drawing to change the item color of the combo box
            commandTypeComboBox.Refresh();
        }

        /// <summary>
        ///     Technical effect which which Processing when changing parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandWhichComboBoxTextUpdate(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the technical effect list
            if (effectListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = effectListView.SelectedIndices[0];

            Command command = item.Effects[index];
            if (command == null)
            {
                return;
            }

            double val;
            if (DoubleHelper.TryParse(commandWhichComboBox.Text, out val))
            {
                // Do nothing if the value does not change
                if (ObjectHelper.IsEqual(val, command.Which))
                {
                    return;
                }

                Log.Info("[Tech] Changed tech effect which: {0} -> {1} [{2}]", ObjectHelper.ToString(command.Which),
                    DoubleHelper.ToString(val), item);

                // Update value
                command.Which = val;
            }
            else
            {
                // Do nothing if the value does not change
                string text = commandWhichComboBox.Text;
                if (ObjectHelper.IsEqual(text, command.Which))
                {
                    return;
                }

                Log.Info("[Tech] Changed tech effect which: {0} -> {1} [{2}]", ObjectHelper.ToString(command.Which),
                    text, item);

                // Update value
                command.Which = text;
            }

            // Update the display of the technical effect list view
            effectListView.Items[index].SubItems[1].Text = ObjectHelper.ToString(command.Which);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            command.SetDirty(CommandItemId.Which);

            // Change the font color
            commandWhichComboBox.ForeColor = Color.Red;

            // Technical effect which which Update drawing to change the item color of the parameter combo box
            commandWhichComboBox.Refresh();
        }

        /// <summary>
        ///     Technical effect value value Processing when changing parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandValueComboBoxTextUpdate(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the technical effect list
            if (effectListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = effectListView.SelectedIndices[0];

            Command command = item.Effects[index];
            if (command == null)
            {
                return;
            }

            double val;
            if (DoubleHelper.TryParse(commandValueComboBox.Text, out val))
            {
                // Do nothing if the value does not change
                if (ObjectHelper.IsEqual(val, command.Value))
                {
                    return;
                }

                Log.Info("[Tech] Changed tech effect value: {0} -> {1} [{2}]", ObjectHelper.ToString(command.Value),
                    DoubleHelper.ToString(val), item);

                // Update value
                command.Value = val;
            }
            else
            {
                // Do nothing if the value does not change
                string text = commandValueComboBox.Text;
                if (ObjectHelper.IsEqual(text, command.Value))
                {
                    return;
                }

                Log.Info("[Tech] Changed tech effect value: {0} -> {1} [{2}]", ObjectHelper.ToString(command.Value),
                    text, item);

                // Update value
                command.Value = text;
            }

            // Update the display of the technical effect list view
            effectListView.Items[index].SubItems[2].Text = ObjectHelper.ToString(command.Value);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            command.SetDirty(CommandItemId.Value);

            // Change the font color
            commandValueComboBox.ForeColor = Color.Red;

            // Technical effect value value Update drawing to change the item color of the parameter combo box
            commandValueComboBox.Refresh();
        }

        /// <summary>
        ///     Technical effect when Processing when changing parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandWhenComboBoxTextUpdate(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the technical effect list
            if (effectListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = effectListView.SelectedIndices[0];

            Command command = item.Effects[index];
            if (command == null)
            {
                return;
            }

            double val;
            if (DoubleHelper.TryParse(commandWhenComboBox.Text, out val))
            {
                // Do nothing if the value does not change
                if (ObjectHelper.IsEqual(val, command.When))
                {
                    return;
                }

                Log.Info("[Tech] Changed tech effect when: {0} -> {1} [{2}]", ObjectHelper.ToString(command.When),
                    DoubleHelper.ToString(val), item);

                // Update value
                command.When = val;
            }
            else
            {
                // Do nothing if the value does not change
                string text = commandWhenComboBox.Text;
                if (ObjectHelper.IsEqual(text, command.When))
                {
                    return;
                }

                Log.Info("[Tech] Changed tech effect when: {0} -> {1} [{2}]", ObjectHelper.ToString(command.When), text,
                    item);

                // Update value
                command.When = text;
            }

            // Update the display of the technical effect list view
            effectListView.Items[index].SubItems[3].Text = ObjectHelper.ToString(command.When);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            command.SetDirty(CommandItemId.When);

            // Change the font color
            commandWhenComboBox.ForeColor = Color.Red;

            // Technical effect when Update drawing to change the item color of the parameter combo box
            commandWhenComboBox.Refresh();
        }

        /// <summary>
        ///     Technical effect where Processing when changing parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCommandWhereComboBoxTextUpdate(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechItem item = GetSelectedItem() as TechItem;
            if (item == null)
            {
                return;
            }

            // Do nothing if there is no selection in the technical effect list
            if (effectListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = effectListView.SelectedIndices[0];

            Command command = item.Effects[index];
            if (command == null)
            {
                return;
            }

            double val;
            if (DoubleHelper.TryParse(commandWhereComboBox.Text, out val))
            {
                // Do nothing if the value does not change
                if (ObjectHelper.IsEqual(val, command.Where))
                {
                    return;
                }

                Log.Info("[Tech] Changed tech effect where: {0} -> {1} [{2}]", ObjectHelper.ToString(command.Where),
                    DoubleHelper.ToString(val), item);

                // Update value
                command.Where = val;
            }
            else
            {
                // Do nothing if the value does not change
                string text = commandWhereComboBox.Text;
                if (ObjectHelper.IsEqual(text, command.Where))
                {
                    return;
                }

                Log.Info("[Tech] Changed tech effect where: {0} -> {1} [{2}]", ObjectHelper.ToString(command.Where),
                    text, item);

                // Update value
                command.Where = text;
            }

            // Update the display of the technical effect list view
            effectListView.Items[index].SubItems[4].Text = ObjectHelper.ToString(command.Where);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            command.SetDirty(CommandItemId.Where);

            // Change the font color
            commandWhereComboBox.ForeColor = Color.Red;

            // Technical effect where Update drawing to change the item color of the parameter combo box
            commandWhereComboBox.Refresh();
        }

        /// <summary>
        ///     Create an item in the technical effect list
        /// </summary>
        /// <param name="command">Technical effect</param>
        /// <returns>Items in the technical effect list</returns>
        private static ListViewItem CreateEffectListItem(Command command)
        {
            ListViewItem li = new ListViewItem { Text = Commands.Strings[(int) command.Type] };
            li.SubItems.Add(ObjectHelper.ToString(command.Which));
            li.SubItems.Add(ObjectHelper.ToString(command.Value));
            li.SubItems.Add(ObjectHelper.ToString(command.When));
            li.SubItems.Add(ObjectHelper.ToString(command.Where));

            return li;
        }

        /// <summary>
        ///     Add an item in the technical effect list
        /// </summary>
        /// <param name="command">Technical effect to be added</param>
        private void AddEffectListItem(Command command)
        {
            // Add an item to the list
            ListViewItem li = CreateEffectListItem(command);
            effectListView.Items.Add(li);

            // Select the added item
            int index = effectListView.Items.Count - 1;
            effectListView.Items[index].Focused = true;
            effectListView.Items[index].Selected = true;
            effectListView.EnsureVisible(index);

            // Enable edit items
            EnableEffectItems();
        }

        /// <summary>
        ///     Insert an item in the technical effect list
        /// </summary>
        /// <param name="command">Technical effect of insertion target</param>
        /// <param name="index">Position to insert</param>
        private void InsertEffectListItem(Command command, int index)
        {
            // Insert an item in the list
            ListViewItem li = CreateEffectListItem(command);
            effectListView.Items.Insert(index, li);

            // Select the inserted item
            effectListView.Items[index].Focused = true;
            effectListView.Items[index].Selected = true;
            effectListView.EnsureVisible(index);
        }

        /// <summary>
        ///     Move items in the technical effect list
        /// </summary>
        /// <param name="src">Source position</param>
        /// <param name="dest">Destination position</param>
        private void MoveEffectListItem(int src, int dest)
        {
            ListViewItem li = effectListView.Items[src].Clone() as ListViewItem;
            if (li == null)
            {
                return;
            }

            if (src > dest)
            {
                // When moving up
                effectListView.Items.Insert(dest, li);
                effectListView.Items.RemoveAt(src + 1);
            }
            else
            {
                // When moving down
                effectListView.Items.Insert(dest + 1, li);
                effectListView.Items.RemoveAt(src);
            }

            // Select the item to move to
            effectListView.Items[dest].Focused = true;
            effectListView.Items[dest].Selected = true;
            effectListView.EnsureVisible(dest);
        }

        /// <summary>
        ///     Remove an item from the Tech Effects list
        /// </summary>
        /// <param name="index">Position of the item to be deleted</param>
        private void RemoveEffectListItem(int index)
        {
            // Remove an item from the list
            effectListView.Items.RemoveAt(index);

            if (index < effectListView.Items.Count)
            {
                // Select the next item after the deleted item
                effectListView.Items[index].Focused = true;
                effectListView.Items[index].Selected = true;
            }
            else if (index > 0)
            {
                // At the end of the list, select the item before the deleted item
                effectListView.Items[effectListView.Items.Count - 1].Focused = true;
                effectListView.Items[effectListView.Items.Count - 1].Selected = true;
            }
            else
            {
                // Disable edit items when there are no more items
                DisableEffectItems();
            }
        }

        #endregion

        #region Label tab

        /// <summary>
        ///     Update items on the Label tab
        /// </summary>
        /// <param name="item">Tech label</param>
        private void UpdateLabelItems(TechLabel item)
        {
            // Update the value of the edit item
            labelNameTextBox.Text = item.ToString();
            UpdateLabelPositionList(item);

            // Update the color of the edit item
            labelNameTextBox.ForeColor = item.IsDirty(TechItemId.Name) ? Color.Red : SystemColors.WindowText;
        }

        /// <summary>
        ///     Enable Label Tab
        /// </summary>
        private void EnableLabelTab()
        {
            // Enable tabs
            editTabControl.TabPages[5].Enabled = true;

            // Reset the value cleared at the time of invalidation
            labelXNumericUpDown.Text = IntHelper.ToString((int) labelXNumericUpDown.Value);
            labelYNumericUpDown.Text = IntHelper.ToString((int) labelYNumericUpDown.Value);
        }

        /// <summary>
        ///     Disable the label tab
        /// </summary>
        private void DisableLabelTab()
        {
            // Disable tabs
            editTabControl.TabPages[5].Enabled = false;

            // Clear the value of the edit item
            labelNameTextBox.ResetText();
            labelPositionListView.Items.Clear();
            labelXNumericUpDown.ResetText();
            labelYNumericUpDown.ResetText();
        }

        /// <summary>
        ///     Enable edit items for technical label coordinates
        /// </summary>
        private void EnableLabelPositionItems()
        {
            // Reset the value cleared at the time of invalidation
            labelXNumericUpDown.Text = IntHelper.ToString((int) labelXNumericUpDown.Value);
            labelYNumericUpDown.Text = IntHelper.ToString((int) labelYNumericUpDown.Value);

            // Enable edit items
            labelXNumericUpDown.Enabled = true;
            labelYNumericUpDown.Enabled = true;

            labelPositionRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for technical label coordinates
        /// </summary>
        private void DisableLabelPositionItems()
        {
            // Clear the value of the edit item
            labelXNumericUpDown.ResetText();
            labelYNumericUpDown.ResetText();

            // Disable edit items
            labelXNumericUpDown.Enabled = false;
            labelYNumericUpDown.Enabled = false;

            labelPositionRemoveButton.Enabled = false;
        }

        /// <summary>
        ///     Update tech label coordinate list
        /// </summary>
        /// <param name="item">Tech label</param>
        private void UpdateLabelPositionList(TechLabel item)
        {
            labelPositionListView.BeginUpdate();
            labelPositionListView.Items.Clear();

            foreach (TechPosition position in item.Positions)
            {
                ListViewItem listItem = new ListViewItem(IntHelper.ToString(position.X));
                listItem.SubItems.Add(IntHelper.ToString(position.Y));
                labelPositionListView.Items.Add(listItem);
            }

            if (labelPositionListView.Items.Count > 0)
            {
                // Select the first item
                labelPositionListView.Items[0].Focused = true;
                labelPositionListView.Items[0].Selected = true;

                // Enable edit items
                EnableLabelPositionItems();
            }
            else
            {
                // Disable edit items
                DisableLabelPositionItems();
            }

            labelPositionListView.EndUpdate();
        }

        /// <summary>
        ///     Processing when changing the label name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLabelNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechLabel item = GetSelectedItem() as TechLabel;
            if (item == null)
            {
                return;
            }

            // Do nothing if the value does not change
            string text = labelNameTextBox.Text;
            if (text.Equals(item.ToString()))
            {
                return;
            }

            // If it is a duplicate character string, reset the definition name
            if (Techs.IsDuplicatedName(item.Name))
            {
                Techs.DecrementDuplicatedListCount(item.Name);
                item.Name = Config.GetTempKey();
                Techs.IncrementDuplicatedListCount(item.Name);
            }

            Log.Info("[Tech] Changed label name: {0} -> {1} <{2}>", item, text, item.Name);

            // Update value
            Config.SetText(item.Name, text, Game.TechTextFileName);

            // The display is updated by resetting the items in the item list box.
            // At this time, the focus will be lost due to reselection, so the event handler will be temporarily disabled.
            techListBox.SelectedIndexChanged -= OnTechListBoxSelectedIndexChanged;
            techListBox.Items[techListBox.SelectedIndex] = item;
            techListBox.SelectedIndexChanged += OnTechListBoxSelectedIndexChanged;

            // Update the label name on the tech tree
            _techTreePanelController.UpdateItem(item);

            // Set the edited flag
            item.SetDirty(TechItemId.Name);

            // Change the font color
            labelNameTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the selection item in the label coordinate list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLabelPositionListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there are no selections in the item list
            TechLabel item = GetSelectedItem() as TechLabel;
            if (item == null)
            {
                return;
            }

            // Disable edit items if there is no selection in the label coordinate list
            if (labelPositionListView.SelectedIndices.Count == 0)
            {
                DisableLabelPositionItems();
                return;
            }

            // Update the value of the edit item
            TechPosition position = item.Positions[labelPositionListView.SelectedIndices[0]];
            labelXNumericUpDown.Value = position.X;
            labelYNumericUpDown.Value = position.Y;

            // Update the color of the edit item
            labelXNumericUpDown.ForeColor = position.IsDirty(TechPositionItemId.X) ? Color.Red : SystemColors.WindowText;
            labelYNumericUpDown.ForeColor = position.IsDirty(TechPositionItemId.Y) ? Color.Red : SystemColors.WindowText;

            // Enable edit items
            EnableLabelPositionItems();
        }

        /// <summary>
        ///     Processing when changing the width of columns in the label coordinate list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLabelPositionListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < PositionListColumnCount))
            {
                HoI2EditorController.Settings.TechEditor.LabelPositionListColumnWidth[e.ColumnIndex] =
                    labelPositionListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Processing before editing items in the label coordinate list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLabelPositionListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // X
                    e.Type = ItemEditType.Text;
                    e.Text = labelXNumericUpDown.Text;
                    break;

                case 1: // Y
                    e.Type = ItemEditType.Text;
                    e.Text = labelYNumericUpDown.Text;
                    break;
            }
        }

        /// <summary>
        ///     Processing after editing items in the label coordinate list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLabelPositionListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // X
                    labelXNumericUpDown.Text = e.Text;
                    break;

                case 1: // Y
                    labelYNumericUpDown.Text = e.Text;
                    break;
            }

            // Since the items in the list view will be updated by yourself, it will be treated as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Processing when replacing items in the label coordinate list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLabelPositionListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // Do nothing if there is no selection
            TechLabel item = GetSelectedItem() as TechLabel;
            if (item == null)
            {
                return;
            }

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;

            // Move label coordinates
            TechPosition position = item.Positions[srcIndex];
            item.Positions.Insert(destIndex, position);
            if (srcIndex < destIndex)
            {
                item.Positions.RemoveAt(srcIndex);
            }
            else
            {
                item.Positions.RemoveAt(srcIndex + 1);
            }

            Log.Info("[Tech] Move label position: {0} -> {1} ({2}, {3}) [{4}]", srcIndex, destIndex, position.X,
                position.Y, item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
        }

        /// <summary>
        ///     label X Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLabelXNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechLabel item = GetSelectedItem() as TechLabel;
            if (item == null)
            {
                return;
            }

            if (labelPositionListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = labelPositionListView.SelectedIndices[0];

            TechPosition position = item.Positions[index];

            // Do nothing if the value does not change
            int x = (int) labelXNumericUpDown.Value;
            if (x == position.X)
            {
                return;
            }

            Log.Info("[Tech] Changed label position: ({0},{1}) -> ({2},{1}) [{3}]", position.X, position.Y, x, item);

            // Update value
            position.X = x;

            // Update items in the label coordinate list view
            labelPositionListView.Items[index].Text = IntHelper.ToString(x);

            // Move labels on the tech tree
            _techTreePanelController.UpdateItem(item, position);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            position.SetDirty(TechPositionItemId.X);

            // Change the font color
            labelXNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     label Y Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLabelYNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechLabel item = GetSelectedItem() as TechLabel;
            if (item == null)
            {
                return;
            }

            if (labelPositionListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = labelPositionListView.SelectedIndices[0];

            TechPosition position = item.Positions[index];

            // Do nothing if the value does not change
            int y = (int) labelYNumericUpDown.Value;
            if (y == position.Y)
            {
                return;
            }

            Log.Info("[Tech] Changed label position: ({0},{1}) -> ({0},{2}) [{3}]", position.X, position.Y, y, item);

            // Update value
            position.Y = y;

            // Update items in the label coordinate list view
            labelPositionListView.Items[index].SubItems[1].Text = IntHelper.ToString(y);

            // Move labels on the tech tree
            _techTreePanelController.UpdateItem(item, position);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            position.SetDirty(TechPositionItemId.Y);

            // Change the font color
            labelYNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the label coordinate addition button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLabelPositionAddButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechLabel item = GetSelectedItem() as TechLabel;
            if (item == null)
            {
                return;
            }

            // Add an item to the label coordinate list
            TechPosition position = new TechPosition { X = 0, Y = 0 };
            item.Positions.Add(position);

            Log.Info("[Tech] Added label position: ({0},{1}) [{2}]", position.X, position.Y, item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            position.SetDirtyAll();

            // Add an item in the label coordinate list view
            ListViewItem li = new ListViewItem { Text = IntHelper.ToString(position.X) };
            li.SubItems.Add(IntHelper.ToString(position.Y));
            labelPositionListView.Items.Add(li);

            // Select the added item
            labelPositionListView.Items[labelPositionListView.Items.Count - 1].Focused = true;
            labelPositionListView.Items[labelPositionListView.Items.Count - 1].Selected = true;

            // Enable edit items for label coordinates
            EnableLabelPositionItems();

            // Add a label to the tech tree
            _techTreePanelController.AddItem(item, position);
        }

        /// <summary>
        ///     Processing when the label coordinate delete button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLabelPositionRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechLabel item = GetSelectedItem() as TechLabel;
            if (item == null)
            {
                return;
            }

            if (labelPositionListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = labelPositionListView.SelectedIndices[0];
            TechPosition position = item.Positions[index];

            Log.Info("[Tech] Removed label position: ({0},{1}) [{2}]", position.X, position.Y, item);

            // Remove an item from the label coordinate list
            item.Positions.RemoveAt(index);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();

            // Remove an item from the label coordinate list view
            labelPositionListView.Items.RemoveAt(index);

            if (index < labelPositionListView.Items.Count)
            {
                // Select the next item after the deleted item
                labelPositionListView.Items[index].Focused = true;
                labelPositionListView.Items[index].Selected = true;
            }
            else if (index > 0)
            {
                // At the end of the list, select the item before the deleted item
                labelPositionListView.Items[labelPositionListView.Items.Count - 1].Focused = true;
                labelPositionListView.Items[labelPositionListView.Items.Count - 1].Selected = true;
            }
            else
            {
                // Disable edit items when there are no more items
                DisableLabelPositionItems();
            }

            // Remove the label from the tech tree
            _techTreePanelController.RemoveItem(item, position);
        }

        #endregion

        #region Invention event tab

        /// <summary>
        ///     Update the item on the invention event tab
        /// </summary>
        /// <param name="item">Technical event</param>
        private void UpdateEventItems(TechEvent item)
        {
            // Update the value of the edit item
            eventIdNumericUpDown.Value = item.Id;
            eventTechNumericUpDown.Value = item.TechId;
            if (Techs.TechIds.Contains(item.TechId))
            {
                eventTechComboBox.SelectedIndex = Techs.TechIds.IndexOf(item.TechId);
            }
            else
            {
                eventTechComboBox.SelectedIndex = -1;
                eventTechComboBox.ResetText();
            }
            UpdateEventPositionList(item);

            // Update the color of the combo box
            eventTechComboBox.Refresh();

            // Update the color of the edit item
            eventIdNumericUpDown.ForeColor = item.IsDirty(TechItemId.Id) ? Color.Red : SystemColors.WindowText;
            eventTechNumericUpDown.ForeColor = item.IsDirty(TechItemId.TechId) ? Color.Red : SystemColors.WindowText;
        }

        /// <summary>
        ///     Update the technology list on the Invention Events tab
        /// </summary>
        private void UpdateEventTechListItems()
        {
            Graphics g = Graphics.FromHwnd(Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;

            eventTechComboBox.BeginUpdate();
            eventTechComboBox.Items.Clear();

            int width = eventTechComboBox.Width;
            foreach (TechItem item in Techs.TechIdMap.Select(pair => pair.Value))
            {
                eventTechComboBox.Items.Add(item);
                width = Math.Max(width,
                    (int) g.MeasureString(item.ToString(), eventTechComboBox.Font).Width +
                    SystemInformation.VerticalScrollBarWidth + margin);
            }
            eventTechComboBox.DropDownWidth = width;

            eventTechComboBox.EndUpdate();
        }

        /// <summary>
        ///     Enable the Invention Events tab
        /// </summary>
        private void EnableEventTab()
        {
            // Enable tabs
            editTabControl.TabPages[6].Enabled = true;

            // Reset the value cleared at the time of invalidation
            eventIdNumericUpDown.Text = IntHelper.ToString((int) eventIdNumericUpDown.Value);
            eventTechNumericUpDown.Text = IntHelper.ToString((int) eventTechNumericUpDown.Value);
            eventXNumericUpDown.Text = IntHelper.ToString((int) eventXNumericUpDown.Value);
            eventYNumericUpDown.Text = IntHelper.ToString((int) eventYNumericUpDown.Value);
        }

        /// <summary>
        ///     Disable the Invention Event tab
        /// </summary>
        private void DisableEventTab()
        {
            // Disable tabs
            editTabControl.TabPages[6].Enabled = false;

            // Clear the value of the edit item
            eventIdNumericUpDown.ResetText();
            eventTechNumericUpDown.ResetText();
            eventTechComboBox.SelectedIndex = -1;
            eventTechComboBox.ResetText();
            eventPositionListView.Items.Clear();
            eventXNumericUpDown.ResetText();
            eventYNumericUpDown.ResetText();
        }

        /// <summary>
        ///     Enable edit items for invention event coordinates
        /// </summary>
        private void EnableEventPositionItems()
        {
            // Reset the character string cleared at the time of invalidation
            eventXNumericUpDown.Text = IntHelper.ToString((int) eventXNumericUpDown.Value);
            eventYNumericUpDown.Text = IntHelper.ToString((int) eventYNumericUpDown.Value);

            // Enable edit items
            eventXNumericUpDown.Enabled = true;
            eventYNumericUpDown.Enabled = true;

            eventPositionRemoveButton.Enabled = true;
        }

        /// <summary>
        ///     Disable edit items for invention event coordinates
        /// </summary>
        private void DisableEventPositionItems()
        {
            // Clear edit items
            eventXNumericUpDown.ResetText();
            eventYNumericUpDown.ResetText();

            // Disable edit items
            eventXNumericUpDown.Enabled = false;
            eventYNumericUpDown.Enabled = false;

            eventPositionRemoveButton.Enabled = false;
        }

        /// <summary>
        ///     Update the invention event coordinate list
        /// </summary>
        /// <param name="item">Invention event</param>
        private void UpdateEventPositionList(TechEvent item)
        {
            eventPositionListView.BeginUpdate();
            eventPositionListView.Items.Clear();

            foreach (TechPosition position in item.Positions)
            {
                ListViewItem listItem = new ListViewItem(IntHelper.ToString(position.X));
                listItem.SubItems.Add(IntHelper.ToString(position.Y));
                eventPositionListView.Items.Add(listItem);
            }

            if (eventPositionListView.Items.Count > 0)
            {
                // Select the first item
                eventPositionListView.Items[0].Focused = true;
                eventPositionListView.Items[0].Selected = true;

                // Enable edit items
                EnableEventPositionItems();
            }
            else
            {
                // Disable edit items
                DisableEventPositionItems();
            }

            eventPositionListView.EndUpdate();
        }

        /// <summary>
        ///     Invention event technology combo box item drawing process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventTechComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            TechEvent item = GetSelectedItem() as TechEvent;
            if (item != null)
            {
                Brush brush;
                if ((Techs.TechIds[e.Index] == item.TechId) && item.IsDirty(TechItemId.TechId))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = eventTechComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Invention event ID Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventIdNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechEvent item = GetSelectedItem() as TechEvent;
            if (item == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int id = (int) eventIdNumericUpDown.Value;
            if (id == item.Id)
            {
                return;
            }

            Log.Info("[Tech] Changed event id: {0} -> {1}", item.Id, id);

            // Update value
            item.Id = id;

            // The display is updated by resetting the items in the item list box.
            // At this time, the focus will be lost due to reselection, so the event handler will be temporarily disabled.
            techListBox.SelectedIndexChanged -= OnTechListBoxSelectedIndexChanged;
            techListBox.Items[techListBox.SelectedIndex] = item;
            techListBox.SelectedIndexChanged += OnTechListBoxSelectedIndexChanged;

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty(TechItemId.Id);

            // Change the font color
            eventIdNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Invention event technology ID Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventTechNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechEvent item = GetSelectedItem() as TechEvent;
            if (item == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int id = (int) eventTechNumericUpDown.Value;
            if (id == item.TechId)
            {
                return;
            }

            Log.Info("[Tech] Changed event tech id: {0} -> {1}", item.TechId, id);

            // Update value
            item.TechId = id;

            // Update technical combo box selections
            if (Techs.TechIds.Contains(id))
            {
                eventTechComboBox.SelectedIndex = Techs.TechIds.IndexOf(id);
            }
            else
            {
                eventTechComboBox.SelectedIndex = -1;
                eventTechComboBox.ResetText();
            }

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty(TechItemId.TechId);

            // Change the font color
            eventTechNumericUpDown.ForeColor = Color.Red;

            // Invention Event Technology Combo box item Color update to change drawing
            eventTechComboBox.Refresh();
        }

        /// <summary>
        ///     Invention event Processing when technology is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventTechComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechEvent item = GetSelectedItem() as TechEvent;
            if (item == null)
            {
                return;
            }

            if (eventTechComboBox.SelectedIndex == -1)
            {
                return;
            }

            // Invention event technology ID Update the value of
            int id = Techs.TechIds[eventTechComboBox.SelectedIndex];
            eventTechNumericUpDown.Value = id;
        }

        /// <summary>
        ///     Invention Event Coordinate list view processing when changing selection items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventPositionListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no choice in the tech list
            TechEvent item = GetSelectedItem() as TechEvent;
            if (item == null)
            {
                return;
            }

            // If there is no selection item in the invention event coordinate list, the edit item is invalidated.
            if (eventPositionListView.SelectedIndices.Count == 0)
            {
                DisableEventPositionItems();
                return;
            }

            // Update the value of the edit item
            TechPosition position = item.Positions[eventPositionListView.SelectedIndices[0]];
            eventXNumericUpDown.Value = position.X;
            eventYNumericUpDown.Value = position.Y;

            // Update the color of the edit item
            eventXNumericUpDown.ForeColor = position.IsDirty(TechPositionItemId.X) ? Color.Red : SystemColors.WindowText;
            eventYNumericUpDown.ForeColor = position.IsDirty(TechPositionItemId.Y) ? Color.Red : SystemColors.WindowText;

            // Enable edit items
            EnableEventPositionItems();
        }

        /// <summary>
        ///     Invention event Processing when changing the width of a column in the coordinate list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventPositionListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < PositionListColumnCount))
            {
                HoI2EditorController.Settings.TechEditor.EventPositionListColumnWidth[e.ColumnIndex] =
                    eventPositionListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Process before editing items in the invention event coordinate list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventPositionListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // X
                    e.Type = ItemEditType.Text;
                    e.Text = eventXNumericUpDown.Text;
                    break;

                case 1: // Y
                    e.Type = ItemEditType.Text;
                    e.Text = eventYNumericUpDown.Text;
                    break;
            }
        }

        /// <summary>
        ///     Processing after editing items in the invention event coordinate list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventPositionListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // X
                    eventXNumericUpDown.Text = e.Text;
                    break;

                case 1: // Y
                    eventYNumericUpDown.Text = e.Text;
                    break;
            }

            // Since the items in the list view will be updated by yourself, it will be treated as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Invention Event Coordinate list view processing when replacing items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventPositionListViewItemReordered(object sender, ItemReorderedEventArgs e)
        {
            // Do nothing if there is no selection
            TechEvent item = GetSelectedItem() as TechEvent;
            if (item == null)
            {
                return;
            }

            int srcIndex = e.OldDisplayIndices[0];
            int destIndex = e.NewDisplayIndex;

            // Move label coordinates
            TechPosition position = item.Positions[srcIndex];
            item.Positions.Insert(destIndex, position);
            if (srcIndex < destIndex)
            {
                item.Positions.RemoveAt(srcIndex);
            }
            else
            {
                item.Positions.RemoveAt(srcIndex + 1);
            }

            Log.Info("[Tech] Move event position: {0} -> {1} ({2}, {3}) [{4}]", srcIndex, destIndex, position.X,
                position.Y, item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
        }

        /// <summary>
        ///     Invention event X Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventXNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechEvent item = GetSelectedItem() as TechEvent;
            if (item == null)
            {
                return;
            }

            if (eventPositionListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = eventPositionListView.SelectedIndices[0];

            TechPosition position = item.Positions[index];

            // Do nothing if the value does not change
            int x = (int) eventXNumericUpDown.Value;
            if (x == position.X)
            {
                return;
            }

            Log.Info("[Tech] Changed event position: ({0},{1}) -> ({2},{1}) [{3}]", position.X, position.Y, x, item);

            // Update value
            position.X = x;

            // Update items in the invention event coordinate list view
            eventPositionListView.Items[index].Text = IntHelper.ToString(x);

            // Move labels on the tech tree
            _techTreePanelController.UpdateItem(item, position);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            position.SetDirty(TechPositionItemId.X);

            // Change the font color
            eventXNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Invention event Y Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventYNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechEvent item = GetSelectedItem() as TechEvent;
            if (item == null)
            {
                return;
            }

            if (eventPositionListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = eventPositionListView.SelectedIndices[0];

            TechPosition position = item.Positions[index];

            // Do nothing if the value does not change
            int y = (int) eventYNumericUpDown.Value;
            if (y == position.Y)
            {
                return;
            }

            Log.Info("[Tech] Changed event position: ({0},{1}) -> ({0},{2}) [{3}]", position.X, position.Y, y, item);

            // Update value
            position.Y = y;

            // Update items in the invention event coordinate list view
            eventPositionListView.Items[index].SubItems[1].Text = IntHelper.ToString(y);

            // Move labels on the tech tree
            _techTreePanelController.UpdateItem(item, position);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            position.SetDirty(TechPositionItemId.Y);

            // Change the font color
            eventYNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the technical event coordinate addition button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventPositionAddButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechEvent item = GetSelectedItem() as TechEvent;
            if (item == null)
            {
                return;
            }

            // Add an item to the invention event coordinate list
            TechPosition position = new TechPosition { X = 0, Y = 0 };
            item.Positions.Add(position);

            Log.Info("[Tech] Added event position: ({0},{1}) [{2}]", position.X, position.Y, item);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();
            position.SetDirtyAll();

            // Add an item to the invention event coordinate list view
            ListViewItem li = new ListViewItem { Text = IntHelper.ToString(position.X) };
            li.SubItems.Add(IntHelper.ToString(position.Y));
            eventPositionListView.Items.Add(li);

            // Select the added item
            eventPositionListView.Items[eventPositionListView.Items.Count - 1].Focused = true;
            eventPositionListView.Items[eventPositionListView.Items.Count - 1].Selected = true;

            // Enable edit items
            EnableEventPositionItems();

            // Add a label to the tech tree
            _techTreePanelController.AddItem(item, position);
        }

        /// <summary>
        ///     Invention Event Coordinate processing when the button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEventPositionRemoveButtonClick(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            TechEvent item = GetSelectedItem() as TechEvent;
            if (item == null)
            {
                return;
            }

            if (eventPositionListView.SelectedIndices.Count == 0)
            {
                return;
            }
            int index = eventPositionListView.SelectedIndices[0];
            TechPosition position = item.Positions[index];

            Log.Info("[Tech] Removed event position: ({0},{1}) [{2}]", position.X, position.Y, item);

            // Remove an item from the invention event coordinate list
            item.Positions.RemoveAt(index);

            // Set the edited flag
            TechGroup grp = GetSelectedGroup();
            grp.SetDirty();
            item.SetDirty();

            // Invention event Remove an item from the coordinate list view
            eventPositionListView.Items.RemoveAt(index);

            if (index < techPositionListView.Items.Count)
            {
                // Select the next item after the deleted item
                eventPositionListView.Items[index].Focused = true;
                eventPositionListView.Items[index].Selected = true;
            }
            else if (index > 0)
            {
                // At the end of the list, select the item before the deleted item
                eventPositionListView.Items[eventPositionListView.Items.Count - 1].Focused = true;
                eventPositionListView.Items[eventPositionListView.Items.Count - 1].Selected = true;
            }
            else
            {
                // Disable edit items when there are no more items
                DisableEventPositionItems();
            }

            // Remove the technical tree label
            _techTreePanelController.RemoveItem(item, position);
        }

        #endregion
    }

    /// <summary>
    ///     Technical tree editor tab
    /// </summary>
    public enum TechEditorTab
    {
        Category, // category
        Tech, // technology
        Required, // Necessary technology
        Component, // Small study
        Effect, // Technical effect
        Label, // Technical label
        Event // Technical event
    }
}
