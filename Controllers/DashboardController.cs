using Facelift_App.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Facelift_App.Services;
using Facelift_App.Models;
using System.Security.Cryptography;
using System.Text;

namespace Facelift_App.Controllers
{
    [SessionCheck]
    public class DashboardController : Controller
    {
        private readonly IUsers IUsers;
        private readonly IPallets IPallets;
        private readonly IDashboards IDashboards;
        private readonly IWarehouses IWarehouses;

        public DashboardController(IUsers Users, IPallets Pallets, IWarehouses Warehouses, IDashboards Dashboards)
        {
            IUsers = Users;
            IPallets = Pallets;
            IWarehouses = Warehouses;
            IDashboards = Dashboards;
            ViewBag.WarehouseDropdown = true;
        }

        private string getColor(int value)
        {
            string hash = GetMd5Hash(value.ToString());
            int r = Convert.ToInt32(hash.Substring(0, 2), 16);
            int g = Convert.ToInt32(hash.Substring(2, 2), 16);
            int b = Convert.ToInt32(hash.Substring(5, 2), 16);
            return string.Format("rgba({0},{1},{2},1)", r, g, b);
        }

        static string GetMd5Hash(string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // GET: Dashboard
        public async Task<ActionResult> Index()
        {
            string username = Session["username"].ToString();
            string warehouseId = Session["warehouseAccess"].ToString();
            MsUser msUser = await IUsers.GetDataByUsernameAsync(username);
            MsWarehouse msWarehouse = await IWarehouses.GetDataByIdAsync(warehouseId);
            Session.Add("full_name", msUser.FullName);
            Dashboard dashboard = new Dashboard();

            //all pallet
            int totalStock = IPallets.GetTotalStock(warehouseId);
            int totalGood = IPallets.GetTotalByCondition(warehouseId, Constant.PalletCondition.GOOD.ToString());
            int totalDamage = IPallets.GetTotalByCondition(warehouseId, Constant.PalletCondition.DAMAGE.ToString());
            int totalLoss = IPallets.GetTotalByCondition(warehouseId, Constant.PalletCondition.LOSS.ToString());
            ViewBag.totalRegistered = totalStock;
            dashboard.TotalPallet = Utilities.FormatThousand(totalStock);
            dashboard.TotalPalletGood = Utilities.FormatThousand(totalGood);
            dashboard.TotalPalletDamage = Utilities.FormatThousand(totalDamage);
            dashboard.TotalPalletLoss = Utilities.FormatThousand(totalLoss);

            //actual pallet
            int actualStock = IPallets.GetActualStock(warehouseId);
            int actualGood = IPallets.GetActualByCondition(warehouseId, Constant.PalletCondition.GOOD.ToString());
            int actualDamage = IPallets.GetActualByCondition(warehouseId, Constant.PalletCondition.DAMAGE.ToString());
            int actualLoss = IPallets.GetActualByCondition(warehouseId, Constant.PalletCondition.LOSS.ToString());
            dashboard.TotalActualPallet = Utilities.FormatThousand(actualStock);
            dashboard.TotalActualGood = Utilities.FormatThousand(actualGood);
            dashboard.TotalActualDamage = Utilities.FormatThousand(actualDamage);
            dashboard.TotalActualLoss = Utilities.FormatThousand(actualLoss);
          
            //shipment header
            dashboard.TotalOutboundLoading = Utilities.FormatThousand(IDashboards.TotalOutboundLoading(warehouseId));
            dashboard.TotalOutboundTransit = Utilities.FormatThousand(IDashboards.TotalOutboundTransit(warehouseId));
            dashboard.TotalOutboundFinished = Utilities.FormatThousand(IDashboards.TotalOutboundFinished(warehouseId));
            dashboard.TotalInboundTransit = Utilities.FormatThousand(IDashboards.TotalInboundTransit(warehouseId));
            dashboard.TotalInboundFinished = Utilities.FormatThousand(IDashboards.TotalInboundFinished(warehouseId));

            //shipment detail
            dashboard.TotalPalletInShipment = Utilities.FormatThousand(IDashboards.TotalPalletOutbound(warehouseId));
            dashboard.TotalPalletIncoming = Utilities.FormatThousand(IDashboards.TotalPalletInbound(warehouseId));

            List<VwShipmentAccident> listAging = IDashboards.GetListAging(warehouseId).ToList();
            //waiting for approval
            dashboard.TotalAccidentPending = Utilities.FormatThousand(listAging.Count());
            if (listAging.Count() > 0)
            {
                int row = 0;
                dashboard.AgingTransaction = from x in listAging
                                             let number = ++row
                                             select new AccidentDTO
                                             {
                                                 TransactionId = Utilities.EncodeTo64(Encryptor.Encrypt(x.ShipmentTransactionId, Constant.facelift_encryption_key)),
                                                 TransactionCode = x.AccidentTransactionCode,
                                                 CreatedAt = Utilities.NullDateTimeToString(x.AccidentCreatedAt),
                                                 Aging = (DateTime.Now - x.AccidentCreatedAt).Days.ToString()
                                             };
            }

            List<VwPalletWarehouse> totalPallet = IPallets.GetWarehouseStock(warehouseId).ToList();
            if(totalPallet.Count() > 0)
            {
                int row = 0;
                dashboard.PalletWarehouses = from x in totalPallet
                                             let number = ++row
                                             select new PalletStockDTO
                                             {
                                                WarehouseName = x.WarehouseName,
                                                TotalPallet = Utilities.FormatThousand(x.TotalPallet.Value),
                                                ChartColor = getColor(number * 21)
                                             };
            }
            
            List<VwPalletWarehouseCondition> palletGood = IPallets.GetWarehouseStockByCondition(warehouseId, Constant.PalletCondition.GOOD.ToString()).ToList();
            if (palletGood.Count() > 0)
            {
                int row = 0;
                dashboard.PalletGood = from x in palletGood
                                             let number = ++row
                                             select new PalletStockDTO
                                             {
                                                WarehouseName = x.WarehouseName,
                                                TotalPallet = Utilities.FormatThousand(x.TotalPallet.Value),
                                                ChartColor = getColor(number * 56)
                                             };
            }
            

            List<VwPalletWarehouseCondition> palletDamage = IPallets.GetWarehouseStockByCondition(warehouseId, Constant.PalletCondition.DAMAGE.ToString()).ToList();
            if (palletDamage.Count() > 0)
            {
                int row = 0;
                dashboard.PalletDamage = from x in palletDamage
                                         let number = ++row
                                         select new PalletStockDTO
                                         {
                                             WarehouseName = x.WarehouseName,
                                             TotalPallet = Utilities.FormatThousand(x.TotalPallet.Value),
                                             ChartColor = getColor(number * 41)
                                         };
            }


            List<VwPalletWarehouseCondition> palletLoss = IPallets.GetWarehouseStockByCondition(warehouseId, Constant.PalletCondition.LOSS.ToString()).ToList();
            if (palletLoss.Count() > 0)
            {
                int row = 0;
                dashboard.PalletLoss = from x in palletLoss
                                       let number = ++row
                                       select new PalletStockDTO
                                       {
                                           WarehouseName = x.WarehouseName,
                                           TotalPallet = Utilities.FormatThousand(x.TotalPallet.Value),
                                           ChartColor = getColor(number * 74)
                                       };
            }


            dashboard.PalletDelivery = IDashboards.PalletDelivery(warehouseId);
            dashboard.PalletIncoming = IDashboards.PalletIncoming(warehouseId);
            dashboard.ShipmentAging = IDashboards.GetShipmentAging(warehouseId);

            bool isApprover = false;

            if (!string.IsNullOrEmpty(msWarehouse.PIC1))
            {
                isApprover = msWarehouse.PIC1.Equals(msUser.Username);
            }

            if (!string.IsNullOrEmpty(msWarehouse.PIC2))
            {
                isApprover = msWarehouse.PIC2.Equals(msUser.Username);
            }


            ViewBag.IsApprover = isApprover;
            ViewBag.TotalAccidentPending = listAging.Count();

            return View(dashboard);
        }
    }
}