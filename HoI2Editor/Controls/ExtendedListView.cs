using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HoI2Editor.Controls
{
    /// <summary>
    ///     Extended list view
    /// </summary>
    public partial class ExtendedListView : ListView
    {
        #region Public properties

        /// <summary>
        ///     Index of selected item
        /// </summary>
        public int SelectedIndex
        {
            get { return SelectedIndices.Count > 0 ? SelectedIndices[0] : -1; }
            set
            {
                foreach (int index in SelectedIndices.Cast<int>().Where(index => index != value))
                {
                    Items[index].Selected = false;
                    Items[index].Focused = false;
                }
                if ((value < 0) || (value >= Items.Count))
                {
                    return;
                }
                Items[value].Selected = true;
                Items[value].Focused = true;
            }
        }

        /// <summary>
        ///     Selected item
        /// </summary>
        public ListViewItem SelectedItem => SelectedItems.Count > 0 ? SelectedItems[0] : null;

        /// <summary>
        ///     Whether to support item swapping
        /// </summary>
        [Category("動作")]
        [DefaultValue(typeof (bool), "false")]
        [Description("ユーザーが項目の順番を再変更できるかどうかを示します。")]
        public bool AllowItemReorder
        {
            get { return _allowItemReorder; }
            set
            {
                _allowItemReorder = value;
                AllowDrop = value;
            }
        }

        [Category("動作")]
        [DefaultValue(typeof (bool), "false")]
        [Description("ユーザーによる項目の編集が可能になります。")]
        public bool ItemEdit { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Whether to support line swapping
        /// </summary>
        private bool _allowItemReorder;

        /// <summary>
        ///     Index of items being dragged and dropped
        /// </summary>
        private static readonly List<int> DragIndices = new List<int>();

        /// <summary>
        ///     Row index being edited
        /// </summary>
        private int _editingRowIndex;

        /// <summary>
        ///     Column index being edited
        /// </summary>
        private int _editingColumnIndex;

        #endregion

        #region Public event

        /// <summary>
        ///     Processing when replacing items
        /// </summary>
        [Category("動作")]
        [Description("項目の順番を再変更したときに発生します。")]
        public event EventHandler<ItemReorderedEventArgs> ItemReordered;

        [Category("動作")]
        [Description("ユーザーが項目の編集を始めたときに発生します。")]
        public event EventHandler<QueryListViewItemEditEventArgs> QueryItemEdit;

        [Category("動作")]
        [Description("ユーザーが項目を編集しようとしているときに発生します。")]
        public event EventHandler<ListViewItemEditEventArgs> BeforeItemEdit;


        [Category("動作")]
        [Description("ユーザーが項目を編集したときに発生します。")]
        public event EventHandler<ListViewItemEditEventArgs> AfterItemEdit;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public ExtendedListView()
        {
            InitializeComponent();
        }

        #endregion

        #region Event handler

        /// <summary>
        ///     Processing when double-clicking the mouse
        /// </summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            // Do nothing unless the click position is above the item
            ListViewHitTestInfo ht = HitTest(e.X, e.Y);
            if (ht.SubItem == null)
            {
                return;
            }

            int rowIndex = ht.Item.Index;
            int columnIndex = ht.Item.SubItems.IndexOf(ht.SubItem);

            // Inquire about the type of edit item
            QueryListViewItemEditEventArgs qe = new QueryListViewItemEditEventArgs(rowIndex, columnIndex);
            QueryItemEdit?.Invoke(this, qe);

            // Show controls for editing
            ShowEditControl(qe);
        }

        /// <summary>
        ///     Processing at the start of dragging
        /// </summary>
        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            base.OnItemDrag(e);

            // Do nothing if drag and drop item swapping is not allowed
            if (!AllowItemReorder)
            {
                return;
            }

            // Do nothing if there is no selection
            if (SelectedItems.Count == 0)
            {
                return;
            }

            // Save item index
            DragIndices.AddRange(SelectedIndices.Cast<int>());

            // Start drag and drop
            DoDragDrop(this, DragDropEffects.Move);
        }

        /// <summary>
        ///     Processing when the dragged item moves into the area
        /// </summary>
        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            // Do nothing if drag and drop item swapping is not allowed
            if (!AllowItemReorder)
            {
                return;
            }

            // ExtendedListView Do not allow drop unless it is an item of
            if (!e.Data.GetDataPresent(typeof (ExtendedListView)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            // Allow drop
            e.Effect = e.AllowedEffect;
        }

        /// <summary>
        ///     Processing when the dragged item moves within the area
        /// </summary>
        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);

            // Do nothing if drag and drop item swapping is not allowed
            if (!AllowItemReorder)
            {
                return;
            }

            // ExtendedListView Do not allow drop unless it is an item of
            if (!e.Data.GetDataPresent(typeof (ExtendedListView)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            // Display insert mark
            Point p = PointToClient(new Point(e.X, e.Y));
            int index = InsertionMark.NearestIndex(p);
            if (index < 0)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            e.Effect = e.AllowedEffect;
            Rectangle bounds = GetItemRect(index);
            InsertionMark.AppearsAfterItem = p.Y > bounds.Top + bounds.Height / 2;
            InsertionMark.Index = index;

            // Automatically scrolls by displaying the item at the insertion position
            Items[index].EnsureVisible();
        }

        /// <summary>
        ///     Processing when the dragged item moves out of the area
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);

            // Do nothing if drag and drop item swapping is not allowed
            if (!AllowItemReorder)
            {
                return;
            }

            // Hide insert mark
            InsertionMark.Index = -1;
        }

        /// <summary>
        ///     What to do when you drop an item
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            // Do nothing if drag and drop item swapping is not allowed
            if (!AllowItemReorder)
            {
                return;
            }

            // Do not allow drops unless it is your own item
            ExtendedListView listView = e.Data.GetData(typeof (ExtendedListView)) as ExtendedListView;
            if (listView != this)
            {
                return;
            }

            // Calculate the insertion position
            int index = InsertionMark.Index;
            if (index < 0)
            {
                return;
            }
            if (InsertionMark.AppearsAfterItem)
            {
                index++;
            }

            // Call the event handler
            ItemReorderedEventArgs re = new ItemReorderedEventArgs(DragIndices, index);
            ItemReordered?.Invoke(this, re);
            if (re.Cancel)
            {
                // Release the drag state
                DragIndices.Clear();
                return;
            }

            // Move items in the list view
            ListViewItem firstItem = null;
            foreach (int dragIndex in DragIndices)
            {
                ListViewItem item = (ListViewItem) Items[dragIndex].Clone();
                if (firstItem == null)
                {
                    firstItem = item;
                }
                Items.Insert(index, item);
                if (index < dragIndex)
                {
                    Items.RemoveAt(dragIndex + 1);
                    index++;
                }
                else
                {
                    Items.RemoveAt(dragIndex);
                }
            }

            // Select the item to move to
            if (firstItem != null)
            {
                firstItem.Selected = true;
                firstItem.Focused = true;
                EnsureVisible(firstItem.Index);
            }

            // Release the drag state
            DragIndices.Clear();
        }

        /// <summary>
        ///     Processing when updating drawing
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        #endregion

        #region Internal method

        /// <summary>
        ///     Show control for editing items
        /// </summary>
        /// <param name="e">Parameter of event before item editing</param>
        private void ShowEditControl(QueryListViewItemEditEventArgs e)
        {
            // Do nothing if no item is edited
            if (e.Type == ItemEditType.None)
            {
                return;
            }

            _editingRowIndex = e.Row;
            _editingColumnIndex = e.Column;

            ListViewItem item = Items[e.Row];
            ListViewItem.ListViewSubItem subItem = item.SubItems[e.Column];

            // Show control for editing items
            switch (e.Type)
            {
                case ItemEditType.Bool:
                    // Invert boolean values without displaying edit controls
                    InvertFlag(e.Flag);
                    break;

                case ItemEditType.Text:
                    ShowEditTextBox(e.Text, new Point(subItem.Bounds.Left, subItem.Bounds.Top),
                        new Size(Columns[e.Column].Width, subItem.Bounds.Height));
                    break;

                case ItemEditType.List:
                    ShowEditComboBox(e.Items, e.Index, new Point(subItem.Bounds.Left, subItem.Bounds.Top),
                        new Size(Columns[e.Column].Width, subItem.Bounds.Height), e.DropDownWidth);
                    break;
            }
        }

        /// <summary>
        ///     Invert the truth value of an item
        /// </summary>
        /// <param name="flag">Initial truth value</param>
        private void InvertFlag(bool flag)
        {
            ListViewItemEditEventArgs ie = new ListViewItemEditEventArgs(_editingRowIndex, _editingColumnIndex, !flag);
            BeforeItemEdit?.Invoke(this, ie);

            // Do not update items if canceled
            if (ie.Cancel)
            {
                return;
            }

            AfterItemEdit?.Invoke(this, ie);
        }

        /// <summary>
        ///     Display a text box for editing items
        /// </summary>
        /// <param name="text">Initial character string</param>
        /// <param name="location">Text box position</param>
        /// <param name="size">Text box size</param>
        private void ShowEditTextBox(string text, Point location, Size size)
        {
            InlineTextBox textBox = new InlineTextBox(text, location, size, this);
            textBox.FinishEdit += OnTextFinishEdit;
            Controls.Add(textBox);
        }

        /// <summary>
        ///     Display a combo box for editing items
        /// </summary>
        /// <param name="items">Item list</param>
        /// <param name="index">Initial index</param>
        /// <param name="location">Combo box position</param>
        /// <param name="size">Combo box size</param>
        /// <param name="dropDownWidth">Drop-down list width</param>
        private void ShowEditComboBox(IEnumerable<string> items, int index, Point location, Size size, int dropDownWidth)
        {
            InlineComboBox comboBox = new InlineComboBox(items, index, location, size, dropDownWidth, this);
            comboBox.FinishEdit += OnListFinishEdit;
            Controls.Add(comboBox);
        }

        /// <summary>
        ///     Processing when editing a character string
        /// </summary>
        private void OnTextFinishEdit(object sender, CancelEventArgs e)
        {
            InlineTextBox textBox = sender as InlineTextBox;
            if (textBox == null)
            {
                return;
            }
            string text = textBox.Text;

            // Delete the event handler
            textBox.FinishEdit -= OnTextFinishEdit;

            // Delete the text box for editing
            Controls.Remove(textBox);

            // Do not update items if canceled
            if (e.Cancel)
            {
                return;
            }

            ListViewItem item = Items[_editingRowIndex];
            ListViewItem.ListViewSubItem subItem = item.SubItems[_editingColumnIndex];

            ListViewItemEditEventArgs ie = new ListViewItemEditEventArgs(_editingRowIndex, _editingColumnIndex,
                textBox.Text);
            BeforeItemEdit?.Invoke(this, ie);

            // Do not update items if canceled
            if (ie.Cancel)
            {
                return;
            }

            // Update the item string
            subItem.Text = text;

            AfterItemEdit?.Invoke(this, ie);
        }

        /// <summary>
        ///     Processing when editing a list
        /// </summary>
        private void OnListFinishEdit(object sender, CancelEventArgs e)
        {
            InlineComboBox comboBox = sender as InlineComboBox;
            if (comboBox == null)
            {
                return;
            }
            string s = comboBox.Text;
            int index = comboBox.SelectedIndex;

            // Delete the event handler
            comboBox.FinishEdit -= OnListFinishEdit;

            // Delete the editing combo box
            Controls.Remove(comboBox);

            // Do not update items if canceled
            if (e.Cancel)
            {
                return;
            }

            ListViewItem item = Items[_editingRowIndex];
            ListViewItem.ListViewSubItem subItem = item.SubItems[_editingColumnIndex];

            ListViewItemEditEventArgs ie = new ListViewItemEditEventArgs(_editingRowIndex, _editingColumnIndex, s, index);
            BeforeItemEdit?.Invoke(this, ie);

            // Do not update items if canceled
            if (ie.Cancel)
            {
                return;
            }

            // Update the item string
            subItem.Text = comboBox.Items[index].ToString();

            AfterItemEdit?.Invoke(this, ie);
        }

        #endregion
    }
}
