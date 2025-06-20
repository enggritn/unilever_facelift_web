//using Facelift_App.Helper;
//using Facelift_App.Models;
//using Facelift_App.Services;
//using Facelift_App.Validators;
//using iText.Html2pdf;
//using iText.Kernel.Geom;
//using iText.Kernel.Pdf;
//using iText.Layout;
//using OfficeOpenXml;
//using OfficeOpenXml.Style;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;

//namespace Facelift_App.Controllers
//{
//    [SessionCheck]
//    [AuthCheck]
//    public class InvoiceController : Controller
//    {
//        private readonly IBillingHistories IBillingHistories;
//        private readonly IWarehouses IWarehouses;
//        private readonly IBillings IBillings;

//        public InvoiceController(IBillingHistories BillingHistories, IWarehouses Warehouses, IBillings Billings)
//        {
//            IBillingHistories = BillingHistories;
//            IWarehouses = Warehouses;
//            IBillings = Billings;
//            ViewBag.WarehouseDropdown = true;
//        }

//        // GET: Invoice
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

//            string agingType = Constant.AgingType.USED.ToString();

//            IEnumerable<TrxBillingRentHistoryHeader> list = IBillingHistories.GetFilteredRentData(warehouseId, agingType,search, sortDirection, sortName);
//            IEnumerable<BillingRentDTO> pagedData = Enumerable.Empty<BillingRentDTO>();

//            int recordsTotal = IBillingHistories.GetTotalRentData(warehouseId, agingType);
//            int recordsFilteredTotal = list.Count();


//            list = list.Skip(start).Take(length).ToList();


//            //re-format
//            if (list != null && list.Count() > 0)
//            {

//                pagedData = from x in list
//                            select new BillingRentDTO
//                            {
//                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
//                                TransactionCode = x.TransactionCode,
//                                CurrentYear = x.CurrentYear.ToString(),
//                                CurrentMonth = x.CurrentMonth.ToString(),
//                                Tax = x.Tax.ToString(),
//                                StartPeriod = Utilities.NullDateToString(x.StartPeriod),
//                                LastPeriod = Utilities.NullDateToString(x.LastPeriod),
//                                CreatedBy = x.CreatedBy,
//                                CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
//                            };
//            }


//            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
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

//            string agingType = Constant.AgingType.UNUSED.ToString();

//            IEnumerable<TrxBillingRentHistoryHeader> list = IBillingHistories.GetFilteredRentData(warehouseId, agingType, search, sortDirection, sortName);
//            IEnumerable<BillingRentDTO> pagedData = Enumerable.Empty<BillingRentDTO>();

//            int recordsTotal = IBillingHistories.GetTotalRentData(warehouseId, agingType);
//            int recordsFilteredTotal = list.Count();


//            list = list.Skip(start).Take(length).ToList();


//            //re-format
//            if (list != null && list.Count() > 0)
//            {

//                pagedData = from x in list
//                            select new BillingRentDTO
//                            {
//                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
//                                TransactionCode = x.TransactionCode,
//                                CurrentYear = x.CurrentYear.ToString(),
//                                CurrentMonth = x.CurrentMonth.ToString(),
//                                Tax = x.Tax.ToString(),
//                                StartPeriod = Utilities.NullDateToString(x.StartPeriod),
//                                LastPeriod = Utilities.NullDateToString(x.LastPeriod),
//                                CreatedBy = x.CreatedBy,
//                                CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
//                            };
//            }


//            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
//                            JsonRequestBehavior.AllowGet);
//        }


//        public async Task<ActionResult> Create(string type)
//        {
//            string warehouseAccess = Session["warehouseAccess"].ToString();

//            MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(warehouseAccess);
          
