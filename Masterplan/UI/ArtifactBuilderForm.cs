using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class ArtifactBuilderForm : Form
    {
        public Artifact Artifact { get; private set; }

        public ArtifactBuilderForm(Artifact artifact)
        {
            InitializeComponent();

            Artifact = artifact.Copy();

            update_statblock();
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "build")
            {
                if (e.Url.LocalPath == "profile")
                {
                    e.Cancel = true;

                    var dlg = new ArtifactProfileForm(Artifact);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Artifact.Name = dlg.Artifact.Name;
                        Artifact.Tier = dlg.Artifact.Tier;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "description")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Artifact.Description, "Description", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Artifact.Description = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "details")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Artifact.Details, "Details", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Artifact.Details = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "goals")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Artifact.Goals, "Goals", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Artifact.Goals = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "rp")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Artifact.RoleplayingTips, "Roleplaying", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Artifact.RoleplayingTips = dlg.Details;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "section")
            {
                if (e.Url.LocalPath == "new")
                {
                    e.Cancel = true;

                    var mis = new MagicItemSection();
                    var dlg = new MagicItemSectionForm(mis);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Artifact.Sections.Add(dlg.Section);
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath.Contains(",new"))
                {
                    e.Cancel = true;

                    try
                    {
                        var str = e.Url.LocalPath.Substring(0, e.Url.LocalPath.IndexOf(","));
                        var n = int.Parse(str);
                        var ac = Artifact.ConcordanceLevels[n];

                        var mis = new MagicItemSection();
                        var dlg = new MagicItemSectionForm(mis);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            ac.Sections.Add(dlg.Section);
                            update_statblock();
                        }
                    }
                    catch
                    {
                        // Not a number
                    }
                }
            }

            if (e.Url.Scheme == "sectionedit")
            {
                if (e.Url.LocalPath.Contains(","))
                {
                    e.Cancel = true;

                    var comma = e.Url.LocalPath.IndexOf(",");
                    var pre = e.Url.LocalPath.Substring(0, comma);
                    var post = e.Url.LocalPath.Substring(comma);

                    try
                    {
                        var acIndex = int.Parse(pre);
                        var sectionIndex = int.Parse(post);

                        var ac = Artifact.ConcordanceLevels[acIndex];
                        var mis = ac.Sections[sectionIndex];
                        var dlg = new MagicItemSectionForm(mis);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            ac.Sections[sectionIndex] = dlg.Section;
                            update_statblock();
                        }
                    }
                    catch
                    {
                        // Not a number
                    }
                }
                else
                {
                    e.Cancel = true;

                    try
                    {
                        var n = int.Parse(e.Url.LocalPath);
                        var mis = Artifact.Sections[n];

                        var dlg = new MagicItemSectionForm(mis);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            Artifact.Sections[n] = dlg.Section;
                            update_statblock();
                        }
                    }
                    catch
                    {
                        // Not a number
                    }
                }
            }

            if (e.Url.Scheme == "sectionremove")
            {
                if (e.Url.LocalPath.Contains(","))
                {
                    e.Cancel = true;

                    var comma = e.Url.LocalPath.IndexOf(",");
                    var pre = e.Url.LocalPath.Substring(0, comma);
                    var post = e.Url.LocalPath.Substring(comma);

                    try
                    {
                        var acIndex = int.Parse(pre);
                        var sectionIndex = int.Parse(post);

                        var ac = Artifact.ConcordanceLevels[acIndex];
                        ac.Sections.RemoveAt(sectionIndex);
                        update_statblock();
                    }
                    catch
                    {
                        // Not a number
                    }
                }
                else
                {
                    e.Cancel = true;

                    try
                    {
                        var n = int.Parse(e.Url.LocalPath);

                        Artifact.Sections.RemoveAt(n);
                        update_statblock();
                    }
                    catch
                    {
                        // Not a number
                    }
                }
            }

            if (e.Url.Scheme == "rule")
            {
                e.Cancel = true;

                if (e.Url.LocalPath == "new")
                {
                    var rule = new Pair<string, string>("", "");
                    var dlg = new ArtifactConcordanceForm(rule);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Artifact.ConcordanceRules.Add(dlg.Concordance);
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "ruleedit")
            {
                e.Cancel = true;

                try
                {
                    var n = int.Parse(e.Url.LocalPath);
                    var rule = Artifact.ConcordanceRules[n];

                    var dlg = new ArtifactConcordanceForm(rule);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Artifact.ConcordanceRules[n] = dlg.Concordance;
                        update_statblock();
                    }
                }
                catch
                {
                    // Not a number
                }
            }

            if (e.Url.Scheme == "ruleremove")
            {
                e.Cancel = true;

                try
                {
                    var n = int.Parse(e.Url.LocalPath);

                    Artifact.ConcordanceRules.RemoveAt(n);
                    update_statblock();
                }
                catch
                {
                    // Not a number
                }
            }

            if (e.Url.Scheme == "quote")
            {
                e.Cancel = true;

                try
                {
                    var n = int.Parse(e.Url.LocalPath);
                    var ac = Artifact.ConcordanceLevels[n];

                    var dlg = new DetailsForm(ac.Quote, "Concordance Quote", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        ac.Quote = dlg.Details;
                        update_statblock();
                    }
                }
                catch
                {
                    // Not a number
                }
            }

            if (e.Url.Scheme == "desc")
            {
                e.Cancel = true;

                try
                {
                    var n = int.Parse(e.Url.LocalPath);
                    var ac = Artifact.ConcordanceLevels[n];

                    var dlg = new DetailsForm(ac.Description, "Concordance Description", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        ac.Description = dlg.Details;
                        update_statblock();
                    }
                }
                catch
                {
                    // Not a number
                }
            }
        }

        private void update_statblock()
        {
            StatBlockBrowser.DocumentText = Html.Artifact(Artifact, Session.Preferences.TextSize, true, true);
        }

        private void FileImport_Click(object sender, EventArgs e)
        {
            var msg = "Importing an artifact file will clear any changes you have made to the item shown.";
            if (MessageBox.Show(msg, "Masterplan", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) ==
                DialogResult.Cancel)
                return;

            var dlg = new OpenFileDialog();
            dlg.Title = "Import Artifact";
            dlg.Filter = Program.ArtifactFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var a = Serialisation<Artifact>.Load(dlg.FileName, SerialisationMode.Binary);
                if (a != null)
                {
                    Artifact = a;
                    update_statblock();
                }
                else
                {
                    var error = "The artifact could not be imported.";
                    MessageBox.Show(error, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FileExport_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Title = "Export Artifact";
            dlg.FileName = Artifact.Name;
            dlg.Filter = Program.ArtifactFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var ok = Serialisation<Artifact>.Save(dlg.FileName, Artifact, SerialisationMode.Binary);

                if (!ok)
                {
                    var error = "The artifact could not be exported.";
                    MessageBox.Show(error, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
