using System;
using System.Collections.Generic;
using System.Drawing;

namespace Masterplan.Data
{
    /// <summary>
    ///     The various hero roles.
    /// </summary>
    public enum HeroRoleType
    {
        /// <summary>
        ///     Controller role.
        /// </summary>
        Controller,

        /// <summary>
        ///     Defender role.
        /// </summary>
        Defender,

        /// <summary>
        ///     Leader role.
        /// </summary>
        Leader,

        /// <summary>
        ///     Striker role.
        /// </summary>
        Striker,

        /// <summary>
        ///     Hybrid role.
        /// </summary>
        Hybrid
    }

    /// <summary>
    ///     Class representing a PC.
    /// </summary>
    [Serializable]
    public class Hero : IToken, IComparable<Hero>
    {
        private int _fAc = 10;

        private string _fClass = "";

        private CombatData _fCombatData = new CombatData();

        private List<OngoingCondition> _fEffects = new List<OngoingCondition>();

        private string _fEpicDestiny = "";

        private int _fFortitude = 10;

        private int _fHp;

        private Guid _fId = Guid.NewGuid();

        private int _fInitBonus;

        private string _fLanguages = "";

        private int _fLevel = Session.Project.Party.Level;

        private string _fName = "";

        private string _fParagonPath = "";

        private int _fPassiveInsight = 10;

        private int _fPassivePerception = 10;

        private string _fPlayer = "";

        private Image _fPortrait;

        private string _fPowerSource = "";

        private string _fRace = "";

        private int _fReflex = 10;

        private HeroRoleType _fRole = HeroRoleType.Controller;

        private CreatureSize _fSize = CreatureSize.Medium;

        private List<CustomToken> _fTokens = new List<CustomToken>();

        private int _fWill = 10;

        /// <summary>
        ///     Gets or sets the unique ID of the hero.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set
            {
                _fId = value;

                if (_fCombatData != null)
                    _fCombatData.Id = value;
            }
        }

        /// <summary>
        ///     Gets or sets the name of the hero.
        /// </summary>
        public string Name
        {
            get => _fName;
            set
            {
                _fName = value;

                if (_fCombatData != null)
                    _fCombatData.DisplayName = value;
            }
        }

        /// <summary>
        ///     Gets or sets the player name.
        /// </summary>
        public string Player
        {
            get => _fPlayer;
            set => _fPlayer = value;
        }

        /// <summary>
        ///     Gets or sets the size of the PC.
        /// </summary>
        public CreatureSize Size
        {
            get => _fSize;
            set => _fSize = value;
        }

        /// <summary>
        ///     Gets or sets the name of the PC's race.
        /// </summary>
        public string Race
        {
            get => _fRace;
            set => _fRace = value;
        }

        /// <summary>
        ///     Gets or sets the PC's level.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets or sets the name of the PC's class.
        /// </summary>
        public string Class
        {
            get => _fClass;
            set => _fClass = value;
        }

        /// <summary>
        ///     Gets or sets the name of the PC's paragon path.
        /// </summary>
        public string ParagonPath
        {
            get => _fParagonPath;
            set => _fParagonPath = value;
        }

        /// <summary>
        ///     Gets or sets the name of the PC's epic destiny.
        /// </summary>
        public string EpicDestiny
        {
            get => _fEpicDestiny;
            set => _fEpicDestiny = value;
        }

        /// <summary>
        ///     Gets or sets the power source of the PC's class.
        /// </summary>
        public string PowerSource
        {
            get => _fPowerSource;
            set => _fPowerSource = value;
        }

        /// <summary>
        ///     Gets or sets the PC's role.
        /// </summary>
        public HeroRoleType Role
        {
            get => _fRole;
            set => _fRole = value;
        }

        /// <summary>
        ///     Gets or sets the hero's combat data.
        /// </summary>
        public CombatData CombatData
        {
            get => _fCombatData;
            set
            {
                _fCombatData = value;
                _fCombatData.Id = _fId;
                _fCombatData.DisplayName = _fName;
            }
        }

