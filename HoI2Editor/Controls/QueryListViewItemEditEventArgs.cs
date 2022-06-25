using System;
using System.Collections.Generic;

namespace HoI2Editor.Controls
{
    /// <summary>
    ///     List view item Pre-edit event parameters
    /// </summary>
    public class QueryListViewItemEditEventArgs : EventArgs
    {
        #region Public properties

        /// <summary>
        ///     Row index of list view item
        /// </summary>
        public int Row { get; private set; }

        /// <summary>
        ///     Column index of list view item
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        ///     Item edit type
        /// </summary>
        public ItemEditType Type { get; set; }

        /// <summary>
        ///     Initial truth value
        /// </summary>
        public bool Flag { get; set; }

        /// <summary>
        ///     Initial character string
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     Initial index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///     List selection item list
        /// </summary>
        public IEnumerable<string> Items { get; set; }

        /// <summary>
        ///     Drop-down list width
        /// </summary>
        public int DropDownWidth { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="row">Row index of list view items</param>
        /// <param name="column">Column index of list view item</param>
        public QueryListViewItemEditEventArgs(int row, int column)
        {
            Row = row;
            Column = column;
        }

        #endregion
    }

    /// <summary>
    ///     Item edit type
    /// </summary>
    public enum ItemEditType
    {
        None, // No edit
        Bool, // Boolean value
        Text, // String editing
        List // List selection
    }
}
