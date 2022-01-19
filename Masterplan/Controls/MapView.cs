using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Events;
using Masterplan.Tools;

namespace Masterplan.Controls
{
    /// <summary>
    ///     Settings for displaying a MapView control.
    /// </summary>
    public enum MapViewMode
    {
        /// <summary>
        ///     A fully interactive MapView.
        /// </summary>
        Normal,

        /// <summary>
        ///     The MapView will be non-interactive with a plain background.
        /// </summary>
        Plain,

        /// <summary>
        ///     A non-interactive MapView.
        /// </summary>
        Thumbnail,

        /// <summary>
        ///     The map is to be shown on the player view.
        /// </summary>
        PlayerView
    }

    /// <summary>
    ///     Controls how portions of a MapView control is displayed.
    /// </summary>
    public enum MapDisplayType
    {
        /// <summary>
        ///     Not shown.
        /// </summary>
        None,

        /// <summary>
        ///     Slightly transparent.
        /// </summary>
        Dimmed,

        /// <summary>
        ///     Opaque.
        /// </summary>
        Opaque
    }

    /// <summary>
    ///     Controls how creature tokens are shown on a MapView control.
    /// </summary>
    public enum CreatureViewMode
    {
        /// <summary>
        ///     All creature tokens are shown.
        /// </summary>
        All,

        /// <summary>
        ///     Only visible creature tokens are shown.
        /// </summary>
        Visible,

        /// <summary>
        ///     No creature tokens are shown.
        /// </summary>
        None
    }

    /// <summary>
    ///     Controls how the grid is shown on a MapView control.
    /// </summary>
    public enum MapGridMode
    {
        /// <summary>
        ///     No gridlines.
        /// </summary>
        None,

        /// <summary>
        ///     Gridlines are shown behind map tiles.
        /// </summary>
        Behind,

        /// <summary>
        ///     Gridlines are shown in front of map tiles.
        /// </summary>
        Overlay
    }

    /// <summary>
    ///     Control for displaying a Map object.
    /// </summary>
    public partial class MapView : UserControl
    {
        private readonly StringFormat _centered = new StringFormat();
        private readonly StringFormat _fBottom = new StringFormat();
        private readonly StringFormat _fLeft = new StringFormat();
        private readonly StringFormat _fRight = new StringFormat();

        private readonly Dictionary<Guid, List<Rectangle>> _fSlotRegions = new Dictionary<Guid, List<Rectangle>>();

        private readonly Dictionary<TokenLink, RectangleF> _fTokenLinkRegions = new Dictionary<TokenLink, RectangleF>();
        private readonly StringFormat _fTop = new StringFormat();

        private bool _fAllowScrolling;

        private Map _fBackgroundMap;

        private int _fBorderSize;

        private Rectangle _fCurrentOutline = Rectangle.Empty;

        private DraggedOutline _fDraggedOutline;
        private DraggedTiles _fDraggedTiles;
        private DraggedToken _fDraggedToken;

        private DrawingData _fDrawing;

        private Encounter _fEncounter;

        private MapDisplayType _fFrameType = MapDisplayType.Dimmed;

        private bool _fHighlightAreas;
        private TileData _fHoverTile;

        private IToken _fHoverToken;

        private TokenLink _fHoverTokenLink;

        private MapData _fLayoutData;

        private bool _fLineOfSight;

        private Map _fMap;

        private MapViewMode _fMode = MapViewMode.Normal;

        private NewTile _fNewTile;

        private NewToken _fNewToken;

        private Plot _fPlot;

        private double _fScalingFactor = 1.0;

        private ScrollingData _fScrollingData;

        private MapArea _fSelectedArea;

        private List<TileData> _fSelectedTiles;

        private bool _fShowAllWaves;

        private bool _fShowAuras = true;

        private bool _fShowConditions = true;

        private bool _fShowCreatureLabels = true;

        private CreatureViewMode _fShowCreatures = CreatureViewMode.All;

        private MapGridMode _fShowGrid = MapGridMode.None;

        private bool _fShowGridLabels;

        private bool _fShowHealthBars;

        private bool _fShowPictureTokens = true;

        private bool _fTactical;

        private List<TokenLink> _fTokenLinks;

        private Rectangle _fViewpoint = Rectangle.Empty;

