using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace HoI2Editor.Controls
{
    /// <summary>
    ///     Item sort event parameters
    /// </summary>
    public class ItemReorderedEventArgs : CancelEventArgs
    {
        #region Public properties

        /// <summary>
        ///     Previous display position
        /// </summary>
        public int[] OldDisplayIndices { get; private set; }

        /// <summary>
        ///     New display position
        /// </summary>
        public int NewDisplayIndex { get; private set; }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="oldDisplayIndices">Previous display position</param>
        /// <param name="newDisplayIndex">New display position</param>
        public ItemReorderedEventArgs(IEnumerable<int> oldDisplayIndices, int newDisplayIndex)
        {
            OldDisplayIndices = oldDisplayIndices.ToArray();
            NewDisplayIndex = newDisplayIndex;
        }

        #endregion
    }
}
