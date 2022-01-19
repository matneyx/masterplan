using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CalendarForm : Form
    {
        public Calendar Calendar { get; }

        public MonthInfo SelectedMonth
        {
            get
            {
                if (MonthList.SelectedItems.Count != 0)
                    return MonthList.SelectedItems[0].Tag as MonthInfo;

                return null;
            }
        }

        public DayInfo SelectedDay
        {
            get
            {
                if (DayList.SelectedItems.Count != 0)
                    return DayList.SelectedItems[0].Tag as DayInfo;

                return null;
            }
        }

        public CalendarEvent SelectedSeason
        {
            get
            {
                if (SeasonList.SelectedItems.Count != 0)
                    return SeasonList.SelectedItems[0].Tag as CalendarEvent;

                return null;
            }
        }

        public CalendarEvent SelectedEvent
        {
            get
            {
                if (EventList.SelectedItems.Count != 0)
                    return EventList.SelectedItems[0].Tag as CalendarEvent;

                return null;
            }
        }

        public Satellite SelectedSatellite
        {
            get
            {
                if (SatelliteList.SelectedItems.Count != 0)
                    return SatelliteList.SelectedItems[0].Tag as Satellite;

                return null;
            }
        }

        public CalendarForm(Calendar calendar)
        {
            InitializeComponent();

            Calendar = calendar.Copy();

            Application.Idle += Application_Idle;
            EventList.ListViewItemSorter = new EventSorter(Calendar);
            SeasonList.ListViewItemSorter = new EventSorter(Calendar);

            NameBox.Text = Calendar.Name;
            YearBox.Value = Calendar.CampaignYear;
            DetailsBox.Text = Calendar.Details;

            update_months();
            update_days();
            update_seasons();
            update_events();
            update_satellites();
        }

        ~CalendarForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            MonthRemoveBtn.Enabled = SelectedMonth != null;
            MonthEditBtn.Enabled = SelectedMonth != null;
            MonthUpBtn.Enabled = SelectedMonth != null && Calendar.Months.IndexOf(SelectedMonth) != 0;
            MonthDownBtn.Enabled = SelectedMonth != null &&
                                   Calendar.Months.IndexOf(SelectedMonth) != Calendar.Months.Count - 1;

            DayRemoveBtn.Enabled = SelectedDay != null;
            DayEditBtn.Enabled = SelectedDay != null;
            DayUpBtn.Enabled = SelectedDay != null && Calendar.Days.IndexOf(SelectedDay) != 0;
            DayDownBtn.Enabled = SelectedDay != null && Calendar.Days.IndexOf(SelectedDay) != Calendar.Days.Count - 1;

            SeasonAddBtn.Enabled = Calendar.Months.Count != 0;
            SeasonRemoveBtn.Enabled = SelectedSeason != null;
            SeasonEditBtn.Enabled = SelectedSeason != null;

            EventAddBtn.Enabled = Calendar.Months.Count != 0;
            EventRemoveBtn.Enabled = SelectedEvent != null;
            EventEditBtn.Enabled = SelectedEvent != null;

            SatelliteRemoveBtn.Enabled = SelectedSatellite != null;
            SatelliteEditBtn.Enabled = SelectedSatellite != null;

            OKBtn.Enabled = Calendar.Months.Count != 0 && Calendar.Days.Count != 0;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Calendar.Name = NameBox.Text;
            Calendar.CampaignYear = (int)YearBox.Value;
            Calendar.Details = DetailsBox.Text;
        }

        private void MonthAddBtn_Click(object sender, EventArgs e)
        {
            var mi = new MonthInfo();
            mi.Name = "New Month";

            var dlg = new MonthForm(mi);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Calendar.Months.Add(dlg.MonthInfo);

                update_months();
                update_seasons();
                update_events();
            }
        }

        private void MonthRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedMonth != null)
            {
                Calendar.Months.Remove(SelectedMonth);

                update_months();
                update_seasons();
                update_events();
            }
        }

        private void MonthEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedMonth != null)
            {
                var index = Calendar.Months.IndexOf(SelectedMonth);

                var dlg = new MonthForm(SelectedMonth);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Calendar.Months[index] = dlg.MonthInfo;

                    update_months();
                    update_seasons();
                    update_events();
                }
            }
        }

        private void MonthUpBtn_Click(object sender, EventArgs e)
        {
            if (SelectedMonth != null)
            {
                var index = Calendar.Months.IndexOf(SelectedMonth);
                if (index == 0)
                    return;

                var tmp = Calendar.Months[index - 1];
                Calendar.Months[index - 1] = SelectedMonth;
                Calendar.Months[index] = tmp;

                update_months();
                update_seasons();
                update_events();

                MonthList.Items[index - 1].Selected = true;
            }
        }

        private void MonthDownBtn_Click(object sender, EventArgs e)
        {
            if (SelectedMonth != null)
            {
                var index = Calendar.Months.IndexOf(SelectedMonth);
                if (index == Calendar.Months.Count - 1)
                    return;

                var tmp = Calendar.Months[index + 1];
                Calendar.Months[index + 1] = SelectedMonth;
                Calendar.Months[index] = tmp;

                update_months();
                update_seasons();
                update_events();

                MonthList.Items[index + 1].Selected = true;
            }
        }

        private void update_months()
        {
            MonthList.Items.Clear();

            foreach (var mi in Calendar.Months)
            {
                var days = mi.DayCount.ToString();
                if (mi.LeapModifier != 0 && mi.LeapPeriod != 0)
                {
                    var count = mi.DayCount + mi.LeapModifier;
                    days += " / " + count;
                }

                var lvi = MonthList.Items.Add(mi.Name);
                lvi.SubItems.Add(days);
                lvi.Tag = mi;
            }

            if (MonthList.Items.Count == 0)
            {
                var lvi = MonthList.Items.Add("(no months)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void DayAddBtn_Click(object sender, EventArgs e)
        {
            var di = new DayInfo();
            di.Name = "New Day";

            var dlg = new DayForm(di);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Calendar.Days.Add(dlg.DayInfo);
                update_days();
            }
        }

        private void DayRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDay != null)
            {
                Calendar.Days.Remove(SelectedDay);
                update_days();
            }
        }

        private void DayEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDay != null)
            {
                var index = Calendar.Days.IndexOf(SelectedDay);

                var dlg = new DayForm(SelectedDay);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Calendar.Days[index] = dlg.DayInfo;
                    update_days();
                }
            }
        }

        private void DayUpBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDay != null)
            {
                var index = Calendar.Days.IndexOf(SelectedDay);
                if (index == 0)
                    return;

                var tmp = Calendar.Days[index - 1];
                Calendar.Days[index - 1] = SelectedDay;
                Calendar.Days[index] = tmp;

                update_days();

                DayList.Items[index - 1].Selected = true;
            }
        }

        private void DayDownBtn_Click(object sender, EventArgs e)
        {
            if (SelectedDay != null)
            {
                var index = Calendar.Days.IndexOf(SelectedDay);
                if (index == Calendar.Days.Count - 1)
                    return;

                var tmp = Calendar.Days[index + 1];
                Calendar.Days[index + 1] = SelectedDay;
                Calendar.Days[index] = tmp;

                update_days();

                DayList.Items[index + 1].Selected = true;
            }
        }

        private void update_days()
        {
            DayList.Items.Clear();

            foreach (var di in Calendar.Days)
            {
                var lvi = DayList.Items.Add(di.Name);
                lvi.Tag = di;
            }

            if (DayList.Items.Count == 0)
            {
                var lvi = DayList.Items.Add("(no days)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void SeasonAddBtn_Click(object sender, EventArgs e)
        {
            var ce = new CalendarEvent();
            ce.Name = "New Season";
            ce.MonthId = Calendar.Months[0].Id;

            var dlg = new SeasonForm(ce, Calendar);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Calendar.Seasons.Add(dlg.Season);
                update_seasons();
            }
        }

        private void SeasonRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSeason != null)
            {
                Calendar.Seasons.Remove(SelectedSeason);
                update_seasons();
            }
        }

        private void SeasonEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSeason != null)
            {
                var index = Calendar.Seasons.IndexOf(SelectedSeason);

                var dlg = new SeasonForm(SelectedSeason, Calendar);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Calendar.Seasons[index] = dlg.Season;
                    update_seasons();
                }
            }
        }

        private void update_seasons()
        {
            SeasonList.Items.Clear();

            foreach (var ce in Calendar.Seasons)
            {
                var mi = Calendar.FindMonth(ce.MonthId);
                var day = ce.DayIndex + 1;

                var lvi = SeasonList.Items.Add(ce.Name);
                lvi.SubItems.Add(mi.Name + " " + day);
                lvi.Tag = ce;
            }

            if (SeasonList.Items.Count == 0)
            {
                var lvi = SeasonList.Items.Add("(no seasons)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            SeasonList.Sort();
        }

        private void EventAddBtn_Click(object sender, EventArgs e)
        {
            var ce = new CalendarEvent();
            ce.Name = "New Event";
            ce.MonthId = Calendar.Months[0].Id;

            var dlg = new CalendarEventForm(ce, Calendar);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Calendar.Events.Add(dlg.Event);
                update_events();
            }
        }

        private void EventRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEvent != null)
            {
                Calendar.Events.Remove(SelectedEvent);
                update_events();
            }
        }

        private void EventEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEvent != null)
            {
                var index = Calendar.Events.IndexOf(SelectedEvent);

                var dlg = new CalendarEventForm(SelectedEvent, Calendar);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Calendar.Events[index] = dlg.Event;
                    update_events();
                }
            }
        }

        private void update_events()
        {
            EventList.Items.Clear();

            foreach (var ce in Calendar.Events)
            {
                var mi = Calendar.FindMonth(ce.MonthId);
                var day = ce.DayIndex + 1;

                var lvi = EventList.Items.Add(ce.Name);
                lvi.SubItems.Add(mi.Name + " " + day);
                lvi.Tag = ce;
            }

            if (EventList.Items.Count == 0)
            {
                var lvi = EventList.Items.Add("(no events)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            EventList.Sort();
        }

        private void SatelliteAddBtn_Click(object sender, EventArgs e)
        {
            var s = new Satellite();
            s.Name = "New Satellite";

            var dlg = new SatelliteForm(s);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Calendar.Satellites.Add(dlg.Satellite);
                update_satellites();
            }
        }

        private void SatelliteRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSatellite != null)
            {
                Calendar.Satellites.Remove(SelectedSatellite);
                update_satellites();
            }
        }

        private void SatelliteEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSatellite != null)
            {
                var index = Calendar.Satellites.IndexOf(SelectedSatellite);

                var dlg = new SatelliteForm(SelectedSatellite);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Calendar.Satellites[index] = dlg.Satellite;
                    update_satellites();
                }
            }
        }

        private void update_satellites()
        {
            SatelliteList.Items.Clear();

            foreach (var s in Calendar.Satellites)
            {
                var lvi = SatelliteList.Items.Add(s.Name);
                lvi.Tag = s;
            }

            if (SatelliteList.Items.Count == 0)
            {
                var lvi = SatelliteList.Items.Add("(no satellites)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private class EventSorter : IComparer
        {
            private readonly Calendar _fCalendar;

            public EventSorter(Calendar c)
            {
                _fCalendar = c;
            }

            public int Compare(object x, object y)
            {
                var lviX = x as ListViewItem;
                var lviY = y as ListViewItem;

                var ceX = lviX.Tag as CalendarEvent;
                var ceY = lviY.Tag as CalendarEvent;

                if (ceX == null || ceY == null)
                    return 0;

                var miX = _fCalendar.FindMonth(ceX.MonthId);
                var miY = _fCalendar.FindMonth(ceY.MonthId);

                var monthX = _fCalendar.Months.IndexOf(miX);
                var monthY = _fCalendar.Months.IndexOf(miY);

                var result = monthX.CompareTo(monthY);
                if (result == 0) result = ceX.DayIndex.CompareTo(ceY.DayIndex);

                return result;
            }
        }
    }
}
