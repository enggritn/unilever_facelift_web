using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface ICompanies
    {
        Task<IEnumerable<MsCompany>> GetAllAsync();
        IEnumerable<MsCompany> GetFilteredData(string search, string sortDirection, string sortColName);
        int GetTotalData();
        Task<MsCompany> GetDataByIdAsync(string id);
        Task<MsCompany> GetDataByCompanyNameAsync(string CompanyName);
        Task<MsCompany> GetDataByCompanyAbbreviationAsync(string CompanyAbb);
        Task<bool> InsertAsync(MsCompany data);
        Task<bool> UpdateAsync(MsCompany data);
    }
}
