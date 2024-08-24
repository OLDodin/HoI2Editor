using System;
using System.Collections.Generic;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     Trigger parsing class
    /// </summary>
    public static class TriggerParser
    {
        #region Internal field

        /// <summary>
        ///     Trigger type string and ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, TriggerType> TypeMap = new Dictionary<string, TriggerType>();

        #endregion

        #region Internal constant

        /// <summary>
        ///     Category name at the time of log output
        /// </summary>
        private const string LogCategory = "Command-Trigger";

        #endregion

        #region Initialization

        /// <summary>
        ///     Static constructor
        /// </summary>
        static TriggerParser()
        {
            foreach (TriggerType type in Enum.GetValues(typeof (TriggerType)))
            {
                TypeMap.Add(Trigger.TypeStringTable[(int) type], type);
            }
        }

        #endregion

        #region Parsing

        /// <summary>
        ///     trigger Parse the section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Trigger list</returns>
        public static List<Trigger> Parse(TextLexer lexer)
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

            return ParseContainerTrigger(lexer);
        }

        /// <summary>
        ///     Parsing the trigger container
        /// </summary>
        /// <param name="lexer"></param>
        /// <returns></returns>
        private static List<Trigger> ParseContainerTrigger(TextLexer lexer)
        {
            List<Trigger> list = new List<Trigger>();
            int lastLineNo = lexer.LineNo;
            while (true)
            {
                Token token = lexer.GetToken();

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

                // Invalid keyword
                if (!TypeMap.ContainsKey(keyword))
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

                Trigger trigger = new Trigger();
                trigger.Type = TypeMap[keyword];
                trigger.LineNum = lexer.LineNo;

                // = =
                token = lexer.GetToken();
                if (token.Type != TokenType.Equal)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                token = lexer.GetToken();
                if (token.Type == TokenType.OpenBrace)
                {
                    List<Trigger> triggers = ParseContainerTrigger(lexer);
                    if (triggers == null)
                    {
                        continue;
                    }
                    trigger.Value = triggers;
                    list.Add(trigger);

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                if (token.Type != TokenType.Number &&
                    token.Type != TokenType.Identifier &&
                    token.Type != TokenType.String)
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

                trigger.Value = token.Value;

                list.Add(trigger);

                // Remember the final interpretation line
                lastLineNo = lexer.LineNo;
            }

            if (list.Count == 0)
            {
                Log.Info("[Trigger] Empty trigger {0}:  L{1}", lexer.PathName, lexer.LineNo);
            }

            return list.Count > 0 ? list : null;
        }

        #endregion
    }
}
