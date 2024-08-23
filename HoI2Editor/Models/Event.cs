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
        ///     Event picture
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        ///     Event actions
        /// </summary>
        public List<EventAction> Actions { get; }

        /// <summary>
        ///     Event triggers
        /// </summary>
        public List<Trigger> Triggers { get; }

        /// <summary>
        ///     Event decision
        /// </summary>
        public List<Trigger> Decision { get; }

        /// <summary>
        ///     Event decision trigger
        /// </summary>
        public List<Trigger> DecisionTriggers { get; }

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
            Actions = new List<EventAction>();
            Triggers = new List<Trigger>();
            Decision = new List<Trigger>();
            DecisionTriggers = new List<Trigger>();
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
            if (actionIndex >= Actions.Count)
                return "";
            return Actions[actionIndex].GetActionNameWithConverID();
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
