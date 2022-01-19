using System;
using System.Collections.Generic;

namespace Masterplan.Tools
{
    /// <summary>
    ///     Utility class for performing quick searches.
    /// </summary>
    /// <typeparam name="T">Type to create the tree for; must implement the IComparable interface.</typeparam>
    public class BinarySearchTree<T> where T : IComparable<T>
    {
        private T _fData;

        private BinarySearchTree<T> _fLeft;
        private BinarySearchTree<T> _fRight;

        /// <summary>
        ///     Gets the number of items in the tree.
        /// </summary>
        public int Count
        {
            get
            {
                if (_fData == null)
                    return 0;

                var count = 1;

                if (_fLeft != null)
                    count += _fLeft.Count;

                if (_fRight != null)
                    count += _fRight.Count;

                return count;
            }
        }

        /// <summary>
        ///     Gets a List containing all the items in the tree in sorted order.
        /// </summary>
        public List<T> SortedList
        {
            get
            {
                var list = new List<T>();

                if (_fData != null)
                {
                    if (_fLeft != null)
                        list.AddRange(_fLeft.SortedList);

                    list.Add(_fData);

                    if (_fRight != null)
                        list.AddRange(_fRight.SortedList);
                }

                return list;
            }
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public BinarySearchTree()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="item">The item to begin the tree with.</param>
        public BinarySearchTree(T item)
        {
            _fData = item;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="list">The list of items to build the tree with.</param>
        public BinarySearchTree(IEnumerable<T> list)
        {
            Add(list);
        }

        /// <summary>
        ///     Adds an item to the tree.
        /// </summary>
        /// <param name="item">The item to add to the tree.</param>
        public void Add(T item)
        {
            if (_fData == null)
            {
                _fData = item;
                return;
            }

            var n = _fData.CompareTo(item);

            if (n > 0)
            {
                if (_fLeft == null)
                    _fLeft = new BinarySearchTree<T>(item);
                else
                    _fLeft.Add(item);
            }

            if (n < 0)
            {
                if (_fRight == null)
                    _fRight = new BinarySearchTree<T>(item);
                else
                    _fRight.Add(item);
            }
        }

        /// <summary>
        ///     Adds a list of items to the tree.
        /// </summary>
        /// <param name="list">The items to add to the tree.</param>
        public void Add(IEnumerable<T> list)
        {
            foreach (var item in list)
                Add(item);
        }

        /// <summary>
        ///     Searches the tree for the given item.
        /// </summary>
        /// <param name="item">The item to look for.</param>
        /// <returns>Returns true if the item is present in the tree; false otherwise.</returns>
        public bool Contains(T item)
        {
            if (_fData == null)
                return false;

            var n = _fData.CompareTo(item);

            if (n > 0)
                return _fLeft?.Contains(item) ?? false;

            if (n < 0)
                return _fRight?.Contains(item) ?? false;

            return true;
        }
    }
}
