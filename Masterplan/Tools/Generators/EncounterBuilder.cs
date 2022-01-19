using System;
using System.Collections.Generic;
using Masterplan.Data;

namespace Masterplan.Tools.Generators
{
    internal class EncounterBuilder
    {
        private const int Tries = 100;

        private static readonly List<EncounterTemplateGroup> FTemplateGroups = new List<EncounterTemplateGroup>();
        private static readonly List<EncounterCard> FCreatures = new List<EncounterCard>();
        private static readonly List<Trap> FTraps = new List<Trap>();
        private static readonly List<SkillChallenge> FChallenges = new List<SkillChallenge>();

        public static bool Build(AutoBuildData data, Encounter enc, bool includeIndividual)
        {
            var minLevel = Math.Max(data.Level - 4, 1);
            var maxLevel = data.Level + 5;
            build_creature_list(minLevel, maxLevel, data.Categories, data.Keywords, true);

            if (FCreatures.Count == 0)
                return false;

            build_template_list(data.Type, data.Difficulty, data.Level, includeIndividual);
            if (FTemplateGroups.Count == 0)
                return false;

            build_trap_list(data.Level);
            build_challenge_list(data.Level);

            var attempts = 0;
            while (attempts < Tries)
            {
                attempts += 1;

                // Pick a template
                var groupIndex = Session.Random.Next() % FTemplateGroups.Count;
                var group = FTemplateGroups[groupIndex];
                var templateIndex = Session.Random.Next() % group.Templates.Count;
                var template = group.Templates[templateIndex];

                var ok = true;
                var slots = new List<EncounterSlot>();
                foreach (var templateSlot in template.Slots)
                {
                    // Attempt to populate the slot
                    var candidates = new List<EncounterCard>();
                    foreach (var card in FCreatures)
                        if (templateSlot.Match(card, data.Level))
                            candidates.Add(card);

                    if (candidates.Count == 0)
                    {
                        ok = false;
                        break;
                    }

                    // Choose a candidate
                    var creatureIndex = Session.Random.Next() % candidates.Count;
                    var candidate = candidates[creatureIndex];

                    // Build the slot
                    var slot = new EncounterSlot();
                    slot.Card = candidate;
                    for (var n = 0; n != templateSlot.Count; ++n)
                    {
                        var ccd = new CombatData();
                        slot.CombatData.Add(ccd);
                    }

                    slots.Add(slot);
                }

                if (ok)
                {
                    enc.Slots = slots;
                    enc.Traps.Clear();
                    enc.SkillChallenges.Clear();

                    // Random modification
                    switch (Session.Random.Next(12))
                    {
                        case 4:
                        case 5:
                        {
                            // Replace a creature with a trap / hazard
                            if (add_trap(enc))
                                remove_creature(enc);
                        }
                            break;
                        case 6:
                        {
                            // Replace a creature with a skill challenge
                            if (add_challenge(enc))
                                remove_creature(enc);
                        }
                            break;
                        case 7:
                        {
                            // Replace a creature with a lurker
                            if (add_lurker(enc))
                                remove_creature(enc);
                        }
                            break;
                        case 8:
                        case 9:
                        {
                            // Add a trap / hazard
                            add_trap(enc);
                            var diff = enc.GetDifficulty(data.Level, data.Size);
                            if (diff == Difficulty.Hard || diff == Difficulty.Extreme)
                                remove_creature(enc);
                        }
                            break;
                        case 10:
                        {
                            // Add a skill challenge
                            var diff = enc.GetDifficulty(data.Level, data.Size);
                            if (diff == Difficulty.Hard || diff == Difficulty.Extreme)
                                remove_creature(enc);
                            add_challenge(enc);
                        }
                            break;
                        case 11:
                        {
                            // Add a lurker
                            add_lurker(enc);
                            var diff = enc.GetDifficulty(data.Level, data.Size);
                            if (diff == Difficulty.Hard || diff == Difficulty.Extreme)
                                remove_creature(enc);
                        }
                            break;
                    }

                    while (enc.GetDifficulty(data.Level, data.Size) == Difficulty.Extreme && enc.Count > 1)
                        remove_creature(enc);

                    foreach (var slot in enc.Slots)
                        slot.SetDefaultDisplayNames();

                    return true;
                }
            }

            // We were unable to build the encounter
            return false;
        }

