using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Models;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Basic data editor form
    /// </summary>
    public partial class MiscEditorForm : Form
    {
        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public MiscEditorForm()
        {
            InitializeComponent();

            // Form initialization
            InitForm();
        }

        #endregion

        #region Data processing

        /// <summary>
        ///     Processing after reading data
        /// </summary>
        public void OnFileLoaded()
        {
            foreach (TabPage page in miscTabControl.TabPages)
            {
                // Update edit items
                UpdateEditableItems(page);
                // Update the color of the edit item
                UpdateItemColor(page);
            }
        }

        /// <summary>
        ///     Processing after data storage
        /// </summary>
        public void OnFileSaved()
        {
            // Update the display as the edited flag is cleared
            foreach (TabPage page in miscTabControl.TabPages)
            {
                UpdateItemColor(page);
            }
        }

        /// <summary>
        ///     Processing after changing edit items
        /// </summary>
        /// <param name="id">Edit items ID</param>
        public void OnItemChanged(EditorItemId id)
        {
            switch (id)
            {
                case EditorItemId.LeaderRetirementYear:
                    Log.Verbose("[Misc] Changed leader retirement year");
                    UpdateLeaderRetirementYear();
                    break;

                case EditorItemId.MinisterEndYear:
                    Log.Verbose("[Misc] Changed minister end year");
                    UpdateMinisterEndYear();
                    break;

                case EditorItemId.MinisterRetirementYear:
                    Log.Verbose("[Misc] Changed minister retirement year");
                    UpdateMinisterRetirementYear();
                    break;

                case EditorItemId.MaxAllowedBrigades:
                    Log.Verbose("[Misc] Changed max allowed brigades");
                    UpdateMaxAllowedBrigades();
                    break;
            }
        }

        /// <summary>
        ///     Update the presence or absence of the commander retirement year
        /// </summary>
        private void UpdateLeaderRetirementYear()
        {
            // DH1.03 Do nothing otherwise
            if ((Game.Type != GameType.DarkestHour) || (Game.Version < 103))
            {
                return;
            }
            foreach (TabPage tabPage in miscTabControl.TabPages)
            {
                foreach (Control control in tabPage.Controls)
                {
                    // Skip untagged labels
                    if (control.Tag == null)
                    {
                        continue;
                    }

                    MiscItemId id = (MiscItemId) control.Tag;
                    if ((MiscItemId) control.Tag == MiscItemId.EnableRetirementYearLeaders)
                    {
                        ComboBox comboBox = control as ComboBox;
                        if (comboBox != null)
                        {
                            comboBox.SelectedIndex = Misc.GetBool(id) ? 1 : 0;
                            comboBox.ForeColor = Misc.IsDirty(id) ? Color.Red : SystemColors.WindowText;
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Update the presence or absence of the ministerial end year
        /// </summary>
        private void UpdateMinisterEndYear()
        {
            // DH Otherwise do nothing
            if (Game.Type != GameType.DarkestHour)
            {
                return;
            }
            foreach (TabPage tabPage in miscTabControl.TabPages)
            {
                foreach (Control control in tabPage.Controls)
                {
                    // Skip untagged labels
                    if (control.Tag == null)
                    {
                        continue;
                    }

                    MiscItemId id = (MiscItemId) control.Tag;
                    if ((MiscItemId) control.Tag == MiscItemId.UseNewMinisterFilesFormat)
                    {
                        ComboBox comboBox = control as ComboBox;
                        if (comboBox != null)
                        {
                            comboBox.SelectedIndex = Misc.GetBool(id) ? 1 : 0;
                            comboBox.ForeColor = Misc.IsDirty(id) ? Color.Red : SystemColors.WindowText;
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Update the presence or absence of a ministerial retirement year
        /// </summary>
        private void UpdateMinisterRetirementYear()
        {
            // DH1.03 Do nothing otherwise
            if ((Game.Type != GameType.DarkestHour) || (Game.Version < 103))
            {
                return;
            }
            foreach (TabPage tabPage in miscTabControl.TabPages)
            {
                foreach (Control control in tabPage.Controls)
                {
                    // Skip untagged labels
                    if (control.Tag == null)
                    {
                        continue;
                    }

                    MiscItemId id = (MiscItemId) control.Tag;
                    if ((MiscItemId) control.Tag == MiscItemId.EnableRetirementYearMinisters)
                    {
                        ComboBox comboBox = control as ComboBox;
                        if (comboBox != null)
                        {
                            comboBox.SelectedIndex = Misc.GetBool(id) ? 1 : 0;
                            comboBox.ForeColor = Misc.IsDirty(id) ? Color.Red : SystemColors.WindowText;
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Update the maximum number of brigades that can be attached
        /// </summary>
        private void UpdateMaxAllowedBrigades()
        {
            // AoD1.07 Do nothing otherwise
            if ((Game.Type != GameType.ArsenalOfDemocracy) || (Game.Version < 107))
            {
                return;
            }
            foreach (TabPage tabPage in miscTabControl.TabPages)
            {
                foreach (Control control in tabPage.Controls)
                {
                    // Skip untagged labels
                    if (control.Tag == null)
                    {
                        continue;
                    }

                    MiscItemId id = (MiscItemId) control.Tag;
                    switch (id)
                    {
                        case MiscItemId.TpMaxAttach: // Maximum number of equipment attached to the transport ship
                        case MiscItemId.SsMaxAttach: // Maximum number of submersible equipment
                        case MiscItemId.SsnMaxAttach: // Maximum number of equipment attached to nuclear submarines
                        case MiscItemId.DdMaxAttach: // Maximum number of equipment attached to the destroyer
                        case MiscItemId.ClMaxAttach: // Maximum number of equipment attached to light cruisers
                        case MiscItemId.CaMaxAttach: // Maximum number of heavy cruisers attached
                        case MiscItemId.BcMaxAttach: // Maximum number of equipment attached to cruise battleships
                        case MiscItemId.BbMaxAttach: // Maximum number of attached equipment for battleships
                        case MiscItemId.CvlMaxAttach: // Maximum number of equipment attached to the light carrier
                        case MiscItemId.CvMaxAttach: // Maximum number of equipment attached to the aircraft carrier
                            TextBox textBox = control as TextBox;
                            if (textBox != null)
                            {
                                textBox.Text = Misc.GetString(id);
                                textBox.ForeColor = Misc.IsDirty(id) ? Color.Red : SystemColors.WindowText;
                            }
                            break;
                    }
                }
            }
        }

        #endregion

        #region Form

        /// <summary>
        ///     Form initialization
        /// </summary>
        private void InitForm()
        {
            // Window position
            Location = HoI2EditorController.Settings.MiscEditor.Location;
            //Size = HoI2Editor.Settings.MiscEditor.Size;

            // Allow tab pages to be displayed wide if the screen resolution is wide enough
            int longHeight = DeviceCaps.GetScaledHeight(720);
            if (Screen.GetWorkingArea(this).Height >= longHeight)
            {
                Height = longHeight;
            }
        }

        /// <summary>
        ///     Processing when loading a form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormLoad(object sender, EventArgs e)
        {
            // Read basic data file
            Misc.Load();

            // Initialize tab pages
            InitTabPages();

            // Processing after reading data
            OnFileLoaded();

            // Initialize the selected tab page
            if (miscTabControl.TabCount > 0)
            {
                int index = HoI2EditorController.Settings.MiscEditor.SelectedTab;
                if ((index < 0) || (index >= miscTabControl.TabCount))
                {
                    index = 0;
                }
                InitTabPage(miscTabControl.TabPages[index]);
                miscTabControl.SelectTab(index);
            }
        }

        /// <summary>
        ///     Processing when closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            // Close form if not edited
            if (!HoI2EditorController.IsDirty())
            {
                return;
            }

            // Ask if you want to save
            DialogResult result = MessageBox.Show(Resources.ConfirmSaveMessage, Text, MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
            switch (result)
            {
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
                case DialogResult.Yes:
                    HoI2EditorController.Save();
                    break;
                case DialogResult.No:
                    HoI2EditorController.SaveCanceled = true;
                    break;
            }
        }

        /// <summary>
        ///     Processing after closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            HoI2EditorController.OnMiscEditorFormClosed();
        }

        /// <summary>
        ///     Processing when moving the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormMove(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                HoI2EditorController.Settings.MiscEditor.Location = Location;
            }
        }

        /// <summary>
        ///     Processing at the time of form resizing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                HoI2EditorController.Settings.MiscEditor.Size = Size;
            }
        }

        /// <summary>
        ///     Processing when the reload button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReloadButtonClick(object sender, EventArgs e)
        {
            // Ask if you want to save it if edited
            if (HoI2EditorController.IsDirty())
            {
                DialogResult result = MessageBox.Show(Resources.ConfirmSaveMessage, Text, MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.Yes:
                        HoI2EditorController.Save();
                        break;
                }
            }

            HoI2EditorController.Reload();
        }

        /// <summary>
        ///     Processing when the save button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.Save();
        }

        /// <summary>
        ///     Processing when the close button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region Tab page processing

        /// <summary>
        ///     Initialize tab pages
        /// </summary>
        private void InitTabPages()
        {
            miscTabControl.TabPages.Clear();

            MiscGameType type = Misc.GetGameType();
            int itemHeight = DeviceCaps.GetScaledHeight(25);
            int itemMargin = DeviceCaps.GetScaledWidth(20);
            int itemsPerColumn = (miscTabControl.ClientSize.Height - miscTabControl.ItemSize.Height - itemMargin) /
                                 itemHeight;
            const int columnsPerPage = 3;

            foreach (MiscSectionId section in Enum.GetValues(typeof (MiscSectionId))
                .Cast<MiscSectionId>()
                .Where(section => Misc.SectionTable[(int) section, (int) type]))
            {
                TabPage tabPage = new TabPage
                {
                    Text = Misc.GetSectionName(section),
                    BackColor = SystemColors.Control
                };
                List<List<MiscItemId>> table = new List<List<MiscItemId>>();
                List<MiscItemId> list = new List<MiscItemId>();
                int row = 0;
                int column = 0;
                int page = 1;
                foreach (MiscItemId id in Misc.SectionItems[(int) section]
                    .Where(id => Misc.ItemTable[(int) id, (int) type]))
                {
                    if (row >= itemsPerColumn)
                    {
                        table.Add(list);

                        list = new List<MiscItemId>();
                        column++;
                        row = 0;

                        if (column >= columnsPerPage)
                        {
                            if (page == 1)
                            {
                                tabPage.Text += IntHelper.ToString(page);
                            }
                            tabPage.Tag = table;
                            miscTabControl.TabPages.Add(tabPage);

                            page++;
                            tabPage = new TabPage
                            {
                                Text = Misc.GetSectionName(section) + IntHelper.ToString(page),
                                BackColor = SystemColors.Control
                            };
                            table = new List<List<MiscItemId>>();
                            column = 0;
                        }
                    }

                    list.Add(id);
                    row++;
                }

                table.Add(list);
                tabPage.Tag = table;
                miscTabControl.TabPages.Add(tabPage);
            }
        }

        /// <summary>
        ///     Initialize the tab page
        /// </summary>
        /// <param name="tabPage">Target tab page</param>
        private void InitTabPage(TabPage tabPage)
        {
            // Create an edit item
            CreateEditableItems(tabPage);

            // Update edit items
            UpdateEditableItems(tabPage);

            // Update the color of the edit item
            UpdateItemColor(tabPage);
        }

        /// <summary>
        ///     Create an edit item
        /// </summary>
        /// <param name="tabPage">Target tab page</param>
        private void CreateEditableItems(TabPage tabPage)
        {
            List<List<MiscItemId>> table = tabPage.Tag as List<List<MiscItemId>>;
            if (table == null)
            {
                return;
            }

            Graphics g = Graphics.FromHwnd(Handle);
            int itemHeight = DeviceCaps.GetScaledHeight(25);
            int labelStartX = DeviceCaps.GetScaledWidth(10);
            int labelStartY = DeviceCaps.GetScaledHeight(13);
            int editStartY = DeviceCaps.GetScaledHeight(10);
            int labelEditMargin = DeviceCaps.GetScaledWidth(8);
            int columnMargin = DeviceCaps.GetScaledWidth(10);
            int textBoxWidth = DeviceCaps.GetScaledWidth(50);
            int textBoxHeight = DeviceCaps.GetScaledHeight(19);
            int comboBoxWidthUnit = DeviceCaps.GetScaledWidth(15);
            int comboBoxWidthBase = DeviceCaps.GetScaledWidth(50);
            int comboBoxWidthMargin = DeviceCaps.GetScaledWidth(8);
            int comboBoxHeight = DeviceCaps.GetScaledHeight(20);

            int labelX = labelStartX;
            foreach (List<MiscItemId> list in table)
            {
                int labelY = labelStartY;
                int editX = labelX; // Right edge of edit control X Coordinate (( Not on the far left )
                foreach (MiscItemId id in list)
                {
                    // Create a label
                    Label label = new Label
                    {
                        Text = Misc.GetItemName(id),
                        AutoSize = true,
                        Location = new Point(labelX, labelY)
                    };
                    string t = Misc.GetItemToolTip(id);
                    if (!string.IsNullOrEmpty(t))
                    {
                        miscToolTip.SetToolTip(label, t);
                    }
                    tabPage.Controls.Add(label);

                    // Find only the width of the edit control
                    int x = labelX + label.Width + labelEditMargin;
                    MiscItemType type = Misc.ItemTypes[(int) id];
                    switch (type)
                    {
                        case MiscItemType.Bool:
                        case MiscItemType.Enum:
                            int maxWidth = comboBoxWidthBase;
                            for (int i = Misc.IntMinValues[id]; i <= Misc.IntMaxValues[id]; i++)
                            {
                                string s = Misc.GetItemChoice(id, i);
                                if (string.IsNullOrEmpty(s))
                                {
                                    continue;
                                }
                                maxWidth = Math.Max(maxWidth,
                                    (int) g.MeasureString(s, Font).Width + SystemInformation.VerticalScrollBarWidth
                                    + comboBoxWidthMargin);
                                maxWidth = comboBoxWidthBase
                                           + (maxWidth - comboBoxWidthBase + (comboBoxWidthUnit - 1))
                                           / comboBoxWidthUnit * comboBoxWidthUnit;
                            }
                            x += maxWidth;
                            break;

                        default:
                            // Items in the text box are fixed
                            x += textBoxWidth;
                            break;
                    }
                    if (x > editX)
                    {
                        editX = x;
                    }
                    labelY += itemHeight;
                }
                int editY = editStartY;
                foreach (MiscItemId id in list)
                {
                    // Create an edit control
                    MiscItemType type = Misc.ItemTypes[(int) id];
                    switch (type)
                    {
                        case MiscItemType.Bool:
                        case MiscItemType.Enum:
                            ComboBox comboBox = new ComboBox
                            {
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                DrawMode = DrawMode.OwnerDrawFixed,
                                Tag = id
                            };
                            // Register the selection items of the combo box and find the maximum width
                            int maxWidth = comboBoxWidthBase;
                            for (int i = Misc.IntMinValues[id]; i <= Misc.IntMaxValues[id]; i++)
                            {
                                string s = Misc.GetItemChoice(id, i);
                                if (string.IsNullOrEmpty(s))
                                {
                                    continue;
                                }
                                comboBox.Items.Add(s);
                                maxWidth = Math.Max(maxWidth,
                                    (int) g.MeasureString(s, Font).Width + SystemInformation.VerticalScrollBarWidth
                                    + comboBoxWidthMargin);
                                maxWidth = comboBoxWidthBase
                                           + (maxWidth - comboBoxWidthBase + (comboBoxWidthUnit - 1))
                                           / comboBoxWidthUnit * comboBoxWidthUnit;
                            }
                            comboBox.Size = new Size(maxWidth, comboBoxHeight);
                            comboBox.Location = new Point(editX - maxWidth, editY);
                            comboBox.DrawItem += OnItemComboBoxDrawItem;
                            comboBox.SelectedIndexChanged += OnItemComboBoxSelectedIndexChanged;
                            tabPage.Controls.Add(comboBox);
                            break;

                        default:
                            TextBox textBox = new TextBox
                            {
                                Size = new Size(textBoxWidth, textBoxHeight),
                                Location = new Point(editX - textBoxWidth, editY),
                                TextAlign = HorizontalAlignment.Right,
                                Tag = id
                            };
                            textBox.Validated += OnItemTextBoxValidated;
                            tabPage.Controls.Add(textBox);
                            break;
                    }
                    editY += itemHeight;
                }
                // Leave a space between the next column
                labelX = editX + columnMargin;
            }
        }

        /// <summary>
        ///     Update edit items
        /// </summary>
        /// <param name="tabPage">Target tab page</param>
        private static void UpdateEditableItems(TabPage tabPage)
        {
            foreach (Control control in tabPage.Controls)
            {
                // Skip untagged labels
                if (control.Tag == null)
                {
                    continue;
                }

                MiscItemId id = (MiscItemId) control.Tag;
                ComboBox comboBox;
                switch (Misc.ItemTypes[(int) id])
                {
                    case MiscItemType.None:
                        break;

                    case MiscItemType.Bool:
                        comboBox = control as ComboBox;
                        if (comboBox != null)
                        {
                            comboBox.SelectedIndex = Misc.GetBool(id) ? 1 : 0;
                        }
                        break;

                    case MiscItemType.Enum:
                        comboBox = control as ComboBox;
                        if (comboBox != null)
                        {
                            int index = Misc.GetInt(id) - Misc.IntMinValues[id];
                            if (index < 0)
                            {
                                index = 0;
                            }
                            if (index >= comboBox.Items.Count)
                            {
                                index = comboBox.Items.Count - 1;
                            }
                            comboBox.SelectedIndex = index;
                        }
                        break;

                    default:
                        TextBox textBox = control as TextBox;
                        if (textBox != null)
                        {
                            textBox.Text = Misc.GetString(id);
                        }
                        break;
                }
            }
        }

        /// <summary>
        ///     Update the color of the edit item
        /// </summary>
        /// <param name="tabPage">Target tab page</param>
        private static void UpdateItemColor(TabPage tabPage)
        {
            foreach (Control control in tabPage.Controls)
            {
                // Skip untagged labels
                if (control.Tag == null)
                {
                    continue;
                }

                MiscItemId id = (MiscItemId) control.Tag;
                switch (Misc.ItemTypes[(int) id])
                {
                    case MiscItemType.None:
                    case MiscItemType.Bool:
                    case MiscItemType.Enum:
                        break;

                    default:
                        TextBox textBox = control as TextBox;
                        if (textBox != null)
                        {
                            textBox.ForeColor = Misc.IsDirty(id) ? Color.Red : SystemColors.WindowText;
                        }
                        break;
                }
            }
        }

        /// <summary>
        ///     Processing when changing tab page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMiscTabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage tabPage = miscTabControl.SelectedTab;
            if (tabPage.Controls.Count == 0)
            {
                InitTabPage(tabPage);
            }

            // Save the selected tab page
            HoI2EditorController.Settings.MiscEditor.SelectedTab = miscTabControl.SelectedIndex;
        }

        #endregion

        #region Edit item operation

        /// <summary>
        ///     Edit item Text box Processing after moving focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemTextBoxValidated(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }
            MiscItemId id = (MiscItemId) textBox.Tag;
            MiscItemType type = Misc.ItemTypes[(int) id];

            double d = 0;
            int i = 0;

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            switch (type)
            {
                case MiscItemType.Int:
                case MiscItemType.PosInt:
                case MiscItemType.NonNegInt:
                case MiscItemType.NonPosInt:
                case MiscItemType.NonNegIntMinusOne:
                case MiscItemType.NonNegInt1:
                case MiscItemType.RangedInt:
                case MiscItemType.RangedPosInt:
                case MiscItemType.RangedIntMinusOne:
                case MiscItemType.RangedIntMinusThree:
                    if (!int.TryParse(textBox.Text, out i))
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.Dbl:
                case MiscItemType.PosDbl:
                case MiscItemType.NonNegDbl:
                case MiscItemType.NonPosDbl:
                case MiscItemType.NonNegDbl0:
                case MiscItemType.NonNegDbl2:
                case MiscItemType.NonNegDbl5:
                case MiscItemType.NonPosDbl0:
                case MiscItemType.NonPosDbl2:
                case MiscItemType.NonNegDblMinusOne:
                case MiscItemType.NonNegDblMinusOne1:
                case MiscItemType.NonNegDbl2AoD:
                case MiscItemType.NonNegDbl4Dda13:
                case MiscItemType.NonNegDbl2Dh103Full:
                case MiscItemType.NonNegDbl2Dh103Full1:
                case MiscItemType.NonNegDbl2Dh103Full2:
                case MiscItemType.NonPosDbl5AoD:
                case MiscItemType.NonPosDbl2Dh103Full:
                case MiscItemType.RangedDbl:
                case MiscItemType.RangedDblMinusOne:
                case MiscItemType.RangedDblMinusOne1:
                case MiscItemType.RangedDbl0:
                case MiscItemType.NonNegIntNegDbl:
                    if (!DoubleHelper.TryParse(textBox.Text, out d))
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.None:
                case MiscItemType.Bool:
                case MiscItemType.Enum:
                    break;
            }

            // If the value is out of the set range, return it.
            switch (type)
            {
                case MiscItemType.PosInt:
                    if (i <= 0)
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.NonNegInt:
                case MiscItemType.NonNegInt1:
                    if (i < 0)
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.NonPosInt:
                    if (i > 0)
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.NonNegIntMinusOne:
                    if ((i < 0) && (i != -1))
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.RangedInt:
                    if ((i < Misc.IntMinValues[id]) || (i > Misc.IntMaxValues[id]))
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.RangedPosInt:
                    if (i < Misc.IntMinValues[id])
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.RangedIntMinusOne:
                    if (((i < Misc.IntMinValues[id]) || (i > Misc.IntMaxValues[id])) && (i != -1))
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.RangedIntMinusThree:
                    if (((i < Misc.IntMinValues[id]) || (i > Misc.IntMaxValues[id])) && (i != -1) && (i != -2) &&
                        (i != -3))
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.PosDbl:
                    if (d <= 0)
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.NonNegDbl:
                case MiscItemType.NonNegDbl0:
                case MiscItemType.NonNegDbl2:
                case MiscItemType.NonNegDbl5:
                case MiscItemType.NonNegDbl2AoD:
                case MiscItemType.NonNegDbl4Dda13:
                case MiscItemType.NonNegDbl2Dh103Full:
                case MiscItemType.NonNegDbl2Dh103Full1:
                case MiscItemType.NonNegDbl2Dh103Full2:
                    if (d < 0)
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.NonPosDbl:
                case MiscItemType.NonPosDbl0:
                case MiscItemType.NonPosDbl2:
                case MiscItemType.NonPosDbl5AoD:
                case MiscItemType.NonPosDbl2Dh103Full:
                    if (d > 0)
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.NonNegDblMinusOne:
                case MiscItemType.NonNegDblMinusOne1:
                    if ((d < 0) && !DoubleHelper.IsEqual(d, -1))
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.RangedDbl:
                case MiscItemType.RangedDbl0:
                    if ((d < Misc.DblMinValues[id]) || (d > Misc.DblMaxValues[id]))
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;

                case MiscItemType.RangedDblMinusOne:
                case MiscItemType.RangedDblMinusOne1:
                    if (((d < Misc.DblMinValues[id]) || (d > Misc.DblMaxValues[id])) && !DoubleHelper.IsEqual(d, -1))
                    {
                        textBox.Text = Misc.GetString(id);
                        return;
                    }
                    break;
            }

            // Do nothing if the value does not change
            switch (type)
            {
                case MiscItemType.Int:
                case MiscItemType.PosInt:
                case MiscItemType.NonNegInt:
                case MiscItemType.NonPosInt:
                case MiscItemType.NonNegIntMinusOne:
                case MiscItemType.NonNegInt1:
                case MiscItemType.RangedInt:
                case MiscItemType.RangedPosInt:
                case MiscItemType.RangedIntMinusOne:
                case MiscItemType.RangedIntMinusThree:
                    if (i == Misc.GetInt(id))
                    {
                        return;
                    }
                    break;

                case MiscItemType.Dbl:
                case MiscItemType.PosDbl:
                case MiscItemType.NonNegDbl:
                case MiscItemType.NonPosDbl:
                case MiscItemType.NonNegDbl0:
                case MiscItemType.NonNegDbl2:
                case MiscItemType.NonNegDbl5:
                case MiscItemType.NonPosDbl0:
                case MiscItemType.NonPosDbl2:
                case MiscItemType.NonNegDblMinusOne:
                case MiscItemType.NonNegDblMinusOne1:
                case MiscItemType.NonNegDbl2AoD:
                case MiscItemType.NonNegDbl4Dda13:
                case MiscItemType.NonNegDbl2Dh103Full:
                case MiscItemType.NonNegDbl2Dh103Full1:
                case MiscItemType.NonNegDbl2Dh103Full2:
                case MiscItemType.NonPosDbl5AoD:
                case MiscItemType.NonPosDbl2Dh103Full:
                case MiscItemType.RangedDbl:
                case MiscItemType.RangedDblMinusOne:
                case MiscItemType.RangedDblMinusOne1:
                case MiscItemType.RangedDbl0:
                case MiscItemType.NonNegIntNegDbl:
                    if (DoubleHelper.IsEqual(d, Misc.GetDouble(id)))
                    {
                        return;
                    }
                    break;
            }

            string old = Misc.GetString(id);

            // Update value
            switch (type)
            {
                case MiscItemType.Int:
                case MiscItemType.PosInt:
                case MiscItemType.NonNegInt:
                case MiscItemType.NonPosInt:
                case MiscItemType.NonNegIntMinusOne:
                case MiscItemType.NonNegInt1:
                case MiscItemType.RangedInt:
                case MiscItemType.RangedPosInt:
                case MiscItemType.RangedIntMinusOne:
                case MiscItemType.RangedIntMinusThree:
                    Misc.SetItem(id, i);
                    break;

                case MiscItemType.Dbl:
                case MiscItemType.PosDbl:
                case MiscItemType.NonNegDbl:
                case MiscItemType.NonPosDbl:
                case MiscItemType.NonNegDbl0:
                case MiscItemType.NonNegDbl2:
                case MiscItemType.NonNegDbl5:
                case MiscItemType.NonPosDbl0:
                case MiscItemType.NonPosDbl2:
                case MiscItemType.NonNegDblMinusOne:
                case MiscItemType.NonNegDblMinusOne1:
                case MiscItemType.NonNegDbl2AoD:
                case MiscItemType.NonNegDbl4Dda13:
                case MiscItemType.NonNegDbl2Dh103Full:
                case MiscItemType.NonNegDbl2Dh103Full1:
                case MiscItemType.NonNegDbl2Dh103Full2:
                case MiscItemType.NonPosDbl5AoD:
                case MiscItemType.NonPosDbl2Dh103Full:
                case MiscItemType.RangedDbl:
                case MiscItemType.RangedDblMinusOne:
                case MiscItemType.RangedDblMinusOne1:
                case MiscItemType.RangedDbl0:
                case MiscItemType.NonNegIntNegDbl:
                    Misc.SetItem(id, d);
                    break;
            }

            Log.Info("[Misc] {0}: {1} -> {2}", Misc.GetItemName(id), old, Misc.GetString(id));

            // Set the edited flag
            Misc.SetDirty(id);

            // Change the font color
            textBox.ForeColor = Color.Red;

            // Notify other forms of updates
            NotifyItemChange(id);
        }

        /// <summary>
        ///     Notify other forms of updates
        /// </summary>
        /// <param name="id">item ID</param>
        private void NotifyItemChange(MiscItemId id)
        {
            switch (id)
            {
                case MiscItemId.TpMaxAttach: // Maximum number of equipment attached to the transport ship
                    if (Units.IsLoaded())
                    {
                        Units.Items[(int) UnitType.Transport].SetDirty(UnitClassItemId.MaxAllowedBrigades);
                    }
                    HoI2EditorController.OnItemChanged(EditorItemId.MaxAllowedBrigades, this);
                    break;

                case MiscItemId.SsMaxAttach: // Maximum number of submersible equipment
                    if (Units.IsLoaded())
                    {
                        Units.Items[(int) UnitType.Submarine].SetDirty(UnitClassItemId.MaxAllowedBrigades);
                    }
                    HoI2EditorController.OnItemChanged(EditorItemId.MaxAllowedBrigades, this);
                    break;

                case MiscItemId.SsnMaxAttach: // Maximum number of equipment attached to nuclear submarines
                    if (Units.IsLoaded())
                    {
                        Units.Items[(int) UnitType.NuclearSubmarine].SetDirty(UnitClassItemId.MaxAllowedBrigades);
                    }
                    HoI2EditorController.OnItemChanged(EditorItemId.MaxAllowedBrigades, this);
                    break;

                case MiscItemId.DdMaxAttach: // Maximum number of equipment attached to the destroyer
                    if (Units.IsLoaded())
                    {
                        Units.Items[(int) UnitType.Destroyer].SetDirty(UnitClassItemId.MaxAllowedBrigades);
                    }
                    HoI2EditorController.OnItemChanged(EditorItemId.MaxAllowedBrigades, this);
                    break;

                case MiscItemId.ClMaxAttach: // Maximum number of equipment attached to light cruisers
                    if (Units.IsLoaded())
                    {
                        Units.Items[(int) UnitType.LightCruiser].SetDirty(UnitClassItemId.MaxAllowedBrigades);
                    }
                    HoI2EditorController.OnItemChanged(EditorItemId.MaxAllowedBrigades, this);
                    break;

                case MiscItemId.CaMaxAttach: // Maximum number of heavy cruisers attached
                    if (Units.IsLoaded())
                    {
                        Units.Items[(int) UnitType.HeavyCruiser].SetDirty(UnitClassItemId.MaxAllowedBrigades);
                    }
                    HoI2EditorController.OnItemChanged(EditorItemId.MaxAllowedBrigades, this);
                    break;

                case MiscItemId.BcMaxAttach: // Maximum number of equipment attached to cruise battleships
                    if (Units.IsLoaded())
                    {
                        Units.Items[(int) UnitType.BattleCruiser].SetDirty(UnitClassItemId.MaxAllowedBrigades);
                    }
                    HoI2EditorController.OnItemChanged(EditorItemId.MaxAllowedBrigades, this);
                    break;

                case MiscItemId.BbMaxAttach: // Maximum number of attached equipment for battleships
                    if (Units.IsLoaded())
                    {
                        Units.Items[(int) UnitType.BattleShip].SetDirty(UnitClassItemId.MaxAllowedBrigades);
                    }
                    HoI2EditorController.OnItemChanged(EditorItemId.MaxAllowedBrigades, this);
                    break;

                case MiscItemId.CvlMaxAttach: // Maximum number of equipment attached to the light carrier
                    if (Units.IsLoaded())
                    {
                        Units.Items[(int) UnitType.EscortCarrier].SetDirty(UnitClassItemId.MaxAllowedBrigades);
                    }
                    HoI2EditorController.OnItemChanged(EditorItemId.MaxAllowedBrigades, this);
                    break;

                case MiscItemId.CvMaxAttach: // Maximum number of equipment attached to the aircraft carrier
                    if (Units.IsLoaded())
                    {
                        Units.Items[(int) UnitType.Carrier].SetDirty(UnitClassItemId.MaxAllowedBrigades);
                    }
                    HoI2EditorController.OnItemChanged(EditorItemId.MaxAllowedBrigades, this);
                    break;
            }
        }

        /// <summary>
        ///     Item drawing process of edit item combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnItemComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null)
            {
                return;
            }
            MiscItemId id = (MiscItemId) comboBox.Tag;
            MiscItemType type = Misc.ItemTypes[(int) id];

            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Brush brush;
            int index = 0;
            switch (type)
            {
                case MiscItemType.Bool:
                    index = Misc.GetBool(id) ? 1 : 0;
                    break;

                case MiscItemType.Enum:
                    index = Misc.GetInt(id);
                    break;
            }
            if ((e.Index + Misc.IntMinValues[id] == index) && Misc.IsDirty(id))
            {
                brush = new SolidBrush(Color.Red);
            }
            else
            {
                brush = new SolidBrush(SystemColors.WindowText);
            }
            string s = comboBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the edit item combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnItemComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null)
            {
                return;
            }
            MiscItemId id = (MiscItemId) comboBox.Tag;
            MiscItemType type = Misc.ItemTypes[(int) id];

            if (comboBox.SelectedIndex == -1)
            {
                return;
            }

            bool b = false;
            int i = 0;

            // Do nothing if the value does not change
            switch (type)
            {
                case MiscItemType.Bool:
                    b = comboBox.SelectedIndex == 1;
                    if (b == Misc.GetBool(id))
                    {
                        return;
                    }
                    break;

                case MiscItemType.Enum:
                    i = comboBox.SelectedIndex + Misc.IntMinValues[id];
                    if (i == Misc.GetInt(id))
                    {
                        return;
                    }
                    break;
            }

            // Update value
            switch (type)
            {
                case MiscItemType.Bool:
                    Log.Info("[Misc] {0}: {1} -> {2}", Misc.GetItemName(id),
                        Misc.GetItemChoice(id, Misc.GetBool(id) ? 1 : 0), Misc.GetItemChoice(id, b ? 1 : 0));
                    Misc.SetItem(id, b);
                    break;

                case MiscItemType.Enum:
                    Log.Info("[Misc] {0}: {1} -> {2}", Misc.GetItemName(id),
                        Misc.GetItemChoice(id, Misc.GetInt(id)), Misc.GetItemChoice(id, i));
                    Misc.SetItem(id, i);
                    break;
            }

            // Set the edited flag
            Misc.SetDirty(id);

            // Update drawing to change item color
            comboBox.Refresh();
        }

        #endregion
    }
}
