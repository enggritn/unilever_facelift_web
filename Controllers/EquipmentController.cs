using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using Facelift_App.Models;
using Facelift_App.Helper;
using Facelift_App.Services;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class EquipmentController : Controller
    {
        private readonly IEquipment IEquipment;
        private FaceliftEntities db = new FaceliftEntities();

        public EquipmentController(IEquipment _IEquipment)
        {
            IEquipment = _IEquipment;
        }
        // GET: Warehouses
        public ActionResult Index()
        {
            ViewBag.Title = "Equipment Configuration - List";
            return View();
        }

        public async Task<ActionResult> Detail(string x)
        {
            ViewBag.Title = "Equipment Configuration - Detail";
            if (String.IsNullOrEmpty(x))
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return RedirectToAction("Index");
            }
            string id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
            MsEquipment data = await IEquipment.GetDataByIdAsync(id);
            if (data == null)
            {
                return HttpNotFound();
            }

            EquipmentVM dataVM = new EquipmentVM
            {
                EquipmentId = data.EquipmentId,
                Description = data.Description,
                Location = data.Location,
                Ip = data.Ip,
                port = data.port,
                Receive = data.Receive,
                Ship = data.Ship,
                IsActive = data.IsActive
            };
            ViewBag.Id = x;
            return View(dataVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Detail(string x, EquipmentVM dataVM)
        {
            
            bool result = false;
            string message = "";
            if (ModelState.IsValid)
            {
              // dataVM.Email = dataVM.Email.ToLower().Trim();
                string id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                MsEquipment data = await IEquipment.GetDataByIdAsync(id);
                if (data != null)
                {
                   // data.EquipmentId = dataVM.EquipmentId;
                    data.Description = dataVM.Description;
                    data.Location = dataVM.Location;
                    data.Ip = dataVM.Ip;
                    data.port = dataVM.port;
                    data.IsActive = dataVM.IsActive;
                    data.Receive = dataVM.Receive;
                    data.Ship = dataVM.Ship;
                    data.ModifiedBy = "System";

                    result = await IEquipment.UpdateAsync(data);

                    if (result)
                    {
                        message = "Update data succeeded.";
                    }
                    else
                    {
                        message = "Update data failed. Please contact system administrator.";
                    }

                }
                else
                {
                    message = "Data not found.";
                }
            }
            return Json(new { stat = result, msg = message });
        }
        public ActionResult Create()
        {
            ViewBag.Title = "User Configuration - Create";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MsEquipment dataVM)
        {
            //ModelState["MemberName"].Errors.Clear();
            bool result = false;
            string message = "";
            if (ModelState.IsValid)
            {
                //dataVM.Email = dataVM.Email.ToLower().Trim();


                MsEquipment data = new MsEquipment
                {
                    EquipmentId = CreateGuid("E"),
                    Description = dataVM.Description,
                    Location = dataVM.Location,
                    Ip = dataVM.Ip,
                    port = dataVM.port,
                    Receive = dataVM.Receive,
                    Ship = dataVM.Ship,
                    IsActive = true,
                    CreatedBy = "System"
                };

                result = await IEquipment.InsertAsync(data);
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

            IEnumerable<MsEquipment> list = IEquipment.GetFilteredData(search, sortDirection, sortName);
            IEnumerable<EquipmentDTO> pagedData = Enumerable.Empty<EquipmentDTO>(); ;

            int recordsTotal = IEquipment.GetTotalData();
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                list = list.Select(x => new MsEquipment
                {
                    EquipmentId = x.EquipmentId,
                    Description = x.Description,
                    Location = x.Location,
                    Ip = x.Ip,
                    port = x.port,
                    Receive = x.Receive,
                    Ship = x.Ship,
                    IsActive = x.IsActive,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedAt = x.ModifiedAt
                });

                pagedData = from x in list
                            select new EquipmentDTO
                            {
                                EquipmentId = Utilities.EncodeTo64(Encryptor.Encrypt(x.EquipmentId, Constant.facelift_encryption_key)),
                                Description = x.Description,
                                Location = x.Location,
                                Ip = x.Ip,
                                port = x.port,
                                Receive = x.Receive,
                                Ship = x.Ship,
                                IsActive = x.IsActive,
                                CreatedBy = x.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                ModifiedBy = !String.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt)
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }

        public static string CreateGuid(string prefix)
        {
            return string.Format("{0}-{1:N}", prefix, Guid.NewGuid());
        }
    }
}
