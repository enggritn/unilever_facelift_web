using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IUsers
    {
        Task<IEnumerable<MsUser>> GetListAsync();
        IEnumerable<MsUser> GetFilteredData(string search, string sortDirection, string sortColName);
        int GetTotalData();
        Task<MsUser> GetDataByIdAsync(string id);
        Task<MsUser> GetDataByUsernameAsync(string username);
        Task<MsUser> GetDataByEmailAsync(string email);
        Task<bool> InsertAsync(MsUser data);
        Task<bool> UpdateAsync(MsUser data);
        Task LastLoginAsync(MsUser data);
        Task<bool> ChangePasswordAsync(MsUser data);
        Task ChangeDefaultWarehouse(MsUser data);
    }
}
