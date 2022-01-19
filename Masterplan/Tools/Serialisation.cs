using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Masterplan.Tools
{
    /// <summary>
    ///     Enumeration defining the supported serialisation modes.
    /// </summary>
    public enum SerialisationMode
    {
        /// <summary>
        ///     Binary file format.
        /// </summary>
        Binary,

        /// <summary>
        ///     XML text format.
        /// </summary>
        Xml
    }

    /// <summary>
    ///     Class containing static methods for serialising (loading and saving) an object.
    /// </summary>
    /// <typeparam name="T">The type of object to be serialised.</typeparam>
    public class Serialisation<T>
    {
        /// <summary>
        ///     Loads an object of type T from a file.
        /// </summary>
        /// <param name="filename">The full path of the file.</param>
        /// <param name="mode">The mode in which the object was saved.</param>
        /// <returns>Returns the loaded object, or default(T) if the object could not be loaded.</returns>
        public static T Load(string filename, SerialisationMode mode)
        {
            var result = default(T);

            try
            {
                switch (mode)
                {
                    case SerialisationMode.Binary:
                    {
                        Stream stream = new FileStream(filename, FileMode.Open);

                        try
                        {
                            IFormatter formatter = new BinaryFormatter();
                            result = (T)formatter.Deserialize(stream);
                        }
                        catch
                        {
                            result = default;
                        }

                        stream.Close();
                    }
                        break;
                    case SerialisationMode.Xml:
                    {
                        var reader = new XmlTextReader(filename);

                        try
                        {
                            var s = new XmlSerializer(typeof(T));
                            result = (T)s.Deserialize(reader);
                        }
                        catch
                        {
                            result = default;
                        }

                        reader.Close();
                    }
                        break;
                }
            }
            catch (Exception)
            {
                result = default;
            }

            return result;
        }

        /// <summary>
        ///     Saves an object of type T to a file.
        /// </summary>
        /// <param name="filename">The full path of the file.</param>
        /// <param name="obj">The object to be saved.</param>
        /// <param name="mode">The mode in which the object was saved.</param>
        /// <returns>Returns true if the object was saved successfully; false otherwise.</returns>
        public static bool Save(string filename, T obj, SerialisationMode mode)
        {
            var ok = false;

            var tempFilename = filename + ".save";

            try
            {
                switch (mode)
                {
                    case SerialisationMode.Binary:
                    {
                        Stream stream = new FileStream(tempFilename, FileMode.Create);

                        try
                        {
                            IFormatter formatter = new BinaryFormatter();
                            formatter.Serialize(stream, obj);
                            stream.Flush();

                            ok = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            ok = false;
                        }

                        stream.Close();
                    }
                        break;
                    case SerialisationMode.Xml:
                    {
                        var writer = new XmlTextWriter(tempFilename, Encoding.UTF8);
                        writer.Formatting = Formatting.Indented;

                        try
                        {
                            var s = new XmlSerializer(typeof(T));
                            s.Serialize(writer, obj);
                            writer.Flush();

                            ok = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            ok = false;
                        }

                        writer.Close();
                    }
                        break;
                }
            }
            catch (Exception)
            {
                ok = false;
            }

            if (ok)
            {
                if (File.Exists(filename))
                    File.Delete(filename);

                File.Move(tempFilename, filename);
            }

            return ok;
        }
    }
}
