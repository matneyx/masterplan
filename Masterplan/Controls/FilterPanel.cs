using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.UI;

namespace Masterplan.Controls
{
    internal partial class FilterPanel : UserControl
    {
        private string[] _fCatTokens;
        private string[] _fKeyTokens;

        private ListMode _fMode = ListMode.Creatures;

        private string[] _fNameTokens;

        private bool _fSuppressEvents;

        public ListMode Mode
        {
            get => _fMode;
            set
            {
                _fMode = value;
                update_allowed_filters();
            }
        }

        public int PartyLevel { get; set; }

        public FilterPanel()
        {
            InitializeComponent();

            var roles = Enum.GetValues(typeof(RoleType));
            foreach (RoleType role in roles)
                FilterRoleBox.Items.Add(role);
            FilterRoleBox.SelectedIndex = 0;

            FilterModBox.Items.Add("Standard");
            FilterModBox.Items.Add("Elite");
            FilterModBox.Items.Add("Solo");
            FilterModBox.Items.Add("Minion");
            FilterModBox.SelectedIndex = 0;

            var origins = Enum.GetValues(typeof(CreatureOrigin));
            foreach (CreatureOrigin origin in origins)
                FilterOriginBox.Items.Add(origin);
            if (FilterOriginBox.Items.Count != 0)
                FilterOriginBox.SelectedIndex = 0;

            var types = Enum.GetValues(typeof(CreatureType));
            foreach (CreatureType type in types)
                FilterTypeBox.Items.Add(type);
            if (FilterTypeBox.Items.Count != 0)
                FilterTypeBox.SelectedIndex = 0;

            foreach (var lib in Session.Libraries)
                FilterSourceBox.Items.Add(lib);
            if (FilterSourceBox.Items.Count != 0)
                FilterSourceBox.SelectedIndex = 0;

            update_allowed_filters();
            update_option_state();

            open_close_editor(false);
        }

