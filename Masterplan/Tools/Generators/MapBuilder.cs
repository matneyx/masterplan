using System;
using System.Collections.Generic;
using System.Drawing;
using Masterplan.Data;

namespace Masterplan.Tools.Generators
{
    internal enum MapAutoBuildType
    {
        Warren,
        FilledArea,
        Freeform
    }

    internal enum Orientation
    {
        Any,
        NorthSouth,
        EastWest
    }

    internal enum Direction
    {
        North,
        East,
        South,
        West
    }

    internal class MapBuilderData
    {
        public bool DelveOnly = false;
        public int Height = 15;

        public List<Library> Libraries = new List<Library>();

        public int MaxAreaCount = 10;
        public int MinAreaCount = 4;
        public MapAutoBuildType Type = MapAutoBuildType.Warren;

        public int Width = 20;
    }

    internal class Endpoint
    {
        public Point BottomRight = Point.Empty;
        public TileCategory Category = TileCategory.Plain;
        public Direction Direction = Direction.North;
        public Point TopLeft = Point.Empty;

        public int Size
        {
            get
            {
                var dx = BottomRight.X - TopLeft.X;
                var dy = BottomRight.Y - TopLeft.Y;
                return Math.Max(dx, dy);
            }
        }

        public Orientation Orientation
        {
            get
            {
                if (TopLeft.X == BottomRight.X)
                    return Orientation.NorthSouth;

                return Orientation.EastWest;
            }
        }
    }

    internal class MapBuilder
    {
        private static MapBuilderData _fData;
        private static Map _fMap;

        private static readonly Dictionary<TileCategory, List<Tile>>
            FTiles = new Dictionary<TileCategory, List<Tile>>();

        private static readonly List<Tile> FRoomTiles = new List<Tile>();
        private static readonly List<Tile> FCorridorTiles = new List<Tile>();
        private static readonly List<Endpoint> FEndpoints = new List<Endpoint>();

        public static void BuildMap(MapBuilderData data, Map map, EventHandler callback)
        {
            _fData = data;
            _fMap = map;

            _fMap.Tiles.Clear();
            _fMap.Areas.Clear();

            switch (_fData.Type)
            {
                case MapAutoBuildType.Warren:
                {
                    FEndpoints.Clear();
                    build_tile_lists();

                    build_warren(callback);
                }
                    break;
                case MapAutoBuildType.FilledArea:
                {
                    build_filled_area(callback);
                }
                    break;
                case MapAutoBuildType.Freeform:
                {
                    build_freeform_area(callback);
                }
                    break;
            }
        }

        private static void build_tile_lists()
        {
            FTiles.Clear();

            foreach (TileCategory cat in Enum.GetValues(typeof(TileCategory)))
                FTiles[cat] = new List<Tile>();

            foreach (var lib in _fData.Libraries)
            foreach (var t in lib.Tiles)
                FTiles[t.Category].Add(t);

            FRoomTiles.Clear();
            FCorridorTiles.Clear();
            foreach (var t in FTiles[TileCategory.Plain])
            {
                var size = Math.Min(t.Size.Width, t.Size.Height);

                if (size == 2)
                    FCorridorTiles.Add(t);

                if (size > 2)
                    FRoomTiles.Add(t);
            }
        }

