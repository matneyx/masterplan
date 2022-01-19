using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class CustomOverlayForm : Form
    {
        private const string Rounded = "Rounded (translucent)";
        private const string Block = "Block (opaque)";

        public CustomToken Token { get; }

        public CustomOverlayForm(CustomToken ct)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Token = ct.Copy();

            NameBox.Text = Token.Name;

            WidthBox.Value = Token.OverlaySize.Width;
            HeightBox.Value = Token.OverlaySize.Height;

            update_power();

            DetailsBox.Text = Token.Details;

            TilePanel.TileSize = Token.OverlaySize;
            TilePanel.Image = Token.Image;
            TilePanel.Colour = Token.Colour;

            StyleBox.Items.Add(Rounded);
            StyleBox.Items.Add(Block);
            switch (Token.OverlayStyle)
            {
                case OverlayStyle.Rounded:
                    StyleBox.Text = Rounded;
                    break;
                case OverlayStyle.Block:
                    StyleBox.Text = Block;
                    break;
            }

            DifficultBox.Checked = Token.DifficultTerrain;
            OpaqueBox.Checked = Token.Opaque;
        }

        ~CustomOverlayForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = Token.TerrainPower != null;
            SelectBtn.Enabled = Session.TerrainPowers.Count != 0;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Token.Name = NameBox.Text;
            Token.OverlaySize = new Size((int)WidthBox.Value, (int)HeightBox.Value);

            Token.Details = DetailsBox.Text;
            Token.DifficultTerrain = DifficultBox.Checked;
            Token.Opaque = OpaqueBox.Checked;

            Token.Image = TilePanel.Image;
            Token.Colour = TilePanel.Colour;

            Token.OverlayStyle = StyleBox.Text == Rounded ? OverlayStyle.Rounded : OverlayStyle.Block;
        }

        private void WidthBox_ValueChanged(object sender, EventArgs e)
        {
            update_tile_size();
        }

        private void HeightBox_ValueChanged(object sender, EventArgs e)
        {
            update_tile_size();
        }

        private void update_tile_size()
        {
            TilePanel.TileSize = new Size((int)WidthBox.Value, (int)HeightBox.Value);
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            var power = Token.TerrainPower;
            if (power == null)
            {
                power = new TerrainPower();
                power.Name = NameBox.Text;
            }

            var dlg = new TerrainPowerForm(power);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Token.TerrainPower = dlg.Power;
                NameBox.Text = Token.TerrainPower.Name;

                update_power();
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            Token.TerrainPower = null;
            update_power();
        }

        private void SelectBtn_Click(object sender, EventArgs e)
        {
            var dlg = new TerrainPowerSelectForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Token.TerrainPower = dlg.TerrainPower.Copy();
                update_power();
            }
        }

        private void update_power()
        {
            PowerBrowser.DocumentText = Html.TerrainPower(Token.TerrainPower, Session.Preferences.TextSize);
        }
    }
}
