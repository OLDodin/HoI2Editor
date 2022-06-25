using System.Collections.Generic;
using System.Linq;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     Parsing class for unit data
    /// </summary>
    public class UnitParser
    {
        #region Internal constant

        /// <summary>
        ///     Category name at the time of log output
        /// </summary>
        private const string LogCategory = "Unit";

        #endregion

        #region Parsing

        /// <summary>
        ///     Parsing the unit file
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="unit">Unit data</param>
        /// <returns>Success or failure of parsing</returns>
        public static bool Parse(string fileName, UnitClass unit)
        {
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
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    string keyword = token.Value as string;
                    if (string.IsNullOrEmpty(keyword))
                    {
                        return false;
                    }
                    keyword = keyword.ToLower();

                    // allowed_brigades
                    if (keyword.Equals("allowed_brigades"))
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

                        string s = token.Value as string;
                        if (string.IsNullOrEmpty(s))
                        {
                            return false;
                        }
                        s = s.ToLower();

                        // Other than the unit class name
                        if (!Units.StringMap.ContainsKey(s))
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }

                        // Unsupported unit class
                        UnitType brigade = Units.StringMap[s];
                        if (!Units.BrigadeTypes.Contains(brigade))
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }

                        // Brigade name
                        unit.AllowedBrigades.Add(brigade);
                        continue;
                    }

                    // max_allowed_brigades
                    if (keyword.Equals("max_allowed_brigades"))
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
                        if (token.Type != TokenType.Number)
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }

                        // Maximum number of brigades
                        unit.MaxAllowedBrigades = (int) (double) token.Value;
                        continue;
                    }

                    // upgrade
                    if (keyword.Equals("upgrade"))
                    {
                        UnitUpgrade upgrade = ParseUpgrade(lexer);
                        if (upgrade == null)
                        {
                            Log.InvalidSection(LogCategory, "upgrade", lexer);
                            continue;
                        }

                        // Improvement information
                        unit.Upgrades.Add(upgrade);
                        continue;
                    }

                    // model
                    if (keyword.Equals("model"))
                    {
                        UnitModel model = ParseModel(lexer);
                        if (model == null)
                        {
                            Log.InvalidSection(LogCategory, "model", lexer);
                            continue;
                        }

                        // Initial setting of automatic improvement destination
                        if (!model.AutoUpgrade)
                        {
                            model.UpgradeClass = unit.Type;
                            model.UpgradeModel = unit.Models.Count + 1;
                        }

                        // Unit model
                        unit.Models.Add(model);
                        continue;
                    }

                    if (Game.Type == GameType.ArsenalOfDemocracy)
                    {
                        // land_unit_type
                        if (keyword.Equals("land_unit_type"))
                        {
                            // = =
                            token = lexer.GetToken();
                            if (token.Type != TokenType.Equal)
                            {
                                Log.InvalidToken(LogCategory, token, lexer);
                                lexer.SkipLine();
                                continue;
                            }

                            // 1
                            token = lexer.GetToken();
                            if (token.Type != TokenType.Number || (int) (double) token.Value != 1)
                            {
                                Log.InvalidToken(LogCategory, token, lexer);
                                lexer.SkipLine();
                                continue;
                            }

                            // Army Division / / brigade
                            unit.Branch = Branch.Army;
                            continue;
                        }

                        // naval_unit_type
                        if (keyword.Equals("naval_unit_type"))
                        {
                            // = =
                            token = lexer.GetToken();
                            if (token.Type != TokenType.Equal)
                            {
                                Log.InvalidToken(LogCategory, token, lexer);
                                lexer.SkipLine();
                                continue;
                            }

                            // 1
                            token = lexer.GetToken();
                            if (token.Type != TokenType.Number || (int) (double) token.Value != 1)
                            {
                                Log.InvalidToken(LogCategory, token, lexer);
                                lexer.SkipLine();
                                continue;
                            }

                            // Navy Division / /brigade
                            unit.Branch = Branch.Navy;
                            continue;
                        }

                        // air_unit_type
                        if (keyword.Equals("air_unit_type"))
                        {
                            // = =
                            token = lexer.GetToken();
                            if (token.Type != TokenType.Equal)
                            {
                                Log.InvalidToken(LogCategory, token, lexer);
                                lexer.SkipLine();
                                continue;
                            }

                            // 1
                            token = lexer.GetToken();
                            if (token.Type != TokenType.Number || (int) (double) token.Value != 1)
                            {
                                Log.InvalidToken(LogCategory, token, lexer);
                                lexer.SkipLine();
                                continue;
                            }

                            // Luftwaffe Division / / brigade
                            unit.Branch = Branch.Airforce;
                            continue;
                        }

                        // max_speed_step
                        if (keyword.Equals("max_speed_step"))
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
                            if (token.Type != TokenType.Number)
                            {
                                Log.InvalidToken(LogCategory, token, lexer);
                                lexer.SkipLine();
                                continue;
                            }
                            int step = (int) (double) token.Value;
                            if (step < 0 || step > 2)
                            {
                                Log.InvalidToken(LogCategory, token, lexer);
                                lexer.SkipLine();
                                continue;
                            }

                            // Maximum production speed
                            unit.MaxSpeedStep = step;
                            continue;
                        }

                        // locked locked
                        if (keyword.Equals("locked"))
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
                            if (token.Type != TokenType.Number)
                            {
                                Log.InvalidToken(LogCategory, token, lexer);
                                lexer.SkipLine();
                                continue;
                            }

                            // Detachable
                            unit.Detachable = false;
                            continue;
                        }
                    }

                    else if (Game.Type == GameType.DarkestHour)
                    {
                        // detachable
                        if (keyword.Equals("detachable"))
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

                            string s = token.Value as string;
                            if (string.IsNullOrEmpty(s))
                            {
                                return false;
                            }
                            s = s.ToLower();

                            // yes
                            if (s.Equals("yes"))
                            {
                                unit.Detachable = true;
                                continue;
                            }

                            // no
                            if (s.Equals("no"))
                            {
                                unit.Detachable = false;
                                continue;
                            }

                            // Invalid token
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                }
            }

            return true;
        }

        /// <summary>
        ///     Parsing the unit model
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Unit model</returns>
        private static UnitModel ParseModel(TextLexer lexer)
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

            UnitModel model = new UnitModel();
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

                // cost
                if (keyword.Equals("cost"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // requirement I C
                    model.Cost = (double) token.Value;
                    continue;
                }

                // buildtime
                if (keyword.Equals("buildtime"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Time required for production
                    model.BuildTime = (double) token.Value;
                    continue;
                }

                // manpower
                if (keyword.Equals("manpower"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Necessary human resources
                    model.ManPower = (double) token.Value;
                    continue;
                }

                // maxspeed
                if (keyword.Equals("maxspeed"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Moving Speed
                    model.MaxSpeed = (double) token.Value;
                    continue;
                }

                // speed_cap_art
                if (keyword.Equals("speed_cap_art"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Speed cap with artillery brigade
                    model.SpeedCapArt = (double) token.Value;
                    continue;
                }

                // speed_cap_eng
                if (keyword.Equals("speed_cap_eng"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Speed cap when accompanied by an engineer brigade
                    model.SpeedCapEng = (double) token.Value;
                    continue;
                }

                // speed_cap_at
                if (keyword.Equals("speed_cap_at"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Speed cap when accompanied by anti-tank brigade
                    model.SpeedCapAt = (double) token.Value;
                    continue;
                }

                // speed_cap_aa
                if (keyword.Equals("speed_cap_aa"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Speed cap when accompanied by anti-aircraft brigade
                    model.SpeedCapAa = (double) token.Value;
                    continue;
                }

                // range
                if (keyword.Equals("range"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Cruising distance
                    model.Range = (double) token.Value;
                    continue;
                }

                // defaultorganization
                if (keyword.Equals("defaultorganisation"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Organization rate
                    model.DefaultOrganization = (double) token.Value;
                    continue;
                }

                // morale
                if (keyword.Equals("morale"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // morale
                    model.Morale = (double) token.Value;
                    continue;
                }

                // defensiveness
                if (keyword.Equals("defensiveness"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Defense power
                    model.Defensiveness = (double) token.Value;
                    continue;
                }

                // seadefence
                if (keyword.Equals("seadefence"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Anti-ship / / Anti-submarine defense
                    model.SeaDefense = (double) token.Value;
                    continue;
                }

                // airdefence
                if (keyword.Equals("airdefence"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Anti-aircraft defense
                    model.AirDefence = (double) token.Value;
                    continue;
                }

                // surface defense
                if (keyword.Equals("surfacedefence"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Ground / / Anti-ship defense
                    model.SurfaceDefence = (double) token.Value;
                    continue;
                }

                // toughness
                if (keyword.Equals("toughness"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Endurance
                    model.Toughness = (double) token.Value;
                    continue;
                }

                // softness
                if (keyword.Equals("softness"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Vulnerability
                    model.Softness = (double) token.Value;
                    continue;
                }

                // suppression
                if (keyword.Equals("suppression"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Control
                    model.Suppression = (double) token.Value;
                    continue;
                }

                // soft attack
                if (keyword.Equals("softattack"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Interpersonal attack power
                    model.SoftAttack = (double) token.Value;
                    continue;
                }

                // hard attack
                if (keyword.Equals("hardattack"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Anti-instep attack power
                    model.HardAttack = (double) token.Value;
                    continue;
                }

                // seaattack
                if (keyword.Equals("seaattack"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Anti-ship attack power (( Navy)
                    model.SeaAttack = (double) token.Value;
                    continue;
                }

                // subattack
                if (keyword.Equals("subattack"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Anti-submarine attack power
                    model.SubAttack = (double) token.Value;
                    continue;
                }

                // convoy attack
                if (keyword.Equals("convoyattack"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Trade destructive power
                    model.ConvoyAttack = (double) token.Value;
                    continue;
                }

                // shore bombardment
                if (keyword.Equals("shorebombardment"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Gulf attack power
                    model.ShoreBombardment = (double) token.Value;
                    continue;
                }

                // air attack
                if (keyword.Equals("airattack"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Anti-aircraft attack power
                    model.AirAttack = (double) token.Value;
                    continue;
                }

                // naval attack
                if (keyword.Equals("navalattack"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Anti-ship attack power (( Air Force )
                    model.NavalAttack = (double) token.Value;
                    continue;
                }

                // strategic attack
                if (keyword.Equals("strategicattack"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Strategic bombing power
                    model.StrategicAttack = (double) token.Value;
                    continue;
                }

                // distance
                if (keyword.Equals("distance"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Range distance
                    model.Distance = (double) token.Value;
                    continue;
                }

                // surfacedetection capability
                if (keyword.Equals("surfacedetectioncapability"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Anti-ship search ability
                    model.SurfaceDetectionCapability = (double) token.Value;
                    continue;
                }

                // subdetection capability
                if (keyword.Equals("subdetectioncapability"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Anti-submarine enemy ability
                    model.SubDetectionCapability = (double) token.Value;
                    continue;
                }

                // airdetection capability
                if (keyword.Equals("airdetectioncapability"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Anti-aircraft search ability
                    model.AirDetectionCapability = (double) token.Value;
                    continue;
                }

                // visibility
                if (keyword.Equals("visibility"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Visibility
                    model.Visibility = (double) token.Value;
                    continue;
                }

                // transportweight
                if (keyword.Equals("transportweight"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Required TC
                    model.TransportWeight = (double) token.Value;
                    continue;
                }

                // transport capability
                if (keyword.Equals("transportcapability"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Transport capacity
                    model.TransportCapability = (double) token.Value;
                    continue;
                }

                // supply consumption
                if (keyword.Equals("supplyconsumption"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Consumables
                    model.SupplyConsumption = (double) token.Value;
                    continue;
                }

                // fuel consumption
                if (keyword.Equals("fuelconsumption"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Fuel consumption
                    model.FuelConsumption = (double) token.Value;
                    continue;
                }

                // upgrade_time_factor
                if (keyword.Equals("upgrade_time_factor"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Improved time correction
                    model.UpgradeTimeFactor = (double) token.Value;
                    continue;
                }

                // upgrade_cost_factor
                if (keyword.Equals("upgrade_cost_factor"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Improvement I C correction
                    model.UpgradeCostFactor = (double) token.Value;
                    continue;
                }

                // AoD Unique
                if (Game.Type == GameType.ArsenalOfDemocracy)
                {
                    // artillery_bombardment
                    if (keyword.Equals("artillery_bombardment"))
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
                        if (token.Type != TokenType.Number)
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }

                        // Artillery attack power
                        model.ArtilleryBombardment = (double) token.Value;
                        continue;
                    }

                    // max_supply_stock
                    if (keyword.Equals("max_supply_stock"))
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
                        if (token.Type != TokenType.Number)
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }

                        // Maximum carry-on supplies
                        model.MaxSupplyStock = (double) token.Value;
                        continue;
                    }

                    // max_oil_stock
                    if (keyword.Equals("max_oil_stock"))
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
                        if (token.Type != TokenType.Number)
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }

                        // Maximum portable fuel
                        model.MaxOilStock = (double) token.Value;
                        continue;
                    }
                }

                // DH Unique
                else if (Game.Type == GameType.DarkestHour)
                {
                    // no_fuel_combat_mod
                    if (keyword.Equals("no_fuel_combat_mod"))
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
                        if (token.Type != TokenType.Number)
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }

                        // Combat correction when fuel runs out
                        model.NoFuelCombatMod = (double) token.Value;
                        continue;
                    }

                    // reinforce_time
                    if (keyword.Equals("reinforce_time"))
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
                        if (token.Type != TokenType.Number)
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }

                        // Replenishment time correction
                        model.ReinforceTimeFactor = (double) token.Value;
                        continue;
                    }

                    // reinforce_cost
                    if (keyword.Equals("reinforce_cost"))
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
                        if (token.Type != TokenType.Number)
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }

                        // Replenishment I C correction
                        model.ReinforceCostFactor = (double) token.Value;
                        continue;
                    }

                    // upgrade_time_boost
                    if (keyword.Equals("upgrade_time_boost"))
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

                        string s = token.Value as string;
                        if (string.IsNullOrEmpty(s))
                        {
                            continue;
                        }
                        s = s.ToLower();

                        if (s.Equals("yes"))
                        {
                            // Whether to correct the improvement time
                            model.UpgradeTimeBoost = true;
                            continue;
                        }

                        if (s.Equals("no"))
                        {
                            // Whether to correct the improvement time
                            model.UpgradeTimeBoost = false;
                            continue;
                        }

                        // Invalid token
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Automatic improvement destination unit class
                    if (Units.StringMap.ContainsKey(keyword))
                    {
                        // Unsupported unit type
                        UnitType type = Units.StringMap[keyword];
                        if (!Units.UnitTypes.Contains(type))
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }

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
                        if (token.Type != TokenType.Number)
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            lexer.SkipLine();
                            continue;
                        }

                        // Automatic improvement to other divisions
                        model.AutoUpgrade = true;
                        model.UpgradeClass = type;
                        model.UpgradeModel = (int) (double) token.Value;
                        continue;
                    }

                    // DH1.03 Unique thereafter
                    if (Game.Version >= 103)
                    {
                        // speed_cap
                        if (keyword.Equals("speed_cap"))
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
                            if (token.Type != TokenType.Number)
                            {
                                Log.InvalidToken(LogCategory, token, lexer);
                                lexer.SkipLine();
                                continue;
                            }

                            // Speed cap
                            model.SpeedCap = (double) token.Value;
                            continue;
                        }

                        // equipment
                        if (keyword.Equals("equipment"))
                        {
                            IEnumerable<UnitEquipment> equipments = ParseEquipment(lexer);
                            if (equipments == null)
                            {
                                Log.InvalidSection(LogCategory, "equipment", lexer);
                                continue;
                            }

                            // Equipment
                            model.Equipments.AddRange(equipments);
                            continue;
                        }
                    }
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return model;
        }

        /// <summary>
        ///     equipment Parse the section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Equipment data</returns>
        private static IEnumerable<UnitEquipment> ParseEquipment(TextLexer lexer)
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

            List<UnitEquipment> equipments = new List<UnitEquipment>();
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

                // resource
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
                keyword = keyword.ToLower();

                if (!Units.EquipmentStringMap.ContainsKey(keyword))
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }
                EquipmentType resource = Units.EquipmentStringMap[keyword];

                // = =
                token = lexer.GetToken();
                if (token.Type != TokenType.Equal)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    return null;
                }

                // value
                token = lexer.GetToken();
                if (token.Type != TokenType.Number)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                UnitEquipment equipment = new UnitEquipment { Resource = resource, Quantity = (double) token.Value };
                equipments.Add(equipment);
            }

            return equipments;
        }

        /// <summary>
        ///     upgrade Parse the section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Improvement information</returns>
        private static UnitUpgrade ParseUpgrade(TextLexer lexer)
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

            UnitUpgrade upgrade = new UnitUpgrade();
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
                    continue;
                }
                string keyword = token.Value as string;
                if (string.IsNullOrEmpty(keyword))
                {
                    return null;
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

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        return null;
                    }
                    s = s.ToLower();

                    // Other than the unit class name
                    if (!Units.StringMap.ContainsKey(s))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Unsupported unit type
                    UnitType type = Units.StringMap[s];
                    if (!Units.DivisionTypes.Contains(type))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Unit type
                    upgrade.Type = type;
                    continue;
                }

                // upgrade_cost_factor
                if (keyword.Equals("upgrade_cost_factor"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Improvement cost
                    upgrade.UpgradeCostFactor = (double) token.Value;
                    continue;
                }

                // upgrade_time_factor
                if (keyword.Equals("upgrade_time_factor"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Improvement time
                    upgrade.UpgradeTimeFactor = (double) token.Value;
                    continue;
                }


                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return upgrade;
        }

        /// <summary>
        ///     Parse the division unit file
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="units">Unit class list</param>
        /// <returns>Success or failure of parsing</returns>
        public static bool ParseDivisionTypes(string fileName, List<UnitClass> units)
        {
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
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    string keyword = token.Value as string;
                    if (string.IsNullOrEmpty(keyword))
                    {
                        return false;
                    }
                    keyword = keyword.ToLower();

                    // eyr
                    if (keyword.Equals("eyr"))
                    {
                        if (!ParseEyr(lexer))
                        {
                            Log.InvalidSection(LogCategory, "eyr", lexer);
                            Log.Warning("[Unit] Parse failed: eyr section");
                            // Once implemented continue To add
                            // continue; continue;
                        }
                        // Store analysis results after implementation
                        continue;
                    }

                    // Other than the unit class name
                    if (!Units.StringMap.ContainsKey(keyword))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Unsupported unit type
                    UnitType type = Units.StringMap[keyword];
                    if (!Units.DivisionTypes.Contains(type))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Unit type
                    if (!ParseUnitClass(lexer, units[(int) type]))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Parse the brigade unit file
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="units">Unit class list</param>
        /// <returns>Success or failure of parsing</returns>
        public static bool ParseBrigadeTypes(string fileName, List<UnitClass> units)
        {
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
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    string keyword = token.Value as string;
                    if (string.IsNullOrEmpty(keyword))
                    {
                        return false;
                    }
                    keyword = keyword.ToLower();

                    // Other than the unit class name
                    if (!Units.StringMap.ContainsKey(keyword))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Unsupported unit type
                    UnitType type = Units.StringMap[keyword];
                    if (!Units.BrigadeTypes.Contains(type))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Unit type
                    if (!ParseUnitClass(lexer, units[(int) type]))
                    {
                        Log.InvalidSection(LogCategory, keyword, lexer);
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     eyr Parse the section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Success or failure of parsing</returns>
        private static bool ParseEyr(TextLexer lexer)
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
                    continue;
                }
                string keyword = token.Value as string;
                if (string.IsNullOrEmpty(keyword))
                {
                    return false;
                }
                keyword = keyword.ToLower();

                // army / navy / air
                if (keyword.Equals("army") || keyword.Equals("navy") || keyword.Equals("air"))
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

                        // Once implemented continue To add
                        //continue; continue;
                    }

                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return true;
        }

        /// <summary>
        ///     Parse the unit class section
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <param name="unit">Unit data</param>
        /// <returns>Success or failure of parsing</returns>
        private static bool ParseUnitClass(TextLexer lexer, UnitClass unit)
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
                    continue;
                }
                string keyword = token.Value as string;
                if (string.IsNullOrEmpty(keyword))
                {
                    return false;
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

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        return false;
                    }
                    s = s.ToLower();

                    if (unit.Organization == UnitOrganization.Division)
                    {
                        // Real unit type name
                        if (Units.RealStringMap.ContainsKey(s))
                        {
                            // Actual unit type
                            unit.RealType = Units.RealStringMap[s];
                            // Set the military department corresponding to the actual unit type
                            unit.Branch = Units.RealBranchTable[(int) unit.RealType];
                            // Set the entity existence flag
                            unit.SetEntity();
                            continue;
                        }
                    }
                    else
                    {
                        // land
                        if (s.Equals("land"))
                        {
                            // Army Brigade
                            unit.Branch = Branch.Army;
                            // Set the entity existence flag
                            unit.SetEntity();
                            continue;
                        }
                        // naval
                        if (s.Equals("naval"))
                        {
                            // Navy brigade
                            unit.Branch = Branch.Navy;
                            // Set the entity existence flag
                            unit.SetEntity();
                            continue;
                        }
                        // air
                        if (s.Equals("air"))
                        {
                            // Air Force Brigade
                            unit.Branch = Branch.Airforce;
                            // Set the entity existence flag
                            unit.SetEntity();
                            continue;
                        }
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
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
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Unit class name
                    unit.Name = token.Value as string;
                    // Set the entity existence flag
                    unit.SetEntity();
                    continue;
                }

                // short_name
                if (keyword.Equals("short_name"))
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

                    // Unit class abbreviated name
                    unit.ShortName = token.Value as string;
                    // Set the entity existence flag
                    unit.SetEntity();
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

                    // Invalid token
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Unit class description
                    unit.Desc = token.Value as string;
                    // Set the entity existence flag
                    unit.SetEntity();
                    continue;
                }

                // short_desc
                if (keyword.Equals("short_desc"))
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

                    // Unit class abbreviated explanation
                    unit.ShortDesc = token.Value as string;
                    // Set the entity existence flag
                    unit.SetEntity();
                    continue;
                }

                // eyr
                if (keyword.Equals("eyr"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Statistics group
                    unit.Eyr = (int) (double) token.Value;
                    // Set the entity existence flag
                    unit.SetEntity();
                    continue;
                }

                // sprite
                if (keyword.Equals("sprite"))
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

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        return false;
                    }
                    s = s.ToLower();

                    // Other than sprite type name
                    if (!Units.SpriteStringMap.ContainsKey(s))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Sprite type
                    unit.Sprite = Units.SpriteStringMap[s];
                    // Set the entity existence flag
                    unit.SetEntity();
                    continue;
                }

                // transmute
                if (keyword.Equals("transmute"))
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

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        return false;
                    }
                    s = s.ToLower();

                    // Other than the unit class name
                    if (!Units.StringMap.ContainsKey(s))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Unsupported unit type
                    UnitType type = Units.StringMap[s];
                    if (!Units.DivisionTypes.Contains(type))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Alternative unit type
                    unit.Transmute = type;
                    // Set the entity existence flag
                    unit.SetEntity();
                    continue;
                }

                // gfx_prio
                if (keyword.Equals("gfx_prio"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Image priority
                    unit.GfxPrio = (int) (double) token.Value;
                    // Set the entity existence flag
                    unit.SetEntity();
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
                        lexer.SkipLine();
                        continue;
                    }

                    // Invalid token
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Military power
                    unit.Value = (double) token.Value;
                    // Set the entity existence flag
                    unit.SetEntity();
                    continue;
                }

                // list_prio
                if (keyword.Equals("list_prio"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // List priority
                    unit.ListPrio = (int) (double) token.Value;
                    if (unit.ListPrio != -1)
                    {
                        // Set the entity existence flag
                        unit.SetEntity();
                    }
                    continue;
                }

                // ui_prio
                if (keyword.Equals("ui_prio"))
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
                    if (token.Type != TokenType.Number)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // UI priority
                    unit.UiPrio = (int) (double) token.Value;
                    // Set the entity existence flag
                    unit.SetEntity();
                    continue;
                }

                // production
                if (keyword.Equals("production"))
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

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    s = s.ToLower();

                    if (s.Equals("yes"))
                    {
                        // Can be produced in the initial state
                        unit.Productable = true;
                        // Set the entity existence flag
                        unit.SetEntity();
                        continue;
                    }

                    if (s.Equals("no"))
                    {
                        // Unable to produce in the initial state
                        unit.Productable = false;
                        // Set the entity existence flag
                        unit.SetEntity();
                        continue;
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                // cag
                if (keyword.Equals("cag"))
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

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    s = s.ToLower();

                    if (s.Equals("yes"))
                    {
                        // Carrier air wing
                        unit.Cag = true;
                        // Set the entity existence flag
                        unit.SetEntity();
                        continue;
                    }

                    if (s.Equals("no"))
                    {
                        // Not a carrier air wing
                        unit.Cag = false;
                        // Set the entity existence flag
                        unit.SetEntity();
                        continue;
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                // escort
                if (keyword.Equals("escort"))
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

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    s = s.ToLower();

                    if (s.Equals("yes"))
                    {
                        // Escort fighter
                        unit.Escort = true;
                        // Set the entity existence flag
                        unit.SetEntity();
                        continue;
                    }

                    if (s.Equals("no"))
                    {
                        // Escort fighter
                        unit.Escort = false;
                        // Set the entity existence flag
                        unit.SetEntity();
                        continue;
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                // engineer
                if (keyword.Equals("engineer"))
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

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    s = s.ToLower();

                    if (s.Equals("yes"))
                    {
                        // Be an engineer
                        unit.Engineer = true;
                        // Set the entity existence flag
                        unit.SetEntity();
                        continue;
                    }

                    if (s.Equals("no"))
                    {
                        // Not an engineer
                        unit.Engineer = false;
                        // Set the entity existence flag
                        unit.SetEntity();
                        continue;
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                // RealUnitType
                if (Units.RealStringMap.ContainsKey(keyword))
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

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    s = s.ToLower();

                    if (s.Equals("yes"))
                    {
                        // The default for real unit types
                        unit.DefaultType = true;
                        // Set the entity existence flag
                        unit.SetEntity();
                        continue;
                    }

                    if (s.Equals("no"))
                    {
                        // Not the default for real unit types
                        unit.DefaultType = false;
                        // Set the entity existence flag
                        unit.SetEntity();
                        continue;
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return true;
        }

        #endregion
    }
}
