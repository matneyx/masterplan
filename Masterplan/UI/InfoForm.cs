using System.Windows.Forms;

namespace Masterplan.UI
{
    internal partial class InfoForm : Form
    {
        public int Level
        {
            get => InfoPanel.Level;
            set => InfoPanel.Level = value;
        }

        public InfoForm()
        {
            InitializeComponent();

            InfoPanel.Level = Session.Project != null ? Session.Project.Party.Level : 1;
        }
    }
}
