using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.Tools.Generators;
using Masterplan.Tools.IO;
using Masterplan.Wizards;

namespace Masterplan.UI
{
    internal partial class CreatureBuilderForm : Form
    {
        private const int SamplePowers = 5;

        private readonly List<CreaturePower> _samplePowers = new List<CreaturePower>();

        private SidebarType _sidebarType = SidebarType.Advice;

        public ICreature Creature { get; private set; }

        public CreatureBuilderForm(ICreature creature)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            if (creature is Creature)
            {
                var c = creature as Creature;
                Creature = c.Copy();
            }

            if (creature is CustomCreature)
            {
                var cc = creature as CustomCreature;
                Creature = cc.Copy();
            }

            if (creature is Npc)
            {
                var npc = creature as Npc;
                Creature = npc.Copy();

                OptionsImport.Enabled = false;
                OptionsVariant.Enabled = false;
            }

            if (Session.Project == null)
            {
                Pages.TabPages.Remove(EntryPage);
                OptionsEntry.Enabled = false;
            }
            else
            {
                update_entry();
            }

            find_sample_powers();

            update_view();
        }

        ~CreatureBuilderForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            PicturePasteBtn.Enabled = Clipboard.ContainsImage();
            PictureClearBtn.Enabled = Creature.Image != null;

            AdviceBtn.Checked = _sidebarType == SidebarType.Advice;
            PowersBtn.Checked = _sidebarType == SidebarType.Powers;
            PreviewBtn.Checked = _sidebarType == SidebarType.Preview;

