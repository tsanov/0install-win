using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Common.Helpers;
using Common.Properties;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Storage
{
    /// <summary>
    /// Allows an easy storage of objects in compressed and encrypted XML files
    /// </summary>
    public static class XmlStorage
    {
        #region Serializer generation
        private static readonly Dictionary<string, XmlSerializer> Serializers = new Dictionary<string, XmlSerializer>();

        /// <summary>
        /// Gets a <see cref="XmlSerializer"/> for classes of the type <paramref name="type"/>. Results are automatically cached internally
        /// </summary>
        /// <param name="type">The type to get the serializer for</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        /// <returns>The cached or newly created <see cref="XmlSerializer"/>.</returns>
        private static XmlSerializer GetSerializer(Type type, IEnumerable<MemberInfo> ignoreMembers)
        {
            // Create a string key containing the type name and optionally the ignore-type names
            string key = type.FullName;
            if (ignoreMembers != null)
            {
                foreach (MemberInfo ignoreMember in ignoreMembers)
                {
                    if (ignoreMember != null)
                        key += " \\ " + ignoreMember.ReflectedType.FullName + ignoreMember.Name;
                }
            }

            XmlSerializer serializer;
            // Try to find a suitable serializer in the cache
            if (!Serializers.TryGetValue(key, out serializer))
            {
                // Create a new serializer and add it to the cache
                serializer = CreateSerializer(type, ignoreMembers);
                Serializers.Add(key, serializer);
            }
            
            return serializer;
        }

        /// <summary>
        /// Creates a new <see cref="XmlSerializer"/> for the type <paramref name="type"/> and applies a set of default augmentations for .NET types
        /// </summary>
        /// <param name="type">The type to create the serializer for</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        /// <returns>The newly created <see cref="XmlSerializer"/>.</returns>
        /// <remarks>This method may be rather slow, so its results should be cached</remarks>
        private static XmlSerializer CreateSerializer(Type type, IEnumerable<MemberInfo> ignoreMembers)
        {
            var overrides = new XmlAttributeOverrides();
            var asAttribute = new XmlAttributes {XmlAttribute = new XmlAttributeAttribute()};
            var ignore = new XmlAttributes {XmlIgnore = true};

            // Augment .NET default types
            overrides.Add(typeof(Point), "X", asAttribute);
            overrides.Add(typeof(Point), "Y", asAttribute);
            overrides.Add(typeof(Size), "Width", asAttribute);
            overrides.Add(typeof(Size), "Height", asAttribute);
            overrides.Add(typeof(Rectangle), "X", asAttribute);
            overrides.Add(typeof(Rectangle), "Y", asAttribute);
            overrides.Add(typeof(Rectangle), "Width", asAttribute);
            overrides.Add(typeof(Rectangle), "Height", asAttribute);
            overrides.Add(typeof(Rectangle), "Location", ignore);
            overrides.Add(typeof(Rectangle), "Size", ignore);
            overrides.Add(typeof(Exception), "Data", ignore);

            // Ignore specific fields
            if (ignoreMembers != null)
            {
                foreach (MemberInfo ignoreMember in ignoreMembers)
                {
                    if (ignoreMember != null) overrides.Add(ignoreMember.ReflectedType, ignoreMember.Name, ignore);
                }
            }

            return new XmlSerializer(type, overrides);
        }
        #endregion

        //--------------------//

        #region Load plain
        /// <summary>
        /// Loads a object from an XML file
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into</typeparam>
        /// <param name="stream">The XML file to be loaded</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data</exception>
        /// <returns>The loaded object</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T Load<T>(Stream stream, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            return (T)GetSerializer(typeof(T), ignoreMembers).Deserialize(stream);
        }

        /// <summary>
        /// Loads a object from an XML file
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into</typeparam>
        /// <param name="path">The XML file to be loaded</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data</exception>
        /// <returns>The loaded object</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T Load<T>(string path, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var fileStream = File.OpenRead(path))
                return Load<T>(fileStream, ignoreMembers);
        }

        /// <summary>
        /// Loads a object from an XML string
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into</typeparam>
        /// <param name="data">The XML string to be parsed</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data</exception>
        /// <returns>The loaded object</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T FromString<T>(string data, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(data)) throw new ArgumentNullException("data");
            #endregion

            using (var stream = new MemoryStream())
            {
                stream.Position = 0;
                using (var writer = new StreamWriter(stream))
                    writer.Write(data);

                return Load<T>(stream, ignoreMembers);
            }
        }
        #endregion

        #region Save plain
        /// <summary>
        /// Saves an object in an XML stream
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an XML stream</typeparam>
        /// <param name="stream">The XML file to be written</param>
        /// <param name="data">The object to be stored</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        public static void Save<T>(Stream stream, T data, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            using (var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true, IndentChars = "\t" }))
            {
                if (xmlWriter == null) throw new IOException(Resources.FailedToCreateXmlWriter);
                var serializer = GetSerializer(typeof(T), ignoreMembers);

                // Detect namespace defintions in XmlRoot attribute
                var rootAttributes = typeof(T).GetCustomAttributes(typeof(XmlRootAttribute), true);
                string defaultNamesapce = (rootAttributes.Length == 0 ? null : ((XmlRootAttribute)rootAttributes[0]).Namespace);
                if (string.IsNullOrEmpty(defaultNamesapce))
                { // Use default serializer namespaces (XMLSchema)
                    serializer.Serialize(xmlWriter, data);
                }
                else
                { // Set custom namespace as default
                    var ns = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", defaultNamesapce) });
                    serializer.Serialize(xmlWriter, data, ns);
                }
            }
        }

        /// <summary>
        /// Saves an object in an XML file
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an XML stream</typeparam>
        /// <param name="path">The XML file to be written</param>
        /// <param name="data">The object to be stored</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        public static void Save<T>(string path, T data, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var fileStream = File.Create(path))
                Save(fileStream, data, ignoreMembers);
        }

        /// <summary>
        /// Returns an object as an XML string
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an XML stream</typeparam>
        /// <param name="data">The object to be stored</param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        /// <returns>A string containing the XML code</returns>
        public static string ToString<T>(T data, params MemberInfo[] ignoreMembers)
        {
            using (var stream = new MemoryStream())
            {
                Save(stream, data, ignoreMembers);

                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
        #endregion

        //--------------------//

        #region Load ZIP
        /// <summary>
        /// Loads a object from an XML file embedded in a ZIP archive
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into</typeparam>
        /// <param name="stream">The ZIP archive to be loaded</param>
        /// <param name="password">The password to use for decryption; <see langword="null"/> for no encryption</param>
        /// <param name="additionalFiles">Additional files stored alongside the XML file in the ZIP archive to be read; may be <see langword="null"/></param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        /// <exception cref="ZipException">Thrown if a problem occurs while reading the ZIP data</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data</exception>
        /// <returns>The loaded object</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T FromZip<T>(Stream stream, string password, EmbeddedFile[] additionalFiles, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            bool xmlFound = false;
            T output = default(T);

            using (var zipFile = new ZipFile(stream))
            {
                zipFile.Password = password;

                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (zipEntry.Name == "Data.xml")
                    {
                        // Read the XML file from the ZIP archive
                        using (var inputStream = zipFile.GetInputStream(zipEntry))
                            output = Load<T>(inputStream, ignoreMembers);
                        xmlFound = true;
                    }
                    else
                    {
                        if (additionalFiles != null)
                        {
                            // Read additional files from the ZIP archive
                            foreach (EmbeddedFile file in additionalFiles)
                            {
                                if (StringHelper.Compare(zipEntry.Name, file.Filename))
                                {
                                    using (var inputStream = zipFile.GetInputStream(zipEntry))
                                        file.StreamDelegate(inputStream);
                                }
                            }
                        }
                    }
                }
            }

            if (xmlFound) return output;
            throw new InvalidOperationException(Resources.NoXmlDataInFile);
        }

        /// <summary>
        /// Loads a object from an XML file embedded in a ZIP archive
        /// </summary>
        /// <typeparam name="T">The type of object the XML stream shall be converted into</typeparam>
        /// <param name="path">The ZIP archive to be loaded</param>
        /// <param name="password">The password to use for decryption; <see langword="null"/> for no encryption</param>
        /// <param name="additionalFiles">Additional files stored alongside the XML file in the ZIP archive to be read; may be <see langword="null"/></param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        /// <exception cref="ZipException">Thrown if a problem occurs while reading the ZIP data</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data</exception>
        /// <returns>The loaded object</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to determine the type of returned object")]
        public static T FromZip<T>(string path, string password, EmbeddedFile[] additionalFiles, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var fileStream = File.OpenRead(path))
                return FromZip<T>(fileStream, password, additionalFiles, ignoreMembers);
        }
        #endregion

        #region Save ZIP
        /// <summary>
        /// Saves an object in an XML file embedded in a ZIP archive
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an XML stream</typeparam>
        /// <param name="stream">The ZIP archive to be written</param>
        /// <param name="data">The object to be stored</param>
        /// <param name="password">The password to use for encryption; <see langword="null"/> for no encryption</param>
        /// <param name="additionalFiles">Additional files to be stored alongside the XML file in the ZIP archive; may be <see langword="null"/></param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        public static void ToZip<T>(Stream stream, T data, string password, EmbeddedFile[] additionalFiles, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            using (var zipStream = new ZipOutputStream(stream))
            {
                zipStream.Password = password;

                // Write the XML file to the ZIP archive
                {
                    var entry = new ZipEntry("Data.xml") { DateTime = DateTime.Now };
                    zipStream.SetLevel(9);
                    zipStream.PutNextEntry(entry);
                    Save(zipStream, data, ignoreMembers);
                    zipStream.CloseEntry();
                }

                // Write additional files to the ZIP archive
                if (additionalFiles != null)
                {
                    foreach (EmbeddedFile file in additionalFiles)
                    {
                        var entry = new ZipEntry(file.Filename) { DateTime = DateTime.Now };
                        zipStream.SetLevel(file.CompressionLevel);
                        zipStream.PutNextEntry(entry);
                        file.StreamDelegate(zipStream);
                        zipStream.CloseEntry();
                    }
                }
            }
        }

        /// <summary>
        /// Saves an object in an XML file embedded in a ZIP archive
        /// </summary>
        /// <typeparam name="T">The type of object to be saved in an XML stream</typeparam>
        /// <param name="path">The ZIP archive to be written</param>
        /// <param name="data">The object to be stored</param>
        /// <param name="password">The password to use for encryption; <see langword="null"/> for no encryption</param>
        /// <param name="additionalFiles">Additional files to be stored alongside the XML file in the ZIP archive; may be <see langword="null"/></param>
        /// <param name="ignoreMembers">Fields to be ignored when serializing</param>
        public static void ToZip<T>(string path, T data, string password, EmbeddedFile[] additionalFiles, params MemberInfo[] ignoreMembers)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var fileStream = File.Create(path))
                ToZip(fileStream, data, password, additionalFiles, ignoreMembers);
        }
        #endregion

        //--------------------//

        #region Embedded files
        /// <summary>
        /// Returns a stream containing a file embedded into a XML-ZIP archive
        /// </summary>
        /// <param name="stream">The ZIP archive to be loaded</param>
        /// <param name="password">The password to use for decryption; <see langword="null"/> for no encryption</param>
        /// <param name="name">The name of the embedded file</param>
        /// <exception cref="ZipException">Thrown if a problem occurs while reading the ZIP data</exception>
        /// <returns>A stream containing the embedded file</returns>
        public static Stream GetEmbeddedFileStream(Stream stream, string password, string name)
        {
            using (var zipFile = new ZipFile(stream))
            {
                zipFile.Password = password;
                return zipFile.GetInputStream(zipFile.GetEntry(name));
            }
        }
        #endregion
    }
}