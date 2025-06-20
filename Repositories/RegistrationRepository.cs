using Facelift_App.Helper;
using Facelift_App.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Facelift_App.Repositories
{
    public class RegistrationRepository : IRegistrations
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<bool> CloseAsync(TrxRegistrationHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ApprovedAt = DateTime.Now;

                    LogRegistrationHeader log = new LogRegistrationHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Posted",
                        ExecutedBy = data.ApprovedBy,
                        ExecutedAt = data.ApprovedAt.Value
                    };

                    db.LogRegistrationHeaders.Add(log);

                    DateTime currentDate = DateTime.Now;

                    int year = currentDate.Year;
                    int month = currentDate.Month;

                    //loop insert to pallet
                    foreach (TrxRegistrationItem item in data.TrxRegistrationItems)
                    {
                       
                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                        if(pallet == null)
                        {
                            pallet = new MsPallet()
                            {
                                TagId = item.TagId,
                                PalletCode = item.TagId,
                                PalletTypeId = data.PalletTypeId,
                                PalletName = data.PalletName,
                                PalletCondition = Constant.PalletCondition.GOOD.ToString(),
                                WarehouseId = data.WarehouseId,
                                WarehouseCode = data.WarehouseCode,
                                WarehouseName = data.WarehouseName,
                                CompanyId = data.CompanyId,
                                PalletOwner = data.PalletOwner,
                                PalletProducer = data.PalletProducer,
                                ProducedDate = data.ProducedDate,
                                Description = data.Description,
                                RegisteredBy = data.ApprovedBy,
                                RegisteredAt = data.ApprovedAt.Value,
                                ReceivedBy = data.ApprovedBy,
                                ReceivedAt = data.ApprovedAt.Value,
                                PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString(),
                                LastTransactionName = Constant.TransactionName.REGISTRATION.ToString(),
                                LastTransactionCode = data.TransactionCode,
                                LastTransactionDate = currentDate
                            };
                            db.MsPallets.Add(pallet);

                            //start billing for unused
                            //MsPalletAging agingDest = new MsPalletAging
                            //{
                            //    AgingId = Utilities.CreateGuid("PAG"),
                            //    PalletId = item.TagId,
                            //    WarehouseId = pallet.WarehouseId,
                            //    ReceivedAt = currentDate,
                            //    CurrentMonth = month,
                            //    CurrentYear = year,
                            //    IsActive = true,
                            //    AgingType = Constant.AgingType.UNUSED.ToString()
                            //};

                            //db.MsPalletAgings.Add(agingDest);

                        }
                        else
                        {
                            item.PalletTagId = pallet.TagId;
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

        public async Task<bool> CreateAsync(TrxRegistrationHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;

                    MsWarehouse warehouse = await db.MsWarehouses.FindAsync(data.WarehouseId);

                    string prefix = data.TransactionId.Substring(0, 3);
                    string palletOwner = data.PalletOwner;
                    string warehouseCode = data.WarehouseCode;
                    string warehouseAlias = warehouse.WarehouseAlias;
                    int year = Convert.ToInt32(data.CreatedAt.Year.ToString().Substring(2));
                    int month = data.CreatedAt.Month;
                    string romanMonth = Utilities.ConvertMonthToRoman(month);
                   
                    // get last number, and do increment.
                     string lastNumber = db.TrxRegistrationHeaders.AsQueryable().OrderByDescending(x => x.TransactionCode)
                         .Where(x => x.WarehouseCode.Equals(warehouseCode) && x.CreatedAt.Year.Equals(data.CreatedAt.Year) && x.CreatedAt.Month.Equals(data.CreatedAt.Month))
                         .AsEnumerable().Select(x => x.TransactionCode).FirstOrDefault();
                    int currentNumber = 0;

                    if (!string.IsNullOrEmpty(lastNumber))
                    {
                        currentNumber = Int32.Parse(lastNumber.Substring(lastNumber.Length - 3));
                    }

                    string runningNumber = string.Format("{0:D3}", currentNumber + 1);

                    data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}/{5}", prefix, palletOwner, warehouseAlias, year, romanMonth, runningNumber);

                    LogRegistrationHeader log = new LogRegistrationHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Create",
                        ExecutedBy = data.CreatedBy,
                        ExecutedAt = data.CreatedAt
                    };

                    LogRegistrationDocument logDoc = new LogRegistrationDocument
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        DeliveryNote = data.DeliveryNote,
                        Description = data.Description,
                        PalletTypeId = data.PalletTypeId,
                        PalletName = data.PalletName,
                        WarehouseId = data.WarehouseId,
                        WarehouseCode = data.WarehouseCode,
                        WarehouseName = data.WarehouseName,
                        PalletOwner = data.PalletOwner,
                        PalletProducer = data.PalletProducer,
                        ProducedDate = data.ProducedDate,
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

                    db.TrxRegistrationHeaders.Add(data);
                    db.LogRegistrationHeaders.Add(log);
                    db.LogRegistrationDocuments.Add(logDoc);
                    

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

        public async Task<bool> DeleteItemAsync(TrxRegistrationHeader data, string[] items, string username)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogRegistrationHeader log = new LogRegistrationHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Delete Item",
                        ExecutedBy = username,
                        ExecutedAt = DateTime.Now
                    };

                    db.LogRegistrationHeaders.Add(log);


                    //delete all items
                    db.TrxRegistrationItems.RemoveRange(db.TrxRegistrationItems.Where(m => m.TransactionId.Equals(data.TransactionId) && items.Contains(m.TransactionItemId) ));


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

        public async Task<TrxRegistrationHeader> GetDataByIdAsync(string id)
        {
            TrxRegistrationHeader data = null;
            try
            {
                data = await db.TrxRegistrationHeaders.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public TrxRegistrationHeader GetDataById(string id)
        {
            TrxRegistrationHeader data = null;
            try
            {
                data = db.TrxRegistrationHeaders.Find(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public IEnumerable<TrxRegistrationHeader> GetFilteredData(string WarehouseId, string stats, string search, string sortDirection, string sortColName)
        {
            IQueryable<TrxRegistrationHeader> query = db.TrxRegistrationHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);
            IEnumerable<TrxRegistrationHeader> list = Enumerable.Empty<TrxRegistrationHeader>();
            try
            {

                if (!string.IsNullOrEmpty(stats))
                {
                    if (stats.Equals("0"))
                    {
                        query = query.Where(m => !m.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));
                    }else if (stats.Equals("1"))
                    {
                        query = query.Where(m => m.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));
                    }
                }

                query = query
                        .Where(m => m.TransactionCode.Contains(search) ||
                           m.DeliveryNote.Contains(search) || m.MsPalletType.PalletName.Contains(search) || m.MsWarehouse.WarehouseName.Contains(search) || 
                           m.TransactionStatus.Contains(search));


                //columns sorting
                Dictionary<string, Func<TrxRegistrationHeader, object>> cols = new Dictionary<string, Func<TrxRegistrationHeader, object>>();
                cols.Add("TransactionCode", x => x.TransactionCode);
                cols.Add("DeliveryNote", x => x.DeliveryNote);
                //cols.Add("Description", x => x.Description);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("WarehouseName", x => x.WarehouseName);
                //cols.Add("MsPalletType.PalletName", x => x.MsPalletType != null ? x.MsPalletType.PalletName : null);
                //cols.Add("MsWarehouse.WarehouseName", x => x.MsWarehouse != null ? x.MsWarehouse.WarehouseName : null);
                cols.Add("PalletOwner", x => x.PalletOwner);
                //cols.Add("MsProducer.ProducerName", x => x.MsProducer != null ? x.MsProducer.ProducerName : null);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("ProducedDate", x => x.ProducedDate);
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
                IQueryable<TrxRegistrationHeader> query = db.TrxRegistrationHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);

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

        public async Task<bool> InsertItemAsync(TrxRegistrationHeader data, string username, string actionName)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogRegistrationHeader log = new LogRegistrationHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = actionName,
                        ExecutedBy = username,
                        ExecutedAt = DateTime.Now
                    };


                    db.LogRegistrationHeaders.Add(log);


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

        public async Task<bool> UpdateAsync(TrxRegistrationHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;

                    LogRegistrationHeader log = new LogRegistrationHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Modify",
                        ExecutedBy = data.ModifiedBy,
                        ExecutedAt = data.ModifiedAt.Value
                    };

                    int currentVersion = db.LogRegistrationDocuments.AsQueryable().OrderByDescending(x => x.Version)
                       .Where(x => x.TransactionId.Equals(data.TransactionId))
                       .AsEnumerable().Select(x => x.Version).FirstOrDefault();


                    LogRegistrationDocument logDoc = new LogRegistrationDocument
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        DeliveryNote = data.DeliveryNote,
                        Description = data.Description,
                        PalletTypeId = data.PalletTypeId,
                        PalletName = data.PalletName,
                        WarehouseId = data.WarehouseId,
                        WarehouseCode = data.WarehouseCode,
                        WarehouseName = data.WarehouseName,
                        PalletOwner = data.PalletOwner,
                        PalletProducer = data.PalletProducer,
                        ProducedDate = data.ProducedDate,
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

                   
                    db.LogRegistrationHeaders.Add(log);
                    db.LogRegistrationDocuments.Add(logDoc);


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

        public IEnumerable<VwRegistrationItem> GetFilteredDataItem(string TransactionId, string search, string sortDirection, string sortColName)
        {
            IQueryable<VwRegistrationItem> query = db.VwRegistrationItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwRegistrationItem> list = null;
            try
            {
                query = query
                        .Where(m => m.TagId.Contains(search) || m.ItemStatus.Contains(search));

                //columns sorting
                Dictionary<string, Func<VwRegistrationItem, object>> cols = new Dictionary<string, Func<VwRegistrationItem, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("ItemStatus", x => x.ItemStatus);
                cols.Add("InsertedBy", x => x.InsertedBy);
                cols.Add("InsertedAt", x => x.InsertedAt);
                cols.Add("InsertMethod", x => x.InsertMethod);


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

        public IEnumerable<VwRegistrationItem> GetDataItem(string TransactionId)
        {
            IQueryable<VwRegistrationItem> query = db.VwRegistrationItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwRegistrationItem> list = null;
            try
            {

                //columns sorting
                Dictionary<string, Func<VwRegistrationItem, object>> cols = new Dictionary<string, Func<VwRegistrationItem, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("ItemStatus", x => x.ItemStatus);
                cols.Add("InsertedBy", x => x.InsertedBy);
                cols.Add("InsertedAt", x => x.InsertedAt);
                cols.Add("InsertMethod", x => x.InsertMethod);

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
                IQueryable<VwRegistrationItem> query = db.VwRegistrationItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<TrxRegistrationItem> GetDataByTransactionTagIdAsync(string TransactionId, string TagId)
        {
            TrxRegistrationItem data = null;
            try
            {
                data = await db.TrxRegistrationItems.Where(x => x.TransactionId.Equals(TransactionId) && x.TagId.Equals(TagId)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<bool> DeleteAsync(TrxRegistrationHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;

                    LogRegistrationHeader log = new LogRegistrationHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Delete",
                        ExecutedBy = data.ModifiedBy,
                        ExecutedAt = data.ModifiedAt.Value
                    };

                    db.LogRegistrationHeaders.Add(log);


                    //delete all items
                    db.TrxRegistrationItems.RemoveRange(db.TrxRegistrationItems.Where(m => m.TransactionId.Equals(data.TransactionId)));


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

        public IEnumerable<TrxRegistrationHeader> GetAvailableTransactions(string warehouseId)
        {
            IQueryable<TrxRegistrationHeader> query = db.TrxRegistrationHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()) && x.IsDeleted == false);
            IEnumerable<TrxRegistrationHeader> list = null;
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

        public IEnumerable<VwRegistrationItem> GetDetailByTransactionId(string TransactionId)
        {
            IQueryable<VwRegistrationItem> query = db.VwRegistrationItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwRegistrationItem> list = null;
            try
            {
                list = query.OrderBy(x => x.InsertedAt);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }

            return list.ToList();
        }

        public IEnumerable<TrxRegistrationHeader> GetAllTransactions(string warehouseId)
        {
            IQueryable<TrxRegistrationHeader> query = db.TrxRegistrationHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId) && x.IsDeleted == false);
            IEnumerable<TrxRegistrationHeader> list = null;
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