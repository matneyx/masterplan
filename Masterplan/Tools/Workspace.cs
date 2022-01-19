using System.Collections.Generic;
using Masterplan.Data;

namespace Masterplan.Tools
{
    internal class Workspace
    {
        public static List<List<PlotPoint>> FindLayers(Plot plot)
        {
            var layers = new List<List<PlotPoint>>();

            var unused = new List<PlotPoint>(plot.Points);

            while (unused.Count > 0)
            {
                var layer = new List<PlotPoint>();

                // Find all unused points which are not linked to by unused points
                foreach (var pp in unused)
                {
                    var topLevel = true;

                    foreach (var point in unused)
                    {
                        if (point == pp)
                            continue;

                        if (point.Links.Contains(pp.Id))
                        {
                            topLevel = false;
                            break;
                        }
                    }

                    if (topLevel)
                        layer.Add(pp);
                }

                if (layer.Count == 0)
                    // There's been a problem; just add all unused points
                    layer.AddRange(unused);

                layers.Add(layer);

                foreach (var pp in layer)
                    unused.Remove(pp);
            }

            return layers;
        }

        public static int GetTotalXp(PlotPoint pp)
        {
            var xp = Session.Project.Party.Xp * Session.Project.Party.Size;

            while (true)
            {
                // Add the XP value for all previous plot points in this plot

                var plot = Session.Project.FindParent(pp);
                if (plot == null)
                    break;

                var layers = FindLayers(plot);
                foreach (var layer in layers)
                {
                    var inLayer = false;
                    foreach (var point in layer)
                        if (point.Id == pp.Id)
                        {
                            inLayer = true;
                            break;
                        }

                    if (inLayer)
                        break;

                    var layerXp = GetLayerXp(layer);
                    xp += layerXp;
                }

                pp = Session.Project.FindParent(plot);
                if (pp == null)
                    break;
            }

            return xp;
        }

        public static int GetLayerXp(List<PlotPoint> layer)
        {
            var gainedXp = 0;
            var totalXp = 0;
            var points = 0;

            foreach (var pp in layer)
            {
                if (pp == null)
                    continue;

                switch (pp.State)
                {
                    case PlotPointState.Normal:
                        totalXp += pp.GetXp();
                        points += 1;
                        break;
                    case PlotPointState.Skipped:
                        // Do nothing
                        break;
                    case PlotPointState.Completed:
                        gainedXp += pp.GetXp();
                        break;
                }
            }

            var predictedXp = totalXp;
            if (!Session.Preferences.AllXp) predictedXp = points != 0 ? totalXp / points : 0;

            return gainedXp + predictedXp;
        }

        public static int GetPartyLevel(PlotPoint pp)
        {
            var totalXp = GetTotalXp(pp);
            var xpPerPlayer = totalXp / Session.Project.Party.Size;

            return Experience.GetHeroLevel(xpPerPlayer);
        }
    }
}
