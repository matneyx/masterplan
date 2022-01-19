using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Extensibility;
using Masterplan.Tools;
using Masterplan.UI;

namespace Masterplan
{
    internal enum SearchType
    {
        Project,
        External,
        Global
    }

    internal class Session
    {
        public static List<Library> Libraries = new List<Library>();
        public static Project Project { get; set; }


        public static Preferences Preferences { get; set; } = new Preferences();

        public static PlayerViewForm PlayerView { get; set; }

        public static bool Modified { get; set; }

        public static string FileName { get; set; } = "";

        public static Random Random { get; } = new Random();

        public static List<IAddIn> AddIns { get; } = new List<IAddIn>();


        public static string LibraryFolder
        {
            get
            {
                var ass = Assembly.GetEntryAssembly();
                return Tools.FileName.Directory(ass.Location) + "Libraries\\";
            }
        }

        public static List<Creature> Creatures
        {
            get
            {
                var list = new List<Creature>();

                foreach (var lib in Libraries)
                foreach (var item in lib.Creatures)
                {
                    if (item == null)
                        continue;

                    list.Add(item);
                }

                if (Project != null)
                {
                    var bst = new BinarySearchTree<Guid>();
                    foreach (var item in list)
                    {
                        if (item == null)
                            continue;

                        bst.Add(item.Id);
                    }

                    foreach (var item in Project.Library.Creatures)
                    {
                        if (item == null)
                            continue;

                        if (!bst.Contains(item.Id))
                            list.Add(item);
                    }
                }

                return list;
            }
        }

        public static List<CreatureTemplate> Templates
        {
            get
            {
                var list = new List<CreatureTemplate>();

                foreach (var lib in Libraries)
                foreach (var item in lib.Templates)
                {
                    if (item == null)
                        continue;

                    list.Add(item);
                }

                if (Project != null)
                {
                    var bst = new BinarySearchTree<Guid>();
                    foreach (var item in list)
                    {
                        if (item == null)
                            continue;

                        bst.Add(item.Id);
                    }

                    foreach (var item in Project.Library.Templates)
                    {
                        if (item == null)
                            continue;

                        if (!bst.Contains(item.Id))
                            list.Add(item);
                    }
                }

                return list;
            }
        }

        public static List<MonsterTheme> Themes
        {
            get
            {
                var list = new List<MonsterTheme>();

                foreach (var lib in Libraries)
                foreach (var item in lib.Themes)
                {
                    if (item == null)
                        continue;

                    list.Add(item);
                }

                if (Project != null)
                {
                    var bst = new BinarySearchTree<Guid>();
                    foreach (var item in list)
                    {
                        if (item == null)
                            continue;

                        bst.Add(item.Id);
                    }

                    foreach (var item in Project.Library.Themes)
                    {
                        if (item == null)
                            continue;

                        if (!bst.Contains(item.Id))
                            list.Add(item);
                    }
                }

                return list;
            }
        }

        public static List<Trap> Traps
        {
            get
            {
                var list = new List<Trap>();

                foreach (var lib in Libraries)
                foreach (var item in lib.Traps)
                {
                    if (item == null)
                        continue;

                    list.Add(item);
                }

                if (Project != null)
                {
                    var bst = new BinarySearchTree<Guid>();
                    foreach (var item in list)
                    {
                        if (item == null)
                            continue;

                        bst.Add(item.Id);
                    }

                    foreach (var item in Project.Library.Traps)
                    {
                        if (item == null)
                            continue;

                        if (!bst.Contains(item.Id))
                            list.Add(item);
                    }
                }

                return list;
            }
        }

        public static List<SkillChallenge> SkillChallenges
        {
            get
            {
                var list = new List<SkillChallenge>();

                foreach (var lib in Libraries)
                    list.AddRange(lib.SkillChallenges);

                if (Project != null)
                {
                    var bst = new BinarySearchTree<Guid>();
                    foreach (var item in list)
                    {
                        if (item == null)
                            continue;

                        bst.Add(item.Id);
                    }

                    foreach (var item in Project.Library.SkillChallenges)
                    {
                        if (item == null)
                            continue;

                        if (!bst.Contains(item.Id))
                            list.Add(item);
                    }
                }

                return list;
            }
        }

        public static List<MagicItem> MagicItems
        {
            get
            {
                var list = new List<MagicItem>();

                foreach (var lib in Libraries)
                foreach (var item in lib.MagicItems)
                {
                    if (item == null)
                        continue;

                    list.Add(item);
                }

                if (Project != null)
                {
                    var bst = new BinarySearchTree<Guid>();
                    foreach (var item in list)
                    {
                        if (item == null)
                            continue;

                        bst.Add(item.Id);
                    }

                    foreach (var item in Project.Library.MagicItems)
                    {
                        if (item == null)
                            continue;

                        if (!bst.Contains(item.Id))
                            list.Add(item);
                    }
                }

                return list;
            }
        }

