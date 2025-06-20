using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IPalletProducers
    {
        Task<IEnumerable<MsProducer>> GetAllAsync();
        IEnumerable<MsProducer> GetFilteredData(string search, string sortDirection, string sortColName);
        int GetTotalData();
        Task<MsProducer> GetDataByIdAsync(string id);
        Task<MsProducer> GetDataByProducerNameAsync(string ProducerName);
        Task<bool> InsertAsync(MsProducer data);
        Task<bool> UpdateAsync(MsProducer data);
    }
}
