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
    public class RoleValidationController : Controller
    {

        private readonly RoleValidator validator;

        public RoleValidationController(IRoles Roles)
        {
            validator = new RoleValidator(Roles);
        }

        public async Task<JsonResult> IsUniqueName(string RoleName, string x)
        {
            string errMsg = "";
            try
            {
                string id = "";
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                }

                errMsg = await validator.IsUniqueName(RoleName, id);
            }
            catch (Exception)
            {
                errMsg = "Validation failed. Try to refresh page or contact system administrator.";
            }

            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> IsRoleExist(string RoleId)
        {
            return Json(await validator.IsRoleExist(RoleId), JsonRequestBehavior.AllowGet);
        }

    }
}