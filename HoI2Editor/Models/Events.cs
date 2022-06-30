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
        ///     Static constructor
        /// </summary>
        static Events()
        {
            TotalEventsList = new List<Event>();
        }

        /// <summary>
        ///     Read the events files
        /// </summary>
        /// <param name="textCodePage">text file encoding</param>
        /// <param name="customEventDirPath">Custom events folder</param>
        public static void Load(int textCodePage, string customEventDirPath)
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
    }
}
