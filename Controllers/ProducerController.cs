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
    public class ProducerController : Controller
    {
        private readonly IPalletProducers IPalletProducers;


        public ProducerController(IPalletProducers Producers)
        {
            IPalletProducers = Producers;
        }
        // GET: Producers
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        private async Task CreateValidation(ProducerVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.ProducerName))
            {
                ProducerValidator validator = new ProducerValidator(IPalletProducers);
                string IsValid = await validator.IsUniqueName(dataVM.ProducerName, "");
                if (!IsValid.Equals("true"))
                {
                    ModelState.AddModelError("ProducerName", IsValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(ProducerVM dataVM)
        {
            //ModelState["MemberName"].Errors.Clear();
            bool result = false;
            string message = "Invalid form submission.";

            //server validation
            await CreateValidation(dataVM);

            if (ModelState.IsValid)
            {
                dataVM.ProducerName = Utilities.ToUpper(dataVM.ProducerName);


                MsProducer data = new MsProducer
                {
                    ProducerName = dataVM.ProducerName,
                    IsActive = true,
                    CreatedBy = Session["username"].ToString()
                };

                result = await IPalletProducers.InsertAsync(data);
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
            MsProducer data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await IPalletProducers.GetDataByIdAsync(id);
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

            ProducerVM dataVM = new ProducerVM
            {
                ProducerName = data.ProducerName,
                IsActive = data.IsActive
            };
            ViewBag.Id = x;
            return View(dataVM);
        }

        private async Task DetailValidation(ProducerVM dataVM, string id)
        {
            if (!string.IsNullOrEmpty(dataVM.ProducerName))
            {
                ProducerValidator validator = new ProducerValidator(IPalletProducers);
                string IsValid = await validator.IsUniqueName(dataVM.ProducerName, id);
                if (!IsValid.Equals("true"))
                {
                    ModelState.AddModelError("ProducerName", IsValid);
                }
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, ProducerVM dataVM)
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

            MsProducer data = await IPalletProducers.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }

            await DetailValidation(dataVM, id);

            if (ModelState.IsValid)
            {

                dataVM.ProducerName = Utilities.ToUpper(dataVM.ProducerName);

                data.ProducerName = dataVM.ProducerName;
                data.IsActive = dataVM.IsActive;
                data.ModifiedBy = Session["username"].ToString();

                result = await IPalletProducers.UpdateAsync(data);

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

            IEnumerable<MsProducer> list = IPalletProducers.GetFilteredData(search, sortDirection, sortName);
            IEnumerable<ProducerDTO> pagedData = Enumerable.Empty<ProducerDTO>();

            int recordsTotal = IPalletProducers.GetTotalData();
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new ProducerDTO
                            {
                                ProducerId = Utilities.EncodeTo64(Encryptor.Encrypt(x.ProducerName, Constant.facelift_encryption_key)),
                                ProducerName = x.ProducerName,
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