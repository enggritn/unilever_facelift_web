using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class OfflineWarehouseDTO
    {
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
    }

    public class OfflineTransporterDTO
    {
        public string TransporterId { get; set; }
        public string TransporterName { get; set; }
    }

    public class OfflineDriverDTO
    {
        public string TransporterId { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
    }

    public class OfflineTruckDTO
    {
        public string TransporterId { get; set; }
        public string TruckId { get; set; }
        public string PlateNumber { get; set; }
    }

}