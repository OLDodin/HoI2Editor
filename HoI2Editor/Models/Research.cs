using System;
using System.Linq;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Research speed calculation class
    /// </summary>
    public class Research
    {
        #region Public properties

        /// <summary>
        ///     Target technology
        /// </summary>
        public TechItem Tech { get; private set; }

        /// <summary>
        ///     Target research institute
        /// </summary>
        public Team Team { get; private set; }

        /// <summary>
        ///     Number of days required for research
        /// </summary>
        public int Days { get; }

        /// <summary>
        ///     Date when the study is completed
        /// </summary>
        public GameDate EndDate { get; private set; }

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="tech">Technical items</param>
        /// <param name="team">research Institute</param>
        public Research(TechItem tech, Team team)
        {
            Tech = tech;
            Team = team;
            GameDate date = Researches.DateMode == ResearchDateMode.Specified
                ? Researches.SpecifiedDate
                : new GameDate(tech.Year);
            Days = GetTechDays(tech, team, date);
            EndDate = date.Plus(Days);
        }

        #endregion

        #region Research speed calculation

        /// <summary>
        ///     Get the number of days required for technology research
        /// </summary>
        /// <param name="tech">Technical items</param>
        /// <param name="team">research Institute</param>
        /// <param name="date">start date</param>
        /// <returns>Number of days required for research</returns>
        private static int GetTechDays(TechItem tech, Team team, GameDate date)
        {
            int offset = date.Difference(new GameDate(tech.Year));
            int days = 0;

            // Consideration of the starting year of the research institute
            if (Researches.ConsiderStartYear)
            {
                // The year when the research started is smaller than the year when the research institution started
                if (date.Year < team.StartYear)
                {
                    offset -= date.Difference(new GameDate(team.StartYear));
                    days -= date.Difference(new GameDate(team.StartYear));   /* 研究開始日時と研究機関スタート日だけ実施日を変更 */
                }
            }

            foreach (TechComponent component in tech.Components)
            {
                int day = GetComponentDays(component, offset, team);
                offset += day;
                days += day;
            }

            return days;
        }

        /// <summary>
        ///     Get the basic progress rate of research speed
        /// </summary>
        /// <param name="component">Small study</param>
        /// <param name="team">research Institute</param>
        /// <returns>Basic progress rate of research speed</returns>
        private static double GetBaseProgress(TechComponent component, Team team)
        {
            int d = component.Difficulty + 2;
            int s = team.Skill;

            // If the characteristics match, double the skill 6 Add
            if (team.Specialities.Contains(component.Speciality))
            {
                s += team.Skill + 6;
            }

            // If there is a research facility, set the scale of the facility
            int t = 0;
            switch (component.Speciality)
            {
                case TechSpeciality.Rocketry:
                    t = Researches.RocketTestingSites;
                    break;

                case TechSpeciality.NuclearPhysics:
                case TechSpeciality.NuclearEngineering:
                    t = Researches.NuclearReactors;
                    break;
            }

            // Get the basic progress rate for each game
            double progress;
            switch (Game.Type)
            {
                case GameType.HeartsOfIron2:
                    progress = (9.3 + 1.5 * s + 10 * t) / d;
                    break;

                case GameType.ArsenalOfDemocracy:
                    progress = (3.0 + 0.5 * (s + 10 * Math.Sqrt(t))) / d;
                    break;

                case GameType.DarkestHour:
                    progress = (9.0 + 1.5 * (s + 5.62 * t)) / d;
                    break;

                default:
                    // If you don't know the type of game HoI2 Treat as
                    progress = (9.3 + 1.5 * s + 10 * t) / d;
                    break;
            }

            // 2 In case of double time setting, it is treated as half progress rate
            if (component.DoubleTime)
            {
                progress /= 2;
            }

            // Blueprint correction
            if (Researches.Blueprint)
            {
                progress *= Misc.BlueprintBonus;
            }

            // AoD in the case of Misc Consider the correction of
            if (Game.Type == GameType.ArsenalOfDemocracy)
            {
                progress *= Misc.TechSpeedModifier;
            }

            // Various other corrections (( Computer technology / / Minister / / difficulty / / Scenario settings, etc. )
            progress *= Researches.Modifier;

            return progress;
        }

        /// <summary>
        ///     Get the number of days required for a quiz
        /// </summary>
        /// <param name="component">Small study</param>
        /// <param name="offset">Number of days difference from the historical year</param>
        /// <param name="team">research Institute</param>
        /// <returns>Days</returns>
        private static int GetComponentDays(TechComponent component, int offset, Team team)
        {
            int totalDays = 0;
            double totalProgress = 0;

            // Find the standard progress rate
            double baseProgress = GetBaseProgress(component, team);

            // STEP1: STEP1: Find the number of days to study at the lower limit of the pre-historical penalty
            if ((offset < 0) && (Misc.PreHistoricalDateModifier < 0))
            {
                // Seeking a pre-historical penalty
                double preHistoricalModifier = Misc.PreHistoricalDateModifier;

                // Find the lower limit of the pre-historical penalty
                double preHistoricalLimit = Game.Type == GameType.ArsenalOfDemocracy
                    ? Misc.PreHistoricalPenaltyLimit
                    : 0.1;

                // Find the day when the pre-historical penalty reaches the lower limit
                int preHistoricalLimitOffset = (int) Math.Floor((1 - preHistoricalLimit) / preHistoricalModifier);

                // When the number of days of difference exceeds the lower limit of days
                if (offset <= preHistoricalLimitOffset)
                {
                    // Find the number of days to complete the study at the lower limit
                    int preHistoricalLimitDays = (int) Math.Ceiling(100 / (baseProgress * preHistoricalLimit));

                    // Returns the number of days if the study is completed at the lower limit
                    if (offset + preHistoricalLimitDays <= preHistoricalLimitOffset)
                    {
                        return preHistoricalLimitDays;
                    }

                    // Add the number of days to study and the progress at the lower limit
                    preHistoricalLimitDays = preHistoricalLimitOffset - offset;
                    totalDays = preHistoricalLimitDays;
                    totalProgress = baseProgress * preHistoricalLimit * preHistoricalLimitDays;
                    offset += preHistoricalLimitDays;
                }
            }

            // STEP2: Find the number of days to study with a pre-year penalty that does not reach the lower limit
            if (offset < 0)
            {
                // Seeking a pre-historical penalty
                double preHistoricalModifier = Misc.PreHistoricalDateModifier;

                // Find the number of days to study with a penalty before the historical year
                int preHistricalDays = GetPreHistoricalDays(baseProgress, 100 - totalProgress, offset,
                    preHistoricalModifier);

                // Returns the number of days if the study is completed before the historical year
                if (offset + preHistricalDays <= 0)
                {
                    totalDays += preHistricalDays;
                    return totalDays;
                }

                // Add the number of days and progress to study before the historical year
                preHistricalDays = -offset;
                totalDays += preHistricalDays;
                totalProgress += GetPreHistoricalProgress(baseProgress, preHistricalDays, offset, preHistoricalModifier);
                offset = 0;
            }

            // STEP3: Find the number of days to study without correction before and after the historical year

            // HoI2 In the case of, there is no post-correction after the historical year
            if (Game.Type == GameType.HeartsOfIron2)
            {
                totalDays += (int) Math.Ceiling((100 - totalProgress) / baseProgress);
                return totalDays;
            }

            // Ask for a bonus after the historical year
            double postHistoricalModifier = Game.Type == GameType.ArsenalOfDemocracy
                ? Misc.PostHistoricalDateModifierAoD
                : Misc.PostHistoricalDateModifierDh;

            // If there is no bonus after the historical year
            if (postHistoricalModifier <= 0)
            {
                totalDays += (int) Math.Ceiling((100 - totalProgress) / baseProgress);
                return totalDays;
            }

            // DH in the case of, 1 No correction until the end of the year
            if (Game.Type == GameType.DarkestHour)
            {
                // Rocket technology / / Post-historical bonuses do not apply to nuclear technology
                switch (component.Speciality)
                {
                    case TechSpeciality.Rocketry:
                    case TechSpeciality.NuclearPhysics:
                    case TechSpeciality.NuclearEngineering:
                        totalDays += (int) Math.Ceiling((100 - totalProgress) / baseProgress);
                        return totalDays;
                }

                offset -= 360;
                if (offset < 0)
                {
                    // Find the number of days to study after the historical year without bonus
                    int historicalDays = (int) Math.Ceiling((100 - totalProgress) / baseProgress);

                    // When the research is completed at an institution without bonus after the historical year
                    if (offset + historicalDays < 0)
                    {
                        totalDays += historicalDays;
                        return totalDays;
                    }

                    // Add days and progress to study without bonus after historical year
                    historicalDays = -offset;
                    totalDays += historicalDays;
                    totalProgress += baseProgress * historicalDays;
                    offset = 0;
                }
            }

            // STEP4: Find the number of days to study with a bonus after the historical year when the upper limit is not reached

            // Find the upper limit of the bonus after the historical year
            double postHistoricalLimit = Game.Type == GameType.ArsenalOfDemocracy
                ? Misc.PostHistoricalBonusLimit
                : Misc.BlueprintBonus;

            // Find the day when the bonus will reach the upper limit after the historical year
            int postHistoricalLimitOffset =
                (int) Math.Ceiling(Math.Abs((postHistoricalLimit - 1) / postHistoricalModifier));

            if (offset < postHistoricalLimitOffset)
            {
                // Find the number of days to study with a bonus after the historical year
                int postHistoricalDays = GetPostHistoricalDays(baseProgress, 100 - totalProgress, offset,
                    postHistoricalModifier);

                // If the study is completed before the bonus reaches the upper limit after the historical year
                if (offset + postHistoricalDays < postHistoricalLimitOffset)
                {
                    totalDays += postHistoricalDays;
                    return totalDays;
                }

                // Add the number of days and progress to study after the historical year
                postHistoricalDays = postHistoricalLimitOffset - offset - 1;
                totalDays += postHistoricalDays;
                totalProgress += GetPostHistoricalProgress(baseProgress, postHistoricalDays, offset,
                    postHistoricalModifier);
            }

            // STEP5: Find the number of days to study with the bonus upper limit after the historical year
            totalDays += (int) Math.Ceiling((100 - totalProgress) / (baseProgress * postHistoricalLimit));
            return totalDays;
        }

        /// <summary>
        ///     Obtain the number of days required for research before the historical year
        /// </summary>
        /// <param name="progress">Basic progress rate</param>
        /// <param name="target">Target progress rate</param>
        /// <param name="offset">Number of days difference from the historical year</param>
        /// <param name="modifier">1 Daily progress rate correction</param>
        /// <returns>Days</returns>
        private static int GetPreHistoricalDays(double progress, double target, int offset, double modifier)
        {
            return (int) Math.Ceiling(GetPositiveSolutionQuadraticEquation(
                -progress * modifier / 2,
                progress / 2 * (2 - (2 * offset + 1) * modifier),
                -target));
        }

        /// <summary>
        ///     Obtain the progress rate considering the pre-historical correction
        /// </summary>
        /// <param name="progress">Basic progress rate</param>
        /// <param name="offset">Number of days difference from the historical year</param>
        /// <param name="days">Target days</param>
        /// <param name="modifier">1 Daily progress rate correction</param>
        /// <returns>Progress rate</returns>
        private static double GetPreHistoricalProgress(double progress, int days, int offset, double modifier)
        {
            return progress * days * (2 - (2 * offset + days + 1) * modifier) / 2;
        }

        /// <summary>
        ///     Obtain the number of days required for research after the historical year
        /// </summary>
        /// <param name="progress">Basic progress rate</param>
        /// <param name="target">Target progress rate</param>
        /// <param name="offset">Number of days difference from the historical year</param>
        /// <param name="modifier">1 Daily progress rate correction</param>
        /// <returns>Days</returns>
        private static int GetPostHistoricalDays(double progress, double target, int offset, double modifier)
        {
            return (int) Math.Ceiling(GetPositiveSolutionQuadraticEquation(
                progress * modifier / 2,
                progress / 2 * (2 + (2 * offset + 1) * modifier),
                -target));
        }

        /// <summary>
        ///     Obtain the progress rate considering the post-historical correction
        /// </summary>
        /// <param name="progress">Basic progress rate</param>
        /// <param name="offset">Number of days difference from the historical year</param>
        /// <param name="days">Target days</param>
        /// <param name="modifier">1 Daily progress rate correction</param>
        /// <returns>Progress rate</returns>
        private static double GetPostHistoricalProgress(double progress, int days, int offset, double modifier)
        {
            return progress * days * (2 + (2 * offset + days + 1) * modifier) / 2;
        }

        /// <summary>
        ///     2 Find the positive solution of the following equation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns>Returns only positive answers</returns>
        /// <remarks>
        ///     a * x ^ 2 + b * x + c = 0
        ///     b'= b / 2a, c'= c / a As
        ///     x = -b'+-sqrt (b'^ 2 --c)
        ///     this house + Is the correct answer
        /// </ remarks>
        private static double GetPositiveSolutionQuadraticEquation(double a, double b, double c)
        {
            double bb = b / a / 2;
            double cc = c / a;
            return -bb + Math.Sqrt(bb * bb - cc);
        }

        #endregion
    }
}
