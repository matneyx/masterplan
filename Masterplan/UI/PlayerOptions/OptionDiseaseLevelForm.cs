using System.Windows.Forms;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionDiseaseLevelForm : Form
    {
        public string DiseaseLevel => DetailsBox.Text;

        public OptionDiseaseLevelForm(string level)
        {
            InitializeComponent();

            DetailsBox.Text = level;
        }
    }
}
