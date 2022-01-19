using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Masterplan.Controls;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.Tools.IO;

namespace Masterplan.UI
{
    internal partial class LibraryListForm : Form
    {
        private readonly List<TabPage> _fCleanPages = new List<TabPage>();

        private readonly Dictionary<Library, bool> _fModified = new Dictionary<Library, bool>();

        private bool _fShowCategorised = true;
        private bool _fShowUncategorised = true;

        public Library SelectedLibrary
        {
            get => LibraryTree.SelectedNode?.Tag as Library;
            set
            {
                var nodes = new List<TreeNode>();
                foreach (TreeNode tn in LibraryTree.Nodes)
                    get_nodes(tn, nodes);

                foreach (var tn in nodes)
                    if (tn.Tag == value)
                    {
                        LibraryTree.SelectedNode = tn;
                        break;
                    }
            }
        }

        public List<Creature> SelectedCreatures
        {
            get
            {
                var list = new List<Creature>();

                foreach (ListViewItem lvi in CreatureList.SelectedItems)
                {
                    var c = lvi.Tag as Creature;
                    if (c != null)
                        list.Add(c);
                }

                return list;
            }
        }

        public List<CreatureTemplate> SelectedTemplates
        {
            get
            {
                var list = new List<CreatureTemplate>();

                foreach (ListViewItem lvi in TemplateList.SelectedItems)
                {
                    var ct = lvi.Tag as CreatureTemplate;
                    if (ct != null)
                        list.Add(ct);
                }

                return list;
            }
        }

        public List<MonsterTheme> SelectedThemes
        {
            get
            {
                var list = new List<MonsterTheme>();

                foreach (ListViewItem lvi in TemplateList.SelectedItems)
                {
                    var mt = lvi.Tag as MonsterTheme;
                    if (mt != null)
                        list.Add(mt);
                }

                return list;
            }
        }

        public List<Trap> SelectedTraps
        {
            get
            {
                var list = new List<Trap>();

                foreach (ListViewItem lvi in TrapList.SelectedItems)
                {
                    var t = lvi.Tag as Trap;
                    if (t != null)
                        list.Add(t);
                }

                return list;
            }
        }

        public List<SkillChallenge> SelectedChallenges
        {
            get
            {
                var list = new List<SkillChallenge>();

                foreach (ListViewItem lvi in ChallengeList.SelectedItems)
                {
                    var sc = lvi.Tag as SkillChallenge;
                    if (sc != null)
                        list.Add(sc);
                }

                return list;
            }
        }

        public string SelectedMagicItemSet
        {
            get
            {
                if (MagicItemList.SelectedItems.Count != 0)
                    return MagicItemList.SelectedItems[0].Text;

                return "";
            }
        }

        public List<MagicItem> SelectedMagicItems
        {
            get
            {
                var list = new List<MagicItem>();

                foreach (ListViewItem lvi in MagicItemVersionList.SelectedItems)
                {
                    var item = lvi.Tag as MagicItem;
                    if (item != null)
                        list.Add(item);
                }

                return list;
            }
        }

        public List<Tile> SelectedTiles
        {
            get
            {
                var list = new List<Tile>();

                foreach (ListViewItem lvi in TileList.SelectedItems)
                {
                    var t = lvi.Tag as Tile;
                    if (t != null)
                        list.Add(t);
                }

                return list;
            }
        }

        public List<TerrainPower> SelectedTerrainPowers
        {
            get
            {
                var list = new List<TerrainPower>();

                foreach (ListViewItem lvi in TerrainPowerList.SelectedItems)
                {
                    var tp = lvi.Tag as TerrainPower;
                    if (tp != null)
                        list.Add(tp);
                }

                return list;
            }
        }

        public List<Artifact> SelectedArtifacts
        {
            get
            {
                var list = new List<Artifact>();

                foreach (ListViewItem lvi in ArtifactList.SelectedItems)
                {
                    var a = lvi.Tag as Artifact;
                    if (a != null)
                        list.Add(a);
                }

                return list;
            }
        }

        public LibraryListForm()
        {
            InitializeComponent();

            CreatureSearchToolbar.Visible = false;

            foreach (var lib in Session.Libraries)
                _fModified[lib] = false;

            Application.Idle += Application_Idle;

            update_libraries();
        }

        ~LibraryListForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void get_nodes(TreeNode tn, List<TreeNode> nodes)
        {
            nodes.Add(tn);

            foreach (TreeNode child in tn.Nodes)
                get_nodes(child, nodes);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            HelpBtn.Text = HelpPanel.Visible ? "Hide Help" : "Show Help";

            if (SelectedLibrary != null && Session.Project != null && SelectedLibrary == Session.Project.Library)
            {
                LibraryRemoveBtn.Enabled = false;
                LibraryEditBtn.Enabled = false;

                CreatureAddBtn.Enabled = false;
                OppRemoveBtn.Enabled = false;
                OppEditBtn.Enabled = false;
                CreatureCopyBtn.Enabled = SelectedCreatures.Count != 0;
                CreatureCutBtn.Enabled = false;
                CreaturePasteBtn.Enabled = false;
                CreatureStatBlockBtn.Enabled = SelectedCreatures.Count == 1;
                CreatureToolsExport.Enabled = SelectedCreatures.Count == 1;

                TemplateAddBtn.Enabled = false;
                TemplateRemoveBtn.Enabled = false;
                TemplateEditBtn.Enabled = false;
                TemplateCopyBtn.Enabled = SelectedTemplates.Count != 0 || SelectedThemes.Count != 0;
                TemplateCutBtn.Enabled = false;
                TemplatePasteBtn.Enabled = false;
                TemplateStatBlock.Enabled = SelectedTemplates.Count == 1 || SelectedThemes.Count == 1;
                TemplateToolsExport.Enabled = SelectedTemplates.Count == 1 || SelectedThemes.Count == 1;

                TrapAdd.Enabled = false;
                TrapRemoveBtn.Enabled = false;
                TrapEditBtn.Enabled = false;
                TrapCopyBtn.Enabled = SelectedTraps.Count != 0;
                TrapCutBtn.Enabled = false;
                TrapPasteBtn.Enabled = false;
                TrapStatBlockBtn.Enabled = SelectedTraps.Count == 1;
                TrapToolsExport.Enabled = SelectedTraps.Count == 1;

                ChallengeAdd.Enabled = false;
                ChallengeRemoveBtn.Enabled = false;
                ChallengeEditBtn.Enabled = false;
                ChallengeCopyBtn.Enabled = SelectedChallenges.Count != 0;
                ChallengeCutBtn.Enabled = false;
                ChallengePasteBtn.Enabled = false;
                ChallengeStatBlockBtn.Enabled = SelectedChallenges.Count == 1;
                ChallengeToolsExport.Enabled = SelectedChallenges.Count == 1;

                MagicItemAdd.Enabled = false;
                MagicItemRemoveBtn.Enabled = false;
                MagicItemEditBtn.Enabled = false;
                MagicItemCopyBtn.Enabled = SelectedMagicItems.Count != 0;
                MagicItemCutBtn.Enabled = false;
                MagicItemPasteBtn.Enabled = false;
                MagicItemStatBlockBtn.Enabled = SelectedMagicItems.Count == 1;
                MagicItemToolsExport.Enabled = SelectedMagicItemSet != "";

                TileAddBtn.Enabled = false;
                TileRemoveBtn.Enabled = false;
                TileEditBtn.Enabled = false;
                TileCopyBtn.Enabled = SelectedTiles.Count != 0;
                TileCutBtn.Enabled = false;
                TilePasteBtn.Enabled = false;
                TileToolsExport.Enabled = SelectedTiles.Count == 1;

                TPAdd.Enabled = false;
                TPRemoveBtn.Enabled = false;
                TPEditBtn.Enabled = false;
                TPCopyBtn.Enabled = SelectedTerrainPowers.Count != 0;
                TPCutBtn.Enabled = false;
                TPPasteBtn.Enabled = false;
                TPStatBlockBtn.Enabled = SelectedTerrainPowers.Count == 1;
                TPToolsExport.Enabled = SelectedTerrainPowers.Count == 1;

                ArtifactAdd.Enabled = false;
                ArtifactRemove.Enabled = false;
                ArtifactEdit.Enabled = false;
                ArtifactCopy.Enabled = SelectedArtifacts.Count != 0;
                ArtifactCut.Enabled = false;
                ArtifactPaste.Enabled = false;
                ArtifactStatBlockBtn.Enabled = SelectedArtifacts.Count == 1;
                ArtifactToolsExport.Enabled = SelectedArtifacts.Count == 1;
            }
            else
            {
                LibraryRemoveBtn.Enabled = SelectedLibrary != null;
                LibraryEditBtn.Enabled = SelectedLibrary != null;

                CreatureAddBtn.Enabled = SelectedLibrary != null;
                OppRemoveBtn.Enabled = SelectedCreatures.Count != 0;
                OppEditBtn.Enabled = SelectedCreatures.Count == 1;
                CreatureCopyBtn.Enabled = SelectedCreatures.Count != 0;
                CreatureCutBtn.Enabled = SelectedCreatures.Count != 0;
                CreaturePasteBtn.Enabled =
                    SelectedLibrary != null && Clipboard.ContainsData(typeof(List<Creature>).ToString());
                CreatureStatBlockBtn.Enabled = SelectedCreatures.Count == 1;
                CreatureToolsExport.Enabled = SelectedCreatures.Count == 1;

                TemplateAddBtn.Enabled = SelectedLibrary != null;
                TemplateRemoveBtn.Enabled = SelectedTemplates.Count != 0 || SelectedThemes.Count != 0;
                TemplateEditBtn.Enabled = SelectedTemplates.Count != 0 || SelectedThemes.Count != 0;
                TemplateCopyBtn.Enabled = SelectedTemplates.Count != 0 || SelectedThemes.Count != 0;
                TemplateCutBtn.Enabled = SelectedTemplates.Count != 0 || SelectedThemes.Count != 0;
                TemplatePasteBtn.Enabled = SelectedLibrary != null &&
                                           (Clipboard.ContainsData(typeof(List<CreatureTemplate>).ToString()) ||
                                            Clipboard.ContainsData(typeof(List<MonsterTheme>).ToString()));
                TemplateStatBlock.Enabled = SelectedTemplates.Count == 1;
                TemplateToolsExport.Enabled = SelectedTemplates.Count == 1 || SelectedThemes.Count == 1;

                TrapAdd.Enabled = SelectedLibrary != null;
                TrapRemoveBtn.Enabled = SelectedTraps.Count != 0;
                TrapEditBtn.Enabled = SelectedTraps.Count == 1;
                TrapCopyBtn.Enabled = SelectedTraps.Count != 0;
                TrapCutBtn.Enabled = SelectedTraps.Count != 0;
                TrapPasteBtn.Enabled = SelectedLibrary != null && Clipboard.ContainsData(typeof(List<Trap>).ToString());
                TrapStatBlockBtn.Enabled = SelectedTraps.Count == 1;
                TrapToolsExport.Enabled = SelectedTraps.Count == 1;

                ChallengeAdd.Enabled = SelectedLibrary != null;
                ChallengeRemoveBtn.Enabled = SelectedChallenges.Count != 0;
                ChallengeEditBtn.Enabled = SelectedChallenges.Count == 1;
                ChallengeCopyBtn.Enabled = SelectedChallenges.Count != 0;
                ChallengeCutBtn.Enabled = SelectedChallenges.Count != 0;
                ChallengePasteBtn.Enabled = SelectedLibrary != null &&
                                            Clipboard.ContainsData(typeof(List<SkillChallenge>).ToString());
                ChallengeStatBlockBtn.Enabled = SelectedChallenges.Count == 1;
                ChallengeToolsExport.Enabled = SelectedChallenges.Count == 1;

                MagicItemAdd.Enabled = SelectedLibrary != null;
                MagicItemRemoveBtn.Enabled = SelectedMagicItems.Count != 0;
                MagicItemEditBtn.Enabled = SelectedMagicItems.Count == 1;
                MagicItemCopyBtn.Enabled = SelectedMagicItems.Count != 0;
                MagicItemCutBtn.Enabled = SelectedMagicItems.Count != 0;
                MagicItemPasteBtn.Enabled = SelectedLibrary != null &&
                                            Clipboard.ContainsData(typeof(List<MagicItem>).ToString());
                MagicItemStatBlockBtn.Enabled = SelectedMagicItems.Count == 1;
                MagicItemToolsExport.Enabled = SelectedMagicItemSet != "";

                TileAddBtn.Enabled = SelectedLibrary != null;
                TileRemoveBtn.Enabled = SelectedTiles.Count != 0;
                TileEditBtn.Enabled = SelectedTiles.Count == 1;
                TileCopyBtn.Enabled = SelectedTiles.Count != 0;
                TileCutBtn.Enabled = SelectedTiles.Count != 0;
                TilePasteBtn.Enabled = SelectedLibrary != null && Clipboard.ContainsData(typeof(List<Tile>).ToString());
                TileToolsExport.Enabled = SelectedTiles.Count == 1;

                TPAdd.Enabled = SelectedLibrary != null;
                TPRemoveBtn.Enabled = SelectedTerrainPowers.Count != 0;
                TPEditBtn.Enabled = SelectedTerrainPowers.Count == 1;
                TPCopyBtn.Enabled = SelectedTerrainPowers.Count != 0;
                TPCutBtn.Enabled = SelectedTerrainPowers.Count != 0;
                TPPasteBtn.Enabled = SelectedLibrary != null &&
                                     Clipboard.ContainsData(typeof(List<TerrainPower>).ToString());
                TPStatBlockBtn.Enabled = SelectedTerrainPowers.Count == 1;
                TPToolsExport.Enabled = SelectedTerrainPowers.Count == 1;

                ArtifactAdd.Enabled = SelectedLibrary != null;
                ArtifactRemove.Enabled = SelectedArtifacts.Count != 0;
                ArtifactEdit.Enabled = SelectedArtifacts.Count == 1;
                ArtifactCopy.Enabled = SelectedArtifacts.Count != 0;
                ArtifactCut.Enabled = SelectedArtifacts.Count != 0;
                ArtifactPaste.Enabled =
                    SelectedLibrary != null && Clipboard.ContainsData(typeof(List<Artifact>).ToString());
                ArtifactStatBlockBtn.Enabled = SelectedArtifacts.Count == 1;
                ArtifactToolsExport.Enabled = SelectedArtifacts.Count == 1;
            }

            CreatureToolsFilterList.Checked = CreatureSearchToolbar.Visible;
            CategorisedBtn.Checked = _fShowCategorised;
            UncategorisedBtn.Checked = _fShowUncategorised;

            CreatureContextRemove.Enabled = SelectedCreatures.Count != 0;
            CreatureContextCategory.Enabled = SelectedCreatures.Count != 0;

            TemplateContextRemove.Enabled = SelectedTemplates.Count != 0;
            TemplateContextType.Enabled = SelectedTemplates.Count != 0;

            TrapContextRemove.Enabled = SelectedTraps.Count != 0;
            TrapContextType.Enabled = SelectedTraps.Count != 0;

            ChallengeContextRemove.Enabled = SelectedChallenges.Count != 0;

            MagicItemContextRemove.Enabled = SelectedMagicItems.Count != 0;

            TileContextRemove.Enabled = SelectedTiles.Count != 0;
            TileContextCategory.Enabled = SelectedTiles.Count != 0;
            TileContextSize.Enabled = SelectedTiles.Count != 0;

            TPContextRemove.Enabled = SelectedTerrainPowers.Count != 0;

            ArtifactContextRemove.Enabled = SelectedArtifacts.Count != 0;
        }

