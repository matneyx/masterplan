using System;
using System.Collections.Generic;
using System.Drawing;
using Masterplan.Data;

namespace Masterplan.Tools.Generators
{
    internal class DelveBuilder
    {
        public static PlotPoint AutoBuild(Map map, AutoBuildData data)
        {
            var pp = new PlotPoint(map.Name + " Delve");
            pp.Details = "This delve was automatically generated.";
            pp.Element = new MapElement(map.Id, Guid.Empty);

            var parcelLevel = data.Level;
            var parcels = Treasure.CreateParcelSet(data.Level, Session.Project.Party.Size, false);

            foreach (var ma in map.Areas)
            {
                var point = new PlotPoint(ma.Name);

                switch (Session.Random.Next() % 8)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        // Encounter
                        point.Element = get_encounter(map, ma, data);
                        break;
                    case 6:
                        // Trap
                        point.Element = get_encounter(map, ma, data);
                        break;
                    case 7:
                        // Skill challenge
                        point.Element = get_encounter(map, ma, data);
                        break;
                }

                // Treasure parcels
                var parcelCount = 0;
                switch (Session.Random.Next() % 8)
                {
                    case 0:
                    case 1:
                        // No parcels
                        parcelCount = 0;
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        // One parcel
                        parcelCount = 1;
                        break;
                    case 7:
                        // Two parcels
                        parcelCount = 2;
                        break;
                }

                for (var n = 0; n != parcelCount; ++n)
                {
                    if (parcels.Count == 0)
                    {
                        // Generate a new list of parcels of the next level
                        parcelLevel = Math.Min(30, parcelLevel + 1);
                        parcels = Treasure.CreateParcelSet(parcelLevel, Session.Project.Party.Size, false);
                    }

                    var index = Session.Random.Next() % parcels.Count;
                    var p = parcels[index];
                    parcels.RemoveAt(index);

                    point.Parcels.Add(p);
                }

                pp.Subplot.Points.Add(point);
            }

            return pp;
        }

