using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HoI2Editor.Utilities
{
    /// <summary>
    ///     Path operation helper class
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        ///     Maximum size of path name
        /// </summary>
        private const int MaxPath = 260;

        /// <summary>
        ///     Path delimiter
        /// </summary>
        private static readonly char[] PathSeparator = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        /// <summary>
        ///     Get relative path name
        /// </summary>
        /// <param name="pathName">Target path name</param>
        /// <param name="baseDirName">Base directory name</param>
        /// <returns>Relative path name</returns>
        public static string GetRelativePathName(string pathName, string baseDirName)
        {
            string[] targets = pathName.Split(PathSeparator);
            string[] bases = baseDirName.Split(PathSeparator);
            if ((bases.Length == 0) || (targets.Length < bases.Length))
            {
                return "";
            }
            if (bases.Where((t, i) => !targets[i].Equals(t)).Any())
            {
                return "";
            }

            StringBuilder sb = new StringBuilder(MaxPath);
            bool result = NativeMethods.PathRelativePathTo(sb, baseDirName, FileAttributes.Directory, pathName,
                FileAttributes.Normal);
            if (!result)
            {
                return "";
            }
            string s = sb.ToString(2, sb.Length - 2);
            s = s.Replace('\\', '/');
            return s;
        }

        /// <summary>
        ///     P / Invoke Method definition class
        /// </summary>
        private static class NativeMethods
        {
            /// <summary>
            ///     PathRelativePathTo Win32API
            /// </summary>
            /// <param name="pszPath"></param>
            /// <param name="pszFrom"></param>
            /// <param name="dwAttrFrom"></param>
            /// <param name="pszTo"></param>
            /// <param name="dwAttrTo"></param>
            /// <returns></returns>
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern bool PathRelativePathTo(StringBuilder pszPath, string pszFrom,
                FileAttributes dwAttrFrom, string pszTo, FileAttributes dwAttrTo);
        }
    }
}
