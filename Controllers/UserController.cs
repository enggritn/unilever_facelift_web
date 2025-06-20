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
    public class UserController : Controller
    {
        private readonly IUsers IUsers;
        private readonly IRoles IRoles;
        private readonly IWarehouses IWarehouses;
        private readonly ICompanies ICompanies;


        public UserController(IUsers Users, IRoles Roles, IWarehouses Warehouses, ICompanies Companies)
        {
            IUsers = Users;
            IRoles = Roles;
            IWarehouses = Warehouses;
            ICompanies = Companies;
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

            IEnumerable<MsUser> list = IUsers.GetFilteredData(search, sortDirection, sortName);
            IEnumerable<UserDTO> pagedData = Enumerable.Empty<UserDTO>(); ;

            int recordsTotal = IUsers.GetTotalData();
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();

            
            //re-format
            if (list != null && list.Count() > 0)
            {
                list = list.Select(x => new MsUser
                {
                    Username = x.Username,
                    FullName = x.FullName,
                    UserEmail = x.UserEmail,
                    MsRole = new MsRole
                    {
                        RoleId = x.MsRole != null ? x.MsRole.RoleId : "",
                        RoleName = x.MsRole != null ? x.MsRole.RoleName : ""
                    },
                    //MsCompany = new MsCompany
                    //{
                    //    CompanyId = x.MsCompany.CompanyId,
                    //    CompanyName = x.MsCompany.CompanyName
                    //},
                    IsActive = x.IsActive,
                    LastVisitUrl = x.LastVisitUrl,
                    LastVisitAt = x.LastVisitAt,
                    ChPassAt = x.ChPassAt,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedAt = x.ModifiedAt
                });

                pagedData = from x in list
                            select new UserDTO
                            {
                                Id = Utilities.EncodeTo64(Encryptor.Encrypt(x.Username, Constant.facelift_encryption_key)),
                                Username = x.Username,
                                FullName = x.FullName,
                                UserEmail = x.UserEmail,
                                RoleName = !x.MsRole.RoleName.Equals("") ? x.MsRole.RoleName : "-",
                                //CompanyName = x.MsCompany.CompanyName,
                                IsActive = Utilities.IsActiveStatusBadge(x.IsActive),
                                LastLoginAt = Utilities.NullDateTimeToString(x.LastLoginAt),
                                LastVisitUrl = !string.IsNullOrEmpty(x.LastVisitUrl) ? x.LastVisitUrl : "-",
                                LastVisitAt = Utilities.NullDateTimeToString(x.LastVisitAt),
                                ChPassAt = Utilities.NullDateTimeToString(x.ChPassAt),
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
            ViewBag.CompanyList = new SelectList(await ICompanies.GetAllAsync(), "CompanyId", "CompanyName");
            IEnumerable<MsRole> roles = await IRoles.GetAllAsync(true);
            ViewBag.RoleList = new SelectList(roles, "RoleId", "RoleName");
            ViewBag.WarehouseList = await IWarehouses.GetAllAsync();
            return View();
        }

        private async Task CreateValidation(UserVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.Username))
            {
                UserValidator validator = new UserValidator(IUsers);
                string UsernameValid = await validator.IsUniqueUsername(dataVM.Username);
                if (!UsernameValid.Equals("true"))
                {
                    ModelState.AddModelError("Username", UsernameValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.UserEmail))
            {
                UserValidator validator = new UserValidator(IUsers);
                string EmailValid = await validator.IsUniqueEmail(dataVM.UserEmail, "");
                if (!EmailValid.Equals("true"))
                {
                    ModelState.AddModelError("UserEmail", EmailValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.RoleId))
            {
                RoleValidator validator = new RoleValidator(IRoles);
                string RoleValid = await validator.IsRoleExist(dataVM.RoleId);
                if (!RoleValid.Equals("true"))
                {
                    ModelState.AddModelError("RoleId", RoleValid);
                }
            }

            //if (!string.IsNullOrEmpty(dataVM.CompanyId))
            //{
            //    CompanyValidator validator = new CompanyValidator(ICompanies);
            //    string IsValid = await validator.IsExist(dataVM.CompanyId);
            //    if (!IsValid.Equals("true"))
            //    {
            //        ModelState.AddModelError("CompanyId", IsValid);
            //    }
            //}

            //check warehouse exist
            if (!string.IsNullOrEmpty(dataVM.DefaultWarehouseId))
            {
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string WarehouseValid = await validator.IsWarehouseExist(dataVM.DefaultWarehouseId);
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("DefaultWarehouseId", WarehouseValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(UserVM dataVM)
        {
            //ModelState["MemberName"].Errors.Clear();
            bool result = false;
            string message = "Invalid form submission.";

            //server validation
            await CreateValidation(dataVM);


            if (ModelState.IsValid)
            {
                dataVM.Username = Utilities.ToLower(dataVM.Username);
                dataVM.UserEmail = Utilities.ToLower(dataVM.UserEmail);
                dataVM.FullName = Utilities.UpperFirstCase(dataVM.FullName);

                //int indexOfAt = dataVM.UserEmail.IndexOf('@');
                //string username = dataVM.UserEmail.Substring(0, indexOfAt);

                MsUser data = new MsUser
                {
                    Username = dataVM.Username,
                    UserEmail = dataVM.UserEmail,
                    UserPassword = Encryptor.HashPassword(dataVM.Password),
                    FullName = dataVM.FullName,
                    RoleId = dataVM.RoleId,
                    //CompanyId = dataVM.CompanyId,
                    DefaultWarehouseId = dataVM.DefaultWarehouseId,
                    IsActive = true,
                    CreatedBy = Session["username"].ToString()
                };


                data.MsWarehouseAccesses = new List<MsWarehouseAccess>();
                if (dataVM.WarehouseIds != null && dataVM.WarehouseIds.Count() > 0)
                {
                    foreach (string warehouseId in dataVM.WarehouseIds)
                    {
                        MsWarehouseAccess warehouseAccess = new MsWarehouseAccess
                        {
                            Username = data.Username,
                            WarehouseId = warehouseId
                        };

                        data.MsWarehouseAccesses.Add(warehouseAccess);
                    }
                }


                result = await IUsers.InsertAsync(data);
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
            MsUser data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await IUsers.GetDataByIdAsync(id);
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

            UserVM dataVM = new UserVM
            {
                Username = data.Username,
                UserEmail = data.UserEmail,
                FullName = data.FullName,
                RoleId = data.MsRole != null ? data.MsRole.RoleId : "",
                //CompanyId = data.CompanyId,
                DefaultWarehouseId = data.DefaultWarehouseId,
                IsActive = data.IsActive
            };

            dataVM.WarehouseIds = data.MsWarehouseAccesses.Select(m => m.WarehouseId).ToArray();

            IEnumerable<MsWarehouse> warehouses = await IWarehouses.GetAllAsync();
            ViewBag.SelectedWarehouseList = warehouses.Where(m => dataVM.WarehouseIds.Contains(m.WarehouseId));
            ViewBag.UnSelectedWarehouseList = warehouses.Where(m => !dataVM.WarehouseIds.Contains(m.WarehouseId));

            ViewBag.Id = x;
            ViewBag.RoleList = new SelectList(await IRoles.GetAllAsync(true), "RoleId", "RoleName");
            ViewBag.CompanyList = new SelectList(await ICompanies.GetAllAsync(), "CompanyId", "CompanyName");
            return View(dataVM);
        }

        private async Task DetailValidation(UserVM dataVM)
        {
            ModelState["Username"].Errors.Clear();
            ModelState["Password"].Errors.Clear();
            ModelState["PasswordConfirmation"].Errors.Clear();

            if (!string.IsNullOrEmpty(dataVM.UserEmail))
            {
                UserValidator validator = new UserValidator(IUsers);
                string EmailValid = await validator.IsUniqueEmail(dataVM.UserEmail, dataVM.Username);
                if (!EmailValid.Equals("true"))
                {
                    ModelState.AddModelError("UserEmail", EmailValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.RoleId))
            {
                RoleValidator validator = new RoleValidator(IRoles);
                string RoleValid = await validator.IsRoleExist(dataVM.RoleId);
                if (!RoleValid.Equals("true"))
                {
                    ModelState.AddModelError("RoleId", RoleValid);
                }
            }

            //if (!string.IsNullOrEmpty(dataVM.CompanyId))
            //{
            //    CompanyValidator validator = new CompanyValidator(ICompanies);
            //    string IsValid = await validator.IsExist(dataVM.CompanyId);
            //    if (!IsValid.Equals("true"))
            //    {
            //        ModelState.AddModelError("CompanyId", IsValid);
            //    }
            //}

            //check warehouse exist
            if (!string.IsNullOrEmpty(dataVM.DefaultWarehouseId))
            {
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string WarehouseValid = await validator.IsWarehouseExist(dataVM.DefaultWarehouseId);
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("DefaultWarehouseId", WarehouseValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, UserVM dataVM)
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
                dataVM.Username = id;
            }
            catch (Exception)
            {
                message = "Update data failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

           
            MsUser data = await IUsers.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            await DetailValidation(dataVM);

            if (ModelState.IsValid)
            {
                dataVM.UserEmail = Utilities.ToLower(dataVM.UserEmail);
                dataVM.FullName = Utilities.UpperFirstCase(dataVM.FullName);

                data.UserEmail = dataVM.UserEmail;
                data.FullName = dataVM.FullName;
                data.DefaultWarehouseId = dataVM.DefaultWarehouseId;
                data.RoleId = dataVM.RoleId;
                //data.CompanyId = dataVM.CompanyId;
                data.IsActive = dataVM.IsActive;
                data.ModifiedBy = Session["username"].ToString();

                List<MsWarehouseAccess> prevAccess = data.MsWarehouseAccesses.ToList();
                string[] PrevWarehouseIds = prevAccess.Select(m => m.WarehouseId).ToArray();

                List<MsWarehouseAccess> warehouseAccesses = new List<MsWarehouseAccess>();
                if (dataVM.WarehouseIds != null && dataVM.WarehouseIds.Count() > 0)
                {
                    foreach (string warehouseId in dataVM.WarehouseIds)
                    {
                        MsWarehouseAccess warehouseAccess = new MsWarehouseAccess();
                        if (!PrevWarehouseIds.Contains(warehouseId))
                        {
                            warehouseAccess.Username = data.Username;
                            warehouseAccess.WarehouseId = warehouseId;
                        }
                        else
                        {
                            warehouseAccess = prevAccess.Where(m => m.WarehouseId.Equals(warehouseId)).FirstOrDefault();
                        }
                        warehouseAccesses.Add(warehouseAccess);
                    }
                }

                data.MsWarehouseAccesses = warehouseAccesses.ToList();

                result = await IUsers.UpdateAsync(data);

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
