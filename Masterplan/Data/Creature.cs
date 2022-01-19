using System;
using System.Collections.Generic;
using System.Drawing;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Enumeration containing the various string fields.
    /// </summary>
    internal enum DetailsField
    {
        None,
        Senses,
        Movement,
        Resist,
        Vulnerable,
        Immune,
        Alignment,
        Languages,
        Skills,
        Equipment,
        Description,
        Tactics
    }

    /// <summary>
    ///     Class representing a creature.
    /// </summary>
    [Serializable]
    public class Creature : ICreature
    {
        private int _fAc = 10;

        private string _fAlignment = "";

        private List<Aura> _fAuras = new List<Aura>();

        private string _fCategory = "";

        private Ability _fCharisma = new Ability();

        private Ability _fConstitution = new Ability();

        private List<CreaturePower> _fCreaturePowers = new List<CreaturePower>();

        private List<DamageModifier> _fDamageModifiers = new List<DamageModifier>();

        private string _fDetails = "";

        private Ability _fDexterity = new Ability();

        private string _fEquipment = "";

        private int _fFortitude = 10;

        private int _fHp;

        private Guid _fId = Guid.NewGuid();

        private Image _fImage;

        private string _fImmune = "";

        private int _fInitiative;

        private Ability _fIntelligence = new Ability();

        private string _fKeywords = "";

        private string _fLanguages = "";

        private int _fLevel = 1;

        private string _fMovement = "6";

        private string _fName = "";

        private CreatureOrigin _fOrigin = CreatureOrigin.Natural;

        private int _fReflex = 10;

        private Regeneration _fRegeneration;

        private string _fResist = "";

        private IRole _fRole = new ComplexRole();

        private string _fSenses = "";

        private CreatureSize _fSize = CreatureSize.Medium;

        private string _fSkills = "";

        private Ability _fStrength = new Ability();

        private string _fTactics = "";

        private CreatureType _fType = CreatureType.MagicalBeast;

        private string _fVulnerable = "";

        private int _fWill = 10;

        private Ability _fWisdom = new Ability();

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public Creature()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="c">The creature to copy from.</param>
        public Creature(ICreature c)
        {
            CreatureHelper.CopyFields(c, this);
        }

        /// <summary>
        ///     Gets a string representation of the creature.
        /// </summary>
        /// <returns>Returns the name of the creature, followed by level and role.</returns>
        public override string ToString()
        {
            return _fName + " (" + Info + ")";
        }

        /// <summary>
        ///     Creates a copy of the creature.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Creature Copy()
        {
            var c = new Creature();

            c.Id = _fId;
            c.Name = _fName;
            c.Details = _fDetails;
            c.Size = _fSize;
            c.Origin = _fOrigin;
            c.Type = _fType;
            c.Keywords = _fKeywords;
            c.Level = _fLevel;
            c.Role = _fRole.Copy();
            c.Senses = _fSenses;
            c.Movement = _fMovement;
            c.Alignment = _fAlignment;
            c.Languages = _fLanguages;
            c.Skills = _fSkills;
            c.Equipment = _fEquipment;
            c.Category = _fCategory;

            c.Strength = _fStrength.Copy();
            c.Constitution = _fConstitution.Copy();
            c.Dexterity = _fDexterity.Copy();
            c.Intelligence = _fIntelligence.Copy();
            c.Wisdom = _fWisdom.Copy();
            c.Charisma = _fCharisma.Copy();

            c.Hp = _fHp;
            c.Initiative = _fInitiative;
            c.Ac = _fAc;
            c.Fortitude = _fFortitude;
            c.Reflex = _fReflex;
            c.Will = _fWill;

            c.Regeneration = _fRegeneration?.Copy();

            foreach (var aura in _fAuras)
                c.Auras.Add(aura.Copy());

            foreach (var cp in _fCreaturePowers)
                c.CreaturePowers.Add(cp.Copy());

            foreach (var dm in _fDamageModifiers)
                c.DamageModifiers.Add(dm.Copy());

            c.Resist = _fResist;
            c.Vulnerable = _fVulnerable;
            c.Immune = _fImmune;
            c.Tactics = _fTactics;

            c.Image = _fImage;

            return c;
        }

        /// <summary>
        ///     Gets the square size of a creature of the given size.
        /// </summary>
        /// <param name="size">The creature size.</param>
        /// <returns>Returns the size in squares.</returns>
        public static int GetSize(CreatureSize size)
        {
            switch (size)
            {
                case CreatureSize.Large:
                    return 2;
                case CreatureSize.Huge:
                    return 3;
                case CreatureSize.Gargantuan:
                    return 4;
            }

            return 1;
        }

        /// <summary>
        ///     Gets the typical speed of a creature of the given size.
        /// </summary>
        /// <param name="size">The creature size.</param>
        /// <returns>Returns the spee din squares.</returns>
        public static int GetSpeed(CreatureSize size)
        {
            switch (size)
            {
                case CreatureSize.Tiny:
                case CreatureSize.Small:
                    return 4;
                case CreatureSize.Medium:
                    return 6;
                case CreatureSize.Large:
                    return 6;
                case CreatureSize.Huge:
                    return 8;
                case CreatureSize.Gargantuan:
                    return 10;
            }

            return 6;
        }

        /// <summary>
        ///     Gets or sets the unique ID.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the size.
        /// </summary>
        public CreatureSize Size
        {
            get => _fSize;
            set => _fSize = value;
        }

        /// <summary>
        ///     Gets or sets the creature origin.
        /// </summary>
        public CreatureOrigin Origin
        {
            get => _fOrigin;
            set => _fOrigin = value;
        }

        /// <summary>
        ///     Gets or sets the creature type.
        /// </summary>
        public CreatureType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the creature keywords.
        /// </summary>
        public string Keywords
        {
            get => _fKeywords;
            set => _fKeywords = value;
        }

        /// <summary>
        ///     Gets or sets the level.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets or sets the role.
        /// </summary>
        public IRole Role
        {
            get => _fRole;
            set => _fRole = value;
        }

        /// <summary>
        ///     Gets or sets the senses.
        /// </summary>
        public string Senses
        {
            get => _fSenses;
            set => _fSenses = value;
        }

        /// <summary>
        ///     Gets or sets the movement.
        /// </summary>
        public string Movement
        {
            get
            {
                if (_fMovement == null || _fMovement == "")
                    return GetSpeed(_fSize) + " squares";
                return _fMovement;
            }
            set => _fMovement = value;
        }

        /// <summary>
        ///     Gets or sets the alignment.
        /// </summary>
        public string Alignment
        {
            get => _fAlignment;
            set => _fAlignment = value;
        }

        /// <summary>
        ///     Gets or sets the languages.
        /// </summary>
        public string Languages
        {
            get => _fLanguages;
            set => _fLanguages = value;
        }

        /// <summary>
        ///     Gets or sets the skills.
        /// </summary>
        public string Skills
        {
            get => _fSkills;
            set => _fSkills = value;
        }

        /// <summary>
        ///     Gets or sets the equipment.
        /// </summary>
        public string Equipment
        {
            get => _fEquipment;
            set => _fEquipment = value;
        }

        /// <summary>
        ///     Gets or sets the category.
        /// </summary>
        public string Category
        {
            get => _fCategory;
            set => _fCategory = value;
        }

        /// <summary>
        ///     Gets or sets the strength ability.
        /// </summary>
        public Ability Strength
        {
            get => _fStrength;
            set => _fStrength = value;
        }

        /// <summary>
        ///     Gets or sets the constitution ability.
        /// </summary>
        public Ability Constitution
        {
            get => _fConstitution;
            set => _fConstitution = value;
        }

        /// <summary>
        ///     Gets or sets the dexterity ability.
        /// </summary>
        public Ability Dexterity
        {
            get => _fDexterity;
            set => _fDexterity = value;
        }

        /// <summary>
        ///     Gets or sets the intelligence ability.
        /// </summary>
        public Ability Intelligence
        {
            get => _fIntelligence;
            set => _fIntelligence = value;
        }

        /// <summary>
        ///     Gets or sets the wisdom ability.
        /// </summary>
        public Ability Wisdom
        {
            get => _fWisdom;
            set => _fWisdom = value;
        }

        /// <summary>
        ///     Gets or sets the charisma ability.
        /// </summary>
        public Ability Charisma
        {
            get => _fCharisma;
            set => _fCharisma = value;
        }

        /// <summary>
        ///     Gets or sets the HP total.
        /// </summary>
        public int Hp
        {
            get => _fHp;
            set => _fHp = value;
        }

        /// <summary>
        ///     Gets or sets the initiative bonus.
        /// </summary>
        public int Initiative
        {
            get => _fInitiative;
            set => _fInitiative = value;
        }

        /// <summary>
        ///     Gets or sets the AC defence.
        /// </summary>
        public int Ac
        {
            get => _fAc;
            set => _fAc = value;
        }

        /// <summary>
        ///     Gets or sets the Fortitude defence.
        /// </summary>
        public int Fortitude
        {
            get => _fFortitude;
            set => _fFortitude = value;
        }

        /// <summary>
        ///     Gets or sets the Reflex defence.
        /// </summary>
        public int Reflex
        {
            get => _fReflex;
            set => _fReflex = value;
        }

        /// <summary>
        ///     Gets or sets the Will defence.
        /// </summary>
        public int Will
        {
            get => _fWill;
            set => _fWill = value;
        }

        /// <summary>
        ///     Gets or sets the creature's regeneration.
        /// </summary>
        public Regeneration Regeneration
        {
            get => _fRegeneration;
            set => _fRegeneration = value;
        }

        /// <summary>
        ///     Gets or sets the list of auras.
        /// </summary>
        public List<Aura> Auras
        {
            get => _fAuras;
            set => _fAuras = value;
        }

        /// <summary>
        ///     Gets or sets the list of powers.
        /// </summary>
        public List<CreaturePower> CreaturePowers
        {
            get => _fCreaturePowers;
            set => _fCreaturePowers = value;
        }

        /// <summary>
        ///     Gets or sets the list of damage modifiers.
        /// </summary>
        public List<DamageModifier> DamageModifiers
        {
            get => _fDamageModifiers;
            set => _fDamageModifiers = value;
        }

        /// <summary>
        ///     Gets or sets the resistances.
        /// </summary>
        public string Resist
        {
            get => _fResist;
            set => _fResist = value;
        }

        /// <summary>
        ///     Gets or sets the vulnerabilities.
        /// </summary>
        public string Vulnerable
        {
            get => _fVulnerable;
            set => _fVulnerable = value;
        }

        /// <summary>
        ///     Gets or sets the immunities.
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
        ///     Gets or sets the picture to display on the map.
        /// </summary>
        public Image Image
        {
            get => _fImage;
            set => _fImage = value;
        }

        /// <summary>
        ///     Level N [role]
        /// </summary>
        public string Info => "Level " + _fLevel + " " + _fRole;

        /// <summary>
        ///     [origin] [type] [keywords]
        /// </summary>
        public string Phenotype
        {
            get
            {
                var str = _fSize + " " + _fOrigin.ToString().ToLower();

                if (_fType == CreatureType.MagicalBeast)
                    str += " magical beast";
                else
                    str += " " + _fType.ToString().ToLower();

                if (_fKeywords != null && _fKeywords != "")
                    str += " (" + _fKeywords.ToLower() + ")";

                return str;
            }
        }

        /// <summary>
        ///     Compares this creature to another.
        /// </summary>
        /// <param name="rhs">The other creature.</param>
        /// <returns>
        ///     Returns -1 if this creature should be sorted before the other, +1 if the other should be sorted before this; 0
        ///     otherwise.
        /// </returns>
        public int CompareTo(ICreature rhs)
        {
            return _fName.CompareTo(rhs.Name);
        }
    }
}