        private static void build_warren(EventHandler callback)
        {
            begin_map();

            var failures = 0;
            while (_fMap.Areas.Count < _fData.MaxAreaCount)
            {
                if (FEndpoints.Count == 0)
                    break;

                if (failures == 100)
                    break;

                // Take a random corridor endpoint from the list
                var index = Session.Random.Next() % FEndpoints.Count;
                var ep = FEndpoints[index];

                // Add an area, corridor or stairway
                var ok = true;
                switch (Session.Random.Next() % 10)
                {
                    case 0:
                    case 1:
                    case 2:
                        try
                        {
                            // Add an area
                            ok = add_area(ep);
                        }
                        catch (Exception ex)
                        {
                            LogSystem.Trace(ex);
                            ok = false;
                        }

                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        try
                        {
                            // Add a corridor
                            ok = add_corridor(ep, false);
                        }
                        catch (Exception ex)
                        {
                            LogSystem.Trace(ex);
                            ok = false;
                        }

                        break;
                    case 8:
                        try
                        {
                            // Add a doorway
                            if (ep.Category != TileCategory.Doorway)
                                ok = add_doorway(ep);
                        }
                        catch (Exception ex)
                        {
                            LogSystem.Trace(ex);
                            ok = false;
                        }

                        break;
                    case 9:
                        try
                        {
                            // Add a stairway
                            ok = add_stairway(ep);
                        }
                        catch (Exception ex)
                        {
                            LogSystem.Trace(ex);
                            ok = false;
                        }

                        break;
                }

                if (ok)
                {
                    FEndpoints.Remove(ep);
                    failures = 0;

                    callback(null, null);
                }
                else
                {
                    failures += 1;
                }
            }

            // Clean the map
            var obsolete = new List<TileData>();
            foreach (var td in _fMap.Tiles)
            {
                // Remove any tiles that somehow don't exist
                var tile = Session.FindTile(td.TileId, SearchType.Global);
                if (tile == null)
                {
                    obsolete.Add(td);
                    continue;
                }

                // Remove doors which don't go anywhere
                if (tile.Category == TileCategory.Doorway)
                {
                    var tileRect = get_rect(tile, td);

                    var clearSides = 0;

                    // North
                    for (var x = tileRect.Left; x != tileRect.Right; ++x)
                    {
                        var y = tileRect.Top - 1;
                        var pt = new Point(x, y);

                        if (tile_at_point(pt) == null)
                        {
                            clearSides += 1;
                            break;
                        }
                    }

                    // South
                    for (var x = tileRect.Left; x != tileRect.Right; ++x)
                    {
                        var y = tileRect.Bottom + 1;
                        var pt = new Point(x, y);

                        if (tile_at_point(pt) == null)
                        {
                            clearSides += 1;
                            break;
                        }
                    }

                    // West
                    for (var y = tileRect.Top; y != tileRect.Bottom; ++y)
                    {
                        var x = tileRect.Left - 1;
                        var pt = new Point(x, y);

                        if (tile_at_point(pt) == null)
                        {
                            clearSides += 1;
                            break;
                        }
                    }

                    // East
                    for (var y = tileRect.Top; y != tileRect.Bottom; ++y)
                    {
                        var x = tileRect.Right + 1;
                        var pt = new Point(x, y);

                        if (tile_at_point(pt) == null)
                        {
                            clearSides += 1;
                            break;
                        }
                    }

                    if (clearSides != 2)
                        obsolete.Add(td);
                }
            }

            foreach (var td in obsolete)
            {
                _fMap.Tiles.Remove(td);
                callback(null, null);
            }
        }

        private static void begin_map()
        {
            var options = new List<TileCategory>();
            if (FCorridorTiles.Count != 0)
                options.Add(TileCategory.Plain);
            if (FTiles[TileCategory.Stairway].Count != 0)
                options.Add(TileCategory.Stairway);

            if (options.Count == 0)
                return;

            var n = Session.Random.Next() % options.Count;
            var option = options[n];
            switch (option)
            {
                case TileCategory.Plain:
                    // Start with a corridor
                    add_corridor(null, false);
                    break;
                case TileCategory.Stairway:
                    // Start with a stairway
                    add_stairway(null);
                    break;
            }
        }

