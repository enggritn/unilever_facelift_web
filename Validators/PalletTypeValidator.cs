using Facelift_App.Services;
using System.Threading.Tasks;

namespace Facelift_App.Validators
{
    public class PalletTypeValidator
    {

        private readonly IPalletTypes IPalletTypes;


        public PalletTypeValidator(IPalletTypes PalletTypes)
        {
            IPalletTypes = PalletTypes;
        }

        public async Task<string> IsUniqueName(string PalletName, string id)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true";
            MsPalletType data = await IPalletTypes.GetDataByPalletNameAsync(PalletName);
            if (data != null && !data.PalletTypeId.Equals(id))
            {
                errMsg = "Pallet Name already registered.";
            }
            return errMsg;
        }

        public async Task<string> IsTypeExist(string PalletTypeId)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            return await IPalletTypes.GetDataByIdAsync(PalletTypeId) != null ? "true" : "Pallet Type not recognized.";
        }

    }
}