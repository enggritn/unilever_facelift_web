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
    public class PalletTypeController : Controller
    {
        private readonly IPalletTypes IPalletTypes;


        public PalletTypeController(IPalletTypes PalletTypes)
        {
            IPalletTypes = PalletTypes;
        }
        // GET: PalletTypes
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        private async Task CreateValidation(PalletTypeVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.PalletName))
            {
                PalletTypeValidator validator = new PalletTypeValidator(IPalletTypes);
                string PalletValid = await validator.IsUniqueName(dataVM.PalletName, "");
                if (!PalletValid.Equals("true"))
                {
                    ModelState.AddModelError("PalletName", PalletValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(PalletTypeVM dataVM)
        {
            //ModelState["MemberName"].Errors.Clear();
            bool result = false;
            string message = "Invalid form submission.";

            //server validation
            await CreateValidation(dataVM);

            if (ModelState.IsValid)
            {
                dataVM.PalletName = Utilities.ToUpper(dataVM.PalletName);
                dataVM.PalletDescription = !string.IsNullOrEmpty(dataVM.PalletDescription) ? dataVM.PalletDescription.Trim() : "";


                MsPalletType data = new MsPalletType
                {
                    PalletTypeId = Utilities.CreateGuid("PT"),
                    PalletName = dataVM.PalletName,
                    PalletDescription = dataVM.PalletDescription,
                    IsActive = true,
                    CreatedBy = Session["username"].ToString()
                };

                result = await IPalletTypes.InsertAsync(data);
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
            MsPalletType data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await IPalletTypes.GetDataByIdAsync(id);
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

            PalletTypeVM dataVM = new PalletTypeVM
            {
                PalletName = data.PalletName,
                PalletDescription = data.PalletDescription,
                IsActive = data.IsActive
            };
            ViewBag.Id = x;
            return View(dataVM);
        }

        private async Task DetailValidation(PalletTypeVM dataVM, string id)
        {
            if (!string.IsNullOrEmpty(dataVM.PalletName))
            {
                PalletTypeValidator validator = new PalletTypeValidator(IPalletTypes);
                string PalletValid = await validator.IsUniqueName(dataVM.PalletName, id);
                if (!PalletValid.Equals("true"))
                {
                    ModelState.AddModelError("PalletName", PalletValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, PalletTypeVM dataVM)
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

            MsPalletType data = await IPalletTypes.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            await DetailValidation(dataVM, id);

            if (ModelState.IsValid)
            {

                dataVM.PalletName = Utilities.ToUpper(dataVM.PalletName);
                dataVM.PalletDescription = !string.IsNullOrEmpty(dataVM.PalletDescription) ? dataVM.PalletDescription.Trim() : "";

                data.PalletName = dataVM.PalletName;
                data.PalletDescription = dataVM.PalletDescription;
                data.IsActive = dataVM.IsActive;
                data.ModifiedBy = Session["username"].ToString();

                result = await IPalletTypes.UpdateAsync(data);

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

            IEnumerable<MsPalletType> list = IPalletTypes.GetFilteredData(search, sortDirection, sortName);
            IEnumerable<PalletTypeDTO> pagedData = Enumerable.Empty<PalletTypeDTO>();

            int recordsTotal = IPalletTypes.GetTotalData();
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new PalletTypeDTO
                            {
                                PalletTypeId = Utilities.EncodeTo64(Encryptor.Encrypt(x.PalletTypeId, Constant.facelift_encryption_key)),
                                PalletName = x.PalletName,
                                PalletDescription = !string.IsNullOrEmpty(x.PalletDescription) ? x.PalletDescription : "-",
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