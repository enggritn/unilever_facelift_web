using Facelift_App.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Facelift_App.Repositories
{
    public class RoleRepository : IRoles
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<MsRole>> GetAllAsync(bool IsActive)
        {
            IEnumerable<MsRole> list = null;
            try
            {
                list = await db.MsRoles.Where(m => m.IsActive == IsActive).ToListAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public async Task<MsRole> GetDataByIdAsync(string id)
        {
            MsRole data = null;
            try
            {
                data = await db.MsRoles.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsRole> GetDataByRoleNameAsync(string roleName)
        {
            MsRole data = null;
            try
            {
                data = await db.MsRoles.Where(s => s.RoleName.ToLower().Equals(roleName.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public IEnumerable<MsRole> GetFilteredData(string search, string sortDirection, string sortColName)
        {
            IQueryable<MsRole> query = db.MsRoles.AsQueryable();
            IEnumerable<MsRole> list = null;
            try
            {
                query = query
                        .Where(m => m.RoleName.Contains(search) ||
                           m.RoleDescription.Contains(search));

                //columns sorting
                Dictionary<string, Func<MsRole, object>> cols = new Dictionary<string, Func<MsRole, object>>();
                cols.Add("RoleName", x => x.RoleName);
                cols.Add("RoleDescription", x => x.RoleDescription);
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
            return list;
        }

        public int GetTotalData()
        {
            int totalData = 0;
            try
            {
                IQueryable<MsRole> query = db.MsRoles.AsQueryable();

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<bool> InsertAsync(MsRole data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;
                    db.MsRoles.Add(data);
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

        public async Task<bool> UpdateAsync(MsRole data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;

                    int[] MenuIds = data.MsRolePermissions.Select(m => m.MenuId).ToArray();
                    if (MenuIds.Count() > 0)
                    {
                        //delete previous access except selected warehouse
                        db.MsRolePermissions.RemoveRange(db.MsRolePermissions.Where(m => m.RoleId.Equals(data.RoleId) && !MenuIds.Contains(m.MenuId)));
                    }
                    else
                    {
                        //delete all warehouse access
                        db.MsRolePermissions.RemoveRange(db.MsRolePermissions.Where(m => m.RoleId.Equals(data.RoleId)));
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
    }
}