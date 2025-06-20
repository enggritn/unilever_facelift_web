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
    public class ShipmentRepository : IShipments
    {
        FaceliftEntities db = new FaceliftEntities();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<bool> CloseAsync(TrxShipmentHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ApprovedAt = DateTime.Now;

                    LogShipmentHeader log = new LogShipmentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Posted",
                        ExecutedBy = data.ApprovedBy,
                        ExecutedAt = data.ApprovedAt.Value
                    };

                    db.LogShipmentHeaders.Add(log);

                    //loop insert to pallet
                    foreach(TrxShipmentItem item in data.TrxShipmentItems)
                    {
                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                        if(pallet != null)
                        {
                            pallet.PalletMovementStatus = item.PalletMovementStatus;
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

        public async Task<bool> CreateAsync(TrxShipmentHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.CreatedAt = DateTime.Now;

                    MsWarehouse warehouse = await db.MsWarehouses.FindAsync(data.WarehouseId);
                    MsWarehouse destination = await db.MsWarehouses.FindAsync(data.DestinationId);

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
                     string lastNumber = db.TrxShipmentHeaders.AsQueryable().OrderByDescending(x => x.CreatedAt)
                         .Where(x => x.WarehouseCode.Equals(warehouseCode) && x.CreatedAt.Year.Equals(data.CreatedAt.Year) && x.CreatedAt.Month.Equals(data.CreatedAt.Month))
                         .AsEnumerable().Select(x => x.TransactionCode).FirstOrDefault();
                    int currentNumber = 0;

                    if (!string.IsNullOrEmpty(lastNumber))
                    {
                        currentNumber = Int32.Parse(lastNumber.Substring(lastNumber.Length - 3));
                    }

                    string runningNumber = string.Format("{0:D3}", currentNumber + 1);

                    //data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}/{5}", prefix, palletOwner, warehouseAlias, year, romanMonth, runningNumber);
                    data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}/{5}", prefix, warehouseAlias, destination.WarehouseAlias, year, romanMonth, runningNumber);

                    LogShipmentHeader log = new LogShipmentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Create",
                        ExecutedBy = data.CreatedBy,
                        ExecutedAt = data.CreatedAt
                    };

                    LogShipmentDocument logDoc = new LogShipmentDocument
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ShipmentNumber = data.ShipmentNumber,
                        Remarks = data.Remarks,
                        WarehouseId = data.WarehouseId,
                        WarehouseCode = data.WarehouseCode,
                        WarehouseName = data.WarehouseName,
                        DestinationId = data.DestinationId,
                        DestinationCode = data.DestinationCode,
                        DestinationName = data.DestinationName,
                        TransporterId = data.TransporterId,
                        TransporterName = data.TransporterName,
                        DriverId = data.DriverId,
                        DriverName = data.DriverName,
                        TruckId = data.TruckId,
                        PlateNumber = data.PlateNumber,
                        PalletQty = data.PalletQty,
                        TransactionStatus = data.TransactionStatus,
                        ShipmentStatus = data.ShipmentStatus,
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

                    db.TrxShipmentHeaders.Add(data);
                    db.LogShipmentHeaders.Add(log);
                    db.LogShipmentDocuments.Add(logDoc);

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

        public async Task<bool> DeleteItemAsync(TrxShipmentHeader data, string[] items, string username)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogShipmentHeader log = new LogShipmentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Delete Item",
                        ExecutedBy = username,
                        ExecutedAt = DateTime.Now
                    };

                    db.LogShipmentHeaders.Add(log);

                    foreach (TrxShipmentItem item in db.TrxShipmentItems.Where(m => m.TransactionId.Equals(data.TransactionId) && items.Contains(m.TransactionItemId)))
                    {
                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                        if (pallet != null)
                        {
                            pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                        }
                    }

                    //delete all items
                    db.TrxShipmentItems.RemoveRange(db.TrxShipmentItems.Where(m => m.TransactionId.Equals(data.TransactionId) && items.Contains(m.TransactionItemId) ));

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

        public async Task<TrxShipmentHeader> GetDataByIdAsync(string id)
        {
            TrxShipmentHeader data = null;
            try
            {
                data = await db.TrxShipmentHeaders.FindAsync(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public TrxShipmentHeader GetDataById(string id)
        {
            TrxShipmentHeader data = null;
            try
            {
                data = db.TrxShipmentHeaders.Find(id);
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public IEnumerable<TrxShipmentHeader> GetFilteredData(string WarehouseId, string search, string sortDirection, string sortColName)
        {
            IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);
            IEnumerable<TrxShipmentHeader> list = null;
            try
            {
                query = query
                        .Where(m => m.TransactionCode.Contains(search) ||
                           m.ShipmentNumber.Contains(search) || m.DestinationName.Contains(search) ||
                           m.TransporterName.Contains(search) || m.DriverName.Contains(search) || m.PlateNumber.Contains(search) ||
                           m.TransactionStatus.Contains(search) || m.ShipmentStatus.Contains(search));

                //columns sorting
                Dictionary<string, Func<TrxShipmentHeader, object>> cols = new Dictionary<string, Func<TrxShipmentHeader, object>>();
                cols.Add("TransactionCode", x => x.TransactionCode);
                cols.Add("ShipmentNumber", x => x.ShipmentNumber);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("DestinationName", x => x.DestinationName);
                cols.Add("TransporterName", x => x.TransporterName);
                cols.Add("DriverName", x => x.DriverName);
                cols.Add("PlateNumber", x => x.PlateNumber);
                cols.Add("PalletQty", x => x.PalletQty);
                cols.Add("TransactionStatus", x => x.TransactionStatus);
                cols.Add("ShipmentStatus", x => x.ShipmentStatus);
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

        public int GetTotalData(string WarehouseId)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<bool> InsertItemAsync(TrxShipmentHeader data, string username, string actionName)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogShipmentHeader log = new LogShipmentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = actionName,
                        ExecutedBy = username,
                        ExecutedAt = DateTime.Now
                    };

                    db.LogShipmentHeaders.Add(log);

                    await db.SaveChangesAsync();

                    //loop update to pallet
                    foreach (TrxShipmentItem item in data.TrxShipmentItems)
                    {
                        //check if exist, ignore
                        TrxShipmentItem item2 = db.TrxShipmentItems.Where(m => m.TagId.Equals(item.TagId)).FirstOrDefault();
                        if(item2 != null)
                        {
                            MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                            if (pallet != null)
                            {
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

        public async Task<bool> UpdateAsync(TrxShipmentHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;

                    LogShipmentHeader log = new LogShipmentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Modify",
                        ExecutedBy = data.ModifiedBy,
                        ExecutedAt = data.ModifiedAt.Value
                    };

                    int currentVersion = db.LogShipmentDocuments.AsQueryable().OrderByDescending(x => x.Version)
                         .Where(x => x.TransactionId.Equals(data.TransactionId))
                         .AsEnumerable().Select(x => x.Version).FirstOrDefault();

                    LogShipmentDocument logDoc = new LogShipmentDocument
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ShipmentNumber = data.ShipmentNumber,
                        Remarks = data.Remarks,
                        WarehouseId = data.WarehouseId,
                        WarehouseCode = data.WarehouseCode,
                        WarehouseName = data.WarehouseName,
                        DestinationId = data.DestinationId,
                        DestinationCode = data.DestinationCode,
                        DestinationName = data.DestinationName,
                        TransporterId = data.TransporterId,
                        TransporterName = data.TransporterName,
                        DriverId = data.DriverId,
                        DriverName = data.DriverName,
                        TruckId = data.TruckId,
                        PlateNumber = data.PlateNumber,
                        PalletQty = data.PalletQty,
                        TransactionStatus = data.TransactionStatus,
                        ShipmentStatus = data.ShipmentStatus,
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

                    db.LogShipmentHeaders.Add(log);
                    db.LogShipmentDocuments.Add(logDoc);

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

        public IEnumerable<VwShipmentItem> GetFilteredDataItem(string TransactionId, string search, string sortDirection, string sortColName)
        {
            IQueryable<VwShipmentItem> query = db.VwShipmentItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwShipmentItem> list = null;
            try
            {
                query = query
                        .Where(m => m.TagId.Contains(search) || m.PalletName.Contains(search) || m.PalletMovementStatus.Contains(search));

                //columns sorting
                Dictionary<string, Func<VwShipmentItem, object>> cols = new Dictionary<string, Func<VwShipmentItem, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
                cols.Add("ScannedBy", x => x.ScannedBy);
                cols.Add("ScannedAt", x => x.ScannedAt);
                cols.Add("DispatchedBy", x => x.DispatchedBy);
                cols.Add("DispatchedAt", x => x.DispatchedAt);
                cols.Add("ReceivedBy", x => x.ReceivedBy);
                cols.Add("ReceivedAt", x => x.ReceivedAt);

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

        public IEnumerable<VwShipmentItem> GetDataItem(string TransactionId)
        {
            IQueryable<VwShipmentItem> query = db.VwShipmentItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwShipmentItem> list = null;
            try
            {
                //columns sorting
                Dictionary<string, Func<VwShipmentItem, object>> cols = new Dictionary<string, Func<VwShipmentItem, object>>();
                cols.Add("TagId", x => x.TagId);
                cols.Add("PalletName", x => x.PalletName);
                cols.Add("PalletOwner", x => x.PalletOwner);
                cols.Add("PalletProducer", x => x.PalletProducer);
                cols.Add("PalletMovementStatus", x => x.PalletMovementStatus);
                cols.Add("ScannedBy", x => x.ScannedBy);
                cols.Add("ScannedAt", x => x.ScannedAt);
                cols.Add("DispatchedBy", x => x.DispatchedBy);
                cols.Add("DispatchedAt", x => x.DispatchedAt);
                cols.Add("ReceivedBy", x => x.ReceivedBy);
                cols.Add("ReceivedAt", x => x.ReceivedAt);

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
                IQueryable<VwShipmentItem> query = db.VwShipmentItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));

                totalData = query.Count();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public async Task<TrxShipmentItem> GetDataByTransactionTagIdAsync(string TransactionId, string TagId)
        {
            TrxShipmentItem data = null;
            try
            {
                data = await db.TrxShipmentItems.Where(x => x.TransactionId.Equals(TransactionId) && x.TagId.Equals(TagId)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<bool> DeleteAsync(TrxShipmentHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ModifiedAt = DateTime.Now;

                    LogShipmentHeader log = new LogShipmentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Delete",
                        ExecutedBy = data.ModifiedBy,
                        ExecutedAt = data.ModifiedAt.Value
                    };

                    db.LogShipmentHeaders.Add(log);


                    //update all pallet status to ST
                    foreach (TrxShipmentItem item in db.TrxShipmentItems.Where(m => m.TransactionId.Equals(data.TransactionId)))
                    {
                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                        if (pallet != null)
                        {
                            pallet.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                        }
                    }

                    //delete all items
                    db.TrxShipmentItems.RemoveRange(db.TrxShipmentItems.Where(m => m.TransactionId.Equals(data.TransactionId)));


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

        public IEnumerable<VwShipmentItem> GetDetailByTransactionId(string TransactionId)
        {
            IQueryable<VwShipmentItem> query = db.VwShipmentItems.AsQueryable().Where(x => x.TransactionId.Equals(TransactionId));
            IEnumerable<VwShipmentItem> list = null;
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

        public async Task<bool> DispatchItemAsync(TrxShipmentHeader data, string username, string actionName)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogShipmentHeader log = new LogShipmentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = actionName,
                        ExecutedBy = username,
                        ExecutedAt = DateTime.Now
                    };


                    db.LogShipmentHeaders.Add(log);

                    //loop update to pallet
                    foreach (TrxShipmentItem item in data.TrxShipmentItems)
                    {
                        item.DispatchedBy = username;
                        item.DispatchedAt = DateTime.Now;
                        item.PalletMovementStatus = Constant.PalletMovementStatus.OT.ToString();

                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                        if (pallet != null)
                        {
                            pallet.PalletMovementStatus = item.PalletMovementStatus;
                            pallet.LastTransactionName = Constant.TransactionName.SHIPMENT_OUTBOUND.ToString();
                            pallet.LastTransactionCode = data.TransactionCode;
                            pallet.LastTransactionDate = DateTime.Now;
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

        public async Task<bool> UpdateItemAsync(string WarehouseId, string TagId)
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
                            pallet.PalletCondition = Constant.PalletCondition.GOOD.ToString();
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

        public async Task<bool> ReceiveItemAsync(TrxShipmentHeader data, string username, string actionName)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogShipmentHeader log = new LogShipmentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = actionName,
                        ExecutedBy = username,
                        ExecutedAt = DateTime.Now
                    };


                    db.LogShipmentHeaders.Add(log);

                    DateTime currentDate = DateTime.Now;

                    int year = currentDate.Year;
                    int month = currentDate.Month;

                    //loop update to pallet
                    foreach (TrxShipmentItem item in data.TrxShipmentItems)
                    {
                        if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.IN.ToString()))
                        {
                            //if (data.ShipmentStatus.Equals(Constant.ShipmentStatus.RECEIVE.ToString()))
                            //{
                            //    item.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                            //}
                            item.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();

                            //check if exist, ignore
                            MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                            if (pallet != null)
                            {
                                if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()))
                                {
                                    pallet.WarehouseId = data.DestinationId;
                                    pallet.WarehouseCode = data.DestinationCode;
                                    pallet.WarehouseName = data.DestinationName;
                                }
                                pallet.PalletMovementStatus = item.PalletMovementStatus;
                                pallet.ReceivedBy = username;
                                pallet.ReceivedAt = currentDate;
                                pallet.LastTransactionName = Constant.TransactionName.SHIPMENT_INBOUND.ToString();
                                pallet.LastTransactionCode = data.TransactionCode;
                                pallet.LastTransactionDate = currentDate;

                                //generate aging row if already settled
                                //if (pallet.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()))
                                //{
                                //    //check previous data, if exist, close unused pallet aging from previous month
                                //    //update unused
                                //    //MsPalletAging unusedAging = await db.MsPalletAgings.Where(x => x.PalletId.Equals(item.TagId) && x.AgingType.Equals(Constant.AgingType.UNUSED.ToString()) && x.CurrentMonth.Equals(month) && x.CurrentYear.Equals(year) && x.WarehouseId.Equals(data.WarehouseId)).FirstOrDefaultAsync();
                                //    MsPalletAging unusedAging = await db.MsPalletAgings.Where(x => x.PalletId.Equals(item.TagId) && x.AgingType.Equals(Constant.AgingType.UNUSED.ToString()) && x.WarehouseId.Equals(data.WarehouseId)).FirstOrDefaultAsync();
                                //    //start billing if no row for exact pallet and warehouse
                                //    if (unusedAging != null && unusedAging.IsActive)
                                //    {
                                //        int totalminutes = (int)(currentDate - unusedAging.ReceivedAt).TotalMinutes;
                                //        unusedAging.TotalMinutes += totalminutes;
                                //        unusedAging.IsActive = false;
                                //    }

                                //    //for origin
                                //    MsPalletAging aging = await db.MsPalletAgings.Where(x => x.PalletId.Equals(item.TagId) && x.AgingType.Equals(Constant.AgingType.USED.ToString()) && x.CurrentMonth.Equals(month) && x.CurrentYear.Equals(year) && x.WarehouseId.Equals(data.WarehouseId)).FirstOrDefaultAsync();
                                //    //start billing if no row for exact pallet and warehouse
                                //    if (aging != null && aging.IsActive)
                                //    {
                                //        int totalminutes = (int)(currentDate - aging.ReceivedAt).TotalMinutes;
                                //        aging.TotalMinutes += totalminutes;
                                //        aging.ReceivedAt = currentDate;
                                //    }

                                //    //for destination
                                //    MsPalletAging agingDest = await db.MsPalletAgings.Where(x => x.PalletId.Equals(item.TagId) && x.AgingType.Equals(Constant.AgingType.USED.ToString()) && x.CurrentMonth.Equals(month) && x.CurrentYear.Equals(year) && x.WarehouseId.Equals(data.DestinationId)).FirstOrDefaultAsync();
                                //    //start billing if no row for exact pallet and warehouse
                                //    if (agingDest == null)
                                //    {
                                //        agingDest = new MsPalletAging
                                //        {
                                //            AgingId = Utilities.CreateGuid("PAG"),
                                //            PalletId = item.TagId,
                                //            WarehouseId = pallet.WarehouseId,
                                //            ReceivedAt = currentDate,
                                //            CurrentMonth = month,
                                //            CurrentYear = year,
                                //            IsActive = true,
                                //            AgingType = Constant.AgingType.USED.ToString()
                                //        };

                                //        db.MsPalletAgings.Add(agingDest);
                                //    }
                                //    else
                                //    {
                                //        if (agingDest.IsActive)
                                //        {
                                //            //update received date
                                //            agingDest.ReceivedAt = currentDate;
                                //        }
                                       
                                //    }
                                //}
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

        public IEnumerable<TrxShipmentHeader> GetOutboundData(string WarehouseId, string stats, string search, string sortDirection, string sortColName)
        {
            IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);
            IEnumerable<TrxShipmentHeader> list = null;
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
                           m.ShipmentNumber.Contains(search) || m.DestinationName.Contains(search) ||
                           m.TransporterName.Contains(search) || m.DriverName.Contains(search) || m.PlateNumber.Contains(search) ||
                           m.TransactionStatus.Contains(search) || m.ShipmentStatus.Contains(search));

                //columns sorting
                Dictionary<string, Func<TrxShipmentHeader, object>> cols = new Dictionary<string, Func<TrxShipmentHeader, object>>();
                cols.Add("TransactionCode", x => x.TransactionCode);
                cols.Add("ShipmentNumber", x => x.ShipmentNumber);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("DestinationName", x => x.DestinationName);
                cols.Add("TransporterName", x => x.TransporterName);
                cols.Add("DriverName", x => x.DriverName);
                cols.Add("PlateNumber", x => x.PlateNumber);
                cols.Add("TransactionStatus", x => x.TransactionStatus);
                cols.Add("ShipmentStatus", x => x.ShipmentStatus);
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

        public int GetTotalOutboundData(string WarehouseId, string stats)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(WarehouseId) && x.IsDeleted == false);

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

        public IEnumerable<TrxShipmentHeader> GetInboundData(string WarehouseId, string stats, string search, string sortDirection, string sortColName)
        {
            IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable().Where(x => x.DestinationId.Equals(WarehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.OPEN.ToString()) && !x.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString()) && x.IsDeleted == false);
            IEnumerable<TrxShipmentHeader> list = null;
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
                           m.ShipmentNumber.Contains(search) || m.DestinationName.Contains(search) ||
                           m.TransporterName.Contains(search) || m.DriverName.Contains(search) || m.PlateNumber.Contains(search) ||
                           m.TransactionStatus.Contains(search) || m.ShipmentStatus.Contains(search));

                //columns sorting
                Dictionary<string, Func<TrxShipmentHeader, object>> cols = new Dictionary<string, Func<TrxShipmentHeader, object>>();
                cols.Add("TransactionCode", x => x.TransactionCode);
                cols.Add("ShipmentNumber", x => x.ShipmentNumber);
                cols.Add("WarehouseName", x => x.WarehouseName);
                cols.Add("DestinationName", x => x.DestinationName);
                cols.Add("TransporterName", x => x.TransporterName);
                cols.Add("DriverName", x => x.DriverName);
                cols.Add("PlateNumber", x => x.PlateNumber);
                cols.Add("TransactionStatus", x => x.TransactionStatus);
                cols.Add("ShipmentStatus", x => x.ShipmentStatus);
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

        public int GetTotalInboundData(string WarehouseId, string stats)
        {
            int totalData = 0;
            try
            {
                IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable().Where(x => x.DestinationId.Equals(WarehouseId) && !x.TransactionStatus.Equals(Constant.TransactionStatus.OPEN.ToString()) && !x.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString()) && x.IsDeleted == false);

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

        public IEnumerable<TrxShipmentHeader> GetListOutbound(string warehouseId)
        {
            IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                .Where(x => x.WarehouseId.Equals(warehouseId)
                && x.ShipmentStatus.Equals(Constant.ShipmentStatus.LOADING.ToString())
                && x.IsDeleted == false);
            IEnumerable<TrxShipmentHeader> list = null;
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

        public IEnumerable<TrxShipmentHeader> GetListInbound(string warehouseId)
        {
            IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                .Where(x => x.DestinationId.Equals(warehouseId)
                && x.ShipmentStatus.Equals(Constant.ShipmentStatus.DISPATCH.ToString())
                && x.IsDeleted == false);
            IEnumerable<TrxShipmentHeader> list = null;
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

        public async Task<TrxShipmentHeader> GetDataByTransactionCodeAsync(string TransactionCode)
        {
            TrxShipmentHeader data = null;
            try
            {
                data = await db.TrxShipmentHeaders.Where(m => m.TransactionCode.Equals(TransactionCode)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<IEnumerable<TrxShipmentHeader>> GetDataAllInboundTransactionProgress()
        {
            IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.Where(x => x.TransactionStatus.Equals("PROGRESS") 
                        && x.ShipmentStatus.Equals("DISPATCH") && x.IsDeleted == false);
            IEnumerable <TrxShipmentHeader> list = null;
            try
            {
                query = query.Where(header => db.TrxShipmentItems.Any(item => item.TransactionId == header.TransactionId && item.PalletMovementStatus == "ST"));
                query = query.OrderByDescending(header => header.TransactionCode);
                list = await query.ToListAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return list;
        }

        public async Task<bool> CreateAccidentReportAsync(TrxShipmentHeader data, string username)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogShipmentHeader log = new LogShipmentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Create BA",
                        ExecutedBy = username,
                        ExecutedAt = DateTime.Now
                    };

                    db.LogShipmentHeaders.Add(log);

                    //loop update to pallet
                    foreach (TrxShipmentItem item in data.TrxShipmentItems)
                    {
                        if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.IN.ToString()))
                        {
                            item.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                            //check if exist, ignore
                            MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                            if (pallet != null)
                            {
                                if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()))
                                {
                                    pallet.WarehouseId = data.DestinationId;
                                    pallet.WarehouseCode = data.DestinationCode;
                                    pallet.WarehouseName = data.DestinationName;
                                }
                                pallet.PalletMovementStatus = item.PalletMovementStatus;
                                pallet.ReceivedBy = username;
                                pallet.ReceivedAt = DateTime.Now;
                                pallet.LastTransactionName = Constant.TransactionName.SHIPMENT_INBOUND.ToString();
                                pallet.LastTransactionCode = data.TransactionCode;
                                pallet.LastTransactionDate = DateTime.Now;
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

        public IEnumerable<TrxShipmentHeader> GetDeliveryData(string WarehouseId, string DestinationId, string startDate, string endDate, string search, string sortDirection, string sortColName)
        {
            IEnumerable<TrxShipmentHeader> list = Enumerable.Empty<TrxShipmentHeader>();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime EndDate = DateTime.Parse(endDate);

                try
                {
                    IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                    .Where(x => x.WarehouseId.Equals(WarehouseId)
                    && x.IsDeleted == false
                    && x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));

                    if (!string.IsNullOrEmpty(DestinationId))
                    {
                        query = query.Where(x => x.DestinationId.Equals(DestinationId));
                    }

                    query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.CreatedAt) <= DbFunctions.TruncateTime(EndDate));
                    query = query
                            .Where(m => m.TransactionCode.Contains(search) ||
                               m.ShipmentNumber.Contains(search) || m.DestinationName.Contains(search) ||
                               m.TransporterName.Contains(search) || m.DriverName.Contains(search) || m.PlateNumber.Contains(search));

                    //columns sorting
                    Dictionary<string, Func<TrxShipmentHeader, object>> cols = new Dictionary<string, Func<TrxShipmentHeader, object>>();
                    cols.Add("TransactionCode", x => x.TransactionCode);
                    cols.Add("CreatedAt", x => x.CreatedAt);
                    cols.Add("ShipmentNumber", x => x.ShipmentNumber);
                    cols.Add("WarehouseName", x => x.WarehouseName);
                    cols.Add("DestinationName", x => x.DestinationName);
                    cols.Add("TransporterName", x => x.TransporterName);
                    cols.Add("DriverName", x => x.DriverName);
                    cols.Add("PlateNumber", x => x.PlateNumber);
                    cols.Add("PalletQty", x => x.PalletQty);


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

        public IEnumerable<TrxShipmentHeader> GetDeliveryData(string WarehouseId, string DestinationId, string startDate, string endDate)
        {
            IEnumerable<TrxShipmentHeader> list = Enumerable.Empty<TrxShipmentHeader>();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime EndDate = DateTime.Parse(endDate);

                try
                {
                    IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                    .Where(x => x.WarehouseId.Equals(WarehouseId)
                    && x.IsDeleted == false
                    && x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));

                    if (!string.IsNullOrEmpty(DestinationId))
                    {
                        query = query.Where(x => x.DestinationId.Equals(DestinationId));
                    }

                    query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.CreatedAt) <= DbFunctions.TruncateTime(EndDate));
                    list = query.ToList();
                }
                catch (Exception e)
                {
                    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                    logger.Error(e, errMsg);
                }

            }

            return list;
        }

        public int GetTotalDelivery(string WarehouseId, string DestinationId, string startDate, string endDate)
        {
            int totalData = 0;
            try
            {
                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    DateTime StartDate = DateTime.Parse(startDate);
                    DateTime EndDate = DateTime.Parse(endDate);

                    IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                    .Where(x => x.WarehouseId.Equals(WarehouseId)
                    && x.IsDeleted == false
                    && x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));

                    if (!string.IsNullOrEmpty(DestinationId))
                    {
                        query = query.Where(x => x.DestinationId.Equals(DestinationId));
                    }

                    query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.CreatedAt) <= DbFunctions.TruncateTime(EndDate));

                    totalData = query.Count();
                }
               
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<TrxShipmentHeader> GetIncomingData(string WarehouseId, string OriginId, string startDate, string endDate, string search, string sortDirection, string sortColName)
        {
            IEnumerable<TrxShipmentHeader> list = Enumerable.Empty<TrxShipmentHeader>();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime EndDate = DateTime.Parse(endDate);

                try
                {
                    IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                    .Where(x => x.DestinationId.Equals(WarehouseId)
                    && x.IsDeleted == false
                    && x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));

                    if (!string.IsNullOrEmpty(OriginId))
                    {
                        query = query.Where(x => x.WarehouseId.Equals(OriginId));
                    }

                    query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.CreatedAt) <= DbFunctions.TruncateTime(EndDate));
                    query = query
                            .Where(m => m.TransactionCode.Contains(search) ||
                               m.ShipmentNumber.Contains(search) || m.DestinationName.Contains(search) ||
                               m.TransporterName.Contains(search) || m.DriverName.Contains(search) || m.PlateNumber.Contains(search));

                    //columns sorting
                    Dictionary<string, Func<TrxShipmentHeader, object>> cols = new Dictionary<string, Func<TrxShipmentHeader, object>>();
                    cols.Add("TransactionCode", x => x.TransactionCode);
                    cols.Add("CreatedAt", x => x.CreatedAt);
                    cols.Add("ShipmentNumber", x => x.ShipmentNumber);
                    cols.Add("WarehouseName", x => x.WarehouseName);
                    cols.Add("DestinationName", x => x.DestinationName);
                    cols.Add("TransporterName", x => x.TransporterName);
                    cols.Add("DriverName", x => x.DriverName);
                    cols.Add("PlateNumber", x => x.PlateNumber);
                    cols.Add("PalletQty", x => x.PalletQty);


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

        public IEnumerable<TrxShipmentHeader> GetIncomingData(string WarehouseId, string OriginId, string startDate, string endDate)
        {
            IEnumerable<TrxShipmentHeader> list = Enumerable.Empty<TrxShipmentHeader>();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime StartDate = DateTime.Parse(startDate);
                DateTime EndDate = DateTime.Parse(endDate);

                try
                {
                    IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                    .Where(x => x.DestinationId.Equals(WarehouseId)
                    && x.IsDeleted == false
                    && x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));

                    if (!string.IsNullOrEmpty(OriginId))
                    {
                        query = query.Where(x => x.WarehouseId.Equals(OriginId));
                    }

                    query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.CreatedAt) <= DbFunctions.TruncateTime(EndDate));
                    list = query.ToList();
                }
                catch (Exception e)
                {
                    string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                    logger.Error(e, errMsg);
                }

            }

            return list.ToList();
        }

        public int GetTotalIncoming(string WarehouseId, string OriginId, string startDate, string endDate)
        {
            int totalData = 0;
            try
            {
                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    DateTime StartDate = DateTime.Parse(startDate);
                    DateTime EndDate = DateTime.Parse(endDate);

                    IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable()
                    .Where(x => x.DestinationId.Equals(WarehouseId)
                    && x.IsDeleted == false
                    && x.TransactionStatus.Equals(Constant.TransactionStatus.CLOSED.ToString()));

                    if (!string.IsNullOrEmpty(OriginId))
                    {
                        query = query.Where(x => x.WarehouseId.Equals(OriginId));
                    }

                    query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.CreatedAt) <= DbFunctions.TruncateTime(EndDate));

                    totalData = query.Count();
                }

            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return totalData;
        }

        public IEnumerable<TrxShipmentHeader> GetAllOutboundTransactions(string warehouseId)
        {
            IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable().Where(x => x.WarehouseId.Equals(warehouseId) && x.IsDeleted == false);
            IEnumerable<TrxShipmentHeader> list = null;
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

        public IEnumerable<TrxShipmentHeader> GetAllInboundTransactions(string warehouseId)
        {
            IQueryable<TrxShipmentHeader> query = db.TrxShipmentHeaders.AsQueryable().Where(x => x.DestinationId.Equals(warehouseId) && x.IsDeleted == false);
            IEnumerable<TrxShipmentHeader> list = null;
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

        public async Task<bool> DispatchManualAsync(TrxShipmentHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {

                    MsWarehouse warehouse = await db.MsWarehouses.FindAsync(data.WarehouseId);
                    MsWarehouse destination = await db.MsWarehouses.FindAsync(data.DestinationId);


                    string prefix = data.TransactionId.Substring(0, 3);
                    string warehouseCode = data.WarehouseCode;
                    string warehouseAlias = warehouse.WarehouseAlias;
                    int year = Convert.ToInt32(data.CreatedAt.Year.ToString().Substring(2));
                    int month = data.CreatedAt.Month;
                    string romanMonth = Utilities.ConvertMonthToRoman(month);

                    // get last number, and do increment.
                    string lastNumber = db.TrxShipmentHeaders.AsQueryable().OrderByDescending(x => x.CreatedAt)
                        .Where(x => x.WarehouseCode.Equals(warehouseCode) && x.CreatedAt.Year.Equals(data.CreatedAt.Year) && x.CreatedAt.Month.Equals(data.CreatedAt.Month))
                        .AsEnumerable().Select(x => x.TransactionCode).FirstOrDefault();
                    int currentNumber = 0;

                    if (!string.IsNullOrEmpty(lastNumber))
                    {
                        currentNumber = Int32.Parse(lastNumber.Substring(lastNumber.Length - 3));
                    }

                    string runningNumber = string.Format("{0:D3}", currentNumber + 1);

                    //data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}/{5}", prefix, palletOwner, warehouseAlias, year, romanMonth, runningNumber);

                    data.TransactionCode = string.Format("{0}-{1}/{2}/{3}/{4}/{5}", prefix, warehouseAlias, destination.WarehouseAlias, year, romanMonth, runningNumber);

                    LogShipmentHeader log = new LogShipmentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Create Manual",
                        ExecutedBy = data.CreatedBy,
                        ExecutedAt = data.CreatedAt
                    };

                    LogShipmentDocument logDoc = new LogShipmentDocument
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ShipmentNumber = data.ShipmentNumber,
                        Remarks = data.Remarks,
                        WarehouseId = data.WarehouseId,
                        WarehouseCode = data.WarehouseCode,
                        WarehouseName = data.WarehouseName,
                        DestinationId = data.DestinationId,
                        DestinationCode = data.DestinationCode,
                        DestinationName = data.DestinationName,
                        TransporterId = data.TransporterId,
                        TransporterName = data.TransporterName,
                        DriverId = data.DriverId,
                        DriverName = data.DriverName,
                        TruckId = data.TruckId,
                        PlateNumber = data.PlateNumber,
                        PalletQty = data.PalletQty,
                        TransactionStatus = data.TransactionStatus,
                        ShipmentStatus = data.ShipmentStatus,
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

                    db.TrxShipmentHeaders.Add(data);
                    db.LogShipmentHeaders.Add(log);
                    db.LogShipmentDocuments.Add(logDoc);

                    foreach (TrxShipmentItem item in data.TrxShipmentItems)
                    {
                        //check if exist, ignore
                        MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                        if (pallet != null)
                        {
                            pallet.PalletMovementStatus = item.PalletMovementStatus;
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

        public async Task<bool> ReceiveManualAsync(TrxShipmentHeader data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    LogShipmentHeader log = new LogShipmentHeader
                    {
                        LogId = Utilities.CreateGuid("LOG"),
                        TransactionId = data.TransactionId,
                        ActionName = "Receive Manual",
                        ExecutedBy = data.CreatedBy,
                        ExecutedAt = DateTime.Now
                    };


                    db.LogShipmentHeaders.Add(log);

                    DateTime currentDate = DateTime.Now;

                    int year = currentDate.Year;
                    int month = currentDate.Month;

                    //loop update to pallet
                    foreach (TrxShipmentItem item in data.TrxShipmentItems)
                    {
                        if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.IN.ToString()))
                        {
                            if (data.ShipmentStatus.Equals(Constant.ShipmentStatus.RECEIVE.ToString()))
                            {
                                item.PalletMovementStatus = Constant.PalletMovementStatus.ST.ToString();
                            }
                            //check if exist, ignore
                            MsPallet pallet = await db.MsPallets.FindAsync(item.TagId);
                            if (pallet != null)
                            {
                                if (item.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()))
                                {
                                    pallet.WarehouseId = data.DestinationId;
                                    pallet.WarehouseCode = data.DestinationCode;
                                    pallet.WarehouseName = data.DestinationName;
                                }
                                pallet.PalletMovementStatus = item.PalletMovementStatus;
                                pallet.ReceivedBy = data.CreatedBy;
                                pallet.ReceivedAt = currentDate;
                                pallet.LastTransactionName = Constant.TransactionName.SHIPMENT_INBOUND.ToString();
                                pallet.LastTransactionCode = data.TransactionCode;
                                pallet.LastTransactionDate = currentDate;

                                //generate aging row if already settled
                                //if (pallet.PalletMovementStatus.Equals(Constant.PalletMovementStatus.ST.ToString()))
                                //{
                                //    //update unused
                                //    MsPalletAging unusedAging = await db.MsPalletAgings.Where(x => x.PalletId.Equals(item.TagId) && x.AgingType.Equals(Constant.AgingType.UNUSED.ToString()) && x.CurrentMonth.Equals(month) && x.CurrentYear.Equals(year) && x.WarehouseId.Equals(data.WarehouseId)).FirstOrDefaultAsync();
                                //    //start billing if no row for exact pallet and warehouse
                                //    if (unusedAging != null && unusedAging.IsActive)
                                //    {
                                //        int totalminutes = (int)(currentDate - unusedAging.ReceivedAt).TotalMinutes;
                                //        unusedAging.TotalMinutes += totalminutes;
                                //        unusedAging.IsActive = false;
                                //    }

                                //    //for origin
                                //    MsPalletAging aging = await db.MsPalletAgings.Where(x => x.PalletId.Equals(item.TagId) && x.AgingType.Equals(Constant.AgingType.USED.ToString()) && x.CurrentMonth.Equals(month) && x.CurrentYear.Equals(year) && x.WarehouseId.Equals(data.WarehouseId)).FirstOrDefaultAsync();
                                //    //start billing if no row for exact pallet and warehouse
                                //    if (aging != null && aging.IsActive)
                                //    {
                                //        int totalminutes = (int)(currentDate - aging.ReceivedAt).TotalMinutes;
                                //        aging.TotalMinutes += totalminutes;
                                //        aging.ReceivedAt = currentDate;
                                //    }

                                //    //for destination
                                //    MsPalletAging agingDest = await db.MsPalletAgings.Where(x => x.PalletId.Equals(item.TagId) && x.AgingType.Equals(Constant.AgingType.USED.ToString()) && x.CurrentMonth.Equals(month) && x.CurrentYear.Equals(year) && x.WarehouseId.Equals(data.DestinationId)).FirstOrDefaultAsync();
                                //    //start billing if no row for exact pallet and warehouse
                                //    if (agingDest == null)
                                //    {
                                //        agingDest = new MsPalletAging
                                //        {
                                //            AgingId = Utilities.CreateGuid("PAG"),
                                //            PalletId = item.TagId,
                                //            WarehouseId = pallet.WarehouseId,
                                //            ReceivedAt = currentDate,
                                //            CurrentMonth = month,
                                //            CurrentYear = year,
                                //            IsActive = true,
                                //            AgingType = Constant.AgingType.USED.ToString()
                                //        };

                                //        db.MsPalletAgings.Add(agingDest);
                                //    }
                                //    else
                                //    {
                                //        if (agingDest.IsActive)
                                //        {
                                //            //update received date
                                //            agingDest.ReceivedAt = currentDate;
                                //        }

                                //    }
                                //}
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

        public async Task<bool> UpdateWarningCountAsync(TrxShipmentHeader data)
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

        public async Task<TrxShipmentItemTemp> GetDataByTransactionTagIdTempAsync(string TransactionId, string TagId, string StatusShipment)
        {
            TrxShipmentItemTemp data = null;
            try
            {
                data = await db.TrxShipmentItemTemps.Where(x => x.TransactionId.Equals(TransactionId) && x.TagId.Equals(TagId) && x.StatusShipment.Equals(StatusShipment)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }

        public async Task<TrxShipmentItemTemp> GetDataByTransactionIdTempAsync(string TransactionId)
        {
            TrxShipmentItemTemp data = null;
            try
            {
                data = await db.TrxShipmentItemTemps.Where(x => x.TransactionId.Equals(TransactionId)).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                string errMsg = (e.InnerException != null) ? e.InnerException.InnerException.Message : e.Message;
                logger.Error(e, errMsg);
            }
            return data;
        }


        public async Task<bool> InsertItemTempAsync(TrxShipmentItemTemp data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    TrxShipmentItemTemp add = new TrxShipmentItemTemp
                    {
                        TempID = data.TempID,
                        TransactionId = data.TransactionId,
                        TagId = data.TagId,
                        ScannedBy = data.ScannedBy,
                        ScannedAt = data.ScannedAt,
                        StatusShipment = data.StatusShipment
                    };

                    db.TrxShipmentItemTemps.Add(add);

                    //check if exist, ignore
                    MsPallet pallet = await db.MsPallets.FindAsync(data.TagId);
                    if (pallet == null)
                    {
                        LogScanTagIdNotRegister add2 = new LogScanTagIdNotRegister
                        {
                            ID = Utilities.CreateGuid("LOG"),
                            TransactionId = data.TransactionId,
                            TagId = data.TagId,
                            ScannedBy = data.ScannedBy,
                            ScannedAt = data.ScannedAt,
                            StatusShipment = data.StatusShipment
                        };

                        db.LogScanTagIdNotRegister.Add(add2);

                        await db.SaveChangesAsync();
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

        public async Task<bool> DeleteItemTempAsync(TrxShipmentItemTemp data)
        {
            bool status = false;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {                   
                    //delete all items
                    db.TrxShipmentItemTemps.RemoveRange(db.TrxShipmentItemTemps.Where(m => m.TransactionId.Equals(data.TransactionId)));

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