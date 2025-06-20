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

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class ReportStockController : Controller
    {
        private readonly IPallets IPallets;

        public ReportStockController(IPallets Pallets)
        {
            IPallets = Pallets;
            ViewBag.WarehouseDropdown = true;
        }

        // GET: ReportStock
        public ActionResult Index()
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            int totalStock = IPallets.GetTotalStock(warehouseId);
            ViewBag.totalRegistered = totalStock;
            return View();
        }

        [HttpPost]
        public ActionResult Datatable()
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            string palletCondition = Request["PalletCondition"];

            IEnumerable<VwPallet> list = IPallets.GetTotalStock(warehouseId, palletCondition, search, sortDirection, sortName);
            IEnumerable<PalletDTO> pagedData = Enumerable.Empty<PalletDTO>(); ;

            int recordsTotal = IPallets.GetTotalStock(warehouseId);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new PalletDTO
                            {
                                TagId = x.TagId,
                                PalletCode = x.PalletCode,
                                PalletTypeId = x.PalletTypeId,
                                PalletName = x.PalletName,
                                PalletCondition = Utilities.PalletConditionBadge(x.PalletCondition),
                                WarehouseId = x.WarehouseId,
                                WarehouseCode = x.WarehouseCode,
                                WarehouseName = x.WarehouseName,
                                PalletOwner = x.PalletOwner,
                                PalletProducer = x.PalletProducer,
                                ProducedDate = Utilities.NullDateToString(x.ProducedDate),
                                Description = x.Description,
                                SapId = x.SapId,
                                RegisteredBy = x.RegisteredBy,
                                RegisteredAt = Utilities.NullDateTimeToString(x.RegisteredAt),
                                ReceivedBy = x.ReceivedBy,
                                ReceivedAt = Utilities.NullDateTimeToString(x.ReceivedAt),
                                PalletMovementStatus = Utilities.PalletMovementStatusBadge(x.PalletMovementStatus),
                                LastTransactionName = x.LastTransactionName,
                                LastTransactionCode = x.LastTransactionCode,
                                LastTransactionDate = Utilities.NullDateToString(x.LastTransactionDate)
                            };
            }

            string totalPallet = Utilities.FormatThousand(recordsTotal);

            int good = IPallets.GetTotalByCondition(warehouseId, Constant.PalletCondition.GOOD.ToString());
            int damage = IPallets.GetTotalByCondition(warehouseId, Constant.PalletCondition.DAMAGE.ToString());
            int loss = IPallets.GetTotalByCondition(warehouseId, Constant.PalletCondition.LOSS.ToString());
            
            string totalGood = Utilities.FormatThousand(good);
            string totalDamage = Utilities.FormatThousand(damage);
            string totalLoss = Utilities.FormatThousand(loss);

            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData, totalPallet = totalPallet, totalGood = totalGood, totalDamage = totalDamage, totalLoss = totalLoss },
                            JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DatatableActual()
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            string palletCondition = Request["PalletCondition"];

            IEnumerable<MsPallet> list = IPallets.GetActualStock(warehouseId, palletCondition, search, sortDirection, sortName);
            IEnumerable<PalletDTO> pagedData = Enumerable.Empty<PalletDTO>(); ;

            int recordsTotal = IPallets.GetActualStock(warehouseId);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new PalletDTO
                            {
                                TagId = x.TagId,
                                PalletCode = x.PalletCode,
                                PalletTypeId = x.PalletTypeId,
                                PalletName = x.PalletName,
                                PalletCondition = Utilities.PalletConditionBadge(x.PalletCondition),
                                WarehouseId = x.WarehouseId,
                                WarehouseCode = x.WarehouseCode,
                                WarehouseName = x.WarehouseName,
                                PalletOwner = x.PalletOwner,
                                PalletProducer = x.PalletProducer,
                                ProducedDate = Utilities.NullDateToString(x.ProducedDate),
                                Description = x.Description,
                                SapId = x.SapId,
                                RegisteredBy = x.RegisteredBy,
                                RegisteredAt = Utilities.NullDateTimeToString(x.RegisteredAt),
                                ReceivedBy = x.ReceivedBy,
                                ReceivedAt = Utilities.NullDateTimeToString(x.ReceivedAt),
                                PalletMovementStatus = Utilities.PalletMovementStatusBadge(x.PalletMovementStatus),
                                LastTransactionName = x.LastTransactionName,
                                LastTransactionCode = x.LastTransactionCode,
                                LastTransactionDate = Utilities.NullDateToString(x.LastTransactionDate)
                            };
            }

            string totalPallet = Utilities.FormatThousand(recordsTotal);

            int good = IPallets.GetActualByCondition(warehouseId, Constant.PalletCondition.GOOD.ToString());
            int damage = IPallets.GetActualByCondition(warehouseId, Constant.PalletCondition.DAMAGE.ToString());
            int loss = IPallets.GetActualByCondition(warehouseId, Constant.PalletCondition.LOSS.ToString());

            string totalGood = Utilities.FormatThousand(good);
            string totalDamage = Utilities.FormatThousand(damage);
            string totalLoss = Utilities.FormatThousand(loss);

            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData, totalPallet = totalPallet, totalGood = totalGood, totalDamage = totalDamage, totalLoss = totalLoss },
                             JsonRequestBehavior.AllowGet);
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


            IEnumerable<VwPalletShipment> list = IPallets.GetDeliveryStock(warehouseId, search, sortDirection, sortName);
            IEnumerable<PalletDTO> pagedData = Enumerable.Empty<PalletDTO>();

            int recordsTotal = IPallets.GetDeliveryStock(warehouseId);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new PalletDTO
                            {
                                TagId = x.TagId,
                                PalletCode = x.PalletCode,
                                PalletName = x.PalletName,
                                PalletCondition = Utilities.PalletConditionBadge(x.PalletCondition),
                                WarehouseName = x.DestinationName,
                                PalletOwner = x.PalletOwner,
                                PalletProducer = x.PalletProducer,
                                ProducedDate = Utilities.NullDateToString(x.ProducedDate),
                                RegisteredBy = x.RegisteredBy,
                                RegisteredAt = Utilities.NullDateTimeToString(x.RegisteredAt),
                                ReceivedBy = x.ReceivedBy,
                                ReceivedAt = Utilities.NullDateTimeToString(x.ReceivedAt),
                                PalletMovementStatus = Utilities.PalletMovementStatusBadge(x.PalletMovementStatus),
                                LastTransactionName = x.LastTransactionName,
                                LastTransactionCode = x.LastTransactionCode,
                                LastTransactionDate = Utilities.NullDateToString(x.LastTransactionDate)
                            };
            }

            string totalPallet = Utilities.FormatThousand(recordsTotal);

            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData, totalPallet = totalPallet},
                             JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Report_Stock_All_{0}.xlsx", date);

            string warehouseId = Session["warehouseAccess"].ToString();

            IEnumerable<VwPallet> list = IPallets.GetStockReport(warehouseId);

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Tag Id";
            workSheet.Cells[1, 2].Value = "Pallet Code";
            workSheet.Cells[1, 3].Value = "Pallet Type";
            workSheet.Cells[1, 4].Value = "Pallet Condition";
            workSheet.Cells[1, 5].Value = "Warehouse";
            workSheet.Cells[1, 6].Value = "Pallet Owner";
            workSheet.Cells[1, 7].Value = "Pallet Producer";
            workSheet.Cells[1, 8].Value = "Produced Date";
            workSheet.Cells[1, 9].Value = "Registered By";
            workSheet.Cells[1, 10].Value = "Registered At";
            workSheet.Cells[1, 11].Value = "Received By";
            workSheet.Cells[1, 12].Value = "Received At";
            workSheet.Cells[1, 13].Value = "Pallet Movement Status";
            workSheet.Cells[1, 14].Value = "Last Transaction Name";
            workSheet.Cells[1, 15].Value = "Last Transaction Code";
            workSheet.Cells[1, 16].Value = "Last Transaction Date";

            int recordIndex = 2;
            foreach (VwPallet header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TagId;
                workSheet.Cells[recordIndex, 2].Value = header.PalletCode;
                workSheet.Cells[recordIndex, 3].Value = header.PalletName;
                workSheet.Cells[recordIndex, 4].Value = header.PalletCondition;
                workSheet.Cells[recordIndex, 5].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 6].Value = header.PalletOwner;
                workSheet.Cells[recordIndex, 7].Value = header.PalletProducer;
                workSheet.Cells[recordIndex, 8].Value = header.ProducedDate;
                workSheet.Cells[recordIndex, 8].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 9].Value = header.RegisteredBy;
                workSheet.Cells[recordIndex, 10].Value = header.RegisteredAt;
                workSheet.Cells[recordIndex, 10].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 11].Value = header.ReceivedBy;
                workSheet.Cells[recordIndex, 12].Value = header.ReceivedAt;
                workSheet.Cells[recordIndex, 12].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 13].Value = header.PalletMovementStatus;
                workSheet.Cells[recordIndex, 14].Value = header.LastTransactionName;
                workSheet.Cells[recordIndex, 15].Value = header.LastTransactionCode;
                workSheet.Cells[recordIndex, 16].Value = header.LastTransactionDate;
                workSheet.Cells[recordIndex, 16].Style.Numberformat.Format = "yyyy-MM-dd";
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

        public ActionResult ExportListActualToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Report_Stock_Actual_{0}.xlsx", date);

            string warehouseId = Session["warehouseAccess"].ToString();

            IEnumerable<VwPallet> list = IPallets.GetStockActualReport(warehouseId);

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Tag Id";
            workSheet.Cells[1, 2].Value = "Pallet Code";
            workSheet.Cells[1, 3].Value = "Pallet Type";
            workSheet.Cells[1, 4].Value = "Pallet Condition";
            workSheet.Cells[1, 5].Value = "Warehouse";
            workSheet.Cells[1, 6].Value = "Pallet Owner";
            workSheet.Cells[1, 7].Value = "Pallet Producer";
            workSheet.Cells[1, 8].Value = "Produced Date";
            workSheet.Cells[1, 9].Value = "Registered By";
            workSheet.Cells[1, 10].Value = "Registered At";
            workSheet.Cells[1, 11].Value = "Received By";
            workSheet.Cells[1, 12].Value = "Received At";
            workSheet.Cells[1, 13].Value = "Pallet Movement Status";
            workSheet.Cells[1, 14].Value = "Last Transaction Name";
            workSheet.Cells[1, 15].Value = "Last Transaction Code";
            workSheet.Cells[1, 16].Value = "Last Transaction Date";

            int recordIndex = 2;
            foreach (VwPallet header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TagId;
                workSheet.Cells[recordIndex, 2].Value = header.PalletCode;
                workSheet.Cells[recordIndex, 3].Value = header.PalletName;
                workSheet.Cells[recordIndex, 4].Value = header.PalletCondition;
                workSheet.Cells[recordIndex, 5].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 6].Value = header.PalletOwner;
                workSheet.Cells[recordIndex, 7].Value = header.PalletProducer;
                workSheet.Cells[recordIndex, 8].Value = header.ProducedDate;
                workSheet.Cells[recordIndex, 8].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 9].Value = header.RegisteredBy;
                workSheet.Cells[recordIndex, 10].Value = header.RegisteredAt;
                workSheet.Cells[recordIndex, 10].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 11].Value = header.ReceivedBy;
                workSheet.Cells[recordIndex, 12].Value = header.ReceivedAt;
                workSheet.Cells[recordIndex, 12].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 13].Value = header.PalletMovementStatus;
                workSheet.Cells[recordIndex, 14].Value = header.LastTransactionName;
                workSheet.Cells[recordIndex, 15].Value = header.LastTransactionCode;
                workSheet.Cells[recordIndex, 16].Value = header.LastTransactionDate;
                workSheet.Cells[recordIndex, 16].Style.Numberformat.Format = "yyyy-MM-dd";
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

        public ActionResult ExportListDeliveryToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Report_Stock_Delivery_{0}.xlsx", date);

            string warehouseId = Session["warehouseAccess"].ToString();

            IEnumerable<VwPalletShipment> list = IPallets.GetStockDeliveryReport(warehouseId);

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Tag Id";
            workSheet.Cells[1, 2].Value = "Pallet Code";
            workSheet.Cells[1, 3].Value = "Pallet Type";
            workSheet.Cells[1, 4].Value = "Pallet Condition";
            workSheet.Cells[1, 5].Value = "Origin";
            workSheet.Cells[1, 6].Value = "Destination";
            workSheet.Cells[1, 7].Value = "Pallet Owner";
            workSheet.Cells[1, 8].Value = "Pallet Producer";
            workSheet.Cells[1, 9].Value = "Produced Date";
            workSheet.Cells[1, 10].Value = "Registered By";
            workSheet.Cells[1, 11].Value = "Registered At";
            workSheet.Cells[1, 12].Value = "Received By";
            workSheet.Cells[1, 13].Value = "Received At";
            workSheet.Cells[1, 14].Value = "Pallet Movement Status";
            workSheet.Cells[1, 15].Value = "Last Transaction Name";
            workSheet.Cells[1, 16].Value = "Last Transaction Code";
            workSheet.Cells[1, 17].Value = "Last Transaction Date";

            int recordIndex = 2;
            foreach (VwPalletShipment header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TagId;
                workSheet.Cells[recordIndex, 2].Value = header.PalletCode;
                workSheet.Cells[recordIndex, 3].Value = header.PalletName;
                workSheet.Cells[recordIndex, 4].Value = header.PalletCondition;
                workSheet.Cells[recordIndex, 5].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 6].Value = header.PalletOwner;
                workSheet.Cells[recordIndex, 7].Value = header.PalletProducer;
                workSheet.Cells[recordIndex, 8].Value = header.ProducedDate;
                workSheet.Cells[recordIndex, 8].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 9].Value = header.RegisteredBy;
                workSheet.Cells[recordIndex, 10].Value = header.RegisteredAt;
                workSheet.Cells[recordIndex, 10].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 11].Value = header.ReceivedBy;
                workSheet.Cells[recordIndex, 12].Value = header.ReceivedAt;
                workSheet.Cells[recordIndex, 12].Style.Numberformat.Format = "yyyy-MM-dd";
                workSheet.Cells[recordIndex, 13].Value = header.PalletMovementStatus;
                workSheet.Cells[recordIndex, 14].Value = header.LastTransactionName;
                workSheet.Cells[recordIndex, 15].Value = header.LastTransactionCode;
                workSheet.Cells[recordIndex, 16].Value = header.LastTransactionDate;
                workSheet.Cells[recordIndex, 16].Style.Numberformat.Format = "yyyy-MM-dd";
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