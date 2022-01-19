using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Masterplan.Tools
{
    /// <summary>
    ///     Class containing XML manipulation methods.
    /// </summary>
    public class XmlHelper
    {
        /// <summary>
        ///     Load an XML document.
        /// </summary>
        /// <param name="xml">The XML source.</param>
        /// <returns>Returns the XML document.</returns>
        public static XmlDocument LoadSource(string xml)
        {
            try
            {
                var reader = new StringReader(xml);
                var xmlReader = XmlReader.Create(reader);
                var doc = new XmlDocument();
                doc.Load(xmlReader);

                return doc;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Creates a child node for the given parent node.
        /// </summary>
        /// <param name="doc">The XML document.</param>
        /// <param name="parent">The parent node.</param>
        /// <param name="name">The name of the new node.</param>
        /// <returns>Returns the new child node.</returns>
        public static XmlNode CreateChild(XmlDocument doc, XmlNode parent, string name)
        {
            try
            {
                var element = doc.CreateElement(name);

                return parent.AppendChild(element);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Finds a named child node.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="name">The name of the child node.</param>
        /// <returns>Returns the node, or null if no such node was found.</returns>
        public static XmlNode FindChild(XmlNode parent, string name)
        {
            foreach (XmlNode child in parent.ChildNodes)
                if (child.Name == name)
                    return child;

            return null;
        }

        /// <summary>
        ///     Finds the child node which has a certain value for a given attribute.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="attribute_name">The name of the attribute.</param>
        /// <param name="attribute_value">The attribute value to search for.</param>
        /// <returns>Returns the first such node, if one exists; null otherwise.</returns>
        public static XmlNode FindChildWithAttribute(XmlNode parent, string attributeName, string attributeValue)
        {
            foreach (XmlNode child in parent.ChildNodes)
            {
                var attribute = GetAttribute(child, attributeName);
                if (attribute == attributeValue)
                    return child;
            }

            return null;
        }

        /// <summary>
        ///     Gets the list of child nodes which have a certain value for a given attribute.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="attribute_name">The name of the attribute.</param>
        /// <param name="attribute_value">The attribute value to search for.</param>
        /// <returns>Returns the list of matching child nodes.</returns>
        public static List<XmlNode> FindChildrenWithAttribute(XmlNode parent, string attributeName,
            string attributeValue)
        {
            var nodes = new List<XmlNode>();

            foreach (XmlNode child in parent.ChildNodes)
            {
                var attribute = GetAttribute(child, attributeName);
                if (attribute == attributeValue)
                    nodes.Add(child);
            }

            return nodes;
        }

        /// <summary>
        ///     Gets the text of a named child node.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="name">The name of the child node.</param>
        /// <returns>Returns the node text, or an empty string if no such node was found.</returns>
        public static string NodeText(XmlNode parent, string name)
        {
            var node = FindChild(parent, name);
            if (node != null)
                return node.InnerText;

            return "";
        }

        /// <summary>
        ///     Gets the string value of the named attribute.
        /// </summary>
        /// <param name="node">The parent node.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>Returns the attribute value.</returns>
        public static string GetAttribute(XmlNode node, string name)
        {
            foreach (XmlAttribute att in node.Attributes)
                if (att.Name == name)
                    return att.Value;

            return "";
        }

        /// <summary>
        ///     Gets the integer value of the named attribute.
        /// </summary>
        /// <param name="node">The parent node.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>Returns the attribute value.</returns>
        public static int GetIntAttribute(XmlNode node, string name)
        {
            var value = GetAttribute(node, name);
            return int.Parse(value);
        }

        /// <summary>
        ///     Gets the boolean value of the named attribute.
        /// </summary>
        /// <param name="node">The parent node.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>Returns the attribute value.</returns>
        public static bool GetBoolAttribute(XmlNode node, string name)
        {
            var value = GetAttribute(node, name);
            return bool.Parse(value);
        }
    }
}
