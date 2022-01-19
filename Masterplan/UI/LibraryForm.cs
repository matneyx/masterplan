using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class LibraryForm : Form
    {
        public Library Library { get; }

        public LibraryForm(Library lib)
        {
            InitializeComponent();

            Library = lib;

            var user = SystemInformation.UserName;
            var machine = SystemInformation.ComputerName;
            InfoLbl.Text = "Note that when you create a library it will be usable only by this user (" + user +
                           ") on this computer (" + machine + ").";

            NameBox.Text = Library.Name;
            NameBox_TextChanged(null, null);
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Library.Name = NameBox.Text;
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            if (NameBox.Text == "")
            {
                OKBtn.Enabled = false;
            }
            else
            {
                var ass = Assembly.GetEntryAssembly();
                var dir = FileName.Directory(ass.FullName);
                var di = new DirectoryInfo(dir);

                var filename = di + NameBox.Text + ".library";

                OKBtn.Enabled = !File.Exists(filename);
            }
        }
    }
}
