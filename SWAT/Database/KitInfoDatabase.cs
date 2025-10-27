using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using S1thK3nny.SWAT.Models.Databases;
using S1thK3nny.SWAT.Models.Teams;

namespace S1thK3nny.SWAT.Database
{
    public class KitInfoXmlDatabase
    {
        private SWATPlugin pluginInstance => SWATPlugin.Instance;

        private XmlSerializer xmlSerializer = new(typeof(KitInfo), new XmlRootAttribute(nameof(KitInfo)));
        private string filePath => $"{pluginInstance.Directory}/KitInfo.xml";

        public KitInfo Database { get; private set; }

        public void Load()
        {
            if (File.Exists(filePath))
            {
                using StreamReader reader = File.OpenText(filePath);
                Database = (KitInfo)xmlSerializer.Deserialize(reader);

                if (Database.Allegiances == null)
                {
                    Database.Allegiances = new System.Collections.Generic.List<AllegianceKitInfo>();
                }

                Console.WriteLine($"{ScriptTag.GetScriptName()} Loaded {Database.Allegiances.Count} allegiance kit records from XML database.");
            }
            else
            {
                Database = new KitInfo();
                Save();
            }
        }

        public void Save()
        {
            using StreamWriter writer = new(filePath);
            xmlSerializer.Serialize(writer, Database);
            Console.WriteLine($"{ScriptTag.GetScriptName()} Saved kit info to XML database.");
        }

        /// <summary>
        /// Sets or updates a kit for a player in a specific allegiance
        /// </summary>
        public void SetKit(ALLEGIANCE allegiance, ulong steam64ID, string kitName)
        {
            var allegianceInfo = Database.GetOrCreateAllegiance(allegiance);
            allegianceInfo.SetKit(steam64ID, kitName);
            Save();
        }

        /// <summary>
        /// Gets the kit name for a player in a specific allegiance
        /// </summary>
        public string GetKit(ALLEGIANCE allegiance, ulong steam64ID)
        {
            var allegianceInfo = Database.Allegiances.FirstOrDefault(a => a.Team == allegiance.ToString());
            return allegianceInfo?.GetKit(steam64ID);
        }

        /// <summary>
        /// Removes a kit for a player in a specific allegiance
        /// </summary>
        public bool RemoveKit(ALLEGIANCE allegiance, ulong steam64ID)
        {
            var allegianceInfo = Database.Allegiances.FirstOrDefault(a => a.Team == allegiance.ToString());
            if (allegianceInfo != null && allegianceInfo.RemoveKit(steam64ID))
            {
                Save();
                return true;
            }
            return false;
        }
    }
}
