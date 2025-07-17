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
    public class ReportShipmentController : Controller
    {
        private readonly IShipments IShipments;
        private readonly IWarehouses IWarehouses;

        public ReportShipmentController(IShipments Shipments, IWarehouses Warehouses)
        {
            IShipments = Shipments;
            IWarehouses = Warehouses;
            ViewBag.WarehouseDropdown = true;
        }

        // GET: ReportShipment
        public async Task<ActionResult> Index()
        {
            string warehouseAccess = Session["warehouseAccess"].ToString();
            ViewBag.DestinationList = new SelectList(await IWarehouses.GetDestinationAsync(warehouseAccess), "WarehouseId", "WarehouseName");
            return View();
        }

        [HttpPost]
        public ActionResult DatatableDelivery()
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

            string DestinationId = Request["DestinationId"];

            IEnumerable<TrxShipmentHeader> list = IShipments.GetDeliveryData(warehouseId, DestinationId, startDate, endDate, search, sortDirection, sortName);
            IEnumerable<ShipmentDTO> pagedData = Enumerable.Empty<ShipmentDTO>();

            int recordsTotal = IShipments.GetTotalDelivery(warehouseId, DestinationId, startDate, endDate);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new ShipmentDTO
                            {
                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
                                CreatedAt = Utilities.NullDateToString(x.CreatedAt),
                                ShipmentNumber = !string.IsNullOrEmpty(x.ShipmentNumber) ? x.ShipmentNumber : "-",
                                WarehouseName = x.WarehouseName,
                                DestinationName = x.DestinationName,
                                TransporterName = x.TransporterName,
                                DriverName = x.DriverName,
                                PlateNumber = x.PlateNumber,
                                PalletQty = x.PalletQty.ToString(),
                                ReceivedQty = x.TrxShipmentItems.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count().ToString(),
                                TransactionCode = x.PalletQty == x.TrxShipmentItems.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count() ? x.TransactionCode : Utilities.ColorBadge(x.TransactionCode, "danger"),
                                LossQty = x.TrxShipmentItems.Where(m => !m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count().ToString()
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DatatableIncoming()
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

            string OriginId = Request["OriginId"];

            IEnumerable<TrxShipmentHeader> list = IShipments.GetIncomingData(warehouseId, OriginId, startDate, endDate, search, sortDirection, sortName);
            IEnumerable<ShipmentDTO> pagedData = Enumerable.Empty<ShipmentDTO>();

            int recordsTotal = IShipments.GetTotalIncoming(warehouseId, OriginId, startDate, endDate);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new ShipmentDTO
                            {
                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
                                CreatedAt = Utilities.NullDateToString(x.CreatedAt),
                                ShipmentNumber = !string.IsNullOrEmpty(x.ShipmentNumber) ? x.ShipmentNumber : "-",
                                WarehouseName = x.WarehouseName,
                                DestinationName = x.DestinationName,
                                TransporterName = x.TransporterName,
                                DriverName = x.DriverName,
                                PlateNumber = x.PlateNumber,
                                PalletQty = x.PalletQty.ToString(),
                                ReceivedQty = x.TrxShipmentItems.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count().ToString(),
                                TransactionCode = x.PalletQty == x.TrxShipmentItems.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count() ? x.TransactionCode : Utilities.ColorBadge(x.TransactionCode, "danger"),
                                LossQty = x.TrxShipmentItems.Where(m => !m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count().ToString()
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportListToExcel(string startDate, string endDate, string destinationId, string originId, string type)
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");

            string fileName = "";
            
            string warehouseId = Session["warehouseAccess"].ToString();
            IEnumerable<TrxShipmentHeader> list;


            if (type.Equals("in"))
            {
                fileName = String.Format("filename=Facelift_Report_Shipment_Incoming_{0}.xlsx", date);
                list = IShipments.GetIncomingData(warehouseId, originId, startDate, endDate);
            }
            else
            {
                fileName = String.Format("filename=Facelift_Report_Shipment_Delivery_{0}.xlsx", date);
                list = IShipments.GetDeliveryData(warehouseId, destinationId, startDate, endDate);
            }

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Transaction Code";
            workSheet.Cells[1, 2].Value = "Transaction Date";
            workSheet.Cells[1, 3].Value = "Shipment Number";
            workSheet.Cells[1, 4].Value = "Origin";
            workSheet.Cells[1, 5].Value = "Destination";
            workSheet.Cells[1, 6].Value = "Transporter";
            workSheet.Cells[1, 7].Value = "Driver";
            workSheet.Cells[1, 8].Value = "Plate Number";
            workSheet.Cells[1, 9].Value = "Sent Qty";
            workSheet.Cells[1, 10].Value = "Received Qty";
            workSheet.Cells[1, 11].Value = "Loss Qty";

            int recordIndex = 2;
            foreach (TrxShipmentHeader header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                workSheet.Cells[recordIndex, 2].Value = header.CreatedAt;
                workSheet.Cells[recordIndex, 2].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 3].Value = header.ShipmentNumber;
                workSheet.Cells[recordIndex, 4].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 5].Value = header.DestinationName;
                workSheet.Cells[recordIndex, 6].Value = header.TransporterName;
                workSheet.Cells[recordIndex, 7].Value = header.DriverName;
                workSheet.Cells[recordIndex, 8].Value = header.PlateNumber;
                workSheet.Cells[recordIndex, 9].Value = header.PalletQty.ToString();
                workSheet.Cells[recordIndex, 10].Value = header.TrxShipmentItems.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count().ToString();
                workSheet.Cells[recordIndex, 11].Value = header.TrxShipmentItems.Where(m => !m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).Count().ToString();
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