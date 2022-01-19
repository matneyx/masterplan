using System;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing some project background information.
    /// </summary>
    [Serializable]
    public class Background
    {
        private string _fDetails = "";

        private Guid _fId = Guid.NewGuid();

        private string _title = "";

        /// <summary>
        ///     Gets or sets the unique ID of the background.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the title of the background information.
        /// </summary>
        public string Title
        {
            get => _title;
            set => _title = value;
        }

        /// <summary>
        ///     Gets or sets the background information details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public Background()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="title">The title of the background information</param>
        public Background(string title)
        {
            _title = title;
        }

        /// <summary>
        ///     Creates a copy of the Background.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Background Copy()
        {
            var b = new Background();

            b.Id = _fId;
            b.Title = _title;
            b.Details = _fDetails;

            return b;
        }

        /// <summary>
        ///     Returns the background item title.
        /// </summary>
        /// <returns>Returns the background item title.</returns>
        public override string ToString()
        {
            return _title;
        }
    }
}
