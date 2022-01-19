using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class DamageTypesForm : Form
    {
        public List<DamageType> Types { get; private set; }

        public DamageTypesForm(List<DamageType> types)
        {
            InitializeComponent();

            Types = types;

            var damageTypes = Enum.GetValues(typeof(DamageType));
            foreach (DamageType dt in damageTypes)
            {
                if (dt == DamageType.Untyped)
                    continue;

                var lvi = TypeList.Items.Add(dt.ToString());
                lvi.Checked = Types.Contains(dt);
                lvi.Tag = dt;
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            var types = new List<DamageType>();
            foreach (ListViewItem lvi in TypeList.CheckedItems)
            {
                var dt = (DamageType)lvi.Tag;
                types.Add(dt);
            }

            Types = types;
        }
    }
}
