using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a Masterplan project.
    /// </summary>
    [Serializable]
    public class Project
    {
        private Dictionary<string, string> _fAddInData = new Dictionary<string, string>();

        private List<Attachment> _fAttachments = new List<Attachment>();

        private string _fAuthor = "";

        private List<Background> _fBackgrounds = new List<Background>();

        private List<Calendar> _fCalendars = new List<Calendar>();

        private CampaignSettings _fCampaignSettings = new CampaignSettings();

        private List<CustomCreature> _fCustomCreatures = new List<CustomCreature>();

        private List<EncounterDeck> _fDecks = new List<EncounterDeck>();

        private Encyclopedia _fEncyclopedia = new Encyclopedia();

        private List<Hero> _fHeroes = new List<Hero>();

        private List<Hero> _fInactiveHeroes = new List<Hero>();

        private Library _fLibrary = new Library();

        private List<Map> _fMaps = new List<Map>();

        private string _fName = "";

        private List<Note> _fNotes = new List<Note>();

        private List<Npc> _fNpCs = new List<Npc>();

        private Party _fParty = new Party();

        private string _fPassword = "";

        private string _fPasswordHint = "";

        private List<IPlayerOption> _fPlayerOptions = new List<IPlayerOption>();

        private Plot _fPlot = new Plot();

        private List<RegionalMap> _fRegionalMaps = new List<RegionalMap>();

        private List<CombatState> _fSavedCombats = new List<CombatState>();

        private List<Parcel> _fTreasureParcels = new List<Parcel>();

        /// <summary>
        ///     Gets or sets the name of the project.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the name of the author of the project.
        /// </summary>
        public string Author
        {
            get => _fAuthor;
            set => _fAuthor = value;
        }

        /// <summary>
        ///     Gets or sets the size and level of the party the project is designed for.
        /// </summary>
        public Party Party
        {
            get => _fParty;
            set => _fParty = value;
        }

        /// <summary>
        ///     Gets or sets the list of PCs.
        /// </summary>
        public List<Hero> Heroes
        {
            get => _fHeroes;
            set => _fHeroes = value;
        }

        /// <summary>
        ///     Gets or sets the list of PCs.
        /// </summary>
        public List<Hero> InactiveHeroes
        {
            get => _fInactiveHeroes;
            set => _fInactiveHeroes = value;
        }

        /// <summary>
        ///     Gets or sets the plot information.
        /// </summary>
        public Plot Plot
        {
            get => _fPlot;
            set => _fPlot = value;
        }

        /// <summary>
        ///     Gets or sets the project encyclopedia.
        /// </summary>
        public Encyclopedia Encyclopedia
        {
            get => _fEncyclopedia;
            set => _fEncyclopedia = value;
        }

        /// <summary>
        ///     Gets or sets the list of notes.
        /// </summary>
        public List<Note> Notes
        {
            get => _fNotes;
            set => _fNotes = value;
        }

        /// <summary>
        ///     Gets or sets the list of tactical maps.
        /// </summary>
        public List<Map> Maps
        {
            get => _fMaps;
            set => _fMaps = value;
        }

        /// <summary>
        ///     Gets or sets the list of tactical maps.
        /// </summary>
        public List<RegionalMap> RegionalMaps
        {
            get => _fRegionalMaps;
            set => _fRegionalMaps = value;
        }

        /// <summary>
        ///     Gets or sets the list of encounter decks.
        /// </summary>
        public List<EncounterDeck> Decks
        {
            get => _fDecks;
            set => _fDecks = value;
        }

        /// <summary>
        ///     Gets or sets the list of NPCs.
        /// </summary>
        public List<Npc> NpCs
        {
            get => _fNpCs;
            set => _fNpCs = value;
        }

        /// <summary>
        ///     Gets or sets the list of project-specific creatures.
        /// </summary>
        public List<CustomCreature> CustomCreatures
        {
            get => _fCustomCreatures;
            set => _fCustomCreatures = value;
        }

        /// <summary>
        ///     Gets or sets the project calendars.
        /// </summary>
        public List<Calendar> Calendars
        {
            get => _fCalendars;
            set => _fCalendars = value;
        }

        /// <summary>
        ///     Gets or sets the project attachments.
        /// </summary>
        public List<Attachment> Attachments
        {
            get => _fAttachments;
            set => _fAttachments = value;
        }

        /// <summary>
        ///     Gets or sets the list of project background information items.
        /// </summary>
        public List<Background> Backgrounds
        {
            get => _fBackgrounds;
            set => _fBackgrounds = value;
        }

        /// <summary>
        ///     Gets or sets the list of unassigned treasure parcels.
        /// </summary>
        public List<Parcel> TreasureParcels
        {
            get => _fTreasureParcels;
            set => _fTreasureParcels = value;
        }

        /// <summary>
        ///     Gets or sets the list of player options.
        /// </summary>
        public List<IPlayerOption> PlayerOptions
        {
            get => _fPlayerOptions;
            set => _fPlayerOptions = value;
        }

        /// <summary>
        ///     Gets or sets the list of saved encounters.
        /// </summary>
        public List<CombatState> SavedCombats
        {
            get => _fSavedCombats;
            set => _fSavedCombats = value;
        }

        /// <summary>
        ///     Gets or sets add-in specific data.
        ///     Data should be stored with add-in name as the key and custom data as the value.
        /// </summary>
        public Dictionary<string, string> AddInData
        {
            get => _fAddInData;
            set => _fAddInData = value;
        }

        /// <summary>
        ///     Gets or sets the campaign settings.
        /// </summary>
        public CampaignSettings CampaignSettings
        {
            get => _fCampaignSettings;
            set => _fCampaignSettings = value;
        }

        /// <summary>
        ///     The project's password.
        /// </summary>
        public string Password
        {
            get => _fPassword;
            set => _fPassword = value;
        }

        /// <summary>
        ///     The project's password hint.
        /// </summary>
        public string PasswordHint
        {
            get => _fPasswordHint;
            set => _fPasswordHint = value;
        }

        /// <summary>
        ///     Gets a list containing all the plot points in the project.
        /// </summary>
        public List<PlotPoint> AllPlotPoints
        {
            get
            {
                var points = new List<PlotPoint>();

                foreach (var pp in _fPlot.Points)
                    points.AddRange(pp.Subtree);

                return points;
            }
        }

        /// <summary>
        ///     Gets a list containing all the treasure parcels in the project.
        /// </summary>
        public List<Parcel> AllTreasureParcels
        {
            get
            {
                var parcels = new List<Parcel>();

                parcels.AddRange(_fTreasureParcels);

                var points = AllPlotPoints;
                foreach (var point in points)
                    parcels.AddRange(point.Parcels);

                return parcels;
            }
        }

        /// <summary>
        ///     Gets or sets the internal library.
        /// </summary>
        public Library Library
        {
            get => _fLibrary;
            set => _fLibrary = value;
        }

        /// <summary>
        ///     Finds the tactical map with the specified ID.
        /// </summary>
        /// <param name="map_id">The ID to search for.</param>
        /// <returns>Returns the map if it exists; null otherwise.</returns>
        public Map FindTacticalMap(Guid mapId)
        {
            foreach (var m in _fMaps)
                if (m.Id == mapId)
                    return m;

            return null;
        }

        /// <summary>
        ///     Finds the regional map with the specified ID.
        /// </summary>
        /// <param name="map_id">The ID to search for.</param>
        /// <returns>Returns the map if it exists; null otherwise.</returns>
        public RegionalMap FindRegionalMap(Guid mapId)
        {
            foreach (var m in _fRegionalMaps)
                if (m.Id == mapId)
                    return m;

            return null;
        }

        /// <summary>
        ///     Finds the encounter deck with the specified ID.
        /// </summary>
        /// <param name="deck_id">The ID to search for.</param>
        /// <returns>Returns the deck if it exists; null otherwise.</returns>
        public EncounterDeck FindDeck(Guid deckId)
        {
            foreach (var d in _fDecks)
                if (d.Id == deckId)
                    return d;

            return null;
        }

        /// <summary>
        ///     Finds the NPC with the specified ID.
        /// </summary>
        /// <param name="npc_id">The ID to search for.</param>
        /// <returns>Returns the NPC if it exists; null otherwise.</returns>
        public Npc FindNpc(Guid npcId)
        {
            foreach (var npc in _fNpCs)
                if (npc.Id == npcId)
                    return npc;

            return null;
        }

        /// <summary>
        ///     Finds the custom creature with the specified ID.
        /// </summary>
        /// <param name="creature_id">The ID to search for.</param>
        /// <returns>Returns the creature if it exists; null otherwise.</returns>
        public CustomCreature FindCustomCreature(Guid creatureId)
        {
            foreach (var cc in _fCustomCreatures)
                if (cc.Id == creatureId)
                    return cc;

            return null;
        }

        /// <summary>
        ///     Finds the custom creature with the specified ID.
        /// </summary>
        /// <param name="creature_name">The ID to search for.</param>
        /// <returns>Returns the creature if it exists; null otherwise.</returns>
        public CustomCreature FindCustomCreature(string creatureName)
        {
            foreach (var cc in _fCustomCreatures)
                if (cc.Name == creatureName)
                    return cc;

            return null;
        }

        /// <summary>
        ///     Finds the calendar with the specified ID.
        /// </summary>
        /// <param name="calendar_id">The ID to search for.</param>
        /// <returns>Returns the calendar if it exists; null otherwise.</returns>
        public Calendar FindCalendar(Guid calendarId)
        {
            foreach (var c in _fCalendars)
                if (c.Id == calendarId)
                    return c;

            return null;
        }

        /// <summary>
        ///     Finds the note with the specified ID.
        /// </summary>
        /// <param name="note_id">The ID to search for.</param>
        /// <returns>Returns the note if it exists; null otherwise.</returns>
        public Note FindNote(Guid noteId)
        {
            foreach (var n in _fNotes)
                if (n.Id == noteId)
                    return n;

            return null;
        }

        /// <summary>
        ///     Finds the attachment with the specified name.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>Returns the attachment if it exists; null otherwise.</returns>
        public Attachment FindAttachment(string name)
        {
            foreach (var att in _fAttachments)
                if (att.Name == name)
                    return att;

            return null;
        }

        /// <summary>
        ///     Finds the background with the specified title.
        /// </summary>
        /// <param name="title">The title to search for.</param>
        /// <returns>Returns the background if it exists; null otherwise.</returns>
        public Background FindBackground(string title)
        {
            foreach (var bg in _fBackgrounds)
                if (bg.Title == title)
                    return bg;

            return null;
        }

        /// <summary>
        ///     Finds the hero with the specified ID.
        /// </summary>
        /// <param name="hero_id">The ID to search for.</param>
        /// <returns>Returns the hero if it exists; null otherwise.</returns>
        public Hero FindHero(Guid heroId)
        {
            foreach (var h in _fHeroes)
                if (h.Id == heroId)
                    return h;

            foreach (var h in _fInactiveHeroes)
                if (h.Id == heroId)
                    return h;

            return null;
        }

        /// <summary>
        ///     Finds the hero with the specified name.
        /// </summary>
        /// <param name="hero_name">The name to search for.</param>
        /// <returns>Returns the hero if it exists; null otherwise.</returns>
        public Hero FindHero(string heroName)
        {
            foreach (var h in _fHeroes)
                if (h.Name == heroName)
                    return h;

            foreach (var h in _fInactiveHeroes)
                if (h.Name == heroName)
                    return h;

            return null;
        }

        /// <summary>
        ///     Finds the player option with the specified ID.
        /// </summary>
        /// <param name="option_id">The ID to search for.</param>
        /// <returns>Returns the player option if it exists; null otherwise.</returns>
        public IPlayerOption FindPlayerOption(Guid optionId)
        {
            foreach (var option in _fPlayerOptions)
                if (option.Id == optionId)
                    return option;

            return null;
        }

        /// <summary>
        ///     Find the plot point which is the parent of the specified plot.
        /// </summary>
        /// <param name="p">The plot.</param>
        /// <returns>Returns the parent plot point, if it exists; null otherwise.</returns>
        public PlotPoint FindParent(Plot p)
        {
            var allPoints = new List<PlotPoint>();
            get_all_points(Session.Project.Plot, allPoints);

            foreach (var pp in allPoints)
                if (pp.Subplot == p)
                    return pp;

            return null;
        }

        private void get_all_points(Plot p, List<PlotPoint> points)
        {
            var children = p != null ? p.Points : Session.Project.Plot.Points;
            foreach (var child in children)
            {
                points.Add(child);
                get_all_points(child.Subplot, points);
            }
        }

        /// <summary>
        ///     Find the plot which is the parent of the specified plot point.
        /// </summary>
        /// <param name="pp">The plot point.</param>
        /// <returns>Returns the parent plot, if it exists; false otherwise.</returns>
        public Plot FindParent(PlotPoint pp)
        {
            var allPlots = new List<Plot>();
            get_all_plots(Session.Project.Plot, allPlots);

            foreach (var p in allPlots)
            foreach (var point in p.Points)
                if (point.Id == pp.Id)
                    return p;

            return null;
        }

        private void get_all_plots(Plot p, List<Plot> plots)
        {
            plots.Add(p);

            var children = p != null ? p.Points : Session.Project.Plot.Points;
            foreach (var child in children) get_all_plots(child.Subplot, plots);
        }

        /// <summary>
        ///     Updates the project to make sure it conforms to the current structure
        /// </summary>
        public void Update()
        {
            _fLibrary.Update();

            if (_fPassword == null)
                _fPassword = "";

            if (_fPasswordHint == null)
                _fPasswordHint = "";

            if (_fParty.Xp == 0)
                _fParty.Xp = Experience.GetHeroXp(_fParty.Level);

            if (_fAuthor == null)
                _fAuthor = "";

            if (_fRegionalMaps == null)
                _fRegionalMaps = new List<RegionalMap>();

            foreach (var map in _fRegionalMaps)
            {
                foreach (var loc in map.Locations)
                    if (loc.Category == null)
                        loc.Category = "";

                Program.SetResolution(map.Image);
            }

            foreach (var hero in _fHeroes)
            {
                if (hero.Level == 0)
                    hero.Level = _fParty.Level;

                if (hero.Effects == null)
                    hero.Effects = new List<OngoingCondition>();
                foreach (var oc in hero.Effects)
                {
                    if (oc.Defences == null)
                        oc.Defences = new List<DefenceType>();
                    if (oc.DamageModifier == null)
                        oc.DamageModifier = new DamageModifier();
                    if (oc.Regeneration == null)
                        oc.Regeneration = new Regeneration();
                    if (oc.Aura == null)
                        oc.Aura = new Aura();
                }

                if (hero.Tokens == null)
                    hero.Tokens = new List<CustomToken>();
                foreach (var ct in hero.Tokens)
                    if (ct.TerrainPower != null && ct.TerrainPower.Id == Guid.Empty)
                        ct.Id = Guid.NewGuid();

                if (hero.Portrait != null)
                    Program.SetResolution(hero.Portrait);

                if (hero.CombatData == null)
                    hero.CombatData = new CombatData();
            }

            if (_fInactiveHeroes == null)
                _fInactiveHeroes = new List<Hero>();

            foreach (var hero in _fInactiveHeroes)
            {
                if (hero.Effects == null)
                    hero.Effects = new List<OngoingCondition>();
                foreach (var oc in hero.Effects)
                {
                    if (oc.Defences == null)
                        oc.Defences = new List<DefenceType>();
                    if (oc.DamageModifier == null)
                        oc.DamageModifier = new DamageModifier();
                    if (oc.Regeneration == null)
                        oc.Regeneration = new Regeneration();
                }

                if (hero.Tokens == null)
                    hero.Tokens = new List<CustomToken>();
                foreach (var ct in hero.Tokens)
                    if (ct.TerrainPower != null && ct.TerrainPower.Id == Guid.Empty)
                        ct.Id = Guid.NewGuid();

                if (hero.Portrait != null)
                    Program.SetResolution(hero.Portrait);

                if (hero.CombatData == null)
                    hero.CombatData = new CombatData();
            }

            if (_fNpCs == null)
                _fNpCs = new List<Npc>();

            while (_fNpCs.Contains(null))
                _fNpCs.Remove(null);
            foreach (var npc in _fNpCs)
            {
                if (npc == null)
                    continue;

                if (npc.Auras == null)
                    npc.Auras = new List<Aura>();

                foreach (var aura in npc.Auras)
                    if (aura.Keywords == null)
                        aura.Keywords = "";

                if (npc.CreaturePowers == null)
                    npc.CreaturePowers = new List<CreaturePower>();

                CreatureHelper.UpdateRegen(npc);
                foreach (var power in npc.CreaturePowers)
                    CreatureHelper.UpdatePowerRange(npc, power);

                if (npc.Tactics == null)
                    npc.Tactics = "";

                if (npc.Image != null)
                    Program.SetResolution(npc.Image);
            }

            while (_fCustomCreatures.Contains(null))
                _fCustomCreatures.Remove(null);
            foreach (var cc in _fCustomCreatures)
            {
                if (cc == null)
                    continue;

                if (cc.Auras == null)
                    cc.Auras = new List<Aura>();

                foreach (var aura in cc.Auras)
                    if (aura.Keywords == null)
                        aura.Keywords = "";

                if (cc.CreaturePowers == null)
                    cc.CreaturePowers = new List<CreaturePower>();

                if (cc.DamageModifiers == null)
                    cc.DamageModifiers = new List<DamageModifier>();

                CreatureHelper.UpdateRegen(cc);
                foreach (var power in cc.CreaturePowers)
                    CreatureHelper.UpdatePowerRange(cc, power);

                if (cc.Tactics == null)
                    cc.Tactics = "";

                if (cc.Image != null)
                    Program.SetResolution(cc.Image);
            }

            if (_fCalendars == null)
                _fCalendars = new List<Calendar>();

            if (_fEncyclopedia == null)
                _fEncyclopedia = new Encyclopedia();

            while (_fEncyclopedia.Entries.Contains(null))
                _fEncyclopedia.Entries.Remove(null);
            foreach (var entry in _fEncyclopedia.Entries)
            {
                if (entry.Category == null)
                    entry.Category = "";

                if (entry.DmInfo == null)
                    entry.DmInfo = "";

                if (entry.Images == null)
                    entry.Images = new List<EncyclopediaImage>();

                foreach (var img in entry.Images)
                    Program.SetResolution(img.Image);
            }

            if (_fEncyclopedia.Groups == null)
                _fEncyclopedia.Groups = new List<EncyclopediaGroup>();

            if (_fNotes == null)
                _fNotes = new List<Note>();

            foreach (var n in _fNotes)
                if (n.Category == null)
                    n.Category = "";

            if (_fAttachments == null)
                _fAttachments = new List<Attachment>();

            if (_fBackgrounds == null)
            {
                _fBackgrounds = new List<Background>();
                SetStandardBackgroundItems();
            }

            if (_fTreasureParcels == null)
                _fTreasureParcels = new List<Parcel>();

            if (_fPlayerOptions == null)
                _fPlayerOptions = new List<IPlayerOption>();

            if (_fSavedCombats == null)
                _fSavedCombats = new List<CombatState>();
            foreach (var cs in _fSavedCombats)
            {
                if (cs.Encounter.Waves == null)
                    cs.Encounter.Waves = new List<EncounterWave>();

                if (cs.Sketches == null)
                    cs.Sketches = new List<MapSketch>();

                if (cs.Log == null)
                    cs.Log = new EncounterLog();

                foreach (var oc in cs.QuickEffects)
                {
                    if (oc.Defences == null)
                        oc.Defences = new List<DefenceType>();
                    if (oc.DamageModifier == null)
                        oc.DamageModifier = new DamageModifier();
                    if (oc.Regeneration == null)
                        oc.Regeneration = new Regeneration();
                    if (oc.Aura == null)
                        oc.Aura = new Aura();
                }
            }

            if (_fAddInData == null)
                _fAddInData = new Dictionary<string, string>();

            if (_fCampaignSettings == null)
                _fCampaignSettings = new CampaignSettings();

            if (_fCampaignSettings.Xp == 0)
                _fCampaignSettings.Xp = 1;

            update_plot(_fPlot);
        }

        private void update_plot(Plot p)
        {
            foreach (var pp in p.Points)
            {
                if (pp.ReadAloud == null)
                    pp.ReadAloud = "";

                if (pp.Parcels == null)
                    pp.Parcels = new List<Parcel>();

                if (pp.EncyclopediaEntryIDs == null)
                    pp.EncyclopediaEntryIDs = new List<Guid>();

                if (pp.Element is Encounter)
                {
                    var enc = pp.Element as Encounter;

                    if (enc.Traps == null)
                        enc.Traps = new List<Trap>();

                    foreach (var t in enc.Traps)
                    {
                        if (t.Description == null)
                            t.Description = "";

                        if (t.Attacks == null)
                            t.Attacks = new List<TrapAttack>();
                        if (t.Attack != null)
                        {
                            t.Attacks.Add(t.Attack);
                            t.Initiative = t.Attack.HasInitiative ? t.Attack.Initiative : int.MinValue;
                            t.Trigger = t.Attack.Trigger;
                            t.Attack = null;
                        }

                        foreach (var ta in t.Attacks)
                        {
                            if (ta.Id == Guid.Empty)
                                ta.Id = Guid.NewGuid();

                            if (ta.Name == null)
                                ta.Name = "Attack";

                            if (ta.Keywords == null)
                                ta.Keywords = "";

                            if (ta.Notes == null)
                                ta.Notes = "";
                        }

                        if (t.Trigger == null)
                            t.Trigger = "";

                        foreach (var tsd in t.Skills)
                            if (tsd.Id == Guid.Empty)
                                tsd.Id = Guid.NewGuid();
                    }

                    if (enc.SkillChallenges == null)
                        enc.SkillChallenges = new List<SkillChallenge>();

                    foreach (var sc in enc.SkillChallenges)
                    {
                        if (sc.Notes == null)
                            sc.Notes = "";

                        foreach (var scd in sc.Skills)
                            if (scd.Results == null)
                                scd.Results = new SkillChallengeResult();
                    }

                    if (enc.CustomTokens == null)
                        enc.CustomTokens = new List<CustomToken>();
                    foreach (var ct in enc.CustomTokens)
                        if (ct.TerrainPower != null && ct.TerrainPower.Id == Guid.Empty)
                            ct.TerrainPower.Id = Guid.NewGuid();

                    if (enc.Notes == null)
                    {
                        enc.Notes = new List<EncounterNote>();
                        enc.SetStandardEncounterNotes();
                    }

                    if (enc.Waves == null)
                        enc.Waves = new List<EncounterWave>();

                    foreach (var slot in enc.AllSlots)
                    {
                        slot.SetDefaultDisplayNames();

                        foreach (var data in slot.CombatData)
                        {
                            data.Initiative = int.MinValue;

                            if (data.Id == Guid.Empty)
                                data.Id = Guid.NewGuid();

                            if (data.UsedPowers == null)
                                data.UsedPowers = new List<Guid>();
                        }
                    }
                }

                if (pp.Element is SkillChallenge)
                {
                    var sc = pp.Element as SkillChallenge;

                    if (sc.Id == Guid.Empty)
                        sc.Id = Guid.NewGuid();

                    if (sc.Name == null)
                        sc.Name = "Skill Challenge";

                    if (sc.Level <= 0)
                        sc.Level = _fParty.Level;

                    if (sc.Notes == null)
                        sc.Notes = "";

                    foreach (var scd in sc.Skills)
                    {
                        if (scd.Difficulty == Difficulty.Random)
                            scd.Difficulty = Difficulty.Moderate;

                        if (scd.Results == null)
                            scd.Results = new SkillChallengeResult();
                    }
                }

                if (pp.Element is TrapElement)
                {
                    var te = pp.Element as TrapElement;

                    if (te.Trap.Description == null)
                        te.Trap.Description = "";

                    if (te.Trap.Attacks == null)
                        te.Trap.Attacks = new List<TrapAttack>();
                    if (te.Trap.Attack != null)
                    {
                        te.Trap.Attacks.Add(te.Trap.Attack);
                        te.Trap.Initiative = te.Trap.Attack.HasInitiative ? te.Trap.Attack.Initiative : int.MinValue;
                        te.Trap.Trigger = te.Trap.Attack.Trigger;
                        te.Trap.Attack = null;
                    }

                    foreach (var ta in te.Trap.Attacks)
                    {
                        if (ta.Id == Guid.Empty)
                            ta.Id = Guid.NewGuid();

                        if (ta.Name == null)
                            ta.Name = "Attack";

                        if (ta.Keywords == null)
                            ta.Keywords = "";

                        if (ta.Notes == null)
                            ta.Notes = "";
                    }

                    if (te.Trap.Trigger == null)
                        te.Trap.Trigger = "";

                    foreach (var tsd in te.Trap.Skills)
                        if (tsd.Id == Guid.Empty)
                            tsd.Id = Guid.NewGuid();
                }

                if (pp.Element is Quest)
                {
                    var q = pp.Element as Quest;
                    if (q.Type == QuestType.Minor)
                        if (q.Xp == 0)
                        {
                            var range = Experience.GetMinorQuestXp(q.Level);
                            q.Xp = range.First;
                        }
                }

                update_plot(pp.Subplot);
            }
        }

        /// <summary>
        ///     Imports data from another project into this one.
        /// </summary>
        /// <param name="p">The other project.</param>
        public void Import(Project p)
        {
            p.Update();

            // Plots
            var pp = new PlotPoint(p.Name);
            pp.Subplot = p.Plot;
            _fPlot.Points.Add(pp);

            // Encyclopedia
            _fEncyclopedia.Import(p.Encyclopedia);

            // Notes
            _fNotes.AddRange(p.Notes);

            // Maps
            _fMaps.AddRange(p.Maps);
            _fRegionalMaps.AddRange(p.RegionalMaps);

            // Decks
            _fDecks.AddRange(p.Decks);

            // NPCs
            _fNpCs.AddRange(p.NpCs);

            // Custom creatures
            _fCustomCreatures.AddRange(p.CustomCreatures);

            // Calendars
            _fCalendars.AddRange(p.Calendars);

            // Attachments
            _fAttachments.AddRange(p.Attachments);

            // Player options
            _fPlayerOptions.AddRange(p.PlayerOptions);

            // Backgrounds
            foreach (var bg in p.Backgrounds)
            {
                if (bg.Details == "")
                    continue;

                var source = FindBackground(bg.Title);
                if (source == null)
                {
                    _fBackgrounds.AddRange(p.Backgrounds);
                }
                else
                {
                    if (source.Details != "")
                        source.Details += Environment.NewLine;

                    source.Details += bg.Details;
                }
            }

            // Update the internal library
            PopulateProjectLibrary();
            _fLibrary.Import(p.Library);
            SimplifyProjectLibrary();
        }

        /// <summary>
        ///     Removes any data from the project's internal library that exists in the external libraries.
        /// </summary>
        public void SimplifyProjectLibrary()
        {
            // Update creatures
            var obsCreatures = new List<Creature>();
            foreach (var c in _fLibrary.Creatures)
            {
                if (c == null)
                    continue;

                var fromLib = Session.FindCreature(c.Id, SearchType.External);

                if (fromLib is Creature)
                    obsCreatures.Add(fromLib as Creature);
            }

            foreach (var c in obsCreatures)
                _fLibrary.Creatures.Remove(c);

            // Update templates
            var obsTemplates = new List<CreatureTemplate>();
            foreach (var ct in _fLibrary.Templates)
            {
                if (ct == null)
                    continue;

                var fromLib = Session.FindTemplate(ct.Id, SearchType.External);

                if (fromLib != null)
                    obsTemplates.Add(ct);
            }

            foreach (var ct in obsTemplates)
                _fLibrary.Templates.Remove(ct);

            // Update themes
            var obsThemes = new List<MonsterTheme>();
            foreach (var mt in _fLibrary.Themes)
            {
                if (mt == null)
                    continue;

                var fromLib = Session.FindTheme(mt.Id, SearchType.External);

                if (fromLib != null)
                    obsThemes.Add(mt);
            }

            foreach (var mt in obsThemes)
                _fLibrary.Themes.Remove(mt);

            // Update traps
            var obsTraps = new List<Trap>();
            foreach (var t in _fLibrary.Traps)
            {
                if (t == null)
                    continue;

                var fromLib = Session.FindTrap(t.Id, SearchType.External);

                if (fromLib != null)
                    obsTraps.Add(t);
            }

            foreach (var t in obsTraps)
                _fLibrary.Traps.Remove(t);

            // Update skill challenges
            var obsChallenges = new List<SkillChallenge>();
            foreach (var sc in _fLibrary.SkillChallenges)
            {
                if (sc == null)
                    continue;

                var fromLib = Session.FindSkillChallenge(sc.Id, SearchType.External);

                if (fromLib != null)
                    obsChallenges.Add(sc);
            }

            foreach (var sc in obsChallenges)
                _fLibrary.SkillChallenges.Remove(sc);

            // Update magic items
            var obsItems = new List<MagicItem>();
            foreach (var item in _fLibrary.MagicItems)
            {
                if (item == null)
                    continue;

                var fromLib = Session.FindMagicItem(item.Id, SearchType.External);

                if (fromLib != null)
                    obsItems.Add(item);
            }

            foreach (var item in obsItems)
                _fLibrary.MagicItems.Remove(item);

            // Update tiles
            var obsTiles = new List<Tile>();
            foreach (var t in _fLibrary.Tiles)
            {
                if (t == null)
                    continue;

                var fromLib = Session.FindTile(t.Id, SearchType.External);

                if (fromLib != null)
                    obsTiles.Add(t);
            }

            foreach (var t in obsTiles)
                _fLibrary.Tiles.Remove(t);
        }

        /// <summary>
        ///     Populates the project's internal library with all the data required for the project.
        /// </summary>
        public void PopulateProjectLibrary()
        {
            // Build lists of data in the project
            var creatureIds = new List<Guid>();
            var templateIds = new List<Guid>();
            var themeIds = new List<Guid>();
            var trapIds = new List<Guid>();
            var challengeIds = new List<Guid>();
            var magicItemIds = new List<Guid>();

            foreach (var pp in _fPlot.Points)
                add_data(pp, creatureIds, templateIds, themeIds, trapIds, challengeIds, magicItemIds);

            foreach (var deck in _fDecks)
                add_data(deck, creatureIds, templateIds, themeIds);

            foreach (var npc in _fNpCs)
                add_data(npc, templateIds);

            populate_creatures(creatureIds);
            populate_templates(templateIds);
            populate_themes(themeIds);
            populate_traps(trapIds);
            populate_challenges(challengeIds);
            populate_magic_items(magicItemIds);
            populate_tiles();
        }

        private void add_data(PlotPoint pp, List<Guid> creatureIds, List<Guid> templateIds, List<Guid> themeIds,
            List<Guid> trapIds, List<Guid> challengeIds, List<Guid> magicItemIds)
        {
            // Add creatures and templates
            if (pp.Element is Encounter)
            {
                var enc = pp.Element as Encounter;

                foreach (var slot in enc.Slots)
                    add_data(slot.Card, creatureIds, templateIds, themeIds);

                foreach (var t in enc.Traps)
                    if (!trapIds.Contains(t.Id))
                        trapIds.Add(t.Id);

                foreach (var sc in enc.SkillChallenges)
                    if (!challengeIds.Contains(sc.Id))
                        challengeIds.Add(sc.Id);
            }

            if (pp.Element is SkillChallenge)
            {
                var sc = pp.Element as SkillChallenge;
                if (!challengeIds.Contains(sc.Id))
                    challengeIds.Add(sc.Id);
            }

            if (pp.Element is Trap)
            {
                var trap = pp.Element as Trap;
                if (!trapIds.Contains(trap.Id))
                    trapIds.Add(trap.Id);
            }

            foreach (var p in pp.Parcels)
                if (p.MagicItemId != Guid.Empty)
                    if (!magicItemIds.Contains(p.MagicItemId))
                        magicItemIds.Add(p.MagicItemId);

            // Recurse through children
            foreach (var child in pp.Subplot.Points)
                add_data(child, creatureIds, templateIds, themeIds, trapIds, challengeIds, magicItemIds);
        }

        private void add_data(EncounterDeck deck, List<Guid> creatureIds, List<Guid> templateIds, List<Guid> themeIds)
        {
            foreach (var card in deck.Cards)
                add_data(card, creatureIds, templateIds, themeIds);
        }

        private void add_data(EncounterCard card, List<Guid> creatureIds, List<Guid> templateIds, List<Guid> themeIds)
        {
            if (!creatureIds.Contains(card.CreatureId))
                creatureIds.Add(card.CreatureId);

            foreach (var templateId in card.TemplateIDs)
                if (!templateIds.Contains(templateId))
                    templateIds.Add(templateId);

            if (card.ThemeId != Guid.Empty)
                if (!themeIds.Contains(card.ThemeId))
                    themeIds.Add(card.ThemeId);
        }

        private void add_data(Npc npc, List<Guid> templateIds)
        {
            if (!templateIds.Contains(npc.TemplateId))
                templateIds.Add(npc.TemplateId);
        }

        private void populate_creatures(List<Guid> creatureIds)
        {
            // Remove creatures in the library that aren't in any encounter
            var obsolete = new List<Creature>();
            foreach (var t in _fLibrary.Creatures)
                if (t == null || !creatureIds.Contains(t.Id))
                    obsolete.Add(t);
            foreach (var t in obsolete) _fLibrary.Creatures.Remove(t);

            // Add creatures that aren't there
            foreach (var creatureId in creatureIds)
                if (_fLibrary.FindCreature(creatureId) == null)
                {
                    var t = Session.FindCreature(creatureId, SearchType.Global);
                    if (t != null)
                        _fLibrary.Creatures.Add(t as Creature);
                }
        }

        private void populate_templates(List<Guid> templateIds)
        {
            // Remove templates in the library that aren't in any encounter
            var obsolete = new List<CreatureTemplate>();
            foreach (var ct in _fLibrary.Templates)
                if (ct == null || !templateIds.Contains(ct.Id))
                    obsolete.Add(ct);
            foreach (var t in obsolete) _fLibrary.Templates.Remove(t);

            // Add templates that aren't there
            foreach (var templateId in templateIds)
                if (_fLibrary.FindTemplate(templateId) == null)
                {
                    var ct = Session.FindTemplate(templateId, SearchType.Global);
                    if (ct != null)
                        _fLibrary.Templates.Add(ct);
                }
        }

        private void populate_themes(List<Guid> themeIds)
        {
            // Remove themes in the library that aren't in any encounter
            var obsolete = new List<MonsterTheme>();
            foreach (var t in _fLibrary.Themes)
                if (t == null || !themeIds.Contains(t.Id))
                    obsolete.Add(t);
            foreach (var mt in obsolete) _fLibrary.Themes.Remove(mt);

            // Add templates that aren't there
            foreach (var themeId in themeIds)
                if (_fLibrary.FindTheme(themeId) == null)
                {
                    var mt = Session.FindTheme(themeId, SearchType.Global);
                    if (mt != null)
                        _fLibrary.Themes.Add(mt);
                }
        }

        private void populate_traps(List<Guid> trapIds)
        {
            // Remove traps in the library that aren't used in the project
            var obsolete = new List<Trap>();
            foreach (var t in _fLibrary.Traps)
                if (t == null || !trapIds.Contains(t.Id))
                    obsolete.Add(t);
            foreach (var t in obsolete) _fLibrary.Traps.Remove(t);

            // Add traps that aren't there
            foreach (var trapId in trapIds)
                if (_fLibrary.FindTrap(trapId) == null)
                {
                    var t = Session.FindTrap(trapId, SearchType.Global);
                    if (t != null)
                        _fLibrary.Traps.Add(t);
                }
        }

        private void populate_challenges(List<Guid> challengeIds)
        {
            // Remove skill challenges in the library that aren't used in the project
            var obsolete = new List<SkillChallenge>();
            foreach (var sc in _fLibrary.SkillChallenges)
                if (sc == null || !challengeIds.Contains(sc.Id))
                    obsolete.Add(sc);
            foreach (var t in obsolete) _fLibrary.SkillChallenges.Remove(t);

            // Add skill challenges that aren't there
            foreach (var challengeId in challengeIds)
                if (_fLibrary.FindSkillChallenge(challengeId) == null)
                {
                    var sc = Session.FindSkillChallenge(challengeId, SearchType.Global);
                    if (sc != null)
                        _fLibrary.SkillChallenges.Add(sc);
                }
        }

        private void populate_magic_items(List<Guid> magicItemIds)
        {
            // Remove magic items in the library that aren't used in the project
            var obsolete = new List<MagicItem>();
            foreach (var item in _fLibrary.MagicItems)
                if (item == null || !magicItemIds.Contains(item.Id))
                    obsolete.Add(item);
            foreach (var item in obsolete) _fLibrary.MagicItems.Remove(item);

            // Add skill challenges that aren't there
            foreach (var itemId in magicItemIds)
                if (_fLibrary.FindMagicItem(itemId) == null)
                {
                    var item = Session.FindMagicItem(itemId, SearchType.Global);
                    if (item != null)
                        _fLibrary.MagicItems.Add(item);
                }
        }

        private void populate_tiles()
        {
            // Build tile list
            var tileIds = new List<Guid>();
            foreach (var m in _fMaps)
            foreach (var td in m.Tiles)
                if (!tileIds.Contains(td.TileId))
                    tileIds.Add(td.TileId);

            // Remove tilesets in the library that aren't in any encounter
            var obsolete = new List<Tile>();
            foreach (var t in _fLibrary.Tiles)
                if (t == null || !tileIds.Contains(t.Id))
                    obsolete.Add(t);
            foreach (var t in obsolete) _fLibrary.Tiles.Remove(t);

            // Add tiles that aren't there
            foreach (var tileId in tileIds)
                if (Session.FindTile(tileId, SearchType.Project) == null)
                {
                    var t = Session.FindTile(tileId, SearchType.External);
                    if (t != null)
                        _fLibrary.Tiles.Add(t);
                }
        }

        /// <summary>
        ///     Adds blank standard background items to the project.
        /// </summary>
        public void SetStandardBackgroundItems()
        {
            _fBackgrounds.Add(new Background("Introduction"));
            _fBackgrounds.Add(new Background("Adventure Synopsis"));
            _fBackgrounds.Add(new Background("Adventure Background"));
            _fBackgrounds.Add(new Background("DM Information"));
            _fBackgrounds.Add(new Background("Player Introduction"));
            _fBackgrounds.Add(new Background("Character Hooks"));
            _fBackgrounds.Add(new Background("Treasure Preparation"));
            _fBackgrounds.Add(new Background("Continuing the Story"));
        }
    }
}
