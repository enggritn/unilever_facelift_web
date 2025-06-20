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
    public class TransporterTruckRepository : ITransporterTrucks
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<MsTruck>> GetAllAsync(string TransporterId)
        {
            IEnumerable<MsTruck> list = null;
            try
            {
                IQueryable<MsTruck> query = db.MsTrucks
                    .Where(m => m.TransporterId.Equals(TransporterId))
                   .Where(m => m.IsActive == true)
                   .OrderBy(m => m.PlateNumber);

                list = await query.ToListAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public IEnumerable<MsTruck> GetFilteredData(string TransporterId, string search, string sortDirection, string sortColName)
        {
            IQueryable<MsTruck> query = db.MsTrucks.AsQueryable().Where(m => m.TransporterId.Equals(TransporterId));
            IEnumerable<MsTruck> list = null;
            try
            {
                query = query
                        .Where(m => m.PlateNumber.Contains(search));

                //columns sorting
                Dictionary<string, Func<MsTruck, object>> cols = new Dictionary<string, Func<MsTruck, object>>();
                cols.Add("PlateNumber", x => x.PlateNumber);
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
                IQueryable<MsTruck> query = db.MsTrucks.AsQueryable().Where(m => m.TransporterId.Equals(TransporterId)).AsQueryable();

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<MsTruck> GetDataByIdAsync(string id)
        {
            MsTruck data = null;
            try
            {
                data = await db.MsTrucks.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsTruck> GetDataByPlateNumberAsync(string PlateNumber)
        {
            MsTruck data = null;
            try
            {
                data = await db.MsTrucks.Where(s => s.PlateNumber.ToLower().Equals(PlateNumber.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }
        public async Task<bool> InsertAsync(MsTruck data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;
                    db.MsTrucks.Add(data);
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

        public async Task<bool> UpdateAsync(MsTruck data)
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