        private void LibrariesForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var lib in Session.Libraries)
            {
                if (_fModified.ContainsKey(lib) && !_fModified[lib])
                    continue;

                Save(lib);
            }
        }

        private void FileNew_Click(object sender, EventArgs e)
        {
            var lib = new Library();
            lib.Name = "New Library";

            var dlg = new LibraryForm(lib);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Libraries.Add(dlg.Library);
                Session.Libraries.Sort();

                Save(dlg.Library);
                _fModified[dlg.Library] = true;

                update_libraries();

                SelectedLibrary = dlg.Library;
                update_content(true);
            }
        }

        private void FileOpen_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.LibraryFilter;
            dlg.Multiselect = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // If any are not in the Libraries folder, offer to copy them

                var libFolder = Session.LibraryFolder;

                var foreignFiles = new List<string>();
                foreach (var filename in dlg.FileNames)
                {
                    var dir = FileName.Directory(filename);
                    if (!dir.Contains(libFolder))
                        foreignFiles.Add(filename);
                }

                if (foreignFiles.Count != 0)
                {
                    var msg = "Do you want to move these libraries into the Libraries folder?";
                    msg += Environment.NewLine;
                    msg += "They will then be loaded automatically the next time Masterplan starts up.";

                    var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                        foreach (var filename in foreignFiles)
                        {
                            var newFilename = libFolder + FileName.Name(filename) + ".library";
                            File.Copy(filename, newFilename);
                        }
                }

                foreach (var filename in dlg.FileNames)
                {
                    if (foreignFiles.Contains(filename))
                        continue;

                    var lib = Session.LoadLibrary(filename);
                    _fModified[lib] = false;
                }

                foreach (var filename in foreignFiles)
                {
                    var lib = Session.LoadLibrary(filename);
                    _fModified[lib] = false;
                }

                if (Session.Project != null)
                    Session.Project.SimplifyProjectLibrary();

                Session.Libraries.Sort();
                update_libraries();
                update_content(true);
            }
        }

        private void FileClose_Click(object sender, EventArgs e)
        {
            if (Session.Project != null)
                Session.Project.PopulateProjectLibrary();

            foreach (var lib in Session.Libraries)
            {
                lib.Creatures.Clear();
                lib.Templates.Clear();
                lib.Themes.Clear();
                lib.Traps.Clear();
                lib.SkillChallenges.Clear();
                lib.MagicItems.Clear();
                lib.Tiles.Clear();
                lib.TerrainPowers.Clear();
                lib.Artifacts.Clear();
            }

            Session.Libraries.Clear();

            if (Session.Project != null)
                Session.Project.SimplifyProjectLibrary();

            update_libraries();
            update_content(true);

            GC.Collect();
        }

        private void LibraryRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary != null)
            {
                var str = "You are about to delete a library; are you sure you want to do this?";
                if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                    return;

                Session.DeleteLibrary(SelectedLibrary);
                update_libraries();

                SelectedLibrary = null;
                update_content(true);
            }
        }

        private void LibraryEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary != null)
            {
                var index = Session.Libraries.IndexOf(SelectedLibrary);
                var oldFilename = Session.GetLibraryFilename(SelectedLibrary);

                if (!File.Exists(oldFilename))
                {
                    var str = "This library cannot be renamed.";
                    MessageBox.Show(str, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return;
                }

                var dlg = new LibraryForm(SelectedLibrary);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Libraries[index] = dlg.Library;
                    Session.Libraries.Sort();

                    // Has the name changed?
                    var newFilename = Session.GetLibraryFilename(dlg.Library);
                    if (oldFilename != newFilename)
                    {
                        // Move the file
                        var fi = new FileInfo(oldFilename);
                        fi.MoveTo(newFilename);
                    }

                    _fModified[dlg.Library] = true;

                    update_libraries();
                    update_content(true);
                }
            }
        }

        private void LibraryMergeBtn_Click(object sender, EventArgs e)
        {
            var dlg = new MergeLibrariesForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var newlib = new Library();
                newlib.Name = dlg.LibraryName;

                foreach (var lib in dlg.SelectedLibraries)
                {
                    newlib.Import(lib);
                    Session.DeleteLibrary(lib);
                }

                Session.Libraries.Add(newlib);
                Session.Libraries.Sort();

                Save(newlib);
                update_libraries();

                SelectedLibrary = newlib;
                update_content(true);
            }
        }

        private void LibraryTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            update_content(true);
        }

        private void LibraryList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var lib = SelectedLibrary;
            if (lib == null)
                return;

            if (Session.Project != null && Session.Project.Library == lib)
                return;

            DoDragDrop(lib, DragDropEffects.Move);
        }

        private void HelpBtn_Click(object sender, EventArgs e)
        {
            show_help(!HelpPanel.Visible);
        }

        private void Pages_SelectedIndexChanged(object sender, EventArgs e)
        {
            update_content(false);
        }

        private void LibraryList_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            // What library are we over?
            var mouse = LibraryTree.PointToClient(Cursor.Position);
            var tn = LibraryTree.GetNodeAt(mouse);
            LibraryTree.SelectedNode = tn;

            if (SelectedLibrary == null)
                return;

            // Can't add to the project library
            if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                return;

            // Only allow the move if it's not already in this library

            var lib = e.Data.GetData(typeof(Library)) as Library;
            if (lib != null)
            {
                if (lib != SelectedLibrary)
                    e.Effect = DragDropEffects.Move;

                return;
            }

            var creatures = e.Data.GetData(typeof(List<Creature>)) as List<Creature>;
            if (creatures != null)
            {
                var ok = false;
                foreach (var c in creatures)
                    if (!SelectedLibrary.Creatures.Contains(c))
                    {
                        ok = true;
                        break;
                    }

                if (ok)
                    e.Effect = DragDropEffects.Move;

                return;
            }

            var templates = e.Data.GetData(typeof(List<CreatureTemplate>)) as List<CreatureTemplate>;
            if (templates != null)
            {
                var ok = false;
                foreach (var ct in templates)
                    if (!SelectedLibrary.Templates.Contains(ct))
                    {
                        ok = true;
                        break;
                    }

                if (ok)
                    e.Effect = DragDropEffects.Move;

                return;
            }

            var traps = e.Data.GetData(typeof(List<Trap>)) as List<Trap>;
            if (traps != null)
            {
                var ok = false;
                foreach (var t in traps)
                    if (!SelectedLibrary.Traps.Contains(t))
                    {
                        ok = true;
                        break;
                    }

                if (ok)
                    e.Effect = DragDropEffects.Move;

                return;
            }

            var challenges = e.Data.GetData(typeof(List<SkillChallenge>)) as List<SkillChallenge>;
            if (challenges != null)
            {
                var ok = false;
                foreach (var sc in challenges)
                    if (!SelectedLibrary.SkillChallenges.Contains(sc))
                    {
                        ok = true;
                        break;
                    }

                if (ok)
                    e.Effect = DragDropEffects.Move;

                return;
            }

            var magicItems = e.Data.GetData(typeof(List<MagicItem>)) as List<MagicItem>;
            if (challenges != null)
            {
                var ok = false;
                foreach (var mi in magicItems)
                    if (!SelectedLibrary.MagicItems.Contains(mi))
                    {
                        ok = true;
                        break;
                    }

                if (ok)
                    e.Effect = DragDropEffects.Move;

                return;
            }

            var tiles = e.Data.GetData(typeof(List<Tile>)) as List<Tile>;
            if (tiles != null)
            {
                var ok = false;
                foreach (var t in tiles)
                    if (!SelectedLibrary.Tiles.Contains(t))
                    {
                        ok = true;
                        break;
                    }

                if (ok)
                    e.Effect = DragDropEffects.Move;

                return;
            }

            var terrainpowers = e.Data.GetData(typeof(List<TerrainPower>)) as List<TerrainPower>;
            if (terrainpowers != null)
            {
                var ok = false;
                foreach (var tp in terrainpowers)
                    if (!SelectedLibrary.TerrainPowers.Contains(tp))
                    {
                        ok = true;
                        break;
                    }

                if (ok)
                    e.Effect = DragDropEffects.Move;

                return;
            }

            var artifacts = e.Data.GetData(typeof(List<Artifact>)) as List<Artifact>;
            if (artifacts != null)
            {
                var ok = false;
                foreach (var a in artifacts)
                    if (!SelectedLibrary.Artifacts.Contains(a))
                    {
                        ok = true;
                        break;
                    }

                if (ok)
                    e.Effect = DragDropEffects.Move;

                return;
            }
        }

        private void LibraryList_DragDrop(object sender, DragEventArgs e)
        {
            // What library are we over?
            var mouse = LibraryTree.PointToClient(Cursor.Position);
            var tn = LibraryTree.GetNodeAt(mouse);
            LibraryTree.SelectedNode = tn;

            if (SelectedLibrary == null)
                return;

            var draggedLib = e.Data.GetData(typeof(Library)) as Library;
            if (draggedLib != null)
            {
                SelectedLibrary.Import(draggedLib);

                _fModified[SelectedLibrary] = true;

                Session.DeleteLibrary(draggedLib);

                update_content(true);
            }

            var creatures = e.Data.GetData(typeof(List<Creature>)) as List<Creature>;
            if (creatures != null)
            {
                foreach (var c in creatures)
                {
                    // Remove from the source library
                    foreach (var lib in Session.Libraries)
                        if (lib.Creatures.Contains(c))
                        {
                            lib.Creatures.Remove(c);
                            _fModified[lib] = true;
                        }

                    // Add it to the new book
                    SelectedLibrary.Creatures.Add(c);
                    _fModified[SelectedLibrary] = true;
                }

                update_creatures();
            }

            var templates = e.Data.GetData(typeof(List<CreatureTemplate>)) as List<CreatureTemplate>;
            if (templates != null)
            {
                foreach (var ct in templates)
                {
                    // Remove from the source library
                    foreach (var lib in Session.Libraries)
                        if (lib.Templates.Contains(ct))
                        {
                            lib.Templates.Remove(ct);
                            _fModified[lib] = true;
                        }

                    // Add it to the new book
                    SelectedLibrary.Templates.Add(ct);
                    _fModified[SelectedLibrary] = true;
                }

                update_templates();
            }

            var traps = e.Data.GetData(typeof(List<Trap>)) as List<Trap>;
            if (traps != null)
            {
                foreach (var t in traps)
                {
                    // Remove from the source library
                    foreach (var lib in Session.Libraries)
                        if (lib.Traps.Contains(t))
                        {
                            lib.Traps.Remove(t);
                            _fModified[lib] = true;
                        }

                    // Add it to the new book
                    SelectedLibrary.Traps.Add(t);
                    _fModified[SelectedLibrary] = true;
                }

                update_traps();
            }

            var challenges = e.Data.GetData(typeof(List<SkillChallenge>)) as List<SkillChallenge>;
            if (challenges != null)
            {
                foreach (var sc in challenges)
                {
                    // Remove from the source library
                    foreach (var lib in Session.Libraries)
                        if (lib.SkillChallenges.Contains(sc))
                        {
                            lib.SkillChallenges.Remove(sc);
                            _fModified[lib] = true;
                        }

                    // Add it to the new book
                    SelectedLibrary.SkillChallenges.Add(sc);
                    _fModified[SelectedLibrary] = true;
                }

                update_challenges();
            }

            var magicItems = e.Data.GetData(typeof(List<MagicItem>)) as List<MagicItem>;
            if (magicItems != null)
            {
                foreach (var sc in magicItems)
                {
                    // Remove from the source library
                    foreach (var lib in Session.Libraries)
                        if (lib.MagicItems.Contains(sc))
                        {
                            lib.MagicItems.Remove(sc);
                            _fModified[lib] = true;
                        }

                    // Add it to the new book
                    SelectedLibrary.MagicItems.Add(sc);
                    _fModified[SelectedLibrary] = true;
                }

                update_magic_item_sets();
            }

            var tiles = e.Data.GetData(typeof(List<Tile>)) as List<Tile>;
            if (tiles != null)
            {
                foreach (var t in tiles)
                {
                    // Remove from the source library
                    foreach (var lib in Session.Libraries)
                        if (lib.Tiles.Contains(t))
                        {
                            lib.Tiles.Remove(t);
                            _fModified[lib] = true;
                        }

                    // Add it to the new book
                    SelectedLibrary.Tiles.Add(t);
                    _fModified[SelectedLibrary] = true;
                }

                update_tiles();
            }

            var terrainpowers = e.Data.GetData(typeof(List<TerrainPower>)) as List<TerrainPower>;
            if (terrainpowers != null)
            {
                foreach (var tp in terrainpowers)
                {
                    // Remove from the source library
                    foreach (var lib in Session.Libraries)
                        if (lib.TerrainPowers.Contains(tp))
                        {
                            lib.TerrainPowers.Remove(tp);
                            _fModified[lib] = true;
                        }

                    // Add it to the new book
                    SelectedLibrary.TerrainPowers.Add(tp);
                    _fModified[SelectedLibrary] = true;
                }

                update_terrain_powers();
            }

            var artifacts = e.Data.GetData(typeof(List<Artifact>)) as List<Artifact>;
            if (artifacts != null)
            {
                foreach (var a in artifacts)
                {
                    // Remove from the source library
                    foreach (var lib in Session.Libraries)
                        if (lib.Artifacts.Contains(a))
                        {
                            lib.Artifacts.Remove(a);
                            _fModified[lib] = true;
                        }

                    // Add it to the new book
                    SelectedLibrary.Artifacts.Add(a);
                    _fModified[SelectedLibrary] = true;
                }

                update_artifacts();
            }
        }

        private void CreatureAddBtn_Click(object sender, EventArgs e)
        {
            var c = new Creature();
            c.Name = "New Creature";

            var dlg = new CreatureBuilderForm(c);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedLibrary.Creatures.Add(dlg.Creature as Creature);
                _fModified[SelectedLibrary] = true;

                update_content(true);
            }
        }

        private void CreatureImport_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary != null)
            {
                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                    return;

                var dlg = new OpenFileDialog();
                dlg.Filter = Program.CreatureAndMonsterFilter;
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                    foreach (var filename in dlg.FileNames)
                    {
                        Creature c = null;
                        if (filename.EndsWith("creature"))
                            c = Serialisation<Creature>.Load(filename, SerialisationMode.Binary);
                        if (filename.EndsWith("monster"))
                        {
                            var xml = File.ReadAllText(filename);
                            c = AppImport.ImportCreature(xml);
                        }

                        if (c != null)
                        {
                            var previous = SelectedLibrary.FindCreature(c.Name, c.Level);
                            if (previous != null)
                            {
                                c.Id = previous.Id;
                                c.Category = previous.Category;

                                SelectedLibrary.Creatures.Remove(previous);
                            }

                            SelectedLibrary.Creatures.Add(c);
                            _fModified[SelectedLibrary] = true;

                            update_content(true);
                        }
                    }
            }
        }

        private void OppRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCreatures.Count != 0)
            {
                var str = "Are you sure you want to delete your selection?";
                if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                    return;

                foreach (var c in SelectedCreatures)
                {
                    var lib = SelectedLibrary;
                    if (lib == null)
                        lib = Session.FindLibrary(c);

                    lib.Creatures.Remove(c);
                    _fModified[lib] = true;
                }

                update_content(true);
            }
        }

        private void OppEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCreatures.Count == 1)
            {
                var lib = Session.FindLibrary(SelectedCreatures[0]);
                if (lib == null)
                    return;

                var index = lib.Creatures.IndexOf(SelectedCreatures[0]);

                var dlg = new CreatureBuilderForm(SelectedCreatures[0]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    lib.Creatures[index] = dlg.Creature as Creature;
                    _fModified[lib] = true;

                    update_content(true);
                }
            }
        }

        private void CreatureCutBtn_Click(object sender, EventArgs e)
        {
            CreatureCopyBtn_Click(sender, e);
            OppRemoveBtn_Click(sender, e);
        }

        private void CreatureCopyBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCreatures.Count != 0) Clipboard.SetData(typeof(List<Creature>).ToString(), SelectedCreatures);
        }

        private void CreaturePasteBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary == null)
                return;

            if (Clipboard.ContainsData(typeof(List<Creature>).ToString()))
            {
                var creatures = Clipboard.GetData(typeof(List<Creature>).ToString()) as List<Creature>;
                foreach (var c in creatures)
                {
                    // Make a copy with a new ID
                    var creature = c.Copy();
                    creature.Id = Guid.NewGuid();

                    SelectedLibrary.Creatures.Add(creature);
                    _fModified[SelectedLibrary] = true;
                }

                update_content(true);
            }
        }

        private void CreatureStatBlockBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCreatures.Count != 1)
                return;

            var card = new EncounterCard();
            card.CreatureId = SelectedCreatures[0].Id;

            var dlg = new CreatureDetailsForm(card);
            dlg.ShowDialog();
        }

        private void CreatureDemoBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new DemographicsForm(SelectedLibrary, DemographicsSource.Creatures);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PowerStatsBtn_Click(object sender, EventArgs e)
        {
            var powers = new List<CreaturePower>();
            var creatures = 0;

            if (SelectedLibrary == null)
            {
                foreach (var lib in Session.Libraries)
                {
                    creatures += lib.Creatures.Count;

                    foreach (var creature in lib.Creatures)
                        powers.AddRange(creature.CreaturePowers);
                }
            }
            else
            {
                creatures = SelectedLibrary.Creatures.Count;

                foreach (ICreature creature in SelectedLibrary.Creatures)
                    if (creature != null)
                        powers.AddRange(creature.CreaturePowers);

                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                {
                    creatures += Session.Project.CustomCreatures.Count;
                    foreach (ICreature creature in Session.Project.CustomCreatures)
                        if (creature != null)
                            powers.AddRange(creature.CreaturePowers);
                }
            }

            var dlg = new PowerStatisticsForm(powers, creatures);
            dlg.ShowDialog();
        }

        private void FilterBtn_Click(object sender, EventArgs e)
        {
            CreatureSearchToolbar.Visible = !CreatureSearchToolbar.Visible;
            update_content(true);
        }

        private void CreatureToolsExport_Click(object sender, EventArgs e)
        {
            if (SelectedCreatures.Count == 1)
            {
                var dlg = new SaveFileDialog();
                dlg.Title = "Export";
                dlg.Filter = Program.CreatureFilter;
                dlg.FileName = SelectedCreatures[0].Name;

                if (dlg.ShowDialog() == DialogResult.OK)
                    Serialisation<Creature>.Save(dlg.FileName, SelectedCreatures[0], SerialisationMode.Binary);
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            update_content(true);
        }

        private void CreatureFilterCategorised_Click(object sender, EventArgs e)
        {
            _fShowCategorised = !_fShowCategorised;

            update_content(true);
        }

        private void CreatureFilterUncategorised_Click(object sender, EventArgs e)
        {
            _fShowUncategorised = !_fShowUncategorised;

            update_content(true);
        }

        private void OppList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedCreatures.Count != 0)
                DoDragDrop(SelectedCreatures, DragDropEffects.Move);
        }

        private void CreatureContextRemove_Click(object sender, EventArgs e)
        {
            OppRemoveBtn_Click(sender, e);
        }

        private void CreatureContextCategory_Click(object sender, EventArgs e)
        {
            var cats = new List<string>();
            foreach (var c in Session.Creatures)
            {
                if (c.Category == null || c.Category == "")
                    continue;

                if (cats.Contains(c.Category))
                    continue;

                cats.Add(c.Category);
            }

            var selectedCats = new List<string>();
            foreach (var c in SelectedCreatures)
            {
                if (c.Category == null || c.Category == "")
                    continue;

                if (selectedCats.Contains(c.Category))
                    continue;

                selectedCats.Add(c.Category);
            }

            var selected = "";
            if (selectedCats.Count == 1) selected = selectedCats[0];
            if (selectedCats.Count == 0)
            {
                if (SelectedCreatures.Count == 1)
                {
                    selected = SelectedCreatures[0].Name;
                }
                else
                {
                    const int minNameLength = 3;
                    var suggestions = new Dictionary<string, int>();

                    for (var x = 0; x != SelectedCreatures.Count; ++x)
                    {
                        var nameX = SelectedCreatures[x].Name;

                        for (var y = x + 1; y != SelectedCreatures.Count; ++y)
                        {
                            var nameY = SelectedCreatures[y].Name;

                            var lcs = StringHelper.LongestCommonToken(nameX, nameY);
                            if (lcs.Length >= minNameLength)
                            {
                                if (!suggestions.ContainsKey(lcs))
                                    suggestions[lcs] = 0;

                                suggestions[lcs] += 1;
                            }
                        }
                    }

                    if (suggestions.Keys.Count != 0)
                    {
                        foreach (var token in suggestions.Keys)
                        {
                            var current = suggestions.ContainsKey(selected) ? suggestions[selected] : 0;
                            if (suggestions[token] > current)
                                selected = token;
                        }

                        if (!cats.Contains(selected))
                            cats.Add(selected);
                    }
                }
            }

            var dlg = new CategoryForm(cats, selected);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (var c in SelectedCreatures)
                {
                    c.Category = dlg.Category;

                    var lib = Session.FindLibrary(c);
                    if (lib != null)
                        _fModified[lib] = true;
                }

                update_creatures();
            }
        }

        private void update_creatures()
        {
            Cursor.Current = Cursors.WaitCursor;

            CreatureList.BeginUpdate();
            var state = ListState.GetState(CreatureList);

            var creatures = new List<Creature>();
            if (SelectedLibrary != null)
            {
                creatures.AddRange(SelectedLibrary.Creatures);

                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                {
                    // Add all project custom creatures
                    foreach (var cc in Session.Project.CustomCreatures)
                    {
                        var c = new Creature(cc);
                        creatures.Add(c);
                    }

                    // Add all project NPCs
                    foreach (var npc in Session.Project.NpCs)
                    {
                        var c = new Creature(npc);
                        creatures.Add(c);
                    }
                }
            }
            else
            {
                creatures.AddRange(Session.Creatures);
            }

            var categories = new BinarySearchTree<string>();
            foreach (var c in creatures)
            {
                if (c == null)
                    continue;

                if (c.Category != "")
                    categories.Add(c.Category);
            }

            var cats = categories.SortedList;
            cats.Insert(0, "Custom Creatures");
            cats.Insert(1, "NPCs");
            cats.Add("Miscellaneous Creatures");

            CreatureList.Groups.Clear();
            foreach (var cat in cats)
                CreatureList.Groups.Add(cat, cat);
            CreatureList.ShowGroups = true;

            var listItems = new List<ListViewItem>();

            foreach (var c in creatures)
            {
                if (c == null)
                    continue;

                if (CreatureSearchToolbar.Visible)
                {
                    if (SearchBox.Text != "")
                    {
                        var search = SearchBox.Text.ToLower();
                        var matched = c.Name.ToLower().Contains(search);

                        if (!matched && c.Category != null && c.Category.ToLower().Contains(search))
                            matched = true;

                        if (!matched)
                            continue;
                    }

                    var show = false;
                    var categorised = c.Category != null && c.Category != "";

                    if (_fShowCategorised && categorised)
                        show = true;
                    if (_fShowUncategorised && !categorised)
                        show = true;

                    if (!show)
                        continue;
                }

                var lvi = new ListViewItem(c.Name);
                lvi.SubItems.Add(c.Info);
                lvi.Tag = c;

                if (c.Category != "")
                    lvi.Group = CreatureList.Groups[c.Category];
                else
                    lvi.Group = CreatureList.Groups["Miscellaneous Creatures"];

                listItems.Add(lvi);
            }

            CreatureList.Items.Clear();
            CreatureList.Items.AddRange(listItems.ToArray());

            if (CreatureList.Items.Count == 0)
            {
                CreatureList.ShowGroups = false;

                var lvi = CreatureList.Items.Add("(no creatures)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            CreatureList.Sort();

            ListState.SetState(CreatureList, state);
            CreatureList.EndUpdate();

            Cursor.Current = Cursors.Default;
        }

        private void TemplateAddBtn_Click(object sender, EventArgs e)
        {
            var ct = new CreatureTemplate();
            ct.Name = "New Template";

            var dlg = new CreatureTemplateBuilderForm(ct);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedLibrary.Templates.Add(dlg.Template);
                _fModified[SelectedLibrary] = true;

                update_content(true);
            }
        }

        private void TemplateAddTheme_Click(object sender, EventArgs e)
        {
            var mt = new MonsterTheme();
            mt.Name = "New Theme";

            var dlg = new MonsterThemeForm(mt);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedLibrary.Themes.Add(dlg.Theme);
                _fModified[SelectedLibrary] = true;

                update_content(true);
            }
        }

        private void TemplateImport_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary != null)
            {
                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                    return;

                var dlg = new OpenFileDialog();
                dlg.Filter = Program.CreatureTemplateAndThemeFilter;
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                    foreach (var filename in dlg.FileNames)
                    {
                        if (filename.EndsWith("creaturetemplate"))
                        {
                            var ct = Serialisation<CreatureTemplate>.Load(filename, SerialisationMode.Binary);
                            if (ct != null)
                            {
                                SelectedLibrary.Templates.Add(ct);
                                _fModified[SelectedLibrary] = true;

                                update_content(true);
                            }
                        }

                        if (filename.EndsWith("theme"))
                        {
                            var theme = Serialisation<MonsterTheme>.Load(filename, SerialisationMode.Binary);
                            if (theme != null)
                            {
                                SelectedLibrary.Themes.Add(theme);
                                _fModified[SelectedLibrary] = true;

                                update_content(true);
                            }
                        }
                    }
            }
        }

        private void TemplateRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTemplates.Count != 0)
            {
                var str = "Are you sure you want to delete your selection?";
                if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                    return;

                foreach (var ct in SelectedTemplates)
                {
                    var lib = SelectedLibrary;
                    if (lib == null)
                        lib = Session.FindLibrary(ct);

                    lib.Templates.Remove(ct);
                    _fModified[lib] = true;
                }

                update_content(true);
            }

            if (SelectedThemes.Count != 0)
            {
                foreach (var mt in SelectedThemes)
                {
                    var lib = SelectedLibrary;
                    if (lib == null)
                        lib = Session.FindLibrary(mt);

                    lib.Themes.Remove(mt);
                    _fModified[lib] = true;
                }

                update_content(true);
            }
        }

        private void TemplateEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTemplates.Count == 1)
            {
                var lib = Session.FindLibrary(SelectedTemplates[0]);
                if (lib == null)
                    return;

                var index = lib.Templates.IndexOf(SelectedTemplates[0]);

                var dlg = new CreatureTemplateBuilderForm(SelectedTemplates[0]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    lib.Templates[index] = dlg.Template;
                    _fModified[lib] = true;

                    update_content(true);
                }
            }

            if (SelectedThemes.Count == 1)
            {
                var lib = Session.FindLibrary(SelectedThemes[0]);
                if (lib == null)
                    return;

                var index = lib.Themes.IndexOf(SelectedThemes[0]);

                var dlg = new MonsterThemeForm(SelectedThemes[0]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    lib.Themes[index] = dlg.Theme;
                    _fModified[lib] = true;

                    update_content(true);
                }
            }
        }

        private void TemplateCutBtn_Click(object sender, EventArgs e)
        {
            TemplateCopyBtn_Click(sender, e);
            TemplateRemoveBtn_Click(sender, e);
        }

        private void TemplateCopyBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTemplates.Count != 0)
                Clipboard.SetData(typeof(List<CreatureTemplate>).ToString(), SelectedTemplates);

            if (SelectedThemes.Count != 0) Clipboard.SetData(typeof(List<MonsterTheme>).ToString(), SelectedThemes);
        }

        private void TemplatePasteBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary == null)
                return;

            if (Clipboard.ContainsData(typeof(List<CreatureTemplate>).ToString()))
            {
                var templates = Clipboard.GetData(typeof(List<CreatureTemplate>).ToString()) as List<CreatureTemplate>;
                foreach (var ct in templates)
                {
                    // Make a copy with a new ID
                    var template = ct.Copy();
                    template.Id = Guid.NewGuid();

                    SelectedLibrary.Templates.Add(template);
                    _fModified[SelectedLibrary] = true;
                }

                update_content(true);
            }

            if (Clipboard.ContainsData(typeof(List<MonsterTheme>).ToString()))
            {
                var themes = Clipboard.GetData(typeof(List<MonsterTheme>).ToString()) as List<MonsterTheme>;
                foreach (var mt in themes)
                {
                    // Make a copy with a new ID
                    var theme = mt.Copy();
                    theme.Id = Guid.NewGuid();

                    SelectedLibrary.Themes.Add(theme);
                    _fModified[SelectedLibrary] = true;
                }

                update_content(true);
            }
        }

        private void TemplateStatBlock_Click(object sender, EventArgs e)
        {
            if (SelectedTemplates.Count != 1)
                return;

            var dlg = new CreatureTemplateDetailsForm(SelectedTemplates[0]);
            dlg.ShowDialog();
        }

        private void TemplateToolsExport_Click(object sender, EventArgs e)
        {
            if (SelectedTemplates.Count == 1)
            {
                var dlg = new SaveFileDialog();
                dlg.Title = "Export";
                dlg.Filter = Program.CreatureTemplateFilter;
                dlg.FileName = SelectedTemplates[0].Name;

                if (dlg.ShowDialog() == DialogResult.OK)
                    Serialisation<CreatureTemplate>.Save(dlg.FileName, SelectedTemplates[0], SerialisationMode.Binary);
            }
            else if (SelectedThemes.Count == 1)
            {
                var dlg = new SaveFileDialog();
                dlg.Title = "Export";
                dlg.Filter = Program.ThemeFilter;
                dlg.FileName = SelectedThemes[0].Name;

                if (dlg.ShowDialog() == DialogResult.OK)
                    Serialisation<MonsterTheme>.Save(dlg.FileName, SelectedThemes[0], SerialisationMode.Binary);
            }
        }

        private void TemplateList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedTemplates.Count != 0)
                DoDragDrop(SelectedTemplates, DragDropEffects.Move);
        }

        private void TemplateContextRemove_Click(object sender, EventArgs e)
        {
            TemplateRemoveBtn_Click(sender, e);
        }

        private void TemplateFunctional_Click(object sender, EventArgs e)
        {
            foreach (var ct in SelectedTemplates)
            {
                ct.Type = CreatureTemplateType.Functional;

                var lib = Session.FindLibrary(ct);
                if (lib != null)
                    _fModified[lib] = true;
            }

            update_templates();
        }

        private void TemplateClass_Click(object sender, EventArgs e)
        {
            foreach (var ct in SelectedTemplates)
            {
                ct.Type = CreatureTemplateType.Class;

                var lib = Session.FindLibrary(ct);
                if (lib != null)
                    _fModified[lib] = true;
            }

            update_templates();
        }

        private void update_templates()
        {
            Cursor.Current = Cursors.WaitCursor;

            TemplateList.BeginUpdate();
            var state = ListState.GetState(TemplateList);

            var templates = new List<CreatureTemplate>();
            var themes = new List<MonsterTheme>();
            if (SelectedLibrary != null)
            {
                templates.AddRange(SelectedLibrary.Templates);
                themes.AddRange(SelectedLibrary.Themes);
            }
            else
            {
                foreach (var lib in Session.Libraries)
                {
                    templates.AddRange(lib.Templates);
                    themes.AddRange(lib.Themes);
                }
            }

            TemplateList.Items.Clear();

            TemplateList.ShowGroups = true;

            foreach (var ct in templates)
            {
                if (ct == null)
                    continue;

                var lvi = TemplateList.Items.Add(ct.Name);
                lvi.SubItems.Add(ct.Info);
                lvi.Tag = ct;
                lvi.Group = TemplateList.Groups[ct.Type == CreatureTemplateType.Functional ? 0 : 1];
            }

            foreach (var mt in themes)
            {
                if (mt == null)
                    continue;

                var lvi = TemplateList.Items.Add(mt.Name);
                lvi.Tag = mt;
                lvi.Group = TemplateList.Groups[2];
            }

            if (TemplateList.Items.Count == 0)
            {
                TemplateList.ShowGroups = false;

                var lvi = TemplateList.Items.Add("(no templates or themes)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            TemplateList.Sort();

            ListState.SetState(TemplateList, state);
            TemplateList.EndUpdate();

            Cursor.Current = Cursors.Default;
        }

        private void TrapAddBtn_Click(object sender, EventArgs e)
        {
            var t = new Trap();
            t.Name = "New Trap";
            t.Attacks.Add(new TrapAttack());

            var dlg = new TrapBuilderForm(t);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedLibrary.Traps.Add(dlg.Trap);
                _fModified[SelectedLibrary] = true;

                update_content(true);
            }
        }

        private void TrapAddImport_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary != null)
            {
                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                    return;

                var dlg = new OpenFileDialog();
                dlg.Filter = Program.TrapFilter;
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                    foreach (var filename in dlg.FileNames)
                    {
                        var t = Serialisation<Trap>.Load(filename, SerialisationMode.Binary);
                        if (t != null)
                        {
                            var previous = SelectedLibrary.FindTrap(t.Name, t.Level, t.Role.ToString());
                            if (previous != null)
                            {
                                t.Id = previous.Id;

                                SelectedLibrary.Traps.Remove(previous);
                            }

                            SelectedLibrary.Traps.Add(t);
                            _fModified[SelectedLibrary] = true;

                            update_content(true);
                        }
                    }
            }
        }

        private void TrapRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTraps.Count != 0)
            {
                var str = "Are you sure you want to delete your selection?";
                if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                    return;

                foreach (var t in SelectedTraps)
                {
                    var lib = SelectedLibrary;
                    if (lib == null)
                        lib = Session.FindLibrary(t);

                    lib.Traps.Remove(t);
                    _fModified[lib] = true;
                }

                update_content(true);
            }
        }

        private void TrapEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTraps.Count == 1)
            {
                var lib = Session.FindLibrary(SelectedTraps[0]);
                if (lib == null)
                    return;

                var index = lib.Traps.IndexOf(SelectedTraps[0]);

                var dlg = new TrapBuilderForm(SelectedTraps[0]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    lib.Traps[index] = dlg.Trap;
                    _fModified[lib] = true;

                    update_content(true);
                }
            }
        }

        private void TrapCutBtn_Click(object sender, EventArgs e)
        {
            TrapCopyBtn_Click(sender, e);
            TrapRemoveBtn_Click(sender, e);
        }

        private void TrapCopyBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTraps.Count != 0) Clipboard.SetData(typeof(List<Trap>).ToString(), SelectedTraps);
        }

        private void TrapPasteBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary == null)
                return;

            if (Clipboard.ContainsData(typeof(List<Trap>).ToString()))
            {
                var traps = Clipboard.GetData(typeof(List<Trap>).ToString()) as List<Trap>;
                foreach (var t in traps)
                {
                    // Make a copy with a new ID
                    var trap = t.Copy();
                    trap.Id = Guid.NewGuid();

                    SelectedLibrary.Traps.Add(trap);
                    _fModified[SelectedLibrary] = true;
                }

                update_content(true);
            }
        }

        private void TrapStatBlockBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTraps.Count != 1)
                return;

            var dlg = new TrapDetailsForm(SelectedTraps[0]);
            dlg.ShowDialog();
        }

        private void TrapDemoBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new DemographicsForm(SelectedLibrary, DemographicsSource.Traps);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void TrapToolsExport_Click(object sender, EventArgs e)
        {
            if (SelectedTraps.Count == 1)
            {
                var dlg = new SaveFileDialog();
                dlg.Title = "Export";
                dlg.Filter = Program.TrapFilter;
                dlg.FileName = SelectedTraps[0].Name;

                if (dlg.ShowDialog() == DialogResult.OK)
                    Serialisation<Trap>.Save(dlg.FileName, SelectedTraps[0], SerialisationMode.Binary);
            }
        }

        private void TrapList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedTraps.Count != 0)
                DoDragDrop(SelectedTraps, DragDropEffects.Move);
        }

        private void TrapContextRemove_Click(object sender, EventArgs e)
        {
            TrapRemoveBtn_Click(sender, e);
        }

        private void TrapTrap_Click(object sender, EventArgs e)
        {
            foreach (var t in SelectedTraps)
            {
                t.Type = TrapType.Trap;

                var lib = Session.FindLibrary(t);
                if (lib != null)
                    _fModified[lib] = true;
            }

            update_traps();
        }

        private void TrapHazard_Click(object sender, EventArgs e)
        {
            foreach (var t in SelectedTraps)
            {
                t.Type = TrapType.Hazard;

                var lib = Session.FindLibrary(t);
                if (lib != null)
                    _fModified[lib] = true;
            }

            update_traps();
        }

        private void update_traps()
        {
            Cursor.Current = Cursors.WaitCursor;

            TrapList.BeginUpdate();
            var state = ListState.GetState(TrapList);

            var traps = new List<Trap>();
            if (SelectedLibrary != null)
            {
                traps.AddRange(SelectedLibrary.Traps);

                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                {
                    var points = Session.Project.AllPlotPoints;
                    foreach (var pp in points)
                    {
                        if (pp.Element is Encounter)
                        {
                            var enc = pp.Element as Encounter;
                            traps.AddRange(enc.Traps);
                        }

                        if (pp.Element is Trap)
                            traps.Add(pp.Element as Trap);
                    }
                }
            }
            else
            {
                foreach (var lib in Session.Libraries) traps.AddRange(lib.Traps);
            }

            TrapList.Items.Clear();

            TrapList.ShowGroups = true;

            var listItems = new List<ListViewItem>();

            foreach (var t in traps)
            {
                if (t == null)
                    continue;

                var lvi = new ListViewItem(t.Name);
                lvi.SubItems.Add(t.Info);
                lvi.Tag = t;
                lvi.Group = TrapList.Groups[t.Type == TrapType.Trap ? 0 : 1];

                listItems.Add(lvi);
            }

            TrapList.Items.AddRange(listItems.ToArray());

            if (TrapList.Items.Count == 0)
            {
                TrapList.ShowGroups = false;

                var lvi = TrapList.Items.Add("(no traps)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            TrapList.Sort();

            ListState.SetState(TrapList, state);
            TrapList.EndUpdate();

            Cursor.Current = Cursors.Default;
        }

        private void ChallengeAddBtn_Click(object sender, EventArgs e)
        {
            var t = new SkillChallenge();
            t.Name = "New Skill Challenge";

            var dlg = new SkillChallengeBuilderForm(t);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedLibrary.SkillChallenges.Add(dlg.SkillChallenge);
                _fModified[SelectedLibrary] = true;

                update_content(true);
            }
        }

        private void ChallengeAddImport_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary != null)
            {
                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                    return;

                var dlg = new OpenFileDialog();
                dlg.Filter = Program.SkillChallengeFilter;
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                    foreach (var filename in dlg.FileNames)
                    {
                        var c = Serialisation<SkillChallenge>.Load(filename, SerialisationMode.Binary);
                        if (c != null)
                        {
                            SelectedLibrary.SkillChallenges.Add(c);
                            _fModified[SelectedLibrary] = true;

                            update_content(true);
                        }
                    }
            }
        }

        private void ChallengeRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedChallenges.Count != 0)
            {
                var str = "Are you sure you want to delete your selection?";
                if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                    return;

                foreach (var sc in SelectedChallenges)
                {
                    var lib = SelectedLibrary;
                    if (lib == null)
                        lib = Session.FindLibrary(sc);

                    lib.SkillChallenges.Remove(sc);
                    _fModified[lib] = true;
                }

                update_content(true);
            }
        }

        private void ChallengeEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedChallenges.Count == 1)
            {
                var lib = Session.FindLibrary(SelectedChallenges[0]);
                if (lib == null)
                    return;

                var index = lib.SkillChallenges.IndexOf(SelectedChallenges[0]);

                var dlg = new SkillChallengeBuilderForm(SelectedChallenges[0]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    lib.SkillChallenges[index] = dlg.SkillChallenge;
                    _fModified[lib] = true;

                    update_content(true);
                }
            }
        }

        private void ChallengeCutBtn_Click(object sender, EventArgs e)
        {
            ChallengeCopyBtn_Click(sender, e);
            ChallengeRemoveBtn_Click(sender, e);
        }

        private void ChallengeCopyBtn_Click(object sender, EventArgs e)
        {
            if (SelectedChallenges != null)
                Clipboard.SetData(typeof(List<SkillChallenge>).ToString(), SelectedChallenges);
        }

        private void ChallengePasteBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary == null)
                return;

            if (Clipboard.ContainsData(typeof(List<SkillChallenge>).ToString()))
            {
                var challenges = Clipboard.GetData(typeof(List<SkillChallenge>).ToString()) as List<SkillChallenge>;
                foreach (var sc in challenges)
                {
                    // Make a copy with a new ID
                    var challenge = sc.Copy() as SkillChallenge;
                    challenge.Id = Guid.NewGuid();

                    SelectedLibrary.SkillChallenges.Add(challenge);
                    _fModified[SelectedLibrary] = true;
                }

                update_content(true);
            }
        }

        private void ChallengeStatBlockBtn_Click(object sender, EventArgs e)
        {
            if (SelectedChallenges.Count != 1)
                return;

            var dlg = new SkillChallengeDetailsForm(SelectedChallenges[0]);
            dlg.ShowDialog();
        }

        private void ChallengeToolsExport_Click(object sender, EventArgs e)
        {
            if (SelectedChallenges.Count == 1)
            {
                var dlg = new SaveFileDialog();
                dlg.Title = "Export";
                dlg.Filter = Program.SkillChallengeFilter;
                dlg.FileName = SelectedChallenges[0].Name;

                if (dlg.ShowDialog() == DialogResult.OK)
                    Serialisation<SkillChallenge>.Save(dlg.FileName, SelectedChallenges[0], SerialisationMode.Binary);
            }
        }

        private void ChallengeList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedChallenges.Count != 0)
                DoDragDrop(SelectedChallenges, DragDropEffects.Move);
        }

        private void ChallengeContextRemove_Click(object sender, EventArgs e)
        {
            ChallengeRemoveBtn_Click(sender, e);
        }

        private void update_challenges()
        {
            Cursor.Current = Cursors.WaitCursor;

            ChallengeList.BeginUpdate();
            var state = ListState.GetState(ChallengeList);

            var challenges = new List<SkillChallenge>();
            if (SelectedLibrary != null)
            {
                challenges.AddRange(SelectedLibrary.SkillChallenges);

                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                {
                    var points = Session.Project.AllPlotPoints;
                    foreach (var pp in points)
                    {
                        if (pp.Element is Encounter)
                        {
                            var enc = pp.Element as Encounter;
                            challenges.AddRange(enc.SkillChallenges);
                        }

                        if (pp.Element is SkillChallenge)
                            challenges.Add(pp.Element as SkillChallenge);
                    }
                }
            }
            else
            {
                foreach (var lib in Session.Libraries) challenges.AddRange(lib.SkillChallenges);
            }

            ChallengeList.Items.Clear();

            ChallengeList.ShowGroups = false;

            foreach (var sc in challenges)
            {
                if (sc == null)
                    continue;

                var lvi = ChallengeList.Items.Add(sc.Name);
                lvi.SubItems.Add(sc.Info);
                lvi.Tag = sc;
            }

            if (ChallengeList.Items.Count == 0)
            {
                var lvi = ChallengeList.Items.Add("(no skill challenges)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            ChallengeList.Sort();

            ListState.SetState(ChallengeList, state);
            ChallengeList.EndUpdate();

            Cursor.Current = Cursors.Default;
        }

        private void MagicItemAddBtn_Click(object sender, EventArgs e)
        {
            var mi = new MagicItem();
            mi.Name = "New Magic Item";

            var dlg = new MagicItemBuilderForm(mi);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedLibrary.MagicItems.Add(dlg.MagicItem);
                _fModified[SelectedLibrary] = true;

                update_content(true);
            }
        }

        private void MagicItemAddImport_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary != null)
            {
                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                    return;

                var dlg = new OpenFileDialog();
                dlg.Filter = Program.MagicItemFilter;
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                    foreach (var filename in dlg.FileNames)
                    {
                        var list = Serialisation<List<MagicItem>>.Load(filename, SerialisationMode.Binary);
                        foreach (var mi in list)
                            if (mi != null)
                            {
                                var previous = SelectedLibrary.FindMagicItem(mi.Name, mi.Level);
                                if (previous != null)
                                {
                                    mi.Id = previous.Id;

                                    SelectedLibrary.MagicItems.Remove(previous);
                                }

                                SelectedLibrary.MagicItems.Add(mi);
                                _fModified[SelectedLibrary] = true;

                                update_content(true);
                            }
                    }
            }
        }

        private void MagicItemRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedMagicItems.Count != 0)
            {
                var str = "Are you sure you want to delete your selection?";
                if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                    return;

                foreach (var mi in SelectedMagicItems)
                {
                    var lib = SelectedLibrary;
                    if (lib == null)
                        lib = Session.FindLibrary(mi);

                    lib.MagicItems.Remove(mi);
                    _fModified[lib] = true;
                }

                update_content(true);
            }
        }

        private void MagicItemEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedMagicItems.Count == 1)
            {
                var lib = Session.FindLibrary(SelectedMagicItems[0]);
                if (lib == null)
                    return;

                var index = lib.MagicItems.IndexOf(SelectedMagicItems[0]);

                var dlg = new MagicItemBuilderForm(SelectedMagicItems[0]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    lib.MagicItems[index] = dlg.MagicItem;
                    _fModified[lib] = true;

                    update_content(true);
                }
            }
        }

        private void MagicItemCutBtn_Click(object sender, EventArgs e)
        {
            MagicItemCopyBtn_Click(sender, e);
            MagicItemRemoveBtn_Click(sender, e);
        }

        private void MagicItemCopyBtn_Click(object sender, EventArgs e)
        {
            if (SelectedMagicItems.Count != 0)
                Clipboard.SetData(typeof(List<MagicItem>).ToString(), SelectedMagicItems);
        }

        private void MagicItemPasteBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary == null)
                return;

            if (Clipboard.ContainsData(typeof(List<MagicItem>).ToString()))
            {
                var items = Clipboard.GetData(typeof(List<MagicItem>).ToString()) as List<MagicItem>;
                foreach (var mi in items)
                {
                    // Make a copy with a new ID
                    var item = mi.Copy();
                    item.Id = Guid.NewGuid();

                    SelectedLibrary.MagicItems.Add(item);
                    _fModified[SelectedLibrary] = true;
                }

                update_content(true);
            }
        }

        private void MagicItemStatBlockBtn_Click(object sender, EventArgs e)
        {
            if (SelectedMagicItems.Count != 1)
                return;

            var dlg = new MagicItemDetailsForm(SelectedMagicItems[0]);
            dlg.ShowDialog();
        }

        private void MagicItemDemoBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new DemographicsForm(SelectedLibrary, DemographicsSource.MagicItems);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MagicItemList_SelectedIndexChanged(object sender, EventArgs e)
        {
            update_magic_item_versions();
        }

        private void MagicItemsToolsExport_Click(object sender, EventArgs e)
        {
            if (SelectedMagicItemSet != "")
            {
                var dlg = new SaveFileDialog();
                dlg.Title = "Export";
                dlg.Filter = Program.MagicItemFilter;
                dlg.FileName = SelectedMagicItemSet;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var items = new List<MagicItem>();
                    if (SelectedLibrary != null)
                        items.AddRange(SelectedLibrary.MagicItems);
                    else
                        foreach (var lib in Session.Libraries)
                            items.AddRange(lib.MagicItems);

                    var list = new List<MagicItem>();
                    foreach (var item in items)
                        if (item.Name == SelectedMagicItemSet)
                            list.Add(item);

                    Serialisation<List<MagicItem>>.Save(dlg.FileName, list, SerialisationMode.Binary);
                }
            }
        }

        private void MagicItemList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedMagicItems.Count != 0)
                DoDragDrop(SelectedMagicItems, DragDropEffects.Move);
        }

        private void MagicItemContextRemove_Click(object sender, EventArgs e)
        {
            MagicItemRemoveBtn_Click(sender, e);
        }

        private void update_magic_item_sets()
        {
            Cursor.Current = Cursors.WaitCursor;

            MagicItemList.BeginUpdate();
            var state = ListState.GetState(MagicItemList);

            var items = new List<MagicItem>();
            if (SelectedLibrary != null)
                items.AddRange(SelectedLibrary.MagicItems);
            else
                foreach (var lib in Session.Libraries)
                    items.AddRange(lib.MagicItems);

            var categorisedItems = new Dictionary<string, BinarySearchTree<string>>();
            foreach (var item in items)
            {
                var cat = item.Type;
                if (cat == "")
                    cat = "Miscallaneous Items";

                if (!categorisedItems.ContainsKey(cat))
                    categorisedItems[cat] = new BinarySearchTree<string>();

                categorisedItems[cat].Add(item.Name);
            }

            var categories = new List<string>();
            categories.AddRange(categorisedItems.Keys);
            categories.Sort();
            var miscIndex = categories.IndexOf("Miscellaneous Items");
            if (miscIndex != -1)
            {
                categories.RemoveAt(miscIndex);
                categories.Add("Miscellaneous Items");
            }

            MagicItemList.Groups.Clear();
            MagicItemList.ShowGroups = true;

            var listItems = new List<ListViewItem>();

            foreach (var cat in categories)
            {
                var lvg = MagicItemList.Groups.Add(cat, cat);

                var itemNames = categorisedItems[cat].SortedList;

                foreach (var item in itemNames)
                {
                    var lvi = new ListViewItem(item);
                    lvi.Group = lvg;

                    listItems.Add(lvi);
                }
            }

            MagicItemList.Items.Clear();
            MagicItemList.Items.AddRange(listItems.ToArray());

            if (MagicItemList.Items.Count == 0)
            {
                MagicItemList.ShowGroups = false;

                var lvi = MagicItemList.Items.Add("(no magic items)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            ListState.SetState(MagicItemList, state);
            MagicItemList.EndUpdate();

            Cursor.Current = Cursors.Default;
        }

        private void update_magic_item_versions()
        {
            if (SelectedMagicItemSet != "")
            {
                MagicItemVersionList.Enabled = true;
                MagicItemVersionList.ShowGroups = true;

                MagicItemVersionList.Items.Clear();

                var items = new List<MagicItem>();
                if (SelectedLibrary != null)
                    items.AddRange(SelectedLibrary.MagicItems);
                else
                    foreach (var lib in Session.Libraries)
                        items.AddRange(lib.MagicItems);

                foreach (var item in items)
                {
                    if (item.Name != SelectedMagicItemSet)
                        continue;

                    var lvi = MagicItemVersionList.Items.Add("Level " + item.Level);
                    lvi.Tag = item;

                    if (item.Level < 11)
                        lvi.Group = MagicItemVersionList.Groups[0];
                    else if (item.Level < 21)
                        lvi.Group = MagicItemVersionList.Groups[1];
                    else
                        lvi.Group = MagicItemVersionList.Groups[2];
                }
            }
            else
            {
                MagicItemVersionList.Enabled = false;
                MagicItemVersionList.ShowGroups = false;

                MagicItemVersionList.Items.Clear();
            }
        }

        private void TileAddBtn_Click(object sender, EventArgs e)
        {
            var t = new Tile();

            var dlg = new TileForm(t);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedLibrary.Tiles.Add(dlg.Tile);
                _fModified[SelectedLibrary] = true;

                update_content(true);
            }
        }

        private void TileAddFolderBtn_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            dlg.Description = "Choose the folder to open.";
            dlg.ShowNewFolderButton = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var dir = new DirectoryInfo(dlg.SelectedPath);

                var extensions = new List<string>();
                extensions.Add("jpg");
                extensions.Add("jpeg");
                extensions.Add("bmp");
                extensions.Add("gif");
                extensions.Add("png");
                extensions.Add("tga");

                var files = new List<FileInfo>();
                foreach (var ext in extensions)
                    files.AddRange(dir.GetFiles("*." + ext));

                var minWidth = int.MaxValue;
                var minHeight = int.MaxValue;
                foreach (var fi in files)
                {
                    var img = Image.FromFile(fi.FullName);

                    if (img.Width < minWidth)
                        minWidth = img.Width;

                    if (img.Height < minHeight)
                        minHeight = img.Height;
                }

                var min = Math.Min(minWidth, minHeight);

                foreach (var fi in files)
                {
                    var t = new Tile();
                    t.Image = Image.FromFile(fi.FullName);
                    t.Size = new Size(t.Image.Width / min, t.Image.Height / min);

                    Program.SetResolution(t.Image);

                    SelectedLibrary.Tiles.Add(t);
                }

                _fModified[SelectedLibrary] = true;

                update_content(true);
            }
        }

        private void TileAddImport_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary != null)
            {
                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                    return;

                var dlg = new OpenFileDialog();
                dlg.Filter = Program.MapTileFilter;
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                    foreach (var filename in dlg.FileNames)
                    {
                        var tile = Serialisation<Tile>.Load(filename, SerialisationMode.Binary);
                        if (tile != null)
                        {
                            SelectedLibrary.Tiles.Add(tile);
                            _fModified[SelectedLibrary] = true;

                            update_content(true);
                        }
                    }
            }
        }

        private void TileSetRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTiles.Count != 0)
            {
                var str = "Are you sure you want to delete your selection?";
                if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                    return;

                foreach (var t in SelectedTiles)
                {
                    var lib = SelectedLibrary;
                    if (lib == null)
                        lib = Session.FindLibrary(t);

                    lib.Tiles.Remove(t);
                    _fModified[lib] = true;
                }

                update_content(true);
            }
        }

        private void TileSetEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTiles.Count == 1)
            {
                var lib = Session.FindLibrary(SelectedTiles[0]);
                if (lib == null)
                    return;

                var index = lib.Tiles.IndexOf(SelectedTiles[0]);

                var dlg = new TileForm(SelectedTiles[0]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    lib.Tiles[index] = dlg.Tile;
                    _fModified[lib] = true;

                    update_content(true);
                }
            }
        }

        private void TileCutBtn_Click(object sender, EventArgs e)
        {
            TileCopyBtn_Click(sender, e);
            TileSetRemoveBtn_Click(sender, e);
        }

        private void TileCopyBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTiles.Count != 0) Clipboard.SetData(typeof(List<Tile>).ToString(), SelectedTiles);
        }

        private void TileToolsExport_Click(object sender, EventArgs e)
        {
            if (SelectedTiles.Count == 1)
            {
                var dlg = new SaveFileDialog();
                dlg.Title = "Export";
                dlg.Filter = Program.MapTileFilter;
                dlg.FileName = SelectedTiles[0].ToString();

                if (dlg.ShowDialog() == DialogResult.OK)
                    Serialisation<Tile>.Save(dlg.FileName, SelectedTiles[0], SerialisationMode.Binary);
            }
        }

        private void TilePasteBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary == null)
                return;

            if (Clipboard.ContainsData(typeof(List<Tile>).ToString()))
            {
                var tiles = Clipboard.GetData(typeof(List<Tile>).ToString()) as List<Tile>;
                foreach (var t in tiles)
                {
                    // Make a copy with a new ID
                    var tile = t.Copy();
                    tile.Id = Guid.NewGuid();

                    SelectedLibrary.Tiles.Add(tile);
                    _fModified[SelectedLibrary] = true;
                }

                update_content(true);
            }
        }

        private void TileSetView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedTiles.Count != 0)
                DoDragDrop(SelectedTiles, DragDropEffects.Move);
        }

        private void TileContextRemove_Click(object sender, EventArgs e)
        {
            TileSetRemoveBtn_Click(sender, e);
        }

        private void TilePlain_Click(object sender, EventArgs e)
        {
            foreach (var t in SelectedTiles)
            {
                t.Category = TileCategory.Plain;

                var lib = Session.FindLibrary(t);
                if (lib != null)
                    _fModified[lib] = true;
            }

            update_tiles();
        }

        private void TileDoorway_Click(object sender, EventArgs e)
        {
            foreach (var t in SelectedTiles)
            {
                t.Category = TileCategory.Doorway;

                var lib = Session.FindLibrary(t);
                if (lib != null)
                    _fModified[lib] = true;
            }

            update_tiles();
        }

        private void TileStairway_Click(object sender, EventArgs e)
        {
            foreach (var t in SelectedTiles)
            {
                t.Category = TileCategory.Stairway;

                var lib = Session.FindLibrary(t);
                if (lib != null)
                    _fModified[lib] = true;
            }

            update_tiles();
        }

        private void TileFeature_Click(object sender, EventArgs e)
        {
            foreach (var t in SelectedTiles)
            {
                t.Category = TileCategory.Feature;

                var lib = Session.FindLibrary(t);
                if (lib != null)
                    _fModified[lib] = true;
            }

            update_tiles();
        }

        private void TileMap_Click(object sender, EventArgs e)
        {
            foreach (var t in SelectedTiles)
            {
                t.Category = TileCategory.Map;

                var lib = Session.FindLibrary(t);
                if (lib != null)
                    _fModified[lib] = true;
            }

            update_tiles();
        }

        private void TileSpecial_Click(object sender, EventArgs e)
        {
            foreach (var t in SelectedTiles)
            {
                t.Category = TileCategory.Special;

                var lib = Session.FindLibrary(t);
                if (lib != null)
                    _fModified[lib] = true;
            }

            update_tiles();
        }

        private void TileContextSize_Click(object sender, EventArgs e)
        {
            var dlg = new TileSizeForm(SelectedTiles);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (var t in SelectedTiles)
                {
                    t.Size = dlg.TileSize;

                    var lib = Session.FindLibrary(t);
                    if (lib != null)
                        _fModified[lib] = true;
                }

                update_tiles();
            }
        }

        private void update_tiles()
        {
            Cursor.Current = Cursors.WaitCursor;

            TileList.BeginUpdate();

            var tiles = new List<Tile>();
            if (SelectedLibrary != null)
                tiles.AddRange(SelectedLibrary.Tiles);
            else
                foreach (var lib in Session.Libraries)
                    tiles.AddRange(lib.Tiles);

            TileList.Groups.Clear();
            TileList.ShowGroups = true;
            var categories = Enum.GetValues(typeof(TileCategory));
            foreach (TileCategory cat in categories)
                TileList.Groups.Add(cat.ToString(), cat.ToString());

            TileList.Items.Clear();

            const int tileSize = 32;

            var listItems = new List<ListViewItem>();

            TileList.LargeImageList = new ImageList();
            TileList.LargeImageList.ColorDepth = ColorDepth.Depth32Bit;
            TileList.LargeImageList.ImageSize = new Size(tileSize, tileSize);

            foreach (var t in tiles)
            {
                var lvi = new ListViewItem(t.ToString());
                lvi.Tag = t;
                lvi.Group = TileList.Groups[t.Category.ToString()];

                // Get tile image
                var img = t.Image ?? t.BlankImage;

                Image bmp = new Bitmap(tileSize, tileSize);
                var g = Graphics.FromImage(bmp);
                if (t.Size.Width > t.Size.Height)
                {
                    var height = t.Size.Height * tileSize / t.Size.Width;
                    var rect = new Rectangle(0, (tileSize - height) / 2, tileSize, height);

                    g.DrawImage(img, rect);
                }
                else
                {
                    var width = t.Size.Width * tileSize / t.Size.Height;
                    var rect = new Rectangle((tileSize - width) / 2, 0, width, tileSize);

                    g.DrawImage(img, rect);
                }

                TileList.LargeImageList.Images.Add(bmp);
                lvi.ImageIndex = TileList.LargeImageList.Images.Count - 1;

                listItems.Add(lvi);
            }

            TileList.Items.AddRange(listItems.ToArray());

            if (TileList.Items.Count == 0)
            {
                TileList.ShowGroups = false;

                var lvi = TileList.Items.Add("(no tiles)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            TileList.EndUpdate();

            Cursor.Current = Cursors.Default;
        }

        private void TPAddBtn_Click(object sender, EventArgs e)
        {
            var t = new TerrainPower();
            t.Name = "New Terrain Power";

            var dlg = new TerrainPowerForm(t);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedLibrary.TerrainPowers.Add(dlg.Power);
                _fModified[SelectedLibrary] = true;

                update_content(true);
            }
        }

        private void TPAddImport_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary != null)
            {
                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                    return;

                var dlg = new OpenFileDialog();
                dlg.Filter = Program.TerrainPowerFilter;
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                    foreach (var filename in dlg.FileNames)
                    {
                        var c = Serialisation<TerrainPower>.Load(filename, SerialisationMode.Binary);
                        if (c != null)
                        {
                            SelectedLibrary.TerrainPowers.Add(c);
                            _fModified[SelectedLibrary] = true;

                            update_content(true);
                        }
                    }
            }
        }

        private void TPRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTerrainPowers.Count != 0)
            {
                var str = "Are you sure you want to delete your selection?";
                if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                    return;

                foreach (var sc in SelectedTerrainPowers)
                {
                    var lib = SelectedLibrary;
                    if (lib == null)
                        lib = Session.FindLibrary(sc);

                    lib.TerrainPowers.Remove(sc);
                    _fModified[lib] = true;
                }

                update_content(true);
            }
        }

        private void TPEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTerrainPowers.Count == 1)
            {
                var lib = Session.FindLibrary(SelectedTerrainPowers[0]);
                if (lib == null)
                    return;

                var index = lib.TerrainPowers.IndexOf(SelectedTerrainPowers[0]);

                var dlg = new TerrainPowerForm(SelectedTerrainPowers[0]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    lib.TerrainPowers[index] = dlg.Power;
                    _fModified[lib] = true;

                    update_content(true);
                }
            }
        }

        private void TPCutBtn_Click(object sender, EventArgs e)
        {
            TPCopyBtn_Click(sender, e);
            TPRemoveBtn_Click(sender, e);
        }

        private void TPCopyBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTerrainPowers != null)
                Clipboard.SetData(typeof(List<TerrainPower>).ToString(), SelectedTerrainPowers);
        }

        private void TPPasteBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary == null)
                return;

            if (Clipboard.ContainsData(typeof(List<TerrainPower>).ToString()))
            {
                var challenges = Clipboard.GetData(typeof(List<TerrainPower>).ToString()) as List<TerrainPower>;
                foreach (var sc in challenges)
                {
                    // Make a copy with a new ID
                    var challenge = sc.Copy();
                    challenge.Id = Guid.NewGuid();

                    SelectedLibrary.TerrainPowers.Add(challenge);
                    _fModified[SelectedLibrary] = true;
                }

                update_content(true);
            }
        }

        private void TPStatBlockBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTerrainPowers.Count != 1)
                return;

            var dlg = new TerrainPowerDetailsForm(SelectedTerrainPowers[0]);
            dlg.ShowDialog();
        }

        private void TPToolsExport_Click(object sender, EventArgs e)
        {
            if (SelectedTerrainPowers.Count == 1)
            {
                var dlg = new SaveFileDialog();
                dlg.Title = "Export";
                dlg.Filter = Program.TerrainPowerFilter;
                dlg.FileName = SelectedTerrainPowers[0].Name;

                if (dlg.ShowDialog() == DialogResult.OK)
                    Serialisation<TerrainPower>.Save(dlg.FileName, SelectedTerrainPowers[0], SerialisationMode.Binary);
            }
        }

        private void TPList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedTerrainPowers.Count != 0)
                DoDragDrop(SelectedTerrainPowers, DragDropEffects.Move);
        }

        private void TPContextRemove_Click(object sender, EventArgs e)
        {
            TPRemoveBtn_Click(sender, e);
        }

        private void update_terrain_powers()
        {
            Cursor.Current = Cursors.WaitCursor;

            TerrainPowerList.BeginUpdate();
            var state = ListState.GetState(TerrainPowerList);

            var challenges = new List<TerrainPower>();
            if (SelectedLibrary != null)
            {
                challenges.AddRange(SelectedLibrary.TerrainPowers);

                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                {
                    var points = Session.Project.AllPlotPoints;
                    foreach (var pp in points)
                        if (pp.Element is Encounter)
                        {
                            var enc = pp.Element as Encounter;
                            foreach (var ct in enc.CustomTokens)
                                if (ct.TerrainPower != null)
                                    challenges.Add(ct.TerrainPower);
                        }
                }
            }
            else
            {
                foreach (var lib in Session.Libraries) challenges.AddRange(lib.TerrainPowers);
            }

            TerrainPowerList.Items.Clear();

            TerrainPowerList.ShowGroups = false;

            foreach (var sc in challenges)
            {
                if (sc == null)
                    continue;

                var lvi = TerrainPowerList.Items.Add(sc.Name);
                lvi.SubItems.Add(sc.Type.ToString());
                lvi.Tag = sc;
            }

            if (TerrainPowerList.Items.Count == 0)
            {
                var lvi = TerrainPowerList.Items.Add("(no terrain powers)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            TerrainPowerList.Sort();

            ListState.SetState(TerrainPowerList, state);
            TerrainPowerList.EndUpdate();

            Cursor.Current = Cursors.Default;
        }

        private void ArtifactAddAdd_Click(object sender, EventArgs e)
        {
            var a = new Artifact();
            a.Name = "New Artifact";

            var dlg = new ArtifactBuilderForm(a);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedLibrary.Artifacts.Add(dlg.Artifact);
                _fModified[SelectedLibrary] = true;

                update_content(true);
            }
        }

        private void ArtifactAddImport_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary != null)
            {
                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                    return;

                var dlg = new OpenFileDialog();
                dlg.Filter = Program.ArtifactFilter;
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                    foreach (var filename in dlg.FileNames)
                    {
                        var a = Serialisation<Artifact>.Load(filename, SerialisationMode.Binary);
                        if (a != null)
                        {
                            SelectedLibrary.Artifacts.Add(a);
                            _fModified[SelectedLibrary] = true;

                            update_content(true);
                        }
                    }
            }
        }

        private void ArtifactRemove_Click(object sender, EventArgs e)
        {
            if (SelectedArtifacts.Count != 0)
            {
                var str = "Are you sure you want to delete your selection?";
                if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                    return;

                foreach (var a in SelectedArtifacts)
                {
                    var lib = SelectedLibrary;
                    if (lib == null)
                        lib = Session.FindLibrary(a);

                    lib.Artifacts.Remove(a);
                    _fModified[lib] = true;
                }

                update_content(true);
            }
        }

        private void ArtifactEdit_Click(object sender, EventArgs e)
        {
            if (SelectedArtifacts.Count == 1)
            {
                var lib = Session.FindLibrary(SelectedArtifacts[0]);
                if (lib == null)
                    return;

                var index = lib.Artifacts.IndexOf(SelectedArtifacts[0]);

                var dlg = new ArtifactBuilderForm(SelectedArtifacts[0]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    lib.Artifacts[index] = dlg.Artifact;
                    _fModified[lib] = true;

                    update_content(true);
                }
            }
        }

        private void ArtifactCut_Click(object sender, EventArgs e)
        {
            ArtifactCopy_Click(sender, e);
            ArtifactRemove_Click(sender, e);
        }

        private void ArtifactCopy_Click(object sender, EventArgs e)
        {
            if (SelectedArtifacts != null) Clipboard.SetData(typeof(List<Artifact>).ToString(), SelectedArtifacts);
        }

        private void ArtifactPaste_Click(object sender, EventArgs e)
        {
            if (SelectedLibrary == null)
                return;

            if (Clipboard.ContainsData(typeof(List<Artifact>).ToString()))
            {
                var artifacts = Clipboard.GetData(typeof(List<Artifact>).ToString()) as List<Artifact>;
                foreach (var a in artifacts)
                {
                    // Make a copy with a new ID
                    var artifact = a.Copy();
                    artifact.Id = Guid.NewGuid();

                    SelectedLibrary.Artifacts.Add(artifact);
                    _fModified[SelectedLibrary] = true;
                }

                update_content(true);
            }
        }

        private void ArtifactStatBlockBtn_Click(object sender, EventArgs e)
        {
            if (SelectedArtifacts.Count != 1)
                return;

            var dlg = new ArtifactDetailsForm(SelectedArtifacts[0]);
            dlg.ShowDialog();
        }

        private void ArtifactToolsExport_Click(object sender, EventArgs e)
        {
            if (SelectedArtifacts.Count == 1)
            {
                var dlg = new SaveFileDialog();
                dlg.Title = "Export";
                dlg.Filter = Program.ArtifactFilter;
                dlg.FileName = SelectedArtifacts[0].Name;

                if (dlg.ShowDialog() == DialogResult.OK)
                    Serialisation<Artifact>.Save(dlg.FileName, SelectedArtifacts[0], SerialisationMode.Binary);
            }
        }

        private void ArtifactList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedArtifacts.Count != 0)
                DoDragDrop(SelectedArtifacts, DragDropEffects.Move);
        }

        private void update_artifacts()
        {
            Cursor.Current = Cursors.WaitCursor;

            ArtifactList.BeginUpdate();
            var state = ListState.GetState(ArtifactList);

            var challenges = new List<Artifact>();
            if (SelectedLibrary != null)
            {
                challenges.AddRange(SelectedLibrary.Artifacts);

                if (Session.Project != null && SelectedLibrary == Session.Project.Library)
                {
                    // Add all from unassigned parcels
                    foreach (var parcel in Session.Project.TreasureParcels)
                        if (parcel.ArtifactId != Guid.Empty)
                        {
                            var a = Session.FindArtifact(parcel.ArtifactId, SearchType.Global);
                            if (a != null)
                                challenges.Add(a);
                        }

                    var points = Session.Project.AllPlotPoints;
                    foreach (var pp in points)
                        // Add all from plot points
                    foreach (var parcel in pp.Parcels)
                        if (parcel.ArtifactId != Guid.Empty)
                        {
                            var a = Session.FindArtifact(parcel.ArtifactId, SearchType.Global);
                            if (a != null)
                                challenges.Add(a);
                        }
                }
            }
            else
            {
                foreach (var lib in Session.Libraries) challenges.AddRange(lib.Artifacts);
            }

            ArtifactList.Items.Clear();

            ArtifactList.ShowGroups = false;

            foreach (var sc in challenges)
            {
                if (sc == null)
                    continue;

                var lvi = ArtifactList.Items.Add(sc.Name);
                lvi.SubItems.Add(sc.Tier.ToString());
                lvi.Tag = sc;
            }

            if (ArtifactList.Items.Count == 0)
            {
                var lvi = ArtifactList.Items.Add("(no artifacts)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            ArtifactList.Sort();

            ListState.SetState(ArtifactList, state);
            ArtifactList.EndUpdate();

            Cursor.Current = Cursors.Default;
        }

        private void Save(Library lib)
        {
            GC.Collect();

            var filename = Session.GetLibraryFilename(lib);
            Serialisation<Library>.Save(filename, lib, SerialisationMode.Binary);
        }

        private void show_help(bool show)
        {
            HelpPanel.Visible = show;
        }

        private void update_libraries()
        {
            LibraryTree.Nodes.Clear();

            if (Session.Libraries.Count != 0)
            {
                var tnAll = LibraryTree.Nodes.Add("All Libraries");
                tnAll.ImageIndex = 0;

                foreach (var lib in Session.Libraries)
                {
                    var lvi = tnAll.Nodes.Add(lib.Name);
                    lvi.Tag = lib;
                }

                tnAll.Expand();
            }
            else
            {
                if (Session.Project == null)
                {
                    var lvi = LibraryTree.Nodes.Add("(no libraries installed)");
                    lvi.ForeColor = SystemColors.GrayText;

                    show_help(true);
                }
            }

            if (Session.Project != null)
            {
                var lvi = LibraryTree.Nodes.Add(Session.Project.Name);
                lvi.Tag = Session.Project.Library;
            }
        }

        private void update_content(bool forceRefresh)
        {
            if (forceRefresh)
                _fCleanPages.Clear();

            if (Pages.SelectedTab == CreaturesPage)
                if (!_fCleanPages.Contains(CreaturesPage))
                {
                    update_creatures();
                    _fCleanPages.Add(CreaturesPage);
                }

            if (Pages.SelectedTab == TemplatesPage)
                if (!_fCleanPages.Contains(TemplatesPage))
                {
                    update_templates();
                    _fCleanPages.Add(TemplatesPage);
                }

            if (Pages.SelectedTab == TrapsPage)
                if (!_fCleanPages.Contains(TrapsPage))
                {
                    update_traps();
                    _fCleanPages.Add(TrapsPage);
                }

            if (Pages.SelectedTab == ChallengePage)
                if (!_fCleanPages.Contains(ChallengePage))
                {
                    update_challenges();
                    _fCleanPages.Add(ChallengePage);
                }

            if (Pages.SelectedTab == MagicItemsPage)
                if (!_fCleanPages.Contains(MagicItemsPage))
                {
                    update_magic_item_sets();
                    update_magic_item_versions();
                    _fCleanPages.Add(MagicItemsPage);
                }

            if (Pages.SelectedTab == TilesPage)
                if (!_fCleanPages.Contains(TilesPage))
                {
                    update_tiles();
                    _fCleanPages.Add(TilesPage);
                }

            if (Pages.SelectedTab == TerrainPowersPage)
                if (!_fCleanPages.Contains(TerrainPowersPage))
                {
                    update_terrain_powers();
                    _fCleanPages.Add(TerrainPowersPage);
                }

            if (Pages.SelectedTab == ArtifactPage)
                if (!_fCleanPages.Contains(ArtifactPage))
                {
                    update_artifacts();
                    _fCleanPages.Add(ArtifactPage);
                }
        }
    }
}
