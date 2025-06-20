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
    public class ReportCycleCountController : Controller
    {
        private readonly ICycleCounts ICycleCounts;
        private readonly IWarehouses IWarehouses;

        public ReportCycleCountController(ICycleCounts CycleCounts, IWarehouses Warehouses)
        {
            ICycleCounts = CycleCounts;
            IWarehouses = Warehouses;
            ViewBag.WarehouseDropdown = true;
        }

        // GET: ReportCycleCount
        public async Task<ActionResult> Index()
        {
            string warehouseAccess = Session["warehouseAccess"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult DatatableCycleCount()
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            string startDate = Request["startDate"];
            string endDate = Request["endDate"];

            IEnumerable<TrxCycleCountHeader> list = ICycleCounts.GetCycleCountData(warehouseId, startDate, endDate, search, sortDirection, sortName);
            IEnumerable<CycleCountDTO> pagedData = Enumerable.Empty<CycleCountDTO>();

            int recordsTotal = ICycleCounts.GetTotalCycleCount(warehouseId, startDate, endDate);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new CycleCountDTO
                            {
                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
                                TransactionCode = x.TrxCycleCountItems.Count() == x.TrxCycleCountItems.Where(y => y.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count() ? x.TransactionCode : Utilities.ColorBadge(x.TransactionCode, "danger"),
                                WarehouseName = x.WarehouseName,
                                WarehouseCode = x.WarehouseCode,
                                TransactionStatus = x.TransactionStatus,
                                Remarks = x.Remarks,
                                AccidentId = x.AccidentId,
                                LossQty = x.TrxCycleCountItems.Where(y => y.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OP.ToString())).Count().ToString(),
                                ScannedQty = x.TrxCycleCountItems.Where(y => y.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count().ToString(),
                                TotalQty = x.TrxCycleCountItems.Count().ToString(),
                                CreatedBy = x.CreatedBy,
                                CreatedAt = Utilities.NullDateToString(x.CreatedAt),
                                ModifiedBy = x.ModifiedBy,
                                ModifiedAt = Utilities.NullDateToString(x.ModifiedAt),
                                ApprovedBy = x.ApprovedBy,
                                ApprovedAt = Utilities.NullDateToString(x.ApprovedAt)
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Report_Cycle_count_{0}.xlsx", date);

            string warehouseId = Session["warehouseAccess"].ToString();
            string startDate = Request["startDate"];
            string endDate = Request["endDate"];

            IEnumerable<TrxCycleCountHeader> list = ICycleCounts.GetCycleCountData(warehouseId, startDate, endDate);

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Transaction Code";
            workSheet.Cells[1, 2].Value = "Warehouse Code";
            workSheet.Cells[1, 3].Value = "Warehouse Name";
            workSheet.Cells[1, 4].Value = "Transaction Status";
            workSheet.Cells[1, 5].Value = "No. BA";
            workSheet.Cells[1, 6].Value = "Scanned Qty";
            workSheet.Cells[1, 7].Value = "Loss Qty";
            workSheet.Cells[1, 8].Value = "Total Qty";
            workSheet.Cells[1, 9].Value = "Remarks";
            workSheet.Cells[1, 10].Value = "Created By";
            workSheet.Cells[1, 11].Value = "Created At";
            workSheet.Cells[1, 12].Value = "Modified By";
            workSheet.Cells[1, 13].Value = "Modified At";
            workSheet.Cells[1, 14].Value = "Approved By";
            workSheet.Cells[1, 15].Value = "Approved At";

            int recordIndex = 2;
            foreach (TrxCycleCountHeader header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                workSheet.Cells[recordIndex, 2].Value = header.WarehouseCode;
                workSheet.Cells[recordIndex, 3].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 4].Value = header.TransactionStatus;
                workSheet.Cells[recordIndex, 5].Value = header.AccidentId;
                workSheet.Cells[recordIndex, 6].Value = header.TrxCycleCountItems.Where(y => y.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count().ToString();
                workSheet.Cells[recordIndex, 7].Value = header.TrxCycleCountItems.Where(y => y.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OP.ToString())).Count().ToString();
                workSheet.Cells[recordIndex, 8].Value = header.TrxCycleCountItems.Count;
                workSheet.Cells[recordIndex, 9].Value = header.Remarks;
                workSheet.Cells[recordIndex, 10].Value = header.CreatedBy;
                workSheet.Cells[recordIndex, 11].Value = header.CreatedAt;
                workSheet.Cells[recordIndex, 11].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 12].Value = header.ModifiedBy;
                workSheet.Cells[recordIndex, 13].Value = header.ModifiedAt;
                workSheet.Cells[recordIndex, 13].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 14].Value = header.ApprovedBy;
                workSheet.Cells[recordIndex, 15].Value = header.ApprovedAt;
                workSheet.Cells[recordIndex, 15].Style.Numberformat.Format = "yyyy-MM-dd";
                recordIndex++;
            }

            for (int i = 1; i <= 15; i++)
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
            return RedirectToAction("Master");
        }
    }
}