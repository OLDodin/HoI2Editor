using System.Collections.Generic;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     Technical data syntax analysis class
    /// </summary>
    public static class TechParser
    {
        #region Internal fixed number

        /// <summary>
        ///     Category name at the time of log output
        /// </summary>
        private const string LogCategory = "Tech";

        #endregion

        #region Parsing

        /// <summary>
        ///     Synthetic analysis of technical files
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <returns>Technical group data</returns>
        public static TechGroup Parse(string fileName)
        {
            using (TextLexer lexer = new TextLexer(fileName, true))
            {
                Token token = lexer.GetToken();
                // Invalid tokens
                if (token.Type != TokenType.Identifier)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    return null;
                }

                string s = token.Value as string;
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                s = s.ToLower();

                // technology
                if (s.Equals("technology"))
                {
                    TechGroup group = ParseTechnology(lexer);
                    if (group == null)
                    {
                        Log.InvalidSection(LogCategory, "technology", lexer);
                    }
                    return group;
                }

                // Invalid tokens
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }
        }

        /// <summary>
        ///     Synthetic analysis of the Technology section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>Technical group data</returns>
        private static TechGroup ParseTechnology(TextLexer lexer)
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

            TechGroup group = new TechGroup();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                // } (End of section)
                if (token.Type == TokenType.CloseBrace)
                {
                    break;
                }

                // Invalid tokens
                if (token.Type != TokenType.Identifier)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
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
                    group.Id = (int) (double) token.Value;
                    continue;
                }

                // category
                if (keyword.Equals("category"))
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

                    // Invalid category string
                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    if (!Techs.CategoryMap.ContainsKey(s))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Technical category
                    group.Category = Techs.CategoryMap[s];
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
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Technical group name
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
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Technical group explanation
                    group.Desc = token.Value as string;
                    continue;
                }

                // label label
                if (keyword.Equals("label"))
                {
                    TechLabel label = ParseLabel(lexer);
                    if (label == null)
                    {
                        Log.InvalidSection(LogCategory, "label", lexer);
                        continue;
                    }

                    // label label
                    group.Items.Add(label);
                    continue;
                }

                // event event
                if (keyword.Equals("event"))
                {
                    TechEvent ev = ParseEvent(lexer);
                    if (ev == null)
                    {
                        Log.InvalidSection(LogCategory, "event", lexer);
                        continue;
                    }

                    // Technical event
                    group.Items.Add(ev);
                    continue;
                }

                // application
                if (keyword.Equals("application"))
                {
                    TechItem application = ParseApplication(lexer);
                    if (application == null)
                    {
                        Log.InvalidSection(LogCategory, "application", lexer);
                        continue;
                    }

                    // technology
                    group.Items.Add(application);
                    continue;
                }

                // Invalid tokens
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return group;
        }

        /// <summary>
        ///     Synthetic analysis of the Application section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>Technical data</returns>
        private static TechItem ParseApplication(TextLexer lexer)
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

            TechItem application = new TechItem();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                // } (End of section)
                if (token.Type == TokenType.CloseBrace)
                {
                    break;
                }

                // Invalid tokens
                if (token.Type != TokenType.Identifier)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
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

                    // Technical ID
                    application.Id = (int) (double) token.Value;
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
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Technical name
                    application.Name = token.Value as string;

                    // Shortening name
                    application.ShortName = "SHORT_" + application.Name;
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
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Technical explanation
                    application.Desc = token.Value as string;
                    continue;
                }

                // position position
                if (keyword.Equals("position"))
                {
                    TechPosition position = ParsePosition(lexer);
                    if (position == null)
                    {
                        Log.InvalidSection(LogCategory, "position", lexer);
                        continue;
                    }

                    // Coordinates
                    application.Positions.Add(position);
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
                    if (token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Image file name
                    application.PictureName = token.Value as string;
                    continue;
                }

                // year year
                if (keyword.Equals("year"))
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

                    // Historical year
                    application.Year = (int) (double) token.Value;
                    continue;
                }

                // component
                if (keyword.Equals("component"))
                {
                    TechComponent component = ParseComponent(lexer);
                    if (component == null)
                    {
                        Log.InvalidSection(LogCategory, "component", lexer);
                        continue;
                    }

                    // Small study
                    application.Components.Add(component);
                    continue;
                }

                // required
                if (keyword.Equals("required"))
                {
                    IEnumerable<int> ids = ParseRequired(lexer);
                    if (ids == null)
                    {
                        Log.InvalidSection(LogCategory, "required", lexer);
                        continue;
                    }

                    // The necessary technical groups (AND)
                    foreach (int id in ids)
                    {
                        RequiredTech tech = new RequiredTech { Id = id };
                        application.AndRequiredTechs.Add(tech);
                    }
                    continue;
                }

                // or_required
                if (keyword.Equals("or_required"))
                {
                    IEnumerable<int> ids = ParseRequired(lexer);
                    if (ids == null)
                    {
                        Log.InvalidSection(LogCategory, "or_required", lexer);
                        continue;
                    }

                    // The necessary technical groups (OR)
                    foreach (int id in ids)
                    {
                        RequiredTech tech = new RequiredTech { Id = id };
                        application.OrRequiredTechs.Add(tech);
                    }
                    continue;
                }

                // effects effects
                if (keyword.Equals("effects"))
                {
                    IEnumerable<Command> commands = ParseEffects(lexer);
                    if (commands == null)
                    {
                        Log.InvalidSection(LogCategory, "effects", lexer);
                        continue;
                    }

                    // Technical effect
                    application.Effects.AddRange(commands);
                    continue;
                }

                // Invalid tokens
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return application;
        }

        /// <summary>
        ///     Synthetic analysis of the Label section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>Technical label data</returns>
        private static TechLabel ParseLabel(TextLexer lexer)
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

            TechLabel label = new TechLabel();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                // } (End of section)
                if (token.Type == TokenType.CloseBrace)
                {
                    break;
                }

                // Invalid tokens
                if (token.Type != TokenType.Identifier)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                string keyword = token.Value as string;
                if (string.IsNullOrEmpty(keyword))
                {
                    continue;
                }
                keyword = keyword.ToLower();

                // tag
                if (keyword.Equals("tag"))
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
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Tag name
                    label.Name = token.Value as string;
                    continue;
                }

                // position position
                if (keyword.Equals("position"))
                {
                    TechPosition position = ParsePosition(lexer);
                    if (position == null)
                    {
                        Log.InvalidSection(LogCategory, "position", lexer);
                        continue;
                    }

                    // Coordinates
                    label.Positions.Add(position);
                    continue;
                }

                // Invalid tokens
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return label;
        }

        /// <summary>
        ///     Synthetic analysis of Event section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>Technical event data</returns>
        private static TechEvent ParseEvent(TextLexer lexer)
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

            TechEvent ev = new TechEvent();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                // } (End of section)
                if (token.Type == TokenType.CloseBrace)
                {
                    break;
                }

                // Invalid tokens
                if (token.Type != TokenType.Identifier)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
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

                    // Technical event ID
                    ev.Id = (int) (double) token.Value;
                    continue;
                }

                // position position
                if (keyword.Equals("position"))
                {
                    TechPosition position = ParsePosition(lexer);
                    if (position == null)
                    {
                        Log.InvalidSection(LogCategory, "position", lexer);
                        continue;
                    }

                    // Coordinates
                    ev.Positions.Add(position);
                    continue;
                }

                // technology
                if (keyword.Equals("technology"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidSection(LogCategory, "technology", lexer);
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

                    // Technical ID
                    ev.TechId = (int) (double) token.Value;
                    continue;
                }

                // Invalid tokens
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return ev;
        }

        /// <summary>
        ///     Synthetic analysis of the Posotion section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>Coordinate data</returns>
        private static TechPosition ParsePosition(TextLexer lexer)
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

            TechPosition position = new TechPosition();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                // } (End of section)
                if (token.Type == TokenType.CloseBrace)
                {
                    break;
                }

                // Invalid tokens
                if (token.Type != TokenType.Identifier)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                string keyword = token.Value as string;
                if (string.IsNullOrEmpty(keyword))
                {
                    continue;
                }
                keyword = keyword.ToLower();

                // x x
                if (keyword.Equals("x"))
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

                    // X Block
                    position.X = (int) (double) token.Value;
                    continue;
                }

                // y y
                if (keyword.Equals("y"))
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

                    // Block
                    position.Y = (int) (double) token.Value;
                    continue;
                }

                // Invalid tokens
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return position;
        }

        /// <summary>
        ///     Synthetic analysis of the Component section
        /// </summary>
        /// <param name="lexer">Word parser</param>
        /// <returns>Small research data</returns>
        private static TechComponent ParseComponent(TextLexer lexer)
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

            TechComponent component = new TechComponent();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    break;
                }

                // } (End of section)
                if (token.Type == TokenType.CloseBrace)
                {
                    break;
                }

                // Invalid tokens
                if (token.Type != TokenType.Identifier)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
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

                    // Small research ID
                    component.Id = (int) (double) token.Value;
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
                    if (token.Type != TokenType.Identifier && token.Type != TokenType.String)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Small research name
                    component.Name = token.Value as string;
                    continue;
                }

                // type
                if (keyword.Equals("type"))
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

                    // Involvable research characteristic character string
                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    s = s.ToLower();
                    if (!Techs.SpecialityStringMap.ContainsKey(s))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Small research characteristics
                    component.Speciality = Techs.SpecialityStringMap[s];
                    continue;
                }

                // difficulty difficulty
                if (keyword.Equals("difficulty"))
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

                    // Degree of difficulty
                    component.Difficulty = (int) (double) token.Value;
                    continue;
                }

                // double_time
                if (keyword.Equals("double_time"))
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

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    s = s.ToLower();

                    if (s.Equals("yes"))
                    {
                        // Whether it takes twice as much time
                        component.DoubleTime = true;
                        continue;
                    }

                    if (s.Equals("no"))
                    {
                        // Whether it takes twice as much time
                        component.DoubleTime = false;
                        continue;
                    }

                    // Invalid tokens
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                // Invalid tokens
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return component;
        }

        /// <summary>
        ///     Parse the required / or required section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Technology ID list</returns>
        private static IEnumerable<int> ParseRequired(TextLexer lexer)
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

            List<int> list = new List<int>();
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
                if (token.Type != TokenType.Number)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                list.Add((int) (double) token.Value);
            }

            return list;
        }

        /// <summary>
        ///     effects effects Parse the section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Command list</returns>
        private static IEnumerable<Command> ParseEffects(TextLexer lexer)
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

            List<Command> list = new List<Command>();
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
                    lexer.SkipLine();
                    continue;
                }

                string keyword = token.Value as string;
                if (string.IsNullOrEmpty(keyword))
                {
                    continue;
                }
                keyword = keyword.ToLower();

                // command command
                if (keyword.Equals("command"))
                {
                    Command command = CommandParser.Parse(lexer);
                    if (command == null)
                    {
                        Log.InvalidSection(LogCategory, "command", lexer);
                        continue;
                    }
                    if (command.Type == CommandType.None)
                    {
                        continue;
                    }

                    // command
                    list.Add(command);
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return list;
        }

        #endregion
    }
}