        public bool AllowItem(object obj, out Difficulty diff)
        {
            diff = Difficulty.Moderate;

            if (obj is ICreature)
            {
                var c = obj as ICreature;

                var outlier = false;
                diff = Ai.GetThreatDifficulty(c.Level, PartyLevel);
                if (diff == Difficulty.Trivial || diff == Difficulty.Extreme)
                    outlier = true;

                if (outlier && FilterLevelAppropriateToggle.Checked)
                    return false;

                if (FilterNameToggle.Checked && _fNameTokens != null)
                {
                    var name = c.Name.ToLower();

                    foreach (var token in _fNameTokens)
                        if (!name.Contains(token))
                            return false;
                }

                if (FilterCatToggle.Checked && _fCatTokens != null)
                {
                    var cat = c.Category.ToLower();

                    foreach (var token in _fCatTokens)
                        if (!cat.Contains(token))
                            return false;
                }

                if (FilterRoleToggle.Checked)
                {
                    var role = (RoleType)FilterRoleBox.SelectedItem;

                    if (c.Role is ComplexRole)
                    {
                        var cr = c.Role as ComplexRole;
                        if (cr.Type != role)
                            return false;
                    }

                    if (c.Role is Minion)
                    {
                        var m = c.Role as Minion;
                        if (!m.HasRole || m.Type != role)
                            return false;
                    }
                }

                if (FilterModToggle.Checked)
                {
                    var flag = RoleFlag.Standard;
                    var minion = false;

                    if (FilterModBox.Text == "Standard")
                    {
                    }

                    if (FilterModBox.Text == "Elite") flag = RoleFlag.Elite;

                    if (FilterModBox.Text == "Solo") flag = RoleFlag.Solo;

                    if (FilterModBox.Text == "Minion") minion = true;

                    var cr = c.Role as ComplexRole;
                    if (cr != null)
                    {
                        if (minion)
                            return false;

                        if (flag != cr.Flag)
                            return false;
                    }

                    var m = c.Role as Minion;
                    if (m != null)
                        if (!minion)
                            return false;
                }

                if (FilterOriginToggle.Checked)
                {
                    var origin = (CreatureOrigin)FilterOriginBox.SelectedItem;

                    if (c.Origin != origin)
                        return false;
                }

                if (FilterTypeToggle.Checked)
                {
                    var type = (CreatureType)FilterTypeBox.SelectedItem;

                    if (c.Type != type)
                        return false;
                }

                if (FilterKeywordToggle.Checked && _fKeyTokens != null)
                {
                    var keywords = c.Keywords != null ? c.Keywords.ToLower() : "";

                    foreach (var token in _fKeyTokens)
                        if (!keywords.Contains(token))
                            return false;
                }

                if (FilterLevelToggle.Checked)
                    if (c.Level < LevelFromBox.Value || c.Level > LevelToBox.Value)
                        return false;

                if (FilterSourceToggle.Checked)
                {
                    var creature = c as Creature;
                    if (creature == null)
                        return false;

                    var lib = FilterSourceBox.SelectedItem as Library;
                    if (!lib.Creatures.Contains(creature))
                        return false;
                }

                return true;
            }

            if (obj is CreatureTemplate)
            {
                var ct = obj as CreatureTemplate;

                if (FilterNameToggle.Checked && _fNameTokens != null)
                {
                    var name = ct.Name.ToLower();

                    foreach (var token in _fNameTokens)
                        if (!name.Contains(token))
                            return false;
                }

                if (FilterCatToggle.Checked && _fCatTokens != null)
                {
                    // Ignore category for templates
                }

                if (FilterRoleToggle.Checked)
                {
                    // Match on role
                    var role = (RoleType)FilterRoleBox.SelectedItem;
                    if (ct.Role != role)
                        return false;
                }

                if (FilterOriginToggle.Checked)
                {
                    // Ignore origin for templates
                }

                if (FilterTypeToggle.Checked)
                {
                    // Ignore type for templates
                }

                if (FilterKeywordToggle.Checked && _fKeyTokens != null)
                {
                    // Ignore keywords for templates
                }

                if (FilterSourceToggle.Checked)
                {
                    var lib = FilterSourceBox.SelectedItem as Library;
                    if (!lib.Templates.Contains(ct))
                        return false;
                }

                return true;
            }

            if (obj is Npc)
            {
                var npc = obj as Npc;

                var outlier = false;
                diff = Ai.GetThreatDifficulty(npc.Level, PartyLevel);
                if (diff == Difficulty.Trivial || diff == Difficulty.Extreme)
                    outlier = true;

                if (outlier && FilterLevelAppropriateToggle.Checked)
                    return false;

                if (FilterNameToggle.Checked && _fNameTokens != null)
                {
                    var name = npc.Name.ToLower();

                    foreach (var token in _fNameTokens)
                        if (!name.Contains(token))
                            return false;
                }

                if (FilterCatToggle.Checked && _fCatTokens != null)
                {
                    var cat = npc.Category.ToLower();

                    foreach (var token in _fCatTokens)
                        if (!cat.Contains(token))
                            return false;
                }

                if (FilterRoleToggle.Checked)
                {
                    // Match on role
                    var role = (RoleType)FilterRoleBox.SelectedItem;

                    if (npc.Role is ComplexRole)
                    {
                        var cr = npc.Role as ComplexRole;
                        if (cr.Type != role)
                            return false;
                    }

                    if (npc.Role is Minion)
                    {
                        var m = npc.Role as Minion;
                        if (!m.HasRole || m.Type != role)
                            return false;
                    }
                }

                if (FilterOriginToggle.Checked)
                {
                    var origin = (CreatureOrigin)FilterOriginBox.SelectedItem;

                    if (npc.Origin != origin)
                        return false;
                }

                if (FilterTypeToggle.Checked)
                {
                    var type = (CreatureType)FilterTypeBox.SelectedItem;

                    if (npc.Type != type)
                        return false;
                }

                if (FilterKeywordToggle.Checked && _fKeyTokens != null)
                {
                    var keywords = npc.Keywords != null ? npc.Keywords.ToLower() : "";

                    foreach (var token in _fKeyTokens)
                        if (!keywords.Contains(token))
                            return false;
                }

                if (FilterLevelToggle.Checked)
                    if (npc.Level < LevelFromBox.Value || npc.Level > LevelToBox.Value)
                        return false;

                return true;
            }

            if (obj is Trap)
            {
                var trap = obj as Trap;

                var outlier = false;
                diff = Ai.GetThreatDifficulty(trap.Level, PartyLevel);
                if (diff == Difficulty.Trivial || diff == Difficulty.Extreme)
                    outlier = true;

                if (outlier && FilterLevelAppropriateToggle.Checked)
                    return false;

                if (FilterNameToggle.Checked && _fNameTokens != null)
                {
                    var name = trap.Name.ToLower();

                    foreach (var token in _fNameTokens)
                        if (!name.Contains(token))
                            return false;
                }

                if (FilterCatToggle.Checked && _fCatTokens != null)
                {
                    // Ignore category for traps
                }

                if (FilterRoleToggle.Checked)
                {
                    // Match on role
                    var role = (RoleType)FilterRoleBox.SelectedItem;

                    if (trap.Role is ComplexRole)
                    {
                        var cr = trap.Role as ComplexRole;
                        if (cr.Type != role)
                            return false;
                    }

                    if (trap.Role is Minion)
                    {
                        var m = trap.Role as Minion;
                        if (!m.HasRole || m.Type != role)
                            return false;
                    }
                }

                if (FilterModToggle.Checked)
                {
                    var flag = RoleFlag.Standard;
                    var minion = false;

                    if (FilterModBox.Text == "Standard")
                    {
                    }

                    if (FilterModBox.Text == "Elite") flag = RoleFlag.Elite;

                    if (FilterModBox.Text == "Solo") flag = RoleFlag.Solo;

                    if (FilterModBox.Text == "Minion") minion = true;

                    var cr = trap.Role as ComplexRole;
                    if (cr != null)
                    {
                        if (minion)
                            return false;

                        if (flag != cr.Flag)
                            return false;
                    }

                    var m = trap.Role as Minion;
                    if (m != null)
                        if (!minion)
                            return false;
                }

                if (FilterOriginToggle.Checked)
                {
                    // Ignore origin for traps
                }

                if (FilterTypeToggle.Checked)
                {
                    // Ignore type for traps
                }

                if (FilterKeywordToggle.Checked && _fKeyTokens != null)
                {
                    // Ignore keywords for traps
                }

                if (FilterLevelToggle.Checked)
                    if (trap.Level < LevelFromBox.Value || trap.Level > LevelToBox.Value)
                        return false;

                if (FilterSourceToggle.Checked)
                {
                    var lib = FilterSourceBox.SelectedItem as Library;
                    if (!lib.Traps.Contains(trap))
                        return false;
                }

                return true;
            }

            if (obj is SkillChallenge)
            {
                var sc = obj as SkillChallenge;

                if (FilterNameToggle.Checked && _fNameTokens != null)
                {
                    var name = sc.Name.ToLower();

                    foreach (var token in _fNameTokens)
                        if (!name.Contains(token))
                            return false;
                }

                if (FilterCatToggle.Checked && _fCatTokens != null)
                {
                    // Ignore category for skill challenges
                }

                if (FilterRoleToggle.Checked)
                {
                    // Ignore roles for skill challenges
                }

                if (FilterOriginToggle.Checked)
                {
                    // Ignore origin for skill challenges
                }

                if (FilterTypeToggle.Checked)
                {
                    // Ignore type for skill challenges
                }

                if (FilterKeywordToggle.Checked && _fKeyTokens != null)
                {
                    // Ignore keywords for skill challenges
                }

                if (FilterSourceToggle.Checked)
                {
                    var lib = FilterSourceBox.SelectedItem as Library;
                    if (!lib.SkillChallenges.Contains(sc))
                        return false;
                }

                return true;
            }

            return false;
        }

