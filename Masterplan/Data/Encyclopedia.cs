using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing the project encyclopedia.
    /// </summary>
    [Serializable]
    public class Encyclopedia
    {
        private List<EncyclopediaEntry> _fEntries = new List<EncyclopediaEntry>();

        private List<EncyclopediaGroup> _fGroups = new List<EncyclopediaGroup>();

        private List<EncyclopediaLink> _fLinks = new List<EncyclopediaLink>();

        /// <summary>
        ///     Gets or sets the list of encyclopedia entries.
        /// </summary>
        public List<EncyclopediaEntry> Entries
        {
            get => _fEntries;
            set => _fEntries = value;
        }

        /// <summary>
        ///     Gets or sets the list of links between encyclopedia entries.
        /// </summary>
        public List<EncyclopediaLink> Links
        {
            get => _fLinks;
            set => _fLinks = value;
        }

        /// <summary>
        ///     Gets or sets the list of encyclopedia groups.
        /// </summary>
        public List<EncyclopediaGroup> Groups
        {
            get => _fGroups;
            set => _fGroups = value;
        }

        /// <summary>
        ///     Finds an encyclopedia entry by ID.
        /// </summary>
        /// <param name="entry_id">The ID of the required entry.</param>
        /// <returns>Returns the entry with the given id, or null if no such entry exists.</returns>
        public EncyclopediaEntry FindEntry(Guid entryId)
        {
            foreach (var entry in _fEntries)
                if (entry.Id == entryId)
                    return entry;

            return null;
        }

        /// <summary>
        ///     Finds an encyclopedia entry by name.
        /// </summary>
        /// <param name="name">The name of the required entry.</param>
        /// <returns>Returns the entry with the given name, or null if no such entry exists.</returns>
        public EncyclopediaEntry FindEntry(string name)
        {
            foreach (var entry in _fEntries)
                if (entry.Name.ToLower() == name.ToLower())
                    return entry;

            return null;
        }

        /// <summary>
        ///     Finds an encyclopedia group by ID.
        /// </summary>
        /// <param name="entry_id">The ID of the required group.</param>
        /// <returns>Returns the group with the given id, or null if no such group exists.</returns>
        public EncyclopediaGroup FindGroup(Guid entryId)
        {
            foreach (var group in _fGroups)
                if (@group.Id == entryId)
                    return @group;

            return null;
        }

        /// <summary>
        ///     Finds a link by the IDs of the entries linked.
        /// </summary>
        /// <param name="entry_id_1">The first entry ID.</param>
        /// <param name="entry_id_2">The second entry ID.</param>
        /// <returns>Returns the link with the given ids, or null if no such link exists.</returns>
        public EncyclopediaLink FindLink(Guid entryId1, Guid entryId2)
        {
            foreach (var link in _fLinks)
                if (link.EntryIDs.Contains(entryId1) && link.EntryIDs.Contains(entryId2))
                    return link;

            return null;
        }

        /// <summary>
        ///     Finds the entry associated with the given attachment.
        /// </summary>
        /// <param name="attachment_id">The ID of the attachment.</param>
        /// <returns>Returns the entry, or null if no such entry exists.</returns>
        public EncyclopediaEntry FindEntryForAttachment(Guid attachmentId)
        {
            foreach (var ee in Session.Project.Encyclopedia.Entries)
                if (ee.AttachmentId == attachmentId)
                    return ee;

            return null;
        }

        /// <summary>
        ///     Creates a copy of the encyclopedia.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Encyclopedia Copy()
        {
            var e = new Encyclopedia();

            foreach (var entry in _fEntries)
                e.Entries.Add(entry.Copy());

            foreach (var link in _fLinks)
                e.Links.Add(link.Copy());

            foreach (var group in _fGroups)
                e.Groups.Add(group.Copy());

            return e;
        }

        /// <summary>
        ///     Imports the data from another encyclopedia into this one.
        /// </summary>
        /// <param name="enc">The encyclopedia to import from.</param>
        public void Import(Encyclopedia enc)
        {
            if (enc == null)
                return;

            foreach (var entry in enc.Entries)
            {
                // Remove any previous entry
                var ee = FindEntry(entry.Id);
                if (ee != null)
                    Entries.Remove(ee);

                Entries.Add(entry);
            }

            foreach (var group in enc.Groups)
            {
                // Remove any previous group
                var eg = FindGroup(group.Id);
                if (eg != null)
                    Groups.Remove(eg);

                Groups.Add(group);
            }

            foreach (var link in enc.Links)
            {
                // Remove any previous link
                var el = FindLink(link.EntryIDs[0], link.EntryIDs[1]);
                if (el != null)
                    Links.Remove(el);

                Links.Add(link);
            }
        }
    }

    /// <summary>
    ///     Interface implemented by EncyclopediaEntry and EncyclopediaGroup.
    /// </summary>
    public interface IEncyclopediaItem
    {
        /// <summary>
        ///     Gets or sets the unique ID of the item.
        /// </summary>
        Guid Id { get; set; }
    }

    /// <summary>
    ///     Class representing an encyclopedia entry.
    /// </summary>
    [Serializable]
    public class EncyclopediaEntry : IEncyclopediaItem, IComparable<EncyclopediaEntry>
    {
        private Guid _fAttachmentId = Guid.Empty;

        private string _fCategory = "";

        private string _fDetails = "";

        private string _fDm = "";

        private Guid _fId = Guid.NewGuid();

        private List<EncyclopediaImage> _fImages = new List<EncyclopediaImage>();

        private string _fName = "";

        /// <summary>
        ///     Gets or sets the entry name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the entry category.
        /// </summary>
        public string Category
        {
            get => _fCategory;
            set => _fCategory = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the PC, NPC, custom creature or regional location this entry is associated with.
        /// </summary>
        public Guid AttachmentId
        {
            get => _fAttachmentId;
            set => _fAttachmentId = value;
        }

        /// <summary>
        ///     Gets or sets the entry details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the DM-only information about this entry
        /// </summary>
        public string DmInfo
        {
            get => _fDm;
            set => _fDm = value;
        }

        /// <summary>
        ///     Gets or sets entry images.
        /// </summary>
        public List<EncyclopediaImage> Images
        {
            get => _fImages;
            set => _fImages = value;
        }

        /// <summary>
        ///     Finds the image with the given ID.
        /// </summary>
        /// <param name="id">The ID to search for.</param>
        /// <returns>Returns the image, if it exists; null otherwise.</returns>
        public EncyclopediaImage FindImage(Guid id)
        {
            foreach (var img in _fImages)
                if (img.Id == id)
                    return img;

            return null;
        }

        /// <summary>
        ///     Creates a copy of the entry.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EncyclopediaEntry Copy()
        {
            var entry = new EncyclopediaEntry();

            entry.Name = _fName;
            entry.Id = _fId;
            entry.Category = _fCategory;
            entry.AttachmentId = _fAttachmentId;
            entry.Details = _fDetails;
            entry.DmInfo = _fDm;

            foreach (var ei in _fImages)
                entry.Images.Add(ei.Copy());

            return entry;
        }

        /// <summary>
        ///     Returns the entry name.
        /// </summary>
        /// <returns>Returns the entry name.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Compares this entry to another.
        /// </summary>
        /// <param name="rhs">The other entry.</param>
        /// <returns>
        ///     Returns -1 if this entry should be sorted before the other; +1 if the other should be sorted before this; 0
        ///     otherwise.
        /// </returns>
        public int CompareTo(EncyclopediaEntry rhs)
        {
            return _fName.CompareTo(rhs.Name);
        }

        /// <summary>
        ///     Gets or sets the unique ID of the entry.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }
    }

    /// <summary>
    ///     Class representing a link between two encyclopedia entries.
    /// </summary>
    [Serializable]
    public class EncyclopediaLink
    {
        private List<Guid> _fIDs = new List<Guid>();

        /// <summary>
        ///     Gets or sets the list of entry IDs.
        /// </summary>
        public List<Guid> EntryIDs
        {
            get => _fIDs;
            set => _fIDs = value;
        }

        /// <summary>
        ///     Creates a copy of the link.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EncyclopediaLink Copy()
        {
            var link = new EncyclopediaLink();

            foreach (var id in _fIDs)
                link.EntryIDs.Add(id);

            return link;
        }
    }

    /// <summary>
    ///     Class representing an image assigned to an encyclopedia entry.
    /// </summary>
    [Serializable]
    public class EncyclopediaImage
    {
        private Guid _fId = Guid.NewGuid();

        private Image _fImage;

        private string _fName = "";

        /// <summary>
        ///     Gets or sets the image name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the image.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the image.
        /// </summary>
        [XmlIgnore]
        public Image Image
        {
            get => _fImage;
            set => _fImage = value;
        }

        /// <summary>
        ///     Gets or sets the image data as a byte array.
        ///     This is used only for XML serialisation.
        /// </summary>
        public byte[] ImageData
        {
            get
            {
                if (_fImage != null)
                {
                    var bitmapConverter = TypeDescriptor.GetConverter(_fImage.GetType());
                    return (byte[])bitmapConverter.ConvertTo(_fImage, typeof(byte[]));
                }

                return null;
            }
            set
            {
                if (value != null)
                    _fImage = new Bitmap(new MemoryStream(value));
                else
                    _fImage = null;
            }
        }

        /// <summary>
        ///     Creates a copy of the image.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EncyclopediaImage Copy()
        {
            var ei = new EncyclopediaImage();

            ei.Id = _fId;
            ei.Name = _fName;
            ei.Image = _fImage;

            return ei;
        }
    }

    /// <summary>
    ///     Class representing a group of encyclopedia entries.
    /// </summary>
    [Serializable]
    public class EncyclopediaGroup : IEncyclopediaItem, IComparable<EncyclopediaGroup>
    {
        private Guid _fId = Guid.NewGuid();

        private List<Guid> _fIDs = new List<Guid>();

        private string _fName = "";

        /// <summary>
        ///     Gets or sets the entry name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the list of entry IDs.
        /// </summary>
        public List<Guid> EntryIDs
        {
            get => _fIDs;
            set => _fIDs = value;
        }

        /// <summary>
        ///     Creates a copy of the link.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EncyclopediaGroup Copy()
        {
            var group = new EncyclopediaGroup();

            group.Name = _fName;
            group.Id = _fId;

            foreach (var id in _fIDs)
                group.EntryIDs.Add(id);

            return group;
        }

        /// <summary>
        ///     Compares this group to another.
        /// </summary>
        /// <param name="rhs">The other group.</param>
        /// <returns>
        ///     Returns -1 if this group should be sorted before the other; +1 if the other should be sorted before this; 0
        ///     otherwise.
        /// </returns>
        public int CompareTo(EncyclopediaGroup rhs)
        {
            return _fName.CompareTo(rhs.Name);
        }

        /// <summary>
        ///     Gets or sets the unique ID of the entry.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }
    }
}