        /// <summary>
        ///     Gets or sets the Map to be displayed.
        /// </summary>
        [Category("Data")]
        [Description("The map to be displayed.")]
        public Map Map
        {
            get => _fMap;
            set
            {
                _fMap = value;
                _fLayoutData = null;

                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the Map to be displayed.
        /// </summary>
        [Category("Data")]
        [Description("The map to be displayed in the background.")]
        public Map BackgroundMap
        {
            get => _fBackgroundMap;
            set
            {
                _fBackgroundMap = value?.Copy();
                _fLayoutData = null;

                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the Encounter containing creature locations.
        /// </summary>
        [Category("Data")]
        [Description("The encounter to be displayed.")]
        public Encounter Encounter
        {
            get => _fEncounter;
            set
            {
                _fEncounter = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the Encounter containing creature locations.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether we should show all waves, or only active waves.")]
        public bool ShowAllWaves
        {
            get => _fShowAllWaves;
            set
            {
                _fShowAllWaves = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the plot containing map-related plot points.
        /// </summary>
        [Category("Data")]
        [Description("The plot to be displayed.")]
        public Plot Plot
        {
            get => _fPlot;
            set
            {
                _fPlot = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the tile co-ordinates to zoom into, or Rectangle.Empty to display the full map.
        /// </summary>
        [Category("Appearance")]
        [Description("The tile co-ordinates to view.")]
        public Rectangle Viewpoint
        {
            get => _fViewpoint;
            set
            {
                _fViewpoint = value;
                _fLayoutData = null;

                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the mode in which to display the map.
        /// </summary>
        [Category("Appearance")]
        [Description("The mode in which to display the map.")]
        public MapViewMode Mode
        {
            get => _fMode;
            set
            {
                _fMode = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether map tokens can be moved around the map.
        /// </summary>
        [Category("Behavior")]
        [Description("Determines whether map tokens can be moved around the map.")]
        public bool Tactical
        {
            get => _fTactical;
            set
            {
                _fTactical = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether map areas are highlighted.
        /// </summary>
        [Category("Appearance")]
        [Description("Determines whether areas are highlighted.")]
        public bool HighlightAreas
        {
            get => _fHighlightAreas;
            set
            {
                _fHighlightAreas = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating how gridlines are drawn on the map.
        /// </summary>
        [Category("Appearance")]
        [Description("Determines how gridlines are shown.")]
        public MapGridMode ShowGrid
        {
            get => _fShowGrid;
            set
            {
                _fShowGrid = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the map grid has labels.
        /// </summary>
        [Category("Appearance")]
        [Description("Determines whether grid rows and columns are labelled.")]
        public bool ShowGridLabels
        {
            get => _fShowGridLabels;
            set
            {
                _fShowGridLabels = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether token images are shown as tokens.
        /// </summary>
        [Category("Appearance")]
        [Description("Determines whether token images are shown as tokens.")]
        public bool ShowPictureTokens
        {
            get => _fShowPictureTokens;
            set
            {
                _fShowPictureTokens = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the number of tiles to be drawn around the map.
        /// </summary>
        [Category("Appearance")]
        [Description("The number of squares to be drawn around the viewpoint.")]
        public int BorderSize
        {
            get => _fBorderSize;
            set
            {
                _fBorderSize = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the selected tiles.
        /// </summary>
        [Category("Data")]
        [Description("The list of selected tiles.")]
        public List<TileData> SelectedTiles
        {
            get => _fSelectedTiles;
            set
            {
                _fSelectedTiles = value;

                Invalidate();
            }
        }

        /// <summary>
        ///     Gets the list of boxed tokens.
        /// </summary>
        [Category("Data")]
        [Description("The list of boxed map tokens.")]
        public List<IToken> BoxedTokens { get; } = new List<IToken>();

        /// <summary>
        ///     Gets the list of selected tokens.
        /// </summary>
        [Category("Appearance")]
        [Description("The list of selected map tokens.")]
        public List<IToken> SelectedTokens { get; } = new List<IToken>();

        /// <summary>
        ///     Gets the hovered token.
        /// </summary>
        [Category("Appearance")]
        [Description("The hovered map token.")]
        public IToken HoverToken
        {
            get => _fHoverToken;
            set
            {
                _fHoverToken = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets the hovered token link.
        /// </summary>
        [Category("Appearance")]
        [Description("The hovered token link.")]
        public TokenLink HoverTokenLink
        {
            get => _fHoverTokenLink;
            set
            {
                _fHoverTokenLink = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the selected region.
        /// </summary>
        [Category("Appearance")]
        [Description("The rubber-band selected rectangle.")]
        public Rectangle Selection
        {
            get => _fCurrentOutline;
            set
            {
                _fCurrentOutline = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets how creatures should be displayed.
        /// </summary>
        [Category("Appearance")]
        [Description("Determines how creatures should be displayed.")]
        public CreatureViewMode ShowCreatures
        {
            get => _fShowCreatures;
            set
            {
                _fShowCreatures = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether creatures should be displayed with a label based on their name.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether creatures should be shown with abbreviated labels.")]
        public bool ShowCreatureLabels
        {
            get => _fShowCreatureLabels;
            set
            {
                _fShowCreatureLabels = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether creatures should be displayed with a bar showing their HP.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether creatures should be shown with an HP bar.")]
        public bool ShowHealthBars
        {
            get => _fShowHealthBars;
            set
            {
                _fShowHealthBars = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether condition badges should be displayed on tokens.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether condition badges should be shown.")]
        public bool ShowConditions
        {
            get => _fShowConditions;
            set
            {
                _fShowConditions = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether creature auras should be displayed on the map.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether creature auras should be shown.")]
        public bool ShowAuras
        {
            get => _fShowAuras;
            set
            {
                _fShowAuras = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets the highlighted MapArea.
        /// </summary>
        [Category("Appearance")]
        [Description("The highlighted MapArea.")]
        public MapArea HighlightedArea { get; private set; }

        /// <summary>
        ///     Gets or sets the selected MapArea.
        /// </summary>
        [Category("Appearance")]
        [Description("The selected MapArea.")]
        public MapArea SelectedArea
        {
            get => _fSelectedArea;
            set
            {
                _fSelectedArea = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether links between tokens can be created using drag and drop.
        /// </summary>
        [Category("Behavior")]
        [Description("Determines whether links between tokens can be created.")]
        public bool AllowLinkCreation { get; set; }

        /// <summary>
        ///     Gets or sets the list of token links.
        /// </summary>
        [Category("Data")]
        [Description("The list of links between map tokens.")]
        public List<TokenLink> TokenLinks
        {
            get => _fTokenLinks;
            set
            {
                _fTokenLinks = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether scrolling the map is allowed.
        /// </summary>
        [Category("Behavior")]
        [Description("Determines whether the map can be scrolled.")]
        public bool AllowScrolling
        {
            get => _fAllowScrolling;
            set
            {
                _fAllowScrolling = value;

                if (_fAllowScrolling == false)
                {
                    if (_fScalingFactor != 1)
                        _fViewpoint = get_current_zoom_rect(false);

                    _fScalingFactor = 1;
                }

                Cursor = _fAllowScrolling ? Cursors.SizeAll : Cursors.Default;
                Invalidate();

                if (!_fAllowScrolling)
                    OnCancelledScrolling();
            }
        }

        /// <summary>
        ///     Gets or sets the map scaling factor.
        /// </summary>
        [Category("Appearance")]
        [Description("The scaling factor for the map; this can be used to zoom in and out.")]
        public double ScalingFactor
        {
            get => _fScalingFactor;
            set
            {
                _fScalingFactor = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets how frames are displayed around the Viewpoint.
        /// </summary>
        [Category("Appearance")]
        [Description("The appearance of the frame around the viewpoint.")]
        public MapDisplayType FrameType
        {
            get => _fFrameType;
            set
            {
                _fFrameType = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets whether line of sight is indicated.
        /// </summary>
        [Category("Appearance")]
        [Description("How the line of sight is displayed.")]
        public bool LineOfSight
        {
            get => _fLineOfSight;
            set
            {
                _fLineOfSight = value;
                Invalidate();
                if (!_fLineOfSight)
                    OnCancelledLOS();
            }
        }

        /// <summary>
        ///     Gets or sets whether drawing on the map is permitted.
        /// </summary>
        public bool AllowDrawing
        {
            get => _fDrawing != null;
            set
            {
                if (value)
                    _fDrawing = new DrawingData();
                else
                    _fDrawing = null;

                Cursor = _fDrawing == null ? Cursors.Default : Cursors.Cross;

                Invalidate();

                if (_fDrawing == null)
                    OnCancelledDrawing();
            }
        }

        /// <summary>
        ///     Gets the list of map sketches.
        /// </summary>
        public List<MapSketch> Sketches { get; } = new List<MapSketch>();

        /// <summary>
        ///     Gets or sets the map caption.
        /// </summary>
        public string Caption { get; set; } = "";

        /// <summary>
        ///     Gets the object containing layout logic for this map.
        /// </summary>
        internal MapData LayoutData
        {
            get
            {
                if (_fLayoutData == null)
                    _fLayoutData = new MapData(this, _fScalingFactor);

                return _fLayoutData;
            }
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public MapView()
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

            _fTop.Alignment = StringAlignment.Center;
            _fTop.LineAlignment = StringAlignment.Near;
            _fTop.Trimming = StringTrimming.EllipsisCharacter;

            _fBottom.Alignment = StringAlignment.Center;
            _fBottom.LineAlignment = StringAlignment.Far;
            _fBottom.Trimming = StringTrimming.EllipsisCharacter;

            _fLeft.Alignment = StringAlignment.Near;
            _fLeft.LineAlignment = StringAlignment.Center;
            _fLeft.Trimming = StringTrimming.EllipsisCharacter;

            _fRight.Alignment = StringAlignment.Far;
            _fRight.LineAlignment = StringAlignment.Center;
            _fRight.Trimming = StringTrimming.EllipsisCharacter;
        }

        /// <summary>
        ///     Updates the MapView control.
        /// </summary>
        public void MapChanged()
        {
            _fLayoutData = null;
            Invalidate();
        }

        /// <summary>
        ///     This method is used to notify the MapView that a user has pressed an arrow key.
        /// </summary>
        /// <param name="e">Event arguments</param>
        public void Nudge(KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        /// <summary>
        ///     Called in response to the Resize event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnResize(EventArgs e)
        {
            // Resized; we'll need to recalculate everything
            _fLayoutData = null;
            Invalidate();
        }

        /// <summary>
        ///     Sets information about the currently dragged map token.
        /// </summary>
        /// <param name="oldPoint">The dragged token's original location</param>
        /// <param name="newPoint">The dragged token's new location</param>
        public void SetDragInfo(Point oldPoint, Point newPoint)
        {
            if (oldPoint == CombatData.NoPoint)
            {
                _fDraggedToken = null;
                Invalidate();
            }
            else
            {
                var pair = get_token_at(oldPoint);
                if (pair != null)
                {
                    _fDraggedToken = new DraggedToken();

                    _fDraggedToken.Token = pair.First;
                    _fDraggedToken.Start = oldPoint;
                    _fDraggedToken.Location = newPoint;

                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Selects a set of map tokens.
        /// </summary>
        /// <param name="tokens">The map tokens to select.</param>
        /// <param name="raiseEvent">Whether the SelectedTokenChanged event should be raised.</param>
        public void SelectTokens(List<IToken> tokens, bool raiseEvent)
        {
            if (tokens == null)
            {
                SelectedTokens.Clear();
                return;
            }

            foreach (var token in tokens)
                if (!SelectedTokens.Contains(token))
                    SelectedTokens.Add(token);

            var obsolete = new List<IToken>();
            foreach (var token in SelectedTokens)
                if (!tokens.Contains(token))
                    obsolete.Add(token);

            foreach (var token in obsolete)
                SelectedTokens.Remove(token);

            Invalidate();

            if (raiseEvent)
                OnSelectedTokensChanged();
        }

        /// <summary>
        ///     This is called when a tile or token has been removed from the map.
        /// </summary>
        [Category("Action")]
        [Description("Called when a tile or token is removed from the map.")]
        public event EventHandler ItemRemoved;

        /// <summary>
        ///     Raises the ItemRemoved event.
        /// </summary>
        protected void OnItemRemoved()
        {
            ItemRemoved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     This is called when a tile or token is dropped onto the map.
        /// </summary>
        [Category("Action")]
        [Description("Called when a tile or token is dropped onto the map.")]
        public event EventHandler ItemDropped;

        /// <summary>
        ///     Raises the ItemDropped event.
        /// </summary>
        protected void OnItemDropped()
        {
            ItemDropped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     This is called when a tile or token is moved around the map.
        /// </summary>
        [Category("Action")]
        [Description("Called when a tile or token is moved around the map.")]
        public event MovementEventHandler ItemMoved;

        /// <summary>
        ///     Raises the ItemMoved event.
        /// </summary>
        protected void OnItemMoved(int distance)
        {
            ItemMoved?.Invoke(this, new MovementEventArgs(distance));
        }

        /// <summary>
        ///     This is called when an area has been selected.
        /// </summary>
        [Category("Action")]
        [Description("Called when an area has been selected.")]
        public event EventHandler RegionSelected;

        /// <summary>
        ///     Raises the RegionSelected event.
        /// </summary>
        protected void OnRegionSelected()
        {
            RegionSelected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     This is called when a context menu should be displayed on the selected tile.
        /// </summary>
        [Category("Action")]
        [Description("Called when a context menu should be displayed.")]
        public event EventHandler TileContext;

        /// <summary>
        ///     Raises the TileContext event.
        /// </summary>
        /// <param name="tile">The tile.</param>
        protected void OnTileContext(TileData tile)
        {
            TileContext?.Invoke(this, new TileEventArgs(tile));
        }

        /// <summary>
        ///     This is called when the hovered token has changed.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the hovered token has changed.")]
        public event EventHandler HoverTokenChanged;

        /// <summary>
        ///     Raises the HoverTokenChanged event.
        /// </summary>
        protected void OnHoverTokenChanged()
        {
            HoverTokenChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     This is called when the selected tokens have changed.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the selected tokens have changed.")]
        public event EventHandler SelectedTokensChanged;

        /// <summary>
        ///     Raises the SelectedTokensChanged event.
        /// </summary>
        protected void OnSelectedTokensChanged()
        {
            SelectedTokensChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     This is called when the highlighted MapArea has changed.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the highlighted map area has changed.")]
        public event EventHandler HighlightedAreaChanged;

        /// <summary>
        ///     Raises the HighlightedAreaChanged event.
        /// </summary>
        protected void OnHighlightedAreaChanged()
        {
            // Don't call if we're not showing areas
            if (!_fHighlightAreas)
                return;

            // Don't call if we're zoomed into that area
            if (HighlightedArea != null && _fViewpoint == HighlightedArea.Region)
                return;

            HighlightedAreaChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     This is called when a token is double-clicked.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a map token is double-clicked.")]
        public event TokenEventHandler TokenActivated;

        /// <summary>
        ///     Raises the TokenActivated event.
        /// </summary>
        /// <param name="token">The token.</param>
        protected void OnTokenActivated(IToken token)
        {
            TokenActivated?.Invoke(this, new TokenEventArgs(token));
        }

        /// <summary>
        ///     This is called when a token is dragged.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a map token is dragged.")]
        public event DraggedTokenEventHandler TokenDragged;

        /// <summary>
        ///     Raises the TokenDragged event.
        /// </summary>
        // /// <param name="token">The token.</param>
        protected void OnTokenDragged()
        {
            if (TokenDragged != null)
            {
                var oldLoc = _fDraggedToken?.Start ?? CombatData.NoPoint;
                var newLoc = _fDraggedToken?.Location ?? CombatData.NoPoint;
                TokenDragged(this, new DraggedTokenEventArgs(oldLoc, newLoc));
            }
        }

        /// <summary>
        ///     This is called when a map area is clicked.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a map area is clicked.")]
        public event MapAreaEventHandler AreaSelected;

        /// <summary>
        ///     Raises the AreaSelected event.
        /// </summary>
        /// <param name="area">The map area.</param>
        protected void OnAreaSelected(MapArea area)
        {
            AreaSelected?.Invoke(this, new MapAreaEventArgs(area));
        }

        /// <summary>
        ///     This is called when a map area is double-clicked.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a map area is double-clicked.")]
        public event MapAreaEventHandler AreaActivated;

        /// <summary>
        ///     Raises the AreaActivated event.
        /// </summary>
        /// <param name="area">The map area.</param>
        protected void OnAreaActivated(MapArea area)
        {
            AreaActivated?.Invoke(this, new MapAreaEventArgs(area));
        }

        /// <summary>
        ///     This is called when a link between tokens should be created.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a link should be created.")]
        public event CreateTokenLinkEventHandler CreateTokenLink;

        /// <summary>
        ///     Raises the CreateTokenLink event.
        /// </summary>
        /// <param name="tokens">The list of tokens.</param>
        protected TokenLink OnCreateTokenLink(List<IToken> tokens)
        {
            return CreateTokenLink?.Invoke(this, new TokenListEventArgs(tokens));
        }

        /// <summary>
        ///     This is called when a link between tokens should be edited.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a link should be edited.")]
        public event TokenLinkEventHandler EditTokenLink;

        /// <summary>
        ///     Raises the EditTokenLink event.
        /// </summary>
        /// <param name="link">The token link to be edited.</param>
        protected TokenLink OnEditTokenLink(TokenLink link)
        {
            return EditTokenLink?.Invoke(this, new TokenLinkEventArgs(link));
        }

        /// <summary>
        ///     This is called when a new sketch is created.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a sketch is created.")]
        public event MapSketchEventHandler SketchCreated;

        /// <summary>
        ///     Raises the SketchCreated event.
        /// </summary>
        /// <param name="sketch">The new sketch.</param>
        protected void OnSketchCreated(MapSketch sketch)
        {
            SketchCreated?.Invoke(this, new MapSketchEventArgs(sketch));
        }

        /// <summary>
        ///     This is called when the mouse wheel is scrolled.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when the mouse wheel is scrolled.")]
        public event MouseEventHandler MouseZoomed;

        /// <summary>
        ///     Raises the MouseScrolled event.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        protected void OnMouseZoom(MouseEventArgs args)
        {
            MouseZoomed?.Invoke(this, args);
        }

        /// <summary>
        ///     This is called when the LOS mode is cancelled.
        /// </summary>
        [Category("Action")]
        [Description("Called when the LOS mode is cancelled.")]
        public event EventHandler CancelledLos;

        /// <summary>
        ///     Raises the CancelledLOS event.
        /// </summary>
        protected void OnCancelledLOS()
        {
            CancelledLos?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     This is called when the drawing mode is cancelled.
        /// </summary>
        [Category("Action")]
        [Description("Called when the drawing mode is cancelled.")]
        public event EventHandler CancelledDrawing;

        /// <summary>
        ///     Raises the CancelledDrawing event.
        /// </summary>
        protected void OnCancelledDrawing()
        {
            CancelledDrawing?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     This is called when the scrolling mode is cancelled.
        /// </summary>
        [Category("Action")]
        [Description("Called when the scrolling mode is cancelled.")]
        public event EventHandler CancelledScrolling;

        /// <summary>
        ///     Raises the CancelledScrolling event.
        /// </summary>
        protected void OnCancelledScrolling()
        {
            CancelledScrolling?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Called in response to the Paint event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (_fLayoutData == null)
                _fLayoutData = new MapData(this, _fScalingFactor);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            switch (_fMode)
            {
                case MapViewMode.Normal:
                {
                    using (Brush background = new SolidBrush(Color.FromArgb(70, 100, 170)))
                    {
                        e.Graphics.FillRectangle(background, ClientRectangle);
                    }
                }
                    break;
                case MapViewMode.Thumbnail:
                {
                    var top = Color.FromArgb(240, 240, 240);
                    var bottom = Color.FromArgb(170, 170, 170);
                    using (Brush background =
                           new LinearGradientBrush(ClientRectangle, top, bottom, LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(background, ClientRectangle);
                    }
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

            if (_fMap == null)
            {
                var b = SystemBrushes.WindowText;
                if (_fMode == MapViewMode.Normal)
                    b = Brushes.White;

                e.Graphics.DrawString("(no map selected)", Font, b, ClientRectangle, _centered);
                return;
            }

            // Draw grid
            if (_fShowGrid == MapGridMode.Behind && _fLayoutData.SquareSize >= 10)
                using (var lightP = new Pen(Color.FromArgb(100, 140, 190)))
                {
                    using (var heavyP = new Pen(Color.FromArgb(150, 200, 230)))
                    {
                        float x = 0;
                        var offsetX = _fLayoutData.MapOffset.Width % _fLayoutData.SquareSize;
                        var xStep = 0;
                        while (x <= ClientRectangle.Width)
                        {
                            if (xStep % 4 == 0)
                                e.Graphics.DrawLine(heavyP, x + offsetX, 0, x + offsetX, ClientRectangle.Height);
                            else
                                e.Graphics.DrawLine(lightP, x + offsetX, 0, x + offsetX, ClientRectangle.Height);

                            x += _fLayoutData.SquareSize;
                            xStep += 1;
                        }

                        float y = 0;
                        var offsetY = _fLayoutData.MapOffset.Height % _fLayoutData.SquareSize;
                        var yStep = 0;
                        while (y <= ClientRectangle.Height)
                        {
                            if (yStep % 4 == 0)
                                e.Graphics.DrawLine(heavyP, 0, y + offsetY, ClientRectangle.Width, y + offsetY);
                            else
                                e.Graphics.DrawLine(lightP, 0, y + offsetY, ClientRectangle.Width, y + offsetY);

                            y += _fLayoutData.SquareSize;
                            yStep += 1;
                        }
                    }
                }

            if (_fEncounter != null)
            {
                _fSlotRegions.Clear();

                foreach (var slot in _fEncounter.AllSlots)
                {
                    _fSlotRegions[slot.Id] = new List<Rectangle>();

                    var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                    if (creature == null)
                        continue;

                    var size = Creature.GetSize(creature.Size);

                    foreach (var cd in slot.CombatData)
                        _fSlotRegions[slot.Id].Add(new Rectangle(cd.Location, new Size(size, size)));
                }
            }

            if (_fHighlightAreas)
                foreach (var area in _fMap.Areas)
                {
                    var rect = _fLayoutData.GetRegion(area.Region.Location, area.Region.Size);

                    Brush b = null;
                    if (area == _fSelectedArea)
                    {
                        b = Brushes.LightBlue;
                    }
                    else
                    {
                        var top = Color.FromArgb(255, 255, 255);
                        var bottom = Color.FromArgb(210, 210, 210);
                        b = new LinearGradientBrush(ClientRectangle, top, bottom, LinearGradientMode.Vertical);
                    }

                    if (_fPlot != null)
                    {
                        var point = _fPlot.FindPointForMapArea(_fMap, area);
                        if (point == null)
                            // There's no plot point for this map area
                            b = null;
                    }

                    if (b != null)
                        e.Graphics.FillRectangle(b, rect);
                }

            if (_fCurrentOutline != Rectangle.Empty)
            {
                var rect = _fLayoutData.GetRegion(_fCurrentOutline.Location, _fCurrentOutline.Size);
                e.Graphics.FillRectangle(Brushes.LightBlue, rect);
            }

            // Draw background tiles
            if (_fBackgroundMap != null)
                foreach (var td in _fBackgroundMap.Tiles)
                {
                    if (!_fLayoutData.Tiles.ContainsKey(td))
                        continue;

                    var tile = _fLayoutData.Tiles[td];

                    // Draw this tile
                    var tileRect = _fLayoutData.TileRegions[td];
                    draw_tile(e.Graphics, tile, td.Rotations, tileRect, true);
                }

            // Draw tiles
            foreach (var td in _fMap.Tiles)
            {
                if (_fDraggedTiles != null && _fDraggedTiles.Tiles.Contains(td))
                    continue;

                if (!_fLayoutData.Tiles.ContainsKey(td))
                    continue;

                var tile = _fLayoutData.Tiles[td];

                // Draw this tile
                var tileRect = _fLayoutData.TileRegions[td];
                draw_tile(e.Graphics, tile, td.Rotations, tileRect, false);

                if (_fSelectedTiles != null && _fSelectedTiles.Contains(td))
                    // Highlight selected tile
                    e.Graphics.DrawRectangle(Pens.Blue, tileRect.X, tileRect.Y, tileRect.Width, tileRect.Height);
                else if (td == _fHoverTile)
                    // Highlight hover tile
                    e.Graphics.DrawRectangle(Pens.DarkBlue, tileRect.X, tileRect.Y, tileRect.Width, tileRect.Height);
            }

            if (_fNewTile != null)
                // Draw dragged tile
                draw_tile(e.Graphics, _fNewTile.Tile, 0, _fNewTile.Region, false);

            if (_fDraggedTiles != null)
                // Draw dragged tiles
                foreach (var td in _fDraggedTiles.Tiles)
                {
                    var t = _fLayoutData.Tiles[td];
                    draw_tile(e.Graphics, t, td.Rotations, _fDraggedTiles.Region, false);
                }

            if (_fShowGrid == MapGridMode.Overlay && _fLayoutData.SquareSize >= 10)
            {
                var p = Pens.DarkGray;

                float x = 0;
                var offsetX = _fLayoutData.MapOffset.Width % _fLayoutData.SquareSize;
                while (x <= ClientRectangle.Width)
                {
                    e.Graphics.DrawLine(p, x + offsetX, 0, x + offsetX, ClientRectangle.Height);

                    x += _fLayoutData.SquareSize;
                }

                float y = 0;
                var offsetY = _fLayoutData.MapOffset.Height % _fLayoutData.SquareSize;
                while (y <= ClientRectangle.Height)
                {
                    e.Graphics.DrawLine(p, 0, y + offsetY, ClientRectangle.Width, y + offsetY);

                    y += _fLayoutData.SquareSize;
                }
            }

            if (_fShowGridLabels)
            {
                var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

                var size = _fLayoutData.SquareSize / 4f;
                var label = new Font(Font.FontFamily, size);

                for (var x = _fLayoutData.MinX; x <= _fLayoutData.MaxX; ++x)
                {
                    var dx = x - _fLayoutData.MinX + 1;
                    var str = dx.ToString();

                    var top = _fLayoutData.GetRegion(new Point(x, _fLayoutData.MinY), new Size(1, 1));
                    draw_grid_label(e.Graphics, str, label, top, _fTop);

                    var bottom = _fLayoutData.GetRegion(new Point(x, _fLayoutData.MaxY), new Size(1, 1));
                    draw_grid_label(e.Graphics, str, label, bottom, _fBottom);
                }

                for (var y = _fLayoutData.MinY; y <= _fLayoutData.MaxY; ++y)
                {
                    var dy = y - _fLayoutData.MinY;

                    var str = "";
                    if (dy >= alphabet.Length)
                    {
                        var first = dy / alphabet.Length;
                        str += alphabet.Substring(first - 1, 1);
                    }

                    var second = dy % alphabet.Length;
                    str += alphabet.Substring(second, 1);

                    var left = _fLayoutData.GetRegion(new Point(_fLayoutData.MinX, y), new Size(1, 1));
                    draw_grid_label(e.Graphics, str, label, left, _fLeft);

                    var right = _fLayoutData.GetRegion(new Point(_fLayoutData.MaxX, y), new Size(1, 1));
                    draw_grid_label(e.Graphics, str, label, right, _fRight);
                }
            }

            if (_fHighlightAreas)
            {
                foreach (var area in _fMap.Areas)
                {
                    var state = PlotPointState.Normal;
                    var point = _fPlot?.FindPointForMapArea(_fMap, area);
                    if (point != null)
                        state = point.State;

                    var rect = _fLayoutData.GetRegion(area.Region.Location, area.Region.Size);

                    var p = Pens.DarkGray;
                    if (area == HighlightedArea || area == _fSelectedArea)
                        p = Pens.DarkBlue;

                    e.Graphics.DrawRectangle(p, rect.X, rect.Y, rect.Width, rect.Height);

                    if (state == PlotPointState.Completed || state == PlotPointState.Skipped)
                    {
                        var topLeft = new PointF(rect.Left, rect.Top);
                        var topRight = new PointF(rect.Right, rect.Top);
                        var bottomLeft = new PointF(rect.Left, rect.Bottom);
                        var bottomRight = new PointF(rect.Right, rect.Bottom);

                        var xPen = new Pen(Color.DarkGray, 2);

                        e.Graphics.DrawLine(xPen, topLeft, bottomRight);
                        e.Graphics.DrawLine(xPen, bottomLeft, topRight);
                    }

                    // Draw area name
                    if (_fViewpoint == Rectangle.Empty && area.Name != "" && _fNewTile == null &&
                        _fDraggedTiles == null)
                    {
                        var textFont = Font;
                        if (state == PlotPointState.Skipped)
                            textFont = new Font(textFont, textFont.Style | FontStyle.Strikeout);

                        float delta = 8;
                        var size = e.Graphics.MeasureString(area.Name, textFont);
                        size = new SizeF(size.Width + delta, size.Height + delta);

                        var dx = (rect.Width - size.Width) / 2;
                        var dy = (rect.Height - size.Height) / 2;

                        var textRect = new RectangleF(rect.Left + dx, rect.Top + dy, size.Width, size.Height);

                        var path = RoundedRectangle.Create(textRect, textRect.Height / 3);
                        using (Brush b = new SolidBrush(Color.FromArgb(200, Color.Black)))
                        {
                            e.Graphics.FillPath(b, path);
                        }

                        e.Graphics.DrawPath(Pens.Black, path);
                        e.Graphics.DrawString(area.Name, textFont, Brushes.White, textRect, _centered);
                    }
                }
            }
            else
            {
                if (_fPlot != null)
                    foreach (var area in _fMap.Areas)
                    {
                        var point = _fPlot.FindPointForMapArea(_fMap, area);
                        if (point != null && point.State == PlotPointState.Completed)
                            continue;

                        // Fill unexplored regions in black
                        var rect = _fLayoutData.GetRegion(area.Region.Location, area.Region.Size);
                        e.Graphics.FillRectangle(Brushes.Black, rect);
                    }
            }

            if (_fShowAuras)
            {
                var tokens = new List<IToken>();
                tokens.AddRange(SelectedTokens);
                if (_fHoverToken != null)
                    tokens.Add(_fHoverToken);

                foreach (var token in tokens)
                {
                    var auraList = new Dictionary<Aura, Rectangle>();

                    var ct = token as CreatureToken;
                    if (ct != null)
                    {
                        if (ct.Data.Location == CombatData.NoPoint)
                            continue;

                        var auras = new List<Aura>();
                        foreach (var oc in ct.Data.Conditions)
                            if (oc.Type == OngoingType.Aura)
                                auras.Add(oc.Aura);

                        var slot = _fEncounter.FindSlot(ct.SlotId);
                        if (slot != null)
                            auras.AddRange(slot.Card.Auras);

                        if (slot != null)
                        {
                            var c = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                            var creatureSize = c != null ? Creature.GetSize(c.Size) : 1;

                            foreach (var aura in auras)
                            {
                                var dimension = aura.Radius + creatureSize + aura.Radius;
                                var auraLoc = new Point(ct.Data.Location.X - aura.Radius,
                                    ct.Data.Location.Y - aura.Radius);
                                var auraSize = new Size(dimension, dimension);

                                auraList[aura] = new Rectangle(auraLoc, auraSize);
                            }
                        }
                    }

                    // Custom tokens: you can't add effects to custom tokens

                    var hero = token as Hero;
                    if (hero != null)
                    {
                        // Get size
                        var creatureSize = Creature.GetSize(hero.Size);

                        // Get auras from CombatData
                        var cd = hero.CombatData;
                        if (cd != null)
                            foreach (var oc in cd.Conditions)
                                if (oc.Type == OngoingType.Aura)
                                {
                                    var dimension = oc.Aura.Radius + creatureSize + oc.Aura.Radius;
                                    var auraLoc = new Point(cd.Location.X - oc.Aura.Radius,
                                        cd.Location.Y - oc.Aura.Radius);
                                    var auraSize = new Size(dimension, dimension);

                                    auraList[oc.Aura] = new Rectangle(auraLoc, auraSize);
                                }
                    }

                    foreach (var aura in auraList.Keys)
                    {
                        var rect = auraList[aura];
                        var auraRect = _fLayoutData.GetRegion(rect.Location, rect.Size);

                        var rounding = _fLayoutData.SquareSize * 0.8f;
                        var path = RoundedRectangle.Create(auraRect, rounding);

                        using (var p = new Pen(Color.FromArgb(200, Color.Red)))
                        {
                            e.Graphics.DrawPath(p, path);
                        }

                        using (Brush b = new SolidBrush(Color.FromArgb(15, Color.Red)))
                        {
                            e.Graphics.FillPath(b, path);
                        }
                    }
                }
            }

            if (_fTokenLinks != null)
                foreach (var link in _fTokenLinks)
                {
                    // Draw link

                    var lhs = link.Tokens[0];
                    var rhs = link.Tokens[1];

                    var cdLhs = get_combat_data(lhs);
                    var cdRhs = get_combat_data(rhs);
                    if (cdLhs.Visible && cdRhs.Visible)
                    {
                        var rectLhs = get_token_rect(lhs);
                        var rectRhs = get_token_rect(rhs);

                        if (rectLhs == RectangleF.Empty || rectRhs == RectangleF.Empty)
                            continue;

                        var c = link == _fHoverTokenLink ? Color.Navy : Color.Black;

                        var ptLhs = new PointF((rectLhs.Left + rectLhs.Right) / 2, (rectLhs.Top + rectLhs.Bottom) / 2);
                        var ptRhs = new PointF((rectRhs.Left + rectRhs.Right) / 2, (rectRhs.Top + rectRhs.Bottom) / 2);
                        using (var linkPen = new Pen(c, 2))
                        {
                            e.Graphics.DrawLine(linkPen, ptLhs, ptRhs);
                        }
                    }
                }

            if (_fEncounter != null)
            {
                // Draw custom overlays
                foreach (var ct in _fEncounter.CustomTokens)
                {
                    if (ct.Type != CustomTokenType.Overlay)
                        continue;

                    if (!is_visible(ct.Data))
                        continue;

                    if (ct.CreatureId != Guid.Empty)
                    {
                        var size = CreatureSize.Medium;

                        var cd = _fEncounter.FindCombatData(ct.CreatureId);
                        if (cd != null)
                        {
                            ct.Data.Location = cd.Location;

                            var slot = _fEncounter.FindSlot(cd);
                            var c = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                            size = c.Size;
                        }

                        var hero = Session.Project.FindHero(ct.CreatureId);
                        if (hero != null)
                        {
                            ct.Data.Location = hero.CombatData.Location;
                            size = hero.Size;
                        }

                        if (ct.Data.Location != CombatData.NoPoint)
                        {
                            // Centre on the creature
                            var creatureSize = (Creature.GetSize(size) + 1) / 2;
                            var x = ct.Data.Location.X - (ct.OverlaySize.Width - creatureSize) / 2;
                            var y = ct.Data.Location.Y - (ct.OverlaySize.Height - creatureSize) / 2;
                            ct.Data.Location = new Point(x, y);
                        }
                    }

                    if (ct.Data.Location == CombatData.NoPoint)
                        continue;

                    var selected = SelectedTokens.Contains(ct);
                    var hovered = false;
                    if (_fHoverToken != null)
                        hovered = get_combat_data(_fHoverToken).Id == ct.Data.Id;

                    draw_custom(e.Graphics, ct.Data.Location, ct, selected, hovered, false);
                }

                // Draw custom tokens
                foreach (var ct in _fEncounter.CustomTokens)
                {
                    if (ct.Type != CustomTokenType.Token)
                        continue;

                    if (ct.Data.Location == CombatData.NoPoint)
                        continue;

                    if (!is_visible(ct.Data))
                        continue;

                    if (_fDraggedToken?.Token is CustomToken)
                    {
                        var token = _fDraggedToken.Token as CustomToken;
                        if (token.Type == CustomTokenType.Token)
                            if (ct.Id == token.Id && ct.Data.Location == _fDraggedToken.Start)
                            {
                                if (ct.Data.Location != _fDraggedToken.Location)
                                    draw_token_placeholder(e.Graphics, ct.Data.Location, _fDraggedToken.Location,
                                        ct.TokenSize, false);

                                continue;
                            }
                    }

                    var selected = SelectedTokens.Contains(ct);
                    var hovered = false;
                    if (_fHoverToken != null)
                        hovered = get_combat_data(_fHoverToken).Id == ct.Data.Id;

                    draw_custom(e.Graphics, ct.Data.Location, ct, selected, hovered, false);
                }

                // Draw creature tokens
                foreach (var slot in _fEncounter.AllSlots)
                {
                    // Ignore this slot if we're not supposed to be showing it
                    var ew = _fEncounter.FindWave(slot);
                    if (ew != null && ew.Active == false && _fShowAllWaves == false)
                        continue;

                    foreach (var cd in slot.CombatData)
                    {
                        if (cd.Location == CombatData.NoPoint)
                            continue;

                        if (!is_visible(cd))
                            continue;

                        if (_fDraggedToken?.Token is CreatureToken)
                        {
                            var token = _fDraggedToken.Token as CreatureToken;
                            if (slot.Id == token.SlotId && cd.Location == _fDraggedToken.Start)
                            {
                                if (cd.Location != _fDraggedToken.Location)
                                {
                                    var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                                    var hasPicture = creature.Image != null;
                                    draw_token_placeholder(e.Graphics, cd.Location, _fDraggedToken.Location,
                                        creature.Size, hasPicture);
                                }

                                continue;
                            }
                        }

                        var selected = false;
                        foreach (var token in SelectedTokens)
                        {
                            var ct = token as CreatureToken;
                            if (ct == null)
                                continue;

                            if (cd == ct.Data)
                                selected = true;
                        }

                        var hovered = false;
                        var hoverToken = _fHoverToken as CreatureToken;
                        if (hoverToken != null)
                            if (cd == hoverToken.Data)
                                hovered = true;

                        draw_creature(e.Graphics, cd.Location, slot.Card, cd, selected, hovered, false);
                    }
                }
            }

            // Draw heroes
            if (_fEncounter != null)
                foreach (var h in Session.Project.Heroes)
                {
                    if (h == null)
                        continue;

                    if (h.CombatData.Location == CombatData.NoPoint)
                        continue;

                    if (_fDraggedToken?.Token is Hero)
                    {
                        var hero = _fDraggedToken.Token as Hero;
                        if (h.Id == hero.Id && h.CombatData.Location == _fDraggedToken.Start)
                        {
                            if (h.CombatData.Location != _fDraggedToken.Location)
                            {
                                var hasPicture = h.Portrait != null;
                                draw_token_placeholder(e.Graphics, h.CombatData.Location, _fDraggedToken.Location,
                                    h.Size, hasPicture);
                            }

                            continue;
                        }
                    }

                    var selected = SelectedTokens.Contains(h);
                    var hovered = h == _fHoverToken;
                    draw_hero(e.Graphics, h.CombatData.Location, h, selected, hovered, false);
                }

            if (_fNewToken != null)
            {
                if (_fNewToken.Token is CreatureToken)
                {
                    // Draw dragged creature
                    var token = _fNewToken.Token as CreatureToken;
                    var slot = _fEncounter.FindSlot(token.SlotId);
                    Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                    draw_creature(e.Graphics, _fNewToken.Location, slot.Card, token.Data, true, true, true);
                }

                if (_fNewToken.Token is Hero)
                {
                    // Draw dragged hero
                    var hero = _fNewToken.Token as Hero;
                    draw_hero(e.Graphics, _fNewToken.Location, hero, true, true, true);
                }

                if (_fNewToken.Token is CustomToken)
                {
                    // Draw dragged custom token
                    var ct = _fNewToken.Token as CustomToken;
                    draw_custom(e.Graphics, _fNewToken.Location, ct, true, true, true);
                }
            }

            if (_fDraggedToken != null)
            {
                if (_fDraggedToken.Token is CreatureToken)
                {
                    var token = _fDraggedToken.Token as CreatureToken;

                    var slot = _fEncounter.FindSlot(token.SlotId);
                    var cd = slot.FindCombatData(_fDraggedToken.Start);

                    draw_creature(e.Graphics, _fDraggedToken.Location, slot.Card, cd, true, true, true);
                }

                if (_fDraggedToken.Token is Hero)
                {
                    var hero = _fDraggedToken.Token as Hero;
                    draw_hero(e.Graphics, _fDraggedToken.Location, hero, true, true, true);
                }

                if (_fDraggedToken.Token is CustomToken)
                {
                    var ct = _fDraggedToken.Token as CustomToken;
                    draw_custom(e.Graphics, _fDraggedToken.Location, ct, true, true, true);
                }

                if (_fDraggedToken.LinkedToken != null)
                {
                    var p = new Pen(Color.Red, 2);
                    var rect = get_token_rect(_fDraggedToken.LinkedToken);
                    e.Graphics.DrawRectangle(p, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }

            _fTokenLinkRegions.Clear();
            if (_fTokenLinks != null)
                foreach (var link in _fTokenLinks)
                {
                    if (link.Text == "")
                        continue;

                    var lhs = link.Tokens[0];
                    var rhs = link.Tokens[1];

                    var cdLhs = get_combat_data(lhs);
                    var cdRhs = get_combat_data(rhs);
                    if (cdLhs.Visible && cdRhs.Visible)
                    {
                        var locLhs = get_token_location(lhs);
                        var locRhs = get_token_location(rhs);

                        if (locLhs == CombatData.NoPoint || locRhs == CombatData.NoPoint)
                            continue;

                        var rectLhs = get_token_rect(lhs);
                        var rectRhs = get_token_rect(rhs);

                        var ptLhs = new PointF((rectLhs.Left + rectLhs.Right) / 2, (rectLhs.Top + rectLhs.Bottom) / 2);
                        var ptRhs = new PointF((rectRhs.Left + rectRhs.Right) / 2, (rectRhs.Top + rectRhs.Bottom) / 2);

                        var str = link.Text;

                        var fontsize = Math.Min(Font.Size, _fLayoutData.SquareSize / 4);
                        using (var linkfont = new Font(Font.FontFamily, fontsize))
                        {
                            var textSize = e.Graphics.MeasureString(str, linkfont);
                            textSize = new SizeF(textSize.Width * 1.2f, textSize.Height * 1.2f);

                            var centre = new PointF((ptLhs.X + ptRhs.X) / 2, (ptLhs.Y + ptRhs.Y) / 2);
                            var textPt = new PointF(centre.X - textSize.Width / 2, centre.Y - textSize.Height / 2);
                            var textRect = new RectangleF(textPt, textSize);

                            var outline = link == _fHoverTokenLink ? Pens.Blue : Pens.Navy;

                            e.Graphics.FillRectangle(Brushes.White, textRect);
                            e.Graphics.DrawString(str, linkfont, Brushes.Navy, textRect, _centered);
                            e.Graphics.DrawRectangle(outline, textRect.X, textRect.Y, textRect.Width, textRect.Height);

                            _fTokenLinkRegions[link] = textRect;
                        }
                    }
                }

            foreach (var sketch in Sketches)
                draw_sketch(e.Graphics, sketch);

            if (_fDrawing?.CurrentSketch != null)
                draw_sketch(e.Graphics, _fDrawing.CurrentSketch);

            if (_fLineOfSight)
            {
                var mouse = PointToClient(Cursor.Position);
                if (ClientRectangle.Contains(mouse))
                {
                    var vertex = get_closest_vertex(mouse);

                    var radius = Math.Max(_fLayoutData.SquareSize / 10, 3);

                    foreach (var token in SelectedTokens)
                    {
                        var rect = get_token_rect(token);

                        var points = new List<PointF>();
                        points.Add(new PointF(rect.Left, rect.Top));
                        points.Add(new PointF(rect.Left, rect.Bottom));
                        points.Add(new PointF(rect.Right, rect.Top));
                        points.Add(new PointF(rect.Right, rect.Bottom));

                        foreach (var point in points)
                        {
                            e.Graphics.DrawLine(Pens.Blue, vertex, point);

                            var pointRect = new RectangleF(point.X - radius, point.Y - radius, radius * 2, radius * 2);
                            e.Graphics.FillEllipse(Brushes.LightBlue, pointRect);
                            e.Graphics.DrawEllipse(Pens.Blue, pointRect);
                        }
                    }

                    var vertexRect = new RectangleF(vertex.X - radius, vertex.Y - radius, radius * 2, radius * 2);
                    e.Graphics.FillEllipse(Brushes.LightBlue, vertexRect);
                    e.Graphics.DrawEllipse(Pens.Blue, vertexRect);
                }
            }

            if (_fDraggedOutline != null)
            {
                var rect = _fLayoutData.GetRegion(_fDraggedOutline.Region.Location, _fDraggedOutline.Region.Size);
                e.Graphics.DrawRectangle(Pens.DarkBlue, rect.X, rect.Y, rect.Width, rect.Height);

                var str = _fDraggedOutline.Region.Width + "x" + _fDraggedOutline.Region.Height;

                var textSize = e.Graphics.MeasureString(str, Font);
                textSize.Width = Math.Min(rect.Width, textSize.Width);
                textSize.Height = Math.Min(rect.Height, textSize.Height);

                var dx = (rect.Width - textSize.Width) / 2;
                var dy = (rect.Height - textSize.Height) / 2;
                var textRect = new RectangleF(rect.X + dx, rect.Y + dy, textSize.Width, textSize.Height);

                using (Brush b = new SolidBrush(Color.FromArgb(150, Color.White)))
                {
                    e.Graphics.FillRectangle(b, textRect);
                }

                e.Graphics.DrawString(str, Font, Brushes.DarkBlue, rect, _centered);
            }

            if (_fCurrentOutline != null)
            {
                var rect = _fLayoutData.GetRegion(_fCurrentOutline.Location, _fCurrentOutline.Size);
                e.Graphics.DrawRectangle(Pens.LightBlue, rect.X, rect.Y, rect.Width, rect.Height);
            }

            if (_fFrameType != MapDisplayType.None && _fViewpoint != Rectangle.Empty && !_fAllowScrolling)
            {
                var frameColour = Color.Black;
                if (_fMode == MapViewMode.Plain)
                    frameColour = Color.White;

                var frameAlpha = 255;
                switch (_fFrameType)
                {
                    case MapDisplayType.Dimmed:
                        frameAlpha = 160;
                        break;
                    case MapDisplayType.Opaque:
                        frameAlpha = 255;
                        break;
                }

                var rect = _fLayoutData.GetRegion(_fViewpoint.Location, _fViewpoint.Size);

                using (Brush sides = new SolidBrush(Color.FromArgb(frameAlpha, frameColour)))
                {
                    e.Graphics.FillRectangle(sides, 0, 0, ClientRectangle.Width, rect.Top);
                    e.Graphics.FillRectangle(sides, 0, rect.Bottom, ClientRectangle.Width, ClientRectangle.Height);
                    e.Graphics.FillRectangle(sides, 0, rect.Top, rect.Left, rect.Height);
                    e.Graphics.FillRectangle(sides, rect.Right, rect.Top, ClientRectangle.Width - rect.Right,
                        rect.Height);
                }

                if (_fHighlightAreas)
                    e.Graphics.DrawRectangle(SystemPens.ControlLight, rect.X, rect.Y, rect.Width, rect.Height);
            }

            var caption = Caption;
            if (caption == "")
            {
                if (_fMode == MapViewMode.Normal && _fMap.Areas.Count == 0)
                    caption = "To create map areas (rooms etc), right-click on the map and drag.";
                if (_fMap.Name == "")
                    caption = "You need to give this map a name.";
                if (_fMode == MapViewMode.Normal && _fMap.Tiles.Count == 0)
                    caption = "To begin, drag tiles from the list on the right onto the blueprint.";
                if (_fAllowScrolling)
                    caption = "Map is in scroll / zoom mode; double-click to return to tactical mode.";
                if (_fDrawing != null)
                    caption = "Map is in drawing mode; double-click to return to tactical mode.";
                if (_fLineOfSight)
                    caption =
                        "Map is in line of sight mode; select tokens to check sightlines, or double-click to return to tactical mode.";

                if (_fDraggedToken?.LinkedToken != null)
                {
                    var link = find_link(_fDraggedToken.Token, _fDraggedToken.LinkedToken);
                    if (link == null)
                    {
                        caption = "Release here to create a link.";
                    }
                    else
                    {
                        var linktext = link.Text == "" ? "link" : "'" + link.Text + "' link";
                        caption = "Release here to remove the " + linktext + ".";
                    }
                }
            }

            if (caption != "")
            {
                float delta = 10;
                var width = ClientRectangle.Width - 2 * delta;
                var size = e.Graphics.MeasureString(caption, Font, (int)width);
                var height = size.Height * 2;

                var rect = new RectangleF(delta, delta, width, height);
                var path = RoundedRectangle.Create(rect, height / 3);
                using (Brush b = new SolidBrush(Color.FromArgb(200, Color.Black)))
                {
                    e.Graphics.FillPath(b, path);
                }

                e.Graphics.DrawPath(Pens.Black, path);
                e.Graphics.DrawString(caption, Font, Brushes.White, rect, _centered);
            }
        }

        private void draw_grid_label(Graphics g, string str, Font font, RectangleF rect, StringFormat sf)
        {
            for (var dx = -2; dx <= 2; ++dx)
            for (var dy = -2; dy <= 2; ++dy)
            {
                var outlineRect = new RectangleF(rect.X + dx, rect.Y + dy, rect.Width, rect.Height);
                using (Brush outline = new SolidBrush(Color.FromArgb(50, Color.White)))
                {
                    g.DrawString(str, font, outline, outlineRect, sf);
                }
            }

            g.DrawString(str, font, Brushes.Black, rect, sf);
        }

        private void draw_tile(Graphics g, Tile tile, int rotation, RectangleF rect, bool ghost)
        {
            try
            {
                var tileImg = tile.Image;
                if (tileImg == null)
                    tileImg = tile.BlankImage;

                var ia = new ImageAttributes();
                if (ghost)
                {
                    var cm = new ColorMatrix();
                    cm.Matrix33 = 0.4F;
                    ia.SetColorMatrix(cm);
                }

                var tileRect = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

                var turns = rotation % 4;
                switch (turns)
                {
                    case 0:
                    {
                        using (var img = new Bitmap(tileImg))
                        {
                            g.DrawImage(img, tileRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                        break;
                    case 1:
                    {
                        using (var img = new Bitmap(tileImg))
                        {
                            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            g.DrawImage(img, tileRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                        break;
                    case 2:
                    {
                        using (var img = new Bitmap(tileImg))
                        {
                            img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            g.DrawImage(img, tileRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                        break;
                    case 3:
                    {
                        using (var img = new Bitmap(tileImg))
                        {
                            img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            g.DrawImage(img, tileRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                        break;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void draw_creature(Graphics g, Point pt, EncounterCard card, CombatData data, bool selected,
            bool hovered, bool ghost)
        {
            var creature = Session.FindCreature(card.CreatureId, SearchType.Global);
            if (creature == null)
                return;

            var c = Color.Black;

            if (creature is Npc)
                c = Color.DarkBlue;

            if (data != null)
            {
                var slot = _fEncounter.FindSlot(data);
                var state = slot.GetState(data);
                switch (state)
                {
                    case CreatureState.Bloodied:
                        c = Color.Maroon;
                        break;
                    case CreatureState.Defeated:
                        c = Color.Gray;
                        break;
                }
            }

            var boxed = false;
            foreach (var token in BoxedTokens)
                if (token is CreatureToken)
                {
                    var ct = token as CreatureToken;
                    if (ct.Data.Id == data.Id)
                    {
                        boxed = true;
                        break;
                    }
                }

            var visible = data.Visible;
            if (!visible && _fShowCreatures == CreatureViewMode.Visible)
                return;

            var name = "";
            if (_fShowCreatureLabels)
                name = data.DisplayName;
            else
                name = creature.Category;
            var caption = TextHelper.Abbreviation(name);

            ghost = ghost || !visible;
            draw_token(g, pt, creature.Size, creature.Image, c, caption, selected, boxed, hovered, ghost,
                data.Conditions, data.Altitude);

            if (_fShowHealthBars && data != null)
            {
                var creatureSize = Creature.GetSize(creature.Size);
                var size = new Size(creatureSize, creatureSize);

                var rect = _fLayoutData.GetRegion(pt, size);
                draw_health_bar(g, rect, data, card.Hp);
            }
        }

        private void draw_hero(Graphics g, Point pt, Hero hero, bool selected, bool hovered, bool ghost)
        {
            var c = Color.FromArgb(0, 80, 0);

            var hpMax = hero.Hp;
            if (hpMax != 0)
            {
                var hpCurrent = hpMax + hero.CombatData.TempHp - hero.CombatData.Damage;
                if (hpCurrent <= 0)
                    c = Color.Gray;
                else if (hpCurrent <= hpMax / 2)
                    c = Color.Maroon;
            }

            var boxed = BoxedTokens.Contains(hero);
            var caption = TextHelper.Abbreviation(hero.Name);

            var visible = true;
            if (!visible && _fShowCreatures == CreatureViewMode.Visible)
                return;

            ghost = ghost || !visible;
            draw_token(g, pt, hero.Size, hero.Portrait, c, caption, selected, boxed, hovered, ghost,
                hero.CombatData.Conditions, hero.CombatData.Altitude);

            if (_fShowHealthBars && hero.CombatData != null && hero.Hp != 0)
            {
                var sz = Creature.GetSize(hero.Size);
                var size = new Size(sz, sz);

                var rect = _fLayoutData.GetRegion(pt, size);
                draw_health_bar(g, rect, hero.CombatData, hero.Hp);
            }
        }

        private void draw_custom(Graphics g, Point pt, CustomToken ct, bool selected, bool hovered, bool ghost)
        {
            var boxed = BoxedTokens.Contains(ct);
            var caption = TextHelper.Abbreviation(ct.Name);

            var visible = ct.Data.Visible;
            if (!visible && _fShowCreatures == CreatureViewMode.Visible)
                return;

            switch (ct.Type)
            {
                case CustomTokenType.Token:
                {
                    ghost = ghost || !visible;
                    var conditions = new List<OngoingCondition>();
                    draw_token(g, pt, ct.TokenSize, ct.Image, ct.Colour, caption, selected, boxed, hovered, ghost,
                        conditions, 0);
                }
                    break;
                case CustomTokenType.Overlay:
                {
                    var rect = _fLayoutData.GetRegion(pt, ct.OverlaySize);

                    var corners = RoundedRectangle.RectangleCorners.All;
                    var alpha = boxed ? 220 : 140;
                    if (ct.OverlayStyle == OverlayStyle.Block)
                    {
                        corners = RoundedRectangle.RectangleCorners.None;
                        alpha = 255;
                    }

                    var rounding = _fLayoutData.SquareSize * 0.3f;
                    var path = RoundedRectangle.Create(rect, rounding, corners);

                    if (ct.Image == null)
                    {
                        using (Brush b = new SolidBrush(Color.FromArgb(alpha, ct.Colour)))
                        {
                            g.FillPath(b, path);
                        }

                        if (_fShowCreatureLabels)
                        {
                            var p = selected || hovered ? Pens.White : new Pen(ct.Colour);
                            g.DrawPath(p, path);
                        }
                    }
                    else
                    {
                        if (ct.OverlayStyle == OverlayStyle.Rounded)
                        {
                            var cm = new ColorMatrix();
                            cm.Matrix33 = 0.4F;

                            var ia = new ImageAttributes();
                            ia.SetColorMatrix(cm);

                            var imgRect = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                            g.SetClip(path);
                            g.DrawImage(ct.Image, imgRect, 0, 0, ct.Image.Width, ct.Image.Height, GraphicsUnit.Pixel,
                                ia);
                            g.ResetClip();
                        }
                        else
                        {
                            g.DrawImage(ct.Image, rect);
                        }

                        var outline = selected || hovered;
                        if (outline && _fShowCreatureLabels)
                            using (var p = new Pen(Color.FromArgb(alpha, Color.White)))
                            {
                                g.DrawPath(p, path);
                            }
                    }

                    if (ct.DifficultTerrain)
                        for (var x = pt.X; x < pt.X + ct.OverlaySize.Width; ++x)
                        for (var y = pt.Y; y < pt.Y + ct.OverlaySize.Height; ++y)
                        {
                            // Draw difficult terrain mark

                            var square = _fLayoutData.GetRegion(new Point(x, y), new Size(1, 1));
                            var delta = square.Width / 10;
                            var side = square.Width / 5;

                            var xRight = square.X + square.Width - delta;
                            var yBottom = square.Y + side + delta;

                            var top = new PointF(xRight - side / 2, yBottom - side);
                            var left = new PointF(xRight - side, yBottom);
                            var right = new PointF(xRight, yBottom);

                            using (Brush b = new SolidBrush(Color.FromArgb(180, Color.White)))
                            {
                                g.FillPolygon(b, new[] { top, left, right });
                            }

                            g.DrawPolygon(Pens.DarkGray, new[] { top, left, right });
                        }

                    if (SelectedTokens.Contains(ct) && _fShowCreatureLabels)
                    {
                        var format = _centered;
                        if (rect.Height > rect.Width)
                        {
                            format = new StringFormat(_centered);
                            format.FormatFlags = format.FormatFlags | StringFormatFlags.DirectionVertical;
                        }

                        using (var f = new Font(Font.FontFamily, _fLayoutData.SquareSize / 5))
                        {
                            var textSize = g.MeasureString(ct.Name, f, rect.Size, format);
                            textSize += new SizeF(4F, 4F);

                            var textRect = new RectangleF(rect.X + (rect.Width - textSize.Width) / 2,
                                rect.Y + (rect.Height - textSize.Height) / 2, textSize.Width, textSize.Height);

                            g.DrawRectangle(Pens.Black, textRect.X, textRect.Y, textRect.Width, textRect.Height);

                            using (Brush brush = new SolidBrush(Color.FromArgb(210, Color.White)))
                            {
                                g.FillRectangle(brush, textRect);
                            }

                            g.DrawString(ct.Name, f, Brushes.Black, textRect, format);
                        }
                    }
                }
                    break;
            }
        }

        private void draw_token(Graphics g, Point pt, CreatureSize size, Image img, Color c, string text, bool selected,
            bool boxed, bool hovered, bool ghost, List<OngoingCondition> conditions, int altitude)
        {
            try
            {
                var sz = Creature.GetSize(size);

                var squareRect = _fLayoutData.GetRegion(pt, new Size(sz, sz));
                var tokenRect = squareRect;

                if (boxed) g.FillRectangle(Brushes.Blue, tokenRect);

                if (size == CreatureSize.Small || size == CreatureSize.Tiny)
                {
                    var delta = tokenRect.Width / 7;
                    tokenRect = new RectangleF(tokenRect.X + delta, tokenRect.Y + delta, tokenRect.Width - 2 * delta,
                        tokenRect.Height - 2 * delta);
                }

                if (img == null)
                {
                    var outline = Pens.White;
                    if (selected || hovered)
                        outline = Pens.Red;

                    float delta = 2;
                    var inner = new RectangleF(tokenRect.X + delta, tokenRect.Y + delta, tokenRect.Width - 2 * delta,
                        tokenRect.Height - 2 * delta);

                    var alpha = ghost ? 140 : 255;
                    using (Brush b = new SolidBrush(Color.FromArgb(alpha, c)))
                    {
                        g.FillEllipse(b, tokenRect);
                    }

                    g.DrawEllipse(outline, inner);

                    // Label it
                    var fontSize = _fLayoutData.SquareSize * sz / 6;
                    if (fontSize > 0)
                        using (var f = new Font(Font.FontFamily, fontSize))
                        {
                            g.DrawString(text, f, Brushes.White, tokenRect, _centered);
                        }
                }
                else
                {
                    var ia = new ImageAttributes();
                    if (ghost)
                    {
                        var cm = new ColorMatrix();
                        cm.Matrix33 = 0.4F;
                        ia.SetColorMatrix(cm);
                    }

                    var imgRect = new Rectangle((int)tokenRect.X, (int)tokenRect.Y, (int)tokenRect.Width,
                        (int)tokenRect.Height);

                    if (_fShowPictureTokens)
                    {
                        var rounding = Math.Min(tokenRect.Width, _fLayoutData.SquareSize) * 0.5f;
                        var path = RoundedRectangle.Create(tokenRect, rounding);

                        g.SetClip(path);
                        g.DrawImage(img, imgRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                        g.ResetClip();

                        var borderColour = Color.Black;
                        float borderWidth = 2;
                        if (selected || hovered)
                        {
                            borderColour = Color.Red;
                            borderWidth = 1;
                        }

                        using (var border = new Pen(borderColour, borderWidth))
                        {
                            g.DrawPath(border, path);
                        }

                        if (c == Color.Maroon)
                        {
                            var overlay = Color.FromArgb(100, Color.Red);
                            using (Brush b = new SolidBrush(overlay))
                            {
                                g.FillPath(b, path);
                            }
                        }
                    }
                    else
                    {
                        // Just draw the image
                        g.DrawImage(img, imgRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);

                        if (c == Color.Maroon)
                        {
                            var overlay = Color.FromArgb(100, Color.Red);
                            using (Brush b = new SolidBrush(overlay))
                            {
                                g.FillRectangle(b, imgRect);
                            }
                        }
                    }
                }

                if (boxed)
                    using (var p = new Pen(Color.White, 2))
                    {
                        g.DrawRectangle(p, squareRect.X, squareRect.Y, squareRect.Width, squareRect.Height);
                    }

                if (_fShowConditions && conditions.Count != 0)
                {
                    // Get badge size
                    var diameter = _fLayoutData.SquareSize * 0.4f;

                    // Get badge location
                    var badgeTopleft = new PointF(squareRect.Right - diameter, squareRect.Top);

                    // Get badge rectangle
                    var badgeRect = new RectangleF(badgeTopleft.X, badgeTopleft.Y, diameter, diameter);

                    // Draw badge
                    using (Brush b = new SolidBrush(Color.FromArgb(200, 0, 0)))
                    {
                        g.FillEllipse(b, badgeRect);
                    }

                    using (var font = new Font(Font.FontFamily, diameter / 3, Font.Style | FontStyle.Bold))
                    {
                        g.DrawString(conditions.Count.ToString(), font, Brushes.White, badgeRect, _centered);
                    }

                    using (var p = new Pen(Color.White, 2))
                    {
                        g.DrawEllipse(p, badgeRect);
                    }
                }

                if (altitude != 0)
                {
                    // Get badge size
                    var diameter = _fLayoutData.SquareSize * 0.4f;

                    // Get badge location
                    var badgeTopleft = new PointF(squareRect.Left, squareRect.Top);

                    // Get badge rectangle
                    var badgeRect = new RectangleF(badgeTopleft.X, badgeTopleft.Y, diameter, diameter);

                    var alt = (altitude > 0 ? "↑" : "↓") + Math.Abs(altitude);

                    // Draw badge
                    using (Brush b = new SolidBrush(Color.FromArgb(80, 80, 80)))
                    {
                        g.FillEllipse(b, badgeRect);
                    }

                    using (var font = new Font(Font.FontFamily, diameter / 3, Font.Style | FontStyle.Bold))
                    {
                        g.DrawString(alt, font, Brushes.White, badgeRect, _centered);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void draw_token_placeholder(Graphics g, Point startLocation, Point newLocation, CreatureSize size,
            bool hasPicture)
        {
            var sz = Creature.GetSize(size);

            var startRect = _fLayoutData.GetRegion(startLocation, new Size(sz, sz));
            var newRect = _fLayoutData.GetRegion(newLocation, new Size(sz, sz));

            if (size == CreatureSize.Small || size == CreatureSize.Tiny)
            {
                var d = startRect.Width / 7;
                startRect = new RectangleF(startRect.X + d, startRect.Y + d, startRect.Width - 2 * d,
                    startRect.Height - 2 * d);
            }

            var delta = 2;
            var inner = new RectangleF(startRect.X + delta, startRect.Y + delta, startRect.Width - 2 * delta,
                startRect.Height - 2 * delta);

            if (hasPicture)
            {
                var rounding = Math.Min(startRect.Width, _fLayoutData.SquareSize) * 0.5f;
                var path = RoundedRectangle.Create(startRect, rounding);

                using (Brush b = new SolidBrush(Color.FromArgb(180, Color.White)))
                {
                    g.FillPath(b, path);
                    g.DrawPath(Pens.Red, path);
                }
            }
            else
            {
                using (Brush b = new SolidBrush(Color.FromArgb(180, Color.White)))
                {
                    g.FillEllipse(b, inner);
                    g.DrawEllipse(Pens.Red, inner);
                }
            }

            using (var f = new Font(Font.FontFamily, _fLayoutData.SquareSize * sz / 4))
            {
                var squares = get_distance(startLocation, newLocation);
                g.DrawString(squares.ToString(), f, Brushes.Red, inner, _centered);
            }

            var fromPt = new PointF(startRect.X + startRect.Width / 2, startRect.Y + startRect.Height / 2);
            var toPt = new PointF(newRect.X + newRect.Width / 2, newRect.Y + newRect.Height / 2);

            double radius = inner.Width / 2;
            if (newLocation.X == startLocation.X)
            {
                fromPt.Y += newLocation.Y > startLocation.Y ? (float)radius : -(float)radius;
                toPt.Y += newLocation.Y > startLocation.Y ? -(float)radius : (float)radius;
            }
            else
            {
                var theta = Math.Atan((newLocation.Y - startLocation.Y) / (double)(newLocation.X - startLocation.X));
                var x = (float)(radius * Math.Cos(theta));
                var y = (float)(radius * Math.Sin(theta));

                fromPt.X += newLocation.X > startLocation.X ? x : -x;
                fromPt.Y += newLocation.X > startLocation.X ? y : -y;

                toPt.X += newLocation.X > startLocation.X ? -x : x;
                toPt.Y += newLocation.X > startLocation.X ? -y : y;
            }

            g.DrawLine(Pens.Red, fromPt, toPt);
        }

        private void draw_sketch(Graphics g, MapSketch sketch)
        {
            using (var p = new Pen(sketch.Colour, sketch.Width))
            {
                for (var index = 1; index < sketch.Points.Count; ++index)
                {
                    var pt1 = get_point(sketch.Points[index - 1]);
                    var pt2 = get_point(sketch.Points[index]);

                    g.DrawLine(p, pt1, pt2);
                }
            }
        }

        private void draw_health_bar(Graphics g, RectangleF rect, CombatData data, int hpFull)
        {
            var hpMax = hpFull + data.TempHp;
            var hpCurrent = hpFull - data.Damage;

            var barColour = Color.Green;
            if (hpCurrent <= 0)
                barColour = Color.Black;
            var hpBloodied = hpFull / 2;
            if (hpCurrent <= hpBloodied)
                barColour = Color.Maroon;

            hpCurrent = Math.Max(hpCurrent, 0);
            var thpFraction = (float)(hpCurrent + data.TempHp) / hpMax;
            var hpFraction = (float)hpCurrent / hpMax;

            var barHeight = Math.Max(rect.Height / 12, 4);
            var barRect = new RectangleF(rect.X, rect.Bottom - barHeight, rect.Width, barHeight);
            g.FillRectangle(Brushes.White, barRect);

            if (data.TempHp > 0)
            {
                var thpRect = new RectangleF(barRect.X, barRect.Y, barRect.Width * thpFraction, barRect.Height);
                g.FillRectangle(Brushes.Blue, thpRect);
            }

            using (Brush b = new SolidBrush(barColour))
            {
                var hpRect = new RectangleF(barRect.X, barRect.Y, barRect.Width * hpFraction, barRect.Height);
                g.FillRectangle(b, hpRect);
            }

            g.DrawRectangle(Pens.Black, barRect.X, barRect.Y, barRect.Width, barRect.Height);
        }

        /// <summary>
        ///     Called in response to the MouseDown event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            try
            {
                Focus();

                if (_fMap == null)
                    return;

                if (_fLayoutData == null)
                    _fLayoutData = new MapData(this, _fScalingFactor);

                if (_fDrawing != null)
                {
                    if (_fDrawing.CurrentSketch == null)
                        _fDrawing.CurrentSketch = new MapSketch();

                    return;
                }

                var cursor = PointToClient(Cursor.Position);
                var square = _fLayoutData.GetSquareAtPoint(cursor);

                if (_fAllowScrolling)
                {
                    if (_fViewpoint == Rectangle.Empty)
                    {
                        // Set the zoom area
                        _fViewpoint = get_current_zoom_rect(true);

                        _fLayoutData = null;
                        Invalidate();
                    }

                    // Start scrolling
                    _fScrollingData = new ScrollingData();
                    _fScrollingData.Start = square;

                    return;
                }

                if (_fTactical && _fEncounter != null)
                {
                    // Is there a creature here?
                    var slotData = get_token_at(square);
                    if (slotData != null)
                    {
                        var shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
                        var ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
                        var right = e.Button == MouseButtons.Right;

                        var creature = slotData.First as CreatureToken;
                        var custom = slotData.First as CustomToken;
                        if (creature != null && !is_visible(creature.Data))
                        {
                            if (!shift && !ctrl && !right)
                                SelectedTokens.Clear();
                        }
                        else if (custom != null && !is_visible(custom.Data))
                        {
                            if (!shift && !ctrl && !right)
                                SelectedTokens.Clear();
                        }
                        else if (custom != null && custom.CreatureId != Guid.Empty)
                        {
                            if (!shift && !ctrl && !right)
                                SelectedTokens.Clear();
                        }
                        else
                        {
                            if (shift || ctrl)
                            {
                                if (e.Button == MouseButtons.Left)
                                {
                                    // Add to / remove from highlighted tokens

                                    var wasSelected = false;
                                    foreach (var token in SelectedTokens)
                                        if (get_token_location(token) == get_token_location(slotData.First))
                                        {
                                            wasSelected = true;
                                            SelectedTokens.Remove(token);
                                            break;
                                        }

                                    if (!wasSelected)
                                        SelectedTokens.Add(slotData.First);

                                    OnSelectedTokensChanged();
                                }
                            }
                            else
                            {
                                // Begin dragging it
                                _fDraggedToken = new DraggedToken();
                                _fDraggedToken.Token = slotData.First;
                                _fDraggedToken.Start = slotData.Second.Location;
                                _fDraggedToken.Location = _fDraggedToken.Start;
                                _fDraggedToken.Offset = new Size(square.X - slotData.Second.Location.X,
                                    square.Y - slotData.Second.Location.Y);

                                // Is this token selected?
                                var selected = false;
                                var cd1 = get_combat_data(slotData.First);
                                foreach (var token in SelectedTokens)
                                {
                                    var cd2 = get_combat_data(token);

                                    if (cd1.Id == cd2.Id)
                                    {
                                        selected = true;
                                        break;
                                    }
                                }

                                if (!selected)
                                {
                                    SelectedTokens.Clear();
                                    SelectedTokens.Add(slotData.First);

                                    OnSelectedTokensChanged();
                                }
                            }
                        }
                    }
                    else
                    {
                        SelectedTokens.Clear();
                        OnSelectedTokensChanged();
                    }
                }

                if (_fMode != MapViewMode.Normal)
                    return;

                // Set selected tile
                if (_fSelectedTiles == null)
                    _fSelectedTiles = new List<TileData>();
                var addToSelection = ModifierKeys == Keys.Control || ModifierKeys == Keys.Shift;
                if (!addToSelection)
                    _fSelectedTiles.Clear();
                var td = _fLayoutData.GetTileAtSquare(square);
                if (td != null && _fMap.Tiles.Contains(td))
                    _fSelectedTiles.Add(td);

                switch (e.Button)
                {
                    case MouseButtons.Left:
                    {
                        if (_fSelectedTiles.Count != 0)
                        {
                            // Start dragging
                            _fDraggedTiles = new DraggedTiles();
                            _fDraggedTiles.Tiles = _fSelectedTiles;
                            _fDraggedTiles.Start = cursor;
                        }
                    }
                        break;
                    case MouseButtons.Right:
                    {
                        // Draw region outline
                        _fCurrentOutline = Rectangle.Empty;
                        _fDraggedOutline = new DraggedOutline();
                        _fDraggedOutline.Start = cursor;
                        _fDraggedOutline.Region = new Rectangle(square, new Size(1, 1));
                    }
                        break;
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
                if (_fMap == null)
                    return;

                if (_fLayoutData == null)
                    _fLayoutData = new MapData(this, _fScalingFactor);

                var cursor = PointToClient(Cursor.Position);
                var square = _fLayoutData.GetSquareAtPoint(cursor);

                if (_fDrawing != null)
                {
                    if (_fDrawing.CurrentSketch != null)
                    {
                        var squareRect = _fLayoutData.GetRegion(square, new Size(1, 1));
                        var dx = (cursor.X - squareRect.X) / squareRect.Width;
                        var dy = (cursor.Y - squareRect.Y) / squareRect.Height;

                        var msp = new MapSketchPoint();
                        msp.Square = square;
                        msp.Location = new PointF(dx, dy);

                        var pt = get_point(msp);
                        Console.WriteLine(pt);

                        _fDrawing.CurrentSketch.Points.Add(msp);
                        Invalidate();
                    }

                    return;
                }

                if (_fAllowScrolling)
                {
                    if (_fScrollingData != null)
                        if (_fViewpoint != Rectangle.Empty)
                            if (_fScrollingData.Start != square)
                            {
                                var dx = _fScrollingData.Start.X - square.X;
                                var dy = _fScrollingData.Start.Y - square.Y;

                                _fViewpoint.X += dx;
                                _fViewpoint.Y += dy;

                                _fLayoutData = null;
                                Invalidate();
                            }

                    return;
                }

                if (_fTactical)
                {
                    var hoverChanged = false;

                    if (_fDraggedToken == null)
                    {
                        if ((ModifierKeys & Keys.Control) == Keys.Control)
                        {
                            // Ignore this
                        }
                        else
                        {
                            _fHoverTokenLink = null;
                            foreach (var link in _fTokenLinkRegions.Keys)
                            {
                                var rect = _fTokenLinkRegions[link];
                                if (rect.Contains(cursor))
                                    _fHoverTokenLink = link;
                            }

                            var slotData = get_token_at(square);
                            if (slotData != null)
                            {
                                var creature = slotData.First as CreatureToken;
                                var custom = slotData.First as CustomToken;
                                if (creature != null && !is_visible(creature.Data))
                                {
                                    // Ignore this
                                }
                                else if (custom != null && !is_visible(custom.Data))
                                {
                                    // Ignore this
                                }
                                else
                                {
                                    if (_fHoverToken == null)
                                    {
                                        hoverChanged = true;
                                    }
                                    else
                                    {
                                        if (slotData.First is CreatureToken)
                                        {
                                            var token1 = _fHoverToken as CreatureToken;
                                            var token2 = slotData.First as CreatureToken;

                                            hoverChanged = token1 == null || token1.Data.Id != token2.Data.Id;
                                        }

                                        if (slotData.First is CustomToken)
                                        {
                                            var token1 = _fHoverToken as CustomToken;
                                            var token2 = slotData.First as CustomToken;

                                            hoverChanged = token1 == null || token1.Data.Id != token2.Data.Id;
                                        }

                                        if (slotData.First is Hero)
                                        {
                                            var token1 = _fHoverToken as Hero;
                                            var token2 = slotData.First as Hero;

                                            hoverChanged = token1 == null || token1.Id != token2.Id;
                                        }
                                    }

                                    _fHoverToken = slotData.First;
                                }
                            }
                            else
                            {
                                if (_fHoverToken != null)
                                {
                                    hoverChanged = true;
                                    _fHoverToken = null;
                                }
                            }
                        }
                    }

                    if (_fDraggedToken != null)
                    {
                        _fDraggedToken.LinkedToken = null;

                        var pt = square - _fDraggedToken.Offset;
                        var size = get_token_size(_fDraggedToken.Token);
                        var creatureRect = new Rectangle(pt, size);

                        var ct = _fDraggedToken.Token as CustomToken;
                        var draggingOverlay = ct != null && ct.Type == CustomTokenType.Overlay;

                        if (draggingOverlay || allow_creature_move(creatureRect, _fDraggedToken.Start))
                        {
                            _fDraggedToken.Location = pt;
                            OnTokenDragged();
                        }
                        else if (AllowLinkCreation)
                        {
                            var slotData = get_token_at(square);
                            if (slotData != null)
                            {
                                _fDraggedToken.Location = _fDraggedToken.Start;
                                _fDraggedToken.LinkedToken = slotData.First;
                                OnTokenDragged();
                            }
                        }
                    }

                    if (hoverChanged)
                        OnHoverTokenChanged();

                    Invalidate();
                }

                MapArea hoverArea = null;
                foreach (var area in _fMap.Areas)
                    if (area.Region.Contains(square))
                        hoverArea = area;

                if (HighlightedArea != hoverArea)
                {
                    HighlightedArea = hoverArea;
                    OnHighlightedAreaChanged();

                    Invalidate();
                }

                if (_fMode != MapViewMode.Normal)
                    return;

                if (_fDraggedTiles != null)
                {
                    // Move selected tiles

                    foreach (var td in _fDraggedTiles.Tiles)
                    {
                        var t = _fLayoutData.Tiles[td];

                        var dx = (int)((cursor.X - _fDraggedTiles.Start.X) / _fLayoutData.SquareSize);
                        var dy = (int)((cursor.Y - _fDraggedTiles.Start.Y) / _fLayoutData.SquareSize);
                        _fDraggedTiles.Offset = new Size(dx, dy);

                        var sq = new Point(td.Location.X + dx, td.Location.Y + dy);
                        var tilesize = t.Size;
                        if (td.Rotations % 2 != 0)
                            tilesize = new Size(t.Size.Height, t.Size.Width);
                        _fDraggedTiles.Region = _fLayoutData.GetRegion(sq, tilesize);
                    }

                    Invalidate();
                }
                else if (_fDraggedOutline != null)
                {
                    // Update region outline

                    var startSquare = _fLayoutData.GetSquareAtPoint(_fDraggedOutline.Start);
                    var currentSquare = _fLayoutData.GetSquareAtPoint(cursor);

                    var x = Math.Min(currentSquare.X, startSquare.X);
                    var y = Math.Min(currentSquare.Y, startSquare.Y);
                    var width = Math.Abs(currentSquare.X - startSquare.X) + 1;
                    var height = Math.Abs(currentSquare.Y - startSquare.Y) + 1;

                    _fDraggedOutline.Region = new Rectangle(x, y, width, height);

                    Invalidate();
                }
                else
                {
                    _fHoverTile = _fLayoutData.GetTileAtSquare(square);

                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        /// <summary>
        ///     Called in response to the MouseUp event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            try
            {
                if (_fMap == null)
                    return;

                if (_fLayoutData == null)
                    _fLayoutData = new MapData(this, _fScalingFactor);

                if (_fDrawing != null)
                {
                    if (_fDrawing.CurrentSketch != null)
                    {
                        Sketches.Add(_fDrawing.CurrentSketch);
                        OnSketchCreated(_fDrawing.CurrentSketch);
                    }

                    _fDrawing.CurrentSketch = null;
                    Invalidate();

                    return;
                }

                var cursor = PointToClient(Cursor.Position);

                if (_fScrollingData != null)
                {
                    if (_fViewpoint != Rectangle.Empty)
                    {
                        // Set the new zoom rectangle
                        _fViewpoint = get_current_zoom_rect(true);

                        _fLayoutData = null;
                        Invalidate();
                    }

                    // Stop scrolling
                    _fScrollingData = null;

                    return;
                }

                if (_fTactical)
                {
                    if (_fDraggedToken != null && _fDraggedToken.Location != _fDraggedToken.Start)
                    {
                        // Finish dragging the token
                        var distance = get_distance(_fDraggedToken.Location, _fDraggedToken.Start);

                        if (_fDraggedToken.Token is CreatureToken)
                        {
                            var token = _fDraggedToken.Token as CreatureToken;
                            var slot = _fEncounter.FindSlot(token.SlotId);
                            var cd = slot.FindCombatData(_fDraggedToken.Start);
                            cd.Location = _fDraggedToken.Location;
                        }

                        if (_fDraggedToken.Token is Hero)
                        {
                            var hero = _fDraggedToken.Token as Hero;
                            hero.CombatData.Location = _fDraggedToken.Location;
                        }

                        if (_fDraggedToken.Token is CustomToken)
                        {
                            var ct = _fDraggedToken.Token as CustomToken;
                            ct.Data.Location = _fDraggedToken.Location;
                        }

                        _fDraggedToken = null;
                        OnItemMoved(distance);
                    }

                    if (_fDraggedToken?.LinkedToken != null)
                    {
                        var currentLink = find_link(_fDraggedToken.Token, _fDraggedToken.LinkedToken);
                        if (currentLink == null)
                        {
                            if (_fDraggedToken.Token != _fDraggedToken.LinkedToken)
                            {
                                // Add a link between these tokens
                                var tokens = new List<IToken>();
                                tokens.Add(_fDraggedToken.Token);
                                tokens.Add(_fDraggedToken.LinkedToken);

                                var link = OnCreateTokenLink(tokens);
                                if (link != null)
                                    _fTokenLinks.Add(link);
                            }
                        }
                        else
                        {
                            _fTokenLinks.Remove(currentLink);
                        }

                        _fDraggedToken = null;
                    }

                    _fDraggedToken = null;
                    OnTokenDragged();

                    Invalidate();
                }

                var square = _fLayoutData.GetSquareAtPoint(cursor);
                MapArea selectedArea = null;
                foreach (var area in _fMap.Areas)
                    if (area.Region.Contains(square))
                        selectedArea = area;
                if (_fSelectedArea != selectedArea)
                {
                    _fSelectedArea = selectedArea;
                    OnAreaSelected(_fSelectedArea);

                    Invalidate();
                }

                if (_fMode != MapViewMode.Normal)
                    return;

                if (_fDraggedTiles != null)
                {
                    // Did we drag it at all?
                    if (cursor != _fDraggedTiles.Start)
                    {
                        var distance = get_distance(cursor, _fDraggedTiles.Start);

                        foreach (var td in _fDraggedTiles.Tiles)
                        {
                            // Finish dragging the tile
                            var x = td.Location.X + _fDraggedTiles.Offset.Width;
                            var y = td.Location.Y + _fDraggedTiles.Offset.Height;
                            td.Location = new Point(x, y);

                            // Move it to the end of the list
                            _fMap.Tiles.Remove(td);
                            _fMap.Tiles.Add(td);
                        }

                        OnItemMoved(distance);
                    }

                    _fDraggedTiles = null;

                    _fLayoutData = null;
                    Invalidate();
                }
                else if (_fDraggedOutline != null)
                {
                    // Did we drag it at all?
                    if (cursor != _fDraggedOutline.Start)
                    {
                        // Set the outline
                        _fCurrentOutline = _fDraggedOutline.Region;
                        OnRegionSelected();
                    }
                    else
                    {
                        var pt = _fLayoutData.GetSquareAtPoint(cursor);
                        var td = _fLayoutData.GetTileAtSquare(pt);
                        if (td != null)
                        {
                            _fSelectedTiles = new List<TileData>();
                            _fSelectedTiles.Add(td);

                            OnTileContext(td);
                        }
                    }

                    _fDraggedOutline = null;

                    Invalidate();
                }
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
                if (_fDrawing != null)
                {
                    if (_fDrawing.CurrentSketch != null)
                    {
                        Sketches.Add(_fDrawing.CurrentSketch);
                        OnSketchCreated(_fDrawing.CurrentSketch);
                    }

                    _fDrawing.CurrentSketch = null;
                    Invalidate();

                    return;
                }

                if (_fAllowScrolling)
                    return;

                if (_fTactical)
                {
                    _fDraggedToken = null;
                    OnTokenDragged();

                    Invalidate();
                }

                if (_fMode != MapViewMode.Normal)
                    return;

                _fHoverTile = null;
                _fHoverToken = null;
                _fHoverTokenLink = null;

                if (SelectedTokens.Count != 0)
                {
                    SelectedTokens.Clear();
                    OnSelectedTokensChanged();
                }

                if (HighlightedArea != null)
                {
                    HighlightedArea = null;
                    OnHighlightedAreaChanged();
                }

                // Cancel dragging and region outlining
                _fDraggedTiles = null;
                _fDraggedOutline = null;

                Invalidate();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        /// <summary>
        ///     Called in response to the DoubleClick event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnDoubleClick(EventArgs e)
        {
            try
            {
                if (_fLineOfSight)
                {
                    LineOfSight = false;
                    return;
                }

                if (_fDrawing != null)
                {
                    AllowDrawing = false;
                    return;
                }

                if (_fAllowScrolling)
                {
                    AllowScrolling = false;
                    return;
                }

                if (SelectedTokens.Count == 1)
                    OnTokenActivated(SelectedTokens[0]);

                if (HighlightedArea != null)
                    OnAreaActivated(HighlightedArea);

                if (_fHoverTokenLink != null)
                {
                    var index = _fTokenLinks.IndexOf(_fHoverTokenLink);

                    var link = OnEditTokenLink(_fHoverTokenLink);
                    if (link != null)
                    {
                        _fTokenLinks[index] = link;
                        Invalidate();
                    }
                }

                base.OnDoubleClick(e);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        /// <summary>
        ///     Called in response to the MouseWheel event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            _fAllowScrolling = true;

            OnMouseZoom(e);
        }

        /// <summary>
        ///     Returns whether a given keypress can be handled by the MapView control.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Returns true if the key was handled, false otherwise.</returns>
        public bool HandleKey(Keys key)
        {
            if (key == Keys.Left
                || key == Keys.Right
                || key == (Keys.Left | Keys.Shift)
                || key == (Keys.Right | Keys.Shift)
                || key == Keys.Up
                || key == Keys.Down
                || key == Keys.Delete)
                return true;

            return false;
        }

        /// <summary>
        ///     Determines whether the given key is an input key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if the key is an input key; false otherwise.</returns>
        protected override bool IsInputKey(Keys key)
        {
            if (HandleKey(key))
                return true;

            return base.IsInputKey(key);
        }

        /// <summary>
        ///     Called in response to the KeyDown event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            var removed = false;
            var moved = false;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (_fSelectedTiles != null && _fSelectedTiles.Count != 0)
                    {
                        if (e.Shift)
                        {
                            foreach (var td in _fSelectedTiles)
                                td.Rotations -= 1;
                            moved = true;
                        }
                        else
                        {
                            foreach (var td in _fSelectedTiles)
                                td.Location = new Point(td.Location.X - 1, td.Location.Y);
                            moved = true;
                        }
                    }

                    break;
                case Keys.Right:
                    if (_fSelectedTiles != null && _fSelectedTiles.Count != 0)
                    {
                        if (e.Shift)
                        {
                            foreach (var td in _fSelectedTiles)
                                td.Rotations += 1;
                            moved = true;
                        }
                        else
                        {
                            foreach (var td in _fSelectedTiles)
                                td.Location = new Point(td.Location.X + 1, td.Location.Y);
                            moved = true;
                        }
                    }

                    break;
                case Keys.Up:
                    if (_fSelectedTiles != null && _fSelectedTiles.Count != 0)
                    {
                        foreach (var td in _fSelectedTiles)
                            td.Location = new Point(td.Location.X, td.Location.Y - 1);
                        moved = true;
                    }

                    break;
                case Keys.Down:
                    if (_fSelectedTiles != null && _fSelectedTiles.Count != 0)
                    {
                        foreach (var td in _fSelectedTiles)
                            td.Location = new Point(td.Location.X, td.Location.Y + 1);
                        moved = true;
                    }

                    break;
                case Keys.Delete:
                    if (_fSelectedTiles != null && _fSelectedTiles.Count != 0)
                    {
                        foreach (var td in _fSelectedTiles)
                            _fMap.Tiles.Remove(td);
                        removed = true;
                    }

                    break;
            }

            _fLayoutData = null;
            Invalidate();

            if (moved)
                OnItemMoved(1);

            if (removed)
                OnItemRemoved();
        }

        /// <summary>
        ///     Called in response to the DragOver event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            var cursor = PointToClient(Cursor.Position);
            var square = _fLayoutData.GetSquareAtPoint(cursor);

            var tile = e.Data.GetData(typeof(Tile)) as Tile;
            if (tile != null)
            {
                e.Effect = DragDropEffects.Copy;

                _fNewTile = new NewTile();
                _fNewTile.Tile = tile;

                // Work out where the dragged tile is
                _fNewTile.Location = _fLayoutData.GetSquareAtPoint(cursor);
                _fNewTile.Region = _fLayoutData.GetRegion(_fNewTile.Location, tile.Size);

                Invalidate();
            }

            var token = e.Data.GetData(typeof(CreatureToken)) as CreatureToken;
            if (token != null)
            {
                var slot = _fEncounter.FindSlot(token.SlotId);

                var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                var size = Creature.GetSize(creature.Size);

                // Disallow drop if not over a tile
                if (allow_creature_move(new Rectangle(square, new Size(size, size)), CombatData.NoPoint))
                {
                    _fNewToken = new NewToken();
                    _fNewToken.Token = token;
                    _fNewToken.Location = square;

                    e.Effect = DragDropEffects.Move;

                    Invalidate();
                }
            }

            var hero = e.Data.GetData(typeof(Hero)) as Hero;
            if (hero != null)
            {
                var size = Creature.GetSize(hero.Size);

                // Disallow drop if not over a tile
                if (allow_creature_move(new Rectangle(square, new Size(size, size)), CombatData.NoPoint))
                {
                    _fNewToken = new NewToken();
                    _fNewToken.Token = hero;
                    _fNewToken.Location = square;

                    e.Effect = DragDropEffects.Move;

                    Invalidate();
                }
            }

            var custom = e.Data.GetData(typeof(CustomToken)) as CustomToken;
            if (custom != null)
            {
                _fNewToken = new NewToken();
                _fNewToken.Token = custom;
                _fNewToken.Location = square;

                e.Effect = DragDropEffects.Move;

                Invalidate();
            }
        }

        /// <summary>
        ///     Called in response to the DragLeave event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnDragLeave(EventArgs e)
        {
            _fNewTile = null;
            _fNewToken = null;

            Invalidate();
        }

        /// <summary>
        ///     Called in response to the DragDrop event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnDragDrop(DragEventArgs e)
        {
            var tile = e.Data.GetData(typeof(Tile)) as Tile;
            if (tile != null)
            {
                // Stop dragging the dragged tile; add it to the map

                var td = new TileData();
                td.TileId = _fNewTile.Tile.Id;
                td.Location = _fNewTile.Location;

                _fNewTile = null;

                _fMap.Tiles.Add(td);
                _fSelectedTiles = new List<TileData>();
                _fSelectedTiles.Add(td);

                _fLayoutData = null;

                Invalidate();
                OnItemDropped();
            }

            var data = e.Data.GetData(typeof(CreatureToken)) as CreatureToken;
            if (data != null)
            {
                // Stop dragging the creature; set this location

                data.Data.Location = _fNewToken.Location;

                _fNewToken = null;

                Invalidate();
                OnItemDropped();
            }

            var hero = e.Data.GetData(typeof(Hero)) as Hero;
            if (hero != null)
            {
                // Stop dragging the hero; set this location
                var h = _fNewToken.Token as Hero;
                h.CombatData.Location = _fNewToken.Location;

                _fNewToken = null;

                Invalidate();
                OnItemDropped();
            }

            var custom = e.Data.GetData(typeof(CustomToken)) as CustomToken;
            if (custom != null)
            {
                // Stop dragging the creature; set this location

                custom.Data.Location = _fNewToken.Location;

                _fNewToken = null;

                Invalidate();
                OnItemDropped();
            }
        }

        private int get_distance(Point from, Point to)
        {
            var dx = Math.Abs(from.X - to.X);
            var dy = Math.Abs(from.Y - to.Y);
            return Math.Max(dx, dy);
        }

        private Pair<IToken, Rectangle> get_token_at(Point square)
        {
            if (_fEncounter == null)
                return null;

            foreach (var slotId in _fSlotRegions.Keys)
            {
                // Find the size of the creature
                var rects = _fSlotRegions[slotId];
                foreach (var rect in rects)
                    if (rect.Contains(square))
                    {
                        var slot = _fEncounter.FindSlot(slotId);
                        var data = slot.FindCombatData(rect.Location);

                        var token = new CreatureToken();
                        token.SlotId = slotId;
                        token.Data = data;

                        return new Pair<IToken, Rectangle>(token, rect);
                    }
            }

            foreach (var h in Session.Project.Heroes)
            {
                if (h == null)
                    return null;

                // Find the hero rectangle
                var size = Creature.GetSize(h.Size);
                var rect = new Rectangle(h.CombatData.Location, new Size(size, size));

                if (rect.Contains(square))
                    return new Pair<IToken, Rectangle>(h, rect);
            }

            foreach (var ct in _fEncounter.CustomTokens)
                if (ct.Type == CustomTokenType.Token)
                {
                    // Find the hero rectangle
                    var size = ct.OverlaySize;
                    if (ct.Type == CustomTokenType.Token)
                    {
                        var sz = Creature.GetSize(ct.TokenSize);
                        size = new Size(sz, sz);
                    }

                    var rect = new Rectangle(ct.Data.Location, size);

                    if (rect.Contains(square))
                        return new Pair<IToken, Rectangle>(ct, rect);
                }

            foreach (var ct in _fEncounter.CustomTokens)
                if (ct.Type == CustomTokenType.Overlay)
                {
                    // Find the hero rectangle
                    var size = ct.OverlaySize;
                    if (ct.Type == CustomTokenType.Token)
                    {
                        var sz = Creature.GetSize(ct.TokenSize);
                        size = new Size(sz, sz);
                    }

                    var rect = new Rectangle(ct.Data.Location, size);

                    if (rect.Contains(square))
                        return new Pair<IToken, Rectangle>(ct, rect);
                }

            return null;
        }

        private bool allow_creature_move(Rectangle targetRect, Point initialLocation)
        {
            for (var x = 0; x != targetRect.Width; ++x)
            for (var y = 0; y != targetRect.Height; ++y)
            {
                var pt = new Point(x + targetRect.X, y + targetRect.Y);

                // Disallow if outside the viewpoint
                if (_fViewpoint != Rectangle.Empty && !_fViewpoint.Contains(pt))
                    return false;

                // Disallow if off the map
                if (_fLayoutData.GetTileAtSquare(pt) == null)
                    return false;

                // Disallow if over another token
                var slotData = get_token_at(pt);
                if (slotData != null && slotData.Second.Location != initialLocation)
                {
                    var ct = slotData.First as CustomToken;
                    var overlay = ct != null && ct.Type == CustomTokenType.Overlay;

                    // We can drag over an overlay, but not any other token
                    if (!overlay)
                        return false;
                }
            }

            return true;
        }

        private bool is_visible(CombatData cd)
        {
            if (cd == null)
                return false;

            switch (_fShowCreatures)
            {
                case CreatureViewMode.All:
                    return true;
                case CreatureViewMode.Visible:
                    return cd.Visible;
                case CreatureViewMode.None:
                    return false;
            }

            return false;
        }

        private Point get_token_location(IToken token)
        {
            if (token is CreatureToken)
            {
                var ct = token as CreatureToken;
                return ct.Data.Location;
            }

            if (token is Hero)
            {
                var h = token as Hero;
                return h.CombatData.Location;
            }

            if (token is CustomToken)
            {
                var ct = token as CustomToken;
                return ct.Data.Location;
            }

            return CombatData.NoPoint;
        }

        private Size get_token_size(IToken token)
        {
            if (token is CreatureToken)
            {
                var ct = token as CreatureToken;
                var slot = _fEncounter.FindSlot(ct.SlotId);
                var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                var size = Creature.GetSize(creature.Size);
                return new Size(size, size);
            }

            if (token is Hero)
            {
                var h = token as Hero;
                var size = Creature.GetSize(h.Size);
                return new Size(size, size);
            }

            if (token is CustomToken)
            {
                var ct = token as CustomToken;

                if (ct.Type == CustomTokenType.Token)
                {
                    var size = Creature.GetSize(ct.TokenSize);
                    return new Size(size, size);
                }

                if (ct.Type == CustomTokenType.Overlay)
                    return ct.OverlaySize;
            }

            return new Size(1, 1);
        }

        private RectangleF get_token_rect(IToken token)
        {
            var location = get_token_location(token);
            if (location == CombatData.NoPoint)
                return RectangleF.Empty;

            var size = get_token_size(token);
            return _fLayoutData.GetRegion(location, size);
        }

        private Rectangle get_current_zoom_rect(bool useScaling)
        {
            var data = useScaling ? new MapData(this, 1.0) : _fLayoutData;

            var topright = data.GetSquareAtPoint(new Point(1, 1));
            var bottomleft = data.GetSquareAtPoint(new Point(ClientRectangle.Right - 1, ClientRectangle.Bottom - 1));

            var width = 1 + (bottomleft.X - topright.X);
            var height = 1 + (bottomleft.Y - topright.Y);
            var size = new Size(width, height);

            return new Rectangle(topright, size);
        }

        private PointF get_point(MapSketchPoint msp)
        {
            var square = _fLayoutData.GetRegion(msp.Square, new Size(1, 1));

            var dx = square.Width * msp.Location.X;
            var dy = square.Height * msp.Location.Y;

            return new PointF(square.X + dx, square.Y + dy);
        }

        private CombatData get_combat_data(IToken token)
        {
            if (token is CreatureToken)
            {
                var ct = token as CreatureToken;
                return ct.Data;
            }

            if (token is CustomToken)
            {
                var ct = token as CustomToken;
                return ct.Data;
            }

            var hero = token as Hero;
            return hero?.CombatData;
        }

        private TokenLink find_link(IToken t1, IToken t2)
        {
            var r1 = get_token_rect(t1);
            var r2 = get_token_rect(t2);

            foreach (var link in _fTokenLinks)
            {
                var r3 = get_token_rect(link.Tokens[0]);
                var r4 = get_token_rect(link.Tokens[1]);

                var first = r1 == r3 || r2 == r3;
                var second = r1 == r4 || r2 == r4;

                if (first && second)
                    return link;
            }

            return null;
        }

        private PointF get_closest_vertex(Point pt)
        {
            var square = _fLayoutData.GetSquareAtPoint(pt);
            var rect = _fLayoutData.GetRegion(square, new Size(1, 1));

            var points = new List<PointF>();
            points.Add(new PointF(rect.Left, rect.Top));
            points.Add(new PointF(rect.Left, rect.Bottom - 1));
            points.Add(new PointF(rect.Right - 1, rect.Top));
            points.Add(new PointF(rect.Right - 1, rect.Bottom - 1));

            var minDistance = double.MaxValue;
            var nearest = PointF.Empty;

            foreach (var point in points)
            {
                var dx = point.X - pt.X;
                var dy = point.Y - pt.Y;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < minDistance)
                {
                    nearest = point;
                    minDistance = distance;
                }
            }

            return nearest;
        }

        private class NewTile
        {
            public Point Location = CombatData.NoPoint;
            public RectangleF Region = RectangleF.Empty;
            public Tile Tile;
        }

        private class DraggedTiles
        {
            public Size Offset = Size.Empty;
            public RectangleF Region = RectangleF.Empty;
            public Point Start = CombatData.NoPoint;
            public List<TileData> Tiles = new List<TileData>();
        }

        private class DraggedOutline
        {
            public Rectangle Region = Rectangle.Empty;
            public Point Start = CombatData.NoPoint;
        }

        private class NewToken
        {
            public Point Location = CombatData.NoPoint;
            public IToken Token;
        }

        private class DraggedToken
        {
            public IToken LinkedToken;
            public Point Location = CombatData.NoPoint;
            public Size Offset = Size.Empty;
            public Point Start = CombatData.NoPoint;
            public IToken Token;
        }

        private class ScrollingData
        {
            public Point Start = Point.Empty;
        }

        private class DrawingData
        {
            public MapSketch CurrentSketch;
        }
    }

    internal class MapData
    {
        // How far, in pixels, the top-left corner of the map is from the top-left corner of the control
        public SizeF MapOffset;
        public int MaxX = int.MinValue;
        public int MaxY = int.MinValue;

        public int MinX = int.MaxValue;
        public int MinY = int.MaxValue;

        public double ScalingFactor;
        public float SquareSize;
        public Dictionary<TileData, RectangleF> TileRegions = new Dictionary<TileData, RectangleF>();

        public Dictionary<TileData, Tile> Tiles = new Dictionary<TileData, Tile>();

        public Dictionary<TileData, Rectangle> TileSquares = new Dictionary<TileData, Rectangle>();

        public int Width => MaxX - MinX + 1;

        public int Height => MaxY - MinY + 1;

        public MapData(MapView mapview, double scalingFactor)
        {
            ScalingFactor = scalingFactor;

            // Work out max and min x and max and min y
            if (mapview.Map != null && mapview.Map.Tiles.Count != 0 ||
                mapview.BackgroundMap != null && mapview.BackgroundMap.Tiles.Count != 0)
            {
                var tiles = new List<TileData>();
                tiles.AddRange(mapview.Map.Tiles);
                if (mapview.BackgroundMap != null)
                    tiles.AddRange(mapview.BackgroundMap.Tiles);

                foreach (var td in tiles)
                {
                    var t = Session.FindTile(td.TileId, SearchType.Global);
                    if (t == null)
                        continue;

                    Tiles[td] = t;

                    // Work out the squares covered by this tile
                    Rectangle tileRect;
                    if (td.Rotations % 2 == 0)
                        tileRect = new Rectangle(td.Location.X, td.Location.Y, t.Size.Width, t.Size.Height);
                    else
                        tileRect = new Rectangle(td.Location.X, td.Location.Y, t.Size.Height, t.Size.Width);
                    TileSquares[td] = tileRect;

                    // Set max / min x and y

                    if (tileRect.X < MinX)
                        MinX = tileRect.X;

                    if (tileRect.Y < MinY)
                        MinY = tileRect.Y;

                    var right = tileRect.X + tileRect.Width - 1;
                    if (right > MaxX)
                        MaxX = right;

                    var bottom = tileRect.Y + tileRect.Height - 1;
                    if (bottom > MaxY)
                        MaxY = bottom;
                }
            }
            else
            {
                MinX = 0;
                MinY = 0;
                MaxX = 0;
                MaxY = 0;
            }

            if (mapview.Map != null && mapview.Viewpoint != Rectangle.Empty)
            {
                // Override this
                MinX = mapview.Viewpoint.X;
                MinY = mapview.Viewpoint.Y;
                MaxX = mapview.Viewpoint.X + mapview.Viewpoint.Width - 1;
                MaxY = mapview.Viewpoint.Y + mapview.Viewpoint.Height - 1;
            }
            else
            {
                // Add a border round the edge
                MinX -= mapview.BorderSize;
                MinY -= mapview.BorderSize;
                MaxX += mapview.BorderSize;
                MaxY += mapview.BorderSize;
            }

            // Work out square dimensions
            var squareWidth = (float)mapview.ClientRectangle.Width / Width;
            var squareHeight = (float)mapview.ClientRectangle.Height / Height;
            SquareSize = Math.Min(squareWidth, squareHeight);
            SquareSize *= (float)ScalingFactor;

            // Work out the map offset
            var xUsed = Width * SquareSize;
            var yUsed = Height * SquareSize;
            var xDiff = mapview.ClientRectangle.Width - xUsed;
            var yDiff = mapview.ClientRectangle.Height - yUsed;
            MapOffset = new SizeF(xDiff / 2, yDiff / 2);

            if (mapview.Map != null)
                // Work out painting region for each tile
                foreach (var td in Tiles.Keys)
                {
                    var squares = TileSquares[td];
                    TileRegions[td] = GetRegion(squares.Location, squares.Size);
                }
        }

        ~MapData()
        {
            Tiles.Clear();
            TileSquares.Clear();
            TileRegions.Clear();
        }

        public Point GetSquareAtPoint(Point pt)
        {
            // Remove the map offset
            var x = (int)(pt.X - MapOffset.Width);
            var y = (int)(pt.Y - MapOffset.Height);

            x = (int)(x / SquareSize);
            y = (int)(y / SquareSize);

            return new Point(x + MinX, y + MinY);
        }

        public TileData GetTileAtSquare(Point square)
        {
            TileData result = null;

            foreach (var td in TileSquares.Keys)
            {
                var squares = TileSquares[td];
                if (squares.Contains(square))
                    result = td;
            }

            return result;
        }

        public RectangleF GetRegion(Point square, Size size)
        {
            var x = (square.X - MinX) * SquareSize + MapOffset.Width;
            var y = (square.Y - MinY) * SquareSize + MapOffset.Height;

            var tileWidth = size.Width * SquareSize;
            var tileHeight = size.Height * SquareSize;

            return new RectangleF(x, y, tileWidth + 1, tileHeight + 1);
        }
    }
}
