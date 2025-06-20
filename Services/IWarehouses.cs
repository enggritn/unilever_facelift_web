using Facelift_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IWarehouses
    {
        Task<IEnumerable<MsWarehouse>> GetAllAsync();
        IEnumerable<MsWarehouse> GetFilteredData(string search, string sortDirection, string sortColName);
        int GetTotalData();
        Task<MsWarehouse> GetDataByIdAsync(string id);
        Task<MsWarehouse> GetDataByWarehouseCodeAsync(string WarehouseCode);
        Task<MsWarehouse> GetDataByWarehouseNameAsync(string WarehouseName);
        Task<MsWarehouse> GetDataByWarehouseAliasAsync(string WarehouseAlias);
        Task<bool> InsertAsync(MsWarehouse data);
        Task<bool> UpdateAsync(MsWarehouse data);

        IEnumerable<MsWarehouse> GetByUsername(string Username);
        //IEnumerable<MsWarehouseAccess> GetAccessByUsername(string Username);
        Task<bool> CheckAccessAsync(string Username, string WarehouseId);

        Task<IEnumerable<MsWarehouse>> GetDestinationAsync(string originId);

        int GetTotalWeight();


    }
}
