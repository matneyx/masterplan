using System;

namespace Masterplan.Tools
{
    /// <summary>
    ///     Class representing a pair of linked objects.
    /// </summary>
    /// <typeparam name="T1">The type of object contained in the First property.</typeparam>
    /// <typeparam name="T2">The type of object contained in the First property.</typeparam>
    [Serializable]
    public class Pair<T1, T2> : IComparable<Pair<T1, T2>>
    {
        private T1 _fFirst;

        private T2 _fSecond;

        /// <summary>
        ///     The first part of the Pair.
        /// </summary>
        public T1 First
        {
            get => _fFirst;
            set => _fFirst = value;
        }

        /// <summary>
        ///     The second part of the Pair.
        /// </summary>
        public T2 Second
        {
            get => _fSecond;
            set => _fSecond = value;
        }

        /// <summary>
        ///     The default constructor.
        /// </summary>
        public Pair()
        {
        }

        /// <summary>
        ///     Constructor which initialises the Pair object.
        /// </summary>
        /// <param name="first">The first part.</param>
        /// <param name="second">The second part.</param>
        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

        /// <summary>
        ///     Returns a string representation of the Pair object in the format [first], [second].
        /// </summary>
        /// <returns>Returns a string representation of the Pair object.</returns>
        public override string ToString()
        {
            return _fFirst + ", " + _fSecond;
        }

        /// <summary>
        ///     Compares this Pair object to another by the contents of their First property.
        /// </summary>
        /// <param name="rhs">The Pair object to compare to.</param>
        /// <returns>
        ///     Returns -1 if this object should be sorted before the other, +1 if it should be sorted after the other, or 0
        ///     if they are identical.
        /// </returns>
        public int CompareTo(Pair<T1, T2> rhs)
        {
            var strA = _fFirst.ToString();
            var strB = rhs.First.ToString();

            return strA.CompareTo(strB);
        }
    }
}
