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
    public class ReportAccidentController : Controller
    {
        private readonly IAccidents IAccidents;
        private readonly IWarehouses IWarehouses;

        public ReportAccidentController(IAccidents Accidents, IWarehouses Warehouses)
        {
            IAccidents = Accidents;
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
        public ActionResult DatatableAccident()
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

            IEnumerable<TrxAccidentHeader> list = IAccidents.GetAccidentData(warehouseId, startDate, endDate, search, sortDirection, sortName);
            IEnumerable<AccidentDTO> pagedData = Enumerable.Empty<AccidentDTO>();

            int recordsTotal = IAccidents.GetTotalAccident(warehouseId, startDate, endDate);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new AccidentDTO
                            {
                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
                                CreatedAt = Utilities.NullDateToString(x.CreatedAt),
                                RefNumber = !string.IsNullOrEmpty(x.RefNumber) ? x.RefNumber : "-",
                                WarehouseName = x.WarehouseName,
                                TransactionStatus = x.TransactionStatus,
                                AccidentType = x.AccidentType,
                                Remarks = x.Remarks,
                                Qty = x.TrxAccidentItems.Count().ToString(),
                                TransactionCode = x.TransactionCode
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Report_Accident_{0}.xlsx", date);

            string warehouseId = Session["warehouseAccess"].ToString();
            string startDate = Request["startDate"];
            string endDate = Request["endDate"];

            IEnumerable<TrxAccidentHeader> list = IAccidents.GetAccidentData(warehouseId, startDate, endDate);

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Transaction Code";
            workSheet.Cells[1, 2].Value = "Transaction Date";
            workSheet.Cells[1, 3].Value = "Ref Number";
            workSheet.Cells[1, 4].Value = "Accident Type";
            workSheet.Cells[1, 6].Value = "Remarks";
            workSheet.Cells[1, 7].Value = "Warehouse";
            workSheet.Cells[1, 8].Value = "Qty";

            int recordIndex = 2;
            foreach (TrxAccidentHeader header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                workSheet.Cells[recordIndex, 2].Value = header.CreatedAt;
                workSheet.Cells[recordIndex, 2].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 3].Value = header.RefNumber;
                workSheet.Cells[recordIndex, 4].Value = header.AccidentType;
                workSheet.Cells[recordIndex, 6].Value = header.Remarks;
                workSheet.Cells[recordIndex, 7].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 8].Value = header.TrxAccidentItems.Count;
                recordIndex++;
            }

            for (int i = 1; i <= 17; i++)
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