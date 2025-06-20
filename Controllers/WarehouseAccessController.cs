using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    public class WarehouseAccessController : Controller
    {

        private readonly IWarehouses IWarehouses;


        public WarehouseAccessController(IWarehouses Warehouses)
        {
            IWarehouses = Warehouses;
        }

        public ActionResult ListWarehouse()
        {
            string username = Session["username"].ToString();

            IEnumerable<MsWarehouse> listWarehouse = IWarehouses.GetByUsername(username);


            return PartialView("ListWarehouse", listWarehouse);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeAccess(string warehouseId)
        {
            string username = Session["username"].ToString();
            bool status = false;
            string message = "";
            Session.Add("warehouseAccess", "");
            try
            {
                //check warehouseid exist and user get access to selected warehouse
                MsWarehouse msWarehouse = await IWarehouses.GetDataByIdAsync(warehouseId);
                if (msWarehouse != null)
                {
                    bool IsAllowed = await IWarehouses.CheckAccessAsync(username, warehouseId);
                    if (IsAllowed)
                    {
                        Session.Add("warehouseAccess", msWarehouse.WarehouseId);
                        status = true;
                        message = "Change warehouse access succeded.";
                    }
                    else
                    {
                        message = "Your account do not have an access to this warehouse.";
                    }
                }
                else
                {
                    message = "Warehouse not exist.";
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            //return Redirect(Request.UrlReferrer.ToString());
            return Json(new { stat = status, msg = message });
        }


    }
}