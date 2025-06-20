using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface ITransporters
    {
        Task<IEnumerable<MsTransporter>> GetAllAsync();
        IEnumerable<MsTransporter> GetFilteredData(string search, string sortDirection, string sortColName);
        int GetTotalData();
        Task<MsTransporter> GetDataByIdAsync(string id);
        Task<MsTransporter> GetDataByTransporterNameAsync(string TransporterName);
        Task<bool> InsertAsync(MsTransporter data);
        Task<bool> UpdateAsync(MsTransporter data);
        Task<List<MsTransporter>> GetAllByWarehouseAsync(string WarehouseId);

    }
}
