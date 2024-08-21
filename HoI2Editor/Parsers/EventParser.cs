using System;
using System.Collections.Generic;
using HoI2Editor.Models;
using HoI2Editor.Utilities;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     Event data syntax analysis class
    /// </summary>
    public static class EventParser
    {
        #region Internal fixed number

        /// <summary>
        ///     Category name at the time of log output
        /// </summary>
        private const string LogCategory = "Events";

        #endregion

        #region Parsing

        /// <summary>
        ///     Synthetic analysis of event files
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="textCodePage">text file encoding</param>
        /// <returns>Events List</returns>
        public static List<Event> Parse(string fileName, int textCodePage)
        {
            string fileContent = File.ReadAllText(fileName, Encoding.GetEncoding(textCodePage));
            using (TextLexer lexer = new TextLexer(fileName, true, textCodePage))
            {
                return ParseEvents(lexer, fileContent);
            }
        }

        /// <summary>
        ///     Synthetic analysis of the Event section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>Events List</returns>
        private static List<Event> ParseEvents(TextLexer lexer, string eventFileContent)
        {
            Token token;
            List<Event> allEvents = new List<Event>();

            Event group = new Event();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                // Invalid tokens
                if (token.Type != TokenType.Identifier)
                {
                    //Log.InvalidToken (LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                string keyword = token.Value as string;
                if (string.IsNullOrEmpty(keyword))
                {
                    continue;
                }
                keyword = keyword.ToLower();

                // event event
                if (keyword.Equals("event"))
                {
                    //5 length "event"
                    long startPos = lexer.Position - 5;
                    Event hoi2Event = ParseEvent(lexer);
                    hoi2Event.PathName = lexer.PathName;
                    
                    if (hoi2Event == null)
                    {
                        Log.InvalidSection(LogCategory, "hoi2Event", lexer);
                        continue;
                    }

                    hoi2Event.EventText = eventFileContent.Substring((int)startPos, (int)(lexer.Position - startPos));

                    foreach (Event existEvent in allEvents)
                    {
                        if (existEvent.Id == hoi2Event.Id)
                        {
                            Log.Error("[Event] Duplicate event ID {0} in file {1}", hoi2Event.Id, lexer.PathName);
                            break;
                        }
                    }
                    allEvents.Add(hoi2Event);
                    
                    continue;
                }
                
                // Invalid tokens
               // Log.InvalidToken (LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return allEvents;
        }

        /// <summary>
        ///     Synthetic analysis of the Event section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>Event data</returns>
        private static Event ParseEvent(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            int openBraceCnt = 0;

            // {
            token = lexer.GetToken();
            if (token.Type != TokenType.OpenBrace)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }
            openBraceCnt++;

            Event hoi2Event = new Event();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                if (token.Type == TokenType.OpenBrace)
                {
                    openBraceCnt++;
                }

                // } (End of section)
                if (token.Type == TokenType.CloseBrace)
                {
                    openBraceCnt--;
                    if (openBraceCnt == 0)
                        break;
                }

                // Invalid tokens
                if (token.Type != TokenType.Identifier)
                {
                    continue;
                }

                string keyword = token.Value as string;
                if (string.IsNullOrEmpty(keyword))
                {
                    continue;
                }
                keyword = keyword.ToLower();

                // id id
                if (keyword.Equals("id"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Invalid tokens
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // event ID
                    hoi2Event.Id = (int)(double)token.Value;
                    continue;
                }

                // name
                if (keyword.Equals("name"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Invalid tokens
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String && token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    hoi2Event.Name = token.Value as string;
                    continue;
                }

                // desc
                if (keyword.Equals("desc"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Invalid tokens
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String && token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    hoi2Event.Desc = token.Value as string;
                    continue;
                }

                // country
                if (keyword.Equals("country"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Invalid tokens
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    hoi2Event.Country = token.Value as string;
                    continue;
                }

                // picture
                if (keyword.Equals("picture"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Invalid tokens
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String && token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }
                    hoi2Event.Picture = token.Value as string;

                    if (hoi2Event.Picture.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 || Regex.IsMatch(hoi2Event.Picture, @"\p{IsCyrillic}"))
                    {
                        Log.Warning("[Event] Picture name contains invalid char {0}:  L{1}", lexer.PathName, lexer.LineNo);
                    }
                    else
                    {
                        string imgFileName = Path.Combine(Game.EventPicturePathName, hoi2Event.Picture);
                        imgFileName += ".bmp";
                        string pathName = Game.GetReadFileName(imgFileName);
                        if (!File.Exists(pathName))
                        {
                            Log.Error("[Event] Picture not exist {0}:  L{1}", lexer.PathName, lexer.LineNo);
                        }
                    }
                    
                    continue;
                }

                if (keyword.Equals("trigger"))
                {
                    ParseTrigger(lexer);
                    continue;
                }

                if (keyword.Equals("decision_trigger"))
                {
                    ParseTrigger(lexer);
                    continue;
                }

                //event action names
                if (keyword.Equals("action") || keyword.IndexOf("action_") == 0)
                {
                    string actionName = ParseAction(lexer);
                    if (actionName != null)
                        hoi2Event.ActionNames.Add(actionName);
                    continue;
                }
            }
            if (openBraceCnt != 0)
            {
                Log.MissingCloseBrace(LogCategory, "event", lexer);
            }
            return hoi2Event;
        }

        /// <summary>
        ///     Synthetic analysis of Event action section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>action name</returns>
        private static string ParseAction(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            // {
            token = lexer.GetToken();
            if (token.Type != TokenType.OpenBrace)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            int openBraceCnt = 0;
            string actionName = "UNKNOWN_STRING";
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                if (token.Type == TokenType.OpenBrace)
                {
                    openBraceCnt++;
                }

                // } (End of section)
                if (token.Type == TokenType.CloseBrace)
                {
                    if (openBraceCnt == 0)
                        break;
                    openBraceCnt--;
                }

                // Invalid tokens
                if (token.Type != TokenType.Identifier)
                {
                    continue;
                }

                string keyword = token.Value as string;
                if (string.IsNullOrEmpty(keyword))
                {
                    continue;
                }
                keyword = keyword.ToLower();

                // id id
                if (keyword.Equals("name"))
                {
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Invalid tokens
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String && token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Event action name
                    actionName = token.Value as string;
                    continue;
                }

                if (keyword.Equals("command"))
                {
                    ParseCommand(lexer);
                    continue;
                }
            }

            return actionName;
        }

        /// <summary>
        ///     Synthetic analysis of Event command section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>none none</returns>
        private static string ParseCommand(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            // {
            token = lexer.GetToken();
            if (token.Type != TokenType.OpenBrace)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            int openBraceCnt = 0;
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                if (token.Type == TokenType.OpenBrace)
                {
                    openBraceCnt++;
                }

                // } (End of section)
                if (token.Type == TokenType.CloseBrace)
                {
                    if (openBraceCnt == 0)
                        break;
                    openBraceCnt--;
                }
            }

            return "";
        }

        /// <summary>
        ///     Synthetic analysis of Event trigger section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>none none</returns>
        private static string ParseTrigger(TextLexer lexer)
        {
            int startLine = lexer.LineNo;
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }
            int openBraceCnt = 0;
            // {
            token = lexer.GetToken();
            if (token.Type != TokenType.OpenBrace)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }
            openBraceCnt++;


            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                if (token.Type == TokenType.OpenBrace)
                {
                    openBraceCnt++;
                }

                // } (End of section)
                if (token.Type == TokenType.CloseBrace)
                {
                    openBraceCnt--;
                    if (openBraceCnt == 0)
                        break;
                }
            }
            if (openBraceCnt != 0)
            {
                Log.Error("[Event] Missing close brace. Section begin L{0}  ({1} L{2})", startLine, lexer.PathName, lexer.LineNo);
            }

            return "";
        }

        #endregion
    }
}
