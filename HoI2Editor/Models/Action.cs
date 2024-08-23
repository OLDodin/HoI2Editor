using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HoI2Editor.Models
{
    
    /// <summary>
    ///     Hoi Event Action
    /// </summary>
    public class EventAction
    {

        #region Public property

        /// <summary>
        ///     Action name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Action commands
        /// </summary>
        public List<Command> СommandList { get; }


        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public EventAction()
        {
            Name = "";
            СommandList = new List<Command>();
        }

        /// <summary>
        ///     Return action name from cvs by ID
        /// </summary>
        public string GetActionNameWithConverID()
        {
            if (Config.ExistsKey(Name))
            {
                return Config.GetText(Name);
            }
            return Name;
        }

     
        #endregion
    }
}
