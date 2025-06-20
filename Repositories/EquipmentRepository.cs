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
    public class EquipmentRepository : IEquipment
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Task<IEnumerable<MsEquipment>> GetByRoleIdAsync()
        {
            throw new NotImplementedException();
        }
        public IEnumerable<MsEquipment> GetFilteredData(string search, string sortDirection, string sortColName)
        {
            IQueryable<MsEquipment> query = db.MsEquipments.AsQueryable();
            IEnumerable<MsEquipment> list = null;
            try
            {
                query = query
                        .Where(m => m.Description.Contains(search) ||
                           m.EquipmentId.Contains(search));

                //columns sorting
                Dictionary<string, Func<MsEquipment, object>> cols = new Dictionary<string, Func<MsEquipment, object>>();
                cols.Add("EquipmentId", x => x.EquipmentId);
                cols.Add("Description", x => x.Description);
                cols.Add("Location", x => x.Location);
                cols.Add("Ip", x => x.Ip);
                cols.Add("port", x => x.port);
                cols.Add("Receive", x => x.Receive);
                cols.Add("Ship", x => x.Ship);
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
                IQueryable<MsEquipment> query = db.MsEquipments.AsQueryable();

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }
        public async Task<MsEquipment> GetDataByWarehouseAsync(string Name)
        {
            MsEquipment data = null;
            try
            {
                data = await db.MsEquipments.Where(s => s.EquipmentId.ToLower().Equals(Name.ToLower())).SingleOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }
        public async Task<bool> InsertAsync(MsEquipment data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;
                    db.MsEquipments.Add(data);
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
        public async Task<MsEquipment> GetDataByIdAsync(string id)
        {
            MsEquipment data = null;
            try
            {
                data = await db.MsEquipments.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }
        public async Task<bool> UpdateAsync(MsEquipment data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;
                    //db.MsUsers.Add(msUser);
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