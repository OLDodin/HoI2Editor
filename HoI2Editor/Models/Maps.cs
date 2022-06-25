using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HoI2Editor.Utilities;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Map data group
    /// </summary>
    public static class Maps
    {
        #region Public properties

        /// <summary>
        ///     Map data
        /// </summary>
        public static Map[] Data;

        /// <summary>
        ///     Arrangement of Providence boundaries
        /// </summary>
        public static Rectangle[] BoundBoxes { get; private set; }

        /// <summary>
        ///     Color scale table
        /// </summary>
        public static Dictionary<string, Color[]> ColorScales { get; private set; }

        /// <summary>
        ///     Color palette
        /// </summary>
        public static Color[] ColorPalette { get; private set; }

        /// <summary>
        ///     Arrangement of color masks
        /// </summary>
        public static byte[] ColorMasks { get; }

        /// <summary>
        ///     Prohibition of map loading
        /// </summary>
        public static bool ForbidLoad;

        /// <summary>
        ///     Loaded flag
        /// </summary>
        public static bool[] IsLoaded;

        #endregion

        #region Internal field

        /// <summary>
        ///     For lazy loading
        /// </summary>
        private static readonly BackgroundWorker[] Workers =
            new BackgroundWorker[Enum.GetValues(typeof (MapLevel)).Length];

        #endregion

        #region Public constant

        /// <summary>
        ///     Maximum number of provisions
        /// </summary>
        public const int MaxProvinces = 10000;

        #endregion

        #region Internal constant

        /// <summary>
        ///     Maximum number of color indexes
        /// </summary>
        private const int MaxColorIndex = 4;

        /// <summary>
        ///     Number of color scales
        /// </summary>
        private const int ColorScaleCount = 64;

        #endregion

        #region Initialization

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Maps()
        {
            int maxLevel = Enum.GetValues(typeof (MapLevel)).Length;

            Data = new Map[maxLevel];
            ColorMasks = new byte[MaxProvinces];

            IsLoaded = new bool[maxLevel];
            for (int i = 0; i < maxLevel; i++)
            {
                Workers[i] = new BackgroundWorker();
            }

            InitColorPalette();
        }

        #endregion

        #region File reading

        /// <summary>
        ///     Request a reload of the map file
        /// </summary>
        public static void RequestReload()
        {
            foreach (MapLevel level in Enum.GetValues(typeof (MapLevel)))
            {
                IsLoaded[(int) level] = false;
            }

            BoundBoxes = null;
            ColorScales = null;
        }

        /// <summary>
        ///     Load the map file
        /// </summary>
        /// <param name="level">Map level</param>
        public static void Load(MapLevel level)
        {
            // Do nothing if read-protected
            if (ForbidLoad)
            {
                return;
            }

            // Do nothing if already loaded
            if (IsLoaded[(int) level])
            {
                return;
            }

            // Wait for completion if loading is in progress
            if (Workers[(int) level].IsBusy)
            {
                WaitLoading(level);
                return;
            }

            LoadFiles(level);
        }

        /// <summary>
        ///     Lazy loading of map file
        /// </summary>
        /// <param name="level">Map level</param>
        /// <param name="handler">Read complete event handler</param>
        public static void LoadAsync(MapLevel level, RunWorkerCompletedEventHandler handler)
        {
            // Do nothing if read-protected
            if (ForbidLoad)
            {
                return;
            }

            // Call the completion event handler if it has already been read
            if (IsLoaded[(int) level])
            {
                handler?.Invoke(null, new RunWorkerCompletedEventArgs(level, null, false));
                return;
            }

            // Register the read completion event handler
            BackgroundWorker worker = Workers[(int) level];
            if (handler != null)
            {
                worker.RunWorkerCompleted += handler;
                worker.RunWorkerCompleted += OnMapWorkerRunWorkerCompleted;
            }

            // Return if loading is in progress
            if (worker.IsBusy)
            {
                return;
            }

            // If it has already been read here, the completion event handler has already been called, so return without doing anything.
            if (IsLoaded[(int) level])
            {
                return;
            }

            // Start lazy loading
            worker.DoWork += MapWorkerDoWork;
            worker.RunWorkerAsync(level);
        }

        /// <summary>
        ///     Wait for the map to finish loading
        /// </summary>
        /// <param name="level">Map level</param>
        public static void WaitLoading(MapLevel level)
        {
            while (Workers[(int) level].IsBusy)
            {
                Application.DoEvents();
            }
        }

        /// <summary>
        ///     Determine if lazy loading is in progress
        /// </summary>
        /// <returns>If delayed reading is in progress true true return it</returns>
        public static bool IsLoading()
        {
            return Workers[(int) MapLevel.Level1].IsBusy ||
                   Workers[(int) MapLevel.Level2].IsBusy ||
                   Workers[(int) MapLevel.Level3].IsBusy ||
                   Workers[(int) MapLevel.Level4].IsBusy;
        }

        /// <summary>
        ///     Map deferred read processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MapWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            MapLevel level = (MapLevel) e.Argument;
            e.Result = level;

            LoadFiles(level);
        }

        /// <summary>
        ///     Processing when lazy loading is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnMapWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Processing when lazy loading is completed
            HoI2EditorController.OnLoadingCompleted();
        }

        /// <summary>
        ///     Read map files
        /// </summary>
        /// <param name="level">Map level</param>
        private static void LoadFiles(MapLevel level)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Map map = new Map(level);

            // Read map data
            map.Load();
            Data[(int) level] = map;

            // Read the Providence boundary definition file
            if (BoundBoxes == null)
            {
                LoadBoundBox();
            }

            // Read the color scale table
            if (ColorScales == null)
            {
                LoadColorScales();
            }

            sw.Stop();
            Log.Info("[Map] Load: {0} {1}ms", map.Level, sw.ElapsedMilliseconds);

            IsLoaded[(int) level] = true;
        }

        /// <summary>
        ///     Read the Providence boundary definition file
        /// </summary>
        private static void LoadBoundBox()
        {
            // Expand province boundary data to memory
            string fileName = Game.GetReadFileName(Game.GetMapFolderName(), Game.BoundBoxFileName);
            byte[] data;
            int count;
            using (FileStream reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                int len = (int) reader.Length;
                data = new byte[len];
                count = len / 16;
                reader.Read(data, 0, len);
            }
            int index = 0;

            // Read the provision boundaries in order
            BoundBoxes = new Rectangle[count];
            for (int i = 0; i < count; i++)
            {
                int left = data[index] | (data[index + 1] << 8);
                BoundBoxes[i].X = left;
                BoundBoxes[i].Width = (data[index + 8] | (data[index + 9] << 8)) - left + 1;
                int top = data[index + 4] | (data[index + 5] << 8);
                BoundBoxes[i].Y = top;
                BoundBoxes[i].Height = (data[index + 12] | (data[index + 13] << 8)) - top + 1;
                index += 16;
            }
        }

        /// <summary>
        ///     Read the color scale table
        /// </summary>
        private static void LoadColorScales()
        {
            string fileName = Game.GetReadFileName(Game.GetMapFolderName(), Game.ColorScalesFileName);
            using (StreamReader reader = new StreamReader(fileName))
            {
                ColorScales = new Dictionary<string, Color[]>();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    string[] tokens = line.Split(';');
                    if (tokens.Length == 0)
                    {
                        continue;
                    }
                    string name = tokens[0].ToLower().Trim('\"');
                    reader.ReadLine();
                    int[][] colors = new int[4][];
                    line = reader.ReadLine();
                    colors[0] = ParseColor(line);
                    if (colors[0] == null)
                    {
                        continue;
                    }
                    line = reader.ReadLine();
                    colors[1] = ParseColor(line);
                    if (colors[1] == null)
                    {
                        continue;
                    }
                    line = reader.ReadLine();
                    colors[2] = ParseColor(line);
                    if (colors[2] == null)
                    {
                        continue;
                    }
                    line = reader.ReadLine();
                    colors[3] = ParseColor(line);
                    if (colors[3] == null)
                    {
                        continue;
                    }
                    Color[] colorScale = GetColorScale(colors);
                    ColorScales[name] = colorScale;
                }
            }
        }

        /// <summary>
        ///     Analyze the color definition
        /// </summary>
        /// <param name="s">Character string to be analyzed</param>
        /// <returns>Color definition</returns>
        private static int[] ParseColor(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            string[] tokens = s.Split(';');
            if (tokens.Length < 4)
            {
                return null;
            }
            int[] color = new int[4];
            for (int i = 0; i < 4; i++)
            {
                if (!int.TryParse(tokens[i], out color[i]))
                {
                    return null;
                }
                if (color[i] > 255)
                {
                    color[i] = 255;
                }
            }
            return color;
        }

        /// <summary>
        ///     Get the color scale
        /// </summary>
        /// <param name="colors">Array of color definitions</param>
        /// <returns>Color scale</returns>
        private static Color[] GetColorScale(int[][] colors)
        {
            Color[] colorScale = new Color[64];
            int width = colors[1][3] - colors[0][3];
            int deltaR = width > 0 ? ((colors[1][0] - colors[0][0]) << 10) / width : 0;
            int deltaG = width > 0 ? ((colors[1][1] - colors[0][1]) << 10) / width : 0;
            int deltaB = width > 0 ? ((colors[1][2] - colors[0][2]) << 10) / width : 0;
            int r = colors[0][0] << 10;
            int g = colors[0][1] << 10;
            int b = colors[0][2] << 10;
            for (int i = colors[0][3]; i < colors[1][3]; i++)
            {
                colorScale[i] = Color.FromArgb(r >> 10, g >> 10, b >> 10);
                r += deltaR;
                g += deltaG;
                b += deltaB;
            }
            width = colors[2][3] - colors[1][3];
            deltaR = width > 0 ? ((colors[2][0] - colors[1][0]) << 10) / width : 0;
            deltaG = width > 0 ? ((colors[2][1] - colors[1][1]) << 10) / width : 0;
            deltaB = width > 0 ? ((colors[2][2] - colors[1][2]) << 10) / width : 0;
            r = colors[1][0] << 10;
            g = colors[1][1] << 10;
            b = colors[1][2] << 10;
            for (int i = colors[1][3]; i < colors[2][3]; i++)
            {
                colorScale[i] = Color.FromArgb(r >> 10, g >> 10, b >> 10);
                r += deltaR;
                g += deltaG;
                b += deltaB;
            }
            width = colors[3][3] - colors[2][3];
            deltaR = width > 0 ? ((colors[3][0] - colors[2][0]) << 10) / width : 0;
            deltaG = width > 0 ? ((colors[3][1] - colors[2][1]) << 10) / width : 0;
            deltaB = width > 0 ? ((colors[3][2] - colors[2][2]) << 10) / width : 0;
            r = colors[2][0] << 10;
            g = colors[2][1] << 10;
            b = colors[2][2] << 10;
            for (int i = colors[2][3]; i < colors[3][3]; i++)
            {
                colorScale[i] = Color.FromArgb(r >> 10, g >> 10, b >> 10);
                r += deltaR;
                g += deltaG;
                b += deltaB;
            }
            return colorScale;
        }

        #endregion

        #region Color scale

        /// <summary>
        ///     Get the Provins Color Index
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <returns>Color index</returns>
        public static int GetColorIndex(ushort id)
        {
            return ColorMasks[id] >> 6;
        }

        /// <summary>
        ///     Set the color index of the province
        /// </summary>
        /// <param name="ids">Providence ID Array of</param>
        /// <param name="index">Color index</param>
        public static void SetColorIndex(IEnumerable<ushort> ids, int index)
        {
            foreach (ushort id in ids)
            {
                ColorMasks[id] = (byte) (index << 6);
            }
        }

        /// <summary>
        ///     Set the color index of the province
        /// </summary>
        /// <param name="id">Providence ID</param>
        /// <param name="index">Color index</param>
        public static void SetColorIndex(ushort id, int index)
        {
            ColorMasks[id] = (byte) (index << 6);
        }

        /// <summary>
        ///     Set the color scale
        /// </summary>
        /// <param name="index">Color index</param>
        /// <param name="color">Color scale name</param>
        public static void SetColorPalette(int index, string color)
        {
            // Do nothing if the color name does not exist
            if (!ColorScales.ContainsKey(color.ToLower()))
            {
                return;
            }

            Color[] colorScale = ColorScales[color];
            for (int i = 0; i < ColorScaleCount; i++)
            {
                ColorPalette[index * ColorScaleCount + i] = colorScale[i];
            }
        }

        /// <summary>
        ///     Initialize the color palette
        /// </summary>
        private static void InitColorPalette()
        {
            ColorPalette = new Color[ColorScaleCount * MaxColorIndex];
            for (int i = 0; i < MaxColorIndex; i++)
            {
                for (int j = 0; j < ColorScaleCount; j++)
                {
                    ColorPalette[i * ColorScaleCount + j] = Color.FromArgb(255 - j * 4, 255 - j * 4, 255 - j * 4);
                }
            }
        }

        #endregion
    }

    /// <summary>
    ///     Map level
    /// </summary>
    public enum MapLevel
    {
        Level1, // 936x360
        Level2, // 468x180
        Level3, // 234x90
        Level4 // 117x45
    }
}
