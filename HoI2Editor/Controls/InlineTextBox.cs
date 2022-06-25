using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HoI2Editor.Controls
{
    /// <summary>
    ///     Text box for editing items
    /// </summary>
    [ToolboxItem(false)]
    public partial class InlineTextBox : TextBox
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
        /// <param name="text">Initial character string</param>
        /// <param name="location">Coordinate</param>
        /// <param name="size">size</param>
        /// <param name="parent">Parent control</param>
        public InlineTextBox(string text, Point location, Size size, Control parent)
        {
            InitializeComponent();

            Init(text, location, size, parent);
        }

        /// <summary>
        ///     Initialization process
        /// </summary>
        /// <param name="text">Initial character string</param>
        /// <param name="location">Coordinate</param>
        /// <param name="size">size</param>
        /// <param name="parent">Parent control</param>
        private void Init(string text, Point location, Size size, Control parent)
        {
            Parent = parent;
            Location = location;
            Size = size;
            Text = text;
            Multiline = false;

            // Select all strings
            SelectAll();

            // Set focus
            Focus();
        }

        #endregion

        #region Event handler

        /// <summary>
        ///     Processing when a key is pressed
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter)
            {
                Finish(false);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Finish(true);
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        ///     Processing when defocusing
        /// </summary>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
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
