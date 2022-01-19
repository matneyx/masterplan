using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class DetailsForm : Form
    {
        private readonly ICreature _fCreature;
        private readonly DetailsField _fField = DetailsField.None;

        public string Details => DetailsBox.Text;

        public DetailsForm(ICreature c, DetailsField field, string hint)
        {
            InitializeComponent();

            _fCreature = c;
            _fField = field;

            if (hint != null && hint != "")
                HintLbl.Text = hint;
            else
                HintStatusbar.Visible = false;

            switch (_fField)
            {
                case DetailsField.Alignment:
                    Text = "Alignment";
                    DetailsBox.Text = _fCreature.Alignment;
                    break;
                case DetailsField.Description:
                    Text = "Description";
                    DetailsBox.Text = _fCreature.Details;
                    break;
                case DetailsField.Equipment:
                    Text = "Equipment";
                    DetailsBox.Text = _fCreature.Equipment;
                    break;
                case DetailsField.Languages:
                    Text = "Languages";
                    DetailsBox.Text = _fCreature.Languages;
                    break;
                case DetailsField.Movement:
                    Text = "Movement";
                    DetailsBox.Text = _fCreature.Movement;
                    break;
                case DetailsField.Senses:
                    Text = "Senses";
                    DetailsBox.Text = _fCreature.Senses;
                    break;
                case DetailsField.Skills:
                    Text = "Skills";
                    DetailsBox.Text = _fCreature.Skills;
                    break;
                case DetailsField.Resist:
                    Text = "Resist";
                    DetailsBox.Text = _fCreature.Resist;
                    break;
                case DetailsField.Immune:
                    Text = "Immune";
                    DetailsBox.Text = _fCreature.Immune;
                    break;
                case DetailsField.Vulnerable:
                    Text = "Vulnerable";
                    DetailsBox.Text = _fCreature.Vulnerable;
                    break;
                case DetailsField.Tactics:
                    Text = "Tactics";
                    DetailsBox.Text = _fCreature.Tactics;
                    break;
            }
        }

        public DetailsForm(string str, string title, string hint)
        {
            InitializeComponent();

            Text = title;
            DetailsBox.Text = str;

            if (hint != null && hint != "")
                HintLbl.Text = hint;
            else
                HintStatusbar.Visible = false;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            switch (_fField)
            {
                case DetailsField.Alignment:
                    _fCreature.Alignment = DetailsBox.Text;
                    break;
                case DetailsField.Description:
                    _fCreature.Details = DetailsBox.Text;
                    break;
                case DetailsField.Equipment:
                    _fCreature.Equipment = DetailsBox.Text;
                    break;
                case DetailsField.Languages:
                    _fCreature.Languages = DetailsBox.Text;
                    break;
                case DetailsField.Movement:
                    _fCreature.Movement = DetailsBox.Text;
                    break;
                case DetailsField.Senses:
                    _fCreature.Senses = DetailsBox.Text;
                    break;
                case DetailsField.Skills:
                    _fCreature.Skills = DetailsBox.Text;
                    break;
                case DetailsField.Resist:
                    _fCreature.Resist = DetailsBox.Text;
                    break;
                case DetailsField.Immune:
                    _fCreature.Immune = DetailsBox.Text;
                    break;
                case DetailsField.Vulnerable:
                    _fCreature.Vulnerable = DetailsBox.Text;
                    break;
                case DetailsField.Tactics:
                    _fCreature.Tactics = DetailsBox.Text;
                    break;
            }
        }
    }
}
