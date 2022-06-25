using System;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Ministerial data
    /// </summary>
    public class Minister
    {
        #region Public properties

        /// <summary>
        ///     Country tag
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        ///     Minister ID
        /// </summary>
        public int Id
        {
            get { return _id; }
            set
            {
                Ministers.IdSet.Remove(_id);
                _id = value;
                Ministers.IdSet.Add(_id);
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
        ///     Ministerial status
        /// </summary>
        public MinisterPosition Position { get; set; }

        /// <summary>
        ///     Ministerial characteristics
        /// </summary>
        public int Personality { get; set; }

        /// <summary>
        ///     Loyalty
        /// </summary>
        public MinisterLoyalty Loyalty { get; set; }

        /// <summary>
        ///     ideology
        /// </summary>
        public MinisterIdeology Ideology { get; set; }

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

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (MinisterItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        /// <summary>
        ///     Minister ID
        /// </summary>
        private int _id;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public Minister()
        {
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">Copy source ministerial data</param>
        public Minister(Minister original)
        {
            Country = original.Country;
            Id = original.Id;
            Name = original.Name;
            PictureName = original.PictureName;
            Position = original.Position;
            Personality = original.Personality;
            Loyalty = original.Loyalty;
            Ideology = original.Ideology;
            StartYear = original.StartYear;
            EndYear = original.EndYear;
            RetirementYear = original.RetirementYear;
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get if ministerial data has been edited
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
        public bool IsDirty(MinisterItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(MinisterItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (MinisterItemId id in Enum.GetValues(typeof (MinisterItemId)))
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
            foreach (MinisterItemId id in Enum.GetValues(typeof (MinisterItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Ministerial status
    /// </summary>
    public enum MinisterPosition
    {
        None,
        HeadOfState, // National leader
        HeadOfGovernment, // Government leaders
        ForeignMinister, // Minister of Foreign Affairs
        MinisterOfArmament, // Minister of Military Demand
        MinisterOfSecurity, // Minister of Interior
        HeadOfMilitaryIntelligence, // Minister of Information
        ChiefOfStaff, // Chief of the Defense Staff
        ChiefOfArmy, // Army General Commander
        ChiefOfNavy, // Navy Commander
        ChiefOfAirForce // Air Force Commander
    }

    /// <summary>
    ///     Ministerial loyalty
    /// </summary>
    public enum MinisterLoyalty
    {
        None,
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh,
        Undying,
        Na
    }

    /// <summary>
    ///     ideology
    /// </summary>
    public enum MinisterIdeology
    {
        None,
        NationalSocialist, // NS National socialism
        Fascist, // FA fascist
        PaternalAutocrat, // PA Authorist
        SocialConservative, // SC Social conservatives
        MarketLiberal, // ML Free economics
        SocialLiberal, // SL Social liberal
        SocialDemocrat, // SD Social democracy
        LeftWingRadical, // LWR Radical left wing
        Leninist, // LELeninist
        Stalinist // ST Stalinist
    }

    /// <summary>
    ///     Ministerial items ID
    /// </summary>
    public enum MinisterItemId
    {
        Country, // Nation
        Id, // ID
        Name, // name
        StartYear, // Start year
        EndYear, // End year
        RetirementYear, // Retirement year
        Position, // Ministerial status
        Personality, // Ministerial characteristics
        Ideology, // ideology
        Loyalty, // Loyalty
        PictureName // Image file name
    }
}
