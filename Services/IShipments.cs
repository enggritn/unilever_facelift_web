using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IShipments
    {
        IEnumerable<TrxShipmentHeader> GetFilteredData(string WarehouseId, string search, string sortDirection, string sortColName);
        int GetTotalData(string WarehouseId);
        
        Task<TrxShipmentHeader> GetDataByIdAsync(string id);
        TrxShipmentHeader GetDataById(string id);
        Task<TrxShipmentHeader> GetDataByTransactionCodeAsync(string TransactionCode);
        Task<bool> CreateAsync(TrxShipmentHeader data);
        Task<bool> UpdateAsync(TrxShipmentHeader data);
        Task<bool> DeleteAsync(TrxShipmentHeader data);
        Task<bool> CloseAsync(TrxShipmentHeader data);

        IEnumerable<VwShipmentItem> GetFilteredDataItem(string TransactionId, string search, string sortDirection, string sortColName);
        IEnumerable<VwShipmentItem> GetDataItem(string TransactionId);
        int GetTotalDataItem(string TransactionId);
        Task<TrxShipmentItem> GetDataByTransactionTagIdAsync(string TransactionId, string TagId);
        Task<bool> InsertItemAsync(TrxShipmentHeader data, string username, string actionName);
        Task<bool> DeleteItemAsync(TrxShipmentHeader data, string[] items, string username);
        IEnumerable<VwShipmentItem> GetDetailByTransactionId(string TransactionId);
        Task<bool> DispatchItemAsync(TrxShipmentHeader data, string username, string actionName);
        Task<bool> ReceiveItemAsync(TrxShipmentHeader data, string username, string actionName);
        Task<bool> UpdateItemAsync(string WarehouseId, string TagId);

        IEnumerable<TrxShipmentHeader> GetOutboundData(string WarehouseId, string stats, string search, string sortDirection, string sortColName);
        int GetTotalOutboundData(string WarehouseId, string stats);

        IEnumerable<TrxShipmentHeader> GetInboundData(string WarehouseId, string stats, string search, string sortDirection, string sortColName);
        int GetTotalInboundData(string WarehouseId, string stats);

        IEnumerable<TrxShipmentHeader> GetListOutbound(string warehouseId);
        IEnumerable<TrxShipmentHeader> GetListInbound(string warehouseId);

        //Task<bool> CreateAccidentReportAsync(TrxShipmentHeader data);
        Task<bool> CreateAccidentReportAsync(TrxShipmentHeader data, string username);

        IEnumerable<TrxShipmentHeader> GetDeliveryData(string WarehouseId, string DestinationId, string startDate, string endDate, string search, string sortDirection, string sortColName);
        IEnumerable<TrxShipmentHeader> GetDeliveryData(string WarehouseId, string DestinationId, string startDate, string endDate);
        int GetTotalDelivery(string WarehouseId, string DestinationId, string startDate, string endDate);

        IEnumerable<TrxShipmentHeader> GetIncomingData(string WarehouseId, string OriginId, string startDate, string endDate, string search, string sortDirection, string sortColName);
        IEnumerable<TrxShipmentHeader> GetIncomingData(string WarehouseId, string OriginId, string startDate, string endDate);
        int GetTotalIncoming(string WarehouseId, string OriginId, string startDate, string endDate);

        IEnumerable<TrxShipmentHeader> GetAllOutboundTransactions(string warehouseId);
        IEnumerable<TrxShipmentHeader> GetAllInboundTransactions(string warehouseId);
        Task<IEnumerable<TrxShipmentHeader>> GetDataAllInboundTransactionProgress();
        Task<IEnumerable<TrxShipmentHeader>> GetDataAllOutboundTransactionProgress();

        Task<bool> DispatchManualAsync(TrxShipmentHeader data);
        Task<bool> ReceiveManualAsync(TrxShipmentHeader data);
        Task<bool> UpdateWarningCountAsync(TrxShipmentHeader data);

        Task<TrxShipmentItemTemp> GetDataByTransactionTagIdTempAsync(string TransactionId, string TagId, string StatusShipment);
        Task<TrxShipmentItemTemp> GetDataByTransactionIdTempAsync(string TransactionId);
        Task<bool> InsertItemTempAsync(TrxShipmentItemTemp data);
        Task<bool> DeleteItemTempAsync(TrxShipmentItemTemp data);
    }
}
