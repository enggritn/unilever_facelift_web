using Facelift_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IDashboards
    {
        int TotalRegistration(string WarehouseId);
        int TotalOutbound(string WarehouseId);
        int TotalInbound(string WarehouseId);
        int TotalInspection(string WarehouseId);
        int TotalCycleCount(string WarehouseId);
        int TotalRecall(string WarehouseId);

        int TotalOutboundLoading(string WarehouseId);
        int TotalOutboundTransit(string WarehouseId);
        int TotalOutboundFinished(string WarehouseId);
        int TotalInboundTransit(string WarehouseId);
        int TotalInboundFinished(string WarehouseId);

        int TotalPalletOutbound(string OriginId);
        int TotalPalletInbound(string DestinationId);


        IEnumerable<PalletStockDTO> PalletDelivery(string OriginId);
        IEnumerable<PalletStockDTO> PalletIncoming(string DestinationId);
        IEnumerable<VwShipmentAccident> GetListAging(string WarehouseId);

        IEnumerable<ShipmentAgingDTO> GetShipmentAging(string WarehouseId);


    }
}
