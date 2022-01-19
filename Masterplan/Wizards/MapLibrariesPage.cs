using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools.Generators;

namespace Masterplan.Wizards
{
    internal partial class MapLibrariesPage : UserControl, IWizardPage
    {
        private MapBuilderData _fData;

        public MapLibrariesPage()
        {
            InitializeComponent();
        }

        private void SelectAllBtn_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in LibraryList.Items)
                lvi.Checked = true;
        }

        private void DeselectAllBtn_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in LibraryList.Items)
                lvi.Checked = false;
        }

        private void set_selected_libraries()
        {
            _fData.Libraries.Clear();
            foreach (ListViewItem lvi in LibraryList.CheckedItems)
            {
                var lib = lvi.Tag as Library;
                _fData.Libraries.Add(lib);
            }
        }

        private void InfoLinkLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var msg =
                "In order to be used with AutoBuild, map tiles need to be categorised (as doors, stairs, etc), so that they can be placed intelligently.";
            msg += Environment.NewLine;
            msg += Environment.NewLine;
            msg += "Libraries which do not have categorised tiles cannot be used, and so are not shown in the list.";
            msg += Environment.NewLine;
            msg += Environment.NewLine;
            msg += "You can set tile categories in the Libraries screen.";

            MessageBox.Show(this, msg, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public bool AllowNext => LibraryList.CheckedItems.Count != 0;

        public bool AllowBack
        {
            get
            {
                if (_fData.DelveOnly)
                    return false;

                return true;
            }
        }

        public bool AllowFinish => LibraryList.CheckedItems.Count != 0;

        public void OnShown(object data)
        {
            if (_fData == null)
            {
                _fData = data as MapBuilderData;

                LibraryList.Items.Clear();
                foreach (var lib in Session.Libraries)
                {
                    if (!lib.ShowInAutoBuild)
                        continue;

                    var lvi = LibraryList.Items.Add(lib.Name);
                    lvi.Checked = _fData.Libraries.Contains(lib);
                    lvi.Tag = lib;
                }

                if (LibraryList.Items.Count == 0)
                {
                    var lvi = LibraryList.Items.Add("(no libraries)");
                    lvi.ForeColor = SystemColors.GrayText;

                    LibraryList.CheckBoxes = false;
                }
            }
        }

        public bool OnBack()
        {
            return true;
        }

        public bool OnNext()
        {
            set_selected_libraries();
            return true;
        }

        public bool OnFinish()
        {
            set_selected_libraries();
            return true;
        }
    }
}
