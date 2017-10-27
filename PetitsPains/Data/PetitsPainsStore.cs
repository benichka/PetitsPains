using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PetitsPains.Model;

namespace PetitsPains.Data
{
    /// <summary>
    /// Helper class to store and retrieve the people list.
    /// </summary>
    internal static class PetitsPainsStore
    {
        private static string _RootPath;
        /// <summary>Root path to files.</summary>
        public static string RootPath
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_RootPath))
                {
                    _RootPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                }

                return _RootPath;
            }
            set
            {
                _RootPath = value;
            }
        }

        /// <summary>Config file path. This path can not be changed: it is located in the .exe folder.</summary>
        public static string ConfigFilePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), @"petitspains.config");
            }
        }

        /// <summary>Default people file name.</summary>
        public static string DefaultPeopleFileName
        {
            get
            {
                return @"ListePersonnesDefaut.json";
            }
        }

        /// <summary>Lines of croissants file name.</summary>
        public static string CroissantLinesFileName
        {
            get
            {
                return @"ListeLignes.json";
            }
        }

        /// <summary>
        /// Reads the config file and set the PersonsStore class properties accordingly.
        /// </summary>
        public static void ReadConfig()
        {
            // The file might not be there if it's the first time the application is launched
            // or if they are deleted.
            var configPath = Path.Combine(RootPath, ConfigFilePath);

            if (File.Exists(configPath))
            {
                using (var configStream = File.OpenRead(configPath))
                {
                    // xml document creation.
                    var xmlDoc = new XmlDocument();

                    // Parse the config file.
                    xmlDoc.Load(configStream);

                    XmlNode rootPathNode = xmlDoc.SelectSingleNode("config/rootPath");
                    RootPath = rootPathNode.InnerText;
                }
            }
        }

        /// <summary>
        /// Writes the appropriate PersonsStore properties to the config file.
        /// </summary>
        public static void WriteConfig()
        {
            // xml document creation.
            var xmlDoc = new XmlDocument();

            // root node creation.
            XmlNode rootNode = xmlDoc.CreateElement("config");
            xmlDoc.AppendChild(rootNode);

            XmlNode rootPathNode = xmlDoc.CreateElement("rootPath");
            rootPathNode.InnerText = RootPath.ToString();
            rootNode.AppendChild(rootPathNode);

            // XmlWritter settings -> how the xml will be created in file.
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                ConformanceLevel = ConformanceLevel.Document,
                OmitXmlDeclaration = false,
                CloseOutput = true,
                Indent = true,
                IndentChars = "  ",
                NewLineHandling = NewLineHandling.Replace
            };

            // Save the xml to the config file.
            using (FileStream configStream = File.Open(ConfigFilePath, FileMode.Create))
            using (XmlWriter writer = XmlWriter.Create(configStream, settings))
            {
                xmlDoc.WriteContentTo(writer);
            }
        }

        /// <summary>
        /// Write the list of lines of croissants to a file.
        /// </summary>
        /// <param name="croissantsLines">List of Lines of croissants.</param>
        public static void WriteCroissantsLines(List<Line> croissantsLines)
        {
            var jsonSer = new JsonSerializer();
            jsonSer.Converters.Add(new IsoDateTimeConverter() { Culture = CultureInfo.InvariantCulture });
            jsonSer.NullValueHandling = NullValueHandling.Include;
            jsonSer.Formatting = Newtonsoft.Json.Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(Path.Combine(RootPath, CroissantLinesFileName), false))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                jsonSer.Serialize(writer, croissantsLines);
            }
        }

        /// <summary>
        /// Read the list of lines of croissants from a file.
        /// </summary>
        /// <returns>List of lines of croissants.</returns>
        public static List<Line> ReadCroissantsLines()
        {
            var filePath = Path.Combine(RootPath, CroissantLinesFileName);

            var result = new List<Line>();

            // The file might not be there if it's the first time the application is launch
            // or if they are deleted.
            if (File.Exists(filePath))
            {
                var jsonSer = new JsonSerializer();
                jsonSer.Converters.Add(new IsoDateTimeConverter() { Culture = CultureInfo.InvariantCulture });
                jsonSer.NullValueHandling = NullValueHandling.Include;

                using (StreamReader sr = File.OpenText(filePath))
                {
                    result = jsonSer.Deserialize(sr, typeof(List<Line>)) as List<Line>;
                }
            }
            else
            {
                // Otherwise, we load the default list of people.
                result = GetDefaultPeople();
            }

            return result;
        }

        /// <summary>
        /// Read the list of people from a file.
        /// </summary>
        /// <returns>List of people.</returns>
        private static List<Person> ReadPersons()
        {
            var filePath = Path.Combine(RootPath, DefaultPeopleFileName);

            List<Person> result = new List<Person>();

            // The file might not be there.
            if (File.Exists(filePath))
            {
                var jsonSer = new JsonSerializer();
                jsonSer.NullValueHandling = NullValueHandling.Include;

                using (StreamReader sr = File.OpenText(filePath))
                {
                    result = jsonSer.Deserialize(sr, typeof(List<Person>)) as List<Person>;
                }
            }

            return result;
        }

        /// <summary>
        /// Get the default list of people and return a list of lines containing them.
        /// </summary>
        /// <returns>List of lines containing the default people.</returns>
        private static List<Line> GetDefaultPeople()
        {
            List<Person> persons = ReadPersons();

            var result = new List<Line>();

            foreach (var person in persons)
            {
                result.Add(new Line()
                {
                    Person = person
                });
            }

            return result;
        }
    }
}
