using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.UI;

namespace Masterplan.Wizards
{
    internal partial class VariantBasePage : UserControl, IWizardPage
    {
        private VariantData _fData;

        public Creature SelectedCreature
        {
            get
            {
                if (CreatureList.SelectedItems.Count != 0)
                    return CreatureList.SelectedItems[0].Tag as Creature;

                return null;
            }
        }

        public VariantBasePage()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;
        }

        ~VariantBasePage()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            SearchClearBtn.Enabled = SearchBox.Text != "";
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            update_list();
        }

        private void SearchClearBtn_Click(object sender, EventArgs e)
        {
            SearchBox.Text = "";
        }

        private void update_list()
        {
            var creatures = Session.Creatures;

            var bst = new BinarySearchTree<string>();
            foreach (var c in creatures)
                if (c.Category != null && c.Category != "")
                    bst.Add(c.Category);

            var cats = bst.SortedList;
            cats.Add("Miscellaneous Creatures");

            CreatureList.BeginUpdate();

            CreatureList.Groups.Clear();
            foreach (var cat in cats)
                CreatureList.Groups.Add(cat, cat);

            var items = new List<ListViewItem>();
            foreach (var c in creatures)
                if (Match(c, SearchBox.Text))
                {
                    var lvi = new ListViewItem(c.Name);
                    lvi.SubItems.Add("Level " + c.Level + " " + c.Role);
                    if (c.Category != null && c.Category != "")
                        lvi.Group = CreatureList.Groups[c.Category];
                    else
                        lvi.Group = CreatureList.Groups["Miscellaneous Creatures"];
                    lvi.Tag = c;

                    items.Add(lvi);
                }

            CreatureList.Items.Clear();
            CreatureList.Items.AddRange(items.ToArray());

            CreatureList.EndUpdate();
        }

        private bool Match(Creature c, string query)
        {
            var tokens = query.Split(null);

            foreach (var token in tokens)
                if (!match_token(c, token))
                    return false;

            return true;
        }

        private bool match_token(Creature c, string token)
        {
            if (c.Name.ToLower().Contains(token.ToLower()))
                return true;

            if (c.Category != null)
                if (c.Category.ToLower().Contains(token.ToLower()))
                    return true;

            if (c.Info.ToLower().Contains(token.ToLower()))
                return true;

            if (c.Phenotype.ToLower().Contains(token.ToLower()))
                return true;

            return false;
        }

        private void CreatureList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedCreature != null)
            {
                var card = new EncounterCard(SelectedCreature.Id);
                var dlg = new CreatureDetailsForm(card);
                dlg.ShowDialog();
            }
        }

        public bool AllowNext => SelectedCreature != null;

        public bool AllowBack => false;

        public bool AllowFinish => false;

        public void OnShown(object data)
        {
            if (_fData == null)
            {
                _fData = data as VariantData;
                update_list();
            }
        }

        public bool OnBack()
        {
            return false;
        }

        public bool OnNext()
        {
            // Set base creature
            _fData.BaseCreature = SelectedCreature;

            if (_fData.BaseCreature.Role is Minion)
                _fData.Templates.Clear();

            return true;
        }

        public bool OnFinish()
        {
            return false;
        }
    }
}
