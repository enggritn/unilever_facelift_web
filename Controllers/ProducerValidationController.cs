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
    public class ProducerValidationController : Controller
    {

        private readonly ProducerValidator validator;

        public ProducerValidationController(IPalletProducers Producers)
        {
            validator = new ProducerValidator(Producers);
        }

        public async Task<JsonResult> IsUniqueName(string ProducerName, string x)
        {
            string errMsg = "";
            try
            {
                string id = "";
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                }

                errMsg = await validator.IsUniqueName(ProducerName, id);
            }
            catch (Exception)
            {
                errMsg = "Validation failed. Try to refresh page or contact system administrator.";
            }

            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }


        public async Task<JsonResult> IsExist(string ProducerName)
        {
            return Json(await validator.IsExist(ProducerName), JsonRequestBehavior.AllowGet);
        }
    }
}