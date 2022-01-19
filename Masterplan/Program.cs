using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.UI;

namespace Masterplan
{
    internal static class Program
    {
        internal static bool FIsBeta = false;

        public static ProgressScreen SplashScreen;

        public static string ProjectFilter = "Masterplan Project|*.masterplan";
        public static string LibraryFilter = "Masterplan Library|*.library";
        public static string EncounterFilter = "Masterplan Encounter|*.encounter";
        public static string BackgroundFilter = "Masterplan Campaign Background|*.background";
        public static string EncyclopediaFilter = "Masterplan Campaign Encyclopedia|*.encyclopedia";
        public static string RulesFilter = "Masterplan Rules|*.crunch";

        public static string CreatureAndMonsterFilter = "Creatures|*.creature;*.monster";
        public static string MonsterFilter = "Adventure Tools Creatures|*.monster";
        public static string CreatureFilter = "Creatures|*.creature";
        public static string CreatureTemplateFilter = "Creature Template|*.creaturetemplate";
        public static string ThemeFilter = "Themes|*.theme";

        public static string CreatureTemplateAndThemeFilter =
            "Creature Templates and Themes|*.creaturetemplate;*.theme";

        public static string TrapFilter = "Traps|*.trap";
        public static string SkillChallengeFilter = "Skill Challenges|*.skillchallenge";
        public static string MagicItemFilter = "Magic Items|*.magicitem";
        public static string ArtifactFilter = "Artifacts|*.artifact";
        public static string MapTileFilter = "Map Tiles|*.maptile";
        public static string TerrainPowerFilter = "Terrain Powers|*.terrainpower";

        public static string HtmlFilter = "HTML File|*.htm";
        public static string ImageFilter = "Image File|*.bmp;*.jpg;*.jpeg;*.gif;*.png;*.tga";

        internal static bool IsBeta => FIsBeta;

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                init_logging();

                SplashScreen = new ProgressScreen("Masterplan", 0);
                SplashScreen.CurrentAction = "Loading...";
                SplashScreen.Show();

                load_preferences();
                load_libraries();

                foreach (var arg in args)
                    handle_arg(arg);

                SplashScreen.CurrentAction = "Starting Masterplan...";
                SplashScreen.Actions = 0;

                try
                {
                    var mainForm = new MainForm();
                    Application.Run(mainForm);
                }
                catch (Exception ex)
                {
                    LogSystem.Trace(ex);
                }

                var forms = new List<Form>();
                foreach (Form form in Application.OpenForms)
                    forms.Add(form);
                foreach (var form in forms)
                    form.Close();

                save_preferences();

                if (IsBeta)
                    check_for_logs();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private static void init_logging()
        {
            try
            {
                // Logging
                var mpDir = FileName.Directory(Application.ExecutablePath);

                // Make sure the log directory exists
                var logdir = mpDir + "Log" + Path.DirectorySeparatorChar;
                if (!Directory.Exists(logdir))
                {
                    var di = Directory.CreateDirectory(logdir);
                    if (di == null)
                        throw new UnauthorizedAccessException();
                }

                // Begin logging
                var logfile = logdir + DateTime.Now.Ticks + ".log";
                LogSystem.LogFile = logfile;
            }
            catch
            {
            }
        }

