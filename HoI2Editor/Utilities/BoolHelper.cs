namespace HoI2Editor.Utilities
{
    /// <summary>
    ///     Logical helper class
    /// </summary>
    public static class BoolHelper
    {
        /// <summary>
        ///     Yes / No Convert to a string of
        /// </summary>
        /// <param name="b">Value to be converted</param>
        /// <returns>Converted string</returns>
        public static string ToString(bool b)
        {
            return b ? "yes" : "no";
        }
    }
}
