using System;
using System.Collections.Generic;

namespace Masterplan.Data
{
    /// <summary>
    ///     Functional or class template.
    /// </summary>
    public enum CreatureTemplateType
    {
        /// <summary>
        ///     Functional template.
        /// </summary>
        Functional,

        /// <summary>
        ///     Class template.
        /// </summary>
        Class
    }

    /// <summary>
    ///     Class representing a functional or class template.
    /// </summary>
    [Serializable]
    public class CreatureTemplate
    {
        private int _fAc;

        private List<Aura> _fAuras = new List<Aura>();

        private List<CreaturePower> _fCreaturePowers = new List<CreaturePower>();

        private List<DamageModifierTemplate> _fDamageModifierTemplates = new List<DamageModifierTemplate>();

        private int _fFortitude;

        private int _fHp;

        private Guid _fId = Guid.NewGuid();

        private string _fImmune = "";

        private int _fInitiative;

        private bool _fLeader;

        private string _fMovement = "";

        private string _fName = "";

        private int _fReflex;

        private Regeneration _fRegeneration;

        private string _fResist = "";

        private RoleType _fRole = RoleType.Artillery;

        private string _fSenses = "";

        private string _fTactics = "";

        private CreatureTemplateType _fType = CreatureTemplateType.Functional;

        private string _fVulnerable = "";

        private int _fWill;

        /// <summary>
        ///     Gets or sets the name of the template.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the template.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the template type.
        /// </summary>
        public CreatureTemplateType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the role the template fulfills.
        /// </summary>
        public RoleType Role
        {
            get => _fRole;
            set => _fRole = value;
        }

        /// <summary>
        ///     Gets or sets whether the template is a Leader.
        /// </summary>
        public bool Leader
        {
            get => _fLeader;
            set => _fLeader = value;
        }

        /// <summary>
        ///     Gets or sets special senses the template provides.
        /// </summary>
        public string Senses
        {
            get => _fSenses;
            set => _fSenses = value;
        }

        /// <summary>
        ///     Gets or sets special movement the template provides.
        /// </summary>
        public string Movement
        {
            get => _fMovement;
            set => _fMovement = value;
        }

        /// <summary>
        ///     Gets or sets the number of HP per level the template provides.
        /// </summary>
        public int Hp
        {
            get => _fHp;
            set => _fHp = value;
        }

        /// <summary>
        ///     Gets or sets the initiative modifier the template provides.
        /// </summary>
        public int Initiative
        {
            get => _fInitiative;
            set => _fInitiative = value;
        }

        /// <summary>
        ///     Gets or sets the AC modifier the template provides.
        /// </summary>
        public int Ac
        {
            get => _fAc;
            set => _fAc = value;
        }

        /// <summary>
        ///     Gets or sets the Fortitude modifier the template provides.
        /// </summary>
        public int Fortitude
        {
            get => _fFortitude;
            set => _fFortitude = value;
        }

        /// <summary>
        ///     Gets or sets the Reflex modifier the template provides.
        /// </summary>
        public int Reflex
        {
            get => _fReflex;
            set => _fReflex = value;
        }

        /// <summary>
        ///     Gets or sets the Will modifier the template provides.
        /// </summary>
        public int Will
        {
            get => _fWill;
            set => _fWill = value;
        }

        /// <summary>
        ///     Gets or sets the regeneration provided by the template.
        /// </summary>
        public Regeneration Regeneration
        {
            get => _fRegeneration;
            set => _fRegeneration = value;
        }

        /// <summary>
        ///     Gets or sets the list of auras provided by the template.
        /// </summary>
        public List<Aura> Auras
        {
            get => _fAuras;
            set => _fAuras = value;
        }

        /// <summary>
        ///     Gets or sets the list of powers provided by the template.
        /// </summary>
        public List<CreaturePower> CreaturePowers
        {
            get => _fCreaturePowers;
            set => _fCreaturePowers = value;
        }

        /// <summary>
        ///     Gets or sets the list of resistances / vulnerabilities / immunities provided by the template.
        /// </summary>
        public List<DamageModifierTemplate> DamageModifierTemplates
        {
            get => _fDamageModifierTemplates;
            set => _fDamageModifierTemplates = value;
        }

        /// <summary>
        ///     Gets or sets the resistances provided by the template.
        /// </summary>
        public string Resist
        {
            get => _fResist;
            set => _fResist = value;
        }

        /// <summary>
        ///     Gets or sets the vulnerabilities provided by the template.
        /// </summary>
        public string Vulnerable
        {
            get => _fVulnerable;
            set => _fVulnerable = value;
        }

        /// <summary>
        ///     Gets or sets the immunities provided by the template.
        /// </summary>
        public string Immune
        {
            get => _fImmune;
            set => _fImmune = value;
        }

        /// <summary>
        ///     Gets or sets the tactics.
        /// </summary>
        public string Tactics
        {
            get => _fTactics;
            set => _fTactics = value;
        }

        /// <summary>
        ///     [Elite] [role] [(L)]
        /// </summary>
        public string Info
        {
            get
            {
                var start = _fType == CreatureTemplateType.Functional ? "Elite " : "";
                var leader = _fLeader ? " (L)" : "";

                return start + _fRole + leader;
            }
        }

        /// <summary>
        ///     Creates a copy of the template.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public CreatureTemplate Copy()
        {
            var t = new CreatureTemplate();

            t.Name = _fName;
            t.Id = _fId;
            t.Type = _fType;
            t.Role = _fRole;
            t.Leader = _fLeader;
            t.Senses = _fSenses;
            t.Movement = _fMovement;

            t.Hp = _fHp;
            t.Initiative = _fInitiative;
            t.Ac = _fAc;
            t.Fortitude = _fFortitude;
            t.Reflex = _fReflex;
            t.Will = _fWill;

            t.Regeneration = _fRegeneration?.Copy();

            foreach (var aura in _fAuras)
                t.Auras.Add(aura.Copy());

            foreach (var cp in _fCreaturePowers)
                t.CreaturePowers.Add(cp.Copy());

            foreach (var dmt in _fDamageModifierTemplates)
                t.DamageModifierTemplates.Add(dmt.Copy());

            t.Resist = _fResist;
            t.Vulnerable = _fVulnerable;
            t.Immune = _fImmune;
            t.Tactics = _fTactics;

            return t;
        }

        /// <summary>
        ///     Returns the name of the template.
        /// </summary>
        /// <returns>Returns the name of the template.</returns>
        public override string ToString()
        {
            return _fName;
        }
    }
}
