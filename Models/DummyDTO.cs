using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class DummyDTO
    {
        public string PalletTypeId { get; set; }
        public string PalletName { get; set; }
        public string PalletCondition { get; set; }
        public string WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string PalletOwner { get; set; }
        public string PalletProducer { get; set; }
        public string RegDate { get; set; }
        public string TotalPallet { get; set; }
        public string PalletAgeDay { get; set; }
        public string PalletAgeMonth { get; set; }
        public string TotalPrice { get; set; }
    }
}