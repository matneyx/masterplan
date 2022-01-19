using System;
using System.Windows.Forms;

namespace Masterplan.Wizards
{
    internal partial class WizardForm : Form
    {
        private readonly Wizard _fWizard;

        public IWizardPage CurrentPage
        {
            get
            {
                if (ContentPnl.Controls.Count != 0)
                    return ContentPnl.Controls[0] as IWizardPage;

                return null;
            }
        }

        public WizardForm(Wizard wiz)
        {
            InitializeComponent();

            _fWizard = wiz;
            Text = _fWizard.Title;

            // Set size
            if (!_fWizard.MaxSize.IsEmpty)
            {
                Width += Math.Max(_fWizard.MaxSize.Width, ContentPnl.Width) - ContentPnl.Width;
                Height += Math.Max(_fWizard.MaxSize.Height, ContentPnl.Height) - ContentPnl.Height;

                ImageBox.Height = ContentPnl.Height;
            }

            Application.Idle += Application_Idle;

            if (_fWizard.Pages.Count != 0)
                set_page(0);
        }

        ~WizardForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            var page = CurrentPage;
            if (page != null)
            {
                BackBtn.Enabled = page.AllowBack;
                NextBtn.Enabled = page.AllowNext;
                FinishBtn.Enabled = page.AllowFinish;

                if (page.AllowFinish)
                    AcceptButton = FinishBtn;
                else if (page.AllowNext)
                    AcceptButton = NextBtn;
                else
                    AcceptButton = null;
            }
        }

        private void BackBtn_Click(object sender, EventArgs e)
        {
            if (CurrentPage != null)
            {
                if (!CurrentPage.AllowBack)
                    return;

                if (!CurrentPage.OnBack())
                    return;

                var currentPage = _fWizard.Pages.IndexOf(CurrentPage);
                var pageindex = _fWizard.BackPageIndex(currentPage);

                if (pageindex == -1)
                    pageindex = currentPage - 1;

                set_page(pageindex);
            }
        }

        private void NextBtn_Click(object sender, EventArgs e)
        {
            if (CurrentPage != null)
            {
                if (!CurrentPage.AllowNext)
                    return;

                if (!CurrentPage.OnNext())
                    return;

                var currentPage = _fWizard.Pages.IndexOf(CurrentPage);
                var pageindex = _fWizard.NextPageIndex(currentPage);

                if (pageindex == -1)
                    pageindex = currentPage + 1;

                set_page(pageindex);
            }
        }

        private void FinishBtn_Click(object sender, EventArgs e)
        {
            if (CurrentPage != null)
            {
                if (!CurrentPage.AllowFinish)
                    return;

                if (!CurrentPage.OnFinish())
                    return;

                _fWizard.OnFinish();

                Close();
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            if (CurrentPage != null) _fWizard.OnCancel();

            Close();
        }

        private void set_page(int pageindex)
        {
            var page = _fWizard.Pages[pageindex];
            var ctrl = page as Control;
            if (ctrl != null)
            {
                ContentPnl.Controls.Clear();
                ContentPnl.Controls.Add(ctrl);
                ctrl.Dock = DockStyle.Fill;

                page.OnShown(_fWizard.Data);
            }
        }
    }
}
