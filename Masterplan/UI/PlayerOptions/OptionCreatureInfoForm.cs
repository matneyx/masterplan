using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionCreatureLoreForm : Form
    {
        public CreatureLore CreatureLore { get; }

        public Pair<int, string> SelectedInformation
        {
            get
            {
                if (InfoList.SelectedItems.Count != 0)
                    return InfoList.SelectedItems[0].Tag as Pair<int, string>;

                return null;
            }
        }

        public OptionCreatureLoreForm(CreatureLore cl)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            SkillBox.Items.Add("Arcana");
            SkillBox.Items.Add("Dungeoneering");
            SkillBox.Items.Add("History");
            SkillBox.Items.Add("Nature");
            SkillBox.Items.Add("Religion");

            CreatureLore = cl.Copy();

            NameBox.Text = CreatureLore.Name;
            SkillBox.Text = CreatureLore.SkillName;

            update_information();
        }

        ~OptionCreatureLoreForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = SelectedInformation != null;
            EditBtn.Enabled = SelectedInformation != null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            CreatureLore.Name = NameBox.Text;
            CreatureLore.SkillName = SkillBox.Text;
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var information = new Pair<int, string>();
            information.First = 10;
            information.Second = "";

            var dlg = new OptionInformationForm(information);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                CreatureLore.Information.Add(dlg.Information);
                update_information();
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedInformation != null)
            {
                CreatureLore.Information.Remove(SelectedInformation);
                update_information();
            }
        }

        private void FeatureEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedInformation != null)
            {
                var index = CreatureLore.Information.IndexOf(SelectedInformation);

                var dlg = new OptionInformationForm(SelectedInformation);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    CreatureLore.Information[index] = dlg.Information;
                    update_information();
                }
            }
        }

        private void update_information()
        {
            CreatureLore.Information.Sort();

            InfoList.Items.Clear();
            foreach (var info in CreatureLore.Information)
            {
                var str = "DC " + info.First + ": " + info.Second;

                var lvi = InfoList.Items.Add(str);
                lvi.Tag = info;
            }
        }
    }
}