        private static Encounter get_encounter(Map map, MapArea ma, AutoBuildData data)
        {
            // Set up the encounter
            var enc = new Encounter();
            enc.MapId = map.Id;
            enc.MapAreaId = ma.Id;
            EncounterBuilder.Build(data, enc, false);

            var diff = enc.GetDifficulty(Session.Project.Party.Level, Session.Project.Party.Size);
            if (diff != Difficulty.Extreme)
                // Add a trap or skill challenge
                switch (Session.Random.Next() % 6)
                {
                    case 0:
                    case 1:
                    case 3:
                        // Add a trap
                        var t = select_trap(data);
                        if (t != null)
                            enc.Traps.Add(t);
                        break;
                    case 4:
                        // Add a skill challenge
                        var sc = select_challenge(data);
                        if (sc != null)
                            enc.SkillChallenges.Add(sc);
                        break;
                }

            // Make matrix of tile squares
            var tiles = new List<Rectangle>();
            foreach (var td in map.Tiles)
            {
                var t = Session.FindTile(td.TileId, SearchType.Global);
                var width = td.Rotations % 2 == 0 ? t.Size.Width : t.Size.Height;
                var height = td.Rotations % 2 == 0 ? t.Size.Height : t.Size.Width;
                var sz = new Size(width, height);

                var rect = new Rectangle(td.Location, sz);
                tiles.Add(rect);
            }

            var matrix = new Dictionary<Point, bool>();
            for (var x = ma.Region.Left; x != ma.Region.Right; ++x)
            for (var y = ma.Region.Top; y != ma.Region.Bottom; ++y)
            {
                var pt = new Point(x, y);

                // Is there a tile at this location?
                var open = false;
                foreach (var rect in tiles)
                    if (rect.Contains(pt))
                    {
                        open = true;
                        break;
                    }

                matrix[pt] = open;
            }

            // Place creatures on the map
            foreach (var slot in enc.Slots)
            {
                var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                var size = Creature.GetSize(creature.Size);

                foreach (var cd in slot.CombatData)
                {
                    // Find candidate points
                    var candidates = new List<Point>();
                    for (var x = ma.Region.Left; x != ma.Region.Right; ++x)
                    for (var y = ma.Region.Top; y != ma.Region.Bottom; ++y)
                    {
                        var pt = new Point(x, y);

                        // Is this location free?
                        var allOpen = true;
                        for (var dx = pt.X; dx != pt.X + size; ++dx)
                        for (var dy = pt.Y; dy != pt.Y + size; ++dy)
                        {
                            var loc = new Point(dx, dy);
                            if (!matrix.ContainsKey(loc) || !matrix[loc])
                                allOpen = false;
                        }

                        if (allOpen)
                            candidates.Add(pt);
                    }

                    if (candidates.Count != 0)
                    {
                        var index = Session.Random.Next() % candidates.Count;
                        var loc = candidates[index];

                        // Place creature
                        cd.Location = loc;

                        // This space is now occupied
                        for (var x = loc.X; x != loc.X + size; ++x)
                        for (var y = loc.Y; y != loc.Y + size; ++y)
                        {
                            var pt = new Point(x, y);
                            matrix[pt] = false;
                        }
                    }
                }
            }

            // Set up notes

            enc.SetStandardEncounterNotes();
            var lightNote = enc.FindNote("Illumination");
            if (lightNote != null)
            {
                var n = Session.Random.Next(6);
                switch (n)
                {
                    case 0:
                    case 1:
                    case 2:
                        lightNote.Contents = "The area is in bright light.";
                        break;
                    case 3:
                    case 4:
                        lightNote.Contents = "The area is in dim light.";
                        break;
                    case 5:
                        lightNote.Contents = "None.";
                        break;
                }
            }

            var victoryNote = enc.FindNote("Victory Conditions");
            if (victoryNote != null)
            {
                var candidates = new List<string>();

                var leaders = new List<string>();
                var hasMinions = false;
                var nonMinions = 0;
                foreach (var slot in enc.Slots)
                {
                    if (slot.CombatData.Count == 1)
                        if (slot.Card.Leader || slot.Card.Flag == RoleFlag.Elite || slot.Card.Flag == RoleFlag.Solo)
                            leaders.Add(slot.CombatData[0].DisplayName);

                    var c = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                    if (c != null)
                    {
                        if (c.Role is Minion)
                            hasMinions = true;
                        else
                            nonMinions += slot.CombatData.Count;
                    }
                }

                if (leaders.Count != 0)
                {
                    var index = Session.Random.Next() % leaders.Count;
                    var leader = leaders[index];

                    if (Session.Random.Next() % 12 == 0)
                    {
                        candidates.Add("Defeat " + leader + ".");
                        candidates.Add("Capture " + leader + ".");
                    }

                    if (Session.Random.Next() % 12 == 0)
                    {
                        var rounds = Session.Dice(2, 4);
                        candidates.Add("The party must defeat " + leader + " within " + rounds + " rounds.");
                    }

                    if (Session.Random.Next() % 12 == 0)
                    {
                        var rounds = Session.Dice(2, 4);
                        candidates.Add("After " + rounds + ", " + leader + " will flee or surrender.");
                    }

                    if (Session.Random.Next() % 12 == 0)
                    {
                        var hp = 10 * Session.Dice(1, 4);
                        candidates.Add("At " + hp + "% HP, " + leader + " will flee or surrender.");
                    }

                    if (Session.Random.Next() % 12 == 0)
                        candidates.Add("The party must obtain an item from " + leader + ".");

                    if (Session.Random.Next() % 12 == 0)
                        candidates.Add("Defeat " + leader + " by destroying a guarded object in the area.");

                    if (hasMinions)
                        candidates.Add("Minions will flee or surrender when " + leader + " is defeated.");
                }

                if (Session.Random.Next() % 12 == 0)
                {
                    var rounds = 2 + Session.Random.Next() % 4;
                    candidates.Add("The party must defeat their opponents within " + rounds + " rounds.");
                }

                if (hasMinions && Session.Random.Next() % 12 == 0)
                {
                    var waves = 2 + Session.Random.Next() % 4;
                    candidates.Add("The party must defend a certain area from " + waves + " waves of minions.");
                }

                if (Session.Random.Next() % 12 == 0)
                {
                    var rounds = 2 + Session.Random.Next() % 4;
                    candidates.Add("At least one character must get to a certain area and stay there for " + rounds +
                                   " consecutive rounds.");
                }

                if (Session.Random.Next() % 12 == 0)
                {
                    var rounds = 2 + Session.Random.Next() % 4;
                    candidates.Add("The party must leave the area within " + rounds + " rounds.");
                }

                if (Session.Random.Next() % 12 == 0)
                    candidates.Add(
                        "The party must keep the enemy away from a certain area for the duration of the encounter.");

                if (Session.Random.Next() % 12 == 0)
                    candidates.Add("The party must escort an NPC safely through the encounter area.");

                if (Session.Random.Next() % 12 == 0)
                    candidates.Add("The party must rescue an NPC from their opponents.");

                if (Session.Random.Next() % 12 == 0)
                    candidates.Add("The party must avoid contact with the enemy in this area.");

                if (Session.Random.Next() % 12 == 0)
                    candidates.Add("The party must attack and destroy a feature of the area.");

                if (nonMinions > 1)
                    if (Session.Random.Next() % 12 == 0)
                    {
                        var n = 1 + Session.Random.Next(nonMinions);
                        candidates.Add("The party must defeat " + n + " non-minion opponents.");
                    }

                if (candidates.Count != 0)
                {
                    // Select an option
                    var index = Session.Random.Next() % candidates.Count;
                    victoryNote.Contents = candidates[index];
                }
            }

            return enc;
        }

