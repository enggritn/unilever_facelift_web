using Facelift_App.Helper;
using Facelift_App.Services;
using Facelift_App.Validators;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;

namespace Facelift_App.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class PalletTypeValidationController : Controller
    {

        private readonly PalletTypeValidator validator;

        public PalletTypeValidationController(IPalletTypes PalletTypes)
        {
            validator = new PalletTypeValidator(PalletTypes);
        }

        public async Task<JsonResult> IsUniqueName(string PalletName, string x)
        {
            string errMsg = "";
            try
            {
                string id = "";
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                }

                errMsg = await validator.IsUniqueName(PalletName, id);
            }
            catch (Exception)
            {
                errMsg = "Validation failed. Try to refresh page or contact system administrator.";
            }

            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> IsTypeExist(string PalletTypeId)
        {
            return Json(await validator.IsTypeExist(PalletTypeId), JsonRequestBehavior.AllowGet);
        }
    }
}