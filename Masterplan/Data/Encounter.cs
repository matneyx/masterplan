using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a combat encounter game element.
    /// </summary>
    [Serializable]
    public class Encounter : IElement
    {
        private List<CustomToken> _fCustomTokens = new List<CustomToken>();

        private Guid _fMapAreaId = Guid.Empty;

        private Guid _fMapId = Guid.Empty;

        private List<EncounterNote> _fNotes = new List<EncounterNote>();

        private List<SkillChallenge> _fSkillChallenges = new List<SkillChallenge>();

        private List<EncounterSlot> _fSlots = new List<EncounterSlot>();

        private List<Trap> _fTraps = new List<Trap>();

        private List<EncounterWave> _fWaves = new List<EncounterWave>();

        /// <summary>
        ///     Gets or sets the list of encounter slots.
        /// </summary>
        public List<EncounterSlot> Slots
        {
            get => _fSlots;
            set => _fSlots = value;
        }

        /// <summary>
        ///     Gets or sets the list of traps in the encounter.
        /// </summary>
        public List<Trap> Traps
        {
            get => _fTraps;
            set => _fTraps = value;
        }

        /// <summary>
        ///     Gets or sets the list of skill challenges in the encounter.
        /// </summary>
        public List<SkillChallenge> SkillChallenges
        {
            get => _fSkillChallenges;
            set => _fSkillChallenges = value;
        }

        /// <summary>
        ///     Gets or sets the list of custom map tokens.
        /// </summary>
        public List<CustomToken> CustomTokens
        {
            get => _fCustomTokens;
            set => _fCustomTokens = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the map to be used, or Guid.Empty to use no map.
        /// </summary>
        public Guid MapId
        {
            get => _fMapId;
            set => _fMapId = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the map area to be used, or Guid.Empty to use the full map.
        /// </summary>
        public Guid MapAreaId
        {
            get => _fMapAreaId;
            set => _fMapAreaId = value;
        }

        /// <summary>
        ///     Gets or sets the list of encounter notes.
        /// </summary>
        public List<EncounterNote> Notes
        {
            get => _fNotes;
            set => _fNotes = value;
        }

        /// <summary>
        ///     Gets or sets the list of encounter waves.
        /// </summary>
        public List<EncounterWave> Waves
        {
            get => _fWaves;
            set => _fWaves = value;
        }

        /// <summary>
        ///     Gets the number of creatures in the encounter.
        /// </summary>
        public int Count
        {
            get
            {
                var count = 0;

                foreach (var slot in AllSlots)
                    count += slot.CombatData.Count;

                return count;
            }
        }

        /// <summary>
        ///     Gets the collection of all the encounter slots for all waves of the encounter.
        /// </summary>
        public List<EncounterSlot> AllSlots
        {
            get
            {
                var slots = new List<EncounterSlot>();

                slots.AddRange(_fSlots);

                if (_fWaves != null)
                    foreach (var ew in _fWaves)
                        slots.AddRange(ew.Slots);

                return slots;
            }
        }

        /// <summary>
        ///     Finds the encounter slot with the given ID.
        /// </summary>
        /// <param name="slotId">The ID of the slot.</param>
        /// <returns>Returns the slot, or null if no such slot exists.</returns>
        public EncounterSlot FindSlot(Guid slotId)
        {
            foreach (var slot in AllSlots)
                if (slot.Id == slotId)
                    return slot;

            return null;
        }

        /// <summary>
        ///     Finds the encounter slot with the given combat data.
        /// </summary>
        /// <param name="data">The combat data.</param>
        /// <returns>Returns the slot, or null if no such slot exists.</returns>
        public EncounterSlot FindSlot(CombatData data)
        {
            foreach (var slot in AllSlots)
                if (slot.CombatData.Contains(data))
                    return slot;

            return null;
        }

        /// <summary>
        ///     Finds the encounter wave with the given encounter slot.
        /// </summary>
        /// <param name="slot">The encounter slot.</param>
        /// <returns>Returns the wave, or null if no such wave exists.</returns>
        public EncounterWave FindWave(EncounterSlot slot)
        {
            foreach (var ew in _fWaves)
                if (ew.Slots.Contains(slot))
                    return ew;

            return null;
        }

        /// <summary>
        ///     Searches for the CombatData with the given ID.
        /// </summary>
        /// <param name="id">The id to search for.</param>
        /// <returns>Returns the CombatData if it exists; null otherwise.</returns>
        public CombatData FindCombatData(Guid id)
        {
            foreach (var slot in AllSlots)
            foreach (var cd in slot.CombatData)
                if (cd.Id == id)
                    return cd;

            return null;
        }

        /// <summary>
        ///     Finds the trap with the given ID.
        /// </summary>
        /// <param name="trapId">The ID of the trap.</param>
        /// <returns>Returns the trap, or null if no such trap exists.</returns>
        public Trap FindTrap(Guid trapId)
        {
            foreach (var trap in _fTraps)
                if (trap.Id == trapId)
                    return trap;

            return null;
        }

        /// <summary>
        ///     Finds the skill challenge with the given ID.
        /// </summary>
        /// <param name="challengeId">The ID of the skill challenge.</param>
        /// <returns>Returns the skill challenge, or null if no such trap exists.</returns>
        public SkillChallenge FindSkillChallenge(Guid challengeId)
        {
            foreach (var sc in _fSkillChallenges)
                if (sc.Id == challengeId)
                    return sc;

            return null;
        }

        /// <summary>
        ///     Finds the encounter note with the given title.
        /// </summary>
        /// <param name="noteTitle">The title of the note.</param>
        /// <returns>Returns the note, or null if no such note exists.</returns>
        public EncounterNote FindNote(string noteTitle)
        {
            foreach (var note in _fNotes)
                if (note.Title == noteTitle)
                    return note;

            return null;
        }

        /// <summary>
        ///     Determines whether the encounter contains the given combatant.
        /// </summary>
        /// <param name="combatantId">The ID of a creature or NPC.</param>
        /// <returns>True if the encounter contains the creature; false otherwise.</returns>
        public bool Contains(Guid combatantId)
        {
            foreach (var slot in AllSlots)
                if (slot.Card.CreatureId == combatantId)
                    return true;

            return false;
        }

        /// <summary>
        ///     Returns the display name of a creature / hero / trap with the given ID.
        /// </summary>
        /// <param name="id">The ID to search for.</param>
        /// <returns>Returns the name if found; empty string otherwise.</returns>
        public string WhoIs(Guid id)
        {
            // Check slot combat data
            foreach (var slot in AllSlots)
            foreach (var data in slot.CombatData)
                if (data.Id == id)
                    return data.DisplayName;

            // Check heroes
            foreach (var hero in Session.Project.Heroes)
                if (hero.Id == id)
                    return hero.Name;

            // Check traps
            foreach (var trap in _fTraps)
                if (trap.Id == id)
                    return trap.Name;

            return "";
        }

        /// <summary>
        ///     Calculates the level of the encounter.
        /// </summary>
        /// <param name="partySize">The party size.</param>
        /// <returns>Returns the encounter level.</returns>
        public int GetLevel(int partySize)
        {
            if (partySize == 0)
                return -1;

            var xp = GetXp();
            if (Session.Project != null)
                xp = (int)(xp / Session.Project.CampaignSettings.Xp);

            xp /= partySize;

            var result = 0;
            var minDiff = int.MaxValue;

            for (var cl = 0; cl <= 40; ++cl)
            {
                var levelXp = Experience.GetCreatureXp(cl);
                var diff = Math.Abs(xp - levelXp);

                if (diff < minDiff)
                {
                    result = cl;
                    minDiff = diff;
                }
            }

            return result;
        }

        /// <summary>
        ///     Adds blank standard notes to the encounter.
        /// </summary>
        public void SetStandardEncounterNotes()
        {
            _fNotes.Add(new EncounterNote("Illumination"));
            _fNotes.Add(new EncounterNote("Features of the Area"));
            _fNotes.Add(new EncounterNote("Setup"));
            _fNotes.Add(new EncounterNote("Tactics"));
            _fNotes.Add(new EncounterNote("Victory Conditions"));
        }

        private Difficulty get_diff(int partyLevel, int partySize)
        {
            if (GetXp() <= 0)
                return Difficulty.Trivial;

            var level = GetLevel(partySize);
            var levelDiff = level - partyLevel;

            if (levelDiff < -2)
                return Difficulty.Trivial;
            if (levelDiff == -2 || levelDiff == -1)
                return Difficulty.Easy;
            if (levelDiff == 0 || levelDiff == 1)
                return Difficulty.Moderate;
            if (levelDiff == 2 || levelDiff == 3 || levelDiff == 4)
                return Difficulty.Hard;
            return Difficulty.Extreme;
        }

        /// <summary>
        ///     Calculates the XP value of the encounter.
        /// </summary>
        /// <returns>Returns the encounter XP value.</returns>
        public int GetXp()
        {
            var total = 0;

            foreach (var slot in AllSlots)
                total += slot.Xp;

            foreach (var trap in _fTraps)
                total += trap.Xp;

            foreach (var sc in _fSkillChallenges)
                total += sc.GetXp();

            total = Math.Max(0, total);
            return total;
        }

        /// <summary>
        ///     Calculates the difficulty of the encounter.
        /// </summary>
        /// <param name="partyLevel">The party level.</param>
        /// <param name="partySize">The party size.</param>
        /// <returns>Returns the encounter difficulty.</returns>
        public Difficulty GetDifficulty(int partyLevel, int partySize)
        {
            var diffs = new List<Difficulty>();

            foreach (var slot in AllSlots)
            {
                if (slot.Type != EncounterSlotType.Opponent)
                    continue;

                var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                if (creature != null)
                    diffs.Add(Ai.GetThreatDifficulty(creature.Level + slot.Card.LevelAdjustment, partyLevel));
            }

            foreach (var trap in _fTraps) diffs.Add(Ai.GetThreatDifficulty(trap.Level, partyLevel));

            foreach (var sc in _fSkillChallenges) diffs.Add(sc.GetDifficulty(partyLevel, partySize));

            diffs.Add(get_diff(partyLevel, partySize));

            if (diffs.Contains(Difficulty.Extreme))
                return Difficulty.Extreme;

            if (diffs.Contains(Difficulty.Hard))
                return Difficulty.Hard;

            if (diffs.Contains(Difficulty.Moderate))
                return Difficulty.Moderate;

            if (diffs.Contains(Difficulty.Easy))
                return Difficulty.Easy;

            return Difficulty.Trivial;
        }

        /// <summary>
        ///     Creates a copy of the encounter.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public IElement Copy()
        {
            var enc = new Encounter();

            foreach (var slot in _fSlots)
                enc.Slots.Add(slot.Copy());

            foreach (var trap in _fTraps)
                enc.Traps.Add(trap.Copy());

            foreach (var sc in _fSkillChallenges)
                enc.SkillChallenges.Add(sc.Copy() as SkillChallenge);

            foreach (var ct in _fCustomTokens)
                enc.CustomTokens.Add(ct.Copy());

            enc.MapId = _fMapId;
            enc.MapAreaId = _fMapAreaId;

            foreach (var en in _fNotes)
                enc.Notes.Add(en.Copy());

            foreach (var ew in _fWaves)
                enc.Waves.Add(ew.Copy());

            return enc;
        }
    }

    /// <summary>
    ///     Class representing a piece of information about an encounter.
    /// </summary>
    [Serializable]
    public class EncounterNote
    {
        private string _fContents = "";

        private Guid _fId = Guid.NewGuid();

        private string _title = "";

        /// <summary>
        ///     Gets or sets the unique ID of the note.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the encounter note title.
        /// </summary>
        public string Title
        {
            get => _title;
            set => _title = value;
        }

        /// <summary>
        ///     Gets or sets the encounter note contents.
        /// </summary>
        public string Contents
        {
            get => _fContents;
            set => _fContents = value;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public EncounterNote()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="title">The title of the note.</param>
        public EncounterNote(string title)
        {
            _title = title;
        }

        /// <summary>
        ///     Creates a copy of the note.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EncounterNote Copy()
        {
            var en = new EncounterNote();

            en.Id = _fId;
            en.Title = _title;
            en.Contents = _fContents;

            return en;
        }

        /// <summary>
        ///     Returns the note title.
        /// </summary>
        /// <returns>Returns the note title.</returns>
        public override string ToString()
        {
            return _title;
        }
    }

    /// <summary>
    ///     Class representing a wave of combatants
    /// </summary>
    [Serializable]
    public class EncounterWave
    {
        private bool _fActive;

        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        private List<EncounterSlot> _fSlots = new List<EncounterSlot>();

        /// <summary>
        ///     Gets or sets the ID of the wave.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the wave.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets whether the wave is active.
        /// </summary>
        public bool Active
        {
            get => _fActive;
            set => _fActive = value;
        }

        /// <summary>
        ///     Gets or sets the list of encounter slots in the wave.
        /// </summary>
        public List<EncounterSlot> Slots
        {
            get => _fSlots;
            set => _fSlots = value;
        }

        /// <summary>
        ///     Gets the number of combatants in this wave.
        /// </summary>
        public int Count
        {
            get
            {
                var count = 0;
                foreach (var slot in _fSlots)
                    count += slot.CombatData.Count;

                return count;
            }
        }

        /// <summary>
        ///     Creates a copy of the wave.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EncounterWave Copy()
        {
            var ew = new EncounterWave();

            ew.Id = _fId;
            ew.Name = _fName;
            ew.Active = _fActive;

            foreach (var slot in _fSlots)
                ew.Slots.Add(slot.Copy());

            return ew;
        }
    }
}
