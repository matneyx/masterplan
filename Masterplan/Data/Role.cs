using System;

namespace Masterplan.Data
{
    /// <summary>
    ///     Creature or trap.
    /// </summary>
    public enum ThreatType
    {
        /// <summary>
        ///     Creature.
        /// </summary>
        Creature,

        /// <summary>
        ///     Trap.
        /// </summary>
        Trap
    }

    /// <summary>
    ///     Creature / trap roles.
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        ///     Artillery role.
        /// </summary>
        Artillery,

        /// <summary>
        ///     Blaster role.
        /// </summary>
        Blaster,

        /// <summary>
        ///     Brute role.
        /// </summary>
        Brute,

        /// <summary>
        ///     Controller role.
        /// </summary>
        Controller,

        /// <summary>
        ///     Lurker role.
        /// </summary>
        Lurker,

        /// <summary>
        ///     Obstacle role.
        /// </summary>
        Obstacle,

        /// <summary>
        ///     Skirmisher role.
        /// </summary>
        Skirmisher,

        /// <summary>
        ///     Soldier role.
        /// </summary>
        Soldier,

        /// <summary>
        ///     Warder role.
        /// </summary>
        Warder
    }

    /// <summary>
    ///     Standard / elite / solo.
    /// </summary>
    public enum RoleFlag
    {
        /// <summary>
        ///     Standard.
        /// </summary>
        Standard,

        /// <summary>
        ///     Elite.
        /// </summary>
        Elite,

        /// <summary>
        ///     Solo.
        /// </summary>
        Solo
    }

    /// <summary>
    ///     Interface for a role.
    ///     Classes Minion and ComplexRole implement this interface.
    /// </summary>
    public interface IRole
    {
        /// <summary>
        ///     Creates a copy of the role.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        IRole Copy();
    }

    /// <summary>
    ///     Minion role.
    /// </summary>
    [Serializable]
    public class Minion : IRole
    {
        private bool _fHasRole;

        private RoleType _fType = RoleType.Artillery;

        /// <summary>
        ///     Gets or sets a value indicating whether this minion has a role.
        /// </summary>
        public bool HasRole
        {
            get => _fHasRole;
            set => _fHasRole = value;
        }

        /// <summary>
        ///     Gets or sets the minion role.
        /// </summary>
        public RoleType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Minion
        ///     or
        ///     Minion [role]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_fHasRole)
                return "Minion " + _fType;
            return "Minion";
        }

        /// <summary>
        ///     Creates a copy of the Minion.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public IRole Copy()
        {
            var m = new Minion();

            m.HasRole = _fHasRole;
            m.Type = _fType;

            return m;
        }
    }

    /// <summary>
    ///     Class representing a creature / trap role.
    /// </summary>
    [Serializable]
    public class ComplexRole : IRole
    {
        private RoleFlag _fFlag = RoleFlag.Standard;

        private bool _fLeader;

        private RoleType _fType = RoleType.Artillery;

        /// <summary>
        ///     Gets or sets the role type.
        /// </summary>
        public RoleType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the role modifier (elite / solo).
        /// </summary>
        public RoleFlag Flag
        {
            get => _fFlag;
            set => _fFlag = value;
        }

        /// <summary>
        ///     Gets or sets the Leader role.
        /// </summary>
        public bool Leader
        {
            get => _fLeader;
            set => _fLeader = value;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public ComplexRole()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="type">The role to set.</param>
        public ComplexRole(RoleType type)
        {
            _fType = type;
        }

        /// <summary>
        ///     [Elite / Solo] [role] [(L)]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var flag = "";
            switch (_fFlag)
            {
                case RoleFlag.Elite:
                    flag = "Elite ";
                    break;
                case RoleFlag.Solo:
                    flag = "Solo ";
                    break;
            }

            var role = _fType.ToString();
            var leader = _fLeader ? " (L)" : "";

            return flag + role + leader;
        }

        /// <summary>
        ///     Creates a copy of the role.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public IRole Copy()
        {
            var cr = new ComplexRole();

            cr.Type = _fType;
            cr.Flag = _fFlag;
            cr.Leader = _fLeader;

            return cr;
        }
    }
}
