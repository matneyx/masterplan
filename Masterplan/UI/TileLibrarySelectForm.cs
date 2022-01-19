using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class TileLibrarySelectForm : Form
    {
        public List<Library> Libraries
        {
            get
            {
                var libs = new List<Library>();

                foreach (ListViewItem lvi in LibraryList.CheckedItems)
                {
                    var lib = lvi.Tag as Library;

                    if (lib != null)
                        libs.Add(lib);
                }

                return libs;
            }
        }

        public TileLibrarySelectForm(List<Library> selectedLibraries)
        {
            InitializeComponent();

            var libraries = new List<Library>();
            libraries.AddRange(Session.Libraries);
            libraries.Add(Session.Project.Library);

            foreach (var lib in libraries)
            {
                if (lib.Tiles.Count == 0)
                    continue;

                var lvi = LibraryList.Items.Add(lib.Name);
                lvi.Tag = lib;

                lvi.Checked = selectedLibraries.Contains(lib);
            }

            Application.Idle += Application_Idle;
        }

        ~TileLibrarySelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = Libraries.Count != 0;
        }

        private void SelectAllBtn_Click(object sender, EventArgs e)
        {
            LibraryList.BeginUpdate();

            foreach (ListViewItem lvi in LibraryList.Items)
                lvi.Checked = true;

            LibraryList.EndUpdate();
        }

        private void DeselectAllBtn_Click(object sender, EventArgs e)
        {
            LibraryList.BeginUpdate();

            foreach (ListViewItem lvi in LibraryList.Items)
                lvi.Checked = false;

            LibraryList.EndUpdate();
        }
    }
}
