using System.Collections.Generic;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     Parsing the ministerial trait definition file (DH)
    /// </summary>
    public static class MinisterPersonalityParser
    {
        #region Internal constant

        /// <summary>
        ///     With the name of the ministerial status in the ministerial characteristic definition file ID Correspondence of
        /// </summary>
        private static readonly Dictionary<string, int> PositionMap
            = new Dictionary<string, int>
            {
                { "", (int) MinisterPosition.None },
                { "headofstate", (int) MinisterPosition.HeadOfState },
                { "headofgovernment", (int) MinisterPosition.HeadOfGovernment },
                { "foreignminister", (int) MinisterPosition.ForeignMinister },
                { "armamentminister", (int) MinisterPosition.MinisterOfArmament },
                { "ministerofsecurity", (int) MinisterPosition.MinisterOfSecurity },
                { "ministerofintelligence", (int) MinisterPosition.HeadOfMilitaryIntelligence },
                { "chiefofstaff", (int) MinisterPosition.ChiefOfStaff },
                { "chiefofarmy", (int) MinisterPosition.ChiefOfArmy },
                { "chiefofnavy", (int) MinisterPosition.ChiefOfNavy },
                { "chiefofair", (int) MinisterPosition.ChiefOfAirForce }
            };

        #endregion

        #region Internal constant

        /// <summary>
        ///     Category name at the time of log output
        /// </summary>
        private const string LogCategory = "Minister";

        #endregion

        #region Parsing

        /// <summary>
        ///     Parsing the ministerial trait definition file
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <returns>Ministerial Characteristic List</returns>
        public static List<MinisterPersonalityInfo> Parse(string fileName)
        {
            List<MinisterPersonalityInfo> list = new List<MinisterPersonalityInfo>();

            using (TextLexer lexer = new TextLexer(fileName, true))
            {
                while (true)
                {
                    Token token = lexer.GetToken();

                    // End of file
                    if (token == null)
                    {
                        break;
                    }

                    // Invalid token
                    if (token.Type != TokenType.Identifier || !((string) token.Value).Equals("minister"))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // minister section
                    MinisterPersonalityInfo info = ParseMinister(lexer);
                    if (info == null)
                    {
                        Log.InvalidSection(LogCategory, "minister", lexer);
                    }

                    // Registered in the Ministerial Characteristic List
                    list.Add(info);
                }

                return list;
            }
        }

        /// <summary>
        ///     minister Parse the section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Ministerial characteristic data</returns>
        private static MinisterPersonalityInfo ParseMinister(TextLexer lexer)
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

            MinisterPersonalityInfo info = new MinisterPersonalityInfo();
            while (true)
            {
                // File termination
                token = lexer.GetToken();
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
                    return null;
                }

                string keyword = token.Value as string;
                if (keyword == null)
                {
                    return null;
                }

                // trait
                if (keyword.Equals("trait"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Invalid token
                    token = lexer.GetToken();
                    if (token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Ministerial characteristic character string
                    info.String = token.Value as string;
                    continue;
                }

                // id id
                if (keyword.Equals("id"))
                {
                    // preliminary : 1 Skip line by line
                    lexer.SkipLine();
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

                    // Invalid token
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Ministerial characteristic name
                    info.Name = token.Value as string;
                    continue;
                }

                // desc
                if (keyword.Equals("desc"))
                {
                    // preliminary : 1 Skip line by line
                    lexer.SkipLine();
                    continue;
                }

                // position position
                if (keyword.Equals("position"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Invalid token
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    string position = token.Value as string;
                    if (string.IsNullOrEmpty(position))
                    {
                        continue;
                    }
                    position = position.ToLower();

                    // Ministerial status
                    if (PositionMap.ContainsKey(position))
                    {
                        // either 1 One
                        info.Position[PositionMap[position]] = true;
                    }
                    else if (position.Equals("all"))
                    {
                        // all
                        for (int i = 0; i < info.Position.Length; i++)
                        {
                            info.Position[i] = true;
                        }
                    }
                    else if (!position.Equals("generic"))
                    {
                        // Invalid token
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                    }
                    continue;
                }

                // value value
                if (keyword.Equals("value"))
                {
                    // preliminary : 1 Skip line by line
                    lexer.SkipLine();
                    continue;
                }

                // command command
                if (keyword.Equals("command"))
                {
                    // preliminary : 1 Skip line by line
                    lexer.SkipLine();
                    //continue; continue;
                }
            }

            return info;
        }

        #endregion
    }
}
