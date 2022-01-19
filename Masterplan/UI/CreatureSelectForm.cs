using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.Tools.Generators;

namespace Masterplan.UI
{
    internal partial class CreatureSelectForm : Form
    {
        private readonly List<Creature> _fCreatures;
        private readonly int _fLevel = 1;
        private readonly EncounterTemplateSlot _fTemplateSlot;

        public EncounterCard Creature
        {
            get
            {
                if (CreatureList.SelectedItems.Count != 0)
                    return CreatureList.SelectedItems[0].Tag as EncounterCard;

                return null;
            }
        }

        public CreatureSelectForm(EncounterTemplateSlot slot, int level)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fTemplateSlot = slot;
            _fLevel = level;

            update_list();

            Browser.DocumentText = "";
            CreatureList_SelectedIndexChanged(null, null);
        }

        public CreatureSelectForm(List<Creature> creatures)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fCreatures = creatures;

            update_list();

            Browser.DocumentText = "";
            CreatureList_SelectedIndexChanged(null, null);
        }

        ~CreatureSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = Creature != null;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (Creature != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void CreatureList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var html = Html.StatBlock(Creature, null, null, true, false, true, CardMode.View,
                Session.Preferences.TextSize);

            Browser.Document.OpenNew(true);
            Browser.Document.Write(html);
        }

        private void update_list()
        {
            CreatureList.Groups.Clear();
            CreatureList.Items.Clear();

            List<EncounterCard> cards = null;
            if (_fCreatures != null)
            {
                cards = new List<EncounterCard>();
                foreach (var creature in _fCreatures)
                    cards.Add(new EncounterCard(creature.Id));
            }
            else
            {
                cards = EncounterBuilder.FindCreatures(_fTemplateSlot, _fLevel, NameBox.Text);
            }

            var bst = new BinarySearchTree<string>();
            foreach (var card in cards)
            {
                var c = Session.FindCreature(card.CreatureId, SearchType.Global);
                bst.Add(c.Category);
            }

            var cats = bst.SortedList;
            cats.Add("Miscellaneous Creatures");
            foreach (var cat in cats)
                CreatureList.Groups.Add(cat, cat);

            foreach (var card in cards)
            {
                var c = Session.FindCreature(card.CreatureId, SearchType.Global);

                var lvi = CreatureList.Items.Add(card.Title);
                lvi.SubItems.Add(card.Info);
                lvi.Tag = card;

                if (c.Category != null && c.Category != "")
                    lvi.Group = CreatureList.Groups[c.Category];
                else
                    lvi.Group = CreatureList.Groups["Miscellaneous Creatures"];
            }
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            update_list();
        }

        private bool Match(Trap trap, string query)
        {
            var tokens = query.ToLower().Split();

            foreach (var token in tokens)
                if (!match_token(trap, token))
                    return false;

            return true;
        }

        private bool match_token(Trap trap, string token)
        {
            if (trap.Name.ToLower().Contains(token))
                return true;

            return false;
        }
    }
}
