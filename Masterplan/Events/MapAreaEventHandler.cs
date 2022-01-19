using System;
using Masterplan.Data;

namespace Masterplan.Events
{
    /// <summary>
    ///     Event arguments including a MapArea object.
    /// </summary>
    public class MapAreaEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the tile.
        /// </summary>
        public MapArea MapArea { get; }

        /// <summary>
        ///     Constructor taking a MapArea object
        /// </summary>
        /// <param name="area">The map area.</param>
        public MapAreaEventArgs(MapArea area)
        {
            MapArea = area;
        }
    }

    /// <summary>
    ///     Delegate with a MapAreaEventArgs parameter.
    /// </summary>
    /// <param name="sender">The sender of the request.</param>
    /// <param name="e">The map area.</param>
    public delegate void MapAreaEventHandler(object sender, MapAreaEventArgs e);
}