        private static void remove_creature(Encounter enc)
        {
            if (enc.Count == 0)
                return;

            var index = Session.Random.Next() % enc.Slots.Count;
            var slot = enc.Slots[index];

            if (slot.CombatData.Count == 1)
                enc.Slots.Remove(slot);
            else
                slot.CombatData.RemoveAt(slot.CombatData.Count - 1);
        }

        private static bool add_trap(Encounter enc)
        {
            if (FTraps.Count != 0)
            {
                var index = Session.Random.Next() % FTraps.Count;
                var trap = FTraps[index];
                enc.Traps.Add(trap.Copy());

                return true;
            }

            return false;
        }

        private static bool add_challenge(Encounter enc)
        {
            if (FChallenges.Count != 0)
            {
                var index = Session.Random.Next() % FChallenges.Count;
                var sc = FChallenges[index];
                enc.SkillChallenges.Add(sc.Copy() as SkillChallenge);

                return true;
            }

            return false;
        }

        private static bool add_lurker(Encounter enc)
        {
            var lurkers = new List<EncounterCard>();
            foreach (var card in FCreatures)
            {
                if (card.Flag != RoleFlag.Standard)
                    continue;

                if (card.Roles.Contains(RoleType.Lurker))
                    lurkers.Add(card);
            }

            if (lurkers.Count != 0)
            {
                var index = Session.Random.Next() % lurkers.Count;

                var slot = new EncounterSlot();
                slot.Card = lurkers[index];
                slot.CombatData.Add(new CombatData());

                enc.Slots.Add(slot);

                return true;
            }

            return false;
        }

        public static EncounterDeck BuildDeck(int level, List<string> categories, List<string> keywords)
        {
            build_creature_list(level - 2, level + 5, categories, keywords, false);
            if (FCreatures.Count == 0)
                return null;

            var roleBreakdown = new Dictionary<CardCategory, Pair<int, int>>();
            roleBreakdown[CardCategory.SoldierBrute] = new Pair<int, int>(0, 18);
            roleBreakdown[CardCategory.Skirmisher] = new Pair<int, int>(0, 14);
            roleBreakdown[CardCategory.Minion] = new Pair<int, int>(0, 5);
            roleBreakdown[CardCategory.Artillery] = new Pair<int, int>(0, 5);
            roleBreakdown[CardCategory.Controller] = new Pair<int, int>(0, 5);
            roleBreakdown[CardCategory.Lurker] = new Pair<int, int>(0, 2);
            roleBreakdown[CardCategory.Solo] = new Pair<int, int>(0, 1);

            var diffBreakdown = new Dictionary<Difficulty, Pair<int, int>>();
            if (level >= 3)
            {
                diffBreakdown[Difficulty.Trivial] = new Pair<int, int>(0, 7);
                diffBreakdown[Difficulty.Easy] = new Pair<int, int>(0, 30);
            }
            else
            {
                diffBreakdown[Difficulty.Easy] = new Pair<int, int>(0, 37);
            }

            diffBreakdown[Difficulty.Moderate] = new Pair<int, int>(0, 8);
            diffBreakdown[Difficulty.Hard] = new Pair<int, int>(0, 5);
            diffBreakdown[Difficulty.Extreme] = new Pair<int, int>(0, 0);

            var deck = new EncounterDeck();
            deck.Level = level;

            var attempts = 0;
            while (attempts < Tries)
            {
                attempts += 1;

                // Pick a card
                var cardIndex = Session.Random.Next() % FCreatures.Count;
                var card = FCreatures[cardIndex];

                // Do we need a card of this type?
                var cat = card.Category;
                var roleCounts = roleBreakdown[cat];
                var roleOk = roleCounts.First < roleCounts.Second;
                if (!roleOk)
                    continue;

                // Do we need a card of this level?
                var diff = card.GetDifficulty(level);
                var diffCounts = diffBreakdown[diff];
                var levelOk = diffCounts.First < diffCounts.Second;
                if (!levelOk)
                    continue;

                // Add this card to the deck
                deck.Cards.Add(card);
                roleBreakdown[cat].First += 1;
                diffBreakdown[diff].First += 1;

                if (deck.Cards.Count == 50)
                    break;
            }

            FillDeck(deck);

            return deck;
        }

