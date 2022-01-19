using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class CreatureStatsForm : Form
    {
        private int _fAc;

        private int _fHp;
        private int _fInit;
        private int _fNad;

        public ICreature Creature { get; }

        public CreatureStatsForm(ICreature c)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Creature = c;

            if (Creature.Role is Minion)
            {
                HPBox.Value = 1;
                HPGroup.Enabled = false;
            }
            else
            {
                HPBox.Value = Creature.Hp;
            }

            InitBox.Value = Creature.Initiative;
            ACBox.Value = Creature.Ac;
            FortBox.Value = Creature.Fortitude;
            RefBox.Value = Creature.Reflex;
            WillBox.Value = Creature.Will;

            update_recommendations();
        }

        ~CreatureStatsForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            HPRecBtn.Enabled = HPBox.Value != _fHp;
            InitRecBtn.Enabled = InitBox.Value != _fInit;
            ACRecBtn.Enabled = ACBox.Value != _fAc;
            FortRecBtn.Enabled = FortBox.Value != _fNad;
            RefRecBtn.Enabled = RefBox.Value != _fNad;
            WillRecBtn.Enabled = WillBox.Value != _fNad;

            DefaultBtn.Enabled = HPRecBtn.Enabled || InitRecBtn.Enabled || ACRecBtn.Enabled || FortRecBtn.Enabled ||
                                 RefRecBtn.Enabled || WillRecBtn.Enabled;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            if (Creature.Role is ComplexRole)
            {
                Creature.Hp = (int)HPBox.Value;
            }

            Creature.Initiative = (int)InitBox.Value;
            Creature.Ac = (int)ACBox.Value;
            Creature.Fortitude = (int)FortBox.Value;
            Creature.Reflex = (int)RefBox.Value;
            Creature.Will = (int)WillBox.Value;
        }

        private void update_recommendations()
        {
            var minion = Creature.Role is Minion;

            _fHp = minion
                ? 1
                : Statistics.Hp(Creature.Level, Creature.Role as ComplexRole, Creature.Constitution.Score);
            _fInit = Statistics.Initiative(Creature.Level, Creature.Role);
            _fAc = Statistics.Ac(Creature.Level, Creature.Role);
            _fNad = Statistics.Nad(Creature.Level, Creature.Role);

            HPRecBtn.Text = minion ? "-" : "Recommended: " + _fHp;
            InitRecBtn.Text = "Recommended: " + _fInit;
            ACRecBtn.Text = "Recommended: " + _fAc;
            FortRecBtn.Text = "Recommended: " + _fNad;
            RefRecBtn.Text = "Recommended: " + _fNad;
            WillRecBtn.Text = "Recommended: " + _fNad;
        }

        private void DefaultBtn_Click(object sender, EventArgs e)
        {
            HPBox.Value = _fHp;
            InitBox.Value = _fInit;
            ACBox.Value = _fAc;
            FortBox.Value = _fNad;
            RefBox.Value = _fNad;
            WillBox.Value = _fNad;
        }

        private void HPRecBtn_Click(object sender, EventArgs e)
        {
            HPBox.Value = _fHp;
        }

        private void InitRecBtn_Click(object sender, EventArgs e)
        {
            InitBox.Value = _fInit;
        }

        private void ACRecBtn_Click(object sender, EventArgs e)
        {
            ACBox.Value = _fAc;
        }

        private void FortRecBtn_Click(object sender, EventArgs e)
        {
            FortBox.Value = _fNad;
        }

        private void RefRecBtn_Click(object sender, EventArgs e)
        {
            RefBox.Value = _fNad;
        }

        private void WillRecBtn_Click(object sender, EventArgs e)
        {
            WillBox.Value = _fNad;
        }
    }
}
