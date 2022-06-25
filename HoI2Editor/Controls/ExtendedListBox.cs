using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HoI2Editor.Controls
{
    public partial class ExtendedListBox : ListBox
    {
        #region Public properties

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

        #endregion

        #region Internal field

        /// <summary>
        ///     Whether to support line swapping
        /// </summary>
        private bool _allowItemReorder;

        /// <summary>
        ///     Drag and drop start position
        /// </summary>
        private static Point _dragPoint = Point.Empty;

        /// <summary>
        ///     Index of items being dragged and dropped
        /// </summary>
        private static readonly List<int> DragIndices = new List<int>();

        #endregion

        #region Public event

        /// <summary>
        ///     Processing when replacing items
        /// </summary>
        [Category("動作")]
        [Description("項目の順番を再変更したときに発生します。")]
        public event EventHandler<ItemReorderedEventArgs> ItemReordered;

        #endregion

        #region Initialization

        /// <summary>
        ///     Extended list box
        /// </summary>
        public ExtendedListBox()
        {
            InitializeComponent();
        }

        #endregion

        #region Event handler

        /// <summary>
        ///     Processing when mouse down
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Do nothing if drag and drop item swapping is not allowed
            if (!AllowItemReorder)
            {
                return;
            }

            // Do nothing if the item at the cursor position is not selected
            int index = IndexFromPoint(e.X, e.Y);
            if (index < 0)
            {
                return;
            }
            if (!SelectedIndices.Contains(index))
            {
                return;
            }

            // If the left button is not down, the drag state is canceled.
            if (e.Button != MouseButtons.Left)
            {
                _dragPoint = Point.Empty;
                DragIndices.Clear();
                return;
            }

            // Set the drag start position
            _dragPoint = new Point(e.X, e.Y);

            // Save item index
            DragIndices.AddRange(SelectedIndices.Cast<int>());
        }

        /// <summary>
        ///     Processing when mouse up
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            // Do nothing if drag and drop item swapping is not allowed
            if (!AllowItemReorder)
            {
                return;
            }

            // Release the drag state
            _dragPoint = Point.Empty;
            DragIndices.Clear();
        }

        /// <summary>
        ///     Processing when moving the mouse
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Do nothing if drag and drop item swapping is not allowed
            if (!AllowItemReorder)
            {
                return;
            }

            // Do nothing unless you are dragging
            if (_dragPoint == Point.Empty)
            {
                return;
            }

            // Do nothing if the drag judgment size is not exceeded
            Size dragSize = SystemInformation.DragSize;
            Rectangle dragRect = new Rectangle(_dragPoint.X - dragSize.Width / 2, _dragPoint.Y - dragSize.Height / 2,
                dragSize.Width, dragSize.Height);
            if (dragRect.Contains(e.X, e.Y))
            {
                return;
            }

            // Start drag and drop
            DoDragDrop(this, DragDropEffects.Move);
        }

        /// <summary>
        ///     Processing when the dragged item moves into the area
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            // Do nothing if drag and drop item swapping is not allowed
            if (!AllowItemReorder)
            {
                return;
            }

            // ExtendedListBox Do not allow drop unless it is an item of
            if (!e.Data.GetDataPresent(typeof (ExtendedListBox)))
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

            // ExtendedListBox Do not allow drop unless it is an item of
            if (!e.Data.GetDataPresent(typeof (ExtendedListBox)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            // Do not allow drop if there is no item at the insertion position
            Point p = PointToClient(new Point(e.X, e.Y));
            int index = IndexFromPoint(p);
            if (index < 0)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            // Automatically scroll when dragged to the edge of the display area
            if (index > 0 && p.Y < ItemHeight)
            {
                TopIndex = index - 1;
            }
            else if (index < Items.Count - 1 && p.Y > ClientRectangle.Height - ItemHeight)
            {
                if (TopIndex + ClientRectangle.Height / ItemHeight < Items.Count)
                {
                    TopIndex = TopIndex + 1;
                }
            }

            // Do not allow drop if the item at the insertion position is the target of dragging
            if (DragIndices.Contains(index))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = e.AllowedEffect;
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
            ExtendedListBox listBox = e.Data.GetData(typeof (ExtendedListBox)) as ExtendedListBox;
            if (listBox != this)
            {
                return;
            }

            // Do not allow drop if there is no item at the insertion position
            Point p = PointToClient(new Point(e.X, e.Y));
            int index = IndexFromPoint(p);
            if (index < 0)
            {
                _dragPoint = Point.Empty;
                DragIndices.Clear();
                return;
            }

            // Do not allow drop if the item at the insertion position is the target of dragging
            if (DragIndices.Contains(index))
            {
                return;
            }

            // Call the event handler
            ItemReorderedEventArgs re = new ItemReorderedEventArgs(DragIndices, index);
            ItemReordered?.Invoke(this, re);
            if (re.Cancel)
            {
                // Release the drag state
                _dragPoint = Point.Empty;
                DragIndices.Clear();
                return;
            }

            // Move items in the list box
            foreach (int dragIndex in DragIndices)
            {
                Items.Insert(index, Items[dragIndex]);
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

            // Release the drag state
            _dragPoint = Point.Empty;
            DragIndices.Clear();
        }

        /// <summary>
        ///     Processing when updating drawing
        /// </summary>
        /// <param name="pe"></param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        #endregion
    }
}
