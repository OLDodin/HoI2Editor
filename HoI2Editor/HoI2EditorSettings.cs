using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HoI2Editor.Forms;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor
{
    /// <summary>
    ///     Class to manage editor settings
    /// </summary>
    public class HoI2EditorSettings
    {
        #region Public properties

        #region Main form

        /// <summary>
        ///     Main form settings
        /// </summary>
        public MainFormSettings Main = new MainFormSettings();

        #endregion

        #region Commander Editor

        /// <summary>
        ///     Commander editor settings
        /// </summary>
        public LeaderEditorSettings LeaderEditor = new LeaderEditorSettings();

        #endregion

        #region Minister Editor

        /// <summary>
        ///     Minister editor settings
        /// </summary>
        public MinisterEditorSettings MinisterEditor = new MinisterEditorSettings();

        #endregion

        #region Research institution editor

        /// <summary>
        ///     Research institution editor settings
        /// </summary>
        public TeamEditorSettings TeamEditor = new TeamEditorSettings();

        #endregion

        #region Providence Editor

        /// <summary>
        ///     Province editor settings
        /// </summary>
        public ProvinceEditorSettings ProvinceEditor = new ProvinceEditorSettings();

        #endregion

        #region Technology Tree Editor

        /// <summary>
        ///     Technical tree editor settings
        /// </summary>
        public TechEditorSettings TechEditor = new TechEditorSettings();

        #endregion

        #region Unit model editor

        /// <summary>
        ///     Unit model editor settings
        /// </summary>
        public UnitEditorSettings UnitEditor = new UnitEditorSettings();

        #endregion

        #region Basic data editor

        /// <summary>
        ///     Basic data editor settings
        /// </summary>
        public MiscEditorSettings MiscEditor = new MiscEditorSettings();

        #endregion

        #region Army name editor

        /// <summary>
        ///     Army name editor settings
        /// </summary>
        public CorpsNameEditorSettings CorpsNameEditor = new CorpsNameEditorSettings();

        #endregion

        #region Unit name editor

        /// <summary>
        ///     Unit name editor settings
        /// </summary>
        public UnitNameEditorSettings UnitNameEditor = new UnitNameEditorSettings();

        #endregion

        #region Unit model name editor

        /// <summary>
        ///     Unit model name editor settings
        /// </summary>
        public ModelNameEditorSettings ModelNameEditor = new ModelNameEditorSettings();

        #endregion

        #region Random commander name editor

        /// <summary>
        ///     Random commander name editor settings
        /// </summary>
        public RandomLeaderEditorSettings RandomLeaderEditor = new RandomLeaderEditorSettings();

        #endregion

        #region Research speed viewer

        /// <summary>
        ///     Research speed viewer settings
        /// </summary>
        public ResearchViewerSettings ResearchViewer = new ResearchViewerSettings();

        #endregion

        #region Scenario editor

        /// <summary>
        ///     Scenario editor settings
        /// </summary>
        public ScenarioEditorSettings ScenarioEditor = new ScenarioEditorSettings();

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        ///     Round the setting value
        /// </summary>
        public void Round()
        {
            Main.Round();
            LeaderEditor.Round();
            MinisterEditor.Round();
            TeamEditor.Round();
            ProvinceEditor.Round();
            TechEditor.Round();
            UnitEditor.Round();
            MiscEditor.Round();
            CorpsNameEditor.Round();
            UnitNameEditor.Round();
            ModelNameEditor.Round();
            RandomLeaderEditor.Round();
            ResearchViewer.Round();
            ScenarioEditor.Round();
        }

        /// <summary>
        ///     Round the position of the form
        /// </summary>
        /// <param name="location">Current position</param>
        /// <param name="size">Current size</param>
        /// <param name="defaultWidth">Default width</param>
        /// <param name="defaultHeight">Default height</param>
        /// <returns>Position after rolling</returns>
        private static Rectangle RoundFormPosition(Point location, Size size, int defaultWidth, int defaultHeight)
        {
            // Get the size of the desktop
            Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

            // Round the size of the form
            int width = size.Width;
            int scaledWidth = DeviceCaps.GetScaledWidth(defaultWidth);
            if ((width > screenRect.Width) || (width < scaledWidth))
            {
                width = scaledWidth;
            }
            int height = size.Height;
            int scaledHeight = DeviceCaps.GetScaledHeight(defaultHeight);
            if ((height > screenRect.Height) || (height < scaledHeight))
            {
                height = scaledHeight;
            }

            // Round the position of the form
            int x = location.X;
            if (x < screenRect.Left)
            {
                x = screenRect.Left;
            }
            else if (x >= screenRect.Right)
            {
                x = screenRect.Right - 1;
            }
            int y = location.Y;
            if (y < screenRect.Top)
            {
                y = screenRect.Top;
            }
            else if (y >= screenRect.Bottom)
            {
                y = screenRect.Bottom - 1;
            }

            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        ///     Round the position of the form (( If the default height is different between low resolution and high resolution )
        /// </summary>
        /// <param name="location">Current position</param>
        /// <param name="size">Current size</param>
        /// <param name="defaultWidth">Default width</param>
        /// <param name="defaultHeightShort">Default height (( Low resolution )</param>
        /// <param name="defaultHeightLong">Default height (( High resolution )</param>
        /// <returns>Position after rolling</returns>
        private static Rectangle RoundFormPosition(Point location, Size size,
            int defaultWidth, int defaultHeightShort, int defaultHeightLong)
        {
            // Get the size of the desktop
            Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

            // Round the size of the form
            int width = size.Width;
            int scaledWidth = DeviceCaps.GetScaledWidth(defaultWidth);
            if ((width > screenRect.Width) || (width < scaledWidth))
            {
                width = scaledWidth;
            }
            int height = size.Height;
            int scaledHeightShort = DeviceCaps.GetScaledHeight(defaultHeightShort);
            if ((height > screenRect.Height) || (height < scaledHeightShort))
            {
                int scaledHeightLong = DeviceCaps.GetScaledHeight(defaultHeightLong);
                height = screenRect.Height >= scaledHeightLong ? scaledHeightLong : scaledHeightShort;
            }

            // Round the position of the form
            int x = location.X;
            if (x < screenRect.Left)
            {
                x = screenRect.Left;
            }
            else if (x >= screenRect.Right)
            {
                x = screenRect.Right - 1;
            }
            int y = location.Y;
            if (y < screenRect.Top)
            {
                y = screenRect.Top;
            }
            else if (y >= screenRect.Bottom)
            {
                y = screenRect.Bottom - 1;
            }

            return new Rectangle(x, y, width, height);
        }

        #endregion

        #region Main form

        /// <summary>
        ///     Main form setting
        /// </summary>
        public class MainFormSettings
        {
            #region Public properties

            /// <summary>
            ///     Game folder name
            /// </summary>
            public string GameFolder
            {
                get { return Game.FolderName; }
                set { Game.FolderName = value; }
            }

            /// <summary>
            ///     MOD Folder name
            /// </summary>
            public string ModFolder
            {
                get { return Game.ModName; }
                set { Game.ModName = value; }
            }

            /// <summary>
            ///     Save folder name
            /// </summary>
            public string ExportFolder
            {
                get { return Game.ExportName; }
                set { Game.ExportName = value; }
            }

            /// <summary>
            ///     Log output level
            /// </summary>
            public int LogLevel
            {
                get { return Log.Level; }
                set { Log.Level = value; }
            }

            /// <summary>
            ///     Prohibition of map loading
            /// </summary>
            public bool ForbidLoadMaps
            {
                get { return Maps.ForbidLoad; }
                set { Maps.ForbidLoad = value; }
            }

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 480;

            /// <summary>
            ///     Default height
            /// </summary>
            private const int DefaultHeight = 350;

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public MainFormSettings()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int height = DeviceCaps.GetScaledHeight(DefaultHeight);
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeight);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);
            }

            #endregion
        }

        #endregion

        #region Commander Editor

        /// <summary>
        ///     Commander editor settings
        /// </summary>
        public class LeaderEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     Commander list view column width
            /// </summary>
            public int[] ListColumnWidth { get; set; } = new int[LeaderEditorForm.LeaderListColumnCount];

            /// <summary>
            ///     Selected nation
            /// </summary>
            public List<Country> Countries { get; set; } = new List<Country>();

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 1000;

            /// <summary>
            ///     Default height (( Low resolution )
            /// </summary>
            private const int DefaultHeightShort = 670;

            /// <summary>
            ///     Default height (( High resolution )
            /// </summary>
            private const int DefaultHeightLong = 720;

            /// <summary>
            ///     Default width of columns in commander list view
            /// </summary>
            private static readonly int[] DefaultListColumnWidth = { 40, 60, 250, 55, 70, 70, 50, 50, 290 };

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public LeaderEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int longHeight = DeviceCaps.GetScaledHeight(DefaultHeightLong);
                int shortHeight = DeviceCaps.GetScaledHeight(DefaultHeightShort);
                int height = screenRect.Height >= longHeight ? longHeight : shortHeight;
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);

                // Set the width of columns in the commander list view
                for (int i = 0; i < LeaderEditorForm.LeaderListColumnCount; i++)
                {
                    ListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultListColumnWidth[i]);
                }
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeightShort, DefaultHeightLong);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);

                // If no nation is selected, it will be the first AFG To select
                if (Countries.Count == 0)
                {
                    Countries.Add(Country.AFG);
                }
            }

            #endregion
        }

        #endregion

        #region Minister Editor

        /// <summary>
        ///     Minister editor settings
        /// </summary>
        public class MinisterEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     Column width in ministerial list view
            /// </summary>
            public int[] ListColumnWidth { get; set; } = new int[MinisterEditorForm.MinisterListColumnCount];

            /// <summary>
            ///     Selected nation
            /// </summary>
            public List<Country> Countries { get; set; } = new List<Country>();

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 800;

            /// <summary>
            ///     Default height
            /// </summary>
            private const int DefaultHeight = 600;

            /// <summary>
            ///     Default width of columns in ministerial list view
            /// </summary>
            private static readonly int[] DefaultListColumnWidth = { 40, 60, 180, 50, 50, 95, 160, 100 };

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public MinisterEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int height = DeviceCaps.GetScaledHeight(DefaultHeight);
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);

                // Set the width of columns in the ministerial list view
                for (int i = 0; i < MinisterEditorForm.MinisterListColumnCount; i++)
                {
                    ListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultListColumnWidth[i]);
                }
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeight);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);

                // If no nation is selected, it will be the first AFG To select
                if (Countries.Count == 0)
                {
                    Countries.Add(Country.AFG);
                }
            }

            #endregion
        }

        #endregion

        #region Research institution editor

        /// <summary>
        ///     Research institution editor settings
        /// </summary>
        public class TeamEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     Research institution list view column width
            /// </summary>
            public int[] ListColumnWidth { get; set; } = new int[TeamEditorForm.TeamListColumnCount];

            /// <summary>
            ///     Selected nation
            /// </summary>
            public List<Country> Countries { get; set; } = new List<Country>();

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 800;

            /// <summary>
            ///     Default height
            /// </summary>
            private const int DefaultHeight = 600;

            /// <summary>
            ///     Default width of columns in research institution list view
            /// </summary>
            private static readonly int[] DefaultListColumnWidth = { 40, 60, 300, 50, 50, 50, 185 };

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public TeamEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int height = DeviceCaps.GetScaledHeight(DefaultHeight);
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);

                // Set the width of columns in the research institution list view
                for (int i = 0; i < TeamEditorForm.TeamListColumnCount; i++)
                {
                    ListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultListColumnWidth[i]);
                }
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeight);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);

                // If no nation is selected, it will be the first AFG To select
                if (Countries.Count == 0)
                {
                    Countries.Add(Country.AFG);
                }
            }

            #endregion
        }

        #endregion

        #region Providence Editor

        /// <summary>
        ///     Province editor settings
        /// </summary>
        public class ProvinceEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     Provincial list view column width
            /// </summary>
            public int[] ListColumnWidth { get; set; } = new int[ProvinceEditorForm.ProvinceListColumnCount];

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 800;

            /// <summary>
            ///     Default height (( Low resolution )
            /// </summary>
            private const int DefaultHeightShort = 670;

            /// <summary>
            ///     Default height (( High resolution )
            /// </summary>
            private const int DefaultHeightLong = 720;

            /// <summary>
            ///     Default width of columns in province list view
            /// </summary>
            private static readonly int[] DefaultListColumnWidth = { 185, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50 };

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public ProvinceEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int longHeight = DeviceCaps.GetScaledHeight(DefaultHeightLong);
                int shortHeight = DeviceCaps.GetScaledHeight(DefaultHeightShort);
                int height = screenRect.Height >= longHeight ? longHeight : shortHeight;
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);

                // Set the width of columns in the provisions list view
                for (int i = 0; i < ProvinceEditorForm.ProvinceListColumnCount; i++)
                {
                    ListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultListColumnWidth[i]);
                }
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeightShort, DefaultHeightLong);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);
            }

            #endregion
        }

        #endregion

        #region Technology Tree Editor

        /// <summary>
        ///     Technical tree editor settings
        /// </summary>
        public class TechEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     AND AND Required technology list view column width
            /// </summary>
            public int[] AndRequiredListColumnWidth { get; set; } = new int[TechEditorForm.RequiredListColumnCount];

            /// <summary>
            ///     OR Required technology list view column width
            /// </summary>
            public int[] OrRequiredListColumnWidth { get; set; } = new int[TechEditorForm.RequiredListColumnCount];

            /// <summary>
            ///     Short study list view column width
            /// </summary>
            public int[] ComponentListColumnWidth { get; set; } = new int[TechEditorForm.ComponentListColumnCount];

            /// <summary>
            ///     Technique effect list view column width
            /// </summary>
            public int[] EffectListColumnWidth { get; set; } = new int[TechEditorForm.EffectListColumnCount];

            /// <summary>
            ///     Technical coordinate list view column width
            /// </summary>
            public int[] TechPositionListColumnWidth { get; set; } = new int[TechEditorForm.PositionListColumnCount];

            /// <summary>
            ///     Label coordinate list view column width
            /// </summary>
            public int[] LabelPositionListColumnWidth { get; set; } = new int[TechEditorForm.PositionListColumnCount];

            /// <summary>
            ///     Event coordinate list view column width
            /// </summary>
            public int[] EventPositionListColumnWidth { get; set; } = new int[TechEditorForm.PositionListColumnCount];

            /// <summary>
            ///     Choices in the technical category list box
            /// </summary>
            public int Category { get; set; }

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 1000;

            /// <summary>
            ///     Default height (( Low resolution )
            /// </summary>
            private const int DefaultHeightShort = 670;

            /// <summary>
            ///     Default height (( Medium resolution )
            /// </summary>
            private const int DefaultHeightMiddle = 720;

            /// <summary>
            ///     Default height (( High resolution )
            /// </summary>
            private const int DefaultHeightLong = 876;

            /// <summary>
            ///     Required Technology List view column default width
            /// </summary>
            private static readonly int[] DefaultRequiredListColumnWidth = { 60, 235 };

            /// <summary>
            ///     Default width of columns in scholarship list view
            /// </summary>
            private static readonly int[] DefaultComponentListColumnWidth = { 60, 250, 180, 60, 60 };

            /// <summary>
            ///     Default width of columns in technical effect list view
            /// </summary>
            private static readonly int[] DefaultEffectListColumnWidth = { 120, 120, 120, 120, 120 };

            /// <summary>
            ///     Default width of columns in coordinate list view
            /// </summary>
            private static readonly int[] DefaultPositionListColumnWidth = { 50, 50 };

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public TechEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int longHeight = DeviceCaps.GetScaledHeight(DefaultHeightLong);
                int middleHeight = DeviceCaps.GetScaledHeight(DefaultHeightMiddle);
                int shortHeight = DeviceCaps.GetScaledHeight(DefaultHeightShort);
                int height = screenRect.Height >= longHeight
                    ? longHeight
                    : screenRect.Height >= middleHeight ? middleHeight : shortHeight;
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);

                // Required technology Set the width of the list view column
                for (int i = 0; i < TechEditorForm.RequiredListColumnCount; i++)
                {
                    AndRequiredListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultRequiredListColumnWidth[i]);
                    OrRequiredListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultRequiredListColumnWidth[i]);
                }

                // Set the width of columns in the scholarship list view
                for (int i = 0; i < TechEditorForm.ComponentListColumnCount; i++)
                {
                    ComponentListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultComponentListColumnWidth[i]);
                }

                // Set the width of the columns in the technical effect list view
                for (int i = 0; i < TechEditorForm.EffectListColumnCount; i++)
                {
                    EffectListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultEffectListColumnWidth[i]);
                }

                // Set the width of the columns in the coordinate list view
                for (int i = 0; i < TechEditorForm.PositionListColumnCount; i++)
                {
                    TechPositionListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultPositionListColumnWidth[i]);
                    LabelPositionListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultPositionListColumnWidth[i]);
                    EventPositionListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultPositionListColumnWidth[i]);
                }
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeightShort, DefaultHeightLong);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);
            }

            #endregion
        }

        #endregion

        #region Unit model editor

        /// <summary>
        ///     Unit model editor settings
        /// </summary>
        public class UnitEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     Unit model list view column width
            /// </summary>
            public int[] ModelListColumnWidth { get; set; } = new int[UnitEditorForm.ModelListColumnCount];

            /// <summary>
            ///     Improved list view column width
            /// </summary>
            public int[] UpgradeListColumnWidth { get; set; } = new int[UnitEditorForm.UpgradeListColumnCount];

            /// <summary>
            ///     Equipment list view column width
            /// </summary>
            public int[] EquipmentListColumnWidth { get; set; } = new int[UnitEditorForm.EquipmentListColumnCount];

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 1000;

            /// <summary>
            ///     Default height (( Low resolution )
            /// </summary>
            private const int DefaultHeightShort = 670;

            /// <summary>
            ///     Default height (( High resolution )
            /// </summary>
            private const int DefaultHeightLong = 720;

            /// <summary>
            ///     Default width of columns in unit model list view
            /// </summary>
            private static readonly int[] DefaultModelListColumnWidth = { 40, 310, 50, 50, 50, 50, 50, 50, 50, 50 };

            /// <summary>
            ///     Default width of columns in improved list view
            /// </summary>
            private static readonly int[] DefaultUpgradeListColumnWidth = { 225, 40, 40 };

            /// <summary>
            ///     Default width for columns in equipment list view
            /// </summary>
            private static readonly int[] DefaultEquipmentListColumnWidth = { 100, 60 };

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public UnitEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int longHeight = DeviceCaps.GetScaledHeight(DefaultHeightLong);
                int shortHeight = DeviceCaps.GetScaledHeight(DefaultHeightShort);
                int height = screenRect.Height >= longHeight ? longHeight : shortHeight;
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);

                // Set the width of columns in the unit model list view
                for (int i = 0; i < UnitEditorForm.ModelListColumnCount; i++)
                {
                    ModelListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultModelListColumnWidth[i]);
                }

                // Set the width of columns in the improved list view
                for (int i = 0; i < UnitEditorForm.UpgradeListColumnCount; i++)
                {
                    UpgradeListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultUpgradeListColumnWidth[i]);
                }

                // Set the width of the equipment list view column
                for (int i = 0; i < UnitEditorForm.EquipmentListColumnCount; i++)
                {
                    EquipmentListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultEquipmentListColumnWidth[i]);
                }
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeightShort, DefaultHeightLong);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);
            }

            #endregion
        }

        #endregion

        #region Basic data editor

        /// <summary>
        ///     Basic data editor settings
        /// </summary>
        public class MiscEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     Selected tab page
            /// </summary>
            public int SelectedTab { get; set; }

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 1000;

            /// <summary>
            ///     Default height (( Low resolution )
            /// </summary>
            private const int DefaultHeightShort = 670;

            /// <summary>
            ///     Default height (( High resolution )
            /// </summary>
            private const int DefaultHeightLong = 720;

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public MiscEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int longHeight = DeviceCaps.GetScaledHeight(DefaultHeightLong);
                int shortHeight = DeviceCaps.GetScaledHeight(DefaultHeightShort);
                int height = screenRect.Height >= longHeight ? longHeight : shortHeight;
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeightShort, DefaultHeightLong);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);
            }

            #endregion
        }

        #endregion

        #region Army name editor

        /// <summary>
        ///     Army name editor settings
        /// </summary>
        public class CorpsNameEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     Selected military department
            /// </summary>
            public int Branch { get; set; }

            /// <summary>
            ///     Selected nation
            /// </summary>
            public int Country { get; set; }

            /// <summary>
            ///     Whether to apply to all military departments
            /// </summary>
            public bool ApplyAllBranches { get; set; }

            /// <summary>
            ///     Whether to apply to all nations
            /// </summary>
            public bool ApplyAllCountires { get; set; }

            /// <summary>
            ///     Whether to use regular expressions
            /// </summary>
            public bool RegularExpression { get; set; }

            /// <summary>
            ///     Replacement source history
            /// </summary>
            public List<string> ToHistory { get; set; } = new List<string>();

            /// <summary>
            ///     Replacement history
            /// </summary>
            public List<string> WithHistory { get; set; } = new List<string>();

            /// <summary>
            ///     Prefix history
            /// </summary>
            public List<string> PrefixHistory { get; set; } = new List<string>();

            /// <summary>
            ///     History of suffixes
            /// </summary>
            public List<string> SuffixHistory { get; set; } = new List<string>();

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 640;

            /// <summary>
            ///     Default height
            /// </summary>
            private const int DefaultHeight = 480;

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public CorpsNameEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int height = DeviceCaps.GetScaledHeight(DefaultHeight);
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeight);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);
            }

            #endregion
        }

        #endregion

        #region Unit name editor

        /// <summary>
        ///     Unit model name editor settings
        /// </summary>
        public class ModelNameEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     Selected nation
            /// </summary>
            public int Country { get; set; }

            /// <summary>
            ///     Selected unit type
            /// </summary>
            public int UnitType { get; set; }

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 640;

            /// <summary>
            ///     Default height
            /// </summary>
            private const int DefaultHeight = 480;

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public ModelNameEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int height = DeviceCaps.GetScaledHeight(DefaultHeight);
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeight);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);
            }

            #endregion
        }

        #endregion

        #region Unit model name editor

        /// <summary>
        ///     Unit name editor settings
        /// </summary>
        public class UnitNameEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     Selected nation
            /// </summary>
            public int Country { get; set; }

            /// <summary>
            ///     Selected unit type
            /// </summary>
            public int UnitType { get; set; }

            /// <summary>
            ///     Whether to apply to all nations
            /// </summary>
            public bool ApplyAllCountires { get; set; }

            /// <summary>
            ///     Whether to apply to all unit types
            /// </summary>
            public bool ApplyAllUnitTypes { get; set; }

            /// <summary>
            ///     Whether to use regular expressions
            /// </summary>
            public bool RegularExpression { get; set; }

            /// <summary>
            ///     Replacement source history
            /// </summary>
            public List<string> ToHistory { get; set; } = new List<string>();

            /// <summary>
            ///     Replacement history
            /// </summary>
            public List<string> WithHistory { get; set; } = new List<string>();

            /// <summary>
            ///     Prefix history
            /// </summary>
            public List<string> PrefixHistory { get; set; } = new List<string>();

            /// <summary>
            ///     History of suffixes
            /// </summary>
            public List<string> SuffixHistory { get; set; } = new List<string>();

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 640;

            /// <summary>
            ///     Default height
            /// </summary>
            private const int DefaultHeight = 480;

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public UnitNameEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int height = DeviceCaps.GetScaledHeight(DefaultHeight);
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeight);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);
            }

            #endregion
        }

        #endregion

        #region Random commander name editor

        /// <summary>
        ///     Random commander name editor settings
        /// </summary>
        public class RandomLeaderEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     Selected nation
            /// </summary>
            public int Country { get; set; }

            /// <summary>
            ///     Whether to apply to all nations
            /// </summary>
            public bool ApplyAllCountires { get; set; }

            /// <summary>
            ///     Whether to use regular expressions
            /// </summary>
            public bool RegularExpression { get; set; }

            /// <summary>
            ///     Replacement source history
            /// </summary>
            public List<string> ToHistory { get; set; } = new List<string>();

            /// <summary>
            ///     Replacement history
            /// </summary>
            public List<string> WithHistory { get; set; } = new List<string>();

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 640;

            /// <summary>
            ///     Default height
            /// </summary>
            private const int DefaultHeight = 480;

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public RandomLeaderEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int height = DeviceCaps.GetScaledHeight(DefaultHeight);
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeight);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);
            }

            #endregion
        }

        #endregion

        #region Research speed viewer

        /// <summary>
        ///     Research speed viewer settings
        /// </summary>
        public class ResearchViewerSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            /// <summary>
            ///     Technology list view column width
            /// </summary>
            public int[] TechListColumnWidth { get; set; } = new int[LeaderEditorForm.LeaderListColumnCount];

            /// <summary>
            ///     Research institution list view column width
            /// </summary>
            public int[] TeamListColumnWidth { get; set; } = new int[LeaderEditorForm.LeaderListColumnCount];

            /// <summary>
            ///     Choices in the technical category list box
            /// </summary>
            public int Category { get; set; }

            /// <summary>
            ///     Selected nation
            /// </summary>
            public List<Country> Countries
            {
                get { return _countries; }
                set { _countries = value; }
            }

            /// <summary>
            ///     Whether to use the specified date
            /// </summary>
            public bool UseSpecifiedDate
            {
                get { return Researches.DateMode == ResearchDateMode.Specified; }
                set { Researches.DateMode = value ? ResearchDateMode.Specified : ResearchDateMode.Historical; }
            }

            /// <summary>
            ///     Specified date
            /// </summary>
            public GameDate SpecifiedDate
            {
                get { return Researches.SpecifiedDate; }
                set { Researches.SpecifiedDate = value; }
            }

            /// <summary>
            ///     Scale of rocket test site
            /// </summary>
            public int RocketTestingSites
            {
                get { return Researches.RocketTestingSites; }
                set { Researches.RocketTestingSites = value; }
            }

            /// <summary>
            ///     Reactor scale
            /// </summary>
            public int NuclearReactors
            {
                get { return Researches.NuclearReactors; }
                set { Researches.NuclearReactors = value; }
            }

            /// <summary>
            ///     Presence or absence of blueprint
            /// </summary>
            public bool Blueprint
            {
                get { return Researches.Blueprint; }
                set { Researches.Blueprint = value; }
            }

            /// <summary>
            ///     Research speed correction
            /// </summary>
            public string Modifier
            {
                get { return DoubleHelper.ToString(Researches.Modifier); }
                set
                {
                    double d;
                    DoubleHelper.TryParse(value, out d);
                    Researches.Modifier = d;

                    // 0 If the value is below, it will not be possible to calculate properly, so insurance
                    if (Researches.Modifier <= 0)
                    {
                        Researches.Modifier = 1;
                    }
                }
            }

            #endregion

            #region Internal field

            /// <summary>
            ///     Selected nation
            /// </summary>
            private List<Country> _countries = new List<Country>();

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 800;

            /// <summary>
            ///     Default height
            /// </summary>
            private const int DefaultHeight = 600;

            /// <summary>
            ///     Default width of columns in technical list view
            /// </summary>
            private static readonly int[] DefaultTechListColumnWidth = { 310, 50, 50, 200 };

            /// <summary>
            ///     Default width of columns in research institution list view
            /// </summary>
            private static readonly int[] DefaultTeamListColumnWidth = { 0, 40, 50, 85, 200, 50, 45, 120 };

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public ResearchViewerSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int height = DeviceCaps.GetScaledHeight(DefaultHeight);
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);

                // Set the width of the columns in the tech list view
                for (int i = 0; i < ResearchViewerForm.TechListColumnCount; i++)
                {
                    TechListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultTechListColumnWidth[i]);
                }

                // Set the width of columns in the research institution list view
                for (int i = 0; i < ResearchViewerForm.TeamListColumnCount; i++)
                {
                    TeamListColumnWidth[i] = DeviceCaps.GetScaledWidth(DefaultTeamListColumnWidth[i]);
                }
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeight);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);

                // If no nation is selected, it will be the first AFG To select
                if (_countries.Count == 0)
                {
                    _countries.Add(Country.AFG);
                }
            }

            #endregion
        }

        #endregion

        #region Scenario editor

        /// <summary>
        ///     Scenario editor settings
        /// </summary>
        public class ScenarioEditorSettings
        {
            #region Public properties

            /// <summary>
            ///     Window position
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            ///     Window size
            /// </summary>
            public Size Size { get; set; }

            #endregion

            #region Internal constant

            /// <summary>
            ///     Default width
            /// </summary>
            private const int DefaultWidth = 1000;

            /// <summary>
            ///     Default height
            /// </summary>
            private const int DefaultHeight = 670;

            #endregion

            #region Initialization

            /// <summary>
            ///     constructor
            /// </summary>
            public ScenarioEditorSettings()
            {
                Init();
            }

            /// <summary>
            ///     Initialize the setting value
            /// </summary>
            private void Init()
            {
                // Get the size of the desktop
                Rectangle screenRect = Screen.GetWorkingArea(new Point(200, 200));

                // Set the window position
                int width = DeviceCaps.GetScaledWidth(DefaultWidth);
                int height = DeviceCaps.GetScaledHeight(DefaultHeight);
                int x = screenRect.X + (screenRect.Width - width) / 2;
                int y = screenRect.Y + (screenRect.Height - height) / 2;
                Location = new Point(x, y);
                Size = new Size(width, height);
            }

            /// <summary>
            ///     Round the setting value
            /// </summary>
            public void Round()
            {
                // Round the window position
                Rectangle rect = RoundFormPosition(Location, Size, DefaultWidth, DefaultHeight);
                Location = new Point(rect.X, rect.Y);
                Size = new Size(rect.Width, rect.Height);
            }

            #endregion
        }

        #endregion
    }
}
