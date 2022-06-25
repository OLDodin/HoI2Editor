using System;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Commander data
    /// </summary>
    public class Leader
    {
        #region Public properties

        /// <summary>
        ///     Country tag
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        ///     Commander ID
        /// </summary>
        public int Id
        {
            get { return _id; }
            set
            {
                Leaders.IdSet.Remove(_id);
                _id = value;
                Leaders.IdSet.Add(_id);
            }
        }

        /// <summary>
        ///     name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Image file name
        /// </summary>
        public string PictureName { get; set; }

        /// <summary>
        ///     Initial skills
        /// </summary>
        public int Skill { get; set; }

        /// <summary>
        ///     Maximum skill
        /// </summary>
        public int MaxSkill { get; set; }

        /// <summary>
        ///     Year of appointment
        /// </summary>
        public int[] RankYear { get; } = new int[RankLength];

        /// <summary>
        ///     Start year
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        ///     End year
        /// </summary>
        public int EndYear { get; set; }

        /// <summary>
        ///     Retirement year
        /// </summary>
        public int RetirementYear { get; set; }

        /// <summary>
        ///     Ideal class
        /// </summary>
        public LeaderRank IdealRank { get; set; }

        /// <summary>
        ///     Commander characteristics
        /// </summary>
        public uint Traits { get; set; }

        /// <summary>
        ///     Experience point
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        ///     Loyalty
        /// </summary>
        public int Loyalty { get; set; }

        /// <summary>
        ///     Army
        /// </summary>
        public Branch Branch { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (LeaderItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        /// <summary>
        ///     Commander ID
        /// </summary>
        private int _id;

        #endregion

        #region Public constant

        /// <summary>
        ///     Number of classes
        /// </summary>
        public const int RankLength = 4;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public Leader()
        {
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">Original commander data</param>
        public Leader(Leader original)
        {
            Country = original.Country;
            Id = original.Id;
            Name = original.Name;
            PictureName = original.PictureName;
            Skill = original.Skill;
            MaxSkill = original.MaxSkill;
            for (int i = 0; i < RankLength; i++)
            {
                RankYear[i] = original.RankYear[i];
            }
            StartYear = original.StartYear;
            EndYear = original.EndYear;
            RetirementYear = original.RetirementYear;
            IdealRank = original.IdealRank;
            Traits = original.Traits;
            Experience = original.Experience;
            Loyalty = original.Loyalty;
            Branch = original.Branch;
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if the commander data has been edited
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
        public bool IsDirty(LeaderItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(LeaderItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (LeaderItemId id in Enum.GetValues(typeof (LeaderItemId)))
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
            foreach (LeaderItemId id in Enum.GetValues(typeof (LeaderItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Commander characteristic value
    /// </summary>
    public static class LeaderTraits
    {
        #region Public constant

        /// <summary>
        ///     No characteristics
        /// </summary>
        public const uint None = 0x00000000;

        /// <summary>
        ///     Station management
        /// </summary>
        public const uint LogisticsWizard = 0x00000001;

        /// <summary>
        ///     Defensive doctrine
        /// </summary>
        public const uint DefensiveDoctrine = 0x00000002;

        /// <summary>
        ///     Offensive doctrine
        /// </summary>
        public const uint OffensiveDoctrine = 0x00000004;

        /// <summary>
        ///     Winter battle
        /// </summary>
        public const uint WinterSpecialist = 0x00000008;

        /// <summary>
        ///     Assault
        /// </summary>
        public const uint Trickster = 0x00000010;

        /// <summary>
        ///     Engineer
        /// </summary>
        public const uint Engineer = 0x00000020;

        /// <summary>
        ///     Fortress attack
        /// </summary>
        public const uint FortressBuster = 0x00000040;

        /// <summary>
        ///     Armored battle
        /// </summary>
        public const uint PanzerLeader = 0x00000080;

        /// <summary>
        ///     Special battle
        /// </summary>
        public const uint Commando = 0x00000100;

        /// <summary>
        ///     Classic school
        /// </summary>
        public const uint OldGuard = 0x00000200;

        /// <summary>
        ///     Sea wolf
        /// </summary>
        public const uint SeaWolf = 0x00000400;

        /// <summary>
        ///     Master of blocking line breakthrough
        /// </summary>
        public const uint BlockadeRunner = 0x00000800;

        /// <summary>
        ///     Outstanding tactician
        /// </summary>
        public const uint SuperiorTactician = 0x00001000;

        /// <summary>
        ///     Searching enemy
        /// </summary>
        public const uint Spotter = 0x00002000;

        /// <summary>
        ///     Anti-tank attack
        /// </summary>
        public const uint TankBuster = 0x00004000;

        /// <summary>
        ///     Rug bombing
        /// </summary>
        public const uint CarpetBomber = 0x00008000;

        /// <summary>
        ///     Night air operations
        /// </summary>
        public const uint NightFlyer = 0x00010000;

        /// <summary>
        ///     Anti-ship attack
        /// </summary>
        public const uint FleetDestroyer = 0x00020000;

        /// <summary>
        ///     Desert fox
        /// </summary>
        public const uint DesertFox = 0x00040000;

        /// <summary>
        ///     Mice in the jungle
        /// </summary>
        public const uint JungleRat = 0x00080000;

        /// <summary>
        ///     City war
        /// </summary>
        public const uint UrbanWarfareSpecialist = 0x00100000;

        /// <summary>
        ///     Ranger
        /// </summary>
        public const uint Ranger = 0x00200000;

        /// <summary>
        ///     Mountain battle
        /// </summary>
        public const uint Mountaineer = 0x00400000;

        /// <summary>
        ///     Highland battle
        /// </summary>
        public const uint HillsFighter = 0x00800000;

        /// <summary>
        ///     Counterattack
        /// </summary>
        public const uint CounterAttacker = 0x01000000;

        /// <summary>
        ///     Assault battle
        /// </summary>
        public const uint Assaulter = 0x02000000;

        /// <summary>
        ///     Siege
        /// </summary>
        public const uint Encircler = 0x04000000;

        /// <summary>
        ///     Surprise battle
        /// </summary>
        public const uint Ambusher = 0x08000000;

        /// <summary>
        ///     Discipline
        /// </summary>
        public const uint Disciplined = 0x10000000;

        /// <summary>
        ///     Tactical retreat
        /// </summary>
        public const uint ElasticDefenceSpecialist = 0x20000000;

        /// <summary>
        ///     Blitzkrieg
        /// </summary>
        public const uint Blitzer = 0x40000000;

        /// <summary>
        ///     Army characteristics
        /// </summary>
        public const uint ArmyTraits =
            LogisticsWizard | DefensiveDoctrine | OffensiveDoctrine | WinterSpecialist | Trickster | Engineer |
            FortressBuster | PanzerLeader | Commando | OldGuard | DesertFox | JungleRat | UrbanWarfareSpecialist |
            Ranger | Mountaineer | HillsFighter | CounterAttacker | Assaulter | Encircler | Ambusher | Disciplined |
            ElasticDefenceSpecialist | Blitzer;

        /// <summary>
        ///     Navy characteristics
        /// </summary>
        public const uint NavyTraits = OldGuard | SeaWolf | BlockadeRunner | SuperiorTactician | Spotter | NightFlyer;

        /// <summary>
        ///     Air Force characteristics
        /// </summary>
        public const uint AirforceTraits =
            OldGuard | SuperiorTactician | Spotter | TankBuster | CarpetBomber | NightFlyer | FleetDestroyer;

        #endregion
    }

    /// <summary>
    ///     Commander characteristics
    /// </summary>
    public enum LeaderTraitsId
    {
        LogisticsWizard, // Station management
        DefensiveDoctrine, // Defensive doctrine
        OffensiveDoctrine, // Offensive doctrine
        WinterSpecialist, // Winter battle
        Trickster, // Assault
        Engineer, // Engineer
        FortressBuster, // Fortress attack
        PanzerLeader, // Armored battle
        Commando, // Special battle
        OldGuard, // Classic school
        SeaWolf, // Sea wolf
        BlockadeRunner, // Master of blocking line breakthrough
        SuperiorTactician, // Outstanding tactician
        Spotter, // Searching enemy
        TankBuster, // Anti-tank attack
        CarpetBomber, // Rug bombing
        NightFlyer, // Night air operations
        FleetDestroyer, // Anti-ship attack
        DesertFox, // Desert fox
        JungleRat, // Dense forest rat
        UrbanWarfareSpecialist, // Urban warfare
        Ranger, // Ranger
        Mountaineer, // Mountain warfare
        HillsFighter, // The Front Line
        CounterAttacker, // Counterattack
        Assaulter, // Assault battle
        Encircler, // Siege
        Ambusher, // Surprise battle
        Disciplined, // Discipline
        ElasticDefenceSpecialist, // Tactical retreat
        Blitzer // Blitzkrieg
    }

    /// <summary>
    ///     Commander class
    /// </summary>
    public enum LeaderRank
    {
        None,
        MajorGeneral, // Admiral
        LieutenantGeneral, // Lieutenant General
        General, // General
        Marshal // Marshal
    }

    /// <summary>
    ///     Commander item ID
    /// </summary>
    public enum LeaderItemId
    {
        Country, // Nation
        Id, // ID
        Name, // name
        Branch, // Army
        IdealRank, // Ideal class
        Skill, // skill
        MaxSkill, // Maximum skill
        Experience, // Experience point
        Loyalty, // Loyalty
        StartYear, // Start year
        EndYear, // End year
        RetirementYear, // Retirement year
        Rank3Year, // Major General Year
        Rank2Year, // Year of middle general
        Rank1Year, // General Year
        Rank0Year, // Marshal Year
        PictureName, // Image file name
        LogisticsWizard, // Characteristic :: Station management
        DefensiveDoctrine, // Characteristic :: Defensive doctrine
        OffensiveDoctrine, // Characteristic :: Offensive doctrine
        WinterSpecialist, // Characteristic :: Winter battle
        Trickster, // Characteristic :: Assault
        Engineer, // Characteristic :: Engineer
        FortressBuster, // Characteristic :: Fortress attack
        PanzerLeader, // Characteristic :: Armored warfare
        Commando, // Characteristic :: Special battle
        OldGuard, // Characteristic :: Classic school
        SeaWolf, // Characteristic :: Sea wolf
        BlockadeRunner, // Characteristic :: Master of blocking line breakthrough
        SuperiorTactician, // Characteristic :: Outstanding tactician
        Spotter, // Characteristic :: Searching enemy
        TankBuster, // Characteristic :: Anti-tank attack
        CarpetBomber, // Characteristic :: Rug bombing
        NightFlyer, // Characteristic :: Night aviation operation
        FleetDestroyer, // Characteristic :: Anti-ship attack
        DesertFox, // Characteristic :: Desert fox
        JungleRat, // Characteristic :: Mice in the jungle
        UrbanWarfareSpecialist, // Characteristic :: City war
        Ranger, // Characteristic :: Ranger
        Mountaineer, // Characteristic :: Mountain battle
        HillsFighter, // Characteristic :: The Front Line
        CounterAttacker, // Characteristic :: Counterattack
        Assaulter, // Characteristic :: Assault battle
        Encircler, // Characteristic :: Siege
        Ambusher, // Characteristic :: Surprise battle
        Disciplined, // Characteristic :: Discipline
        ElasticDefenceSpecialist, // Characteristic :: Tactical retreat
        Blitzer // Characteristic :: Blitzkrieg
    }
}
