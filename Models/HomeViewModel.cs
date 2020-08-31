using System;
using System.Collections.Generic;

namespace AuditQualification.Models
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            Companies = new List<CompanyModel>();
        }
        public string Jsonstring { get; set; }
        public IList<CompanyModel> Companies { get; set; }
    }

    public class CompanyModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}
