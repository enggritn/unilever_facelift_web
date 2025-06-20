using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Facelift_App.Models
{
    public class ShipmentVM
    {
        public string TransactionCode { get; set; }
        public string ShipmentNumber { get; set; }
        public string Remarks { get; set; }
        [Required(ErrorMessage = "Warehouse is required.")]
        [Remote("IsWarehouseExist", "WarehouseValidation")]
        public string WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }

        [Required(ErrorMessage = "Destination is required.")]
        [Remote("IsWarehouseExist", "WarehouseValidation")]
        public string DestinationId { get; set; }
        public string DestinationCode { get; set; }
        public string DestinationName { get; set; }

        [Required(ErrorMessage = "Transporter is required.")]
        [Remote("IsTransporterExist", "TransporterValidation")]
        public string TransporterId { get; set; }
        public string TransporterName { get; set; }

        [Required(ErrorMessage = "Driver is required.")]
        [Remote("IsDriverExist", "TransporterValidation")]
        public string DriverId { get; set; }
        public string DriverName { get; set; }

        [Required(ErrorMessage = "Truck is required.")]
        [Remote("IsTruckExist", "TransporterValidation")]
        public string TruckId { get; set; }
        public string PlateNumber { get; set; }

        //[Range(1, int.MaxValue, ErrorMessage = "Quantity must bigger than 0")]
        [Remote("IsMGVAllowed", "WarehouseValidation", AdditionalFields = "DestinationId")]
        public int PalletQty { get; set; }

        public string TransactionStatus { get; set; }
        public string ShipmentStatus { get; set; }


        public string CreatedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime? ModifiedAt { get; set; }
        public string ApprovedBy { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:ii:ss}")]
        public DateTime? ApprovedAt { get; set; }

        public List<LogShipmentHeader> logs { get; set; }
        public List<LogShipmentDocument> versions { get; set; }

        public ShipmentBA shipmentBA { get; set; }
    }

}