        private static bool add_area(Endpoint ep)
        {
            if (FRoomTiles.Count == 0)
                return false;

            // Start with an open area with 1-5 room tiles
            var tiles = new List<Tile>();
            var tileCount = 1 + Session.Random.Next() % 5;
            while (tiles.Count != tileCount)
            {
                var index = Session.Random.Next() % FRoomTiles.Count;
                var t = FRoomTiles[index];
                tiles.Add(t);
            }

            // Add the tiles to the map
            var endpoints = new List<Endpoint>();
            endpoints.Add(ep);
            var tileSet = new List<Pair<Tile, TileData>>();
            foreach (var t in tiles)
            {
                if (endpoints.Count == 0)
                    break;

                // Pick an endpoint
                var index = Session.Random.Next() % endpoints.Count;
                var current = endpoints[index];

                var pair = add_tile(t, current, false, false);
                if (pair != null)
                {
                    endpoints.Remove(current);
                    tileSet.Add(new Pair<Tile, TileData>(t, pair.First));

                    if (pair.Second != Direction.South)
                        // Add northern endpoint
                        endpoints.Add(get_endpoint(t, pair.First, Direction.North));

                    if (pair.Second != Direction.West)
                        // Add eastern endpoint
                        endpoints.Add(get_endpoint(t, pair.First, Direction.East));

                    if (pair.Second != Direction.North)
                        // Add southern endpoint
                        endpoints.Add(get_endpoint(t, pair.First, Direction.South));

                    if (pair.Second != Direction.East)
                        // Add western endpoint
                        endpoints.Add(get_endpoint(t, pair.First, Direction.West));
                }
            }

            if (tileSet.Count != 0)
            {
                // Add the area to the map
                add_map_area(tileSet);

                var featureTiles = FTiles[TileCategory.Feature];
                if (featureTiles.Count != 0)
                {
                    var totalArea = 0;
                    foreach (var pair in tileSet)
                        totalArea += pair.First.Area;

                    var featureCount = Session.Random.Next() % (totalArea / 10);
                    var features = 0;
                    var featureFailures = 0;
                    var featureSet = new List<Pair<Tile, TileData>>();
                    while (features != featureCount)
                    {
                        if (featureFailures == 1000)
                            break;

                        // Pick a tile
                        var index = Session.Random.Next() % featureTiles.Count;
                        var t = featureTiles[index];

                        var td = new TileData();
                        td.TileId = t.Id;
                        td.Rotations = Session.Random.Next() % 4;

                        var featureWidth = td.Rotations % 2 == 0 ? t.Size.Width : t.Size.Height;
                        var featureHeight = td.Rotations % 2 == 0 ? t.Size.Height : t.Size.Width;

                        // Look for tiles it fits on
                        var candidates = new List<Pair<Tile, TileData>>();
                        foreach (var pair in tileSet)
                        {
                            var targetWidth = pair.Second.Rotations % 2 == 0
                                ? pair.First.Size.Width
                                : pair.First.Size.Height;
                            var targetHeight = pair.Second.Rotations % 2 == 0
                                ? pair.First.Size.Height
                                : pair.First.Size.Width;

                            var dx = targetWidth - featureWidth;
                            var dy = targetHeight - featureHeight;

                            if (dx >= 0 && dy >= 0)
                                candidates.Add(pair);
                        }

                        var added = false;
                        if (candidates.Count != 0)
                        {
                            var targetIndex = Session.Random.Next() % candidates.Count;
                            var target = candidates[targetIndex];

                            var targetWidth = target.Second.Rotations % 2 == 0
                                ? target.First.Size.Width
                                : target.First.Size.Height;
                            var targetHeight = target.Second.Rotations % 2 == 0
                                ? target.First.Size.Height
                                : target.First.Size.Width;

                            var dx = targetWidth - featureWidth;
                            var dy = targetHeight - featureHeight;
                            if (dx >= 0 && dy >= 0)
                            {
                                var x = target.Second.Location.X;
                                if (dx != 0)
                                    x += Session.Random.Next() % dx;

                                var y = target.Second.Location.Y;
                                if (dy != 0)
                                    y += Session.Random.Next() % dy;

                                td.Location = new Point(x, y);

                                // Check other features don't obstruct
                                var ok = true;
                                var newFeature = get_rect(t, td);
                                foreach (var feature in featureSet)
                                {
                                    var existing = get_rect(feature.First, feature.Second);
                                    if (existing.IntersectsWith(newFeature))
                                    {
                                        ok = false;
                                        break;
                                    }
                                }

                                if (ok)
                                {
                                    _fMap.Tiles.Add(td);
                                    featureSet.Add(new Pair<Tile, TileData>(t, td));

                                    added = true;
                                    break;
                                }
                            }
                        }

                        if (added)
                        {
                            features += 1;
                            featureFailures = 0;
                        }
                        else
                        {
                            featureFailures += 1;
                        }
                    }
                }

                // Create some exits off this room
                var exitCount = 1 + Session.Random.Next() % 3;
                var exits = 0;
                var exitFailures = 0;
                while (exits != exitCount)
                {
                    if (endpoints.Count == 0)
                        break;

                    if (exitFailures == 1000)
                        break;

                    var index = Session.Random.Next() % endpoints.Count;
                    var point = endpoints[index];

                    var ok = true;
                    switch (Session.Random.Next() % 2)
                    {
                        case 0:
                            ok = add_doorway(point);
                            break;
                        case 1:
                            ok = add_corridor(point, true);
                            break;
                    }

                    if (ok)
                    {
                        exits += 1;
                        endpoints.Remove(point);
                        exitFailures = 0;
                    }
                    else
                    {
                        exitFailures += 1;
                    }
                }
            }

            return tileSet.Count != 0;
        }

