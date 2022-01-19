using System;
using System.Collections.Generic;

namespace Masterplan.Data
{
    /// <summary>
    ///     Enumeration for the types of cards used in an encounter deck.
    /// </summary>
    public enum CardCategory
    {
        /// <summary>
        ///     Artillery.
        /// </summary>
        Artillery,

        /// <summary>
        ///     Controller.
        /// </summary>
        Controller,

        /// <summary>
        ///     Lurker.
        /// </summary>
        Lurker,

        /// <summary>
        ///     Skirmisher.
        /// </summary>
        Skirmisher,

        /// <summary>
        ///     Soldier or brute.
        /// </summary>
        SoldierBrute,

        /// <summary>
        ///     Minion.
        /// </summary>
        Minion,

        /// <summary>
        ///     Solo.
        /// </summary>
        Solo
    }

    /// <summary>
    ///     Class representing an encounter deck.
    /// </summary>
    [Serializable]
    public class EncounterDeck
    {
        private List<EncounterCard> _cards = new List<EncounterCard>();

        private Guid _fId = Guid.NewGuid();

        private int _fLevel = 1;

        private string _fName = "";

        /// <summary>
        ///     Gets or sets the unique ID of the deck.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the deck.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the level of the deck.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets or sets the list of cards in the deck.
        /// </summary>
        public List<EncounterCard> Cards
        {
            get => _cards;
            set => _cards = value;
        }

        /// <summary>
        ///     Draws cards from the deck to create an encounter.
        /// </summary>
        /// <param name="enc">The encounter to add to.</param>
        /// <returns>Returns true if the process succeeded; false otherwise.</returns>
        public bool DrawEncounter(Encounter enc)
        {
            if (_cards.Count == 0)
                return false;

            var cards = new List<EncounterCard>();

            var availableCards = new List<EncounterCard>();
            foreach (var card in _cards)
                if (!card.Drawn)
                    availableCards.Add(card);

            var attempts = 0;
            while (true)
            {
                attempts += 1;

                var lurker = false;

                var handSize = Session.Project.Party.Size;
                while (cards.Count < handSize && availableCards.Count != 0)
                {
                    var index = Session.Random.Next() % availableCards.Count;
                    var card = availableCards[index];

                    cards.Add(card);
                    availableCards.Remove(card);

                    // If there's a lurker, draw an extra card
                    if (card.Category == CardCategory.Lurker && !lurker)
                    {
                        handSize += 1;
                        lurker = true;
                    }
                }

                var soldierCards = 0;
                foreach (var card in cards)
                    if (card.Category == CardCategory.SoldierBrute)
                        soldierCards += 1;

                if (soldierCards == 1 || attempts == 1000)
                    break;

                availableCards.AddRange(cards);
                cards.Clear();
            }

            // If this hand contains the solo creature, take that card and return the others
            foreach (var c in cards)
                if (c.Category == CardCategory.Solo)
                {
                    cards.Remove(c);

                    availableCards.AddRange(cards);
                    cards.Clear();

                    cards.Add(c);

                    break;
                }

            foreach (var card in cards)
                card.Drawn = true;

            enc.Slots.Clear();

            foreach (var card in cards)
            {
                // Do we already have a card of this type?
                EncounterSlot slot = null;
                foreach (var s in enc.Slots)
                    if (s.Card.CreatureId == card.CreatureId)
                    {
                        slot = s;
                        break;
                    }

                if (slot == null)
                {
                    slot = new EncounterSlot();
                    slot.Card = card;
                    enc.Slots.Add(slot);
                }

                var count = 1;
                switch (card.Category)
                {
                    case CardCategory.SoldierBrute:
                        count = 2;
                        break;
                    case CardCategory.Minion:
                        count += 4;
                        break;
                }

                for (var n = 0; n != count; ++n)
                {
                    var ccd = new CombatData();
                    slot.CombatData.Add(ccd);
                }
            }

            foreach (var slot in enc.Slots)
                slot.SetDefaultDisplayNames();

            return true;
        }

        /// <summary>
        ///     Draws cards from the deck to create a delve.
        /// </summary>
        /// <param name="pp">The plot point to use as the parent plot.</param>
        /// <param name="map">The map to create the delve for.</param>
        /// <returns>Returns true if the process succeeded; false otherwise.</returns>
        public bool DrawDelve(PlotPoint pp, Map map)
        {
            foreach (var area in map.Areas)
            {
                var enc = new Encounter();
                var ok = DrawEncounter(enc);
                if (!ok)
                    return false;

                var encPoint = new PlotPoint(area.Name);
                encPoint.Element = enc;

                pp.Subplot.Points.Add(encPoint);
            }

            return true;
        }

        /// <summary>
        ///     Calculates the number of cards of a given category in the deck.
        /// </summary>
        /// <param name="cat">The card category.</param>
        /// <returns>Returns the number of cards.</returns>
        public int Count(CardCategory cat)
        {
            var count = 0;

            foreach (var card in _cards)
                if (card.Category == cat)
                    count += 1;

            return count;
        }

        /// <summary>
        ///     Calculates the number of cards of a given level in the deck.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>Returns the number of cards.</returns>
        public int Count(int level)
        {
            var count = 0;

            foreach (var card in _cards)
                if (card.Level == level)
                    count += 1;

            return count;
        }

        /// <summary>
        ///     Creates a copy of the deck.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EncounterDeck Copy()
        {
            var deck = new EncounterDeck();

            deck.Id = _fId;
            deck.Name = _fName;
            deck.Level = _fLevel;

            foreach (var card in _cards)
                deck.Cards.Add(card.Copy());

            return deck;
        }

        /// <summary>
        ///     Returns the name of the deck.
        /// </summary>
        /// <returns>Returns the name of the deck.</returns>
        public override string ToString()
        {
            return _fName;
        }
    }
}