        /// <summary>
        ///     Gets or sets the hero's hit points.
        /// </summary>
        public int Hp
        {
            get => _fHp;
            set => _fHp = value;
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
        ///     Gets or sets the hero's initiative bonus
        /// </summary>
        public int InitBonus
        {
            get => _fInitBonus;
            set => _fInitBonus = value;
        }

        /// <summary>
        ///     Gets or sets the PC's passive perception.
        /// </summary>
        public int PassivePerception
        {
            get => _fPassivePerception;
            set => _fPassivePerception = value;
        }

        /// <summary>
        ///     Gets or sets the PC's passive insight.
        /// </summary>
        public int PassiveInsight
        {
            get => _fPassiveInsight;
            set => _fPassiveInsight = value;
        }

        /// <summary>
        ///     Gets or sets the languages spoken by the hero.
        /// </summary>
        public string Languages
        {
            get => _fLanguages;
            set => _fLanguages = value;
        }

        /// <summary>
        ///     Gets or sets the set of ongoing effects this character can impose in combat.
        /// </summary>
        public List<OngoingCondition> Effects
        {
            get => _fEffects;
            set => _fEffects = value;
        }

        /// <summary>
        ///     Gets or sets the set of map tokens and overlays the character can use in combat.
        /// </summary>
        public List<CustomToken> Tokens
        {
            get => _fTokens;
            set => _fTokens = value;
        }

        /// <summary>
        ///     Level [N] [race] [class] / [paragon path] / [epic destiny]
        /// </summary>
        public string Info
        {
            get
            {
                var str = "Level " + _fLevel;

                if (_fRace != "")
                {
                    if (str != "")
                        str += " ";

                    str += _fRace;
                }

                if (_fClass != "")
                {
                    if (str != "")
                        str += " ";

                    str += _fClass;
                }

                if (_fParagonPath != "")
                {
                    if (str != "")
                        str += " / ";

                    str += _fParagonPath;
                }

                if (_fEpicDestiny != "")
                {
                    if (str != "")
                        str += " / ";

                    str += _fEpicDestiny;
                }

                return str;
            }
        }

        /// <summary>
        ///     Gets or sets the PC's portrait image.
        /// </summary>
        public Image Portrait
        {
            get => _fPortrait;
            set => _fPortrait = value;
        }

        /// <summary>
        ///     Calculates the hero's current state.
        /// </summary>
        /// <param name="damage">The hero's current damage.</param>
        /// <returns>Returns the hero's state.</returns>
        public CreatureState GetState(int damage)
        {
            if (_fHp != 0)
            {
                var hpCurrent = _fHp - damage;
                var hpBloodied = _fHp / 2;

                if (hpCurrent <= 0)
                    return CreatureState.Defeated;
                if (hpCurrent <= hpBloodied)
                    return CreatureState.Bloodied;
            }

            return CreatureState.Active;
        }

        /// <summary>
        ///     Creates a copy of the Hero.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Hero Copy()
        {
            var h = new Hero();

            h.Id = _fId;
            h.Name = _fName;
            h.Player = _fPlayer;
            h.Size = _fSize;
            h.Race = _fRace;
            h.Level = _fLevel;
            h.Class = _fClass;
            h.ParagonPath = _fParagonPath;
            h.EpicDestiny = _fEpicDestiny;
            h.PowerSource = _fPowerSource;
            h.Role = _fRole;
            h.CombatData = _fCombatData.Copy();
            h.Hp = _fHp;
            h.Ac = _fAc;
            h.Fortitude = _fFortitude;
            h.Reflex = _fReflex;
            h.Will = _fWill;
            h.InitBonus = _fInitBonus;
            h.PassivePerception = _fPassivePerception;
            h.PassiveInsight = _fPassiveInsight;
            h.Languages = _fLanguages;
            h.Portrait = _fPortrait;

            foreach (var oc in _fEffects)
                h.Effects.Add(oc.Copy());

            foreach (var ct in _fTokens)
                h.Tokens.Add(ct.Copy());

            return h;
        }

        /// <summary>
        ///     Returns the hero's name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Compares this Hero to another.
        /// </summary>
        /// <param name="rhs">The other Hero object.</param>
        /// <returns>Returns -1 if this Hero should be sorted before rhs, +1 if rhs should be sorted before this, 0 otherwise.</returns>
        public int CompareTo(Hero rhs)
        {
            return _fName.CompareTo(rhs.Name);
        }
    }
}
