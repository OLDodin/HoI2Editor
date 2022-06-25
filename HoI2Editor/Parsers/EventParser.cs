using System;
using System.Collections.Generic;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

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
            using (TextLexer lexer = new TextLexer(fileName, true, textCodePage))
            {
                return ParseEvents(lexer);
            }
        }

        /// <summary>
        ///     Synthetic analysis of the Event section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>Events List</returns>
        private static List<Event> ParseEvents(TextLexer lexer)
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
                    Event hoi2Event = ParseEvent(lexer);
                    if (hoi2Event == null)
                    {
                        Log.InvalidSection(LogCategory, "hoi2Event", lexer);
                        continue;
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

            // {
            token = lexer.GetToken();
            if (token.Type != TokenType.OpenBrace)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            int openBraceCnt = 0;
            Event group = new Event();
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

                    // Technical group ID
                    group.Id = (int)(double)token.Value;
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

                    group.Name = token.Value as string;
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

                    group.Desc = token.Value as string;
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
                        group.ActionNames.Add(actionName);
                    continue;
                }
            }

            return group;
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
            string actionName = "";
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

        #endregion
    }
}
