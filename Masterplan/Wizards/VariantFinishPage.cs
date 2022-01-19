using System.Windows.Forms;

namespace Masterplan.Wizards
{
    internal partial class VariantFinishPage : UserControl, IWizardPage
    {
        public VariantFinishPage()
        {
            InitializeComponent();
        }

        public bool AllowNext => false;

        public bool AllowBack => true;

        public bool AllowFinish => true;

        public void OnShown(object data)
        {
        }

        public bool OnBack()
        {
            return true;
        }

        public bool OnNext()
        {
            return false;
        }

        public bool OnFinish()
        {
            return true;
        }
    }
}
