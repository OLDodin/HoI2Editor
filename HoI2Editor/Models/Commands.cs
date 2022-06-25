﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Command data management class
    /// </summary>
    public static class Commands
    {
        #region Public properties

        /// <summary>
        ///     Correspondence between command string and command type
        /// </summary>
        public static Dictionary<string, CommandType> StringMap = new Dictionary<string, CommandType>();

        /// <summary>
        ///     Available command types
        /// </summary>
        public static List<CommandType> Types = new List<CommandType>();

        #endregion

        #region Internal field

        /// <summary>
        ///     Initialized flag
        /// </summary>
        private static bool _initialized;

        /// <summary>
        ///     Game type
        /// </summary>
        private static CommandGameType _gameType;

        #endregion

        #region Public constant

        /// <summary>
        ///     Command string
        /// </summary>
        public static readonly string[] Strings =
        {
            "",
            "endgame",
            "setflag",
            "clrflag",
            "local_setflag",
            "local_clrflag",
            "regime_falls",
            "inherit",
            "country",
            "addcore",
            "removecore",
            "secedeprovince",
            "control",
            "capital",
            "civil_war",
            "change_idea",
            "belligerence",
            "dissent",
            "province_revoltrisk",
            "domestic",
            "set_domestic",
            "change_policy",
            "change_partisan_activity",
            "set_partisan_activity",
            "change_unit_xp",
            "set_unit_xp",
            "change_leader_xp",
            "set_leader_xp",
            "change_retool_time",
            "set_retool_time",
            "independence",
            "alliance",
            "leave_alliance",
            "relation",
            "set_relation",
            "peace",
            "peace_with_all",
            "war",
            "end_puppet",
            "end_mastery",
            "make_puppet",
            "coup_nation",
            "access",
            "end_access",
            "access_to_alliance",
            "end_non_aggression",
            "non_aggression",
            "end_trades",
            "end_guarantee",
            "guarantee",
            "grant_military_control",
            "end_military_control",
            "add_team_skill",
            "set_team_skill",
            "add_team_research_type",
            "remove_team_research_type",
            "sleepteam",
            "waketeam",
            "giveteam",
            "sleepminister",
            "sleepleader",
            "wakeleader",
            "giveleader",
            "set_leader_skill",
            "add_leader_skill",
            "add_leader_trait",
            "remove_leader_trait",
            "allow_dig_in",
            "build_division",
            "add_corps",
            "activate_division",
            "add_division",
            "remove_division",
            "damage_division",
            "disorg_division",
            "delete_unit",
            "switch_allegiance",
            "scrap_model",
            "lock_division",
            "unlock_division",
            "new_model",
            "activate_unit_type",
            "deactivate_unit_type",
            "carrier_level",
            "research_sabotaged",
            "deactivate",
            "activate",
            "info_may_cause",
            "steal_tech",
            "gain_tech",
            "resource",
            "supplies",
            "oilpool",
            "metalpool",
            "energypool",
            "rarematerialspool",
            "money",
            "manpowerpool",
            "relative_manpower",
            "province_manpower",
            "free_ic",
            "free_oil",
            "free_supplies",
            "free_money",
            "free_metal",
            "free_energy",
            "free_rare_materials",
            "free_transport",
            "free_escort",
            "free_manpower",
            "add_prov_resource",
            "allow_building",
            "construct",
            "allow_convoy_escorts",
            "transport_pool",
            "escort_pool",
            "convoy",
            "peacetime_ic_mod",
            "tc_mod",
            "tc_occupied_mod",
            "attrition_mod",
            "supply_dist_mod",
            "repair_mod",
            "research_mod",
            "building_prod_mod",
            "convoy_prod_mod",
            "radar_eff",
            "enable_task",
            "task_efficiency",
            "max_positioning",
            "min_positioning",
            "province_keypoints",
            "ai",
            "extra_tc",
            "vp",
            "songs",
            "trigger",
            "sleepevent",
            "max_reactor_size",
            "abomb_production",
            "double_nuke_prod",
            "nuke_damage",
            "gas_attack",
            "gas_protection",
            "revolt",
            "ai_prepare_war",
            "start_pattern",
            "add_to_pattern",
            "end_pattern",
            "set_ground",
            "counterattack",
            "assault",
            "encirclement",
            "ambush",
            "delay",
            "tactical_withdrawal",
            "breakthrough",
            "hq_supply_eff",
            "sce_frequency",
            "nuclear_carrier",
            "missile_carrier",
            "out_of_fuel_speed",
            "no_fuel_combat_mod",
            "no_supplies_combat_mod",
            "soft_attack",
            "hard_attack",
            "defensiveness",
            "air_attack",
            "air_defense",
            "build_cost",
            "build_time",
            "manpower",
            "speed",
            "max_organization",
            "morale",
            "transport_weight",
            "supply_consumption",
            "fuel_consumption",
            "speed_cap_art",
            "speed_cap_eng",
            "speed_cap_at",
            "speed_cap_aa",
            "artillery_bombardment",
            "suppression",
            "softness",
            "toughness",
            "strategic_attack",
            "tactical_attack",
            "naval_attack",
            "surface_detection",
            "air_detection",
            "sub_detection",
            "transport_capacity",
            "range",
            "shore_attack",
            "naval_defense",
            "visibility",
            "sub_attack",
            "convoy_attack",
            "plain_attack",
            "plain_defense",
            "desert_attack",
            "desert_defense",
            "mountain_attack",
            "mountain_defense",
            "hill_attack",
            "hill_defense",
            "forest_attack",
            "forest_defense",
            "swamp_attack",
            "swamp_defense",
            "jungle_attack",
            "jungle_defense",
            "urban_attack",
            "urban_defense",
            "river_attack",
            "paradrop_attack",
            "fort_attack",
            "plain_move",
            "desert_move",
            "mountain_move",
            "hill_move",
            "forest_move",
            "swamp_move",
            "jungle_move",
            "urban_move",
            "river_crossing",
            "clear_attack",
            "clear_defense",
            "frozen_attack",
            "frozen_defense",
            "snow_attack",
            "snow_defense",
            "blizzard_attack",
            "blizzard_defense",
            "rain_attack",
            "rain_defense",
            "storm_attack",
            "storm_defense",
            "muddy_attack",
            "muddy_defense",
            "frozen_move",
            "snow_move",
            "blizzard_move",
            "rain_move",
            "storm_move",
            "muddy_move",
            "night_move",
            "night_attack",
            "night_defense",
            "minisub_bonus",
            "surprise",
            "intelligence",
            "army_detection",
            "aa_batteries",
            "industrial_multiplier",
            "industrial_modifier",
            "trickleback_mod",
            "max_amphib_mod",
            "building_eff_mod",
            "headofstate",
            "headofgovernment",
            "foreignminister",
            "armamentminister",
            "ministerofsecurity",
            "ministerofintelligence",
            "chiefofstaff",
            "chiefofarmy",
            "chiefofnavy",
            "chiefofair",
            "alliance_leader",
            "alliance_name",
            "stockpile",
            "auto_trade",
            "auto_trade_reset",
            "military_control",
            "flag_ext",
            "embargo",
            "name",
            "secedearea",
            "secederegion",
            "trade",
            "wartime_ic_mod",
            "addclaim",
            "removeclaim",
            "land_fort_eff",
            "coast_fort_eff",
            "convoy_def_eff",
            "ground_def_eff",
            "strength",
            "event",
            "wakeminister",
            "demobilize",
            "strength_cap",
            "remove_units"
        };

        #endregion

        #region Internal constant

        /// <summary>
        ///     Presence or absence of items for each game
        /// </summary>
        private static readonly bool[,] CommandTypeTable =
        {
            { false, false, false, false, false, false, false, false },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, true, true, false, false, false },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, true, true, false, false, false },
            { false, false, false, true, true, false, false, false },
            { false, false, false, true, true, false, false, false },
            { false, false, false, true, true, false, false, false },
            { false, false, false, true, true, false, false, false },
            { false, false, false, true, true, false, false, false },
            { false, false, false, true, true, false, false, false },
            { false, false, false, true, true, false, false, false },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, true, true, false, false, false },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, false, true, false, false, false },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, true, true, false, false, false },
            { false, false, false, true, true, false, false, false },
            { false, false, true, true, true, false, false, false },
            { false, false, true, true, true, false, false, false },
            { false, false, true, true, true, false, false, false },
            { false, false, true, true, true, false, false, false },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false },
            { true, true, true, true, true, true, true, true },
            { false, false, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false },
            { false, false, true, true, true, false, false, false },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, false, false, false, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true }
        };

        #endregion

        #region Initialization

        /// <summary>
        ///     Initialization process
        /// </summary>
        public static void Init()
        {
            CommandGameType gameType = GetGameType();

            // Do nothing if it is initialized and there is no change in the game type
            if (_initialized && (gameType == _gameType))
            {
                return;
            }

            _gameType = gameType;

            // Initialize command type
            InitTypes();

            // Set the initialized flag
            _initialized = true;
        }

        /// <summary>
        ///     Initialize command type
        /// </summary>
        private static void InitTypes()
        {
            Types.Clear();
            StringMap.Clear();
            foreach (CommandType type in Enum.GetValues(typeof (CommandType))
                .Cast<CommandType>()
                .Where(type => CommandTypeTable[(int) type, (int) _gameType]))
            {
                Types.Add(type);
                StringMap.Add(Strings[(int) type], type);
            }
        }

        #endregion

        #region Game version

        /// <summary>
        ///     Get the game type of the command
        /// </summary>
        /// <returns>Command game type</returns>
        public static CommandGameType GetGameType()
        {
            switch (Game.Type)
            {
                case GameType.HeartsOfIron2:
                    return Game.Version >= 130 ? CommandGameType.Dda13 : CommandGameType.Dda12;

                case GameType.ArsenalOfDemocracy:
                    return Game.Version >= 108
                        ? CommandGameType.Aod108
                        : (Game.Version <= 104 ? CommandGameType.Aod104 : CommandGameType.Aod107);

                case GameType.DarkestHour:
                    return Game.Version >= 104
                        ? CommandGameType.Dh104
                        : Game.Version >= 103 ? CommandGameType.Dh103 : CommandGameType.Dh102;
            }
            return CommandGameType.Dda12;
        }

        #endregion
    }

    /// <summary>
    ///     Command game type
    /// </summary>
    public enum CommandGameType
    {
        Dda12, // DDA1.2
        Dda13, // DDA1.3
        Aod104, // AoD1.04
        Aod107, // AoD1.07
        Aod108, // AoD1.08
        Dh102, // DH1.02
        Dh103, // DH1.03
        Dh104 // DH1.04
    }
}
