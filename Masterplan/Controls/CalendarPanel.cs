using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.Controls
{
    /// <summary>
    ///     Panel for displaying a Calendar object.
    /// </summary>
    public partial class CalendarPanel : UserControl
    {
        private readonly StringFormat fCentred = new StringFormat();
        private readonly StringFormat fTopRight = new StringFormat();

        private Calendar fCalendar;
        private int fDayOffset;

        private int fMonthIndex;
        private int fWeeks;

        private int fYear;

        /// <summary>
        ///     Gets or sets the calendar to display.
        /// </summary>
        [Category("Data")]
        [Description("The calendar to display.")]
        public Calendar Calendar
        {
            get => fCalendar;
            set
            {
                fCalendar = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the year to be displayed.
        /// </summary>
        [Category("Data")]
        [Description("The year to be displayed.")]
        public int Year
        {
            get => fYear;
            set
            {
                fYear = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the 0-based index of the month to be displayed.
        /// </summary>
        [Category("Data")]
        [Description("The 0-based index of the month to be displayed.")]
        public int MonthIndex
        {
            get => fMonthIndex;
            set
            {
                fMonthIndex = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public CalendarPanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint, true);

            fCentred.Alignment = StringAlignment.Center;
            fCentred.LineAlignment = StringAlignment.Center;
            fCentred.Trimming = StringTrimming.EllipsisWord;

            fTopRight.Alignment = StringAlignment.Far;
            fTopRight.LineAlignment = StringAlignment.Near;
            fTopRight.Trimming = StringTrimming.EllipsisWord;
        }

        /// <summary>
        ///     Called in response to the Paint event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Brush b = new LinearGradientBrush(ClientRectangle, Color.FromArgb(225, 225, 225),
                Color.FromArgb(180, 180, 180), LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(b, ClientRectangle);

            if (fCalendar == null)
            {
                e.Graphics.DrawString("(no calendar)", Font, SystemBrushes.WindowText, ClientRectangle, fCentred);
                return;
            }

            analyse_month();

            var header = new Font(Font, FontStyle.Bold);

            for (var day = 0; day != fCalendar.Days.Count; ++day)
            {
                var di = fCalendar.Days[day];

                // Draw col header cell
                var columnHeaderCell = get_rect(day, -1);
                e.Graphics.DrawString(di.Name, header, SystemBrushes.WindowText, columnHeaderCell, fCentred);
            }

            var mi = fCalendar.Months[fMonthIndex];
            var days = mi.DayCount;
            if (mi.LeapModifier != 0 && mi.LeapPeriod != 0)
                if (fYear % mi.LeapPeriod == 0)
                    days += mi.LeapModifier;

            var plot_points = new Dictionary<int, List<PlotPoint>>();
            Session.Project.AllPlotPoints
                .Where(pp => pp.Date != null)
                .Where(pp =>
                    pp.Date.CalendarId == fCalendar.Id && pp.Date.MonthId == mi.Id && pp.Date.Year == fYear)
                .ToList()
                .ForEach(pp =>
                {
                    if (!plot_points.ContainsKey(pp.Date.DayIndex))
                        plot_points[pp.Date.DayIndex] = new List<PlotPoint>();

                    plot_points[pp.Date.DayIndex].Add(pp);
                });

            for (var day_index = 0; day_index != days; ++day_index)
            {
                var day = day_index + 1;
                var count = get_days_so_far() + day_index;
                var str = "";
                var moons = "";

                fCalendar.Satellites
                    .ForEach(sat =>
                    {
                        if (sat.Period != 0) return;

                        var phase = (count - sat.Offset) % sat.Period;
                        if (phase < 0)
                            phase += sat.Period;

                        if (phase == 0)
                            // New moon
                            moons += "●";

                        if (phase == sat.Period / 2)
                            // Full moon
                            moons += "○";
                    });

                fCalendar.Seasons.Where(ce => ce.MonthId == mi.Id && ce.DayIndex == day_index)
                    .ToList()
                    .ForEach(ce =>
                    {
                        if (str != "")
                            str += Environment.NewLine;

                        str += "Start of " + ce.Name;
                    });

                fCalendar.Events.Where(ce => ce.MonthId == mi.Id && ce.DayIndex == day_index)
                    .ToList()
                    .ForEach(ce =>
                    {
                        if (str != "")
                            str += Environment.NewLine;

                        str += ce.Name;
                    });

                if (plot_points.ContainsKey(day_index))
                    plot_points[day_index].ForEach(pp =>
                    {
                        if (str != "")
                            str += Environment.NewLine;

                        str += pp.Name;
                    });

                var rect = get_rect(day_index);
                e.Graphics.FillRectangle(SystemBrushes.Window, rect);

                var day_rect = new RectangleF(rect.X, rect.Y, 25, 20);
                e.Graphics.DrawString(day.ToString(), Font, SystemBrushes.WindowText, day_rect, fCentred);
                e.Graphics.DrawRectangle(Pens.Gray, day_rect.X, day_rect.Y, day_rect.Width, day_rect.Height);

                if (moons != "")
                    e.Graphics.DrawString(moons, Font, SystemBrushes.WindowText, rect, fTopRight);

                if (str != "")
                {
                    var info_rect = new RectangleF(rect.X, day_rect.Bottom, rect.Width, rect.Bottom - day_rect.Bottom);
                    e.Graphics.DrawString(str, Font, SystemBrushes.WindowText, info_rect, fCentred);
                }

                e.Graphics.DrawRectangle(SystemPens.ControlDark, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        private void analyse_month()
        {
            fWeeks = 0;
            fDayOffset = 0;

            if (fCalendar == null)
                return;

            var days_so_far = get_days_so_far();
            fDayOffset = days_so_far % fCalendar.Days.Count;
            if (fDayOffset < 0)
                fDayOffset += fCalendar.Days.Count;

            // Count the days in the month
            var mi = fCalendar.Months[fMonthIndex];
            var days = mi.DayCount + fDayOffset;
            if (mi.LeapModifier != 0 && mi.LeapPeriod != 0)
                if (fYear % mi.LeapPeriod == 0)
                    days += mi.LeapModifier;

            // How many weeks are in this month?
            fWeeks = days / fCalendar.Days.Count;
            var left_over = days % fCalendar.Days.Count;
            if (left_over != 0)
                fWeeks += 1;
        }

        private int get_days_so_far()
        {
            var days_so_far = 0;

            // Account for intervening years
            var min = Math.Min(fYear, fCalendar.CampaignYear);
            var max = Math.Max(fYear, fCalendar.CampaignYear);
            for (var year = min; year != max; ++year) days_so_far += fCalendar.DayCount(year);
            if (fYear < fCalendar.CampaignYear)
                days_so_far = -days_so_far;

            // Add days of months so far
            for (var month_index = 0; month_index != fMonthIndex; ++month_index)
            {
                var month = fCalendar.Months[month_index];
                days_so_far += month.DayCount;
            }

            return days_so_far;
        }

        private RectangleF get_rect(int day_index)
        {
            // What day of the week is this?
            var cell_index = fDayOffset + day_index;
            var day = cell_index % fCalendar.Days.Count;

            // What week of the month is this?
            var week = cell_index / fCalendar.Days.Count;

            return get_rect(day, week);
        }

        private RectangleF get_rect(int day, int week)
        {
            const float top_line_height = 25;

            var width = (float)ClientRectangle.Width / fCalendar.Days.Count;
            var height = (ClientRectangle.Height - top_line_height) / fWeeks;

            return week == -1
                ? new RectangleF(day * width, 0, width, top_line_height)
                : new RectangleF(day * width, week * height + top_line_height, width, height);
        }
    }
}
