using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface ITransporterDrivers
    {
        Task<IEnumerable<MsDriver>> GetAllAsync(string TransporterId);
        IEnumerable<MsDriver> GetFilteredData(string TransporterId, string search, string sortDirection, string sortColName);
        int GetTotalData(string TransporterId);
        Task<MsDriver> GetDataByIdAsync(string id);
        Task<MsDriver> GetDataByLicenseNumberAsync(string LicenseNumber);
        Task<bool> InsertAsync(MsDriver data);
        Task<bool> UpdateAsync(MsDriver data);
    }
}
