using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class EncounterNoteForm : Form
    {
        public EncounterNote Note { get; }

        public EncounterNoteForm(EncounterNote bg)
        {
            InitializeComponent();

            Note = bg.Copy();

            TitleBox.Text = Note.Title;
            DetailsBox.Text = Note.Contents;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Note.Title = TitleBox.Text;
            Note.Contents = DetailsBox.Text != DetailsBox.DefaultText ? DetailsBox.Text : "";
        }
    }
}
