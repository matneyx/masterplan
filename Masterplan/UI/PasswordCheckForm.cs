using System;
using System.Windows.Forms;

namespace Masterplan.UI
{
    internal partial class PasswordCheckForm : Form
    {
        private readonly string _fHint = "";

        private readonly string _fPassword = "";

        public PasswordCheckForm(string password, string hint)
        {
            InitializeComponent();

            _fPassword = password;
            _fHint = hint;

            HintBtn.Visible = _fHint != "";
            Application.Idle += Application_Idle;
        }

        ~PasswordCheckForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = PasswordBox.Text.ToLower() == _fPassword;
        }

        private void HintBtn_Click(object sender, EventArgs e)
        {
            var str = "Password hint: " + _fHint;
            MessageBox.Show(this, str, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
