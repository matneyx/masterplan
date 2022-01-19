using System;
using System.Drawing;

namespace Masterplan
{
    /// <summary>
    ///     Class used to store user window settings.
    /// </summary>
    [Serializable]
    public class WindowPreferences
    {
        /// <summary>
        ///     Gets or sets whether the application is maximised.
        /// </summary>
        public bool Maximised { get; set; }

        /// <summary>
        ///     Gets or sets the size of the application main form.
        /// </summary>
        public Size Size { get; set; } = Size.Empty;

        /// <summary>
        ///     Gets or sets the location of the application main form.
        /// </summary>
        public Point Location { get; set; } = Point.Empty;
    }
}
