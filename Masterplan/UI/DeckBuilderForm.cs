using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.Tools.Generators;

namespace Masterplan.UI
{
    internal partial class DeckBuilderForm : Form
    {
        public EncounterDeck Deck { get; private set; }

        public EncounterCard SelectedCard
        {
            get
            {
                if (CardList.SelectedItems.Count != 0)
                    return CardList.SelectedItems[0].Tag as EncounterCard;

                return null;
            }
        }

        public ICreature SelectedCreature
        {
            get
            {
                if (CreatureList.SelectedItems.Count != 0)
                    return CreatureList.SelectedItems[0].Tag as ICreature;

                return null;
            }
        }

        public DeckBuilderForm(EncounterDeck deck)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Deck = deck.Copy();

            NameBox.Text = Deck.Name;
            LevelBox.Value = Deck.Level;
            DeckView.Deck = Deck;

            update_groups();
            DeckView_SelectedCellChanged(null, null);
        }

        ~DeckBuilderForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            DuplicateBtn.Enabled = SelectedCard != null;
            RemoveBtn.Enabled = SelectedCard != null;
            RefreshBtn.Enabled = SelectedCard != null && SelectedCard.Drawn;

            var drawn = false;
            foreach (var card in Deck.Cards)
                if (card.Drawn)
                {
                    drawn = true;
                    break;
                }

