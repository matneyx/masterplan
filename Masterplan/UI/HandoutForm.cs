using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class HandoutForm : Form
    {
        private readonly List<object> _fItems = new List<object>();
        private readonly List<Type> _fTypes = new List<Type>();
        private bool _fShowDmInfo;

        public object SelectedSource
        {
            get
            {
                if (SourceList.SelectedItems.Count != 0)
                    return SourceList.SelectedItems[0].Tag;

                return null;
            }
        }

        public object SelectedItem
        {
            get
            {
                if (ItemList.SelectedItems.Count != 0)
                    return ItemList.SelectedItems[0].Tag;

                return null;
            }
        }

        public HandoutForm()
        {
            InitializeComponent();
            Browser.DocumentText = "";

            _fTypes.Add(typeof(Background));
            _fTypes.Add(typeof(EncyclopediaEntry));
            _fTypes.Add(typeof(Race));
            _fTypes.Add(typeof(Class));
            _fTypes.Add(typeof(Theme));
            _fTypes.Add(typeof(ParagonPath));
            _fTypes.Add(typeof(EpicDestiny));
            _fTypes.Add(typeof(PlayerBackground));
            _fTypes.Add(typeof(Feat));
            _fTypes.Add(typeof(Weapon));
            _fTypes.Add(typeof(Artifact));
            _fTypes.Add(typeof(Ritual));
            _fTypes.Add(typeof(CreatureLore));
            _fTypes.Add(typeof(Disease));
            _fTypes.Add(typeof(Poison));

            update_source_list();
            update_item_list();
            update_handout();

            Application.Idle += Application_Idle;
        }

        ~HandoutForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            AddBtn.Enabled = SelectedSource != null;
            AddAllBtn.Enabled = SourceList.ShowGroups;
            RemoveBtn.Enabled = SelectedItem != null;
            ClearBtn.Enabled = _fItems.Count != 0;

            UpBtn.Enabled = SelectedItem != null && _fItems.IndexOf(SelectedItem) != 0;
            DownBtn.Enabled = SelectedItem != null && _fItems.IndexOf(SelectedItem) != _fItems.Count - 1;

            ExportBtn.Enabled = _fItems.Count != 0;
            PlayerViewBtn.Enabled = _fItems.Count != 0;

            var hasDmEntries = false;
            foreach (var item in _fItems)
                if (item is EncyclopediaEntry)
                {
                    var entry = item as EncyclopediaEntry;
                    if (entry.DmInfo != "")
                    {
                        hasDmEntries = true;
                        break;
                    }
                }

            DMInfoBtn.Enabled = hasDmEntries;
            DMInfoBtn.Checked = _fShowDmInfo;
        }

        public void AddBackgroundEntries()
        {
            foreach (ListViewItem lvi in SourceList.Items)
            {
                var item = lvi.Tag;

                if (item is Background)
                    _fItems.Add(item);
            }

            update_source_list();
            update_item_list();
            update_handout();
        }

        public void AddEncyclopediaEntries()
        {
            foreach (ListViewItem lvi in SourceList.Items)
            {
                var item = lvi.Tag;

                if (item is EncyclopediaEntry)
                    _fItems.Add(item);
            }

            update_source_list();
            update_item_list();
            update_handout();
        }

        public void AddRulesEntries()
        {
            foreach (ListViewItem lvi in SourceList.Items)
            {
                var item = lvi.Tag;

                if (item is IPlayerOption)
                    _fItems.Add(item);
            }

            update_source_list();
            update_item_list();
            update_handout();
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSource == null)
                return;

            if (_fItems.Contains(SelectedSource))
                return;

            _fItems.Add(SelectedSource);

            update_source_list();
            update_item_list();
            update_handout();
        }

        private void AddAllBtn_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in SourceList.Items)
            {
                var item = lvi.Tag;

                if (_fItems.Contains(item))
                    return;

                _fItems.Add(item);
            }

            update_source_list();
            update_item_list();
            update_handout();
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedItem == null)
                return;

            _fItems.Remove(SelectedItem);

            update_source_list();
            update_item_list();
            update_handout();
        }

        private void RemoveAll_Click(object sender, EventArgs e)
        {
            _fItems.Clear();

            update_source_list();
            update_item_list();
            update_handout();
        }

        private void UpBtn_Click(object sender, EventArgs e)
        {
            if (SelectedItem == null)
                return;

            var index = _fItems.IndexOf(SelectedItem);
            if (index == 0)
                return;

            var tmp = _fItems[index - 1];
            _fItems[index - 1] = SelectedItem;
            _fItems[index] = tmp;

            update_item_list();
            update_handout();

            ItemList.SelectedIndices.Add(index - 1);
        }

        private void DownBtn_Click(object sender, EventArgs e)
        {
            if (SelectedItem == null)
                return;

            var index = _fItems.IndexOf(SelectedItem);
            if (index == _fItems.Count - 1)
                return;

            var tmp = _fItems[index + 1];
            _fItems[index + 1] = SelectedItem;
            _fItems[index] = tmp;

            update_item_list();
            update_handout();

            ItemList.SelectedIndices.Add(index + 1);
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            if (_fItems.Count == 0)
                return;

            var dlg = new SaveFileDialog();
            dlg.Filter = Program.HtmlFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(dlg.FileName, Browser.DocumentText);
                Process.Start(dlg.FileName);
            }
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowHandout(_fItems, _fShowDmInfo);
        }

        private void update_source_list()
        {
            var bst = new BinarySearchTree<string>();
            foreach (var entry in Session.Project.Encyclopedia.Entries)
                if (entry.Category != null && entry.Category != "")
                    bst.Add(entry.Category);
            var cats = bst.SortedList;
            cats.Insert(0, "Background Items");
            cats.Add("Miscellaneous");
            cats.Add("Player Options");

            SourceList.Groups.Clear();
            foreach (var cat in cats)
                SourceList.Groups.Add(cat, cat);
            SourceList.ShowGroups = true;

            SourceList.Items.Clear();

            foreach (var bg in Session.Project.Backgrounds)
            {
                if (_fItems.Contains(bg))
                    continue;

                if (bg.Details == "")
                    continue;

                var lvi = SourceList.Items.Add(bg.Title);
                lvi.Tag = bg;
                lvi.Group = SourceList.Groups["Background Items"];
            }

            foreach (var entry in Session.Project.Encyclopedia.Entries)
            {
                if (_fItems.Contains(entry))
                    continue;

                if (entry.Details == "")
                    continue;

                var lvi = SourceList.Items.Add(entry.Name);
                lvi.Tag = entry;

                if (entry.Category != null && entry.Category != "")
                    lvi.Group = SourceList.Groups[entry.Category];
                else
                    lvi.Group = SourceList.Groups["Miscellaneous"];
            }

            foreach (var option in Session.Project.PlayerOptions)
            {
                if (_fItems.Contains(option))
                    continue;

                var lvi = SourceList.Items.Add(option.Name);
                lvi.Tag = option;
                lvi.Group = SourceList.Groups["Player Options"];
            }

            if (SourceList.Items.Count == 0)
            {
                SourceList.ShowGroups = false;
                var lvi = SourceList.Items.Add("(no items)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void update_item_list()
        {
            ItemList.Items.Clear();

            foreach (var item in _fItems)
            {
                var lvi = ItemList.Items.Add(item.ToString());
                lvi.Tag = item;
            }

            if (ItemList.Items.Count == 0)
            {
                var lvi = ItemList.Items.Add("(no items)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void update_handout()
        {
            Browser.Document.OpenNew(true);
            Browser.Document.Write(Html.Handout(_fItems, Session.Preferences.TextSize, _fShowDmInfo));
        }

        private void SourceList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedSource == null)
                return;

            DoDragDrop(SelectedSource, DragDropEffects.Move);
        }

        private void SourceList_DragOver(object sender, DragEventArgs e)
        {
            foreach (var type in _fTypes)
            {
                var obj = e.Data.GetData(type);
                if (obj != null && _fItems.Contains(obj))
                {
                    e.Effect = DragDropEffects.Move;
                    return;
                }
            }
        }

        private void SourceList_DragDrop(object sender, DragEventArgs e)
        {
            foreach (var type in _fTypes)
            {
                var obj = e.Data.GetData(type);
                if (obj != null && _fItems.Contains(obj))
                {
                    _fItems.Remove(obj);

                    update_source_list();
                    update_item_list();
                    update_handout();

                    return;
                }
            }
        }

        private void ItemList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedItem == null)
                return;

            DoDragDrop(SelectedItem, DragDropEffects.Move);
        }

        private void ItemList_DragOver(object sender, DragEventArgs e)
        {
            foreach (var type in _fTypes)
            {
                var obj = e.Data.GetData(type);
                if (obj != null && !_fItems.Contains(obj))
                {
                    e.Effect = DragDropEffects.Move;
                    return;
                }
            }
        }

        private void ItemList_DragDrop(object sender, DragEventArgs e)
        {
            foreach (var type in _fTypes)
            {
                var obj = e.Data.GetData(type);
                if (obj != null && !_fItems.Contains(obj))
                {
                    _fItems.Add(obj);

                    update_source_list();
                    update_item_list();
                    update_handout();

                    return;
                }
            }
        }

        private void DMInfoBtn_Click(object sender, EventArgs e)
        {
            _fShowDmInfo = !_fShowDmInfo;
            update_handout();
        }
    }
}
