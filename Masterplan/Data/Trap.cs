using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Specifies trap or hazard.
    /// </summary>
    public enum TrapType
    {
        /// <summary>
        ///     An artificial trap.
        /// </summary>
        Trap,

        /// <summary>
        ///     A natural / environmental hazard.
        /// </summary>
        Hazard,

        /// <summary>
        ///     A terrain effect.
        /// </summary>
        Terrain
    }

    /// <summary>
    ///     Class representing a trap or hazard.
    /// </summary>
    [Serializable]
    public class Trap : IComparable<Trap>
    {
        private TrapAttack _fAttack = new TrapAttack();

        private List<TrapAttack> _fAttacks = new List<TrapAttack>();

        private List<string> _fCountermeasures = new List<string>();

        private string _fDescription = "";

        private string _fDetails = "";

        private Guid _fId = Guid.NewGuid();

        private int _fInitiative = int.MinValue;

        private int _fLevel = 1;

        private string _fName = "";

        private string _fReadAloud = "";

        private IRole _fRole = new ComplexRole(RoleType.Blaster);

        private List<TrapSkillData> _fSkills = new List<TrapSkillData>();

        private string _fTrigger = "";

        private TrapType _fType = TrapType.Trap;

        /// <summary>
        ///     Gets or sets the unique ID.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets whether the object is a trap or a hazard.
        /// </summary>
        public TrapType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the level of the trap.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets or sets the role of the trap.
        /// </summary>
        public IRole Role
        {
            get => _fRole;
            set => _fRole = value;
        }

        /// <summary>
        ///     Gets or sets the read-aloud text for the trap.
        /// </summary>
        public string ReadAloud
        {
            get => _fReadAloud;
            set => _fReadAloud = value;
        }

        /// <summary>
        ///     Gets or sets the trap description.
        /// </summary>
        public string Description
        {
            get => _fDescription;
            set => _fDescription = value;
        }

        /// <summary>
        ///     Gets or sets the trap details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the list of skills that can be used with this trap.
        /// </summary>
        public List<TrapSkillData> Skills
        {
            get => _fSkills;
            set => _fSkills = value;
        }

        /// <summary>
        ///     Gets or sets the trap's initiative bonus (or int.MinValue if the trap does not roll initiative).
        /// </summary>
        public int Initiative
        {
            get => _fInitiative;
            set => _fInitiative = value;
        }

        /// <summary>
        ///     Gets or sets the trigger.
        /// </summary>
        public string Trigger
        {
            get => _fTrigger;
            set => _fTrigger = value;
        }

        /// <summary>
        ///     Gets or sets the trap attack data.
        /// </summary>
        public TrapAttack Attack
        {
            get => _fAttack;
            set => _fAttack = value;
        }

        /// <summary>
        ///     Gets or sets the trap's secondary attacks.
        /// </summary>
        public List<TrapAttack> Attacks
        {
            get => _fAttacks;
            set => _fAttacks = value;
        }

        /// <summary>
        ///     Gets or sets the list of trap countermeasures.
        /// </summary>
        public List<string> Countermeasures
        {
            get => _fCountermeasures;
            set => _fCountermeasures = value;
        }

        /// <summary>
        ///     Gets the XP value for the trap.
        /// </summary>
        public int Xp
        {
            get
            {
                var xp = 0;

                if (_fRole is Minion)
                {
                    var experience = (float)Experience.GetCreatureXp(_fLevel) / 4;
                    xp = (int)Math.Round(experience, MidpointRounding.AwayFromZero);
                }
                else
                {
                    var role = _fRole as ComplexRole;

                    xp = Experience.GetCreatureXp(_fLevel);
                    switch (role.Flag)
                    {
                        case RoleFlag.Elite:
                            xp *= 2;
                            break;
                        case RoleFlag.Solo:
                            xp *= 5;
                            break;
                    }
                }

                if (Session.Project != null)
                    // Apply campaign settings
                    xp = (int)(xp * Session.Project.CampaignSettings.Xp);

                return xp;
            }
        }

        /// <summary>
        ///     Level N [role] [trap/hazard]
        /// </summary>
        public string Info => "Level " + _fLevel + " " + _fRole + " " + _fType.ToString().ToLower();

        /// <summary>
        ///     Creates a copy of this object.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Trap Copy()
        {
            var t = new Trap();

            t.Id = _fId;
            t.Type = _fType;
            t.Name = _fName;
            t.Level = _fLevel;
            t.Role = _fRole.Copy();
            t.ReadAloud = _fReadAloud;
            t.Description = _fDescription;
            t.Details = _fDetails;

            foreach (var tsd in _fSkills)
                t.Skills.Add(tsd.Copy());

            t.Initiative = _fInitiative;
            t.Trigger = _fTrigger;

            t.Attack = _fAttack?.Copy();

            foreach (var ta in _fAttacks)
                t.Attacks.Add(ta.Copy());

            foreach (var cm in _fCountermeasures)
                t.Countermeasures.Add(cm);

            return t;
        }

        /// <summary>
        ///     Returns the name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Finds the specified skill data.
        /// </summary>
        /// <param name="skillname">The name of the skill to look for.</param>
        /// <returns>Returns the skill data if it is present; null otherwise.</returns>
        public TrapSkillData FindSkill(string skillname)
        {
            foreach (var tsd in _fSkills)
                if (tsd.SkillName == skillname)
                    return tsd;

            return null;
        }

        /// <summary>
        ///     Finds the specified skill data.
        /// </summary>
        /// <param name="id">The ID of the skill to look for.</param>
        /// <returns>Returns the skill data if it is present; null otherwise.</returns>
        public TrapSkillData FindSkill(Guid id)
        {
            foreach (var tsd in _fSkills)
                if (tsd.Id == id)
                    return tsd;

            return null;
        }

        /// <summary>
        ///     Finds the specified attack.
        /// </summary>
        /// <param name="id">The ID of the attack to look for.</param>
        /// <returns>Returns the attack if it is present; null otherwise.</returns>
        public TrapAttack FindAttack(Guid id)
        {
            foreach (var ta in _fAttacks)
                if (ta.Id == id)
                    return ta;

            return null;
        }

        /// <summary>
        ///     Adjusts the trap's level and attack data.
        /// </summary>
        /// <param name="delta">The level adjustment delta.</param>
        public void AdjustLevel(int delta)
        {
            _fLevel += delta;
            _fLevel = Math.Max(1, _fLevel);

            if (_fInitiative != int.MinValue)
            {
                Initiative += delta;
                _fInitiative = Math.Max(1, _fInitiative);
            }

            foreach (var ta in _fAttacks)
            {
                if (ta.Attack != null)
                {
                    ta.Attack.Bonus += delta;
                    ta.Attack.Bonus = Math.Max(1, ta.Attack.Bonus);
                }

                var hitDmg = Ai.ExtractDamage(ta.OnHit);
                if (hitDmg != "")
                {
                    var exp = DiceExpression.Parse(hitDmg);
                    var expAdj = exp?.Adjust(delta);
                    if (expAdj != null && exp.ToString() != expAdj.ToString())
                        ta.OnHit = ta.OnHit.Replace(hitDmg, expAdj + " damage");
                }

                var missDmg = Ai.ExtractDamage(ta.OnMiss);
                if (missDmg != "")
                {
                    var exp = DiceExpression.Parse(missDmg);
                    var expAdj = exp?.Adjust(delta);
                    if (expAdj != null && exp.ToString() != expAdj.ToString())
                        ta.OnMiss = ta.OnMiss.Replace(missDmg, expAdj + " damage");
                }

                var effectDmg = Ai.ExtractDamage(ta.Effect);
                if (effectDmg != "")
                {
                    var exp = DiceExpression.Parse(effectDmg);
                    var expAdj = exp?.Adjust(delta);
                    if (expAdj != null && exp.ToString() != expAdj.ToString())
                        ta.Effect = ta.Effect.Replace(effectDmg, expAdj + " damage");
                }
            }

            foreach (var tsd in _fSkills)
                tsd.Dc += delta;
        }

        /// <summary>
        ///     Compares this trap to another.
        /// </summary>
        /// <param name="rhs">The other trap.</param>
        /// <returns>
        ///     Returns -1 if this trap should be sorted before the other, +1 if the other should be sorted before this; 0
        ///     otherwise.
        /// </returns>
        public int CompareTo(Trap rhs)
        {
            return _fName.CompareTo(rhs.Name);
        }
    }

    /// <summary>
    ///     Class encapsulating skill usage data for a trap.
    /// </summary>
    [Serializable]
    public class TrapSkillData : IComparable<TrapSkillData>
    {
        private int _fDc = 10;

        private string _fDetails = "";

        private Guid _fId = Guid.NewGuid();

        private string _fSkillName = "Perception";

        /// <summary>
        ///     Gets or sets the ID of the skill data.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the skill.
        /// </summary>
        public string SkillName
        {
            get => _fSkillName;
            set => _fSkillName = value;
        }

        /// <summary>
        ///     Gets or sets the skill DC.
        /// </summary>
        public int Dc
        {
            get => _fDc;
            set => _fDc = value;
        }

        /// <summary>
        ///     Gets or sets the skill usage information.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     [skill name] DC XX: [details]
        ///     or
        ///     [skill name]: [details]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_fDc == 0)
                return _fSkillName + ": " + _fDetails;
            return _fSkillName + " DC " + _fDc + ": " + _fDetails;
        }

        /// <summary>
        ///     Creates a copy of the TrapSkillData.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public TrapSkillData Copy()
        {
            var tsd = new TrapSkillData();

            tsd.Id = _fId;
            tsd.SkillName = _fSkillName;
            tsd.Dc = _fDc;
            tsd.Details = _fDetails;

            return tsd;
        }

        /// <summary>
        ///     Sorts Perception first, then other skills alphabetically.
        ///     Skills with the same name are sorted by ascending DC.
        /// </summary>
        /// <param name="rhs">The other TrapSkillData object.</param>
        /// <returns>Returns -1 if this object should be sorted before rhs, +1 if rhs should be sorted before this, 0 otherwise.</returns>
        public int CompareTo(TrapSkillData rhs)
        {
            if (_fSkillName != rhs.SkillName)
            {
                // Sort by skill name
                if (_fSkillName == "Perception")
                    return -1;

                if (rhs.SkillName == "Perception")
                    return 1;

                return _fSkillName.CompareTo(rhs.SkillName);
            }

            // Sort by DC
            return _fDc.CompareTo(rhs.Dc);
        }
    }

    /// <summary>
    ///     Class encapsulating attack data for a trap.
    /// </summary>
    [Serializable]
    public class TrapAttack
    {
        private ActionType _fAction = ActionType.Standard;

        private PowerAttack _fAttack = new PowerAttack();

        private string _fEffect = "";

        private bool _fHasInitiative;

        private Guid _fId = Guid.NewGuid();

        private int _fInitiative;

        private string _fKeywords = "";

        private string _fName = "";

        private string _fNotes = "";

        private string _fOnHit = "";

        private string _fOnMiss = "";

        private string _fRange = "";

        private string _fTarget = "";

        private string _fTrigger = "";

        /// <summary>
        ///     Gets or sets the unique ID.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the attack name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the trap trigger details.
        /// </summary>
        public string Trigger
        {
            get => _fTrigger;
            set => _fTrigger = value;
        }

        /// <summary>
        ///     Gets or sets the action required.
        /// </summary>
        public ActionType Action
        {
            get => _fAction;
            set => _fAction = value;
        }

        /// <summary>
        ///     Gets or sets the range of the trap.
        /// </summary>
        public string Range
        {
            get => _fRange;
            set => _fRange = value;
        }

        /// <summary>
        ///     Gets or sets the trap attack keywords.
        /// </summary>
        public string Keywords
        {
            get => _fKeywords;
            set => _fKeywords = value;
        }

        /// <summary>
        ///     Gets or sets the trap target.
        /// </summary>
        public string Target
        {
            get => _fTarget;
            set => _fTarget = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the trap has an initiative score.
        /// </summary>
        public bool HasInitiative
        {
            get => _fHasInitiative;
            set => _fHasInitiative = value;
        }

        /// <summary>
        ///     Gets or sets the trap's initiative bonus.
        /// </summary>
        public int Initiative
        {
            get => _fInitiative;
            set => _fInitiative = value;
        }

        /// <summary>
        ///     Gets or sets the attack bonus and targeted defence.
        /// </summary>
        public PowerAttack Attack
        {
            get => _fAttack;
            set => _fAttack = value;
        }

        /// <summary>
        ///     Gets or sets the Hit details.
        /// </summary>
        public string OnHit
        {
            get => _fOnHit;
            set => _fOnHit = value;
        }

        /// <summary>
        ///     Gets or sets the Miss details.
        /// </summary>
        public string OnMiss
        {
            get => _fOnMiss;
            set => _fOnMiss = value;
        }

        /// <summary>
        ///     Gets or sets the Effect details.
        /// </summary>
        public string Effect
        {
            get => _fEffect;
            set => _fEffect = value;
        }

        /// <summary>
        ///     Gets or sets the trap attack miscellaneous notes.
        /// </summary>
        public string Notes
        {
            get => _fNotes;
            set => _fNotes = value;
        }

        /// <summary>
        ///     Creates a copy of the TrapAttack object.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public TrapAttack Copy()
        {
            var ta = new TrapAttack();

            ta.Id = _fId;
            ta.Name = _fName;
            ta.Trigger = _fTrigger;
            ta.Action = _fAction;
            ta.Keywords = _fKeywords;
            ta.Range = _fRange;
            ta.Target = _fTarget;
            ta.HasInitiative = _fHasInitiative;
            ta.Initiative = _fInitiative;
            ta.Attack = _fAttack.Copy();
            ta.OnHit = _fOnHit;
            ta.OnMiss = _fOnMiss;
            ta.Effect = _fEffect;
            ta.Notes = _fNotes;

            return ta;
        }
    }

    /// <summary>
    ///     Wrapper class to enable traps to be added to plot points.
    /// </summary>
    [Serializable]
    public class TrapElement : IElement
    {
        private Guid _fMapAreaId = Guid.Empty;

        private Guid _fMapId = Guid.Empty;

        private Trap _fTrap = new Trap();

        /// <summary>
        ///     Gets or sets the trap.
        /// </summary>
        public Trap Trap
        {
            get => _fTrap;
            set => _fTrap = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the map containing the trap.
        /// </summary>
        public Guid MapId
        {
            get => _fMapId;
            set => _fMapId = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the map area containing the trap.
        /// </summary>
        public Guid MapAreaId
        {
            get => _fMapAreaId;
            set => _fMapAreaId = value;
        }

        /// <summary>
        ///     Calculates the XP value of the trap.
        /// </summary>
        /// <returns>Returns the XP value.</returns>
        public int GetXp()
        {
            return _fTrap.Xp;
        }

        /// <summary>
        ///     Calculates the difficulty of the trap.
        /// </summary>
        /// <param name="party_level">The party level.</param>
        /// <param name="party_size">The party size.</param>
        /// <returns>Returns the difficulty.</returns>
        public Difficulty GetDifficulty(int partyLevel, int partySize)
        {
            return Ai.GetThreatDifficulty(_fTrap.Level, partyLevel);
        }

        /// <summary>
        ///     Creates a copy of the TrapElement.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public IElement Copy()
        {
            var te = new TrapElement();

            te.Trap = _fTrap.Copy();
            te.MapId = _fMapId;
            te.MapAreaId = _fMapAreaId;

            return te;
        }
    }
}