//            try
//            {
//                if (!string.IsNullOrEmpty(type))
//                {
//                    switch (type)
//                    {
//                        case "used":
//                            BillingRentVM dataVM = new BillingRentVM();
//                            dataVM.WarehouseCode = warehouse.WarehouseCode;
//                            dataVM.WarehouseName = warehouse.WarehouseName;
//                            dataVM.Tax = 10;
//                            dataVM.AgingType = Constant.AgingType.USED.ToString();
//                            dataVM.Payment = 100;
//                            return View("CreateRent", dataVM);
//                        case "unused":
//                            dataVM = new BillingRentVM();
//                            //dataVM.WarehouseCode = warehouse.WarehouseCode;
//                            //dataVM.WarehouseName = warehouse.WarehouseName;
//                            //dataVM.Tax = 10;
//                            //dataVM.AgingType = Constant.AgingType.UNUSED.ToString();
//                            //dataVM.Payment = warehouse.PalletUsedTarget;
//                            //foreach (var wh in await IWarehouses.GetAllAsync())
//                            //{
//                            //    dataVM.Remarks += string.Format("<br>" + "{0} - {1} : ({2}%)", wh.WarehouseCode, wh.WarehouseName, wh.PalletUsedTarget.ToString());
//                            //}
//                            return View("CreateRent", dataVM);
//                        case "accident":
//                            return View("CreateAccident");
//                    }
//                }
//                else
//                {
//                    throw new Exception();
//                }
//            }
//            catch (Exception)
//            {
               
//            }

//            return RedirectToAction("Index");

//        }


//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<JsonResult> CreateRent(BillingRentVM dataVM)
//        {
//            Dictionary<string, object> response = new Dictionary<string, object>();
//            bool result = false;
//            string message = "Invalid form submission.";

//            dataVM.WarehouseId = Session["warehouseAccess"].ToString();



//            //server validation

//            if (!string.IsNullOrEmpty(dataVM.AgingType))
//            {
//                if(!dataVM.AgingType.Equals(Constant.AgingType.USED.ToString()) && !dataVM.AgingType.Equals(Constant.AgingType.UNUSED.ToString()))
//                {
//                    ModelState.AddModelError("AgingType", "Unrecognized Aging Type.");
//                }
//            }

//            if (!string.IsNullOrEmpty(dataVM.AgingType))
//            {
//                if (dataVM.AgingType.Equals(Constant.AgingType.UNUSED.ToString()))
//                {
//                    //check logic, if all weight already 100% then allow
//                    int totalWeight = IWarehouses.GetTotalWeight();
//                    if(totalWeight < 100)
//                    {
//                        ModelState.AddModelError("Payment", "");
//                        message = "Incomplete weight setting, total weight must be 100. Please contact system administrator.";
//                    }
//                }
//            }


//            if (!string.IsNullOrEmpty(dataVM.WarehouseId))
//            {
//                ModelState["WarehouseId"].Errors.Clear();
//                WarehouseValidator validator = new WarehouseValidator(IWarehouses);
//                string WarehouseValid = await validator.IsWarehouseExist(dataVM.WarehouseId);
//                if (!WarehouseValid.Equals("true"))
//                {
//                    ModelState.AddModelError("WarehouseId", WarehouseValid);
//                }
//            }

//            var allowedYear = Enumerable.Range(2020, DateTime.Now.Year);
//            var allowedMonth = Enumerable.Range(1, 12);

//            if (!allowedYear.Contains(dataVM.CurrentYear))
//            {
//                ModelState.AddModelError("CurrentYear", "Not allowed year.");
//            }

//            if (!allowedMonth.Contains(dataVM.CurrentMonth))
//            {
//                ModelState.AddModelError("CurrentMonth", "Not allowed month.");
//            }

//            double? total = IBillings.GetTotalPrice(dataVM.WarehouseId, dataVM.AgingType, dataVM.CurrentMonth, dataVM.CurrentYear);
            

//            if(total == null || total < 1)
//            {
//                ModelState.AddModelError("CurrentYear", "");
//                ModelState.AddModelError("CurrentMonth", "");
//                message = "No billing for selected year and month.";
//            }

