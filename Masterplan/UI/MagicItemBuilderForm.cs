using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class MagicItemBuilderForm : Form
    {
        public MagicItem MagicItem { get; private set; }

        public MagicItemBuilderForm(MagicItem item)
        {
            InitializeComponent();

            MagicItem = item.Copy();

            update_statblock();
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "build")
            {
                if (e.Url.LocalPath == "profile")
                {
                    e.Cancel = true;

                    var dlg = new MagicItemProfileForm(MagicItem);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        MagicItem.Name = dlg.MagicItem.Name;
                        MagicItem.Level = dlg.MagicItem.Level;
                        MagicItem.Type = dlg.MagicItem.Type;
                        MagicItem.Rarity = dlg.MagicItem.Rarity;

                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "desc")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(MagicItem.Description, "Description", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        MagicItem.Description = dlg.Details;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "section")
            {
                e.Cancel = true;

                if (e.Url.LocalPath == "new")
                {
                    var section = new MagicItemSection();
                    section.Header = "New Section";

                    var dlg = new MagicItemSectionForm(section);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        MagicItem.Sections.Add(dlg.Section);
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "edit")
            {
                e.Cancel = true;

                var index = int.Parse(e.Url.LocalPath);

                var dlg = new MagicItemSectionForm(MagicItem.Sections[index]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    MagicItem.Sections[index] = dlg.Section;
                    update_statblock();
                }
            }

            if (e.Url.Scheme == "remove")
            {
                e.Cancel = true;

                var index = int.Parse(e.Url.LocalPath);

                MagicItem.Sections.RemoveAt(index);
                update_statblock();
            }

            if (e.Url.Scheme == "moveup")
            {
                e.Cancel = true;

                var index = int.Parse(e.Url.LocalPath);

                var tmp = MagicItem.Sections[index - 1];
                MagicItem.Sections[index - 1] = MagicItem.Sections[index];
                MagicItem.Sections[index] = tmp;

                update_statblock();
            }

            if (e.Url.Scheme == "movedown")
            {
                e.Cancel = true;

                var index = int.Parse(e.Url.LocalPath);

                var tmp = MagicItem.Sections[index + 1];
                MagicItem.Sections[index + 1] = MagicItem.Sections[index];
                MagicItem.Sections[index] = tmp;

                update_statblock();
            }
        }

        private void OptionsVariant_Click(object sender, EventArgs e)
        {
            var dlg = new MagicItemSelectForm(MagicItem.Level);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                MagicItem = dlg.MagicItem.Copy();
                MagicItem.Id = Guid.NewGuid();

                update_statblock();
            }
        }

        private void update_statblock()
        {
            StatBlockBrowser.DocumentText = Html.MagicItem(MagicItem, Session.Preferences.TextSize, true, true);
        }

        private void FileExport_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Title = "Export Magic Item";
            dlg.FileName = MagicItem.Name;
            dlg.Filter = Program.MagicItemFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var ok = Serialisation<MagicItem>.Save(dlg.FileName, MagicItem, SerialisationMode.Binary);

                if (!ok)
                {
                    var error = "The magic item could not be exported.";
                    MessageBox.Show(error, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
