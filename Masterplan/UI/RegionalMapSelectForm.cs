using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class RegionalMapSelectForm : Form
    {
        public RegionalMap Map
        {
            get
            {
                if (MapList.SelectedItems.Count != 0)
                    return MapList.SelectedItems[0].Tag as RegionalMap;

                return null;
            }
        }

        public List<RegionalMap> Maps
        {
            get
            {
                var maps = new List<RegionalMap>();

                foreach (ListViewItem lvi in MapList.CheckedItems)
                {
                    var map = lvi.Tag as RegionalMap;
                    if (map != null)
                        maps.Add(map);
                }

                return maps;
            }
        }

        public RegionalMapSelectForm(List<RegionalMap> maps, List<Guid> exclude, bool multiSelect)
        {
            InitializeComponent();

            foreach (var map in maps)
            {
                if (exclude != null && exclude.Contains(map.Id))
                    continue;

                var lvi = MapList.Items.Add(map.Name);
                lvi.Tag = map;
            }

            if (multiSelect) MapList.CheckBoxes = true;

            Application.Idle += Application_Idle;
        }

        ~RegionalMapSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            if (MapList.CheckBoxes)
                OKBtn.Enabled = MapList.CheckedItems.Count != 0;
            else
                OKBtn.Enabled = Map != null;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (Map != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