//            //check if data already exist for current month and year, if already exist cannot create same invoice
//            TrxBillingRentHistoryHeader prevData = await IBillingHistories.GetRentInvoiceAsync(dataVM.WarehouseId, dataVM.AgingType, dataVM.CurrentYear, dataVM.CurrentMonth);
//            if(prevData != null)
//            {
//                ModelState.AddModelError("CurrentYear", "");
//                ModelState.AddModelError("CurrentMonth", "");
//                message = "Invoice already exist for selected year and month.";
//            }

//            if (ModelState.IsValid)
//            {
//                MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(dataVM.WarehouseId);

//                TrxBillingRentHistoryHeader data = new TrxBillingRentHistoryHeader
//                {
//                    TransactionId = Utilities.CreateGuid("INV"),
//                    WarehouseId = dataVM.WarehouseId,
//                    WarehouseCode = warehouse.WarehouseCode,
//                    WarehouseName = warehouse.WarehouseName,
//                    CurrentYear = dataVM.CurrentYear,
//                    CurrentMonth = dataVM.CurrentMonth,
//                    Tax = dataVM.Tax,
//                    CreatedBy = Session["username"].ToString()
//                };

//                Constant.AgingType agingType = (Constant.AgingType)Enum.Parse(typeof(Constant.AgingType), dataVM.AgingType, true);
//                switch (agingType)
//                {
//                    case Constant.AgingType.USED:
//                        data.Payment = 100;
//                        break;
//                    case Constant.AgingType.UNUSED:
//                        //data.Payment = warehouse.PalletUsedTarget;
//                        //int index = 0;
//                        //foreach (var wh in await IWarehouses.GetAllAsync())
//                        //{
//                        //    if(index > 0)
//                        //    {
//                        //        data.Remarks += "<br>";
//                        //    }
//                        //    data.Remarks += string.Format("{0} - {1} : ({2}%)", wh.WarehouseCode, wh.WarehouseName, wh.PalletUsedTarget.ToString());
//                        //    index++;
//                        //}

//                        break;
//                }


//                data.AgingType = agingType.ToString();



//                result = await IBillingHistories.CreateRentAsync(data);
//                if (result)
//                {
//                    message = "Create data succeeded.";
//                    TempData["TempMessage"] = message;
//                    response.Add("transactionId", Utilities.EncodeTo64(Encryptor.Encrypt(data.TransactionId, Constant.facelift_encryption_key)));
//                }
//                else
//                {
//                    message = "Create data failed. Please contact system administrator.";
//                }

//            }


//            response.Add("stat", result);
//            response.Add("msg", message);

//            return Json(response);
//        }


//        public async Task<ActionResult> Detail(string type, string x)
//        {
//            string warehouseId = Session["warehouseAccess"].ToString();
//            MsWarehouse warehouse = await IWarehouses.GetDataByIdAsync(warehouseId);
//            ViewBag.TempMessage = TempData["TempMessage"];
//            string id = "";
//            try
//            {
//                if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(x))
//                {
//                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
//                    ViewBag.Id = x;
//                    //check type
//                    switch (type)
//                    {
//                        case "used":
//                            TrxBillingRentHistoryHeader data = await IBillingHistories.GetRentDataByIdAsync(id);
//                            if(data == null)
//                            {
//                                throw new Exception();
//                            }

//                            if (!data.WarehouseId.Equals(warehouseId))
//                            {
//                                throw new Exception();
//                            }

//                            double? total = IBillingHistories.GetTotalRentPrice(id);
//                            string totalPrice = total != null && total > 0 ? Utilities.FormatDoubleToThousand(total) : "0";
//                            double tax = data.Tax / 100.0;
//                            double? grandTotal = total + (total * tax);
//                            string GrandTotal = grandTotal != null && grandTotal > 0 ? Utilities.FormatDoubleToThousand(grandTotal) : "0";

