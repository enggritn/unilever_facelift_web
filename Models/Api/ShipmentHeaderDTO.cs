using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class ShipmentHeaderDTO
    {
        public string TransactionId { get; set; }
        public string TransactionCode { get; set; }
        public string ShipmentNumber { get; set; }
        public string Remarks { get; set; }
        public string WarehouseName { get; set; }
        public string DestinationName { get; set; }
        public string TransporterName { get; set; }
        public string DriverName { get; set; }
        public string PlateNumber { get; set; }
        public string PalletQty { get; set; }
        public string TransactionStatus { get; set; }
        public string ShipmentStatus { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedAt { get; set; }

        public IEnumerable<ShipmentItemDTO> items { get; set; }
    }

    public class OfflineShipmentHeaderDTO
    {
        public string TransactionCode { get; set; }
        public string ShipmentNumber { get; set; }
        public string Remarks { get; set; }
        public string OriginName { get; set; }
        public string DestinationName { get; set; }
        public string TransporterName { get; set; }
        public string DriverName { get; set; }
        public string PlateNumber { get; set; }
        public string PalletQty { get; set; }
        public string CreatedAt { get; set; }
        public string UploadedAt { get; set; }

        public IEnumerable<OfflineShipmentItemDTO> items { get; set; }

    }

    public class OfflineShipmentItemDTO
    {
        public string TransactionCode { get; set; }
        public string TagId { get; set; }
        public string ScannedAt { get; set; }
        public string UploadedAt { get; set; }

    }
}