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
    public class InspectionController : Controller
    {

        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public InspectionController()
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


            IQueryable<TrxInspectionHeader> query = db.TrxInspectionHeaders.AsQueryable().Where(m => m.ApproverWarehouseId.Equals(warehouseId) && (string.IsNullOrEmpty(m.ApprovalStatus) || m.ApprovalStatus.Equals("APPROVED") ) && string.IsNullOrEmpty(m.IssuedBy) && m.IsDeleted == false);
            IEnumerable<TrxInspectionHeader> list = Enumerable.Empty<TrxInspectionHeader>();

            int recordsTotal = 0;
            try
            {
                recordsTotal = query.Count();

                query = query
                        .Where(m => m.TransactionCode.Contains(search) ||
                        m.PIC.Contains(search) ||
                           m.Classification.Contains(search));

                //columns sorting
                Dictionary<string, Func<TrxInspectionHeader, object>> cols = new Dictionary<string, Func<TrxInspectionHeader, object>>();
                cols.Add("TransactionCode", m => m.TransactionCode);
                cols.Add("RefNumber", m => m.RefNumber);
                cols.Add("Type", m => m.Type);
                cols.Add("WarehouseCode", m => m.WarehouseCode);
                cols.Add("PIC", m => m.PIC);
                cols.Add("Classification", m => m.Classification);
                cols.Add("Remarks", m => m.Remarks);
                cols.Add("CreatedAt", m => m.CreatedAt);
                cols.Add("CreatedBy", m => m.CreatedBy);
                cols.Add("ApprovedAt", m => m.ApprovedAt);
                cols.Add("ApprovedBy", m => m.ApprovedBy);


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


            IEnumerable<InspectionHeaderDTO> pagedData = Enumerable.Empty<InspectionHeaderDTO>();


            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {

                pagedData = from m in list
                            select new InspectionHeaderDTO
                            {
                                TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(m.TransactionId, Constant.facelift_encryption_key)),
                                TransactionCode = m.TransactionCode,
                                RefNumber = m.RefNumber,
                                Type = m.Type,
                                Remarks = m.Remarks,
                                Classification = m.Classification,
                                PIC = m.PIC,
                                WarehouseCode = m.WarehouseCode,
                                WarehouseName = m.WarehouseName,
                                CreatedBy = m.CreatedBy,
                                CreatedAt = Utilities.NullDateTimeToString(m.CreatedAt),
                                ApprovedBy = !string.IsNullOrEmpty(m.ApprovedBy) ? m.ApprovedBy : "-",
                                ApprovedAt = Utilities.NullDateTimeToString(m.ApprovedAt),
                                ApprovalStatus = m.ApprovalStatus
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
                            JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Detail(string x)
        {
            string warehouseId = Session["warehouseAccess"].ToString();
            ViewBag.TempMessage = TempData["TempMessage"];
            string id = "";
            TrxInspectionHeader data = null;
            try
            {
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                    data = db.TrxInspectionHeaders.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();
                    if (data == null)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        if (!data.ApproverWarehouseId.Equals(warehouseId) || data.IsDeleted)
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

            InspectionVM dataVM = new InspectionVM
            {
                TransactionCode = data.TransactionCode,
                RefNumber = data.RefNumber,
                Type = data.Type,
                PIC = data.PIC,
                Classification = data.Classification,
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
                ApprovalStatus = data.ApprovalStatus,
                IssuedBy = data.IssuedBy,
                IssuedAt = data.IssuedAt,
                logs = data.LogInspectionHeaders.OrderBy(m => m.ExecutedAt).ToList(),
                versions = data.LogInspectionDocuments.OrderBy(n => n.Version).ToList()
            };


            ViewBag.TransactionStatus = data.TransactionStatus;
            ViewBag.Id = x;


            return View(dataVM);
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

                IQueryable<TrxInspectionItem> query = db.TrxInspectionItems.AsQueryable().Where(x => x.TransactionId.Equals(transactionId));
                IEnumerable<TrxInspectionItem> list = null;

                int recordsTotal = query.Count();

                try
                {
                    query = query
                            .Where(m => m.TagId.Contains(search));

                    //columns sorting
                    Dictionary<string, Func<TrxInspectionItem, object>> cols = new Dictionary<string, Func<TrxInspectionItem, object>>();
                    cols.Add("TagId", x => x.TagId);
                    cols.Add("ScannedBy", x => x.ScannedBy);
                    cols.Add("ScannedAt", x => x.ScannedAt);



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

                IEnumerable<InspectionItemDTO> pagedData = Enumerable.Empty<InspectionItemDTO>(); ;

                int recordsFilteredTotal = list.Count();

                list = list.Skip(start).Take(length).ToList();


                //re-format
                if (list != null && list.Count() > 0)
                {
                    pagedData = from m in list
                                select new InspectionItemDTO
                                {
                                    TagId = m.TagId,
                                    ScannedBy = m.ScannedBy,
                                    ScannedAt = Utilities.NullDateTimeToString(m.ScannedAt),
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

            TrxInspectionHeader data = db.TrxInspectionHeaders.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!data.ApproverWarehouseId.Equals(warehouseId) || data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }

                if (!string.IsNullOrEmpty(data.ApprovalStatus))
                {
                    message = "Approve document not allowed, document already processed.";
                    return Json(new { stat = result, msg = message });
                }

            }

            data.ApprovalStatus = Constant.StatusApproval.APPROVED.ToString();
            message = "Inspection approved.";

            data.ApprovedAt = DateTime.Now;
            data.ApprovedBy = Session["username"].ToString();

            foreach (TrxInspectionItem item in data.TrxInspectionItems)
            {
                MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(item.TagId)).FirstOrDefault();
                pallet.PalletCondition = data.Type;
                pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                pallet.WarehouseId = data.ApproverWarehouseId;
                pallet.WarehouseCode = data.ApproverWarehouseCode;
                pallet.WarehouseName = data.ApproverWarehouseName;
            }

            LogInspectionHeader log = new LogInspectionHeader
            {
                LogId = Utilities.CreateGuid("LOG"),
                TransactionId = data.TransactionId,
                ActionName = "Approve Document",
                ExecutedBy = data.ApprovedBy,
                ExecutedAt = data.ApprovedAt.Value
            };

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.LogInspectionHeaders.Add(log);
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

            TrxInspectionHeader data = db.TrxInspectionHeaders.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!data.ApproverWarehouseId.Equals(warehouseId) || data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }

                if (!string.IsNullOrEmpty(data.ApprovalStatus))
                {
                    message = "Reject document not allowed, document already processed.";
                    return Json(new { stat = result, msg = message });
                }

            }

            data.ApprovalStatus = Constant.StatusApproval.REJECTED.ToString();
            message = "Inspection rejected.";

            data.ApprovedAt = DateTime.Now;
            data.ApprovedBy = Session["username"].ToString();

            foreach (TrxInspectionItem item in data.TrxInspectionItems)
            {
                MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(item.TagId)).FirstOrDefault();
                pallet.PalletCondition = Constant.PalletCondition.GOOD.ToString();
                pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
            }

            LogInspectionHeader log = new LogInspectionHeader
            {
                LogId = Utilities.CreateGuid("LOG"),
                TransactionId = data.TransactionId,
                ActionName = "Reject Document",
                ExecutedBy = data.ApprovedBy,
                ExecutedAt = data.ApprovedAt.Value
            };

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.LogInspectionHeaders.Add(log);
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


        [HttpPost]
        public async Task<JsonResult> Issue(string x)
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

            TrxInspectionHeader data = db.TrxInspectionHeaders.Where(m => m.TransactionId.Equals(id)).FirstOrDefault();
            if (data == null)
            {
                message = "Data not found.";
                return Json(new { stat = result, msg = message });
            }
            else
            {
                if (!data.ApproverWarehouseId.Equals(warehouseId) || data.IsDeleted)
                {
                    message = "Bad Request. Try to refresh page or contact system administrator.";
                    return Json(new { stat = result, msg = message });
                }

                if (!data.ApprovalStatus.Equals("APPROVED"))
                {
                    message = "Issue document not allowed, document already processed.";
                    return Json(new { stat = result, msg = message });
                }

            }

            message = "Issued Inspection Completed.";

            data.IssuedAt = DateTime.Now;
            data.IssuedBy = Session["username"].ToString();

            foreach (TrxInspectionItem item in data.TrxInspectionItems)
            {
                MsPallet pallet = db.MsPallets.Where(m => m.TagId.Equals(item.TagId)).FirstOrDefault();
                //pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                //update IsIssued = true
            }

            LogInspectionHeader log = new LogInspectionHeader
            {
                LogId = Utilities.CreateGuid("LOG"),
                TransactionId = data.TransactionId,
                ActionName = "Issued Document",
                ExecutedBy = data.ApprovedBy,
                ExecutedAt = data.ApprovedAt.Value
            };

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.LogInspectionHeaders.Add(log);
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
                message = "Issue document succeeded.";
                TempData["TempMessage"] = message;
            }
            else
            {
                message = "Issue document failed. Please contact system administrator.";
            }

            return Json(new { stat = result, msg = message });
        }

    }
}