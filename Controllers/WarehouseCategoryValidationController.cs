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
    public class WarehouseCategoryValidationController : Controller
    {

        private readonly WarehouseCategoryValidator validator;

        public WarehouseCategoryValidationController(IWarehouseCategories WarehouseCategories)
        {
            validator = new WarehouseCategoryValidator(WarehouseCategories);
        }


        public async Task<JsonResult> IsExist(string CategoryId)
        {
            return Json(await validator.IsExist(CategoryId), JsonRequestBehavior.AllowGet);
        }
    }
}