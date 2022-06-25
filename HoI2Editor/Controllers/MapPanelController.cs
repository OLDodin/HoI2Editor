using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Models;

namespace HoI2Editor.Controllers
{
    /// <summary>
    ///     Map panel controller class
    /// </summary>
    public class MapPanelController
    {
        #region Public properties

        /// <summary>
        ///     Map level
        /// </summary>
        public MapLevel Level { get; set; }

        /// <summary>
        ///     Filter mode
        /// </summary>
        public MapFilterMode FilterMode
        {
            get { return _mode; }
            set
            {
                // Update map image by provision
                if (Maps.IsLoaded[(int) Level])
                {
                    List<ushort> prev = GetHighlightedProvinces(_mode, _country);
                    List<ushort> next = GetHighlightedProvinces(value, _country);
                    UpdateProvinces(prev, next);
                }

                // Update filter mode
                _mode = value;
            }
        }

        /// <summary>
        ///     Selected country
        /// </summary>
        public Country SelectedCountry
        {
            get { return _country; }
            set
            {
                // Update map image by provision
                if (Maps.IsLoaded[(int) Level])
                {
                    List<ushort> prev = GetHighlightedProvinces(_mode, _country);
                    List<ushort> next = GetHighlightedProvinces(_mode, value);
                    UpdateProvinces(prev, next);
                }

                // Update selected countries
                _country = value;
            }
        }

        #endregion

        #region Internal field

        /// <summary>
        ///     Map panel
        /// </summary>
        private readonly Panel _panel;

        /// <summary>
        ///     Map panel picture box
        /// </summary>
        private readonly PictureBox _pictureBox;

        /// <summary>
        ///     Filter mode
        /// </summary>
        private MapFilterMode _mode;

        /// <summary>
        ///     Selected country
        /// </summary>
        private Country _country;

        /// <summary>
        ///     Drag and drop start position
        /// </summary>
        private static Point _dragPoint = Point.Empty;

        #endregion

        #region Public constant

        /// <summary>
        ///     Map display filter mode
        /// </summary>
        public enum MapFilterMode
        {
            None, // No filter
            Core, // Core Providence
            Owned, // Owned Providence
            Controlled, // Domination Providence
            Claimed // Province claim
        }

        #endregion

        #region Internal constant

        /// <summary>
        ///     Color index
        /// </summary>
        private enum MapColorIndex
        {
            Land, // Land
            Sea, // Ocean
            Highlighted, // Highlighting
            Invalid // invalid
        }

        #endregion

        #region Public event

        /// <summary>
        ///     Providence Mouse click event
        /// </summary>
        public event EventHandler<ProvinceEventArgs> ProvinceMouseClick;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="panel">Map panel</param>
        /// <param name="pictureBox">Map panel picture box</param>
        public MapPanelController(Panel panel, PictureBox pictureBox)
        {
            _panel = panel;
            _pictureBox = pictureBox;

            Level = MapLevel.Level2;
        }

        #endregion

        #region Map image display

        /// <summary>
        ///     Display map image
        /// </summary>
        public void Show()
        {
            // Initialize the color palette
            InitColorPalette();

            // Ocean / / Generate an invalid provision list
            List<ushort> seaList = new List<ushort>();
            List<ushort> invalidList = new List<ushort>();
            int maxId = Maps.BoundBoxes.Length;
            foreach (Province province in Provinces.Items)
            {
                if ((province.Id <= 0) || (province.Id >= maxId))
                {
                    continue;
                }
                if (province.IsLand)
                {
                    continue;
                }
                if (province.IsSea)
                {
                    seaList.Add((ushort) province.Id);
                }
                else if (province.IsInvalid)
                {
                    invalidList.Add((ushort) province.Id);
                }
            }

            // Change the color index of marine provinces
            Maps.SetColorIndex(seaList, (int) MapColorIndex.Sea);

            // Change the color index of invalid provinces
            Maps.SetColorIndex(invalidList, (int) MapColorIndex.Invalid);

            // Update map image by provision
            Map map = Maps.Data[(int) Level];
            map.UpdateProvinces(seaList);
            map.UpdateProvinces(invalidList);

            // Update the color palette
            map.UpdateColorPalette();

            // Set an image in the picture box
            Image prev = _pictureBox.Image;
            _pictureBox.Image = map.Image;
            prev?.Dispose();

            // Initialize the event handler
            InitEventHandler();
        }

        /// <summary>
        ///     Initialize the color palette
        /// </summary>
        private static void InitColorPalette()
        {
            Maps.SetColorPalette((int) MapColorIndex.Land, "orange");
            Maps.SetColorPalette((int) MapColorIndex.Sea, "water");
            Maps.SetColorPalette((int) MapColorIndex.Highlighted, "green");
            Maps.SetColorPalette((int) MapColorIndex.Invalid, "black");
        }

