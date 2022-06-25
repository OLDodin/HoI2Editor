using System;
using System.Globalization;

namespace HoI2Editor.Utilities
{
    /// <summary>
    ///     Real type helper class
    /// </summary>
    public static class DoubleHelper
    {
        #region Numerical comparison

        /// <summary>
        ///     Determine if the numbers are equal
        /// </summary>
        /// <param name="val1">Numerical value 1</param>
        /// <param name="val2">Numerical value 2</param>
        /// <returns>If the numbers are equal true true return it</returns>
        public static bool IsEqual(double val1, double val2)
        {
            // After the decimal point 6 Consider up to digits
            return Math.Abs(val1 - val2) < 0.0000005;
        }

        /// <summary>
        ///     Determine if the numbers are equal (( No subdecimal )
        /// </summary>
        /// <param name="val1">Numerical value 1</param>
        /// <param name="val2">Numerical value 2</param>
        /// <returns>If the numbers are equal true true return it</returns>
        public static bool IsEqual0(double val1, double val2)
        {
            return Math.Abs(val1 - val2) < 0.5;
        }

        /// <summary>
        ///     Determine if the numbers are equal (( After the decimal point 1 digit )
        /// </summary>
        /// <param name="val1">Numerical value 1</param>
        /// <param name="val2">Numerical value 2</param>
        /// <returns>If the numbers are equal true true return it</returns>
        public static bool IsEqual1(double val1, double val2)
        {
            return Math.Abs(val1 - val2) < 0.05;
        }

        /// <summary>
        ///     Determine if the numbers are equal (( After the decimal point 2 digit )
        /// </summary>
        /// <param name="val1">Numerical value 1</param>
        /// <param name="val2">Numerical value 2</param>
        /// <returns>If the numbers are equal true true return it</returns>
        public static bool IsEqual2(double val1, double val2)
        {
            return Math.Abs(val1 - val2) < 0.005;
        }

        /// <summary>
        ///     Determine if the numbers are equal (( After the decimal point 3 digit )
        /// </summary>
        /// <param name="val1">Numerical value 1</param>
        /// <param name="val2">Numerical value 2</param>
        /// <returns>If the numbers are equal true true return it</returns>
        public static bool IsEqual3(double val1, double val2)
        {
            return Math.Abs(val1 - val2) < 0.0005;
        }

        /// <summary>
        ///     Determine if the numbers are equal (( After the decimal point Four digit )
        /// </summary>
        /// <param name="val1">Numerical value 1</param>
        /// <param name="val2">Numerical value 2</param>
        /// <returns>If the numbers are equal true true return it</returns>
        public static bool IsEqual4(double val1, double val2)
        {
            return Math.Abs(val1 - val2) < 0.00005;
        }

        /// <summary>
        ///     Determine if the numbers are equal (( After the decimal point Five digit )
        /// </summary>
        /// <param name="val1">Numerical value 1</param>
        /// <param name="val2">Numerical value 2</param>
        /// <returns>If the numbers are equal true true return it</returns>
        public static bool IsEqual5(double val1, double val2)
        {
            return Math.Abs(val1 - val2) < 0.000005;
        }

        /// <summary>
        ///     Determine if the numbers are equal (( After the decimal point 6 digit )
        /// </summary>
        /// <param name="val1">Numerical value 1</param>
        /// <param name="val2">Numerical value 2</param>
        /// <returns>If the numbers are equal true true return it</returns>
        public static bool IsEqual6(double val1, double val2)
        {
            return Math.Abs(val1 - val2) < 0.0000005;
        }

        /// <summary>
        ///     The numbers are 0 To determine if it is equal to
        /// </summary>
        /// <param name="val">Numerical value</param>
        /// <returns>The numbers are 0 If equal to true true return it</returns>
        public static bool IsZero(double val)
        {
            return Math.Abs(val) < 0.0000005;
        }

        /// <summary>
        ///     Determine if the value is less than or equal to the specified value
        /// </summary>
        /// <param name="val1">Numerical value 1</param>
        /// <param name="val2">Numerical value 2</param>
        /// <returns>Numerical value 1 Is a numerical value 2 If true true return it</returns>
        public static bool IsLessOrEqual(double val1, double val2)
        {
            return val1 - val2 < 0.0000005;
        }

        /// <summary>
        ///     Determine if the value is greater than or equal to the specified value
        /// </summary>
        /// <param name="val1">Numerical value 1</param>
        /// <param name="val2">Numerical value 2</param>
        /// <returns>Numerical value 1Is a numerical value 2 If it is above true true return it</returns>
        public static bool IsGreaterOrEqual(double val1, double val2)
        {
            return val1 - val2 > -0.0000005;
        }

        /// <summary>
        ///     Determine if the number is positive
        /// </summary>
        /// <param name="val">Numerical value</param>
        /// <returns>If the number is positive true true return it</returns>
        public static bool IsPositive(double val)
        {
            return val > 0.0000005;
        }

        /// <summary>
        ///     Determine if the number is negative
        /// </summary>
        /// <param name="val">Numerical value</param>
        /// <returns>If the number is negative true true return it</returns>
        public static bool IsNegative(double val)
        {
            return val < -0.0000005;
        }

        #endregion

        #region String conversion

