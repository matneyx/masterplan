using System;
using System.Collections.Generic;
using System.Drawing;

namespace Masterplan.Data
{
    /// <summary>
    ///     Used to modify the XP cost of an EncounterSlot.
    /// </summary>
    public enum EncounterSlotType
    {
        /// <summary>
        ///     Opponent.
        /// </summary>
        Opponent,

        /// <summary>
        ///     Ally.
        /// </summary>
        Ally,

        /// <summary>
        ///     Neutral.
        /// </summary>
        Neutral
    }

    /// <summary>
    ///     Class representing a slot in an Encounter.
    /// </summary>
    [Serializable]
    public class EncounterSlot
    {
        private EncounterCard _fCard = new EncounterCard();

        private List<CombatData> _fCombatData = new List<CombatData>();

        private Guid _fId = Guid.NewGuid();

        private EncounterSlotType _fType = EncounterSlotType.Opponent;

        /// <summary>
        ///     Gets or sets the unique ID of the slot.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the creature occupying this slot.
        /// </summary>
        public EncounterCard Card
        {
            get => _fCard;
            set => _fCard = value;
        }

        /// <summary>
        ///     Gets or sets the type of slot.
        /// </summary>
        public EncounterSlotType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the list of CombatData items for this slot.
        /// </summary>
        public List<CombatData> CombatData
        {
            get => _fCombatData;
            set => _fCombatData = value;
        }

        /// <summary>
        ///     Gets the XP value of the creature or creatures in this slot.
        /// </summary>
        public int Xp
        {
            get
            {
                var mod = 0;
                switch (_fType)
                {
                    case EncounterSlotType.Opponent:
                        mod = 1;
                        break;
                    case EncounterSlotType.Ally:
                        mod = -1;
                        break;
                    case EncounterSlotType.Neutral:
                        mod = 0;
                        break;
                }

                return _fCard.Xp * _fCombatData.Count * mod;
            }
        }

        /// <summary>
        ///     Finds the CombatData item which has the specified map location.
        /// </summary>
        /// <param name="location">The map location.</param>
        /// <returns>Returns the CombatData item, if it exists; null otherwise.</returns>
        public CombatData FindCombatData(Point location)
        {
            foreach (var cmd in _fCombatData)
                if (cmd.Location == location)
                    return cmd;

            return null;
        }

        /// <summary>
        ///     Creates a copy of the slot.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EncounterSlot Copy()
        {
            var slot = new EncounterSlot();

            slot.Id = _fId;
            slot.Card = _fCard.Copy();
            slot.Type = _fType;

            foreach (var ccd in _fCombatData)
                slot.CombatData.Add(ccd.Copy());

            return slot;
        }

        /// <summary>
        ///     Sets the default display name for each CombatData element in the slot.
        /// </summary>
        public void SetDefaultDisplayNames()
        {
            var title = _fCard.Title;

            if (_fCombatData == null)
            {
                _fCombatData = new List<CombatData>();
                _fCombatData.Add(new CombatData());
            }

            foreach (var cd in _fCombatData)
                if (_fCombatData.Count == 1)
                {
                    cd.DisplayName = title;
                }
                else
                {
                    var n = _fCombatData.IndexOf(cd) + 1;
                    cd.DisplayName = title + " " + n;
                }
        }

        /// <summary>
        ///     Determines the combat state (active, bloodied, defeated) for a creature.
        /// </summary>
        /// <param name="data">The CombatData object for the creature.</param>
        /// <returns>Returns the CreatureState for the creature.</returns>
        public CreatureState GetState(CombatData data)
        {
            var hpMax = _fCard.Hp;

            var hpBloodied = hpMax / 2;
            var hpCurrent = hpMax - data.Damage;

            if (hpCurrent <= 0)
                return CreatureState.Defeated;

            if (hpCurrent <= hpBloodied)
                return CreatureState.Bloodied;

            return CreatureState.Active;
        }
    }
}
