using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class PowerRangeForm : Form
    {
        public string PowerRange => RangeBox.Text;

        public PowerRangeForm(CreaturePower power)
        {
            InitializeComponent();

            RangeBox.Items.Add("Melee");
            RangeBox.Items.Add("Melee Touch");
            RangeBox.Items.Add("Melee Weapon");
            RangeBox.Items.Add("Melee N");
            RangeBox.Items.Add("Reach N");
            RangeBox.Items.Add("Ranged N");
            RangeBox.Items.Add("Close Blast N");
            RangeBox.Items.Add("Close Burst N");
            RangeBox.Items.Add("Area Burst N within N");
            RangeBox.Items.Add("Personal");

            RangeBox.Text = power.Range;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }
    }
}
