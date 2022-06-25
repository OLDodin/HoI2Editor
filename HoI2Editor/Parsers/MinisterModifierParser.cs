﻿using System.Collections.Generic;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     Parsing the ministerial trait definition file (AoD)
    /// </summary>
    internal class MinisterModifierParser
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
            List<MinisterPersonalityInfo> list = null;

            using (TextLexer lexer = new TextLexer(fileName, true))
            {
                while (true)
                {
                    Token token = lexer.GetToken();

                    // End of file
                    if (token == null)
                    {
                        return list;
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

                    // minister_modifiers section
                    if (keyword.Equals("minister_modifiers"))
                    {
                        if (!ParseMinisterModifiers(lexer))
                        {
                            Log.InvalidSection(LogCategory, "minister_modifiers", lexer);
                        }
                        continue;
                    }

                    // minister_personalities section
                    if (keyword.Equals("minister_personalities"))
                    {
                        list = ParseMinisterPersonalities(lexer);
                        continue;
                    }

                    Log.InvalidToken(LogCategory, token, lexer);
                }
            }
        }

        /// <summary>
        ///     minister_modifiers Parse the section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Success or failure of parsing</returns>
        private static bool ParseMinisterModifiers(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return false;
            }

            // {
            token = lexer.GetToken();
            if (token.Type != TokenType.OpenBrace)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return false;
            }

            while (true)
            {
                // preliminary :: Skip the identifier
                token = lexer.GetToken();
                if (token.Type == TokenType.Identifier)
                {
                    lexer.SkipLine();
                    continue;
                }

                // } ( Section end )
                if (token.Type == TokenType.CloseBrace)
                {
                    break;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     minister_personalities Parse the section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Ministerial Characteristic List</returns>
        private static List<MinisterPersonalityInfo> ParseMinisterPersonalities(TextLexer lexer)
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

            List<MinisterPersonalityInfo> list = new List<MinisterPersonalityInfo>();
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
                if (token.Type != TokenType.Identifier || !((string) token.Value).Equals("personality"))
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                // personality section
                MinisterPersonalityInfo info = ParseMinisterPersonality(lexer);
                if (info == null)
                {
                    Log.InvalidSection(LogCategory, "personality", lexer);
                    continue;
                }

                // Registered in the Ministerial Characteristic List
                list.Add(info);
            }

            return list;
        }

        /// <summary>
        ///     personality Parse the section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Ministerial characteristic data</returns>
        private static MinisterPersonalityInfo ParseMinisterPersonality(TextLexer lexer)
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

                // personality_string
                if (keyword.Equals("personality_string"))
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

                // minister_position
                if (keyword.Equals("minister_position"))
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
                    else if (Game.Type != GameType.DarkestHour || !position.Equals("generic"))
                    {
                        // Invalid token
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                    }
                    continue;
                }

                // modifier
                if (keyword.Equals("modifier"))
                {
                    if (!ParseMinisterPersonalityModifier(lexer))
                    {
                        Log.InvalidSection(LogCategory, "modifier", lexer);
                    }
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
            }

            return info;
        }

        /// <summary>
        ///     modifier Parse the section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Success or failure of parsing</returns>
        private static bool ParseMinisterPersonalityModifier(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return false;
            }

            // {
            token = lexer.GetToken();
            if (token.Type != TokenType.OpenBrace)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return false;
            }

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
                    return false;
                }

                string keyword = token.Value as string;
                if (keyword == null)
                {
                    return false;
                }

                // type
                if (keyword.Equals("type"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }

                    // identifier
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }
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
                        return false;
                    }

                    // identifier / / Numbers
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }
                    continue;
                }

                // option1
                if (keyword.Equals("option1"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }

                    // Numbers
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }
                    continue;
                }

                // option2
                if (keyword.Equals("option2"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }

                    // Numbers
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }
                    continue;
                }

                // modifier_effect
                if (keyword.Equals("modifier_effect"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }

                    // Numbers
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }
                    continue;
                }

                // division
                if (keyword.Equals("division"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }

                    // identifier
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }
                    continue;
                }

                // extra
                if (keyword.Equals("extra"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }

                    // identifier
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        return false;
                    }
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                return false;
            }

            return true;
        }

        #endregion
    }
}