        public static void FillDeck(EncounterDeck deck)
        {
            build_creature_list(deck.Level - 2, deck.Level + 5, null, null, false);
            if (FCreatures.Count == 0)
                return;

            while (deck.Cards.Count < 50)
            {
                // Pick a card
                var cardIndex = Session.Random.Next() % FCreatures.Count;
                var card = FCreatures[cardIndex];

                // Add this card to the deck
                deck.Cards.Add(card);
            }
        }

        public static List<Pair<EncounterTemplateGroup, EncounterTemplate>> FindTemplates(Encounter enc, int level,
            bool includeIndividual)
        {
            build_template_list("", Difficulty.Random, level, includeIndividual);

            var templates = new List<Pair<EncounterTemplateGroup, EncounterTemplate>>();
            foreach (var group in FTemplateGroups)
            foreach (var template in @group.Templates)
            {
                var tmpl = template.Copy();

                // Does this template fit what we've already got?
                var match = true;
                foreach (var encSlot in enc.Slots)
                {
                    var templateSlot = tmpl.FindSlot(encSlot, level);

                    if (templateSlot != null)
                    {
                        templateSlot.Count -= encSlot.CombatData.Count;
                        if (templateSlot.Count <= 0)
                            tmpl.Slots.Remove(templateSlot);
                    }
                    else
                    {
                        match = false;
                        break;
                    }
                }

                if (!match)
                    continue;

                // Make sure there's at least one possible creature for each remaining slot
                var hasCreatures = true;
                foreach (var ets in tmpl.Slots)
                {
                    var foundCreature = false;

                    var creatureLevel = level + ets.LevelAdjustment;
                    build_creature_list(creatureLevel, creatureLevel, null, null, true);
                    foreach (var card in FCreatures)
                        if (ets.Match(card, level))
                        {
                            foundCreature = true;
                            break;
                        }

                    if (!foundCreature)
                    {
                        hasCreatures = false;
                        break;
                    }
                }

                if (!hasCreatures)
                    continue;

                templates.Add(new Pair<EncounterTemplateGroup, EncounterTemplate>(@group, tmpl));
            }

            return templates;
        }

        public static List<string> FindTemplateNames()
        {
            build_template_list("", Difficulty.Random, -1, true);

            var names = new List<string>();

            foreach (var group in FTemplateGroups)
                names.Add(group.Name);

            names.Sort();

            return names;
        }

        public static List<EncounterCard> FindCreatures(EncounterTemplateSlot slot, int partyLevel, string query)
        {
            var creatureLevel = partyLevel + slot.LevelAdjustment;
            build_creature_list(creatureLevel, creatureLevel, null, null, true);

            var cards = new List<EncounterCard>();
            foreach (var card in FCreatures)
                if (slot.Match(card, partyLevel) && Match(card, query))
                    cards.Add(card);

            return cards;
        }

        private static bool Match(EncounterCard card, string query)
        {
            var tokens = query.ToLower().Split();

            foreach (var token in tokens)
                if (!match_token(card, token))
                    return false;

            return true;
        }

        private static bool match_token(EncounterCard card, string token)
        {
            if (card.Title.ToLower().Contains(token) || card.Category.ToString().ToLower().Contains(token))
                return true;

            return false;
        }