        /// <summary>
        ///     Scroll to see the specified province
        /// </summary>
        /// <param name="id">Providence ID</param>
        public void ScrollToProvince(int id)
        {
            // Do nothing if unloaded
            if (!Maps.IsLoaded[(int) Level])
            {
                return;
            }

            Rectangle rect = Maps.BoundBoxes[id];
            int provLeft = rect.Left >> 1;
            int provTop = rect.Top >> 1;
            int provWidth = rect.Width >> 1;
            int provHeight = rect.Height >> 1;
            int panelX = _panel.HorizontalScroll.Value - SystemInformation.VerticalScrollBarWidth;
            int panelY = _panel.VerticalScroll.Value - SystemInformation.HorizontalScrollBarHeight;
            int panelWidth = _panel.Width;
            int panelHeight = _panel.Height;

            // Do nothing if the entire specified provision is displayed
            if ((provLeft >= panelX) &&
                (provTop >= panelY) &&
                (provLeft + provWidth <= panelX + panelWidth) &&
                (provTop + provHeight <= panelY + panelHeight))
            {
                return;
            }

            // Scroll so that the specified provision is displayed in the center
            int x = provLeft + provWidth / 2 - panelWidth / 2;
            int mapWidth = _pictureBox.Width;
            if (x < 0)
            {
                x = 0;
            }
            else if (x + panelWidth > mapWidth)
            {
                x = mapWidth - panelWidth;
            }
            int y = provTop + provHeight / 2 - panelHeight / 2;
            int mapHeight = _pictureBox.Height;
            if (y < 0)
            {
                y = 0;
            }
            else if (y + panelHeight > mapHeight)
            {
                y = mapHeight - panelHeight;
            }
            _panel.HorizontalScroll.Value = x;
            _panel.VerticalScroll.Value = y;
        }

        /// <summary>
        ///     Update map image by provision
        /// </summary>
        /// <param name="id">Providence to be updated ID</param>
        /// <param name="highlighted">With or without highlighting</param>
        public void UpdateProvince(ushort id, bool highlighted)
        {
            // Do nothing if unloaded
            if (!Maps.IsLoaded[(int) Level])
            {
                return;
            }

            // Change the color index of the target Providence
            Maps.SetColorIndex(id, (int) (highlighted ? MapColorIndex.Highlighted : MapColorIndex.Land));

            // Update map image by provision
            Map map = Maps.Data[(int) Level];
            map.UpdateProvince(id);

            // Redraw the picture box
            _pictureBox.Refresh();
        }

        /// <summary>
        ///     Update map image by provision
        /// </summary>
        /// <param name="prev">Providence highlighted before the update</param>
        /// <param name="next">Providence to highlight after update</param>
        private void UpdateProvinces(List<ushort> prev, List<ushort> next)
        {
            // Get the provision list that is normally displayed
            List<ushort> normal = prev.Where(id => !next.Contains(id)).ToList();

            // Get the provided list to be highlighted
            List<ushort> highlighted = next.Where(id => !prev.Contains(id)).ToList();

            // Change the color index of the normal display provisions
            Maps.SetColorIndex(normal, (int) MapColorIndex.Land);

            // Change the color index of the highlighting provision
            Maps.SetColorIndex(highlighted, (int) MapColorIndex.Highlighted);

            // Update map image by provision
            Map map = Maps.Data[(int) Level];
            map.UpdateProvinces(normal);
            map.UpdateProvinces(highlighted);

            // Redraw the picture box
            _pictureBox.Refresh();
        }

        /// <summary>
        ///     Get a list of provinces to highlight
        /// </summary>
        /// <param name="mode">Filter mode</param>
        /// <param name="country">Target country</param>
        /// <returns>List of provisions to highlight</returns>
        private static List<ushort> GetHighlightedProvinces(MapFilterMode mode, Country country)
        {
            if (mode == MapFilterMode.None)
            {
                return new List<ushort>();
            }

            CountrySettings settings = Scenarios.GetCountrySettings(country);
            if (settings == null)
            {
                return new List<ushort>();
            }

            switch (mode)
            {
                case MapFilterMode.Core:
                    return Provinces.Items.Where(province => settings.NationalProvinces.Contains(province.Id))
                        .Select(province => (ushort) province.Id)
                        .ToList();

                case MapFilterMode.Owned:
                    return Provinces.Items.Where(province => settings.OwnedProvinces.Contains(province.Id))
                        .Select(province => (ushort) province.Id)
                        .ToList();

                case MapFilterMode.Controlled:
                    return Provinces.Items.Where(province => settings.ControlledProvinces.Contains(province.Id))
                        .Select(province => (ushort) province.Id)
                        .ToList();

                case MapFilterMode.Claimed:
                    return Provinces.Items.Where(province => settings.ClaimedProvinces.Contains(province.Id))
                        .Select(province => (ushort) province.Id)
                        .ToList();
            }

            return null;
        }

