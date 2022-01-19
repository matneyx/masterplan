using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class MapSelectForm : Form
    {
        public Map Map
        {
            get
            {
                if (MapList.SelectedItems.Count != 0)
                    return MapList.SelectedItems[0].Tag as Map;

                return null;
            }
        }

        public List<Map> Maps
        {
            get
            {
                var maps = new List<Map>();

                foreach (ListViewItem lvi in MapList.CheckedItems)
                {
                    var map = lvi.Tag as Map;
                    if (map != null)
                        maps.Add(map);
                }

                return maps;
            }
        }

        public MapSelectForm(List<Map> maps, List<Guid> exclude, bool multiSelect)
        {
            InitializeComponent();

            // Categories
            var bst = new BinarySearchTree<string>();
            foreach (var map in maps)
                if (map.Category != null && map.Category != "")
                    bst.Add(map.Category);
            var cats = bst.SortedList;
            cats.Add("Miscellaneous Maps");

            foreach (var cat in cats)
                MapList.Groups.Add(cat, cat);

            foreach (var map in maps)
            {
                if (exclude != null && exclude.Contains(map.Id))
                    continue;

                var lvi = MapList.Items.Add(map.Name);
                lvi.Tag = map;

                if (map.Category != null && map.Category != "")
                    lvi.Group = MapList.Groups[map.Category];
                else
                    lvi.Group = MapList.Groups["Miscellaneous Maps"];
            }

            if (multiSelect) MapList.CheckBoxes = true;

            Application.Idle += Application_Idle;
        }

        ~MapSelectForm()
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
