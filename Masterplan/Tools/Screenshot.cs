using System;
using System.Collections.Generic;
using System.Drawing;
using Masterplan.Controls;
using Masterplan.Data;

namespace Masterplan.Tools
{
    internal class Screenshot
    {
        public static Bitmap Plot(Plot plot, Size size)
        {
            var ctrl = new PlotView();
            ctrl.Plot = plot;
            ctrl.Mode = PlotViewMode.Plain;
            ctrl.Size = size;

            var bmp = new Bitmap(ctrl.Width, ctrl.Height);
            ctrl.DrawToBitmap(bmp, ctrl.ClientRectangle);

            return bmp;
        }

        public static Bitmap Map(Map map, Rectangle view, Encounter enc, Dictionary<Guid, CombatData> heroes,
            List<TokenLink> tokens)
        {
            var mapview = new MapView();
            mapview.Map = map;
            mapview.Viewpoint = view;
            mapview.Mode = MapViewMode.Plain;
            mapview.LineOfSight = false;
            mapview.Encounter = enc;
            mapview.TokenLinks = tokens;

            return Map(mapview);
        }

        public static Bitmap Map(MapView mapview)
        {
            // Make it a decent size
            const int squareSize = 64;
            if (mapview.Viewpoint != Rectangle.Empty)
                mapview.Size = new Size(mapview.Viewpoint.Width * squareSize, mapview.Viewpoint.Height * squareSize);
            else
                mapview.Size = new Size(mapview.LayoutData.Width * squareSize, mapview.LayoutData.Height * squareSize);

            var bmp = new Bitmap(mapview.Width, mapview.Height);
            mapview.DrawToBitmap(bmp, mapview.ClientRectangle);

            return bmp;
        }

        public static Bitmap Calendar(Calendar calendar, int monthIndex, int year, Size size)
        {
            var ctrl = new CalendarPanel();
            ctrl.Calendar = calendar;
            ctrl.MonthIndex = monthIndex;
            ctrl.Year = year;
            ctrl.Size = size;

            var bmp = new Bitmap(ctrl.Width, ctrl.Height);
            ctrl.DrawToBitmap(bmp, ctrl.ClientRectangle);

            return bmp;
        }
    }
}
