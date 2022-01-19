using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools.Generators;

namespace Masterplan.UI
{
    internal partial class MapAreaForm : Form
    {
        public MapArea Area { get; }

        public MapAreaForm(MapArea area, Map m)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Area = area.Copy();
            MapView.Map = m;
            MapView.Viewpoint = Area.Region;

            NameBox.Text = Area.Name;
            DetailsBox.Text = Area.Details;
        }

        ~MapAreaForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            LeftLessBtn.Enabled = MapView.Viewpoint.Width != 1;
            RightLessBtn.Enabled = MapView.Viewpoint.Width != 1;

            TopLessBtn.Enabled = MapView.Viewpoint.Height != 1;
            BottomLessBtn.Enabled = MapView.Viewpoint.Height != 1;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Area.Name = NameBox.Text;
            Area.Details = DetailsBox.Text;
            Area.Region = MapView.Viewpoint;
        }

        private void MoveUpBtn_Click(object sender, EventArgs e)
        {
            Change(0, -1, 0, 0);
        }

        private void MoveLeftBtn_Click(object sender, EventArgs e)
        {
            Change(-1, 0, 0, 0);
        }

        private void MoveRightBtn_Click(object sender, EventArgs e)
        {
            Change(1, 0, 0, 0);
        }

        private void MoveDownBtn_Click(object sender, EventArgs e)
        {
            Change(0, 1, 0, 0);
        }

        private void TopMoreBtn_Click(object sender, EventArgs e)
        {
            Change(0, -1, 0, 1);
        }

        private void TopLessBtn_Click(object sender, EventArgs e)
        {
            Change(0, 1, 0, -1);
        }

        private void LeftMoreBtn_Click(object sender, EventArgs e)
        {
            Change(-1, 0, 1, 0);
        }

        private void LeftLessBtn_Click(object sender, EventArgs e)
        {
            Change(1, 0, -1, 0);
        }

        private void RightMoreBtn_Click(object sender, EventArgs e)
        {
            Change(0, 0, 1, 0);
        }

        private void RightLessBtn_Click(object sender, EventArgs e)
        {
            Change(0, 0, -1, 0);
        }

        private void BottomMoreBtn_Click(object sender, EventArgs e)
        {
            Change(0, 0, 0, 1);
        }

        private void BottomLessBtn_Click(object sender, EventArgs e)
        {
            Change(0, 0, 0, -1);
        }

        private void Change(int x, int y, int width, int height)
        {
            x += MapView.Viewpoint.X;
            y += MapView.Viewpoint.Y;
            width += MapView.Viewpoint.Width;
            height += MapView.Viewpoint.Height;

            MapView.Viewpoint = new Rectangle(x, y, width, height);
        }

        private void RandomNameBtn_Click(object sender, EventArgs e)
        {
            var name = "";
            while (name == "")
                name = RoomBuilder.Name();

            NameBox.Text = name;
        }

        private void RandomDescBtn_Click(object sender, EventArgs e)
        {
            var details = "";
            while (details == "")
                details = RoomBuilder.Details();

            DetailsBox.Text = details;
        }
    }
}
