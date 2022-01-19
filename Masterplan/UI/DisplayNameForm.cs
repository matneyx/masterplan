using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class DisplayNameForm : Form
    {
        private readonly Encounter _fEncounter;

        public List<CombatData> Combatants { get; }

        public CombatData SelectedCombatant
        {
            get
            {
                if (CombatantList.SelectedItems.Count != 0)
                    return CombatantList.SelectedItems[0].Tag as CombatData;

                return null;
            }
        }

        public DisplayNameForm(List<CombatData> combatants, Encounter enc)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Combatants = combatants;
            _fEncounter = enc;

            Map map = null;
            if (_fEncounter.MapId != Guid.Empty)
                map = Session.Project.FindTacticalMap(_fEncounter.MapId);

            if (map == null)
            {
                Pages.TabPages.Remove(MapPage);
            }
            else
            {
                Map.Map = map;
                Map.Encounter = _fEncounter;
            }

            update_list();
            update_stat_block();
            update_map_area();
        }

        ~DisplayNameForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            NameBox.Enabled = SelectedCombatant != null;
            SetNameBtn.Enabled = NameBox.Text != "" && SelectedCombatant != null &&
                                 NameBox.Text != SelectedCombatant.DisplayName;
        }

        private void CombatantList_SelectedIndexChanged(object sender, EventArgs e)
        {
            NameBox.Text = SelectedCombatant != null ? SelectedCombatant.DisplayName : "";

            update_stat_block();
            update_map_area();
        }

        private void update_list()
        {
            CombatantList.Items.Clear();

            foreach (var cd in Combatants)
            {
                var lvi = CombatantList.Items.Add(cd.DisplayName);
                lvi.Tag = cd;
            }
        }

        private void update_stat_block()
        {
            var slot = _fEncounter.FindSlot(SelectedCombatant);
            var card = slot?.Card;
            Browser.DocumentText = Html.StatBlock(card, SelectedCombatant, _fEncounter, true, false, true,
                CardMode.View, Session.Preferences.TextSize);
        }

        private void update_map_area()
        {
            var view = Rectangle.Empty;

            Map.BoxedTokens.Clear();

            if (SelectedCombatant != null && SelectedCombatant.Location != CombatData.NoPoint)
            {
                var slot = _fEncounter.FindSlot(SelectedCombatant);
                Map.BoxedTokens.Add(new CreatureToken(slot.Id, SelectedCombatant));
                Map.MapChanged();

                var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                var size = Creature.GetSize(creature.Size);

                var dx = 7;
                var dy = 4;

                var left = SelectedCombatant.Location.X - dx;
                var top = SelectedCombatant.Location.Y - dy;
                var width = dx + size + dx;
                var height = dy + size + dy;

                view = new Rectangle(left, top, width, height);
            }
            else if (_fEncounter.MapAreaId != Guid.Empty)
            {
                var ma = Map.Map.FindArea(_fEncounter.MapAreaId);
                if (ma != null)
                    view = ma.Region;
            }

            Map.Viewpoint = view;
        }

        private void SetNameBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCombatant != null)
            {
                SelectedCombatant.DisplayName = NameBox.Text;

                update_list();
                update_stat_block();
                update_map_area();
            }
        }
    }
}
