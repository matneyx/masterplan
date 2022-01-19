using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class PausedCombatListForm : Form
    {
        public CombatState SelectedCombat
        {
            get
            {
                if (EncounterList.SelectedItems.Count != 0)
                    return EncounterList.SelectedItems[0].Tag as CombatState;

                return null;
            }
        }

        public PausedCombatListForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            update_list();
            set_map();
        }

        ~PausedCombatListForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RunBtn.Enabled = SelectedCombat != null;
            RemoveBtn.Enabled = SelectedCombat != null;
        }

        public void UpdateEncounters()
        {
            update_list();
            set_map();
        }

        private void RunBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCombat != null)
            {
                Session.Project.SavedCombats.Remove(SelectedCombat);
                Session.Modified = true;

                Close();

                var dlg = new CombatForm(SelectedCombat);
                dlg.Show();
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCombat != null)
            {
                Session.Project.SavedCombats.Remove(SelectedCombat);
                Session.Modified = true;

                update_list();
                set_map();
            }
        }

        private void EncounterList_SelectedIndexChanged(object sender, EventArgs e)
        {
            set_map();
        }

        private void update_list()
        {
            EncounterList.Items.Clear();

            foreach (var cs in Session.Project.SavedCombats)
            {
                var lvi = EncounterList.Items.Add(cs.ToString());
                lvi.Tag = cs;
            }

            if (Session.Project.SavedCombats.Count == 0)
            {
                var lvi = EncounterList.Items.Add("(none)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void set_map()
        {
            if (SelectedCombat != null)
            {
                var m = Session.Project.FindTacticalMap(SelectedCombat.Encounter.MapId);

                MapView.Map = m;
                MapView.Viewpoint = SelectedCombat.Viewpoint;
                MapView.Encounter = SelectedCombat.Encounter;
                MapView.TokenLinks = SelectedCombat.TokenLinks;

                MapView.Sketches.Clear();
                foreach (var sketch in SelectedCombat.Sketches)
                    MapView.Sketches.Add(sketch.Copy());
            }
            else
            {
                MapView.Map = null;
            }
        }
    }
}
