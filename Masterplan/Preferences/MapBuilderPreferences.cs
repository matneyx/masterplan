using System;
using Masterplan.UI;

namespace Masterplan
{
    /// <summary>
    ///     Class used to store user map builder settings.
    /// </summary>
    [Serializable]
    public class MapBuilderPreferences
    {
        /// <summary>
        ///     Gets or sets the default map builder tile view mode.
        /// </summary>
        public TileView TileView { get; set; } = TileView.Size;

        /// <summary>
        ///     Gets or sets the default map builder tile size.
        /// </summary>
        public TileSize TileSize { get; set; } = TileSize.Medium;
    }
}
