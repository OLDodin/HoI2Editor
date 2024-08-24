using System;
using System.Collections.Generic;
using System.Text;
using HoI2Editor.Utilities;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Event command
    /// </summary>
    public class Command
    {
        #region Public properties

        /// <summary>
        ///     Command type
        /// </summary>
        public CommandType Type { get; set; }

        /// <summary>
        ///     Parameters --which which
        /// </summary>
        public object Which { get; set; }

        /// <summary>
        ///     Parameters --value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        ///     Parameters --when
        /// </summary>
        public object When { get; set; }

        /// <summary>
        ///     Parameters --where
        /// </summary>
        public object Where { get; set; }

        /// <summary>
        ///     Parameters --name
        /// </summary>
        public object Name { get; set; }

        /// <summary>
        ///     Parameters --org
        /// </summary>
        public object Org { get; set; }

        /// <summary>
        ///     Parameters --cost
        /// </summary>
        public object Cost { get; set; }

        /// <summary>
        ///     Parameters --energy
        /// </summary>
        public object Energy { get; set; }

        /// <summary>
        ///     Parameters --metal
        /// </summary>
        public object Metal { get; set; }

        /// <summary>
        ///     Parameters --rare_materials
        /// </summary>
        public object RareMaterials { get; set; }

        /// <summary>
        ///     Parameters --oil
        /// </summary>
        public object Oil { get; set; }

        /// <summary>
        ///     Parameters --supplies
        /// </summary>
        public object Supplies { get; set; }

        /// <summary>
        ///     Parameters --money
        /// </summary>
        public object Money { get; set; }

        /// <summary>
        ///     Command trigger
        /// </summary>
        public List<Trigger> Triggers { get; } = new List<Trigger>();

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (CommandItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public Command()
        {
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">Event command from the replication source</param>
        public Command(Command original)
        {
            Type = original.Type;

            // If you want to store a reference type object, click here deep copy Change to
            Which = original.Which;
            Value = original.Value;
            When = original.When;
            Where = original.Where;

            Name = original.Name;
            Org = original.Org;
            Cost = original.Cost;
            Energy = original.Energy;
            Metal = original.Metal;
            RareMaterials = original.RareMaterials;
            Oil = original.Oil;
            Supplies = original.Supplies;
            Money = original.Money;

            foreach (Trigger trigger in original.Triggers)
            {
                Triggers.Add(new Trigger(trigger));
            }
        }

        #endregion

        #region String operation

        /// <summary>
        ///     Convert to a string
        /// </summary>
        /// <returns>Character string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Game.Type == GameType.DarkestHour && Triggers != null && Triggers.Count > 0)
            {
                sb.Append("trigger = {");
                foreach (Trigger trigger in Triggers)
                {
                    sb.AppendFormat(" {0}", trigger);
                }
                sb.Append(" } ");
            }
            sb.AppendFormat("type = {0}", Commands.Strings[(int) Type]);
            if (Which != null)
            {
                sb.AppendFormat(" which = {0}", ObjectHelper.ToString(Which));
            }
            if (When != null)
            {
                sb.AppendFormat(" when = {0}", ObjectHelper.ToString(When));
            }
            if (Where != null)
            {
                sb.AppendFormat(" where = {0}", ObjectHelper.ToString(Where));
            }
            if (Value != null)
            {
                sb.AppendFormat(" value = {0}", ObjectHelper.ToString(Value));
            }
            if (Name != null)
            {
                sb.AppendFormat(" name = {0}", ObjectHelper.ToString(Name));
            }
            if (Org != null)
            {
                sb.AppendFormat(" org = {0}", ObjectHelper.ToString(Org));
            }
            if (Cost != null)
            {
                sb.AppendFormat(" cost = {0}", ObjectHelper.ToString(Cost));
            }
            if (Energy != null)
            {
                sb.AppendFormat(" energy = {0}", ObjectHelper.ToString(Energy));
            }
            if (Metal != null)
            {
                sb.AppendFormat(" metal = {0}", ObjectHelper.ToString(Metal));
            }
            if (RareMaterials != null)
            {
                sb.AppendFormat(" rare_materials = {0}", ObjectHelper.ToString(RareMaterials));
            }
            if (Oil != null)
            {
                sb.AppendFormat(" oil = {0}", ObjectHelper.ToString(Oil));
            }
            if (Supplies != null)
            {
                sb.AppendFormat(" supplies = {0}", ObjectHelper.ToString(Supplies));
            }
            if (Money != null)
            {
                sb.AppendFormat(" money = {0}", ObjectHelper.ToString(Money));
            }

            return sb.ToString();
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the technical item data has been edited
        /// </summary>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirty()
        {
            return _dirtyFlag;
        }

        /// <summary>
        ///     Get if the item has been edited
        /// </summary>
        /// <param name="id">item ID</param>
        /// <returns>If editedtrue true return it</returns>
        public bool IsDirty(CommandItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(CommandItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (CommandItemId id in Enum.GetValues(typeof (CommandItemId)))
            {
                _dirtyFlags[(int) id] = true;
            }
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Clear all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (CommandItemId id in Enum.GetValues(typeof (CommandItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Command type
    /// </summary>
    public enum CommandType
    {
        None,
        Endgame,
        SetFlag,
        ClrFlag,
        LocalSetFlag,
        LocalClrFlag,
        RegimeFalls,
        Inherit,
        Country,
        AddCore,
        RemoveCore,
        SecedeProvince,
        Control,
        Capital,
        CivilWar,
        ChangeIdea,
        Belligerence,
        Dissent,
        ProvinceRevoltRisk,
        Domestic,
        SetDomestic,
        ChangePolicy,
        ChangePartisanActivity,
        SetPartisanActivity,
        ChangeUnitXp,
        SetUnitXp,
        ChangeLeaderXp,
        SetLeaderXp,
        ChangeRetoolTime,
        SetRetoolTime,
        Independence,
        Alliance,
        LeaveAlliance,
        Relation,
        SetRelation,
        Peace,
        PeaceWithAll,
        War,
        EndPuppet,
        EndMastery,
        MakePuppet,
        CoupNation,
        Access,
        EndAccess,
        AccessToAlliance,
        EndNonAggression,
        NonAggression,
        EndTrades,
        EndGuarantee,
        Guarantee,
        GrantMilitaryControl,
        EndMilitaryControl,
        AddTeamSkill,
        SetTeamSkill,
        AddTeamResearchType,
        RemoveTeamResearchType,
        SleepTeam,
        WakeTeam,
        GiveTeam,
        SleepMinister,
        SleepLeader,
        WakeLeader,
        GiveLeader,
        SetLeaderSkill,
        AddLeaderSkill,
        AddLeaderTrait,
        RemoveLeaderTrait,
        AllowDigIn,
        BuildDivision,
        AddCorps,
        ActivateDivision,
        AddDivision,
        RemoveDivision,
        DamageDivision,
        DisorgDivision,
        DeleteUnit,
        SwitchAllegiance,
        ScrapModel,
        LockDivision,
        UnlockDivision,
        NewModel,
        ActivateUnitType,
        DeactivateUnitType,
        CarrierLevel,
        ResearchSabotaged,
        Deactivate,
        Activate,
        InfoMayCause,
        StealTech,
        GainTech,
        Resource,
        Supplies,
        OilPool,
        MetalPool,
        EnergyPool,
        RareMaterialsPool,
        Money,
        ManPowerPool,
        RelativeManPower,
        ProvinceManPower,
        FreeIc,
        FreeOil,
        FreeSupplies,
        FreeMoney,
        FreeMetal,
        FreeEnergy,
        FreeRareMaterials,
        FreeTransport,
        FreeEscort,
        FreeManPower,
        AddProvResource,
        AllowBuilding,
        Construct,
        AllowConvoyEscorts,
        TransportPool,
        EscortPool,
        Convoy,
        PeaceTimeIcMod,
        TcMod,
        TcOccupiedMod,
        AttritionMod,
        SupplyDistMod,
        RepairMod,
        ResearchMod,
        BuildingProdMod,
        ConvoyProdMod,
        RadarEff,
        EnableTask,
        TaskEfficiency,
        MaxPositioning,
        MinPositioning,
        ProvinceKeyPoints,
        Ai,
        ExtraTc,
        Vp,
        Songs,
        Trigger,
        SleepEvent,
        MaxReactorSize,
        AbombProduction,
        DoubleNukeProd,
        NukeDamage,
        GasAttack,
        GasProtection,
        Revolt,
        AiPrepareWar,
        StartPattern,
        AddToPattern,
        EndPattern,
        SetGround,
        CounterAttack,
        Assault,
        Encirclement,
        Ambush,
        Delay,
        TacticalWithdrawal,
        Breakthrough,
        HqSupplyEff,
        SceFrequency,
        NuclearCarrier,
        MissileCarrier,
        OutOfFuelSpeed,
        NoFuelCombatMod,
        NoSuppliesCombatMod,
        SoftAttack,
        HardAttack,
        Defensiveness,
        AirAttack,
        AirDefense,
        BuildCost,
        BuildTime,
        ManPower,
        Speed,
        MaxOrganization,
        Morale,
        TransportWeight,
        SupplyConsumption,
        FuelConsumption,
        SpeedCapArt,
        SpeedCapEng,
        SpeedCapAt,
        SpeedCapAa,
        ArtilleryBombardment,
        Suppression,
        Softness,
        Toughness,
        StrategicAttack,
        TacticalAttack,
        NavalAttack,
        SurfaceDetection,
        AirDetection,
        SubDetection,
        TransportCapacity,
        Range,
        ShoreAttack,
        NavalDefense,
        Visibility,
        SubAttack,
        ConvoyAttack,
        PlainAttack,
        PlainDefense,
        DesertAttack,
        DesertDefense,
        MountainAttack,
        MountainDefense,
        HillAttack,
        HillDefense,
        ForestAttack,
        ForestDefense,
        SwampAttack,
        SwampDefense,
        JungleAttack,
        JungleDefense,
        UrbanAttack,
        UrbanDefense,
        RiverAttack,
        ParaDropAttack,
        FortAttack,
        PlainMove,
        DesertMove,
        MountainMove,
        HillMove,
        ForestMove,
        SwampMove,
        JungleMove,
        UrbanMove,
        RiverCrossing,
        ClearAttack,
        ClearDefense,
        FrozenAttack,
        FrozenDefense,
        SnowAttack,
        SnowDefense,
        BlizzardAttack,
        BlizzardDefense,
        RainAttack,
        RainDefense,
        StormAttack,
        StormDefense,
        MuddyAttack,
        MuddyDefense,
        FrozenMove,
        SnowMove,
        BlizzardMove,
        RainMove,
        StormMove,
        MuddyMove,
        NightMove,
        NightAttack,
        NightDefense,
        MinisubBonus,
        Surprise,
        Intelligence,
        ArmyDetection,
        AaBatteries,
        IndustrialMultiplier,
        IndustrialModifier,
        TrickleBackMod,
        MaxAmphibMod,
        BuildingEffMod,
        HeadOfState,
        HeadOfGovernment,
        ForeignMinister,
        ArmamentMinister,
        MinisterOfSecurity,
        MinisterOfIntelligence,
        ChiefOfStaff,
        ChiefOfArmy,
        ChiefOfNavy,
        ChiefOfAir,
        AllianceLeader,
        AllianceName,
        Stockpile,
        AutoTrade,
        AutoTradeReset,
        MilitaryControl,
        FlagExt,
        Embargo,
        Name,
        SecedeArea,
        SecedeRegion,
        Trade,
        WarTimeIcMod,
        AddClaim,
        RemoveClaim,
        LandFortEff,
        CoastFortEff,
        ConvoyDefEff,
        GroundDefEff,
        Strength,
        Event,
        WakeMinister,
        Demobilize,
        StrengthCap,
        RemoveUnits,
        BuildBrigade,
        AddCoreArea,
        AddCoreRegion,
        RemoveCoreArea,
        RemoveCoreRegion,
        AddClaimArea,
        AddClaimRegion,
        RemoveClaimArea,
        RemoveClaimRegion
    }

    /// <summary>
    ///     Command item ID
    /// </summary>
    public enum CommandItemId
    {
        Type, // Command type
        Which, // which which Parameters
        Value, // value value Parameters
        When, // when Parameters
        Where // where Parameters
    }
}
