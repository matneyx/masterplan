using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.Tools.Generators;

namespace Masterplan.UI
{
    internal partial class AutoBuildForm : Form
    {
        public enum Mode
        {
            Encounter,
            Delve,
            Deck
        }

        private const string Random = "Random";

        private readonly Mode _fMode = Mode.Encounter;

        public AutoBuildData Data { get; }

        public AutoBuildForm(Mode mode)
        {
            InitializeComponent();

            Data = new AutoBuildData();
            _fMode = mode;

            init_options();

            switch (_fMode)
            {
                case Mode.Encounter:
                {
                    TemplateBox.Items.Add(Random);
                    var names = EncounterBuilder.FindTemplateNames();
                    foreach (var name in names)
                        TemplateBox.Items.Add(name);
                    TemplateBox.SelectedItem = Data.Type != "" ? Data.Type : Random;

                    DiffBox.Items.Add(Difficulty.Random);
                    DiffBox.Items.Add(Difficulty.Easy);
                    DiffBox.Items.Add(Difficulty.Moderate);
                    DiffBox.Items.Add(Difficulty.Hard);
                    DiffBox.SelectedItem = Data.Difficulty;

                    LevelBox.Value = Data.Level;
                    update_cats();
                }
                    break;
                case Mode.Delve:
                {
                    TemplateLbl.Enabled = false;
                    TemplateBox.Enabled = false;
                    TemplateBox.Items.Add("(not applicable)");
                    TemplateBox.SelectedIndex = 0;

                    DiffLbl.Enabled = false;
                    DiffBox.Enabled = false;
                    DiffBox.Items.Add("(not applicable)");
                    DiffBox.SelectedIndex = 0;

                    LevelBox.Value = Data.Level;
                    update_cats();
                }
                    break;
                case Mode.Deck:
                {
                    TemplateLbl.Enabled = false;
                    TemplateBox.Enabled = false;
                    TemplateBox.Items.Add("(not applicable)");
                    TemplateBox.SelectedIndex = 0;

                    DiffLbl.Enabled = false;
                    DiffBox.Enabled = false;
                    DiffBox.Items.Add("(not applicable)");
                    DiffBox.SelectedIndex = 0;

                    LevelBox.Value = Data.Level;
                    update_cats();
                }
                    break;
            }
        }

        private void init_options()
        {
            var categoryTree = new BinarySearchTree<string>();
            var keywordTree = new BinarySearchTree<string>();

            foreach (var c in Session.Creatures)
            {
                if (c.Category != null && c.Category != "")
                    categoryTree.Add(c.Category);

                if (c.Keywords != null && c.Keywords != "")
                {
                    var tokens = c.Keywords.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var token in tokens)
                        keywordTree.Add(token.Trim().ToLower());
                }
            }

            if (categoryTree.Count == 0)
            {
                CatLbl.Enabled = false;
                CatBtn.Enabled = false;
            }

            var keywords = keywordTree.SortedList;
            foreach (var keyword in keywords)
                KeywordBox.Items.Add(keyword);
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            var tokens = KeywordBox.Text.ToLower().Split(null);
            Data.Keywords.Clear();
            foreach (var token in tokens)
                if (token != "")
                    Data.Keywords.Add(token);

            switch (_fMode)
            {
                case Mode.Encounter:
                {
                    Data.Type = TemplateBox.Text != Random ? TemplateBox.Text : "";
                    Data.Difficulty = (Difficulty)DiffBox.SelectedItem;
                    Data.Level = (int)LevelBox.Value;
                }
                    break;
                case Mode.Delve:
                {
                    Data.Type = "";
                    Data.Difficulty = Difficulty.Random;
                    Data.Level = (int)LevelBox.Value;
                }
                    break;
                case Mode.Deck:
                {
                    Data.Type = "";
                    Data.Difficulty = Difficulty.Random;
                    Data.Level = (int)LevelBox.Value;
                }
                    break;
            }
        }

        private void CatBtn_Click(object sender, EventArgs e)
        {
            var dlg = new CategoryListForm(Data.Categories);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Data.Categories = dlg.Categories;
                update_cats();
            }
        }

        private void update_cats()
        {
            CatBtn.Text = Data.Categories == null ? "All Categories" : Data.Categories.Count + " Categories";
        }
    }
}
