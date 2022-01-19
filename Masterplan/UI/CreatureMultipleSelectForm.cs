using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class CreatureMultipleSelectForm : Form
    {
        public List<ICreature> SelectedCreatures { get; } = new List<ICreature>();

        public ICreature SelectedCreature
        {
            get
            {
                if (CreatureList.SelectedItems.Count != 0)
                    return CreatureList.SelectedItems[0].Tag as ICreature;

                return null;
            }
        }

        public CreatureMultipleSelectForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            update_list();

            Browser.DocumentText = "";

            update_stats();
        }

        ~CreatureMultipleSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = SelectedCreatures.Count >= 2;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedCreature != null)
            {
                SelectedCreatures.Add(SelectedCreature);

                update_list();
                update_stats();
            }
        }

        private void CreatureList_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void update_list()
        {
            CreatureList.BeginUpdate();

            CreatureList.Groups.Clear();
            CreatureList.Items.Clear();

            var creatures = Session.Creatures;

            var bst = new BinarySearchTree<string>();
            foreach (var c in creatures)
                bst.Add(c.Category);

            var cats = bst.SortedList;
            cats.Add("Miscellaneous Creatures");
            foreach (var cat in cats)
                CreatureList.Groups.Add(cat, cat);

            var items = new List<ListViewItem>();
            foreach (var c in creatures)
            {
                if (!Match(c, NameBox.Text))
                    continue;

                if (SelectedCreatures.Contains(c))
                    continue;

                var lvi = new ListViewItem(c.Name);
                lvi.SubItems.Add(c.Info);
                lvi.Tag = c;

                if (c.Category != null && c.Category != "")
                    lvi.Group = CreatureList.Groups[c.Category];
                else
                    lvi.Group = CreatureList.Groups["Miscellaneous Creatures"];

                items.Add(lvi);
            }

            CreatureList.Items.AddRange(items.ToArray());
            CreatureList.EndUpdate();
        }

        private void update_stats()
        {
            var lines = Html.GetHead("", "", Session.Preferences.TextSize);

            lines.Add("<BODY>");

            if (SelectedCreatures.Count != 0)
            {
                lines.Add("<P class=table>");
                lines.Add("<TABLE>");
                lines.Add("<TR class=heading>");
                lines.Add("<TD colspan=3><B>Selected Creatures</B></TD>");
                lines.Add("</TR>");

                foreach (var c in SelectedCreatures)
                {
                    lines.Add("<TR class=header>");
                    lines.Add("<TD colspan=2>" + c.Name + "</TD>");
                    lines.Add("<TD align=center><A href=remove:" + c.Id + ">remove</A></TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }
            else
            {
                lines.Add("<P class=instruction>");
                lines.Add(
                    "You have not yet selected any creatures; to select a creature, drag it from the list at the left onto the box above");
                lines.Add("</P>");
            }

            foreach (var creature in SelectedCreatures)
            {
                var card = new EncounterCard(creature);

                lines.Add("<P class=table>");
                lines.AddRange(card.AsText(null, CardMode.View, false));
                lines.Add("</P>");
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            var html = Html.Concatenate(lines);

            Browser.Document.OpenNew(true);
            Browser.Document.Write(html);
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            update_list();
        }

        private bool Match(ICreature creature, string query)
        {
            var tokens = query.ToLower().Split();

            foreach (var token in tokens)
                if (!match_token(creature, token))
                    return false;

            return true;
        }

        private bool match_token(ICreature creature, string token)
        {
            if (creature.Name.ToLower().Contains(token))
                return true;

            if (creature.Info.ToLower().Contains(token))
                return true;

            return false;
        }

        private void CreatureList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedCreature != null)
            {
                DragLbl.BackColor = SystemColors.Highlight;
                DragLbl.ForeColor = SystemColors.HighlightText;

                if (DoDragDrop(SelectedCreature, DragDropEffects.Move) == DragDropEffects.Move)
                {
                    SelectedCreatures.Add(SelectedCreature);

                    update_list();
                    update_stats();
                }

                DragLbl.BackColor = SystemColors.Control;
                DragLbl.ForeColor = SystemColors.ControlText;
            }
        }

        private void DragLbl_DragOver(object sender, DragEventArgs e)
        {
            if (has_creature(e.Data))
                e.Effect = DragDropEffects.Move;
        }

        private void DragLbl_DragDrop(object sender, DragEventArgs e)
        {
            if (has_creature(e.Data))
                e.Effect = DragDropEffects.Move;
        }

        private bool has_creature(IDataObject data)
        {
            var c = data.GetData(typeof(Creature)) as Creature;
            if (c != null)
                return true;

            var cc = data.GetData(typeof(CustomCreature)) as CustomCreature;
            if (cc != null)
                return true;

            var npc = data.GetData(typeof(Npc)) as Npc;
            if (npc != null)
                return true;

            return false;
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "remove")
            {
                var id = new Guid(e.Url.LocalPath);
                var creature = find_creature(id);
                if (creature != null)
                {
                    e.Cancel = true;

                    SelectedCreatures.Remove(creature);

                    update_list();
                    update_stats();
                }
            }
        }

        private ICreature find_creature(Guid id)
        {
            foreach (var creature in SelectedCreatures)
                if (creature.Id == id)
                    return creature;

            return null;
        }
    }
}
