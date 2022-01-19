using System;
using System.Windows.Forms;
using Masterplan.Tools.Generators;

namespace Masterplan.Wizards
{
    internal partial class MapAreasPage : UserControl, IWizardPage
    {
        private MapBuilderData _fData;

        public MapAreasPage()
        {
            InitializeComponent();
        }

        private void MaxAreasBox_ValueChanged(object sender, EventArgs e)
        {
            MinAreasBox.Maximum = MaxAreasBox.Value;
        }

        public bool AllowNext => false;

        public bool AllowBack => true;

        public bool AllowFinish => true;

        public void OnShown(object data)
        {
            if (_fData == null)
            {
                _fData = data as MapBuilderData;
                MaxAreasBox.Value = _fData.MaxAreaCount;
                MinAreasBox.Value = _fData.MinAreaCount;
            }
        }

        public bool OnBack()
        {
            return true;
        }

        public bool OnNext()
        {
            return true;
        }

        public bool OnFinish()
        {
            _fData.MaxAreaCount = (int)MaxAreasBox.Value;
            _fData.MinAreaCount = (int)MinAreasBox.Value;
            return true;
        }
    }
}
