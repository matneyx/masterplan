using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class SeasonForm : Form
    {
        private readonly Calendar _fCalendar;

        public CalendarEvent Season { get; }

        public SeasonForm(CalendarEvent ce, Calendar calendar)
        {
            InitializeComponent();

            Season = ce.Copy();
            _fCalendar = calendar;

            foreach (var mi in _fCalendar.Months)
                MonthBox.Items.Add(mi);

            NameBox.Text = Season.Name;
            DayBox.Value = Season.DayIndex + 1;

            var month = _fCalendar.FindMonth(Season.MonthId);
            MonthBox.SelectedItem = month;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Season.Name = NameBox.Text;
            Season.DayIndex = (int)DayBox.Value - 1;

            var mi = MonthBox.SelectedItem as MonthInfo;
            Season.MonthId = mi.Id;
        }

        private void MonthBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mi = MonthBox.SelectedItem as MonthInfo;
            DayBox.Maximum = mi.DayCount;
        }
    }
}
