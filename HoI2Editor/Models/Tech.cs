using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HoI2Editor.Utilities;

namespace HoI2Editor.Models
{
    /// <summary>
    ///     Technical group
    /// </summary>
    public class TechGroup
    {
        #region Public property

        /// <summary>
        ///     Technical group ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Technical category
        /// </summary>
        public TechCategory Category { get; set; }

        /// <summary>
        ///     Technical group name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Technical group explanation
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        ///     Item list
        /// </summary>
        public List<ITechItem> Items { get; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag of item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (TechGroupItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public TechGroup()
        {
            Items = new List<ITechItem>();
        }

        #endregion

        #region Technical item list operation

        /// <summary>
        ///     Insert the item in the technical item list
        /// </summary>
        /// <param name="item">Items to be added</param>
        public void AddItem(ITechItem item)
        {
            Items.Add(item);
        }

        /// <summary>
        ///     Insert the item in the technical item list
        /// </summary>
        /// <param name="item">Items to be inserted</param>
        /// <param name="position">Items just before the insertion position</param>
        public void InsertItem(ITechItem item, ITechItem position)
        {
            Items.Insert(Items.IndexOf(position) + 1, item);
        }

        /// <summary>
        ///     Delete items from the technical item list
        /// </summary>
        /// <param name="item">Items to be deleted</param>
        public void RemoveItem(ITechItem item)
        {
            Items.Remove(item);

            TechItem techItem = item as TechItem;
            if (techItem != null)
            {
                // Delete the temporary key
                techItem.RemoveTempKey();
                // Update technical items and ID correspondence
                Techs.TechIds.Remove(techItem.Id);
                Techs.TechIdMap.Remove(techItem.Id);
            }
            else if (item is TechLabel)
            {
                TechLabel labelItem = (TechLabel) item;
                // Delete the temporary key
                labelItem.RemoveTempKey();
            }

            // Delete the item of the duplicate character string list
            Techs.RemoveDuplicatedListItem(item);
        }

        /// <summary>
        ///     Move the item in the technical item list
        /// </summary>
        /// <param name="src">Items to be moved</param>
        /// <param name="dest">Items at the destination position</param>
        public void MoveItem(ITechItem src, ITechItem dest)
        {
            int srcIndex = Items.IndexOf(src);
            int destIndex = Items.IndexOf(dest);

            if (srcIndex > destIndex)
            {
                // When moving up
                Items.Insert(destIndex, src);
                Items.RemoveAt(srcIndex + 1);
            }
            else
            {
                // When moving down
                Items.Insert(destIndex + 1, src);
                Items.RemoveAt(srcIndex);
            }
        }

        #endregion

        #region Text column operation

        /// <summary>
        ///     Get the technical group name
        /// </summary>
        /// <returns>Technical group name</returns>
        public override string ToString()
        {
            return Config.ExistsKey(Name) ? Config.GetText(Name) : "";
        }

        /// <summary>
        ///     Get the technical group explanation
        /// </summary>
        /// <returns>Technical group explanation</returns>
        public string GetDesc()
        {
            return Config.ExistsKey(Desc) ? Config.GetText(Desc) : "";
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get whether the technical group has been edited
        /// </summary>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty()
        {
            return _dirtyFlag;
        }

        /// <summary>
        ///     Get whether or not the item has been edited
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty(TechGroupItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">Project ID</param>
        public void SetDirty(TechGroupItemId id)
        {
            _dirtyFlags[(int) id] = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
            Techs.SetDirty();
        }

        /// <summary>
        ///     Unlock all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (TechGroupItemId id in Enum.GetValues(typeof (TechGroupItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            foreach (ITechItem item in Items)
            {
                item.ResetDirtyAll();
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Common interface of technical items
    /// </summary>
    public interface ITechItem
    {
        #region Public property

        /// <summary>
        ///     Coordinates
        /// </summary>
        List<TechPosition> Positions { get; }

        #endregion

        #region Initialization

        /// <summary>
        ///     Duplicate technical items
        /// </summary>
        /// <returns>Duplicated technical items</returns>
        ITechItem Clone();

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get whether the technical item data has been edited
        /// </summary>
        /// <returns>If you have edited, return True</returns>
        bool IsDirty();

        /// <summary>
        ///     Get whether or not the item has been edited
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>If you have edited, return True</returns>
        bool IsDirty(TechItemId id);

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">Project ID</param>
        void SetDirty(TechItemId id);

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        void SetDirty();

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        void SetDirtyAll();

        /// <summary>
        ///     Unlock all edited flags
        /// </summary>
        void ResetDirtyAll();

        #endregion
    }

    /// <summary>
    ///     Technical project
    /// </summary>
    public class TechItem : ITechItem
    {
        #region Public property

        /// <summary>
        ///     Technical ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Technical name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Technical shortening name
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        ///     Technical explanation
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        ///     Image file name
        /// </summary>
        public string PictureName { get; set; }

        /// <summary>
        ///     Historical year
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        ///     Small research list
        /// </summary>
        public List<TechComponent> Components { get; }

        /// <summary>
        ///     Required Technical List (AND conditions)
        /// </summary>
        public List<RequiredTech> AndRequiredTechs { get; }

        /// <summary>
        ///     Required technical list (or conditional)
        /// </summary>
        public List<RequiredTech> OrRequiredTechs { get; }

        /// <summary>
        ///     Technical effect list
        /// </summary>
        public List<Command> Effects { get; }

        /// <summary>
        ///     Coordinates
        /// </summary>
        public List<TechPosition> Positions { get; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Regular expression of technical name
        /// </summary>
        private static readonly Regex RegexTechName = new Regex("TECH_APP_(\\w+)_(\\d+)_NAME");

        /// <summary>
        ///     Regular expression of technical explanation
        /// </summary>
        private static readonly Regex RegexTechDesc = new Regex("TECH_APP_(\\w+)_(\\d+)_DESC");

        /// <summary>
        ///     Regular expression of small research name
        /// </summary>
        private static readonly Regex RegexComponentName = new Regex("TECH_CMP_(\\w+)_(\\d+)_(\\d+)_NAME");

        /// <summary>
        ///     Edited flag of item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (TechItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public TechItem()
        {
            Positions = new List<TechPosition>();
            AndRequiredTechs = new List<RequiredTech>();
            OrRequiredTechs = new List<RequiredTech>();
            Components = new List<TechComponent>();
            Effects = new List<Command>();
        }

        /// <summary>
        ///     Duplicate technical applications
        /// </summary>
        /// <returns>Duplicated technical application</returns>
        public ITechItem Clone()
        {
            TechItem item = new TechItem
            {
                Id = Techs.GetNewId(Id + 10),
                Name = Config.GetTempKey(),
                ShortName = Config.GetTempKey(),
                Desc = Config.GetTempKey(),
                PictureName = PictureName,
                Year = Year
            };

            // Text column setting
            Config.SetText(item.Name, Config.GetText(Name), Game.TechTextFileName);
            Config.SetText(item.ShortName, Config.GetText(ShortName), Game.TechTextFileName);
            Config.SetText(item.Desc, Config.GetText(Desc), Game.TechTextFileName);

            // Coordinates
            foreach (TechPosition position in Positions)
            {
                item.Positions.Add(position.Clone());
            }

            // Small research list
            foreach (TechComponent component in Components)
            {
                item.Components.Add(component.Clone());
            }
            foreach (TechComponent component in item.Components)
            {
                component.Id = item.GetNewComponentId(item.Id + 1);
            }

            // Required technical list
            foreach (RequiredTech required in AndRequiredTechs)
            {
                item.AndRequiredTechs.Add(required.Clone());
            }
            foreach (RequiredTech required in OrRequiredTechs)
            {
                item.OrRequiredTechs.Add(required.Clone());
            }

            // Technical effect list
            foreach (Command command in Effects)
            {
                item.Effects.Add(new Command(command));
            }

            return item;
        }

        /// <summary>
        ///     Create new small research
        /// </summary>
        public void CreateNewComponents()
        {
            for (int i = 0; i < 5; i++)
            {
                TechComponent component = TechComponent.Create();
                component.Id = GetNewComponentId(Id + 1);
                Components.Add(component);
            }
        }

        #endregion

        #region Small research list

        /// <summary>
        ///     Add items to small research lists
        /// </summary>
        /// <param name="component">Additional items</param>
        public void AddComponent(TechComponent component)
        {
            Components.Add(component);
        }

        /// <summary>
        ///     Insert the item in the small research list
        /// </summary>
        /// <param name="component">Items to be inserted</param>
        /// <param name="index">Position to insert</param>
        public void InsertComponent(TechComponent component, int index)
        {
            Components.Insert(index, component);
        }

        /// <summary>
        ///     Move the item of the small research list
        /// </summary>
        /// <param name="src">Location of the source</param>
        /// <param name="dest">Position of destination</param>
        public void MoveComponent(int src, int dest)
        {
            TechComponent component = Components[src];

            if (src > dest)
            {
                // When moving up
                Components.Insert(dest, component);
                Components.RemoveAt(src + 1);
            }
            else
            {
                // When moving down
                Components.Insert(dest + 1, component);
                Components.RemoveAt(src);
            }
        }

        /// <summary>
        ///     Delete the items in the small research list
        /// </summary>
        /// <param name="index">Position of item to be deleted</param>
        public void RemoveComponent(int index)
        {
            // Delete items from the duplicate character string list
            Techs.DecrementDuplicatedListCount(Components[index].Name);

            Components.RemoveAt(index);
        }

        /// <summary>
        ///     Get unused small research ID
        /// </summary>
        /// <param name="startId">ID to start search</param>
        /// <returns>Unused small research ID</returns>
        public int GetNewComponentId(int startId)
        {
            int id = startId;
            List<int> ids = Components.Select(component => component.Id).ToList();
            while (ids.Contains(id))
            {
                id++;
            }
            return id;
        }

        #endregion

        #region Technical effect

        /// <summary>
        ///     Add items to the technical effect list
        /// </summary>
        /// <param name="command">Additional items</param>
        public void AddCommand(Command command)
        {
            Effects.Add(command);
        }

        /// <summary>
        ///     Insert items in the technical effect list
        /// </summary>
        /// <param name="command">Items to be inserted</param>
        /// <param name="index">Position to insert</param>
        public void InsertCommand(Command command, int index)
        {
            Effects.Insert(index, command);
        }

        /// <summary>
        ///     Move the technical effect list item
        /// </summary>
        /// <param name="src">Location of the source</param>
        /// <param name="dest">Position of destination</param>
        public void MoveCommand(int src, int dest)
        {
            Command command = Effects[src];

            if (src > dest)
            {
                // When moving up
                Effects.Insert(dest, command);
                Effects.RemoveAt(src + 1);
            }
            else
            {
                // When moving down
                Effects.Insert(dest + 1, command);
                Effects.RemoveAt(src);
            }
        }

        /// <summary>
        ///     Delete the technical effect list item
        /// </summary>
        /// <param name="index">Position of item to be deleted</param>
        public void RemoveCommand(int index)
        {
            Effects.RemoveAt(index);
        }

        #endregion

        #region Text column operation

        /// <summary>
        ///     Register the character string key number in the list
        /// </summary>
        /// <param name="list">List of registration destination</param>
        public void AddKeyNumbers(List<int> list)
        {
            int no;
            Match match;
            // Technical name
            if (!string.IsNullOrEmpty(Name))
            {
                match = RegexTechName.Match(Name);
                if (match.Success && int.TryParse(match.Groups[2].Value, out no) && !list.Contains(no))
                {
                    list.Add(no);
                }
            }
            // Technical explanation
            if (!string.IsNullOrEmpty(Desc))
            {
                match = RegexTechDesc.Match(Name);
                if (match.Success && int.TryParse(match.Groups[2].Value, out no) && !list.Contains(no))
                {
                    list.Add(no);
                }
            }
            // Small research name
            foreach (TechComponent component in Components.Where(component => !string.IsNullOrEmpty(component.Name)))
            {
                match = RegexComponentName.Match(component.Name);
                if (match.Success && int.TryParse(match.Groups[2].Value, out no) && !list.Contains(no))
                {
                    list.Add(no);
                }
            }
        }

        /// <summary>
        ///     Change the character string key to a storage format
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="list">Key number list</param>
        /// <returns>Return true if there is a change</returns>
        public bool RenameKeys(string categoryName, List<int> list)
        {
            bool dirty = false;
            int no = 0;

            // Technical name
            if (Config.IsTempKey(Name))
            {
                no = GetKeyNumber(list, categoryName);
                string newKey = $"TECH_APP_{categoryName}_{no}_NAME";
                string oldKey = Name;
                if (!Techs.IsDuplicatedName(oldKey))
                {
                    Config.RenameText(oldKey, newKey, Game.TechTextFileName);
                }
                else
                {
                    string s = Config.GetText(oldKey);
                    Config.SetText(newKey, !s.Equals(oldKey) ? s : "", Game.TechTextFileName);
                }
                Name = newKey;
                Techs.DecrementDuplicatedListCount(oldKey);
                Techs.IncrementDuplicatedListCount(newKey);

                // Set the edited flag
                SetDirty(TechItemId.Name);
                dirty = true;
            }
            // Technical shortening name
            if (Config.IsTempKey(ShortName))
            {
                if (no == 0)
                {
                    no = GetKeyNumber(list, categoryName);
                }
                string newKey = $"SHORT_TECH_APP_{categoryName}_{no}_NAME";
                string oldKey = ShortName;
                if (!Techs.IsDuplicatedName(oldKey))
                {
                    Config.RenameText(oldKey, newKey, Game.TechTextFileName);
                }
                else
                {
                    string s = Config.GetText(oldKey);
                    Config.SetText(newKey, !s.Equals(oldKey) ? s : "", Game.TechTextFileName);
                }
                ShortName = newKey;
                Techs.DecrementDuplicatedListCount(oldKey);
                Techs.IncrementDuplicatedListCount(newKey);

                // Set the edited flag
                SetDirty(TechItemId.ShortName);
                dirty = true;
            }
            // Technical explanation
            if (Config.IsTempKey(Desc))
            {
                if (no == 0)
                {
                    no = GetKeyNumber(list, categoryName);
                }
                string newKey = $"TECH_APP_{categoryName}_{no}_DESC";
                string oldKey = Desc;
                if (!Techs.IsDuplicatedName(oldKey))
                {
                    Config.RenameText(oldKey, newKey, Game.TechTextFileName);
                }
                else
                {
                    string s = Config.GetText(oldKey);
                    Config.SetText(newKey, !s.Equals(oldKey) ? s : "", Game.TechTextFileName);
                }
                Desc = newKey;
                Techs.DecrementDuplicatedListCount(oldKey);
                Techs.IncrementDuplicatedListCount(newKey);

                // Set the edited flag
                SetDirty(TechItemId.Desc);
                dirty = true;
            }
            // Small research name
            int componentId = 1;
            foreach (TechComponent component in Components)
            {
                if (Config.IsTempKey(component.Name))
                {
                    if (no == 0)
                    {
                        no = GetKeyNumber(list, categoryName);
                    }
                    string newKey = $"TECH_CMP_{categoryName}_{no}_{componentId}_NAME";
                    while (Config.ExistsKey(newKey))
                    {
                        componentId++;
                        newKey = $"TECH_CMP_{categoryName}_{no}_{componentId}_NAME";
                    }
                    string oldKey = component.Name;
                    if (!Techs.IsDuplicatedName(oldKey))
                    {
                        Config.RenameText(oldKey, newKey, Game.TechTextFileName);
                    }
                    else
                    {
                        string s = Config.GetText(oldKey);
                        Config.SetText(newKey, !s.Equals(oldKey) ? s : "", Game.TechTextFileName);
                    }
                    component.Name = newKey;
                    Techs.DecrementDuplicatedListCount(oldKey);
                    Techs.IncrementDuplicatedListCount(newKey);

                    // Set the edited flag
                    component.SetDirty(TechComponentItemId.Name);
                    SetDirty();
                    dirty = true;
                }
                componentId++;
            }

            return dirty;
        }

        /// <summary>
        ///     Get the character string key number
        /// </summary>
        /// <param name="list">Number list</param>
        /// <param name="categoryName">Category name</param>
        /// <returns>Number of string keys</returns>
        private int GetKeyNumber(List<int> list, string categoryName)
        {
            int no;
            Match match;
            // Get the number used in the technical name
            if (!string.IsNullOrEmpty(Name))
            {
                match = RegexTechName.Match(Name);
                if (match.Success && int.TryParse(match.Groups[2].Value, out no))
                {
                    return no;
                }
            }
            // Get the number used in the technical explanation
            if (!string.IsNullOrEmpty(Desc))
            {
                match = RegexTechDesc.Match(Desc);
                if (match.Success && int.TryParse(match.Groups[2].Value, out no))
                {
                    return no;
                }
            }
            // Get the number used in the small research name
            foreach (TechComponent component in Components.Where(component => !string.IsNullOrEmpty(component.Name)))
            {
                match = RegexComponentName.Match(component.Name);
                if (match.Success && int.TryParse(match.Groups[2].Value, out no))
                {
                    return no;
                }
            }

            // Return the free number
            no = 1;
            while (list.Contains(no) || ExistsUnlinkedKey(categoryName, no))
            {
                no++;
            }
            list.Add(no);
            return no;
        }

        /// <summary>
        ///     Check if there is an unused technical name definition
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="no">Technical number</param>
        /// <returns>Return true if there is a definition</returns>
        private bool ExistsUnlinkedKey(string categoryName, int no)
        {
            // Technical name
            string name = $"TECH_APP_{categoryName}_{no}_NAME";
            if (Config.ExistsKey(name))
            {
                return true;
            }
            // Technical shortening name
            name = $"SHORT_TECH_APP_{categoryName}_{no}_Name";
            if (Config.ExistsKey(name))
            {
                return true;
            }
            // Technical name
            name = $"TECH_APP_{categoryName}_{no}_DESC";
            if (Config.ExistsKey(name))
            {
                return true;
            }
            // Small research name
            for (int i = 1; i <= Components.Count; i++)
            {
                name = $"TECH_CMP_{categoryName}_{no}_{i}_NAME";
                if (Config.ExistsKey(name))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Delete the temporary key of the string
        /// </summary>
        public void RemoveTempKey()
        {
            // Technical name
            if (Config.IsTempKey(Name))
            {
                Config.RemoveText(Name, Game.TechTextFileName);
            }
            // Technical shortening name
            if (Config.IsTempKey(ShortName))
            {
                Config.RemoveText(ShortName, Game.TechTextFileName);
            }
            // Technical explanation
            if (Config.IsTempKey(Desc))
            {
                Config.RemoveText(Desc, Game.TechTextFileName);
            }

            // Small research name
            foreach (TechComponent component in Components)
            {
                if (Config.IsTempKey(component.Name))
                {
                    Config.RemoveText(component.Name, Game.TechTextFileName);
                }
            }
        }

        /// <summary>
        ///     Get the technical name
        /// </summary>
        /// <returns>Technical name</returns>
        public override string ToString()
        {
            return Config.ExistsKey(Name) ? Config.GetText(Name) : "";
        }

        /// <summary>
        ///     Get the name of technology shortening
        /// </summary>
        /// <returns>Technical shortening name</returns>
        public string GetShortName()
        {
            return Config.ExistsKey(ShortName) ? Config.GetText(ShortName) : "";
        }

        /// <summary>
        ///     Get technical explanation
        /// </summary>
        /// <returns>Technical explanation</returns>
        public string GetDesc()
        {
            return Config.ExistsKey(Desc) ? Config.GetText(Desc) : "";
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get whether the technical item data has been edited
        /// </summary>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty()
        {
            return _dirtyFlag;
        }

        /// <summary>
        ///     Get whether or not the item has been edited
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty(TechItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">Project ID</param>
        public void SetDirty(TechItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (TechItemId id in Enum.GetValues(typeof (TechItemId)))
            {
                _dirtyFlags[(int) id] = true;
            }
            foreach (TechPosition position in Positions)
            {
                position.SetDirtyAll();
            }
            foreach (RequiredTech tech in AndRequiredTechs)
            {
                tech.SetDirty();
            }
            foreach (RequiredTech tech in OrRequiredTechs)
            {
                tech.SetDirty();
            }
            foreach (TechComponent component in Components)
            {
                component.SetDirtyAll();
            }
            foreach (Command command in Effects)
            {
                command.SetDirtyAll();
            }
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Unlock all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (TechItemId id in Enum.GetValues(typeof (TechItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            foreach (TechPosition position in Positions)
            {
                position.ResetDirtyAll();
            }
            foreach (RequiredTech tech in AndRequiredTechs)
            {
                tech.ResetDirty();
            }
            foreach (RequiredTech tech in OrRequiredTechs)
            {
                tech.ResetDirty();
            }
            foreach (TechComponent component in Components)
            {
                component.ResetDirtyAll();
            }
            foreach (Command command in Effects)
            {
                command.ResetDirtyAll();
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Technical label
    /// </summary>
    public class TechLabel : ITechItem
    {
        #region Public property

        /// <summary>
        ///     Label name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Coordinates
        /// </summary>
        public List<TechPosition> Positions { get; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Regular expression of new category names
        /// </summary>
        private static readonly Regex RegexNewLabelName = new Regex("TECH_CAT_(\\w+)_(\\d+)");

        /// <summary>
        ///     Regular expression of old category names
        /// </summary>
        private static readonly Regex RegexOldLabelName = new Regex("TECH_CAT_(\\d+)");

        /// <summary>
        ///     Edited flag of item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (TechItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public TechLabel()
        {
            Positions = new List<TechPosition>();
        }

        /// <summary>
        ///     Duplicate technical labels
        /// </summary>
        /// <returns>Duplicated technology label</returns>
        public ITechItem Clone()
        {
            TechLabel item = new TechLabel { Name = Config.GetTempKey() };

            // Text column setting
            Config.SetText(item.Name, Config.GetText(Name), Game.TechTextFileName);

            // Coordinates
            foreach (TechPosition position in Positions)
            {
                item.Positions.Add(position.Clone());
            }

            return item;
        }

        /// <summary>
        ///     Create a technical label
        /// </summary>
        /// <returns>Created technology label</returns>
        public static TechLabel Create()
        {
            TechLabel item = new TechLabel { Name = Config.GetTempKey() };

            // Text column setting
            Config.SetText(item.Name, "", Game.TechTextFileName);

            return item;
        }

        #endregion

        #region Text column operation

        /// <summary>
        ///     Register the character string key number in the list
        /// </summary>
        /// <param name="list">List of registration destination</param>
        public void AddKeyNumbers(List<int> list)
        {
            if (string.IsNullOrEmpty(Name))
            {
                return;
            }

            int no;
            // Label name
            Match match = RegexNewLabelName.Match(Name);
            if (match.Success && int.TryParse(match.Groups[2].Value, out no) && !list.Contains(no))
            {
                list.Add(no);
            }
        }

        /// <summary>
        ///     Change the character string key to a storage format
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="list">Key number list</param>
        /// <returns>Return true if there is a change</returns>
        public bool RenameKeys(string categoryName, List<int> list)
        {
            bool dirty = false;

            // Label name
            if (Config.IsTempKey(Name) || IsOldStyleKey(Name))
            {
                int no = GetKeyNumber(list, categoryName);
                string newKey = $"TECH_CAT_{categoryName}_{no}";
                string oldKey = Name;
                if (!Techs.IsDuplicatedName(oldKey))
                {
                    Config.RenameText(oldKey, newKey, Game.TechTextFileName);
                }
                else
                {
                    string s = Config.GetText(oldKey);
                    Config.SetText(newKey, !s.Equals(oldKey) ? s : "", Game.TechTextFileName);
                }
                Name = newKey;
                Techs.DecrementDuplicatedListCount(oldKey);
                Techs.IncrementDuplicatedListCount(newKey);

                // Set the edited flag
                SetDirty(TechItemId.Name);
                dirty = true;
            }

            return dirty;
        }

        /// <summary>
        ///     Judge whether the character string key is an old format
        /// </summary>
        /// <param name="key">Character string key</param>
        /// <returns>Return true if it is an old format</returns>
        private static bool IsOldStyleKey(string key)
        {
            return !string.IsNullOrEmpty(key) && RegexOldLabelName.IsMatch(key);
        }

        /// <summary>
        ///     Get the character string key number
        /// </summary>
        /// <param name="list">Number list</param>
        /// <param name="categoryName">Category name</param>
        /// <returns>Number of string keys</returns>
        private static int GetKeyNumber(List<int> list, string categoryName)
        {
            // Return the free number
            int no = 1;
            while (list.Contains(no) || ExistsUnlinkedKey(categoryName, no))
            {
                no++;
            }
            list.Add(no);
            return no;
        }

        /// <summary>
        ///     Check if there is an unused label definition
        /// </summary>
        /// <param name="categoryName">Category name</param>
        /// <param name="no">Label number</param>
        /// <returns>Return true if there is a definition</returns>
        private static bool ExistsUnlinkedKey(string categoryName, int no)
        {
            string name = $"TECH_CAT_{categoryName}_{no}";
            if (Config.ExistsKey(name))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Delete the temporary key of the string
        /// </summary>
        public void RemoveTempKey()
        {
            // Label name
            if (Config.IsTempKey(Name))
            {
                Config.RemoveText(Name, Game.TechTextFileName);
            }
        }

        /// <summary>
        ///     Get the technical label name
        /// </summary>
        /// <returns>Technical label name</returns>
        public override string ToString()
        {
            if (!Config.ExistsKey(Name))
            {
                return "";
            }
            string s = Config.GetText(Name);

            // Read the color specified character string
            if (!string.IsNullOrEmpty(s) &&
                (s[0] == '%' || s[0] == 'ｧ' || s[0] == '§') &&
                s.Length > 4 &&
                s[1] >= '0' && s[1] <= '9' &&
                s[2] >= '0' && s[2] <= '9' &&
                s[3] >= '0' && s[3] <= '9')
            {
                s = s.Substring(4);
            }

            return s ?? "";
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get whether the technical item data has been edited
        /// </summary>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty()
        {
            return _dirtyFlag;
        }

        /// <summary>
        ///     Get whether or not the item has been edited
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty(TechItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">Project ID</param>
        public void SetDirty(TechItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (TechItemId id in Enum.GetValues(typeof (TechItemId)))
            {
                _dirtyFlags[(int) id] = true;
            }
            foreach (TechPosition position in Positions)
            {
                position.SetDirtyAll();
            }
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Unlock all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (TechItemId id in Enum.GetValues(typeof (TechItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            foreach (TechPosition position in Positions)
            {
                position.ResetDirtyAll();
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Technical event
    /// </summary>
    public class TechEvent : ITechItem
    {
        #region Public property

        /// <summary>
        ///     Technical event ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Technical ID
        /// </summary>
        public int TechId { get; set; }

        /// <summary>
        ///     Coordinates
        /// </summary>
        public List<TechPosition> Positions { get; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag of item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (TechItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public TechEvent()
        {
            Positions = new List<TechPosition>();
        }

        /// <summary>
        ///     Duplicate technical events
        /// </summary>
        /// <returns>Duplicated technical event</returns>
        public ITechItem Clone()
        {
            TechEvent item = new TechEvent { Id = Id, TechId = TechId };

            // Coordinates
            foreach (TechPosition position in Positions)
            {
                item.Positions.Add(position.Clone());
            }

            return item;
        }

        #endregion

        #region Text column operation

        /// <summary>
        ///     Delete the temporary key of the string
        /// </summary>
        public void RemoveTempKey()
        {
            // do nothing
        }

        /// <summary>
        ///     Get a technical event character string
        /// </summary>
        /// <returns>Technical event character string</returns>
        public override string ToString()
        {
            // Since there is no name, return the ID instead
            return IntHelper.ToString(Id);
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get whether the technical item data has been edited
        /// </summary>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty()
        {
            return _dirtyFlag;
        }

        /// <summary>
        ///     Get whether or not the item has been edited
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty(TechItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">Project ID</param>
        public void SetDirty(TechItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (TechItemId id in Enum.GetValues(typeof (TechItemId)))
            {
                _dirtyFlags[(int) id] = true;
            }
            foreach (TechPosition position in Positions)
            {
                position.SetDirtyAll();
            }
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Unlock all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (TechItemId id in Enum.GetValues(typeof (TechItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            foreach (TechPosition position in Positions)
            {
                position.ResetDirtyAll();
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Coordinates in the technical tree
    /// </summary>
    public class TechPosition
    {
        #region Public property

        /// <summary>
        ///     X Block
        /// </summary>
        public int X { get; set; }

        /// <summary>
        ///     Block
        /// </summary>
        public int Y { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag of item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (TechPositionItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Initialization

        /// <summary>
        ///     Duplicate coordinates
        /// </summary>
        /// <returns>Duplicated coordinates</returns>
        public TechPosition Clone()
        {
            TechPosition position = new TechPosition { X = X, Y = Y };

            return position;
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get whether the technical item data has been edited
        /// </summary>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty()
        {
            return _dirtyFlag;
        }

        /// <summary>
        ///     Get whether or not the item has been edited
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty(TechPositionItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">Project ID</param>
        public void SetDirty(TechPositionItemId id)
        {
            _dirtyFlags[(int) id] = true;
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (TechPositionItemId id in Enum.GetValues(typeof (TechPositionItemId)))
            {
                _dirtyFlags[(int) id] = true;
            }
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Unlock all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (TechPositionItemId id in Enum.GetValues(typeof (TechPositionItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Necessary technology
    /// </summary>
    public class RequiredTech
    {
        #region Public property

        /// <summary>
        ///     Technical ID
        /// </summary>
        public int Id { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Initialization

        /// <summary>
        ///     Duplicate coordinates
        /// </summary>
        /// <returns>Duplicated coordinates</returns>
        public RequiredTech Clone()
        {
            RequiredTech required = new RequiredTech { Id = Id };

            return required;
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get whether or not it has been edited
        /// </summary>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty()
        {
            return _dirtyFlag;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Unlock the edited flag
        /// </summary>
        public void ResetDirty()
        {
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Small study
    /// </summary>
    public class TechComponent
    {
        #region Public property

        /// <summary>
        ///     Small research ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Small research name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Small research characteristics
        /// </summary>
        public TechSpeciality Speciality { get; set; }

        /// <summary>
        ///     Degree of difficulty
        /// </summary>
        public int Difficulty { get; set; }

        /// <summary>
        ///     Whether it takes twice as much time
        /// </summary>
        public bool DoubleTime { get; set; }

        #endregion

        #region Internal field

        /// <summary>
        ///     Edited flag of item
        /// </summary>
        private readonly bool[] _dirtyFlags = new bool[Enum.GetValues(typeof (TechComponentItemId)).Length];

        /// <summary>
        ///     Edited flag
        /// </summary>
        private bool _dirtyFlag;

        #endregion

        #region Initialization

        /// <summary>
        ///     Create small research
        /// </summary>
        /// <returns>Created small research</returns>
        public static TechComponent Create()
        {
            TechComponent component = new TechComponent
            {
                Name = Config.GetTempKey(),
                Speciality = TechSpeciality.Artillery,
                Difficulty = 1
            };

            // Text column setting
            Config.SetText(component.Name, "", Game.TechTextFileName);

            return component;
        }

        /// <summary>
        ///     Duplicate small research
        /// </summary>
        /// <returns>Duplicated small research</returns>
        public TechComponent Clone()
        {
            TechComponent component = new TechComponent
            {
                Name = Config.GetTempKey(),
                Speciality = Speciality,
                Difficulty = Difficulty,
                DoubleTime = DoubleTime
            };

            Config.SetText(component.Name, Config.GetText(Name), Game.TechTextFileName);

            return component;
        }

        #endregion

        #region Text column operation

        /// <summary>
        ///     Get a small research name
        /// </summary>
        /// <returns>Small research name</returns>
        public override string ToString()
        {
            return Config.ExistsKey(Name) ? Config.GetText(Name) : "";
        }

        #endregion

        #region Edited flag operation

        /// <summary>
        ///     Get whether the technical item data has been edited
        /// </summary>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty()
        {
            return _dirtyFlag;
        }

        /// <summary>
        ///     Get whether or not the item has been edited
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>If you have edited, return True</returns>
        public bool IsDirty(TechComponentItemId id)
        {
            return _dirtyFlags[(int) id];
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        /// <param name="id">Project ID</param>
        public void SetDirty(TechComponentItemId id)
        {
            _dirtyFlags[(int) id] = true;
        }

        /// <summary>
        ///     Set the edited flag
        /// </summary>
        public void SetDirty()
        {
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Set all edited flags
        /// </summary>
        public void SetDirtyAll()
        {
            foreach (TechComponentItemId id in Enum.GetValues(typeof (TechComponentItemId)))
            {
                _dirtyFlags[(int) id] = true;
            }
            _dirtyFlag = true;
        }

        /// <summary>
        ///     Unlock all edited flags
        /// </summary>
        public void ResetDirtyAll()
        {
            foreach (TechComponentItemId id in Enum.GetValues(typeof (TechComponentItemId)))
            {
                _dirtyFlags[(int) id] = false;
            }
            _dirtyFlag = false;
        }

        #endregion
    }

    /// <summary>
    ///     Technical category
    /// </summary>
    public enum TechCategory
    {
        Infantry, // infantry
        Armor, // Armor and gun
        Naval, // Ship
        Aircraft, // Airline
        Industry, // industry
        LandDoctrines, // Land Battle Doctrine
        SecretWeapons, // Secret weapon
        NavalDoctrines, // Battle Doctrine
        AirDoctrines // Air Battle Doctrine
    }

    /// <summary>
    ///     Research characteristics
    /// </summary>
    public enum TechSpeciality
    {
        None,

        // Common
        Artillery, // artillery
        Mechanics, // Mechanical engineering
        Electronics, // Electronic engineering
        Chemistry, // Chemical
        Training, // train
        GeneralEquipment, // General equipment
        Rocketry, // Rocket engineering
        NavalEngineering, // Naval engineering
        Aeronautics, // Aviation science
        NuclearPhysics, // Nuclear physics
        NuclearEngineering, // Nuclear engineering
        Management, // manage manage
        IndustrialEngineering, // Industrial engineering
        Mathematics, // math
        SmallUnitTactics, // Small unit tactics
        LargeUnitTactics, // Large -scale unit tactics
        CentralizedExecution, // Intensive execution
        DecentralizedExecution, // Diversification execution
        TechnicalEfficiency, // Technical efficiency
        IndividualCourage, // Each of your courage
        InfantryFocus, // Focusing on infantry
        CombinedArmsFocus, // Emphasis on the All Works Federation
        LargeUnitFocus, // Large -scale troops attach importance to
        NavalArtillery, // Artillery
        NavalTraining, // Naval training
        AircraftTesting, // Aircraft examinations
        FighterTactics, // Fighter tactics
        BomberTactics, // Bomber tactics
        CarrierTactics, // Aircraft carrier tactics
        SubmarineTactics, // Submarine tactics
        LargeTaskforceTactics, // Large -scale mechanical unit tactics
        SmallTaskforceTactics, // Small martial arts unit tactics
        Seamanship, // Maneuvering
        Piloting, // Coastal navigation

        // DH only
        Avionics, // Aviation electronic engineering
        Munitions, // ammunition
        VehicleEngineering, // Vehicle engineering
        CarrierDesign, // Aircraft carrier design
        SubmarineDesign, // Diving ship design
        FighterDesign, // Fighter design
        BomberDesign, // Bomber design
        MountainTraining, // Mountain training
        AirborneTraining, // Airborne training
        MarineTraining, // Marine training
        ManeuverTactics, // Mobile tactics
        BlitzkriegTactics, // Dengeki Tactics
        StaticDefenseTactics, // Quiet defense
        Medicine, // Medical science
        CavalryTactics, // Cavalry tactics (DH1.03 or later only)
        RtUser1,
        RtUser2,
        RtUser3,
        RtUser4,
        RtUser5,
        RtUser6,
        RtUser7,
        RtUser8,
        RtUser9,
        RtUser10,
        RtUser11,
        RtUser12,
        RtUser13,
        RtUser14,
        RtUser15,
        RtUser16,
        RtUser17, // Since then only DH1.03 or later
        RtUser18,
        RtUser19,
        RtUser20,
        RtUser21,
        RtUser22,
        RtUser23,
        RtUser24,
        RtUser25,
        RtUser26,
        RtUser27,
        RtUser28,
        RtUser29,
        RtUser30,
        RtUser31,
        RtUser32,
        RtUser33,
        RtUser34,
        RtUser35,
        RtUser36,
        RtUser37,
        RtUser38,
        RtUser39,
        RtUser40,
        RtUser41,
        RtUser42,
        RtUser43,
        RtUser44,
        RtUser45,
        RtUser46,
        RtUser47,
        RtUser48,
        RtUser49,
        RtUser50,
        RtUser51,
        RtUser52,
        RtUser53,
        RtUser54,
        RtUser55,
        RtUser56,
        RtUser57,
        RtUser58,
        RtUser59,
        RtUser60
    }

    /// <summary>
    ///     Technical group item ID
    /// </summary>
    public enum TechGroupItemId
    {
        Name, // name
        Desc // explanation
    }

    /// <summary>
    ///     Technical project ID
    /// </summary>
    public enum TechItemId
    {
        Id, // ID
        Name, // name
        ShortName, // Shortening name
        Desc, // explanation
        PictureName, // Image file name
        Year, // Historical year
        TechId // Technical ID
    }

    /// <summary>
    ///     Technical seat logo project ID
    /// </summary>
    public enum TechPositionItemId
    {
        X, // X Block
        Y // Y coordinate
    }

    /// <summary>
    ///     Small research project ID
    /// </summary>
    public enum TechComponentItemId
    {
        Id, // Small research ID
        Name, // Small research name
        Specilaity, // Research characteristics
        Difficulty, // Degree of difficulty
        DoubleTime // Whether it takes twice as much time
    }
}
