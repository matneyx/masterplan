using System;
using System.Collections.Generic;
using Masterplan.Data;

namespace Masterplan.Tools
{
    internal class HeroData
    {
        public ClassData Class;

        public RaceData Race;

        public HeroData(RaceData rd, ClassData cd)
        {
            Race = rd;
            Class = cd;
        }

        public static HeroData Create()
        {
            // Select a class candidate
            var classIndex = Session.Random.Next() % Sourcebook.Classes.Count;
            var selectedClass = Sourcebook.Classes[classIndex];

            // Select a race candidate
            var raceIndex = Session.Random.Next() % Sourcebook.Races.Count;
            var selectedRace = Sourcebook.Races[raceIndex];

            return new HeroData(selectedRace, selectedClass);
        }

        public Hero ConvertToHero()
        {
            var hero = new Hero();

            hero.Name = Race.Name + " " + Class.Name;
            hero.Class = Class.Name;
            hero.Role = Class.Role;
            hero.PowerSource = Class.PowerSource.ToString();
            hero.Race = Race.Name;

            return hero;
        }
    }

    internal class HeroGroup
    {
        public List<HeroData> Heroes = new List<HeroData>();

        public List<PowerSource> RequiredPowerSources
        {
            get
            {
                var powerSources = Enum.GetValues(typeof(PowerSource));

                var min = int.MaxValue;
                foreach (PowerSource powerSource in powerSources)
                {
                    var count = Count(powerSource);
                    if (count < min)
                        min = count;
                }

                var required = new List<PowerSource>();
                foreach (PowerSource powerSource in powerSources)
                {
                    var count = Count(powerSource);
                    if (count == min)
                        required.Add(powerSource);
                }

                return required;
            }
        }

        public List<PrimaryAbility> RequiredAbilities
        {
            get
            {
                var abilities = Enum.GetValues(typeof(PrimaryAbility));

                var min = int.MaxValue;
                foreach (PrimaryAbility ability in abilities)
                {
                    var count = Count(ability);
                    if (count < min)
                        min = count;
                }

                var required = new List<PrimaryAbility>();
                foreach (PrimaryAbility ability in abilities)
                {
                    var count = Count(ability);
                    if (count == min)
                        required.Add(ability);
                }

                return required;
            }
        }

        public List<HeroRoleType> RequiredRoles
        {
            get
            {
                var roles = Enum.GetValues(typeof(HeroRoleType));

                var min = int.MaxValue;
                foreach (HeroRoleType role in roles)
                {
                    var count = Count(role);
                    if (count < min)
                        min = count;
                }

                var required = new List<HeroRoleType>();
                foreach (HeroRoleType role in roles)
                {
                    if (role == HeroRoleType.Hybrid)
                        continue;

                    var count = Count(role);
                    if (count == min)
                        required.Add(role);
                }

                return required;
            }
        }

        public static HeroGroup CreateGroup(int size)
        {
            var group = new HeroGroup();

            var fails = 0;
            while (group.Heroes.Count != size)
            {
                var hero = group.Suggest();
                if (hero != null)
                    @group.Heroes.Add(hero);
                else
                    fails += 1;

                if (fails >= 100)
                    return CreateGroup(size - 1);
            }

            return group;
        }

        public HeroData Suggest()
        {
            // What type of class do we need?
            var powerSources = RequiredPowerSources;
            var abilities = RequiredAbilities;
            var roles = RequiredRoles;

            // Make up a class candidate list
            var classCandidates = Sourcebook.Filter(powerSources, abilities, roles);
            if (classCandidates.Count == 0)
            {
                // Try without the ability score restriction
                classCandidates = Sourcebook.Filter(powerSources, new List<PrimaryAbility>(), roles);

                if (classCandidates.Count == 0)
                    return null;
            }

            // Remove classes we already have
            var obsoleteClasses = new List<ClassData>();
            foreach (var cd in classCandidates)
                if (Contains(cd))
                    obsoleteClasses.Add(cd);
            if (obsoleteClasses.Count != classCandidates.Count)
                foreach (var cd in obsoleteClasses)
                    classCandidates.Remove(cd);

            // Select a class candidate
            var classIndex = Session.Random.Next() % classCandidates.Count;
            var selectedClass = classCandidates[classIndex];

            // Make up a race candidate list
            var raceCandidates = Sourcebook.Filter(selectedClass.KeyAbility);
            if (raceCandidates.Count == 0)
                return null;

            // Remove races we already have
            var obsoleteRaces = new List<RaceData>();
            foreach (var rd in raceCandidates)
                if (Contains(rd))
                    obsoleteRaces.Add(rd);
            if (obsoleteRaces.Count != raceCandidates.Count)
                foreach (var rd in obsoleteRaces)
                    raceCandidates.Remove(rd);

            // Select a race candidate
            var raceIndex = Session.Random.Next() % raceCandidates.Count;
            var selectedRace = raceCandidates[raceIndex];

            return new HeroData(selectedRace, selectedClass);
        }

