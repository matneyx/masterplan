﻿using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class MapLocationForm : Form
    {
        public MapLocation MapLocation { get; }

        public MapLocationForm(MapLocation loc)
        {
            InitializeComponent();

            CatBox.Items.Add("City");
            CatBox.Items.Add("Town");
            CatBox.Items.Add("Lake");
            CatBox.Items.Add("Port");
            CatBox.Items.Add("Mountain");
            CatBox.Items.Add("Volcano");
            CatBox.Items.Add("Chasm");
            CatBox.Items.Add("Sinkhole");
            CatBox.Items.Add("Cavern");
            CatBox.Items.Add("Marsh");
            CatBox.Items.Add("Swamp");
            CatBox.Items.Add("Fen");
            CatBox.Items.Add("Desert");
            CatBox.Items.Add("River");
            CatBox.Items.Add("Waterfall");
            CatBox.Items.Add("Ruin");
            CatBox.Items.Add("Outpost");

            CatBox.Items.Add("Inn");
            CatBox.Items.Add("Tower");
            CatBox.Items.Add("Barracks");
            CatBox.Items.Add("Hall");
            CatBox.Items.Add("Shop");
            CatBox.Items.Add("Market");
            CatBox.Items.Add("Gate");
            CatBox.Items.Add("Stables");
            CatBox.Items.Add("Warehouse");
            CatBox.Items.Add("Temple");

            MapLocation = loc;

            NameBox.Text = MapLocation.Name;
            CatBox.Text = MapLocation.Category;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            MapLocation.Name = NameBox.Text;
            MapLocation.Category = CatBox.Text;
        }
    }
}
