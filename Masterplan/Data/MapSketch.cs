using System;
using System.Collections.Generic;
using System.Drawing;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a hand-drawn annotation on a map.
    /// </summary>
    [Serializable]
    public class MapSketch
    {
        private Color _fColour = Color.Black;

        private int _fWidth = 3;

        /// <summary>
        ///     Gets or sets the colour of the sketch.
        /// </summary>
        public Color Colour
        {
            get => _fColour;
            set => _fColour = value;
        }

        /// <summary>
        ///     Gets or sets the width of the sketch pen.
        /// </summary>
        public int Width
        {
            get => _fWidth;
            set => _fWidth = value;
        }

        /// <summary>
        ///     Gets the list of points in the sketch line.
        /// </summary>
        public List<MapSketchPoint> Points { get; } = new List<MapSketchPoint>();

        /// <summary>
        ///     Creates a copy of the sketch.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public MapSketch Copy()
        {
            var sketch = new MapSketch();

            sketch.Colour = _fColour;
            sketch.Width = _fWidth;

            foreach (var msp in Points)
                sketch.Points.Add(msp.Copy());

            return sketch;
        }
    }

    /// <summary>
    ///     Class representing a point in a map sketch.
    /// </summary>
    [Serializable]
    public class MapSketchPoint
    {
        private PointF _fLocation = new PointF(0, 0);

        private Point _fSquare = new Point(0, 0);

        /// <summary>
        ///     Gets or sets the map square containing the point.
        /// </summary>
        public Point Square
        {
            get => _fSquare;
            set => _fSquare = value;
        }

        /// <summary>
        ///     Gets or sets the location of the point in the square.
        /// </summary>
        public PointF Location
        {
            get => _fLocation;
            set => _fLocation = value;
        }

        /// <summary>
        ///     Creates a copy of the point.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public MapSketchPoint Copy()
        {
            var msp = new MapSketchPoint();

            msp.Square = new Point(_fSquare.X, _fSquare.Y);
            msp.Location = new PointF(_fLocation.X, _fLocation.Y);

            return msp;
        }
    }
}
