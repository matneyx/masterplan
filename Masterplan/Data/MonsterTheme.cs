using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a theme which can be applied to groups of monsters.
    /// </summary>
    [Serializable]
    public class MonsterTheme
    {
        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        private List<ThemePowerData> _fPowers = new List<ThemePowerData>();

        private List<Pair<string, int>> _fSkillBonuses = new List<Pair<string, int>>();

        /// <summary>
        ///     Gets or sets the unique ID of the monster theme.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the monster theme.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the list of skill bonuses.
        /// </summary>
        public List<Pair<string, int>> SkillBonuses
        {
            get => _fSkillBonuses;
            set => _fSkillBonuses = value;
        }

        /// <summary>
        ///     Gets or sets the list of theme powers.
        /// </summary>
        public List<ThemePowerData> Powers
        {
            get => _fPowers;
            set => _fPowers = value;
        }

        /// <summary>
        ///     Finds the power with the specified ID.
        /// </summary>
        /// <param name="power_id">The ID to search for.</param>
        /// <returns>Returns the power if it exists; null otherwise.</returns>
        public ThemePowerData FindPower(Guid powerId)
        {
            foreach (var tpd in _fPowers)
                if (tpd.Power.Id == powerId)
                    return tpd;

            return null;
        }

        /// <summary>
        ///     Returns a list of the powers that fit the specified roles.
        /// </summary>
        /// <param name="creature_roles">The roles to fit.</param>
        /// <param name="type">The power type to list (attack or utility).</param>
        /// <returns>Returns a list of matching powers.</returns>
        public List<ThemePowerData> ListPowers(List<RoleType> creatureRoles, PowerType type)
        {
            var candidates = new List<ThemePowerData>();

            foreach (var power in _fPowers)
            {
                if (power.Type != type)
                    continue;

                if (power.Roles.Count == 0)
                {
                    candidates.Add(power);
                }
                else
                {
                    var match = false;
                    foreach (var role in creatureRoles)
                        if (power.Roles.Contains(role))
                            match = true;

                    if (match)
                        candidates.Add(power);
                }
            }

            return candidates;
        }

        /// <summary>
        ///     Creates a copy of the monster theme.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public MonsterTheme Copy()
        {
            var mt = new MonsterTheme();

            mt.Id = _fId;
            mt.Name = _fName;

            foreach (var sb in _fSkillBonuses)
                mt.SkillBonuses.Add(new Pair<string, int>(sb.First, sb.Second));

            foreach (var tpd in _fPowers)
                mt.Powers.Add(tpd.Copy());

            return mt;
        }

        /// <summary>
        ///     Returns the theme name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _fName;
        }
    }

    /// <summary>
    ///     Power types.
    /// </summary>
    public enum PowerType
    {
        /// <summary>
        ///     Attack power.
        /// </summary>
        Attack,

        /// <summary>
        ///     Utility power.
        /// </summary>
        Utility
    }

    /// <summary>
    ///     Class representing a power in a monster theme.
    /// </summary>
    [Serializable]
    public class ThemePowerData
    {
        private CreaturePower _fPower = new CreaturePower();

        private List<RoleType> _fRoles = new List<RoleType>();

        private PowerType _fType = PowerType.Attack;

        /// <summary>
        ///     Gets or sets the power.
        /// </summary>
        public CreaturePower Power
        {
            get => _fPower;
            set => _fPower = value;
        }

        /// <summary>
        ///     Gets or sets the power type (attack or utility).
        /// </summary>
        public PowerType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the creature roles the power is suitable for.
        /// </summary>
        public List<RoleType> Roles
        {
            get => _fRoles;
            set => _fRoles = value;
        }

        /// <summary>
        ///     Creates a copy of the power data.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public ThemePowerData Copy()
        {
            var tpd = new ThemePowerData();

            tpd.Power = _fPower.Copy();
            tpd.Type = _fType;

            foreach (var rt in _fRoles)
                tpd.Roles.Add(rt);

            return tpd;
        }

        /// <summary>
        ///     Returns the power name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _fPower.Name;
        }
    }
}