        private static void add_map_area(List<Pair<Tile, TileData>> tiles)
        {
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;

            foreach (var pair in tiles)
            {
                var rect = get_rect(pair.First, pair.Second);

                if (rect.Left < minX)
                    minX = rect.Left;

                if (rect.Right > maxX)
                    maxX = rect.Right;

                if (rect.Top < minY)
                    minY = rect.Top;

                if (rect.Bottom > maxY)
                    maxY = rect.Bottom;
            }

            minX -= 1;
            minY -= 1;
            maxX += 1;
            maxY += 1;

            var area = new MapArea();
            area.Name = "Area " + (_fMap.Areas.Count + 1);
            area.Region = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            _fMap.Areas.Add(area);
        }

        private static bool add_corridor(Endpoint ep, bool follow)
        {
            if (FCorridorTiles.Count == 0)
                return false;

            var index = Session.Random.Next() % FCorridorTiles.Count;
            var t = FCorridorTiles[index];

            if (ep == null)
            {
                var td = add_first_tile(t);

                var orient = get_orientation(t, td);
                var dir = get_starting_direction(orient);
                FEndpoints.Add(get_endpoint(t, td, dir));
            }
            else
            {
                // Add the tile anywhere on the endpoint
                var pair = add_tile(t, ep, follow, true);
                if (pair == null)
                    return false;

                // Add the endpoint
                FEndpoints.Add(get_endpoint(t, pair.First, pair.Second));
            }

            return true;
        }

        private static bool add_doorway(Endpoint ep)
        {
            var doors = FTiles[TileCategory.Doorway];
            if (doors.Count == 0)
                return false;

            var index = Session.Random.Next() % doors.Count;
            var t = doors[index];

            if (ep != null)
            {
                // Add the tile anywhere on the endpoint
                var pair = add_tile(t, ep, true, true);
                if (pair == null)
                    return false;

                // Add the endpoint
                FEndpoints.Add(get_endpoint(t, pair.First, pair.Second));
            }

            return true;
        }

        private static bool add_stairway(Endpoint ep)
        {
            var stairs = FTiles[TileCategory.Stairway];
            if (stairs.Count == 0)
                return false;

            var index = Session.Random.Next() % stairs.Count;
            var t = stairs[index];

            if (ep == null)
            {
                var td = add_first_tile(t);

                var orient = get_orientation(t, td);
                var dir = get_starting_direction(orient);
                FEndpoints.Add(get_endpoint(t, td, dir));
            }
            else
            {
                // Add the tile following the endpoint
                var pair = add_tile(t, ep, true, true);
                if (pair == null)
                    return false;
            }

            return true;
        }

        private static TileData add_first_tile(Tile t)
        {
            var td = new TileData();
            td.TileId = t.Id;

            td.Location = new Point(0, 0);
            td.Rotations = Session.Random.Next() % 4;

            _fMap.Tiles.Add(td);
            return td;
        }

