using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using Masterplan.Data;
using Masterplan.Properties;
using Masterplan.Tools.Generators;
using Masterplan.UI;

namespace Masterplan.Tools
{
    /// <summary>
    ///     Text size.
    /// </summary>
    public enum DisplaySize
    {
        /// <summary>
        ///     Small text.
        /// </summary>
        Small,

        /// <summary>
        ///     Medium text.
        /// </summary>
        Medium,

        /// <summary>
        ///     Large text.
        /// </summary>
        Large,

        /// <summary>
        ///     Extra large text.
        /// </summary>
        ExtraLarge
    }

    internal class Html
    {
        private static readonly Dictionary<DisplaySize, List<string>> FStyles =
            new Dictionary<DisplaySize, List<string>>();

        private readonly Dictionary<Guid, List<Guid>> _fMaps = new Dictionary<Guid, List<Guid>>();

        private readonly List<Pair<string, Plot>> _fPlots = new List<Pair<string, Plot>>();
        private string _fFullPath = "";

        private string _fRelativePath = "";

        public static string Text(string str, bool stripHtml, bool centred, DisplaySize size)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(null, null, size));

            lines.Add("<BODY style=\"background-color=black\">");

            var details = Process(str, stripHtml);
            if (details != "")
            {
                if (centred)
                    lines.Add("<P class=instruction style=\"color=white\">" + details + "</P>");
                else
                    lines.Add("<P style=\"color=white\">" + details + "</P>");
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string StatBlock(EncounterCard card, CombatData data, Encounter enc, bool includeWrapper,
            bool initiativeHolder, bool full, CardMode mode, DisplaySize size)
        {
            var lines = new List<string>();

            if (includeWrapper)
            {
                lines.Add("<HTML>");
                lines.AddRange(GetStyle(size));
                lines.Add("<BODY>");
            }

            if (full)
            {
                if (data != null && data.Location == CombatData.NoPoint && enc != null && enc.MapId != Guid.Empty)
                    lines.Add("<P class=instruction>Drag this creature from the list onto the map.</P>");

                if (data != null)
                    lines.AddRange(get_combat_data(data, card.Hp, enc, initiativeHolder));
            }

            if (card != null)
            {
                lines.Add("<P class=table>");
                lines.AddRange(card.AsText(data, mode, full));
                lines.Add("</P>");
            }
            else
            {
                lines.Add("<P class=instruction>(no creature selected)</P>");
            }

            if (includeWrapper)
            {
                lines.Add("</BODY>");
                lines.Add("</HTML>");
            }

            return Concatenate(lines);
        }

        public static string StatBlock(Hero hero, Encounter enc, bool includeWrapper,
            bool initiativeHolder, bool showEffects, DisplaySize size)
        {
            var lines = new List<string>();

            if (includeWrapper)
            {
                lines.Add("<HTML>");
                lines.AddRange(GetStyle(size));
                lines.Add("<BODY>");
            }

            if (enc != null)
            {
                if (enc.MapId == Guid.Empty && hero.CombatData.Initiative == int.MinValue)
                    lines.Add(
                        "<P class=instruction>Double-click this character on the list to set its initiative score.</P>");
                else if (enc.MapId != Guid.Empty && hero.CombatData.Location == CombatData.NoPoint)
                    lines.Add("<P class=instruction>Drag this character from the list onto the map.</P>");
            }

            lines.AddRange(get_hero(hero, enc, initiativeHolder, showEffects));

            if (includeWrapper)
            {
                lines.Add("</BODY>");
                lines.Add("</HTML>");
            }

            return Concatenate(lines);
        }

        public static string CustomMapToken(CustomToken ct, bool drag, bool includeWrapper, DisplaySize size)
        {
            var lines = new List<string>();

            if (includeWrapper)
            {
                lines.Add("<HTML>");
                lines.AddRange(GetStyle(size));
                lines.Add("<BODY>");
            }

            if (drag)
                lines.Add("<P class=instruction>Drag this item from the list onto the map.</P>");

            lines.AddRange(get_custom_token(ct));

            if (includeWrapper)
            {
                lines.Add("</BODY>");
                lines.Add("</HTML>");
            }

            return Concatenate(lines);
        }

        public static string Trap(Trap trap, CombatData cd, bool includeWrapper, bool initiativeHolder,
            bool builder, DisplaySize size)
        {
            var lines = new List<string>();

            if (includeWrapper)
            {
                lines.Add("<HTML>");
                lines.AddRange(GetStyle(size));
                lines.Add("<BODY>");
            }

            if (trap != null)
                lines.AddRange(get_trap(trap, cd, initiativeHolder, builder));
            else
                lines.Add("<P class=instruction>(no trap / hazard selected)</P>");

            if (includeWrapper)
            {
                lines.Add("</BODY>");
                lines.Add("</HTML>");
            }

            return Concatenate(lines);
        }

        public static string SkillChallenge(SkillChallenge challenge, bool includeLinks, bool includeWrapper,
            DisplaySize size)
        {
            var lines = new List<string>();

            if (includeWrapper)
            {
                lines.Add("<HTML>");
                lines.AddRange(GetStyle(size));
                lines.Add("<BODY>");
            }

            if (challenge != null)
                lines.AddRange(get_skill_challenge(challenge, includeLinks));
            else
                lines.Add("<P class=instruction>(no skill challenge selected)</P>");

            if (includeWrapper)
            {
                lines.Add("</BODY>");
                lines.Add("</HTML>");
            }

            return Concatenate(lines);
        }

        public static string CreatureTemplate(CreatureTemplate template, DisplaySize size, bool builder)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(null, null, size));
            lines.Add("<BODY>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=template>");

            lines.Add("<TD colspan=2>");
            lines.Add("<B>" + Process(template.Name, true) + "</B>");
            lines.Add("</TD>");

            lines.Add("<TD>");
            lines.Add("<B>" + Process(template.Info, true) + "</B>");
            lines.Add("</TD>");

