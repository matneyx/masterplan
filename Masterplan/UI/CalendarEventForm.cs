using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CalendarEventForm : Form
    {
        private readonly Calendar _fCalendar;

        public CalendarEvent Event { get; }

        public CalendarEventForm(CalendarEvent ce, Calendar calendar)
        {
            InitializeComponent();

            Event = ce.Copy();
            _fCalendar = calendar;

            foreach (var mi in _fCalendar.Months)
                MonthBox.Items.Add(mi);

            NameBox.Text = Event.Name;
            DayBox.Value = Event.DayIndex + 1;

            var month = _fCalendar.FindMonth(Event.MonthId);
            MonthBox.SelectedItem = month;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Event.Name = NameBox.Text;
            Event.DayIndex = (int)DayBox.Value - 1;

            var mi = MonthBox.SelectedItem as MonthInfo;
            Event.MonthId = mi.Id;
        }

        private void MonthBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var mi = MonthBox.SelectedItem as MonthInfo;
            DayBox.Maximum = mi.DayCount;
        }
    }
}
