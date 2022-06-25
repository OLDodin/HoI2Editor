using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using HoI2Editor.Parsers;
using HoI2Editor.Properties;

namespace HoI2Editor.Utilities
{
    /// <summary>
    ///     Log management
    /// </summary>
    public static class Log
    {
        #region Public properties

        /// <summary>
        ///     Log output level
        /// </summary>
        public static int Level
        {
            get { return (int) Sw.Level; }
            set
            {
                if (Sw.Level == TraceLevel.Off)
                {
                    if (value > 0)
                    {
                        Init();
                    }
                }
                else
                {
                    if (value == 0)
                    {
                        Terminate();
                    }
                }
                if ((TraceLevel) value == Sw.Level)
                {
                    return;
                }
                Sw.Level = (TraceLevel) value;
            }
        }

        #endregion

        #region Internal field

        /// <summary>
        ///     Log output switch
        /// </summary>
        private static readonly TraceSwitch Sw = new TraceSwitch(LogFileIdentifier, HoI2EditorController.Name);

        /// <summary>
        ///     For writing log files
        /// </summary>
        private static StreamWriter _writer;

        /// <summary>
        ///     For writing log files
        /// </summary>
        private static TextWriterTraceListener _listener;

        #endregion

        #region Internal constant

        /// <summary>
        ///     Log file name
        /// </summary>
        private const string LogFileName = "HoI2Editor.log";

        /// <summary>
        ///     Log file identifier
        /// </summary>
        private const string LogFileIdentifier = "HoI2EditorLog";

        #endregion

        #region Initialization

        /// <summary>
        ///     Initialize the log
        /// </summary>
        public static void Init()
        {
            try
            {
                _writer = new StreamWriter(LogFileName, true, Encoding.UTF8) { AutoFlush = true };
                _listener = new TextWriterTraceListener(_writer, LogFileIdentifier);
                Trace.Listeners.Add(_listener);
            }
            catch (Exception)
            {
                MessageBox.Show(Resources.LogFileOpenError, HoI2EditorController.Name, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Terminate();
            }
            Verbose("[Log] Init");
        }

        /// <summary>
        ///     End logging
        /// </summary>
        public static void Terminate()
        {
            Verbose("[Log] Terminate");
            if (_listener != null)
            {
                Trace.Listeners.Remove(_listener);
            }
            _writer?.Close();
        }

        #endregion

        #region Log output

        /// <summary>
        ///     Log invalid token error
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="token">token</param>
        /// <param name="lexer">Lexical analyzer</param>
        public static void InvalidToken(string categoryName, Token token, TextLexer lexer)
        {
            Warning("[{0}] Invalid token: {1} ({2} L{3})",
                categoryName, ObjectHelper.ToString(token.Value), lexer.FileName, lexer.LineNo);
        }

        /// <summary>
        ///     Log the close analysis error
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="clauseName">Close name</param>
        /// <param name="lexer">Lexical analyzer</param>
        public static void InvalidClause(string categoryName, string clauseName, TextLexer lexer)
        {
            Warning("[{0}] Parse failed: {1} clause ({2} L{3})", categoryName, clauseName, lexer.FileName, lexer.LineNo);
        }

        /// <summary>
        ///     Log section analysis error
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="sectionName">Section name</param>
        /// <param name="lexer">Lexical analyzer</param>
        public static void InvalidSection(string categoryName, string sectionName, TextLexer lexer)
        {
            Warning("[{0}] Parse failed: {1} section ({2} L{3})", categoryName, sectionName, lexer.FileName,
                lexer.LineNo);
        }

        /// <summary>
        ///     Log output of forgetting closing parenthesis error
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="sectionName">Section name</param>
        /// <param name="lexer">Lexical analyzer</param>
        public static void MissingCloseBrace(string categoryName, string sectionName, TextLexer lexer)
        {
            Warning("[{0}] Missing close brace: {1} section ({2} L{3})", categoryName, sectionName, lexer.FileName,
                lexer.LineNo);
        }

        /// <summary>
        ///     Log output of out-of-range errors
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="itemName">item name</param>
        /// <param name="o">Target value</param>
        /// <param name="lexer">Lexical analyzer</param>
        public static void OutOfRange(string categoryName, string itemName, Object o, TextLexer lexer)
        {
            Warning("[{0}] Out of range: {1} at {2} ({3} L{4})",
                categoryName, ObjectHelper.ToString(o), itemName, lexer.FileName, lexer.LineNo);
        }

        /// <summary>
        ///     Output error log
        /// </summary>
        /// <param name="s">Target character string</param>
        /// <param name="args">Parameters</param>
        public static void Error(string s, params object[] args)
        {
            WriteLine(TraceLevel.Error, s, args);
        }

        /// <summary>
        ///     Output error log
        /// </summary>
        /// <param name="s">Target character string</param>
        public static void Error(string s)
        {
            WriteLine(TraceLevel.Error, s);
        }

        /// <summary>
        ///     Output warning log
        /// </summary>
        /// <param name="s">Target character string</param>
        /// <param name="args">Parameters</param>
        public static void Warning(string s, params object[] args)
        {
            WriteLine(TraceLevel.Warning, s, args);
        }

        /// <summary>
        ///     Output warning log
        /// </summary>
        /// <param name="s">Target character string</param>
        public static void Warning(string s)
        {
            WriteLine(TraceLevel.Warning, s);
        }

        /// <summary>
        ///     Output information log
        /// </summary>
        /// <param name="s">Target character string</param>
        /// <param name="args">Parameters</param>
        public static void Info(string s, params object[] args)
        {
            WriteLine(TraceLevel.Info, s, args);
        }

        /// <summary>
        ///     Output information log
        /// </summary>
        /// <param name="s">Target character string</param>
        public static void Info(string s)
        {
            WriteLine(TraceLevel.Info, s);
        }

        /// <summary>
        ///     Output detailed log
        /// </summary>
        /// <param name="s">Target character string</param>
        /// <param name="args">Parameters</param>
        public static void Verbose(string s, params object[] args)
        {
            WriteLine(TraceLevel.Verbose, s, args);
        }

        /// <summary>
        ///     Output detailed log
        /// </summary>
        /// <param name="s">Target character string</param>
        public static void Verbose(string s)
        {
            WriteLine(TraceLevel.Verbose, s);
        }

        /// <summary>
        ///     Output log
        /// </summary>
        /// <param name="level">Log output level</param>
        /// <param name="s">Target character string</param>
        /// <param name="args">Parameters</param>
        private static void WriteLine(TraceLevel level, string s, params object[] args)
        {
            bool condition;
            switch (level)
            {
                case TraceLevel.Error:
                    condition = Sw.TraceError;
                    break;

                case TraceLevel.Warning:
                    condition = Sw.TraceWarning;
                    break;

                case TraceLevel.Info:
                    condition = Sw.TraceInfo;
                    break;

                case TraceLevel.Verbose:
                    condition = Sw.TraceVerbose;
                    break;

                default:
                    return;
            }
            string t = string.Format(s, args);
            Trace.WriteLineIf(condition, t);
        }

        #endregion
    }
}
