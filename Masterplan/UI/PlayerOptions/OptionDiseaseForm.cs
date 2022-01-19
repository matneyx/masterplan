using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionDiseaseForm : Form
    {
        public Disease Disease { get; }

        public string SelectedLevel
        {
            get
            {
                if (LevelList.SelectedItems.Count != 0)
                    return LevelList.SelectedItems[0].Tag as string;

                return null;
            }
        }

        public OptionDiseaseForm(Disease disease)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Disease = disease.Copy();

            NameBox.Text = Disease.Name;
            LevelBox.Text = Disease.Level;
            ImproveBox.Text = Disease.ImproveDc;
            MaintainBox.Text = Disease.MaintainDc;

            DetailsBox.Text = Disease.Details;

            update_list();
        }

        ~OptionDiseaseForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = SelectedLevel != null;
            EditBtn.Enabled = SelectedLevel != null;
            UpBtn.Enabled = SelectedLevel != null && Disease.Levels[0] != SelectedLevel;
            DownBtn.Enabled = SelectedLevel != null && Disease.Levels[Disease.Levels.Count - 1] != SelectedLevel;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Disease.Name = NameBox.Text;
            Disease.Level = LevelBox.Text;
            Disease.ImproveDc = ImproveBox.Text;
            Disease.MaintainDc = MaintainBox.Text;

            Disease.Details = DetailsBox.Text;
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var level = "New Disease Level";

            var dlg = new OptionDiseaseLevelForm(level);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Disease.Levels.Add(dlg.DiseaseLevel);
                update_list();
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLevel != null)
            {
                Disease.Levels.Remove(SelectedLevel);
                update_list();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLevel != null)
            {
                var index = Disease.Levels.IndexOf(SelectedLevel);

                var dlg = new OptionDiseaseLevelForm(SelectedLevel);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Disease.Levels[index] = dlg.DiseaseLevel;
                    update_list();
                }
            }
        }

        private void UpBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLevel != null)
            {
                var index = Disease.Levels.IndexOf(SelectedLevel);
                var temp = Disease.Levels[index - 1];

                Disease.Levels[index - 1] = SelectedLevel;
                Disease.Levels[index] = temp;

                update_list();
            }
        }

        private void DownBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLevel != null)
            {
                var index = Disease.Levels.IndexOf(SelectedLevel);
                var temp = Disease.Levels[index + 1];

                Disease.Levels[index + 1] = SelectedLevel;
                Disease.Levels[index] = temp;

                update_list();
            }
        }

        private void update_list()
        {
            LevelList.Items.Clear();

            LevelList.Items.Add("The target is cured.");
            foreach (var level in Disease.Levels)
            {
                var displayLevel = level;
                if (Disease.Levels.Count > 1)
                {
                    var index = Disease.Levels.IndexOf(level);
                    if (index == 0)
                        displayLevel = "Initial state: " + displayLevel;
                    if (index == Disease.Levels.Count - 1)
                        displayLevel = "Final state: " + displayLevel;
                }

                var lvi = LevelList.Items.Add(displayLevel);
                lvi.Tag = level;
            }
        }
    }
}
