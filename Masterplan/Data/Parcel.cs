using System;
using Masterplan.Tools.Generators;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a treasure parcel.
    /// </summary>
    [Serializable]
    public class Parcel
    {
        private Guid _fArtifactId = Guid.Empty;

        private string _fDetails = "";

        private Guid _fHeroId = Guid.Empty;

        private Guid _fMagicItemId = Guid.Empty;

        private string _fName = "";

        private int _fValue;

        /// <summary>
        ///     Gets or sets the name of the parcel.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the parcel details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the value of the parcel in GP.
        /// </summary>
        public int Value
        {
            get => _fValue;
            set => _fValue = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the magic item.
        /// </summary>
        public Guid MagicItemId
        {
            get => _fMagicItemId;
            set => _fMagicItemId = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the artifact.
        /// </summary>
        public Guid ArtifactId
        {
            get => _fArtifactId;
            set => _fArtifactId = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the PC to whom the item has been given.
        /// </summary>
        public Guid HeroId
        {
            get => _fHeroId;
            set => _fHeroId = value;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public Parcel()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="item">The magic item to create the parcel with.</param>
        public Parcel(MagicItem item)
        {
            SetAsMagicItem(item);
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="artifact">The artifact to create the parcel with.</param>
        public Parcel(Artifact artifact)
        {
            SetAsArtifact(artifact);
        }

        /// <summary>
        ///     Sets the parcel to contain the given magic item.
        /// </summary>
        /// <param name="item">The magic item.</param>
        public void SetAsMagicItem(MagicItem item)
        {
            _fName = item.Name;
            _fDetails = item.Description;
            _fMagicItemId = item.Id;
            _fArtifactId = Guid.Empty;
            _fValue = Treasure.GetItemValue(item.Level);
        }

        /// <summary>
        ///     Sets the parcel to contain the given magic item.
        /// </summary>
        /// <param name="artifact">The magic item.</param>
        public void SetAsArtifact(Artifact artifact)
        {
            _fName = artifact.Name;
            _fDetails = artifact.Description;
            _fMagicItemId = Guid.Empty;
            _fArtifactId = artifact.Id;
            _fValue = 0;
        }

        /// <summary>
        ///     Calculates the level of the contained magic item.
        /// </summary>
        /// <returns>Returns the level.</returns>
        public int FindItemLevel()
        {
            var item = Session.FindMagicItem(_fMagicItemId, SearchType.Global);
            if (item != null)
                return item.Level;

            var index = Treasure.PlaceholderIDs.IndexOf(_fMagicItemId);
            if (index != -1)
                return index + 1;

            if (_fValue > 0)
                for (var level = 30; level >= 1; --level)
                {
                    var value = Treasure.GetItemValue(level);
                    if (value < _fValue)
                        return level;
                }

            return -1;
        }

        /// <summary>
        ///     Calculates the tier of the contained artifact.
        /// </summary>
        /// <returns>Returns the tier.</returns>
        public Tier FindItemTier()
        {
            var artifact = Session.FindArtifact(_fArtifactId, SearchType.Global);
            if (artifact != null)
                return artifact.Tier;

            var index = Treasure.PlaceholderIDs.IndexOf(_fMagicItemId);
            switch (index)
            {
                case 0:
                    return Tier.Heroic;
                case 1:
                    return Tier.Paragon;
                case 2:
                    return Tier.Epic;
            }

            return Tier.Heroic;
        }

        /// <summary>
        ///     Creates a copy of the parcel.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Parcel Copy()
        {
            var p = new Parcel();

            p.Name = _fName;
            p.Details = _fDetails;
            p.Value = _fValue;
            p.MagicItemId = _fMagicItemId;
            p.ArtifactId = _fArtifactId;
            p.HeroId = _fHeroId;

            return p;
        }
    }
}
