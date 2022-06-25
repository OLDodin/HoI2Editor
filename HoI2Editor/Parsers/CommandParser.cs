using System.Collections.Generic;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     Command parsing class
    /// </summary>
    public static class CommandParser
    {
        #region Internal fixed number

        /// <summary>
        ///     Category name at the time of log output
        /// </summary>
        private const string LogCategory = "Command";

        #endregion

        #region Parsing

        /// <summary>
        ///     command command Parse the section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>command</returns>
        public static Command Parse(TextLexer lexer)
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

            Command command = new Command();
            int lastLineNo = lexer.LineNo;
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                // } ( Section end )
                if (token.Type == TokenType.CloseBrace)
                {
                    break;
                }

                // Invalid token
                if (token.Type != TokenType.Identifier)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    if (lexer.LineNo != lastLineNo)
                    {
                        // If the current line is different from the last interpreted line, it is considered that the closing parenthesis is missing.
                        lexer.ReserveToken(token);
                        break;
                    }
                    continue;
                }

                string keyword = token.Value as string;
                if (string.IsNullOrEmpty(keyword))
                {
                    continue;
                }
                keyword = keyword.ToLower();

                // type
                if (keyword.Equals("type"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Invalid token
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Invalid command type string
                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    s = s.ToLower();
                    if (!Commands.StringMap.ContainsKey(s))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Command type
                    command.Type = Commands.StringMap[s];

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // which which
                if (keyword.Equals("which"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Invalid token
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Number && token.Type != TokenType.Identifier &&
                        token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Parameters --which which
                    command.Which = token.Value;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // value value
                if (keyword.Equals("value"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Invalid token
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Number && token.Type != TokenType.Identifier &&
                        token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Parameters --value
                    command.Value = token.Value;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // when
                if (keyword.Equals("when"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Invalid token
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Number && token.Type != TokenType.Identifier &&
                        token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Parameters --when
                    command.When = token.Value;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // where
                if (keyword.Equals("where"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Invalid token
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Number && token.Type != TokenType.Identifier &&
                        token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Parameters --where
                    command.Where = token.Value;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // trigger
                if (keyword.Equals("trigger"))
                {
                    List<Trigger> triggers = TriggerParser.Parse(lexer);
                    if (triggers == null)
                    {
                        continue;
                    }

                    // trigger
                    command.Triggers.AddRange(triggers);

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                if (lexer.LineNo != lastLineNo)
                {
                    // If the current line is different from the last interpreted line, it is considered that the closing parenthesis is missing.
                    lexer.ReserveToken(token);
                    break;
                }
            }

            return command;
        }

        #endregion
    }
}
