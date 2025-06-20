using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class ReportPalletMovementController : Controller
    {
        private readonly IPallets IPallets;
        private readonly IWarehouses IWarehouses;

        public ReportPalletMovementController(IPallets Pallets, IWarehouses Warehouses)
        {
            IPallets = Pallets;
            IWarehouses = Warehouses;
            ViewBag.WarehouseDropdown = true;
        }

        // GET: ReportAccident
        public async Task<ActionResult> Index()
        {
            string warehouseAccess = Session["warehouseAccess"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult DatatablePalletMovement()
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            //string startDate = Request["startDate"];
            //string endDate = Request["endDate"];

            IEnumerable<VwPalletHistory> list = IPallets.GetPalletHistory(warehouseId, search, sortDirection, sortName);
            IEnumerable<PalletHistoryDTO> pagedData = Enumerable.Empty<PalletHistoryDTO>();

            int recordsTotal = IPallets.GetTotalPalletHistory(warehouseId);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new PalletHistoryDTO
                            {
                                TagId = x.TagId,
                                PalletName = x.PalletName,
                                PalletOwner = x.PalletOwner,
                                PalletProducer = x.PalletProducer,
                                ProducedDate = Utilities.NullDateToString(x.ProducedDate),
                                WarehouseCode = x.WarehouseCode,
                                WarehouseName = x.WarehouseName,
                                TransactionCode = x.TransactionCode,
                                TransactionType = Utilities.TransactionTypeBadge(x.TransactionType),
                                TransactionStatus = Utilities.TransactionStatusBadge(x.TransactionStatus),
                                TransactionDate = Utilities.NullDateTimeToString(x.TransactionDate),
                                ScannedDate = Utilities.NullDateTimeToString(x.ScannedDate),
                                ScannedBy = x.ScannedBy
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData},
                            JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Report_Pallet_Movement_{0}.xlsx", date);

            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            string warehouseId = Session["warehouseAccess"].ToString();

            IEnumerable<VwPalletHistory> list = IPallets.GetPalletHistory(warehouseId);

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;

            List<string> cols = new List<string>();
            cols.Add("Tag Id");
            cols.Add("Pallet Name");
            cols.Add("Pallet Owner");
            cols.Add("Pallet Producer");
            cols.Add("Produced Date");
            cols.Add("Warehouse Code");
            cols.Add("Warehouse Name");
            cols.Add("Transaction Code");
            cols.Add("Transaction Type");
            cols.Add("Transaction Status");
            cols.Add("Transaction Date");
            cols.Add("Scanned Date");
            cols.Add("Scanned By");

            int index = 1;
            foreach(string col in cols)
            {
                workSheet.Cells[1, index].Value = col;
                index++;
            }


            int recordIndex = 2;
            foreach (VwPalletHistory header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TagId;
                workSheet.Cells[recordIndex, 2].Value = header.PalletName;
                workSheet.Cells[recordIndex, 3].Value = header.PalletOwner;
                workSheet.Cells[recordIndex, 4].Value = header.PalletProducer;
                workSheet.Cells[recordIndex, 5].Value = header.ProducedDate;
                workSheet.Cells[recordIndex, 6].Value = header.WarehouseCode;
                workSheet.Cells[recordIndex, 7].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 8].Value = header.TransactionCode;
                workSheet.Cells[recordIndex, 9].Value = header.TransactionType;
                workSheet.Cells[recordIndex, 10].Value = header.TransactionStatus;
                workSheet.Cells[recordIndex, 11].Value = Utilities.NullDateTimeToString(header.TransactionDate);
                workSheet.Cells[recordIndex, 12].Value = Utilities.NullDateTimeToString(header.ScannedDate);
                workSheet.Cells[recordIndex, 13].Value = header.ScannedBy;
                recordIndex++;
            }

            for (int i = 1; i <= 13; i++)
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