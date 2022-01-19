using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class BackgroundForm : Form
    {
        public Background Background { get; }

        public BackgroundForm(Background bg)
        {
            InitializeComponent();

            Background = bg.Copy();

            TitleBox.Text = Background.Title;
            DetailsBox.Text = Background.Details;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Background.Title = TitleBox.Text;
            Background.Details = DetailsBox.Text != DetailsBox.DefaultText ? DetailsBox.Text : "";
        }
    }
}
