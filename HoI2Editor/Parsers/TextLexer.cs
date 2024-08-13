using System;
using System.IO;
using System.Text;
using HoI2Editor.Models;
using HoI2Editor.Utilities;
using System.Reflection;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     Text file phrase analysis class
    /// </summary>
    public class TextLexer : IDisposable
    {
        #region Public properties

        /// <summary>
        ///     File name being analyzed
        /// </summary>
        public string PathName { get; private set; }

        /// <summary>
        ///     File name being analyzed (( Excluding directories )
        /// </summary>
        public string FileName => Path.GetFileName(PathName);

        /// <summary>
        ///     Line number being analyzed
        /// </summary>
        public int LineNo { get; private set; }

        /// <summary>
        ///     Char position  being analyzed
        /// </summary>
        public long Position
        {
            get
            {
                if (_reader == null)
                    return 0;

                Int32 charpos = (Int32)_reader.GetType().InvokeMember("charPos",
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.GetField
                 , null, _reader, null);

                Int32 charlen = (Int32)_reader.GetType().InvokeMember("charLen",
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.GetField
                    , null, _reader, null);

                return (Int32)_reader.BaseStream.Position - charlen + charpos;
            }
        }

        #endregion

        #region Internal field

        /// <summary>
        ///     Whether to skip whitespace
        /// </summary>
        private readonly bool _skipWhiteSpace;

        /// <summary>
        ///     For reading text files
        /// </summary>
        private StreamReader _reader;

        /// <summary>
        ///     Tokens on hold
        /// </summary>
        private Token _token;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="fileName">File name to be analyzed</param>
        /// <param name="skipWhiteSpace">Whether to skip whitespace</param>
        public TextLexer(string fileName, bool skipWhiteSpace)
        {
            _skipWhiteSpace = skipWhiteSpace;

            Open(fileName);
        }

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="fileName">File name to be analyzed</param>
        /// <param name="skipWhiteSpace">Whether to skip whitespace</param>
        /// <param name="textCodePage">text file encoding</param>
        public TextLexer(string fileName, bool skipWhiteSpace, int textCodePage)
        {
            _skipWhiteSpace = skipWhiteSpace;

            Open(fileName, textCodePage);
        }

        /// <summary>
        ///     Processing when destroying an object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Destructor
        /// </summary>
        ~TextLexer()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Processing when destroying an object
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_reader != null)
            {
                Close();
            }
        }

        /// <summary>
        ///     Open file
        /// </summary>
        /// <param name="fileName">file name</param>
        public void Open(string fileName)
        {
            Open(fileName, Game.CodePage);
        }

        /// <summary>
        ///     Open file
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="textCodePage">text file encoding</param>
        public void Open(string fileName, int textCodePage)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            // Close any files that are already open
            if (_reader != null)
            {
                Close();
            }

            _reader = new StreamReader(fileName, Encoding.GetEncoding(textCodePage));

            PathName = fileName;
            LineNo = 1;
        }

        /// <summary>
        ///     Close file
        /// </summary>
        public void Close()
        {
            _reader.Close();
            _reader = null;
        }

        #endregion

        #region Phrase analysis

        /// <summary>
        ///     Lexical analysis
        /// </summary>
        /// <returns>token</returns>
        public Token GetToken()
        {
            return Read();
        }

        /// <summary>
        ///     Request a token of the specified type
        /// </summary>
        /// <param name="type">Token type to request</param>
        /// <returns>If the type required by the next token true true return it</returns>
        public bool WantToken(TokenType type)
        {
            Token token = Peek();

            if (token != null && token.Type == type)
            {
                Read();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Request an identifier token of the specified type
        /// </summary>
        /// <param name="keyword">Keyword name to request</param>
        /// <returns>If the required identifier true true return it</returns>
        public bool WantIdentifier(string keyword)
        {
            Token token = Peek();

            if (token != null && token.Type == TokenType.Identifier && ((string) token.Value).Equals(keyword))
            {
                Read();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Analyze the first token and move the reading pointer
        /// </summary>
        /// <returns></returns>
        private Token Read()
        {
            // Return if there is already an analyzed token
            if (_token != null)
            {
                Token result = _token;
                _token = null;
                return result;
            }

            return Parse();
        }

        /// <summary>
        ///     Parse the first token and do not move the read pointer
        /// </summary>
        /// <returns></returns>
        private Token Peek()
        {
            // Returns any tokens that have already been parsed
            if (_token != null)
            {
                return _token;
            }

            _token = Parse();
            return _token;
        }

        /// <summary>
        ///     Word analysis
        /// </summary>
        /// <returns>token</returns>
        private Token Parse()
        {
            int c = _reader.Peek();

            // Return null at the end of the file
            if (c == -1)
            {
                return null;
            }

            // Skip whitespace and comments
            if (_skipWhiteSpace)
            {
                while (true)
                {
                    // At the end of the file null return it
                    if (c == -1)
                    {
                        return null;
                    }

                    // Blank character / / Skip control characters
                    if (char.IsWhiteSpace((char) c) || char.IsControl((char) c))
                    {
                        if (c == '\r')
                        {
                            LineNo++;
                            _reader.Read();
                            c = _reader.Peek();
                            if (c == '\n')
                            {
                                _reader.Read();
                                c = _reader.Peek();
                            }
                            continue;
                        }
                        if (c == '\n')
                        {
                            LineNo++;
                        }
                        _reader.Read();
                        c = _reader.Peek();
                        continue;
                    }

                    // # After that is regarded as a line comment
                    if (c == '#')
                    {
                        _reader.ReadLine();
                        c = _reader.Peek();
                        LineNo++;
                        continue;
                    }

                    // If it is not a space character and a comment, it is interpreted as the beginning of the token.
                    break;
                }
            }

            // Numbers
            if (char.IsDigit((char) c) || c == '-' || c == '.')
            {
                return ParseNumber();
            }

            // identifier
            if (char.IsLetter((char) c) || c == '_')
            {
                return ParseIdentifier();
            }

            // Character string
            if (c == '"')
            {
                _reader.Read();
                return ParseString();
            }

            // equal sign
            if (c == '=')
            {
                _reader.Read();
                return new Token { Type = TokenType.Equal };
            }

            // Open curly braces
            if (c == '{')
            {
                _reader.Read();
                return new Token { Type = TokenType.OpenBrace };
            }

            // Closed curly braces
            if (c == '}')
            {
                _reader.Read();
                return new Token { Type = TokenType.CloseBrace };
            }

            if (!_skipWhiteSpace)
            {
                // White space / / Control character
                if (char.IsWhiteSpace((char) c) || char.IsControl((char) c))
                {
                    return ParseWhiteSpace();
                }

                // Start commenting
                if (c == '#')
                {
                    return ParseComment();
                }
            }

            // Invalid string
            return ParseInvalid();
        }

        /// <summary>
        ///     Analyze numbers
        /// </summary>
        /// <returns>token</returns>
        private Token ParseNumber()
        {
            StringBuilder sb = new StringBuilder();
            bool minus = false;
            bool point = false;
            bool identifier = false;

            int c = _reader.Peek();
            if (c == '-')
            {
                minus = true;
                _reader.Read();
                sb.Append((char) c);
            }

            while (true)
            {
                c = _reader.Peek();

                // If it is the end of the file, the reading ends
                if (c == -1)
                {
                    break;
                }

                // Read on if it's a number
                if (char.IsDigit((char) c))
                {
                    _reader.Read();
                    sb.Append((char) c);
                    continue;
                }

                // Decimal point
                if (!point && !identifier && c == '.')
                {
                    point = true;
                    _reader.Read();
                    sb.Append((char) c);
                    continue;
                }

                // If it is an alphabetic character, switch to the identifier and continue reading
                if (!minus && !point && (char.IsLetter((char) c) || c == '_'))
                {
                    identifier = true;
                    _reader.Read();
                    sb.Append((char) c);
                    continue;
                }

                // If it is a character that is not covered, it will be omitted
                break;
            }

            // Returns an identifier token
            if (identifier)
            {
                return new Token { Type = TokenType.Identifier, Value = sb.ToString() };
            }

            // Returns a numeric token
            double d;
            if (DoubleHelper.TryParse(sb.ToString(), out d))
            {
                return new Token { Type = TokenType.Number, Value = d };
            }

            return new Token { Type = TokenType.Invalid, Value = sb.ToString() };
        }

        /// <summary>
        ///     Parse the identifier
        /// </summary>
        /// <returns>token</returns>
        private Token ParseIdentifier()
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                int c = _reader.Peek();

                // If it is the end of the file, the reading ends
                if (c == -1)
                {
                    break;
                }

                // Continue reading if it is an alphabetic letter or number
                if (char.IsLetter((char) c) || char.IsNumber((char) c) || c == '_')
                {
                    _reader.Read();
                    sb.Append((char) c);
                    continue;
                }

                // If it is a character that is not covered, it will be omitted
                break;
            }

            return new Token { Type = TokenType.Identifier, Value = sb.ToString() };
        }

        /// <summary>
        ///     Parse the string
        /// </summary>
        /// <returns>token</returns>
        private Token ParseString()
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                int c = _reader.Peek();

                // If it is the end of the file, the reading ends
                if (c == -1)
                {
                    break;
                }

                // Fool loop forgot to close quotes
                // Exit if a newline character appears
                if (c == '\r' || c == '\n')
                {
                    break;
                }

                // End of string with quote
                if (c == '"')
                {
                    _reader.Read();
                    break;
                }

                _reader.Read();
                sb.Append((char) c);
            }

            return new Token { Type = TokenType.String, Value = sb.ToString() };
        }

        /// <summary>
        ///     Parse blank characters
        /// </summary>
        /// <returns>token</returns>
        private Token ParseWhiteSpace()
        {
            StringBuilder sb = new StringBuilder();

            int c = _reader.Peek();
            while (true)
            {
                // If it is the end of the file, the reading ends
                if (c == -1)
                {
                    break;
                }

                // If it is blank, read on
                if (char.IsWhiteSpace((char) c) || char.IsControl((char) c))
                {
                    if (c == '\r')
                    {
                        LineNo++;
                        sb.Append((char) c);
                        _reader.Read();
                        c = _reader.Peek();
                        if (c == '\n')
                        {
                            sb.Append((char) c);
                            _reader.Read();
                            c = _reader.Peek();
                        }
                        continue;
                    }
                    if (c == '\n')
                    {
                        LineNo++;
                    }
                    sb.Append((char) c);
                    _reader.Read();
                    c = _reader.Peek();
                    continue;
                }

                // If it is a character that is not covered, it will be omitted
                break;
            }

            return new Token { Type = TokenType.WhiteSpace, Value = sb.ToString() };
        }

        /// <summary>
        ///     Analyze comments
        /// </summary>
        /// <returns>token</returns>
        private Token ParseComment()
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                int c = _reader.Peek();

                // If it is the end of the file, the reading ends
                if (c == -1)
                {
                    break;
                }

                // If it is a line break, reading ends
                if (c == '\r' || c == '\n')
                {
                    break;
                }

                sb.Append((char) c);
                _reader.Read();
            }

            return new Token { Type = TokenType.Comment, Value = sb.ToString() };
        }

        /// <summary>
        ///     Parse invalid tokens
        /// </summary>
        /// <returns>token</returns>
        private Token ParseInvalid()
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                int c = _reader.Peek();

                // If it is the end of the file, the reading ends
                if (c == -1)
                {
                    break;
                }

                // If it is a character that can be another token, it ends reading
                if (char.IsWhiteSpace((char) c) ||
                    char.IsLetter((char) c) ||
                    char.IsDigit((char) c) ||
                    char.IsControl((char) c) ||
                    c == '"' ||
                    c == '=' ||
                    c == '{' ||
                    c == '}' ||
                    c == '_')
                {
                    break;
                }

                _reader.Read();
                sb.Append((char) c);
            }

            return new Token { Type = TokenType.Invalid, Value = sb.ToString() };
        }

        /// <summary>
        ///     Skip to the end of the line
        /// </summary>
        public void SkipLine()
        {
            _reader.ReadLine();
            LineNo++;
        }

        /// <summary>
        ///     Skip to the specified type of token
        /// </summary>
        /// <param name="type"></param>
        public void SkipToToken(TokenType type)
        {
            while (true)
            {
                Token token = GetToken();
                if (token == null)
                {
                    return;
                }

                if (token.Type == type)
                {
                    return;
                }
            }
        }

        /// <summary>
        ///     Hold token
        /// </summary>
        /// <param name="token"></param>
        public void ReserveToken(Token token)
        {
            _token = token;
        }

        #endregion
    }
}
