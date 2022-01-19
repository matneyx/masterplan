using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.UI;

namespace Masterplan.Controls.Elements
{
    internal partial class TrapElementPanel : UserControl
    {
        private TrapElement _fTrapElement;

        public TrapElement Trap
        {
            set
            {
                _fTrapElement = value;
                update_view();
            }
        }

        private TrapSkillData SelectedSkill
        {
            get
            {
                if (TrapList.SelectedItems.Count != 0)
                    return TrapList.SelectedItems[0].Tag as TrapSkillData;

                return null;
            }
        }

        private string SelectedCountermeasure
        {
            get
            {
                if (TrapList.SelectedItems.Count != 0)
                    return TrapList.SelectedItems[0].Tag as string;

                return null;
            }
        }

        public TrapElementPanel()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            update_view();
        }

        ~TrapElementPanel()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            ChooseBtn.Enabled = Session.Traps.Count != 0;
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            var dlg = new TrapBuilderForm(_fTrapElement.Trap);

            if (dlg.ShowDialog() != DialogResult.OK) return;

            _fTrapElement.Trap = dlg.Trap;
            update_view();
        }

        private void LocationBtn_Click(object sender, EventArgs e)
        {
            var dlg = new MapAreaSelectForm(_fTrapElement.MapId, _fTrapElement.MapAreaId);

            if (dlg.ShowDialog() != DialogResult.OK) return;

            _fTrapElement.MapId = dlg.Map?.Id ?? Guid.Empty;
            _fTrapElement.MapAreaId = dlg.MapArea?.Id ?? Guid.Empty;

            update_view();
        }

        private void ChooseBtn_Click(object sender, EventArgs e)
        {
            // Choose a standard trap
            var dlg = new TrapSelectForm();

            if (dlg.ShowDialog() != DialogResult.OK) return;

            _fTrapElement.Trap = dlg.Trap.Copy();
            update_view();
        }

        private void TrapList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedSkill != null)
            {
                var index = _fTrapElement.Trap.Skills.IndexOf(SelectedSkill);

                var dlg = new TrapSkillForm(SelectedSkill, _fTrapElement.Trap.Level);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _fTrapElement.Trap.Skills[index] = dlg.SkillData;
                    update_view();
                }
            }

            if (SelectedCountermeasure != null)
            {
                var index = _fTrapElement.Trap.Countermeasures.IndexOf(SelectedCountermeasure);

                var dlg = new TrapCountermeasureForm(SelectedCountermeasure, _fTrapElement.Trap.Level);

                if (dlg.ShowDialog() != DialogResult.OK) return;

                _fTrapElement.Trap.Countermeasures[index] = dlg.Countermeasure;
                update_view();
            }
        }

        private void update_view()
        {
            TrapList.Items.Clear();

            if (_fTrapElement == null)
                return;

            var nameLvi = TrapList.Items.Add(_fTrapElement.Trap.Name + ": " + _fTrapElement.GetXp() + " XP");
            nameLvi.Group = TrapList.Groups[0];

            var infoLvi = TrapList.Items.Add(_fTrapElement.Trap.Info);
            infoLvi.Group = TrapList.Groups[0];

            if (_fTrapElement.MapId != Guid.Empty)
            {
                var m = Session.Project.FindTacticalMap(_fTrapElement.MapId);
                var ma = m.FindArea(_fTrapElement.MapAreaId);

                var str = "Location: " + m.Name;
                if (ma != null)
                    str += " (" + ma.Name + ")";

                var lviLoc = TrapList.Items.Add(str);
                lviLoc.Group = TrapList.Groups[0];
            }

            foreach (var tsd in _fTrapElement.Trap.Skills)
            {
                var lvi = TrapList.Items.Add(tsd.ToString());
                lvi.Group = TrapList.Groups[1];
                lvi.Tag = tsd;
            }

            if (_fTrapElement.Trap.Skills.Count == 0)
            {
                var lvi = TrapList.Items.Add("(no skills)");
                lvi.Group = TrapList.Groups[1];
                lvi.ForeColor = SystemColors.GrayText;
            }

            foreach (var cm in _fTrapElement.Trap.Countermeasures)
            {
                var lvi = TrapList.Items.Add(cm);
                lvi.Group = TrapList.Groups[2];
                lvi.Tag = cm;
            }

            if (_fTrapElement.Trap.Countermeasures.Count == 0)
            {
                var lvi = TrapList.Items.Add("(no countermeasures)");
                lvi.Group = TrapList.Groups[2];
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void AddLibraryBtn_Click(object sender, EventArgs e)
        {
            var dlg = new LibrarySelectForm();

            if (dlg.ShowDialog() != DialogResult.OK) return;

            var lib = dlg.SelectedLibrary;
            lib.Traps.Add(_fTrapElement.Trap.Copy());
        }
    }
}
