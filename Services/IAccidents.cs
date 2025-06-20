using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IAccidents
    {
        IEnumerable<TrxAccidentHeader> GetFilteredData(string WarehouseId, string stats, string search, string sortDirection, string sortColName);
        int GetTotalData(string WarehouseId, string stats);

        Task<TrxAccidentHeader> GetDataByIdAsync(string id);
        TrxAccidentHeader GetDataById(string id);
        Task<TrxAccidentHeader> GetDataByTransactionCodeAsync(string TransactionCode);
        Task<bool> CreateAsync(TrxAccidentHeader data);
        Task<bool> UpdateAsync(TrxAccidentHeader data);
        Task<bool> DeleteAsync(TrxAccidentHeader data);
        Task<bool> CloseAsync(TrxAccidentHeader data);

        IEnumerable<VwAccidentItem> GetFilteredDataItem(string TransactionId, string search, string sortDirection, string sortColName);
        IEnumerable<VwAccidentItem> GetDataItem(string TransactionId);
        int GetTotalDataItem(string TransactionId);

        Task<TrxAccidentItem> GetDataByTransactionTagIdAsync(string TransactionId, string TagId);
        IEnumerable<VwAccidentItem> GetDetailByTransactionId(string TransactionId);
        Task<bool> InsertItemAsync(TrxAccidentHeader data, string username, string actionName);
        Task<bool> DeleteItemAsync(TrxAccidentHeader data, string[] items, string username);

        IEnumerable<TrxAccidentHeader> GetList(string warehouseId);

        IEnumerable<TrxAccidentHeader> GetAccidentData(string WarehouseId, string startDate, string endDate, string search, string sortDirection, string sortColName);
        IEnumerable<TrxAccidentHeader> GetAccidentData(string WarehouseId, string startDate, string endDate);
        int GetTotalAccident(string WarehouseId, string startDate, string endDate);


        IEnumerable<TrxAccidentHeader> GetAllTransactions(string warehouseId);

        Task<bool> UpdateItemAsync(TrxAccidentItem item);
    }
}
