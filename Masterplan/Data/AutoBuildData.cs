using System;
using System.Collections.Generic;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class holding data for the Encounter AutoBuild feature.
    /// </summary>
    [Serializable]
    public class AutoBuildData
    {
        private List<string> _fCategories;

        private Difficulty _fDifficulty = Difficulty.Random;

        private List<string> _fKeywords = new List<string>();

        private int _fLevel = Session.Project.Party.Level;

        private int _fSize = Session.Project.Party.Size;

        private string _fType = "";

        /// <summary>
        ///     Gets or sets the desired difficulty.
        /// </summary>
        public Difficulty Difficulty
        {
            get => _fDifficulty;
            set => _fDifficulty = value;
        }

        /// <summary>
        ///     Gets or sets the desired level.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets or sets the party size.
        /// </summary>
        public int Size
        {
            get => _fSize;
            set => _fSize = value;
        }

        /// <summary>
        ///     Gets or sets the name of the encounter template group to use.
        /// </summary>
        public string Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the list of creature categories to use.
        ///     If blank, AutoBuild will use all creatures.
        /// </summary>
        public List<string> Categories
        {
            get => _fCategories;
            set => _fCategories = value;
        }

        /// <summary>
        ///     Gets or sets the keywords to select creatures with.
        ///     If blank, AutoBuild will use all creatures.
        /// </summary>
        public List<string> Keywords
        {
            get => _fKeywords;
            set => _fKeywords = value;
        }

        /// <summary>
        ///     Creates a copy of the data.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public AutoBuildData Copy()
        {
            var data = new AutoBuildData();

            data.Difficulty = _fDifficulty;
            data.Level = _fLevel;
            data.Size = _fSize;
            data.Type = _fType;

            if (_fKeywords != null)
            {
                data.Keywords = new List<string>();
                data.Keywords.AddRange(_fKeywords);
            }

            if (_fCategories != null)
            {
                data.Categories = new List<string>();
                data.Categories.AddRange(_fCategories);
            }

            return data;
        }
    }
}
