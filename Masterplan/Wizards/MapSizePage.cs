using System.Windows.Forms;
using Masterplan.Tools.Generators;

namespace Masterplan.Wizards
{
    internal partial class MapSizePage : UserControl, IWizardPage
    {
        private MapBuilderData _fData;

        public MapSizePage()
        {
            InitializeComponent();
        }

        public bool AllowNext => false;

        public bool AllowBack => true;

        public bool AllowFinish => true;

        public void OnShown(object data)
        {
            if (_fData == null)
            {
                _fData = data as MapBuilderData;

                WidthBox.Value = _fData.Width;
                HeightBox.Value = _fData.Height;
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
            _fData.Width = (int)WidthBox.Value;
            _fData.Height = (int)HeightBox.Value;

            return true;
        }
    }
}
