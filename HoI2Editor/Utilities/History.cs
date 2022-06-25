using System.Collections.Generic;

namespace HoI2Editor.Utilities
{
    /// <summary>
    ///     Class that manages the history of strings
    /// </summary>
    public class History
    {
        /// <summary>
        ///     The substance of history
        /// </summary>
        private readonly List<string> _items;

        /// <summary>
        ///     Maximum number of histories
        /// </summary>
        private readonly int _size;

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="size">Maximum number of histories</param>
        public History(int size)
        {
            _size = size;
            _items = new List<string>();
        }

        /// <summary>
        ///     Clear history items
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        ///     Get history items
        /// </summary>
        /// <returns>History item</returns>
        public string[] Get()
        {
            return _items.ToArray();
        }

        /// <summary>
        ///     Set history items
        /// </summary>
        /// <param name="items">History item</param>
        public void Set(string[] items)
        {
            _items.Clear();
            _items.AddRange(items);
        }

        /// <summary>
        ///     Add history item
        /// </summary>
        /// <param name="item">History item</param>
        public void Add(string item)
        {
            if (!_items.Contains(item))
            {
                // Add an item at the beginning
                _items.Insert(0, item);
                // Delete the last item if the maximum number is exceeded
                if (_items.Count > _size)
                {
                    _items.RemoveAt(_size);
                }
            }
            else
            {
                // Remove duplicate elements
                int index = _items.IndexOf(item);
                _items.RemoveAt(index);
                // Add an item at the beginning
                _items.Insert(0, item);
            }
        }
    }
}
