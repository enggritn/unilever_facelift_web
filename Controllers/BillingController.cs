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
    [AuthCheck]
    public class BillingController : Controller
    {
        private readonly IBillings IBillings;


        public BillingController(IBillings Billings)
        {
            IBillings = Billings;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        private async Task CreateValidation(BillingConfigurationVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.BillingYear))
            {
                BillingConfigurationValidator validator = new BillingConfigurationValidator(IBillings);
                string BillingValid = await validator.IsUniqueYear(Convert.ToInt32(dataVM.BillingYear), "");
                if (!BillingValid.Equals("true"))
                {
                    ModelState.AddModelError("BillingYear", BillingValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(BillingConfigurationVM dataVM)
        {
            //ModelState["MemberName"].Errors.Clear();
            bool result = false;
            string message = "Invalid form submission.";

            //server validation
            await CreateValidation(dataVM);

            if (ModelState.IsValid)
            {


                MsBillingConfiguration data = new MsBillingConfiguration
                {
                    BillingId = Utilities.CreateGuid("BIL"),
                    BillingYear = Convert.ToInt32(dataVM.BillingYear),
                    BillingPrice = Convert.ToDecimal(dataVM.BillingPrice),
                    IsActive = true,
                    CreatedBy = Session["username"].ToString()
                };

                result = await IBillings.InsertAsync(data);
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
            MsBillingConfiguration data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await IBillings.GetDataByIdAsync(id);
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

            BillingConfigurationVM dataVM = new BillingConfigurationVM
            {
                BillingYear = data.BillingYear.ToString(),
                BillingPrice = Utilities.FormatDecimalToThousand(data.BillingPrice),
                IsActive = data.IsActive
            };
            ViewBag.Id = x;
            return View(dataVM);
        }

        private async Task DetailValidation(BillingConfigurationVM dataVM, string id)
        {
            if (!string.IsNullOrEmpty(dataVM.BillingYear))
            {
                BillingConfigurationValidator validator = new BillingConfigurationValidator(IBillings);
                string BillingValid = await validator.IsUniqueYear(Convert.ToInt32(dataVM.BillingYear), id);
                if (!BillingValid.Equals("true"))
                {
                    ModelState.AddModelError("BillingYear", BillingValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, BillingConfigurationVM dataVM)
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

            MsBillingConfiguration data = await IBillings.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            await DetailValidation(dataVM, id);

            if (ModelState.IsValid)
            {

                data.BillingYear = Convert.ToInt32(dataVM.BillingYear);
                data.BillingPrice = Convert.ToDecimal(dataVM.BillingPrice);
                data.IsActive = dataVM.IsActive;
                data.ModifiedBy = Session["username"].ToString();

                result = await IBillings.UpdateAsync(data);

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

            IEnumerable<MsBillingConfiguration> list = IBillings.GetFilteredData(search, sortDirection, sortName);
            IEnumerable<BillingConfigurationDTO> pagedData = Enumerable.Empty<BillingConfigurationDTO>();

            int recordsTotal = IBillings.GetTotalData();
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new BillingConfigurationDTO
                            {
                                BillingId = Utilities.EncodeTo64(Encryptor.Encrypt(x.BillingId, Constant.facelift_encryption_key)),
                                BillingYear = x.BillingYear.ToString(),
                                BillingPrice = Utilities.FormatDecimalToThousand(x.BillingPrice),
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