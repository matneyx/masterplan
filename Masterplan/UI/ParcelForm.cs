using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.Tools.Generators;

namespace Masterplan.UI
{
    internal partial class ParcelForm : Form
    {
        public Parcel Parcel { get; private set; }

        public ParcelForm(Parcel p)
        {
            InitializeComponent();

            Parcel = p.Copy();

            set_controls();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            if (Parcel.MagicItemId == Guid.Empty && Parcel.ArtifactId == Guid.Empty)
            {
                Parcel.Name = NameBox.Text;
                Parcel.Details = DetailsBox.Text;
            }
        }

        private void ChangeToMundaneParcel_Click(object sender, EventArgs e)
        {
            Parcel.MagicItemId = Guid.Empty;
            Parcel.ArtifactId = Guid.Empty;

            Parcel.Name = "";
            Parcel.Details = "";

            set_controls();
        }

        private void ChangeToMagicItem_Click(object sender, EventArgs e)
        {
            // Browse for another item
            var dlg = new MagicItemSelectForm(Parcel.FindItemLevel());
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Parcel.SetAsMagicItem(dlg.MagicItem);

                NameBox.Text = Parcel.Name;
                DetailsBox.Text = Parcel.Details;

                set_controls();
            }
        }

        private void ChangeToArtifact_Click(object sender, EventArgs e)
        {
            // Browse for another artifact
            var dlg = new ArtifactSelectForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Parcel.SetAsArtifact(dlg.Artifact);

                NameBox.Text = Parcel.Name;
                DetailsBox.Text = Parcel.Details;

                set_controls();
            }
        }

        private void SelectBtn_Click(object sender, EventArgs e)
        {
            if (Parcel.MagicItemId != Guid.Empty)
            {
                ChangeToMagicItem_Click(this, e);
            }
            else if (Parcel.ArtifactId != Guid.Empty)
            {
                ChangeToArtifact_Click(this, e);
            }
        }

        private void RandomiseBtn_Click(object sender, EventArgs e)
        {
            if (Parcel.MagicItemId != Guid.Empty)
            {
                // Select a random item
                var item = Treasure.RandomMagicItem(Parcel.FindItemLevel());
                if (item != null)
                    Parcel.SetAsMagicItem(item);

                set_controls();
            }
            else if (Parcel.ArtifactId != Guid.Empty)
            {
                // Select a random artifact
                var item = Treasure.RandomArtifact(Parcel.FindItemTier());
                if (item != null)
                    Parcel.SetAsArtifact(item);

                set_controls();
            }
            else
            {
                var value = Parcel.Value;
                if (value == 0)
                    value = Treasure.GetItemValue(Session.Project.Party.Level);

                // Create random parcel of this value
                Parcel = Treasure.CreateParcel(value, false);

                NameBox.Text = Parcel.Name;
                DetailsBox.Text = Parcel.Details;

                set_controls();
            }
        }

        private void set_controls()
        {
            var magic = Parcel.MagicItemId != Guid.Empty;
            var artifact = Parcel.ArtifactId != Guid.Empty;
            var mundane = !magic && !artifact;

            ChangeToMundaneParcel.Enabled = !mundane;
            ChangeToMagicItem.Enabled = !magic && Session.MagicItems.Count != 0;
            ChangeToArtifact.Enabled = !artifact && Session.Artifacts.Count != 0;

            Browser.Visible = !mundane;
            DetailsPanel.Visible = mundane;

            SelectBtn.Enabled = magic || artifact;

            if (mundane)
            {
                NameBox.Text = Parcel.Name;
                DetailsBox.Text = Parcel.Details;
            }
            else
            {
                var item = Session.FindMagicItem(Parcel.MagicItemId, SearchType.Global);
                if (item != null)
                {
                    var html = Html.MagicItem(item, Session.Preferences.TextSize, false, true);
                    Browser.DocumentText = html;
                }

                var a = Session.FindArtifact(Parcel.ArtifactId, SearchType.Global);
                if (a != null)
                {
                    var html = Html.Artifact(a, Session.Preferences.TextSize, false, true);
                    Browser.DocumentText = html;
                }
            }
        }
    }
}
