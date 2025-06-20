using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Facelift_App.Validators;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using NLog;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ZXing;
using ZXing.QrCode;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    [AuthCheck]
    public class RecallController : Controller
    {

        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public RecallController()
        {
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



            IQueryable<TrxRecallHeader> query = db.TrxRecallHeaders.AsQueryable().Where(m => m.WarehouseId.Equals(warehouseId) && m.IsDeleted == false);
            IEnumerable<TrxRecallHeader> list = Enumerable.Empty<TrxRecallHeader>();

            int recordsTotal = 0;
            try
            {
                if (!string.IsNullOrEmpty(stats))
                {
                    if (stats.Equals("0"))
                    {
                        query = query.Where(m => !m.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));
                    }
                    else if (stats.Equals("1"))
                    {
                        query = query.Where(m => m.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));
                    }
                }

                recordsTotal = query.Count();

                query = query
                        .Where(m => m.TransactionCode.Contains(search) ||
                           m.TransactionStatus.Contains(search));

                //columns sorting
                Dictionary<string, Func<TrxRecallHeader, object>> cols = new Dictionary<string, Func<TrxRecallHeader, object>>();
                cols.Add("TransactionCode", m => m.TransactionCode);
                cols.Add("WarehouseName", m => m.WarehouseName);
                cols.Add("TransactionStatus", m => m.TransactionStatus);
                cols.Add("CreatedBy", m => m.CreatedBy);
                cols.Add("CreatedAt", m => m.CreatedAt);
                cols.Add("ModifiedBy", m => m.ModifiedBy);
                cols.Add("ModifiedAt", m => m.ModifiedAt);
                cols.Add("ApprovedBy", m => m.ApprovedBy);
                cols.Add("ApprovedAt", m => m.ApprovedAt);


                if (sortDirection.Equals("asc"))
                    list = query.OrderBy(cols[sortName]);
                else
                    list = query.OrderByDescending(cols[sortName]);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            IEnumerable<RecallDTO> pagedData = Enumerable.Empty<RecallDTO>();

            
            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {

                pagedData = from m in list
                            select new RecallDTO
                            {
                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(m.TransactionId, Constant.facelift_encryption_key)),
                                TransactionCode = m.TransactionCode,
                                WarehouseName = m.WarehouseName,
                                TransactionStatus = Utilities.TransactionStatusBadge(m.TransactionStatus),
                                CreatedBy = m.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(m.CreatedAt),
                                ModifiedBy = !string.IsNullOrEmpty(m.ModifiedBy) ? m.ModifiedBy : "-",
                                ModifiedAt = Utilities.NullDateTimeToString(m.ModifiedAt),
                                ApprovedBy = !string.IsNullOrEmpty(m.ApprovedBy) ? m.ApprovedBy : "-",
                                ApprovedAt = Utilities.NullDateTimeToString(m.ApprovedAt)
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> Create()
        {
            RecallVM dataVM = new RecallVM();
            dataVM.WarehouseId = Session["warehouseAccess"].ToString();

            MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseId.Equals(dataVM.WarehouseId)).FirstOrDefault();
            dataVM.WarehouseCode = warehouse.WarehouseCode;
            dataVM.WarehouseName = warehouse.WarehouseName;

            return View(dataVM);
        }

        private async Task SaveValidation(RecallVM dataVM)
        {
            
            if (!string.IsNullOrEmpty(dataVM.WarehouseId))
            {
                ModelState["WarehouseId"].Errors.Clear();
                string WarehouseValid = db.MsWarehouses.Where(m => m.WarehouseId.Equals(dataVM.WarehouseId)).FirstOrDefault() != null ? "true" : "Warehouse not recognized.";
                if (!WarehouseValid.Equals("true"))
                {
                    ModelState.AddModelError("WarehouseId", WarehouseValid);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(RecallVM dataVM)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            bool result = false;
            string message = "Invalid form submission.";

            dataVM.WarehouseId = Session["warehouseAccess"].ToString();


            //server validation
            await SaveValidation(dataVM);


            if (ModelState.IsValid)
            {
                MsWarehouse warehouse = db.MsWarehouses.Where(m => m.WarehouseId.Equals(dataVM.WarehouseId)).FirstOrDefault();

                TrxRecallHeader data = new TrxRecallHeader
                {
                    TransactionId = Utilities.CreateGuid("RCL"),
                    Remarks = dataVM.Remarks,
                    WarehouseId = dataVM.WarehouseId,
                    WarehouseCode = warehouse.WarehouseCode,
                    WarehouseName = warehouse.WarehouseName,
                    TransactionStatus = Constant.TransactionStatus.OPEN.ToString(),
                    CreatedBy = Session["username"].ToString()
                };

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        data.CreatedAt = DateTime.Now;


                        string prefix = data.TransactionId.Substring(0, 3);
                        //string palletOwner = company.CompanyAbb;
                        string warehouseCode = data.WarehouseCode;
                        string warehouseAlias = warehouse.WarehouseAlias;
                        int year = Convert.ToInt32(data.CreatedAt.Year.ToString().Substring(2));
                        int month = data.CreatedAt.Month;
                        string romanMonth = Utilities.ConvertMonthToRoman(month);

                        // get last number, and do increment.
                        string lastNumber = db.TrxRecallHeaders.AsQueryable().OrderByDescending(m => m.CreatedAt)
                            .Where(m => m.WarehouseCode.Equals(warehouseCode) && m.CreatedAt.Year.Equals(data.CreatedAt.Year) && m.CreatedAt.Month.Equals(data.CreatedAt.Month))
                            .AsEnumerable().Select(m => m.TransactionCode).FirstOrDefault();
                        int currentNumber = 0;

                        if (!string.IsNullOrEmpty(lastNumber))
                        {
                            currentNumber = Int32.Parse(lastNumber.Substring(lastNumber.Length - 3));
                        }

                        string runningNumber = string.Format("{0:D3}", currentNumber + 1);

                        data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}", prefix, warehouseAlias, year, romanMonth, runningNumber);

                        LogRecallHeader log = new LogRecallHeader
                        {
                            LogId = Utilities.CreateGuid("LOG"),
                            TransactionId = data.TransactionId,
                            ActionName = "Create",
                            ExecutedBy = data.CreatedBy,
                            ExecutedAt = data.CreatedAt
                        };

                        LogRecallDocument logDoc = new LogRecallDocument
                        {
                            LogId = Utilities.CreateGuid("LOG"),
                            TransactionId = data.TransactionId,
                            Remarks = data.Remarks,
                            WarehouseId = data.WarehouseId,
                            WarehouseCode = data.WarehouseCode,
                            WarehouseName = data.WarehouseName,
                            TransactionStatus = data.TransactionStatus,
                            IsDeleted = data.IsDeleted,
                            CreatedBy = data.CreatedBy,
                            CreatedAt = data.CreatedAt,
                            ModifiedBy = data.ModifiedBy,
                            ModifiedAt = data.ModifiedAt,
                            ApprovedBy = data.ApprovedBy,
                            ApprovedAt = data.ApprovedAt,
                            Version = 1
                        };

                        db.TrxRecallHeaders.Add(data);
                        db.LogRecallHeaders.Add(log);
                        db.LogRecallDocuments.Add(logDoc);

                        await db.SaveChangesAsync();
                        transaction.Commit();
                        result = true;
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                        logger.Error(e, errMsg);
                    }
                }

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
            TrxRecallHeader data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = db.TrxRecallHeaders.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();
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

            RecallVM dataVM = new RecallVM
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
                logs = data.LogRecallHeaders.OrderBy(m => m.ExecutedAt).ToList(),
                versions = data.LogRecallDocuments.OrderBy(n => n.Version).ToList()
            };


            ViewBag.TransactionStatus = data.TransactionStatus;
            ViewBag.Id = x;

       
            return View(dataVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Detail(string x, RecallVM dataVM)
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

            TrxRecallHeader data = db.TrxRecallHeaders.Where(m => m.WarehouseId.Equals(warehouseId)).FirstOrDefault();
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

                if (!data.TransactionStatus.Equals(Constant.TransactionStatus.OPEN.ToString()))
                {
                    message = "Update data not allowed, document already processed.";
                    return Json(new { stat = result, msg = message });
                }
                
            }

            dataVM.WarehouseId = data.WarehouseId;

            await SaveValidation(dataVM);

            if (ModelState.IsValid)
            {

                data.Remarks = dataVM.Remarks;

                data.ModifiedBy = Session["username"].ToString();

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        data.ModifiedAt = DateTime.Now;

                        LogRecallHeader log = new LogRecallHeader
                        {
                            LogId = Utilities.CreateGuid("LOG"),
                            TransactionId = data.TransactionId,
                            ActionName = "Modify",
                            ExecutedBy = data.ModifiedBy,
                            ExecutedAt = data.ModifiedAt.Value
                        };

                        int currentVersion = db.LogRecallDocuments.AsQueryable().OrderByDescending(m => m.Version)
                             .Where(m => m.TransactionId.Equals(data.TransactionId))
                             .AsEnumerable().Select(m => m.Version).FirstOrDefault();

                        LogRecallDocument logDoc = new LogRecallDocument
                        {
                            LogId = Utilities.CreateGuid("LOG"),
                            TransactionId = data.TransactionId,
                            Remarks = data.Remarks,
                            WarehouseId = data.WarehouseId,
                            WarehouseCode = data.WarehouseCode,
                            WarehouseName = data.WarehouseName,
                            TransactionStatus = data.TransactionStatus,
                            IsDeleted = data.IsDeleted,
                            CreatedBy = data.CreatedBy,
                            CreatedAt = data.CreatedAt,
                            ModifiedBy = data.ModifiedBy,
                            ModifiedAt = data.ModifiedAt,
                            ApprovedBy = data.ApprovedBy,
                            ApprovedAt = data.ApprovedAt,
                            Version = currentVersion + 1
                        };

                        db.LogRecallHeaders.Add(log);
                        db.LogRecallDocuments.Add(logDoc);


                        await db.SaveChangesAsync();
                        transaction.Commit();
                        result = true;


                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                        logger.Error(e, errMsg);
                    }
                }


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
        public async Task<JsonResult> Approve(string x)
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

            TrxRecallHeader data = db.TrxRecallHeaders.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();
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

                if (!data.TransactionStatus.Equals(Constant.TransactionStatus.PROGRESS.ToString()))
                {
                    message = "Delete document not allowed, document already processed.";
                    return Json(new { stat = result, msg = message });
                }

            }

            data.ApprovalStatus = Constant.StatusApproval.APPROVED.ToString();
            message = "Recall approved.";

            data.ApprovedAt = DateTime.Now;
            data.ApprovedBy = Session["username"].ToString();
            data.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();

            foreach (TrxRecallItem item in data.TrxRecallItems)
            {
                MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(item.TagId)).FirstOrDefault();
                pallet.PalletCondition = Constant.PalletCondition.GOOD.ToString();
                pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                pallet.WarehouseId = data.WarehouseId;
                pallet.WarehouseCode = data.WarehouseCode;
                pallet.WarehouseName = data.WarehouseName;
                pallet.IsDeleted = false;
            }

            LogRecallHeader log = new LogRecallHeader
            {
                LogId = Utilities.CreateGuid("LOG"),
                TransactionId = data.TransactionId,
                ActionName = "Approval : " + data.ApprovalStatus,
                ExecutedBy = data.ApprovedBy,
                ExecutedAt = data.ApprovedAt.Value
            };

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.LogRecallHeaders.Add(log);
                    db.SaveChanges();
                    transaction.Commit();
                    result = true;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                    logger.Error(e, errMsg);
                    throw new Exception("Submit data failed. Please contact system administrator.");
                }
            }


            if (result)
            {
                message = "Approve document succeeded.";
                TempData["TempMessage"] = message;
            }
            else
            {
                message = "Approve document failed. Please contact system administrator.";
            }

            return Json(new { stat = result, msg = message });
        }

        [HttpPost]
        public async Task<JsonResult> Reject(string x)
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

            TrxRecallHeader data = db.TrxRecallHeaders.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();
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

                if (!data.TransactionStatus.Equals(Constant.TransactionStatus.PROGRESS.ToString()))
                {
                    message = "Delete document not allowed, document already processed.";
                    return Json(new { stat = result, msg = message });
                }

            }

            data.ApprovalStatus = Constant.StatusApproval.REJECTED.ToString();
            message = "Recall rejected.";

            data.ApprovedAt = DateTime.Now;
            data.ApprovedBy = Session["username"].ToString();
            data.TransactionStatus = Constant.TransactionStatus.CLOSED.ToString();

            foreach (TrxRecallItem item in data.TrxRecallItems)
            {
                MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(item.TagId)).FirstOrDefault();
                pallet.PalletCondition = item.PreviousPalletCondition;
                pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
            }

            LogRecallHeader log = new LogRecallHeader
            {
                LogId = Utilities.CreateGuid("LOG"),
                TransactionId = data.TransactionId,
                ActionName = "Approval : " + data.ApprovalStatus,
                ExecutedBy = data.ApprovedBy,
                ExecutedAt = data.ApprovedAt.Value
            };

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.LogRecallHeaders.Add(log);
                    db.SaveChanges();
                    transaction.Commit();
                    result = true;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                    logger.Error(e, errMsg);
                    throw new Exception("Submit data failed. Please contact system administrator.");
                }
            }


            if (result)
            {
                message = "Reject document succeeded.";
                TempData["TempMessage"] = message;
            }
            else
            {
                message = "Reject document failed. Please contact system administrator.";
            }

            return Json(new { stat = result, msg = message });
        }

        //[HttpPost]
        //public async Task<JsonResult> Delete(string x)
        //{
        //    bool result = false;
        //    string message = "";
        //    string id = "";
        //    string warehouseId = Session["warehouseAccess"].ToString();
        //    try
        //    {
        //        if (string.IsNullOrEmpty(x))
        //        {
        //            throw new Exception();
        //        }

        //        id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
        //    }
        //    catch (Exception)
        //    {
        //        message = "Delete document failed. Try to refresh page or contact system administrator.";
        //        return Json(new { stat = result, msg = message });
        //    }

        //    TrxRecallHeader data = await IRecalls.GetDataByIdAsync(id);
        //    if (data == null)
        //    {
        //        message = "Data not found.";
        //        return Json(new { stat = result, msg = message });
        //    }
        //    else
        //    {
        //        if (!data.WarehouseId.Equals(warehouseId) || data.IsDeleted)
        //        {
        //            message = "Bad Request. Try to refresh page or contact system administrator.";
        //            return Json(new { stat = result, msg = message });
        //        }

        //        if (!data.TransactionStatus.Equals(Constant.TransactionStatus.OPEN.ToString()))
        //        {
        //            message = "Delete document not allowed, document already processed.";
        //            return Json(new { stat = result, msg = message });
        //        }

        //        if (!data.RecallStatus.Equals(Constant.RecallStatus.LOADING.ToString()))
        //        {
        //            message = "Delete document not allowed, document already processed.";
        //            return Json(new { stat = result, msg = message });
        //        }
        //    }

        //    data.IsDeleted = true;

        //    data.ModifiedBy = Session["username"].ToString();

        //    result = await IRecalls.DeleteAsync(data);

        //    if (result)
        //    {
        //        message = "Delete document succeeded.";
        //        TempData["TempMessage"] = message;
        //    }
        //    else
        //    {
        //        message = "Delete document failed. Please contact system administrator.";
        //    }

        //    return Json(new { stat = result, msg = message });
        //}

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

                IQueryable<TrxRecallItem> query = db.TrxRecallItems.AsQueryable().Where(x => x.TransactionId.Equals(transactionId));
                IEnumerable<TrxRecallItem> list = null;

                int recordsTotal = query.Count();

                try
                {
                    query = query
                            .Where(m => m.TagId.Contains(search));

                    //columns sorting
                    Dictionary<string, Func<TrxRecallItem, object>> cols = new Dictionary<string, Func<TrxRecallItem, object>>();
                    cols.Add("TagId", x => x.TagId);
                    cols.Add("PreviousWarehouseName", x => x.PreviousWarehouseName);
                    cols.Add("ScannedBy", x => x.ScannedBy);
                    cols.Add("ScannedAt", x => x.ScannedAt);
                    cols.Add("ModifiedBy", x => x.ModifiedBy);
                    cols.Add("ModifiedAt", x => x.ModifiedAt);
                    cols.Add("PreviousPalletCondition", x => x.PreviousPalletCondition);
                    cols.Add("NewPalletCondition", x => x.NewPalletCondition);



                    if (sortDirection.Equals("asc"))
                        list = query.OrderBy(cols[sortName]);
                    else
                        list = query.OrderByDescending(cols[sortName]);
                }
                catch (Exception e)
                {
                    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                    logger.Error(e, errMsg);
                }

                IEnumerable<RecallItemDTO> pagedData = Enumerable.Empty<RecallItemDTO>(); ;
                
                int recordsFilteredTotal = list.Count();

                list = list.Skip(start).Take(length).ToList();


                //re-format
                if (list != null && list.Count() > 0)
                {
                    pagedData = from m in list
                                select new RecallItemDTO
                                {
                                    TransactionItemId = Utilities.EncodeTo64(Encryptor.Encrypt(m.TransactionItemId, Constant.facelift_encryption_key)),
                                    TagId = m.TagId,
                                    PreviousWarehouseName = m.PreviousWarehouseName,
                                    PreviousPalletCondition = Utilities.PalletConditionBadge(m.PreviousPalletCondition),
                                    NewPalletCondition = String.IsNullOrEmpty(m.NewPalletCondition) ? "-" : Utilities.PalletConditionBadge(m.NewPalletCondition),
                                    ScannedBy = m.ScannedBy,
                                    ScannedAt = Utilities.NullDateTimeToString(m.ScannedAt),
                                    ModifiedBy = m.ScannedBy,
                                    ModifiedAt = Utilities.NullDateTimeToString(m.ScannedAt),
                                };
                }

                return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData},
                                JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { result = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }

        }


        //public ActionResult ExportListToExcel()
        //{
        //    String date = DateTime.Now.ToString("yyyyMMddhhmmss");
        //    String fileName = String.Format("filename=Facelift_Outbound_{0}.xlsx", date);
        //    string warehouseId = Session["warehouseAccess"].ToString();
        //    IEnumerable<TrxRecallHeader> list = IRecalls.GetAllOutboundTransactions(warehouseId);
        //    ExcelPackage excel = new ExcelPackage();
        //    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
        //    workSheet.TabColor = System.Drawing.Color.Black;


        //    workSheet.Row(1).Height = 25;
        //    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //    workSheet.Row(1).Style.Font.Bold = true;
        //    workSheet.Cells[1, 1].Value = "Transaction Code";
        //    workSheet.Cells[1, 2].Value = "Recall Number";
        //    workSheet.Cells[1, 3].Value = "Remarks";
        //    workSheet.Cells[1, 4].Value = "Warehouse Code";
        //    workSheet.Cells[1, 5].Value = "Warehouse Name";
        //    workSheet.Cells[1, 6].Value = "Destination Code";
        //    workSheet.Cells[1, 7].Value = "Destination Name";
        //    workSheet.Cells[1, 8].Value = "Transporter Name";
        //    workSheet.Cells[1, 9].Value = "Driver Name";
        //    workSheet.Cells[1, 10].Value = "Plate Number";
        //    workSheet.Cells[1, 11].Value = "Transaction Status";
        //    workSheet.Cells[1, 12].Value = "Recall Status";
        //    workSheet.Cells[1, 13].Value = "Created By";
        //    workSheet.Cells[1, 14].Value = "Created At";
        //    workSheet.Cells[1, 15].Value = "Modified By";
        //    workSheet.Cells[1, 16].Value = "Modified At";
        //    workSheet.Cells[1, 17].Value = "Approved By";
        //    workSheet.Cells[1, 18].Value = "Approved At";

        //    int recordIndex = 2;
        //    foreach (TrxRecallHeader header in list)
        //    {
        //        workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
        //        workSheet.Cells[recordIndex, 2].Value = header.RecallNumber;
        //        workSheet.Cells[recordIndex, 3].Value = header.Remarks;
        //        workSheet.Cells[recordIndex, 4].Value = header.WarehouseCode;
        //        workSheet.Cells[recordIndex, 5].Value = header.WarehouseName;
        //        workSheet.Cells[recordIndex, 6].Value = header.DestinationCode;
        //        workSheet.Cells[recordIndex, 7].Value = header.DestinationName;
        //        workSheet.Cells[recordIndex, 8].Value = header.TransporterName;
        //        workSheet.Cells[recordIndex, 9].Value = header.DriverName;
        //        workSheet.Cells[recordIndex, 10].Value = header.PlateNumber;
        //        workSheet.Cells[recordIndex, 11].Value = header.TransactionStatus;
        //        workSheet.Cells[recordIndex, 12].Value = header.RecallStatus;
        //        workSheet.Cells[recordIndex, 13].Value = header.CreatedBy;
        //        workSheet.Cells[recordIndex, 14].Value = Utilities.NullDateTimeToString(header.CreatedAt);
        //        workSheet.Cells[recordIndex, 15].Value = header.ModifiedBy;
        //        workSheet.Cells[recordIndex, 16].Value = Utilities.NullDateTimeToString(header.ModifiedAt);
        //        workSheet.Cells[recordIndex, 17].Value = header.ApprovedBy;
        //        workSheet.Cells[recordIndex, 18].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
        //        recordIndex++;
        //    }

        //    for (int i = 1; i <= 18; i++)
        //    {
        //        workSheet.Column(i).AutoFit();
        //    }

        //    using (var memoryStream = new MemoryStream())
        //    {
        //        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        Response.AddHeader("content-disposition", "attachment;" + fileName);
        //        excel.SaveAs(memoryStream);
        //        memoryStream.WriteTo(Response.OutputStream);
        //        Response.Flush();
        //        Response.End();
        //    }
        //    return RedirectToAction("Index");
        //}

        //public ActionResult ExportDetailListToExcel()
        //{
        //    String date = DateTime.Now.ToString("yyyyMMddhhmmss");
        //    String fileName = String.Format("filename=Facelift_Outbound_Details_{0}.xlsx", date);
        //    string warehouseId = Session["warehouseAccess"].ToString();
        //    IEnumerable<TrxRecallHeader> list = IRecalls.GetAllOutboundTransactions(warehouseId);
        //    ExcelPackage excel = new ExcelPackage();
        //    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
        //    workSheet.TabColor = System.Drawing.Color.Black;


        //    workSheet.Row(1).Height = 25;
        //    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //    workSheet.Row(1).Style.Font.Bold = true;
        //    workSheet.Cells[1, 1].Value = "Transaction Code";
        //    workSheet.Cells[1, 2].Value = "Recall Number";
        //    workSheet.Cells[1, 3].Value = "Remarks";
        //    workSheet.Cells[1, 4].Value = "Warehouse Code";
        //    workSheet.Cells[1, 5].Value = "Warehouse Name";
        //    workSheet.Cells[1, 6].Value = "Destination Code";
        //    workSheet.Cells[1, 7].Value = "Destination Name";
        //    workSheet.Cells[1, 8].Value = "Transporter Name";
        //    workSheet.Cells[1, 9].Value = "Driver Name";
        //    workSheet.Cells[1, 10].Value = "Plate Number";
        //    workSheet.Cells[1, 11].Value = "Transaction Status";
        //    workSheet.Cells[1, 12].Value = "Recall Status";
        //    workSheet.Cells[1, 13].Value = "Created By";
        //    workSheet.Cells[1, 14].Value = "Created At";
        //    workSheet.Cells[1, 15].Value = "Modified By";
        //    workSheet.Cells[1, 16].Value = "Modified At";
        //    workSheet.Cells[1, 17].Value = "Approved By";
        //    workSheet.Cells[1, 18].Value = "Approved At";
        //    workSheet.Cells[1, 19].Value = "Tag Id";
        //    workSheet.Cells[1, 20].Value = "Scanned By";
        //    workSheet.Cells[1, 21].Value = "Scanned At";
        //    workSheet.Cells[1, 22].Value = "Dispatched By";
        //    workSheet.Cells[1, 23].Value = "Dispatched At";
        //    workSheet.Cells[1, 24].Value = "Received By";
        //    workSheet.Cells[1, 25].Value = "Received At";


        //    int recordIndex = 2;
        //    foreach (TrxRecallHeader header in list)
        //    {
        //        foreach (TrxRecallItem item in header.TrxRecallItems)
        //        {
        //            workSheet.Cells[recordIndex, 1].Value = header.TransactionCode;
        //            workSheet.Cells[recordIndex, 2].Value = header.RecallNumber;
        //            workSheet.Cells[recordIndex, 3].Value = header.Remarks;
        //            workSheet.Cells[recordIndex, 4].Value = header.WarehouseCode;
        //            workSheet.Cells[recordIndex, 5].Value = header.WarehouseName;
        //            workSheet.Cells[recordIndex, 6].Value = header.DestinationCode;
        //            workSheet.Cells[recordIndex, 7].Value = header.DestinationName;
        //            workSheet.Cells[recordIndex, 8].Value = header.TransporterName;
        //            workSheet.Cells[recordIndex, 9].Value = header.DriverName;
        //            workSheet.Cells[recordIndex, 10].Value = header.PlateNumber;
        //            workSheet.Cells[recordIndex, 11].Value = header.TransactionStatus;
        //            workSheet.Cells[recordIndex, 12].Value = header.RecallStatus;
        //            workSheet.Cells[recordIndex, 13].Value = header.CreatedBy;
        //            workSheet.Cells[recordIndex, 14].Value = Utilities.NullDateTimeToString(header.CreatedAt);
        //            workSheet.Cells[recordIndex, 15].Value = header.ModifiedBy;
        //            workSheet.Cells[recordIndex, 16].Value = header.ModifiedAt;
        //            workSheet.Cells[recordIndex, 17].Value = header.ApprovedBy;
        //            workSheet.Cells[recordIndex, 18].Value = Utilities.NullDateTimeToString(header.ApprovedAt);
        //            workSheet.Cells[recordIndex, 19].Value = item.TagId;
        //            workSheet.Cells[recordIndex, 20].Value = item.ScannedBy;
        //            workSheet.Cells[recordIndex, 21].Value = Utilities.NullDateTimeToString(item.ScannedAt);
        //            workSheet.Cells[recordIndex, 22].Value = item.DispatchedBy;
        //            workSheet.Cells[recordIndex, 23].Value = Utilities.NullDateTimeToString(item.DispatchedAt);
        //            workSheet.Cells[recordIndex, 24].Value = item.ReceivedBy;
        //            workSheet.Cells[recordIndex, 25].Value = Utilities.NullDateTimeToString(item.ReceivedAt);
        //            recordIndex++;
        //        }
        //    }

        //    for (int i = 1; i <= 25; i++)
        //    {
        //        workSheet.Column(i).AutoFit();
        //    }

        //    using (var memoryStream = new MemoryStream())
        //    {
        //        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        Response.AddHeader("content-disposition", "attachment;" + fileName);
        //        excel.SaveAs(memoryStream);
        //        memoryStream.WriteTo(Response.OutputStream);
        //        Response.Flush();
        //        Response.End();
        //    }
        //    return RedirectToAction("Index");
        //}

    }
}