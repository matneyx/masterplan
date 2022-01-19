using System;
using System.Collections.Generic;
using System.Drawing;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a regional map.
    /// </summary>
    [Serializable]
    public class RegionalMap
    {
        private Guid _fId = Guid.NewGuid();

        private Image _fImage;

        private List<MapLocation> _fLocations = new List<MapLocation>();

        private string _fName = "";

        /// <summary>
        ///     Gets or sets the map name.
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
        ///     Gets or sets the map image.
        /// </summary>
        public Image Image
        {
            get => _fImage;
            set => _fImage = value;
        }

        /// <summary>
        ///     Gets or sets the list of map locations.
        /// </summary>
        public List<MapLocation> Locations
        {
            get => _fLocations;
            set => _fLocations = value;
        }

        /// <summary>
        /// </summary>
        /// <param name="location_id"></param>
        /// <returns></returns>
        public MapLocation FindLocation(Guid locationId)
        {
            foreach (var loc in _fLocations)
                if (loc.Id == locationId)
                    return loc;

            return null;
        }

        /// <summary>
        ///     Creates a copy of the map.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public RegionalMap Copy()
        {
            var rm = new RegionalMap();

            rm.Name = _fName;
            rm.Id = _fId;
            rm.Image = _fImage;

            foreach (var ml in _fLocations)
                rm.Locations.Add(ml.Copy());

            return rm;
        }

        /// <summary>
        ///     Returns the map name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _fName;
        }
    }

    /// <summary>
    ///     Class representing a location on a regional map.
    /// </summary>
    [Serializable]
    public class MapLocation
    {
        private string _fCategory = "";

        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        private PointF _fPoint = PointF.Empty;

        /// <summary>
        ///     Gets or sets the name of the location.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the location category.
        /// </summary>
        public string Category
        {
            get => _fCategory;
            set => _fCategory = value;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the location.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the location point.
        /// </summary>
        public PointF Point
        {
            get => _fPoint;
            set => _fPoint = value;
        }

        /// <summary>
        ///     Creates a copy of the map location.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public MapLocation Copy()
        {
            var ml = new MapLocation();

            ml.Name = _fName;
            ml.Category = _fCategory;
            ml.Id = _fId;
            ml.Point = new PointF(_fPoint.X, _fPoint.Y);

            return ml;
        }

        /// <summary>
        ///     Returns the location name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _fName;
        }
    }
}
