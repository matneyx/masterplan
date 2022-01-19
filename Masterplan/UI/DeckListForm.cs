using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class DeckListForm : Form
    {
        public EncounterDeck SelectedDeck
        {
            get
            {
                if (DeckList.SelectedItems.Count != 0)
                    return DeckList.SelectedItems[0].Tag as EncounterDeck;

                return null;
            }
        }

        public DeckListForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            update_decks();
        }

        ~DeckListForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = SelectedDeck != null;
            EditBtn.Enabled = SelectedDeck != null;

            ViewBtn.Enabled = SelectedDeck != null && SelectedDeck.Cards.Count != 0;
            RunBtn.Enabled = SelectedDeck != null && SelectedDeck.Cards.Count != 0;
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var deck = new EncounterDeck();
            deck.Name = "New Deck";
            deck.Level = Session.Project.Party.Level;

            var dlg = new DeckBuilderForm(deck);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.Decks.Add(dlg.Deck);
                Session.Modified = true;

                update_decks();
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDeck != null)
            {
                Session.Project.Decks.Remove(SelectedDeck);
                Session.Modified = true;

                update_decks();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDeck != null)
            {
                var index = Session.Project.Decks.IndexOf(SelectedDeck);

                var dlg = new DeckBuilderForm(SelectedDeck);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.Decks[index] = dlg.Deck;
                    Session.Modified = true;

                    update_decks();
                }
            }
        }

        private void ViewBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDeck != null)
            {
                var dlg = new DeckViewForm(SelectedDeck.Cards);
                dlg.ShowDialog();
            }
        }

        private void RunBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDeck != null)
                run_encounter(SelectedDeck, false);
        }

        private void RunMap_Click(object sender, EventArgs e)
        {
            if (SelectedDeck != null)
                run_encounter(SelectedDeck, true);
        }

        private void run_encounter(EncounterDeck deck, bool chooseMap)
        {
            MapAreaSelectForm mapDlg = null;
            if (chooseMap)
            {
                mapDlg = new MapAreaSelectForm(Guid.Empty, Guid.Empty);
                if (mapDlg.ShowDialog() != DialogResult.OK)
                    return;
            }

            var enc = new Encounter();
            var ok = deck.DrawEncounter(enc);
            update_decks();

            if (ok)
            {
                var cs = new CombatState();
                cs.Encounter = enc;
                cs.PartyLevel = Session.Project.Party.Level;

                if (mapDlg?.Map != null)
                {
                    cs.Encounter.MapId = mapDlg.Map.Id;

                    if (mapDlg.MapArea != null)
                        cs.Encounter.MapAreaId = mapDlg.MapArea.Id;
                }

                var dlg = new CombatForm(cs);
                dlg.Show();
            }
            else
            {
                var str =
                    "An encounter could not be built from this deck; check that there are enough cards remaining.";
                MessageBox.Show(str, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void update_decks()
        {
            DeckList.Items.Clear();

            foreach (var deck in Session.Project.Decks)
            {
                var available = 0;
                foreach (var card in deck.Cards)
                    if (!card.Drawn)
                        available += 1;

                var count = deck.Cards.Count.ToString();
                if (available != deck.Cards.Count)
                    count = available + " / " + deck.Cards.Count;

                var lvi = DeckList.Items.Add(deck.Name);
                lvi.SubItems.Add(deck.Level.ToString());
                lvi.SubItems.Add(count);
                lvi.Tag = deck;
            }

            if (DeckList.Items.Count == 0)
            {
                var lvi = DeckList.Items.Add("(no decks)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
