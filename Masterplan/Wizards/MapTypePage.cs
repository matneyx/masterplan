using System.Windows.Forms;
using Masterplan.Tools.Generators;

namespace Masterplan.Wizards
{
    internal partial class MapTypePage : UserControl, IWizardPage
    {
        private MapBuilderData _fData;

        public MapTypePage()
        {
            InitializeComponent();
        }

        private void set_data()
        {
            if (DungeonBtn.Checked)
                _fData.Type = MapAutoBuildType.Warren;

            if (AreaBtn.Checked)
                _fData.Type = MapAutoBuildType.FilledArea;

            if (FreeformBtn.Checked)
                _fData.Type = MapAutoBuildType.Freeform;
        }

        public bool AllowNext => true;

        public bool AllowBack => false;

        public bool AllowFinish => false;

        public void OnShown(object data)
        {
            if (_fData == null)
            {
                _fData = data as MapBuilderData;

                switch (_fData.Type)
                {
                    case MapAutoBuildType.Warren:
                        DungeonBtn.Checked = true;
                        break;
                    case MapAutoBuildType.FilledArea:
                        AreaBtn.Checked = true;
                        break;
                    case MapAutoBuildType.Freeform:
                        FreeformBtn.Checked = true;
                        break;
                }
            }
        }

        public bool OnBack()
        {
            return true;
        }

        public bool OnNext()
        {
            set_data();
            return true;
        }

        public bool OnFinish()
        {
            set_data();
            return true;
        }
    }
}
