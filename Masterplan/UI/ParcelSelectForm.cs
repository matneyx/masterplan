using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools.Generators;

namespace Masterplan.UI
{
    internal partial class ParcelSelectForm : Form
    {
        public Parcel Parcel
        {
            get
            {
                if (ParcelList.SelectedItems.Count != 0)
                    return ParcelList.SelectedItems[0].Tag as Parcel;

                return null;
            }
        }

        public ParcelSelectForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            update_list();
        }

        ~ParcelSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RandomiseBtn.Enabled = Parcel != null;

            ChangeItemBtn.Enabled = Parcel != null && Parcel.MagicItemId != Guid.Empty;
            StatBlockBtn.Enabled = Parcel != null && Parcel.MagicItemId != Guid.Empty &&
                                   !Treasure.PlaceholderIDs.Contains(Parcel.MagicItemId);

            OKBtn.Enabled = Parcel != null;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (Parcel != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void ChangeItemBtn_Click(object sender, EventArgs e)
        {
            if (Parcel != null && Parcel.MagicItemId != Guid.Empty)
            {
                var level = 0;
                var item = Session.FindMagicItem(Parcel.MagicItemId, SearchType.Global);
                if (item != null)
                {
                    level = item.Level;
                }
                else
                {
                    var index = Treasure.PlaceholderIDs.IndexOf(Parcel.MagicItemId);
                    if (index != -1)
                        level = index + 1;
                }

                if (level > 0)
                {
                    var dlg = new MagicItemSelectForm(level);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Parcel.SetAsMagicItem(dlg.MagicItem);
                        Session.Modified = true;

                        update_list();
                    }
                }
            }
        }

        private void StatBlockBtn_Click(object sender, EventArgs e)
        {
            if (Parcel != null && Parcel.MagicItemId != Guid.Empty)
            {
                var item = Session.FindMagicItem(Parcel.MagicItemId, SearchType.Global);
                if (item != null)
                {
                    var dlg = new MagicItemDetailsForm(item);
                    dlg.ShowDialog();
                }
            }
        }

        private void RandomiseBtn_Click(object sender, EventArgs e)
        {
            if (Parcel != null)
            {
                Randomise(Parcel);
                update_list();
            }
        }

        private void RandomiseAllBtn_Click(object sender, EventArgs e)
        {
            foreach (var parcel in Session.Project.TreasureParcels)
                Randomise(parcel);

            update_list();
        }

        private void Randomise(Parcel parcel)
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
            else
            {
                parcel.Details = Treasure.RandomMundaneItem(parcel.Value);
            }
        }

        private void update_list()
        {
            ParcelList.Items.Clear();

            var parcels = Session.Project.TreasureParcels;
            foreach (var parcel in parcels)
            {
                var name = parcel.Name != "" ? parcel.Name : "(undefined parcel)";
                var lvi = ParcelList.Items.Add(name);
                lvi.SubItems.Add(parcel.Details);
                lvi.Tag = parcel;

                var groupIndex = parcel.MagicItemId != Guid.Empty ? 0 : 1;
                lvi.Group = ParcelList.Groups[groupIndex];
            }

            ParcelList.Sort();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }

        private void ParcelSelectForm_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
    }
}
