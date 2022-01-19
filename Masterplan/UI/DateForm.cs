using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class DateForm : Form
    {
        public CalendarDate Date { get; }

        public Calendar SelectedCalendar => CalendarBox.SelectedItem as Calendar;

        public MonthInfo SelectedMonth => MonthBox.SelectedItem as MonthInfo;

        public DateForm(CalendarDate date)
        {
            InitializeComponent();

            // Populate calendar box
            foreach (var c in Session.Project.Calendars)
                CalendarBox.Items.Add(c);

            Date = date.Copy();

            var cal = Session.Project.FindCalendar(Date.CalendarId);
            if (cal != null)
                CalendarBox.SelectedItem = cal;
            else
                CalendarBox.SelectedIndex = 0;

            YearBox.Value = Date.Year;

            var month = SelectedCalendar.FindMonth(Date.MonthId);
            if (month != null)
                MonthBox.SelectedItem = month;
            else
                MonthBox.SelectedIndex = 0;

            DayBox.Value = Date.DayIndex + 1;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            var cal = CalendarBox.SelectedItem as Calendar;
            var month = MonthBox.SelectedItem as MonthInfo;

            Date.CalendarId = cal.Id;
            Date.Year = (int)YearBox.Value;
            Date.MonthId = month.Id;
            Date.DayIndex = (int)DayBox.Value - 1;
        }

        private void CalendarBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            MonthBox.Items.Clear();
            foreach (var month in SelectedCalendar.Months)
                MonthBox.Items.Add(month);

            YearBox.Value = SelectedCalendar.CampaignYear;
            MonthBox.SelectedIndex = 0;
        }

        private void YearBox_ValueChanged(object sender, EventArgs e)
        {
            set_days();
        }

        private void MonthBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            set_days();
        }

        private void set_days()
        {
            if (SelectedMonth == null)
                return;

            // How many days in this month?
            var days = SelectedMonth.DayCount;

            var year = (int)YearBox.Value;
            if (year % SelectedMonth.LeapPeriod == 0)
                days += SelectedMonth.LeapModifier;

            DayBox.Maximum = days;
        }
    }
}
