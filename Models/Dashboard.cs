using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facelift_App.Models
{
    public class Dashboard
    {
        public string TotalPallet { get; set; }
        public string TotalActualPallet { get; set; }
        public string TotalPalletInShipment { get; set; }
        public string TotalPalletIncoming { get; set; }
        public string TotalPalletGood { get; set; }
        public string TotalPalletDamage { get; set; }
        public string TotalPalletLoss { get; set; }
        public string TotalActualGood { get; set; }
        public string TotalActualDamage { get; set; }
        public string TotalActualLoss { get; set; }
        public string PercentageRegistration { get; set; }
        public string PercentageShipment { get; set; }
        public string PercentageCycleCount { get; set; }
        public string PercentageAccident { get; set; }
        public string TotalOutboundLoading { get; set; }
        public string TotalOutboundTransit { get; set; }
        public string TotalOutboundFinished { get; set; }
        public string TotalInboundTransit { get; set; }
        public string TotalInboundFinished { get; set; }
        public string TotalAccidentPending { get; set; }
        public IEnumerable<PalletStockDTO> PalletWarehouses { get; set; }
        public IEnumerable<PalletStockDTO> PalletGood { get; set; }
        public IEnumerable<PalletStockDTO> PalletDamage { get; set; }
        public IEnumerable<PalletStockDTO> PalletLoss { get; set; }
        public IEnumerable<PalletStockDTO> PalletDelivery { get; set; }
        public IEnumerable<PalletStockDTO> PalletIncoming { get; set; }
        public IEnumerable<AccidentDTO> AgingTransaction { get; set; }

        public IEnumerable<ShipmentAgingDTO> ShipmentAging { get; set; }
    }
}