        public void Collapse()
        {
            open_close_editor(false);
        }

        public void Expand()
        {
            open_close_editor(true);
        }

        public void FilterByPartyLevel()
        {
            FilterLevelAppropriateToggle.Checked = true;
            OnFilterChanged();

            if (!FilterPnl.Visible)
                InfoLbl.Text = get_filter_string();
        }

        public bool SetFilter(int level, IRole role)
        {
            _fSuppressEvents = true;

            var setFilter = false;

            if (level != 0)
            {
                FilterLevelToggle.Checked = true;
                LevelFromBox.Value = level;
                LevelToBox.Value = level;

                setFilter = true;
            }

            if (role != null)
            {
                if (role is ComplexRole)
                {
                    var cr = role as ComplexRole;

                    FilterRoleToggle.Checked = true;
                    FilterRoleBox.SelectedItem = cr.Type;

                    FilterModToggle.Checked = true;
                    FilterModBox.SelectedItem = cr.Flag.ToString();

                    setFilter = true;
                }

                if (role is Minion)
                {
                    var minion = role as Minion;

                    if (minion.HasRole)
                    {
                        FilterRoleToggle.Checked = true;
                        FilterRoleBox.SelectedItem = minion.Type;
                    }

                    FilterModToggle.Checked = true;
                    FilterModBox.SelectedItem = "Minion";

                    setFilter = true;
                }
            }

            _fSuppressEvents = false;

            if (setFilter)
            {
                update_option_state();

                OnFilterChanged();

                if (!FilterPnl.Visible)
                    InfoLbl.Text = get_filter_string();
            }

            return setFilter;
        }

