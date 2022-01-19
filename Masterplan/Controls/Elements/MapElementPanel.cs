using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.UI;

namespace Masterplan.Controls.Elements
{
    internal partial class MapElementPanel : UserControl
    {
        private MapElement _mapElement;

        public MapElement MapElement
        {
            get => _mapElement;
            set
            {
                _mapElement = value;
                update_view();
            }
        }

        public MapElementPanel()
        {
            InitializeComponent();
        }

        private void MapSelectBtn_Click(object sender, EventArgs e)
        {
            var mapAreaSelectForm = new MapAreaSelectForm(_mapElement.MapId, _mapElement.MapAreaId);

            if (mapAreaSelectForm.ShowDialog() != DialogResult.OK) return;

            _mapElement.MapId = mapAreaSelectForm.Map?.Id ?? Guid.Empty;
            _mapElement.MapAreaId = mapAreaSelectForm.MapArea?.Id ?? Guid.Empty;

            update_view();
        }

        private void update_view()
        {
            var map = Session.Project.FindTacticalMap(_mapElement.MapId);
            if (map != null)
            {
                MapView.Map = map;

                var area = map.FindArea(_mapElement.MapAreaId);
                MapView.Viewpoint = area?.Region ?? Rectangle.Empty;
            }
            else
            {
                MapView.Map = null;
                MapView.Viewpoint = Rectangle.Empty;
            }
        }
    }
}