        /// <summary>
        ///     Convert to a string
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Converted string</returns>
        public static string ToString(double val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Convert to a string (( No decimal point )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Converted string</returns>
        public static string ToString0(double val)
        {
            // After the decimal point 6 digit
            if (Math.Abs(val - Math.Round(val, 5)) > 0.0000005)
            {
                return val.ToString("F6", CultureInfo.InvariantCulture);
            }
            // After the decimal point Five digit
            if (Math.Abs(val - Math.Round(val, 4)) > 0.000005)
            {
                return val.ToString("F5", CultureInfo.InvariantCulture);
            }
            // After the decimal point Four digit
            if (Math.Abs(val - Math.Round(val, 3)) > 0.00005)
            {
                return val.ToString("F4", CultureInfo.InvariantCulture);
            }
            // After the decimal point 3 digit
            if (Math.Abs(val - Math.Round(val, 2)) > 0.0005)
            {
                return val.ToString("F3", CultureInfo.InvariantCulture);
            }
            // After the decimal point 2 digit
            if (Math.Abs(val - Math.Round(val, 1)) > 0.005)
            {
                return val.ToString("F2", CultureInfo.InvariantCulture);
            }
            // After the decimal point 1 digit
            if (Math.Abs(val - Math.Round(val)) > 0.05)
            {
                return val.ToString("F1", CultureInfo.InvariantCulture);
            }
            // No subdecimal
            return val.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Convert to a string (( After the decimal point 1 digit )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Converted string</returns>
        public static string ToString1(double val)
        {
            // After the decimal point 6 digit
            if (Math.Abs(val - Math.Round(val, 5)) > 0.0000005)
            {
                return val.ToString("F6", CultureInfo.InvariantCulture);
            }
            // After the decimal point Five digit
            if (Math.Abs(val - Math.Round(val, 4)) > 0.000005)
            {
                return val.ToString("F5", CultureInfo.InvariantCulture);
            }
            // After the decimal point Four digit
            if (Math.Abs(val - Math.Round(val, 3)) > 0.00005)
            {
                return val.ToString("F4", CultureInfo.InvariantCulture);
            }
            // After the decimal point 3 digit
            if (Math.Abs(val - Math.Round(val, 2)) > 0.0005)
            {
                return val.ToString("F3", CultureInfo.InvariantCulture);
            }
            // After the decimal point 2 digit
            if (Math.Abs(val - Math.Round(val, 1)) > 0.005)
            {
                return val.ToString("F2", CultureInfo.InvariantCulture);
            }
            // After the decimal point 1 Guarantee digits
            return val.ToString("F1", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Convert to a string (( After the decimal point 2 digit )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Converted string</returns>
        public static string ToString2(double val)
        {
            // After the decimal point 6 digit
            if (Math.Abs(val - Math.Round(val, 5)) > 0.0000005)
            {
                return val.ToString("F6", CultureInfo.InvariantCulture);
            }
            // After the decimal point Five digit
            if (Math.Abs(val - Math.Round(val, 4)) > 0.000005)
            {
                return val.ToString("F5", CultureInfo.InvariantCulture);
            }
            // After the decimal point Four digit
            if (Math.Abs(val - Math.Round(val, 3)) > 0.00005)
            {
                return val.ToString("F4", CultureInfo.InvariantCulture);
            }
            // After the decimal point 3 digit
            if (Math.Abs(val - Math.Round(val, 2)) > 0.0005)
            {
                return val.ToString("F3", CultureInfo.InvariantCulture);
            }
            // After the decimal point 2 Guarantee digits
            return val.ToString("F2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Convert to a string ((After the decimal point 3 digit )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Converted string</returns>
        public static string ToString3(double val)
        {
            // After the decimal point 6 digit
            if (Math.Abs(val - Math.Round(val, 5)) > 0.0000005)
            {
                return val.ToString("F6", CultureInfo.InvariantCulture);
            }
            // After the decimal point Five digit
            if (Math.Abs(val - Math.Round(val, 4)) > 0.000005)
            {
                return val.ToString("F5", CultureInfo.InvariantCulture);
            }
            // After the decimal point Four digit
            if (Math.Abs(val - Math.Round(val, 3)) > 0.00005)
            {
                return val.ToString("F4", CultureInfo.InvariantCulture);
            }
            // After the decimal point 3 Guarantee digits
            return val.ToString("F3", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Convert to a string (( After the decimal point Four digit )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Converted string</returns>
        public static string ToString4(double val)
        {
            // After the decimal point 6 digit
            if (Math.Abs(val - Math.Round(val, 5)) > 0.0000005)
            {
                return val.ToString("F6", CultureInfo.InvariantCulture);
            }
            // After the decimal point Five digit
            if (Math.Abs(val - Math.Round(val, 4)) > 0.000005)
            {
                return val.ToString("F5", CultureInfo.InvariantCulture);
            }
            // After the decimal point Four Guarantee digits
            return val.ToString("F4", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Convert to a string (( Real number / / After the decimal point Five digit )
        /// </summary>
        /// <param name="val">Value to be converted</param>
        /// <returns>Converted string</returns>
        public static string ToString5(double val)
        {
            // After the decimal point 6 digit
            if (Math.Abs(val - Math.Round(val, 5)) > 0.0000005)
            {
                return val.ToString("F6", CultureInfo.InvariantCulture);
            }
            // After the decimal point Five Guarantee digits
            return val.ToString("F5", CultureInfo.InvariantCulture);
        }

        #endregion

        #region Numerical conversion

        /// <summary>
        ///     Convert a string to a number
        /// </summary>
        /// <param name="s">Character string to be converted</param>
        /// <param name="val">Converted value</param>
        /// <returns>If the conversion is successful true true return it</returns>
        public static bool TryParse(string s, out double val)
        {
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out val);
        }

        #endregion
    }
}
