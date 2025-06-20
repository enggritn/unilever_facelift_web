using Facelift_App.Services;
using System.Threading.Tasks;

namespace Facelift_App.Validators
{
    public class BillingConfigurationValidator
    {

        private readonly IBillings IBillings;


        public BillingConfigurationValidator(IBillings Billings)
        {
            IBillings = Billings;
        }

        public async Task<string> IsUniqueYear(int year, string id)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true";
            MsBillingConfiguration data = await IBillings.GetDataByYearAsync(year);
            if (data != null && !data.BillingId.Equals(id))
            {
                errMsg = "Depreciation Year already registered.";
            }
            return errMsg;
        }

    }
}