using System;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Research institution data
    /// </summary>
    public class Team
    {
        #region Public properties

        /// <summary>
        ///     Country tag
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        ///     research Institute ID
        /// </summary>
        public int Id
        {
            get { return _id; }
            set
            {
                Teams.IdSet.Remove(_id);
                _id = value;
                Teams.IdSet.Add(_id);
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
        ///     skill
        /// </summary>
        public int Skill { get; set; }

        /// <summary>
        ///     Start year
        /// </summary>
        public int StartYear { get; set; }

        /// <summary>
        ///     End year
        /// </summary>
        public int EndYear { get; set; }

        /// <summary>
        ///     Research characteristics
        /// </summary>
        public TechSpeciality[] Specialities { get; } = new TechSpeciality[SpecialityLength];

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag for item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (TeamItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        /// <summary>
        ///     research Institute ID
        /// </summary>
        private int _id;

        #endregion

        #region Public constant

        /// <summary>
        ///     Number of study characteristic definitions
        /// </summary>
        public const int SpecialityLength = 32;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public Team()
        {
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">Data from the research institute from which it was duplicated</param>
        public Team(Team original)
        {
            Country = original.Country;
            Id = original.Id;
            Name = original.Name;
            PictureName = original.PictureName;
            Skill = original.Skill;
            StartYear = original.StartYear;
            EndYear = original.EndYear;
            for (int i = 0; i < SpecialityLength; i++)
            {
                Specialities[i] = original.Specialities[i];
            }
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get whether the research institute data has been edited
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
        public bool IsDirty(TeamItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">item ID</param>
        public void SetDirty(TeamItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (TeamItemId id in Enum.GetValues(typeof (TeamItemId)))
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
            foreach (TeamItemId id in Enum.GetValues(typeof (TeamItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Research institute items ID
    /// </summary>
    public enum TeamItemId
    {
        Country, // Nation
        Id, // ID
        Name, // name
        Skill, // skill
        StartYear, // Start year
        EndYear, // End year
        PictureName, // Image file name
        Speciality1, // Research characteristics 1
        Speciality2, // Research characteristics 2
        Speciality3, // Research characteristics 3
        Speciality4, // Research characteristics Four
        Speciality5, // Research characteristics Five
        Speciality6, // Research characteristics 6
        Speciality7, // Research characteristics 7
        Speciality8, // Research characteristics 8
        Speciality9, // Research characteristics 9
        Speciality10, // Research characteristics Ten
        Speciality11, // Research characteristics 11 11
        Speciality12, // Research characteristics 12
        Speciality13, // Research characteristics 13
        Speciality14, // Research characteristics 14
        Speciality15, // Research characteristics 15
        Speciality16, // Research characteristics 16 16
        Speciality17, // Research characteristics 17 17
        Speciality18, // Research characteristics 18 18
        Speciality19, // Research characteristics 19 19
        Speciality20, // Research characteristics 20
        Speciality21, // Research characteristics twenty one
        Speciality22, // Research characteristics twenty two
        Speciality23, // Research characteristics twenty three
        Speciality24, // Research characteristics twenty four
        Speciality25, // Research characteristics twenty five
        Speciality26, // Research characteristics 26
        Speciality27, // Research characteristics 27
        Speciality28, // Research characteristics 28 28
        Speciality29, // Research characteristics 29
        Speciality30, // Research characteristics 30
        Speciality31, // Research characteristics 31
        Speciality32 // Research characteristics 32
    }
}
