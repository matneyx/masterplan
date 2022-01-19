using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class TokenLinkListForm : Form
    {
        private readonly List<TokenLink> _fLinks;

        public TokenLink SelectedLink
        {
            get
            {
                if (EffectList.SelectedItems.Count != 0)
                    return EffectList.SelectedItems[0].Tag as TokenLink;

                return null;
            }
        }

        public TokenLinkListForm(List<TokenLink> links)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fLinks = links;

            update_list();
        }

        ~TokenLinkListForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = SelectedLink != null;
            EditBtn.Enabled = SelectedLink != null;
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLink != null)
            {
                _fLinks.Remove(SelectedLink);
                update_list();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLink != null)
            {
                var index = _fLinks.IndexOf(SelectedLink);

                var dlg = new TokenLinkForm(SelectedLink);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _fLinks[index] = dlg.Link;
                    update_list();
                }
            }
        }

        private void update_list()
        {
            EffectList.Items.Clear();

            foreach (var link in _fLinks)
            {
                var tokens = "";
                foreach (var token in link.Tokens)
                {
                    var name = "";

                    if (token is CreatureToken)
                    {
                        var ct = token as CreatureToken;
                        name = ct.Data.DisplayName;
                    }

                    if (token is Hero)
                    {
                        var hero = token as Hero;
                        name = hero.Name;
                    }

                    if (token is CustomToken)
                    {
                        var ct = token as CustomToken;
                        name = ct.Name;
                    }

                    if (name == "")
                        name = "(unknown map token)";

                    if (tokens != "")
                        tokens += ", ";

                    tokens += name;
                }

                var lvi = EffectList.Items.Add(tokens);
                lvi.SubItems.Add(link.Text);
                lvi.Tag = link;
            }
        }
    }
}
