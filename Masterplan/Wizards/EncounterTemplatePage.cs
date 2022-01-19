using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.Wizards
{
    internal partial class EncounterTemplatePage : UserControl, IWizardPage
    {
        private AdviceData _fData;

        public EncounterTemplate SelectedTemplate
        {
            get
            {
                if (TemplatesList.SelectedItems.Count != 0)
                    return TemplatesList.SelectedItems[0].Tag as EncounterTemplate;

                return null;
            }
        }

        public EncounterTemplatePage()
        {
            InitializeComponent();
        }

        public bool AllowNext => SelectedTemplate != null;

        public bool AllowBack => false;

        public bool AllowFinish => false;

        public void OnShown(object data)
        {
            if (_fData == null)
            {
                _fData = data as AdviceData;

                if (_fData.TabulaRasa)
                    InfoLbl.Text = "The following encounter templates are available. Select one to continue.";
                else
                    InfoLbl.Text =
                        "The following encounter templates fit the creatures you have added to the encounter so far. Select one to continue.";

                var bst = new BinarySearchTree<string>();
                foreach (var template in _fData.Templates)
                    bst.Add(template.First.Category);

                var cats = bst.SortedList;
                foreach (var cat in cats)
                    TemplatesList.Groups.Add(cat, cat);

                TemplatesList.Items.Clear();
                foreach (var template in _fData.Templates)
                {
                    var lvi = TemplatesList.Items.Add(template.First.Name + " (" +
                                                      template.Second.Difficulty.ToString().ToLower() + ")");
                    lvi.Tag = template.Second;
                    lvi.Group = TemplatesList.Groups[template.First.Category];
                }

                if (TemplatesList.Items.Count == 0)
                {
                    TemplatesList.ShowGroups = false;

                    var lvi = TemplatesList.Items.Add("(no templates)");
                    lvi.ForeColor = SystemColors.GrayText;
                }
            }
        }

        public bool OnBack()
        {
            return true;
        }

        public bool OnNext()
        {
            if (_fData.SelectedTemplate != SelectedTemplate)
            {
                _fData.SelectedTemplate = SelectedTemplate;
                _fData.FilledSlots.Clear();
            }

            return true;
        }

        public bool OnFinish()
        {
            return true;
        }
    }
}