        private static void build_template_list(string groupName, Difficulty diff, int level, bool includeIndividual)
        {
            FTemplateGroups.Clear();

            build_template_battlefield_control();
            build_template_commander_and_troops();
            build_template_double_line();
            build_template_dragons_den();
            build_template_grand_melee();
            build_template_wolf_pack();

            if (includeIndividual)
                build_template_duel();

            // Filter by name
            if (groupName != "")
            {
                var obsolete = new List<EncounterTemplateGroup>();
                foreach (var group in FTemplateGroups)
                    if (@group.Name != groupName)
                        obsolete.Add(@group);
                foreach (var group in obsolete)
                    FTemplateGroups.Remove(group);
            }

            // Filter by difficulty
            if (diff != Difficulty.Random)
            {
                var obsoleteGroups = new List<EncounterTemplateGroup>();
                foreach (var group in FTemplateGroups)
                {
                    var obsolete = new List<EncounterTemplate>();
                    foreach (var template in group.Templates)
                        if (template.Difficulty != diff)
                            obsolete.Add(template);
                    foreach (var template in obsolete)
                        group.Templates.Remove(template);

                    if (group.Templates.Count == 0)
                        obsoleteGroups.Add(group);
                }

                foreach (var group in obsoleteGroups)
                    FTemplateGroups.Remove(group);
            }

            // Filter by level (removes those we don't have creatures to fill)
            if (level != -1)
            {
                var obsoleteGroups = new List<EncounterTemplateGroup>();
                foreach (var group in FTemplateGroups)
                {
                    var obsolete = new List<EncounterTemplate>();
                    foreach (var template in group.Templates)
                    {
                        var ok = true;
                        foreach (var ets in template.Slots)
                            if (level + ets.LevelAdjustment < 1)
                            {
                                ok = false;
                                break;
                            }

                        if (!ok)
                            obsolete.Add(template);
                    }

                    foreach (var template in obsolete)
                        group.Templates.Remove(template);

                    if (group.Templates.Count == 0)
                        obsoleteGroups.Add(group);
                }

                foreach (var group in obsoleteGroups)
                    FTemplateGroups.Remove(group);
            }
        }

        private static void build_template_battlefield_control()
        {
            var group = new EncounterTemplateGroup("Battlefield Control", "Entire Party");

            var bc1 = new EncounterTemplate(Difficulty.Easy);
            bc1.Slots.Add(new EncounterTemplateSlot(1, -2, RoleType.Controller));
            bc1.Slots.Add(new EncounterTemplateSlot(6, -4, RoleType.Skirmisher));
            group.Templates.Add(bc1);

            var bc2 = new EncounterTemplate(Difficulty.Moderate);
            bc2.Slots.Add(new EncounterTemplateSlot(1, 1, RoleType.Controller));
            bc2.Slots.Add(new EncounterTemplateSlot(6, -2, RoleType.Skirmisher));
            group.Templates.Add(bc2);

            var bc3 = new EncounterTemplate(Difficulty.Hard);
            bc3.Slots.Add(new EncounterTemplateSlot(1, 5, RoleType.Controller));
            bc3.Slots.Add(new EncounterTemplateSlot(5, 1, RoleType.Skirmisher));
            group.Templates.Add(bc3);

            FTemplateGroups.Add(group);
        }

        private static void build_template_commander_and_troops()
        {
            var group = new EncounterTemplateGroup("Commander and Troops", "Entire Party");

            var ct1 = new EncounterTemplate(Difficulty.Easy);
            ct1.Slots.Add(new EncounterTemplateSlot(1, 0,
                new[] { RoleType.Controller, RoleType.Soldier, RoleType.Lurker, RoleType.Skirmisher }));
            ct1.Slots.Add(new EncounterTemplateSlot(4, -3, new[] { RoleType.Brute, RoleType.Soldier }));
            group.Templates.Add(ct1);

            var ct2 = new EncounterTemplate(Difficulty.Moderate);
            ct2.Slots.Add(new EncounterTemplateSlot(1, 3,
                new[] { RoleType.Controller, RoleType.Soldier, RoleType.Lurker, RoleType.Skirmisher }));
            ct2.Slots.Add(new EncounterTemplateSlot(5, -2, new[] { RoleType.Brute, RoleType.Soldier }));
            group.Templates.Add(ct2);

            var ct3 = new EncounterTemplate(Difficulty.Hard);
            ct3.Slots.Add(new EncounterTemplateSlot(1, 5,
                new[] { RoleType.Controller, RoleType.Soldier, RoleType.Lurker, RoleType.Skirmisher }));
            ct3.Slots.Add(new EncounterTemplateSlot(3, 1, new[] { RoleType.Brute, RoleType.Soldier }));
            ct3.Slots.Add(new EncounterTemplateSlot(2, 1, new[] { RoleType.Artillery }));
            group.Templates.Add(ct3);

            FTemplateGroups.Add(group);
        }