            LevelDownBtn.Enabled = Creature.Level > 1;
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "build")
            {
                if (e.Url.LocalPath == "profile")
                {
                    e.Cancel = true;

                    var level = Creature.Level;
                    var role = Creature.Role.ToString();

                    var dlg = new CreatureProfileForm(Creature);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        if (Creature.Level != level || Creature.Role.ToString() != role)
                        {
                            var msg = "You've changed this creature's level and/or role.";
                            msg += Environment.NewLine;
                            msg += "Do you want to update its combat statistics to match?";
                            if (MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                                DialogResult.Yes)
                            {
                                // HP
                                if (Creature.Role is ComplexRole)
                                    Creature.Hp = Statistics.Hp(Creature.Level, Creature.Role as ComplexRole,
                                        Creature.Constitution.Score);
                                else
                                    Creature.Hp = 1;

                                // Init
                                Creature.Initiative = Statistics.Initiative(Creature.Level, Creature.Role);

                                // AC
                                Creature.Ac = Statistics.Ac(Creature.Level, Creature.Role);

                                // Fort / Ref / Will
                                Creature.Fortitude = Statistics.Nad(Creature.Level, Creature.Role);
                                Creature.Reflex = Statistics.Nad(Creature.Level, Creature.Role);
                                Creature.Will = Statistics.Nad(Creature.Level, Creature.Role);
                            }
                        }

                        find_sample_powers();
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "combat")
                {
                    e.Cancel = true;

                    var dlg = new CreatureStatsForm(Creature);
                    if (dlg.ShowDialog() == DialogResult.OK) update_statblock();
                }

                if (e.Url.LocalPath == "ability")
                {
                    e.Cancel = true;

                    var dlg = new CreatureAbilityForm(Creature);
                    if (dlg.ShowDialog() == DialogResult.OK) update_statblock();
                }

                if (e.Url.LocalPath == "damage")
                {
                    e.Cancel = true;

                    var dlg = new DamageModListForm(Creature);
                    if (dlg.ShowDialog() == DialogResult.OK) update_statblock();
                }

                if (e.Url.LocalPath == "senses")
                {
                    e.Cancel = true;

                    var hint = "Note: Do not add Perception here; it should be entered as a skill.";
                    var dlg = new DetailsForm(Creature, DetailsField.Senses, hint);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.Senses = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "movement")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Creature, DetailsField.Movement, null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.Movement = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "alignment")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Creature, DetailsField.Alignment, null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.Alignment = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "languages")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Creature, DetailsField.Languages, null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.Languages = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "skills")
                {
                    e.Cancel = true;

                    var dlg = new CreatureSkillsForm(Creature);
                    if (dlg.ShowDialog() == DialogResult.OK)
                        update_statblock();
                }

                if (e.Url.LocalPath == "equipment")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Creature, DetailsField.Equipment, null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.Equipment = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "tactics")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Creature, DetailsField.Tactics, null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.Tactics = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "import")
                {
                    e.Cancel = true;

                    import_creature();
                }

                if (e.Url.LocalPath == "variant")
                {
                    e.Cancel = true;

                    create_variant();
                }

                if (e.Url.LocalPath == "random")
                {
                    e.Cancel = true;

                    create_random();
                }

                if (e.Url.LocalPath == "hybrid")
                {
                    e.Cancel = true;

                    create_hybrid();
                }

                if (e.Url.LocalPath == "name")
                {
                    e.Cancel = true;

                    var originalName = Creature.Name;
                    Creature.Name = generate_name();

                    for (var n = 0; n != Creature.CreaturePowers.Count; ++n)
                    {
                        var cp = Creature.CreaturePowers[n];
                        cp = replace_name(cp, originalName, "", Creature.Name);
                        Creature.CreaturePowers[n] = cp;
                    }

                    for (var n = 0; n != _samplePowers.Count; ++n)
                    {
                        var cp = _samplePowers[n];
                        cp = replace_name(cp, originalName, "", Creature.Name);
                        _samplePowers[n] = cp;
                    }

                    update_statblock();
                }

                if (e.Url.LocalPath == "template")
                {
                    e.Cancel = true;

                    // Select and apply a template
                    var dlg = new CreatureTemplateSelectForm();
                    if (dlg.ShowDialog() == DialogResult.OK)
                        if (dlg.Template != null)
                        {
                            var card = new EncounterCard(Creature);
                            card.TemplateIDs.Add(dlg.Template.Id);

                            ICreature custom = null;

                            if (Creature is Creature)
                                custom = new Creature();

                            if (Creature is CustomCreature)
                                custom = new CustomCreature();

                            if (Creature is Npc)
                                custom = new Npc();

                            custom.Name = card.Title;
                            custom.Level = card.Level;

                            custom.Senses = card.Senses;
                            custom.Movement = card.Movement;
                            custom.Resist = card.Resist;
                            custom.Vulnerable = card.Vulnerable;
                            custom.Immune = card.Immune;

                            // Role
                            var cr = new ComplexRole();
                            cr.Leader = card.Leader;
                            cr.Flag = card.Flag;
                            cr.Type = card.Roles[0];
                            custom.Role = cr;

                            // Combat stats
                            custom.Initiative = card.Initiative;
                            custom.Hp = card.Hp;
                            custom.Ac = card.Ac;
                            custom.Fortitude = card.Fortitude;
                            custom.Reflex = card.Reflex;
                            custom.Will = card.Will;

                            // Regeneration
                            custom.Regeneration = card.Regeneration;

                            // Auras
                            var auras = card.Auras;
                            foreach (var aura in auras)
                                custom.Auras.Add(aura.Copy());

                            // Add powers
                            var powers = card.CreaturePowers;
                            foreach (var power in powers)
                                custom.CreaturePowers.Add(power.Copy());

                            // Add damage modifiers
                            var mods = card.DamageModifiers;
                            foreach (var mod in mods)
                                custom.DamageModifiers.Add(mod.Copy());

                            var id = Creature.Id;
                            CreatureHelper.CopyFields(custom, Creature);
                            Creature.Id = id;

                            find_sample_powers();
                            update_view();
                        }
                }
            }

            if (e.Url.Scheme == "power")
            {
                if (e.Url.LocalPath == "addpower")
                {
                    e.Cancel = true;

                    var pwr = new CreaturePower();
                    pwr.Name = "New Power";
                    pwr.Action = new PowerAction();

                    var dlg = new PowerBuilderForm(pwr, Creature, false);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.CreaturePowers.Add(dlg.Power);
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "addtrait")
                {
                    e.Cancel = true;

                    var pwr = new CreaturePower();
                    pwr.Name = "New Trait";
                    pwr.Action = null;

                    var dlg = new PowerBuilderForm(pwr, Creature, false);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.CreaturePowers.Add(dlg.Power);
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "addaura")
                {
                    e.Cancel = true;

                    var aura = new Aura();
                    aura.Name = "New Aura";
                    aura.Details = "1";

                    var dlg = new AuraForm(aura);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.Auras.Add(dlg.Aura);
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "browse")
                {
                    e.Cancel = true;

                    OptionsPowerBrowser_Click(null, null);
                }

                if (e.Url.LocalPath == "statistics")
                {
                    e.Cancel = true;

                    var creatures = find_matching_creatures(Creature.Role, Creature.Level, true);
                    var powers = new List<CreaturePower>();
                    foreach (var c in creatures)
                        powers.AddRange(c.CreaturePowers);
                    var dlg = new PowerStatisticsForm(powers, creatures.Count);
                    dlg.ShowDialog();
                }

                if (e.Url.LocalPath == "regenedit")
                {
                    e.Cancel = true;

                    var regen = Creature.Regeneration;
                    if (regen == null)
                        regen = new Regeneration(5, "");

                    var dlg = new RegenerationForm(regen);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.Regeneration = dlg.Regeneration;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "regenremove")
                {
                    e.Cancel = true;

                    Creature.Regeneration = null;
                    update_statblock();
                }

                if (e.Url.LocalPath == "refresh")
                {
                    e.Cancel = true;

                    find_sample_powers();

                    update_statblock();
                }
            }

            if (e.Url.Scheme == "poweredit")
            {
                var pwr = find_power(new Guid(e.Url.LocalPath));
                if (pwr != null)
                {
                    e.Cancel = true;
                    var index = Creature.CreaturePowers.IndexOf(pwr);

                    var dlg = new PowerBuilderForm(pwr, Creature, false);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.CreaturePowers[index] = dlg.Power;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "powerremove")
            {
                var pwr = find_power(new Guid(e.Url.LocalPath));
                if (pwr != null)
                {
                    e.Cancel = true;

                    Creature.CreaturePowers.Remove(pwr);
                    update_statblock();
                }
            }

            if (e.Url.Scheme == "powerduplicate")
            {
                var pwr = find_power(new Guid(e.Url.LocalPath));
                if (pwr != null)
                {
                    e.Cancel = true;

                    var index = Creature.CreaturePowers.IndexOf(pwr);

                    pwr = pwr.Copy();
                    pwr.Id = Guid.NewGuid();

                    Creature.CreaturePowers.Insert(index, pwr);
                    update_statblock();
                }
            }

            if (e.Url.Scheme == "auraedit")
            {
                var aura = find_aura(new Guid(e.Url.LocalPath));
                if (aura != null)
                {
                    e.Cancel = true;
                    var index = Creature.Auras.IndexOf(aura);

                    var dlg = new AuraForm(aura);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Creature.Auras[index] = dlg.Aura;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "auraremove")
            {
                var aura = find_aura(new Guid(e.Url.LocalPath));
                if (aura != null)
                {
                    e.Cancel = true;

                    Creature.Auras.Remove(aura);
                    update_statblock();
                }
            }

            if (e.Url.Scheme == "samplepower")
            {
                var power = find_sample_power(new Guid(e.Url.LocalPath));
                if (power != null)
                {
                    e.Cancel = true;

                    Creature.CreaturePowers.Add(power);

                    _samplePowers.Remove(power);
                    if (_samplePowers.Count == 0)
                        find_sample_powers();

                    update_statblock();
                }
            }
        }

        private void OptionsImport_Click(object sender, EventArgs e)
        {
            import_creature();
        }

        private void FileExport_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Title = "Export Creature";
            dlg.FileName = Creature.Name;
            dlg.Filter = Program.CreatureFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var c = new Creature(Creature);
                var ok = Serialisation<Creature>.Save(dlg.FileName, c, SerialisationMode.Binary);

                if (!ok)
                {
                    var error = "The creature could not be exported.";
                    MessageBox.Show(error, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OptionsVariant_Click(object sender, EventArgs e)
        {
            create_variant();
        }

        private void OptionsRandom_Click(object sender, EventArgs e)
        {
            create_random();
        }

        private void OptionsEntry_Click(object sender, EventArgs e)
        {
            var entry = Session.Project.Encyclopedia.FindEntryForAttachment(Creature.Id);

            if (entry == null)
            {
                // If there is no entry, ask to create it
                var msg = "There is no encyclopedia entry associated with this creature.";
                msg += Environment.NewLine;
                msg += "Would you like to create one now?";
                if (MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                    DialogResult.No)
                    return;

                entry = new EncyclopediaEntry();
                entry.Name = Creature.Name;
                entry.AttachmentId = Creature.Id;
                entry.Category = "Creatures";

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

                update_entry();
            }
        }

        private void OptionsPowerBrowser_Click(object sender, EventArgs e)
        {
            var dlg = new PowerBrowserForm(Creature.Name, Creature.Level, Creature.Role, add_power);
            dlg.ShowDialog();
        }

        private void AdviceBtn_Click(object sender, EventArgs e)
        {
            if (_sidebarType != SidebarType.Advice)
            {
                _sidebarType = SidebarType.Advice;
                update_statblock();
            }
        }

        private void PowersBtn_Click(object sender, EventArgs e)
        {
            if (_sidebarType != SidebarType.Powers)
            {
                _sidebarType = SidebarType.Powers;
                update_statblock();
            }
        }

        private void PreviewBtn_Click(object sender, EventArgs e)
        {
            if (_sidebarType != SidebarType.Preview)
            {
                _sidebarType = SidebarType.Preview;
                update_statblock();
            }
        }

        private void LevelUpBtn_Click(object sender, EventArgs e)
        {
            CreatureHelper.AdjustCreatureLevel(Creature, +1);
            find_sample_powers();
            update_statblock();
        }

        private void LevelDownBtn_Click(object sender, EventArgs e)
        {
            CreatureHelper.AdjustCreatureLevel(Creature, -1);
            find_sample_powers();
            update_statblock();
        }

        private void PictureBrowseBtn_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.ImageFilter;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Creature.Image = Image.FromFile(dlg.FileName);
                Program.SetResolution(Creature.Image);
                update_picture();
            }
        }

        private void PicturePasteBtn_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Creature.Image = Clipboard.GetImage();
                Program.SetResolution(Creature.Image);
                update_picture();
            }
        }

        private void PictureClearBtn_Click(object sender, EventArgs e)
        {
            Creature.Image = null;
            update_picture();
        }

        private void update_view()
        {
            update_statblock();
            update_picture();
        }

        private void update_statblock()
        {
            var card = new EncounterCard(Creature);

            var lines = Html.GetHead("Creature", "", Session.Preferences.TextSize);
            lines.Add("<BODY>");

            if (_sidebarType != SidebarType.Preview)
            {
                lines.Add("<TABLE class=clear>");
                lines.Add("<TR class=clear>");
                lines.Add("<TD class=clear>");
            }

            lines.Add("<P class=table>");
            lines.AddRange(
                card.AsText(null, _sidebarType != SidebarType.Preview ? CardMode.Build : CardMode.View, true));
            lines.Add("</P>");

            if (_sidebarType != SidebarType.Preview)
            {
                lines.Add("</TD>");

                switch (_sidebarType)
                {
                    case SidebarType.Advice:
                    {
                        lines.Add("<TD class=clear>");
                        lines.Add("<P class=table>");

                        var hybrid = Session.Creatures.Count >= 2;

                        lines.Add("<P class=table>");
                        lines.Add("<TABLE>");
                        lines.Add("<TR class=heading>");
                        lines.Add("<TD><B>Create A New Creature</B></TD>");
                        lines.Add("</TR>");
                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add("Import a <A href=build:import>creature file</A> from Adventure Tools");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add("Create a <A href=build:variant>variant</A> of an existing creature");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add("Generate a <A href=build:random>random creature</A>");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                        if (hybrid)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD>");
                            lines.Add("Generate a <A href=build:hybrid>hybrid creature</A>");
                            lines.Add("</TD>");
                            lines.Add("</TR>");
                        }

                        lines.Add("</TABLE>");
                        lines.Add("</P>");

                        var template = false;
                        var cr = Creature.Role as ComplexRole;
                        if (cr != null)
                            template = cr.Flag != RoleFlag.Solo;

                        lines.Add("<P class=table>");
                        lines.Add("<TABLE>");
                        lines.Add("<TR class=heading>");
                        lines.Add("<TD><B>Modify This Creature</B></TD>");
                        lines.Add("</TR>");
                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add("Generate a <A href=build:name>new name</A> for this creature");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add("Browse for <A href=power:browse>existing powers</A> for this creature");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                        if (template)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD>");
                            lines.Add("Apply a <A href=build:template>template</A> to this creature");
                            lines.Add("</TD>");
                        }

                        lines.Add("</TR>");
                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add("See <A href=power:statistics>power statistics</A> for other creatures of this type");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                        lines.Add("</TABLE>");
                        lines.Add("</P>");

                        lines.Add("<P class=table>");
                        lines.Add("<TABLE>");

                        lines.Add("<TR class=heading>");
                        lines.Add("<TD colspan=2><B>Creature Advice</B></TD>");
                        lines.Add("</TR>");

                        var init = Statistics.Initiative(Creature.Level, Creature.Role);
                        var ac = Statistics.Ac(Creature.Level, Creature.Role);
                        var nad = Statistics.Nad(Creature.Level, Creature.Role);

                        var minion = Creature.Role is Minion;
                        if (!minion)
                        {
                            _ = minion
                                ? 1
                                : Statistics.Hp(Creature.Level, Creature.Role as ComplexRole,
                                    Creature.Constitution.Score);

                            lines.Add("<TR>");
                            lines.Add("<TD>Hit Points</TD>");
                            lines.Add("<TD align=center>+" +
                                      Statistics.AttackBonus(DefenceType.Ac, Creature.Level, Creature.Role) + "</TD>");
                            lines.Add("</TR>");
                        }

                        lines.Add("<TR>");
                        lines.Add("<TD>Initiative Bonus</TD>");
                        lines.Add("<TD align=center>+" + init + "</TD>");
                        lines.Add("</TR>");

                        lines.Add("<TR>");
                        lines.Add("<TD>Armour Class</TD>");
                        lines.Add("<TD align=center>+" + ac + "</TD>");
                        lines.Add("</TR>");

                        lines.Add("<TR>");
                        lines.Add("<TD>Other Defences</TD>");
                        lines.Add("<TD align=center>+" + nad + "</TD>");
                        lines.Add("</TR>");

                        lines.Add("<TR>");
                        lines.Add("<TD>Number of Powers</TD>");
                        lines.Add("<TD align=center>4 - 6</TD>");
                        lines.Add("</TR>");

                        lines.Add("</TABLE>");
                        lines.Add("</P>");

                        lines.Add("</TD>");
                    }
                        break;
                    case SidebarType.Powers:
                    {
                        lines.Add("<TD class=clear>");
                        lines.Add("<P class=table>");

                        if (_samplePowers.Count != 0)
                        {
                            lines.Add("<P text-align=left>");
                            lines.Add("The following powers might be suitable for this creature.");
                            lines.Add(
                                "Click <A href=power:refresh>here</A> to resample this list, or <A href=power:browse>here</A> to look for other powers.");
                            lines.Add("</P>");

                            lines.Add("<P class=table>");
                            lines.Add("<TABLE>");

                            var powers = new Dictionary<CreaturePowerCategory, List<CreaturePower>>();

                            var powerCategories = Enum.GetValues(typeof(CreaturePowerCategory));
                            foreach (CreaturePowerCategory cat in powerCategories)
                                powers[cat] = new List<CreaturePower>();

                            foreach (var cp in _samplePowers)
                                powers[cp.Category].Add(cp);

                            foreach (CreaturePowerCategory cat in powerCategories)
                            {
                                var count = powers[cat].Count;
                                if (count == 0)
                                    continue;

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

                                lines.Add("<TR class=creature>");
                                lines.Add("<TD colspan=3>");
                                lines.Add("<B>" + name + "</B>");
                                lines.Add("</TD>");
                                lines.Add("</TR>");

                                foreach (var cp in powers[cat])
                                {
                                    lines.AddRange(cp.AsHtml(null, CardMode.View, false));

                                    lines.Add("<TR>");
                                    lines.Add("<TD colspan=3 align=center>");
                                    lines.Add("<A href=samplepower:" + cp.Id + ">copy this power into " +
                                              Creature.Name + "</A>");
                                    lines.Add("</TD>");
                                    lines.Add("</TR>");
                                }
                            }

                            lines.Add("</TABLE>");
                            lines.Add("</P>");
                        }

                        lines.Add("</TD>");
                    }
                        break;
                }

                lines.Add("</TR>");
                lines.Add("</TABLE>");
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            StatBlockBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void update_picture()
        {
            PortraitBox.Image = Creature.Image;
        }

        private void update_entry()
        {
            var entry = Session.Project.Encyclopedia.FindEntryForAttachment(Creature.Id);
            EntryBrowser.DocumentText = Html.EncyclopediaEntry(entry, Session.Project.Encyclopedia,
                Session.Preferences.TextSize, true, false, false, false);
        }

        private string generate_name()
        {
            var name = ExoticName.SingleName();

            var creatures = find_matching_creatures(Creature.Role, Creature.Level, false);
            var desc = find_description(creatures);
            if (desc != "")
                name = name + " " + desc;

            return name;
        }

        private string find_description(List<ICreature> creatures)
        {
            var endings = new List<string>();
            endings.Add("er");
            endings.Add("ist");

            var names = new List<string>();
            foreach (var c in creatures)
            {
                var tokens = c.Name.Split(null);

                foreach (var token in tokens)
                {
                    var ok = false;
                    foreach (var ending in endings)
                        if (token.EndsWith(ending))
                        {
                            ok = true;
                            break;
                        }

                    if (ok)
                        names.Add(token);
                }
            }

            if (names.Count != 0)
            {
                var index = Session.Random.Next(names.Count);
                return names[index];
            }

            return "";
        }

        private CreaturePower find_power(Guid id)
        {
            foreach (var pwr in Creature.CreaturePowers)
                if (pwr.Id == id)
                    return pwr;

            return null;
        }

        private Aura find_aura(Guid id)
        {
            foreach (var aura in Creature.Auras)
                if (aura.Id == id)
                    return aura;

            return null;
        }

        private void add_power(CreaturePower power)
        {
            Creature.CreaturePowers.Add(power);
            update_statblock();
        }

        private void find_sample_powers()
        {
            var powerNames = new BinarySearchTree<string>();
            var allPowers = new List<CreaturePower>();

            foreach (var power in Creature.CreaturePowers)
                if (power != null)
                    powerNames.Add(power.Name.ToLower());

            var creatures = find_matching_creatures(Creature.Role, Creature.Level, false);
            foreach (var creature in creatures)
            foreach (var power in creature.CreaturePowers)
            {
                if (power == null)
                    continue;

                if (powerNames.Contains(power.Name.ToLower()))
                    continue;

                var cp = replace_name(power, creature.Name, creature.Category, Creature.Name);
                cp = alter_power_level(cp, creature.Level, Creature.Level);
                if (cp != null)
                {
                    allPowers.Add(cp.Copy());
                    powerNames.Add(cp.Name);
                }
            }

            var count = Math.Min(allPowers.Count, SamplePowers);

            _samplePowers.Clear();
            while (_samplePowers.Count != count)
            {
                var index = Session.Random.Next(allPowers.Count);
                var power = allPowers[index];
                if (power != null)
                {
                    _samplePowers.Add(power);
                    allPowers.Remove(power);
                }
            }
        }

        private CreaturePower find_sample_power(Guid id)
        {
            foreach (var power in _samplePowers)
                if (power.Id == id)
                    return power;

            return null;
        }

        private List<ICreature> find_matching_creatures(IRole role, int level, bool exactMatch)
        {
            var list = new List<ICreature>();

            var levelDelta = exactMatch ? 0 : 1;

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

            foreach (var creature in creatures)
            {
                var matchLevel = Math.Abs(creature.Level - level) <= levelDelta;
                var matchRole = creature.Role.ToString() == role.ToString();

                if (matchLevel && matchRole)
                    list.Add(creature);
            }

            return list;
        }

        private CreaturePower replace_name(CreaturePower power, string originalName, string originalCategory,
            string replacement)
        {
            var cp = power.Copy();

            if (!string.IsNullOrEmpty(originalName) && !replacement.Contains(originalName))
            {
                cp.Details = replace_text(cp.Details, originalName, replacement);
                cp.Description = replace_text(cp.Description, originalName, replacement);
                cp.Condition = replace_text(cp.Condition, originalName, replacement);
                cp.Range = replace_text(cp.Range, originalName, replacement);
            }

            if (!string.IsNullOrEmpty(originalCategory) && !replacement.Contains(originalCategory))
            {
                cp.Details = replace_text(cp.Details, originalCategory, replacement);
                cp.Description = replace_text(cp.Description, originalCategory, replacement);
                cp.Condition = replace_text(cp.Condition, originalCategory, replacement);
                cp.Range = replace_text(cp.Range, originalCategory, replacement);
            }

            return cp;
        }

        private string replace_text(string source, string original, string replacement)
        {
            if (source == null || original == null || replacement == null)
                return source;

            // Make sure we don't get into an infinite loop
            if (replacement.Contains(original))
                return source;

            var str = source;

            while (true)
            {
                var index = str.ToLower().IndexOf(original.ToLower());
                if (index == -1)
                    break;

                var prefix = str.Substring(0, index);
                var suffix = str.Substring(index + original.Length);
                str = prefix + replacement.ToLower() + suffix;
            }

            return str;
        }

        private void import_creature()
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.MonsterFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var xml = File.ReadAllText(dlg.FileName);
                var c = AppImport.ImportCreature(xml);
                if (c != null)
                {
                    var id = Creature.Id;
                    CreatureHelper.CopyFields(c, Creature);
                    //fCreature = c;
                    Creature.Id = id;

                    find_sample_powers();
                    update_view();
                }
            }
        }

        private void create_random()
        {
            var dlg = new RandomCreatureForm(Creature.Level, Creature.Role);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var creatures = find_matching_creatures(dlg.Role, dlg.Level, true);
                if (creatures.Count == 0)
                    return;

                Splice(creatures);

                find_sample_powers();
                update_view();
            }
        }

        private void create_variant()
        {
            var wizard = new VariantWizard();
            if (wizard.Show() == DialogResult.OK)
            {
                var data = wizard.Data as VariantData;

                var card = new EncounterCard();
                card.CreatureId = data.BaseCreature.Id;
                foreach (var ct in data.Templates)
                    card.TemplateIDs.Add(ct.Id);

                ICreature custom = null;

                if (Creature is Creature)
                    custom = new Creature();

                if (Creature is CustomCreature)
                    custom = new CustomCreature();

                if (Creature is Npc)
                    custom = new Npc();

                custom.Name = "Variant " + card.Title;
                custom.Details = data.BaseCreature.Details;
                custom.Size = data.BaseCreature.Size;
                custom.Level = card.Level;
                if (data.BaseCreature.Image != null)
                    custom.Image = new Bitmap(data.BaseCreature.Image);

                custom.Senses = card.Senses;
                custom.Movement = card.Movement;
                custom.Resist = card.Resist;
                custom.Vulnerable = card.Vulnerable;
                custom.Immune = card.Immune;

                if (data.BaseCreature.Role is Minion)
                {
                    custom.Role = new Minion();
                }
                else
                {
                    var cr = new ComplexRole();

                    cr.Type = data.Roles[data.SelectedRoleIndex];
                    cr.Flag = card.Flag;
                    cr.Leader = card.Leader;

                    custom.Role = cr;
                }

                // Set ability scores
                custom.Strength.Score = data.BaseCreature.Strength.Score;
                custom.Constitution.Score = data.BaseCreature.Constitution.Score;
                custom.Dexterity.Score = data.BaseCreature.Dexterity.Score;
                custom.Intelligence.Score = data.BaseCreature.Intelligence.Score;
                custom.Wisdom.Score = data.BaseCreature.Wisdom.Score;
                custom.Charisma.Score = data.BaseCreature.Charisma.Score;

                // Combat stats
                custom.Initiative = data.BaseCreature.Initiative;
                custom.Hp = card.Hp;
                custom.Ac = card.Ac;
                custom.Fortitude = card.Fortitude;
                custom.Reflex = card.Reflex;
                custom.Will = card.Will;

                // Regeneration
                custom.Regeneration = card.Regeneration;

                // Auras
                var auras = card.Auras;
                foreach (var aura in auras)
                    custom.Auras.Add(aura.Copy());

                // Add powers
                var powers = card.CreaturePowers;
                foreach (var power in powers)
                    custom.CreaturePowers.Add(power.Copy());

                // Add damage modifiers
                var mods = card.DamageModifiers;
                foreach (var mod in mods)
                    custom.DamageModifiers.Add(mod.Copy());

                var id = Creature.Id;
                CreatureHelper.CopyFields(custom, Creature);
                Creature.Id = id;

                find_sample_powers();
                update_view();
            }
        }

        private void create_hybrid()
        {
            var dlg = new CreatureMultipleSelectForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Splice(dlg.SelectedCreatures);

                find_sample_powers();
                update_view();
            }
        }

        private void Splice(List<ICreature> genepool)
        {
            if (Creature is Creature)
                Creature = new Creature();

            if (Creature is CustomCreature)
                Creature = new CustomCreature();

            if (Creature is Npc)
                Creature = new Npc();

            // Level
            var minLevel = int.MaxValue;
            var maxLevel = int.MinValue;
            foreach (Creature c in genepool)
            {
                minLevel = Math.Min(minLevel, c.Level);
                maxLevel = Math.Max(maxLevel, c.Level);
            }

            Creature.Level = Session.Random.Next(minLevel, maxLevel + 1);

            // Role
            var roleIndex = Session.Random.Next(genepool.Count);
            Creature.Role = genepool[roleIndex].Role.Copy();

            // Phenotype
            var sizeIndex = Session.Random.Next(genepool.Count);
            Creature.Size = genepool[sizeIndex].Size;
            var originIndex = Session.Random.Next(genepool.Count);
            Creature.Origin = genepool[originIndex].Origin;
            var typeIndex = Session.Random.Next(genepool.Count);
            Creature.Type = genepool[typeIndex].Type;

            // Name and category
            var commonName = find_common_name(genepool);
            if (commonName == "")
            {
                Creature.Name = generate_name();
            }
            else
            {
                Creature.Name = commonName;

                var desc = find_description(genepool);
                if (desc != "")
                    Creature.Name += " " + desc;
                else
                    Creature.Name = "New " + Creature.Name;
            }

            Creature.Category = "";

            // Keywords
            var keywordIndex = Session.Random.Next(genepool.Count);
            Creature.Keywords = genepool[keywordIndex].Keywords;

            // Ability scores
            var scoreIndex = Session.Random.Next(genepool.Count);
            Creature.Strength.Score = genepool[scoreIndex].Strength.Score;
            Creature.Constitution.Score = genepool[scoreIndex].Constitution.Score;
            Creature.Dexterity.Score = genepool[scoreIndex].Dexterity.Score;
            Creature.Intelligence.Score = genepool[scoreIndex].Intelligence.Score;
            Creature.Wisdom.Score = genepool[scoreIndex].Wisdom.Score;
            Creature.Charisma.Score = genepool[scoreIndex].Charisma.Score;

            // Defences
            var defIndex = Session.Random.Next(genepool.Count);
            var expectedAc = Statistics.Ac(genepool[defIndex].Level, genepool[defIndex].Role);
            var expectedNad = Statistics.Nad(genepool[defIndex].Level, genepool[defIndex].Role);
            var observedAc = genepool[defIndex].Ac;
            var observedFort = genepool[defIndex].Fortitude;
            var observedRef = genepool[defIndex].Reflex;
            var observedWill = genepool[defIndex].Will;
            var bonusAc = observedAc - expectedAc;
            var bonusFort = observedFort - expectedNad;
            var bonusRef = observedRef - expectedNad;
            var bonusWill = observedWill - expectedNad;
            Creature.Ac = Statistics.Ac(Creature.Level, Creature.Role) + bonusAc;
            Creature.Fortitude = Statistics.Nad(Creature.Level, Creature.Role) + bonusFort;
            Creature.Reflex = Statistics.Nad(Creature.Level, Creature.Role) + bonusRef;
            Creature.Will = Statistics.Nad(Creature.Level, Creature.Role) + bonusWill;

            // HP
            if (Creature.Role is ComplexRole)
            {
                var nonMinions = new List<ICreature>();
                foreach (var c in genepool)
                    if (c.Role is ComplexRole)
                        nonMinions.Add(c);
                var hpIndex = Session.Random.Next(nonMinions.Count);

                var expectedHp = Statistics.Hp(nonMinions[hpIndex].Level, nonMinions[hpIndex].Role as ComplexRole,
                    nonMinions[hpIndex].Constitution.Score);
                var observedHp = nonMinions[hpIndex].Hp;
                var bonusHp = observedHp - expectedHp;
                Creature.Hp = Statistics.Hp(Creature.Level, Creature.Role as ComplexRole, Creature.Constitution.Score) +
                              bonusHp;
            }

            // Init
            var initIndex = Session.Random.Next(genepool.Count);
            var expectedInit = Statistics.Initiative(genepool[initIndex].Level, genepool[initIndex].Role);
            var observedInit = genepool[initIndex].Initiative;
            var bonusInit = observedInit - expectedInit;
            Creature.Initiative = Statistics.Initiative(Creature.Level, Creature.Role) + bonusInit;

            // Languages
            var langsIndex = Session.Random.Next(genepool.Count);
            Creature.Languages = genepool[langsIndex].Languages;

            // Movement
            var moveIndex = Session.Random.Next(genepool.Count);
            Creature.Movement = genepool[moveIndex].Movement;

            // Senses
            var sensesIndex = Session.Random.Next(genepool.Count);
            Creature.Senses = genepool[sensesIndex].Senses;

            // Damage modifiers
            var damageIndex = Session.Random.Next(genepool.Count);
            Creature.DamageModifiers.Clear();
            foreach (var dm in genepool[damageIndex].DamageModifiers)
                Creature.DamageModifiers.Add(dm.Copy());
            Creature.Resist = genepool[damageIndex].Resist;
            Creature.Vulnerable = genepool[damageIndex].Vulnerable;
            Creature.Immune = genepool[damageIndex].Immune;

            // Auras
            var auraIndex = Session.Random.Next(genepool.Count);
            Creature.Auras.Clear();
            foreach (var a in genepool[auraIndex].Auras)
                Creature.Auras.Add(a.Copy());

            // Regeneration
            var regenIndex = Session.Random.Next(genepool.Count);
            Creature.Regeneration = genepool[regenIndex].Regeneration;

            var powers = new Dictionary<CreaturePowerCategory, List<CreaturePower>>();
            var creatureNameLookup = new Dictionary<Guid, string>();
            var creatureCategoryLookup = new Dictionary<Guid, string>();

            var powerCategories = Enum.GetValues(typeof(CreaturePowerCategory));
            foreach (CreaturePowerCategory cat in powerCategories)
                powers[cat] = new List<CreaturePower>();

            foreach (var c in genepool)
            foreach (var cp in c.CreaturePowers)
            {
                powers[cp.Category].Add(cp);
                creatureNameLookup[cp.Id] = c.Name;
                creatureCategoryLookup[cp.Id] = c.Name;
            }

            // Powers
            Creature.CreaturePowers.Clear();
            foreach (CreaturePowerCategory cat in powerCategories)
            {
                if (powers[cat].Count == 0)
                    continue;

                var count = powers[cat].Count / genepool.Count;
                if (Session.Random.Next(6) == 0)
                    count += 1;

                for (var n = 0; n != count; ++n)
                {
                    var powerIndex = Session.Random.Next(powers[cat].Count);
                    var cp = powers[cat][powerIndex];

                    // Remove original creature name / category
                    var name = creatureNameLookup[cp.Id];
                    var category = creatureCategoryLookup[cp.Id];
                    cp = replace_name(cp, name, category, Creature.Name);

                    Creature.CreaturePowers.Add(cp);
                }
            }

            // Skills
            var skillsIndex = Session.Random.Next(genepool.Count);
            Creature.Skills = genepool[skillsIndex].Skills;

            // Alignment
            var alignIndex = Session.Random.Next(genepool.Count);
            Creature.Alignment = genepool[alignIndex].Alignment;

            // Tactics, equipment, details
            Creature.Tactics = "";
            Creature.Equipment = "";
            Creature.Details = "";

            Creature.Image = null;
        }

        private CreaturePower alter_power_level(CreaturePower power, int originalLevel, int newLevel)
        {
            var cp = power.Copy();

            var delta = newLevel - originalLevel;
            CreatureHelper.AdjustPowerLevel(cp, delta);

            if (originalLevel <= 15 && newLevel > 15)
            {
                // Dazed => Stunned
                cp.Details = cp.Details.Replace("Dazed", "Stunned");
                cp.Details = cp.Details.Replace("dazed", "stunned");

                // Slowed => Immobilised
                cp.Details = cp.Details.Replace("Slowed", "Immobilised");
                cp.Details = cp.Details.Replace("slowed", "immobilised");
            }

            if (originalLevel > 15 && newLevel <= 15)
            {
                // Stunned => Dazed
                cp.Details = cp.Details.Replace("Stunned", "Dazed");
                cp.Details = cp.Details.Replace("Stunned", "Dazed");

                // Immobilised => Immobilized
                cp.Details = cp.Details.Replace("Immobilised", "Immobilized");
                cp.Details = cp.Details.Replace("immobilised", "immobilized");

                // Immobilized => Slowed
                cp.Details = cp.Details.Replace("Immobilized", "Slowed");
                cp.Details = cp.Details.Replace("immobilized", "slowed");
            }

            return cp;
        }

        private string find_common_name(List<ICreature> creatures)
        {
            const int minNameLength = 3;
            var suggestions = new Dictionary<string, int>();

            for (var x = 0; x != creatures.Count; ++x)
            {
                var nameX = creatures[x].Name;

                for (var y = x + 1; y != creatures.Count; ++y)
                {
                    var nameY = creatures[y].Name;

                    var lcs = StringHelper.LongestCommonToken(nameX, nameY);
                    if (lcs.Length >= minNameLength)
                    {
                        if (!suggestions.ContainsKey(lcs))
                            suggestions[lcs] = 0;

                        suggestions[lcs] += 1;
                    }
                }
            }

            var selected = "";

            if (suggestions.Keys.Count != 0)
                foreach (var token in suggestions.Keys)
                {
                    var current = suggestions.ContainsKey(selected) ? suggestions[selected] : 0;
                    if (suggestions[token] > current)
                        selected = token;
                }

            return selected;
        }

        private enum SidebarType
        {
            Advice,
            Powers,
            Preview
        }
    }
}
