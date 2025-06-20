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
    public class TransporterRepository : ITransporters
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<MsTransporter>> GetAllAsync()
        {
            IEnumerable<MsTransporter> list = null;
            try
            {
                IQueryable<MsTransporter> query = db.MsTransporters
                   .Where(m => m.IsActive == true)
                   .OrderBy(m => m.TransporterName);

                list = await query.ToListAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public IEnumerable<MsTransporter> GetFilteredData(string search, string sortDirection, string sortColName)
        {
            IQueryable<MsTransporter> query = db.MsTransporters.AsQueryable();
            IEnumerable<MsTransporter> list = null;
            try
            {
                query = query
                        .Where(m => m.TransporterName.Contains(search) ||
                           m.Address.Contains(search) || m.Phone.Contains(search) || m.PIC.Contains(search) || m.Email.Contains(search));

                //columns sorting
                Dictionary<string, Func<MsTransporter, object>> cols = new Dictionary<string, Func<MsTransporter, object>>();
                cols.Add("TransporterName", x => x.TransporterName);
                cols.Add("Address", x => x.Address);
                cols.Add("Phone", x => x.Phone);
                cols.Add("Email", x => x.Email);
                cols.Add("PIC", x => x.PIC);
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
                IQueryable<MsTransporter> query = db.MsTransporters.AsQueryable();

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<MsTransporter> GetDataByIdAsync(string id)
        {
            MsTransporter data = null;
            try
            {
                data = await db.MsTransporters.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<MsTransporter> GetDataByTransporterNameAsync(string TransporterName)
        {
            MsTransporter data = null;
            try
            {
                data = await db.MsTransporters.Where(s => s.TransporterName.ToLower().Equals(TransporterName.ToLower())).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }
        public async Task<bool> InsertAsync(MsTransporter data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;
                    db.MsTransporters.Add(data);
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

        public async Task<bool> UpdateAsync(MsTransporter data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;

                    string[] WarehouseIds = data.MsTransporterAccesses.Select(m => m.WarehouseId).ToArray();
                    if (WarehouseIds.Count() > 0)
                    {
                        //delete previous access except selected warehouse
                        db.MsTransporterAccesses.RemoveRange(db.MsTransporterAccesses.Where(m => m.TransporterId.Equals(data.TransporterId) && !WarehouseIds.Contains(m.WarehouseId)));
                    }
                    else
                    {
                        //delete all warehouse access
                        db.MsTransporterAccesses.RemoveRange(db.MsTransporterAccesses.Where(m => m.TransporterId.Equals(data.TransporterId)));
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

        public async Task<List<MsTransporter>> GetAllByWarehouseAsync(string WarehouseId)
        {
            List<MsTransporter> list = new List<MsTransporter>();
            try
            {
                //IQueryable<MsTransporter> query = db.MsTransporters
                //   .Where(m => m.IsActive == true && m.MsTransporterAccesses.Wa)
                //   .OrderBy(m => m.TransporterName);
                IEnumerable<MsTransporterAccess> listAccess = null;
                IQueryable<MsTransporterAccess> query = db.MsTransporterAccesses
                   .Where(m => m.WarehouseId.Equals(WarehouseId));

                listAccess = await query.ToListAsync();
                if(listAccess.Count() > 0)
                {
                    foreach (MsTransporterAccess access in listAccess)
                    {
                        MsTransporter transporter = access.MsTransporter;
                        list.Add(transporter);
                    }
                }
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;

        }
    }
}