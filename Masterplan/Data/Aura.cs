using System;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a creature aura.
    /// </summary>
    [Serializable]
    public class Aura
    {
        private string _fDescription = "";

        private string _fDetails = "";

        private bool _fExtractedData;

        private Guid _fId = Guid.NewGuid();

        private string _fKeywords = "";

        private string _fName = "";

        private int _radius = int.MinValue;

        /// <summary>
        ///     Gets or sets the unique ID of the aura.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the aura.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the aura keywords.
        /// </summary>
        public string Keywords
        {
            get => _fKeywords;
            set => _fKeywords = value;
        }

        /// <summary>
        ///     Gets or sets the details of the aura, including the radius size.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set
            {
                _fDetails = value;
                Extract();
            }
        }

        internal string Description
        {
            get
            {
                if (!_fExtractedData)
                    Extract();

                return _fDescription;
            }
        }

        internal int Radius
        {
            get
            {
                if (!_fExtractedData)
                    Extract();

                return _radius;
            }
        }

        /// <summary>
        ///     Creates a copy of the aura.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Aura Copy()
        {
            var a = new Aura();

            a.Id = _fId;
            a.Name = _fName;
            a.Keywords = _fKeywords;
            a.Details = _fDetails;

            return a;
        }

        private void Extract()
        {
            var valStr = "";

            var startedValue = false;
            for (var n = 0; n != _fDetails.Length; ++n)
            {
                var ch = _fDetails[n];
                startedValue = char.IsDigit(ch);

                if (!startedValue && valStr != "")
                {
                    _fDescription = _fDetails.Substring(n);
                    break;
                }

                if (startedValue)
                    valStr += ch;
            }

            var radius = 1;
            try
            {
                radius = int.Parse(valStr);
            }
            catch
            {
                radius = 1;
            }

            if (_fDescription == null)
            {
                _fDescription = "";
            }
            else
            {
                if (_fDescription.StartsWith(":"))
                    _fDescription = _fDescription.Substring(1);
                _fDescription = _fDescription.Trim();
            }

            _radius = radius;
            _fExtractedData = true;
        }
    }
}
