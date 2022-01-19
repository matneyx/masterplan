using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a plot structure.
    /// </summary>
    [Serializable]
    public class Plot
    {
        private List<PlotPoint> _fPoints = new List<PlotPoint>();

        /// <summary>
        ///     Gets or sets the list of plot points in the plot.
        /// </summary>
        public List<PlotPoint> Points
        {
            get => _fPoints;
            set => _fPoints = value;
        }

        /// <summary>
        ///     Returns a list containing all the plot points in this plot and its subplots.
        /// </summary>
        public List<PlotPoint> AllPlotPoints
        {
            get
            {
                var points = new List<PlotPoint>();

                foreach (var pp in _fPoints)
                {
                    points.Add(pp);
                    points.AddRange(pp.Subplot.AllPlotPoints);
                }

                return points;
            }
        }

        /// <summary>
        ///     Finds the plot point with the given ID.
        /// </summary>
        /// <param name="id">The ID of the desired plot point.</param>
        /// <returns>Returns the plot point.</returns>
        public PlotPoint FindPoint(Guid id)
        {
            foreach (var pp in _fPoints)
                if (pp.Id == id)
                    return pp;

            return null;
        }

        /// <summary>
        ///     Removes the specified plot point, and all references to it.
        /// </summary>
        /// <param name="point">The plot point.</param>
        public void RemovePoint(PlotPoint point)
        {
            new List<Guid>();
            foreach (var pp in _fPoints)
                if (pp.Links.Contains(point.Id))
                {
                    // Remove the reference to this point
                    while (pp.Links.Contains(point.Id))
                        pp.Links.Remove(point.Id);

                    // Link this to all points on the other side
                    foreach (var pointId in point.Links)
                    {
                        if (pp.Links.Contains(pointId))
                            continue;

                        pp.Links.Add(pointId);
                    }
                }

            _fPoints.Remove(point);
        }

        /// <summary>
        ///     Find all points in this plot which lead to the point with the specified ID.
        /// </summary>
        /// <param name="point_id">The ID of the plot point.</param>
        /// <returns>Returns the list of points.</returns>
        public List<PlotPoint> FindPrerequisites(Guid pointId)
        {
            var points = new List<PlotPoint>();

            foreach (var pp in _fPoints)
                if (pp.Links.Contains(pointId))
                    points.Add(pp);

            return points;
        }

        /// <summary>
        ///     Find all points in this plot which lead from the point with the specified ID.
        /// </summary>
        /// <param name="pp">The ID of the plot point.</param>
        /// <returns>Returns the list of points.</returns>
        public List<PlotPoint> FindSubtree(PlotPoint pp)
        {
            var subtree = new List<PlotPoint>();
            subtree.Add(pp);

            foreach (var id in pp.Links)
            {
                var child = FindPoint(id);
                var branch = FindSubtree(child);

                subtree.AddRange(branch);
            }

            return subtree;
        }

        /// <summary>
        ///     Returns the plot point which is associated with the specified map and map area.
        ///     Does not recurse into subplots.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="area">The map area.</param>
        /// <returns>The plot point, if one exists; false otherwise.</returns>
        public PlotPoint FindPointForMapArea(Map map, MapArea area)
        {
            foreach (var point in _fPoints)
            {
                Map m = null;
                MapArea ma = null;
                point.GetTacticalMapArea(ref m, ref ma);
                if (m == map && ma == area)
                    return point;
            }

            return null;
        }

        /// <summary>
        ///     Finds the list of tactical maps which are used in this plot.
        /// </summary>
        /// <returns>Returns the list of IDs of maps.</returns>
        public List<Guid> FindTacticalMaps()
        {
            var bst = new BinarySearchTree<Guid>();

            foreach (var pp in _fPoints)
                if (pp.Element != null)
                {
                    if (pp.Element is Encounter)
                    {
                        var enc = pp.Element as Encounter;

                        if (enc.MapId != Guid.Empty && enc.MapAreaId != Guid.Empty)
                            bst.Add(enc.MapId);
                    }

                    if (pp.Element is TrapElement)
                    {
                        var te = pp.Element as TrapElement;

                        if (te.MapId != Guid.Empty && te.MapAreaId != Guid.Empty)
                            bst.Add(te.MapId);
                    }

                    if (pp.Element is SkillChallenge)
                    {
                        var sc = pp.Element as SkillChallenge;

                        if (sc.MapId != Guid.Empty && sc.MapAreaId != Guid.Empty)
                            bst.Add(sc.MapId);
                    }

                    if (pp.Element is MapElement)
                    {
                        var me = pp.Element as MapElement;

                        if (me.MapId != Guid.Empty)
                            bst.Add(me.MapId);
                    }
                }

            var list = bst.SortedList;
            list.Remove(Guid.Empty);

            return list;
        }

        /// <summary>
        ///     Finds the list of regional maps which are used in this plot.
        /// </summary>
        /// <returns>Returns the list of IDs of maps.</returns>
        public List<Guid> FindRegionalMaps()
        {
            var bst = new BinarySearchTree<Guid>();

            foreach (var pp in _fPoints)
                if (pp.RegionalMapId != Guid.Empty && pp.MapLocationId != Guid.Empty)
                    bst.Add(pp.RegionalMapId);

            var list = bst.SortedList;
            list.Remove(Guid.Empty);

            return list;
        }

        /// <summary>
        ///     Creates a copy of the plot.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Plot Copy()
        {
            var p = new Plot();

            foreach (var pp in _fPoints)
                p.Points.Add(pp.Copy());

            return p;
        }
    }
}
