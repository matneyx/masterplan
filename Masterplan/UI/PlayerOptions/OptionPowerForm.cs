using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionPowerForm : Form
    {
        public PlayerPower Power { get; }

        public PlayerPowerSection SelectedSection
        {
            get
            {
                if (SectionList.SelectedItems.Count != 0)
                    return SectionList.SelectedItems[0].Tag as PlayerPowerSection;

                return null;
            }
        }

        public OptionPowerForm(PlayerPower power)
        {
            InitializeComponent();

            var types = Enum.GetValues(typeof(PlayerPowerType));
            foreach (PlayerPowerType type in types)
                TypeBox.Items.Add(type);

            var actions = Enum.GetValues(typeof(ActionType));
            foreach (ActionType action in actions)
                ActionBox.Items.Add(action);

            RangeBox.Items.Add("Personal");
            RangeBox.Items.Add("Melee touch");
            RangeBox.Items.Add("Melee 1");
            RangeBox.Items.Add("Melee weapon");
            RangeBox.Items.Add("Ranged 10");
            RangeBox.Items.Add("Ranged weapon");
            RangeBox.Items.Add("Ranged sight");
            RangeBox.Items.Add("Close burst 1");
            RangeBox.Items.Add("Close blast 3");
            RangeBox.Items.Add("Area burst 3 within 10");
            RangeBox.Items.Add("Area wall 3 within 10");

            Application.Idle += Application_Idle;

            Power = power.Copy();

            NameBox.Text = Power.Name;
            TypeBox.SelectedItem = Power.Type;
            ActionBox.SelectedItem = Power.Action;
            KeywordBox.Text = Power.Keywords;
            RangeBox.Text = Power.Range;
            ReadAloudBox.Text = Power.ReadAloud;

            update_sections();
        }

        ~OptionPowerForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            var index = Power.Sections.IndexOf(SelectedSection);

            SectionRemoveBtn.Enabled = SelectedSection != null;
            SectionEditBtn.Enabled = SelectedSection != null;

            SectionUpBtn.Enabled = SelectedSection != null && index != 0;
            SectionDownBtn.Enabled = SelectedSection != null && index != Power.Sections.Count - 1;

            SectionLeftBtn.Enabled = SelectedSection != null && SelectedSection.Indent > 0;
            SectionRightBtn.Enabled = false;
            if (index > 0)
            {
                var prev = Power.Sections[index - 1];
                SectionRightBtn.Enabled = SelectedSection != null && SelectedSection.Indent <= prev.Indent;
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Power.Name = NameBox.Text;
            Power.Type = (PlayerPowerType)TypeBox.SelectedItem;
            Power.Action = (ActionType)ActionBox.SelectedItem;
            Power.Keywords = KeywordBox.Text;
            Power.Range = RangeBox.Text;
            Power.ReadAloud = ReadAloudBox.Text;
        }

        private void SectionAddBtn_Click(object sender, EventArgs e)
        {
            var section = new PlayerPowerSection();

            var dlg = new OptionPowerSectionForm(section);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Power.Sections.Add(dlg.Section);
                update_sections();
            }
        }

        private void SectionRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                Power.Sections.Remove(SelectedSection);
                update_sections();
            }
        }

        private void SectionEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                var index = Power.Sections.IndexOf(SelectedSection);

                var dlg = new OptionPowerSectionForm(SelectedSection);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Power.Sections[index] = dlg.Section;
                    update_sections();
                }
            }
        }

        private void SectionUpBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                var index = Power.Sections.IndexOf(SelectedSection);

                var tmp = Power.Sections[index - 1];
                Power.Sections[index - 1] = SelectedSection;
                Power.Sections[index] = tmp;

                update_sections();

                SectionList.SelectedIndices.Add(index - 1);
            }
        }

        private void SectionDownBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                var index = Power.Sections.IndexOf(SelectedSection);

                var tmp = Power.Sections[index + 1];
                Power.Sections[index + 1] = SelectedSection;
                Power.Sections[index] = tmp;

                update_sections();

                SectionList.SelectedIndices.Add(index + 1);
            }
        }

        private void SectionLeftBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                var index = Power.Sections.IndexOf(SelectedSection);

                SelectedSection.Indent -= 1;
                update_sections();

                SectionList.SelectedIndices.Add(index);
            }
        }

        private void SectionRightBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSection != null)
            {
                var index = Power.Sections.IndexOf(SelectedSection);

                SelectedSection.Indent += 1;
                update_sections();

                SectionList.SelectedIndices.Add(index);
            }
        }

        private void update_sections()
        {
            SectionList.Items.Clear();
            foreach (var section in Power.Sections)
            {
                var str = "";
                for (var n = 0; n != section.Indent; ++n)
                    str += "    ";
                str += section.Header + ": " + section.Details;

                var lvi = SectionList.Items.Add(str);
                lvi.Tag = section;
            }

            if (Power.Sections.Count == 0)
            {
                var lvi = SectionList.Items.Add("(none)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
