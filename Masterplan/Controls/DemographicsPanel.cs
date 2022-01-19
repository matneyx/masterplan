using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.Controls
{
    /// <summary>
    ///     Different types of information that can be shown in a DemographicsPanel control.
    /// </summary>
    public enum DemographicsSource
    {
        /// <summary>
        ///     Show creature breakdown.
        /// </summary>
        Creatures,

        /// <summary>
        ///     Show trap breakdown.
        /// </summary>
        Traps,

        /// <summary>
        ///     Show magic item breakdown.
        /// </summary>
        MagicItems
    }

    /// <summary>
    ///     Different types of breakdown that can be used in a DemographicsPanel control.
    /// </summary>
    public enum DemographicsMode
    {
        /// <summary>
        ///     Show breakdown by level.
        /// </summary>
        Level,

        /// <summary>
        ///     Show breakdown by role.
        /// </summary>
        Role,

        /// <summary>
        ///     Show breakdown by standard / elite / solo / minion.
        /// </summary>
        Status
    }

    /// <summary>
    ///     Panel to show various breakdowns of creatures in a library.
    /// </summary>
    public partial class DemographicsPanel : UserControl
    {
        private readonly StringFormat _centered = new StringFormat();

        private Dictionary<string, int> _breakdown;

        private Library _fLibrary;

        private DemographicsMode _fMode = DemographicsMode.Level;

        private DemographicsSource _fSource = DemographicsSource.Creatures;

        /// <summary>
        ///     Gets or sets the library to display.
        /// </summary>
        [Category("Data")]
        [Description("The library to display.")]
        public Library Library
        {
            get => _fLibrary;
            set
            {
                _fLibrary = value;
                _breakdown = null;

                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the category of information to show.
        /// </summary>
        [Category("Appearance")]
        [Description("The category of information to show.")]
        public DemographicsSource Source
        {
            get => _fSource;
            set
            {
                _fSource = value;
                _breakdown = null;

                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the type of breakdown to show.
        /// </summary>
        [Category("Appearance")]
        [Description("The type of breakdown to show.")]
        public DemographicsMode Mode
        {
            get => _fMode;
            set
            {
                _fMode = value;
                _breakdown = null;

                Invalidate();
            }
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public DemographicsPanel()
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

        internal void ShowTable(ReportTable table)
        {
            _breakdown = new Dictionary<string, int>();

            foreach (var row in table.Rows) _breakdown[row.Heading] = row.Total;

            Invalidate();
        }

        /// <summary>
        ///     Called in response to the Paint event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (_breakdown == null)
                analyse_data();

            e.Graphics.FillRectangle(Brushes.White, ClientRectangle);

            var columnCount = _breakdown.Keys.Count;
            if (columnCount == 0)
                return;

            var minValue = 0;
            var maxValue = 0;
            foreach (var key in _breakdown.Keys)
            {
                var value = _breakdown[key];

                maxValue = Math.Max(maxValue, value);
                minValue = Math.Min(minValue, value);
            }

            var range = maxValue - minValue;
            if (range == 0)
                return;

            var border = 20;
            var rect = new Rectangle(border, border, ClientRectangle.Width - 2 * border,
                ClientRectangle.Height - 3 * border);
            var barWidth = (float)rect.Width / columnCount;

            var labels = new List<string>();
            labels.AddRange(_breakdown.Keys);

            using (var countFont = new Font(Font.FontFamily, Font.Size * 0.8f))
            {
                for (var columnIndex = 0; columnIndex != labels.Count; ++columnIndex)
                {
                    var label = labels[columnIndex];

                    var x = barWidth * columnIndex;
                    var labelRect = new RectangleF(rect.Left + x, rect.Bottom, barWidth, border);
                    e.Graphics.DrawString(label, countFont, Brushes.Black, labelRect, _centered);

                    var value = _breakdown[label];
                    if (value != 0)
                    {
                        var topColour = value >= 0 ? Color.LightGray : Color.White;
                        var bottomColour = value >= 0 ? Color.White : Color.LightGray;

                        var top = Math.Max(value, 0);
                        var bottom = Math.Min(value, 0);

                        var topY = rect.Bottom - (rect.Height - border) * (top - minValue) / range;
                        var height = (rect.Height - border) * (top - bottom) / range;
                        var bar = new RectangleF(rect.Left + x, topY, barWidth, height);

                        using (Brush barFill =
                               new LinearGradientBrush(bar, topColour, bottomColour, LinearGradientMode.Vertical))
                        {
                            e.Graphics.FillRectangle(barFill, bar);
                            e.Graphics.DrawRectangle(Pens.Gray, bar.X, bar.Y, bar.Width, bar.Height);
                        }

                        var countRect = new RectangleF(rect.Left + x, rect.Top, barWidth, border);
                        e.Graphics.DrawString(value.ToString(), countFont, Brushes.Gray, countRect, _centered);
                    }
                }
            }

            var zeroY = rect.Bottom - (rect.Height - border) * (0 - minValue) / range;

            e.Graphics.DrawLine(Pens.Black, rect.Left, zeroY, rect.Right, zeroY);
            e.Graphics.DrawLine(Pens.Black, rect.Left, rect.Bottom, rect.Left, rect.Top);
        }

        private void analyse_data()
        {
            try
            {
                var libraries = new List<Library>();
                if (_fLibrary == null)
                    libraries.AddRange(Session.Libraries);
                else
                    libraries.Add(_fLibrary);

                _breakdown = new Dictionary<string, int>();

                set_labels(libraries);

                foreach (var lib in libraries)
                    add_library(lib);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void set_labels(List<Library> libraries)
        {
            switch (_fMode)
            {
                case DemographicsMode.Level:
                {
                    var maxLevel = find_max_level(_fSource, libraries);
                    for (var n = 1; n <= maxLevel; ++n)
                        _breakdown[n.ToString()] = 0;
                }
                    break;
                case DemographicsMode.Role:
                {
                    switch (_fSource)
                    {
                        case DemographicsSource.Creatures:
                        {
                            _breakdown["Artillery"] = 0;
                            _breakdown["Brute"] = 0;
                            _breakdown["Controller"] = 0;
                            _breakdown["Lurker"] = 0;
                            _breakdown["Skirmisher"] = 0;
                            _breakdown["Soldier"] = 0;
                        }
                            break;
                        case DemographicsSource.Traps:
                        {
                            _breakdown["Blaster"] = 0;
                            _breakdown["Lurker"] = 0;
                            _breakdown["Obstacle"] = 0;
                            _breakdown["Warder"] = 0;
                        }
                            break;
                    }
                }
                    break;
                case DemographicsMode.Status:
                {
                    _breakdown["Standard"] = 0;
                    _breakdown["Elite"] = 0;
                    _breakdown["Solo"] = 0;
                    _breakdown["Minion"] = 0;
                    _breakdown["Leader"] = 0;
                }
                    break;
            }
        }

        private int find_max_level(DemographicsSource source, List<Library> libraries)
        {
            var maxLevel = 0;

            foreach (var lib in libraries)
                switch (source)
                {
                    case DemographicsSource.Creatures:
                    {
                        foreach (var c in lib.Creatures)
                            if (c.Level > maxLevel)
                                maxLevel = c.Level;
                    }
                        break;
                    case DemographicsSource.Traps:
                    {
                        foreach (var t in lib.Traps)
                            if (t.Level > maxLevel)
                                maxLevel = t.Level;
                    }
                        break;
                    case DemographicsSource.MagicItems:
                    {
                        foreach (var mi in lib.MagicItems)
                            if (mi.Level > maxLevel)
                                maxLevel = mi.Level;
                    }
                        break;
                }

            return maxLevel;
        }

        private void add_library(Library library)
        {
            switch (_fSource)
            {
                case DemographicsSource.Creatures:
                {
                    foreach (var c in library.Creatures)
                        switch (_fMode)
                        {
                            case DemographicsMode.Level:
                            {
                                Add(c.Level.ToString());
                            }
                                break;
                            case DemographicsMode.Role:
                            case DemographicsMode.Status:
                            {
                                analyse_role(c.Role);
                            }
                                break;
                        }
                }
                    break;
                case DemographicsSource.Traps:
                {
                    foreach (var t in library.Traps)
                        switch (_fMode)
                        {
                            case DemographicsMode.Level:
                            {
                                Add(t.Level.ToString());
                            }
                                break;
                            case DemographicsMode.Role:
                            case DemographicsMode.Status:
                            {
                                analyse_role(t.Role);
                            }
                                break;
                        }
                }
                    break;
                case DemographicsSource.MagicItems:
                {
                    foreach (var mi in library.MagicItems)
                        switch (_fMode)
                        {
                            case DemographicsMode.Level:
                            {
                                Add(mi.Level.ToString());
                            }
                                break;
                        }
                }
                    break;
            }
        }

        private void analyse_role(IRole role)
        {
            var cr = role as ComplexRole;
            if (cr != null)
                switch (_fMode)
                {
                    case DemographicsMode.Role:
                    {
                        Add(cr.Type.ToString());
                    }
                        break;
                    case DemographicsMode.Status:
                    {
                        Add(cr.Flag.ToString());

                        if (cr.Leader)
                            Add("Leader");
                    }
                        break;
                }

            var m = role as Minion;
            if (m != null)
                switch (_fMode)
                {
                    case DemographicsMode.Role:
                    {
                        if (m.HasRole)
                            Add(m.Type.ToString());
                    }
                        break;
                    case DemographicsMode.Status:
                    {
                        Add("Minion");
                    }
                        break;
                }
        }

        private void Add(string label)
        {
            if (_breakdown.ContainsKey(label))
                _breakdown[label] += 1;
        }
    }
}
