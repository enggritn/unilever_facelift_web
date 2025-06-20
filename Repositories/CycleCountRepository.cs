using Facelift_App.Helper;
using Facelift_App.Services;
using NLog;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Facelift_App.Repositories
{
    public class CycleCountRepository : ICycleCounts
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<bool> CloseAsync(TrxCycleCountHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ApprovedAt = DateTime.Now;

                    LogCycleCountHeader log = new LogCycleCountHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Approve",
                        ExecutedBy = data.ApprovedBy,
                        ExecutedAt = data.ApprovedAt.Value
                    };

                    db.LogCycleCountHeaders.Add(log);

                    ////loop update to pallet
                    //foreach (TrxCycleCountItem item in data.TrxCycleCountItems)
                    //{
                    //    item.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                    //    //check if exist, ignore
                    //    MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                    //    if (pallet != null)
                    //    {
                    //        pallet.PalletCondition = Constant.PalletCondition.GOOD.ToString();
                    //        pallet.PalletMovementStatus = item.PalletMovementStatus;
                    //        pallet.WarehouseId = data.WarehouseId;
                    //        pallet.WarehouseCode = data.WarehouseCode;
                    //        pallet.WarehouseName = data.WarehouseName;
                    //    }

                    //    //update pallet billing if found or extra found
                    //}

                    List<string> items = data.TrxCycleCountItems.Select(m => m.TagId).ToList();
                    List<MsPallet> pallets = await db.MsPallets.Where(m => items.Contains(m.TagId)).ToListAsync();

                    foreach (MsPallet pallet in pallets)
                    {
                        var item = data.TrxCycleCountItems.Where(m => m.TagId.Equals(pallet.TagId)).FirstOrDefault();
                        item.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                        pallet.PalletCondition = Constant.PalletCondition.GOOD.ToString();
                        pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                        pallet.WarehouseId = data.WarehouseId;
                        pallet.WarehouseCode = data.WarehouseCode;
                        pallet.WarehouseName = data.WarehouseName;
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

        public async Task<bool> CreateAsync(TrxCycleCountHeader data)
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

                    string prefix = data.TransactionId.Substring(0, 3);
                    //string palletOwner = company.CompanyAbb;
                    string warehouseCode = data.WarehouseCode;
                    string warehouseAlias = warehouse.WarehouseAlias;
                    int year = Convert.ToInt32(data.CreatedAt.Year.ToString().Substring(2));
                    int month = data.CreatedAt.Month;
                    string romanMonth = Utilities.ConvertMonthToRoman(month);

                    // get last number, and do increment.
                    string lastNumber = db.TrxCycleCountHeaders.AsQueryable().OrderByDescending(x => x.TransactionCode)
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

                    LogCycleCountHeader log = new LogCycleCountHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Create",
                        ExecutedBy = data.CreatedBy,
                        ExecutedAt = data.CreatedAt
                    };

                    LogCycleCountDocument logDoc = new LogCycleCountDocument
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        Remarks = data.Remarks,
                        WarehouseId = data.WarehouseId,
                        WarehouseCode = data.WarehouseCode,
                        WarehouseName = data.WarehouseName,
                        TransactionStatus = data.TransactionStatus,
                        IsDeleted = data.IsDeleted,
                        AccidentId = data.AccidentId,
                        CreatedBy = data.CreatedBy,
                        CreatedAt = data.CreatedAt,
                        ModifiedBy = data.ModifiedBy,
                        ModifiedAt = data.ModifiedAt,
                        ApprovedBy = data.ApprovedBy,
                        ApprovedAt = data.ApprovedAt,
                        Version = 1
                    };

                    data.TrxCycleCountItems = new List<TrxCycleCountItem>();


                    //insert all pallet to item
                    //foreach(MsPallet pallet in await db.MsPallets.Where(p => p.WarehouseId.Equals(data.WarehouseId) 
                    //&& !p.PalletCondition.Equals(Constant.PalletCondition.DAMAGE.ToString())).ToListAsync())
                    //commented by Muhammad Bhovdair 4 Aug 2021, only stock opname for good pallet only, damage and loss will be ignored
                    foreach (MsPallet pallet in await db.MsPallets.Where(p => p.WarehouseId.Equals(data.WarehouseId)
                    && p.PalletCondition.Equals(Constant.PalletCondition.GOOD.ToString()) && !p.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString())).ToListAsync())
                    {
                        //insert to detail
                        TrxCycleCountItem item = new TrxCycleCountItem();
                        item.TransactionItemId = Utilities.CreateGuid("STI");
                        item.TransactionId = data.TransactionId;
                        item.TagId = pallet.TagId;
                        item.PalletCondition = pallet.PalletCondition;
                        item.PalletMovementStatus = Constant.PalletMovementStatus.OP.ToString();
                        data.TrxCycleCountItems.Add(item);

                        //update pallet
                        pallet.PalletMovementStatus = item.PalletMovementStatus;
                        pallet.PalletCondition = Constant.PalletCondition.FREEZE.ToString();
                    }

                    db.TrxCycleCountHeaders.Add(data);
                    db.LogCycleCountHeaders.Add(log);
                    db.LogCycleCountDocuments.Add(logDoc);

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

        public async Task<bool> DeleteAsync(TrxCycleCountHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;

                    LogCycleCountHeader log = new LogCycleCountHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Delete",
                        ExecutedBy = data.ModifiedBy,
                        ExecutedAt = data.ModifiedAt.Value
                    };

                    db.LogCycleCountHeaders.Add(log);


                    //update all pallet status to ST
                    foreach (TrxCycleCountItem item in db.TrxCycleCountItems.Where(m => m.TransactionId.Equals(data.TransactionId)))
                    {
                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                        if (pallet != null)
                        {
                            pallet.PalletCondition = item.PalletCondition;
                            pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                        }
                    }

                    //delete all items
                    db.TrxCycleCountItems.RemoveRange(db.TrxCycleCountItems.Where(m => m.TransactionId.Equals(data.TransactionId)));


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

        public async Task<TrxCycleCountHeader> GetDataByIdAsync(string id)
        {
            TrxCycleCountHeader data = null;
            try
            {
                data = await db.TrxCycleCountHeaders.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public TrxCycleCountHeader GetDataById(string id)
        {
            TrxCycleCountHeader data = null;
            try
            {
                data = db.TrxCycleCountHeaders.Find(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<TrxCycleCountHeader> GetDataByTransactionCodeAsync(string TransactionCode)
        {
            TrxCycleCountHeader data = null;
            try
            {
                data = await db.TrxCycleCountHeaders.Where(m => m.TransactionCode.Equals(TransactionCode)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<TrxCycleCountItem> GetDataByTransactionTagIdAsync(string TransactionId, string TagId)
        {
            TrxCycleCountItem data = null;
            try
            {
                data = await db.TrxCycleCountItems.Where(x => x.TransactionId.Equals(TransactionId) && x.TagId.Equals(TagId)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public IEnumerable<VwCycleCountItem> GetDetailByTransactionId(string TransactionId)
        {
            IQueryable<VwCycleCountItem> query = db.VwCycleCountItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwCycleCountItem> list = null;
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

        public IEnumerable<TrxCycleCountHeader> GetFilteredData(string WarehouseId, string stats, string search, string sortDirection, string sortColName)
        {
            IQueryable<TrxCycleCountHeader> query = db.TrxCycleCountHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);
            IEnumerable<TrxCycleCountHeader> list = null;
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
                           m.TransactionStatus.Contains(search));

                //columns sorting
                Dictionary<string, Func<TrxCycleCountHeader, object>> cols = new Dictionary<string, Func<TrxCycleCountHeader, object>>();
                cols.Add("TransactionCode", x => x.TransactionCode);
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

        public IEnumerable<VwCycleCountItem> GetFilteredDataItem(string TransactionId, string search, string sortDirection, string sortColName)
        {
            IQueryable<VwCycleCountItem> query = db.VwCycleCountItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwCycleCountItem> list = null;
            try
            {
                query = query
                        .Where(m => m.TagId.Contains(search) || m.PalletCondition.Contains(search) || m.PalletName.Contains(search) || m.PalletMovementStatus.Contains(search));

                //columns sorting
                Dictionary<string, Func<VwCycleCountItem, object>> cols = new Dictionary<string, Func<VwCycleCountItem, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletCondition", x => x.PalletCondition);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
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

        public IEnumerable<VwCycleCountItem> GetDataItem(string TransactionId)
        {
            IQueryable<VwCycleCountItem> query = db.VwCycleCountItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwCycleCountItem> list = null;
            try
            {
                //columns sorting
                Dictionary<string, Func<VwCycleCountItem, object>> cols = new Dictionary<string, Func<VwCycleCountItem, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletCondition", x => x.PalletCondition);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
                cols.Add("ScannedBy", x => x.ScannedBy);
                cols.Add("ScannedAt", x => x.ScannedAt);
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
                IQueryable<TrxCycleCountHeader> query = db.TrxCycleCountHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);

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

        public int GetTotalDataItem(string TransactionId)
        {
            int totalData = 0;
            try
            {
                IQueryable<VwCycleCountItem> query = db.VwCycleCountItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<bool> UpdateAsync(TrxCycleCountHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;

                    LogCycleCountHeader log = new LogCycleCountHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Modify",
                        ExecutedBy = data.ModifiedBy,
                        ExecutedAt = data.ModifiedAt.Value
                    };


                    int currentVersion = db.LogCycleCountDocuments.AsQueryable().OrderByDescending(x => x.Version)
                         .Where(x => x.TransactionId.Equals(data.TransactionId))
                         .AsEnumerable().Select(x => x.Version).FirstOrDefault();


                    LogCycleCountDocument logDoc = new LogCycleCountDocument
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        Remarks = data.Remarks,
                        WarehouseId = data.WarehouseId,
                        WarehouseCode = data.WarehouseCode,
                        WarehouseName = data.WarehouseName,
                        TransactionStatus = data.TransactionStatus,
                        IsDeleted = data.IsDeleted,
                        AccidentId = data.AccidentId,
                        CreatedBy = data.CreatedBy,
                        CreatedAt = data.CreatedAt,
                        ModifiedBy = data.ModifiedBy,
                        ModifiedAt = data.ModifiedAt,
                        ApprovedBy = data.ApprovedBy,
                        ApprovedAt = data.ApprovedAt,
                        Version = currentVersion + 1
                    };


                    db.LogCycleCountHeaders.Add(log);
                    db.LogCycleCountDocuments.Add(logDoc);



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

        public async Task<bool> UpdateItemAsync(TrxCycleCountHeader data, List<string> items, string username, string actionName)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogCycleCountHeader log = new LogCycleCountHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = actionName,
                        ExecutedBy = username,
                        ExecutedAt = DateTime.Now
                    };


                    db.LogCycleCountHeaders.Add(log);
                    //List<TrxCycleCountItem> cycleCountItems = data.TrxCycleCountItems.Where(m => items.Contains(m.TagId)).ToList();

                    List<MsPallet> pallets = await db.MsPallets.Where(m => items.Contains(m.TagId)).ToListAsync();

                    foreach(MsPallet pallet in pallets)
                    {
                        var item = data.TrxCycleCountItems.Where(m => m.TagId.Equals(pallet.TagId)).FirstOrDefault();
                        if(item != null)
                        {
                            if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString()))
                            {
                                pallet.PalletMovementStatus = item.PalletMovementStatus;
                                pallet.LastTransactionName = Constant.TransactionName.STOCK_TAKE.ToString();
                                pallet.LastTransactionCode = data.TransactionCode;
                                pallet.LastTransactionDate = DateTime.Now;
                            }
                            //if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.IN.ToString()))
                            //{
                            //    pallet.WarehouseId = data.WarehouseId;
                            //    pallet.WarehouseCode = data.WarehouseCode;
                            //    pallet.WarehouseName = data.WarehouseName;
                            //    pallet.PalletCondition = Constant.PalletCondition.GOOD.ToString();
                            //    pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                            //    pallet.LastTransactionName = Constant.TransactionName.STOCK_TAKE.ToString();
                            //    pallet.LastTransactionCode = data.TransactionCode;
                            //    pallet.LastTransactionDate = DateTime.Now;
                            //}
                            //if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OP.ToString()))
                            //{
                            //    pallet.PalletCondition = Constant.PalletCondition.GOOD.ToString();
                            //    pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                            //    pallet.LastTransactionName = Constant.TransactionName.STOCK_TAKE.ToString();
                            //    pallet.LastTransactionCode = data.TransactionCode;
                            //    pallet.LastTransactionDate = DateTime.Now;
                            //}
                        }
                        
                    }


                    //loop update to pallet
                    //foreach (TrxCycleCountItem item in cycleCountItems)
                    //{
                    //    if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString()))
                    //    {
                    //        //check if exist, ignore
                    //        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                    //        if (pallet != null)
                    //        {
                    //            pallet.PalletMovementStatus = item.PalletMovementStatus;
                    //            pallet.LastTransactionName = Constant.TransactionName.STOCK_TAKE.ToString();
                    //            pallet.LastTransactionCode = data.TransactionCode;
                    //            pallet.LastTransactionDate = DateTime.Now;
                    //        }
                    //    }
                    //}

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

        public async Task<bool> UpdatePalletAsync(string WarehouseId, string TagId, string PalletCondition)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    MsWarehouse warehouse = await db.MsWarehouses.FindAsync(WarehouseId);
                    if (warehouse != null)
                    {
                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(TagId);
                        if (pallet != null)
                        {
                            pallet.WarehouseId = warehouse.WarehouseId;
                            pallet.WarehouseCode = warehouse.WarehouseCode;
                            pallet.WarehouseName = warehouse.WarehouseName;
                            pallet.PalletCondition = PalletCondition;
                            pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();

                            await db.SaveChangesAsync();
                            transaction.Commit();
                            status = true;
                        }
                    }
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

        public int GetTotalOpenData(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxCycleCountHeader> query = db.TrxCycleCountHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false && !x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public int GetTotalProgressShipment(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable().Where(x => x.DestinationId.Equals(WarehouseId) && x.TransactionStatus.Equals("PROGRESS") && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<TrxCycleCountHeader> GetList(string warehouseId)
        {
            IQueryable<TrxCycleCountHeader> query = db.TrxCycleCountHeaders.AsQueryable()
                .Where(x => x.WarehouseId.Equals(warehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString())
                && x.IsDeleted == false);
            IEnumerable<TrxCycleCountHeader> list = null;
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

        public async Task<bool> CreateAccidentReportAsync(TrxCycleCountHeader data, string username)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogCycleCountHeader log = new LogCycleCountHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Create BA",
                        ExecutedBy = username,
                        ExecutedAt = DateTime.Now
                    };

                    db.LogCycleCountHeaders.Add(log);

                    //loop update to pallet
                    foreach (TrxCycleCountItem item in data.TrxCycleCountItems)
                    {
                        if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.OT.ToString()))
                        {
                            item.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                            //check if exist, ignore
                            MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                            if (pallet != null)
                            {
                                pallet.PalletCondition = Constant.PalletCondition.GOOD.ToString();
                                pallet.PalletMovementStatus = item.PalletMovementStatus;
                            }
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

        public IEnumerable<TrxCycleCountHeader> GetCycleCountData(string WarehouseId, string startDate, string endDate, string search, string sortDirection, string sortColName)
        {
            IEnumerable<TrxCycleCountHeader> list = Enumerable.Empty<TrxCycleCountHeader>();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime EndDate = DateTime.Parse(endDate);

                try
                {
                    IQueryable<TrxCycleCountHeader> query = db.TrxCycleCountHeaders.AsQueryable()
                    .Where(x => x.WarehouseId.Equals(WarehouseId)
                    && x.IsDeleted == false);
                    //&& x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));

                    query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.CreatedAt) <= DbFunctions.TruncateTime(EndDate));
                    query = query.Where(m => m.TransactionCode.Contains(search));

                    //columns sorting
                    Dictionary<string, Func<TrxCycleCountHeader, object>> cols = new Dictionary<string, Func<TrxCycleCountHeader, object>>();
                    cols.Add("TransactionCode", x => x.TransactionCode);
                    cols.Add("Remarks", x => x.Remarks);
                    cols.Add("WarehouseId", x => x.WarehouseId);
                    cols.Add("WarehouseCode", x => x.WarehouseCode);
                    cols.Add("WarehouseName", x => x.WarehouseName);
                    cols.Add("TransactionStatus", x => x.TransactionStatus);
                    cols.Add("AccidentId", x => x.AccidentId);
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

        public IEnumerable<TrxCycleCountHeader> GetCycleCountData(string WarehouseId, string startDate, string endDate)
        {
            IEnumerable<TrxCycleCountHeader> list = Enumerable.Empty<TrxCycleCountHeader>();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime EndDate = DateTime.Parse(endDate);

                try
                {
                    IQueryable<TrxCycleCountHeader> query = db.TrxCycleCountHeaders.AsQueryable()
                    .Where(x => x.WarehouseId.Equals(WarehouseId)
                    && x.IsDeleted == false);
                    //&& x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));

                    query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.CreatedAt) <= DbFunctions.TruncateTime(EndDate));

                    //columns sorting
                    Dictionary<string, Func<TrxCycleCountHeader, object>> cols = new Dictionary<string, Func<TrxCycleCountHeader, object>>();
                    cols.Add("TransactionCode", x => x.TransactionCode);
                    cols.Add("Remarks", x => x.Remarks);
                    cols.Add("WarehouseId", x => x.WarehouseId);
                    cols.Add("WarehouseCode", x => x.WarehouseCode);
                    cols.Add("WarehouseName", x => x.WarehouseName);
                    cols.Add("TransactionStatus", x => x.TransactionStatus);
                    cols.Add("AccidentId", x => x.AccidentId);
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

        public int GetTotalCycleCount(string WarehouseId, string startDate, string endDate)
        {
            int totalData = 0;

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime EndDate = DateTime.Parse(endDate);

                try
                {
                    IQueryable<TrxCycleCountHeader> query = db.TrxCycleCountHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);
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

        public IEnumerable<TrxCycleCountHeader> GetAllTransactions(string warehouseId)
        {
            IQueryable<TrxCycleCountHeader> query = db.TrxCycleCountHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId) && x.IsDeleted == false);
            IEnumerable<TrxCycleCountHeader> list = null;
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
    }
}