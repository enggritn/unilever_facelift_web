using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Facelift_App.Validators;
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
    public class CycleCountController : Controller
    {
        private readonly ICycleCounts ICycleCounts;
        private readonly IWarehouses IWarehouses;
        private readonly IPallets IPallets;
        private readonly IAccidents IAccidents;


        public CycleCountController(ICycleCounts CycleCounts, IWarehouses Warehouses, IPallets Pallets, IAccidents Accidents)
        {
            ICycleCounts = CycleCounts;
            IWarehouses = Warehouses;
            IPallets = Pallets;
            IAccidents = Accidents;
            ViewBag.WarehouseDropdown = true;
        }

        // GET: CycleCount
        public ActionResult Index()
        {
            ViewBag.TempMessage = TempData["TempMessage"];
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

            string stats = Request["stats"];

            IEnumerable<TrxCycleCountHeader> list = ICycleCounts.GetFilteredData(warehouseId, stats, search, sortDirection, sortName);
            IEnumerable<CycleCountDTO> pagedData = Enumerable.Empty<CycleCountDTO>(); ;

            int recordsTotal = ICycleCounts.GetTotalData(warehouseId, stats);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {

                pagedData = from x in list
                            select new CycleCountDTO
                            {
                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
                                TransactionCode = x.TransactionCode,
                                WarehouseName = x.WarehouseName,
                                TransactionStatus = Utilities.TransactionStatusBadge(x.TransactionStatus),
                                CreatedBy = x.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(x.ModifiedBy) ? x.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(x.ModifiedAt),
                                ApprovedBy = !string.IsNullOrEmpty(x.ApprovedBy) ? x.ApprovedBy : "-",
                                ApprovedAt = Utilities.NullDateTimeToString(x.ApprovedAt)
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Create()
        {
            CycleCountVM dataVM = new CycleCountVM();
            dataVM.WarehouseId = Session["warehouseAccess"].ToString();

            MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(dataVM.WarehouseId);
            dataVM.WarehouseCode = warehouse.WarehouseCode;
            dataVM.WarehouseName = warehouse.WarehouseName;


            return View(dataVM);
        }


        private async Task SaveValidation(CycleCountVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.WarehouseId))
            {
                ModelState["WarehouseId"].Errors.Clear();
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string WarehouseValid = await validator.IsWarehouseExist(dataVM.WarehouseId);
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("WarehouseId", WarehouseValid);
                }
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(CycleCountVM dataVM)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            bool result = false;
            string message = "Invalid form submission.";

            dataVM.WarehouseId = Session["warehouseAccess"].ToString();

            await SaveValidation(dataVM);

            int totalOpenData = ICycleCounts.GetTotalOpenData(dataVM.WarehouseId);
            if (totalOpenData > 0)
            {
                message = "Can not create cycle count, because there still opened cycle count transaction.";
                return Json(new { stat = result, msg = message });
            }

            int totalProgressShipment = ICycleCounts.GetTotalProgressShipment(dataVM.WarehouseId);
            if (totalProgressShipment > 0)
            {
                message = "Cannot make a cycle count, because there are still inbound transactions that have not been completed.";
                return Json(new { stat = result, msg = message });
            }

            if (ModelState.IsValid)
            {
                MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(dataVM.WarehouseId);

                TrxCycleCountHeader data = new TrxCycleCountHeader
                {
                    TransactionId = Utilities.CreateGuid("STK"),
                    Remarks = dataVM.Remarks,
                    WarehouseId = dataVM.WarehouseId,
                    WarehouseCode = warehouse.WarehouseCode,
                    WarehouseName = warehouse.WarehouseName,
                    TransactionStatus = Constant.TransactionStatus.OPEN.ToString(),
                    CreatedBy = Session["username"].ToString()
                };

                result = await ICycleCounts.CreateAsync(data);
                if (result)
                {
                    message = "Create data succeeded.";
                    TempData["TempMessage"] = message;
                    response.Add("transactionId", Utilities.EncodeTo64(Encryptor.Encrypt(data.TransactionId, Constant.facelift_encryption_key)));
                }
                else
                {
                    message = "Create data failed. Please contact system administrator.";
                }

            }


            response.Add("stat", result);
            response.Add("msg", message);

            return Json(response);
        }


        public async Task<ActionResult> Detail(string x)
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            ViewBag.TempMessage = TempData["TempMessage"];
            string id = "";
            TrxCycleCountHeader data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await ICycleCounts.GetDataByIdAsync(id);
                    if (data == null)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                        {
                            throw new Exception();
                        }
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }

            CycleCountVM dataVM = new CycleCountVM
            {
                TransactionCode = data.TransactionCode,
                Remarks = data.Remarks,
                WarehouseId = data.WarehouseId,
                WarehouseCode = data.WarehouseCode,
                WarehouseName = data.WarehouseName,
                TransactionStatus = Utilities.TransactionStatusBadge(data.TransactionStatus),
                CreatedBy = data.CreatedBy,
                CreatedAt = data.CreatedAt,
                ModifiedBy = data.ModifiedBy,
                ModifiedAt = data.ModifiedAt,
                ApprovedBy = data.ApprovedBy,
                ApprovedAt = data.ApprovedAt,
                logs = data.LogCycleCountHeaders.OrderBy(m => m.ExecutedAt).ToList(),
                versions = data.LogCycleCountDocuments.OrderBy(m => m.Version).ToList()
            };

            ViewBag.TransactionStatus = data.TransactionStatus;
            ViewBag.Id = x;

            return View(dataVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, CycleCountVM dataVM)
        {
            bool result = false;
            string message = "Invalid form submission.";
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
            try
            {
                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception();
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                dataVM.TransactionCode = id;
            }
            catch (Exception)
            {
                message = "Update data failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxCycleCountHeader data = await ICycleCounts.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }

                if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Update data not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }

            }

            dataVM.WarehouseId = data.WarehouseId;

            await SaveValidation(dataVM);

            if (ModelState.IsValid)
            {

                data.Remarks = dataVM.Remarks;

                data.ModifiedBy = Session["username"].ToString();

                result = await ICycleCounts.UpdateAsync(data);

                if (result)
                {
                    message = "Update data succeeded.";
                    TempData["TempMessage"] = message;
                }
                else
                {
                    message = "Update data failed. Please contact system administrator.";
                }
            }

            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public async Task<JsonResult> Delete(string x)
        {
            bool result = false;
            string message = "";
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
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
                message = "Delete document failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxCycleCountHeader data = await ICycleCounts.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }

                if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Delete document not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }

            }

            data.IsDeleted = true;

            data.ModifiedBy = Session["username"].ToString();

            result = await ICycleCounts.DeleteAsync(data);

            if (result)
            {
                message = "Delete document succeeded.";
                TempData["TempMessage"] = message;
            }
            else
            {
                message = "Delete document failed. Please contact system administrator.";
            }

            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public ActionResult DatatableItem(string id)
        {
            string transactionId = "";
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception();
                }

                transactionId = Encryptor.Decrypt(Utilities.DecodeFrom64(id), Constant.facelift_encryption_key);

                string warehouseId = Session["warehouseAccess"].ToString();
                int draw = Convert.ToInt32(Request["draw"]);
                int start = Convert.ToInt32(Request["start"]);
                int length = Convert.ToInt32(Request["length"]);
                string search = Request["search[value]"];
                string orderCol = Request["order[0][column]"];
                string sortName = Request["columns[" + orderCol + "][name]"];
                string sortDirection = Request["order[0][dir]"];

                IEnumerable<VwCycleCountItem> list = ICycleCounts.GetFilteredDataItem(transactionId, search, sortDirection, sortName);
                IEnumerable<CycleCountItemDTO> pagedData = Enumerable.Empty<CycleCountItemDTO>();
                List<VwCycleCountItem> listScanned = new List<VwCycleCountItem>();

                int recordsTotal = ICycleCounts.GetTotalDataItem(transactionId);
                int recordsFilteredTotal = list.Count();


                listScanned = list.Where(x => x.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString()) || x.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString())).ToList();


                list = list.Skip(start).Take(length).ToList();


                //re-format
                if (list != null && list.Count() > 0)
                {
                    pagedData = from x in list
                                select new CycleCountItemDTO
                                {
                                    TransactionItemId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionItemId, Constant.facelift_encryption_key)),
                                    TagId = x.TagId,
                                    PalletCondition = Utilities.PalletConditionBadge(x.PalletCondition),
                                    PalletMovementStatus = Utilities.MovementStatusBadge(x.PalletMovementStatus),
                                    PalletTypeName = x.PalletName,
                                    PalletOwner = x.PalletOwner,
                                    PalletProducer = x.PalletProducer,
                                    ScannedBy = !string.IsNullOrEmpty(x.ScannedBy) ? x.ScannedBy : "-",
                                    ScannedAt = Utilities.NullDateTimeToString(x.ScannedAt)
                                };
                }

                int totalScanned = listScanned.Count();

                return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData, totalScanned = totalScanned },
                                JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { result = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public async Task<JsonResult> Close(string x)
        {
            bool result = false;
            string message = "";
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
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
                message = "Approve document failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxCycleCountHeader data = await ICycleCounts.GetDataByIdAsync(id);
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }
                //check previous data, close can be executed if status == on progress

                if (data.TrxCycleCountItems.Count() < 1 && !data.TransactionStatus.Equals(Constant.TransactionStatus.PROGRESS.ToString()))
                {
                    message = "Document not allowed to be approved.";
                    return Json(new { stat = result, msg = message });
                }
                
                if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Approve document not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }

                int totalRecord = data.TrxCycleCountItems.Count();

                if (data.TrxCycleCountItems.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString())).Count() != totalRecord)
                {
                    message = "Document not allowed to be approved. All item not scanned yet.";
                    return Json(new { stat = result, msg = message });
                }

            }

            //posted only for new pallet only ? if there's one registered pallet exist, can not posted data ? clarify this.

            data.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();
            data.ApprovedBy = Session["username"].ToString();

            result = await ICycleCounts.CloseAsync(data);
            //after close, insert pallet to master pallet
            if (result)
            {
                message = "Approve data succeeded.";
                TempData["TempMessage"] = message;
            }
            else
            {
                message = "Approve document failed. Please contact system administrator.";
            }



            return Json(new { stat = result, msg = message });
        }

        public async Task<ActionResult> CreateBA(string x)
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            ViewBag.TempMessage = TempData["TempMessage"];
            string id = "";
            TrxCycleCountHeader data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await ICycleCounts.GetDataByIdAsync(id);
                    if (data == null)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
                        {
                            throw new Exception();
                        }

                        if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                        {
                            throw new Exception();
                        }
                        if (!string.IsNullOrEmpty(data.AccidentId))
                        {
                            throw new Exception();
                        }
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }


            AccidentVM dataVM = new AccidentVM
            {
                TransactionCode = data.TransactionCode,
                AccidentType = Constant.AccidentType.STOCK_TAKE.ToString(),
                WarehouseId = data.WarehouseId,
                WarehouseCode = data.WarehouseCode,
                WarehouseName = data.WarehouseName
            };

            
           
            ViewBag.Id = x;

            return View(dataVM);
        }

        private async Task SaveValidation(AccidentVM dataVM)
        {
            if (!string.IsNullOrEmpty(dataVM.WarehouseId))
            {
                ModelState["WarehouseId"].Errors.Clear();
                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
                string WarehouseValid = await validator.IsWarehouseExist(dataVM.WarehouseId);
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("WarehouseId", WarehouseValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateBA(string x, AccidentVM dataVM)
        {
            bool result = false;
            string message = "Invalid form submission.";
            string id = "";
            bool status = false;
            string warehouseId = Session["warehouseAccess"].ToString();
            string transactionId = "";
            try
            {
                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception();
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                dataVM.TransactionCode = id;
            }
            catch (Exception)
            {
                message = "Create BA failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxCycleCountHeader cycle = await ICycleCounts.GetDataByIdAsync(id);
            if (cycle == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!cycle.WarehouseId.Equals(warehouseId) || cycle.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }

                if (cycle.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Create BA data not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }

                if (!string.IsNullOrEmpty(cycle.AccidentId))
                {
                    message = "Create BA not allowed, BA already exist.";
                    return Json(new { stat = result, msg = message });
                }
            }
            string accidentType = Constant.AccidentType.STOCK_TAKE.ToString();

            dataVM.AccidentType = accidentType;
            dataVM.WarehouseId = warehouseId;


            await SaveValidation(dataVM);

            if (ModelState.IsValid)
            {
                MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(dataVM.WarehouseId);
                TrxAccidentHeader data = new TrxAccidentHeader
                {
                    TransactionId = Utilities.CreateGuid("BA"),
                    RefNumber = cycle.TransactionCode,
                    AccidentType = dataVM.AccidentType,
                    Remarks = dataVM.Remarks,
                    WarehouseId = dataVM.WarehouseId,
                    WarehouseCode = warehouse.WarehouseCode,
                    WarehouseName = warehouse.WarehouseName,
                    TransactionStatus = Constant.TransactionStatus.PROGRESS.ToString(),
                    CreatedBy = Session["username"].ToString()
                };

                string[] items = cycle.TrxCycleCountItems.Where(m => m.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OP.ToString())).Select(m => m.TagId).ToArray();
                data.TrxAccidentItems = new List<TrxAccidentItem>();
                foreach (string tag in items)
                {
                    TrxAccidentItem item = new TrxAccidentItem();
                    item.TransactionItemId = Utilities.CreateGuid("BAI");
                    item.TransactionId = data.TransactionId;
                    item.TagId = tag;
                    //item.ScannedBy = Session["username"].ToString();
                    //item.ScannedAt = DateTime.Now;
                    data.TrxAccidentItems.Add(item);

                    // update FREEZE to GOOD
                    status = await ICycleCounts.UpdatePalletAsync(dataVM.WarehouseId, tag, Constant.PalletCondition.GOOD.ToString());
                }

                //create BA
                result = await IAccidents.CreateAsync(data);
                if (result)
                {
                    //close transaction without updating stock
                    cycle.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();
                    cycle.AccidentId = data.TransactionCode;
                    result = await ICycleCounts.CreateAccidentReportAsync(cycle, Session["username"].ToString());

                    if (result)
                    {
                        message = "Create BA succeeded.";
                        TempData["TempMessage"] = message;
                        transactionId = data.TransactionId;
                    }
                    else
                    {
                        message = "Create BA failed. Please contact system administrator.";
                    }
                   
                }
                else
                {
                    message = "Create BA failed. Please contact system administrator.";
                }
            }

            return Json(new { stat = result, msg = message, transactionId = transactionId });
        }

        public ActionResult ExportListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_CycleCount_{0}.xlsx", date);
            string warehouseId = Session["warehouseAccess"].ToString();
            IEnumerable<TrxCycleCountHeader> list = ICycleCounts.GetAllTransactions(warehouseId);
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Transaction Code";
            workSheet.Cells[1, 2].Value = "Remarks";
            workSheet.Cells[1, 3].Value = "Warehouse Code";
            workSheet.Cells[1, 4].Value = "Warehouse Name";
            workSheet.Cells[1, 5].Value = "Transaction Status";
            workSheet.Cells[1, 6].Value = "Created By";
            workSheet.Cells[1, 7].Value = "Created At";
            workSheet.Cells[1, 8].Value = "Modified By";
            workSheet.Cells[1, 9].Value = "Modified At";
            workSheet.Cells[1, 10].Value = "Approved By";
            workSheet.Cells[1, 11].Value = "Approved At";

            int recordIndex = 2;
            foreach (TrxCycleCountHeader header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                workSheet.Cells[recordIndex, 2].Value = header.Remarks;
                workSheet.Cells[recordIndex, 3].Value = header.WarehouseCode;
                workSheet.Cells[recordIndex, 4].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 5].Value = header.TransactionStatus;
                workSheet.Cells[recordIndex, 6].Value = header.CreatedBy;
                workSheet.Cells[recordIndex, 7].Value = Utilities.NullDateTimeToString(header.CreatedAt);
                workSheet.Cells[recordIndex, 8].Value = header.ModifiedBy;
                workSheet.Cells[recordIndex, 9].Value = Utilities.NullDateTimeToString(header.ModifiedAt);
                workSheet.Cells[recordIndex, 10].Value = header.ApprovedBy;
                workSheet.Cells[recordIndex, 11].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
                recordIndex++;
            }

            for (int i = 1; i <= 11; i++)
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

        public ActionResult ExportDetailListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_CycleCount_Details_{0}.xlsx", date);
            string warehouseId = Session["warehouseAccess"].ToString();
            IEnumerable<TrxCycleCountHeader> list = ICycleCounts.GetAllTransactions(warehouseId);

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Transaction Code";
            workSheet.Cells[1, 2].Value = "Remarks";
            workSheet.Cells[1, 3].Value = "Warehouse Code";
            workSheet.Cells[1, 4].Value = "Warehouse Name";
            workSheet.Cells[1, 5].Value = "Transaction Status";
            workSheet.Cells[1, 6].Value = "Created By";
            workSheet.Cells[1, 7].Value = "Created At";
            workSheet.Cells[1, 8].Value = "Modified By";
            workSheet.Cells[1, 9].Value = "Modified At";
            workSheet.Cells[1, 10].Value = "Approved By";
            workSheet.Cells[1, 11].Value = "Approved At";
            workSheet.Cells[1, 12].Value = "Tag Id";
            workSheet.Cells[1, 13].Value = "Scanned By";
            workSheet.Cells[1, 14].Value = "Scanned At";


            int recordIndex = 2;
            foreach (TrxCycleCountHeader header in list)
            {
                foreach (TrxCycleCountItem item in header.TrxCycleCountItems)
                {
                    workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                    workSheet.Cells[recordIndex, 2].Value = header.Remarks;
                    workSheet.Cells[recordIndex, 3].Value = header.WarehouseCode;
                    workSheet.Cells[recordIndex, 4].Value = header.WarehouseName;
                    workSheet.Cells[recordIndex, 5].Value = header.TransactionStatus;
                    workSheet.Cells[recordIndex, 6].Value = header.CreatedBy;
                    workSheet.Cells[recordIndex, 7].Value = Utilities.NullDateTimeToString(header.CreatedAt);
                    workSheet.Cells[recordIndex, 8].Value = header.ModifiedBy;
                    workSheet.Cells[recordIndex, 9].Value = Utilities.NullDateTimeToString(header.ModifiedAt);
                    workSheet.Cells[recordIndex, 10].Value = header.ApprovedBy;
                    workSheet.Cells[recordIndex, 11].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
                    workSheet.Cells[recordIndex, 12].Value = item.TagId;
                    workSheet.Cells[recordIndex, 13].Value = item.ScannedBy;
                    workSheet.Cells[recordIndex, 14].Value = Utilities.NullDateTimeToString(item.ScannedAt);
                    recordIndex++;
                }
            }

            for (int i = 1; i <= 14; i++)
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