using System;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Quest type (major or minor).
    /// </summary>
    public enum QuestType
    {
        /// <summary>
        ///     A major quest.
        /// </summary>
        Major,

        /// <summary>
        ///     A minor quest.
        /// </summary>
        Minor
    }

    /// <summary>
    ///     Class representing a quest element.
    /// </summary>
    [Serializable]
    public class Quest : IElement
    {
        private int _fLevel = Session.Project.Party.Level;

        private QuestType _fType = QuestType.Minor;

        private int _fXp;

        /// <summary>
        ///     The level of the quest.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     The type of quest.
        /// </summary>
        public QuestType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the XP value for the quest, to be awarded to each party member.
        ///     If the quest is a major quest, this value is can't be set using this property; major quests have a set XP value.
        /// </summary>
        public int Xp
        {
            get
            {
                switch (_fType)
                {
                    case QuestType.Major:
                        return Experience.GetCreatureXp(_fLevel);
                    case QuestType.Minor:
                        return _fXp;
                }

                return int.MinValue;
            }
            set
            {
                if (_fType == QuestType.Minor)
                    _fXp = value;
            }
        }

        /// <summary>
        ///     Calculates the XP value of the quest.
        /// </summary>
        /// <returns>Returns the XP value.</returns>
        public int GetXp()
        {
            // Each party member gets the full XP amount
            return Xp * Session.Project.Party.Size;
        }

        /// <summary>
        ///     Calculates the difficulty of the quest.
        /// </summary>
        /// <param name="party_level">The party level.</param>
        /// <param name="party_size">The party size.</param>
        /// <returns>Returns the difficulty.</returns>
        public Difficulty GetDifficulty(int partyLevel, int partySize)
        {
            var p = new Party();
            p.Level = partyLevel;
            p.Size = partySize;

            return p.GetDifficulty(_fLevel);
        }

        /// <summary>
        ///     Creates a copy of the Quest.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public IElement Copy()
        {
            var q = new Quest();

            q.Level = _fLevel;
            q.Type = _fType;
            q.Xp = _fXp;

            return q;
        }
    }
}
