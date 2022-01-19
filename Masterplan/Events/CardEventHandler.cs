﻿using System;
using System.Collections.Generic;
using Masterplan.Data;

namespace Masterplan.Events
{
    /// <summary>
    ///     Event arguments containing an encounter card.
    /// </summary>
    public class CardEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the encounter card.
        /// </summary>
        public EncounterCard Card { get; }

        /// <summary>
        ///     Constructor taking an EncounterCard object.
        /// </summary>
        /// <param name="card">The card.</param>
        public CardEventArgs(EncounterCard card)
        {
            Card = card;
        }
    }

    /// <summary>
    ///     Event arguments containing a list of encounter cards.
    /// </summary>
    public class CardListEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the list of encounter cards.
        /// </summary>
        public List<EncounterCard> Cards { get; } = new List<EncounterCard>();

        /// <summary>
        ///     Constructor taking a list of EncounterCard objects.
        /// </summary>
        /// <param name="cards">The encounter cards.</param>
        public CardListEventArgs(List<EncounterCard> cards)
        {
            Cards = cards;
        }
    }

    /// <summary>
    ///     Delegate with a CardEventArgs parameter.
    /// </summary>
    /// <param name="sender">The sender of the request.</param>
    /// <param name="e">The card.</param>
    public delegate void CardEventHandler(object sender, CardEventArgs e);

    /// <summary>
    ///     Delegate with a CardListEventArgs parameter.
    /// </summary>
    /// <param name="sender">The sender of the request.</param>
    /// <param name="e">The list of cards.</param>
    public delegate void CardListEventHandler(object sender, CardListEventArgs e);
}
