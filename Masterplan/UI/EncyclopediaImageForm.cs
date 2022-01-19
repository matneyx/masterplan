using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class EncyclopediaImageForm : Form
    {
        public EncyclopediaImage Image { get; }

        public EncyclopediaImageForm(EncyclopediaImage img)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Image = img.Copy();

            NameBox.Text = Image.Name;
            PictureBox.Image = Image.Image;
        }

        ~EncyclopediaImageForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            PasteBtn.Enabled = Clipboard.ContainsImage();
            PlayerViewBtn.Enabled = Image.Image != null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Image.Name = NameBox.Text;
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            var openDlg = new OpenFileDialog();
            openDlg.Filter = Program.ImageFilter;
            if (openDlg.ShowDialog() != DialogResult.OK)
                return;

            Image.Image = System.Drawing.Image.FromFile(openDlg.FileName);
            Program.SetResolution(Image.Image);
            PictureBox.Image = Image.Image;
        }

        private void PasteBtn_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Image.Image = Clipboard.GetImage();
                Program.SetResolution(Image.Image);
                PictureBox.Image = Image.Image;
            }
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowImage(Image.Image);
        }
    }
}
