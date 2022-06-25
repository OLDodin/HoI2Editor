using System;

namespace HoI2Editor.Controls
{
    /// <summary>
    ///     List view item edit event parameters
    /// </summary>
    public class ListViewItemEditEventArgs : EventArgs
    {
        #region Public properties

        /// <summary>
        ///     Whether it was canceled
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        ///     Row index of list view item
        /// </summary>
        public int Row { get; private set; }

        /// <summary>
        ///     Column index of list view item
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        ///     Authenticity value after editing
        /// </summary>
        public bool Flag { get; private set; }

        /// <summary>
        ///     Edited string
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        ///     Index after selection
        /// </summary>
        public int Index { get; private set; }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="row">Row index of list view items</param>
        /// <param name="column">Column index of list view item</param>
        /// <param name="flag">Authenticity value after editing</param>
        public ListViewItemEditEventArgs(int row, int column, bool flag)
        {
            Row = row;
            Column = column;
            Flag = flag;
        }

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="row">Row index of list view items</param>
        /// <param name="column">Column index of list view item</param>
        /// <param name="text">Edited string</param>
        public ListViewItemEditEventArgs(int row, int column, string text)
        {
            Row = row;
            Column = column;
            Text = text;
        }

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="row">Row index of list view items</param>
        /// <param name="column">Column index of list view item</param>
        /// <param name="text">Edited string</param>
        /// <param name="index">Index after selection</param>
        public ListViewItemEditEventArgs(int row, int column, string text, int index)
        {
            Row = row;
            Column = column;
            Text = text;
            Index = index;
        }

        #endregion
    }
}
