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
    public class WarehouseRepository : IWarehouses
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<MsWarehouse>> GetAllAsync()
        {
            IEnumerable<MsWarehouse> list = null;
            try
            {
                IQueryable<MsWarehouse> query = db.MsWarehouses
                   .Where(m => m.IsActive == true)
                   .OrderBy(m => m.WarehouseCode);

                list = await query.ToListAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public IEnumerable<MsWarehouse> GetFilteredData(string search, string sortDirection, string sortColName)
        {
            IQueryable<MsWarehouse> query = db.MsWarehouses.AsQueryable();
            IEnumerable<MsWarehouse> list = null;
            try
            {
                query = query
                        .Where(m => m.WarehouseName.Contains(search) ||
                           m.WarehouseCode.Contains(search) || m.Address.Contains(search) || m.Phone.Contains(search) || m.PIC1.Contains(search) || m.PIC2.Contains(search));

                //columns sorting
                Dictionary<string, Func<MsWarehouse, object>> cols = new Dictionary<string, Func<MsWarehouse, object>>();
                cols.Add("WarehouseCode", x => x.WarehouseCode);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("WarehouseAlias", x => x.WarehouseAlias);
                cols.Add("MsWarehouseCategory.CategoryName", x => x.MsWarehouseCategory.CategoryName);
                cols.Add("Address", x => x.Address);
                cols.Add("Phone", x => x.Phone);
                cols.Add("PIC1", x => x.PIC1);
                cols.Add("PIC2", x => x.PIC2);
                cols.Add("MaxCapacity", x => x.MaxCapacity);
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
                IQueryable<MsWarehouse> query = db.MsWarehouses.AsQueryable();

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<MsWarehouse> GetDataByIdAsync(string id)
        {
            MsWarehouse data = null;
            try
            {
                data = await db.MsWarehouses.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsWarehouse> GetDataByWarehouseCodeAsync(string WarehouseCode)
        {
            MsWarehouse data = null;
            try
            {
                data = await db.MsWarehouses.Where(s => s.WarehouseCode.ToLower().Equals(WarehouseCode.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsWarehouse> GetDataByWarehouseNameAsync(string WarehouseName)
        {
            MsWarehouse data = null;
            try
            {
                data = await db.MsWarehouses.Where(s => s.WarehouseName.ToLower().Equals(WarehouseName.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }
        public async Task<bool> InsertAsync(MsWarehouse data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //get last number, and do increment.
                    string lastNumber = db.MsWarehouses.AsQueryable().OrderByDescending(x => x.WarehouseCode).AsEnumerable().Select(x => x.WarehouseCode).FirstOrDefault();
                    int currentNumber = 0;
                    
                    if (!string.IsNullOrEmpty(lastNumber))
                    {
                        currentNumber = Int32.Parse(lastNumber.Substring(lastNumber.Length - 3));
                    }

                    data.WarehouseCode = "WHC" + string.Format("{0:D3}", currentNumber + 1);
                    data.CreatedAt = DateTime.Now;
                    db.MsWarehouses.Add(data);
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
        
        public async Task<bool> UpdateAsync(MsWarehouse data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;
                    //db.MsUsers.Add(msUser);

                    string[] CompanyIds = data.MsWarehouseCompanies.Select(m => m.CompanyId).ToArray();
                    if (CompanyIds.Count() > 0)
                    {
                        //delete previous access except selected warehouse
                        db.MsWarehouseCompanies.RemoveRange(db.MsWarehouseCompanies.Where(m => m.WarehouseId.Equals(data.WarehouseId) && !CompanyIds.Contains(m.CompanyId)));
                    }
                    else
                    {
                        //delete all warehouse access
                        db.MsWarehouseCompanies.RemoveRange(db.MsWarehouseCompanies.Where(m => m.WarehouseId.Equals(data.WarehouseId)));
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


        public IEnumerable<MsWarehouse> GetByUsername(string Username)
        {
            IEnumerable<MsWarehouse> list = null;
            try
            {
                MsUser msUser = db.MsUsers.Find(Username);
                string[] WarehouseIds = db.MsWarehouseAccesses.Where(m => m.Username.Equals(msUser.Username)).Select(x => x.WarehouseId).ToArray();
                IQueryable<MsWarehouse> query = db.MsWarehouses
                    .Where(m => m.IsActive == true && WarehouseIds.Contains(m.WarehouseId))
                    .OrderBy(m => m.WarehouseCode);

                list = query.ToList();

            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public IEnumerable<MsWarehouseAccess> GetAccessByUsername(string Username)
        {
            IEnumerable<MsWarehouseAccess> list = null;
            try
            {
                MsUser msUser = db.MsUsers.Find(Username);
                string[] WarehouseIds = db.MsWarehouseAccesses.Where(m => m.Username.Equals(msUser.Username)).Select(x => x.WarehouseId).ToArray();
                IQueryable<MsWarehouseAccess> query = db.MsWarehouseAccesses
                    .Where(m => WarehouseIds.Contains(m.WarehouseId));

                list = query.ToList();

            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public async Task<bool> CheckAccessAsync(string Username, string WarehouseId)
        {
            bool IsAllowed = false;

            try
            {
                MsWarehouseAccess msWarehouseAccess = await db.MsWarehouseAccesses.Where(m => m.Username.Equals(Username) && m.WarehouseId.Equals(WarehouseId)).FirstOrDefaultAsync();
                IsAllowed = msWarehouseAccess != null ? true : false;
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return IsAllowed;
        }

        public async Task<IEnumerable<MsWarehouse>> GetDestinationAsync(string originId)
        {
            IEnumerable<MsWarehouse> list = null;
            try
            {
                IQueryable<MsWarehouse> query = db.MsWarehouses
                   .Where(m => m.IsActive == true)
                   .Where(m => !m.WarehouseId.Equals(originId))
                   .OrderBy(m => m.WarehouseCode);

                list = await query.ToListAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public async Task<MsWarehouse> GetDataByWarehouseAliasAsync(string WarehouseAlias)
        {
            MsWarehouse data = null;
            try
            {
                data = await db.MsWarehouses.Where(s => s.WarehouseAlias.ToLower().Equals(WarehouseAlias.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public int GetTotalWeight()
        {
            int totalWeight = 0;
            //try
            //{
            //    totalWeight = db.MsWarehouses
            //        .Sum(i => i.PalletUsedTarget);

            //}
            //catch (Exception e)
            //{
            //    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
            //    logger.Error(e, errMsg);
            //}
            return totalWeight;
        }
    }
}