//                            BillingRentVM dataVM = new BillingRentVM()
//                            {
//                                TransactionCode = data.TransactionCode,
//                                WarehouseId = data.WarehouseId,
//                                WarehouseCode = data.WarehouseCode,
//                                WarehouseName = data.WarehouseName,
//                                CurrentYear = data.CurrentYear,
//                                CurrentMonth = data.CurrentMonth,
//                                Tax = data.Tax,
//                                Payment = data.Payment,
//                                Remarks = data.Remarks,
//                                StartPeriod = data.StartPeriod,
//                                LastPeriod = data.LastPeriod,
//                                CreatedBy = data.CreatedBy,
//                                CreatedAt = data.CreatedAt,
//                                TotalPallet = Utilities.FormatThousand(data.TrxBillingRentHistoryItems.Count()),
//                                TotalPrice = totalPrice,
//                                GrandTotal = GrandTotal,
//                                AgingType = Constant.AgingType.USED.ToString()
//                            };
                            
                            
//                            return View("DetailRent", dataVM);
//                        case "unused":
//                            data = await IBillingHistories.GetRentDataByIdAsync(id);
//                            if(data == null)
//                            {
//                                throw new Exception();
//                            }

//                            if (!data.WarehouseId.Equals(warehouseId))
//                            {
//                                throw new Exception();
//                            }

//                            total = IBillingHistories.GetTotalRentPrice(id);
//                            totalPrice = total != null && total > 0 ? Utilities.FormatDoubleToThousand(total) : "0";
//                            tax = data.Tax / 100.0;
//                            grandTotal = total + (total * tax);
//                            GrandTotal = grandTotal != null && grandTotal > 0 ? Utilities.FormatDoubleToThousand(grandTotal) : "0";

//                            dataVM = new BillingRentVM()
//                            {
//                                TransactionCode = data.TransactionCode,
//                                WarehouseId = data.WarehouseId,
//                                WarehouseCode = data.WarehouseCode,
//                                WarehouseName = data.WarehouseName,
//                                CurrentYear = data.CurrentYear,
//                                CurrentMonth = data.CurrentMonth,
//                                Tax = data.Tax,
//                                Payment = data.Payment,
//                                Remarks = data.Remarks,
//                                StartPeriod = data.StartPeriod,
//                                LastPeriod = data.LastPeriod,
//                                CreatedBy = data.CreatedBy,
//                                CreatedAt = data.CreatedAt,
//                                TotalPallet = Utilities.FormatThousand(data.TrxBillingRentHistoryItems.Count()),
//                                TotalPrice = totalPrice,
//                                GrandTotal = GrandTotal,
//                                AgingType = Constant.AgingType.UNUSED.ToString()
//                            };
                            
                            
//                            return View("DetailRent", dataVM);
//                        case "accident":
//                            return View("DetailAccident");
//                    }
//                }
//                else
//                {
//                    throw new Exception();
//                }
//            }
//            catch (Exception)
//            {
                
//            }

           

//            return RedirectToAction("Index");
//        }


//        [HttpPost]
//        public ActionResult DatatableItemRent(string id)
//        {
//            string transactionId = "";
//            try
//            {
//                if (string.IsNullOrEmpty(id))
//                {
//                    throw new Exception();
//                }

//                transactionId = Encryptor.Decrypt(Utilities.DecodeFrom64(id), Constant.facelift_encryption_key);

//                string warehouseId = Session["warehouseAccess"].ToString();
//                int draw = Convert.ToInt32(Request["draw"]);
//                int start = Convert.ToInt32(Request["start"]);
//                int length = Convert.ToInt32(Request["length"]);
//                string search = Request["search[value]"];
//                string orderCol = Request["order[0][column]"];
//                string sortName = Request["columns[" + orderCol + "][name]"];
//                string sortDirection = Request["order[0][dir]"];

//                IEnumerable<VwBillingRentHistoryItem> list = IBillingHistories.GetFilteredRentDataItem(transactionId, search, sortDirection, sortName);
//                IEnumerable<BillingRentItemDTO> pagedData = Enumerable.Empty<BillingRentItemDTO>();

//                int recordsTotal = IBillingHistories.GetTotalRentDataItem(transactionId);
//                int recordsFilteredTotal = list.Count();


//                list = list.Skip(start).Take(length).ToList();

