using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Facelift_App.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Globalization;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class AccidentController : Controller
    {
        private readonly IAccidents IAccidents;
        private readonly IWarehouses IWarehouses;
        private readonly IPallets IPallets;
        private readonly IShipments IShipments;


        public AccidentController(IAccidents Accidents, IWarehouses Warehouses, IPallets Pallets, IShipments Shipments)
        {
            IAccidents = Accidents;
            IWarehouses = Warehouses;
            IPallets = Pallets;
            IShipments = Shipments;
            ViewBag.WarehouseDropdown = true;
        }

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

            IEnumerable<TrxAccidentHeader> list = IAccidents.GetFilteredData(warehouseId, stats, search, sortDirection, sortName);
            IEnumerable<AccidentDTO> pagedData = Enumerable.Empty<AccidentDTO>();

            int recordsTotal = IAccidents.GetTotalData(warehouseId, stats);
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {
                pagedData = from x in list
                            select new AccidentDTO
                            {
                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
                                TransactionCode = x.TransactionCode,
                                RefNumber = !string.IsNullOrEmpty(x.RefNumber) ? x.RefNumber : "-",
                                AccidentType = x.AccidentType,
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

                IEnumerable<VwAccidentItem> list = IAccidents.GetFilteredDataItem(transactionId, search, sortDirection, sortName);
                IEnumerable<AccidentItemDTO> pagedData = Enumerable.Empty<AccidentItemDTO>();
                List<VwAccidentItem> listScanned = new List<VwAccidentItem>();

                int recordsTotal = IAccidents.GetTotalDataItem(transactionId);
                int recordsFilteredTotal = list.Count();


                list = list.Skip(start).Take(length).ToList();


                //re-format
                if (list != null && list.Count() > 0)
                {
                    pagedData = from x in list
                                select new AccidentItemDTO
                                {
                                    TransactionItemId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionItemId, Constant.facelift_encryption_key)),
                                    TagId = x.TagId,
                                    PalletTypeName = x.PalletName,
                                    ReasonType = x.ReasonType,
                                    ReasonName = x.ReasonName,
                                    ScannedBy = x.ScannedBy,
                                    ScannedAt = Utilities.NullDateTimeToString(x.ScannedAt)
                                };
                }


                return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                                JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { result = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }

        }



        public async Task<ActionResult> Detail(string x)
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            ViewBag.TempMessage = TempData["TempMessage"];
            string id = "";
            TrxAccidentHeader data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await IAccidents.GetDataByIdAsync(id);
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

            AccidentVM dataVM = new AccidentVM
            {
                TransactionCode = data.TransactionCode,
                AccidentType = data.AccidentType,
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
                logs = data.LogAccidentHeaders.OrderBy(m => m.ExecutedAt).ToList(),
                versions = data.LogAccidentDocuments.OrderBy(n => n.Version).ToList(),
                attachments = GetAttachments(id)
            };

            //string[] reasonList = Constant.ReasonList.Where(r => r.Value.Equals(Constant.ReasonType.DAMAGE)).Select(r => r.Key.ToString()).ToArray();
            //var reasonList = Constant.ReasonList.Where(r => r.Value.ToString().Equals(data.ReasonType));

            //ViewBag.ReasonList = new SelectList(reasonList, "Key", "Key", data.ReasonName);
            //ViewBag.ReasonList = new SelectList(reasonList, "ReasonName", "ReasonName");
            ViewBag.TransactionStatus = data.TransactionStatus;

            if (data.AccidentType.Equals(Constant.AccidentType.INBOUND.ToString()))
            {
                TrxShipmentHeader shipment = await IShipments.GetDataByTransactionCodeAsync(data.RefNumber);
                ViewBag.ShipmentId = shipment.TransactionId;

                ShipmentDTO shipmentDTO = new ShipmentDTO
                {
                    TransactionId = shipment.TransactionId,
                    TransactionCode = shipment.TransactionCode,
                    ShipmentNumber = shipment.ShipmentNumber,
                    Remarks = shipment.Remarks,
                    WarehouseCode = shipment.WarehouseCode,
                    WarehouseName = shipment.WarehouseName,
                    DestinationCode = shipment.DestinationCode,
                    DestinationName = shipment.DestinationName,
                    TransporterName = shipment.TransporterName,
                    DriverName = shipment.DriverName,
                    PlateNumber = shipment.PlateNumber,
                    PalletQty = Utilities.FormatThousand(shipment.PalletQty),
                    TransactionStatus = shipment.TransactionStatus,
                    ShipmentStatus = shipment.ShipmentStatus,
                    CreatedBy = shipment.CreatedBy,
                    CreatedAt = Utilities.NullDateTimeToString(shipment.CreatedAt),
                    ModifiedBy = shipment.ModifiedBy,
                    ModifiedAt = Utilities.NullDateTimeToString(shipment.ModifiedAt),
                    ApprovedBy = shipment.ApprovedBy,
                    ApprovedAt = Utilities.NullDateTimeToString(shipment.ApprovedAt)
                };
            }

            ViewBag.Id = x;

            return View(dataVM);
        }


        private async Task SaveValidation(AccidentVM dataVM)
        {
            //if (!string.IsNullOrEmpty(dataVM.ReasonName))
            //{
            //    ModelState["ReasonName"].Errors.Clear();
            //    if (Constant.ReasonList.ContainsKey(dataVM.ReasonName))
            //    {
            //        string reasonType = Constant.ReasonList[dataVM.ReasonName].ToString();
            //        if (!reasonType.Equals(dataVM.ReasonType))
            //        {
            //            ModelState.AddModelError("ReasonName", "Reason not valid.");
            //        }
            //    }
            //    else
            //    {
            //        ModelState.AddModelError("ReasonName", "Reason not valid.");
            //    }
            //}

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
        public async Task<JsonResult> Detail(string x, AccidentVM dataVM)
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

            TrxAccidentHeader data = await IAccidents.GetDataByIdAsync(id);
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

                if (data.AccidentType.Equals(Constant.AccidentType.INBOUND.ToString()))
                {
                    message = "Update data not allowed, your user not authorized.";
                    return Json(new { stat = result, msg = message });
                }



            }

            dataVM.WarehouseId = data.WarehouseId;

            await SaveValidation(dataVM);

            if (ModelState.IsValid)
            {
                //data.ReasonName = dataVM.ReasonName;
                //data.ReasonType = Constant.ReasonList[data.ReasonName].ToString();
                data.Remarks = dataVM.Remarks;

                data.ModifiedBy = Session["username"].ToString();

                result = await IAccidents.UpdateAsync(data);

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

            TrxAccidentHeader data = await IAccidents.GetDataByIdAsync(id);
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

            result = await IAccidents.DeleteAsync(data);

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

            TrxAccidentHeader data = await IAccidents.GetDataByIdAsync(id);
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

                if (data.TrxAccidentItems.Count() < 1 && !data.TransactionStatus.Equals(Constant.TransactionStatus.PROGRESS.ToString()))
                {
                    message = "Document not allowed to be approved.";
                    return Json(new { stat = result, msg = message });
                }

                int totalUnscanned = data.TrxAccidentItems.Where(z => string.IsNullOrEmpty(z.ReasonType) || string.IsNullOrEmpty(z.ReasonName)).ToList().Count();
                if (totalUnscanned > 0)
                {
                    message = "Document not allowed to be approved. Please update all pallet reason.";
                    return Json(new { stat = result, msg = message });
                }

                if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Approve document not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }

            }


            data.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();
            data.ApprovedBy = Session["username"].ToString();

            result = await IAccidents.CloseAsync(data);
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

        [HttpPost]
        public async Task<JsonResult> DeleteItem(string x, bool selectAll, string[] items)
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
                message = "Delete item failed. Try to refresh page or contact system administrator.";
                return Json(new { stat = result, msg = message });
            }

            TrxAccidentHeader data = await IAccidents.GetDataByIdAsync(id);
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

                if (!data.TransactionStatus.Equals(Constant.AccidentType.INSPECTION.ToString()))
                {
                    message = "Delete data not allowed.";
                    return Json(new { stat = result, msg = message });
                }

                if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                {
                    message = "Delete data not allowed, document already closed.";
                    return Json(new { stat = result, msg = message });
                }
            }

            if (selectAll)
            {
                items = data.TrxAccidentItems.Select(m => m.TransactionItemId).ToArray();
                //if all data deleted, document status will become open
                data.TransactionStatus = Constant.TransactionStatus.OPEN.ToString();
            }
            else
            {
                try
                {
                    items = items.Select(s => Encryptor.Decrypt(Utilities.DecodeFrom64(s), Constant.facelift_encryption_key)).ToArray();
                }
                catch (Exception)
                {
                    message = "Delete item failed. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }
            }
            result = await IAccidents.DeleteItemAsync(data, items, Session["username"].ToString());

            if (result)
            {
                message = "Delete item succeeded.";
                TempData["TempMessage"] = message;
            }
            else
            {
                message = "Delete item failed. Please contact system administrator.";
            }


            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public async Task<JsonResult> UploadAttachment(String x)
        {
            bool result = false;
            string message = "";
            string transactionId = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
            string dir = "~/Content/img/BA/" + transactionId;

            if (Request.Files.Count > 0)
            {
                if (!Directory.Exists(Server.MapPath(dir)))
                {
                    Directory.CreateDirectory(Server.MapPath(dir));
                }

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];

                    if (file != null && file.ContentLength > 0 && (Path.GetExtension(file.FileName).ToLower() == ".jpeg"
                        || Path.GetExtension(file.FileName).ToLower() == ".jpg" || Path.GetExtension(file.FileName).ToLower() == ".png"))
                    {
                        if (file.ContentLength < (4 * 1024 * 1024))
                        {
                            try
                            {
                                string fileName = Path.GetFileName(file.FileName);
                                string filePath = Path.Combine(Server.MapPath(dir), fileName);
                                file.SaveAs(filePath);
                                result = true;
                            }
                            catch (Exception e)
                            {
                                message = "Upload item failed" + e.Message;
                            }
                        }
                        else
                        {
                            message = "Upload failed. Maximum allowed file size : 4MB ";
                        }

                    }
                    else
                    {
                        message = "Upload item failed. File is invalid.";
                    }

                }
            }

            return Json(new { stat = result, msg = message });
        }

        // PDF
        public ActionResult BeritaAcara(AccidentDTO accident)
        {
            return View(accident);
        }

        public async Task<ActionResult> GeneratePDFAsync(string x)
        {
            string id = "";
            string warehouseId = Session["warehouseAccess"].ToString();
            TrxAccidentHeader data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = await IAccidents.GetDataByIdAsync(id);
                    if (data == null)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        if (data.IsDeleted)
                        {
                            throw new Exception();
                        }

                        //if (data.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()))
                        //{
                        //    throw new Exception();
                        //}
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                return Content(@"<body>
                       <script type='text/javascript'>
                         window.close();
                       </script>
                     </body> ");
            }

            AccidentDTO accident = new AccidentDTO();
            accident.TransactionCode = data.TransactionCode;
            accident.WarehouseName = data.WarehouseName;
            accident.Day = data.CreatedAt.ToString("dddd", CultureInfo.CreateSpecificCulture("id-ID"));
            accident.Date = data.CreatedAt.ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("id-ID"));
            accident.Time = data.CreatedAt.ToString("H:mm");
            accident.Qty = data.TrxAccidentItems.Count.ToString();
            accident.Remarks = !string.IsNullOrEmpty(data.Remarks) ? data.Remarks : "-";
            accident.CreatedBy = data.CreatedBy;
            accident.AccidentType = data.AccidentType;
            accident.Items = Enumerable.Empty<AccidentItemDTO>();

            if (data.TrxAccidentItems != null && data.TrxAccidentItems.Count() > 0)
            {
                accident.Items = from y in data.TrxAccidentItems
                                 select new AccidentItemDTO
                                 {
                                     TagId = y.TagId,
                                     ReasonName = y.ReasonName
                                 };
            }


            string Domain = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

            ViewBag.Logo = Domain + "/Content/img/logo_black.png";
            String body = Utilities.RenderViewToString(this.ControllerContext, "BeritaAcara", accident);
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                using (var pdfWriter = new PdfWriter(stream))
                {
                    HtmlConverter.ConvertToPdf(body, pdfWriter);
                    byte[] file = stream.ToArray();
                    MemoryStream output = new MemoryStream();
                    output.Write(file, 0, file.Length);
                    output.Position = 0;

                    Response.AddHeader("content-disposition", "inline; filename=form.pdf");
                    // Return the output stream
                    return File(output, "application/pdf");
                }
            }
        }

        public List<FileAttachment> GetAttachments(string transactionId)
        {
            List<FileAttachment> attachments = new List<FileAttachment>();
            try
            {
                string dir = "~/Content/img/BA/" + transactionId;

                List<string> files = Directory.GetFiles(Server.MapPath(dir), "*.*", SearchOption.AllDirectories)
                .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".jpeg")).ToList();

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    attachments.Add(new FileAttachment
                    {
                        FileName = fileName,
                        FilePath = dir + "/" + fileName
                    });
                }
                   
            }
            catch(Exception e)
            {
                string s = e.Message;
            }

            return attachments;
        }

        public ActionResult ExportListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Accident_{0}.xlsx", date);
            string warehouseId = Session["warehouseAccess"].ToString();
            IEnumerable<TrxAccidentHeader> list = IAccidents.GetAllTransactions(warehouseId);
            
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Transaction Code";
            workSheet.Cells[1, 2].Value = "Ref Number";
            workSheet.Cells[1, 3].Value = "Accident Type";
            workSheet.Cells[1, 4].Value = "Remarks";
            workSheet.Cells[1, 5].Value = "Warehouse Code";
            workSheet.Cells[1, 6].Value = "Warehouse Name";
            workSheet.Cells[1, 7].Value = "Transaction Status";
            workSheet.Cells[1, 8].Value = "Created By";
            workSheet.Cells[1, 9].Value = "Created At";
            workSheet.Cells[1, 10].Value = "Modified By";
            workSheet.Cells[1, 11].Value = "Modified At";
            workSheet.Cells[1, 12].Value = "Approved By";
            workSheet.Cells[1, 13].Value = "Approved At";

            int recordIndex = 2;
            foreach (TrxAccidentHeader header in list)
            {
                workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                workSheet.Cells[recordIndex, 2].Value = header.RefNumber;
                workSheet.Cells[recordIndex, 3].Value = header.AccidentType;
                workSheet.Cells[recordIndex, 4].Value = header.Remarks;
                workSheet.Cells[recordIndex, 5].Value = header.WarehouseCode;
                workSheet.Cells[recordIndex, 6].Value = header.WarehouseName;
                workSheet.Cells[recordIndex, 7].Value = header.TransactionStatus;
                workSheet.Cells[recordIndex, 8].Value = header.CreatedBy;
                workSheet.Cells[recordIndex, 9].Value = Utilities.NullDateTimeToString(header.CreatedAt);
                workSheet.Cells[recordIndex, 10].Value = header.ModifiedBy;
                workSheet.Cells[recordIndex, 11].Value = Utilities.NullDateTimeToString(header.ModifiedAt);
                workSheet.Cells[recordIndex, 12].Value = header.ApprovedBy;
                workSheet.Cells[recordIndex, 13].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
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

        public ActionResult ExportDetailListToExcel()
        {
            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileName = String.Format("filename=Facelift_Accident_Details_{0}.xlsx", date);
            string warehouseId = Session["warehouseAccess"].ToString();
            IEnumerable<TrxAccidentHeader> list = IAccidents.GetAllTransactions(warehouseId);

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;


            workSheet.Row(1).Height = 25;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = "Transaction Code";
            workSheet.Cells[1, 2].Value = "Ref Number";
            workSheet.Cells[1, 3].Value = "Accident Type";
            workSheet.Cells[1, 4].Value = "Remarks";
            workSheet.Cells[1, 5].Value = "Warehouse Code";
            workSheet.Cells[1, 6].Value = "Warehouse Name";
            workSheet.Cells[1, 7].Value = "Transaction Status";
            workSheet.Cells[1, 8].Value = "Created By";
            workSheet.Cells[1, 9].Value = "Created At";
            workSheet.Cells[1, 10].Value = "Modified By";
            workSheet.Cells[1, 11].Value = "Modified At";
            workSheet.Cells[1, 12].Value = "Approved By";
            workSheet.Cells[1, 13].Value = "Approved At";
            workSheet.Cells[1, 14].Value = "Tag Id";
            workSheet.Cells[1, 15].Value = "Scanned By";
            workSheet.Cells[1, 16].Value = "Scanned At";
            

            int recordIndex = 2;
            foreach (TrxAccidentHeader header in list)
            {
                foreach (TrxAccidentItem item in header.TrxAccidentItems)
                {
                    workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                    workSheet.Cells[recordIndex, 2].Value = header.RefNumber;
                    workSheet.Cells[recordIndex, 3].Value = header.AccidentType;
                    workSheet.Cells[recordIndex, 4].Value = header.Remarks;
                    workSheet.Cells[recordIndex, 5].Value = header.WarehouseCode;
                    workSheet.Cells[recordIndex, 6].Value = header.WarehouseName;
                    workSheet.Cells[recordIndex, 7].Value = header.TransactionStatus;
                    workSheet.Cells[recordIndex, 8].Value = header.CreatedBy;
                    workSheet.Cells[recordIndex, 9].Value = Utilities.NullDateTimeToString(header.CreatedAt);
                    workSheet.Cells[recordIndex, 10].Value = header.ModifiedBy;
                    workSheet.Cells[recordIndex, 11].Value = Utilities.NullDateTimeToString(header.ModifiedAt);
                    workSheet.Cells[recordIndex, 12].Value = header.ApprovedBy;
                    workSheet.Cells[recordIndex, 13].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
                    workSheet.Cells[recordIndex, 14].Value = item.TagId;
                    workSheet.Cells[recordIndex, 15].Value = item.ScannedBy;
                    workSheet.Cells[recordIndex, 16].Value = Utilities.NullDateTimeToString(item.ScannedAt);
                    recordIndex++;
                }
            }

            for (int i = 1; i <= 16; i++)
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

        public JsonResult DeleteAttachment(string id, string fileName)
        {
            bool result = false;
            string message = "";
            string transactionId = Encryptor.Decrypt(Utilities.DecodeFrom64(id), Constant.facelift_encryption_key);
            string filePath = "~/Content/img/BA/" + transactionId + "/" + fileName;

            if (System.IO.File.Exists(Server.MapPath(filePath)))
            {
                System.IO.File.Delete(Server.MapPath(filePath));
                result = true;
                message = "Delete succeeded.";
            }

            return Json(new { stat = result, msg = message });
        }

        public FileResult DownloadAttachment(string id, string fileName)
        {

            string transactionId = Encryptor.Decrypt(Utilities.DecodeFrom64(id), Constant.facelift_encryption_key);
            string filePath = "~/Content/img/BA/" + transactionId + "/" + fileName;

            if (System.IO.File.Exists(Server.MapPath(filePath)))
            {
                string name = transactionId + "_" + fileName;
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath(filePath));
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, name);
            }

            return null;           
        }

    }
}