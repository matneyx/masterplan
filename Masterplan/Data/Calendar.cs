using System;
using System.Collections.Generic;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a custom calendar.
    /// </summary>
    [Serializable]
    public class Calendar
    {
        private int _fCampaignYear = 1000;

        private List<DayInfo> _fDays = new List<DayInfo>();

        private string _fDetails = "";

        private List<CalendarEvent> _fEvents = new List<CalendarEvent>();

        private Guid _fId = Guid.NewGuid();

        private List<MonthInfo> _fMonths = new List<MonthInfo>();

        private string _fName = "";

        private List<Satellite> _fSatellites = new List<Satellite>();

        private List<CalendarEvent> _fSeasons = new List<CalendarEvent>();

        /// <summary>
        ///     Gets or sets the unique ID of the calendar.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the calendar name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the calendar details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the current year for the calendar.
        /// </summary>
        public int CampaignYear
        {
            get => _fCampaignYear;
            set => _fCampaignYear = value;
        }

        /// <summary>
        ///     Gets or sets the list of months.
        /// </summary>
        public List<MonthInfo> Months
        {
            get => _fMonths;
            set => _fMonths = value;
        }

        /// <summary>
        ///     Gets or sets the list of days.
        /// </summary>
        public List<DayInfo> Days
        {
            get => _fDays;
            set => _fDays = value;
        }

        /// <summary>
        ///     Gets or sets the list of seasons.
        /// </summary>
        public List<CalendarEvent> Seasons
        {
            get => _fSeasons;
            set => _fSeasons = value;
        }

        /// <summary>
        ///     Gets or sets the list of events.
        /// </summary>
        public List<CalendarEvent> Events
        {
            get => _fEvents;
            set => _fEvents = value;
        }

        /// <summary>
        ///     Gets or sets the list of satellites.
        /// </summary>
        public List<Satellite> Satellites
        {
            get => _fSatellites;
            set => _fSatellites = value;
        }

        /// <summary>
        ///     Calculate the number of days in a year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>Returns the number of days in the year.</returns>
        public int DayCount(int year)
        {
            var days = 0;

            foreach (var mi in _fMonths)
            {
                days += mi.DayCount;

                if (mi.LeapModifier != 0 && mi.LeapPeriod != 0)
                    if (year % mi.LeapPeriod == 0)
                        days += mi.LeapModifier;
            }

            return days;
        }

        /// <summary>
        ///     Finds the month with the given ID.
        /// </summary>
        /// <param name="monthId">The ID of the month.</param>
        /// <returns>Returns the month if it exists; null otherwise.</returns>
        public MonthInfo FindMonth(Guid monthId)
        {
            foreach (var mi in _fMonths)
                if (mi.Id == monthId)
                    return mi;

            return null;
        }

        /// <summary>
        ///     Returns the calendar name.
        /// </summary>
        /// <returns>Returns the calendar name.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Creates a copy of the calendar.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Calendar Copy()
        {
            var c = new Calendar();

            c.Id = _fId;
            c.Name = _fName;
            c.Details = _fDetails;
            c.CampaignYear = _fCampaignYear;

            foreach (var mi in _fMonths)
                c.Months.Add(mi.Copy());

            foreach (var di in _fDays)
                c.Days.Add(di.Copy());

            foreach (var ce in _fSeasons)
                c.Seasons.Add(ce.Copy());

            foreach (var ce in _fEvents)
                c.Events.Add(ce.Copy());

            foreach (var s in _fSatellites)
                c.Satellites.Add(s.Copy());

            return c;
        }
    }

    /// <summary>
    ///     Class representing a month in a custom calendar.
    /// </summary>
    [Serializable]
    public class MonthInfo
    {
        private int _fDayCount = 30;

        private Guid _fId = Guid.NewGuid();

        private int _fModifier;

        private string _fName = "";

        private int _fPeriod = 4;

        /// <summary>
        ///     Gets or sets the unique ID of the month.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the month.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the number of days in the month.
        /// </summary>
        public int DayCount
        {
            get => _fDayCount;
            set => _fDayCount = value;
        }

        /// <summary>
        ///     Gets or sets the change to the number of days in a leap year.
        /// </summary>
        public int LeapModifier
        {
            get => _fModifier;
            set => _fModifier = value;
        }

        /// <summary>
        ///     Gets or sets the frequency of leap years.
        /// </summary>
        public int LeapPeriod
        {
            get => _fPeriod;
            set => _fPeriod = value;
        }

        /// <summary>
        ///     Creates a copy of the MonthInfo.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public MonthInfo Copy()
        {
            var mi = new MonthInfo();

            mi.Id = _fId;
            mi.Name = _fName;
            mi.DayCount = _fDayCount;
            mi.LeapModifier = _fModifier;
            mi.LeapPeriod = _fPeriod;

            return mi;
        }

        /// <summary>
        ///     Returns the month name.
        /// </summary>
        /// <returns>Returns the month name.</returns>
        public override string ToString()
        {
            return _fName;
        }
    }

    /// <summary>
    ///     Class representing a day in a custom calendar.
    /// </summary>
    [Serializable]
    public class DayInfo
    {
        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        /// <summary>
        ///     Gets or sets the unique ID of the day.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the day name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Creates a copy of the DayInfo.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public DayInfo Copy()
        {
            var di = new DayInfo();

            di.Id = _fId;
            di.Name = _fName;

            return di;
        }
    }

    /// <summary>
    ///     Class representing a yearly event in a custom calendar.
    /// </summary>
    [Serializable]
    public class CalendarEvent
    {
        private int _fDayIndex = 1;

        private Guid _fId = Guid.NewGuid();

        private Guid _fMonthId = Guid.Empty;

        private string _fName = "";

        /// <summary>
        ///     Gets or sets the unique ID of the event.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the event.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the month the event occurs in.
        /// </summary>
        public Guid MonthId
        {
            get => _fMonthId;
            set => _fMonthId = value;
        }

        /// <summary>
        ///     Gets or sets the day on which the event occurs.
        /// </summary>
        public int DayIndex
        {
            get => _fDayIndex;
            set => _fDayIndex = value;
        }

        /// <summary>
        ///     Creates a copy of the event.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public CalendarEvent Copy()
        {
            var ce = new CalendarEvent();

            ce.Id = _fId;
            ce.Name = _fName;
            ce.MonthId = _fMonthId;
            ce.DayIndex = _fDayIndex;

            return ce;
        }
    }

    /// <summary>
    ///     Class representing a satellite (moon etc) in a custom calendar.
    /// </summary>
    [Serializable]
    public class Satellite
    {
        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        private int _fOffset;

        private int _fPeriod = 1;

        /// <summary>
        ///     Gets or sets the unique ID of the satellite.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the satellite.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the number of days the satellite takes for a full rotation.
        /// </summary>
        public int Period
        {
            get => _fPeriod;
            set => _fPeriod = value;
        }

        /// <summary>
        ///     Gets or sets the offset for the satellite.
        /// </summary>
        public int Offset
        {
            get => _fOffset;
            set => _fOffset = value;
        }

        /// <summary>
        ///     Creates a copy of the satellite.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Satellite Copy()
        {
            var s = new Satellite();

            s.Id = _fId;
            s.Name = _fName;
            s.Period = _fPeriod;
            s.Offset = _fOffset;

            return s;
        }
    }

    /// <summary>
    ///     Class representing a specific date in a custom calendar.
    /// </summary>
    [Serializable]
    public class CalendarDate
    {
        private Guid _fCalendarId = Guid.Empty;

        private int _fDayIndex;

        private Guid _fId = Guid.NewGuid();

        private Guid _fMonthId = Guid.Empty;

        private int _fYear;

        /// <summary>
        ///     Gets or sets the unique ID of the date.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the calendar to use.
        /// </summary>
        public Guid CalendarId
        {
            get => _fCalendarId;
            set => _fCalendarId = value;
        }

        /// <summary>
        ///     Gets or sets the calendar year.
        /// </summary>
        public int Year
        {
            get => _fYear;
            set => _fYear = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the month.
        /// </summary>
        public Guid MonthId
        {
            get => _fMonthId;
            set => _fMonthId = value;
        }

        /// <summary>
        ///     Gets or sets the 0-based index of the day.
        /// </summary>
        public int DayIndex
        {
            get => _fDayIndex;
            set => _fDayIndex = value;
        }

        /// <summary>
        ///     Month N, Year
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var cal = Session.Project.FindCalendar(_fCalendarId);

            var month = cal?.FindMonth(_fMonthId);
            if (month == null)
                return "";

            var day = _fDayIndex + 1;

            return month.Name + " " + day + ", " + _fYear;
        }

        /// <summary>
        ///     Creates a copy of the date.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public CalendarDate Copy()
        {
            var cd = new CalendarDate();

            cd.Id = _fId;
            cd.Year = _fYear;
            cd.CalendarId = _fCalendarId;
            cd.MonthId = _fMonthId;
            cd.DayIndex = _fDayIndex;

            return cd;
        }
    }
}
