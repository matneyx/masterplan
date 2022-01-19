using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class PowerDetailsForm : Form
    {
        private readonly ICreature _fCreature;

        public string Details => DetailsBox.Text;

        public PowerDetailsForm(string str, ICreature creature)
        {
            InitializeComponent();

            DetailsBox.Text = str;
            _fCreature = creature;

            var level = _fCreature?.Level ?? 0;
            var role = _fCreature?.Role;

            var damage = "1d8 + 2";
            if (role != null)
            {
                if (role is Minion)
                    damage = Statistics.Damage(level, DamageExpressionType.Minion);
                else
                    damage = Statistics.Damage(level, DamageExpressionType.Normal);
            }

            var examples = new List<string>();
            examples.Add(damage + " damage");
            examples.Add(damage + " damage, and the target is knocked prone");
            examples.Add("The target is slowed (save ends)");
            examples.Add("The target is immobilised until the start of your next turn");

            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);
            lines.Add("<BODY>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD><B>Examples</B></TD>");
            lines.Add("</TR>");

            foreach (var example in examples)
            {
                lines.Add("<TR>");
                lines.Add("<TD>" + example + "</TD>");
                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            Browser.DocumentText = Html.Concatenate(lines);
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }
    }
}
