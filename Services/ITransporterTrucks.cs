using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface ITransporterTrucks
    {
        Task<IEnumerable<MsTruck>> GetAllAsync(string TransporterId);
        IEnumerable<MsTruck> GetFilteredData(string id, string search, string sortDirection, string sortColName);
        int GetTotalData(string id);
        Task<MsTruck> GetDataByIdAsync(string id);
        Task<MsTruck> GetDataByPlateNumberAsync(string PlateNumber);
        Task<bool> InsertAsync(MsTruck data);
        Task<bool> UpdateAsync(MsTruck data);
    }
}
