using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.UI;

namespace Masterplan.Wizards
{
    internal partial class VariantTemplatesPage : UserControl, IWizardPage
    {
        private VariantData _fData;

        public VariantTemplatesPage()
        {
            InitializeComponent();
        }

        private void TemplateList_DoubleClick(object sender, EventArgs e)
        {
            if (TemplateList.SelectedItems.Count != 0)
            {
                var ct = TemplateList.SelectedItems[0].Tag as CreatureTemplate;
                if (ct != null)
                {
                    var dlg = new CreatureTemplateDetailsForm(ct);
                    dlg.ShowDialog();
                }
            }
        }

        public bool AllowNext => true;

        public bool AllowBack => true;

        public bool AllowFinish => false;

        public void OnShown(object data)
        {
            if (_fData == null)
            {
                _fData = data as VariantData;

                var templates = Session.Templates;
                foreach (var ct in templates)
                {
                    var lvi = TemplateList.Items.Add(ct.Name);
                    lvi.SubItems.Add(ct.Info);
                    lvi.Tag = ct;
                }
            }
        }

        public bool OnBack()
        {
            return true;
        }

        public bool OnNext()
        {
            var steps = 0;
            var role = _fData.BaseCreature.Role as ComplexRole;
            switch (role.Flag)
            {
                case RoleFlag.Elite:
                    steps = 1;
                    break;
                case RoleFlag.Solo:
                    steps = 2;
                    break;
            }

            steps += TemplateList.CheckedItems.Count;

            if (steps > 2)
            {
                var str = "You can not normally apply that many templates to this creature.";
                str += Environment.NewLine;
                str += "Are you sure you want to continue?";

                var dr = MessageBox.Show(str, "Creature Builder", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No)
                    return false;
            }

            // Set templates
            _fData.Templates.Clear();
            foreach (ListViewItem lvi in TemplateList.CheckedItems)
                _fData.Templates.Add(lvi.Tag as CreatureTemplate);

            return true;
        }

        public bool OnFinish()
        {
            return false;
        }
    }
}
