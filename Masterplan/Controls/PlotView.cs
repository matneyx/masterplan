using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.Controls
{
    /// <summary>
    ///     Settings for displaying plot points on a PlotView control.
    /// </summary>
    public enum PlotViewMode
    {
        /// <summary>
        ///     Display all plot points.
        /// </summary>
        Normal,

        /// <summary>
        ///     Display all plot points on a plain background.
        /// </summary>
        Plain,

        /// <summary>
        ///     Highlight the selected plot point and those connected to it.
        /// </summary>
        HighlightSelected,

        /// <summary>
        ///     Highight plot points containing encounters.
        /// </summary>
        HighlightEncounter,

        /// <summary>
        ///     Highight plot points containing traps.
        /// </summary>
        HighlightTrap,

        /// <summary>
        ///     Highight plot points containing skill challenges.
        /// </summary>
        HighlightChallenge,

        /// <summary>
        ///     Highight plot points containing quests.
        /// </summary>
        HighlightQuest,

        /// <summary>
        ///     Highight plot points containing treasure parcels.
        /// </summary>
        HighlightParcel
    }

    /// <summary>
    ///     Settings for displaying plot point links.
    /// </summary>
    public enum PlotViewLinkStyle
    {
        /// <summary>
        ///     Curved links.
        /// </summary>
        Curved,

        /// <summary>
        ///     Angled links.
        /// </summary>
        Angled,

        /// <summary>
        ///     Straight links.
        /// </summary>
        Straight
    }

    /// <summary>
    ///     A control for displaying the structure of a Plot object
    /// </summary>
    public partial class PlotView : UserControl
    {
        private const int ArrowSize = 6;

        private readonly StringFormat _centered = new StringFormat();
        private Rectangle _fDownRect = Rectangle.Empty;

        private DragLocation _fDragLocation;

        private string _fFilter = "";

        private PlotPoint _fHoverPoint;

        private List<List<PlotPoint>> _fLayers;
        private Dictionary<Guid, Dictionary<Guid, List<PointF>>> _fLinkPaths;

        private PlotViewLinkStyle _fLinkStyle = PlotViewLinkStyle.Curved;

        private PlotViewMode _fMode = PlotViewMode.Normal;

        private Plot _fPlot;

        private PlotPoint _fSelectedPoint;

        private bool _fShowLevels = true;

        private Rectangle _fUpRect = Rectangle.Empty;
        private Dictionary<Guid, RectangleF> _regions;

        /// <summary>
        ///     Gets or sets the Plot to be displayed.
        /// </summary>
        [Category("Data")]
        [Description("The plot to display.")]
        public Plot Plot
        {
            get => _fPlot;
            set
            {
                if (_fPlot != value)
                {
                    _fPlot = value;
                    _fSelectedPoint = null;
                    _fHoverPoint = null;

                    RecalculateLayout();
                    Invalidate();

                    OnSelectionChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the plot display mode.
        /// </summary>
        [Category("Appearance")]
        [Description("How the plot should be displayed.")]
        public PlotViewMode Mode
        {
            get => _fMode;
            set
            {
                _fMode = value;

                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the link display mode.
        /// </summary>
        [Category("Appearance")]
        [Description("How plot point links should be displayed.")]
        public PlotViewLinkStyle LinkStyle
        {
            get => _fLinkStyle;
            set
            {
                _fLinkStyle = value;

                RecalculateLayout();
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the plot point filter.
        ///     Plot points which do not contain this text will not be shown.
        /// </summary>
        [Category("Behavior")]
        [Description("Plot points which do not contain this text are not shown.")]
        public string Filter
        {
            get => _fFilter;
            set
            {
                _fFilter = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether levelling information is shown on the control.
        /// </summary>
        [Category("Appearance")]
        [Description("Determines whether levelling information is shown.")]
        public bool ShowLevels
        {
            get => _fShowLevels;
            set
            {
                _fShowLevels = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the selected plot point.
        /// </summary>
        [Category("Behavior")]
        [Description("The selected point.")]
        public PlotPoint SelectedPoint
        {
            get => _fSelectedPoint;
            set
            {
                if (_fSelectedPoint != value)
                {
                    _fSelectedPoint = value;

                    Invalidate();

                    OnSelectionChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether plot tooltips should be shown.
        /// </summary>
        [Category("Appearance")]
        [Description("Determines whether tooltips are shown.")]
        public bool ShowTooltips { get; set; } = true;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public PlotView()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint
                     | ControlStyles.Selectable, true);

            _centered.Alignment = StringAlignment.Center;
            _centered.LineAlignment = StringAlignment.Center;
            _centered.Trimming = StringTrimming.EllipsisWord;
        }

        /// <summary>
        ///     Handles keyboard navigation.
        /// </summary>
        /// <param name="key">The keypress to handle.</param>
        /// <returns>Returns true if the key was handled; false otherwise.</returns>
        public bool Navigate(Keys key)
        {
            try
            {
                if (SelectedPoint == null)
                    return false;

                var layers = Workspace.FindLayers(_fPlot);

                int currentLayer;
                for (currentLayer = 0; currentLayer != layers.Count; ++currentLayer)
                    if (layers[currentLayer].Contains(SelectedPoint))
                        break;

                if (key == Keys.Up)
                {
                    if (currentLayer != 0)
                    {
                        var layer = layers[currentLayer - 1];

                        foreach (var pp in layer)
                            if (pp.Links.Contains(SelectedPoint.Id))
                            {
                                SelectedPoint = pp;
                                break;
                            }
                    }

                    return true;
                }

                if (key == Keys.Down)
                {
                    if (currentLayer != layers.Count - 1)
                    {
                        var layer = layers[currentLayer + 1];

                        foreach (var pp in layer)
                            if (SelectedPoint.Links.Contains(pp.Id))
                            {
                                SelectedPoint = layer[0];
                                break;
                            }
                    }

                    return true;
                }

                if (key == Keys.Left)
                {
                    var layer = layers[currentLayer];
                    var index = layer.IndexOf(SelectedPoint);
                    if (index != 0)
                        SelectedPoint = layer[index - 1];

                    return true;
                }

                if (key == Keys.Right)
                {
                    var layer = layers[currentLayer];
                    var index = layer.IndexOf(SelectedPoint);
                    if (index != layer.Count - 1)
                        SelectedPoint = layer[index + 1];

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return false;
        }

        /// <summary>
        ///     Invalidates all layout calculations
        /// </summary>
        public void RecalculateLayout()
        {
            clear_layout_calculations();
        }

        /// <summary>
        ///     Occurs when the selected plot point changes.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the selected point changes.")]
        public event EventHandler SelectionChanged;

        /// <summary>
        ///     Raises the SelectionChanged event.
        /// </summary>
        protected void OnSelectionChanged()
        {
            try
            {
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        /// <summary>
        ///     Occurs when the plot layout changes.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the plot layout changes.")]
        public event EventHandler PlotLayoutChanged;

        /// <summary>
        ///     Raises the PlotChanged event.
        /// </summary>
        protected void OnPlotLayoutChanged()
        {
            try
            {
                PlotLayoutChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        /// <summary>
        ///     Occurs when the current plot changes.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the current plot changes.")]
        public event EventHandler PlotChanged;

        /// <summary>
        ///     Raises the PlotChanged event.
        /// </summary>
        protected void OnPlotChanged()
        {
            try
            {
                PlotChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
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

                if (_fLayers == null)
                    do_layout_calculations();

                // Draw background
                if (_fMode == PlotViewMode.Plain)
                {
                    e.Graphics.FillRectangle(SystemBrushes.Window, ClientRectangle);
                }
                else
                {
                    var top = Color.FromArgb(240, 240, 240);
                    var bottom = Color.FromArgb(170, 170, 170);
                    using (Brush background =
                           new LinearGradientBrush(ClientRectangle, top, bottom, LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(background, ClientRectangle);
                    }

                    var mouse = PointToClient(Cursor.Position);

                    var pp = Session.Project.FindParent(_fPlot);
                    if (pp != null)
                        using (var f = new Font(Font.FontFamily, Font.Size * 2))
                        {
                            e.Graphics.DrawString(pp.Name, f, Brushes.DarkGray, ClientRectangle.Left + 10,
                                ClientRectangle.Top + 10);
                        }

                    var upBtn = pp == null ? Color.DarkGray : Color.Black;
                    var downBtn = _fSelectedPoint == null ? Color.DarkGray : Color.Black;

                    using (var upPen = new Pen(upBtn))
                    {
                        using (var downPen = new Pen(downBtn))
                        {
                            using (Brush upBrush = new SolidBrush(upBtn))
                            {
                                using (Brush downBrush = new SolidBrush(downBtn))
                                {
                                    // Draw 'up' arrow
                                    var bottomLeft = new Point(_fUpRect.Left + 5, _fUpRect.Bottom - 5);
                                    var bottomRight = new Point(_fUpRect.Right - 5, _fUpRect.Bottom - 5);
                                    var topCentre = new Point((_fUpRect.Right + _fUpRect.Left) / 2, _fUpRect.Top + 5);
                                    if (_fUpRect.Contains(mouse))
                                        e.Graphics.FillPolygon(upBrush, new[] { bottomLeft, bottomRight, topCentre });
                                    else
                                        e.Graphics.DrawPolygon(upPen, new[] { bottomLeft, bottomRight, topCentre });

                                    // Draw 'down' arrow
                                    var topLeft = new Point(_fDownRect.Left + 5, _fDownRect.Top + 5);
                                    var topRight = new Point(_fDownRect.Right - 5, _fDownRect.Top + 5);
                                    var bottomCentre = new Point((_fDownRect.Right + _fDownRect.Left) / 2,
                                        _fDownRect.Bottom - 5);
                                    if (_fDownRect.Contains(mouse))
                                        e.Graphics.FillPolygon(downBrush, new[] { topLeft, topRight, bottomCentre });
                                    else
                                        e.Graphics.DrawPolygon(downPen, new[] { topLeft, topRight, bottomCentre });
                                }
                            }
                        }
                    }
                }

                if (Session.Project == null)
                {
                    var str = "(no project)";
                    e.Graphics.DrawString(str, Font, SystemBrushes.WindowText, ClientRectangle, _centered);

                    return;
                }

                if (_fPlot == null || _fPlot.Points.Count == 0)
                {
                    var str = "(no plot points)";
                    e.Graphics.DrawString(str, Font, SystemBrushes.WindowText, ClientRectangle, _centered);

                    return;
                }

                if (_fDragLocation != null && _fHoverPoint == null)
                {
                    var midpoint = _fDragLocation.Rect.Left + _fDragLocation.Rect.Width / 2;
                    using (var p2 = new Pen(Color.DarkBlue, 2))
                    {
                        e.Graphics.DrawLine(p2, midpoint, _fDragLocation.Rect.Top, midpoint,
                            _fDragLocation.Rect.Bottom);
                    }

                    float xDelta = 3;

                    using (var p = new Pen(Color.DarkBlue, 1))
                    {
                        var tl = new PointF(midpoint - xDelta, _fDragLocation.Rect.Top);
                        var tr = new PointF(midpoint + xDelta, _fDragLocation.Rect.Top);
                        e.Graphics.DrawLine(p, tl, tr);

                        var bl = new PointF(midpoint - xDelta, _fDragLocation.Rect.Bottom);
                        var br = new PointF(midpoint + xDelta, _fDragLocation.Rect.Bottom);
                        e.Graphics.DrawLine(p, bl, br);
                    }
                }

                // Draw level up lines
                if (_fShowLevels)
                    for (var layerIndex = 0; layerIndex != _fLayers.Count; ++layerIndex)
                    {
                        var layer = _fLayers[layerIndex];

                        var startXp = Workspace.GetTotalXp(layer[0]);
                        var endXp = startXp + Workspace.GetLayerXp(layer);

                        var startLevel = Experience.GetHeroLevel(startXp / Session.Project.Party.Size);
                        var endLevel = Experience.GetHeroLevel(endXp / Session.Project.Party.Size);

                        if (startLevel != endLevel)
                        {
                            // Draw a line under this layer

                            var firstPoint = layer[0];
                            var firstRect = _regions[firstPoint.Id];

                            var index = _fLayers.IndexOf(layer);
                            var y = firstRect.Height * (index * 2 + 2.5F);

                            var start = new PointF(0F, y);
                            var end = new PointF(Width, y);
                            e.Graphics.DrawLine(SystemPens.ControlDarkDark, start, end);

                            var text = new PointF(0F, y - Font.Height);
                            e.Graphics.DrawString("Level " + endLevel, Font, SystemBrushes.WindowText, text);
                        }
                    }

                // Draw links
                foreach (var pp in _fPlot.Points)
                {
                    if (!_regions.ContainsKey(pp.Id))
                        continue;

                    foreach (var id in pp.Links)
                    {
                        if (!_regions.ContainsKey(id))
                            continue;

                        // Draw this link

                        var fromRect = _regions[pp.Id];
                        var toRect = _regions[id];

                        var fromDelta = new PointF(fromRect.X + fromRect.Width / 2, fromRect.Bottom);
                        var fromPt = new PointF(fromRect.X + fromRect.Width / 2, fromRect.Bottom + ArrowSize * 2);
                        var toPt = new PointF(toRect.X + toRect.Width / 2, toRect.Top - ArrowSize * 2);
                        var toDelta = new PointF(toPt.X, toPt.Y + ArrowSize);
                        new PointF(toRect.X + toRect.Width / 2, toRect.Top);

                        var linkAlpha = 130;
                        float linkWidth = 2;

                        var toPp = _fPlot.FindPoint(id);
                        if (draw_link(pp, toPp))
                        {
                            var left = new PointF(toDelta.X - ArrowSize, toDelta.Y);
                            var right = new PointF(toDelta.X + ArrowSize, toDelta.Y);
                            var bottom = new PointF(toDelta.X, toDelta.Y + ArrowSize);

                            e.Graphics.FillPolygon(SystemBrushes.Window, new[] { left, right, bottom });
                            e.Graphics.DrawPolygon(Pens.Maroon, new[] { left, right, bottom });
                        }
                        else
                        {
                            linkAlpha = 60;
                            linkWidth = 0.5F;
                            toDelta = new PointF(toPt.X, toRect.Top);
                        }

                        using (var linkPen = new Pen(Color.FromArgb(linkAlpha, Color.Maroon), linkWidth))
                        {
                            e.Graphics.DrawLine(linkPen, fromDelta, fromPt);
                            e.Graphics.DrawLine(linkPen, toPt, toDelta);

                            switch (_fLinkStyle)
                            {
                                case PlotViewLinkStyle.Curved:
                                {
                                    var drawn = false;

                                    if (_fLinkPaths.ContainsKey(pp.Id))
                                    {
                                        var linkPaths = _fLinkPaths[pp.Id];
                                        if (linkPaths.ContainsKey(id))
                                        {
                                            var path = linkPaths[id];
                                            e.Graphics.DrawCurve(linkPen, path.ToArray());
                                            drawn = true;
                                        }
                                    }

                                    if (!drawn)
                                        e.Graphics.DrawLine(linkPen, fromPt, toPt);
                                }
                                    break;
                                case PlotViewLinkStyle.Angled:
                                {
                                    var drawn = false;

                                    if (_fLinkPaths.ContainsKey(pp.Id))
                                    {
                                        var linkPaths = _fLinkPaths[pp.Id];
                                        if (linkPaths.ContainsKey(id))
                                        {
                                            var path = linkPaths[id];
                                            e.Graphics.DrawLines(linkPen, path.ToArray());
                                            drawn = true;
                                        }
                                    }

                                    if (!drawn)
                                        e.Graphics.DrawLine(linkPen, fromPt, toPt);
                                }
                                    break;
                                case PlotViewLinkStyle.Straight:
                                {
                                    e.Graphics.DrawLine(linkPen, fromPt, toPt);
                                }
                                    break;
                            }
                        }
                    }
                }

                // Draw plot points
                foreach (var pp in _fPlot.Points)
                {
                    if (!_regions.ContainsKey(pp.Id))
                        continue;

                    var pointRect = _regions[pp.Id];

                    var alpha = 255;
                    if (pp.State != PlotPointState.Normal)
                        alpha = 50;

                    Brush pointBackground = null;
                    if (pp == _fSelectedPoint)
                    {
                        pointBackground = Brushes.White;
                    }
                    else
                    {
                        var gradient = GetColourGradient(pp.Colour, alpha);
                        pointBackground = new LinearGradientBrush(pointRect, gradient.First, gradient.Second,
                            LinearGradientMode.Vertical);
                    }

                    var outline = pp == _fHoverPoint ? SystemPens.Highlight : SystemPens.WindowText;
                    var font = pp != _fSelectedPoint ? Font : new Font(Font, Font.Style | FontStyle.Bold);
                    if (pp.State == PlotPointState.Skipped)
                        font = new Font(font, Font.Style | FontStyle.Strikeout);

                    if (pp.Element != null)
                    {
                        var level = Workspace.GetPartyLevel(pp);

                        var diff = pp.Element.GetDifficulty(level, Session.Project.Party.Size);
                        if (diff == Difficulty.Trivial || diff == Difficulty.Extreme)
                            if (pp != _fSelectedPoint)
                            {
                                pointBackground = new SolidBrush(Color.FromArgb(alpha, Color.Pink));
                                outline = Pens.Red;
                            }
                    }

                    if (draw_point(pp))
                    {
                        var text = SystemBrushes.WindowText;

                        if (pp.State == PlotPointState.Normal)
                        {
                            // Draw shadow
                            var shadow = new RectangleF(pointRect.Location, pointRect.Size);
                            shadow.Offset(3, 4);
                            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
                            {
                                e.Graphics.FillRectangle(shadowBrush, shadow);
                            }
                        }
                        else
                        {
                            if (pp != _fSelectedPoint)
                                text = new SolidBrush(Color.FromArgb(alpha, Color.Black));
                        }

                        // Draw plot point
                        e.Graphics.FillRectangle(pointBackground, pointRect);
                        e.Graphics.DrawRectangle(outline, pointRect.X, pointRect.Y, pointRect.Width, pointRect.Height);

                        var fontSize = font.Size;
                        while (e.Graphics.MeasureString(pp.Name, font, (int)pointRect.Width).Height > pointRect.Height)
                        {
                            fontSize *= 0.95F;
                            font = new Font(font.FontFamily, fontSize, font.Style);
                        }

                        e.Graphics.DrawString(pp.Name, font, text, pointRect, _centered);

                        if (pp.Subplot.Points.Count > 0)
                        {
                            pointRect = RectangleF.Inflate(pointRect, -2, -2);
                            e.Graphics.DrawRectangle(outline, pointRect.X, pointRect.Y, pointRect.Width,
                                pointRect.Height);
                        }

                        if (pp.Details != "" || pp.ReadAloud != "")
                        {
                            const int deltaX = 20;
                            const int deltaY = 5;

                            var theta = Math.Atan((double)deltaY / deltaX) * 2;
                            var dx = deltaX - (float)(deltaX * Math.Cos(theta));
                            var dy = (float)(deltaX * Math.Sin(theta));

                            var bottom = new PointF(pointRect.Right - deltaX, pointRect.Bottom);
                            var right = new PointF(pointRect.Right, pointRect.Bottom - deltaY);
                            var top = new PointF(pointRect.Right - dx, pointRect.Bottom - dy);
                            var corner = new PointF(pointRect.Right, pointRect.Bottom);

                            e.Graphics.DrawPolygon(Pens.Gray, new[] { top, right, bottom });
                            using (Brush b = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                            {
                                e.Graphics.FillPolygon(b, new[] { right, bottom, corner });
                            }
                        }
                    }
                    else
                    {
                        // Draw outline only
                        using (new Pen(Color.FromArgb(60, outline.Color)))
                        {
                            e.Graphics.DrawRectangle(outline, pointRect.X, pointRect.Y, pointRect.Width,
                                pointRect.Height);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void clear_layout_calculations()
        {
            _fUpRect = Rectangle.Empty;
            _fDownRect = Rectangle.Empty;

            _fLayers = null;
            _regions = null;
            _fLinkPaths = null;
        }

        private void do_layout_calculations()
        {
            try
            {
                clear_layout_calculations();

                _fUpRect = new Rectangle(ClientRectangle.Right - 35, ClientRectangle.Top + 15, 25, 20);
                _fDownRect = new Rectangle(ClientRectangle.Right - 35, ClientRectangle.Top + 40, 25, 20);

                // Start by determining layers
                _fLayers = Workspace.FindLayers(_fPlot);

                // Determine plot point locations
                _regions = new Dictionary<Guid, RectangleF>();
                var levels = _fLayers.Count * 2 + 1;
                var height = (float)(ClientRectangle.Height - 1) / levels;
                foreach (var layer in _fLayers)
                {
                    var layerIndex = _fLayers.IndexOf(layer) * 2 + 1;

                    var y = layerIndex * height;
                    var layerRect = new RectangleF(ClientRectangle.X, y, ClientRectangle.Width, height);

                    var layerSpaces = layer.Count * 2 + 1;
                    var width = layerRect.Width / layerSpaces;

                    foreach (var pp in layer)
                    {
                        var pointIndex = layer.IndexOf(pp) * 2 + 1;

                        var x = pointIndex * width;
                        var pointRect = new RectangleF(x, y, width, height);

                        _regions[pp.Id] = pointRect;
                    }
                }

                // Calculate link paths
                if (_fLinkStyle != PlotViewLinkStyle.Straight)
                {
                    _fLinkPaths = new Dictionary<Guid, Dictionary<Guid, List<PointF>>>();
                    foreach (var pp in _fPlot.Points)
                    {
                        if (!_regions.ContainsKey(pp.Id))
                            continue;

                        var linkPaths = new Dictionary<Guid, List<PointF>>();

                        foreach (var id in pp.Links)
                        {
                            if (!_regions.ContainsKey(id))
                                continue;

                            var from = _regions[pp.Id];
                            var to = _regions[id];

                            var fromPt = new PointF(from.X + @from.Width / 2, from.Bottom + ArrowSize * 2);
                            var toPt = new PointF(to.X + to.Width / 2, to.Top - ArrowSize * 2);

                            var points = new List<PointF>();
                            points.Add(fromPt);

                            var finished = false;
                            while (!finished)
                            {
                                PlotPoint inTheWay = null;
                                var layerDiff = find_layer_index(id, _fLayers) - find_layer_index(pp.Id, _fLayers);
                                if (layerDiff > 1)
                                    inTheWay = get_blocking_point(fromPt, toPt);

                                if (inTheWay == null)
                                {
                                    finished = true;
                                }
                                else
                                {
                                    var pointRect = _regions[inTheWay.Id];

                                    var layerIndex = find_layer_index(inTheWay.Id, _fLayers);
                                    var layer = _fLayers[layerIndex];

                                    var delta = pointRect.Width / 3;
                                    if (layer.Count == 1)
                                        delta = pointRect.Width / 6;

                                    var midpoint = (float)Math.Round(pointRect.Left + pointRect.Width / 2);
                                    var x = midpoint >= toPt.X ? pointRect.Left - delta : pointRect.Right + delta;

                                    var top = new PointF(x, pointRect.Top);
                                    var bottom = new PointF(x, pointRect.Bottom);

                                    points.Add(top);
                                    points.Add(bottom);

                                    fromPt = bottom;
                                }
                            }

                            points.Add(toPt);
                            linkPaths[id] = points;
                        }

                        _fLinkPaths[pp.Id] = linkPaths;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        /// <summary>
        ///     Called in response to the Resize event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnResize(EventArgs e)
        {
            try
            {
                base.OnResize(e);

                clear_layout_calculations();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        /// <summary>
        ///     Called in response to the MouseLeave event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            try
            {
                _fHoverPoint = null;

                Invalidate();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        /// <summary>
        ///     Called in response to the MouseDown event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            try
            {
                if (_fMode == PlotViewMode.Plain)
                    return;

                var mouse = PointToClient(Cursor.Position);

                if (_fUpRect.Contains(mouse))
                {
                    // Move up
                    var parentPoint = Session.Project.FindParent(_fPlot);
                    if (parentPoint != null)
                    {
                        var p = Session.Project.FindParent(parentPoint);
                        if (p != null)
                        {
                            Plot = p;
                            OnPlotChanged();
                        }
                    }
                }

                if (_fDownRect.Contains(mouse))
                    // Move down
                    if (_fSelectedPoint != null)
                    {
                        Plot = _fSelectedPoint.Subplot;
                        OnPlotChanged();
                    }

                var pp = find_point_at(mouse);
                if (_fSelectedPoint != pp)
                {
                    _fSelectedPoint = pp;
                    Invalidate();

                    OnSelectionChanged();
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
            try
            {
                if (_fMode == PlotViewMode.Plain)
                    return;

                var mouse = PointToClient(Cursor.Position);
                var hovered = find_point_at(mouse);
                if (_fHoverPoint != hovered)
                {
                    _fHoverPoint = hovered;
                    set_tooltip();
                }

                if (e.Button == MouseButtons.Left && _fSelectedPoint != null)
                    DoDragDrop(_fSelectedPoint, DragDropEffects.All);

                Invalidate();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        /// <summary>
        ///     Called in response to the DragOver event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            try
            {
                var mouse = PointToClient(Cursor.Position);
                _fHoverPoint = find_point_at(mouse);

                var dragged = e.Data.GetData(typeof(PlotPoint)) as PlotPoint;

                e.Effect = DragDropEffects.None;
                if (_fHoverPoint != null)
                {
                    if (allow_drop(dragged, _fHoverPoint))
                        e.Effect = DragDropEffects.Move;
                }
                else
                {
                    _fDragLocation = allow_drop(dragged, mouse);

                    if (_fDragLocation != null)
                        e.Effect = DragDropEffects.Move;
                }

                Invalidate();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        /// <summary>
        ///     Called in response to the DragDrop event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnDragDrop(DragEventArgs e)
        {
            try
            {
                var dragged = e.Data.GetData(typeof(PlotPoint)) as PlotPoint;

                if (_fHoverPoint != null)
                {
                    if (allow_drop(dragged, _fHoverPoint))
                    {
                        // Create link from target to source
                        _fHoverPoint.Links.Add(dragged.Id);

                        OnPlotLayoutChanged();
                    }
                }
                else
                {
                    if (_fDragLocation != null)
                    {
                        _fPlot.Points.Remove(dragged);

                        if (_fDragLocation.Rhs != null)
                        {
                            var index = _fPlot.Points.IndexOf(_fDragLocation.Rhs);
                            _fPlot.Points.Insert(index, dragged);

                            OnPlotLayoutChanged();
                        }
                        else
                        {
                            _fPlot.Points.Add(dragged);

                            OnPlotLayoutChanged();
                        }
                    }

                    _fDragLocation = null;
                }

                do_layout_calculations();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private bool allow_drop(PlotPoint dragged, PlotPoint target)
        {
            try
            {
                // Check that the target is not the same as the dragged point
                if (dragged == target)
                    return false;

                // Check that the target is not the parent of the dragged point
                if (target.Links.Contains(dragged.Id))
                    return false;

                // Check that the target is not in the dragged point's subtree
                var subtree = _fPlot.FindSubtree(dragged);
                if (subtree.Contains(target))
                    return false;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return true;
        }

        private DragLocation allow_drop(PlotPoint dragged, Point pt)
        {
            try
            {
                var draggedRect = _regions[dragged.Id];
                var layerRect = new RectangleF(0, draggedRect.Y, ClientRectangle.Width, draggedRect.Height);

                if (!layerRect.Contains(pt))
                    return null;

                // Find all the points on this layer
                var points = new List<PlotPoint>();
                foreach (var pp in _fPlot.Points)
                {
                    var pointRect = _regions[pp.Id];

                    if (!layerRect.Contains(pointRect))
                        continue;

                    if (pointRect.Contains(pt))
                        return null;

                    points.Add(pp);
                }

                if (points.Count == 0)
                    return null;

                var pairs = new List<Pair<PlotPoint, PlotPoint>>();
                foreach (var pp in points)
                {
                    var index = points.IndexOf(pp);

                    if (index == 0)
                        pairs.Add(new Pair<PlotPoint, PlotPoint>(null, pp));
                    else
                        pairs.Add(new Pair<PlotPoint, PlotPoint>(points[index - 1], pp));

                    if (index == points.Count - 1)
                        pairs.Add(new Pair<PlotPoint, PlotPoint>(pp, null));
                }

                foreach (var pair in pairs)
                {
                    if (pair.First == dragged || pair.Second == dragged)
                        continue;

                    float left = 0;
                    float right = ClientRectangle.Width;
                    if (pair.First != null)
                    {
                        var leftRect = _regions[pair.First.Id];
                        left = leftRect.Right;
                    }

                    if (pair.Second != null)
                    {
                        var rightRect = _regions[pair.Second.Id];
                        right = rightRect.Left;
                    }

                    var rect = new RectangleF(left, layerRect.Y, right - left, layerRect.Height);
                    if (rect.Contains(pt))
                    {
                        var db = new DragLocation();
                        db.Lhs = pair.First;
                        db.Rhs = pair.Second;
                        db.Rect = rect;
                        return db;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return null;
        }

        private PlotPoint find_point_at(Point pt)
        {
            try
            {
                if (_regions == null)
                    do_layout_calculations();

                foreach (var id in _regions.Keys)
                {
                    var rect = _regions[id];

                    if (rect.Contains(pt))
                        return _fPlot.FindPoint(id);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return null;
        }

        private bool draw_point(PlotPoint pp)
        {
            try
            {
                if (_fFilter != "")
                    return match_filter(pp);

                if (_fMode == PlotViewMode.HighlightSelected)
                    return _fSelectedPoint == null || pp == _fSelectedPoint || _fSelectedPoint.Links.Contains(pp.Id) ||
                           pp.Links.Contains(_fSelectedPoint.Id);

                if (_fMode == PlotViewMode.HighlightEncounter)
                    return pp.Element is Encounter;

                if (_fMode == PlotViewMode.HighlightTrap)
                {
                    if (pp.Element == null)
                        return false;

                    if (pp.Element is TrapElement)
                        return true;

                    if (pp.Element is Encounter)
                    {
                        var enc = pp.Element as Encounter;
                        return enc.Traps.Count != 0;
                    }
                }

                if (_fMode == PlotViewMode.HighlightChallenge)
                {
                    if (pp.Element == null)
                        return false;

                    if (pp.Element is SkillChallenge)
                        return true;

                    if (pp.Element is Encounter)
                    {
                        var enc = pp.Element as Encounter;
                        return enc.SkillChallenges.Count != 0;
                    }
                }

                if (_fMode == PlotViewMode.HighlightQuest)
                    return pp.Element is Quest;

                if (_fMode == PlotViewMode.HighlightParcel)
                    return pp.Parcels.Count != 0;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return true;
        }

        private bool draw_link(PlotPoint pp1, PlotPoint pp2)
        {
            try
            {
                if (_fFilter != "")
                    return draw_point(pp1) && draw_point(pp2);

                if (_fMode == PlotViewMode.HighlightSelected)
                    return _fSelectedPoint == null || pp1 == _fSelectedPoint || pp2 == _fSelectedPoint;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return true;
        }

        private bool match_filter(PlotPoint pp)
        {
            try
            {
                var tokens = _fFilter.Split();

                foreach (var token in tokens)
                    if (!match_token(pp, token))
                        return false;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return true;
        }

        private bool match_token(PlotPoint pp, string token)
        {
            try
            {
                token = token.ToLower();

                if (pp.Name.ToLower().Contains(token))
                    return true;

                if (pp.Details.ToLower().Contains(token))
                    return true;

                if (pp.ReadAloud.ToLower().Contains(token))
                    return true;

                if (pp.Element is Encounter)
                {
                    var enc = pp.Element as Encounter;

                    foreach (var slot in enc.Slots)
                    {
                        var t = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                        if (t.Name.ToLower().Contains(token))
                            return true;
                    }

                    foreach (var note in enc.Notes)
                        if (note.Contents.ToLower().Contains(token))
                            return true;
                }

                if (pp.Element is SkillChallenge)
                {
                    var sc = pp.Element as SkillChallenge;

                    if (sc.Success.ToLower().Contains(token))
                        return true;

                    if (sc.Failure.ToLower().Contains(token))
                        return true;

                    foreach (var scd in sc.Skills)
                    {
                        if (scd.SkillName.ToLower().Contains(token))
                            return true;

                        if (scd.Details.ToLower().Contains(token))
                            return true;
                    }
                }

                if (pp.Element is TrapElement)
                {
                    var te = pp.Element as TrapElement;

                    if (te.Trap.Name.ToLower().Contains(token))
                        return true;

                    foreach (var tsd in te.Trap.Skills)
                    {
                        if (tsd.SkillName.ToLower().Contains(token))
                            return true;

                        if (tsd.Details.ToLower().Contains(token))
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return false;
        }

        private void set_tooltip()
        {
            try
            {
                if (ShowTooltips && _fHoverPoint != null)
                {
                    var contents = new List<string>();

                    if (_fHoverPoint.Element != null)
                    {
                        if (_fHoverPoint.Element is Encounter)
                        {
                            var enc = _fHoverPoint.Element as Encounter;

                            var str = "Encounter: " + enc.GetXp() + " XP";

                            foreach (var slot in enc.Slots)
                            {
                                var t = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);

                                if (t != null)
                                {
                                    str += Environment.NewLine + t.Name;
                                    if (slot.CombatData.Count > 1)
                                        str += " (x" + slot.CombatData.Count + ")";
                                }
                            }

                            foreach (var trap in enc.Traps) str += Environment.NewLine + trap.Name + ": " + trap.Info;

                            foreach (var sc in enc.SkillChallenges)
                                str += Environment.NewLine + sc.Name + ": " + sc.Info;

                            contents.Add(str);
                        }

                        if (_fHoverPoint.Element is TrapElement)
                        {
                            var te = _fHoverPoint.Element as TrapElement;

                            var str = te.Trap.Name + ": " + te.GetXp() + " XP";
                            str += Environment.NewLine + te.Trap.Info + " " + te.Trap.Type.ToString().ToLower();

                            contents.Add(str);
                        }

                        if (_fHoverPoint.Element is SkillChallenge)
                        {
                            var sc = _fHoverPoint.Element as SkillChallenge;

                            var str = sc.Name + ": " + sc.GetXp() + " XP";
                            str += Environment.NewLine + sc.Info;

                            contents.Add(str);
                        }

                        if (_fHoverPoint.Element is Quest)
                        {
                            var q = _fHoverPoint.Element as Quest;

                            var str = "";
                            switch (q.Type)
                            {
                                case QuestType.Major:
                                    str = "Major quest: " + q.GetXp() + " XP";
                                    break;
                                case QuestType.Minor:
                                    str = "Minor quest: " + q.GetXp() + " XP";
                                    break;
                            }

                            contents.Add(str);
                        }
                    }

                    var parcels = "";
                    foreach (var p in _fHoverPoint.Parcels)
                    {
                        if (parcels != "")
                            parcels += ", ";

                        parcels += p.Name;
                    }

                    if (parcels != "")
                        contents.Add("Treasure parcels: " + parcels);

                    var s = "";
                    foreach (var str in contents)
                    {
                        if (s != "")
                            s += Environment.NewLine + Environment.NewLine;

                        s += TextHelper.Wrap(str);
                    }

                    Tooltip.ToolTipTitle = _fHoverPoint.Name;
                    Tooltip.ToolTipIcon = ToolTipIcon.Info;
                    Tooltip.SetToolTip(this, s);
                }
                else
                {
                    Tooltip.ToolTipTitle = "";
                    Tooltip.ToolTipIcon = ToolTipIcon.None;
                    Tooltip.SetToolTip(this, null);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private int find_layer_index(Guid pointId, List<List<PlotPoint>> layers)
        {
            try
            {
                for (var index = 0; index != layers.Count; ++index)
                {
                    var layer = layers[index];
                    foreach (var point in layer)
                        if (point.Id == pointId)
                            return index;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return -1;
        }

        private PlotPoint get_blocking_point(PointF fromPt, PointF toPt)
        {
            try
            {
                // First, work out what layer we're in / about to enter
                var layer = find_layer(fromPt.Y);
                var targetLayer = find_layer(toPt.Y);
                if (layer == null || layer == targetLayer)
                    return null;

                if (layer != null)
                {
                    var startIndex = _fLayers.IndexOf(layer);
                    var targetIndex = _fLayers.IndexOf(targetLayer);

                    var max = Math.Min(targetIndex, _fLayers.Count) - 1;
                    for (var index = startIndex; index <= max; ++index)
                    {
                        layer = _fLayers[index];

                        // Work out the x co-ordinates where we enter / leave this layer
                        var first = layer[0];
                        var firstRect = _regions[first.Id];
                        var yTop = firstRect.Top;
                        var yBottom = firstRect.Bottom;

                        var xDist = toPt.X - fromPt.X;
                        var yDist = toPt.Y - fromPt.Y;

                        var pTop = (yTop - fromPt.Y) / yDist;
                        var pBottom = (yBottom - fromPt.Y) / yDist;

                        var xTop = fromPt.X + pTop * xDist;
                        var xBottom = fromPt.X + pBottom * xDist;

                        var enter = new PointF(xTop, yTop);
                        var leave = new PointF(xBottom, yBottom);

                        foreach (var point in layer)
                        {
                            var rect = _regions[point.Id];
                            var largerRect = RectangleF.Inflate(rect, 2, 2);

                            if (largerRect.Contains(enter) || largerRect.Contains(leave))
                                return point;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return null;
        }

        private List<PlotPoint> find_layer(float y)
        {
            try
            {
                foreach (var layer in _fLayers)
                {
                    var first = layer[0];
                    var firstRect = _regions[first.Id];

                    if (y < firstRect.Bottom)
                        return layer;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return null;
        }

        internal static Pair<Color, Color> GetColourGradient(PlotPointColour colour, int alpha)
        {
            var top = Color.White;
            var bottom = Color.Black;

            switch (colour)
            {
                case PlotPointColour.Yellow:
                    top = Color.FromArgb(alpha, 255, 255, 215);
                    bottom = Color.FromArgb(alpha, 255, 255, 165);
                    break;
                case PlotPointColour.Blue:
                    top = Color.FromArgb(alpha, 215, 215, 255);
                    bottom = Color.FromArgb(alpha, 165, 165, 255);
                    break;
                case PlotPointColour.Green:
                    top = Color.FromArgb(alpha, 215, 255, 215);
                    bottom = Color.FromArgb(alpha, 165, 255, 165);
                    break;
                case PlotPointColour.Purple:
                    top = Color.FromArgb(alpha, 240, 205, 255);
                    bottom = Color.FromArgb(alpha, 240, 150, 255);
                    break;
                case PlotPointColour.Orange:
                    top = Color.FromArgb(alpha, 255, 240, 210);
                    bottom = Color.FromArgb(alpha, 255, 165, 120);
                    break;
                case PlotPointColour.Brown:
                    top = Color.FromArgb(alpha, 255, 240, 215);
                    bottom = Color.FromArgb(alpha, 170, 140, 110);
                    break;
                case PlotPointColour.Grey:
                    top = Color.FromArgb(alpha, 225, 225, 225);
                    bottom = Color.FromArgb(alpha, 175, 175, 175);
                    break;
            }

            return new Pair<Color, Color>(top, bottom);
        }

        private class DragLocation
        {
            public PlotPoint Lhs;

            public RectangleF Rect = RectangleF.Empty;
            public PlotPoint Rhs;
        }
    }
}
