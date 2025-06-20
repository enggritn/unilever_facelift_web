using Facelift_App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Facelift_App.Validators
{
    public class WarehouseValidator
    {
        private readonly IWarehouses IWarehouses;
        FaceliftEntities db = new FaceliftEntities();
        public WarehouseValidator(IWarehouses Warehouses)
        {
            IWarehouses = Warehouses;
        }

        public async Task<string> IsUniqueName(string WarehouseName, string id)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true";
            MsWarehouse data = await IWarehouses.GetDataByWarehouseNameAsync(WarehouseName);
            if (data != null && !data.WarehouseId.Equals(id))
            {
                errMsg = "Warehouse Name already registered.";
            }
            return errMsg;
        }

        public async Task<string> IsUniqueAlias(string WarehouseAlias, string id)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true";
            MsWarehouse data = await IWarehouses.GetDataByWarehouseNameAsync(WarehouseAlias);
            if (data != null && !data.WarehouseId.Equals(id))
            {
                errMsg = "Warehouse Alias already registered.";
            }
            return errMsg;
        }

        public async Task<string> IsWarehouseExist(string WarehouseId)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            return await IWarehouses.GetDataByIdAsync(WarehouseId) != null ? "true" : "Warehouse not recognized.";
        }

        public async Task<string> IsWeightAllowed(int MaxCapacity, string id)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            //MsWarehouse data = await IWarehouses.GetDataByIdAsync(id);
            //int maxWeight = 100;
            //int totalWeight = IWarehouses.GetTotalWeight();
            //int prevWeight = 0;
            //if (data != null)
            //{
            //    prevWeight = data.MaxCapacity;
            //    totalWeight -= prevWeight;
            //}

            //totalWeight += PalletUsedTarget;
            ////get total PalletUsedTarget

            string errMsg = "true";
            //if (maxWeight < totalWeight)
            //{
            //    errMsg = "Pallet Used Target exceeding total weight.";
            //}
            return errMsg;
        }

        public async Task<string> IsMGVAllowed(int PalletQty, string DestinationId)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            MsWarehouse data = await IWarehouses.GetDataByIdAsync(DestinationId);
            int maxCapacity = 0;
            int currentCapacity = 0;
            string errMsg = "true";

            if (data != null)
            {
                if(PalletQty > 0)
                {
                    maxCapacity = data.MaxCapacity;
                    if (maxCapacity > 0)
                    {
                        currentCapacity = db.MsPallets.Where(m => m.WarehouseId.Equals(data.WarehouseId) && m.PalletCondition.Equals("GOOD")).Count();
                        int currentShipment = db.TrxShipmentHeaders.Where(m => m.DestinationId.Equals(data.WarehouseId) && !m.TransactionStatus.Equals("CLOSED") && m.IsDeleted == false).Select(m => m.PalletQty).DefaultIfEmpty(0).Sum();
                        currentCapacity += currentShipment;

                        if (currentCapacity + PalletQty > maxCapacity)
                        {
                            int availableQty = maxCapacity - currentCapacity; 
                            errMsg = string.Format("Exceeding max capacity, current capacity {0}. Allowed Pallet Qty : {1}", currentCapacity, availableQty);
                        }
                    }
                }
                else
                {
                    errMsg = "Quantity must bigger than 0";
                }
            }
            return errMsg;
        }
    }
}