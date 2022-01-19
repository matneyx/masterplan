using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     An encounter log, containing entries.
    /// </summary>
    [Serializable]
    public class EncounterLog
    {
        private bool _fActive = true;

        private List<IEncounterLogEntry> _fEntries = new List<IEncounterLogEntry>();

        /// <summary>
        ///     Gets or sets the list of encounter log entries.
        /// </summary>
        public List<IEncounterLogEntry> Entries
        {
            get => _fEntries;
            set => _fEntries = value;
        }

        /// <summary>
        ///     Gets or sets whether the log responds to Add() methods.
        /// </summary>
        public bool Active
        {
            get => _fActive;
            set => _fActive = value;
        }

        /// <summary>
        ///     Adds a StartRoundLogEntry to the log.
        /// </summary>
        /// <param name="round">The number of the round.</param>
        public void AddStartRoundEntry(int round)
        {
            if (!_fActive)
                return;

            var entry = new StartRoundLogEntry();
            entry.Round = round;
            _fEntries.Add(entry);
        }

        /// <summary>
        ///     Adds a StartTurnLogEntry to the log.
        /// </summary>
        /// <param name="id">The ID of the creature / hero / trap.</param>
        public void AddStartTurnEntry(Guid id)
        {
            if (!_fActive)
                return;

            var entry = new StartTurnLogEntry();
            entry.CombatantId = id;
            _fEntries.Add(entry);
        }

        /// <summary>
        ///     Adds a DamageLogEntry to the log.
        /// </summary>
        /// <param name="id">The ID of the creature / hero.</param>
        /// <param name="damage">The amount of damage (or healing if negative).</param>
        /// <param name="types">The damage type.</param>
        public void AddDamageEntry(Guid id, int damage, List<DamageType> types)
        {
            if (!_fActive)
                return;

            var entry = new DamageLogEntry();
            entry.CombatantId = id;
            entry.Amount = damage;
            entry.Types = types;
            _fEntries.Add(entry);
        }

        /// <summary>
        ///     Adds a StateLogEntry to the log.
        /// </summary>
        /// <param name="id">The ID of the creature / hero.</param>
        /// <param name="state">The new state.</param>
        public void AddStateEntry(Guid id, CreatureState state)
        {
            if (!_fActive)
                return;

            var entry = new StateLogEntry();
            entry.CombatantId = id;
            entry.State = state;
            _fEntries.Add(entry);
        }

        /// <summary>
        ///     Adds an EffectLogEntry to the log.
        /// </summary>
        /// <param name="id">The ID of the creature / hero.</param>
        /// <param name="text">The text of the effect.</param>
        /// <param name="added">True if the effect is being added; false if it's being removed.</param>
        public void AddEffectEntry(Guid id, string text, bool added)
        {
            if (!_fActive)
                return;

            var entry = new EffectLogEntry();
            entry.CombatantId = id;
            entry.EffectText = text;
            entry.Added = added;
            _fEntries.Add(entry);
        }

        /// <summary>
        ///     Adds a PowerLogEntry to the log.
        /// </summary>
        /// <param name="id">The ID of the creature / hero.</param>
        /// <param name="text">The name of the power.</param>
        /// <param name="added">True if the power is being used; false if it's being recharged.</param>
        public void AddPowerEntry(Guid id, string text, bool added)
        {
            if (!_fActive)
                return;

            var entry = new PowerLogEntry();
            entry.CombatantId = id;
            entry.PowerName = text;
            entry.Added = added;
            _fEntries.Add(entry);
        }

        /// <summary>
        ///     Adds a SkillLogEntry to the log.
        /// </summary>
        /// <param name="id">The ID of the creature / hero.</param>
        /// <param name="text">The name of the skill.</param>
        public void AddSkillEntry(Guid id, string text)
        {
            if (!_fActive)
                return;

            var entry = new SkillLogEntry();
            entry.CombatantId = id;
            entry.SkillName = text;
            _fEntries.Add(entry);
        }

        /// <summary>
        ///     Adds a SkillChallengeLogEntry to the log.
        /// </summary>
        /// <param name="id">The ID of the creature / hero.</param>
        /// <param name="success">True for a success; false for a failure.</param>
        public void AddSkillChallengeEntry(Guid id, bool success)
        {
            if (!_fActive)
                return;

            var entry = new SkillChallengeLogEntry();
            entry.CombatantId = id;
            entry.Success = success;
            _fEntries.Add(entry);
        }

        /// <summary>
        ///     Adds a MoveEntry to the log.
        /// </summary>
        /// <param name="id">The ID of the token.</param>
        /// <param name="distance">The distance moved.</param>
        /// <param name="text">Any additional details.</param>
        public void AddMoveEntry(Guid id, int distance, string text)
        {
            if (!_fActive)
                return;

            var entry = new MoveLogEntry();
            entry.CombatantId = id;
            entry.Distance = distance;
            entry.Details = text;
            _fEntries.Add(entry);
        }

        /// <summary>
        ///     Adds a PauseLogEntry to the log.
        /// </summary>
        public void AddPauseEntry()
        {
            if (!_fActive)
                return;

            var entry = new PauseLogEntry();
            _fEntries.Add(entry);
        }

        /// <summary>
        ///     Adds a ResumeLogEntry to the log.
        /// </summary>
        public void AddResumeEntry()
        {
            if (!_fActive)
                return;

            var entry = new ResumeLogEntry();
            _fEntries.Add(entry);
        }

        internal EncounterReport CreateReport(Encounter enc, bool allEntries)
        {
            var report = new EncounterReport();

            RoundLog round = null;
            TurnLog turn = null;
            foreach (var entry in _fEntries)
            {
                var startRound = entry as StartRoundLogEntry;
                var startTurn = entry as StartTurnLogEntry;
                if (startRound != null)
                {
                    if (round != null)
                        report.Rounds.Add(round);

                    round = new RoundLog(startRound.Round);
                }
                else if (startTurn != null)
                {
                    if (turn != null)
                    {
                        turn.End = startTurn.Timestamp;
                        round.Turns.Add(turn);
                    }

                    turn = new TurnLog(startTurn.CombatantId);
                    turn.Start = startTurn.Timestamp;
                }
                else
                {
                    if (allEntries || entry.Important)
                        turn.Entries.Add(entry);
                }
            }

            if (round != null)
            {
                if (turn != null)
                {
                    if (turn.Entries.Count != 0)
                        turn.End = turn.Entries[turn.Entries.Count - 1].Timestamp;

                    round.Turns.Add(turn);
                }

                report.Rounds.Add(round);
            }

            return report;
        }

        internal static string GetName(Guid id, Encounter enc, bool detailed)
        {
            // Creature / NPC
            var cd = enc.FindCombatData(id);
            if (cd != null)
            {
                if (detailed)
                    return cd.DisplayName;

                var slot = enc.FindSlot(cd);
                if (slot != null)
                {
                    var c = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                    if (c != null && c.Category != "")
                        return c.Category;
                }
            }

            // Hero
            var hero = Session.Project.FindHero(id);
            if (hero != null)
                return hero.Name;

            // Trap
            var trap = enc.FindTrap(id);
            if (trap != null)
                return trap.Name;

            return "Creature";
        }
    }

    /// <summary>
    ///     Interface for encounter log entries.
    /// </summary>
    public interface IEncounterLogEntry
    {
        /// <summary>
        ///     Gets the ID of the creature, hero or trap.
        /// </summary>
        Guid CombatantId { get; }

        /// <summary>
        ///     Gets the timestamp for this entry.
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        bool Important { get; }

        /// <summary>
        ///     Gets the HTML text description of the entry.
        /// </summary>
        /// <param name="enc">The encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns a text description of this entry.</returns>
        string Description(Encounter enc, bool detailed);
    }

    /// <summary>
    ///     Log entry for the start of a round.
    /// </summary>
    [Serializable]
    public class StartRoundLogEntry : IEncounterLogEntry
    {
        private int _fRound = 1;

        private DateTime _fTimestamp = DateTime.Now;

        /// <summary>
        ///     Gets or sets the round.
        /// </summary>
        public int Round
        {
            get => _fRound;
            set => _fRound = value;
        }

        /// <summary>
        ///     Gets the ID of the combatant.
        /// </summary>
        public Guid CombatantId => Guid.Empty;

        /// <summary>
        ///     Gets or sets the entry timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets the HTML string description of the entry.
        /// </summary>
        /// <param name="enc">The current encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns the string description of the entry.</returns>
        public string Description(Encounter enc, bool detailed)
        {
            return "Round " + _fRound;
        }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        public bool Important => true;
    }

    /// <summary>
    ///     Log entry for the start of a turn.
    /// </summary>
    [Serializable]
    public class StartTurnLogEntry : IEncounterLogEntry
    {
        private Guid _fId = Guid.Empty;

        private DateTime _fTimestamp = DateTime.Now;

        /// <summary>
        ///     Gets or sets the ID of the combatant.
        /// </summary>
        public Guid CombatantId
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the entry timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets the HTML string description of the entry.
        /// </summary>
        /// <param name="enc">The current encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns the string description of the entry.</returns>
        public string Description(Encounter enc, bool detailed)
        {
            return "Start turn: " + EncounterLog.GetName(_fId, enc, detailed);
        }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        public bool Important => true;
    }

    /// <summary>
    ///     Log entry for damage taken / healed.
    /// </summary>
    [Serializable]
    public class DamageLogEntry : IEncounterLogEntry
    {
        private int _fAmount;

        private Guid _fId = Guid.Empty;

        private DateTime _fTimestamp = DateTime.Now;

        private List<DamageType> _fTypes = new List<DamageType>();

        /// <summary>
        ///     Gets or sets the amount.
        /// </summary>
        public int Amount
        {
            get => _fAmount;
            set => _fAmount = value;
        }

        /// <summary>
        ///     Gets or sets the damage type(s).
        /// </summary>
        public List<DamageType> Types
        {
            get => _fTypes;
            set => _fTypes = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the combatant.
        /// </summary>
        public Guid CombatantId
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the entry timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets the HTML string description of the entry.
        /// </summary>
        /// <param name="enc">The current encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns the string description of the entry.</returns>
        public string Description(Encounter enc, bool detailed)
        {
            var types = "";
            if (_fTypes != null)
                foreach (var type in _fTypes)
                {
                    types += " ";
                    types += type.ToString().ToLower();
                }

            var verb = _fAmount >= 0 ? "takes" : "heals";
            return EncounterLog.GetName(_fId, enc, detailed) + " " + verb + " " + Math.Abs(_fAmount) + types +
                   " damage";
        }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        public bool Important => false;
    }

    /// <summary>
    ///     Log entry for a change in active / bloodied / defeated state.
    /// </summary>
    [Serializable]
    public class StateLogEntry : IEncounterLogEntry
    {
        private Guid _fId = Guid.Empty;

        private CreatureState _fState = CreatureState.Active;

        private DateTime _fTimestamp = DateTime.Now;

        /// <summary>
        ///     Gets or sets the new state.
        /// </summary>
        public CreatureState State
        {
            get => _fState;
            set => _fState = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the combatant.
        /// </summary>
        public Guid CombatantId
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the entry timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets the HTML string description of the entry.
        /// </summary>
        /// <param name="enc">The current encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns the string description of the entry.</returns>
        public string Description(Encounter enc, bool detailed)
        {
            var state = "not bloodied";
            if (_fState != CreatureState.Active)
                state = _fState.ToString().ToLower();

            return EncounterLog.GetName(_fId, enc, detailed) + " is <B>" + state + "</B>";
        }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        public bool Important => true;
    }

    /// <summary>
    ///     Log entry for the addition or removal of an effect.
    /// </summary>
    [Serializable]
    public class EffectLogEntry : IEncounterLogEntry
    {
        private bool _fAdded = true;

        private string _fEffectText = "";

        private Guid _fId = Guid.Empty;

        private DateTime _fTimestamp = DateTime.Now;

        /// <summary>
        ///     Gets or sets the text of the effect.
        /// </summary>
        public string EffectText
        {
            get => _fEffectText;
            set => _fEffectText = value;
        }

        /// <summary>
        ///     True if the effect has been added; false if the effect has been removed.
        /// </summary>
        public bool Added
        {
            get => _fAdded;
            set => _fAdded = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the combatant.
        /// </summary>
        public Guid CombatantId
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the entry timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets the HTML string description of the entry.
        /// </summary>
        /// <param name="enc">The current encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns the string description of the entry.</returns>
        public string Description(Encounter enc, bool detailed)
        {
            var name = EncounterLog.GetName(_fId, enc, detailed);
            if (_fAdded)
                return name + " gained " + _fEffectText;
            return name + " lost " + _fEffectText;
        }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        public bool Important => false;
    }

    /// <summary>
    ///     Log entry for the usage of a power.
    /// </summary>
    [Serializable]
    public class PowerLogEntry : IEncounterLogEntry
    {
        private bool _fAdded = true;

        private Guid _fId = Guid.Empty;

        private string _fPowerName = "";

        private DateTime _fTimestamp = DateTime.Now;

        /// <summary>
        ///     Gets or sets the power name.
        /// </summary>
        public string PowerName
        {
            get => _fPowerName;
            set => _fPowerName = value;
        }

        /// <summary>
        ///     True if the power has been used; false if the power has been recharged.
        /// </summary>
        public bool Added
        {
            get => _fAdded;
            set => _fAdded = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the combatant.
        /// </summary>
        public Guid CombatantId
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the entry timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets the HTML string description of the entry.
        /// </summary>
        /// <param name="enc">The current encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns the string description of the entry.</returns>
        public string Description(Encounter enc, bool detailed)
        {
            var name = EncounterLog.GetName(_fId, enc, detailed);
            if (_fAdded)
                return name + " used <B>" + _fPowerName + "</B>";
            return name + " recharged <B>" + _fPowerName + "</B>";
        }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        public bool Important => false;
    }

    /// <summary>
    ///     Log entry for a skill usage.
    /// </summary>
    [Serializable]
    public class SkillLogEntry : IEncounterLogEntry
    {
        private Guid _fId = Guid.Empty;

        private string _fSkillName = "";

        private DateTime _fTimestamp = DateTime.Now;

        /// <summary>
        ///     Gets or sets the skill name.
        /// </summary>
        public string SkillName
        {
            get => _fSkillName;
            set => _fSkillName = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the combatant.
        /// </summary>
        public Guid CombatantId
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the entry timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets the HTML string description of the entry.
        /// </summary>
        /// <param name="enc">The current encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns the string description of the entry.</returns>
        public string Description(Encounter enc, bool detailed)
        {
            var name = EncounterLog.GetName(_fId, enc, detailed);
            return name + " used <B>" + _fSkillName + "</B>";
        }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        public bool Important => false;
    }

    /// <summary>
    ///     Log entry for a skill in a skill challenge.
    /// </summary>
    [Serializable]
    public class SkillChallengeLogEntry : IEncounterLogEntry
    {
        private Guid _fId = Guid.Empty;

        private bool _fSuccess = true;

        private DateTime _fTimestamp = DateTime.Now;

        /// <summary>
        ///     True if the skill was a success; false if it was a failure.
        /// </summary>
        public bool Success
        {
            get => _fSuccess;
            set => _fSuccess = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the combatant.
        /// </summary>
        public Guid CombatantId
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the entry timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets the HTML string description of the entry.
        /// </summary>
        /// <param name="enc">The current encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns the string description of the entry.</returns>
        public string Description(Encounter enc, bool detailed)
        {
            var name = EncounterLog.GetName(_fId, enc, detailed);
            if (_fSuccess)
                return name + " gained a success";
            return name + " incurred a failure";
        }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        public bool Important => false;
    }

    /// <summary>
    ///     Log entry for a map movement.
    /// </summary>
    [Serializable]
    public class MoveLogEntry : IEncounterLogEntry
    {
        private string _fDetails = "";

        private int _fDistance;

        private Guid _fId = Guid.Empty;

        private DateTime _fTimestamp = DateTime.Now;

        /// <summary>
        ///     Gets or sets any additional details about the movement.
        /// </summary>
        public int Distance
        {
            get => _fDistance;
            set => _fDistance = value;
        }

        /// <summary>
        ///     Gets or sets any additional details about the movement.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the combatant.
        /// </summary>
        public Guid CombatantId
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the entry timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets the HTML string description of the entry.
        /// </summary>
        /// <param name="enc">The current encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns the string description of the entry.</returns>
        public string Description(Encounter enc, bool detailed)
        {
            var name = EncounterLog.GetName(_fId, enc, detailed);
            var str = name + " moves";
            if (_fDistance > 0)
                str += " " + _fDistance + " sq";
            if (_fDetails != "")
                str += " " + _fDetails.Trim();
            return str;
        }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        public bool Important => false;
    }

    /// <summary>
    ///     Log entry for pausing an encounter.
    /// </summary>
    [Serializable]
    public class PauseLogEntry : IEncounterLogEntry
    {
        private DateTime _fTimestamp = DateTime.Now;

        /// <summary>
        ///     Gets the ID of the combatant.
        /// </summary>
        public Guid CombatantId => Guid.Empty;

        /// <summary>
        ///     Gets or sets the entry timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets the HTML string description of the entry.
        /// </summary>
        /// <param name="enc">The current encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns the string description of the entry.</returns>
        public string Description(Encounter enc, bool detailed)
        {
            return "Paused (" + _fTimestamp.ToShortTimeString() + " " + _fTimestamp.ToShortDateString() + ")";
        }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        public bool Important => false;
    }

    /// <summary>
    ///     Log enry for resuming a paused encounter.
    /// </summary>
    [Serializable]
    public class ResumeLogEntry : IEncounterLogEntry
    {
        private DateTime _fTimestamp = DateTime.Now;

        /// <summary>
        ///     Gets the ID of the combatant.
        /// </summary>
        public Guid CombatantId => Guid.Empty;

        /// <summary>
        ///     Gets or sets the entry timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get => _fTimestamp;
            set => _fTimestamp = value;
        }

        /// <summary>
        ///     Gets the HTML string description of the entry.
        /// </summary>
        /// <param name="enc">The current encounter.</param>
        /// <param name="detailed">True if the description should include detailed creature names; false otherwise.</param>
        /// <returns>Returns the string description of the entry.</returns>
        public string Description(Encounter enc, bool detailed)
        {
            return "Resumed (" + _fTimestamp.ToShortTimeString() + " " + _fTimestamp.ToShortDateString() + ")";
        }

        /// <summary>
        ///     Gets whether the entry should always be shown.
        /// </summary>
        public bool Important => false;
    }

    internal class EncounterReport
    {
        public List<RoundLog> Rounds { get; } = new List<RoundLog>();

        public List<Guid> Combatants
        {
            get
            {
                var ids = new List<Guid>();

                foreach (var round in Rounds)
                foreach (var turn in round.Turns)
                    if (!ids.Contains(turn.Id))
                        ids.Add(turn.Id);

                return ids;
            }
        }

        public RoundLog GetRound(int round)
        {
            foreach (var rl in Rounds)
                if (rl.Round == round)
                    return rl;

            return null;
        }

        private List<TurnLog> get_turns(Guid id)
        {
            var turns = new List<TurnLog>();

            foreach (var round in Rounds)
            foreach (var turn in round.Turns)
                if (turn.Id == id)
                    turns.Add(turn);

            return turns;
        }

        public List<Guid> MvPs(Encounter enc)
        {
            var standings = new Dictionary<Guid, int>();

            // Qickest mean times
            var timeTable = CreateTable(ReportType.Time, BreakdownType.Controller, enc);
            timeTable.ReduceToPCs();
            add_table(timeTable, standings);

            // Most damage to enemies
            var enemyTable = CreateTable(ReportType.DamageToEnemies, BreakdownType.Controller, enc);
            enemyTable.ReduceToPCs();
            add_table(enemyTable, standings);

            // Least damage to allies
            var allyTable = CreateTable(ReportType.DamageToAllies, BreakdownType.Controller, enc);
            allyTable.ReduceToPCs();
            add_table(allyTable, standings);

            var leaders = new List<Guid>();
            var max = int.MinValue;

            foreach (var combatant in standings.Keys)
            {
                var total = standings[combatant];

                if (total > max)
                {
                    max = total;
                    leaders.Clear();
                }

                if (total == max) leaders.Add(combatant);
            }

            return leaders;
        }

        private void add_table(ReportTable table, Dictionary<Guid, int> standings)
        {
            var points = new List<int> { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1 };

            var values = new List<int>();
            foreach (var row in table.Rows)
                if (!values.Contains(row.Total))
                    values.Add(row.Total);

            var allocations = new Dictionary<Guid, int>();

            foreach (var value in values)
            {
                var pointsForThisValue = 0;
                if (allocations.Count < points.Count)
                    pointsForThisValue = points[allocations.Count];

                foreach (var row in table.Rows)
                {
                    if (row.Total != value)
                        continue;

                    allocations[row.CombatantId] = pointsForThisValue;
                }
            }

            foreach (var combatant in allocations.Keys)
            {
                if (!standings.ContainsKey(combatant))
                    standings[combatant] = 0;

                standings[combatant] += allocations[combatant];
            }
        }

        public int Time(Guid id, int round)
        {
            var ts = new TimeSpan();

            foreach (var rl in Rounds)
                if (rl.Round == round || round == 0)
                    foreach (var tl in rl.Turns)
                        if (tl.Id == id || id == Guid.Empty)
                            ts += tl.Time();

            return (int)ts.TotalSeconds;
        }

        public int Damage(Guid id, int round, bool allies, Encounter enc)
        {
            var damage = 0;

            foreach (var rl in Rounds)
                if (rl.Round == round || round == 0)
                    foreach (var tl in rl.Turns)
                        if (tl.Id == id || id == Guid.Empty)
                        {
                            var allyIDs = get_allies(tl.Id, enc);

                            var ids = new List<Guid>();
                            if (allies)
                                // Use the list of allies
                                ids.AddRange(allyIDs);
                            else
                                // Reverse the list
                                foreach (var i in Combatants)
                                    if (!allyIDs.Contains(i))
                                        ids.Add(i);

                            damage += tl.Damage(ids);
                        }

            return damage;
        }

        public int Movement(Guid id, int round)
        {
            var movement = 0;

            foreach (var rl in Rounds)
                if (rl.Round == round || round == 0)
                    foreach (var tl in rl.Turns)
                        if (tl.Id == id || id == Guid.Empty)
                            movement += tl.Movement();

            return movement;
        }

        private static List<Guid> get_allies(Guid id, Encounter enc)
        {
            var allyIDs = new List<Guid>();

            if (Session.Project.FindHero(id) != null)
            {
                // All heroes
                foreach (var hero in Session.Project.Heroes)
                    allyIDs.Add(hero.Id);

                // All allied creatures
                foreach (var slot in enc.Slots)
                {
                    if (slot.Type != EncounterSlotType.Ally)
                        continue;

                    foreach (var cd in slot.CombatData)
                        allyIDs.Add(cd.Id);
                }
            }
            else
            {
                // Get faction
                var cd = enc.FindCombatData(id);
                if (cd != null)
                {
                    var slot = enc.FindSlot(cd);
                    if (slot != null)
                    {
                        // All of same faction
                        foreach (var s in enc.Slots)
                        {
                            if (s.Type != slot.Type)
                                continue;

                            foreach (var c in s.CombatData)
                                allyIDs.Add(c.Id);
                        }

                        // If ally, all heroes
                        if (slot.Type == EncounterSlotType.Ally)
                            foreach (var hero in Session.Project.Heroes)
                                allyIDs.Add(hero.Id);
                    }
                }
            }

            return allyIDs;
        }

        public ReportTable CreateTable(ReportType reportType, BreakdownType breakdownType, Encounter enc)
        {
            var table = new ReportTable();
            table.ReportType = reportType;
            table.BreakdownType = breakdownType;

            var rowsets = new List<Pair<string, List<Guid>>>();
            switch (breakdownType)
            {
                case BreakdownType.Individual:
                {
                    // Add individually

                    var combatantIds = Combatants;
                    foreach (var id in combatantIds)
                    {
                        var list = new List<Guid>();
                        list.Add(id);
                        rowsets.Add(new Pair<string, List<Guid>>(enc.WhoIs(id), list));
                    }
                }
                    break;
                case BreakdownType.Controller:
                {
                    // Add by controller (PCs, DM)

                    var dmIds = new List<Guid>();

                    var combatantIds = Combatants;
                    foreach (var id in combatantIds)
                        if (Session.Project.FindHero(id) != null)
                        {
                            var list = new List<Guid>();
                            list.Add(id);
                            rowsets.Add(new Pair<string, List<Guid>>(enc.WhoIs(id), list));
                        }
                        else
                        {
                            dmIds.Add(id);
                        }

                    rowsets.Add(new Pair<string, List<Guid>>("DM", dmIds));
                }
                    break;
                case BreakdownType.Faction:
                {
                    // Add by faction (PC, ally, neutral, enemy)

                    var pcIds = new List<Guid>();
                    var allyIds = new List<Guid>();
                    var neutralIds = new List<Guid>();
                    var enemyIds = new List<Guid>();

                    var combatantIds = Combatants;
                    foreach (var id in combatantIds)
                        if (Session.Project.FindHero(id) != null)
                        {
                            pcIds.Add(id);
                        }
                        else
                        {
                            var cd = enc.FindCombatData(id);
                            var slot = enc.FindSlot(cd);
                            switch (slot.Type)
                            {
                                case EncounterSlotType.Ally:
                                    allyIds.Add(id);
                                    break;
                                case EncounterSlotType.Neutral:
                                    neutralIds.Add(id);
                                    break;
                                case EncounterSlotType.Opponent:
                                    enemyIds.Add(id);
                                    break;
                            }
                        }

                    rowsets.Add(new Pair<string, List<Guid>>("PCs", pcIds));
                    rowsets.Add(new Pair<string, List<Guid>>("Allies", allyIds));
                    rowsets.Add(new Pair<string, List<Guid>>("Neutral", neutralIds));
                    rowsets.Add(new Pair<string, List<Guid>>("Enemies", enemyIds));
                }
                    break;
            }

            foreach (var rowset in rowsets)
            {
                if (rowset.Second.Count == 0)
                    continue;

                var row = new ReportRow();
                row.Heading = rowset.First;

                if (rowset.Second.Count == 1)
                    row.CombatantId = rowset.Second[0];

                for (var round = 1; round <= Rounds.Count; ++round)
                    switch (reportType)
                    {
                        case ReportType.Time:
                        {
                            var total = 0;
                            foreach (var id in rowset.Second)
                                total += Time(id, round);
                            row.Values.Add(total);
                        }
                            break;
                        case ReportType.DamageToEnemies:
                        {
                            var total = 0;
                            foreach (var id in rowset.Second)
                                total += Damage(id, round, false, enc);
                            row.Values.Add(total);
                        }
                            break;
                        case ReportType.DamageToAllies:
                        {
                            var total = 0;
                            foreach (var id in rowset.Second)
                                total += Damage(id, round, true, enc);
                            row.Values.Add(total);
                        }
                            break;
                        case ReportType.Movement:
                        {
                            var total = 0;
                            foreach (var id in rowset.Second)
                                total += Movement(id, round);
                            row.Values.Add(total);
                        }
                            break;
                    }

                table.Rows.Add(row);
            }

            table.Rows.Sort();

            switch (table.ReportType)
            {
                case ReportType.Time:
                case ReportType.DamageToAllies:
                    // For these reports, lower numbers are better
                    table.Rows.Reverse();
                    break;
            }

            return table;
        }
    }

    internal class RoundLog
    {
        public int Round { get; }

        public List<TurnLog> Turns { get; } = new List<TurnLog>();

        public int Count
        {
            get
            {
                var count = 0;
                foreach (var turn in Turns)
                    count += turn.Entries.Count;

                return count;
            }
        }

        public RoundLog(int round)
        {
            Round = round;
        }

        public TurnLog GetTurn(Guid id)
        {
            foreach (var tl in Turns)
                if (tl.Id == id)
                    return tl;

            return null;
        }
    }

    internal class TurnLog
    {
        public DateTime End = DateTime.MinValue;

        public DateTime Start = DateTime.MinValue;

        public Guid Id { get; } = Guid.Empty;

        public List<IEncounterLogEntry> Entries { get; } = new List<IEncounterLogEntry>();

        public TurnLog(Guid id)
        {
            Id = id;
        }

        public TimeSpan Time()
        {
            var time = End - Start;

            if (time.Ticks < 0)
                return new TimeSpan(0);

            IEncounterLogEntry pauseStart = null;
            foreach (var entry in Entries)
            {
                if (entry is PauseLogEntry)
                {
                    pauseStart = entry;
                    continue;
                }

                if (entry is ResumeLogEntry)
                    if (pauseStart != null)
                    {
                        var pause = entry.Timestamp - pauseStart.Timestamp;
                        time -= pause;

                        pauseStart = null;
                    }
            }

            return time;
        }

        public int Damage(List<Guid> allyIDs)
        {
            var damage = 0;

            foreach (var entry in Entries)
            {
                var dle = entry as DamageLogEntry;
                if (dle != null)
                    if (allyIDs.Contains(dle.CombatantId))
                        damage += dle.Amount;
            }

            return damage;
        }

        public int Movement()
        {
            var movement = 0;

            foreach (var entry in Entries)
            {
                var mle = entry as MoveLogEntry;
                if (mle != null)
                    if (mle.Distance > 0)
                        movement += mle.Distance;
            }

            return movement;
        }
    }

    internal enum ReportType
    {
        Time,
        DamageToEnemies,
        DamageToAllies,
        Movement
    }

    internal enum BreakdownType
    {
        Individual,
        Controller,
        Faction
    }

    internal class ReportTable
    {
        public ReportType ReportType { get; set; } = ReportType.Time;

        public BreakdownType BreakdownType { get; set; } = BreakdownType.Individual;

        public List<ReportRow> Rows { get; } = new List<ReportRow>();

        public int Rounds
        {
            get
            {
                var max = 0;

                foreach (var row in Rows)
                    max = Math.Max(max, row.Values.Count);

                return max;
            }
        }

        public int GrandTotal
        {
            get
            {
                var total = 0;

                foreach (var row in Rows)
                    total += row.Total;

                return total;
            }
        }

        public int ColumnTotal(int column)
        {
            var total = 0;

            foreach (var row in Rows)
                total += row.Values[column];

            return total;
        }

        public void ReduceToPCs()
        {
            var obsolete = new List<ReportRow>();
            foreach (var row in Rows)
            {
                var hero = Session.Project.FindHero(row.CombatantId);
                if (hero == null)
                    obsolete.Add(row);
            }

            foreach (var row in obsolete)
                Rows.Remove(row);
        }
    }

    internal class ReportRow : IComparable<ReportRow>
    {
        public string Heading { get; set; } = "";

        public Guid CombatantId { get; set; } = Guid.Empty;

        public List<int> Values { get; } = new List<int>();

        public int Total
        {
            get
            {
                var total = 0;

                foreach (var value in Values)
                    total += value;

                return total;
            }
        }

        public double Average => (double)Total / Values.Count;

        public int CompareTo(ReportRow rhs)
        {
            return Total.CompareTo(rhs.Total) * -1;
        }
    }
}
