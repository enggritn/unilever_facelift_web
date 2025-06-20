using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IBillingHistories
    {
        //rent
        IEnumerable<TrxBillingRentHistoryHeader> GetFilteredRentData(string WarehouseId, string AgingType,string search, string sortDirection, string sortColName);
        int GetTotalRentData(string WarehouseId, string AgingType);
        IEnumerable<TrxBillingRentHistoryHeader> GetRentData(string WarehouseId, string AgingType);

        Task<TrxBillingRentHistoryHeader> GetRentDataByIdAsync(string id);
        Task<TrxBillingRentHistoryHeader> GetRentDataByTransactionCodeAsync(string TransactionCode);
        Task<TrxBillingRentHistoryHeader> GetRentInvoiceAsync(string WarehouseId, string AgingType ,int CurrentYear, int CurrentMonth);
        Task<bool> CreateRentAsync(TrxBillingRentHistoryHeader data);

        IEnumerable<VwBillingRentHistoryItem> GetFilteredRentDataItem(string TransactionId, string search, string sortDirection, string sortColName);
        int GetTotalRentDataItem(string TransactionId);
        double? GetTotalRentPrice(string TransactionId);

        IEnumerable<VwBillingRentHistoryItem> GetRentDataItemsById(string transactionId);
        Task<VwBillingRentHistoryItem> GetRentDetailByIdAsync(string transactionItemId);


        //accident
    }
}
