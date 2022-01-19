using System;
using System.Collections.Generic;
using System.Drawing;

namespace Masterplan.Data
{
    /// <summary>
    ///     Enumeration describing the combat state of a creature.
    /// </summary>
    public enum CreatureState
    {
        /// <summary>
        ///     The creature has over half its HP remaining.
        /// </summary>
        Active,

        /// <summary>
        ///     The creature has no more than half its HP remaining.
        /// </summary>
        Bloodied,

        /// <summary>
        ///     The creature is at or below 0 HP.
        /// </summary>
        Defeated
    }

    /// <summary>
    ///     Class containing data about a creature in combat.
    /// </summary>
    [Serializable]
    public class CombatData : IComparable<CombatData>
    {
        /// <summary>
        ///     Used by the Location property to specify that the token is not on the map.
        /// </summary>
        public static Point NoPoint = new Point(int.MinValue, int.MinValue);

        private int _fAltitude;

        private List<OngoingCondition> _fConditions = new List<OngoingCondition>();

        private int _fDamage;

        private bool _fDelaying;

        private string _fDisplayName = "";

        private Guid _fId = Guid.NewGuid();

        private int _fInitiative = int.MinValue;

        private Point _fLocation = NoPoint;

        private int _fTempHp;

        private List<Guid> _fUsedPowers = new List<Guid>();

        private bool _fVisible = true;

        /// <summary>
        ///     Gets or sets the unique ID of this token.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name to be displayed for this token.
        /// </summary>
        public string DisplayName
        {
            get => _fDisplayName;
            set => _fDisplayName = value;
        }

        /// <summary>
        ///     Gets or sets the token location.
        /// </summary>
        public Point Location
        {
            get => _fLocation;
            set => _fLocation = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the token is visible to the PCs.
        /// </summary>
        public bool Visible
        {
            get => _fVisible;
            set => _fVisible = value;
        }

        /// <summary>
        ///     Gets or sets the token's initiative score.
        /// </summary>
        public int Initiative
        {
            get => _fInitiative;
            set => _fInitiative = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the token is delaying / readying its action.
        /// </summary>
        public bool Delaying
        {
            get => _fDelaying;
            set => _fDelaying = value;
        }

        /// <summary>
        ///     Gets or sets the total hit point damage taken by the token.
        /// </summary>
        public int Damage
        {
            get => _fDamage;
            set => _fDamage = value;
        }

        /// <summary>
        ///     Gets or sets the token's temporary hit points.
        /// </summary>
        public int TempHp
        {
            get => _fTempHp;
            set => _fTempHp = value;
        }

        /// <summary>
        ///     Gets or sets the token's altitude.
        /// </summary>
        public int Altitude
        {
            get => _fAltitude;
            set => _fAltitude = value;
        }

        /// <summary>
        ///     Gets or sets the list of expended powers.
        /// </summary>
        public List<Guid> UsedPowers
        {
            get => _fUsedPowers;
            set => _fUsedPowers = value;
        }

        /// <summary>
        ///     Gets or sets the list of conditions affecting the token.
        /// </summary>
        public List<OngoingCondition> Conditions
        {
            get => _fConditions;
            set => _fConditions = value;
        }

        /// <summary>
        ///     Resets the CombatData for a new encounter.
        /// </summary>
        /// <param name="resetDamage">True to reset damage, false otherwise</param>
        public void Reset(bool resetDamage)
        {
            _fLocation = NoPoint;
            _fVisible = true;
            _fInitiative = int.MinValue;
            _fDelaying = false;
            _fTempHp = 0;
            _fAltitude = 0;

            _fUsedPowers.Clear();
            _fConditions.Clear();

            if (resetDamage)
                _fDamage = 0;
        }

        /// <summary>
        ///     Creates a copy of the CombatData.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public CombatData Copy()
        {
            var data = new CombatData();

            data.Id = _fId;
            data.DisplayName = _fDisplayName;
            data.Location = new Point(_fLocation.X, _fLocation.Y);
            data.Visible = _fVisible;
            data.Initiative = _fInitiative;
            data.Delaying = _fDelaying;
            data.Damage = _fDamage;
            data.TempHp = _fTempHp;
            data.Altitude = _fAltitude;

            foreach (var powerId in _fUsedPowers)
                data.UsedPowers.Add(powerId);

            foreach (var c in _fConditions)
                data.Conditions.Add(c.Copy());

            return data;
        }

        /// <summary>
        ///     Returns the display name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _fDisplayName;
        }

        /// <summary>
        ///     Compares this CombatData to another.
        /// </summary>
        /// <param name="rhs">The CombatData to compare to.</param>
        /// <returns>
        ///     Returns -1 if this CombatData should be sorted before the other, +1 if the other should be sorted first, 0
        ///     otherwise.
        /// </returns>
        public int CompareTo(CombatData rhs)
        {
            return _fDisplayName.CompareTo(rhs.DisplayName);
        }
    }
}
