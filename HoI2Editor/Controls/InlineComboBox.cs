using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HoI2Editor.Controls
{
    /// <summary>
    ///     Item editing combo box
    /// </summary>
    [ToolboxItem(false)]
    public partial class InlineComboBox : ComboBox
    {
        #region Public event

        /// <summary>
        ///     Processing when item editing is completed
        /// </summary>
        [Category("動作")]
        [Description("項目の編集を完了したときに発生します。")]
        public event EventHandler<CancelEventArgs> FinishEdit;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="items">Item list</param>
        /// <param name="index">Initial index</param>
        /// <param name="location">Coordinate</param>
        /// <param name="size">size</param>
        /// <param name="parent">Parent control</param>
        /// <param name="dropDownWidth">Drop-down list width</param>
        public InlineComboBox(IEnumerable<string> items, int index, Point location, Size size, int dropDownWidth,
            Control parent)
        {
            InitializeComponent();

            Init(items, index, location, size, dropDownWidth, parent);
        }

        /// <summary>
        ///     Initialization process
        /// </summary>
        /// <param name="items">Item list</param>
        /// <param name="index">Initial index</param>
        /// <param name="location">Coordinate</param>
        /// <param name="size">size</param>
        /// <param name="parent">Parent control</param>
        /// <param name="dropDownWidth">Drop-down list width</param>
        private void Init(IEnumerable<string> items, int index, Point location, Size size, int dropDownWidth,
            Control parent)
        {
            Parent = parent;
            Location = location;
            Size = size;
            foreach (string s in items)
            {
                Items.Add(s);
            }
            SelectedIndex = index;
            DropDownStyle = ComboBoxStyle.DropDownList;
            DropDownWidth = dropDownWidth > size.Width ? dropDownWidth : size.Width;

            // Open drop-down list
            DroppedDown = true;
        }

        #endregion

        #region Event handler

        /// <summary>
        ///     Processing when a key is pressed
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape)
            {
                Finish(true);
            }
        }

        /// <summary>
        ///     Processing when defocusing
        /// </summary>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Finish(true);
        }

        /// <summary>
        ///     Processing when changing selection items
        /// </summary>
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Finish(false);
        }

        /// <summary>
        ///     What to do when the drop-down list is closed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);

            // I can't think of a way to distinguish between out-of-area clicks and item selection, so I always consider it updated.
            Finish(false);
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
        ///     Processing when editing is completed
        /// </summary>
        /// <param name="cancel">Whether it was canceled</param>
        private void Finish(bool cancel)
        {
            CancelEventArgs e = new CancelEventArgs(cancel);
            FinishEdit?.Invoke(this, e);
        }

        #endregion
    }
}
