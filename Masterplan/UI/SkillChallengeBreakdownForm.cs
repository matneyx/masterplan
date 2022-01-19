using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class SkillChallengeBreakdownForm : Form
    {
        public SkillChallengeBreakdownForm(SkillChallenge sc)
        {
            InitializeComponent();

            AbilitiesPanel.Analyse(sc);
        }
    }
}
