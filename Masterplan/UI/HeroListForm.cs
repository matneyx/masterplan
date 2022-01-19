using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.Tools.IO;

namespace Masterplan.UI
{
    internal partial class HeroListForm : Form
    {
        public Hero SelectedHero
        {
            get
            {
                if (HeroList.SelectedItems.Count != 0)
                    return HeroList.SelectedItems[0].Tag as Hero;

                return null;
            }
        }

        public Parcel SelectedParcel
        {
            get
            {
                if (ParcelList.SelectedItems.Count != 0)
                    return ParcelList.SelectedItems[0].Tag as Parcel;

                return null;
            }
        }

        public HeroListForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;
            BreakdownPnl.Heroes = Session.Project.Heroes;

            update_view();
        }

        ~HeroListForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = SelectedHero != null;
            EditBtn.Enabled = SelectedHero != null;

            ActiveBtn.Enabled = SelectedHero != null;
            ActiveBtn.Checked = SelectedHero != null && Session.Project.Heroes.Contains(SelectedHero);

            StatBlockBtn.Enabled = SelectedHero != null;
            EntryBtn.Enabled = SelectedHero != null;
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var hero = new Hero();
            hero.Name = "New Character";

            var dlg = new HeroForm(hero);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                add_hero(dlg.Hero);
                update_view();
            }
        }

        private void Import_CB_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Character File|*.dnd4e";
            dlg.Multiselect = true;

            if (dlg.ShowDialog() == DialogResult.OK)
                foreach (var filename in dlg.FileNames)
                {
                    var xml = File.ReadAllText(filename);
                    var hero = AppImport.ImportHero(xml);

                    if (hero != null)
                    {
                        add_hero(hero);
                        update_view();
                    }
                    else
                    {
                        MessageBox.Show("The character file could not be loaded.", "Masterplan", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
        }

        private void RandomPC_Click(object sender, EventArgs e)
        {
            var hd = HeroData.Create();
            var h = hd.ConvertToHero();

            Session.Project.Heroes.Add(h);
            Session.Modified = true;

            update_view();
        }

        private void RandomParty_Click(object sender, EventArgs e)
        {
            if (Session.Project.Heroes.Count != 0)
            {
                var msg = "This will clear the PC list.";
                msg += Environment.NewLine;
                msg += "Are you sure you want to do this?";

                var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No)
                    return;

                Session.Project.Heroes.Clear();
            }

            var group = HeroGroup.CreateGroup(Session.Project.Party.Size);
            foreach (var hd in group.Heroes)
            {
                if (hd == null)
                    continue;

                var h = hd.ConvertToHero();
                Session.Project.Heroes.Add(h);
            }

            Session.Modified = true;

            update_view();
        }

        private void AddSuggest_Click(object sender, EventArgs e)
        {
            var group = new HeroGroup();

            // Set up the group
            foreach (var hero in Session.Project.Heroes)
            {
                var rd = Sourcebook.GetRace(hero.Race);
                var cd = Sourcebook.GetClass(hero.Class);

                group.Heroes.Add(new HeroData(rd, cd));
            }

            // Ask for another
            var hd = group.Suggest();
            if (hd != null)
            {
                var h = hd.ConvertToHero();
                Session.Project.Heroes.Add(h);
            }

            update_view();
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedHero != null)
            {
                var msg = "Are you sure you want to delete this PC?";
                var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No)
                    return;

                var list = Session.Project.Heroes.Contains(SelectedHero)
                    ? Session.Project.Heroes
                    : Session.Project.InactiveHeroes;
                list.Remove(SelectedHero);

                foreach (var parcel in Session.Project.AllTreasureParcels)
                    if (parcel.HeroId == SelectedHero.Id)
                        parcel.HeroId = Guid.Empty;

                Session.Modified = true;

                update_view();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedHero != null) edit_hero();
        }

        private void edit_hero()
        {
            var list = Session.Project.Heroes.Contains(SelectedHero)
                ? Session.Project.Heroes
                : Session.Project.InactiveHeroes;
            var index = list.IndexOf(SelectedHero);

            var dlg = new HeroForm(SelectedHero);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                list[index] = dlg.Hero;
                Session.Modified = true;

                update_view();
            }
        }

        private void ActiveBtn_Click(object sender, EventArgs e)
        {
            var hero = SelectedHero;
            if (hero == null)
                return;

            if (Session.Project.Heroes.Contains(hero))
            {
                Session.Project.Heroes.Remove(hero);
                Session.Project.InactiveHeroes.Add(hero);
            }
            else
            {
                Session.Project.InactiveHeroes.Remove(hero);
                Session.Project.Heroes.Add(hero);
            }

            Session.Modified = true;

            update_view();
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowPCs();
        }

        private void StatBlockBtn_Click(object sender, EventArgs e)
        {
            if (SelectedHero != null)
            {
                var dlg = new HeroDetailsForm(SelectedHero);
                dlg.ShowDialog();
            }
        }

        private void EntryBtn_Click(object sender, EventArgs e)
        {
            if (SelectedHero == null)
                return;

            var entry = Session.Project.Encyclopedia.FindEntryForAttachment(SelectedHero.Id);

            if (entry == null)
            {
                // If there is no entry, ask to create it
                var msg = "There is no encyclopedia entry associated with this PC.";
                msg += Environment.NewLine;
                msg += "Would you like to create one now?";
                if (MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                    DialogResult.No)
                    return;

                entry = new EncyclopediaEntry();
                entry.Name = SelectedHero.Name;
                entry.AttachmentId = SelectedHero.Id;
                entry.Category = "People";

                Session.Project.Encyclopedia.Entries.Add(entry);
                Session.Modified = true;
            }

            // Edit the entry
            var index = Session.Project.Encyclopedia.Entries.IndexOf(entry);
            var dlg = new EncyclopediaEntryForm(entry);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.Encyclopedia.Entries[index] = dlg.Entry;
                Session.Modified = true;
            }
        }

        private void ParcelList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedParcel != null)
            {
                var dlg = new ParcelForm(SelectedParcel);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // Save changes
                    SelectedParcel.Name = dlg.Parcel.Name;
                    SelectedParcel.Details = dlg.Parcel.Details;
                    SelectedParcel.HeroId = dlg.Parcel.HeroId;
                    SelectedParcel.MagicItemId = dlg.Parcel.MagicItemId;
                    SelectedParcel.ArtifactId = dlg.Parcel.ArtifactId;
                    SelectedParcel.Value = dlg.Parcel.Value;

                    update_parcels();

                    Session.Modified = true;
                }
            }
        }

        private void update_view()
        {
            update_heroes();
            update_parcels();
        }

        private void update_heroes()
        {
            HeroList.Items.Clear();

            foreach (var hero in Session.Project.Heroes)
                add_to_list(hero, true);

            foreach (var hero in Session.Project.InactiveHeroes)
                add_to_list(hero, false);

            if (Session.Project.Heroes.Count == 0)
            {
                var lvi = HeroList.Items.Add("(no heroes)");
                lvi.ForeColor = SystemColors.GrayText;
                lvi.Group = HeroList.Groups[0];
            }

            StatusBar.Visible = Session.Project.Heroes.Count > Session.Project.Party.Size;
            PartySizeLbl.Text = "Your project is set up for a party size of " + Session.Project.Party.Size +
                                "; click here to change it";
        }

        private void update_parcels()
        {
            ParcelList.Groups.Clear();
            ParcelList.Items.Clear();

            ParcelList.ShowGroups = true;
            foreach (var hero in Session.Project.Heroes)
                ParcelList.Groups.Add(hero.Name, hero.Name);

            foreach (var parcel in Session.Project.TreasureParcels)
                add_parcel(parcel);

            foreach (var pp in Session.Project.AllPlotPoints)
            foreach (var parcel in pp.Parcels)
                add_parcel(parcel);

            if (ParcelList.Items.Count == 0)
            {
                ParcelList.ShowGroups = false;
                var lvi = ParcelList.Items.Add("(none assigned)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void add_parcel(Parcel parcel)
        {
            if (parcel.HeroId == Guid.Empty)
                return;

            var hero = Session.Project.FindHero(parcel.HeroId);
            if (hero == null)
                return;

            var lvi = ParcelList.Items.Add(parcel.Name);
            lvi.SubItems.Add(parcel.Details);
            lvi.Tag = parcel;
            lvi.Group = ParcelList.Groups[hero.Name];
        }

        private void add_to_list(Hero hero, bool active)
        {
            var nameStr = hero.Name != "" ? hero.Name : "(unnamed)";
            if (hero.Player != "")
                nameStr += " (" + hero.Player + ")";

            var lvi = HeroList.Items.Add(nameStr);
            lvi.SubItems.Add(hero.Info);
            lvi.SubItems.Add(hero.PassiveInsight.ToString());
            lvi.SubItems.Add(hero.PassivePerception.ToString());
            lvi.Tag = hero;

            lvi.Group = HeroList.Groups[active ? 0 : 1];
        }

        private void add_hero(Hero hero)
        {
            var previous = Session.Project.FindHero(hero.Name);
            var list = Session.Project.InactiveHeroes.Contains(previous)
                ? Session.Project.InactiveHeroes
                : Session.Project.Heroes;

            if (previous != null)
            {
                hero.Id = previous.Id;
                hero.Effects.AddRange(previous.Effects);

                list.Remove(previous);
            }

            list.Add(hero);
            Session.Modified = true;
        }
    }
}
