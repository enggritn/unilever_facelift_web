using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models.Api
{
    public class ShipmentVM
    {
        public string ShipmentNumber { get; set; }
        public string OriginId { get; set; }
        public string DestinationId { get; set; }
        public string TransporterId { get; set; }
        public string DriverId { get; set; }
        public string TruckId { get; set; }
        public int PalletQty { get; set; }
        public string TransactionDate { get; set; }
        public string Remarks { get; set; }

        public List<ShipmentItemVM> items { get; set; }
    }

    public class OfflineHeaderShipmentVM
    {
        public string TransactionCode { get; set; }
        public string ShipmentNumber { get; set; }
        public string OriginName { get; set; }
        public string DestinationName { get; set; }
        public string TransporterName { get; set; }
        public string DriverName { get; set; }
        public string PlateNumber { get; set; }
        public int PalletQty { get; set; }
        public string TransactionDate { get; set; }
        public string Remarks { get; set; }
    }

    public class OfflineItemShipmentVM
    {
        public string TransactionCode { get; set; }
        public string TagId { get; set; }
        public string ScannedAt { get; set; }
        public string ShipmentType { get; set; }
    }
}