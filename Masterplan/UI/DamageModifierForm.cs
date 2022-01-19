using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class DamageModifierForm : Form
    {
        public DamageModifier Modifier { get; }

        public DamageModifierForm(DamageModifier dm)
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

            Modifier = dm.Copy();

            if (Modifier.Type == DamageType.Untyped)
                DamageTypeBox.SelectedIndex = 0;
            else
                DamageTypeBox.SelectedItem = Modifier.Type;

            if (Modifier.Value == 0) TypeBox.SelectedIndex = 0;
            if (Modifier.Value < 0)
            {
                TypeBox.SelectedIndex = 1;
                ValueBox.Value = Math.Abs(Modifier.Value);
            }

            if (Modifier.Value > 0)
            {
                TypeBox.SelectedIndex = 2;
                ValueBox.Value = Modifier.Value;
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Modifier.Type = (DamageType)DamageTypeBox.SelectedItem;

            switch (TypeBox.SelectedIndex)
            {
                case 0:
                    Modifier.Value = 0;
                    break;
                case 1:
                    var val = (int)ValueBox.Value;
                    Modifier.Value = -val;
                    break;
                case 2:
                    Modifier.Value = (int)ValueBox.Value;
                    break;
            }
        }

        private void TypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValueLbl.Enabled = TypeBox.SelectedIndex != 0;
            ValueBox.Enabled = TypeBox.SelectedIndex != 0;
        }
    }
}
