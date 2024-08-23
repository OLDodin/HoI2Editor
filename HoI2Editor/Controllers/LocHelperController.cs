using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using HoI2Editor.Models;
using HoI2Editor.Parsers;
using HoI2Editor.Utilities;

namespace HoI2Editor.Controllers
{
    /// <summary>
    ///     Info about column --text codepage and path to file
    /// </summary>
    public class MergeFilesInfo
    {
        /// <summary>
        ///     Text code page in file
        /// </summary>
        public int ResultCodePage { get; }

        /// <summary>
        ///     File path
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        ///     constructor
        /// </summary>
        public MergeFilesInfo(string aPath, int aResultCodePage)
        {
            ResultCodePage = aResultCodePage;
            FilePath = aPath;
        }
    }

    /// <summary>
    ///     Info about column --text codepage and filename
    /// </summary>
    public class SliceFilesInfo
    {
        /// <summary>
        ///     Text code page in file
        /// </summary>
        public int SrcCodePage { get; }

        /// <summary>
        ///     File name
        /// </summary>
        public string FileName { get; }

        /// <summary>
        ///     constructor
        /// </summary>
        public SliceFilesInfo(string aFileName, int aSrcCodePage)
        {
            SrcCodePage = aSrcCodePage;
            FileName = aFileName;
        }
    }

    /// <summary>
    ///     Localization helper panel controller class
    /// </summary>
    class LocHelperController
    {
        /// <summary>
        ///     Text ID list
        /// </summary>
        private List<string> _alreadyExistEventKeysList = new List<string>();

        #region Build localization csv file helper
        /// <summary>
        ///     Splite the source file to several files
        /// </summary>
        /// <param name="srcFilepath">source text file</param>
        /// <param name="resultDirPath">output folder</param>
        /// <param name="sliceFilesInfo">info for create output files</param>
        public void SpliteFileByColumn(string srcFilepath, string resultDirPath, Dictionary<string, SliceFilesInfo> sliceFilesInfo)
        {
            ExtractColumn(srcFilepath, resultDirPath, sliceFilesInfo["IDs"], 0);
            ExtractColumn(srcFilepath, resultDirPath, sliceFilesInfo["ENG"], 1);
            ExtractColumn(srcFilepath, resultDirPath, sliceFilesInfo["FRA"], 2);
            ExtractColumn(srcFilepath, resultDirPath, sliceFilesInfo["ITA"], 3);
            ExtractColumn(srcFilepath, resultDirPath, sliceFilesInfo["SPA"], 4);
            ExtractColumn(srcFilepath, resultDirPath, sliceFilesInfo["GER"], 5);
            ExtractColumn(srcFilepath, resultDirPath, sliceFilesInfo["POL"], 6);
            ExtractColumn(srcFilepath, resultDirPath, sliceFilesInfo["POR"], 7);
            ExtractColumn(srcFilepath, resultDirPath, sliceFilesInfo["RUS"], 8);
        }


