using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.Controls
{
    internal partial class QuestPanel : UserControl
    {
        private Quest _fQuest;

        private bool _fUpdating;

        public Quest Quest
        {
            get => _fQuest;
            set
            {
                _fQuest = value;
                update_view();
            }
        }

        public QuestPanel()
        {
            InitializeComponent();

            var types = Enum.GetValues(typeof(QuestType));
            foreach (QuestType type in types)
                TypeBox.Items.Add(type);
        }

        private void TypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_fUpdating)
                return;

            _fQuest.Type = (QuestType)TypeBox.SelectedItem;
            update_view();
        }

        private void LevelBox_ValueChanged(object sender, EventArgs e)
        {
            if (_fUpdating)
                return;

            _fQuest.Level = (int)LevelBox.Value;
            update_view();
        }

        private void XPSlider_Scroll(object sender, EventArgs e)
        {
            if (_fUpdating)
                return;

            _fQuest.Xp = XPSlider.Value;
            update_view();
        }

        private void update_view()
        {
            _fUpdating = true;

            TypeBox.SelectedItem = _fQuest.Type;
            LevelBox.Value = _fQuest.Level;

            XPSlider.Visible = _fQuest.Type == QuestType.Minor;
            if (XPSlider.Visible)
            {
                var range = Experience.GetMinorQuestXp(_fQuest.Level);
                XPSlider.SetRange(range.First, range.Second);
                MinMaxLbl.Text = range.First + " - " + range.Second;

                if (_fQuest.Xp < range.First)
                    _fQuest.Xp = range.First;
                if (_fQuest.Xp > range.Second)
                    _fQuest.Xp = range.Second;

                XPSlider.Value = _fQuest.Xp;
            }

            XPBox.Text = _fQuest.GetXp() + " XP";

            _fUpdating = false;
        }
    }
}
