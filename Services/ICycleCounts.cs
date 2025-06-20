using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface ICycleCounts
    {
        IEnumerable<TrxCycleCountHeader> GetFilteredData(string WarehouseId, string stats, string search, string sortDirection, string sortColName);
        int GetTotalData(string WarehouseId, string stats);
        int GetTotalOpenData(string WarehouseId);
        int GetTotalProgressShipment(string WarehouseId);


        Task<TrxCycleCountHeader> GetDataByIdAsync(string id);
        TrxCycleCountHeader GetDataById(string id);
        Task<TrxCycleCountHeader> GetDataByTransactionCodeAsync(string TransactionCode);
        Task<bool> CreateAsync(TrxCycleCountHeader data);
        Task<bool> UpdateAsync(TrxCycleCountHeader data);
        Task<bool> DeleteAsync(TrxCycleCountHeader data);
        Task<bool> CloseAsync(TrxCycleCountHeader data);

        IEnumerable<VwCycleCountItem> GetFilteredDataItem(string TransactionId, string search, string sortDirection, string sortColName);
        IEnumerable<VwCycleCountItem> GetDataItem(string TransactionId);
        int GetTotalDataItem(string TransactionId);
        Task<TrxCycleCountItem> GetDataByTransactionTagIdAsync(string TransactionId, string TagId);
        IEnumerable<VwCycleCountItem> GetDetailByTransactionId(string TransactionId);
        Task<bool> UpdateItemAsync(TrxCycleCountHeader data, List<string> items, string username, string actionName);
        Task<bool> UpdatePalletAsync(string WarehouseId, string TagId, string PalletCondition);

        IEnumerable<TrxCycleCountHeader> GetList(string warehouseId);

        Task<bool> CreateAccidentReportAsync(TrxCycleCountHeader data, string username);

        IEnumerable<TrxCycleCountHeader> GetCycleCountData(string WarehouseId, string startDate, string endDate, string search, string sortDirection, string sortColName);
        IEnumerable<TrxCycleCountHeader> GetCycleCountData(string WarehouseId, string startDate, string endDate);
        int GetTotalCycleCount(string WarehouseId, string startDate, string endDate);

        IEnumerable<TrxCycleCountHeader> GetAllTransactions(string warehouseId);
    }
}
