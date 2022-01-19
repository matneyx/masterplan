using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class NpcForm : Form
    {
        private readonly bool _fUpdating;

        public Npc Npc { get; }

        public CreaturePower SelectedPower
        {
            get
            {
                if (PowerList.SelectedItems.Count != 0)
                    return PowerList.SelectedItems[0].Tag as CreaturePower;

                return null;
            }
        }

        public DamageModifier SelectedDamageMod
        {
            get
            {
                if (DamageList.SelectedItems.Count != 0)
                    return DamageList.SelectedItems[0].Tag as DamageModifier;

                return null;
            }
        }

        public Aura SelectedAura
        {
            get
            {
                if (AuraList.SelectedItems.Count != 0)
                    return AuraList.SelectedItems[0].Tag as Aura;

                return null;
            }
        }

        public DetailsField SelectedField
        {
            get
            {
                if (DetailsList.SelectedItems.Count != 0)
                    return (DetailsField)DetailsList.SelectedItems[0].Tag;

                return DetailsField.None;
            }
        }

        public NpcForm(Npc cc, bool unused)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

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

            Npc = cc.Copy();

            _fUpdating = true;

            NameBox.Text = Npc.Name;
            LevelBox.Value = Npc.Level;
            InfoBtn.Text = Npc.Phenotype;

            var ct = Session.FindTemplate(Npc.TemplateId, SearchType.Global);
            if (ct != null)
                TemplateBox.SelectedItem = ct;
            else
                TemplateBox.SelectedIndex = 0;

            StrBox.Value = Npc.Strength.Score;
            ConBox.Value = Npc.Constitution.Score;
            DexBox.Value = Npc.Dexterity.Score;
            IntBox.Value = Npc.Intelligence.Score;
            WisBox.Value = Npc.Wisdom.Score;
            ChaBox.Value = Npc.Charisma.Score;

            InitModBox.Value = Npc.InitiativeModifier;
            HPModBox.Value = Npc.HpModifier;
            ACModBox.Value = Npc.AcModifier;
            FortModBox.Value = Npc.FortitudeModifier;
            RefModBox.Value = Npc.ReflexModifier;
            WillModBox.Value = Npc.WillModifier;

            _fUpdating = false;

            update_fields();
            update_powers_list();
            update_aura_list();
            update_damage_list();
            update_details();

            if (Npc.Image != null)
                PortraitBox.Image = Npc.Image;
        }

        ~NpcForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            PowerRemoveBtn.Enabled = SelectedPower != null;
            PowerEditBtn.Enabled = SelectedPower != null;

            PowerUpBtn.Enabled = SelectedPower != null && Npc.CreaturePowers.IndexOf(SelectedPower) != 0;
            PowerDownBtn.Enabled = SelectedPower != null &&
                                   Npc.CreaturePowers.IndexOf(SelectedPower) != Npc.CreaturePowers.Count - 1;

            AuraRemoveBtn.Enabled = SelectedAura != null;
            AuraEditBtn.Enabled = SelectedAura != null;

            RemoveDmgBtn.Enabled = SelectedDamageMod != null;
            EditDmgBtn.Enabled = SelectedDamageMod != null;

            ClearRegenLbl.Visible = Npc.Regeneration != null;

            DetailsEditBtn.Enabled = SelectedField != DetailsField.None;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Npc.Name = NameBox.Text;
        }

        private void TemplateBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ct = TemplateBox.SelectedItem as CreatureTemplate;
            Npc.TemplateId = ct?.Id ?? Guid.Empty;

            update_fields();
        }

        private void ParameterChanged(object sender, EventArgs e)
        {
            update_fields();
        }

        private void update_fields()
        {
            if (_fUpdating)
                return;

            Npc.Level = (int)LevelBox.Value;

            Npc.Strength.Score = (int)StrBox.Value;
            Npc.Constitution.Score = (int)ConBox.Value;
            Npc.Dexterity.Score = (int)DexBox.Value;
            Npc.Intelligence.Score = (int)IntBox.Value;
            Npc.Wisdom.Score = (int)WisBox.Value;
            Npc.Charisma.Score = (int)ChaBox.Value;

            // Count point-buy cost
            var cost = Npc.AbilityCost;
            CostBox.Text = cost != -1 ? cost.ToString() : "(n/a)";

            Npc.InitiativeModifier = (int)InitModBox.Value;
            Npc.HpModifier = (int)HPModBox.Value;
            Npc.AcModifier = (int)ACModBox.Value;
            Npc.FortitudeModifier = (int)FortModBox.Value;
            Npc.ReflexModifier = (int)RefModBox.Value;
            Npc.WillModifier = (int)WillModBox.Value;

            var level = Npc.Level / 2;

            var strength = Npc.Strength.Modifier;
            StrModBox.Text = strength + " / " + (strength + level);

            var constitution = Npc.Constitution.Modifier;
            ConModBox.Text = constitution + " / " + (constitution + level);

            var dexterity = Npc.Dexterity.Modifier;
            DexModBox.Text = dexterity + " / " + (dexterity + level);

            var intelligence = Npc.Intelligence.Modifier;
            IntModBox.Text = intelligence + " / " + (intelligence + level);

            var wisdom = Npc.Wisdom.Modifier;
            WisModBox.Text = wisdom + " / " + (wisdom + level);

            var charisma = Npc.Charisma.Modifier;
            ChaModBox.Text = charisma + " / " + (charisma + level);

            InitBox.Text = Npc.Initiative.ToString();
            HPBox.Text = Npc.Hp.ToString();
            ACBox.Text = Npc.Ac.ToString();
            FortBox.Text = Npc.Fortitude.ToString();
            RefBox.Text = Npc.Reflex.ToString();
            WillBox.Text = Npc.Will.ToString();
        }

        private void PowerAddBtn_Click(object sender, EventArgs e)
        {
            var p = new CreaturePower();
            p.Name = "New Power";

            var dlg = new PowerBuilderForm(p, Npc, false);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Npc.CreaturePowers.Add(dlg.Power);
                update_powers_list();
            }
        }

        private void PowerRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPower != null)
            {
                Npc.CreaturePowers.Remove(SelectedPower);
                update_powers_list();
            }
        }

        private void PowerEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPower != null)
            {
                var index = Npc.CreaturePowers.IndexOf(SelectedPower);

                var dlg = new PowerBuilderForm(SelectedPower, Npc, false);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Npc.CreaturePowers[index] = dlg.Power;
                    update_powers_list();
                }
            }
        }

        private void update_powers_list()
        {
            PowerList.Items.Clear();
            foreach (var p in Npc.CreaturePowers)
            {
                var lvi = PowerList.Items.Add(p.Name);
                lvi.Tag = p;
            }

            if (PowerList.Items.Count == 0)
            {
                var lvi = PowerList.Items.Add("(no powers)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void AddDmgBtn_Click(object sender, EventArgs e)
        {
            var dm = new DamageModifier();

            var dlg = new DamageModifierForm(dm);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Npc.DamageModifiers.Add(dlg.Modifier);
                update_damage_list();
            }
        }

        private void RemoveDmgBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDamageMod != null)
            {
                Npc.DamageModifiers.Remove(SelectedDamageMod);
                update_damage_list();
            }
        }

        private void EditDmgBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDamageMod != null)
            {
                var index = Npc.DamageModifiers.IndexOf(SelectedDamageMod);

                var dlg = new DamageModifierForm(SelectedDamageMod);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Npc.DamageModifiers[index] = dlg.Modifier;
                    update_damage_list();
                }
            }
        }

        private void update_damage_list()
        {
            DamageList.Items.Clear();
            DamageList.ShowGroups = true;

            foreach (var dm in Npc.DamageModifiers)
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

            if (Npc.DamageModifiers.Count == 0)
            {
                DamageList.ShowGroups = false;

                var lvi = DamageList.Items.Add("(none)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void AuraAddBtn_Click(object sender, EventArgs e)
        {
            var a = new Aura();
            a.Name = "New Aura";

            var dlg = new AuraForm(a);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Npc.Auras.Add(dlg.Aura);
                update_aura_list();
            }
        }

        private void AuraRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedAura != null)
            {
                Npc.Auras.Remove(SelectedAura);
                update_aura_list();
            }
        }

        private void AuraEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedAura != null)
            {
                var index = Npc.Auras.IndexOf(SelectedAura);

                var dlg = new AuraForm(SelectedAura);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Npc.Auras[index] = dlg.Aura;
                    update_aura_list();
                }
            }
        }

        private void update_aura_list()
        {
            AuraList.Items.Clear();
            foreach (var a in Npc.Auras)
            {
                var lvi = AuraList.Items.Add(a.Name);
                lvi.Tag = a;
            }

            if (AuraList.Items.Count == 0)
            {
                var lvi = AuraList.Items.Add("(no auras)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void DetailsEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedField != DetailsField.None)
            {
                var dlg = new DetailsForm(Npc, SelectedField, null);
                if (dlg.ShowDialog() == DialogResult.OK)
                    update_details();
            }
        }

        private void update_details()
        {
            DetailsList.Items.Clear();

            var senses = DetailsList.Items.Add("Senses");
            senses.SubItems.Add(Npc.Senses);
            senses.Tag = DetailsField.Senses;

            var move = DetailsList.Items.Add("Movement");
            move.SubItems.Add(Npc.Movement);
            move.Tag = DetailsField.Movement;

            var resist = DetailsList.Items.Add("Resist");
            resist.SubItems.Add(Npc.Resist);
            resist.Tag = DetailsField.Resist;

            var vuln = DetailsList.Items.Add("Vulnerable");
            vuln.SubItems.Add(Npc.Vulnerable);
            vuln.Tag = DetailsField.Vulnerable;

            var immune = DetailsList.Items.Add("Immune");
            immune.SubItems.Add(Npc.Immune);
            immune.Tag = DetailsField.Immune;

            var align = DetailsList.Items.Add("Alignment");
            align.SubItems.Add(Npc.Alignment);
            align.Tag = DetailsField.Alignment;

            var lang = DetailsList.Items.Add("Languages");
            lang.SubItems.Add(Npc.Languages);
            lang.Tag = DetailsField.Languages;

            var skills = DetailsList.Items.Add("Skills");
            skills.SubItems.Add(Npc.Skills);
            skills.Tag = DetailsField.Skills;

            var equip = DetailsList.Items.Add("Equipment");
            equip.SubItems.Add(Npc.Equipment);
            equip.Tag = DetailsField.Equipment;

            var desc = DetailsList.Items.Add("Description");
            desc.SubItems.Add(Npc.Details);
            desc.Tag = DetailsField.Description;

            var tactics = DetailsList.Items.Add("Tactics");
            tactics.SubItems.Add(Npc.Tactics);
            tactics.Tag = DetailsField.Tactics;
        }

        private void SelectPowerBtn_Click(object sender, EventArgs e)
        {
            var dlg = new PowerBrowserForm(NameBox.Text, (int)LevelBox.Value, Npc.Role, add_power);
            dlg.ShowDialog();
        }

        private void add_power(CreaturePower power)
        {
            Npc.CreaturePowers.Add(power);
            update_powers_list();
        }

        private void PortraitBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.ImageFilter;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Npc.Image = Image.FromFile(dlg.FileName);
                image_changed();
            }
        }

        private void PortraitClear_Click(object sender, EventArgs e)
        {
            Npc.Image = null;
            image_changed();
        }

        private void image_changed()
        {
            PortraitBox.Image = Npc.Image;
        }

        private void InfoBtn_Click(object sender, EventArgs e)
        {
            var dlg = new CreatureClassForm(Npc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Npc.Size = dlg.CreatureSize;
                Npc.Origin = dlg.Origin;
                Npc.Type = dlg.Type;
                Npc.Keywords = dlg.Keywords;

                InfoBtn.Text = Npc.Phenotype;
            }
        }

        private void PowerUpBtn_Click(object sender, EventArgs e)
        {
            var index = Npc.CreaturePowers.IndexOf(SelectedPower);
            var tmp = Npc.CreaturePowers[index - 1];

            Npc.CreaturePowers[index - 1] = SelectedPower;
            Npc.CreaturePowers[index] = tmp;

            update_powers_list();
            PowerList.Items[index - 1].Selected = true;
        }

        private void PowerDownBtn_Click(object sender, EventArgs e)
        {
            var index = Npc.CreaturePowers.IndexOf(SelectedPower);
            var tmp = Npc.CreaturePowers[index + 1];

            Npc.CreaturePowers[index + 1] = SelectedPower;
            Npc.CreaturePowers[index] = tmp;

            update_powers_list();
            PowerList.Items[index + 1].Selected = true;
        }

        private void RegenBtn_Click(object sender, EventArgs e)
        {
            var regen = Npc.Regeneration;
            if (regen == null)
                regen = new Regeneration();

            var dlg = new RegenerationForm(regen);
            if (dlg.ShowDialog() == DialogResult.OK)
                Npc.Regeneration = dlg.Regeneration;
        }

        private void ClearRegenLbl_Click(object sender, EventArgs e)
        {
            Npc.Regeneration = null;
        }
    }
}