        private static void build_template_dragons_den()
        {
            var group = new EncounterTemplateGroup("Dragon's Den", "Entire Party");

            var dd1 = new EncounterTemplate(Difficulty.Easy);
            dd1.Slots.Add(new EncounterTemplateSlot(1, -2, RoleFlag.Solo));
            group.Templates.Add(dd1);

            var dd2 = new EncounterTemplate(Difficulty.Moderate);
            dd2.Slots.Add(new EncounterTemplateSlot(1, 0, RoleFlag.Solo));
            group.Templates.Add(dd2);

            var dd3 = new EncounterTemplate(Difficulty.Moderate);
            dd3.Slots.Add(new EncounterTemplateSlot(1, 1, RoleFlag.Solo));
            group.Templates.Add(dd3);

            var dd4 = new EncounterTemplate(Difficulty.Hard);
            dd4.Slots.Add(new EncounterTemplateSlot(1, 3, RoleFlag.Solo));
            group.Templates.Add(dd4);

            var dd5 = new EncounterTemplate(Difficulty.Hard);
            dd5.Slots.Add(new EncounterTemplateSlot(1, 1, RoleFlag.Solo));
            dd5.Slots.Add(new EncounterTemplateSlot(1, 0, RoleFlag.Elite));
            group.Templates.Add(dd5);

            FTemplateGroups.Add(group);
        }

        private static void build_template_double_line()
        {
            var group = new EncounterTemplateGroup("Double Line", "Entire Party");

            var dl1 = new EncounterTemplate(Difficulty.Easy);
            dl1.Slots.Add(new EncounterTemplateSlot(3, -4, new[] { RoleType.Brute, RoleType.Soldier }));
            dl1.Slots.Add(new EncounterTemplateSlot(2, -2, new[] { RoleType.Artillery, RoleType.Controller }));
            group.Templates.Add(dl1);

            var dl2 = new EncounterTemplate(Difficulty.Moderate);
            dl2.Slots.Add(new EncounterTemplateSlot(3, 0, new[] { RoleType.Brute, RoleType.Soldier }));
            dl2.Slots.Add(new EncounterTemplateSlot(2, 0, new[] { RoleType.Artillery, RoleType.Controller }));
            group.Templates.Add(dl2);

            var dl3 = new EncounterTemplate(Difficulty.Moderate);
            dl3.Slots.Add(new EncounterTemplateSlot(3, -2, new[] { RoleType.Brute, RoleType.Soldier }));
            dl3.Slots.Add(new EncounterTemplateSlot(2, 3, new[] { RoleType.Artillery, RoleType.Controller }));
            group.Templates.Add(dl3);

            var dl4 = new EncounterTemplate(Difficulty.Hard);
            dl4.Slots.Add(new EncounterTemplateSlot(3, 2, new[] { RoleType.Brute, RoleType.Soldier }));
            dl4.Slots.Add(new EncounterTemplateSlot(1, 4, RoleType.Controller));
            dl4.Slots.Add(new EncounterTemplateSlot(1, 4, new[] { RoleType.Artillery, RoleType.Lurker }));
            group.Templates.Add(dl4);

            var dl5 = new EncounterTemplate(Difficulty.Hard);
            dl5.Slots.Add(new EncounterTemplateSlot(3, 0, new[] { RoleType.Brute, RoleType.Soldier }));
            dl5.Slots.Add(new EncounterTemplateSlot(2, 1, RoleType.Artillery));
            dl5.Slots.Add(new EncounterTemplateSlot(1, 2, RoleType.Controller));
            dl5.Slots.Add(new EncounterTemplateSlot(1, 2, RoleType.Lurker));
            group.Templates.Add(dl5);

            FTemplateGroups.Add(group);
        }

