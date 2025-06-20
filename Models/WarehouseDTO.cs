using System;

namespace Facelift_App.Models
{
    public class WarehouseDTO
    {
        public string WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseAlias { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string PIC1 { get; set; }
        public string PIC2 { get; set; }
        public string CategoryName { get; set; }
        public string MaxCapacity { get; set; }
        public string IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
    }
}