using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IPallets
    {
        Task<MsPallet> GetDataByTagIdAsync(string TagId);
        Task<MsPallet> GetDataByPalletCodeAsync(string PalletCode);
        Task<MsPallet> GetDataByTagWarehouseIdAsync(string TagId, string WarehouseId);

        IEnumerable<VwPallet> GetTotalStock(string OriginId, string PalletCondition, string search, string sortDirection, string sortColName);
        IEnumerable<VwPallet> GetStockReport(string OriginId);
        IEnumerable<VwPallet> GetStockActualReport(string OriginId);
        IEnumerable<VwPalletShipment> GetStockDeliveryReport(string OriginId);
        int GetTotalStock(string OriginId);

        IEnumerable<MsPallet> GetActualStock(string WarehouseId, string PalletCondition, string search, string sortDirection, string sortColName);
        IEnumerable<MsPallet> GetActualStockReport(string WarehouseId);
        int GetActualStock(string WarehouseId);

        int GetTotalInShipment(string WarehouseId);

        int GetTotalByCondition(string OriginId, string Condition);
        int GetActualByCondition(string WarehouseId, string Condition);

        IEnumerable<VwPalletWarehouse> GetWarehouseStock(string OriginId);

        IEnumerable<VwPalletWarehouseCondition> GetWarehouseStockByCondition(string OriginId, string PalletCondition);

        IEnumerable<VwPalletShipment> GetDeliveryStock(string OriginId, string search, string sortDirection, string sortColName);
        int GetDeliveryStock(string OriginId);

        IEnumerable<VwPalletShipment> GetIncomingStock(string DestinationId, string search, string sortDirection, string sortColName);
        int GetIncomingStock(string DestinationId);

        IEnumerable<VwPalletMovement> GetPalletMovement(string WarehouseId, string startDate, string endDate, string search, string sortDirection, string sortColName);
        IEnumerable<VwPalletMovement> GetPalletMovement(string WarehouseId);

        IEnumerable<VwPalletHistory> GetPalletHistory(string WarehouseId, string search, string sortDirection, string sortColName);
        IEnumerable<VwPalletHistory> GetPalletHistory(string WarehouseId);
        int GetTotalPalletHistory(string WarehouseId);


        //update stock manually
        Task<bool> UpdateManualAsync(string tagId, string newWarehouseName, string palletCondition);
    }
}