        private static void build_template_wolf_pack()
        {
            var group = new EncounterTemplateGroup("Wolf Pack", "Entire Party");

            var wp1 = new EncounterTemplate(Difficulty.Easy);
            wp1.Slots.Add(new EncounterTemplateSlot(7, -4, RoleType.Skirmisher));
            group.Templates.Add(wp1);

            var wp2 = new EncounterTemplate(Difficulty.Moderate);
            wp2.Slots.Add(new EncounterTemplateSlot(7, -2, RoleType.Skirmisher));
            group.Templates.Add(wp2);

            var wp3 = new EncounterTemplate(Difficulty.Moderate);
            wp3.Slots.Add(new EncounterTemplateSlot(5, 0, RoleType.Skirmisher));
            group.Templates.Add(wp3);

            var wp4 = new EncounterTemplate(Difficulty.Hard);
            wp4.Slots.Add(new EncounterTemplateSlot(3, 5, RoleType.Skirmisher));
            group.Templates.Add(wp4);

            var wp5 = new EncounterTemplate(Difficulty.Hard);
            wp5.Slots.Add(new EncounterTemplateSlot(4, 5, RoleType.Skirmisher));
            group.Templates.Add(wp5);

            var wp6 = new EncounterTemplate(Difficulty.Hard);
            wp6.Slots.Add(new EncounterTemplateSlot(6, 2, RoleType.Skirmisher));
            group.Templates.Add(wp6);

            FTemplateGroups.Add(group);
        }