        /// <summary>
        ///     Merge input files to one result file
        /// </summary>
        /// <param name="resultFilepath">Result file path</param>
        /// <param name="mergeFilesInfo">Input files info</param>
        public void MergeColumnFiles(string resultFilepath, Dictionary<string, MergeFilesInfo> mergeFilesInfo)
        {
            if (File.Exists(resultFilepath))
                File.Delete(resultFilepath);

            
            string[] IDsLines = ReadAllFileLines(mergeFilesInfo["IDs"].FilePath);
            string[] ENGLines = ReadAllFileLines(mergeFilesInfo["ENG"].FilePath);
            string[] FRALines = ReadAllFileLines(mergeFilesInfo["FRA"].FilePath);
            string[] ITALines = ReadAllFileLines(mergeFilesInfo["ITA"].FilePath);
            string[] SPALines = ReadAllFileLines(mergeFilesInfo["SPA"].FilePath);
            string[] GERLines = ReadAllFileLines(mergeFilesInfo["GER"].FilePath);
            string[] POLLines = ReadAllFileLines(mergeFilesInfo["POL"].FilePath);
            string[] PORLines = ReadAllFileLines(mergeFilesInfo["POR"].FilePath);
            string[] RUSLines = ReadAllFileLines(mergeFilesInfo["RUS"].FilePath);

            if (IDsLines == null)
            {
                MessageBox.Show("IDs file not exist!");
                return;
            }
            bool isNotSameSize = ENGLines != null && IDsLines.Length != ENGLines.Length
                || FRALines != null && IDsLines.Length != FRALines.Length
                || ITALines != null && IDsLines.Length != ITALines.Length
                || SPALines != null && IDsLines.Length != SPALines.Length
                || GERLines != null && IDsLines.Length != GERLines.Length
                || POLLines != null && IDsLines.Length != POLLines.Length
                || PORLines != null && IDsLines.Length != PORLines.Length
                || RUSLines != null && IDsLines.Length != RUSLines.Length;
            if (isNotSameSize)
            {
                MessageBox.Show("Column files has different number of lines!");
                return;
            }
            FileStream fs = new FileStream(resultFilepath, FileMode.Append);
            StreamWriter fileWriterIDs = new StreamWriter(fs, Encoding.GetEncoding(mergeFilesInfo["IDs"].ResultCodePage));
            StreamWriter fileWriterENG = new StreamWriter(fs, Encoding.GetEncoding(mergeFilesInfo["ENG"].ResultCodePage));
            StreamWriter fileWriterFRA = new StreamWriter(fs, Encoding.GetEncoding(mergeFilesInfo["FRA"].ResultCodePage));
            StreamWriter fileWriterITA = new StreamWriter(fs, Encoding.GetEncoding(mergeFilesInfo["ITA"].ResultCodePage));
            StreamWriter fileWriterSPA = new StreamWriter(fs, Encoding.GetEncoding(mergeFilesInfo["SPA"].ResultCodePage));
            StreamWriter fileWriterGER = new StreamWriter(fs, Encoding.GetEncoding(mergeFilesInfo["GER"].ResultCodePage));
            StreamWriter fileWriterPOL = new StreamWriter(fs, Encoding.GetEncoding(mergeFilesInfo["POL"].ResultCodePage));
            StreamWriter fileWriterPOR = new StreamWriter(fs, Encoding.GetEncoding(mergeFilesInfo["POR"].ResultCodePage));
            StreamWriter fileWriterRUS = new StreamWriter(fs, Encoding.GetEncoding(mergeFilesInfo["RUS"].ResultCodePage));

            for (int i = 0; i < IDsLines.Length; i++)
            {
                if (IDsLines[i] != "")
                    AppendStringToFile(fileWriterIDs, IDsLines[i]);
                else
                    AppendStringToFile(fileWriterIDs, "#");
                AppendStringToFile(fileWriterENG, ENGLines?[i]);
                AppendStringToFile(fileWriterFRA, FRALines?[i]);
                AppendStringToFile(fileWriterITA, ITALines?[i]);
                AppendStringToFile(fileWriterSPA, SPALines?[i]);
                AppendStringToFile(fileWriterGER, GERLines?[i]);
                AppendStringToFile(fileWriterPOL, POLLines?[i]);
                AppendStringToFile(fileWriterPOR, PORLines?[i]);
                AppendStringToFile(fileWriterRUS, RUSLines?[i]);

                fileWriterIDs.Write(";;X\r\n");
                fileWriterIDs.Flush();
            }

            fs.Close();
        }

        /// <summary>
        ///     Append text element to result table
        /// </summary>
        /// <param name="streamWriter">File writter with specified codepage</param>
        /// <param name="stringToAppend">Text element for writing</param>
        private void AppendStringToFile(StreamWriter streamWriter, string stringToAppend)
        {
            if (stringToAppend != null)
                streamWriter.Write(stringToAppend);
            streamWriter.Write(';');
            streamWriter.Flush();
        }

