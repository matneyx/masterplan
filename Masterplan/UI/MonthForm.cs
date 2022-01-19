using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class MonthForm : Form
    {
        public MonthInfo MonthInfo { get; }

        public MonthForm(MonthInfo month)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            MonthInfo = month.Copy();

            NameBox.Text = MonthInfo.Name;
            DaysBox.Value = MonthInfo.DayCount;
            LeapModBox.Value = MonthInfo.LeapModifier;
            LeapPeriodBox.Value = Math.Max(2, MonthInfo.LeapPeriod);
        }

        ~MonthForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            LeapPeriodLbl.Enabled = LeapModBox.Value != 0;
            LeapPeriodBox.Enabled = LeapModBox.Value != 0;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            MonthInfo.Name = NameBox.Text;
            MonthInfo.DayCount = (int)DaysBox.Value;
            MonthInfo.LeapModifier = (int)LeapModBox.Value;
            MonthInfo.LeapPeriod = (int)LeapPeriodBox.Value;
        }
    }
}
