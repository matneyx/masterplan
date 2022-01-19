using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class EncyclopediaEntryForm : Form
    {
        public EncyclopediaEntry Entry { get; }

        public EncyclopediaImage SelectedImage
        {
            get
            {
                if (PictureList.SelectedItems.Count != 0)
                    return PictureList.SelectedItems[0].Tag as EncyclopediaImage;

                return null;
            }
        }

        public EncyclopediaEntryForm(EncyclopediaEntry entry)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            var bst = new BinarySearchTree<string>();
            bst.Add("People");
            bst.Add("Places");
            bst.Add("Things");
            bst.Add("History");
            bst.Add("Culture");
            bst.Add("Geography");
            bst.Add("Organisations");

            foreach (var ee in Session.Project.Encyclopedia.Entries)
                if (ee.Category != null && ee.Category != "")
                    bst.Add(ee.Category);
            CatBox.Items.AddRange(bst.SortedList.ToArray());

            Entry = entry.Copy();

            TitleBox.Text = Entry.Name;
            CatBox.Text = Entry.Category;
            DetailsBox.Text = Entry.Details;
            DMBox.Text = Entry.DmInfo;

            foreach (var ee in Session.Project.Encyclopedia.Entries)
            {
                if (ee.Id == Entry.Id)
                    continue;

                var lvi = EntryList.Items.Add(ee.Name);
                lvi.Tag = ee;
                lvi.Checked = Session.Project.Encyclopedia.FindLink(Entry.Id, ee.Id) != null;
            }

            if (EntryList.Items.Count == 0)
            {
                var lvi = EntryList.Items.Add("(no entries)");
                lvi.ForeColor = SystemColors.GrayText;

                EntryList.CheckBoxes = false;
            }

            update_pictures();
        }

        ~EncyclopediaEntryForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = SelectedImage != null;
            EditBtn.Enabled = SelectedImage != null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Entry.Name = TitleBox.Text;
            Entry.Category = CatBox.Text;
            Entry.Details = DetailsBox.Text != DetailsBox.DefaultText ? DetailsBox.Text : "";
            Entry.DmInfo = DMBox.Text != DMBox.DefaultText ? DMBox.Text : "";

            // Remove all links containing this entry
            var obsolete = new List<EncyclopediaLink>();
            foreach (var link in Session.Project.Encyclopedia.Links)
                if (link.EntryIDs.Contains(Entry.Id))
                    obsolete.Add(link);
            foreach (var link in obsolete)
                Session.Project.Encyclopedia.Links.Remove(link);

            // Add the required links
            foreach (ListViewItem lvi in EntryList.CheckedItems)
            {
                var ee = lvi.Tag as EncyclopediaEntry;

                var link = new EncyclopediaLink();
                link.EntryIDs.Add(Entry.Id);
                link.EntryIDs.Add(ee.Id);

                Session.Project.Encyclopedia.Links.Add(link);
            }
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var img = new EncyclopediaImage();
            img.Name = "(name)";

            var dlg = new EncyclopediaImageForm(img);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Entry.Images.Add(dlg.Image);
                update_pictures();
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedImage != null)
            {
                Entry.Images.Remove(SelectedImage);
                update_pictures();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedImage != null)
            {
                var index = Entry.Images.IndexOf(SelectedImage);

                var dlg = new EncyclopediaImageForm(SelectedImage);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Entry.Images[index] = dlg.Image;
                    update_pictures();
                }
            }
        }

        private void update_pictures()
        {
            PictureList.Items.Clear();
            PictureList.LargeImageList = null;

            const int pictureSize = 64;

            var images = new ImageList();
            images.ImageSize = new Size(pictureSize, pictureSize);
            images.ColorDepth = ColorDepth.Depth32Bit;
            PictureList.LargeImageList = images;

            foreach (var img in Entry.Images)
            {
                var lvi = PictureList.Items.Add(img.Name);
                lvi.Tag = img;

                Image bmp = new Bitmap(pictureSize, pictureSize);
                var g = Graphics.FromImage(bmp);
                if (img.Image.Size.Width > img.Image.Size.Height)
                {
                    var height = img.Image.Size.Height * pictureSize / img.Image.Size.Width;
                    var rect = new Rectangle(0, (pictureSize - height) / 2, pictureSize, height);

                    g.DrawImage(img.Image, rect);
                }
                else
                {
                    var width = img.Image.Size.Width * pictureSize / img.Image.Size.Height;
                    var rect = new Rectangle((pictureSize - width) / 2, 0, width, pictureSize);

                    g.DrawImage(img.Image, rect);
                }

                images.Images.Add(bmp);
                lvi.ImageIndex = images.Images.Count - 1;
            }

            if (PictureList.Items.Count == 0)
            {
                var lvi = PictureList.Items.Add("(no images)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
