using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CreatureAbilityForm : Form
    {
        public ICreature Creature { get; }

        public CreatureAbilityForm(ICreature c)
        {
            InitializeComponent();

            Creature = c;

            StrBox.Value = Creature.Strength.Score;
            ConBox.Value = Creature.Constitution.Score;
            DexBox.Value = Creature.Dexterity.Score;
            IntBox.Value = Creature.Intelligence.Score;
            WisBox.Value = Creature.Wisdom.Score;
            ChaBox.Value = Creature.Charisma.Score;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Creature.Strength.Score = (int)StrBox.Value;
            Creature.Constitution.Score = (int)ConBox.Value;
            Creature.Dexterity.Score = (int)DexBox.Value;
            Creature.Intelligence.Score = (int)IntBox.Value;
            Creature.Wisdom.Score = (int)WisBox.Value;
            Creature.Charisma.Score = (int)ChaBox.Value;
        }

        private void StrBox_ValueChanged(object sender, EventArgs e)
        {
            update_mods();
        }

        private void update_mods()
        {
            StrModBox.Text = get_text((int)StrBox.Value);
            ConModBox.Text = get_text((int)ConBox.Value);
            DexModBox.Text = get_text((int)DexBox.Value);
            IntModBox.Text = get_text((int)IntBox.Value);
            WisModBox.Text = get_text((int)WisBox.Value);
            ChaModBox.Text = get_text((int)ChaBox.Value);
        }

        private string get_text(int score)
        {
            var bonus = Ability.GetModifier(score);
            var mod = bonus + Creature.Level / 2;

            var bonusStr = bonus.ToString();
            if (bonus >= 0)
                bonusStr = "+" + bonusStr;

            var modStr = mod.ToString();
            if (mod >= 0)
                modStr = "+" + modStr;

            return bonusStr + " / " + modStr;
        }
    }
}
