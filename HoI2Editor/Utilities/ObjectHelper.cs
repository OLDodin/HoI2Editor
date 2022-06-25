using System.Globalization;
using HoI2Editor.Models;

namespace HoI2Editor.Utilities
{
    /// <summary>
    ///     Object type helper class
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        ///     Convert object type to string
        /// </summary>
        /// <param name="o">Conversion target</param>
        /// <returns>Character string</returns>
        public static string ToString(object o)
        {
            if (o == null)
            {
                return string.Empty;
            }
            if (o is double)
            {
                return ((double) o).ToString(CultureInfo.InvariantCulture);
            }
            if (o is int)
            {
                return ((int) o).ToString(CultureInfo.InvariantCulture);
            }
            if (o is bool)
            {
                return (bool) o ? "yes" : "no";
            }
            if (o is Country)
            {
                return Countries.Strings[(int) (Country) o];
            }
            return o.ToString();
        }

        /// <summary>
        ///     2 Returns whether two objects have equal values
        /// </summary>
        /// <param name="x">targets for comparison 1</param>
        /// <param name="y">targets for comparison 2</param>
        /// <returns>2 If two objects have equal values true true return it</returns>
        public static bool IsEqual(object x, object y)
        {
            if (x is double && y is double)
            {
                return DoubleHelper.IsEqual((double) x, (double) y);
            }
            return x.Equals(y);
        }

        /// <summary>
        ///     The object is null Returns either the empty string or the empty string
        /// </summary>
        /// <param name="o">Judgment target</param>
        /// <returns>null null Or if it is an empty string true true return it</returns>
        public static bool IsNullOrEmpty(object o)
        {
            return string.IsNullOrEmpty(o as string);
        }
    }
}
