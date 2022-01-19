using System;
using System.Collections.Generic;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a group of EncounterTemplate items.
    /// </summary>
    [Serializable]
    public class EncounterTemplateGroup
    {
        private string _fCategory = "";

        private string _fName = "";

        private List<EncounterTemplate> _fTemplates = new List<EncounterTemplate>();

        /// <summary>
        ///     Gets or sets the category of the group.
        /// </summary>
        public string Category
        {
            get => _fCategory;
            set => _fCategory = value;
        }

        /// <summary>
        ///     Gets or sets the name of the group.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the list of EncounterTemplate items.
        /// </summary>
        public List<EncounterTemplate> Templates
        {
            get => _fTemplates;
            set => _fTemplates = value;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public EncounterTemplateGroup()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="name">The name of the group.</param>
        /// <param name="category">The category of the group.</param>
        public EncounterTemplateGroup(string name, string category)
        {
            _fName = name;
            _fCategory = category;
        }
    }

    /// <summary>
    ///     Class representing a template for an encounter.
    /// </summary>
    [Serializable]
    public class EncounterTemplate
    {
        private Difficulty _fDifficulty = Difficulty.Moderate;

        private List<EncounterTemplateSlot> _fSlots = new List<EncounterTemplateSlot>();

        /// <summary>
        ///     Gets or sets the difficulty of the template.
        /// </summary>
        public Difficulty Difficulty
        {
            get => _fDifficulty;
            set => _fDifficulty = value;
        }

        /// <summary>
        ///     Gets or sets the list of slots in this template.
        /// </summary>
        public List<EncounterTemplateSlot> Slots
        {
            get => _fSlots;
            set => _fSlots = value;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public EncounterTemplate()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="diff">The difficulty of the template.</param>
        public EncounterTemplate(Difficulty diff)
        {
            _fDifficulty = diff;
        }

        /// <summary>
        ///     Find a template slot matching the given encounter slot.
        /// </summary>
        /// <param name="enc_slot">The encounter slot.</param>
        /// <param name="level">The encounter level.</param>
        /// <returns>Returns the matching template slot, or null if no slot is found.</returns>
        public EncounterTemplateSlot FindSlot(EncounterSlot encSlot, int level)
        {
            foreach (var templateSlot in _fSlots)
            {
                if (templateSlot.Count < encSlot.CombatData.Count)
                    continue;

                var match = templateSlot.Match(encSlot.Card, level);
                if (!match)
                    continue;

                return templateSlot;
            }

            return null;
        }

        /// <summary>
        ///     Creates a copy of the template.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EncounterTemplate Copy()
        {
            var et = new EncounterTemplate();

            et.Difficulty = _fDifficulty;

            foreach (var slot in _fSlots)
                et.Slots.Add(slot.Copy());

            return et;
        }
    }

    /// <summary>
    ///     Class representing a slot in an EncounterTemplate.
    /// </summary>
    [Serializable]
    public class EncounterTemplateSlot
    {
        private int _fCount = 1;

        private RoleFlag _fFlag = RoleFlag.Standard;

        private int _fLevelAdjustment;

        private bool _fMinions;

        private List<RoleType> _fRoles = new List<RoleType>();

        /// <summary>
        ///     Gets or sets the list of allowed roles.
        ///     If empty, all roles are allowed.
        /// </summary>
        public List<RoleType> Roles
        {
            get => _fRoles;
            set => _fRoles = value;
        }

        /// <summary>
        ///     Gets or sets the type of creature (standard, elite, solo).
        /// </summary>
        public RoleFlag Flag
        {
            get => _fFlag;
            set => _fFlag = value;
        }

        /// <summary>
        ///     Gets or sets the level adjustment.
        /// </summary>
        public int LevelAdjustment
        {
            get => _fLevelAdjustment;
            set => _fLevelAdjustment = value;
        }

        /// <summary>
        ///     Gets or sets the number of creatures in the slot.
        /// </summary>
        public int Count
        {
            get => _fCount;
            set => _fCount = value;
        }

        /// <summary>
        ///     Gets or sets whether the slot should contain minions.
        /// </summary>
        public bool Minions
        {
            get => _fMinions;
            set => _fMinions = value;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public EncounterTemplateSlot()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="count">The number of creatures.</param>
        /// <param name="level_adj">The level adjustment.</param>
        /// <param name="flag">The type of creature.</param>
        /// <param name="role">The allowed role.</param>
        public EncounterTemplateSlot(int count, int levelAdj, RoleFlag flag, RoleType role)
        {
            _fCount = count;
            _fLevelAdjustment = levelAdj;
            _fFlag = flag;
            _fRoles.Add(role);
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="count">The number of creatures.</param>
        /// <param name="level_adj">The level adjustment.</param>
        /// <param name="flag">The type of creature.</param>
        /// <param name="roles">The allowed roles.</param>
        public EncounterTemplateSlot(int count, int levelAdj, RoleFlag flag, RoleType[] roles)
        {
            _fCount = count;
            _fLevelAdjustment = levelAdj;
            _fFlag = flag;
            _fRoles.AddRange(roles);
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="count">The number of creatures.</param>
        /// <param name="level_adj">The level adjustment.</param>
        /// <param name="flag">The type of creature.</param>
        public EncounterTemplateSlot(int count, int levelAdj, RoleFlag flag)
        {
            _fCount = count;
            _fLevelAdjustment = levelAdj;
            _fFlag = flag;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="count">The number of creatures.</param>
        /// <param name="level_adj">The level adjustment.</param>
        /// <param name="role">The allowed role.</param>
        public EncounterTemplateSlot(int count, int levelAdj, RoleType role)
        {
            _fCount = count;
            _fLevelAdjustment = levelAdj;
            _fRoles.Add(role);
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="count">The number of creatures.</param>
        /// <param name="level_adj">The level adjustment.</param>
        /// <param name="roles">The allowed roles.</param>
        public EncounterTemplateSlot(int count, int levelAdj, RoleType[] roles)
        {
            _fCount = count;
            _fLevelAdjustment = levelAdj;
            _fRoles.AddRange(roles);
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="count">The number of creatures.</param>
        /// <param name="level_adj">The level adjustment.</param>
        public EncounterTemplateSlot(int count, int levelAdj)
        {
            _fCount = count;
            _fLevelAdjustment = levelAdj;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="count">The number of creatures.</param>
        /// <param name="level_adj">The level adjustment.</param>
        /// <param name="minions">Whether the slot should contain minions.</param>
        public EncounterTemplateSlot(int count, int levelAdj, bool minions)
        {
            _fCount = count;
            _fLevelAdjustment = levelAdj;
            _fMinions = minions;
        }

        /// <summary>
        ///     Determine whether a given creature fits this slot.
        /// </summary>
        /// <param name="card">The creature.</param>
        /// <param name="encounter_level">The level of the encounter.</param>
        /// <returns>True if the creature matches; false otherwise.</returns>
        public bool Match(EncounterCard card, int encounterLevel)
        {
            var creature = Session.FindCreature(card.CreatureId, SearchType.Global);

            // Check the level
            var level = encounterLevel + _fLevelAdjustment;
            if (level < 1)
                level = 1;
            if (creature.Level != level)
                return false;

            // Check minion status
            var isMinion = creature.Role is Minion;
            if (isMinion != _fMinions)
                return false;

            // Check the role matches
            var roleOk = false;
            if (_fRoles.Count == 0)
            {
                // We match any role
                roleOk = true;
            }
            else
            {
                var role = creature.Role as ComplexRole;
                foreach (var r in card.Roles)
                    if (_fRoles.Contains(role.Type))
                    {
                        roleOk = true;
                        break;
                    }
            }

            if (!roleOk)
                return false;

            // Check the elite / solo flag matches
            if (_fFlag != card.Flag)
                return false;

            return true;
        }

        /// <summary>
        ///     Creates a copy of the slot.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EncounterTemplateSlot Copy()
        {
            var slot = new EncounterTemplateSlot();

            slot.Roles.AddRange(_fRoles);
            slot.Flag = _fFlag;
            slot.LevelAdjustment = _fLevelAdjustment;
            slot.Count = _fCount;
            slot.Minions = _fMinions;

            return slot;
        }
    }
}