        /// <summary>
        ///     Read file lines
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>Array of file lines</returns>
        private string[] ReadAllFileLines(string filePath)
        {
            try
            {
                return File.ReadAllLines(filePath);
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        /// <summary>
        ///     Extract column with some codepage and write to output file
        /// </summary>
        /// <param name="srcFilepath">Source text file</param>
        /// /// <param name="resultDirPath">Directory for save extracted columns</param>
        /// /// <param name="columnInfo">Info about column --codepage and result filename</param>
        /// /// <param name="columnIndex">Column index</param>
        private void ExtractColumn(string srcFilepath, string resultDirPath, SliceFilesInfo columnInfo, int columnIndex)
        {
            string[] lines = File.ReadAllLines(srcFilepath, Encoding.GetEncoding(columnInfo.SrcCodePage));

            string resultFilepath = resultDirPath + columnInfo.FileName;
            if (File.Exists(resultFilepath))
                File.Delete(resultFilepath);
            using (FileStream fs = new FileStream(resultFilepath, FileMode.Create))
            {
                using (StreamWriter fileWriter = new StreamWriter(fs, Encoding.Unicode))
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] splittedByComment = lines[i].Split('#');
                        string[] columnValues = splittedByComment[0].Split(';');
                        int expectedCount = 12;
                        if (Path.GetFileName(srcFilepath).Equals("famous_quotes.csv"))
                        {
                            expectedCount = 16;
                            MessageBox.Show("famous_quotes.csv not supported");
                            return;
                        }
                        if (columnValues.Length != expectedCount)
                        {
                            if (splittedByComment.Length == 1 || splittedByComment[0].Contains(';'))
                            {
                                MessageBox.Show("Number of columns on line" + (i + 1).ToString() + " not equal " + expectedCount.ToString());
                                return;
                            }
                            fileWriter.WriteLine("");
                        }
                        else
                            fileWriter.WriteLine(columnValues[columnIndex]);
                    }
                }
            }
        }
        #endregion

        #region Extract event texts to csv file
        /// <summary>
        ///     Extract text from event files and replace this text by textID
        /// </summary>
        /// <param name="textCodePage">Events files encoding</param>
        /// <param name="makeBackup">Backup events files</param>
        /// <param name="customEventDirPath">Custom events folder</param>
        public void ExtractAllTextFromEvents(int textCodePage, bool makeBackup, string customEventDirPath)
        {
            string eventPathName = Game.EventsPathName;
            if (!string.IsNullOrEmpty(customEventDirPath))
                eventPathName = customEventDirPath;
            if (!CheckPaths(eventPathName))
            {
                return;
            }
            if (makeBackup)
            {
                MakeBackup(Game.GetReadFileName(eventPathName), Path.Combine(Game.GetReadFileName(Game.DatabasePathName), "eventBackup"));
            }

            LoadTextKeysFromCvs();

            // make new text ID
            string pathName = Game.GetReadFileName(eventPathName);
            Dictionary<string, string> eventToExportTextList = new Dictionary<string, string>();
            foreach (Event ev in Events.TotalEventsList)
            {
                if (CheckEventString(ev.Name))
                    AddToDictionary(eventToExportTextList, "EVT_" + ev.Id + "_NAME", ev.Name);
                if (CheckEventString(ev.Desc))
                    AddToDictionary(eventToExportTextList, "EVT_" + ev.Id + "_DESC", ev.Desc);

                for (int i = 0; i < ev.Actions.Count; i++)
                {
                    string actionName = ev.Actions[i].Name;
                    if (CheckEventString(actionName))
                    {
                        char actionIndex = 'A';
                        actionIndex = Convert.ToChar(actionIndex + i);
                        AddToDictionary(eventToExportTextList, "ACTIONNAME" + ev.Id + actionIndex, actionName);
                    }
                }
            }
            // replace text in event files with text ID
            PatchEventFiles(pathName, eventToExportTextList, textCodePage);
            
            WriteCvsFile(eventToExportTextList);
        }

