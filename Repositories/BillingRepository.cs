using Facelift_App.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Facelift_App.Repositories
{
    public class BillingRepository : IBillings
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<VwPalletBilling> GetBilling(string WarehouseId, string AgingType, int month, int year, string search, string sortDirection, string sortColName)
        {
            IEnumerable<VwPalletBilling> list = Enumerable.Empty<VwPalletBilling>();

            try
            {
                IQueryable<VwPalletBilling> query = db.VwPalletBillings.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.AgingType.Equals(AgingType) && x.CurrentMonth == month && x.CurrentYear == year);

                query = query.Where(m => m.PalletId.Contains(search));

                //columns sorting
                Dictionary<string, Func<VwPalletBilling, object>> cols = new Dictionary<string, Func<VwPalletBilling, object>>();
                cols.Add("PalletId", x => x.PalletId);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("ReceivedAt", x => x.ReceivedAt);
                cols.Add("PalletStatus", x => x.PalletStatus);
                cols.Add("TotalMinutes", x => x.TotalMinutes);
                cols.Add("TotalHours", x => x.TotalHours);
                cols.Add("TotalDays", x => x.TotalDays);
                cols.Add("PalletAgeMonth", x => x.PalletAgeMonth);
                cols.Add("PalletAgeYear", x => x.PalletAgeYear);
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

        public IEnumerable<VwPalletBilling> GetBilling(string WarehouseId, string AgingType, int month, int year)
        {
            IEnumerable<VwPalletBilling> list = Enumerable.Empty<VwPalletBilling>();

            try
            {
                IQueryable<VwPalletBilling> query = db.VwPalletBillings.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.AgingType.Equals(AgingType) && x.CurrentMonth == month && x.CurrentYear == year);

                //columns sorting
                Dictionary<string, Func<VwPalletBilling, object>> cols = new Dictionary<string, Func<VwPalletBilling, object>>();
                cols.Add("TagId", x => x.PalletId);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("ReceivedAt", x => x.ReceivedAt);
                cols.Add("PalletStatus", x => x.PalletStatus);
                cols.Add("TotalMinutes", x => x.TotalMinutes);
                cols.Add("TotalHours", x => x.TotalHours);
                cols.Add("TotalDays", x => x.TotalDays);
                cols.Add("PalletAgeMonth", x => x.PalletAgeMonth);
                cols.Add("PalletAgeYear", x => x.PalletAgeYear);
                cols.Add("BillingPrice", x => x.BillingPrice);
                cols.Add("TotalBilling", x => x.TotalBilling);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public int GetTotalBilling(string WarehouseId, string AgingType, int month, int year)
        {
            int totalData = 0;
            try
            {
                IQueryable<VwPalletBilling> query = db.VwPalletBillings.AsQueryable()
                    .Where(x => x.WarehouseId.Equals(WarehouseId) && x.AgingType.Equals(AgingType) && x.CurrentMonth == month && x.CurrentYear == year);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public double? GetTotalPrice(string WarehouseId, string AgingType, int month, int year)
        {
            double? totalData = 0;
            try
            {
                totalData = db.VwPalletBillings
                   .Where(x => x.WarehouseId.Equals(WarehouseId) && x.AgingType.Equals(AgingType) && x.CurrentMonth == month && x.CurrentYear == year)
                    .Sum(i => i.TotalBilling);

            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<MsBillingConfiguration> GetFilteredData(string search, string sortDirection, string sortColName)
        {
            IQueryable<MsBillingConfiguration> query = db.MsBillingConfigurations.AsQueryable();
            IEnumerable<MsBillingConfiguration> list = null;
            try
            {
                query = query
                        .Where(m => m.BillingYear.ToString().Contains(search) ||
                           m.BillingPrice.ToString().Contains(search));

                //columns sorting
                Dictionary<string, Func<MsBillingConfiguration, object>> cols = new Dictionary<string, Func<MsBillingConfiguration, object>>();
                cols.Add("BillingId", x => x.BillingId);
                cols.Add("BillingYear", x => x.BillingYear);
                cols.Add("BillingPrice", x => x.BillingPrice);
                cols.Add("IsActive", x => x.IsActive);
                cols.Add("CreatedBy", x => x.CreatedBy);
                cols.Add("CreatedAt", x => x.CreatedAt);
                cols.Add("ModifiedBy", x => x.ModifiedBy);
                cols.Add("ModifiedAt", x => x.ModifiedAt);

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

        public int GetTotalData()
        {
            int totalData = 0;
            try
            {
                IQueryable<MsBillingConfiguration> query = db.MsBillingConfigurations.AsQueryable();

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<MsBillingConfiguration> GetDataByIdAsync(string id)
        {
            MsBillingConfiguration data = null;
            try
            {
                data = await db.MsBillingConfigurations.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<bool> InsertAsync(MsBillingConfiguration data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;
                    db.MsBillingConfigurations.Add(data);
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

        public async Task<bool> UpdateAsync(MsBillingConfiguration data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;
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

        public async Task<MsBillingConfiguration> GetDataByYearAsync(int year)
        {
            MsBillingConfiguration data = null;
            try
            {
                data = await db.MsBillingConfigurations.Where(s => s.BillingYear == year).FirstOrDefaultAsync();
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