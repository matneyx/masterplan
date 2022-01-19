using System;
using System.Collections.Generic;

namespace Masterplan.Data
{
    /// <summary>
    ///     Ongoing condition / damage.
    /// </summary>
    public enum OngoingType
    {
        /// <summary>
        ///     Ongoing condition.
        /// </summary>
        Condition,

        /// <summary>
        ///     Ongoing damage.
        /// </summary>
        Damage,

        /// <summary>
        ///     Modifier to defences.
        /// </summary>
        DefenceModifier,

        /// <summary>
        ///     Damage resistance, vulnerability or immunity.
        /// </summary>
        DamageModifier,

        /// <summary>
        ///     Regeneration.
        /// </summary>
        Regeneration,

        /// <summary>
        ///     An aura.
        /// </summary>
        Aura
    }

    /// <summary>
    ///     Specifies how an ongoing condition can end.
    /// </summary>
    public enum DurationType
    {
        /// <summary>
        ///     Lasts for the duration of the encounter.
        /// </summary>
        Encounter,

        /// <summary>
        ///     Lasts until a successful save is made.
        /// </summary>
        SaveEnds,

        /// <summary>
        ///     Lasts until the beginning of a creature's / PC's turn.
        /// </summary>
        BeginningOfTurn,

        /// <summary>
        ///     Lasts until the end of a creature's / PC's turn.
        /// </summary>
        EndOfTurn
    }

    /// <summary>
    ///     Class representing an ongoing combat effect.
    /// </summary>
    [Serializable]
    public class OngoingCondition : IComparable<OngoingCondition>
    {
        private Aura _fAura = new Aura();

        private DamageModifier _fDamageModifier = new DamageModifier();

        private DamageType _fDamageType = DamageType.Untyped;

        private string _fData = "";

        private int _fDefenceMod = 2;

        private List<DefenceType> _fDefences = new List<DefenceType>();

        private DurationType _fDuration = DurationType.SaveEnds;

        private Guid _fDurationCreatureId = Guid.Empty;

        private int _fDurationRound = int.MinValue;

        private Regeneration _fRegeneration = new Regeneration();

        private int _fSavingThrowModifier;

        private OngoingType _fType = OngoingType.Condition;

        private int _fValue = 2;

        /// <summary>
        ///     Gets or sets the type of condition.
        /// </summary>
        public OngoingType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the condition.
        /// </summary>
        public string Data
        {
            get => _fData;
            set => _fData = value;
        }

        /// <summary>
        ///     Gets or sets the type of the damage.
        /// </summary>
        public DamageType DamageType
        {
            get => _fDamageType;
            set => _fDamageType = value;
        }

        /// <summary>
        ///     Gets or sets the value of the damage.
        /// </summary>
        public int Value
        {
            get => _fValue;
            set => _fValue = value;
        }

        /// <summary>
        ///     Gets or sets the value of the defence modifier.
        /// </summary>
        public int DefenceMod
        {
            get => _fDefenceMod;
            set => _fDefenceMod = value;
        }

        /// <summary>
        ///     Gets or sets the defences to be modified.
        /// </summary>
        public List<DefenceType> Defences
        {
            get => _fDefences;
            set => _fDefences = value;
        }

        /// <summary>
        ///     Gets or sets the regeneration.
        /// </summary>
        public Regeneration Regeneration
        {
            get => _fRegeneration;
            set => _fRegeneration = value;
        }

        /// <summary>
        ///     Gets or sets the damage modifier.
        /// </summary>
        public DamageModifier DamageModifier
        {
            get => _fDamageModifier;
            set => _fDamageModifier = value;
        }

        /// <summary>
        ///     Gets or sets the aura.
        /// </summary>
        public Aura Aura
        {
            get => _fAura;
            set => _fAura = value;
        }

        /// <summary>
        ///     Gets or sets the duration of the condition.
        /// </summary>
        public DurationType Duration
        {
            get => _fDuration;
            set => _fDuration = value;
        }

        /// <summary>
        ///     Gets or sets the creature the condition is dependent on.
        ///     This is one of the following:
        ///     The ID of the CombatData representing the creature
        ///     The ID of the Hero representing the PC
        ///     The ID of the Trap
        /// </summary>
        public Guid DurationCreatureId
        {
            get => _fDurationCreatureId;
            set => _fDurationCreatureId = value;
        }

        /// <summary>
        ///     Gets or sets the minimum round on which durations will end.
        ///     This is used for beginning of turn / end of turn durations.
        /// </summary>
        public int DurationRound
        {
            get => _fDurationRound;
            set => _fDurationRound = value;
        }

        /// <summary>
        ///     Gets or sets the saving throw modifier.
        /// </summary>
        public int SavingThrowModifier
        {
            get => _fSavingThrowModifier;
            set => _fSavingThrowModifier = value;
        }

