using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace S1thK3nny.SWAT.Models.Databases
{
    [XmlRoot("PerMapInfos")]
    public class PerMapInfos
    {
        [XmlElement("Map")]
        public List<MapInfo> Maps { get; set; } = new();
    }

    public class MapInfo
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        [XmlElement("Allegiance")]
        public List<AllegianceInfo> Allegiances { get; set; } // Optional, wait until created via /sposition

        [XmlElement("SwatVehicleInfos")]
        public SwatVehicleInfos SwatVehicleInfos { get; set; } // Optional, wait until created via /svehicle
    }
    
    public class SwatVehicleInfos
    {
        [XmlElement("VehicleID")]
        public ushort VehicleID { get; set; }
        [XmlElement("SpawnPosition")]
        public Vector3 SpawnPosition { get; set; } = new Vector3();

        [XmlElement("SpawnRotation")]
        public Vector3 SpawnRotation { get; set; } = new Vector3();
    }

    public class AllegianceInfo
    {
        [XmlAttribute("team")]
        public string Team { get; set; } = string.Empty;

        [XmlElement("PlayerInfo")]
        public List<PlayerInfo> Players { get; set; } = new();
    }

    public class PlayerInfo
    {
        [XmlElement("Steam64ID")]
        public ulong Steam64Id { get; set; }

        [XmlElement("Position")]
        public Vector3 Position { get; set; } = new Vector3();

        [XmlElement("Rotation")]
        public Vector3 Rotation { get; set; } = new Vector3();
    }
}