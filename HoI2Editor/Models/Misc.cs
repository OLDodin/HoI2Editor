using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using HoI2Editor.Parsers;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;
using HoI2Editor.Writers;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     misc File setting items
    /// </summary>
    public static class Misc
    {
        #region Public properties

        #region economy section

        /// <summary>
        ///     Maximum number of equipment attached to the transport ship
        /// </summary>
        public static int TpMaxAttach
        {
            get { return (int?) _items[(int) MiscItemId.TpMaxAttach] ?? 0; }
            set
            {
                _items[(int) MiscItemId.TpMaxAttach] = value;
                SetDirty(MiscItemId.TpMaxAttach);
            }
        }

        /// <summary>
        ///     Maximum number of submarines attached
        /// </summary>
        public static int SsMaxAttach
        {
            get { return (int?) _items[(int) MiscItemId.SsMaxAttach] ?? 0; }
            set
            {
                _items[(int) MiscItemId.SsMaxAttach] = value;
                SetDirty(MiscItemId.SsMaxAttach);
            }
        }

        /// <summary>
        ///     Maximum number of equipment attached to nuclear submarines
        /// </summary>
        public static int SsnMaxAttach
        {
            get { return (int?) _items[(int) MiscItemId.SsnMaxAttach] ?? 0; }
            set
            {
                _items[(int) MiscItemId.SsnMaxAttach] = value;
                SetDirty(MiscItemId.SsnMaxAttach);
            }
        }

        /// <summary>
        ///     Maximum number of destroyer accessories
        /// </summary>
        public static int DdMaxAttach
        {
            get { return (int?) _items[(int) MiscItemId.DdMaxAttach] ?? 1; }
            set
            {
                _items[(int) MiscItemId.DdMaxAttach] = value;
                SetDirty(MiscItemId.DdMaxAttach);
            }
        }

        /// <summary>
        ///     Maximum number of equipment attached to light cruisers
        /// </summary>
        public static int ClMaxAttach
        {
            get { return (int?) _items[(int) MiscItemId.ClMaxAttach] ?? 2; }
            set
            {
                _items[(int) MiscItemId.ClMaxAttach] = value;
                SetDirty(MiscItemId.ClMaxAttach);
            }
        }

        /// <summary>
        ///     Maximum number of heavy cruisers attached
        /// </summary>
        public static int CaMaxAttach
        {
            get { return (int?) _items[(int) MiscItemId.CaMaxAttach] ?? 3; }
            set
            {
                _items[(int) MiscItemId.CaMaxAttach] = value;
                SetDirty(MiscItemId.CaMaxAttach);
            }
        }

        /// <summary>
        ///     Maximum number of equipment attached to cruise battleships
        /// </summary>
        public static int BcMaxAttach
        {
            get { return (int?) _items[(int) MiscItemId.BcMaxAttach] ?? 4; }
            set
            {
                _items[(int) MiscItemId.BcMaxAttach] = value;
                SetDirty(MiscItemId.BcMaxAttach);
            }
        }

        /// <summary>
        ///     Maximum number of attached equipment for battleships
        /// </summary>
        public static int BbMaxAttach
        {
            get { return (int?) _items[(int) MiscItemId.BbMaxAttach] ?? 5; }
            set
            {
                _items[(int) MiscItemId.BbMaxAttach] = value;
                SetDirty(MiscItemId.BbMaxAttach);
            }
        }

        /// <summary>
        ///     Maximum number of equipment attached to the light carrier
        /// </summary>
        public static int CvlMaxAttach
        {
            get { return (int?) _items[(int) MiscItemId.CvlMaxAttach] ?? 1; }
            set
            {
                _items[(int) MiscItemId.CvlMaxAttach] = value;
                SetDirty(MiscItemId.CvlMaxAttach);
            }
        }

        /// <summary>
        ///     Maximum number of aircraft carriers included
        /// </summary>
        public static int CvMaxAttach
        {
            get { return (int?) _items[(int) MiscItemId.CvMaxAttach] ?? 1; }
            set
            {
                _items[(int) MiscItemId.CvMaxAttach] = value;
                SetDirty(MiscItemId.CvMaxAttach);
            }
        }

        #endregion

        #region combat section

        /// <summary>
        ///     Combat mode
        /// </summary>
        /// <remarks>
        ///     false: false: Traditional format
        ///     true: DH1.03 New format thereafter
        /// </ remarks>
        public static bool CombatMode
        {
            get
            {
                return (_items[(int) MiscItemId.CombatMode] != null) && ((int) _items[(int) MiscItemId.CombatMode] != 0);
            }
            set
            {
                _items[(int) MiscItemId.CombatMode] = value;
                SetDirty(MiscItemId.CombatMode);
            }
        }

        #endregion

        #region research section

        /// <summary>
        ///     Blue Photo Bonus
        /// </summary>
        public static double BlueprintBonus => (double?) _items[(int) MiscItemId.BlueprintBonus] ?? 1;

        /// <summary>
        ///     Prehistoric year research penalty
        /// </summary>
        public static double PreHistoricalDateModifier
            => (double?) _items[(int) MiscItemId.PreHistoricalDateModifier] ?? 1;

        /// <summary>
        ///     Research bonus after historical year
        /// </summary>
        public static double PostHistoricalDateModifier
        {
            get
            {
                switch (Game.Type)
                {
                    case GameType.ArsenalOfDemocracy:
                        return (double?) _items[(int) MiscItemId.PostHistoricalDateModifierAoD] ?? 1;

                    case GameType.DarkestHour:
                        return (double?) _items[(int) MiscItemId.PostHistoricalDateModifierDh] ?? 1;
                }
                return 1;
            }
        }

        /// <summary>
        ///     Research bonus after historical year (AoD)
        /// </summary>
        public static double PostHistoricalDateModifierAoD
            => (double?) _items[(int) MiscItemId.PostHistoricalDateModifierAoD] ?? 1;

        /// <summary>
        ///     Research bonus after historical year (DH)
        /// </summary>
        public static double PostHistoricalDateModifierDh
            => (double?) _items[(int) MiscItemId.PostHistoricalDateModifierDh] ?? 1;

        /// <summary>
        ///     Research speed correction
        /// </summary>
        public static double TechSpeedModifier
        {
            get
            {
                if (Game.Type == GameType.ArsenalOfDemocracy)
                {
                    return (double?) _items[(int) MiscItemId.TechSpeedModifier] ?? 1;
                }
                return 1;
            }
        }

        /// <summary>
        ///     Upper limit of research penalty before historical year
        /// </summary>
        public static double PreHistoricalPenaltyLimit
        {
            get
            {
                if (Game.Type == GameType.ArsenalOfDemocracy)
                {
                    return (double?) _items[(int) MiscItemId.PreHistoricalPenaltyLimit] ?? 1;
                }
                return 1;
            }
        }

        /// <summary>
        ///     Research bonus limit after historical year
        /// </summary>
        public static double PostHistoricalBonusLimit
        {
            get
            {
                if (Game.Type == GameType.ArsenalOfDemocracy)
                {
                    return (double?) _items[(int) MiscItemId.PostHistoricalBonusLimit] ?? 1;
                }
                return 1;
            }
        }

        #endregion

        #region mod mod section

        /// <summary>
        ///     New format ministerial file format
        /// </summary>
        public static bool UseNewMinisterFilesFormat
        {
            get
            {
                return (_items[(int) MiscItemId.UseNewMinisterFilesFormat] != null) &&
                       (bool) _items[(int) MiscItemId.UseNewMinisterFilesFormat];
            }
            set
            {
                _items[(int) MiscItemId.UseNewMinisterFilesFormat] = value;
                SetDirty(MiscItemId.UseNewMinisterFilesFormat);
            }
        }

        /// <summary>
        ///     Use ministerial retirement year
        /// </summary>
        public static bool EnableRetirementYearMinisters
        {
            get
            {
                return (_items[(int) MiscItemId.EnableRetirementYearMinisters] != null) &&
                       (bool) _items[(int) MiscItemId.EnableRetirementYearMinisters];
            }
            set
            {
                _items[(int) MiscItemId.EnableRetirementYearMinisters] = value;
                SetDirty(MiscItemId.EnableRetirementYearMinisters);
            }
        }

        /// <summary>
        ///     Use commander retirement year
        /// </summary>
        public static bool EnableRetirementYearLeaders
        {
            get
            {
                return (_items[(int) MiscItemId.EnableRetirementYearLeaders] != null) &&
                       (bool) _items[(int) MiscItemId.EnableRetirementYearLeaders];
            }
            set
            {
                _items[(int) MiscItemId.EnableRetirementYearLeaders] = value;
                SetDirty(MiscItemId.EnableRetirementYearLeaders);
            }
        }

        #endregion

        #region map section

        /// <summary>
        ///     Map number
        /// </summary>
        public static int MapNumber => (int?) _items[(int) MiscItemId.MapNumber] ?? 0;

        #endregion

        #endregion

        #region Internal field

        /// <summary>
        ///     Item value
        /// </summary>
        private static object[] _items = new object[Enum.GetValues(typeof (MiscItemId)).Length];

        /// <summary>
        ///     Loaded flag
        /// </summary>
        private static bool _loaded;

        /// <summary>
        ///     Edited flag
        /// </summary>
        private static bool _dirtyFlag;

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private static readonly bool[] DirtyFlags = new bool[Enum.GetValues(typeof (MiscItemId)).Length];

        /// <summary>
        ///     Item comment
        /// </summary>
        private static string[] _comments;

        /// <summary>
        ///     String at the end of the section
        /// </summary>
        private static string[] _suffixes;

        #endregion

        #region Public constant

        /// <summary>
        ///     Section string
        /// </summary>
        public static readonly string[] SectionStrings =
        {
            "economy",
            "intelligence",
            "diplomacy",
            "combat",
            "mission",
            "country",
            "research",
            "trade",
            "ai",
            "mod",
            "map"
        };

        /// <summary>
        ///     Presence or absence of sections for each game
        /// </summary>
        public static bool[,] SectionTable =
        {
            {true,true,true,true,true,true,true,true,true}, // Economy
            {false,false,false,false,false,true,true,true,true}, // Intelligence
            {false,false,false,false,false,true,true,true,true}, // Diplomatic
            {true,true,true,true,true,true,true,true,true}, // fight
            {false,false,false,false,false,true,true,true,true}, // mission
            {false,false,false,false,false,true,true,true,true}, // Nation
            {true,true,true,true,true,true,true,true,true}, // the study
            {false,false,false,false,false,true,true,true,true}, // Trade
            {false,false,false,false,false,true,true,true,true}, // AI
            {false,false,false,false,false,true,true,true,true}, // MOD
            {false,false,false,false,false,true,true,true,true}, // map
        };

        /// <summary>
        ///     Items for each section
        /// </summary>
        public static MiscItemId[][] SectionItems =
        {
            new[]
            {
                MiscItemId.IcToTcRatio,
                MiscItemId.IcToSuppliesRatio,
                MiscItemId.IcToConsumerGoodsRatio,
                MiscItemId.IcToMoneyRatio,
                MiscItemId.DissentChangeSpeed,
                MiscItemId.MinAvailableIc,
                MiscItemId.MinFinalIc,
                MiscItemId.DissentReduction,
                MiscItemId.MaxGearingBonus,
                MiscItemId.GearingBonusIncrement,
                MiscItemId.GearingResourceIncrement,
                MiscItemId.GearingLossNoIc,
                MiscItemId.IcMultiplierNonNational,
                MiscItemId.IcMultiplierNonOwned,
                MiscItemId.IcMultiplierPuppet,
                MiscItemId.ResourceMultiplierNonNational,
                MiscItemId.ResourceMultiplierNonOwned,
                MiscItemId.ResourceMultiplierNonNationalAi,
                MiscItemId.ResourceMultiplierPuppet,
                MiscItemId.TcLoadUndeployedDivision,
                MiscItemId.TcLoadOccupied,
                MiscItemId.TcLoadMultiplierLand,
                MiscItemId.TcLoadMultiplierAir,
                MiscItemId.TcLoadMultiplierNaval,
                MiscItemId.TcLoadPartisan,
                MiscItemId.TcLoadFactorOffensive,
                MiscItemId.TcLoadProvinceDevelopment,
                MiscItemId.TcLoadBase,
                MiscItemId.ManpowerMultiplierNational,
                MiscItemId.ManpowerMultiplierNonNational,
                MiscItemId.ManpowerMultiplierColony,
                MiscItemId.ManpowerMultiplierPuppet,
                MiscItemId.ManpowerMultiplierWartimeOversea,
                MiscItemId.ManpowerMultiplierPeacetime,
                MiscItemId.ManpowerMultiplierWartime,
                MiscItemId.DailyRetiredManpower,
                MiscItemId.RequirementAffectSlider,
                MiscItemId.TrickleBackFactorManpower,
                MiscItemId.ReinforceManpower,
                MiscItemId.ReinforceCost,
                MiscItemId.ReinforceTime,
                MiscItemId.UpgradeCost,
                MiscItemId.UpgradeTime,
                MiscItemId.ReinforceToUpdateModifier,
                MiscItemId.SupplyToUpdateModifier,
                MiscItemId.NationalismStartingValue,
                MiscItemId.NationalismPerManpowerAoD,
                MiscItemId.NationalismPerManpowerDh,
                MiscItemId.MaxNationalism,
                MiscItemId.MaxRevoltRisk,
                MiscItemId.MonthlyNationalismReduction,
                MiscItemId.SendDivisionDays,
                MiscItemId.TcLoadUndeployedBrigade,
                MiscItemId.CanUnitSendNonAllied,
                MiscItemId.SpyMissionDays,
                MiscItemId.IncreateIntelligenceLevelDays,
                MiscItemId.ChanceDetectSpyMission,
                MiscItemId.RelationshipsHitDetectedMissions,
                MiscItemId.ShowThirdCountrySpyReports,
                MiscItemId.DistanceModifierNeighbours,
                MiscItemId.SpyInformationAccuracyModifier,
                MiscItemId.AiPeacetimeSpyMissions,
                MiscItemId.MaxIcCostModifier,
                MiscItemId.AiSpyMissionsCostModifier,
                MiscItemId.AiDiplomacyCostModifier,
                MiscItemId.AiInfluenceModifier,
                MiscItemId.CostRepairBuildings,
                MiscItemId.TimeRepairBuilding,
                MiscItemId.ProvinceEfficiencyRiseTime,
                MiscItemId.CoreProvinceEfficiencyRiseTime,
                MiscItemId.LineUpkeep,
                MiscItemId.LineStartupTime,
                MiscItemId.LineUpgradeTime,
                MiscItemId.RetoolingCost,
                MiscItemId.RetoolingResource,
                MiscItemId.DailyAgingManpower,
                MiscItemId.SupplyConvoyHunt,
                MiscItemId.SupplyNavalStaticAoD,
                MiscItemId.SupplyNavalMoving,
                MiscItemId.SupplyNavalBattleAoD,
                MiscItemId.SupplyAirStaticAoD,
                MiscItemId.SupplyAirMoving,
                MiscItemId.SupplyAirBattleAoD,
                MiscItemId.SupplyAirBombing,
                MiscItemId.SupplyLandStaticAoD,
                MiscItemId.SupplyLandMoving,
                MiscItemId.SupplyLandBattleAoD,
                MiscItemId.SupplyLandBombing,
                MiscItemId.SupplyStockLand,
                MiscItemId.SupplyStockAir,
                MiscItemId.SupplyStockNaval,
                MiscItemId.RestockSpeedLand,
                MiscItemId.RestockSpeedAir,
                MiscItemId.RestockSpeedNaval,
                MiscItemId.SyntheticOilConversionMultiplier,
                MiscItemId.SyntheticRaresConversionMultiplier,
                MiscItemId.MilitarySalary,
                MiscItemId.MaxIntelligenceExpenditure,
                MiscItemId.MaxResearchExpenditure,
                MiscItemId.MilitarySalaryAttrictionModifier,
                MiscItemId.MilitarySalaryDissentModifier,
                MiscItemId.NuclearSiteUpkeepCost,
                MiscItemId.NuclearPowerUpkeepCost,
                MiscItemId.SyntheticOilSiteUpkeepCost,
                MiscItemId.SyntheticRaresSiteUpkeepCost,
                MiscItemId.DurationDetection,
                MiscItemId.ConvoyProvinceHostileTime,
                MiscItemId.ConvoyProvinceBlockedTime,
                MiscItemId.AutoTradeConvoy,
                MiscItemId.SpyUpkeepCost,
                MiscItemId.SpyDetectionChance,
                MiscItemId.SpyCoupDissentModifier,
                MiscItemId.InfraEfficiencyModifier,
                MiscItemId.ManpowerToConsumerGoods,
                MiscItemId.TimeBetweenSliderChangesAoD,
                MiscItemId.MinimalPlacementIc,
                MiscItemId.NuclearPower,
                MiscItemId.FreeInfraRepair,
                MiscItemId.MaxSliderDissent,
                MiscItemId.MinSliderDissent,
                MiscItemId.MaxDissentSliderMove,
                MiscItemId.IcConcentrationBonus,
                MiscItemId.TransportConversion,
                MiscItemId.ConvoyDutyConversion,
                MiscItemId.EscortDutyConversion,
                MiscItemId.MinisterChangeDelay,
                MiscItemId.MinisterChangeEventDelay,
                MiscItemId.IdeaChangeDelay,
                MiscItemId.IdeaChangeEventDelay,
                MiscItemId.LeaderChangeDelay,
                MiscItemId.ChangeIdeaDissent,
                MiscItemId.ChangeMinisterDissent,
                MiscItemId.MinDissentRevolt,
                MiscItemId.DissentRevoltMultiplier,
                MiscItemId.TpMaxAttach,
                MiscItemId.SsMaxAttach,
                MiscItemId.SsnMaxAttach,
                MiscItemId.DdMaxAttach,
                MiscItemId.ClMaxAttach,
                MiscItemId.CaMaxAttach,
                MiscItemId.BcMaxAttach,
                MiscItemId.BbMaxAttach,
                MiscItemId.CvlMaxAttach,
                MiscItemId.CvMaxAttach,
                MiscItemId.CanChangeIdeas,
                MiscItemId.CanUnitSendNonAlliedDh,
                MiscItemId.BluePrintsCanSoldNonAllied,
                MiscItemId.ProvinceCanSoldNonAllied,
                MiscItemId.TransferAlliedCoreProvinces,
                MiscItemId.ProvinceBuildingsRepairModifier,
                MiscItemId.ProvinceResourceRepairModifier,
                MiscItemId.StockpileLimitMultiplierResource,
                MiscItemId.StockpileLimitMultiplierSuppliesOil,
                MiscItemId.OverStockpileLimitDailyLoss,
                MiscItemId.MaxResourceDepotSize,
                MiscItemId.MaxSuppliesOilDepotSize,
                MiscItemId.DesiredStockPilesSuppliesOil,
                MiscItemId.MaxManpower,
                MiscItemId.ConvoyTransportsCapacity,
                MiscItemId.SuppyLandStaticDh,
                MiscItemId.SupplyLandBattleDh,
                MiscItemId.FuelLandStatic,
                MiscItemId.FuelLandBattle,
                MiscItemId.SupplyAirStaticDh,
                MiscItemId.SupplyAirBattleDh,
                MiscItemId.FuelAirNavalStatic,
                MiscItemId.FuelAirBattle,
                MiscItemId.SupplyNavalStaticDh,
                MiscItemId.SupplyNavalBattleDh,
                MiscItemId.FuelNavalNotMoving,
                MiscItemId.FuelNavalBattle,
                MiscItemId.TpTransportsConversionRatio,
                MiscItemId.DdEscortsConversionRatio,
                MiscItemId.ClEscortsConversionRatio,
                MiscItemId.CvlEscortsConversionRatio,
                MiscItemId.ProductionLineEdit,
                MiscItemId.GearingBonusLossUpgradeUnit,
                MiscItemId.GearingBonusLossUpgradeBrigade,
                MiscItemId.DissentNukes,
                MiscItemId.MaxDailyDissent,
                MiscItemId.NukesProductionModifier,
                MiscItemId.ConvoySystemOptionsAllied,
                MiscItemId.ResourceConvoysBackUnneeded,
                MiscItemId.SupplyDistanceModCalculationModifier
            },
            new[]
            {
                MiscItemId.SpyMissionDaysDh,
                MiscItemId.IncreateIntelligenceLevelDaysDh,
                MiscItemId.ChanceDetectSpyMissionDh,
                MiscItemId.RelationshipsHitDetectedMissionsDh,
                MiscItemId.DistanceModifier,
                MiscItemId.DistanceModifierNeighboursDh,
                MiscItemId.SpyLevelBonusDistanceModifier,
                MiscItemId.SpyLevelBonusDistanceModifierAboveTen,
                MiscItemId.SpyInformationAccuracyModifierDh,
                MiscItemId.IcModifierCost,
                MiscItemId.MinIcCostModifier,
                MiscItemId.MaxIcCostModifierDh,
                MiscItemId.ExtraMaintenanceCostAboveTen,
                MiscItemId.ExtraCostIncreasingAboveTen,
                MiscItemId.ShowThirdCountrySpyReportsDh,
                MiscItemId.SpiesMoneyModifier
            },
            new[]
            {
                MiscItemId.DaysBetweenDiplomaticMissions,
                MiscItemId.TimeBetweenSliderChangesDh,
                MiscItemId.RequirementAffectSliderDh,
                MiscItemId.UseMinisterPersonalityReplacing,
                MiscItemId.RelationshipHitCancelTrade,
                MiscItemId.RelationshipHitCancelPermanentTrade,
                MiscItemId.PuppetsJoinMastersAlliance,
                MiscItemId.MastersBecomePuppetsPuppets,
                MiscItemId.AllowManualClaimsChange,
                MiscItemId.BelligerenceClaimedProvince,
                MiscItemId.BelligerenceClaimsRemoval,
                MiscItemId.JoinAutomaticallyAllesAxis,
                MiscItemId.AllowChangeHosHog,
                MiscItemId.ChangeTagCoup,
                MiscItemId.FilterReleaseCountries,
                MiscItemId.ReturnOccupiedProvinces
            },
            new[]
            {
                MiscItemId.LandXpGainFactor,
                MiscItemId.NavalXpGainFactor,
                MiscItemId.AirXpGainFactor,
                MiscItemId.AirDogfightXpGainFactor,
                MiscItemId.DivisionXpGainFactor,
                MiscItemId.LeaderXpGainFactor,
                MiscItemId.AttritionSeverityModifier,
                MiscItemId.NoSupplyAttritionSeverity,
                MiscItemId.NoSupplyMinimunAttrition,
                MiscItemId.OutOfSupplyAttritionLand,
                MiscItemId.OutOfSupplyAttritionNaval,
                MiscItemId.OutOfSupplyAttritionAir,
                MiscItemId.LowestStrAttritionLosses,
                MiscItemId.BaseProximity,
                MiscItemId.ShoreBombardmentModifier,
                MiscItemId.ShoreBombardmentCap,
                MiscItemId.InvasionModifier,
                MiscItemId.MultipleCombatModifier,
                MiscItemId.OffensiveCombinedArmsBonus,
                MiscItemId.DefensiveCombinedArmsBonus,
                MiscItemId.SurpriseModifier,
                MiscItemId.LandCommandLimitModifier,
                MiscItemId.AirCommandLimitModifier,
                MiscItemId.NavalCommandLimitModifier,
                MiscItemId.EnvelopmentModifier,
                MiscItemId.EncircledModifier,
                MiscItemId.LandFortMultiplier,
                MiscItemId.CoastalFortMultiplier,
                MiscItemId.HardUnitsAttackingUrbanPenalty,
                MiscItemId.DissentMultiplier,
                MiscItemId.SupplyProblemsModifier,
                MiscItemId.SupplyProblemsModifierLand,
                MiscItemId.SupplyProblemsModifierAir,
                MiscItemId.SupplyProblemsModifierNaval,
                MiscItemId.FuelProblemsModifierLand,
                MiscItemId.FuelProblemsModifierAir,
                MiscItemId.FuelProblemsModifierNaval,
                MiscItemId.RaderStationMultiplier,
                MiscItemId.RaderStationAaMultiplier,
                MiscItemId.InterceptorBomberModifier,
                MiscItemId.AirOverstackingModifier,
                MiscItemId.AirOverstackingModifierAoD,
                MiscItemId.NavalOverstackingModifier,
                MiscItemId.BombingEntrenchedArmiesModifier,
                MiscItemId.DefendingEntrenchedArmiesModifier,
                MiscItemId.LandLeaderCommandLimitRank0,
                MiscItemId.LandLeaderCommandLimitRank1,
                MiscItemId.LandLeaderCommandLimitRank2,
                MiscItemId.LandLeaderCommandLimitRank3,
                MiscItemId.AirLeaderCommandLimitRank0,
                MiscItemId.AirLeaderCommandLimitRank1,
                MiscItemId.AirLeaderCommandLimitRank2,
                MiscItemId.AirLeaderCommandLimitRank3,
                MiscItemId.NavalLeaderCommandLimitRank0,
                MiscItemId.NavalLeaderCommandLimitRank1,
                MiscItemId.NavalLeaderCommandLimitRank2,
                MiscItemId.NavalLeaderCommandLimitRank3,
                MiscItemId.HqCommandLimitFactor,
                MiscItemId.ConvoyProtectionFactor,
                MiscItemId.ConvoyEscortsModel,
                MiscItemId.ConvoyTransportsModel,
                MiscItemId.ChanceEscortCarrier,
                MiscItemId.ConvoyEscortCarrierModel,
                MiscItemId.DelayAfterCombatEnds,
                MiscItemId.LandDelayBeforeOrders,
                MiscItemId.NavalDelayBeforeOrders,
                MiscItemId.AirDelayBeforeOrders,
                MiscItemId.MaximumSizesAirStacks,
                MiscItemId.DurationAirToAirBattles,
                MiscItemId.DurationNavalPortBombing,
                MiscItemId.DurationStrategicBombing,
                MiscItemId.DurationGroundAttackBombing,
                MiscItemId.EffectExperienceCombat,
                MiscItemId.DamageNavalBasesBombing,
                MiscItemId.DamageAirBaseBombing,
                MiscItemId.DamageAaBombing,
                MiscItemId.DamageRocketBombing,
                MiscItemId.DamageNukeBombing,
                MiscItemId.DamageRadarBombing,
                MiscItemId.DamageInfraBombing,
                MiscItemId.DamageIcBombing,
                MiscItemId.DamageResourcesBombing,
                MiscItemId.DamageSyntheticOilBombing,
                MiscItemId.HowEffectiveGroundDef,
                MiscItemId.ChanceAvoidDefencesLeft,
                MiscItemId.ChanceAvoidNoDefences,
                MiscItemId.LandChanceAvoidDefencesLeft,
                MiscItemId.AirChanceAvoidDefencesLeft,
                MiscItemId.NavalChanceAvoidDefencesLeft,
                MiscItemId.ChanceAvoidAaDefencesLeft,
                MiscItemId.LandChanceAvoidNoDefences,
                MiscItemId.AirChanceAvoidNoDefences,
                MiscItemId.NavalChanceAvoidNoDefences,
                MiscItemId.ChanceAvoidAaNoDefences,
                MiscItemId.ChanceGetTerrainTrait,
                MiscItemId.ChanceGetEventTrait,
                MiscItemId.BonusTerrainTrait,
                MiscItemId.BonusSimilarTerrainTrait,
                MiscItemId.BonusEventTrait,
                MiscItemId.BonusLeaderSkillPointLand,
                MiscItemId.BonusLeaderSkillPointAir,
                MiscItemId.BonusLeaderSkillPointNaval,
                MiscItemId.ChanceLeaderDying,
                MiscItemId.AirOrgDamage,
                MiscItemId.AirStrDamageOrg,
                MiscItemId.AirStrDamage,
                MiscItemId.LandMinOrgDamage,
                MiscItemId.LandOrgDamageHardSoftEach,
                MiscItemId.LandOrgDamageHardVsSoft,
                MiscItemId.LandMinStrDamage,
                MiscItemId.LandStrDamageHardSoftEach,
                MiscItemId.LandStrDamageHardVsSoft,
                MiscItemId.AirMinOrgDamage,
                MiscItemId.AirAdditionalOrgDamage,
                MiscItemId.AirMinStrDamage,
                MiscItemId.AirAdditionalStrDamage,
                MiscItemId.AirStrDamageEntrenced,
                MiscItemId.NavalMinOrgDamage,
                MiscItemId.NavalAdditionalOrgDamage,
                MiscItemId.NavalMinStrDamage,
                MiscItemId.NavalAdditionalStrDamage,
                MiscItemId.AirOrgDamageLimitLand,
                MiscItemId.LandOrgDamageLimitAir,
                MiscItemId.AirOrgDamageLimitNavy,
                MiscItemId.NavalOrgDamageLimitAir,
                MiscItemId.AirOrgDamageLimitAa,
                MiscItemId.BasesOrgDamageHourCarriers,
                MiscItemId.BasesOrgDamageLimitCarriers,
                MiscItemId.BasesOrgDamageAfterCarriers,
                MiscItemId.BasesStrDamageCarriers,
                MiscItemId.AirCriticalHitChanceNavy,
                MiscItemId.AirCriticalHitModifierNavy,
                MiscItemId.NavalCriticalHitChanceNavy,
                MiscItemId.NavalCriticalHitModifierNavy,
                MiscItemId.AirStrDamageLandOrg,
                MiscItemId.AirStrDamageLandOrgDh104,
                MiscItemId.AirOrgDamageLandDh,
                MiscItemId.AirStrDamageLandDh,
                MiscItemId.LandOrgDamageLandOrg,
                MiscItemId.LandOrgDamageLandUrban,
                MiscItemId.LandOrgDamageLandFort,
                MiscItemId.RequiredLandFortSize,
                MiscItemId.LandStrDamageLandDh,
                MiscItemId.LandStrDamageLimitLand,
                MiscItemId.AirOrgDamageAirDh,
                MiscItemId.AirStrDamageAirDh,
                MiscItemId.LandOrgDamageAirDh,
                MiscItemId.LandStrDamageAirDh,
                MiscItemId.NavalOrgDamageAirDh,
                MiscItemId.NavalStrDamageAirDh,
                MiscItemId.SubsOrgDamageAir,
                MiscItemId.SubsStrDamageAir,
                MiscItemId.AirOrgDamageNavyDh,
                MiscItemId.AirStrDamageNavyDh,
                MiscItemId.NavalOrgDamageNavyDh,
                MiscItemId.NavalStrDamageNavyDh,
                MiscItemId.SubsOrgDamageNavy,
                MiscItemId.SubsStrDamageNavy,
                MiscItemId.SubsOrgDamage,
                MiscItemId.SubsStrDamage,
                MiscItemId.SubStacksDetectionModifier,
                MiscItemId.AirOrgDamageLandAoD,
                MiscItemId.AirStrDamageLandAoD,
                MiscItemId.LandDamageArtilleryBombardment,
                MiscItemId.InfraDamageArtilleryBombardment,
                MiscItemId.IcDamageArtilleryBombardment,
                MiscItemId.ResourcesDamageArtilleryBombardment,
                MiscItemId.PenaltyArtilleryBombardment,
                MiscItemId.ArtilleryStrDamage,
                MiscItemId.ArtilleryOrgDamage,
                MiscItemId.LandStrDamageLandAoD,
                MiscItemId.LandOrgDamageLand,
                MiscItemId.LandStrDamageAirAoD,
                MiscItemId.LandOrgDamageAirAoD,
                MiscItemId.NavalStrDamageAirAoD,
                MiscItemId.NavalOrgDamageAirAoD,
                MiscItemId.AirStrDamageAirAoD,
                MiscItemId.AirOrgDamageAirAoD,
                MiscItemId.NavalStrDamageNavyAoD,
                MiscItemId.NavalOrgDamageNavyAoD,
                MiscItemId.AirStrDamageNavyAoD,
                MiscItemId.AirOrgDamageNavyAoD,
                MiscItemId.MilitaryExpenseAttritionModifier,
                MiscItemId.NavalMinCombatTime,
                MiscItemId.LandMinCombatTime,
                MiscItemId.AirMinCombatTime,
                MiscItemId.LandOverstackingModifier,
                MiscItemId.LandOrgLossMoving,
                MiscItemId.AirOrgLossMoving,
                MiscItemId.NavalOrgLossMoving,
                MiscItemId.SupplyDistanceSeverity,
                MiscItemId.SupplyBase,
                MiscItemId.LandOrgGain,
                MiscItemId.AirOrgGain,
                MiscItemId.NavalOrgGain,
                MiscItemId.NukeManpowerDissent,
                MiscItemId.NukeIcDissent,
                MiscItemId.NukeTotalDissent,
                MiscItemId.LandFriendlyOrgGain,
                MiscItemId.AirLandStockModifier,
                MiscItemId.ScorchDamage,
                MiscItemId.StandGroundDissent,
                MiscItemId.ScorchGroundBelligerence,
                MiscItemId.DefaultLandStack,
                MiscItemId.DefaultNavalStack,
                MiscItemId.DefaultAirStack,
                MiscItemId.DefaultRocketStack,
                MiscItemId.FortDamageArtilleryBombardment,
                MiscItemId.ArtilleryBombardmentOrgCost,
                MiscItemId.LandDamageFort,
                MiscItemId.AirRebaseFactor,
                MiscItemId.AirMaxDisorganized,
                MiscItemId.AaInflictedStrDamage,
                MiscItemId.AaInflictedOrgDamage,
                MiscItemId.AaInflictedFlyingDamage,
                MiscItemId.AaInflictedBombingDamage,
                MiscItemId.HardAttackStrDamage,
                MiscItemId.HardAttackOrgDamage,
                MiscItemId.ArmorSoftBreakthroughMin,
                MiscItemId.ArmorSoftBreakthroughMax,
                MiscItemId.NavalCriticalHitChance,
                MiscItemId.NavalCriticalHitEffect,
                MiscItemId.LandFortDamage,
                MiscItemId.PortAttackSurpriseChanceDay,
                MiscItemId.PortAttackSurpriseChanceNight,
                MiscItemId.PortAttackSurpriseModifier,
                MiscItemId.RadarAntiSurpriseChance,
                MiscItemId.RadarAntiSurpriseModifier,
                MiscItemId.CounterAttackStrDefenderAoD,
                MiscItemId.CounterAttackOrgDefenderAoD,
                MiscItemId.CounterAttackStrAttackerAoD,
                MiscItemId.CounterAttackOrgAttackerAoD,
                MiscItemId.AssaultStrDefenderAoD,
                MiscItemId.AssaultOrgDefenderAoD,
                MiscItemId.AssaultStrAttackerAoD,
                MiscItemId.AssaultOrgAttackerAoD,
                MiscItemId.EncirclementStrDefenderAoD,
                MiscItemId.EncirclementOrgDefenderAoD,
                MiscItemId.EncirclementStrAttackerAoD,
                MiscItemId.EncirclementOrgAttackerAoD,
                MiscItemId.AmbushStrDefenderAoD,
                MiscItemId.AmbushOrgDefenderAoD,
                MiscItemId.AmbushStrAttackerAoD,
                MiscItemId.AmbushOrgAttackerAoD,
                MiscItemId.DelayStrDefenderAoD,
                MiscItemId.DelayOrgDefenderAoD,
                MiscItemId.DelayStrAttackerAoD,
                MiscItemId.DelayOrgAttackerAoD,
                MiscItemId.TacticalWithdrawStrDefenderAoD,
                MiscItemId.TacticalWithdrawOrgDefenderAoD,
                MiscItemId.TacticalWithdrawStrAttackerAoD,
                MiscItemId.TacticalWithdrawOrgAttackerAoD,
                MiscItemId.BreakthroughStrDefenderAoD,
                MiscItemId.BreakthroughOrgDefenderAoD,
                MiscItemId.BreakthroughStrAttackerAoD,
                MiscItemId.BreakthroughOrgAttackerAoD,
                MiscItemId.AaMinOrgDamage,
                MiscItemId.AaAdditionalOrgDamage,
                MiscItemId.AaMinStrDamage,
                MiscItemId.AaAdditionalStrDamage,
                MiscItemId.NavalOrgDamageAa,
                MiscItemId.AirOrgDamageAa,
                MiscItemId.AirStrDamageAa,
                MiscItemId.AaAirFiringRules,
                MiscItemId.AaAirNightModifier,
                MiscItemId.AaAirBonusRadars,
                MiscItemId.NukesStrDamage,
                MiscItemId.NukesMaxStrDamageFriendly,
                MiscItemId.NukesOrgDamage,
                MiscItemId.NukesOrgDamageNonFriendly,
                MiscItemId.NavalBombardmentChanceDamaged,
                MiscItemId.NavalBombardmentChanceBest,
                MiscItemId.TacticalBombardmentChanceDamaged,
                MiscItemId.MovementBonusTerrainTrait,
                MiscItemId.MovementBonusSimilarTerrainTrait,
                MiscItemId.LogisticsWizardEseBonus,
                MiscItemId.OffensiveSupplyESEBonus,
                MiscItemId.DaysOffensiveSupply,
                MiscItemId.MinisterBonuses,
                MiscItemId.OrgRegainBonusFriendly,
                MiscItemId.OrgRegainBonusFriendlyCap,
                MiscItemId.NewOrgRegainLogic,
                MiscItemId.OrgRegainMorale,
                MiscItemId.OrgRegainClear,
                MiscItemId.OrgRegainFrozen,
                MiscItemId.OrgRegainRaining,
                MiscItemId.OrgRegainSnowing,
                MiscItemId.OrgRegainStorm,
                MiscItemId.OrgRegainBlizzard,
                MiscItemId.OrgRegainMuddy,
                MiscItemId.OrgRegainNaval,
                MiscItemId.OrgRegainNavalOutOfFuel,
                MiscItemId.OrgRegainNavalOutOfSupplies,
                MiscItemId.OrgRegainNavalCurrent,
                MiscItemId.OrgRegainNavalBase,
                MiscItemId.OrgRegainNavalSea,
                MiscItemId.OrgRegainAir,
                MiscItemId.OrgRegainAirOutOfFuel,
                MiscItemId.OrgRegainAirOutOfSupplies,
                MiscItemId.OrgRegainAirCurrent,
                MiscItemId.OrgRegainAirBaseSize,
                MiscItemId.OrgRegainAirOutOfBase,
                MiscItemId.OrgRegainArmy,
                MiscItemId.OrgRegainArmyOutOfFuel,
                MiscItemId.OrgRegainArmyOutOfSupplies,
                MiscItemId.OrgRegainArmyCurrent,
                MiscItemId.OrgRegainArmyFriendly,
                MiscItemId.OrgRegainArmyTransportation,
                MiscItemId.OrgRegainArmyMoving,
                MiscItemId.OrgRegainArmyRetreating,
                MiscItemId.ConvoyInterceptionMissions,
                MiscItemId.AutoReturnTransportFleets,
                MiscItemId.AllowProvinceRegionTargeting,
                MiscItemId.NightHoursWinter,
                MiscItemId.NightHoursSpringFall,
                MiscItemId.NightHoursSummer,
                MiscItemId.RecalculateLandArrivalTimes,
                MiscItemId.SynchronizeArrivalTimePlayer,
                MiscItemId.SynchronizeArrivalTimeAi,
                MiscItemId.RecalculateArrivalTimesCombat,
                MiscItemId.LandSpeedModifierCombat,
                MiscItemId.LandSpeedModifierBombardment,
                MiscItemId.LandSpeedModifierSupply,
                MiscItemId.LandSpeedModifierOrg,
                MiscItemId.LandAirSpeedModifierFuel,
                MiscItemId.DefaultSpeedFuel,
                MiscItemId.FleetSizeRangePenaltyRatio,
                MiscItemId.FleetSizeRangePenaltyThrethold,
                MiscItemId.FleetSizeRangePenaltyMax,
                MiscItemId.ApplyRangeLimitsAreasRegions,
                MiscItemId.RadarBonusDetection,
                MiscItemId.BonusDetectionFriendly,
                MiscItemId.ScreensCapitalRatioModifier,
                MiscItemId.ChanceTargetNoOrgLand,
                MiscItemId.ScreenCapitalShipsTargeting,
                MiscItemId.FleetPositioningDaytime,
                MiscItemId.FleetPositioningLeaderSkill,
                MiscItemId.FleetPositioningFleetSize,
                MiscItemId.FleetPositioningFleetComposition,
                MiscItemId.LandCoastalFortsDamage,
                MiscItemId.LandCoastalFortsMaxDamage,
                MiscItemId.MinSoftnessBrigades,
                MiscItemId.AutoRetreatOrg,
                MiscItemId.LandOrgNavalTransportation,
                MiscItemId.MaxLandDig,
                MiscItemId.DigIncreaseDay,
                MiscItemId.BreakthroughEncirclementMinSpeed,
                MiscItemId.BreakthroughEncirclementMaxChance,
                MiscItemId.BreakthroughEncirclementChanceModifier,
                MiscItemId.CombatEventDuration,
                MiscItemId.CounterAttackOrgAttackerDh,
                MiscItemId.CounterAttackStrAttackerDh,
                MiscItemId.CounterAttackOrgDefenderDh,
                MiscItemId.CounterAttackStrDefenderDh,
                MiscItemId.AssaultOrgAttackerDh,
                MiscItemId.AssaultStrAttackerDh,
                MiscItemId.AssaultOrgDefenderDh,
                MiscItemId.AssaultStrDefenderDh,
                MiscItemId.EncirclementOrgAttackerDh,
                MiscItemId.EncirclementStrAttackerDh,
                MiscItemId.EncirclementOrgDefenderDh,
                MiscItemId.EncirclementStrDefenderDh,
                MiscItemId.AmbushOrgAttackerDh,
                MiscItemId.AmbushStrAttackerDh,
                MiscItemId.AmbushOrgDefenderDh,
                MiscItemId.AmbushStrDefenderDh,
                MiscItemId.DelayOrgAttackerDh,
                MiscItemId.DelayStrAttackerDh,
                MiscItemId.DelayOrgDefenderDh,
                MiscItemId.DelayStrDefenderDh,
                MiscItemId.TacticalWithdrawOrgAttackerDh,
                MiscItemId.TacticalWithdrawStrAttackerDh,
                MiscItemId.TacticalWithdrawOrgDefenderDh,
                MiscItemId.TacticalWithdrawStrDefenderDh,
                MiscItemId.BreakthroughOrgAttackerDh,
                MiscItemId.BreakthroughStrAttackerDh,
                MiscItemId.BreakthroughOrgDefenderDh,
                MiscItemId.BreakthroughStrDefenderDh,
                MiscItemId.HqStrDamageBreakthrough,
                MiscItemId.CombatMode
            }
            ,
            new[]
            {
                MiscItemId.AttackMission,
                MiscItemId.AttackStartingEfficiency,
                MiscItemId.AttackSpeedBonus,
                MiscItemId.RebaseMission,
                MiscItemId.RebaseStartingEfficiency,
                MiscItemId.RebaseChanceDetected,
                MiscItemId.StratRedeployMission,
                MiscItemId.StratRedeployStartingEfficiency,
                MiscItemId.StratRedeployAddedValue,
                MiscItemId.StratRedeployDistanceMultiplier,
                MiscItemId.SupportAttackMission,
                MiscItemId.SupportAttackStartingEfficiency,
                MiscItemId.SupportAttackSpeedBonus,
                MiscItemId.SupportDefenseMission,
                MiscItemId.SupportDefenseStartingEfficiency,
                MiscItemId.SupportDefenseSpeedBonus,
                MiscItemId.ReservesMission,
                MiscItemId.ReservesStartingEfficiency,
                MiscItemId.ReservesSpeedBonus,
                MiscItemId.AntiPartisanDutyMission,
                MiscItemId.AntiPartisanDutyStartingEfficiency,
                MiscItemId.AntiPartisanDutySuppression,
                MiscItemId.PlannedDefenseMission,
                MiscItemId.PlannedDefenseStartingEfficiency,
                MiscItemId.AirSuperiorityMission,
                MiscItemId.AirSuperiorityStartingEfficiency,
                MiscItemId.AirSuperiorityDetection,
                MiscItemId.AirSuperiorityMinRequired,
                MiscItemId.GroundAttackMission,
                MiscItemId.GroundAttackStartingEfficiency,
                MiscItemId.GroundAttackOrgDamage,
                MiscItemId.GroundAttackStrDamage,
                MiscItemId.InterdictionMission,
                MiscItemId.InterdictionStartingEfficiency,
                MiscItemId.InterdictionOrgDamage,
                MiscItemId.InterdictionStrDamage,
                MiscItemId.StrategicBombardmentMission,
                MiscItemId.StrategicBombardmentStartingEfficiency,
                MiscItemId.LogisticalStrikeMission,
                MiscItemId.LogisticalStrikeStartingEfficiency,
                MiscItemId.RunwayCrateringMission,
                MiscItemId.RunwayCrateringStartingEfficiency,
                MiscItemId.InstallationStrikeMission,
                MiscItemId.InstallationStrikeStartingEfficiency,
                MiscItemId.NavalStrikeMission,
                MiscItemId.NavalStrikeStartingEfficiency,
                MiscItemId.PortStrikeMission,
                MiscItemId.PortStrikeStartingEfficiency,
                MiscItemId.ConvoyAirRaidingMission,
                MiscItemId.ConvoyAirRaidingStartingEfficiency,
                MiscItemId.AirSupplyMission,
                MiscItemId.AirSupplyStartingEfficiency,
                MiscItemId.AirborneAssaultMission,
                MiscItemId.AirborneAssaultStartingEfficiency,
                MiscItemId.NukeMission,
                MiscItemId.NukeStartingEfficiency,
                MiscItemId.AirScrambleMission,
                MiscItemId.AirScrambleStartingEfficiency,
                MiscItemId.AirScrambleDetection,
                MiscItemId.AirScrambleMinRequired,
                MiscItemId.ConvoyRadingMission,
                MiscItemId.ConvoyRadingStartingEfficiency,
                MiscItemId.ConvoyRadingRangeModifier,
                MiscItemId.ConvoyRadingChanceDetected,
                MiscItemId.AswMission,
                MiscItemId.AswStartingEfficiency,
                MiscItemId.NavalInterdictionMission,
                MiscItemId.NavalInterdictionStartingEfficiency,
                MiscItemId.ShoreBombardmentMission,
                MiscItemId.ShoreBombardmentStartingEfficiency,
                MiscItemId.ShoreBombardmentModifierDh,
                MiscItemId.AmphibousAssaultMission,
                MiscItemId.AmphibousAssaultStartingEfficiency,
                MiscItemId.SeaTransportMission,
                MiscItemId.SeaTransportStartingEfficiency,
                MiscItemId.SeaTransportRangeModifier,
                MiscItemId.SeaTransportChanceDetected,
                MiscItemId.NavalCombatPatrolMission,
                MiscItemId.NavalCombatPatrolStartingEfficiency,
                MiscItemId.NavalPortStrikeMission,
                MiscItemId.NavalPortStrikeStartingEfficiency,
                MiscItemId.NavalAirbaseStrikeMission,
                MiscItemId.NavalAirbaseStrikeStartingEfficiency,
                MiscItemId.SneakMoveMission,
                MiscItemId.SneakMoveStartingEfficiency,
                MiscItemId.SneakMoveRangeModifier,
                MiscItemId.SneakMoveChanceDetected,
                MiscItemId.NavalScrambleMission,
                MiscItemId.NavalScrambleStartingEfficiency,
                MiscItemId.NavalScrambleSpeedBonus,
                MiscItemId.UseAttackEfficiencyCombatModifier
            },
            new[]
            {
                MiscItemId.LandFortEfficiency,
                MiscItemId.CoastalFortEfficiency,
                MiscItemId.GroundDefenseEfficiency,
                MiscItemId.ConvoyDefenseEfficiency,
                MiscItemId.ManpowerBoost,
                MiscItemId.TransportCapacityModifier,
                MiscItemId.OccupiedTransportCapacityModifier,
                MiscItemId.AttritionModifier,
                MiscItemId.ManpowerTrickleBackModifier,
                MiscItemId.SupplyDistanceModifier,
                MiscItemId.RepairModifier,
                MiscItemId.ResearchModifier,
                MiscItemId.RadarEfficiency,
                MiscItemId.HqSupplyEfficiencyBonus,
                MiscItemId.HqCombatEventsBonus,
                MiscItemId.CombatEventChances,
                MiscItemId.FriendlyArmyDetectionChance,
                MiscItemId.EnemyArmyDetectionChance,
                MiscItemId.FriendlyIntelligenceChance,
                MiscItemId.EnemyIntelligenceChance,
                MiscItemId.MaxAmphibiousArmySize,
                MiscItemId.EnergyToOil,
                MiscItemId.TotalProductionEfficiency,
                MiscItemId.OilProductionEfficiency,
                MiscItemId.MetalProductionEfficiency,
                MiscItemId.EnergyProductionEfficiency,
                MiscItemId.RareMaterialsProductionEfficiency,
                MiscItemId.MoneyProductionEfficiency,
                MiscItemId.SupplyProductionEfficiency,
                MiscItemId.AaPower,
                MiscItemId.AirSurpriseChance,
                MiscItemId.LandSurpriseChance,
                MiscItemId.NavalSurpriseChance,
                MiscItemId.PeacetimeIcModifier,
                MiscItemId.WartimeIcModifier,
                MiscItemId.BuildingsProductionModifier,
                MiscItemId.ConvoysProductionModifier,
                MiscItemId.MinShipsPositioningBattle,
                MiscItemId.MaxShipsPositioningBattle,
                MiscItemId.PeacetimeStockpilesResources,
                MiscItemId.WartimeStockpilesResources,
                MiscItemId.PeacetimeStockpilesOilSupplies,
                MiscItemId.WartimeStockpilesOilSupplies,
                MiscItemId.MaxLandDigDH105,
                MiscItemId.DigIncreaseDayDH105
            },
            new[]
            {
                MiscItemId.BlueprintBonus,
                MiscItemId.PreHistoricalDateModifier,
                MiscItemId.PostHistoricalDateModifierDh,
                MiscItemId.CostSkillLevel,
                MiscItemId.MeanNumberInventionEventsYear,
                MiscItemId.PostHistoricalDateModifierAoD,
                MiscItemId.TechSpeedModifier,
                MiscItemId.PreHistoricalPenaltyLimit,
                MiscItemId.PostHistoricalBonusLimit,
                MiscItemId.MaxActiveTechTeamsAoD,
                MiscItemId.RequiredIcEachTechTeamAoD,
                MiscItemId.MaximumRandomModifier,
                MiscItemId.UseNewTechnologyPageLayout,
                MiscItemId.TechOverviewPanelStyle,
                MiscItemId.MaxActiveTechTeamsDh,
                MiscItemId.MinActiveTechTeams,
                MiscItemId.RequiredIcEachTechTeamDh,
                MiscItemId.NewCountryRocketryComponent,
                MiscItemId.NewCountryNuclearPhysicsComponent,
                MiscItemId.NewCountryNuclearEngineeringComponent,
                MiscItemId.NewCountrySecretTechs,
                MiscItemId.MaxTechTeamSkill
            },
            new[]
            {
                MiscItemId.DaysTradeOffers,
                MiscItemId.DelayGameStartNewTrades,
                MiscItemId.LimitAiNewTradesGameStart,
                MiscItemId.DesiredOilStockpile,
                MiscItemId.CriticalOilStockpile,
                MiscItemId.DesiredSuppliesStockpile,
                MiscItemId.CriticalSuppliesStockpile,
                MiscItemId.DesiredResourcesStockpile,
                MiscItemId.CriticalResourceStockpile,
                MiscItemId.WartimeDesiredStockpileMultiplier,
                MiscItemId.PeacetimeExtraOilImport,
                MiscItemId.WartimeExtraOilImport,
                MiscItemId.ExtraImportBelowDesired,
                MiscItemId.PercentageProducedSupplies,
                MiscItemId.PercentageProducedMoney,
                MiscItemId.ExtraImportStockpileSelected,
                MiscItemId.DaysDeliverResourcesTrades,
                MiscItemId.MergeTradeDeals,
                MiscItemId.ManualTradeDeals,
                MiscItemId.PuppetsSendSuppliesMoney,
                MiscItemId.PuppetsCriticalSupplyStockpile,
                MiscItemId.PuppetsMaxPoolResources,
                MiscItemId.NewTradeDealsMinEffectiveness,
                MiscItemId.CancelTradeDealsEffectiveness,
                MiscItemId.AutoTradeAiTradeDeals
            },
            new[]
            {
                MiscItemId.OverproduceSuppliesBelowDesired,
                MiscItemId.MultiplierOverproduceSuppliesWar,
                MiscItemId.NotProduceSuppliesStockpileOver,
                MiscItemId.MaxSerialLineProductionGarrisonMilitia,
                MiscItemId.MinIcSerialProductionNavalAir,
                MiscItemId.NotProduceNewUnitsManpowerRatio,
                MiscItemId.NotProduceNewUnitsManpowerValue,
                MiscItemId.NotProduceNewUnitsSupply,
                MiscItemId.MilitaryStrengthTotalIcRatioPeacetime,
                MiscItemId.MilitaryStrengthTotalIcRatioWartime,
                MiscItemId.MilitaryStrengthTotalIcRatioMajor,
                MiscItemId.NotUseOffensiveSupplyStockpile,
                MiscItemId.NotUseOffensiveOilStockpile,
                MiscItemId.NotUseOffensiveEse,
                MiscItemId.NotUseOffensiveOrgStrDamage,
                MiscItemId.AiPeacetimeSpyMissionsDh,
                MiscItemId.AiSpyMissionsCostModifierDh,
                MiscItemId.AiDiplomacyCostModifierDh,
                MiscItemId.AiInfluenceModifierDh,
                MiscItemId.NewDowRules,
                MiscItemId.NewDowRules2,
                MiscItemId.ForcePuppetsJoinMastersAllianceNeutrality,
                MiscItemId.CountriesLeaveBadRelationAlliance,
                MiscItemId.NewAiReleaseRules,
                MiscItemId.AiEventsActionSelectionRules,
                MiscItemId.ForceStrategicRedeploymentHour,
                MiscItemId.MaxRedeploymentDaysAi,
                MiscItemId.UseQuickAreaCheckGarrisonAi,
                MiscItemId.AiMastersGetProvincesConquredPuppets,
                MiscItemId.ReleaseCountryWarZone,
                MiscItemId.MinDaysRequiredAiReleaseCountry,
                MiscItemId.MinDaysRequiredAiAllied,
                MiscItemId.MinDaysRequiredAiAlliedSupplyBase,
                MiscItemId.MinRequiredRelationsAlliedClaimed,
                MiscItemId.AiUnitPowerCalculationStrOrg,
                MiscItemId.AiUnitPowerCalculationGde,
                MiscItemId.AiUnitPowerCalculationMinOrg,
                MiscItemId.AiUnitPowerCalculationMinStr
            },
            new[]
            {
                MiscItemId.AiSpyDiplomaticMissionLogger,
                MiscItemId.CountryLogger,
                MiscItemId.SwitchedAiFilesLogger,
                MiscItemId.UseNewAutoSaveFileFormat,
                MiscItemId.LoadNewAiSwitchingAllClients,
                MiscItemId.TradeEfficiencyCalculationSystem,
                MiscItemId.MergeRelocateProvincialDepots,
                MiscItemId.InGameLossesLogging,
                MiscItemId.InGameLossLogging2,
                MiscItemId.AllowBrigadeAttachingInSupply,
                MiscItemId.MultipleDeploymentSizeArmies,
                MiscItemId.MultipleDeploymentSizeFleets,
                MiscItemId.MultipleDeploymentSizeAir,
                MiscItemId.AllowUniquePicturesAllLandProvinces,
                MiscItemId.AutoReplyEvents,
                MiscItemId.ForceActionsShow,
                MiscItemId.EnableDicisionsPlayers,
                MiscItemId.RebelsArmyComposition,
                MiscItemId.RebelsArmyTechLevel,
                MiscItemId.RebelsArmyMinStr,
                MiscItemId.RebelsArmyMaxStr,
                MiscItemId.RebelsOrgRegain,
                MiscItemId.ExtraRebelBonusNeighboringProvince,
                MiscItemId.ExtraRebelBonusOccupied,
                MiscItemId.ExtraRebelBonusMountain,
                MiscItemId.ExtraRebelBonusHill,
                MiscItemId.ExtraRebelBonusForest,
                MiscItemId.ExtraRebelBonusJungle,
                MiscItemId.ExtraRebelBonusSwamp,
                MiscItemId.ExtraRebelBonusDesert,
                MiscItemId.ExtraRebelBonusPlain,
                MiscItemId.ExtraRebelBonusUrban,
                MiscItemId.ExtraRebelBonusAirNavalBases,
                MiscItemId.ReturnRebelliousProvince,
                MiscItemId.UseNewMinisterFilesFormat,
                MiscItemId.EnableRetirementYearMinisters,
                MiscItemId.EnableRetirementYearLeaders,
                MiscItemId.LoadSpritesModdirOnly,
                MiscItemId.LoadUnitIconsModdirOnly,
                MiscItemId.LoadUnitPicturesModdirOnly,
                MiscItemId.LoadAiFilesModdirOnly,
                MiscItemId.UseSpeedSetGarrisonStatus,
                MiscItemId.UseOldSaveGameFormat,
                MiscItemId.ProductionPanelUiStyle,
                MiscItemId.UnitPicturesSize,
                MiscItemId.EnablePicturesNavalBrigades,
                MiscItemId.BuildingsBuildableOnlyProvinces,
                MiscItemId.UnitModifiersStatisticsPages,
                MiscItemId.CheatMultiPlayer,
                MiscItemId.ManualChangeConvoy,
                MiscItemId.BrigadesRepairManpowerCost,
                MiscItemId.StrPercentageBrigadesAttachment
            },
            new[]
            {
                MiscItemId.MapNumber,
                MiscItemId.TotalProvinces,
                MiscItemId.DistanceCalculationModel,
                MiscItemId.MapWidth,
                MiscItemId.MapHeight
            }
        };

        /// <summary>
        ///     Presence or absence of items for each game
        /// </summary>
        public static bool[,] ItemTable =
        {
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false, false },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { true, true, true, true, true, false, false, false, false },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, false, false, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, false, false, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, false, false, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, false, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, false, false, false, false },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false, false },
            { true, true, true, true, true, true, true, true, true },
            { true, true, false, false, false, true, true, true, true },
            { false, false, true, true, true, false, false, false, false },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, false, false, false, false, false, true },
            { false, false, false, false, false, false, false, false, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false, false },
            { true, true, true, true, true, true, false, false, false },
            { true, true, true, true, true, true, false, false, false },
            { true, true, true, true, true, true, false, false, false },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, false, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, false, false, false, false },
            { true, true, false, false, false, false, false, false, false },
            { true, true, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, true, true, false, false },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, false, false, false, false },
            { false, true, false, false, false, true, true, true, true },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, false, false, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, false },
            { false, false, false, false, false, false, true, true, false },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, true, false, false },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, false, false, true },
            { false, false, false, false, false, false, false, false, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { true, true, true, true, true, true, true, true, true },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, true, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, true, true, false, false, false, false },
            { false, false, false, false, true, false, false, false, false },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, false, false, false },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, false, false, false },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, true, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, false, false, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
            { false, false, false, false, false, true, true, true, true },
        };

        /// <summary>
        ///     Item type
        /// </summary>
        public static MiscItemType[] ItemTypes =
        {
            MiscItemType.NonNegDbl2AoD,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDblMinusOne,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2Dh103Full2,
            MiscItemType.NonNegDbl2Dh103Full2,
            MiscItemType.NonNegDbl2Dh103Full2,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.RangedInt,
            MiscItemType.NonNegDbl4Dda13,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.RangedInt,
            MiscItemType.RangedInt,
            MiscItemType.Enum,
            MiscItemType.RangedDbl,
            MiscItemType.RangedInt,
            MiscItemType.Enum,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.Bool,
            MiscItemType.Enum,
            MiscItemType.Enum,
            MiscItemType.Enum,
            MiscItemType.Bool,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDblMinusOne,
            MiscItemType.NonNegDblMinusOne,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Enum,
            MiscItemType.NonNegDblMinusOne1,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.RangedInt,
            MiscItemType.RangedInt,
            MiscItemType.Int,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.RangedInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.Enum,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedInt,
            MiscItemType.RangedIntMinusOne,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.NonNegDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.Bool,
            MiscItemType.Enum,
            MiscItemType.Bool,
            MiscItemType.Enum,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonNegDbl2AoD,
            MiscItemType.NonNegDbl2AoD,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl2Dh103Full,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.Dbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl2,
            MiscItemType.NonPosDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonPosInt,
            MiscItemType.NonPosInt,
            MiscItemType.RangedDbl0,
            MiscItemType.NonPosInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.PosInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Dbl,
            MiscItemType.Dbl,
            MiscItemType.Dbl,
            MiscItemType.Dbl,
            MiscItemType.Dbl,
            MiscItemType.Dbl,
            MiscItemType.Dbl,
            MiscItemType.Dbl,
            MiscItemType.Dbl,
            MiscItemType.Dbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.RangedDbl0,
            MiscItemType.NonNegDbl,
            MiscItemType.RangedDbl0,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDblMinusOne1,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2Dh103Full,
            MiscItemType.NonNegDbl,
            MiscItemType.RangedDbl,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDblMinusOne,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonPosInt,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Enum,
            MiscItemType.RangedDbl0,
            MiscItemType.NonNegDbl0,
            MiscItemType.NonNegDbl,
            MiscItemType.RangedDbl,
            MiscItemType.RangedDbl,
            MiscItemType.RangedDbl,
            MiscItemType.RangedInt,
            MiscItemType.RangedInt,
            MiscItemType.RangedInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.Enum,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Enum,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Enum,
            MiscItemType.Enum,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.RangedDbl,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Enum,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegIntMinusOne,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegIntMinusOne,
            MiscItemType.RangedDbl,
            MiscItemType.Enum,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.RangedInt,
            MiscItemType.NonNegIntMinusOne,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonPosDbl,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt1,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.Enum,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt1,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonPosDbl5AoD,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl5,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.RangedInt,
            MiscItemType.PosInt,
            MiscItemType.NonNegDbl0,
            MiscItemType.Enum,
            MiscItemType.Enum,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.RangedPosInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl2,
            MiscItemType.NonNegDbl2Dh103Full1,
            MiscItemType.NonNegDbl2Dh103Full1,
            MiscItemType.NonNegDbl2,
            MiscItemType.PosInt,
            MiscItemType.Bool,
            MiscItemType.RangedIntMinusOne,
            MiscItemType.NonNegInt,
            MiscItemType.PosDbl,
            MiscItemType.NonNegIntNegDbl,
            MiscItemType.RangedInt,
            MiscItemType.RangedInt,
            MiscItemType.RangedInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.RangedInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.RangedDbl,
            MiscItemType.RangedDblMinusOne1,
            MiscItemType.RangedDblMinusOne1,
            MiscItemType.PosDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.RangedDbl,
            MiscItemType.Enum,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Enum,
            MiscItemType.Enum,
            MiscItemType.NonNegInt,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.RangedIntMinusThree,
            MiscItemType.NonNegIntMinusOne,
            MiscItemType.NonNegInt,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.RangedDbl,
            MiscItemType.Enum,
            MiscItemType.Enum,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegDbl,
            MiscItemType.Bool,
            MiscItemType.NonNegIntMinusOne,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Int,
            MiscItemType.NonNegInt,
            MiscItemType.Bool,
            MiscItemType.Enum,
            MiscItemType.Bool,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.PosInt,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Enum,
            MiscItemType.Bool,
            MiscItemType.RangedInt,
            MiscItemType.NonNegIntMinusOne,
            MiscItemType.RangedInt,
            MiscItemType.RangedInt,
            MiscItemType.NonNegDbl,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.NonNegInt,
            MiscItemType.PosInt,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Bool,
            MiscItemType.Enum,
            MiscItemType.Bool,
            MiscItemType.Enum,
            MiscItemType.Enum,
            MiscItemType.Bool,
            MiscItemType.Enum,
            MiscItemType.PosInt,
            MiscItemType.Bool,
            MiscItemType.Enum,
            MiscItemType.Bool,
            MiscItemType.RangedDbl,
            MiscItemType.NonNegInt,
            MiscItemType.RangedInt,
            MiscItemType.Enum,
            MiscItemType.PosInt,
            MiscItemType.PosInt
        };

        /// <summary>
        ///     Minimum value of item (( Integer type / / Enumeration type )
        /// </summary>
        public static readonly Dictionary<MiscItemId, int> IntMinValues = new Dictionary<MiscItemId, int>
        {
            { MiscItemId.MaxRevoltRisk, 0 },
            { MiscItemId.CanUnitSendNonAllied, 0 },
            { MiscItemId.ChanceDetectSpyMission, 0 },
            { MiscItemId.RelationshipsHitDetectedMissions, 0 },
            { MiscItemId.ShowThirdCountrySpyReports, 0 },
            { MiscItemId.SpyInformationAccuracyModifier, -10 },
            { MiscItemId.AiPeacetimeSpyMissions, 0 },
            { MiscItemId.CanChangeIdeas, 0 },
            { MiscItemId.CanUnitSendNonAlliedDh, 0 },
            { MiscItemId.BluePrintsCanSoldNonAllied, 0 },
            { MiscItemId.ProvinceCanSoldNonAllied, 0 },
            { MiscItemId.TransferAlliedCoreProvinces, 0 },
            { MiscItemId.ProductionLineEdit, 0 },
            { MiscItemId.ConvoySystemOptionsAllied, 0 },
            { MiscItemId.ChanceDetectSpyMissionDh, 0 },
            { MiscItemId.RelationshipsHitDetectedMissionsDh, 0 },
            { MiscItemId.SpyInformationAccuracyModifierDh, -10 },
            { MiscItemId.ShowThirdCountrySpyReportsDh, 0 },
            { MiscItemId.UseMinisterPersonalityReplacing, 0 },
            { MiscItemId.RelationshipHitCancelTrade, 0 },
            { MiscItemId.RelationshipHitCancelPermanentTrade, 0 },
            { MiscItemId.PuppetsJoinMastersAlliance, 0 },
            { MiscItemId.MastersBecomePuppetsPuppets, 0 },
            { MiscItemId.AllowManualClaimsChange, 0 },
            { MiscItemId.JoinAutomaticallyAllesAxis, 0 },
            { MiscItemId.AllowChangeHosHog, 0 },
            { MiscItemId.ChangeTagCoup, 0 },
            { MiscItemId.FilterReleaseCountries, 0 },
            { MiscItemId.AaAirFiringRules, 0 },
            { MiscItemId.NavalBombardmentChanceDamaged, 0 },
            { MiscItemId.NavalBombardmentChanceBest, 0 },
            { MiscItemId.TacticalBombardmentChanceDamaged, 0 },
            { MiscItemId.MinisterBonuses, 1 },
            { MiscItemId.NewOrgRegainLogic, 0 },
            { MiscItemId.ConvoyInterceptionMissions, 0 },
            { MiscItemId.AutoReturnTransportFleets, 0 },
            { MiscItemId.AllowProvinceRegionTargeting, 0 },
            { MiscItemId.RecalculateArrivalTimesCombat, 0 },
            { MiscItemId.ApplyRangeLimitsAreasRegions, 0 },
            { MiscItemId.ChanceTargetNoOrgLand, 0 },
            { MiscItemId.HqStrDamageBreakthrough, 0 },
            { MiscItemId.CombatMode, 0 },
            { MiscItemId.AttackMission, 0 },
            { MiscItemId.RebaseMission, 0 },
            { MiscItemId.StratRedeployMission, 0 },
            { MiscItemId.SupportAttackMission, 0 },
            { MiscItemId.SupportDefenseMission, 0 },
            { MiscItemId.ReservesMission, 0 },
            { MiscItemId.AntiPartisanDutyMission, 0 },
            { MiscItemId.PlannedDefenseMission, 0 },
            { MiscItemId.AirSuperiorityMission, 0 },
            { MiscItemId.GroundAttackMission, 0 },
            { MiscItemId.InterdictionMission, 0 },
            { MiscItemId.StrategicBombardmentMission, 0 },
            { MiscItemId.LogisticalStrikeMission, 0 },
            { MiscItemId.RunwayCrateringMission, 0 },
            { MiscItemId.InstallationStrikeMission, 0 },
            { MiscItemId.NavalStrikeMission, 0 },
            { MiscItemId.PortStrikeMission, 0 },
            { MiscItemId.ConvoyAirRaidingMission, 0 },
            { MiscItemId.AirSupplyMission, 0 },
            { MiscItemId.AirborneAssaultMission, 0 },
            { MiscItemId.NukeMission, 0 },
            { MiscItemId.AirScrambleMission, 0 },
            { MiscItemId.ConvoyRadingMission, 0 },
            { MiscItemId.AswMission, 0 },
            { MiscItemId.NavalInterdictionMission, 0 },
            { MiscItemId.ShoreBombardmentMission, 0 },
            { MiscItemId.AmphibousAssaultMission, 0 },
            { MiscItemId.SeaTransportMission, 0 },
            { MiscItemId.NavalCombatPatrolMission, 0 },
            { MiscItemId.NavalPortStrikeMission, 0 },
            { MiscItemId.NavalAirbaseStrikeMission, 0 },
            { MiscItemId.SneakMoveMission, 0 },
            { MiscItemId.NavalScrambleMission, 0 },
            { MiscItemId.UseAttackEfficiencyCombatModifier, 0 },
            { MiscItemId.MaxActiveTechTeamsAoD, 1 },
            { MiscItemId.UseNewTechnologyPageLayout, 0 },
            { MiscItemId.TechOverviewPanelStyle, 0 },
            { MiscItemId.NewCountryRocketryComponent, 0 },
            { MiscItemId.NewCountryNuclearPhysicsComponent, 0 },
            { MiscItemId.NewCountryNuclearEngineeringComponent, 0 },
            { MiscItemId.NewCountrySecretTechs, 0 },
            { MiscItemId.DelayGameStartNewTrades, 2 },
            { MiscItemId.MergeTradeDeals, 0 },
            { MiscItemId.ManualTradeDeals, 0 },
            { MiscItemId.NewTradeDealsMinEffectiveness, 0 },
            { MiscItemId.CancelTradeDealsEffectiveness, 0 },
            { MiscItemId.AutoTradeAiTradeDeals, 0 },
            { MiscItemId.MaxSerialLineProductionGarrisonMilitia, 1 },
            { MiscItemId.AiPeacetimeSpyMissionsDh, 0 },
            { MiscItemId.NewDowRules, 0 },
            { MiscItemId.NewDowRules2, 0 },
            { MiscItemId.CountriesLeaveBadRelationAlliance, 0 },
            { MiscItemId.NewAiReleaseRules, 0 },
            { MiscItemId.AiEventsActionSelectionRules, 0 },
            { MiscItemId.UseQuickAreaCheckGarrisonAi, 0 },
            { MiscItemId.AiMastersGetProvincesConquredPuppets, 0 },
            { MiscItemId.ReleaseCountryWarZone, 0 },
            { MiscItemId.AiUnitPowerCalculationStrOrg, 0 },
            { MiscItemId.AiUnitPowerCalculationGde, 0 },
            { MiscItemId.AiUnitPowerCalculationMinOrg, 0 },
            { MiscItemId.AiSpyDiplomaticMissionLogger, 0 },
            { MiscItemId.SwitchedAiFilesLogger, 0 },
            { MiscItemId.UseNewAutoSaveFileFormat, 0 },
            { MiscItemId.LoadNewAiSwitchingAllClients, 0 },
            { MiscItemId.InGameLossesLogging, 0 },
            { MiscItemId.InGameLossLogging2, 0 },
            { MiscItemId.AllowBrigadeAttachingInSupply, 0 },
            { MiscItemId.AllowUniquePicturesAllLandProvinces, 0 },
            { MiscItemId.AutoReplyEvents, 0 },
            { MiscItemId.ForceActionsShow, 0 },
            { MiscItemId.EnableDicisionsPlayers, 0 },
            { MiscItemId.RebelsArmyComposition, 0 },
            { MiscItemId.RebelsArmyMinStr, 1 },
            { MiscItemId.RebelsArmyMaxStr, 1 },
            { MiscItemId.UseNewMinisterFilesFormat, 0 },
            { MiscItemId.EnableRetirementYearMinisters, 0 },
            { MiscItemId.EnableRetirementYearLeaders, 0 },
            { MiscItemId.LoadSpritesModdirOnly, 0 },
            { MiscItemId.LoadUnitIconsModdirOnly, 0 },
            { MiscItemId.LoadUnitPicturesModdirOnly, 0 },
            { MiscItemId.LoadAiFilesModdirOnly, 0 },
            { MiscItemId.UseSpeedSetGarrisonStatus, 0 },
            { MiscItemId.UseOldSaveGameFormat, 0 },
            { MiscItemId.ProductionPanelUiStyle, 0 },
            { MiscItemId.UnitPicturesSize, 0 },
            { MiscItemId.EnablePicturesNavalBrigades, 0 },
            { MiscItemId.BuildingsBuildableOnlyProvinces, 0 },
            { MiscItemId.CheatMultiPlayer, 0 },
            { MiscItemId.ManualChangeConvoy, 0 },
            { MiscItemId.BrigadesRepairManpowerCost, 0 },
            { MiscItemId.TotalProvinces, 1 },
            { MiscItemId.DistanceCalculationModel, 0 }
        };

        /// <summary>
        ///     Maximum value of item (( Integer type / / Enum )
        /// </summary>
        public static Dictionary<MiscItemId, int> IntMaxValues = new Dictionary<MiscItemId, int>
        {
            { MiscItemId.MaxRevoltRisk, 100 },
            { MiscItemId.CanUnitSendNonAllied, 1 },
            { MiscItemId.ChanceDetectSpyMission, 100 },
            { MiscItemId.RelationshipsHitDetectedMissions, 400 },
            { MiscItemId.ShowThirdCountrySpyReports, 3 },
            { MiscItemId.SpyInformationAccuracyModifier, 10 },
            { MiscItemId.AiPeacetimeSpyMissions, 2 },
            { MiscItemId.CanChangeIdeas, 1 },
            { MiscItemId.CanUnitSendNonAlliedDh, 2 },
            { MiscItemId.BluePrintsCanSoldNonAllied, 2 },
            { MiscItemId.ProvinceCanSoldNonAllied, 2 },
            { MiscItemId.TransferAlliedCoreProvinces, 1 },
            { MiscItemId.ProductionLineEdit, 1 },
            { MiscItemId.ConvoySystemOptionsAllied, 2 },
            { MiscItemId.ChanceDetectSpyMissionDh, 100 },
            { MiscItemId.RelationshipsHitDetectedMissionsDh, 400 },
            { MiscItemId.SpyInformationAccuracyModifierDh, 10 },
            { MiscItemId.ShowThirdCountrySpyReportsDh, 3 },
            { MiscItemId.UseMinisterPersonalityReplacing, 1 },
            { MiscItemId.RelationshipHitCancelTrade, 400 },
            { MiscItemId.RelationshipHitCancelPermanentTrade, 100 },
            { MiscItemId.PuppetsJoinMastersAlliance, 1 },
            { MiscItemId.MastersBecomePuppetsPuppets, 1 },
            { MiscItemId.AllowManualClaimsChange, 1 },
            { MiscItemId.JoinAutomaticallyAllesAxis, 1 },
            { MiscItemId.AllowChangeHosHog, 3 },
            { MiscItemId.ChangeTagCoup, 1 },
            { MiscItemId.FilterReleaseCountries, 3 },
            { MiscItemId.AaAirFiringRules, 1 },
            { MiscItemId.NavalBombardmentChanceDamaged, 100 },
            { MiscItemId.NavalBombardmentChanceBest, 100 },
            { MiscItemId.TacticalBombardmentChanceDamaged, 100 },
            { MiscItemId.MinisterBonuses, 5 },
            { MiscItemId.NewOrgRegainLogic, 1 },
            { MiscItemId.ConvoyInterceptionMissions, 2 },
            { MiscItemId.AutoReturnTransportFleets, 2 },
            { MiscItemId.AllowProvinceRegionTargeting, 1 },
            { MiscItemId.RecalculateArrivalTimesCombat, 2 },
            { MiscItemId.ApplyRangeLimitsAreasRegions, 2 },
            { MiscItemId.ChanceTargetNoOrgLand, 100 },
            { MiscItemId.HqStrDamageBreakthrough, 1 },
            { MiscItemId.CombatMode, 1 },
            { MiscItemId.AttackMission, 1 },
            { MiscItemId.RebaseMission, 1 },
            { MiscItemId.StratRedeployMission, 1 },
            { MiscItemId.SupportAttackMission, 1 },
            { MiscItemId.SupportDefenseMission, 1 },
            { MiscItemId.ReservesMission, 1 },
            { MiscItemId.AntiPartisanDutyMission, 1 },
            { MiscItemId.PlannedDefenseMission, 1 },
            { MiscItemId.AirSuperiorityMission, 1 },
            { MiscItemId.GroundAttackMission, 1 },
            { MiscItemId.InterdictionMission, 1 },
            { MiscItemId.StrategicBombardmentMission, 1 },
            { MiscItemId.LogisticalStrikeMission, 1 },
            { MiscItemId.RunwayCrateringMission, 1 },
            { MiscItemId.InstallationStrikeMission, 1 },
            { MiscItemId.NavalStrikeMission, 1 },
            { MiscItemId.PortStrikeMission, 1 },
            { MiscItemId.ConvoyAirRaidingMission, 1 },
            { MiscItemId.AirSupplyMission, 1 },
            { MiscItemId.AirborneAssaultMission, 1 },
            { MiscItemId.NukeMission, 1 },
            { MiscItemId.AirScrambleMission, 1 },
            { MiscItemId.ConvoyRadingMission, 1 },
            { MiscItemId.AswMission, 1 },
            { MiscItemId.NavalInterdictionMission, 1 },
            { MiscItemId.ShoreBombardmentMission, 1 },
            { MiscItemId.AmphibousAssaultMission, 1 },
            { MiscItemId.SeaTransportMission, 1 },
            { MiscItemId.NavalCombatPatrolMission, 1 },
            { MiscItemId.NavalPortStrikeMission, 1 },
            { MiscItemId.NavalAirbaseStrikeMission, 1 },
            { MiscItemId.SneakMoveMission, 1 },
            { MiscItemId.NavalScrambleMission, 1 },
            { MiscItemId.UseAttackEfficiencyCombatModifier, 1 },
            { MiscItemId.MaxActiveTechTeamsAoD, 20 },
            { MiscItemId.UseNewTechnologyPageLayout, 1 },
            { MiscItemId.TechOverviewPanelStyle, 1 },
            { MiscItemId.NewCountryRocketryComponent, 1 },
            { MiscItemId.NewCountryNuclearPhysicsComponent, 1 },
            { MiscItemId.NewCountryNuclearEngineeringComponent, 1 },
            { MiscItemId.NewCountrySecretTechs, 1 },
            { MiscItemId.DelayGameStartNewTrades, 0 },
            { MiscItemId.MergeTradeDeals, 1 },
            { MiscItemId.ManualTradeDeals, 100 },
            { MiscItemId.NewTradeDealsMinEffectiveness, 100 },
            { MiscItemId.CancelTradeDealsEffectiveness, 100 },
            { MiscItemId.AutoTradeAiTradeDeals, 100 },
            { MiscItemId.MaxSerialLineProductionGarrisonMilitia, 99 },
            { MiscItemId.AiPeacetimeSpyMissionsDh, 2 },
            { MiscItemId.NewDowRules, 1 },
            { MiscItemId.NewDowRules2, 2 },
            { MiscItemId.CountriesLeaveBadRelationAlliance, 1 },
            { MiscItemId.NewAiReleaseRules, 1 },
            { MiscItemId.AiEventsActionSelectionRules, 100 },
            { MiscItemId.UseQuickAreaCheckGarrisonAi, 1 },
            { MiscItemId.AiMastersGetProvincesConquredPuppets, 1 },
            { MiscItemId.ReleaseCountryWarZone, 1 },
            { MiscItemId.AiUnitPowerCalculationStrOrg, 1 },
            { MiscItemId.AiUnitPowerCalculationGde, 1 },
            { MiscItemId.AiUnitPowerCalculationMinOrg, 1 },
            { MiscItemId.AiSpyDiplomaticMissionLogger, 1 },
            { MiscItemId.SwitchedAiFilesLogger, 1 },
            { MiscItemId.UseNewAutoSaveFileFormat, 1 },
            { MiscItemId.LoadNewAiSwitchingAllClients, 1 },
            { MiscItemId.InGameLossesLogging, 1 },
            { MiscItemId.InGameLossLogging2, 4 },
            { MiscItemId.AllowBrigadeAttachingInSupply, 1 },
            { MiscItemId.AllowUniquePicturesAllLandProvinces, 1 },
            { MiscItemId.AutoReplyEvents, 1 },
            { MiscItemId.ForceActionsShow, 2 },
            { MiscItemId.EnableDicisionsPlayers, 1 },
            { MiscItemId.RebelsArmyComposition, 100 },
            { MiscItemId.RebelsArmyMinStr, 100 },
            { MiscItemId.RebelsArmyMaxStr, 100 },
            { MiscItemId.UseNewMinisterFilesFormat, 1 },
            { MiscItemId.EnableRetirementYearMinisters, 1 },
            { MiscItemId.EnableRetirementYearLeaders, 1 },
            { MiscItemId.LoadSpritesModdirOnly, 1 },
            { MiscItemId.LoadUnitIconsModdirOnly, 1 },
            { MiscItemId.LoadUnitPicturesModdirOnly, 1 },
            { MiscItemId.LoadAiFilesModdirOnly, 1 },
            { MiscItemId.UseSpeedSetGarrisonStatus, 1 },
            { MiscItemId.UseOldSaveGameFormat, 1 },
            { MiscItemId.ProductionPanelUiStyle, 1 },
            { MiscItemId.UnitPicturesSize, 1 },
            { MiscItemId.EnablePicturesNavalBrigades, 1 },
            { MiscItemId.BuildingsBuildableOnlyProvinces, 2 },
            { MiscItemId.CheatMultiPlayer, 1 },
            { MiscItemId.ManualChangeConvoy, 1 },
            { MiscItemId.BrigadesRepairManpowerCost, 1 },
            { MiscItemId.TotalProvinces, 10000 },
            { MiscItemId.DistanceCalculationModel, 1 }
        };

        /// <summary>
        ///     Minimum value of item (( Real number type )
        /// </summary>
        public static Dictionary<MiscItemId, double> DblMinValues = new Dictionary<MiscItemId, double>
        {
            { MiscItemId.DistanceModifierNeighbours, 0 },
            { MiscItemId.OverStockpileLimitDailyLoss, 0 },
            { MiscItemId.GearingBonusLossUpgradeUnit, 0 },
            { MiscItemId.GearingBonusLossUpgradeBrigade, 0 },
            { MiscItemId.MaxDailyDissent, 0 },
            { MiscItemId.DistanceModifierNeighboursDh, 0 },
            { MiscItemId.BombingEntrenchedArmiesModifier, 0 },
            { MiscItemId.ChanceEscortCarrier, 0 },
            { MiscItemId.AirCriticalHitChanceNavy, 0 },
            { MiscItemId.NavalCriticalHitChanceNavy, 0 },
            { MiscItemId.LandOrgDamageLandUrban, 0 },
            { MiscItemId.LandOrgDamageLandFort, 0 },
            { MiscItemId.SubStacksDetectionModifier, 0 },
            { MiscItemId.AaAirNightModifier, 0 },
            { MiscItemId.NukesMaxStrDamageFriendly, 0 },
            { MiscItemId.NukesOrgDamage, 0 },
            { MiscItemId.NukesOrgDamageNonFriendly, 0 },
            { MiscItemId.NightHoursWinter, 0 },
            { MiscItemId.NightHoursSpringFall, 0 },
            { MiscItemId.NightHoursSummer, 0 },
            { MiscItemId.FleetSizeRangePenaltyRatio, 0 },
            { MiscItemId.FleetSizeRangePenaltyMax, 0 },
            { MiscItemId.FleetPositioningFleetComposition, 0 },
            { MiscItemId.AttackStartingEfficiency, 0.05 },
            { MiscItemId.RebaseStartingEfficiency, 0.05 },
            { MiscItemId.StratRedeployStartingEfficiency, 0.05 },
            { MiscItemId.SupportAttackStartingEfficiency, 0.05 },
            { MiscItemId.SupportDefenseStartingEfficiency, 0.05 },
            { MiscItemId.ReservesStartingEfficiency, 0.05 },
            { MiscItemId.AntiPartisanDutyStartingEfficiency, 0.05 },
            { MiscItemId.PlannedDefenseStartingEfficiency, 0.05 },
            { MiscItemId.AirSuperiorityStartingEfficiency, 0.05 },
            { MiscItemId.GroundAttackStartingEfficiency, 0.05 },
            { MiscItemId.InterdictionStartingEfficiency, 0.05 },
            { MiscItemId.StrategicBombardmentStartingEfficiency, 0.05 },
            { MiscItemId.LogisticalStrikeStartingEfficiency, 0.05 },
            { MiscItemId.RunwayCrateringStartingEfficiency, 0.05 },
            { MiscItemId.InstallationStrikeStartingEfficiency, 0.05 },
            { MiscItemId.NavalStrikeStartingEfficiency, 0.05 },
            { MiscItemId.PortStrikeStartingEfficiency, 0.05 },
            { MiscItemId.ConvoyAirRaidingStartingEfficiency, 0.05 },
            { MiscItemId.AirSupplyStartingEfficiency, 0.05 },
            { MiscItemId.AirborneAssaultStartingEfficiency, 0.05 },
            { MiscItemId.NukeStartingEfficiency, 0.05 },
            { MiscItemId.AirScrambleStartingEfficiency, 0.05 },
            { MiscItemId.ConvoyRadingStartingEfficiency, 0.05 },
            { MiscItemId.AswStartingEfficiency, 0.05 },
            { MiscItemId.NavalInterdictionStartingEfficiency, 0.05 },
            { MiscItemId.ShoreBombardmentStartingEfficiency, 0.05 },
            { MiscItemId.AmphibousAssaultStartingEfficiency, 0.05 },
            { MiscItemId.SeaTransportStartingEfficiency, 0.05 },
            { MiscItemId.NavalCombatPatrolStartingEfficiency, 0.05 },
            { MiscItemId.NavalPortStrikeStartingEfficiency, 0.05 },
            { MiscItemId.NavalAirbaseStrikeStartingEfficiency, 0.05 },
            { MiscItemId.SneakMoveStartingEfficiency, 0.05 },
            { MiscItemId.NavalScrambleStartingEfficiency, 0.05 },
            { MiscItemId.CombatEventChances, 0 },
            { MiscItemId.NotProduceNewUnitsSupply, 0 },
            { MiscItemId.MilitaryStrengthTotalIcRatioPeacetime, 0 },
            { MiscItemId.MilitaryStrengthTotalIcRatioWartime, 0 },
            { MiscItemId.NotUseOffensiveOrgStrDamage, 0 },
            { MiscItemId.MinRequiredRelationsAlliedClaimed, -200 },
            { MiscItemId.StrPercentageBrigadesAttachment, 0 }
        };

        /// <summary>
        ///     Maximum value of item (( Real number type )
        /// </summary>
        public static Dictionary<MiscItemId, double> DblMaxValues = new Dictionary<MiscItemId, double>
        {
            { MiscItemId.DistanceModifierNeighbours, 1 },
            { MiscItemId.OverStockpileLimitDailyLoss, 1 },
            { MiscItemId.GearingBonusLossUpgradeUnit, 1 },
            { MiscItemId.GearingBonusLossUpgradeBrigade, 1 },
            { MiscItemId.MaxDailyDissent, 1 },
            { MiscItemId.DistanceModifierNeighboursDh, 1 },
            { MiscItemId.BombingEntrenchedArmiesModifier, 0.99 },
            { MiscItemId.ChanceEscortCarrier, 100 },
            { MiscItemId.AirCriticalHitChanceNavy, 100 },
            { MiscItemId.NavalCriticalHitChanceNavy, 100 },
            { MiscItemId.LandOrgDamageLandUrban, 1 },
            { MiscItemId.LandOrgDamageLandFort, 1 },
            { MiscItemId.SubStacksDetectionModifier, 1 },
            { MiscItemId.AaAirNightModifier, 1 },
            { MiscItemId.NukesMaxStrDamageFriendly, 1 },
            { MiscItemId.NukesOrgDamage, 1 },
            { MiscItemId.NukesOrgDamageNonFriendly, 1 },
            { MiscItemId.NightHoursWinter, 24 },
            { MiscItemId.NightHoursSpringFall, 24 },
            { MiscItemId.NightHoursSummer, 24 },
            { MiscItemId.FleetSizeRangePenaltyRatio, 1 },
            { MiscItemId.FleetSizeRangePenaltyMax, 1 },
            { MiscItemId.FleetPositioningFleetComposition, 1 },
            { MiscItemId.AttackStartingEfficiency, 10 },
            { MiscItemId.RebaseStartingEfficiency, 10 },
            { MiscItemId.StratRedeployStartingEfficiency, 10 },
            { MiscItemId.SupportAttackStartingEfficiency, 10 },
            { MiscItemId.SupportDefenseStartingEfficiency, 10 },
            { MiscItemId.ReservesStartingEfficiency, 10 },
            { MiscItemId.AntiPartisanDutyStartingEfficiency, 10 },
            { MiscItemId.PlannedDefenseStartingEfficiency, 10 },
            { MiscItemId.AirSuperiorityStartingEfficiency, 10 },
            { MiscItemId.GroundAttackStartingEfficiency, 10 },
            { MiscItemId.InterdictionStartingEfficiency, 10 },
            { MiscItemId.StrategicBombardmentStartingEfficiency, 10 },
            { MiscItemId.LogisticalStrikeStartingEfficiency, 10 },
            { MiscItemId.RunwayCrateringStartingEfficiency, 10 },
            { MiscItemId.InstallationStrikeStartingEfficiency, 10 },
            { MiscItemId.NavalStrikeStartingEfficiency, 10 },
            { MiscItemId.PortStrikeStartingEfficiency, 10 },
            { MiscItemId.ConvoyAirRaidingStartingEfficiency, 10 },
            { MiscItemId.AirSupplyStartingEfficiency, 10 },
            { MiscItemId.AirborneAssaultStartingEfficiency, 10 },
            { MiscItemId.NukeStartingEfficiency, 10 },
            { MiscItemId.AirScrambleStartingEfficiency, 10 },
            { MiscItemId.ConvoyRadingStartingEfficiency, 10 },
            { MiscItemId.AswStartingEfficiency, 10 },
            { MiscItemId.NavalInterdictionStartingEfficiency, 10 },
            { MiscItemId.ShoreBombardmentStartingEfficiency, 10 },
            { MiscItemId.AmphibousAssaultStartingEfficiency, 10 },
            { MiscItemId.SeaTransportStartingEfficiency, 10 },
            { MiscItemId.NavalCombatPatrolStartingEfficiency, 10 },
            { MiscItemId.NavalPortStrikeStartingEfficiency, 10 },
            { MiscItemId.NavalAirbaseStrikeStartingEfficiency, 10 },
            { MiscItemId.SneakMoveStartingEfficiency, 10 },
            { MiscItemId.NavalScrambleStartingEfficiency, 10 },
            { MiscItemId.CombatEventChances, 1 },
            { MiscItemId.NotProduceNewUnitsSupply, 1 },
            { MiscItemId.MilitaryStrengthTotalIcRatioPeacetime, 1 },
            { MiscItemId.MilitaryStrengthTotalIcRatioWartime, 1 },
            { MiscItemId.NotUseOffensiveOrgStrDamage, 1 },
            { MiscItemId.MinRequiredRelationsAlliedClaimed, 200 },
            { MiscItemId.StrPercentageBrigadesAttachment, 100 }
        };

        /// <summary>
        ///     item name
        /// </summary>
        public static readonly string[] ItemNames =
        {
            "IcToTcRatio",
            "IcToSuppliesRatio",
            "IcToConsumerGoodsRatio",
            "IcToMoneyRatio",
            "DissentChangeSpeed",
            "MinAvailableIc",
            "MinFinalIc",
            "DissentReduction",
            "MaxGearingBonus",
            "GearingBonusIncrement",
            "GearingResourceIncrement",
            "GearingLossNoIc",
            "IcMultiplierNonNational",
            "IcMultiplierNonOwned",
            "IcMultiplierPuppet",
            "ResourceMultiplierNonNational",
            "ResourceMultiplierNonOwned",
            "ResourceMultiplierNonNationalAi",
            "ResourceMultiplierPuppet",
            "TcLoadUndeployedDivision",
            "TcLoadOccupied",
            "TcLoadMultiplierLand",
            "TcLoadMultiplierAir",
            "TcLoadMultiplierNaval",
            "TcLoadPartisan",
            "TcLoadFactorOffensive",
            "TcLoadProvinceDevelopment",
            "TcLoadBase",
            "ManpowerMultiplierNational",
            "ManpowerMultiplierNonNational",
            "ManpowerMultiplierColony",
            "ManpowerMultiplierPuppet",
            "ManpowerMultiplierWartimeOversea",
            "ManpowerMultiplierPeacetime",
            "ManpowerMultiplierWartime",
            "DailyRetiredManpower",
            "RequirementAffectSlider",
            "TrickleBackFactorManpower",
            "ReinforceManpower",
            "ReinforceCost",
            "ReinforceTime",
            "UpgradeCost",
            "UpgradeTime",
            "ReinforceToUpdateModifier",
            "SupplyToUpdateModifier",
            "NationalismStartingValue",
            "NationalismPerManpowerAoD",
            "NationalismPerManpowerDh",
            "MaxNationalism",
            "MaxRevoltRisk",
            "MonthlyNationalismReduction",
            "SendDivisionDays",
            "TcLoadUndeployedBrigade",
            "CanUnitSendNonAllied",
            "SpyMissionDays",
            "IncreateIntelligenceLevelDays",
            "ChanceDetectSpyMission",
            "RelationshipsHitDetectedMissions",
            "ShowThirdCountrySpyReports",
            "DistanceModifierNeighbours",
            "SpyInformationAccuracyModifier",
            "AiPeacetimeSpyMissions",
            "MaxIcCostModifier",
            "AiSpyMissionsCostModifier",
            "AiDiplomacyCostModifier",
            "AiInfluenceModifier",
            "CostRepairBuildings",
            "TimeRepairBuilding",
            "ProvinceEfficiencyRiseTime",
            "CoreProvinceEfficiencyRiseTime",
            "LineUpkeep",
            "LineStartupTime",
            "LineUpgradeTime",
            "RetoolingCost",
            "RetoolingResource",
            "DailyAgingManpower",
            "SupplyConvoyHunt",
            "SupplyNavalStaticAoD",
            "SupplyNavalMoving",
            "SupplyNavalBattleAoD",
            "SupplyAirStaticAoD",
            "SupplyAirMoving",
            "SupplyAirBattleAoD",
            "SupplyAirBombing",
            "SupplyLandStaticAoD",
            "SupplyLandMoving",
            "SupplyLandBattleAoD",
            "SupplyLandBombing",
            "SupplyStockLand",
            "SupplyStockAir",
            "SupplyStockNaval",
            "RestockSpeedLand",
            "RestockSpeedAir",
            "RestockSpeedNaval",
            "SyntheticOilConversionMultiplier",
            "SyntheticRaresConversionMultiplier",
            "MilitarySalary",
            "MaxIntelligenceExpenditure",
            "MaxResearchExpenditure",
            "MilitarySalaryAttrictionModifier",
            "MilitarySalaryDissentModifier",
            "NuclearSiteUpkeepCost",
            "NuclearPowerUpkeepCost",
            "SyntheticOilSiteUpkeepCost",
            "SyntheticRaresSiteUpkeepCost",
            "DurationDetection",
            "ConvoyProvinceHostileTime",
            "ConvoyProvinceBlockedTime",
            "AutoTradeConvoy",
            "SpyUpkeepCost",
            "SpyDetectionChance",
            "SpyCoupDissentModifier",
            "InfraEfficiencyModifier",
            "ManpowerToConsumerGoods",
            "TimeBetweenSliderChangesAoD",
            "MinimalPlacementIc",
            "NuclearPower",
            "FreeInfraRepair",
            "MaxSliderDissent",
            "MinSliderDissent",
            "MaxDissentSliderMove",
            "IcConcentrationBonus",
            "TransportConversion",
            "ConvoyDutyConversion",
            "EscortDutyConversion",
            "MinisterChangeDelay",
            "MinisterChangeEventDelay",
            "IdeaChangeDelay",
            "IdeaChangeEventDelay",
            "LeaderChangeDelay",
            "ChangeIdeaDissent",
            "ChangeMinisterDissent",
            "MinDissentRevolt",
            "DissentRevoltMultiplier",
            "TpMaxAttach",
            "SsMaxAttach",
            "SsnMaxAttach",
            "DdMaxAttach",
            "ClMaxAttach",
            "CaMaxAttach",
            "BcMaxAttach",
            "BbMaxAttach",
            "CvlMaxAttach",
            "CvMaxAttach",
            "CanChangeIdeas",
            "CanUnitSendNonAlliedDh",
            "BluePrintsCanSoldNonAllied",
            "ProvinceCanSoldNonAllied",
            "TransferAlliedCoreProvinces",
            "ProvinceBuildingsRepairModifier",
            "ProvinceResourceRepairModifier",
            "StockpileLimitMultiplierResource",
            "StockpileLimitMultiplierSuppliesOil",
            "OverStockpileLimitDailyLoss",
            "MaxResourceDepotSize",
            "MaxSuppliesOilDepotSize",
            "DesiredStockPilesSuppliesOil",
            "MaxManpower",
            "ConvoyTransportsCapacity",
            "SuppyLandStaticDh",
            "SupplyLandBattleDh",
            "FuelLandStatic",
            "FuelLandBattle",
            "SupplyAirStaticDh",
            "SupplyAirBattleDh",
            "FuelAirNavalStatic",
            "FuelAirBattle",
            "SupplyNavalStaticDh",
            "SupplyNavalBattleDh",
            "FuelNavalNotMoving",
            "FuelNavalBattle",
            "TpTransportsConversionRatio",
            "DdEscortsConversionRatio",
            "ClEscortsConversionRatio",
            "CvlEscortsConversionRatio",
            "ProductionLineEdit",
            "GearingBonusLossUpgradeUnit",
            "GearingBonusLossUpgradeBrigade",
            "DissentNukes",
            "MaxDailyDissent",
            "NukesProductionModifier",
            "ConvoySystemOptionsAllied",
            "ResourceConvoysBackUnneeded",
            "SupplyDistanceModCalculationModifier",
            "SpyMissionDaysDh",
            "IncreateIntelligenceLevelDaysDh",
            "ChanceDetectSpyMissionDh",
            "RelationshipsHitDetectedMissionsDh",
            "DistanceModifier",
            "DistanceModifierNeighboursDh",
            "SpyLevelBonusDistanceModifier",
            "SpyLevelBonusDistanceModifierAboveTen",
            "SpyInformationAccuracyModifierDh",
            "IcModifierCost",
            "MinIcCostModifier",
            "MaxIcCostModifierDh",
            "ExtraMaintenanceCostAboveTen",
            "ExtraCostIncreasingAboveTen",
            "ShowThirdCountrySpyReportsDh",
            "SpiesMoneyModifier",
            "DaysBetweenDiplomaticMissions",
            "TimeBetweenSliderChangesDh",
            "RequirementAffectSliderDh",
            "UseMinisterPersonalityReplacing",
            "RelationshipHitCancelTrade",
            "RelationshipHitCancelPermanentTrade",
            "PuppetsJoinMastersAlliance",
            "MastersBecomePuppetsPuppets",
            "AllowManualClaimsChange",
            "BelligerenceClaimedProvince",
            "BelligerenceClaimsRemoval",
            "JoinAutomaticallyAllesAxis",
            "AllowChangeHosHog",
            "ChangeTagCoup",
            "FilterReleaseCountries",
            "ReturnOccupiedProvinces",
            "LandXpGainFactor",
            "NavalXpGainFactor",
            "AirXpGainFactor",
            "AirDogfightXpGainFactor",
            "DivisionXpGainFactor",
            "LeaderXpGainFactor",
            "AttritionSeverityModifier",
            "NoSupplyAttritionSeverity",
            "NoSupplyMinimunAttrition",
            "OutOfSupplyAttritionLand",
            "OutOfSupplyAttritionNaval",
            "OutOfSupplyAttritionAir",
            "LowestStrAttritionLosses",
            "BaseProximity",
            "ShoreBombardmentModifier",
            "ShoreBombardmentCap",
            "InvasionModifier",
            "MultipleCombatModifier",
            "OffensiveCombinedArmsBonus",
            "DefensiveCombinedArmsBonus",
            "SurpriseModifier",
            "LandCommandLimitModifier",
            "AirCommandLimitModifier",
            "NavalCommandLimitModifier",
            "EnvelopmentModifier",
            "EncircledModifier",
            "LandFortMultiplier",
            "CoastalFortMultiplier",
            "HardUnitsAttackingUrbanPenalty",
            "DissentMultiplier",
            "SupplyProblemsModifier",
            "SupplyProblemsModifierLand",
            "SupplyProblemsModifierAir",
            "SupplyProblemsModifierNaval",
            "FuelProblemsModifierLand",
            "FuelProblemsModifierAir",
            "FuelProblemsModifierNaval",
            "RaderStationMultiplier",
            "RaderStationAaMultiplier",
            "InterceptorBomberModifier",
            "AirOverstackingModifier",
            "AirOverstackingModifierAoD",
            "NavalOverstackingModifier",
            "BombingEntrenchedArmiesModifier",
            "DefendingEntrenchedArmiesModifier",
            "LandLeaderCommandLimitRank0",
            "LandLeaderCommandLimitRank1",
            "LandLeaderCommandLimitRank2",
            "LandLeaderCommandLimitRank3",
            "AirLeaderCommandLimitRank0",
            "AirLeaderCommandLimitRank1",
            "AirLeaderCommandLimitRank2",
            "AirLeaderCommandLimitRank3",
            "NavalLeaderCommandLimitRank0",
            "NavalLeaderCommandLimitRank1",
            "NavalLeaderCommandLimitRank2",
            "NavalLeaderCommandLimitRank3",
            "HqCommandLimitFactor",
            "ConvoyProtectionFactor",
            "ConvoyEscortsModel",
            "ConvoyTransportsModel",
            "DistanceModifierNeighboursDh",
            "ConvoyEscortCarrierModel",
            "DelayAfterCombatEnds",
            "LandDelayBeforeOrders",
            "NavalDelayBeforeOrders",
            "AirDelayBeforeOrders",
            "MaximumSizesAirStacks",
            "DurationAirToAirBattles",
            "DurationNavalPortBombing",
            "DurationStrategicBombing",
            "DurationGroundAttackBombing",
            "EffectExperienceCombat",
            "DamageNavalBasesBombing",
            "DamageAirBaseBombing",
            "DamageAaBombing",
            "DamageRocketBombing",
            "DamageNukeBombing",
            "DamageRadarBombing",
            "DamageInfraBombing",
            "DamageIcBombing",
            "DamageResourcesBombing",
            "DamageSyntheticOilBombing",
            "HowEffectiveGroundDef",
            "ChanceAvoidDefencesLeft",
            "ChanceAvoidNoDefences",
            "LandChanceAvoidDefencesLeft",
            "AirChanceAvoidDefencesLeft",
            "NavalChanceAvoidDefencesLeft",
            "ChanceAvoidAaDefencesLeft",
            "LandChanceAvoidNoDefences",
            "AirChanceAvoidNoDefences",
            "NavalChanceAvoidNoDefences",
            "ChanceAvoidAaNoDefences",
            "ChanceGetTerrainTrait",
            "ChanceGetEventTrait",
            "BonusTerrainTrait",
            "BonusSimilarTerrainTrait",
            "BonusEventTrait",
            "BonusLeaderSkillPointLand",
            "BonusLeaderSkillPointAir",
            "BonusLeaderSkillPointNaval",
            "ChanceLeaderDying",
            "AirOrgDamage",
            "AirStrDamageOrg",
            "AirStrDamage",
            "LandMinOrgDamage",
            "LandOrgDamageHardSoftEach",
            "LandOrgDamageHardVsSoft",
            "LandMinStrDamage",
            "LandStrDamageHardSoftEach",
            "LandStrDamageHardVsSoft",
            "AirMinOrgDamage",
            "AirAdditionalOrgDamage",
            "AirMinStrDamage",
            "AirAdditionalStrDamage",
            "AirStrDamageEntrenced",
            "NavalMinOrgDamage",
            "NavalAdditionalOrgDamage",
            "NavalMinStrDamage",
            "NavalAdditionalStrDamage",
            "AirOrgDamageLimitLand",
            "LandOrgDamageLimitAir",
            "AirOrgDamageLimitNavy",
            "NavalOrgDamageLimitAir",
            "AirOrgDamageLimitAa",
            "BasesOrgDamageHourCarriers",
            "BasesOrgDamageLimitCarriers",
            "BasesOrgDamageAfterCarriers",
            "BasesStrDamageCarriers",
            "AirCriticalHitChanceNavy",
            "AirCriticalHitModifierNavy",
            "NavalCriticalHitChanceNavy",
            "NavalCriticalHitModifierNavy",
            "AirStrDamageLandOrg",
            "AirStrDamageLandOrgDh104",
            "AirOrgDamageLandDh",
            "AirStrDamageLandDh",
            "LandOrgDamageLandOrg",
            "LandOrgDamageLandUrban",
            "LandOrgDamageLandFort",
            "RequiredLandFortSize",
            "LandStrDamageLandDh",
            "LandStrDamageLimitLand",
            "AirOrgDamageAirDh",
            "AirStrDamageAirDh",
            "LandOrgDamageAirDh",
            "LandStrDamageAirDh",
            "NavalOrgDamageAirDh",
            "NavalStrDamageAirDh",
            "SubsOrgDamageAir",
            "SubsStrDamageAir",
            "AirOrgDamageNavyDh",
            "AirStrDamageNavyDh",
            "NavalOrgDamageNavyDh",
            "NavalStrDamageNavyDh",
            "SubsOrgDamageNavy",
            "SubsStrDamageNavy",
            "SubsOrgDamage",
            "SubsStrDamage",
            "SubStacksDetectionModifier",
            "AirOrgDamageLandAoD",
            "AirStrDamageLandAoD",
            "LandDamageArtilleryBombardment",
            "InfraDamageArtilleryBombardment",
            "IcDamageArtilleryBombardment",
            "ResourcesDamageArtilleryBombardment",
            "PenaltyArtilleryBombardment",
            "ArtilleryStrDamage",
            "ArtilleryOrgDamage",
            "LandStrDamageLandAoD",
            "LandOrgDamageLand",
            "LandStrDamageAirAoD",
            "LandOrgDamageAirAoD",
            "NavalStrDamageAirAoD",
            "NavalOrgDamageAirAoD",
            "AirStrDamageAirAoD",
            "AirOrgDamageAirAoD",
            "NavalStrDamageNavyAoD",
            "NavalOrgDamageNavyAoD",
            "AirStrDamageNavyAoD",
            "AirOrgDamageNavyAoD",
            "MilitaryExpenseAttritionModifier",
            "NavalMinCombatTime",
            "LandMinCombatTime",
            "AirMinCombatTime",
            "LandOverstackingModifier",
            "LandOrgLossMoving",
            "AirOrgLossMoving",
            "NavalOrgLossMoving",
            "SupplyDistanceSeverity",
            "SupplyBase",
            "LandOrgGain",
            "AirOrgGain",
            "NavalOrgGain",
            "NukeManpowerDissent",
            "NukeIcDissent",
            "NukeTotalDissent",
            "LandFriendlyOrgGain",
            "AirLandStockModifier",
            "ScorchDamage",
            "StandGroundDissent",
            "ScorchGroundBelligerence",
            "DefaultLandStack",
            "DefaultNavalStack",
            "DefaultAirStack",
            "DefaultRocketStack",
            "FortDamageArtilleryBombardment",
            "ArtilleryBombardmentOrgCost",
            "LandDamageFort",
            "AirRebaseFactor",
            "AirMaxDisorganized",
            "AaInflictedStrDamage",
            "AaInflictedOrgDamage",
            "AaInflictedFlyingDamage",
            "AaInflictedBombingDamage",
            "HardAttackStrDamage",
            "HardAttackOrgDamage",
            "ArmorSoftBreakthroughMin",
            "ArmorSoftBreakthroughMax",
            "NavalCriticalHitChance",
            "NavalCriticalHitEffect",
            "LandFortDamage",
            "PortAttackSurpriseChanceDay",
            "PortAttackSurpriseChanceNight",
            "PortAttackSurpriseModifier",
            "RadarAntiSurpriseChance",
            "RadarAntiSurpriseModifier",
            "CounterAttackStrDefenderAoD",
            "CounterAttackOrgDefenderAoD",
            "CounterAttackStrAttackerAoD",
            "CounterAttackOrgAttackerAoD",
            "AssaultStrDefenderAoD",
            "AssaultOrgDefenderAoD",
            "AssaultStrAttackerAoD",
            "AssaultOrgAttackerAoD",
            "EncirclementStrDefenderAoD",
            "EncirclementOrgDefenderAoD",
            "EncirclementStrAttackerAoD",
            "EncirclementOrgAttackerAoD",
            "AmbushStrDefenderAoD",
            "AmbushOrgDefenderAoD",
            "AmbushStrAttackerAoD",
            "AmbushOrgAttackerAoD",
            "DelayStrDefenderAoD",
            "DelayOrgDefenderAoD",
            "DelayStrAttackerAoD",
            "DelayOrgAttackerAoD",
            "TacticalWithdrawStrDefenderAoD",
            "TacticalWithdrawOrgDefenderAoD",
            "TacticalWithdrawStrAttackerAoD",
            "TacticalWithdrawOrgAttackerAoD",
            "BreakthroughStrDefenderAoD",
            "BreakthroughOrgDefenderAoD",
            "BreakthroughStrAttackerAoD",
            "BreakthroughOrgAttackerAoD",
            "AaMinOrgDamage",
            "AaAdditionalOrgDamage",
            "AaMinStrDamage",
            "AaAdditionalStrDamage",
            "NavalOrgDamageAa",
            "AirOrgDamageAa",
            "AirStrDamageAa",
            "AaAirFiringRules",
            "AaAirNightModifier",
            "AaAirBonusRadars",
            "NukesStrDamage",
            "NukesMaxStrDamageFriendly",
            "NukesOrgDamage",
            "NukesOrgDamageNonFriendly",
            "NavalBombardmentChanceDamaged",
            "NavalBombardmentChanceBest",
            "TacticalBombardmentChanceDamaged",
            "MovementBonusTerrainTrait",
            "MovementBonusSimilarTerrainTrait",
            "LogisticsWizardEseBonus",
            "OffensiveSupplyESEBonus",
            "DaysOffensiveSupply",
            "MinisterBonuses",
            "OrgRegainBonusFriendly",
            "OrgRegainBonusFriendlyCap",
            "NewOrgRegainLogic",
            "OrgRegainMorale",
            "OrgRegainClear",
            "OrgRegainFrozen",
            "OrgRegainRaining",
            "OrgRegainSnowing",
            "OrgRegainStorm",
            "OrgRegainBlizzard",
            "OrgRegainMuddy",
            "OrgRegainNaval",
            "OrgRegainNavalOutOfFuel",
            "OrgRegainNavalOutOfSupplies",
            "OrgRegainNavalCurrent",
            "OrgRegainNavalBase",
            "OrgRegainNavalSea",
            "OrgRegainAir",
            "OrgRegainAirOutOfFuel",
            "OrgRegainAirOutOfSupplies",
            "OrgRegainAirCurrent",
            "OrgRegainAirBaseSize",
            "OrgRegainAirOutOfBase",
            "OrgRegainArmy",
            "OrgRegainArmyOutOfFuel",
            "OrgRegainArmyOutOfSupplies",
            "OrgRegainArmyCurrent",
            "OrgRegainArmyFriendly",
            "OrgRegainArmyTransportation",
            "OrgRegainArmyMoving",
            "OrgRegainArmyRetreating",
            "ConvoyInterceptionMissions",
            "AutoReturnTransportFleets",
            "AllowProvinceRegionTargeting",
            "NightHoursWinter",
            "NightHoursSpringFall",
            "NightHoursSummer",
            "RecalculateLandArrivalTimes",
            "SynchronizeArrivalTimePlayer",
            "SynchronizeArrivalTimeAi",
            "RecalculateArrivalTimesCombat",
            "LandSpeedModifierCombat",
            "LandSpeedModifierBombardment",
            "LandSpeedModifierSupply",
            "LandSpeedModifierOrg",
            "LandAirSpeedModifierFuel",
            "DefaultSpeedFuel",
            "FleetSizeRangePenaltyRatio",
            "FleetSizeRangePenaltyThrethold",
            "FleetSizeRangePenaltyMax",
            "ApplyRangeLimitsAreasRegions",
            "RadarBonusDetection",
            "BonusDetectionFriendly",
            "ScreensCapitalRatioModifier",
            "ChanceTargetNoOrgLand",
            "ScreenCapitalShipsTargeting",
            "FleetPositioningDaytime",
            "FleetPositioningLeaderSkill",
            "FleetPositioningFleetSize",
            "FleetPositioningFleetComposition",
            "LandCoastalFortsDamage",
            "LandCoastalFortsMaxDamage",
            "MinSoftnessBrigades",
            "AutoRetreatOrg",
            "LandOrgNavalTransportation",
            "MaxLandDig",
            "DigIncreaseDay",
            "BreakthroughEncirclementMinSpeed",
            "BreakthroughEncirclementMaxChance",
            "BreakthroughEncirclementChanceModifier",
            "CombatEventDuration",
            "CounterAttackOrgAttackerDh",
            "CounterAttackStrAttackerDh",
            "CounterAttackOrgDefenderDh",
            "CounterAttackStrDefenderDh",
            "AssaultOrgAttackerDh",
            "AssaultStrAttackerDh",
            "AssaultOrgDefenderDh",
            "AssaultStrDefenderDh",
            "EncirclementOrgAttackerDh",
            "EncirclementStrAttackerDh",
            "EncirclementOrgDefenderDh",
            "EncirclementStrDefenderDh",
            "AmbushOrgAttackerDh",
            "AmbushStrAttackerDh",
            "AmbushOrgDefenderDh",
            "AmbushStrDefenderDh",
            "DelayOrgAttackerDh",
            "DelayStrAttackerDh",
            "DelayOrgDefenderDh",
            "DelayStrDefenderDh",
            "TacticalWithdrawOrgAttackerDh",
            "TacticalWithdrawStrAttackerDh",
            "TacticalWithdrawOrgDefenderDh",
            "TacticalWithdrawStrDefenderDh",
            "BreakthroughOrgAttackerDh",
            "BreakthroughStrAttackerDh",
            "BreakthroughOrgDefenderDh",
            "BreakthroughStrDefenderDh",
            "HqStrDamageBreakthrough",
            "CombatMode",
            "AttackMission",
            "AttackStartingEfficiency",
            "AttackSpeedBonus",
            "RebaseMission",
            "RebaseStartingEfficiency",
            "RebaseChanceDetected",
            "StratRedeployMission",
            "StratRedeployStartingEfficiency",
            "StratRedeployAddedValue",
            "StratRedeployDistanceMultiplier",
            "SupportAttackMission",
            "SupportAttackStartingEfficiency",
            "SupportAttackSpeedBonus",
            "SupportDefenseMission",
            "SupportDefenseStartingEfficiency",
            "SupportDefenseSpeedBonus",
            "ReservesMission",
            "ReservesStartingEfficiency",
            "ReservesSpeedBonus",
            "AntiPartisanDutyMission",
            "AntiPartisanDutyStartingEfficiency",
            "AntiPartisanDutySuppression",
            "PlannedDefenseMission",
            "PlannedDefenseStartingEfficiency",
            "AirSuperiorityMission",
            "AirSuperiorityStartingEfficiency",
            "AirSuperiorityDetection",
            "AirSuperiorityMinRequired",
            "GroundAttackMission",
            "GroundAttackStartingEfficiency",
            "GroundAttackOrgDamage",
            "GroundAttackStrDamage",
            "InterdictionMission",
            "InterdictionStartingEfficiency",
            "InterdictionOrgDamage",
            "InterdictionStrDamage",
            "StrategicBombardmentMission",
            "StrategicBombardmentStartingEfficiency",
            "LogisticalStrikeMission",
            "LogisticalStrikeStartingEfficiency",
            "RunwayCrateringMission",
            "RunwayCrateringStartingEfficiency",
            "InstallationStrikeMission",
            "InstallationStrikeStartingEfficiency",
            "NavalStrikeMission",
            "NavalStrikeStartingEfficiency",
            "PortStrikeMission",
            "PortStrikeStartingEfficiency",
            "ConvoyAirRaidingMission",
            "ConvoyAirRaidingStartingEfficiency",
            "AirSupplyMission",
            "AirSupplyStartingEfficiency",
            "AirborneAssaultMission",
            "AirborneAssaultStartingEfficiency",
            "NukeMission",
            "NukeStartingEfficiency",
            "AirScrambleMission",
            "AirScrambleStartingEfficiency",
            "AirScrambleDetection",
            "AirScrambleMinRequired",
            "ConvoyRadingMission",
            "ConvoyRadingStartingEfficiency",
            "ConvoyRadingRangeModifier",
            "ConvoyRadingChanceDetected",
            "AswMission",
            "AswStartingEfficiency",
            "NavalInterdictionMission",
            "NavalInterdictionStartingEfficiency",
            "ShoreBombardmentMission",
            "ShoreBombardmentStartingEfficiency",
            "ShoreBombardmentModifierDh",
            "AmphibousAssaultMission",
            "AmphibousAssaultStartingEfficiency",
            "SeaTransportMission",
            "SeaTransportStartingEfficiency",
            "SeaTransportRangeModifier",
            "SeaTransportChanceDetected",
            "NavalCombatPatrolMission",
            "NavalCombatPatrolStartingEfficiency",
            "NavalPortStrikeMission",
            "NavalPortStrikeStartingEfficiency",
            "NavalAirbaseStrikeMission",
            "NavalAirbaseStrikeStartingEfficiency",
            "SneakMoveMission",
            "SneakMoveStartingEfficiency",
            "SneakMoveRangeModifier",
            "SneakMoveChanceDetected",
            "NavalScrambleMission",
            "NavalScrambleStartingEfficiency",
            "NavalScrambleSpeedBonus",
            "UseAttackEfficiencyCombatModifier",
            "LandFortEfficiency",
            "CoastalFortEfficiency",
            "GroundDefenseEfficiency",
            "ConvoyDefenseEfficiency",
            "ManpowerBoost",
            "TransportCapacityModifier",
            "OccupiedTransportCapacityModifier",
            "AttritionModifier",
            "ManpowerTrickleBackModifier",
            "SupplyDistanceModifier",
            "RepairModifier",
            "ResearchModifier",
            "RadarEfficiency",
            "HqSupplyEfficiencyBonus",
            "HqCombatEventsBonus",
            "CombatEventChances",
            "FriendlyArmyDetectionChance",
            "EnemyArmyDetectionChance",
            "FriendlyIntelligenceChance",
            "EnemyIntelligenceChance",
            "MaxAmphibiousArmySize",
            "EnergyToOil",
            "TotalProductionEfficiency",
            "OilProductionEfficiency",
            "MetalProductionEfficiency",
            "EnergyProductionEfficiency",
            "RareMaterialsProductionEfficiency",
            "MoneyProduction Efficiency",
            "SupplyProductionEfficiency",
            "AaPower",
            "AirSurpriseChance",
            "LandSurpriseChance",
            "NavalSurpriseChance",
            "PeacetimeIcModifier",
            "WartimeIcModifier",
            "BuildingsProductionModifier",
            "ConvoysProductionModifier",
            "MinShipsPositioningBattle",
            "MaxShipsPositioningBattle",
            "PeacetimeStockpilesResources",
            "WartimeStockpilesResources",
            "PeacetimeStockpilesOilSupplies",
            "WartimeStockpilesOilSupplies",
            "MaxLandDigDH105",
            "DigIncreaseDayDH105",
            "BlueprintBonus",
            "PreHistoricalDateModifier",
            "PostHistoricalDateModifierDh",
            "CostSkillLevel",
            "MeanNumberInventionEventsYear",
            "PostHistoricalDateModifierAoD",
            "TechSpeedModifier",
            "PreHistoricalPenaltyLimit",
            "PostHistoricalBonusLimit",
            "MaxActiveTechTeamsAoD",
            "RequiredIcEachTechTeamAoD",
            "MaximumRandomModifier",
            "UseNewTechnologyPageLayout",
            "TechOverviewPanelStyle",
            "MaxActiveTechTeamsDh",
            "MinActiveTechTeams",
            "RequiredIcEachTechTeamDh",
            "NewCountryRocketryComponent",
            "NewCountryNuclearPhysicsComponent",
            "NewCountryNuclearEngineeringComponent",
            "NewCountrySecretTechs",
            "MaxTechTeamSkill",
            "DaysTradeOffers",
            "DelayGameStartNewTrades",
            "LimitAiNewTradesGameStart",
            "DesiredOilStockpile",
            "CriticalOilStockpile",
            "DesiredSuppliesStockpile",
            "CriticalSuppliesStockpile",
            "DesiredResourcesStockpile",
            "CriticalResourceStockpile",
            "WartimeDesiredStockpileMultiplier",
            "PeacetimeExtraOilImport",
            "WartimeExtraOilImport",
            "ExtraImportBelowDesired",
            "PercentageProducedSupplies",
            "PercentageProducedMoney",
            "ExtraImportStockpileSelected",
            "DaysDeliverResourcesTrades",
            "MergeTradeDeals",
            "ManualTradeDeals",
            "PuppetsSendSuppliesMoney",
            "PuppetsCriticalSupplyStockpile",
            "PuppetsMaxPoolResources",
            "NewTradeDealsMinEffectiveness",
            "CancelTradeDealsEffectiveness",
            "AutoTradeAiTradeDeals",
            "OverproduceSuppliesBelowDesired",
            "MultiplierOverproduceSuppliesWar",
            "NotProduceSuppliesStockpileOver",
            "MaxSerialLineProductionGarrisonMilitia",
            "MinIcSerialProductionNavalAir",
            "NotProduceNewUnitsManpowerRatio",
            "NotProduceNewUnitsManpowerValue",
            "NotProduceNewUnitsSupply",
            "MilitaryStrengthTotalIcRatioPeacetime",
            "MilitaryStrengthTotalIcRatioWartime",
            "MilitaryStrengthTotalIcRatioMajor",
            "NotUseOffensiveSupplyStockpile",
            "NotUseOffensiveOilStockpile",
            "NotUseOffensiveEse",
            "NotUseOffensiveOrgStrDamage",
            "AiPeacetimeSpyMissionsDh",
            "AiSpyMissionsCostModifierDh",
            "AiDiplomacyCostModifierDh",
            "AiInfluenceModifierDh",
            "NewDowRules",
            "NewDowRules2",
            "ForcePuppetsJoinMastersAllianceNeutrality",
            "CountriesLeaveBadRelationAlliance",
            "NewAiReleaseRules",
            "AiEventsActionSelectionRules",
            "ForceStrategicRedeploymentHour",
            "MaxRedeploymentDaysAi",
            "UseQuickAreaCheckGarrisonAi",
            "AiMastersGetProvincesConquredPuppets",
            "ReleaseCountryWarZone",
            "MinDaysRequiredAiReleaseCountry",
            "MinDaysRequiredAiAllied",
            "MinDaysRequiredAiAlliedSupplyBase",
            "MinRequiredRelationsAlliedClaimed",
            "AiUnitPowerCalculationStrOrg",
            "AiUnitPowerCalculationGde",
            "AiUnitPowerCalculationMinOrg",
            "AiUnitPowerCalculationMinStr",
            "AiSpyDiplomaticMissionLogger",
            "CountryLogger",
            "SwitchedAiFilesLogger",
            "UseNewAutoSaveFileFormat",
            "LoadNewAiSwitchingAllClients",
            "TradeEfficiencyCalculationSystem",
            "MergeRelocateProvincialDepots",
            "InGameLossesLogging",
            "InGameLossLogging2",
            "AllowBrigadeAttachingInSupply",
            "MultipleDeploymentSizeArmies",
            "MultipleDeploymentSizeFleets",
            "MultipleDeploymentSizeAir",
            "AllowUniquePicturesAllLandProvinces",
            "AutoReplyEvents",
            "ForceActionsShow",
            "EnableDicisionsPlayers",
            "RebelsArmyComposition",
            "RebelsArmyTechLevel",
            "RebelsArmyMinStr",
            "RebelsArmyMaxStr",
            "RebelsOrgRegain",
            "ExtraRebelBonusNeighboringProvince",
            "ExtraRebelBonusOccupied",
            "ExtraRebelBonusMountain",
            "ExtraRebelBonusHill",
            "ExtraRebelBonusForest",
            "ExtraRebelBonusJungle",
            "ExtraRebelBonusSwamp",
            "ExtraRebelBonusDesert",
            "ExtraRebelBonusPlain",
            "ExtraRebelBonusUrban",
            "ExtraRebelBonusAirNavalBases",
            "ReturnRebelliousProvince",
            "UseNewMinisterFilesFormat",
            "EnableRetirementYearMinisters",
            "EnableRetirementYearLeaders",
            "LoadSpritesModdirOnly",
            "LoadUnitIconsModdirOnly",
            "LoadUnitPicturesModdirOnly",
            "LoadAiFilesModdirOnly",
            "UseSpeedSetGarrisonStatus",
            "UseOldSaveGameFormat",
            "ProductionPanelUiStyle",
            "UnitPicturesSize",
            "EnablePicturesNavalBrigades",
            "BuildingsBuildableOnlyProvinces",
            "UnitModifiersStatisticsPages",
            "CheatMultiPlayer",
            "ManualChangeConvoy",
            "BrigadesRepairManpowerCost",
            "StrPercentageBrigadesAttachment",
            "MapNumber",
            "TotalProvinces",
            "DistanceCalculationModel",
            "MapWidth",
            "MapHeight"
        };

        #endregion

        #region Internal constant

        /// <summary>
        ///     Section name
        /// </summary>
        private static readonly string[] SectionNames =
        {
            "Economy",
            "Intelligence",
            "Diplomacy",
            "Combat",
            "Mission",
            "Country",
            "Research",
            "Trade",
            "Ai",
            "Mod",
            "Map"
        };

        #endregion

        #region File reading

        /// <summary>
        ///     misc Request file reload
        /// </summary>
        /// <remarks>
        ///     Game folder, MOD Call when the name or game type changes
        /// </ remarks>
        public static void RequestReload()
        {
            _loaded = false;
        }

        /// <summary>
        ///     misc Reload the file
        /// </summary>
        public static void Reload()
        {
            // Do nothing before loading
            if (!_loaded)
            {
                return;
            }

            _loaded = false;

            Load();
        }


        /// <summary>
        ///     misc Read the file
        /// </summary>
        public static void Load()
        {
            // Back if loaded
            if (_loaded)
            {
                return;
            }

            // Initialize the setting value
            _items = new object[Enum.GetValues(typeof (MiscItemId)).Length];

            // Initialize the comment
            _comments = new string[Enum.GetValues(typeof (MiscItemId)).Length];

            // Initialize the string at the end of the section
            _suffixes = new string[Enum.GetValues(typeof (MiscSectionId)).Length];

            // miscInterpret the file
            string fileName = Game.GetReadFileName(Game.MiscPathName);
            Log.Verbose("[Misc] Load: {0}", Path.GetFileName(fileName));
            try
            {
                MiscParser.Parse(fileName);
            }
            catch (Exception)
            {
                Log.Error("[Misc] Read error: {0}", fileName);
                MessageBox.Show($"{Resources.FileReadError}: {fileName}",
                    Resources.EditorMisc, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Clear all edited flags
            ResetDirtyAll();

            // Set the read flag
            _loaded = true;
        }

        #endregion

        #region File writing

        /// <summary>
        ///     misc Save the file
        /// </summary>
        /// <returns>If saving fails false false return it</returns>
        public static bool Save()
        {
            // Do nothing if not edited
            if (!IsDirty())
            {
                return true;
            }

            string fileName = Game.GetWriteFileName(Game.MiscPathName);
            try
            {
                // db db. If there is no folder, create it
                string folderName = Game.GetWriteFileName(Game.DatabasePathName);
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                // misc Save the file
                Log.Info("[Misc] Save: {0}", Path.GetFileName(fileName));
                MiscWriter.Write(fileName);
            }
            catch (Exception)
            {
                Log.Error("[Misc] Write error: {0}", fileName);
                MessageBox.Show($"{Resources.FileWriteError}: {fileName}",
                    Resources.EditorMisc, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Clear all edited flags
            ResetDirtyAll();

            return true;
        }

        #endregion

        #region Setting item operation

        /// <summary>
        ///     Get the value of an item
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>Item value</returns>
        public static object GetItem(MiscItemId id)
        {
            return _items[(int) id];
        }

        /// <summary>
        ///     Set the value of the item
        /// </summary>
        /// <param name="id">item ID</param>
        /// <param name="o">Item value</param>
        public static void SetItem(MiscItemId id, object o)
        {
            _items[(int) id] = o;
        }

        /// <summary>
        ///     Get the value of an item as a boolean value
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>Item value</returns>
        public static bool GetBool(MiscItemId id)
        {
            object item = GetItem(id);

            if (item is bool)
            {
                return (bool) item;
            }

            return false;
        }

        /// <summary>
        ///     Get the value of an item as an integer value
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>Item value</returns>
        public static int GetInt(MiscItemId id)
        {
            object item = GetItem(id);
            int n = 0;

            if (item is int)
            {
                n = (int) item;
            }
            else if (item is double)
            {
                n = (int) (double) item;
            }

            // For enumeration type, round the value if it is not between the minimum value and the maximum value.
            if (ItemTypes[(int) id] != MiscItemType.Enum)
            {
                if (IntMinValues.ContainsKey(id) && n < IntMinValues[id])
                {
                    n = IntMinValues[id];
                }
                else if (IntMaxValues.ContainsKey(id) && n > IntMaxValues[id])
                {
                    n = IntMaxValues[id];
                }
            }

            return n;
        }

        /// <summary>
        ///     Get the value of an item as a real number
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>Item value</returns>
        public static double GetDouble(MiscItemId id)
        {
            object item = GetItem(id);
            double d = 0;

            if (item is double)
            {
                d = (double) item;
            }
            else if (item is int)
            {
                d = (int) item;
            }

            return d;
        }

        /// <summary>
        ///     White text in the item / / Get comments
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>White text in the item / / comment</returns>
        public static string GetComment(MiscItemId id)
        {
            return _comments[(int) id];
        }

        /// <summary>
        ///     White text in the item / / Set a comment
        /// </summary>
        /// <param name="id">item ID</param>
        /// <param name="s">Character string to set</param>
        public static void SetComment(MiscItemId id, string s)
        {
            _comments[(int) id] = s;
        }

        /// <summary>
        ///     White space at the end of the section / / Get comments
        /// </summary>
        /// <param name="section">section ID</param>
        /// <returns>White space at the end of the section / / comment</returns>
        public static string GetSuffix(MiscSectionId section)
        {
            return _suffixes[(int) section];
        }

        /// <summary>
        ///     White space at the end of the section / / Set a comment
        /// </summary>
        /// <param name="section">section ID</param>
        /// <param name="s">Character string to set</param>
        public static void SetSuffix(MiscSectionId section, string s)
        {
            _suffixes[(int) section] = s;
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if it has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirty()
        {
            return _dirtyFlag;
        }

        /// <summary>
        ///     Get if the item has been edited
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>If editedtrue true return it</returns>
        public static bool IsDirty(MiscItemId id)
        {
            return DirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public static void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public static void SetDirty(MiscItemId id)
        {
            DirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public static void ResetDirtyAll()
        {
            foreach (MiscItemId id in Enum.GetValues(typeof (MiscItemId)))
            {
                DirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion

        #region String operation

        /// <summary>
        ///     Get the section name
        /// </summary>
        /// <param name="section">section</param>
        /// <returns>Section name</returns>
        public static string GetSectionName(MiscSectionId section)
        {
            return HoI2EditorController.GetResourceString("MiscSection" + SectionNames[(int) section]);
        }

        /// <summary>
        ///     Get the item name
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>item name</returns>
        public static string GetItemName(MiscItemId id)
        {
            return HoI2EditorController.GetResourceString("MiscLabel" + ItemNames[(int) id]);
        }

        /// <summary>
        ///     Get the tooltip string for an item
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>Tooltip string</returns>
        public static string GetItemToolTip(MiscItemId id)
        {
            return HoI2EditorController.GetResourceString("MiscToolTip" + ItemNames[(int) id]);
        }

        /// <summary>
        ///     Get the string of item choices
        /// </summary>
        /// <param name="id">item ID</param>
        /// <param name="index">Index of choices</param>
        /// <returns>Character string of choice</returns>
        public static string GetItemChoice(MiscItemId id, int index)
        {
            string s = IntHelper.ToString(index);
            return s + ": " + HoI2EditorController.GetResourceString("MiscEnum" + ItemNames[(int) id] + s);
        }

        /// <summary>
        ///     Get the item string
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>Character string</returns>
        /// <remarks> Bool / Enum Item is expressed as an integer </ remarks>
        public static string GetString(MiscItemId id)
        {
            switch (ItemTypes[(int) id])
            {
                case MiscItemType.Bool:
                    return GetBool(id) ? "1" : "0";

                case MiscItemType.Enum:
                case MiscItemType.Int:
                case MiscItemType.PosInt:
                case MiscItemType.NonNegInt:
                case MiscItemType.NonPosInt:
                case MiscItemType.NonNegIntMinusOne:
                case MiscItemType.RangedInt:
                case MiscItemType.RangedPosInt:
                case MiscItemType.RangedIntMinusOne:
                case MiscItemType.RangedIntMinusThree:
                    return IntHelper.ToString0(GetInt(id));

                case MiscItemType.NonNegInt1:
                    return IntHelper.ToString1(GetInt(id));

                case MiscItemType.Dbl:
                case MiscItemType.PosDbl:
                case MiscItemType.NonNegDbl:
                case MiscItemType.NonPosDbl:
                case MiscItemType.NonNegDblMinusOne1:
                case MiscItemType.RangedDbl:
                case MiscItemType.RangedDblMinusOne1:
                    return DoubleHelper.ToString1(GetDouble(id));

                case MiscItemType.NonNegDbl0:
                case MiscItemType.NonPosDbl0:
                case MiscItemType.RangedDbl0:
                    return DoubleHelper.ToString0(GetDouble(id));

                case MiscItemType.NonNegDbl2:
                case MiscItemType.NonPosDbl2:
                    return DoubleHelper.ToString2(GetDouble(id));

                case MiscItemType.NonNegDbl5:
                    return DoubleHelper.ToString5(GetDouble(id));

                case MiscItemType.NonNegDblMinusOne:
                case MiscItemType.RangedDblMinusOne:
                    return GetDbl1MinusOneString(GetDouble(id));

                case MiscItemType.NonNegDbl2AoD:
                    return GetDbl1AoD2String(GetDouble(id));

                case MiscItemType.NonNegDbl4Dda13:
                    return GetDbl1Dda134String(GetDouble(id));

                case MiscItemType.NonNegDbl2Dh103Full:
                    return GetDbl1Range2String(GetDouble(id), 0, 0.1000005);

                case MiscItemType.NonNegDbl2Dh103Full1:
                    return GetDbl2Range1String(GetDouble(id), 0, 0.2000005);

                case MiscItemType.NonNegDbl2Dh103Full2:
                    return GetDbl1Range2String(GetDouble(id), 0, 1);

                case MiscItemType.NonPosDbl5AoD:
                    return GetDbl1AoD5String(GetDouble(id));

                case MiscItemType.NonPosDbl2Dh103Full:
                    return GetDbl1Range2String(GetDouble(id), -0.1000005, 0);

                case MiscItemType.NonNegIntNegDbl:
                    return GetNonNegIntNegDblString(GetDouble(id));
            }

            return string.Empty;
        }

        /// <summary>
        ///     Get a string (( Real number / / After the decimal point 1 digit or -1)
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Character string</returns>
        private static string GetDbl1MinusOneString(double val)
        {
            return DoubleHelper.IsEqual(val, -1) ? "-1" : DoubleHelper.ToString1(val);
        }

        /// <summary>
        ///     Get a string (( Real number / / After the decimal point 1 digit /DDA1.3 or DH Only after the decimal point Four digit )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Character string</returns>
        private static string GetDbl1Dda134String(double val)
        {
            return ((Game.Type == GameType.HeartsOfIron2) && (Game.Version >= 130)) ||
                   (Game.Type == GameType.DarkestHour)
                ? DoubleHelper.ToString4(val)
                : DoubleHelper.ToString1(val);
        }

        /// <summary>
        ///     Get a string (( Real number / / After the decimal point 1 digit / AoD Only after the decimal point 2 digit )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Character string</returns>
        private static string GetDbl1AoD2String(double val)
        {
            return Game.Type == GameType.ArsenalOfDemocracy
                ? DoubleHelper.ToString2(val)
                : DoubleHelper.ToString1(val);
        }

        /// <summary>
        ///     Get a string (( Real number / / After the decimal point 1 digit / AoD Only after the decimal point Five digit )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Character string</returns>
        private static string GetDbl1AoD5String(double val)
        {
            return Game.Type == GameType.ArsenalOfDemocracy
                ? DoubleHelper.ToString5(val)
                : DoubleHelper.ToString1(val);
        }

        /// <summary>
        ///     Get a string (( Real number / / After the decimal point 1 digit / / If it is within the specified range, it is after the decimal point 2 digit )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <param name="min">Minimum value in range</param>
        /// <param name="max">Maximum value in range</param>
        /// <returns>Character string</returns>
        private static string GetDbl1Range2String(double val, double min, double max)
        {
            return val > min && val < max ? DoubleHelper.ToString2(val) : DoubleHelper.ToString1(val);
        }

        /// <summary>
        ///     Get a string (( Real number / / After the decimal point 2 digit / / If it is out of the specified range, it is after the decimal point 1 digit )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Character string</returns>
        /// <param name="min">Minimum value in range</param>
        /// <param name="max">Maximum value in range</param>
        private static string GetDbl2Range1String(double val, double min, double max)
        {
            return val > min && val < max ? DoubleHelper.ToString2(val) : DoubleHelper.ToString1(val);
        }

        /// <summary>
        ///     Get a string (( Non-negative integer or Negative real number )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Character string</returns>
        private static string GetNonNegIntNegDblString(double val)
        {
            return val < 0 ? DoubleHelper.ToString1(val) : IntHelper.ToString0((int) val);
        }

        #endregion

        #region Game version

        /// <summary>
        ///     misc Get the file type
        /// </summary>
        /// <returns></returns>
        public static MiscGameType GetGameType()
        {
            switch (Game.Type)
            {
                case GameType.HeartsOfIron2:
                    return Game.Version >= 130 ? MiscGameType.Dda13 : MiscGameType.Dda12;

                case GameType.ArsenalOfDemocracy:
                    return Game.Version >= 108
                        ? MiscGameType.Aod108
                        : (Game.Version <= 104 ? MiscGameType.Aod104 : MiscGameType.Aod107);

                case GameType.DarkestHour:
                    if ( Game.Version >= 105)
                    {
                        return MiscGameType.Dh105 ;
                    } else if ( Game.Version == 103 ) {
                        return MiscGameType.Dh103 ;
                    } else {
                        return MiscGameType.Dh102 ;
                    }
            }
            return MiscGameType.Dda12;
        }

        #endregion
    }

    /// <summary>
    ///     misc item ID
    /// </summary>
    public enum MiscItemId
    {
        // economy
        IcToTcRatio, // I C from TC Conversion efficiency to
        IcToSuppliesRatio, // I C Conversion efficiency from goods to goods
        IcToConsumerGoodsRatio, // I C Conversion efficiency from to consumer goods
        IcToMoneyRatio, // I C Conversion efficiency from to funds
        DissentChangeSpeed, // Dissatisfaction reduction rate
        MinAvailableIc, // Minimum effective I C Ratio
        MinFinalIc, // Minimum effective I C
        DissentReduction, // Dissatisfaction reduction correction
        MaxGearingBonus, // Maximum gearing bonus
        GearingBonusIncrement, // Increased gearing bonus
        GearingResourceIncrement, // Increased resource consumption during continuous production
        GearingLossNoIc, // I C Gearing bonus reduction value when there is a shortage
        IcMultiplierNonNational, // Non-core state I C correction
        IcMultiplierNonOwned, // Of the occupied territory I C correction
        IcMultiplierPuppet, // Of the client state I C correction
        ResourceMultiplierNonNational, // Resource correction for non-core states
        ResourceMultiplierNonOwned, // Resource correction for occupied territories
        ResourceMultiplierNonNationalAi, // Resource correction for non-core states (AI)
        ResourceMultiplierPuppet, // Resource correction of the country of origin
        TcLoadUndeployedDivision, // Of undeployed divisions TC load
        TcLoadOccupied, // Of the occupied territories TC load
        TcLoadMultiplierLand, // Of the Army Division TC Load compensation
        TcLoadMultiplierAir, // Of the Air Force Division TC Load compensation
        TcLoadMultiplierNaval, // Of the Navy Division TC Load compensation
        TcLoadPartisan, // Partisan TC load
        TcLoadFactorOffensive, // At the time of offensive TC Load coefficient
        TcLoadProvinceDevelopment, // Of Providence development TC load
        TcLoadBase, // Of undeployed basesTC load
        ManpowerMultiplierNational, // Human resource correction in the core state
        ManpowerMultiplierNonNational, // Human resource correction for non-core states
        ManpowerMultiplierColony, // Human resource correction for overseas provinces
        ManpowerMultiplierPuppet, // Human resource correction of the country of origin
        ManpowerMultiplierWartimeOversea, // Human resource correction of foreign states during the war
        ManpowerMultiplierPeacetime, // Human resource correction in peacetime
        ManpowerMultiplierWartime, // Wartime human resource correction
        DailyRetiredManpower, // Aging rate of human resources
        RequirementAffectSlider, // To influence the policy slider I C ratio
        TrickleBackFactorManpower, // Recovery coefficient from loss due to battle
        ReinforceManpower, // Ratio of human resources required for replenishment
        ReinforceCost, // Necessary for replenishment I C Ratio
        ReinforceTime, // Percentage of time required for replenishment
        UpgradeCost, // Needed for improvement I C Ratio
        UpgradeTime, // Percentage of time required for improvement
        ReinforceToUpdateModifier, // Replenishment factor for improvement
        SupplyToUpdateModifier, // Material factor for improvement
        NationalismStartingValue, // Initial value of nationalism
        NationalismPerManpowerAoD, // Correction value of nationalism by human resources
        NationalismPerManpowerDh, // Correction value of nationalism by human resources
        MaxNationalism, // Maximum nationalism
        MaxRevoltRisk, // Maximum rebellion rate
        MonthlyNationalismReduction, // Monthly reduction in nationalism
        SendDivisionDays, // Time to deploy after transfer of division
        TcLoadUndeployedBrigade, // Undeployed brigade TC load
        CanUnitSendNonAllied, // Sold the division to a non-allied country / / assignment
        SpyMissionDays, // Intelligence mission intervals
        IncreateIntelligenceLevelDays, // Intelligence level increase interval
        ChanceDetectSpyMission, // Probability of discovering domestic intelligence activity
        RelationshipsHitDetectedMissions, // Amount of decrease in friendship when an intelligence mission is discovered
        ShowThirdCountrySpyReports, // Report on intelligence activities in third countries
        DistanceModifierNeighbours, // Neighboring country correction of intelligence mission
        SpyInformationAccuracyModifier, // Information accuracy correction
        AiPeacetimeSpyMissions, // In normal times AI Aggressive intelligence activity
        MaxIcCostModifier, // Maximum intelligence cost compensation I C
        AiSpyMissionsCostModifier, // AI Intelligence cost compensation
        AiDiplomacyCostModifier, // AI Diplomatic cost correction
        AiInfluenceModifier, // AI Diplomatic interference frequency correction
        CostRepairBuildings, // Building repair cost compensation
        TimeRepairBuilding, // Building repair time correction
        ProvinceEfficiencyRiseTime, // Providence efficiency increase time
        CoreProvinceEfficiencyRiseTime, // Core Providence Efficiency Increase Time
        LineUpkeep, // Line maintenance cost correction
        LineStartupTime, // Line start time
        LineUpgradeTime, // Line improvement time
        RetoolingCost, // Line adjustment cost correction
        RetoolingResource, // Line adjustment resource correction
        DailyAgingManpower, // Human resource aging correction
        SupplyConvoyHunt, // Correction of material usage when attacking the fleet
        SupplyNavalStaticAoD, // Navy standby material usage correction
        SupplyNavalMoving, // Navy travel material usage correction
        SupplyNavalBattleAoD, // Navy combat material usage correction
        SupplyAirStaticAoD, // Air Force standby supplies usage correction
        SupplyAirMoving, // Correction of the amount of supplies used when moving by the Air Force
        SupplyAirBattleAoD, // Air Force Combat Material Usage Correction
        SupplyAirBombing, // Correction of material usage during bombing of the Air Force
        SupplyLandStaticAoD, // Army standby material usage correction
        SupplyLandMoving, // Correction of the amount of supplies used when moving by the Army
        SupplyLandBattleAoD, // Army combat material usage correction
        SupplyLandBombing, // Army artillery material usage correction
        SupplyStockLand, // Army supplies
        SupplyStockAir, // Air Force stockpile
        SupplyStockNaval, // Navy supplies
        RestockSpeedLand, // Army restocking speed
        RestockSpeedAir, // Air Force stockpile speed
        RestockSpeedNaval, // Navy restocking speed
        SyntheticOilConversionMultiplier, // Synthetic petroleum conversion factor
        SyntheticRaresConversionMultiplier, // Synthetic rare resource conversion coefficient
        MilitarySalary, // Army salary
        MaxIntelligenceExpenditure, // Maximum intelligence ratio
        MaxResearchExpenditure, // Maximum research expense ratio
        MilitarySalaryAttrictionModifier, // Consumption compensation when the army's salary is insufficient
        MilitarySalaryDissentModifier, // Dissatisfaction correction when the army's salary is insufficient
        NuclearSiteUpkeepCost, // Reactor maintenance cost
        NuclearPowerUpkeepCost, // Nuclear power plant maintenance cost
        SyntheticOilSiteUpkeepCost, // Synthetic oil factory maintenance cost
        SyntheticRaresSiteUpkeepCost, // Synthetic rare resource factory maintenance cost
        DurationDetection, // Duration of Navy Information
        ConvoyProvinceHostileTime, // Fleet attack avoidance time
        ConvoyProvinceBlockedTime, // Fleet attack obstruction time
        AutoTradeConvoy, // Percentage of transport fleets required for automatic trade
        SpyUpkeepCost, // Intelligence maintenance costs
        SpyDetectionChance, // Spy discovery probability
        SpyCoupDissentModifier, // Coup success rate correction due to dissatisfaction
        InfraEfficiencyModifier, // Province efficiency correction by infrastructure
        ManpowerToConsumerGoods, // Correction of consumer goods production of human resources
        TimeBetweenSliderChangesAoD, // Slider movement interval
        MinimalPlacementIc, // Necessity of placement in overseas provisions I C
        NuclearPower, // Nuclear power generation
        FreeInfraRepair, // Natural recovery rate of infrastructure
        MaxSliderDissent, // Maximum dissatisfaction when moving the slider
        MinSliderDissent, // Minimum dissatisfaction when moving the slider
        MaxDissentSliderMove, // Maximum dissatisfaction that the slider can move
        IcConcentrationBonus, // Factory concentration bonus
        TransportConversion, // Transport ship conversion factor
        ConvoyDutyConversion, // Transport fleet conversion coefficient
        EscortDutyConversion, // Escort fleet conversion coefficient
        MinisterChangeDelay, // Number of days delayed for ministerial change
        MinisterChangeEventDelay, // Number of days delayed for ministerial change (( event )
        IdeaChangeDelay, // Number of days delayed for national policy change
        IdeaChangeEventDelay, // Number of days delayed for national policy change (( event )
        LeaderChangeDelay, // Commander change delay days
        ChangeIdeaDissent, // Amount of increase in dissatisfaction when changing national policy
        ChangeMinisterDissent, // Amount of increase in dissatisfaction when changing ministers
        MinDissentRevolt, // Minimum dissatisfaction with rebellion
        DissentRevoltMultiplier, // Rebel army rate coefficient due to dissatisfaction
        TpMaxAttach, // Maximum number of equipment attached to the transport ship
        SsMaxAttach, // Maximum number of submersible equipment
        SsnMaxAttach, // Maximum number of equipment attached to nuclear submarines
        DdMaxAttach, // Maximum number of equipment attached to the destroyer
        ClMaxAttach, // Maximum number of equipment attached to light cruisers
        CaMaxAttach, // Maximum number of heavy cruisers attached
        BcMaxAttach, // Maximum number of equipment attached to cruise battleships
        BbMaxAttach, // Maximum number of attached equipment for battleships
        CvlMaxAttach, // Maximum number of equipment attached to the light carrier
        CvMaxAttach, // Maximum number of equipment attached to the aircraft carrier
        CanChangeIdeas, // Allow players to change national policy
        CanUnitSendNonAlliedDh, // Sold the division to a non-allied country
        BluePrintsCanSoldNonAllied, // Sell blueprints to non-allied countries
        ProvinceCanSoldNonAllied, // Selling provinces to non-allied countries
        TransferAlliedCoreProvinces, // Return of the core state of the occupied ally
        ProvinceBuildingsRepairModifier, // Building repair speed correction
        ProvinceResourceRepairModifier, // Resource recovery speed correction
        StockpileLimitMultiplierResource, // Resource reserve upper limit correction
        StockpileLimitMultiplierSuppliesOil, // Supplies / / Fuel reserve upper limit correction
        OverStockpileLimitDailyLoss, // Excess stock loss ratio
        MaxResourceDepotSize, // Resource reserve upper limit
        MaxSuppliesOilDepotSize, // Supplies / / Fuel stockpile upper limit
        DesiredStockPilesSuppliesOil, // Ideal supplies / / Fuel stockpile ratio
        MaxManpower, // Maximum human resources
        ConvoyTransportsCapacity, // Fleet transport capacity
        SuppyLandStaticDh, // Army standby material usage correction
        SupplyLandBattleDh, // Army combat material usage correction
        FuelLandStatic, // Army standby fuel usage correction
        FuelLandBattle, // Army combat fuel usage correction
        SupplyAirStaticDh, // Air Force standby supplies usage correction
        SupplyAirBattleDh, // Air Force Combat Material Usage Correction
        FuelAirNavalStatic, // Air Force / / Navy standby fuel usage correction
        FuelAirBattle, // Air Force Combat Fuel Usage Correction
        SupplyNavalStaticDh, // Navy standby material usage correction
        SupplyNavalBattleDh, // Navy combat material usage correction
        FuelNavalNotMoving, // Navy non-moving fuel usage correction
        FuelNavalBattle, // Navy combat fuel usage correction
        TpTransportsConversionRatio, // Conversion ratio of transport ships to transport fleets
        DdEscortsConversionRatio, // Conversion ratio of destroyers to convoys
        ClEscortsConversionRatio, // Conversion ratio of light cruisers to convoys
        CvlEscortsConversionRatio, // Conversion ratio of light aircraft carriers to convoys
        ProductionLineEdit, // Editing the production line
        GearingBonusLossUpgradeUnit, // Gearing bonus reduction rate when improving the unit
        GearingBonusLossUpgradeBrigade, // Gearing bonus reduction rate when improving the brigade
        DissentNukes, // Dissatisfaction increase coefficient at the time of core state nuclear attack
        MaxDailyDissent, // Supplies / / Maximum dissatisfaction increase value when there is a shortage of consumer goods
        NukesProductionModifier, // Nuclear weapon production correction
        ConvoySystemOptionsAllied, // Fleet system for allies
        ResourceConvoysBackUnneeded, // Unnecessary resources / / Fuel recovery ratio
        SupplyDistanceModCalculationModifier, // Distance coefficient of remote replenishment correction

        // intelligence
        SpyMissionDaysDh, // Intelligence mission intervals
        IncreateIntelligenceLevelDaysDh, // Intelligence level increase interval
        ChanceDetectSpyMissionDh, // Probability of discovering domestic intelligence activity
        RelationshipsHitDetectedMissionsDh, // Amount of decrease in friendship when an intelligence mission is discovered
        DistanceModifier, // Distance correction for intelligence missions
        DistanceModifierNeighboursDh, // Neighboring country correction of intelligence mission
        SpyLevelBonusDistanceModifier, // Intelligence level distance correction
        SpyLevelBonusDistanceModifierAboveTen, // Intelligence level Ten Distance correction when exceeding
        SpyInformationAccuracyModifierDh, // Information accuracy correction
        IcModifierCost, // Of intelligence costs I C correction
        MinIcCostModifier, // Minimum intelligence cost compensation I C
        MaxIcCostModifierDh, // Maximum intelligence cost compensation I C
        ExtraMaintenanceCostAboveTen, // Intelligence level Ten Additional maintenance cost when exceeded
        ExtraCostIncreasingAboveTen, // Intelligence level Ten Increased cost when exceeded
        ShowThirdCountrySpyReportsDh, // Report on intelligence activities in third countries
        SpiesMoneyModifier, // Intelligence fund allocation amendment

        // diplomacy
        DaysBetweenDiplomaticMissions, // Diplomat dispatch interval
        TimeBetweenSliderChangesDh, // Slider movement interval
        RequirementAffectSliderDh, // To influence the policy slider I C ratio
        UseMinisterPersonalityReplacing, // Apply ministerial characteristics when changing ministers
        RelationshipHitCancelTrade, // Decreased friendship when canceling trade
        RelationshipHitCancelPermanentTrade, // Decreased friendship when canceling permanent trade
        PuppetsJoinMastersAlliance, // The nation forcibly joins the alliance of the sovereign nation
        MastersBecomePuppetsPuppets, // Can the nation of the nation be established?
        AllowManualClaimsChange, // Change of sovereignty claim
        BelligerenceClaimedProvince, // Warlikeness increase value when claiming sovereignty
        BelligerenceClaimsRemoval, // Warlikeness reduction value when territorial rights are withdrawn
        JoinAutomaticallyAllesAxis, // Automatically joins the opposing camp when declared war
        AllowChangeHosHog, // Head of State / / Change of government leaders
        ChangeTagCoup, // Change to sibling country when coup d'etat occurs
        FilterReleaseCountries, // Independent country setting
        ReturnOccupiedProvinces, // Occupation Province Return Days

        // combat
        LandXpGainFactor, // Army experience value acquisition coefficient
        NavalXpGainFactor, // Navy experience value acquisition coefficient
        AirXpGainFactor, // Air Force XP Acquisition Factor
        AirDogfightXpGainFactor, // Air Force Air Force Experience Value Acquisition Factor
        DivisionXpGainFactor, // Division experience value acquisition coefficient
        LeaderXpGainFactor, // Commander experience value acquisition coefficient
        AttritionSeverityModifier, // Consumption coefficient
        NoSupplyAttritionSeverity, // Natural condition consumption coefficient without replenishment
        NoSupplyMinimunAttrition, // Consumption coefficient when not replenished
        OutOfSupplyAttritionLand, // Army force consumption when unsupplied
        OutOfSupplyAttritionNaval, // Navy force consumption when unsupplied
        OutOfSupplyAttritionAir, // Air force force consumption when no supply
        LowestStrAttritionLosses, // Lower limit of force reduction due to exhaustion
        BaseProximity, // Base combat correction
        ShoreBombardmentModifier, // Ship gun shooting combat correction
        ShoreBombardmentCap, // Ship gun shooting combat efficiency upper limit
        InvasionModifier, // Assault landing penalty
        MultipleCombatModifier, // Side attack penalty
        OffensiveCombinedArmsBonus, // Attacker Union of Military Department Bonus
        DefensiveCombinedArmsBonus, // Defender Military Union Bonus
        SurpriseModifier, // Surprise attack penalty
        LandCommandLimitModifier, // Army command cap penalty
        AirCommandLimitModifier, // Air Force Command Upper Penalty
        NavalCommandLimitModifier, // Navy command cap penalty
        EnvelopmentModifier, // Multi-direction attack correction
        EncircledModifier, // Siege attack penalty
        LandFortMultiplier, // Fortress attack penalty
        CoastalFortMultiplier, // Coastal fortress attack penalty
        HardUnitsAttackingUrbanPenalty, // City attack penalty for armored units
        DissentMultiplier, // National dissatisfaction penalty
        SupplyProblemsModifier, // Insufficient supply penalty
        SupplyProblemsModifierLand, // Army supplies shortage penalty
        SupplyProblemsModifierAir, // Air Force supplies shortage penalty
        SupplyProblemsModifierNaval, // Navy supplies shortage penalty
        FuelProblemsModifierLand, // Army fuel shortage penalty
        FuelProblemsModifierAir, // Air Force Fuel Shortage Penalty
        FuelProblemsModifierNaval, // Navy fuel shortage penalty
        RaderStationMultiplier, // Radar correction
        RaderStationAaMultiplier, // radar / / Anti-aircraft gun compound correction
        InterceptorBomberModifier, // Bomber interception bonus
        AirOverstackingModifier, // Air Force Stack Penalty
        AirOverstackingModifierAoD, // Air Force Stack Penalty
        NavalOverstackingModifier, // Navy stack penalty
        BombingEntrenchedArmiesModifier, // Penalty for bombing the pit
        DefendingEntrenchedArmiesModifier, // Trench defense bonus
        LandLeaderCommandLimitRank0, // Army Marshal Command Upper Limit
        LandLeaderCommandLimitRank1, // Army general command upper limit
        LandLeaderCommandLimitRank2, // Army Vice Admiral Command Upper Limit
        LandLeaderCommandLimitRank3, // Army General Command Upper Limit
        AirLeaderCommandLimitRank0, // Air Force Marshal Command Upper Limit
        AirLeaderCommandLimitRank1, // Air Force General Command Upper Limit
        AirLeaderCommandLimitRank2, // Air Force Vice Admiral Command Upper Limit
        AirLeaderCommandLimitRank3, // Air Force Admiral Command Upper Limit
        NavalLeaderCommandLimitRank0, // Navy Marshal Command Upper Limit
        NavalLeaderCommandLimitRank1, // Admiral command limit
        NavalLeaderCommandLimitRank2, // Navy Vice Admiral Command Upper Limit
        NavalLeaderCommandLimitRank3, // Rear Admiral Command Upper Limit
        HqCommandLimitFactor, // Command upper limit coefficient
        ConvoyProtectionFactor, // Transport fleet escort coefficient
        ConvoyEscortsModel, // Convoy escort ship model
        ConvoyTransportsModel, // Transport fleet transport ship model
        ChanceEscortCarrier, // Convoy escort carrier ratio
        ConvoyEscortCarrierModel, // Convoy escort carrier model
        DelayAfterCombatEnds, // Post-combat command delay time
        LandDelayBeforeOrders, // Army command delay time
        NavalDelayBeforeOrders, // Navy command delay time
        AirDelayBeforeOrders, // Air Force command delay time
        MaximumSizesAirStacks, // Air Force maximum stack size
        DurationAirToAirBattles, // Minimum battle time for air battle
        DurationNavalPortBombing, // Minimum battle time for port attack
        DurationStrategicBombing, // Strategic bombing minimum combat time
        DurationGroundAttackBombing, // Ground bombing minimum combat time
        EffectExperienceCombat, // Experience value correction
        DamageNavalBasesBombing, // Navy base strategic bombing coefficient
        DamageAirBaseBombing, // Air Force Base Strategic Bombing Coefficient
        DamageAaBombing, // Anti-aircraft gun strategic bombing coefficient
        DamageRocketBombing, // Rocket test site strategic bombing coefficient
        DamageNukeBombing, // Reactor Strategic Bombing Coefficient
        DamageRadarBombing, // Radar strategy bombing coefficient
        DamageInfraBombing, // Infrastructure strategy bombing coefficient
        DamageIcBombing, // I C Strategic bombing coefficient
        DamageResourcesBombing, // Resource strategy bombing coefficient
        DamageSyntheticOilBombing, // Synthetic oil factory strategic bombing coefficient
        HowEffectiveGroundDef, // Ground defense efficiency correction
        ChanceAvoidDefencesLeft, // Basic avoidance rate (( There are times of defense )
        ChanceAvoidNoDefences, // Basic avoidance rate (( No defense count )
        LandChanceAvoidDefencesLeft, // Army basic avoidance rate (( There are times of defense )
        AirChanceAvoidDefencesLeft, // Air Force basic avoidance rate (( There are times of defense )
        NavalChanceAvoidDefencesLeft, // Navy basic avoidance rate (( There are times of defense )
        ChanceAvoidAaDefencesLeft, // Anti-aircraft gun basic avoidance rate (( There are times of defense )
        LandChanceAvoidNoDefences, // Army basic avoidance rate (( No defense count )
        AirChanceAvoidNoDefences, // Air Force basic avoidance rate (( No defense count )
        NavalChanceAvoidNoDefences, // Navy basic avoidance rate (( No defense count )
        ChanceAvoidAaNoDefences, // Anti-aircraft gun basic avoidance rate (( No defense count )
        ChanceGetTerrainTrait, // Possibility of acquiring terrain characteristics
        ChanceGetEventTrait, // Possibility to acquire combat characteristics
        BonusTerrainTrait, // Topographical characteristic correction
        BonusSimilarTerrainTrait, // Similar terrain characteristic correction
        BonusEventTrait, // Combat characteristic correction
        BonusLeaderSkillPointLand, // Army commander skill correction
        BonusLeaderSkillPointAir, // Air Force Commander Skill Correction
        BonusLeaderSkillPointNaval, // Navy commander skill correction
        ChanceLeaderDying, // Commander death probability
        AirOrgDamage, // Air Force Organization Rate Damage Damage
        AirStrDamageOrg, // Air Force force damage (( Organizational power )
        AirStrDamage, // Air Force force damage
        LandMinOrgDamage, // Army minimum organization rate damage taken
        LandOrgDamageHardSoftEach, // Army organization rate damage taken (( Armor / / Unarmored )
        LandOrgDamageHardVsSoft, // Army organization rate damage taken (( Armored vs. unarmored )
        LandMinStrDamage, // Army minimum force damage
        LandStrDamageHardSoftEach, // Army force damage (( Armor / / Unarmored )
        LandStrDamageHardVsSoft, // Army force damage (( Armored vs. unarmored )
        AirMinOrgDamage, // Air Force Minimum Organization Rate Damage Damage
        AirAdditionalOrgDamage, // Air Force additional organization rate Damage taken
        AirMinStrDamage, // Air Force Minimum Force Damage Damage
        AirAdditionalStrDamage, // Air Force additional force damage
        AirStrDamageEntrenced, // Air Force force damage (( Anti-塹 壕)
        NavalMinOrgDamage, // Navy minimum organization rate damage taken
        NavalAdditionalOrgDamage, // Navy additional organization rate damage taken
        NavalMinStrDamage, // Navy minimum force damage
        NavalAdditionalStrDamage, // Navy additional force damage
        AirOrgDamageLimitLand, // Air Force vs. Army Organization Rate Damage Minimum
        LandOrgDamageLimitAir, // Army anti-air force organization rate lower limit of damage
        AirOrgDamageLimitNavy, // Air Force vs. Navy Organization Rate Minimum Damage Damage
        NavalOrgDamageLimitAir, // Navy vs. Air Force Organization Rate Damage Minimum
        AirOrgDamageLimitAa, // Air Force anti-aircraft gun organization rate Lower limit of damage
        BasesOrgDamageHourCarriers, // Organization rate damage per base attack time
        BasesOrgDamageLimitCarriers, // Base attack organization rate Lower limit of damage
        BasesOrgDamageAfterCarriers, // Organization rate damage after base attack
        BasesStrDamageCarriers, // Base attack force damage correction
        AirCriticalHitChanceNavy, // Air Force vs. Navy Additional Damage Probability
        AirCriticalHitModifierNavy, // Air Force vs. Navy Additional Damage Compensation
        NavalCriticalHitChanceNavy, // Navy vs. Navy additional damage probability
        NavalCriticalHitModifierNavy, // Navy vs. Navy additional damage compensation
        AirStrDamageLandOrg, // Air Force vs. Army Force Damage Damage (( Organization rate )
        AirStrDamageLandOrgDh104, // Air Force vs. Army Force Damage Damage (( Organization rate )
        AirOrgDamageLandDh, // Air Force vs. Army Organization Rate Damage Damage
        AirStrDamageLandDh, // Air Force vs. Army Force Damage Damage
        LandOrgDamageLandOrg, // Army vs. Army Organization Rate Damage Damage (( Organization rate )
        LandOrgDamageLandUrban, // Army vs. Army Organization Rate Damage Damage (( City )
        LandOrgDamageLandFort, // Army vs. Army Organization Rate Damage Damage (( Fortress )
        RequiredLandFortSize, // Required fortress scale
        LandStrDamageLandDh, // Army vs. Army Force Damage
        LandStrDamageLimitLand, // Army vs. Army Force Damage Minimum
        AirOrgDamageAirDh, // Air Force vs. Air Force Organization Rate Damage Damage
        AirStrDamageAirDh, // Air Force vs. Air Force Force Damage
        LandOrgDamageAirDh, // Army vs. Air Force Organization Rate Damage Damage
        LandStrDamageAirDh, // Army anti-air force force damage
        NavalOrgDamageAirDh, // Navy vs. Air Force Organization Rate Damage Damage
        NavalStrDamageAirDh, // Navy vs. Air Force force damage
        SubsOrgDamageAir, // Submarine anti-air force organization rate damage taken
        SubsStrDamageAir, // Submarine anti-air force force damage
        AirOrgDamageNavyDh, // Air Force vs. Navy Organization Rate Damage Damage
        AirStrDamageNavyDh, // Air Force vs. Navy Force Damage
        NavalOrgDamageNavyDh, // Navy vs. Navy organization rate Damage taken
        NavalStrDamageNavyDh, // Navy vs. Navy force damage
        SubsOrgDamageNavy, // Submarine vs. Navy Organization Rate Damage Damage
        SubsStrDamageNavy, // Submarine vs. Navy Force Damage
        SubsOrgDamage, // Submarine organization rate Damage taken
        SubsStrDamage, // Submarine force damage
        SubStacksDetectionModifier, // Submarine discovery correction
        AirOrgDamageLandAoD, // Air Force vs. Army Organization Rate Damage Damage
        AirStrDamageLandAoD, // Air Force vs. Army Force Damage Damage
        LandDamageArtilleryBombardment, // Bombardment damage compensation (( Ground troops )
        InfraDamageArtilleryBombardment, // Fire damage compensation (( infrastructure )
        IcDamageArtilleryBombardment, // Bombardment damage compensation (I C)
        ResourcesDamageArtilleryBombardment, // Bombardment damage compensation (( resource )
        PenaltyArtilleryBombardment, // Attacked penalty during shooting
        ArtilleryStrDamage, // Bombardment force damage
        ArtilleryOrgDamage, // Artillery rate damage
        LandStrDamageLandAoD, // Army vs. Army Force Damage
        LandOrgDamageLand, // Army vs. Army Organization Rate Damage Damage
        LandStrDamageAirAoD, // Army anti-air force force damage
        LandOrgDamageAirAoD, // Army vs. Air Force Organization Rate Damage Damage
        NavalStrDamageAirAoD, // Navy vs. Air Force force damage
        NavalOrgDamageAirAoD, // Navy vs. Air Force Organization Rate Damage Damage
        AirStrDamageAirAoD, // Air Force vs. Air Force Force Damage
        AirOrgDamageAirAoD, // Air Force vs. Air Force Organization Rate Damage Damage
        NavalStrDamageNavyAoD, // Navy vs. Navy force damage
        NavalOrgDamageNavyAoD, // Navy vs. Navy organization rate Damage taken
        AirStrDamageNavyAoD, // Air Force vs. Navy Force Damage
        AirOrgDamageNavyAoD, // Air Force vs. Navy Organization Rate Damage Damage
        MilitaryExpenseAttritionModifier, // Combat correction when salary is insufficient
        NavalMinCombatTime, // Navy minimum combat time
        LandMinCombatTime, // Army minimum combat time
        AirMinCombatTime, // Air Force Minimum Combat Time
        LandOverstackingModifier, // Army stack penalty
        LandOrgLossMoving, // Army movement organization rate reduction coefficient
        AirOrgLossMoving, // Air Force Movement Organization Rate Reduction Factor
        NavalOrgLossMoving, // Navy movement rate reduction coefficient
        SupplyDistanceSeverity, // Remote replenishment coefficient
        SupplyBase, // Basic replenishment efficiency
        LandOrgGain, // Army organization rate correction
        AirOrgGain, // Air Force Organization Rate Correction
        NavalOrgGain, // Navy organization rate correction
        NukeManpowerDissent, // Nuclear attack dissatisfaction coefficient (( Human resources )
        NukeIcDissent, // Nuclear attack dissatisfaction coefficient (I C)
        NukeTotalDissent, // Nuclear attack dissatisfaction coefficient (( total )
        LandFriendlyOrgGain, // Army friendship area organization rate correction
        AirLandStockModifier, // Stop attack reserve correction
        ScorchDamage, // Burnt command damage
        StandGroundDissent, // Increased dissatisfaction with death guard orders
        ScorchGroundBelligerence, // Burnt order increased warlikeness
        DefaultLandStack, // Army default stack number
        DefaultNavalStack, // Navy default stack count
        DefaultAirStack, // Air Force default stack count
        DefaultRocketStack, // Rocket default stack count
        FortDamageArtilleryBombardment, // Fortress artillery damage compensation
        ArtilleryBombardmentOrgCost, // Decrease in shooting organization rate
        LandDamageFort, // Army vs. Fortress Damage Factor
        AirRebaseFactor, // Air Force Base Mobile Organization Rate Decrease Factor
        AirMaxDisorganized, // Penalty for occupying the airport
        AaInflictedStrDamage, // Anti-aircraft force damage correction
        AaInflictedOrgDamage, // Anti-aircraft gun organization rate damage correction
        AaInflictedFlyingDamage, // Anti-aircraft gun over-the-air damage correction
        AaInflictedBombingDamage, // Damage correction during anti-aircraft bombing
        HardAttackStrDamage, // Armored unit strength damage compensation
        HardAttackOrgDamage, // Armor unit organization rate damage correction
        ArmorSoftBreakthroughMin, // Tank-to-person minimum breakthrough coefficient
        ArmorSoftBreakthroughMax, // Tank-to-person maximum breakthrough coefficient
        NavalCriticalHitChance, // Navy critical hit probability
        NavalCriticalHitEffect, // Navy critical hit effect
        LandFortDamage, // Fortress damage compensation
        PortAttackSurpriseChanceDay, // Japan-China port attack surprise attack probability
        PortAttackSurpriseChanceNight, // Night port attack surprise attack probability
        PortAttackSurpriseModifier, // Port attack surprise attack correction
        RadarAntiSurpriseChance, // Radar surprise attack probability reduction value
        RadarAntiSurpriseModifier, // Radar surprise attack effect reduction value
        CounterAttackStrDefenderAoD, // Counterattack Event Defender Strength Correction
        CounterAttackOrgDefenderAoD, // Counterattack event Defender organization rate correction
        CounterAttackStrAttackerAoD, // Counterattack event Attack side force correction
        CounterAttackOrgAttackerAoD, // Counterattack event Attacker organization rate correction
        AssaultStrDefenderAoD, // Assault Event Defender Strength Correction
        AssaultOrgDefenderAoD, // Assault event defender organization rate correction
        AssaultStrAttackerAoD, // Assault event Attack side force correction
        AssaultOrgAttackerAoD, // Assault event Attack side organization rate correction
        EncirclementStrDefenderAoD, // Siege Event Defender Strength Correction
        EncirclementOrgDefenderAoD, // Siege event defender organization rate correction
        EncirclementStrAttackerAoD, // Siege event Attack side force correction
        EncirclementOrgAttackerAoD, // Siege event Attacker organization rate correction
        AmbushStrDefenderAoD, // Ambush Event Defender Strength Correction
        AmbushOrgDefenderAoD, // Ambush Event Defender Organization Rate Correction
        AmbushStrAttackerAoD, // Ambush Event Attacker Strength Correction
        AmbushOrgAttackerAoD, // Ambush Event Attacker Organization Rate Correction
        DelayStrDefenderAoD, // Delayed event defender strength correction
        DelayOrgDefenderAoD, // Delayed event defender organization rate correction
        DelayStrAttackerAoD, // Delayed event Attack side force correction
        DelayOrgAttackerAoD, // Delayed event Attacker organization rate correction
        TacticalWithdrawStrDefenderAoD, // Retreat event defender strength correction
        TacticalWithdrawOrgDefenderAoD, // Retreat event defender organization rate correction
        TacticalWithdrawStrAttackerAoD, // Retreat event Attack side force correction
        TacticalWithdrawOrgAttackerAoD, // Retreat event Attacker organization rate correction
        BreakthroughStrDefenderAoD, // Breakthrough Event Defender Strength Correction
        BreakthroughOrgDefenderAoD, // Breakthrough event Defender organization rate correction
        BreakthroughStrAttackerAoD, // Breakthrough event Attack side force correction
        BreakthroughOrgAttackerAoD, // Breakthrough event Attack side organization rate correction
        AaMinOrgDamage, // Anti-aircraft gun organization rate minimum damage
        AaAdditionalOrgDamage, // Anti-aircraft gun organization rate additional damage
        AaMinStrDamage, // Minimum anti-aircraft force damage
        AaAdditionalStrDamage, // Anti-aircraft gun strength additional damage
        NavalOrgDamageAa, // Navy anti-aircraft gun organization rate damage taken
        AirOrgDamageAa, // Air Force anti-aircraft gun organization rate damage taken
        AirStrDamageAa, // Air Force Anti-Air Cannon Force Damage
        AaAirFiringRules, // Anti-aircraft attack rules
        AaAirNightModifier, // Anti-aircraft gun night attack correction
        AaAirBonusRadars, // Anti-aircraft attack radar bonus
        NukesStrDamage, // Nuclear attack force damage coefficient
        NukesMaxStrDamageFriendly, // Nuclear attack friendly force maximum damage coefficient
        NukesOrgDamage, // Nuclear attack organization rate damage coefficient
        NukesOrgDamageNonFriendly, // Nuclear Weapon Enemy Army Organization Rate Damage Factor
        NavalBombardmentChanceDamaged, // Navy bombing maximum damage ship hit probability
        NavalBombardmentChanceBest, // Navy bombing minimum damage ship hit probability
        TacticalBombardmentChanceDamaged, // Tactical bombing maximum damage unit hit probability
        MovementBonusTerrainTrait, // Terrain proper movement bonus
        MovementBonusSimilarTerrainTrait, // Similar terrain proper movement bonus
        LogisticsWizardEseBonus, // Replenishment efficiency bonus for military station management
        OffensiveSupplyESEBonus, // At the time of offensive ESE Bonus
        DaysOffensiveSupply, // Number of days to continue the offensive
        MinisterBonuses, // How to apply the ministerial bonus
        OrgRegainBonusFriendly, // Friendship area organization rate recovery bonus
        OrgRegainBonusFriendlyCap, // Friendship area organization rate recovery bonus upper limit
        NewOrgRegainLogic, // New organization rate recovery logic
        OrgRegainMorale, // Organization rate recovery morale correction
        OrgRegainClear, // Organization rate recovery fine weather correction
        OrgRegainFrozen, // Tissue rate recovery below freezing point correction
        OrgRegainRaining, // Organization rate recovery Rain correction
        OrgRegainSnowing, // Organization rate recovery Snowfall correction
        OrgRegainStorm, // Organization rate recovery storm correction
        OrgRegainBlizzard, // Organization rate recovery Snowstorm correction
        OrgRegainMuddy, // Organization rate recovery muddy land correction
        OrgRegainNaval, // Navy organization rate recovery coefficient
        OrgRegainNavalOutOfFuel, // Navy organization rate recovery fuel shortage correction
        OrgRegainNavalOutOfSupplies, // Navy organization rate recovery supplies shortage correction
        OrgRegainNavalCurrent, // Navy organization rate recovery current value correction
        OrgRegainNavalBase, // Navy organization rate recovery base correction
        OrgRegainNavalSea, // Navy organization rate recovery sea area correction
        OrgRegainAir, // Air Force Organization Rate Recovery Factor
        OrgRegainAirOutOfFuel, // Air Force Organization Rate Recovery Fuel Shortage Correction
        OrgRegainAirOutOfSupplies, // Air Force organization rate recovery supplies shortage correction
        OrgRegainAirCurrent, // Air Force Organization Rate Recovery Current Value Correction
        OrgRegainAirBaseSize, // Air Force Organization Rate Recovery Base Satisfaction Rate Correction
        OrgRegainAirOutOfBase, // Air Force Organization Rate Recovery Base Shortage Correction
        OrgRegainArmy, // Army organization rate recovery coefficient
        OrgRegainArmyOutOfFuel, // Army organization rate recovery fuel shortage correction
        OrgRegainArmyOutOfSupplies, // Army organization rate recovery supplies shortage correction
        OrgRegainArmyCurrent, // Army organization rate recovery current value correction
        OrgRegainArmyFriendly, // Army organization rate recovery friendly land correction
        OrgRegainArmyTransportation, // Army organization rate recovery in transit correction
        OrgRegainArmyMoving, // Army organization rate recovery on the move correction
        OrgRegainArmyRetreating, // Army organization rate recovery withdrawal correction
        ConvoyInterceptionMissions, // Convoy obstruction during maritime missions
        AutoReturnTransportFleets, // Automatic return of the transport fleet
        AllowProvinceRegionTargeting, // Single Providence / / Regional designated mission
        NightHoursWinter, // Winter night time
        NightHoursSpringFall, // Spring season / / Autumn night time
        NightHoursSummer, // Summer night time
        RecalculateLandArrivalTimes, // Ground unit arrival time recalculation interval
        SynchronizeArrivalTimePlayer, // Simultaneous arrival correction (( player )
        SynchronizeArrivalTimeAi, // Simultaneous arrival correction (AI)
        RecalculateArrivalTimesCombat, // Recalculation of arrival time after battle
        LandSpeedModifierCombat, // Army movement speed correction during battle
        LandSpeedModifierBombardment, // Army movement speed correction during coastal shooting
        LandSpeedModifierSupply, // Army movement speed correction when supplies run out
        LandSpeedModifierOrg, // Army movement speed correction when organization rate declines
        LandAirSpeedModifierFuel, // Army when fuel runs out / / Air Force movement speed correction
        DefaultSpeedFuel, // Default movement speed when running out of fuel
        FleetSizeRangePenaltyRatio, // Fleet scale cruising range penalty ratio
        FleetSizeRangePenaltyThrethold, // Fleet scale cruising range penalty threshold
        FleetSizeRangePenaltyMax, // Fleet scale cruising range penalty upper limit
        ApplyRangeLimitsAreasRegions, // Local / / Applying distance restrictions within the area
        RadarBonusDetection, // Radar aircraft discovery bonus
        BonusDetectionFriendly, // Friendship Aircraft Discovery Bonus
        ScreensCapitalRatioModifier, // Main ship / / Auxiliary ship ratio correction
        ChanceTargetNoOrgLand, // Army organization rate insufficient unit target probability
        ScreenCapitalShipsTargeting, // Capital ship / / Auxiliary ship target position value
        FleetPositioningDaytime, // Naval position value daytime bonus
        FleetPositioningLeaderSkill, // Naval position value skill correction
        FleetPositioningFleetSize, // Naval position value fleet scale correction
        FleetPositioningFleetComposition, // Naval position value fleet composition correction
        LandCoastalFortsDamage, // Fortress damage compensation
        LandCoastalFortsMaxDamage, // Fortress maximum damage
        MinSoftnessBrigades, // Minimal vulnerability due to attached brigade
        AutoRetreatOrg, // Automatic withdrawal organization rate
        LandOrgNavalTransportation, // Post-sea transport organization rate correction for the Army
        MaxLandDig, // Maximum trench value
        DigIncreaseDay, // 1 Increase in trenches of the day
        BreakthroughEncirclementMinSpeed, // Breakthrough / / Minimum siege speed
        BreakthroughEncirclementMaxChance, // Breakthrough / / Maximum siege probability
        BreakthroughEncirclementChanceModifier, // Breakthrough / / Siege probability correction
        CombatEventDuration, // Combat event duration
        CounterAttackOrgAttackerDh, // Counterattack event Attacker organization rate correction
        CounterAttackStrAttackerDh, // Counterattack event Attack side force correction
        CounterAttackOrgDefenderDh, // Counterattack event Defender organization rate correction
        CounterAttackStrDefenderDh, // Counterattack Event Defender Strength Correction
        AssaultOrgAttackerDh, // Assault event Attack side organization rate correction
        AssaultStrAttackerDh, // Assault event Attack side force correction
        AssaultOrgDefenderDh, // Assault event defender organization rate correction
        AssaultStrDefenderDh, // Assault Event Defender Strength Correction
        EncirclementOrgAttackerDh, // Siege event Attacker organization rate correction
        EncirclementStrAttackerDh, // Siege event Attack side force correction
        EncirclementOrgDefenderDh, // Siege event defender organization rate correction
        EncirclementStrDefenderDh, // Siege Event Defender Strength Correction
        AmbushOrgAttackerDh, // Ambush Event Attacker Organization Rate Correction
        AmbushStrAttackerDh, // Ambush Event Attacker Strength Correction
        AmbushOrgDefenderDh, // Ambush Event Defender Organization Rate Correction
        AmbushStrDefenderDh, // Ambush Event Defender Strength Correction
        DelayOrgAttackerDh, // Delayed event Attacker organization rate correction
        DelayStrAttackerDh, // Delayed event Attack side force correction
        DelayOrgDefenderDh, // Delayed event defender organization rate correction
        DelayStrDefenderDh, // Delayed event defender strength correction
        TacticalWithdrawOrgAttackerDh, // Retreat event Attacker organization rate correction
        TacticalWithdrawStrAttackerDh, // Retreat event Attack side force correction
        TacticalWithdrawOrgDefenderDh, // Retreat event defender organization rate correction
        TacticalWithdrawStrDefenderDh, // Retreat event defender strength correction
        BreakthroughOrgAttackerDh, // Breakthrough event Attack side organization rate correction
        BreakthroughStrAttackerDh, // Breakthrough event Attack side force correction
        BreakthroughOrgDefenderDh, // Breakthrough event Defender organization rate correction
        BreakthroughStrDefenderDh, // Breakthrough Event Defender Strength Correction
        HqStrDamageBreakthrough, // Command damages only during breakthrough events
        CombatMode, // Combat mode

        // mission mission
        AttackMission, // Attack mission
        AttackStartingEfficiency, // Initial attack efficiency
        AttackSpeedBonus, // Attack speed bonus
        RebaseMission, // Base move mission
        RebaseStartingEfficiency, // Base movement initial efficiency
        RebaseChanceDetected, // Base movement detection probability
        StratRedeployMission, // Strategic relocation mission
        StratRedeployStartingEfficiency, // Strategic relocation initial efficiency
        StratRedeployAddedValue, // Strategic relocation addition value
        StratRedeployDistanceMultiplier, // Strategic relocation distance correction
        SupportAttackMission, // Support attack mission
        SupportAttackStartingEfficiency, // Support attack initial efficiency
        SupportAttackSpeedBonus, // Support attack speed bonus
        SupportDefenseMission, // Defense support mission
        SupportDefenseStartingEfficiency, // Initial efficiency of defense support
        SupportDefenseSpeedBonus, // Defense support speed bonus
        ReservesMission, // Standby mission
        ReservesStartingEfficiency, // Standby initial efficiency
        ReservesSpeedBonus, // Wait speed bonus
        AntiPartisanDutyMission, // Anti-Partisan scavenging mission
        AntiPartisanDutyStartingEfficiency, // Partisan mopping initial efficiency
        AntiPartisanDutySuppression, // Anti-Partisan Suppression Pressure Compensation
        PlannedDefenseMission, // Defense planning mission
        PlannedDefenseStartingEfficiency, // National Defense Program Initial Efficiency
        AirSuperiorityMission, // Air superiority mission
        AirSuperiorityStartingEfficiency, // Initial efficiency of air control rights
        AirSuperiorityDetection, // Air superiority enemy aircraft discovery correction
        AirSuperiorityMinRequired, // Minimum number of air superiority units
        GroundAttackMission, // Ground attack mission
        GroundAttackStartingEfficiency, // Ground attack initial efficiency
        GroundAttackOrgDamage, // Ground attack organization rate damage correction
        GroundAttackStrDamage, // Ground attack force damage correction
        InterdictionMission, // Stop attack mission
        InterdictionStartingEfficiency, // Initial efficiency of blocking attack
        InterdictionOrgDamage, // Interdiction organization rate damage correction
        InterdictionStrDamage, // Stop attack force damage correction
        StrategicBombardmentMission, // Strategic bombing mission
        StrategicBombardmentStartingEfficiency, // Strategic bombing initial efficiency
        LogisticalStrikeMission, // Soldier attack mission
        LogisticalStrikeStartingEfficiency, // Initial efficiency of soldier attack
        RunwayCrateringMission, // Airport air bombing mission
        RunwayCrateringStartingEfficiency, // Airport air bomb initial efficiency
        InstallationStrikeMission, // Military facility attack mission
        InstallationStrikeStartingEfficiency, // Initial efficiency of military facility attack
        NavalStrikeMission, // Ship attack mission
        NavalStrikeStartingEfficiency, // Initial efficiency of ship attack
        PortStrikeMission, // Port attack mission
        PortStrikeStartingEfficiency, // Initial efficiency of port attack
        ConvoyAirRaidingMission, // Air fleet bombing mission
        ConvoyAirRaidingStartingEfficiency, // Air fleet bombing initial efficiency
        AirSupplyMission, // Air transport supply mission
        AirSupplyStartingEfficiency, // Initial efficiency of air transportation supply
        AirborneAssaultMission, // Airborne assault mission
        AirborneAssaultStartingEfficiency, // Airborne assault initial efficiency
        NukeMission, // Nuclear attack mission
        NukeStartingEfficiency, // Initial efficiency of nuclear attack
        AirScrambleMission, // Air emergency sortie mission
        AirScrambleStartingEfficiency, // Air emergency sortie initial efficiency
        AirScrambleDetection, // Air emergency sortie enemy aircraft discovery correction
        AirScrambleMinRequired, // Minimum number of aviation emergency sortie units
        ConvoyRadingMission, // Fleet raid mission
        ConvoyRadingStartingEfficiency, // Initial efficiency of fleet attack
        ConvoyRadingRangeModifier, // Fleet attack cruising range correction
        ConvoyRadingChanceDetected, // Fleet attack detection probability
        AswMission, // Anti-submarine operation mission
        AswStartingEfficiency, // Anti-submarine operation initial efficiency
        NavalInterdictionMission, // Maritime deterrence mission
        NavalInterdictionStartingEfficiency, // Maritime blocking initial efficiency
        ShoreBombardmentMission, // Coastal artillery mission
        ShoreBombardmentStartingEfficiency, // Coastal shooting initial efficiency
        ShoreBombardmentModifierDh, // Coastal artillery correction
        AmphibousAssaultMission, // Assault landing mission
        AmphibousAssaultStartingEfficiency, // Initial efficiency of assault landing
        SeaTransportMission, // Sea shipping mission
        SeaTransportStartingEfficiency, // Initial efficiency of sea shipping
        SeaTransportRangeModifier, // Maritime transport cruising distance correction
        SeaTransportChanceDetected, // Probability of discovery by sea shipping
        NavalCombatPatrolMission, // Maritime combat patrol mission
        NavalCombatPatrolStartingEfficiency, // Maritime Combat Patroll Initial Efficiency
        NavalPortStrikeMission, // Port attack mission by carrier
        NavalPortStrikeStartingEfficiency, // Initial efficiency of port attack by aircraft carrier
        NavalAirbaseStrikeMission, // Aircraft carrier attack mission
        NavalAirbaseStrikeStartingEfficiency, // Initial efficiency of air base attack by aircraft carrier
        SneakMoveMission, // Covert movement mission
        SneakMoveStartingEfficiency, // Onmitsu movement initial efficiency
        SneakMoveRangeModifier, // Covert movement cruising range correction
        SneakMoveChanceDetected, // Onmitsu movement detection probability
        NavalScrambleMission, // Maritime emergency sortie mission
        NavalScrambleStartingEfficiency, // Initial efficiency of maritime emergency sortie
        NavalScrambleSpeedBonus, // Maritime emergency sortie speed bonus
        UseAttackEfficiencyCombatModifier, // attack / / Use support attack efficiency as a combat correction

        // country country
        LandFortEfficiency, // Land fortress efficiency
        CoastalFortEfficiency, // Coastal fortress efficiency
        GroundDefenseEfficiency, // Ground defense efficiency
        ConvoyDefenseEfficiency, // Fleet defense efficiency
        ManpowerBoost, // Increased human resources
        TransportCapacityModifier, // TC correction
        OccupiedTransportCapacityModifier, // Occupied territory TC correction
        AttritionModifier, // Consumption compensation
        ManpowerTrickleBackModifier, // Human resource return correction
        SupplyDistanceModifier, // Remote replenishment correction
        RepairModifier, // Repair correction
        ResearchModifier, // Research correction
        RadarEfficiency, // Radar efficiency
        HqSupplyEfficiencyBonus, // HQ supply efficiency bonus
        HqCombatEventsBonus, // HQ Combat Event Bonus
        CombatEventChances, // Combat event occurrence probability
        FriendlyArmyDetectionChance, // Friend discovery probability
        EnemyArmyDetectionChance, // Enemy army discovery probability
        FriendlyIntelligenceChance, // Friendly country intelligence probability
        EnemyIntelligenceChance, // Enemy country intelligence probability
        MaxAmphibiousArmySize, // Maximum assault landing scale
        EnergyToOil, // energy / / Oil conversion efficiency
        TotalProductionEfficiency, // Total production efficiency
        OilProductionEfficiency, // Oil production efficiency
        MetalProductionEfficiency, // Metal production efficiency
        EnergyProductionEfficiency, // Energy production efficiency
        RareMaterialsProductionEfficiency, // Rare resource production efficiency
        MoneyProductionEfficiency, // Efficiency of money conversion from consumer goods
        SupplyProductionEfficiency, // Material production efficiency
        AaPower, // Anti-aircraft gun attack power
        AirSurpriseChance, // Air Force surprise attack probability
        LandSurpriseChance, // Army surprise attack probability
        NavalSurpriseChance, // Navy surprise attack probability
        PeacetimeIcModifier, // Normal time I C correction
        WartimeIcModifier, // War time I C correction
        BuildingsProductionModifier, // Building production correction
        ConvoysProductionModifier, // Transport fleet production correction
        MinShipsPositioningBattle, // Minimum ship position value
        MaxShipsPositioningBattle, // Maximum ship position value
        PeacetimeStockpilesResources, // Peacetime resource reserve correction
        WartimeStockpilesResources, // Wartime resource reserve correction
        PeacetimeStockpilesOilSupplies, // Peacetime supplies / / Fuel stockpile correction
        WartimeStockpilesOilSupplies, // Wartime supplies / Fuel stockpile correction
        MaxLandDigDH105, // Maximum trench value
        DigIncreaseDayDH105, // 1 Increase in trenches of the day

        // research
        BlueprintBonus, // Blueprint bonus
        PreHistoricalDateModifier, // Prehistoric research penalties
        PostHistoricalDateModifierDh, // Research bonus after historical year
        CostSkillLevel, // Cost per research institute level
        MeanNumberInventionEventsYear, // 1 Average number of invention events per year
        PostHistoricalDateModifierAoD, // Research bonus after historical year
        TechSpeedModifier, // Research speed correction
        PreHistoricalPenaltyLimit, // Upper limit of research penalties before historical years
        PostHistoricalBonusLimit, // Research bonus limit after historical year
        MaxActiveTechTeamsAoD, // Research slot limit
        RequiredIcEachTechTeamAoD, // Required for each research slot I C
        MaximumRandomModifier, // Maximum random correction
        UseNewTechnologyPageLayout, // Research page layout
        TechOverviewPanelStyle, // Research outline panel style
        MaxActiveTechTeamsDh, // Research slot limit
        MinActiveTechTeams, // Research slot lower limit
        RequiredIcEachTechTeamDh, // Required for each research slot I C
        NewCountryRocketryComponent, // Inheriting rocket technology in a new nation
        NewCountryNuclearPhysicsComponent, // Inheriting nuclear physics technology in a new nation
        NewCountryNuclearEngineeringComponent, // Inheriting nuclear engineering technology in a new nation
        NewCountrySecretTechs, // Inheriting secret weapon technology in a new nation
        MaxTechTeamSkill, // Maximum research institute skills

        // trade
        DaysTradeOffers, // Trade negotiation interval
        DelayGameStartNewTrades, // Days of trade negotiation delay immediately after the start of the game
        LimitAiNewTradesGameStart, // Immediately after the game starts AIDays of delay in trade negotiations with friendly countries
        DesiredOilStockpile, // Ideal oil stockpiling
        CriticalOilStockpile, // Danger level oil reserves
        DesiredSuppliesStockpile, // Ideal stockpile
        CriticalSuppliesStockpile, // Danger level stockpile
        DesiredResourcesStockpile, // Ideal resource stockpile
        CriticalResourceStockpile, // Danger level resource reserve
        WartimeDesiredStockpileMultiplier, // Wartime ideal reserve coefficient
        PeacetimeExtraOilImport, // Temporary import ratio of normal oil
        WartimeExtraOilImport, // Temporary import ratio of wartime oil
        ExtraImportBelowDesired, // Temporary import ratio when ideal stock is not reached
        PercentageProducedSupplies, // Material production ratio
        PercentageProducedMoney, // Fund production ratio
        ExtraImportStockpileSelected, // Temporary import ratio when selecting stock
        DaysDeliverResourcesTrades, // Trade Agreement Resources Transport Days
        MergeTradeDeals, // Integration of trade agreements
        ManualTradeDeals, // Manual trade agreement
        PuppetsSendSuppliesMoney, // Puppet country delivery supplies / / Funding
        PuppetsCriticalSupplyStockpile, // Puppet State Material Danger Level
        PuppetsMaxPoolResources, // Puppet country's largest stockpile of resources
        NewTradeDealsMinEffectiveness, // New trade agreement minimum efficiency
        CancelTradeDealsEffectiveness, // Trade agreement destruction efficiency
        AutoTradeAiTradeDeals, // Automatic / AI Trade Agreement Minimum Efficiency

        // ai
        OverproduceSuppliesBelowDesired, // Surplus material production ratio when ideal stock is not reached
        MultiplierOverproduceSuppliesWar, // Wartime surplus material production coefficient
        NotProduceSuppliesStockpileOver, // Material production ban factor when stocking margin
        MaxSerialLineProductionGarrisonMilitia, // Garrison / / Maximum continuous production of militias
        MinIcSerialProductionNavalAir, // Navy / / Air Force continuous production minimum I C
        NotProduceNewUnitsManpowerRatio, // New production ban human resource ratio
        NotProduceNewUnitsManpowerValue, // New production ban human resource value
        NotProduceNewUnitsSupply, // New production prohibited material ratio
        MilitaryStrengthTotalIcRatioPeacetime, // Total I C Military power ratio to (( Peacetime )
        MilitaryStrengthTotalIcRatioWartime, // Total I C Military power ratio to (( War time )
        MilitaryStrengthTotalIcRatioMajor, // Total I C Military power ratio to (( Major countries )
        NotUseOffensiveSupplyStockpile, // Stockpile of supplies canceled offensive
        NotUseOffensiveOilStockpile, // Offensive stop fuel stockpile
        NotUseOffensiveEse, // Offensive stop supply efficiency
        NotUseOffensiveOrgStrDamage, // Offensive cancellation organization rate / / Force damage
        AiPeacetimeSpyMissionsDh, // AI Offensive intelligence activities in peacetime
        AiSpyMissionsCostModifierDh, // AI Intelligence cost compensation
        AiDiplomacyCostModifierDh, // AI Diplomatic cost correction
        AiInfluenceModifierDh, // AI Diplomatic interference frequency correction
        NewDowRules, // AI New War Declaration Rule
        NewDowRules2, // AI New War Declaration Rule
        ForcePuppetsJoinMastersAllianceNeutrality, // Neutrality for puppet nations to forcibly join the alliance of sovereign nations
        CountriesLeaveBadRelationAlliance, // new AI Occupied territory release rule
        NewAiReleaseRules, // AI Withdrawal from an alliance that has a bad relationship with the nation's ally
        AiEventsActionSelectionRules, // AI Event selection rule
        ForceStrategicRedeploymentHour, // Forced strategic relocation time
        MaxRedeploymentDaysAi, // AI Maximum number of relocation days
        UseQuickAreaCheckGarrisonAi, // Garrison AISimple check
        AiMastersGetProvincesConquredPuppets, // AI The sovereign nation controls the occupied territory of the puppet country
        ReleaseCountryWarZone, // Liberation of occupied territories adjacent to enemy nations
        MinDaysRequiredAiReleaseCountry, // AI Minimum number of days to release occupied territory
        MinDaysRequiredAiAllied, // AI Minimum number of days to return occupied territories
        MinDaysRequiredAiAlliedSupplyBase, // AI Minimum number of days to return occupied territories (( Supply base )
        MinRequiredRelationsAlliedClaimed, // Minimum friendship to join the coalition when claiming sovereignty
        AiUnitPowerCalculationStrOrg, // AI Force calculation method (( Organization rate / / Strength )
        AiUnitPowerCalculationGde, // AI Force calculation method (( Ground defense correction )
        AiUnitPowerCalculationMinOrg, // AI Force calculation method (( Minimum organization rate )
        AiUnitPowerCalculationMinStr, // AI Force calculation method (( Minimum strength )

        // mod
        AiSpyDiplomaticMissionLogger, // AI Intelligence / / Log diplomacy
        CountryLogger, // Log national information
        SwitchedAiFilesLogger, // AI Log switching
        UseNewAutoSaveFileFormat, // New automatic save file name
        LoadNewAiSwitchingAllClients, // In multiplayer AI Load new settings when switching
        TradeEfficiencyCalculationSystem, // Trade efficiency calculation interval
        MergeRelocateProvincialDepots, // Stock recalculation interval
        InGameLossesLogging, // Record loss
        InGameLossLogging2, // Record loss
        AllowBrigadeAttachingInSupply, // Allowed brigade attachment in occupied territories
        MultipleDeploymentSizeArmies, // Number of Army batch deployments
        MultipleDeploymentSizeFleets, // Number of Navy batch deployments
        MultipleDeploymentSizeAir, // Number of batch deployments of the Air Force
        AllowUniquePicturesAllLandProvinces, // Allow unique images for all land provisions
        AutoReplyEvents, // Delegate event choices
        ForceActionsShow, // Forced display of event choices
        EnableDicisionsPlayers, // Use decisions
        RebelsArmyComposition, // Partisan infantry composition ratio
        RebelsArmyTechLevel, // Partisan technical level
        RebelsArmyMinStr, // Partisan minimum strength
        RebelsArmyMaxStr, // Partisan maximum strength
        RebelsOrgRegain, // Partisan organization rate recovery speed
        ExtraRebelBonusNeighboringProvince, // Partisan Bonus (( Occupation of adjacent land )
        ExtraRebelBonusOccupied, // Partisan Bonus (( Occupied territory )
        ExtraRebelBonusMountain, // Partisan Bonus (( Mountains )
        ExtraRebelBonusHill, // Partisan Bonus (( Hills )
        ExtraRebelBonusForest, // Partisan Bonus (( forest )
        ExtraRebelBonusJungle, // Partisan Bonus (( Dense forest )
        ExtraRebelBonusSwamp, // Partisan Bonus (( Wetlands )
        ExtraRebelBonusDesert, // Partisan Bonus (( desert )
        ExtraRebelBonusPlain, // Partisan Bonus (( Flat ground )
        ExtraRebelBonusUrban, // Partisan Bonus (( City )
        ExtraRebelBonusAirNavalBases, // Partisan Bonus (( Aviation / / Naval base )
        ReturnRebelliousProvince, // Partisan Occupation Province Return Time
        UseNewMinisterFilesFormat, // New format ministerial file format
        EnableRetirementYearMinisters, // Use Ministerial Retirement Year
        EnableRetirementYearLeaders, // Use commander retirement year
        LoadSpritesModdirOnly, // Sprite MODDIR Read only from
        LoadUnitIconsModdirOnly, // Unit icon MODDIR Read only from
        LoadUnitPicturesModdirOnly, // Unit image MODDIR Read only from
        LoadAiFilesModdirOnly, // AI file MODDIR Read only from
        UseSpeedSetGarrisonStatus, // Garrison judgment rule
        UseOldSaveGameFormat, // Use old save format
        ProductionPanelUiStyle, // Of the production panel UI style
        UnitPicturesSize, // Unit image size
        EnablePicturesNavalBrigades, // Use images for equipment attached to ships
        BuildingsBuildableOnlyProvinces, // Build a building only in Providence
        UnitModifiersStatisticsPages, // New style transition threshold on unit correction page
        CheatMultiPlayer, // Use cheet in multiplayer
        ManualChangeConvoy, // Manual change of convoy
        BrigadesRepairManpowerCost, // Includes brigades attached to human resource replenishment costs
        StrPercentageBrigadesAttachment, // Attached brigade detachable force ratio

        // map
        MapNumber, // Map number
        TotalProvinces, // Total provinces
        DistanceCalculationModel, // Distance calculation method
        MapWidth, // Map width
        MapHeight // Map height
    }

    /// <summary>
    ///     misc section ID
    /// </summary>
    public enum MiscSectionId
    {
        Economy, // Economy
        Intelligence, // Intelligence
        Diplomacy, // Diplomatic
        Combat, // fight
        Mission, // mission
        Country, // Nation
        Research, // the study
        Trade, // Trade
        Ai, // AI
        Mod, // MOD
        Map // map
    }

    /// <summary>
    ///     misc File type
    /// </summary>
    public enum MiscGameType
    {
        Dda12, // DDA1.2
        Dda13, // DDA1.3
        Aod104, // AoD1.04
        Aod107, // AoD1.07
        Aod108, // AoD1.08-
        Dh102, // DH1.02
        Dh103, // DH1.03
        Dh104, // DH1.04
        Dh105 // DH1.05
    }

    /// <summary>
    ///     misc Item type
    /// </summary>
    public enum MiscItemType
    {
        None, // Termination item
        Bool, // Boolean value
        Enum, // Choices
        Int, // integer
        PosInt, // Positive integer
        NonNegInt, // Non-negative integer
        NonPosInt, // Non-positive integer
        NonNegIntMinusOne, // Non-negative integer or -1
        NonNegInt1, // Non-negative integer (( After the decimal point 1 digit )
        RangedInt, // Ranged integer
        RangedPosInt, // Ranged positive integer
        RangedIntMinusOne, // Ranged integer or -1
        RangedIntMinusThree, // Real number with range or -1 or -2 or -3
        Dbl, // Real number
        PosDbl, // Positive real number
        NonNegDbl, // Non-negative real number
        NonPosDbl, // Non-positive real number
        NonNegDbl0, // Non-negative real number (( No decimal point )
        NonNegDbl2, // Non-negative real number (( After the decimal point 2 digit )
        NonNegDbl5, // Non-negative real number (( After the decimal point Five digit )
        NonPosDbl0, // Non-positive real number (( No subdecimal )
        NonPosDbl2, // Non-positive real number (( After the decimal point 2 digit )
        NonNegDblMinusOne, // Non-negative real number or -1
        NonNegDblMinusOne1, // Non-negative real number or -1.0
        NonNegDbl2AoD, // Non-negative real number (AoD Only after the decimal point 2 digit )
        NonNegDbl4Dda13, // Non-negative real number (DDA1.3 / DH Only after the decimal point Four digit )
        NonNegDbl2Dh103Full, // Non-negative real number (0 Larger 0.10 After the decimal point only in the following cases 2 digit )
        NonNegDbl2Dh103Full1, // Non-negative real number (( After the decimal point 2 digit / 0 Larger 0.20 After the decimal point only in the following cases 1 digit )
        NonNegDbl2Dh103Full2, // Non-negative real number (0 Larger 1Less than or equal to the decimal point 2 digit )
        NonPosDbl5AoD, // Non-positive real number (AoD Only after the decimal point Five digit )
        NonPosDbl2Dh103Full, // Non-positive real number (-0.10 that's all 0 Below the decimal point only if less than 2 digit )
        RangedDbl, // Real number with range
        RangedDblMinusOne, // Real number with range or -1
        RangedDblMinusOne1, // Real number with range or -1.0
        RangedDbl0, // Real number with range (( No subdecimal )
        NonNegIntNegDbl // Non-negative integer or Negative real number
    }
}