        private static Pair<TileData, Direction> add_tile(Tile t, Endpoint ep, bool followDirection, bool notAlongside)
        {
            var td = new TileData();
            td.TileId = t.Id;

            var dir = ep.Direction;
            if (!followDirection)
            {
                var dirs = new List<Direction>();
                if (ep.Direction != Direction.North)
                    dirs.Add(Direction.South);
                if (ep.Direction != Direction.East)
                    dirs.Add(Direction.West);
                if (ep.Direction != Direction.South)
                    dirs.Add(Direction.North);
                if (ep.Direction != Direction.West)
                    dirs.Add(Direction.East);

                var index = Session.Random.Next() % dirs.Count;
                dir = dirs[index];
            }

            if (followDirection)
            {
                // Since we're following the previous direction, make sure it's oriented properly

                var min = Math.Min(t.Size.Width, t.Size.Height);

                if (dir == Direction.North || dir == Direction.South)
                {
                    if (min > 1)
                    {
                        if (t.Size.Width > t.Size.Height)
                            td.Rotations = 1;
                    }
                    else
                    {
                        if (t.Size.Width < t.Size.Height)
                            td.Rotations = 1;
                    }
                }

                if (dir == Direction.East || dir == Direction.West)
                {
                    if (min > 1)
                    {
                        if (t.Size.Height > t.Size.Width)
                            td.Rotations = 1;
                    }
                    else
                    {
                        if (t.Size.Height < t.Size.Width)
                            td.Rotations = 1;
                    }
                }
            }
            else
            {
                // We can rotate as many times as we like
                td.Rotations = Session.Random.Next() % 4;
            }

            var width = td.Rotations % 2 == 0 ? t.Size.Width : t.Size.Height;
            var height = td.Rotations % 2 == 0 ? t.Size.Height : t.Size.Width;

            switch (ep.Direction)
            {
                case Direction.North:
                {
                    td.Location = new Point(ep.TopLeft.X, ep.TopLeft.Y - (height - 1));
                }
                    break;
                case Direction.East:
                {
                    td.Location = ep.TopLeft;
                }
                    break;
                case Direction.South:
                {
                    td.Location = ep.TopLeft;
                }
                    break;
                case Direction.West:
                {
                    td.Location = new Point(ep.TopLeft.X - (width - 1), ep.TopLeft.Y);
                }
                    break;
            }

            var rect = get_rect(t, td);
            if (notAlongside)
                switch (dir)
                {
                    case Direction.North:
                    case Direction.South:
                        // Increase width
                        rect = new Rectangle(rect.X - 1, rect.Y, rect.Width + 2, rect.Height);
                        break;
                    case Direction.East:
                    case Direction.West:
                        // Increase height
                        rect = new Rectangle(rect.X, rect.Y - 1, rect.Width, rect.Height + 2);
                        break;
                }

            if (!check_rect_is_empty(rect))
                return null;

            _fMap.Tiles.Add(td);
            return new Pair<TileData, Direction>(td, dir);
        }

        private static Direction get_starting_direction(Orientation orient)
        {
            switch (orient)
            {
                case Orientation.NorthSouth:
                    return Direction.South;
                case Orientation.EastWest:
                    return Direction.East;
            }

            return Session.Random.Next() % 2 == 0 ? Direction.East : Direction.South;
        }

        private static Endpoint get_endpoint(Tile t, TileData td, Direction dir)
        {
            var ep = new Endpoint();
            ep.Category = t.Category;
            ep.Direction = dir;

            var width = td.Rotations % 2 == 0 ? t.Size.Width : t.Size.Height;
            var height = td.Rotations % 2 == 0 ? t.Size.Height : t.Size.Width;

            switch (dir)
            {
                case Direction.North:
                    ep.TopLeft = new Point(td.Location.X, td.Location.Y - 1);
                    ep.BottomRight = new Point(td.Location.X + width - 1, td.Location.Y - 1);
                    break;
                case Direction.East:
                    ep.TopLeft = new Point(td.Location.X + width, td.Location.Y);
                    ep.BottomRight = new Point(td.Location.X + width, td.Location.Y + height - 1);
                    break;
                case Direction.South:
                    ep.TopLeft = new Point(td.Location.X, td.Location.Y + height);
                    ep.BottomRight = new Point(td.Location.X + width - 1, td.Location.Y + height);
                    break;
                case Direction.West:
                    ep.TopLeft = new Point(td.Location.X - 1, td.Location.Y);
                    ep.BottomRight = new Point(td.Location.X - 1, td.Location.Y + height - 1);
                    break;
            }

            return ep;
        }

        private static Orientation get_orientation(Tile t, TileData td)
        {
            var wide = t.Size.Width >= t.Size.Height;
            if (td.Rotations % 2 == 0)
                return wide ? Orientation.EastWest : Orientation.NorthSouth;
            return wide ? Orientation.NorthSouth : Orientation.EastWest;
        }

