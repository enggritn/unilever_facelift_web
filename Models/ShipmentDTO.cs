using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class ShipmentDTO
    {
        public string TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string ShipmentNumber { get; set; }
        public string Remarks { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string DestinationCode { get; set; }
        public string DestinationName { get; set; }
        public string TransporterName { get; set; }
        public string DriverName { get; set; }
        public string PlateNumber { get; set; }
        public string PalletQty { get; set; }
        public string ReceivedQty { get; set; }
        public string LossQty { get; set; }
        public string TransactionStatus { get; set; }
        public string ShipmentStatus { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedAt { get; set; }
    }

    public class ShipmentAgingDTO
    {
        public string DeliveryNumber { get; set; }
        public string OriginCode { get; set; }
        public string OriginName { get; set; }
        public string DestinationCode { get; set; }
        public string DestinationName { get; set; }
        public string ShipmentStatus { get; set; }
        public string CreatedAt { get; set; }
        public string AgingDay { get; set; }
        public string AgingMin { get; set; }
    }
}