using System.Linq;
using System.Text;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     misc File syntax analysis class
    /// </summary>
    public static class MiscParser
    {
        #region Internal constant

        /// <summary>
        ///     Category name at the time of log output
        /// </summary>
        private const string LogCategory = "Misc";

        #endregion

        #region Parsing

        /// <summary>
        ///     misc Parse the file
        /// </summary>
        /// <param name="fileName">file name</param>
        public static void Parse(string fileName)
        {
            // Set the game type
            MiscGameType type = Misc.GetGameType();

            using (TextLexer lexer = new TextLexer(fileName, false))
            {
                while (true)
                {
                    Token token = lexer.GetToken();

                    // End of file
                    if (token == null)
                    {
                        return;
                    }

                    // Blank character / / Skip comments
                    if (token.Type == TokenType.WhiteSpace || token.Type == TokenType.Comment)
                    {
                        continue;
                    }

                    // Invalid token
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    string keyword = token.Value as string;
                    if (string.IsNullOrEmpty(keyword))
                    {
                        continue;
                    }

                    // economy section
                    if (keyword.Equals("economy"))
                    {
                        if (!ParseSection(MiscSectionId.Economy, type, lexer))
                        {
                            Log.InvalidSection(LogCategory, "economy", lexer);
                        }
                        continue;
                    }

                    // intelligence section
                    if (keyword.Equals("intelligence"))
                    {
                        if (!ParseSection(MiscSectionId.Intelligence, type, lexer))
                        {
                            Log.InvalidSection(LogCategory, "intelligence", lexer);
                        }
                        continue;
                    }

                    // diplomacy section
                    if (keyword.Equals("diplomacy"))
                    {
                        if (!ParseSection(MiscSectionId.Diplomacy, type, lexer))
                        {
                            Log.InvalidSection(LogCategory, "diplomacy", lexer);
                        }
                        continue;
                    }

                    // combat section
                    if (keyword.Equals("combat"))
                    {
                        if (!ParseSection(MiscSectionId.Combat, type, lexer))
                        {
                            Log.InvalidSection(LogCategory, "combat", lexer);
                        }
                        continue;
                    }

                    // mission mission section
                    if (keyword.Equals("mission"))
                    {
                        if (!ParseSection(MiscSectionId.Mission, type, lexer))
                        {
                            Log.InvalidSection(LogCategory, "mission", lexer);
                        }
                        continue;
                    }

                    // country country section
                    if (keyword.Equals("country"))
                    {
                        if (!ParseSection(MiscSectionId.Country, type, lexer))
                        {
                            Log.InvalidSection(LogCategory, "country", lexer);
                        }
                        continue;
                    }

                    // research section
                    if (keyword.Equals("research"))
                    {
                        if (!ParseSection(MiscSectionId.Research, type, lexer))
                        {
                            Log.InvalidSection(LogCategory, "research", lexer);
                        }
                        continue;
                    }

                    // trade section
                    if (keyword.Equals("trade"))
                    {
                        if (!ParseSection(MiscSectionId.Trade, type, lexer))
                        {
                            Log.InvalidSection(LogCategory, "trade", lexer);
                        }
                        continue;
                    }

                    // ai section
                    if (keyword.Equals("ai"))
                    {
                        if (!ParseSection(MiscSectionId.Ai, type, lexer))
                        {
                            Log.InvalidSection(LogCategory, "ai", lexer);
                        }
                        continue;
                    }

                    // mod mod section
                    if (keyword.Equals("mod"))
                    {
                        if (!ParseSection(MiscSectionId.Mod, type, lexer))
                        {
                            Log.InvalidSection(LogCategory, "mod", lexer);
                        }
                        continue;
                    }

                    // map section
                    if (keyword.Equals("map"))
                    {
                        if (!ParseSection(MiscSectionId.Map, type, lexer))
                        {
                            Log.InvalidSection(LogCategory, "map", lexer);
                        }
                        continue;
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                }
            }
        }

        /// <summary>
        ///     Parse the section
        /// </summary>
        /// <param name="section">section ID</param>
        /// <param name="type">Game type</param>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Success or failure of parsing</returns>
        private static bool ParseSection(MiscSectionId section, MiscGameType type, TextLexer lexer)
        {
            // Blank character / / Skip comments
            Token token;
            while (true)
            {
                token = lexer.GetToken();
                if (token.Type != TokenType.WhiteSpace && token.Type != TokenType.Comment)
                {
                    break;
                }
            }

            // = =
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return false;
            }

            // Blank character / / Skip comments
            while (true)
            {
                token = lexer.GetToken();
                if (token.Type != TokenType.WhiteSpace && token.Type != TokenType.Comment)
                {
                    break;
                }
            }

            // {
            if (token.Type != TokenType.OpenBrace)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return false;
            }

            StringBuilder sb;
            foreach (MiscItemId id in Misc.SectionItems[(int) section]
                .Where(id => Misc.ItemTable[(int) id, (int) type]))
            {
                // White space / / Save comment
                sb = new StringBuilder();
                while (true)
                {
                    token = lexer.GetToken();
                    if (token.Type != TokenType.WhiteSpace && token.Type != TokenType.Comment)
                    {
                        break;
                    }
                    sb.Append(token.Value);
                }
                Misc.SetComment(id, sb.ToString());

                // Set value
                if (token.Type != TokenType.Number)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    return false;
                }
                switch (Misc.ItemTypes[(int) id])
                {
                    case MiscItemType.Bool:
                        Misc.SetItem(id, (int) (double) token.Value != 0);
                        break;

                    case MiscItemType.Enum:
                    case MiscItemType.Int:
                    case MiscItemType.PosInt:
                    case MiscItemType.NonNegInt:
                    case MiscItemType.NonPosInt:
                    case MiscItemType.NonNegIntMinusOne:
                    case MiscItemType.NonNegInt1:
                    case MiscItemType.RangedInt:
                    case MiscItemType.RangedPosInt:
                    case MiscItemType.RangedIntMinusOne:
                    case MiscItemType.RangedIntMinusThree:
                        Misc.SetItem(id, (int) (double) token.Value);
                        break;

                    case MiscItemType.Dbl:
                    case MiscItemType.PosDbl:
                    case MiscItemType.NonNegDbl:
                    case MiscItemType.NonPosDbl:
                    case MiscItemType.NonNegDbl0:
                    case MiscItemType.NonNegDbl2:
                    case MiscItemType.NonNegDbl5:
                    case MiscItemType.NonPosDbl0:
                    case MiscItemType.NonPosDbl2:
                    case MiscItemType.NonNegDblMinusOne:
                    case MiscItemType.NonNegDblMinusOne1:
                    case MiscItemType.NonNegDbl2AoD:
                    case MiscItemType.NonNegDbl4Dda13:
                    case MiscItemType.NonNegDbl2Dh103Full:
                    case MiscItemType.NonNegDbl2Dh103Full1:
                    case MiscItemType.NonNegDbl2Dh103Full2:
                    case MiscItemType.NonPosDbl5AoD:
                    case MiscItemType.NonPosDbl2Dh103Full:
                    case MiscItemType.RangedDbl:
                    case MiscItemType.RangedDblMinusOne:
                    case MiscItemType.RangedDblMinusOne1:
                    case MiscItemType.RangedDbl0:
                    case MiscItemType.NonNegIntNegDbl:
                        Misc.SetItem(id, (double) token.Value);
                        break;
                }
            }

            // White space at the end of the section / / Save comment
            sb = new StringBuilder();
            while (true)
            {
                token = lexer.GetToken();
                if (token.Type != TokenType.WhiteSpace && token.Type != TokenType.Comment)
                {
                    break;
                }
                sb.Append(token.Value);
            }
            Misc.SetSuffix(section, sb.ToString());

            // } ( Section end )
            if (token.Type != TokenType.CloseBrace)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return false;
            }

            return true;
        }

        #endregion
    }
}
