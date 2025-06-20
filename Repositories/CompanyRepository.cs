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
    public class CompanyRepository : ICompanies
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<MsCompany>> GetAllAsync()
        {
            IEnumerable<MsCompany> list = null;
            try
            {
                IQueryable<MsCompany> query = db.MsCompanies
                   .Where(m => m.IsActive == true)
                   .OrderBy(m => m.CompanyName);

                list = await query.ToListAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public IEnumerable<MsCompany> GetFilteredData(string search, string sortDirection, string sortColName)
        {
            IQueryable<MsCompany> query = db.MsCompanies.AsQueryable();
            IEnumerable<MsCompany> list = null;
            try
            {
                query = query
                        .Where(m => m.CompanyName.Contains(search) ||
                           m.CompanyAbb.Contains(search));

                //columns sorting
                Dictionary<string, Func<MsCompany, object>> cols = new Dictionary<string, Func<MsCompany, object>>();
                cols.Add("CompanyId", x => x.CompanyId);
                cols.Add("CompanyName", x => x.CompanyName);
                cols.Add("CompanyAbb", x => x.CompanyAbb);
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
                IQueryable<MsCompany> query = db.MsCompanies.AsQueryable();

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<MsCompany> GetDataByIdAsync(string id)
        {
            MsCompany data = null;
            try
            {
                data = await db.MsCompanies.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsCompany> GetDataByCompanyNameAsync(string CompanyName)
        {
            MsCompany data = null;
            try
            {
                data = await db.MsCompanies.Where(s => s.CompanyName.ToLower().Equals(CompanyName.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsCompany> GetDataByCompanyAbbreviationAsync(string CompanyAbb)
        {
            MsCompany data = null;
            try
            {
                data = await db.MsCompanies.Where(s => s.CompanyAbb.ToLower().Equals(CompanyAbb.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<bool> InsertAsync(MsCompany data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;
                    db.MsCompanies.Add(data);
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

        public async Task<bool> UpdateAsync(MsCompany data)
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