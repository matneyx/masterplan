using System;
using System.Collections.Generic;

namespace Masterplan.Data
{
    /// <summary>
    ///     Rarities for magic items.
    /// </summary>
    public enum MagicItemRarity
    {
        /// <summary>
        ///     Common items.
        /// </summary>
        Common,

        /// <summary>
        ///     Uncommon items.
        /// </summary>
        Uncommon,

        /// <summary>
        ///     Rare items.
        /// </summary>
        Rare,

        /// <summary>
        ///     Unique items.
        /// </summary>
        Unique
    }

    /// <summary>
    ///     Class representing a magic item.
    /// </summary>
    [Serializable]
    public class MagicItem : IComparable<MagicItem>
    {
        private string _fDescription = "";

        private Guid _fId = Guid.NewGuid();

        private int _fLevel = 1;

        private string _fName = "";

        private MagicItemRarity _fRarity = MagicItemRarity.Uncommon;

        private List<MagicItemSection> _fSections = new List<MagicItemSection>();

        private string _fType = "Weapon";

        /// <summary>
        ///     Gets or sets the unique ID.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the item type.
        /// </summary>
        public string Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the item type.
        /// </summary>
        public MagicItemRarity Rarity
        {
            get => _fRarity;
            set => _fRarity = value;
        }

        /// <summary>
        ///     Gets or sets the item level.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        public string Description
        {
            get => _fDescription;
            set => _fDescription = value;
        }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        public List<MagicItemSection> Sections
        {
            get => _fSections;
            set => _fSections = value;
        }

        /// <summary>
        ///     Level N [type]
        /// </summary>
        public string Info => "Level " + _fLevel + " " + _fType.ToLower();

        /// <summary>
        ///     Creates a copy of the creature.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public MagicItem Copy()
        {
            var mi = new MagicItem();

            mi.Id = _fId;
            mi.Name = _fName;
            mi.Type = _fType;
            mi.Rarity = _fRarity;
            mi.Level = _fLevel;
            mi.Description = _fDescription;

            foreach (var section in _fSections)
                mi.Sections.Add(section.Copy());

            return mi;
        }

        /// <summary>
        ///     Compares this item to another.
        /// </summary>
        /// <param name="rhs">The other item.</param>
        /// <returns>
        ///     Returns -1 if this item should be sorted before the other, +1 if the other should be sorted before this; 0
        ///     otherwise.
        /// </returns>
        public int CompareTo(MagicItem rhs)
        {
            return _fName.CompareTo(rhs.Name);
        }
    }

    /// <summary>
    ///     Class to hold information about a magic item property or power.
    /// </summary>
    [Serializable]
    public class MagicItemSection
    {
        private string _fDetails = "";

        private string _fHeader = "";

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header
        {
            get => _fHeader;
            set => _fHeader = value;
        }

        /// <summary>
        ///     Gets or sets the details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Creates a copy of the section.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public MagicItemSection Copy()
        {
            var section = new MagicItemSection();

            section.Header = _fHeader;
            section.Details = _fDetails;

            return section;
        }

        /// <summary>
        ///     [header]: [details]
        /// </summary>
        public override string ToString()
        {
            if (_fDetails != "")
                return _fHeader + ": " + _fDetails;
            return _fHeader;
        }
    }
}
