using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IRoles
    {
        Task<IEnumerable<MsRole>> GetAllAsync(bool IsActive);
        IEnumerable<MsRole> GetFilteredData(string search, string sortDirection, string sortColName);
        int GetTotalData();
        Task<MsRole> GetDataByIdAsync(string id);
        Task<MsRole> GetDataByRoleNameAsync(string roleName);
        Task<bool> InsertAsync(MsRole data);
        Task<bool> UpdateAsync(MsRole data);
    }
}
