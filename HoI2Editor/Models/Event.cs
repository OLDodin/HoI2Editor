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
        ///     Event country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        ///     Event action names
        /// </summary>
        public List<string> ActionNames { get; }

        /// <summary>
        ///     Event path
        /// </summary>
        public string PathName { get; set; }

        /// <summary>
        ///     All Event Text
        /// </summary>
        public string EventText { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public Event()
        {
            ActionNames = new List<string>();
            Name = "";
            Desc = "";
            Country = "";
            EventText = "";
        }

        /// <summary>
        ///     Return event name from cvs by ID
        /// </summary>
        public string GetEventNameWithConverID()
        {
            if (Config.ExistsKey(Name))
            {
                return Config.GetText(Name);
            }
            return Name;
        }

        /// <summary>
        ///     Return event desc from cvs by ID
        /// </summary>
        public string GetEventDescWithConverID()
        {
            if (Config.ExistsKey(Desc))
            {
                return Config.GetText(Desc);
            }
            return Name;
        }

        /// <summary>
        ///     Return action name from cvs by ID
        /// </summary>
        /// <param name="actionIndex">action index</param>
        public string GetActionNameWithConverID(int actionIndex)
        {
            if (actionIndex >= ActionNames.Count)
                return "";
            string actionName = ActionNames[actionIndex];
            if (Config.ExistsKey(actionName))
            {
                return Config.GetText(actionName);
            }
            return actionName;
        }

        /// <summary>
        ///     ToString
        /// </summary>
        public override string ToString()
        {
            return Id.ToString() + "  " + GetEventNameWithConverID();
        }
        #endregion
    }
}
