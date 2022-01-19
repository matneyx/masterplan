using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;
using Masterplan.Properties;
using Masterplan.Tools;

namespace Masterplan.Controls
{
    internal partial class TitlePanel : UserControl
    {
        public enum TitlePanelMode
        {
            WelcomeScreen,
            PlayerView
        }

        private const int MaxAlpha = 255;
        private const int MaxColor = 60;

        private readonly StringFormat _fFormat = new StringFormat();

        private readonly string _fVersion = get_version_string();

        private int _fAlpha;

        private TitlePanelMode _fMode = TitlePanelMode.WelcomeScreen;
        private Rectangle _fVersionRect = Rectangle.Empty;

        private Rectangle _titleRect = Rectangle.Empty;

        [Category("Appearance")] public string Title { get; set; } = "";

        [Category("Layout")]
        public TitlePanelMode Mode
        {
            get => _fMode;
            set
            {
                _fMode = value;
                Invalidate();
            }
        }

        [Category("Behavior")] public bool Zooming { get; set; }

        public TitlePanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint, true);

            _fFormat.Alignment = StringAlignment.Center;
            _fFormat.LineAlignment = StringAlignment.Center;
            _fFormat.Trimming = StringTrimming.EllipsisWord;

            FadeTimer.Enabled = true;
        }

        public event EventHandler FadeFinished;

        protected void OnFadeFinished()
        {
            FadeFinished?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            reset_view();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            reset_view();
        }

        private void reset_view()
        {
            _titleRect = Rectangle.Empty;
            _fVersionRect = Rectangle.Empty;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            try
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                if (_titleRect == Rectangle.Empty)
                {
                    var rect = ClientRectangle;
                    var versionSize = e.Graphics.MeasureString(_fVersion, Font);
                    double versionHeight = versionSize.Height + Height / 10;

                    _titleRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1,
                        (int)(rect.Height - versionHeight - 1));
                    _fVersionRect = new Rectangle(rect.Left, _titleRect.Bottom, rect.Width - 1, (int)versionHeight);
                }

                if (_fMode == TitlePanelMode.WelcomeScreen)
                {
                    var cm = new ColorMatrix();
                    cm.Matrix33 = 0.25F * _fAlpha / MaxAlpha;

                    var ia = new ImageAttributes();
                    ia.SetColorMatrix(cm);

                    Image scrollImg = Resources.Scroll;

                    var y = ClientRectangle.Y + (int)(ClientRectangle.Height * 0.1);
                    var imgHeight = (int)(ClientRectangle.Height * 0.8);
                    var imgWidth = scrollImg.Width * imgHeight / scrollImg.Height;
                    if (imgWidth > ClientRectangle.Width)
                    {
                        imgWidth = ClientRectangle.Width;
                        imgHeight = scrollImg.Height * imgWidth / scrollImg.Width;
                    }

                    var x = ClientRectangle.X + (ClientRectangle.Width - imgWidth) / 2;

                    var imgRect = new Rectangle(x, y, imgWidth, imgHeight);
                    e.Graphics.DrawImage(scrollImg, imgRect, 0, 0, scrollImg.Width, scrollImg.Height,
                        GraphicsUnit.Pixel, ia);
                }

                using (Brush titleBrush = new SolidBrush(Color.FromArgb(_fAlpha, ForeColor)))
                {
                    var textHeight = _titleRect.Height / 2F;
                    float textWidth = _titleRect.Width / Title.Length;
                    var textSize = Math.Min(textHeight, textWidth);

                    if (Zooming)
                    {
                        var delta = 0.1F * _fAlpha / MaxAlpha;
                        textSize *= 0.9F + delta;
                    }

                    if (textHeight > 0)
                        using (var titleFont = new Font(Font.FontFamily, textSize))
                        {
                            e.Graphics.DrawString(Title, titleFont, titleBrush, _titleRect, _fFormat);
                        }

                    if (_fMode == TitlePanelMode.WelcomeScreen)
                        e.Graphics.DrawString(_fVersion, Font, titleBrush, _fVersionRect, _fFormat);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            _fAlpha = Math.Min(_fAlpha + 4, MaxAlpha);

            Invalidate();

            if (_fAlpha == MaxAlpha)
            {
                FadeTimer.Enabled = false;
                OnFadeFinished();

                if (_fMode == TitlePanelMode.PlayerView) PulseTimer.Enabled = true;
            }
        }

        private void PulseTimer_Tick(object sender, EventArgs e)
        {
            _fAlpha = Math.Max(_fAlpha - 1, 0);

            if (Session.Random.Next() % 10 == 0)
                BackColor = change_colour(BackColor);

            Invalidate();
        }

        public void Wake()
        {
            if (PulseTimer.Enabled)
            {
                PulseTimer.Enabled = false;
                FadeTimer.Enabled = true;
            }
        }

        private static string get_version_string()
        {
            var str = "Adventure Design Studio";

            var ass = Assembly.GetEntryAssembly();
            if (ass != null)
            {
                var version = ass.GetName().Version;
                if (version != null)
                {
                    if (str != "")
                        str += Environment.NewLine;

                    str += "Version " + version.Major;

                    if (version.Build != 0)
                        str += "." + version.Minor + "." + version.Build;
                    else if (version.Minor != 0) str += "." + version.Minor;
                }
            }

            if (Program.IsBeta)
            {
                if (str != "")
                    str += Environment.NewLine + Environment.NewLine;

                str += "BETA";
            }

            return str;
        }

        private Color change_colour(Color colour)
        {
            int r = colour.R;
            int g = colour.G;
            int b = colour.B;

            switch (Session.Random.Next() % 4)
            {
                case 0:
                    r = Math.Min(MaxColor, r + 1);
                    break;
                case 1:
                    g = Math.Min(MaxColor, g + 1);
                    break;
                case 2:
                    b = Math.Min(MaxColor, b + 1);
                    break;
                case 3:
                    r = Math.Max(0, r - 1);
                    break;
                case 4:
                    g = Math.Max(0, g - 1);
                    break;
                case 5:
                    b = Math.Max(0, b - 1);
                    break;
            }

            return Color.FromArgb(r, g, b);
        }
    }
}
