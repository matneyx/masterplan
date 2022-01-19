using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class ProjectForm : Form
    {
        public Project Project { get; }

        public ProjectForm(Project p)
        {
            InitializeComponent();

            Project = p;

            NameBox.Text = Project.Name;
            AuthorBox.Text = Project.Author;

            SizeBox.Value = Project.Party.Size;
            LevelBox.Value = Project.Party.Level;
            LevelBox_ValueChanged(null, null);

            XPBox.Value = Project.Party.Xp;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Project.Name = NameBox.Text;
            Project.Author = AuthorBox.Text;

            Project.Party.Size = (int)SizeBox.Value;
            Project.Party.Level = (int)LevelBox.Value;
            Project.Party.Xp = (int)XPBox.Value;

            Project.Library.Name = Project.Name;
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void SizeBox_ValueChanged(object sender, EventArgs e)
        {
        }

        private void LevelBox_ValueChanged(object sender, EventArgs e)
        {
            var level = (int)LevelBox.Value;

            XPBox.Minimum = Experience.GetHeroXp(level);
            XPBox.Maximum = Math.Max(Experience.GetHeroXp(level + 1) - 1, XPBox.Minimum);

            XPBox.Value = XPBox.Minimum;
        }

        private void XPBox_ValueChanged(object sender, EventArgs e)
        {
        }
    }
}
