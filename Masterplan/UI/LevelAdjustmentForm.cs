using System;
using System.Windows.Forms;

namespace Masterplan.UI
{
    internal partial class LevelAdjustmentForm : Form
    {
        private const string LevelUp = "More difficult";
        private const string LevelDown = "Less difficult";

        public int LevelAdjustment
        {
            get
            {
                var levels = (int)LevelBox.Value;

                if (DirectionBox.SelectedItem.ToString() == LevelDown)
                    levels *= -1;

                return levels;
            }
        }

        public LevelAdjustmentForm()
        {
            InitializeComponent();

            DirectionBox.Items.Add(LevelUp);
            DirectionBox.Items.Add(LevelDown);

            DirectionBox.SelectedIndex = 0;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }
    }
}
