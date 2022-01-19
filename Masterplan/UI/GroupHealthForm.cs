using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class GroupHealthForm : Form
    {
        private readonly Label _fPlaceholder = new Label();

        private bool _fUpdating;

        public Hero SelectedHero
        {
            get
            {
                if (CombatantList.SelectedItems.Count != 0)
                    return CombatantList.SelectedItems[0].Tag as Hero;

                return null;
            }
        }

        public GroupHealthForm()
        {
            InitializeComponent();

            _fPlaceholder.Text = "Select a PC from the list to set its current HP";
            _fPlaceholder.TextAlign = ContentAlignment.MiddleCenter;
            _fPlaceholder.Dock = DockStyle.Fill;
            HPPanel.Controls.Add(_fPlaceholder);
            _fPlaceholder.BringToFront();

            update_list();
        }

        private void CombatantList_SelectedIndexChanged(object sender, EventArgs e)
        {
            _fUpdating = true;

            update_hp_panel();

            _fUpdating = false;
        }

        private void CombatantList_DoubleClick(object sender, EventArgs e)
        {
        }

        private void MaxHPBox_ValueChanged(object sender, EventArgs e)
        {
            if (_fUpdating)
                return;

            SelectedHero.Hp = (int)MaxHPBox.Value;
            Session.Modified = true;

            CurrentHPBox.Maximum = SelectedHero.Hp;

            update_hp_panel();
            update_list_hp(SelectedHero);
        }

        private void CurrentHPBox_ValueChanged(object sender, EventArgs e)
        {
            if (_fUpdating)
                return;

            var damage = SelectedHero.Hp - (int)CurrentHPBox.Value;

            SelectedHero.CombatData.Damage = damage;
            Session.Modified = true;

            update_hp_panel();
            update_list_hp(SelectedHero);
        }

        private void TempHPBox_ValueChanged(object sender, EventArgs e)
        {
            if (_fUpdating)
                return;

            SelectedHero.CombatData.TempHp = (int)TempHPBox.Value;
            Session.Modified = true;

            update_hp_panel();
            update_list_hp(SelectedHero);
        }

        private void FullHealBtn_Click(object sender, EventArgs e)
        {
            if (SelectedHero != null)
            {
                SelectedHero.CombatData.Damage = 0;
                Session.Modified = true;

                update_hp_panel();
                update_list_hp(SelectedHero);
            }
        }

        private void update_list()
        {
            CombatantList.Items.Clear();

            foreach (var hero in Session.Project.Heroes)
            {
                if (hero.Hp == 0)
                    continue;

                var lvi = CombatantList.Items.Add(hero.Name);
                lvi.SubItems.Add("");
                lvi.Tag = hero;
            }

            foreach (var hero in Session.Project.Heroes)
                update_list_hp(hero);
        }

        private void update_hp_panel()
        {
            if (SelectedHero != null)
            {
                _fPlaceholder.Visible = false;

                HeroNameLbl.Text = SelectedHero.Name;

                MaxHPBox.Value = SelectedHero.Hp;
                CurrentHPBox.Value = SelectedHero.Hp - SelectedHero.CombatData.Damage;
                TempHPBox.Value = SelectedHero.CombatData.TempHp;

                HPGauge.FullHp = SelectedHero.Hp;
                HPGauge.Damage = SelectedHero.CombatData.Damage;
                HPGauge.TempHp = SelectedHero.CombatData.TempHp;

                FullHealBtn.Enabled = SelectedHero.CombatData.Damage != 0;
            }
            else
            {
                _fPlaceholder.Visible = true;
            }
        }

        private void update_list_hp(Hero hero)
        {
            var str = hero.Hp.ToString();
            if (hero.CombatData.Damage > 0)
            {
                var current = hero.Hp - hero.CombatData.Damage;
                str = current + " / " + hero.Hp;
            }

            if (hero.CombatData.TempHp > 0)
                str += " (+" + hero.CombatData.TempHp + ")";

            ListViewItem heroLvi = null;
            foreach (ListViewItem lvi in CombatantList.Items)
                if (lvi.Tag == hero)
                {
                    heroLvi = lvi;
                    break;
                }

            if (heroLvi != null)
                heroLvi.SubItems[1].Text = str;
        }
    }
}
