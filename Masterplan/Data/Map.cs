using System;
using System.Collections.Generic;
using System.Drawing;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a tactical map.
    /// </summary>
    [Serializable]
    public class Map
    {
        private List<MapArea> _fAreas = new List<MapArea>();

        private string _fCategory = "";

        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        private List<TileData> _fTiles = new List<TileData>();

        /// <summary>
        ///     Gets or sets the name of the map.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the map.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the category of the map.
        /// </summary>
        public string Category
        {
            get => _fCategory;
            set => _fCategory = value;
        }

        /// <summary>
        ///     Gets or sets the list of tiles in the map.
        /// </summary>
        public List<TileData> Tiles
        {
            get => _fTiles;
            set => _fTiles = value;
        }

        /// <summary>
        ///     Gets or sets the list of map areas.
        /// </summary>
        public List<MapArea> Areas
        {
            get => _fAreas;
            set => _fAreas = value;
        }

        /// <summary>
        ///     Finds the map area with the given ID.
        /// </summary>
        /// <param name="area_id">The ID to search for.</param>
        /// <returns>Returns the map area, if it exists; null otherwise.</returns>
        public MapArea FindArea(Guid areaId)
        {
            foreach (var area in _fAreas)
                if (area.Id == areaId)
                    return area;

            return null;
        }

        /// <summary>
        ///     Returns the name of the map.
        /// </summary>
        /// <returns>Returns the name of the map.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Creates a copy of the map.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Map Copy()
        {
            var m = new Map();

            m.Name = _fName;
            m.Id = _fId;
            m.Category = _fCategory;

            foreach (var td in _fTiles)
                m.Tiles.Add(td.Copy());

            foreach (var area in _fAreas)
                m.Areas.Add(area.Copy());

            return m;
        }
    }

    /// <summary>
    ///     Class representing a tile on a map.
    /// </summary>
    [Serializable]
    public class TileData
    {
        private Guid _fId = Guid.NewGuid();

        private Point _fLocation = new Point(0, 0);

        private int _fRotations;

        private Guid _fTileId = Guid.Empty;

        /// <summary>
        ///     Gets or sets the unique ID of this TileData.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the tile to be used.
        /// </summary>
        public Guid TileId
        {
            get => _fTileId;
            set => _fTileId = value;
        }

        /// <summary>
        ///     Gets or sets the location of the top-left square of the tile.
        /// </summary>
        public Point Location
        {
            get => _fLocation;
            set => _fLocation = value;
        }

        /// <summary>
        ///     Gets or sets the number of 90-degree turns the tile has turned.
        /// </summary>
        public int Rotations
        {
            get => _fRotations;
            set
            {
                _fRotations = value;

                while (_fRotations < 0)
                    _fRotations += 4;

                _fRotations = _fRotations % 4;
            }
        }

        /// <summary>
        ///     Creates a copy of the TileData.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public TileData Copy()
        {
            var td = new TileData();

            td.Id = _fId;
            td.TileId = _fTileId;
            td.Location = new Point(_fLocation.X, _fLocation.Y);
            td.Rotations = _fRotations;

            return td;
        }
    }

    /// <summary>
    ///     Class representing an area of a map.
    /// </summary>
    [Serializable]
    public class MapArea
    {
        private string _fDetails = "";

        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        private Rectangle _fRegion = new Rectangle(0, 0, 1, 1);

        /// <summary>
        ///     Gets or sets the area name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the area.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the area details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the area bounds.
        /// </summary>
        public Rectangle Region
        {
            get => _fRegion;
            set => _fRegion = value;
        }

        /// <summary>
        ///     Returns the area name.
        /// </summary>
        /// <returns>Returns the area name.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Creates a copy of the area.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public MapArea Copy()
        {
            var area = new MapArea();

            area.Name = _fName;
            area.Id = _fId;
            area.Details = _fDetails;
            area.Region = new Rectangle(_fRegion.X, _fRegion.Y, _fRegion.Width, _fRegion.Height);

            return area;
        }
    }

    /// <summary>
    ///     Wrapper class for adding a map to a plot point.
    /// </summary>
    [Serializable]
    public class MapElement : IElement
    {
        private Guid _fMapAreaId = Guid.Empty;

        private Guid _fMapId = Guid.Empty;

        /// <summary>
        ///     Gets or sets the ID of the map.
        /// </summary>
        public Guid MapId
        {
            get => _fMapId;
            set => _fMapId = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the map area.
        ///     Guid.Empty for the whole map.
        /// </summary>
        public Guid MapAreaId
        {
            get => _fMapAreaId;
            set => _fMapAreaId = value;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public MapElement()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="map_id">The ID of the map.</param>
        /// <param name="map_area_id">The ID of the map area; Guid.Empty for the whole map.</param>
        public MapElement(Guid mapId, Guid mapAreaId)
        {
            _fMapId = mapId;
            _fMapAreaId = mapAreaId;
        }

        /// <summary>
        ///     Not used; always returns 0.
        /// </summary>
        /// <returns>Always returns 0.</returns>
        public int GetXp()
        {
            return 0;
        }

        /// <summary>
        ///     Not used; always returns Difficulty.Moderate.
        /// </summary>
        /// <param name="party_level">The party level.</param>
        /// <param name="party_size">The party size.</param>
        /// <returns>Always returns Difficulty.Moderate.</returns>
        public Difficulty GetDifficulty(int partyLevel, int partySize)
        {
            return Difficulty.Moderate;
        }

        /// <summary>
        ///     Creates a copy of the MapElement.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public IElement Copy()
        {
            var me = new MapElement();

            me.MapId = _fMapId;
            me.MapAreaId = _fMapAreaId;

            return me;
        }
    }
}