        public bool Contains(ClassData cd)
        {
            foreach (var hero in Heroes)
                if (hero.Class == cd)
                    return true;

            return false;
        }

        public bool Contains(RaceData rd)
        {
            foreach (var hero in Heroes)
                if (hero.Race == rd)
                    return true;

            return false;
        }

        public int Count(PowerSource powerSource)
        {
            var count = 0;

            foreach (var hero in Heroes)
            {
                if (hero.Class == null)
                    continue;

                if (hero.Class.PowerSource == powerSource)
                    count += 1;
            }

            return count;
        }

        public int Count(PrimaryAbility keyAbility)
        {
            var count = 0;

            foreach (var hero in Heroes)
            {
                if (hero.Class == null)
                    continue;

                if (hero.Class.KeyAbility == keyAbility)
                    count += 1;
            }

            return count;
        }

        public int Count(HeroRoleType role)
        {
            var count = 0;

            foreach (var hero in Heroes)
            {
                if (hero.Class == null)
                    continue;

                if (hero.Class.Role == role)
                    count += 1;
            }

            return count;
        }
    }

    internal enum PowerSource
    {
        Martial,
        Arcane,
        Divine,
        Primal,
        Psionic,
        Shadow
    }

    internal enum PrimaryAbility
    {
        Strength,
        Constitution,
        Dexterity,
        Intelligence,
        Wisdom,
        Charisma
    }

    internal class ClassData
    {
        public PrimaryAbility KeyAbility = PrimaryAbility.Strength;

        public string Name = "";
        public PowerSource PowerSource = PowerSource.Martial;
        public HeroRoleType Role = HeroRoleType.Controller;

