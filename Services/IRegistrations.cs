using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IRegistrations
    {
        IEnumerable<TrxRegistrationHeader> GetFilteredData(string WarehouseId, string stats, string search, string sortDirection, string sortColName);
        int GetTotalData(string WarehouseId, string stats);
        IEnumerable<TrxRegistrationHeader> GetAvailableTransactions(string warehouseId);
        Task<TrxRegistrationHeader> GetDataByIdAsync(string id);
        TrxRegistrationHeader GetDataById(string id);
        Task<bool> CreateAsync(TrxRegistrationHeader data);
        Task<bool> UpdateAsync(TrxRegistrationHeader data);
        Task<bool> DeleteAsync(TrxRegistrationHeader data);
        Task<bool> CloseAsync(TrxRegistrationHeader data);

        IEnumerable<VwRegistrationItem> GetFilteredDataItem(string TransactionId, string search, string sortDirection, string sortColName);
        IEnumerable<VwRegistrationItem> GetDataItem(string TransactionId);
        int GetTotalDataItem(string TransactionId);
        Task<TrxRegistrationItem> GetDataByTransactionTagIdAsync(string TransactionId, string TagId);
        Task<bool> InsertItemAsync(TrxRegistrationHeader data, string username, string actionName);
        Task<bool> DeleteItemAsync(TrxRegistrationHeader data, string[] items, string username);
        IEnumerable<VwRegistrationItem> GetDetailByTransactionId(string TransactionId);



        IEnumerable<TrxRegistrationHeader> GetAllTransactions(string warehouseId);
    }
}
