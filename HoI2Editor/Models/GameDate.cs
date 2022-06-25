using System.Globalization;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     In-game date
    /// </summary>
    public class GameDate
    {
        #region Public properties

        /// <summary>
        ///     Year
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        ///     Moon
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        ///     Day
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        ///     Time
        /// </summary>
        public int Hour { get; set; }

        #endregion

        #region Public constant

        /// <summary>
        ///     Minimum of the year
        /// </summary>
        public const int MinYear = 0;

        /// <summary>
        ///     Maximum value of the year
        /// </summary>
        public const int MaxYear = 9999;

        /// <summary>
        ///     Monthly minimum
        /// </summary>
        public const int MinMonth = 1;

        /// <summary>
        ///     Monthly maximum
        /// </summary>
        public const int MaxMonth = 12;

        /// <summary>
        ///     Minimum of days
        /// </summary>
        public const int MinDay = 1;

        /// <summary>
        ///     Maximum value of the day
        /// </summary>
        public const int MaxDay = 30;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Moon</param>
        /// <param name="day">Day</param>
        public GameDate(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="year">Year</param>
        public GameDate(int year)
        {
            Year = year;
            Month = 1;
            Day = 1;
        }

        /// <summary>
        ///     constructor
        /// </summary>
        public GameDate()
        {
            Year = 1936;
            Month = 1;
            Day = 1;
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="original">Copy source game date and time</param>
        public GameDate(GameDate original)
        {
            Year = original.Year;
            Month = original.Month;
            Day = original.Day;
            Hour = original.Hour;
        }

        #endregion

        #region Days calculation

        /// <summary>
        ///     Add the number of days
        /// </summary>
        /// <param name="days">Number of days to add</param>
        /// <returns>Date after addition</returns>
        public GameDate Plus(int days)
        {
            int offset = Year * 360 + (Month - 1) * 30 + (Day - 1) + days;

            int year = offset / 360;
            offset -= year * 360;
            int month = offset / 30 + 1;
            int day = offset % 30 + 1;

            return new GameDate(year, month, day);
        }

        /// <summary>
        ///     Subtract the number of days
        /// </summary>
        /// <param name="days">Number of days to subtract</param>
        /// <returns>Date after addition</returns>
        public GameDate Minus(int days)
        {
            int offset = Year * 360 + (Month - 1) * 30 + (Day - 1) - days;

            int year = offset / 360;
            offset -= year * 360;
            int month = offset / 30 + 1;
            int day = offset % 30 + 1;

            return new GameDate(year, month, day);
        }

        /// <summary>
        ///     Get the difference days
        /// </summary>
        /// <param name="date">Date to compare</param>
        /// <returns>Difference days</returns>
        public int Difference(GameDate date)
        {
            return (Year - date.Year) * 360 + (Month - date.Month) * 30 + (Day - date.Day);
        }

        /// <summary>
        ///     Get the difference days
        /// </summary>
        /// <param name="year">Year to compare</param>
        /// <returns>Difference days</returns>
        public int Difference(int year)
        {
            return (Year - year) * 360 + (Month - 1) * 30 + (Day - 1);
        }

        #endregion

        #region String operation

        /// <summary>
        ///     Get the date string
        /// </summary>
        /// <returns>Date string</returns>
        public override string ToString()
        {
            string format = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern;
            format = format.Replace("yyyy", "{0}");
            format = format.Replace("MM", "{1:D2}");
            format = format.Replace("M", "{1}");
            format = format.Replace("dd", "{2:D2}");
            format = format.Replace("d", "{2}");
            return string.Format(format, Year, Month, Day);
        }

        #endregion
    }
}
