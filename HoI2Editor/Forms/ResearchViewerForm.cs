using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Models;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Study Speed Viewer Form
    /// </summary>
    public partial class ResearchViewerForm : Form
    {
        #region Internal field

        /// <summary>
        ///     List of research institutes after narrowing down
        /// </summary>
        private readonly List<Team> _teamList = new List<Team>();

        /// <summary>
        ///     Technical list after narrowing down
        /// </summary>
        private readonly List<TechItem> _techList = new List<TechItem>();

        /// <summary>
        ///     Research characteristic overlay icon
        /// </summary>
        private Bitmap _techOverlayIcon;

        #endregion

        #region Public constant

        /// <summary>
        ///     Number of columns in the tech list view
        /// </summary>
        public const int TechListColumnCount = 4;

        /// <summary>
        ///     Number of columns in the research institution list view
        /// </summary>
        public const int TeamListColumnCount = 8;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public ResearchViewerForm()
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
            // Narrow down the list of research institutes
            NarrowTeamList();

            // Initialize the category list box
            InitCategoryList();

            // Update the display of the technology list
            UpdateTechList();

            // Update the display as the edited flag is cleared
            countryListBox.Refresh();
        }

        /// <summary>
        ///     Processing after changing edit items
        /// </summary>
        /// <param name="id">Edit items ID</param>
        public void OnItemChanged(EditorItemId id)
        {
            switch (id)
            {
                case EditorItemId.TeamList:
                    Log.Verbose("[Research] Changed team list");
                    // Narrow down the list of research institutes
                    NarrowTeamList();
                    // Update the research institution list
                    UpdateTeamList();
                    break;

                case EditorItemId.TeamCountry:
                    Log.Verbose("[Research] Changed team country");
                    // Narrow down the list of research institutes
                    NarrowTeamList();
                    // Update the research institution list
                    UpdateTeamList();
                    break;

                case EditorItemId.TeamName:
                    Log.Verbose("[Research] Changed team name");
                    // Update the research institution list
                    UpdateTeamList();
                    break;

                case EditorItemId.TeamId:
                    Log.Verbose("[Research] Changed team id");
                    // Update the research institution list
                    UpdateTeamList();
                    break;

                case EditorItemId.TeamSkill:
                    Log.Verbose("[Research] Changed team skill");
                    // Update the research institution list
                    UpdateTeamList();
                    break;

                case EditorItemId.TeamSpeciality:
                    Log.Verbose("[Research] Changed team speciality");
                    // Update the research institution list
                    UpdateTeamList();
                    break;

                case EditorItemId.TechItemList:
                    Log.Verbose("[Research] Changed tech item list");
                    // Update the tech list
                    UpdateTechList();
                    break;

                case EditorItemId.TechItemName:
                    Log.Verbose("[Research] Changed tech item name");
                    // Update the tech list
                    UpdateTechList();
                    break;

                case EditorItemId.TechItemId:
                    Log.Verbose("[Research] Changed tech item id");
                    // Update the tech list
                    UpdateTechList();
                    break;

                case EditorItemId.TechItemYear:
                    Log.Verbose("[Research] Changed tech item year");
                    // Update the tech list
                    UpdateTechList();
                    break;

                case EditorItemId.TechComponentList:
                    Log.Verbose("[Research] Changed tech component list");
                    // Update the tech list
                    UpdateTechList();
                    break;

                case EditorItemId.TechComponentSpeciality:
                    Log.Verbose("[Research] Changed tech component speciality");
                    // Update the tech list
                    UpdateTechList();
                    break;

                case EditorItemId.TechComponentDifficulty:
                    Log.Verbose("[Research] Changed tech component difficulty");
                    // Update the tech list
                    UpdateTechList();
                    break;

                case EditorItemId.TechComponentDoubleTime:
                    Log.Verbose("[Research] Changed tech component double time");
                    // Update the tech list
                    UpdateTechList();
                    break;
            }
        }

        #endregion

        #region Form

        /// <summary>
        ///     Form initialization
        /// </summary>
        private void InitForm()
        {
            // Technique list view
            techNameColumnHeader.Width = HoI2EditorController.Settings.ResearchViewer.TechListColumnWidth[0];
            techIdColumnHeader.Width = HoI2EditorController.Settings.ResearchViewer.TechListColumnWidth[1];
            techYearColumnHeader.Width = HoI2EditorController.Settings.ResearchViewer.TechListColumnWidth[2];
            techComponentsColumnHeader.Width = HoI2EditorController.Settings.ResearchViewer.TechListColumnWidth[3];

            // National list box
            countryListBox.ColumnWidth = DeviceCaps.GetScaledWidth(countryListBox.ColumnWidth);
            countryListBox.ItemHeight = DeviceCaps.GetScaledHeight(countryListBox.ItemHeight);

            // Research institution list view
            teamRankColumnHeader.Width = HoI2EditorController.Settings.ResearchViewer.TeamListColumnWidth[1];
            teamDaysColumnHeader.Width = HoI2EditorController.Settings.ResearchViewer.TeamListColumnWidth[2];
            teamEndDateColumnHeader.Width = HoI2EditorController.Settings.ResearchViewer.TeamListColumnWidth[3];
            teamNameColumnHeader.Width = HoI2EditorController.Settings.ResearchViewer.TeamListColumnWidth[4];
            teamIdColumnHeader.Width = HoI2EditorController.Settings.ResearchViewer.TeamListColumnWidth[5];
            teamSkillColumnHeader.Width = HoI2EditorController.Settings.ResearchViewer.TeamListColumnWidth[6];
            teamSpecialityColumnHeader.Width = HoI2EditorController.Settings.ResearchViewer.TeamListColumnWidth[7];

            // Window position
            Location = HoI2EditorController.Settings.ResearchViewer.Location;
            Size = HoI2EditorController.Settings.ResearchViewer.Size;
        }

        /// <summary>
        ///     Processing when loading a form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormLoad(object sender, EventArgs e)
        {
            // Initialize national data
            Countries.Init();

            // Initialize research characteristics
            Techs.InitSpecialities();

            // Load the game settings file
            Misc.Load();

            // Read the character string definition file
            Config.Load();

            // Create a dummy image list to set the height of the tech list view
            techListView.SmallImageList = new ImageList { ImageSize = new Size(1, DeviceCaps.GetScaledHeight(18)) };

            // Create a dummy image list to set the height of the research institution list view
            teamListView.SmallImageList = new ImageList { ImageSize = new Size(1, DeviceCaps.GetScaledHeight(18)) };

            // Initialize the research characteristic overlay icon
            _techOverlayIcon = new Bitmap(Game.GetReadFileName(Game.TechIconOverlayPathName));
            _techOverlayIcon.MakeTransparent(Color.Lime);

            // Initialize option items
            InitOptionItems();

            // Read the technical definition file
            Techs.Load();

            // Read research institution files
            Teams.Load();

            // Initialize the national list box
            InitCountryListBox();

            // Processing after reading data
            OnFileLoaded();
        }

        /// <summary>
        ///     Processing after closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            HoI2EditorController.OnResearchViewerFormClosed();
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
                HoI2EditorController.Settings.ResearchViewer.Location = Location;
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
                HoI2EditorController.Settings.ResearchViewer.Size = Size;
            }
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

        #region Technology category list

        /// <summary>
        ///     Initialize the category list box
        /// </summary>
        private void InitCategoryList()
        {
            categoryListBox.Items.Clear();
            foreach (TechGroup grp in Techs.Groups)
            {
                categoryListBox.Items.Add(grp);
            }

            // Reflect the selected category
            int index = HoI2EditorController.Settings.ResearchViewer.Category;
            if ((index < 0) || (index >= categoryListBox.Items.Count))
            {
                index = 0;
            }
            categoryListBox.SelectedIndex = index;
        }

        /// <summary>
        ///     Processing when changing the selected item in the category list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCategoryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the tech list
            UpdateTechList();

            // Save the selected category
            HoI2EditorController.Settings.ResearchViewer.Category = categoryListBox.SelectedIndex;
        }

        #endregion

        #region Technique list view

        /// <summary>
        ///     Update the display of the technology list
        /// </summary>
        private void UpdateTechList()
        {
            // Narrow down the technology list
            _techList.Clear();
            _techList.AddRange(categoryListBox.SelectedIndices.Cast<int>()
                .SelectMany(index => Techs.Groups[index].Items.Where(item => item is TechItem).Cast<TechItem>()));

            techListView.BeginUpdate();
            techListView.Items.Clear();

            // Register items in order
            foreach (TechItem tech in _techList)
            {
                techListView.Items.Add(CreateTechListViewItem(tech));
            }

            if (techListView.Items.Count > 0)
            {
                // Select the first item
                techListView.Items[0].Focused = true;
                techListView.Items[0].Selected = true;
            }

            techListView.EndUpdate();
        }

        /// <summary>
        ///     Create an item in the technical list view
        /// </summary>
        /// <param name="tech">Technical data</param>
        /// <returns>Items in the technical list view</returns>
        private static ListViewItem CreateTechListViewItem(TechItem tech)
        {
            if (tech == null)
            {
                return null;
            }

            ListViewItem item = new ListViewItem
            {
                Text = Config.GetText(tech.Name),
                Tag = tech
            };
            item.SubItems.Add(IntHelper.ToString(tech.Id));
            item.SubItems.Add(IntHelper.ToString(tech.Year));
            item.SubItems.Add("");

            return item;
        }

        /// <summary>
        ///     Technical list view sub-item drawing process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechListViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 3: // Small study
                    e.Graphics.FillRectangle(
                        techListView.SelectedIndices.Count > 0 && e.ItemIndex == techListView.SelectedIndices[0]
                            ? (techListView.Focused ? SystemBrushes.Highlight : SystemBrushes.Control)
                            : SystemBrushes.Window, e.Bounds);
                    TechItem tech = techListView.Items[e.ItemIndex].Tag as TechItem;
                    if (tech == null)
                    {
                        break;
                    }
                    DrawTechSpecialityItems(e, tech);
                    break;

                default:
                    e.DrawDefault = true;
                    break;
            }
        }

        /// <summary>
        ///     Technology list view research characteristic item drawing process
        /// </summary>
        /// <param name="e"></param>
        /// <param name="tech">Technical items</param>
        private void DrawTechSpecialityItems(DrawListViewSubItemEventArgs e, TechItem tech)
        {
            if (tech == null)
            {
                e.DrawDefault = true;
                return;
            }

            Rectangle gr = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 1, DeviceCaps.GetScaledWidth(16),
                DeviceCaps.GetScaledHeight(16));
            Rectangle tr = new Rectangle(e.Bounds.X + DeviceCaps.GetScaledWidth(16) + 3, e.Bounds.Y + 3,
                e.Bounds.Width - DeviceCaps.GetScaledWidth(16) - 3, e.Bounds.Height);
            Brush brush = new SolidBrush(
                (techListView.SelectedIndices.Count > 0) && (e.ItemIndex == techListView.SelectedIndices[0])
                    ? (techListView.Focused ? SystemColors.HighlightText : SystemColors.ControlText)
                    : SystemColors.WindowText);

            foreach (TechComponent component in tech.Components)
            {
                // Draw a research characteristic icon
                if ((component.Speciality != TechSpeciality.None) &&
                    ((int) component.Speciality - 1 < Techs.SpecialityImages.Images.Count))
                {
                    e.Graphics.DrawImage(
                        Techs.SpecialityImages.Images[Array.IndexOf(Techs.Specialities, component.Speciality) - 1], gr);
                }

                // Draw study difficulty
                e.Graphics.DrawString(IntHelper.ToString(component.Difficulty), techListView.Font, brush, tr);

                // Calculate the start position of the next item
                int offset = DeviceCaps.GetScaledWidth(32);
                gr.X += offset;
                tr.X += offset;
            }

            brush.Dispose();
        }

        /// <summary>
        ///     Column header drawing process of research institution list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechListViewDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        /// <summary>
        ///     Processing when changing the selection item in the technical list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            if (techListView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Update the research institution list
            UpdateTeamList();
        }

        /// <summary>
        ///     What to do when changing the width of a column in the technical list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTechListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < TechListColumnCount))
            {
                HoI2EditorController.Settings.ResearchViewer.TechListColumnWidth[e.ColumnIndex] =
                    techListView.Columns[e.ColumnIndex].Width;
            }
        }

        #endregion

        #region National list box

        /// <summary>
        ///     Initialize the national list box
        /// </summary>
        private void InitCountryListBox()
        {
            countryListBox.BeginUpdate();
            countryListBox.Items.Clear();
            foreach (Country country in Countries.Tags)
            {
                countryListBox.Items.Add(Countries.Strings[(int) country]);
            }

            // Processing selection events takes time, so temporarily disable it
            countryListBox.SelectedIndexChanged -= OnCountryListBoxSelectedIndexChanged;
            // Reflect the selected nation
            foreach (Country country in HoI2EditorController.Settings.ResearchViewer.Countries)
            {
                int index = Array.IndexOf(Countries.Tags, country);
                if (index >= 0)
                {
                    countryListBox.SetSelected(Array.IndexOf(Countries.Tags, country), true);
                }
            }
            // Undo selection event
            countryListBox.SelectedIndexChanged += OnCountryListBoxSelectedIndexChanged;

            countryListBox.EndUpdate();
        }

        /// <summary>
        ///     Processing when changing the selection item of the national list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Save the selected nation
            HoI2EditorController.Settings.ResearchViewer.Countries =
                countryListBox.SelectedIndices.Cast<int>().Select(index => Countries.Tags[index]).ToList();

            // Update the research institution list
            NarrowTeamList();
            UpdateTeamList();
        }

        #endregion

        #region Research institution list

        /// <summary>
        ///     Update the list of research institutes
        /// </summary>
        private void UpdateTeamList()
        {
            // Clear the research institute list if there is no selection in the technology list view
            if (techListView.SelectedItems.Count == 0)
            {
                teamListView.BeginUpdate();
                teamListView.Items.Clear();
                teamListView.EndUpdate();
                return;
            }

            TechItem tech = techListView.SelectedItems[0].Tag as TechItem;
            if (tech == null)
            {
                return;
            }

            // Update research speed list
            Researches.UpdateResearchList(tech, _teamList);

            teamListView.BeginUpdate();
            teamListView.Items.Clear();

            // Register items in order
            int rank = 1;
            foreach (Research research in Researches.Items)
            {
                teamListView.Items.Add(CreateTeamListViewItem(research, rank));
                rank++;
            }

            teamListView.EndUpdate();
        }

        /// <summary>
        ///     Narrow down the list of research institutes
        /// </summary>
        private void NarrowTeamList()
        {
            _teamList.Clear();

            // Create a list of selected nations
            List<Country> tags =
                countryListBox.SelectedItems.Cast<string>().Select(s => Countries.StringMap[s]).ToList();

            // Narrow down the research institutions belonging to the selected country in order
            _teamList.AddRange(Teams.Items.Where(team => tags.Contains(team.Country)));
        }

        /// <summary>
        ///     Create an item in the research institution list view
        /// </summary>
        /// <param name="research">Research speed data</param>
        /// <param name="rank">Research speed ranking</param>
        /// <returns>Items in the research institution list view</returns>
        private static ListViewItem CreateTeamListViewItem(Research research, int rank)
        {
            if (research == null)
            {
                return null;
            }

            ListViewItem item = new ListViewItem
            {
                Tag = research
            };
            item.SubItems.Add(IntHelper.ToString(rank));
            item.SubItems.Add(IntHelper.ToString(research.Days));
            item.SubItems.Add(research.EndDate.ToString());
            item.SubItems.Add(research.Team.Name);
            item.SubItems.Add(IntHelper.ToString(research.Team.Id));
            item.SubItems.Add(IntHelper.ToString(research.Team.Skill));
            item.SubItems.Add("");

            return item;
        }

        /// <summary>
        ///     Research institution list view sub-item drawing process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTeamListViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 7: // Research characteristics
                    e.Graphics.FillRectangle(
                        teamListView.SelectedIndices.Count > 0 && e.ItemIndex == teamListView.SelectedIndices[0]
                            ? (teamListView.Focused ? SystemBrushes.Highlight : SystemBrushes.Control)
                            : SystemBrushes.Window, e.Bounds);
                    Research research = teamListView.Items[e.ItemIndex].Tag as Research;
                    if (research == null)
                    {
                        break;
                    }
                    DrawTeamSpecialityIcon(e, research);
                    break;

                default:
                    e.DrawDefault = true;
                    break;
            }
        }

        /// <summary>
        ///     Research characteristic icon drawing process of research institution list view
        /// </summary>
        /// <param name="e"></param>
        /// <param name="research">Research speed data</param>
        private void DrawTeamSpecialityIcon(DrawListViewSubItemEventArgs e, Research research)
        {
            if (research == null)
            {
                return;
            }

            Rectangle rect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 1, DeviceCaps.GetScaledWidth(16),
                DeviceCaps.GetScaledHeight(16));
            for (int i = 0; i < Team.SpecialityLength; i++)
            {
                // Do nothing without research characteristics
                if (research.Team.Specialities[i] == TechSpeciality.None)
                {
                    continue;
                }

                // Draw a research characteristic icon
                if ((int) research.Team.Specialities[i] - 1 < Techs.SpecialityImages.Images.Count)
                {
                    e.Graphics.DrawImage(
                        Techs.SpecialityImages.Images[
                            Array.IndexOf(Techs.Specialities, research.Team.Specialities[i]) - 1], rect);
                }

                // Draw a research characteristic overlay icon
                if (research.Tech.Components.Any(component => component.Speciality == research.Team.Specialities[i]))
                {
                    e.Graphics.DrawImage(_techOverlayIcon, rect);
                }

                rect.X += DeviceCaps.GetScaledWidth(16) + 3;
            }
        }

        /// <summary>
        ///     Column header drawing process of research institution list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTeamListViewDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        /// <summary>
        ///     Processing when changing the width of columns in the research institution list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTeamListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < TeamListColumnCount))
            {
                HoI2EditorController.Settings.ResearchViewer.TeamListColumnWidth[e.ColumnIndex] =
                    teamListView.Columns[e.ColumnIndex].Width;
            }
        }

        #endregion

        #region Optional item

        /// <summary>
        ///     Initialize option items
        /// </summary>
        private void InitOptionItems()
        {
            if (Researches.DateMode == ResearchDateMode.Historical)
            {
                historicalRadioButton.Checked = true;
                yearNumericUpDown.Enabled = false;
                monthNumericUpDown.Enabled = false;
                dayNumericUpDown.Enabled = false;
            }
            else
            {
                specifiedRadioButton.Checked = true;
                yearNumericUpDown.Enabled = true;
                monthNumericUpDown.Enabled = true;
                dayNumericUpDown.Enabled = true;
            }
            yearNumericUpDown.Value = Researches.SpecifiedDate.Year;
            monthNumericUpDown.Value = Researches.SpecifiedDate.Month;
            dayNumericUpDown.Value = Researches.SpecifiedDate.Day;
            rocketNumericUpDown.Value = Researches.RocketTestingSites;
            nuclearNumericUpDown.Value = Researches.NuclearReactors;
            blueprintCheckBox.Checked = Researches.Blueprint;
            modifierTextBox.Text = DoubleHelper.ToString(Researches.Modifier);
        }

        /// <summary>
        ///     Use historical year Processing when changing the state of the check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHistoricalRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            // Update value
            Researches.DateMode = historicalRadioButton.Checked
                ? ResearchDateMode.Historical
                : ResearchDateMode.Specified;

            // Allow editing of dates only when using specified dates
            bool flag = Researches.DateMode == ResearchDateMode.Specified;
            yearNumericUpDown.Enabled = flag;
            monthNumericUpDown.Enabled = flag;
            dayNumericUpDown.Enabled = flag;

            // Update the research institution list
            UpdateTeamList();
        }

        /// <summary>
        ///     Processing when changing the designated year
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnYearNumericUpDownValueChanged(object sender, EventArgs e)
        {
            Researches.SpecifiedDate.Year = (int) yearNumericUpDown.Value;

            // Update the research institution list
            UpdateTeamList();
        }

        /// <summary>
        ///     Processing when changing the specified month
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMonthNumericUpDownValueChanged(object sender, EventArgs e)
        {
            Researches.SpecifiedDate.Month = (int) monthNumericUpDown.Value;

            // Update the research institution list
            UpdateTeamList();
        }

        /// <summary>
        ///     Processing when changing the specified date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDayNumericUpDownValueChanged(object sender, EventArgs e)
        {
            Researches.SpecifiedDate.Day = (int) dayNumericUpDown.Value;

            // Update the research institution list
            UpdateTeamList();
        }

        /// <summary>
        ///     Processing when the scale of the rocket test site is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRocketNumericUpDownValueChanged(object sender, EventArgs e)
        {
            Researches.RocketTestingSites = (int) rocketNumericUpDown.Value;

            // Update the research institution list
            UpdateTeamList();
        }

        /// <summary>
        ///     Processing when the scale of the reactor is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNuclearNumericUpDownValueChanged(object sender, EventArgs e)
        {
            Researches.NuclearReactors = (int) nuclearNumericUpDown.Value;

            // Update the research institution list
            UpdateTeamList();
        }

        /// <summary>
        ///     Processing when changing the state of the blue photo check box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBlueprintCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            Researches.Blueprint = blueprintCheckBox.Checked;

            // Update the research institution list
            UpdateTeamList();
        }

        /// <summary>
        ///     Processing when the state of the research institution start year consideration check box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConsiderStartYearCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            Researches.ConsiderStartYear = techteamstartyearCheckBox.Checked;

            // Update the research institution list
            UpdateTeamList();
        }

        /// <summary>
        ///     Processing when changing the value of research speed correction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModifierTextBoxValidated(object sender, EventArgs e)
        {
            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double modifier;
            if (!DoubleHelper.TryParse(modifierTextBox.Text, out modifier))
            {
                modifierTextBox.Text = DoubleHelper.ToString(Researches.Modifier);
                return;
            }

            // 0 If the value is below, it will not be possible to calculate properly, so insurance
            if (modifier <= 0.00005)
            {
                modifierTextBox.Text = DoubleHelper.ToString(Researches.Modifier);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(modifier, Researches.Modifier))
            {
                return;
            }

            // Update value
            Researches.Modifier = modifier;

            // Update the research institution list
            UpdateTeamList();
        }

        #endregion
    }
}
