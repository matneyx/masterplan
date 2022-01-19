using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.UI;

namespace Masterplan.Controls
{
    internal partial class WelcomePanel : UserControl
    {
        public WelcomePanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint, true);

            MenuBrowser.DocumentText = "";
            RefreshOptions();
        }

        private void TitlePanel_FadeFinished(object sender, EventArgs e)
        {
        }

        private void MenuBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "masterplan")
            {
                e.Cancel = true;

                if (e.Url.LocalPath == "new")
                    OnNewProjectClicked();

                if (e.Url.LocalPath == "open")
                    OnOpenProjectClicked();

                if (e.Url.LocalPath == "last")
                    OnOpenLastProjectClicked();

                if (e.Url.LocalPath == "delve")
                    OnDelveClicked();

                if (e.Url.LocalPath == "genesis")
                {
                    var c = new Creature();
                    c.Name = "New Creature";

                    var dlg = new CreatureBuilderForm(c);
                    dlg.ShowDialog();
                }

                if (e.Url.LocalPath == "exodus")
                {
                    var ct = new CreatureTemplate();
                    ct.Name = "New Template";

                    var dlg = new CreatureTemplateBuilderForm(ct);
                    dlg.ShowDialog();
                }

                if (e.Url.LocalPath == "minos")
                {
                    var trap = new Trap();
                    trap.Name = "New Trap";
                    trap.Attacks.Add(new TrapAttack());

                    var dlg = new TrapBuilderForm(trap);
                    dlg.ShowDialog();
                }

                if (e.Url.LocalPath == "excalibur")
                {
                    var mi = new MagicItem();
                    mi.Name = "New Magic Item";

                    var dlg = new MagicItemBuilderForm(mi);
                    dlg.ShowDialog();
                }

                if (e.Url.LocalPath == "indiana")
                {
                    var a = new Artifact();
                    a.Name = "New Artifact";

                    var dlg = new ArtifactBuilderForm(a);
                    dlg.ShowDialog();
                }

                if (e.Url.LocalPath == "manual")
                    open_manual();
            }
        }

        [Category("Actions")] public event EventHandler NewProjectClicked;

        [Category("Actions")] public event EventHandler OpenProjectClicked;

        [Category("Actions")] public event EventHandler OpenLastProjectClicked;

        [Category("Actions")] public event EventHandler DelveClicked;

        protected void OnNewProjectClicked()
        {
            NewProjectClicked?.Invoke(this, EventArgs.Empty);
        }

        protected void OnOpenProjectClicked()
        {
            OpenProjectClicked?.Invoke(this, EventArgs.Empty);
        }

        protected void OnOpenLastProjectClicked()
        {
            OpenLastProjectClicked?.Invoke(this, EventArgs.Empty);
        }

        protected void OnDelveClicked()
        {
            DelveClicked?.Invoke(this, EventArgs.Empty);
        }

        public void RefreshOptions()
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(Html.GetHead("Masterplan", "Main Menu", Session.Preferences.TextSize));
            lines.Add("<BODY>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE class=wide>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD>");
            lines.Add("<B>Getting Started</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (show_last_file_option())
            {
                var name = FileName.Name(Session.Preferences.LastFile);

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"masterplan:last\">Reopen <I>" + name + "</I></A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("<TR>");
            lines.Add("<TD>");
            lines.Add("<A href=\"masterplan:new\">Create a new adventure project</A>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>");
            lines.Add("<A href=\"masterplan:open\">Open an existing project</A>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (show_delve_option())
            {
                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"masterplan:delve\">Generate a random dungeon delve</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (show_manual_option())
            {
                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"masterplan:manual\">Read the Masterplan user manual</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            if (Program.IsBeta)
            {
                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>Development Links</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"masterplan:genesis\">Project Genesis</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"masterplan:exodus\">Project Exodus</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"masterplan:minos\">Project Minos</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"masterplan:excalibur\">Project Excalibur</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"masterplan:indiana\">Project Indiana</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            MenuBrowser.Document.OpenNew(true);
            MenuBrowser.Document.Write(Html.Concatenate(lines));
        }

        private bool show_last_file_option()
        {
            if (Session.Preferences.LastFile == null || Session.Preferences.LastFile == "")
                return false;

            return File.Exists(Session.Preferences.LastFile);
        }

        private bool show_delve_option()
        {
            foreach (var lib in Session.Libraries)
                if (lib.ShowInAutoBuild)
                    return true;

            return false;
        }

        private bool show_manual_option()
        {
            var manualFile = get_manual_filename();
            return File.Exists(manualFile);
        }

        private void open_manual()
        {
            var manualFile = get_manual_filename();
            if (!File.Exists(manualFile))
                return;

            Process.Start(manualFile);
        }

        private string get_manual_filename()
        {
            var ass = Assembly.GetEntryAssembly();
            return FileName.Directory(ass.FullName) + "Manual.pdf";
        }

        private class Headline : IComparable<Headline>
        {
            public DateTime Date = DateTime.Now;
            public string Title = "";
            public string Url = "";

            public int CompareTo(Headline rhs)
            {
                return Date.CompareTo(rhs.Date) * -1;
            }
        }
    }
}
