using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Interface for a game element.
    /// </summary>
    public interface IElement
    {
        /// <summary>
        ///     Calculates the XP value of the game element.
        /// </summary>
        /// <returns>Returns the XP value.</returns>
        int GetXp();

        /// <summary>
        ///     Calculates the difficulty of the game element.
        /// </summary>
        /// <param name="party_level">The party level.</param>
        /// <param name="party_size">The party size.</param>
        /// <returns>Returns the difficulty.</returns>
        Difficulty GetDifficulty(int partyLevel, int partySize);

        /// <summary>
        ///     Creates a copy of the game element.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        IElement Copy();
    }

    /// <summary>
    ///     Enumeration containing types of plot point state.
    /// </summary>
    public enum PlotPointState
    {
        /// <summary>
        ///     The default state; the plot point is upcoming or in progress.
        /// </summary>
        Normal,

        /// <summary>
        ///     The plot point has been played.
        /// </summary>
        Completed,

        /// <summary>
        ///     The plot point was skipped.
        /// </summary>
        Skipped
    }

    /// <summary>
    ///     Enumeration containing allowed plot point colours.
    /// </summary>
    public enum PlotPointColour
    {
        /// <summary>
        ///     Yellow.
        /// </summary>
        Yellow,

        /// <summary>
        ///     Blue.
        /// </summary>
        Blue,

        /// <summary>
        ///     Green.
        /// </summary>
        Green,

        /// <summary>
        ///     Purple.
        /// </summary>
        Purple,

        /// <summary>
        ///     Orange.
        /// </summary>
        Orange,

        /// <summary>
        ///     Brown.
        /// </summary>
        Brown,

        /// <summary>
        ///     Grey.
        /// </summary>
        Grey
    }

    /// <summary>
    ///     Class representing a plot point.
    /// </summary>
    [Serializable]
    public class PlotPoint
    {
        private int _fAdditionalXp;

        private PlotPointColour _fColour = PlotPointColour.Yellow;

        private CalendarDate _fDate;

        private string _fDetails = "";

        private IElement _fElement;

        private List<Guid> _fEncyclopediaEntries = new List<Guid>();

        private Guid _fId = Guid.NewGuid();

        private List<Guid> _fLinks = new List<Guid>();

        private Guid _fMapLocationId = Guid.Empty;

        private string _fName = "";

        private List<Parcel> _fParcels = new List<Parcel>();

        private string _fReadAloud = "";

        private Guid _fRegionalMapId = Guid.Empty;

        private PlotPointState _fState = PlotPointState.Normal;

        private Plot _fSubplot = new Plot();

        /// <summary>
        ///     Gets or sets the point's unique ID.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the point's name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the point's state.
        /// </summary>
        public PlotPointState State
        {
            get => _fState;
            set => _fState = value;
        }

        /// <summary>
        ///     Gets or sets the point's colour.
        /// </summary>
        public PlotPointColour Colour
        {
            get => _fColour;
            set => _fColour = value;
        }

        /// <summary>
        ///     Gets or sets the point's details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the point's read-aloud text.
        /// </summary>
        public string ReadAloud
        {
            get => _fReadAloud;
            set => _fReadAloud = value;
        }

        /// <summary>
        ///     Gets or sets the list of the IDs of the plot points to which this point is linked.
        /// </summary>
        public List<Guid> Links
        {
            get => _fLinks;
            set => _fLinks = value;
        }

        /// <summary>
        ///     Gets or sets the subplot for this point.
        /// </summary>
        public Plot Subplot
        {
            get => _fSubplot;
            set => _fSubplot = value;
        }

        /// <summary>
        ///     Gets or sets this point's game element.
        /// </summary>
        public IElement Element
        {
            get => _fElement;
            set => _fElement = value;
        }

        /// <summary>
        ///     Gets or sets the list of treasure parcels held in this point.
        /// </summary>
        public List<Parcel> Parcels
        {
            get => _fParcels;
            set => _fParcels = value;
        }

        /// <summary>
        ///     Gets or sets the list of encyclopedia entry IDs.
        /// </summary>
        public List<Guid> EncyclopediaEntryIDs
        {
            get => _fEncyclopediaEntries;
            set => _fEncyclopediaEntries = value;
        }

        /// <summary>
        ///     Gets or sets the date of this plot point.
        /// </summary>
        public CalendarDate Date
        {
            get => _fDate;
            set => _fDate = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the regional map where the plot point takes place.
        /// </summary>
        public Guid RegionalMapId
        {
            get => _fRegionalMapId;
            set => _fRegionalMapId = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the map location where the plot point takes place.
        /// </summary>
        public Guid MapLocationId
        {
            get => _fMapLocationId;
            set => _fMapLocationId = value;
        }

        /// <summary>
        ///     Gets or sets the additional XP granted by this point.
        /// </summary>
        public int AdditionalXp
        {
            get => _fAdditionalXp;
            set => _fAdditionalXp = value;
        }

        /// <summary>
        ///     Returns the list of plot points leading from this point.
        /// </summary>
        public List<PlotPoint> Subtree
        {
            get
            {
                var points = new List<PlotPoint>();

                points.Add(this);
                foreach (var pp in _fSubplot.Points)
                    points.AddRange(pp.Subtree);

                return points;
            }
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public PlotPoint()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="name">The name of the point.</param>
        public PlotPoint(string name)
        {
            _fName = name;
        }

        /// <summary>
        ///     Calculates the XP value of this plot point.
        /// </summary>
        /// <returns>Returns the XP value.</returns>
        public int GetXp()
        {
            var xp = _fAdditionalXp;

            if (_fElement != null)
                xp += _fElement.GetXp();

            if (_fSubplot.Points.Count != 0)
            {
                var layers = Workspace.FindLayers(_fSubplot);

                foreach (var layer in layers)
                    xp += Workspace.GetLayerXp(layer);
            }

            return xp;
        }

        /// <summary>
        ///     Gets the tactical map and map area associated with the point, if any.
        /// </summary>
        /// <param name="map">The map associated with the point.</param>
        /// <param name="map_area">The map area associated with the point.</param>
        public void GetTacticalMapArea(ref Map map, ref MapArea mapArea)
        {
            var mapId = Guid.Empty;
            var mapAreaId = Guid.Empty;

            var enc = _fElement as Encounter;
            if (enc != null)
            {
                mapId = enc.MapId;
                mapAreaId = enc.MapAreaId;
            }

            var sc = _fElement as SkillChallenge;
            if (sc != null)
            {
                mapId = sc.MapId;
                mapAreaId = sc.MapAreaId;
            }

            var te = _fElement as TrapElement;
            if (te != null)
            {
                mapId = te.MapId;
                mapAreaId = te.MapAreaId;
            }

            var me = _fElement as MapElement;
            if (me != null)
            {
                mapId = me.MapId;
                mapAreaId = me.MapAreaId;
            }

            if (mapId != Guid.Empty && mapAreaId != Guid.Empty)
            {
                map = Session.Project.FindTacticalMap(mapId);
                if (map != null)
                    mapArea = map.FindArea(mapAreaId);
            }
        }

        /// <summary>
        ///     Gets the regional map and location associated with the point, if any.
        /// </summary>
        /// <param name="map">The map associated with the point.</param>
        /// <param name="map_location">The map location associated with the point.</param>
        /// <param name="project">The current project.</param>
        public void GetRegionalMapArea(ref RegionalMap map, ref MapLocation mapLocation, Project project)
        {
            if (_fRegionalMapId != Guid.Empty && _fMapLocationId != Guid.Empty)
            {
                map = Session.Project.FindRegionalMap(_fRegionalMapId);
                if (map != null)
                    mapLocation = map.FindLocation(_fMapLocationId);
            }
        }

        /// <summary>
        ///     Returns the name of the point.
        /// </summary>
        /// <returns>Returns the name of the point.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Creates a copy of the point.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public PlotPoint Copy()
        {
            var pp = new PlotPoint();

            pp.Id = _fId;
            pp.Name = _fName;
            pp.State = _fState;
            pp.Colour = _fColour;
            pp.Details = _fDetails;
            pp.ReadAloud = _fReadAloud;
            pp.Links.AddRange(_fLinks);
            pp.Subplot = _fSubplot.Copy();
            pp.Element = _fElement?.Copy();
            pp.Date = _fDate?.Copy();
            pp.RegionalMapId = _fRegionalMapId;
            pp.MapLocationId = _fMapLocationId;
            pp.AdditionalXp = _fAdditionalXp;

            foreach (var parcel in _fParcels)
                pp.Parcels.Add(parcel.Copy());

            foreach (var entryId in _fEncyclopediaEntries)
                pp.EncyclopediaEntryIDs.Add(entryId);

            return pp;
        }
    }
}
