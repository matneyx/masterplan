using System;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class to hold campaign-specific modifications.
    /// </summary>
    [Serializable]
    public class CampaignSettings
    {
        private int _fAcBonus;

        private int _fAttackBonus;

        private double _fHp = 1.0;

        private int _fNadBonus;

        private double _fXp = 1.0;

        /// <summary>
        ///     Gets or sets the HP modifier.
        /// </summary>
        public double Hp
        {
            get => _fHp;
            set => _fHp = value;
        }

        /// <summary>
        ///     Gets or sets the XP modifier.
        /// </summary>
        public double Xp
        {
            get => _fXp;
            set => _fXp = value;
        }

        /// <summary>
        ///     Gets or sets the bonus to attacks.
        /// </summary>
        public int AttackBonus
        {
            get => _fAttackBonus;
            set => _fAttackBonus = value;
        }

        /// <summary>
        ///     Gets or sets the bonus to AC.
        /// </summary>
        public int AcBonus
        {
            get => _fAcBonus;
            set => _fAcBonus = value;
        }

        /// <summary>
        ///     Gets or sets the bonus to non-AC defences.
        /// </summary>
        public int NadBonus
        {
            get => _fNadBonus;
            set => _fNadBonus = value;
        }
    }
}
