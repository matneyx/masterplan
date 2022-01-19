using System;
using System.Collections.Generic;
using System.Drawing;
using Masterplan.Data;

namespace Masterplan.Events
{
    /// <summary>
    ///     Event arguments containing a map token.
    /// </summary>
    public class TokenEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the map token.
        /// </summary>
        public IToken Token { get; }

        /// <summary>
        ///     Constructor taking an IToken.
        /// </summary>
        /// <param name="token">The map token.</param>
        public TokenEventArgs(IToken token)
        {
            Token = token;
        }
    }

    /// <summary>
    ///     Event arguments containing a list of map tokens.
    /// </summary>
    public class TokenListEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the list of map tokens.
        /// </summary>
        public List<IToken> Tokens { get; } = new List<IToken>();

        /// <summary>
        ///     Constructor taking a list of IToken objects.
        /// </summary>
        /// <param name="tokens">The map tokens.</param>
        public TokenListEventArgs(List<IToken> tokens)
        {
            Tokens = tokens;
        }
    }

    /// <summary>
    ///     Event arguments containing a DraggedToken object.
    /// </summary>
    public class DraggedTokenEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the map token.
        /// </summary>
        public Point OldLocation { get; } = CombatData.NoPoint;

        /// <summary>
        ///     Gets the map token.
        /// </summary>
        public Point NewLocation { get; } = CombatData.NoPoint;

        /// <summary>
        ///     Constructor.
        /// </summary>
        // /// <param name="token">The map token.</param>
        public DraggedTokenEventArgs(Point oldLocation, Point newLocation)
        {
            OldLocation = oldLocation;
            NewLocation = newLocation;
        }
    }

    /// <summary>
    ///     Event arguments containing a token link.
    /// </summary>
    public class TokenLinkEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the token link.
        /// </summary>
        public TokenLink Link { get; }

        /// <summary>
        ///     Constructor taking a TokenLink.
        /// </summary>
        /// <param name="link">The token link.</param>
        public TokenLinkEventArgs(TokenLink link)
        {
            Link = link;
        }
    }

    /// <summary>
    ///     Delegate with a TokenEventArgs parameter.
    /// </summary>
    /// <param name="sender">The sender of the request.</param>
    /// <param name="e">The token.</param>
    public delegate void TokenEventHandler(object sender, TokenEventArgs e);

    /// <summary>
    ///     Delegate used when creating a TokenLink object.
    /// </summary>
    /// <param name="sender">The sender of the request.</param>
    /// <param name="e">The list of tokens for the link.</param>
    /// <returns>Returns the new link, if one was created; false otherwise.</returns>
    public delegate TokenLink CreateTokenLinkEventHandler(object sender, TokenListEventArgs e);

    /// <summary>
    ///     Delegate with a DraggedTokenEventArgs parameter.
    /// </summary>
    /// <param name="sender">The sender of the request.</param>
    /// <param name="e">The DraggedToken object.</param>
    public delegate void DraggedTokenEventHandler(object sender, DraggedTokenEventArgs e);

    /// <summary>
    ///     Delegate with a TokenLinkEventArgs parameter.
    /// </summary>
    /// <param name="sender">The sender of the request.</param>
    /// <param name="e">The TokenLink object.</param>
    public delegate TokenLink TokenLinkEventHandler(object sender, TokenLinkEventArgs e);
}
