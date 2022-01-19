using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan
{
    /// <summary>
    ///     Class used to store user settings.
    /// </summary>
    [Serializable]
    public class Preferences
    {
        /// <summary>
        ///     Gets or sets user combat settings.
        /// </summary>
        public CombatPreferences Combat = new CombatPreferences();

        /// <summary>
        ///     Gets or sets user map builder settings.
        /// </summary>
        public MapBuilderPreferences MapBuilder = new MapBuilderPreferences();

        /// <summary>
        ///     Gets or sets user window settings.
        /// </summary>
        public WindowPreferences Window = new WindowPreferences();

        /// <summary>
        ///     Gets or sets user workspace settings.
        /// </summary>
        public WorkspacePreferences Workspace = new WorkspacePreferences();

        /// <summary>
        ///     Gets or sets the last file to be opened.
        /// </summary>
        public string LastFile { get; set; } = "";

        /// <summary>
        ///     Gets or sets the text size for the application.
        /// </summary>
        public DisplaySize TextSize { get; set; } = DisplaySize.Small;

        /// <summary>
        ///     Gets or sets the text size for the player view.
        /// </summary>
        public DisplaySize PlayerViewTextSize { get; set; } = DisplaySize.Small;

        /// <summary>
        ///     Gets or sets whether the XP calculation sums all plot points.
        /// </summary>
        public bool AllXp { get; set; } = true;

        /// <summary>
        ///     Gets or sets the list of tile libraries which are selected in the mapper.
        /// </summary>
        public List<Guid> TileLibraries { get; set; } = new List<Guid>();
    }
}