        private static void build_filled_area(EventHandler callback)
        {
            var tiles = new List<Tile>();
            var oneTiles = new List<Tile>();
            foreach (var lib in _fData.Libraries)
            foreach (var t in lib.Tiles)
                if (t.Category == TileCategory.Plain || t.Category == TileCategory.Feature)
                {
                    tiles.Add(t);

                    if (t.Area == 1)
                        oneTiles.Add(t);
                }

            if (tiles.Count == 0 || oneTiles.Count == 0)
                return;

            var ma = new MapArea();
            ma.Name = "Area";
            ma.Region = new Rectangle(0, 0, _fData.Width, _fData.Height);
            _fMap.Areas.Add(ma);

            var filledArea = 0;
            var fails = 0;
            while (true)
            {
                var oneByOne = Session.Random.Next(20) == 0;

                // Pick a tile at random
                var tileList = oneByOne ? oneTiles : tiles;
                var tileIndex = Session.Random.Next(tileList.Count);
                var tile = tileList[tileIndex];

                // Rotate it randomly
                var td = new TileData();
                td.TileId = tile.Id;
                td.Rotations = Session.Random.Next(4);

                // Find its dimensions
                var width = tile.Size.Width;
                var height = tile.Size.Height;
                if (td.Rotations == 1 || td.Rotations == 3)
                {
                    width = tile.Size.Height;
                    height = tile.Size.Width;
                }

                // Find points where we can place this tile
                var points = new List<Point>();
                if (oneByOne)
                {
                    // Find squares where a 1x1 tile is needed

                    for (var x = 0; x <= _fData.Width; ++x)
                    for (var y = 0; y <= _fData.Height; ++y)
                    {
                        var pt = new Point(x, y);
                        if (tile_at_point(pt) != null)
                            continue;

                        var borders = 0;
                        if (tile_at_point(new Point(x + 1, y)) != null)
                            borders += 1;
                        if (tile_at_point(new Point(x - 1, y)) != null)
                            borders += 1;
                        if (tile_at_point(new Point(x, y + 1)) != null)
                            borders += 1;
                        if (tile_at_point(new Point(x, y - 1)) != null)
                            borders += 1;

                        if (borders >= 3)
                            points.Add(pt);
                    }
                }
                else
                {
                    var increment = tile.Area < 4 ? 1 : 2;
                    for (var x = 0; x <= _fData.Width; x += increment)
                    for (var y = 0; y <= _fData.Height; y += increment)
                    {
                        var rect = new Rectangle(x, y, width, height);

                        if (rect.Right > _fData.Width || rect.Bottom > _fData.Height)
                            continue;

                        if (check_rect_is_empty(rect))
                        {
                            var pt = new Point(x, y);
                            points.Add(pt);
                        }
                    }
                }

                if (points.Count != 0)
                {
                    // Pick a point at random
                    var pointIndex = Session.Random.Next(points.Count);
                    var pt = points[pointIndex];

                    // Place the tile
                    td.Location = pt;
                    _fMap.Tiles.Add(td);

                    filledArea += tile.Area;
                }
                else
                {
                    fails += 1;
                    if (fails >= 100)
                    {
                        fails = 0;

                        if (_fMap.Tiles.Count != 0)
                        {
                            // Remove a tile at random
                            var removeIndex = Session.Random.Next(_fMap.Tiles.Count);
                            var removeTd = _fMap.Tiles[removeIndex];
                            _fMap.Tiles.Remove(removeTd);

                            var removeTile = Session.FindTile(removeTd.TileId, SearchType.Global);
                            filledArea -= removeTile.Area;
                        }
                    }
                }

                callback(null, null);

                var fullArea = _fData.Width * _fData.Height;
                if (filledArea == fullArea)
                {
                    _fMap.Areas.Clear();
                    break;
                }
            }
        }

