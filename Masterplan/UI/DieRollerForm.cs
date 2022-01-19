using System.Windows.Forms;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class DieRollerForm : Form
    {
        public DiceExpression Expression
        {
            get => DicePanel.Expression;
            set => DicePanel.Expression = value;
        }

        public DieRollerForm()
        {
            InitializeComponent();

            DicePanel.UpdateView();
        }
    }
}
