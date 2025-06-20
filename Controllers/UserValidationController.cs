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
    public class UserValidationController : Controller
    {

        private readonly UserValidator validator;

        public UserValidationController(IUsers Users)
        {
            validator = new UserValidator(Users);
        }

        public async Task<JsonResult> IsUsernameUnique(string Username)
        {
            string errMsg = await validator.IsUniqueUsername(Username);
            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> IsUniqueEmail(string UserEmail, string x)
        {
            string errMsg = "";
            try
            {
                string id = "";
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                }

                errMsg = await validator.IsUniqueEmail(UserEmail, id);
            }
            catch (Exception)
            {
                errMsg = "Validation failed. Try to refresh page or contact system administrator.";
            }

            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> IsPasswordMatch(string CurrentPassword)
        {
            string username = Session["username"].ToString();
            string errMsg = await validator.IsPasswordMatch(username, CurrentPassword);
            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }

    }
}