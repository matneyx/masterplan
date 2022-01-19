using System;
using Masterplan.Controls;
using Masterplan.UI;

/// <summary>
///     Class used to store user combat settings.
/// </summary>
[Serializable]
public class CombatPreferences
{
    /// <summary>
    ///     Gets or sets the combat initiative mode for creatures.
    /// </summary>
    public InitiativeMode InitiativeMode { get; set; } = InitiativeMode.AutoGroup;

    /// <summary>
    ///     Gets or sets the combat initiative mode for PCs.
    /// </summary>
    public InitiativeMode HeroInitiativeMode { get; set; } = InitiativeMode.ManualIndividual;

    /// <summary>
    ///     Gets or sets the combat initiative mode for traps.
    /// </summary>
    public InitiativeMode TrapInitiativeMode { get; set; } = InitiativeMode.AutoIndividual;

    /// <summary>
    ///     Gets or sets whether creatures are removed from combat when reduced to 0 HP.
    /// </summary>
    public bool CreatureAutoRemove { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the combat view starts with two columns when there is a combat map.
    /// </summary>
    public bool CombatTwoColumns { get; set; }

    /// <summary>
    ///     Gets or sets whether the combat view starts with two columns when there is no map.
    /// </summary>
    public bool CombatTwoColumnsNoMap { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the combat map should be shown on the right (true) or at the bottom (false).
    /// </summary>
    public bool CombatMapRight { get; set; } = true;

    /// <summary>
    ///     Gets or sets the combat map fog of war setting.
    /// </summary>
    public CreatureViewMode CombatFog { get; set; } = CreatureViewMode.All;

    /// <summary>
    ///     Gets or sets the player view tactical map fog of war setting.
    /// </summary>
    public CreatureViewMode PlayerViewFog { get; set; } = CreatureViewMode.Visible;

    /// <summary>
    ///     Gets or sets whether the health bars are shown on the combat map.
    /// </summary>
    public bool CombatHealthBars { get; set; }

    /// <summary>
    ///     Gets or sets whether condition badges are shown on the player view tactical map.
    /// </summary>
    public bool PlayerViewConditionBadges { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether condition badges are shown on the combat map.
    /// </summary>
    public bool CombatConditionBadges { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the health bars are shown on the player view tactical map.
    /// </summary>
    public bool PlayerViewHealthBars { get; set; }

    /// <summary>
    ///     Gets or sets whether the player view tactical map shows full creature labels.
    /// </summary>
    public bool PlayerViewCreatureLabels { get; set; }

    /// <summary>
    ///     Gets or sets whether creature images are shown as tokens on the combat map.
    /// </summary>
    public bool CombatPictureTokens { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether creature images are shown as tokens on the player map.
    /// </summary>
    public bool PlayerViewPictureTokens { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the map is shown on the player view during combat.
    /// </summary>
    public bool PlayerViewMap { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the initiative list is shown on the player view during combat.
    /// </summary>
    public bool PlayerViewInitiative { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the grid is shown on the combat map.
    /// </summary>
    public bool CombatGrid { get; set; }

    /// <summary>
    ///     Gets or sets whether the grid is shown on the player map.
    /// </summary>
    public bool PlayerViewGrid { get; set; }

    /// <summary>
    ///     Gets or sets whether grid labels are shown on the combat map.
    /// </summary>
    public bool CombatGridLabels { get; set; }

    /// <summary>
    ///     Gets or sets whether grid labels are shown on the player map.
    /// </summary>
    public bool PlayerViewGridLabels { get; set; }

    /// <summary>
    ///     Gets or sets whether the combat list shows initiative scores.
    /// </summary>
    public bool CombatColumnInitiative { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the combat list shows hit points.
    /// </summary>
    public bool CombatColumnHp { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the combat list shows defence scores.
    /// </summary>
    public bool CombatColumnDefences { get; set; }

    /// <summary>
    ///     Gets or sets whether the combat list shows ongoing effects.
    /// </summary>
    public bool CombatColumnEffects { get; set; }
}
