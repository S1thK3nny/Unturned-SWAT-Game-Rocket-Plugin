using System.Collections.Generic;
using S1thK3nny.SWAT.Models.Teams;

namespace S1thK3nny.SWAT.Models.Databases
{
    public class Allegiance
    {
        public List<AllegianceData> Allegiances { get; set; }

        public Allegiance()
        {
            Allegiances = new List<AllegianceData>();
        }
    }
    
    public class AllegianceData
    {
        public ulong Steam64ID { get; set; }
        public ALLEGIANCE Team { get; set; }
    }
}