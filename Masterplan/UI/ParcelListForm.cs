using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools.Generators;

namespace Masterplan.UI
{
    internal partial class ParcelListForm : Form
    {
        private bool _fViewAssigned;
        private bool _fViewUnassigned = true;

        public Parcel SelectedParcel
        {
            get
            {
                if (ParcelList.SelectedItems.Count != 0)
                    return ParcelList.SelectedItems[0].Tag as Parcel;

                return null;
            }
        }

        public ParcelListForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            update_list();
        }

        ~ParcelListForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            AddMagicItem.Enabled = Session.MagicItems.Count != 0;
            AddArtifact.Enabled = Session.Artifacts.Count != 0;

            RemoveBtn.Enabled = SelectedParcel != null;
            EditBtn.Enabled = SelectedParcel != null;

            RandomiseAllBtn.Enabled = Session.Project.TreasureParcels.Count != 0 && _fViewUnassigned;
        }

        private void AddParcel_Click(object sender, EventArgs e)
        {
            var p = new Parcel();
            p.Name = "New Treasure Parcel";

            var dlg = new ParcelForm(p);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.TreasureParcels.Add(dlg.Parcel);
                Session.Modified = true;

                update_list();
            }
        }

        private void AddMagicItem_Click(object sender, EventArgs e)
        {
            var dlg = new MagicItemSelectForm(Session.Project.Party.Level);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var parcel = new Parcel(dlg.MagicItem);

                Session.Project.TreasureParcels.Add(parcel);
                Session.Modified = true;

                update_list();
            }
        }

        private void AddArtifact_Click(object sender, EventArgs e)
        {
            var dlg = new ArtifactSelectForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var parcel = new Parcel(dlg.Artifact);

                Session.Project.TreasureParcels.Add(parcel);
                Session.Modified = true;

                update_list();
            }
        }

        private void AddSet_Click(object sender, EventArgs e)
        {
            var dlg = new LevelForm(Session.Project.Party.Level);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var list = Treasure.CreateParcelSet(dlg.Level, Session.Project.Party.Size, true);

                Session.Project.TreasureParcels.AddRange(list);
                Session.Modified = true;

                update_list();
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedParcel != null)
            {
                var list = get_list_containing(SelectedParcel);

                list.Remove(SelectedParcel);
                Session.Modified = true;

                update_list();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedParcel != null)
            {
                var list = get_list_containing(SelectedParcel);

                var index = list.IndexOf(SelectedParcel);

                var dlg = new ParcelForm(SelectedParcel);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    list[index] = dlg.Parcel;
                    Session.Modified = true;

                    update_list();
                }
            }
        }

        private void update_list()
        {
            ParcelList.BeginUpdate();
            ParcelList.Items.Clear();

            if (_fViewAssigned)
            {
                var allPoints = Session.Project.AllPlotPoints;
                foreach (var pp in allPoints) add_list(pp.Parcels, 0);
            }

            if (_fViewUnassigned) add_list(Session.Project.TreasureParcels, 1);

            if (_fViewAssigned && ParcelList.Groups[0].Items.Count == 0)
            {
                var lvi = ParcelList.Items.Add("(no parcels)");
                lvi.ForeColor = SystemColors.GrayText;
                lvi.Group = ParcelList.Groups[0];
            }

            if (_fViewUnassigned && ParcelList.Groups[1].Items.Count == 0)
            {
                var lvi = ParcelList.Items.Add("(no parcels)");
                lvi.ForeColor = SystemColors.GrayText;
                lvi.Group = ParcelList.Groups[1];
            }

            ParcelList.Sort();
            ParcelList.EndUpdate();
        }

        private List<Parcel> get_list_containing(Parcel p)
        {
            if (Session.Project.TreasureParcels.Contains(p))
            {
                return Session.Project.TreasureParcels;
            }

            var allPoints = Session.Project.AllPlotPoints;
            foreach (var pp in allPoints)
                if (pp.Parcels.Contains(p))
                    return pp.Parcels;

            return null;
        }

        private void add_list(List<Parcel> list, int groupIndex)
        {
            foreach (var p in list)
            {
                var name = p.Name != "" ? p.Name : "(undefined parcel)";

                var lvi = ParcelList.Items.Add(name);
                lvi.SubItems.Add(p.Details);
                lvi.Tag = p;
                lvi.Group = ParcelList.Groups[groupIndex];

                Hero hero = null;
                if (p.HeroId != Guid.Empty)
                    hero = Session.Project.FindHero(p.HeroId);

                var lvsi = lvi.SubItems.Add(hero != null ? hero.Name : "");
                if (hero == null)
                {
                    lvsi.ForeColor = SystemColors.GrayText;
                    lvi.UseItemStyleForSubItems = false;
                }
            }
        }

        private void ViewMenu_DropDownOpening(object sender, EventArgs e)
        {
            ViewAssigned.Checked = _fViewAssigned;
            ViewUnassigned.Checked = _fViewUnassigned;
        }

        private void ViewAssigned_Click(object sender, EventArgs e)
        {
            _fViewAssigned = !_fViewAssigned;
            update_list();
        }

        private void ViewUnassigned_Click(object sender, EventArgs e)
        {
            _fViewUnassigned = !_fViewUnassigned;
            update_list();
        }

        private void ChangeItemBtn_Click(object sender, EventArgs e)
        {
            if (SelectedParcel != null)
            {
                if (SelectedParcel.MagicItemId != Guid.Empty)
                {
                    var level = SelectedParcel.FindItemLevel();
                    if (level != -1)
                    {
                        var dlg = new MagicItemSelectForm(level);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            SelectedParcel.SetAsMagicItem(dlg.MagicItem);
                            Session.Modified = true;

                            update_list();
                        }
                    }
                }

                if (SelectedParcel.ArtifactId != Guid.Empty)
                {
                    var dlg = new ArtifactSelectForm();
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        SelectedParcel.SetAsArtifact(dlg.Artifact);
                        Session.Modified = true;

                        update_list();
                    }
                }
            }
        }

        private void assign_to_hero(object sender, EventArgs e)
        {
            if (SelectedParcel == null)
                return;

            var tsi = sender as ToolStripItem;
            if (tsi == null)
                return;

            var hero = tsi.Tag as Hero;
            SelectedParcel.HeroId = hero?.Id ?? Guid.Empty;

            update_list();

            Session.Modified = true;
        }

        private void RandomiseBtn_Click(object sender, EventArgs e)
        {
            foreach (var parcel in Session.Project.TreasureParcels)
                randomise_parcel(parcel);

            update_list();

            Session.Modified = true;
        }

        private void RandomiseItem_Click(object sender, EventArgs e)
        {
            if (SelectedParcel != null)
            {
                randomise_parcel(SelectedParcel);
                update_list();
            }
        }

        private void ResetItem_Click(object sender, EventArgs e)
        {
            if (SelectedParcel != null)
            {
                reset_parcel(SelectedParcel);
                update_list();
            }
        }

        private void randomise_parcel(Parcel parcel)
        {
            if (parcel.MagicItemId != Guid.Empty)
            {
                var level = parcel.FindItemLevel();
                if (level != -1)
                {
                    var newItem = Treasure.RandomMagicItem(level);
                    if (newItem != null)
                        parcel.SetAsMagicItem(newItem);
                }
            }
            else if (parcel.ArtifactId != Guid.Empty)
            {
                var tier = parcel.FindItemTier();
                var newItem = Treasure.RandomArtifact(tier);
                if (newItem != null)
                    parcel.SetAsArtifact(newItem);
            }
            else
            {
                if (parcel.Value != 0)
                    parcel.Details = Treasure.RandomMundaneItem(parcel.Value);
            }
        }

        private void reset_parcel(Parcel parcel)
        {
            if (parcel.MagicItemId != Guid.Empty)
            {
                var level = parcel.FindItemLevel();
                if (level != -1)
                {
                    parcel.MagicItemId = Treasure.PlaceholderIDs[level - 1];
                    parcel.Name = "Magic item (level " + level + ")";
                }
                else
                {
                    parcel.Name = "Magic item";
                }
            }
            else if (parcel.ArtifactId != Guid.Empty)
            {
                var tier = parcel.FindItemTier();
                parcel.ArtifactId = Treasure.PlaceholderIDs[(int)tier];
                parcel.Name = "Artifact ( " + tier.ToString().ToLower() + " tier)";
            }
            else
            {
                parcel.Name = "Items worth " + parcel.Value + " GP";
            }

            parcel.Details = "";
        }

        private void ContextMenu_Opening(object sender, CancelEventArgs e)
        {
            ChangeItem.Enabled = SelectedParcel != null && SelectedParcel.MagicItemId != Guid.Empty;
            ChangeAssign.Enabled = SelectedParcel != null;
            RandomiseItem.Enabled = SelectedParcel != null;
            ResetItem.Enabled = SelectedParcel != null;

            ChangeAssign.DropDownItems.Clear();
            foreach (var hero in Session.Project.Heroes)
            {
                var tsmi = new ToolStripMenuItem(hero.Name);
                tsmi.Tag = hero;
                tsmi.Click += assign_to_hero;

                if (SelectedParcel != null)
                    tsmi.Checked = SelectedParcel.HeroId == hero.Id;

                ChangeAssign.DropDownItems.Add(tsmi);
            }

            if (Session.Project.Heroes.Count != 0)
                ChangeAssign.DropDownItems.Add(new ToolStripSeparator());

            var tsmiNone = new ToolStripMenuItem("Not Allocated");
            tsmiNone.Tag = null;
            tsmiNone.Click += assign_to_hero;

            if (SelectedParcel != null)
                tsmiNone.Checked = SelectedParcel.HeroId == Guid.Empty;

            ChangeAssign.DropDownItems.Add(tsmiNone);
        }

        private void ParcelListForm_Shown(object sender, EventArgs e)
        {
            // XP bug
            ParcelList.Invalidate();
        }
    }
}
