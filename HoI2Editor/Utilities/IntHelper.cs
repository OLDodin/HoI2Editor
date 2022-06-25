using System.Globalization;

namespace HoI2Editor.Utilities
{
    /// <summary>
    ///     Integer helper class
    /// </summary>
    public static class IntHelper
    {
        #region String conversion

        /// <summary>
        ///     Convert to a string
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Converted string</returns>
        public static string ToString(int val)
        {
            return ToString0(val);
        }

        /// <summary>
        ///     Convert to a string
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Converted string</returns>
        public static string ToString0(int val)
        {
            return val.ToString("D", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Convert to a string (( After the decimal point 1 digit )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Converted string</returns>
        public static string ToString1(int val)
        {
            return val.ToString("F1", CultureInfo.InvariantCulture);
        }

        #endregion

        #region Numerical conversion

        /// <summary>
        ///     Convert a string to a number
        /// </summary>
        /// <param name="s">Character string to be converted</param>
        /// <param name="val">Converted value</param>
        /// <returns>If the conversion is successful true true return it</returns>
        public static bool TryParse(string s, out int val)
        {
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out val);
        }

        #endregion
    }
}
