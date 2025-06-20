using Facelift_App.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Entity;
using Facelift_App.Models;
using NLog;
using System.Linq;

namespace Facelift_App.Repositories
{
    public class UserRepository : IUsers
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<MsUser>> GetListAsync()
        {
            IEnumerable<MsUser> list = null;
            try
            {
                list = await db.MsUsers.Include(m => m.MsRole).Where(m => m.IsActive == true).ToListAsync();
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
                IQueryable<MsUser> query = db.MsUsers.AsQueryable();

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<MsUser> GetFilteredData(string search, string sortDirection, string sortColName)
        {
            IQueryable<MsUser> query = db.MsUsers.AsQueryable();
            IEnumerable<MsUser> list = null;
            try
            {
                query = query
                        .Where(m => m.Username.Contains(search) ||
                           m.FullName.Contains(search) ||
                           m.UserEmail.Contains(search));

                //columns sorting
                Dictionary<string, Func<MsUser, object>> cols = new Dictionary<string, Func<MsUser, object>>();
                cols.Add("Username", x => x.Username);
                cols.Add("FullName", x => x.FullName);
                cols.Add("UserEmail", x => x.UserEmail);
                cols.Add("MsRole.RoleName", x => x.MsRole != null ? x.MsRole.RoleName : null);
                //cols.Add("MsCompany.CompanyName", x=> x.MsCompany.CompanyName);
                cols.Add("IsActive", x => x.IsActive);
                cols.Add("LastLoginAt", x => x.LastLoginAt);
                cols.Add("LastVisitUrl", x => x.LastVisitUrl);
                cols.Add("LastVisitAt", x => x.LastVisitAt);
                cols.Add("ChPassAt", x => x.ChPassAt);
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

        public async Task<MsUser> GetDataByIdAsync(string id)
        {
            MsUser data = null;
            try
            {
                data = await db.MsUsers.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsUser> GetDataByUsernameAsync(string username)
        {
            MsUser data = null;
            try
            {
                data = await db.MsUsers.Where(s => s.Username.ToLower().Equals(username.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsUser> GetDataByEmailAsync(string email)
        {
            MsUser data = null;
            try
            {
                data = await db.MsUsers.Where(s => s.UserEmail.ToLower().Equals(email.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<bool> InsertAsync(MsUser data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;
                    db.MsUsers.Add(data);
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

        public async Task<bool> UpdateAsync(MsUser data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;
                    
                    string[] WarehouseIds = data.MsWarehouseAccesses.Select(m => m.WarehouseId).ToArray();
                    if (WarehouseIds.Count() > 0)
                    {
                        //delete previous access except selected warehouse
                        db.MsWarehouseAccesses.RemoveRange(db.MsWarehouseAccesses.Where(m => m.Username.Equals(data.Username) && !WarehouseIds.Contains(m.WarehouseId)));
                    }
                    else
                    {
                        //delete all warehouse access
                        db.MsWarehouseAccesses.RemoveRange(db.MsWarehouseAccesses.Where(m => m.Username.Equals(data.Username)));
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

        public async Task LastLoginAsync(MsUser data)
        {

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.LastLoginAt = DateTime.Now;

                    await db.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                    logger.Error(e, errMsg);
                }
            }

        }

        public async Task<bool> ChangePasswordAsync(MsUser data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ChPassAt = DateTime.Now;
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

        public async Task ChangeDefaultWarehouse(MsUser data)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    await db.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                    logger.Error(e, errMsg);
                }
            }
        }
    }
}