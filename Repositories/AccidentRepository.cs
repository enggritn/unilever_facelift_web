using Facelift_App.Helper;
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
    public class AccidentRepository : IAccidents
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<TrxAccidentHeader> GetFilteredData(string WarehouseId, string stats, string search, string sortDirection, string sortColName)
        {
            IQueryable<TrxAccidentHeader> query = db.TrxAccidentHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);
            IEnumerable<TrxAccidentHeader> list = null;
            try
            {
                if (!string.IsNullOrEmpty(stats))
                {
                    if (stats.Equals("0"))
                    {
                        query = query.Where(m => !m.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));
                    }
                    else if (stats.Equals("1"))
                    {
                        query = query.Where(m => m.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));
                    }
                }

                query = query
                        .Where(m => m.TransactionCode.Contains(search) ||
                           m.AccidentType.Contains(search) || m.RefNumber.Contains(search));

                //columns sorting
                Dictionary<string, Func<TrxAccidentHeader, object>> cols = new Dictionary<string, Func<TrxAccidentHeader, object>>();
                cols.Add("TransactionCode", x => x.TransactionCode);
                cols.Add("RefNumber", x => x.RefNumber);
                cols.Add("AccidentType", x => x.AccidentType);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("TransactionStatus", x => x.TransactionStatus);
                cols.Add("CreatedBy", x => x.CreatedBy);
                cols.Add("CreatedAt", x => x.CreatedAt);
                cols.Add("ModifiedBy", x => x.ModifiedBy);
                cols.Add("ModifiedAt", x => x.ModifiedAt);
                cols.Add("ApprovedBy", x => x.ApprovedBy);
                cols.Add("ApprovedAt", x => x.ApprovedAt);


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

        public int GetTotalData(string WarehouseId, string stats)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxAccidentHeader> query = db.TrxAccidentHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);

                if (!string.IsNullOrEmpty(stats))
                {
                    if (stats.Equals("0"))
                    {
                        query = query.Where(m => !m.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));
                    }
                    else if (stats.Equals("1"))
                    {
                        query = query.Where(m => m.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));
                    }
                }

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<TrxAccidentHeader> GetDataByIdAsync(string id)
        {
            TrxAccidentHeader data = null;
            try
            {
                data = await db.TrxAccidentHeaders.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public TrxAccidentHeader GetDataById(string id)
        {
            TrxAccidentHeader data = null;
            try
            {
                data = db.TrxAccidentHeaders.Find(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<TrxAccidentHeader> GetDataByTransactionCodeAsync(string TransactionCode)
        {
            TrxAccidentHeader data = null;
            try
            {
                data = await db.TrxAccidentHeaders.Where(m => m.TransactionCode.Equals(TransactionCode)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<bool> CreateAsync(TrxAccidentHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;

                    MsWarehouse warehouse = await db.MsWarehouses.FindAsync(data.WarehouseId);


                    //MsUser user = await db.MsUsers.FindAsync(data.CreatedBy);

                    //MsCompany company = await db.MsCompanies.FindAsync(user.CompanyId);

                    string prefix = data.TransactionId.Substring(0, 2);
                    //string palletOwner = Constant.facelift_pallet_owner;
                    //string palletOwner = company.CompanyAbb;
                    string warehouseCode = data.WarehouseCode;
                    string warehouseAlias = warehouse.WarehouseAlias;
                    int year = Convert.ToInt32(data.CreatedAt.Year.ToString().Substring(2));
                    int month = data.CreatedAt.Month;
                    string romanMonth = Utilities.ConvertMonthToRoman(month);

                    // get last number, and do increment.
                    string lastNumber = db.TrxAccidentHeaders.AsQueryable().OrderByDescending(x => x.TransactionCode)
                        .Where(x => x.WarehouseCode.Equals(warehouseCode) && x.CreatedAt.Year.Equals(data.CreatedAt.Year) && x.CreatedAt.Month.Equals(data.CreatedAt.Month))
                        .AsEnumerable().Select(x => x.TransactionCode).FirstOrDefault();
                    int currentNumber = 0;

                    if (!string.IsNullOrEmpty(lastNumber))
                    {
                        currentNumber = Int32.Parse(lastNumber.Substring(lastNumber.Length - 3));
                    }

                    string runningNumber = string.Format("{0:D3}", currentNumber + 1);

                    //data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}/{5}", prefix, palletOwner, warehouseAlias, year, romanMonth, runningNumber);

                    data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}", prefix, warehouseAlias, year, romanMonth, runningNumber);

                    LogAccidentHeader log = new LogAccidentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Create",
                        ExecutedBy = data.CreatedBy,
                        ExecutedAt = data.CreatedAt
                    };

                    LogAccidentDocument logDoc = new LogAccidentDocument
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        RefNumber = data.RefNumber,
                        AccidentType = data.AccidentType,
                        Remarks = data.Remarks,
                        WarehouseId = data.WarehouseId,
                        WarehouseCode = data.WarehouseCode,
                        WarehouseName = data.WarehouseName,
                        TransactionStatus = data.TransactionStatus,
                        IsDeleted = data.IsDeleted,
                        CreatedBy = data.CreatedBy,
                        CreatedAt = data.CreatedAt,
                        ModifiedBy = data.ModifiedBy,
                        ModifiedAt = data.ModifiedAt,
                        ApprovedBy = data.ApprovedBy,
                        ApprovedAt = data.ApprovedAt,
                        Version = 1
                    };

                    db.TrxAccidentHeaders.Add(data);
                    db.LogAccidentHeaders.Add(log);
                    db.LogAccidentDocuments.Add(logDoc);

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

        public async Task<bool> UpdateAsync(TrxAccidentHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;

                    LogAccidentHeader log = new LogAccidentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Modify",
                        ExecutedBy = data.ModifiedBy,
                        ExecutedAt = data.ModifiedAt.Value
                    };

                    int currentVersion = db.LogAccidentDocuments.AsQueryable().OrderByDescending(x => x.Version)
                         .Where(x => x.TransactionId.Equals(data.TransactionId))
                         .AsEnumerable().Select(x => x.Version).FirstOrDefault();

                    LogAccidentDocument logDoc = new LogAccidentDocument
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        RefNumber = data.RefNumber,
                        AccidentType = data.AccidentType,
                        Remarks = data.Remarks,
                        WarehouseId = data.WarehouseId,
                        WarehouseCode = data.WarehouseCode,
                        WarehouseName = data.WarehouseName,
                        TransactionStatus = data.TransactionStatus,
                        IsDeleted = data.IsDeleted,
                        CreatedBy = data.CreatedBy,
                        CreatedAt = data.CreatedAt,
                        ModifiedBy = data.ModifiedBy,
                        ModifiedAt = data.ModifiedAt,
                        ApprovedBy = data.ApprovedBy,
                        ApprovedAt = data.ApprovedAt,
                        Version = currentVersion + 1
                    };

                    db.LogAccidentHeaders.Add(log);
                    db.LogAccidentDocuments.Add(logDoc);



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

        public async Task<bool> DeleteAsync(TrxAccidentHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;

                    LogAccidentHeader log = new LogAccidentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Delete",
                        ExecutedBy = data.ModifiedBy,
                        ExecutedAt = data.ModifiedAt.Value
                    };

                    db.LogAccidentHeaders.Add(log);


                    //update all pallet status to ST
                    foreach (TrxAccidentItem item in db.TrxAccidentItems.Where(m => m.TransactionId.Equals(data.TransactionId)))
                    {
                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                        if (pallet != null)
                        {
                            pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                        }
                    }

                    //delete all items
                    db.TrxAccidentItems.RemoveRange(db.TrxAccidentItems.Where(m => m.TransactionId.Equals(data.TransactionId)));


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

        public async Task<bool> CloseAsync(TrxAccidentHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ApprovedAt = DateTime.Now;

                    LogAccidentHeader log = new LogAccidentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Approve",
                        ExecutedBy = data.ApprovedBy,
                        ExecutedAt = data.ApprovedAt.Value
                    };

                    db.LogAccidentHeaders.Add(log);

                    DateTime currentDate = DateTime.Now;

                    int year = currentDate.Year;
                    int month = currentDate.Month;



                    //loop update to pallet
                    foreach (TrxAccidentItem item in data.TrxAccidentItems)
                    {
                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                        if (pallet != null)
                        {
                            pallet.PalletCondition = item.ReasonType;
                            pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                            pallet.LastTransactionName = Constant.TransactionName.INSPECTION.ToString();
                            pallet.LastTransactionCode = data.TransactionCode;
                            pallet.LastTransactionDate = DateTime.Now;

                            //stop billing 
                            //MsPalletAging aging = await db.MsPalletAgings.Where(x => x.PalletId.Equals(item.TagId) && x.IsActive == true && x.CurrentMonth.Equals(month) && x.CurrentYear.Equals(year) && x.WarehouseId.Equals(data.WarehouseId)).FirstOrDefaultAsync();
                            //if (aging != null)
                            //{
                            //    int totalminutes = (int)(currentDate - aging.ReceivedAt).TotalMinutes;
                            //    aging.TotalMinutes += totalminutes;
                            //    aging.IsActive = false;
                            //}
                        }


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

        public IEnumerable<VwAccidentItem> GetFilteredDataItem(string TransactionId, string search, string sortDirection, string sortColName)
        {
            IQueryable<VwAccidentItem> query = db.VwAccidentItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwAccidentItem> list = null;
            try
            {
                query = query
                        .Where(m => m.TagId.Contains(search) || m.PalletName.Contains(search));

                //columns sorting
                Dictionary<string, Func<VwAccidentItem, object>> cols = new Dictionary<string, Func<VwAccidentItem, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("ReasonType", x => x.ReasonType);
                cols.Add("ReasonName", x => x.ReasonName);
                cols.Add("ScannedBy", x => x.ScannedBy);
                cols.Add("ScannedAt", x => x.ScannedAt);


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

        public IEnumerable<VwAccidentItem> GetDataItem(string TransactionId)
        {
            IQueryable<VwAccidentItem> query = db.VwAccidentItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwAccidentItem> list = null;
            try
            {
                //columns sorting
                Dictionary<string, Func<VwAccidentItem, object>> cols = new Dictionary<string, Func<VwAccidentItem, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("ReasonType", x => x.ReasonType);
                cols.Add("ReasonName", x => x.ReasonName);
                cols.Add("ScannedBy", x => x.ScannedBy);
                cols.Add("ScannedAt", x => x.ScannedAt);

                list = query.OrderBy(cols["TagId"]);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public int GetTotalDataItem(string TransactionId)
        {
            int totalData = 0;
            try
            {
                IQueryable<VwAccidentItem> query = db.VwAccidentItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<TrxAccidentItem> GetDataByTransactionTagIdAsync(string TransactionId, string TagId)
        {
            TrxAccidentItem data = null;
            try
            {
                data = await db.TrxAccidentItems.Where(x => x.TransactionId.Equals(TransactionId) && x.TagId.Equals(TagId)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public IEnumerable<VwAccidentItem> GetDetailByTransactionId(string TransactionId)
        {
            IQueryable<VwAccidentItem> query = db.VwAccidentItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwAccidentItem> list = null;
            try
            {
                list = query.OrderBy(x => x.ScannedAt);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public async Task<bool> InsertItemAsync(TrxAccidentHeader data, string username, string actionName)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogAccidentHeader log = new LogAccidentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = actionName,
                        ExecutedBy = username,
                        ExecutedAt = DateTime.Now
                    };


                    db.LogAccidentHeaders.Add(log);

                    foreach (TrxAccidentItem item in data.TrxAccidentItems)
                    {
                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                        if (pallet != null)
                        {
                            pallet.PalletMovementStatus = Constant.PalletMovementStatus.OP.ToString();
                        }
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

        public IEnumerable<TrxAccidentHeader> GetList(string warehouseId)
        {
            IQueryable<TrxAccidentHeader> query = db.TrxAccidentHeaders.AsQueryable()
               .Where(x => x.WarehouseId.Equals(warehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString())
               && x.IsDeleted == false);
            IEnumerable<TrxAccidentHeader> list = null;
            try
            {
                list = query.OrderByDescending(x => x.TransactionCode);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public async Task<bool> DeleteItemAsync(TrxAccidentHeader data, string[] items, string username)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogAccidentHeader log = new LogAccidentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Delete Item",
                        ExecutedBy = username,
                        ExecutedAt = DateTime.Now
                    };

                    db.LogAccidentHeaders.Add(log);

                    foreach (TrxAccidentItem item in db.TrxAccidentItems.Where(m => m.TransactionId.Equals(data.TransactionId) && items.Contains(m.TransactionItemId)))
                    {
                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                        if (pallet != null)
                        {
                            pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                        }
                    }

                    //delete all items
                    db.TrxAccidentItems.RemoveRange(db.TrxAccidentItems.Where(m => m.TransactionId.Equals(data.TransactionId) && items.Contains(m.TransactionItemId)));


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

        public IEnumerable<TrxAccidentHeader> GetAccidentData(string WarehouseId, string startDate, string endDate, string search, string sortDirection, string sortColName)
        {
            IEnumerable<TrxAccidentHeader> list = Enumerable.Empty<TrxAccidentHeader>();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime EndDate = DateTime.Parse(endDate);

                try
                {
                    IQueryable<TrxAccidentHeader> query = db.TrxAccidentHeaders.AsQueryable()
                    .Where(x => x.WarehouseId.Equals(WarehouseId)
                    && x.IsDeleted == false);
                    //&& x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));

                    query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.CreatedAt) <= DbFunctions.TruncateTime(EndDate));
                    query = query
                            .Where(m => m.TransactionCode.Contains(search) ||
                               m.RefNumber.Contains(search));

                    //columns sorting
                    Dictionary<string, Func<TrxAccidentHeader, object>> cols = new Dictionary<string, Func<TrxAccidentHeader, object>>();
                    cols.Add("TransactionCode", x => x.TransactionCode);
                    cols.Add("RefNumber", x => x.RefNumber);
                    cols.Add("AccidentType", x => x.AccidentType);
                    cols.Add("Remarks", x => x.Remarks);
                    cols.Add("WarehouseCode", x => x.WarehouseCode);
                    cols.Add("WarehouseName", x => x.WarehouseName);
                    cols.Add("TransactionStatus", x => x.TransactionStatus);
                    cols.Add("ReasonType", x => "-");
                    cols.Add("ReasonName", x => "-");
                    cols.Add("CreatedBy", x => x.CreatedBy);
                    cols.Add("CreatedAt", x => x.CreatedAt);
                    cols.Add("ModifiedBy", x => x.ModifiedBy);
                    cols.Add("ModifiedAt", x => x.ModifiedAt);
                    cols.Add("ApprovedBy", x => x.ApprovedBy);
                    cols.Add("ApprovedAt", x => x.ApprovedAt);


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

            }

            return list.ToList();
        }

        public IEnumerable<TrxAccidentHeader> GetAccidentData(string WarehouseId, string startDate, string endDate)
        {
            IEnumerable<TrxAccidentHeader> list = Enumerable.Empty<TrxAccidentHeader>();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime EndDate = DateTime.Parse(endDate);

                try
                {
                    IQueryable<TrxAccidentHeader> query = db.TrxAccidentHeaders.AsQueryable()
                    .Where(x => x.WarehouseId.Equals(WarehouseId)
                    && x.IsDeleted == false);
                    //&& x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));

                    query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.CreatedAt) <= DbFunctions.TruncateTime(EndDate));

                    //columns sorting
                    Dictionary<string, Func<TrxAccidentHeader, object>> cols = new Dictionary<string, Func<TrxAccidentHeader, object>>();
                    cols.Add("TransactionCode", x => x.TransactionCode);
                    cols.Add("RefNumber", x => x.RefNumber);
                    cols.Add("AccidentType", x => x.AccidentType);
                    cols.Add("Remarks", x => x.Remarks);
                    cols.Add("WarehouseCode", x => x.WarehouseCode);
                    cols.Add("WarehouseName", x => x.WarehouseName);
                    cols.Add("TransactionStatus", x => x.TransactionStatus);
                    cols.Add("CreatedBy", x => x.CreatedBy);
                    cols.Add("CreatedAt", x => x.CreatedAt);
                    cols.Add("ModifiedBy", x => x.ModifiedBy);
                    cols.Add("ModifiedAt", x => x.ModifiedAt);
                    cols.Add("ApprovedBy", x => x.ApprovedBy);
                    cols.Add("ApprovedAt", x => x.ApprovedAt);

                    list = query.OrderBy(cols["TransactionCode"]);

                }
                catch (Exception e)
                {
                    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                    logger.Error(e, errMsg);
                }

            }

            return list.ToList();
        }

        public int GetTotalAccident(string WarehouseId, string startDate, string endDate)
        {
            int totalData = 0;

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime EndDate = DateTime.Parse(endDate);

                try
                {
                    IQueryable<TrxAccidentHeader> query = db.TrxAccidentHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);
                    query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.CreatedAt) <= DbFunctions.TruncateTime(EndDate));
                    totalData = query.Count();
                }
                catch (Exception e)
                {
                    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                    logger.Error(e, errMsg);
                }
            }

            return totalData;
        }

        public IEnumerable<TrxAccidentHeader> GetAllTransactions(string warehouseId)
        {
            IQueryable<TrxAccidentHeader> query = db.TrxAccidentHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId) && x.IsDeleted == false);
            IEnumerable<TrxAccidentHeader> list = null;
            try
            {
                list = query.OrderByDescending(x => x.TransactionCode);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public async Task<bool> UpdateItemAsync(TrxAccidentItem item)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
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