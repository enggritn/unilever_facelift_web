using Facelift_App.Services;
using System.Threading.Tasks;

namespace Facelift_App.Validators
{
    public class CompanyValidator
    {

        private readonly ICompanies ICompanies;


        public CompanyValidator(ICompanies Companies)
        {
            ICompanies = Companies;
        }

        public async Task<string> IsUniqueName(string CompanyName, string id)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true";
            MsCompany data = await ICompanies.GetDataByCompanyNameAsync(CompanyName);
            if (data != null && !data.CompanyId.Equals(id))
            {
                errMsg = "Company Name already registered.";
            }
            return errMsg;
        }

        public async Task<string> IsUniqueAbb(string CompanyAbb, string id)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true";
            MsCompany data = await ICompanies.GetDataByCompanyAbbreviationAsync(CompanyAbb);
            if (data != null && !data.CompanyId.Equals(id))
            {
                errMsg = "Company Abbreviation already registered.";
            }
            return errMsg;
        }

        public async Task<string> IsExist(string CompanyId)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            return await ICompanies.GetDataByIdAsync(CompanyId) != null ? "true" : "Company not recognized.";
        }

    }
}