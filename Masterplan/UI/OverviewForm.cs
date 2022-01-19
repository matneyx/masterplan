using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal enum OverviewType
    {
        Encounters,
        Traps,
        SkillChallenges,
        Treasure
    }

    internal partial class OverviewForm : Form
    {
        private readonly List<PlotPoint> _fPoints = new List<PlotPoint>();

        private OverviewType _fType = OverviewType.Encounters;

        public Pair<PlotPoint, Encounter> SelectedEncounter
        {
            get
            {
                if (ItemList.SelectedItems.Count != 0)
                    return ItemList.SelectedItems[0].Tag as Pair<PlotPoint, Encounter>;

                return null;
            }
        }

        public Pair<PlotPoint, Trap> SelectedTrap
        {
            get
            {
                if (ItemList.SelectedItems.Count != 0)
                    return ItemList.SelectedItems[0].Tag as Pair<PlotPoint, Trap>;

                return null;
            }
        }

        public Pair<PlotPoint, SkillChallenge> SelectedChallenge
        {
            get
            {
                if (ItemList.SelectedItems.Count != 0)
                    return ItemList.SelectedItems[0].Tag as Pair<PlotPoint, SkillChallenge>;

                return null;
            }
        }

        public Pair<PlotPoint, Parcel> SelectedParcel
        {
            get
            {
                if (ItemList.SelectedItems.Count != 0)
                    return ItemList.SelectedItems[0].Tag as Pair<PlotPoint, Parcel>;

                return null;
            }
        }

        public OverviewForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            add_points(null);

            update_list();
        }

        ~OverviewForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            EncounterBtn.Checked = _fType == OverviewType.Encounters;
            TrapBtn.Checked = _fType == OverviewType.Traps;
            ChallengeBtn.Checked = _fType == OverviewType.SkillChallenges;
            TreasureBtn.Checked = _fType == OverviewType.Treasure;
        }

        private void EncounterBtn_Click(object sender, EventArgs e)
        {
            _fType = OverviewType.Encounters;
            update_list();
        }

        private void TrapBtn_Click(object sender, EventArgs e)
        {
            _fType = OverviewType.Traps;
            update_list();
        }

        private void ChallengeBtn_Click(object sender, EventArgs e)
        {
            _fType = OverviewType.SkillChallenges;
            update_list();
        }

        private void TreasureBtn_Click(object sender, EventArgs e)
        {
            _fType = OverviewType.Treasure;
            update_list();
        }

        private void update_list()
        {
            ItemList.Items.Clear();

            switch (_fType)
            {
                case OverviewType.Encounters:
                {
                    foreach (var pp in _fPoints)
                        if (pp.Element is Encounter)
                        {
                            var enc = pp.Element as Encounter;

                            var creatures = "";
                            foreach (var slot in enc.AllSlots)
                            {
                                if (creatures != "")
                                    creatures += ", ";

                                creatures += slot.Card.Title;
                                if (slot.CombatData.Count != 1)
                                    creatures += " (x" + slot.CombatData.Count + ")";
                            }

                            foreach (var trap in enc.Traps)
                            {
                                if (creatures != "")
                                    creatures += ", ";

                                creatures += trap.Name;
                            }

                            var lvi = ItemList.Items.Add(pp.Name);
                            lvi.SubItems.Add(enc.GetXp() + " XP; " + creatures);
                            lvi.Tag = new Pair<PlotPoint, Encounter>(pp, enc);
                        }
                }
                    break;
                case OverviewType.Traps:
                {
                    foreach (var pp in _fPoints)
                    {
                        if (pp.Element == null)
                            continue;

                        if (pp.Element is TrapElement)
                        {
                            var te = pp.Element as TrapElement;

                            var lvi = ItemList.Items.Add(pp.Name);
                            lvi.SubItems.Add(Experience.GetCreatureXp(te.Trap.Level) + " XP; " + te.Trap.Name);
                            lvi.Tag = new Pair<PlotPoint, Trap>(pp, te.Trap);
                        }

                        if (pp.Element is Encounter)
                        {
                            var enc = pp.Element as Encounter;

                            foreach (var trap in enc.Traps)
                            {
                                var lvi = ItemList.Items.Add(pp.Name);
                                lvi.SubItems.Add(Experience.GetCreatureXp(trap.Level) + " XP; " + trap.Name);
                                lvi.Tag = new Pair<PlotPoint, Trap>(pp, trap);
                            }
                        }
                    }
                }
                    break;
                case OverviewType.SkillChallenges:
                {
                    foreach (var pp in _fPoints)
                    {
                        if (pp.Element == null)
                            continue;

                        if (pp.Element is SkillChallenge)
                        {
                            var sc = pp.Element as SkillChallenge;

                            var lvi = ItemList.Items.Add(pp.Name);
                            lvi.SubItems.Add(sc.GetXp() + " XP");
                            lvi.Tag = new Pair<PlotPoint, SkillChallenge>(pp, sc);
                        }

                        if (pp.Element is Encounter)
                        {
                            var enc = pp.Element as Encounter;

                            foreach (var sc in enc.SkillChallenges)
                            {
                                var lvi = ItemList.Items.Add(pp.Name);
                                lvi.SubItems.Add(sc.GetXp() + " XP");
                                lvi.Tag = new Pair<PlotPoint, SkillChallenge>(pp, sc);
                            }
                        }
                    }
                }
                    break;
                case OverviewType.Treasure:
                {
                    foreach (var pp in _fPoints)
                    foreach (var parcel in pp.Parcels)
                    {
                        var name = parcel.Name != "" ? parcel.Name : "(undefined parcel)";

                        var lvi = ItemList.Items.Add(pp.Name);
                        lvi.SubItems.Add(name + ": " + parcel.Details);
                        lvi.Tag = new Pair<PlotPoint, Parcel>(pp, parcel);
                    }
                }
                    break;
            }

            if (ItemList.Items.Count == 0)
            {
                var lvi = ItemList.Items.Add("(no items)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            ItemList.Sort();
        }

        private void add_points(Plot plot)
        {
            var points = plot != null ? plot.Points : Session.Project.Plot.Points;

            _fPoints.AddRange(points);

            foreach (var pp in points)
                add_points(pp.Subplot);
        }

        private void ItemList_DoubleClick(object sender, EventArgs e)
        {
            switch (_fType)
            {
                case OverviewType.Encounters:
                {
                    if (SelectedEncounter != null)
                    {
                        var level = Workspace.GetPartyLevel(SelectedEncounter.First);

                        var dlg = new EncounterBuilderForm(SelectedEncounter.Second, level, false);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            SelectedEncounter.First.Element = dlg.Encounter;
                            Session.Modified = true;

                            update_list();
                        }
                    }
                }
                    break;
                case OverviewType.Traps:
                {
                    if (SelectedTrap != null)
                    {
                        if (SelectedTrap.First.Element is TrapElement)
                        {
                            var te = SelectedTrap.First.Element as TrapElement;

                            var dlg = new TrapBuilderForm(SelectedTrap.Second);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                te.Trap = dlg.Trap;
                                Session.Modified = true;

                                update_list();
                            }

                            return;
                        }

                        if (SelectedTrap.First.Element is Encounter)
                        {
                            var enc = SelectedTrap.First.Element as Encounter;
                            var index = enc.Traps.IndexOf(SelectedTrap.Second);

                            var dlg = new TrapBuilderForm(SelectedTrap.Second);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                enc.Traps[index] = dlg.Trap;
                                Session.Modified = true;

                                update_list();
                            }
                        }
                    }
                }
                    break;
                case OverviewType.SkillChallenges:
                {
                    if (SelectedChallenge != null)
                    {
                        if (SelectedChallenge.First.Element is SkillChallenge)
                        {
                            var sc = SelectedChallenge.First.Element as SkillChallenge;

                            var dlg = new SkillChallengeBuilderForm(SelectedChallenge.Second);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                sc.Name = dlg.SkillChallenge.Name;
                                sc.Level = dlg.SkillChallenge.Level;
                                sc.Complexity = dlg.SkillChallenge.Complexity;
                                sc.Success = dlg.SkillChallenge.Success;
                                sc.Failure = dlg.SkillChallenge.Failure;

                                sc.Skills.Clear();
                                foreach (var scd in dlg.SkillChallenge.Skills)
                                    sc.Skills.Add(scd.Copy());

                                Session.Modified = true;

                                update_list();
                            }

                            return;
                        }

                        if (SelectedChallenge.First.Element is Encounter)
                        {
                            var enc = SelectedChallenge.First.Element as Encounter;
                            var index = enc.SkillChallenges.IndexOf(SelectedChallenge.Second);

                            var dlg = new SkillChallengeBuilderForm(SelectedChallenge.Second);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                enc.SkillChallenges[index] = dlg.SkillChallenge;
                                Session.Modified = true;

                                update_list();
                            }
                        }
                    }
                }
                    break;
                case OverviewType.Treasure:
                {
                    if (SelectedParcel != null)
                    {
                        var index = SelectedParcel.First.Parcels.IndexOf(SelectedParcel.Second);

                        var dlg = new ParcelForm(SelectedParcel.Second);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            SelectedParcel.First.Parcels[index] = dlg.Parcel;
                            Session.Modified = true;

                            update_list();
                        }
                    }
                }
                    break;
            }
        }
    }
}
