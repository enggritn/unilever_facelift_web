using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Facelift_App.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class CompanyController : Controller
    {
        private readonly ICompanies ICompanies;


        public CompanyController(ICompanies Companies)
        {
            ICompanies = Companies;
        }
        // GET: Companies
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        private async Task CreateValidation(CompanyVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.CompanyName))
            {
                CompanyValidator validator = new CompanyValidator(ICompanies);
                string IsValid = await validator.IsUniqueName(dataVM.CompanyName, "");
                if (!IsValid.Equals("true"))
                {
                    ModelState.AddModelError("CompanyName", IsValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.CompanyAbb))
            {
                CompanyValidator validator = new CompanyValidator(ICompanies);
                string IsValid = await validator.IsUniqueAbb(dataVM.CompanyAbb, "");
                if (!IsValid.Equals("true"))
                {
                    ModelState.AddModelError("CompanyAbb", IsValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(CompanyVM dataVM)
        {
            //ModelState["MemberName"].Errors.Clear();
            bool result = false;
            string message = "Invalid form submission.";

            //server validation
            await CreateValidation(dataVM);

            if (ModelState.IsValid)
            {
                dataVM.CompanyName = Utilities.ToUpper(dataVM.CompanyName);
                dataVM.CompanyAbb = Utilities.ToUpper(dataVM.CompanyAbb);


                MsCompany data = new MsCompany
                {
                    CompanyId = Utilities.CreateGuid("COM"),
                    CompanyName = dataVM.CompanyName,
                    CompanyAbb = dataVM.CompanyAbb,
                    IsActive = true,
                    CreatedBy = Session["username"].ToString()
                };

                result = await ICompanies.InsertAsync(data);
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
            MsCompany data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await ICompanies.GetDataByIdAsync(id);
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

            CompanyVM dataVM = new CompanyVM
            {
                CompanyName = data.CompanyName,
                CompanyAbb = data.CompanyAbb,
                IsActive = data.IsActive
            };
            ViewBag.Id = x;
            return View(dataVM);
        }

        private async Task DetailValidation(CompanyVM dataVM, string id)
        {
            if (!string.IsNullOrEmpty(dataVM.CompanyName))
            {
                CompanyValidator validator = new CompanyValidator(ICompanies);
                string IsValid = await validator.IsUniqueName(dataVM.CompanyName, id);
                if (!IsValid.Equals("true"))
                {
                    ModelState.AddModelError("CompanyName", IsValid);
                }
            }

            if (!string.IsNullOrEmpty(dataVM.CompanyAbb))
            {
                CompanyValidator validator = new CompanyValidator(ICompanies);
                string IsValid = await validator.IsUniqueAbb(dataVM.CompanyAbb, id);
                if (!IsValid.Equals("true"))
                {
                    ModelState.AddModelError("CompanyAbb", IsValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, CompanyVM dataVM)
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

            MsCompany data = await ICompanies.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            await DetailValidation(dataVM, id);

            if (ModelState.IsValid)
            {

                dataVM.CompanyName = Utilities.ToUpper(dataVM.CompanyName);
                dataVM.CompanyAbb = Utilities.ToUpper(dataVM.CompanyAbb);

                data.CompanyName = dataVM.CompanyName;
                data.CompanyAbb = dataVM.CompanyAbb;
                data.IsActive = dataVM.IsActive;
                data.ModifiedBy = Session["username"].ToString();

                result = await ICompanies.UpdateAsync(data);

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

            IEnumerable<MsCompany> list = ICompanies.GetFilteredData(search, sortDirection, sortName);
            IEnumerable<CompanyDTO> pagedData = Enumerable.Empty<CompanyDTO>();

            int recordsTotal = ICompanies.GetTotalData();
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new CompanyDTO
                            {
                                CompanyId = Utilities.EncodeTo64(Encryptor.Encrypt(x.CompanyId, Constant.facelift_encryption_key)),
                                CompanyName = x.CompanyName,
                                CompanyAbb = x.CompanyAbb,
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
    }
}