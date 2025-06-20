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
    public class WarehouseController : Controller
    {
        private readonly IWarehouses IWarehouses;
        private readonly IUsers IUsers;
        private readonly IWarehouseCategories IWarehouseCategories;
        private readonly ICompanies ICompanies;


        public WarehouseController(IWarehouses Warehouses, IUsers Users, IWarehouseCategories WarehouseCategories, ICompanies Companies)
        {
            IWarehouses = Warehouses;
            IUsers = Users;
            IWarehouseCategories = WarehouseCategories;
            ICompanies = Companies;
        }
        // GET: Warehouses
        public ActionResult Index()
        {
            return View();
        }


        public async Task<ActionResult> Detail(string x)
        {
            string id = "";
            MsWarehouse data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await IWarehouses.GetDataByIdAsync(id);
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

            WarehouseVM dataVM = new WarehouseVM
            {
                WarehouseCode = data.WarehouseCode,
                WarehouseName = data.WarehouseName,
                WarehouseAlias = data.WarehouseAlias,
                Address = data.Address,
                PIC1 = data.PIC1,
                PIC2 = data.PIC2,
                Phone = data.Phone,
                MaxCapacity = data.MaxCapacity,
                CategoryId = data.CategoryId.ToString(),
                IsActive = data.IsActive
            };

            dataVM.CompanyIds = data.MsWarehouseCompanies.Select(m => m.CompanyId).ToArray();

            dataVM.CompanyList = await ICompanies.GetAllAsync();

            ViewBag.Id = x;
            IEnumerable<MsUser> users = await IUsers.GetListAsync();
            ViewBag.UserList = new SelectList(users, "Username", "Username");

            ViewBag.CategoryList = new SelectList(await IWarehouseCategories.GetAllAsync(), "CategoryId", "CategoryName");

            return View(dataVM);
        }

        private async Task DetailValidation(WarehouseVM dataVM, string id)
        {
            if (!string.IsNullOrEmpty(dataVM.WarehouseName))
            {
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string WarehouseValid = await validator.IsUniqueName(dataVM.WarehouseName, id);
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("WarehouseName", WarehouseValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.WarehouseAlias))
            {
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string WarehouseValid = await validator.IsUniqueAlias(dataVM.WarehouseAlias, id);
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("WarehouseAlias", WarehouseValid);
                }
            }


            if (!string.IsNullOrEmpty(dataVM.PIC1))
            {
                UserValidator validator = new UserValidator(IUsers);
                string UserValid = await validator.IsUserExist(dataVM.PIC1);
                if (!UserValid.Equals("true"))
                {
                    ModelState.AddModelError("PIC1", UserValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.PIC2))
            {
                UserValidator validator = new UserValidator(IUsers);
                string UserValid = await validator.IsUserExist(dataVM.PIC2);
                if (!UserValid.Equals("true"))
                {
                    ModelState.AddModelError("PIC2", UserValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.CategoryId))
            {
                WarehouseCategoryValidator validator = new WarehouseCategoryValidator(IWarehouseCategories);
                string IsValid = await validator.IsExist(dataVM.CategoryId);
                if (!IsValid.Equals("true"))
                {
                    ModelState.AddModelError("CategoryId", IsValid);
                }
            }

            //if (dataVM.PalletUsedTarget > 0)
            //{
            //    WarehouseValidator validator = new WarehouseValidator(IWarehouses);
            //    string WeightValid = await validator.IsWeightAllowed(dataVM.PalletUsedTarget, id);
            //    if (!WeightValid.Equals("true"))
            //    {
            //        ModelState.AddModelError("PalletUsedTarget", WeightValid);
            //    }
            //}
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, WarehouseVM dataVM)
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

            MsWarehouse data = await IWarehouses.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            await DetailValidation(dataVM, id);

            if (ModelState.IsValid)
            {
                dataVM.WarehouseName = Utilities.ToUpper(dataVM.WarehouseName);
                //dataVM.WarehouseAlias = Utilities.ToUpper(dataVM.WarehouseAlias);
                dataVM.Address = !string.IsNullOrEmpty(dataVM.Address) ? dataVM.Address.Trim() : "";

                data.WarehouseName = dataVM.WarehouseName;
                data.Address = dataVM.Address;
                data.Phone = dataVM.Phone;
                data.PIC1 = dataVM.PIC1;
                data.PIC2 = dataVM.PIC2;
                data.IsActive = dataVM.IsActive;
                data.MaxCapacity = dataVM.MaxCapacity;
                data.CategoryId = Convert.ToInt32(dataVM.CategoryId);
                data.ModifiedBy = Session["username"].ToString();

                List<MsWarehouseCompany> prevWhCompanies = data.MsWarehouseCompanies.ToList();
                string[] PrevCompanyIds = prevWhCompanies.Select(m => m.CompanyId).ToArray();

                List<MsWarehouseCompany> warehouseCompanies = new List<MsWarehouseCompany>();
                if (dataVM.CompanyIds != null && dataVM.CompanyIds.Count() > 0)
                {
                    foreach (string companyId in dataVM.CompanyIds)
                    {
                        MsWarehouseCompany warehouseCompany = new MsWarehouseCompany();
                        if (!PrevCompanyIds.Contains(companyId))
                        {
                            warehouseCompany.WarehouseId = data.WarehouseId;
                            warehouseCompany.CompanyId = companyId;
                        }
                        else
                        {
                            warehouseCompany = prevWhCompanies.Where(m => m.CompanyId.Equals(companyId)).FirstOrDefault();
                        }
                        warehouseCompanies.Add(warehouseCompany);
                    }
                }

               data.MsWarehouseCompanies = warehouseCompanies.ToList();

                result = await IWarehouses.UpdateAsync(data);

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
        public async Task<ActionResult> Create()
        {
            IEnumerable<MsUser> users = await IUsers.GetListAsync();
            ViewBag.UserList = new SelectList(users, "Username", "Username");

            ViewBag.CategoryList = new SelectList(await IWarehouseCategories.GetAllAsync(), "CategoryId", "CategoryName");

            WarehouseVM dataVM = new WarehouseVM();
            dataVM.CompanyList = await ICompanies.GetAllAsync();

            return View(dataVM);
        }

        private async Task CreateValidation(WarehouseVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.WarehouseName))
            {
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string WarehouseValid = await validator.IsUniqueName(dataVM.WarehouseName, "");
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("WarehouseName", WarehouseValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.WarehouseAlias))
            {
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string WarehouseValid = await validator.IsUniqueAlias(dataVM.WarehouseAlias, "");
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("WarehouseAlias", WarehouseValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.PIC1))
            {
                UserValidator validator = new UserValidator(IUsers);
                string UserValid = await validator.IsUserExist(dataVM.PIC1);
                if (!UserValid.Equals("true"))
                {
                    ModelState.AddModelError("PIC1", UserValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.PIC2))
            {
                UserValidator validator = new UserValidator(IUsers);
                string UserValid = await validator.IsUserExist(dataVM.PIC2);
                if (!UserValid.Equals("true"))
                {
                    ModelState.AddModelError("PIC2", UserValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.CategoryId))
            {
                WarehouseCategoryValidator validator = new WarehouseCategoryValidator(IWarehouseCategories);
                string IsValid = await validator.IsExist(dataVM.CategoryId);
                if (!IsValid.Equals("true"))
                {
                    ModelState.AddModelError("CategoryId", IsValid);
                }
            }

            //if (dataVM.PalletUsedTarget > 0)
            //{
            //    WarehouseValidator validator = new WarehouseValidator(IWarehouses);
            //    string WeightValid = await validator.IsWeightAllowed(dataVM.PalletUsedTarget, "");
            //    if (!WeightValid.Equals("true"))
            //    {
            //        ModelState.AddModelError("PalletUsedTarget", WeightValid);
            //    }
            //}
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(WarehouseVM dataVM)
        {
            //ModelState["MemberName"].Errors.Clear();
            bool result = false;
            string message = "Invalid form submission.";

            //server validation
            await CreateValidation(dataVM);

            if (ModelState.IsValid)
            {
                dataVM.WarehouseName = Utilities.ToUpper(dataVM.WarehouseName);
                dataVM.WarehouseAlias = Utilities.ToUpper(dataVM.WarehouseAlias);
                dataVM.Address = !string.IsNullOrEmpty(dataVM.Address) ? dataVM.Address.Trim() : "";


                MsWarehouse data = new MsWarehouse
                {
                    WarehouseId = Utilities.CreateGuid("W"),
                    WarehouseName = dataVM.WarehouseName,
                    WarehouseAlias = dataVM.WarehouseAlias,
                    Address = dataVM.Address,
                    Phone = dataVM.Phone,
                    PIC1 = dataVM.PIC1,
                    PIC2 = dataVM.PIC2,
                    MaxCapacity = dataVM.MaxCapacity,
                    CategoryId = Convert.ToInt32(dataVM.CategoryId),
                    IsActive = true,
                    CreatedBy = Session["username"].ToString()
                };

                data.MsWarehouseCompanies = new List<MsWarehouseCompany>();
                if (dataVM.CompanyIds != null && dataVM.CompanyIds.Count() > 0)
                {
                    foreach (string companyId in dataVM.CompanyIds)
                    {
                        MsWarehouseCompany warehouseCompany = new MsWarehouseCompany
                        {
                            WarehouseId = data.WarehouseId,
                            CompanyId = companyId
                        };

                        data.MsWarehouseCompanies.Add(warehouseCompany);
                    }
                }

                result = await IWarehouses.InsertAsync(data);
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

            IEnumerable<MsWarehouse> list = IWarehouses.GetFilteredData(search, sortDirection, sortName);
            IEnumerable<WarehouseDTO> pagedData = Enumerable.Empty<WarehouseDTO>(); ;

            int recordsTotal = IWarehouses.GetTotalData();
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new WarehouseDTO
                            {
                                WarehouseId = Utilities.EncodeTo64(Encryptor.Encrypt(x.WarehouseId, Constant.facelift_encryption_key)),
                                WarehouseCode = x.WarehouseCode,
                                WarehouseName = x.WarehouseName,
                                WarehouseAlias = x.WarehouseAlias,
                                CategoryName = x.MsWarehouseCategory.CategoryName,
                                Address = !string.IsNullOrEmpty(x.Address) ? x.Address : "-",
                                Phone = !string.IsNullOrEmpty(x.Phone) ? x.Phone : "-",
                                PIC1 = x.PIC1,
                                PIC2 = x.PIC2,
                                IsActive = Utilities.IsActiveStatusBadge(x.IsActive),
                                MaxCapacity = x.MaxCapacity.ToString(),
                                CreatedBy = x.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt)
                            };
            }

            //int totalWeight = IWarehouses.GetTotalWeight();

            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData},
                            JsonRequestBehavior.AllowGet);
        }
    }
}
