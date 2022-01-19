using System;
using Masterplan.Data;

namespace Masterplan.Events
{
    /// <summary>
    ///     Event arguments including a MapSketch object.
    /// </summary>
    public class MapSketchEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the tile.
        /// </summary>
        public MapSketch Sketch { get; }

        /// <summary>
        ///     Constructor taking a MapSketch object
        /// </summary>
        /// <param name="sketch">The map sketch.</param>
        public MapSketchEventArgs(MapSketch sketch)
        {
            Sketch = sketch;
        }
    }

    /// <summary>
    ///     Delegate with a MapSketchEventArgs parameter.
    /// </summary>
    /// <param name="sender">The sender of the request.</param>
    /// <param name="e">The map sketch.</param>
    public delegate void MapSketchEventHandler(object sender, MapSketchEventArgs e);
}
