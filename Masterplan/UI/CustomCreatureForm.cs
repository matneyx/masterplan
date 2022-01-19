using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CustomCreatureForm : Form
    {
        private readonly bool _fUpdating;

        public CustomCreature Creature { get; }

        public CreaturePower SelectedPower
        {
            get
            {
                if (PowerList.SelectedItems.Count != 0)
                    return PowerList.SelectedItems[0].Tag as CreaturePower;

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

        public DamageModifier SelectedDamageMod
        {
            get
            {
                if (DamageList.SelectedItems.Count != 0)
                    return DamageList.SelectedItems[0].Tag as DamageModifier;

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

        public CustomCreatureForm(CustomCreature cc, bool unused)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Creature = cc.Copy();

            _fUpdating = true;

            NameBox.Text = Creature.Name;
            LevelBox.Value = Creature.Level;
            RoleBtn.Text = Creature.Role.ToString();
            InfoBtn.Text = Creature.Phenotype;

            StrBox.Value = Creature.Strength.Score;
            ConBox.Value = Creature.Constitution.Score;
            DexBox.Value = Creature.Dexterity.Score;
            IntBox.Value = Creature.Intelligence.Score;
            WisBox.Value = Creature.Wisdom.Score;
            ChaBox.Value = Creature.Charisma.Score;

            InitModBox.Value = Creature.InitiativeModifier;
            HPModBox.Value = Creature.HpModifier;
            ACModBox.Value = Creature.AcModifier;
            FortModBox.Value = Creature.FortitudeModifier;
            RefModBox.Value = Creature.ReflexModifier;
            WillModBox.Value = Creature.WillModifier;

            _fUpdating = false;

            update_fields();
            update_powers_list();
            update_aura_list();
            update_damage_list();
            update_details();

            if (Creature.Image != null)
                PortraitBox.Image = Creature.Image;
        }

        ~CustomCreatureForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            PowerRemoveBtn.Enabled = SelectedPower != null;
            PowerEditBtn.Enabled = SelectedPower != null;

            PowerUpBtn.Enabled = SelectedPower != null && Creature.CreaturePowers.IndexOf(SelectedPower) != 0;
            PowerDownBtn.Enabled = SelectedPower != null &&
                                   Creature.CreaturePowers.IndexOf(SelectedPower) != Creature.CreaturePowers.Count - 1;

            AuraRemoveBtn.Enabled = SelectedAura != null;
            AuraEditBtn.Enabled = SelectedAura != null;

            RemoveDmgBtn.Enabled = SelectedDamageMod != null;
            EditDmgBtn.Enabled = SelectedDamageMod != null;

            ClearRegenLbl.Visible = Creature.Regeneration != null;

            DetailsEditBtn.Enabled = SelectedField != DetailsField.None;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Creature.Name = NameBox.Text;
        }

        private void ParameterChanged(object sender, EventArgs e)
        {
            update_fields();
        }

        private void RoleBtn_Click(object sender, EventArgs e)
        {
            var dlg = new RoleForm(Creature.Role, ThreatType.Creature);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Creature.Role = dlg.Role;
                RoleBtn.Text = Creature.Role.ToString();

                update_fields();
            }
        }

        private void update_fields()
        {
            if (_fUpdating)
                return;

            Creature.Level = (int)LevelBox.Value;

            Creature.Strength.Score = (int)StrBox.Value;
            Creature.Constitution.Score = (int)ConBox.Value;
            Creature.Dexterity.Score = (int)DexBox.Value;
            Creature.Intelligence.Score = (int)IntBox.Value;
            Creature.Wisdom.Score = (int)WisBox.Value;
            Creature.Charisma.Score = (int)ChaBox.Value;

            Creature.InitiativeModifier = (int)InitModBox.Value;
            Creature.HpModifier = (int)HPModBox.Value;
            Creature.AcModifier = (int)ACModBox.Value;
            Creature.FortitudeModifier = (int)FortModBox.Value;
            Creature.ReflexModifier = (int)RefModBox.Value;
            Creature.WillModifier = (int)WillModBox.Value;

            var level = Creature.Level / 2;

            var strength = Creature.Strength.Modifier;
            StrModBox.Text = strength + " / " + (strength + level);

            var constitution = Creature.Constitution.Modifier;
            ConModBox.Text = constitution + " / " + (constitution + level);

            var dexterity = Creature.Dexterity.Modifier;
            DexModBox.Text = dexterity + " / " + (dexterity + level);

            var intelligence = Creature.Intelligence.Modifier;
            IntModBox.Text = intelligence + " / " + (intelligence + level);

            var wisdom = Creature.Wisdom.Modifier;
            WisModBox.Text = wisdom + " / " + (wisdom + level);

            var charisma = Creature.Charisma.Modifier;
            ChaModBox.Text = charisma + " / " + (charisma + level);

            InitBox.Text = Creature.Initiative.ToString();
            HPBox.Text = Creature.Hp.ToString();
            ACBox.Text = Creature.Ac.ToString();
            FortBox.Text = Creature.Fortitude.ToString();
            RefBox.Text = Creature.Reflex.ToString();
            WillBox.Text = Creature.Will.ToString();
        }

        private void PowerAddBtn_Click(object sender, EventArgs e)
        {
            var p = new CreaturePower();
            p.Name = "New Power";

            var dlg = new PowerBuilderForm(p, Creature, false);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Creature.CreaturePowers.Add(dlg.Power);
                update_powers_list();
            }
        }

        private void PowerRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPower != null)
            {
                Creature.CreaturePowers.Remove(SelectedPower);
                update_powers_list();
            }
        }

        private void PowerEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPower != null)
            {
                var index = Creature.CreaturePowers.IndexOf(SelectedPower);

                var dlg = new PowerBuilderForm(SelectedPower, Creature, false);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Creature.CreaturePowers[index] = dlg.Power;
                    update_powers_list();
                }
            }
        }

        private void update_powers_list()
        {
            PowerList.Items.Clear();
            foreach (var p in Creature.CreaturePowers)
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

        private void AuraAddBtn_Click(object sender, EventArgs e)
        {
            var a = new Aura();
            a.Name = "New Aura";

            var dlg = new AuraForm(a);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Creature.Auras.Add(dlg.Aura);
                update_aura_list();
            }
        }

        private void AuraRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedAura != null)
            {
                Creature.Auras.Remove(SelectedAura);
                update_aura_list();
            }
        }

        private void AuraEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedAura != null)
            {
                var index = Creature.Auras.IndexOf(SelectedAura);

                var dlg = new AuraForm(SelectedAura);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Creature.Auras[index] = dlg.Aura;
                    update_aura_list();
                }
            }
        }

        private void update_aura_list()
        {
            AuraList.Items.Clear();
            foreach (var a in Creature.Auras)
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

        private void AddDmgBtn_Click(object sender, EventArgs e)
        {
            var dm = new DamageModifier();

            var dlg = new DamageModifierForm(dm);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Creature.DamageModifiers.Add(dlg.Modifier);
                update_damage_list();
            }
        }

        private void RemoveDmgBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDamageMod != null)
            {
                Creature.DamageModifiers.Remove(SelectedDamageMod);
                update_damage_list();
            }
        }

        private void EditDmgBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDamageMod != null)
            {
                var index = Creature.DamageModifiers.IndexOf(SelectedDamageMod);

                var dlg = new DamageModifierForm(SelectedDamageMod);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Creature.DamageModifiers[index] = dlg.Modifier;
                    update_damage_list();
                }
            }
        }

        private void update_damage_list()
        {
            DamageList.Items.Clear();
            DamageList.ShowGroups = true;

            foreach (var dm in Creature.DamageModifiers)
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

            if (Creature.DamageModifiers.Count == 0)
            {
                DamageList.ShowGroups = false;

                var lvi = DamageList.Items.Add("(none)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void DetailsEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedField != DetailsField.None)
            {
                var dlg = new DetailsForm(Creature, SelectedField, null);
                if (dlg.ShowDialog() == DialogResult.OK)
                    update_details();
            }
        }

        private void update_details()
        {
            DetailsList.Items.Clear();

            var senses = DetailsList.Items.Add("Senses");
            senses.SubItems.Add(Creature.Senses);
            senses.Tag = DetailsField.Senses;

            var move = DetailsList.Items.Add("Movement");
            move.SubItems.Add(Creature.Movement);
            move.Tag = DetailsField.Movement;

            var resist = DetailsList.Items.Add("Resist");
            resist.SubItems.Add(Creature.Resist);
            resist.Tag = DetailsField.Resist;

            var vuln = DetailsList.Items.Add("Vulnerable");
            vuln.SubItems.Add(Creature.Vulnerable);
            vuln.Tag = DetailsField.Vulnerable;

            var immune = DetailsList.Items.Add("Immune");
            immune.SubItems.Add(Creature.Immune);
            immune.Tag = DetailsField.Immune;

            var align = DetailsList.Items.Add("Alignment");
            align.SubItems.Add(Creature.Alignment);
            align.Tag = DetailsField.Alignment;

            var lang = DetailsList.Items.Add("Languages");
            lang.SubItems.Add(Creature.Languages);
            lang.Tag = DetailsField.Languages;

            var skills = DetailsList.Items.Add("Skills");
            skills.SubItems.Add(Creature.Skills);
            skills.Tag = DetailsField.Skills;

            var equip = DetailsList.Items.Add("Equipment");
            equip.SubItems.Add(Creature.Equipment);
            equip.Tag = DetailsField.Equipment;

            var desc = DetailsList.Items.Add("Description");
            desc.SubItems.Add(Creature.Details);
            desc.Tag = DetailsField.Description;

            var tactics = DetailsList.Items.Add("Tactics");
            tactics.SubItems.Add(Creature.Tactics);
            tactics.Tag = DetailsField.Tactics;
        }

        private void SelectPowerBtn_Click(object sender, EventArgs e)
        {
            var dlg = new PowerBrowserForm(NameBox.Text, (int)LevelBox.Value, Creature.Role, add_power);
            dlg.ShowDialog();
        }

        private void add_power(CreaturePower power)
        {
            Creature.CreaturePowers.Add(power);
            update_powers_list();
        }

        private void PortraitBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.ImageFilter;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Creature.Image = Image.FromFile(dlg.FileName);
                image_changed();
            }
        }

        private void PortraitClear_Click(object sender, EventArgs e)
        {
            Creature.Image = null;
            image_changed();
        }

        private void image_changed()
        {
            PortraitBox.Image = Creature.Image;
        }

        private void InfoBtn_Click(object sender, EventArgs e)
        {
            var dlg = new CreatureClassForm(Creature);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Creature.Size = dlg.CreatureSize;
                Creature.Origin = dlg.Origin;
                Creature.Type = dlg.Type;
                Creature.Keywords = dlg.Keywords;

                InfoBtn.Text = Creature.Phenotype;
            }
        }

        private void PowerUpBtn_Click(object sender, EventArgs e)
        {
            var index = Creature.CreaturePowers.IndexOf(SelectedPower);
            var tmp = Creature.CreaturePowers[index - 1];

            Creature.CreaturePowers[index - 1] = SelectedPower;
            Creature.CreaturePowers[index] = tmp;

            update_powers_list();
            PowerList.Items[index - 1].Selected = true;
        }

        private void PowerDownBtn_Click(object sender, EventArgs e)
        {
            var index = Creature.CreaturePowers.IndexOf(SelectedPower);
            var tmp = Creature.CreaturePowers[index + 1];

            Creature.CreaturePowers[index + 1] = SelectedPower;
            Creature.CreaturePowers[index] = tmp;

            update_powers_list();
            PowerList.Items[index + 1].Selected = true;
        }

        private void RegenBtn_Click(object sender, EventArgs e)
        {
            var regen = Creature.Regeneration;
            if (regen == null)
                regen = new Regeneration();

            var dlg = new RegenerationForm(regen);
            if (dlg.ShowDialog() == DialogResult.OK)
                Creature.Regeneration = dlg.Regeneration;
        }

        private void ClearRegenLbl_Click(object sender, EventArgs e)
        {
            Creature.Regeneration = null;
        }
    }
}
