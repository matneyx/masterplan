using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.UI;

namespace Masterplan.Controls.Elements
{
    internal partial class EncounterPanel : UserControl
    {
        private Encounter _encounter;

        private int _partyLevel = Session.Project.Party.Level;

        public Encounter Encounter
        {
            get => _encounter;
            set
            {
                _encounter = value;
                UpdateView();
            }
        }

        public int PartyLevel
        {
            get => _partyLevel;
            set
            {
                _partyLevel = value;
                UpdateView();
            }
        }

        private EncounterSlot SelectedSlot
        {
            get
            {
                if (ItemList.SelectedItems.Count != 0)
                    return ItemList.SelectedItems[0].Tag as EncounterSlot;

                return null;
            }
        }

        private Trap SelectedTrap
        {
            get
            {
                if (ItemList.SelectedItems.Count != 0)
                    return ItemList.SelectedItems[0].Tag as Trap;

                return null;
            }
        }

        private SkillChallenge SelectedChallenge
        {
            get
            {
                if (ItemList.SelectedItems.Count != 0)
                    return ItemList.SelectedItems[0].Tag as SkillChallenge;

                return null;
            }
        }

        public EncounterPanel()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;
        }

        ~EncounterPanel()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RunBtn.Enabled = _encounter.Count != 0 || _encounter.Traps.Count != 0 ||
                             _encounter.SkillChallenges.Count != 0;
        }

        public void Edit()
        {
            EditBtn_Click(null, null);
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            var encounterBuilderForm = new EncounterBuilderForm(_encounter, _partyLevel, false);

            if (encounterBuilderForm.ShowDialog() != DialogResult.OK) return;

            _encounter.Slots = encounterBuilderForm.Encounter.Slots;
            _encounter.Traps = encounterBuilderForm.Encounter.Traps;
            _encounter.SkillChallenges = encounterBuilderForm.Encounter.SkillChallenges;
            _encounter.CustomTokens = encounterBuilderForm.Encounter.CustomTokens;
            _encounter.MapId = encounterBuilderForm.Encounter.MapId;
            _encounter.MapAreaId = encounterBuilderForm.Encounter.MapAreaId;
            _encounter.Notes = encounterBuilderForm.Encounter.Notes;
            _encounter.Waves = encounterBuilderForm.Encounter.Waves;

            UpdateView();
        }

        private void RunBtn_Click(object sender, EventArgs e)
        {
            var combatState = new CombatState
            {
                Encounter = _encounter,
                PartyLevel = _partyLevel
            };

            var combatForm = new CombatForm(combatState);
            combatForm.Show();
        }

        private void UpdateView()
        {
            ItemList.Items.Clear();

            _encounter.Slots.ForEach(slot =>
            {
                var listViewItem = ItemList.Items.Add(slot.Card.Title);
                listViewItem.SubItems.Add(slot.Card.Info);
                listViewItem.SubItems.Add(slot.CombatData.Count.ToString());
                listViewItem.SubItems.Add(slot.Xp.ToString());
                listViewItem.Tag = slot;

                var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                var difficulty = Ai.GetThreatDifficulty(creature.Level + slot.Card.LevelAdjustment, _partyLevel);
                switch (difficulty)
                {
                    case Difficulty.Trivial:
                        listViewItem.ForeColor = Color.Green;
                        break;
                    case Difficulty.Extreme:
                        listViewItem.ForeColor = Color.Red;
                        break;
                }
            });

            _encounter.Traps.ForEach(trap =>
            {
                var listViewItem = ItemList.Items.Add(trap.Name);
                listViewItem.SubItems.Add(trap.Info);
                listViewItem.SubItems.Add("1");
                listViewItem.SubItems.Add(Experience.GetCreatureXp(trap.Level).ToString());
                listViewItem.Tag = trap;
            });

            _encounter.SkillChallenges.ForEach(sc =>
            {
                var listViewItem = ItemList.Items.Add(sc.Name);
                listViewItem.SubItems.Add(sc.Info);
                listViewItem.SubItems.Add("1");
                listViewItem.SubItems.Add(sc.GetXp().ToString());
                listViewItem.Tag = sc;
            });

            if (ItemList.Items.Count == 0)
            {
                var listViewItem = ItemList.Items.Add("(none)");
                listViewItem.ForeColor = SystemColors.GrayText;
            }

            ItemList.Sort();

            XPLbl.Text = _encounter.GetXp() + " XP";
            DiffLbl.Text = "Difficulty: " + _encounter.GetDifficulty(_partyLevel, Session.Project.Party.Size);
        }

        private void CreatureList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedSlot != null)
            {
                var creatureDetailsForm = new CreatureDetailsForm(SelectedSlot.Card);
                creatureDetailsForm.ShowDialog();
            }

            if (SelectedTrap != null)
            {
                var trapDetailsForm = new TrapDetailsForm(SelectedTrap);
                trapDetailsForm.ShowDialog();
            }

            if (SelectedChallenge != null)
            {
                var skillChallengeDetailsForm = new SkillChallengeDetailsForm(SelectedChallenge);
                skillChallengeDetailsForm.ShowDialog();
            }
        }
    }
}
