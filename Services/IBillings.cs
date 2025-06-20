using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facelift_App.Services
{
    public interface IBillings
    {
        //billing configuration
        IEnumerable<MsBillingConfiguration> GetFilteredData(string search, string sortDirection, string sortColName);
        int GetTotalData();
        Task<MsBillingConfiguration> GetDataByIdAsync(string id);
        Task<MsBillingConfiguration> GetDataByYearAsync(int year);
        Task<bool> InsertAsync(MsBillingConfiguration data);
        Task<bool> UpdateAsync(MsBillingConfiguration data);


        //billing rent pallet
        IEnumerable<VwPalletBilling> GetBilling(string WarehouseId, string AgingType, int month, int year, string search, string sortDirection, string sortColName);
        IEnumerable<VwPalletBilling> GetBilling(string WarehouseId, string AgingType, int month, int year);
        int GetTotalBilling(string WarehouseId, string AgingType, int month, int year);
        double? GetTotalPrice(string WarehouseId, string AgingType, int month, int year);



        //billing damage/ loss

    }
}
