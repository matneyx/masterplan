using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class MagicItemProfileForm : Form
    {
        public MagicItem MagicItem { get; }

        public MagicItemProfileForm(MagicItem item)
        {
            InitializeComponent();

            TypeBox.Items.Add("Armour");
            TypeBox.Items.Add("Weapon");
            TypeBox.Items.Add("Ammunition");
            TypeBox.Items.Add("Item Slot (head)");
            TypeBox.Items.Add("Item Slot (neck)");
            TypeBox.Items.Add("Item Slot (waist)");
            TypeBox.Items.Add("Item Slot (arms)");
            TypeBox.Items.Add("Item Slot (hands)");
            TypeBox.Items.Add("Item Slot (feet)");
            TypeBox.Items.Add("Item Slot (ring)");
            TypeBox.Items.Add("Implement");
            TypeBox.Items.Add("Alchemical Item");
            TypeBox.Items.Add("Divine Boon");
            TypeBox.Items.Add("Grandmaster Training");
            TypeBox.Items.Add("Potion");
            TypeBox.Items.Add("Reagent");
            TypeBox.Items.Add("Whetstone");
            TypeBox.Items.Add("Wondrous Item");

            var rarities = Enum.GetValues(typeof(MagicItemRarity));
            foreach (MagicItemRarity mir in rarities)
                RarityBox.Items.Add(mir);

            MagicItem = item.Copy();

            NameBox.Text = MagicItem.Name;
            LevelBox.Value = MagicItem.Level;
            TypeBox.Text = MagicItem.Type;
            RarityBox.SelectedItem = MagicItem.Rarity;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            MagicItem.Name = NameBox.Text;
            MagicItem.Level = (int)LevelBox.Value;
            MagicItem.Type = TypeBox.Text;
            MagicItem.Rarity = (MagicItemRarity)RarityBox.SelectedItem;
        }
    }
}
