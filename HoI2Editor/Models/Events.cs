using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HoI2Editor.Parsers;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;
using HoI2Editor.Writers;

namespace HoI2Editor.Models
{
    public class StartEventsLoadParams
    {
        public int TextCodePage { get; }
        public List<string> FilesToLoad { get; }

        public StartEventsLoadParams(int textCodePage, List<string> filesToLoad)
        {
            TextCodePage = textCodePage;
            FilesToLoad = filesToLoad;
        }
    }
    /// <summary>
    ///     Event data group
    /// </summary>
    public static class Events
    {
        /// <summary>
        ///     Events list
        /// </summary>
        public static List<Event> TotalEventsList { get; }

        /// <summary>
        ///     Readed flag
        /// </summary>
        private static bool _loaded;

        /// <summary>
        ///     For delay reading
        /// </summary>
        private static readonly BackgroundWorker Worker = new BackgroundWorker();

        /// <summary>
        ///     Last load parameters
        /// </summary>
        private static StartEventsLoadParams LastLoadParams = null;

        /// <summary>
        ///     Last complete handler
        /// </summary>
        private static RunWorkerCompletedEventHandler _completeHandler = null;

        /// <summary>
        ///     Last progress handler
        /// </summary>
        private static ProgressChangedEventHandler _progressHandler = null;

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Events()
        {
            TotalEventsList = new List<Event>();
            Worker.WorkerReportsProgress = true;

            Worker.DoWork += OnWorkerDoWork;
        }

        /// <summary>
        ///     Set  handlers for BackgroundWorker
        /// </summary>
        public static void SetHandlers(RunWorkerCompletedEventHandler completeHandler, ProgressChangedEventHandler progressHandler)
        {
            // Register the reading event handler
            if (completeHandler != null)
            {
                if (_completeHandler != null)
                {
                    Worker.RunWorkerCompleted -= _completeHandler;
                }
                _completeHandler = completeHandler;
                Worker.RunWorkerCompleted += completeHandler;
            }
            if (progressHandler != null)
            {
                if (_progressHandler != null)
                {
                    Worker.ProgressChanged -= _progressHandler;
                }
                _progressHandler = progressHandler;
                Worker.ProgressChanged += progressHandler;
            }
        }

        /// <summary>
        ///     Request a relay of event files
        /// </summary>
        public static void RequestReload()
        {
            _loaded = false;

            TotalEventsList.Clear();
        }

        /// <summary>
        ///     Reload the event files
        /// </summary>
        public static void Reload()
        {
            // Do nothing before reading
            if (!_loaded)
            {
                return;
            }

            _loaded = false;
            TotalEventsList.Clear();

            Load();
        }

        /// <summary>
        ///     Read the event files
        /// </summary>
        public static void Load()
        {
            // If you have read it, go back
            if (_loaded)
            {
                return;
            }

            // Wait for completion if you are in the middle
            if (Worker.IsBusy)
            {
                WaitLoading();
                return;
            }

            if (LastLoadParams != null)
            {
                Load(null, LastLoadParams.TextCodePage, LastLoadParams.FilesToLoad);
            }
        }

        /// <summary>
        ///     Delayed the event files
        /// </summary>
        /// <param name="handler">Reading completion event handler</param>
        public static void LoadAsync(int textCodePage, List<string> filesToLoad)
        {
            StartEventsLoadParams startParams = new StartEventsLoadParams(textCodePage, filesToLoad);
            // If you have already loaded, call the completed event handler
            if (_loaded)
            {
                _completeHandler?.Invoke(null, new RunWorkerCompletedEventArgs(startParams, null, false));
                return;
            }

            // Return if you are in the middle
            if (Worker.IsBusy)
            {
                return;
            }
            
            LastLoadParams = startParams;
            // Start late reading
            Worker.RunWorkerAsync(startParams);
        }

        /// <summary>
        ///     Wait until the loading is completed
        /// </summary>
        public static void WaitLoading()
        {
            while (Worker.IsBusy)
            {
                Application.DoEvents();
            }
        }

        /// <summary>
        ///     Judge whether or not to read delayed
        /// </summary>
        /// <returns>Return True if you are ready to read</returns>
        public static bool IsLoading()
        {
            return Worker.IsBusy;
        }

