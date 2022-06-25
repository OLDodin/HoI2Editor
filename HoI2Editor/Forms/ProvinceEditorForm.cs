using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Controls;
using HoI2Editor.Models;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Providence Editor Form
    /// </summary>
    public partial class ProvinceEditorForm : Form
    {
        #region Internal field

        /// <summary>
        ///     Provincial list after narrowing down
        /// </summary>
        private readonly List<Province> _list = new List<Province>();

        /// <summary>
        ///     World-wide node
        /// </summary>
        private readonly TreeNode _worldNode = new TreeNode { Text = Resources.World };

        /// <summary>
        ///     Sort target
        /// </summary>
        private SortKey _key = SortKey.None;

        /// <summary>
        ///     Sort order
        /// </summary>
        private SortOrder _order = SortOrder.Ascendant;

        /// <summary>
        ///     Sort target
        /// </summary>
        private enum SortKey
        {
            None,
            Name,
            Id,
            Sea,
            Port,
            Beach,
            Infrastructure,
            Ic,
            Manpower,
            Energy,
            Metal,
            RareMaterials,
            Oil
        }

        /// <summary>
        ///     Sort order
        /// </summary>
        private enum SortOrder
        {
            Ascendant,
            Decendant
        }

        #endregion

        #region Public constant

        /// <summary>
        ///     Number of columns in the Providence list view
        /// </summary>
        public const int ProvinceListColumnCount = 12;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public ProvinceEditorForm()
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
            // Update the edit items of the sea area
            UpdateSeaZoneItems();

            // Update the display of the world tree view
            UpdateWorldTree();
        }

        /// <summary>
        ///     Processing after data storage
        /// </summary>
        public void OnFileSaved()
        {
            // Update the display as the edited flag is cleared
            UpdateEditableItems();
        }

        /// <summary>
        ///     Processing after changing edit items
        /// </summary>
        /// <param name="id">Edit items ID</param>
        public void OnItemChanged(EditorItemId id)
        {
            // do nothing
        }

        #endregion

        #region Form

        /// <summary>
        ///     Form initialization
        /// </summary>
        private void InitForm()
        {
            // Providence list view
            nameColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[0];
            idColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[1];
            seaColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[2];
            portColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[3];
            beachColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[4];
            infraColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[5];
            icColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[6];
            manpowerColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[7];
            energyColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[8];
            metalColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[9];
            rareMaterialsColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[10];
            oilColumnHeader.Width = HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[11];

            // Window position
            Location = HoI2EditorController.Settings.ProvinceEditor.Location;
            Size = HoI2EditorController.Settings.ProvinceEditor.Size;
        }

        /// <summary>
        ///     Processing when loading a form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormLoad(object sender, EventArgs e)
        {
            // Initialize province data
            Provinces.Init();

            // Load the game settings file
            Misc.Load();

            // Read the character string definition file
            Config.Load();

            // Initialize edit items
            InitEditableItems();

            // Read the province file
            Provinces.Load();

            // Processing after reading data
            OnFileLoaded();
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
            HoI2EditorController.OnProvinceEditorFormClosed();
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
                HoI2EditorController.Settings.ProvinceEditor.Location = Location;
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
                HoI2EditorController.Settings.ProvinceEditor.Size = Size;
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

        #region World tree view

        /// <summary>
        ///     Update the world tree display
        /// </summary>
        private void UpdateWorldTree()
        {
            worldTreeView.BeginUpdate();
            worldTreeView.Nodes.Clear();

            // Add world-wide nodes
            worldTreeView.Nodes.Add(_worldNode);

            // Add continental nodes in order
            _worldNode.Nodes.Clear();
            foreach (ContinentId continent in Enum.GetValues(typeof (ContinentId)))
            {
                AddContinentTreeItem(continent, _worldNode);
            }

            // Expand world-wide nodes
            _worldNode.Expand();

            // Select a global node
            worldTreeView.SelectedNode = _worldNode;

            worldTreeView.EndUpdate();
        }

        /// <summary>
        ///     Processing when changing the selected node in the world tree view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWorldTreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            // Narrow down the Providence list
            NarrowProvinceList();

            // Update the display of the provision list
            UpdateProvinceList();
        }

        /// <summary>
        ///     Add a continental node
        /// </summary>
        /// <param name="continent">Continent</param>
        /// <param name="parent">Parent node</param>
        private static void AddContinentTreeItem(ContinentId continent, TreeNode parent)
        {
            // Add a continental node
            TreeNode node = new TreeNode { Text = Provinces.GetContinentName(continent), Tag = continent };
            parent.Nodes.Add(node);

            // Add local nodes in order
            if (Provinces.ContinentRegionMap.ContainsKey(continent))
            {
                foreach (RegionId region in Provinces.ContinentRegionMap[continent])
                {
                    AddRegionTreeItem(region, node);
                }
            }
        }

        /// <summary>
        ///     Add a local node
        /// </summary>
        /// <param name="region">Local</param>
        /// <param name="parent">Parent node</param>
        private static void AddRegionTreeItem(RegionId region, TreeNode parent)
        {
            // Add a local node
            TreeNode node = new TreeNode { Text = Provinces.GetRegionName(region), Tag = region };
            parent.Nodes.Add(node);

            // Add regional nodes in order
            if (Provinces.RegionAreaMap.ContainsKey(region))
            {
                foreach (AreaId area in Provinces.RegionAreaMap[region])
                {
                    AddAreaTreeItem(area, node);
                }
            }
        }

        /// <summary>
        ///     Add a regional node
        /// </summary>
        /// <param name="area">area</param>
        /// <param name="parent">Parent node</param>
        private static void AddAreaTreeItem(AreaId area, TreeNode parent)
        {
            // Add a regional node
            TreeNode node = new TreeNode { Text = Provinces.GetAreaName(area), Tag = area };
            parent.Nodes.Add(node);
        }

        #endregion

        #region Providence list view

        /// <summary>
        ///     Update the display of the provision list
        /// </summary>
        private void UpdateProvinceList()
        {
            provinceListView.BeginUpdate();
            provinceListView.Items.Clear();

            // Register items in order
            foreach (Province province in _list)
            {
                provinceListView.Items.Add(CreateProvinceListViewItem(province));
            }

            if (provinceListView.Items.Count > 0)
            {
                // Select the first item
                provinceListView.Items[0].Focused = true;
                provinceListView.Items[0].Selected = true;

                // Enable edit items
                EnableEditableItems();
            }
            else
            {
                // Disable edit items
                DisableEditableItems();
            }

            provinceListView.EndUpdate();
        }

        /// <summary>
        ///     Narrow down the Providence list
        /// </summary>
        private void NarrowProvinceList()
        {
            _list.Clear();

            TreeNode node = worldTreeView.SelectedNode;

            // The whole world
            if (node.Tag == null)
            {
                _list.AddRange(Provinces.Items);
                return;
            }

            // Continent
            if (node.Tag is ContinentId)
            {
                ContinentId continent = (ContinentId) node.Tag;
                _list.AddRange(Provinces.Items.Where(province => province.Continent == continent));
                return;
            }

            // Local
            if (node.Tag is RegionId)
            {
                RegionId region = (RegionId) node.Tag;
                _list.AddRange(Provinces.Items.Where(province => province.Region == region));
                return;
            }

            // area
            if (node.Tag is AreaId)
            {
                AreaId area = (AreaId) node.Tag;
                _list.AddRange(Provinces.Items.Where(province => province.Area == area));
                return;
            }

            _list.AddRange(Provinces.Items);
        }

        /// <summary>
        ///     Sort the province list
        /// </summary>
        private void SortProvinceList()
        {
            switch (_key)
            {
                case SortKey.None: // No sort
                    break;

                case SortKey.Name: // name
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((province1, province2) => string.CompareOrdinal(province1.Name, province2.Name));
                    }
                    else
                    {
                        _list.Sort((province1, province2) => string.CompareOrdinal(province2.Name, province1.Name));
                    }
                    break;

                case SortKey.Id: // ID
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((province1, province2) => province1.Id - province2.Id);
                    }
                    else
                    {
                        _list.Sort((province1, province2) => province2.Id - province1.Id);
                    }
                    break;

                case SortKey.Sea: // Whether it is a marine province
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((province1, province2) =>
                        {
                            if (province1.Terrain == TerrainId.Ocean && province2.Terrain != TerrainId.Ocean)
                            {
                                return 1;
                            }
                            if (province2.Terrain == TerrainId.Ocean && province1.Terrain != TerrainId.Ocean)
                            {
                                return -1;
                            }
                            return 0;
                        });
                    }
                    else
                    {
                        _list.Sort((province1, province2) =>
                        {
                            if (province2.Terrain == TerrainId.Ocean && province1.Terrain != TerrainId.Ocean)
                            {
                                return 1;
                            }
                            if (province1.Terrain == TerrainId.Ocean && province2.Terrain != TerrainId.Ocean)
                            {
                                return -1;
                            }
                            return 0;
                        });
                    }
                    break;

                case SortKey.Port: // Presence or absence of a port
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((province1, province2) =>
                        {
                            if (province1.PortAllowed && !province2.PortAllowed)
                            {
                                return 1;
                            }
                            if (!province1.PortAllowed && province2.PortAllowed)
                            {
                                return -1;
                            }
                            return 0;
                        });
                    }
                    else
                    {
                        _list.Sort((province1, province2) =>
                        {
                            if (province2.PortAllowed && !province1.PortAllowed)
                            {
                                return 1;
                            }
                            if (!province2.PortAllowed && province1.PortAllowed)
                            {
                                return -1;
                            }
                            return 0;
                        });
                    }
                    break;

                case SortKey.Beach: // Presence or absence of sandy beach
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((province1, province2) =>
                        {
                            if (province1.Beaches && !province2.Beaches)
                            {
                                return 1;
                            }
                            if (!province1.Beaches && province2.Beaches)
                            {
                                return -1;
                            }
                            return 0;
                        });
                    }
                    else
                    {
                        _list.Sort((province1, province2) =>
                        {
                            if (province2.Beaches && !province1.Beaches)
                            {
                                return 1;
                            }
                            if (!province2.Beaches && province1.Beaches)
                            {
                                return -1;
                            }
                            return 0;
                        });
                    }
                    break;

                case SortKey.Infrastructure: // infrastructure
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort(
                            (province1, province2) => Math.Sign(province1.Infrastructure - province2.Infrastructure));
                    }
                    else
                    {
                        _list.Sort(
                            (province1, province2) => Math.Sign(province2.Infrastructure - province1.Infrastructure));
                    }
                    break;

                case SortKey.Ic: // I C
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((province1, province2) => Math.Sign(province1.Ic - province2.Ic));
                    }
                    else
                    {
                        _list.Sort((province1, province2) => Math.Sign(province2.Ic - province1.Ic));
                    }
                    break;

                case SortKey.Manpower: // Labor force
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((province1, province2) => Math.Sign(province1.Manpower - province2.Manpower));
                    }
                    else
                    {
                        _list.Sort((province1, province2) => Math.Sign(province2.Manpower - province1.Manpower));
                    }
                    break;

                case SortKey.Energy: // energy
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((province1, province2) => Math.Sign(province1.Energy - province2.Energy));
                    }
                    else
                    {
                        _list.Sort((province1, province2) => Math.Sign(province2.Energy - province1.Energy));
                    }
                    break;

                case SortKey.Metal: // metal
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((province1, province2) => Math.Sign(province1.Metal - province2.Metal));
                    }
                    else
                    {
                        _list.Sort((province1, province2) => Math.Sign(province2.Metal - province1.Metal));
                    }
                    break;

                case SortKey.RareMaterials: // Rare resources
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort(
                            (province1, province2) => Math.Sign(province1.RareMaterials - province2.RareMaterials));
                    }
                    else
                    {
                        _list.Sort(
                            (province1, province2) => Math.Sign(province2.RareMaterials - province1.RareMaterials));
                    }
                    break;

                case SortKey.Oil: // oil
                    if (_order == SortOrder.Ascendant)
                    {
                        _list.Sort((province1, province2) => Math.Sign(province1.Oil - province2.Oil));
                    }
                    else
                    {
                        _list.Sort((province1, province2) => Math.Sign(province2.Oil - province1.Oil));
                    }
                    break;
            }
        }

        /// <summary>
        ///     Processing when changing the selection item in the province list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update edit items
            UpdateEditableItems();
        }

        /// <summary>
        ///     Processing before editing items in the Providence list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceListViewQueryItemEdit(object sender, QueryListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // name
                    e.Type = ItemEditType.Text;
                    e.Text = nameTextBox.Text;
                    break;

                case 3: // Harbor
                    e.Type = ItemEditType.Bool;
                    e.Flag = portCheckBox.Checked;
                    break;

                case 4: // Sandy beach
                    e.Type = ItemEditType.Bool;
                    e.Flag = beachCheckBox.Checked;
                    break;

                case 5: // infrastructure
                    e.Type = ItemEditType.Text;
                    e.Text = infraTextBox.Text;
                    break;

                case 6: // I C
                    e.Type = ItemEditType.Text;
                    e.Text = icTextBox.Text;
                    break;

                case 7: // Labor force
                    e.Type = ItemEditType.Text;
                    e.Text = manpowerTextBox.Text;
                    break;

                case 8: // energy
                    e.Type = ItemEditType.Text;
                    e.Text = energyTextBox.Text;
                    break;

                case 9: // metal
                    e.Type = ItemEditType.Text;
                    e.Text = metalTextBox.Text;
                    break;

                case 10: // Rare resources
                    e.Type = ItemEditType.Text;
                    e.Text = rareMaterialsTextBox.Text;
                    break;

                case 11: // oil
                    e.Type = ItemEditType.Text;
                    e.Text = oilTextBox.Text;
                    break;
            }
        }

        /// <summary>
        ///     Processing after editing items in the Providence list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceListViewBeforeItemEdit(object sender, ListViewItemEditEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // name
                    nameTextBox.Text = e.Text;
                    break;

                case 3: // Harbor
                    portCheckBox.Checked = e.Flag;
                    break;

                case 4: // Sandy beach
                    beachCheckBox.Checked = e.Flag;
                    break;

                case 5: // infrastructure
                    infraTextBox.Text = e.Text;
                    OnInfraTextBoxValidated(infraTextBox, new EventArgs());
                    break;

                case 6: // I C
                    icTextBox.Text = e.Text;
                    OnIcTextBoxValidated(icTextBox, new EventArgs());
                    break;

                case 7: // Labor force
                    manpowerTextBox.Text = e.Text;
                    OnManpowerTextBoxValidated(manpowerTextBox, new EventArgs());
                    break;

                case 8: // energy
                    energyTextBox.Text = e.Text;
                    OnEnergyTextBoxValidated(energyTextBox, new EventArgs());
                    break;

                case 9: // metal
                    metalTextBox.Text = e.Text;
                    OnMetalTextBoxValidated(metalTextBox, new EventArgs());
                    break;

                case 10: // Rare resources
                    rareMaterialsTextBox.Text = e.Text;
                    OnRareMaterialsTextBoxValidated(rareMaterialsTextBox, new EventArgs());
                    break;

                case 11: // oil
                    oilTextBox.Text = e.Text;
                    OnOilNumericUpDownValidated(oilTextBox, new EventArgs());
                    break;
            }

            // Since the items in the list view will be updated by yourself, it will be treated as canceled.
            e.Cancel = true;
        }

        /// <summary>
        ///     Processing when a column is clicked in the ministerial list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeaderListViewColumnClick(object sender, ColumnClickEventArgs e)
        {
            switch (e.Column)
            {
                case 0: // name
                    if (_key == SortKey.Name)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Name;
                    }
                    break;

                case 1: // ID
                    if (_key == SortKey.Id)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Id;
                    }
                    break;

                case 2: // Whether it is a marine province
                    if (_key == SortKey.Sea)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Sea;
                    }
                    break;

                case 3: // Presence or absence of a port
                    if (_key == SortKey.Port)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Port;
                    }
                    break;

                case 4: // Presence or absence of sandy beach
                    if (_key == SortKey.Beach)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Beach;
                    }
                    break;

                case 5: // infrastructure
                    if (_key == SortKey.Infrastructure)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Infrastructure;
                    }
                    break;

                case 6: // I C
                    if (_key == SortKey.Ic)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Ic;
                    }
                    break;

                case 7: // Labor force
                    if (_key == SortKey.Manpower)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Manpower;
                    }
                    break;

                case 8: // energy
                    if (_key == SortKey.Energy)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Energy;
                    }
                    break;

                case 9: // metal
                    if (_key == SortKey.Metal)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Metal;
                    }
                    break;

                case 10: // Rare resources
                    if (_key == SortKey.RareMaterials)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.RareMaterials;
                    }
                    break;

                case 11: // oil
                    if (_key == SortKey.Oil)
                    {
                        _order = _order == SortOrder.Ascendant ? SortOrder.Decendant : SortOrder.Ascendant;
                    }
                    else
                    {
                        _key = SortKey.Oil;
                    }
                    break;

                default:
                    // Do nothing when clicking on a column with no items
                    return;
            }

            // Sort the provisions list
            SortProvinceList();

            // Update the province list
            UpdateProvinceList();
        }

        /// <summary>
        ///     Processing when changing the width of columns in the Providence list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProvinceListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if ((e.ColumnIndex >= 0) && (e.ColumnIndex < ProvinceListColumnCount))
            {
                HoI2EditorController.Settings.ProvinceEditor.ListColumnWidth[e.ColumnIndex] =
                    provinceListView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        ///     Create an item in the Providence list view
        /// </summary>
        /// <param name="province">Providence data</param>
        /// <returns>Province list view items</returns>
        private static ListViewItem CreateProvinceListViewItem(Province province)
        {
            if (province == null)
            {
                return null;
            }

            ListViewItem item = new ListViewItem
            {
                Text = province.GetName(),
                Tag = province
            };
            item.SubItems.Add(IntHelper.ToString(province.Id));
            item.SubItems.Add(province.Terrain == TerrainId.Ocean ? Resources.Yes : Resources.No);
            item.SubItems.Add(province.PortAllowed ? Resources.Yes : Resources.No);
            item.SubItems.Add(province.Beaches ? Resources.Yes : Resources.No);
            item.SubItems.Add(DoubleHelper.ToString(province.Infrastructure));
            item.SubItems.Add(DoubleHelper.ToString(province.Ic));
            item.SubItems.Add(DoubleHelper.ToString(province.Manpower));
            item.SubItems.Add(DoubleHelper.ToString(province.Energy));
            item.SubItems.Add(DoubleHelper.ToString(province.Metal));
            item.SubItems.Add(DoubleHelper.ToString(province.RareMaterials));
            item.SubItems.Add(DoubleHelper.ToString(province.Oil));

            return item;
        }

        /// <summary>
        ///     Get the selected province data
        /// </summary>
        /// <returns>Selected province data</returns>
        private Province GetSelectedProvince()
        {
            // If there is no selection
            if (provinceListView.SelectedItems.Count == 0)
            {
                return null;
            }

            return provinceListView.SelectedItems[0].Tag as Province;
        }

        #endregion

        #region Edit items

        /// <summary>
        ///     Initialize edit items
        /// </summary>
        private void InitEditableItems()
        {
            Graphics g = Graphics.FromHwnd(Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;

            // Continent
            continentComboBox.BeginUpdate();
            continentComboBox.Items.Clear();
            int width = continentComboBox.Width;
            foreach (ContinentId continent in Provinces.Continents)
            {
                string s = Provinces.GetContinentName(continent);
                continentComboBox.Items.Add(s);
                width = Math.Max(width, (int) g.MeasureString(s, continentComboBox.Font).Width + margin);
            }
            continentComboBox.DropDownWidth = width;
            continentComboBox.EndUpdate();

            // Local
            regionComboBox.BeginUpdate();
            regionComboBox.Items.Clear();
            width = regionComboBox.Width;
            foreach (RegionId region in Provinces.Regions)
            {
                string s = Provinces.GetRegionName(region);
                regionComboBox.Items.Add(s);
                width = Math.Max(width,
                    (int) g.MeasureString(s, regionComboBox.Font).Width + SystemInformation.VerticalScrollBarWidth +
                    margin);
            }
            regionComboBox.DropDownWidth = width;
            regionComboBox.EndUpdate();

            // area
            areaComboBox.BeginUpdate();
            areaComboBox.Items.Clear();
            width = areaComboBox.Width;
            foreach (AreaId area in Provinces.Areas)
            {
                string s = Provinces.GetAreaName(area);
                areaComboBox.Items.Add(s);
                width = Math.Max(width,
                    (int) g.MeasureString(s, areaComboBox.Font).Width + SystemInformation.VerticalScrollBarWidth +
                    margin);
            }
            areaComboBox.DropDownWidth = width;
            areaComboBox.EndUpdate();

            // climate
            climateComboBox.BeginUpdate();
            climateComboBox.Items.Clear();
            width = climateComboBox.Width;
            foreach (ClimateId climate in Provinces.Climates)
            {
                string s = Provinces.GetClimateName(climate);
                climateComboBox.Items.Add(s);
                width = Math.Max(width, (int) g.MeasureString(s, climateComboBox.Font).Width + margin);
            }
            climateComboBox.DropDownWidth = width;
            climateComboBox.EndUpdate();

            // terrain
            terrainComboBox.BeginUpdate();
            terrainComboBox.Items.Clear();
            width = terrainComboBox.Width;
            foreach (TerrainId terrain in Provinces.Terrains)
            {
                string s = Provinces.GetTerrainName(terrain);
                terrainComboBox.Items.Add(s);
                width = Math.Max(width, (int) g.MeasureString(s, terrainComboBox.Font).Width + margin);
            }
            terrainComboBox.DropDownWidth = width;
            terrainComboBox.EndUpdate();
        }

        /// <summary>
        ///     Update edit items
        /// </summary>
        private void UpdateEditableItems()
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Update the value of the edit item
            UpdateEditableItemsValue(province);

            // Update the color of the edit item
            UpdateEditableItemsColor(province);

            // Update the Providence image
            UpdateProvinceImage(province);
        }

        /// <summary>
        ///     Update the value of the edit item
        /// </summary>
        /// <param name="province">Providence data</param>
        private void UpdateEditableItemsValue(Province province)
        {
            // basic setting
            idNumericUpDown.Value = province.Id;
            nameTextBox.Text = province.GetName();
            if (Provinces.Continents.Contains(province.Continent))
            {
                continentComboBox.SelectedIndex = Provinces.Continents.IndexOf(province.Continent);
            }
            else
            {
                continentComboBox.SelectedIndex = -1;
                continentComboBox.Text = Provinces.GetContinentName(province.Continent);
            }
            if (Provinces.Regions.Contains(province.Region))
            {
                regionComboBox.SelectedIndex = Provinces.Regions.IndexOf(province.Region);
            }
            else
            {
                regionComboBox.SelectedIndex = -1;
                regionComboBox.Text = Provinces.GetRegionName(province.Region);
            }
            if (Provinces.Areas.Contains(province.Area))
            {
                areaComboBox.SelectedIndex = Provinces.Areas.IndexOf(province.Area);
            }
            else
            {
                areaComboBox.SelectedIndex = -1;
                areaComboBox.Text = Provinces.GetAreaName(province.Area);
            }
            if (Provinces.Climates.Contains(province.Climate))
            {
                climateComboBox.SelectedIndex = Provinces.Climates.IndexOf(province.Climate);
            }
            else
            {
                climateComboBox.SelectedIndex = -1;
                climateComboBox.Text = Provinces.GetClimateName(province.Climate);
            }
            if (Provinces.Terrains.Contains(province.Terrain))
            {
                terrainComboBox.SelectedIndex = Provinces.Terrains.IndexOf(province.Terrain);
            }
            else
            {
                terrainComboBox.SelectedIndex = -1;
                terrainComboBox.Text = Provinces.GetTerrainName(province.Terrain);
            }

            // Resource setting
            infraTextBox.Text = DoubleHelper.ToString(province.Infrastructure);
            icTextBox.Text = DoubleHelper.ToString(province.Ic);
            manpowerTextBox.Text = DoubleHelper.ToString(province.Manpower);
            energyTextBox.Text = DoubleHelper.ToString(province.Energy);
            metalTextBox.Text = DoubleHelper.ToString(province.Metal);
            rareMaterialsTextBox.Text = DoubleHelper.ToString(province.RareMaterials);
            oilTextBox.Text = DoubleHelper.ToString(province.Oil);

            // Coordinate setting
            beachCheckBox.Checked = province.Beaches;
            beachXNumericUpDown.Value = province.BeachXPos;
            beachYNumericUpDown.Value = province.BeachYPos;
            beachIconNumericUpDown.Value = province.BeachIcon;
            portCheckBox.Checked = province.PortAllowed;
            portXNumericUpDown.Value = province.PortXPos;
            portYNumericUpDown.Value = province.PortYPos;
            portSeaZoneNumericUpDown.Value = province.PortSeaZone;
            if (Provinces.SeaZones.Contains(province.PortSeaZone))
            {
                portSeaZoneComboBox.SelectedIndex = Provinces.SeaZones.IndexOf(province.PortSeaZone);
            }
            else
            {
                portSeaZoneComboBox.SelectedIndex = -1;
                if (province.PortSeaZone > 0)
                {
                    Province seaProvince = Provinces.Items.First(prov => prov.Id == province.PortSeaZone);
                    portSeaZoneComboBox.Text = Config.GetText(seaProvince.Name);
                }
            }
            cityXNumericUpDown.Value = province.CityXPos;
            cityYNumericUpDown.Value = province.CityYPos;
            fortXNumericUpDown.Value = province.FortXPos;
            fortYNumericUpDown.Value = province.FortYPos;
            aaXNumericUpDown.Value = province.AaXPos;
            aaYNumericUpDown.Value = province.AaYPos;
            armyXNumericUpDown.Value = province.ArmyXPos;
            armyYNumericUpDown.Value = province.ArmyYPos;
            counterXNumericUpDown.Value = province.CounterXPos;
            counterYNumericUpDown.Value = province.CounterYPos;
            fillXNumericUpDown1.Value = province.FillCoordX1;
            fillYNumericUpDown1.Value = province.FillCoordY1;
            fillXNumericUpDown2.Value = province.FillCoordX2;
            fillYNumericUpDown2.Value = province.FillCoordY2;
            fillXNumericUpDown3.Value = province.FillCoordX3;
            fillYNumericUpDown3.Value = province.FillCoordY3;
            fillXNumericUpDown4.Value = province.FillCoordX4;
            fillYNumericUpDown4.Value = province.FillCoordY4;
        }

        /// <summary>
        ///     Update the color of the edit item
        /// </summary>
        /// <param name="province">Providence data</param>
        private void UpdateEditableItemsColor(Province province)
        {
            // Update the color of the combo box
            continentComboBox.Refresh();
            regionComboBox.Refresh();
            areaComboBox.Refresh();
            climateComboBox.Refresh();
            terrainComboBox.Refresh();
            portSeaZoneComboBox.Refresh();

            // Update the color of the edit item
            idNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.Id) ? Color.Red : SystemColors.WindowText;
            nameTextBox.ForeColor = province.IsDirty(ProvinceItemId.Name) ? Color.Red : SystemColors.WindowText;

            infraTextBox.ForeColor = province.IsDirty(ProvinceItemId.Infrastructure)
                ? Color.Red
                : SystemColors.WindowText;
            icTextBox.ForeColor = province.IsDirty(ProvinceItemId.Ic) ? Color.Red : SystemColors.WindowText;
            manpowerTextBox.ForeColor = province.IsDirty(ProvinceItemId.Manpower)
                ? Color.Red
                : SystemColors.WindowText;
            energyTextBox.ForeColor = province.IsDirty(ProvinceItemId.Energy) ? Color.Red : SystemColors.WindowText;
            metalTextBox.ForeColor = province.IsDirty(ProvinceItemId.Metal) ? Color.Red : SystemColors.WindowText;
            rareMaterialsTextBox.ForeColor = province.IsDirty(ProvinceItemId.RareMaterials)
                ? Color.Red
                : SystemColors.WindowText;
            oilTextBox.ForeColor = province.IsDirty(ProvinceItemId.Oil) ? Color.Red : SystemColors.WindowText;

            beachCheckBox.ForeColor = province.IsDirty(ProvinceItemId.Beaches) ? Color.Red : SystemColors.WindowText;
            beachXNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.BeachXPos)
                ? Color.Red
                : SystemColors.WindowText;
            beachYNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.BeachYPos)
                ? Color.Red
                : SystemColors.WindowText;
            beachIconNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.BeachIcon)
                ? Color.Red
                : SystemColors.WindowText;
            portCheckBox.ForeColor = province.IsDirty(ProvinceItemId.PortAllowed) ? Color.Red : SystemColors.WindowText;
            portXNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.PortXPos)
                ? Color.Red
                : SystemColors.WindowText;
            portYNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.PortYPos)
                ? Color.Red
                : SystemColors.WindowText;
            portSeaZoneNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.PortSeaZone)
                ? Color.Red
                : SystemColors.WindowText;
            cityXNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.CityXPos)
                ? Color.Red
                : SystemColors.WindowText;
            cityYNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.CityYPos)
                ? Color.Red
                : SystemColors.WindowText;
            fortXNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.FortXPos)
                ? Color.Red
                : SystemColors.WindowText;
            fortYNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.FortYPos)
                ? Color.Red
                : SystemColors.WindowText;
            aaXNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.AaXPos) ? Color.Red : SystemColors.WindowText;
            aaYNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.AaYPos) ? Color.Red : SystemColors.WindowText;
            armyXNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.ArmyXPos)
                ? Color.Red
                : SystemColors.WindowText;
            armyYNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.ArmyYPos)
                ? Color.Red
                : SystemColors.WindowText;
            counterXNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.CounterXPos)
                ? Color.Red
                : SystemColors.WindowText;
            counterYNumericUpDown.ForeColor = province.IsDirty(ProvinceItemId.CounterYPos)
                ? Color.Red
                : SystemColors.WindowText;
            fillXNumericUpDown1.ForeColor = province.IsDirty(ProvinceItemId.FillCoordX1)
                ? Color.Red
                : SystemColors.WindowText;
            fillYNumericUpDown1.ForeColor = province.IsDirty(ProvinceItemId.FillCoordY1)
                ? Color.Red
                : SystemColors.WindowText;
            fillXNumericUpDown2.ForeColor = province.IsDirty(ProvinceItemId.FillCoordX2)
                ? Color.Red
                : SystemColors.WindowText;
            fillYNumericUpDown2.ForeColor = province.IsDirty(ProvinceItemId.FillCoordY2)
                ? Color.Red
                : SystemColors.WindowText;
            fillXNumericUpDown3.ForeColor = province.IsDirty(ProvinceItemId.FillCoordX3)
                ? Color.Red
                : SystemColors.WindowText;
            fillYNumericUpDown3.ForeColor = province.IsDirty(ProvinceItemId.FillCoordY3)
                ? Color.Red
                : SystemColors.WindowText;
            fillXNumericUpDown4.ForeColor = province.IsDirty(ProvinceItemId.FillCoordX4)
                ? Color.Red
                : SystemColors.WindowText;
            fillYNumericUpDown4.ForeColor = province.IsDirty(ProvinceItemId.FillCoordY4)
                ? Color.Red
                : SystemColors.WindowText;
        }

        /// <summary>
        ///     Enable edit items
        /// </summary>
        private void EnableEditableItems()
        {
            basicGroupBox.Enabled = true;
            resourceGroupBox.Enabled = true;
            positionGroupBox.Enabled = true;

            // Reset the character string cleared at the time of invalidation
            idNumericUpDown.Text = IntHelper.ToString((int) idNumericUpDown.Value);
            beachXNumericUpDown.Text = IntHelper.ToString((int) beachXNumericUpDown.Value);
            beachYNumericUpDown.Text = IntHelper.ToString((int) beachYNumericUpDown.Value);
            beachIconNumericUpDown.Text = IntHelper.ToString((int) beachIconNumericUpDown.Value);
            portXNumericUpDown.Text = IntHelper.ToString((int) portXNumericUpDown.Value);
            portYNumericUpDown.Text = IntHelper.ToString((int) portYNumericUpDown.Value);
            portSeaZoneNumericUpDown.Text = IntHelper.ToString((int) portSeaZoneNumericUpDown.Value);
            cityXNumericUpDown.Text = IntHelper.ToString((int) cityXNumericUpDown.Value);
            cityYNumericUpDown.Text = IntHelper.ToString((int) cityYNumericUpDown.Value);
            fortXNumericUpDown.Text = IntHelper.ToString((int) fortXNumericUpDown.Value);
            fortYNumericUpDown.Text = IntHelper.ToString((int) fortYNumericUpDown.Value);
            aaXNumericUpDown.Text = IntHelper.ToString((int) aaXNumericUpDown.Value);
            aaYNumericUpDown.Text = IntHelper.ToString((int) aaYNumericUpDown.Value);
            armyXNumericUpDown.Text = IntHelper.ToString((int) armyXNumericUpDown.Value);
            armyYNumericUpDown.Text = IntHelper.ToString((int) armyYNumericUpDown.Value);
            counterXNumericUpDown.Text = IntHelper.ToString((int) counterXNumericUpDown.Value);
            counterYNumericUpDown.Text = IntHelper.ToString((int) counterYNumericUpDown.Value);
            fillXNumericUpDown1.Text = IntHelper.ToString((int) fillXNumericUpDown1.Value);
            fillYNumericUpDown1.Text = IntHelper.ToString((int) fillYNumericUpDown1.Value);
            fillXNumericUpDown2.Text = IntHelper.ToString((int) fillXNumericUpDown2.Value);
            fillYNumericUpDown2.Text = IntHelper.ToString((int) fillYNumericUpDown2.Value);
            fillXNumericUpDown3.Text = IntHelper.ToString((int) fillXNumericUpDown3.Value);
            fillYNumericUpDown3.Text = IntHelper.ToString((int) fillYNumericUpDown3.Value);
            fillXNumericUpDown4.Text = IntHelper.ToString((int) fillXNumericUpDown4.Value);
            fillYNumericUpDown4.Text = IntHelper.ToString((int) fillYNumericUpDown4.Value);
        }

        /// <summary>
        ///     Disable edit items
        /// </summary>
        private void DisableEditableItems()
        {
            idNumericUpDown.ResetText();
            nameTextBox.ResetText();
            continentComboBox.SelectedIndex = -1;
            continentComboBox.ResetText();
            regionComboBox.SelectedIndex = -1;
            regionComboBox.ResetText();
            areaComboBox.SelectedIndex = -1;
            areaComboBox.ResetText();
            climateComboBox.SelectedIndex = -1;
            climateComboBox.ResetText();
            terrainComboBox.SelectedIndex = -1;
            terrainComboBox.ResetText();

            infraTextBox.ResetText();
            icTextBox.ResetText();
            manpowerTextBox.ResetText();
            energyTextBox.ResetText();
            metalTextBox.ResetText();
            rareMaterialsTextBox.ResetText();
            oilTextBox.ResetText();

            beachCheckBox.Checked = false;
            beachXNumericUpDown.ResetText();
            beachYNumericUpDown.ResetText();
            beachIconNumericUpDown.ResetText();
            portCheckBox.Checked = false;
            portXNumericUpDown.ResetText();
            portYNumericUpDown.ResetText();
            portSeaZoneNumericUpDown.ResetText();
            portSeaZoneComboBox.SelectedIndex = -1;
            portSeaZoneComboBox.ResetText();
            cityXNumericUpDown.ResetText();
            cityYNumericUpDown.ResetText();
            fortXNumericUpDown.ResetText();
            fortYNumericUpDown.ResetText();
            aaXNumericUpDown.ResetText();
            aaYNumericUpDown.ResetText();
            armyXNumericUpDown.ResetText();
            armyYNumericUpDown.ResetText();
            counterXNumericUpDown.ResetText();
            counterYNumericUpDown.ResetText();
            fillXNumericUpDown1.ResetText();
            fillYNumericUpDown1.ResetText();
            fillXNumericUpDown2.ResetText();
            fillYNumericUpDown2.ResetText();
            fillXNumericUpDown3.ResetText();
            fillYNumericUpDown3.ResetText();
            fillXNumericUpDown4.ResetText();
            fillYNumericUpDown4.ResetText();

            basicGroupBox.Enabled = false;
            resourceGroupBox.Enabled = false;
            positionGroupBox.Enabled = false;
        }

        /// <summary>
        ///     Update the item of the sea area combo box of the port
        /// </summary>
        private void UpdateSeaZoneItems()
        {
            Graphics g = Graphics.FromHwnd(Handle);
            int margin = DeviceCaps.GetScaledWidth(2) + 1;

            portSeaZoneComboBox.BeginUpdate();
            portSeaZoneComboBox.Items.Clear();
            int maxWidth = portSeaZoneComboBox.Width;
            foreach (int id in Provinces.SeaZones)
            {
                Province province = Provinces.SeaZoneMap[id];
                string s = province.GetName();
                portSeaZoneComboBox.Items.Add(s);
                maxWidth = Math.Max(maxWidth,
                    (int) g.MeasureString(s, portSeaZoneComboBox.Font).Width + SystemInformation.VerticalScrollBarWidth +
                    margin);
            }
            portSeaZoneComboBox.DropDownWidth = maxWidth;
            portSeaZoneComboBox.EndUpdate();
        }

        /// <summary>
        ///     Update province images
        /// </summary>
        /// <param name="province">Providence</param>
        private void UpdateProvinceImage(Province province)
        {
            string fileName = Game.GetReadFileName(Game.GetProvinceImageFileName(province.Id));
            provincePictureBox.ImageLocation = File.Exists(fileName) ? fileName : "";
        }

        /// <summary>
        ///     Item drawing process of continental combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContinentComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Province province = GetSelectedProvince();
            if (province != null)
            {
                Brush brush;
                if ((Provinces.Continents[e.Index] == province.Continent) && province.IsDirty(ProvinceItemId.Continent))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = continentComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of local combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRegionComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Province province = GetSelectedProvince();
            if (province != null)
            {
                Brush brush;
                if ((Provinces.Regions[e.Index] == province.Region) && province.IsDirty(ProvinceItemId.Region))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = regionComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of regional combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAreaComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Province province = GetSelectedProvince();
            if (province != null)
            {
                Brush brush;
                if ((Provinces.Areas[e.Index] == province.Area) && province.IsDirty(ProvinceItemId.Area))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = areaComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of climate combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClimateComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Province province = GetSelectedProvince();
            if (province != null)
            {
                Brush brush;
                if ((Provinces.Climates[e.Index] == province.Climate) && province.IsDirty(ProvinceItemId.Climate))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = climateComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of terrain combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTerrainComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Province province = GetSelectedProvince();
            if (province != null)
            {
                Brush brush;
                if ((Provinces.Terrains[e.Index] == province.Terrain) && province.IsDirty(ProvinceItemId.Terrain))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = terrainComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Item drawing process of the sea area combo box of the port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPortSeaZoneComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Province province = GetSelectedProvince();
            if (province != null)
            {
                Brush brush;
                if ((Provinces.SeaZones[e.Index] == province.PortSeaZone) &&
                    province.IsDirty(ProvinceItemId.PortSeaZone))
                {
                    brush = new SolidBrush(Color.Red);
                }
                else
                {
                    brush = new SolidBrush(SystemColors.WindowText);
                }
                string s = portSeaZoneComboBox.Items[e.Index].ToString();
                e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
                brush.Dispose();
            }

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the name string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNameTextBoxTextChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            string name = nameTextBox.Text;
            if (string.IsNullOrEmpty(name))
            {
                if (string.IsNullOrEmpty(province.Name))
                {
                    return;
                }
            }
            else
            {
                if (name.Equals(province.GetName()))
                {
                    return;
                }
            }

            Log.Info("[Province] name: {0} -> {1} ({2})", province.GetName(), name, province.Id);

            // Update value
            Config.SetText(province.Name, name, Game.ProvinceTextFileName);

            // Update items in the province list view
            provinceListView.SelectedItems[0].SubItems[0].Text = province.GetName();

            // Set the edited flag
            province.SetDirty(ProvinceItemId.Name);

            // Change the font color
            nameTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing continents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContinentComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (continentComboBox.SelectedIndex == -1)
            {
                return;
            }
            ContinentId continent = Provinces.Continents[continentComboBox.SelectedIndex];
            if (continent == province.Continent)
            {
                return;
            }

            Log.Info("[Province] continent: {0} -> {1} ({2}: {3})", Provinces.GetContinentName(province.Continent),
                Provinces.GetContinentName(continent), province.Id, province.GetName());

            // Update value
            Provinces.ModifyContinent(province, continent);

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Continent);

            // Update drawing to change the item color of the continent combo box
            continentComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when changing regions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRegionComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (regionComboBox.SelectedIndex == -1)
            {
                return;
            }
            RegionId region = Provinces.Regions[regionComboBox.SelectedIndex];
            if (region == province.Region)
            {
                return;
            }

            Log.Info("[Province] region: {0} -> {1} ({2}: {3})", Provinces.GetRegionName(province.Region),
                Provinces.GetRegionName(region), province.Id, province.GetName());

            // Update value
            Provinces.ModifyRegion(province, region);

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Region);

            // Update drawing to change the item color of the local combo box
            regionComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when changing regions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAreaComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (areaComboBox.SelectedIndex == -1)
            {
                return;
            }
            AreaId area = Provinces.Areas[areaComboBox.SelectedIndex];
            if (area == province.Area)
            {
                return;
            }

            Log.Info("[Province] area: {0} -> {1} ({2}: {3})", Provinces.GetAreaName(province.Area),
                Provinces.GetAreaName(area), province.Id, province.GetName());

            // Update value
            Provinces.ModifyArea(province, area);

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Area);

            // Update drawing to change the item color of the area combo box
            areaComboBox.Refresh();
        }

        /// <summary>
        ///     Treatment at the time of climate change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClimateComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (climateComboBox.SelectedIndex == -1)
            {
                return;
            }
            ClimateId climate = Provinces.Climates[climateComboBox.SelectedIndex];
            if (climate == province.Climate)
            {
                return;
            }

            Log.Info("[Province] climate: {0} -> {1} ({2}: {3})", Provinces.GetClimateName(province.Climate),
                Provinces.GetClimateName(climate), province.Id, province.GetName());

            // Update value
            province.Climate = climate;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Climate);

            // Update drawing to change the item color of the climate combo box
            climateComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when changing terrain
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTerrainComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (terrainComboBox.SelectedIndex == -1)
            {
                return;
            }
            TerrainId terrain = Provinces.Terrains[terrainComboBox.SelectedIndex];
            if (terrain == province.Terrain || (terrain == TerrainId.Plains && province.Terrain == TerrainId.Clear))
            {
                return;
            }

            Log.Info("[Province] terrain: {0} -> {1} ({2}: {3})", Provinces.GetTerrainName(province.Terrain),
                Provinces.GetTerrainName(terrain), province.Id, province.GetName());

            // Update value
            province.Terrain = terrain;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Terrain);

            // Update drawing to change the item color of the terrain combo box
            terrainComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when infrastructure changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInfraTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(infraTextBox.Text, out val))
            {
                infraTextBox.Text = DoubleHelper.ToString(province.Infrastructure);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, province.Infrastructure))
            {
                return;
            }

            Log.Info("[Province] infrastructure: {0} -> {1} ({2}: {3})", province.Infrastructure, val, province.Id,
                province.GetName());

            // Update value
            province.Infrastructure = val;

            // Update items in the province list view
            provinceListView.SelectedItems[0].SubItems[5].Text = DoubleHelper.ToString(province.Infrastructure);

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Infrastructure);

            // Change the font color
            infraTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     I C Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIcTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(icTextBox.Text, out val))
            {
                icTextBox.Text = DoubleHelper.ToString(province.Ic);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, province.Ic))
            {
                return;
            }

            Log.Info("[Province] ic: {0} -> {1} ({2}: {3})", province.Ic, val, province.Id, province.GetName());

            // Update value
            province.Ic = val;

            // Update items in the province list view
            provinceListView.SelectedItems[0].SubItems[6].Text = DoubleHelper.ToString(province.Ic);

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Ic);

            // Change the font color
            icTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing labor force
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnManpowerTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(manpowerTextBox.Text, out val))
            {
                manpowerTextBox.Text = DoubleHelper.ToString(province.Manpower);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, province.Manpower))
            {
                return;
            }

            Log.Info("[Province] manpower: {0} -> {1} ({2}: {3})", province.Manpower, val, province.Id,
                province.GetName());

            // Update value
            province.Manpower = val;

            // Update items in the province list view
            provinceListView.SelectedItems[0].SubItems[7].Text = DoubleHelper.ToString(province.Manpower);

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Manpower);

            // Change the font color
            manpowerTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing energy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEnergyTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(energyTextBox.Text, out val))
            {
                energyTextBox.Text = DoubleHelper.ToString(province.Energy);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, province.Energy))
            {
                return;
            }

            Log.Info("[Province] energy: {0} -> {1} ({2}: {3})", province.Energy, val, province.Id, province.GetName());

            // Update value
            province.Energy = val;

            // Update items in the province list view
            provinceListView.SelectedItems[0].SubItems[8].Text = DoubleHelper.ToString(province.Energy);

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Energy);

            // Change the font color
            energyTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing metal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMetalTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(metalTextBox.Text, out val))
            {
                metalTextBox.Text = DoubleHelper.ToString(province.Metal);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, province.Metal))
            {
                return;
            }

            Log.Info("[Province] metal: {0} -> {1} ({2}: {3})", province.Metal, val, province.Id, province.GetName());

            // Update value
            province.Metal = val;

            // Update items in the province list view
            provinceListView.SelectedItems[0].SubItems[9].Text = DoubleHelper.ToString(province.Metal);

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Metal);

            // Change the font color
            metalTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing rare resources
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRareMaterialsTextBoxValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(rareMaterialsTextBox.Text, out val))
            {
                rareMaterialsTextBox.Text = DoubleHelper.ToString(province.RareMaterials);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, province.RareMaterials))
            {
                return;
            }

            Log.Info("[Province] rare materials: {0} -> {1} ({2}: {3})", province.RareMaterials, val, province.Id,
                province.GetName());

            // Update value
            province.RareMaterials = val;

            // Update items in the province list view
            provinceListView.SelectedItems[0].SubItems[10].Text = DoubleHelper.ToString(province.RareMaterials);

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.RareMaterials);

            // Change the font color
            rareMaterialsTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing oil
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOilNumericUpDownValidated(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // If the changed character string cannot be converted to a numerical value, the value is returned.
            double val;
            if (!DoubleHelper.TryParse(oilTextBox.Text, out val))
            {
                oilTextBox.Text = DoubleHelper.ToString(province.Oil);
                return;
            }

            // Do nothing if the value does not change
            if (DoubleHelper.IsEqual(val, province.Oil))
            {
                return;
            }

            Log.Info("[Province] oil: {0} -> {1} ({2}: {3})", province.Oil, val, province.Id, province.GetName());

            // Update value
            province.Oil = val;

            // Update items in the province list view
            provinceListView.SelectedItems[0].SubItems[11].Text = DoubleHelper.ToString(province.Oil);

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Oil);

            // Change the font color
            oilTextBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the check status of the sandy beach check box changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBeachCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            bool flag = beachCheckBox.Checked;
            if (flag == province.Beaches)
            {
                return;
            }

            Log.Info("[Province] beach: {0} -> {1} ({2}: {3})", BoolHelper.ToString(province.Beaches),
                BoolHelper.ToString(flag), province.Id, province.GetName());

            // Update value
            province.Beaches = flag;

            // Update items in the province list view
            provinceListView.SelectedItems[0].SubItems[4].Text = province.Beaches ? Resources.Yes : Resources.No;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.Beaches);

            // Change the font color
            beachCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     On the sandy beach X Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBeachXNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int x = (int) beachXNumericUpDown.Value;
            if (x == province.BeachXPos)
            {
                return;
            }

            Log.Info("[Province] beach position x: {0} -> {1} ({2}: {3})", province.BeachXPos, x, province.Id,
                province.GetName());

            // Update value
            province.BeachXPos = x;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.BeachXPos);

            // Change the font color
            beachXNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     On the sandy beach Y Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBeachYNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int y = (int) beachYNumericUpDown.Value;
            if (y == province.BeachYPos)
            {
                return;
            }

            Log.Info("[Province] beach position y: {0} -> {1} ({2}: {3})", province.BeachYPos, y, province.Id,
                province.GetName());

            // Update value
            province.BeachYPos = y;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.BeachYPos);

            // Change the font color
            beachYNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the icon of the sandy beach
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBeachIconNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int icon = (int) beachIconNumericUpDown.Value;
            if (icon == province.BeachIcon)
            {
                return;
            }

            Log.Info("[Province] beach icon: {0} -> {1} ({2}: {3})", province.BeachIcon, icon, province.Id,
                province.GetName());

            // Update value
            province.BeachIcon = icon;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.BeachIcon);

            // Change the font color
            beachIconNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when the check status of the port check box changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPortCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            bool flag = portCheckBox.Checked;
            if (flag == province.PortAllowed)
            {
                return;
            }

            Log.Info("[Province] port allowed: {0} -> {1} ({2}: {3})", BoolHelper.ToString(province.PortAllowed),
                BoolHelper.ToString(flag), province.Id, province.GetName());

            // Update value
            province.PortAllowed = flag;

            // Update items in the province list view
            provinceListView.SelectedItems[0].SubItems[3].Text = province.PortAllowed ? Resources.Yes : Resources.No;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.PortAllowed);

            // Change the font color
            portCheckBox.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Of the harbor X Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPortXNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int x = (int) portXNumericUpDown.Value;
            if (x == province.PortXPos)
            {
                return;
            }

            Log.Info("[Province] port position x: {0} -> {1} ({2}: {3})", province.PortXPos, x, province.Id,
                province.GetName());

            // Update value
            province.PortXPos = x;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.PortXPos);

            // Change the font color
            portXNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Of the harbor Y Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPortYNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int y = (int) portYNumericUpDown.Value;
            if (y == province.PortYPos)
            {
                return;
            }

            Log.Info("[Province] port position y: {0} -> {1} ({2}: {3})", province.PortYPos, y, province.Id,
                province.GetName());

            // Update value
            province.PortYPos = y;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.PortYPos);

            // Change the font color
            portYNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Processing when changing the sea area of the port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPortSeaZoneNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int seaZone = (int) portSeaZoneNumericUpDown.Value;
            if (seaZone == province.PortSeaZone)
            {
                return;
            }

            Province oldProv = Provinces.Items.Find(prov => prov.Id == province.PortSeaZone);
            Province newProv = Provinces.Items.Find(prov => prov.Id == seaZone);

            Log.Info("[Province] port sea zone: {0} [{1}] -> {2} [{3}] ({4}: {5})", province.PortSeaZone,
                oldProv != null ? oldProv.GetName() : "", seaZone, newProv != null ? newProv.GetName() : "",
                province.Id, province.GetName());

            // Update value
            province.PortSeaZone = seaZone;

            // Update the item of the sea area combo box of the port
            if (Provinces.SeaZones.Contains(seaZone))
            {
                portSeaZoneComboBox.SelectedIndex = Provinces.SeaZones.IndexOf(seaZone);
            }
            else
            {
                portSeaZoneComboBox.SelectedIndex = -1;
                if (seaZone > 0)
                {
                    Province seaProvince = Provinces.Items.Find(prov => prov.Id == seaZone);
                    if (seaProvince != null)
                    {
                        portSeaZoneComboBox.Text = seaProvince.GetName();
                    }
                }
            }

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.PortSeaZone);

            // Change the font color
            portSeaZoneNumericUpDown.ForeColor = Color.Red;

            // Update drawing to change the item color of the sea area combo box of the port
            portSeaZoneComboBox.Refresh();
        }

        /// <summary>
        ///     Processing when changing the selection item of the sea area combo box of the port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPortSeaZoneComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            if (portSeaZoneComboBox.SelectedIndex == -1)
            {
                return;
            }
            int seaZone = Provinces.SeaZones[portSeaZoneComboBox.SelectedIndex];
            if (seaZone == province.PortSeaZone)
            {
                return;
            }

            // Update the value of the sea area of the port
            portSeaZoneNumericUpDown.Value = seaZone;
        }

        /// <summary>
        ///     Of city X Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCityXNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int x = (int) cityXNumericUpDown.Value;
            if (x == province.CityXPos)
            {
                return;
            }

            Log.Info("[Province] city position x: {0} -> {1} ({2}: {3})", province.CityXPos, x, province.Id,
                province.GetName());

            // Update value
            province.CityXPos = x;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.CityXPos);

            // Change the font color
            cityXNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Of city Y Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCityYNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int y = (int) cityYNumericUpDown.Value;
            if (y == province.CityYPos)
            {
                return;
            }

            Log.Info("[Province] city position y: {0} -> {1} ({2}: {3})", province.CityYPos, y, province.Id,
                province.GetName());

            // Update value
            province.CityYPos = y;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.CityYPos);

            // Change the font color
            cityYNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Fortress X Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFortXNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int x = (int) fortXNumericUpDown.Value;
            if (x == province.FortXPos)
            {
                return;
            }

            Log.Info("[Province] fort position x: {0} -> {1} ({2}: {3})", province.FortXPos, x, province.Id,
                province.GetName());

            // Update value
            province.FortXPos = x;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.FortXPos);

            // Change the font color
            fortXNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Fortress Y Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFortYNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int y = (int) fortYNumericUpDown.Value;
            if (y == province.FortYPos)
            {
                return;
            }

            Log.Info("[Province] fort position y: {0} -> {1} ({2}: {3})", province.FortYPos, y, province.Id,
                province.GetName());

            // Update value
            province.FortYPos = y;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.FortYPos);

            // Change the font color
            fortYNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-aircraft gun X Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAaXNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int x = (int) aaXNumericUpDown.Value;
            if (x == province.AaXPos)
            {
                return;
            }

            Log.Info("[Province] aa position x: {0} -> {1} ({2}: {3})", province.AaXPos, x, province.Id,
                province.GetName());

            // Update value
            province.AaXPos = x;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.AaXPos);

            // Change the font color
            aaXNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Anti-aircraft gun Y Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAaYNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int y = (int) aaYNumericUpDown.Value;
            if (y == province.AaYPos)
            {
                return;
            }

            Log.Info("[Province] aa position y: {0} -> {1} ({2}: {3})", province.AaYPos, y, province.Id,
                province.GetName());

            // Update value
            province.AaYPos = y;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.AaYPos);

            // Change the font color
            aaYNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Of the army X Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnArmyXNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int x = (int) armyXNumericUpDown.Value;
            if (x == province.ArmyXPos)
            {
                return;
            }

            Log.Info("[Province] army position x: {0} -> {1} ({2}: {3})", province.ArmyXPos, x, province.Id,
                province.GetName());

            // Update value
            province.ArmyXPos = x;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.ArmyXPos);

            // Change the font color
            armyXNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Of the army Y Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnArmyYNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int y = (int) armyYNumericUpDown.Value;
            if (y == province.ArmyYPos)
            {
                return;
            }

            Log.Info("[Province] army position y: {0} -> {1} ({2}: {3})", province.ArmyYPos, y, province.Id,
                province.GetName());

            // Update value
            province.ArmyYPos = y;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.ArmyYPos);

            // Change the font color
            armyYNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Of the counter X Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCounterXNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int x = (int) counterXNumericUpDown.Value;
            if (x == province.CounterXPos)
            {
                return;
            }

            Log.Info("[Province] counter position x: {0} -> {1} ({2}: {3})", province.CounterXPos, x, province.Id,
                province.GetName());

            // Update value
            province.CounterXPos = x;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.CounterXPos);

            // Change the font color
            counterXNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     Of the counter Y Processing when changing coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCounterYNumericUpDownValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int y = (int) counterYNumericUpDown.Value;
            if (y == province.CounterYPos)
            {
                return;
            }

            Log.Info("[Province] counter position y: {0} -> {1} ({2}: {3})", province.CounterYPos, y, province.Id,
                province.GetName());

            // Update value
            province.CounterYPos = y;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.CounterYPos);

            // Change the font color
            counterYNumericUpDown.ForeColor = Color.Red;
        }

        /// <summary>
        ///     fill X Coordinate 1 Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFillXNumericUpDown1ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int x = (int) fillXNumericUpDown1.Value;
            if (x == province.FillCoordX1)
            {
                return;
            }

            Log.Info("[Province] fill coord position x1: {0} -> {1} ({2}: {3})", province.FillCoordX1, x, province.Id,
                province.GetName());

            // Update value
            province.FillCoordX1 = x;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.FillCoordX1);

            // Change the font color
            fillXNumericUpDown1.ForeColor = Color.Red;
        }

        /// <summary>
        ///     fill Y Coordinate 1 Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFillYNumericUpDown1ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int y = (int) fillYNumericUpDown1.Value;
            if (y == province.FillCoordY1)
            {
                return;
            }

            Log.Info("[Province] fill coord position y1: {0} -> {1} ({2}: {3})", province.FillCoordY1, y, province.Id,
                province.GetName());

            // Update value
            province.FillCoordY1 = y;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.FillCoordY1);

            // Change the font color
            fillYNumericUpDown1.ForeColor = Color.Red;
        }

        /// <summary>
        ///     fill X Coordinate 2 Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFillXNumericUpDown2ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int x = (int) fillXNumericUpDown2.Value;
            if (x == province.FillCoordX2)
            {
                return;
            }

            Log.Info("[Province] fill coord position x2: {0} -> {1} ({2}: {3})", province.FillCoordX2, x, province.Id,
                province.GetName());

            // Update value
            province.FillCoordX2 = x;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.FillCoordX2);

            // Change the font color
            fillXNumericUpDown2.ForeColor = Color.Red;
        }

        /// <summary>
        ///     fill YCoordinate 2 Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFillYNumericUpDown2ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int y = (int) fillYNumericUpDown2.Value;
            if (y == province.FillCoordY2)
            {
                return;
            }

            Log.Info("[Province] fill coord position y2: {0} -> {1} ({2}: {3})", province.FillCoordY2, y, province.Id,
                province.GetName());

            // Update value
            province.FillCoordY2 = y;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.FillCoordY2);

            // Change the font color
            fillYNumericUpDown2.ForeColor = Color.Red;
        }

        /// <summary>
        ///     fill X Coordinate 3 Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFillXNumericUpDown3ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int x = (int) fillXNumericUpDown3.Value;
            if (x == province.FillCoordX3)
            {
                return;
            }

            Log.Info("[Province] fill coord position x3: {0} -> {1} ({2}: {3})", province.FillCoordX3, x, province.Id,
                province.GetName());

            // Update value
            province.FillCoordX3 = x;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.FillCoordX3);

            // Change the font color
            fillXNumericUpDown3.ForeColor = Color.Red;
        }

        /// <summary>
        ///     fill Y Coordinate 3 Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFillYNumericUpDown3ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int y = (int) fillYNumericUpDown3.Value;
            if (y == province.FillCoordY3)
            {
                return;
            }

            Log.Info("[Province] fill coord position y3: {0} -> {1} ({2}: {3})", province.FillCoordY3, y, province.Id,
                province.GetName());

            // Update value
            province.FillCoordY3 = y;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.FillCoordY3);

            // Change the font color
            fillYNumericUpDown3.ForeColor = Color.Red;
        }

        /// <summary>
        ///     fill X Coordinate Four Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFillXNumericUpDown4ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int x = (int) fillXNumericUpDown4.Value;
            if (x == province.FillCoordX4)
            {
                return;
            }

            Log.Info("[Province] fill coord position x4: {0} -> {1} ({2}: {3})", province.FillCoordX4, x, province.Id,
                province.GetName());

            // Update value
            province.FillCoordX4 = x;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.FillCoordX4);

            // Change the font color
            fillXNumericUpDown4.ForeColor = Color.Red;
        }

        /// <summary>
        ///     fill Y Coordinate Four Processing at the time of change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFillYNumericUpDown4ValueChanged(object sender, EventArgs e)
        {
            // Do nothing if there is no selection
            Province province = GetSelectedProvince();
            if (province == null)
            {
                return;
            }

            // Do nothing if the value does not change
            int y = (int) fillYNumericUpDown4.Value;
            if (y == province.FillCoordY4)
            {
                return;
            }

            Log.Info("[Province] fill coord position y4: {0} -> {1} ({2}: {3})", province.FillCoordY4, y, province.Id,
                province.GetName());

            // Update value
            province.FillCoordY4 = y;

            // Set the edited flag
            Provinces.SetDirty();
            province.SetDirty(ProvinceItemId.FillCoordY4);

            // Change the font color
            fillYNumericUpDown4.ForeColor = Color.Red;
        }

        #endregion
    }
}
