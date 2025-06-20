using System;

namespace Facelift_App.Models
{
    public class EquipmentDTO
    {
        public string EquipmentId { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Ip { get; set; }
        public string port { get; set; }
        public bool IsActive { get; set; }
        public bool Receive { get; set; }
        public bool Ship { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
    }
}