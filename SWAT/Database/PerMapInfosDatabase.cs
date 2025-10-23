using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using S1thK3nny.SWAT.Models.Databases;
using System;

namespace S1thK3nny.SWAT.Database
{
    public class PerMapInfosXmlDatabase
    {
        private SWATPlugin pluginInstance => SWATPlugin.Instance;

        private XmlSerializer xmlSerializer = new(typeof(PerMapInfos), new XmlRootAttribute(nameof(PerMapInfos)));
        private string filePath => $"{pluginInstance.Directory}/PerMapInfos.xml";

        public PerMapInfos Database { get; private set; }

        public List<MapInfo> Maps => Database?.Maps; // Convenience property

        public void Load()
        {
            if (File.Exists(filePath))
            {
                using StreamReader reader = File.OpenText(filePath);
                Database = (PerMapInfos)xmlSerializer.Deserialize(reader);

                if (Database.Maps == null)
                {
                    Database.Maps = new List<MapInfo>();
                }

                Console.WriteLine($"[SWATPlugin] Loaded {Database.Maps.Count} map records from XML database.");
            }
            else
            {
                Database = new PerMapInfos
                {
                    Maps = new List<MapInfo>()
                };
                Save();
            }
        }

        public void Save()
        {
            using StreamWriter writer = new(filePath);
            xmlSerializer.Serialize(writer, Database);
            Console.WriteLine($"[SWATPlugin] Saved {Database.Maps.Count} map records to XML database.");
        }
    }
}
