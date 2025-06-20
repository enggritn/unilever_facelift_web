using Facelift_App.Services;
using System.Threading.Tasks;

namespace Facelift_App.Validators
{
    public class WarehouseCategoryValidator
    {

        private readonly IWarehouseCategories IWarehouseCategories;


        public WarehouseCategoryValidator(IWarehouseCategories WarehouseCategories)
        {
            IWarehouseCategories = WarehouseCategories;
        }

        public async Task<string> IsExist(string WarehouseCategoryId)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            return await IWarehouseCategories.GetDataByIdAsync(WarehouseCategoryId) != null ? "true" : "Category not recognized.";
        }

    }
}