using System;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class holding information on a party of heroes.
    /// </summary>
    [Serializable]
    public class Party
    {
        private int _fLevel = 1;

        private int _fSize = 5;

        private int _fXp;

        /// <summary>
        ///     Gets or sets the number of heroes in the party.
        /// </summary>
        public int Size
        {
            get => _fSize;
            set => _fSize = value;
        }

        /// <summary>
        ///     Gets or sets the average XP of the party.
        /// </summary>
        public int Xp
        {
            get => _fXp;
            set => _fXp = value;
        }

        /// <summary>
        ///     Gets or sets the average level of the party.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public Party()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="size">The number of heroes in the party.</param>
        /// <param name="level">The average level of the heroes in the party.</param>
        public Party(int size, int level)
        {
            _fSize = size;
            _fLevel = level;
        }

        /// <summary>
        ///     Calculates the relative difficulty of an item of the given level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>Returns the difficulty.</returns>
        public Difficulty GetDifficulty(int level)
        {
            if (level <= _fLevel - 3)
                return Difficulty.Trivial;

            if (level <= _fLevel - 1)
                return Difficulty.Easy;

            if (level <= _fLevel + 1)
                return Difficulty.Moderate;

            if (level <= _fLevel + 4)
                return Difficulty.Hard;

            return Difficulty.Extreme;
        }

        /// <summary>
        ///     Creates a copy of the party information.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Party Copy()
        {
            var p = new Party();

            p.Size = _fSize;
            p.Level = _fLevel;
            p.Xp = _fXp;

            return p;
        }
    }
}