        public ClassData(string name, PowerSource powerSource, PrimaryAbility keyAbility, HeroRoleType role)
        {
            Name = name;
            PowerSource = powerSource;
            KeyAbility = keyAbility;
            Role = role;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    internal class RaceData
    {
        public List<PrimaryAbility> Abilities;

        public string Name = "";

        public RaceData(string name, PrimaryAbility[] abilities)
        {
            Name = name;
            Abilities = new List<PrimaryAbility>(abilities);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    internal class Sourcebook
    {
        private static List<ClassData> _allClasses;

        private static List<RaceData> _allRaces;

        public static List<ClassData> Classes
        {
            get
            {
                if (_allClasses == null)
                {
                    _allClasses = new List<ClassData>();

                    _allClasses.Add(new ClassData("Cleric", PowerSource.Divine, PrimaryAbility.Wisdom,
                        HeroRoleType.Leader));
                    _allClasses.Add(new ClassData("Cleric", PowerSource.Divine, PrimaryAbility.Strength,
                        HeroRoleType.Leader));
                    _allClasses.Add(new ClassData("Fighter", PowerSource.Martial, PrimaryAbility.Strength,
                        HeroRoleType.Defender));
                    _allClasses.Add(new ClassData("Paladin", PowerSource.Divine, PrimaryAbility.Strength,
                        HeroRoleType.Defender));
                    _allClasses.Add(new ClassData("Paladin", PowerSource.Divine, PrimaryAbility.Charisma,
                        HeroRoleType.Defender));
                    _allClasses.Add(new ClassData("Ranger", PowerSource.Martial, PrimaryAbility.Strength,
                        HeroRoleType.Striker));
                    _allClasses.Add(new ClassData("Ranger", PowerSource.Martial, PrimaryAbility.Dexterity,
                        HeroRoleType.Striker));
                    _allClasses.Add(new ClassData("Rogue", PowerSource.Martial, PrimaryAbility.Dexterity,
                        HeroRoleType.Striker));
                    _allClasses.Add(new ClassData("Warlock", PowerSource.Arcane, PrimaryAbility.Charisma,
                        HeroRoleType.Striker));
                    _allClasses.Add(new ClassData("Warlord", PowerSource.Martial, PrimaryAbility.Strength,
                        HeroRoleType.Leader));
                    _allClasses.Add(new ClassData("Wizard", PowerSource.Arcane, PrimaryAbility.Intelligence,
                        HeroRoleType.Controller));

                    _allClasses.Add(new ClassData("Avenger", PowerSource.Divine, PrimaryAbility.Wisdom,
                        HeroRoleType.Striker));
                    _allClasses.Add(new ClassData("Barbarian", PowerSource.Primal, PrimaryAbility.Strength,
                        HeroRoleType.Striker));
                    _allClasses.Add(new ClassData("Bard", PowerSource.Arcane, PrimaryAbility.Charisma,
                        HeroRoleType.Leader));
                    _allClasses.Add(new ClassData("Druid", PowerSource.Primal, PrimaryAbility.Wisdom,
                        HeroRoleType.Controller));
                    _allClasses.Add(new ClassData("Invoker", PowerSource.Divine, PrimaryAbility.Wisdom,
                        HeroRoleType.Controller));
                    _allClasses.Add(new ClassData("Shaman", PowerSource.Primal, PrimaryAbility.Wisdom,
                        HeroRoleType.Leader));
                    _allClasses.Add(new ClassData("Sorcerer", PowerSource.Arcane, PrimaryAbility.Charisma,
                        HeroRoleType.Striker));
                    _allClasses.Add(new ClassData("Warden", PowerSource.Primal, PrimaryAbility.Strength,
                        HeroRoleType.Defender));

                    _allClasses.Add(new ClassData("Ardent", PowerSource.Psionic, PrimaryAbility.Charisma,
                        HeroRoleType.Leader));
                    _allClasses.Add(new ClassData("Battlemind", PowerSource.Psionic, PrimaryAbility.Constitution,
                        HeroRoleType.Defender));
                    _allClasses.Add(new ClassData("Monk", PowerSource.Psionic, PrimaryAbility.Dexterity,
                        HeroRoleType.Striker));
                    _allClasses.Add(new ClassData("Psion", PowerSource.Psionic, PrimaryAbility.Intelligence,
                        HeroRoleType.Controller));
                    _allClasses.Add(new ClassData("Runepriest", PowerSource.Divine, PrimaryAbility.Strength,
                        HeroRoleType.Leader));
                    _allClasses.Add(new ClassData("Seeker", PowerSource.Primal, PrimaryAbility.Wisdom,
                        HeroRoleType.Controller));

                    _allClasses.Add(new ClassData("Artificer", PowerSource.Arcane, PrimaryAbility.Intelligence,
                        HeroRoleType.Leader));
                    _allClasses.Add(new ClassData("Assassin", PowerSource.Shadow, PrimaryAbility.Dexterity,
                        HeroRoleType.Striker));
                    _allClasses.Add(new ClassData("Swordmage", PowerSource.Arcane, PrimaryAbility.Intelligence,
                        HeroRoleType.Defender));
                    _allClasses.Add(new ClassData("Vampire", PowerSource.Shadow, PrimaryAbility.Dexterity,
                        HeroRoleType.Striker));
                }

                return _allClasses;
            }
        }

        public static List<RaceData> Races
        {
            get
            {
                if (_allRaces == null)
                {
                    _allRaces = new List<RaceData>();

                    _allRaces.Add(
                        new RaceData("Dragonborn", new[] { PrimaryAbility.Charisma, PrimaryAbility.Strength }));
                    _allRaces.Add(new RaceData("Dwarf", new[] { PrimaryAbility.Constitution, PrimaryAbility.Wisdom }));
                    _allRaces.Add(new RaceData("Eladrin",
                        new[] { PrimaryAbility.Dexterity, PrimaryAbility.Intelligence }));
                    _allRaces.Add(new RaceData("Elf", new[] { PrimaryAbility.Dexterity, PrimaryAbility.Wisdom }));
                    _allRaces.Add(new RaceData("Half-Elf",
                        new[] { PrimaryAbility.Charisma, PrimaryAbility.Constitution }));
                    _allRaces.Add(new RaceData("Halfling",
                        new[] { PrimaryAbility.Charisma, PrimaryAbility.Dexterity }));
                    _allRaces.Add(new RaceData("Human",
                        new[]
                        {
                            PrimaryAbility.Strength, PrimaryAbility.Constitution, PrimaryAbility.Dexterity,
                            PrimaryAbility.Intelligence, PrimaryAbility.Wisdom, PrimaryAbility.Charisma
                        }));
                    _allRaces.Add(new RaceData("Tiefling",
                        new[] { PrimaryAbility.Charisma, PrimaryAbility.Intelligence }));

                    _allRaces.Add(new RaceData("Deva", new[] { PrimaryAbility.Intelligence, PrimaryAbility.Wisdom }));
                    _allRaces.Add(new RaceData("Gnome",
                        new[] { PrimaryAbility.Charisma, PrimaryAbility.Intelligence }));
                    _allRaces.Add(new RaceData("Goliath",
                        new[] { PrimaryAbility.Constitution, PrimaryAbility.Strength }));
                    _allRaces.Add(new RaceData("Half-Orc",
                        new[] { PrimaryAbility.Dexterity, PrimaryAbility.Strength }));
                    _allRaces.Add(new RaceData("Longtooth Shifter",
                        new[] { PrimaryAbility.Strength, PrimaryAbility.Wisdom }));
                    _allRaces.Add(new RaceData("Razorclaw Shifter",
                        new[] { PrimaryAbility.Dexterity, PrimaryAbility.Wisdom }));

                    _allRaces.Add(new RaceData("Githzerai",
                        new[] { PrimaryAbility.Wisdom, PrimaryAbility.Dexterity, PrimaryAbility.Intelligence }));
                    _allRaces.Add(new RaceData("Minotaur",
                        new[] { PrimaryAbility.Strength, PrimaryAbility.Constitution, PrimaryAbility.Wisdom }));
                    _allRaces.Add(new RaceData("Shardmind",
                        new[] { PrimaryAbility.Intelligence, PrimaryAbility.Charisma, PrimaryAbility.Wisdom }));
                    _allRaces.Add(new RaceData("Wilden",
                        new[] { PrimaryAbility.Wisdom, PrimaryAbility.Constitution, PrimaryAbility.Dexterity }));

                    _allRaces.Add(new RaceData("Drow", new[] { PrimaryAbility.Charisma, PrimaryAbility.Dexterity }));
                    _allRaces.Add(
                        new RaceData("Genasi", new[] { PrimaryAbility.Intelligence, PrimaryAbility.Strength }));

                    _allRaces.Add(new RaceData("Changeling",
                        new[] { PrimaryAbility.Charisma, PrimaryAbility.Dexterity, PrimaryAbility.Intelligence }));
                    _allRaces.Add(new RaceData("Kalashtar", new[] { PrimaryAbility.Charisma, PrimaryAbility.Wisdom }));
                    _allRaces.Add(new RaceData("Warforged",
                        new[] { PrimaryAbility.Constitution, PrimaryAbility.Strength }));

                    _allRaces.Add(new RaceData("Revenant",
                        new[] { PrimaryAbility.Constitution, PrimaryAbility.Dexterity }));
                    _allRaces.Add(new RaceData("Shadar-kai",
                        new[] { PrimaryAbility.Dexterity, PrimaryAbility.Intelligence }));

                    _allRaces.Add(new RaceData("Shade", new[] { PrimaryAbility.Dexterity, PrimaryAbility.Charisma }));
                    _allRaces.Add(new RaceData("Vryloka", new[] { PrimaryAbility.Dexterity, PrimaryAbility.Charisma }));
                }

                return _allRaces;
            }
        }

        public static List<ClassData> Filter(List<PowerSource> powerSources, List<PrimaryAbility> abilities,
            List<HeroRoleType> roles)
        {
            var classes = new List<ClassData>();

            foreach (var cd in Classes)
            {
                if (powerSources.Count != 0 && !powerSources.Contains(cd.PowerSource))
                    continue;

                if (abilities.Count != 0 && !abilities.Contains(cd.KeyAbility))
                    continue;

                if (roles.Count != 0 && !roles.Contains(cd.Role))
                    continue;

                classes.Add(cd);
            }

            return classes;
        }

        public static List<RaceData> Filter(PrimaryAbility ability)
        {
            var races = new List<RaceData>();

            foreach (var rd in Races)
                if (rd.Abilities.Contains(ability))
                    races.Add(rd);

            return races;
        }

        public static ClassData GetClass(string name)
        {
            foreach (var cd in Classes)
                if (cd.Name == name)
                    return cd;

            return null;
        }

        public static RaceData GetRace(string name)
        {
            foreach (var rd in Races)
                if (rd.Name == name)
                    return rd;

            return null;
        }
    }
}
