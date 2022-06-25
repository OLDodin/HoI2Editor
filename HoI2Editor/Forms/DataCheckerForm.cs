using System;
using System.Windows.Forms;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Check result output form
    /// </summary>
    public partial class DataCheckerForm : Form
    {
        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public DataCheckerForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Form

        /// <summary>
        ///     Processing when the clear button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClearButtonClick(object sender, EventArgs e)
        {
            resultRichTextBox.Clear();
        }

        /// <summary>
        ///     Processing when the copy button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCopyButtonClick(object sender, EventArgs e)
        {
            resultRichTextBox.SelectAll();
            resultRichTextBox.Copy();
            resultRichTextBox.DeselectAll();
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

        #region Check result output

        /// <summary>
        ///     Output check result
        /// </summary>
        /// <param name="s">Target character string</param>
        /// <param name="args">Parameters</param>
        public void Write(string s, params object[] args)
        {
            string t = string.Format(s, args);
            resultRichTextBox.AppendText(t);
        }

        /// <summary>
        ///     Output check result
        /// </summary>
        /// <param name="s">Target character string</param>
        /// <param name="args">Parameters</param>
        public void WriteLine(string s, params object[] args)
        {
            string t = string.Format(s, args);
            resultRichTextBox.AppendText(t);
            resultRichTextBox.AppendText(Environment.NewLine);
        }

        /// <summary>
        ///     Output check result
        /// </summary>
        public void WriteLine()
        {
            resultRichTextBox.AppendText(Environment.NewLine);
        }

        #endregion
    }
}
