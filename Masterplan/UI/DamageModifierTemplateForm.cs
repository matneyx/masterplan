using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class DamageModifierTemplateForm : Form
    {
        public DamageModifierTemplate Modifier { get; }

        public DamageModifierTemplateForm(DamageModifierTemplate dmt)
        {
            InitializeComponent();

            foreach (DamageType type in Enum.GetValues(typeof(DamageType)))
            {
                if (type == DamageType.Untyped)
                    continue;

                DamageTypeBox.Items.Add(type);
            }

            TypeBox.Items.Add("Immunity to this damage type");
            TypeBox.Items.Add("Resistance to this damage type");
            TypeBox.Items.Add("Vulnerability to this damage type");

            Modifier = dmt.Copy();

            if (Modifier.Type == DamageType.Untyped)
                DamageTypeBox.SelectedIndex = 0;
            else
                DamageTypeBox.SelectedItem = Modifier.Type;

            var totalMod = Modifier.HeroicValue + Modifier.ParagonValue + Modifier.EpicValue;
            if (totalMod == 0) TypeBox.SelectedIndex = 0;
            if (totalMod < 0) TypeBox.SelectedIndex = 1;
            if (totalMod > 0) TypeBox.SelectedIndex = 2;

            HeroicBox.Value = Math.Abs(Modifier.HeroicValue);
            ParagonBox.Value = Math.Abs(Modifier.ParagonValue);
            EpicBox.Value = Math.Abs(Modifier.EpicValue);
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Modifier.Type = (DamageType)DamageTypeBox.SelectedItem;

            switch (TypeBox.SelectedIndex)
            {
                case 0:
                    Modifier.HeroicValue = 0;
                    Modifier.ParagonValue = 0;
                    Modifier.EpicValue = 0;
                    break;
                case 1:
                    Modifier.HeroicValue = -(int)HeroicBox.Value;
                    Modifier.ParagonValue = -(int)ParagonBox.Value;
                    Modifier.EpicValue = -(int)EpicBox.Value;
                    break;
                case 2:
                    Modifier.HeroicValue = (int)HeroicBox.Value;
                    Modifier.ParagonValue = (int)ParagonBox.Value;
                    Modifier.EpicValue = (int)EpicBox.Value;
                    break;
            }
        }

        private void TypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            HeroicLbl.Enabled = TypeBox.SelectedIndex != 0;
            HeroicBox.Enabled = TypeBox.SelectedIndex != 0;

            ParagonLbl.Enabled = TypeBox.SelectedIndex != 0;
            ParagonBox.Enabled = TypeBox.SelectedIndex != 0;

            EpicLbl.Enabled = TypeBox.SelectedIndex != 0;
            EpicBox.Enabled = TypeBox.SelectedIndex != 0;
        }
    }
}
