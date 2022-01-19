using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CreatureClassForm : Form
    {
        private readonly ICreature _fCreature;

        public CreatureSize CreatureSize => (CreatureSize)SizeBox.SelectedItem;

        public CreatureOrigin Origin => (CreatureOrigin)OriginBox.SelectedItem;

        public CreatureType Type => (CreatureType)TypeBox.SelectedItem;

        public string Keywords => KeywordBox.Text;

        public CreatureClassForm(ICreature c)
        {
            InitializeComponent();

            // Populate size
            foreach (CreatureSize size in Enum.GetValues(typeof(CreatureSize)))
                SizeBox.Items.Add(size);

            // Populate origin
            var origins = Enum.GetValues(typeof(CreatureOrigin));
            foreach (CreatureOrigin origin in origins)
                OriginBox.Items.Add(origin);

            // Populate type
            var types = Enum.GetValues(typeof(CreatureType));
            foreach (CreatureType type in types)
                TypeBox.Items.Add(type);

            _fCreature = c;

            SizeBox.SelectedItem = _fCreature.Size;
            OriginBox.SelectedItem = _fCreature.Origin;
            TypeBox.SelectedItem = _fCreature.Type;
            KeywordBox.Text = _fCreature.Keywords;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }
    }
}
