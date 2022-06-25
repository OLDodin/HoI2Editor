using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Models;
using HoI2Editor.Properties;

namespace HoI2Editor.Controllers
{
    /// <summary>
    ///     Unit tree controller class
    /// </summary>
    public class UnitTreeController
    {
        #region Public properties

        /// <summary>
        ///     Selected country
        /// </summary>
        public Country Country
        {
            get { return _country; }
            set
            {
                _country = value;
                Update();
            }
        }

        #endregion

        #region Internal field

        /// <summary>
        ///     Unit tree view
        /// </summary>
        private readonly TreeView _treeView;

        /// <summary>
        ///     Selected country
        /// </summary>
        private Country _country;

        #endregion

        #region Internal constant

        /// <summary>
        ///     Types of special nodes
        /// </summary>
        public enum NodeType
        {
            Land,
            Naval,
            Air,
            Boarding,
            UndeployedLand,
            UndeployedNaval,
            UndeployedAir,
            DivisionDevelopment,
            ConvoyDevelopment,
            ProvinceDevelopment
        }

        #endregion

        #region Public event

        /// <summary>
        ///     Processing when selecting a node
        /// </summary>
        public event EventHandler<UnitTreeViewEventArgs> AfterSelect;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="treeView">Unit tree view</param>
        public UnitTreeController(TreeView treeView)
        {
            _treeView = treeView;

            Init();
        }

        /// <summary>
        ///     Initialization process
        /// </summary>
        private void Init()
        {
            _treeView.AfterSelect += OnUnitTreeViewAfterSelect;
        }

        #endregion

        #region Tree view

        /// <summary>
        ///     Update the unit tree
        /// </summary>
        public void Update()
        {
            _treeView.BeginUpdate();
            _treeView.Nodes.Clear();

            if (_country == Country.None)
            {
                _treeView.EndUpdate();
                return;
            }

            CountrySettings settings = Scenarios.GetCountrySettings(_country);

            // Army unit
            TreeNode node = new TreeNode(Resources.UnitTreeLand) { Tag = NodeType.Land };
            if (settings != null)
            {
                foreach (Unit unit in settings.LandUnits)
                {
                    node.Nodes.Add(CreateLandUnitNode(unit));
                }
            }
            _treeView.Nodes.Add(node);

            // Navy unit
            node = new TreeNode(Resources.UnitTreeNaval) { Tag = NodeType.Naval };
            if (settings != null)
            {
                foreach (Unit unit in settings.NavalUnits)
                {
                    node.Nodes.Add(CreateNavalUnitNode(unit));
                }
            }
            _treeView.Nodes.Add(node);

            // Air Force Unit
            node = new TreeNode(Resources.UnitTreeAir) { Tag = NodeType.Air };
            if (settings != null)
            {
                foreach (Unit unit in settings.AirUnits)
                {
                    node.Nodes.Add(CreateAirUnitNode(unit));
                }
            }
            _treeView.Nodes.Add(node);

            // Undeployed Army Division
            node = new TreeNode(Resources.UnitTreeUndeployedLand) { Tag = NodeType.UndeployedLand };
            if (settings != null)
            {
                foreach (Division division in settings.LandDivisions)
                {
                    node.Nodes.Add(CreateLandDivisionNode(division));
                }
            }
            _treeView.Nodes.Add(node);

            // Undeployed Navy Division
            node = new TreeNode(Resources.UnitTreeUndeployedNaval) { Tag = NodeType.UndeployedNaval };
            if (settings != null)
            {
                foreach (Division division in settings.NavalDivisions)
                {
                    node.Nodes.Add(CreateNavalDivisionNode(division));
                }
            }
            _treeView.Nodes.Add(node);

            // Undeployed Luftwaffe Division
            node = new TreeNode(Resources.UnitTreeUndeployedAir) { Tag = NodeType.UndeployedAir };
            if (settings != null)
            {
                foreach (Division division in settings.AirDivisions)
                {
                    node.Nodes.Add(CreateAirDivisionNode(division));
                }
            }
            _treeView.Nodes.Add(node);

            _treeView.EndUpdate();
        }

        /// <summary>
        ///     Update the label of the selected unit node
        /// </summary>
        public void UpdateUnitNodeLabel(string name)
        {
            TreeNode node = GetSelectedUnitNode();
            if (node == null)
            {
                return;
            }

            node.Text = name;
        }

        /// <summary>
        ///     Update the label of the selected division node
        /// </summary>
        public void UpdateDivisionNodeLabel(string name)
        {
            TreeNode node = GetSelectedDivisionNode();
            if (node == null)
            {
                return;
            }

            node.Text = name;
        }

