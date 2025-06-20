using Facelift_App.Helper;
using Facelift_App.Services;
using Facelift_App.Validators;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;

namespace Facelift_App.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class WarehouseValidationController : Controller
    {

        private readonly WarehouseValidator validator;

        public WarehouseValidationController(IWarehouses Warehouses)
        {
            validator = new WarehouseValidator(Warehouses);
        }

        public async Task<JsonResult> IsUniqueName(string WarehouseName, string x)
        {
            string errMsg = "";
            try
            {
                string id = "";
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                }

                errMsg = await validator.IsUniqueName(WarehouseName, id);
            }
            catch (Exception)
            {
                errMsg = "Validation failed. Try to refresh page or contact system administrator.";
            }

            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> IsUniqueAlias(string WarehouseAlias, string x)
        {
            string errMsg = "";
            try
            {
                string id = "";
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                }

                errMsg = await validator.IsUniqueAlias(WarehouseAlias, id);
            }
            catch (Exception)
            {
                errMsg = "Validation failed. Try to refresh page or contact system administrator.";
            }

            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }

        //public async Task<JsonResult> IsWeightAllowed(int PalletUsedTarget, string x)
        //{
        //    string errMsg = "";
        //    try
        //    {
        //        string id = "";
        //        if (!string.IsNullOrEmpty(x))
        //        {
        //            id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
        //        }

        //        errMsg = await validator.IsWeightAllowed(PalletUsedTarget, id);
        //    }
        //    catch (Exception)
        //    {
        //        errMsg = "Validation failed. Try to refresh page or contact system administrator.";
        //    }

        //    return Json(errMsg, JsonRequestBehavior.AllowGet);
        //}

        public async Task<JsonResult> IsMGVAllowed(int PalletQty, string DestinationId)
        {
            string errMsg = "";
            try
            {
                errMsg = await validator.IsMGVAllowed(PalletQty, DestinationId);
            }
            catch (Exception)
            {
                errMsg = "Validation failed. Try to refresh page or contact system administrator.";
            }

            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }
    }
}