using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class DamageModListForm : Form
    {
        private readonly ICreature _fCreature;
        private readonly CreatureTemplate _fTemplate;

        public List<DamageModifier> Modifiers { get; }

        public List<DamageModifierTemplate> ModifierTemplates { get; }

        public DamageModifier SelectedDamageMod
        {
            get
            {
                if (DamageList.SelectedItems.Count != 0)
                    return DamageList.SelectedItems[0].Tag as DamageModifier;

                return null;
            }
        }

        public DamageModifierTemplate SelectedDamageModTemplate
        {
            get
            {
                if (DamageList.SelectedItems.Count != 0)
                    return DamageList.SelectedItems[0].Tag as DamageModifierTemplate;

                return null;
            }
        }

        public DamageModListForm(ICreature creature)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fCreature = creature;

            Modifiers = new List<DamageModifier>();
            foreach (var mod in _fCreature.DamageModifiers)
                Modifiers.Add(mod.Copy());

            update_damage_list();

            ResistBox.Text = _fCreature.Resist;
            VulnerableBox.Text = _fCreature.Vulnerable;
            ImmuneBox.Text = _fCreature.Immune;
        }

        public DamageModListForm(CreatureTemplate template)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fTemplate = template;

            ModifierTemplates = new List<DamageModifierTemplate>();
            foreach (var mod in _fTemplate.DamageModifierTemplates)
                ModifierTemplates.Add(mod.Copy());

            update_damage_list();

            ResistBox.Text = _fTemplate.Resist;
            VulnerableBox.Text = _fTemplate.Vulnerable;
            ImmuneBox.Text = _fTemplate.Immune;
        }

        ~DamageModListForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveDmgBtn.Enabled = SelectedDamageMod != null || SelectedDamageModTemplate != null;
            EditDmgBtn.Enabled = SelectedDamageMod != null || SelectedDamageModTemplate != null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            if (_fCreature != null)
            {
                _fCreature.DamageModifiers = Modifiers;
                _fCreature.Resist = ResistBox.Text;
                _fCreature.Vulnerable = VulnerableBox.Text;
                _fCreature.Immune = ImmuneBox.Text;
            }

            if (_fTemplate != null)
            {
                _fTemplate.DamageModifierTemplates = ModifierTemplates;
                _fTemplate.Resist = ResistBox.Text;
                _fTemplate.Vulnerable = VulnerableBox.Text;
                _fTemplate.Immune = ImmuneBox.Text;
            }
        }

        private void AddDmgBtn_Click(object sender, EventArgs e)
        {
            if (_fCreature != null)
            {
                var dm = new DamageModifier();

                var dlg = new DamageModifierForm(dm);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Modifiers.Add(dlg.Modifier);
                    update_damage_list();
                }
            }

            if (_fTemplate != null)
            {
                var dm = new DamageModifierTemplate();

                var dlg = new DamageModifierTemplateForm(dm);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ModifierTemplates.Add(dlg.Modifier);
                    update_damage_list();
                }
            }
        }

        private void RemoveDmgBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDamageMod != null)
            {
                Modifiers.Remove(SelectedDamageMod);
                update_damage_list();
            }

            if (SelectedDamageModTemplate != null)
            {
                ModifierTemplates.Remove(SelectedDamageModTemplate);
                update_damage_list();
            }
        }

        private void EditDmgBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDamageMod != null)
            {
                var index = Modifiers.IndexOf(SelectedDamageMod);

                var dlg = new DamageModifierForm(SelectedDamageMod);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Modifiers[index] = dlg.Modifier;
                    update_damage_list();
                }
            }

            if (SelectedDamageModTemplate != null)
            {
                var index = ModifierTemplates.IndexOf(SelectedDamageModTemplate);

                var dlg = new DamageModifierTemplateForm(SelectedDamageModTemplate);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ModifierTemplates[index] = dlg.Modifier;
                    update_damage_list();
                }
            }
        }

        private void update_damage_list()
        {
            DamageList.Items.Clear();
            DamageList.ShowGroups = true;

            if (Modifiers != null)
                foreach (var dm in Modifiers)
                {
                    var lvi = DamageList.Items.Add(dm.ToString());
                    lvi.Tag = dm;

                    if (dm.Value == 0)
                        lvi.Group = DamageList.Groups[0];
                    if (dm.Value < 0)
                        lvi.Group = DamageList.Groups[1];
                    if (dm.Value > 0)
                        lvi.Group = DamageList.Groups[2];
                }

            if (ModifierTemplates != null)
                foreach (var dm in ModifierTemplates)
                {
                    var lvi = DamageList.Items.Add(dm.ToString());
                    lvi.Tag = dm;

                    var value = dm.HeroicValue + dm.ParagonValue + dm.EpicValue;

                    if (value == 0)
                        lvi.Group = DamageList.Groups[0];
                    if (value < 0)
                        lvi.Group = DamageList.Groups[1];
                    if (value > 0)
                        lvi.Group = DamageList.Groups[2];
                }

            if (DamageList.Items.Count == 0)
            {
                DamageList.ShowGroups = false;

                var lvi = DamageList.Items.Add("(none)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
