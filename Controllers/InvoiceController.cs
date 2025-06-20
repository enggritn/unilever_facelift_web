using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Facelift_App.Validators;
using iText.Html2pdf;
using iText.Kernel.Utils;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using NLog;
using NPOI.Util;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class InvoiceController : Controller
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public InvoiceController()
        {
        }

        // GET: Invoice
        public ActionResult Index()
        {
            IQueryable<MsWarehouse> query = db.MsWarehouses
                   .Where(m => m.IsActive == true)
                   .OrderBy(m => m.WarehouseCode);

            ViewBag.WarehouseList = query.ToList();

            int allowance = Convert.ToInt32(ConfigurationManager.AppSettings["damage_loss_billing_treshold"].ToString());
            int pricePerPallet = Convert.ToInt32(ConfigurationManager.AppSettings["damage_loss_billing_unit_price"].ToString());

            ViewBag.AllowanceDL = allowance;
            ViewBag.PriceDL = Utilities.FormatThousand(pricePerPallet);
            return View();
        }

        [HttpPost]
        public ActionResult DatatableRent()
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            IEnumerable<TrxInvoiceRentHeader> list = Enumerable.Empty<TrxInvoiceRentHeader>();
            IQueryable<TrxInvoiceRentHeader> query = null;
            IEnumerable<InvoiceRentDTO> pagedData = Enumerable.Empty<InvoiceRentDTO>();

            int recordsTotal = 0;
            int recordsFilteredTotal = 0;


            try
            {
                query = db.TrxInvoiceRentHeaders.AsQueryable();


                query = query
                        .Where(m => m.TransactionCode.Contains(search)
                        );

                recordsTotal = query.Count();

                //columns sorting
                Dictionary<string, Func<TrxInvoiceRentHeader, object>> cols = new Dictionary<string, Func<TrxInvoiceRentHeader, object>>();
                cols.Add("TransactionId", x => x.TransactionId);
                cols.Add("TransactionCode", x => x.TransactionCode);
                cols.Add("CurrentMonth", x => x.CurrentMonth);
                cols.Add("CurrentYear", x => x.CurrentYear);
                cols.Add("Tax", x => x.Tax);
                cols.Add("Remarks", x => x.Remarks);
                cols.Add("CreatedBy", x => x.CreatedBy);
                cols.Add("CreatedAt", x => x.CreatedAt);



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
                                select new InvoiceRentDTO
                                {
                                    TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
                                    TransactionCode = x.TransactionCode,
                                    CurrentMonth = x.CurrentMonth.ToString(),
                                    CurrentYear = x.CurrentYear.ToString(),
                                    Tax = Utilities.FormatDecimalToThousand(x.Tax),
                                    Remarks = x.Remarks,
                                    CreatedBy = x.CreatedBy,
                                    CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt)
                                };
                }

            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }


            string generatedDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");

            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateRent(InvoiceRentVM dataVM)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            bool result = false;
            string message = "Invalid form submission.";


            //server validation


            var allowedYear = Enumerable.Range(2020, DateTime.Now.Year);
            var allowedMonth = Enumerable.Range(1, 12);

            if (!allowedYear.Contains(dataVM.CurrentYear))
            {
                ModelState.AddModelError("CurrentYear", "Not allowed year.");
            }

            if (!allowedMonth.Contains(dataVM.CurrentMonth))
            {
                ModelState.AddModelError("CurrentMonth", "Not allowed month.");
            }

            //check if data already exist for current month and year, if already exist cannot create same invoice
            TrxInvoiceRentHeader prevData = db.TrxInvoiceRentHeaders.Where(m => m.CurrentMonth == dataVM.CurrentMonth && m.CurrentYear == dataVM.CurrentYear).FirstOrDefault();
            if (prevData != null)
            {
                ModelState.AddModelError("CurrentYear", "");
                ModelState.AddModelError("CurrentMonth", "");
                message = "Invoice already created for selected year and month.";
            }
            else
            {
                DateTime now = DateTime.Now;
                int currentMonth = now.Month;
                int currentYear = now.Year;

                if (dataVM.CurrentMonth == currentMonth && dataVM.CurrentYear == currentYear)
                {
                    ModelState.AddModelError("CurrentYear", "");
                    ModelState.AddModelError("CurrentMonth", "");
                    message = string.Format("Cannot create invoice for current month, please create invoice on the next month.");
                }
            }


            //check billing exist or not

            double? total = db.VwInvoiceRents.Where(m => m.CurrentMonth == dataVM.CurrentMonth && m.CurrentYear == dataVM.CurrentYear).Sum(i => i.TotalBillingRent) ?? 0;
            if (total == null || total < 1)
            {
                ModelState.AddModelError("CurrentYear", "");
                ModelState.AddModelError("CurrentMonth", "");
                message = "No billing for selected year and month.";
            }

            if (ModelState.IsValid)
            {

                TrxInvoiceRentHeader data = new TrxInvoiceRentHeader
                {
                    TransactionId = Utilities.CreateGuid("INV"),
                    CurrentYear = dataVM.CurrentYear,
                    CurrentMonth = dataVM.CurrentMonth,
                    Tax = dataVM.Tax,
                    CreatedBy = Session["username"].ToString(),
                    CreatedAt = DateTime.Now,
                    Remarks = dataVM.Remarks
                };

                string prefix = data.TransactionId.Substring(0, 3);
                int year = Convert.ToInt32(data.CreatedAt.Year.ToString().Substring(2));
                int month = data.CreatedAt.Month;
                string romanMonth = Utilities.ConvertMonthToRoman(dataVM.CurrentMonth);

                //data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}/{5}", prefix, palletOwner, warehouseAlias, data.AgingType ,year, romanMonth);
                data.TransactionCode = string.Format("{0}-{1}/{2}/{3}", prefix, "RENT", year, romanMonth);

                db.TrxInvoiceRentHeaders.Add(data);

                await db.SaveChangesAsync();

                result = true;
                message = "Create invoice succeeded.";
                TempData["TempMessage"] = message;
                response.Add("transactionId", Utilities.EncodeTo64(Encryptor.Encrypt(data.TransactionId, Constant.facelift_encryption_key)));

            }


            response.Add("stat", result);
            response.Add("msg", message);

            return Json(response);
        }


        // PDF
        public ActionResult InvoiceRent(InvoiceRentDetailDTO data)
        {
            return View(data);
        }


        public ActionResult PDFRent(string x)
        {
            ViewBag.TempMessage = TempData["TempMessage"];
            string id = "";
            try
            {

                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception("Parameter is required.");
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                ViewBag.Id = x;
                //check type

                TrxInvoiceRentHeader header = db.TrxInvoiceRentHeaders.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();

                InvoiceRentDTO headerDTO = new InvoiceRentDTO();

                headerDTO.TransactionId = header.TransactionId;
                headerDTO.TransactionCode = header.TransactionCode;
                headerDTO.CurrentMonth = header.CurrentMonth.ToString();
                headerDTO.CurrentYear = header.CurrentYear.ToString();
                headerDTO.Tax = header.Tax.ToString();
                headerDTO.Remarks = header.Remarks;
                headerDTO.CreatedBy = header.CreatedBy;
                headerDTO.CreatedAt = Utilities.NullDateToString(header.CreatedAt);



                IQueryable<VwInvoiceRent> query = db.VwInvoiceRents.Where(m => m.CurrentMonth.Equals(header.CurrentMonth) && m.CurrentYear.Equals(header.CurrentYear)).OrderBy(m => m.WarehouseCode).AsQueryable();
                IEnumerable<VwInvoiceRent> details = query.ToList();

                headerDTO.list = from detail in details
                                 select new InvoiceRentDetailDTO
                                 {
                                     WarehouseCode = detail.WarehouseCode,
                                     WarehouseName = detail.WarehouseName,
                                     WarehouseCategory = detail.CategoryName,
                                     CurrentMonth = detail.CurrentMonth.ToString(),
                                     CurrentYear = detail.CurrentYear.ToString(),
                                     TotalBillingRent = Utilities.FormatDoubleToThousand(detail.TotalBillingRent)
                                 };


                double? totalBilling = query.Sum(m => m.TotalBillingRent).Value;
                double tax = header.Tax / 100.0;
                double? grandTotal = totalBilling + (totalBilling * tax);

                headerDTO.TotalBilling = Utilities.FormatDoubleToThousand(totalBilling);
                headerDTO.GrandTotal = Utilities.FormatDoubleToThousand(grandTotal);

                string Domain = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

                ViewBag.Logo = Domain + "/Content/img/logo_black.png";
                ViewBag.Approved = Domain + "/Content/img/approved.png";

                string view = "InvoiceRent";

                String body = Utilities.RenderViewToString(this.ControllerContext, view, headerDTO);

                List<string> bodies = new List<string>();

               

                bodies.Add(body);

                foreach(VwInvoiceRent detail in details)
                {
                    headerDTO.WarehouseCode = detail.WarehouseCode;
                    headerDTO.WarehouseName = detail.WarehouseName;
                    string wh = detail.WarehouseCode + "_" + detail.WarehouseName;

                    headerDTO.list = null;

                    totalBilling = query.Where(m => m.WarehouseCode.Equals(detail.WarehouseCode)).Sum(m => m.TotalBillingRent).Value;
                    tax = header.Tax / 100.0;
                    grandTotal = totalBilling + (totalBilling * tax);

                    headerDTO.TotalBilling = Utilities.FormatDoubleToThousand(totalBilling);
                    headerDTO.GrandTotal = Utilities.FormatDoubleToThousand(grandTotal);

                    Domain = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

                    ViewBag.Logo = Domain + "/Content/img/logo_black.png";
                    ViewBag.Approved = Domain + "/Content/img/approved.png";

                    view = "InvoiceRentEach";

                    body = Utilities.RenderViewToString(this.ControllerContext, view, headerDTO);
                    bodies.Add(body);
                }

                using (MemoryStream stream = new System.IO.MemoryStream())
                {
                    using (var pdfWriter = new PdfWriter(stream))
                    {
                        PdfDocument pdf = new PdfDocument(pdfWriter);
                        PdfMerger merger = new PdfMerger(pdf);
                        //loop in here, try
                        foreach (string bod in bodies)
                        {
                            ByteArrayOutputStream baos = new ByteArrayOutputStream();
                            PdfDocument temp = new PdfDocument(new PdfWriter(baos));
                            PageSize pageSize = new PageSize(PageSize.A4);
                            Document document = new Document(temp, pageSize);
                            PdfPage page = temp.AddNewPage(pageSize);
                            HtmlConverter.ConvertToPdf(bod, temp, null);
                            temp = new PdfDocument(new PdfReader(new ByteArrayInputStream(baos.ToByteArray())));
                            merger.Merge(temp, 1, temp.GetNumberOfPages());
                            temp.Close();
                        }
                        pdf.Close();

                        byte[] file = stream.ToArray();
                        MemoryStream output = new MemoryStream();
                        output.Write(file, 0, file.Length);
                        output.Position = 0;


                        //PdfDocument pdf = new PdfDocument(pdfWriter);
                        //Document document = new Document(pdf, PageSize.A4);
                        //HtmlConverter.ConvertToPdf(body, pdf, null);
                        //byte[] file = stream.ToArray();
                        //MemoryStream output = new MemoryStream();
                        //output.Write(file, 0, file.Length);
                        //output.Position = 0;

                        Response.AddHeader("content-disposition", "inline; filename=form.pdf");
                        // Return the output stream
                        return File(output, "application/pdf");
                    }
                }


            }
            catch (Exception e)
            {

            }

            return Content(@"<body>
                            <script type='text/javascript'>
                                window.close();
                            </script>
                            </body> ");

        }

        public ActionResult InvoiceRentMGV(InvoiceRentDetailDTO data)
        {
            return View(data);
        }


        public ActionResult PDFRentMGV(string x)
        {
            ViewBag.TempMessage = TempData["TempMessage"];
            string id = "";
            try
            {

                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception("Parameter is required.");
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                ViewBag.Id = x;
                //check type

                TrxInvoiceRentHeader header = db.TrxInvoiceRentHeaders.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();

                InvoiceRentDTO headerDTO = new InvoiceRentDTO();

                headerDTO.TransactionId = header.TransactionId;
                headerDTO.TransactionCode = header.TransactionCode;
                headerDTO.CurrentMonth = header.CurrentMonth.ToString();
                headerDTO.CurrentYear = header.CurrentYear.ToString();
                headerDTO.Tax = header.Tax.ToString();
                headerDTO.MGV = "98";
                headerDTO.Remarks = header.Remarks;
                headerDTO.CreatedBy = header.CreatedBy;
                headerDTO.CreatedAt = Utilities.NullDateToString(header.CreatedAt);



                IQueryable<VwInvoiceRent> query = db.VwInvoiceRents.Where(m => m.CurrentMonth.Equals(header.CurrentMonth) && m.CurrentYear.Equals(header.CurrentYear)).OrderBy(m => m.WarehouseCode).AsQueryable();
                IEnumerable<VwInvoiceRent> details = query.ToList();

                headerDTO.list = from detail in details
                                 select new InvoiceRentDetailDTO
                                 {
                                     WarehouseCode = detail.WarehouseCode,
                                     WarehouseName = detail.WarehouseName,
                                     WarehouseCategory = detail.CategoryName,
                                     CurrentMonth = detail.CurrentMonth.ToString(),
                                     CurrentYear = detail.CurrentYear.ToString(),
                                     TotalBillingRent = Utilities.FormatDoubleToThousand(detail.TotalBillingMGV)
                                 };


                double? totalBilling = query.Sum(m => m.TotalBillingMGV).Value;
                double tax = header.Tax / 100.0;
                double? grandTotal = totalBilling + (totalBilling * tax);

                headerDTO.TotalBilling = Utilities.FormatDoubleToThousand(totalBilling);
                headerDTO.GrandTotal = Utilities.FormatDoubleToThousand(grandTotal);

                string Domain = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

                ViewBag.Logo = Domain + "/Content/img/logo_black.png";
                ViewBag.Approved = Domain + "/Content/img/approved.png";

                string view = "InvoiceRentMGV";

                String body = Utilities.RenderViewToString(this.ControllerContext, view, headerDTO);

                List<string> bodies = new List<string>();



                bodies.Add(body);

                foreach (VwInvoiceRent detail in details)
                {
                    headerDTO.WarehouseCode = detail.WarehouseCode;
                    headerDTO.WarehouseName = detail.WarehouseName;
                    string wh = detail.WarehouseCode + "_" + detail.WarehouseName;

                    headerDTO.list = null;

                    totalBilling = query.Where(m => m.WarehouseCode.Equals(detail.WarehouseCode)).Sum(m => m.TotalBillingMGV).Value;
                    tax = header.Tax / 100.0;
                    grandTotal = totalBilling + (totalBilling * tax);

                    headerDTO.TotalBilling = Utilities.FormatDoubleToThousand(totalBilling);
                    headerDTO.GrandTotal = Utilities.FormatDoubleToThousand(grandTotal);

                    Domain = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

                    ViewBag.Logo = Domain + "/Content/img/logo_black.png";
                    ViewBag.Approved = Domain + "/Content/img/approved.png";

                    view = "InvoiceRentEach";

                    body = Utilities.RenderViewToString(this.ControllerContext, view, headerDTO);
                    bodies.Add(body);
                }

                using (MemoryStream stream = new System.IO.MemoryStream())
                {
                    using (var pdfWriter = new PdfWriter(stream))
                    {
                        PdfDocument pdf = new PdfDocument(pdfWriter);
                        PdfMerger merger = new PdfMerger(pdf);
                        //loop in here, try
                        foreach (string bod in bodies)
                        {
                            ByteArrayOutputStream baos = new ByteArrayOutputStream();
                            PdfDocument temp = new PdfDocument(new PdfWriter(baos));
                            PageSize pageSize = new PageSize(PageSize.A4);
                            Document document = new Document(temp, pageSize);
                            PdfPage page = temp.AddNewPage(pageSize);
                            HtmlConverter.ConvertToPdf(bod, temp, null);
                            temp = new PdfDocument(new PdfReader(new ByteArrayInputStream(baos.ToByteArray())));
                            merger.Merge(temp, 1, temp.GetNumberOfPages());
                            temp.Close();
                        }
                        pdf.Close();

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
            catch (Exception e)
            {

            }

            return Content(@"<body>
                            <script type='text/javascript'>
                                window.close();
                            </script>
                            </body> ");
        }

        //public ActionResult EmailRent(string x)
        //{
        //    string message = "";
        //    string id = "";
        //    try
        //    {

        //        if (string.IsNullOrEmpty(x))
        //        {
        //            throw new Exception("Parameter is required.");
        //        }

        //        id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
        //        ViewBag.Id = x;
        //        //check type

        //        TrxInvoiceRentHeader header = db.TrxInvoiceRentHeaders.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();

        //        InvoiceRentDTO headerDTO = new InvoiceRentDTO();

        //        headerDTO.TransactionId = header.TransactionId;
        //        headerDTO.TransactionCode = header.TransactionCode;
        //        headerDTO.CurrentMonth = header.CurrentMonth.ToString();
        //        headerDTO.CurrentYear = header.CurrentYear.ToString();
        //        headerDTO.Tax = header.Tax.ToString();
        //        headerDTO.Remarks = header.Remarks;
        //        headerDTO.CreatedBy = header.CreatedBy;
        //        headerDTO.CreatedAt = Utilities.NullDateToString(header.CreatedAt);



        //        IQueryable<VwInvoiceRent> query = db.VwInvoiceRents.Where(m => m.CurrentMonth.Equals(header.CurrentMonth) && m.CurrentYear.Equals(header.CurrentYear)).AsQueryable();
        //        IEnumerable<VwInvoiceRent> details = query.ToList();

        //        foreach (VwInvoiceRent detail in details)
        //        {
        //            //loop per warehouse
        //            //create PDF file
        //            //send PDF as attachment in email


        //            //send email multi task
        //            //Mailing mailing = new Mailing();
        //            //Task.Factory.StartNew(() => mailing.SendEmail(emails, "Facelift - Outbound Warning", body));
        //        }

        //        //headerDTO.list = from detail in details
        //        //                 select new InvoiceRentDetailDTO
        //        //                 {
        //        //                     WarehouseCode = detail.WarehouseCode,
        //        //                     WarehouseName = detail.WarehouseName,
        //        //                     WarehouseCategory = detail.CategoryName,
        //        //                     CurrentMonth = detail.CurrentMonth.ToString(),
        //        //                     CurrentYear = detail.CurrentYear.ToString(),
        //        //                     TotalBillingRent = Utilities.FormatDoubleToThousand(detail.TotalBillingRent)
        //        //                 };


        //        //double? totalBilling = query.Sum(m => m.TotalBillingRent).Value;
        //        //double tax = header.Tax / 100.0;
        //        //double? grandTotal = totalBilling + (totalBilling * tax);

        //        //headerDTO.TotalBilling = Utilities.FormatDoubleToThousand(totalBilling);
        //        //headerDTO.GrandTotal = Utilities.FormatDoubleToThousand(grandTotal);

        //        //string Domain = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

        //        //ViewBag.Logo = Domain + "/Content/img/logo_black.png";
        //        //ViewBag.Approved = Domain + "/Content/img/approved.png";

        //        //string view = "InvoiceRent";

        //        //String body = Utilities.RenderViewToString(this.ControllerContext, view, headerDTO);
        //        //using (MemoryStream stream = new System.IO.MemoryStream())
        //        //{
        //        //    using (var pdfWriter = new PdfWriter(stream))
        //        //    {
        //        //        PdfDocument pdf = new PdfDocument(pdfWriter);
        //        //        Document document = new Document(pdf, PageSize.A4);
        //        //        HtmlConverter.ConvertToPdf(body, pdf, null);
        //        //        byte[] file = stream.ToArray();
        //        //        MemoryStream output = new MemoryStream();
        //        //        output.Write(file, 0, file.Length);
        //        //        output.Position = 0;

        //        //        Response.AddHeader("content-disposition", "inline; filename=form.pdf");
        //        //        // Return the output stream
        //        //        return File(output, "application/pdf");
        //        //    }
        //        //}


        //    }
        //    catch (Exception e)
        //    {

        //    }

        //    return Content(@"<body>
        //                    <script type='text/javascript'>
        //                        window.close();
        //                    </script>
        //                    </body> ");

        //    ViewBag.TempMessage = TempData["TempMessage"];

        //}

        [HttpPost]
        public JsonResult EmailRent(string x)
        {
            bool result = false;
            string message = "";
            try
            {
                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception("Parameter is required.");
                }

                string id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                ViewBag.Id = x;
                //check type

                TrxInvoiceRentHeader header = db.TrxInvoiceRentHeaders.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();

                InvoiceRentDTO headerDTO = new InvoiceRentDTO();

                headerDTO.TransactionId = header.TransactionId;
                headerDTO.TransactionCode = header.TransactionCode;
                headerDTO.CurrentMonth = header.CurrentMonth.ToString();
                headerDTO.CurrentYear = header.CurrentYear.ToString();
                headerDTO.Tax = header.Tax.ToString();
                headerDTO.Remarks = header.Remarks;
                headerDTO.CreatedBy = header.CreatedBy;
                headerDTO.CreatedAt = Utilities.NullDateToString(header.CreatedAt);



                IQueryable<VwInvoiceRent> query = db.VwInvoiceRents.Where(m => m.CurrentMonth.Equals(header.CurrentMonth) && m.CurrentYear.Equals(header.CurrentYear)).AsQueryable();
                IEnumerable<VwInvoiceRent> details = query.ToList();

                foreach (VwInvoiceRent detail in details)
                {
                    //loop per warehouse
                    //create PDF file
                    //send PDF as attachment in email
                    //find PIC

                    MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseCode.Equals(detail.WarehouseCode)).FirstOrDefault();
                    //List<string> emails = new List<string>();
                    //MsUser pic1 = db.MsUsers.Where(m => m.Username.Equals(warehouse.PIC1)).FirstOrDefault();
                    //MsUser pic2 = db.MsUsers.Where(m => m.Username.Equals(warehouse.PIC2)).FirstOrDefault();
                    //if (pic1 != null)
                    //{
                    //    //emails.Add("tss1@sgp-dkp.com");
                    //    if (!emails.Contains(pic1.UserEmail))
                    //    {
                    //        emails.Add(pic1.UserEmail);
                    //    }

                    //}

                    //if (pic2 != null)
                    //{
                    //    if (!emails.Contains(pic2.UserEmail))
                    //    {
                    //        emails.Add(pic2.UserEmail);
                    //    }
                    //}

                    List<string> emails = new List<string>();

                    List<InvoiceEmail> invoiceEmails = db.InvoiceEmails.Where(m => m.Warehouses.Contains(detail.WarehouseCode)).ToList();
                    foreach(InvoiceEmail email in invoiceEmails)
                    {
                        emails.Add(email.Email);
                    }

                    if (emails.Count == 0)
                    {
                        throw new Exception("No recipients or PIC available, please check warehouse configuration.");
                    }

                    headerDTO.WarehouseCode = detail.WarehouseCode;
                    headerDTO.WarehouseName = detail.WarehouseName;
                    string wh = detail.WarehouseCode + "_" + detail.WarehouseName;
                    
                    headerDTO.list = null;

                    double? totalBilling = query.Where(m => m.WarehouseCode.Equals(detail.WarehouseCode)).Sum(m => m.TotalBillingMGV).Value;
                    double tax = header.Tax / 100.0;
                    double? grandTotal = totalBilling + (totalBilling * tax);

                    headerDTO.TotalBilling = Utilities.FormatDoubleToThousand(totalBilling);
                    headerDTO.GrandTotal = Utilities.FormatDoubleToThousand(grandTotal);

                    string Domain = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

                    ViewBag.Logo = Domain + "/Content/img/logo_black.png";
                    ViewBag.Approved = Domain + "/Content/img/approved.png";

                    string view = "InvoiceRentEach";

                    String body = Utilities.RenderViewToString(this.ControllerContext, view, headerDTO);
                    using (MemoryStream stream = new System.IO.MemoryStream())
                    {
                        using (var pdfWriter = new PdfWriter(stream))
                        {
                            PdfDocument pdf = new PdfDocument(pdfWriter);
                            Document document = new Document(pdf, PageSize.A4);
                            HtmlConverter.ConvertToPdf(body, pdf, null);
                            byte[] file = stream.ToArray();
                            MemoryStream output = new MemoryStream();
                            output.Write(file, 0, file.Length);
                            output.Position = 0;

                            string file_name = string.Format("Facelift_Invoice_{0}_{1}_{2}.pdf", wh, headerDTO.CurrentMonth, headerDTO.CurrentYear);

                            var path = System.IO.Path.Combine(Server.MapPath("~/Content/file"), file_name);

                            if (!System.IO.File.Exists(path))
                            {
                                using (Stream fileStream = new FileStream(path, FileMode.CreateNew))
                                {
                                    output.CopyTo(fileStream);
                                }
                            }

                            List<Attachment> attachments = new List<Attachment>();

                            attachments.Add(new Attachment(path));

                            string html = string.Format("Dear PIC {0}, <br><br> Terlampir Facelift Invoice Periode : {1} {2} <br><br> Terima kasih.", warehouse.WarehouseName, headerDTO.CurrentMonth, headerDTO.CurrentYear);

                            Mailing mailing = new Mailing();
                            Task.Factory.StartNew(() => mailing.SendEmail(emails, string.Format("Facelift_Invoice_{0}_{1}_{2}", wh, headerDTO.CurrentMonth, headerDTO.CurrentYear), html, attachments));

                        }
                    }

                  
                    result = true;
                    message = "Email sent.";


                }

            }
            catch(Exception e)
            {
                message = e.Message;
            }


            return Json(new { stat = result, msg = message });
        }


        [HttpPost]
        public ActionResult DatatableEmail()
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            IEnumerable<InvoiceEmail> list = Enumerable.Empty<InvoiceEmail>();
            IQueryable<InvoiceEmail> query = null;
            IEnumerable<InvoiceEmailDTO> pagedData = Enumerable.Empty<InvoiceEmailDTO>();

            int recordsTotal = 0;
            int recordsFilteredTotal = 0;


            try
            {
                query = db.InvoiceEmails.AsQueryable();


                query = query
                        .Where(m => m.Email.Contains(search)
                        );

                recordsTotal = query.Count();

                //columns sorting
                Dictionary<string, Func<InvoiceEmail, object>> cols = new Dictionary<string, Func<InvoiceEmail, object>>();
                cols.Add("Email", x => x.Email);


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
                                select new InvoiceEmailDTO
                                {
                                    Email = x.Email,
                                    Warehouses = getWarehouseAlias(x.Warehouses)
                                };
                }

            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }


            string generatedDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");

            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }

        private string getWarehouseAlias(string warehouses)
        {
            string[] wh = warehouses.Split(',');
            List<string> whs = new List<string>();
            foreach (string warehousecode in wh)
            {
                MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseCode.Equals(warehousecode)).FirstOrDefault();
                if (warehouse != null)
                {
                    whs.Add(warehouse.WarehouseCode + " - " + warehouse.WarehouseName);
                }
            }
            return string.Join(" | ", whs);
        }

        [HttpPost]
        public JsonResult AddEmail(InvoiceEmailVM dataVM)
        {
            bool result = false;
            string message = "";
            try
            {
                if (string.IsNullOrEmpty(dataVM.UserEmail))
                {
                    throw new Exception("Email is required.");
                }
                else
                {
                    dataVM.UserEmail = dataVM.UserEmail.ToLower().Trim();
                    InvoiceEmail email = db.InvoiceEmails.Where(m => m.Email.Equals(dataVM.UserEmail)).FirstOrDefault();
                    if(email != null)
                    {
                        throw new Exception("Email already exists.");
                    }
                }

                if(dataVM.WarehouseIds == null || dataVM.WarehouseIds.Count() < 1)
                {
                    throw new Exception("Please choose at least 1 warehouse.");
                }

                List<string> warehouses = new List<string>();
                if (dataVM.WarehouseIds != null && dataVM.WarehouseIds.Count() > 0)
                {
                    foreach (string warehouseId in dataVM.WarehouseIds)
                    {
                        MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseId.Equals(warehouseId)).FirstOrDefault();
                        if(warehouse != null)
                        {
                            warehouses.Add(warehouse.WarehouseCode);
                        }
                    }
                }

                InvoiceEmail invoiceEmail = new InvoiceEmail();
                invoiceEmail.Email = dataVM.UserEmail;
                invoiceEmail.Warehouses = string.Join(",", warehouses);

                db.InvoiceEmails.Add(invoiceEmail);

                db.SaveChanges();

                result = true;
                message = "Add email succeded.";
                
            }
            catch (Exception e)
            {
                message = e.Message;
            }


            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public JsonResult DeleteEmail(InvoiceEmailVM dataVM)
        {
            bool result = false;
            string message = "";
            try
            {
                if (string.IsNullOrEmpty(dataVM.UserEmail))
                {
                    throw new Exception("Email is required.");
                }
                else
                {
                    dataVM.UserEmail = dataVM.UserEmail.ToLower().Trim();
                    InvoiceEmail email = db.InvoiceEmails.Where(m => m.Email.Equals(dataVM.UserEmail)).FirstOrDefault();
                    if (email != null)
                    {

                        db.InvoiceEmails.Remove(email);

                        db.SaveChanges();

                        result = true;
                        message = "Delete email succeded.";
                    }
                    else
                    {
                        message = "Email not found.";
                    }
                }


            }
            catch (Exception e)
            {
                message = e.Message;
            }


            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public JsonResult UpdateEmail(InvoiceEmailVM dataVM)
        {
            bool result = false;
            string message = "";
            try
            {
                InvoiceEmail email = null;
                if (string.IsNullOrEmpty(dataVM.UserEmail))
                {
                    throw new Exception("Email is required.");
                }
                else
                {
                    dataVM.UserEmail = dataVM.UserEmail.ToLower().Trim();
                    email = db.InvoiceEmails.Where(m => m.Email.Equals(dataVM.UserEmail)).FirstOrDefault();
                    if (email == null)
                    {
                        throw new Exception("Email not found.");
                    }
                }

                if (dataVM.WarehouseIds == null || dataVM.WarehouseIds.Count() < 1)
                {
                    throw new Exception("Please choose at least 1 warehouse.");
                }

                List<string> warehouses = new List<string>();
                if (dataVM.WarehouseIds != null && dataVM.WarehouseIds.Count() > 0)
                {
                    foreach (string warehouseId in dataVM.WarehouseIds)
                    {
                        MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseId.Equals(warehouseId)).FirstOrDefault();
                        if (warehouse != null)
                        {
                            warehouses.Add(warehouse.WarehouseCode);
                        }
                    }
                }

                email.Warehouses = string.Join(",", warehouses);

                db.SaveChanges();

                result = true;
                message = "Update email succeded.";

            }
            catch (Exception e)
            {
                message = e.Message;
            }


            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public JsonResult GetEmailById(string Id)
        {
            bool result = false;
            string message = "";
            InvoiceEmailDTO data = null;
            List<WarehouseDTO> unSelectedWarehouse = new List<WarehouseDTO>();
            List<WarehouseDTO> selectedWarehouse = new List<WarehouseDTO>();
            try
            {
                InvoiceEmail email = null;
                if (string.IsNullOrEmpty(Id))
                {
                    throw new Exception("Email is required.");
                }
                else
                {
                    email = db.InvoiceEmails.Where(m => m.Email.Equals(Id)).FirstOrDefault();
                    if (email == null)
                    {
                        throw new Exception("Email not found.");
                    }
                }

                data = new InvoiceEmailDTO()
                {
                    Email = email.Email
                };

                data.WarehouseIds = email.Warehouses.Split(',');
                foreach (string warehouseCode in data.WarehouseIds)
                {
                    MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseCode.Equals(warehouseCode)).FirstOrDefault();
                    if(warehouse != null)
                    {
                        WarehouseDTO warehouseDTO = new WarehouseDTO();
                        warehouseDTO.WarehouseId = warehouse.WarehouseId;
                        warehouseDTO.WarehouseCode = warehouse.WarehouseCode;
                        warehouseDTO.WarehouseName = warehouse.WarehouseName;
                        selectedWarehouse.Add(warehouseDTO);
                    }
                }

                IQueryable<MsWarehouse> query = db.MsWarehouses
                  .Where(m => !data.WarehouseIds.Contains(m.WarehouseCode))
                  .OrderBy(m => m.WarehouseCode);

                foreach(MsWarehouse wh in query.ToList())
                {
                    WarehouseDTO warehouseDTO = new WarehouseDTO();
                    warehouseDTO.WarehouseId = wh.WarehouseId;
                    warehouseDTO.WarehouseCode = wh.WarehouseCode;
                    warehouseDTO.WarehouseName = wh.WarehouseName;

                    unSelectedWarehouse.Add(warehouseDTO);
                }


                result = true;
                message = "Fetch data succeded.";

            }
            catch (Exception e)
            {
                message = e.Message;
            }


            return Json(new { stat = result, msg = message, data = data, unSelectedWarehouse = unSelectedWarehouse, selectedWarehouse = selectedWarehouse });
        }


        ////damage loss

        private int CalculateDeffectAllowanceQty(int totalPallet, int allowance)
        {
            double percentage = allowance / 100.0;
            return Convert.ToInt32(totalPallet * percentage);
        }

        //private int CalculateDeffectTotalExceededQty(int totalDeffect, int totalPallet, int allowance)
        //{
        //    return totalDeffect - CalculateDeffectAllowanceQty(totalPallet, allowance);
        //}

        //private int CalculateDeffectExceededQty(int totalDeffect, int lastExceed, int totalPallet, int allowance)
        //{
        //    int ExceedQty = CalculateDeffectTotalExceededQty(totalDeffect, totalPallet, allowance) - lastExceed;
        //    return ExceedQty > 0 ? ExceedQty : 0;
        //}

        private decimal CalculateDeffectBilling(decimal pricePerPallet, int exceedQty)
        {
            return exceedQty > 0 ? pricePerPallet * exceedQty : 0;
        }

        private decimal CalculateDeffectTotalBilling(decimal pricePerPallet, int exceedQty, int tax)
        {
            decimal billing = CalculateDeffectBilling(pricePerPallet, exceedQty);
            double percentage = tax / 100.0;
            decimal discount = billing * Convert.ToDecimal(percentage);
            return exceedQty > 0 ? billing - discount : 0;
        }

        [HttpPost]
        public ActionResult DatatableDeffect()
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            IEnumerable<TrxInvoiceDeffect> list = Enumerable.Empty<TrxInvoiceDeffect>();
            IQueryable<TrxInvoiceDeffect> query = null;
            IEnumerable<InvoiceDeffectDTO> pagedData = Enumerable.Empty<InvoiceDeffectDTO>();

            int recordsTotal = 0;
            int recordsFilteredTotal = 0;


            try
            {
                query = db.TrxInvoiceDeffects.AsQueryable();


                query = query
                        .Where(m => m.TransactionCode.Contains(search)
                        );

                recordsTotal = query.Count();

                //columns sorting
                Dictionary<string, Func<TrxInvoiceDeffect, object>> cols = new Dictionary<string, Func<TrxInvoiceDeffect, object>>();
                cols.Add("TransactionId", x => x.TransactionId);
                cols.Add("TransactionCode", x => x.TransactionCode);
                cols.Add("CurrentMonth", x => x.CurrentMonth);
                cols.Add("CurrentYear", x => x.CurrentYear);
                cols.Add("Allowance", x => x.Allowance);
                cols.Add("TotalPallet", x => x.TotalPallet);
                cols.Add("CurrentDeffect", x => x.CurrentDeffect);
                cols.Add("PreviousDeffect", x => x.PreviousDeffect);
                cols.Add("TotalDeffect", x => x.TotalDeffect);
                cols.Add("AllowanceQty", x => x.AllowanceQty);
                cols.Add("TotalExceed", x => x.TotalExceed);
                cols.Add("LastExceed", x => x.LastExceed);
                cols.Add("ExceedQty", x => x.ExceedQty);
                cols.Add("PricePerPallet", x => x.PricePerPallet);
                cols.Add("TotalBilling", x => CalculateDeffectBilling(x.PricePerPallet, x.ExceedQty));
                cols.Add("Tax", x => x.Tax);
                cols.Add("GrandTotal", x => CalculateDeffectTotalBilling(x.PricePerPallet, x.ExceedQty, x.Tax));
                cols.Add("Remarks", x => x.Remarks);
                cols.Add("CreatedBy", x => x.CreatedBy);
                cols.Add("CreatedAt", x => x.CreatedAt);



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
                                select new InvoiceDeffectDTO
                                {
                                    TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.TransactionId, Constant.facelift_encryption_key)),
                                    TransactionCode = x.TransactionCode,
                                    CurrentMonth = x.CurrentMonth.ToString(),
                                    CurrentYear = x.CurrentYear.ToString(),
                                    Allowance = Utilities.FormatThousand(x.Allowance),
                                    TotalPallet = Utilities.FormatThousand(x.TotalPallet),
                                    CurrentDeffect = Utilities.FormatThousand(x.CurrentDeffect),
                                    PreviousDeffect = Utilities.FormatThousand(x.PreviousDeffect),
                                    TotalDeffect = Utilities.FormatThousand(x.TotalDeffect),
                                    AllowanceQty = Utilities.FormatDecimalToThousand(x.AllowanceQty),
                                    TotalExceed = Utilities.FormatDecimalToThousand(x.TotalExceed),
                                    LastExceed = Utilities.FormatDecimalToThousand(x.LastExceed),
                                    ExceedQty = Utilities.FormatDecimalToThousand(x.ExceedQty),
                                    PricePerPallet = Utilities.FormatDecimalToThousand(x.PricePerPallet),
                                    TotalBilling = Utilities.FormatDecimalToThousand(CalculateDeffectBilling(x.PricePerPallet, x.ExceedQty)),
                                    Tax = Utilities.FormatDecimalToThousand(x.Tax),
                                    GrandTotal = Utilities.FormatDecimalToThousand(CalculateDeffectTotalBilling(x.PricePerPallet, x.ExceedQty, x.Tax)),
                                    Remarks = x.Remarks,
                                    CreatedBy = x.CreatedBy,
                                    CreatedAt = Utilities.NullDateTimeToString(x.CreatedAt),
                                    pdfButton = x.ExceedQty > 0,
                                    excelButton = x.TrxInvoiceDeffectItems.Count() > 0
                                };
                }

            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }


            string generatedDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");

            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateDeffect(InvoiceDeffectVM dataVM)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            bool result = false;
            string message = "Invalid form submission.";


            //server validation


            try
            {
                var allowedYear = Enumerable.Range(2021, DateTime.Now.Year);
                var allowedMonth = Enumerable.Range(1, 12);

                if (!allowedYear.Contains(dataVM.CurrentYear))
                {
                    ModelState.AddModelError("CurrentYear", "Not allowed year.");
                }

                if (!allowedMonth.Contains(dataVM.CurrentMonth))
                {
                    ModelState.AddModelError("CurrentMonth", "Not allowed month.");
                }

                //check if data already exist for current month and year, if already exist cannot create same invoice
                TrxInvoiceDeffect prevData = db.TrxInvoiceDeffects.Where(m => m.CurrentMonth == dataVM.CurrentMonth && m.CurrentYear == dataVM.CurrentYear).FirstOrDefault();
                if (prevData != null)
                {
                    ModelState.AddModelError("CurrentYear", "");
                    ModelState.AddModelError("CurrentMonth", "");
                    message = "Invoice already created for selected year and month.";
                }
                else
                {
                    //DateTime now = DateTime.Now;
                    //int currentMonth = now.Month;
                    //int currentYear = now.Year;

                    //if (dataVM.CurrentMonth == currentMonth && dataVM.CurrentYear == currentYear)
                    //{
                    //    ModelState.AddModelError("CurrentYear", "");
                    //    ModelState.AddModelError("CurrentMonth", "");
                    //    message = string.Format("Cannot create invoice for current month, please create invoice on the next month.");
                    //}
                }


                //check pallet damage/loss exist or not

                //double? total = db.VwInvoiceDeffects.Where(m => m.CurrentMonth == dataVM.CurrentMonth && m.CurrentYear == dataVM.CurrentYear).Sum(i => i.TotalBillingDeffect) ?? 0;
                //if (total == null || total < 1)
                //{
                //    ModelState.AddModelError("CurrentYear", "");
                //    ModelState.AddModelError("CurrentMonth", "");
                //    message = "No billing for selected year and month.";
                //}


                if (ModelState.IsValid)
                {
                    int totalPallet = db.MsPallets.Where(m => m.PalletOwner.Equals("DHL")).Count();
                    TrxInvoiceDeffect prevInvoice = db.TrxInvoiceDeffects.Where(m => m.CurrentYear == dataVM.CurrentYear).OrderByDescending(m => m.CreatedAt).FirstOrDefault();
                    string[] deffects = { "DAMAGE", "LOSS" };

                    IQueryable<MsPallet> palletQuery = db.MsPallets.Where(m => m.PalletOwner.Equals("DHL") && !m.IsDeleted && deffects.Contains(m.PalletCondition)).AsQueryable();

                    int currentDeffect = palletQuery.Count();

                    if(currentDeffect <= 0)
                    {
                        throw new Exception("No Damage/Loss pallet found.");
                    }

                    int prevDeffect = 0;
                    int lastExceeded = 0;

                    if (prevInvoice != null)
                    {
                        prevDeffect = prevInvoice.CurrentDeffect + prevInvoice.PreviousDeffect;
                        int prevtotalDeffect = prevInvoice.CurrentDeffect + prevInvoice.PreviousDeffect;
                        int prevallowanceQty = CalculateDeffectAllowanceQty(prevInvoice.TotalPallet, prevInvoice.Allowance);
                        int prevtotalExceeded = prevtotalDeffect - prevallowanceQty;

                        if (prevtotalExceeded > 0)
                        {
                            lastExceeded = prevtotalExceeded;
                        }
                        else
                        {
                            lastExceeded = prevInvoice.LastExceed;
                        }


                    }

                    //int actualDeffectQty = currentDeffectQty  + prevDeffectQty;
                    //int palletQty = db.MsPallets.Where(m => m.PalletCondition.Equals(dataVM.Type)).Count();
                    int allowance = Convert.ToInt32(ConfigurationManager.AppSettings["damage_loss_billing_treshold"].ToString());
                    int pricePerPallet = Convert.ToInt32(ConfigurationManager.AppSettings["damage_loss_billing_unit_price"].ToString());


                    int totalDeffect = currentDeffect + prevDeffect;
                    int allowanceQty = CalculateDeffectAllowanceQty(totalPallet, allowance);
                    int totalExceeded = totalDeffect - allowanceQty;


                    int exceedQty = totalExceeded - lastExceeded;

                    if (exceedQty < 0)
                    {
                        exceedQty = 0;
                    }



                    TrxInvoiceDeffect data = new TrxInvoiceDeffect
                    {
                        TransactionId = Utilities.CreateGuid("INV"),
                        CurrentYear = dataVM.CurrentYear,
                        CurrentMonth = dataVM.CurrentMonth,
                        Allowance = allowance,
                        TotalPallet = totalPallet,
                        CurrentDeffect = currentDeffect,
                        PreviousDeffect = prevDeffect,
                        TotalDeffect = currentDeffect + prevDeffect,
                        AllowanceQty = allowanceQty,
                        TotalExceed = totalExceeded,
                        LastExceed = lastExceeded,
                        ExceedQty = exceedQty,
                        PricePerPallet = pricePerPallet,
                        Tax = dataVM.Tax,
                        CreatedBy = Session["username"].ToString(),
                        CreatedAt = DateTime.Now,
                        Remarks = dataVM.Remarks
                    };


                    foreach (MsPallet pallet in palletQuery)
                    {
                        pallet.IsDeleted = true;

                        TrxInvoiceDeffectItem item = new TrxInvoiceDeffectItem();
                        item.TransactionId = data.TransactionId;
                        item.TransactionItemId = Utilities.CreateGuid("INVi");
                        item.PalletId = pallet.TagId;
                        item.WarehouseId = pallet.WarehouseId;
                        item.WarehouseCode = pallet.WarehouseCode;
                        item.WarehouseName = pallet.WarehouseName;
                        item.DeffectType = pallet.PalletCondition;

                        data.TrxInvoiceDeffectItems.Add(item);
                    }


                    string prefix = data.TransactionId.Substring(0, 3);
                    int year = Convert.ToInt32(dataVM.CurrentYear.ToString().Substring(2));
                    int month = data.CreatedAt.Month;
                    string romanMonth = Utilities.ConvertMonthToRoman(dataVM.CurrentMonth);

                    //data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}/{5}", prefix, palletOwner, warehouseAlias, data.AgingType ,year, romanMonth);
                    data.TransactionCode = string.Format("{0}-{1}/{2}/{3}", prefix, "DL", year, romanMonth);

                    db.TrxInvoiceDeffects.Add(data);

                    await db.SaveChangesAsync();

                    result = true;
                    message = "Create invoice succeeded.";
                    TempData["TempMessage"] = message;
                    response.Add("transactionId", Utilities.EncodeTo64(Encryptor.Encrypt(data.TransactionId, Constant.facelift_encryption_key)));

                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }


            response.Add("stat", result);
            response.Add("msg", message);

            return Json(response);
        }


        // PDF
        public ActionResult InvoiceDeffect(InvoiceDeffectDTO data)
        {
            return View(data);
        }


        public ActionResult PDFDeffect(string x)
        {
            ViewBag.TempMessage = TempData["TempMessage"];
            string id = "";
            try
            {

                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception("Parameter is required.");
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                ViewBag.Id = x;
                //check type

                TrxInvoiceDeffect header = db.TrxInvoiceDeffects.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();

                InvoiceDeffectDTO headerDTO = new InvoiceDeffectDTO();

                headerDTO.TransactionId = header.TransactionId;
                headerDTO.TransactionCode = header.TransactionCode;
                headerDTO.CurrentMonth = header.CurrentMonth.ToString();
                headerDTO.CurrentYear = header.CurrentYear.ToString();
                headerDTO.Tax = header.Tax.ToString();
                headerDTO.Remarks = header.Remarks;
                headerDTO.CreatedBy = header.CreatedBy;
                headerDTO.CreatedAt = Utilities.NullDateToString(header.CreatedAt);

                headerDTO.ExceedQty = Utilities.FormatThousand(header.ExceedQty);
                headerDTO.TotalBilling = Utilities.FormatDecimalToThousand(CalculateDeffectBilling(header.PricePerPallet, header.ExceedQty));
                headerDTO.GrandTotal = Utilities.FormatDecimalToThousand(CalculateDeffectTotalBilling(header.PricePerPallet, header.ExceedQty, header.Tax));
                headerDTO.TotalDamage = Utilities.FormatThousand(header.TrxInvoiceDeffectItems.Where(m => m.DeffectType.Equals("DAMAGE")).Count());
                headerDTO.TotalLoss = Utilities.FormatThousand(header.TrxInvoiceDeffectItems.Where(m => m.DeffectType.Equals("LOSS")).Count());



                string Domain = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

                ViewBag.Logo = Domain + "/Content/img/logo_black.png";
                ViewBag.Approved = Domain + "/Content/img/approved.png";

                string view = "InvoiceDeffect";

                String body = Utilities.RenderViewToString(this.ControllerContext, view, headerDTO);

                List<string> bodies = new List<string>();

                bodies.Add(body);


                using (MemoryStream stream = new System.IO.MemoryStream())
                {
                    using (var pdfWriter = new PdfWriter(stream))
                    {
                        PdfDocument pdf = new PdfDocument(pdfWriter);
                        PdfMerger merger = new PdfMerger(pdf);
                        //loop in here, try
                        foreach (string bod in bodies)
                        {
                            ByteArrayOutputStream baos = new ByteArrayOutputStream();
                            PdfDocument temp = new PdfDocument(new PdfWriter(baos));
                            PageSize pageSize = new PageSize(PageSize.A4);
                            Document document = new Document(temp, pageSize);
                            PdfPage page = temp.AddNewPage(pageSize);
                            HtmlConverter.ConvertToPdf(bod, temp, null);
                            temp = new PdfDocument(new PdfReader(new ByteArrayInputStream(baos.ToByteArray())));
                            merger.Merge(temp, 1, temp.GetNumberOfPages());
                            temp.Close();
                        }
                        pdf.Close();

                        byte[] file = stream.ToArray();
                        MemoryStream output = new MemoryStream();
                        output.Write(file, 0, file.Length);
                        output.Position = 0;


                        //PdfDocument pdf = new PdfDocument(pdfWriter);
                        //Document document = new Document(pdf, PageSize.A4);
                        //HtmlConverter.ConvertToPdf(body, pdf, null);
                        //byte[] file = stream.ToArray();
                        //MemoryStream output = new MemoryStream();
                        //output.Write(file, 0, file.Length);
                        //output.Position = 0;

                        Response.AddHeader("content-disposition", "inline; filename=form.pdf");
                        // Return the output stream
                        return File(output, "application/pdf");
                    }
                }


            }
            catch (Exception e)
            {

            }

            return Content(@"<body>
                            <script type='text/javascript'>
                                window.close();
                            </script>
                            </body> ");

        }


        public ActionResult ExcelDeffect(string x)
        {
            ViewBag.TempMessage = TempData["TempMessage"];
            string id = "";
            try
            {

                if (string.IsNullOrEmpty(x))
                {
                    throw new Exception("Parameter is required.");
                }

                id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                ViewBag.Id = x;
                //check type

                TrxInvoiceDeffect header = db.TrxInvoiceDeffects.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();

                String date = DateTime.Now.ToString("yyyyMMddhhmmss");
                String fileName = String.Format("filename=Facelift_Invoice_Deffect_{0}_{1}_{2}.xlsx", header.CurrentYear, header.CurrentMonth, date);

                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                workSheet.TabColor = System.Drawing.Color.Black;

                workSheet.Row(1).Height = 25;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                string[] columns = { "Transaction Code", "Year", "Month", "Created By", "Created At", "Pallet ID", "Warehouse", "Condition" };

                int colNum = 1;
                foreach(string column in columns)
                {
                    workSheet.Cells[1, colNum].Value = column;
                    colNum++;
                }

                string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };


                int recordIndex = 2;
                foreach (TrxInvoiceDeffectItem item in header.TrxInvoiceDeffectItems.OrderBy(m => m.PalletId).OrderBy(m => m.WarehouseCode).OrderBy(m => m.DeffectType))
                {
                    workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
                    workSheet.Cells[recordIndex, 2].Value = header.CurrentYear;
                    workSheet.Cells[recordIndex, 3].Value = months[header.CurrentMonth];
                    workSheet.Cells[recordIndex, 4].Value = header.CreatedBy;
                    workSheet.Cells[recordIndex, 5].Value = Utilities.NullDateTimeToString(header.CreatedAt);
                    workSheet.Cells[recordIndex, 6].Value = item.PalletId;
                    workSheet.Cells[recordIndex, 7].Value = item.WarehouseCode + " - " + item.WarehouseName;
                    workSheet.Cells[recordIndex, 8].Value = item.DeffectType;
                    recordIndex++;
                }

                for (int i = 1; i <= columns.Count(); i++)
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
            }
            catch (Exception e)
            {

            }

            return RedirectToAction("Index");

        }

    }



}