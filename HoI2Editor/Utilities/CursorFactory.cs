using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HoI2Editor.Utilities
{
    /// <summary>
    ///     Helper class for creating custom cursors
    /// </summary>
    public static class CursorFactory
    {
        /// <summary>
        ///     Create a cursor
        /// </summary>
        /// <param name="bitmap">Cursor image</param>
        /// <param name="xHotSpot">Of hot spots X Coordinate</param>
        /// <param name="yHotSpot">Of hot spots Y Coordinate</param>
        /// <returns>Created cursor</returns>
        public static Cursor CreateCursor(Bitmap bitmap, int xHotSpot, int yHotSpot)
        {
            Bitmap andMask = new Bitmap(bitmap.Width, bitmap.Height);
            Bitmap xorMask = new Bitmap(bitmap.Width, bitmap.Height);
            Color transparent = bitmap.GetPixel(bitmap.Width - 1, bitmap.Height - 1);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    if (pixel == transparent)
                    {
                        andMask.SetPixel(x, y, Color.White);
                        xorMask.SetPixel(x, y, Color.Transparent);
                    }
                    else
                    {
                        andMask.SetPixel(x, y, Color.Black);
                        xorMask.SetPixel(x, y, pixel);
                    }
                }
            }

            IntPtr hIcon = bitmap.GetHicon();
            IconInfo info = new IconInfo();
            NativeMethods.GetIconInfo(hIcon, ref info);
            info.xHotspot = xHotSpot;
            info.yHotspot = yHotSpot;
            info.hbmMask = andMask.GetHbitmap();
            info.hbmColor = xorMask.GetHbitmap();
            info.fIcon = false;
            hIcon = NativeMethods.CreateIconIndirect(ref info);
            return new Cursor(hIcon);
        }

        /// <summary>
        ///     Create a cursor
        /// </summary>
        /// <param name="bitmap">Cursor image</param>
        /// <param name="andMask">AND AND Mask image</param>
        /// <param name="xHotSpot">Of hot spots X Coordinate</param>
        /// <param name="yHotSpot">Of hot spots Y Coordinate</param>
        /// <returns>Created cursor</returns>
        public static Cursor CreateCursor(Bitmap bitmap, Bitmap andMask, int xHotSpot, int yHotSpot)
        {
            Bitmap xorMask = new Bitmap(bitmap.Width, bitmap.Height);
            Color transparent = andMask.GetPixel(0, 0);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    xorMask.SetPixel(x, y,
                        andMask.GetPixel(x, y) == transparent ? Color.Transparent : bitmap.GetPixel(x, y));
                }
            }

            IntPtr hIcon = bitmap.GetHicon();
            IconInfo info = new IconInfo();
            NativeMethods.GetIconInfo(hIcon, ref info);
            info.xHotspot = xHotSpot;
            info.yHotspot = yHotSpot;
            info.hbmMask = andMask.GetHbitmap();
            info.hbmColor = xorMask.GetHbitmap();
            info.fIcon = false;
            hIcon = NativeMethods.CreateIconIndirect(ref info);
            return new Cursor(hIcon);
        }

        /// <summary>
        ///     P / Invoke Method definition class
        /// </summary>
        private static class NativeMethods
        {
            /// <summary>
            ///     GetIconInfo Win32API
            /// </summary>
            /// <param name="hIcon"></param>
            /// <param name="pIconInfo"></param>
            /// <returns></returns>
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

            /// <summary>
            ///     CreateIconIndirect Win32API
            /// </summary>
            /// <param name="icon"></param>
            /// <returns></returns>
            [DllImport("user32.dll")]
            public static extern IntPtr CreateIconIndirect(ref IconInfo icon);
        }
    }

    /// <summary>
    ///     IconInfo Win32 struct
    /// </summary>
    public struct IconInfo
    {
        #region fIcon

        // ReSharper disable Inconsistent Naming
        public bool fIcon;
        // ReSharper restore Inconsistent Naming

        #endregion

        #region xHotspot

        // ReSharper disable Inconsistent Naming
        public int xHotspot;
        // ReSharper restore Inconsistent Naming

        #endregion

        #region yHotspot

        // ReSharper disable Inconsistent Naming
        public int yHotspot;
        // ReSharper restore Inconsistent Naming

        #endregion

        #region hbmMask

        // ReSharper disable Inconsistent Naming
        public IntPtr hbmMask;
        // ReSharper restore Inconsistent Naming

        #endregion

        #region hbmColor

        // ReSharper disable Inconsistent Naming
        public IntPtr hbmColor;
        // ReSharper restore Inconsistent Naming

        #endregion
    }
}
