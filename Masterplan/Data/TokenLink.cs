using System;
using System.Collections.Generic;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a link between map tokens.
    /// </summary>
    [Serializable]
    public class TokenLink
    {
        private string _fText = "";

        private List<IToken> _fTokens = new List<IToken>();

        /// <summary>
        ///     Gets or sets the text of the link.
        /// </summary>
        public string Text
        {
            get => _fText;
            set => _fText = value;
        }

        /// <summary>
        ///     Gets or sets the list of IToken objects.
        /// </summary>
        public List<IToken> Tokens
        {
            get => _fTokens;
            set => _fTokens = value;
        }

        /// <summary>
        ///     Creates a copy of the link.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public TokenLink Copy()
        {
            var link = new TokenLink();

            link.Text = _fText;

            foreach (var token in _fTokens)
                link.Tokens.Add(token);

            return link;
        }
    }
}
