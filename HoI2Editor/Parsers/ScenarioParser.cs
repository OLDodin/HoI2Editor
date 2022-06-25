using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     Scenario syntax analysis class
    /// </summary>
    public static class ScenarioParser
    {
        #region Internal field

        /// <summary>
        ///     File name being analyzed
        /// </summary>
        private static string _fileName;

        /// <summary>
        ///     File name stack
        /// </summary>
        private static readonly Stack<string> FileNameStack = new Stack<string>();

        #endregion

        #region Internal constant

        /// <summary>
        ///     Category name at the time of log output
        /// </summary>
        private const string LogCategory = "Scenario";

        #endregion

        #region Parsing

        #region Scenario data

        /// <summary>
        ///     Parsing the scenario file
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="scenario">Scenario data</param>
        /// <returns>Success or failure of parsing</returns>
        public static bool Parse(string fileName, Scenario scenario)
        {
            _fileName = fileName;
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

                    // name
                    if (keyword.Equals("name"))
                    {
                        string s = ParseString(lexer);
                        if (s == null)
                        {
                            Log.InvalidClause(LogCategory, "name", lexer);
                            continue;
                        }

                        // Scenario name
                        scenario.Name = s;
                        continue;
                    }

                    // panel
                    if (keyword.Equals("panel"))
                    {
                        string s = ParseString(lexer);
                        if (s == null)
                        {
                            Log.InvalidClause(LogCategory, "panel", lexer);
                            continue;
                        }

                        // Panel image name
                        scenario.PanelName = s;
                        continue;
                    }

                    // header
                    if (keyword.Equals("header"))
                    {
                        ScenarioHeader header = ParseHeader(lexer);
                        if (header == null)
                        {
                            Log.InvalidSection(LogCategory, "header", lexer);
                            continue;
                        }

                        // Scenario header
                        scenario.Header = header;
                        continue;
                    }

                    // globaldata
                    if (keyword.Equals("globaldata"))
                    {
                        ScenarioGlobalData data = ParseGlobalData(lexer);
                        if (data == null)
                        {
                            Log.InvalidSection(LogCategory, "globaldata", lexer);
                            continue;
                        }

                        // Scenario global data
                        scenario.GlobalData = data;
                        continue;
                    }

                    // history
                    if (keyword.Equals("history"))
                    {
                        IEnumerable<int> list = ParseIdList(lexer);
                        if (list == null)
                        {
                            Log.InvalidSection(LogCategory, "history", lexer);
                            continue;
                        }

                        // Occurred event
                        scenario.HistoryEvents.AddRange(list);
                        continue;
                    }

                    // sleepevent
                    if (keyword.Equals("sleepevent"))
                    {
                        IEnumerable<int> list = ParseIdList(lexer);
                        if (list == null)
                        {
                            Log.InvalidSection(LogCategory, "sleepevent", lexer);
                            continue;
                        }

                        // Pause event
                        scenario.SleepEvents.AddRange(list);
                        continue;
                    }

                    // save_date
                    if (keyword.Equals("save_date"))
                    {
                        // If the scenario start date and time is not set 1936/1/1 Consider
                        GameDate startDate = scenario.GlobalData?.StartDate ?? new GameDate();

                        Dictionary<int, GameDate> dates = ParseSaveDate(lexer, startDate);
                        if (dates == null)
                        {
                            Log.InvalidSection(LogCategory, "save_date", lexer);
                            continue;
                        }

                        // Save date and time
                        scenario.SaveDates = dates;
                        continue;
                    }

                    // map
                    if (keyword.Equals("map"))
                    {
                        MapSettings map = ParseMap(lexer);
                        if (map == null)
                        {
                            Log.InvalidSection(LogCategory, "map", lexer);
                            continue;
                        }

                        // Map settings
                        scenario.Map = map;
                        continue;
                    }

                    // event event
                    if (keyword.Equals("event"))
                    {
                        string s = ParseString(lexer);
                        if (s == null)
                        {
                            Log.InvalidClause(LogCategory, "event", lexer);
                            continue;
                        }

                        // Event file
                        if (GetScenarioFileKind() == ScenarioFileKind.Top)
                        {
                            scenario.EventFiles.Add(s);
                        }
                        continue;
                    }

                    // include
                    if (keyword.Equals("include"))
                    {
                        string s = ParseString(lexer);
                        if (s == null)
                        {
                            Log.InvalidClause(LogCategory, "include", lexer);
                            continue;
                        }

                        // Include file
                        if (GetScenarioFileKind() == ScenarioFileKind.Top)
                        {
                            scenario.IncludeFiles.Add(s);

                            // Set include folders
                            string folderName = Path.GetDirectoryName(s);
                            scenario.IncludeFolder = Path.GetFileName(folderName);
                        }

                        string pathName = Game.GetReadFileName(s);
                        if (!File.Exists(pathName))
                        {
                            Log.Warning("[Scenario] Not exist include file: {0}", s);
                            continue;
                        }

                        // Interpret the include file
                        FileNameStack.Push(_fileName);
                        Log.Verbose("[Scenario] Include: {0}", s);
                        Parse(pathName, scenario);
                        _fileName = FileNameStack.Pop();
                        continue;
                    }

                    // province province
                    if (keyword.Equals("province"))
                    {
                        ProvinceSettings province = ParseProvince(lexer);
                        if (province == null)
                        {
                            Log.InvalidSection(LogCategory, "province", lexer);
                            continue;
                        }

                        // Province settings
                        Scenarios.AddProvinceSettings(province);
                        switch (GetScenarioFileKind())
                        {
                            case ScenarioFileKind.BasesInc: // bases.inc
                                scenario.IsBaseProvinceSettings = true;
                                break;

                            case ScenarioFileKind.BasesDodInc: // bases_DOD.inc
                                scenario.IsBaseDodProvinceSettings = true;
                                break;

                            case ScenarioFileKind.DepotsInc: // depots.inc
                                scenario.IsDepotsProvinceSettings = true;
                                break;

                            case ScenarioFileKind.VpInc: // vp.inc
                                scenario.IsVpProvinceSettings = true;
                                break;

                            case ScenarioFileKind.Top: // scenario .eug
                                break;

                            default:
                                scenario.IsCountryProvinceSettings = true;
                                break;
                        }
                        continue;
                    }

                    // country country
                    if (keyword.Equals("country"))
                    {
                        CountrySettings country = ParseCountry(lexer, scenario);
                        if (country == null)
                        {
                            Log.InvalidSection(LogCategory, "country", lexer);
                            continue;
                        }

                        if (!scenario.Countries.Contains(country))
                        {
                            // Associate file name
                            country.FileName = Path.GetFileName(_fileName);

                            // National setting
                            scenario.Countries.Add(country);
                        }
                        continue;
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                }
            }

            return true;
        }

        /// <summary>
        ///     Parse the save date and time
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <param name="startDate">Start date and time</param>
        /// <returns>Save date and time</returns>
        private static Dictionary<int, GameDate> ParseSaveDate(TextLexer lexer, GameDate startDate)
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

            Dictionary<int, GameDate> dates = new Dictionary<int, GameDate>();
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
                    lexer.SkipLine();
                    continue;
                }

                int id = (int) (double) token.Value;

                int? n = ParseInt(lexer);
                if (!n.HasValue)
                {
                    Log.InvalidClause(LogCategory, IntHelper.ToString(id), lexer);
                    continue;
                }

                // Save date and time
                GameDate date = startDate.Minus((int) n);
                dates[id] = date;
            }

            return dates;
        }

        #endregion

        #region Scenario header

        /// <summary>
        ///     Parse the scenario header
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Scenario header</returns>
        private static ScenarioHeader ParseHeader(TextLexer lexer)
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

            ScenarioHeader header = new ScenarioHeader();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    Log.MissingCloseBrace(LogCategory, "header", lexer);
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

                // name
                if (keyword.Equals("name"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "name", lexer);
                        continue;
                    }

                    // Scenario header name
                    header.Name = s;
                    continue;
                }

                // startdate
                if (keyword.Equals("startdate"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "startdate", lexer);
                        continue;
                    }

                    // Start date and time
                    header.StartDate = date;
                    continue;
                }

                // startyear
                if (keyword.Equals("startyear"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "startyear", lexer);
                        continue;
                    }

                    // Start year
                    header.StartYear = (int) n;
                    continue;
                }

                // endyear
                if (keyword.Equals("endyear"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "endyear", lexer);
                        continue;
                    }

                    // End year
                    header.EndYear = (int) n;
                    continue;
                }

                // free free
                if (keyword.Equals("free"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "free", lexer);
                        continue;
                    }

                    // Free choice of the nation
                    header.IsFreeSelection = (bool) b;
                    continue;
                }

                // combat
                if (keyword.Equals("combat"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "combat", lexer);
                        continue;
                    }

                    // Short scenario
                    header.IsBattleScenario = (bool) b;
                    continue;
                }

                // selectable
                if (keyword.Equals("selectable"))
                {
                    IEnumerable<Country> list = ParseCountryList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "selectable", lexer);
                        continue;
                    }

                    // Selectable countries
                    header.SelectableCountries.AddRange(list);
                    continue;
                }

                // set_ai_aggresive
                if (keyword.Equals("set_ai_aggresive"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "set_ai_aggresive", lexer);
                        continue;
                    }

                    // AI Aggression
                    header.AiAggressive = (int) n;
                    continue;
                }

                // set_difficulty
                if (keyword.Equals("set_difficulty"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "set_difficulty", lexer);
                        continue;
                    }

                    // difficulty
                    header.Difficulty = (int) n;
                    continue;
                }

                // set_gamespeed
                if (keyword.Equals("set_gamespeed"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "set_gamespeed", lexer);
                        continue;
                    }

                    // Game speed
                    header.GameSpeed = (int) n;
                    continue;
                }

                // Country tag
                string tagName = keyword.ToUpper();
                if (Countries.StringMap.ContainsKey(tagName))
                {
                    Country tag = Countries.StringMap[tagName];
                    if (Countries.Tags.Contains(tag))
                    {
                        MajorCountrySettings major = ParseMajorCountry(lexer);
                        if (major == null)
                        {
                            Log.InvalidSection(LogCategory, tagName, lexer);
                            continue;
                        }

                        // Major country setting
                        major.Country = tag;
                        MajorCountrySettings prev = header.MajorCountries.FirstOrDefault(m => m.Country == tag);
                        if (prev != null)
                        {
                            header.MajorCountries.Remove(prev);
                        }
                        header.MajorCountries.Add(major);
                        continue;
                    }
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return header;
        }

        /// <summary>
        ///     Parsing major country information
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Major country information</returns>
        private static MajorCountrySettings ParseMajorCountry(TextLexer lexer)
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

            MajorCountrySettings major = new MajorCountrySettings();
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

                // desc
                if (keyword.Equals("desc"))
                {
                    string s = ParseStringOrIdentifier(lexer);
                    if (string.IsNullOrEmpty(s))
                    {
                        Log.InvalidClause(LogCategory, "desc", lexer);
                        continue;
                    }

                    // Explanatory text
                    major.Desc = s;
                    continue;
                }

                // countrytactics
                if (keyword.Equals("countrytactics"))
                {
                    string s = ParseStringOrIdentifier(lexer);
                    if (string.IsNullOrEmpty(s))
                    {
                        Log.InvalidClause(LogCategory, "countrytactics", lexer);
                        continue;
                    }

                    // National strategy
                    major.CountryTactics = s;
                    continue;
                }

                // picture
                if (keyword.Equals("picture"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "picture", lexer);
                        continue;
                    }

                    // Propaganda image name
                    major.PictureName = s;
                    continue;
                }

                // songs
                if (keyword.Equals("songs"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "songs", lexer);
                        continue;
                    }

                    // Music file name
                    major.Songs = s;
                    continue;
                }

                // bottom
                if (keyword.Equals("bottom"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "bottom", lexer);
                        continue;
                    }

                    // Placed at the right end
                    major.Bottom = (bool) b;
                    continue;
                }

                if ((Game.Type == GameType.DarkestHour) && (Game.Version >= 104))
                {
                    // name
                    if (keyword.Equals("name"))
                    {
                        string s = ParseStringOrIdentifier(lexer);
                        if (string.IsNullOrEmpty(s))
                        {
                            Log.InvalidClause(LogCategory, "name", lexer);
                            continue;
                        }

                        // Country name
                        major.Name = s;
                        continue;
                    }

                    // flag_ext
                    if (keyword.Equals("flag_ext"))
                    {
                        string s = ParseStringOrIdentifier(lexer);
                        if (string.IsNullOrEmpty(s))
                        {
                            Log.InvalidClause(LogCategory, "flag_ext", lexer);
                            continue;
                        }

                        // Flag suffix
                        major.FlagExt = s;
                        continue;
                    }
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return major;
        }

        #endregion

        #region Scenario global data

        /// <summary>
        ///     Scenario Parse global data
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Scenario global data</returns>
        private static ScenarioGlobalData ParseGlobalData(TextLexer lexer)
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

            ScenarioGlobalData data = new ScenarioGlobalData();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    Log.MissingCloseBrace(LogCategory, "globaldata", lexer);
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

                // rules rules
                if (keyword.Equals("rules"))
                {
                    ScenarioRules rules = ParseRules(lexer);
                    if (rules == null)
                    {
                        Log.InvalidSection(LogCategory, "rules", lexer);
                        continue;
                    }

                    // Rule setting
                    data.Rules = rules;
                    continue;
                }

                // startdate
                if (keyword.Equals("startdate"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "startdate", lexer);
                        continue;
                    }

                    // Start date and time
                    data.StartDate = date;
                    continue;
                }

                // enddate
                if (keyword.Equals("enddate"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "enddate", lexer);
                        continue;
                    }

                    // End date and time
                    data.EndDate = date;
                    continue;
                }

                // axis axis
                if (keyword.Equals("axis"))
                {
                    Alliance alliance = ParseAlliance(lexer);
                    if (alliance == null)
                    {
                        Log.InvalidSection(LogCategory, "axis", lexer);
                        continue;
                    }

                    // Axis country
                    data.Axis = alliance;
                    continue;
                }

                // allies
                if (keyword.Equals("allies"))
                {
                    Alliance alliance = ParseAlliance(lexer);
                    if (alliance == null)
                    {
                        Log.InvalidSection(LogCategory, "allies", lexer);
                        continue;
                    }

                    // Allied
                    data.Allies = alliance;
                    continue;
                }

                // comintern
                if (keyword.Equals("comintern"))
                {
                    Alliance alliance = ParseAlliance(lexer);
                    if (alliance == null)
                    {
                        Log.InvalidSection(LogCategory, "comintern", lexer);
                        continue;
                    }

                    // Communist country
                    data.Comintern = alliance;
                    continue;
                }

                // alliance
                if (keyword.Equals("alliance"))
                {
                    Alliance alliance = ParseAlliance(lexer);
                    if (alliance == null)
                    {
                        Log.InvalidSection(LogCategory, "alliance", lexer);
                        continue;
                    }

                    // Allies
                    data.Alliances.Add(alliance);
                    continue;
                }

                // war
                if (keyword.Equals("war"))
                {
                    War war = ParseWar(lexer);
                    if (war == null)
                    {
                        Log.InvalidSection(LogCategory, "war", lexer);
                        continue;
                    }

                    // war
                    data.Wars.Add(war);
                    continue;
                }

                // treaty
                if (keyword.Equals("treaty"))
                {
                    Treaty treaty = ParseTreaty(lexer);
                    if (treaty == null)
                    {
                        Log.InvalidSection(LogCategory, "treaty", lexer);
                        continue;
                    }

                    // Diplomatic agreement
                    switch (treaty.Type)
                    {
                        case TreatyType.NonAggression:
                            data.NonAggressions.Add(treaty);
                            break;

                        case TreatyType.Peace:
                            data.Peaces.Add(treaty);
                            break;

                        case TreatyType.Trade:
                            data.Trades.Add(treaty);
                            break;
                    }
                    continue;
                }

                // flags flags
                if (keyword.Equals("flags"))
                {
                    Dictionary<string, string> flags = ParseFlags(lexer);
                    if (flags == null)
                    {
                        Log.InvalidSection(LogCategory, "flags", lexer);
                        continue;
                    }

                    // Global flag list
                    data.Flags = flags;
                    continue;
                }

                // queued_events
                if (keyword.Equals("queued_events"))
                {
                    List<QueuedEvent> events = ParseQueuedEvents(lexer);
                    if (events == null)
                    {
                        Log.InvalidSection(LogCategory, "queued_events", lexer);
                        continue;
                    }

                    // Waiting event list
                    data.QueuedEvents.AddRange(events);
                    continue;
                }

                // dormant_leaders
                if (keyword.Equals("dormant_leaders"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    token = lexer.GetToken();
                    if (token.Type == TokenType.Identifier)
                    {
                        string s = token.Value as string;
                        if (string.IsNullOrEmpty(s))
                        {
                            continue;
                        }
                        s = s.ToLower();

                        // yes
                        if (s.Equals("yes"))
                        {
                            data.DormantLeadersAll = true;
                            continue;
                        }

                        // no
                        if (s.Equals("no"))
                        {
                            data.DormantLeadersAll = false;
                            continue;
                        }

                        // Invalid token
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // {
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
                            lexer.SkipLine();
                            continue;
                        }

                        // ID
                        list.Add((int) (double) token.Value);
                    }

                    // Pause commander
                    data.DormantLeaders.AddRange(list);
                    continue;
                }

                // dormant_ministers
                if (keyword.Equals("dormant_ministers"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "dormant_ministers", lexer);
                        continue;
                    }

                    // Paused ministers
                    data.DormantMinisters.AddRange(list);
                    continue;
                }

                // dormant_teams
                if (keyword.Equals("dormant_teams"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "dormant_teams", lexer);
                        continue;
                    }

                    // Rest research institution
                    data.DormantTeams.AddRange(list);
                    continue;
                }

                // weather
                if (keyword.Equals("weather"))
                {
                    Weather weather = ParseWeather(lexer);
                    if (weather == null)
                    {
                        Log.InvalidSection(LogCategory, "weather", lexer);
                        continue;
                    }

                    // Weather settings
                    data.Weather = weather;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return data;
        }

        /// <summary>
        ///     Parsing the waiting event list
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Waiting event list</returns>
        private static List<QueuedEvent> ParseQueuedEvents(TextLexer lexer)
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

            List<QueuedEvent> list = new List<QueuedEvent>();
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

                // event event
                if (keyword.Equals("event"))
                {
                    QueuedEvent qe = ParseQueuedEvent(lexer);
                    if (qe == null)
                    {
                        Log.InvalidSection(LogCategory, "event", lexer);
                        continue;
                    }

                    // Waiting event
                    list.Add(qe);
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return list;
        }

        /// <summary>
        ///     Parse the pending event
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Waiting event</returns>
        private static QueuedEvent ParseQueuedEvent(TextLexer lexer)
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

            QueuedEvent qe = new QueuedEvent();
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

                // tag
                if (keyword.Equals("tag"))
                {
                    Country? tag = ParseTag(lexer);
                    if (!tag.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "tag", lexer);
                        continue;
                    }

                    // Country of occurrence of the event
                    qe.Country = (Country) tag;
                    continue;
                }

                // id id
                if (keyword.Equals("id"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "id", lexer);
                        continue;
                    }

                    // event ID
                    qe.Id = (int) n;
                    continue;
                }

                // hour
                if (keyword.Equals("hour"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "hour", lexer);
                        continue;
                    }

                    // Event occurrence wait time
                    qe.Hour = (int) n;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return qe;
        }

        /// <summary>
        ///     Synthesize rule settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Rule setting</returns>
        private static ScenarioRules ParseRules(TextLexer lexer)
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

            ScenarioRules rules = new ScenarioRules();
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

                // diplomacy
                if (keyword.Equals("diplomacy"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "diplomacy", lexer);
                        continue;
                    }

                    // Diplomatic
                    rules.AllowDiplomacy = (bool) b;
                    continue;
                }

                // production
                if (keyword.Equals("production"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "production", lexer);
                        continue;
                    }

                    // production
                    rules.AllowProduction = (bool) b;
                    continue;
                }

                // technology
                if (keyword.Equals("technology"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "technology", lexer);
                        continue;
                    }

                    // Technology
                    rules.AllowTechnology = (bool) b;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return rules;
        }

        #endregion

        #region weather

        /// <summary>
        ///     Parsing weather settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Weather settings</returns>
        private static Weather ParseWeather(TextLexer lexer)
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

            Weather weather = new Weather();
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

                // static static
                if (keyword.Equals("static"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "static", lexer);
                        continue;
                    }

                    // Fixed setting
                    weather.Static = (bool) b;
                    continue;
                }

                // pattern pattern
                if (keyword.Equals("pattern"))
                {
                    WeatherPattern pattern = ParseWeatherPattern(lexer);
                    if (pattern == null)
                    {
                        Log.InvalidSection(LogCategory, "pattern", lexer);
                        continue;
                    }

                    // Weather pattern
                    weather.Patterns.Add(pattern);
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return weather;
        }

        /// <summary>
        ///     Parsing weather patterns
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Weather pattern</returns>
        private static WeatherPattern ParseWeatherPattern(TextLexer lexer)
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

            WeatherPattern pattern = new WeatherPattern();
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

                // id id
                if (keyword.Equals("id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "id", lexer);
                        continue;
                    }

                    // type When id id Pair of
                    pattern.Id = id;
                    continue;
                }

                // provinces
                if (keyword.Equals("provinces"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "provinces", lexer);
                        continue;
                    }

                    // Provincial list
                    pattern.Provinces.AddRange(list);
                    continue;
                }

                // center center
                if (keyword.Equals("centre"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "centre", lexer);
                        continue;
                    }

                    // Central Providence
                    pattern.Centre = (int) n;
                    continue;
                }

                // speed
                if (keyword.Equals("speed"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "speed", lexer);
                        continue;
                    }

                    // speed
                    pattern.Speed = (int) n;
                    continue;
                }

                // heading
                if (keyword.Equals("heading"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "heading", lexer);
                        continue;
                    }

                    // direction
                    pattern.Heading = s;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return pattern;
        }

        #endregion

        #region map

        /// <summary>
        ///     Parse map settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Map settings</returns>
        private static MapSettings ParseMap(TextLexer lexer)
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

            MapSettings map = new MapSettings();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    Log.MissingCloseBrace(LogCategory, "map", lexer);
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

                // yes
                if (keyword.Equals("yes"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Providence ID
                    token = lexer.GetToken();
                    if (token.Type == TokenType.Number)
                    {
                        map.Yes.Add((int) (double) token.Value);
                        continue;
                    }

                    if (token.Type == TokenType.Identifier)
                    {
                        string s = token.Value as string;
                        if (string.IsNullOrEmpty(s))
                        {
                            continue;
                        }
                        s = s.ToLower();

                        // all
                        if (s.Equals("all"))
                        {
                            map.All = true;
                            continue;
                        }
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                // no
                if (keyword.Equals("no"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    // Providence ID
                    token = lexer.GetToken();
                    if (token.Type == TokenType.Number)
                    {
                        map.No.Add((int) (double) token.Value);
                        continue;
                    }

                    if (token.Type == TokenType.Identifier)
                    {
                        string s = token.Value as string;
                        if (string.IsNullOrEmpty(s))
                        {
                            continue;
                        }
                        s = s.ToLower();

                        // all
                        if (s.Equals("all"))
                        {
                            map.All = false;
                            continue;
                        }
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                // top top
                if (keyword.Equals("top"))
                {
                    MapPoint point = ParsePoint(lexer);
                    if (point == null)
                    {
                        Log.InvalidSection(LogCategory, "top", lexer);
                        continue;
                    }

                    // Map range (( upper left )
                    map.Top = point;
                    continue;
                }

                // bottom
                if (keyword.Equals("bottom"))
                {
                    MapPoint point = ParsePoint(lexer);
                    if (point == null)
                    {
                        Log.InvalidSection(LogCategory, "bottom", lexer);
                        continue;
                    }

                    // Map range (( Bottom right )
                    map.Bottom = point;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return map;
        }

        /// <summary>
        ///     Parse the coordinates of the map
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Map coordinates</returns>
        private static MapPoint ParsePoint(TextLexer lexer)
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

            MapPoint point = new MapPoint();
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

                // x x
                if (keyword.Equals("x"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "x", lexer);
                        continue;
                    }

                    // X Coordinate
                    point.X = (int) n;
                    continue;
                }

                // y y
                if (keyword.Equals("y"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "y", lexer);
                        continue;
                    }

                    // Y Coordinate
                    point.Y = (int) n;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return point;
        }

        #endregion

        #region Providence

        /// <summary>
        ///     Parse the Providence settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Providence settings</returns>
        private static ProvinceSettings ParseProvince(TextLexer lexer)
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

            ProvinceSettings province = new ProvinceSettings();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    Log.MissingCloseBrace(LogCategory, "province", lexer);
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

                // id id
                if (keyword.Equals("id"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "id", lexer);
                        continue;
                    }

                    // Providence ID
                    province.Id = (int) n;
                    continue;
                }

                // I C
                if (keyword.Equals("ic"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "ic", lexer);
                        continue;
                    }

                    // I C
                    province.Ic = size;
                    continue;
                }

                // infra
                if (keyword.Equals("infra"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "infra", lexer);
                        continue;
                    }

                    // infrastructure
                    province.Infrastructure = size;
                    continue;
                }

                // landfort
                if (keyword.Equals("landfort"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "landfort", lexer);
                        continue;
                    }

                    // Land fortress
                    province.LandFort = size;
                    continue;
                }

                // coastalfort
                if (keyword.Equals("coastalfort"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "coastalfort", lexer);
                        continue;
                    }

                    // Coastal fortress
                    province.CoastalFort = size;
                    continue;
                }

                // anti_air
                if (keyword.Equals("anti_air"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "anti_air", lexer);
                        continue;
                    }

                    // Anti-aircraft gun
                    province.AntiAir = size;
                    continue;
                }

                // air_base
                if (keyword.Equals("air_base"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "air_base", lexer);
                        continue;
                    }

                    // Air Force Base
                    province.AirBase = size;
                    continue;
                }

                // naval_base
                if (keyword.Equals("naval_base"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "naval_base", lexer);
                        continue;
                    }

                    // Navy base
                    province.NavalBase = size;
                    continue;
                }

                // radar_station
                if (keyword.Equals("radar_station"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "radar_station", lexer);
                        continue;
                    }

                    // Radar base
                    province.RadarStation = size;
                    continue;
                }

                // nuclear_reactor
                if (keyword.Equals("nuclear_reactor"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "nuclear_reactor", lexer);
                        continue;
                    }

                    // Reactor
                    province.NuclearReactor = size;
                    continue;
                }

                // rocket_test
                if (keyword.Equals("rocket_test"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "rocket_test", lexer);
                        continue;
                    }

                    // Rocket test site
                    province.RocketTest = size;
                    continue;
                }

                // synthetic_oil
                if (keyword.Equals("synthetic_oil"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "synthetic_oil", lexer);
                        continue;
                    }

                    // Synthetic oil factory
                    province.SyntheticOil = size;
                    continue;
                }

                // synthetic_rares
                if (keyword.Equals("synthetic_rares"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "synthetic_rares", lexer);
                        continue;
                    }

                    // Synthetic material factory
                    province.SyntheticRares = size;
                    continue;
                }

                // nuclear_power
                if (keyword.Equals("nuclear_power"))
                {
                    BuildingSize size = ParseSize(lexer);
                    if (size == null)
                    {
                        Log.InvalidSection(LogCategory, "nuclear_power", lexer);
                        continue;
                    }

                    // Nuclear power plant
                    province.NuclearPower = size;
                    continue;
                }

                // supplypool
                if (keyword.Equals("supplypool"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "supplypool", lexer);
                        continue;
                    }

                    // Stock of supplies
                    province.SupplyPool = (double) d;
                    continue;
                }

                // oilpool
                if (keyword.Equals("oilpool"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "oilpool", lexer);
                        continue;
                    }

                    // Oil reserves
                    province.OilPool = (double) d;
                    continue;
                }

                // energypool
                if (keyword.Equals("energypool"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "energypool", lexer);
                        continue;
                    }

                    // Energy stockpile
                    province.EnergyPool = (double) d;
                    continue;
                }

                // metalpool
                if (keyword.Equals("metalpool"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "metalpool", lexer);
                        continue;
                    }

                    // Metal reserves
                    province.MetalPool = (double) d;
                    continue;
                }

                // rarematerialspool
                if (keyword.Equals("rarematerialspool"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "rarematerialspool", lexer);
                        continue;
                    }

                    // Stockpile of rare resources
                    province.RareMaterialsPool = (double) d;
                    continue;
                }

                // energy
                if (keyword.Equals("energy"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "energy", lexer);
                        continue;
                    }

                    // Energy output
                    province.Energy = (double) d;
                    continue;
                }

                // max_energy
                if (keyword.Equals("max_energy"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "max_energy", lexer);
                        continue;
                    }

                    // Maximum energy output
                    province.MaxEnergy = (double) d;
                    continue;
                }

                // metal
                if (keyword.Equals("metal"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "metal", lexer);
                        continue;
                    }

                    // Metal output
                    province.Metal = (double) d;
                    continue;
                }

                // max_metal
                if (keyword.Equals("max_metal"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "max_metal", lexer);
                        continue;
                    }

                    // Maximum metal output
                    province.MaxMetal = (double) d;
                    continue;
                }

                // rare_materials
                if (keyword.Equals("rare_materials"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "rare_materials", lexer);
                        continue;
                    }

                    // Rare resource output
                    province.RareMaterials = (double) d;
                    continue;
                }

                // max_rare_materials
                if (keyword.Equals("max_rare_materials"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "max_rare_materials", lexer);
                        continue;
                    }

                    // Maximum scarce resource output
                    province.MaxRareMaterials = (double) d;
                    continue;
                }

                // oil
                if (keyword.Equals("oil"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "oil", lexer);
                        continue;
                    }

                    // Oil output
                    province.Oil = (double) d;
                    continue;
                }

                // max_oil
                if (keyword.Equals("max_oil"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "max_oil", lexer);
                        continue;
                    }

                    // Maximum oil output
                    province.MaxOil = (double) d;
                    continue;
                }

                // manpower
                if (keyword.Equals("manpower"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "manpower", lexer);
                        continue;
                    }

                    // Human resources
                    province.Manpower = (double) d;
                    continue;
                }

                // max_manpower
                if (keyword.Equals("max_manpower"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "max_manpower", lexer);
                        continue;
                    }

                    // Maximum human resources
                    province.MaxManpower = (double) d;
                    continue;
                }

                // points points
                if (keyword.Equals("points"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "points", lexer);
                        continue;
                    }

                    // Victory points
                    province.Vp = (int) n;
                    continue;
                }

                // province_revoltrisk
                if (keyword.Equals("province_revoltrisk"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "province_revoltrisk", lexer);
                        continue;
                    }

                    // Rebellion rate
                    province.RevoltRisk = (double) d;
                    continue;
                }

                // weather
                if (keyword.Equals("weather"))
                {
                    string s = ParseIdentifier(lexer);
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    s = s.ToLower();

                    if (!Scenarios.WeatherStrings.Contains(s))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // weather
                    province.Weather = (WeatherType) Array.IndexOf(Scenarios.WeatherStrings, s);
                    continue;
                }

                if (Game.Type == GameType.DarkestHour)
                {
                    // name
                    if (keyword.Equals("name"))
                    {
                        string s = ParseStringOrIdentifier(lexer);
                        if (string.IsNullOrEmpty(s))
                        {
                            Log.InvalidClause(LogCategory, "name", lexer);
                            continue;
                        }

                        // Province name
                        province.Name = s;
                        continue;
                    }
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return province;
        }

        #endregion

        #region building

        /// <summary>
        ///     Parse the size of the building
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Building size</returns>
        private static BuildingSize ParseSize(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            // Relative size specification
            token = lexer.GetToken();
            if (token.Type == TokenType.Number)
            {
                return new BuildingSize { Size = (double) token.Value };
            }

            // {
            if (token.Type != TokenType.OpenBrace)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            BuildingSize size = new BuildingSize();
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

                // size
                if (keyword.Equals("size"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "size", lexer);
                        continue;
                    }

                    if (d < 0)
                    {
                        Log.OutOfRange(LogCategory, "size", d, lexer);
                        continue;
                    }

                    // Maximum size
                    size.MaxSize = (double) d;
                    continue;
                }

                // current_size
                if (keyword.Equals("current_size"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "current_size", lexer);
                        continue;
                    }

                    if (d < 0)
                    {
                        Log.OutOfRange(LogCategory, "current_size", d, lexer);
                        continue;
                    }

                    // Current size
                    size.CurrentSize = (double) d;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return size;
        }

        /// <summary>
        ///     Syntactically analyze a building in production
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Building in production</returns>
        private static BuildingDevelopment ParseBuildingDevelopment(TextLexer lexer)
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

            BuildingDevelopment building = new BuildingDevelopment();
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

                // id id
                if (keyword.Equals("id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "id", lexer);
                        continue;
                    }

                    // type When id id Pair of
                    building.Id = id;
                    continue;
                }

                // name
                if (keyword.Equals("name"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "name", lexer);
                        continue;
                    }

                    // name
                    building.Name = s;
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

                    // Invalid token
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Identifier)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        return null;
                    }
                    s = s.ToLower();

                    if (!Scenarios.BuildingStrings.Contains(s))
                    {
                        // Invalid token
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Building type
                    building.Type = (BuildingType) Array.IndexOf(Scenarios.BuildingStrings, s);
                    continue;
                }

                // location
                if (keyword.Equals("location"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "location", lexer);
                        continue;
                    }

                    // position
                    building.Location = (int) n;
                    continue;
                }

                // cost
                if (keyword.Equals("cost"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "cost", lexer);
                        continue;
                    }

                    // requirement I C
                    building.Cost = (double) d;
                    continue;
                }

                // manpower
                if (keyword.Equals("manpower"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "manpower", lexer);
                        continue;
                    }

                    // Necessary human resources
                    building.Manpower = (double) d;
                    continue;
                }

                // date
                if (keyword.Equals("date"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "date", lexer);
                        continue;
                    }

                    // Completion date
                    building.Date = date;
                    continue;
                }

                // progress
                if (keyword.Equals("progress"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "progress", lexer);
                        continue;
                    }

                    // Progress rate increment
                    building.Progress = (double) d;
                    continue;
                }

                // total_progress
                if (keyword.Equals("total_progress"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "total_progress", lexer);
                        continue;
                    }

                    // Total progress rate
                    building.TotalProgress = (double) d;
                    continue;
                }

                // gearing_bonus
                if (keyword.Equals("gearing_bonus"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "gearing_bonus", lexer);
                        continue;
                    }

                    // Continuous production bonus
                    building.GearingBonus = (double) d;
                    continue;
                }

                // size
                if (keyword.Equals("size"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "size", lexer);
                        continue;
                    }

                    // Total production number
                    building.Size = (int) n;
                    continue;
                }

                // done done
                if (keyword.Equals("done"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "done", lexer);
                        continue;
                    }

                    // Number of completed production
                    building.Done = (int) n;
                    continue;
                }

                // days
                if (keyword.Equals("days"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "days", lexer);
                        continue;
                    }

                    // Days to complete
                    building.Days = (int) n;
                    continue;
                }

                // days_for_first
                if (keyword.Equals("days_for_first"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "days_for_first", lexer);
                        continue;
                    }

                    // 1 Number of days to complete the unit
                    building.DaysForFirst = (int) n;
                    continue;
                }

                // halted
                if (keyword.Equals("halted"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "halted", lexer);
                        continue;
                    }

                    // Stopping
                    building.Halted = (bool) b;
                    continue;
                }

                // close_when_finished
                if (keyword.Equals("close_when_finished"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "close_when_finished", lexer);
                        continue;
                    }

                    // Whether to delete the queue on completion
                    building.CloseWhenFinished = (bool) b;
                    continue;
                }

                // waiting for closure
                if (keyword.Equals("waitingforclosure"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "waitingforclosure", lexer);
                        continue;
                    }

                    // details unknown
                    building.WaitingForClosure = (bool) b;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return building;
        }

        #endregion

        #region Diplomatic

        /// <summary>
        ///     Parsing ally settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Alliance setting</returns>
        private static Alliance ParseAlliance(TextLexer lexer)
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

            Alliance alliance = new Alliance();
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

                // id id
                if (keyword.Equals("id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "id", lexer);
                        continue;
                    }

                    // type When id id Pair of
                    alliance.Id = id;
                    continue;
                }

                // participant
                if (keyword.Equals("participant"))
                {
                    IEnumerable<Country> list = ParseCountryList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "participant", lexer);
                        continue;
                    }

                    // Participating countries
                    alliance.Participant.AddRange(list);
                    continue;
                }

                // defensive
                if (keyword.Equals("defensive"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "defensive", lexer);
                        continue;
                    }

                    // Whether it is a defense alliance
                    alliance.Defensive = (bool) b;
                    continue;
                }

                if (Game.Type == GameType.DarkestHour)
                {
                    // name
                    if (keyword.Equals("name"))
                    {
                        string s = ParseString(lexer);
                        if (s == null)
                        {
                            Log.InvalidClause(LogCategory, "name", lexer);
                            continue;
                        }

                        // Alliance name
                        alliance.Name = s;
                        continue;
                    }
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return alliance;
        }

        /// <summary>
        ///     Syntactically analyze the war setting
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>War setting</returns>
        private static War ParseWar(TextLexer lexer)
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

            War war = new War();
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

                // id id
                if (keyword.Equals("id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "id", lexer);
                        continue;
                    }

                    // type When id id Pair of
                    war.Id = id;
                    continue;
                }

                // date
                if (keyword.Equals("date"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "date", lexer);
                        continue;
                    }

                    // Start date and time
                    war.StartDate = date;
                    continue;
                }

                // enddate
                if (keyword.Equals("enddate"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "enddate", lexer);
                        continue;
                    }

                    // End date and time
                    war.EndDate = date;
                    continue;
                }

                // attackers
                if (keyword.Equals("attackers"))
                {
                    Alliance alliance = ParseAlliance(lexer);
                    if (alliance == null)
                    {
                        Log.InvalidSection(LogCategory, "attackers", lexer);
                        continue;
                    }

                    // Attacking Participating Countries
                    war.Attackers = alliance;
                    continue;
                }

                // defenders
                if (keyword.Equals("defenders"))
                {
                    Alliance alliance = ParseAlliance(lexer);
                    if (alliance == null)
                    {
                        Log.InvalidSection(LogCategory, "defenders", lexer);
                        continue;
                    }

                    // Defender Participating Countries
                    war.Defenders = alliance;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return war;
        }

        /// <summary>
        ///     Parsing diplomatic agreement settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Diplomatic agreement setting</returns>
        private static Treaty ParseTreaty(TextLexer lexer)
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

            Treaty treaty = new Treaty();
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

                // id id
                if (keyword.Equals("id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "id", lexer);
                        continue;
                    }

                    // type When id id Pair of
                    treaty.Id = id;
                    continue;
                }

                // type
                if (keyword.Equals("type"))
                {
                    string s = ParseIdentifier(lexer);
                    if (string.IsNullOrEmpty(s))
                    {
                        Log.InvalidClause(LogCategory, "type", lexer);
                        continue;
                    }
                    s = s.ToLower();

                    // non_aggression
                    if (s.Equals("non_aggression"))
                    {
                        treaty.Type = TreatyType.NonAggression;
                        continue;
                    }

                    // peace
                    if (s.Equals("peace"))
                    {
                        treaty.Type = TreatyType.Peace;
                        continue;
                    }

                    // trade
                    if (s.Equals("trade"))
                    {
                        treaty.Type = TreatyType.Trade;
                        continue;
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                // country country
                if (keyword.Equals("country"))
                {
                    Country? tag = ParseTag(lexer);
                    if (!tag.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "country", lexer);
                        continue;
                    }

                    // Target country
                    if (treaty.Country1 == Country.None)
                    {
                        treaty.Country1 = (Country) tag;
                    }
                    else if (treaty.Country2 == Country.None)
                    {
                        treaty.Country2 = (Country) tag;
                    }
                    continue;
                }

                // startdate
                if (keyword.Equals("startdate"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "startdate", lexer);
                        continue;
                    }

                    // Start date and time
                    treaty.StartDate = date;
                    continue;
                }

                // expirydate
                if (keyword.Equals("expirydate"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "expirydate", lexer);
                        continue;
                    }

                    // Expiration date and time
                    treaty.EndDate = date;
                    continue;
                }

                // money
                if (keyword.Equals("money"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "money", lexer);
                        continue;
                    }

                    // Funding
                    treaty.Money = (double) d;
                    continue;
                }

                // supplies supplies
                if (keyword.Equals("supplies"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "supplies", lexer);
                        continue;
                    }

                    // Supplies
                    treaty.Supplies = (double) d;
                    continue;
                }

                // energy
                if (keyword.Equals("energy"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "energy", lexer);
                        continue;
                    }

                    // energy
                    treaty.Energy = (double) d;
                    continue;
                }

                // metal
                if (keyword.Equals("metal"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "metal", lexer);
                        continue;
                    }

                    // metal
                    treaty.Metal = (double) d;
                    continue;
                }

                // rare_materials
                if (keyword.Equals("rare_materials"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "rare_materials", lexer);
                        continue;
                    }

                    // Rare resources
                    treaty.RareMaterials = (double) d;
                    continue;
                }

                // oil
                if (keyword.Equals("oil"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "oil", lexer);
                        continue;
                    }

                    // oil
                    treaty.Oil = (double) d;
                    continue;
                }

                // cancel cancel
                if (keyword.Equals("cancel"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "cancel", lexer);
                        continue;
                    }

                    // Whether it can be canceled
                    treaty.Cancel = (bool) b;
                    continue;
                }

                if (Game.Type == GameType.ArsenalOfDemocracy)
                {
                    // isoversea
                    if (keyword.Equals("isoversea"))
                    {
                        bool? b = ParseBool(lexer);
                        if (!b.HasValue)
                        {
                            Log.InvalidClause(LogCategory, "isoversea", lexer);
                            continue;
                        }

                        // Whether it is foreign trade
                        treaty.IsOverSea = (bool) b;
                        continue;
                    }
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return treaty;
        }

        /// <summary>
        ///     Parsing diplomatic settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Diplomatic settings</returns>
        private static IEnumerable<Relation> ParseDiplomacy(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
                return null;
            }

            // {
            token = lexer.GetToken();
            if (token.Type != TokenType.OpenBrace)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
                return null;
            }

            List<Relation> list = new List<Relation>();
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

                // relation
                if (keyword.Equals("relation"))
                {
                    Relation relation = ParseRelation(lexer);
                    if (relation == null)
                    {
                        Log.InvalidSection(LogCategory, "relation", lexer);
                        continue;
                    }

                    // Diplomatic relations settings
                    list.Add(relation);
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.ReserveToken(token);
                break;
            }

            return list;
        }

        /// <summary>
        ///     Parsing diplomatic relations settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Diplomatic information</returns>
        private static Relation ParseRelation(TextLexer lexer)
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

            Relation relation = new Relation();
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

                // tag
                if (keyword.Equals("tag"))
                {
                    Country? tag = ParseTag(lexer);
                    if (!tag.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "tag", lexer);
                        continue;
                    }

                    // Country tag
                    relation.Country = (Country) tag;
                    continue;
                }

                // value value
                if (keyword.Equals("value"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "value", lexer);
                        continue;
                    }

                    // Relationship value
                    relation.Value = (double) d;
                    continue;
                }

                // access
                if (keyword.Equals("access"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "access", lexer);
                        continue;
                    }

                    // Passage permission
                    relation.Access = (bool) b;
                    continue;
                }

                // guaranteed
                if (keyword.Equals("guaranteed"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "guaranteed", lexer);
                        continue;
                    }

                    // Independence guarantee deadline
                    relation.Guaranteed = date;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return relation;
        }

        #endregion

        #region Nation

        /// <summary>
        ///     Parsing national settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <param name="scenario">Scenario data</param>
        /// <returns>National setting</returns>
        private static CountrySettings ParseCountry(TextLexer lexer, Scenario scenario)
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

            CountrySettings settings = new CountrySettings();
            while (true)
            {
                token = lexer.GetToken();

                // End of file
                if (token == null)
                {
                    Log.MissingCloseBrace(LogCategory, "country", lexer);
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

                // tag
                if (keyword.Equals("tag"))
                {
                    Country? tag = ParseTag(lexer);
                    if (tag == null)
                    {
                        Log.InvalidClause(LogCategory, "tag", lexer);
                        continue;
                    }

                    CountrySettings prev = scenario.Countries.FirstOrDefault(s => s.Country == tag.Value);
                    if (prev != null)
                    {
                        settings = prev;
                    }

                    // Country tag
                    settings.Country = (Country) tag;
                    continue;
                }

                // regular_id
                if (keyword.Equals("regular_id"))
                {
                    Country? tag = ParseTag(lexer);
                    if (tag == null)
                    {
                        Log.InvalidClause(LogCategory, "regular_id", lexer);
                        continue;
                    }

                    // Brotherhood
                    settings.RegularId = (Country) tag;
                    continue;
                }

                // intrinsic_gov_type
                if (keyword.Equals("intrinsic_gov_type"))
                {
                    string s = ParseIdentifier(lexer);
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    s = s.ToLower();

                    if (!Scenarios.GovernmentStrings.Contains(s))
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Independent regime
                    settings.IntrinsicGovType = (GovernmentType) Array.IndexOf(Scenarios.GovernmentStrings, s);
                    continue;
                }

                // puppet
                if (keyword.Equals("puppet"))
                {
                    Country? tag = ParseTag(lexer);
                    if (tag == null)
                    {
                        Log.InvalidClause(LogCategory, "puppet", lexer);
                        continue;
                    }

                    // Suzerainty
                    settings.Master = (Country) tag;
                    continue;
                }

                // control
                if (keyword.Equals("control"))
                {
                    Country? tag = ParseTag(lexer);
                    if (tag == null)
                    {
                        Log.InvalidClause(LogCategory, "control", lexer);
                        continue;
                    }

                    // Country of acquisition of commandership
                    settings.Control = (Country) tag;
                    continue;
                }

                // belligerence
                if (keyword.Equals("belligerence"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "belligerence", lexer);
                        continue;
                    }

                    // Warlikeness
                    settings.Belligerence = (int) n;
                    continue;
                }

                // extra_tc
                if (keyword.Equals("extra_tc"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "extra_tc", lexer);
                        continue;
                    }

                    // Additional transport capacity
                    settings.ExtraTc = (double) d;
                    continue;
                }

                // dissent
                if (keyword.Equals("dissent"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "dissent", lexer);
                        continue;
                    }

                    // National dissatisfaction
                    settings.Dissent = (double) d;
                    continue;
                }

                // capital
                if (keyword.Equals("capital"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "capital", lexer);
                        continue;
                    }

                    // Province of the capital ID
                    settings.Capital = (int) (double) n;
                    continue;
                }

                // tc_mod
                if (keyword.Equals("tc_mod"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "tc_mod", lexer);
                        continue;
                    }

                    // TC correction
                    settings.TcModifier = (double) d;
                    continue;
                }

                // tc_occupied_mod
                if (keyword.Equals("tc_occupied_mod"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "tc_occupied_mod", lexer);
                        continue;
                    }

                    // Occupied territory TC correction
                    settings.TcOccupiedModifier = (double) d;
                    continue;
                }

                // attrition_mod
                if (keyword.Equals("attrition_mod"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "attrition_mod", lexer);
                        continue;
                    }

                    // Consumption compensation
                    settings.AttritionModifier = (double) d;
                    continue;
                }

                // trickleback_mod
                if (keyword.Equals("trickleback_mod"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "trickleback_mod", lexer);
                        continue;
                    }

                    // Gradual withdrawal correction
                    settings.TricklebackModifier = (double) d;
                    continue;
                }

                // max_amphib_mod
                if (keyword.Equals("max_amphib_mod"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "max_amphib_mod", lexer);
                        continue;
                    }

                    // Maximum assault landing correction
                    settings.MaxAmphibModifier = (int) n;
                    continue;
                }

                // supply_dist_mod
                if (keyword.Equals("supply_dist_mod"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "supply_dist_mod", lexer);
                        continue;
                    }

                    // Replenishment correction
                    settings.SupplyDistModifier = (double) d;
                    continue;
                }

                // repair_mod
                if (keyword.Equals("repair_mod"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "repair_mod", lexer);
                        continue;
                    }

                    // Repair correction
                    settings.RepairModifier = (double) d;
                    continue;
                }

                // research_mod
                if (keyword.Equals("research_mod"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "research_mod", lexer);
                        continue;
                    }

                    // Research correction
                    settings.ResearchModifier = (double) d;
                    continue;
                }

                // peacetime_ic_mod
                if (keyword.Equals("peacetime_ic_mod"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "peacetime_ic_mod", lexer);
                        continue;
                    }

                    // Normal time I C correction
                    settings.PeacetimeIcModifier = (double) d;
                    continue;
                }

                // wartime_ic_mod
                if (keyword.Equals("wartime_ic_mod"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "wartime_ic_mod", lexer);
                        continue;
                    }

                    // War time I C correction
                    settings.WartimeIcModifier = (double) d;
                    continue;
                }

                // industrial_modifier
                if (keyword.Equals("industrial_modifier"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "industrial_modifier", lexer);
                        continue;
                    }

                    // Industrial power correction
                    settings.IndustrialModifier = (double) d;
                    continue;
                }

                // ground_def_eff
                if (keyword.Equals("ground_def_eff"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "ground_def_eff", lexer);
                        continue;
                    }

                    // Ground defense correction
                    settings.GroundDefEff = (double) d;
                    continue;
                }

                // ai
                if (keyword.Equals("ai"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "ai", lexer);
                        continue;
                    }

                    // AI file name
                    settings.AiFileName = s;
                    continue;
                }

                // manpower
                if (keyword.Equals("manpower"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "manpower", lexer);
                        continue;
                    }

                    // Human resources
                    settings.Manpower = (double) d;
                    continue;
                }

                // relative_manpower
                if (keyword.Equals("relative_manpower"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "relative_manpower", lexer);
                        continue;
                    }

                    // Human resource correction value
                    settings.RelativeManpower = (double) d;
                    continue;
                }

                // energy
                if (keyword.Equals("energy"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "energy", lexer);
                        continue;
                    }

                    // energy
                    settings.Energy = (double) d;
                    continue;
                }

                // metal
                if (keyword.Equals("metal"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "metal", lexer);
                        continue;
                    }

                    // metal
                    settings.Metal = (double) d;
                    continue;
                }

                // rare_materials
                if (keyword.Equals("rare_materials"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "rare_materials", lexer);
                        continue;
                    }

                    // Rare resources
                    settings.RareMaterials = (double) d;
                    continue;
                }

                // oil
                if (keyword.Equals("oil"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "oil", lexer);
                        continue;
                    }

                    // oil
                    settings.Oil = (double) d;
                    continue;
                }

                // supplies supplies
                if (keyword.Equals("supplies"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "supplies", lexer);
                        continue;
                    }

                    // Supplies
                    settings.Supplies = (double) d;
                    continue;
                }

                // money
                if (keyword.Equals("money"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "money", lexer);
                        continue;
                    }

                    // Funding
                    settings.Money = (double) d;
                    continue;
                }

                // transports
                if (keyword.Equals("transports"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "transports", lexer);
                        continue;
                    }

                    // Transport fleet
                    settings.Transports = (int) n;
                    continue;
                }

                // escorts
                if (keyword.Equals("escorts"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "escorts", lexer);
                        continue;
                    }

                    // Escort ship
                    settings.Escorts = (int) n;
                    continue;
                }

                // nuke
                if (keyword.Equals("nuke"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "nuke", lexer);
                        continue;
                    }

                    // nuclear weapons
                    settings.Nuke = (int) n;
                    continue;
                }

                // free free
                if (keyword.Equals("free"))
                {
                    ResourceSettings free = ParseFree(lexer);
                    if (free == null)
                    {
                        Log.InvalidSection(LogCategory, "free", lexer);
                        continue;
                    }

                    // Off-map resources
                    settings.Offmap = free;
                    continue;
                }

                // consumer
                if (keyword.Equals("consumer"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "consumer", lexer);
                        continue;
                    }

                    // Consumer goods I C ratio
                    settings.ConsumerSlider = (double) d;
                    continue;
                }

                // supply
                if (keyword.Equals("supply"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "supply", lexer);
                        continue;
                    }

                    // Supplies I C ratio
                    settings.ConsumerSlider = (double) d;
                    continue;
                }

                // production
                if (keyword.Equals("production"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "production", lexer);
                        continue;
                    }

                    // production I C ratio
                    settings.ConsumerSlider = (double) d;
                    continue;
                }

                // reinforcement
                if (keyword.Equals("reinforcement"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "reinforcement", lexer);
                        continue;
                    }

                    // Replenishment I C ratio
                    settings.ConsumerSlider = (double) d;
                    continue;
                }

                // diplomacy
                if (keyword.Equals("diplomacy"))
                {
                    IEnumerable<Relation> list = ParseDiplomacy(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "diplomacy", lexer);
                        continue;
                    }

                    // Diplomatic settings
                    settings.Relations.AddRange(list);
                    continue;
                }

                // spyinfo
                if (keyword.Equals("spyinfo"))
                {
                    SpySettings spy = ParseSpyInfo(lexer);
                    if (spy == null)
                    {
                        Log.InvalidSection(LogCategory, "spyinfo", lexer);
                        continue;
                    }

                    // Intelligence settings
                    settings.Intelligence.Add(spy);
                    continue;
                }

                // nationalprovinces
                if (keyword.Equals("nationalprovinces"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "nationalprovinces", lexer);
                        continue;
                    }

                    // Core Providence
                    settings.NationalProvinces.AddRange(list);
                    continue;
                }

                // ownedprovinces
                if (keyword.Equals("ownedprovinces"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "ownedprovinces", lexer);
                        continue;
                    }

                    // Owned Providence
                    settings.OwnedProvinces.AddRange(list);
                    continue;
                }

                // controlledprovinces
                if (keyword.Equals("controlledprovinces"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "controlledprovinces", lexer);
                        continue;
                    }

                    // Domination Providence
                    settings.ControlledProvinces.AddRange(list);
                    continue;
                }

                // techapps
                if (keyword.Equals("techapps"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "techapps", lexer);
                        continue;
                    }

                    // Owned technology
                    settings.TechApps.AddRange(list);
                    continue;
                }

                // blueprints
                if (keyword.Equals("blueprints"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "blueprints", lexer);
                        continue;
                    }

                    // Blueprint
                    settings.BluePrints.AddRange(list);
                    continue;
                }

                // inventions
                if (keyword.Equals("inventions"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "inventions", lexer);
                        continue;
                    }

                    // Invention event
                    settings.Inventions.AddRange(list);
                    continue;
                }

                // deactivate
                if (keyword.Equals("deactivate"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "deactivate", lexer);
                        continue;
                    }

                    // Invalid technology
                    settings.Deactivate.AddRange(list);
                    continue;
                }

                // policy
                if (keyword.Equals("policy"))
                {
                    CountryPolicy policy = ParsePolicy(lexer);
                    if (policy == null)
                    {
                        Log.InvalidSection(LogCategory, "policy", lexer);
                        continue;
                    }

                    // Policy slider
                    settings.Policy = policy;
                    continue;
                }

                // nukedate
                if (keyword.Equals("nukedate"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "nukedate", lexer);
                        continue;
                    }

                    // Date and time of completion of nuclear weapons
                    settings.NukeDate = date;
                    continue;
                }

                // headofstate
                if (keyword.Equals("headofstate"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "headofstate", lexer);
                        continue;
                    }

                    // National leader
                    settings.HeadOfState = id;
                    continue;
                }

                // headofgovernment
                if (keyword.Equals("headofgovernment"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "headofgovernment", lexer);
                        continue;
                    }

                    // Government leaders
                    settings.HeadOfGovernment = id;
                    continue;
                }

                // foreignminister
                if (keyword.Equals("foreignminister"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "foreignminister", lexer);
                        continue;
                    }

                    // Minister of Foreign Affairs
                    settings.ForeignMinister = id;
                    continue;
                }

                // armamentminister
                if (keyword.Equals("armamentminister"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "armamentminister", lexer);
                        continue;
                    }

                    // Minister of Military Demand
                    settings.ArmamentMinister = id;
                    continue;
                }

                // ministerofsecurity
                if (keyword.Equals("ministerofsecurity"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "ministerofsecurity", lexer);
                        continue;
                    }

                    // Minister of Interior
                    settings.MinisterOfSecurity = id;
                    continue;
                }

                // ministerofintelligence
                if (keyword.Equals("ministerofintelligence"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "ministerofintelligence", lexer);
                        continue;
                    }

                    // Minister of Information
                    settings.MinisterOfIntelligence = id;
                    continue;
                }

                // chiefofstaff
                if (keyword.Equals("chiefofstaff"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "chiefofstaff", lexer);
                        continue;
                    }

                    // Chief of the Defense Staff
                    settings.ChiefOfStaff = id;
                    continue;
                }

                // chiefofarmy
                if (keyword.Equals("chiefofarmy"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "chiefofarmy", lexer);
                        continue;
                    }

                    // Army General Commander
                    settings.ChiefOfArmy = id;
                    continue;
                }

                // chiefofnavy
                if (keyword.Equals("chiefofnavy"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "chiefofnavy", lexer);
                        continue;
                    }

                    // Navy Commander
                    settings.ChiefOfNavy = id;
                    continue;
                }

                // chiefofair
                if (keyword.Equals("chiefofair"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "chiefofair", lexer);
                        continue;
                    }

                    // Air Force Commander
                    settings.ChiefOfAir = id;
                    continue;
                }

                // national identity
                if (keyword.Equals("nationalidentity"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "nationalidentity", lexer);
                        continue;
                    }

                    // Public awareness
                    settings.NationalIdentity = s;
                    continue;
                }

                // socialpolicy
                if (keyword.Equals("socialpolicy"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "socialpolicy", lexer);
                        continue;
                    }

                    // Social policy
                    settings.SocialPolicy = s;
                    continue;
                }

                // nationalculture
                if (keyword.Equals("nationalculture"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "nationalculture", lexer);
                        continue;
                    }

                    // National culture
                    settings.NationalCulture = s;
                    continue;
                }

                // dormant_leaders
                if (keyword.Equals("dormant_leaders"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "dormant_leaders", lexer);
                        continue;
                    }

                    // Pause commander
                    settings.DormantLeaders.AddRange(list);
                    continue;
                }

                // dormant_ministers
                if (keyword.Equals("dormant_ministers"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "dormant_ministers", lexer);
                        continue;
                    }

                    // Paused ministers
                    settings.DormantMinisters.AddRange(list);
                    continue;
                }

                // dormant_teams
                if (keyword.Equals("dormant_teams"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "dormant_teams", lexer);
                        continue;
                    }

                    // Rest research institution
                    settings.DormantTeams.AddRange(list);
                    continue;
                }

                // steal_leader
                if (keyword.Equals("steal_leader"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "steal_leader", lexer);
                        continue;
                    }

                    // Extraction commander
                    settings.StealLeaders.Add((int) n);
                    continue;
                }

                // allowed_divisions
                if (keyword.Equals("allowed_divisions"))
                {
                    Dictionary<UnitType, bool> divisions = ParseAllowedDivisions(lexer);
                    if (divisions == null)
                    {
                        Log.InvalidClause(LogCategory, "allowed_divisions", lexer);
                        continue;
                    }

                    // Producible division
                    foreach (KeyValuePair<UnitType, bool> pair in divisions)
                    {
                        settings.AllowedDivisions[pair.Key] = pair.Value;
                    }
                    continue;
                }

                // allowed_brigades
                if (keyword.Equals("allowed_brigades"))
                {
                    Dictionary<UnitType, bool> brigades = ParseAllowedBrigades(lexer);
                    if (brigades == null)
                    {
                        Log.InvalidClause(LogCategory, "allowed_brigades", lexer);
                        continue;
                    }

                    // Producible brigade
                    foreach (KeyValuePair<UnitType, bool> pair in brigades)
                    {
                        settings.AllowedDivisions[pair.Key] = pair.Value;
                    }
                    continue;
                }

                // convoy
                if (keyword.Equals("convoy"))
                {
                    Convoy convoy = ParseConvoy(lexer);
                    if (convoy == null)
                    {
                        Log.InvalidSection(LogCategory, "convoy", lexer);
                        continue;
                    }

                    // Transport fleet
                    settings.Convoys.Add(convoy);
                    continue;
                }

                // landunit
                if (keyword.Equals("landunit"))
                {
                    Unit unit = ParseUnit(lexer, Branch.Army);
                    if (unit == null)
                    {
                        Log.InvalidSection(LogCategory, "landunit", lexer);
                        continue;
                    }

                    // Army unit
                    settings.LandUnits.Add(unit);
                    continue;
                }

                // navalunit
                if (keyword.Equals("navalunit"))
                {
                    Unit unit = ParseUnit(lexer, Branch.Navy);
                    if (unit == null)
                    {
                        Log.InvalidSection(LogCategory, "navalunit", lexer);
                        continue;
                    }

                    // Navy unit
                    settings.NavalUnits.Add(unit);
                    continue;
                }

                // airunit
                if (keyword.Equals("airunit"))
                {
                    Unit unit = ParseUnit(lexer, Branch.Airforce);
                    if (unit == null)
                    {
                        Log.InvalidSection(LogCategory, "airunit", lexer);
                        continue;
                    }

                    // Air Force Unit
                    settings.AirUnits.Add(unit);
                    continue;
                }

                // division_development
                if (keyword.Equals("division_development"))
                {
                    DivisionDevelopment division = ParseDivisionDevelopment(lexer);
                    if (division == null)
                    {
                        Log.InvalidSection(LogCategory, "division_development", lexer);
                        continue;
                    }

                    // Division in production
                    settings.DivisionDevelopments.Add(division);
                    continue;
                }

                // convoy_development
                if (keyword.Equals("convoy_development"))
                {
                    ConvoyDevelopment convoy = ParseConvoyDevelopment(lexer);
                    if (convoy == null)
                    {
                        Log.InvalidSection(LogCategory, "convoy_development", lexer);
                        continue;
                    }

                    // Convoy in production
                    settings.ConvoyDevelopments.Add(convoy);
                    continue;
                }

                // province_development
                if (keyword.Equals("province_development"))
                {
                    BuildingDevelopment building = ParseBuildingDevelopment(lexer);
                    if (building == null)
                    {
                        Log.InvalidSection(LogCategory, "province_development", lexer);
                        continue;
                    }

                    // Building in production
                    settings.BuildingDevelopments.Add(building);
                    continue;
                }

                // landdivision
                if (keyword.Equals("landdivision"))
                {
                    Division division = ParseDivision(lexer);
                    if (division == null)
                    {
                        Log.InvalidSection(LogCategory, "landdivision", lexer);
                        continue;
                    }

                    // Army division
                    division.Branch = Branch.Army;
                    settings.LandDivisions.Add(division);
                    continue;
                }

                // navaldivision
                if (keyword.Equals("navaldivision"))
                {
                    Division division = ParseDivision(lexer);
                    if (division == null)
                    {
                        Log.InvalidSection(LogCategory, "navaldivision", lexer);
                        continue;
                    }

                    // Navy Division
                    division.Branch = Branch.Navy;
                    settings.NavalDivisions.Add(division);
                    continue;
                }

                // airdivision
                if (keyword.Equals("airdivision"))
                {
                    Division division = ParseDivision(lexer);
                    if (division == null)
                    {
                        Log.InvalidSection(LogCategory, "airdivision", lexer);
                        continue;
                    }

                    // Air Force Division
                    division.Branch = Branch.Airforce;
                    settings.AirDivisions.Add(division);
                    continue;
                }

                if (Game.Type == GameType.DarkestHour)
                {
                    // name
                    if (keyword.Equals("name"))
                    {
                        string s = ParseStringOrIdentifier(lexer);
                        if (string.IsNullOrEmpty(s))
                        {
                            Log.InvalidClause(LogCategory, "name", lexer);
                            continue;
                        }

                        // Country name
                        settings.Name = s;
                        continue;
                    }

                    // flag_ext
                    if (keyword.Equals("flag_ext"))
                    {
                        string s = ParseStringOrIdentifier(lexer);
                        if (string.IsNullOrEmpty(s))
                        {
                            Log.InvalidClause(LogCategory, "flag_ext", lexer);
                            continue;
                        }

                        // Flag suffix
                        settings.FlagExt = s;
                        continue;
                    }

                    // ai_settings
                    if (keyword.Equals("ai_settings"))
                    {
                        AiSettings ai = ParseAiSettings(lexer);
                        if (settings == null)
                        {
                            Log.InvalidSection(LogCategory, "ai_settings", lexer);
                            continue;
                        }

                        // AI setting
                        settings.AiSettings = ai;
                        continue;
                    }

                    // claimedprovinces
                    if (keyword.Equals("claimedprovinces"))
                    {
                        IEnumerable<int> list = ParseIdList(lexer);
                        if (list == null)
                        {
                            Log.InvalidSection(LogCategory, "claimedprovinces", lexer);
                            continue;
                        }

                        // Province claim
                        settings.ClaimedProvinces.AddRange(list);
                        continue;
                    }
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return settings;
        }

        /// <summary>
        ///     AI Parse the settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>AI setting</returns>
        private static AiSettings ParseAiSettings(TextLexer lexer)
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

            AiSettings settings = new AiSettings();
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

                // flags flags
                if (keyword.Equals("flags"))
                {
                    Dictionary<string, string> flags = ParseFlags(lexer);
                    if (flags == null)
                    {
                        Log.InvalidSection(LogCategory, "flags", lexer);
                        continue;
                    }

                    // Local flag list
                    settings.Flags = flags;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return settings;
        }

        /// <summary>
        ///     Parse resource settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Resource setting</returns>
        private static ResourceSettings ParseFree(TextLexer lexer)
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

            ResourceSettings free = new ResourceSettings();
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

                // I C
                if (keyword.Equals("ic"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "ic", lexer);
                        continue;
                    }

                    // Industrial power
                    free.Ic = (double) d;
                    continue;
                }

                // manpower
                if (keyword.Equals("manpower"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "manpower", lexer);
                        continue;
                    }

                    // Human resources
                    free.Manpower = (double) d;
                    continue;
                }

                // energy
                if (keyword.Equals("energy"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "energy", lexer);
                        continue;
                    }

                    // energy
                    free.Energy = (double) d;
                    continue;
                }

                // metal
                if (keyword.Equals("metal"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "metal", lexer);
                        continue;
                    }

                    // metal
                    free.Metal = (double) d;
                    continue;
                }

                // rare_materials
                if (keyword.Equals("rare_materials"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "rare_materials", lexer);
                        continue;
                    }

                    // Rare resources
                    free.RareMaterials = (double) d;
                    continue;
                }

                // oil
                if (keyword.Equals("oil"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "oil", lexer);
                        continue;
                    }

                    // oil
                    free.Oil = (double) d;
                    continue;
                }

                // supplies supplies
                if (keyword.Equals("supplies"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "supplies", lexer);
                        continue;
                    }

                    // Supplies
                    free.Supplies = (double) d;
                    continue;
                }

                // money
                if (keyword.Equals("money"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "money", lexer);
                        continue;
                    }

                    // Funding
                    free.Money = (double) d;
                    continue;
                }

                // transport
                if (keyword.Equals("transport"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "transport", lexer);
                        continue;
                    }

                    // Transport fleet
                    free.Transports = (int) n;
                    continue;
                }

                // escort
                if (keyword.Equals("escort"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "escort", lexer);
                        continue;
                    }

                    // Escort ship
                    free.Escorts = (int) n;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return free;
        }

        /// <summary>
        ///     Syntactically analyze intelligence settings
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Intelligence settings</returns>
        private static SpySettings ParseSpyInfo(TextLexer lexer)
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

            SpySettings spy = new SpySettings();
            int lastLineNo = -1;
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

                // country country
                if (keyword.Equals("country"))
                {
                    Country? tag = ParseTag(lexer);
                    if (!tag.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "country", lexer);
                        continue;
                    }

                    // Country tag
                    spy.Country = (Country) tag;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // numberofspies
                if (keyword.Equals("numberofspies"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "numberofspies", lexer);
                        continue;
                    }

                    // Number of spies
                    spy.Spies = (int) n;

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
                lexer.SkipLine();
            }

            return spy;
        }

        /// <summary>
        ///     Parsing policy sliders
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Policy slider</returns>
        private static CountryPolicy ParsePolicy(TextLexer lexer)
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

            CountryPolicy policy = new CountryPolicy();
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

                // date
                if (keyword.Equals("date"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "date", lexer);
                        continue;
                    }

                    // Slider movable date and time
                    policy.Date = date;
                    continue;
                }

                // democratic
                if (keyword.Equals("democratic"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "democratic", lexer);
                        continue;
                    }

                    if (n < 1 || n > 10)
                    {
                        Log.OutOfRange(LogCategory, "democratic", n, lexer);
                        continue;
                    }

                    // Democratic ――――Dictatorship
                    policy.Democratic = (int) n;
                    continue;
                }

                // political_left
                if (keyword.Equals("political_left"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "political_left", lexer);
                        continue;
                    }

                    if (n < 1 || n > 10)
                    {
                        Log.OutOfRange(LogCategory, "political_left", n, lexer);
                        continue;
                    }

                    // Political left ―――― Political right
                    policy.PoliticalLeft = (int) n;
                    continue;
                }

                // freedom
                if (keyword.Equals("freedom"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "freedom", lexer);
                        continue;
                    }

                    if (n < 1 || n > 10)
                    {
                        Log.OutOfRange(LogCategory, "freedom", n, lexer);
                        continue;
                    }

                    // Open society ―――― Closed society
                    policy.Freedom = (int) n;
                    continue;
                }

                // free_market
                if (keyword.Equals("free_market"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "freedom", lexer);
                        continue;
                    }

                    if (n < 1 || n > 10)
                    {
                        Log.OutOfRange(LogCategory, "freedom", n, lexer);
                        continue;
                    }

                    // Free economy ―――― Central planned economy
                    policy.FreeMarket = (int) n;
                    continue;
                }

                // professional_army
                if (keyword.Equals("professional_army"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "professional_army", lexer);
                        continue;
                    }

                    if (n < 1 || n > 10)
                    {
                        Log.OutOfRange(LogCategory, "professional_army", n, lexer);
                        continue;
                    }

                    // Standing army ―――― Conscription army (DH Full Then mobilize ―――― Reinstatement )
                    policy.ProfessionalArmy = (int) n;
                    continue;
                }

                // defense_lobby
                if (keyword.Equals("defense_lobby"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "defense_lobby", lexer);
                        continue;
                    }

                    if (n < 1 || n > 10)
                    {
                        Log.OutOfRange(LogCategory, "defense_lobby", n, lexer);
                        continue;
                    }

                    // Taka faction ―――― Pigeon faction
                    policy.DefenseLobby = (int) n;
                    continue;
                }

                // interventionism
                if (keyword.Equals("interventionism"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "interventionism", lexer);
                        continue;
                    }

                    if (n < 1 || n > 10)
                    {
                        Log.OutOfRange(LogCategory, "interventionism", n, lexer);
                        continue;
                    }

                    // Interventionism ―――― Isolation
                    policy.Interventionism = (int) n;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return policy;
        }

        /// <summary>
        ///     Syntactically analyze productionable divisions
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Producible division</returns>
        private static Dictionary<UnitType, bool> ParseAllowedDivisions(TextLexer lexer)
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

            Dictionary<UnitType, bool> divisions = new Dictionary<UnitType, bool>();
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

                bool? b = ParseBool(lexer);
                if (!b.HasValue)
                {
                    Log.InvalidClause(LogCategory, "allowed_divisions", lexer);
                    lexer.SkipLine();
                    continue;
                }

                divisions[type] = (bool) b;
            }

            return divisions;
        }

        /// <summary>
        ///     Syntactically analyze the produceable brigade
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Producible brigade</returns>
        private static Dictionary<UnitType, bool> ParseAllowedBrigades(TextLexer lexer)
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

            Dictionary<UnitType, bool> brigades = new Dictionary<UnitType, bool>();
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
                if (!Units.BrigadeTypes.Contains(type))
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                bool? b = ParseBool(lexer);
                if (!b.HasValue)
                {
                    Log.InvalidClause(LogCategory, Units.Strings[(int) type], lexer);
                    lexer.SkipLine();
                    continue;
                }

                brigades[type] = (bool) b;
            }

            return brigades;
        }

        #endregion

        #region unit

        /// <summary>
        ///     Parse the unit
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <param name="branch">Military department</param>
        /// <returns>unit</returns>
        private static Unit ParseUnit(TextLexer lexer, Branch branch)
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

            Unit unit = new Unit { Branch = branch };
            int lastLineNo = -1;
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

                // id id
                if (keyword.Equals("id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "id", lexer);
                        continue;
                    }

                    // type When id id Pair of
                    unit.Id = id;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // name
                if (keyword.Equals("name"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "name", lexer);
                        continue;
                    }

                    // Unit name
                    unit.Name = s;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // control
                if (keyword.Equals("control"))
                {
                    Country? tag = ParseTag(lexer);
                    if (!tag.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "control", lexer);
                        continue;
                    }

                    // Commander's country
                    unit.Control = (Country) tag;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // leader
                if (keyword.Equals("leader"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "leader", lexer);
                        continue;
                    }

                    // Commander
                    unit.Leader = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // location
                if (keyword.Equals("location"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "location", lexer);
                        continue;
                    }

                    // present location
                    unit.Location = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // prevprov
                if (keyword.Equals("prevprov"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "prevprov", lexer);
                        continue;
                    }

                    // Immediately before position
                    unit.PrevProv = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // home
                if (keyword.Equals("home"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "home", lexer);
                        continue;
                    }

                    // Reference position
                    unit.Home = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // base
                if (keyword.Equals("base"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "base", lexer);
                        continue;
                    }

                    // Affiliation base
                    unit.Base = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // dig_in
                if (keyword.Equals("dig_in"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "dig_in", lexer);
                        continue;
                    }

                    // 塹 壕 level
                    unit.DigIn = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // morale
                if (keyword.Equals("morale"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "morale", lexer);
                        continue;
                    }

                    // morale
                    unit.Morale = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // mission mission
                if (keyword.Equals("mission"))
                {
                    Mission mission = ParseMission(lexer);
                    if (mission == null)
                    {
                        Log.InvalidSection(LogCategory, "mission", lexer);
                        continue;
                    }

                    // mission
                    unit.Mission = mission;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // date
                if (keyword.Equals("date"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "date", lexer);
                        continue;
                    }

                    // Specified date and time
                    unit.Date = date;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // development
                if (keyword.Equals("development"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "development", lexer);
                        continue;
                    }

                    // development ( details unknown )
                    unit.Development = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // movetime
                if (keyword.Equals("movetime"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "movetime", lexer);
                        continue;
                    }

                    // Move completion date and time
                    unit.MoveTime = date;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // movement
                if (keyword.Equals("movement"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "movement", lexer);
                        continue;
                    }

                    // Travel route
                    unit.Movement.AddRange(list);

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // attack
                if (keyword.Equals("attack"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "attack", lexer);
                        continue;
                    }

                    // Attack start date and time
                    unit.AttackDate = date;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // invasion
                if (keyword.Equals("invasion"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "invasion", lexer);
                        continue;
                    }

                    // During landing
                    unit.Invasion = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // target target
                if (keyword.Equals("target"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "target", lexer);
                        continue;
                    }

                    // Landing destination
                    unit.Target = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // stand_ground
                if (keyword.Equals("stand_ground"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "stand_ground", lexer);
                        continue;
                    }

                    // Death guard order
                    unit.StandGround = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // scorch_ground
                if (keyword.Equals("scorch_ground"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "scorch_ground", lexer);
                        continue;
                    }

                    // Scorched earth operation
                    unit.ScorchGround = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // prioritized
                if (keyword.Equals("prioritized"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "prioritized", lexer);
                        continue;
                    }

                    // priority
                    unit.Prioritized = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // can_upgrade
                if (keyword.Equals("can_upgrade"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "can_upgrade", lexer);
                        continue;
                    }

                    // Can be improved
                    unit.CanUpgrade = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // can_reinforce
                if (keyword.Equals("can_reinforce"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "can_reinforce", lexer);
                        continue;
                    }

                    // Can be replenished
                    unit.CanReinforcement = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // division
                if (keyword.Equals("division"))
                {
                    Division division = ParseDivision(lexer);
                    if (division == null)
                    {
                        Log.InvalidSection(LogCategory, "division", lexer);
                        continue;
                    }

                    // Set up the military department
                    division.Branch = unit.Branch;

                    // Division
                    unit.Divisions.Add(division);

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // landunit
                if (keyword.Equals("landunit") && unit.Branch != Branch.Army)
                {
                    Unit landUnit = ParseUnit(lexer, Branch.Army);
                    if (landUnit == null)
                    {
                        Log.InvalidSection(LogCategory, "landunit", lexer);
                        continue;
                    }

                    // On-board unit
                    unit.LandUnits.Add(landUnit);

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // strength
                if (keyword.Equals("strength"))
                {
                    Log.InvalidToken(LogCategory, token, lexer);

                    // strength Is targeted for the division, but it is discarded to avoid subsequent errors.
                    ParseDouble(lexer);

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // locked locked
                if (keyword.Equals("locked"))
                {
                    Log.InvalidToken(LogCategory, token, lexer);

                    // locked locked Is targeted for the division, but it is discarded to avoid subsequent errors.
                    ParseBool(lexer);

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
                lexer.SkipLine();
            }

            return unit;
        }

        #endregion

        #region Division

        /// <summary>
        ///     Parsing the division
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Division</returns>
        private static Division ParseDivision(TextLexer lexer)
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

            Division division = new Division();
            int lastLineNo = -1;
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

                // id id
                if (keyword.Equals("id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "id", lexer);
                        continue;
                    }

                    // type When id id Pair of
                    division.Id = id;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // name
                if (keyword.Equals("name"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "name", lexer);
                        continue;
                    }

                    // Division name
                    division.Name = s;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // type
                if (keyword.Equals("type"))
                {
                    UnitType? type = ParseDivisionType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "type", lexer);
                        continue;
                    }

                    // Unit type
                    division.Type = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // model
                if (keyword.Equals("model"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "model", lexer);
                        continue;
                    }

                    // Model number
                    division.Model = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // nuke
                if (keyword.Equals("nuke"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "nuke", lexer);
                        continue;
                    }

                    // Equipped with nuclear weapons
                    division.Nuke = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra
                if (keyword.Equals("extra"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra1 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra1
                if (keyword.Equals("extra1"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra1", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra1 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra2
                if (keyword.Equals("extra2"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra2", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra2 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra3
                if (keyword.Equals("extra3"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra3", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra3 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra4
                if (keyword.Equals("extra4"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra4", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra4 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra5
                if (keyword.Equals("extra5"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra5", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra5 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model
                if (keyword.Equals("brigade_model"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel1 = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model1
                if (keyword.Equals("brigade_model1"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model1", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel1 = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model2
                if (keyword.Equals("brigade_model2"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model2", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel2 = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model3
                if (keyword.Equals("brigade_model3"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model3", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel3 = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model4
                if (keyword.Equals("brigade_model4"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model4", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel4 = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model5
                if (keyword.Equals("brigade_model5"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model5", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel5 = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // max_strength
                if (keyword.Equals("max_strength"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "max_strength", lexer);
                        continue;
                    }

                    // Maximum strength
                    division.MaxStrength = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // strength
                if (keyword.Equals("strength"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "strength", lexer);
                        continue;
                    }

                    // Strength
                    division.Strength = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // defaultorganization
                if (keyword.Equals("defaultorganisation"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "defaultorganisation", lexer);
                        continue;
                    }

                    // Maximum organization rate
                    division.MaxOrganisation = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // organizationisation
                if (keyword.Equals("organisation"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "organisation", lexer);
                        continue;
                    }

                    // Organization rate
                    division.Organisation = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // morale
                if (keyword.Equals("morale"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "morale", lexer);
                        continue;
                    }

                    // morale
                    division.Morale = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // experience experience
                if (keyword.Equals("experience"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "experience", lexer);
                        continue;
                    }

                    // Experience point
                    division.Experience = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // div_upgr_progress
                if (keyword.Equals("div_upgr_progress"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "div_upgr_progress", lexer);
                        continue;
                    }

                    // Improvement progress rate
                    division.UpgradeProgress = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // redep_target
                if (keyword.Equals("redep_target"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "redep_target", lexer);
                        continue;
                    }

                    // Relocation destination province
                    division.RedeployTarget = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // redep_unit_name
                if (keyword.Equals("redep_unit_name"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "redep_unit_name", lexer);
                        continue;
                    }

                    // Relocation destination unit name
                    division.RedeployUnitName = s;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // redep_unit_id
                if (keyword.Equals("redep_unit_id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "redep_unit_id", lexer);
                        continue;
                    }

                    // Relocation destination unit ID
                    division.RedeployUnitId = id;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // offensive
                if (keyword.Equals("offensive"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "offensive", lexer);
                        continue;
                    }

                    // Offensive start date and time
                    division.Offensive = date;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // supplies supplies
                if (keyword.Equals("supplies"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "supplies", lexer);
                        continue;
                    }

                    // Supplies
                    division.Supplies = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // oil
                if (keyword.Equals("oil"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "oil", lexer);
                        continue;
                    }

                    // fuel
                    division.Fuel = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // max_supply_stock
                if (keyword.Equals("max_supply_stock"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "max_supply_stock", lexer);
                        continue;
                    }

                    // Largest supplies
                    division.MaxSupplies = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // max_oil_stock
                if (keyword.Equals("max_oil_stock"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "max_oil_stock", lexer);
                        continue;
                    }

                    // Maximum fuel
                    division.MaxFuel = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // supply consumption
                if (keyword.Equals("supplyconsumption"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "supplyconsumption", lexer);
                        continue;
                    }

                    // Consumption of supplies
                    division.SupplyConsumption = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // fuel consumption
                if (keyword.Equals("fuelconsumption"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "fuelconsumption", lexer);
                        continue;
                    }

                    // Fuel consumption
                    division.FuelConsumption = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // maxspeed
                if (keyword.Equals("maxspeed"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "maxspeed", lexer);
                        continue;
                    }

                    // Maximum speed
                    division.MaxSpeed = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // speed_cap_art
                if (keyword.Equals("speed_cap_art"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "speed_cap_art", lexer);
                        continue;
                    }

                    // Artillery speed cap
                    division.SpeedCapArt = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // speed_cap_eng
                if (keyword.Equals("speed_cap_eng"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "speed_cap_eng", lexer);
                        continue;
                    }

                    // Engineer speed cap
                    division.SpeedCapEng = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // speed_cap_aa
                if (keyword.Equals("speed_cap_aa"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "speed_cap_aa", lexer);
                        continue;
                    }

                    // Anti-aircraft speed cap
                    division.SpeedCapAa = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // speed_cap_at
                if (keyword.Equals("speed_cap_at"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "speed_cap_at", lexer);
                        continue;
                    }

                    // Anti-tank speed cap
                    division.SpeedCapAt = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // transportweight
                if (keyword.Equals("transportweight"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "transportweight", lexer);
                        continue;
                    }

                    // Transport load
                    division.TransportWeight = (double) d;
                    continue;
                }

                // transport capability
                if (keyword.Equals("transportcapability"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "transportcapability", lexer);
                        continue;
                    }

                    // Transport capacity
                    division.TransportCapability = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // defensiveness
                if (keyword.Equals("defensiveness"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "defensiveness", lexer);
                        continue;
                    }

                    // Defense power
                    division.Defensiveness = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // toughness
                if (keyword.Equals("toughness"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "toughness", lexer);
                        continue;
                    }

                    // Endurance
                    division.Toughness = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // softness
                if (keyword.Equals("softness"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "softness", lexer);
                        continue;
                    }

                    // Vulnerability
                    division.Softness = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // suppression
                if (keyword.Equals("suppression"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "suppression", lexer);
                        continue;
                    }

                    // Control
                    division.Suppression = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // seadefence
                if (keyword.Equals("seadefence"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "seadefence", lexer);
                        continue;
                    }

                    // Anti-ship / / Anti-submarine defense
                    division.SeaDefense = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // surface defense
                if (keyword.Equals("surfacedefence"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "surfacedefence", lexer);
                        continue;
                    }

                    // Ground defense
                    division.SurfaceDefence = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // airdefence
                if (keyword.Equals("airdefence"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "airdefence", lexer);
                        continue;
                    }

                    // Anti-aircraft defense
                    division.AirDefence = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // soft attack
                if (keyword.Equals("softattack"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "softattack", lexer);
                        continue;
                    }

                    // Interpersonal attack power
                    division.SoftAttack = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // hard attack
                if (keyword.Equals("hardattack"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "hardattack", lexer);
                        continue;
                    }

                    // Anti-instep attack power
                    division.HardAttack = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // seaattack
                if (keyword.Equals("seaattack"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "seaattack", lexer);
                        continue;
                    }

                    // Anti-ship attack power (( Navy)
                    division.SeaAttack = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // subattack
                if (keyword.Equals("subattack"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "subattack", lexer);
                        continue;
                    }

                    // Anti-submarine attack power
                    division.SubAttack = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // convoy attack
                if (keyword.Equals("convoyattack"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "convoyattack", lexer);
                        continue;
                    }

                    // Trade destructive power
                    division.ConvoyAttack = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // shore bombardment
                if (keyword.Equals("shorebombardment"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "shorebombardment", lexer);
                        continue;
                    }

                    // Coastal artillery ability
                    division.ShoreBombardment = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // air attack
                if (keyword.Equals("airattack"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "airattack", lexer);
                        continue;
                    }

                    // Anti-aircraft attack power
                    division.AirAttack = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // strategic attack
                if (keyword.Equals("strategicattack"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "strategicattack", lexer);
                        continue;
                    }

                    // Strategic bombing attack power
                    division.StrategicAttack = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // naval attack
                if (keyword.Equals("navalattack"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "navalattack", lexer);
                        continue;
                    }

                    // Air-to-ship attack power
                    division.NavalAttack = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // artillery_bombardment
                if (keyword.Equals("artillery_bombardment"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "artillery_bombardment", lexer);
                        continue;
                    }

                    // Shooting ability
                    division.ArtilleryBombardment = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // surfacedetection capability
                if (keyword.Equals("surfacedetectioncapability"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "surfacedetectioncapability", lexer);
                        continue;
                    }

                    // Anti-ship search ability
                    division.SurfaceDetection = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // airdetection capability
                if (keyword.Equals("airdetectioncapability"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "airdetectioncapability", lexer);
                        continue;
                    }

                    // Anti-aircraft search ability
                    division.AirDetection = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // subdetection capability
                if (keyword.Equals("subdetectioncapability"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "subdetectioncapability", lexer);
                        continue;
                    }

                    // Anti-submarine enemy ability
                    division.SubDetection = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // visibility
                if (keyword.Equals("visibility"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "visibility", lexer);
                        continue;
                    }

                    // Visibility
                    division.Visibility = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // range
                if (keyword.Equals("range"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "range", lexer);
                        continue;
                    }

                    // Cruising distance
                    division.Range = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // distance
                if (keyword.Equals("distance"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "distance", lexer);
                        continue;
                    }

                    // Range distance
                    division.Distance = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // travelled
                if (keyword.Equals("travelled"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "travelled", lexer);
                        continue;
                    }

                    // Moving distance
                    division.Travelled = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // locked locked
                if (keyword.Equals("locked"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "dormant", lexer);
                        continue;
                    }

                    // Cannot move
                    division.Locked = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // dormant
                if (keyword.Equals("dormant"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "dormant", lexer);
                        continue;
                    }

                    // Hibernate
                    division.Dormant = (bool) b;

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
                lexer.SkipLine();
            }

            return division;
        }

        /// <summary>
        ///     Parsing the production division
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Division in production</returns>
        private static DivisionDevelopment ParseDivisionDevelopment(TextLexer lexer)
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

            DivisionDevelopment division = new DivisionDevelopment();
            int lastLineNo = -1;
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

                // id id
                if (keyword.Equals("id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "id", lexer);
                        continue;
                    }

                    // type When id id Pair of
                    division.Id = id;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // name
                if (keyword.Equals("name"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "name", lexer);
                        continue;
                    }

                    // Division name
                    division.Name = s;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // cost
                if (keyword.Equals("cost"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "cost", lexer);
                        continue;
                    }

                    // requirement I C
                    division.Cost = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // manpower
                if (keyword.Equals("manpower"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "manpower", lexer);
                        continue;
                    }

                    // Necessary human resources
                    division.Manpower = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // unitcost
                if (keyword.Equals("unitcost"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "unitcost", lexer);
                        continue;
                    }

                    // unitcost
                    division.UnitCost = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // new_model
                if (keyword.Equals("new_model"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "unitcost", lexer);
                        continue;
                    }

                    // new_model
                    division.NewModel = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // date
                if (keyword.Equals("date"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "date", lexer);
                        continue;
                    }

                    // Completion date
                    division.Date = date;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // progress
                if (keyword.Equals("progress"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "progress", lexer);
                        continue;
                    }

                    // Progress rate increment
                    division.Progress = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // total_progress
                if (keyword.Equals("total_progress"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "total_progress", lexer);
                        continue;
                    }

                    // Total progress rate
                    division.TotalProgress = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // gearing_bonus
                if (keyword.Equals("gearing_bonus"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "gearing_bonus", lexer);
                        continue;
                    }

                    // Continuous production bonus
                    division.GearingBonus = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // size
                if (keyword.Equals("size"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "size", lexer);
                        continue;
                    }

                    // Total production number
                    division.Size = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // done done
                if (keyword.Equals("done"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "done", lexer);
                        continue;
                    }

                    // Number of completed production
                    division.Done = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // days
                if (keyword.Equals("days"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "days", lexer);
                        continue;
                    }

                    // Days to complete
                    division.Days = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // days_for_first
                if (keyword.Equals("days_for_first"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "days_for_first", lexer);
                        continue;
                    }

                    // 1 Number of days to complete the unit
                    division.DaysForFirst = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // halted
                if (keyword.Equals("halted"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "halted", lexer);
                        continue;
                    }

                    // Stopping
                    division.Halted = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // close_when_finished
                if (keyword.Equals("close_when_finished"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "close_when_finished", lexer);
                        continue;
                    }

                    // Whether to delete the queue on completion
                    division.CloseWhenFinished = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // waiting for closure
                if (keyword.Equals("waitingforclosure"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "waitingforclosure", lexer);
                        continue;
                    }

                    // waitingforclosure (waitforclosure ( details unknown )
                    division.WaitingForClosure = (bool) b;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // retooling_time
                if (keyword.Equals("retooling_time"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "retooling_time", lexer);
                        continue;
                    }

                    // Production line preparation time
                    division.RetoolingTime = (double) d;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // type
                if (keyword.Equals("type"))
                {
                    UnitType? type = ParseDivisionType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "type", lexer);
                        continue;
                    }

                    // Unit type
                    division.Type = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // model
                if (keyword.Equals("model"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "model", lexer);
                        continue;
                    }

                    // Model number
                    division.Model = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra
                if (keyword.Equals("extra"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra1 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra1
                if (keyword.Equals("extra1"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra1", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra1 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra2
                if (keyword.Equals("extra2"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra2", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra2 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra3
                if (keyword.Equals("extra3"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra3", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra3 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra4
                if (keyword.Equals("extra4"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra4", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra4 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // extra5
                if (keyword.Equals("extra5"))
                {
                    UnitType? type = ParseBrigadeType(lexer);
                    if (type == null)
                    {
                        Log.InvalidClause(LogCategory, "extra5", lexer);
                        continue;
                    }

                    // Attached brigade unit type
                    division.Extra5 = (UnitType) type;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model
                if (keyword.Equals("brigade_model"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel1 = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model1
                if (keyword.Equals("brigade_model1"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model1", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel1 = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model2
                if (keyword.Equals("brigade_model2"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model2", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel2 = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model3
                if (keyword.Equals("brigade_model3"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model3", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel3 = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model4
                if (keyword.Equals("brigade_model4"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model4", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel4 = (int) n;

                    // Remember the final interpretation line
                    lastLineNo = lexer.LineNo;
                    continue;
                }

                // brigade_model5
                if (keyword.Equals("brigade_model5"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "brigade_model5", lexer);
                        continue;
                    }

                    // Model number of the attached brigade
                    division.BrigadeModel5 = (int) n;

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
                lexer.SkipLine();
            }

            return division;
        }

        #endregion

        #region mission

        /// <summary>
        ///     Parse the mission
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>mission</returns>
        private static Mission ParseMission(TextLexer lexer)
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

            Mission mission = new Mission();
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
                        continue;
                    }

                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        return null;
                    }
                    s = s.ToLower();

                    if (!Scenarios.MissionStrings.Contains(s))
                    {
                        // Invalid token
                        Log.InvalidToken(LogCategory, token, lexer);
                        continue;
                    }

                    // Type of mission
                    mission.Type = (MissionType) Array.IndexOf(Scenarios.MissionStrings, s);
                    continue;
                }

                // target target
                if (keyword.Equals("target"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "target", lexer);
                        continue;
                    }

                    // Target Providence
                    mission.Target = (int) n;
                    continue;
                }

                // missionscope
                if (keyword.Equals("missionscope"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "missionscope", lexer);
                        continue;
                    }

                    // Coverage
                    mission.MissionScope = (int) n;
                    continue;
                }

                // percentage
                if (keyword.Equals("percentage"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "percentage", lexer);
                        continue;
                    }

                    // Strength / / Command and control rate lower limit
                    mission.Percentage = (double) d;
                    continue;
                }

                // night night
                if (keyword.Equals("night"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "night", lexer);
                        continue;
                    }

                    // Perform at night
                    mission.Night = (bool) b;
                    continue;
                }

                // day
                if (keyword.Equals("day"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "day", lexer);
                        continue;
                    }

                    // Daytime execution
                    mission.Day = (bool) b;
                    continue;
                }

                // tz
                if (keyword.Equals("tz"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "tz", lexer);
                        continue;
                    }

                    // Coverage
                    mission.TargetZone = (int) n;
                    continue;
                }

                // ac
                if (keyword.Equals("ac"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "ac", lexer);
                        continue;
                    }

                    // Fleet attack
                    mission.AttackConvoy = (bool) b;
                    continue;
                }

                // org
                if (keyword.Equals("org"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "org", lexer);
                        continue;
                    }

                    // Lower limit of organization rate
                    mission.OrgLimit = (double) d;
                    continue;
                }

                // startdate
                if (keyword.Equals("startdate"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "startdate", lexer);
                        continue;
                    }

                    // Start date and time
                    mission.StartDate = date;
                    continue;
                }

                // enddate
                if (keyword.Equals("enddate"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "enddate", lexer);
                        continue;
                    }

                    // End date and time
                    mission.EndDate = date;
                    continue;
                }

                // task task
                if (keyword.Equals("task"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "task", lexer);
                        continue;
                    }

                    // mission
                    mission.Task = (int) n;
                    continue;
                }

                // location
                if (keyword.Equals("location"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "location", lexer);
                        continue;
                    }

                    // position
                    mission.Location = (int) n;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return mission;
        }

        #endregion

        #region Transport fleet

        /// <summary>
        ///     Syntactically analyze the convoy
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Convoy</returns>
        private static Convoy ParseConvoy(TextLexer lexer)
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

            Convoy convoy = new Convoy();
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

                // id id
                if (keyword.Equals("id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "id", lexer);
                        continue;
                    }

                    // type When id id Pair of
                    convoy.Id = id;
                    continue;
                }

                // trade_id
                if (keyword.Equals("trade_id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "trade_id", lexer);
                        continue;
                    }

                    // Trade ID
                    convoy.TradeId = id;
                    continue;
                }

                // istradeconvoy
                if (keyword.Equals("istradeconvoy"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "istradeconvoy", lexer);
                        continue;
                    }

                    // Whether it is a convoy for trade
                    convoy.IsTrade = (bool) b;
                }

                // transports
                if (keyword.Equals("transports"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "transports", lexer);
                        continue;
                    }

                    // Number of transport ships
                    convoy.Transports = (int) n;
                    continue;
                }

                // escorts
                if (keyword.Equals("escorts"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "escorts", lexer);
                        continue;
                    }

                    // Number of escort vessels
                    convoy.Escorts = (int) n;
                    continue;
                }

                // energy
                if (keyword.Equals("energy"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "energy", lexer);
                        continue;
                    }

                    // Whether or not energy is transported
                    convoy.Energy = (bool) b;
                    continue;
                }

                // metal
                if (keyword.Equals("metal"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "metal", lexer);
                        continue;
                    }

                    // Whether metal is transported
                    convoy.Metal = (bool) b;
                    continue;
                }

                // rare_materials
                if (keyword.Equals("rare_materials"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "rare_materials", lexer);
                        continue;
                    }

                    // Whether or not rare resources are transported
                    convoy.RareMaterials = (bool) b;
                    continue;
                }

                // oil
                if (keyword.Equals("oil"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "rare_materials", lexer);
                        continue;
                    }

                    // With or without oil transportation
                    convoy.Oil = (bool) b;
                    continue;
                }

                // supplies supplies
                if (keyword.Equals("supplies"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "rare_materials", lexer);
                        continue;
                    }

                    // Whether or not goods are transported
                    convoy.Supplies = (bool) b;
                    continue;
                }

                // path
                if (keyword.Equals("path"))
                {
                    IEnumerable<int> list = ParseIdList(lexer);
                    if (list == null)
                    {
                        Log.InvalidSection(LogCategory, "list", lexer);
                        continue;
                    }

                    // sea route
                    convoy.Path.AddRange(list);
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return convoy;
        }

        /// <summary>
        ///     Syntactically analyze the in-production transport fleet
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Transport fleet in production</returns>
        private static ConvoyDevelopment ParseConvoyDevelopment(TextLexer lexer)
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

            ConvoyDevelopment convoy = new ConvoyDevelopment();
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

                // id id
                if (keyword.Equals("id"))
                {
                    TypeId id = ParseTypeId(lexer);
                    if (id == null)
                    {
                        Log.InvalidSection(LogCategory, "id", lexer);
                        continue;
                    }

                    // type When id id Pair of
                    convoy.Id = id;
                    continue;
                }

                // name
                if (keyword.Equals("name"))
                {
                    string s = ParseString(lexer);
                    if (s == null)
                    {
                        Log.InvalidClause(LogCategory, "name", lexer);
                        continue;
                    }

                    // name
                    convoy.Name = s;
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

                    token = lexer.GetToken();
                    if (token.Type == TokenType.Identifier)
                    {
                        // Invalid token
                        string s = token.Value as string;
                        if (string.IsNullOrEmpty(s))
                        {
                            continue;
                        }
                        s = s.ToLower();

                        // transports
                        if (s.Equals("transports"))
                        {
                            // Transport ship
                            convoy.Type = ConvoyType.Transports;
                            continue;
                        }

                        if (s.Equals("escorts"))
                        {
                            // Escort ship
                            convoy.Type = ConvoyType.Escorts;
                            continue;
                        }
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                // location
                if (keyword.Equals("location"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "location", lexer);
                        continue;
                    }

                    // position
                    convoy.Location = (int) n;
                    continue;
                }

                // cost
                if (keyword.Equals("cost"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "cost", lexer);
                        continue;
                    }

                    // requirement I C
                    convoy.Cost = (double) d;
                    continue;
                }

                // manpower
                if (keyword.Equals("manpower"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "manpower", lexer);
                        continue;
                    }

                    // Necessary human resources
                    convoy.Manpower = (double) d;
                    continue;
                }

                // date
                if (keyword.Equals("date"))
                {
                    GameDate date = ParseDate(lexer);
                    if (date == null)
                    {
                        Log.InvalidSection(LogCategory, "date", lexer);
                        continue;
                    }

                    // Completion date
                    convoy.Date = date;
                    continue;
                }

                // progress
                if (keyword.Equals("progress"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "progress", lexer);
                        continue;
                    }

                    // Progress rate increment
                    convoy.Progress = (double) d;
                    continue;
                }

                // total_progress
                if (keyword.Equals("total_progress"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "total_progress", lexer);
                        continue;
                    }

                    // Total progress rate
                    convoy.TotalProgress = (double) d;
                    continue;
                }

                // gearing_bonus
                if (keyword.Equals("gearing_bonus"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "gearing_bonus", lexer);
                        continue;
                    }

                    // Continuous production bonus
                    convoy.GearingBonus = (double) d;
                    continue;
                }

                // size
                if (keyword.Equals("size"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "size", lexer);
                        continue;
                    }

                    // Total production number
                    convoy.Size = (int) n;
                    continue;
                }

                // done done
                if (keyword.Equals("done"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "done", lexer);
                        continue;
                    }

                    // Number of completed production
                    convoy.Done = (int) n;
                    continue;
                }

                // days
                if (keyword.Equals("days"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "days", lexer);
                        continue;
                    }

                    // Days to complete
                    convoy.Days = (int) n;
                    continue;
                }

                // days_for_first
                if (keyword.Equals("days_for_first"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "days_for_first", lexer);
                        continue;
                    }

                    // 1 Number of days to complete the unit
                    convoy.DaysForFirst = (int) n;
                    continue;
                }

                // halted
                if (keyword.Equals("halted"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "halted", lexer);
                        continue;
                    }

                    // Stopping
                    convoy.Halted = (bool) b;
                    continue;
                }

                // close_when_finished
                if (keyword.Equals("close_when_finished"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "close_when_finished", lexer);
                        continue;
                    }

                    // Whether to delete the queue on completion
                    convoy.CloseWhenFinished = (bool) b;
                    continue;
                }

                // waiting for closure
                if (keyword.Equals("waitingforclosure"))
                {
                    bool? b = ParseBool(lexer);
                    if (!b.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "waitingforclosure", lexer);
                        continue;
                    }

                    // waitingforclosure (waitforclosure ( details unknown )
                    convoy.WaitingForClosure = (bool) b;
                    continue;
                }

                // retooling_time
                if (keyword.Equals("retooling_time"))
                {
                    double? d = ParseDouble(lexer);
                    if (!d.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "retooling_time", lexer);
                        continue;
                    }

                    // Production line preparation time
                    convoy.RetoolingTime = (double) d;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return convoy;
        }

        #endregion

        #region General purpose

        /// <summary>
        ///     ID Parse the list
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>ID list</returns>
        private static IEnumerable<int> ParseIdList(TextLexer lexer)
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
            int lastLineNo = -1;
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
                    if (lexer.LineNo != lastLineNo)
                    {
                        // If the current line is different from the last interpreted line, it is considered that the closing parenthesis is missing.
                        lexer.ReserveToken(token);
                        break;
                    }
                    lexer.SkipLine();
                    continue;
                }

                // ID
                list.Add((int) (double) token.Value);

                // Remember the final interpretation line
                lastLineNo = lexer.LineNo;
            }

            return list;
        }

        /// <summary>
        ///     Parsing the national list
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>National list</returns>
        private static IEnumerable<Country> ParseCountryList(TextLexer lexer)
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

            List<Country> list = new List<Country>();
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

                string name = token.Value as string;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                name = name.ToUpper();

                if (!Countries.StringMap.ContainsKey(name))
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                Country tag = Countries.StringMap[name];
                if (!Countries.Tags.Contains(tag))
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                // Country tag
                list.Add(tag);
            }

            return list;
        }

        /// <summary>
        ///     Parse the date and time
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Date and time</returns>
        private static GameDate ParseDate(TextLexer lexer)
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

            GameDate date = new GameDate();
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

                // year year
                if (keyword.Equals("year"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "year", lexer);
                        continue;
                    }

                    // Year
                    date.Year = (int) n;
                    continue;
                }

                // month
                if (keyword.Equals("month"))
                {
                    // = =
                    token = lexer.GetToken();
                    if (token.Type != TokenType.Equal)
                    {
                        Log.InvalidToken(LogCategory, token, lexer);
                        lexer.SkipLine();
                        continue;
                    }

                    token = lexer.GetToken();
                    if (token.Type == TokenType.Number)
                    {
                        // Invalid token
                        int month = (int) (double) token.Value;
                        if (month < 0 || month >= 12)
                        {
                            Log.OutOfRange(LogCategory, "month", month, lexer);
                        }

                        // Moon
                        date.Month = month + 1;
                        continue;
                    }

                    if (token.Type == TokenType.Identifier)
                    {
                        // Invalid token
                        string name = token.Value as string;
                        if (string.IsNullOrEmpty(name))
                        {
                            continue;
                        }
                        name = name.ToLower();

                        if (!Scenarios.MonthStrings.Contains(name))
                        {
                            Log.InvalidToken(LogCategory, token, lexer);
                            continue;
                        }

                        int month = Array.IndexOf(Scenarios.MonthStrings, name);
                        if (month < 0 || month >= 12)
                        {
                            Log.OutOfRange(LogCategory, "month", month, lexer);
                            continue;
                        }

                        // Moon
                        date.Month = month + 1;
                        continue;
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                // day
                if (keyword.Equals("day"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "day", lexer);
                        continue;
                    }

                    // 30 Because there are many descriptions of the date [ information ]Output an error at the level
                    if (n == 30)
                    {
                        Log.Info("[Scenario] Out of range: {0} at day ({1} L{2})", n, lexer.FileName, lexer.LineNo);
                    }
                    else if (n < 0 || n > 30)
                    {
                        Log.OutOfRange(LogCategory, "day", n, lexer);
                    }

                    // Day
                    date.Day = (int) n + 1;
                    continue;
                }

                // hour
                if (keyword.Equals("hour"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "day", lexer);
                        continue;
                    }

                    if (n < 0 || n >= 24)
                    {
                        Log.OutOfRange(LogCategory, "hour", n, lexer);
                    }

                    // Time
                    date.Hour = (int) n;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return date;
        }

        /// <summary>
        ///     type When id Parsing a pair of
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>type When id id Pair of</returns>
        private static TypeId ParseTypeId(TextLexer lexer)
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

            TypeId id = new TypeId();
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

                // type
                if (keyword.Equals("type"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "type", lexer);
                        continue;
                    }

                    // type
                    id.Type = (int) n;
                    continue;
                }

                // id id
                if (keyword.Equals("id"))
                {
                    int? n = ParseInt(lexer);
                    if (!n.HasValue)
                    {
                        Log.InvalidClause(LogCategory, "id", lexer);
                        continue;
                    }

                    // id id
                    id.Id = (int) n;
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return id;
        }

        /// <summary>
        ///     Parse the flag list
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Flag list</returns>
        private static Dictionary<string, string> ParseFlags(TextLexer lexer)
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

            Dictionary<string, string> flags = new Dictionary<string, string>();
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
                if (token.Type != TokenType.Identifier && token.Type != TokenType.Number)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    lexer.SkipLine();
                    continue;
                }

                string keyword = ObjectHelper.ToString(token.Value);
                if (string.IsNullOrEmpty(keyword))
                {
                    continue;
                }

                // = =
                token = lexer.GetToken();
                if (token.Type != TokenType.Equal)
                {
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                token = lexer.GetToken();
                if (token.Type == TokenType.Number)
                {
                    int n = (int) (double) token.Value;
                    if (n == 0 || n == 1)
                    {
                        flags[keyword] = ObjectHelper.ToString(token.Value);
                        continue;
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                if (token.Type == TokenType.Identifier)
                {
                    string s = token.Value as string;
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                    s = s.ToLower();

                    if (s.Equals("yes") || s.Equals("no"))
                    {
                        flags[keyword] = ObjectHelper.ToString(token.Value);
                        continue;
                    }

                    // Invalid token
                    Log.InvalidToken(LogCategory, token, lexer);
                    continue;
                }

                // Invalid token
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
            }

            return flags;
        }

        #endregion

        #region value

        /// <summary>
        ///     Parse integer values
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Integer value</returns>
        private static int? ParseInt(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
                return null;
            }

            // Invalid token
            token = lexer.GetToken();
            if (token.Type != TokenType.Number)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            return (int) (double) token.Value;
        }

        /// <summary>
        ///     Parsing real numbers
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Real number</returns>
        private static double? ParseDouble(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
                return null;
            }

            // Invalid token
            token = lexer.GetToken();
            if (token.Type != TokenType.Number)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            return (double) token.Value;
        }

        /// <summary>
        ///     Parse string values
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>String value</returns>
        private static string ParseString(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
                return null;
            }

            // Invalid token
            token = lexer.GetToken();
            if (token.Type != TokenType.String)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            return token.Value as string;
        }

        /// <summary>
        ///     Parse the identifier value
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>String value</returns>
        private static string ParseIdentifier(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
                return null;
            }

            // Invalid token
            token = lexer.GetToken();
            if (token.Type != TokenType.Identifier)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            return token.Value as string;
        }

        /// <summary>
        ///     Parse a string or identifier value
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>String value</returns>
        private static string ParseStringOrIdentifier(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
                return null;
            }

            // Invalid token
            token = lexer.GetToken();
            if (token.Type != TokenType.String && token.Type != TokenType.Identifier)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            return token.Value as string;
        }

        /// <summary>
        ///     Parse Boolean values
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Boolean value</returns>
        private static bool? ParseBool(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
                return null;
            }

            // Invalid token
            token = lexer.GetToken();
            if (token.Type == TokenType.Identifier)
            {
                string s = token.Value as string;
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                s = s.ToLower();

                // yes
                if (s.Equals("yes"))
                {
                    return true;
                }

                // no
                if (s.Equals("no"))
                {
                    return false;
                }
            }

            else if (token.Type == TokenType.Number)
            {
                int n = (int) (double) token.Value;

                if (n == 1)
                {
                    return true;
                }

                if (n == 0)
                {
                    return false;
                }
            }

            // Invalid token
            Log.InvalidToken(LogCategory, token, lexer);
            return null;
        }

        /// <summary>
        ///     Parse the country tag
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Country tag</returns>
        private static Country? ParseTag(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
                return null;
            }

            // Invalid token
            token = lexer.GetToken();
            if (token.Type != TokenType.String && token.Type != TokenType.Identifier)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            string name = token.Value as string;
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            name = name.ToUpper();

            if (!Countries.StringMap.ContainsKey(name))
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            Country tag = Countries.StringMap[name];
            if (!Countries.Tags.Contains(tag))
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            return tag;
        }

        /// <summary>
        ///     Parsing division unit types
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Unit type</returns>
        private static UnitType? ParseDivisionType(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
                return null;
            }

            // Invalid token
            token = lexer.GetToken();
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

            // Other than the unit class name
            if (!Units.StringMap.ContainsKey(s))
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            // Unsupported unit type
            UnitType type = Units.StringMap[s];
            if (!Units.DivisionTypes.Contains(type))
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            return type;
        }

        /// <summary>
        ///     Syntactically analyze the unit type of the brigade
        /// </summary>
        /// <param name="lexer">Lexical analyzer</param>
        /// <returns>Unit type</returns>
        private static UnitType? ParseBrigadeType(TextLexer lexer)
        {
            // = =
            Token token = lexer.GetToken();
            if (token.Type != TokenType.Equal)
            {
                Log.InvalidToken(LogCategory, token, lexer);
                lexer.SkipLine();
                return null;
            }

            // Invalid token
            token = lexer.GetToken();
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

            // Other than the unit class name
            if (!Units.StringMap.ContainsKey(s))
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            // Unsupported unit type
            UnitType type = Units.StringMap[s];
            if (!Units.BrigadeTypes.Contains(type))
            {
                Log.InvalidToken(LogCategory, token, lexer);
                return null;
            }

            return type;
        }

        #endregion

        #endregion

        #region File discrimination

        /// <summary>
        ///     Scenario file type
        /// </summary>
        public enum ScenarioFileKind
        {
            Normal, // others
            Top, // Top-level file
            BasesInc, // bases.inc
            BasesDodInc, // bases_DOD.inc
            DepotsInc, // depots.inc
            VpInc // vp.inc
        }

        /// <summary>
        ///     Get the type of scenario file being analyzed
        /// </summary>
        /// <returns>Scenario file type</returns>
        private static ScenarioFileKind GetScenarioFileKind()
        {
            // The directory name isscenarios Is regarded as the top-level file
            string dirName = Path.GetDirectoryName(_fileName);
            string parentName = Path.GetFileName(dirName);
            if (string.IsNullOrEmpty(parentName))
            {
                return ScenarioFileKind.Normal;
            }
            if (parentName.Equals("scenarios"))
            {
                return ScenarioFileKind.Top;
            }

            string name = Path.GetFileName(_fileName);
            if (string.IsNullOrEmpty(name))
            {
                return ScenarioFileKind.Normal;
            }
            name = name.ToLower();

            // bases.inc
            if (name.Equals("bases.inc"))
            {
                return ScenarioFileKind.BasesInc;
            }

            // bases_DOD.inc
            if (name.Equals("bases_dod.inc"))
            {
                return ScenarioFileKind.BasesDodInc;
            }

            // depots.inc
            if (name.Equals("depots.inc"))
            {
                return ScenarioFileKind.DepotsInc;
            }

            // vp.inc
            if (name.Equals("vp.inc"))
            {
                return ScenarioFileKind.VpInc;
            }

            return ScenarioFileKind.Normal;
        }

        #endregion
    }
}