        /// <summary>
        ///     Late reading processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            StartEventsLoadParams startParams = (StartEventsLoadParams)e.Argument;
            Load(worker, startParams.TextCodePage, startParams.FilesToLoad);
        }

        /// <summary>
        ///     Read the events files
        /// </summary>
        /// <param name="textCodePage">text file encoding</param>
        /// <param name="customEventDirPath">Custom events folder</param>
        public static void LoadAll(int textCodePage, string customEventDirPath)
        {
            string eventPathName = Game.EventsPathName;
            if (!string.IsNullOrEmpty(customEventDirPath))
                eventPathName = customEventDirPath;
            string pathName = Game.GetReadFileName(eventPathName);
            try
            {
                TotalEventsList.Clear();
                LoadFiles(pathName, textCodePage);
            }
            catch (Exception)
            {
                Log.Error("[Event] Read error: {0}", pathName);
                if (MessageBox.Show($"{Resources.FileReadError}: {pathName}",
                    Resources.EditorTech, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                    == DialogResult.Cancel)
                {
                    return;
                }
            }
        }

        /// <summary>
        ///     Read the events files
        /// </summary>
        /// <param name="pathName">Event folder</param>
        /// <param name="textCodePage">text file encoding</param>
        private static void LoadFiles(string pathName, int textCodePage)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(pathName);
            foreach (FileInfo fi in dirInfo.GetFiles())
            {
                LoadFile(fi.FullName, textCodePage);
            }

            foreach (DirectoryInfo diSourceSubDir in dirInfo.GetDirectories())
            {
                LoadFiles(diSourceSubDir.FullName, textCodePage);
            }
        }
        /// <summary>
        ///     Read the events files
        /// </summary>
        /// <param name="textCodePage">text file encoding</param>
        /// <param name="customEventDirPath">Custom events folder</param>
        public static void Load(BackgroundWorker worker, int textCodePage, List<string> filesToLoad)
        {
            TotalEventsList.Clear();
            for (int  i = 0; i < filesToLoad.Count; i++)
            {
                string pathName = Game.GetReadFileName(filesToLoad[i]);
                try
                {
                    if (File.Exists(pathName))
                        LoadFile(pathName, textCodePage);
                    if (worker != null)
                        worker.ReportProgress((int)(((float)i*100)/(float)(filesToLoad.Count)));
                }
                catch (Exception)
                {
                    Log.Error("[Event] Read error: {0}", pathName);
                    if (MessageBox.Show($"{Resources.FileReadError}: {pathName}",
                        Resources.EditorTech, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                        == DialogResult.Cancel)
                    {
                        return;
                    }
                }
            }

            foreach (Event currentHoi2Event in TotalEventsList)
            {
                VerifyTriggers(currentHoi2Event.Triggers, currentHoi2Event.PathName, TriggerType.None);
                VerifyTriggers(currentHoi2Event.Decision, currentHoi2Event.PathName, TriggerType.None);
                VerifyTriggers(currentHoi2Event.DecisionTriggers, currentHoi2Event.PathName, TriggerType.None);

                VerifyCommands(currentHoi2Event);
            }
        }

        /// <summary>
        ///     Read the event file
        /// </summary>
        /// <param name="fileName">Event file name</param>
        /// <param name="textCodePage">text file encoding</param>
        private static void LoadFile(string fileName, int textCodePage)
        {
            if (Path.GetFileName(fileName) == "event commands.txt")
            {
                Log.Verbose("[Event] Skip: {0}", Path.GetFileName(fileName));
                return;
            }
            Log.Verbose("[Event] Load: {0}", Path.GetFileName(fileName));

            List<Event> fileEventsList = EventParser.Parse(fileName, textCodePage);
            TotalEventsList.AddRange(fileEventsList);
            if (fileEventsList == null)
            {
                Log.Error("[Event] Read error: {0}", Path.GetFileName(fileName));
                return;
            }
        }

        /// <summary>
        ///     Get event list by country tag
        /// </summary>
        /// <param name="countryName">country tag for event</param>
        public static List<Event> GetCountryEventList(string countryName)
        {
            List<Event> countryEventList = new List<Event>();
            foreach (Event hoi2Event in TotalEventsList)
            {
                if (hoi2Event.Country == countryName)
                {
                    countryEventList.Add(hoi2Event);
                }
            }
            return countryEventList;
        }

        /// <summary>
        ///     Get event list without country tag
        /// </summary>
        public static List<Event> GetCommonEventList()
        {
            List<Event> commonEventList = new List<Event>();
            foreach (Event hoi2Event in TotalEventsList)
            {
                if (string.IsNullOrEmpty(hoi2Event.Country))
                {
                    commonEventList.Add(hoi2Event);
                }
            }
            return commonEventList;
        }

        /// <summary>
        ///     check semantic commands
        /// </summary>
        /// <param name="hoi2Event">event with commands</param>
        private static void VerifyCommands(Event hoi2Event)
        {
            foreach (EventAction eventAction in hoi2Event.Actions)
            {
                foreach (Command eventCommand in eventAction.СommandList)
                {
                    if (eventCommand.Type == CommandType.Trigger || eventCommand.Type == CommandType.SleepEvent || eventCommand.Type == CommandType.Event)
                    {
                        if (eventCommand.Which is double)
                        {
                            int eventID = (int)((double)eventCommand.Which);
                            bool eventExist = false;
                            foreach (Event searchingEvent in TotalEventsList)
                            {
                                if (searchingEvent.Id == eventID)
                                {
                                    eventExist = true;
                                    break;
                                }
                            }
                            if (!eventExist)
                            {
                                Log.Error("[Event] Command with non exist eventID ({0})  {1}:  L{2}", eventID, hoi2Event.PathName, eventCommand.LineNum);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     check semantic triggers
        /// </summary>
        /// <param name="triggersList">event triggers</param>
        /// <param name="eventPathName">event Path</param>
        /// <param name="parentType">trigger parent type</param>
        private static void VerifyTriggers(List<Trigger> triggersList, string eventPathName, TriggerType parentType)
        {
            foreach (Trigger eventTrigger in triggersList)
            {
                if (eventTrigger.Value.GetType() == typeof(List<Trigger>))
                {
                    VerifyTriggers((List<Trigger>)eventTrigger.Value, eventPathName, eventTrigger.Type);
                }
                else if (eventTrigger.Type == TriggerType.Day)
                {
                    if (eventTrigger.Value is double)
                    {
                        double day = (double)eventTrigger.Value;
                        if (day < 0 || day > 29)
                            Log.Warning("[Event-trigger] Day ({0}) must be from [0-29] {1}:  L{2}", day, eventPathName, eventTrigger.LineNum);
                    }
                }
                else if (eventTrigger.Type == TriggerType.Event || (parentType == TriggerType.Event && eventTrigger.Type == TriggerType.Id))
                {
                    if (eventTrigger.Value is double)
                    {
                        int eventID = (int)((double)eventTrigger.Value);
                        bool eventExist = false;
                        foreach (Event searchingEvent in TotalEventsList)
                        {
                            if (searchingEvent.Id == eventID)
                            {
                                eventExist = true;
                                break;
                            }
                        }
                        if (!eventExist)
                        {
                            Log.Error("[Event-trigger] Trigger with non exist eventID ({0})  {1}:  L{2}", eventID, eventPathName, eventTrigger.LineNum);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Verify events refers to exist minister, leader, tech teams
        /// </summary>
        /// <param name="ministers">parsed ministers</param>
        /// <param name="leaders">parsed leaders</param>
        /// <param name="techTeams">parsed tech teams</param>
        public static void VerifyEventsByIDs(List<Minister> ministers, List<Leader> leaders, List<Team> techTeams, List<int> techIDs)
        {
            foreach (Event currentHoi2Event in TotalEventsList)
            {
                VerifyTriggersByIDs(ministers, leaders, techTeams, techIDs, currentHoi2Event.Triggers, currentHoi2Event.PathName, TriggerType.None);
                VerifyTriggersByIDs(ministers, leaders, techTeams, techIDs, currentHoi2Event.Decision, currentHoi2Event.PathName, TriggerType.None);
                VerifyTriggersByIDs(ministers, leaders, techTeams, techIDs, currentHoi2Event.DecisionTriggers, currentHoi2Event.PathName, TriggerType.None);

                VerifyCommandsByIDs(currentHoi2Event, ministers, leaders, techTeams, techIDs);
            }
        }

        /// <summary>
        ///     Verify events commands refers to exist minister, leader, tech teams
        /// </summary>
        /// <param name="hoi2Event">event with commands</param>
        /// <param name="ministers">parsed ministers</param>
        /// <param name="leaders">parsed leaders</param>
        /// <param name="techTeams">parsed tech teams</param>
        private static void VerifyCommandsByIDs(Event hoi2Event, List<Minister> ministers, List<Leader> leaders, List<Team> techTeams, List<int> techIDs)
        {
            foreach (EventAction eventAction in hoi2Event.Actions)
            {
                foreach (Command eventCommand in eventAction.СommandList)
                {
                    if (eventCommand.Type == CommandType.SleepMinister || eventCommand.Type == CommandType.WakeMinister)
                    {
                        if (eventCommand.Which is double)
                        {
                            int someID = (int)((double)eventCommand.Which);
                            if (someID > 0)
                            {
                                bool idExist = false;
                                foreach (Minister searchingObj in ministers)
                                {
                                    if (searchingObj.Id == someID)
                                    {
                                        idExist = true;
                                        break;
                                    }
                                }
                                if (!idExist)
                                {
                                    Log.Error("[Event] Command with non exist minister ID ({0})  {1}:  L{2}", someID, hoi2Event.PathName, eventCommand.LineNum);
                                }
                            }
                        }
                    }
                    else if (eventCommand.Type == CommandType.SleepLeader || eventCommand.Type == CommandType.WakeLeader || eventCommand.Type == CommandType.SetLeaderSkill
                        || eventCommand.Type == CommandType.AddLeaderSkill || eventCommand.Type == CommandType.AddLeaderTrait || eventCommand.Type == CommandType.RemoveLeaderTrait
                        || eventCommand.Type == CommandType.ChangeLeaderXp || eventCommand.Type == CommandType.SetLeaderXp
                        || eventCommand.Type == CommandType.AddCorps)
                    {
                        int someID = -1;
                        if (eventCommand.Type == CommandType.AddCorps)
                        {
                            if (eventCommand.When is double)
                            {
                                someID = (int)((double)eventCommand.When);
                            }
                        }
                        else
                        {
                            if (eventCommand.Which is double)
                            {
                                someID = (int)((double)eventCommand.Which);
                            }
                        }
                        if (someID > 0)
                        {
                            bool idExist = false;
                            foreach (Leader searchingObj in leaders)
                            {
                                if (searchingObj.Id == someID)
                                {
                                    idExist = true;
                                    break;
                                }
                            }
                            if (!idExist)
                            {
                                Log.Error("[Event] Command with non exist leader ID ({0})  {1}:  L{2}", someID, hoi2Event.PathName, eventCommand.LineNum);
                            }
                        }
                    }
                    else if (eventCommand.Type == CommandType.SetTeamSkill || eventCommand.Type == CommandType.AddTeamSkill || eventCommand.Type == CommandType.AddTeamResearchType
                        || eventCommand.Type == CommandType.RemoveTeamResearchType || eventCommand.Type == CommandType.SleepTeam || eventCommand.Type == CommandType.WakeTeam
                        || eventCommand.Type == CommandType.GiveTeam)
                    {
                        if (eventCommand.Which is double)
                        {
                            int someID = (int)((double)eventCommand.Which);
                            if (someID > 0)
                            {
                                bool idExist = false;
                                foreach (Team searchingObj in techTeams)
                                {
                                    if (searchingObj.Id == someID)
                                    {
                                        idExist = true;
                                        break;
                                    }
                                }
                                if (!idExist)
                                {
                                    Log.Error("[Event] Command with non exist team ID ({0})  {1}:  L{2}", someID, hoi2Event.PathName, eventCommand.LineNum);
                                }
                            }
                        }
                    }
                    else if (eventCommand.Type == CommandType.GainTech || eventCommand.Type == CommandType.Deactivate 
                        || eventCommand.Type == CommandType.InfoMayCause || eventCommand.Type == CommandType.Activate)
                    {
                        if (eventCommand.Which is double)
                        {
                            int someID = (int)((double)eventCommand.Which);
                            if (someID > 0)
                            {
                                bool idExist = false;
                                foreach (int techID in techIDs)
                                {
                                    if (techID == someID)
                                    {
                                        idExist = true;
                                        break;
                                    }
                                }
                                if (!idExist)
                                {
                                    Log.Error("[Event] Command with non exist technology ID ({0})  {1}:  L{2}", someID, hoi2Event.PathName, eventCommand.LineNum);
                                }
                            }
                        }
                    }
                }
            }  
        }

        /// <summary>
        ///     Verify events triggers refers to exist minister, leader, tech teams
        /// </summary>
        /// <param name="ministers">parsed ministers</param>
        /// <param name="leaders">parsed leaders</param>
        /// <param name="techTeams">parsed tech teams</param>
        /// <param name="triggersList">event triggers</param>
        /// <param name="eventPathName">event Path</param>
        /// <param name="parentType">trigger parent type</param>
        private static void VerifyTriggersByIDs(List<Minister> ministers, List<Leader> leaders, List<Team> techTeams, List<int> techIDs, List<Trigger> triggersList, string eventPathName, TriggerType parentType)
        {
            foreach (Trigger eventTrigger in triggersList)
            {
                if (eventTrigger.Value.GetType() == typeof(List<Trigger>))
                {
                    VerifyTriggersByIDs(ministers, leaders, techTeams, techIDs, (List<Trigger>)eventTrigger.Value, eventPathName, eventTrigger.Type);
                }
                else if (eventTrigger.Type == TriggerType.HeadOfState || eventTrigger.Type == TriggerType.HeadOfGovernment
                    || eventTrigger.Type == TriggerType.Minister || eventTrigger.Type == TriggerType.InCabinet)
                {
                    if (eventTrigger.Value is double)
                    {
                        int someID = (int)((double)eventTrigger.Value);
                        bool idExist = false;
                        foreach (Minister searchingObj in ministers)
                        {
                            if (searchingObj.Id == someID)
                            {
                                idExist = true;
                                break;
                            }
                        }
                        if (!idExist)
                        {
                            Log.Error("[Event-trigger] Trigger with non exist minister ID ({0})  {1}:  L{2}", someID, eventPathName, eventTrigger.LineNum);
                        }
                    }
                }
                else if (eventTrigger.Type == TriggerType.Leader)
                {
                    if (eventTrigger.Value is double)
                    {
                        int someID = (int)((double)eventTrigger.Value);
                        bool idExist = false;
                        foreach (Leader searchingObj in leaders)
                        {
                            if (searchingObj.Id == someID)
                            {
                                idExist = true;
                                break;
                            }
                        }
                        if (!idExist)
                        {
                            Log.Error("[Event-trigger] Trigger with non exist leader ID ({0})  {1}:  L{2}", someID, eventPathName, eventTrigger.LineNum);
                        }
                    }
                }
                else if (eventTrigger.Type == TriggerType.TechTeam || (parentType == TriggerType.TechTeam && eventTrigger.Type == TriggerType.Id))
                {
                    if (eventTrigger.Value is double)
                    {
                        int someID = (int)((double)eventTrigger.Value);
                        bool idExist = false;
                        foreach (Team searchingObj in techTeams)
                        {
                            if (searchingObj.Id == someID)
                            {
                                idExist = true;
                                break;
                            }
                        }
                        if (!idExist)
                        {
                            Log.Error("[Event-trigger] Trigger with non exist team ID ({0})  {1}:  L{2}", someID, eventPathName, eventTrigger.LineNum);
                        }
                    }
                }
                else if (eventTrigger.Type == TriggerType.Technology || (parentType == TriggerType.Technology && eventTrigger.Type == TriggerType.Value)
                    || eventTrigger.Type == TriggerType.IsTechActive)
                {
                    if (eventTrigger.Value is double)
                    {
                        int someID = (int)((double)eventTrigger.Value);
                        bool idExist = false;
                        foreach (int techID in techIDs)
                        {
                            if (techID == someID)
                            {
                                idExist = true;
                                break;
                            }
                        }
                        if (!idExist)
                        {
                            Log.Error("[Event-trigger] Trigger with non exist technology ID ({0})  {1}:  L{2}", someID, eventPathName, eventTrigger.LineNum);
                        }
                    }
                }
            }
        }
    }
}
