using System;
using System.Collections;
using System.Windows.Forms;

namespace Masterplan.Tools
{
    /// <summary>
    ///     Provides methods for sorting ListView contents by column.
    ///     An instance of this class should be set as the ListView's ListViewItemSorter property.
    /// </summary>
    public class ListViewSorter : IComparer
    {
        /// <summary>
        ///     Gets or sets a value indicating the column the ListView contents should be sorted by.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the ListView contents should be sorted in ascending order.
        /// </summary>
        public bool Ascending { get; set; } = true;

        /// <summary>
        ///     Sets the column used for sorting.
        ///     If this method is called multiple times with the same column, the value of the Ascending property is toggled on and
        ///     off.
        /// </summary>
        /// <param name="col">The column to be used for sorting.</param>
        public void SetColumn(int col)
        {
            if (Column == col)
            {
                Ascending = !Ascending;
            }
            else
            {
                Column = col;
                Ascending = true;
            }
        }

        /// <summary>
        ///     Sorts the contents of a ListView control.
        ///     This method should be called in response to a ListView.ColumnClicked event.
        /// </summary>
        /// <param name="list">The ListView control to be sorted.</param>
        /// <param name="column">The column to sort by.</param>
        public static void Sort(ListView list, int column)
        {
            var sorter = list.ListViewItemSorter as ListViewSorter;
            if (sorter != null)
            {
                sorter.SetColumn(column);
                list.Sort();
            }
        }

        /// <summary>
        ///     Compares two ListViewItem objects, given the values of the Column and Ascending properties.
        /// </summary>
        /// <param name="x">The first ListViewItem object to compare.</param>
        /// <param name="y">The second ListViewItem object to compare.</param>
        /// <returns>Returns -1 if x should be sorted before y, +1 if y should be sorted before x, and 0 if they are identical.</returns>
        public int Compare(object x, object y)
        {
            var lhs = x as ListViewItem;
            var rhs = y as ListViewItem;

            if (lhs == null || rhs == null)
                throw new ArgumentException();

            var lhsStr = lhs.SubItems[Column].Text;
            var rhsStr = rhs.SubItems[Column].Text;

            // Try integers
            try
            {
                var lhsInt = int.Parse(lhsStr);
                var rhsInt = int.Parse(rhsStr);

                return lhsInt.CompareTo(rhsInt) * (Ascending ? 1 : -1);
            }
            catch
            {
            }

            // Try floating point
            try
            {
                var lhsFlt = float.Parse(lhsStr);
                var rhsFlt = float.Parse(rhsStr);

                return lhsFlt.CompareTo(rhsFlt) * (Ascending ? 1 : -1);
            }
            catch
            {
            }

            // Try dates
            try
            {
                var lhsDt = DateTime.Parse(lhsStr);
                var rhsDt = DateTime.Parse(rhsStr);

                return lhsDt.CompareTo(rhsDt) * (Ascending ? 1 : -1);
            }
            catch
            {
            }

            // Must just be a string then
            return lhsStr.CompareTo(rhsStr) * (Ascending ? 1 : -1);
        }
    }
}