        public static List<Artifact> Artifacts
        {
            get
            {
                var list = new List<Artifact>();

                foreach (var lib in Libraries)
                foreach (var item in lib.Artifacts)
                {
                    if (item == null)
                        continue;

                    list.Add(item);
                }

                if (Project != null)
                {
                    var bst = new BinarySearchTree<Guid>();
                    foreach (var item in list)
                    {
                        if (item == null)
                            continue;

                        bst.Add(item.Id);
                    }

                    foreach (var item in Project.Library.Artifacts)
                    {
                        if (item == null)
                            continue;

                        if (!bst.Contains(item.Id))
                            list.Add(item);
                    }
                }

                return list;
            }
        }

        public static List<Tile> Tiles
        {
            get
            {
                var list = new List<Tile>();

                foreach (var lib in Libraries)
                foreach (var item in lib.Tiles)
                {
                    if (item == null)
                        continue;

                    list.Add(item);
                }

                if (Project != null)
                {
                    var bst = new BinarySearchTree<Guid>();
                    foreach (var item in list)
                    {
                        if (item == null)
                            continue;

                        bst.Add(item.Id);
                    }

                    foreach (var item in Project.Library.Tiles)
                    {
                        if (item == null)
                            continue;

                        if (!bst.Contains(item.Id))
                            list.Add(item);
                    }
                }

                return list;
            }
        }

        public static List<TerrainPower> TerrainPowers
        {
            get
            {
                var list = new List<TerrainPower>();

                foreach (var lib in Libraries)
                foreach (var item in lib.TerrainPowers)
                {
                    if (item == null)
                        continue;

                    list.Add(item);
                }

                if (Project != null)
                {
                    var bst = new BinarySearchTree<Guid>();
                    foreach (var item in list)
                    {
                        if (item == null)
                            continue;

                        bst.Add(item.Id);
                    }

                    foreach (var item in Project.Library.TerrainPowers)
                    {
                        if (item == null)
                            continue;

                        if (!bst.Contains(item.Id))
                            list.Add(item);
                    }
                }

                return list;
            }
        }

        public static MainForm MainForm { get; set; }

        public static Encounter CurrentEncounter { get; set; }

        public static List<string> DisabledLibraries { get; set; } = new List<string>();

        public static string GetLibraryFilename(Library lib)
        {
            var di = new DirectoryInfo(LibraryFolder);

            var filename = Tools.FileName.TrimInvalidCharacters(lib.Name);

            return di + filename + ".library";
        }

        public static Library FindLibrary(string name)
        {
            var filename = Tools.FileName.TrimInvalidCharacters(name);

            foreach (var lib in Libraries)
            {
                if (lib.Name == name)
                    return lib;

                var libFilename = Tools.FileName.TrimInvalidCharacters(lib.Name);
                if (libFilename == filename)
                    return lib;
            }

            return null;
        }

