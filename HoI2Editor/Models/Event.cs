using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HoI2Editor.Models
{
    
    /// <summary>
    ///     Hoi Event
    /// </summary>
    public class Event
    {

        #region Public property

        /// <summary>
        ///     Event ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Event name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Event description
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        ///     Event action names
        /// </summary>
        public List<string> ActionNames { get; }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public Event()
        {
            ActionNames = new List<string>();
        }
        #endregion
    }
}