        private static void load_libraries()
        {
            try
            {
                SplashScreen.CurrentAction = "Loading libraries...";

                var ass = Assembly.GetEntryAssembly();
                var rootDir = FileName.Directory(ass.Location);

                var libDir = rootDir + "Libraries\\";
                if (!Directory.Exists(libDir))
                    Directory.CreateDirectory(libDir);

                // Move libraries from root directory
                var files = Directory.GetFiles(rootDir, "*.library");
                foreach (var filename in files)
                    try
                    {
                        var libName = libDir + FileName.Name(filename) + ".library";

                        if (!File.Exists(libName))
                            File.Move(filename, libName);
                    }
                    catch (Exception ex)
                    {
                        LogSystem.Trace(ex);
                    }

                // Load libraries
                var libraries = Directory.GetFiles(libDir, "*.library");
                SplashScreen.Actions = libraries.Length;
                foreach (var filename in libraries)
                    Session.LoadLibrary(filename);

                Session.Libraries.Sort();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private static void load_preferences()
        {
            try
            {
                var ass = Assembly.GetEntryAssembly();
                var rootDir = FileName.Directory(ass.Location);
                var filename = rootDir + "Preferences.xml";

                if (File.Exists(filename))
                {
                    SplashScreen.CurrentAction = "Loading user preferences";

                    var prefs = Serialisation<Preferences>.Load(filename, SerialisationMode.Xml);
                    if (prefs != null)
                        Session.Preferences = prefs;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private static void save_preferences()
        {
            try
            {
                var ass = Assembly.GetEntryAssembly();
                var rootDir = FileName.Directory(ass.Location);
                var filename = rootDir + "Preferences.xml";

                Serialisation<Preferences>.Save(filename, Session.Preferences, SerialisationMode.Xml);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private static void handle_arg(string arg)
        {
            try
            {
                if (arg == "-creaturestats") run_creature_stats();

                var fi = new FileInfo(arg);
                if (fi.Exists)
                {
                    SplashScreen.CurrentAction = "Loading project...";
                    SplashScreen.CurrentSubAction = FileName.Name(fi.Name);

                    // Load file
                    var p = Serialisation<Project>.Load(arg, SerialisationMode.Binary);
                    if (p != null)
                        Session.CreateBackup(arg);
                    else
                        p = Session.LoadBackup(arg);

                    if (p != null)
                        if (Session.CheckPassword(p))
                        {
                            Session.Project = p;
                            Session.FileName = arg;

                            p.Update();
                            p.SimplifyProjectLibrary();
                        }
                }
            }
            catch
            {
            }
        }

        private static void check_for_logs()
        {
            var logfile = LogSystem.LogFile;

            if (logfile == null || logfile == "")
                return;

            if (!File.Exists(logfile))
                return;

            var logdir = FileName.Directory(logfile);
            Process.Start(logdir);
        }

        private static void run_creature_stats()
        {
            // Run stats
            var creatures = Session.Creatures;
            bool[] isMinionOptions = { false, true };
            bool[] isLeaderOptions = { false, true };

            var datafile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Creatures.csv";
            var sw = new StreamWriter(datafile);
            try
            {
                sw.Write("Level,Flag,Role,Minion,Leader,Tier,TierX,Creatures,Powers");
                // Conditions
                foreach (var condition in Conditions.GetConditions())
                    sw.Write("," + condition);
                // Damage types
                foreach (DamageType damage in Enum.GetValues(typeof(DamageType)))
                    sw.Write("," + damage);
                sw.WriteLine();

                for (var level = 1; level <= 40; ++level)
                    foreach (var isMinion in isMinionOptions)
                    foreach (var isLeader in isLeaderOptions)
                    foreach (RoleType role in Enum.GetValues(typeof(RoleType)))
                    foreach (RoleFlag flag in Enum.GetValues(typeof(RoleFlag)))
                    {
                        var list = get_creatures(creatures, level, isMinion, isLeader, role, flag);

                        var powers = new List<CreaturePower>();
                        foreach (var c in list)
                            powers.AddRange(c.CreaturePowers);
                        if (powers.Count == 0)
                            continue;

                        var tier = "";
                        if (level < 11)
                            tier = "heroic";
                        else if (level < 21)
                            tier = "paragon";
                        else
                            tier = "epic";

                        var tierx = "";
                        if (level < 4)
                            tierx = "early heroic";
                        else if (level < 8)
                            tierx = "mid heroic";
                        else if (level < 11)
                            tierx = "late heroic";
                        else if (level < 14)
                            tierx = "early paragon";
                        else if (level < 18)
                            tierx = "mid paragon";
                        else if (level < 21)
                            tierx = "late paragon";
                        else if (level < 24)
                            tierx = "early epic";
                        else if (level < 28)
                            tierx = "mid epic";
                        else if (level < 31)
                            tierx = "late epic";
                        else
                            tierx = "epic plus";

                        sw.Write(level + "," + flag + "," + role + "," + isMinion + "," + isLeader + "," + tier + "," +
                                 tierx + "," + list.Count + "," + powers.Count);

                        foreach (var condition in Conditions.GetConditions())
                        {
                            var count = 0;

                            var str = condition.ToLower();
                            foreach (var power in powers)
                                if (power.Details.ToLower().Contains(str))
                                    count += 1;

                            double pc = 0;
                            if (powers.Count != 0)
                                pc = (double)count / powers.Count;

                            sw.Write("," + pc);
                        }

                        foreach (DamageType damage in Enum.GetValues(typeof(DamageType)))
                        {
                            var count = 0;

                            var str = damage.ToString().ToLower();
                            foreach (var power in powers)
                                if (power.Details.ToLower().Contains(str))
                                    count += 1;

                            double pc = 0;
                            if (powers.Count != 0)
                                pc = (double)count / powers.Count;

                            sw.Write("," + pc);
                        }

                        sw.WriteLine();
                    }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
            finally
            {
                sw.Close();
            }
        }

        private static List<Creature> get_creatures(List<Creature> creatures, int level, bool isMinion, bool isLeader,
            RoleType role, RoleFlag flag)
        {
            var list = new List<Creature>();

            foreach (var c in creatures)
            {
                if (c.Level != level)
                    continue;

                var cr = c.Role as ComplexRole;
                var m = c.Role as Minion;

                if (m != null && !m.HasRole)
                    continue;

                var minion = m != null;
                if (minion != isMinion)
                    continue;

                var leader = cr != null && cr.Leader;
                if (leader != isLeader)
                    continue;

                var rt = RoleType.Blaster;
                var rf = RoleFlag.Standard;
                if (cr != null)
                {
                    rt = cr.Type;
                    rf = cr.Flag;
                }

                if (m != null)
                {
                    rt = m.Type;
                    rf = RoleFlag.Standard;
                }

                if (rt != role)
                    continue;

                if (rf != flag)
                    continue;

                list.Add(c);
            }

            return list;
        }

        internal static void SetResolution(Image img)
        {
            var bmp = img as Bitmap;
            if (bmp != null)
                try
                {
                    var xDpi = Math.Min(bmp.HorizontalResolution, 96);
                    var yDpi = Math.Min(bmp.VerticalResolution, 96);

                    bmp.SetResolution(xDpi, yDpi);
                }
                catch
                {
                    // Didn't set anything
                }
        }
    }
}
