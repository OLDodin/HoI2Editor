using System;
using System.Runtime.InteropServices;

namespace HoI2Editor.Utilities
{
    /// <summary>
    ///     Device information wrapper class
    /// </summary>
    public static class DeviceCaps
    {
        /// <summary>
        ///     standard XDirectional resolution
        /// </summary>
        private const int DefaultDpiX = 96;

        /// <summary>
        ///     standard Y Directional resolution
        /// </summary>
        private const int DefaultDpiY = 96;

        /// <summary>
        ///     X Directional resolution
        /// </summary>
        private static readonly int DpiX;

        /// <summary>
        ///     Y Directional resolution
        /// </summary>
        private static readonly int DpiY;

        /// <summary>
        ///     Static constructor
        /// </summary>
        static DeviceCaps()
        {
            IntPtr hDc = NativeMethods.GetDC(IntPtr.Zero);
            DpiX = NativeMethods.GetDeviceCaps(hDc, (int) DeviceCapsIndices.LogPixelsX);
            DpiY = NativeMethods.GetDeviceCaps(hDc, (int) DeviceCapsIndices.LogPixelsY);
        }

        /// <summary>
        ///     Get the scaled width
        /// </summary>
        /// <param name="x">Width before scaling</param>
        /// <returns>Width after scaling</returns>
        public static int GetScaledWidth(int x)
        {
            return NativeMethods.MulDiv(x, DpiX, DefaultDpiX);
        }

        /// <summary>
        ///     Get the height after scaling
        /// </summary>
        /// <param name="y">Height before scaling</param>
        /// <returns>Height after scaling</returns>
        public static int GetScaledHeight(int y)
        {
            return NativeMethods.MulDiv(y, DpiY, DefaultDpiY);
        }

        /// <summary>
        ///     Get the width before scaling
        /// </summary>
        /// <param name="x">Width after scaling</param>
        /// <returns>Width before scaling</returns>
        public static int GetUnscaledWidth(int x)
        {
            return NativeMethods.MulDiv(x, DefaultDpiX, DpiX);
        }

        /// <summary>
        ///     Get the height before scaling
        /// </summary>
        /// <param name="y">Height after scaling</param>
        /// <returns>Height before scaling</returns>
        public static int GetUnscaledHeight(int y)
        {
            return NativeMethods.MulDiv(y, DefaultDpiY, DpiY);
        }

        /// <summary>
        ///     GetDeviceCaps Items to be acquired
        /// </summary>
        private enum DeviceCapsIndices
        {
            LogPixelsX = 88,
            LogPixelsY = 90
        }

        /// <summary>
        ///     P / Invoke Method definition class
        /// </summary>
        private static class NativeMethods
        {
            /// <summary>
            ///     GetDeviceCaps Win32API
            /// </summary>
            /// <param name="hdc"></param>
            /// <param name="nIndex"></param>
            /// <returns></returns>
            [DllImport("gdi32.dll")]
            public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

            /// <summary>
            ///     GetDC Win32API
            /// </summary>
            /// <param name="hWnd"></param>
            /// <returns></returns>
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetDC(IntPtr hWnd);

            /// <summary>
            ///     MulDiv Win32API
            /// </summary>
            /// <param name="nNumber"></param>
            /// <param name="nNumerator"></param>
            /// <param name="nDenominator"></param>
            /// <returns></returns>
            [DllImport("kernel32.dll")]
            public static extern int MulDiv(int nNumber, int nNumerator, int nDenominator);
        }
    }
}
