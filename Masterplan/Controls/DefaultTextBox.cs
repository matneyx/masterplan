using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Masterplan.Controls
{
    /// <summary>
    ///     Text box which supports default text.
    /// </summary>
    public partial class DefaultTextBox : TextBox
    {
        private string _fDefaultText = "";

        private bool _fUpdating;

        /// <summary>
        ///     Gets or sets the default text to be shown in the text box.
        /// </summary>
        [DefaultValue("")]
        [Description("The default text to be shown in the text box.")]
        [Category("Appearance")]
        public string DefaultText
        {
            get => _fDefaultText;
            set
            {
                if (Text == _fDefaultText)
                    Text = "";

                _fDefaultText = value;

                if (Text == "")
                    Text = _fDefaultText;
            }
        }

        /// <summary>
        ///     Default constructor
        /// </summary>
        public DefaultTextBox()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Sets the default text on the control.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (!_fUpdating && !Focused)
                if (Text == "")
                    Text = _fDefaultText;
        }

        /// <summary>
        ///     Removes the default text from the control, if present, and selects the contents.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            if (Text == _fDefaultText)
            {
                _fUpdating = true;
                Text = "";
                _fUpdating = false;
            }

            SelectAll();
        }

        /// <summary>
        ///     Updates the control with the default text.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            if (Text == "")
            {
                _fUpdating = true;
                Text = _fDefaultText;
                _fUpdating = false;
            }
        }

        /// <summary>
        ///     Ensures that Ctrl-A selects all text.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((e.Modifiers & Keys.Control) == Keys.Control && (e.Modifiers & Keys.Alt) != Keys.Alt)
                if (e.KeyCode == Keys.A)
                {
                    SelectAll();
                    return;
                }

            base.OnKeyDown(e);
        }
    }
}
