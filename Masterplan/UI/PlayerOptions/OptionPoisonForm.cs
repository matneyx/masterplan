using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionPoisonForm : Form
    {
        public Poison Poison { get; }

        public PlayerPowerSection SelectedSection
        {
            get
            {
                if (SectionList.SelectedItems.Count != 0)
                    return SectionList.SelectedItems[0].Tag as PlayerPowerSection;

                return null;
            }
        }

        public OptionPoisonForm(Poison poison)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Poison = poison.Copy();

            NameBox.Text = Poison.Name;
            LevelBox.Value = Poison.Level;
            DetailsBox.Text = Poison.Details;

            update_sections();
        }

        ~OptionPoisonForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            var index = Poison.Sections.IndexOf(SelectedSection);

            SectionRemoveBtn.Enabled = SelectedSection != null;
            SectionEditBtn.Enabled = SelectedSection != null;

            SectionUpBtn.Enabled = SelectedSection != null && index != 0;
            SectionDownBtn.Enabled = SelectedSection != null && index != Poison.Sections.Count - 1;

            SectionLeftBtn.Enabled = SelectedSection != null && SelectedSection.Indent > 0;
            SectionRightBtn.Enabled = false;
            if (index > 0)
            {
                var prev = Poison.Sections[index - 1];
                SectionRightBtn.Enabled = SelectedSection != null && SelectedSection.Indent <= prev.Indent;
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Poison.Name = NameBox.Text;
            Poison.Level = (int)LevelBox.Value;
            Poison.Details = DetailsBox.Text;
        }

        private void SectionAddBtn_Click(object sender, EventArgs e)
        {
            var section = new PlayerPowerSection();

            var dlg = new OptionPowerSectionForm(section);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Poison.Sections.Add(dlg.Section);
                update_sections();
            }
        }

        private void SectionRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                Poison.Sections.Remove(SelectedSection);
                update_sections();
            }
        }

        private void SectionEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                var index = Poison.Sections.IndexOf(SelectedSection);

                var dlg = new OptionPowerSectionForm(SelectedSection);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Poison.Sections[index] = dlg.Section;
                    update_sections();
                }
            }
        }

        private void SectionUpBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                var index = Poison.Sections.IndexOf(SelectedSection);

                var tmp = Poison.Sections[index - 1];
                Poison.Sections[index - 1] = SelectedSection;
                Poison.Sections[index] = tmp;

                update_sections();

                SectionList.SelectedIndices.Add(index - 1);
            }
        }

        private void SectionDownBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                var index = Poison.Sections.IndexOf(SelectedSection);

                var tmp = Poison.Sections[index + 1];
                Poison.Sections[index + 1] = SelectedSection;
                Poison.Sections[index] = tmp;

                update_sections();

                SectionList.SelectedIndices.Add(index + 1);
            }
        }

        private void SectionLeftBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                var index = Poison.Sections.IndexOf(SelectedSection);

                SelectedSection.Indent -= 1;
                update_sections();

                SectionList.SelectedIndices.Add(index);
            }
        }

        private void SectionRightBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                var index = Poison.Sections.IndexOf(SelectedSection);

                SelectedSection.Indent += 1;
                update_sections();

                SectionList.SelectedIndices.Add(index);
            }
        }

        private void update_sections()
        {
            SectionList.Items.Clear();
            foreach (var section in Poison.Sections)
            {
                var str = "";
                for (var n = 0; n != section.Indent; ++n)
                    str += "    ";
                str += section.Header + ": " + section.Details;

                var lvi = SectionList.Items.Add(str);
                lvi.Tag = section;
            }

            if (Poison.Sections.Count == 0)
            {
                var lvi = SectionList.Items.Add("(none)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
