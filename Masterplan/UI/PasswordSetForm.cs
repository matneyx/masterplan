using System;
using System.Windows.Forms;

namespace Masterplan.UI
{
    internal partial class PasswordSetForm : Form
    {
        public string Password => PasswordBox.Text.ToLower();

        public string PasswordHint => HintBox.Text;

        public PasswordSetForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            PasswordBox.Text = Session.Project.Password;
            RetypeBox.Text = Session.Project.Password;
            HintBox.Text = Session.Project.PasswordHint;

            ClearBtn.Visible = Session.Project.Password != "";
        }

        ~PasswordSetForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = PasswordBox.Text.ToLower() == RetypeBox.Text.ToLower();
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            Session.Project.Password = "";
            Session.Project.PasswordHint = "";

            Session.Modified = true;

            DialogResult = DialogResult.Ignore;
            Close();
        }
    }
}
