using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class CompanyDTO
    {
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAbb { get; set; }
        public string IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
    }
}