        private static void build_template_duel()
        {
            var groupController = new EncounterTemplateGroup("Duel vs Controller", "Individual PC");

            var c1 = new EncounterTemplate(Difficulty.Easy);
            c1.Slots.Add(new EncounterTemplateSlot(1, 0, RoleType.Artillery));
            groupController.Templates.Add(c1);

            var c2 = new EncounterTemplate(Difficulty.Easy);
            c2.Slots.Add(new EncounterTemplateSlot(1, -1, new[] { RoleType.Controller, RoleType.Skirmisher }));
            groupController.Templates.Add(c2);

            var c3 = new EncounterTemplate(Difficulty.Moderate);
            c3.Slots.Add(new EncounterTemplateSlot(1, 2, RoleType.Artillery));
            groupController.Templates.Add(c3);

            var c4 = new EncounterTemplate(Difficulty.Moderate);
            c4.Slots.Add(new EncounterTemplateSlot(1, 1, new[] { RoleType.Controller, RoleType.Skirmisher }));
            groupController.Templates.Add(c4);

            var c5 = new EncounterTemplate(Difficulty.Hard);
            c5.Slots.Add(new EncounterTemplateSlot(1, 4, RoleType.Artillery));
            groupController.Templates.Add(c5);

            var c6 = new EncounterTemplate(Difficulty.Hard);
            c6.Slots.Add(new EncounterTemplateSlot(1, 3, new[] { RoleType.Controller, RoleType.Skirmisher }));
            groupController.Templates.Add(c6);

            FTemplateGroups.Add(groupController);

            var groupDefender = new EncounterTemplateGroup("Duel vs Defender", "Individual PC");

            var d1 = new EncounterTemplate(Difficulty.Easy);
            d1.Slots.Add(new EncounterTemplateSlot(1, 0, RoleType.Skirmisher));
            groupDefender.Templates.Add(d1);

            var d2 = new EncounterTemplate(Difficulty.Easy);
            d2.Slots.Add(new EncounterTemplateSlot(1, -1, new[] { RoleType.Brute, RoleType.Soldier }));
            groupDefender.Templates.Add(d2);

            var d3 = new EncounterTemplate(Difficulty.Moderate);
            d3.Slots.Add(new EncounterTemplateSlot(1, 2, RoleType.Skirmisher));
            groupDefender.Templates.Add(d3);

            var d4 = new EncounterTemplate(Difficulty.Moderate);
            d4.Slots.Add(new EncounterTemplateSlot(1, 1, new[] { RoleType.Brute, RoleType.Soldier }));
            groupDefender.Templates.Add(d4);

            var d5 = new EncounterTemplate(Difficulty.Hard);
            d5.Slots.Add(new EncounterTemplateSlot(1, 4, RoleType.Skirmisher));
            groupDefender.Templates.Add(d5);

            var d6 = new EncounterTemplate(Difficulty.Hard);
            d6.Slots.Add(new EncounterTemplateSlot(1, 3, new[] { RoleType.Controller, RoleType.Skirmisher }));
            groupDefender.Templates.Add(d6);

            FTemplateGroups.Add(groupDefender);

            var groupLeader = new EncounterTemplateGroup("Duel vs Leader", "Individual PC");

            var l1 = new EncounterTemplate(Difficulty.Easy);
            l1.Slots.Add(new EncounterTemplateSlot(1, 0, RoleType.Skirmisher));
            groupLeader.Templates.Add(l1);

            var l2 = new EncounterTemplate(Difficulty.Easy);
            l2.Slots.Add(new EncounterTemplateSlot(1, -1, new[] { RoleType.Controller, RoleType.Soldier }));
            groupLeader.Templates.Add(l2);

            var l3 = new EncounterTemplate(Difficulty.Moderate);
            l3.Slots.Add(new EncounterTemplateSlot(1, 2, RoleType.Skirmisher));
            groupLeader.Templates.Add(l3);

            var l4 = new EncounterTemplate(Difficulty.Moderate);
            l4.Slots.Add(new EncounterTemplateSlot(1, 1, new[] { RoleType.Controller, RoleType.Soldier }));
            groupLeader.Templates.Add(l4);

            var l5 = new EncounterTemplate(Difficulty.Hard);
            l5.Slots.Add(new EncounterTemplateSlot(1, 4, RoleType.Skirmisher));
            groupLeader.Templates.Add(l5);

            var l6 = new EncounterTemplate(Difficulty.Hard);
            l6.Slots.Add(new EncounterTemplateSlot(1, 3, new[] { RoleType.Controller, RoleType.Soldier }));
            groupLeader.Templates.Add(l6);

            FTemplateGroups.Add(groupLeader);

            var groupStriker = new EncounterTemplateGroup("Duel vs Striker", "Individual PC");

            var s1 = new EncounterTemplate(Difficulty.Easy);
            s1.Slots.Add(new EncounterTemplateSlot(1, 0, RoleType.Skirmisher));
            groupStriker.Templates.Add(s1);

            var s2 = new EncounterTemplate(Difficulty.Easy);
            s2.Slots.Add(new EncounterTemplateSlot(1, -1, new[] { RoleType.Brute, RoleType.Soldier }));
            groupStriker.Templates.Add(s2);

            var s3 = new EncounterTemplate(Difficulty.Moderate);
            s3.Slots.Add(new EncounterTemplateSlot(1, 2, RoleType.Skirmisher));
            groupStriker.Templates.Add(s3);

            var s4 = new EncounterTemplate(Difficulty.Moderate);
            s4.Slots.Add(new EncounterTemplateSlot(1, 1, new[] { RoleType.Brute, RoleType.Soldier }));
            groupStriker.Templates.Add(s4);

            var s5 = new EncounterTemplate(Difficulty.Hard);
            s5.Slots.Add(new EncounterTemplateSlot(1, 4, RoleType.Skirmisher));
            groupStriker.Templates.Add(s5);

            var s6 = new EncounterTemplate(Difficulty.Hard);
            s6.Slots.Add(new EncounterTemplateSlot(1, 3, new[] { RoleType.Brute, RoleType.Soldier }));
            groupStriker.Templates.Add(s6);

            FTemplateGroups.Add(groupStriker);
        }

