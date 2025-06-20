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
    public class PalletTypeRepository : IPalletTypes
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<MsPalletType>> GetAllAsync()
        {
            IEnumerable<MsPalletType> list = null;
            try
            {
                IQueryable<MsPalletType> query = db.MsPalletTypes
                   .Where(m => m.IsActive == true)
                   .OrderBy(m => m.PalletName);

                list = await query.ToListAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public IEnumerable<MsPalletType> GetFilteredData(string search, string sortDirection, string sortColName)
        {
            IQueryable<MsPalletType> query = db.MsPalletTypes.AsQueryable();
            IEnumerable<MsPalletType> list = null;
            try
            {
                query = query
                        .Where(m => m.PalletName.Contains(search) ||
                           m.PalletDescription.Contains(search));

                //columns sorting
                Dictionary<string, Func<MsPalletType, object>> cols = new Dictionary<string, Func<MsPalletType, object>>();
                cols.Add("PalletTypeId", x => x.PalletTypeId);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletDescription", x => x.PalletDescription);
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
                IQueryable<MsPalletType> query = db.MsPalletTypes.AsQueryable();

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<MsPalletType> GetDataByIdAsync(string id)
        {
            MsPalletType data = null;
            try
            {
                data = await db.MsPalletTypes.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsPalletType> GetDataByPalletNameAsync(string PalletName)
        {
            MsPalletType data = null;
            try
            {
                data = await db.MsPalletTypes.Where(s => s.PalletName.ToLower().Equals(PalletName.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }
        public async Task<bool> InsertAsync(MsPalletType data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;
                    db.MsPalletTypes.Add(data);
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

        public async Task<bool> UpdateAsync(MsPalletType data)
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