//                //re-format
//                if (list != null && list.Count() > 0)
//                {
//                    pagedData = from x in list
//                                select new BillingRentItemDTO
//                                {
//                                    TransactionItemId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionItemId, Constant.facelift_encryption_key)),
//                                    PalletId = x.PalletId,
//                                    PalletTypeName = x.PalletName,
//                                    PalletOwner = x.PalletOwner,
//                                    PalletProducer = x.PalletProducer,
//                                    CurrentYear = x.CurrentYear.ToString(),
//                                    CurrentMonth = x.CurrentMonth.ToString(),
//                                    TotalMinutes = Utilities.FormatThousand(x.TotalMinutes),
//                                    TotalHours = Utilities.FormatDoubleToThousand(x.TotalHours),
//                                    TotalDays = Utilities.FormatDoubleToThousand(x.TotalDays),
//                                    BillingYear = x.BillingYear.ToString(),
//                                    BillingPrice = Utilities.FormatDecimalToThousand(x.BillingPrice),
//                                    TotalBilling = Utilities.FormatDoubleToThousand(x.TotalBilling)
//                                };
//                }

              

//                return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData},
//                                JsonRequestBehavior.AllowGet);
//            }
//            catch (Exception e)
//            {
//                return Json(new { result = false, message = e.Message }, JsonRequestBehavior.AllowGet);
//            }

//        }


//        // PDF
//        public ActionResult InvoiceRent(BillingRentDTO data)
//        {
//            return View(data);
//        }

//        public async Task<ActionResult> GeneratePDFAsync(string x)
//        {
//            string id = "";
//            string warehouseId = Session["warehouseAccess"].ToString();
//            TrxBillingRentHistoryHeader data = null;
//            try
//            {
//                if (!string.IsNullOrEmpty(x))
//                {
//                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
//                    data = await IBillingHistories.GetRentDataByIdAsync(id);
//                    if (data == null)
//                    {
//                        throw new Exception();
//                    }
//                    else
//                    {
//                        if (!data.WarehouseId.Equals(warehouseId))
//                        {
//                            throw new Exception();
//                        }
//                    }
//                }
//                else
//                {
//                    throw new Exception();
//                }
//            }
//            catch (Exception)
//            {
//                return Content(@"<body>
//                       <script type='text/javascript'>
//                         window.close();
//                       </script>
//                     </body> ");
//            }



//            double? total = IBillingHistories.GetTotalRentPrice(id);
//            string totalPrice = total != null && total > 0 ? Utilities.FormatDoubleToThousand(total) : "0";
//            double tax = data.Tax / 100.0;
//            double? grandTotal = total + (total * tax);
//            string GrandTotal = grandTotal != null && grandTotal > 0 ? Utilities.FormatDoubleToThousand(grandTotal) : "0";

//            BillingRentDTO dto = new BillingRentDTO();
//            dto.TransactionCode = data.TransactionCode;
//            dto.WarehouseCode = data.WarehouseCode;
//            dto.WarehouseName = data.WarehouseName;
//            dto.TotalBilling = totalPrice;
//            dto.Tax = data.Tax.ToString() + "%";
//            dto.Payment = data.Payment.ToString() + "%";
//            dto.Remarks = data.Remarks;
//            dto.GrandTotal = GrandTotal;
//            dto.CurrentYear = data.CurrentYear.ToString();
//            dto.CurrentMonth = data.CurrentMonth.ToString();
//            dto.StartPeriod = Utilities.NullDateToString(data.StartPeriod);
//            dto.LastPeriod = Utilities.NullDateToString(data.LastPeriod);
//            dto.CreatedBy = data.CreatedBy;
//            dto.CreatedAt = Utilities.NullDateToString(data.CreatedAt);

//            dto.Items = Enumerable.Empty<BillingRentItemDTO>();

//            IEnumerable<VwBillingRentHistoryItem> items = IBillingHistories.GetRentDataItemsById(id);

