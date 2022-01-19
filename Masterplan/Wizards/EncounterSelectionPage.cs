using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.UI;

namespace Masterplan.Wizards
{
    internal partial class EncounterSelectionPage : UserControl, IWizardPage
    {
        private AdviceData _fData;

        public EncounterTemplateSlot SelectedSlot
        {
            get
            {
                if (SlotList.SelectedItems.Count != 0)
                    return SlotList.SelectedItems[0].Tag as EncounterTemplateSlot;

                return null;
            }
        }

        public EncounterSelectionPage()
        {
            InitializeComponent();
        }

        private void SlotList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedSlot != null)
            {
                var dlg = new CreatureSelectForm(SelectedSlot, _fData.PartyLevel);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _fData.FilledSlots[SelectedSlot] = dlg.Creature;
                    update_list();
                }
            }
        }

        private void update_list()
        {
            SlotList.Items.Clear();

            foreach (var slot in _fData.SelectedTemplate.Slots)
            {
                var lvi = SlotList.Items.Add(slot_info(slot));
                if (_fData.FilledSlots.ContainsKey(slot))
                    lvi.SubItems.Add(_fData.FilledSlots[slot].Title);
                else
                    lvi.SubItems.Add("(not filled)");

                lvi.Tag = slot;
            }

            if (SlotList.Items.Count == 0)
            {
                var lvi = SlotList.Items.Add("(no unused slots)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private string slot_info(EncounterTemplateSlot slot)
        {
            var level = _fData.PartyLevel + slot.LevelAdjustment;
            var flag = slot.Flag != RoleFlag.Standard ? " " + slot.Flag : "";

            var roles = "";
            foreach (var role in slot.Roles)
            {
                if (roles != "")
                    roles += " / ";

                roles += role.ToString().ToLower();
            }

            if (roles == "")
                roles = "any role";
            if (slot.Minions)
                roles += ", minion";

            var count = "";
            if (slot.Count != 1)
                count = " (x" + slot.Count + ")";

            return "Level " + level + flag + " " + roles + count;
        }

        public bool AllowNext => false;

        public bool AllowBack => true;

        public bool AllowFinish
        {
            get
            {
                foreach (var slot in _fData.SelectedTemplate.Slots)
                    if (!_fData.FilledSlots.ContainsKey(slot))
                        return false;

                return true;
            }
        }

        public void OnShown(object data)
        {
            if (_fData == null)
                _fData = data as AdviceData;

            update_list();
        }

        public bool OnBack()
        {
            return true;
        }

        public bool OnNext()
        {
            return true;
        }

        public bool OnFinish()
        {
            return true;
        }
    }
}