        /// <summary>
        ///     Add comment to every event and to file begining
        /// </summary>
        /// <param name="textCodePage">Events files encoding</param>
        /// <param name="makeBackup">Backup events files</param>
        public void AddCommentForEvents(int textCodePage, bool makeBackup)
        {
            List<string> eventFilesList = new List<string>();
            foreach (Event hoi2Event in Events.TotalEventsList)
            {
                if (!eventFilesList.Contains(hoi2Event.PathName))
                    eventFilesList.Add(hoi2Event.PathName);
            }
            if (makeBackup)
            {
                string backupPath = GetBackupPath(Path.Combine(Game.GetReadFileName(Game.DatabasePathName), "eventBackup"));
                Directory.CreateDirectory(backupPath);
                foreach (string eventFilePath in eventFilesList)
                {
                    string newPath = eventFilePath.Replace(Path.GetFullPath(Game.GetReadFileName(Game.DatabasePathName)), Path.GetFullPath(backupPath));
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                    File.Copy(eventFilePath, newPath, true);
                }
            }

            // add list of all events in the file
            Dictionary<string, List<string>> changedFilesContent = new Dictionary<string, List<string>>();
            foreach (Event hoi2Event in Events.TotalEventsList)
            {
                if (!changedFilesContent.ContainsKey(hoi2Event.PathName))
                {
                    changedFilesContent.Add(hoi2Event.PathName, new List<string>());
                    changedFilesContent[hoi2Event.PathName].Add("##########################################################");
                }
                List<string> newFileContent = changedFilesContent[hoi2Event.PathName];
                newFileContent.Add("#\t" + hoi2Event.Id.ToString() + " - " + hoi2Event.Country + "\t\t" + hoi2Event.GetEventNameWithConverID());
            }
            foreach (string eventFilePath in eventFilesList)
            {
                changedFilesContent[eventFilePath].Add("##########################################################");
                changedFilesContent[eventFilePath].Add("");
            }
            // add events with comment
            foreach (Event hoi2Event in Events.TotalEventsList)
            {
                
                if (!changedFilesContent.ContainsKey(hoi2Event.PathName))
                {
                    changedFilesContent.Add(hoi2Event.PathName, new List<string>());
                }

                List<string> newFileContent = changedFilesContent[hoi2Event.PathName];
                newFileContent.Add("");
                newFileContent.Add("##########################################################");
                newFileContent.Add("#\t" + hoi2Event.GetEventNameWithConverID());
                newFileContent.Add("#\tACTIONS:");
                for (int i = 0; i < hoi2Event.Actions.Count; i++)
                {
                    newFileContent.Add("#\t" + (i+1).ToString() + ")\t" + hoi2Event.GetActionNameWithConverID(i));
                }
                newFileContent.Add("##########################################################");

                newFileContent.Add(hoi2Event.EventText);

            }
            // write to the files
            foreach (string eventFilePath in eventFilesList)
            {
                File.WriteAllLines(eventFilePath, changedFilesContent[eventFilePath].ToArray(), Encoding.GetEncoding(textCodePage));
            }
        }

        /// <summary>
        ///     Load text ID from file
        /// </summary>
        private void LoadTextKeysFromCvs()
        {
            // read exist text keys
            _alreadyExistEventKeysList.Clear();
            foreach (string fileName in Directory.GetFiles(Game.GetReadFileName(Game.ConfigPathName), "*.csv"))
            {
                string name = Path.GetFileName(fileName);
                if (name.Equals("editor.csv") || name.Equals("launcher.csv") || name.Equals("famous_quotes.csv"))
                    continue;
                LoadCsvFile(fileName);
            }
            if (Game.Type == GameType.ArsenalOfDemocracy)
            {
                string additionalPath = Path.Combine(Game.GetReadFileName(Game.ConfigPathName), "Additional");
                if (Directory.Exists(additionalPath))
                    foreach (string fileName in Directory.GetFiles(additionalPath, "*.csv"))
                    {
                        LoadCsvFile(fileName);
                    }
            }
        }

