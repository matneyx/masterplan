using System;
using System.Windows.Forms;

namespace Masterplan.Wizards
{
    internal partial class VariantRolePage : UserControl, IWizardPage
    {
        private VariantData _fData;

        public VariantRolePage()
        {
            InitializeComponent();
        }

        private void RoleBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _fData.SelectedRoleIndex = RoleBox.SelectedIndex;
        }

        public bool AllowNext => true;

        public bool AllowBack => true;

        public bool AllowFinish => false;

        public void OnShown(object data)
        {
            if (_fData == null)
                _fData = data as VariantData;

            RoleBox.Items.Clear();
            foreach (var role in _fData.Roles)
                RoleBox.Items.Add(role);

            RoleBox.SelectedIndex = _fData.SelectedRoleIndex;
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
            return false;
        }
    }
}
