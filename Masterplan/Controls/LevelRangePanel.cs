using System;
using System.Windows.Forms;

namespace Masterplan.Controls
{
    internal partial class LevelRangePanel : UserControl
    {
        private bool _fInitialising;

        public int MinimumLevel => (int)MinBox.Value;

        public int MaximumLevel => (int)MaxBox.Value;

        public string NameQuery => NameBox.Text;

        public LevelRangePanel()
        {
            InitializeComponent();
        }

        public void SetLevelRange(int minlevel, int maxlevel)
        {
            _fInitialising = true;

            MinBox.Value = Math.Max(MinBox.Minimum, minlevel);
            MaxBox.Value = Math.Min(MaxBox.Maximum, maxlevel);

            _fInitialising = false;
        }

        public event EventHandler RangeChanged;

        private void MinBox_ValueChanged(object sender, EventArgs e)
        {
            if (_fInitialising)
                return;

            _fInitialising = true;

            MaxBox.Value = Math.Max(MaxBox.Value, MinBox.Value);

            _fInitialising = false;

            RangeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MaxBox_ValueChanged(object sender, EventArgs e)
        {
            if (_fInitialising)
                return;

            _fInitialising = true;

            MinBox.Value = Math.Min(MaxBox.Value, MinBox.Value);

            _fInitialising = false;

            RangeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            RangeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
