using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class MapLocationSelectForm : Form
    {
        public RegionalMap Map => MapBox.SelectedItem as RegionalMap;

        public MapLocation MapLocation => LocationBox.SelectedItem as MapLocation;

        public MapLocationSelectForm(Guid mapId, Guid mapLocationId)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            MapBox.Items.Add("(no map)");
            foreach (var m in Session.Project.RegionalMaps)
                MapBox.Items.Add(m);

            var map = Session.Project.FindRegionalMap(mapId);
            if (map != null)
            {
                MapBox.SelectedItem = map;

                var loc = map.FindLocation(mapLocationId);
                if (loc != null)
                    LocationBox.SelectedItem = loc;
                else
                    LocationBox.SelectedIndex = 0;
            }
            else
            {
                MapBox.SelectedIndex = 0;

                LocationBox.Items.Add("(no map)");
                LocationBox.SelectedIndex = 0;
            }
        }

        ~MapLocationSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            MapLbl.Enabled = Session.Project.RegionalMaps.Count != 0;
            MapBox.Enabled = Session.Project.RegionalMaps.Count != 0;

            var m = MapBox.SelectedItem as RegionalMap;
            var locations = m != null && m.Locations.Count != 0;

            LocationLbl.Enabled = locations;
            LocationBox.Enabled = locations;

            OKBtn.Enabled = MapLocation != null;
        }

        private void MapBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LocationBox.Items.Clear();

            var m = MapBox.SelectedItem as RegionalMap;
            if (m != null)
            {
                LocationBox.Items.Add("(entire map)");

                foreach (var loc in m.Locations)
                    LocationBox.Items.Add(loc);

                LocationBox.SelectedIndex = 0;
            }

            show_map();
        }

        private void AreaBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            show_map();
        }

        private void show_map()
        {
            var m = MapBox.SelectedItem as RegionalMap;
            if (m != null)
            {
                MapPanel.Map = m;

                var loc = LocationBox.SelectedItem as MapLocation;
                MapPanel.HighlightedLocation = loc;
            }
            else
            {
                MapPanel.Map = null;
            }
        }
    }
}
