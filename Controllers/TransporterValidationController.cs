using Facelift_App.Helper;
using Facelift_App.Models;
using Facelift_App.Services;
using Facelift_App.Validators;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;

namespace Facelift_App.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class TransporterValidationController : Controller
    {

        private readonly TransporterValidator validator;


        public TransporterValidationController(ITransporters Transporters, ITransporterDrivers TransporterDrivers, ITransporterTrucks TransporterTrucks)
        {
            validator = new TransporterValidator(Transporters, TransporterDrivers, TransporterTrucks);
        }


        public async Task<JsonResult> IsUniqueName(string TransporterName, string x)
        {
            string errMsg = "";
            try
            {
                string id = "";
                if (!string.IsNullOrEmpty(x))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(x), Constant.facelift_encryption_key);
                }

                errMsg = await validator.IsUniqueName(TransporterName, id);
            }
            catch (Exception)
            {
                errMsg = "Validation failed. Try to refresh page or contact system administrator.";
            }

            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> IsUniqueLicense(TransporterDriverVM transporterDriverVM)
        {
            string LicenseNumber = transporterDriverVM.LicenseNumber;
            string DriverId = transporterDriverVM.DriverId;
            string errMsg = "";
            try
            {
                string id = "";
                if (!string.IsNullOrEmpty(DriverId))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(DriverId), Constant.facelift_encryption_key);
                }

                errMsg = await validator.IsUniqueLicense(LicenseNumber, id);
            }
            catch (Exception)
            {
                errMsg = "Validation failed. Try to refresh page or contact system administrator.";
            }

            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> IsUniquePlate(TransporterTruckVM transporterTruckVM)
        {
            string PlateNumber = transporterTruckVM.PlateNumber;
            string TruckId = transporterTruckVM.TruckId;
            string errMsg = "";
            try
            {
                string id = "";
                if (!string.IsNullOrEmpty(TruckId))
                {
                    id = Encryptor.Decrypt(Utilities.DecodeFrom64(TruckId), Constant.facelift_encryption_key);
                }

                errMsg = await validator.IsUniquePlate(PlateNumber, id);
            }
            catch (Exception)
            {
                errMsg = "Validation failed. Try to refresh page or contact system administrator.";
            }

            return Json(errMsg, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> IsTransporterExist(string TransporterId)
        {
            return Json(await validator.IsTransporterExist(TransporterId), JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> IsDriverExist(string DriverId)
        {
            return Json(await validator.IsDriverExist(DriverId), JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> IsTruckExist(string TruckId)
        {
            return Json(await validator.IsTruckExist(TruckId), JsonRequestBehavior.AllowGet);
        }

    }
}