        /// <summary>
        ///     Returns the duration of the effect as a string.
        /// </summary>
        /// <param name="enc">The encounter.</param>
        /// <returns></returns>
        public string GetDuration(Encounter enc)
        {
            var str = "";

            switch (_fDuration)
            {
                case DurationType.Encounter:
                    // Effectively, does not end
                    break;
                case DurationType.SaveEnds:
                {
                    str = "save ends";

                    if (SavingThrowModifier != 0)
                    {
                        var sign = SavingThrowModifier >= 0 ? "+" : "";
                        str += " with " + sign + SavingThrowModifier + " mod";
                    }
                }
                    break;
                case DurationType.BeginningOfTurn:
                {
                    var name = "";
                    if (_fDurationCreatureId == Guid.Empty)
                    {
                        name = "someone else's";
                    }
                    else
                    {
                        if (enc != null)
                            name = enc.WhoIs(_fDurationCreatureId) + "'s";
                        else
                            name = "my";
                    }

                    str += "until the start of " + name + " next turn";
                }
                    break;
                case DurationType.EndOfTurn:
                {
                    var name = "";
                    if (_fDurationCreatureId == Guid.Empty)
                    {
                        name = "someone else's";
                    }
                    else
                    {
                        if (enc != null)
                            name = enc.WhoIs(_fDurationCreatureId) + "'s";
                        else
                            name = "my";
                    }

                    str += "until the end of " + name + " next turn";
                }
                    break;
            }

            return str;
        }

        /// <summary>
        ///     [blinded / marked / etc]
        ///     or
        ///     N ongoing [fire / cold etc] damage
        ///     or
        ///     +N to [defences]
        ///     or
        ///     Regeneration N
        ///     or
        ///     [Resist / Vulnerable / Immune] N [fire / cold etc]
        ///     plus end condition data
        /// </summary>
        /// <param name="enc">The encounter.</param>
        /// <param name="html">Whether the string should include HTML tags.</param>
        /// <returns></returns>
        public string ToString(Encounter enc, bool html)
        {
            var str = ToString();

            if (html)
                str = "<B>" + str + "</B>";

            var duration = GetDuration(enc);
            if (duration != "")
                str += " (" + duration + ")";

            return str;
        }

        /// <summary>
        ///     [blinded / marked / etc]
        ///     or
        ///     N ongoing [fire / cold etc] damage
        ///     or
        ///     +N to [defences]
        ///     or
        ///     Regeneration N
        ///     or
        ///     [Resist / Vulnerable / Immune] N [fire / cold etc]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var str = "";

            switch (_fType)
            {
                case OngoingType.Condition:
                    str = _fData;
                    break;
                case OngoingType.Damage:
                {
                    if (_fDamageType == DamageType.Untyped)
                    {
                        str = _fValue + " ongoing damage";
                    }
                    else
                    {
                        var dmg = _fDamageType.ToString().ToLower();
                        str = _fValue + " ongoing " + dmg + " damage";
                    }
                }
                    break;
                case OngoingType.DefenceModifier:
                {
                    str = _fDefenceMod.ToString();
                    if (_fDefenceMod >= 0)
                        str = "+" + str;

                    var defences = "";
                    if (_fDefences.Count == 4)
                        defences = "defences";
                    else
                        foreach (var type in _fDefences)
                        {
                            if (defences != "")
                                defences += ", ";

                            defences += type.ToString();
                        }

                    str += " to " + defences;
                }
                    break;
                case OngoingType.DamageModifier:
                    str = _fDamageModifier.ToString();
                    break;
                case OngoingType.Regeneration:
                    str = "Regeneration " + _fRegeneration.Value;
                    break;
                case OngoingType.Aura:
                    str = "Aura " + _fAura.Radius + ": " + _fAura.Description;
                    break;
            }

            return str;
        }

        /// <summary>
        ///     Creates a copy of the condition.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public OngoingCondition Copy()
        {
            var oc = new OngoingCondition();

            oc.Type = _fType;

            oc.Data = _fData;

            oc.DamageType = _fDamageType;
            oc.Value = _fValue;

            oc.DefenceMod = _fDefenceMod;
            oc.Defences = new List<DefenceType>();
            foreach (var type in _fDefences)
                oc._fDefences.Add(type);

            oc.Regeneration = _fRegeneration?.Copy();
            oc.DamageModifier = _fDamageModifier?.Copy();
            oc.Aura = _fAura?.Copy();

            oc.Duration = _fDuration;
            oc.DurationCreatureId = _fDurationCreatureId;
            oc.DurationRound = _fDurationRound;
            oc.SavingThrowModifier = _fSavingThrowModifier;

            return oc;
        }

        /// <summary>
        ///     Compares this condition to another.
        /// </summary>
        /// <param name="rhs">The other condition.</param>
        /// <returns>
        ///     Returns -1 if this condition should be sorted before the other, +1 if the other should be sorted before this;
        ///     0 otherwise.
        /// </returns>
        public int CompareTo(OngoingCondition rhs)
        {
            return ToString().CompareTo(rhs.ToString());
        }
    }
}