        /// <summary>
        ///     Create a node for the unit
        /// </summary>
        /// <param name="unit">unit</param>
        /// <returns>Tree node</returns>
        private static TreeNode CreateUnitNode(Unit unit)
        {
            TreeNode node = new TreeNode(unit.Name) { Tag = unit };

            // Division
            foreach (Division division in unit.Divisions)
            {
                node.Nodes.Add(CreateDivisionNode(division));
            }

            // On-board unit
            if (unit.Branch == Branch.Navy || unit.Branch == Branch.Airforce)
            {
                TreeNode boarding = new TreeNode(Resources.UnitTreeBoarding) { Tag = NodeType.Boarding };
                foreach (Unit landUnit in unit.LandUnits)
                {
                    boarding.Nodes.Add(CreateUnitNode(landUnit));
                }
                node.Nodes.Add(boarding);
            }

            return node;
        }

        /// <summary>
        ///     Create a division node
        /// </summary>
        /// <param name="division">Division</param>
        /// <returns>Tree node</returns>
        private static TreeNode CreateDivisionNode(Division division)
        {
            return new TreeNode(division.Name) { Tag = division };
        }

        /// <summary>
        ///     Create a node for an army unit
        /// </summary>
        /// <param name="unit">unit</param>
        /// <returns>Tree node</returns>
        private static TreeNode CreateLandUnitNode(Unit unit)
        {
            TreeNode node = new TreeNode(unit.Name) { Tag = unit };

            // Army division
            foreach (Division division in unit.Divisions)
            {
                node.Nodes.Add(CreateLandDivisionNode(division));
            }

            return node;
        }

        /// <summary>
        ///     Create a node for a navy unit
        /// </summary>
        /// <param name="unit">unit</param>
        /// <returns>Tree node</returns>
        private static TreeNode CreateNavalUnitNode(Unit unit)
        {
            TreeNode node = new TreeNode(unit.Name) { Tag = unit };

            // Navy Division
            foreach (Division division in unit.Divisions)
            {
                node.Nodes.Add(CreateNavalDivisionNode(division));
            }

            // On-board unit
            TreeNode boarding = new TreeNode(Resources.UnitTreeBoarding) { Tag = NodeType.Boarding };
            foreach (Unit landUnit in unit.LandUnits)
            {
                boarding.Nodes.Add(CreateLandUnitNode(landUnit));
            }
            node.Nodes.Add(boarding);

            return node;
        }

        /// <summary>
        ///     Create a node for an air force unit
        /// </summary>
        /// <param name="unit">unit</param>
        /// <returns>Tree node</returns>
        private static TreeNode CreateAirUnitNode(Unit unit)
        {
            TreeNode node = new TreeNode(unit.Name) { Tag = unit };

            // Air Force Division
            foreach (Division division in unit.Divisions)
            {
                node.Nodes.Add(CreateAirDivisionNode(division));
            }

            // On-board unit
            TreeNode boarding = new TreeNode(Resources.UnitTreeBoarding) { Tag = NodeType.Boarding };
            foreach (Unit landUnit in unit.LandUnits)
            {
                boarding.Nodes.Add(CreateLandUnitNode(landUnit));
            }
            node.Nodes.Add(boarding);

            return node;
        }

        /// <summary>
        ///     Create an Army Division node
        /// </summary>
        /// <param name="division">Division</param>
        /// <returns>Tree node</returns>
        private static TreeNode CreateLandDivisionNode(Division division)
        {
            return new TreeNode(division.Name) { Tag = division };
        }

        /// <summary>
        ///     Create a node for the Navy Division
        /// </summary>
        /// <param name="division">Division</param>
        /// <returns>Tree node</returns>
        private static TreeNode CreateNavalDivisionNode(Division division)
        {
            return new TreeNode(division.Name) { Tag = division };
        }

        /// <summary>
        ///     Create a node for the Air Force Division
        /// </summary>
        /// <param name="division">Division</param>
        /// <returns>Tree node</returns>
        private static TreeNode CreateAirDivisionNode(Division division)
        {
            return new TreeNode(division.Name) { Tag = division };
        }

        #endregion

        #region Tree operation

