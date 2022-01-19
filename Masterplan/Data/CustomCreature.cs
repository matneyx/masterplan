using System;
using System.Collections.Generic;
using System.Drawing;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a custom creature.
    /// </summary>
    [Serializable]
    public class CustomCreature : ICreature
    {
        private int _fAcModifier;

        private string _fAlignment = "";

        private List<Aura> _fAuras = new List<Aura>();

        private Ability _fCharisma = new Ability();

        private Ability _fConstitution = new Ability();

        private List<CreaturePower> _fCreaturePowers = new List<CreaturePower>();

        private List<DamageModifier> _fDamageModifiers = new List<DamageModifier>();

        private string _fDetails = "";

        private Ability _fDexterity = new Ability();

        private string _fEquipment = "";

        private int _fFortitudeModifier;

        private int _fHpModifier;

        private Guid _fId = Guid.NewGuid();

        private Image _fImage;

        private string _fImmune = "";

        private int _fInitiativeModifier;

        private Ability _fIntelligence = new Ability();

        private string _fKeywords = "";

        private string _fLanguages = "";

        private int _fLevel = 1;

        private string _fMovement = "";

        private string _fName = "";

        private CreatureOrigin _fOrigin = CreatureOrigin.Natural;

        private int _fReflexModifier;

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

        private int _fWillModifier;

        private Ability _fWisdom = new Ability();

        /// <summary>
        ///     Gets or sets the modifier for the initiative bonus.
        /// </summary>
        public int InitiativeModifier
        {
            get => _fInitiativeModifier;
            set => _fInitiativeModifier = value;
        }

        /// <summary>
        ///     Gets or sets the modifier for the HP total.
        /// </summary>
        public int HpModifier
        {
            get => _fHpModifier;
            set => _fHpModifier = value;
        }

        /// <summary>
        ///     Gets or sets the modifier for the AC defence.
        /// </summary>
        public int AcModifier
        {
            get => _fAcModifier;
            set => _fAcModifier = value;
        }

        /// <summary>
        ///     Gets or sets the modifier for the Fortitude defence.
        /// </summary>
        public int FortitudeModifier
        {
            get => _fFortitudeModifier;
            set => _fFortitudeModifier = value;
        }

        /// <summary>
        ///     Gets or sets the modifier for the Reflex defence.
        /// </summary>
        public int ReflexModifier
        {
            get => _fReflexModifier;
            set => _fReflexModifier = value;
        }

        /// <summary>
        ///     Gets or sets the modifier for the Will defence.
        /// </summary>
        public int WillModifier
        {
            get => _fWillModifier;
            set => _fWillModifier = value;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        public CustomCreature()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="c">The creature to copy from.</param>
        public CustomCreature(ICreature c)
        {
            CreatureHelper.CopyFields(c, this);
        }

        /// <summary>
        ///     Gets a string representation of the creature.
        /// </summary>
        /// <returns>Returns the name of the creature.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Creates a copy of the creature.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public CustomCreature Copy()
        {
            var cc = new CustomCreature();

            cc.Id = _fId;
            cc.Name = _fName;
            cc.Details = _fDetails;
            cc.Size = _fSize;
            cc.Origin = _fOrigin;
            cc.Type = _fType;
            cc.Keywords = _fKeywords;
            cc.Level = _fLevel;
            cc.Role = _fRole.Copy();
            cc.Senses = _fSenses;
            cc.Movement = _fMovement;
            cc.Alignment = _fAlignment;
            cc.Languages = _fLanguages;
            cc.Skills = _fSkills;
            cc.Equipment = _fEquipment;

            cc.Strength = _fStrength.Copy();
            cc.Constitution = _fConstitution.Copy();
            cc.Dexterity = _fDexterity.Copy();
            cc.Intelligence = _fIntelligence.Copy();
            cc.Wisdom = _fWisdom.Copy();
            cc.Charisma = _fCharisma.Copy();

            cc.InitiativeModifier = _fInitiativeModifier;
            cc.HpModifier = _fHpModifier;
            cc.AcModifier = _fAcModifier;
            cc.FortitudeModifier = _fFortitudeModifier;
            cc.ReflexModifier = _fReflexModifier;
            cc.WillModifier = _fWillModifier;

            cc.Regeneration = _fRegeneration?.Copy();

            foreach (var aura in _fAuras)
                cc.Auras.Add(aura.Copy());

            foreach (var cp in _fCreaturePowers)
                cc.CreaturePowers.Add(cp.Copy());

            foreach (var dm in _fDamageModifiers)
                cc.DamageModifiers.Add(dm.Copy());

            cc.Resist = _fResist;
            cc.Vulnerable = _fVulnerable;
            cc.Immune = _fImmune;
            cc.Tactics = _fTactics;

            cc.Image = _fImage;

            return cc;
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
                    return Creature.GetSpeed(_fSize) + " squares";
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
        ///     Gets the category.
        /// </summary>
        public string Category
        {
            get => "";
            set { }
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
        ///     Gets the initiative bonus.
        /// </summary>
        public int Initiative
        {
            get
            {
                var basic = Statistics.Initiative(_fLevel, _fRole);
                return basic + _fDexterity.Modifier + _fInitiativeModifier;
            }
            set
            {
                var basic = Statistics.Initiative(_fLevel, _fRole);
                _fInitiativeModifier = value - basic - _fDexterity.Modifier;
            }
        }

        /// <summary>
        ///     Gets the HP total.
        /// </summary>
        public int Hp
        {
            get
            {
                if (_fRole is Minion)
                {
                    return 1;
                }

                var basic = Statistics.Hp(_fLevel, _fRole as ComplexRole, _fConstitution.Score);
                return basic + _fHpModifier;
            }
            set
            {
                if (_fRole is Minion)
                {
                    // Do nothing
                }
                else
                {
                    var basic = Statistics.Hp(_fLevel, _fRole as ComplexRole, _fConstitution.Score);
                    _fHpModifier = value - basic;
                }
            }
        }

        /// <summary>
        ///     Gets the AC defence.
        /// </summary>
        public int Ac
        {
            get
            {
                var ac = Statistics.Ac(_fLevel, _fRole);
                return ac + _fAcModifier;
            }
            set
            {
                var ac = Statistics.Ac(_fLevel, _fRole);
                _fAcModifier = value - ac;
            }
        }

        /// <summary>
        ///     Gets the Fortitude defence.
        /// </summary>
        public int Fortitude
        {
            get
            {
                var defence = Statistics.Nad(_fLevel, _fRole);
                var score = Math.Max(_fStrength.Score, _fConstitution.Score);
                var mod = Ability.GetModifier(score);

                return defence + mod + _fFortitudeModifier;
            }
            set
            {
                var defence = Statistics.Nad(_fLevel, _fRole);
                var score = Math.Max(_fStrength.Score, _fConstitution.Score);
                var mod = Ability.GetModifier(score);

                _fFortitudeModifier = value - defence - mod;
            }
        }

        /// <summary>
        ///     Gets the Reflex defence.
        /// </summary>
        public int Reflex
        {
            get
            {
                var defence = Statistics.Nad(_fLevel, _fRole);
                var score = Math.Max(_fDexterity.Score, _fIntelligence.Score);
                var mod = Ability.GetModifier(score);

                return defence + mod + _fReflexModifier;
            }
            set
            {
                var defence = Statistics.Nad(_fLevel, _fRole);
                var score = Math.Max(_fDexterity.Score, _fIntelligence.Score);
                var mod = Ability.GetModifier(score);

                _fReflexModifier = value - defence - mod;
            }
        }

        /// <summary>
        ///     Gets the Will defence.
        /// </summary>
        public int Will
        {
            get
            {
                var defence = Statistics.Nad(_fLevel, _fRole);
                var score = Math.Max(_fWisdom.Score, _fCharisma.Score);
                var mod = Ability.GetModifier(score);

                return defence + mod + _fWillModifier;
            }
            set
            {
                var defence = Statistics.Nad(_fLevel, _fRole);
                var score = Math.Max(_fWisdom.Score, _fCharisma.Score);
                var mod = Ability.GetModifier(score);

                _fWillModifier = value - defence - mod;
            }
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
