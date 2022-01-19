using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class TerrainPowerForm : Form
    {
        public TerrainPower Power { get; }

        public TerrainPowerForm(TerrainPower power)
        {
            InitializeComponent();

            var types = Enum.GetValues(typeof(TerrainPowerType));
            foreach (TerrainPowerType type in types)
                TypeBox.Items.Add(type);

            var actions = Enum.GetValues(typeof(ActionType));
            foreach (ActionType action in actions)
                ActionBox.Items.Add(action);

            Power = power.Copy();

            NameBox.Text = Power.Name;
            TypeBox.SelectedItem = Power.Type;
            ActionBox.SelectedItem = Power.Action;

            FlavourBox.Text = Power.FlavourText;
            RequirementBox.Text = Power.Requirement;

            CheckBox.Text = Power.Check;
            SuccessBox.Text = Power.Success;
            FailureBox.Text = Power.Failure;

            AttackBox.Text = Power.Attack;
            TargetBox.Text = Power.Target;
            HitBox.Text = Power.Hit;
            MissBox.Text = Power.Miss;
            EffectBox.Text = Power.Effect;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Power.Name = NameBox.Text;
            Power.Type = (TerrainPowerType)TypeBox.SelectedItem;
            Power.Action = (ActionType)ActionBox.SelectedItem;

            Power.FlavourText = FlavourBox.Text;
            Power.Requirement = RequirementBox.Text;

            Power.Check = CheckBox.Text;
            Power.Success = SuccessBox.Text;
            Power.Failure = FailureBox.Text;

            Power.Attack = AttackBox.Text;
            Power.Target = TargetBox.Text;
            Power.Hit = HitBox.Text;
            Power.Miss = MissBox.Text;
            Power.Effect = EffectBox.Text;
        }
    }
}
