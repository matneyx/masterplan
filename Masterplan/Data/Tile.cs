using System;
using System.Drawing;

namespace Masterplan.Data
{
    /// <summary>
    ///     Type of tile.
    /// </summary>
    public enum TileCategory
    {
        /// <summary>
        ///     Open space.
        /// </summary>
        Plain,

        /// <summary>
        ///     A door or curtain etc.
        /// </summary>
        Doorway,

        /// <summary>
        ///     A stairway or other way of travelling between levels.
        /// </summary>
        Stairway,

        /// <summary>
        ///     Decoration, furniture etc.
        /// </summary>
        Feature,

        /// <summary>
        ///     Miscellaneous tiles.
        /// </summary>
        Special,

        /// <summary>
        ///     Tiles which are full maps.
        /// </summary>
        Map
    }

    /// <summary>
    ///     Class representing a map tile.
    /// </summary>
    [Serializable]
    public class Tile
    {
        private Color _fBlankColour = Color.White;

        private TileCategory _fCategory = TileCategory.Special;

        private Guid _fId = Guid.NewGuid();

        private Image _fImage;

        private string _fKeywords = "";

        private Size _fSize = new Size(2, 2);

        /// <summary>
        ///     Gets or sets the unique ID of the tile.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the tile category.
        /// </summary>
        public TileCategory Category
        {
            get => _fCategory;
            set => _fCategory = value;
        }

        /// <summary>
        ///     Gets or sets the dimensions of the tile, in squares.
        /// </summary>
        public Size Size
        {
            get => _fSize;
            set => _fSize = value;
        }

        /// <summary>
        ///     Gets or sets the tile image.
        /// </summary>
        public Image Image
        {
            get => _fImage;
            set => _fImage = value;
        }

        /// <summary>
        ///     Gets or sets the colour of tiles with no Image.
        /// </summary>
        public Color BlankColour
        {
            get => _fBlankColour;
            set => _fBlankColour = value;
        }

        /// <summary>
        ///     Gets or sets the tile's keywords.
        /// </summary>
        public string Keywords
        {
            get => _fKeywords;
            set => _fKeywords = value;
        }

        /// <summary>
        ///     Gets the area of the tile, in squares.
        /// </summary>
        public int Area => _fSize.Width * _fSize.Height;

        /// <summary>
        ///     Gets a plain image for this tile.
        /// </summary>
        public Image BlankImage
        {
            get
            {
                var squareSize = 32;

                var width = _fSize.Width * squareSize + 1;
                var height = _fSize.Height * squareSize + 1;

                var img = new Bitmap(width, height);

                for (var x = 0; x != width; ++x)
                for (var y = 0; y != height; ++y)
                {
                    var c = _fBlankColour;
                    if (x % squareSize == 0 || y % squareSize == 0)
                        c = Color.DarkGray;

                    img.SetPixel(x, y, c);
                }

                return img;
            }
        }

        /// <summary>
        ///     [width] x [height]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _fSize.Width + " x " + _fSize.Height;
        }

        /// <summary>
        ///     Creates a copy of the tile.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Tile Copy()
        {
            var tile = new Tile();

            tile.Id = _fId;
            tile.Category = _fCategory;
            tile.Size = new Size(_fSize.Width, _fSize.Height);
            tile.Image = _fImage;
            tile.Keywords = _fKeywords;

            return tile;
        }
    }
}
