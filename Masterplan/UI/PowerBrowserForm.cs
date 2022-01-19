using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal delegate void PowerCallback(CreaturePower power);

    internal partial class PowerBrowserForm : Form
    {
        private readonly List<string> _fAddedPowers = new List<string>();

        private readonly PowerCallback _fCallback;
        private readonly int _fLevel;

        private readonly string _fName = "";
        private readonly IRole _fRole;
        private bool _fShowAll = true;

        public List<ICreature> SelectedCreatures
        {
            get
            {
                var creatures = new List<ICreature>();

                if (_fShowAll)
                    foreach (ListViewItem lvi in CreatureList.Items)
                    {
                        var creature = lvi.Tag as ICreature;
                        if (creature != null)
                            creatures.Add(creature);
                    }
                else
                    foreach (ListViewItem lvi in CreatureList.SelectedItems)
                    {
                        var creature = lvi.Tag as ICreature;
                        if (creature != null)
                            creatures.Add(creature);
                    }

                return creatures;
            }
        }

        public List<CreaturePower> ShownPowers { get; private set; }

        public CreaturePower SelectedPower { get; private set; }

        public PowerBrowserForm(string name, int level, IRole role, PowerCallback callback)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fName = name;
            _fLevel = level;
            _fRole = role;
            _fCallback = callback;

            var setFilter = FilterPanel.SetFilter(_fLevel, _fRole);

            if (!setFilter)
            {
                update_creature_list();

                if (SelectedCreatures.Count > 100)
                    _fShowAll = false;

                update_powers();
            }
        }

        ~PowerBrowserForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            ModeAll.Checked = _fShowAll;
            ModeSelection.Checked = !_fShowAll;
        }

        private void ModeAll_Click(object sender, EventArgs e)
        {
            _fShowAll = true;

            update_powers();
        }

        private void ModeSelection_Click(object sender, EventArgs e)
        {
            _fShowAll = false;

            update_powers();
        }

        private void FilterPanel_FilterChanged(object sender, EventArgs e)
        {
            update_creature_list();
            update_powers();
        }

        private void StatsBtn_Click(object sender, EventArgs e)
        {
            var dlg = new PowerStatisticsForm(ShownPowers, SelectedCreatures.Count);
            dlg.ShowDialog();
        }

        private void CreatureList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_fShowAll)
                update_powers();
        }

        private void PowerDisplay_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "copy")
            {
                var id = new Guid(e.Url.LocalPath);
                var power = find_power(id);
                if (power != null)
                {
                    e.Cancel = true;

                    copy_power(power);
                }
            }
        }

        private void update_creature_list()
        {
            CreatureList.BeginUpdate();

            var creatures = new List<ICreature>();
            var allCreatures = Session.Creatures;
            foreach (ICreature c in allCreatures)
                creatures.Add(c);
            if (Session.Project != null)
            {
                foreach (ICreature c in Session.Project.CustomCreatures)
                    creatures.Add(c);
                foreach (ICreature c in Session.Project.NpCs)
                    creatures.Add(c);
            }

            var categories = new BinarySearchTree<string>();
            foreach (var c in creatures)
                if (c.Category != "")
                    categories.Add(c.Category);

            var cats = categories.SortedList;
            cats.Insert(0, "Custom Creatures");
            cats.Insert(1, "NPCs");
            cats.Add("Miscellaneous Creatures");

            CreatureList.Groups.Clear();
            foreach (var cat in cats)
                CreatureList.Groups.Add(cat, cat);
            CreatureList.ShowGroups = true;

            var itemList = new List<ListViewItem>();

            foreach (var c in creatures)
            {
                if (c == null)
                    continue;

                Difficulty diff;
                if (!FilterPanel.AllowItem(c, out diff))
                    continue;

                var lvi = new ListViewItem(c.Name);
                lvi.SubItems.Add(c.Info);
                lvi.Tag = c;

                if (c.Category != "")
                    lvi.Group = CreatureList.Groups[c.Category];
                else
                    lvi.Group = CreatureList.Groups["Miscellaneous Creatures"];

                itemList.Add(lvi);
            }

            CreatureList.Items.Clear();
            CreatureList.Items.AddRange(itemList.ToArray());

            if (CreatureList.Items.Count == 0)
            {
                CreatureList.ShowGroups = false;

                var lvi = CreatureList.Items.Add("(no creatures)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            CreatureList.EndUpdate();
        }

        private bool Match(Creature c, string token)
        {
            if (c.Name.ToLower().Contains(token.ToLower()))
                return true;

            if (c.Category.ToLower().Contains(token.ToLower()))
                return true;

            if (c.Info.ToLower().Contains(token.ToLower()))
                return true;

            if (c.Phenotype.ToLower().Contains(token.ToLower()))
                return true;

            return false;
        }

        private bool role_matches(IRole roleA, IRole roleB)
        {
            if (roleA is ComplexRole && roleB is ComplexRole)
            {
                var crA = roleA as ComplexRole;
                var crB = roleB as ComplexRole;

                return crA.Type == crB.Type;
            }

            if (roleA is Minion && roleB is Minion)
            {
                var minionA = roleA as Minion;
                var minionB = roleB as Minion;

                if (minionA.HasRole == false && minionB.HasRole == false)
                    return true;

                if (minionA.HasRole && minionB.HasRole)
                    return minionA.Type == minionB.Type;

                return false;
            }

            return false;
        }

        private void update_powers()
        {
            Cursor.Current = Cursors.WaitCursor;

            var content = new List<string>();
            ShownPowers = new List<CreaturePower>();

            content.AddRange(Html.GetHead(null, null, Session.Preferences.TextSize));
            content.Add("<BODY>");

            var creatures = SelectedCreatures;
            if (creatures != null && creatures.Count != 0)
            {
                content.Add("<P class=instruction>");
                if (_fShowAll)
                    content.Add("These creatures have the following powers:");
                else
                    content.Add("The selected creatures have the following powers:");
                content.Add("</P>");

                var powers = new Dictionary<CreaturePowerCategory, List<CreaturePower>>();

                var powerCategories = Enum.GetValues(typeof(CreaturePowerCategory));
                foreach (CreaturePowerCategory cat in powerCategories)
                    powers[cat] = new List<CreaturePower>();

                foreach (var c in creatures)
                foreach (var cp in c.CreaturePowers)
                {
                    powers[cp.Category].Add(cp);
                    ShownPowers.Add(cp);
                }

                content.Add("<P class=table>");
                content.Add("<TABLE>");

                foreach (CreaturePowerCategory cat in powerCategories)
                {
                    var count = powers[cat].Count;
                    if (count == 0)
                        continue;

                    powers[cat].Sort();

                    var name = "";
                    switch (cat)
                    {
                        case CreaturePowerCategory.Trait:
                            name = "Traits";
                            break;
                        case CreaturePowerCategory.Standard:
                        case CreaturePowerCategory.Move:
                        case CreaturePowerCategory.Minor:
                        case CreaturePowerCategory.Free:
                            name = cat + " Actions";
                            break;
                        case CreaturePowerCategory.Triggered:
                            name = "Triggered Actions";
                            break;
                        case CreaturePowerCategory.Other:
                            name = "Other Actions";
                            break;
                    }

                    content.Add("<TR class=creature>");
                    content.Add("<TD colspan=3>");
                    content.Add("<B>" + name + "</B>");
                    content.Add("</TD>");
                    content.Add("</TR>");

                    foreach (var power in powers[cat])
                    {
                        content.AddRange(power.AsHtml(null, CardMode.View, false));

                        content.Add("<TR>");
                        content.Add("<TD colspan=3 align=center>");

                        if (_fName != null && _fName != "")
                            content.Add("<A href=copy:" + power.Id + ">copy this power into " + _fName + "</A>");
                        else
                            content.Add("<A href=copy:" + power.Id + ">select this power</A>");

                        content.Add("</TD>");
                        content.Add("</TR>");
                    }
                }

                content.Add("</TABLE>");
                content.Add("</P>");
            }
            else
            {
                content.Add("<P class=instruction>");
                content.Add("(no creatures selected)");
                content.Add("</P>");
            }

            content.Add("</BODY>");
            content.Add("</HTML>");

            PowerDisplay.DocumentText = Html.Concatenate(content);

            Cursor.Current = Cursors.Default;
        }

        private CreaturePower find_power(Guid id)
        {
            foreach (var power in ShownPowers)
                if (power.Id == id)
                    return power;

            return null;
        }

        private void copy_power(CreaturePower power)
        {
            var cp = power.Copy();
            cp.Id = Guid.NewGuid();

            if (_fCallback != null)
            {
                _fCallback(cp);
                _fAddedPowers.Add(cp.Name);

                update_powers();
            }
            else
            {
                SelectedPower = power;

                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
