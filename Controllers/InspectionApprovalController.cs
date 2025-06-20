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
    public class InspectionApprovalController : Controller
    {

        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public InspectionApprovalController()
        {
            ViewBag.WarehouseDropdown = true;
        }

        public ActionResult Index()
        {
            ViewBag.TempMessage = TempData["TempMessage"];
            return View();
        }

        //[HttpPost]
        //public ActionResult Datatable()
        //{
        //    string warehouseId = Session["warehouseAccess"].ToString();
        //    int draw = Convert.ToInt32(Request["draw"]);
        //    int start = Convert.ToInt32(Request["start"]);
        //    int length = Convert.ToInt32(Request["length"]);
        //    string search = Request["search[value]"];
        //    string orderCol = Request["order[0][column]"];
        //    string sortName = Request["columns[" + orderCol + "][name]"];
        //    string sortDirection = Request["order[0][dir]"];


        //    IQueryable<TrxInspectionItem> query = db.TrxInspectionItems.AsQueryable().Where(m => m.WarehouseId.Equals(warehouseId) && string.IsNullOrEmpty(m.ApprovedBy));
        //    IEnumerable<TrxInspectionItem> list = Enumerable.Empty<TrxInspectionItem>();

        //    int recordsTotal = 0;
        //    try
        //    {
        //        recordsTotal = query.Count();

        //        query = query
        //                .Where(m => m.TagId.Contains(search) ||
        //                m.PIC.Contains(search) ||
        //                   m.Classification.Contains(search));

        //        //columns sorting
        //        Dictionary<string, Func<TrxInspectionItem, object>> cols = new Dictionary<string, Func<TrxInspectionItem, object>>();
        //        cols.Add("TransactionCode", m => m.TrxInspectionHeader.TransactionCode);
        //        cols.Add("Origin", m => m.TrxInspectionHeader.WarehouseCode);
        //        cols.Add("TagId", m => m.TagId);
        //        cols.Add("Classification", m => m.Classification);
        //        cols.Add("PIC", m => m.PIC);
        //        cols.Add("ScannedAt", m => m.ScannedAt);
        //        cols.Add("ScannedBy", m => m.ScannedBy);
        //        cols.Add("VerifiedAt", m => m.VerifiedAt);
        //        cols.Add("VerifiedBy", m => m.VerifiedBy);
        //        cols.Add("ApprovedAt", m => m.ApprovedAt);
        //        cols.Add("ApprovedBy", m => m.ApprovedBy);


        //        if (sortDirection.Equals("asc"))
        //            list = query.OrderBy(cols[sortName]);
        //        else
        //            list = query.OrderByDescending(cols[sortName]);
        //    }
        //    catch (Exception e)
        //    {
        //        string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
        //        logger.Error(e, errMsg);
        //    }


        //    IEnumerable<InspectionItemDTO> pagedData = Enumerable.Empty<InspectionItemDTO>();

            
        //    int recordsFilteredTotal = list.Count();


        //    list = list.Skip(start).Take(length).ToList();


        //    //re-format
        //    if (list != null && list.Count() > 0)
        //    {

        //        pagedData = from m in list
        //                    select new InspectionItemDTO
        //                    {
        //                        TransactionItemId = m.TransactionItemId,
        //                        TransactionCode = m.TrxInspectionHeader.TransactionCode,
        //                        OriginName = m.TrxInspectionHeader.WarehouseCode + " - " + m.TrxInspectionHeader.WarehouseName,
        //                        TagId = m.TagId,
        //                        Classification = m.Classification,
        //                        PIC = m.PIC,
        //                        WarehouseCode = m.WarehouseCode,
        //                        WarehouseName = m.WarehouseName,
        //                        ScannedBy = m.ScannedBy,
        //                        ScannedAt = Utilities.NullDateTimeToString(m.ScannedAt),
        //                        VerifiedBy = !string.IsNullOrEmpty(m.VerifiedBy) ? m.VerifiedBy : "-",
        //                        VerifiedAt = Utilities.NullDateTimeToString(m.VerifiedAt),
        //                        ApprovedBy = !string.IsNullOrEmpty(m.ApprovedBy) ? m.ApprovedBy : "-",
        //                        ApprovedAt = Utilities.NullDateTimeToString(m.ApprovedAt)
        //                    };
        //    }


        //    return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData },
        //                    JsonRequestBehavior.AllowGet);
        //}


    }
}