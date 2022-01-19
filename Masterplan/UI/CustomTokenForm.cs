using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class CustomTokenForm : Form
    {
        public CustomToken Token { get; }

        public CustomTokenForm(CustomToken ct)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            var sizes = Enum.GetValues(typeof(CreatureSize));
            foreach (CreatureSize size in sizes)
                SizeBox.Items.Add(size);

            Token = ct.Copy();

            NameBox.Text = Token.Name;
            SizeBox.SelectedItem = Token.TokenSize;

            update_power();

            DetailsBox.Text = Token.Details;

            var n = Creature.GetSize((CreatureSize)SizeBox.SelectedItem);
            TilePanel.TileSize = new Size(n, n);
            TilePanel.Image = Token.Image;
            TilePanel.Colour = Token.Colour;
        }

        ~CustomTokenForm()
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
            Token.TokenSize = (CreatureSize)SizeBox.SelectedItem;
            Token.Details = DetailsBox.Text;

            Token.Image = TilePanel.Image;
            Token.Colour = TilePanel.Colour;
        }

        private void SizeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var n = Creature.GetSize((CreatureSize)SizeBox.SelectedItem);
            TilePanel.TileSize = new Size(n, n);
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