//            if (items != null && items.Count() > 0)
//            {
//                dto.Items = from y in items
//                            select new BillingRentItemDTO
//                                {
//                                    PalletId = y.PalletId,
//                                    PalletTypeName = y.PalletName,
//                                    PalletOwner = y.PalletOwner,
//                                    PalletProducer = y.PalletProducer,
//                                    CurrentYear = y.CurrentYear.ToString(),
//                                    CurrentMonth = y.CurrentMonth.ToString(),
//                                    TotalMinutes = Utilities.FormatThousand(y.TotalMinutes),
//                                    TotalHours = Utilities.FormatDoubleToThousand(y.TotalHours),
//                                    TotalDays = Utilities.FormatDoubleToThousand(y.TotalDays),
//                                    BillingYear = y.BillingYear.ToString(),
//                                    BillingPrice = Utilities.FormatDecimalToThousand(y.BillingPrice),
//                                    TotalBilling = Utilities.FormatDoubleToThousand(y.TotalBilling)
//                                };
//            }


//            string Domain = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

//            ViewBag.Logo = Domain + "/Content/img/logo_black.png";
//            ViewBag.Approved = Domain + "/Content/img/approved.png";

//            string view = "";
//            switch (data.AgingType)
//            {
//                case "USED":
//                    view = "InvoiceUsed";
//                    break;
//                case "UNUSED":
//                    view = "InvoiceUnused";
//                    break;
//            }

//            String body = Utilities.RenderViewToString(this.ControllerContext, view, dto);
//            using (MemoryStream stream = new System.IO.MemoryStream())
//            {
//                using (var pdfWriter = new PdfWriter(stream))
//                {
//                    PdfDocument pdf = new PdfDocument(pdfWriter);
//                    Document document = new Document(pdf, PageSize.A4.Rotate());
//                    HtmlConverter.ConvertToPdf(body, pdf, null);
//                    byte[] file = stream.ToArray();
//                    MemoryStream output = new MemoryStream();
//                    output.Write(file, 0, file.Length);
//                    output.Position = 0;

//                    Response.AddHeader("content-disposition", "inline; filename=form.pdf");
//                    // Return the output stream
//                    return File(output, "application/pdf");
//                }
//            }
//        }

//        public async Task<ActionResult> ExportListToExcel(string type)
//        {
//            string warehouseAccess = Session["warehouseAccess"].ToString();
//            try
//            {
//                if (!string.IsNullOrEmpty(type))
//                {
//                    switch (type)
//                    {
//                        case "used":
//                            ExportRentListToExcel(Constant.AgingType.USED.ToString());
//                            break;
//                        case "unused":
//                            return View("CreateUnused");
//                        case "accident":
//                            return View("CreateAccident");
//                    }
//                }
//                else
//                {
//                    throw new Exception();
//                }
//            }
//            catch (Exception)
//            {

//            }

//            return RedirectToAction("Index");

//        }

//        public async Task<ActionResult> ExportDetailListToExcel(string type)
//        {
//            string warehouseAccess = Session["warehouseAccess"].ToString();
//            try
//            {
//                if (!string.IsNullOrEmpty(type))
//                {
//                    switch (type)
//                    {
//                        case "used":
//                            ExportUsedDetailListToExcel();
//                            break;
//                        case "unused":
//                            return View("CreateUnused");
//                        case "accident":
//                            return View("CreateAccident");
//                    }
//                }
//                else
//                {
//                    throw new Exception();
//                }
//            }
//            catch (Exception)
//            {

//            }

//            return RedirectToAction("Index");

//        }

//        public void ExportRentListToExcel(string agingType)
//        {
//            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
//            String fileName = String.Format("filename=Facelift_Invoice_Used_Pallet_{0}.xlsx", date);
//            string warehouseId = Session["warehouseAccess"].ToString();
//            IEnumerable<TrxBillingRentHistoryHeader> list = IBillingHistories.GetRentData(warehouseId, agingType);
//            ExcelPackage excel = new ExcelPackage();
//            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
//            workSheet.TabColor = System.Drawing.Color.Black;


