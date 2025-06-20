using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class TransporterDTO
    {
        public string TransporterId { get; set; }
        public string TransporterName { get; set; }
        public string Address { get; set; }
        public string PIC { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
    }
}