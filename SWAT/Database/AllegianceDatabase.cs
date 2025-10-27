using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using S1thK3nny.SWAT.Models.Databases;
using System;

namespace S1thK3nny.SWAT.Database
{
    public class AllegianceXmlDatabase
    {
        private SWATPlugin pluginInstance => SWATPlugin.Instance;

        private XmlSerializer xmlSerializer = new(typeof(Allegiance), new XmlRootAttribute(nameof(Allegiance)));
        private string filePath => $"{pluginInstance.Directory}/Allegiance.xml";

        public Allegiance Database { get; private set; }

        public List<AllegianceData> Allegiances => Database?.Allegiances; // Convenience property

        public void Load()
        {
            if (File.Exists(filePath))
            {
                using StreamReader reader = File.OpenText(filePath);
                Database = (Allegiance)xmlSerializer.Deserialize(reader);

                if (Database.Allegiances == null)
                {
                    Database.Allegiances = new List<AllegianceData>();
                }

                Console.WriteLine($"{ScriptTag.GetScriptName()} Loaded {Database.Allegiances.Count} allegiance records from XML database.");
            }
            else
            {
                Database = new Allegiance
                {
                    Allegiances = new List<AllegianceData>()
                };
                Save();
            }
        }

        public void Save()
        {
            using StreamWriter writer = new(filePath);
            xmlSerializer.Serialize(writer, Database);
            Console.WriteLine($"{ScriptTag.GetScriptName()} Saved {Database.Allegiances.Count} allegiance records to XML database.");
        }
    }
}
