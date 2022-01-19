using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class SatelliteForm : Form
    {
        public Satellite Satellite { get; }

        public SatelliteForm(Satellite sat)
        {
            InitializeComponent();

            Satellite = sat.Copy();

            if (Satellite.Period == 0)
                Satellite.Period = 1;

            NameBox.Text = Satellite.Name;
            PeriodBox.Value = Satellite.Period;
            OffsetBox.Value = Satellite.Offset;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Satellite.Name = NameBox.Text;
            Satellite.Period = (int)PeriodBox.Value;
            Satellite.Offset = (int)OffsetBox.Value;
        }
    }
}
