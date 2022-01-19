using System;
using Masterplan.Controls;

namespace Masterplan
{
    /// <summary>
    ///     Class used to store user workspace settings.
    /// </summary>
    [Serializable]
    public class WorkspacePreferences
    {
        /// <summary>
        ///     Gets or sets whether the plot navigation panel is shown.
        /// </summary>
        public bool ShowNavigation { get; set; }

        /// <summary>
        ///     Gets or sets whether the plot preview panel is shown.
        /// </summary>
        public bool ShowPreview { get; set; } = true;

        /// <summary>
        ///     Gets or sets how plot point links are drawn.
        /// </summary>
        public PlotViewLinkStyle LinkStyle { get; set; } = PlotViewLinkStyle.Curved;
    }
}