            if (builder)
            {
                lines.Add("<TR class=template>");
                lines.Add("<TD colspan=3 align=center>");
                lines.Add("<A href=build:profile style=\"color:white\">(click here to edit this top section)</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("</TR>");

            var initStr = template.Initiative.ToString();
            if (template.Initiative >= 0)
                initStr = "+" + initStr;

            if (builder)
                initStr = "<A href=build:combat>" + initStr + "</A>";

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Initiative</B> " + initStr);
            lines.Add("</TD>");
            lines.Add("</TR>");

            var move = Process(template.Movement, true);
            if (move != "" || builder)
            {
                if (builder)
                {
                    if (move == "")
                        move = "no additional movement modes";

                    move = "<A href=build:movement>" + move + "</A>";
                }

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Movement</B> " + move);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (template.Senses != "" || builder)
            {
                var rows = 2;

                // Add 1 row if there are damage mods
                if (template.Resist != "" || template.Vulnerable != "" || template.Immune != "" ||
                    template.DamageModifierTemplates.Count != 0)
                    rows += 1;

                var senses = Process(template.Senses, true);

                if (builder)
                {
                    if (senses == "")
                        senses = "no additional senses";

                    senses = "<A href=build:senses>" + senses + "</A>";
                }

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Senses</B> " + senses);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var defences = "";
            if (template.Ac != 0)
            {
                var sign = template.Ac > 0 ? "+" : "";
                defences += sign + template.Ac + " AC";
            }

            if (template.Fortitude != 0)
            {
                if (defences != "")
                    defences += "; ";

                var sign = template.Fortitude > 0 ? "+" : "";
                defences += sign + template.Fortitude + " Fort";
            }

            if (template.Reflex != 0)
            {
                if (defences != "")
                    defences += "; ";

                var sign = template.Reflex > 0 ? "+" : "";
                defences += sign + template.Reflex + " Ref";
            }

            if (template.Will != 0)
            {
                if (defences != "")
                    defences += "; ";

                var sign = template.Will > 0 ? "+" : "";
                defences += sign + template.Will + " Will";
            }

            if (defences != "" || builder)
            {
                if (builder)
                {
                    if (defences == "")
                        defences = "no defence bonuses";

                    defences = "<A href=build:combat>" + defences + "</A>";
                }

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Defences</B> " + defences);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var resist = Process(template.Resist, true);
            var vuln = Process(template.Vulnerable, true);
            var immune = Process(template.Immune, true);
            if (resist == null)
                resist = "";
            if (vuln == null)
                vuln = "";
            if (immune == null)
                immune = "";

            foreach (var dm in template.DamageModifierTemplates)
            {
                var total = dm.HeroicValue + dm.ParagonValue + dm.EpicValue;

                // Immunity
                if (total == 0)
                {
                    if (immune != "")
                        immune += ", ";

                    immune += dm.Type.ToString().ToLower();
                }

                // Vulnerability
                if (total > 0)
                {
                    if (vuln != "")
                        vuln += ", ";

                    vuln += dm.HeroicValue + "/" + dm.ParagonValue + "/" + dm.EpicValue + " " +
                            dm.Type.ToString().ToLower();
                }

                // Resistance
                if (total < 0)
                {
                    if (resist != "")
                        resist += ", ";

                    var val1 = Math.Abs(dm.HeroicValue);
                    var val2 = Math.Abs(dm.ParagonValue);
                    var val3 = Math.Abs(dm.EpicValue);
                    resist += val1 + "/" + val2 + "/" + val3 + " " + dm.Type.ToString().ToLower();
                }
            }

            var damage = "";
            if (immune != "")
            {
                if (damage != "")
                    damage += " ";

                damage += "<B>Immune</B> " + immune;
            }

            if (resist != "")
            {
                if (damage != "")
                    damage += " ";

                damage += "<B>Resist</B> " + resist;
            }

            if (vuln != "")
            {
                if (damage != "")
                    damage += " ";

                damage += "<B>Vulnerable</B> " + vuln;
            }

            if (damage != "" || builder)
            {
                if (builder)
                {
                    if (damage == "")
                        damage = "Set resistances / vulnerabilities / immunities";

                    damage = "<A href=build:damage>" + damage + "</A>";
                }

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add(damage);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Saving Throws</B> +2");
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Action Point</B> 1");
            lines.Add("</TD>");
            lines.Add("</TR>");

            var hp = "+" + template.Hp + " per level + Constitution score";
            if (builder)
                hp = "<A href=build:combat>" + hp + "</A>";

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>HP</B> " + hp);
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (builder)
            {
                lines.Add("<TR class=template>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Powers and Traits</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD colspan=3 align=center>");
                lines.Add("<A href=power:addtrait>add a trait</A>");
                lines.Add("|");
                lines.Add("<A href=power:addpower>add a power</A>");
                lines.Add("|");
                lines.Add("<A href=power:addaura>add an aura</A>");
                if (template.Regeneration == null)
                {
                    lines.Add("|");
                    lines.Add("<A href=power:regenedit>add regeneration</A>");
                }

                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var powers = new Dictionary<CreaturePowerCategory, List<CreaturePower>>();

            var powerCategories = Enum.GetValues(typeof(CreaturePowerCategory));
            foreach (CreaturePowerCategory cat in powerCategories)
                powers[cat] = new List<CreaturePower>();

            foreach (var cp in template.CreaturePowers)
                powers[cp.Category].Add(cp);

            foreach (CreaturePowerCategory cat in powerCategories)
            {
                var count = powers[cat].Count;
                if (cat == CreaturePowerCategory.Trait)
                {
                    // Add auras
                    count += template.Auras.Count;

                    // Add regeneration
                    var regen = template.Regeneration;
                    if (regen != null)
                        count += 1;
                }

                if (count == 0)
                    continue;

                var name = "";
                switch (cat)
                {
                    case CreaturePowerCategory.Trait:
                        name = "Traits";
                        break;
                    case CreaturePowerCategory.Standard:
                    case CreaturePowerCategory.Move:
                    case CreaturePowerCategory.Minor:
                    case CreaturePowerCategory.Free:
                        name = cat + " Actions";
                        break;
                    case CreaturePowerCategory.Triggered:
                        name = "Triggered Actions";
                        break;
                    case CreaturePowerCategory.Other:
                        name = "Other Actions";
                        break;
                }

                lines.Add("<TR class=creature>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>" + name + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (cat == CreaturePowerCategory.Trait)
                {
                    // Auras
                    foreach (var aura in template.Auras)
                    {
                        var auraDetails = Process(aura.Details.Trim(), true);
                        if (!auraDetails.StartsWith("aura", StringComparison.OrdinalIgnoreCase))
                            auraDetails = "Aura " + auraDetails;
                        else
                            auraDetails = "A" + auraDetails.Substring(1);

                        var ms = new MemoryStream();
                        Resources.Aura.Save(ms, ImageFormat.Png);
                        var byteImage = ms.ToArray();
                        var data = Convert.ToBase64String(byteImage);

                        lines.Add("<TR class=shaded>");
                        lines.Add("<TD colspan=3>");
                        lines.Add("<img src=data:image/png;base64," + data + ">");
                        lines.Add("<B>" + Process(aura.Name, true) + "</B>");
                        if (aura.Keywords != "")
                            lines.Add("(" + aura.Keywords + ")");
                        if (aura.Radius > 0)
                            lines.Add(" &diams; Aura " + aura.Radius);
                        lines.Add("</TD>");
                        lines.Add("</TR>");

                        lines.Add("<TR>");
                        lines.Add("<TD colspan=3>");
                        lines.Add(auraDetails);
                        lines.Add("</TD>");
                        lines.Add("</TR>");

                        if (builder)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD colspan=3 align=center>");
                            lines.Add("<A href=auraedit:" + aura.Id + ">edit</A>");
                            lines.Add("|");
                            lines.Add("<A href=auraremove:" + aura.Id + ">remove</A>");
                            lines.Add("</TD>");
                            lines.Add("</TR>");
                        }
                    }

                    // Regeneration
                    if (template.Regeneration != null)
                    {
                        lines.Add("<TR class=shaded>");
                        lines.Add("<TD colspan=3>");
                        lines.Add("<B>Regeneration</B>");
                        lines.Add("</TD>");
                        lines.Add("</TR>");

                        lines.Add("<TR>");
                        lines.Add("<TD colspan=3>");
                        lines.Add("Regeneration " + Process(template.Regeneration.ToString(), true));
                        lines.Add("</TD>");
                        lines.Add("</TR>");

                        if (builder)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD colspan=3 align=center>");
                            lines.Add("<A href=power:regenedit>edit</A>");
                            lines.Add("|");
                            lines.Add("<A href=power:regenremove>remove</A>");
                            lines.Add("</TD>");
                            lines.Add("</TR>");
                        }
                    }
                }

                foreach (var cp in powers[cat])
                {
                    lines.AddRange(cp.AsHtml(null, CardMode.View, false));

                    if (builder)
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD colspan=3 align=center>");
                        lines.Add("<A href=\"poweredit:" + cp.Id + "\">edit this power</A>");
                        lines.Add("|");
                        lines.Add("<A href=\"powerremove:" + cp.Id + "\">remove this power</A>");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }
                }
            }

            if (template.Tactics != "" || builder)
            {
                var tactics = Process(template.Tactics, true);

                if (builder)
                {
                    if (tactics == "")
                        tactics = "no tactics specified";

                    tactics = "<A href=build:tactics>" + tactics + "</A>";
                }

                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Tactics</B> " + tactics);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var lib = Session.FindLibrary(template);
            if (lib != null && lib.Name != "")
                if (Session.Project == null || lib != Session.Project.Library)
                {
                    var reference = Process(lib.Name, true);

                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD colspan=3>");
                    lines.Add(reference);
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string MagicItem(MagicItem item, DisplaySize size, bool builder, bool wrapper)
        {
            var lines = new List<string>();

            if (wrapper)
            {
                lines.Add("<HTML>");
                lines.AddRange(GetHead(null, null, size));
            }

            lines.Add("<BODY>");

            if (item != null)
                lines.AddRange(get_magic_item(item, builder));
            else
                lines.Add("<P class=instruction>(no item selected)</P>");

            if (wrapper)
            {
                lines.Add("</BODY>");
                lines.Add("</HTML>");
            }

            return Concatenate(lines);
        }

        public static string Artifact(Artifact artifact, DisplaySize size, bool builder, bool wrapper)
        {
            var lines = new List<string>();

            if (wrapper)
            {
                lines.Add("<HTML>");
                lines.AddRange(GetHead(null, null, size));
            }

            lines.Add("<BODY>");

            if (artifact != null)
                lines.AddRange(get_artifact(artifact, builder));
            else
                lines.Add("<P class=instruction>(no item selected)</P>");

            if (wrapper)
            {
                lines.Add("</BODY>");
                lines.Add("</HTML>");
            }

            return Concatenate(lines);
        }

        public static string PlotPoint(PlotPoint pp, Plot plot, int partyLevel, bool links, MainForm.ViewType view,
            DisplaySize size)
        {
            if (Session.Project == null)
                return null;

            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(null, null, size));
            lines.Add("<BODY>");

            if (pp != null)
            {
                lines.Add("<H3>" + Process(pp.Name, true) + "</H3>");

                switch (pp.State)
                {
                    case PlotPointState.Completed:
                        lines.Add("<P class=instruction>(completed)</P>");
                        break;
                    case PlotPointState.Skipped:
                        lines.Add("<P class=instruction>(skipped)</P>");
                        break;
                }

                if (links)
                {
                    var options = new List<string>();

                    if (view == MainForm.ViewType.Flowchart)
                        options.Add("<A href=\"plot:edit\">Open</A> this plot point.");

                    if (pp.Element == null)
                    {
                        options.Add("Turn this point into a <A href=plot:encounter>combat encounter</A>.");
                        options.Add("Turn this point into a <A href=plot:challenge>skill challenge</A>.");
                    }

                    if (pp.Subplot.Points.Count != 0)
                        options.Add("This plot point has a <A href=\"plot:explore\">subplot</A>.");
                    else
                        options.Add("Create a <A href=\"plot:explore\">subplot</A> for this point.");

                    var enc = pp.Element as Encounter;
                    if (enc != null)
                        options.Add(
                            "This plot point contains an <A href=plot:element>encounter</A> (<A href=plot:run>run it</a>).");

                    var sc = pp.Element as SkillChallenge;
                    if (sc != null) options.Add("This plot point contains a <A href=plot:element>skill challenge</A>.");

                    var te = pp.Element as TrapElement;
                    if (te != null)
                    {
                        var type = te.Trap.Type == TrapType.Trap ? "trap" : "hazard";
                        options.Add("This plot point contains a <A href=plot:element>" + type + "</A>.");
                    }

                    Map map = null;
                    MapArea mapArea = null;
                    pp.GetTacticalMapArea(ref map, ref mapArea);
                    if (map != null && mapArea != null)
                    {
                        var name = Process(mapArea.Name, true);
                        options.Add("This plot point occurs in <A href=plot:maparea>" + name + "</A>.");
                    }

                    RegionalMap rmap = null;
                    MapLocation loc = null;
                    pp.GetRegionalMapArea(ref rmap, ref loc, Session.Project);
                    if (rmap != null && loc != null)
                    {
                        var name = Process(loc.Name, true);
                        options.Add("This plot point occurs at <A href=plot:maploc>" + name + "</A>.");
                    }

                    if (options.Count != 0)
                    {
                        lines.Add("<P class=table>");
                        lines.Add("<TABLE>");

                        lines.Add("<TR class=heading>");
                        lines.Add("<TD><B>Options</B></TD>");
                        lines.Add("</TR>");

                        for (var index = 0; index != options.Count; ++index)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD>");
                            lines.Add(options[index]);
                            lines.Add("</TD>");
                            lines.Add("</TR>");
                        }

                        lines.Add("</TABLE>");
                        lines.Add("</P>");
                    }
                }

                var readaloud = Process(pp.ReadAloud, false);
                if (readaloud != "")
                {
                    readaloud = readaloud.Replace("<p>", "<p class=readaloud>");
                    lines.Add(readaloud);
                }

                var details = Process(pp.Details, false);
                if (details != "") lines.Add(details);

                if (partyLevel != Session.Project.Party.Level)
                    lines.Add("<P><B>Party level</B>: " + partyLevel + "</P>");

                if (pp.Date != null)
                    lines.Add("<P><B>Date</B>: " + pp.Date + "</P>");

                lines.AddRange(get_map_area_details(pp));

                if (links)
                {
                    var entrySet = new BinarySearchTree<EncyclopediaEntry>();
                    foreach (var entryId in pp.EncyclopediaEntryIDs)
                    {
                        var entry = Session.Project.Encyclopedia.FindEntry(entryId);
                        if (entry != null)
                            entrySet.Add(entry);
                    }

                    if (pp.MapLocationId != Guid.Empty)
                    {
                        var entry = Session.Project.Encyclopedia.FindEntryForAttachment(pp.MapLocationId);
                        if (entry != null)
                            entrySet.Add(entry);
                    }

                    if (pp.Element is Encounter)
                    {
                        var enc = pp.Element as Encounter;

                        foreach (var npc in Session.Project.NpCs)
                        {
                            var entry = Session.Project.Encyclopedia.FindEntryForAttachment(npc.Id);
                            if (entry == null)
                                continue;

                            if (enc.Contains(npc.Id))
                                entrySet.Add(entry);
                        }
                    }

                    var entries = entrySet.SortedList;
                    if (entries.Count != 0)
                    {
                        lines.Add("<P><B>See also</B>:</P>");
                        lines.Add("<UL>");

                        foreach (var entry in entries)
                            lines.Add("<LI><A href=\"entry:" + entry.Id + "\">" + entry.Name + "</A></LI>");

                        lines.Add("</UL>");
                    }
                }

                if (pp.Element != null)
                {
                    var enc = pp.Element as Encounter;
                    if (enc != null)
                        lines.AddRange(get_encounter(enc));

                    var te = pp.Element as TrapElement;
                    if (te != null)
                        lines.AddRange(get_trap(te.Trap, null, false, false));

                    var sc = pp.Element as SkillChallenge;
                    if (sc != null)
                        lines.AddRange(get_skill_challenge(sc, links));

                    var q = pp.Element as Quest;
                    if (q != null)
                        lines.AddRange(get_quest(q));
                }

                if (pp.Parcels.Count != 0)
                    lines.AddRange(get_parcels(pp, links));
            }
            else
            {
                var parentPoint = Session.Project.FindParent(plot);

                var heading = parentPoint != null ? parentPoint.Name : Session.Project.Name;
                lines.Add("<H2>" + Process(heading, true) + "</H2>");

                if (parentPoint != null)
                {
                    if (parentPoint.Date != null)
                        lines.Add("<P>" + parentPoint.Date + "</P>");

                    if (parentPoint.Details != "")
                        lines.Add("<P>" + Process(parentPoint.Details, false) + "</P>");
                }
                else
                {
                    if (Session.Project.Author != "")
                        lines.Add("<P class=instruction>by " + Session.Project.Author + "</P>");

                    var count = Session.Project.Party.Size;
                    var level = Session.Project.Party.Level;
                    var startXp = Session.Project.Party.Xp;
                    var normalXp = Experience.GetHeroXp(level);

                    var str = "<B>" + Process(Session.Project.Name, true) + "</B> is designed for a party of " + count +
                              " characters at level " + level;
                    if (startXp != normalXp)
                        str += ", starting with " + startXp + " XP";
                    str += ".";

                    lines.Add("<P>" + str + "</P>");
                }

                var xp = 0;
                var layers = Workspace.FindLayers(plot);
                foreach (var layer in layers)
                    xp += Workspace.GetLayerXp(layer);

                if (xp != 0)
                {
                    var xpStr = "XP available: " + xp + ".";

                    if (plot == Session.Project.Plot)
                    {
                        var levelInitial = Session.Project.Party.Level;
                        var xpCurrent = Experience.GetHeroXp(levelInitial);
                        var xpFinal = xpCurrent + xp / Session.Project.Party.Size;
                        var levelFinal = Experience.GetHeroLevel(xpFinal);

                        if (levelFinal != -1 && levelFinal != levelInitial)
                        {
                            xpStr += "<BR>";
                            xpStr += "The party will reach level " + levelFinal + ".";
                        }
                    }

                    lines.Add("<P>" + xpStr + "</P>");
                }

                if (links)
                {
                    lines.Add("<P class=table>");
                    lines.Add("<TABLE>");

                    lines.Add("<TR class=heading>");
                    lines.Add("<TD><B>Options</B></TD>");
                    lines.Add("</TR>");

                    if (view == MainForm.ViewType.Flowchart)
                    {
                        if (plot.Points.Count == 0)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD>This plot is empty:</TD>");
                            lines.Add("</TR>");

                            lines.Add("<TR>");
                            lines.Add("<TD class=indent>Add a <A href=\"plot:add\">plot point</A>.</TD>");
                            lines.Add("</TR>");

                            lines.Add("<TR>");
                            lines.Add("<TD class=indent>Add a <A href=\"plot:encounter\">combat encounter</A>.</TD>");
                            lines.Add("</TR>");

                            lines.Add("<TR>");
                            lines.Add("<TD class=indent>Add a <A href=\"plot:challenge\">skill challenge</A>.</TD>");
                            lines.Add("</TR>");
                        }

                        if (parentPoint != null)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD>Move up <A href=\"plot:up\">one plot level</A>.</TD>");
                            lines.Add("</TR>");
                        }

                        var tacticalMapIds = plot.FindTacticalMaps();
                        if (tacticalMapIds.Count == 0)
                        {
                            if (Session.Project.Maps.Count == 0)
                            {
                                lines.Add("<TR>");
                                lines.Add(
                                    "<TD>Create a <A href=\"delveview:build\">tactical map</A> to use as the basis of this plot.</TD>");
                                lines.Add("</TR>");
                            }
                            else
                            {
                                lines.Add("<TR>");
                                lines.Add("<TD>Use a tactical map as the basis of this plot:</TD>");
                                lines.Add("</TR>");

                                lines.Add("<TR>");
                                lines.Add("<TD class=indent>Build a <A href=\"delveview:build\">new map</A>.</TD>");
                                lines.Add("</TR>");

                                lines.Add("<TR>");
                                lines.Add(
                                    "<TD class=indent>Select an <A href=\"delveview:select\">existing map</A>.</TD>");
                                lines.Add("</TR>");
                            }
                        }
                        else if (tacticalMapIds.Count == 1)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD>Switch to <A href=\"delveview:" + tacticalMapIds[0] +
                                      "\">delve view</A>.</TD>");
                            lines.Add("</TR>");
                        }
                        else
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD>Switch to delve view using one of the following maps:</TD>");
                            lines.Add("</TR>");

                            foreach (var mapId in tacticalMapIds)
                            {
                                if (mapId == Guid.Empty)
                                    continue;

                                var map = Session.Project.FindTacticalMap(mapId);
                                if (map == null)
                                    continue;

                                lines.Add("<TR>");
                                lines.Add("<TD class=indent><A href=\"delveview:" + mapId + "\">" +
                                          Process(map.Name, true) + "</A></TD>");
                                lines.Add("</TR>");
                            }

                            lines.Add("<TR>");
                            lines.Add(
                                "<TD class=indent><A href=\"delveview:select\">Select (or create) a map</A></TD>");
                            lines.Add("</TR>");
                        }

                        var regionalMapIds = plot.FindRegionalMaps();
                        if (regionalMapIds.Count == 0)
                        {
                            if (Session.Project.RegionalMaps.Count == 0)
                            {
                                lines.Add("<TR>");
                                lines.Add(
                                    "<TD>Create a <A href=\"mapview:build\">regional map</A> to use as the basis of this plot.</TD>");
                                lines.Add("</TR>");
                            }
                            else
                            {
                                lines.Add("<TR>");
                                lines.Add("<TD>Use a regional map as the basis of this plot:</TD>");
                                lines.Add("</TR>");

                                lines.Add("<TR>");
                                lines.Add("<TD class=indent>Build a <A href=\"mapview:build\">new map</A>.</TD>");
                                lines.Add("</TR>");

                                lines.Add("<TR>");
                                lines.Add(
                                    "<TD class=indent>Select an <A href=\"mapview:select\">existing map</A>.</TD>");
                                lines.Add("</TR>");
                            }
                        }
                        else if (regionalMapIds.Count == 1)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD>Switch to <A href=\"mapview:" + regionalMapIds[0] + "\">map view</A>.</TD>");
                            lines.Add("</TR>");
                        }
                        else
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD>Switch to map view using one of the following maps:</TD>");
                            lines.Add("</TR>");

                            foreach (var mapId in regionalMapIds)
                            {
                                if (mapId == Guid.Empty)
                                    continue;

                                var map = Session.Project.FindRegionalMap(mapId);
                                if (map == null)
                                    continue;

                                lines.Add("<TR>");
                                lines.Add("<TD class=indent><A href=\"mapview:" + mapId + "\">" +
                                          Process(map.Name, true) + "</A></TD>");
                                lines.Add("</TR>");
                            }

                            lines.Add("<TR>");
                            lines.Add("<TD class=indent><A href=\"mapview:select\">Select (or create) a map</A></TD>");
                            lines.Add("</TR>");
                        }

