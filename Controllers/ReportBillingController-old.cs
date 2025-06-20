//using Facelift_App.Helper;
//using Facelift_App.Models;
//using Facelift_App.Services;
//using OfficeOpenXml;
//using OfficeOpenXml.Style;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace Facelift_App.Controllers
//{
//    [SessionCheck]
//    [AuthCheck]
//    public class ReportBillingController : Controller
//    {
//        private readonly IBillings IBillings;

//        public ReportBillingController(IBillings Billings)
//        {
//            IBillings = Billings;
//            ViewBag.WarehouseDropdown = true;
//        }

//        // GET: ReportBilling
//        public ActionResult Index()
//        {
//            return View();
//        }

//        [HttpPost]
//        public ActionResult DatatableUsed()
//        {
//            string warehouseId = Session["warehouseAccess"].ToString();
//            int draw = Convert.ToInt32(Request["draw"]);
//            int start = Convert.ToInt32(Request["start"]);
//            int length = Convert.ToInt32(Request["length"]);
//            string search = Request["search[value]"];
//            string orderCol = Request["order[0][column]"];
//            string sortName = Request["columns[" + orderCol + "][name]"];
//            string sortDirection = Request["order[0][dir]"];

//            string currentMonth = Request["month"];
//            string currentYear = Request["year"];

//            int month = string.IsNullOrEmpty(currentMonth) ? DateTime.Now.Month : Convert.ToInt32(currentMonth);
//            int year = string.IsNullOrEmpty(currentYear) ? DateTime.Now.Year : Convert.ToInt32(currentYear);

//            string agingType = Constant.AgingType.USED.ToString();

//            IEnumerable<VwPalletBilling> list = IBillings.GetBilling(warehouseId, agingType, month, year, search, sortDirection, sortName);
//            IEnumerable<PalletBillingDTO> pagedData = Enumerable.Empty<PalletBillingDTO>();

//            int recordsTotal = IBillings.GetTotalBilling(warehouseId, agingType, month, year);
//            int recordsFilteredTotal = list.Count();
//            double? total = IBillings.GetTotalPrice(warehouseId, agingType, month, year);
//            string totalPrice = total != null && total > 0 ? Utilities.FormatDoubleToThousand(total) : "0";


//            list = list.Skip(start).Take(length).ToList();


//            //re-format
//            if (list != null && list.Count() > 0)
//            {
//                pagedData = from x in list
//                            select new PalletBillingDTO
//                            {
//                                AgingId = x.AgingId,
//                                PalletId = x.PalletId,
//                                ReceivedAt = Utilities.NullDateTimeToString(x.ReceivedAt),
//                                CurrentMonth = x.CurrentMonth.ToString(),
//                                CurrentYear = x.CurrentYear.ToString(),
//                                PalletName = x.PalletName,
//                                WarehouseCode = x.WarehouseCode,
//                                WarehouseName = x.WarehouseName,
//                                PalletOwner = x.PalletOwner,
//                                PalletProducer = x.PalletProducer,
//                                WarehouseId = x.WarehouseId,
//                                PalletStatus = Utilities.RentStatusBadge(x.PalletStatus),
//                                TotalMinutes = Utilities.FormatDoubleToThousand(x.TotalMinutes),
//                                TotalHours = Utilities.FormatDoubleToThousand(x.TotalHours),
//                                TotalDays = Utilities.FormatDoubleToThousand(x.TotalDays),
//                                PalletAgeMonth = x.PalletAgeMonth.ToString(),
//                                PalletAgeYear = x.PalletAgeYear.ToString(),
//                                BillingPrice = Utilities.FormatDecimalToThousand(x.BillingPrice),
//                                TotalBilling = Utilities.FormatDoubleToThousand(x.TotalBilling)
//                            };
//            }

//            string generatedDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");

//            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData, month = month, year = year, generatedDate = generatedDate, totalPrice = totalPrice },
//                            JsonRequestBehavior.AllowGet);
//        }

//        [HttpPost]
//        public ActionResult DatatableUnused()
//        {
//            string warehouseId = Session["warehouseAccess"].ToString();
//            int draw = Convert.ToInt32(Request["draw"]);
//            int start = Convert.ToInt32(Request["start"]);
//            int length = Convert.ToInt32(Request["length"]);
//            string search = Request["search[value]"];
//            string orderCol = Request["order[0][column]"];
//            string sortName = Request["columns[" + orderCol + "][name]"];
//            string sortDirection = Request["order[0][dir]"];

//            string currentMonth = Request["month"];
//            string currentYear = Request["year"];

//            int month = string.IsNullOrEmpty(currentMonth) ? DateTime.Now.Month : Convert.ToInt32(currentMonth);
//            int year = string.IsNullOrEmpty(currentYear) ? DateTime.Now.Year : Convert.ToInt32(currentYear);

//            string agingType = Constant.AgingType.UNUSED.ToString();

//            IEnumerable<VwPalletBilling> list = IBillings.GetBilling(warehouseId, agingType, month, year, search, sortDirection, sortName);
//            IEnumerable<PalletBillingDTO> pagedData = Enumerable.Empty<PalletBillingDTO>();

//            int recordsTotal = IBillings.GetTotalBilling(warehouseId, agingType, month, year);
//            int recordsFilteredTotal = list.Count();
//            double? total = IBillings.GetTotalPrice(warehouseId, agingType, month, year);
//            string totalPrice = total != null && total > 0 ? Utilities.FormatDoubleToThousand(total) : "0";


//            list = list.Skip(start).Take(length).ToList();


//            //re-format
//            if (list != null && list.Count() > 0)
//            {
//                pagedData = from x in list
//                            select new PalletBillingDTO
//                            {
//                                AgingId = x.AgingId,
//                                PalletId = x.PalletId,
//                                ReceivedAt = Utilities.NullDateTimeToString(x.ReceivedAt),
//                                CurrentMonth = x.CurrentMonth.ToString(),
//                                CurrentYear = x.CurrentYear.ToString(),
//                                PalletName = x.PalletName,
//                                WarehouseCode = x.WarehouseCode,
//                                WarehouseName = x.WarehouseName,
//                                PalletOwner = x.PalletOwner,
//                                PalletProducer = x.PalletProducer,
//                                WarehouseId = x.WarehouseId,
//                                PalletStatus = Utilities.RentStatusBadge(x.PalletStatus),
//                                TotalMinutes = Utilities.FormatDoubleToThousand(x.TotalMinutes),
//                                TotalHours = Utilities.FormatDoubleToThousand(x.TotalHours),
//                                TotalDays = Utilities.FormatDoubleToThousand(x.TotalDays),
//                                PalletAgeMonth = x.PalletAgeMonth.ToString(),
//                                PalletAgeYear = x.PalletAgeYear.ToString(),
//                                BillingPrice = Utilities.FormatDecimalToThousand(x.BillingPrice),
//                                TotalBilling = Utilities.FormatDoubleToThousand(x.TotalBilling)
//                            };
//            }

//            string generatedDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");

//            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData, month = month, year = year, generatedDate = generatedDate, totalPrice = totalPrice },
//                            JsonRequestBehavior.AllowGet);
//        }
//    }
//}