using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IWarehouseCategories
    {
        Task<IEnumerable<MsWarehouseCategory>> GetAllAsync();
        Task<MsWarehouseCategory> GetDataByIdAsync(string id);
    }
}
