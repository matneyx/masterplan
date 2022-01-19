using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Masterplan.Controls;
using Masterplan.Data;

namespace Masterplan.Tools
{
    internal class MapPrinting
    {
        private static Map _fMap;
        private static Rectangle _fViewpoint = Rectangle.Empty;
        private static Encounter _fEncounter;
        private static bool _fShowGridlines;
        private static bool _fPosterMode;

        private static List<Rectangle> _fPages;

        public static void Print(MapView mapview, bool poster, PrinterSettings settings)
        {
            _fMap = mapview.Map;
            _fViewpoint = mapview.Viewpoint;
            _fEncounter = mapview.Encounter;
            _fShowGridlines = mapview.ShowGrid == MapGridMode.Overlay;
            _fPosterMode = poster;

            var doc = new PrintDocument();
            doc.DocumentName = _fMap.Name;
            doc.PrinterSettings = settings;

            _fPages = null;

            doc.PrintPage += print_map_page;
            doc.Print();
        }

        private static void print_map_page(object sender, PrintPageEventArgs e)
        {
            var ctrl = new MapView();
            ctrl.Map = _fMap;
            ctrl.Viewpoint = _fViewpoint;
            ctrl.Encounter = _fEncounter;
            ctrl.LineOfSight = false;
            ctrl.Mode = MapViewMode.Plain;
            ctrl.Size = e.PageBounds.Size;
            ctrl.BorderSize = 1;

            if (_fShowGridlines)
                ctrl.ShowGrid = MapGridMode.Overlay;

            if (_fPages == null)
            {
                if (_fPosterMode)
                {
                    var squareCountH = e.PageSettings.PaperSize.Width / 100;
                    var squareCountV = e.PageSettings.PaperSize.Height / 100;

                    _fPages = get_pages(ctrl, squareCountH, squareCountV);
                }
                else
                {
                    _fPages = new List<Rectangle>();
                    _fPages.Add(ctrl.Viewpoint);
                }
            }

            ctrl.Viewpoint = _fPages[0];
            _fPages.RemoveAt(0);

            var mapWider = ctrl.LayoutData.Width > ctrl.LayoutData.Height;
            var pageWider = e.PageBounds.Width > e.PageBounds.Height;
            var rotate = mapWider != pageWider;

            if (rotate)
            {
                ctrl.Width = e.PageBounds.Height;
                ctrl.Height = e.PageBounds.Width;
            }

            var bmp = new Bitmap(ctrl.Width, ctrl.Height);
            ctrl.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

            if (rotate) bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

            e.Graphics.DrawImage(bmp, e.PageBounds);

            e.HasMorePages = _fPages.Count != 0;
        }

        private static List<Rectangle> get_pages(MapView ctrl, int squareCountH, int squareCountV)
        {
            var width = Math.Max(squareCountH, squareCountV);
            var height = Math.Min(squareCountH, squareCountV);

            var squares = new List<Point>();
            for (var x = ctrl.LayoutData.MinX; x <= ctrl.LayoutData.MaxX; ++x)
            for (var y = ctrl.LayoutData.MinY; y <= ctrl.LayoutData.MaxY; ++y)
            {
                var pt = new Point(x, y);
                var td = ctrl.LayoutData.GetTileAtSquare(pt);
                if (td != null)
                    squares.Add(pt);
            }

            var pages = new List<Rectangle>();

            for (var x = ctrl.LayoutData.MinX; x <= ctrl.LayoutData.MaxX; x += width)
            for (var y = ctrl.LayoutData.MinY; y <= ctrl.LayoutData.MaxY; y += height)
            {
                var rect = new Rectangle(x, y, width, height);

                var containsTile = false;
                foreach (var square in squares)
                    if (rect.Contains(square))
                    {
                        containsTile = true;
                        break;
                    }

                if (containsTile)
                    pages.Add(rect);
            }

            return pages;
        }
    }

    internal class BlankMap
    {
        public static void Print()
        {
            var dlg = new PrintDialog();
            dlg.AllowPrintToFile = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var doc = new PrintDocument();
                doc.DocumentName = "Blank Grid";
                doc.PrinterSettings = dlg.PrinterSettings;

                for (var page = 0; page != dlg.PrinterSettings.Copies; ++page)
                {
                    doc.PrintPage += print_blank_page;
                    doc.Print();
                }
            }
        }

        private static void print_blank_page(object sender, PrintPageEventArgs e)
        {
            var squareCountH = e.PageSettings.PaperSize.Width / 100;
            var squareCountV = e.PageSettings.PaperSize.Height / 100;

            var squareSizeH = e.PageBounds.Width / squareCountH;
            var squareSizeV = e.PageBounds.Height / squareCountV;
            var squareSize = Math.Min(squareSizeH, squareSizeV);

            var width = squareCountH * squareSize + 1;
            var height = squareCountV * squareSize + 1;

            var img = new Bitmap(width, height);

            for (var x = 0; x != width; ++x)
            for (var y = 0; y != height; ++y)
                if (x % squareSize == 0 || y % squareSize == 0)
                    img.SetPixel(x, y, Color.DarkGray);

            var xOffset = (e.PageBounds.Width - width) / 2;
            var yOffset = (e.PageBounds.Height - height) / 2;
            var rect = new Rectangle(xOffset, yOffset, width, height);

            e.Graphics.DrawRectangle(Pens.Black, rect);
            e.Graphics.DrawImage(img, rect);
        }
    }
}
