using System;
using System.Collections.Generic;

namespace Masterplan.Data
{
    /// <summary>
    ///     Damage types.
    /// </summary>
    public enum DamageType
    {
        /// <summary>
        ///     Untyped damage.
        /// </summary>
        Untyped,

        /// <summary>
        ///     Acid damage.
        /// </summary>
        Acid,

        /// <summary>
        ///     Cold damage.
        /// </summary>
        Cold,

        /// <summary>
        ///     Fire damage.
        /// </summary>
        Fire,

        /// <summary>
        ///     Force damage.
        /// </summary>
        Force,

        /// <summary>
        ///     Lightning damage.
        /// </summary>
        Lightning,

        /// <summary>
        ///     Nectoric damage.
        /// </summary>
        Necrotic,

        /// <summary>
        ///     Poison damage.
        /// </summary>
        Poison,

        /// <summary>
        ///     Psychic damage.
        /// </summary>
        Psychic,

        /// <summary>
        ///     Radiant damage.
        /// </summary>
        Radiant,

        /// <summary>
        ///     Thunder damage.
        /// </summary>
        Thunder
    }

    /// <summary>
    ///     Class representing damage resistance / vulnerability / immunity.
    /// </summary>
    [Serializable]
    public class DamageModifier
    {
        private DamageType _fType = DamageType.Fire;

        private int _fValue = -5;

        /// <summary>
        ///     Gets or sets the type of damage.
        /// </summary>
        public DamageType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the value of the modifier.
        ///     If positive, vulnerable by this amount.
        ///     If negative, resistant by this amount.
        ///     If 0, immune.
        /// </summary>
        public int Value
        {
            get => _fValue;
            set => _fValue = value;
        }

        /// <summary>
        ///     Creates a copy of the damage modifier.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public DamageModifier Copy()
        {
            var dm = new DamageModifier();

            dm.Type = _fType;
            dm.Value = _fValue;

            return dm;
        }

        /// <summary>
        ///     Immune to [damage type]
        ///     or
        ///     [Resist / Vulnerable] N [damage type]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_fValue == 0)
                return "Immune to " + _fType.ToString().ToLower();

            var header = _fValue < 0 ? "Resist" : "Vulnerable";
            var val = Math.Abs(_fValue);

            return header + " " + val + " " + _fType.ToString().ToLower();
        }

        /// <summary>
        ///     Creates a DamageModifier object.
        /// </summary>
        /// <param name="damageType">The damage type as a string.</param>
        /// <param name="value">The modifier value.</param>
        /// <returns>Returns the damage modifier object.</returns>
        public static DamageModifier Parse(string damageType, int value)
        {
            var types = Enum.GetNames(typeof(DamageType));
            var typeList = new List<string>();
            foreach (var type in types)
                typeList.Add(type);

            try
            {
                var mod = new DamageModifier();

                mod.Type = (DamageType)Enum.Parse(typeof(DamageType), damageType, true);
                mod.Value = value;

                return mod;
            }
            catch
            {
            }

            return null;
        }
    }

    /// <summary>
    ///     Class representing damage resistance / vulnerability / immunity for a creature template.
    /// </summary>
    [Serializable]
    public class DamageModifierTemplate
    {
        private int _fEpicValue = -15;

        private int _fHeroicValue = -5;

        private int _fParagonValue = -10;

        private DamageType _fType = DamageType.Untyped;

        /// <summary>
        ///     Gets or sets the type of damage.
        /// </summary>
        public DamageType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the amount of resistance / vulnerability at the heroic tier.
        /// </summary>
        public int HeroicValue
        {
            get => _fHeroicValue;
            set => _fHeroicValue = value;
        }

        /// <summary>
        ///     Gets or sets the amount of resistance / vulnerability at the paragon tier.
        /// </summary>
        public int ParagonValue
        {
            get => _fParagonValue;
            set => _fParagonValue = value;
        }

        /// <summary>
        ///     Gets or sets the amount of resistance / vulnerability at the epic tier.
        /// </summary>
        public int EpicValue
        {
            get => _fEpicValue;
            set => _fEpicValue = value;
        }

        /// <summary>
        ///     Creates a copy of the DamageModifierTemplate.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public DamageModifierTemplate Copy()
        {
            var dmt = new DamageModifierTemplate();

            dmt.Type = _fType;
            dmt.HeroicValue = _fHeroicValue;
            dmt.ParagonValue = _fParagonValue;
            dmt.EpicValue = _fEpicValue;

            return dmt;
        }

        /// <summary>
        ///     Immume to [damage type]
        ///     or
        ///     [Resist / Vulnerable] HH / PP / EE [damage type]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var totalMod = _fHeroicValue + _fParagonValue + _fEpicValue;
            if (totalMod == 0)
                return "Immune to " + _fType.ToString().ToLower();

            var header = _fHeroicValue < 0 ? "Resist" : "Vulnerable";
            var heroic = Math.Abs(_fHeroicValue);
            var paragon = Math.Abs(_fParagonValue);
            var epic = Math.Abs(_fEpicValue);

            return header + " " + heroic + " / " + paragon + " / " + epic + " " + _fType.ToString().ToLower();
        }
    }
}