//            workSheet.Row(1).Height = 25;
//            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
//            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
//            workSheet.Row(1).Style.Font.Bold = true;
//            workSheet.Cells[1, 1].Value = "Transaction Code";
//            workSheet.Cells[1, 2].Value = "Warehouse Code";
//            workSheet.Cells[1, 3].Value = "Warehouse Name";
//            workSheet.Cells[1, 4].Value = "Created By";
//            workSheet.Cells[1, 5].Value = "Created At";
//            workSheet.Cells[1, 6].Value = "Start Period";
//            workSheet.Cells[1, 7].Value = "Last Period";
//            workSheet.Cells[1, 8].Value = "Total Pallet";
//            workSheet.Cells[1, 9].Value = "Total Price";
//            workSheet.Cells[1, 10].Value = "Tax";
//            workSheet.Cells[1, 11].Value = "Grand Total";

//            int recordIndex = 2;
//            foreach (TrxBillingRentHistoryHeader header in list)
//            {
//                double? total = IBillingHistories.GetTotalRentPrice(header.TransactionId);
//                string totalPrice = total != null && total > 0 ? Utilities.FormatDoubleToThousand(total) : "0";
//                double tax = header.Tax / 100.0;
//                double? grandTotal = total + (total * tax);
//                string GrandTotal = grandTotal != null && grandTotal > 0 ? Utilities.FormatDoubleToThousand(grandTotal) : "0";

//                workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
//                workSheet.Cells[recordIndex, 2].Value = header.WarehouseCode;
//                workSheet.Cells[recordIndex, 3].Value = header.WarehouseName;
//                workSheet.Cells[recordIndex, 4].Value = header.CreatedBy;
//                workSheet.Cells[recordIndex, 5].Value = Utilities.NullDateTimeToString(header.CreatedAt);
//                workSheet.Cells[recordIndex, 6].Value = Utilities.NullDateToString(header.StartPeriod);
//                workSheet.Cells[recordIndex, 7].Value = Utilities.NullDateToString(header.LastPeriod);                
//                workSheet.Cells[recordIndex, 8].Value = Utilities.FormatThousand(header.TrxBillingRentHistoryItems.Count());
//                workSheet.Cells[recordIndex, 9].Value = totalPrice;
//                workSheet.Cells[recordIndex, 10].Value = header.Tax;
//                workSheet.Cells[recordIndex, 11].Value = GrandTotal;
//                recordIndex++;
//            }

//            for (int i = 1; i <= 11; i++)
//            {
//                workSheet.Column(i).AutoFit();
//            }

//            using (var memoryStream = new MemoryStream())
//            {
//                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
//                Response.AddHeader("content-disposition", "attachment;" + fileName);
//                excel.SaveAs(memoryStream);
//                memoryStream.WriteTo(Response.OutputStream);
//                Response.Flush();
//                Response.End();
//            }
//        }

//        public async Task ExportUsedDetailListToExcel()
//        {
//            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
//            String fileName = String.Format("filename=Facelift_Invoice_Used_Pallet_Details_{0}.xlsx", date);
//            string warehouseId = Session["warehouseAccess"].ToString();
//            IEnumerable<TrxBillingRentHistoryHeader> list = IBillingHistories.GetRentData(warehouseId, "");
//            ExcelPackage excel = new ExcelPackage();
//            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
//            workSheet.TabColor = System.Drawing.Color.Black;