        private static TrapElement get_trap(Map map, MapArea ma, AutoBuildData data)
        {
            var t = select_trap(data);
            if (t != null)
            {
                var te = new TrapElement();

                te.Trap = t;
                te.MapId = map.Id;
                te.MapAreaId = ma.Id;

                return te;
            }

            return null;
        }

        private static SkillChallenge get_challenge(Map map, MapArea ma, AutoBuildData data)
        {
            var sc = select_challenge(data);
            if (sc != null)
            {
                sc.MapId = map.Id;
                sc.MapAreaId = ma.Id;

                return sc;
            }

            return null;
        }

        private static Trap select_trap(AutoBuildData data)
        {
            var traps = new List<Trap>();

            var minLevel = data.Level - 3;
            var maxLevel = data.Level + 5;

            traps.Clear();
            foreach (var trap in Session.Traps)
            {
                if (trap.Level < minLevel || trap.Level > maxLevel)
                    continue;

                traps.Add(trap.Copy());
            }

            if (traps.Count != 0)
            {
                var index = Session.Random.Next() % traps.Count;
                return traps[index];
            }

            return null;
        }

        private static SkillChallenge select_challenge(AutoBuildData data)
        {
            var challenges = new List<SkillChallenge>();

            var minLevel = data.Level - 3;
            var maxLevel = data.Level + 5;

            challenges.Clear();
            foreach (var sc in Session.SkillChallenges)
                if (sc.Level == -1)
                {
                    var challenge = sc.Copy() as SkillChallenge;
                    challenge.Level = Session.Project.Party.Level;
                    challenges.Add(challenge);
                }
                else
                {
                    if (sc.Level < minLevel || sc.Level > maxLevel)
                        continue;

                    challenges.Add(sc.Copy() as SkillChallenge);
                }

            if (challenges.Count != 0)
            {
                var index = Session.Random.Next() % challenges.Count;
                return challenges[index];
            }

            return null;
        }
    }
}
