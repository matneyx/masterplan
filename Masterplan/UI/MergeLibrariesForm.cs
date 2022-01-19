using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class MergeLibrariesForm : Form
    {
        public List<Library> SelectedLibraries
        {
            get
            {
                var list = new List<Library>();

                foreach (ListViewItem lvi in ThemeList.CheckedItems)
                {
                    var lib = lvi.Tag as Library;
                    if (lib != null)
                        list.Add(lib);
                }

                return list;
            }
        }

        public string LibraryName => NameBox.Text;

        public MergeLibrariesForm()
        {
            InitializeComponent();

            foreach (var lib in Session.Libraries)
            {
                var lvi = ThemeList.Items.Add(lib.Name);
                lvi.Tag = lib;
            }

            NameBox.Text = "Merged Library";

            Application.Idle += Application_Idle;
        }

        ~MergeLibrariesForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = SelectedLibraries.Count >= 2;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
        }
    }
}
