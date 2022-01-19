using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CreatureTemplateStatsForm : Form
    {
        public CreatureTemplate Template { get; }

        public CreatureTemplateStatsForm(CreatureTemplate t)
        {
            InitializeComponent();

            Template = t.Copy();

            HPBox.Value = Template.Hp;
            InitBox.Value = Template.Initiative;
            ACBox.Value = Template.Ac;
            FortBox.Value = Template.Fortitude;
            RefBox.Value = Template.Reflex;
            WillBox.Value = Template.Will;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Template.Hp = (int)HPBox.Value;
            Template.Initiative = (int)InitBox.Value;
            Template.Ac = (int)ACBox.Value;
            Template.Fortitude = (int)FortBox.Value;
            Template.Reflex = (int)RefBox.Value;
            Template.Will = (int)WillBox.Value;
        }
    }
}
