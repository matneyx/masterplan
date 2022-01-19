using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class LibrarySelectForm : Form
    {
        public Library SelectedLibrary
        {
            get
            {
                if (ThemeList.SelectedItems.Count != 0)
                    return ThemeList.SelectedItems[0].Tag as Library;

                return null;
            }
        }

        public LibrarySelectForm()
        {
            InitializeComponent();

            foreach (var lib in Session.Libraries)
            {
                var lvi = ThemeList.Items.Add(lib.Name);
                lvi.Tag = lib;
            }

            Application.Idle += Application_Idle;
        }

        ~LibrarySelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = SelectedLibrary != null;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedLibrary != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
