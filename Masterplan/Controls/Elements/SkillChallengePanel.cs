using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.UI;

namespace Masterplan.Controls.Elements
{
    internal partial class SkillChallengePanel : UserControl
    {
        private SkillChallenge _fChallenge;

        private int _fPartyLevel = Session.Project.Party.Level;

        public SkillChallenge Challenge
        {
            get => _fChallenge;
            set
            {
                _fChallenge = value;
                update_view();
            }
        }

        public int PartyLevel
        {
            set
            {
                _fPartyLevel = value;
                update_view();
            }
        }

        private SkillChallengeData SelectedSkill
        {
            get
            {
                if (SkillList.SelectedItems.Count != 0)
                    return SkillList.SelectedItems[0].Tag as SkillChallengeData;

                return null;
            }
        }

        public SkillChallengePanel()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;
        }

        ~SkillChallengePanel()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            ChooseBtn.Enabled = Session.SkillChallenges.Count != 0;
        }

        public void Edit()
        {
            EditBtn_Click(null, null);
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            var dlg = new SkillChallengeBuilderForm(_fChallenge);

            if (dlg.ShowDialog() != DialogResult.OK) return;

            _fChallenge.Name = dlg.SkillChallenge.Name;
            _fChallenge.Complexity = dlg.SkillChallenge.Complexity;
            _fChallenge.Level = dlg.SkillChallenge.Level;
            _fChallenge.Success = dlg.SkillChallenge.Success;
            _fChallenge.Failure = dlg.SkillChallenge.Failure;
            _fChallenge.Notes = dlg.SkillChallenge.Notes;

            _fChallenge.Skills.Clear();
            foreach (var scd in dlg.SkillChallenge.Skills)
                _fChallenge.Skills.Add(scd.Copy());

            update_view();
        }

        private void LocationBtn_Click(object sender, EventArgs e)
        {
            var dlg = new MapAreaSelectForm(_fChallenge.MapId, _fChallenge.MapAreaId);

            if (dlg.ShowDialog() != DialogResult.OK) return;

            _fChallenge.MapId = dlg.Map?.Id ?? Guid.Empty;
            _fChallenge.MapAreaId = dlg.MapArea?.Id ?? Guid.Empty;

            update_view();
        }

        private void ChooseBtn_Click(object sender, EventArgs e)
        {
            var dlg = new SkillChallengeSelectForm();

            if (dlg.ShowDialog() != DialogResult.OK) return;

            _fChallenge.Name = dlg.SkillChallenge.Name;
            _fChallenge.Complexity = dlg.SkillChallenge.Complexity;
            _fChallenge.Success = dlg.SkillChallenge.Success;
            _fChallenge.Failure = dlg.SkillChallenge.Failure;

            _fChallenge.Skills.Clear();
            foreach (var scd in dlg.SkillChallenge.Skills)
                _fChallenge.Skills.Add(scd.Copy());

            _fChallenge.Level = _fPartyLevel;

            update_view();
        }

        private void SkillList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedSkill == null) return;

            var index = _fChallenge.Skills.IndexOf(SelectedSkill);

            var dlg = new SkillChallengeSkillForm(SelectedSkill);

            if (dlg.ShowDialog() != DialogResult.OK) return;

            _fChallenge.Skills[index] = dlg.SkillData;
            update_view();
        }

        private void update_view()
        {
            SkillList.Items.Clear();

            var nameLvi = SkillList.Items.Add(_fChallenge.Name + ": " + _fChallenge.GetXp() + " XP");
            nameLvi.Group = SkillList.Groups[0];

            var infoLvi = SkillList.Items.Add(_fChallenge.Info);
            infoLvi.Group = SkillList.Groups[0];

            if (_fChallenge.MapId != Guid.Empty)
            {
                var m = Session.Project.FindTacticalMap(_fChallenge.MapId);
                var ma = m?.FindArea(_fChallenge.MapAreaId);
                if (ma != null)
                {
                    var str = "Location: " + m.Name;
                    if (ma != null)
                        str += " (" + ma.Name + ")";

                    var lviLoc = SkillList.Items.Add(str);
                    lviLoc.Group = SkillList.Groups[0];
                }
            }

            foreach (var scd in _fChallenge.Skills)
            {
                var diff = scd.Difficulty.ToString().ToLower() + " DCs";
                if (scd.DcModifier != 0)
                {
                    if (scd.DcModifier > 0)
                        diff += " +" + scd.DcModifier;
                    else
                        diff += " " + scd.DcModifier;
                }

                var str = scd.SkillName + " (" + diff + ")";
                if (scd.Details != "")
                    str += ": " + scd.Details;

                var lvi = SkillList.Items.Add(str);
                lvi.Tag = scd;

                switch (scd.Type)
                {
                    case SkillType.Primary:
                        lvi.Group = SkillList.Groups[1];
                        break;
                    case SkillType.Secondary:
                        lvi.Group = SkillList.Groups[2];
                        break;
                    case SkillType.AutoFail:
                        lvi.Group = SkillList.Groups[3];
                        break;
                }

                if (scd.Difficulty == Difficulty.Trivial || scd.Difficulty == Difficulty.Extreme)
                    lvi.ForeColor = Color.Red;
            }

            if (SkillList.Groups[1].Items.Count == 0)
            {
                var lvi = SkillList.Items.Add("(none)");
                lvi.Group = SkillList.Groups[1];
                lvi.ForeColor = SystemColors.GrayText;
            }

            if (SkillList.Groups[2].Items.Count == 0)
            {
                var lvi = SkillList.Items.Add("(none)");
                lvi.Group = SkillList.Groups[2];
                lvi.ForeColor = SystemColors.GrayText;
            }

            SkillList.Sort();
        }

        private void AddLibraryBtn_Click(object sender, EventArgs e)
        {
            var dlg = new LibrarySelectForm();

            if (dlg.ShowDialog() != DialogResult.OK) return;

            var lib = dlg.SelectedLibrary;
            lib.SkillChallenges.Add(_fChallenge.Copy() as SkillChallenge);
        }
    }
}
