using System;
using System.IO;
using System.Text;
using HoI2Editor.Models;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     CSV file phrase analysis class
    /// </summary>
    public class CsvLexer : IDisposable
    {
        #region Public property

        /// <summary>
        ///     File name during analysis
        /// </summary>
        public string PathName { get; private set; }

        /// <summary>
        ///     File name being analyzed (excluding directories)
        /// </summary>
        public string FileName => Path.GetFileName(PathName);

        /// <summary>
        ///     Row number during analysis
        /// </summary>
        public int LineNo { get; private set; }

        /// <summary>
        ///     Return if you reach the end of the file
        /// </summary>
        public bool EndOfStream => _reader.EndOfStream;

        #endregion

        #region Internal field

        /// <summary>
        ///     For reading text files
        /// </summary>
        private StreamReader _reader;

        #endregion

        #region Internal fixed number

        /// <summary>
        ///     CSV file separated character
        /// </summary>
        private static readonly char[] Separator = { ';' };

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="fileName">File name to be analyzed</param>
        public CsvLexer(string fileName)
        {
            Open(fileName);
        }

        /// <summary>
        ///     Processing when the object is destroyed
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Destructor
        /// </summary>
        ~CsvLexer()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Processing when the object is destroyed
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_reader != null)
            {
                Close();
            }
        }

        /// <summary>
        ///     Open the file
        /// </summary>
        /// <param name="fileName">file name</param>
        public void Open(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            // Close if there is already an open file
            if (_reader != null)
            {
                Close();
            }

            _reader = new StreamReader(fileName, Encoding.GetEncoding(Game.CodePage));

            PathName = fileName;
            LineNo = 0;
        }

        /// <summary>
        ///     Close the file
        /// </summary>
        public void Close()
        {
            _reader.Close();
            _reader = null;
        }

        #endregion

        #region Word analysis

        /// <summary>
        ///     Word analysis
        /// </summary>
        /// <returns>Token column</returns>
        public string[] GetTokens()
        {
            // When you reach the end of the file, return null
            if (_reader.EndOfStream)
            {
                return null;
            }

            LineNo++;
            string line = _reader.ReadLine();

            // Return NULL if it is a blank line
            if (string.IsNullOrEmpty(line))
            {
                return null;
            }

            // Return NULL if it is a comment
            if (line[0] == '#')
            {
                return null;
            }

            return line.Split(Separator);
        }

        /// <summary>
        ///     1 Read and skip
        /// </summary>
        public void SkipLine()
        {
            // Do nothing when you reach the end of the file
            if (_reader.EndOfStream)
            {
                return;
            }

            LineNo++;
            _reader.ReadLine();
        }

        #endregion
    }
}