        public static Library LoadLibrary(string filename)
        {
            try
            {
                if (Program.SplashScreen != null)
                {
                    Program.SplashScreen.CurrentSubAction = Tools.FileName.Name(filename);
                    Program.SplashScreen.Progress += 1;
                }

                var lib = Serialisation<Library>.Load(filename, SerialisationMode.Binary);
                if (lib != null)
                {
                    lib.Name = Tools.FileName.Name(filename);
                    lib.Update();

                    Libraries.Add(lib);
                }
                else
                {
                    LogSystem.Trace("Could not load " + Tools.FileName.Name(filename));
                }

                return lib;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return null;
        }

        public static void DeleteLibrary(Library lib)
        {
            // Delete library file
            var filename = GetLibraryFilename(lib);
            var fi = new FileInfo(filename);
            fi.Delete();

            Libraries.Remove(lib);
        }

        public static Library FindLibrary(Creature c)
        {
            if (c == null)
                return null;

            foreach (var lib in Libraries)
            foreach (var item in lib.Creatures)
            {
                if (item == null)
                    continue;

                if (item.Id == c.Id)
                    return lib;
            }

            if (Project != null)
                foreach (var item in Project.Library.Creatures)
                {
                    if (item == null)
                        continue;

                    if (item.Id == c.Id)
                        return Project.Library;
                }

            return null;
        }

        public static Library FindLibrary(CreatureTemplate ct)
        {
            if (ct == null)
                return null;

            foreach (var lib in Libraries)
            foreach (var item in lib.Templates)
            {
                if (item == null)
                    continue;

                if (item.Id == ct.Id)
                    return lib;
            }

            if (Project != null)
                foreach (var item in Project.Library.Templates)
                {
                    if (item == null)
                        continue;

                    if (item.Id == ct.Id)
                        return Project.Library;
                }

            return null;
        }

        public static Library FindLibrary(MonsterTheme mt)
        {
            if (mt == null)
                return null;

            foreach (var lib in Libraries)
            foreach (var item in lib.Themes)
            {
                if (item == null)
                    continue;

                if (item.Id == mt.Id)
                    return lib;
            }

            if (Project != null)
                foreach (var item in Project.Library.Themes)
                {
                    if (item == null)
                        continue;

                    if (item.Id == mt.Id)
                        return Project.Library;
                }

            return null;
        }

        public static Library FindLibrary(Trap t)
        {
            if (t == null)
                return null;

            foreach (var lib in Libraries)
            foreach (var item in lib.Traps)
            {
                if (item == null)
                    continue;

                if (item.Id == t.Id)
                    return lib;
            }

            if (Project != null)
                foreach (var item in Project.Library.Traps)
                {
                    if (item == null)
                        continue;

                    if (item.Id == t.Id)
                        return Project.Library;
                }

            return null;
        }

        public static Library FindLibrary(SkillChallenge sc)
        {
            if (sc == null)
                return null;

            foreach (var lib in Libraries)
            foreach (var item in lib.SkillChallenges)
            {
                if (item == null)
                    continue;

                if (item.Id == sc.Id)
                    return lib;
            }

            if (Project != null)
                foreach (var item in Project.Library.SkillChallenges)
                {
                    if (item == null)
                        continue;

                    if (item.Id == sc.Id)
                        return Project.Library;
                }

            return null;
        }

        public static Library FindLibrary(MagicItem mi)
        {
            if (mi == null)
                return null;

            foreach (var lib in Libraries)
            foreach (var item in lib.MagicItems)
            {
                if (item == null)
                    continue;

                if (item.Id == mi.Id)
                    return lib;
            }

            if (Project != null)
                foreach (var item in Project.Library.MagicItems)
                {
                    if (item == null)
                        continue;

                    if (item.Id == mi.Id)
                        return Project.Library;
                }

            return null;
        }

        public static Library FindLibrary(Artifact a)
        {
            if (a == null)
                return null;

            foreach (var lib in Libraries)
            foreach (var item in lib.Artifacts)
            {
                if (item == null)
                    continue;

                if (item.Id == a.Id)
                    return lib;
            }

            if (Project != null)
                foreach (var item in Project.Library.Artifacts)
                {
                    if (item == null)
                        continue;

                    if (item.Id == a.Id)
                        return Project.Library;
                }

            return null;
        }

        public static Library FindLibrary(Tile t)
        {
            if (t == null)
                return null;

            foreach (var lib in Libraries)
            foreach (var item in lib.Tiles)
            {
                if (item == null)
                    continue;

                if (item.Id == t.Id)
                    return lib;
            }

            if (Project != null)
                foreach (var item in Project.Library.Tiles)
                {
                    if (item == null)
                        continue;

                    if (item.Id == t.Id)
                        return Project.Library;
                }

            return null;
        }

        public static Library FindLibrary(TerrainPower tp)
        {
            if (tp == null)
                return null;

            foreach (var lib in Libraries)
            foreach (var item in lib.TerrainPowers)
            {
                if (item == null)
                    continue;

                if (item.Id == tp.Id)
                    return lib;
            }

            if (Project != null)
                foreach (var item in Project.Library.TerrainPowers)
                {
                    if (item == null)
                        continue;

                    if (item.Id == tp.Id)
                        return Project.Library;
                }

            return null;
        }

        public static ICreature FindCreature(Guid creatureId, SearchType searchType)
        {
            if (searchType == SearchType.External || searchType == SearchType.Global)
                foreach (var lib in Libraries)
                {
                    var creature = lib.FindCreature(creatureId);
                    if (creature != null)
                        return creature;
                }

            if (searchType == SearchType.Project || searchType == SearchType.Global)
                if (Project != null)
                {
                    var creature = Project.Library.FindCreature(creatureId);
                    if (creature != null)
                        return creature;

                    var cc = Project.FindCustomCreature(creatureId);
                    if (cc != null)
                        return cc;

                    var npc = Project.FindNpc(creatureId);
                    if (npc != null)
                        return npc;
                }

            return null;
        }

        public static CreatureTemplate FindTemplate(Guid templateId, SearchType searchType)
        {
            if (searchType == SearchType.External || searchType == SearchType.Global)
                foreach (var lib in Libraries)
                {
                    var template = lib.FindTemplate(templateId);
                    if (template != null)
                        return template;
                }

            if (searchType == SearchType.Project || searchType == SearchType.Global)
            {
                var template = Project?.Library.FindTemplate(templateId);
                if (template != null)
                    return template;
            }

            return null;
        }

        public static MonsterTheme FindTheme(Guid themeId, SearchType searchType)
        {
            if (searchType == SearchType.External || searchType == SearchType.Global)
                foreach (var lib in Libraries)
                {
                    var theme = lib.FindTheme(themeId);
                    if (theme != null)
                        return theme;
                }

            if (searchType == SearchType.Project || searchType == SearchType.Global)
            {
                var theme = Project?.Library.FindTheme(themeId);
                if (theme != null)
                    return theme;
            }

            return null;
        }

        public static Trap FindTrap(Guid trapId, SearchType searchType)
        {
            if (searchType == SearchType.External || searchType == SearchType.Global)
                foreach (var lib in Libraries)
                {
                    var trap = lib.FindTrap(trapId);
                    if (trap != null)
                        return trap;
                }

            if (searchType == SearchType.Project || searchType == SearchType.Global)
            {
                var trap = Project?.Library.FindTrap(trapId);
                if (trap != null)
                    return trap;
            }

            return null;
        }

        public static SkillChallenge FindSkillChallenge(Guid scId, SearchType searchType)
        {
            if (searchType == SearchType.External || searchType == SearchType.Global)
                foreach (var lib in Libraries)
                {
                    var sc = lib.FindSkillChallenge(scId);
                    if (sc != null)
                        return sc;
                }

            if (searchType == SearchType.Project || searchType == SearchType.Global)
            {
                var sc = Project?.Library.FindSkillChallenge(scId);
                if (sc != null)
                    return sc;
            }

            return null;
        }

        public static MagicItem FindMagicItem(Guid itemId, SearchType searchType)
        {
            if (searchType == SearchType.External || searchType == SearchType.Global)
                foreach (var lib in Libraries)
                {
                    var item = lib.FindMagicItem(itemId);
                    if (item != null)
                        return item;
                }

            if (searchType == SearchType.Project || searchType == SearchType.Global)
            {
                var item = Project?.Library.FindMagicItem(itemId);
                if (item != null)
                    return item;
            }

            return null;
        }

        public static Artifact FindArtifact(Guid artifactId, SearchType searchType)
        {
            if (searchType == SearchType.External || searchType == SearchType.Global)
                foreach (var lib in Libraries)
                {
                    var item = lib.FindArtifact(artifactId);
                    if (item != null)
                        return item;
                }

            if (searchType == SearchType.Project || searchType == SearchType.Global)
            {
                var item = Project?.Library.FindArtifact(artifactId);
                if (item != null)
                    return item;
            }

            return null;
        }

        public static Tile FindTile(Guid tileId, SearchType searchType)
        {
            if (searchType == SearchType.External || searchType == SearchType.Global)
                foreach (var lib in Libraries)
                {
                    var t = lib.FindTile(tileId);
                    if (t != null)
                        return t;
                }

            if (searchType == SearchType.Project || searchType == SearchType.Global)
            {
                var t = Project?.Library.FindTile(tileId);
                if (t != null)
                    return t;
            }

            return null;
        }

        public static TerrainPower FindTerrainPower(Guid powerId, SearchType searchType)
        {
            if (searchType == SearchType.External || searchType == SearchType.Global)
                foreach (var lib in Libraries)
                {
                    var tp = lib.FindTerrainPower(powerId);
                    if (tp != null)
                        return tp;
                }

            if (searchType == SearchType.Project || searchType == SearchType.Global)
            {
                var tp = Project?.Library.FindTerrainPower(powerId);
                if (tp != null)
                    return tp;
            }

            return null;
        }

        public static void CreateBackup(string filename)
        {
            try
            {
                var ass = Assembly.GetEntryAssembly();
                var dir = Tools.FileName.Directory(ass.Location) + "Backup\\";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var newName = dir + Tools.FileName.Name(filename);
                File.Copy(filename, newName, true);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        public static Project LoadBackup(string filename)
        {
            Project p = null;

            try
            {
                var ass = Assembly.GetEntryAssembly();
                var dir = Tools.FileName.Directory(ass.Location) + "Backup\\";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var backupName = dir + Tools.FileName.Name(filename);
                if (File.Exists(backupName))
                {
                    p = Serialisation<Project>.Load(backupName, SerialisationMode.Binary);
                    if (p != null)
                    {
                        var str =
                            "There was a problem opening this project; it has been recovered from its most recent backup version.";
                        MessageBox.Show(str, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return p;
        }

        public static int Dice(int throws, int sides)
        {
            var result = 0;

            for (var n = 0; n != throws; ++n)
            {
                var roll = 1 + Random.Next() % sides;
                result += roll;
            }

            return result;
        }

        public static bool CheckPassword(Project p)
        {
            if (p.Password == null || p.Password == "")
                return true;

            var dlg = new PasswordCheckForm(p.Password, p.PasswordHint);
            return dlg.ShowDialog() == DialogResult.OK;
        }
    }
}