        #endregion

        #region Mouse event handler

        /// <summary>
        ///     Initialize the event handler
        /// </summary>
        private void InitEventHandler()
        {
            _pictureBox.MouseClick += OnPictureBoxMouseClick;
            _pictureBox.MouseDown += OnPictureBoxMouseDown;
            _pictureBox.MouseUp += OnPictureBoxMouseUp;
            _pictureBox.MouseMove += OnPictureBoxMouseMove;
            _pictureBox.GiveFeedback += OnPictureBoxGiveFeedback;
            _panel.DragEnter += OnPanelDragEnter;
            _panel.DragDrop += OnPanelDragDrop;
        }

        /// <summary>
        ///     Processing when mouse clicks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPictureBoxMouseClick(object sender, MouseEventArgs e)
        {
            Map map = Maps.Data[(int) Level];
            ushort id = map.ProvinceIds[e.X, e.Y];
            ProvinceMouseClick?.Invoke(sender, new ProvinceEventArgs(id, e));
        }

        /// <summary>
        ///     Processing when right mouse down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPictureBoxMouseDown(object sender, MouseEventArgs e)
        {
            // If the left button is not down, the drag state is canceled.
            if (e.Button != MouseButtons.Right)
            {
                _dragPoint = Point.Empty;
                Cursor.Current = Cursors.Default;
                return;
            }

            // Set the drag start position
            _dragPoint = new Point(e.X - _panel.HorizontalScroll.Value, e.Y - _panel.VerticalScroll.Value);
        }

        /// <summary>
        ///     Processing when mouse up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPictureBoxMouseUp(object sender, MouseEventArgs e)
        {
            // Release the drag state
            _dragPoint = Point.Empty;
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        ///     Processing when moving the mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPictureBoxMouseMove(object sender, MouseEventArgs e)
        {
            // Do nothing unless you are dragging
            if (_dragPoint == Point.Empty)
            {
                return;
            }

            // Do nothing if the drag judgment size is not exceeded
            Size dragSize = SystemInformation.DragSize;
            Rectangle dragRect = new Rectangle(_dragPoint.X - dragSize.Width / 2, _dragPoint.Y - dragSize.Height / 2,
                dragSize.Width, dragSize.Height);
            if (dragRect.Contains(e.X, e.Y))
            {
                return;
            }

            // Start drag and drop
            _pictureBox.DoDragDrop(sender, DragDropEffects.Move);

            // Release the drag state
            _dragPoint = Point.Empty;
        }

        /// <summary>
        ///     Cursor update process while dragging
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPictureBoxGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if ((e.Effect & DragDropEffects.Scroll) != 0)
            {
                e.UseDefaultCursors = false;
                Cursor.Current = Cursors.SizeAll;
            }
            else
            {
                e.UseDefaultCursors = true;
            }
        }

        /// <summary>
        ///     Processing at the start of dragging
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPanelDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof (PictureBox)))
            {
                e.Effect = DragDropEffects.Scroll;
            }
        }

        /// <summary>
        ///     Processing when dropped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPanelDragDrop(object sender, DragEventArgs e)
        {
            // Scroll the map
            Point point = _panel.PointToClient(new Point(e.X, e.Y));
            int panelWidth = _panel.Width - SystemInformation.VerticalScrollBarWidth;
            int panelHeight = _panel.Height - SystemInformation.HorizontalScrollBarHeight;
            int mapWidth = _pictureBox.Width;
            int mapHeight = _pictureBox.Height;
            int x = _panel.HorizontalScroll.Value + _dragPoint.X - point.X;
            if (x < 0)
            {
                x = 0;
            }
            else if (x + panelWidth > mapWidth)
            {
                x = mapWidth - panelWidth;
            }
            int y = _panel.VerticalScroll.Value + _dragPoint.Y - point.Y;
            if (y < 0)
            {
                y = 0;
            }
            else if (y + panelHeight > mapHeight)
            {
                y = mapHeight - panelHeight;
            }
            _panel.HorizontalScroll.Value = x;
            _panel.VerticalScroll.Value = y;
        }

        #endregion

        #region Inner class

        /// <summary>
        ///     Providence event parameters
        /// </summary>
        public class ProvinceEventArgs : MouseEventArgs
        {
            /// <summary>
            ///     Providence ID
            /// </summary>
            public int Id { get; private set; }

            /// <summary>
            ///     constructor
            /// </summary>
            /// <param name="id">Providence ID</param>
            /// <param name="e">Mouse event parameters</param>
            public ProvinceEventArgs(int id, MouseEventArgs e) : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
            {
                Id = id;
            }
        }

        #endregion
    }
}
