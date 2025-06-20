using Facelift_App.Helper;
using Facelift_App.Services;
using NLog;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Facelift_App.Repositories
{
    public class BillingHistoryRepository : IBillingHistories
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<bool> CreateRentAsync(TrxBillingRentHistoryHeader data)
        {

            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;

                    MsWarehouse warehouse = await db.MsWarehouses.FindAsync(data.WarehouseId);

                    //MsUser user = await db.MsUsers.FindAsync(data.CreatedBy);

                    //MsCompany company = await db.MsCompanies.FindAsync(user.CompanyId);

                    string prefix = data.TransactionId.Substring(0, 3);
                    //string palletOwner = Constant.facelift_pallet_owner;
                    //string palletOwner = company.CompanyAbb;
                    string warehouseCode = data.WarehouseCode;
                    string warehouseAlias = warehouse.WarehouseAlias;
                    int year = Convert.ToInt32(data.CreatedAt.Year.ToString().Substring(2));
                    int month = data.CreatedAt.Month;
                    string romanMonth = Utilities.ConvertMonthToRoman(month);

                    //data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}/{5}", prefix, palletOwner, warehouseAlias, data.AgingType ,year, romanMonth);
                    data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}", prefix, warehouseAlias, data.AgingType, year, romanMonth);

                    db.TrxBillingRentHistoryHeaders.Add(data);

                    //get latest month from existing invoice
                    TrxBillingRentHistoryHeader prevData = db.TrxBillingRentHistoryHeaders.AsQueryable().OrderByDescending(x => x.TransactionCode)
                       .Where(x => x.WarehouseId.Equals(data.WarehouseId) && x.AgingType.Equals(data.AgingType)).OrderByDescending(x => x.CreatedAt)
                       .AsEnumerable().FirstOrDefault();

                    
                    //IQueryable<VwPalletBilling> prevBill = null;
                    if (prevData == null)
                    {
                        //get first start period
                        MsPalletAging startingPeriod = db.MsPalletAgings.Where(m => m.AgingType.Equals(data.AgingType)).OrderBy(m => m.ReceivedAt).FirstOrDefault();
                        data.StartPeriod = new DateTime(startingPeriod.ReceivedAt.Year, startingPeriod.ReceivedAt.Month, startingPeriod.ReceivedAt.Day);
                    }
                    else
                    {
                        data.StartPeriod = prevData.LastPeriod;
                        //check previous billing, if previous month still have billing it will be included to current month
                    }

                    

                    var LastStartDate = new DateTime(data.CurrentYear, data.CurrentMonth, 1);
                    var LastEndDate = LastStartDate.AddMonths(1).AddDays(-1);

                    if(data.CreatedAt >= LastEndDate)
                    {
                        data.LastPeriod = LastEndDate;
                    }
                    else
                    {
                        data.LastPeriod = data.CreatedAt;
                    }


                    //prevBill = db.VwPalletBillings.AsQueryable().Where(x => x.WarehouseId.Equals(data.WarehouseId) && x.AgingType.Equals(data.AgingType));
                    //prevBill = prevBill.Where(x => x.CurrentYear == prevData.CurrentYear && x.CurrentMonth == prevData.CurrentMonth);


                    //get billing for current month
                    IQueryable<VwPalletBilling> currentBillings = db.VwPalletBillings.AsQueryable().Where(x => x.WarehouseId.Equals(data.WarehouseId) && x.AgingType.Equals(data.AgingType));
                    //currentBillings = currentBillings.Where(x => x.CurrentYear == data.CreatedAt.Year && x.CurrentMonth == data.CreatedAt.Month);
                    currentBillings = currentBillings.Where(x => (x.CurrentYear >= data.StartPeriod.Year && x.CurrentMonth >= data.StartPeriod.Month) || (x.CurrentYear <= data.LastPeriod.Year && x.CurrentMonth <= data.LastPeriod.Month));


                    List<VwPalletBilling> billings = new List<VwPalletBilling>();

                    //if (prevBill != null)
                    //{
                    //    var prevB = prevBill.ToList();
                    //    var curB = currentBillings.ToList();
                    //    prevB.AddRange(curB);
                    //    billings = prevB;
                    //    //prevBill.ToList().AddRange(currentBillings.ToList());
                    //}
                    //else
                    //{
                    //    var curB = currentBillings.ToList();
                    //    billings = curB;
                    //}

                    var curB = currentBillings.ToList();
                    billings = curB;


                    //loop insert
                    foreach (VwPalletBilling billing in billings)
                    {
                        MsPalletAging msPalletAging = await db.MsPalletAgings.FindAsync(billing.AgingId);
                        //check if exist, ignore
                        if (msPalletAging != null && billing.TotalMinutes > 0)
                        {
                            //insert item
                            TrxBillingRentHistoryItem item = new TrxBillingRentHistoryItem()
                            {
                                TransactionItemId = Utilities.CreateGuid("IVI"),
                                TransactionId = data.TransactionId,
                                AgingId = msPalletAging.AgingId,
                                PalletId = msPalletAging.PalletId,
                                TotalMinutes = billing.TotalMinutes.Value,
                                BillingId = billing.BillingId,
                                BillingYear = billing.BillingYear,
                                BillingPrice = billing.BillingPrice,
                                CurrentMonth = billing.CurrentMonth,
                                CurrentYear = billing.CurrentYear
                            };

                            db.TrxBillingRentHistoryItems.Add(item);
                            //reset billing

                            //db.MsPalletAgings.Remove(msPalletAging);

                            msPalletAging.TotalMinutes = 0;
                            if (billing.PalletStatus.Equals("OnHand"))
                            {
                                msPalletAging.ReceivedAt = DateTime.Now;
                            }

                        }
                    }


                    await db.SaveChangesAsync();
                    transaction.Commit();
                    status = true;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                    logger.Error(e, errMsg);
                }
            }

            return status;
        }

        public async Task<TrxBillingRentHistoryHeader> GetRentDataByIdAsync(string id)
        {
            TrxBillingRentHistoryHeader data = null;
            try
            {
                data = await db.TrxBillingRentHistoryHeaders.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<TrxBillingRentHistoryHeader> GetRentDataByTransactionCodeAsync(string TransactionCode)
        {
            TrxBillingRentHistoryHeader data = null;
            try
            {
                data = await db.TrxBillingRentHistoryHeaders.Where(m => m.TransactionCode.Equals(TransactionCode)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public int GetTotalRentData(string WarehouseId, string AgingType)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxBillingRentHistoryHeader> query = db.TrxBillingRentHistoryHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.AgingType.Equals(AgingType));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<TrxBillingRentHistoryHeader> GetFilteredRentData(string WarehouseId, string AgingType,string search, string sortDirection, string sortColName)
        {
            IQueryable<TrxBillingRentHistoryHeader> query = db.TrxBillingRentHistoryHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.AgingType.Equals(AgingType));
            IEnumerable<TrxBillingRentHistoryHeader> list = null;
            try
            {
                query = query
                        .Where(m => m.TransactionCode.Contains(search) ||
                           m.CurrentYear.ToString().Contains(search) || m.CurrentMonth.ToString().Contains(search));

                //columns sorting
                Dictionary<string, Func<TrxBillingRentHistoryHeader, object>> cols = new Dictionary<string, Func<TrxBillingRentHistoryHeader, object>>();
                cols.Add("TransactionCode", x => x.TransactionCode);
                cols.Add("CurrentYear", x => x.CurrentYear);
                cols.Add("CurrentMonth", x => x.CurrentMonth);
                cols.Add("Tax", x => x.Tax);
                cols.Add("StartPeriod", x => x.StartPeriod);
                cols.Add("LastPeriod", x => x.LastPeriod);
                cols.Add("CreatedBy", x => x.CreatedBy);
                cols.Add("CreatedAt", x => x.CreatedAt);


                if (sortDirection.Equals("asc"))
                    list = query.OrderBy(cols[sortColName]);
                else
                    list = query.OrderByDescending(cols[sortColName]);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public IEnumerable<VwBillingRentHistoryItem> GetFilteredRentDataItem(string TransactionId, string search, string sortDirection, string sortColName)
        {
            IQueryable<VwBillingRentHistoryItem> query = db.VwBillingRentHistoryItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwBillingRentHistoryItem> list = null;
            try
            {
                query = query
                        .Where(m => m.PalletId.Contains(search) || m.PalletName.Contains(search));

                //columns sorting
                Dictionary<string, Func<VwBillingRentHistoryItem, object>> cols = new Dictionary<string, Func<VwBillingRentHistoryItem, object>>();
                cols.Add("PalletId", x => x.PalletId);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("CurrentYear", x => x.CurrentYear);
                cols.Add("CurrentMonth", x => x.CurrentMonth);
                cols.Add("TotalMinutes", x => x.TotalMinutes);
                cols.Add("TotalHours", x => x.TotalHours);
                cols.Add("TotalDays", x => x.TotalDays);
                cols.Add("BillingYear", x => x.BillingYear);
                cols.Add("BillingPrice", x => x.BillingPrice);
                cols.Add("TotalBilling", x => x.TotalBilling);


                if (sortDirection.Equals("asc"))
                    list = query.OrderBy(cols[sortColName]);
                else
                    list = query.OrderByDescending(cols[sortColName]);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public int GetTotalRentDataItem(string TransactionId)
        {
            int totalData = 0;
            try
            {
                IQueryable<VwBillingRentHistoryItem> query = db.VwBillingRentHistoryItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public double? GetTotalRentPrice(string TransactionId)
        {
            double? totalData = 0;
            try
            {
                totalData = db.VwBillingRentHistoryItems
                   .Where(x => x.TransactionId.Equals(TransactionId))
                    .Sum(i => i.TotalBilling);

            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<TrxBillingRentHistoryHeader> GetRentInvoiceAsync(string WarehouseId, string AgingType,int CurrentYear, int CurrentMonth)
        {
            TrxBillingRentHistoryHeader data = null;
            try
            {
                data = await db.TrxBillingRentHistoryHeaders.Where(m => m.WarehouseId.Equals(WarehouseId) && m.AgingType.Equals(AgingType) && m.CurrentYear == CurrentYear && m.CurrentMonth == CurrentMonth).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public IEnumerable<VwBillingRentHistoryItem> GetRentDataItemsById(string transactionId)
        {
            IQueryable<VwBillingRentHistoryItem> query = db.VwBillingRentHistoryItems.AsQueryable().Where(x => x.TransactionId.Equals(transactionId));
            IEnumerable<VwBillingRentHistoryItem> list = null;
            try
            {
                query = query.OrderBy(x => x.CurrentYear).ThenBy(x => x.CurrentMonth);
                list = query.ToList();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public IEnumerable<TrxBillingRentHistoryHeader> GetRentData(string WarehouseId, string AgingType)
        {
            IQueryable<TrxBillingRentHistoryHeader> query = db.TrxBillingRentHistoryHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.AgingType.Equals(AgingType));
            IEnumerable<TrxBillingRentHistoryHeader> list = null;
            try
            {
                query = query.OrderBy(x => x.CreatedAt);
                list = query.ToList();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public async Task<VwBillingRentHistoryItem> GetRentDetailByIdAsync(string transactionItemId)
        {
            VwBillingRentHistoryItem data = null;
            try
            {
                data = await db.VwBillingRentHistoryItems.Where(m => m.TransactionItemId.Equals(transactionItemId)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }
    }
}