        private static void build_template_grand_melee()
        {
            var group = new EncounterTemplateGroup("Grand Melee", "Entire Party");

            var gm1 = new EncounterTemplate(Difficulty.Easy);
            gm1.Slots.Add(new EncounterTemplateSlot(4, -2, RoleType.Brute));
            gm1.Slots.Add(new EncounterTemplateSlot(11, -4, true));
            group.Templates.Add(gm1);

            var gm2 = new EncounterTemplate(Difficulty.Moderate);
            gm2.Slots.Add(new EncounterTemplateSlot(2, -1, RoleType.Soldier));
            gm2.Slots.Add(new EncounterTemplateSlot(4, -2, RoleType.Brute));
            gm2.Slots.Add(new EncounterTemplateSlot(12, -4, true));
            group.Templates.Add(gm2);

            var gm3 = new EncounterTemplate(Difficulty.Hard);
            gm3.Slots.Add(new EncounterTemplateSlot(2, 0, RoleType.Soldier));
            gm3.Slots.Add(new EncounterTemplateSlot(4, -1, RoleType.Brute));
            gm3.Slots.Add(new EncounterTemplateSlot(17, -2, true));
            group.Templates.Add(gm3);

            FTemplateGroups.Add(group);
        }

        private static void build_creature_list(int minLevel, int maxLevel, List<string> categories,
            List<string> keywords, bool includeTemplates)
        {
            FCreatures.Clear();

            var creatures = Session.Creatures;
            foreach (var creature in creatures)
            {
                if (creature == null)
                    continue;

                if (minLevel != -1)
                    if (creature.Level < minLevel)
                        continue;

                if (maxLevel != -1)
                    if (creature.Level > maxLevel)
                        continue;

                if (categories != null && categories.Count != 0 && !categories.Contains(creature.Category))
                    continue;

                if (keywords != null && keywords.Count != 0)
                {
                    var matched = false;
                    foreach (var keyword in keywords)
                        if (creature.Phenotype.ToLower().Contains(keyword.ToLower()))
                        {
                            matched = true;
                            break;
                        }

                    if (!matched)
                        continue;
                }

                var card = new EncounterCard();
                card.CreatureId = creature.Id;

                FCreatures.Add(card);

                if (includeTemplates)
                    add_templates(creature);
            }

            foreach (var cc in Session.Project.CustomCreatures)
            {
                var card = new EncounterCard();
                card.CreatureId = cc.Id;

                FCreatures.Add(card);

                if (includeTemplates)
                    add_templates(cc);
            }
        }

        private static void add_templates(ICreature creature)
        {
            // Can't add a template to a minion
            if (creature.Role is Minion)
                return;

            // Can't add a template to a solo
            var role = creature.Role as ComplexRole;
            if (role.Flag == RoleFlag.Solo)
                return;

            foreach (var lib in Session.Libraries)
            foreach (var tmpl in lib.Templates)
            {
                var card = new EncounterCard();
                card.CreatureId = creature.Id;
                card.TemplateIDs.Add(tmpl.Id);
            }
        }

        private static void build_trap_list(int level)
        {
            var minLevel = level - 3;
            var maxLevel = level + 5;

            FTraps.Clear();
            foreach (var trap in Session.Traps)
            {
                if (trap.Level < minLevel || trap.Level > maxLevel)
                    continue;

                FTraps.Add(trap);
            }
        }

        private static void build_challenge_list(int level)
        {
            var minLevel = level - 3;
            var maxLevel = level + 5;

            FChallenges.Clear();
            foreach (var sc in Session.SkillChallenges)
            {
                if (sc.Level < minLevel || sc.Level > maxLevel)
                    continue;

                FChallenges.Add(sc);
            }
        }
    }
}
