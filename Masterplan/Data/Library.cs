using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a library.
    /// </summary>
    [Serializable]
    public class Library : IComparable<Library>
    {
        private List<Artifact> _fArtifacts = new List<Artifact>();

        private List<Creature> _fCreatures = new List<Creature>();

        private Guid _fId = Guid.NewGuid();

        private List<MagicItem> _fMagicItems = new List<MagicItem>();

        private string _fName = "";

        private List<SkillChallenge> _fSkillChallenges = new List<SkillChallenge>();

        private List<CreatureTemplate> _fTemplates = new List<CreatureTemplate>();

        private List<TerrainPower> _fTerrainPowers = new List<TerrainPower>();

        private List<MonsterTheme> _fThemes = new List<MonsterTheme>();

        private List<Tile> _fTiles = new List<Tile>();

        private List<Trap> _fTraps = new List<Trap>();

        /// <summary>
        ///     Gets or sets the unique ID of the library.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the library.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the list of creatures in the library.
        /// </summary>
        public List<Creature> Creatures
        {
            get => _fCreatures;
            set => _fCreatures = value;
        }

        /// <summary>
        ///     Gets or sets the list of creature templates in the library.
        /// </summary>
        public List<CreatureTemplate> Templates
        {
            get => _fTemplates;
            set => _fTemplates = value;
        }

        /// <summary>
        ///     Gets or sets the list of monster themes in the library.
        /// </summary>
        public List<MonsterTheme> Themes
        {
            get => _fThemes;
            set => _fThemes = value;
        }

        /// <summary>
        ///     Gets or sets the list of traps in the library.
        /// </summary>
        public List<Trap> Traps
        {
            get => _fTraps;
            set => _fTraps = value;
        }

        /// <summary>
        ///     Gets or sets the list of skill challenges in the library.
        /// </summary>
        public List<SkillChallenge> SkillChallenges
        {
            get => _fSkillChallenges;
            set => _fSkillChallenges = value;
        }

        /// <summary>
        ///     Gets or sets the list of magic items in the library.
        /// </summary>
        public List<MagicItem> MagicItems
        {
            get => _fMagicItems;
            set => _fMagicItems = value;
        }

        /// <summary>
        ///     Gets or sets the list of artifacts in the library.
        /// </summary>
        public List<Artifact> Artifacts
        {
            get => _fArtifacts;
            set => _fArtifacts = value;
        }

        /// <summary>
        ///     Gets or sets the list of map tiles in the library.
        /// </summary>
        public List<Tile> Tiles
        {
            get => _fTiles;
            set => _fTiles = value;
        }

        /// <summary>
        ///     Gets or sets the list of terrain powers in the library.
        /// </summary>
        public List<TerrainPower> TerrainPowers
        {
            get => _fTerrainPowers;
            set => _fTerrainPowers = value;
        }

        /// <summary>
        ///     Get whether the library can be used for map autobuilding
        /// </summary>
        public bool ShowInAutoBuild
        {
            get
            {
                foreach (var t in _fTiles)
                {
                    if (t == null)
                        continue;

                    if (t.Category != TileCategory.Special && t.Category != TileCategory.Map)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Finds the creature with the given ID in this library.
        /// </summary>
        /// <param name="creature_id">The ID to search for.</param>
        /// <returns>Returns the creature if it exists; null otherwise.</returns>
        public Creature FindCreature(Guid creatureId)
        {
            foreach (var creature in _fCreatures)
            {
                if (creature == null)
                    continue;

                if (creature.Id == creatureId)
                    return creature;
            }

            return null;
        }

        /// <summary>
        ///     Finds the creature with the given name in this library.
        /// </summary>
        /// <param name="creature_name">The name to search for.</param>
        /// <returns>Returns the creature if it exists; null otherwise.</returns>
        public Creature FindCreature(string creatureName)
        {
            foreach (var creature in _fCreatures)
            {
                if (creature == null)
                    continue;

                if (creature.Name == creatureName)
                    return creature;
            }

            return null;
        }

        /// <summary>
        ///     Finds the creature with the given name in this library.
        /// </summary>
        /// <param name="creature_name">The name to search for.</param>
        /// <param name="level">The level of the creature to search for.</param>
        /// <returns>Returns the creature if it exists; null otherwise.</returns>
        public Creature FindCreature(string creatureName, int level)
        {
            foreach (var creature in _fCreatures)
            {
                if (creature == null)
                    continue;

                if (creature.Name == creatureName && creature.Level == level)
                    return creature;
            }

            return null;
        }

        /// <summary>
        ///     Finds the creature template with the given ID in this library.
        /// </summary>
        /// <param name="template_id">The ID to search for.</param>
        /// <returns>Returns the template if it exists; null otherwise.</returns>
        public CreatureTemplate FindTemplate(Guid templateId)
        {
            foreach (var template in _fTemplates)
            {
                if (template == null)
                    continue;

                if (template.Id == templateId)
                    return template;
            }

            return null;
        }

        /// <summary>
        ///     Finds the monster theme with the given ID in this library.
        /// </summary>
        /// <param name="theme_id">The ID to search for.</param>
        /// <returns>Returns the monster theme if it exists; null otherwise.</returns>
        public MonsterTheme FindTheme(Guid themeId)
        {
            foreach (var theme in _fThemes)
            {
                if (theme == null)
                    continue;

                if (theme.Id == themeId)
                    return theme;
            }

            return null;
        }

        /// <summary>
        ///     Finds the trap with the given ID in this library.
        /// </summary>
        /// <param name="trap_id">The ID to search for.</param>
        /// <returns>Returns the trap if it exists; null otherwise.</returns>
        public Trap FindTrap(Guid trapId)
        {
            foreach (var trap in _fTraps)
            {
                if (trap == null)
                    continue;

                if (trap.Id == trapId)
                    return trap;
            }

            return null;
        }

        /// <summary>
        ///     Finds the trap with the given name in this library.
        /// </summary>
        /// <param name="trap_name">The name to search for.</param>
        /// <returns>Returns the trap if it exists; null otherwise.</returns>
        public Trap FindTrap(string trapName)
        {
            foreach (var trap in _fTraps)
            {
                if (trap == null)
                    continue;

                if (trap.Name == trapName)
                    return trap;
            }

            return null;
        }

        /// <summary>
        ///     Finds the trap with the given name in this library.
        /// </summary>
        /// <param name="trap_name">The name to search for.</param>
        /// <param name="level">The level of the trap to search for.</param>
        /// <param name="role_string">The role of the trap to search for.</param>
        /// <returns>Returns the trap if it exists; null otherwise.</returns>
        public Trap FindTrap(string trapName, int level, string roleString)
        {
            foreach (var trap in _fTraps)
            {
                if (trap == null)
                    continue;

                if (trap.Name == trapName && trap.Level == level && trap.Role.ToString() == roleString)
                    return trap;
            }

            return null;
        }

        /// <summary>
        ///     Finds the skill challenge with the given ID in this library.
        /// </summary>
        /// <param name="sc_id">The ID to search for.</param>
        /// <returns>Returns the skill challenge if it exists; null otherwise.</returns>
        public SkillChallenge FindSkillChallenge(Guid scId)
        {
            foreach (var sc in _fSkillChallenges)
            {
                if (sc == null)
                    continue;

                if (sc.Id == scId)
                    return sc;
            }

            return null;
        }

        /// <summary>
        ///     Finds the magic item with the given ID in this library.
        /// </summary>
        /// <param name="item_id">The ID to search for.</param>
        /// <returns>Returns the item if it exists; null otherwise.</returns>
        public MagicItem FindMagicItem(Guid itemId)
        {
            foreach (var item in _fMagicItems)
            {
                if (item == null)
                    continue;

                if (item.Id == itemId)
                    return item;
            }

            return null;
        }

        /// <summary>
        ///     Finds the magic item with the given name in this library.
        /// </summary>
        /// <param name="item_name">The name to search for.</param>
        /// <returns>Returns the item if it exists; null otherwise.</returns>
        public MagicItem FindMagicItem(string itemName)
        {
            foreach (var item in _fMagicItems)
            {
                if (item == null)
                    continue;

                if (item.Name == itemName)
                    return item;
            }

            return null;
        }

        /// <summary>
        ///     Finds the magic item with the given ID in this library.
        /// </summary>
        /// <param name="item_name">The name to search for.</param>
        /// <param name="level">The level of the item to search for.</param>
        /// <returns>Returns the item if it exists; null otherwise.</returns>
        public MagicItem FindMagicItem(string itemName, int level)
        {
            foreach (var item in _fMagicItems)
            {
                if (item == null)
                    continue;

                if (item.Name == itemName && item.Level == level)
                    return item;
            }

            return null;
        }

        /// <summary>
        ///     Finds the artifact with the given ID in this library.
        /// </summary>
        /// <param name="item_id">The ID to search for.</param>
        /// <returns>Returns the artifact if it exists; null otherwise.</returns>
        public Artifact FindArtifact(Guid itemId)
        {
            foreach (var item in _fArtifacts)
            {
                if (item == null)
                    continue;

                if (item.Id == itemId)
                    return item;
            }

            return null;
        }

        /// <summary>
        ///     Finds the artifact with the given name in this library.
        /// </summary>
        /// <param name="item_name">The name to search for.</param>
        /// <returns>Returns the artifact if it exists; null otherwise.</returns>
        public Artifact FindArtifact(string itemName)
        {
            foreach (var item in _fArtifacts)
            {
                if (item == null)
                    continue;

                if (item.Name == itemName)
                    return item;
            }

            return null;
        }

        /// <summary>
        ///     Finds the map tile with the given ID in this library.
        /// </summary>
        /// <param name="tile_id">The ID to search for.</param>
        /// <returns>Returns the tile if it exists; null otherwise.</returns>
        public Tile FindTile(Guid tileId)
        {
            foreach (var t in _fTiles)
            {
                if (t == null)
                    continue;

                if (t.Id == tileId)
                    return t;
            }

            return null;
        }

        /// <summary>
        ///     Finds the terrain power with the given ID in this library.
        /// </summary>
        /// <param name="item_id">The ID to search for.</param>
        /// <returns>Returns the terrain power if it exists; null otherwise.</returns>
        public TerrainPower FindTerrainPower(Guid itemId)
        {
            foreach (var item in _fTerrainPowers)
            {
                if (item == null)
                    continue;

                if (item.Id == itemId)
                    return item;
            }

            return null;
        }

        /// <summary>
        ///     Finds the terrain power with the given name in this library.
        /// </summary>
        /// <param name="item_name">The name to search for.</param>
        /// <returns>Returns the terrain power if it exists; null otherwise.</returns>
        public TerrainPower FindTerrainPower(string itemName)
        {
            foreach (var item in _fTerrainPowers)
            {
                if (item == null)
                    continue;

                if (item.Name == itemName)
                    return item;
            }

            return null;
        }

        /// <summary>
        ///     Returns the name of the library.
        /// </summary>
        /// <returns>Returns the name of the library.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Updates the library to the latest format.
        /// </summary>
        public void Update()
        {
            if (_fId == Guid.Empty)
                _fId = Guid.NewGuid();

            foreach (var c in _fCreatures)
            {
                if (c == null)
                    continue;

                if (c.Category == null)
                    c.Category = "";

                if (c.Senses == null)
                    c.Senses = "";

                if (c.Movement == null)
                    c.Movement = "";

                if (c.Auras == null)
                    c.Auras = new List<Aura>();

                foreach (var aura in c.Auras)
                    if (aura.Keywords == null)
                        aura.Keywords = "";

                if (c.CreaturePowers == null)
                    c.CreaturePowers = new List<CreaturePower>();

                if (c.DamageModifiers == null)
                    c.DamageModifiers = new List<DamageModifier>();

                foreach (var cp in c.CreaturePowers)
                {
                    if (cp.Condition == null)
                        cp.Condition = "";

                    cp.ExtractAttackDetails();
                }

                CreatureHelper.UpdateRegen(c);
                foreach (var power in c.CreaturePowers)
                    CreatureHelper.UpdatePowerRange(c, power);

                if (c.Tactics == null)
                    c.Tactics = "";

                if (c.Image != null)
                    Program.SetResolution(c.Image);
            }

            foreach (var ct in _fTemplates)
            {
                if (ct == null)
                    continue;

                if (ct.Senses == null)
                    ct.Senses = "";

                if (ct.Movement == null)
                    ct.Movement = "";

                if (ct.Auras == null)
                    ct.Auras = new List<Aura>();

                foreach (var aura in ct.Auras)
                    if (aura.Keywords == null)
                        aura.Keywords = "";

                if (ct.CreaturePowers == null)
                    ct.CreaturePowers = new List<CreaturePower>();

                if (ct.DamageModifierTemplates == null)
                    ct.DamageModifierTemplates = new List<DamageModifierTemplate>();

                foreach (var cp in ct.CreaturePowers)
                {
                    if (cp.Condition == null)
                        cp.Condition = "";

                    cp.ExtractAttackDetails();
                }

                if (ct.Tactics == null)
                    ct.Tactics = "";
            }

            if (_fThemes == null)
                _fThemes = new List<MonsterTheme>();

            foreach (var mt in _fThemes)
            {
                if (mt == null)
                    continue;

                foreach (var tpd in mt.Powers)
                    tpd.Power.ExtractAttackDetails();
            }

            if (_fTraps == null)
                _fTraps = new List<Trap>();

            foreach (var t in _fTraps)
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

            if (_fSkillChallenges == null)
                _fSkillChallenges = new List<SkillChallenge>();

            foreach (var sc in _fSkillChallenges)
            {
                if (sc.Notes == null)
                    sc.Notes = "";

                foreach (var scd in sc.Skills)
                    if (scd.Results == null)
                        scd.Results = new SkillChallengeResult();
            }

            if (_fMagicItems == null)
                _fMagicItems = new List<MagicItem>();

            if (_fArtifacts == null)
                _fArtifacts = new List<Artifact>();

            foreach (var t in _fTiles)
            {
                Program.SetResolution(t.Image);

                if (t.Keywords == null)
                    t.Keywords = "";
            }

            if (_fTerrainPowers == null)
                _fTerrainPowers = new List<TerrainPower>();
        }

        /// <summary>
        ///     Imports information from another library.
        /// </summary>
        /// <param name="lib">The other library</param>
        public void Import(Library lib)
        {
            foreach (var c in lib.Creatures)
            {
                if (c == null)
                    continue;

                if (FindCreature(c.Id) == null)
                    _fCreatures.Add(c);
            }

            foreach (var ct in lib.Templates)
            {
                if (ct == null)
                    continue;

                if (FindTemplate(ct.Id) == null)
                    _fTemplates.Add(ct);
            }

            foreach (var mt in lib.Themes)
            {
                if (mt == null)
                    continue;

                if (FindTheme(mt.Id) == null)
                    _fThemes.Add(mt);
            }

            foreach (var t in lib.Traps)
            {
                if (t == null)
                    continue;

                if (FindTrap(t.Id) == null)
                    _fTraps.Add(t);
            }

            foreach (var sc in lib.SkillChallenges)
            {
                if (sc == null)
                    continue;

                if (FindSkillChallenge(sc.Id) == null)
                    _fSkillChallenges.Add(sc);
            }

            foreach (var item in lib.MagicItems)
            {
                if (item == null)
                    continue;

                if (FindMagicItem(item.Id) == null)
                    _fMagicItems.Add(item);
            }

            foreach (var a in lib.Artifacts)
            {
                if (a == null)
                    continue;

                if (FindArtifact(a.Id) == null)
                    _fArtifacts.Add(a);
            }

            foreach (var t in lib.Tiles)
            {
                if (t == null)
                    continue;

                if (FindTile(t.Id) == null)
                    _fTiles.Add(t);
            }
        }

        /// <summary>
        ///     Compares this library to another.
        /// </summary>
        /// <param name="rhs">The other library.</param>
        /// <returns>Returns -1 if this library should be sorted before rhs,+1 if rhs should be sorted before this, 0 otherwise.</returns>
        public int CompareTo(Library rhs)
        {
            return _fName.CompareTo(rhs.Name);
        }
    }
}
