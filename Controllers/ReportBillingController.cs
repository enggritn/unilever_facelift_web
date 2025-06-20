using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using System.Configuration;
using System.Data.Entity;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class ReportBillingController : Controller
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public ReportBillingController()
        {
            ViewBag.WarehouseDropdown = true;
        }


        // GET: ReportBilling
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DatatableRent()
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            string currentMonth = Request["month"];
            string currentYear = Request["year"];

            int month = string.IsNullOrEmpty(currentMonth) ? DateTime.Now.Month : Convert.ToInt32(currentMonth);
            int year = string.IsNullOrEmpty(currentYear) ? DateTime.Now.Year : Convert.ToInt32(currentYear);

            IEnumerable<VwBillingRentDetail> list = Enumerable.Empty<VwBillingRentDetail>();
            IQueryable<VwBillingRentDetail> query = null;
            IEnumerable<PalletBillingDTO> pagedData = Enumerable.Empty<PalletBillingDTO>();

            int recordsTotal = 0;
            int recordsFilteredTotal = 0;

            string totalPrice = "";
            TrxInvoiceRentHeader invoice = db.TrxInvoiceRentHeaders.Where(m => m.CurrentMonth.Equals(month) && m.CurrentYear.Equals(year)).FirstOrDefault();
            string invoiceStatus = Utilities.InvoiceStatusBadge(invoice != null);
            try
            {
                query = db.VwBillingRentDetails.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId) && x.CurrentMonth == month && x.CurrentYear == year);

                double? total = query.Sum(i => i.TotalBilling);
                totalPrice = total != null && total > 0 ? Utilities.FormatDoubleToThousand(total) : "0";

                recordsTotal = query.Count();

                query = query
                        .Where(m => m.PalletId.Contains(search)
                        );

                //columns sorting
                Dictionary<string, Func<VwBillingRentDetail, object>> cols = new Dictionary<string, Func<VwBillingRentDetail, object>>();
                cols.Add("TagId", x => x.PalletId);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("TotalDays", x => x.TotalDays);
                cols.Add("PalletAgeMonth", x => x.PalletAgeMonth);
                cols.Add("PalletAgeYear", x => x.PalletAgeYear);
                cols.Add("BillingPrice", x => x.BillingPrice);
                cols.Add("TotalBilling", x => x.TotalBilling);
                cols.Add("PalletCondition", x => x.PalletCondition);
                cols.Add("LastTransactionDate", x => x.LastTransactionDate);
                cols.Add("CutOffAt", x => x.CutOffAt);



                if (sortDirection.Equals("asc"))
                    list = query.OrderBy(cols[sortName]);
                else
                    list = query.OrderByDescending(cols[sortName]);

                recordsFilteredTotal = list.Count();


                list = list.Skip(start).Take(length).ToList();


                //re-format
                if (list != null && list.Count() > 0)
                {
                    pagedData = from x in list
                                select new PalletBillingDTO
                                {
                                    AgingId = x.AgingId,
                                    PalletId = x.PalletId,
                                    CurrentMonth = x.CurrentMonth.ToString(),
                                    CurrentYear = x.CurrentYear.ToString(),
                                    PalletName = x.PalletName,
                                    WarehouseCode = x.WarehouseCode,
                                    WarehouseName = x.WarehouseName,
                                    PalletOwner = x.PalletOwner,
                                    PalletProducer = x.PalletProducer,
                                    WarehouseId = x.WarehouseId,
                                    TotalDays = Utilities.FormatDoubleToThousand(x.TotalDays),
                                    PalletAgeMonth = x.PalletAgeMonth.ToString(),
                                    PalletAgeYear = x.PalletAgeYear.ToString(),
                                    BillingPrice = Utilities.FormatDecimalToThousand(x.BillingPrice),
                                    TotalBilling = Utilities.FormatDoubleToThousand(x.TotalBilling),
                                    PalletCondition = x.PalletCondition,
                                    LastTransactionDate = Utilities.NullDateTimeToString(x.LastTransactionDate),
                                    CutOffAt = Utilities.NullDateTimeToString(x.CutOffAt)
                                };
                }

            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }


            string generatedDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");

            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData, month = month, year = year, generatedDate = generatedDate, totalPrice = totalPrice, invoiceStatus = invoiceStatus },
                            JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportExcelRent()
        {

            string currentMonth = Request["m"];
            string currentYear = Request["y"];

            int month = string.IsNullOrEmpty(currentMonth) ? DateTime.Now.Month : Convert.ToInt32(currentMonth);
            int year = string.IsNullOrEmpty(currentYear) ? DateTime.Now.Year : Convert.ToInt32(currentYear);

            string warehouseId = Session["warehouseAccess"].ToString();

            MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseId.Equals(warehouseId)).FirstOrDefault();

            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Billing_{0}_{1}_{2}_{3}.xlsx", warehouse.WarehouseCode, warehouse.WarehouseName,month, year);
            

            IEnumerable<VwBillingRentDetail> list = Enumerable.Empty<VwBillingRentDetail>();
            IQueryable<VwBillingRentDetail> query = null;
            IEnumerable<PalletBillingDTO> pagedData = Enumerable.Empty<PalletBillingDTO>();

            query = db.VwBillingRentDetails.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId) && x.CurrentMonth == month && x.CurrentYear == year);

            list = query.ToList();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Warehouse";
            workSheet.Cells[1, 2].Value = "Tag ID / Pallet ID";
            workSheet.Cells[1, 3].Value = "Status Pallet";
            workSheet.Cells[1, 4].Value = "Pallet Name";
            workSheet.Cells[1, 5].Value = "Pallet Age (Month)";
            workSheet.Cells[1, 6].Value = "Pallet Age (Year)";
            workSheet.Cells[1, 7].Value = "Last Transaction Date";
            workSheet.Cells[1, 8].Value = "Date Cut Off Time";
            workSheet.Cells[1, 9].Value = "Total Days";
            workSheet.Cells[1, 10].Value = "Price / Day";
            workSheet.Cells[1, 11].Value = "Total Price";

            int recordIndex = 2;
            foreach (VwBillingRentDetail item in list)
            {
                workSheet.Cells[recordIndex, 1].Value = item.WarehouseCode + " - " + item.WarehouseName;
                workSheet.Cells[recordIndex, 2].Value = item.PalletId;
                workSheet.Cells[recordIndex, 3].Value = item.PalletCondition;
                workSheet.Cells[recordIndex, 4].Value = item.PalletName;
                workSheet.Cells[recordIndex, 5].Value = item.PalletAgeMonth;
                workSheet.Cells[recordIndex, 6].Value = item.PalletAgeYear;
                workSheet.Cells[recordIndex, 7].Value = Utilities.NullDateTimeToString(item.LastTransactionDate);
                workSheet.Cells[recordIndex, 8].Value = Utilities.NullDateTimeToString(item.CutOffAt);
                workSheet.Cells[recordIndex, 9].Value = item.TotalDays;
                workSheet.Cells[recordIndex, 10].Value = item.BillingPrice;
                workSheet.Cells[recordIndex, 11].Value = item.TotalBilling;
                recordIndex++;
            }

            for (int i = 1; i <= 8; i++)
            {
                workSheet.Column(i).AutoFit();
            }

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;" + fileName);
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult DatatableDeffect()
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            string currentMonth = Request["month"];
            string currentYear = Request["year"];

            int month = string.IsNullOrEmpty(currentMonth) ? DateTime.Now.Month : Convert.ToInt32(currentMonth);
            int year = string.IsNullOrEmpty(currentYear) ? DateTime.Now.Year : Convert.ToInt32(currentYear);

            IEnumerable<MsPallet> list = Enumerable.Empty<MsPallet>();
            IQueryable<MsPallet> query = null;
            IEnumerable<PalletDTO> pagedData = Enumerable.Empty<PalletDTO>();

            int recordsTotal = 0;
            int recordsFilteredTotal = 0;

            string totalPrice = "";

            int totalPreviousDeffect = 0;
            int totalCurrentDeffect = 0;
            int totalPalletDeffect = 0;
            int totalPallet = 0;

            int treshold = Convert.ToInt32(ConfigurationManager.AppSettings["damage_loss_billing_treshold"].ToString());

            double price = Convert.ToDouble(ConfigurationManager.AppSettings["damage_loss_billing_unit_price"].ToString());

            string damagePrice = Utilities.FormatDoubleToThousand(price);

           

            string invoiceStatus = Utilities.InvoiceStatusBadge(false);

            try
            {
                query = db.MsPallets.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId) 
                && (x.PalletCondition.Equals(Constant.PalletCondition.DAMAGE.ToString()) || x.PalletCondition.Equals(Constant.PalletCondition.LOSS.ToString()))
                && (x.LastTransactionDate.Year == year && x.LastTransactionDate.Month == month));


                recordsTotal = query.Count();

            

                //columns sorting
                Dictionary<string, Func<MsPallet, object>> cols = new Dictionary<string, Func<MsPallet, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletCondition", x => x.PalletCondition);



                query = query
                        .Where(m => m.TagId.Contains(search)
                        );

                if (sortDirection.Equals("asc"))
                    list = query.OrderBy(cols[sortName]);
                else
                    list = query.OrderByDescending(cols[sortName]);

                recordsFilteredTotal = list.Count();


                list = list.Skip(start).Take(length).ToList();


                //re-format
                if (list != null && list.Count() > 0)
                {
                    pagedData = from x in list
                                select new PalletDTO
                                {
                                    TagId = x.TagId,
                                    PalletName = x.PalletName,
                                    PalletCondition = Utilities.PalletConditionBadge(x.PalletCondition)
                                };
                }


                totalCurrentDeffect = recordsTotal;

                DateTime selectedDate = new DateTime(year, month, 1);

                totalPreviousDeffect = db.MsPallets.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId)
                && (x.PalletCondition.Equals(Constant.PalletCondition.DAMAGE.ToString()) || x.PalletCondition.Equals(Constant.PalletCondition.LOSS.ToString()))
                && DbFunctions.TruncateTime(x.LastTransactionDate) < DbFunctions.TruncateTime(selectedDate)).Count();

                totalPalletDeffect = totalPreviousDeffect + totalCurrentDeffect;
                totalPallet = db.MsPallets.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId)).Count();


            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }


            string generatedDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");

            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData, invoiceStatus = invoiceStatus,
                month = month, year = year, generatedDate = generatedDate, totalPrice = totalPrice, totalCurrentDeffect = totalCurrentDeffect,
                totalPallet = totalPallet, totalPalletDeffect = totalPalletDeffect, totalPreviousDeffect = totalPreviousDeffect, treshold = treshold, price = damagePrice
            }, 
                            JsonRequestBehavior.AllowGet);
        }


        public ActionResult ExportExcelDeffect()
        {

            string currentMonth = Request["m"];
            string currentYear = Request["y"];

            int month = string.IsNullOrEmpty(currentMonth) ? DateTime.Now.Month : Convert.ToInt32(currentMonth);
            int year = string.IsNullOrEmpty(currentYear) ? DateTime.Now.Year : Convert.ToInt32(currentYear);

            string warehouseId = Session["warehouseAccess"].ToString();

            MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseId.Equals(warehouseId)).FirstOrDefault();

            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Billing_Damage_{0}_{1}_{2}_{3}.xlsx", warehouse.WarehouseCode, warehouse.WarehouseName, month, year);


            IEnumerable<MsPallet> list = Enumerable.Empty<MsPallet>();
            IQueryable<MsPallet> query = null;
            IEnumerable<PalletDTO> pagedData = Enumerable.Empty<PalletDTO>();

            query = db.MsPallets.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId)
                 && (x.PalletCondition.Equals(Constant.PalletCondition.DAMAGE.ToString()) || x.PalletCondition.Equals(Constant.PalletCondition.LOSS.ToString()))
                 && (x.LastTransactionDate.Year == year && x.LastTransactionDate.Month == month));

            list = query.ToList();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Warehouse";
            workSheet.Cells[1, 2].Value = "Tag ID / Pallet ID";
            workSheet.Cells[1, 3].Value = "Pallet Name";
            workSheet.Cells[1, 4].Value = "Pallet Condition";

            int recordIndex = 2;
            foreach (MsPallet item in list)
            {
                workSheet.Cells[recordIndex, 1].Value = item.WarehouseCode + " - " + item.WarehouseName;
                workSheet.Cells[recordIndex, 2].Value = item.TagId;
                workSheet.Cells[recordIndex, 3].Value = item.PalletName;
                workSheet.Cells[recordIndex, 4].Value = item.PalletCondition;
                recordIndex++;
            }

            for (int i = 1; i <= 4; i++)
            {
                workSheet.Column(i).AutoFit();
            }

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;" + fileName);
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            return RedirectToAction("Index");
        }

    }
}