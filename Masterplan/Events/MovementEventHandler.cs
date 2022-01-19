using System;

namespace Masterplan.Events
{
    /// <summary>
    ///     Event arguments concerning map movement.
    /// </summary>
    public class MovementEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the distance moved.
        /// </summary>
        public int Distance { get; }

        /// <summary>
        ///     Constructor taking a distance parameter.
        /// </summary>
        /// <param name="distance">The distance moved.</param>
        public MovementEventArgs(int distance)
        {
            Distance = distance;
        }
    }

    /// <summary>
    ///     Delegate taking a MovementEventArgs as a parameter.
    /// </summary>
    /// <param name="sender">The sender of the request.</param>
    /// <param name="e">The movement data.</param>
    public delegate void MovementEventHandler(object sender, MovementEventArgs e);
}
