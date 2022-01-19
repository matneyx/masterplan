using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CustomCreatureListForm : Form
    {
        public CustomCreature SelectedCreature
        {
            get
            {
                if (CreatureList.SelectedItems.Count != 0)
                    return CreatureList.SelectedItems[0].Tag as CustomCreature;

                return null;
            }
        }

        public Npc SelectedNpc
        {
            get
            {
                if (CreatureList.SelectedItems.Count != 0)
                    return CreatureList.SelectedItems[0].Tag as Npc;

                return null;
            }
        }

        public CustomCreatureListForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            update_creatures();
        }

        ~CustomCreatureListForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = SelectedCreature != null || SelectedNpc != null;
            EditBtn.Enabled = SelectedCreature != null || SelectedNpc != null;
            StatBlockBtn.Enabled = SelectedCreature != null || SelectedNpc != null;
            EncEntryBtn.Enabled = SelectedCreature != null || SelectedNpc != null;
        }

        private void AddCreature_Click(object sender, EventArgs e)
        {
            var cc = new CustomCreature();
            cc.Name = "New Creature";

            var dlg = new CreatureBuilderForm(cc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.CustomCreatures.Add(dlg.Creature as CustomCreature);
                Session.Modified = true;

                update_creatures();
            }
        }

        private void AddNPC_Click(object sender, EventArgs e)
        {
            if (class_templates_exist())
            {
                var npc = new Npc();
                npc.Name = "New NPC";

                foreach (var ct in Session.Templates)
                    if (ct.Type == CreatureTemplateType.Class)
                    {
                        npc.TemplateId = ct.Id;
                        break;
                    }

                var dlg = new CreatureBuilderForm(npc);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.NpCs.Add(dlg.Creature as Npc);
                    Session.Modified = true;

                    update_creatures();
                }
            }
            else
            {
                // Show message
                var msg = "NPCs require class templates; you have no class templates defined.";
                msg += Environment.NewLine;
                msg += "You can define templates in the Libraries screen.";
                MessageBox.Show(msg, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCreature != null)
            {
                var msg = "Are you sure you want to delete this creature?";
                var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No)
                    return;

                Session.Project.CustomCreatures.Remove(SelectedCreature);
                Session.Modified = true;

                update_creatures();
            }

            if (SelectedNpc != null)
            {
                var msg = "Are you sure you want to delete this NPC?";
                var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No)
                    return;

                Session.Project.NpCs.Remove(SelectedNpc);
                Session.Modified = true;

                update_creatures();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCreature != null)
            {
                var index = Session.Project.CustomCreatures.IndexOf(SelectedCreature);

                var dlg = new CreatureBuilderForm(SelectedCreature);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.CustomCreatures[index] = dlg.Creature as CustomCreature;
                    Session.Modified = true;

                    update_creatures();
                }
            }

            if (SelectedNpc != null)
            {
                var index = Session.Project.NpCs.IndexOf(SelectedNpc);

                var dlg = new CreatureBuilderForm(SelectedNpc);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.NpCs[index] = dlg.Creature as Npc;
                    Session.Modified = true;

                    update_creatures();
                }
            }
        }

        private void StatBlockBtn_Click(object sender, EventArgs e)
        {
            EncounterCard card = null;

            if (SelectedCreature != null)
            {
                card = new EncounterCard();
                card.CreatureId = SelectedCreature.Id;
            }

            if (SelectedNpc != null)
            {
                card = new EncounterCard();
                card.CreatureId = SelectedNpc.Id;
            }

            var dlg = new CreatureDetailsForm(card);
            dlg.ShowDialog();
        }

        private void EncEntryBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCreature == null && SelectedNpc == null)
                return;

            var id = SelectedNpc?.Id ?? SelectedCreature.Id;
            var name = SelectedNpc != null ? SelectedNpc.Name : SelectedCreature.Name;
            var cat = SelectedNpc != null ? "People" : "Creatures";

            var entry = Session.Project.Encyclopedia.FindEntryForAttachment(id);

            if (entry == null)
            {
                // If there is no entry, ask to create it
                var msg = "There is no encyclopedia entry associated with " + name + ".";
                msg += Environment.NewLine;
                msg += "Would you like to create one now?";
                if (MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                    DialogResult.No)
                    return;

                entry = new EncyclopediaEntry();
                entry.Name = name;
                entry.AttachmentId = id;
                entry.Category = cat;

                Session.Project.Encyclopedia.Entries.Add(entry);
                Session.Modified = true;
            }

            // Edit the entry
            var index = Session.Project.Encyclopedia.Entries.IndexOf(entry);
            var dlg = new EncyclopediaEntryForm(entry);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.Encyclopedia.Entries[index] = dlg.Entry;
                Session.Modified = true;
            }
        }

        private void update_creatures()
        {
            CreatureList.Items.Clear();

            foreach (var cc in Session.Project.CustomCreatures)
            {
                if (cc == null)
                    return;

                var lvi = CreatureList.Items.Add(cc.Name);
                lvi.SubItems.Add("Level " + cc.Level + " " + cc.Role);
                lvi.SubItems.Add(cc.Hp + " HP; AC " + cc.Ac + ", Fort " + cc.Fortitude + ", Ref " + cc.Reflex +
                                 ", Will " + cc.Will);
                lvi.Group = CreatureList.Groups[0];
                lvi.Tag = cc;
            }

            foreach (var npc in Session.Project.NpCs)
            {
                if (npc == null)
                    return;

                var lvi = CreatureList.Items.Add(npc.Name);
                lvi.SubItems.Add("Level " + npc.Level + " " + npc.Role);
                lvi.SubItems.Add(npc.Hp + " HP; AC " + npc.Ac + ", Fort " + npc.Fortitude + ", Ref " + npc.Reflex +
                                 ", Will " + npc.Will);
                lvi.Group = CreatureList.Groups[1];
                lvi.Tag = npc;
            }

            if (CreatureList.Groups[0].Items.Count == 0)
            {
                var lvi = CreatureList.Items.Add("(no custom creatures)");
                lvi.Group = CreatureList.Groups[0];
                lvi.ForeColor = SystemColors.GrayText;
            }

            if (CreatureList.Groups[1].Items.Count == 0)
            {
                var lvi = CreatureList.Items.Add("(no NPCs)");
                lvi.Group = CreatureList.Groups[1];
                lvi.ForeColor = SystemColors.GrayText;
            }

            CreatureList.Sort();
        }

        private bool class_templates_exist()
        {
            foreach (var lib in Session.Libraries)
            foreach (var ct in lib.Templates)
                if (ct.Type == CreatureTemplateType.Class)
                    return true;

            return false;
        }

        private void CustomCreatureListForm_Shown(object sender, EventArgs e)
        {
            // XP bug
            CreatureList.Invalidate();
        }
    }
}
