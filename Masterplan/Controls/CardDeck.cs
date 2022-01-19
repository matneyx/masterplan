using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.Controls
{
    internal partial class CardDeck : UserControl
    {
        private const float _radius = 10;

        private readonly StringFormat _centered = new StringFormat();
        private readonly StringFormat _info = new StringFormat();
        private readonly StringFormat _title = new StringFormat();

        private List<Pair<EncounterCard, int>> _cards;

        private EncounterCard _hoverCard;

        private List<Pair<RectangleF, EncounterCard>> _regions;
        private int _visibleCards;

        public EncounterCard TopCard
        {
            get
            {
                if (_cards == null || _cards.Count == 0)
                    return null;

                return _cards[0].First;
            }
        }

        public CardDeck()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint, true);

            _centered.Alignment = StringAlignment.Center;
            _centered.LineAlignment = StringAlignment.Center;
            _centered.Trimming = StringTrimming.EllipsisWord;

            _title.Alignment = StringAlignment.Near;
            _title.LineAlignment = StringAlignment.Center;
            _title.Trimming = StringTrimming.Character;

            _info.Alignment = StringAlignment.Far;
            _info.LineAlignment = StringAlignment.Center;
            _info.Trimming = StringTrimming.Character;
        }

        public void SetCards(List<EncounterCard> cards)
        {
            _cards = new List<Pair<EncounterCard, int>>();

            var titles = new BinarySearchTree<string>();
            foreach (var card in cards)
                titles.Add(card.Title);

            var titleList = titles.SortedList;
            foreach (var title in titleList)
            {
                var pair = new Pair<EncounterCard, int>();

                foreach (var card in cards)
                    if (card.Title == title)
                    {
                        pair.First = card;
                        pair.Second += 1;
                    }

                _cards.Add(pair);
            }

            Invalidate();
        }

        public event EventHandler DeckOrderChanged;

        private void OnDeckOrderChanged()
        {
            DeckOrderChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Transparent, ClientRectangle);

            if (_cards == null || _cards.Count == 0)
            {
                e.Graphics.DrawString("(no cards)", Font, Brushes.Black, ClientRectangle, _centered);
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var deckRect = new RectangleF(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 1,
                ClientRectangle.Height - 1);

            _regions = new List<Pair<RectangleF, EncounterCard>>();

            var cardDeltaY = Font.Height * 1.8f;
            var cardDeltaX = cardDeltaY * 0.2f;

            var usableHeight = Height - 4 * _radius;
            var maxCards = (int)(usableHeight / cardDeltaY);
            _visibleCards = Math.Min(maxCards, _cards.Count);
            if (_visibleCards + 1 == _cards.Count)
                _visibleCards += 1;

            var moreCards = _cards.Count > _visibleCards;
            var edges = moreCards ? _visibleCards + 1 : _visibleCards;

            if (moreCards)
            {
                var dx = cardDeltaX * _visibleCards;

                var x = deckRect.X + dx;
                var y = deckRect.Y;
                var width = deckRect.Width - cardDeltaX * (edges - 1);
                var height = deckRect.Height - y;

                var cardRect = new RectangleF(x, y, width, height);

                draw_card(null, 0, false, cardRect, e.Graphics);
            }

            for (var index = _visibleCards - 1; index >= 0; --index)
            {
                var dx = cardDeltaX * index;
                var dy = cardDeltaY * (edges - index - 1);

                var x = deckRect.X + dx;
                var y = deckRect.Y + dy;
                var width = deckRect.Width - cardDeltaX * (edges - 1);
                var height = deckRect.Height - y;

                var cardRect = new RectangleF(x, y, width, height);

                var cardInfo = _cards[index];
                var topmost = index == 0;
                draw_card(cardInfo.First, cardInfo.Second, topmost, cardRect, e.Graphics);

                _regions.Add(new Pair<RectangleF, EncounterCard>(cardRect, cardInfo.First));
            }
        }

        private void draw_card(EncounterCard card, int count, bool topmost, RectangleF rect, Graphics g)
        {
            var alpha = card != null ? 255 : 100;

            var gp = RoundedRectangle.Create(rect, _radius,
                RoundedRectangle.RectangleCorners.TopLeft | RoundedRectangle.RectangleCorners.TopRight);
            using (Brush b = new SolidBrush(Color.FromArgb(alpha, 54, 79, 39)))
            {
                g.FillPath(b, gp);
            }

            g.DrawPath(Pens.White, gp);

            var cardDeltaY = Font.Height * 1.5f;
            var textRect = new RectangleF(rect.X + _radius, rect.Y, rect.Width - 2 * _radius, cardDeltaY);

            if (card != null)
            {
                var title = card.Title;
                if (count > 1)
                    title = "(" + count + "x) " + title;

                var textColour = card != _hoverCard ? Color.White : Color.PaleGreen;
                using (Brush b = new SolidBrush(textColour))
                {
                    using (var f = new Font(Font, Font.Style | FontStyle.Bold))
                    {
                        g.DrawString(title, f, b, textRect, _title);
                    }

                    g.DrawString(card.Info, Font, b, textRect, _info);
                }

                if (topmost)
                {
                    var dx = _radius * 0.2f;
                    var content = new RectangleF(rect.X + dx, rect.Y + textRect.Height, rect.Width - 2 * dx,
                        rect.Height - textRect.Height);
                    using (Brush b = new SolidBrush(Color.FromArgb(225, 231, 197)))
                    {
                        g.FillRectangle(b, content);
                    }

                    var msg = "Click on a card to move it to the front of the deck.";
                    g.DrawString(msg, Font, Brushes.Black, content, _centered);
                }
            }
            else
            {
                var remaining = _cards.Count - _visibleCards;
                g.DrawString("(" + remaining + " more cards)", Font, Brushes.White, textRect, _centered);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_regions == null)
                return;

            EncounterCard card = null;

            foreach (var pair in _regions)
                if (pair.First.Top <= e.Location.Y && pair.First.Bottom >= e.Location.Y)
                    card = pair.Second;

            _hoverCard = card;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hoverCard = null;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_hoverCard == null)
                return;

            var card = _hoverCard;
            _hoverCard = null;

            while (_cards[0].First != card)
            {
                var topCard = _cards[0];

                _cards.RemoveAt(0);
                _cards.Add(topCard);

                Refresh();
            }

            OnDeckOrderChanged();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                var topCard = _cards[0];

                _cards.RemoveAt(0);
                _cards.Add(topCard);
            }
            else
            {
                var lastCard = _cards[_cards.Count - 1];

                _cards.RemoveAt(_cards.Count - 1);
                _cards.Insert(0, lastCard);
            }

            _hoverCard = null;
            Refresh();

            OnDeckOrderChanged();
        }
    }
}
