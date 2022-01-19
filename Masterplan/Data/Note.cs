using System;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a user note.
    /// </summary>
    [Serializable]
    public class Note : IComparable<Note>
    {
        private string _fCategory = "";

        private string _fContent = "";

        private Guid _fId = Guid.NewGuid();

        /// <summary>
        ///     Gets or sets the unique ID.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the content of the note.
        /// </summary>
        public string Content
        {
            get => _fContent;
            set => _fContent = value;
        }

        /// <summary>
        ///     Gets or sets the category of the note.
        /// </summary>
        public string Category
        {
            get => _fCategory;
            set => _fCategory = value;
        }

        /// <summary>
        ///     Gets the name of the note.
        /// </summary>
        public string Name
        {
            get
            {
                var breaks = new[] { Environment.NewLine };
                var lines = _fContent.Split(breaks, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length == 0)
                    return "(blank note)";

                return lines[0];
            }
        }

        /// <summary>
        ///     Returns the text of the note.
        /// </summary>
        /// <returns>Returns the text of the note.</returns>
        public override string ToString()
        {
            return _fContent;
        }

        /// <summary>
        ///     Creates a copy of the note.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Note Copy()
        {
            var n = new Note();

            n.Id = _fId;
            n.Content = _fContent;
            n.Category = _fCategory;

            return n;
        }

        /// <summary>
        ///     Compares this note to another.
        /// </summary>
        /// <param name="rhs">The other note.</param>
        /// <returns>
        ///     Returns -1 if this note should be sorted before the other, +1 if the other should be sorted before this; 0
        ///     otherwise.
        /// </returns>
        public int CompareTo(Note rhs)
        {
            return Name.CompareTo(rhs.Name);
        }
    }
}