        public event EventHandler FilterChanged;

        protected void OnFilterChanged()
        {
            if (_fSuppressEvents)
                return;

            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ToggleChanged(object sender, EventArgs e)
        {
            update_option_state();

            if (sender == FilterNameToggle && FilterNameBox.Text == "")
                return;

            if (sender == FilterCatToggle && FilterCatBox.Text == "")
                return;

            if (sender == FilterKeywordToggle && FilterKeywordBox.Text == "")
                return;

            if (sender == FilterLevelToggle)
                FilterLevelAppropriateToggle.Checked = false;

            if (sender == FilterLevelAppropriateToggle)
                FilterLevelToggle.Checked = false;

            OnFilterChanged();
        }

        private void TextOptionChanged(object sender, EventArgs e)
        {
            _fNameTokens = FilterNameBox.Text.ToLower().Split(null);
            if (_fNameTokens.Length == 0)
                _fNameTokens = null;

            _fCatTokens = FilterCatBox.Text.ToLower().Split(null);
            if (_fCatTokens.Length == 0)
                _fCatTokens = null;

            _fKeyTokens = FilterKeywordBox.Text.ToLower().Split(null);
            if (_fKeyTokens.Length == 0)
                _fKeyTokens = null;

            OnFilterChanged();
        }

        private void DropdownOptionChanged(object sender, EventArgs e)
        {
            OnFilterChanged();
        }

        private void NumericOptionChanged(object sender, EventArgs e)
        {
            if (sender == LevelFromBox)
                LevelToBox.Minimum = LevelFromBox.Value;
            if (sender == LevelToBox)
                LevelFromBox.Maximum = LevelToBox.Value;

            OnFilterChanged();
        }

        private void EditLbl_Click(object sender, EventArgs e)
        {
            // Open / close the editor
            open_close_editor(!FilterPnl.Visible);
        }

        private void update_allowed_filters()
        {
            FilterCatToggle.Enabled = _fMode == ListMode.Creatures || _fMode == ListMode.NpCs;
            FilterRoleToggle.Enabled = _fMode != ListMode.SkillChallenges;
            FilterModToggle.Enabled = _fMode == ListMode.Creatures || _fMode == ListMode.Traps;
            FilterOriginToggle.Enabled = _fMode == ListMode.Creatures || _fMode == ListMode.NpCs;
            FilterTypeToggle.Enabled = _fMode == ListMode.Creatures || _fMode == ListMode.NpCs;
            FilterKeywordToggle.Enabled = _fMode == ListMode.Creatures || _fMode == ListMode.NpCs;
            FilterLevelToggle.Enabled =
                _fMode == ListMode.Creatures || _fMode == ListMode.NpCs || _fMode == ListMode.Traps;
            FilterLevelAppropriateToggle.Enabled = PartyLevel != 0 &&
                                                   (_fMode == ListMode.Creatures || _fMode == ListMode.NpCs ||
                                                    _fMode == ListMode.Traps);
            FilterSourceToggle.Enabled = Session.Libraries.Count != 0 && (_fMode == ListMode.Creatures ||
                                                                          _fMode == ListMode.Templates ||
                                                                          _fMode == ListMode.Traps ||
                                                                          _fMode == ListMode.SkillChallenges);
        }

        private void update_option_state()
        {
            FilterNameBox.Enabled = FilterNameToggle.Checked;
            FilterCatBox.Enabled = FilterCatToggle.Enabled && FilterCatToggle.Checked;
            FilterRoleBox.Enabled = FilterRoleToggle.Enabled && FilterRoleToggle.Checked;
            FilterModBox.Enabled = FilterModToggle.Enabled && FilterModToggle.Checked;
            FilterOriginBox.Enabled = FilterOriginToggle.Enabled && FilterOriginToggle.Checked;
            FilterTypeBox.Enabled = FilterTypeToggle.Enabled && FilterTypeToggle.Checked;
            FilterKeywordBox.Enabled = FilterKeywordToggle.Enabled && FilterKeywordToggle.Checked;
            FilterSourceBox.Enabled = FilterSourceToggle.Enabled && FilterSourceToggle.Checked;

            FromLbl.Enabled = FilterLevelToggle.Enabled && FilterLevelToggle.Checked;
            ToLbl.Enabled = FilterLevelToggle.Enabled && FilterLevelToggle.Checked;
            LevelFromBox.Enabled = FilterLevelToggle.Enabled && FilterLevelToggle.Checked;
            LevelToBox.Enabled = FilterLevelToggle.Enabled && FilterLevelToggle.Checked;
        }

        private void open_close_editor(bool open)
        {
            if (open)
            {
                FilterPnl.Visible = true;

                InfoLbl.Text = "";
                EditLbl.Text = "Collapse";
            }
            else
            {
                FilterPnl.Visible = false;

                InfoLbl.Text = get_filter_string();
                EditLbl.Text = "Expand";
            }
        }

        private string get_filter_string()
        {
            var str = "";

            if (FilterNameToggle.Checked && FilterNameToggle.Enabled && FilterNameBox.Text != "")
            {
                if (str != "")
                    str += "; ";

                str += "Name: " + FilterNameBox.Text;
            }

            if (FilterCatToggle.Checked && FilterCatToggle.Enabled && FilterCatBox.Text != "")
            {
                if (str != "")
                    str += "; ";

                str += "Category: " + FilterCatBox.Text;
            }

            var role = "";
            if (FilterModToggle.Checked && FilterModToggle.Enabled) role += FilterModBox.SelectedItem;
            if (FilterRoleToggle.Checked && FilterRoleToggle.Enabled)
            {
                if (role != "")
                    role += " ";

                role += FilterRoleBox.SelectedItem;
            }

            if (role != "")
            {
                if (str != "")
                    str += "; ";

                str += "Role: " + role;
            }

            if (FilterTypeToggle.Checked && FilterTypeToggle.Enabled)
            {
                if (str != "")
                    str += "; ";

                str += "Type: " + FilterTypeBox.SelectedItem;
            }

            if (FilterOriginToggle.Checked && FilterOriginToggle.Enabled)
            {
                if (str != "")
                    str += "; ";

                str += "Origin: " + FilterOriginBox.SelectedItem;
            }

            if (FilterKeywordToggle.Checked && FilterKeywordToggle.Enabled && FilterKeywordBox.Text != "")
            {
                if (str != "")
                    str += "; ";

                str += "Keywords: " + FilterKeywordBox.Text;
            }

            if (FilterLevelToggle.Checked && FilterLevelToggle.Enabled)
            {
                if (str != "")
                    str += "; ";

                var lvlFrom = (int)LevelFromBox.Value;
                var lvlTo = (int)LevelToBox.Value;
                if (lvlFrom == lvlTo)
                    str += "Level: " + lvlFrom;
                else
                    str += "Level: " + lvlFrom + "-" + lvlTo;
            }

            if (FilterLevelAppropriateToggle.Checked && FilterLevelAppropriateToggle.Enabled)
            {
                if (str != "")
                    str += "; ";

                str += "Level-appropriate only";
            }

            if (FilterSourceToggle.Checked && FilterSourceToggle.Enabled)
            {
                if (str != "")
                    str += "; ";

                str += "Source: " + FilterSourceBox.SelectedItem;
            }

            return str;
        }
    }
}
