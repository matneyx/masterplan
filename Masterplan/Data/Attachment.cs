using System;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Enumeration containing the known types of attachment.
    /// </summary>
    public enum AttachmentType
    {
        /// <summary>
        ///     Miscellaneous file.
        /// </summary>
        Miscellaneous,

        /// <summary>
        ///     Plan text file.
        /// </summary>
        PlainText,

        /// <summary>
        ///     Rich text file.
        /// </summary>
        RichText,

        /// <summary>
        ///     Image file.
        /// </summary>
        Image,

        /// <summary>
        ///     URL link.
        /// </summary>
        Url,

        /// <summary>
        ///     HTML file.
        /// </summary>
        Html
    }

    /// <summary>
    ///     Class representing a handout file.
    /// </summary>
    [Serializable]
    public class Attachment : IComparable<Attachment>
    {
        private byte[] _fContents;

        private Guid _fId = Guid.NewGuid();

        private string _fName;

        /// <summary>
        ///     Gets or sets the unique ID.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the handout.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the handout file contents.
        /// </summary>
        public byte[] Contents
        {
            get => _fContents;
            set => _fContents = value;
        }

        /// <summary>
        ///     Gets the type of file.
        /// </summary>
        public AttachmentType Type
        {
            get
            {
                var ext = FileName.Extension(_fName).ToLower();

                if (ext == "txt")
                    return AttachmentType.PlainText;

                if (ext == "rtf")
                    return AttachmentType.RichText;

                if (ext == "bmp")
                    return AttachmentType.Image;

                if (ext == "jpg")
                    return AttachmentType.Image;

                if (ext == "jpeg")
                    return AttachmentType.Image;

                if (ext == "gif")
                    return AttachmentType.Image;

                if (ext == "tga")
                    return AttachmentType.Image;

                if (ext == "png")
                    return AttachmentType.Image;

                if (ext == "url")
                    return AttachmentType.Url;

                if (ext == "htm")
                    return AttachmentType.Html;

                if (ext == "html")
                    return AttachmentType.Html;

                return AttachmentType.Miscellaneous;
            }
        }

        /// <summary>
        ///     Creates a copy of the handout.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Attachment Copy()
        {
            var h = new Attachment();

            h.Id = _fId;
            h.Name = _fName;

            h.Contents = new byte[_fContents.Length];
            for (var index = 0; index != _fContents.Length; ++index)
                h.Contents[index] = _fContents[index];

            return h;
        }

        /// <summary>
        ///     Compares this attachment to another.
        /// </summary>
        /// <param name="rhs">The other attachment.</param>
        /// <returns>
        ///     Returns -1 if this attachment should be sorted before the other, +1 if the other should be sorted before this;
        ///     0 otherwise.
        /// </returns>
        public int CompareTo(Attachment rhs)
        {
            var lhsName = FileName.Name(_fName);
            var rhsName = FileName.Name(rhs.Name);

            return lhsName.CompareTo(rhsName);
        }
    }
}
