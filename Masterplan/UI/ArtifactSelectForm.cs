using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class ArtifactSelectForm : Form
    {
        public Artifact Artifact
        {
            get
            {
                if (ItemList.SelectedItems.Count != 0)
                    return ItemList.SelectedItems[0].Tag as Artifact;

                return null;
            }
        }

        public ArtifactSelectForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Browser.DocumentText = "";
            ItemList_SelectedIndexChanged(null, null);

            update_list();
        }

        ~ArtifactSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = Artifact != null;
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            update_list();
        }

        private void ItemList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var html = Html.Artifact(Artifact, Session.Preferences.TextSize, false, true);

            Browser.Document.OpenNew(true);
            Browser.Document.Write(html);
        }

        private void ItemList_DoubleClick(object sender, EventArgs e)
        {
            if (Artifact != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void update_list()
        {
            var artifacts = new List<Artifact>();
            foreach (var a in Session.Artifacts)
                if (Match(a, NameBox.Text))
                    artifacts.Add(a);

            var lvgHeroic = ItemList.Groups.Add("Heroic Tier", "Heroic Tier");
            var lvgParagon = ItemList.Groups.Add("Paragon Tier", "Paragon Tier");
            var lvgEpic = ItemList.Groups.Add("Epic Tier", "Epic Tier");

            var listItems = new List<ListViewItem>();
            foreach (var item in artifacts)
            {
                var lvi = new ListViewItem(item.Name);
                lvi.SubItems.Add(item.Tier + " Tier");
                lvi.Tag = item;

                switch (item.Tier)
                {
                    case Tier.Heroic:
                        lvi.Group = lvgHeroic;
                        break;
                    case Tier.Paragon:
                        lvi.Group = lvgParagon;
                        break;
                    case Tier.Epic:
                        lvi.Group = lvgEpic;
                        break;
                }

                listItems.Add(lvi);
            }

            ItemList.BeginUpdate();
            ItemList.Items.Clear();
            ItemList.Items.AddRange(listItems.ToArray());
            ItemList.EndUpdate();
        }

        private bool Match(Artifact item, string query)
        {
            var tokens = query.ToLower().Split();

            foreach (var token in tokens)
                if (!match_token(item, token))
                    return false;

            return true;
        }

        private bool match_token(Artifact item, string token)
        {
            if (item.Name.ToLower().Contains(token))
                return true;

            return false;
        }
    }
}