        /// <summary>
        ///     Add a unit
        /// </summary>
        public void AddUnit()
        {
            CountrySettings settings = Scenarios.GetCountrySettings(_country) ??
                                       Scenarios.CreateCountrySettings(_country);
            TreeNode node = _treeView.SelectedNode;
            TreeNode parent = node.Parent;

            // Army / / Navy / / Air force unit root node
            if (parent == null)
            {
                switch ((NodeType) node.Tag)
                {
                    case NodeType.Land:
                        AddUnit(Branch.Army, node, settings.LandUnits, settings);
                        break;

                    case NodeType.Naval:
                        AddUnit(Branch.Navy, node, settings.NavalUnits, settings);
                        break;

                    case NodeType.Air:
                        AddUnit(Branch.Airforce, node, settings.AirUnits, settings);
                        break;
                }
                return;
            }

            // Onboard unit root node
            Unit transport = parent.Tag as Unit;
            if (transport != null)
            {
                AddUnit(Branch.Army, node, transport.LandUnits, settings);
                return;
            }

            // Army / / Navy / / Air Force / / On-board unit
            int index = parent.Nodes.IndexOf(node) + 1;
            switch ((NodeType) parent.Tag)
            {
                case NodeType.Land:
                    AddUnit(index, Branch.Army, parent, settings.LandUnits, settings);
                    break;

                case NodeType.Naval:
                    AddUnit(index, Branch.Navy, parent, settings.NavalUnits, settings);
                    break;

                case NodeType.Air:
                    AddUnit(index, Branch.Airforce, parent, settings.AirUnits, settings);
                    break;

                case NodeType.Boarding:
                    transport = (Unit) parent.Parent.Tag;
                    AddUnit(index, Branch.Army, parent, transport.LandUnits, settings);
                    break;
            }
        }

        /// <summary>
        ///     Add a unit
        /// </summary>
        /// <param name="branch">Military department</param>
        /// <param name="parent">Parent node</param>
        /// <param name="units">Unit list</param>
        /// <param name="settings">National setting</param>
        private void AddUnit(Branch branch, TreeNode parent, List<Unit> units, CountrySettings settings)
        {
            // Create a unit
            Unit unit = new Unit { Id = settings.GetNewUnitTypeId(), Branch = branch };
            unit.SetDirtyAll();

            // Initialize the position of the unit
            InitUnitLocation(unit, settings);

            // Add a tree node
            TreeNode node = CreateUnitNode(unit);
            node.Text = Resources.UnitTreeNewUnit;
            parent.Nodes.Add(node);

            // Add to unit list
            units.Add(unit);

            // Set the edited flag
            settings.SetDirty();
            Scenarios.SetDirty();

            // Select the added node
            _treeView.SelectedNode = node;
        }

        /// <summary>
        ///     Add a unit
        /// </summary>
        /// <param name="index">Index to add to</param>
        /// <param name="branch">Military department</param>
        /// <param name="parent">Parent node</param>
        /// <param name="units">Unit list</param>
        /// <param name="settings">National setting</param>
        private void AddUnit(int index, Branch branch, TreeNode parent, List<Unit> units, CountrySettings settings)
        {
            // Create a unit
            Unit unit = new Unit { Id = settings.GetNewUnitTypeId(), Branch = branch };
            unit.SetDirtyAll();

            // Initialize the position of the unit
            InitUnitLocation(unit, settings);

            // Add a tree node
            TreeNode node = CreateUnitNode(unit);
            node.Text = Resources.UnitTreeNewUnit;
            parent.Nodes.Insert(index, node);

            // Add to unit list
            units.Insert(index, unit);

            // Set the edited flag
            settings.SetDirty();
            Scenarios.SetDirty();

            // Select the added node
            _treeView.SelectedNode = node;
        }

