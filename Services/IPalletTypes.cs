using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IPalletTypes
    {
        Task<IEnumerable<MsPalletType>> GetAllAsync();
        IEnumerable<MsPalletType> GetFilteredData(string search, string sortDirection, string sortColName);
        int GetTotalData();
        Task<MsPalletType> GetDataByIdAsync(string id);
        Task<MsPalletType> GetDataByPalletNameAsync(string PalletName);
        Task<bool> InsertAsync(MsPalletType data);
        Task<bool> UpdateAsync(MsPalletType data);
    }
}
