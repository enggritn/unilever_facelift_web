using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class TransporterDriverDTO
    {
        public string DriverId { get; set; }
        public string TransporterId { get; set; }
        public string DriverName { get; set; }
        public string Phone { get; set; }
        public string LicenseNumber { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
    }
}