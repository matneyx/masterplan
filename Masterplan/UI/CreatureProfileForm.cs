using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CreatureProfileForm : Form
    {
        private IRole _fRole;

        public ICreature Creature { get; }

        public CreatureProfileForm(ICreature creature)
        {
            InitializeComponent();

            Creature = creature;
            _fRole = creature.Role;

            // Populate size
            foreach (CreatureSize size in Enum.GetValues(typeof(CreatureSize)))
                SizeBox.Items.Add(size);

            // Populate origin
            var origins = Enum.GetValues(typeof(CreatureOrigin));
            foreach (CreatureOrigin origin in origins)
                OriginBox.Items.Add(origin);

            // Populate type
            var types = Enum.GetValues(typeof(CreatureType));
            foreach (CreatureType type in types)
                TypeBox.Items.Add(type);

            if (Creature is Npc)
            {
                var templateIds = new List<Guid>();
                foreach (var template in Session.Templates)
                    if (template.Type == CreatureTemplateType.Class)
                        templateIds.Add(template.Id);

                foreach (var template in Session.Project.Library.Templates)
                    if (template.Type == CreatureTemplateType.Class)
                    {
                        if (templateIds.Contains(template.Id))
                            continue;

                        templateIds.Add(template.Id);
                    }

                foreach (var templateId in templateIds)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    TemplateBox.Items.Add(template);
                }

                RoleLbl.Enabled = false;
                RoleBtn.Enabled = false;

                CatLbl.Enabled = false;
                CatBox.Enabled = false;
            }
            else if (Creature is CustomCreature)
            {
                TemplateLbl.Enabled = false;
                TemplateBox.Enabled = false;

                CatLbl.Enabled = false;
                CatBox.Enabled = false;
            }
            else
            {
                TemplateLbl.Enabled = false;
                TemplateBox.Enabled = false;

                // Populate categories
                var cats = new List<string>();
                foreach (var c in Session.Creatures)
                {
                    var cat = c.Category;
                    if (cat != "" && !cats.Contains(cat))
                        cats.Add(cat);
                }

                foreach (var cat in cats)
                    CatBox.Items.Add(cat);
            }

            NameBox.Text = Creature.Name;
            LevelBox.Value = Creature.Level;
            SizeBox.SelectedItem = Creature.Size;
            OriginBox.SelectedItem = Creature.Origin;
            TypeBox.SelectedItem = Creature.Type;
            KeywordBox.Text = Creature.Keywords;

            if (Creature is Npc)
            {
                var npc = Creature as Npc;
                var ct = Session.FindTemplate(npc.TemplateId, SearchType.Global);

                if (ct != null)
                    TemplateBox.SelectedItem = ct;
                else
                    TemplateBox.SelectedIndex = 0;

                CatBox.Text = "NPC";
            }
            else if (Creature is CustomCreature)
            {
                CatBox.Text = "Custom Creature";
            }
            else
            {
                RoleBtn.Text = Creature.Role.ToString();
                CatBox.Text = Creature.Category;
            }
        }

        private void RoleBtn_Click(object sender, EventArgs e)
        {
            var dlg = new RoleForm(Creature.Role, ThreatType.Creature);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _fRole = dlg.Role;
                RoleBtn.Text = _fRole.ToString();
            }
        }

        private void TemplateBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ct = TemplateBox.SelectedItem as CreatureTemplate;
            if (ct != null) RoleBtn.Text = ct.Role.ToString();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Creature.Name = NameBox.Text;
            Creature.Level = (int)LevelBox.Value;
            Creature.Size = (CreatureSize)SizeBox.SelectedItem;
            Creature.Origin = (CreatureOrigin)OriginBox.SelectedItem;
            Creature.Type = (CreatureType)TypeBox.SelectedItem;
            Creature.Keywords = KeywordBox.Text;

            if (Creature is Npc)
            {
                var ct = TemplateBox.SelectedItem as CreatureTemplate;

                var npc = Creature as Npc;
                npc.TemplateId = ct?.Id ?? Guid.Empty;
            }
            else
            {
                Creature.Role = _fRole;
                Creature.Category = CatBox.Text;

                if (Creature.Role is Minion)
                    Creature.Hp = 1;
            }
        }
    }
}