            RefillBtn.Enabled = drawn;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Deck.Name = NameBox.Text;
        }

        private void DuplicateBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCard != null)
            {
                var card = SelectedCard.Copy();
                Deck.Cards.Add(card);

                DeckView.Invalidate();
                update_card_list();
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCard != null)
            {
                if (Deck.Cards.Contains(SelectedCard))
                    Deck.Cards.Remove(SelectedCard);

                DeckView.Invalidate();
                update_card_list();
            }
        }

        private void RefreshBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCard != null && SelectedCard.Drawn)
            {
                SelectedCard.Drawn = false;

                DeckView.Invalidate();
                update_card_list();
            }
        }

        private void ViewBtn_Click(object sender, EventArgs e)
        {
            var cards = new List<EncounterCard>();

            foreach (var card in Deck.Cards)
            {
                if (!DeckView.InSelectedCell(card))
                    continue;

                cards.Add(card);
            }

            if (cards.Count != 0)
            {
                var dlg = new DeckViewForm(cards);
                dlg.ShowDialog();
            }
        }

        private void AutoBuildBtn_Click(object sender, EventArgs e)
        {
            var dlg = new AutoBuildForm(AutoBuildForm.Mode.Deck);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var deck = EncounterBuilder.BuildDeck(dlg.Data.Level, dlg.Data.Categories, dlg.Data.Keywords);
                if (deck != null)
                {
                    Deck = deck;

                    LevelBox.Value = Deck.Level;
                    DeckView.Deck = Deck;
                    DeckView_SelectedCellChanged(null, null);
                }
            }
        }

        private void RefillBtn_Click(object sender, EventArgs e)
        {
            foreach (var card in Deck.Cards)
                card.Drawn = false;

            DeckView.Invalidate();
            update_card_list();
        }

        private void CreatureList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedCreature != null)
            {
                var card = new EncounterCard();
                card.CreatureId = SelectedCreature.Id;

                var dlg = new CreatureDetailsForm(card);
                dlg.ShowDialog();
            }
        }

        private void CardList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedCard != null)
            {
                var dlg = new CreatureDetailsForm(SelectedCard);
                dlg.ShowDialog();
            }
        }

        private void CreatureList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedCreature != null)
                if (DoDragDrop(SelectedCreature, DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    var card = new EncounterCard();
                    card.CreatureId = SelectedCreature.Id;

                    Deck.Cards.Add(card);

                    DeckView.Invalidate();
                    update_card_list();
                }
        }

        private void DeckView_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            var c = e.Data.GetData(typeof(Creature)) as Creature;
            if (c != null)
                e.Effect = DragDropEffects.Copy;

            var cc = e.Data.GetData(typeof(CustomCreature)) as CustomCreature;
            if (cc != null)
                e.Effect = DragDropEffects.Copy;
        }

        private void DeckView_DragDrop(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            var c = e.Data.GetData(typeof(Creature)) as Creature;
            if (c != null)
                e.Effect = DragDropEffects.Copy;

            var cc = e.Data.GetData(typeof(CustomCreature)) as CustomCreature;
            if (cc != null)
                e.Effect = DragDropEffects.Copy;
        }

        private void LevelBox_ValueChanged(object sender, EventArgs e)
        {
            Deck.Level = (int)LevelBox.Value;
            DeckView.Invalidate();
        }

        private void DeckView_SelectedCellChanged(object sender, EventArgs e)
        {
            if (DeckView.IsCellSelected)
                InfoLbl.Text = "Drag creatures from this list onto the grid to the left to add them into your deck.";
            else
                InfoLbl.Text =
                    "Select a cell on the grid to display the list of creatures that can be added to the deck.";

            Cursor.Current = Cursors.WaitCursor;

            update_card_list();
            update_creature_list();

            Cursor.Current = Cursors.Default;
        }

        private void DeckView_CellActivated(object sender, EventArgs e)
        {
            ViewBtn_Click(null, null);
        }

        private void update_groups()
        {
            var bst = new BinarySearchTree<string>();

            foreach (var c in Session.Creatures)
                if (c.Category != "")
                    bst.Add(c.Category);

            var cats = bst.SortedList;
            cats.Insert(0, "Custom Creatures");
            cats.Add("Miscellaneous Creatures");

            foreach (var cat in cats)
            {
                CardList.Groups.Add(cat, cat);
                CreatureList.Groups.Add(cat, cat);
            }
        }

        private void update_card_list()
        {
            CardList.BeginUpdate();
            CardList.ShowGroups = true;

            var itemList = new List<ListViewItem>();

            if (DeckView.IsCellSelected)
            {
                foreach (var card in Deck.Cards)
                {
                    if (!DeckView.InSelectedCell(card))
                        continue;

                    itemList.Add(add_card(card));
                }

                if (itemList.Count == 0)
                {
                    CardList.ShowGroups = false;

                    var lvi = new ListViewItem("(no cards)");
                    lvi.ForeColor = SystemColors.GrayText;

                    itemList.Add(lvi);
                }
            }
            else
            {
                CardList.ShowGroups = false;

                var lvi = new ListViewItem("(no cell selected)");
                lvi.ForeColor = SystemColors.GrayText;

                itemList.Add(lvi);
            }

            CardList.Items.Clear();
            CardList.Items.AddRange(itemList.ToArray());
            CardList.EndUpdate();

            ViewBtn.Enabled = itemList.Count != 0;
        }

        private ListViewItem add_card(EncounterCard card)
        {
            var lvi = new ListViewItem(card.Title);
            lvi.SubItems.Add(card.Info);
            lvi.Tag = card;

            if (card.Drawn)
                lvi.ForeColor = SystemColors.GrayText;

            var c = Session.FindCreature(card.CreatureId, SearchType.Global);
            var catName = c.Category != "" ? c.Category : "Miscellaneous Creatures";
            lvi.Group = CardList.Groups[catName];

            return lvi;
        }

        private void update_creature_list()
        {
            CreatureList.BeginUpdate();
            CreatureList.ShowGroups = true;

            var itemList = new List<ListViewItem>();

            if (DeckView.IsCellSelected)
            {
                var creatures = new List<ICreature>();
                foreach (var c in Session.Creatures)
                    creatures.Add(c);
                foreach (var cc in Session.Project.CustomCreatures)
                    creatures.Add(cc);

                foreach (var c in creatures)
                {
                    var card = new EncounterCard();
                    card.CreatureId = c.Id;

                    if (!DeckView.InSelectedCell(card))
                        continue;

                    var lvi = new ListViewItem(c.Name);
                    lvi.Tag = c;

                    var catName = c.Category != "" ? c.Category : "Miscellaneous Creatures";
                    lvi.Group = CreatureList.Groups[catName];

                    itemList.Add(lvi);
                }

                if (itemList.Count == 0)
                {
                    CreatureList.ShowGroups = false;

                    var lvi = new ListViewItem("(no creatures)");
                    lvi.ForeColor = SystemColors.GrayText;

                    itemList.Add(lvi);
                }
            }
            else
            {
                CreatureList.ShowGroups = false;

                var lvi = new ListViewItem("(no cell selected)");
                lvi.ForeColor = SystemColors.GrayText;

                itemList.Add(lvi);
            }

            CreatureList.Items.Clear();
            CreatureList.Items.AddRange(itemList.ToArray());
            CreatureList.EndUpdate();
        }
    }
}
