using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.Controls.Elements
{
    /// <summary>
    ///     Control for displaying a RegionalMap object.
    /// </summary>
    public partial class RegionalMapPanel : UserControl
    {
        private const float LocationRadius = 8;

        private readonly StringFormat _centered = new StringFormat();

        private MapLocation _highlightedLocation;

        private RegionalMap _map;

        private MapViewMode _mode = MapViewMode.Normal;

        private Plot _plot;

        private MapLocation _selectedLocation;

        private bool _showLocations = true;

        /// <summary>
        ///     Gets or sets the map to be displayed.
        /// </summary>
        public RegionalMap Map
        {
            get => _map;
            set
            {
                _map = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the mode in which to display the map.
        /// </summary>
        public MapViewMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the plot to be used for drawing links.
        /// </summary>
        public Plot Plot
        {
            get => _plot;
            set
            {
                _plot = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets whether to show locations on the map.
        /// </summary>
        public bool ShowLocations
        {
            get => _showLocations;
            set
            {
                _showLocations = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets whether to allow map locations to be moved.
        /// </summary>
        public bool AllowEditing { get; set; }

        /// <summary>
        ///     Gets the hovered map location.
        /// </summary>
        private MapLocation HoverLocation { get; set; }

        /// <summary>
        ///     Gets or sets the selected map location.
        /// </summary>
        public MapLocation SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                _selectedLocation = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the highlighted map location.
        /// </summary>
        public MapLocation HighlightedLocation
        {
            get => _highlightedLocation;
            set
            {
                _highlightedLocation = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets the rectangle within which the map is drawn.
        /// </summary>
        public RectangleF MapRectangle
        {
            get
            {
                if (_map?.Image == null)
                    return RectangleF.Empty;

                var squareX = (double)ClientRectangle.Width / _map.Image.Width;
                var squareY = (double)ClientRectangle.Height / _map.Image.Height;
                var squareSize = (float)Math.Min(squareX, squareY);

                var imgWidth = squareSize * _map.Image.Width;
                var imgHeight = squareSize * _map.Image.Height;

                var dx = (ClientRectangle.Width - imgWidth) / 2;
                var dy = (ClientRectangle.Height - imgHeight) / 2;

                return new RectangleF(dx, dy, imgWidth, imgHeight);
            }
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        public RegionalMapPanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint, true);

            _centered.Alignment = StringAlignment.Center;
            _centered.LineAlignment = StringAlignment.Center;
            _centered.Trimming = StringTrimming.EllipsisWord;
        }

        /// <summary>
        ///     This is called when the selected location has been modified.
        /// </summary>
        public event EventHandler SelectedLocationModified;

        /// <summary>
        ///     Raises the SelectedLocationModified event.
        /// </summary>
        protected void OnSelectedLocationModified()
        {
            SelectedLocationModified?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     This is called when a location has been modified.
        /// </summary>
        public event EventHandler LocationModified;

        /// <summary>
        ///     Raises the LocationModified event.
        /// </summary>
        protected void OnLocationModified()
        {
            LocationModified?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Called in response to the Paint event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                switch (_mode)
                {
                    case MapViewMode.Normal:
                    case MapViewMode.Thumbnail:
                    {
                        var top = Color.FromArgb(240, 240, 240);
                        var bottom = Color.FromArgb(170, 170, 170);
                        Brush background =
                            new LinearGradientBrush(ClientRectangle, top, bottom, LinearGradientMode.Vertical);

                        e.Graphics.FillRectangle(background, ClientRectangle);
                    }
                        break;
                    case MapViewMode.Plain:
                    {
                        e.Graphics.FillRectangle(Brushes.White, ClientRectangle);
                    }
                        break;
                    case MapViewMode.PlayerView:
                    {
                        e.Graphics.FillRectangle(Brushes.Black, ClientRectangle);
                    }
                        break;
                }

                if (_map?.Image == null)
                {
                    e.Graphics.DrawString("(no map selected)", Font, Brushes.Black, ClientRectangle, _centered);
                    return;
                }

                var imgRect = MapRectangle;
                e.Graphics.DrawImage(_map.Image, imgRect);

                if (_showLocations)
                    foreach (var loc in _map.Locations)
                    {
                        if (loc == null)
                            continue;

                        if (_highlightedLocation != null && loc.Id != _highlightedLocation.Id)
                            continue;

                        var c = Color.White;
                        if (loc == HoverLocation)
                            c = Color.Blue;
                        if (loc == _selectedLocation)
                            c = Color.Blue;

                        var locRect = get_loc_rect(loc, imgRect);
                        e.Graphics.DrawEllipse(new Pen(Color.Black, 5), locRect);
                        e.Graphics.DrawEllipse(new Pen(c, 2), locRect);
                    }

                if (_plot != null)
                    foreach (var pp in _plot.Points)
                    {
                        if (pp.RegionalMapId != _map.Id)
                            continue;

                        var source = _map.FindLocation(pp.MapLocationId);
                        if (source == null)
                            continue;

                        var sourcePt = get_loc_pt(source, imgRect);

                        var sourceRect = get_loc_rect(source, imgRect);
                        sourceRect.Inflate(-5, -5);

                        foreach (var link in pp.Links)
                        {
                            var destPoint = _plot.FindPoint(link);
                            if (destPoint == null)
                                continue;

                            if (destPoint.RegionalMapId != _map.Id)
                                continue;

                            var dest = _map.FindLocation(destPoint.MapLocationId);
                            if (dest == null)
                                continue;

                            var destPt = get_loc_pt(dest, imgRect);
                            e.Graphics.DrawLine(new Pen(Color.Red, 3), sourcePt, destPt);

                            var destRect = get_loc_rect(dest, imgRect);
                            destRect.Inflate(-5, -5);

                            e.Graphics.FillEllipse(Brushes.Red, sourceRect);
                            e.Graphics.FillEllipse(Brushes.Red, destRect);

                            // TODO: Draw an arrow
                        }
                    }

                if (_showLocations)
                    foreach (var loc in _map.Locations)
                    {
                        if (_highlightedLocation != null && loc != _highlightedLocation)
                            continue;

                        if (loc != HoverLocation && loc != _selectedLocation && loc != _highlightedLocation)
                            continue;

                        var showCategory = loc.Category != "" &&
                                           (_mode == MapViewMode.Normal || _mode == MapViewMode.Thumbnail);

                        var locRect = get_loc_rect(loc, imgRect);

                        var nameSize = e.Graphics.MeasureString(loc.Name, Font);
                        var catSize = e.Graphics.MeasureString(loc.Category, Font);
                        var textWidth = showCategory ? Math.Max(nameSize.Width, catSize.Width) : nameSize.Width;
                        var textHeight = showCategory ? nameSize.Height + catSize.Height : nameSize.Height;
                        var labelSize = new SizeF(textWidth, textHeight);

                        labelSize.Width += 2;
                        labelSize.Height += 2;

                        var left = locRect.X + locRect.Width / 2 - labelSize.Width / 2;
                        var top = locRect.Top - labelSize.Height - 5;

                        // If it's too high, move it below the location
                        if (top < ClientRectangle.Top)
                            top = locRect.Bottom + 5;

                        // Move left or right
                        left = Math.Max(left, 0);
                        var rightOverlap = left + labelSize.Width - ClientRectangle.Right;
                        if (rightOverlap > 0)
                            left -= rightOverlap;

                        var textRect = new RectangleF(new PointF(left, top), labelSize);

                        var path = RoundedRectangle.Create(textRect, Font.Height * 0.35f);

                        e.Graphics.FillPath(Brushes.LightYellow, path);
                        e.Graphics.DrawPath(Pens.Black, path);

                        if (showCategory)
                        {
                            var height = textRect.Height / 2;
                            var middle = textRect.Y + height;

                            var nameRect = new RectangleF(textRect.X, textRect.Y, textRect.Width, height);
                            var catRect = new RectangleF(textRect.X, middle, textRect.Width, height);

                            e.Graphics.DrawLine(Pens.Gray, textRect.X, middle, textRect.X + textRect.Width, middle);

                            e.Graphics.DrawString(loc.Name, Font, Brushes.Black, nameRect, _centered);
                            e.Graphics.DrawString(loc.Category, Font, Brushes.DarkGray, catRect, _centered);
                        }
                        else
                        {
                            e.Graphics.DrawString(loc.Name, Font, Brushes.Black, textRect, _centered);
                        }
                    }

                if (_mode == MapViewMode.Normal && _map.Locations.Count == 0)
                {
                    const string caption = "Double-click on the map to set a location.";

                    const float delta = 10;
                    var width = ClientRectangle.Width - 2 * delta;
                    var size = e.Graphics.MeasureString(caption, Font, (int)width);
                    var height = size.Height * 2;

                    var rect = new RectangleF(delta, delta, width, height);
                    var path = RoundedRectangle.Create(rect, height / 3);
                    e.Graphics.FillPath(new SolidBrush(Color.FromArgb(200, Color.Black)), path);
                    e.Graphics.DrawPath(Pens.Black, path);
                    e.Graphics.DrawString(caption, Font, Brushes.White, rect, _centered);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        /// <summary>
        ///     Called in response to the MouseMove event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_mode == MapViewMode.Plain || _mode == MapViewMode.PlayerView)
                return;

            // Get location at mouse
            var mouse = PointToClient(Cursor.Position);
            HoverLocation = get_location_at(mouse);

            if (AllowEditing && e.Button == MouseButtons.Left && _selectedLocation != null)
            {
                var pt = get_point(mouse);
                if (pt == PointF.Empty)
                {
                    _selectedLocation = null;
                }
                else
                {
                    _selectedLocation.Point = pt;
                    OnLocationModified();
                }
            }

            Invalidate();
        }

        /// <summary>
        ///     Called in response to the MouseDown event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_mode == MapViewMode.Plain || _mode == MapViewMode.PlayerView)
                return;

            _selectedLocation = HoverLocation;
            OnSelectedLocationModified();

            Invalidate();
        }

        /// <summary>
        ///     Called in response to the MouseLeave event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            if (_mode == MapViewMode.Plain || _mode == MapViewMode.PlayerView)
                return;

            HoverLocation = null;
            Invalidate();
        }

        private static PointF get_loc_pt(MapLocation loc, RectangleF imgRect)
        {
            var x = imgRect.X + imgRect.Width * loc.Point.X;
            var y = imgRect.Y + imgRect.Height * loc.Point.Y;

            return new PointF(x, y);
        }

        private static RectangleF get_loc_rect(MapLocation loc, RectangleF imgRect)
        {
            var pt = get_loc_pt(loc, imgRect);

            return new RectangleF(pt.X - LocationRadius, pt.Y - LocationRadius, 2 * LocationRadius, 2 * LocationRadius);
        }

        private MapLocation get_location_at(Point pt)
        {
            if (_map == null)
                return null;

            var imgRect = MapRectangle;

            return _map.Locations.Select(loc => new { loc, rect = get_loc_rect(loc, imgRect) })
                .Where(t => t.rect.Contains(pt))
                .Select(t => t.loc)
                .FirstOrDefault();
        }

        private PointF get_point(Point pt)
        {
            var imgRect = MapRectangle;

            if (!imgRect.Contains(pt))
                return PointF.Empty;

            var dx = (pt.X - imgRect.X) / imgRect.Width;
            var dy = (pt.Y - imgRect.Y) / imgRect.Height;

            return new PointF(dx, dy);
        }
    }
}
