using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class CreatureTemplateSelectForm : Form
    {
        public CreatureTemplate Template
        {
            get
            {
                if (CreatureList.SelectedItems.Count != 0)
                    return CreatureList.SelectedItems[0].Tag as CreatureTemplate;

                return null;
            }
        }

        public CreatureTemplateSelectForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            update_list();

            Browser.DocumentText = "";
            CreatureList_SelectedIndexChanged(null, null);
        }

        ~CreatureTemplateSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = Template != null;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (Template != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void CreatureList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var html = "";
            if (Template == null)
            {
                var lines = new List<string>();

                lines.AddRange(Html.GetHead("", "", Session.Preferences.TextSize));
                lines.Add("<BODY>");
                lines.Add("<P class=instruction>");
                lines.Add("(select a template from the list to see its details here)");
                lines.Add("</P>");
                lines.Add("</BODY>");
                lines.Add("</HTML>");

                html = Html.Concatenate(lines);
            }
            else
            {
                html = Html.CreatureTemplate(Template, Session.Preferences.TextSize, false);
            }

            Browser.Document.OpenNew(true);
            Browser.Document.Write(html);
        }

        private void update_list()
        {
            CreatureList.Items.Clear();

            var templates = Session.Templates;
            foreach (var ct in templates)
            {
                if (!Match(ct, NameBox.Text))
                    continue;

                var lvi = CreatureList.Items.Add(ct.Name);
                lvi.SubItems.Add(ct.Info);
                lvi.Tag = ct;

                switch (ct.Type)
                {
                    case CreatureTemplateType.Functional:
                        lvi.Group = CreatureList.Groups[0];
                        break;
                    case CreatureTemplateType.Class:
                        lvi.Group = CreatureList.Groups[1];
                        break;
                }
            }
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            update_list();
            CreatureList_SelectedIndexChanged(null, null);
        }

        private bool Match(CreatureTemplate ct, string query)
        {
            var tokens = query.ToLower().Split();

            foreach (var token in tokens)
                if (!match_token(ct, token))
                    return false;

            return true;
        }

        private bool match_token(CreatureTemplate ct, string token)
        {
            if (ct.Name.ToLower().Contains(token))
                return true;

            if (ct.Info.ToLower().Contains(token))
                return true;

            return false;
        }
    }
}
