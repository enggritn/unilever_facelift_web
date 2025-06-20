using Facelift_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IEquipment
    {
        Task<IEnumerable<MsEquipment>> GetByRoleIdAsync();
        IEnumerable<MsEquipment> GetFilteredData(string search, string sortDirection, string sortColName);
        int GetTotalData();

        Task<MsEquipment> GetDataByWarehouseAsync(string WarehouseName);
        Task<bool> InsertAsync(MsEquipment data);
        Task<bool> UpdateAsync(MsEquipment data);

        Task<MsEquipment> GetDataByIdAsync(string id);

    }
}
