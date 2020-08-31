using System.Collections.Generic;

namespace AuditQualification.Models
{
    public class CompanyDetailsModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public Location Location  {get; set;}
        public Risks Risks { get; set; }
    }
    public class Location
    {
        public string Country { get; set; }
    }

    public class Risks
    {
        public string COVID_19_riskscore { get; set; }
        public bool COVID_19_Flagged { get; set; }
        public string DeforestationRisk { get; set; }
        public bool DeforestationRisk_Flagged { get; set; }
        public string CorruptionIndexScore { get; set; }
        public bool CorruptionIndexScore_Flagged { get; set; }
    }
}
