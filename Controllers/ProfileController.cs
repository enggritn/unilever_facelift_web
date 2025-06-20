using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Facelift_App.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    public class ProfileController : Controller
    {
        private readonly IUsers IUsers;
        private readonly IWarehouses IWarehouses;

        public ProfileController(IUsers Users, IWarehouses Warehouses)
        {
            IUsers = Users;
            IWarehouses = Warehouses;
        }

        // GET: Dashboard
        public async Task<ActionResult> Index()
        {
            string username = Session["username"].ToString();
            MsUser msUser = await IUsers.GetDataByUsernameAsync(username);
            Session.Add("full_name", msUser.FullName);
            ViewBag.Title = "Profile";

            ProfileVM dataVM = new ProfileVM
            {
                Username = msUser.Username,
                UserEmail = msUser.UserEmail,
                FullName = msUser.FullName,
                RoleName = msUser.MsRole.RoleName,
                LastVisitUrl = msUser.LastVisitUrl,
                LastVisitAt = msUser.LastVisitAt,
                LastLoginAt = msUser.LastLoginAt,
                CreatedBy = msUser.CreatedBy,
                CreatedAt = msUser.CreatedAt,
                ModifiedBy = msUser.ModifiedBy,
                ModifiedAt = msUser.ModifiedAt,
                ChPassAt = msUser.ChPassAt,
                WarehouseId = msUser.DefaultWarehouseId
            };

            ViewBag.WarehouseList = msUser.MsWarehouseAccesses.OrderBy(x => x.MsWarehouse.WarehouseCode);

            return View(dataVM);
        }

        [HttpPost]
        public async Task<JsonResult> ChangePassword(ProfileVM dataVM)
        {
            //get user by current session
            string id = Session["username"].ToString();
            bool result = false;
            string message = "Invalid form submission.";

            if (!string.IsNullOrEmpty(dataVM.CurrentPassword))
            {
                UserValidator validator = new UserValidator(IUsers);
                string PasswordValid = await validator.IsPasswordMatch(id, dataVM.CurrentPassword);
                if (!PasswordValid.Equals("true"))
                {
                    ModelState.AddModelError("CurrentPassword", PasswordValid);
                }
            }

            if (ModelState.IsValid)
            {
                MsUser data = await IUsers.GetDataByIdAsync(id);
                if (data != null)
                {
                    data.UserPassword = Encryptor.HashPassword(dataVM.NewPassword);
                    result = await IUsers.ChangePasswordAsync(data);

                    if (result)
                    {
                        message = "Change password succeeded.";
                    }
                    else
                    {
                        message = "Change password failed. Please contact system administrator.";
                    }
                }
                else
                {
                    message = "Data not found.";
                }
            }

            return Json(new { stat = result, msg = message });
        }


        [HttpPost]
        public async Task<JsonResult> ChangeDefaultWarehouse(ProfileVM dataVM)
        {
            ModelState["CurrentPassword"].Errors.Clear();
            ModelState["NewPassword"].Errors.Clear();
            ModelState["PasswordConfirmation"].Errors.Clear();
            //get user by current session
            string id = Session["username"].ToString();
            bool result = false;
            string message = "Invalid form submission.";

            if (!string.IsNullOrEmpty(dataVM.WarehouseId))
            {
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string WarehouseValid = await validator.IsWarehouseExist(dataVM.WarehouseId);
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("WarehouseId", WarehouseValid);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    MsUser data = await IUsers.GetDataByIdAsync(id);
                    if (data != null)
                    {
                        data.DefaultWarehouseId = dataVM.WarehouseId;

                        await IUsers.ChangeDefaultWarehouse(data);
                        result = true;
                        message = "Change default warehouse succeeded.";
                    }
                    else
                    {
                        message = "Data not found.";
                    }

                }
                catch(Exception)
                {
                    message = "Change default warehouse. Please contact system administrator.";
                }
            }

            return Json(new { stat = result, msg = message });
        }
    }
}