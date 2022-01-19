using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class TileForm : Form
    {
        public Tile Tile { get; }

        public TileForm(Tile t)
        {
            InitializeComponent();

            foreach (TileCategory cat in Enum.GetValues(typeof(TileCategory)))
                CatBox.Items.Add(cat);

            Application.Idle += Application_Idle;

            Tile = t.Copy();

            WidthBox.Value = Tile.Size.Width;
            HeightBox.Value = Tile.Size.Height;
            CatBox.SelectedItem = Tile.Category;
            KeywordBox.Text = Tile.Keywords;

            image_changed();
        }

        ~TileForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            PasteBtn.Enabled = Clipboard.ContainsImage();
            ClearBtn.Enabled = Tile.Image != null;
            SetColourBtn.Enabled = Tile.Image == null;
            GridBtn.Checked = TilePanel.ShowGridlines;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            var width = (int)WidthBox.Value;
            var height = (int)HeightBox.Value;

            Tile.Size = new Size(width, height);
            Tile.Category = (TileCategory)CatBox.SelectedItem;
            Tile.Keywords = KeywordBox.Text;
        }

        private void WidthBox_ValueChanged(object sender, EventArgs e)
        {
            image_changed();
        }

        private void HeightBox_ValueChanged(object sender, EventArgs e)
        {
            image_changed();
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.ImageFilter;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Tile.Image = Image.FromFile(dlg.FileName);
                Program.SetResolution(Tile.Image);
                image_changed();
            }
        }

        private void PasteBtn_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Tile.Image = Clipboard.GetImage();
                Program.SetResolution(Tile.Image);
                image_changed();
            }
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            Tile.Image = null;
            image_changed();
        }

        private void SetColourBtn_Click(object sender, EventArgs e)
        {
            var dlg = new ColorDialog();
            dlg.AllowFullOpen = false;
            dlg.Color = Tile.BlankColour;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Tile.BlankColour = dlg.Color;
                image_changed();
            }
        }

        private void GridBtn_Click(object sender, EventArgs e)
        {
            TilePanel.ShowGridlines = !TilePanel.ShowGridlines;
        }

        private void image_changed()
        {
            var width = (int)WidthBox.Value;
            var height = (int)HeightBox.Value;

            TilePanel.TileImage = Tile.Image;
            TilePanel.TileColour = Tile.BlankColour;
            TilePanel.TileSize = new Size(width, height);
        }
    }
}
