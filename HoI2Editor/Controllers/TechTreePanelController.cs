using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Controllers
{
    /// <summary>
    ///     Technical tree panel controller class
    /// </summary>
    public class TechTreePanelController
    {
        #region Public property

        /// <summary>
        ///     Technical category
        /// </summary>
        public TechCategory Category { get; set; }

        /// <summary>
        ///     Whether the status of the item is reflected in the display of the item label
        /// </summary>
        public bool ApplyItemStatus { get; set; }

        /// <summary>
        ///     Is it possible to allow drug and drop in the item label
        /// </summary>
        public bool AllowDragDrop { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Picture box of technical tree panel
        /// </summary>
        private readonly PictureBox _pictureBox;

        /// <summary>
        ///     Technical label width
        /// </summary>
        private static int _techLabelWidth;

        /// <summary>
        ///     Technical label height
        /// </summary>
        private static int _techLabelHeight;

        /// <summary>
        ///     Event label width
        /// </summary>
        private static int _eventLabelWidth;

        /// <summary>
        ///     Event label height
        /// </summary>
        private static int _eventLabelHeight;

        /// <summary>
        ///     Image of technical label
        /// </summary>
        private static Bitmap _techLabelBitmap;

        /// <summary>
        ///     Completed technology label image
        /// </summary>
        private static Bitmap _doneTechLabelBitmap;

        /// <summary>
        ///     Image of technology label with blue photo
        /// </summary>
        private static Bitmap _blueprintTechLabelBitmap;

        /// <summary>
        ///     Image of completion technology label with blue photo
        /// </summary>
        private static Bitmap _blueprintDoneTechLabelBitmap;

        /// <summary>
        ///     Event label image
        /// </summary>
        private static Bitmap _eventLabelBitmap;

        /// <summary>
        ///     Completed event label image
        /// </summary>
        private static Bitmap _doneEventLabelBitmap;

        /// <summary>
        ///     Technical label mask image
        /// </summary>
        private static Bitmap _techLabelMask;

        /// <summary>
        ///     Event label mask image
        /// </summary>
        private static Bitmap _eventLabelMask;

        /// <summary>
        ///     Drawing area of ​​technical label
        /// </summary>
        private static Region _techLabelRegion;

        /// <summary>
        ///     Event label drawing area
        /// </summary>
        private static Region _eventLabelRegion;

        /// <summary>
        ///     Label image read flag
        /// </summary>
        private static bool _labelInitialized;

        /// <summary>
        ///     Start position of drag and drop
        /// </summary>
        private static Point _dragPoint = Point.Empty;

        /// <summary>
        ///     Cursor during dragging
        /// </summary>
        private static Cursor _dragCursor;

        #endregion

        #region Internal fixed number

        /// <summary>
        ///     Standard value of technical label width
        /// </summary>
        private const int TechLabelWidthBase = 112;

        /// <summary>
        ///     Standard value of the height of the technical label
        /// </summary>
        private const int TechLabelHeightBase = 16;

        /// <summary>
        ///     Event label width standard value
        /// </summary>
        private const int EventLabelWidthBase = 112;

        /// <summary>
        ///     Event label height standard value
        /// </summary>
        private const int EventLabelHeightBase = 24;

        /// <summary>
        ///     Blue photo icon width
        /// </summary>
        private const int BlueprintIconWidth = 16;

        /// <summary>
        ///     Blue photo icon width
        /// </summary>
        private const int BlueprintIconHeight = 16;

        /// <summary>
        ///     X coordinates of blue photo icon
        /// </summary>
        private const int BlueprintIconX = 88;

        /// <summary>
        ///     Y coordinates of blue photo icon
        /// </summary>
        private const int BlueprintIconY = 0;

        /// <summary>
        ///     Technical tree image file name
        /// </summary>
        private static readonly string[] TechTreeFileNames =
        {
            "techtree_infantry.bmp",
            "techtree_armor.bmp",
            "techtree_naval.bmp",
            "techtree_aircraft.bmp",
            "techtree_industry.bmp",
            "techtree_land_doctrine.bmp",
            "techtree_secret_weapons.bmp",
            "techtree_naval_doctrines.bmp",
            "techtree_air_doctrines.bmp"
        };

        #endregion

        #region Public event

        /// <summary>
        ///     Event when clicking item label
        /// </summary>
        public event EventHandler<ItemEventArgs> ItemClick;

        /// <summary>
        ///     Events at the time of clicking on the item label mouse
        /// </summary>
        public event EventHandler<ItemMouseEventArgs> ItemMouseClick;

        /// <summary>
        ///     Events at the time of item label mouse down
        /// </summary>
        public event EventHandler<ItemMouseEventArgs> ItemMouseDown;

        /// <summary>
        ///     Event at the time of the item label drag and drop
        /// </summary>
        public event EventHandler<ItemDragEventArgs> ItemDragDrop;

        /// <summary>
        ///     Items status inquiry event
        /// </summary>
        public event EventHandler<QueryItemStatusEventArgs> QueryItemStatus;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="pictureBox">Technical tree picture box</param>
        public TechTreePanelController(PictureBox pictureBox)
        {
            _pictureBox = pictureBox;

            // Initialize label images
            InitLabelBitmap();

            // Initialize the event handler
            InitEventHandler();

            // Available for drag and drop to the Technical Tree Picture Box
            // Since it does not exist in the design of the design, set it at the time of initialization
            _pictureBox.AllowDrop = true;
        }

        /// <summary>
        ///     Initialize the event handler
        /// </summary>
        private void InitEventHandler()
        {
            _pictureBox.DragOver += OnPictureBoxDragOver;
            _pictureBox.DragDrop += OnPictureBoxDragDrop;
        }

        #endregion

        #region Technical tree operation

        /// <summary>
        ///     Update the technical tree
        /// </summary>
        public void Update()
        {
            // Update the technical tree image
            UpdateTechTreeImage();

            // Update the item label
            UpdateItems();
        }

        /// <summary>
        ///     Clear the technical tree
        /// </summary>
        public void Clear()
        {
            // Clear the technical tree image
            ClearTechTreeImage();

            // Clear the item label
            ClearItems();
        }

        #endregion

        #region Technical tree image

        /// <summary>
        ///     Update the technical tree image
        /// </summary>
        private void UpdateTechTreeImage()
        {
            Bitmap original = new Bitmap(Game.GetReadFileName(Game.PicturePathName, TechTreeFileNames[(int) Category]));
            original.MakeTransparent(Color.Lime);

            int width = DeviceCaps.GetScaledWidth(original.Width);
            int height = DeviceCaps.GetScaledHeight(original.Height);
            Bitmap bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);
            g.DrawImage(original, 0, 0, width, height);
            g.Dispose();
            original.Dispose();

            Image prev = _pictureBox.Image;
            _pictureBox.Image = bitmap;
            prev?.Dispose();
        }

        /// <summary>
        ///     Clear the technical tree image
        /// </summary>
        private void ClearTechTreeImage()
        {
            Image prev = _pictureBox.Image;
            _pictureBox.Image = null;
            prev?.Dispose();
        }

        #endregion

        #region Item label

        /// <summary>
        ///     Update the item label of the technical tree
        /// </summary>
        private void UpdateItems()
        {
            _pictureBox.Controls.Clear();
            foreach (ITechItem item in Techs.Groups[(int) Category].Items)
            {
                AddItem(item);
            }
        }

        /// <summary>
        ///     Clear the item label of the technical tree
        /// </summary>
        private void ClearItems()
        {
            _pictureBox.Controls.Clear();
        }

        /// <summary>
        ///     Add item label group to the technical tree
        /// </summary>
        /// <param name="item">Additional items</param>
        public void AddItem(ITechItem item)
        {
            foreach (TechPosition position in item.Positions)
            {
                AddItem(item, position);
            }
        }

        /// <summary>
        ///     Add an item label to the technical tree
        /// </summary>
        /// <param name="item">Additional items</param>
        /// <param name="position">Additional position</param>
        public void AddItem(ITechItem item, TechPosition position)
        {
            TechItem tech = item as TechItem;
            if (tech != null)
            {
                AddTechItem(tech, position);
                return;
            }

            TechLabel label = item as TechLabel;
            if (label != null)
            {
                AddLabelItem(label, position);
                return;
            }

            TechEvent ev = item as TechEvent;
            if (ev != null)
            {
                AddEventItem(ev, position);
            }
        }

        /// <summary>
        ///     Add technical items to the technical tree
        /// </summary>
        /// <param name="item">Additional items</param>
        /// <param name="position">Additional position</param>
        private void AddTechItem(TechItem item, TechPosition position)
        {
            Label label = new Label
            {
                Location = new Point(DeviceCaps.GetScaledWidth(position.X), DeviceCaps.GetScaledHeight(position.Y)),
                BackColor = Color.Transparent,
                Tag = new TechLabelInfo { Item = item, Position = position },
                Size = new Size(_techLabelBitmap.Width, _techLabelBitmap.Height),
                Region = _techLabelRegion
            };

            // Set the label image
            if (ApplyItemStatus && (QueryItemStatus != null))
            {
                QueryItemStatusEventArgs e = new QueryItemStatusEventArgs(item);
                QueryItemStatus(this, e);
                label.Image = e.Done
                    ? (e.Blueprint ? _blueprintDoneTechLabelBitmap : _doneTechLabelBitmap)
                    : (e.Blueprint ? _blueprintTechLabelBitmap : _techLabelBitmap);
            }
            else
            {
                label.Image = _techLabelBitmap;
            }

            label.Click += OnItemLabelClick;
            label.MouseClick += OnItemLabelMouseClick;
            label.MouseDown += OnItemLabelMouseDown;
            label.MouseUp += OnItemLabelMouseUp;
            label.MouseMove += OnItemLabelMouseMove;
            label.GiveFeedback += OnItemLabelGiveFeedback;
            label.Paint += OnTechItemPaint;

            _pictureBox.Controls.Add(label);
        }

        /// <summary>
        ///     Add a technical label to the technical tree
        /// </summary>
        /// <param name="item">Additional items</param>
        /// <param name="position">Additional position</param>
        private void AddLabelItem(TechLabel item, TechPosition position)
        {
            Label label = new Label
            {
                Location = new Point(DeviceCaps.GetScaledWidth(position.X), DeviceCaps.GetScaledHeight(position.Y)),
                BackColor = Color.Transparent,
                Tag = new TechLabelInfo { Item = item, Position = position }
            };
            label.Size = Graphics.FromHwnd(label.Handle).MeasureString(item.ToString(), label.Font).ToSize();

            label.Click += OnItemLabelClick;
            label.MouseClick += OnItemLabelMouseClick;
            label.MouseDown += OnItemLabelMouseDown;
            label.MouseUp += OnItemLabelMouseUp;
            label.MouseMove += OnItemLabelMouseMove;
            label.GiveFeedback += OnItemLabelGiveFeedback;
            label.Paint += OnTechLabelPaint;

            _pictureBox.Controls.Add(label);
        }

        /// <summary>
        ///     Add an invention event to the technical tree
        /// </summary>
        /// <param name="item">Additional items</param>
        /// <param name="position">Additional position</param>
        private void AddEventItem(TechEvent item, TechPosition position)
        {
            Label label = new Label
            {
                Location = new Point(DeviceCaps.GetScaledWidth(position.X), DeviceCaps.GetScaledHeight(position.Y)),
                BackColor = Color.Transparent,
                Tag = new TechLabelInfo { Item = item, Position = position },
                Size = new Size(_eventLabelBitmap.Width, _eventLabelBitmap.Height),
                Region = _eventLabelRegion
            };

            // Set the label image
            if (ApplyItemStatus && (QueryItemStatus != null))
            {
                QueryItemStatusEventArgs e = new QueryItemStatusEventArgs(item);
                QueryItemStatus(this, e);
                label.Image = e.Done ? _doneEventLabelBitmap : _eventLabelBitmap;
            }
            else
            {
                label.Image = _doneEventLabelBitmap;
            }

            label.Click += OnItemLabelClick;
            label.MouseClick += OnItemLabelMouseClick;
            label.MouseDown += OnItemLabelMouseDown;
            label.MouseUp += OnItemLabelMouseUp;
            label.MouseMove += OnItemLabelMouseMove;
            label.GiveFeedback += OnItemLabelGiveFeedback;

            _pictureBox.Controls.Add(label);
        }

        /// <summary>
        ///     Delete the item group of the technical tree
        /// </summary>
        /// <param name="item">Items to be deleted</param>
        public void RemoveItem(ITechItem item)
        {
            Control.ControlCollection labels = _pictureBox.Controls;
            foreach (Label label in labels)
            {
                TechLabelInfo info = label.Tag as TechLabelInfo;
                if (info == null)
                {
                    continue;
                }

                if (info.Item == item)
                {
                    _pictureBox.Controls.Remove(label);
                }
            }
        }

        /// <summary>
        ///     Delete the technical tree item
        /// </summary>
        /// <param name="item">Items to be deleted</param>
        /// <param name="position">Position to be deleted</param>
        public void RemoveItem(ITechItem item, TechPosition position)
        {
            Control.ControlCollection labels = _pictureBox.Controls;
            foreach (Label label in labels)
            {
                TechLabelInfo info = label.Tag as TechLabelInfo;
                if (info == null)
                {
                    continue;
                }

                if (info.Item == item && info.Position == position)
                {
                    _pictureBox.Controls.Remove(label);
                }
            }
        }

        /// <summary>
        ///     Update the item label of the technical tree
        /// </summary>
        /// <param name="item">Items to be updated</param>
        public void UpdateItem(ITechItem item)
        {
            Control.ControlCollection labels = _pictureBox.Controls;
            foreach (Label label in labels)
            {
                TechLabelInfo info = label.Tag as TechLabelInfo;
                if (info == null)
                {
                    continue;
                }

                if (info.Item != item)
                {
                    continue;
                }

                // In the case of label items, recalculate the size
                if (item is TechLabel)
                {
                    label.Size = Graphics.FromHwnd(label.Handle).MeasureString(item.ToString(), label.Font).ToSize();
                }

                // If the status of the item does not reflect the status of the item label, only redraw
                if (!ApplyItemStatus || (QueryItemStatus == null))
                {
                    label.Refresh();
                    continue;
                }

                if (item is TechItem)
                {
                    // Set the label image
                    QueryItemStatusEventArgs e = new QueryItemStatusEventArgs(item);
                    QueryItemStatus(this, e);
                    label.Image = e.Done
                        ? (e.Blueprint ? _blueprintDoneTechLabelBitmap : _doneTechLabelBitmap)
                        : (e.Blueprint ? _blueprintTechLabelBitmap : _techLabelBitmap);
                }
                else if (item is TechEvent)
                {
                    // Set the label image
                    QueryItemStatusEventArgs e = new QueryItemStatusEventArgs(item);
                    QueryItemStatus(this, e);
                    label.Image = e.Done ? _doneEventLabelBitmap : _eventLabelBitmap;
                }
            }
        }

        /// <summary>
        ///     Update the technical tree item
        /// </summary>
        /// <param name="item">Items to be updated</param>
        /// <param name="position">Coordinates to be updated</param>
        public void UpdateItem(ITechItem item, TechPosition position)
        {
            Control.ControlCollection labels = _pictureBox.Controls;
            foreach (Label label in labels)
            {
                TechLabelInfo info = label.Tag as TechLabelInfo;
                if (info == null)
                {
                    continue;
                }

                if (info.Item != item)
                {
                    continue;
                }

                // Update the position of the label
                if (info.Position == position)
                {
                    label.Location = new Point(DeviceCaps.GetScaledWidth(position.X),
                        DeviceCaps.GetScaledHeight(position.Y));
                }

                // If the status of the item does not reflect the status of the item label, only the coordinate change
                if (!ApplyItemStatus || (QueryItemStatus == null))
                {
                    continue;
                }

                if (item is TechItem)
                {
                    // Set the label image
                    QueryItemStatusEventArgs e = new QueryItemStatusEventArgs(item);
                    QueryItemStatus(this, e);
                    label.Image = e.Done
                        ? (e.Blueprint ? _blueprintDoneTechLabelBitmap : _doneTechLabelBitmap)
                        : (e.Blueprint ? _blueprintTechLabelBitmap : _techLabelBitmap);
                }
                else if (item is TechEvent)
                {
                    // Set the label image
                    QueryItemStatusEventArgs e = new QueryItemStatusEventArgs(item);
                    QueryItemStatus(this, e);
                    label.Image = e.Done ? _doneEventLabelBitmap : _eventLabelBitmap;
                }
            }
        }

        /// <summary>
        ///     Processing when drawing technical items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnTechItemPaint(object sender, PaintEventArgs e)
        {
            Label label = sender as Label;
            TechLabelInfo info = label?.Tag as TechLabelInfo;
            TechItem techItem = info?.Item as TechItem;

            string s = techItem?.GetShortName();
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            Brush brush = new SolidBrush(Color.Black);
            e.Graphics.DrawString(s, label.Font, brush, 6, 2);
            brush.Dispose();
        }

        /// <summary>
        ///     Processing when drawing label items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnTechLabelPaint(object sender, PaintEventArgs e)
        {
            Label label = sender as Label;
            TechLabelInfo info = label?.Tag as TechLabelInfo;
            TechLabel labelItem = info?.Item as TechLabel;

            string s = labelItem?.ToString();
            if (string.IsNullOrEmpty(s))
            {
                return;
            }

            // Interpret the color specified character string
            Brush brush;
            if ((s[0] == '%' || s[0] == 'ｧ' || s[0] == '§') &&
                s.Length > 4 &&
                s[1] >= '0' && s[1] <= '9' &&
                s[2] >= '0' && s[2] <= '9' &&
                s[3] >= '0' && s[3] <= '9')
            {
                brush = new SolidBrush(Color.FromArgb((s[3] - '0') << 5, (s[2] - '0') << 5, (s[1] - '0') << 5));
                s = s.Substring(4);
            }
            else
            {
                brush = new SolidBrush(Color.White);
            }
            e.Graphics.DrawString(s, label.Font, brush, -2, 0);
            brush.Dispose();
        }

        /// <summary>
        ///     Processing when clicking item label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemLabelClick(object sender, EventArgs e)
        {
            // Call the event handler
            if (ItemClick != null)
            {
                Label label = sender as Label;
                TechLabelInfo info = label?.Tag as TechLabelInfo;
                if (info == null)
                {
                    return;
                }

                ItemClick(sender, new ItemEventArgs(info.Item, info.Position));
            }
        }

        /// <summary>
        ///     Item label mouse click processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemLabelMouseClick(object sender, MouseEventArgs e)
        {
            // Call the event handler
            if (ItemMouseClick != null)
            {
                Label label = sender as Label;
                TechLabelInfo info = label?.Tag as TechLabelInfo;
                if (info == null)
                {
                    return;
                }

                ItemMouseClick(sender, new ItemMouseEventArgs(info.Item, info.Position, e));
            }
        }

        /// <summary>
        ///     Item label mouse processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemLabelMouseDown(object sender, MouseEventArgs e)
        {
            Label label = sender as Label;
            TechLabelInfo info = label?.Tag as TechLabelInfo;
            if (info == null)
            {
                return;
            }

            // Preparing for drag and drop
            if (AllowDragDrop)
            {
                // Unless the left button down is down, cancel the drag state
                if (e.Button != MouseButtons.Left)
                {
                    _dragPoint = Point.Empty;
                    Cursor.Current = Cursors.Default;
                    return;
                }

                // Set the drag start position
                _dragPoint = new Point(label.Left + e.X, label.Top + e.Y);
            }

            // Call the event handler
            ItemMouseDown?.Invoke(sender, new ItemMouseEventArgs(info.Item, info.Position, e));
        }

        /// <summary>
        ///     Item label mouse processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemLabelMouseUp(object sender, MouseEventArgs e)
        {
            // If drag and drop is invalid, do nothing
            if (!AllowDragDrop)
            {
                return;
            }

            // Cancel the drag state
            _dragPoint = Point.Empty;
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        ///     Item label mouse processing when moving
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemLabelMouseMove(object sender, MouseEventArgs e)
        {
            // If drag and drop is invalid, do nothing
            if (!AllowDragDrop)
            {
                return;
            }

            // If you are not in dragging, do nothing
            if (_dragPoint == Point.Empty)
            {
                return;
            }

            Label label = sender as Label;
            if (label == null)
            {
                return;
            }

            // Do nothing unless it exceeds the drug judgment size
            Size dragSize = SystemInformation.DragSize;
            Rectangle dragRect = new Rectangle(_dragPoint.X - dragSize.Width / 2, _dragPoint.Y - dragSize.Height / 2,
                dragSize.Width, dragSize.Height);
            if (dragRect.Contains(label.Left + e.X, label.Top + e.Y))
            {
                return;
            }

            TechLabelInfo info = label.Tag as TechLabelInfo;
            if (info == null)
            {
                return;
            }

            // Create a cursor image
            Bitmap bitmap = new Bitmap(label.Width, label.Height);
            bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
            label.DrawToBitmap(bitmap, new Rectangle(0, 0, label.Width, label.Height));
            if (info.Item is TechItem)
            {
                _dragCursor = CursorFactory.CreateCursor(bitmap, _techLabelMask, _dragPoint.X - label.Left,
                    _dragPoint.Y - label.Top);
            }
            else if (info.Item is TechLabel)
            {
                _dragCursor = CursorFactory.CreateCursor(bitmap, _dragPoint.X - label.Left,
                    _dragPoint.Y - label.Top);
            }
            else
            {
                _dragCursor = CursorFactory.CreateCursor(bitmap, _eventLabelMask, _dragPoint.X - label.Left,
                    _dragPoint.Y - label.Top);
            }

            // Start drag and drop
            label.DoDragDrop(sender, DragDropEffects.Move);

            // Cancel the drag state
            _dragPoint = Point.Empty;
            _dragCursor.Dispose();

            bitmap.Dispose();
        }

        /// <summary>
        ///     Items label cursor update process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemLabelGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            // If drag and drop is invalid, do nothing
            if (!AllowDragDrop)
            {
                return;
            }

            if ((e.Effect & DragDropEffects.Move) != 0)
            {
                // Set the cursor image
                e.UseDefaultCursors = false;
                Cursor.Current = _dragCursor;
            }
            else
            {
                e.UseDefaultCursors = true;
            }
        }

        /// <summary>
        ///     Processing when dragging into a technical tree picture box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPictureBoxDragOver(object sender, DragEventArgs e)
        {
            // If drag and drop is invalid, do nothing
            if (!AllowDragDrop)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            // Do nothing unless it's a label
            if (!e.Data.GetDataPresent(typeof (Label)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            Label label = e.Data.GetData(typeof (Label)) as Label;
            if (label == null)
            {
                return;
            }

            // Prohibit drops if it is out of the scope of the technical tree image
            Rectangle dragRect = new Rectangle(0, 0, _pictureBox.Image.Width, _pictureBox.Image.Height);
            Point p = _pictureBox.PointToClient(new Point(e.X, e.Y));
            Rectangle r = new Rectangle(label.Left + p.X - _dragPoint.X, label.Top + p.Y - _dragPoint.Y, label.Width,
                label.Height);
            e.Effect = dragRect.Contains(r) ? DragDropEffects.Move : DragDropEffects.None;
        }

        /// <summary>
        ///     Processing when dropped into a technical tree picture box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPictureBoxDragDrop(object sender, DragEventArgs e)
        {
            // If drag and drop is invalid, do nothing
            if (!AllowDragDrop)
            {
                return;
            }

            // Do nothing unless it's a label
            if (!e.Data.GetDataPresent(typeof (Label)))
            {
                return;
            }

            Label label = e.Data.GetData(typeof (Label)) as Label;
            if (label == null)
            {
                return;
            }

            // Calculate the drop coordinates on the technology tree
            Point p = new Point(e.X, e.Y);
            p = _pictureBox.PointToClient(p);
            p.X = label.Left + p.X - _dragPoint.X;
            p.Y = label.Top + p.Y - _dragPoint.Y;

            // Update the coordinates of label information
            TechLabelInfo info = label.Tag as TechLabelInfo;
            if (info == null)
            {
                return;
            }
            info.Position.X = DeviceCaps.GetUnscaledWidth(p.X);
            info.Position.Y = DeviceCaps.GetUnscaledHeight(p.Y);

            // Update the coordinates of the label
            label.Location = p;

            // Call the event handler
            ItemDragDrop?.Invoke(this, new ItemDragEventArgs(info.Item, info.Position, e));
        }

        /// <summary>
        ///     Initialize label images
        /// </summary>
        private static void InitLabelBitmap()
        {
            // If you have already initialized it will not do anything
            if (_labelInitialized)
            {
                return;
            }

            // Technical label
            Bitmap bitmap = new Bitmap(Game.GetReadFileName(Game.TechLabelPathName));
            _techLabelWidth = DeviceCaps.GetScaledWidth(TechLabelWidthBase);
            _techLabelHeight = DeviceCaps.GetScaledHeight(TechLabelHeightBase);
            _techLabelBitmap = new Bitmap(_techLabelWidth, _techLabelHeight);
            Graphics g = Graphics.FromImage(_techLabelBitmap);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(bitmap, new Rectangle(0, 0, _techLabelWidth, _techLabelHeight),
                new Rectangle(0, 0, TechLabelWidthBase, TechLabelHeightBase), GraphicsUnit.Pixel);
            g.Dispose();
            Color transparent = _techLabelBitmap.GetPixel(0, 0);

            // Technical label with blue photo
            Bitmap icon = new Bitmap(Game.GetReadFileName(Game.BlueprintIconPathName));
            icon.MakeTransparent(icon.GetPixel(0, 0));
            g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(icon, new Rectangle(BlueprintIconX, BlueprintIconY, BlueprintIconWidth, BlueprintIconHeight),
                new Rectangle(0, 0, BlueprintIconWidth, BlueprintIconHeight), GraphicsUnit.Pixel);
            g.Dispose();
            _blueprintTechLabelBitmap = new Bitmap(_techLabelWidth, _techLabelHeight);
            g = Graphics.FromImage(_blueprintTechLabelBitmap);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(bitmap, new Rectangle(0, 0, _techLabelWidth, _techLabelHeight),
                new Rectangle(0, 0, TechLabelWidthBase, TechLabelHeightBase), GraphicsUnit.Pixel);
            g.Dispose();
            bitmap.Dispose();

            // Completion technology label
            bitmap = new Bitmap(Game.GetReadFileName(Game.DoneTechLabelPathName));
            _doneTechLabelBitmap = new Bitmap(_techLabelWidth, _techLabelHeight);
            g = Graphics.FromImage(_doneTechLabelBitmap);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(bitmap, new Rectangle(0, 0, _techLabelWidth, _techLabelHeight),
                new Rectangle(0, 0, TechLabelWidthBase, TechLabelHeightBase), GraphicsUnit.Pixel);
            g.Dispose();

            // Completed technology label with blue photo
            g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(icon, new Rectangle(BlueprintIconX, BlueprintIconY, BlueprintIconWidth, BlueprintIconHeight),
                new Rectangle(0, 0, BlueprintIconWidth, BlueprintIconHeight), GraphicsUnit.Pixel);
            g.Dispose();
            icon.Dispose();
            _blueprintDoneTechLabelBitmap = new Bitmap(_techLabelWidth, _techLabelHeight);
            g = Graphics.FromImage(_blueprintDoneTechLabelBitmap);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(bitmap, new Rectangle(0, 0, _techLabelWidth, _techLabelHeight),
                new Rectangle(0, 0, TechLabelWidthBase, TechLabelHeightBase), GraphicsUnit.Pixel);
            g.Dispose();
            bitmap.Dispose();

            // Technical label area
            _techLabelMask = new Bitmap(_techLabelWidth, _techLabelHeight);
            _techLabelRegion = new Region(new Rectangle(0, 0, _techLabelWidth, _techLabelHeight));
            for (int y = 0; y < _techLabelBitmap.Height; y++)
            {
                for (int x = 0; x < _techLabelBitmap.Width; x++)
                {
                    if (_techLabelBitmap.GetPixel(x, y) == transparent)
                    {
                        _techLabelRegion.Exclude(new Rectangle(x, y, 1, 1));
                        _techLabelMask.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        _techLabelMask.SetPixel(x, y, Color.Black);
                    }
                }
            }

            // Transparent color setting of technical label
            _techLabelBitmap.MakeTransparent(transparent);
            _blueprintTechLabelBitmap.MakeTransparent(transparent);
            _doneTechLabelBitmap.MakeTransparent(transparent);
            _blueprintDoneTechLabelBitmap.MakeTransparent(transparent);

            // Invention event label
            bitmap = new Bitmap(Game.GetReadFileName(Game.SecretLabelPathName));
            _eventLabelWidth = DeviceCaps.GetScaledWidth(EventLabelWidthBase);
            _eventLabelHeight = DeviceCaps.GetScaledHeight(EventLabelHeightBase);
            _eventLabelBitmap = new Bitmap(_eventLabelWidth, _eventLabelHeight);
            g = Graphics.FromImage(_eventLabelBitmap);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(bitmap, new Rectangle(0, 0, _eventLabelWidth, _eventLabelHeight),
                new Rectangle(EventLabelWidthBase, 0, EventLabelWidthBase, EventLabelHeightBase), GraphicsUnit.Pixel);
            g.Dispose();
            transparent = _eventLabelBitmap.GetPixel(0, 0);

            // Completed invention event label
            _doneEventLabelBitmap = new Bitmap(_eventLabelWidth, _eventLabelHeight);
            g = Graphics.FromImage(_doneEventLabelBitmap);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(bitmap, new Rectangle(0, 0, _eventLabelWidth, _eventLabelHeight),
                new Rectangle(0, 0, EventLabelWidthBase, EventLabelHeightBase), GraphicsUnit.Pixel);
            g.Dispose();
            bitmap.Dispose();

            // Invention event label area
            _eventLabelMask = new Bitmap(_eventLabelWidth, _eventLabelHeight);
            _eventLabelRegion = new Region(new Rectangle(0, 0, _eventLabelWidth, _eventLabelHeight));
            for (int y = 0; y < _eventLabelBitmap.Height; y++)
            {
                for (int x = 0; x < _eventLabelBitmap.Width; x++)
                {
                    if (_eventLabelBitmap.GetPixel(x, y) == transparent)
                    {
                        _eventLabelRegion.Exclude(new Rectangle(x, y, 1, 1));
                        _eventLabelMask.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        _eventLabelMask.SetPixel(x, y, Color.Black);
                    }
                }
            }

            // Transparent color setting of invention event label
            _eventLabelBitmap.MakeTransparent(transparent);
            _doneEventLabelBitmap.MakeTransparent(transparent);

            // Set the initialized flag
            _labelInitialized = true;
        }

        #endregion

        #region Internal class

        /// <summary>
        ///     Information associated with the technical label
        /// </summary>
        private class TechLabelInfo
        {
            /// <summary>
            ///     Technical project
            /// </summary>
            public ITechItem Item;

            /// <summary>
            ///     Location Location
            /// </summary>
            public TechPosition Position;
        }

        /// <summary>
        ///     Item label event parameters
        /// </summary>
        public class ItemEventArgs : EventArgs
        {
            /// <summary>
            ///     Technical project
            /// </summary>
            public ITechItem Item { get; private set; }

            /// <summary>
            ///     The position of the item label
            /// </summary>
            public TechPosition Position { get; private set; }

            /// <summary>
            ///     constructor
            /// </summary>
            /// <param name="item">Technical project</param>
            /// <param name="position">The position of the item label</param>
            public ItemEventArgs(ITechItem item, TechPosition position)
            {
                Item = item;
                Position = position;
            }
        }

        /// <summary>
        ///     Item label mouse event parameter
        /// </summary>
        public class ItemMouseEventArgs : MouseEventArgs
        {
            /// <summary>
            ///     Technical project
            /// </summary>
            public ITechItem Item { get; private set; }

            /// <summary>
            ///     The position of the item label
            /// </summary>
            public TechPosition Position { get; private set; }

            /// <summary>
            ///     constructor
            /// </summary>
            /// <param name="item">Technical project</param>
            /// <param name="position">The position of the item label</param>
            /// <param name="e">Mouse event parameter</param>
            public ItemMouseEventArgs(ITechItem item, TechPosition position, MouseEventArgs e)
                : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
            {
                Item = item;
                Position = position;
            }
        }

        /// <summary>
        ///     Item label drag and drop event parameter
        /// </summary>
        public class ItemDragEventArgs : DragEventArgs
        {
            /// <summary>
            ///     Technical project
            /// </summary>
            public ITechItem Item { get; private set; }

            /// <summary>
            ///     The position of the item label
            /// </summary>
            public TechPosition Position { get; private set; }

            /// <summary>
            ///     constructor
            /// </summary>
            /// <param name="item">Technical project</param>
            /// <param name="position">The position of the item label</param>
            /// <param name="e">Drug and drop event parameters</param>
            public ItemDragEventArgs(ITechItem item, TechPosition position, DragEventArgs e)
                : base(e.Data, e.KeyState, e.X, e.Y, e.AllowedEffect, e.Effect)
            {
                Item = item;
                Position = position;
            }
        }

        /// <summary>
        ///     Item status inquiry event parameter
        /// </summary>
        public class QueryItemStatusEventArgs : EventArgs
        {
            /// <summary>
            ///     Technical project
            /// </summary>
            public ITechItem Item { get; private set; }

            /// <summary>
            ///     Whether it was completed
            /// </summary>
            public bool Done;

            /// <summary>
            ///     Whether there is a blue photo
            /// </summary>
            public bool Blueprint;

            /// <summary>
            ///     constructor
            /// </summary>
            /// <param name="item">Technical project</param>
            public QueryItemStatusEventArgs(ITechItem item)
            {
                Item = item;
            }
        }

        #endregion
    }
}
