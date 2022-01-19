using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class HeroForm : Form
    {
        public Hero Hero { get; }

        public OngoingCondition SelectedEffect
        {
            get
            {
                if (EffectList.SelectedItems.Count != 0)
                    return EffectList.SelectedItems[0].Tag as OngoingCondition;

                return null;
            }
        }

        public CustomToken SelectedToken
        {
            get
            {
                if (EffectList.SelectedItems.Count != 0)
                    return EffectList.SelectedItems[0].Tag as CustomToken;

                return null;
            }
        }

        public HeroForm(Hero h)
        {
            InitializeComponent();

            foreach (CreatureSize size in Enum.GetValues(typeof(CreatureSize)))
                SizeBox.Items.Add(size);

            foreach (HeroRoleType role in Enum.GetValues(typeof(HeroRoleType)))
                RoleBox.Items.Add(role);

            SourceBox.Items.Add("Arcane");
            SourceBox.Items.Add("Divine");
            SourceBox.Items.Add("Elemental");
            SourceBox.Items.Add("Martial");
            SourceBox.Items.Add("Primal");
            SourceBox.Items.Add("Psionic");
            SourceBox.Items.Add("Shadow");

            Application.Idle += Application_Idle;

            Hero = h.Copy();

            update_hero();
        }

        ~HeroForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            PortraitPasteBtn.Enabled = Clipboard.ContainsImage();
            PortraitClear.Enabled = Hero.Portrait != null;

            EffectRemoveBtn.Enabled = SelectedEffect != null || SelectedToken != null;
            EffectEditBtn.Enabled = SelectedEffect != null || SelectedToken != null;
        }

        private void HeroForm_Shown(object sender, EventArgs e)
        {
            NameBox.Focus();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Hero.Name = NameBox.Text;
            Hero.Player = PlayerBox.Text;
            Hero.Level = (int)LevelBox.Value;

            Hero.Race = RaceBox.Text;
            Hero.Size = (CreatureSize)SizeBox.SelectedItem;

            Hero.Class = ClassBox.Text;
            Hero.ParagonPath = PPBox.Text;
            Hero.EpicDestiny = EDBox.Text;
            Hero.PowerSource = SourceBox.Text;
            Hero.Role = (HeroRoleType)RoleBox.SelectedItem;

            Hero.Languages = LanguageBox.Text;

            Hero.Hp = (int)HPBox.Value;

            Hero.Ac = (int)ACBox.Value;
            Hero.Fortitude = (int)FortBox.Value;
            Hero.Reflex = (int)RefBox.Value;
            Hero.Will = (int)WillBox.Value;

            Hero.InitBonus = (int)InitBox.Value;

            Hero.PassiveInsight = (int)InsightBox.Value;
            Hero.PassivePerception = (int)PerceptionBox.Value;
        }

        private void PortraitBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.ImageFilter;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Hero.Portrait = Image.FromFile(dlg.FileName);
                Program.SetResolution(Hero.Portrait);
                image_changed();
            }
        }

        private void PortraitPaste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Hero.Portrait = Clipboard.GetImage();
                Program.SetResolution(Hero.Portrait);
                image_changed();
            }
        }

        private void PortraitClear_Click(object sender, EventArgs e)
        {
            Hero.Portrait = null;
            image_changed();
        }

        private void EffectAddBtn_Click(object sender, EventArgs e)
        {
            var oc = new OngoingCondition();

            var dlg = new EffectForm(oc, Hero);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Hero.Effects.Add(dlg.Effect);
                update_effects();
            }
        }

        private void EffectAddToken_Click(object sender, EventArgs e)
        {
            var token = new CustomToken();
            token.Name = "New Token";
            token.Type = CustomTokenType.Token;

            var dlg = new CustomTokenForm(token);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Hero.Tokens.Add(dlg.Token);
                update_effects();
            }
        }

        private void EffectAddOverlay_Click(object sender, EventArgs e)
        {
            var overlay = new CustomToken();
            overlay.Name = "New Overlay";
            overlay.Type = CustomTokenType.Overlay;

            var dlg = new CustomOverlayForm(overlay);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Hero.Tokens.Add(dlg.Token);
                update_effects();
            }
        }

        private void EffectRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEffect != null)
            {
                Hero.Effects.Remove(SelectedEffect);
                update_effects();
            }

            if (SelectedToken != null)
            {
                Hero.Tokens.Remove(SelectedToken);
                update_effects();
            }
        }

        private void EffectEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEffect != null)
            {
                var index = Hero.Effects.IndexOf(SelectedEffect);

                var dlg = new EffectForm(SelectedEffect, Hero);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Hero.Effects[index] = dlg.Effect;
                    update_effects();
                }
            }

            if (SelectedToken != null)
            {
                var index = Hero.Tokens.IndexOf(SelectedToken);

                switch (SelectedToken.Type)
                {
                    case CustomTokenType.Token:
                    {
                        var dlg = new CustomTokenForm(SelectedToken);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            Hero.Tokens[index] = dlg.Token;
                            update_effects();
                        }
                    }
                        break;
                    case CustomTokenType.Overlay:
                    {
                        var dlg = new CustomOverlayForm(SelectedToken);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            Hero.Tokens[index] = dlg.Token;
                            update_effects();
                        }
                    }
                        break;
                }
            }
        }

        private void image_changed()
        {
            PortraitBox.Image = Hero.Portrait;
        }

        private void update_hero()
        {
            NameBox.Text = Hero.Name;
            PlayerBox.Text = Hero.Player;
            LevelBox.Value = Hero.Level;

            RaceBox.Text = Hero.Race;
            SizeBox.SelectedItem = Hero.Size;

            ClassBox.Text = Hero.Class;
            PPBox.Text = Hero.ParagonPath;
            EDBox.Text = Hero.EpicDestiny;
            SourceBox.Text = Hero.PowerSource;
            RoleBox.SelectedItem = Hero.Role;

            LanguageBox.Text = Hero.Languages;

            if (Hero.Portrait != null)
                PortraitBox.Image = Hero.Portrait;

            HPBox.Value = Hero.Hp;

            ACBox.Value = Hero.Ac;
            FortBox.Value = Hero.Fortitude;
            RefBox.Value = Hero.Reflex;
            WillBox.Value = Hero.Will;

            InitBox.Value = Hero.InitBonus;

            InsightBox.Value = Hero.PassiveInsight;
            PerceptionBox.Value = Hero.PassivePerception;

            update_effects();
        }

        private void update_effects()
        {
            EffectList.Items.Clear();

            foreach (var oc in Hero.Effects)
            {
                var lvi = EffectList.Items.Add(oc.ToString(null, false));
                lvi.Tag = oc;
                lvi.Group = EffectList.Groups[0];
            }

            foreach (var ct in Hero.Tokens)
            {
                var lvi = EffectList.Items.Add(ct.Name);
                lvi.Tag = ct;
                switch (ct.Type)
                {
                    case CustomTokenType.Token:
                        lvi.Group = EffectList.Groups[1];
                        break;
                    case CustomTokenType.Overlay:
                        lvi.Group = EffectList.Groups[2];
                        break;
                }
            }
        }
    }
}