        /// <summary>
        ///     Load text ID from file
        /// </summary>
        /// <param name="filePath">file path for parse</param>
        private void LoadCsvFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }
            using (CsvLexer lexer = new CsvLexer(filePath))
            {
                // Read the empty file
                if (lexer.EndOfStream)
                {
                    return;
                }

                // Round the header
                lexer.SkipLine();

                // Read the file only for the header
                if (lexer.EndOfStream)
                {
                    return;
                }

                while (!lexer.EndOfStream)
                {
                    string eventKeyName = ParseLine(lexer);
                    if (eventKeyName == null)
                    {
                        continue;
                    }
                    eventKeyName = eventKeyName.ToLower();
                    if (!_alreadyExistEventKeysList.Contains(eventKeyName))
                        _alreadyExistEventKeysList.Add(eventKeyName);
                }
            }
        }

        /// <summary>
        ///     Load text ID from line
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>Text ID</returns>
        private string ParseLine(CsvLexer lexer)
        {
            string[] tokens = lexer.GetTokens();

            if (string.IsNullOrEmpty(tokens?[0]))
            {
                return null;
            }

            if (tokens.Length != 12)
            {
                Log.Warning("[Event text] Invalid token count: {0} ({1} L{2})", tokens.Length, lexer.FileName,
                    lexer.LineNo);
                if (tokens.Length == 0)
                    return null;
                else
                    return tokens[0];
            }

            return tokens[0];
        }

        /// <summary>
        ///     Replace text in event files with text ID
        /// </summary>
        /// <param name="pathName">File path for patch</param>
        /// <param name="eventDictionary">Event texts by textID</param>
        /// <param name="textCodePage">Events files encoding</param>
        private void PatchEventFiles(string pathName, Dictionary<string, string> eventDictionary, int textCodePage)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(pathName);
            foreach (FileInfo fi in dirInfo.GetFiles())
            {
                PatchEventFile(fi.FullName, eventDictionary, textCodePage);
            }

            foreach (DirectoryInfo diSourceSubDir in dirInfo.GetDirectories())
            {
                PatchEventFiles(diSourceSubDir.FullName, eventDictionary, textCodePage);
            }
        }

        /// <summary>
        ///     Replace text in event file with text ID
        /// </summary>
        /// <param name="fileName">File name for patch</param>
        /// <param name="eventDictionary">Event texts by textID</param>
        /// <param name="textCodePage">Events files encoding</param>
        private void PatchEventFile(string fileName, Dictionary<string, string> eventDictionary, int textCodePage)
        {
            string[] fileLines = File.ReadAllLines(fileName, Encoding.GetEncoding(textCodePage));
            for (int i = 0; i < fileLines.Length; i++)
            {
                string lineStr = fileLines[i].ToLower();
                if (IsLineForPatch(lineStr))
                {
                    foreach (KeyValuePair<string, string> eventInfo in eventDictionary)
                    {
                        fileLines[i] = fileLines[i].Replace("\"" + eventInfo.Value + "\"", eventInfo.Key);
                    }
                }
            }
            File.WriteAllLines(fileName, fileLines, Encoding.GetEncoding(textCodePage));
            /*
            string fileText = File.ReadAllText(fileName, Encoding.GetEncoding(1252));
            foreach (KeyValuePair<string, string> eventInfo in aEventDictionary)
            { 
                fileText = fileText.Replace("\"" + eventInfo.Value + "\"", eventInfo.Key);
            }
            File.WriteAllText(fileName, fileText, Encoding.GetEncoding(1252));
            */
        }

        /// <summary>
        ///     Check line
        /// </summary>
        /// <param name="lineStr">Line to check</param>
        /// <returns>true if this line for patching</returns>
        private bool IsLineForPatch(string lineStr)
        {
            string[] lineContent = lineStr.Split('#');
            if (lineContent.Length > 1 && lineContent[0] != "")
            {
                lineStr = lineContent[0];
            }

            lineStr = lineStr.Replace(" ", "");
            lineStr = lineStr.Replace("\t", "");

            if (lineStr.Contains("name=") || lineStr.Contains("desc="))
                return true;
            return false;
        }

        /// <summary>
        ///     Check text ID already exist
        /// </summary>
        /// <param name="textID">Text ID</param>
        /// <returns>True if text ID not exist</returns>
        private bool CheckEventString(string textID)
        {
            if (textID != null)
            {
                string lowerStr = textID.ToLower();
                if (_alreadyExistEventKeysList.Contains(lowerStr))
                    return false;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Add new text with check text already exist
        /// </summary>
        /// <param name="linesDictionary">Text ID and text value</param>
        /// <param name="newKey">New text ID</param>
        /// <param name="newValue">New text value</param>
        private void AddToDictionary(Dictionary<string, string> linesDictionary, string newKey, string newValue)
        {
            string newKeyName = newKey;
            int cnt = 0;
            //if text already added use them
            if (linesDictionary.ContainsValue(newValue))
            {
                return;
            }
            //check same key (files contain events with same id for different scenario)
            if (linesDictionary.ContainsKey(newKeyName))
            {
                //duplicate with same text value
                if (linesDictionary[newKeyName] == newValue)
                    return;
            }
            //generate new key
            while (linesDictionary.ContainsKey(newKeyName))
            {
                cnt++;
                newKeyName = newKey + "_" + cnt;
            }
            linesDictionary.Add(newKeyName, newValue);
        }

        /// <summary>
        ///     Write text ID and text value to files
        /// </summary>
        /// <param name="linesDictionary">Text ID and text value for save</param>
        private void WriteCvsFile(Dictionary<string, string> linesDictionary)
        {
            string pathName = Path.Combine(Game.GetReadFileName(Game.ConfigPathName), "exported");
            if (!Directory.Exists(pathName))
                Directory.CreateDirectory(pathName);
            else
                MakeBackup(pathName, Path.Combine(Game.GetReadFileName(Game.ConfigPathName), "exportedBackup"));
            string exprotFileName = Path.Combine(pathName, "exported.csv");
            string keysFileName = Path.Combine(pathName, "ids.csv");
            if (File.Exists(exprotFileName))
                File.Delete(exprotFileName);
            if (File.Exists(keysFileName))
                File.Delete(keysFileName);
            FileStream fsExportedText = File.Create(exprotFileName);
            FileStream fsExportedIDs = File.Create(keysFileName);
            // Encoding utf8WithoutBom = new UTF8Encoding (false);
            StreamWriter fileTextWriter = new StreamWriter(fsExportedText, Encoding.Unicode);
            StreamWriter fileIDsWriter = new StreamWriter(fsExportedIDs, Encoding.Unicode);
            foreach (KeyValuePair<string, string> lineInfo in linesDictionary)
            {
                fileTextWriter.WriteLine(lineInfo.Value.Replace(';', ','));
                fileIDsWriter.WriteLine(lineInfo.Key);
            }
            fileIDsWriter.Close();
            fsExportedIDs.Close();
            fileTextWriter.Close();
            fsExportedText.Close();
        }

        /// <summary>
        ///     Check exist all needed paths
        /// </summary>
        /// <param name="eventPathName">event path name</param>
        /// <returns>True if all path exists</returns>
        private bool CheckPaths(string eventPathName)
        {
            if (CheckFileName(eventPathName) == null)
            {
                MessageBox.Show("Not found event directory");
                return false;
            }
            if (CheckFileName(Game.DatabasePathName) == null)
            {
                MessageBox.Show("Not found db directory");
                return false;
            }
            if (CheckFileName(Game.ConfigPathName) == null)
            {
                MessageBox.Show("Not found config directory");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Get a file name for reading in consideration of MOD folder / saved folder
        /// </summary>
        /// <param name="pathName">Pass name</param>
        /// <returns>file name</returns>
        private string CheckFileName(string pathName)
        {
            string fileName;
            if (Game.IsModActive)
            {
                fileName = Game.GetModFileName(pathName);
                if (File.Exists(fileName) || Directory.Exists(fileName))
                {
                    return fileName;
                }
                else
                {
                    return null;
                }
            }

            fileName = Game.GetVanillaFileName(pathName);
            if (File.Exists(fileName) || Directory.Exists(fileName))
            {
                return fileName;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Copy Directory
        /// <summary>
        ///     Copy directory
        /// </summary>
        /// <param name="sourceDirectory">source directory</param>
        /// <param name="targetDirectory">target directory</param>
        private void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        /// <summary>
        ///     Copy directory
        /// </summary>
        /// <param name="source">source DirectoryInfo</param>
        /// <param name="target">target Directory Info</param>
        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        /// <summary>
        ///     Backup directory
        /// </summary>
        /// <param name="dirToBackupPath">Directory for backup</param>
        /// <param name="backupDirPath">Destination backup directory</param>
        private void MakeBackup(string dirToBackupPath, string backupDirPath)
        {
            Copy(dirToBackupPath, GetBackupPath(backupDirPath));
        }

        /// <summary>
        ///     Backup directory
        /// </summary>
        /// <param name="backupDirPath">Destination backup directory</param>
        private string GetBackupPath(string backupDirPath)
        {
            int cnt = 0;
            string backupPathName;
            do
            {
                cnt++;
                backupPathName = backupDirPath + cnt.ToString();
            }
            while (Directory.Exists(backupPathName));

            return backupPathName;
        }
        #endregion
    }
}
