using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class CreatureTemplateBuilderForm : Form
    {
        public CreatureTemplate Template { get; }

        public CreatureTemplateBuilderForm(CreatureTemplate template)
        {
            InitializeComponent();

            Template = template.Copy();

            update_statblock();
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "build")
            {
                if (e.Url.LocalPath == "profile")
                {
                    e.Cancel = true;

                    var dlg = new CreatureTemplateProfileForm(Template);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Template.Name = dlg.Template.Name;
                        Template.Type = dlg.Template.Type;
                        Template.Role = dlg.Template.Role;
                        Template.Leader = dlg.Template.Leader;

                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "combat")
                {
                    e.Cancel = true;

                    var dlg = new CreatureTemplateStatsForm(Template);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Template.Hp = dlg.Template.Hp;
                        Template.Initiative = dlg.Template.Initiative;
                        Template.Ac = dlg.Template.Ac;
                        Template.Fortitude = dlg.Template.Fortitude;
                        Template.Reflex = dlg.Template.Reflex;
                        Template.Will = dlg.Template.Will;

                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "damage")
                {
                    e.Cancel = true;

                    var dlg = new DamageModListForm(Template);
                    if (dlg.ShowDialog() == DialogResult.OK) update_statblock();
                }

                if (e.Url.LocalPath == "senses")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Template.Senses, "Senses", "");
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Template.Senses = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "movement")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Template.Movement, "Movement", "");
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Template.Movement = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "tactics")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Template.Tactics, "Tactics", "");
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Template.Tactics = dlg.Details;
                        update_statblock();
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

                    var functional = Template.Type == CreatureTemplateType.Functional;
                    var dlg = new PowerBuilderForm(pwr, null, functional);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Template.CreaturePowers.Add(dlg.Power);
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "addtrait")
                {
                    e.Cancel = true;

                    var pwr = new CreaturePower();
                    pwr.Name = "New Trait";
                    pwr.Action = null;

                    var functional = Template.Type == CreatureTemplateType.Functional;
                    var dlg = new PowerBuilderForm(pwr, null, functional);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Template.CreaturePowers.Add(dlg.Power);
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
                        Template.Auras.Add(dlg.Aura);
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "regenedit")
                {
                    e.Cancel = true;

                    var regen = Template.Regeneration;
                    if (regen == null)
                        regen = new Regeneration(5, "");

                    var dlg = new RegenerationForm(regen);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Template.Regeneration = dlg.Regeneration;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "regenremove")
                {
                    e.Cancel = true;

                    Template.Regeneration = null;
                    update_statblock();
                }
            }

            if (e.Url.Scheme == "powerup")
            {
            }

            if (e.Url.Scheme == "powerdown")
            {
            }

            if (e.Url.Scheme == "poweredit")
            {
                var pwr = find_power(new Guid(e.Url.LocalPath));
                if (pwr != null)
                {
                    e.Cancel = true;
                    var index = Template.CreaturePowers.IndexOf(pwr);

                    var functional = Template.Type == CreatureTemplateType.Functional;
                    var dlg = new PowerBuilderForm(pwr, null, functional);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Template.CreaturePowers[index] = dlg.Power;
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

                    Template.CreaturePowers.Remove(pwr);
                    update_statblock();
                }
            }

            if (e.Url.Scheme == "auraedit")
            {
                var aura = find_aura(new Guid(e.Url.LocalPath));
                if (aura != null)
                {
                    e.Cancel = true;
                    var index = Template.Auras.IndexOf(aura);

                    var dlg = new AuraForm(aura);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Template.Auras[index] = dlg.Aura;
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

                    Template.Auras.Remove(aura);
                    update_statblock();
                }
            }
        }

        private CreaturePower find_power(Guid id)
        {
            foreach (var pwr in Template.CreaturePowers)
                if (pwr.Id == id)
                    return pwr;

            return null;
        }

        private Aura find_aura(Guid id)
        {
            foreach (var aura in Template.Auras)
                if (aura.Id == id)
                    return aura;

            return null;
        }

        private void add_power(CreaturePower power)
        {
            Template.CreaturePowers.Add(power);
            update_statblock();
        }

        private void OptionsVariant_Click(object sender, EventArgs e)
        {
        }

        private void update_statblock()
        {
            StatBlockBrowser.DocumentText = Html.CreatureTemplate(Template, Session.Preferences.TextSize, true);
        }

        private void FileExport_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Title = "Export Creature Template";
            dlg.FileName = Template.Name;
            dlg.Filter = Program.CreatureTemplateFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var ok = Serialisation<CreatureTemplate>.Save(dlg.FileName, Template, SerialisationMode.Binary);

                if (!ok)
                {
                    var error = "The creature template could not be exported.";
                    MessageBox.Show(error, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
