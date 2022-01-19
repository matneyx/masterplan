using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class MonsterThemeSelectForm : Form
    {
        public MonsterTheme MonsterTheme
        {
            get
            {
                if (ThemeList.SelectedItems.Count != 0)
                    return ThemeList.SelectedItems[0].Tag as MonsterTheme;

                return null;
            }
        }

        public MonsterThemeSelectForm()
        {
            InitializeComponent();

            var themes = Session.Themes;

            foreach (var mt in themes)
            {
                var lvi = ThemeList.Items.Add(mt.Name);
                lvi.Tag = mt;
            }

            Application.Idle += Application_Idle;
        }

        ~MonsterThemeSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = MonsterTheme != null;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (MonsterTheme != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