                        if (parentPoint == null)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD>Edit the <A href=\"plot:properties\">project properties</A>.</TD>");
                            lines.Add("</TR>");
                        }
                    }
                    else if (view == MainForm.ViewType.Delve)
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD>Switch to <A href=\"delveview:off\">flowchart view</A>.</TD>");
                        lines.Add("</TR>");

                        lines.Add("<TR>");
                        lines.Add("<TD><A href=\"delveview:edit\">Edit this map</A>.</TD>");
                        lines.Add("</TR>");

                        lines.Add("<TR>");
                        lines.Add("<TD>Send this map to the <A href=\"delveview:playerview\">player view</A>.</TD>");
                        lines.Add("</TR>");
                    }
                    else if (view == MainForm.ViewType.Map)
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD>Switch to <A href=\"mapview:off\">flowchart view</A>.</TD>");
                        lines.Add("</TR>");

                        lines.Add("<TR>");
                        lines.Add("<TD><A href=\"mapview:edit\">Edit this map</A>.</TD>");
                        lines.Add("</TR>");

                        lines.Add("<TR>");
                        lines.Add("<TD>Send this map to the <A href=\"mapview:playerview\">player view</A>.</TD>");
                        lines.Add("</TR>");
                    }

                    lines.Add("</TR>");
                    lines.Add("</TABLE>");
                    lines.Add("</P>");
                }
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string MapArea(MapArea area, DisplaySize size)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(null, null, size));
            lines.Add("<BODY>");

            if (area != null)
            {
                var name = Process(area.Name, true);
                lines.Add("<H3>" + name + "</H3>");

                if (area.Details != "")
                {
                    lines.Add("<P>");
                    lines.Add(Process(area.Details, true));
                    lines.Add("</P>");
                }

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD><B>Options</B></TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"maparea:edit\">View information</A> about this map area.");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"maparea:create\">Create a plot point</A> here.");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD class=indent>");
                lines.Add("... containing a <A href=\"maparea:encounter\">combat encounter</A>.");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD class=indent>");
                lines.Add("... containing a <A href=\"maparea:trap\">trap or hazard</A>.");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD class=indent>");
                lines.Add("... containing a <A href=\"maparea:challenge\">skill challenge</A>.");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            return Concatenate(lines);
        }

        public static string MapLocation(MapLocation loc, DisplaySize size)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(null, null, size));
            lines.Add("<BODY>");

            if (loc != null)
            {
                var name = Process(loc.Name, true);
                lines.Add("<H3>" + name + "</H3>");

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD><B>Options</B></TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"maploc:edit\">View information</A> about this map location.");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<A href=\"maploc:create\">Create a plot point</A> here.");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD class=indent>");
                lines.Add("... containing a <A href=\"maploc:encounter\">combat encounter</A>.");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD class=indent>");
                lines.Add("... containing a <A href=\"maploc:trap\">trap or hazard</A>.");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD class=indent>");
                lines.Add("... containing a <A href=\"maploc:challenge\">skill challenge</A>.");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            return Concatenate(lines);
        }

        public static string EncounterNote(EncounterNote en, DisplaySize size)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(null, null, size));

            lines.Add("<BODY>");
            if (en != null)
            {
                lines.Add("<H3>" + Process(en.Title, true) + "</H3>");

                var str = Process(en.Contents, false);
                if (str != "")
                {
                    str = str.Replace("<p>", "<p class=encounter_note>");
                    lines.Add(str);
                }
                else
                {
                    lines.Add("<P class=instruction>" + "This note has no contents." + "</P>");
                    lines.Add("<P class=instruction>" +
                              "Press <A href=\"note:edit\">Edit</A> to add information to this note." + "</P>");
                }
            }
            else
            {
                lines.Add("<P class=instruction>" + "(no note selected)" + "</P>");
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string Background(Background bg, DisplaySize size)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(null, null, size));

            lines.Add("<BODY>");
            if (bg != null)
            {
                var details = Process(bg.Details, false);
                if (details != "")
                {
                    details = details.Replace("<p>", "<p class=background>");
                    lines.Add(details);
                }
                else
                {
                    lines.Add("<P class=instruction>" +
                              "Press <A href=\"background:edit\">Edit</A> to add information to this item." + "</P>");
                }
            }
            else
            {
                lines.Add("<P class=instruction>" + "(no background selected)" + "</P>");
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string Background(List<Background> backgrounds, DisplaySize size)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(null, null, size));

            lines.Add("<BODY>");

            foreach (var bg in backgrounds)
            {
                var title = Process(bg.Title, false);
                var details = Process(bg.Details, false);

                if (title != "" && details != "")
                {
                    lines.Add("<H3>" + title + "</H3>");

                    details = details.Replace("<p>", "<p class=background>");
                    lines.Add(details);
                }
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string EncyclopediaEntry(EncyclopediaEntry entry, Encyclopedia encyclopedia, DisplaySize size,
            bool includeDmInfo, bool includeEntryLinks, bool includeAttachment, bool includePictureLinks)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(null, null, size));

            lines.Add("<BODY>");
            if (entry != null)
            {
                lines.Add("<H4>" + Process(entry.Name, true) + "</H4>");
                lines.Add("<HR>");
            }

            if (entry != null)
            {
                if (includeAttachment && entry.AttachmentId != Guid.Empty)
                {
                    MapLocation loc = null;
                    foreach (var map in Session.Project.RegionalMaps)
                    {
                        loc = map.FindLocation(entry.AttachmentId);
                        if (loc != null)
                            break;
                    }

                    if (loc != null)
                    {
                        lines.Add("<P class=instruction>" + "<A href=\"map:" + entry.AttachmentId +
                                  "\">View location on map</A>." + "</P>");
                        lines.Add("<HR>");
                    }
                }

                var details = process_encyclopedia_info(entry.Details, encyclopedia, includeEntryLinks, includeDmInfo);
                var dmInfo = process_encyclopedia_info(entry.DmInfo, encyclopedia, includeEntryLinks, includeDmInfo);

                if (details == "" && dmInfo == "")
                    lines.Add("<P class=instruction>" +
                              "Press <A href=\"entry:edit\">Edit</A> to add information to this entry." + "</P>");

                if (details != "")
                    lines.Add("<P class=encyclopedia_entry>" + Process(details, false) + "</P>");

                if (includeDmInfo && dmInfo != "")
                {
                    lines.Add("<H4>For DMs Only</H4>");
                    lines.Add("<P class=encyclopedia_entry>" + Process(dmInfo, false) + "</P>");
                }

                if (includePictureLinks)
                    if (entry.Images.Count != 0)
                    {
                        lines.Add("<H4>Pictures</H4>");
                        lines.Add("<UL>");

                        foreach (var img in entry.Images)
                        {
                            lines.Add("<LI>");
                            lines.Add("<A href=picture:" + img.Id + ">" + img.Name + "</A>");
                            lines.Add("</LI>");
                        }

                        lines.Add("</UL>");
                    }

                if (includeAttachment)
                    if (entry.AttachmentId != Guid.Empty)
                    {
                        // PC
                        var hero = Session.Project.FindHero(entry.AttachmentId);
                        if (hero != null) lines.AddRange(get_hero(hero, null, false, false));

                        // Creature or NPC
                        var c = Session.FindCreature(entry.AttachmentId, SearchType.Global);
                        if (c != null)
                        {
                            var card = new EncounterCard(c.Id);

                            lines.Add("<P class=table>");
                            lines.AddRange(card.AsText(null, CardMode.View, true));
                            lines.Add("</P>");
                        }

                        // Player option
                        var option = Session.Project.FindPlayerOption(entry.AttachmentId);
                        if (option != null) lines.AddRange(get_player_option(option));
                    }

                if (includeEntryLinks && encyclopedia != null)
                {
                    // Links
                    var links = new List<EncyclopediaLink>();
                    foreach (var link in encyclopedia.Links)
                        if (link.EntryIDs.Contains(entry.Id))
                            links.Add(link);

                    if (links.Count != 0)
                    {
                        lines.Add("<HR>");
                        lines.Add("<P><B>See also</B>:</P>");
                        lines.Add("<UL>");

                        foreach (var link in links)
                        foreach (var entryId in link.EntryIDs)
                        {
                            if (entryId == entry.Id)
                                continue;

                            var linkedEntry = encyclopedia.FindEntry(entryId);
                            lines.Add("<LI><A href=\"entry:" + entryId + "\">" + Process(linkedEntry.Name, true) +
                                      "</A></LI>");
                        }

                        lines.Add("</UL>");
                    }

                    // Groups
                    var groups = new List<EncyclopediaGroup>();
                    foreach (var group in encyclopedia.Groups)
                        if (@group.EntryIDs.Contains(entry.Id))
                            groups.Add(@group);

                    if (groups.Count != 0)
                    {
                        lines.Add("<HR>");
                        lines.Add("<P><B>Groups</B>:</P>");

                        foreach (var group in groups)
                        {
                            lines.Add("<P class=table>");
                            lines.Add("<TABLE>");

                            lines.Add("<TR class=shaded align=center>");
                            lines.Add("<TD>");
                            lines.Add("<B><A href=\"group:" + group.Id + "\">" + Process(group.Name, true) +
                                      "</A></B>");
                            lines.Add("</TD>");
                            lines.Add("</TR>");

                            lines.Add("<TR>");
                            lines.Add("<TD>");

                            var entries = new List<EncyclopediaEntry>();
                            foreach (var entryId in group.EntryIDs)
                            {
                                var ee = encyclopedia.FindEntry(entryId);
                                if (ee == null)
                                    continue;

                                entries.Add(ee);
                            }

                            entries.Sort();

                            foreach (var ee in entries)
                                if (ee != entry)
                                    lines.Add("<A href=\"entry:" + ee.Id + "\">" + Process(ee.Name, true) + "</A>");
                                else
                                    lines.Add("<B>" + Process(ee.Name, true) + "</B>");

                            lines.Add("</TD>");
                            lines.Add("</TR>");

                            lines.Add("</TABLE>");
                        }
                    }
                }
            }
            else
            {
                lines.Add("<P class=instruction>" + "(no entry selected)" + "</P>");
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string EncyclopediaGroup(EncyclopediaGroup group, Encyclopedia encyclopedia, DisplaySize size,
            bool includeDmInfo, bool includeEntryLinks)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(null, null, size));

            lines.Add("<BODY>");
            if (group != null)
            {
                if (encyclopedia != null)
                {
                    // Links
                    var links = new List<EncyclopediaEntry>();
                    foreach (var entryId in group.EntryIDs)
                    {
                        var entry = encyclopedia.FindEntry(entryId);
                        if (entry != null)
                            links.Add(entry);
                    }

                    if (links.Count != 0)
                        foreach (var entry in links)
                        {
                            lines.Add("<H3>" + Process(entry.Name, true) + "</H3>");

                            var str = process_encyclopedia_info(entry.Details, encyclopedia, includeEntryLinks,
                                includeDmInfo);
                            lines.Add("<P class=encyclopedia_entry>" + Process(str, false) + "</P>");
                        }
                    else
                        lines.Add("<P class=instruction>" + "(no entries in group)" + "</P>");
                }
            }
            else
            {
                lines.Add("<P class=instruction>" + "(no group selected)" + "</P>");
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string Handout(List<object> items, DisplaySize size, bool includeDmInfo)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(Session.Project.Name, "Handout", size));

            lines.Add("<BODY>");

            if (items.Count != 0)
                foreach (var item in items)
                {
                    if (item is Background)
                    {
                        var bg = item as Background;

                        var details = Process(bg.Details, false);
                        details = details.Replace("<p>", "<p class=background>");

                        lines.Add("<H3>" + Process(bg.Title, true) + "</H3>");
                        lines.Add(details);
                    }

                    if (item is EncyclopediaEntry)
                    {
                        var entry = item as EncyclopediaEntry;

                        lines.Add("<H3>" + Process(entry.Name, true) + "</H3>");
                        var details = process_encyclopedia_info(entry.Details, Session.Project.Encyclopedia, false,
                            includeDmInfo);
                        lines.Add("<P class=encyclopedia_entry>" + Process(details, false) + "</P>");

                        if (includeDmInfo && entry.DmInfo != "")
                        {
                            var dmInfo = process_encyclopedia_info(entry.DmInfo, Session.Project.Encyclopedia, false,
                                includeDmInfo);

                            lines.Add("<H4>For DMs Only</H4>");
                            lines.Add("<P class=encyclopedia_entry>" + Process(dmInfo, false) + "</P>");
                        }
                    }

                    if (item is IPlayerOption)
                    {
                        var option = item as IPlayerOption;

                        lines.Add("<H3>" + Process(option.Name, true) + "</H3>");
                        lines.AddRange(get_player_option(option));
                    }
                }
            else
                lines.Add("<P class=instruction>" + "(no items selected)" + "</P>");

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string PlayerOption(IPlayerOption option, DisplaySize size)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(null, null, size));

            lines.Add("<BODY>");

            if (option != null)
                lines.AddRange(get_player_option(option));
            else
                lines.Add("<P class=instruction>(no item selected)</P>");

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string PartyBreakdown(DisplaySize size)
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(GetHead("Party", null, size));

            lines.Add("<BODY>");
            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD colspan=2>");
            lines.Add("<B>Party</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR class=shaded>");
            lines.Add("<TD colspan=2>");
            lines.Add("<B>PCs</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            var breakdown = new Dictionary<HeroRoleType, int>();
            foreach (HeroRoleType role in Enum.GetValues(typeof(HeroRoleType)))
                breakdown[role] = 0;

            foreach (var hero in Session.Project.Heroes)
            {
                var nameStr = "<B>" + hero.Name + "</B>";
                if (hero.Player != "")
                    nameStr += " (" + hero.Player + ")";

                var raceClassStr = hero.Race;
                if (hero.Class != null && hero.Class != "")
                    raceClassStr += " " + hero.Class;
                if (hero.ParagonPath != null && hero.ParagonPath != "")
                    raceClassStr += " / " + hero.ParagonPath;
                if (hero.EpicDestiny != null && hero.EpicDestiny != "")
                    raceClassStr += " / " + hero.EpicDestiny;

                lines.Add("<TR>");

                lines.Add("<TD>");
                lines.Add(nameStr);
                lines.Add("</TD>");

                lines.Add("<TD>");
                lines.Add(raceClassStr);
                lines.Add("</TD>");

                lines.Add("</TR>");

                breakdown[hero.Role] += 1;
            }

            lines.Add("<TR class=shaded>");
            lines.Add("<TD colspan=2>");
            lines.Add("<B>Roles</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            foreach (var role in breakdown.Keys)
            {
                lines.Add("<TR>");

                lines.Add("<TD>");
                lines.Add("<B>" + role + "</B>");
                lines.Add("</TD>");

                lines.Add("<TD>");
                lines.Add(breakdown[role].ToString());
                lines.Add("</TD>");

                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");
            lines.Add("</BODY>");

            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string PCs(string secondary, DisplaySize size)
        {
            var lines = new List<string>();

            lines.AddRange(GetHead(null, null, size));
            lines.Add("<BODY>");

            if (Session.Project != null)
            {
                if (Session.Project.Heroes.Count == 0)
                {
                    lines.Add("<P class=instruction>");
                    lines.Add("No PC details have been entered; click <A href=\"party:edit\">here</A> to do this now.");
                    lines.Add("</P>");

                    lines.Add("<P class=instruction>");
                    lines.Add(
                        "When PCs have been entered, you will see a useful breakdown of their defences, passive skills and known languages here.");
                    lines.Add("</P>");
                }
                else
                {
                    var minAc = int.MaxValue;
                    var minFort = int.MaxValue;
                    var minRef = int.MaxValue;
                    var minWill = int.MaxValue;
                    var maxAc = int.MinValue;
                    var maxFort = int.MinValue;
                    var maxRef = int.MinValue;
                    var maxWill = int.MinValue;

                    var minPerc = int.MaxValue;
                    var minIns = int.MaxValue;
                    var maxPerc = int.MinValue;
                    var maxIns = int.MinValue;

                    var languageBst = new BinarySearchTree<string>();

                    foreach (var hero in Session.Project.Heroes)
                    {
                        minAc = Math.Min(minAc, hero.Ac);
                        minFort = Math.Min(minFort, hero.Fortitude);
                        minRef = Math.Min(minRef, hero.Reflex);
                        minWill = Math.Min(minWill, hero.Will);
                        maxAc = Math.Max(maxAc, hero.Ac);
                        maxFort = Math.Max(maxFort, hero.Fortitude);
                        maxRef = Math.Max(maxRef, hero.Reflex);
                        maxWill = Math.Max(maxWill, hero.Will);

                        minPerc = Math.Min(minPerc, hero.PassivePerception);
                        minIns = Math.Min(minIns, hero.PassiveInsight);
                        maxPerc = Math.Max(maxPerc, hero.PassivePerception);
                        maxIns = Math.Max(maxIns, hero.PassiveInsight);

                        var langs = hero.Languages;
                        langs = langs.Replace(".", "");
                        langs = langs.Replace(",", "");
                        langs = langs.Replace(";", "");
                        langs = langs.Replace(":", "");
                        langs = langs.Replace("/", "");

                        var tokens = langs.Split(null);
                        foreach (var token in tokens)
                            if (token != "")
                                languageBst.Add(token);
                    }

                    lines.Add("<P class=table>");
                    lines.Add("<TABLE class=clear>");
                    lines.Add("<TR class=clear>");
                    lines.Add("<TD class=clear>");

                    lines.Add("<P class=table>");
                    lines.Add("<TABLE>");

                    lines.Add("<TR class=heading>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("<B>Party Breakdown</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("<B>The Party</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    foreach (var hero in Session.Project.Heroes)
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD><A href=show:" + hero.Id + ">" + hero.Name + "</A></TD>");
                        lines.Add("<TD colspan=2>" + hero.Info + "</TD>");
                        lines.Add("</TR>");
                    }

                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("<B>Defences</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR>");
                    lines.Add("<TD><A href=show:ac>Armour Class</A></TD>");
                    lines.Add("<TD colspan=2>");
                    if (minAc == maxAc)
                        lines.Add(minAc.ToString());
                    else
                        lines.Add(minAc + " - " + maxAc);
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR>");
                    lines.Add("<TD><A href=show:fort>Fortitude</A></TD>");
                    lines.Add("<TD colspan=2>");
                    if (minFort == maxFort)
                        lines.Add(minFort.ToString());
                    else
                        lines.Add(minFort + " - " + maxFort);
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR>");
                    lines.Add("<TD><A href=show:ref>Reflex</A></TD>");
                    lines.Add("<TD colspan=2>");
                    if (minRef == maxRef)
                        lines.Add(minRef.ToString());
                    else
                        lines.Add(minRef + " - " + maxRef);
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR>");
                    lines.Add("<TD><A href=show:will>Will</A></TD>");
                    lines.Add("<TD colspan=2>");
                    if (minWill == maxWill)
                        lines.Add(minWill.ToString());
                    else
                        lines.Add(minWill + " - " + maxWill);
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("<B>Passive Skills</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR>");
                    lines.Add("<TD><A href=show:passiveinsight>Insight</A></TD>");
                    lines.Add("<TD colspan=2>");
                    if (minIns == maxIns)
                        lines.Add(minIns.ToString());
                    else
                        lines.Add(minIns + " - " + maxIns);
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR>");
                    lines.Add("<TD><A href=show:passiveperception>Perception</A></TD>");
                    lines.Add("<TD colspan=2>");
                    if (minPerc == maxPerc)
                        lines.Add(minPerc.ToString());
                    else
                        lines.Add(minPerc + " - " + maxPerc);
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    if (languageBst.Count != 0)
                    {
                        lines.Add("<TR class=shaded>");
                        lines.Add("<TD colspan=3>");
                        lines.Add("<B>Known Languages</B>");
                        lines.Add("</TD>");
                        lines.Add("</TR>");

                        var languages = languageBst.SortedList;
                        foreach (var language in languages)
                        {
                            var heroes = "";
                            foreach (var hero in Session.Project.Heroes)
                                if (hero.Languages.Contains(language))
                                {
                                    if (heroes != "")
                                        heroes += ", ";

                                    heroes += hero.Name;
                                }

                            lines.Add("<TR>");
                            lines.Add("<TD>" + language + "</TD>");
                            lines.Add("<TD colspan=2>" + heroes + "</TD>");
                            lines.Add("</TR>");
                        }
                    }

                    lines.Add("</TABLE>");
                    lines.Add("</P>");

                    lines.Add("</TD>");
                    lines.Add("<TD class=clear>");

                    if (secondary == "")
                    {
                        lines.Add("<P class=instruction>");
                        lines.Add("Click on a link to the right to show details here");
                        lines.Add("</P>");
                    }
                    else
                    {
                        var id = Guid.Empty;
                        try
                        {
                            id = new Guid(secondary);
                        }
                        catch
                        {
                            id = Guid.Empty;
                        }

                        if (id != Guid.Empty)
                        {
                            var hero = Session.Project.FindHero(id);
                            lines.Add(StatBlock(hero, null, false, false, false, size));
                        }
                        else
                        {
                            var title = "";
                            var data = new Dictionary<int, string>();

                            if (secondary == "ac")
                                title = "Armour Class";
                            if (secondary == "fort")
                                title = "Fortitude";
                            if (secondary == "ref")
                                title = "Reflex";
                            if (secondary == "will")
                                title = "Will";
                            if (secondary == "passiveinsight")
                                title = "Passive Insight";
                            if (secondary == "passiveperception")
                                title = "Passive Perception";

                            foreach (var hero in Session.Project.Heroes)
                            {
                                var value = 0;

                                if (secondary == "ac")
                                    value = hero.Ac;
                                if (secondary == "fort")
                                    value = hero.Fortitude;
                                if (secondary == "ref")
                                    value = hero.Reflex;
                                if (secondary == "will")
                                    value = hero.Will;
                                if (secondary == "passiveinsight")
                                    value = hero.PassiveInsight;
                                if (secondary == "passiveperception")
                                    value = hero.PassivePerception;

                                var str = "<A href=show:" + hero.Id + ">" + hero.Name + "</A>";
                                if (data.ContainsKey(value))
                                    data[value] += ", " + str;
                                else
                                    data[value] = str;
                            }

                            lines.Add("<P class=table>");
                            lines.Add("<TABLE>");

                            lines.Add("<TR class=heading>");
                            lines.Add("<TD colspan=3>");
                            lines.Add("<B>" + title + "</B>");
                            lines.Add("</TD>");
                            lines.Add("</TR>");

                            var values = new List<int>(data.Keys);
                            values.Sort();
                            values.Reverse();
                            foreach (var value in values)
                            {
                                lines.Add("<TR>");
                                lines.Add("<TD>" + value + "</TD>");
                                lines.Add("<TD colspan=2>" + data[value] + "</TD>");
                                lines.Add("</TR>");
                            }

                            lines.Add("</TABLE>");
                            lines.Add("</P>");
                        }
                    }

                    lines.Add("</TD>");
                    lines.Add("</TR>");
                    lines.Add("</TABLE>");
                    lines.Add("</P>");
                }
            }
            else
            {
                lines.Add("<P class=instruction>");
                lines.Add("(no project loaded)");
                lines.Add("</P>");
            }

            lines.Add("</BODY>");

            return Concatenate(lines);
        }

        public static string EncounterReportTable(ReportTable table, DisplaySize size)
        {
            var lines = new List<string>();

            lines.AddRange(GetHead("Encounter Log", "", size));
            lines.Add("<BODY>");

            var title = "";
            switch (table.ReportType)
            {
                case ReportType.Time:
                    title = "Time Taken";
                    break;
                case ReportType.DamageToEnemies:
                    title = "Damage (to opponents)";
                    break;
                case ReportType.DamageToAllies:
                    title = "Damage (to allies)";
                    break;
                case ReportType.Movement:
                    title = "Movement";
                    break;
            }

            switch (table.BreakdownType)
            {
                case BreakdownType.Controller:
                    title += " (by controller)";
                    break;
                case BreakdownType.Faction:
                    title += " (by faction)";
                    break;
            }

            lines.Add("<H3>");
            lines.Add(title);
            lines.Add("</H3>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=encounterlog>");

            lines.Add("<TD align=center>");
            lines.Add("<B>Combatant</B>");
            lines.Add("</TD>");

            for (var round = 1; round <= table.Rounds; ++round)
            {
                lines.Add("<TD align=right>");
                lines.Add("<B>Round " + round + "</B>");
                lines.Add("</TD>");
            }

            lines.Add("<TD align=right>");
            lines.Add("<B>Total</B>");
            lines.Add("</TD>");

            lines.Add("</TR>");

            foreach (var row in table.Rows)
            {
                lines.Add("<TR>");

                lines.Add("<TD>");
                lines.Add("<B>" + row.Heading + "</B>");
                lines.Add("</TD>");

                for (var round = 0; round <= table.Rounds - 1; ++round)
                {
                    lines.Add("<TD align=right>");
                    switch (table.ReportType)
                    {
                        case ReportType.Time:
                        {
                            var ts = new TimeSpan(0, 0, row.Values[round]);
                            if (ts.TotalSeconds >= 1)
                                lines.Add(get_time(ts));
                            else
                                lines.Add("-");
                        }
                            break;
                        case ReportType.DamageToEnemies:
                        case ReportType.DamageToAllies:
                        case ReportType.Movement:
                        {
                            var value = row.Values[round];
                            if (value != 0)
                                lines.Add(value.ToString());
                            else
                                lines.Add("-");
                        }
                            break;
                    }

                    lines.Add("</TD>");
                }

                lines.Add("<TD align=right>");
                switch (table.ReportType)
                {
                    case ReportType.Time:
                    {
                        var ts = new TimeSpan(0, 0, row.Total);
                        if (ts.TotalSeconds >= 1)
                            lines.Add(get_time(ts));
                        else
                            lines.Add("-");
                    }
                        break;
                    case ReportType.DamageToEnemies:
                    case ReportType.DamageToAllies:
                    case ReportType.Movement:
                    {
                        var value = row.Total;
                        if (value != 0)
                            lines.Add(value.ToString());
                        else
                            lines.Add("-");
                    }
                        break;
                }

                lines.Add("</TD>");

                lines.Add("</TR>");
            }

            lines.Add("<TR>");

            lines.Add("<TD>");
            lines.Add("<B>Totals</B>");
            lines.Add("</TD>");

            for (var round = 0; round <= table.Rounds - 1; ++round)
            {
                lines.Add("<TD align=right>");
                switch (table.ReportType)
                {
                    case ReportType.Time:
                    {
                        var ts = new TimeSpan(0, 0, table.Rows[round].Total);
                        if (ts.TotalSeconds >= 1)
                            lines.Add(get_time(ts));
                        else
                            lines.Add("-");
                    }
                        break;
                    case ReportType.DamageToEnemies:
                    case ReportType.DamageToAllies:
                    case ReportType.Movement:
                    {
                        var value = table.Rows[round].Total;
                        if (value != 0)
                            lines.Add(value.ToString());
                        else
                            lines.Add("-");
                    }
                        break;
                }

                lines.Add("</TD>");
            }

            lines.Add("<TD align=right>");
            switch (table.ReportType)
            {
                case ReportType.Time:
                {
                    var ts = new TimeSpan(0, 0, table.GrandTotal);
                    if (ts.TotalSeconds >= 1)
                        lines.Add(get_time(ts));
                    else
                        lines.Add("-");
                }
                    break;
                case ReportType.DamageToEnemies:
                case ReportType.DamageToAllies:
                case ReportType.Movement:
                {
                    var value = table.GrandTotal;
                    if (value != 0)
                        lines.Add(value.ToString());
                    else
                        lines.Add("-");
                }
                    break;
            }

            lines.Add("</TD>");

            lines.Add("</TR>");

            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public static string TerrainPower(TerrainPower tp, DisplaySize size)
        {
            var lines = new List<string>();

            lines.AddRange(GetHead(null, null, size));
            lines.Add("<BODY>");
            lines.AddRange(get_terrain_power(tp));
            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Concatenate(lines);
        }

        public bool ExportProject(string filename)
        {
            try
            {
                // Remove extension from main filename
                var dir = FileName.Directory(filename);
                _fRelativePath = FileName.Name(filename) + " Files" + Path.DirectorySeparatorChar;
                _fFullPath = dir + _fRelativePath;

                var sw = new StreamWriter(filename);

                var content = get_content(Session.Preferences.TextSize);
                foreach (var line in content)
                    sw.WriteLine(line);

                sw.Close();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
                return false;
            }

            if (_fPlots.Count != 0 || _fMaps.Keys.Count != 0)
                // Make sure dir exists
                Directory.CreateDirectory(_fFullPath);

            // Save each plot image
            foreach (var pair in _fPlots)
                try
                {
                    var bmp = Screenshot.Plot(pair.Second, new Size(800, 600));
                    var imgFilename = get_filename(pair.First, "jpg", true);
                    bmp.Save(imgFilename, ImageFormat.Jpeg);
                }
                catch (Exception ex)
                {
                    LogSystem.Trace(ex);
                    return false;
                }

            // Save each map image
            foreach (var mapId in _fMaps.Keys)
                try
                {
                    var map = Session.Project.FindTacticalMap(mapId);

                    foreach (var areaId in _fMaps[mapId])
                    {
                        var view = Rectangle.Empty;
                        if (areaId != Guid.Empty)
                        {
                            var area = map.FindArea(areaId);
                            view = area.Region;
                        }

                        var bmp = Screenshot.Map(map, view, null, null, null);
                        var mapName = get_map_name(mapId, areaId);
                        var imgFilename = get_filename(mapName, "jpg", true);
                        bmp.Save(imgFilename, ImageFormat.Jpeg);
                    }
                }
                catch (Exception ex)
                {
                    LogSystem.Trace(ex);
                    return false;
                }

            return true;
        }

        public static string Concatenate(List<string> lines)
        {
            var text = "";
            foreach (var line in lines)
            {
                if (text != "")
                    text += Environment.NewLine;

                text += line;
            }

            return text;
        }

        public static string Process(string rawText, bool stripHtml)
        {
            var pairs = new List<Pair<string, string>>();

            pairs.Add(new Pair<string, string>("&", "&amp;"));
            pairs.Add(new Pair<string, string>("Á", "&Aacute;"));
            pairs.Add(new Pair<string, string>("á", "&aacute;"));
            pairs.Add(new Pair<string, string>("À", "&Agrave;"));
            pairs.Add(new Pair<string, string>("Â", "&Acirc;"));
            pairs.Add(new Pair<string, string>("à", "&agrave;"));
            pairs.Add(new Pair<string, string>("Â", "&Acirc;"));
            pairs.Add(new Pair<string, string>("â", "&acirc;"));
            pairs.Add(new Pair<string, string>("Ä", "&Auml;"));
            pairs.Add(new Pair<string, string>("ä", "&auml;"));
            pairs.Add(new Pair<string, string>("Ã", "&Atilde;"));
            pairs.Add(new Pair<string, string>("ã", "&atilde;"));
            pairs.Add(new Pair<string, string>("Å", "&Aring;"));
            pairs.Add(new Pair<string, string>("å", "&aring;"));
            pairs.Add(new Pair<string, string>("Æ", "&Aelig;"));
            pairs.Add(new Pair<string, string>("æ", "&aelig;"));
            pairs.Add(new Pair<string, string>("Ç", "&Ccedil;"));
            pairs.Add(new Pair<string, string>("ç", "&ccedil;"));
            pairs.Add(new Pair<string, string>("Ð", "&Eth;"));
            pairs.Add(new Pair<string, string>("ð", "&eth;"));
            pairs.Add(new Pair<string, string>("É", "&Eacute;"));
            pairs.Add(new Pair<string, string>("é", "&eacute;"));
            pairs.Add(new Pair<string, string>("È", "&Egrave;"));
            pairs.Add(new Pair<string, string>("è", "&egrave;"));
            pairs.Add(new Pair<string, string>("Ê", "&Ecirc;"));
            pairs.Add(new Pair<string, string>("ê", "&ecirc;"));
            pairs.Add(new Pair<string, string>("Ë", "&Euml;"));
            pairs.Add(new Pair<string, string>("ë", "&euml;"));
            pairs.Add(new Pair<string, string>("Í", "&Iacute;"));
            pairs.Add(new Pair<string, string>("í", "&iacute;"));
            pairs.Add(new Pair<string, string>("Ì", "&Igrave;"));
            pairs.Add(new Pair<string, string>("ì", "&igrave;"));
            pairs.Add(new Pair<string, string>("Î", "&Icirc;"));
            pairs.Add(new Pair<string, string>("î", "&icirc;"));
            pairs.Add(new Pair<string, string>("Ï", "&Iuml;"));
            pairs.Add(new Pair<string, string>("ï", "&iuml;"));
            pairs.Add(new Pair<string, string>("Ñ", "&Ntilde;"));
            pairs.Add(new Pair<string, string>("ñ", "&ntilde;"));
            pairs.Add(new Pair<string, string>("Ó", "&Oacute;"));
            pairs.Add(new Pair<string, string>("ó", "&oacute;"));
            pairs.Add(new Pair<string, string>("Ò", "&Ograve;"));
            pairs.Add(new Pair<string, string>("ò", "&ograve;"));
            pairs.Add(new Pair<string, string>("Ô", "&Ocirc;"));
            pairs.Add(new Pair<string, string>("ô", "&ocirc;"));
            pairs.Add(new Pair<string, string>("Ö", "&Ouml;"));
            pairs.Add(new Pair<string, string>("ö", "&ouml;"));
            pairs.Add(new Pair<string, string>("Õ", "&Otilde;"));
            pairs.Add(new Pair<string, string>("õ", "&otilde;"));
            pairs.Add(new Pair<string, string>("Ø", "&Oslash;"));
            pairs.Add(new Pair<string, string>("ø", "&oslash;"));
            pairs.Add(new Pair<string, string>("ß", "&szlig;"));
            pairs.Add(new Pair<string, string>("Þ", "&Thorn;"));
            pairs.Add(new Pair<string, string>("þ", "&thorn;"));
            pairs.Add(new Pair<string, string>("Ú", "&Uacute;"));
            pairs.Add(new Pair<string, string>("ú", "&uacute;"));
            pairs.Add(new Pair<string, string>("Ù", "&Ugrave;"));
            pairs.Add(new Pair<string, string>("ù", "&ugrave;"));
            pairs.Add(new Pair<string, string>("Û", "&Ucirc;"));
            pairs.Add(new Pair<string, string>("û", "&ucirc;"));
            pairs.Add(new Pair<string, string>("Ü", "&Uuml;"));
            pairs.Add(new Pair<string, string>("ü", "&uuml;"));
            pairs.Add(new Pair<string, string>("Ý", "&Yacute;"));
            pairs.Add(new Pair<string, string>("ý", "&yacute;"));
            pairs.Add(new Pair<string, string>("ÿ", "&yuml;"));
            pairs.Add(new Pair<string, string>("©", "&copy;"));
            pairs.Add(new Pair<string, string>("®", "&reg;"));
            pairs.Add(new Pair<string, string>("™", "&trade;"));
            pairs.Add(new Pair<string, string>("€", "&euro;"));
            pairs.Add(new Pair<string, string>("¢", "&cent;"));
            pairs.Add(new Pair<string, string>("£", "&pound;"));
            pairs.Add(new Pair<string, string>("‘", "&lsquo;"));
            pairs.Add(new Pair<string, string>("’", "&rsquo;"));
            pairs.Add(new Pair<string, string>("“", "&ldquo;"));
            pairs.Add(new Pair<string, string>("”", "&rdquo;"));
            pairs.Add(new Pair<string, string>("«", "&laquo;"));
            pairs.Add(new Pair<string, string>("»", "&raquo;"));
            pairs.Add(new Pair<string, string>("—", "&mdash;"));
            pairs.Add(new Pair<string, string>("–", "&ndash;"));
            pairs.Add(new Pair<string, string>("°", "&deg;"));
            pairs.Add(new Pair<string, string>("±", "&plusmn;"));
            pairs.Add(new Pair<string, string>("¼", "&frac14;"));
            pairs.Add(new Pair<string, string>("½", "&frac12;"));
            pairs.Add(new Pair<string, string>("¾", "&frac34;"));
            pairs.Add(new Pair<string, string>("×", "&times;"));
            pairs.Add(new Pair<string, string>("÷", "&divide;"));
            pairs.Add(new Pair<string, string>("α", "&alpha;"));
            pairs.Add(new Pair<string, string>("β", "&beta;"));
            pairs.Add(new Pair<string, string>("∞", "&infin;"));

            if (stripHtml)
            {
                pairs.Add(new Pair<string, string>("\"", "&quot;"));

                pairs.Add(new Pair<string, string>("<", "&lt;"));
                pairs.Add(new Pair<string, string>(">", "&gt;"));
            }

            var html = rawText;
            foreach (var pair in pairs)
                html = html.Replace(pair.First, pair.Second);

            return html;
        }

        public static List<string> GetHead(string title, string description, DisplaySize size)
        {
            var lines = new List<string>();

            lines.Add("<HEAD>");

            lines.Add("<META http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
            lines.Add("<META name=\"Generator\" content=\"Masterplan\" />");
            lines.Add("<META name=\"Originator\" content=\"Masterplan\" />");

            if (description != null)
                lines.Add("<META name=\"Description\" content=\"" + description + "\" />");

            lines.AddRange(GetStyle(size));

            if (title != null)
                lines.Add(Wrap(title, "title"));

            lines.Add("</HEAD>");

            return lines;
        }

        public static List<string> GetStyle(DisplaySize size)
        {
            if (FStyles.ContainsKey(size))
                return FStyles[size];

            var ptSizes = new Dictionary<int, int>();
            switch (size)
            {
                case DisplaySize.Small:
                    ptSizes[8] = 8;
                    ptSizes[9] = 9;
                    ptSizes[12] = 12;
                    ptSizes[16] = 16;
                    ptSizes[24] = 24;
                    break;
                case DisplaySize.Medium:
                    ptSizes[8] = 12;
                    ptSizes[9] = 14;
                    ptSizes[12] = 18;
                    ptSizes[16] = 24;
                    ptSizes[24] = 36;
                    break;
                case DisplaySize.Large:
                    ptSizes[8] = 16;
                    ptSizes[9] = 18;
                    ptSizes[12] = 24;
                    ptSizes[16] = 32;
                    ptSizes[24] = 48;
                    break;
                case DisplaySize.ExtraLarge:
                    ptSizes[8] = 25;
                    ptSizes[9] = 30;
                    ptSizes[12] = 40;
                    ptSizes[16] = 50;
                    ptSizes[24] = 72;
                    break;
            }

            var lines = new List<string>();

            lines.Add("<STYLE type=\"text/css\">");

            var loaded = false;
            var ass = Assembly.GetEntryAssembly();
            if (ass != null)
            {
                var externalStyleFile = FileName.Directory(ass.Location) + "Style." + size + ".css";
                if (File.Exists(externalStyleFile))
                {
                    var fileLines = File.ReadAllLines(externalStyleFile);
                    lines.AddRange(fileLines);

                    loaded = true;
                }
            }

            if (!loaded)
            {
                lines.Add("body                 { font-family: 'Segoe UI'; font-size: " + ptSizes[9] + "pt }");
                lines.Add("h1, h2, h3, h4       { color: #000060 }");
                lines.Add("h1                   { font-size: " + ptSizes[24] +
                          "pt; font-weight: bold; text-align: center }");
                lines.Add("h2                   { font-size: " + ptSizes[16] +
                          "pt; font-weight: bold; text-align: center }");
                lines.Add("h3                   { font-size: " + ptSizes[12] + "pt }");
                lines.Add("h4                   { font-size: " + ptSizes[9] + "pt }");
                lines.Add("p                    { padding: 3px 0 }");
                lines.Add(
                    "p.instruction        { color: #666666; text-align: center; font-size: " + ptSizes[8] + "pt }");
                lines.Add("p.description        { }");
                lines.Add("p.signature          { color: #666666; text-align: center }");
                lines.Add("p.readaloud          { padding-left: 15px; padding-right: 15px; font-style: italic }");
                lines.Add("p.background         { }");
                lines.Add("p.encounter_note     { }");
                lines.Add("p.encyclopedia_entry { }");
                lines.Add("p.note               { }");
                lines.Add("p.table              { text-align: center }");
                lines.Add("p.figure             { text-align: center }");
                lines.Add("table                { font-size: " + ptSizes[8] +
                          "pt; border-color: #BBBBBB; border-style: solid; border-width: 1px; border-collapse: collapse; table-layout: fixed; width: 99% }");
                lines.Add("table.clear          { border-style: none }");
                lines.Add("table.initiative     { table-layout: auto; border-style: none }");
                lines.Add("tr                   { background-color: #E1E7C5 }");
                lines.Add("tr.clear             { background-color: #FFFFFF }");
                lines.Add("tr.heading           { background-color: #143D5F; color: #FFFFFF }");
                lines.Add("tr.trap              { background-color: #5B1F34; color: #FFFFFF }");
                lines.Add("tr.template          { background-color: #5B1F34; color: #FFFFFF }");
                lines.Add("tr.creature          { background-color: #364F27; color: #FFFFFF }");
                lines.Add("tr.hero              { background-color: #143D5F; color: #FFFFFF }");
                lines.Add("tr.item              { background-color: #D06015; color: #FFFFFF }");
                lines.Add("tr.artifact          { background-color: #5B1F34; color: #FFFFFF }");
                lines.Add("tr.encounterlog      { background-color: #303030; color: #FFFFFF }");
                lines.Add("tr.shaded            { background-color: #9FA48D }");
                lines.Add("tr.dimmed            { color: #666666; text-decoration: line-through }");
                lines.Add("tr.shaded_dimmed     { background-color: #9FA48D; color: #666666 }");
                lines.Add("tr.atwill            { background-color: #238E23; color: #FFFFFF }");
                lines.Add("tr.encounter         { background-color: #8B0000; color: #FFFFFF }");
                lines.Add("tr.daily             { background-color: #000000; color: #FFFFFF }");
                lines.Add("tr.warning           { background-color: #E5A0A0; color: #000000; text-align: center }");
                lines.Add("td                   { padding-top: 3px; padding-bottom: 3px; vertical-align: top }");
                lines.Add("td.clear             { vertical-align: top; padding-left: 10px; padding-right: 10px }");
                lines.Add("td.indent            { padding-left: 15px }");
                lines.Add("td.readaloud         { font-style: italic }");
                lines.Add("td.dimmed            { color: #666666 }");
                lines.Add("td.pvlogentry        { color: lightgrey; background-color: #000000 }");
                lines.Add("td.pvlogindent       { color: #FFFFFF; background-color: #000000; padding-left: 15px }");
                lines.Add("ul, ol               { font-size: " + ptSizes[8] + "pt }");
                lines.Add("a                    { text-decoration: none }");
                lines.Add("a:link               { color: #0000C0 }");
                lines.Add("a:visited            { color: #0000C0 }");
                lines.Add("a:active             { color: #FF0000 }");
                lines.Add("a.missing            { color: #FF0000 }");
                lines.Add("a:hover              { text-decoration: underline }");
            }

            lines.Add("</STYLE>");

            FStyles[size] = lines;
            return lines;
        }

        private static string Wrap(string content, string tag)
        {
            var on = "<" + tag.ToUpper() + ">";
            var off = "</" + tag.ToUpper() + ">";

            return on + content + off;
        }

        private List<string> get_content(DisplaySize size)
        {
            var lines = new List<string>();

            var desc = Session.Project.Name + ": " + get_description();

            lines.Add("<HTML>");
            lines.AddRange(GetHead(Session.Project.Name, desc, size));
            lines.AddRange(get_body());
            lines.Add("</HTML>");

            return lines;
        }

        private List<string> get_body()
        {
            var lines = new List<string>();

            lines.Add("<BODY>");

            lines.Add("<H1>" + Session.Project.Name + "</H1>");
            lines.Add("<P class=description>" + get_description() + "</P>");
            if (Session.Project.Author != "")
                lines.Add("<P class=description>" + "By " + Process(Session.Project.Author, true) + "</P>");

            if (Session.Project.Backgrounds.Count != 0) lines.AddRange(get_backgrounds());

            if (Session.Project.Plot.Points.Count != 0)
            {
                lines.Add("<HR>");
                lines.AddRange(get_full_plot());
            }

            if (Session.Project.NpCs.Count != 0)
            {
                lines.Add("<HR>");
                lines.AddRange(get_npcs());
            }

            if (Session.Project.Encyclopedia.Entries.Count != 0)
            {
                lines.Add("<HR>");
                lines.AddRange(get_encyclopedia());
            }

            if (Session.Project.Notes.Count != 0)
            {
                lines.Add("<HR>");
                lines.AddRange(get_notes());
            }

            lines.Add("<HR>");
            lines.Add("<P class=signature>Designed using <B>Masterplan</B></P>");

            lines.Add("</BODY>");

            return lines;
        }

        private List<string> get_backgrounds()
        {
            var lines = new List<string>();

            foreach (var bg in Session.Project.Backgrounds)
            {
                if (bg.Details == "")
                    continue;

                lines.Add(Wrap(Process(bg.Title, true), "h3"));
                lines.Add("<P class=background>" + Process(bg.Details, false) + "</P>");
            }

            return lines;
        }

        private List<string> get_full_plot()
        {
            var lines = new List<string>();

            lines.Add(Wrap(Process(Session.Project.Name, true), "h2"));
            lines.AddRange(get_plot(Session.Project.Name, Session.Project.Plot));

            return lines;
        }

        private List<string> get_npcs()
        {
            var lines = new List<string>();

            lines.Add(Wrap("Encyclopedia", "h2"));

            foreach (var npc in Session.Project.NpCs)
            {
                lines.Add(Wrap(Process(npc.Name, true), "h3"));

                var details = Process(npc.Details, true);
                if (details != "")
                    lines.Add("<P>" + details + "</P>");

                lines.Add("<P class=table>");
                var card = new EncounterCard();
                card.CreatureId = npc.Id;
                lines.AddRange(card.AsText(null, CardMode.View, true));
                lines.Add("</P>");
            }

            return lines;
        }

        private List<string> get_encyclopedia()
        {
            var lines = new List<string>();

            lines.Add(Wrap("Encyclopedia", "h2"));

            foreach (var e in Session.Project.Encyclopedia.Entries)
            {
                lines.Add(Wrap(Process(e.Name, true), "h3"));
                lines.Add("<P class=encyclopedia_entry>" + Process(e.Details, false) + "</P>");
            }

            return lines;
        }

        private static string process_encyclopedia_info(string details, Encyclopedia encyclopedia,
            bool includeEntryLinks, bool includeDmInfo)
        {
            while (true)
            {
                var marker = "[[DM]]";

                var startIndex = details.IndexOf(marker);
                if (startIndex == -1)
                    break;

                var endIndex = details.IndexOf(marker, startIndex + marker.Length);
                if (endIndex == -1)
                    break;

                var middleStart = startIndex + marker.Length;
                var dmInfo = details.Substring(middleStart, endIndex - middleStart);

                if (includeDmInfo)
                    details = details.Substring(0, startIndex) + "<B>" + dmInfo + "</B>" +
                              details.Substring(endIndex + marker.Length);
                else
                    details = details.Substring(0, startIndex) + details.Substring(endIndex + marker.Length);
            }

            while (true)
            {
                var start = "[[";
                var end = "]]";

                var startIndex = details.IndexOf(start);
                if (startIndex == -1)
                    break;

                var endIndex = details.IndexOf(end, startIndex + start.Length);
                if (endIndex == -1)
                    break;

                var middleStart = startIndex + start.Length;
                var middle = details.Substring(middleStart, endIndex - middleStart);

                var entryName = middle;
                var displayText = middle;

                if (middle.Contains("|"))
                {
                    var index = middle.IndexOf("|");

                    entryName = middle.Substring(0, index);
                    displayText = middle.Substring(index + 1);

                    displayText = displayText.Trim();
                }

                var link = "";
                if (includeEntryLinks)
                {
                    var ee = encyclopedia.FindEntry(entryName);
                    if (ee == null)
                        link = "<A class=\"missing\" href=\"missing:" + entryName + "\" title=\"Create entry '" +
                               entryName + "'\">" + displayText + "</A>";
                    else
                        link = "<A href=\"entry:" + ee.Id + "\" title=\"" + ee.Name + "\">" + displayText + "</A>";
                }
                else
                {
                    link = displayText;
                }

                details = details.Substring(0, startIndex) + link + details.Substring(endIndex + end.Length);
            }

            details = details.Replace("<p>", "<p class=encyclopedia_entry>");

            return details;
        }

        private List<string> get_notes()
        {
            var lines = new List<string>();

            lines.Add(Wrap("Notes", "h2"));

            foreach (var n in Session.Project.Notes)
                lines.Add("<P class=note>" + Process(n.Content, true) + "</P>");

            return lines;
        }

        private string get_description()
        {
            return "An adventure for " + Session.Project.Party.Size + " characters of level " +
                   Session.Project.Party.Level + ".";
        }

        private List<string> get_plot(string plotName, Plot p)
        {
            var lines = new List<string>();

            if (p.Points.Count > 1)
            {
                // Plot flowchart image
                _fPlots.Add(new Pair<string, Plot>(plotName, p));
                var plotFile = get_filename(plotName, "jpg", false);
                lines.Add("<P class=figure><A href=\"" + plotFile + "\"><IMG src=\"" + plotFile + "\" alt=\"" +
                          Process(plotName, true) + "\" height=200></A></P>");
            }

            var layers = Workspace.FindLayers(p);
            foreach (var layer in layers)
            foreach (var pp in layer)
                lines.AddRange(get_plot_point(pp));

            return lines;
        }

        private List<string> get_plot_point(PlotPoint pp)
        {
            var lines = new List<string>();

            lines.Add(Wrap(Process(pp.Name, true), "h3"));

            if (pp.ReadAloud != "")
                lines.Add("<P class=readaloud>" + Process(pp.ReadAloud, false) + "</P>");

            if (pp.Details != "")
                lines.Add("<P>" + Process(pp.Details, false) + "</P>");

            if (pp.Date != null)
                lines.Add("<P>Date: " + pp.Date + "</P>");

            var enc = pp.Element as Encounter;
            if (enc != null)
            {
                lines.AddRange(get_encounter(enc));

                // Encounter map
                if (enc.MapId != Guid.Empty)
                {
                    add_map(enc.MapId, enc.MapAreaId);

                    var mapName = get_map_name(enc.MapId, enc.MapAreaId);
                    var mapFile = get_filename(mapName, "jpg", false);
                    lines.Add("<P class=figure><A href=\"" + mapFile + "\"><IMG src=\"" + mapFile + "\" alt=\"" +
                              Process(pp.Name, true) + "\" height=200></A></P>");
                }
            }

            var te = pp.Element as TrapElement;
            if (te != null)
                lines.AddRange(get_trap(te.Trap, null, false, false));

            var sc = pp.Element as SkillChallenge;
            if (sc != null)
                lines.AddRange(get_skill_challenge(sc, false));

            var q = pp.Element as Quest;
            if (q != null)
                lines.AddRange(get_quest(q));

            var m = pp.Element as MapElement;
            if (m != null)
                if (m.MapId != Guid.Empty)
                {
                    add_map(m.MapId, m.MapAreaId);

                    var mapName = get_map_name(m.MapId, m.MapAreaId);
                    var mapFile = get_filename(mapName, "jpg", false);
                    lines.Add("<P class=figure><A href=\"" + mapFile + "\"><IMG src=\"" + mapFile + "\" alt=\"" +
                              Process(mapName, true) + "\" height=200></A></P>");
                }

            if (pp.Parcels.Count != 0)
                lines.AddRange(get_parcels(pp, false));

            if (pp.Subplot.Points.Count != 0)
            {
                lines.Add("<BLOCKQUOTE>");
                lines.AddRange(get_plot(pp.Name, pp.Subplot));
                lines.Add("</BLOCKQUOTE>");
            }

            return lines;
        }

        private static List<string> get_map_area_details(PlotPoint pp)
        {
            var lines = new List<string>();

            Map map = null;
            MapArea mapArea = null;
            pp.GetTacticalMapArea(ref map, ref mapArea);

            if (map != null && mapArea != null && mapArea.Details != "")
            {
                lines.Add("<P><B>" + Process(mapArea.Name, true) + "</B>:</P>");
                lines.Add("<P>" + Process(mapArea.Details, true) + "</P>");
            }

            return lines;
        }

        private static List<string> get_encounter(Encounter enc)
        {
            var lines = new List<string>();

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD colspan=2>");
            lines.Add("<B>Encounter</B>");
            lines.Add("</TD>");
            lines.Add("<TD>");
            lines.Add(enc.GetXp() + " XP");
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Level</B> " + enc.GetLevel(Session.Project.Party.Size));
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (enc.Slots.Count != 0)
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=2>");
                lines.Add("<B>Combatants</B>");
                lines.Add("</TD>");
                lines.Add("<TD>");
                lines.Add("<B>" + enc.Count + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                foreach (var slot in enc.Slots)
                {
                    lines.Add("<TR>");

                    lines.Add("<TD colspan=2>");
                    lines.Add(slot.Card.Title);
                    lines.Add("</TD>");

                    lines.Add("<TD>");
                    if (slot.CombatData.Count > 1)
                        lines.Add("x" + slot.CombatData.Count);
                    lines.Add("</TD>");

                    lines.Add("</TR>");
                }
            }

            foreach (var ew in enc.Waves)
            {
                if (ew.Count == 0)
                    continue;

                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=2>");
                lines.Add("<B>" + ew.Name + "</B>");
                lines.Add("</TD>");
                lines.Add("<TD>");
                lines.Add("<B>" + ew.Count + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                foreach (var slot in ew.Slots)
                {
                    lines.Add("<TR>");

                    lines.Add("<TD colspan=2>");
                    lines.Add(slot.Card.Title);
                    lines.Add("</TD>");

                    lines.Add("<TD>");
                    if (slot.CombatData.Count > 1)
                        lines.Add("x" + slot.CombatData.Count);
                    lines.Add("</TD>");

                    lines.Add("</TR>");
                }
            }

            if (enc.Traps.Count != 0)
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=2>");
                lines.Add("<B>Traps / Hazards</B>");
                lines.Add("</TD>");
                lines.Add("<TD>");
                lines.Add("<B>" + enc.Traps.Count + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                foreach (var trap in enc.Traps)
                {
                    lines.Add("<TR>");

                    lines.Add("<TD colspan=3>");
                    lines.Add(Process(trap.Name, true));
                    lines.Add("</TD>");

                    lines.Add("</TR>");
                }
            }

            if (enc.SkillChallenges.Count != 0)
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=2>");
                lines.Add("<B>Skill Challenges</B>");
                lines.Add("</TD>");
                lines.Add("<TD>");
                lines.Add("<B>" + enc.SkillChallenges.Count + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                foreach (var sc in enc.SkillChallenges)
                {
                    lines.Add("<TR>");

                    lines.Add("<TD colspan=3>");
                    lines.Add(Process(sc.Name, true));
                    lines.Add("</TD>");

                    lines.Add("</TR>");
                }
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            foreach (var note in enc.Notes)
            {
                if (note.Contents == "")
                    continue;

                lines.Add("<P class=encounter_note>");
                lines.Add("<B>" + Process(note.Title, true) + "</B>");
                lines.Add("</P>");

                lines.Add("<P class=encounter_note>");
                lines.Add(Process(note.Contents, false));
                lines.Add("</P>");
            }

            // Opponent stats
            var shown = new List<string>();
            foreach (var slot in enc.AllSlots)
            {
                if (shown.Contains(slot.Card.Title))
                    continue;

                lines.Add("<P class=table>");
                lines.AddRange(slot.Card.AsText(null, CardMode.View, true));
                lines.Add("</P>");

                shown.Add(slot.Card.Title);
            }

            foreach (var trap in enc.Traps) lines.AddRange(get_trap(trap, null, false, false));

            foreach (var sc in enc.SkillChallenges) lines.AddRange(get_skill_challenge(sc, false));

            foreach (var ct in enc.CustomTokens)
            {
                if (ct.Type == CustomTokenType.Token)
                    continue;

                lines.AddRange(get_custom_token(ct));
            }

            return lines;
        }

        private static List<string> get_trap(Trap trap, CombatData cd, bool initiativeHolder, bool builder)
        {
            var lines = new List<string>();

            if (initiativeHolder)
            {
                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>Information</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add(Process(trap.Name, true) + " is the current initiative holder");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=trap>");
            lines.Add("<TD colspan=2>");
            lines.Add("<B>" + Process(trap.Name, true) + "</B>");
            lines.Add("<BR>");
            lines.Add(Process(trap.Info, true));
            lines.Add("</TD>");
            lines.Add("<TD>");
            lines.Add(trap.Xp + " XP");
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (builder)
            {
                lines.Add("<TR class=trap>");
                lines.Add("<TD colspan=3 align=center>");
                lines.Add("<A href=build:profile style=\"color:white\">(click here to edit this top section)</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3 align=center>");

                lines.Add("<A href=build:addskill>add a skill</A>");
                lines.Add("|");
                lines.Add("<A href=build:addattack>add an attack</A>");
                lines.Add("|");
                lines.Add("<A href=build:addcm>add a countermeasure</A>");

                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var readaloud = Process(trap.ReadAloud, true);
            if (builder)
            {
                if (readaloud == "")
                    readaloud = "<A href=build:readaloud>Click here to enter read-aloud text</A>";
                else
                    readaloud += " <A href=build:readaloud>(edit)</A>";
            }

            if (readaloud != "")
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD class=readaloud colspan=3>");
                lines.Add(readaloud);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var desc = Process(trap.Description, true);
            if (builder)
            {
                if (desc == "")
                    desc = "<A href=build:desc>Click here to enter a description</A>";
                else
                    desc += " <A href=build:desc>(edit)</A>";
            }

            if (desc != "")
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add(desc);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var details = Process(trap.Details, true);
            if (builder)
            {
                if (details == "")
                    details = "<A href=build:details>(no trap details entered)</A>";
                else
                    details += " <A href=build:details>(edit)</A>";
            }

            if (details != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>" + trap.Type + "</B>: ");
                lines.Add(details);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var skillnames = new List<string>();
            var skills = new Dictionary<string, List<TrapSkillData>>();
            foreach (var tsd in trap.Skills)
            {
                if (tsd.Details == "")
                    continue;

                if (tsd.SkillName != "Perception" && !skillnames.Contains(tsd.SkillName))
                    skillnames.Add(tsd.SkillName);

                if (!skills.ContainsKey(tsd.SkillName))
                    skills[tsd.SkillName] = new List<TrapSkillData>();

                skills[tsd.SkillName].Add(tsd);
            }

            skillnames.Sort();
            if (skills.ContainsKey("Perception"))
                skillnames.Insert(0, "Perception");

            foreach (var skillname in skillnames)
            {
                var data = skills[skillname];
                data.Sort();

                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>" + Process(skillname, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                foreach (var tsd in data)
                {
                    lines.Add("<TR>");
                    lines.Add("<TD colspan=3>");
                    if (tsd.Dc != 0)
                        lines.Add("<B>DC " + tsd.Dc + "</B>:");
                    lines.Add(Process(tsd.Details, true));
                    if (builder)
                        lines.Add("(<A href=skill:" + tsd.Id + ">edit</A> | <A href=skillremove:" + tsd.Id +
                                  ">remove</A>)");
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }
            }

            if (trap.Initiative != int.MinValue)
            {
                var initStr = trap.Initiative.ToString();
                if (trap.Initiative >= 0)
                    initStr = "+" + initStr;

                if (cd != null)
                    initStr = cd.Initiative + " (" + initStr + ")";

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Initiative</B>:");
                if (builder)
                    lines.Add("<A href=build:profile>");
                if (cd != null)
                    lines.Add("<A href=init:" + cd.Id + ">");
                lines.Add(initStr);
                if (cd != null)
                    lines.Add("</A>");
                if (builder)
                    lines.Add("</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }
            else if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Initiative</B>: <A href=build:profile>The trap does not roll initiative</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (trap.Trigger != "")
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Trigger</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                if (builder)
                    lines.Add("<A href=build:trigger>");
                lines.Add(Process(trap.Trigger, true));
                if (builder)
                    lines.Add("</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }
            else if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Trigger</B>: <A href=build:trigger>Set trigger</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            foreach (var ta in trap.Attacks)
                lines.AddRange(get_trap_attack(ta, cd != null, builder));

            if (trap.Countermeasures.Count != 0)
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Countermeasures</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                for (var index = 0; index != trap.Countermeasures.Count; ++index)
                {
                    var cm = trap.Countermeasures[index];

                    lines.Add("<TR>");
                    lines.Add("<TD colspan=3>");
                    if (builder)
                        lines.Add("<A href=cm:" + index + ">");
                    lines.Add(Process(cm, true));
                    if (builder)
                        lines.Add("</A>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }
            }

            var t = Session.FindTrap(trap.Id, SearchType.External);
            if (t != null)
            {
                var lib = Session.FindLibrary(t);
                if (lib != null)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD colspan=3>");
                    lines.Add(Process(lib.Name, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            return lines;
        }

        private static List<string> get_trap_attack(TrapAttack trapAttack, bool links, bool builder)
        {
            var lines = new List<string>();

            var name = trapAttack.Name;
            if (name == "")
                name = "Attack";
            lines.Add("<TR class=shaded>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>" + name + "</B>");
            if (builder)
            {
                lines.Add("<A href=attackaction:" + trapAttack.Id + ">");
                lines.Add("(edit)");
                lines.Add("</A>");

                lines.Add("<A href=attackremove:" + trapAttack.Id + ">");
                lines.Add("(remove)");
                lines.Add("</A>");
            }

            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Action</B>:");
            if (builder)
                lines.Add("<A href=attackaction:" + trapAttack.Id + ">");
            lines.Add(trapAttack.Action.ToString().ToLower());
            if (builder)
                lines.Add("</A>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (trapAttack.Range != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Range</B>:");
                if (builder)
                    lines.Add("<A href=attackaction:" + trapAttack.Id + ">");
                lines.Add(Process(trapAttack.Range, true));
                if (builder)
                    lines.Add("</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }
            else if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Range</B>: <A href=attackaction:" + trapAttack.Id + ">Set range</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (trapAttack.Target != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Target</B>:");
                if (builder)
                    lines.Add("<A href=attackaction:" + trapAttack.Id + ">");
                lines.Add(Process(trapAttack.Target, true));
                if (builder)
                    lines.Add("</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }
            else if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Target</B>: <A href=attackaction:" + trapAttack.Id + ">Set target</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (trapAttack.Attack != null)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Attack</B>:");
                if (builder)
                    lines.Add("<A href=attackattack:" + trapAttack.Id + ">");
                if (links)
                    lines.Add("<A href=power:" + trapAttack.Id + ">");
                lines.Add(trapAttack.Attack.ToString());
                if (links)
                    lines.Add("</A>");
                if (builder)
                    lines.Add("</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }
            else if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Attack</B>: <A href=attackattack:" + trapAttack.Id + ">Set attack</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (trapAttack.OnHit != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Hit</B>:");
                if (builder)
                    lines.Add("<A href=attackhit:" + trapAttack.Id + ">");
                lines.Add(Process(trapAttack.OnHit, true));
                if (builder)
                    lines.Add("</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }
            else if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Hit</B>: <A href=attackhit:" + trapAttack.Id + ">Set hit</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (trapAttack.OnMiss != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Miss</B>:");
                if (builder)
                    lines.Add("<A href=attackmiss:" + trapAttack.Id + ">");
                lines.Add(Process(trapAttack.OnMiss, true));
                if (builder)
                    lines.Add("</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }
            else if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Miss</B>: <A href=attackmiss:" + trapAttack.Id + ">Set miss</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (trapAttack.Effect != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Effect</B>:");
                if (builder)
                    lines.Add("<A href=attackeffect:" + trapAttack.Id + ">");
                lines.Add(Process(trapAttack.Effect, true));
                if (builder)
                    lines.Add("</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }
            else if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Effect</B>: <A href=attackeffect:" + trapAttack.Id + ">Set effect</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (trapAttack.Notes != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Notes</B>:");
                if (builder)
                    lines.Add("<A href=attacknotes:" + trapAttack.Id + ">");
                lines.Add(Process(trapAttack.Notes, true));
                if (builder)
                    lines.Add("</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }
            else if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Notes</B>: <A href=attacknotes:" + trapAttack.Id + ">Set notes</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            return lines;
        }

        private static List<string> get_skill_challenge(SkillChallenge sc, bool includeLinks)
        {
            // Separate primary and secondary skills
            var primarySkills = new List<SkillChallengeData>();
            var otherSkills = new List<SkillChallengeData>();
            var failSkills = new List<SkillChallengeData>();

            foreach (var scd in sc.Skills)
                switch (scd.Type)
                {
                    case SkillType.Primary:
                        primarySkills.Add(scd);
                        break;
                    case SkillType.Secondary:
                        otherSkills.Add(scd);
                        break;
                    case SkillType.AutoFail:
                        failSkills.Add(scd);
                        break;
                }

            var lines = new List<string>();

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=trap>");
            lines.Add("<TD colspan=2>");
            lines.Add("<B>" + Process(sc.Name, true) + "</B>");
            lines.Add("</TD>");
            lines.Add("<TD>");
            lines.Add(sc.GetXp() + " XP");
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Level</B> " + sc.Level);
            lines.Add("<BR>");
            lines.Add("<B>Complexity</B> " + sc.Complexity + " (requires " + sc.Successes +
                      " successes before 3 failures)");
            lines.Add("</TD>");
            lines.Add("</TR>");

            var results = sc.Results;
            if (results.Successes + results.Fails != 0)
            {
                var str = "In Progress";
                if (results.Fails >= 3)
                    str = "Failed";
                else if (results.Successes >= sc.Successes)
                    str = "Succeeded";

                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>" + str + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Successes</B> " + results.Successes + " <B>Failures</B> " + results.Fails);
                if (includeLinks)
                    // Add reset link
                    lines.Add("(<A href=\"sc:reset\">reset</A>)");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            // Primary skills
            if (primarySkills.Count != 0)
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Primary Skills</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                foreach (var scd in primarySkills)
                    lines.AddRange(get_skill(scd, sc.Level, true, includeLinks));
            }

            // Other skills
            if (otherSkills.Count != 0)
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Other Skills</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                foreach (var scd in otherSkills)
                    lines.AddRange(get_skill(scd, sc.Level, true, false));
            }

            // Automatic failure skills
            if (failSkills.Count != 0)
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Automatic Failure</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                foreach (var scd in failSkills)
                    lines.AddRange(get_skill(scd, sc.Level, false, false));
            }

            if (sc.Success != "")
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Victory</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add(Process(sc.Success, true));
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (sc.Failure != "")
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Defeat</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add(Process(sc.Failure, true));
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (sc.Notes != "")
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Notes</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add(Process(sc.Notes, true));
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var ch = Session.FindSkillChallenge(sc.Id, SearchType.External);
            if (ch != null)
            {
                var lib = Session.FindLibrary(ch);
                if (lib != null)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD colspan=3>");
                    lines.Add(Process(lib.Name, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            return lines;
        }

        private static List<string> get_skill(SkillChallengeData scd, int level, bool includeDetails, bool includeLinks)
        {
            var lines = new List<string>();

            var info = "<B>" + scd.SkillName + "</B>";
            if (includeDetails)
            {
                var dc = Ai.GetSkillDc(scd.Difficulty, level) + scd.DcModifier;
                info += " (DC " + dc + ")";
            }

            if (scd.Details != "")
                info += ": " + scd.Details;

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add(Process(info, false));
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (includeDetails)
            {
                if (scd.Success != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD class=indent colspan=3>");
                    lines.Add("<B>Success</B>: " + Process(scd.Success, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (scd.Failure != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD class=indent colspan=3>");
                    lines.Add("<B>Failure</B>: " + Process(scd.Failure, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }
            }

            lines.Add("<TR>");
            lines.Add("<TD class=indent colspan=3>");
            if (includeLinks)
            {
                // Add success / failure links
                lines.Add("Add a <A href=\"success:" + scd.SkillName + "\">success</A>");
                if (scd.Results.Successes > 0)
                    lines.Add("(" + scd.Results.Successes + ")");
                lines.Add("or <A href=\"failure:" + scd.SkillName + "\">failure</A>");
                if (scd.Results.Fails > 0)
                    lines.Add("(" + scd.Results.Fails + ")");
            }

            lines.Add("</TD>");
            lines.Add("</TR>");

            return lines;
        }

        private static List<string> get_quest(Quest q)
        {
            var name = q.Type == QuestType.Major ? "Major Quest" : "Minor Quest";

            var lines = new List<string>();

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD colspan=2>");
            lines.Add("<B>" + name + "</B>");
            lines.Add("</TD>");
            lines.Add("<TD>");
            lines.Add(q.GetXp() + " XP");
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Level</B> " + q.Level);
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("</TABLE>");
            lines.Add("</P>");

            return lines;
        }

        private static List<string> get_hero(Hero hero, Encounter enc, bool initiativeHolder, bool showEffects)
        {
            var lines = new List<string>();

            if (enc != null)
                lines.AddRange(get_combat_data(hero.CombatData, hero.Hp, enc, initiativeHolder));

            if (showEffects && hero.Effects.Count != 0)
            {
                lines.Add("<P class=table>");
                lines.Add("<TABLE>");
                lines.Add("<TR class=heading><TD colspan=3><B>Effects</B></TD></TR>");
                foreach (var oc in hero.Effects)
                {
                    var index = hero.Effects.IndexOf(oc);
                    lines.Add("<TR><TD colspan=2>" + oc.ToString(enc, true) + "</TD>");
                    lines.Add("<TD align=right><A href=addeffect:" + index + ">Apply &#8658</A></TD></TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=hero>");
            lines.Add("<TD colspan=2>");
            lines.Add("<B>" + Process(hero.Name, true) + "</B>");
            lines.Add("</TD>");
            lines.Add("<TD align=right>");
            lines.Add(Process(hero.Player, true));
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add(Process(hero.Info, true));
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR class=shaded>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Combat</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            var initStr = hero.InitBonus.ToString();
            if (hero.InitBonus >= 0)
                initStr = "+" + initStr;
            if (hero.CombatData != null && hero.CombatData.Initiative != int.MinValue)
                initStr = hero.CombatData.Initiative + " (" + initStr + ")";

            if (enc != null) initStr = "<A href=init:" + hero.CombatData.Id + ">" + initStr + "</A>";

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Initiative</B> " + initStr);
            lines.Add("</TD>");
            lines.Add("</TR>");

            var hp = hero.Hp.ToString();
            if (hero.CombatData != null && hero.CombatData.Damage != 0)
            {
                var health = hero.Hp - hero.CombatData.Damage;
                hp = health + " of " + hero.Hp;
            }

            var hpStr = "<B>HP</B> " + hp;
            if (enc != null) hpStr = "<A href=hp:" + hero.Id + ">" + hpStr + "</A>";
            hpStr += "; " + "<B>Bloodied</B>" + " " + hero.Hp / 2;
            if (hero.CombatData != null && hero.CombatData.TempHp > 0)
                hpStr += "; " + "<B>Temp HP</B> " + hero.CombatData.TempHp;

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add(hpStr);
            lines.Add("</TD>");
            lines.Add("</TR>");

            var ac = hero.Ac;
            var fort = hero.Fortitude;
            var reflex = hero.Reflex;
            var will = hero.Will;

            if (hero.CombatData != null)
                foreach (var oc in hero.CombatData.Conditions)
                {
                    if (oc.Type != OngoingType.DefenceModifier)
                        continue;

                    if (oc.Defences.Contains(DefenceType.Ac))
                        ac += oc.DefenceMod;
                    if (oc.Defences.Contains(DefenceType.Fortitude))
                        fort += oc.DefenceMod;
                    if (oc.Defences.Contains(DefenceType.Reflex))
                        reflex += oc.DefenceMod;
                    if (oc.Defences.Contains(DefenceType.Will))
                        will += oc.DefenceMod;
                }

            var acStr = ac.ToString();
            if (ac != hero.Ac)
                acStr = "<B><I>" + acStr + "</I></B>";
            var fortStr = fort.ToString();
            if (fort != hero.Fortitude)
                fortStr = "<B><I>" + fortStr + "</I></B>";
            var refStr = reflex.ToString();
            if (reflex != hero.Reflex)
                refStr = "<B><I>" + refStr + "</I></B>";
            var willStr = will.ToString();
            if (will != hero.Will)
                willStr = "<B><I>" + willStr + "</I></B>";

            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>AC</B> " + acStr + "; <B>Fort</B> " + fortStr + "; <B>Ref</B> " + refStr + "; <B>Will</B> " +
                      willStr);
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR class=shaded>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Skills</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");
            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Passive Insight</B> " + hero.PassiveInsight);
            lines.Add("<BR>");
            lines.Add("<B>Passive Perception</B> " + hero.PassivePerception);
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (hero.Languages != "")
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Languages</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add(Process(hero.Languages, true));
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            return lines;
        }

        private static List<string> get_combat_data(CombatData cd, int maxHp, Encounter enc, bool initiativeHolder)
        {
            var bloodiedHp = maxHp / 2;
            var currentHp = maxHp - cd.Damage;
            var isBloodied = maxHp != 0 && currentHp <= bloodiedHp;
            var isDead = maxHp != 0 && currentHp <= 0;

            var lines = new List<string>();

            if (cd.Conditions.Count != 0 || isBloodied || isDead || initiativeHolder || cd.Altitude != 0)
            {
                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>Information</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (initiativeHolder)
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add(cd.DisplayName + " is the current initiative holder");
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (cd.Altitude != 0)
                {
                    var squares = Math.Abs(cd.Altitude) == 1 ? "square" : "squares";
                    var direction = cd.Altitude > 0 ? "up" : "down";

                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add(cd.DisplayName + " is " + Math.Abs(cd.Altitude) + " " + squares + " <B>" + direction +
                              "</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (isDead)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>Hit Points</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add(cd.DisplayName + " is <B>dead</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }
                else if (isBloodied)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>Hit Points</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add(cd.DisplayName + " is <B>bloodied</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (cd.Conditions.Count != 0)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>Effects</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    foreach (var oc in cd.Conditions)
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD>");

                        lines.Add(oc.ToString(enc, true));

                        // Add link for quick removal
                        var index = cd.Conditions.IndexOf(oc);
                        lines.Add("<A href=\"effect:" + cd.Id + ":" + index + "\">(remove)</A>");

                        if (oc.Type == OngoingType.Condition)
                        {
                            lines.Add("</TD>");
                            lines.Add("</TR>");

                            lines.Add("<TR>");
                            lines.Add("<TD class=indent>");

                            var info = Conditions.GetConditionInfo(oc.Data);
                            if (info.Count != 0)
                                lines.AddRange(info);

                            lines.Add("</TD>");
                            lines.Add("</TR>");
                        }
                    }
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            return lines;
        }

        private static List<string> get_magic_item(MagicItem item, bool builder)
        {
            var lines = new List<string>();

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=item>");
            lines.Add("<TD colspan=2>");
            lines.Add("<B>" + Process(item.Name, true) + "</B>");
            lines.Add("</TD>");
            lines.Add("<TD>");
            lines.Add(Process(item.Type, true));
            lines.Add("</TD>");
            lines.Add("</TR>");
            if (builder)
            {
                lines.Add("<TR class=item>");
                lines.Add("<TD colspan=3 align=center>");
                lines.Add("<A href=build:profile style=\"color:white\">(click here to edit this top section)</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var desc = Process(item.Description, true);
            if (builder && desc == "")
                desc = "(no description set)";
            if (desc != "")
            {
                if (builder)
                    desc = "<A href=build:desc>" + desc + "</A>";

                lines.Add("<TR>");
                lines.Add("<TD class=readaloud colspan=3>");
                lines.Add(desc);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var rarity = item.Rarity.ToString();
            if (builder)
                rarity = "<A href=build:profile>" + rarity + "</A>";
            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Availability</B> " + rarity);
            lines.Add("</TD>");
            lines.Add("</TR>");

            var level = item.Level.ToString();
            if (builder)
                level = "<A href=build:profile>" + level + "</A>";
            lines.Add("<TR>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Level</B> " + level);
            lines.Add("</TD>");
            lines.Add("</TR>");

            // Sections
            foreach (var section in item.Sections)
            {
                var index = item.Sections.IndexOf(section);

                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>" + Process(section.Header, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD class=indent colspan=3>");
                lines.Add(Process(section.Details, true));
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (builder)
                {
                    lines.Add("<TR>");
                    lines.Add("<TD colspan=3 align=center>");
                    lines.Add("<A href=edit:" + index + ">edit</A>");
                    lines.Add("|");
                    lines.Add("<A href=remove:" + index + ">remove</A>");
                    if (item.Sections.Count > 1)
                    {
                        if (index != 0)
                        {
                            lines.Add("|");
                            lines.Add("<A href=moveup:" + index + ">move up</A>");
                        }

                        if (index != item.Sections.Count - 1)
                        {
                            lines.Add("|");
                            lines.Add("<A href=movedown:" + index + ">move down</A>");
                        }
                    }

                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }
            }

            if (builder)
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Sections</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (item.Sections.Count == 0)
                {
                    lines.Add("<TR>");
                    lines.Add("<TD class=indent colspan=3>");
                    lines.Add("No details set");
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("<TR>");
                lines.Add("<TD colspan=3 align=center>");
                lines.Add("<A href=section:new>add a new section</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var lib = Session.FindLibrary(item);
            if (lib != null)
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add(Process(lib.Name, true));
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            return lines;
        }

        private static List<string> get_artifact(Artifact artifact, bool builder)
        {
            var lines = new List<string>();

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=artifact>");
            lines.Add("<TD colspan=2>");
            lines.Add("<B>" + Process(artifact.Name, true) + "</B>");
            lines.Add("</TD>");
            lines.Add("<TD align=center>");
            lines.Add(artifact.Tier + " tier");
            lines.Add("</TD>");
            lines.Add("</TR>");
            if (builder)
            {
                lines.Add("<TR class=artifact>");
                lines.Add("<TD colspan=3 align=center>");
                lines.Add("<A href=build:profile style=\"color:white\">(click here to edit this top section)</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var desc = Process(artifact.Description, true);
            if (builder)
            {
                if (desc == "")
                    desc = "click to set description";

                desc = "<A href=build:description>" + desc + "</A>";
            }

            if (desc != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD class=readaloud colspan=3>");
                lines.Add(desc);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var details = Process(artifact.Details, true);
            if (builder)
            {
                if (details == "")
                    details = "click to set details";

                details = "<A href=build:details>" + details + "</A>";
            }

            if (details != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add(details);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            foreach (var section in artifact.Sections)
            {
                var index = artifact.Sections.IndexOf(section);

                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>" + Process(section.Header, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD class=indent colspan=3>");
                lines.Add(Process(section.Details, true));
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (builder)
                {
                    lines.Add("<TR>");
                    lines.Add("<TD colspan=3 align=center>");
                    lines.Add("<A href=sectionedit:" + index + ">edit</A>");
                    lines.Add("|");
                    lines.Add("<A href=sectionremove:" + index + ">remove</A>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }
            }

            if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3 align=center>");
                lines.Add("<A href=section:new>add a section</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            // Goals
            var goals = Process(artifact.Goals, true);
            if (builder)
            {
                if (goals == "")
                    goals = "click to set goals";

                goals = "<A href=build:goals>" + goals + "</A>";
            }

            if (goals != "")
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Goals of " + Process(artifact.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add(goals);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            // Roleplaying
            var rp = Process(artifact.RoleplayingTips, true);
            if (builder)
            {
                if (rp == "")
                    rp = "click to set roleplaying tips";

                rp = "<A href=build:rp>" + rp + "</A>";
            }

            if (rp != "")
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Roleplaying " + Process(artifact.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");
                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add(rp);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            // Concordance table
            lines.Add("<TR class=shaded>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Concordance</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");
            lines.Add("<TR>");
            lines.Add("<TD colspan=2>Starting score</TD>");
            lines.Add("<TD align=center>5</TD>");
            lines.Add("</TR>");
            foreach (var ac in artifact.ConcordanceRules)
            {
                var index = artifact.ConcordanceRules.IndexOf(ac);

                lines.Add("<TR>");
                lines.Add("<TD colspan=2>");
                lines.Add(ac.First);
                if (builder)
                {
                    lines.Add("<A href=ruleedit:" + index + ">edit</A>");
                    lines.Add("|");
                    lines.Add("<A href=ruleremove:" + index + ">remove</A>");
                }

                lines.Add("</TD>");
                lines.Add("<TD align=center>");
                lines.Add(ac.Second);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (builder)
            {
                lines.Add("<TR>");
                lines.Add("<TD colspan=3 align=center>");
                lines.Add("<A href=rule:new>add a concordance rule</A>");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            // Concordance levels
            foreach (var ac in artifact.ConcordanceLevels)
            {
                var acIndex = artifact.ConcordanceLevels.IndexOf(ac);

                var name = Process(ac.Name, true);
                if (ac.ValueRange != "")
                    name += " (" + Process(ac.ValueRange, true) + ")";

                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>" + name + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                var acQuote = Process(ac.Quote, true);
                if (builder)
                {
                    if (acQuote == "")
                        acQuote = "click to set a quote for this concordance level";

                    acQuote = "<A href=quote:" + acIndex + ">" + acQuote + "</A>";
                }

                if (acQuote != "")
                {
                    lines.Add("<TR class=readaloud>");
                    lines.Add("<TD colspan=3>");
                    lines.Add(acQuote);
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                var acDesc = Process(ac.Description, true);
                if (builder)
                {
                    if (acDesc == "")
                        acDesc = "click to set concordance details";

                    acDesc = "<A href=desc:" + acIndex + ">" + acDesc + "</A>";
                }

                if (acDesc != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD colspan=3>");
                    lines.Add(acDesc);
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                // Properties
                if (ac.ValueRange != "")
                {
                    foreach (var section in ac.Sections)
                    {
                        var index = artifact.Sections.IndexOf(section);

                        lines.Add("<TR class=shaded>");
                        lines.Add("<TD colspan=3>");
                        lines.Add("<B>" + Process(section.Header, true) + "</B>");
                        lines.Add("</TD>");
                        lines.Add("</TR>");

                        lines.Add("<TR>");
                        lines.Add("<TD class=indent colspan=3>");
                        lines.Add(Process(section.Details, true));
                        lines.Add("</TD>");
                        lines.Add("</TR>");

                        if (builder)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD colspan=3 align=center>");
                            lines.Add("<A href=sectionedit:" + acIndex + "," + index + ">edit</A>");
                            lines.Add("|");
                            lines.Add("<A href=sectionremove:" + acIndex + "," + index + ">remove</A>");
                            lines.Add("</TD>");
                            lines.Add("</TR>");
                        }
                    }

                    if (builder)
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD colspan=3 align=center>");
                        lines.Add("<A href=section:" + acIndex + ",new>add a section</A>");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }
                }
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            return lines;
        }

        private static List<string> get_custom_token(CustomToken ct)
        {
            var lines = new List<string>();

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD>");
            lines.Add("<B>" + Process(ct.Name, true) + "</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>");
            lines.Add(ct.Details != "" ? Process(ct.Details, true) : "(no details)");
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (ct.TerrainPower != null)
            {
                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add(Process("Includes the terrain power \"" + ct.TerrainPower.Name + "\"", true));
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");

            if (ct.TerrainPower != null)
            {
                lines.Add("<BR>");
                lines.AddRange(get_terrain_power(ct.TerrainPower));
            }

            lines.Add("</BODY>");

            lines.Add("</HTML>");
            lines.Add("</P>");

            return lines;
        }

        private static List<string> get_terrain_power(TerrainPower tp)
        {
            var lines = new List<string>();

            if (tp != null)
            {
                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>" + Process(tp.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("<TD>");
                lines.Add(tp.Type == TerrainPowerType.AtWill ? "At-Will Terrain" : "Single-Use Terrain");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (tp.FlavourText != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD class=readaloud colspan=2>");
                    lines.Add(Process(tp.FlavourText, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (tp.Requirement != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD colspan=2>");
                    lines.Add("<B>Requirement</B> " + Process(tp.Requirement, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (tp.Check != "")
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD colspan=2>");
                    lines.Add("<B>Check</B> " + Process(tp.Check, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (tp.Success != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD colspan=2>");
                    lines.Add("<B>Success</B> " + Process(tp.Success, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (tp.Failure != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD colspan=2>");
                    lines.Add("<B>Failure</B> " + Process(tp.Failure, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (tp.Attack != "")
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD colspan=2>");
                    lines.Add("<B>Attack</B> " + Process(tp.Attack, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (tp.Target != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD colspan=2>");
                    lines.Add("<B>Target</B> " + Process(tp.Target, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (tp.Hit != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD colspan=2>");
                    lines.Add("<B>Hit</B> " + Process(tp.Hit, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (tp.Miss != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD colspan=2>");
                    lines.Add("<B>Miss</B> " + Process(tp.Miss, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (tp.Effect != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD colspan=2>");
                    lines.Add("<B>Effect</B> " + Process(tp.Effect, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }
            else
            {
                lines.Add("<P class=instruction>(none)</P>");
            }

            return lines;
        }

        private static List<string> get_parcels(PlotPoint pp, bool links)
        {
            var lines = new List<string>();

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD>");
            lines.Add("<B>Treasure Parcels</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            foreach (var parcel in pp.Parcels)
            {
                MagicItem item = null;
                if (parcel.MagicItemId != Guid.Empty)
                    item = Session.FindMagicItem(parcel.MagicItemId, SearchType.Global);

                var name = parcel.Name != "" ? Process(parcel.Name, true) : "(undefined parcel)";
                if (links && item != null)
                    name = "<A href=\"item:" + item.Id + "\">" + name + "</A>";

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>" + name + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (parcel.Details != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add(Process(parcel.Details, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (parcel.MagicItemId != Guid.Empty)
                    if (item != null)
                    {
                        var lib = Session.FindLibrary(item);
                        if (lib != null)
                        {
                            if (Session.Project != null && Session.Project.Library == lib)
                            {
                                // Don't show this
                            }
                            else
                            {
                                lines.Add("<TR>");
                                lines.Add("<TD>");
                                lines.Add(Process(lib.Name, true));
                                lines.Add("</TD>");
                                lines.Add("</TR>");
                            }
                        }
                    }
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            return lines;
        }

        private static List<string> get_player_option(IPlayerOption option)
        {
            var lines = new List<string>();

            if (option is Race)
            {
                var race = option as Race;

                if (race.Quote != null && race.Quote != "")
                {
                    lines.Add("<P class=readaloud>");
                    lines.Add(Process(race.Quote, true));
                    lines.Add("</P>");
                }

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>" + Process(race.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (race.HeightRange != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Average Height</B> " + Process(race.HeightRange, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (race.WeightRange != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Average Weight</B> " + Process(race.WeightRange, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (race.AbilityScores != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Ability Scores</B> " + Process(race.AbilityScores, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>Size</B> " + race.Size);
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (race.Speed != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Speed</B> " + Process(race.Speed, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (race.Vision != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Vision</B> " + Process(race.Vision, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (race.Languages != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Languages</B> " + Process(race.Languages, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (race.SkillBonuses != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Skill Bonuses</B> " + Process(race.SkillBonuses, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                foreach (var ft in race.Features)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>" + Process(ft.Name, true) + "</B> " + Process(ft.Details, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");

                foreach (var power in race.Powers)
                    lines.AddRange(get_player_power(power, 0));

                if (race.Details != "")
                {
                    lines.Add("<P>");
                    lines.Add(Process(race.Details, true));
                    lines.Add("</P>");
                }
            }

            if (option is Class)
            {
                var c = option as Class;

                if (c.Quote != null && c.Quote != "")
                {
                    lines.Add("<P class=readaloud>");
                    lines.Add(Process(c.Quote, true));
                    lines.Add("</P>");
                }

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>" + Process(c.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (c.Role != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Role</B> " + Process(c.Role, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (c.PowerSource != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Power Source</B> " + Process(c.PowerSource, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (c.KeyAbilities != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Key Abilities</B> " + Process(c.KeyAbilities, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (c.ArmourProficiencies != "")
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>Armour Proficiencies</B> " + Process(c.ArmourProficiencies, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (c.WeaponProficiencies != "")
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>Weapon Proficiencies</B> " + Process(c.WeaponProficiencies, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (c.Implements != "")
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>Implements</B> " + Process(c.Implements, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (c.DefenceBonuses != "")
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>Defence Bonuses</B> " + Process(c.DefenceBonuses, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>Hit Points at 1st Level</B> " + c.HpFirst + " + Constitution score");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>HP per Level Gained</B> " + c.HpSubsequent);
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>Healing Surges per Day</B> " + c.HealingSurges + " + Constitution modifier");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (c.TrainedSkills != "")
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>Trained Skills</B> " + Process(c.TrainedSkills, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                var classFeatures = "";
                foreach (var ft in c.FeatureData.Features)
                {
                    if (classFeatures != "")
                        classFeatures += ", ";

                    classFeatures += ft.Name;
                }

                if (classFeatures != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Class Features</B> " + Process(classFeatures, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");

                if (c.Description != "")
                {
                    lines.Add("<P>");
                    lines.Add(Process(c.Description, true));
                    lines.Add("</P>");
                }

                if (c.OverviewCharacteristics != "" || c.OverviewReligion != "" || c.OverviewRaces != "")
                {
                    lines.Add("<P class=table>");
                    lines.Add("<TABLE>");

                    lines.Add("<TR class=heading>");
                    lines.Add("<TD>");
                    lines.Add("<B>Overview</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    if (c.OverviewCharacteristics != "")
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add("<B>Characteristics</B> " + Process(c.OverviewCharacteristics, true));
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }

                    if (c.OverviewReligion != "")
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add("<B>Religion</B> " + Process(c.OverviewReligion, true));
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }

                    if (c.OverviewRaces != "")
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add("<B>Races</B> " + Process(c.OverviewRaces, true));
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }

                    lines.Add("</TABLE>");
                    lines.Add("</P>");
                }

                if (c.FeatureData.Features.Count != 0)
                {
                    lines.Add("<H4>Class Features</H4>");

                    foreach (var ft in c.FeatureData.Features)
                    {
                        lines.Add("<P class=table>");
                        lines.Add("<TABLE>");

                        lines.Add("<TR class=heading>");
                        lines.Add("<TD>");
                        lines.Add("<B>" + Process(ft.Name, true) + "</B>");
                        lines.Add("</TD>");
                        lines.Add("</TR>");

                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add(Process(ft.Details, true));
                        lines.Add("</TD>");
                        lines.Add("</TR>");

                        lines.Add("</TABLE>");
                        lines.Add("</P>");
                    }
                }

                foreach (var pwr in c.FeatureData.Powers)
                    lines.AddRange(get_player_power(pwr, 0));

                foreach (var ld in c.Levels)
                    if (ld.Powers.Count != 0)
                    {
                        lines.Add("<H4>Level " + ld.Level + " Powers</H4>");

                        foreach (var pwr in ld.Powers)
                            lines.AddRange(get_player_power(pwr, ld.Level));
                    }
            }

            if (option is Theme)
            {
                var theme = option as Theme;

                if (theme.Quote != null && theme.Quote != "")
                {
                    lines.Add("<P class=readaloud>");
                    lines.Add(Process(theme.Quote, true));
                    lines.Add("</P>");
                }

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>" + Process(theme.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (theme.Prerequisites != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Prerequisites</B> " + Process(theme.Prerequisites, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (theme.SecondaryRole != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Secondary Role</B> " + Process(theme.SecondaryRole, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (theme.PowerSource != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Power Source</B> " + Process(theme.PowerSource, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>Granted Power</B> " + Process(theme.GrantedPower.Name, true));
                lines.Add("</TD>");
                lines.Add("</TR>");

                foreach (var ld in theme.Levels)
                foreach (var ft in ld.Features)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>" + Process(ft.Name, true) + " (level " + ld.Level + ")</B> " +
                              Process(ft.Details, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");

                lines.AddRange(get_player_power(theme.GrantedPower, 0));

                foreach (var ld in theme.Levels)
                foreach (var power in ld.Powers)
                    lines.AddRange(get_player_power(power, ld.Level));

                if (theme.Details != "")
                {
                    lines.Add("<P>");
                    lines.Add(Process(theme.Details, true));
                    lines.Add("</P>");
                }
            }

            if (option is ParagonPath)
            {
                var pp = option as ParagonPath;

                if (pp.Quote != null && pp.Quote != "")
                {
                    lines.Add("<P class=readaloud>");
                    lines.Add(Process(pp.Quote, true));
                    lines.Add("</P>");
                }

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>" + Process(pp.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (pp.Prerequisites != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Prerequisites</B> " + Process(pp.Prerequisites, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                foreach (var ld in pp.Levels)
                foreach (var ft in ld.Features)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>" + Process(ft.Name, true) + " (level " + ld.Level + ")</B> " +
                              Process(ft.Details, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");

                foreach (var ld in pp.Levels)
                foreach (var power in ld.Powers)
                    lines.AddRange(get_player_power(power, ld.Level));

                if (pp.Details != "")
                {
                    lines.Add("<P>");
                    lines.Add(Process(pp.Details, true));
                    lines.Add("</P>");
                }
            }

            if (option is EpicDestiny)
            {
                var ed = option as EpicDestiny;

                if (ed.Quote != null && ed.Quote != "")
                {
                    lines.Add("<P class=readaloud>");
                    lines.Add(Process(ed.Quote, true));
                    lines.Add("</P>");
                }

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>" + Process(ed.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (ed.Prerequisites != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Prerequisites</B> " + Process(ed.Prerequisites, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                foreach (var ld in ed.Levels)
                foreach (var ft in ld.Features)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>" + Process(ft.Name, true) + " (level " + ld.Level + ")</B> " +
                              Process(ft.Details, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");

                foreach (var ld in ed.Levels)
                foreach (var power in ld.Powers)
                    lines.AddRange(get_player_power(power, ld.Level));

                if (ed.Details != "")
                {
                    lines.Add("<P>");
                    lines.Add(Process(ed.Details, true));
                    lines.Add("</P>");
                }

                if (ed.Immortality != "")
                {
                    lines.Add("<P>");
                    lines.Add("<B>Immortality</B> " + Process(ed.Immortality, true));
                    lines.Add("</P>");
                }
            }

            if (option is PlayerBackground)
            {
                var bg = option as PlayerBackground;

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>" + Process(bg.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (bg.Details != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add(Process(bg.Details, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (bg.AssociatedSkills != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Associated Skills</B> " + Process(bg.AssociatedSkills, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (bg.RecommendedFeats != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Recommended Feats</B> " + Process(bg.RecommendedFeats, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            if (option is Feat)
            {
                var feat = option as Feat;

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>" + Process(feat.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (feat.Prerequisites != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Prerequisites</B> " + Process(feat.Prerequisites, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (feat.Benefits != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Benefit</B> " + Process(feat.Benefits, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            if (option is Weapon)
            {
                var wpn = option as Weapon;

                var info = wpn.Type + " " + wpn.Category;
                info += wpn.TwoHanded ? " two-handed weapon" : " one-handed weapon";

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=item>");
                lines.Add("<TD>");
                lines.Add("<B>" + Process(wpn.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add(info);
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>Proficiency</B> +" + wpn.Proficiency);
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (wpn.Damage != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Damage</B> " + wpn.Damage);
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (wpn.Range != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Range</B> " + wpn.Range);
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (wpn.Price != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Price</B> " + wpn.Price);
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (wpn.Weight != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Weight</B> " + wpn.Weight);
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (wpn.Group != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Group</B> " + wpn.Group);
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (wpn.Properties != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Properties</B> " + wpn.Properties);
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");

                if (wpn.Description != "")
                    lines.Add("<P>" + Process(wpn.Description, true) + "</P>");
            }

            if (option is Ritual)
            {
                var ritual = option as Ritual;

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>" + Process(ritual.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (ritual.ReadAloud != "")
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD class=readaloud>");
                    lines.Add(Process(ritual.ReadAloud, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>Level</B> " + ritual.Level);
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>Category</B> " + ritual.Category);
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (ritual.Time != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Time</B> " + Process(ritual.Time, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (ritual.Duration != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Duration</B> " + Process(ritual.Duration, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (ritual.ComponentCost != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Component Cost</B> " + Process(ritual.ComponentCost, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (ritual.MarketPrice != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Market Price</B> " + Process(ritual.MarketPrice, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (ritual.KeySkill != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("<B>Key Skill</B> " + Process(ritual.KeySkill, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                if (ritual.Details != "")
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add(Process(ritual.Details, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            if (option is CreatureLore)
            {
                var lore = option as CreatureLore;

                lines.Add("<H3>" + Process(lore.Name, true) + " Lore</H3>");

                lines.Add("<P>");
                lines.Add("A character knows the following information with a successful <B>" + lore.SkillName +
                          "</B> check:");
                lines.Add("</P>");

                lines.Add("<UL>");
                foreach (var info in lore.Information)
                {
                    lines.Add("<LI>");
                    lines.Add("<B>DC " + info.First + "</B>: " + info.Second);
                    lines.Add("</LI>");
                }

                lines.Add("</UL>");
            }

            if (option is Disease)
            {
                var disease = option as Disease;

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=trap>");
                lines.Add("<TD colspan=2>");
                lines.Add("<B>" + Process(disease.Name, true) + "</B>");
                lines.Add("</TD>");
                lines.Add("<TD>");
                if (disease.Level != "")
                    lines.Add("Level " + disease.Level + " Disease");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (disease.Details != "")
                {
                    lines.Add("<TR>");
                    lines.Add("<TD class=readaloud colspan=3>");
                    lines.Add(Process(disease.Details, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                // Stages
                if (disease.Levels.Count != 0)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("<B>Stages</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("<B>Cured</B>: The target is cured.");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    foreach (var level in disease.Levels)
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD colspan=3>");

                        if (disease.Levels.Count > 1)
                        {
                            var index = disease.Levels.IndexOf(level);
                            if (index == 0)
                                lines.Add("<B>Initial state</B>:");
                            if (index == disease.Levels.Count - 1)
                                lines.Add("<B>Final state</B>:");
                        }

                        lines.Add(Process(level, true));

                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }
                }

                // DCs
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Check</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Maintain</B>: DC " + Process(disease.MaintainDc, true));
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD colspan=3>");
                lines.Add("<B>Improve</B>: DC " + Process(disease.ImproveDc, true));
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            if (option is Poison)
            {
                var poison = option as Poison;

                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                lines.Add("<TR class=trap>");
                lines.Add("<TD>");
                lines.Add("<B>" + Process(poison.Name, true) + "</B> (level " + poison.Level + ")");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (poison.Details != "")
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD class=readaloud>");
                    lines.Add(Process(poison.Details, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                var price = Treasure.GetItemValue(poison.Level) / 4;
                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>Price</B>: " + price + " gp");
                lines.Add("</TD>");
                lines.Add("</TR>");

                foreach (var section in poison.Sections)
                {
                    lines.Add("<TR>");
                    if (section.Indent == 0)
                    {
                        lines.Add("<TD>");
                    }
                    else
                    {
                        var padding = section.Indent * 15;
                        lines.Add("<TD style=\"padding-left=" + padding + "px\">");
                    }

                    lines.Add("<B>" + Process(section.Header, true) + "</B> " + Process(section.Details, true));
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            return lines;
        }

        private static List<string> get_player_power(PlayerPower power, int level)
        {
            var lines = new List<string>();

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            var name = Process(power.Name, true);
            if (name == "")
                name = "(unnamed power)";

            var header = "<B>" + name + "</B>";
            if (level != 0)
                header += " (level " + level + ")";

            switch (power.Type)
            {
                case PlayerPowerType.AtWill:
                    lines.Add("<TR class=atwill>");
                    break;
                case PlayerPowerType.Encounter:
                    lines.Add("<TR class=encounter>");
                    break;
                case PlayerPowerType.Daily:
                    lines.Add("<TR class=daily>");
                    break;
            }

            lines.Add("<TD>");
            lines.Add(header);
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (power.ReadAloud != "")
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD class=readaloud>");
                lines.Add(Process(power.ReadAloud, true));
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var line1 = power.Type.ToString();
            if (power.Keywords != "")
                line1 += " &diams; " + Process(power.Keywords, true);

            lines.Add("<TR>");
            lines.Add("<TD>");
            lines.Add(line1);
            lines.Add("</TD>");
            lines.Add("</TR>");

            var line2 = "<B>Action</B> " + power.Action;
            if (power.Range != "")
                line2 += "; <B>Range</B> " + Process(power.Range, true);

            lines.Add("<TR>");
            lines.Add("<TD>");
            lines.Add(line2);
            lines.Add("</TD>");
            lines.Add("</TR>");

            foreach (var section in power.Sections)
            {
                lines.Add("<TR>");
                if (section.Indent == 0)
                {
                    lines.Add("<TD>");
                }
                else
                {
                    var padding = section.Indent * 15;
                    lines.Add("<TD style=\"padding-left=" + padding + "px\">");
                }

                lines.Add("<B>" + Process(section.Header, true) + "</B> " + Process(section.Details, true));
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            return lines;
        }

        private void add_map(Guid mapId, Guid areaId)
        {
            if (mapId == Guid.Empty)
                return;

            if (!_fMaps.ContainsKey(mapId))
                _fMaps[mapId] = new List<Guid>();

            if (!_fMaps[mapId].Contains(areaId))
                _fMaps[mapId].Add(areaId);
        }

        private string get_filename(string itemName, string extension, bool fullPath)
        {
            var cleaned = itemName;

            var prohibited = new List<string>();
            prohibited.Add("\\");
            prohibited.Add("/");
            prohibited.Add(":");
            prohibited.Add("*");
            prohibited.Add("?");
            prohibited.Add("\"");
            prohibited.Add("<");
            prohibited.Add(">");
            prohibited.Add("|");

            foreach (var bad in prohibited)
                cleaned = cleaned.Replace(bad, "");

            var result = (fullPath ? _fFullPath : _fRelativePath) + cleaned + "." + extension;

            if (!fullPath)
                result = result.Replace(" ", "%20");

            return result;
        }

        private string get_map_name(Guid mapId, Guid areaId)
        {
            var m = Session.Project.FindTacticalMap(mapId);
            if (m == null)
                return "";

            if (areaId == Guid.Empty)
            {
                return m.Name;
            }

            var area = m.FindArea(areaId);
            return m.Name + " - " + area.Name;
        }

        private static string get_time(TimeSpan ts)
        {
            return ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
        }
    }
}
