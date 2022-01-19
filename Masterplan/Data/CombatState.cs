using System;
using System.Collections.Generic;
using System.Drawing;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class holding information about an encounter in progress.
    /// </summary>
    [Serializable]
    public class CombatState
    {
        private Guid _fCurrentActor = Guid.Empty;

        private int _fCurrentRound = 1;

        private Encounter _fEncounter;

        private Dictionary<Guid, CombatData> _fHeroData;

        private EncounterLog _fLog = new EncounterLog();

        private int _fPartyLevel = Session.Project.Party.Level;

        private List<OngoingCondition> _fQuickEffects = new List<OngoingCondition>();

        private int _fRemovedCreatureXp;

        private List<MapSketch> _fSketches = new List<MapSketch>();

        private DateTime _fTimestamp = DateTime.Now;

        private List<TokenLink> _fTokenLinks;

        private Dictionary<Guid, CombatData> _fTrapData;

        private Rectangle _fViewpoint = Rectangle.Empty;

        /// <summary>
        ///     Gets or sets the time at which the combat was paused.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets or sets the level of the party.
        /// </summary>
        public int PartyLevel
        {
            get => _fPartyLevel;
            set => _fPartyLevel = value;
        }

        /// <summary>
        ///     Gets or sets the encounter data.
        /// </summary>
        public Encounter Encounter
        {
            get => _fEncounter;
            set => _fEncounter = value;
        }

        /// <summary>
        ///     Gets or sets the current round.
        /// </summary>
        public int CurrentRound
        {
            get => _fCurrentRound;
            set => _fCurrentRound = value;
        }

        /// <summary>
        ///     Gets or sets the combat data for heroes in the encounter.
        /// </summary>
        public Dictionary<Guid, CombatData> HeroData
        {
            get => _fHeroData;
            set => _fHeroData = value;
        }

        /// <summary>
        ///     Gets or sets the combat data for traps in the encounter.
        /// </summary>
        public Dictionary<Guid, CombatData> TrapData
        {
            get => _fTrapData;
            set => _fTrapData = value;
        }

        /// <summary>
        ///     Gets or sets the links between tokens.
        /// </summary>
        public List<TokenLink> TokenLinks
        {
            get => _fTokenLinks;
            set => _fTokenLinks = value;
        }

        /// <summary>
        ///     Gets or sets the XP gained from creatures which are no longer in the combat.
        /// </summary>
        public int RemovedCreatureXp
        {
            get => _fRemovedCreatureXp;
            set => _fRemovedCreatureXp = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the current actor.
        /// </summary>
        public Guid CurrentActor
        {
            get => _fCurrentActor;
            set => _fCurrentActor = value;
        }

        /// <summary>
        ///     Gets or sets the visible map area.
        /// </summary>
        public Rectangle Viewpoint
        {
            get => _fViewpoint;
            set => _fViewpoint = value;
        }

        /// <summary>
        ///     Gets or sets the map sketches.
        /// </summary>
        public List<MapSketch> Sketches
        {
            get => _fSketches;
            set => _fSketches = value;
        }

        /// <summary>
        ///     Gets or sets the list of previously added effects.
        /// </summary>
        public List<OngoingCondition> QuickEffects
        {
            get => _fQuickEffects;
            set => _fQuickEffects = value;
        }

        /// <summary>
        ///     Gets or sets the endounter log
        /// </summary>
        public EncounterLog Log
        {
            get => _fLog;
            set => _fLog = value;
        }

        /// <summary>
        ///     Returns the timestamp.
        /// </summary>
        /// <returns>Returns the timestamp.</returns>
        public override string ToString()
        {
            return _fTimestamp.ToShortDateString() + " at " + _fTimestamp.ToShortTimeString();
        }
    }
}
