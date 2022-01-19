﻿using System;
using Masterplan.Data;

namespace Masterplan.Events
{
    /// <summary>
    ///     Event arguments containing a creature token.
    /// </summary>
    public class CreatureEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the creature token.
        /// </summary>
        public CreatureToken Token { get; }

        /// <summary>
        ///     Constructor taking a CreatureToken object.
        /// </summary>
        /// <param name="token">The creature token.</param>
        public CreatureEventArgs(CreatureToken token)
        {
            Token = token;
        }
    }

    /// <summary>
    ///     Delegate taking a CreatureToken as a parameter.
    /// </summary>
    /// <param name="sender">The sender of the request.</param>
    /// <param name="e">The creature token.</param>
    public delegate void CreatureEventHandler(object sender, CreatureEventArgs e);
}
