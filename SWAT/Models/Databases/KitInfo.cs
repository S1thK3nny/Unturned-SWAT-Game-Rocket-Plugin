using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using S1thK3nny.SWAT.Models.Teams;

namespace S1thK3nny.SWAT.Models.Databases
{
    [XmlRoot("KitInfo")]
    public class KitInfo
    {
        [XmlElement("Allegiance")]
        public List<AllegianceKitInfo> Allegiances { get; set; }

        public KitInfo()
        {
            Allegiances = new List<AllegianceKitInfo>();
        }

        /// <summary>
        /// Gets or creates the allegiance kit info for a specific team
        /// </summary>
        public AllegianceKitInfo GetOrCreateAllegiance(ALLEGIANCE team)
        {
            var allegiance = Allegiances.FirstOrDefault(a => a.Team == team.ToString());
            if (allegiance == null)
            {
                allegiance = new AllegianceKitInfo { Team = team.ToString() };
                Allegiances.Add(allegiance);
            }
            return allegiance;
        }
    }

    public class AllegianceKitInfo
    {
        [XmlAttribute("team")]
        public string Team { get; set; } = string.Empty;

        [XmlElement("Kit")]
        public List<KitData> Kits { get; set; } = new List<KitData>();

        /// <summary>
        /// Sets or updates a kit for a specific player (one kit per player per allegiance)
        /// </summary>
        public void SetKit(ulong steam64ID, string kitName)
        {
            var existingKit = Kits.FirstOrDefault(k => k.OwnerSteam64ID == steam64ID);
            if (existingKit != null)
            {
                existingKit.KitName = kitName;
            }
            else
            {
                Kits.Add(new KitData { KitName = kitName, OwnerSteam64ID = steam64ID });
            }
        }

        /// <summary>
        /// Gets the kit name for a specific player
        /// </summary>
        public string GetKit(ulong steam64ID)
        {
            return Kits.FirstOrDefault(k => k.OwnerSteam64ID == steam64ID)?.KitName;
        }

        /// <summary>
        /// Removes a kit for a specific player
        /// </summary>
        public bool RemoveKit(ulong steam64ID)
        {
            var kit = Kits.FirstOrDefault(k => k.OwnerSteam64ID == steam64ID);
            if (kit != null)
            {
                Kits.Remove(kit);
                return true;
            }
            return false;
        }
    }

    public class KitData
    {
        [XmlElement("KitName")]
        public string KitName { get; set; }

        [XmlElement("OwnerSteam64ID")]
        public ulong OwnerSteam64ID { get; set; }
    }
}