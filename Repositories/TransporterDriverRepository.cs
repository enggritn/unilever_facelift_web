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
    public class TransporterDriverRepository : ITransporterDrivers
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<MsDriver>> GetAllAsync(string TransporterId)
        {
            IEnumerable<MsDriver> list = null;
            try
            {
                IQueryable<MsDriver> query = db.MsDrivers
                    .Where(m => m.TransporterId.Equals(TransporterId))
                   .Where(m => m.IsActive == true)
                   .OrderBy(m => m.DriverName);

                list = await query.ToListAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public IEnumerable<MsDriver> GetFilteredData(string TransporterId, string search, string sortDirection, string sortColName)
        {
            IQueryable<MsDriver> query = db.MsDrivers.AsQueryable().Where(m => m.TransporterId.Equals(TransporterId));
            IEnumerable<MsDriver> list = null;
            try
            {
                query = query
                        .Where(m => m.DriverName.Contains(search) || m.LicenseNumber.Contains(search));

                //columns sorting
                Dictionary<string, Func<MsDriver, object>> cols = new Dictionary<string, Func<MsDriver, object>>();
                cols.Add("DriverName", x => x.DriverName);
                cols.Add("Phone", x => x.Phone);
                cols.Add("LicenseNumber", x => x.LicenseNumber);
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
        public int GetTotalData(string TransporterId)
        {
            int totalData = 0;
            try
            {
                IQueryable<MsDriver> query = db.MsDrivers.Where(m => m.TransporterId.Equals(TransporterId)).AsQueryable();

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<MsDriver> GetDataByIdAsync(string id)
        {
            MsDriver data = null;
            try
            {
                data = await db.MsDrivers.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsDriver> GetDataByLicenseNumberAsync(string LicenseNumber)
        {
            MsDriver data = null;
            try
            {
                data = await db.MsDrivers.Where(s => s.LicenseNumber.ToLower().Equals(LicenseNumber.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }
        public async Task<bool> InsertAsync(MsDriver data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;
                    db.MsDrivers.Add(data);
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

        public async Task<bool> UpdateAsync(MsDriver data)
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
    }
}