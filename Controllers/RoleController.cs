using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Facelift_App.Validators;
using System.Web.Mvc;
using Facelift_App.Models;
using Facelift_App.Helper;
using Facelift_App.Services;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class RoleController : Controller
    {
        private readonly IRoles IRoles;
        private readonly IMenus IMenus;


        public RoleController(IRoles Roles, IMenus Menus)
        {
            IRoles = Roles;
            IMenus = Menus;
        }


        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Datatable()
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            IEnumerable<MsRole> list = IRoles.GetFilteredData(search, sortDirection, sortName);
            IEnumerable<RoleDTO> pagedData = Enumerable.Empty<RoleDTO>();

            int recordsTotal = IRoles.GetTotalData();
            int recordsFilteredTotal = list.Count();

            list = list.Skip(start).Take(length).ToList();

            
            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new RoleDTO
                            {
                                Id = Utilities.EncodeTo64(Encryptor.Encrypt(x.RoleId, Constant.facelift_encryption_key)),
                                RoleId = x.RoleId,
                                RoleName = x.RoleName,
                                RoleDescription = x.RoleDescription,
                                IsActive = Utilities.IsActiveStatusBadge(x.IsActive),
                                CreatedBy = x.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt)
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> Create()
        {
            RoleVM roleVM = new RoleVM();
            roleVM.MenuList = await IMenus.GetAllAsync();
            return View(roleVM);
        }

        private async Task CreateValidation(RoleVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.RoleName))
            {
                RoleValidator validator = new RoleValidator(IRoles);
                string RoleValid = await validator.IsUniqueName(dataVM.RoleName, "");
                if (!RoleValid.Equals("true"))
                {
                    ModelState.AddModelError("RoleName", RoleValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(RoleVM dataVM)
        {
            //ModelState["MemberName"].Errors.Clear();
            bool result = false;
            string message = "Invalid form submission.";

            //server validation
            await CreateValidation(dataVM);

            if (ModelState.IsValid)
            {
                dataVM.RoleName = Utilities.UpperFirstCase(dataVM.RoleName);

                //int indexOfAt = dataVM.UserEmail.IndexOf('@');
                //string username = dataVM.UserEmail.Substring(0, indexOfAt);

                MsRole data = new MsRole
                {
                    RoleId = Utilities.CreateGuid("R"),
                    RoleName = dataVM.RoleName,
                    RoleDescription = dataVM.RoleDescription,
                    IsActive = true,
                    CreatedBy = Session["username"].ToString()
                };

                data.MsRolePermissions = new List<MsRolePermission>();
                if (dataVM.MenuIds != null && dataVM.MenuIds.Count() > 0)
                {
                    foreach (int menuId in dataVM.MenuIds)
                    {
                        MsRolePermission rolePermission = new MsRolePermission
                        {
                            RoleId = data.RoleId,
                            MenuId = menuId
                        };

                        data.MsRolePermissions.Add(rolePermission);
                    }
                }

                result = await IRoles.InsertAsync(data);
                if (result)
                {
                    message = "Create data succeeded.";
                }
                else
                {
                    message = "Create data failed. Please contact system administrator.";
                }

            }

            return Json(new { stat = result, msg = message });
        }


        public async Task<ActionResult> Detail(string x)
        {
            string id = "";
            MsRole data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await IRoles.GetDataByIdAsync(id);
                    if (data == null)
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Create");
            }

            RoleVM dataVM = new RoleVM
            {
                RoleName = data.RoleName,
                RoleDescription = data.RoleDescription,
                IsActive = data.IsActive
            };

            dataVM.MenuIds = data.MsRolePermissions.Select(m => m.MenuId).ToArray();

            dataVM.MenuList = await IMenus.GetAllAsync();

            ViewBag.Id = x;
            return View(dataVM);
        }

        private async Task DetailValidation(RoleVM dataVM, string id)
        {
            if (!string.IsNullOrEmpty(dataVM.RoleName))
            {
                RoleValidator validator = new RoleValidator(IRoles);
                string RoleValid = await validator.IsUniqueName(dataVM.RoleName, id);
                if (!RoleValid.Equals("true"))
                {
                    ModelState.AddModelError("RoleName", RoleValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, RoleVM dataVM)
        {
            bool result = false;
            string message = "Invalid form submission.";
            string id = "";

            try
            {
                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception();
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
            }
            catch (Exception)
            {
                message = "Update data failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }
            
            MsRole data = await IRoles.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            await DetailValidation(dataVM, id);

            if (ModelState.IsValid)
            {
                dataVM.RoleName = Utilities.UpperFirstCase(dataVM.RoleName);

                data.RoleName = dataVM.RoleName;
                data.RoleDescription = dataVM.RoleDescription;
                data.IsActive = dataVM.IsActive;
                data.ModifiedBy = Session["username"].ToString();

                List<MsRolePermission> prevPermission = data.MsRolePermissions.ToList();
                int[] PrevMenuIds = prevPermission.Select(m => m.MenuId).ToArray();

                List<MsRolePermission> rolePermissions = new List<MsRolePermission>();
                if (dataVM.MenuIds != null && dataVM.MenuIds.Count() > 0)
                {
                    foreach (int menuId in dataVM.MenuIds)
                    {
                        MsRolePermission rolePermission = new MsRolePermission();
                        if (!PrevMenuIds.Contains(menuId))
                        {
                            rolePermission.RoleId = data.RoleId;
                            rolePermission.MenuId = menuId;
                        }
                        else
                        {
                            rolePermission = prevPermission.Where(m => m.MenuId.Equals(menuId)).FirstOrDefault();
                        }
                        rolePermissions.Add(rolePermission);
                    }
                }

                data.MsRolePermissions = rolePermissions.ToList();

                result = await IRoles.UpdateAsync(data);

                if (result)
                {
                    message = "Update data succeeded.";
                }
                else
                {
                    message = "Update data failed. Please contact system administrator.";
                }
            }
            return Json(new { stat = result, msg = message });
        }



    }
}
