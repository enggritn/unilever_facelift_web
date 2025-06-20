using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using NLog;
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
    public class ReportConditionHistoryController : Controller
    {

        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();


        // GET: ReportAccident
        public async Task<ActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Datatable()
        {
            int draw = Convert.ToInt32(Request["draw"]);
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string search = Request["search[value]"];
            string orderCol = Request["order[0][column]"];
            string sortName = Request["columns[" + orderCol + "][name]"];
            string sortDirection = Request["order[0][dir]"];


            IQueryable<VwPalletDeffectHistory> query = db.VwPalletDeffectHistories.OrderBy(m => m.TagId).OrderBy(m => m.ScannedAt).AsQueryable();
            IEnumerable<VwPalletDeffectHistory> list = Enumerable.Empty<VwPalletDeffectHistory>();

            int recordsTotal = 0;
            try
            {

                recordsTotal = query.Count();

                query = query
                        .Where(m => m.TransactionCode.Contains(search) ||
                           m.TagId.Contains(search));

                //columns sorting
                Dictionary<string, Func<VwPalletDeffectHistory, object>> cols = new Dictionary<string, Func<VwPalletDeffectHistory, object>>();
                cols.Add("TagId", m => m.TagId);
                cols.Add("WarehouseCode", m => m.WarehouseCode);
                cols.Add("WarehouseName", m => m.WarehouseName);
                cols.Add("ScannedAt", m => m.ScannedAt);
                cols.Add("ScannedBy", m => m.ScannedBy);
                cols.Add("TransactionCode", m => m.TransactionCode);
                cols.Add("PalletCondition", m => m.PalletCondition);


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


            IEnumerable<PalletDeffectHistoryDTO> pagedData = Enumerable.Empty<PalletDeffectHistoryDTO>();


            int recordsFilteredTotal = list.Count();


            list = list.Skip(start).Take(length).ToList();


            //re-format
            if (list != null && list.Count() > 0)
            {

                pagedData = from m in list
                            select new PalletDeffectHistoryDTO
                            {
                                TagId = m.TagId,
                                WarehouseCode = m.WarehouseCode,
                                WarehouseName = m.WarehouseName,
                                ScannedAt = Utilities.NullDateTimeToString(m.ScannedAt),
                                ScannedBy = m.ScannedBy,
                                TransactionCode = m.TransactionCode,
                                PalletCondition = Utilities.PalletConditionBadge(m.PalletCondition),
                            };
            }


            return Json(new { draw = draw, recordsFiltered = recordsFilteredTotal, recordsTotal = recordsTotal, data = pagedData},
                            JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportListToExcel()
        {
            return RedirectToAction("Index");
        }

    }
}