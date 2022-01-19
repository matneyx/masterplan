using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionWeaponForm : Form
    {
        public Weapon Weapon { get; }

        public OptionWeaponForm(Weapon weapon)
        {
            InitializeComponent();

            var cats = Enum.GetValues(typeof(WeaponCategory));
            foreach (WeaponCategory cat in cats)
                CatBox.Items.Add(cat);

            var types = Enum.GetValues(typeof(WeaponType));
            foreach (WeaponType type in types)
                TypeBox.Items.Add(type);

            GroupBox.Items.Add("Axe");
            GroupBox.Items.Add("Box");
            GroupBox.Items.Add("Crossbow");
            GroupBox.Items.Add("Flail");
            GroupBox.Items.Add("Hammer");
            GroupBox.Items.Add("Heavy Blade");
            GroupBox.Items.Add("Light Blade");
            GroupBox.Items.Add("Mace");
            GroupBox.Items.Add("Pick");
            GroupBox.Items.Add("Polearm");
            GroupBox.Items.Add("Sling");
            GroupBox.Items.Add("Spear");
            GroupBox.Items.Add("Staff");
            GroupBox.Items.Add("Unarmed");

            PropertiesBox.Items.Add("Brutal 1");
            PropertiesBox.Items.Add("Brutal 2");
            PropertiesBox.Items.Add("Defensive");
            PropertiesBox.Items.Add("Heavy Thrown");
            PropertiesBox.Items.Add("High Crit");
            PropertiesBox.Items.Add("Light Thrown");
            PropertiesBox.Items.Add("Load Free");
            PropertiesBox.Items.Add("Load Minor");
            PropertiesBox.Items.Add("Off-Hand");
            PropertiesBox.Items.Add("Reach");
            PropertiesBox.Items.Add("Small");
            PropertiesBox.Items.Add("Stout");
            PropertiesBox.Items.Add("Versatile");

            Weapon = weapon.Copy();

            NameBox.Text = Weapon.Name;
            CatBox.SelectedItem = Weapon.Category;
            TypeBox.SelectedItem = Weapon.Type;
            TwoHandBox.Checked = Weapon.TwoHanded;
            ProfBox.Value = Weapon.Proficiency;
            DamageBox.Text = Weapon.Damage;
            RangeBox.Text = Weapon.Range;
            PriceBox.Text = Weapon.Price;
            WeightBox.Text = Weapon.Weight;
            GroupBox.Text = Weapon.Group;
            PropertiesBox.Text = Weapon.Properties;
            DetailsBox.Text = Weapon.Description;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Weapon.Name = NameBox.Text;
            Weapon.Category = (WeaponCategory)CatBox.SelectedItem;
            Weapon.Type = (WeaponType)TypeBox.SelectedItem;
            Weapon.TwoHanded = TwoHandBox.Checked;
            Weapon.Proficiency = (int)ProfBox.Value;
            Weapon.Damage = DamageBox.Text;
            Weapon.Range = RangeBox.Text;
            Weapon.Price = PriceBox.Text;
            Weapon.Weight = WeightBox.Text;
            Weapon.Group = GroupBox.Text;
            Weapon.Properties = PropertiesBox.Text;
            Weapon.Description = DetailsBox.Text;
        }
    }
}
