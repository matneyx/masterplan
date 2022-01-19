using System;
using System.Drawing.Printing;
using System.Windows.Forms;
using Masterplan.Controls;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class MapPrintingForm : Form
    {
        private readonly MapView _fMapView;
        private PrinterSettings _fSettings = new PrinterSettings();

        public MapPrintingForm(MapView mapview)
        {
            InitializeComponent();

            _fMapView = mapview;
            OnePageBtn.Checked = true;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            var poster = PosterBtn.Checked;
            MapPrinting.Print(_fMapView, poster, _fSettings);
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            var dlg = new PrintDialog();
            dlg.AllowPrintToFile = false;
            dlg.PrinterSettings = _fSettings;

            if (dlg.ShowDialog() == DialogResult.OK)
                _fSettings = dlg.PrinterSettings;
        }
    }
}