        private static void build_freeform_area(EventHandler callback)
        {
            var tiles = new List<Tile>();
            foreach (var lib in _fData.Libraries)
            foreach (var t in lib.Tiles)
                if (t.Category == TileCategory.Plain || t.Category == TileCategory.Feature)
                    tiles.Add(t);
            if (tiles.Count == 0)
                return;

            var area = _fData.Height * _fData.Width;
            while (area > 0)
            {
                callback(null, null);

                // TODO: Find an enclosed space
                var enclosed = false;
                if (enclosed)
                {
                    // Pick a tile that might fit
                    Tile tile = null;

                    if (tile == null)
                        continue;

                    // Place this tile
                    var location = new Point(0, 0);

                    var tiledata = new TileData();
                    tiledata.TileId = tile.Id;
                    tiledata.Location = location;
                    _fMap.Tiles.Add(tiledata);

                    // Update the available area
                    area -= tile.Area;
                }
                else
                {
                    // Otherwise, pick a tile and add it contiguously
                    var index = Session.Random.Next() % tiles.Count;
                    var tile = tiles[index];

                    var location = new Point(0, 0);
                    if (_fMap.Tiles.Count != 0)
                    {
                        // Pick a current tile
                        var tdIndex = Session.Random.Next() % _fMap.Tiles.Count;
                        var td = _fMap.Tiles[tdIndex];
                        var t = Session.FindTile(td.TileId, SearchType.Global);

                        // Find rectangles next to this tile
                        var rects = new List<Rectangle>();
                        var xMin = td.Location.X - (tile.Size.Width - 1);
                        var xMax = td.Location.X + (t.Size.Width - 1);
                        var yMin = td.Location.Y - (tile.Size.Height - 1);
                        var yMax = td.Location.Y + (t.Size.Height - 1);
                        for (var x = xMin; x <= xMax; ++x)
                        {
                            // North
                            var y = td.Location.Y - tile.Size.Height;
                            var rect = new Rectangle(x, y, tile.Size.Width, tile.Size.Height);
                            rects.Add(rect);
                        }

                        for (var x = xMin; x <= xMax; ++x)
                        {
                            // South
                            var y = td.Location.Y + t.Size.Height;
                            var rect = new Rectangle(x, y, tile.Size.Width, tile.Size.Height);
                            rects.Add(rect);
                        }

                        for (var y = yMin; y <= yMax; ++y)
                        {
                            // West
                            var x = td.Location.X - t.Size.Width;
                            var rect = new Rectangle(x, y, tile.Size.Width, tile.Size.Height);
                            rects.Add(rect);
                        }

                        for (var y = yMin; y <= yMax; ++y)
                        {
                            // East
                            var x = td.Location.X + t.Size.Width;
                            var rect = new Rectangle(x, y, tile.Size.Width, tile.Size.Height);
                            rects.Add(rect);
                        }

                        var candidates = new List<Rectangle>();
                        foreach (var rect in rects)
                            if (check_rect_is_empty(rect))
                                candidates.Add(rect);

                        if (candidates.Count == 0)
                            continue;

                        var rectIndex = Session.Random.Next() % candidates.Count;
                        var r = candidates[rectIndex];
                        location = r.Location;
                    }

                    var tiledata = new TileData();
                    tiledata.TileId = tile.Id;
                    tiledata.Location = location;
                    _fMap.Tiles.Add(tiledata);

                    // Update the available area
                    area -= tile.Area;
                }
            }

            var left = 0;
            var right = 0;
            var top = 0;
            var bottom = 0;
            foreach (var td in _fMap.Tiles)
            {
                var tile = Session.FindTile(td.TileId, SearchType.Global);
                var rect = new Rectangle(td.Location, tile.Size);

                left = Math.Min(left, rect.Left);
                right = Math.Max(right, rect.Right);
                top = Math.Min(top, rect.Top);
                bottom = Math.Max(bottom, rect.Bottom);
            }

            var ma = new MapArea();
            ma.Name = "Area";
            ma.Region = new Rectangle(left, top, right - left, bottom - top);
            _fMap.Areas.Add(ma);
        }

        private static bool check_rect_is_empty(Rectangle rect)
        {
            foreach (var td in _fMap.Tiles)
            {
                var t = Session.FindTile(td.TileId, SearchType.Global);
                var tileRect = get_rect(t, td);

                if (tileRect.IntersectsWith(rect))
                    return false;
            }

            return true;
        }

        private static TileData tile_at_point(Point pt)
        {
            foreach (var td in _fMap.Tiles)
            {
                var t = Session.FindTile(td.TileId, SearchType.Global);
                var tileRect = get_rect(t, td);

                if (tileRect.Contains(pt))
                    return td;
            }

            return null;
        }

        private static Rectangle get_rect(Tile t, TileData td)
        {
            var width = td.Rotations % 2 == 0 ? t.Size.Width : t.Size.Height;
            var height = td.Rotations % 2 == 0 ? t.Size.Height : t.Size.Width;

            return new Rectangle(td.Location.X, td.Location.Y, width, height);
        }
    }
}
