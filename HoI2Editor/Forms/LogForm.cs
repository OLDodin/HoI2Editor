using System;
using System.Windows.Forms;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Log output form
    /// </summary>
    public partial class LogForm : Form
    {
        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public LogForm()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Processing when the close button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            Hide();
        }

        #endregion

        #region Log operation

        /// <summary>
        ///     Output log
        /// </summary>
        /// <param name="s">Target character string</param>
        public void Write(string s)
        {
            if (!Visible)
            {
                logRichTextBox.Clear();
                Show();
            }
            logRichTextBox.AppendText(s);
        }

        /// <summary>
        ///     Processing when the copy button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCopyButtonClick(object sender, EventArgs e)
        {
            logRichTextBox.SelectAll();
            logRichTextBox.Copy();
            logRichTextBox.DeselectAll();
        }

        /// <summary>
        ///     Processing when the clear button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClearButtonClick(object sender, EventArgs e)
        {
            logRichTextBox.Clear();
        }

        #endregion
    }
}