        /// <summary>
        ///     Initialize the position of the unit
        /// </summary>
        /// <param name="unit">unit</param>
        /// <param name="settings">National setting</param>
        private static void InitUnitLocation(Unit unit, CountrySettings settings)
        {
            ProvinceSettings capitalSettings;

            switch (unit.Branch)
            {
                case Branch.Army:
                    unit.Location = settings.Capital;
                    break;

                case Branch.Navy:
                    capitalSettings = Scenarios.GetProvinceSettings(settings.Capital);
                    if (capitalSettings?.NavalBase != null)
                    {
                        unit.Location = settings.Capital;
                        unit.Base = settings.Capital;
                    }
                    else
                    {
                        foreach (ProvinceSettings ps in settings.ControlledProvinces
                            .Select(Scenarios.GetProvinceSettings)
                            .Where(ps => ps?.NavalBase != null))
                        {
                            unit.Location = ps.Id;
                            unit.Base = ps.Id;
                            break;
                        }
                    }
                    break;

                case Branch.Airforce:
                    capitalSettings = Scenarios.GetProvinceSettings(settings.Capital);
                    if (capitalSettings?.AirBase != null)
                    {
                        unit.Location = settings.Capital;
                        unit.Base = settings.Capital;
                    }
                    else
                    {
                        foreach (ProvinceSettings ps in settings.ControlledProvinces
                            .Select(Scenarios.GetProvinceSettings)
                            .Where(ps => ps?.AirBase != null))
                        {
                            unit.Location = ps.Id;
                            unit.Base = ps.Id;
                            break;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        ///     Add a division
        /// </summary>
        public void AddDivision()
        {
            CountrySettings settings = Scenarios.GetCountrySettings(_country) ??
                                       Scenarios.CreateCountrySettings(_country);
            TreeNode node = _treeView.SelectedNode;
            TreeNode parent = node.Parent;

            // Set the edited flag
            settings.SetDirty();
            Scenarios.SetDirty();

            // Undeployed Army / / Navy / / Air Force Division Root Node
            if (parent == null)
            {
                switch ((NodeType) node.Tag)
                {
                    case NodeType.UndeployedLand:
                        AddDivision(Branch.Army, node, settings.LandDivisions, settings);
                        break;

                    case NodeType.UndeployedNaval:
                        AddDivision(Branch.Navy, node, settings.NavalDivisions, settings);
                        break;

                    case NodeType.UndeployedAir:
                        AddDivision(Branch.Airforce, node, settings.AirDivisions, settings);
                        break;
                }
                return;
            }

            // Army / / Navy / / Air Force / / On-board unit
            Unit unit = node.Tag as Unit;
            if (unit != null)
            {
                AddDivision(unit.Branch, node, unit.Divisions, settings);
                return;
            }

            int index = parent.Nodes.IndexOf(node) + 1;

            // Army / / Navy / / Air Force / / Onboard division
            unit = parent.Tag as Unit;
            if (unit != null)
            {
                AddDivision(index, unit.Branch, parent, unit.Divisions, settings);
                return;
            }

            // Undeployed Army / / Navy / / Luftwaffe Division
            switch ((NodeType) parent.Tag)
            {
                case NodeType.UndeployedLand:
                    AddDivision(index, Branch.Army, parent, settings.LandDivisions, settings);
                    break;

                case NodeType.UndeployedNaval:
                    AddDivision(index, Branch.Navy, parent, settings.NavalDivisions, settings);
                    break;

                case NodeType.UndeployedAir:
                    AddDivision(index, Branch.Airforce, parent, settings.AirDivisions, settings);
                    break;
            }
        }

        /// <summary>
        ///     Add a division
        /// </summary>
        /// <param name="branch">Military department</param>
        /// <param name="parent">Parent node</param>
        /// <param name="divisions">Division list</param>
        /// <param name="settings">National setting</param>
        private void AddDivision(Branch branch, TreeNode parent, List<Division> divisions, CountrySettings settings)
        {
            // Create a division
            Division division = new Division { Id = settings.GetNewUnitTypeId(), Branch = branch };
            division.SetDirtyAll();
            switch (branch)
            {
                case Branch.Army:
                    division.Type = UnitType.Infantry;
                    break;

                case Branch.Navy:
                    division.Type = UnitType.BattleShip;
                    break;

                case Branch.Airforce:
                    division.Type = UnitType.MultiRole;
                    break;
            }

            // Add a tree node
            TreeNode node = CreateDivisionNode(division);
            node.Text = Resources.UnitTreeNewDivision;
            int index = parent.Nodes.Count;
            if (parent.Tag is Unit && (branch == Branch.Navy || branch == Branch.Airforce))
            {
                index--;
            }
            parent.Nodes.Insert(index, node);

            // Add to division list
            divisions.Add(division);

            // Select the added node
            _treeView.SelectedNode = node;
        }

        /// <summary>
        ///     Add a unit
        /// </summary>
        /// <param name="index">Index to add to</param>
        /// <param name="branch">Military department</param>
        /// <param name="parent">Parent node</param>
        /// <param name="divisions">Division list</param>
        /// <param name="settings">National setting</param>
        private void AddDivision(int index, Branch branch, TreeNode parent, List<Division> divisions,
            CountrySettings settings)
        {
            // Create a division
            Division division = new Division { Id = settings.GetNewUnitTypeId(), Branch = branch };
            division.SetDirtyAll();
            switch (branch)
            {
                case Branch.Army:
                    division.Type = UnitType.Infantry;
                    break;

                case Branch.Navy:
                    division.Type = UnitType.BattleShip;
                    break;

                case Branch.Airforce:
                    division.Type = UnitType.MultiRole;
                    break;
            }

            // Add a tree node
            TreeNode node = CreateDivisionNode(division);
            node.Text = Resources.UnitTreeNewDivision;
            parent.Nodes.Insert(index, node);

            // Add to division list
            divisions.Insert(index, division);

            // Select the added node
            _treeView.SelectedNode = node;
        }

        /// <summary>
        ///     unit / / Duplicate the division
        /// </summary>
        public void Clone()
        {
            TreeNode node = _treeView.SelectedNode;

            Unit unit = node.Tag as Unit;
            if (unit != null)
            {
                CloneUnit(unit);
            }

            Division division = node.Tag as Division;
            if (division != null)
            {
                CloneDivision(division);
            }
        }

        /// <summary>
        ///     Duplicate the unit
        /// </summary>
        /// <param name="original">Unit to be duplicated</param>
        private void CloneUnit(Unit original)
        {
            CountrySettings settings = Scenarios.GetCountrySettings(_country) ??
                                       Scenarios.CreateCountrySettings(_country);
            Unit unit = new Unit(original);
            unit.SetDirtyAll();
            TreeNode selected = _treeView.SelectedNode;
            TreeNode parent = selected.Parent;
            TreeNode node;
            int index = parent.Nodes.IndexOf(selected) + 1;

            // Set the edited flag
            settings.SetDirty();
            Scenarios.SetDirty();

            // Army / / Navy / / Air Force / / On-board unit
            switch ((NodeType) parent.Tag)
            {
                case NodeType.Land:
                    node = CreateLandUnitNode(unit);
                    if (string.IsNullOrEmpty(unit.Name))
                    {
                        node.Text = Resources.UnitTreeNewUnit;
                    }
                    parent.Nodes.Insert(index, node);
                    settings.LandUnits.Insert(index, unit);
                    _treeView.SelectedNode = node;
                    break;

                case NodeType.Naval:
                    node = CreateNavalUnitNode(unit);
                    if (string.IsNullOrEmpty(unit.Name))
                    {
                        node.Text = Resources.UnitTreeNewUnit;
                    }
                    parent.Nodes.Insert(index, node);
                    settings.NavalUnits.Insert(index, unit);
                    _treeView.SelectedNode = node;
                    break;

                case NodeType.Air:
                    node = CreateAirUnitNode(unit);
                    if (string.IsNullOrEmpty(unit.Name))
                    {
                        node.Text = Resources.UnitTreeNewUnit;
                    }
                    parent.Nodes.Insert(index, node);
                    settings.AirUnits.Insert(index, unit);
                    _treeView.SelectedNode = node;
                    break;

                case NodeType.Boarding:
                    node = CreateLandUnitNode(unit);
                    if (string.IsNullOrEmpty(unit.Name))
                    {
                        node.Text = Resources.UnitTreeNewUnit;
                    }
                    parent.Nodes.Insert(index, node);
                    Unit transport = (Unit) parent.Parent.Tag;
                    transport.LandUnits.Insert(index, unit);
                    _treeView.SelectedNode = node;
                    break;
            }
        }

        /// <summary>
        ///     Duplicate the division
        /// </summary>
        /// <param name="original">Division to be duplicated</param>
        private void CloneDivision(Division original)
        {
            Division division = new Division(original);
            division.SetDirtyAll();
            TreeNode selected = _treeView.SelectedNode;
            TreeNode parent = selected.Parent;
            TreeNode node;
            int index = parent.Nodes.IndexOf(selected) + 1;

            CountrySettings settings = Scenarios.GetCountrySettings(_country) ??
                                       Scenarios.CreateCountrySettings(_country);

            // Set the edited flag
            settings.SetDirty();
            Scenarios.SetDirty();

            // Army / / Navy / / Air Force / / Onboard division
            Unit unit = parent.Tag as Unit;
            if (unit != null)
            {
                switch ((NodeType) parent.Parent.Tag)
                {
                    case NodeType.Land:
                    case NodeType.Boarding:
                        node = CreateLandDivisionNode(division);
                        if (string.IsNullOrEmpty(division.Name))
                        {
                            node.Text = Resources.UnitTreeNewDivision;
                        }
                        parent.Nodes.Insert(index, node);
                        unit.Divisions.Insert(index, division);
                        _treeView.SelectedNode = node;
                        break;

                    case NodeType.Naval:
                        node = CreateNavalDivisionNode(division);
                        if (string.IsNullOrEmpty(division.Name))
                        {
                            node.Text = Resources.UnitTreeNewDivision;
                        }
                        parent.Nodes.Insert(index, node);
                        unit.Divisions.Insert(index, division);
                        _treeView.SelectedNode = node;
                        break;

                    case NodeType.Air:
                        node = CreateAirDivisionNode(division);
                        if (string.IsNullOrEmpty(division.Name))
                        {
                            node.Text = Resources.UnitTreeNewDivision;
                        }
                        parent.Nodes.Insert(index, node);
                        unit.Divisions.Insert(index, division);
                        _treeView.SelectedNode = node;
                        break;
                }
                return;
            }

            // Undeployed Army / / Navy / / Luftwaffe Division
            switch ((NodeType) parent.Tag)
            {
                case NodeType.UndeployedLand:
                    node = CreateLandDivisionNode(division);
                    if (string.IsNullOrEmpty(division.Name))
                    {
                        node.Text = Resources.UnitTreeNewDivision;
                    }
                    parent.Nodes.Insert(index, node);
                    settings.LandDivisions.Insert(index, division);
                    _treeView.SelectedNode = node;
                    break;

                case NodeType.UndeployedNaval:
                    node = CreateNavalDivisionNode(division);
                    if (string.IsNullOrEmpty(division.Name))
                    {
                        node.Text = Resources.UnitTreeNewDivision;
                    }
                    parent.Nodes.Insert(index, node);
                    settings.NavalDivisions.Insert(index, division);
                    _treeView.SelectedNode = node;
                    break;

                case NodeType.UndeployedAir:
                    node = CreateAirDivisionNode(division);
                    if (string.IsNullOrEmpty(division.Name))
                    {
                        node.Text = Resources.UnitTreeNewDivision;
                    }
                    parent.Nodes.Insert(index, node);
                    settings.AirDivisions.Insert(index, division);
                    _treeView.SelectedNode = node;
                    break;
            }
        }

        /// <summary>
        ///     unit / / Delete the division
        /// </summary>
        public void Remove()
        {
            TreeNode node = _treeView.SelectedNode;

            Unit unit = node.Tag as Unit;
            if (unit != null)
            {
                // Delete a unit
                RemoveUnit(unit);

                // type When id idDelete the pair of
                unit.RemoveTypeId();
            }

            Division division = node.Tag as Division;
            if (division != null)
            {
                // Delete the division
                RemoveDivision(division);

                // type When id idDelete the pair of
                division.RemoveTypeId();
            }
        }

        /// <summary>
        ///     Delete a unit
        /// </summary>
        /// <param name="unit">Unit to be deleted</param>
        private void RemoveUnit(Unit unit)
        {
            TreeNode node = _treeView.SelectedNode;
            TreeNode parent = node.Parent;

            parent.Nodes.Remove(node);

            CountrySettings settings = Scenarios.GetCountrySettings(_country) ??
                                       Scenarios.CreateCountrySettings(_country);

            // Set the edited flag
            settings.SetDirty();
            Scenarios.SetDirty();

            // Army / / Navy / / Air Force / / On-board unit
            switch ((NodeType) parent.Tag)
            {
                case NodeType.Land:
                    settings.LandUnits.Remove(unit);
                    break;

                case NodeType.Naval:
                    settings.NavalUnits.Remove(unit);
                    break;

                case NodeType.Air:
                    settings.AirUnits.Remove(unit);
                    break;

                case NodeType.Boarding:
                    Unit transport = (Unit) parent.Parent.Tag;
                    transport.LandUnits.Remove(unit);
                    break;
            }
        }

        /// <summary>
        ///     Delete the division
        /// </summary>
        /// <param name="division">Division to be deleted</param>
        private void RemoveDivision(Division division)
        {
            TreeNode node = _treeView.SelectedNode;
            TreeNode parent = node.Parent;

            parent.Nodes.Remove(node);

            CountrySettings settings = Scenarios.GetCountrySettings(_country) ??
                                       Scenarios.CreateCountrySettings(_country);

            // Set the edited flag
            settings.SetDirty();
            Scenarios.SetDirty();

            // Army / / Navy / / Air Force / / Onboard division
            Unit unit = parent.Tag as Unit;
            if (unit != null)
            {
                unit.Divisions.Remove(division);
                return;
            }

            // Undeployed Army / / Navy / / Luftwaffe Division
            switch ((NodeType) parent.Tag)
            {
                case NodeType.UndeployedLand:
                    settings.LandDivisions.Remove(division);
                    break;

                case NodeType.UndeployedNaval:
                    settings.NavalDivisions.Remove(division);
                    break;

                case NodeType.UndeployedAir:
                    settings.AirDivisions.Remove(division);
                    break;
            }
        }

        /// <summary>
        ///     unit / / Move the division to the top
        /// </summary>
        public void MoveTop()
        {
            TreeNode node = _treeView.SelectedNode;

            Unit unit = node.Tag as Unit;
            if (unit != null)
            {
                MoveUnit(unit, 0);
            }

            Division division = node.Tag as Division;
            if (division != null)
            {
                MoveDivision(division, 0);
            }
        }

        /// <summary>
        ///     unit / / Division 1 Move up
        /// </summary>
        public void MoveUp()
        {
            TreeNode node = _treeView.SelectedNode;
            TreeNode parent = node.Parent;
            int index = parent.Nodes.IndexOf(node) - 1;

            Unit unit = node.Tag as Unit;
            if (unit != null)
            {
                MoveUnit(unit, index);
            }

            Division division = node.Tag as Division;
            if (division != null)
            {
                MoveDivision(division, index);
            }
        }

        /// <summary>
        ///     unit / / Division 1 Move down
        /// </summary>
        public void MoveDown()
        {
            TreeNode node = _treeView.SelectedNode;
            TreeNode parent = node.Parent;
            int index = parent.Nodes.IndexOf(node) + 1;

            Unit unit = node.Tag as Unit;
            if (unit != null)
            {
                MoveUnit(unit, index);
            }

            Division division = node.Tag as Division;
            if (division != null)
            {
                MoveDivision(division, index);
            }
        }

        /// <summary>
        ///     unit / / Move the division to the end
        /// </summary>
        public void MoveBottom()
        {
            TreeNode node = _treeView.SelectedNode;
            TreeNode parent = node.Parent;
            int index = parent.Nodes.Count - 1;

            Unit unit = node.Tag as Unit;
            if (unit != null)
            {
                MoveUnit(unit, index);
            }

            Division division = node.Tag as Division;
            if (division != null)
            {
                if ((NodeType) parent.Nodes[parent.Nodes.Count - 1].Tag == NodeType.Boarding)
                {
                    index--;
                }
                MoveDivision(division, index);
            }
        }

        /// <summary>
        ///     Move the unit
        /// </summary>
        /// <param name="unit">Unit to move</param>
        /// <param name="index">Destination index</param>
        private void MoveUnit(Unit unit, int index)
        {
            TreeNode node = _treeView.SelectedNode;
            TreeNode parent = node.Parent;

            parent.Nodes.Remove(node);
            parent.Nodes.Insert(index, node);

            CountrySettings settings = Scenarios.GetCountrySettings(_country) ??
                                       Scenarios.CreateCountrySettings(_country);

            // Set the edited flag
            settings.SetDirty();
            Scenarios.SetDirty();

            // Army / / Navy / / Air Force / / On-board unit
            switch ((NodeType) parent.Tag)
            {
                case NodeType.Land:
                    settings.LandUnits.Remove(unit);
                    settings.LandUnits.Insert(index, unit);
                    break;

                case NodeType.Naval:
                    settings.NavalUnits.Remove(unit);
                    settings.NavalUnits.Insert(index, unit);
                    break;

                case NodeType.Air:
                    settings.AirUnits.Remove(unit);
                    settings.AirUnits.Insert(index, unit);
                    break;

                case NodeType.Boarding:
                    Unit transport = (Unit) parent.Parent.Tag;
                    transport.LandUnits.Remove(unit);
                    transport.LandUnits.Insert(index, unit);
                    break;
            }

            // Select the node to move
            _treeView.SelectedNode = node;
        }

        /// <summary>
        ///     Move the division
        /// </summary>
        /// <param name="division">The division to be moved</param>
        /// <param name="index">Destination index</param>
        private void MoveDivision(Division division, int index)
        {
            TreeNode node = _treeView.SelectedNode;
            TreeNode parent = node.Parent;
            parent.Nodes.Remove(node);
            parent.Nodes.Insert(index, node);

            CountrySettings settings = Scenarios.GetCountrySettings(_country) ??
                                       Scenarios.CreateCountrySettings(_country);

            // Set the edited flag
            settings.SetDirty();
            Scenarios.SetDirty();

            // Army / / Navy / / Air Force / / Onboard division
            Unit unit = parent.Tag as Unit;
            if (unit != null)
            {
                unit.Divisions.Remove(division);
                unit.Divisions.Insert(index, division);

                // Select the node to move
                _treeView.SelectedNode = node;
                return;
            }

            // Undeployed Army / / Navy / / Luftwaffe Division
            switch ((NodeType) parent.Tag)
            {
                case NodeType.UndeployedLand:
                    settings.LandDivisions.Remove(division);
                    settings.LandDivisions.Insert(index, division);
                    break;

                case NodeType.UndeployedNaval:
                    settings.NavalDivisions.Remove(division);
                    settings.NavalDivisions.Insert(index, division);
                    break;

                case NodeType.UndeployedAir:
                    settings.AirDivisions.Remove(division);
                    settings.AirDivisions.Insert(index, division);
                    break;
            }

            // Select the node to move
            _treeView.SelectedNode = node;
        }

        #endregion

        #region Node selection

        /// <summary>
        ///     Get the selected unit
        /// </summary>
        /// <returns>Selected unit</returns>
        public Unit GetSelectedUnit()
        {
            TreeNode node = _treeView.SelectedNode;
            if (node == null)
            {
                return null;
            }

            Unit unit = node.Tag as Unit;
            if (unit != null)
            {
                return unit;
            }

            node = node.Parent;

            return node?.Tag as Unit;
        }

        /// <summary>
        ///     Get the selected division
        /// </summary>
        /// <returns>Selected division</returns>
        public Division GetSelectedDivision()
        {
            TreeNode node = _treeView.SelectedNode;

            return node?.Tag as Division;
        }

        /// <summary>
        ///     Get the selected unit node
        /// </summary>
        /// <returns>Selected unit node</returns>
        private TreeNode GetSelectedUnitNode()
        {
            TreeNode node = _treeView.SelectedNode;
            if (node == null)
            {
                return null;
            }

            if (node.Tag is Unit)
            {
                return node;
            }

            node = node.Parent;
            if (node?.Tag is Unit)
            {
                return node;
            }

            return null;
        }

        /// <summary>
        ///     Get the selected division node
        /// </summary>
        /// <returns>Selected division node</returns>
        private TreeNode GetSelectedDivisionNode()
        {
            TreeNode node = _treeView.SelectedNode;
            if (node?.Tag is Division)
            {
                return node;
            }

            return null;
        }

        /// <summary>
        ///     Processing when selecting a node in the unit tree view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnitTreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            Unit unit = e.Node.Tag as Unit;
            if (unit != null)
            {
                OnUnitAfterSelect(unit, sender, e);
                return;
            }

            Division division = e.Node.Tag as Division;
            if (division != null)
            {
                OnDivisionAfterSelect(division, sender, e);
                return;
            }

            OnOtherAfterSelect(sender, e);
        }

        /// <summary>
        ///     Processing when selecting a unit node in the unit tree view
        /// </summary>
        /// <param name="unit">unit</param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnitAfterSelect(Unit unit, object sender, TreeViewEventArgs e)
        {
            int index = e.Node.Parent.Nodes.IndexOf(e.Node);
            int bottom = e.Node.Parent.Nodes.Count - 1;

            UnitTreeViewEventArgs args = new UnitTreeViewEventArgs(e)
            {
                Unit = unit,
                CanAddUnit = true,
                CanAddDivision = true,
                IsTop = index == 0,
                IsBottom = index == bottom
            };

            AfterSelect?.Invoke(sender, args);
        }

        /// <summary>
        ///     Processing when selecting a division node in the unit tree view
        /// </summary>
        /// <param name="division">Division</param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDivisionAfterSelect(Division division, object sender, TreeViewEventArgs e)
        {
            int index = e.Node.Parent.Nodes.IndexOf(e.Node);
            int bottom = e.Node.Parent.Nodes.Count - 1;
            // For the mounted unit -1 do
            if (division.Branch == Branch.Navy || division.Branch == Branch.Airforce)
            {
                bottom--;
            }

            UnitTreeViewEventArgs args = new UnitTreeViewEventArgs(e)
            {
                Division = division,
                Unit = e.Node.Parent.Tag as Unit,
                CanAddDivision = true,
                IsTop = index <= 0,
                IsBottom = index >= bottom
            };

            AfterSelect?.Invoke(sender, args);
        }

        /// <summary>
        ///     Units in the unit tree view / / Processing when selecting a node other than the division
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOtherAfterSelect(object sender, TreeViewEventArgs e)
        {
            UnitTreeViewEventArgs args = new UnitTreeViewEventArgs(e);
            switch ((NodeType) e.Node.Tag)
            {
                case NodeType.Land:
                case NodeType.Naval:
                case NodeType.Air:
                    args.CanAddUnit = true;
                    break;

                case NodeType.Boarding:
                    args.Unit = e.Node.Parent.Tag as Unit;
                    args.CanAddUnit = true;
                    break;

                case NodeType.UndeployedLand:
                case NodeType.UndeployedNaval:
                case NodeType.UndeployedAir:
                    args.CanAddDivision = true;
                    break;
            }

            AfterSelect?.Invoke(sender, args);
        }

        #endregion

        #region Inner class

        /// <summary>
        ///     Unit tree event parameters
        /// </summary>
        public class UnitTreeViewEventArgs : TreeViewEventArgs
        {
            /// <summary>
            ///     Target unit
            /// </summary>
            public Unit Unit { get; set; }

            /// <summary>
            ///     Target division
            /// </summary>
            public Division Division { get; set; }

            /// <summary>
            ///     Whether units can be added
            /// </summary>
            public bool CanAddUnit { get; set; }

            /// <summary>
            ///     Whether it is possible to add a division
            /// </summary>
            public bool CanAddDivision { get; set; }

            /// <summary>
            ///     Whether it is the first node
            /// </summary>
            public bool IsTop { get; set; }

            /// <summary>
            ///     Whether it is the last node
            /// </summary>
            public bool IsBottom { get; set; }

            /// <summary>
            ///     constructor
            /// </summary>
            /// <param name="e">Tree event parameters</param>
            public UnitTreeViewEventArgs(TreeViewEventArgs e) : base(e.Node, e.Action)
            {
            }
        }

        #endregion
    }
}
