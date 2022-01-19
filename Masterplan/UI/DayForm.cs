using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class DayForm : Form
    {
        public DayInfo DayInfo { get; }

        public DayForm(DayInfo day)
        {
            InitializeComponent();

            DayInfo = day.Copy();

            NameBox.Text = DayInfo.Name;
            NameBox.SelectAll();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            DayInfo.Name = NameBox.Text;
        }
    }
}
