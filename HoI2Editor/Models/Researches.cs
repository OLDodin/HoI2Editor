using System.Collections.Generic;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Research velocity data group
    /// </summary>
    public static class Researches
    {
        #region Public properties

        /// <summary>
        ///     Research speed list
        /// </summary>
        public static readonly List<Research> Items = new List<Research>();

        /// <summary>
        ///     Base year when calculating research speed
        /// </summary>
        public static ResearchDateMode DateMode { get; set; }

        /// <summary>
        ///     Specified date
        /// </summary>
        public static GameDate SpecifiedDate { get; set; }

        /// <summary>
        ///     Scale of rocket test site
        /// </summary>
        public static int RocketTestingSites { get; set; }

        /// <summary>
        ///     Reactor scale
        /// </summary>
        public static int NuclearReactors { get; set; }

        /// <summary>
        ///     Presence or absence of blueprint
        /// </summary>
        public static bool Blueprint { get; set; }

        /// <summary>
        ///     Research speed correction
        /// </summary>
        public static double Modifier { get; set; }

        /// <summary>
        ///     Consideration of the starting year of the research institute
        /// </summary>
        public static bool ConsiderStartYear { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Researches()
        {
            SpecifiedDate = new GameDate();
            Modifier = 1;
        }

        #endregion

        #region Research speed list

        /// <summary>
        ///     Update research speed list
        /// </summary>
        /// <param name="tech">Target technology</param>
        /// <param name="teams">research Institute</param>
        public static void UpdateResearchList(TechItem tech, IEnumerable<Team> teams)
        {
            Items.Clear();

            // Register research speeds in order
            foreach (Team team in teams)
            {
                Research research = new Research(tech, team);

                // When considering the start and end years of a research institution
                if( ConsiderStartYear )
                {
                    GameDate date;    
                    if( DateMode == ResearchDateMode.Specified )
                    {
                        date = SpecifiedDate;
                    } else
                    {
                        date = new GameDate(tech.Year);
                    }
                    // The research institution is past the end year
                    if ( team.EndYear <= date.Year )
                    {
                        continue;   /* リストに入れない */
                    }
                }

                Items.Add(research);
            }

            // Sort by number of study days
            Items.Sort((research1, research2) => research1.Days - research2.Days);
        }

        #endregion
    }

    /// <summary>
    ///     Reference date mode when calculating research speed
    /// </summary>
    public enum ResearchDateMode
    {
        Historical, // Use historical year
        Specified // Use the specified date
    }
}