//            workSheet.Row(1).Height = 25;
//            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
//            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
//            workSheet.Row(1).Style.Font.Bold = true;
//            workSheet.Cells[1, 1].Value = "Transaction Code";
//            workSheet.Cells[1, 2].Value = "Warehouse Code";
//            workSheet.Cells[1, 3].Value = "Warehouse Name";
//            workSheet.Cells[1, 4].Value = "Created By";
//            workSheet.Cells[1, 5].Value = "Created At";
//            workSheet.Cells[1, 6].Value = "Start Period";
//            workSheet.Cells[1, 7].Value = "Last Period";
//            workSheet.Cells[1, 8].Value = "Total Pallet";
//            workSheet.Cells[1, 9].Value = "Tag Id";
//            workSheet.Cells[1, 10].Value = "Year";
//            workSheet.Cells[1, 11].Value = "Month";
//            workSheet.Cells[1, 12].Value = "Total Minutes";
//            workSheet.Cells[1, 13].Value = "Total Hours";
//            workSheet.Cells[1, 14].Value = "Total Days";
//            workSheet.Cells[1, 15].Value = "Pallet Age (Year)";
//            workSheet.Cells[1, 16].Value = "Price / Day";
//            workSheet.Cells[1, 17].Value = "Total Price";
//            workSheet.Cells[1, 18].Value = "Tax";
//            workSheet.Cells[1, 19].Value = "Grand Total";

//            int recordIndex = 2;
//            try
//            {

//                foreach (TrxBillingRentHistoryHeader header in list)
//                {
//                    double? total = IBillingHistories.GetTotalRentPrice(header.TransactionId);
//                    string totalPrice = total != null && total > 0 ? Utilities.FormatDoubleToThousand(total) : "0";
//                    double disc = header.Tax / 100.0;
//                    double? grandTotal = total - (total * disc);
//                    string GrandTotal = grandTotal != null && grandTotal > 0 ? Utilities.FormatDoubleToThousand(grandTotal) : "0";

//                    foreach (TrxBillingRentHistoryItem item in header.TrxBillingRentHistoryItems)
//                    {
//                        //get detail
//                        VwBillingRentHistoryItem data = await IBillingHistories.GetRentDetailByIdAsync(item.TransactionItemId);

//                        if (data != null)
//                        {
//                            workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
//                            workSheet.Cells[recordIndex, 2].Value = header.WarehouseCode;
//                            workSheet.Cells[recordIndex, 3].Value = header.WarehouseName;
//                            workSheet.Cells[recordIndex, 4].Value = header.CreatedBy;
//                            workSheet.Cells[recordIndex, 5].Value = Utilities.NullDateTimeToString(header.CreatedAt);
//                            workSheet.Cells[recordIndex, 6].Value = Utilities.NullDateToString(header.StartPeriod);
//                            workSheet.Cells[recordIndex, 7].Value = Utilities.NullDateToString(header.LastPeriod);
//                            workSheet.Cells[recordIndex, 8].Value = Utilities.FormatThousand(header.TrxBillingRentHistoryItems.Count());
//                            workSheet.Cells[recordIndex, 9].Value = data.PalletId;
//                            workSheet.Cells[recordIndex, 10].Value = data.CurrentYear;
//                            workSheet.Cells[recordIndex, 11].Value = data.CurrentMonth;
//                            workSheet.Cells[recordIndex, 12].Value = Utilities.FormatThousand(data.TotalMinutes);
//                            workSheet.Cells[recordIndex, 13].Value = Utilities.FormatDoubleToThousand(data.TotalHours);
//                            workSheet.Cells[recordIndex, 14].Value = Utilities.FormatDoubleToThousand(data.TotalDays);
//                            workSheet.Cells[recordIndex, 15].Value = data.BillingYear;
//                            workSheet.Cells[recordIndex, 16].Value = data.BillingPrice;
//                            workSheet.Cells[recordIndex, 17].Value = totalPrice;
//                            workSheet.Cells[recordIndex, 18].Value = header.Tax;
//                            workSheet.Cells[recordIndex, 19].Value = GrandTotal;
//                            recordIndex++;
//                        }

//                    }

//                }

//                for (int i = 1; i <= 19; i++)
//                {
//                    workSheet.Column(i).AutoFit();
//                }

//                using (var memoryStream = new MemoryStream())
//                {
//                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
//                    Response.AddHeader("content-disposition", "attachment;" + fileName);
//                    excel.SaveAs(memoryStream);
//                    memoryStream.WriteTo(Response.OutputStream);
//                    Response.Flush();
//                    Response.End();
//                }
//            }
//            catch(Exception e)
//            {

//            }

            
//        }

//    }
//}